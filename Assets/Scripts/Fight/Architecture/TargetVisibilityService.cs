using UnityEngine;

/// <summary>目标不满足可见条件的原因，供 AI 调试、警戒 UI 和后续声音感知扩展复用。</summary>
public enum TargetVisibilityResult
{
    Visible,
    OutOfRange,
    OutsideViewAngle,
    BlockedByObstacle
}

/// <summary>侦测单位的视觉参数。距离、视野角和遮挡规则只在目标已经通过阵营/状态筛选后使用。</summary>
public struct TargetDetectionSettings
{
    public const float DefaultSightRange = 12f;
    public const float DefaultViewAngle = 120f;

    public float sightRange;
    public float viewAngle;
    public bool ignoreObstacleOcclusion;

    public TargetDetectionSettings(float sightRange, float viewAngle, bool ignoreObstacleOcclusion)
    {
        this.sightRange = Mathf.Max(0.01f, sightRange);
        this.viewAngle = Mathf.Clamp(viewAngle, 1f, 360f);
        this.ignoreObstacleOcclusion = ignoreObstacleOcclusion;
    }

    public static TargetDetectionSettings Default
    {
        get { return new TargetDetectionSettings(DefaultSightRange, DefaultViewAngle, false); }
    }
}

/// <summary>
/// 统一目标可见性判定。使用地图网格作为遮挡来源，保证视觉遮挡与实际移动阻挡一致；
/// 目标服务只在本类返回 Visible 后才把目标交给 AI。
/// </summary>
public static class TargetVisibilityService
{
    public static TargetVisibilityResult Evaluate(
        Vector3 observerPosition,
        Vector3 observerForward,
        Vector3 targetPosition,
        TargetDetectionSettings settings,
        float externalMaxDistance = float.PositiveInfinity)
    {
        Vector3 toTarget = targetPosition - observerPosition;
        toTarget.z = 0f;
        float distance = toTarget.magnitude;
        float allowedRange = Mathf.Min(settings.sightRange, externalMaxDistance);
        if (distance > allowedRange) return TargetVisibilityResult.OutOfRange;

        if (distance > 0.0001f && settings.viewAngle < 359.99f)
        {
            observerForward.z = 0f;
            if (observerForward.sqrMagnitude <= 0.0001f) observerForward = Vector3.right;
            float viewAngle = Vector3.Angle(observerForward, toTarget);
            if (viewAngle > settings.viewAngle * 0.5f) return TargetVisibilityResult.OutsideViewAngle;
        }

        if (!settings.ignoreObstacleOcclusion && IsBlockedByMap(observerPosition, targetPosition))
        {
            return TargetVisibilityResult.BlockedByObstacle;
        }

        return TargetVisibilityResult.Visible;
    }

    /// <summary>
    /// 战斗角色沿 XY 平面移动，地图网格同样以 X/Y 查询阻挡；没有初始化地图时不人为制造遮挡。
    /// 后续若引入动态门、烟雾或冰墙，可在此处叠加动态遮挡查询。
    /// </summary>
    public static bool IsBlockedByMap(Vector3 observerPosition, Vector3 targetPosition)
    {
        return SceneVariants.map != null && SceneVariants.map.HasGroundObstacleBetween(observerPosition, targetPosition);
    }
}
