using System.Collections.Generic;
using UnityEngine;

public enum SkillTargetModeSource
{
    SkillModel,
    RequestHeuristic,
    DefaultFallback
}

public readonly struct SkillTargetModeDecision
{
    public SkillTargetMode mode { get; }
    public SkillTargetModeSource source { get; }
    public string fallbackPath { get; }

    public string modeName => mode.ToString();
    public string sourceName => source.ToString();

    public SkillTargetModeDecision(SkillTargetMode mode, SkillTargetModeSource source, string fallbackPath)
    {
        this.mode = mode;
        this.source = source;
        this.fallbackPath = fallbackPath;
    }

    public string Describe()
    {
        return "source=" + source + ", mode=" + mode;
    }
}

public static class SkillTargetModeStrategy
{
    public static readonly SkillTargetMode[] DefaultFallbackChain = new SkillTargetMode[]
    {
        SkillTargetMode.LockOnTarget,
        SkillTargetMode.GroundPoint,
        SkillTargetMode.Cone,
        SkillTargetMode.Line
    };

    public static SkillTargetModeDecision Resolve(SkillObj skill, SkillCastRequest request)
    {
        if (skill != null && skill.model.targetMode != SkillTargetMode.Auto)
        {
            return new SkillTargetModeDecision(
                skill.model.targetMode,
                SkillTargetModeSource.SkillModel,
                DescribeConfiguredPath(skill.model.targetMode)
            );
        }

        if (request.hasTarget && request.target != null)
        {
            return new SkillTargetModeDecision(
                SkillTargetMode.LockOnTarget,
                SkillTargetModeSource.RequestHeuristic,
                "request.target -> LockOnTarget"
            );
        }

        if (request.hasTargetPosition && request.hasAimDirection)
        {
            return new SkillTargetModeDecision(
                SkillTargetMode.Line,
                SkillTargetModeSource.RequestHeuristic,
                "request.targetPosition + request.aimDirection -> Line"
            );
        }

        if (request.hasTargetPosition)
        {
            return new SkillTargetModeDecision(
                SkillTargetMode.GroundPoint,
                SkillTargetModeSource.RequestHeuristic,
                "request.targetPosition -> GroundPoint"
            );
        }

        if (request.hasAimDirection)
        {
            return new SkillTargetModeDecision(
                SkillTargetMode.Line,
                SkillTargetModeSource.RequestHeuristic,
                "request.aimDirection -> Line"
            );
        }

        return new SkillTargetModeDecision(
            SkillTargetMode.GroundPoint,
            SkillTargetModeSource.DefaultFallback,
            "default -> GroundPoint"
        );
    }

    public static IReadOnlyList<SkillTargetMode> GetFallbackChain(SkillTargetMode mode)
    {
        switch (mode)
        {
            case SkillTargetMode.GroundPoint:
                return new SkillTargetMode[]
                {
                    SkillTargetMode.GroundPoint,
                    SkillTargetMode.LockOnTarget,
                    SkillTargetMode.Cone,
                    SkillTargetMode.Line
                };
            case SkillTargetMode.Cone:
                return new SkillTargetMode[]
                {
                    SkillTargetMode.Cone,
                    SkillTargetMode.LockOnTarget,
                    SkillTargetMode.GroundPoint,
                    SkillTargetMode.Line
                };
            case SkillTargetMode.Line:
                return new SkillTargetMode[]
                {
                    SkillTargetMode.Line,
                    SkillTargetMode.GroundPoint,
                    SkillTargetMode.LockOnTarget,
                    SkillTargetMode.Cone
                };
            case SkillTargetMode.LockOnTarget:
            default:
                return new SkillTargetMode[]
                {
                    SkillTargetMode.LockOnTarget,
                    SkillTargetMode.GroundPoint,
                    SkillTargetMode.Cone,
                    SkillTargetMode.Line
                };
        }
    }

    public static string DescribeFallbackPath(SkillTargetMode mode)
    {
        return string.Join(" -> ", GetFallbackChain(mode));
    }

    private static string DescribeConfiguredPath(SkillTargetMode mode)
    {
        return "SkillModel.targetMode -> " + mode;
    }
}
