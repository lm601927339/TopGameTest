using System.Collections.Generic;

public readonly struct SkillTargetFilterPresenter
{
    public SkillTargetFilterResult filterResult { get; }
    public bool hasFilterResult => filterResult != null;
    public int acceptedCount => filterResult != null ? filterResult.accepted.Count : 0;
    public int rejectedCount => filterResult != null ? filterResult.rejected.Count : 0;
    public string policySummary => filterResult != null ? filterResult.policy.summary : string.Empty;
    public string fallbackPath => filterResult != null ? filterResult.fallbackPath : string.Empty;
    public IReadOnlyList<string> traceLines => GetTraceLines();

    public SkillTargetFilterPresenter(SkillTargetFilterResult filterResult)
    {
        this.filterResult = filterResult;
    }

    public static SkillTargetFilterPresenter From(SkillTargetFilterResult filterResult)
    {
        return new SkillTargetFilterPresenter(filterResult);
    }

    public string Describe()
    {
        if (hasFilterResult == false)
        {
            return "filter=unavailable";
        }

        return "accepted=" + acceptedCount + ", rejected=" + rejectedCount + ", policySummary=" + policySummary + ", fallbackPath=" + fallbackPath;
    }

    public IReadOnlyList<string> GetDetailLines()
    {
        List<string> lines = new List<string>();
        if (hasFilterResult == false)
        {
            lines.Add("filter=unavailable");
            return lines;
        }

        lines.Add("acceptedCount=" + acceptedCount);
        lines.Add("rejectedCount=" + rejectedCount);
        lines.Add("policySummary=" + policySummary);
        lines.Add("fallbackPath=" + fallbackPath);
        lines.AddRange(GetTraceLines());
        return lines;
    }

    public IReadOnlyList<string> GetTraceLines()
    {
        if (hasFilterResult == false)
        {
            return new string[] { "filter=unavailable" };
        }

        return filterResult.GetDetailLines();
    }
}
