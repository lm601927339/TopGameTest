using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全局天气视野服务。天气控制器只登记自身状态，目标可见性服务从这里取得当前最严格的视野倍率，
/// 避免每个 AI 在 FixedUpdate 中反复扫描场景对象，也避免云雾天气只停留在配置字段里。
/// </summary>
public static class WeatherVisibilityService
{
    private static readonly List<WeatherController> activeControllers = new List<WeatherController>();

    /// <summary>登记启用中的天气控制器；重复登记不会重复影响视野。</summary>
    public static void Register(WeatherController controller)
    {
        if (controller != null && !activeControllers.Contains(controller)) activeControllers.Add(controller);
    }

    /// <summary>注销已禁用或销毁的天气控制器，恢复其对全局视野的影响。</summary>
    public static void Unregister(WeatherController controller)
    {
        if (controller != null) activeControllers.Remove(controller);
    }

    /// <summary>
    /// 返回当前全局视野距离倍率。多个降视野天气同时存在时取最小值，
    /// 不叠乘以免多个控制器意外把视野压到不可用；晴天或没有天气时恒为 1。
    /// </summary>
    public static float GetSightRangeMultiplier()
    {
        float multiplier = 1f;
        for (int i = activeControllers.Count - 1; i >= 0; i--)
        {
            WeatherController controller = activeControllers[i];
            if (controller == null || !controller.isActiveAndEnabled)
            {
                activeControllers.RemoveAt(i);
                continue;
            }

            multiplier = Mathf.Min(multiplier, controller.VisibilityRangeMultiplier);
        }
        return Mathf.Clamp(multiplier, 0.01f, 1f);
    }

#if UNITY_EDITOR
    /// <summary>仅供 EditMode 测试清空静态登记，避免上一条用例的对象影响下一条用例。</summary>
    public static void ResetForTests()
    {
        activeControllers.Clear();
    }
#endif
}
