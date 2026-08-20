using System;
using UnityEngine;

/// <summary>
/// 统一角色死亡事件的载荷。事件在 ChaState 第一次进入死亡状态后立即发布，
/// 后续掉落、经验、波次计数、Boss 进度和统计系统只需订阅这一份数据。
/// </summary>
public sealed class CharacterDeathEvent
{
    public readonly int sequence;
    public readonly ChaState victim;
    public readonly GameObject victimObject;
    public readonly ChaState attacker;
    public readonly GameObject attackerObject;
    public readonly bool isPlayer;
    public readonly int victimSide;
    public readonly WaveEnemyMarker waveMarker;
    public readonly float time;

    public bool IsEnemy { get { return waveMarker != null || (!isPlayer && victimSide != 0); } }

    public CharacterDeathEvent(
        int sequence,
        ChaState victim,
        GameObject attackerObject,
        bool isPlayer,
        int victimSide,
        WaveEnemyMarker waveMarker,
        float time)
    {
        this.sequence = sequence;
        this.victim = victim;
        this.victimObject = victim != null ? victim.gameObject : null;
        this.attackerObject = attackerObject;
        this.attacker = attackerObject != null ? attackerObject.GetComponent<ChaState>() : null;
        this.isPlayer = isPlayer;
        this.victimSide = victimSide;
        this.waveMarker = waveMarker;
        this.time = time;
    }
}

/// <summary>
/// 角色死亡事件服务。发布者只允许 ChaState 调用，订阅者可来自波次、掉落、经验、Boss UI 等系统。
/// 事件按每个角色只发布一次；即使多个伤害、延迟 Buff 或脚本重复调用 Kill，也不会重复计数。
/// </summary>
public static class CharacterDeathService
{
    private static int nextSequence;

    public static event Action<CharacterDeathEvent> CharacterDied;

    /// <summary>供 ChaState 使用的统一发布入口；killer 可为空（环境、脚本或直接 Kill）。</summary>
    public static CharacterDeathEvent Publish(ChaState victim, GameObject killer)
    {
        if (victim == null) return null;

        GameObject mainActor = SceneVariants.MainActor();
        WaveEnemyMarker waveMarker = victim.GetComponent<WaveEnemyMarker>();
        CharacterDeathEvent death = new CharacterDeathEvent(
            ++nextSequence,
            victim,
            killer,
            victim.gameObject == mainActor,
            victim.side,
            waveMarker,
            Time.time);

        Action<CharacterDeathEvent> callbacks = CharacterDied;
        if (callbacks == null) return death;

        // 单个掉落或统计订阅者出错不能阻断死亡动画、尸体清理和其他订阅者。
        Delegate[] invocationList = callbacks.GetInvocationList();
        for (int i = 0; i < invocationList.Length; i++)
        {
            try
            {
                ((Action<CharacterDeathEvent>)invocationList[i])(death);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        return death;
    }

    /// <summary>仅供 EditMode 测试清理静态订阅，生产流程不应调用。</summary>
    public static void ResetForTests()
    {
        CharacterDied = null;
        nextSequence = 0;
    }
}
