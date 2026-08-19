using System;
using UnityEngine;

/// <summary>单个异常状态的持续时间抗性。0 表示不减免，1 表示完全免疫该状态。</summary>
[Serializable]
public struct AbilityStateResistance
{
    public ChaAbilityStateType stateType;
    [Range(0f, 1f)] public float durationReduction;

    public AbilityStateResistance(ChaAbilityStateType stateType, float durationReduction)
    {
        this.stateType = stateType;
        this.durationReduction = Mathf.Clamp01(durationReduction);
    }
}

/// <summary>
/// 可挂在任意角色上的状态抗性配置。ChaState 在新增或刷新有限时状态时统一查询它，避免每个技能重复计算抗性。
/// </summary>
[DisallowMultipleComponent]
public class AbilityStateResistanceProfile : MonoBehaviour
{
    [SerializeField] private AbilityStateResistance[] resistances = new AbilityStateResistance[0];

    /// <summary>英雄初始化或预制体配置时写入抗性快照，防止外部数组随后被意外修改。</summary>
    public void Configure(AbilityStateResistance[] values)
    {
        if (values == null || values.Length == 0)
        {
            resistances = new AbilityStateResistance[0];
            return;
        }

        resistances = new AbilityStateResistance[values.Length];
        Array.Copy(values, resistances, values.Length);
    }

    /// <summary>按状态类型修正有限持续时间；返回 false 表示该状态被完全抵抗。</summary>
    public bool TryAdjustDuration(ChaAbilityStateType stateType, ref float duration)
    {
        if (duration < 0f || resistances == null) return true;
        float strongestReduction = 0f;
        for (int i = 0; i < resistances.Length; i++)
        {
            AbilityStateResistance resistance = resistances[i];
            if (resistance.stateType == stateType) strongestReduction = Mathf.Max(strongestReduction, resistance.durationReduction);
        }

        if (strongestReduction <= 0f) return true;
        if (strongestReduction >= 1f) return false;
        duration *= 1f - strongestReduction;
        return duration > 0f;
    }

    /// <summary>供角色面板和调试工具显示当前角色对指定状态的减免比例。</summary>
    public float GetDurationReduction(ChaAbilityStateType stateType)
    {
        float strongestReduction = 0f;
        if (resistances == null) return strongestReduction;
        for (int i = 0; i < resistances.Length; i++)
        {
            if (resistances[i].stateType == stateType) strongestReduction = Mathf.Max(strongestReduction, resistances[i].durationReduction);
        }
        return strongestReduction;
    }
}
