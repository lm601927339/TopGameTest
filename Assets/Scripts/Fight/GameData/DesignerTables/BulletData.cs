using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesingerTables
{
    
    ///<summary>
    ///BulletModel
    ///</summary>
    public class BulletData{
        public static Dictionary<string, BulletModel> data = new Dictionary<string, BulletModel>(){
            {"bullet2d", new BulletModel(
                "normal2d", "heroBullet2d",
                "", new object[0],
                "CommonBulletHit", new object[]{1.0f,0.05f,"Effect/HitEffect_A","Body"},
                "CommonBulletRemoved", new object[]{"Effect/HitEffect_A"}
            )},
            {"normal0", new BulletModel(
                "normal0", "BulletNormal0", 
                "", new object[0],
                "CommonBulletHit", new object[]{1.0f,0.05f,"Effect/HitEffect_A","Body"},
                "CommonBulletRemoved", new object[]{"Effect/HitEffect_A"}
            )},
            {"normal1", new BulletModel(
                "normal1", "BulletNormal1", 
                "", new object[0],                                                  //create
                "CommonBulletHit", new object[]{1.0f,0.05f,"Effect/HitEffect_A"},   //hit
                "CommonBulletRemoved", new object[]{"Effect/HitEffect_A"}           //remove
            )},
            //测试
            {"normal2", new BulletModel(
                "normal0", "BulletNormal0",
                "", new object[0],
                //"BulletBeatBackOnHit", new object[]{1.0f,0.05f,"Effect/HitEffect_A","Body"},//击退
                //"BulletSlowDownSpeedOnHit", new object[]{1.0f,0.05f,"Effect/HitEffect_A","Body"},//减速
                "BulletPoisoningOnHit", new object[]{},//中毒
                "CommonBulletRemoved", new object[]{"Effect/HitEffect_A"},
                MoveType.fly, false, 0.1f,3,5,true,false
                )},
            //海星大战
            {"poisonSlowBullet", new BulletModel(
                "poisonSlowBullet", "PoisonSlowBullet",
                "", new object[0],
                "BulletPoisonSlowOnHit", new object[]{1.0f,0.05f,"Effect/HitEffect_A","Body"},
                "CommonBulletRemoved", new object[]{"Effect/HitEffect_A"}
            )},
            {"stunBullet", new BulletModel(
                "stunBullet", "StunBullet",
                "", new object[0],
                "BulletStunOnHit", new object[]{1.0f,0.05f,"Effect/HitEffect_A","Body"},
                "CommonBulletRemoved", new object[]{"Effect/HitEffect_A"}
            )},
            {"laserBullet", new BulletModel(
                "laserBullet", "LaserBullet",
                "SetDistanceForward", new object[0],
                "CommonBulletHit", new object[]{1.8f,0.10f,"Effect/HitEffect_A","Body"},
                "CommonBulletRemoved", new object[]{"Effect/HitEffect_A"}
            )},
            {"lightningBullet", new BulletModel(
                "lightningBullet", "LightningBullet",
                "", new object[0],
                "CommonBulletHit", new object[]{1.4f,0.15f,"Effect/HitEffect_A","Body"},
                "CommonBulletRemoved", new object[]{"Effect/HitEffect_A"}
            )},
             {"starFish", new BulletModel(
                "normal1", "Starfish",
                "", new object[0],                                                  //create
                "CommonBulletHit", new object[]{1.0f,0.05f,"Effect/HitEffect_A"},   //hit
                "CommonBulletRemoved", new object[]{"Effect/HitEffect_A"},           //remove
                MoveType.ground, false, 0.5f,100,5f,true,true
            )},

            {"cloakBoomerang", new BulletModel(
                "cloakBoomerang", "Boomerang",
                "SetDistanceForward", new object[0],
                "CloakBoomerangHit", new object[]{1.0f,0.05f,"Effect/HitEffect_A"}, 
                "", new object[0],
                MoveType.fly, false, 0.5f, 99999, 0.5f, true, true)
            },
            {"teleportBullet", new BulletModel(
                "teleportBullet", "TeleportBulletEff",
                "RecordBullet", new object[0],
                "CommonBulletHit", new object[]{0.6f, 0.0f, "Effect/Star_B"},
                "CommonBulletRemoved", new object[]{"Effect/Star_B"}, 
                MoveType.fly, true
            )},
            {"boomball", new BulletModel(
                "boomball", "BombBall",
                "SetBombBouncing", new object[0],
                "CreateAoEOnHit", new object[]{new AoeLauncher(DesingerTables.AoEData.data["BoomExplosive"], null, Vector3.zero, 1.5f, 0, 0)},
                "CreateAoEOnRemoved", new object[]{
                    new AoeLauncher(DesingerTables.AoEData.data["StayingBoom"], null, Vector3.zero, 0.1f, 3.0f, 0),  //反正碰撞也没用，不如就直接…………
                    new AoeLauncher(DesingerTables.AoEData.data["BoomExplosive"], null, Vector3.zero, 1.5f, 0, 0)                    
                }
            )}
        };
    }
}
