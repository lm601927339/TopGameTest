using System.Collections.Generic;
using System.Text;
using UnityEngine;

public readonly struct SkillCastPlan
{
    public SkillCastPreparation preparation { get; }
    public SkillCastCommitResult? commitResult { get; }
    public bool hasCommitResult => commitResult.HasValue;
    public bool success => preparation.success;
    public bool canCast => preparation.canCast;
    public SkillCastFailureReason failureReason => preparation.failureReason;
    public SkillCastContext context => preparation.context;
    public SkillTargetModeDecision targetModeDecision => context != null ? context.targetModeDecision : default(SkillTargetModeDecision);
    public SkillTargetMode targetMode => context != null ? context.targetMode : SkillTargetMode.Auto;
    public SkillTargetModeSource targetModeSource => context != null ? context.targetModeSource : SkillTargetModeSource.DefaultFallback;
    public SkillTargetSelectionDecision targetSelectionDecision => context != null ? context.targetSelectionDecision : default(SkillTargetSelectionDecision);
    public SkillDirectionDecision directionDecisionData => context != null ? context.directionDecisionData : default(SkillDirectionDecision);
    public SkillTargetSelectionSource targetSource => context != null ? context.targetSource : SkillTargetSelectionSource.None;
    public SkillDirectionSource directionSource => context != null ? context.directionSource : SkillDirectionSource.None;
    public SkillTargetGeometryDecision geometryDecision => context != null ? SkillTargetGeometryStrategy.Create(context.caster, targetMode, context.request) : default(SkillTargetGeometryDecision);
    public SkillTargetGeometryKind geometryKind => geometryDecision.geometryKind;
    public SkillTargetGeometrySource geometrySource => geometryDecision.geometrySource;
    public string targetModeFallbackPath => context != null ? context.targetModeFallbackPath : string.Empty;
    public string targetFallbackPath => context != null ? context.targetFallbackPath : string.Empty;
    public string directionFallbackPath => context != null ? context.directionFallbackPath : string.Empty;
    public string geometrySummary => geometryDecision.geometrySummary;
    public string geometryFallbackPath => geometryDecision.geometryFallbackPath;
    public SkillTargetCollectionResult targetCollection => ToSnapshot().targetCollection;
    public SkillTargetFilterResult targetFilterResult => targetCollection != null ? targetCollection.filterResult : null;
    public SkillTargetFilterPresenter targetFilterPresenter => new SkillTargetFilterPresenter(targetFilterResult);
    public int targetFilterAcceptedCount => targetFilterPresenter.acceptedCount;
    public int targetFilterRejectedCount => targetFilterPresenter.rejectedCount;
    public string targetFilterSummary => targetFilterPresenter.Describe();
    public IReadOnlyList<string> targetFilterDetailLines => targetFilterPresenter.GetDetailLines();
    public IReadOnlyList<string> targetFilterTraceLines => targetFilterPresenter.traceLines;
    public SkillTargetFilterPresenter filterPresenter => targetFilterPresenter;
    public int acceptedCount => targetFilterAcceptedCount;
    public int rejectedCount => targetFilterRejectedCount;
    public string policySummary => targetFilterPresenter.policySummary;
    public IReadOnlyList<string> traceLines => targetFilterTraceLines;
    public string fallbackPath => targetFilterPresenter.fallbackPath;
    public IReadOnlyList<string> filterSummaryLines => SkillCastPlanSnapshot.BuildFilterSummaryLines(targetFilterPresenter);
    public IReadOnlyList<string> filterSummary => filterSummaryLines;
    public SkillCastCommitView commitView => commitResult.HasValue ? new SkillCastCommitView(commitResult.Value) : default(SkillCastCommitView);
    public string commitSummary => commitResult.HasValue ? commitResult.Value.Describe() : "commit=pending";
    public IReadOnlyList<string> commitStageSummaries => commitResult.HasValue ? commitResult.Value.GetStageSummaries() : new string[] { "Resource: pending", "Cooldown: pending", "Timeline: pending" };
    public SkillCastPlanPresenter presenter => ToSnapshot().presenter;
    public SkillCastPlanPresenter planPresenter => presenter;

    public SkillCastPlan(SkillCastPreparation preparation, SkillCastCommitResult? commitResult)
    {
        this.preparation = preparation;
        this.commitResult = commitResult;
    }

    public SkillCastPlanSnapshot ToSnapshot()
    {
        return new SkillCastPlanSnapshot(this);
    }

    public SkillCastPreparationView ToView()
    {
        return new SkillCastPreparationView(this);
    }

    public SkillCastPreparationDiagnostic ToDiagnostic()
    {
        return new SkillCastPreparationDiagnostic(this);
    }

    public SkillCastPlanPresenter ToPresenter()
    {
        return presenter;
    }
}

public readonly struct SkillCastPlanSnapshot
{
    private static readonly string[] PendingCommitStageSummaries = new string[]
    {
        "Resource: pending",
        "Cooldown: pending",
        "Timeline: pending"
    };

    public SkillCastPlan plan { get; }
    private readonly SkillTargetGeometryDecision _geometryDecision;
    private readonly SkillCastCommitView _commitView;
    public SkillCastPreparation preparation => plan.preparation;
    public SkillCastCommitResult? commitResult => plan.commitResult;
    public bool hasCommitResult => commitResult.HasValue;
    public SkillCastContext context => preparation.context;
    public SkillTargetResolution targetResolution => preparation.targetResolution;
    public SkillTargetModeDecision targetModeDecision => context != null ? context.targetModeDecision : default(SkillTargetModeDecision);
    public SkillTargetMode targetMode => context != null ? context.targetMode : SkillTargetMode.Auto;
    public SkillTargetModeSource targetModeSource => context != null ? context.targetModeSource : SkillTargetModeSource.DefaultFallback;
    public string targetModeName => targetMode.ToString();
    public IReadOnlyList<SkillTargetMode> targetModeFallbackChain => context != null ? context.targetModeFallbackChain : SkillTargetModeStrategy.DefaultFallbackChain;
    public SkillTargetSelectionDecision targetSelectionDecision => context != null ? context.targetSelectionDecision : default(SkillTargetSelectionDecision);
    public SkillDirectionDecision directionDecisionData => context != null ? context.directionDecisionData : default(SkillDirectionDecision);
    public SkillTargetGeometryDecision geometryDecision => _geometryDecision;
    public SkillTargetGeometryKind geometryKind => geometryDecision.geometryKind;
    public SkillTargetGeometrySource geometrySource => geometryDecision.geometrySource;
    public SkillTargetMode geometryTargetMode => geometryDecision.targetMode;
    public SkillTargetGeometryKind targetGeometryKind => geometryKind;
    public string targetGeometrySource => geometrySource.ToString();
    public string targetGeometrySummary => geometrySummary;
    public string targetGeometryFallbackPath => geometryFallbackPath;
    public SkillTargetCollectionResult targetCollection => SkillTargetResolver.ResolveTargetCollection(preparation.ToSnapshot());
    public SkillTargetFilterResult targetFilterResult => targetCollection != null ? targetCollection.filterResult : null;
    public SkillTargetFilterPresenter targetFilterPresenter => new SkillTargetFilterPresenter(targetFilterResult);
    public int targetFilterAcceptedCount => targetFilterPresenter.acceptedCount;
    public int targetFilterRejectedCount => targetFilterPresenter.rejectedCount;
    public string targetFilterSummary => targetFilterPresenter.Describe();
    public IReadOnlyList<string> targetFilterDetailLines => targetFilterPresenter.GetDetailLines();
    public IReadOnlyList<string> targetFilterTraceLines => targetFilterPresenter.traceLines;
    public SkillTargetFilterPresenter filterPresenter => targetFilterPresenter;
    public int acceptedCount => targetFilterAcceptedCount;
    public int rejectedCount => targetFilterRejectedCount;
    public string policySummary => targetFilterPresenter.policySummary;
    public IReadOnlyList<string> traceLines => targetFilterTraceLines;
    public string fallbackPath => targetFilterPresenter.fallbackPath;
    public IReadOnlyList<string> filterSummaryLines => BuildFilterSummaryLines(targetFilterPresenter);
    public IReadOnlyList<string> filterSummary => filterSummaryLines;
    public Vector3 geometryOrigin => geometryDecision.geometryOrigin;
    public Vector3 geometryDirection => geometryDecision.geometryDirection;
    public Vector3 geometryAnchorPoint => geometryDecision.geometryAnchorPoint;
    public ChaState geometryTargetUnit => geometryDecision.geometryTargetUnit;
    public string geometrySummary => geometryDecision.geometrySummary;
    public string geometryFallbackPath => geometryDecision.geometryFallbackPath;
    public bool hasGeometryTargetUnit => geometryDecision.hasTargetUnit;
    public bool hasGeometryAnchorPoint => geometryDecision.hasAnchorPoint;
    public SkillTargetSelectionSource targetSource => context != null ? context.targetSource : SkillTargetSelectionSource.None;
    public SkillDirectionSource directionSource => context != null ? context.directionSource : SkillDirectionSource.None;
    public IReadOnlyList<SkillTargetSelectionSource> targetFallbackChain => context != null ? context.targetFallbackChain : SkillTargetSelectionStrategy.GetFallbackChain(targetMode);
    public IReadOnlyList<SkillDirectionSource> directionFallbackChain => context != null ? context.directionFallbackChain : SkillDirectionStrategy.GetFallbackChain(targetMode);
    public bool hasModeFallback => targetModeSource != SkillTargetModeSource.SkillModel;
    public bool hasTargetFallback => targetSource != SkillTargetSelectionSource.TargetUnit;
    public bool hasDirectionFallback => directionSource != SkillDirectionSource.ExplicitDirection;
    public bool hasKeyFallback => hasModeFallback || hasTargetFallback || hasDirectionFallback;
    public string keyFallbackSummary => hasKeyFallback
        ? "modeFallback=" + hasModeFallback + ", targetFallback=" + hasTargetFallback + ", directionFallback=" + hasDirectionFallback
        : "none";
    public string skillId => preparation.skillId;
    public bool success => preparation.success;
    public bool canCast => preparation.canCast;
    public SkillCastFailureReason failureReason => preparation.failureReason;
    public string failureMessage => SkillCastFeedbackUtility.GetMessage(failureReason);
    public bool hasTimeline => preparation.hasTimeline;
    public bool hasTarget => preparation.hasTarget;
    public bool hasTargetPosition => preparation.hasTargetPosition;
    public bool hasAimDirection => preparation.hasAimDirection;
    public string targetSummary => preparation.targetSummary;
    public string targetSelectionSummary => context != null ? context.targetSelectionSummary : string.Empty;
    public string directionSummary => context != null ? context.directionSummary : string.Empty;
    public string decisionSummary => context != null ? context.decisionSummary : string.Empty;
    public string targetModeFallbackPath => context != null ? context.targetModeFallbackPath : string.Empty;
    public string targetFallbackPath => context != null ? context.targetFallbackPath : string.Empty;
    public string directionFallbackPath => context != null ? context.directionFallbackPath : string.Empty;
    public string commitSummary => commitResult.HasValue ? commitResult.Value.Describe() : "commit=pending";
    public IReadOnlyList<string> commitStageSummaries => commitResult.HasValue ? commitResult.Value.GetStageSummaries() : PendingCommitStageSummaries;
    public SkillCastCommitView commitView => _commitView;
    public Vector3 casterPosition => context != null ? context.casterPosition : Vector3.zero;
    public Vector3 requestedTargetPosition => context != null ? context.requestedTargetPosition : Vector3.zero;
    public Vector3 requestedAimDirection => context != null ? context.requestedAimDirection : Vector3.zero;
    public Vector3 resolvedTargetPosition => context != null ? context.resolvedTargetPosition : Vector3.zero;
    public Vector3 resolvedAimDirection => context != null ? context.resolvedAimDirection : Vector3.zero;
    public bool hasPayload => context != null && context.hasPayload;
    public string payloadSummary => context != null ? context.payloadSummary : "none";
    public SkillCastPlanPresenter presenter => new SkillCastPlanPresenter(this);
    public SkillCastPlanPresenter planPresenter => presenter;

    public SkillCastPlanSnapshot(SkillCastPlan plan)
    {
        this.plan = plan;
        SkillCastPreparation preparation = plan.preparation;
        SkillCastContext context = preparation.context;
        SkillTargetMode targetMode = context != null ? context.targetMode : SkillTargetMode.Auto;
        this._geometryDecision = context != null
            ? SkillTargetGeometryStrategy.Create(context.caster, targetMode, context.request)
            : default(SkillTargetGeometryDecision);
        this._commitView = plan.commitResult.HasValue ? new SkillCastCommitView(plan.commitResult.Value) : default(SkillCastCommitView);
    }

    public SkillCastPlanSnapshot(SkillCastPreparation preparation)
        : this(new SkillCastPlan(preparation, null))
    {
    }

    public SkillCastPlanSnapshot(SkillCastPreparation preparation, SkillCastCommitResult? commitResult)
        : this(new SkillCastPlan(preparation, commitResult))
    {
    }

    public SkillCastPlanSnapshot(SkillCastResult result)
        : this(result.plan)
    {
    }

    public string Describe()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("skill=").Append(skillId);
        builder.Append(", canCast=").Append(canCast);
        builder.Append(", failureReason=").Append(failureReason);
        builder.Append(", failureMessage=").Append(failureMessage);
        builder.Append(", hasTimeline=").Append(hasTimeline);
        builder.Append(", targetMode=").Append(targetModeName);
        builder.Append(", targetModeSource=").Append(targetModeSource);
        builder.Append(", geometryKind=").Append(geometryKind);
        builder.Append(", geometrySource=").Append(geometrySource);
        builder.Append(", geometry=").Append(geometrySummary);
        builder.Append(", targetSelection=").Append(targetSelectionDecision.Describe());
        builder.Append(", directionDecision=").Append(directionDecisionData.Describe());
        builder.Append(", targetModeFallbackPath=").Append(targetModeFallbackPath);
        builder.Append(", targetFallbackPath=").Append(targetFallbackPath);
        builder.Append(", directionFallbackPath=").Append(directionFallbackPath);
        builder.Append(", keyFallback=").Append(keyFallbackSummary);
        builder.Append(", commit=").Append(commitSummary);
        return builder.ToString();
    }

    public IReadOnlyList<string> GetDetailLines()
    {
        List<string> lines = new List<string>();
        lines.Add("skill=" + skillId);
        lines.Add("canCast=" + canCast);
        lines.Add("failureReason=" + failureReason);
        lines.Add("failureMessage=" + failureMessage);
        lines.Add("hasTimeline=" + hasTimeline);
        lines.Add("targetMode=" + targetModeName);
        lines.Add("targetModeSource=" + targetModeSource);
        lines.Add("targetSource=" + targetSource);
        lines.Add("directionSource=" + directionSource);
        lines.Add("geometryKind=" + geometryKind);
        lines.Add("geometrySource=" + geometrySource);
        lines.Add("geometrySummary=" + geometrySummary);
        lines.Add("geometryFallbackPath=" + geometryFallbackPath);
        lines.Add("targetModeDecision=" + targetModeDecision.Describe());
        lines.Add("targetSelectionDecision=" + targetSelectionDecision.Describe());
        lines.Add("directionDecisionData=" + directionDecisionData.Describe());
        lines.Add("targetModeFallbackPath=" + targetModeFallbackPath);
        lines.Add("targetDecisionText=" + targetSelectionSummary);
        lines.Add("directionDecisionText=" + directionSummary);
        lines.Add("decision=" + decisionSummary);
        lines.Add("targetModeFallbackChain=" + BuildChainSummary(targetModeFallbackChain));
        lines.Add("targetFallbackPath=" + targetFallbackPath);
        lines.Add("directionFallbackPath=" + directionFallbackPath);
        lines.Add("hasModeFallback=" + hasModeFallback);
        lines.Add("hasTargetFallback=" + hasTargetFallback);
        lines.Add("hasDirectionFallback=" + hasDirectionFallback);
        lines.Add("hasKeyFallback=" + hasKeyFallback);
        lines.Add("keyFallbackSummary=" + keyFallbackSummary);
        lines.Add("targetFallbackChain=" + BuildChainSummary(targetFallbackChain));
        lines.Add("directionFallbackChain=" + BuildChainSummary(directionFallbackChain));
        lines.Add("commitSummary=" + commitSummary);
        lines.Add("commitStageSummaries=" + BuildChainSummary(commitStageSummaries));
        lines.Add("filterSummary:");
        lines.AddRange(filterSummaryLines);
        lines.Add("casterPosition=" + casterPosition);
        lines.Add("requestedTargetPosition=" + requestedTargetPosition);
        lines.Add("requestedAimDirection=" + requestedAimDirection);
        lines.Add("resolvedTargetPosition=" + resolvedTargetPosition);
        lines.Add("resolvedAimDirection=" + resolvedAimDirection);
        lines.Add("payload=" + payloadSummary);
        return lines;
    }

    public static IReadOnlyList<string> BuildFilterSummaryLines(SkillTargetFilterPresenter presenter)
    {
        if (presenter.hasFilterResult == false)
        {
            return new string[] { "filter=unavailable" };
        }

        List<string> lines = new List<string>();
        lines.Add("policySummary=" + presenter.policySummary);
        lines.Add("acceptedCount=" + presenter.acceptedCount);
        lines.Add("rejectedCount=" + presenter.rejectedCount);
        lines.Add("fallbackPath=" + presenter.fallbackPath);
        lines.Add("traceLines:");
        IReadOnlyList<string> traceLines = presenter.traceLines;
        if (traceLines != null)
        {
            for (int i = 0; i < traceLines.Count; i++)
            {
                lines.Add("  " + traceLines[i]);
            }
        }

        return lines;
    }

    private static string BuildChainSummary<T>(IReadOnlyList<T> chain)
    {
        if (chain == null || chain.Count <= 0)
        {
            return string.Empty;
        }

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < chain.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(" -> ");
            }

            builder.Append(chain[i]);
        }

        return builder.ToString();
    }
}

public readonly struct SkillCastPlanPresenter
{
    public SkillCastPlanSnapshot snapshot { get; }
    public SkillCastPlan plan => snapshot.plan;
    public SkillCastPreparation preparation => snapshot.preparation;
    public SkillCastContext context => snapshot.context;
    public SkillTargetMode targetMode => snapshot.targetMode;
    public SkillTargetGeometryKind geometryKind => snapshot.geometryKind;
    public SkillTargetFilterPresenter filterPresenter => snapshot.filterPresenter;
    public SkillCastCommitView commitView => snapshot.commitView;
    public string commitSummary => snapshot.commitSummary;
    public IReadOnlyList<string> stageSummaries => snapshot.commitStageSummaries;
    public IReadOnlyList<string> commitStageSummaries => snapshot.commitStageSummaries;
    public IReadOnlyList<string> filterSummaryLines => snapshot.filterSummaryLines;
    public IReadOnlyList<string> filterSummary => filterSummaryLines;
    public int acceptedCount => snapshot.acceptedCount;
    public int rejectedCount => snapshot.rejectedCount;
    public string policySummary => snapshot.policySummary;
    public IReadOnlyList<string> traceLines => snapshot.traceLines;
    public string fallbackPath => snapshot.fallbackPath;
    public string Describe() => snapshot.Describe();
    public IReadOnlyList<string> GetDetailLines() => snapshot.GetDetailLines();

    public SkillCastPlanPresenter(SkillCastPlanSnapshot snapshot)
    {
        this.snapshot = snapshot;
    }

    public static SkillCastPlanPresenter From(SkillCastPlanSnapshot snapshot)
    {
        return new SkillCastPlanPresenter(snapshot);
    }
}
