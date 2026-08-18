using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class CollliderHandler : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
         Debug.Log(">>>>>>>>>>>>>>>>>  OnTriggerEnter2D    " + collision.name);
        HitTarget(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        //Debug.Log(">>>>>>>>>>>>>>>>>  OnTriggerStay2D");
        StayHarget(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
         Debug.Log(">>>>>>>>>>>>>>>>>  OnTriggerExit2D    " + collision.name);
        LevelTarget(collision);
    }
    /// <summary>
    /// 子弹中玩家 或者敌人
    /// </summary>
    /// <param name="collision"></param>
    private void HitTarget(Collider2D collision) {
        var targetTag = collision.gameObject.tag;
        var currTag = gameObject.tag;
        //layer EnemyBullet Enemy Hero HeroBullet EnemyAoe HeroAoe Wall
        //tag RPGHeroBullet  RPGHero RPGEnemy RPGEnemyBullet RPGAOE Character

        bool isBulletHit = false;
        if((currTag == "RPGEnemyBullet" && targetTag == "RPGHero")||(currTag == "RPGHeroBullet" && targetTag == "RPGEnemy")) 
        {
            isBulletHit = true;
        }

        if (currTag == "RPGHeroBullet" || currTag == "RPGEnemyBullet")
        {
            var bs = gameObject.transform.parent.parent.GetComponent<BulletState>();
            if (isBulletHit && bs!=null)
            {
                Debug.Log(currTag+  "  bullet hit tag: " + targetTag);
                var cs = collision.transform.parent.parent.GetComponent<ChaState>();

                int bSide = -1;
                if (bs.caster)
                {
                    ChaState bcs = bs.caster.GetComponent<ChaState>();
                    if (bcs)
                    {
                        bSide = bcs.side;
                    }
                }

                if (bs.CanHit(collision.transform.parent.parent.gameObject) == false)
                {
                    return;
                }
                if (!cs || cs.dead == true || cs.immuneTime > 0)
                {
                    return;
                }
                if ((bs.model.hitAlly == false && bSide == cs.side) ||
                            (bs.model.hitFoe == false && bSide != cs.side)
                        )
                    return;
                //命中了
                Debug.Log(">>>>>>>>>>>>>>>>>  命中了    ");
                bs.hp -= 1;

                if (bs.model.onHit != null)
                {
                    bs.model.onHit(gameObject.transform.parent.parent.gameObject, collision.transform.parent.parent.gameObject);
                }

                if (bs.hp > 0)
                {
                    bs.AddHitRecord(collision.transform.parent.parent.gameObject);
                }
                else
                {
                    Destroy(gameObject.transform.parent.parent);
                }

            }
            else if (targetTag == "RPGWall")
            {
                Debug.Log(currTag + "  bullet hit wall: " + targetTag);
                //命中了
                bs.hp -= 1;
                if (bs.hp > 0) //反弹
                {
                    Vector3 velocity = bs.velocity;
                    //Utils.GetCollisionNormal(wallRect, new Vector2(gameObject.transform.position.x, gameObject.transform.position.z), ref velocity);
                    //bs.SetMoveAngle(velocity);
                }
                else
                {
                    Destroy(gameObject.transform.parent.parent);
                }

            }

        }
        else if (currTag == "RPGAOE") {
            var aoeState = gameObject.GetComponent<AoeState>();
            if (targetTag == "RPGHero" || targetTag == "RPGEnemy")
            {
                Debug.Log(currTag + "  Aoe hit tag: " + targetTag);
                //捕获所有角色 进入
                aoeState.characterInRange.Add(collision.transform.parent.parent.gameObject);

                if (aoeState.model.onChaEnter != null)
                {
                    aoeState.model.onChaEnter(gameObject.transform.parent.parent.gameObject, new List<GameObject>() { collision.transform.parent.parent.gameObject });
                }
            }
            else if (targetTag == "RPGEnemyBullet" || targetTag == "RPGHeroBullet") {
                Debug.Log(currTag + "  Aoe hit bullet: " + targetTag);
                //捕获所有的子弹 离开
                aoeState.bulletInRange.Add(collision.transform.parent.parent.gameObject);

                if (aoeState.model.onBulletEnter != null)
                {
                    aoeState.model.onBulletEnter(gameObject.transform.parent.parent.gameObject, new List<GameObject>() { collision.transform.parent.parent.gameObject });
                }
            }
           
        }

    }

    void StayHarget(Collider2D collision) { 
        

     }

    void LevelTarget(Collider2D collision)
    {
        var targetTag = collision.transform.parent.parent.gameObject.tag;
        var currTag = gameObject.tag;
        //tag RPGHeroBullet  RPGHero RPGEnemy RPGEnemyBullet RPGAOE Character
        if (currTag == "Bullet")
        {
            var bs = gameObject.GetComponent<BulletState>();
            if (targetTag == "Player")
            {
            }
            else if (targetTag == "Wall")
            {

            }

        }
        else if (currTag == "RPGAOE")
        {
            //已经创建完成的
            //先抓角色离开事件
            var aoeState = gameObject.GetComponent<AoeState>();
            if (targetTag == "RPGHero" || targetTag == "RPGEnemy")
            {
                Debug.Log(currTag + "  Aoe level tag: " + targetTag);
                //捕获所有角色 离开
                aoeState.characterInRange.Remove(collision.transform.parent.parent.gameObject);

                if (aoeState.model.onChaLeave != null)
                {
                    aoeState.model.onChaLeave(gameObject.transform.parent.parent.gameObject, null);
                }
            }
            else if (targetTag == "RPGHeroBullet" || targetTag == "RPGEnemyBullet")
            {
                Debug.Log(currTag + "  Aoe level bullet: " + targetTag);
                //捕获所有的子弹 离开
                aoeState.bulletInRange.Remove(collision.transform.parent.parent.gameObject);

                if (aoeState.model.onBulletLeave != null)
                {
                    aoeState.model.onBulletLeave(gameObject.transform.parent.parent.gameObject, new List<GameObject>() { collision.transform.parent.parent.gameObject });
                }
            }
        }
    }
}
