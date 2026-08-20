using System;
using UnityEngine;

/// <summary>一次击杀发放的局内奖励。RewardGranted 事件可由经验条、货币 HUD 和后续拾取表现订阅。</summary>
public struct BattleRewardGrant
{
    public int experience;
    public int currency;
    public int levelBefore;
    public int levelAfter;
    public int totalKills;
    public bool leveledUp;

    public BattleRewardGrant(int experience, int currency, int levelBefore, int levelAfter, int totalKills)
    {
        this.experience = experience;
        this.currency = currency;
        this.levelBefore = levelBefore;
        this.levelAfter = levelAfter;
        this.totalKills = totalKills;
        this.leveledUp = levelAfter > levelBefore;
    }
}

/// <summary>
/// 纯逻辑的局内成长状态。经验达到当前等级阈值后自动升级，保留溢出经验，便于 EditMode 独立验证。
/// </summary>
public sealed class BattleRewardState
{
    private readonly int baseExperiencePerLevel;
    private int level;
    private int experience;
    private int currency;
    private int totalKills;

    public int Level { get { return level; } }
    public int Experience { get { return experience; } }
    public int Currency { get { return currency; } }
    public int TotalKills { get { return totalKills; } }
    public int ExperienceToNextLevel { get { return level * baseExperiencePerLevel; } }

    public BattleRewardState(int startingLevel = 1, int startingExperience = 0, int startingCurrency = 0, int baseExperiencePerLevel = 100)
    {
        this.baseExperiencePerLevel = Mathf.Max(1, baseExperiencePerLevel);
        level = Mathf.Max(1, startingLevel);
        experience = Mathf.Max(0, startingExperience);
        currency = Mathf.Max(0, startingCurrency);
        totalKills = 0;
        NormalizeExperience();
    }

    /// <summary>应用一次击杀奖励；负值会被钳制，避免错误配置倒扣成长。</summary>
    public BattleRewardGrant Apply(int rewardExperience, int rewardCurrency)
    {
        int safeExperience = Mathf.Max(0, rewardExperience);
        int safeCurrency = Mathf.Max(0, rewardCurrency);
        int levelBefore = level;
        experience += safeExperience;
        currency += safeCurrency;
        totalKills += 1;
        NormalizeExperience();
        return new BattleRewardGrant(safeExperience, safeCurrency, levelBefore, level, totalKills);
    }

    private void NormalizeExperience()
    {
        while (experience >= ExperienceToNextLevel)
        {
            experience -= ExperienceToNextLevel;
            level += 1;
        }
    }
}

/// <summary>
/// 订阅统一死亡事件的局内奖励服务。当前奖励直接进入局内状态，不创建拾取物预制体；
/// RewardGranted 已为后续实体掉落、经验条动画和随机强化界面预留统一出口。
/// </summary>
[DisallowMultipleComponent]
public class BattleRewardService : MonoBehaviour
{
    private GameDesignConfig config;
    private GameObject playerActor;
    private bool initialized;
    private bool subscribed;
    private BattleRewardState state;

    public BattleRewardState State { get { return state; } }
    public bool IsInitialized { get { return initialized; } }
    public event Action<BattleRewardGrant> RewardGranted;

    private void OnEnable()
    {
        SubscribeDeathEvents();
    }

    private void OnDisable()
    {
        UnsubscribeDeathEvents();
    }

    /// <summary>由 GameManager 注入当前局配置和主角，重开场景时奖励状态从零开始。</summary>
    public void ConfigureForBattle(GameDesignConfig gameConfig, GameObject mainActor)
    {
        // 运行时 AddComponent 和 EditMode 夹具的 OnEnable 时机不同；配置入口必须保证已订阅。
        SubscribeDeathEvents();
        config = gameConfig != null ? gameConfig : GameRuntime.Config;
        playerActor = mainActor;
        state = new BattleRewardState(1, 0, 0, config != null ? config.rewardExperiencePerLevel : 100);
        initialized = true;
    }

    /// <summary>
    /// 只给主角造成的敌人击杀发放奖励；环境伤害、敌人互殴和未归属攻击不会凭空增加玩家成长。
    /// </summary>
    private void OnCharacterDied(CharacterDeathEvent death)
    {
        if (!initialized || state == null || config == null || !config.enableBattleRewards) return;
        if (death == null || death.isPlayer || !death.IsEnemy) return;

        GameObject effectivePlayer = playerActor != null ? playerActor : GameRuntime.MainActor;
        if (effectivePlayer == null || death.attackerObject != effectivePlayer) return;

        WaveEnemyRank rank = death.waveMarker != null ? death.waveMarker.rank : WaveEnemyRank.Normal;
        int experience = GetExperienceReward(rank);
        int currency = GetCurrencyReward(rank);
        if (experience <= 0 && currency <= 0) return;

        BattleRewardGrant grant = state.Apply(experience, currency);
        Debug.Log(
            "[BattleRewardService] Granted exp=" + grant.experience +
            " currency=" + grant.currency +
            " level=" + grant.levelAfter +
            " kills=" + grant.totalKills);
        Action<BattleRewardGrant> callbacks = RewardGranted;
        if (callbacks != null) callbacks(grant);
    }

    private int GetExperienceReward(WaveEnemyRank rank)
    {
        if (rank == WaveEnemyRank.Boss) return Mathf.Max(0, config.bossEnemyExperience);
        if (rank == WaveEnemyRank.Elite) return Mathf.Max(0, config.eliteEnemyExperience);
        return Mathf.Max(0, config.normalEnemyExperience);
    }

    private int GetCurrencyReward(WaveEnemyRank rank)
    {
        if (rank == WaveEnemyRank.Boss) return Mathf.Max(0, config.bossEnemyCurrency);
        if (rank == WaveEnemyRank.Elite) return Mathf.Max(0, config.eliteEnemyCurrency);
        return Mathf.Max(0, config.normalEnemyCurrency);
    }

    private void SubscribeDeathEvents()
    {
        if (subscribed) return;
        CharacterDeathService.CharacterDied += OnCharacterDied;
        subscribed = true;
    }

    private void UnsubscribeDeathEvents()
    {
        if (!subscribed) return;
        CharacterDeathService.CharacterDied -= OnCharacterDied;
        subscribed = false;
    }
}
