using System.Collections.Generic;
using UnityEngine;

public static class GameStartupValidator
{
    public static bool Validate(GameDesignConfig config)
    {
        bool hasError = false;
        List<string> warnings = new List<string>();

        if (config == null)
        {
            Debug.LogError("[GameStartupValidator] Missing GameDesignConfig asset.");
            return false;
        }

        if (config.mapWidth <= 0 || config.mapHeight <= 0)
        {
            warnings.Add("Map size must be positive. Current: " + config.mapWidth + "x" + config.mapHeight);
        }

        if (config.tileSpacing <= 0)
        {
            warnings.Add("tileSpacing must be positive. Current: " + config.tileSpacing);
        }

        if (config.spawnPeriod <= 0)
        {
            warnings.Add("spawnPeriod must be positive. Current: " + config.spawnPeriod);
        }

        ValidatePrefab(config.playerPrefabPath, "playerPrefabPath", ref hasError, warnings);
        ValidatePrefab(config.characterRootPrefab, "characterRootPrefab", ref hasError, warnings);
        ValidatePrefab(config.bulletPrefabPath, "bulletPrefabPath", ref hasError, warnings);
        ValidatePrefab(config.aoePrefabPath, "aoePrefabPath", ref hasError, warnings);
        ValidatePrefab(config.wallPrefabPath, "wallPrefabPath", ref hasError, warnings);
        ValidatePrefab(config.popTextPrefabPath, "popTextPrefabPath", ref hasError, warnings);

        if (config.designCatalog != null)
        {
            config.designCatalog.AppendValidationWarnings(warnings);
        }
        else
        {
            warnings.Add("designCatalog is missing. Runtime will fall back to the legacy mirror until a formal catalog asset is assigned.");
        }

        if (config.startingSkills != null)
        {
            for (int i = 0; i < config.startingSkills.Length; i++)
            {
                string skillId = config.startingSkills[i];
                if (string.IsNullOrEmpty(skillId))
                {
                    warnings.Add("startingSkills contains an empty entry.");
                    continue;
                }

                if (!GameDatabase.HasSkill(skillId))
                {
                    warnings.Add("Unknown starting skill id: " + skillId);
                }
            }
        }

        if (config.playerSkillBindings != null)
        {
            HashSet<string> seenButtons = new HashSet<string>();
            for (int i = 0; i < config.playerSkillBindings.Length; i++)
            {
                SkillBinding binding = config.playerSkillBindings[i];
                if (string.IsNullOrEmpty(binding.buttonName))
                {
                    warnings.Add("playerSkillBindings contains an empty button name.");
                }
                else if (!seenButtons.Add(binding.buttonName))
                {
                    warnings.Add("Duplicate button binding: " + binding.buttonName);
                }
                if (string.IsNullOrEmpty(binding.skillId))
                {
                    warnings.Add("playerSkillBindings contains an empty skill id.");
                    continue;
                }
                if (!GameDatabase.HasSkill(binding.skillId))
                {
                    warnings.Add("Unknown bound skill id: " + binding.skillId + " for button " + binding.buttonName);
                }
            }
        }

        for (int i = 0; i < warnings.Count; i++)
        {
            Debug.LogWarning("[GameStartupValidator] " + warnings[i]);
        }

        return !hasError;
    }

    private static void ValidatePrefab(string prefabPath, string label, ref bool hasError, List<string> warnings)
    {
        if (string.IsNullOrEmpty(prefabPath))
        {
            warnings.Add(label + " is empty.");
            return;
        }

        if (!GameDatabase.HasPrefab(prefabPath))
        {
            warnings.Add("Missing prefab for " + label + ": " + prefabPath);
        }
    }
}
