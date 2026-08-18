using System.Collections.Generic;
using UnityEngine;

public enum SkillDirectionSource
{
    None,
    ExplicitDirection,
    TargetUnitDirection,
    TargetPositionDirection,
    CasterFacing,
    WorldUp
}

public readonly struct SkillDirectionDecision
{
    public SkillDirectionSource source { get; }
    public Vector3 direction { get; }
    public bool hasDirection => direction.sqrMagnitude > 0.0001f;

    public SkillDirectionDecision(SkillDirectionSource source, Vector3 direction)
    {
        this.source = source;
        this.direction = direction;
    }

    public string Describe()
    {
        return "source=" + source + ", direction=" + direction;
    }
}

public static class SkillDirectionStrategy
{
    public static readonly SkillDirectionSource[] DefaultFallbackChain = new SkillDirectionSource[]
    {
        SkillDirectionSource.ExplicitDirection,
        SkillDirectionSource.TargetUnitDirection,
        SkillDirectionSource.TargetPositionDirection,
        SkillDirectionSource.CasterFacing,
        SkillDirectionSource.WorldUp
    };

    private static readonly SkillDirectionSource[] GroundPointFallbackChain = new SkillDirectionSource[]
    {
        SkillDirectionSource.ExplicitDirection,
        SkillDirectionSource.TargetPositionDirection,
        SkillDirectionSource.CasterFacing,
        SkillDirectionSource.WorldUp
    };

    private static readonly SkillDirectionSource[] ConeFallbackChain = new SkillDirectionSource[]
    {
        SkillDirectionSource.ExplicitDirection,
        SkillDirectionSource.TargetUnitDirection,
        SkillDirectionSource.TargetPositionDirection,
        SkillDirectionSource.CasterFacing,
        SkillDirectionSource.WorldUp
    };

    private static readonly SkillDirectionSource[] LineFallbackChain = new SkillDirectionSource[]
    {
        SkillDirectionSource.ExplicitDirection,
        SkillDirectionSource.TargetPositionDirection,
        SkillDirectionSource.TargetUnitDirection,
        SkillDirectionSource.CasterFacing,
        SkillDirectionSource.WorldUp
    };

    public static SkillDirectionDecision Resolve(ChaState caster, SkillCastRequest request, SkillTargetSelectionDecision targetSelection)
    {
        return Resolve(SkillTargetMode.LockOnTarget, caster, request, targetSelection);
    }

    public static SkillDirectionDecision Resolve(SkillTargetMode targetMode, ChaState caster, SkillCastRequest request, SkillTargetSelectionDecision targetSelection)
    {
        if (request.hasAimDirection)
        {
            return new SkillDirectionDecision(
                SkillDirectionSource.ExplicitDirection,
                request.aimDirection.normalized
            );
        }

        switch (targetMode)
        {
            case SkillTargetMode.GroundPoint:
                return ResolveGroundPointDirection(caster, targetSelection);
            case SkillTargetMode.Cone:
                return ResolveConeDirection(caster, request, targetSelection);
            case SkillTargetMode.Line:
                return ResolveLineDirection(caster, request, targetSelection);
            case SkillTargetMode.LockOnTarget:
            default:
                return ResolveLockOnTargetDirection(caster, request, targetSelection);
        }
    }

    public static IReadOnlyList<SkillDirectionSource> GetFallbackChain(SkillTargetMode targetMode)
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

    private static SkillDirectionDecision ResolveLockOnTargetDirection(ChaState caster, SkillCastRequest request, SkillTargetSelectionDecision targetSelection)
    {
        if (request.hasTarget && request.target != null && caster != null)
        {
            Vector3 targetDirection = request.target.transform.position - caster.transform.position;
            if (targetDirection.sqrMagnitude > 0.0001f)
            {
                return new SkillDirectionDecision(
                    SkillDirectionSource.TargetUnitDirection,
                    targetDirection.normalized
                );
            }
        }

        if (caster != null)
        {
            Vector3 positionDirection = targetSelection.targetPosition - caster.transform.position;
            if (positionDirection.sqrMagnitude > 0.0001f)
            {
                return new SkillDirectionDecision(
                    SkillDirectionSource.TargetPositionDirection,
                    positionDirection.normalized
                );
            }

            Vector3 facingDirection = caster.transform.up;
            if (facingDirection.sqrMagnitude > 0.0001f)
            {
                return new SkillDirectionDecision(
                    SkillDirectionSource.CasterFacing,
                    facingDirection.normalized
                );
            }
        }

        return new SkillDirectionDecision(SkillDirectionSource.WorldUp, Vector3.up);
    }

    private static SkillDirectionDecision ResolveGroundPointDirection(ChaState caster, SkillTargetSelectionDecision targetSelection)
    {
        if (caster != null)
        {
            Vector3 positionDirection = targetSelection.targetPosition - caster.transform.position;
            if (positionDirection.sqrMagnitude > 0.0001f)
            {
                return new SkillDirectionDecision(
                    SkillDirectionSource.TargetPositionDirection,
                    positionDirection.normalized
                );
            }

            Vector3 facingDirection = caster.transform.up;
            if (facingDirection.sqrMagnitude > 0.0001f)
            {
                return new SkillDirectionDecision(
                    SkillDirectionSource.CasterFacing,
                    facingDirection.normalized
                );
            }
        }

        return new SkillDirectionDecision(SkillDirectionSource.WorldUp, Vector3.up);
    }

    private static SkillDirectionDecision ResolveConeDirection(ChaState caster, SkillCastRequest request, SkillTargetSelectionDecision targetSelection)
    {
        if (request.hasTarget && request.target != null && caster != null)
        {
            Vector3 targetDirection = request.target.transform.position - caster.transform.position;
            if (targetDirection.sqrMagnitude > 0.0001f)
            {
                return new SkillDirectionDecision(
                    SkillDirectionSource.TargetUnitDirection,
                    targetDirection.normalized
                );
            }
        }

        return ResolveGroundPointDirection(caster, targetSelection);
    }

    private static SkillDirectionDecision ResolveLineDirection(ChaState caster, SkillCastRequest request, SkillTargetSelectionDecision targetSelection)
    {
        if (caster != null)
        {
            Vector3 positionDirection = targetSelection.targetPosition - caster.transform.position;
            if (positionDirection.sqrMagnitude > 0.0001f)
            {
                return new SkillDirectionDecision(
                    SkillDirectionSource.TargetPositionDirection,
                    positionDirection.normalized
                );
            }

            if (request.hasTarget && request.target != null)
            {
                Vector3 targetDirection = request.target.transform.position - caster.transform.position;
                if (targetDirection.sqrMagnitude > 0.0001f)
                {
                    return new SkillDirectionDecision(
                        SkillDirectionSource.TargetUnitDirection,
                        targetDirection.normalized
                    );
                }
            }
        }

        return ResolveGroundPointDirection(caster, targetSelection);
    }
}
