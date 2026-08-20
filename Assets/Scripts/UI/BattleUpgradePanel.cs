using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 升级强化的运行时占位界面。当前阶段不依赖 UI 预制体：美术完成后可保留服务事件，替换本组件的创建逻辑。
/// </summary>
[DisallowMultipleComponent]
public class BattleUpgradePanel : MonoBehaviour
{
    private GameObject overlayObject;
    private Transform choicesRoot;
    private Text levelText;
    private Font builtinFont;
    private BattleUpgradeService upgradeService;

    /// <summary>在配置 Canvas 上创建/复用强化面板；没有 Canvas 时仅保留暂停和强化逻辑。</summary>
    public static BattleUpgradePanel Ensure(GameDesignConfig config, BattleUpgradeService service)
    {
        if (config == null || service == null) return null;
        GameObject canvasObject = GameRuntime.FindSceneObject(config.canvasObjectName);
        if (canvasObject == null || canvasObject.GetComponent<Canvas>() == null) return null;

        BattleUpgradePanel panel = canvasObject.GetComponent<BattleUpgradePanel>();
        if (panel == null) panel = canvasObject.AddComponent<BattleUpgradePanel>();
        panel.Bind(service);
        panel.EnsureBuilt();
        return panel;
    }

    private void OnDisable() { Unbind(); }

    private void Bind(BattleUpgradeService service)
    {
        if (upgradeService == service) return;
        Unbind();
        upgradeService = service;
        upgradeService.UpgradeOffered += Show;
        upgradeService.UpgradeClosed += Hide;
        if (upgradeService.IsAwaitingChoice) Show(upgradeService.CurrentSelection);
    }

    private void Unbind()
    {
        if (upgradeService == null) return;
        upgradeService.UpgradeOffered -= Show;
        upgradeService.UpgradeClosed -= Hide;
        upgradeService = null;
    }

    private void Show(BattleUpgradeSelection selection)
    {
        EnsureBuilt();
        if (overlayObject == null || selection == null) return;
        ClearChoices();

        BattleRewardState rewards = GameRuntime.BattleRewards != null ? GameRuntime.BattleRewards.State : null;
        levelText.text = rewards != null ? "等级提升 - Lv." + rewards.Level : "等级提升";
        for (int i = 0; i < selection.Candidates.Count; i++)
        {
            CreateChoiceButton(selection.Candidates[i], i);
        }
        overlayObject.SetActive(true);
        overlayObject.transform.SetAsLastSibling();
    }

    private void Hide()
    {
        if (overlayObject != null) overlayObject.SetActive(false);
    }

    private void EnsureBuilt()
    {
        if (overlayObject != null) return;
        builtinFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        EnsureEventSystem();

        overlayObject = new GameObject("BattleUpgradeOverlay", typeof(RectTransform), typeof(Image));
        overlayObject.transform.SetParent(transform, false);
        RectTransform overlay = overlayObject.GetComponent<RectTransform>();
        overlay.anchorMin = Vector2.zero;
        overlay.anchorMax = Vector2.one;
        overlay.offsetMin = Vector2.zero;
        overlay.offsetMax = Vector2.zero;
        overlayObject.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.07f, 0.90f);

        GameObject cardObject = new GameObject("UpgradeCard", typeof(RectTransform), typeof(Image));
        cardObject.transform.SetParent(overlay, false);
        RectTransform card = cardObject.GetComponent<RectTransform>();
        card.anchorMin = new Vector2(0.5f, 0.5f);
        card.anchorMax = new Vector2(0.5f, 0.5f);
        card.pivot = new Vector2(0.5f, 0.5f);
        card.sizeDelta = new Vector2(760f, 310f);
        cardObject.GetComponent<Image>().color = new Color(0.09f, 0.14f, 0.24f, 0.98f);

        levelText = CreateLabel(card, "LevelText", new Vector2(0f, 112f), new Vector2(660f, 42f), 28, FontStyle.Bold);
        levelText.color = new Color(0.98f, 0.84f, 0.28f);
        Text tipText = CreateLabel(card, "TipText", new Vector2(0f, 70f), new Vector2(660f, 30f), 17, FontStyle.Normal);
        tipText.text = "战斗已暂停，请选择一项强化";

        GameObject choicesObject = new GameObject("Choices", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        choicesObject.transform.SetParent(card, false);
        RectTransform choicesRect = choicesObject.GetComponent<RectTransform>();
        choicesRect.anchorMin = new Vector2(0.5f, 0.5f);
        choicesRect.anchorMax = new Vector2(0.5f, 0.5f);
        choicesRect.pivot = new Vector2(0.5f, 0.5f);
        choicesRect.anchoredPosition = new Vector2(0f, -38f);
        choicesRect.sizeDelta = new Vector2(700f, 160f);
        HorizontalLayoutGroup layout = choicesObject.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 14f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = false;
        layout.childControlWidth = false;
        choicesRoot = choicesObject.transform;
        overlayObject.SetActive(false);
    }

    private void ClearChoices()
    {
        if (choicesRoot == null) return;
        for (int i = choicesRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(choicesRoot.GetChild(i).gameObject);
        }
    }

    private void CreateChoiceButton(BattleUpgradeCandidate candidate, int index)
    {
        GameObject choiceObject = new GameObject("Choice_" + index, typeof(RectTransform), typeof(Image), typeof(Button));
        choiceObject.transform.SetParent(choicesRoot, false);
        RectTransform rect = choiceObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(220f, 145f);
        Image image = choiceObject.GetComponent<Image>();
        image.color = GetChoiceColor(candidate.kind);

        Button button = choiceObject.GetComponent<Button>();
        button.targetGraphic = image;
        int choiceIndex = index;
        button.onClick.AddListener(() => OnChoose(choiceIndex));

        Text title = CreateLabel(rect, "Title", new Vector2(0f, 42f), new Vector2(204f, 35f), 18, FontStyle.Bold);
        title.text = candidate.displayName;
        Text description = CreateLabel(rect, "Description", new Vector2(0f, -13f), new Vector2(196f, 68f), 15, FontStyle.Normal);
        description.text = candidate.description;
        description.horizontalOverflow = HorizontalWrapMode.Wrap;
        description.verticalOverflow = VerticalWrapMode.Overflow;
    }

    private void OnChoose(int index)
    {
        if (upgradeService == null || !upgradeService.TryChooseUpgrade(index))
        {
            Debug.LogWarning("[BattleUpgradePanel] Upgrade selection was rejected.");
        }
    }

    private Text CreateLabel(Transform parent, string objectName, Vector2 position, Vector2 size, int fontSize, FontStyle style)
    {
        GameObject labelObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(parent, false);
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Text text = labelObject.GetComponent<Text>();
        text.font = builtinFont;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        return text;
    }

    private static Color GetChoiceColor(BattleUpgradeKind kind)
    {
        if (kind == BattleUpgradeKind.SkillLevel) return new Color(0.20f, 0.38f, 0.72f, 1f);
        if (kind == BattleUpgradeKind.NewSkill) return new Color(0.45f, 0.24f, 0.70f, 1f);
        return new Color(0.18f, 0.48f, 0.38f, 1f);
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        GameObject eventSystemObject = new GameObject("BattleUpgradeEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        DontDestroyOnLoad(eventSystemObject);
    }
}
