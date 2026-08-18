using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace DesignerScripts
{
    ///<summary>
    ///子弹的效果
    ///</summary>
    public class BulletScript{
        public static Dictionary<string, BulletOnCreate> onCreateFunc = new Dictionary<string, BulletOnCreate>(){
            {"RecordBullet", RecordBullet},
            {"SetBombBouncing", SetBombBouncing},
            {"SetDistanceForward",SetDistanceForward },
        };
        public static Dictionary<string, BulletOnHit> onHitFunc = new Dictionary<string, BulletOnHit>(){
            {"CommonBulletHit", CommonBulletHit},
            { "BulletBeatBackOnHit",BulletBeatBackOnHit},
            {"CreateAoEOnHit", CreateAoEOnHit},
            {"CloakBoomerangHit", CloakBoomerangHit},
            {"BulletSlowDownSpeedOnHit",BulletSlowDownSpeedOnHit },
            {"BulletPoisoningOnHit",BulletPoisoningOnHit },
            {"BulletPoisonSlowOnHit", BulletPoisonSlowOnHit},
            {"BulletStunOnHit", BulletStunOnHit},
        };
        public static Dictionary<string, BulletOnRemoved> onRemovedFunc = new Dictionary<string, BulletOnRemoved>(){
            {"CommonBulletRemoved", CommonBulletRemoved},
            {"CreateAoEOnRemoved", CreateAoEOnRemoved}
        };
        public static Dictionary<string, BulletTween> bulletTween = new Dictionary<string, BulletTween>(){
            
            {"FollowingTarget", FollowingTarget},
            {"CloakBoomerangTween", CloakBoomerangTween},
            {"NewCloakBoomerangTween",NewCloakBoomerangTween },
            {"SlowlyFaster", SlowlyFaster},
            {"BoomBallRolling", BoomBallRolling},
            {"StarFishMove",StarFishMove },
        };
        public static Dictionary<string, BulletTargettingFunction> targettingFunc = new Dictionary<string, BulletTargettingFunction>(){   
            {"GetNearestEnemy", GetNearestEnemy},
            {"BulletCasterSelf", BulletCasterSelf}
        };

        ///<summary>
        ///onHit
        ///普通子弹命中效果，参数：
        ///[0]伤害倍数
        ///[1]基础暴击率
        ///[2]命中视觉特效
        ///[3]播放特效位于目标的绑点，默认Body
        ///</summary>
        private static void CommonBulletHit(GameObject bullet, GameObject target){
            BulletState bulletState = bullet.GetComponent<BulletState>();
            if (!bulletState) return;
            object[] onHitParam = bulletState.model.onHitParams;
            float damageTimes = onHitParam.Length > 0 ? (float)onHitParam[0] : 1.00f;
            float critRate = onHitParam.Length > 1 ? (float) onHitParam[1] : 0.00f;
            string sightEffect = onHitParam.Length > 2 ? (string) onHitParam[2] : "";
            string bpName = onHitParam.Length > 3 ? (string) onHitParam[3] : "Body";
            Debug.Log("[BulletScript] CommonBulletHit bullet=" + bullet.name + " target=" + target.name + " damageTimes=" + damageTimes);
            if (sightEffect != ""){
                UnitBindManager ubm = target.GetComponent<UnitBindManager>();
                if (ubm){
                    ubm.AddBindGameObject(bpName, "Prefabs/"+sightEffect, "", false);
                }
            }
            SceneVariants.CreateDamage(
                bulletState.caster, 
                target,
                 new Damage(Mathf.CeilToInt(damageTimes * bulletState.propWhileCast.attack)),
                 //new Damage(20),
                bullet.transform.eulerAngles.z,
                critRate,
                new DamageInfoTag[]{DamageInfoTag.directDamage, }
            );
        }
        /// <summary>
        /// 子弹碰撞后添加击退效果 buff
        /// </summary>
        /// <param name="bullet"></param>
        /// <param name="target"></param>
        private static void BulletBeatBackOnHit(GameObject bullet, GameObject target) {
            BulletState bs = bullet.GetComponent<BulletState>();
            if (!bs) return;
            ChaState cs = target.GetComponent<ChaState>();
            if (!cs) return;
            cs.AddBuff(new AddBuffInfo(
                    DesingerTables.BuffData.data["BulletBeatBackOnHit"], bullet, target, 1, 0, true, true 
                ));

        }
        /// <summary>
        /// 子弹碰撞后添加减速效果 buff
        /// </summary>
        /// <param name="bullet"></param>
        /// <param name="target"></param>
        /// <summary>
        /// 命中后添加减速 buff，用于测试减速命中链路。
        /// </summary>
        public static void BulletSlowDownSpeedOnHit(GameObject bullet, GameObject target)
        {
            BulletState bs = bullet.GetComponent<BulletState>();
            if (!bs) return;
            ChaState cs = target.GetComponent<ChaState>();
            if (!cs) return;
            Debug.Log("[BulletScript] BulletSlowDownSpeedOnHit target=" + target.name);
            cs.AddBuff(new AddBuffInfo(
                    DesingerTables.BuffData.data["BulletSlowDownSpeedOnHit"], bullet, target, 1, 5, true, false
                ));
        }

        /// <summary>
        /// 子弹碰撞后添加中毒效果 buff
        /// </summary>
        /// <param name="bullet"></param>
        /// <param name="target"></param>
        /// <summary>
        /// 命中后添加中毒 buff，用于测试持续伤害链路。
        /// </summary>
        public static void BulletPoisoningOnHit(GameObject bullet, GameObject target)
        {
            BulletState bs = bullet.GetComponent<BulletState>();
            if (!bs) return;
            ChaState cs = target.GetComponent<ChaState>();
            if (!cs) return;
            Debug.Log("[BulletScript] BulletPoisoningOnHit target=" + target.name);
            cs.AddBuff(new AddBuffInfo(
                    DesingerTables.BuffData.data["BulletPoisoningOnHit"], bullet, target, 1, 5, true, false
                ));
        }

        /// <summary>
        /// 命中后添加“中毒+减速”复合 buff，用于测试完整异常状态流程。
        /// </summary>
        public static void BulletPoisonSlowOnHit(GameObject bullet, GameObject target)
        {
            BulletState bs = bullet.GetComponent<BulletState>();
            if (!bs) return;
            ChaState cs = target.GetComponent<ChaState>();
            if (!cs) return;
            Debug.Log("[BulletScript] BulletPoisonSlowOnHit target=" + target.name);
            cs.AddBuff(new AddBuffInfo(
                    DesingerTables.BuffData.data["SkillPoisonSlowOnHit"], bullet, target, 1, 5, true, false
                ));
        }

        /// <summary>
        /// 命中后添加眩晕 buff，用于测试控制状态命中链路。
        /// </summary>
        public static void BulletStunOnHit(GameObject bullet, GameObject target)
        {
            BulletState bs = bullet.GetComponent<BulletState>();
            if (!bs) return;
            ChaState cs = target.GetComponent<ChaState>();
            if (!cs) return;
            Debug.Log("[BulletScript] BulletStunOnHit target=" + target.name);
            cs.AddBuff(new AddBuffInfo(
                    DesingerTables.BuffData.data["SkillStunOnHit"], bullet, target, 1, 1.2f, true, false
                ));
        }
        ///<summary>
        ///onRemoved
        ///普通子结束，参数：
        ///[0]命中视觉特效
        ///</summary>
        private static void CommonBulletRemoved(GameObject bullet){
            BulletState bulletState = bullet.GetComponent<BulletState>();
            if (!bulletState) return;
            object[] onRemovedParams = bulletState.model.onRemovedParams;
            string sightEffect = onRemovedParams.Length > 0 ? (string)onRemovedParams[0] : "";
            if (sightEffect != ""){
                SceneVariants.CreateSightEffect(
                    sightEffect, 
                    bullet.transform.position, 
                    bullet.transform.rotation.eulerAngles.z
                );      
            }
        }

        ///<summary>
        ///targetting
        ///选择最近的敌人作为目标
        ///</summary>
        private static GameObject GetNearestEnemy(GameObject bullet, GameObject[] targets){
            BulletState bs = bullet.GetComponent<BulletState>();
            int side = -1;
            if (bs.caster){
                ChaState ccs = bs.caster.GetComponent<ChaState>();
                if (ccs) side = ccs.side;
            }

            GameObject bestTarget = null;
            float bestDis = float.MaxValue;
            for (int i = 0; i < targets.Length; i++){
                ChaState tcs = targets[i].GetComponent<ChaState>();
                if (!tcs || tcs.side == side || tcs.dead == true) continue;
                float dis2 = (
                    Mathf.Pow(bullet.transform.position.x - targets[i].transform.position.x, 2) +
                    Mathf.Pow(bullet.transform.position.y - targets[i].transform.position.y, 2)
                );
                if (bestDis > dis2 || bestTarget == null){
                    bestTarget = targets[i];
                    bestDis = dis2;
                }
            }
            var startPos = new Vector2(bullet.transform.position.x, bullet.transform.position.y) ;
            var lastPos = new Vector2(bestTarget.transform.position.x, bestTarget.transform.position.y);

            BulletBezier bezier = new BulletBezier(startPos, lastPos);
            bezier.GetBezierMidPos(bullet.transform,startPos,lastPos);

            if (!bs.param.ContainsKey("BulletBezier"))
            {
                bs.param.Add("BulletBezier", bezier);
            }
            else {
                bs.param["BulletBezier"] = bs.param;
            }

            return bestTarget;
        }

        ///<summary>
        ///tween
        ///跟踪目标
        ///</summary>
        private static Vector3 FollowingTarget(float t, GameObject bullet, GameObject target){
            BulletState bs = bullet.GetComponent<BulletState>();
            if (bs.param.ContainsKey("BulletBezier") ) {
                BulletBezier bb = (BulletBezier)bs.param["BulletBezier"];
                if (target!=null && target.activeSelf) { 
                    bb.SetEndPos(new Vector2(target.transform.position.x, target.transform.position.y));
                }
                bb.percentFly += bs.speed/30 * t;
                if (bb.percentFly > 1f) { 
                    bb.percentFly = 1f;
                    //该销毁了
                    if (target == null)
                    {
                        bs.DestoryBullet();
                    }
                    else if (target != null && target.GetComponent<ChaState>().dead) {
                        bs.DestoryBullet();
                    }
                }
                var pos = bb.Bezier();

                Vector3 tarDir = new Vector3(pos.x,0,pos.y) - bullet.transform.position;
                float flyingRad = (Mathf.Atan2(tarDir.y,  tarDir.z) * 180 / Mathf.PI - bullet.transform.eulerAngles.z) * Mathf.PI / 180;

                tarDir.x = Mathf.Cos(flyingRad);
                tarDir.y = Mathf.Sin(flyingRad);
                return tarDir;
            }
            return Vector3.zero;
        }

        ///<summary>
        ///tween
        ///获取距离前方位置
        ///</summary>
        private static void SetDistanceForward(GameObject bullet)
        {
            BulletState bs = bullet.GetComponent<BulletState>();

            float distance = 10f;
            Vector2 fwdPos = bullet.transform.right * distance + bullet.transform.position;

            var startPos = new Vector2(bullet.transform.position.x, bullet.transform.position.y);
            var lastPos = new Vector2(fwdPos.x, fwdPos.y);

            BulletBezier bezier = new BulletBezier(startPos, lastPos);
            bezier.GetBezierMidPos(bullet.transform, startPos, lastPos);
            Debug.Log(">>>>>>>>>>>>>>>>bezier  startPos:"+startPos+"  lastPos: "+lastPos);
            if (!bs.param.ContainsKey("BulletBezier"))
            {
                bs.param.Add("BulletBezier", bezier);
            }
            else
            {
                bs.param["BulletBezier"] = bs.param;
            }
        }

        private static Vector3 NewCloakBoomerangTween(float t, GameObject bullet, GameObject target)
        {
            BulletState bs = bullet.GetComponent<BulletState>();
            if (!bs) return Vector3.right;
            float backTime = bs.param.ContainsKey("backTime") ? (float)bs.param["backTime"] : 2.0f; //默认1秒 
            
            if (t < backTime)
            {
                // 抛出运动，做一个半圆形运动
                float percentTime = t / backTime;
                var pos = Vector3.Slerp(Vector2.right , Vector2.up , percentTime);
                Debug.Log(">>>>>>>>>>>>>> pos11111   " + t + "    " + pos);
                return new Vector3(pos.x, pos.z, 0);
            }
            else if (t < backTime * 2)
            {
                // 返回运动，做另一个半圆形运动
                float percentTime = (t - backTime) / backTime;
                var pos = Vector3.Slerp(Vector2.left, Vector2.down, percentTime);
                Debug.Log(">>>>>>>>>>>>>> pos22222   " + pos);
                return new Vector3(pos.x, pos.z, 0);
            }
            // float throwForce = 2f;
            // if (t < backTime)
            // {
            //     // 抛出运动，通过插值逐渐增加速度
            //     float percentTime = t / backTime;
            //     var pos = Vector2.Lerp(Vector2.zero, Vector3.right , percentTime);
            //     // Debug.Log(">>>>>>>>>>>>>> pos11111   " + t + "    " + pos);
            //     return pos;
            // }
            // else if (backTime <= t && t < backTime + backTime)
            // {
            //     // 返回运动，通过插值逐渐减小速度
            //     float percentTime = (t - backTime) / backTime;
            //     var pos = Vector2.Lerp(Vector3.right * -1, Vector2.zero, percentTime);
            //     // Debug.Log(">>>>>>>>>>>>>> pos22222   " + pos);
            //     return pos;
            // }
            else {
                ChaState ccs = bs.caster.GetComponent<ChaState>();
                SceneVariants.RemoveBullet(bullet);
                if (ccs) ccs.PlaySightEffect("Body", "Effect/Heart");
                return Vector3.zero;
            }


            //if (bs.param.ContainsKey("BulletBezier"))
            //{
            //    BulletBezier bb = (BulletBezier)bs.param["BulletBezier"];
            //    float percentTime = t / backTime;
            //    bb.percentFly += bs.speed / 30 * Time.fixedDeltaTime * percentTime;
            //    //if (t < backTime)
            //    //{
            //    //    if (bb.percentFly > 1f)
            //    //    {
            //    //        bb.percentFly = 0f;
            //    //    }
            //    //}
            //    //else
            //    //{
            //    //    if (target != null && target.activeSelf)
            //    //    {
            //    //        bb.SetEndPos(new Vector2(target.transform.position.x, target.transform.position.y));
            //    //    }
                
            //    if (bb.percentFly > 1f)
            //    {
            //        bb.percentFly = 1f;
            //        //该销毁了
            //        if (target == null)
            //        {
            //            bs.DestoryBullet();
            //        }
            //        else if (target != null && target.GetComponent<ChaState>().dead)
            //        {
            //            bs.DestoryBullet();
            //        }
            //    }
            //    //}
            //    var pos = bb.Bezier();
            //    Vector3 tarDir = new Vector3(pos.x, pos.y, 0) - bullet.transform.position;
            //    // Atan2返回的是弧度，所以我们首先将子弹的角度转换为弧度
            //    float bulletRotationRad = bullet.transform.eulerAngles.z * Mathf.Deg2Rad;
            //    // 计算目标方向的弧度
            //    float targetDirRad = Mathf.Atan2(tarDir.y, tarDir.x);

            //    // 计算飞行方向的弧度
            //    float flyingRad = targetDirRad - bulletRotationRad;

            //    //flyingRad += Mathf.PI / 2;

            //    // 确保flyingRad在-π到π之间
            //    //flyingRad = (flyingRad + Mathf.PI) % (2 * Mathf.PI) - Mathf.PI;



            //    Debug.Log(">>>>>>>>>>>>>>>>pos: "+pos+ "  bullet.pos:  "+ bullet.transform.position + "   tarDir: "+tarDir);
            //    // float deg = Mathf.Atan2(tarDir.y, tarDir.x) * 180 / Mathf.PI - bullet.transform.eulerAngles.z;
            //    // float flyingRad = (deg) * Mathf.PI / 180;
            //    Debug.Log(">>>>>>>>>>>>>>>>bb.percentFly：  "+bb.percentFly + "   flyingRad: " + flyingRad);
            //    tarDir.x = Mathf.Cos(flyingRad);
            //    tarDir.y = Mathf.Sin(flyingRad);
            //    tarDir.z = 0;
            //    return tarDir;
            //}
            return Vector3.zero;

        }

        ///<summary>
        ///targetting
        ///选择子弹的施法者作为跟踪的目标
        ///</summary>
        private static GameObject BulletCasterSelf(GameObject bullet, GameObject[] targets){
            BulletState bulletState = bullet.GetComponent<BulletState>();
            if (!bulletState) return null;
            return bulletState.caster;
        }

        ///<summary>
        ///Tween
        ///氪漏氪回力标的轨迹，向前丢出去以后，会开始飞回到丢出去的人手里，bulletObj.param
        ///["backTime"]多少秒以后回头，在这个时间内移动速度呈sin函数
        ///</summary>
        private static Vector3 CloakBoomerangTween(float t, GameObject bullet, GameObject target)
        {
            BulletState bs = bullet.GetComponent<BulletState>();
            if (!bs) return Vector3.right;
            float backTime = bs.param.ContainsKey("backTime") ? (float)bs.param["backTime"] : 2.0f; //默认1秒 
            if (t < backTime)
            {
                //飞出去的过程 获取的是个0到1的半圆弧度
                float rad = t / backTime * Mathf.PI;
                Debug.Log(">>>>>>>>>>>>>>>>Vector3.Sin：  " + Mathf.Sin(rad) + "   Mathf.Cos(rad): " + Mathf.Cos(rad));
                //(Mathf.Sin(rad)的值为0到1再到0，射出去X轴可以变成从慢变快再到慢
                //Mathf.Cos(rad) 的值是1到0再到-1 射出去Y轴变成从快再到慢，再倒退
                //return Vector3.right * (Mathf.Sin(rad) + 0.200f); 
                //（0,1） (1,0) (0，-1)
                return new Vector3(
                   Mathf.Sin(rad) + 0.200f,
                   Mathf.Cos(rad),
                   0
               );
            }
            else
            {
                //  if (target == null) return Vector3.right;
                //飞出去的过程 获取的是个0到1的半圆弧度
                float rad = Mathf.Min((t - backTime) / backTime * Mathf.PI, 0.5f);
                //(Mathf.Sin(rad)的值为0到1再到0，dis变成从慢变快再到慢
                float dis = Mathf.Sin(rad) + 0.100f;
                Vector3 tarDir = bs.caster.transform.position - bullet.transform.position;
                if (tarDir.sqrMagnitude < 0.001f)
                {
                    ChaState ccs = bs.caster.GetComponent<ChaState>();
                    SceneVariants.RemoveBullet(bullet);
                    if (ccs) ccs.PlaySightEffect("Body", "Effect/Heart");
                    return Vector3.zero;
                }

                float toRad = Mathf.Atan2(tarDir.y, tarDir.x) - bs.fireDegree * Mathf.PI / 180;
                Debug.Log("tarDir111==========tarDir  " + tarDir + "  toRad  " + toRad + "  dis  " + dis + "  " + Mathf.Cos(toRad) + "   " + Mathf.Sin(toRad));
                //(-1,-1) (-1,0) (-1,1)
                
                return new Vector3(
                    Mathf.Cos(toRad) * dis,
                    Mathf.Sin(toRad) * dis,
                    0
                );


                //if (target == null) return Vector3.left;
                //Vector3 tarDir = target.transform.position - bullet.transform.position;
                //if (tarDir.sqrMagnitude < 0.001f)
                //{
                //    ChaState ccs = bs.caster.GetComponent<ChaState>();
                //    SceneVariants.RemoveBullet(bullet);
                //    if (ccs) ccs.PlaySightEffect("Body", "Effect/Heart");
                //    return Vector3.zero;
                //}


                //float rad = Mathf.Min((t - backTime) / backTime * Mathf.PI, 0.5f); //获取一个旋转的弧度rad0到90度
                //float dis = Mathf.Sin(rad) + 0.200f;//计算一个位移距离dis
                //float toRad = Mathf.Atan2(tarDir.y, tarDir.x) - bs.fireDegree * Mathf.PI / 180; //计算目标方向的弧度值toRad
                //return new Vector3(
                //    Mathf.Cos(toRad) * dis,
                //    Mathf.Sin(toRad) * dis,
                //    0
                //);
            }
        }
       

        ///<summary>
        ///onHit
        ///氪漏氪回力标命中效果，除了普通效果，就是命中自己的时候移除子弹，参数：
        ///[0]伤害倍数
        ///[1]基础暴击率
        ///[2]命中视觉特效
        ///[3]播放特效位于目标的绑点，默认Body
        ///</summary>
        private static void CloakBoomerangHit(GameObject bullet, GameObject target){
            BulletState bs = bullet.GetComponent<BulletState>();
            if (!bs) return;

            ChaState ccs = bs.caster.GetComponent<ChaState>();
            ChaState tcs = target.GetComponent<ChaState>();
            Debug.Log("tarDir==========  11111" );
            if (ccs != null && tcs != null && ccs.side != tcs.side){
                CommonBulletHit(bullet, target);
            }else{
                float backTime = bs.param.ContainsKey("backTime") ? (float)bs.param["backTime"] : 1.0f; //默认1秒 
                if (bs.timeElapsed > backTime && target.Equals(bs.caster)){
                    SceneVariants.RemoveBullet(bullet);
                    if (ccs) ccs.PlaySightEffect("Body","Effect/Heart");
                }
            }
        }

        ///<summary>
        ///Tween
        ///逐渐加速的子弹，bulletObj参数：
        ///["turningPoint"]float：在第几秒达到预设的速度（100%），并且逐渐减缓增速。
        ///</summary>
        private static Vector3 SlowlyFaster(float t, GameObject bullet, GameObject target){
            BulletState bs = bullet.GetComponent<BulletState>();
            if (!bs) return Vector3.forward;
            float tp = 5.0f; //默认5秒后达到100%速度
            if (bs.param.ContainsKey("turningPoint")) tp = (float)bs.param["turningPoint"];
            if (tp < 1.0f) tp = 1.0f;
            return Vector3.right * (2 * t / (t + tp));
        }

        ///<summary>
        ///onCreate
        ///记录一下这个子弹，作为最后发射的子弹
        ///</summary>
        private static void RecordBullet(GameObject bullet){
            //Debug.Log("记录 RecordBullet  ");
            BulletState bs = bullet.GetComponent<BulletState>();
            if (!bs || bs.caster == null) return;
            ChaState cs = bs.caster.GetComponent<ChaState>();
            if (!cs) return;
            List<BuffObj> bos = cs.GetBuffById("TeleportBulletPassive", new List<GameObject>(){bs.caster});
            //Debug.Log("记录 RecordBullet  " + (bos.Count <= 0));
            if (bos.Count <= 0){
                //玩家身上添加一个buff (传参 fireBullet)
                cs.AddBuff(new AddBuffInfo(
                    DesingerTables.BuffData.data["TeleportBulletPassive"], bs.caster, bs.caster, 1, 10, true, true, new Dictionary<string, object>(){{"firedBullet", bullet}}
                ));
            }else{
                bos[0].buffParam["firedBullet"] = bullet;
            }
        }
        ///<summary>
        ///onCreate
        ///手雷丢出去，要设置一下动画那个
        ///</summary>
        private static void SetBombBouncing(GameObject bullet){
            BulletState bs = bullet.GetComponent<BulletState>();
            BouncingBallY bb = bullet.GetComponentInChildren<BouncingBallY>();
            if (!bs || !bb) return;
            float totalTime = bs.duration;
            if (totalTime <= 0){
                Debug.Log("Boom Explosed immeditly");
                return;
            }
            float[] dTime = new float[]{
                totalTime * 3.000f / 6.000f,
                totalTime * 5.000f / 6.000f,
                totalTime - 0.001f
            };
            float highest = 2.2f;
            bb.ResetTo(highest, dTime);           
        }

        ///<summary>
        ///Tween
        ///手雷的轨迹，在这里要做的是修改bullet的移动模式，一般不推荐这么干
        ///</summary>
        private static Vector3 BoomBallRolling(float t, GameObject bullet, GameObject target){
            BulletState bs = bullet.GetComponent<BulletState>();
            BouncingBallY bb = bullet.GetComponentInChildren<BouncingBallY>();
            if (!bs || !bb) return Vector3.right;
            MoveType toType = MoveType.fly;
            if (bb.hitGroundAt.Length <= 0 || t > bb.hitGroundAt[bb.hitGroundAt.Length - 1]){
                toType = MoveType.ground;
            }else{
                float tt = Time.fixedDeltaTime;
                for (int i = 0; i < bb.hitGroundAt.Length; i++){
                    if (bb.hitGroundAt[i] - tt <= t && t <= bb.hitGroundAt[i] + tt){
                        toType = MoveType.ground;
                        break;
                    }
                }
            }
            
            bs.SetMoveType(toType);
            return Vector3.right;
        }
        /// <summary>
        /// 如果离显示区太远就转向
        /// </summary>
        /// <param name="t"></param>
        /// <param name="bullet"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private static Vector3 StarFishMove(float t, GameObject bullet, GameObject target)
        {
            BulletState bs = bullet.GetComponent<BulletState>();
            if (!bs) return Vector3.right;
            //飞出去的过程
            float rad = t  * Mathf.PI;
            //return Vector3.right * (Mathf.Sin(rad) + 0.200f);
            return Vector3.right;
        }

        ///<summary>
        ///onRemoved
        ///在子弹位置创建一个aoe，所以aoe的始作俑者肯定是caster了，位置也是子弹位置，填写什么都无效，角度也是子弹角度，参数：
        ///[0]AoeLauncher：aoe的发射器，caster在这里被重新赋值，position则作为增量加给现在的角色坐标
        ///[1]AoeLauncher：如果bullet移除时后duration>0或者是obstacled，则会创建这个，如果有这个的话
        ///</summary>
        private static void CreateAoEOnRemoved(GameObject bullet){
            BulletState bulletState = bullet.GetComponent<BulletState>();
            if (!bulletState) return;
            object[] onRemovedParams = bulletState.model.onRemovedParams;
            if (onRemovedParams.Length <= 0)    return;
            AoeLauncher al = (AoeLauncher)onRemovedParams[0];
            if (onRemovedParams.Length > 1 && (bulletState.duration > 0 || bulletState.HitObstacle() == true)) {
                al = (AoeLauncher)onRemovedParams[1];
            }
            al.caster = bulletState.caster;
            al.position = bullet.transform.position;
            al.degree = bullet.transform.eulerAngles.z;
            Debug.Log("to create aoe effect " + al.model.prefab);
            SceneVariants.CreateAoE(al);
        }

        ///<summary>
        ///onHit
        ///在子弹位置创建一个aoe，所以aoe的始作俑者肯定是caster了，位置也是子弹位置，填写什么都无效，角度也是子弹角度，参数：
        ///[0]AoeLauncher：aoe的发射器，caster在这里被重新赋值，position则作为增量加给现在的角色坐标
        ///</summary>
        private static void CreateAoEOnHit(GameObject bullet, GameObject target){
            BulletState bulletState = bullet.GetComponent<BulletState>();
            if (!bulletState) return;
            object[] onHitParams = bulletState.model.onHitParams;
            if (onHitParams.Length <= 0)    return;
            AoeLauncher al = (AoeLauncher)onHitParams[0];
            al.caster = bulletState.caster;
            al.position = bullet.transform.position;
            al.degree = bullet.transform.eulerAngles.z;
            SceneVariants.CreateAoE(al);
        }

    }
}
