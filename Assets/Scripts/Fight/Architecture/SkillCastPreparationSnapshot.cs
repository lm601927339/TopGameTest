using System.Collections.Generic;
using System.Text;
using UnityEngine;

public readonly struct SkillCastPreparationSnapshot
{
    public SkillCastPlanSnapshot planSnapshot { get; }
    public SkillCastPlan plan => planSnapshot.plan;
    public SkillCastPreparation preparation => planSnapshot.preparation;
    public SkillCastCommitResult? commitResult => planSnapshot.commitResult;
    public SkillCastContext context => planSnapshot.context;
    public SkillTargetResolution targetResolution => planSnapshot.targetResolution;
    public SkillTargetModeDecision targetModeDecision => planSnapshot.targetModeDecision;
    public SkillTargetMode targetMode => planSnapshot.targetMode;
    public SkillTargetModeSource targetModeSource => planSnapshot.targetModeSource;
    public SkillTargetGeometryDecision targetGeometryDecision => planSnapshot.geometryDecision;
    public SkillTargetSelectionDecision targetSelectionDecision => planSnapshot.targetSelectionDecision;
    public SkillDirectionDecision directionDecisionData => planSnapshot.directionDecisionData;
    public SkillTargetSelectionSource targetSource => planSnapshot.targetSource;
    public SkillDirectionSource directionSource => planSnapshot.directionSource;
    public SkillTargetGeometryDecision geometryDecision => planSnapshot.geometryDecision;
    public SkillTargetGeometryKind geometryKind => planSnapshot.geometryKind;
    public SkillTargetGeometrySource geometrySource => planSnapshot.geometrySource;
    public SkillTargetMode geometryTargetMode => planSnapshot.geometryTargetMode;
    public Vector3 geometryOrigin => planSnapshot.geometryOrigin;
    public Vector3 geometryDirection => planSnapshot.geometryDirection;
    public Vector3 geometryAnchorPoint => planSnapshot.geometryAnchorPoint;
    public ChaState geometryTargetUnit => planSnapshot.geometryTargetUnit;
    public string geometrySummary => planSnapshot.geometrySummary;
    public string geometryFallbackPath => planSnapshot.geometryFallbackPath;
    public bool hasGeometryTargetUnit => planSnapshot.hasGeometryTargetUnit;
    public bool hasGeometryAnchorPoint => planSnapshot.hasGeometryAnchorPoint;
    public string targetModeName => planSnapshot.targetModeName;
    public IReadOnlyList<SkillTargetMode> targetModeFallbackChain => planSnapshot.targetModeFallbackChain;
    public IReadOnlyList<SkillTargetSelectionSource> targetFallbackChain => planSnapshot.targetFallbackChain;
    public IReadOnlyList<SkillDirectionSource> directionFallbackChain => planSnapshot.directionFallbackChain;
    public bool hasModeFallback => planSnapshot.hasModeFallback;
    public bool hasTargetFallback => planSnapshot.hasTargetFallback;
    public bool hasDirectionFallback => planSnapshot.hasDirectionFallback;
    public bool hasKeyFallback => planSnapshot.hasKeyFallback;
    public string keyFallbackSummary => planSnapshot.keyFallbackSummary;
    public SkillTargetCollectionResult targetCollection => planSnapshot.targetCollection;
    public SkillTargetFilterResult targetFilterResult => planSnapshot.targetFilterResult;
    public SkillTargetFilterPresenter targetFilterPresenter => planSnapshot.targetFilterPresenter;
    public int targetFilterAcceptedCount => planSnapshot.targetFilterAcceptedCount;
    public int targetFilterRejectedCount => planSnapshot.targetFilterRejectedCount;
    public string targetFilterSummary => planSnapshot.targetFilterSummary;
    public IReadOnlyList<string> targetFilterDetailLines => planSnapshot.targetFilterDetailLines;
    public IReadOnlyList<string> targetFilterTraceLines => planSnapshot.targetFilterTraceLines;
    public SkillTargetFilterPresenter filterPresenter => planSnapshot.filterPresenter;
    public int acceptedCount => planSnapshot.acceptedCount;
    public int rejectedCount => planSnapshot.rejectedCount;
    public string policySummary => planSnapshot.policySummary;
    public IReadOnlyList<string> traceLines => planSnapshot.traceLines;
    public string fallbackPath => planSnapshot.fallbackPath;
    public IReadOnlyList<string> filterSummaryLines => planSnapshot.filterSummaryLines;
    public IReadOnlyList<string> filterSummary => filterSummaryLines;
    public SkillCastPlanPresenter planPresenter => planSnapshot.presenter;
    public SkillCastPlanPresenter presenter => planPresenter;
    public string skillId => planSnapshot.skillId;
    public bool success => planSnapshot.success;
    public bool canCast => planSnapshot.canCast;
    public SkillCastFailureReason failureReason => planSnapshot.failureReason;
    public string failureMessage => planSnapshot.failureMessage;
    public bool hasTimeline => planSnapshot.hasTimeline;
    public SkillCastCommitView commitView => planSnapshot.commitView;
    public bool hasTarget => planSnapshot.hasTarget;
    public bool hasTargetPosition => planSnapshot.hasTargetPosition;
    public bool hasAimDirection => planSnapshot.hasAimDirection;
    public string targetSummary => planSnapshot.targetSummary;
    public string targetSelectionSummary => planSnapshot.targetSelectionSummary;
    public string directionSummary => planSnapshot.directionSummary;
    public string decisionSummary => planSnapshot.decisionSummary;
    public string targetModeFallbackPath => planSnapshot.targetModeFallbackPath;
    public string targetFallbackPath => planSnapshot.targetFallbackPath;
    public string directionFallbackPath => planSnapshot.directionFallbackPath;
    public bool hasCommitResult => planSnapshot.hasCommitResult;
    public string commitSummary => planSnapshot.commitSummary;
    public IReadOnlyList<string> commitStageSummaries => planSnapshot.commitStageSummaries;
    public SkillTargetGeometryKind targetGeometryKind => planSnapshot.targetGeometryKind;
    public string targetGeometrySource => planSnapshot.targetGeometrySource;
    public string targetGeometrySummary => planSnapshot.targetGeometrySummary;
    public string targetGeometryFallbackPath => planSnapshot.targetGeometryFallbackPath;
    public Vector3 casterPosition => planSnapshot.casterPosition;
    public Vector3 requestedTargetPosition => planSnapshot.requestedTargetPosition;
    public Vector3 requestedAimDirection => planSnapshot.requestedAimDirection;
    public Vector3 resolvedTargetPosition => planSnapshot.resolvedTargetPosition;
    public Vector3 resolvedAimDirection => planSnapshot.resolvedAimDirection;
    public bool hasPayload => planSnapshot.hasPayload;
    public string payloadSummary => planSnapshot.payloadSummary;

    public SkillCastPreparationSnapshot(SkillCastPlanSnapshot planSnapshot)
    {
        this.planSnapshot = planSnapshot;
    }

    public SkillCastPreparationSnapshot(SkillCastPlan plan)
        : this(new SkillCastPlanSnapshot(plan))
    {
    }

    public SkillCastPreparationSnapshot(SkillCastPreparation preparation)
        : this(new SkillCastPlanSnapshot(preparation))
    {
    }

    public SkillCastPreparationSnapshot(SkillCastPreparation preparation, SkillCastCommitResult? commitResult)
        : this(new SkillCastPlanSnapshot(preparation, commitResult))
    {
    }

    public SkillCastPreparationSnapshot(SkillCastResult result)
        : this(result.plan)
    {
    }

    public string Describe()
    {
        return planSnapshot.Describe();
    }

    public IReadOnlyList<string> GetDetailLines()
    {
        return planSnapshot.GetDetailLines();
    }
}
