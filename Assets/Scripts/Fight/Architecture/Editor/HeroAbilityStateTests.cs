using System.Collections.Generic;
using DesignerScripts;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// 三英雄能力状态的编辑器回归测试。
/// 重点保证毒系英雄的中毒抗性不会只缩短 HUD 状态而遗漏 DOT/减速 Buff。
/// </summary>
public class HeroAbilityStateTests
{
    [Test]
    public void AvailableHeroesUseConfiguredOverrideAndIncludeNewHero()
    {
        GameDesignConfig config = ScriptableObject.CreateInstance<GameDesignConfig>();
        try
        {
            HeroDefinition overriddenShadow = CreateHero(HeroCatalog.ShadowStalkerId, "测试暗影", HeroArchetype.Stealth);
            HeroDefinition customHero = CreateHero("test_custom_hero", "测试扩展英雄", HeroArchetype.Custom);
            customHero.roleDisplayName = "召唤";
            customHero.specialtySummary = "召唤、护盾";
            config.heroDefinitions = new[] { overriddenShadow, customHero };

            HeroDefinition[] heroes = HeroCatalog.GetAvailableHeroes(config);

            Assert.That(heroes.Length, Is.EqualTo(4));
            Assert.That(heroes[0].displayName, Is.EqualTo("测试暗影"));
            Assert.That(heroes[3].id, Is.EqualTo("test_custom_hero"));
            Assert.That(HeroCatalog.GetRoleDisplayName(heroes[3]), Is.EqualTo("召唤"));
            Assert.That(HeroCatalog.GetSpecialtySummary(heroes[3]), Is.EqualTo("召唤、护盾"));
        }
        finally
        {
            Object.DestroyImmediate(config);
        }
    }

    [Test]
    public void PoisonSlowBuffUsesResistanceAdjustedDuration()
    {
        GameObject target = new GameObject("PoisonResistanceTarget");
        GameObject bullet = new GameObject("PoisonResistanceBullet");
        try
        {
            ChaState targetState = target.AddComponent<ChaState>();
            targetState.InitBaseProp(new ChaProperty(100, 100, 0, 0));
            AbilityStateResistanceProfile resistance = target.AddComponent<AbilityStateResistanceProfile>();
            resistance.Configure(new[] { new AbilityStateResistance(ChaAbilityStateType.Poisoned, 0.60f) });
            bullet.AddComponent<BulletState>();

            BulletScript.BulletPoisonSlowOnHit(bullet, target);

            Assert.That(targetState.GetStateRemaining(ChaAbilityStateType.Poisoned), Is.EqualTo(2f).Within(0.001f));
            List<BuffObj> poisonBuffs = targetState.GetBuffById("SkillPoisonSlowOnHit", new List<GameObject> { bullet });
            Assert.That(poisonBuffs.Count, Is.EqualTo(1));
            Assert.That(poisonBuffs[0].duration, Is.EqualTo(2f).Within(0.001f));
        }
        finally
        {
            Object.DestroyImmediate(bullet);
            Object.DestroyImmediate(target);
        }
    }

    [Test]
    public void DefaultEnemyTargetRuleRejectsInvisibleHero()
    {
        GameObject enemy = new GameObject("DefaultEnemy");
        GameObject hero = new GameObject("InvisibleHero");
        try
        {
            ChaState enemyState = enemy.AddComponent<ChaState>();
            enemyState.side = 2;
            ChaState heroState = hero.AddComponent<ChaState>();
            heroState.side = 1;
            heroState.AddState(ChaAbilityStateType.Invisible, "test", 5f);
            enemy.transform.position = Vector3.zero;
            hero.transform.position = Vector3.right;

            // EditMode 不会驱动 ChaState 的 OnEnable 注册角色列表；这里直接验证
            // AI 与目标服务共用的筛选谓词，避免测试结果依赖 Unity 场景生命周期。
            Assert.That(TargetService.IsTargetable(enemyState, heroState), Is.False);
        }
        finally
        {
            Object.DestroyImmediate(hero);
            Object.DestroyImmediate(enemy);
        }
    }

    [Test]
    public void DetectionEnemyTargetRuleAcceptsInvisibleHero()
    {
        GameObject enemy = new GameObject("DetectionEnemy");
        GameObject hero = new GameObject("InvisibleHero");
        try
        {
            ChaState enemyState = enemy.AddComponent<ChaState>();
            enemyState.side = 2;
            TargetDetectionProfile detection = enemy.AddComponent<TargetDetectionProfile>();
            detection.Configure(true);
            ChaState heroState = hero.AddComponent<ChaState>();
            heroState.side = 1;
            heroState.AddState(ChaAbilityStateType.Invisible, "test", 5f);
            enemy.transform.position = Vector3.zero;
            hero.transform.position = Vector3.right;

            Assert.That(
                TargetService.IsTargetable(
                    enemyState,
                    heroState,
                    detection.CanDetectInvisible,
                    detection.CanTargetInvincible),
                Is.True);
        }
        finally
        {
            Object.DestroyImmediate(hero);
            Object.DestroyImmediate(enemy);
        }
    }

    [Test]
    public void EnemyAwarenessSearchesLastSeenPositionBeforePatrolling()
    {
        EnemyAwarenessStateMachine awareness = new EnemyAwarenessStateMachine();
        EnemyAwarenessSettings settings = new EnemyAwarenessSettings(2f, 0.25f);
        Vector3 lastSeenPosition = new Vector3(4f, 2f, 0f);

        EnemyAwarenessDecision chase = awareness.Advance(true, lastSeenPosition, Vector3.zero, 0f, settings);
        EnemyAwarenessDecision search = awareness.Advance(false, Vector3.zero, Vector3.zero, 0.1f, settings);
        EnemyAwarenessDecision arrivedSearch = awareness.Advance(false, Vector3.zero, lastSeenPosition, 0.1f, settings);
        EnemyAwarenessDecision patrol = awareness.Advance(false, Vector3.zero, lastSeenPosition, 2f, settings);

        Assert.That(chase.state, Is.EqualTo(EnemyAwarenessState.Chase));
        Assert.That(search.state, Is.EqualTo(EnemyAwarenessState.Search));
        Assert.That(search.shouldMoveToInterest, Is.True);
        Assert.That(search.interestPosition, Is.EqualTo(lastSeenPosition));
        Assert.That(arrivedSearch.shouldMoveToInterest, Is.False);
        Assert.That(patrol.state, Is.EqualTo(EnemyAwarenessState.Patrol));
        Assert.That(awareness.HasLastSeenPosition, Is.False);
    }

    [Test]
    public void EnemyAwarenessRefreshesLastSeenPositionWhileTargetVisible()
    {
        EnemyAwarenessStateMachine awareness = new EnemyAwarenessStateMachine();
        EnemyAwarenessSettings settings = new EnemyAwarenessSettings(2f, 0.25f);
        Vector3 firstPosition = new Vector3(1f, 0f, 0f);
        Vector3 latestPosition = new Vector3(6f, -3f, 0f);

        awareness.Advance(true, firstPosition, Vector3.zero, 0f, settings);
        awareness.Advance(true, latestPosition, Vector3.zero, 0.5f, settings);
        EnemyAwarenessDecision search = awareness.Advance(false, Vector3.zero, Vector3.zero, 0.1f, settings);

        Assert.That(search.state, Is.EqualTo(EnemyAwarenessState.Search));
        Assert.That(search.interestPosition, Is.EqualTo(latestPosition));
        Assert.That(awareness.LastSeenPosition, Is.EqualTo(latestPosition));
    }

    [Test]
    public void TargetVisibilityRejectsOutOfRangeAndOutsideViewAngle()
    {
        TargetDetectionSettings settings = new TargetDetectionSettings(5f, 90f, true);

        TargetVisibilityResult outOfRange = TargetVisibilityService.Evaluate(
            Vector3.zero, Vector3.right, new Vector3(6f, 0f, 0f), settings);
        TargetVisibilityResult outsideViewAngle = TargetVisibilityService.Evaluate(
            Vector3.zero, Vector3.right, new Vector3(0f, 4f, 0f), settings);

        Assert.That(outOfRange, Is.EqualTo(TargetVisibilityResult.OutOfRange));
        Assert.That(outsideViewAngle, Is.EqualTo(TargetVisibilityResult.OutsideViewAngle));
    }

    [Test]
    public void TargetVisibilitySupportsOmnidirectionalVision()
    {
        TargetDetectionSettings settings = new TargetDetectionSettings(5f, 360f, true);

        TargetVisibilityResult result = TargetVisibilityService.Evaluate(
            Vector3.zero, Vector3.right, new Vector3(-4f, 0f, 0f), settings);

        Assert.That(result, Is.EqualTo(TargetVisibilityResult.Visible));
    }

    [Test]
    public void MapLineOfSightBlocksOpaqueGroundGrid()
    {
        GridInfo[,] grid = new GridInfo[5, 1];
        for (int x = 0; x < grid.GetLength(0); x++) grid[x, 0] = new GridInfo("Ground", true, true);
        grid[2, 0] = new GridInfo("Wall", false, true);
        MapInfo map = new MapInfo(grid, Vector2.one);

        Assert.That(map.HasGroundObstacleBetween(Vector3.zero, new Vector3(4f, 0f, 0f)), Is.True);
        Assert.That(map.HasGroundObstacleBetween(Vector3.zero, new Vector3(1f, 0f, 0f)), Is.False);
    }

    [Test]
    public void PoisonImmuneTargetReceivesNeitherStateNorBuff()
    {
        GameObject target = new GameObject("PoisonImmuneTarget");
        GameObject bullet = new GameObject("PoisonImmuneBullet");
        try
        {
            ChaState targetState = target.AddComponent<ChaState>();
            targetState.InitBaseProp(new ChaProperty(100, 100, 0, 0));
            AbilityStateResistanceProfile resistance = target.AddComponent<AbilityStateResistanceProfile>();
            resistance.Configure(new[] { new AbilityStateResistance(ChaAbilityStateType.Poisoned, 1f) });
            bullet.AddComponent<BulletState>();

            BulletScript.BulletPoisonSlowOnHit(bullet, target);

            Assert.That(targetState.HasState(ChaAbilityStateType.Poisoned), Is.False);
            Assert.That(targetState.GetBuffById("SkillPoisonSlowOnHit", new List<GameObject> { bullet }), Is.Empty);
        }
        finally
        {
            Object.DestroyImmediate(bullet);
            Object.DestroyImmediate(target);
        }
    }

    private static HeroDefinition CreateHero(string id, string displayName, HeroArchetype archetype)
    {
        return new HeroDefinition(
            id,
            displayName,
            archetype,
            "ninja",
            "Default_Gunner",
            new ChaProperty(100, 100, 10, 10),
            new[] { "fire" },
            new[] { new SkillBinding("Fire1", "fire") }
        );
    }
}
