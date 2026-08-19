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

    public bool CanDetectInvisible { get { return canDetectInvisible; } }
    public bool CanTargetInvincible { get { return canTargetInvincible; } }

    /// <summary>供生成器和测试在运行时配置侦测规则，不要求预制体先增加字段。</summary>
    public void Configure(bool detectInvisible, bool targetInvincible = false)
    {
        canDetectInvisible = detectInvisible;
        canTargetInvincible = targetInvincible;
    }
}
