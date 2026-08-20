using System;
using UnityEngine;

[CreateAssetMenu(menuName = "BuffTopGame/Game Design Config", fileName = "GameDesignConfig")]
public class GameDesignConfig : ScriptableObject
{
    [Header("Catalog")]
    // Phase 1 catalog: prefer editable assets, then fall back to legacy static tables.
    public GameDesignCatalogAsset designCatalog;

    [Header("Scene Names")]
    public string gameManagerObjectName = "GameManager";
    public string rootObjectName = "GameObjectLayer";
    public string mainCameraObjectName = "Main Camera";
    public string playerHpObjectName = "PlayerHP";
    public string canvasObjectName = "Canvas";

    [Header("Tags")]
    public string characterTag = "Character";
    public string heroTag = "RPGHero";

    [Header("Map")]
    public int mapWidth = 10;
    public int mapHeight = 10;
    public float tileSpacing = 5.0f;
    public float tileDepth = 5.0f;
    public float waterline = 6.0f;

    [Header("Player")]
    public string playerPrefabPath = "ninja";
    public string playerViewAnimInfo = "Default_Gunner";
    public int playerSide = 1;
    public int playerMoveSpeed = 200;
    public IntRange playerHp = new IntRange(5000, 7000);
    public int playerAmmo = 600;
    public IntRange playerAttack = new IntRange(50, 70);
    public int playerActionSpeed = 100;
    public float playerBodyRadius = 0.25f;
    public float playerHitRadius = 0.25f;
    public string[] startingSkills = new[]
    {
        "fire",
        "fireMultiple",
        "roll",
        "spaceMonkeyBall",
        "homingMissle",
        "cloakBoomerang",
        "teleportBullet",
        "grenade",
        "explosiveBarrel",
    };

    [Header("Heroes")]
    /// <summary>本局使用的英雄 ID；为空时默认使用暗影潜行者，保证旧场景无需补配置也能运行。</summary>
    public string selectedHeroId = HeroCatalog.DefaultHeroId;
    /// <summary>可在 Inspector 覆盖内置英雄同 ID 的定义，后续扩展新英雄也放在这里。</summary>
    public HeroDefinition[] heroDefinitions = new HeroDefinition[0];
    /// <summary>是否在战斗 Canvas 中显示三英雄切换面板；关闭后仍会读取已保存的英雄选择。</summary>
    public bool showHeroSelectionPanel = true;

    [Header("Input")]
    public SkillBinding[] playerSkillBindings = new[]
    {
        new SkillBinding("Fire5", "homingMissle"),
        new SkillBinding("Fire4", "teleportBullet"),
        new SkillBinding("Fire3", "grenade"),
        new SkillBinding("Fire2", "fireMultiple"),
        new SkillBinding("Fire1", "cloakBoomerang"),
        new SkillBinding("Jump", "roll"),
    };

    [Header("Enemy")]
    public string enemyPrefabPath = "Ghost";
    public string enemyViewAnimInfo = "Default_Gunner";
    public int enemySide = 2;
    public int enemyMoveSpeedMin = 50;
    public int enemyMoveSpeedMax = 70;
    public int enemyHpBase = 50;
    public int enemyHpPerSpawn = 2;
    public int enemyAttackMin = 15;
    public int enemyAttackMax = 30;
    public int enemyAttackPerSpawn = 1;
    public int enemyActionSpeed = 100;
    public float enemyBodyRadius = 0.25f;
    public float enemyHitRadius = 0.40f;
    /// <summary>
    /// 敌人职业定义。为空时使用 EnemyCatalog 的首批内置职业；填入后本数组为该关卡的权威来源。
    /// 保留上方旧字段，供非法职业配置或旧关卡安全回退。
    /// </summary>
    public EnemyDefinition[] enemyDefinitions = new EnemyDefinition[0];
    public int maxMob = 0;
    public float spawnPeriod = 10.0f;

    [Header("Wave Progression")]
    /// <summary>开启后由 MobSpawnManager 按波次清场；关闭则保留旧的持续补怪模式。</summary>
    public bool enableWaveProgression = true;
    public int waveCount = 3;
    public int enemiesPerWave = 3;
    public float waveSpawnInterval = 0.6f;
    public float waveIntermission = 1.5f;
    public int eliteWave = 2;
    public int bossWave = 3;
    public float eliteHpMultiplier = 1.75f;
    public float eliteAttackMultiplier = 1.35f;
    public float bossHpMultiplier = 4.0f;
    public float bossAttackMultiplier = 2.0f;

    [Header("Prefabs")]
    public string characterRootPrefab = "CharacterObj";
    public string bulletPrefabPath = "BulletObj";
    public string wallPrefabPath = "Wall";
    public string aoePrefabPath = "Effect/AoeObj";
    public string popTextPrefabPath = "UI/PopText";

    public ChaProperty BuildPlayerProperty()
    {
        return new ChaProperty(
            playerMoveSpeed,
            playerHp.Roll(),
            playerAmmo,
            playerAttack.Roll(),
            playerActionSpeed,
            playerBodyRadius,
            playerHitRadius
        );
    }

    /// <summary>解析当前选择的英雄；失败时由调用方回退旧玩家配置，避免错误配置导致无法进入战斗。</summary>
    public bool TryGetSelectedHero(out HeroDefinition hero)
    {
        return HeroCatalog.TryGet(this, selectedHeroId, out hero);
    }

    public ChaProperty BuildEnemyProperty(int spawnedCount)
    {
        return new ChaProperty(
            UnityEngine.Random.Range(enemyMoveSpeedMin, enemyMoveSpeedMax + 1),
            enemyHpBase + spawnedCount * enemyHpPerSpawn,
            0,
            UnityEngine.Random.Range(enemyAttackMin, enemyAttackMax + 1) + spawnedCount * enemyAttackPerSpawn,
            enemyActionSpeed,
            enemyBodyRadius,
            enemyHitRadius
        );
    }
}

[Serializable]
public struct IntRange
{
    public int min;
    public int max;

    public IntRange(int min, int max)
    {
        this.min = min;
        this.max = max;
    }

    public int Roll()
    {
        int low = Mathf.Min(min, max);
        int high = Mathf.Max(min, max);
        return UnityEngine.Random.Range(low, high + 1);
    }
}

[Serializable]
public struct SkillBinding
{
    public string buttonName;
    public string skillId;

    public SkillBinding(string buttonName, string skillId)
    {
        this.buttonName = buttonName;
        this.skillId = skillId;
    }
}
