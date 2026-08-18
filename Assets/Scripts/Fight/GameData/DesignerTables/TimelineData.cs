using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesingerTables
{
    public class TimelineData{
        public static Dictionary<string, TimelineModel> data = new Dictionary<string, TimelineModel>(){
            //发射普通子弹
            { "skill_fire", new TimelineModel("skill_fire", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle","Effect/MuzzleFlash","",false}),
                new TimelineNode(0.10f, "FireBullet", new object[]{
                   new BulletLauncher(
                        BulletData.data["bullet2d"], null, Vector3.zero, 0, 5.0f, 2.0f, 0f, null, null, true
                    ), "Muzzle"
                }),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},
            { "skill_BeatBackAnimate",new TimelineModel("skill_BeatBackAnimate",new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{false, false, true}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{ "Stand", true}),
                new TimelineNode(0.20f, "CasterForceBeatBack", new object[]{0.8f, 0.2f}),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})    //早0.1秒恢复操作状态手感好点
            },0.60f, TimelineGoTo.Null)},

            { "skill_fireMultiple",new TimelineModel("skill_fireMultiple",new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle","Effect/MuzzleFlash","",false}),
                new TimelineNode(0.10f, "FireMultipleBullet", new object[]{3, "Muzzle"}),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            },0.60f, TimelineGoTo.Null)},
            //填装子弹
            { "skill_shoot1", new TimelineModel("skill_shoot1", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle","Effect/MuzzleFlash","",false}),
                new TimelineNode(0.10f, "FireBullet", new object[]{
                    new BulletLauncher(
                        BulletData.data["bullet2d"], null, Vector3.zero, 0, 5.0f, 2.0f, 0f, null, null, true
                    ), "Muzzle"
                }),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},

            { "skill_shoot2", new TimelineModel("skill_shoot2", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle","Effect/MuzzleFlash","",false}),
                new TimelineNode(0.10f, "FireMultipleBullet", new object[]{2, "Muzzle"}),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},

            { "skill_shoot3", new TimelineModel("skill_shoot3", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle","Effect/MuzzleFlash","",false}),
                new TimelineNode(0.10f, "FireMultipleBullet", new object[]{3, "Muzzle"}),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},

            { "skill_poisonShot", new TimelineModel("skill_poisonShot", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle","Effect/MuzzleFlash","",false}),
                new TimelineNode(0.10f, "FireBullet", new object[]{
                    new BulletLauncher(
                        BulletData.data["poisonSlowBullet"], null, Vector3.zero, 0, 5.0f, 3.0f
                    ), "Muzzle"
                }),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},

            { "skill_stunShot", new TimelineModel("skill_stunShot", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle","Effect/MuzzleFlash","",false}),
                new TimelineNode(0.10f, "FireBullet", new object[]{
                    new BulletLauncher(
                        BulletData.data["stunBullet"], null, Vector3.zero, 0, 6.0f, 3.0f
                    ), "Muzzle"
                }),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},

            { "skill_laser", new TimelineModel("skill_laser", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.08f, "PlaySightEffectOnCaster", new object[]{"Muzzle","Effect/MuzzleFlash","",false}),
                new TimelineNode(0.08f, "FireBullet", new object[]{
                    new BulletLauncher(
                        BulletData.data["laserBullet"], null, Vector3.zero, 0, 18.0f, 1.2f
                    ), "Muzzle"
                }),
                new TimelineNode(0.35f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.35f, TimelineGoTo.Null)},

            { "skill_lightning", new TimelineModel("skill_lightning", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.08f, "PlaySightEffectOnCaster", new object[]{"Muzzle","Effect/MuzzleFlash","",false}),
                new TimelineNode(0.08f, "FireBullet", new object[]{
                    new BulletLauncher(
                        BulletData.data["lightningBullet"], null, Vector3.zero, 0, 10.0f, 30.0f, 0,
                        DesignerScripts.BulletScript.bulletTween["FollowingTarget"],
                        DesignerScripts.BulletScript.targettingFunc["GetNearestEnemy"], false
                    ), "Muzzle"
                }),
                new TimelineNode(0.45f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.45f, TimelineGoTo.Null)},

            { "skill_haste", new TimelineModel("skill_haste", new TimelineNode[]{
                new TimelineNode(0.00f, "AddBuffToCaster", new object[]{
                    new AddBuffInfo(DesingerTables.BuffData.data["SkillHaste"], null, null, 1, 5.0f, true, false)
                })
            }, 0.10f, TimelineGoTo.Null)},

            { "skill_regen", new TimelineModel("skill_regen", new TimelineNode[]{
                new TimelineNode(0.00f, "AddBuffToCaster", new object[]{
                    new AddBuffInfo(DesingerTables.BuffData.data["SkillRegen"], null, null, 1, 5.0f, true, false)
                })
            }, 0.10f, TimelineGoTo.Null)},

             { "skill_debugInvincible", new TimelineModel("skill_debugInvincible", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Stand", true}),
                new TimelineNode(0.05f, "PlaySightEffectOnCaster", new object[]{"Body","Effect/Heart","debug_invincible",false}),
                new TimelineNode(0.05f, "CasterImmune", new object[]{5.0f}),
                new TimelineNode(0.10f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.10f, TimelineGoTo.Null)},

             { "skill_debugStopMove", new TimelineModel("skill_debugStopMove", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{false, false, true}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Stand", true}),
                new TimelineNode(0.05f, "PlaySightEffectOnCaster", new object[]{"Body","Effect/Heart","debug_stopmove",false}),
                new TimelineNode(0.10f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.10f, TimelineGoTo.Null)},

             { "skill_debugFlow", new TimelineModel("skill_debugFlow", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.05f, "AddBuffToCaster", new object[]{
                    new AddBuffInfo(DesingerTables.BuffData.data["SkillHaste"], null, null, 1, 4.0f, true, false)
                }),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle","Effect/MuzzleFlash","debug_flow",false}),
                new TimelineNode(0.10f, "FireBullet", new object[]{
                    new BulletLauncher(
                        BulletData.data["laserBullet"], null, Vector3.zero, 0, 18.0f, 1.2f
                    ), "Muzzle"
                }),
                new TimelineNode(0.20f, "FireBullet", new object[]{
                    new BulletLauncher(
                        BulletData.data["lightningBullet"], null, Vector3.zero, 0, 10.0f, 30.0f, 0,
                        DesignerScripts.BulletScript.bulletTween["FollowingTarget"],
                        DesignerScripts.BulletScript.targettingFunc["GetNearestEnemy"], false
                    ), "Muzzle"
                }),
                new TimelineNode(0.30f, "AddBuffToCaster", new object[]{
                    new AddBuffInfo(DesingerTables.BuffData.data["SkillRegen"], null, null, 1, 4.0f, true, false)
                }),
                new TimelineNode(0.45f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.45f, TimelineGoTo.Null)},

             { "skill_reload", new TimelineModel("skill_reload", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Reload", false}),
                new TimelineNode(1.10f, "CasterAddAmmo", new object[]{99999}),
                new TimelineNode(1.12f, "SetCasterControlState", new object[]{true, true, true})
            }, 1.15f, TimelineGoTo.Null)},

            //发射氪漏氪回力标
            { "skill_cloakBoomerang", new TimelineModel("skill_cloakBoomerang", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Head","Effect/Heart","",false}),
                new TimelineNode(0.10f, "FireBullet", new object[]{
                    new BulletLauncher(
                        BulletData.data["cloakBoomerang"], null, Vector3.zero, 0, 10.0f, 10.0f, 0,
                        DesignerScripts.BulletScript.bulletTween["CloakBoomerangTween"],
                        //DesignerScripts.BulletScript.targettingFunc["BulletCasterSelf"],
                        //DesignerScripts.BulletScript.bulletTween["NewCloakBoomerangTween"],
                        null,
                        true, new Dictionary<string, object>(){{"backTime", 2.0f}}
                    ), "Muzzle"
                }),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},

            //发射传送子弹
            { "skill_teleportBullet_fire", new TimelineModel("skill_teleportBullet_fire", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle","Effect/MuzzleFlash","",false}),
                new TimelineNode(0.10f, "FireBullet", new object[]{
                    new BulletLauncher(
                        BulletData.data["teleportBullet"], null, Vector3.zero, 0, 6.0f, 3.0f, 0,
                        DesignerScripts.BulletScript.bulletTween["SlowlyFaster"]
                    ), "Muzzle"
                }),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},
            //闪现过去吃掉传送子弹(直接交给buff去办)
            { "skill_teleportBullet_tele", new TimelineModel("skill_teleportBullet_tele", new TimelineNode[]{
                new TimelineNode(0.00f, "AddBuffToCaster", new object[]{
                    new AddBuffInfo(DesingerTables.BuffData.data["TeleportTo"], null, null, 1, 0.0f, true, false)
                })
            }, 0.10f, TimelineGoTo.Null)},

            //发射跟踪子弹
            { "skill_followfire", new TimelineModel("skill_followfire", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle","Effect/MuzzleFlash","",false}),
                new TimelineNode(0.10f, "FireBullet", new object[]{
                    new BulletLauncher(
                        BulletData.data["normal0"], null, Vector3.zero, 0, 3.0f, 100.0f, 0,  
                            DesignerScripts.BulletScript.bulletTween["FollowingTarget"], 
                            DesignerScripts.BulletScript.targettingFunc["GetNearestEnemy"], false
                        ), "Muzzle"
                    }
                ),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},

            //丢手雷
            { "skill_grenade", new TimelineModel("skill_grenade", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.10f, "FireBullet", new object[]{
                    new BulletLauncher(
                        BulletData.data["boomball"], null, Vector3.zero, 0, 3.0f, 2.0f, 0,  
                            DesignerScripts.BulletScript.bulletTween["BoomBallRolling"]
                        ), "Muzzle"
                    }
                ),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},

            //召唤炸药桶
            { "skill_exploseBarrel", new TimelineModel("skill_exploseBarrel", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.10f, "SummonCharacter", new object[]{
                    "Barrel", new ChaProperty(0, 5), 0f, "",
                    new string[]{"Barrel"},
                    new AddBuffInfo[]{
                        new AddBuffInfo(DesingerTables.BuffData.data["ExplosionBarrel"], null, null, 1, 10, true, true)
                    }
                }),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},

            //发射小猴球
            { "skill_spaceMonkeyBall", new TimelineModel("skill_spaceMonkeyBall", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Fire", false}),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle","Effect/MuzzleFlash","",false}),
                new TimelineNode(0.10f, "CreateAoE", new object[]{
                    new AoeLauncher(
                        AoEData.data["SpaceMonkeyBall"], null, Vector3.zero, 0.25f, 100, 0,
                        DesignerScripts.AoEScript.aoeTweenFunc["SpaceMonkeyBallRolling"], new object[]{Vector3.forward * 0.1f} //小猴球原始滚动速度0.1米/秒
                    ), true
                }),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},

            //角色向移动方向打滚的技能效果
            { "skill_roll", new TimelineModel("skill_roll", new TimelineNode[]{
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{"Roll", true}),
                new TimelineNode(0.00f, "PlaySightEffectOnCaster", new object[]{"Body","Effect/Fire_B","fire_following",true}),
                new TimelineNode(0.01f, "SetCasterControlState", new object[]{false, false, false}),
                new TimelineNode(0.10f, "CasterImmune", new object[]{0.70f}),
                new TimelineNode(0.20f, "CasterForceMove", new object[]{2.0f, 0.5f, 0.00f, true, false}),
                new TimelineNode(0.80f, "StopSightEffectOnCaster", new object[]{"Body", "fire_following"}),
                new TimelineNode(0.80f, "PlaySightEffectOnCaster", new object[]{"Body","Effect/ShockWave","shockWave",false}),
                new TimelineNode(0.80f, "SetCasterControlState", new object[]{true, true, true})    //早0.1秒恢复操作状态手感好点
            }, 0.90f, TimelineGoTo.Null) }

        };

        
        //private static TimelineModel skill_roll = ;
    }    
}
