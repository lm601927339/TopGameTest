using System.Collections.Generic;

public readonly struct SkillCastCommitDiagnostic
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
    public SkillCastCommitStageSnapshot resourceStage => presenter.resource;
    public SkillCastCommitStageSnapshot cooldownStage => presenter.cooldown;
    public SkillCastCommitStageSnapshot timelineStage => presenter.timeline;
    public IReadOnlyList<SkillCastCommitStageSnapshot> stages => presenter.stages;
    public TimelineObj timelineObj => presenter.timelineObj;
    public string timelineIdentity => presenter.timelineIdentity;
    public string summaryText => presenter.summaryText;
    public IReadOnlyList<string> stageSummaries => presenter.stageSummaries;
    public IReadOnlyList<string> detailLines => presenter.detailLines;

    public SkillCastCommitDiagnostic(SkillCastCommitSnapshot snapshot)
    {
        _hasValue = true;
        _snapshot = snapshot;
    }

    public string Describe()
    {
        return summaryText;
    }

    public IReadOnlyList<string> GetDetailLines()
    {
        return detailLines;
    }
}
