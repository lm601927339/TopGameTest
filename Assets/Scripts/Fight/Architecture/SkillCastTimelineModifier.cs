using System.Collections.Generic;
using UnityEngine;

public static class SkillCastTimelineModifier
{
    public static TimelineObj ApplyOnCastModifiers(SkillCastContext context, TimelineObj timeline)
    {
        if (context == null || timeline == null || context.hasSkillModel == false)
        {
            return timeline;
        }

        if (context.hasCasterBuffs == false)
        {
            return timeline;
        }

        for (int i = 0; i < context.caster.buffs.Count; i++)
        {
            BuffObj buffObj = context.caster.buffs[i];
            if (buffObj == null || EqualityComparer<BuffModel>.Default.Equals(buffObj.model, default(BuffModel)) || buffObj.model.onCast == null)
            {
                continue;
            }

            timeline = buffObj.model.onCast(buffObj, context.skill, timeline);
            if (timeline == null)
            {
                return null;
            }
        }

        return timeline;
    }
}
