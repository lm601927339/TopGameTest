using System.Collections.Generic;

public readonly struct SkillCastCommitStageSnapshot
{
    public SkillCastCommitStage stage { get; }
    public bool executed { get; }
    public bool success { get; }
    public SkillCastFailureReason failureReason { get; }
    public string detail { get; }

    public bool skipped => executed == false;
    public bool failed => executed && success == false;
    public bool passed => executed && success;

    public string statusText
    {
        get
        {
            if (skipped)
            {
                return "skipped";
            }

            if (passed)
            {
                return "success";
            }

            return "failed";
        }
    }

    public SkillCastCommitStageSnapshot(
        SkillCastCommitStage stage,
        bool executed,
        bool success,
        SkillCastFailureReason failureReason,
        string detail
    )
    {
        this.stage = stage;
        this.executed = executed;
        this.success = success;
        this.failureReason = failureReason;
        this.detail = detail;
    }

    public SkillCastCommitStageSnapshot(SkillCastCommitStageResult stageResult)
        : this(stageResult.stage, stageResult.executed, stageResult.success, stageResult.failureReason, stageResult.detail)
    {
    }

    public string Describe()
    {
        if (skipped)
        {
            return stage + ": skipped";
        }

        if (passed)
        {
            return stage + ": success";
        }

        return stage + ": failed (" + SkillCastFeedbackUtility.GetMessage(failureReason) + ")";
    }
}

public readonly struct SkillCastCommitSnapshot
{
    public bool success { get; }
    public SkillCastFailureReason failureReason { get; }
    public string failureText => SkillCastFeedbackUtility.GetMessage(failureReason);
    public SkillCastCommitStageSnapshot resource { get; }
    public SkillCastCommitStageSnapshot cooldown { get; }
    public SkillCastCommitStageSnapshot timeline { get; }
    public TimelineObj timelineObj { get; }
    public string timelineIdentity => GetTimelineIdentity(timelineObj);

    public IReadOnlyList<SkillCastCommitStageSnapshot> stages
    {
        get
        {
            return new[]
            {
                resource,
                cooldown,
                timeline
            };
        }
    }

    public bool resourceCommitted => resource.passed;
    public bool cooldownCommitted => cooldown.passed;
    public bool timelineSubmitted => timeline.passed;
    public IReadOnlyList<string> stageSummaries => GetStageSummaries();
    public SkillCastCommitPresenter presenter => new SkillCastCommitPresenter(this);

    public SkillCastCommitSnapshot(SkillCastCommitResult result)
    {
        success = result.success;
        failureReason = result.failureReason;
        resource = new SkillCastCommitStageSnapshot(result.resourceStage);
        cooldown = new SkillCastCommitStageSnapshot(result.cooldownStage);
        timeline = new SkillCastCommitStageSnapshot(result.timelineStage);
        timelineObj = result.timeline;
    }

    public string Describe()
    {
        return "commit=" + (success ? "success" : "failed")
            + ", resource=" + resource.Describe()
            + ", cooldown=" + cooldown.Describe()
            + ", timeline=" + timeline.Describe();
    }

    public IReadOnlyList<string> GetStageSummaries()
    {
        return new[]
        {
            resource.Describe(),
            cooldown.Describe(),
            timeline.Describe()
        };
    }

    private static string GetTimelineIdentity(TimelineObj timelineObj)
    {
        if (timelineObj == null || timelineObj.model.id == null)
        {
            return string.Empty;
        }

        return timelineObj.model.id;
    }
}
