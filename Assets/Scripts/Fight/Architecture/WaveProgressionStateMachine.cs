/// <summary>单局波次阶段；每一波先生成完，再等待场上敌人全部死亡。</summary>
public enum WaveProgressionState
{
    NotStarted,
    Spawning,
    Clearing,
    Intermission,
    Completed
}

/// <summary>
/// 波次推进的纯逻辑状态机。
/// 刷怪数量、精英/Boss 规则由 MobSpawnManager 决定，本类只保证阶段顺序和清场条件正确。
/// </summary>
public sealed class WaveProgressionStateMachine
{
    private readonly int waveCount;
    private readonly float intermissionDuration;
    private float intermissionRemaining;
    private WaveProgressionState state = WaveProgressionState.NotStarted;
    private int currentWave;

    public WaveProgressionState State { get { return state; } }
    public int CurrentWave { get { return currentWave; } }
    public bool IsCompleted { get { return state == WaveProgressionState.Completed; } }

    public WaveProgressionStateMachine(int waveCount, float intermissionDuration)
    {
        this.waveCount = UnityEngine.Mathf.Max(0, waveCount);
        this.intermissionDuration = UnityEngine.Mathf.Max(0f, intermissionDuration);
    }

    /// <summary>开始第一波；零波次直接视为已完成，避免配置错误导致战局永不结束。</summary>
    public void Begin()
    {
        if (waveCount <= 0)
        {
            state = WaveProgressionState.Completed;
            currentWave = 0;
            return;
        }

        currentWave = 1;
        state = WaveProgressionState.Spawning;
    }

    /// <summary>当前波达到生成配额后进入清场阶段。</summary>
    public bool MarkSpawnQuotaReached()
    {
        if (state != WaveProgressionState.Spawning) return false;
        state = WaveProgressionState.Clearing;
        return true;
    }

    /// <summary>只有场上没有存活敌人时才允许离开清场阶段；最终波清场后直接完成。</summary>
    public bool MarkCleared(int aliveEnemyCount)
    {
        if (state != WaveProgressionState.Clearing || aliveEnemyCount > 0) return false;
        if (currentWave >= waveCount)
        {
            state = WaveProgressionState.Completed;
            return true;
        }

        state = WaveProgressionState.Intermission;
        intermissionRemaining = intermissionDuration;
        return true;
    }

    /// <summary>推进波间休息；返回 true 表示下一波已开始生成。</summary>
    public bool Tick(float deltaTime)
    {
        if (state != WaveProgressionState.Intermission) return false;
        intermissionRemaining -= UnityEngine.Mathf.Max(0f, deltaTime);
        if (intermissionRemaining > 0f) return false;

        currentWave += 1;
        state = WaveProgressionState.Spawning;
        return true;
    }
}
