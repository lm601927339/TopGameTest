using UnityEngine;

/// <summary>阶段五能力效果类型，技能脚本只提交请求，不直接改写角色底层数据。</summary>
public enum AbilityEffectKind{
    Invisible = 0,
    Invincible = 1,
    Disguise = 2,
    ControlMonster = 3,
    Clone = 4,
    Decoy = 5,
    Lightning = 6
}

/// <summary>能力效果的通用配置，方便时间轴、技能表和调试按钮共用。</summary>
[System.Serializable]
public class AbilityEffectRequest{
    public AbilityEffectKind effectKind = AbilityEffectKind.Invisible;
    public string sourceId = "skill";
    public float duration = 3f;
    public int priority = 20;
    public int sideOverride = int.MinValue;
    public float radius = 5f;
    public int damage = 10;
    public GameObject prefab;
}

/// <summary>
/// 阶段五能力服务。所有能力都经过状态、控制权或伤害服务，便于后续接入技能表。
/// </summary>
public static class AbilityEffectService{
    /// <summary>施加隐身状态，目标服务会自动过滤隐身目标。</summary>
    public static bool ApplyInvisible(ChaState target, string sourceId, float duration, int priority = 20){
        return ApplyState(target, ChaAbilityStateType.Invisible, sourceId, duration, priority);
    }

    /// <summary>施加无敌状态，伤害仍然走 DamageManager，只在状态判定处拦截。</summary>
    public static bool ApplyInvincible(ChaState target, string sourceId, float duration, int priority = 20){
        return ApplyState(target, ChaAbilityStateType.Invincible, sourceId, duration, priority);
    }

    /// <summary>将中毒语义同步到能力状态；持续伤害和减速仍由原有 Buff 链路负责。</summary>
    public static bool ApplyPoisoned(ChaState target, string sourceId, float duration, int priority = 20){
        float resolvedDuration;
        return TryApplyPoisoned(target, sourceId, duration, out resolvedDuration, priority);
    }

    /// <summary>
    /// 施加中毒并返回抗性结算后的实际持续时间。
    /// 毒性 DOT/减速 Buff 必须使用该返回值，避免状态图标结束后 Buff 仍继续生效。
    /// </summary>
    public static bool TryApplyPoisoned(ChaState target, string sourceId, float duration, out float resolvedDuration, int priority = 20){
        resolvedDuration = 0f;
        if (target == null || target.dead || string.IsNullOrEmpty(sourceId)) return false;

        ChaAbilityState state = target.AddState(ChaAbilityStateType.Poisoned, sourceId, duration, priority);
        if (state == null) return false;
        resolvedDuration = state.duration;
        return true;
    }

    /// <summary>施加伪装状态；阵营关系读取伪装后的有效阵营。</summary>
    public static bool ApplyDisguise(ChaState target, string sourceId, int disguiseSide, float duration, int priority = 20){
        if (target == null || target.dead || disguiseSide == int.MinValue) return false;
        target.AddState(ChaAbilityStateType.Disguised, sourceId, duration, priority, disguiseSide);
        return true;
    }

    /// <summary>移除指定来源的单个能力效果。</summary>
    public static bool RemoveState(ChaState target, ChaAbilityStateType stateType, string sourceId){
        return target != null && target.RemoveState(stateType, sourceId) > 0;
    }

    /// <summary>远程控制怪物；控制权结束后目标自动回到原 AI。</summary>
    public static bool ControlMonster(ChaState controller, ChaState target, string sourceId, float duration, int priority = 100){
        if (controller == null || target == null || controller == target || target.dead) return false;
        return target.AcquireControl(ControlAuthorityKind.Puppet, sourceId, priority, duration, controller.EffectiveSide);
    }

    /// <summary>创建一个有时限的分身或诱饵，销毁由 AbilityLifetime 统一管理。</summary>
    public static GameObject SpawnClone(ChaState source, GameObject prefab, Vector3 offset, float duration, bool decoy = false, string sourceId = "clone"){
        if (source == null || source.dead || prefab == null) return null;
        GameObject clone = Object.Instantiate(prefab, source.transform.position + offset, source.transform.rotation);
        ChaState cloneState = clone.GetComponent<ChaState>();
        if (cloneState != null){
            cloneState.side = source.OriginalSide;
            if (decoy) cloneState.AddState(ChaAbilityStateType.Disguised, sourceId, duration, 10, source.EffectiveSide);
        }
        AbilityLifetime lifetime = clone.GetComponent<AbilityLifetime>();
        if (lifetime == null) lifetime = clone.AddComponent<AbilityLifetime>();
        lifetime.duration = duration;
        lifetime.owner = source;
        lifetime.sourceId = sourceId;
        return clone;
    }

    /// <summary>召唤闪电锁定最近敌人，伤害和眩晕都走统一状态/伤害链路。</summary>
    public static ChaState SummonLightning(ChaState caster, float radius, int damage, float stunDuration, string sourceId = "lightning"){
        if (caster == null || caster.dead) return null;
        ChaState target = TargetService.FindNearestEnemy(caster, radius);
        if (target == null) return null;
        SceneVariants.CreateDamage(caster.gameObject, target.gameObject, new Damage(damage), 0f, 0f, new DamageInfoTag[]{DamageInfoTag.directDamage});
        target.AddState(ChaAbilityStateType.Stunned, sourceId, stunDuration, 30);
        return target;
    }

    private static bool ApplyState(ChaState target, ChaAbilityStateType stateType, string sourceId, float duration, int priority){
        if (target == null || target.dead || string.IsNullOrEmpty(sourceId)) return false;
        return target.AddState(stateType, sourceId, duration, priority) != null;
    }
}

/// <summary>分身、诱饵等临时对象的生命周期组件。</summary>
public class AbilityLifetime:MonoBehaviour{
    public float duration = -1f;
    public string sourceId = "ability";
    public ChaState owner;

    private void FixedUpdate(){
        if (duration < 0f) return;
        duration -= Time.fixedDeltaTime;
        if (duration <= 0f) Destroy(gameObject);
    }
}
