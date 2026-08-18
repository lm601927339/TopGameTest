using System;
using System.Collections.Generic;
using UnityEngine;

public static class SkillTargetResolver
{
    public static SkillTargetResolution Resolve(ChaState caster, SkillCastRequest request)
    {
        return Resolve(caster, null, request);
    }

    public static SkillTargetModeDecision ResolveTargetMode(SkillObj skill, SkillCastRequest request)
    {
        return SkillTargetModeStrategy.Resolve(skill, request);
    }

    public static SkillTargetResolution Resolve(ChaState caster, SkillObj skill, SkillCastRequest request)
    {
        SkillTargetModeDecision targetModeDecision = ResolveTargetMode(skill, request);
        SkillTargetSelectionDecision targetSelection = ResolveTargetSelection(targetModeDecision.mode, caster, request);
        SkillDirectionDecision directionDecision = ResolveDirection(targetModeDecision.mode, caster, request, targetSelection);

        return new SkillTargetResolution(
            targetSelection.target,
            targetSelection.targetPosition,
            directionDecision.direction
        );
    }

    public static SkillTargetSelectionDecision ResolveTargetSelection(ChaState caster, SkillCastRequest request)
    {
        return ResolveTargetSelection(SkillTargetMode.LockOnTarget, caster, request);
    }

    public static SkillTargetSelectionDecision ResolveTargetSelection(SkillTargetMode targetMode, ChaState caster, SkillCastRequest request)
    {
        return SkillTargetSelectionStrategy.Resolve(targetMode, caster, request);
    }

    public static SkillDirectionDecision ResolveDirection(ChaState caster, SkillCastRequest request, SkillTargetSelectionDecision targetSelection)
    {
        return ResolveDirection(SkillTargetMode.LockOnTarget, caster, request, targetSelection);
    }

    public static SkillDirectionDecision ResolveDirection(
        SkillTargetMode targetMode,
        ChaState caster,
        SkillCastRequest request,
        SkillTargetSelectionDecision targetSelection
    )
    {
        return SkillDirectionStrategy.Resolve(targetMode, caster, request, targetSelection);
    }

    public static string Describe(SkillTargetResolution resolution)
    {
        return "target=" + DescribeTarget(resolution.target)
            + ", pos=" + resolution.targetPosition
            + ", dir=" + resolution.aimDirection;
    }

    public static SkillTargetCollectionResult ResolveTargetCollection(SkillCastPreparationSnapshot snapshot)
    {
        if (EqualityComparer<SkillCastPreparationSnapshot>.Default.Equals(snapshot, default(SkillCastPreparationSnapshot)))
        {
            return new SkillTargetCollectionResult(null, null);
        }

        if (snapshot.context == null)
        {
            return new SkillTargetCollectionResult(null, null);
        }

        IReadOnlyList<ChaState> candidates = CollectTargetCandidates(snapshot.context.caster);
        return SkillTargetQuery.CollectDetailed(snapshot, candidates);
    }

    private static IReadOnlyList<ChaState> CollectTargetCandidates(ChaState caster)
    {
        if (caster == null)
        {
            return Array.Empty<ChaState>();
        }

        try
        {
            ChaState[] candidates = UnityEngine.Object.FindObjectsOfType<ChaState>();
            return candidates ?? Array.Empty<ChaState>();
        }
        catch
        {
            return Array.Empty<ChaState>();
        }
    }

    private static string DescribeTarget(GameObject target)
    {
        if (target == null)
        {
            return "none";
        }

        return target.name;
    }
}

public readonly struct SkillTargetResolution
{
    public GameObject target { get; }
    public Vector3 targetPosition { get; }
    public Vector3 aimDirection { get; }

    public bool hasTarget => target != null;
    public bool hasTargetPosition => targetPosition != Vector3.zero;
    public bool hasAimDirection => aimDirection.sqrMagnitude > 0.0001f;

    public SkillTargetResolution(GameObject target, Vector3 targetPosition, Vector3 aimDirection)
    {
        this.target = target;
        this.targetPosition = targetPosition;
        this.aimDirection = aimDirection;
    }
}
