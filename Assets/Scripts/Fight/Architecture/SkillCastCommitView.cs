using System.Collections.Generic;

public readonly struct SkillCastCommitView
{
    private readonly bool _hasValue;
    private readonly SkillCastCommitSnapshot _snapshot;

    public bool hasValue => _hasValue;
    public SkillCastCommitSnapshot snapshot => _snapshot;
    public SkillCastCommitPresenter presenter => _hasValue ? new SkillCastCommitPresenter(_snapshot) : default(SkillCastCommitPresenter);
    public SkillCastCommitDiagnostic diagnostic => _hasValue ? new SkillCastCommitDiagnostic(_snapshot) : default(SkillCastCommitDiagnostic);
    public SkillCastCommitDiagnosticView diagnosticView => _hasValue ? new SkillCastCommitDiagnosticView(_snapshot) : default(SkillCastCommitDiagnosticView);
    public bool success => presenter.success;
    public SkillCastFailureReason failureReason => presenter.failureReason;
    public string failureText => presenter.failureText;
    public string failureMessage => failureText;
    public SkillCastCommitStageSnapshot resource => presenter.resource;
    public SkillCastCommitStageSnapshot cooldown => presenter.cooldown;
    public SkillCastCommitStageSnapshot timeline => presenter.timeline;
    public TimelineObj timelineObj => presenter.timelineObj;
    public string timelineIdentity => presenter.timelineIdentity;
    public string summaryText => presenter.summaryText;
    public IReadOnlyList<SkillCastCommitStageSnapshot> stages => presenter.stages;
    public IReadOnlyList<string> stageSummaries => presenter.stageSummaries;
    public IReadOnlyList<string> detailLines => presenter.detailLines;
    public string resourceFilterReason => diagnosticView.resourceFilterReason;
    public string cooldownFilterReason => diagnosticView.cooldownFilterReason;
    public string timelineFilterReason => diagnosticView.timelineFilterReason;
    public string resourceDetail => diagnosticView.resourceDetail;
    public string cooldownDetail => diagnosticView.cooldownDetail;
    public string timelineDetail => diagnosticView.timelineDetail;

    public SkillCastCommitView(SkillCastCommitSnapshot snapshot)
    {
        _hasValue = true;
        _snapshot = snapshot;
    }

    public SkillCastCommitView(SkillCastCommitResult result)
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
}
