using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 阶段六状态视觉绑定器。状态逻辑不依赖渲染器，预制体可以按需挂载本组件。
/// </summary>
[DisallowMultipleComponent]
public class AbilityVisualStateBinder:MonoBehaviour{
    public bool hideWhenInvisible = true;
    public bool hideWhenUnderground = true;
    public GameObject undergroundEffect;
    public Renderer[] targetRenderers;

    private ChaState chaState;
    private readonly Dictionary<Renderer, bool> originalEnabled = new Dictionary<Renderer, bool>();

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
        bool hide = invisible || underground;
        foreach (KeyValuePair<Renderer, bool> pair in originalEnabled){
            if (pair.Key != null) pair.Key.enabled = pair.Value && !hide;
        }
        if (undergroundEffect != null) undergroundEffect.SetActive(underground);
    }

    private void CacheRenderers(){
        if (targetRenderers == null || targetRenderers.Length == 0) targetRenderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < targetRenderers.Length; i++){
            Renderer target = targetRenderers[i];
            if (target != null && !originalEnabled.ContainsKey(target)) originalEnabled.Add(target, target.enabled);
        }
    }

    private void OnDisable(){
        foreach (KeyValuePair<Renderer, bool> pair in originalEnabled){
            if (pair.Key != null) pair.Key.enabled = pair.Value;
        }
        if (undergroundEffect != null) undergroundEffect.SetActive(false);
    }
}
