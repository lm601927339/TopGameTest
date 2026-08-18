using System.Collections.Generic;

public readonly struct SkillCastExecutionContext
{
    public ChaState caster { get; }
    public SkillObj skill { get; }
    public SkillCastContext castContext { get; }
    public TimelineObj timeline { get; }

    public SkillCastRequest request => castContext != null ? castContext.request : default(SkillCastRequest);
    public SkillTargetResolution targetResolution => castContext != null ? castContext.targetResolution : default(SkillTargetResolution);
    public bool hasTimeline => EqualityComparer<TimelineObj>.Default.Equals(timeline, default(TimelineObj)) == false;
    public bool hasCaster => caster != null;
    public bool hasSkill => skill != null;
    public bool hasCastContext => castContext != null;
    public bool hasResolvedTarget => targetResolution.hasTarget || targetResolution.hasTargetPosition || targetResolution.hasAimDirection;

    public SkillCastExecutionContext(ChaState caster, SkillObj skill, SkillCastContext castContext, TimelineObj timeline)
    {
        this.caster = caster;
        this.skill = skill;
        this.castContext = castContext;
        this.timeline = timeline;
    }
}
