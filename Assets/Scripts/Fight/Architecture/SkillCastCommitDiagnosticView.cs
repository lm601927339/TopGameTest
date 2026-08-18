using System.Collections.Generic;

public readonly struct SkillCastCommitDiagnosticView
{
    private readonly bool _hasValue;
    private readonly SkillCastCommitSnapshot _snapshot;

    public bool hasValue => _hasValue;
    public SkillCastCommitSnapshot snapshot => _snapshot;
    public SkillCastCommitPresenter presenter => _hasValue ? new SkillCastCommitPresenter(_snapshot) : default(SkillCastCommitPresenter);
    public bool success => presenter.success;
    public SkillCastFailureReason failureReason => presenter.failureReason;
    public string failureText => presenter.failureText;
    public string failureMessage => failureText;
    public SkillCastCommitStageSnapshot resource => presenter.resource;
    public SkillCastCommitStageSnapshot cooldown => presenter.cooldown;
    public SkillCastCommitStageSnapshot timeline => presenter.timeline;
    public TimelineObj timelineObj => presenter.timelineObj;
    public string timelineIdentity => presenter.timelineIdentity;
    public IReadOnlyList<SkillCastCommitStageSnapshot> stages => presenter.stages;

    public string summaryText => presenter.summaryText;
    public IReadOnlyList<string> stageSummaries => presenter.stageSummaries;
    public IReadOnlyList<string> detailLines => presenter.detailLines;

    public string resourceFilterReason => GetStageFilterReason(resource);
    public string cooldownFilterReason => GetStageFilterReason(cooldown);
    public string timelineFilterReason => GetStageFilterReason(timeline);

    public string resourceDetail => GetStageDetail(resource);
    public string cooldownDetail => GetStageDetail(cooldown);
    public string timelineDetail => GetStageDetail(timeline);

    public SkillCastCommitDiagnosticView(SkillCastCommitSnapshot snapshot)
    {
        _hasValue = true;
        _snapshot = snapshot;
    }

    public SkillCastCommitDiagnosticView(SkillCastCommitResult result)
        : this(result.snapshot)
    {
    }

    public string Describe()
    {
        return presenter.Describe();
    }

    public IReadOnlyList<string> GetDetailLines()
    {
        return detailLines;
    }

    private static string GetStageFilterReason(SkillCastCommitStageSnapshot stage)
    {
        if (!stage.executed)
        {
            return string.IsNullOrEmpty(stage.statusText) ? "skipped" : stage.statusText;
        }

        if (!stage.success)
        {
            string reason = stage.failureReason.ToString();
            return string.IsNullOrEmpty(reason) ? "failed" : reason;
        }

        return string.Empty;
    }

    private static string GetStageDetail(SkillCastCommitStageSnapshot stage)
    {
        if (!string.IsNullOrEmpty(stage.detail))
        {
            return stage.detail;
        }

        if (!string.IsNullOrEmpty(stage.statusText))
        {
            return stage.statusText;
        }

        return string.Empty;
    }
}
