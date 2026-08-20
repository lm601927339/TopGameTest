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
    public void BattleRunStateMachineLocksFirstTerminalResult()
    {
        BattleRunStateMachine victoryRun = new BattleRunStateMachine();
        Assert.That(victoryRun.TryVictory(true), Is.True);
        Assert.That(victoryRun.State, Is.EqualTo(BattleRunState.Victory));
        Assert.That(victoryRun.TryDefeat(), Is.False);
        Assert.That(victoryRun.State, Is.EqualTo(BattleRunState.Victory));

        BattleRunStateMachine defeatRun = new BattleRunStateMachine();
        Assert.That(defeatRun.TryVictory(false), Is.False);
        Assert.That(defeatRun.State, Is.EqualTo(BattleRunState.InProgress));
        Assert.That(defeatRun.TryDefeat(), Is.True);
        Assert.That(defeatRun.TryVictory(true), Is.False);
        Assert.That(defeatRun.State, Is.EqualTo(BattleRunState.Defeat));
    }

    [Test]
    public void EnemyAttackTimingOnlyAdvancesForValidVisibleTarget()
    {
        EnemyAttackTiming timing = new EnemyAttackTiming(0.5f);

        // 目标可见且在 8 米攻击距离内时，冷却正常推进并在 0.5 秒后允许攻击。
        Assert.That(timing.Advance(true, 64f, 8f, true, 0.25f), Is.False);
        Assert.That(timing.Advance(true, 64f, 8f, true, 0.25f), Is.True);

        timing.Restart(2f);
        // 隐身/遮挡后的不可见目标、超射程目标和被控制锁定的敌人都不能偷跑冷却。
        Assert.That(timing.Advance(false, 1f, 8f, true, 10f), Is.False);
        Assert.That(timing.RemainingCooldown, Is.EqualTo(2f).Within(0.001f));
        Assert.That(timing.Advance(true, 81f, 8f, true, 10f), Is.False);
        Assert.That(timing.RemainingCooldown, Is.EqualTo(2f).Within(0.001f));
        Assert.That(timing.Advance(true, 1f, 8f, false, 10f), Is.False);
        Assert.That(timing.RemainingCooldown, Is.EqualTo(2f).Within(0.001f));
        Assert.That(timing.Advance(true, 1f, 8f, true, 2f), Is.True);
    }

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
    public void WeatherVisibilityLimitsSightUnlessDetectionIgnoresWeather()
    {
        WeatherVisibilityService.ResetForTests();
        GameObject weatherObject = new GameObject("CloudyWeather");
        try
        {
            WeatherController weather = weatherObject.AddComponent<WeatherController>();
            // 本用例只验证全局视野，不创建局部环境体积，避免 EditMode 的运行时销毁限制干扰断言。
            weather.applyLocalVolume = false;
            weather.ConfigureVisibilityMultiplier(0.5f);
            weather.SetWeather(WeatherType.Cloudy, -1f, Vector2.zero, 1f, true);
            TargetDetectionSettings ordinarySight = new TargetDetectionSettings(12f, 360f, true);
            TargetDetectionSettings radarSight = new TargetDetectionSettings(12f, 360f, true, true);
            Vector3 targetPosition = new Vector3(8f, 0f, 0f);

            Assert.That(
                TargetVisibilityService.Evaluate(Vector3.zero, Vector3.right, targetPosition, ordinarySight),
                Is.EqualTo(TargetVisibilityResult.ObscuredByWeather));
            Assert.That(
                TargetVisibilityService.Evaluate(Vector3.zero, Vector3.right, targetPosition, radarSight),
                Is.EqualTo(TargetVisibilityResult.Visible));

            weather.ClearWeather();
            Assert.That(
                TargetVisibilityService.Evaluate(Vector3.zero, Vector3.right, targetPosition, ordinarySight),
                Is.EqualTo(TargetVisibilityResult.Visible));
        }
        finally
        {
            Object.DestroyImmediate(weatherObject);
            WeatherVisibilityService.ResetForTests();
        }
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
    public void SoundAlertIgnoresSameSideAndRespectsHearingRange()
    {
        SoundAlertService.Clear();
        try
        {
            int alertSequence = SoundAlertService.Report(SoundAlertKind.Gunfire, Vector3.zero, 1, 9f, 2f);
            SoundAlert alert;

            Assert.That(
                SoundAlertService.TryGetLatestHostileAlert(new Vector3(5f, 0f, 0f), 2, 10f, 0, out alert),
                Is.True);
            Assert.That(alert.sequence, Is.EqualTo(alertSequence));
            Assert.That(
                SoundAlertService.TryGetLatestHostileAlert(Vector3.zero, 1, 10f, 0, out alert),
                Is.False);
            Assert.That(
                SoundAlertService.TryGetLatestHostileAlert(new Vector3(10f, 0f, 0f), 2, 10f, 0, out alert),
                Is.False);
        }
        finally
        {
            SoundAlertService.Clear();
        }
    }

    [Test]
    public void SoundAlertRefreshesSearchInterestPosition()
    {
        SoundAlertService.Clear();
        try
        {
            EnemyAwarenessStateMachine awareness = new EnemyAwarenessStateMachine();
            EnemyAwarenessSettings settings = new EnemyAwarenessSettings(2f, 0.25f);
            Vector3 alertPosition = new Vector3(7f, -2f, 0f);
            SoundAlertService.Report(SoundAlertKind.Explosion, alertPosition, 1, 14f, 2f);
            SoundAlert alert;

            Assert.That(
                SoundAlertService.TryGetLatestHostileAlert(Vector3.zero, 2, 10f, 0, out alert),
                Is.True);
            awareness.InvestigateAlert(alert.position, settings);
            EnemyAwarenessDecision decision = awareness.Advance(false, Vector3.zero, Vector3.zero, 0.1f, settings);

            Assert.That(decision.state, Is.EqualTo(EnemyAwarenessState.Search));
            Assert.That(decision.shouldMoveToInterest, Is.True);
            Assert.That(decision.interestPosition, Is.EqualTo(alertPosition));
        }
        finally
        {
            SoundAlertService.Clear();
        }
    }

    [Test]
    public void VisibleTargetTakesPriorityOverSoundAlert()
    {
        EnemyAwarenessStateMachine awareness = new EnemyAwarenessStateMachine();
        EnemyAwarenessSettings settings = new EnemyAwarenessSettings(2f, 0.25f);
        Vector3 alertPosition = new Vector3(-4f, 1f, 0f);
        Vector3 visibleTargetPosition = new Vector3(3f, 2f, 0f);

        awareness.InvestigateAlert(alertPosition, settings);
        EnemyAwarenessDecision decision = awareness.Advance(
            true,
            visibleTargetPosition,
            Vector3.zero,
            0.1f,
            settings);

        Assert.That(decision.state, Is.EqualTo(EnemyAwarenessState.Chase));
        Assert.That(decision.interestPosition, Is.EqualTo(visibleTargetPosition));
        Assert.That(awareness.LastSeenPosition, Is.EqualTo(visibleTargetPosition));
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
