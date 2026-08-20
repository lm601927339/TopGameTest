using System;
using UnityEngine;

/// <summary>
/// 敌人职责分类。该分类用于内容配置、运行时标记和后续掉落/图鉴/UI，
/// 不把具体攻击逻辑硬编码在刷怪器中。
/// </summary>
public enum EnemyArchetype
{
    Assault = 0,
    Ranged = 1,
    Swift = 2,
    Armored = 3,
    // 预留给后续加入的特殊敌人，例如飞行、施法、分身或魅惑怪。
    Custom = 99
}

/// <summary>波次中敌人的等级。等级倍率由 MobSpawnManager 结算，职业负责基础玩法差异。</summary>
public enum WaveEnemyRank
{
    Normal,
    Elite,
    Boss
}

/// <summary>
/// 可配置的敌人模板。当前不要求新建预制体：多个职业可以复用同一角色视图，
/// 通过属性和攻击节奏产生实际差异；正式模型、动画与特效在后续内容阶段补齐。
/// </summary>
[Serializable]
public class EnemyDefinition
{
    /// <summary>稳定且唯一的敌人 ID，供波次、掉落和图鉴引用。</summary>
    public string id;
    /// <summary>供日志、HUD 与后续图鉴使用的中文名。</summary>
    public string displayName;
    public EnemyArchetype archetype;
    /// <summary>角色视图预制体路径；当前首批职业均可复用 Ghost。</summary>
    public string prefabPath;
    /// <summary>角色动画表 ID。</summary>
    public string viewAnimInfo;
    /// <summary>同一波次内的相对出现权重；0 表示本定义暂时禁用。</summary>
    [Min(0)] public int spawnWeight = 1;
    /// <summary>允许出现的最小波次，从 1 开始。</summary>
    [Min(1)] public int minWave = 1;
    /// <summary>允许出现的最大波次；0 表示没有上限。</summary>
    [Min(0)] public int maxWave;
    /// <summary>精英波能否选择该职业。</summary>
    public bool canBeElite = true;
    /// <summary>Boss 波能否选择该职业；避免普通职业被随机放大后直接充当 Boss。</summary>
    public bool canBeBoss;

    [Header("Property Multipliers")]
    [Min(0.01f)] public float hpMultiplier = 1f;
    [Min(0.01f)] public float attackMultiplier = 1f;
    [Min(0.01f)] public float moveSpeedMultiplier = 1f;
    [Min(0.01f)] public float actionSpeedMultiplier = 1f;

    [Header("Attack Behaviour")]
    [Tooltip("可见目标进入此距离后才开火；不同职业可有不同压迫距离。")]
    [Min(0.1f)] public float attackRange = 8f;
    [Tooltip("生成后首次攻击前的等待时间（秒）。")]
    [Min(0f)] public float initialAttackDelay = 0.75f;
    [Tooltip("每次攻击后的随机冷却区间（秒）。")]
    public Vector2 attackCooldownRange = new Vector2(2f, 5f);

    public EnemyDefinition(
        string id,
        string displayName,
        EnemyArchetype archetype,
        string prefabPath,
        string viewAnimInfo,
        int spawnWeight,
        int minWave,
        int maxWave,
        bool canBeElite,
        bool canBeBoss,
        float hpMultiplier,
        float attackMultiplier,
        float moveSpeedMultiplier,
        float actionSpeedMultiplier,
        float attackRange,
        float initialAttackDelay,
        Vector2 attackCooldownRange)
    {
        this.id = id;
        this.displayName = displayName;
        this.archetype = archetype;
        this.prefabPath = prefabPath;
        this.viewAnimInfo = viewAnimInfo;
        this.spawnWeight = spawnWeight;
        this.minWave = minWave;
        this.maxWave = maxWave;
        this.canBeElite = canBeElite;
        this.canBeBoss = canBeBoss;
        this.hpMultiplier = hpMultiplier;
        this.attackMultiplier = attackMultiplier;
        this.moveSpeedMultiplier = moveSpeedMultiplier;
        this.actionSpeedMultiplier = actionSpeedMultiplier;
        this.attackRange = attackRange;
        this.initialAttackDelay = initialAttackDelay;
        this.attackCooldownRange = attackCooldownRange;
    }

    /// <summary>仅验证可实例化的最小条件；权重为 0 仍是合法的“暂时关闭”配置。</summary>
    public bool IsStructurallyValid
    {
        get { return !string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(prefabPath); }
    }

    /// <summary>判断定义是否可以参与指定波次和等级的抽取。</summary>
    public bool CanSpawnIn(int wave, WaveEnemyRank rank)
    {
        if (!IsStructurallyValid || spawnWeight <= 0 || wave < Mathf.Max(1, minWave)) return false;
        if (maxWave > 0 && wave > maxWave) return false;
        if (rank == WaveEnemyRank.Elite && !canBeElite) return false;
        if (rank == WaveEnemyRank.Boss && !canBeBoss) return false;
        return true;
    }

    /// <summary>在旧敌人数值基础上应用职业倍率，等级倍率仍由波次管理器后置叠加。</summary>
    public ChaProperty ApplyTo(ChaProperty property)
    {
        property.hp = MultiplyPositive(property.hp, hpMultiplier);
        property.attack = MultiplyPositive(property.attack, attackMultiplier);
        property.moveSpeed = MultiplyPositive(property.moveSpeed, moveSpeedMultiplier);
        property.actionSpeed = MultiplyPositive(property.actionSpeed, actionSpeedMultiplier);
        return property;
    }

    private static int MultiplyPositive(int value, float multiplier)
    {
        if (value <= 0) return 0;
        return Mathf.Max(1, Mathf.RoundToInt(value * Mathf.Max(0.01f, multiplier)));
    }
}

/// <summary>
/// 敌人定义目录。GameDesignConfig 未填写敌人数组时使用内置首批职业；
/// 一旦填写数组，则该数组成为关卡的权威配置，非法或无候选时由调用方回退旧敌人字段。
/// </summary>
public static class EnemyCatalog
{
    public const string AssaultId = "assault_grunt";
    public const string RangedId = "ranged_grunt";
    public const string SwiftId = "swift_runner";
    public const string ArmoredId = "armored_colossus";

    private static readonly EnemyDefinition[] builtInDefinitions = new EnemyDefinition[]
    {
        new EnemyDefinition(
            AssaultId, "突击怪", EnemyArchetype.Assault, "Ghost", "Default_Gunner",
            3, 1, 0, true, false, 0.90f, 0.95f, 1.15f, 1.05f, 5.5f, 0.65f, new Vector2(1.5f, 3.0f)),
        new EnemyDefinition(
            RangedId, "枪手怪", EnemyArchetype.Ranged, "Ghost", "Default_Gunner",
            4, 1, 0, true, false, 1.00f, 1.00f, 1.00f, 1.00f, 8.0f, 0.75f, new Vector2(2.0f, 5.0f)),
        new EnemyDefinition(
            SwiftId, "疾行怪", EnemyArchetype.Swift, "Ghost", "Default_Gunner",
            2, 1, 0, true, false, 0.65f, 0.80f, 1.55f, 1.15f, 6.0f, 0.45f, new Vector2(1.2f, 2.4f)),
        new EnemyDefinition(
            ArmoredId, "重甲巨兽", EnemyArchetype.Armored, "Ghost", "Default_Gunner",
            1, 2, 0, true, true, 2.25f, 1.05f, 0.65f, 0.85f, 7.0f, 1.00f, new Vector2(2.8f, 4.8f))
    };

    /// <summary>返回有效来源；自定义数组为空时才回退内置职业，避免半配置关卡悄悄混入默认怪物。</summary>
    public static EnemyDefinition[] GetDefinitions(GameDesignConfig config)
    {
        if (config != null && config.enemyDefinitions != null && config.enemyDefinitions.Length > 0)
        {
            return config.enemyDefinitions;
        }

        return (EnemyDefinition[])builtInDefinitions.Clone();
    }

    /// <summary>
    /// 按波次、等级与权重选择敌人。randomValue 由调用方传入以便 EditMode 测试稳定复现。
    /// 没有合法候选时返回 false，调用方必须使用旧敌人字段兜底，保证错误配置不会卡住战斗。
    /// </summary>
    public static bool TrySelect(
        GameDesignConfig config,
        int wave,
        WaveEnemyRank rank,
        int randomValue,
        out EnemyDefinition definition)
    {
        EnemyDefinition[] definitions = GetDefinitions(config);
        int totalWeight = 0;
        for (int i = 0; i < definitions.Length; i++)
        {
            EnemyDefinition candidate = definitions[i];
            if (candidate == null || !candidate.CanSpawnIn(wave, rank)) continue;
            totalWeight += candidate.spawnWeight;
        }

        if (totalWeight <= 0)
        {
            definition = null;
            return false;
        }

        // 转为 uint 避免 int.MinValue 取绝对值溢出，让测试可覆盖全部 int 输入。
        int roll = (int)((uint)randomValue % (uint)totalWeight);
        for (int i = 0; i < definitions.Length; i++)
        {
            EnemyDefinition candidate = definitions[i];
            if (candidate == null || !candidate.CanSpawnIn(wave, rank)) continue;
            if (roll < candidate.spawnWeight)
            {
                definition = candidate;
                return true;
            }

            roll -= candidate.spawnWeight;
        }

        definition = null;
        return false;
    }

    /// <summary>构造兼容旧配置的运行时定义；仅在敌人数组没有任何合法候选时使用。</summary>
    public static EnemyDefinition CreateLegacyFallback(GameDesignConfig config)
    {
        GameDesignConfig safeConfig = config != null ? config : GameRuntime.Config;
        return new EnemyDefinition(
            "legacy_enemy",
            "默认敌人",
            EnemyArchetype.Ranged,
            safeConfig != null ? safeConfig.enemyPrefabPath : "Ghost",
            safeConfig != null ? safeConfig.enemyViewAnimInfo : "Default_Gunner",
            1, 1, 0, true, true,
            1f, 1f, 1f, 1f, 8f, 0.75f, new Vector2(2f, 5f));
    }
}

/// <summary>
/// 敌人运行时身份。无论通过波次、旧补怪还是 GameRuntime 生成，都用该组件记录职业，
/// 并把定义中的攻击节奏同步给 SimpleAI，从而不依赖预制体手工补组件。
/// </summary>
[DisallowMultipleComponent]
public class EnemyRuntime : MonoBehaviour
{
    [SerializeField] private string enemyId;
    [SerializeField] private string enemyDisplayName;
    [SerializeField] private EnemyArchetype archetype;

    public string EnemyId { get { return enemyId; } }
    public string EnemyDisplayName { get { return enemyDisplayName; } }
    public EnemyArchetype Archetype { get { return archetype; } }

    public bool Configure(EnemyDefinition definition)
    {
        if (definition == null || !definition.IsStructurallyValid) return false;

        enemyId = definition.id;
        enemyDisplayName = definition.displayName;
        archetype = definition.archetype;

        SimpleAI ai = GetComponent<SimpleAI>();
        if (ai != null)
        {
            ai.ConfigureAttack(
                definition.attackRange,
                definition.initialAttackDelay,
                definition.attackCooldownRange);
        }

        return true;
    }
}
