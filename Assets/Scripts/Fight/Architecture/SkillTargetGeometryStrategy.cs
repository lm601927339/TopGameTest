using UnityEngine;

public enum SkillTargetGeometryKind
{
    Unknown,
    LockOnTarget,
    GroundPoint,
    Cone,
    Line
}

public enum SkillTargetGeometrySource
{
    Unknown,
    TargetMode,
    SelectionFallback
}

public readonly struct SkillTargetGeometryDecision
{
    public SkillTargetGeometryKind geometryKind { get; }
    public SkillTargetGeometrySource geometrySource { get; }
    public SkillTargetMode targetMode { get; }
    public Vector3 geometryOrigin { get; }
    public Vector3 geometryDirection { get; }
    public Vector3 geometryAnchorPoint { get; }
    public ChaState geometryTargetUnit { get; }
    public string geometrySummary { get; }
    public string geometryFallbackPath { get; }

    public bool hasTargetUnit => geometryTargetUnit != null;
    public bool hasAnchorPoint => geometryAnchorPoint.sqrMagnitude > 0f;

    public SkillTargetGeometryDecision(
        SkillTargetGeometryKind geometryKind,
        SkillTargetGeometrySource geometrySource,
        SkillTargetMode targetMode,
        Vector3 geometryOrigin,
        Vector3 geometryDirection,
        Vector3 geometryAnchorPoint,
        ChaState geometryTargetUnit,
        string geometrySummary,
        string geometryFallbackPath
    )
    {
        this.geometryKind = geometryKind;
        this.geometrySource = geometrySource;
        this.targetMode = targetMode;
        this.geometryOrigin = geometryOrigin;
        this.geometryDirection = geometryDirection;
        this.geometryAnchorPoint = geometryAnchorPoint;
        this.geometryTargetUnit = geometryTargetUnit;
        this.geometrySummary = geometrySummary ?? string.Empty;
        this.geometryFallbackPath = geometryFallbackPath ?? string.Empty;
    }

    public static SkillTargetGeometryDecision Create(
        ChaState caster,
        SkillTargetMode targetMode,
        SkillCastRequest request
    )
    {
        SkillTargetGeometryKind geometryKind = ResolveGeometryKind(targetMode);
        Vector3 origin = ResolveOrigin(caster, geometryKind, request);
        Vector3 resolvedDirection = ResolveDirection(caster, request);
        Vector3 anchorPoint = ResolveAnchorPoint(origin, request);
        string summary = BuildSummary(geometryKind, request, origin, resolvedDirection, anchorPoint);
        string fallbackPath = BuildFallbackPath(geometryKind);
        return new SkillTargetGeometryDecision(
            geometryKind,
            SkillTargetGeometrySource.TargetMode,
            targetMode,
            origin,
            resolvedDirection,
            anchorPoint,
            null,
            summary,
            fallbackPath
        );
    }

    private static SkillTargetGeometryKind ResolveGeometryKind(SkillTargetMode targetMode)
    {
        switch (targetMode)
        {
            case SkillTargetMode.LockOnTarget:
                return SkillTargetGeometryKind.LockOnTarget;
            case SkillTargetMode.GroundPoint:
                return SkillTargetGeometryKind.GroundPoint;
            case SkillTargetMode.Cone:
                return SkillTargetGeometryKind.Cone;
            case SkillTargetMode.Line:
                return SkillTargetGeometryKind.Line;
            default:
                return SkillTargetGeometryKind.Unknown;
        }
    }

    private static Vector3 ResolveOrigin(ChaState caster, SkillTargetGeometryKind geometryKind, SkillCastRequest request)
    {
        if ((geometryKind == SkillTargetGeometryKind.GroundPoint || geometryKind == SkillTargetGeometryKind.LockOnTarget) && request.hasTargetPosition)
        {
            return request.targetPosition;
        }

        if (caster != null)
        {
            return caster.transform.position;
        }

        return Vector3.zero;
    }

    private static Vector3 ResolveDirection(ChaState caster, SkillCastRequest request)
    {
        if (request.hasAimDirection && request.aimDirection.sqrMagnitude > 0f)
        {
            return request.aimDirection.normalized;
        }

        if (request.hasTargetPosition && caster != null && caster.transform != null)
        {
            Vector3 toPoint = request.targetPosition - caster.transform.position;
            if (toPoint.sqrMagnitude > 0f)
            {
                return toPoint.normalized;
            }
        }

        if (caster != null && caster.transform != null)
        {
            Vector3 facing = caster.transform.up;
            if (facing.sqrMagnitude > 0f)
            {
                return facing.normalized;
            }
        }

        return Vector3.up;
    }

    private static Vector3 ResolveAnchorPoint(Vector3 origin, SkillCastRequest request)
    {
        if (request.hasTargetPosition)
        {
            return request.targetPosition;
        }

        return origin;
    }

    private static string BuildSummary(
        SkillTargetGeometryKind geometryKind,
        SkillCastRequest request,
        Vector3 origin,
        Vector3 resolvedDirection,
        Vector3 anchorPoint
    )
    {
        string pointText = request.hasTargetPosition ? request.targetPosition.ToString("F3") : "none";
        string directionText = request.hasAimDirection ? request.aimDirection.ToString("F3") : "none";
        return "geometry=" + geometryKind + ", hasTarget=" + request.hasTarget + ", point=" + pointText + ", aim=" + directionText + ", origin=" + origin.ToString("F3") + ", anchor=" + anchorPoint.ToString("F3") + ", direction=" + resolvedDirection.ToString("F3");
    }

    private static string BuildFallbackPath(SkillTargetGeometryKind geometryKind)
    {
        switch (geometryKind)
        {
            case SkillTargetGeometryKind.LockOnTarget:
                return "target unit -> target point -> caster position";
            case SkillTargetGeometryKind.GroundPoint:
                return "ground point -> target unit -> caster position";
            case SkillTargetGeometryKind.Cone:
                return "caster origin -> resolved direction";
            case SkillTargetGeometryKind.Line:
                return "caster origin -> resolved direction";
            default:
                return "mode driven fallback";
        }
    }
}

public static class SkillTargetGeometryStrategy
{
    public static SkillTargetGeometryDecision Create(ChaState caster, SkillTargetMode targetMode, SkillCastRequest request)
    {
        return SkillTargetGeometryDecision.Create(caster, targetMode, request);
    }
}
