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
