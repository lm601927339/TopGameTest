using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 玩家血量旁的异常状态图标。图标只读取 ChaState，不修改战斗状态，因此可安全替换为正式美术 HUD。
/// </summary>
[DisallowMultipleComponent]
public class AbilityStatusHud : MonoBehaviour
{
    private class StatusIcon
    {
        public ChaAbilityStateType stateType;
        public string shortName;
        public Image background;
        public Text label;

        public StatusIcon(ChaAbilityStateType stateType, string shortName)
        {
            this.stateType = stateType;
            this.shortName = shortName;
        }
    }

    private readonly StatusIcon[] icons = new[]
    {
        new StatusIcon(ChaAbilityStateType.Invisible, "隐"),
        new StatusIcon(ChaAbilityStateType.Poisoned, "毒"),
        new StatusIcon(ChaAbilityStateType.Frozen, "冰"),
        new StatusIcon(ChaAbilityStateType.Stunned, "晕"),
        new StatusIcon(ChaAbilityStateType.Polymorphed, "羊"),
        new StatusIcon(ChaAbilityStateType.Invincible, "无")
    };

    private ChaState target;
    private RectTransform root;
    private Font builtinFont;
    private bool built;

    private void Awake()
    {
        EnsureBuilt();
    }

    /// <summary>绑定当前玩家状态；重复调用只更新引用，不会重复创建图标对象。</summary>
    public void Bind(ChaState state)
    {
        target = state;
    }

    private void LateUpdate()
    {
        Refresh();
    }

    private void EnsureBuilt()
    {
        if (built) return;
        built = true;
        builtinFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        GameObject rootObject = new GameObject("AbilityStatusIcons", typeof(RectTransform));
        rootObject.transform.SetParent(transform, false);
        root = rootObject.GetComponent<RectTransform>();
        root.anchorMin = new Vector2(0.5f, 0f);
        root.anchorMax = new Vector2(0.5f, 0f);
        root.pivot = new Vector2(0.5f, 1f);
        root.anchoredPosition = new Vector2(0f, -8f);
        root.sizeDelta = new Vector2(192f, 28f);

        for (int i = 0; i < icons.Length; i++)
        {
            CreateIcon(icons[i], i);
        }
    }

    private void CreateIcon(StatusIcon icon, int index)
    {
        GameObject iconObject = new GameObject("Status_" + icon.stateType, typeof(RectTransform), typeof(Image));
        iconObject.transform.SetParent(root, false);
        RectTransform rect = iconObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(index * 31f, 0f);
        rect.sizeDelta = new Vector2(28f, 28f);
        icon.background = iconObject.GetComponent<Image>();
        icon.background.color = GetColor(icon.stateType);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(iconObject.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        icon.label = labelObject.GetComponent<Text>();
        icon.label.font = builtinFont;
        icon.label.fontSize = 12;
        icon.label.alignment = TextAnchor.MiddleCenter;
        icon.label.color = Color.white;
        iconObject.SetActive(false);
    }

    private void Refresh()
    {
        if (!built) EnsureBuilt();
        for (int i = 0; i < icons.Length; i++)
        {
            StatusIcon icon = icons[i];
            if (icon.background == null) continue;
            bool active = IsActive(icon.stateType);
            icon.background.gameObject.SetActive(active);
            if (active && icon.label != null) icon.label.text = icon.shortName + "\n" + GetDurationText(icon.stateType);
        }
    }

    private bool IsActive(ChaAbilityStateType stateType)
    {
        if (target == null || target.dead) return false;
        if (stateType == ChaAbilityStateType.Invincible) return target.IsInvincible;
        return target.HasState(stateType);
    }

    private string GetDurationText(ChaAbilityStateType stateType)
    {
        if (target == null || stateType == ChaAbilityStateType.Invincible) return "∞";
        float remaining = target.GetStateRemaining(stateType);
        if (remaining < 0f) return "∞";
        return Mathf.Max(1, Mathf.CeilToInt(remaining)).ToString();
    }

    private static Color GetColor(ChaAbilityStateType stateType)
    {
        if (stateType == ChaAbilityStateType.Invisible) return new Color(0.32f, 0.29f, 0.70f, 0.92f);
        if (stateType == ChaAbilityStateType.Poisoned) return new Color(0.20f, 0.58f, 0.24f, 0.92f);
        if (stateType == ChaAbilityStateType.Frozen) return new Color(0.20f, 0.58f, 0.86f, 0.92f);
        if (stateType == ChaAbilityStateType.Stunned) return new Color(0.95f, 0.63f, 0.16f, 0.92f);
        if (stateType == ChaAbilityStateType.Polymorphed) return new Color(0.82f, 0.43f, 0.75f, 0.92f);
        return new Color(0.86f, 0.76f, 0.18f, 0.92f);
    }
}
