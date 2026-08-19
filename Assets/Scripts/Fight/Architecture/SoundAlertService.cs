using System.Collections.Generic;
using UnityEngine;

/// <summary>战斗中可被 AI 调查的声音类型。声音只提供位置线索，不会暴露目标引用。</summary>
public enum SoundAlertKind
{
    Gunfire,
    Impact,
    Explosion
}

/// <summary>单次声音警报的运行时快照。序号用于让每个 AI 只消费一次同一条警报。</summary>
public struct SoundAlert
{
    public int sequence;
    public SoundAlertKind kind;
    public Vector3 position;
    public int sourceSide;
    public float range;
    public float expiresAt;

    public SoundAlert(int sequence, SoundAlertKind kind, Vector3 position, int sourceSide, float range, float expiresAt)
    {
        this.sequence = sequence;
        this.kind = kind;
        this.position = position;
        this.sourceSide = sourceSide;
        this.range = range;
        this.expiresAt = expiresAt;
    }
}

/// <summary>听觉参数。普通敌人不挂组件时使用默认值，便于后续为特殊怪物单独调节。</summary>
public struct SoundAlertSettings
{
    public const float DefaultHearingRange = 10f;

    public float hearingRange;

    public SoundAlertSettings(float hearingRange)
    {
        this.hearingRange = Mathf.Max(0.01f, hearingRange);
    }

    public static SoundAlertSettings Default
    {
        get { return new SoundAlertSettings(DefaultHearingRange); }
    }
}

/// <summary>
/// 轻量声音警报总线。它不播放音效，而是把开火、命中和爆炸转换为短时位置事件，
/// 供看不见目标的敌人调查。声音默认不受墙体阻挡，视觉遮挡仍由目标可见性服务处理。
/// </summary>
public static class SoundAlertService
{
    public const int NeutralSide = int.MinValue;

    private const float GunfireRange = 9f;
    private const float ImpactRange = 7f;
    private const float ExplosionRange = 14f;
    private const float GunfireDuration = 0.8f;
    private const float ImpactDuration = 0.6f;
    private const float ExplosionDuration = 1.2f;

    private static readonly List<SoundAlert> activeAlerts = new List<SoundAlert>();
    private static int nextSequence;

    /// <summary>记录一次实际开火；没有角色来源时按中立声音处理。</summary>
    public static int ReportGunfire(Vector3 position, GameObject source)
    {
        return Report(SoundAlertKind.Gunfire, position, GetSourceSide(source), GunfireRange, GunfireDuration);
    }

    /// <summary>记录一次有效受击；来源阵营决定哪些敌人应忽略该声音。</summary>
    public static int ReportImpact(Vector3 position, GameObject source)
    {
        return Report(SoundAlertKind.Impact, position, GetSourceSide(source), ImpactRange, ImpactDuration);
    }

    /// <summary>记录一次爆炸；爆炸使用更大的默认传播距离。</summary>
    public static int ReportExplosion(Vector3 position, GameObject source)
    {
        return Report(SoundAlertKind.Explosion, position, GetSourceSide(source), ExplosionRange, ExplosionDuration);
    }

    /// <summary>供战斗系统和 EditMode 测试提交自定义声音事件。</summary>
    public static int Report(SoundAlertKind kind, Vector3 position, int sourceSide, float range, float duration)
    {
        RemoveExpiredAlerts();
        int sequence = ++nextSequence;
        activeAlerts.Add(new SoundAlert(
            sequence,
            kind,
            position,
            sourceSide,
            Mathf.Max(0.01f, range),
            Time.time + Mathf.Max(0.01f, duration)));
        return sequence;
    }

    /// <summary>
    /// 查询监听者尚未处理的最新敌对声音。阵营相同的来源会被忽略；中立来源可被所有阵营调查。
    /// 听觉范围与声音传播范围取较小值，避免特殊怪物的配置绕过事件本身的传播距离。
    /// </summary>
    public static bool TryGetLatestHostileAlert(
        Vector3 listenerPosition,
        int listenerSide,
        float hearingRange,
        int minimumSequence,
        out SoundAlert alert)
    {
        RemoveExpiredAlerts();
        float listenerRange = Mathf.Max(0.01f, hearingRange);
        for (int i = activeAlerts.Count - 1; i >= 0; i--)
        {
            SoundAlert candidate = activeAlerts[i];
            if (candidate.sequence <= minimumSequence) continue;
            if (candidate.sourceSide != NeutralSide && candidate.sourceSide == listenerSide) continue;

            float allowedRange = Mathf.Min(listenerRange, candidate.range);
            Vector3 offset = candidate.position - listenerPosition;
            offset.z = 0f;
            if (offset.sqrMagnitude > allowedRange * allowedRange) continue;

            alert = candidate;
            return true;
        }

        alert = default(SoundAlert);
        return false;
    }

    /// <summary>场景重载和 EditMode 用例开始前清空短时缓存，避免旧战局的警报残留。</summary>
    public static void Clear()
    {
        activeAlerts.Clear();
        nextSequence = 0;
    }

    private static int GetSourceSide(GameObject source)
    {
        ChaState sourceState = source ? source.GetComponent<ChaState>() : null;
        return sourceState != null ? sourceState.side : NeutralSide;
    }

    private static void RemoveExpiredAlerts()
    {
        float now = Time.time;
        for (int i = activeAlerts.Count - 1; i >= 0; i--)
        {
            if (activeAlerts[i].expiresAt <= now) activeAlerts.RemoveAt(i);
        }
    }
}

/// <summary>可选听觉配置。普通敌人自动使用默认距离，不要求现有预制体补挂组件。</summary>
[DisallowMultipleComponent]
public class SoundAlertProfile : MonoBehaviour
{
    [Tooltip("敌人可调查声音的最大距离。最终距离还会受声音事件自身的传播范围限制。")]
    [SerializeField] private float hearingRange = SoundAlertSettings.DefaultHearingRange;

    public SoundAlertSettings Settings
    {
        get { return new SoundAlertSettings(hearingRange); }
    }

    /// <summary>供特殊敌人、关卡生成器和测试调整听觉距离，不依赖预制体序列化配置。</summary>
    public void Configure(float newHearingRange)
    {
        hearingRange = Mathf.Max(0.01f, newHearingRange);
    }
}
