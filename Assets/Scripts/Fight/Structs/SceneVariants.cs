using System.Collections.Generic;
using UnityEngine;

public class SceneVariants
{
    public static MapInfo map;
    public static List<Wall> wallList;

    public static void RandomMap(int mapWidth, int mapHeight, Vector2 gridSize, float waterline = 6.00f)
    {
        GridInfo grass = new GridInfo("Wall");
        GridInfo[,] mGrids = new GridInfo[mapWidth, mapHeight];
        for (var i = 0; i < mapWidth; i++)
        {
            for (var j = 0; j < mapHeight; j++)
            {
                mGrids[i, j] = grass;
            }
        }
        map = new MapInfo(mGrids, gridSize);
    }

    public static GameObject MainActor()
    {
        return GameRuntime.MainActor;
    }

    public static void CreateBullet(BulletLauncher bulletLauncher)
    {
        if (GameRuntime.Manager) GameRuntime.Manager.CreateBullet(bulletLauncher);
    }

    public static void RemoveBullet(GameObject bullet, bool immediately = false)
    {
        if (GameRuntime.Manager) GameRuntime.Manager.RemoveBullet(bullet, immediately);
    }

    public static void CreateAoE(AoeLauncher aoeLauncher)
    {
        if (GameRuntime.Manager) GameRuntime.Manager.CreateAoE(aoeLauncher);
    }

    public static void RemoveAoE(GameObject aoe, bool immediately = false)
    {
        if (GameRuntime.Manager) GameRuntime.Manager.RemoveAoE(aoe, immediately);
    }

    public static void CreateTimeline(TimelineModel timelineModel, GameObject caster, object source)
    {
        if (GameRuntime.Timeline) GameRuntime.Timeline.AddTimeline(timelineModel, caster, source);
    }

    public static void CreateTimeline(TimelineObj timeline)
    {
        if (GameRuntime.Timeline) GameRuntime.Timeline.AddTimeline(timeline);
    }

    public static void CreateSightEffect(string prefab, Vector3 pos, float degree, string key = "", bool loop = false)
    {
        if (GameRuntime.Effect) GameRuntime.Effect.CreateSightEffect(prefab, pos, degree, key, loop);
    }

    public static void RemoveSightEffect(string key)
    {
        if (GameRuntime.Effect) GameRuntime.Effect.RemoveSightEffect(key);
    }

    public static void CreateDamage(GameObject attacker, GameObject target, Damage damage, float damageDegree, float criticalRate, DamageInfoTag[] tags)
    {
        if (GameRuntime.Damage) GameRuntime.Damage.DoDamage(attacker, target, damage, damageDegree, criticalRate, tags);
    }

    public static GameObject CreateCharacter(string prefab, int side, Vector3 pos, ChaProperty baseProp, float degree, string unitAnimInfo = "Default_Gunner", string[] tags = null)
    {
        return GameRuntime.Manager ? GameRuntime.Manager.CreateCharacter(prefab, side, pos, baseProp, degree, unitAnimInfo, tags) : null;
    }

    public static void PopUpNumberOnCharacter(GameObject cha, int value, bool asHeal = false, bool asCritical = false)
    {
        if (GameRuntime.PopText) GameRuntime.PopText.PopUpNumberOnCharacter(cha, value, asHeal, asCritical);
    }

    public static void PopUpStringOnCharacter(GameObject cha, string text, int size = 30)
    {
        if (GameRuntime.PopText) GameRuntime.PopText.PopUpStringOnCharacter(cha, text, size);
    }
}
