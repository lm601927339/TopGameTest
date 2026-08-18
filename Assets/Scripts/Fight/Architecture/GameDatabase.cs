using System.Collections.Generic;
using System;
using UnityEngine;

public static class GameDatabase
{
    public static bool TryGetSkill(string id, out SkillModel value)
    {
        GameDesignCatalogAsset catalog = GetCatalog();
        if (catalog != null)
        {
            if (catalog.HasSkillDefinition(id))
            {
                return catalog.TryGetSkill(id, out value);
            }

            if (catalog.TryGetSkill(id, out value))
            {
                return true;
            }
        }

        return TryGet(DesingerTables.SkillData.data, id, "skill", out value);
    }

    public static bool TryGetBullet(string id, out BulletModel value)
    {
        GameDesignCatalogAsset catalog = GetCatalog();
        if (catalog != null)
        {
            if (catalog.HasBulletDefinition(id))
            {
                return catalog.TryGetBullet(id, out value);
            }

            if (catalog.TryGetBullet(id, out value))
            {
                return true;
            }
        }

        return TryGet(DesingerTables.BulletData.data, id, "bullet", out value);
    }

    public static bool TryGetAoE(string id, out AoeModel value)
    {
        GameDesignCatalogAsset catalog = GetCatalog();
        if (catalog != null)
        {
            if (catalog.HasAoeDefinition(id))
            {
                return catalog.TryGetAoE(id, out value);
            }

            if (catalog.TryGetAoE(id, out value))
            {
                return true;
            }
        }

        return TryGet(DesingerTables.AoEData.data, id, "aoe", out value);
    }

    public static bool TryGetBuff(string id, out BuffModel value)
    {
        GameDesignCatalogAsset catalog = GetCatalog();
        if (catalog != null)
        {
            if (catalog.HasBuffDefinition(id))
            {
                return catalog.TryGetBuff(id, out value);
            }

            if (catalog.TryGetBuff(id, out value))
            {
                return true;
            }
        }

        return TryGet(DesingerTables.BuffData.data, id, "buff", out value);
    }

    public static bool TryGetTimeline(string id, out TimelineModel value)
    {
        GameDesignCatalogAsset catalog = GetCatalog();
        if (catalog != null)
        {
            if (catalog.HasTimelineDefinition(id))
            {
                return catalog.TryGetTimeline(id, out value);
            }

            if (catalog.TryGetTimeline(id, out value))
            {
                return true;
            }
        }

        return TryGet(DesingerTables.TimelineData.data, id, "timeline", out value);
    }

    public static bool TryGetUnitAnimInfo(string id, out Dictionary<string, AnimInfo> value)
    {
        return TryGet(DesingerTables.UnitAnimInfo.data, id, "unit anim", out value);
    }

    public static bool HasSkill(string id)
    {
        SkillModel value;
        return TryGetSkill(id, out value);
    }

    public static string[] GetSkillIds()
    {
        List<string> ids = new List<string>();
        HashSet<string> seen = new HashSet<string>();

        GameDesignCatalogAsset catalog = GetCatalog();
        if (catalog != null && catalog.skills != null)
        {
            for (int i = 0; i < catalog.skills.Length; i++)
            {
                if (string.IsNullOrEmpty(catalog.skills[i].id))
                {
                    continue;
                }

                if (seen.Add(catalog.skills[i].id))
                {
                    ids.Add(catalog.skills[i].id);
                }
            }
        }

        if (DesingerTables.SkillData.data != null)
        {
            foreach (KeyValuePair<string, SkillModel> pair in DesingerTables.SkillData.data)
            {
                if (string.IsNullOrEmpty(pair.Key))
                {
                    continue;
                }

                if (seen.Add(pair.Key))
                {
                    ids.Add(pair.Key);
                }
            }
        }

        ids.Sort(StringComparer.Ordinal);
        return ids.ToArray();
    }

    public static bool HasPrefab(string prefabPath)
    {
        return GameRuntime.HasPrefab(prefabPath);
    }

    private static bool TryGet<T>(Dictionary<string, T> table, string id, string tableName, out T value)
    {
        value = default(T);
        if (table == null)
        {
            Debug.LogError("[GameDatabase] Missing " + tableName + " table.");
            return false;
        }

        if (string.IsNullOrEmpty(id))
        {
            Debug.LogError("[GameDatabase] Empty " + tableName + " id.");
            return false;
        }

        if (table.TryGetValue(id, out T found))
        {
            value = found;
            return true;
        }

        Debug.LogError("[GameDatabase] Unknown " + tableName + " id: " + id);
        return false;
    }

    private static GameDesignCatalogAsset GetCatalog()
    {
        // Keep this lookup centralized so catalog migration stays opt-in and easy to audit.
        return GameRuntime.Config != null ? GameRuntime.Config.designCatalog : null;
    }
}
