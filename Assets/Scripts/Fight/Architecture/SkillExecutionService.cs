using System.Collections.Generic;
using UnityEngine;

public enum SkillCastFailureReason
{
    None,
    CasterNull,
    SkillLocked,
    SkillNotFound,
    SkillModelMissing,
    SkillEffectMissing,
    CasterResourceMissing,
    CasterBuffsMissing,
    SkillCoolingDown,
    ResourceNotEnough,
    TimelineRejected
}

public struct SkillCastResult
{
    public bool success;
    public SkillCastFailureReason failureReason;
    public SkillCastPreparation preparation;
    public SkillCastPlan plan;
    public SkillObj skill;
    public SkillCastContext context;
    public TimelineObj timeline;
    public SkillCastCommitResult? commitResult;

    public bool hasCommitResult => commitResult.HasValue;

    public static SkillCastResult Success(SkillCastPlan plan, SkillObj skill, SkillCastContext context, TimelineObj timeline, SkillCastCommitResult? commitResult = null)
    {
        SkillCastPreparation preparation = plan.preparation;
        return new SkillCastResult
        {
            success = true,
            failureReason = SkillCastFailureReason.None,
            preparation = preparation,
            plan = new SkillCastPlan(preparation, commitResult),
            skill = skill,
            context = context,
            timeline = timeline,
            commitResult = commitResult
        };
    }

    public static SkillCastResult Fail(SkillCastPlan plan, SkillCastFailureReason reason, TimelineObj timeline = default(TimelineObj), SkillCastCommitResult? commitResult = null)
    {
        SkillCastPreparation preparation = plan.preparation;
        return new SkillCastResult
        {
            success = false,
            failureReason = reason,
            preparation = preparation,
            plan = new SkillCastPlan(preparation, commitResult),
            timeline = timeline,
            commitResult = commitResult
        };
    }

    public static SkillCastResult Success(SkillObj skill, SkillCastContext context, TimelineObj timeline, SkillCastCommitResult? commitResult = null)
    {
        SkillCastPreparation preparation = SkillCastPreparation.Success(skill, context);
        return new SkillCastResult
        {
            success = true,
            failureReason = SkillCastFailureReason.None,
            preparation = preparation,
            plan = new SkillCastPlan(preparation, commitResult),
            skill = skill,
            context = context,
            timeline = timeline,
            commitResult = commitResult
        };
    }

    public static SkillCastResult Fail(SkillCastFailureReason reason, TimelineObj timeline = default(TimelineObj), SkillCastCommitResult? commitResult = null)
    {
        SkillCastPreparation preparation = SkillCastPreparation.Fail(reason);
        return new SkillCastResult
        {
            success = false,
            failureReason = reason,
            preparation = preparation,
            plan = new SkillCastPlan(preparation, commitResult),
            timeline = timeline,
            commitResult = commitResult
        };
    }
}

public readonly struct SkillCastPreparation
{
    public bool success { get; }
    public SkillCastFailureReason failureReason { get; }
    public SkillObj skill { get; }
    public SkillCastContext context { get; }

    public SkillTargetResolution targetResolution => context != null ? context.targetResolution : default(SkillTargetResolution);
    public SkillTargetSelectionDecision targetSelectionDecision => context != null ? context.targetSelectionDecision : default(SkillTargetSelectionDecision);
    public SkillDirectionDecision directionDecision => context != null ? context.directionDecisionData : default(SkillDirectionDecision);
    public IReadOnlyList<SkillTargetSelectionSource> targetFallbackChain => context != null ? context.targetFallbackChain : SkillTargetSelectionStrategy.DefaultFallbackChain;
    public IReadOnlyList<SkillDirectionSource> directionFallbackChain => context != null ? context.directionFallbackChain : SkillDirectionStrategy.DefaultFallbackChain;
    public string skillId => context != null ? context.request.skillId : string.Empty;
    public bool canCast => success;
    public bool hasTarget => context != null && context.hasTarget;
    public bool hasTargetPosition => context != null && context.request.hasTargetPosition;
    public bool hasAimDirection => context != null && context.request.hasAimDirection;
    public bool hasTimeline => context != null && context.hasSkillEffect;
    public string targetSummary => context != null ? SkillTargetResolver.Describe(context.targetResolution) : string.Empty;

    public SkillCastPreparation(
        bool success,
        SkillCastFailureReason failureReason,
        SkillObj skill,
        SkillCastContext context
    )
    {
        this.success = success;
        this.failureReason = failureReason;
        this.skill = skill;
        this.context = context;
    }

    public static SkillCastPreparation Success(SkillObj skill, SkillCastContext context)
    {
        return new SkillCastPreparation(true, SkillCastFailureReason.None, skill, context);
    }

    public static SkillCastPreparation Fail(SkillCastFailureReason reason)
    {
        return new SkillCastPreparation(false, reason, null, null);
    }

    public SkillCastPreparationView ToView()
    {
        return new SkillCastPreparationView(this);
    }

    public SkillCastPlan ToPlan()
    {
        return new SkillCastPlan(this, null);
    }

    public SkillCastPreparationSnapshot ToSnapshot()
    {
        return new SkillCastPreparationSnapshot(this);
    }
}

public struct SkillCastRequest
{
    public ChaState caster;
    public string skillId;
    public GameObject target;
    public Vector3 targetPosition;
    public Vector3 aimDirection;
    public bool hasTarget;
    public bool hasTargetPosition;
    public bool hasAimDirection;
    public object payload;

    public SkillCastRequest(ChaState caster, string skillId)
    {
        this.caster = caster;
        this.skillId = skillId;
        this.target = null;
        this.targetPosition = Vector3.zero;
        this.aimDirection = Vector3.zero;
        this.hasTarget = false;
        this.hasTargetPosition = false;
        this.hasAimDirection = false;
        this.payload = null;
    }

    public SkillCastRequest(ChaState caster, string skillId, GameObject target)
        : this(caster, skillId)
    {
        this.target = target;
        this.hasTarget = target != null;
        if (target)
        {
            this.targetPosition = target.transform.position;
            this.hasTargetPosition = true;
        }
    }

    public SkillCastRequest(ChaState caster, string skillId, Vector3 targetPosition)
        : this(caster, skillId)
    {
        this.targetPosition = targetPosition;
        this.hasTargetPosition = true;
    }

    public SkillCastRequest(ChaState caster, string skillId, Vector3 targetPosition, Vector3 aimDirection)
        : this(caster, skillId)
    {
        this.targetPosition = targetPosition;
        this.aimDirection = aimDirection;
        this.hasTargetPosition = true;
        this.hasAimDirection = aimDirection.sqrMagnitude > 0.0001f;
    }

    public SkillCastRequest(
        ChaState caster, string skillId, GameObject target,
        Vector3 targetPosition, Vector3 aimDirection, object payload = null
    ) : this(caster, skillId)
    {
        this.target = target;
        this.targetPosition = targetPosition;
        this.aimDirection = aimDirection;
        this.hasTarget = target != null;
        this.hasTargetPosition = true;
        this.hasAimDirection = aimDirection.sqrMagnitude > 0.0001f;
        this.payload = payload;
    }

    public static SkillCastRequest ForTarget(ChaState caster, string skillId, GameObject target, object payload = null)
    {
        return new SkillCastRequest(caster, skillId, target, target ? target.transform.position : Vector3.zero, Vector3.zero, payload);
    }

    public static SkillCastRequest ForPosition(ChaState caster, string skillId, Vector3 targetPosition, object payload = null)
    {
        return new SkillCastRequest(caster, skillId, null, targetPosition, Vector3.zero, payload);
    }

    public static SkillCastRequest ForDirection(ChaState caster, string skillId, Vector3 aimDirection, object payload = null)
    {
        return new SkillCastRequest(caster, skillId, null, Vector3.zero, aimDirection, payload);
    }
}

public static class SkillExecutionService
{
    public static SkillCastPlan PreparePlan(ChaState caster, string skillId)
    {
        return PreparePlan(new SkillCastRequest(caster, skillId));
    }

    public static SkillCastPlan PreparePlan(SkillCastRequest request)
    {
        return PrepareCast(request).ToPlan();
    }

    public static SkillCastPreparation PrepareCast(ChaState caster, string skillId)
    {
        return PrepareCast(new SkillCastRequest(caster, skillId));
    }

    public static SkillCastPreparation PrepareCast(SkillCastRequest request)
    {
        SkillCastFailureReason failureReason = ValidateCaster(request.caster);
        if (failureReason != SkillCastFailureReason.None)
        {
            return SkillCastPreparation.Fail(failureReason);
        }

        failureReason = ValidateCasterState(request.caster);
        if (failureReason != SkillCastFailureReason.None)
        {
            return SkillCastPreparation.Fail(failureReason);
        }

        SkillObj skillObj = ResolveSkill(request.caster, request.skillId, out failureReason);
        if (failureReason != SkillCastFailureReason.None)
        {
            return SkillCastPreparation.Fail(failureReason);
        }

        failureReason = ValidateSkillDefinition(skillObj);
        if (failureReason != SkillCastFailureReason.None)
        {
            return SkillCastPreparation.Fail(failureReason);
        }

        failureReason = ValidateSkillState(skillObj);
        if (failureReason != SkillCastFailureReason.None)
        {
            return SkillCastPreparation.Fail(failureReason);
        }

        failureReason = ValidateResourceRequirement(request.caster, skillObj);
        if (failureReason != SkillCastFailureReason.None)
        {
            return SkillCastPreparation.Fail(failureReason);
        }

        SkillCastContext context = BuildContext(request.caster, skillObj, request);
        return SkillCastPreparation.Success(skillObj, context);
    }

    public static bool TryPrepareCast(ChaState caster, string skillId, out SkillCastPreparation preparation)
    {
        preparation = PrepareCast(caster, skillId);
        return preparation.success;
    }

    public static bool TryPrepareCast(SkillCastRequest request, out SkillCastPreparation preparation)
    {
        preparation = PrepareCast(request);
        return preparation.success;
    }

    public static SkillCastPreparationDiagnostic DescribePreparation(ChaState caster, string skillId)
    {
        return new SkillCastPreparationDiagnostic(PreparePlan(caster, skillId));
    }

    public static SkillCastPreparationDiagnostic DescribePreparation(SkillCastRequest request)
    {
        return new SkillCastPreparationDiagnostic(PreparePlan(request));
    }

    public static SkillCastPreparationDiagnostic DescribePlan(ChaState caster, string skillId)
    {
        return new SkillCastPreparationDiagnostic(PreparePlan(caster, skillId));
    }

    public static SkillCastPreparationDiagnostic DescribePlan(SkillCastRequest request)
    {
        return new SkillCastPreparationDiagnostic(PreparePlan(request));
    }

    public static SkillCastPreparationDiagnostic DescribeCast(SkillCastResult result)
    {
        return new SkillCastPreparationDiagnostic(result);
    }

    public static SkillCastResult TryCast(ChaState caster, string skillId)
    {
        return TryCast(PreparePlan(caster, skillId));
    }

    public static SkillCastResult TryCast(SkillCastRequest request)
    {
        return TryCast(PreparePlan(request));
    }

    public static SkillCastResult TryCast(SkillCastPlan plan)
    {
        if (plan.preparation.success == false)
        {
            LogFailure(plan.preparation.skillId, plan.failureReason);
            return SkillCastResult.Fail(plan, plan.failureReason);
        }

        return ExecuteCast(plan);
    }

    private static SkillCastFailureReason ValidateCaster(ChaState caster)
    {
        if (caster == null)
        {
            return SkillCastFailureReason.CasterNull;
        }

        if (caster.controlState.canUseSkill == false)
        {
            return SkillCastFailureReason.SkillLocked;
        }

        return SkillCastFailureReason.None;
    }

    private static SkillCastFailureReason ValidateCasterState(ChaState caster)
    {
        if (caster == null)
        {
            return SkillCastFailureReason.CasterNull;
        }

        if (caster.resource == null)
        {
            return SkillCastFailureReason.CasterResourceMissing;
        }

        if (caster.buffs == null)
        {
            return SkillCastFailureReason.CasterBuffsMissing;
        }

        return SkillCastFailureReason.None;
    }

    private static SkillObj ResolveSkill(ChaState caster, string skillId, out SkillCastFailureReason failureReason)
    {
        failureReason = SkillCastFailureReason.None;
        SkillObj skillObj = caster.GetSkillById(skillId);
        if (skillObj == null)
        {
            failureReason = SkillCastFailureReason.SkillNotFound;
            return null;
        }

        return skillObj;
    }

    private static SkillCastFailureReason ValidateSkillDefinition(SkillObj skillObj)
    {
        if (skillObj == null)
        {
            return SkillCastFailureReason.SkillModelMissing;
        }

        if (EqualityComparer<SkillModel>.Default.Equals(skillObj.model, default(SkillModel)))
        {
            return SkillCastFailureReason.SkillModelMissing;
        }

        if (EqualityComparer<TimelineModel>.Default.Equals(skillObj.model.effect, default(TimelineModel)))
        {
            return SkillCastFailureReason.SkillEffectMissing;
        }

        return SkillCastFailureReason.None;
    }

    private static SkillCastFailureReason ValidateSkillState(SkillObj skillObj)
    {
        if (skillObj == null)
        {
            return SkillCastFailureReason.SkillModelMissing;
        }

        if (skillObj.cooldown > 0)
        {
            return SkillCastFailureReason.SkillCoolingDown;
        }

        return SkillCastFailureReason.None;
    }

    private static SkillCastFailureReason ValidateResourceRequirement(ChaState caster, SkillObj skillObj)
    {
        if (caster == null || caster.resource == null)
        {
            return SkillCastFailureReason.CasterResourceMissing;
        }

        if (skillObj == null || EqualityComparer<SkillModel>.Default.Equals(skillObj.model, default(SkillModel)))
        {
            return SkillCastFailureReason.SkillModelMissing;
        }

        if (caster.resource.Enough(skillObj.model.condition) == false)
        {
            return SkillCastFailureReason.ResourceNotEnough;
        }

        return SkillCastFailureReason.None;
    }

    private static SkillCastContext BuildContext(ChaState caster, SkillObj skillObj, SkillCastRequest request)
    {
        return new SkillCastContext(caster, skillObj, request);
    }

    private static SkillCastResult ExecuteCast(SkillCastPreparation preparation)
    {
        return ExecuteCast(preparation.ToPlan());
    }

    private static SkillCastResult ExecuteCast(SkillCastPlan plan)
    {
        SkillCastPreparation preparation = plan.preparation;
        SkillObj skillObj = preparation.skill;
        SkillCastContext context = preparation.context;
        if (preparation.success == false || skillObj == null || context == null || context.caster == null)
        {
            return SkillCastResult.Fail(plan, preparation.failureReason == SkillCastFailureReason.None ? SkillCastFailureReason.TimelineRejected : preparation.failureReason);
        }

        ChaState caster = context.caster;

        TimelineObj timeline = BuildTimeline(context);
        if (IsDefaultTimeline(timeline))
        {
            return SkillCastResult.Fail(SkillCastFailureReason.TimelineRejected);
        }

        SkillCastExecutionContext executionContext = BuildExecutionContext(caster, skillObj, context, timeline);
        SkillCastCommitResult commitResult = SkillCastCommitService.Commit(executionContext);
        if (commitResult.success == false)
        {
            return SkillCastResult.Fail(plan, commitResult.failureReason, commitResult.timeline, commitResult);
        }

        return SkillCastResult.Success(plan, skillObj, context, commitResult.timeline, commitResult);
    }

    private static TimelineObj BuildTimeline(SkillCastContext context)
    {
        if (context == null || context.skill == null || context.hasSkillModel == false || context.hasSkillEffect == false)
        {
            return default(TimelineObj);
        }

        TimelineObj timeline = CreateBaseTimeline(context);
        if (IsDefaultTimeline(timeline))
        {
            return default(TimelineObj);
        }

        return ApplyCastModifiers(context, timeline);
    }

    private static TimelineObj CreateBaseTimeline(SkillCastContext context)
    {
        if (context == null || context.skill == null || EqualityComparer<SkillModel>.Default.Equals(context.skill.model, default(SkillModel)) || context.hasSkillEffect == false)
        {
            return default(TimelineObj);
        }

        return new TimelineObj(
            context.skill.model.effect, context.casterObject, context
        );
    }

    private static TimelineObj ApplyCastModifiers(SkillCastContext context, TimelineObj timeline)
    {
        return SkillCastTimelineModifier.ApplyOnCastModifiers(context, timeline);
    }

    private static bool IsDefaultTimeline(TimelineObj timeline)
    {
        return EqualityComparer<TimelineObj>.Default.Equals(timeline, default(TimelineObj));
    }

    private static SkillCastExecutionContext BuildExecutionContext(ChaState caster, SkillObj skillObj, SkillCastContext context, TimelineObj timeline)
    {
        return new SkillCastExecutionContext(caster, skillObj, context, timeline);
    }

    private static void LogFailure(string skillId, SkillCastFailureReason failureReason)
    {
        Debug.LogWarning("[SkillCast] " + skillId + " -> " + SkillCastFeedbackUtility.GetMessage(failureReason));
    }
}
