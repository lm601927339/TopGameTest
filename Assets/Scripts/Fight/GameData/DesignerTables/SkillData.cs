using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesingerTables
{
    
    ///<summary>
    ///BulletModel
    ///</summary>
    public class SkillData{
        public static Dictionary<string, SkillModel> data = new Dictionary<string, SkillModel>(){
            //发射子弹
            {"fire", new SkillModel("fire", new ChaResource(0, 1), ChaResource.Null, "skill_fire", new AddBuffInfo[]{
                new AddBuffInfo(DesingerTables.BuffData.data["AutoCheckReload"], null, null, 1, 1, true, true),
                //new AddBuffInfo(DesingerTables.Buff.data["BulletBeatBackOnHit"],null, null, 1, 1, true, true),
            })}, //即使没有子弹也可以用这个技能，但是因为有buff会让他自动转向另一个reload的timeline
            {"fireMultiple", new SkillModel("fireMultiple", new ChaResource(0, 1), ChaResource.Null, "skill_fireMultiple", new AddBuffInfo[]{
                new AddBuffInfo(DesingerTables.BuffData.data["AutoCheckReload"], null, null, 1, 1, true, true),
                //new AddBuffInfo(DesingerTables.Buff.data["BulletBeatBackOnHit"],null, null, 1, 1, true, true),
            })}, //发射多发子弹

            {"spaceMonkeyBall", new SkillModel("spaceMonkeyBall", new ChaResource(0, 3), ChaResource.Null, "skill_spaceMonkeyBall", null)},
            {"grenade", new SkillModel("grenade", ChaResource.Null, ChaResource.Null, "skill_grenade", null)},
            {"explosiveBarrel", new SkillModel("explosiveBarrel", ChaResource.Null, ChaResource.Null, "skill_exploseBarrel", null)},
            {"homingMissle", new SkillModel("homingMissle", new ChaResource(0, 2), ChaResource.Null, "skill_followfire", null)},
            {"cloakBoomerang", new SkillModel("cloakBoomerang", ChaResource.Null, ChaResource.Null, "skill_cloakBoomerang", null)},
            {"teleportBullet", new SkillModel("teleportBullet", ChaResource.Null, ChaResource.Null, "skill_teleportBullet_fire", new AddBuffInfo[]{
                new AddBuffInfo(DesingerTables.BuffData.data["TeleportBulletPassive"], null, null, 1, 1, true, true)
            })},
            {"shoot1", new SkillModel("shoot1", ChaResource.Null, ChaResource.Null, "skill_shoot1", null)},
            {"shoot2", new SkillModel("shoot2", ChaResource.Null, ChaResource.Null, "skill_shoot2", null)},
            {"shoot3", new SkillModel("shoot3", ChaResource.Null, ChaResource.Null, "skill_shoot3", null)},
            {"poisonShot", new SkillModel("poisonShot", ChaResource.Null, ChaResource.Null, "skill_poisonShot", null)},
            {"stunShot", new SkillModel("stunShot", ChaResource.Null, ChaResource.Null, "skill_stunShot", null)},
            {"laser", new SkillModel("laser", ChaResource.Null, ChaResource.Null, "skill_laser", null)},
            {"lightning", new SkillModel("lightning", ChaResource.Null, ChaResource.Null, "skill_lightning", null)},
            {"haste", new SkillModel("haste", ChaResource.Null, ChaResource.Null, "skill_haste", null)},
            {"regen", new SkillModel("regen", ChaResource.Null, ChaResource.Null, "skill_regen", null)},
            {"debugInvincible", new SkillModel("debugInvincible", ChaResource.Null, ChaResource.Null, "skill_debugInvincible", null)},
            {"debugStopMove", new SkillModel("debugStopMove", ChaResource.Null, ChaResource.Null, "skill_debugStopMove", null)},
            {"debugFlow", new SkillModel("debugFlow", ChaResource.Null, ChaResource.Null, "skill_debugFlow", null)},
            {"roll", new SkillModel("roll", ChaResource.Null, ChaResource.Null, "skill_roll", null)}
        };
    }
}
