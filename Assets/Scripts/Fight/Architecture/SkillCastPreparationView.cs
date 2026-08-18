using System.Collections.Generic;

public readonly struct SkillCastPreparationView
{
    public SkillCastPreparationSnapshot snapshot { get; }
    public SkillCastPlanSnapshot planSnapshot => snapshot.planSnapshot;
    public SkillCastPlan plan => snapshot.plan;
    public SkillCastPreparation preparation => snapshot.preparation;
    public SkillCastContext context => snapshot.context;
    public SkillTargetResolution targetResolution => snapshot.targetResolution;
    public SkillTargetModeDecision targetModeDecision => snapshot.targetModeDecision;
    public SkillTargetSelectionDecision targetSelectionDecision => snapshot.targetSelectionDecision;
    public SkillDirectionDecision directionDecisionData => snapshot.directionDecisionData;
    public SkillTargetMode targetMode => snapshot.targetMode;
    public SkillTargetModeSource targetModeSource => snapshot.targetModeSource;
    public SkillTargetSelectionSource targetSource => snapshot.targetSource;
    public SkillDirectionSource directionSource => snapshot.directionSource;
    public string targetModeName => snapshot.targetModeName;
    public string targetModeFallbackPath => snapshot.targetModeFallbackPath;
    public IReadOnlyList<SkillTargetMode> targetModeFallbackChain => snapshot.targetModeFallbackChain;
    public IReadOnlyList<SkillTargetSelectionSource> targetFallbackChain => snapshot.targetFallbackChain;
    public IReadOnlyList<SkillDirectionSource> directionFallbackChain => snapshot.directionFallbackChain;
    public bool hasTargetFallback => snapshot.hasTargetFallback;
    public bool hasDirectionFallback => snapshot.hasDirectionFallback;
    public bool hasKeyFallback => snapshot.hasKeyFallback;
    public string keyFallbackSummary => snapshot.keyFallbackSummary;
    public SkillTargetCollectionResult targetCollection => snapshot.targetCollection;
    public SkillTargetFilterResult targetFilterResult => snapshot.targetFilterResult;
    public SkillTargetFilterPresenter targetFilterPresenter => snapshot.targetFilterPresenter;
    public int targetFilterAcceptedCount => snapshot.targetFilterAcceptedCount;
    public int targetFilterRejectedCount => snapshot.targetFilterRejectedCount;
    public string targetFilterSummary => snapshot.targetFilterSummary;
    public IReadOnlyList<string> targetFilterDetailLines => snapshot.targetFilterDetailLines;
    public IReadOnlyList<string> targetFilterTraceLines => snapshot.targetFilterTraceLines;
    public SkillTargetFilterPresenter filterPresenter => snapshot.filterPresenter;
    public int acceptedCount => snapshot.acceptedCount;
    public int rejectedCount => snapshot.rejectedCount;
    public string policySummary => snapshot.policySummary;
    public IReadOnlyList<string> traceLines => snapshot.traceLines;
    public string fallbackPath => snapshot.fallbackPath;
    public IReadOnlyList<string> filterSummaryLines => snapshot.filterSummaryLines;
    public IReadOnlyList<string> filterSummary => filterSummaryLines;
    public SkillCastPlanPresenter planPresenter => snapshot.planPresenter;
    public SkillCastPlanPresenter presenter => planPresenter;
    public SkillCastCommitResult? commitResult => snapshot.commitResult;
    public string skillId => snapshot.skillId;
    public bool canCast => snapshot.canCast;
    public SkillCastFailureReason failureReason => snapshot.failureReason;
    public string failureMessage => snapshot.failureMessage;
    public bool hasTimeline => snapshot.hasTimeline;
    public bool hasCommitResult => snapshot.hasCommitResult;
    public SkillCastCommitView commitView => snapshot.commitView;
    public string commitSummary => snapshot.commitSummary;
    public IReadOnlyList<string> commitStageSummaries => snapshot.commitStageSummaries;
    public IReadOnlyList<string> commitDetailLines => snapshot.commitView.GetDetailLines();
    public bool hasTarget => snapshot.hasTarget;
    public bool hasTargetPosition => snapshot.hasTargetPosition;
    public bool hasAimDirection => snapshot.hasAimDirection;
    public string targetSummary => snapshot.targetSummary;
    public string targetModeSummary => snapshot.targetModeDecision.Describe();
    public string targetDecision => snapshot.targetSelectionSummary;
    public string directionDecision => snapshot.directionSummary;
    public string decisionSummary => snapshot.decisionSummary;
    public string targetFallbackPath => snapshot.targetFallbackPath;
    public string directionFallbackPath => snapshot.directionFallbackPath;
    public bool hasPayload => snapshot.hasPayload;
    public string payloadSummary => snapshot.payloadSummary;

    public SkillCastPreparationView(SkillCastPreparation preparation)
    {
        this.snapshot = preparation.ToSnapshot();
    }

    public SkillCastPreparationView(SkillCastPlan plan)
    {
        this.snapshot = new SkillCastPreparationSnapshot(plan);
    }

    public SkillCastPreparationView(SkillCastPreparationSnapshot snapshot)
    {
        this.snapshot = snapshot;
    }

    public SkillCastPreparationView(SkillCastResult result)
    {
        this.snapshot = new SkillCastPreparationSnapshot(result);
    }

    public SkillTargetGeometryKind targetGeometryKind => snapshot.targetGeometryKind;
    public string targetGeometrySource => snapshot.targetGeometrySource;
    public string targetGeometrySummary => snapshot.targetGeometrySummary;
    public string targetGeometryFallbackPath => snapshot.targetGeometryFallbackPath;

    public string Describe()
    {
        return snapshot.Describe();
    }

    public IReadOnlyList<string> GetDetailLines()
    {
        return snapshot.GetDetailLines();
    }
}
