using System.Collections.Generic;

public static class SkillCastCommitResultExtensions
{
    public static IReadOnlyList<string> GetStageSummaries(this SkillCastCommitResult commitResult)
    {
        return commitResult.snapshot.GetStageSummaries();
    }

    public static SkillCastCommitPresenter ToPresenter(this SkillCastCommitResult commitResult)
    {
        return new SkillCastCommitPresenter(commitResult);
    }

    public static SkillCastCommitPresenter ToPresenter(this SkillCastCommitSnapshot commitSnapshot)
    {
        return new SkillCastCommitPresenter(commitSnapshot);
    }
}
