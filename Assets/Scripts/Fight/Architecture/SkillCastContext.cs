using System.Collections.Generic;
using UnityEngine;

public sealed class SkillCastContext
{
    public ChaState caster { get; }
    public SkillObj skill { get; }
    public SkillCastRequest request { get; }

    public SkillTargetModeDecision targetModeDecision { get; }
    public SkillTargetSelectionDecision targetSelection { get; }
    public SkillDirectionDecision directionDecision { get; }
    public SkillTargetResolution targetResolution { get; }
    public SkillTargetMode targetMode => targetModeDecision.mode;
    public SkillTargetModeSource targetModeSource => targetModeDecision.source;
    public SkillTargetSelectionDecision targetSelectionDecision => targetSelection;
    public SkillDirectionDecision directionDecisionData => directionDecision;
    public GameObject target => targetResolution.target;
    public bool hasTarget => targetResolution.hasTarget;
    public bool requestedHasTarget => request.hasTarget;
    public bool requestedHasTargetPosition => request.hasTargetPosition;
    public bool requestedHasAimDirection => request.hasAimDirection;
    public string targetModeSummary => targetModeDecision.Describe();
    public SkillTargetSelectionSource targetSource => targetSelection.source;
    public SkillDirectionSource directionSource => directionDecision.source;
    public Vector3 requestedTargetPosition => request.targetPosition;
    public Vector3 requestedAimDirection => request.aimDirection;
    public Vector3 resolvedTargetPosition => targetResolution.targetPosition;
    public Vector3 resolvedAimDirection => targetResolution.aimDirection;
    public Vector3 targetPosition => targetResolution.targetPosition;
    public Vector3 aimDirection => targetResolution.aimDirection;
    public object payload => request.payload;
    public bool hasPayload => payload != null;
    public string payloadSummary => hasPayload ? payload.GetType().Name : "none";
    public GameObject casterObject => caster ? caster.gameObject : null;
    public Vector3 casterPosition => caster ? caster.transform.position : Vector3.zero;
    public Vector3 castOrigin => casterPosition;
    public IReadOnlyList<SkillTargetMode> targetModeFallbackChain => SkillTargetModeStrategy.GetFallbackChain(targetMode);
    public string targetSelectionSummary => targetSelection.Describe();
    public string directionSummary => directionDecision.Describe();
    public string decisionSummary => "mode=" + targetModeSummary + ", target=" + targetSelectionSummary + ", direction=" + directionSummary;
    public IReadOnlyList<SkillTargetSelectionSource> targetFallbackChain => SkillTargetSelectionStrategy.GetFallbackChain(targetMode);
    public IReadOnlyList<SkillDirectionSource> directionFallbackChain => SkillDirectionStrategy.GetFallbackChain(targetMode);
    public string targetFallbackPath => SkillTargetSelectionStrategy.DescribeFallbackPath(targetMode);
    public string directionFallbackPath => SkillDirectionStrategy.DescribeFallbackPath(targetMode);
    public string targetModeFallbackPath => targetModeDecision.fallbackPath;
    public bool hasSkillModel => skill != null && EqualityComparer<SkillModel>.Default.Equals(skill.model, default(SkillModel)) == false;
    public bool hasSkillEffect => hasSkillModel && EqualityComparer<TimelineModel>.Default.Equals(skill.model.effect, default(TimelineModel)) == false;
    public bool hasCasterResource => caster != null && caster.resource != null;
    public bool hasCasterBuffs => caster != null && caster.buffs != null;

    public SkillCastContext(ChaState caster, SkillObj skill, SkillCastRequest request)
    {
        SkillTargetModeDecision targetModeDecision = SkillTargetResolver.ResolveTargetMode(skill, request);
        SkillTargetSelectionDecision targetSelection = SkillTargetResolver.ResolveTargetSelection(targetModeDecision.mode, caster, request);
        SkillDirectionDecision directionDecision = SkillTargetResolver.ResolveDirection(targetModeDecision.mode, caster, request, targetSelection);

        this.caster = caster;
        this.skill = skill;
        this.request = request;
        this.targetModeDecision = targetModeDecision;
        this.targetSelection = targetSelection;
        this.directionDecision = directionDecision;
        this.targetResolution = new SkillTargetResolution(
            targetSelection.target,
            targetSelection.targetPosition,
            directionDecision.direction
        );
    }

    public SkillCastContext(
        ChaState caster,
        SkillObj skill,
        SkillCastRequest request,
        SkillTargetSelectionDecision targetSelection,
        SkillDirectionDecision directionDecision,
        SkillTargetResolution targetResolution
    )
    {
        this.caster = caster;
        this.skill = skill;
        this.request = request;
        this.targetModeDecision = SkillTargetModeStrategy.Resolve(skill, request);
        this.targetSelection = targetSelection;
        this.directionDecision = directionDecision;
        this.targetResolution = targetResolution;
    }
}
