using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>局内强化的类型。属性强化不依赖具体技能，技能强化严格受当前英雄技能契约限制。</summary>
public enum BattleUpgradeKind
{
    Property = 0,
    SkillLevel = 1,
    NewSkill = 2
}

/// <summary>一条可由策划配置的局内强化定义。</summary>
[Serializable]
public class BattleUpgradeDefinition
{
    public string id;
    public string displayName;
    [TextArea(1, 3)] public string description;
    public BattleUpgradeKind kind;
    public ChaProperty propertyBonus;
    /// <summary>技能升级/新技能的目标 ID；技能升级为空时由目录为每个已拥有技能展开候选。</summary>
    public string targetSkillId;
    [Min(0)] public int weight = 1;
    /// <summary>允许同一局多次出现；技能等级强化通常允许重复。</summary>
    public bool repeatable;

    public BattleUpgradeDefinition(
        string id,
        string displayName,
        string description,
        BattleUpgradeKind kind,
        ChaProperty propertyBonus,
        string targetSkillId,
        int weight,
        bool repeatable)
    {
        this.id = id;
        this.displayName = displayName;
        this.description = description;
        this.kind = kind;
        this.propertyBonus = propertyBonus;
        this.targetSkillId = targetSkillId;
        this.weight = weight;
        this.repeatable = repeatable;
    }

    public bool IsStructurallyValid
    {
        // 权重为 0 表示“本局暂时禁用”，仍是一条有效配置，以便覆盖内置强化。
        get { return !string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(displayName); }
    }
}

/// <summary>一次升级面板展示的候选快照，选择后不可再次选择。</summary>
public sealed class BattleUpgradeCandidate
{
    public readonly string id;
    public readonly string displayName;
    public readonly string description;
    public readonly BattleUpgradeKind kind;
    public readonly ChaProperty propertyBonus;
    public readonly string targetSkillId;
    public readonly int weight;
    public readonly bool repeatable;

    public BattleUpgradeCandidate(BattleUpgradeDefinition definition, string resolvedId, string resolvedName, string resolvedDescription, string resolvedSkillId)
    {
        id = resolvedId;
        displayName = resolvedName;
        description = resolvedDescription;
        kind = definition.kind;
        propertyBonus = definition.propertyBonus;
        targetSkillId = resolvedSkillId;
        weight = definition.weight;
        repeatable = definition.repeatable;
    }
}

/// <summary>候选选择状态。纯逻辑实现便于验证“不重复、只能选一次”。</summary>
public sealed class BattleUpgradeSelection
{
    private readonly List<BattleUpgradeCandidate> candidates;
    private bool selected;
    private int selectedIndex = -1;

    public IReadOnlyList<BattleUpgradeCandidate> Candidates { get { return candidates; } }
    public bool IsSelected { get { return selected; } }
    public int SelectedIndex { get { return selectedIndex; } }

    public BattleUpgradeSelection(List<BattleUpgradeCandidate> values)
    {
        candidates = values ?? new List<BattleUpgradeCandidate>();
    }

    public bool TrySelect(int index, out BattleUpgradeCandidate candidate)
    {
        candidate = null;
        if (selected || index < 0 || index >= candidates.Count) return false;
        selected = true;
        selectedIndex = index;
        candidate = candidates[index];
        return candidate != null;
    }
}

/// <summary>
/// 局内强化目录。内置定义保证旧场景无需补资产即可运行；GameDesignConfig 可覆盖同 ID 或追加新定义。
/// </summary>
public static class BattleUpgradeCatalog
{
    private static readonly BattleUpgradeDefinition[] builtInDefinitions = new[]
    {
        new BattleUpgradeDefinition(
            "attack_plus_10", "强袭训练", "攻击力 +10", BattleUpgradeKind.Property,
            new ChaProperty(0, 0, 0, 10), "", 4, false),
        new BattleUpgradeDefinition(
            "move_speed_plus_10", "疾行训练", "移动属性 +10", BattleUpgradeKind.Property,
            new ChaProperty(10, 0, 0, 0), "", 3, false),
        new BattleUpgradeDefinition(
            "max_hp_plus_100", "生命强化", "最大生命 +100，并补充等量当前生命", BattleUpgradeKind.Property,
            new ChaProperty(0, 100, 0, 0), "", 3, false),
        new BattleUpgradeDefinition(
            "skill_level_up", "技能精进", "提升一个已拥有技能的等级", BattleUpgradeKind.SkillLevel,
            ChaProperty.zero, "", 3, true),
        new BattleUpgradeDefinition(
            "new_skill_fireMultiple", "散射解锁", "学习英雄技能契约中的散射技能", BattleUpgradeKind.NewSkill,
            ChaProperty.zero, "fireMultiple", 1, false)
    };

    public static BattleUpgradeDefinition[] GetDefinitions(GameDesignConfig config)
    {
        List<BattleUpgradeDefinition> definitions = new List<BattleUpgradeDefinition>();
        HashSet<string> ids = new HashSet<string>();
        for (int i = 0; i < builtInDefinitions.Length; i++)
        {
            BattleUpgradeDefinition configured = FindConfigured(config, builtInDefinitions[i].id);
            BattleUpgradeDefinition effective = configured != null && configured.IsStructurallyValid
                ? configured : builtInDefinitions[i];
            if (ids.Add(effective.id)) definitions.Add(effective);
        }

        if (config != null && config.battleUpgradeDefinitions != null)
        {
            for (int i = 0; i < config.battleUpgradeDefinitions.Length; i++)
            {
                BattleUpgradeDefinition definition = config.battleUpgradeDefinitions[i];
                if (definition != null && definition.IsStructurallyValid && ids.Add(definition.id)) definitions.Add(definition);
            }
        }
        return definitions.ToArray();
    }

    /// <summary>使用固定种子抽取候选，测试和重放不依赖 UnityEngine.Random 的全局状态。</summary>
    public static BattleUpgradeSelection CreateSelection(
        GameDesignConfig config,
        ChaState player,
        HeroDefinition hero,
        int choiceCount,
        int randomSeed,
        ISet<string> appliedIds = null)
    {
        List<BattleUpgradeCandidate> pool = ExpandEligible(config, player, hero, appliedIds);
        List<BattleUpgradeCandidate> selected = new List<BattleUpgradeCandidate>();
        System.Random random = new System.Random(randomSeed);
        int count = Mathf.Min(Mathf.Max(0, choiceCount), pool.Count);
        while (selected.Count < count && pool.Count > 0)
        {
            int totalWeight = 0;
            for (int i = 0; i < pool.Count; i++) totalWeight += Mathf.Max(0, pool[i].weight);
            if (totalWeight <= 0) break;
            int roll = random.Next(totalWeight);
            int index = 0;
            for (; index < pool.Count - 1; index++)
            {
                int weight = Mathf.Max(0, pool[index].weight);
                if (roll < weight) break;
                roll -= weight;
            }
            selected.Add(pool[index]);
            pool.RemoveAt(index);
        }
        return new BattleUpgradeSelection(selected);
    }

    private static List<BattleUpgradeCandidate> ExpandEligible(GameDesignConfig config, ChaState player, HeroDefinition hero, ISet<string> appliedIds)
    {
        List<BattleUpgradeCandidate> pool = new List<BattleUpgradeCandidate>();
        BattleUpgradeDefinition[] definitions = GetDefinitions(config);
        for (int i = 0; i < definitions.Length; i++)
        {
            BattleUpgradeDefinition definition = definitions[i];
            if (definition == null || !definition.IsStructurallyValid || definition.weight <= 0) continue;
            if (!definition.repeatable && appliedIds != null && appliedIds.Contains(definition.id)) continue;

            if (definition.kind == BattleUpgradeKind.SkillLevel && string.IsNullOrEmpty(definition.targetSkillId))
            {
                if (player == null || player.skills == null) continue;
                for (int j = 0; j < player.skills.Count; j++)
                {
                    SkillObj skill = player.skills[j];
                    if (skill == null || skill.model.id == null) continue;
                    string candidateId = definition.id + ":" + skill.model.id;
                    pool.Add(new BattleUpgradeCandidate(
                        definition,
                        candidateId,
                        "技能精进：" + GetSkillDisplayName(skill.model.id),
                        "提升「" + GetSkillDisplayName(skill.model.id) + "」等级",
                        skill.model.id));
                }
                continue;
            }

            if (definition.kind == BattleUpgradeKind.SkillLevel)
            {
                if (player == null || player.GetSkillById(definition.targetSkillId) == null) continue;
            }
            if (definition.kind == BattleUpgradeKind.NewSkill)
            {
                if (player == null || player.HasSkill(definition.targetSkillId) || !HeroAllowsSkill(hero, definition.targetSkillId)) continue;
                SkillModel unused;
                if (!GameDatabase.TryGetSkill(definition.targetSkillId, out unused)) continue;
            }
            pool.Add(new BattleUpgradeCandidate(definition, definition.id, definition.displayName, definition.description, definition.targetSkillId));
        }
        return pool;
    }

    private static bool HeroAllowsSkill(HeroDefinition hero, string skillId)
    {
        if (hero == null || hero.skillIds == null || string.IsNullOrEmpty(skillId)) return false;
        for (int i = 0; i < hero.skillIds.Length; i++) if (hero.skillIds[i] == skillId) return true;
        return false;
    }

    private static BattleUpgradeDefinition FindConfigured(GameDesignConfig config, string id)
    {
        if (config == null || config.battleUpgradeDefinitions == null) return null;
        for (int i = 0; i < config.battleUpgradeDefinitions.Length; i++)
        {
            BattleUpgradeDefinition definition = config.battleUpgradeDefinitions[i];
            if (definition != null && definition.id == id) return definition;
        }
        return null;
    }

    private static string GetSkillDisplayName(string skillId)
    {
        if (skillId == "fire") return "单发射击";
        if (skillId == "fireMultiple") return "散射射击";
        if (skillId == "poisonShot") return "毒性射击";
        if (skillId == "shadowVeil") return "暗影隐身";
        if (skillId == "spellLightning") return "闪电术";
        if (skillId == "spellFreeze") return "冰冻术";
        if (skillId == "spellPolymorph") return "变羊术";
        if (skillId == "haste") return "迅捷术";
        return string.IsNullOrEmpty(skillId) ? "未知技能" : skillId;
    }
}

/// <summary>暂停来源状态。多个系统不会互相误恢复，终局可以使用 ForceResume 兜底。</summary>
public sealed class BattlePauseState
{
    private string sourceId;
    public bool IsPaused { get { return !string.IsNullOrEmpty(sourceId); } }
    public string SourceId { get { return sourceId; } }

    public bool TryPause(string requestSource)
    {
        if (string.IsNullOrEmpty(requestSource) || IsPaused) return false;
        sourceId = requestSource;
        return true;
    }

    public bool TryResume(string requestSource)
    {
        if (!IsPaused || requestSource != sourceId) return false;
        sourceId = null;
        return true;
    }

    public void ForceResume() { sourceId = null; }
}

/// <summary>战斗暂停的 Unity 桥接。记录并恢复原始时间缩放，升级选择 UI 在 timeScale=0 时仍可点击。</summary>
[DisallowMultipleComponent]
public class BattlePauseService : MonoBehaviour
{
    private readonly BattlePauseState state = new BattlePauseState();
    private float previousTimeScale = 1f;
    private bool changedTimeScale;

    public BattlePauseState State { get { return state; } }

    public bool TryPause(string sourceId)
    {
        if (!state.TryPause(sourceId)) return false;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        changedTimeScale = true;
        return true;
    }

    public bool TryResume(string sourceId)
    {
        if (!state.TryResume(sourceId)) return false;
        RestoreTimeScale();
        return true;
    }

    public void ForceResume()
    {
        state.ForceResume();
        RestoreTimeScale();
    }

    private void RestoreTimeScale()
    {
        if (!changedTimeScale) return;
        Time.timeScale = previousTimeScale;
        changedTimeScale = false;
    }

    private void OnDisable() { ForceResume(); }
}

/// <summary>
/// 升级服务：接收经验升级事件、暂停战斗、生成候选并把选中的强化应用到主角。
/// </summary>
[DisallowMultipleComponent]
public class BattleUpgradeService : MonoBehaviour
{
    private const string PauseSourceId = "battle_upgrade_choice";
    private GameDesignConfig config;
    private GameObject playerActor;
    private HeroDefinition hero;
    private BattleUpgradeSelection selection;
    private readonly HashSet<string> appliedIds = new HashSet<string>();
    private bool initialized;
    private bool subscribed;

    public BattleUpgradeSelection CurrentSelection { get { return selection; } }
    public bool IsAwaitingChoice { get { return selection != null && !selection.IsSelected; } }
    public event Action<BattleUpgradeSelection> UpgradeOffered;
    public event Action<BattleUpgradeCandidate> UpgradeApplied;
    public event Action UpgradeClosed;

    private void OnEnable() { SubscribeRewardEvent(); }
    private void OnDisable()
    {
        UnsubscribeRewardEvent();
        CloseSelection();
    }

    public void ConfigureForBattle(GameDesignConfig gameConfig, GameObject mainActor, HeroDefinition selectedHero)
    {
        SubscribeRewardEvent();
        config = gameConfig != null ? gameConfig : GameRuntime.Config;
        playerActor = mainActor;
        hero = selectedHero;
        initialized = true;
        appliedIds.Clear();
        selection = null;
    }

    public bool TryChooseUpgrade(int index)
    {
        if (!IsAwaitingChoice || !IsBattleInProgress()) return false;
        BattleUpgradeCandidate candidate;
        if (!selection.TrySelect(index, out candidate) || candidate == null) return false;
        ChaState player = playerActor != null ? playerActor.GetComponent<ChaState>() : GameRuntime.MainActorState;
        if (!ApplyCandidate(player, candidate))
        {
            Debug.LogWarning("[BattleUpgradeService] Upgrade application failed: " + candidate.id);
            // 候选应用异常时不能留下“已锁定但无法选择”的暂停死锁，安全关闭本次选择。
            CloseSelection();
            return false;
        }
        if (!candidate.repeatable) appliedIds.Add(candidate.id.Split(':')[0]);
        Action<BattleUpgradeCandidate> applied = UpgradeApplied;
        if (applied != null) applied(candidate);
        CloseSelection();
        return true;
    }

    public void CloseSelection()
    {
        if (selection == null) return;
        selection = null;
        BattlePauseService pause = GameRuntime.BattlePause;
        if (pause != null) pause.TryResume(PauseSourceId);
        Action callbacks = UpgradeClosed;
        if (callbacks != null) callbacks();
    }

    private void OnRewardGranted(BattleRewardGrant grant)
    {
        if (!initialized || config == null || !config.enableBattleUpgrades || !grant.leveledUp || !IsBattleInProgress() || IsAwaitingChoice) return;
        ChaState player = playerActor != null ? playerActor.GetComponent<ChaState>() : GameRuntime.MainActorState;
        if (player == null) return;
        int seed = grant.totalKills * 7919 + grant.levelAfter * 104729;
        BattleUpgradeSelection next = BattleUpgradeCatalog.CreateSelection(
            config, player, hero, config != null ? config.upgradeChoiceCount : 3, seed, appliedIds);
        if (next.Candidates.Count == 0) return;
        BattlePauseService pause = GameRuntime.BattlePause;
        if (pause == null || !pause.TryPause(PauseSourceId)) return;
        selection = next;
        Action<BattleUpgradeSelection> offered = UpgradeOffered;
        if (offered != null) offered(selection);
    }

    private bool ApplyCandidate(ChaState player, BattleUpgradeCandidate candidate)
    {
        if (player == null || candidate == null) return false;
        if (candidate.kind == BattleUpgradeKind.Property)
        {
            player.ApplyPermanentPropertyBonus(candidate.propertyBonus);
            return true;
        }
        if (candidate.kind == BattleUpgradeKind.SkillLevel)
        {
            SkillObj current = player.GetSkillById(candidate.targetSkillId);
            return current != null && player.LearnSkill(current.model, current.level + 1);
        }
        if (candidate.kind == BattleUpgradeKind.NewSkill)
        {
            if (!HeroAllowsSkill(candidate.targetSkillId)) return false;
            SkillModel skill;
            return GameDatabase.TryGetSkill(candidate.targetSkillId, out skill) && player.LearnSkill(skill);
        }
        return false;
    }

    private bool HeroAllowsSkill(string skillId)
    {
        if (hero == null || hero.skillIds == null) return false;
        for (int i = 0; i < hero.skillIds.Length; i++) if (hero.skillIds[i] == skillId) return true;
        return false;
    }

    private bool IsBattleInProgress()
    {
        GameManager manager = GameRuntime.Manager;
        return manager == null || manager.IsBattleInProgress;
    }

    private void SubscribeRewardEvent()
    {
        if (subscribed) return;
        BattleRewardService rewards = GameRuntime.BattleRewards;
        if (rewards == null) return;
        rewards.RewardGranted += OnRewardGranted;
        subscribed = true;
    }

    private void UnsubscribeRewardEvent()
    {
        if (!subscribed) return;
        BattleRewardService rewards = GameRuntime.BattleRewards;
        if (rewards != null) rewards.RewardGranted -= OnRewardGranted;
        subscribed = false;
    }
}
