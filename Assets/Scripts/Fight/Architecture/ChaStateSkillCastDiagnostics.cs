public static class ChaStateSkillCastDiagnostics
{
    public static SkillCastPreparationDiagnostic DescribeSkillCast(this ChaState caster, string skillId)
    {
        return SkillExecutionService.DescribePreparation(caster, skillId);
    }

    public static SkillCastPreparationDiagnostic DescribeSkillCast(this ChaState caster, SkillCastRequest request)
    {
        if (request.caster == null)
        {
            request.caster = caster;
        }

        return SkillExecutionService.DescribePreparation(request);
    }

    public static SkillCastPreparationDiagnostic DescribeSkillCast(this ChaState caster, SkillCastPlan plan)
    {
        return new SkillCastPreparationDiagnostic(plan);
    }
}
