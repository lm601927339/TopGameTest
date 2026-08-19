using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 阶段六状态视觉绑定器。状态逻辑不依赖渲染器，预制体可以按需挂载本组件。
/// </summary>
[DisallowMultipleComponent]
public class AbilityVisualStateBinder:MonoBehaviour{
    public bool hideWhenInvisible = true;
    public bool hideWhenUnderground = true;
    [Tooltip("变羊时隐藏原角色并展示临时羊模型；可在角色预制体上指定正式美术预制体。")]
    public bool showPolymorphVisual = true;
    public GameObject polymorphVisualPrefab;
    public GameObject undergroundEffect;
    public Renderer[] targetRenderers;

    private ChaState chaState;
    private readonly Dictionary<Renderer, bool> originalEnabled = new Dictionary<Renderer, bool>();
    private GameObject polymorphVisual;
    private bool wasPolymorphed;

    /// <summary>任何角色被变羊时都能自动补齐视觉绑定器，不要求怪物预制体预先挂组件。</summary>
    public static void EnsureFor(ChaState state){
        if (state == null) return;
        AbilityVisualStateBinder binder = state.GetComponent<AbilityVisualStateBinder>();
        if (binder == null) binder = state.gameObject.AddComponent<AbilityVisualStateBinder>();
        binder.RefreshNow();
    }

    private void Awake(){
        chaState = GetComponent<ChaState>();
        CacheRenderers();
    }

    private void OnEnable(){
        if (chaState == null) chaState = GetComponent<ChaState>();
        CacheRenderers();
        RefreshNow();
    }

    private void LateUpdate(){
        RefreshNow();
    }

    /// <summary>立即刷新一次，编辑器预览和状态切换时可以主动调用。</summary>
    public void RefreshNow(){
        if (chaState == null) return;
        bool invisible = hideWhenInvisible && chaState.IsInvisible;
        bool underground = hideWhenUnderground && chaState.HasState(ChaAbilityStateType.Underground);
        bool polymorphed = showPolymorphVisual && chaState.HasState(ChaAbilityStateType.Polymorphed);
        bool hide = invisible || underground || polymorphed;
        foreach (KeyValuePair<Renderer, bool> pair in originalEnabled){
            if (pair.Key != null) pair.Key.enabled = pair.Value && !hide;
        }
        if (undergroundEffect != null) undergroundEffect.SetActive(underground);
        RefreshPolymorphVisual(polymorphed && !invisible && !underground);
        if (polymorphed && !wasPolymorphed) chaState.PlaySightEffect("Body", "Effect/Spark", "polymorph-enter", false);
        if (!polymorphed && wasPolymorphed) chaState.PlaySightEffect("Body", "Effect/Heart", "polymorph-exit", false);
        wasPolymorphed = polymorphed;
    }

    private void CacheRenderers(){
        if (targetRenderers == null || targetRenderers.Length == 0) targetRenderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < targetRenderers.Length; i++){
            Renderer target = targetRenderers[i];
            if (target != null && !IsPolymorphRenderer(target) && !originalEnabled.ContainsKey(target)) originalEnabled.Add(target, target.enabled);
        }
    }

    private bool IsPolymorphRenderer(Renderer renderer){
        return polymorphVisual != null && renderer != null && renderer.transform.IsChildOf(polymorphVisual.transform);
    }

    private void RefreshPolymorphVisual(bool visible){
        if (visible && polymorphVisual == null) polymorphVisual = CreatePolymorphVisual();
        if (polymorphVisual != null) polymorphVisual.SetActive(visible);
    }

    private GameObject CreatePolymorphVisual(){
        GameObject visual;
        if (polymorphVisualPrefab != null){
            visual = Instantiate(polymorphVisualPrefab, transform);
            visual.name = "PolymorphVisual";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            return visual;
        }

        // 正式羊预制体尚未导入时，用无碰撞的程序化模型保证变羊状态有可见反馈。
        visual = new GameObject("PolymorphFallbackSheep");
        visual.transform.SetParent(transform, false);
        CreateSheepPart(visual.transform, "Body", PrimitiveType.Sphere, new Vector3(0f, 0f, 0f), new Vector3(0.7f, 0.42f, 0.42f), new Color(0.92f, 0.87f, 0.70f));
        CreateSheepPart(visual.transform, "Head", PrimitiveType.Sphere, new Vector3(0.42f, 0.05f, 0f), new Vector3(0.33f, 0.29f, 0.29f), new Color(0.98f, 0.93f, 0.75f));
        CreateSheepPart(visual.transform, "EarLeft", PrimitiveType.Sphere, new Vector3(0.50f, 0.23f, 0.16f), new Vector3(0.16f, 0.10f, 0.08f), new Color(0.78f, 0.58f, 0.45f));
        CreateSheepPart(visual.transform, "EarRight", PrimitiveType.Sphere, new Vector3(0.50f, 0.23f, -0.16f), new Vector3(0.16f, 0.10f, 0.08f), new Color(0.78f, 0.58f, 0.45f));
        CreateSheepPart(visual.transform, "LegFront", PrimitiveType.Cylinder, new Vector3(0.25f, -0.32f, 0.20f), new Vector3(0.08f, 0.22f, 0.08f), new Color(0.62f, 0.48f, 0.34f));
        CreateSheepPart(visual.transform, "LegBack", PrimitiveType.Cylinder, new Vector3(-0.28f, -0.32f, -0.20f), new Vector3(0.08f, 0.22f, 0.08f), new Color(0.62f, 0.48f, 0.34f));
        return visual;
    }

    private static void CreateSheepPart(Transform parent, string partName, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color){
        GameObject part = GameObject.CreatePrimitive(primitiveType);
        part.name = partName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null) renderer.material.color = color;
    }

    private void OnDisable(){
        foreach (KeyValuePair<Renderer, bool> pair in originalEnabled){
            if (pair.Key != null) pair.Key.enabled = pair.Value;
        }
        if (undergroundEffect != null) undergroundEffect.SetActive(false);
        if (polymorphVisual != null) polymorphVisual.SetActive(false);
    }

    private void OnDestroy(){
        if (polymorphVisual != null) Destroy(polymorphVisual);
    }
}
