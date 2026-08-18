public static class SkillCastFeedbackUtility
{
    public static string GetMessage(SkillCastFailureReason reason)
    {
        switch (reason)
        {
            case SkillCastFailureReason.CasterNull:
                return "Cast failed: caster is missing.";
            case SkillCastFailureReason.SkillLocked:
                return "Cast failed: skill usage is currently locked.";
            case SkillCastFailureReason.SkillNotFound:
                return "Cast failed: skill was not found.";
            case SkillCastFailureReason.SkillModelMissing:
                return "Cast failed: skill model is missing.";
            case SkillCastFailureReason.SkillEffectMissing:
                return "Cast failed: skill effect is missing.";
            case SkillCastFailureReason.CasterResourceMissing:
                return "Cast failed: caster resource container is missing.";
            case SkillCastFailureReason.CasterBuffsMissing:
                return "Cast failed: caster buff list is missing.";
            case SkillCastFailureReason.SkillCoolingDown:
                return "Cast failed: skill is cooling down.";
            case SkillCastFailureReason.ResourceNotEnough:
                return "Cast failed: not enough resources.";
            case SkillCastFailureReason.TimelineRejected:
                return "Cast failed: timeline construction was rejected.";
            default:
                return "Cast result unknown.";
        }
    }

    public static string Describe(SkillCastResult result)
    {
        if (result.success)
        {
            return "Cast succeeded.";
        }

        return GetMessage(result.failureReason);
    }
}
