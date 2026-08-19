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

    void Start() {
        chaState = this.gameObject.GetComponent<ChaState>();
        moveDegree = this.transform.rotation.eulerAngles.z;
    }

    private void FixedUpdate() {
        if (!chaState || chaState.dead == true || chaState.IsControlled) return;

        float timePassed = Time.fixedDeltaTime;
        ChaState target = TargetService.FindNearestEnemy(chaState);
        if (target == null)
        {
            GameObject mainActor = SceneVariants.MainActor();
            target = mainActor ? mainActor.GetComponent<ChaState>() : null;
        }

        float rotateTo = chaState.transform.rotation.eulerAngles.z;
        if (target != null)
        {
            Vector3 faceVec = target.transform.position - this.transform.position;
            rotateTo = Mathf.Atan2(faceVec.y, faceVec.x) * 180.00f / Mathf.PI;
        }

        toNextRotate -= timePassed;
        if (toNextRotate <= 0) {
            moveDegree += Random.Range(-90.00f, 90.00f);
            toNextRotate = Random.Range(1.60f, 3.20f);
        }
        chaState.OrderRotateTo(rotateTo);
        float rRadius = moveDegree * Mathf.PI / 180;
        float mSpd = chaState.moveSpeed;
        chaState.OrderMove(new Vector3(
            Mathf.Cos(rRadius) * mSpd,
            Mathf.Sin(rRadius) * mSpd,
            0
        ));

        // 阶段三先保留旧射击时间轴，后续 EnemyBrain 可替换这里的意图生成。
        //toNextFire -= timePassed;
        //if (toNextFire <= 0){
        //    SceneVariants.CreateTimeline(fire, this.gameObject, null);
        //    toNextFire = Random.Range(2.00f, 5.00f);
        //}
    }
}