using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>英雄职责分类。职责只用于内容配置和后续 AI/UI，不直接替代技能效果。</summary>
public enum HeroArchetype{
    Stealth = 0,
    Toxic = 1,
    SpellControl = 2,
    // 后续新增职业可先使用 Custom，并完全由英雄定义中的中文展示字段描述。
    Custom = 99
}

/// <summary>
/// 可配置的英雄模板。基础属性、技能和按键均归属于英雄，避免继续把所有角色能力写进 GameDesignConfig 的单玩家字段。
/// </summary>
[Serializable]
public class HeroDefinition{
    /// <summary>稳定且唯一的英雄 ID，用于配置选择和存档。</summary>
    public string id;
    /// <summary>供 UI 使用的中文显示名。</summary>
    public string displayName;
    /// <summary>英雄专长分类。</summary>
    public HeroArchetype archetype;
    /// <summary>英雄卡片显示的职业名；为空时才按 archetype 使用兼容默认文案。</summary>
    public string roleDisplayName;
    /// <summary>英雄卡片显示的专长摘要；新增英雄不再需要修改 UI 代码。</summary>
    [TextArea(1, 2)] public string specialtySummary;
    /// <summary>角色视图预制体路径；为空时兼容旧玩家预制体。</summary>
    public string prefabPath;
    /// <summary>角色动画表 ID。</summary>
    public string viewAnimInfo;
    /// <summary>英雄初始属性，不与其他英雄共享运行时数值。</summary>
    public ChaProperty baseProperty;
    /// <summary>英雄出生时学习的主动/被动技能 ID。</summary>
    public string[] skillIds = new string[0];
    /// <summary>英雄专属按键映射；为空时回退全局玩家映射。</summary>
    public SkillBinding[] skillBindings = new SkillBinding[0];
    /// <summary>英雄对异常状态的持续时间减免；数值由统一状态入口计算。</summary>
    public AbilityStateResistance[] stateResistances = new AbilityStateResistance[0];

    public HeroDefinition(
        string id,
        string displayName,
        HeroArchetype archetype,
        string prefabPath,
        string viewAnimInfo,
        ChaProperty baseProperty,
        string[] skillIds,
        SkillBinding[] skillBindings,
        AbilityStateResistance[] stateResistances = null,
        string roleDisplayName = "",
        string specialtySummary = ""
    ){
        this.id = id;
        this.displayName = displayName;
        this.archetype = archetype;
        this.prefabPath = prefabPath;
        this.viewAnimInfo = viewAnimInfo;
        this.baseProperty = baseProperty;
        this.skillIds = skillIds ?? new string[0];
        this.skillBindings = skillBindings ?? new SkillBinding[0];
        this.stateResistances = stateResistances ?? new AbilityStateResistance[0];
        this.roleDisplayName = roleDisplayName;
        this.specialtySummary = specialtySummary;
    }

    /// <summary>验证英雄定义的最小可运行条件；详细技能存在性由启动验证器检查。</summary>
    public bool IsStructurallyValid{
        get { return !string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(prefabPath); }
    }
}

/// <summary>
/// 首批三个英雄的内置模板。项目尚未创建 HeroCatalog 资产时仍可立即试玩；后续配置可在 GameDesignConfig.heroDefinitions 中覆盖同 ID。
/// </summary>
public static class HeroCatalog{
    public const string ShadowStalkerId = "shadow_stalker";
    public const string ToxicAlchemistId = "toxic_alchemist";
    public const string StormSageId = "storm_sage";
    public const string DefaultHeroId = ShadowStalkerId;

    private static readonly HeroDefinition[] builtInHeroes = new HeroDefinition[]{
        new HeroDefinition(
            ShadowStalkerId,
            "暗影潜行者",
            HeroArchetype.Stealth,
            "ninja",
            "Default_Gunner",
            new ChaProperty(185, 720, 80, 58, 120, 0.25f, 0.25f),
            new[]{ "shadowVeil", "cloakBoomerang", "roll" },
            new[]{
                new SkillBinding("Fire1", "shadowVeil"),
                new SkillBinding("Fire2", "cloakBoomerang"),
                new SkillBinding("Jump", "roll")
            },
            new[]{ new AbilityStateResistance(ChaAbilityStateType.Poisoned, 0.20f) },
            "潜行",
            "隐身、回旋镖、翻滚"
        ),
        new HeroDefinition(
            ToxicAlchemistId,
            "毒雾炼金师",
            HeroArchetype.Toxic,
            "ninja",
            "Default_Gunner",
            new ChaProperty(125, 1050, 120, 52, 95, 0.28f, 0.28f),
            new[]{ "poisonShot", "grenade", "regen" },
            new[]{
                new SkillBinding("Fire1", "poisonShot"),
                new SkillBinding("Fire2", "grenade"),
                new SkillBinding("Fire3", "regen")
            },
            new[]{
                new AbilityStateResistance(ChaAbilityStateType.Poisoned, 0.60f),
                new AbilityStateResistance(ChaAbilityStateType.Burning, 0.20f)
            },
            "毒系",
            "毒伤、减速、再生"
        ),
        new HeroDefinition(
            StormSageId,
            "风暴术士",
            HeroArchetype.SpellControl,
            "ninja",
            "Default_Gunner",
            new ChaProperty(110, 680, 160, 46, 115, 0.24f, 0.24f),
            new[]{ "spellPolymorph", "spellLightning", "spellFreeze", "haste" },
            new[]{
                new SkillBinding("Fire1", "spellPolymorph"),
                new SkillBinding("Fire2", "spellLightning"),
                new SkillBinding("Fire3", "spellFreeze"),
                new SkillBinding("Fire4", "haste")
            },
            new[]{
                new AbilityStateResistance(ChaAbilityStateType.Frozen, 0.35f),
                new AbilityStateResistance(ChaAbilityStateType.Stunned, 0.20f)
            },
            "法控",
            "变羊、闪电、冰冻、加速"
        )
    };

    /// <summary>先查询场景配置的覆盖项，再查询内置首批英雄。</summary>
    public static bool TryGet(GameDesignConfig config, string heroId, out HeroDefinition hero){
        string requestedId = string.IsNullOrEmpty(heroId) ? DefaultHeroId : heroId;
        if (config != null && config.heroDefinitions != null){
            for (int i = 0; i < config.heroDefinitions.Length; i++){
                HeroDefinition configured = config.heroDefinitions[i];
                if (configured != null && configured.id == requestedId && configured.IsStructurallyValid){
                    hero = configured;
                    return true;
                }
            }
        }
        for (int i = 0; i < builtInHeroes.Length; i++){
            if (builtInHeroes[i].id == requestedId){
                hero = builtInHeroes[i];
                return true;
            }
        }
        hero = null;
        return false;
    }

    /// <summary>返回内置英雄快照供启动验证和配置 UI 枚举，调用方不得修改内容定义。</summary>
    public static HeroDefinition[] GetBuiltInHeroes(){
        return (HeroDefinition[])builtInHeroes.Clone();
    }

    /// <summary>
    /// 返回选择界面可展示的有效英雄：内置英雄保持既定排序，配置同 ID 会覆盖内置定义，
    /// 其余有效配置英雄追加在末尾。这样新增英雄不需要再修改 UI 代码。
    /// </summary>
    public static HeroDefinition[] GetAvailableHeroes(GameDesignConfig config){
        List<HeroDefinition> heroes = new List<HeroDefinition>();
        HashSet<string> includedIds = new HashSet<string>();

        for (int i = 0; i < builtInHeroes.Length; i++){
            HeroDefinition builtInHero = builtInHeroes[i];
            if (builtInHero == null || string.IsNullOrEmpty(builtInHero.id)) continue;

            HeroDefinition effectiveHero;
            if (TryGet(config, builtInHero.id, out effectiveHero) && includedIds.Add(effectiveHero.id)){
                heroes.Add(effectiveHero);
            }
        }

        if (config == null || config.heroDefinitions == null) return heroes.ToArray();
        for (int i = 0; i < config.heroDefinitions.Length; i++){
            HeroDefinition configuredHero = config.heroDefinitions[i];
            if (configuredHero == null || !configuredHero.IsStructurallyValid || !includedIds.Add(configuredHero.id)) continue;
            heroes.Add(configuredHero);
        }
        return heroes.ToArray();
    }

    /// <summary>获取英雄卡片职业文案；旧英雄未填展示字段时仍可按原分类显示。</summary>
    public static string GetRoleDisplayName(HeroDefinition hero){
        if (hero != null && !string.IsNullOrEmpty(hero.roleDisplayName)) return hero.roleDisplayName;
        if (hero == null) return "未知";
        if (hero.archetype == HeroArchetype.Stealth) return "潜行";
        if (hero.archetype == HeroArchetype.Toxic) return "毒系";
        if (hero.archetype == HeroArchetype.SpellControl) return "法控";
        return "自定义";
    }

    /// <summary>获取英雄卡片专长摘要；正式新增英雄应填写 specialtySummary。</summary>
    public static string GetSpecialtySummary(HeroDefinition hero){
        if (hero != null && !string.IsNullOrEmpty(hero.specialtySummary)) return hero.specialtySummary;
        if (hero == null) return "未配置";
        if (hero.archetype == HeroArchetype.Stealth) return "隐身、回旋镖、翻滚";
        if (hero.archetype == HeroArchetype.Toxic) return "毒伤、减速、再生";
        if (hero.archetype == HeroArchetype.SpellControl) return "变羊、闪电、冰冻、加速";
        return "待配置英雄专长";
    }
}

/// <summary>
/// 挂在英雄实体上的运行时身份。玩家控制器从这里获取专属按键，角色创建时从这里装配属性和技能。
/// </summary>
[DisallowMultipleComponent]
public class HeroRuntime:MonoBehaviour{
    [SerializeField] private string heroId;
    [SerializeField] private string heroDisplayName;
    [SerializeField] private HeroArchetype archetype;

    private HeroDefinition definition;

    public string HeroId{ get { return heroId; } }
    public string HeroDisplayName{ get { return heroDisplayName; } }
    public HeroArchetype Archetype{ get { return archetype; } }

    /// <summary>把英雄模板写入实体并学习其技能；重复调用不会重复添加同一技能。</summary>
    public bool Configure(HeroDefinition value){
        if (value == null || !value.IsStructurallyValid) return false;
        ChaState state = GetComponent<ChaState>();
        if (state == null) return false;

        definition = value;
        heroId = value.id;
        heroDisplayName = value.displayName;
        archetype = value.archetype;
        state.InitBaseProp(value.baseProperty);

        AbilityStateResistanceProfile resistanceProfile = GetComponent<AbilityStateResistanceProfile>();
        if (resistanceProfile == null) resistanceProfile = gameObject.AddComponent<AbilityStateResistanceProfile>();
        resistanceProfile.Configure(value.stateResistances);

        string[] skillIds = value.skillIds ?? new string[0];
        for (int i = 0; i < skillIds.Length; i++){
            SkillModel skill;
            string skillId = skillIds[i];
            if (!string.IsNullOrEmpty(skillId) && GameDatabase.TryGetSkill(skillId, out skill)) state.LearnSkill(skill);
        }

        // 隐身和后续羊化等视觉状态统一由绑定器读取 ChaState，不让技能直接操作 Renderer。
        if (GetComponent<AbilityVisualStateBinder>() == null) gameObject.AddComponent<AbilityVisualStateBinder>();
        return true;
    }

    /// <summary>取英雄专属按键；返回 false 表示继续使用全局兼容按键。</summary>
    public bool TryGetSkillBindings(out SkillBinding[] bindings){
        bindings = definition != null ? definition.skillBindings : null;
        return bindings != null && bindings.Length > 0;
    }
}
