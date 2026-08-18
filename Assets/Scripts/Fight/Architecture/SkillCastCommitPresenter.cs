using System.Collections.Generic;

public readonly struct SkillCastCommitPresenter
{
    private readonly bool _hasValue;
    private readonly SkillCastCommitSnapshot _snapshot;

    public bool hasValue => _hasValue;
    public SkillCastCommitSnapshot snapshot => _snapshot;
    public bool success => _hasValue && _snapshot.success;
    public SkillCastFailureReason failureReason => _hasValue ? _snapshot.failureReason : SkillCastFailureReason.None;
    public string failureText => _hasValue ? _snapshot.failureText : SkillCastFeedbackUtility.GetMessage(failureReason);
    public string failureMessage => failureText;
    public string timelineIdentity => _hasValue ? _snapshot.timelineIdentity : string.Empty;
    public TimelineObj timelineObj => _hasValue ? _snapshot.timelineObj : null;
    public SkillCastCommitStageSnapshot resource => _hasValue ? _snapshot.resource : default(SkillCastCommitStageSnapshot);
    public SkillCastCommitStageSnapshot cooldown => _hasValue ? _snapshot.cooldown : default(SkillCastCommitStageSnapshot);
    public SkillCastCommitStageSnapshot timeline => _hasValue ? _snapshot.timeline : default(SkillCastCommitStageSnapshot);
    public IReadOnlyList<SkillCastCommitStageSnapshot> stages => _hasValue ? _snapshot.stages : System.Array.Empty<SkillCastCommitStageSnapshot>();
    public IReadOnlyList<string> stageSummaries => _hasValue ? _snapshot.stageSummaries : System.Array.Empty<string>();
    public IReadOnlyList<string> detailLines => stageSummaries;
    public string summaryText => Describe();

    public SkillCastCommitPresenter(SkillCastCommitSnapshot snapshot)
    {
        _hasValue = true;
        _snapshot = snapshot;
    }

    public SkillCastCommitPresenter(SkillCastCommitResult result)
        : this(result.snapshot)
    {
    }

    public string Describe()
    {
        if (!_hasValue)
        {
            return string.Empty;
        }

        return _snapshot.Describe();
    }

    public IReadOnlyList<string> GetStageSummaries()
    {
        return stageSummaries;
    }

    public IReadOnlyList<string> GetDetailLines()
    {
        return detailLines;
    }
}
