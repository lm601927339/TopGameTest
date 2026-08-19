using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 运行时生成的轻量英雄选择面板。它不依赖新预制体，因此美术完成正式界面前也能验证选择和存档链路。
/// </summary>
[DisallowMultipleComponent]
public class HeroSelectionPanel : MonoBehaviour
{
    [SerializeField] private bool reloadSceneAfterSelection = true;

    private GameDesignConfig config;
    private RectTransform panel;
    private Font builtinFont;

    /// <summary>由 GameManager 在现有 Canvas 上确保面板存在，场景没有 Canvas 时安全跳过。</summary>
    public static void Ensure(GameDesignConfig gameConfig)
    {
        if (gameConfig == null || !gameConfig.showHeroSelectionPanel) return;
        GameObject canvasObject = GameRuntime.FindSceneObject(gameConfig.canvasObjectName);
        if (canvasObject == null || canvasObject.GetComponent<Canvas>() == null) return;
        HeroSelectionPanel selectionPanel = canvasObject.GetComponent<HeroSelectionPanel>();
        if (selectionPanel == null) selectionPanel = canvasObject.AddComponent<HeroSelectionPanel>();
        selectionPanel.Configure(gameConfig);
    }

    /// <summary>供自定义 Canvas 或 Unity 按钮在运行时重新绑定配置。</summary>
    public void Configure(GameDesignConfig gameConfig)
    {
        config = gameConfig;
        EnsureEventSystem();
        if (panel == null) BuildPanel();
    }

    public void OnBtnSelectShadowStalker() { SelectAndReload(HeroCatalog.ShadowStalkerId); }
    public void OnBtnSelectToxicAlchemist() { SelectAndReload(HeroCatalog.ToxicAlchemistId); }
    public void OnBtnSelectStormSage() { SelectAndReload(HeroCatalog.StormSageId); }

    private void BuildPanel()
    {
        builtinFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        GameObject panelObject = new GameObject("HeroSelectionPanel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(transform, false);
        panel = panelObject.GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(0f, 1f);
        panel.anchorMax = new Vector2(0f, 1f);
        panel.pivot = new Vector2(0f, 1f);
        panel.anchoredPosition = new Vector2(18f, -18f);
        HeroDefinition[] heroes = HeroCatalog.GetAvailableHeroes(config);
        panel.sizeDelta = new Vector2(250f, 52f + heroes.Length * 64f);
        panelObject.GetComponent<Image>().color = new Color(0.04f, 0.07f, 0.12f, 0.86f);

        CreateLabel(panel, "Title", "选择英雄（点击后重开本局）", new Vector2(10f, -9f), new Vector2(230f, 28f), 16, FontStyle.Bold, TextAnchor.MiddleCenter);
        for (int i = 0; i < heroes.Length; i++)
        {
            HeroDefinition hero = heroes[i];
            if (hero == null) continue;
            CreateHeroButton(hero, i);
        }
    }

    private void CreateHeroButton(HeroDefinition hero, int index)
    {
        GameObject buttonObject = new GameObject("Hero_" + hero.id, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(panel, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(10f, -43f - index * 64f);
        rect.sizeDelta = new Vector2(230f, 56f);

        bool selected = config != null && config.selectedHeroId == hero.id;
        Image image = buttonObject.GetComponent<Image>();
        image.color = selected ? new Color(0.16f, 0.47f, 0.35f, 0.96f) : new Color(0.14f, 0.19f, 0.29f, 0.96f);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        string heroId = hero.id;
        button.onClick.AddListener(delegate { SelectAndReload(heroId); });

        // 由英雄定义提供展示内容，后续加职业无需改动选择界面。
        string label = hero.displayName + "  " + HeroCatalog.GetRoleDisplayName(hero) + "\n" + HeroCatalog.GetSpecialtySummary(hero);
        CreateLabel(rect, "Label", label, new Vector2(8f, -2f), new Vector2(214f, 52f), 13, FontStyle.Normal, TextAnchor.MiddleLeft);
    }

    private void SelectAndReload(string heroId)
    {
        if (!HeroSelectionService.SelectHero(config, heroId))
        {
            Debug.LogWarning("[HeroSelection] Cannot select hero: " + heroId);
            return;
        }

        Debug.Log("[HeroSelection] Selected hero: " + heroId);
        if (!reloadSceneAfterSelection) return;
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.buildIndex < 0)
        {
            Debug.Log("[HeroSelection] Hero selection was saved and will apply next time the battle scene starts.");
            return;
        }
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    private Text CreateLabel(Transform parent, string objectName, string content, Vector2 position, Vector2 size, int fontSize, FontStyle style, TextAnchor alignment)
    {
        GameObject labelObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(parent, false);
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Text text = labelObject.GetComponent<Text>();
        text.font = builtinFont;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = Color.white;
        text.text = content;
        return text;
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        GameObject eventSystemObject = new GameObject("HeroSelectionEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        DontDestroyOnLoad(eventSystemObject);
    }
}
