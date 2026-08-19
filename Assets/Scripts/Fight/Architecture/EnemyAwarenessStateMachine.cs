using UnityEngine;

/// <summary>
/// 敌人感知阶段。追击只在当前可侦测到目标时成立；丢失目标后先搜索最后位置，
/// 搜索超时才回到巡逻，避免隐身单位让敌人立刻转身或继续错误追踪。
/// </summary>
public enum EnemyAwarenessState
{
    Patrol,
    Chase,
    Search
}

/// <summary>感知状态机的可配置参数。没有挂配置组件时使用 Default。</summary>
public struct EnemyAwarenessSettings
{
    public const float DefaultSearchDuration = 2.5f;
    public const float DefaultSearchArrivalDistance = 0.35f;

    public float searchDuration;
    public float searchArrivalDistance;

    public EnemyAwarenessSettings(float searchDuration, float searchArrivalDistance)
    {
        this.searchDuration = Mathf.Max(0f, searchDuration);
        this.searchArrivalDistance = Mathf.Max(0.01f, searchArrivalDistance);
    }

    public static EnemyAwarenessSettings Default
    {
        get { return new EnemyAwarenessSettings(DefaultSearchDuration, DefaultSearchArrivalDistance); }
    }
}

/// <summary>状态机给 AI 的移动意图；不直接修改角色，保持与控制权系统解耦。</summary>
public struct EnemyAwarenessDecision
{
    public EnemyAwarenessState state;
    public Vector3 interestPosition;
    public bool shouldMoveToInterest;

    public EnemyAwarenessDecision(EnemyAwarenessState state, Vector3 interestPosition, bool shouldMoveToInterest)
    {
        this.state = state;
        this.interestPosition = interestPosition;
        this.shouldMoveToInterest = shouldMoveToInterest;
    }
}

/// <summary>
/// 纯逻辑敌人感知状态机，便于 EditMode 测试且可被未来的复杂 EnemyBrain 复用。
/// 它只记录最后一次可见位置，不保存目标引用，因此隐身、死亡或销毁均能安全切换到搜索。
/// </summary>
public sealed class EnemyAwarenessStateMachine
{
    private EnemyAwarenessState state = EnemyAwarenessState.Patrol;
    private Vector3 lastSeenPosition;
    private bool hasLastSeenPosition;
    private float remainingSearchTime;

    public EnemyAwarenessState State { get { return state; } }
    public bool HasLastSeenPosition { get { return hasLastSeenPosition; } }
    public Vector3 LastSeenPosition { get { return lastSeenPosition; } }

    /// <summary>
    /// 根据这一帧的可见目标推进状态。
    /// visibleTargetPosition 仅在 hasVisibleTarget 为 true 时读取。
    /// </summary>
    public EnemyAwarenessDecision Advance(
        bool hasVisibleTarget,
        Vector3 visibleTargetPosition,
        Vector3 selfPosition,
        float deltaTime,
        EnemyAwarenessSettings settings)
    {
        if (hasVisibleTarget)
        {
            state = EnemyAwarenessState.Chase;
            lastSeenPosition = visibleTargetPosition;
            hasLastSeenPosition = true;
            remainingSearchTime = settings.searchDuration;
            return new EnemyAwarenessDecision(state, visibleTargetPosition, true);
        }

        if (!hasLastSeenPosition)
        {
            state = EnemyAwarenessState.Patrol;
            return new EnemyAwarenessDecision(state, selfPosition, false);
        }

        if (state != EnemyAwarenessState.Search)
        {
            state = EnemyAwarenessState.Search;
        }

        remainingSearchTime -= Mathf.Max(0f, deltaTime);
        if (remainingSearchTime <= 0f)
        {
            state = EnemyAwarenessState.Patrol;
            hasLastSeenPosition = false;
            return new EnemyAwarenessDecision(state, selfPosition, false);
        }

        float arrivalDistance = Mathf.Max(0.01f, settings.searchArrivalDistance);
        bool needsMove = (lastSeenPosition - selfPosition).sqrMagnitude > arrivalDistance * arrivalDistance;
        return new EnemyAwarenessDecision(state, lastSeenPosition, needsMove);
    }
}

/// <summary>
/// 可选敌人感知参数。暂不要求预制体挂载；普通敌人自动使用默认搜索时长。
/// </summary>
[DisallowMultipleComponent]
public class EnemyAwarenessProfile : MonoBehaviour
{
    [Tooltip("失去可见目标后，在最后可见位置附近搜索的时长（秒）。")]
    [SerializeField] private float searchDuration = EnemyAwarenessSettings.DefaultSearchDuration;

    [Tooltip("接近最后可见位置到此距离内后停止移动并原地搜索。")]
    [SerializeField] private float searchArrivalDistance = EnemyAwarenessSettings.DefaultSearchArrivalDistance;

    public EnemyAwarenessSettings Settings
    {
        get { return new EnemyAwarenessSettings(searchDuration, searchArrivalDistance); }
    }

    /// <summary>供生成器和测试调整搜索行为，不依赖预制体序列化配置。</summary>
    public void Configure(float newSearchDuration, float newSearchArrivalDistance)
    {
        searchDuration = Mathf.Max(0f, newSearchDuration);
        searchArrivalDistance = Mathf.Max(0.01f, newSearchArrivalDistance);
    }
}
