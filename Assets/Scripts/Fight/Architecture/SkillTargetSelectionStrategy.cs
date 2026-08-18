using System.Collections.Generic;
using UnityEngine;

public enum SkillTargetSelectionSource
{
    None,
    TargetUnit,
    ExplicitPosition,
    CasterPosition
}

public readonly struct SkillTargetSelectionDecision
{
    public SkillTargetSelectionSource source { get; }
    public GameObject target { get; }
    public Vector3 targetPosition { get; }
    public GameObject targetUnit => target;
    public bool hasTargetUnit => target != null;
    public bool hasTarget => target != null;
    public bool hasTargetPosition => source != SkillTargetSelectionSource.None;

    public SkillTargetSelectionDecision(SkillTargetSelectionSource source, GameObject target, Vector3 targetPosition)
    {
        this.source = source;
        this.target = target;
        this.targetPosition = targetPosition;
    }

    public string Describe()
    {
        string targetName = target != null ? target.name : "none";
        return "source=" + source + ", target=" + targetName + ", position=" + targetPosition;
    }
}

public static class SkillTargetSelectionStrategy
{
    public static readonly SkillTargetSelectionSource[] DefaultFallbackChain = new SkillTargetSelectionSource[]
    {
        SkillTargetSelectionSource.TargetUnit,
        SkillTargetSelectionSource.ExplicitPosition,
        SkillTargetSelectionSource.CasterPosition
    };

    private static readonly SkillTargetSelectionSource[] GroundPointFallbackChain = new SkillTargetSelectionSource[]
    {
        SkillTargetSelectionSource.ExplicitPosition,
        SkillTargetSelectionSource.TargetUnit,
        SkillTargetSelectionSource.CasterPosition
    };

    private static readonly SkillTargetSelectionSource[] ConeFallbackChain = new SkillTargetSelectionSource[]
    {
        SkillTargetSelectionSource.TargetUnit,
        SkillTargetSelectionSource.ExplicitPosition,
        SkillTargetSelectionSource.CasterPosition
    };

    private static readonly SkillTargetSelectionSource[] LineFallbackChain = new SkillTargetSelectionSource[]
    {
        SkillTargetSelectionSource.ExplicitPosition,
        SkillTargetSelectionSource.TargetUnit,
        SkillTargetSelectionSource.CasterPosition
    };

    public static SkillTargetSelectionDecision Resolve(ChaState caster, SkillCastRequest request)
    {
        return Resolve(SkillTargetMode.LockOnTarget, caster, request);
    }

    public static SkillTargetSelectionDecision Resolve(SkillTargetMode targetMode, ChaState caster, SkillCastRequest request)
    {
        switch (targetMode)
        {
            case SkillTargetMode.GroundPoint:
                return ResolveByOrder(GroundPointFallbackChain, caster, request);
            case SkillTargetMode.Cone:
                return ResolveByOrder(ConeFallbackChain, caster, request);
            case SkillTargetMode.Line:
                return ResolveByOrder(LineFallbackChain, caster, request);
            case SkillTargetMode.LockOnTarget:
            default:
                return ResolveByOrder(DefaultFallbackChain, caster, request);
        }
    }

    public static IReadOnlyList<SkillTargetSelectionSource> GetFallbackChain(SkillTargetMode targetMode)
    {
        switch (targetMode)
        {
            case SkillTargetMode.GroundPoint:
                return GroundPointFallbackChain;
            case SkillTargetMode.Cone:
                return ConeFallbackChain;
            case SkillTargetMode.Line:
                return LineFallbackChain;
            case SkillTargetMode.LockOnTarget:
            default:
                return DefaultFallbackChain;
        }
    }

    public static string DescribeFallbackPath(SkillTargetMode targetMode)
    {
        return string.Join(" -> ", GetFallbackChain(targetMode));
    }

    private static SkillTargetSelectionDecision ResolveByOrder(SkillTargetSelectionSource[] chain, ChaState caster, SkillCastRequest request)
    {
        for (int i = 0; i < chain.Length; i++)
        {
            SkillTargetSelectionSource source = chain[i];
            switch (source)
            {
                case SkillTargetSelectionSource.TargetUnit:
                    if (request.hasTarget && request.target != null)
                    {
                        return new SkillTargetSelectionDecision(
                            SkillTargetSelectionSource.TargetUnit,
                            request.target,
                            request.target.transform.position
                        );
                    }
                    break;
                case SkillTargetSelectionSource.ExplicitPosition:
                    if (request.hasTargetPosition)
                    {
                        return new SkillTargetSelectionDecision(
                            SkillTargetSelectionSource.ExplicitPosition,
                            null,
                            request.targetPosition
                        );
                    }
                    break;
                case SkillTargetSelectionSource.CasterPosition:
                    return new SkillTargetSelectionDecision(
                        SkillTargetSelectionSource.CasterPosition,
                        null,
                        caster != null ? caster.transform.position : Vector3.zero
                    );
            }
        }

        return new SkillTargetSelectionDecision(
            SkillTargetSelectionSource.CasterPosition,
            null,
            caster != null ? caster.transform.position : Vector3.zero
        );
    }
}
