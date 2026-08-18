using System;
using System.Collections.Generic;
using System.Text;
using DesignerScripts;
using UnityEngine;

// Phase 2 catalog layer:
// designers can author skills, timelines, buffs, bullets, and aoes here, while runtime still
// keeps the legacy static tables as a safe fallback during migration.
[CreateAssetMenu(menuName = "BuffTopGame/Game Design Catalog", fileName = "GameDesignCatalog")]
public class GameDesignCatalogAsset : ScriptableObject
{
    [Header("Skills")]
    public SkillDefinition[] skills = new SkillDefinition[0];

    [Header("Timelines")]
    public TimelineDefinition[] timelines = new TimelineDefinition[0];

    [Header("Buffs")]
    public BuffDefinition[] buffs = new BuffDefinition[0];

    [Header("Bullets")]
    public BulletDefinition[] bullets = new BulletDefinition[0];

    [Header("AoEs")]
    public AoeDefinition[] aoes = new AoeDefinition[0];

    [Header("Main Chain Migration")]
    public MainChainMigrationEntry[] mainChainMigrationEntries = new MainChainMigrationEntry[0];

    [Header("Main Chain Migration Schema")]
    public MainChainMigrationSchemaRow[] mainChainMigrationSchema = new MainChainMigrationSchemaRow[0];

    private Dictionary<string, SkillDefinition> skillDefinitionMap;
    private Dictionary<string, TimelineDefinition> timelineDefinitionMap;
    private Dictionary<string, BuffDefinition> buffDefinitionMap;
    private Dictionary<string, BulletDefinition> bulletDefinitionMap;
    private Dictionary<string, AoeDefinition> aoeDefinitionMap;
    private Dictionary<string, SkillModel> skillCache;
    private Dictionary<string, TimelineModel> timelineCache;
    private Dictionary<string, BuffModel> buffCache;
    private Dictionary<string, BulletModel> bulletCache;
    private Dictionary<string, AoeModel> aoeCache;

    private void OnEnable()
    {
        ResetCache();
    }

    public void ResetCache()
    {
        skillDefinitionMap = null;
        timelineDefinitionMap = null;
        buffDefinitionMap = null;
        bulletDefinitionMap = null;
        aoeDefinitionMap = null;
        skillCache = null;
        timelineCache = null;
        buffCache = null;
        bulletCache = null;
        aoeCache = null;
    }

    public bool TryGetSkill(string id, out SkillModel value)
    {
        value = default(SkillModel);
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        EnsureCaches();
        if (skillCache.TryGetValue(id, out value))
        {
            return true;
        }

        if (skillDefinitionMap != null && skillDefinitionMap.TryGetValue(id, out SkillDefinition definition))
        {
            if (definition.TryBuild(this, out value))
            {
                skillCache[id] = value;
                return true;
            }

            return false;
        }

        if (DesingerTables.SkillData.data.TryGetValue(id, out value))
        {
            skillCache[id] = value;
            return true;
        }

        return false;
    }

    public bool HasSkill(string id)
    {
        SkillModel value;
        return TryGetSkill(id, out value);
    }

    public bool HasSkillDefinition(string id)
    {
        EnsureCaches();
        return skillDefinitionMap != null && skillDefinitionMap.ContainsKey(id);
    }

    public bool TryGetTimeline(string id, out TimelineModel value)
    {
        value = default(TimelineModel);
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        EnsureCaches();
        if (timelineCache.TryGetValue(id, out value))
        {
            return true;
        }

        if (timelineDefinitionMap != null && timelineDefinitionMap.TryGetValue(id, out TimelineDefinition definition))
        {
            if (definition.TryBuild(this, out value))
            {
                timelineCache[id] = value;
                return true;
            }

            return false;
        }

        if (DesingerTables.TimelineData.data.TryGetValue(id, out value))
        {
            timelineCache[id] = value;
            return true;
        }

        return false;
    }

    public bool HasTimelineDefinition(string id)
    {
        EnsureCaches();
        return timelineDefinitionMap != null && timelineDefinitionMap.ContainsKey(id);
    }

    public bool TryGetBuff(string id, out BuffModel value)
    {
        value = default(BuffModel);
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        EnsureCaches();
        if (buffCache.TryGetValue(id, out value))
        {
            return true;
        }

        if (buffDefinitionMap != null && buffDefinitionMap.TryGetValue(id, out BuffDefinition definition))
        {
            if (definition.TryBuild(this, out value))
            {
                buffCache[id] = value;
                return true;
            }

            return false;
        }

        if (DesingerTables.BuffData.data.TryGetValue(id, out value))
        {
            buffCache[id] = value;
            return true;
        }

        return false;
    }

    public bool HasBuffDefinition(string id)
    {
        EnsureCaches();
        return buffDefinitionMap != null && buffDefinitionMap.ContainsKey(id);
    }

    public bool TryGetBullet(string id, out BulletModel value)
    {
        value = default(BulletModel);
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        EnsureCaches();
        if (bulletCache.TryGetValue(id, out value))
        {
            return true;
        }

        if (bulletDefinitionMap != null && bulletDefinitionMap.TryGetValue(id, out BulletDefinition definition))
        {
            if (definition.TryBuild(this, out value))
            {
                bulletCache[id] = value;
                return true;
            }

            return false;
        }

        if (DesingerTables.BulletData.data.TryGetValue(id, out value))
        {
            bulletCache[id] = value;
            return true;
        }

        return false;
    }

    public bool HasBulletDefinition(string id)
    {
        EnsureCaches();
        return bulletDefinitionMap != null && bulletDefinitionMap.ContainsKey(id);
    }

    public bool TryGetAoE(string id, out AoeModel value)
    {
        value = default(AoeModel);
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        EnsureCaches();
        if (aoeCache.TryGetValue(id, out value))
        {
            return true;
        }

        if (aoeDefinitionMap != null && aoeDefinitionMap.TryGetValue(id, out AoeDefinition definition))
        {
            if (definition.TryBuild(this, out value))
            {
                aoeCache[id] = value;
                return true;
            }

            return false;
        }

        if (DesingerTables.AoEData.data.TryGetValue(id, out value))
        {
            aoeCache[id] = value;
            return true;
        }

        return false;
    }

    public bool HasAoeDefinition(string id)
    {
        EnsureCaches();
        return aoeDefinitionMap != null && aoeDefinitionMap.ContainsKey(id);
    }

    public void AppendValidationWarnings(List<string> warnings)
    {
        EnsureCaches();

        ValidateSkillDefinitions(warnings);
        ValidateTimelineDefinitions(warnings);
        ValidateBuffDefinitions(warnings);
        ValidateBulletDefinitions(warnings);
        ValidateAoeDefinitions(warnings);
        ValidateMainChainMigrationEntries(warnings);
        ValidateMainChainMigrationSchema(warnings);
    }

    internal bool TryResolveBulletModel(string id, out BulletModel value)
    {
        return TryGetBullet(id, out value);
    }

    internal bool TryResolveAoeModel(string id, out AoeModel value)
    {
        return TryGetAoE(id, out value);
    }

    internal bool TryResolveBulletTween(string id, out BulletTween value)
    {
        if (string.IsNullOrEmpty(id))
        {
            value = null;
            return false;
        }

        return BulletScript.bulletTween.TryGetValue(id, out value);
    }

    internal bool TryResolveBulletTargetting(string id, out BulletTargettingFunction value)
    {
        if (string.IsNullOrEmpty(id))
        {
            value = null;
            return false;
        }

        return BulletScript.targettingFunc.TryGetValue(id, out value);
    }

    internal bool TryResolveAoeTween(string id, out AoeTween value)
    {
        if (string.IsNullOrEmpty(id))
        {
            value = null;
            return false;
        }

        return AoEScript.aoeTweenFunc.TryGetValue(id, out value);
    }

    private void EnsureCaches()
    {
        if (skillCache != null)
        {
            return;
        }

        skillCache = new Dictionary<string, SkillModel>();
        timelineCache = new Dictionary<string, TimelineModel>();
        buffCache = new Dictionary<string, BuffModel>();
        bulletCache = new Dictionary<string, BulletModel>();
        aoeCache = new Dictionary<string, AoeModel>();

        skillDefinitionMap = BuildSkillDefinitionMap();
        timelineDefinitionMap = BuildTimelineDefinitionMap();
        buffDefinitionMap = BuildBuffDefinitionMap();
        bulletDefinitionMap = BuildBulletDefinitionMap();
        aoeDefinitionMap = BuildAoeDefinitionMap();
    }

    private Dictionary<string, SkillDefinition> BuildSkillDefinitionMap()
    {
        Dictionary<string, SkillDefinition> map = new Dictionary<string, SkillDefinition>();
        if (skills == null)
        {
            return map;
        }

        for (int i = 0; i < skills.Length; i++)
        {
            if (string.IsNullOrEmpty(skills[i].id))
            {
                continue;
            }

            map[skills[i].id] = skills[i];
        }

        return map;
    }

    private Dictionary<string, TimelineDefinition> BuildTimelineDefinitionMap()
    {
        Dictionary<string, TimelineDefinition> map = new Dictionary<string, TimelineDefinition>();
        if (timelines == null)
        {
            return map;
        }

        for (int i = 0; i < timelines.Length; i++)
        {
            if (string.IsNullOrEmpty(timelines[i].id))
            {
                continue;
            }

            map[timelines[i].id] = timelines[i];
        }

        return map;
    }

    private Dictionary<string, BuffDefinition> BuildBuffDefinitionMap()
    {
        Dictionary<string, BuffDefinition> map = new Dictionary<string, BuffDefinition>();
        if (buffs == null)
        {
            return map;
        }

        for (int i = 0; i < buffs.Length; i++)
        {
            if (string.IsNullOrEmpty(buffs[i].id))
            {
                continue;
            }

            map[buffs[i].id] = buffs[i];
        }

        return map;
    }

    private Dictionary<string, BulletDefinition> BuildBulletDefinitionMap()
    {
        Dictionary<string, BulletDefinition> map = new Dictionary<string, BulletDefinition>();
        if (bullets == null)
        {
            return map;
        }

        for (int i = 0; i < bullets.Length; i++)
        {
            if (string.IsNullOrEmpty(bullets[i].id))
            {
                continue;
            }

            map[bullets[i].id] = bullets[i];
        }

        return map;
    }

    private Dictionary<string, AoeDefinition> BuildAoeDefinitionMap()
    {
        Dictionary<string, AoeDefinition> map = new Dictionary<string, AoeDefinition>();
        if (aoes == null)
        {
            return map;
        }

        for (int i = 0; i < aoes.Length; i++)
        {
            if (string.IsNullOrEmpty(aoes[i].id))
            {
                continue;
            }

            map[aoes[i].id] = aoes[i];
        }

        return map;
    }

    private void ValidateSkillDefinitions(List<string> warnings)
    {
        if (skills == null)
        {
            return;
        }

        for (int i = 0; i < skills.Length; i++)
        {
            SkillDefinition skill = skills[i];
            if (string.IsNullOrEmpty(skill.id))
            {
                warnings.Add("[GameDesignCatalog] Skill entry has an empty id.");
                continue;
            }

            TimelineModel timeline;
            if (!TryGetTimeline(skill.effectTimelineId, out timeline))
            {
                warnings.Add("[GameDesignCatalog] Skill " + skill.id + " references missing timeline: " + skill.effectTimelineId);
            }

            if (skill.buffs == null)
            {
                continue;
            }

            for (int j = 0; j < skill.buffs.Length; j++)
            {
                if (string.IsNullOrEmpty(skill.buffs[j].buffId))
                {
                    warnings.Add("[GameDesignCatalog] Skill " + skill.id + " contains an empty learn-buff id.");
                    continue;
                }

                BuffModel buff;
                if (!TryGetBuff(skill.buffs[j].buffId, out buff))
                {
                    warnings.Add("[GameDesignCatalog] Skill " + skill.id + " references missing learn buff: " + skill.buffs[j].buffId);
                }
            }
        }
    }

    private void ValidateTimelineDefinitions(List<string> warnings)
    {
        if (timelines == null)
        {
            return;
        }

        for (int i = 0; i < timelines.Length; i++)
        {
            TimelineDefinition timeline = timelines[i];
            if (string.IsNullOrEmpty(timeline.id))
            {
                warnings.Add("[GameDesignCatalog] Timeline entry has an empty id.");
                continue;
            }

            if (timeline.nodes == null)
            {
                continue;
            }

            for (int j = 0; j < timeline.nodes.Length; j++)
            {
                ValidateTimelineArgumentArray(timeline.nodes[j].arguments, warnings, "timeline " + timeline.id + " node " + j);

                if (string.IsNullOrEmpty(timeline.nodes[j].eventId))
                {
                    warnings.Add("[GameDesignCatalog] Timeline " + timeline.id + " node " + j + " has an empty event id.");
                    continue;
                }

                TimelineEvent timelineEvent;
                if (!TimelineEventRegistry.TryGet(timeline.nodes[j].eventId, out timelineEvent))
                {
                    warnings.Add("[GameDesignCatalog] Timeline " + timeline.id + " node " + j + " references missing event: " + timeline.nodes[j].eventId);
                }
            }
        }
    }

    private void ValidateBuffDefinitions(List<string> warnings)
    {
        if (buffs == null)
        {
            return;
        }

        for (int i = 0; i < buffs.Length; i++)
        {
            BuffDefinition buff = buffs[i];
            if (string.IsNullOrEmpty(buff.id))
            {
                warnings.Add("[GameDesignCatalog] Buff entry has an empty id.");
                continue;
            }

            ValidateEventCall(buff.id, "onOccur", buff.onOccur, warnings);
            ValidateEventCall(buff.id, "onRemoved", buff.onRemoved, warnings);
            ValidateEventCall(buff.id, "onTick", buff.onTick, warnings);
            ValidateEventCall(buff.id, "onCast", buff.onCast, warnings);
            ValidateEventCall(buff.id, "onHit", buff.onHit, warnings);
            ValidateEventCall(buff.id, "onBeHurt", buff.onBeHurt, warnings);
            ValidateEventCall(buff.id, "onKill", buff.onKill, warnings);
            ValidateEventCall(buff.id, "onBeKilled", buff.onBeKilled, warnings);
        }
    }

    private void ValidateBulletDefinitions(List<string> warnings)
    {
        if (bullets == null)
        {
            return;
        }

        for (int i = 0; i < bullets.Length; i++)
        {
            BulletDefinition bullet = bullets[i];
            if (string.IsNullOrEmpty(bullet.id))
            {
                warnings.Add("[GameDesignCatalog] Bullet entry has an empty id.");
                continue;
            }

            if (!string.IsNullOrEmpty(bullet.prefab) && !GameDatabase.HasPrefab(bullet.prefab))
            {
                warnings.Add("[GameDesignCatalog] Bullet " + bullet.id + " references missing prefab: " + bullet.prefab);
            }

            ValidateBulletEvent(bullet.id, "onCreate", bullet.onCreate, warnings);
            ValidateBulletEvent(bullet.id, "onHit", bullet.onHit, warnings);
            ValidateBulletEvent(bullet.id, "onRemoved", bullet.onRemoved, warnings);
            ValidateTimelineArgumentArray(bullet.onCreateParams, warnings, "bullet " + bullet.id + " onCreate");
            ValidateTimelineArgumentArray(bullet.onHitParams, warnings, "bullet " + bullet.id + " onHit");
            ValidateTimelineArgumentArray(bullet.onRemovedParams, warnings, "bullet " + bullet.id + " onRemoved");
        }
    }

    private void ValidateAoeDefinitions(List<string> warnings)
    {
        if (aoes == null)
        {
            return;
        }

        for (int i = 0; i < aoes.Length; i++)
        {
            AoeDefinition aoe = aoes[i];
            if (string.IsNullOrEmpty(aoe.id))
            {
                warnings.Add("[GameDesignCatalog] AoE entry has an empty id.");
                continue;
            }

            if (!string.IsNullOrEmpty(aoe.prefab) && !GameDatabase.HasPrefab(aoe.prefab))
            {
                warnings.Add("[GameDesignCatalog] AoE " + aoe.id + " references missing prefab: " + aoe.prefab);
            }

            ValidateAoeEvent(aoe.id, "onCreate", aoe.onCreate, warnings);
            ValidateAoeEvent(aoe.id, "onRemoved", aoe.onRemoved, warnings);
            ValidateAoeEvent(aoe.id, "onTick", aoe.onTick, warnings);
            ValidateAoeEvent(aoe.id, "onChaEnter", aoe.onChaEnter, warnings);
            ValidateAoeEvent(aoe.id, "onChaLeave", aoe.onChaLeave, warnings);
            ValidateAoeEvent(aoe.id, "onBulletEnter", aoe.onBulletEnter, warnings);
            ValidateAoeEvent(aoe.id, "onBulletLeave", aoe.onBulletLeave, warnings);
            ValidateTimelineArgumentArray(aoe.onCreateParams, warnings, "aoe " + aoe.id + " onCreate");
            ValidateTimelineArgumentArray(aoe.onRemovedParams, warnings, "aoe " + aoe.id + " onRemoved");
            ValidateTimelineArgumentArray(aoe.onTickParams, warnings, "aoe " + aoe.id + " onTick");
            ValidateTimelineArgumentArray(aoe.onChaEnterParams, warnings, "aoe " + aoe.id + " onChaEnter");
            ValidateTimelineArgumentArray(aoe.onChaLeaveParams, warnings, "aoe " + aoe.id + " onChaLeave");
            ValidateTimelineArgumentArray(aoe.onBulletEnterParams, warnings, "aoe " + aoe.id + " onBulletEnter");
            ValidateTimelineArgumentArray(aoe.onBulletLeaveParams, warnings, "aoe " + aoe.id + " onBulletLeave");
        }
    }

    private void ValidateMainChainMigrationEntries(List<string> warnings)
    {
        if (mainChainMigrationEntries == null)
        {
            return;
        }

        HashSet<string> ids = new HashSet<string>();
        for (int i = 0; i < mainChainMigrationEntries.Length; i++)
        {
            MainChainMigrationEntry entry = mainChainMigrationEntries[i];
            if (string.IsNullOrEmpty(entry.skillId))
            {
                warnings.Add("[GameDesignCatalog] Main-chain migration entry has an empty skill id.");
                continue;
            }

            if (!ids.Add(entry.skillId))
            {
                warnings.Add("[GameDesignCatalog] Main-chain migration entry has a duplicate skill id: " + entry.skillId);
            }

            if (string.IsNullOrEmpty(entry.timelineId))
            {
                warnings.Add("[GameDesignCatalog] Main-chain migration entry " + entry.skillId + " has an empty timeline id.");
            }

            if (entry.cooldown < 0f)
            {
                warnings.Add("[GameDesignCatalog] Main-chain migration entry " + entry.skillId + " has a negative cooldown.");
            }

            ValidateMainChainMigrationIds(warnings, entry.skillId, "bullet", entry.bulletIds, HasBulletDefinition, DesingerTables.BulletData.data);
            ValidateMainChainMigrationIds(warnings, entry.skillId, "aoe", entry.aoeIds, HasAoeDefinition, DesingerTables.AoEData.data);
            ValidateMainChainMigrationIds(warnings, entry.skillId, "buff", entry.buffIds, HasBuffDefinition, DesingerTables.BuffData.data);

            if (entry.dependencyEntrances == null || entry.dependencyEntrances.Length <= 0)
            {
                warnings.Add("[GameDesignCatalog] Main-chain migration entry " + entry.skillId + " has no dependency entrances recorded.");
            }
        }
    }

    private void ValidateMainChainMigrationSchema(List<string> warnings)
    {
        if (mainChainMigrationSchema == null || mainChainMigrationSchema.Length <= 0)
        {
            warnings.Add("[GameDesignCatalog] Main-chain migration schema is missing.");
            return;
        }

        HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < mainChainMigrationSchema.Length; i++)
        {
            MainChainMigrationSchemaRow row = mainChainMigrationSchema[i];
            if (string.IsNullOrEmpty(row.name))
            {
                warnings.Add("[GameDesignCatalog] Main-chain migration schema row " + i + " has an empty name.");
                continue;
            }

            if (!names.Add(row.name))
            {
                warnings.Add("[GameDesignCatalog] Main-chain migration schema contains a duplicate field name: " + row.name);
            }

            if (string.IsNullOrEmpty(row.type))
            {
                warnings.Add("[GameDesignCatalog] Main-chain migration schema row " + row.name + " has an empty type.");
            }

            if (string.IsNullOrEmpty(row.source))
            {
                warnings.Add("[GameDesignCatalog] Main-chain migration schema row " + row.name + " has an empty source.");
            }
        }

        string[] expected = new[] { "skillId", "timelineId", "bulletId", "aoeId", "buffId", "targetMode", "cost", "cooldown", "tags", "notes" };
        for (int i = 0; i < expected.Length; i++)
        {
            if (!names.Contains(expected[i]))
            {
                warnings.Add("[GameDesignCatalog] Main-chain migration schema is missing required field: " + expected[i]);
            }
        }
    }

    private void ValidateMainChainMigrationIds<T>(List<string> warnings, string skillId, string category, string[] ids, Func<string, bool> hasAssetDefinition, Dictionary<string, T> legacySource)
    {
        if (ids == null)
        {
            return;
        }

        for (int i = 0; i < ids.Length; i++)
        {
            string id = ids[i];
            if (string.IsNullOrEmpty(id))
            {
                warnings.Add("[GameDesignCatalog] Main-chain migration entry " + skillId + " contains an empty " + category + " id.");
                continue;
            }

            if (hasAssetDefinition != null && hasAssetDefinition(id))
            {
                continue;
            }

            if (legacySource != null && legacySource.ContainsKey(id))
            {
                continue;
            }

            warnings.Add("[GameDesignCatalog] Main-chain migration entry " + skillId + " references missing " + category + " id: " + id);
        }
    }

    private void ValidateEventCall(string ownerId, string label, DesignEventCall call, List<string> warnings)
    {
        if (string.IsNullOrEmpty(call.eventId))
        {
            return;
        }

        bool hasEvent =
            (label == "onOccur" && BuffScript.onOccurFunc.ContainsKey(call.eventId)) ||
            (label == "onRemoved" && BuffScript.onRemovedFunc.ContainsKey(call.eventId)) ||
            (label == "onTick" && BuffScript.onTickFunc.ContainsKey(call.eventId)) ||
            (label == "onCast" && BuffScript.onCastFunc.ContainsKey(call.eventId)) ||
            (label == "onHit" && BuffScript.onHitFunc.ContainsKey(call.eventId)) ||
            (label == "onBeHurt" && BuffScript.beHurtFunc.ContainsKey(call.eventId)) ||
            (label == "onKill" && BuffScript.onKillFunc.ContainsKey(call.eventId)) ||
            (label == "onBeKilled" && BuffScript.beKilledFunc.ContainsKey(call.eventId));
        if (!hasEvent)
        {
            warnings.Add("[GameDesignCatalog] Buff " + ownerId + " references missing " + label + " event: " + call.eventId);
        }

        ValidateTimelineArgumentArray(call.arguments, warnings, "buff " + ownerId + " " + label);
    }

    private void ValidateBulletEvent(string ownerId, string label, string eventId, List<string> warnings)
    {
        if (string.IsNullOrEmpty(eventId))
        {
            return;
        }

        bool hasEvent = label == "onCreate" ? BulletScript.onCreateFunc.ContainsKey(eventId) :
            label == "onHit" ? BulletScript.onHitFunc.ContainsKey(eventId) :
            BulletScript.onRemovedFunc.ContainsKey(eventId);
        if (!hasEvent)
        {
            warnings.Add("[GameDesignCatalog] Bullet " + ownerId + " references missing " + label + " event: " + eventId);
        }
    }

    private void ValidateAoeEvent(string ownerId, string label, string eventId, List<string> warnings)
    {
        if (string.IsNullOrEmpty(eventId))
        {
            return;
        }

        bool hasEvent =
            (label == "onCreate" && AoEScript.onCreateFunc.ContainsKey(eventId)) ||
            (label == "onRemoved" && AoEScript.onRemovedFunc.ContainsKey(eventId)) ||
            (label == "onTick" && AoEScript.onTickFunc.ContainsKey(eventId)) ||
            (label == "onChaEnter" && AoEScript.onChaEnterFunc.ContainsKey(eventId)) ||
            (label == "onChaLeave" && AoEScript.onChaLeaveFunc.ContainsKey(eventId)) ||
            (label == "onBulletEnter" && AoEScript.onBulletEnterFunc.ContainsKey(eventId)) ||
            (label == "onBulletLeave" && AoEScript.onBulletLeaveFunc.ContainsKey(eventId));
        if (!hasEvent)
        {
            warnings.Add("[GameDesignCatalog] AoE " + ownerId + " references missing " + label + " event: " + eventId);
        }
    }

    private void ValidateTimelineArgumentArray(DesignTimelineArgument[] arguments, List<string> warnings, string context)
    {
        if (arguments == null)
        {
            return;
        }

        for (int i = 0; i < arguments.Length; i++)
        {
            ValidateTimelineArgument(arguments[i], warnings, context + " arg " + i);
        }
    }

    private void ValidateTimelineArgument(DesignTimelineArgument argument, List<string> warnings, string context)
    {
        switch (argument.kind)
        {
            case DesignValueKind.AddBuffInfo:
                if (!string.IsNullOrEmpty(argument.addBuffValue.buffId) && !TryGetBuff(argument.addBuffValue.buffId, out _))
                {
                    warnings.Add("[GameDesignCatalog] " + context + " references missing buff: " + argument.addBuffValue.buffId);
                }
                break;
            case DesignValueKind.BulletLauncher:
                ValidateBulletLauncher(argument.bulletLauncherValue, warnings, context);
                break;
            case DesignValueKind.AoeLauncher:
                ValidateAoeLauncher(argument.aoeLauncherValue, warnings, context);
                break;
            case DesignValueKind.Array:
                ValidateTimelineArgumentArray(argument.arrayValues, warnings, context);
                break;
        }
    }

    private void ValidateBulletLauncher(DesignBulletLauncher launcher, List<string> warnings, string context)
    {
        if (!string.IsNullOrEmpty(launcher.bulletId) && !TryGetBullet(launcher.bulletId, out _))
        {
            warnings.Add("[GameDesignCatalog] " + context + " references missing bullet: " + launcher.bulletId);
        }

        if (!string.IsNullOrEmpty(launcher.tweenId) && !BulletScript.bulletTween.ContainsKey(launcher.tweenId))
        {
            warnings.Add("[GameDesignCatalog] " + context + " references missing bullet tween: " + launcher.tweenId);
        }

        if (!string.IsNullOrEmpty(launcher.targetFuncId) && !BulletScript.targettingFunc.ContainsKey(launcher.targetFuncId))
        {
            warnings.Add("[GameDesignCatalog] " + context + " references missing bullet target func: " + launcher.targetFuncId);
        }

        ValidateKeyValues(launcher.param, warnings, context + " param");
    }

    private void ValidateAoeLauncher(DesignAoeLauncher launcher, List<string> warnings, string context)
    {
        if (!string.IsNullOrEmpty(launcher.aoeId) && !TryGetAoE(launcher.aoeId, out _))
        {
            warnings.Add("[GameDesignCatalog] " + context + " references missing aoe: " + launcher.aoeId);
        }

        if (!string.IsNullOrEmpty(launcher.tweenId) && !AoEScript.aoeTweenFunc.ContainsKey(launcher.tweenId))
        {
            warnings.Add("[GameDesignCatalog] " + context + " references missing aoe tween: " + launcher.tweenId);
        }

        ValidateTimelineArgumentArray(launcher.tweenParam, warnings, context + " tweenParam");
        ValidateKeyValues(launcher.param, warnings, context + " param");
    }

    private void ValidateKeyValues(DesignKeyValue[] values, List<string> warnings, string context)
    {
        if (values == null)
        {
            return;
        }

        for (int i = 0; i < values.Length; i++)
        {
            ValidateTimelineArgument(values[i].value, warnings, context + " key " + values[i].key);
        }
    }
}

[Serializable]
public struct DesignChaResource
{
    public int hp;
    public int ammo;
    public int stamina;

    public ChaResource ToRuntime()
    {
        return new ChaResource(hp, ammo, stamina);
    }
}

[Serializable]
public struct DesignChaProperty
{
    public int moveSpeed;
    public int hp;
    public int ammo;
    public int attack;
    public int actionSpeed;
    public float bodyRadius;
    public float hitRadius;
    public MoveType moveType;

    public ChaProperty ToRuntime()
    {
        return new ChaProperty(moveSpeed, hp, ammo, attack, actionSpeed, bodyRadius, hitRadius, moveType);
    }
}

[Serializable]
public struct DesignDamage
{
    public int bullet;
    public int explosion;
    public int mental;

    public Damage ToRuntime()
    {
        return new Damage(bullet, explosion, mental);
    }
}

[Serializable]
public struct DesignKeyValue
{
    public string key;
    public DesignTimelineArgument value;

    public KeyValuePair<string, object> ToRuntime(GameDesignCatalogAsset catalog)
    {
        return new KeyValuePair<string, object>(key, value.ToRuntime(catalog));
    }
}

[Serializable]
public struct DesignAddBuffInfo
{
    public string buffId;
    public int stack;
    public float duration;
    public bool durationSetTo;
    public bool permanent;
    public DesignKeyValue[] buffParam;

    public AddBuffInfo ToRuntime(GameDesignCatalogAsset catalog)
    {
        BuffModel buffModel;
        if (!catalog.TryGetBuff(buffId, out buffModel))
        {
            Debug.LogError("[GameDesignCatalog] Unknown buff id: " + buffId);
            buffModel = default(BuffModel);
        }

        Dictionary<string, object> param = GameDesignCatalogAssetUtility.BuildDictionary(catalog, buffParam);
        return new AddBuffInfo(buffModel, null, null, stack, duration, durationSetTo, permanent, param);
    }
}

[Serializable]
public struct DesignBulletLauncher
{
    public string bulletId;
    public float speed;
    public float duration;
    public float canHitAfterCreated;
    public string tweenId;
    public string targetFuncId;
    public bool useFireDegreeForever;
    public DesignKeyValue[] param;

    public BulletLauncher ToRuntime(GameDesignCatalogAsset catalog)
    {
        BulletModel model;
        if (!catalog.TryResolveBulletModel(bulletId, out model))
        {
            Debug.LogError("[GameDesignCatalog] Unknown bullet id: " + bulletId);
            model = default(BulletModel);
        }

        BulletTween tween = null;
        if (!catalog.TryResolveBulletTween(tweenId, out tween))
        {
            tween = null;
        }

        BulletTargettingFunction targetFunc = null;
        if (!catalog.TryResolveBulletTargetting(targetFuncId, out targetFunc))
        {
            targetFunc = null;
        }

        Dictionary<string, object> dict = GameDesignCatalogAssetUtility.BuildDictionary(catalog, param);
        return new BulletLauncher(model, null, Vector3.zero, 0, speed, duration, canHitAfterCreated, tween, targetFunc, useFireDegreeForever, dict);
    }
}

[Serializable]
public struct DesignAoeLauncher
{
    public string aoeId;
    public Vector3 position;
    public float radius;
    public float duration;
    public float degree;
    public string tweenId;
    public DesignTimelineArgument[] tweenParam;
    public DesignKeyValue[] param;

    public AoeLauncher ToRuntime(GameDesignCatalogAsset catalog)
    {
        AoeModel model;
        if (!catalog.TryResolveAoeModel(aoeId, out model))
        {
            Debug.LogError("[GameDesignCatalog] Unknown aoe id: " + aoeId);
            model = default(AoeModel);
        }

        AoeTween tween = null;
        if (!catalog.TryResolveAoeTween(tweenId, out tween))
        {
            tween = null;
        }

        object[] tweenArgs = GameDesignCatalogAssetUtility.BuildObjectArray(catalog, tweenParam, DesignValueKind.Null);
        Dictionary<string, object> dict = GameDesignCatalogAssetUtility.BuildDictionary(catalog, param);
        return new AoeLauncher(model, null, position, radius, duration, degree, tween, tweenArgs, dict);
    }
}

[Serializable]
public struct DesignTimelineArgument
{
    public DesignValueKind kind;
    public int intValue;
    public float floatValue;
    public bool boolValue;
    public string stringValue;
    public Vector3 vector3Value;
    public DesignChaResource resourceValue;
    public DesignChaProperty propertyValue;
    public DesignDamage damageValue;
    public DesignAddBuffInfo addBuffValue;
    public DesignBulletLauncher bulletLauncherValue;
    public DesignAoeLauncher aoeLauncherValue;
    public DesignValueKind arrayElementKind;
    public DesignTimelineArgument[] arrayValues;

    public object ToRuntime(GameDesignCatalogAsset catalog)
    {
        switch (kind)
        {
            case DesignValueKind.Int:
                return intValue;
            case DesignValueKind.Float:
                return floatValue;
            case DesignValueKind.Bool:
                return boolValue;
            case DesignValueKind.String:
                return stringValue;
            case DesignValueKind.Vector3:
                return vector3Value;
            case DesignValueKind.ChaResource:
                return resourceValue.ToRuntime();
            case DesignValueKind.ChaProperty:
                return propertyValue.ToRuntime();
            case DesignValueKind.Damage:
                return damageValue.ToRuntime();
            case DesignValueKind.AddBuffInfo:
                return addBuffValue.ToRuntime(catalog);
            case DesignValueKind.BulletLauncher:
                return bulletLauncherValue.ToRuntime(catalog);
            case DesignValueKind.AoeLauncher:
                return aoeLauncherValue.ToRuntime(catalog);
            case DesignValueKind.Array:
                return BuildArrayRuntime(catalog);
            default:
                return null;
        }
    }

    private object BuildArrayRuntime(GameDesignCatalogAsset catalog)
    {
        return GameDesignCatalogAssetUtility.BuildArrayObject(catalog, arrayValues, arrayElementKind);
    }
}

[Serializable]
public struct DesignEventCall
{
    public string eventId;
    public DesignTimelineArgument[] arguments;

    public object[] ToRuntimeParams(GameDesignCatalogAsset catalog)
    {
        return GameDesignCatalogAssetUtility.BuildObjectArray(catalog, arguments, DesignValueKind.Null);
    }
}

[Serializable]
public struct SkillDefinition
{
    public string id;
    public DesignChaResource condition;
    public DesignChaResource cost;
    public string effectTimelineId;
    public DesignAddBuffInfo[] buffs;

    public bool TryBuild(GameDesignCatalogAsset catalog, out SkillModel value)
    {
        TimelineModel effect;
        if (!catalog.TryGetTimeline(effectTimelineId, out effect))
        {
            Debug.LogError("[GameDesignCatalog] Unknown effect timeline id for skill " + id + ": " + effectTimelineId);
            value = default(SkillModel);
            return false;
        }

        AddBuffInfo[] learnBuffs = GameDesignCatalogAssetUtility.BuildAddBuffArray(catalog, buffs);
        value = new SkillModel(id, cost.ToRuntime(), condition.ToRuntime(), effect, learnBuffs);
        return true;
    }
}

[Serializable]
public struct TimelineDefinition
{
    public string id;
    public TimelineNodeDefinition[] nodes;
    public float duration;
    public TimelineGoTo chargeGoBack;

    public bool TryBuild(GameDesignCatalogAsset catalog, out TimelineModel value)
    {
        TimelineNode[] runtimeNodes = GameDesignCatalogAssetUtility.BuildNodes(catalog, nodes);
        value = new TimelineModel(id, runtimeNodes, duration, chargeGoBack);
        return true;
    }
}

[Serializable]
public struct TimelineNodeDefinition
{
    public float timeElapsed;
    public string eventId;
    public DesignTimelineArgument[] arguments;

    public bool TryBuild(GameDesignCatalogAsset catalog, out TimelineNode value)
    {
        object[] args = GameDesignCatalogAssetUtility.BuildObjectArray(catalog, arguments, DesignValueKind.Null);
        value = new TimelineNode(timeElapsed, eventId, args);
        return true;
    }
}

[Serializable]
public struct BuffDefinition
{
    public string id;
    public string name;
    public string[] tags;
    public int priority;
    public int maxStack;
    public float tickTime;
    public DesignEventCall onOccur;
    public DesignEventCall onRemoved;
    public DesignEventCall onTick;
    public DesignEventCall onCast;
    public DesignEventCall onHit;
    public DesignEventCall onBeHurt;
    public DesignEventCall onKill;
    public DesignEventCall onBeKilled;
    public ChaControlState stateMod;
    public DesignChaProperty[] propMod;

    public bool TryBuild(GameDesignCatalogAsset catalog, out BuffModel value)
    {
        ChaProperty[] runtimeProps = GameDesignCatalogAssetUtility.BuildPropArray(propMod);
        value = new BuffModel(
            id,
            name,
            tags,
            priority,
            maxStack,
            tickTime,
            onOccur.eventId,
            onOccur.ToRuntimeParams(catalog),
            onRemoved.eventId,
            onRemoved.ToRuntimeParams(catalog),
            onTick.eventId,
            onTick.ToRuntimeParams(catalog),
            onCast.eventId,
            onCast.ToRuntimeParams(catalog),
            onHit.eventId,
            onHit.ToRuntimeParams(catalog),
            onBeHurt.eventId,
            onBeHurt.ToRuntimeParams(catalog),
            onKill.eventId,
            onKill.ToRuntimeParams(catalog),
            onBeKilled.eventId,
            onBeKilled.ToRuntimeParams(catalog),
            stateMod,
            runtimeProps
        );
        return true;
    }
}

[Serializable]
public struct BulletDefinition
{
    public string id;
    public string prefab;
    public string onCreate;
    public DesignTimelineArgument[] onCreateParams;
    public string onHit;
    public DesignTimelineArgument[] onHitParams;
    public string onRemoved;
    public DesignTimelineArgument[] onRemovedParams;
    public MoveType moveType;
    public bool removeOnObstacle;
    public float radius;
    public int hitTimes;
    public float sameTargetDelay;
    public bool hitFoe;
    public bool hitAlly;

    public bool TryBuild(GameDesignCatalogAsset catalog, out BulletModel value)
    {
        object[] createParams = GameDesignCatalogAssetUtility.BuildObjectArray(catalog, onCreateParams, DesignValueKind.Null);
        object[] hitParams = GameDesignCatalogAssetUtility.BuildObjectArray(catalog, onHitParams, DesignValueKind.Null);
        object[] removedParams = GameDesignCatalogAssetUtility.BuildObjectArray(catalog, onRemovedParams, DesignValueKind.Null);
        value = new BulletModel(
            id,
            prefab,
            onCreate,
            createParams,
            onHit,
            hitParams,
            onRemoved,
            removedParams,
            moveType,
            removeOnObstacle,
            radius,
            hitTimes,
            sameTargetDelay,
            hitFoe,
            hitAlly
        );
        return true;
    }
}

[Serializable]
public struct AoeDefinition
{
    public string id;
    public string prefab;
    public string[] tags;
    public float tickTime;
    public bool removeOnObstacle;
    public string onCreate;
    public DesignTimelineArgument[] onCreateParams;
    public string onRemoved;
    public DesignTimelineArgument[] onRemovedParams;
    public string onTick;
    public DesignTimelineArgument[] onTickParams;
    public string onChaEnter;
    public DesignTimelineArgument[] onChaEnterParams;
    public string onChaLeave;
    public DesignTimelineArgument[] onChaLeaveParams;
    public string onBulletEnter;
    public DesignTimelineArgument[] onBulletEnterParams;
    public string onBulletLeave;
    public DesignTimelineArgument[] onBulletLeaveParams;

    public bool TryBuild(GameDesignCatalogAsset catalog, out AoeModel value)
    {
        object[] createParams = GameDesignCatalogAssetUtility.BuildObjectArray(catalog, onCreateParams, DesignValueKind.Null);
        object[] removedParams = GameDesignCatalogAssetUtility.BuildObjectArray(catalog, onRemovedParams, DesignValueKind.Null);
        object[] tickParams = GameDesignCatalogAssetUtility.BuildObjectArray(catalog, onTickParams, DesignValueKind.Null);
        object[] chaEnterParams = GameDesignCatalogAssetUtility.BuildObjectArray(catalog, onChaEnterParams, DesignValueKind.Null);
        object[] chaLeaveParams = GameDesignCatalogAssetUtility.BuildObjectArray(catalog, onChaLeaveParams, DesignValueKind.Null);
        object[] bulletEnterParams = GameDesignCatalogAssetUtility.BuildObjectArray(catalog, onBulletEnterParams, DesignValueKind.Null);
        object[] bulletLeaveParams = GameDesignCatalogAssetUtility.BuildObjectArray(catalog, onBulletLeaveParams, DesignValueKind.Null);
        value = new AoeModel(
            id,
            prefab,
            tags,
            tickTime,
            removeOnObstacle,
            onCreate,
            createParams,
            onRemoved,
            removedParams,
            onTick,
            tickParams,
            onChaEnter,
            chaEnterParams,
            onChaLeave,
            chaLeaveParams,
            onBulletEnter,
            bulletEnterParams,
            onBulletLeave,
            bulletLeaveParams
        );
        return true;
    }
}

[Serializable]
public struct MainChainMigrationEntry
{
    public string skillId;
    public string timelineId;
    public string[] bulletIds;
    public string[] aoeIds;
    public string[] buffIds;
    public SkillTargetMode targetMode;
    public DesignChaResource cost;
    public float cooldown;
    public string[] tags;
    public string notes;
    public int migrationPriority;
    public string originSource;
    public string targetSource;
    public string[] dependencyEntrances;
}

[Serializable]
public struct MainChainMigrationSchemaRow
{
    public string name;
    public string type;
    public bool required;
    public string source;
    public string defaultValue;
    public string validation;
}

[Serializable]
public struct MainChainMigrationAcceptanceRow
{
    public string skillId;
    public string status;
    public string diffSummary;
    public string timelineId;
    public string bulletIds;
    public string aoeIds;
    public string buffIds;
    public string targetMode;
    public string cost;
    public string cooldown;
    public string tags;
    public string notes;
}

[Serializable]
public enum DesignValueKind
{
    Null = 0,
    Int = 1,
    Float = 2,
    Bool = 3,
    String = 4,
    Vector3 = 5,
    ChaResource = 6,
    ChaProperty = 7,
    Damage = 8,
    AddBuffInfo = 9,
    BulletLauncher = 10,
    AoeLauncher = 11,
    Array = 12
}

public static class GameDesignCatalogAssetUtility
{
    public static readonly string[] MainChainSkillIds = new[]
    {
        "fire",
        "fireMultiple",
        "spaceMonkeyBall",
        "grenade",
        "explosiveBarrel",
        "cloakBoomerang",
        "teleportBullet",
    };

    private const string LegacyStaticTableSourceLabel = "legacy static table";
    private const string CatalogAssetSourceLabel = "GameDesignCatalog asset";

    public static AddBuffInfo[] BuildAddBuffArray(GameDesignCatalogAsset catalog, DesignAddBuffInfo[] values)
    {
        if (values == null || values.Length <= 0)
        {
            return new AddBuffInfo[0];
        }

        AddBuffInfo[] runtime = new AddBuffInfo[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            runtime[i] = values[i].ToRuntime(catalog);
        }

        return runtime;
    }

    public static ChaProperty[] BuildPropArray(DesignChaProperty[] values)
    {
        if (values == null || values.Length <= 0)
        {
            return new ChaProperty[0];
        }

        ChaProperty[] runtime = new ChaProperty[Mathf.Min(2, values.Length)];
        for (int i = 0; i < runtime.Length; i++)
        {
            runtime[i] = values[i].ToRuntime();
        }

        return runtime;
    }

    public static TimelineNode[] BuildNodes(GameDesignCatalogAsset catalog, TimelineNodeDefinition[] values)
    {
        if (values == null || values.Length <= 0)
        {
            return new TimelineNode[0];
        }

        TimelineNode[] runtime = new TimelineNode[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            TimelineNode node;
            values[i].TryBuild(catalog, out node);
            runtime[i] = node;
        }

        return runtime;
    }

    public static object[] BuildObjectArray(GameDesignCatalogAsset catalog, DesignTimelineArgument[] values, DesignValueKind arrayElementKind)
    {
        if (values == null || values.Length <= 0)
        {
            return new object[0];
        }

        object[] runtime = new object[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            runtime[i] = values[i].ToRuntime(catalog);
        }

        return runtime;
    }

    public static object BuildArrayObject(GameDesignCatalogAsset catalog, DesignTimelineArgument[] values, DesignValueKind elementKind)
    {
        if (values == null || values.Length <= 0)
        {
            return CreateEmptyArray(elementKind);
        }

        switch (elementKind)
        {
            case DesignValueKind.String:
            {
                string[] runtime = new string[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    runtime[i] = values[i].ToRuntime(catalog) as string;
                }
                return runtime;
            }
            case DesignValueKind.Int:
            {
                int[] runtime = new int[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    runtime[i] = (int)values[i].ToRuntime(catalog);
                }
                return runtime;
            }
            case DesignValueKind.Float:
            {
                float[] runtime = new float[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    runtime[i] = (float)values[i].ToRuntime(catalog);
                }
                return runtime;
            }
            case DesignValueKind.Bool:
            {
                bool[] runtime = new bool[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    runtime[i] = (bool)values[i].ToRuntime(catalog);
                }
                return runtime;
            }
            case DesignValueKind.Vector3:
            {
                Vector3[] runtime = new Vector3[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    runtime[i] = (Vector3)values[i].ToRuntime(catalog);
                }
                return runtime;
            }
            case DesignValueKind.ChaResource:
            {
                ChaResource[] runtime = new ChaResource[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    runtime[i] = (ChaResource)values[i].ToRuntime(catalog);
                }
                return runtime;
            }
            case DesignValueKind.ChaProperty:
            {
                ChaProperty[] runtime = new ChaProperty[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    runtime[i] = (ChaProperty)values[i].ToRuntime(catalog);
                }
                return runtime;
            }
            case DesignValueKind.Damage:
            {
                Damage[] runtime = new Damage[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    runtime[i] = (Damage)values[i].ToRuntime(catalog);
                }
                return runtime;
            }
            case DesignValueKind.AddBuffInfo:
            {
                AddBuffInfo[] runtime = new AddBuffInfo[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    runtime[i] = (AddBuffInfo)values[i].ToRuntime(catalog);
                }
                return runtime;
            }
            case DesignValueKind.BulletLauncher:
            {
                BulletLauncher[] runtime = new BulletLauncher[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    runtime[i] = (BulletLauncher)values[i].ToRuntime(catalog);
                }
                return runtime;
            }
            case DesignValueKind.AoeLauncher:
            {
                AoeLauncher[] runtime = new AoeLauncher[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    runtime[i] = (AoeLauncher)values[i].ToRuntime(catalog);
                }
                return runtime;
            }
            default:
            {
                object[] runtime = new object[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    runtime[i] = values[i].ToRuntime(catalog);
                }
                return runtime;
            }
        }
    }

    public static Dictionary<string, object> BuildDictionary(GameDesignCatalogAsset catalog, DesignKeyValue[] values)
    {
        if (values == null || values.Length <= 0)
        {
            return null;
        }

        Dictionary<string, object> dict = new Dictionary<string, object>();
        for (int i = 0; i < values.Length; i++)
        {
            if (string.IsNullOrEmpty(values[i].key))
            {
                continue;
            }
            dict[values[i].key] = values[i].value.ToRuntime(catalog);
        }

        return dict;
    }

    public static GameDesignCatalogAsset CreateLegacyBulletAoeCatalog()
    {
        GameDesignCatalogAsset catalog = ScriptableObject.CreateInstance<GameDesignCatalogAsset>();
        catalog.bullets = BuildLegacyBulletDefinitions();
        catalog.aoes = BuildLegacyAoeDefinitions();
        return catalog;
    }

    public static GameDesignCatalogAsset CreateLegacyFullCatalog()
    {
        GameDesignCatalogAsset catalog = ScriptableObject.CreateInstance<GameDesignCatalogAsset>();
        catalog.skills = BuildLegacySkillDefinitions();
        catalog.timelines = BuildLegacyTimelineDefinitions();
        catalog.buffs = BuildLegacyBuffDefinitions();
        catalog.bullets = BuildLegacyBulletDefinitions();
        catalog.aoes = BuildLegacyAoeDefinitions();
        catalog.mainChainMigrationEntries = BuildMainChainMigrationEntries(catalog);
        catalog.mainChainMigrationSchema = BuildMainChainMigrationSchemaRows();
        return catalog;
    }

    public static MainChainMigrationEntry[] BuildMainChainMigrationEntries(GameDesignCatalogAsset catalog)
    {
        List<MainChainMigrationEntry> entries = new List<MainChainMigrationEntry>(MainChainSkillIds.Length);
        for (int i = 0; i < MainChainSkillIds.Length; i++)
        {
            entries.Add(BuildMainChainMigrationEntry(catalog, MainChainSkillIds[i], i + 1));
        }

        return entries.ToArray();
    }

    public static string[] BuildMainChainMigrationChecklistLines(GameDesignCatalogAsset catalog)
    {
        MainChainMigrationEntry[] entries = BuildMainChainMigrationEntries(catalog);
        List<string> lines = new List<string>();
        lines.Add("# Main Chain Migration Checklist");
        lines.Add("originSource: " + LegacyStaticTableSourceLabel);
        lines.Add("targetSource: " + CatalogAssetSourceLabel);
        lines.Add("runtimeFallback: legacy mirror");
        lines.Add("");

        for (int i = 0; i < entries.Length; i++)
        {
            AppendMainChainEntry(lines, entries[i]);
        }

        return lines.ToArray();
    }

    public static MainChainMigrationSchemaRow[] BuildMainChainMigrationSchemaRows()
    {
        return new[]
        {
            new MainChainMigrationSchemaRow
            {
                name = "skillId",
                type = "string",
                required = true,
                source = LegacyStaticTableSourceLabel,
                defaultValue = "",
                validation = "non-empty, unique, resolves to SkillData/SkillDefinition"
            },
            new MainChainMigrationSchemaRow
            {
                name = "timelineId",
                type = "string",
                required = true,
                source = LegacyStaticTableSourceLabel,
                defaultValue = "",
                validation = "non-empty, resolves to TimelineData/TimelineDefinition"
            },
            new MainChainMigrationSchemaRow
            {
                name = "bulletId",
                type = "string[]",
                required = false,
                source = LegacyStaticTableSourceLabel,
                defaultValue = "[]",
                validation = "optional; each non-empty id resolves to BulletData/BulletDefinition"
            },
            new MainChainMigrationSchemaRow
            {
                name = "aoeId",
                type = "string[]",
                required = false,
                source = LegacyStaticTableSourceLabel,
                defaultValue = "[]",
                validation = "optional; each non-empty id resolves to AoEData/AoeDefinition"
            },
            new MainChainMigrationSchemaRow
            {
                name = "buffId",
                type = "string[]",
                required = false,
                source = LegacyStaticTableSourceLabel,
                defaultValue = "[]",
                validation = "optional; each non-empty id resolves to BuffData/BuffDefinition"
            },
            new MainChainMigrationSchemaRow
            {
                name = "targetMode",
                type = "SkillTargetMode",
                required = true,
                source = LegacyStaticTableSourceLabel,
                defaultValue = "Auto",
                validation = "must be a defined SkillTargetMode value"
            },
            new MainChainMigrationSchemaRow
            {
                name = "cost",
                type = "DesignChaResource",
                required = true,
                source = LegacyStaticTableSourceLabel,
                defaultValue = "hp=0, ammo=0, stamina=0",
                validation = "non-null; each resource component should be non-negative"
            },
            new MainChainMigrationSchemaRow
            {
                name = "cooldown",
                type = "float",
                required = false,
                source = "migration metadata; runtime cooldown stays on SkillObj",
                defaultValue = "0",
                validation = "must be >= 0; default 0"
            },
            new MainChainMigrationSchemaRow
            {
                name = "tags",
                type = "string[]",
                required = false,
                source = "migration metadata",
                defaultValue = "[main-chain, legacy-static, catalog-ready]",
                validation = "unique, non-empty tokens recommended"
            },
            new MainChainMigrationSchemaRow
            {
                name = "notes",
                type = "string",
                required = false,
                source = "migration metadata",
                defaultValue = "",
                validation = "freeform summary of origin and dependency entrances"
            }
        };
    }

    public static void SynchronizeMainChainMigrationData(GameDesignCatalogAsset catalog)
    {
        if (catalog == null)
        {
            return;
        }

        catalog.mainChainMigrationEntries = BuildMainChainMigrationEntries(catalog);
        catalog.mainChainMigrationSchema = BuildMainChainMigrationSchemaRows();
    }

    public static string[] BuildMainChainMigrationSchemaLines()
    {
        MainChainMigrationSchemaRow[] rows = BuildMainChainMigrationSchemaRows();
        List<string> lines = new List<string>();
        lines.Add("# Main Chain Migration Schema");
        lines.Add("originSource: " + LegacyStaticTableSourceLabel);
        lines.Add("targetSource: " + CatalogAssetSourceLabel);
        lines.Add("");
        lines.Add("| name | type | required | source | defaultValue | validation |");
        lines.Add("| --- | --- | --- | --- | --- | --- |");

        for (int i = 0; i < rows.Length; i++)
        {
            MainChainMigrationSchemaRow row = rows[i];
            lines.Add("| " + row.name + " | " + row.type + " | " + (row.required ? "yes" : "no") + " | " + row.source + " | " + row.defaultValue + " | " + row.validation + " |");
        }

        return lines.ToArray();
    }

    private static MainChainMigrationEntry BuildMainChainMigrationEntry(GameDesignCatalogAsset catalog, string skillId, int migrationPriority)
    {
        SkillModel skill = default(SkillModel);
        if (catalog == null || !catalog.TryGetSkill(skillId, out skill))
        {
            DesingerTables.SkillData.data.TryGetValue(skillId, out skill);
        }

        HashSet<string> bulletIds = new HashSet<string>();
        HashSet<string> aoeIds = new HashSet<string>();
        HashSet<string> buffIds = new HashSet<string>();
        HashSet<string> timelineIds = new HashSet<string>();
        List<string> dependencyEntrances = new List<string>();

        AddDependencyEntrance(dependencyEntrances, LegacyStaticTableSourceLabel + "/SkillData." + skillId);

        if (!string.IsNullOrEmpty(skill.effect.id))
        {
            timelineIds.Add(skill.effect.id);
            AddDependencyEntrance(dependencyEntrances, LegacyStaticTableSourceLabel + "/TimelineData." + skill.effect.id);
            CollectTimelineDependencies(skill.effect, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances);
        }

        if (skill.buff != null)
        {
            for (int i = 0; i < skill.buff.Length; i++)
            {
                AddBuffInfo buffInfo = skill.buff[i];
                if (string.IsNullOrEmpty(buffInfo.buffModel.id))
                {
                    continue;
                }

                buffIds.Add(buffInfo.buffModel.id);
                AddDependencyEntrance(dependencyEntrances, LegacyStaticTableSourceLabel + "/BuffData." + buffInfo.buffModel.id);
                CollectBuffDependencies(buffInfo.buffModel, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances);
            }
        }

        if (buffIds.Contains("AutoCheckReload"))
        {
            timelineIds.Add("skill_reload");
            AddDependencyEntrance(dependencyEntrances, "origin/runtime: BuffData.AutoCheckReload -> BuffScript.ReloadAmmo -> TimelineData.skill_reload");
        }

        if (buffIds.Contains("TeleportBulletPassive"))
        {
            timelineIds.Add("skill_teleportBullet_tele");
            buffIds.Add("TeleportTo");
            AddDependencyEntrance(dependencyEntrances, "origin/runtime: BuffData.TeleportBulletPassive -> BuffScript.FireTeleportBullet -> TimelineData.skill_teleportBullet_tele");
            AddDependencyEntrance(dependencyEntrances, "origin/runtime: TimelineData.skill_teleportBullet_tele -> BuffData.TeleportTo");
        }

        MainChainMigrationEntry entry = new MainChainMigrationEntry();
        entry.skillId = skillId;
        entry.timelineId = skill.effect.id;
        entry.bulletIds = ToSortedArray(bulletIds);
        entry.aoeIds = ToSortedArray(aoeIds);
        entry.buffIds = ToSortedArray(buffIds);
        entry.targetMode = skill.targetMode;
        entry.cost = ToDesignChaResource(skill.cost);
        entry.cooldown = 0f;
        entry.tags = BuildMainChainTags(skillId, entry);
        entry.notes = BuildMainChainNotes(skillId, entry);
        entry.migrationPriority = migrationPriority;
        entry.originSource = LegacyStaticTableSourceLabel;
        entry.targetSource = CatalogAssetSourceLabel;
        entry.dependencyEntrances = dependencyEntrances.ToArray();
        return entry;
    }

    private static void CollectTimelineDependencies(TimelineModel timeline, HashSet<string> bulletIds, HashSet<string> aoeIds, HashSet<string> buffIds, HashSet<string> timelineIds, List<string> dependencyEntrances)
    {
        if (timeline.nodes == null)
        {
            return;
        }

        for (int i = 0; i < timeline.nodes.Length; i++)
        {
            CollectDesignDependencies(timeline.nodes[i].eveParams, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, LegacyStaticTableSourceLabel + "/TimelineData." + timeline.id);
        }
    }

    private static void CollectBulletDependencies(BulletModel bullet, HashSet<string> bulletIds, HashSet<string> aoeIds, HashSet<string> buffIds, HashSet<string> timelineIds, List<string> dependencyEntrances)
    {
        if (string.IsNullOrEmpty(bullet.id))
        {
            return;
        }

        CollectDesignDependencies(bullet.onCreateParam, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, LegacyStaticTableSourceLabel + "/BulletData." + bullet.id);
        CollectDesignDependencies(bullet.onHitParams, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, LegacyStaticTableSourceLabel + "/BulletData." + bullet.id);
        CollectDesignDependencies(bullet.onRemovedParams, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, LegacyStaticTableSourceLabel + "/BulletData." + bullet.id);
    }

    private static void CollectAoeDependencies(AoeModel aoe, HashSet<string> bulletIds, HashSet<string> aoeIds, HashSet<string> buffIds, HashSet<string> timelineIds, List<string> dependencyEntrances)
    {
        if (string.IsNullOrEmpty(aoe.id))
        {
            return;
        }

        CollectDesignDependencies(aoe.onCreateParams, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, LegacyStaticTableSourceLabel + "/AoEData." + aoe.id);
        CollectDesignDependencies(aoe.onRemovedParams, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, LegacyStaticTableSourceLabel + "/AoEData." + aoe.id);
        CollectDesignDependencies(aoe.onTickParams, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, LegacyStaticTableSourceLabel + "/AoEData." + aoe.id);
        CollectDesignDependencies(aoe.onChaEnterParams, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, LegacyStaticTableSourceLabel + "/AoEData." + aoe.id);
        CollectDesignDependencies(aoe.onChaLeaveParams, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, LegacyStaticTableSourceLabel + "/AoEData." + aoe.id);
        CollectDesignDependencies(aoe.onBulletEnterParams, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, LegacyStaticTableSourceLabel + "/AoEData." + aoe.id);
        CollectDesignDependencies(aoe.onBulletLeaveParams, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, LegacyStaticTableSourceLabel + "/AoEData." + aoe.id);
    }

    private static void CollectBuffDependencies(BuffModel buff, HashSet<string> bulletIds, HashSet<string> aoeIds, HashSet<string> buffIds, HashSet<string> timelineIds, List<string> dependencyEntrances)
    {
        if (string.IsNullOrEmpty(buff.id))
        {
            return;
        }

        CollectDesignDependencies(buff.onOccurParams, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, LegacyStaticTableSourceLabel + "/BuffData." + buff.id);
        CollectDesignDependencies(buff.onRemovedParams, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, LegacyStaticTableSourceLabel + "/BuffData." + buff.id);
        CollectDesignDependencies(buff.onTickParams, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, LegacyStaticTableSourceLabel + "/BuffData." + buff.id);
        CollectDesignDependencies(buff.onCastParams, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, LegacyStaticTableSourceLabel + "/BuffData." + buff.id);
        CollectDesignDependencies(buff.onHitParams, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, LegacyStaticTableSourceLabel + "/BuffData." + buff.id);
        CollectDesignDependencies(buff.onBeHurtParams, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, LegacyStaticTableSourceLabel + "/BuffData." + buff.id);
        CollectDesignDependencies(buff.onKillParams, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, LegacyStaticTableSourceLabel + "/BuffData." + buff.id);
        CollectDesignDependencies(buff.onBeKilledParams, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, LegacyStaticTableSourceLabel + "/BuffData." + buff.id);
    }

    private static void CollectDesignDependencies(object value, HashSet<string> bulletIds, HashSet<string> aoeIds, HashSet<string> buffIds, HashSet<string> timelineIds, List<string> dependencyEntrances, string originLabel)
    {
        if (value == null)
        {
            return;
        }

        Array arrayValue = value as Array;
        if (arrayValue != null)
        {
            for (int i = 0; i < arrayValue.Length; i++)
            {
                CollectDesignDependencies(arrayValue.GetValue(i), bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, originLabel);
            }

            return;
        }

        Dictionary<string, object> dictionaryValue = value as Dictionary<string, object>;
        if (dictionaryValue != null)
        {
            foreach (KeyValuePair<string, object> pair in dictionaryValue)
            {
                CollectDesignDependencies(pair.Value, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, originLabel);
            }

            return;
        }

        if (value is AddBuffInfo)
        {
            AddBuffInfo addBuffInfo = (AddBuffInfo)value;
            if (!string.IsNullOrEmpty(addBuffInfo.buffModel.id))
            {
                buffIds.Add(addBuffInfo.buffModel.id);
                AddDependencyEntrance(dependencyEntrances, originLabel + " -> BuffData." + addBuffInfo.buffModel.id);
                CollectBuffDependencies(addBuffInfo.buffModel, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances);
            }

            CollectDesignDependencies(addBuffInfo.buffParam, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, originLabel);
            return;
        }

        BulletLauncher bulletLauncher = value as BulletLauncher;
        if (bulletLauncher != null)
        {
            if (!string.IsNullOrEmpty(bulletLauncher.model.id))
            {
                bulletIds.Add(bulletLauncher.model.id);
                AddDependencyEntrance(dependencyEntrances, originLabel + " -> BulletData." + bulletLauncher.model.id);
                CollectBulletDependencies(bulletLauncher.model, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances);
            }

            CollectDesignDependencies(bulletLauncher.param, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, originLabel);
            return;
        }

        AoeLauncher aoeLauncher = value as AoeLauncher;
        if (aoeLauncher != null)
        {
            if (!string.IsNullOrEmpty(aoeLauncher.model.id))
            {
                aoeIds.Add(aoeLauncher.model.id);
                AddDependencyEntrance(dependencyEntrances, originLabel + " -> AoEData." + aoeLauncher.model.id);
                CollectAoeDependencies(aoeLauncher.model, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances);
            }

            CollectDesignDependencies(aoeLauncher.param, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, originLabel);
            CollectDesignDependencies(aoeLauncher.tweenParam, bulletIds, aoeIds, buffIds, timelineIds, dependencyEntrances, originLabel);
            return;
        }
    }

    private static string[] BuildMainChainTags(string skillId, MainChainMigrationEntry entry)
    {
        List<string> tags = new List<string>();
        tags.Add("main-chain");
        tags.Add("legacy-static");
        tags.Add("catalog-ready");

        if (entry.bulletIds != null && entry.bulletIds.Length > 0)
        {
            tags.Add("projectile");
        }

        if (entry.aoeIds != null && entry.aoeIds.Length > 0)
        {
            tags.Add("aoe");
        }

        if (entry.buffIds != null && entry.buffIds.Length > 0)
        {
            tags.Add("buff");
        }

        if (string.Equals(skillId, "fire", StringComparison.Ordinal) || string.Equals(skillId, "fireMultiple", StringComparison.Ordinal) || string.Equals(skillId, "teleportBullet", StringComparison.Ordinal))
        {
            tags.Add("core-fire");
        }

        if (string.Equals(skillId, "grenade", StringComparison.Ordinal) || string.Equals(skillId, "spaceMonkeyBall", StringComparison.Ordinal))
        {
            tags.Add("area-control");
        }

        if (string.Equals(skillId, "explosiveBarrel", StringComparison.Ordinal))
        {
            tags.Add("summon");
        }

        if (string.Equals(skillId, "cloakBoomerang", StringComparison.Ordinal))
        {
            tags.Add("boomerang");
        }

        return tags.ToArray();
    }

    private static string BuildMainChainNotes(string skillId, MainChainMigrationEntry entry)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("legacy static table -> GameDesignCatalog asset; runtime fallback remains legacy mirror.");
        builder.Append(" cooldown stays runtime-only on SkillObj.");

        if (string.Equals(skillId, "fire", StringComparison.Ordinal))
        {
            builder.Append(" SkillData.fire maps to TimelineData.skill_fire and BulletData.bullet2d; AutoCheckReload routes to skill_reload.");
        }
        else if (string.Equals(skillId, "fireMultiple", StringComparison.Ordinal))
        {
            builder.Append(" SkillData.fireMultiple maps to TimelineData.skill_fireMultiple and BulletData.bullet2d; AutoCheckReload routes to skill_reload.");
        }
        else if (string.Equals(skillId, "spaceMonkeyBall", StringComparison.Ordinal))
        {
            builder.Append(" SkillData.spaceMonkeyBall maps to TimelineData.skill_spaceMonkeyBall and AoEData.SpaceMonkeyBall.");
        }
        else if (string.Equals(skillId, "grenade", StringComparison.Ordinal))
        {
            builder.Append(" SkillData.grenade maps to TimelineData.skill_grenade and BulletData.boomball, with nested AoEData.BoomExplosive and AoEData.StayingBoom from bullet params.");
        }
        else if (string.Equals(skillId, "explosiveBarrel", StringComparison.Ordinal))
        {
            builder.Append(" SkillData.explosiveBarrel maps to TimelineData.skill_exploseBarrel and BuffData.ExplosionBarrel.");
        }
        else if (string.Equals(skillId, "cloakBoomerang", StringComparison.Ordinal))
        {
            builder.Append(" SkillData.cloakBoomerang maps to TimelineData.skill_cloakBoomerang and BulletData.cloakBoomerang.");
        }
        else if (string.Equals(skillId, "teleportBullet", StringComparison.Ordinal))
        {
            builder.Append(" SkillData.teleportBullet maps to TimelineData.skill_teleportBullet_fire and BulletData.teleportBullet; TeleportBulletPassive triggers skill_teleportBullet_tele and TeleportTo in the runtime buff hook.");
        }

        if (entry.bulletIds != null && entry.bulletIds.Length > 0)
        {
            builder.Append(" bullets=").Append(JoinIds(entry.bulletIds));
        }

        if (entry.aoeIds != null && entry.aoeIds.Length > 0)
        {
            builder.Append(" aoes=").Append(JoinIds(entry.aoeIds));
        }

        if (entry.buffIds != null && entry.buffIds.Length > 0)
        {
            builder.Append(" buffs=").Append(JoinIds(entry.buffIds));
        }

        return builder.ToString();
    }

    private static void AppendMainChainEntry(List<string> lines, MainChainMigrationEntry entry)
    {
        lines.Add("- skillId: " + entry.skillId);
        lines.Add("  migrationPriority: P" + entry.migrationPriority.ToString());
        lines.Add("  source: " + entry.originSource + " -> " + entry.targetSource);
        lines.Add("  timelineId: " + SafeValue(entry.timelineId));
        lines.Add("  bulletIds: " + FormatIds(entry.bulletIds));
        lines.Add("  aoeIds: " + FormatIds(entry.aoeIds));
        lines.Add("  buffIds: " + FormatIds(entry.buffIds));
        lines.Add("  targetMode: " + entry.targetMode.ToString());
        lines.Add("  cost: " + FormatChaResource(entry.cost));
        lines.Add("  cooldown: " + entry.cooldown.ToString("0.###"));
        lines.Add("  tags: " + FormatIds(entry.tags));
        lines.Add("  notes: " + entry.notes);
        lines.Add("  dependencyEntrances:");
        if (entry.dependencyEntrances != null)
        {
            for (int i = 0; i < entry.dependencyEntrances.Length; i++)
            {
                lines.Add("    - " + entry.dependencyEntrances[i]);
            }
        }

        lines.Add("");
    }

    private static void AddDependencyEntrance(List<string> entries, string value)
    {
        if (entries == null || string.IsNullOrEmpty(value))
        {
            return;
        }

        if (!entries.Contains(value))
        {
            entries.Add(value);
        }
    }

    private static string[] ToSortedArray(HashSet<string> values)
    {
        if (values == null || values.Count <= 0)
        {
            return new string[0];
        }

        List<string> list = new List<string>(values);
        list.Sort(StringComparer.Ordinal);
        return list.ToArray();
    }

    private static string SafeValue(string value)
    {
        return string.IsNullOrEmpty(value) ? "<none>" : value;
    }

    private static string FormatIds(string[] values)
    {
        if (values == null || values.Length <= 0)
        {
            return "<none>";
        }

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < values.Length; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }

            builder.Append(values[i]);
        }

        return builder.ToString();
    }

    private static string JoinIds(string[] values)
    {
        return FormatIds(values);
    }

    private static string FormatChaResource(DesignChaResource resource)
    {
        return "hp=" + resource.hp + ", ammo=" + resource.ammo + ", stamina=" + resource.stamina;
    }

    private static SkillDefinition[] BuildLegacySkillDefinitions()
    {
        Dictionary<string, SkillModel> source = DesingerTables.SkillData.data;
        if (source == null || source.Count <= 0)
        {
            return new SkillDefinition[0];
        }

        List<SkillDefinition> definitions = new List<SkillDefinition>(source.Count);
        foreach (KeyValuePair<string, SkillModel> pair in source)
        {
            definitions.Add(ToSkillDefinition(pair.Value));
        }

        return definitions.ToArray();
    }

    private static TimelineDefinition[] BuildLegacyTimelineDefinitions()
    {
        Dictionary<string, TimelineModel> source = DesingerTables.TimelineData.data;
        if (source == null || source.Count <= 0)
        {
            return new TimelineDefinition[0];
        }

        List<TimelineDefinition> definitions = new List<TimelineDefinition>(source.Count);
        foreach (KeyValuePair<string, TimelineModel> pair in source)
        {
            definitions.Add(ToTimelineDefinition(pair.Value));
        }

        return definitions.ToArray();
    }

    private static BuffDefinition[] BuildLegacyBuffDefinitions()
    {
        Dictionary<string, BuffModel> source = DesingerTables.BuffData.data;
        if (source == null || source.Count <= 0)
        {
            return new BuffDefinition[0];
        }

        List<BuffDefinition> definitions = new List<BuffDefinition>(source.Count);
        foreach (KeyValuePair<string, BuffModel> pair in source)
        {
            definitions.Add(ToBuffDefinition(pair.Value));
        }

        return definitions.ToArray();
    }

    private static BulletDefinition[] BuildLegacyBulletDefinitions()
    {
        Dictionary<string, BulletModel> source = DesingerTables.BulletData.data;
        if (source == null || source.Count <= 0)
        {
            return new BulletDefinition[0];
        }

        List<BulletDefinition> definitions = new List<BulletDefinition>(source.Count);
        foreach (KeyValuePair<string, BulletModel> pair in source)
        {
            definitions.Add(ToBulletDefinition(pair.Value));
        }

        return definitions.ToArray();
    }

    private static AoeDefinition[] BuildLegacyAoeDefinitions()
    {
        Dictionary<string, AoeModel> source = DesingerTables.AoEData.data;
        if (source == null || source.Count <= 0)
        {
            return new AoeDefinition[0];
        }

        List<AoeDefinition> definitions = new List<AoeDefinition>(source.Count);
        foreach (KeyValuePair<string, AoeModel> pair in source)
        {
            definitions.Add(ToAoeDefinition(pair.Value));
        }

        return definitions.ToArray();
    }

    private static BulletDefinition ToBulletDefinition(BulletModel model)
    {
        BulletDefinition definition = new BulletDefinition();
        definition.id = model.id;
        definition.prefab = model.prefab;
        definition.onCreate = ResolveBulletEventId(BulletScript.onCreateFunc, model.onCreate);
        definition.onCreateParams = ToTimelineArguments(model.onCreateParam);
        definition.onHit = ResolveBulletEventId(BulletScript.onHitFunc, model.onHit);
        definition.onHitParams = ToTimelineArguments(model.onHitParams);
        definition.onRemoved = ResolveBulletEventId(BulletScript.onRemovedFunc, model.onRemoved);
        definition.onRemovedParams = ToTimelineArguments(model.onRemovedParams);
        definition.moveType = model.moveType;
        definition.removeOnObstacle = model.removeOnObstacle;
        definition.radius = model.radius;
        definition.hitTimes = model.hitTimes;
        definition.sameTargetDelay = model.sameTargetDelay;
        definition.hitFoe = model.hitFoe;
        definition.hitAlly = model.hitAlly;
        return definition;
    }

    private static AoeDefinition ToAoeDefinition(AoeModel model)
    {
        AoeDefinition definition = new AoeDefinition();
        definition.id = model.id;
        definition.prefab = model.prefab;
        definition.tags = model.tags;
        definition.tickTime = model.tickTime;
        definition.removeOnObstacle = model.removeOnObstacle;
        definition.onCreate = ResolveAoeEventId(AoEScript.onCreateFunc, model.onCreate);
        definition.onCreateParams = ToTimelineArguments(model.onCreateParams);
        definition.onRemoved = ResolveAoeEventId(AoEScript.onRemovedFunc, model.onRemoved);
        definition.onRemovedParams = ToTimelineArguments(model.onRemovedParams);
        definition.onTick = ResolveAoeEventId(AoEScript.onTickFunc, model.onTick);
        definition.onTickParams = ToTimelineArguments(model.onTickParams);
        definition.onChaEnter = ResolveAoeEventId(AoEScript.onChaEnterFunc, model.onChaEnter);
        definition.onChaEnterParams = ToTimelineArguments(model.onChaEnterParams);
        definition.onChaLeave = ResolveAoeEventId(AoEScript.onChaLeaveFunc, model.onChaLeave);
        definition.onChaLeaveParams = ToTimelineArguments(model.onChaLeaveParams);
        definition.onBulletEnter = ResolveAoeEventId(AoEScript.onBulletEnterFunc, model.onBulletEnter);
        definition.onBulletEnterParams = ToTimelineArguments(model.onBulletEnterParams);
        definition.onBulletLeave = ResolveAoeEventId(AoEScript.onBulletLeaveFunc, model.onBulletLeave);
        definition.onBulletLeaveParams = ToTimelineArguments(model.onBulletLeaveParams);
        return definition;
    }

    private static SkillDefinition ToSkillDefinition(SkillModel model)
    {
        SkillDefinition definition = new SkillDefinition();
        definition.id = model.id;
        definition.condition = ToDesignChaResource(model.condition);
        definition.cost = ToDesignChaResource(model.cost);
        definition.effectTimelineId = model.effect.id;
        definition.buffs = ToDesignAddBuffInfoArray(model.buff);
        return definition;
    }

    private static TimelineDefinition ToTimelineDefinition(TimelineModel model)
    {
        TimelineDefinition definition = new TimelineDefinition();
        definition.id = model.id;
        definition.duration = model.duration;
        definition.chargeGoBack = model.chargeGoBack;
        definition.nodes = ToTimelineNodeDefinitions(model.nodes);
        return definition;
    }

    private static BuffDefinition ToBuffDefinition(BuffModel model)
    {
        BuffDefinition definition = new BuffDefinition();
        definition.id = model.id;
        definition.name = model.name;
        definition.tags = model.tags;
        definition.priority = model.priority;
        definition.maxStack = model.maxStack;
        definition.tickTime = model.tickTime;
        definition.onOccur = ToDesignEventCall(model.onOccur, model.onOccurParams, BuffScript.onOccurFunc, "onOccur");
        definition.onRemoved = ToDesignEventCall(model.onRemoved, model.onRemovedParams, BuffScript.onRemovedFunc, "onRemoved");
        definition.onTick = ToDesignEventCall(model.onTick, model.onTickParams, BuffScript.onTickFunc, "onTick");
        definition.onCast = ToDesignEventCall(model.onCast, model.onCastParams, BuffScript.onCastFunc, "onCast");
        definition.onHit = ToDesignEventCall(model.onHit, model.onHitParams, BuffScript.onHitFunc, "onHit");
        definition.onBeHurt = ToDesignEventCall(model.onBeHurt, model.onBeHurtParams, BuffScript.beHurtFunc, "onBeHurt");
        definition.onKill = ToDesignEventCall(model.onKill, model.onKillParams, BuffScript.onKillFunc, "onKill");
        definition.onBeKilled = ToDesignEventCall(model.onBeKilled, model.onBeKilledParams, BuffScript.beKilledFunc, "onBeKilled");
        definition.stateMod = model.stateMod;
        definition.propMod = ToDesignChaPropertyArray(model.propMod);
        return definition;
    }

    private static TimelineNodeDefinition[] ToTimelineNodeDefinitions(TimelineNode[] nodes)
    {
        if (nodes == null || nodes.Length <= 0)
        {
            return new TimelineNodeDefinition[0];
        }

        TimelineNodeDefinition[] runtime = new TimelineNodeDefinition[nodes.Length];
        for (int i = 0; i < nodes.Length; i++)
        {
            runtime[i].timeElapsed = nodes[i].timeElapsed;
            runtime[i].eventId = ResolveTimelineEventId(nodes[i].doEvent);
            runtime[i].arguments = ToTimelineArguments(nodes[i].eveParams);
        }

        return runtime;
    }

    private static DesignAddBuffInfo[] ToDesignAddBuffInfoArray(AddBuffInfo[] values)
    {
        if (values == null || values.Length <= 0)
        {
            return new DesignAddBuffInfo[0];
        }

        DesignAddBuffInfo[] runtime = new DesignAddBuffInfo[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            runtime[i] = ToDesignAddBuffInfo(values[i]);
        }

        return runtime;
    }

    private static DesignChaProperty[] ToDesignChaPropertyArray(ChaProperty[] values)
    {
        if (values == null || values.Length <= 0)
        {
            return new DesignChaProperty[0];
        }

        DesignChaProperty[] runtime = new DesignChaProperty[Mathf.Min(2, values.Length)];
        for (int i = 0; i < runtime.Length; i++)
        {
            runtime[i] = ToDesignChaProperty(values[i]);
        }

        return runtime;
    }

    private static DesignEventCall ToDesignEventCall<T>(T eventHandler, object[] args, Dictionary<string, T> registry, string label) where T : class
    {
        DesignEventCall result = new DesignEventCall();
        result.eventId = ResolveEventId(registry, eventHandler, label);
        result.arguments = ToTimelineArguments(args);
        return result;
    }

    private static string ResolveBulletEventId<T>(Dictionary<string, T> map, T value)
    {
        return ResolveEventId(map, value, "bullet");
    }

    private static string ResolveAoeEventId<T>(Dictionary<string, T> map, T value)
    {
        return ResolveEventId(map, value, "aoe");
    }

    private static string ResolveTimelineEventId(TimelineEvent value)
    {
        if (value == null)
        {
            return "";
        }

        if (DesignerScripts.TimelineScript.functions == null)
        {
            return "";
        }

        foreach (KeyValuePair<string, TimelineEvent> pair in DesignerScripts.TimelineScript.functions)
        {
            if (EqualityComparer<TimelineEvent>.Default.Equals(pair.Value, value))
            {
                return pair.Key;
            }
        }

        Debug.LogWarning("[GameDesignCatalog] Failed to resolve timeline event id for legacy mirror: " + value.Method.Name);
        return "";
    }

    private static string ResolveEventId<T>(Dictionary<string, T> map, T value, string label)
    {
        if (map == null)
        {
            return "";
        }

        if (value == null)
        {
            return "";
        }

        foreach (KeyValuePair<string, T> pair in map)
        {
            if (EqualityComparer<T>.Default.Equals(pair.Value, value))
            {
                return pair.Key;
            }
        }

        Debug.LogWarning("[GameDesignCatalog] Failed to resolve " + label + " event id for legacy mirror: " + value);
        return "";
    }

    private static DesignTimelineArgument[] ToTimelineArguments(object[] values)
    {
        if (values == null || values.Length <= 0)
        {
            return new DesignTimelineArgument[0];
        }

        DesignTimelineArgument[] runtime = new DesignTimelineArgument[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            runtime[i] = ToTimelineArgument(values[i]);
        }

        return runtime;
    }

    private static DesignTimelineArgument ToTimelineArgument(object value)
    {
        DesignTimelineArgument argument = new DesignTimelineArgument();
        if (value == null)
        {
            argument.kind = DesignValueKind.Null;
            return argument;
        }

        if (value is int intValue)
        {
            argument.kind = DesignValueKind.Int;
            argument.intValue = intValue;
            return argument;
        }

        if (value is float floatValue)
        {
            argument.kind = DesignValueKind.Float;
            argument.floatValue = floatValue;
            return argument;
        }

        if (value is bool boolValue)
        {
            argument.kind = DesignValueKind.Bool;
            argument.boolValue = boolValue;
            return argument;
        }

        if (value is string stringValue)
        {
            argument.kind = DesignValueKind.String;
            argument.stringValue = stringValue;
            return argument;
        }

        if (value is Vector3 vector3Value)
        {
            argument.kind = DesignValueKind.Vector3;
            argument.vector3Value = vector3Value;
            return argument;
        }

        if (value is ChaResource resourceValue)
        {
            argument.kind = DesignValueKind.ChaResource;
            argument.resourceValue = ToDesignChaResource(resourceValue);
            return argument;
        }

        if (value is ChaProperty propertyValue)
        {
            argument.kind = DesignValueKind.ChaProperty;
            argument.propertyValue = ToDesignChaProperty(propertyValue);
            return argument;
        }

        if (value is Damage damageValue)
        {
            argument.kind = DesignValueKind.Damage;
            argument.damageValue = ToDesignDamage(damageValue);
            return argument;
        }

        if (value is AddBuffInfo addBuffInfo)
        {
            argument.kind = DesignValueKind.AddBuffInfo;
            argument.addBuffValue = ToDesignAddBuffInfo(addBuffInfo);
            return argument;
        }

        if (value is BulletLauncher bulletLauncher)
        {
            argument.kind = DesignValueKind.BulletLauncher;
            argument.bulletLauncherValue = ToDesignBulletLauncher(bulletLauncher);
            return argument;
        }

        if (value is AoeLauncher aoeLauncher)
        {
            argument.kind = DesignValueKind.AoeLauncher;
            argument.aoeLauncherValue = ToDesignAoeLauncher(aoeLauncher);
            return argument;
        }

        if (value is Array arrayValue)
        {
            return ToArrayArgument(arrayValue);
        }

        argument.kind = DesignValueKind.Null;
        Debug.LogWarning("[GameDesignCatalog] Unsupported legacy mirror argument type: " + value.GetType().FullName);
        return argument;
    }

    private static DesignChaResource ToDesignChaResource(ChaResource value)
    {
        DesignChaResource result = new DesignChaResource();
        if (value == null)
        {
            return result;
        }

        result.hp = value.hp;
        result.ammo = value.ammo;
        result.stamina = value.stamina;
        return result;
    }

    private static DesignChaProperty ToDesignChaProperty(ChaProperty value)
    {
        DesignChaProperty result = new DesignChaProperty();
        result.moveSpeed = value.moveSpeed;
        result.hp = value.hp;
        result.ammo = value.ammo;
        result.attack = value.attack;
        result.actionSpeed = value.actionSpeed;
        result.bodyRadius = value.bodyRadius;
        result.hitRadius = value.hitRadius;
        result.moveType = value.moveType;
        return result;
    }

    private static DesignDamage ToDesignDamage(Damage value)
    {
        DesignDamage result = new DesignDamage();
        result.bullet = value.bullet;
        result.explosion = value.explosion;
        result.mental = value.mental;
        return result;
    }

    private static DesignTimelineArgument ToArrayArgument(Array values)
    {
        DesignTimelineArgument argument = new DesignTimelineArgument();
        argument.kind = DesignValueKind.Array;
        if (values == null || values.Length <= 0)
        {
            argument.arrayElementKind = DesignValueKind.Null;
            argument.arrayValues = new DesignTimelineArgument[0];
            return argument;
        }

        DesignValueKind elementKind = InferArrayElementKind(values.GetValue(0));
        DesignTimelineArgument[] runtimeValues = new DesignTimelineArgument[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            runtimeValues[i] = ToTimelineArgument(values.GetValue(i));
        }

        argument.arrayElementKind = elementKind;
        argument.arrayValues = runtimeValues;
        return argument;
    }

    private static DesignValueKind InferArrayElementKind(object value)
    {
        if (value == null)
        {
            return DesignValueKind.Null;
        }

        if (value is int)
        {
            return DesignValueKind.Int;
        }

        if (value is float)
        {
            return DesignValueKind.Float;
        }

        if (value is bool)
        {
            return DesignValueKind.Bool;
        }

        if (value is string)
        {
            return DesignValueKind.String;
        }

        if (value is Vector3)
        {
            return DesignValueKind.Vector3;
        }

        if (value is ChaResource)
        {
            return DesignValueKind.ChaResource;
        }

        if (value is ChaProperty)
        {
            return DesignValueKind.ChaProperty;
        }

        if (value is Damage)
        {
            return DesignValueKind.Damage;
        }

        if (value is AddBuffInfo)
        {
            return DesignValueKind.AddBuffInfo;
        }

        if (value is BulletLauncher)
        {
            return DesignValueKind.BulletLauncher;
        }

        if (value is AoeLauncher)
        {
            return DesignValueKind.AoeLauncher;
        }

        if (value is Array)
        {
            return DesignValueKind.Array;
        }

        return DesignValueKind.Null;
    }

    private static DesignAddBuffInfo ToDesignAddBuffInfo(AddBuffInfo value)
    {
        DesignAddBuffInfo result = new DesignAddBuffInfo();
        result.buffId = value.buffModel.id;
        result.stack = value.addStack;
        result.duration = value.duration;
        result.durationSetTo = value.durationSetTo;
        result.permanent = value.permanent;
        result.buffParam = ToDesignKeyValues(value.buffParam);
        return result;
    }

    private static DesignBulletLauncher ToDesignBulletLauncher(BulletLauncher value)
    {
        DesignBulletLauncher result = new DesignBulletLauncher();
        result.bulletId = value.model.id;
        result.speed = value.speed;
        result.duration = value.duration;
        result.canHitAfterCreated = value.canHitAfterCreated;
        result.tweenId = ResolveBulletEventId(BulletScript.bulletTween, value.tween);
        result.targetFuncId = ResolveBulletEventId(BulletScript.targettingFunc, value.targetFunc);
        result.useFireDegreeForever = value.useFireDegreeForever;
        result.param = ToDesignKeyValues(value.param);
        return result;
    }

    private static DesignAoeLauncher ToDesignAoeLauncher(AoeLauncher value)
    {
        DesignAoeLauncher result = new DesignAoeLauncher();
        result.aoeId = value.model.id;
        result.position = value.position;
        result.radius = value.radius;
        result.duration = value.duration;
        result.degree = value.degree;
        result.tweenId = ResolveAoeEventId(AoEScript.aoeTweenFunc, value.tween);
        result.tweenParam = ToTimelineArguments(value.tweenParam);
        result.param = ToDesignKeyValues(value.param);
        return result;
    }

    private static DesignKeyValue[] ToDesignKeyValues(Dictionary<string, object> values)
    {
        if (values == null || values.Count <= 0)
        {
            return new DesignKeyValue[0];
        }

        DesignKeyValue[] runtime = new DesignKeyValue[values.Count];
        int index = 0;
        foreach (KeyValuePair<string, object> pair in values)
        {
            runtime[index].key = pair.Key;
            runtime[index].value = ToTimelineArgument(pair.Value);
            index++;
        }

        return runtime;
    }

    private static object CreateEmptyArray(DesignValueKind elementKind)
    {
        switch (elementKind)
        {
            case DesignValueKind.String:
                return new string[0];
            case DesignValueKind.Int:
                return new int[0];
            case DesignValueKind.Float:
                return new float[0];
            case DesignValueKind.Bool:
                return new bool[0];
            case DesignValueKind.Vector3:
                return new Vector3[0];
            case DesignValueKind.ChaResource:
                return new ChaResource[0];
            case DesignValueKind.ChaProperty:
                return new ChaProperty[0];
            case DesignValueKind.Damage:
                return new Damage[0];
            case DesignValueKind.AddBuffInfo:
                return new AddBuffInfo[0];
            case DesignValueKind.BulletLauncher:
                return new BulletLauncher[0];
            case DesignValueKind.AoeLauncher:
                return new AoeLauncher[0];
            default:
                return new object[0];
        }
    }
}
