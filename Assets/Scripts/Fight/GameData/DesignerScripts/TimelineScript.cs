using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignerScripts
{
    /// <summary>
    /// Timeline是一个时间轴，在这个时间轴上由若干个节点，每个节点都有一个时间点，即从timeline开始运作后多久；
    /// 然后是其执行的事件——时间点和事件形成的数据组成了数组就是Timeline的基本逻辑。通过Timeline，
    /// 我们可以“预约”控制一些事件的发生，这和Unity的Timeline控制动画的概念是一样的，只是我们控制的不是动画，
    /// 而是“事件”——即类似“让角色做个动作”、“产生一个AoE”等等等等事件
    /// </summary>
    public class TimelineScript{
        public static Dictionary<string, TimelineEvent> functions = new Dictionary<string, TimelineEvent>(){
            {TimelineEventIds.CasterPlayAnim, CasterPlayAnim},
            {TimelineEventIds.CasterForceMove, CasterForceMove},
            {TimelineEventIds.CasterForceBeatBack, CasterForceBeatBack},
            {TimelineEventIds.SetCasterControlState, SetCasterControlState},
            {TimelineEventIds.PlaySightEffectOnCaster, PlaySightEffectOnCaster},
            {TimelineEventIds.StopSightEffectOnCaster, StopSightEffectOnCaster},
            {TimelineEventIds.FireBullet, FireBullet},
            {TimelineEventIds.FireMultipleBullet, FireMultipleBullet },
            {TimelineEventIds.CasterImmune, CasterImmune},
            {TimelineEventIds.CreateAoE, CreateAoE},
            {TimelineEventIds.AddBuffToCaster, AddBuffToCaster},
            {TimelineEventIds.CasterAddAmmo, CasterAddAmmo},
            {TimelineEventIds.SummonCharacter, SummonCharacter},
            {TimelineEventIds.ApplyAbilityState, ApplyAbilityState},
            {TimelineEventIds.ApplyAbilityStateToNearestEnemy, ApplyAbilityStateToNearestEnemy},
            {TimelineEventIds.RemoveAbilityState, RemoveAbilityState},
            {TimelineEventIds.ControlNearestMonster, ControlNearestMonster},
            {TimelineEventIds.SpawnAbilityClone, SpawnAbilityClone},
            {TimelineEventIds.SummonAbilityLightning, SummonAbilityLightning},
            {TimelineEventIds.SetWeather, SetWeather}
        };

        public static void EnsureInitialized()
        {
        }

        ///<summary>
        ///在Caster的某个绑点(Muzzle/Head/Body)上发射一个子弹出来
        ///<param name="args">总共3个参数：
        ///[0]BulletLauncher：子弹发射信息，其中caster和position是需要获得后该写的，degree则需要加上角色的转向
        ///[1]string：角色身上绑点位置，默认Muzzle
        ///</param>
        ///</summary>
        private static void FireBullet(TimelineObj tlo, params object[] args){
            if (args.Length <= 0) return;
            Debug.Log(">>>>>>>>>>>>>>>>>>>>  FireBullet");
            if (tlo.caster){
                UnitBindManager ubm = tlo.caster.GetComponent<UnitBindManager>();
                if (!ubm) return;

                BulletLauncher bLauncher = (BulletLauncher)args[0];
                UnitBindPoint ubp = ubm.GetBindPointByKey(args.Length > 1 ? (string)args[1] : "Muzzle");
                if (!ubp) return;

                bLauncher.caster = tlo.caster;
                bLauncher.fireDegree = tlo.caster.transform.rotation.eulerAngles.z;
                bLauncher.firePosition = ubp.gameObject.transform.position;
                Debug.Log("[TimelineScript] FireBullet caster=" + tlo.caster.name + " bindPoint=" + ubp.key);

                SceneVariants.CreateBullet(bLauncher);
            }
        }
        
        ///<summary>
        ///在Caster的某个绑点(Muzzle/Head/Body)上发射多发子弹出来
        ///<param name="args">总共3个参数：
        ///[0]BulletLauncher：子弹发射信息，其中caster和position是需要获得后该写的，degree则需要加上角色的转向
        ///[1]string：角色身上绑点位置，默认Muzzle
        ///</param>
        ///</summary>
        private static void FireMultipleBullet(TimelineObj tlo, params object[] args)
        {
            if (args.Length <= 0) return;
            if (tlo.caster)
            {
                int count = args.Length >= 1 ? (int)(args[0]) : 2;
                UnitBindManager ubm = tlo.caster.GetComponent<UnitBindManager>();
                if (!ubm) return;
                UnitBindPoint ubp = ubm.GetBindPointByKey(args.Length > 1 ? (string)args[1] : "Muzzle");
                if (!ubp) return;

                List<int> angles = new List<int>() {-10,0,10 };
                for (int i = 0; i < count; i++)
                {
                    BulletLauncher bLauncher = new BulletLauncher(
                       // Keep each spread ray on its own launch angle after firing.
                       DesingerTables.BulletData.data["bullet2d"], null, Vector3.zero, 0, 6.0f, 10.0f, 0f, null, null, true
                    );
                    bLauncher.caster = tlo.caster;
                    bLauncher.fireDegree = tlo.caster.transform.rotation.eulerAngles.z+ angles[i];
                    bLauncher.firePosition = ubp.gameObject.transform.position;
                    Debug.Log("[TimelineScript] FireMultipleBullet caster=" + tlo.caster.name + " index=" + i + " degree=" + bLauncher.fireDegree);

                    SceneVariants.CreateBullet(bLauncher);
                }
                
            }
        }
        ///<summary>
        ///在caster=timeline.caster的面前位置aoe
        ///<param name="args">总共3个参数：
        ///[0]AoeLauncher：aoe的发射器，caster在这里被重新赋值，position则作为增量加给现在的角色坐标
        ///[1]bool：true=面前，false=角色坐标
        ///</param>
        ///</summary>
        private static void CreateAoE(TimelineObj tlo, params object[] args){
            if (args.Length <= 0) return;
            
            if (tlo.caster){
                UnitBindManager ubm = tlo.caster.GetComponent<UnitBindManager>();
                if (!ubm) return;

                AoeLauncher aLauncher = ((AoeLauncher)args[0]).Clone(); //必须克隆出来，去掉ref属性，使之变成临时的属性
                bool inFront = args.Length > 1 ? (bool)args[1] : true;
                
                aLauncher.caster = tlo.caster;
                aLauncher.degree += tlo.caster.transform.rotation.eulerAngles.z;

                float rr = aLauncher.degree * Mathf.PI / 180;
                Vector3 pos = aLauncher.position;
                
                float dis = Mathf.Sqrt(Mathf.Pow(pos.x, 2) + Mathf.Pow(pos.y, 2));
                if (inFront == true){
                    dis += tlo.caster.GetComponent<ChaState>().property.bodyRadius + aLauncher.radius;
                } 

                aLauncher.position.x = dis * Mathf.Sin(rr) + tlo.caster.transform.position.x;
                aLauncher.position.y = dis * Mathf.Cos(rr) + tlo.caster.transform.position.y;

                aLauncher.tweenParam = new object[]{
                    new Vector3(
                        dis * Mathf.Sin(rr),
                        dis * Mathf.Cos(rr),
                        0
                    )
                };

                SceneVariants.CreateAoE(aLauncher);
            }
        }

        ///<summary>
        ///timelien的焦点角色播放某个动作，是否是跳转到那个动作一直播放还是会回到站立，这取决于animator里面做的，我也无能为力
        ///<param name="args">总共3个参数：
        ///[0]string：是要播放的动画
        ///[1]bool：是否要取得动画的方向，如果不要就直接用预设的值了
        ///[2]bool：是否启用当前正在进行的面向和移动角度，如果false或者缺省了，就代表启用timelineObj中储存的（开始时的）
        ///</param>
        ///</summary>
        private static void CasterPlayAnim(TimelineObj tlo, params object[] args){
            if (tlo.caster){
                string animName = args.Length >= 1 ? (string)(args[0]) : "";

                if (animName == "") return;

                bool getTail = args.Length >= 2 ? (bool)(args[1]) : false;
                bool useCurrentDeg = args.Length >= 3 ? (bool)(args[2]) : false;
                
                ChaState cs = tlo.caster.GetComponent<ChaState>();
                if (cs){
                    float faceDeg = useCurrentDeg == true ? cs.faceDegree : (float)tlo.GetValue("faceDegree");
                    float moveDeg = useCurrentDeg == true ? cs.moveDegree : (float)tlo.GetValue("moveDegree");
                    if (getTail == true) animName += Utils.GetTailStringByDegree(faceDeg, moveDeg);
                    Debug.Log("[TimelineScript] CasterPlayAnim caster=" + tlo.caster.name + " anim=" + animName);
                    cs.Play(animName); 
                }
            }
        }

        ///<summary>
        ///timeline的焦点角色强制进行移动
        ///<param name="args">总共4个参数：
        ///[0]float：想要强行移动的距离，单位：米。
        ///[1]float：在多久内完成这个移动，单位：秒。这是匀速直线移动的。
        ///[2]float：基于角色移动方向或者面向（取决于[2]），获得一个基础的移动角度偏移量。
        ///[3]bool：是否要基于角色移动方向，如果不是，就是基于角色的面朝方向。
        ///[4]bool：如果启用面向，是否启用正在进行的，而非timeline创建时的，缺省或者false代表启用timeline创建时产生的
        ///</param>
        ///</summary>
        private static void CasterForceMove(TimelineObj tlo, params object[] args){
            if (tlo.caster){
                ChaState cs = tlo.caster.GetComponent<ChaState>();
                float dis = args.Length >= 1 ? (float)args[0] : 0.00f;
                float inSec = (args.Length >= 2 ? (float)args[1] : 0.00f) / tlo.timeScale;  //移动速度可得手动设置倍速
                float degOffset = args.Length >= 3 ? (float)args[2] : 0.00f;
                bool basedOnMoveDir = args.Length >= 4 ? (bool)args[3] : true;
                bool useCurrentDeg = args.Length >= 5 ? (bool)args[4] : false;
                
                if (cs){
                    float mr = (
                        (
                            basedOnMoveDir == true ? 
                                (useCurrentDeg == true ? cs.moveDegree : (float)tlo.GetValue("moveDegree")) : 
                                (useCurrentDeg == true ? cs.faceDegree : (float)tlo.GetValue("faceDegree"))
                        ) + degOffset
                    ) * Mathf.PI / 180.00f;

                    Vector3 mdir = new Vector3(
                        Mathf.Sin(mr) * dis,
                        Mathf.Cos(mr) * dis,
                        0
                    );
                    cs.AddForceMove(new MovePreorder(mdir, inSec));
                }
            }
        }
        /// <summary>
        /// 角色被强制击退 
        /// 根据子弹的方向击退角色
        /// </summary>
        ///<param name="args">总共4个参数：
        ///[0]float：想要强行移动的距离，单位：米。
        ///[1]float：在多久内完成这个移动，单位：秒。这是匀速直线移动的。
        ///</param>{-0.6f, 0.3f, 0.00f, true, false}
        private static void CasterForceBeatBack(TimelineObj tlo, params object[] args)
        {
            if (tlo.caster)
            {
                GameObject source = null;
                if (tlo.param is SkillCastContext castContext && castContext.payload is GameObject contextSource)
                {
                    source = contextSource;
                }
                else if (tlo.param != null && tlo.param is object[] sourceArgs && sourceArgs.Length > 0)
                {
                    source = sourceArgs[0] as GameObject;
                }
                if (source == null) return;
                float sourceAngle = source.transform.rotation.eulerAngles.z;

                ChaState cs = tlo.caster.GetComponent<ChaState>();
                float dis = args.Length >= 1 ? (float)args[0] : 0.00f;
                float inSec = (args.Length >= 2 ? (float)args[1] : 0.00f) / tlo.timeScale;  //移动速度可得手动设置倍速

                if (cs)
                {
                    float mr = sourceAngle * Mathf.PI / 180.00f;

                    Vector3 mdir = new Vector3(
                        Mathf.Sin(mr) * dis,
                        Mathf.Cos(mr) * dis,
                        0
                    );
                    cs.AddForceMove(new MovePreorder(mdir, inSec));
                }
            }
        }

        ///<summary>
        ///设置timeline的焦点角色的ChaControlState
        ///<param name="args">总共3个参数：
        ///[0]bool：可否移动，如果得不到参数，就保持原值。
        ///[1]bool：可否转身，如果得不到参数，就保持原值。
        ///[2]bool：可否释放技能，如果得不到参数，就保持原值。
        ///</param>
        ///</summary>
        private static void SetCasterControlState(TimelineObj tlo, params object[] args){
            if (tlo.caster){
                ChaState cs = tlo.caster.GetComponent<ChaState>();
                if (cs){
                    if (args.Length >= 1) cs.timelineControlState.canMove = (bool)args[0];
                    if (args.Length >= 2) cs.timelineControlState.canRotate = (bool)args[1];
                    if (args.Length >= 3) cs.timelineControlState.canUseSkill = (bool)args[2];
                    Debug.Log("[TimelineScript] SetCasterControlState caster=" + tlo.caster.name + " move=" + cs.timelineControlState.canMove + " rotate=" + cs.timelineControlState.canRotate + " skill=" + cs.timelineControlState.canUseSkill);
                }
            }
        }

        ///<summary>
        ///在timeline焦点角色身上播放一个视觉特效
        ///<param name="args">总共4个参数：
        ///[0]string：要播放特效的绑点
        ///[1]string：特效的文件名，位于Prafabs/下
        ///[2]string：特效的key，用于删除的
        ///[3]bool：是否循环播放特效（循环就要手动删除）
        ///</param>
        ///</summary>
        private static void PlaySightEffectOnCaster(TimelineObj tlo, params object[] args){
            if (tlo.caster){
                ChaState cs = tlo.caster.GetComponent<ChaState>();
                if (cs){
                    string bindPointKey = args.Length >= 1 ? (string)args[0] : "Body";
                    string effectName = args.Length >= 2 ? (string)args[1] : "";
                    string effectKey = args.Length >= 3 ? (string)args[2] : Random.value.ToString();
                    bool loop = args.Length >= 4 ? (bool)args[3] : false;
                    cs.PlaySightEffect(bindPointKey, effectName, effectKey, loop);
                }
            }
        }

        ///<summary>
        ///在timeline焦点角色身上关闭一个视觉特效
        ///<param name="args">总共2个参数：
        ///[0]string：要关闭的特效所处绑点
        ///[1]string：特效的key，创建时产生的
        ///</param>
        ///</summary>
        private static void StopSightEffectOnCaster(TimelineObj tlo, params object[] args){
            if (tlo.caster){
                ChaState cs = tlo.caster.GetComponent<ChaState>();
                if (cs){
                    string bindPointKey = args.Length >= 1 ? (string)args[0] : "Body";
                    string effectKey = args.Length >= 2 ? (string)args[1] : "";
                    if (effectKey == "") return;
                    cs.StopSightEffect(bindPointKey, effectKey);
                }
            }
        }

        ///<summary>
        ///设置timeline的caster身上的无敌时间
        ///<param name="args">总共1个参数：
        ///[0]float：无敌的时间，单位：秒
        ///</param>
        ///</summary>
        private static void CasterImmune(TimelineObj timelineObj, params object[] args){
            if (timelineObj.caster){
                ChaState cs = timelineObj.caster.GetComponent<ChaState>();
                if (cs && args.Length > 0){
                    float immT = (float)args[0];
                    Debug.Log("[TimelineScript] CasterImmune caster=" + timelineObj.caster.name + " time=" + immT);
                    cs.SetImmuneTime(immT);
                }
            }
        }

        ///<summary>
        ///修改timeline的caster身上的子弹数量
        ///[0]int：需要添加的数量，负数就是减少了
        ///</summary>
        private static void CasterAddAmmo(TimelineObj timelineObj, params object[] args){
            if (timelineObj.caster){
                ChaState cs = timelineObj.caster.GetComponent<ChaState>();
                if (cs && args.Length > 0){
                    int modCount = (int)args[0];
                    cs.ModResource(new ChaResource(cs.resource.hp, modCount + cs.resource.ammo, cs.resource.stamina));
                }
            }
        }

        ///<summary>
        ///给timeline的caster添加一个buff
        ///[0]AddBuffInfo：如何添加一个buff，其中caster和carrier都会是timeline.caster本身
        ///</summary>
        private static void AddBuffToCaster(TimelineObj timelineObj, params object[] args){
            if (timelineObj.caster && args.Length > 0){
                AddBuffInfo abi = (AddBuffInfo)args[0];
                abi.caster = timelineObj.caster;
                abi.target = timelineObj.caster;
                ChaState cs = timelineObj.caster.GetComponent<ChaState>();
                if (cs){
                    Debug.Log("[TimelineScript] AddBuffToCaster caster=" + timelineObj.caster.name);
                    cs.AddBuff(abi);
                }
            }
        }

        ///<summary>
        ///创建一个buff给角色，并且给他添加一系列buff
        ///[0]string： prefab, 
        ///[1]ChaProperty: baseProp, 
        ///[2]float: degree, 
        ///[3]string: unitAnimInfo = "Default_Gunner", 
        ///[4]string[]: tags = null
        ///[5]AddBuffInfo[]: 开始时候要添加的buff
        ///</summary>
        private static void SummonCharacter(TimelineObj timelineObj, params object[] args){
            if (!timelineObj.caster) return;

            string prefab = args.Length > 0 ? (string)args[0] : "";
            int side = -1;
            Vector3 pos = timelineObj.caster.transform.position;
            ChaProperty cp = args.Length >  1? (ChaProperty)args[1] : new ChaProperty(100, 1); 
            float degree = args.Length > 2 ? (float)args[2] : 0;
            string uaInfo = args.Length > 3 ? (string)args[3] : "";
            string[] tags = args.Length > 4 ? (string[])args[4] : null;
            AddBuffInfo[] addBuffs = args.Length > 5 ? (AddBuffInfo[])args[5] : new AddBuffInfo[0];

            Debug.Log("[TimelineScript] SummonCharacter prefab=" + prefab + " side=" + side + " degree=" + degree);
            GameObject sumGuy = SceneVariants.CreateCharacter(prefab, side, pos, cp, degree, uaInfo, tags);
            ChaState sgs = sumGuy.GetComponent<ChaState>();
            for (int i = 0; i < addBuffs.Length; i++){
                addBuffs[i].caster = timelineObj.caster;
                addBuffs[i].target = sumGuy;
                sgs.AddBuff(addBuffs[i]);
            }
        }

        /// <summary>时间轴统一申请能力状态，参数依次为状态、来源、持续时间、优先级、伪装阵营。</summary>
        private static void ApplyAbilityState(TimelineObj timelineObj, params object[] args){
            if (!timelineObj.caster || args.Length <= 0) return;
            ChaState state = timelineObj.caster.GetComponent<ChaState>();
            if (state == null) return;
            ChaAbilityStateType stateType = args[0] is ChaAbilityStateType ? (ChaAbilityStateType)args[0] : (ChaAbilityStateType)(int)args[0];
            string sourceId = args.Length > 1 ? (string)args[1] : "timeline-ability";
            float duration = args.Length > 2 ? (float)args[2] : -1f;
            int priority = args.Length > 3 ? (int)args[3] : 20;
            int sideOverride = args.Length > 4 ? (int)args[4] : int.MinValue;
            state.AddState(stateType, sourceId, duration, priority, sideOverride);
        }

        /// <summary>向最近敌人施加状态；参数依次为半径、状态、来源、时长、优先级。</summary>
        private static void ApplyAbilityStateToNearestEnemy(TimelineObj timelineObj, params object[] args){
            if (!timelineObj.caster || args.Length < 2) return;
            ChaState caster = timelineObj.caster.GetComponent<ChaState>();
            if (caster == null) return;
            float radius = args[0] is float ? (float)args[0] : float.PositiveInfinity;
            ChaAbilityStateType stateType = args[1] is ChaAbilityStateType ? (ChaAbilityStateType)args[1] : (ChaAbilityStateType)(int)args[1];
            string sourceId = args.Length > 2 ? (string)args[2] : "timeline-enemy-state";
            float duration = args.Length > 3 ? (float)args[3] : -1f;
            int priority = args.Length > 4 ? (int)args[4] : 20;
            ChaState target = TargetService.FindNearestEnemy(caster, radius);
            if (target != null) target.AddState(stateType, sourceId, duration, priority);
        }

        /// <summary>时间轴按来源清理能力状态，避免技能结束后残留隐身或伪装。</summary>
        private static void RemoveAbilityState(TimelineObj timelineObj, params object[] args){
            if (!timelineObj.caster || args.Length <= 0) return;
            ChaState state = timelineObj.caster.GetComponent<ChaState>();
            if (state == null) return;
            ChaAbilityStateType stateType = args[0] is ChaAbilityStateType ? (ChaAbilityStateType)args[0] : (ChaAbilityStateType)(int)args[0];
            string sourceId = args.Length > 1 ? (string)args[1] : null;
            state.RemoveState(stateType, sourceId);
        }

        /// <summary>时间轴控制最近敌方怪物，结束后由控制权自动释放并恢复 AI。</summary>
        private static void ControlNearestMonster(TimelineObj timelineObj, params object[] args){
            if (!timelineObj.caster) return;
            ChaState controller = timelineObj.caster.GetComponent<ChaState>();
            if (controller == null) return;
            float radius = args.Length > 0 ? (float)args[0] : float.PositiveInfinity;
            float duration = args.Length > 1 ? (float)args[1] : 3f;
            string sourceId = args.Length > 2 ? (string)args[2] : "timeline-control";
            int priority = args.Length > 3 ? (int)args[3] : 100;
            ChaState target = TargetService.FindNearestEnemy(controller, radius);
            AbilityEffectService.ControlMonster(controller, target, sourceId, duration, priority);
        }

        /// <summary>时间轴创建限时分身或诱饵，参数为预制体、偏移、时长、是否诱饵、来源。</summary>
        private static void SpawnAbilityClone(TimelineObj timelineObj, params object[] args){
            if (!timelineObj.caster || args.Length <= 0) return;
            ChaState source = timelineObj.caster.GetComponent<ChaState>();
            GameObject prefab = args[0] as GameObject;
            Vector3 offset = args.Length > 1 ? (Vector3)args[1] : Vector3.right;
            float duration = args.Length > 2 ? (float)args[2] : 5f;
            bool decoy = args.Length > 3 && (bool)args[3];
            string sourceId = args.Length > 4 ? (string)args[4] : "timeline-clone";
            AbilityEffectService.SpawnClone(source, prefab, offset, duration, decoy, sourceId);
        }

        /// <summary>时间轴调用统一闪电能力，伤害仍排入 DamageManager。</summary>
        private static void SummonAbilityLightning(TimelineObj timelineObj, params object[] args){
            if (!timelineObj.caster) return;
            ChaState caster = timelineObj.caster.GetComponent<ChaState>();
            float radius = args.Length > 0 ? (float)args[0] : 5f;
            int damage = args.Length > 1 ? (int)args[1] : 10;
            float stunDuration = args.Length > 2 ? (float)args[2] : 0.5f;
            string sourceId = args.Length > 3 ? (string)args[3] : "timeline-lightning";
            AbilityEffectService.SummonLightning(caster, radius, damage, stunDuration, sourceId);
        }
        /// <summary>时间轴切换天气，参数依次为天气类型、持续时间、全局风、风力和是否降低可见度。</summary>
        private static void SetWeather(TimelineObj timelineObj, params object[] args){
            WeatherController controller = timelineObj != null && timelineObj.caster != null ? timelineObj.caster.GetComponent<WeatherController>() : null;
            if (controller == null) controller = Object.FindObjectOfType<WeatherController>();
            if (controller == null || args.Length <= 0) return;
            WeatherType weatherType = args[0] is WeatherType ? (WeatherType)args[0] : (WeatherType)(int)args[0];
            float duration = args.Length > 1 ? (float)args[1] : -1f;
            Vector2 globalWind = args.Length > 2 ? (Vector2)args[2] : Vector2.zero;
            float movementStrength = args.Length > 3 ? (float)args[3] : 1f;
            bool lowerVisibility = args.Length > 4 && (bool)args[4];
            controller.SetWeather(weatherType, duration, globalWind, movementStrength, lowerVisibility);
        }
    }
}
