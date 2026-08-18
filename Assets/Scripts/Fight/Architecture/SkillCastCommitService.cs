using System.Collections.Generic;
using UnityEngine;

public enum SkillCastCommitStage
{
    Resource,
    Cooldown,
    Timeline
}

public readonly struct SkillCastCommitStageResult
{
    public SkillCastCommitStage stage { get; }
    public bool executed { get; }
    public bool success { get; }
    public SkillCastFailureReason failureReason { get; }
    public string detail { get; }

    public bool skipped => executed == false;

    private SkillCastCommitStageResult(
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

    public static SkillCastCommitStageResult Success(SkillCastCommitStage stage, string detail = null)
    {
        return new SkillCastCommitStageResult(stage, true, true, SkillCastFailureReason.None, detail);
    }

    public static SkillCastCommitStageResult Fail(SkillCastCommitStage stage, SkillCastFailureReason failureReason, string detail = null)
    {
        return new SkillCastCommitStageResult(stage, true, false, failureReason, detail);
    }

    public static SkillCastCommitStageResult Skipped(SkillCastCommitStage stage, string detail = null)
    {
        return new SkillCastCommitStageResult(stage, false, false, SkillCastFailureReason.None, detail);
    }

    public string Describe()
    {
        if (skipped)
        {
            return stage + ": skipped";
        }

        if (success)
        {
            return stage + ": success";
        }

        return stage + ": failed (" + SkillCastFeedbackUtility.GetMessage(failureReason) + ")";
    }
}

public readonly struct SkillCastCommitResult
{
    private readonly bool _success;
    private readonly SkillCastFailureReason _failureReason;
    private readonly SkillCastCommitStageResult _resourceStage;
    private readonly SkillCastCommitStageResult _cooldownStage;
    private readonly SkillCastCommitStageResult _timelineStage;
    private readonly TimelineObj _timeline;

    public bool success => _success;
    public SkillCastFailureReason failureReason => _failureReason;
    public SkillCastCommitStageResult resourceStage => _resourceStage;
    public SkillCastCommitStageResult cooldownStage => _cooldownStage;
    public SkillCastCommitStageResult timelineStage => _timelineStage;
    public TimelineObj timeline => _timeline;
    public SkillCastCommitSnapshot snapshot => new SkillCastCommitSnapshot(this);

    public bool resourceCommitted => resourceStage.success;
    public bool cooldownCommitted => cooldownStage.success;
    public bool timelineSubmitted => timelineStage.success;

    public SkillCastCommitResult(
        bool success,
        SkillCastFailureReason failureReason,
        SkillCastCommitStageResult resourceStage,
        SkillCastCommitStageResult cooldownStage,
        SkillCastCommitStageResult timelineStage,
        TimelineObj timeline
    )
    {
        this._success = success;
        this._failureReason = failureReason;
        this._resourceStage = resourceStage;
        this._cooldownStage = cooldownStage;
        this._timelineStage = timelineStage;
        this._timeline = timeline;
    }

    public static SkillCastCommitResult Success(
        SkillCastCommitStageResult resourceStage,
        SkillCastCommitStageResult cooldownStage,
        SkillCastCommitStageResult timelineStage,
        TimelineObj timeline
    )
    {
        return new SkillCastCommitResult(true, SkillCastFailureReason.None, resourceStage, cooldownStage, timelineStage, timeline);
    }

    public static SkillCastCommitResult Fail(
        SkillCastFailureReason failureReason,
        SkillCastCommitStageResult resourceStage,
        SkillCastCommitStageResult cooldownStage,
        SkillCastCommitStageResult timelineStage,
        TimelineObj timeline
    )
    {
        return new SkillCastCommitResult(false, failureReason, resourceStage, cooldownStage, timelineStage, timeline);
    }

    public string Describe()
    {
        return snapshot.Describe();
    }

    public IReadOnlyList<string> GetStageSummaries()
    {
        return snapshot.GetStageSummaries();
    }

    public SkillCastCommitSnapshot ToSnapshot()
    {
        return snapshot;
    }

    public static implicit operator SkillCastCommitSnapshot(SkillCastCommitResult commitResult)
    {
        return commitResult.ToSnapshot();
    }
}

public static class SkillCastCommitService
{
    public static SkillCastCommitResult Commit(ChaState caster, SkillObj skill, TimelineObj timeline)
    {
        SkillCastContext castContext = new SkillCastContext(caster, skill, new SkillCastRequest(caster, string.Empty));
        return Commit(new SkillCastExecutionContext(caster, skill, castContext, timeline));
    }

    public static SkillCastCommitResult Commit(SkillCastExecutionContext executionContext)
    {
        SkillCastFailureReason failureReason = ValidateCommitInput(executionContext);
        if (failureReason != SkillCastFailureReason.None)
        {
            return CreateFailureResult(
                failureReason,
                executionContext.timeline,
                SkillCastCommitStageResult.Skipped(SkillCastCommitStage.Resource),
                SkillCastCommitStageResult.Skipped(SkillCastCommitStage.Cooldown),
                SkillCastCommitStageResult.Skipped(SkillCastCommitStage.Timeline)
            );
        }

        SkillCastCommitStageResult resourceStage = CommitResourceCost(executionContext);
        if (resourceStage.success == false)
        {
            return CreateFailureResult(
                resourceStage.failureReason,
                executionContext.timeline,
                resourceStage,
                SkillCastCommitStageResult.Skipped(SkillCastCommitStage.Cooldown),
                SkillCastCommitStageResult.Skipped(SkillCastCommitStage.Timeline)
            );
        }

        SkillCastCommitStageResult cooldownStage = CommitCooldown(executionContext);
        if (cooldownStage.success == false)
        {
            return CreateFailureResult(
                cooldownStage.failureReason,
                executionContext.timeline,
                resourceStage,
                cooldownStage,
                SkillCastCommitStageResult.Skipped(SkillCastCommitStage.Timeline)
            );
        }

        SkillCastCommitStageResult timelineStage = SubmitTimeline(executionContext);
        if (timelineStage.success == false)
        {
            return CreateFailureResult(
                timelineStage.failureReason,
                executionContext.timeline,
                resourceStage,
                cooldownStage,
                timelineStage
            );
        }

        return SkillCastCommitResult.Success(resourceStage, cooldownStage, timelineStage, executionContext.timeline);
    }

    public static SkillCastCommitStageResult CommitResourceCost(SkillCastExecutionContext executionContext)
    {
        if (HasValidCasterAndSkill(executionContext) == false)
        {
            return SkillCastCommitStageResult.Fail(SkillCastCommitStage.Resource, SkillCastFailureReason.SkillModelMissing);
        }

        if (executionContext.caster.resource == null)
        {
            return SkillCastCommitStageResult.Fail(SkillCastCommitStage.Resource, SkillCastFailureReason.CasterResourceMissing);
        }

        executionContext.caster.ModResource(-1 * executionContext.skill.model.cost);
        return SkillCastCommitStageResult.Success(SkillCastCommitStage.Resource, "resource cost applied");
    }

    public static SkillCastCommitStageResult CommitCooldown(SkillCastExecutionContext executionContext)
    {
        if (HasValidCasterAndSkill(executionContext) == false)
        {
            return SkillCastCommitStageResult.Fail(SkillCastCommitStage.Cooldown, SkillCastFailureReason.SkillModelMissing);
        }

        executionContext.skill.cooldown = 0.1f;
        return SkillCastCommitStageResult.Success(SkillCastCommitStage.Cooldown, "cooldown written back");
    }

    public static SkillCastCommitStageResult SubmitTimeline(SkillCastExecutionContext executionContext)
    {
        if (HasValidTimeline(executionContext) == false)
        {
            return SkillCastCommitStageResult.Fail(SkillCastCommitStage.Timeline, SkillCastFailureReason.TimelineRejected);
        }

        SceneVariants.CreateTimeline(executionContext.timeline);
        return SkillCastCommitStageResult.Success(SkillCastCommitStage.Timeline, "timeline submitted");
    }

    private static SkillCastFailureReason ValidateCommitInput(SkillCastExecutionContext executionContext)
    {
        if (executionContext.hasCaster == false)
        {
            return SkillCastFailureReason.CasterNull;
        }

        if (executionContext.hasSkill == false || EqualityComparer<SkillModel>.Default.Equals(executionContext.skill.model, default(SkillModel)))
        {
            return SkillCastFailureReason.SkillModelMissing;
        }

        if (executionContext.hasCastContext == false)
        {
            return SkillCastFailureReason.TimelineRejected;
        }

        if (executionContext.hasTimeline == false)
        {
            return SkillCastFailureReason.TimelineRejected;
        }

        return SkillCastFailureReason.None;
    }

    private static bool HasValidCasterAndSkill(SkillCastExecutionContext executionContext)
    {
        return executionContext.hasCaster
            && executionContext.hasSkill
            && executionContext.hasCastContext
            && EqualityComparer<SkillModel>.Default.Equals(executionContext.skill.model, default(SkillModel)) == false;
    }

    private static bool HasValidTimeline(SkillCastExecutionContext executionContext)
    {
        return executionContext.hasTimeline;
    }

    private static SkillCastCommitResult CreateFailureResult(
        SkillCastFailureReason failureReason,
        TimelineObj timeline,
        SkillCastCommitStageResult resourceStage,
        SkillCastCommitStageResult cooldownStage,
        SkillCastCommitStageResult timelineStage
    )
    {
        return SkillCastCommitResult.Fail(failureReason, resourceStage, cooldownStage, timelineStage, timeline);
    }
}
