using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人攻击冷却的纯逻辑计时器。
/// 只有当前仍看得见目标、目标在攻击距离内且自身允许施法时才推进冷却，
/// 防止敌人隔墙、追查声音或被眩晕时提前攒好一次攻击。
/// </summary>
public sealed class EnemyAttackTiming
{
    private float remainingCooldown;

    public float RemainingCooldown { get { return remainingCooldown; } }

    public EnemyAttackTiming(float initialCooldown)
    {
        remainingCooldown = Mathf.Max(0f, initialCooldown);
    }

    /// <summary>
    /// 推进攻击冷却，并在本帧可开始攻击时返回 true。
    /// 调用方在真正提交攻击时间轴后必须调用 Restart，避免同一攻击连续触发。
    /// </summary>
    public bool Advance(
        bool hasVisibleTarget,
        float targetDistanceSqr,
        float attackRange,
        bool canUseSkill,
        float deltaTime)
    {
        float validRange = Mathf.Max(0.01f, attackRange);
        if (!hasVisibleTarget || !canUseSkill || targetDistanceSqr > validRange * validRange)
        {
            return false;
        }

        remainingCooldown = Mathf.Max(0f, remainingCooldown - Mathf.Max(0f, deltaTime));
        return remainingCooldown <= 0f;
    }

    /// <summary>攻击已开始后重置下一次攻击冷却。</summary>
    public void Restart(float cooldown)
    {
        remainingCooldown = Mathf.Max(0.01f, cooldown);
    }
}

/// <summary>
/// 轻量 AI：只提交移动、转向和技能意图；被控制时完全让出控制权。
/// </summary>
public class SimpleAI:MonoBehaviour{
    [Header("Attack")]
    [Tooltip("敌人当前看得见目标时，进入此距离才会开火；超出距离仍会继续追击。")]
    [SerializeField, Min(0.1f)] private float attackRange = 8.0f;

    [Tooltip("敌人首次发现可攻击目标后的开火延迟（秒），避免生成瞬间造成伤害。")]
    [SerializeField, Min(0.0f)] private float initialAttackDelay = 0.75f;

    [Tooltip("每次开火后的随机冷却范围（秒）。")]
    [SerializeField] private Vector2 attackCooldownRange = new Vector2(2.0f, 5.0f);

    private float toNextRotate = 2.0f;
    private float moveDegree;
    private readonly EnemyAwarenessStateMachine awareness = new EnemyAwarenessStateMachine();

    private TimelineModel fire = new TimelineModel("", new TimelineNode[]{
        new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
        new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
        new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle","Effect/MuzzleFlash","",false}),
        new TimelineNode(0.10f, "FireBullet", new object[]{
            new BulletLauncher(
                DesingerTables.BulletData.data["bullet2d"], null, Vector3.zero, 0, 6.0f, 10.0f, 0,
                // 普通子弹应固定使用射出时朝向，避免敌人之后转身让子弹拐弯。
                null, null, true
            ), "Muzzle"
        }),
        new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
    }, 0.50f, TimelineGoTo.Null);

    private ChaState chaState;
    private EnemyAwarenessProfile awarenessProfile;
    private SoundAlertProfile soundAlertProfile;
    private EnemyAttackTiming attackTiming;
    private int lastConsumedSoundAlertSequence;

    /// <summary>供战斗调试和后续行为树读取当前感知阶段。</summary>
    public EnemyAwarenessState AwarenessState { get { return awareness.State; } }

    void Start() {
        chaState = this.gameObject.GetComponent<ChaState>();
        awarenessProfile = this.gameObject.GetComponent<EnemyAwarenessProfile>();
        soundAlertProfile = this.gameObject.GetComponent<SoundAlertProfile>();
        attackTiming = new EnemyAttackTiming(initialAttackDelay);
        moveDegree = this.transform.rotation.eulerAngles.z;
    }

    private void FixedUpdate() {
        if (!chaState || chaState.dead == true || chaState.IsControlled) return;

        float timePassed = Time.fixedDeltaTime;
        // 所有 AI 锁定都经由侦测配置和统一目标服务，不能再直接回退锁定主角，
        // 否则隐身英雄会在没有侦测能力的敌人面前失效。
        ChaState target = TargetService.FindNearestDetectedEnemy(chaState);
        EnemyAwarenessSettings awarenessSettings = awarenessProfile != null
            ? awarenessProfile.Settings
            : EnemyAwarenessSettings.Default;
        // 视觉目标始终优先：即使附近刚有爆炸，只要当前看得见敌人就继续追击，
        // 防止声音事件把已经确认的目标意图错误覆盖掉。
        if (target == null)
        {
            SoundAlertSettings soundSettings = soundAlertProfile != null
                ? soundAlertProfile.Settings
                : SoundAlertSettings.Default;
            SoundAlert soundAlert;
            if (SoundAlertService.TryGetLatestHostileAlert(
                transform.position,
                chaState.side,
                soundSettings.hearingRange,
                lastConsumedSoundAlertSequence,
                out soundAlert))
            {
                awareness.InvestigateAlert(soundAlert.position, awarenessSettings);
                lastConsumedSoundAlertSequence = soundAlert.sequence;
            }
        }
        EnemyAwarenessDecision awarenessDecision = awareness.Advance(
            target != null,
            target != null ? target.transform.position : Vector3.zero,
            transform.position,
            timePassed,
            awarenessSettings);

        float rotateTo = chaState.transform.rotation.eulerAngles.z;
        Vector3 desiredMove = Vector3.zero;
        if (target != null)
        {
            Vector3 faceVec = target.transform.position - this.transform.position;
            rotateTo = Mathf.Atan2(faceVec.y, faceVec.x) * 180.00f / Mathf.PI;
            desiredMove = faceVec;
        }
        else if (awarenessDecision.shouldMoveToInterest)
        {
            // 目标隐身、离开侦测范围或听到敌对声音时，只能前往最后一次确认的位置，
            // 不保留目标引用，也不会继续追踪目标的实时坐标。
            desiredMove = awarenessDecision.interestPosition - transform.position;
            rotateTo = Mathf.Atan2(desiredMove.y, desiredMove.x) * 180.00f / Mathf.PI;
        }
        else
        {
            // 搜索抵达最后位置后原地转向观察；搜索结束再恢复旧的随机巡逻移动。
            toNextRotate -= timePassed;
            if (toNextRotate <= 0) {
                moveDegree += Random.Range(-90.00f, 90.00f);
                toNextRotate = Random.Range(1.60f, 3.20f);
            }
            rotateTo = moveDegree;
            if (awarenessDecision.state == EnemyAwarenessState.Patrol)
            {
                float patrolRadians = moveDegree * Mathf.PI / 180;
                desiredMove = new Vector3(Mathf.Cos(patrolRadians), Mathf.Sin(patrolRadians), 0);
            }
        }
        chaState.OrderRotateTo(rotateTo);
        float mSpd = chaState.moveSpeed;
        chaState.OrderMove(desiredMove.sqrMagnitude > 0.0001f ? desiredMove.normalized * mSpd : Vector3.zero);

        TryStartAttack(target, timePassed);
    }

    /// <summary>
    /// 只攻击本帧通过统一侦测服务确认的目标：隐身、遮挡、丢失视野或仅听到声音时都不会开火。
    /// 真正的伤害仍由既有 BulletState 和 DamageManager 处理，避免 AI 绕过战斗结算链路。
    /// </summary>
    private void TryStartAttack(ChaState target, float timePassed)
    {
        if (attackTiming == null) attackTiming = new EnemyAttackTiming(initialAttackDelay);

        bool hasVisibleTarget = target != null && !target.dead;
        float targetDistanceSqr = hasVisibleTarget
            ? (target.transform.position - transform.position).sqrMagnitude
            : float.PositiveInfinity;
        if (!attackTiming.Advance(
            hasVisibleTarget,
            targetDistanceSqr,
            attackRange,
            chaState.controlState.canUseSkill,
            timePassed))
        {
            return;
        }

        float minimumCooldown = Mathf.Max(0.01f, Mathf.Min(attackCooldownRange.x, attackCooldownRange.y));
        float maximumCooldown = Mathf.Max(minimumCooldown, Mathf.Max(attackCooldownRange.x, attackCooldownRange.y));
        attackTiming.Restart(Random.Range(minimumCooldown, maximumCooldown));
        SceneVariants.CreateTimeline(fire, gameObject, null);
        Debug.Log("[SimpleAI] Attack started: " + gameObject.name + " -> " + target.gameObject.name);
    }
}
