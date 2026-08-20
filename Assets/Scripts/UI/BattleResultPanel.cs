using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 运行时生成的战局结算面板。
/// 暂不依赖正式 UI 预制体，确保失败和胜利流程可先完整验证；美术界面完成后可替换此组件的创建逻辑。
/// </summary>
[DisallowMultipleComponent]
public class BattleResultPanel : MonoBehaviour
{
    private GameObject overlayObject;
    private Text titleText;
    private Text detailText;
    private Font builtinFont;

    /// <summary>在配置的 Canvas 上创建或复用结算面板；场景无 Canvas 时安全跳过。</summary>
    public static BattleResultPanel Ensure(GameDesignConfig config)
    {
        if (config == null) return null;
        GameObject canvasObject = GameRuntime.FindSceneObject(config.canvasObjectName);
        if (canvasObject == null || canvasObject.GetComponent<Canvas>() == null) return null;

        BattleResultPanel resultPanel = canvasObject.GetComponent<BattleResultPanel>();
        if (resultPanel == null) resultPanel = canvasObject.AddComponent<BattleResultPanel>();
        resultPanel.EnsureBuilt();
        return resultPanel;
    }

    /// <summary>展示失败或胜利，并确保结算层覆盖现有的战斗/英雄选择 UI。</summary>
    public void Show(BattleRunState result, string detail)
    {
        EnsureBuilt();
        if (overlayObject == null) return;

        bool victory = result == BattleRunState.Victory;
        titleText.text = victory ? "战斗胜利" : "战斗失败";
        titleText.color = victory ? new Color(0.95f, 0.82f, 0.28f) : new Color(1f, 0.42f, 0.42f);
        detailText.text = string.IsNullOrEmpty(detail)
            ? (victory ? "已完成本局目标" : "英雄已倒下")
            : detail;
        overlayObject.SetActive(true);
        overlayObject.transform.SetAsLastSibling();
    }

    /// <summary>供运行时按钮绑定，重新加载当前场景开始新一局。</summary>
    public void OnBtnRestartBattle()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.buildIndex < 0)
        {
            Debug.LogWarning("[BattleResult] Active scene is not in Build Settings; restart was skipped.");
            return;
        }

        SceneManager.LoadScene(activeScene.buildIndex);
    }

    private void EnsureBuilt()
    {
        if (overlayObject != null) return;
        builtinFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        EnsureEventSystem();

        overlayObject = new GameObject("BattleResultOverlay", typeof(RectTransform), typeof(Image));
        overlayObject.transform.SetParent(transform, false);
        RectTransform overlay = overlayObject.GetComponent<RectTransform>();
        overlay.anchorMin = Vector2.zero;
        overlay.anchorMax = Vector2.one;
        overlay.offsetMin = Vector2.zero;
        overlay.offsetMax = Vector2.zero;
        overlayObject.GetComponent<Image>().color = new Color(0.015f, 0.02f, 0.04f, 0.88f);

        GameObject cardObject = new GameObject("ResultCard", typeof(RectTransform), typeof(Image));
        cardObject.transform.SetParent(overlay, false);
        RectTransform card = cardObject.GetComponent<RectTransform>();
        card.anchorMin = new Vector2(0.5f, 0.5f);
        card.anchorMax = new Vector2(0.5f, 0.5f);
        card.pivot = new Vector2(0.5f, 0.5f);
        card.anchoredPosition = Vector2.zero;
        card.sizeDelta = new Vector2(360f, 220f);
        cardObject.GetComponent<Image>().color = new Color(0.10f, 0.15f, 0.25f, 0.98f);

        titleText = CreateLabel(card, "Title", new Vector2(0f, 66f), new Vector2(330f, 42f), 28, FontStyle.Bold);
        detailText = CreateLabel(card, "Detail", new Vector2(0f, 13f), new Vector2(310f, 40f), 17, FontStyle.Normal);
        CreateRestartButton(card);
        overlayObject.SetActive(false);
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

    private void CreateRestartButton(Transform parent)
    {
        GameObject buttonObject = new GameObject("RestartButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -62f);
        rect.sizeDelta = new Vector2(190f, 48f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.18f, 0.48f, 0.83f, 1f);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(OnBtnRestartBattle);

        Text label = CreateLabel(rect, "Label", Vector2.zero, new Vector2(175f, 40f), 18, FontStyle.Bold);
        label.text = "重新开始";
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        GameObject eventSystemObject = new GameObject("BattleResultEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        DontDestroyOnLoad(eventSystemObject);
    }
}
