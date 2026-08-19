using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家输入控制器。阶段三允许把输入目标切换为被控傀儡，角色本身仍由 ChaState 执行移动。
/// </summary>
public class PlayerController : MonoBehaviour
{
    public Camera mainCamera;
    public SkillCastFailureReason lastSkillCastFailureReason = SkillCastFailureReason.None;

    /// <summary>当前被玩家接管的角色；为空时控制挂载本组件的角色。</summary>
    public ChaState controlledTarget;

    private ChaState chaState;
    private string controlSourceId;

    private ChaState CommandTarget
    {
        get
        {
            if (controlledTarget != null && controlledTarget.dead == false)
            {
                // 控制权可能在目标自己的 FixedUpdate 中自动过期，也可能被更高优先级来源覆盖。
                ControlAuthorityState authority = controlledTarget.CurrentControlAuthority;
                if (authority != null && authority.IsExpired == false && authority.sourceId == controlSourceId)
                {
                    return controlledTarget;
                }
                ClearControlledTarget();
            }
            return chaState;
        }
    }

    void Start()
    {
        chaState = this.gameObject.GetComponent<ChaState>();
        controlSourceId = "player-control-" + GetInstanceID();
    }

    /// <summary>
    /// 开始远程控制目标。目标原始阵营不会被改写，只在目标关系结算时使用玩家有效阵营。
    /// </summary>
    public bool SetControlledTarget(ChaState target, float duration = -1f)
    {
        if (target == null || target == chaState || target.dead) return false;
        ClearControlledTarget();
        int controllingSide = chaState != null ? chaState.EffectiveSide : int.MinValue;
        if (!target.AcquireControl(ControlAuthorityKind.Player, controlSourceId, 100, duration, controllingSide)) return false;
        controlledTarget = target;
        return true;
    }

    /// <summary>结束远程控制并释放目标的控制权。</summary>
    public void ClearControlledTarget()
    {
        if (controlledTarget != null) controlledTarget.ReleaseControl(controlSourceId);
        controlledTarget = null;
    }

    void FixedUpdate()
    {
        ChaState commandTarget = CommandTarget;
        if (!commandTarget || commandTarget.dead == true) return;
        // 被别的控制源接管时，本组件不再覆盖它的移动意图。
        if (controlledTarget == null && commandTarget.IsControlled) return;
        if (!mainCamera) mainCamera = GameRuntime.MainCamera;

        float ix = Input.GetAxis("Horizontal");
        float iy = Input.GetAxis("Vertical");
        Vector2 cursorPos = Input.mousePosition;

        float rotateTo = commandTarget.transform.rotation.eulerAngles.z;
        if (mainCamera)
        {
            Vector2 mScreenPos = RectTransformUtility.WorldToScreenPoint(mainCamera, commandTarget.transform.position);
            rotateTo = Mathf.Atan2(cursorPos.y - mScreenPos.y, cursorPos.x - mScreenPos.x) * 180.00f / Mathf.PI;
            commandTarget.OrderRotateTo(rotateTo);
        }

        if (ix != 0 || iy != 0)
        {
            float mSpd = commandTarget.moveSpeed;
            commandTarget.OrderMove(new Vector3(ix * mSpd, iy * mSpd, 0));
        }

        bool btnHolding = false;
        SkillBinding[] bindings = GameRuntime.Config.playerSkillBindings;
        if (bindings == null || bindings.Length <= 0)
        {
            bindings = new SkillBinding[]
            {
                new SkillBinding("Fire5", "homingMissle"),
                new SkillBinding("Fire4", "teleportBullet"),
                new SkillBinding("Fire3", "grenade"),
                new SkillBinding("Fire2", "fireMultiple"),
                new SkillBinding("Fire1", "cloakBoomerang"),
                new SkillBinding("Jump", "roll"),
            };
        }

        for (int i = 0; i < bindings.Length; i++)
        {
            if (Input.GetButton(bindings[i].buttonName) == true)
            {
                SkillCastResult castResult = commandTarget.TryCastSkill(bindings[i].skillId);
                lastSkillCastFailureReason = castResult.success
                    ? SkillCastFailureReason.None
                    : castResult.failureReason;
                if (castResult.success == false)
                {
                    Debug.LogWarning(SkillCastFeedbackUtility.GetMessage(castResult.failureReason));
                }
                btnHolding = true;
            }
        }
        commandTarget.charging = btnHolding;
    }
}
