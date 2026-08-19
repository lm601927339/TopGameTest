using UnityEngine;

/// <summary>
/// 英雄选择的持久化服务。只保存稳定英雄 ID，不保存运行时属性、技能或 Unity 对象引用。
/// </summary>
public static class HeroSelectionService
{
    public const string PlayerPrefsKey = "buff-topgame.hero-selection.v1";

    /// <summary>把已保存且仍有效的选择写回当前配置；过期 ID 会被清除，避免阻断旧配置回退。</summary>
    public static void ApplySavedSelection(GameDesignConfig config)
    {
        if (config == null || !PlayerPrefs.HasKey(PlayerPrefsKey)) return;
        string savedHeroId = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
        HeroDefinition hero;
        if (HeroCatalog.TryGet(config, savedHeroId, out hero))
        {
            config.selectedHeroId = savedHeroId;
            return;
        }

        PlayerPrefs.DeleteKey(PlayerPrefsKey);
        PlayerPrefs.Save();
        Debug.LogWarning("[HeroSelection] Cleared an unknown saved hero id: " + savedHeroId);
    }

    /// <summary>选择英雄并立即持久化；返回 false 表示 ID 没有通过英雄目录验证。</summary>
    public static bool SelectHero(GameDesignConfig config, string heroId)
    {
        HeroDefinition hero;
        if (config == null || !HeroCatalog.TryGet(config, heroId, out hero)) return false;
        config.selectedHeroId = heroId;
        PlayerPrefs.SetString(PlayerPrefsKey, heroId);
        PlayerPrefs.Save();
        return true;
    }

    /// <summary>供测试和“恢复默认英雄”按钮使用，删除存档后配置回到内置默认英雄。</summary>
    public static void ClearSelection(GameDesignConfig config)
    {
        PlayerPrefs.DeleteKey(PlayerPrefsKey);
        PlayerPrefs.Save();
        if (config != null) config.selectedHeroId = HeroCatalog.DefaultHeroId;
    }
}
