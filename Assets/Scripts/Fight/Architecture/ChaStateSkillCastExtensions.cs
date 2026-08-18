public static class ChaStateSkillCastExtensions
{
    public static SkillCastPreparation PrepareSkillCast(this ChaState caster, string skillId)
    {
        return SkillExecutionService.PrepareCast(caster, skillId);
    }

    public static SkillCastPreparation PrepareSkillCast(this ChaState caster, SkillCastRequest request)
    {
        if (request.caster == null)
        {
            request.caster = caster;
        }

        return SkillExecutionService.PrepareCast(request);
    }

    public static bool TryPrepareSkillCast(this ChaState caster, string skillId, out SkillCastPreparation preparation)
    {
        preparation = PrepareSkillCast(caster, skillId);
        return preparation.success;
    }

    public static bool TryPrepareSkillCast(this ChaState caster, SkillCastRequest request, out SkillCastPreparation preparation)
    {
        preparation = PrepareSkillCast(caster, request);
        return preparation.success;
    }
}
