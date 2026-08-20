using UnityEngine;

/// <summary>挂在波次生成单位上的运行时标记，供结算、Boss 血条和后续掉落系统读取。</summary>
[DisallowMultipleComponent]
public class WaveEnemyMarker : MonoBehaviour
{
    public int wave;
    public WaveEnemyRank rank;
    /// <summary>职业唯一 ID；掉落、图鉴和死亡事件只依赖此稳定值，不读取 GameObject 名称。</summary>
    public string enemyId;
    /// <summary>职业分类，供 Boss 进度和后续策划规则筛选。</summary>
    public EnemyArchetype archetype;

    public bool IsBoss { get { return rank == WaveEnemyRank.Boss; } }
    public bool IsElite { get { return rank == WaveEnemyRank.Elite; } }

    public void Configure(int waveNumber, WaveEnemyRank enemyRank, EnemyDefinition definition)
    {
        wave = waveNumber;
        rank = enemyRank;
        enemyId = definition != null ? definition.id : "legacy_enemy";
        archetype = definition != null ? definition.archetype : EnemyArchetype.Ranged;
    }
}

/// <summary>
/// 波次刷怪管理器。
/// 旧的 maxMob 维持数量模式保留为关闭波次时的兼容路径；开启后按“生成配额 → 清场 → 下一波”推进。
/// </summary>
public class MobSpawnManager : MonoBehaviour
{
    [Tooltip("Whether to use the centralized design config as the default tuning source.")]
    public bool useDesignConfig = true;

    [Header("Legacy Spawn")]
    [Tooltip("关闭波次模式时，保持场上最多有多少个怪物。")]
    public int maxMob;

    [Tooltip("关闭波次模式时的补怪周期。")]
    public float spawnPeriod = 10.0f;

    [Header("Wave Spawn")]
    [Tooltip("按波次生成并在最终波清场后结束战斗。")]
    public bool enableWaveProgression = true;

    [Min(1)] public int waveCount = 3;
    [Min(1)] public int enemiesPerWave = 3;
    [Min(0.05f)] public float waveSpawnInterval = 0.6f;
    [Min(0f)] public float waveIntermission = 1.5f;
    [Min(1)] public int eliteWave = 2;
    [Min(1)] public int bossWave = 3;
    [Min(1.0f)] public float eliteHpMultiplier = 1.75f;
    [Min(1.0f)] public float eliteAttackMultiplier = 1.35f;
    [Min(1.0f)] public float bossHpMultiplier = 4.0f;
    [Min(1.0f)] public float bossAttackMultiplier = 2.0f;

    private float timePassed;
    private bool initialized;
    private int spawned;
    private int spawnedInWave;
    private int waveQuota;
    private int mobSide = 2;
    private GameDesignConfig config;
    private WaveProgressionStateMachine progression;
    private bool loggedLegacyDefinitionFallback;

    public WaveProgressionState WaveState
    {
        get { return progression != null ? progression.State : WaveProgressionState.NotStarted; }
    }

    public int CurrentWave
    {
        get { return progression != null ? progression.CurrentWave : 0; }
    }

    public bool IsCompleted
    {
        get { return progression != null && progression.IsCompleted; }
    }

    private void Awake()
    {
        // 先使用 Inspector 默认值；GameManager.Start 会用实际设计配置调用 ConfigureForBattle。
        if (!useDesignConfig) ConfigureForBattle(null);
    }

    private void Start()
    {
        if (!initialized) ConfigureForBattle(GameRuntime.Config);
    }

    /// <summary>由 GameManager 显式注入配置，确保自动创建和场景内组件走同一套参数。</summary>
    public void ConfigureForBattle(GameDesignConfig gameConfig)
    {
        if (initialized) return;
        config = gameConfig != null ? gameConfig : GameRuntime.Config;
        if (useDesignConfig && config != null)
        {
            maxMob = config.maxMob;
            spawnPeriod = config.spawnPeriod;
            mobSide = config.enemySide;
            enableWaveProgression = config.enableWaveProgression;
            waveCount = config.waveCount;
            enemiesPerWave = config.enemiesPerWave;
            waveSpawnInterval = config.waveSpawnInterval;
            waveIntermission = config.waveIntermission;
            eliteWave = config.eliteWave;
            bossWave = config.bossWave;
            eliteHpMultiplier = config.eliteHpMultiplier;
            eliteAttackMultiplier = config.eliteAttackMultiplier;
            bossHpMultiplier = config.bossHpMultiplier;
            bossAttackMultiplier = config.bossAttackMultiplier;
        }

        waveCount = Mathf.Max(0, waveCount);
        enemiesPerWave = Mathf.Max(1, enemiesPerWave);
        waveSpawnInterval = Mathf.Max(0.05f, waveSpawnInterval);
        waveIntermission = Mathf.Max(0f, waveIntermission);
        initialized = true;

        if (enableWaveProgression)
        {
            progression = new WaveProgressionStateMachine(waveCount, waveIntermission);
            progression.Begin();
            BeginWave();
            if (progression.IsCompleted) ResolveVictoryIfNeeded();
        }
    }

    private void FixedUpdate()
    {
        if (!initialized || !GameRuntime.Manager || !GameRuntime.Manager.IsBattleInProgress) return;
        if (enableWaveProgression) TickWaveProgression();
        else TickLegacySpawn();
    }

    private void TickWaveProgression()
    {
        if (progression == null || progression.IsCompleted) return;
        if (progression.State == WaveProgressionState.Spawning)
        {
            timePassed += Time.fixedDeltaTime;
            int aliveEnemyCount = GameRuntime.CountCharactersBySide(mobSide, true);
            int maxConcurrent = maxMob > 0 ? maxMob : enemiesPerWave;
            if (spawnedInWave < waveQuota && aliveEnemyCount < maxConcurrent && timePassed >= waveSpawnInterval)
            {
                timePassed = 0f;
                if (SpawnWaveEnemy())
                {
                    spawnedInWave += 1;
                    if (spawnedInWave >= waveQuota) progression.MarkSpawnQuotaReached();
                }
            }
        }
        else if (progression.State == WaveProgressionState.Clearing)
        {
            if (progression.MarkCleared(GameRuntime.CountCharactersBySide(mobSide, true)) && progression.IsCompleted)
            {
                ResolveVictoryIfNeeded();
            }
        }
        else if (progression.State == WaveProgressionState.Intermission)
        {
            if (progression.Tick(Time.fixedDeltaTime)) BeginWave();
        }
    }

    private void TickLegacySpawn()
    {
        if (maxMob <= 0) return;
        timePassed += Time.fixedDeltaTime;
        if (timePassed >= spawnPeriod)
        {
            timePassed = 0f;
            SpawnLegacy();
        }
    }

    private void BeginWave()
    {
        timePassed = waveSpawnInterval;
        spawnedInWave = 0;
        waveQuota = GetWaveQuota(progression.CurrentWave);
        Debug.Log("[MobSpawnManager] Wave started: " + progression.CurrentWave + "/" + waveCount + " quota=" + waveQuota);
    }

    private int GetWaveQuota(int wave)
    {
        if (wave == bossWave) return 1;
        if (wave == eliteWave) return Mathf.Max(1, Mathf.CeilToInt(enemiesPerWave * 0.5f));
        return enemiesPerWave;
    }

    private WaveEnemyRank GetWaveRank(int wave)
    {
        if (wave == bossWave) return WaveEnemyRank.Boss;
        if (wave == eliteWave) return WaveEnemyRank.Elite;
        return WaveEnemyRank.Normal;
    }

    private bool SpawnWaveEnemy()
    {
        WaveEnemyRank rank = GetWaveRank(progression.CurrentWave);
        EnemyDefinition definition = SelectEnemyDefinition(progression.CurrentWave, rank);
        ChaProperty property = BuildWaveProperty(rank, definition);
        GameObject enemy = SceneVariants.CreateCharacter(
            definition.prefabPath,
            mobSide,
            SceneVariants.map.GetRandomPosForCharacter(new RectInt(0, 0, SceneVariants.map.MapWidth(), SceneVariants.map.MapHeight())),
            property,
            Random.Range(0.00f, 359.99f),
            definition.viewAnimInfo
        );
        if (!enemy) return false;

        enemy.AddComponent<SimpleAI>();
        ConfigureEnemyRuntime(enemy, definition);
        WaveEnemyMarker marker = enemy.AddComponent<WaveEnemyMarker>();
        marker.Configure(progression.CurrentWave, rank, definition);
        spawned += 1;
        Debug.Log("[MobSpawnManager] Spawned " + rank + " enemy=" + definition.id + " in wave " + progression.CurrentWave + ".");
        return true;
    }

    private ChaProperty BuildWaveProperty(WaveEnemyRank rank, EnemyDefinition definition)
    {
        ChaProperty property = config.BuildEnemyProperty(spawned);
        if (definition != null) property = definition.ApplyTo(property);
        float hpMultiplier = rank == WaveEnemyRank.Boss ? bossHpMultiplier : rank == WaveEnemyRank.Elite ? eliteHpMultiplier : 1f;
        float attackMultiplier = rank == WaveEnemyRank.Boss ? bossAttackMultiplier : rank == WaveEnemyRank.Elite ? eliteAttackMultiplier : 1f;
        property.hp = Mathf.Max(1, Mathf.RoundToInt(property.hp * hpMultiplier));
        property.attack = Mathf.Max(1, Mathf.RoundToInt(property.attack * attackMultiplier));
        return property;
    }

    private void SpawnLegacy()
    {
        int aliveEnemyCount = GameRuntime.CountCharactersBySide(mobSide, true);
        int toSpawn = Mathf.Max(0, maxMob - aliveEnemyCount);
        for (int i = 0; i < toSpawn; i++)
        {
            EnemyDefinition definition = SelectEnemyDefinition(1, WaveEnemyRank.Normal);
            GameObject enemy = SceneVariants.CreateCharacter(
                definition.prefabPath,
                mobSide,
                SceneVariants.map.GetRandomPosForCharacter(new RectInt(0, 0, SceneVariants.map.MapWidth(), SceneVariants.map.MapHeight())),
                definition.ApplyTo(config.BuildEnemyProperty(spawned)),
                Random.Range(0.00f, 359.99f),
                definition.viewAnimInfo
            );
            if (enemy)
            {
                enemy.AddComponent<SimpleAI>();
                ConfigureEnemyRuntime(enemy, definition);
            }
            spawned += 1;
        }
    }

    /// <summary>按当前波次抽职业；错误或未完成的关卡配置会回退旧字段，不能阻塞生成流程。</summary>
    private EnemyDefinition SelectEnemyDefinition(int wave, WaveEnemyRank rank)
    {
        EnemyDefinition definition;
        if (EnemyCatalog.TrySelect(config, wave, rank, Random.Range(0, int.MaxValue), out definition)) return definition;

        if (!loggedLegacyDefinitionFallback)
        {
            loggedLegacyDefinitionFallback = true;
            Debug.LogWarning("[MobSpawnManager] No eligible enemy definition. Falling back to legacy enemy fields.");
        }
        return EnemyCatalog.CreateLegacyFallback(config);
    }

    /// <summary>统一为波次和旧补怪路径挂职业身份，并同步攻击节奏。</summary>
    private static void ConfigureEnemyRuntime(GameObject enemy, EnemyDefinition definition)
    {
        if (!enemy) return;
        EnemyRuntime runtime = enemy.AddComponent<EnemyRuntime>();
        runtime.Configure(definition);
    }

    private void ResolveVictoryIfNeeded()
    {
        if (GameRuntime.Manager != null) GameRuntime.Manager.ResolveVictory("已完成全部 " + waveCount + " 波敌人");
    }
}
