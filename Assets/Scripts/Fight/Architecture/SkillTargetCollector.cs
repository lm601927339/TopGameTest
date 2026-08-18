using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;

public enum SkillTargetQueryReason
{
    DirectLock,
    GroundZone,
    ConeHit,
    LineHit,
    FallbackNearest,
    OutsideShape,
    NoSelection,
    InvalidCandidate
}

public readonly struct SkillTargetQueryTrace
{
    public ChaState unit { get; }
    public bool included { get; }
    public SkillTargetQueryReason reason { get; }
    public float distance { get; }
    public float score { get; }
    public Vector3 position { get; }
    public string detail { get; }

    public SkillTargetQueryTrace(
        ChaState unit,
        bool included,
        SkillTargetQueryReason reason,
        float distance,
        float score,
        Vector3 position,
        string detail
    )
    {
        this.unit = unit;
        this.included = included;
        this.reason = reason;
        this.distance = distance;
        this.score = score;
        this.position = position;
        this.detail = detail ?? string.Empty;
    }
}

public sealed class SkillTargetQueryResult
{
    private readonly IReadOnlyList<SkillTargetQueryTrace> _traces;
    private readonly IReadOnlyList<ChaState> _hits;
    private readonly IReadOnlyList<ChaState> _excluded;

    public SkillTargetMode targetMode { get; }
    public SkillTargetGeometryKind geometryKind { get; }
    public Vector3 origin { get; }
    public Vector3 direction { get; }
    public Vector3 anchorPoint { get; }
    public string summary { get; }
    public string fallbackPath { get; }
    public IReadOnlyList<SkillTargetQueryTrace> traces => _traces;
    public IReadOnlyList<ChaState> hits => _hits;
    public IReadOnlyList<ChaState> excluded => _excluded;

    public SkillTargetQueryResult(
        SkillTargetMode targetMode,
        SkillTargetGeometryKind geometryKind,
        Vector3 origin,
        Vector3 direction,
        Vector3 anchorPoint,
        IReadOnlyList<SkillTargetQueryTrace> traces,
        string summary,
        string fallbackPath
    )
    {
        this.targetMode = targetMode;
        this.geometryKind = geometryKind;
        this.origin = origin;
        this.direction = direction;
        this.anchorPoint = anchorPoint;
        this._traces = traces ?? Array.Empty<SkillTargetQueryTrace>();
        this._hits = this._traces.Where(trace => trace.included && trace.unit != null).Select(trace => trace.unit).ToArray();
        this._excluded = this._traces.Where(trace => trace.included == false && trace.unit != null).Select(trace => trace.unit).ToArray();
        this.summary = summary ?? string.Empty;
        this.fallbackPath = fallbackPath ?? string.Empty;
    }

    public string Describe()
    {
        return "mode=" + targetMode + ", geometry=" + geometryKind + ", hits=" + _hits.Count + ", excluded=" + _excluded.Count + ", origin=" + origin.ToString("F3") + ", direction=" + direction.ToString("F3");
    }

    public IReadOnlyList<string> GetDetailLines()
    {
        List<string> lines = new List<string>();
        lines.Add("summary=" + summary);
        lines.Add("fallbackPath=" + fallbackPath);
        lines.Add("origin=" + origin.ToString("F3"));
        lines.Add("direction=" + direction.ToString("F3"));
        lines.Add("anchorPoint=" + anchorPoint.ToString("F3"));

        foreach (SkillTargetQueryTrace trace in _traces)
        {
            string unitName = trace.unit != null ? trace.unit.name : "null";
            string status = trace.included ? "hit" : "excluded";
            lines.Add(status + ":" + unitName + " reason=" + trace.reason + " distance=" + trace.distance.ToString("F3", CultureInfo.InvariantCulture) + " score=" + trace.score.ToString("F3", CultureInfo.InvariantCulture) + " detail=" + trace.detail);
        }

        return lines;
    }
}

public static class SkillTargetQuery
{
    public static SkillTargetQueryResult Collect(SkillCastPreparationSnapshot snapshot, IEnumerable<ChaState> candidates)
    {
        if (EqualityComparer<SkillCastPreparationSnapshot>.Default.Equals(snapshot, default(SkillCastPreparationSnapshot)))
        {
            return new SkillTargetQueryResult(SkillTargetMode.Auto, SkillTargetGeometryKind.Unknown, Vector3.zero, Vector3.up, Vector3.zero, Array.Empty<SkillTargetQueryTrace>(), "snapshot=null", "snapshot unavailable");
        }

        SkillTargetGeometryDecision geometryDecision = snapshot.targetGeometryDecision;
        SkillTargetMode targetMode = GetValue(snapshot, "targetMode", geometryDecision.targetMode);
        SkillTargetSelectionDecision selectionDecision = GetNestedValue(snapshot, "targetSelectionDecision", default(SkillTargetSelectionDecision));
        ChaState selectedTarget = GetNestedValue<ChaState>(selectionDecision, "targetUnit", null);
        Vector3 selectedPoint = GetNestedValue(selectionDecision, "targetPosition", geometryDecision.geometryAnchorPoint);

        float lockRadius = ResolveFloat(snapshot, 0.75f, "lockRadius", "targetRadius", "queryRadius", "radius");
        float groundRadius = ResolveFloat(snapshot, 1.50f, "groundRadius", "radius", "aoeRadius", "queryRadius");
        float coneRange = ResolveFloat(snapshot, 4.00f, "coneRange", "range", "radius", "queryRadius");
        float coneAngle = ResolveFloat(snapshot, 60.0f, "coneAngle", "angle", "spreadAngle");
        float lineLength = ResolveFloat(snapshot, 5.00f, "lineLength", "length", "range", "radius");
        float lineWidth = ResolveFloat(snapshot, 1.00f, "lineWidth", "width", "radius", "thickness");

        IReadOnlyList<SkillTargetQueryTrace> traces = BuildTraces(
            geometryDecision,
            targetMode,
            selectedTarget,
            selectedPoint,
            candidates ?? Array.Empty<ChaState>(),
            lockRadius,
            groundRadius,
            coneRange,
            coneAngle,
            lineLength,
            lineWidth
        );

        return new SkillTargetQueryResult(
            targetMode,
            geometryDecision.geometryKind,
            geometryDecision.geometryOrigin,
            geometryDecision.geometryDirection,
            geometryDecision.geometryAnchorPoint,
            traces,
            GetValue(snapshot, "targetGeometrySummary", geometryDecision.geometrySummary),
            GetValue(snapshot, "targetGeometryFallbackPath", geometryDecision.geometryFallbackPath)
        );
    }

    public static SkillTargetCollectionResult CollectDetailed(SkillCastPreparationSnapshot snapshot, IEnumerable<ChaState> candidates)
    {
        SkillTargetQueryResult queryResult = Collect(snapshot, candidates);
        SkillTargetFilterResult filterResult = SkillTargetFilterStrategy.Apply(snapshot, queryResult);
        return new SkillTargetCollectionResult(queryResult, filterResult);
    }

    private static IReadOnlyList<SkillTargetQueryTrace> BuildTraces(
        SkillTargetGeometryDecision geometryDecision,
        SkillTargetMode targetMode,
        ChaState selectedTarget,
        Vector3 selectedPoint,
        IEnumerable<ChaState> candidates,
        float lockRadius,
        float groundRadius,
        float coneRange,
        float coneAngle,
        float lineLength,
        float lineWidth
    )
    {
        List<SkillTargetQueryTrace> traces = new List<SkillTargetQueryTrace>();
        SkillTargetGeometryKind geometryKind = geometryDecision.geometryKind;
        Vector3 origin = geometryDecision.geometryOrigin;
        Vector3 direction = geometryDecision.geometryDirection.sqrMagnitude > 0f ? geometryDecision.geometryDirection.normalized : Vector3.up;

        ChaState nearestCandidate = null;
        float nearestDistance = float.MaxValue;

        foreach (ChaState candidate in candidates)
        {
            if (candidate == null)
            {
                traces.Add(new SkillTargetQueryTrace(null, false, SkillTargetQueryReason.InvalidCandidate, 0f, 0f, Vector3.zero, "candidate=null"));
                continue;
            }

            Vector3 position = candidate.transform.position;
            float distanceToOrigin = Vector3.Distance(position, origin);
            bool included = false;
            SkillTargetQueryReason reason = SkillTargetQueryReason.OutsideShape;
            string detail = string.Empty;
            float score = 0f;

            if (geometryKind == SkillTargetGeometryKind.LockOnTarget)
            {
                if (selectedTarget != null)
                {
                    included = ReferenceEquals(candidate, selectedTarget);
                    reason = included ? SkillTargetQueryReason.DirectLock : SkillTargetQueryReason.NoSelection;
                    detail = included ? "selected target unit" : "not selected target";
                    score = included ? 1f : 0f;
                }
                else
                {
                    if (distanceToOrigin < nearestDistance)
                    {
                        nearestDistance = distanceToOrigin;
                        nearestCandidate = candidate;
                    }
                    included = distanceToOrigin <= lockRadius;
                    reason = included ? SkillTargetQueryReason.FallbackNearest : SkillTargetQueryReason.OutsideShape;
                    detail = included ? "within lock radius" : "outside lock radius";
                    score = included ? Mathf.Max(0.01f, 1f - distanceToOrigin / Mathf.Max(lockRadius, 0.0001f)) : 0f;
                }
            }
            else if (geometryKind == SkillTargetGeometryKind.GroundPoint)
            {
                included = Vector3.Distance(position, geometryDecision.geometryAnchorPoint) <= groundRadius;
                reason = included ? SkillTargetQueryReason.GroundZone : SkillTargetQueryReason.OutsideShape;
                detail = included ? "inside ground radius" : "outside ground radius";
                score = included ? Mathf.Max(0.01f, 1f - Vector3.Distance(position, geometryDecision.geometryAnchorPoint) / Mathf.Max(groundRadius, 0.0001f)) : 0f;
            }
            else if (geometryKind == SkillTargetGeometryKind.Cone)
            {
                Vector3 toCandidate = position - origin;
                float distance = toCandidate.magnitude;
                float angle = Vector3.Angle(direction, toCandidate);
                included = distance <= coneRange && angle <= coneAngle * 0.5f;
                reason = included ? SkillTargetQueryReason.ConeHit : SkillTargetQueryReason.OutsideShape;
                detail = included ? "inside cone range/angle" : "outside cone range/angle";
                score = included ? Mathf.Max(0.01f, 1f - distance / Mathf.Max(coneRange, 0.0001f)) : 0f;
            }
            else if (geometryKind == SkillTargetGeometryKind.Line)
            {
                Vector3 toCandidate = position - origin;
                float forwardDistance = Vector3.Dot(toCandidate, direction);
                Vector3 projected = origin + direction * forwardDistance;
                float lateralDistance = Vector3.Distance(position, projected);
                included = forwardDistance >= 0f && forwardDistance <= lineLength && lateralDistance <= lineWidth * 0.5f;
                reason = included ? SkillTargetQueryReason.LineHit : SkillTargetQueryReason.OutsideShape;
                detail = included ? "inside line bounds" : "outside line bounds";
                score = included ? Mathf.Max(0.01f, 1f - lateralDistance / Mathf.Max(lineWidth, 0.0001f)) : 0f;
            }
            else
            {
                included = false;
                reason = SkillTargetQueryReason.NoSelection;
                detail = "unknown geometry";
            }

            traces.Add(new SkillTargetQueryTrace(candidate, included, reason, distanceToOrigin, score, position, detail));
        }

        if (geometryKind == SkillTargetGeometryKind.LockOnTarget && selectedTarget == null && nearestCandidate != null)
        {
            for (int i = 0; i < traces.Count; i++)
            {
                SkillTargetQueryTrace trace = traces[i];
                if (trace.unit == nearestCandidate)
                {
                    traces[i] = new SkillTargetQueryTrace(trace.unit, true, SkillTargetQueryReason.FallbackNearest, trace.distance, 1f, trace.position, "nearest candidate fallback");
                }
            }
        }

        return traces;
    }

    private static T GetValue<T>(object owner, string propertyName, T fallback)
    {
        object value = GetMemberValue(owner, propertyName);
        if (value is T typed)
        {
            return typed;
        }

        return fallback;
    }

    private static T GetNestedValue<T>(object owner, string propertyName, T fallback)
    {
        object value = GetMemberValue(owner, propertyName);
        if (value is T typed)
        {
            return typed;
        }

        return fallback;
    }

    private static float ResolveFloat(object owner, float fallback, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            object value = GetMemberValue(owner, propertyName);
            if (value is float floatValue)
            {
                return floatValue;
            }

            if (value is int intValue)
            {
                return intValue;
            }

            if (value is double doubleValue)
            {
                return (float)doubleValue;
            }
        }

        return fallback;
    }

    private static object GetMemberValue(object owner, string memberName)
    {
        if (owner == null || string.IsNullOrEmpty(memberName))
        {
            return null;
        }

        Type ownerType = owner.GetType();
        PropertyInfo property = ownerType.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property != null)
        {
            return property.GetValue(owner, null);
        }

        FieldInfo field = ownerType.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null)
        {
            return field.GetValue(owner);
        }

        return null;
    }
}
