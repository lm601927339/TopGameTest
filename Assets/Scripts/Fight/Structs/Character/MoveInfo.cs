using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 旧版强制移动预约，保留该类型是为了兼容已有技能脚本。
/// </summary>
public class MovePreorder{
    /// <summary>想要移动的方向和距离。</summary>
    public Vector3 velocity;

    /// <summary>创建时的总时长。</summary>
    private float inTime;

    /// <summary>剩余移动时长，单位：秒。</summary>
    public float duration;

    public MovePreorder(Vector3 velocity, float duration){
        this.velocity = velocity;
        this.duration = duration;
        this.inTime = duration;
    }

    /// <summary>
    /// 消耗一段时间并返回这一帧的速度。
    /// </summary>
    public Vector3 VeloInTime(float time){
        if (time >= duration){
            this.duration = 0;
        }else{
            this.duration -= time;
        }
        return inTime <= 0 ? velocity : (velocity / inTime);
    }
}

/// <summary>
/// 运动影响的叠加方式。覆盖会替换自主移动，叠加会在同优先级下相加。
/// </summary>
public enum MotionInfluenceBlendMode{
    Additive = 0,
    Override = 1
}

/// <summary>
/// 阶段二统一运动影响数据：龙卷风、磁力、击退、牵引等都使用这一结构。
/// </summary>
public class MotionInfluence{
    /// <summary>来源标识，用于移除某个技能或环境效果。</summary>
    public string sourceId;
    /// <summary>影响速度，单位：米/秒。</summary>
    public Vector3 velocity;
    /// <summary>剩余时长，单位：秒。</summary>
    public float duration;
    /// <summary>优先级越高越先结算，低优先级会被高优先级覆盖。</summary>
    public int priority;
    /// <summary>覆盖或叠加自主移动。</summary>
    public MotionInfluenceBlendMode blendMode;
    /// <summary>角色不能主动移动时是否仍然生效。</summary>
    public bool ignoreControlState;

    public bool IsExpired{
        get { return duration <= 0f; }
    }

    public MotionInfluence(
        string sourceId,
        Vector3 velocity,
        float duration,
        int priority = 0,
        MotionInfluenceBlendMode blendMode = MotionInfluenceBlendMode.Additive,
        bool ignoreControlState = false
    ){
        this.sourceId = sourceId;
        this.velocity = velocity;
        this.duration = Mathf.Max(0f, duration);
        this.priority = priority;
        this.blendMode = blendMode;
        this.ignoreControlState = ignoreControlState;
    }

    /// <summary>便于技能脚本按速度、时长直接创建运动影响。</summary>
    public MotionInfluence(
        Vector3 velocity,
        float duration,
        int priority = 0,
        MotionInfluenceBlendMode blendMode = MotionInfluenceBlendMode.Additive,
        bool ignoreControlState = false,
        string sourceId = ""
    ) : this(sourceId, velocity, duration, priority, blendMode, ignoreControlState){
    }
}

/// <summary>
/// 运动影响结算器。它不依赖 MonoBehaviour，方便编辑器和单元测试验证规则。
/// </summary>
public static class MotionResolver{
    /// <summary>
    /// 结算自主移动与外部运动影响，并在结算后消耗影响时长。
    /// </summary>
    public static Vector3 Resolve(
        Vector3 autonomousVelocity,
        IList<MotionInfluence> influences,
        float deltaTime,
        bool canMove
    ){
        if (influences == null || influences.Count == 0){
            return canMove ? autonomousVelocity : Vector3.zero;
        }

        int highestPriority = int.MinValue;
        for (int i = 0; i < influences.Count; i++){
            MotionInfluence influence = influences[i];
            if (influence == null || influence.IsExpired) continue;
            if (!canMove && !influence.ignoreControlState) continue;
            highestPriority = Mathf.Max(highestPriority, influence.priority);
        }

        Vector3 result = canMove ? autonomousVelocity : Vector3.zero;
        bool hasOverride = false;
        Vector3 overrideVelocity = Vector3.zero;
        Vector3 additiveVelocity = Vector3.zero;

        if (highestPriority != int.MinValue){
            for (int i = 0; i < influences.Count; i++){
                MotionInfluence influence = influences[i];
                if (influence == null || influence.IsExpired || influence.priority != highestPriority) continue;
                if (!canMove && !influence.ignoreControlState) continue;
                if (influence.blendMode == MotionInfluenceBlendMode.Override){
                    hasOverride = true;
                    overrideVelocity += influence.velocity;
                }else{
                    additiveVelocity += influence.velocity;
                }
            }
            result = (hasOverride ? overrideVelocity : result) + additiveVelocity;
        }

        float safeDeltaTime = Mathf.Max(0f, deltaTime);
        for (int i = influences.Count - 1; i >= 0; i--){
            MotionInfluence influence = influences[i];
            if (influence == null){
                influences.RemoveAt(i);
                continue;
            }
            influence.duration -= safeDeltaTime;
            if (influence.IsExpired) influences.RemoveAt(i);
        }
        return result;
    }
}

/// <summary>单位的移动模式。</summary>
public enum MoveType{
    ground = 0,
    fly = 1,
    // 钻地单位使用地下通行规则。
    underground = 2,
    // 壁虎等单位使用墙面通行规则。
    wallClimbing = 3
}