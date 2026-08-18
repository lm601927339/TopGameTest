using System.Collections.Generic;

public readonly struct SkillCastPreparationDiagnostic
{
    public SkillCastPreparationSnapshot snapshot { get; }
    public SkillCastPlanSnapshot planSnapshot => snapshot.planSnapshot;
    public SkillCastPlan plan => snapshot.plan;
    public SkillCastPreparationView view => new SkillCastPreparationView(snapshot);
    public SkillCastCommitResult? commitResult => snapshot.commitResult;
    public SkillCastPreparation preparation => snapshot.preparation;
    public SkillTargetResolution targetResolution => snapshot.targetResolution;
    public SkillTargetModeDecision targetModeDecision => snapshot.targetModeDecision;
    public SkillTargetGeometryDecision geometryDecision => snapshot.geometryDecision;
    public string skillId => snapshot.skillId;
    public bool canCast => snapshot.canCast;
    public SkillCastFailureReason failureReason => snapshot.failureReason;
    public bool hasTimeline => snapshot.hasTimeline;
    public SkillCastCommitView commitView => snapshot.commitView;
    public SkillTargetMode targetMode => snapshot.targetMode;
    public SkillTargetModeSource targetModeSource => snapshot.targetModeSource;
    public string targetModeName => snapshot.targetModeName;
    public string targetModeFallbackPath => snapshot.targetModeFallbackPath;
    public SkillTargetGeometryKind targetGeometryKind => snapshot.targetGeometryKind;
    public string targetGeometrySource => snapshot.targetGeometrySource;
    public string targetGeometrySummary => snapshot.targetGeometrySummary;
    public string targetGeometryFallbackPath => snapshot.targetGeometryFallbackPath;
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
    public bool hasTargetFallback => snapshot.hasTargetFallback;
    public bool hasDirectionFallback => snapshot.hasDirectionFallback;
    public bool hasKeyFallback => snapshot.hasKeyFallback;
    public string keyFallbackSummary => snapshot.keyFallbackSummary;
    public bool hasCommitResult => snapshot.hasCommitResult;
    public string commitSummary => snapshot.commitSummary;
    public IReadOnlyList<string> commitStageSummaries => snapshot.commitStageSummaries;
    public IReadOnlyList<string> commitDetailLines => snapshot.commitView.GetDetailLines();
    public bool? hasEnoughResource
    {
        get
        {
            if (snapshot.canCast)
            {
                return true;
            }

            if (snapshot.failureReason == SkillCastFailureReason.ResourceNotEnough)
            {
                return false;
            }

            return null;
        }
    }
    public SkillTargetSelectionSource targetSource => snapshot.targetSource;
    public SkillDirectionSource directionSource => snapshot.directionSource;
    public SkillTargetSelectionDecision targetSelectionDecision => snapshot.targetSelectionDecision;
    public SkillDirectionDecision directionDecisionData => snapshot.directionDecisionData;
    public IReadOnlyList<SkillTargetMode> targetModeFallbackChain => snapshot.targetModeFallbackChain;
    public IReadOnlyList<SkillTargetSelectionSource> targetFallbackChain => snapshot.targetFallbackChain;
    public IReadOnlyList<SkillDirectionSource> directionFallbackChain => snapshot.directionFallbackChain;
    public string decisionSummary => snapshot.decisionSummary;
    public string targetFallbackPath => snapshot.targetFallbackPath;
    public string directionFallbackPath => snapshot.directionFallbackPath;
    public string targetDecision => snapshot.targetSelectionSummary;
    public string directionDecision => snapshot.directionSummary;
    public string failureMessage => snapshot.failureMessage;

    public SkillCastPreparationDiagnostic(SkillCastPreparation preparation)
    {
        this.snapshot = preparation.ToSnapshot();
    }

    public SkillCastPreparationDiagnostic(SkillCastPlan plan)
    {
        this.snapshot = new SkillCastPreparationSnapshot(plan);
    }

    public SkillCastPreparationDiagnostic(SkillCastPreparation preparation, SkillCastCommitResult? commitResult)
    {
        this.snapshot = new SkillCastPreparationSnapshot(preparation, commitResult);
    }

    public SkillCastPreparationDiagnostic(SkillCastResult result)
    {
        this.snapshot = new SkillCastPreparationSnapshot(result);
    }

    public string Describe()
    {
        return snapshot.Describe();
    }

    public IReadOnlyList<string> GetHints()
    {
        List<string> hints = new List<string>();
        IReadOnlyList<string> summaryLines = filterSummaryLines;
        if (summaryLines != null && summaryLines.Count > 0)
        {
            hints.Add("filterSummary:");
            hints.AddRange(summaryLines);
        }

        if (snapshot.canCast && snapshot.hasCommitResult == false)
        {
            hints.Add("ready");
            return hints;
        }

        if (snapshot.canCast)
        {
            hints.Add("ready");
        }

        if (snapshot.hasCommitResult)
        {
            hints.Add(snapshot.commitSummary);
            IReadOnlyList<string> commitStages = snapshot.commitStageSummaries;
            if (commitStages != null)
            {
                for (int i = 0; i < commitStages.Count; i++)
                {
                    hints.Add(commitStages[i]);
                }
            }
        }

        if (snapshot.canCast == false)
        {
            hints.Add(snapshot.failureMessage);
            if (snapshot.failureReason == SkillCastFailureReason.ResourceNotEnough)
            {
                hints.Add("check caster resources and cost");
            }

            if (snapshot.failureReason == SkillCastFailureReason.SkillCoolingDown)
            {
                hints.Add("wait for cooldown");
            }
        }

        return hints;
    }
}
