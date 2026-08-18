using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TestManager : MonoBehaviour
{
    private static readonly string[] FallbackSkillIds = { "shoot1", "shoot2", "shoot3", "poisonShot", "stunShot", "laser", "lightning", "haste", "regen", "grenade", "roll", "fire", "fireMultiple" };
    private static readonly Dictionary<string, string> SkillDisplayTexts = new Dictionary<string, string>
    {
        { "shoot1", "一发子弹|单发直射" },
        { "shoot2", "二发子弹|两发扇形直射" },
        { "shoot3", "三发子弹|三发扇形直射" },
        { "poisonShot", "中毒弹|命中中毒、减速、持续伤害5秒" },
        { "stunShot", "眩晕弹|命中伤害并眩晕1.2秒" },
        { "laser", "激光|高速直线高伤害" },
        { "lightning", "闪电|追踪最近敌人并造成伤害" },
        { "haste", "加速|自身移动、行动加速5秒" },
        { "regen", "再生|自身持续回复5秒" },
        { "grenade", "手雷|范围爆炸伤害" },
        { "roll", "翻滚|向当前朝向快速位移" },
        { "fire", "普通射击|单发直线，消耗1发弹药" },
        { "fireMultiple", "三连射|三发扇形直射，消耗1发弹药" },
        { "spaceMonkeyBall", "太空猴球|范围攻击" },
        { "homingMissle", "追踪导弹|追踪敌人并造成伤害" },
        { "cloakBoomerang", "回旋镖|往返飞行造成伤害" },
        { "teleportBullet", "传送子弹|再次使用时传送至子弹位置" },
        { "explosiveBarrel", "爆炸桶|放置爆炸物" },
        { "debugInvincible", "调试无敌|短暂无敌测试" },
        { "debugStopMove", "调试停移|限制移动测试" },
        { "debugFlow", "流程测试|加速、激光、闪电、再生" },
        { "reload", "换弹|恢复弹药" }
    };

    [Header("Skill Panel")]
    public GameObject SkillNode;
    [Header("Skill Debug")]
    public string debugSkillId = "";
    public int debugSkillLevel = 1;
    public string[] cachedSkillIds = new string[0];

    private Transform skillListRoot;
    private Text skillSummaryText;
    private Font builtinFont;
    private bool skillPanelLayoutLogged;

    private void Start() { EnsureEventSystem(); }

    // Lists known skills and refreshes the debug panel.
    public void OnBtnClickListSkills() { RefreshSkillPanel(true); }

    // Learns the selected skill without casting it.
    public void OnBtnClickLearnSkill()
    {
        ChaState state = GameRuntime.MainActorState;
        SkillModel model;
        if (!EnsureActor(state, "learn") || !GameDatabase.TryGetSkill(debugSkillId, out model)) return;
        state.LearnSkill(model, Mathf.Max(1, debugSkillLevel));
        Debug.Log("[TestManager] Skill learned: " + debugSkillId);
        RefreshSkillPanel();
    }

    // Removes the selected skill from the main actor.
    public void OnBtnClickRemoveSkill()
    {
        ChaState state = GameRuntime.MainActorState;
        if (!EnsureActor(state, "remove")) return;
        Debug.Log("[TestManager] Remove skill " + debugSkillId + ": " + state.RemoveSkill(debugSkillId));
        RefreshSkillPanel();
    }

    // Learns and casts the selected skill for a Unity button binding.
    public void OnBtnClickUseSelectedSkill() { SelectLearnAndCast(debugSkillId); }

    // Spawns one configured enemy for combat testing.
    public void OnBtnClickAddEnemy()
    {
        GameObject enemy = GameRuntime.SpawnEnemy();
        Debug.Log(enemy ? "[TestManager] Spawned enemy: " + enemy.name : "[TestManager] Spawn enemy failed.");
    }

    // Clears all configured enemy-side characters.
    public void OnBtnClickClearEnemy()
    {
        Debug.Log("[TestManager] Cleared enemies: " + GameRuntime.ClearCharactersBySide(GameRuntime.Config.enemySide));
    }

    // Toggles debug invincibility on the main actor.
    public void OnBtnClickToggleInvincible()
    {
        ChaState state = GameRuntime.MainActorState;
        if (!EnsureActor(state, "toggle invincibility")) return;
        state.SetDebugInvincible(!state.IsInvincible);
        Debug.Log("[TestManager] Invincible = " + state.IsInvincible);
    }

    // Toggles main-actor movement control.
    public void OnBtnClickToggleStopMove()
    {
        ChaState state = GameRuntime.MainActorState;
        if (!EnsureActor(state, "toggle movement")) return;
        state.SetMoveEnabled(!state.IsMoveControlEnabled);
        Debug.Log("[TestManager] Move enabled = " + state.IsMoveControlEnabled);
    }

    // Opens or closes the skill debug panel.
    public void OnBtnClickAddSkill()
    {
        GameObject panel = EnsureSkillPanel();
        if (!panel) return;
        panel.SetActive(!panel.activeSelf);
        if (panel.activeSelf) RefreshSkillPanel();
    }

    // Forces a two-column skill-list refresh for a Unity button binding.
    public void OnBtnClickRefreshSkillPanel() { RefreshSkillPanel(true); }

    private GameObject EnsureSkillPanel()
    {
        EnsureEventSystem();
        if (!SkillNode)
        {
            GameObject canvas = GameObject.Find("Canvas");
            if (!canvas)
            {
                canvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            }
            SkillNode = new GameObject("SkillNode", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            SkillNode.transform.SetParent(canvas.transform, false);
            RectTransform rect = SkillNode.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.62f, 0.08f); rect.anchorMax = new Vector2(0.98f, 0.92f); rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            SkillNode.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);
        }
        ConfigureSkillPanelLayout(SkillNode);
        EnsureSkillPanelChildren(SkillNode.transform);
        return SkillNode;
    }

    /// <summary>
    /// Expands the runtime debug skill panel out of DebugTest's one-row SkillNode template.
    /// </summary>
    private void ConfigureSkillPanelLayout(GameObject panel)
    {
        Canvas canvas = panel.GetComponentInParent<Canvas>();
        if (!canvas)
        {
            Debug.LogWarning("[TestManager] Skill panel layout skipped because no Canvas was found.");
            return;
        }

        if (panel.transform.parent != canvas.transform)
        {
            panel.transform.SetParent(canvas.transform, false);
        }

        RectTransform rect = panel.GetComponent<RectTransform>();
        if (!rect)
        {
            Debug.LogWarning("[TestManager] Skill panel layout skipped because SkillNode has no RectTransform.");
            return;
        }

        rect.anchorMin = new Vector2(0.60f, 0.08f);
        rect.anchorMax = new Vector2(0.98f, 0.92f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image background = panel.GetComponent<Image>();
        if (background) background.color = new Color(0f, 0f, 0f, 0.72f);
        if (!skillPanelLayoutLogged)
        {
            skillPanelLayoutLogged = true;
            Debug.Log("[TestManager] Skill panel expanded to the Canvas right side for the two-column scroll list.");
        }
    }

    private void EnsureSkillPanelChildren(Transform panel)
    {
        Transform title = panel.Find("SkillTitle");
        if (!title)
        {
            title = CreateText(panel, "SkillTitle", "技能列表（点击学习并使用）", 22, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white).transform;
            RectTransform rect = title.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f); rect.anchorMax = new Vector2(1f, 1f); rect.pivot = new Vector2(.5f, 1f); rect.anchoredPosition = new Vector2(0f, -8f); rect.sizeDelta = new Vector2(0f, 34f);
        }
        Text titleText = title.GetComponent<Text>();
        if (titleText) titleText.text = "技能列表（点击学习并使用）";
        skillSummaryText = panel.Find("SkillSummary") ? panel.Find("SkillSummary").GetComponent<Text>() : null;
        if (!skillSummaryText)
        {
            skillSummaryText = CreateText(panel, "SkillSummary", "当前技能：-", 15, FontStyle.Normal, TextAnchor.MiddleLeft, Color.white);
            RectTransform rect = skillSummaryText.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f); rect.anchorMax = new Vector2(1f, 1f); rect.pivot = new Vector2(.5f, 1f); rect.anchoredPosition = new Vector2(0f, -44f); rect.sizeDelta = new Vector2(0f, 28f);
        }
        EnsureRuntimeSkillScrollView(panel);
    }

    // Keeps debug UI isolated from incomplete scene Scroll View objects.
    private void EnsureRuntimeSkillScrollView(Transform panel)
    {
        Transform root = panel.Find("RuntimeSkillScrollView");
        if (!root)
        {
            Transform legacy = panel.Find("SkillScrollView") ?? panel.Find("Scroll View");
            if (legacy) legacy.gameObject.SetActive(false);
            GameObject rootGO = new GameObject("RuntimeSkillScrollView", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect));
            rootGO.transform.SetParent(panel, false);
            root = rootGO.transform;
        }
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero; rootRect.anchorMax = Vector2.one; rootRect.offsetMin = new Vector2(12f, 12f); rootRect.offsetMax = new Vector2(-12f, -78f);
        ScrollRect scroll = root.GetComponent<ScrollRect>();
        Transform viewport = root.Find("Viewport");
        if (!viewport)
        {
            GameObject go = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
            go.transform.SetParent(root, false); viewport = go.transform;
        }
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero; viewportRect.anchorMax = Vector2.one; viewportRect.offsetMin = Vector2.zero; viewportRect.offsetMax = Vector2.zero;
        viewport.GetComponent<Mask>().showMaskGraphic = false;
        Transform content = viewport.Find("Content");
        if (!content)
        {
            GameObject go = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            go.transform.SetParent(viewport, false); content = go.transform;
        }
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f); contentRect.anchorMax = new Vector2(1f, 1f); contentRect.pivot = new Vector2(.5f, 1f); contentRect.anchoredPosition = Vector2.zero; contentRect.sizeDelta = Vector2.zero;
        GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 2; grid.startCorner = GridLayoutGroup.Corner.UpperLeft; grid.spacing = new Vector2(6f, 6f);
        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained; fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        Canvas.ForceUpdateCanvases();
        grid.cellSize = new Vector2(Mathf.Max(72f, (viewportRect.rect.width - 6f) * .5f), 64f);
        scroll.content = contentRect; scroll.viewport = viewportRect; scroll.horizontal = false; scroll.vertical = true; scroll.scrollSensitivity = 36f;
        skillListRoot = content;
    }

    private void RefreshSkillPanel(bool forceOpen = false)
    {
        GameObject panel = EnsureSkillPanel();
        if (!panel) return;
        if (forceOpen) panel.SetActive(true);
        cachedSkillIds = GameRuntime.GetSkillIds();
        if (cachedSkillIds == null || cachedSkillIds.Length == 0) cachedSkillIds = GetFallbackSkillIds();
        ClearChildren(skillListRoot);
        if (cachedSkillIds.Length == 0) { Debug.LogWarning("[TestManager] No test skill ids are available."); return; }
        ChaState state = GameRuntime.MainActorState;
        for (int i = 0; i < cachedSkillIds.Length; i++) CreateSkillButton(skillListRoot, cachedSkillIds[i], state != null && state.HasSkill(cachedSkillIds[i]));
        if (string.IsNullOrEmpty(debugSkillId)) debugSkillId = cachedSkillIds[0];
        if (skillSummaryText) skillSummaryText.text = "当前技能：" + GetSkillDisplayName(debugSkillId) + " | 技能数：" + cachedSkillIds.Length;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(skillListRoot.GetComponent<RectTransform>());
        Debug.Log("[TestManager] Skill panel refreshed, count=" + cachedSkillIds.Length + ", current=" + debugSkillId);
    }

    private void CreateSkillButton(Transform parent, string id, bool learned)
    {
        GameObject go = new GameObject("Skill_" + id, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>(); image.color = learned ? new Color(.18f, .42f, .22f, .95f) : new Color(.22f, .22f, .22f, .95f);
        Button button = go.GetComponent<Button>(); button.targetGraphic = image;
        Text label = CreateText(go.transform, "Label", (learned ? "已学 " : "") + GetSkillListText(id), 14, FontStyle.Normal, TextAnchor.MiddleLeft, Color.white);
        label.resizeTextForBestFit = true; label.resizeTextMinSize = 10; label.resizeTextMaxSize = 16;
        RectTransform rect = label.rectTransform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = new Vector2(8f, 3f); rect.offsetMax = new Vector2(-8f, -3f);
        button.onClick.AddListener(() => SelectLearnAndCast(id));
    }

    private void SelectLearnAndCast(string id)
    {
        debugSkillId = id;
        ChaState state = GameRuntime.MainActorState; SkillModel model;
        if (!EnsureActor(state, "use") || !GameDatabase.TryGetSkill(id, out model)) return;
        if (!state.LearnSkill(model, Mathf.Max(1, debugSkillLevel)))
        {
            Debug.LogWarning("[TestManager] Skill learn failed: " + id);
            return;
        }

        GameObject target = GetOrSpawnDebugSkillTarget();
        Vector3 targetPosition = target ? target.transform.position : state.transform.position + state.transform.up * 4f;
        Vector3 aimDirection = targetPosition - state.transform.position;
        if (aimDirection.sqrMagnitude <= 0.0001f) aimDirection = state.transform.right;
        AimDebugCasterAt(state, aimDirection);
        SkillCastRequest request = new SkillCastRequest(state, id, target, targetPosition, aimDirection);
        SkillCastResult result = state.TryCastSkill(request);
        Debug.Log(result.success
            ? "[TestManager] Skill learned and used: " + id + ", target=" + (target ? target.name : "position")
            : "[TestManager] Skill learned but use failed: " + id + ", reason=" + result.failureReason);
        RefreshSkillPanel();
        if (skillSummaryText)
        {
            skillSummaryText.text = result.success ? "已学习并使用：" + GetSkillDisplayName(id) : "使用失败：" + GetSkillDisplayName(id) + "（" + SkillCastFeedbackUtility.GetMessage(result.failureReason) + "）";
        }
    }

    // Provides a valid target for target, projectile, and area skills triggered from the debug list.
    private GameObject GetOrSpawnDebugSkillTarget()
    {
        GameObject[] enemies = GameRuntime.GetCharactersBySide(GameRuntime.Config.enemySide, true);
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i])
            {
                return enemies[i];
            }
        }

        GameObject enemy = GameRuntime.SpawnEnemy();
        Debug.Log(enemy
            ? "[TestManager] Auto-spawned an enemy for debug skill use: " + enemy.name
            : "[TestManager] Failed to auto-spawn an enemy for debug skill use.");
        return enemy;
    }

    /// <summary>
    /// Provides a two-line Chinese label containing the debug skill name and its verified effect.
    /// </summary>
    private static string GetSkillListText(string id)
    {
        return GetSkillDisplayText(id).Replace("|", "\n");
    }

    /// <summary>
    /// Extracts the Chinese skill name for summaries and cast feedback.
    /// </summary>
    private static string GetSkillDisplayName(string id)
    {
        string text = GetSkillDisplayText(id);
        int separatorIndex = text.IndexOf('|');
        return separatorIndex >= 0 ? text.Substring(0, separatorIndex) : text;
    }

    /// <summary>
    /// Resolves registered debug-skill text while retaining the id for unconfigured database entries.
    /// </summary>
    private static string GetSkillDisplayText(string id)
    {
        string text;
        if (SkillDisplayTexts.TryGetValue(id, out text))
        {
            return text;
        }

        return "未命名技能（" + id + "）|已注册，可用于施放流程测试";
    }

    /// <summary>
    /// Immediately faces the debug caster toward the supplied direction before a timeline fires a straight projectile.
    /// </summary>
    private static void AimDebugCasterAt(ChaState state, Vector3 aimDirection)
    {
        if (!state || aimDirection.sqrMagnitude <= 0.0001f) return;
        float degree = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        state.OrderRotateTo(degree);
        state.transform.rotation = Quaternion.Euler(0f, 0f, degree);
        Debug.Log("[TestManager] Debug caster aimed at " + degree + " degrees for a straight projectile.");
    }

    private string[] GetFallbackSkillIds()
    {
        List<string> ids = new List<string>();
        for (int i = 0; i < FallbackSkillIds.Length; i++) if (GameDatabase.HasSkill(FallbackSkillIds[i])) ids.Add(FallbackSkillIds[i]);
        Debug.Log("[TestManager] Fallback skills: " + string.Join(", ", ids.ToArray()));
        return ids.ToArray();
    }

    private void ClearChildren(Transform parent)
    {
        if (!parent) return;
        for (int i = parent.childCount - 1; i >= 0; i--) Destroy(parent.GetChild(i).gameObject);
    }

    private Text CreateText(Transform parent, string name, string value, int size, FontStyle style, TextAnchor anchor, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); go.transform.SetParent(parent, false);
        Text text = go.GetComponent<Text>(); text.font = GetFont(); text.text = value; text.fontSize = size; text.fontStyle = style; text.alignment = anchor; text.color = color; text.raycastTarget = false; return text;
    }

    private Font GetFont() { if (!builtinFont) builtinFont = Resources.GetBuiltinResource<Font>("Arial.ttf"); return builtinFont; }
    private bool EnsureActor(ChaState state, string action) { if (state) return true; Debug.LogWarning("[TestManager] Cannot " + action + ": main actor is missing."); return false; }
    private void EnsureEventSystem() { if (!Object.FindObjectOfType<EventSystem>()) new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)); }
}