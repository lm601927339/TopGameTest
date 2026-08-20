/// <summary>
/// 单局战斗的最终结果。结算后不允许再被后续伤害、延迟事件或重复回调覆盖。
/// </summary>
public enum BattleRunState
{
    InProgress,
    Defeat,
    Victory
}

/// <summary>
/// 纯逻辑的单局结果状态机。
/// GameManager 负责读取角色死亡和波次完成事件，本类只负责保证终局转换原子且可测试。
/// </summary>
public sealed class BattleRunStateMachine
{
    private BattleRunState state = BattleRunState.InProgress;

    public BattleRunState State { get { return state; } }
    public bool IsInProgress { get { return state == BattleRunState.InProgress; } }

    /// <summary>玩家死亡时切换失败；已经结算的战局保持原结果。</summary>
    public bool TryDefeat()
    {
        if (!IsInProgress) return false;
        state = BattleRunState.Defeat;
        return true;
    }

    /// <summary>只有玩家仍存活时才允许结算胜利，避免同帧死亡被错误判胜。</summary>
    public bool TryVictory(bool isPlayerAlive)
    {
        if (!IsInProgress || !isPlayerAlive) return false;
        state = BattleRunState.Victory;
        return true;
    }
}
