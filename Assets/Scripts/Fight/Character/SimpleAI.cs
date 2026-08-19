using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 轻量 AI：只提交移动、转向和技能意图；被控制时完全让出控制权。
/// </summary>
public class SimpleAI:MonoBehaviour{
    private float toNextFire = 3.0f;
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
                null, null, false
            ), "Muzzle"
        }),
        new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
    }, 0.50f, TimelineGoTo.Null);

    private ChaState chaState;
    private EnemyAwarenessProfile awarenessProfile;
    private SoundAlertProfile soundAlertProfile;
    private int lastConsumedSoundAlertSequence;

    /// <summary>供战斗调试和后续行为树读取当前感知阶段。</summary>
    public EnemyAwarenessState AwarenessState { get { return awareness.State; } }

    void Start() {
        chaState = this.gameObject.GetComponent<ChaState>();
        awarenessProfile = this.gameObject.GetComponent<EnemyAwarenessProfile>();
        soundAlertProfile = this.gameObject.GetComponent<SoundAlertProfile>();
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

        // 仍保留旧射击时间轴；后续 EnemyBrain 可替换这里的技能意图生成。
        //toNextFire -= timePassed;
        //if (toNextFire <= 0){
        //    SceneVariants.CreateTimeline(fire, this.gameObject, null);
        //    toNextFire = Random.Range(2.00f, 5.00f);
        //}
    }
}
