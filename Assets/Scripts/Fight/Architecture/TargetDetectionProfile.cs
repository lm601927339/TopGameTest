using UnityEngine;

/// <summary>
/// 目标侦测能力配置。默认敌人不能锁定隐身目标；只有挂载本组件并启用侦测后，
/// 才会把隐身目标传给统一 TargetService，避免 AI 自行绕过目标筛选规则。
/// </summary>
[DisallowMultipleComponent]
public class TargetDetectionProfile : MonoBehaviour
{
    [Tooltip("是否能够锁定隐身单位。用于后续侦测怪、雷达、真视领域等玩法。")]
    [SerializeField] private bool canDetectInvisible;

    [Tooltip("是否能够锁定无敌单位。默认关闭，保留给特殊脚本目标或调试敌人。")]
    [SerializeField] private bool canTargetInvincible;

    [Header("视野")]
    [Tooltip("基础视觉侦测距离。普通敌人未挂本组件时使用 12 米默认值。")]
    [SerializeField] private float sightRange = TargetDetectionSettings.DefaultSightRange;

    [Range(1f, 360f)]
    [Tooltip("视野总角度，360 表示全向感知。")]
    [SerializeField] private float viewAngle = TargetDetectionSettings.DefaultViewAngle;

    [Tooltip("忽略地图阻挡。用于雷达、真视领域等特殊单位；普通敌人默认关闭。")]
    [SerializeField] private bool ignoreObstacleOcclusion;

    public bool CanDetectInvisible { get { return canDetectInvisible; } }
    public bool CanTargetInvincible { get { return canTargetInvincible; } }
    public TargetDetectionSettings Settings { get { return new TargetDetectionSettings(sightRange, viewAngle, ignoreObstacleOcclusion); } }

    /// <summary>供生成器和测试在运行时配置侦测规则，不要求预制体先增加字段。</summary>
    public void Configure(bool detectInvisible, bool targetInvincible = false)
    {
        canDetectInvisible = detectInvisible;
        canTargetInvincible = targetInvincible;
    }

    /// <summary>供特殊敌人、测试和关卡生成器设置距离、视野角与穿墙侦测，不要求先修改预制体。</summary>
    public void ConfigureVision(float newSightRange, float newViewAngle, bool newIgnoreObstacleOcclusion = false)
    {
        TargetDetectionSettings settings = new TargetDetectionSettings(newSightRange, newViewAngle, newIgnoreObstacleOcclusion);
        sightRange = settings.sightRange;
        viewAngle = settings.viewAngle;
        ignoreObstacleOcclusion = settings.ignoreObstacleOcclusion;
    }
}
