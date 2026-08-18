using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

public enum SkillTargetRelationKind
{
    Unknown,
    Self,
    Ally,
    Enemy
}

public enum SkillTargetFilterReason
{
    Allowed,
    GeometryExcluded,
    SelfExcluded,
    AllyExcluded,
    EnemyExcluded,
    DeadExcluded,
    InvincibleExcluded,
    InvisibleExcluded,
    UnknownRelationAllowed,
    InvalidCandidate,
    NoSnapshot,
    NoQueryResult
}

public readonly struct SkillTargetFilterPolicy
{
    public bool allowSelf { get; }
    public bool allowAlly { get; }
    public bool allowEnemy { get; }
    public bool allowDead { get; }
    public bool allowInvincible { get; }
    public bool allowInvisible { get; }
    public string source { get; }
    public string summary { get; }

    public SkillTargetFilterPolicy(
        bool allowSelf,
        bool allowAlly,
        bool allowEnemy,
        bool allowDead,
        bool allowInvincible,
        bool allowInvisible,
        string source,
        string summary
    )
    {
        this.allowSelf = allowSelf;
        this.allowAlly = allowAlly;
        this.allowEnemy = allowEnemy;
        this.allowDead = allowDead;
        this.allowInvincible = allowInvincible;
        this.allowInvisible = allowInvisible;
        this.source = source ?? string.Empty;
        this.summary = summary ?? string.Empty;
    }

    public static SkillTargetFilterPolicy Default => new SkillTargetFilterPolicy(true, true, true, false, false, false, "default", "self/ally/enemy allowed, dead/invincible/invisible filtered");
}

public readonly struct SkillTargetFilterTrace
{
    public ChaState unit { get; }
    public bool included { get; }
    public SkillTargetRelationKind relation { get; }
    public bool isDead { get; }
    public bool isInvincible { get; }
    public bool isInvisible { get; }
    public SkillTargetFilterReason reason { get; }
    public float distance { get; }
    public string detail { get; }

    public SkillTargetFilterTrace(
        ChaState unit,
        bool included,
        SkillTargetRelationKind relation,
        bool isDead,
        bool isInvincible,
        bool isInvisible,
        SkillTargetFilterReason reason,
        float distance,
        string detail
    )
    {
        this.unit = unit;
        this.included = included;
        this.relation = relation;
        this.isDead = isDead;
        this.isInvincible = isInvincible;
        this.isInvisible = isInvisible;
        this.reason = reason;
        this.distance = distance;
        this.detail = detail ?? string.Empty;
    }
}

public sealed class SkillTargetFilterResult
{
    private readonly IReadOnlyList<SkillTargetFilterTrace> _traces;
    private readonly IReadOnlyList<ChaState> _accepted;
    private readonly IReadOnlyList<ChaState> _rejected;

    public SkillTargetMode targetMode { get; }
    public SkillTargetGeometryKind geometryKind { get; }
    public SkillTargetFilterPolicy policy { get; }
    public string summary { get; }
    public string fallbackPath { get; }
    public IReadOnlyList<SkillTargetFilterTrace> traces => _traces;
    public IReadOnlyList<ChaState> accepted => _accepted;
    public IReadOnlyList<ChaState> rejected => _rejected;

    public SkillTargetFilterResult(
        SkillTargetMode targetMode,
        SkillTargetGeometryKind geometryKind,
        SkillTargetFilterPolicy policy,
        IReadOnlyList<SkillTargetFilterTrace> traces,
        string summary,
        string fallbackPath
    )
    {
        this.targetMode = targetMode;
        this.geometryKind = geometryKind;
        this.policy = policy;
        this._traces = traces ?? Array.Empty<SkillTargetFilterTrace>();
        this._accepted = _traces.Where(trace => trace.included && trace.unit != null).Select(trace => trace.unit).ToArray();
        this._rejected = _traces.Where(trace => trace.included == false && trace.unit != null).Select(trace => trace.unit).ToArray();
        this.summary = summary ?? string.Empty;
        this.fallbackPath = fallbackPath ?? string.Empty;
    }

    public string Describe()
    {
        return "mode=" + targetMode + ", geometry=" + geometryKind + ", accepted=" + _accepted.Count + ", rejected=" + _rejected.Count + ", policy=" + policy.summary;
    }

    public IReadOnlyList<string> GetDetailLines()
    {
        List<string> lines = new List<string>();
        lines.Add("summary=" + summary);
        lines.Add("fallbackPath=" + fallbackPath);
        lines.Add("policySource=" + policy.source);
        lines.Add("policySummary=" + policy.summary);
        lines.Add("allowSelf=" + policy.allowSelf + ", allowAlly=" + policy.allowAlly + ", allowEnemy=" + policy.allowEnemy + ", allowDead=" + policy.allowDead + ", allowInvincible=" + policy.allowInvincible + ", allowInvisible=" + policy.allowInvisible);

        foreach (SkillTargetFilterTrace trace in _traces)
        {
            string unitName = trace.unit != null ? trace.unit.name : "null";
            string status = trace.included ? "accepted" : "rejected";
            lines.Add(status + ":" + unitName + " relation=" + trace.relation + " reason=" + trace.reason + " dead=" + trace.isDead + " invincible=" + trace.isInvincible + " invisible=" + trace.isInvisible + " distance=" + trace.distance.ToString("F3", CultureInfo.InvariantCulture) + " detail=" + trace.detail);
        }

        return lines;
    }
}

public static class SkillTargetFilterStrategy
{
    public static SkillTargetFilterResult Apply(SkillCastPreparationSnapshot snapshot, SkillTargetQueryResult queryResult)
    {
        if (EqualityComparer<SkillCastPreparationSnapshot>.Default.Equals(snapshot, default(SkillCastPreparationSnapshot)))
        {
            return new SkillTargetFilterResult(SkillTargetMode.Auto, SkillTargetGeometryKind.Unknown, SkillTargetFilterPolicy.Default, Array.Empty<SkillTargetFilterTrace>(), "snapshot missing", "snapshot unavailable");
        }

        if (queryResult == null)
        {
            return new SkillTargetFilterResult(snapshot.targetMode, snapshot.targetGeometryKind, SkillTargetFilterPolicy.Default, Array.Empty<SkillTargetFilterTrace>(), "queryResult missing", "query unavailable");
        }

        ChaState caster = ResolveCaster(snapshot);
        object skill = ResolveSkill(snapshot);
        SkillTargetFilterPolicy policy = ResolvePolicy(skill, snapshot.targetMode, snapshot.targetGeometryKind);

        List<SkillTargetFilterTrace> traces = new List<SkillTargetFilterTrace>();
        foreach (SkillTargetQueryTrace queryTrace in queryResult.traces)
        {
            if (queryTrace.unit == null)
            {
                traces.Add(new SkillTargetFilterTrace(null, false, SkillTargetRelationKind.Unknown, false, false, false, SkillTargetFilterReason.InvalidCandidate, queryTrace.distance, "candidate is null"));
                continue;
            }

            SkillTargetRelationKind relation = ResolveRelation(caster, queryTrace.unit);
            StatusFlags statusFlags = ResolveStatusFlags(queryTrace.unit);
            SkillTargetFilterReason reason;
            bool included = Evaluate(policy, relation, statusFlags, out reason);
            string detail = BuildDetail(relation, statusFlags, policy, reason, queryTrace);
            traces.Add(new SkillTargetFilterTrace(queryTrace.unit, included, relation, statusFlags.isDead, statusFlags.isInvincible, statusFlags.isInvisible, reason, queryTrace.distance, detail));
        }

        string summary = BuildSummary(snapshot.targetMode, snapshot.targetGeometryDecision.geometryKind, policy, traces.Count, queryResult.hits.Count);
        return new SkillTargetFilterResult(snapshot.targetMode, snapshot.targetGeometryDecision.geometryKind, policy, traces, summary, queryResult.fallbackPath);
    }

    private static bool Evaluate(SkillTargetFilterPolicy policy, SkillTargetRelationKind relation, StatusFlags statusFlags, out SkillTargetFilterReason reason)
    {
        if (statusFlags.isDead && policy.allowDead == false)
        {
            reason = SkillTargetFilterReason.DeadExcluded;
            return false;
        }

        if (statusFlags.isInvincible && policy.allowInvincible == false)
        {
            reason = SkillTargetFilterReason.InvincibleExcluded;
            return false;
        }

        if (statusFlags.isInvisible && policy.allowInvisible == false)
        {
            reason = SkillTargetFilterReason.InvisibleExcluded;
            return false;
        }

        switch (relation)
        {
            case SkillTargetRelationKind.Self:
                if (policy.allowSelf == false)
                {
                    reason = SkillTargetFilterReason.SelfExcluded;
                    return false;
                }
                break;
            case SkillTargetRelationKind.Ally:
                if (policy.allowAlly == false)
                {
                    reason = SkillTargetFilterReason.AllyExcluded;
                    return false;
                }
                break;
            case SkillTargetRelationKind.Enemy:
                if (policy.allowEnemy == false)
                {
                    reason = SkillTargetFilterReason.EnemyExcluded;
                    return false;
                }
                break;
            default:
                reason = SkillTargetFilterReason.UnknownRelationAllowed;
                return true;
        }

        reason = SkillTargetFilterReason.Allowed;
        return true;
    }

    private static string BuildDetail(SkillTargetRelationKind relation, StatusFlags statusFlags, SkillTargetFilterPolicy policy, SkillTargetFilterReason reason, SkillTargetQueryTrace queryTrace)
    {
        return "relation=" + relation + ", reason=" + reason + ", dead=" + statusFlags.isDead + ", invincible=" + statusFlags.isInvincible + ", invisible=" + statusFlags.isInvisible + ", geometryReason=" + queryTrace.reason + ", policy=" + policy.summary;
    }

    private static string BuildSummary(SkillTargetMode targetMode, SkillTargetGeometryKind geometryKind, SkillTargetFilterPolicy policy, int traceCount, int acceptedCount)
    {
        return "mode=" + targetMode + ", geometry=" + geometryKind + ", traces=" + traceCount + ", accepted=" + acceptedCount + ", policySource=" + policy.source;
    }

    private static SkillTargetRelationKind ResolveRelation(ChaState caster, ChaState target)
    {
        if (ReferenceEquals(caster, target))
        {
            return SkillTargetRelationKind.Self;
        }

        if (caster == null || target == null)
        {
            return SkillTargetRelationKind.Unknown;
        }

        object casterTeam = ResolveTeamValue(caster);
        object targetTeam = ResolveTeamValue(target);
        if (casterTeam != null && targetTeam != null)
        {
            return Equals(casterTeam, targetTeam) ? SkillTargetRelationKind.Ally : SkillTargetRelationKind.Enemy;
        }

        return SkillTargetRelationKind.Unknown;
    }

    private static object ResolveTeamValue(object unit)
    {
        string[] memberNames = { "teamId", "team", "camp", "faction", "side", "group", "party", "campId", "ownerTeamId" };
        foreach (string memberName in memberNames)
        {
            object value = GetMemberValue(unit, memberName);
            if (value != null)
            {
                return value;
            }
        }

        return null;
    }

    private static StatusFlags ResolveStatusFlags(object unit)
    {
        bool isDead = ResolveBool(unit, false, "isDead", "dead", "isDie", "isDied", "killed");
        bool isInvincible = ResolveBool(unit, false, "isInvincible", "invincible", "immune", "isImmune");
        bool isInvisible = ResolveBool(unit, false, "isInvisible", "invisible", "hidden", "isHidden");
        bool isAlive = ResolveBool(unit, true, "isAlive", "alive");
        float hp = ResolveFloat(unit, float.NaN, "hp", "currentHp", "currentHP", "health", "life", "curHp");

        if (float.IsNaN(hp) == false && hp <= 0f)
        {
            isDead = true;
        }

        if (isAlive == false)
        {
            isDead = true;
        }

        return new StatusFlags(isDead, isInvincible, isInvisible);
    }

    private static SkillTargetFilterPolicy ResolvePolicy(object skill, SkillTargetMode targetMode, SkillTargetGeometryKind geometryKind)
    {
        bool allowSelf = ResolveNullableBool(skill, null, "allowSelf", "canTargetSelf", "targetSelf", "selfTarget", "targetOwner") ?? true;
        bool allowAlly = ResolveNullableBool(skill, null, "allowAlly", "canTargetAlly", "targetFriend", "targetFriendly") ?? true;
        bool allowEnemy = ResolveNullableBool(skill, null, "allowEnemy", "canTargetEnemy", "targetHostile", "targetFoe") ?? true;
        bool allowDead = ResolveNullableBool(skill, null, "allowDead", "canTargetDead", "targetDead") ?? false;
        bool allowInvincible = ResolveNullableBool(skill, null, "allowInvincible", "canTargetInvincible", "targetInvincible") ?? false;
        bool allowInvisible = ResolveNullableBool(skill, null, "allowInvisible", "canTargetInvisible", "targetInvisible") ?? false;

        string source = skill != null ? "skill-config" : "default";
        string summary = "mode=" + targetMode + ", geometry=" + geometryKind + ", self=" + allowSelf + ", ally=" + allowAlly + ", enemy=" + allowEnemy + ", dead=" + allowDead + ", invincible=" + allowInvincible + ", invisible=" + allowInvisible;
        return new SkillTargetFilterPolicy(allowSelf, allowAlly, allowEnemy, allowDead, allowInvincible, allowInvisible, source, summary);
    }

    private static ChaState ResolveCaster(SkillCastPreparationSnapshot snapshot)
    {
        object caster = GetMemberValue(snapshot, "caster");
        if (caster is ChaState typedCaster)
        {
            return typedCaster;
        }

        object context = GetMemberValue(snapshot, "context");
        if (context != null)
        {
            object nestedCaster = GetMemberValue(context, "caster");
            if (nestedCaster is ChaState nestedTypedCaster)
            {
                return nestedTypedCaster;
            }
        }

        return null;
    }

    private static object ResolveSkill(SkillCastPreparationSnapshot snapshot)
    {
        object skill = GetMemberValue(snapshot, "skill");
        if (skill != null)
        {
            return skill;
        }

        object context = GetMemberValue(snapshot, "context");
        if (context != null)
        {
            object nestedSkill = GetMemberValue(context, "skill");
            if (nestedSkill != null)
            {
                return nestedSkill;
            }
        }

        return null;
    }

    private static bool ResolveBool(object owner, bool fallback, params string[] memberNames)
    {
        bool? value = ResolveNullableBool(owner, null, memberNames);
        return value ?? fallback;
    }

    private static bool? ResolveNullableBool(object owner, bool? fallback, params string[] memberNames)
    {
        foreach (string memberName in memberNames)
        {
            object value = GetMemberValue(owner, memberName);
            if (value is bool boolValue)
            {
                return boolValue;
            }
        }

        return fallback;
    }

    private static float ResolveFloat(object owner, float fallback, params string[] memberNames)
    {
        foreach (string memberName in memberNames)
        {
            object value = GetMemberValue(owner, memberName);
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

    private readonly struct StatusFlags
    {
        public bool isDead { get; }
        public bool isInvincible { get; }
        public bool isInvisible { get; }

        public StatusFlags(bool isDead, bool isInvincible, bool isInvisible)
        {
            this.isDead = isDead;
            this.isInvincible = isInvincible;
            this.isInvisible = isInvisible;
        }
    }
}

public sealed class SkillTargetCollectionResult
{
    public SkillTargetQueryResult queryResult { get; }
    public SkillTargetFilterResult filterResult { get; }
    public IReadOnlyList<ChaState> finalTargets => filterResult != null ? filterResult.accepted : Array.Empty<ChaState>();

    public SkillTargetCollectionResult(SkillTargetQueryResult queryResult, SkillTargetFilterResult filterResult)
    {
        this.queryResult = queryResult;
        this.filterResult = filterResult;
    }

    public string Describe()
    {
        if (filterResult != null)
        {
            return filterResult.Describe();
        }

        if (queryResult != null)
        {
            return queryResult.Describe();
        }

        return "collection=empty";
    }

    public IReadOnlyList<string> GetDetailLines()
    {
        List<string> lines = new List<string>();
        if (queryResult != null)
        {
            lines.Add("[query] " + queryResult.Describe());
            lines.AddRange(queryResult.GetDetailLines());
        }

        if (filterResult != null)
        {
            lines.Add("[filter] " + filterResult.Describe());
            lines.AddRange(filterResult.GetDetailLines());
        }

        return lines;
    }
}
