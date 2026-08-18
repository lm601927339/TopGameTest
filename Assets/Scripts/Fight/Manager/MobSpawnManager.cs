using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///刷怪管理器。
///</summary>
public class MobSpawnManager : MonoBehaviour
{
    [Tooltip("Whether to use the centralized design config as the default tuning source.")]
    public bool useDesignConfig = true;

    [Tooltip("保持最多有多少个怪物的数量，一旦超过就不刷人")]
    public int maxMob;

    [Tooltip("刷怪的周期。")]
    public float spawnPeriod = 10.0f;

    private float timePassed = 0;
    private bool justCreated = true;
    private int spawned = 0;

    private int mobSide = 2;

    private GameDesignConfig Config
    {
        get
        {
            return GameRuntime.Config;
        }
    }

    private void Awake()
    {
        if (useDesignConfig)
        {
            maxMob = Config.maxMob;
            spawnPeriod = Config.spawnPeriod;
            mobSide = Config.enemySide;
        }
    }

    private void FixedUpdate()
    {
        if (justCreated == true && maxMob > 0)
        {
            justCreated = false;
            Spawn();
        }

        timePassed += Time.fixedDeltaTime;
        if (timePassed >= spawnPeriod)
        {
            timePassed = 0;
            Spawn();
        }
    }

    private void Spawn()
    {
        GameObject[] cha = GameObject.FindGameObjectsWithTag(Config.characterTag);
        int toSpawn = maxMob;
        for (int i = 0; i < cha.Length; i++)
        {
            ChaState cs = cha[i].GetComponent<ChaState>();
            if (cs != null && cs.dead == false && cs.side == mobSide)
            {
                toSpawn -= 1;
            }
        }

        for (int i = 0; i < toSpawn; i++)
        {
            GameObject enemy = SceneVariants.CreateCharacter(
                Config.enemyPrefabPath,
                mobSide,
                SceneVariants.map.GetRandomPosForCharacter(new RectInt(0, 0, SceneVariants.map.MapWidth(), SceneVariants.map.MapHeight())),
                Config.BuildEnemyProperty(spawned),
                Random.Range(0.00f, 359.99f),
                Config.enemyViewAnimInfo
            );
            if (enemy)
            {
                enemy.AddComponent<SimpleAI>();
            }
            spawned += 1;
        }
    }
}
