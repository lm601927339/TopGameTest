using System;
using System.Collections.Generic;

/// <summary>
/// 英雄内容的启动期契约验证。它只报告配置问题，不阻断旧玩家配置回退，方便策划在 Console 中定位问题。
/// </summary>
public static class HeroRuntimeContractVerifier
{
    public static void AppendWarnings(GameDesignConfig config, List<string> warnings)
    {
        if (config == null || warnings == null) return;

        HeroDefinition[] builtInHeroes = HeroCatalog.GetBuiltInHeroes();
        HashSet<string> builtInIds = new HashSet<string>();
        for (int i = 0; i < builtInHeroes.Length; i++)
        {
            HeroDefinition hero = builtInHeroes[i];
            if (hero == null || !builtInIds.Add(hero.id))
            {
                warnings.Add("Built-in hero id is empty or duplicated: " + (hero != null ? hero.id : "null"));
                continue;
            }
            AppendHeroWarnings(hero, "built-in", warnings);
        }

        HashSet<string> configuredIds = new HashSet<string>();
        if (config.heroDefinitions != null)
        {
            for (int i = 0; i < config.heroDefinitions.Length; i++)
            {
                HeroDefinition hero = config.heroDefinitions[i];
                if (hero == null)
                {
                    warnings.Add("heroDefinitions contains a null entry at index " + i + ".");
                    continue;
                }
                if (!configuredIds.Add(hero.id)) warnings.Add("Duplicate configured hero id: " + hero.id);
                AppendHeroWarnings(hero, "configured", warnings);
            }
        }

        HeroDefinition selectedHero;
        if (!config.TryGetSelectedHero(out selectedHero)) warnings.Add("Unknown selectedHeroId: " + config.selectedHeroId);
    }

    private static void AppendHeroWarnings(HeroDefinition hero, string source, List<string> warnings)
    {
        if (hero == null) return;
        if (!hero.IsStructurallyValid)
        {
            warnings.Add(source + " hero is structurally invalid: " + hero.id);
            return;
        }

        if (!GameDatabase.HasPrefab(hero.prefabPath)) warnings.Add("Hero " + hero.id + " references a missing prefab: " + hero.prefabPath);

        HashSet<string> skillIds = new HashSet<string>();
        string[] heroSkillIds = hero.skillIds ?? new string[0];
        for (int i = 0; i < heroSkillIds.Length; i++)
        {
            string skillId = heroSkillIds[i];
            if (string.IsNullOrEmpty(skillId)) warnings.Add("Hero " + hero.id + " contains an empty skill id.");
            else if (!skillIds.Add(skillId)) warnings.Add("Hero " + hero.id + " contains a duplicate skill id: " + skillId);
            else if (!GameDatabase.HasSkill(skillId)) warnings.Add("Hero " + hero.id + " references an unknown skill: " + skillId);
        }

        HashSet<string> buttons = new HashSet<string>();
        SkillBinding[] bindings = hero.skillBindings ?? new SkillBinding[0];
        for (int i = 0; i < bindings.Length; i++)
        {
            SkillBinding binding = bindings[i];
            if (string.IsNullOrEmpty(binding.buttonName)) warnings.Add("Hero " + hero.id + " contains an empty button binding.");
            else if (!buttons.Add(binding.buttonName)) warnings.Add("Hero " + hero.id + " contains a duplicate button: " + binding.buttonName);
            if (string.IsNullOrEmpty(binding.skillId)) warnings.Add("Hero " + hero.id + " binds an empty skill id.");
            else if (!skillIds.Contains(binding.skillId)) warnings.Add("Hero " + hero.id + " binds a skill it does not own: " + binding.skillId);
        }
    }
}
