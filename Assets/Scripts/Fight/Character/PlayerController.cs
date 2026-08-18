using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///玩家操作的控制器。
///</summary>
public class PlayerController : MonoBehaviour
{
    public Camera mainCamera;
    public SkillCastFailureReason lastSkillCastFailureReason = SkillCastFailureReason.None;

    private ChaState chaState;

    void Start()
    {
        chaState = this.gameObject.GetComponent<ChaState>();
    }

    void FixedUpdate()
    {
        if (!chaState || chaState.dead == true) return;
        if (!mainCamera) mainCamera = GameRuntime.MainCamera;

        float ix = Input.GetAxis("Horizontal");
        float iy = Input.GetAxis("Vertical");

        Vector2 cursorPos = Input.mousePosition;

        float rotateTo = transform.rotation.eulerAngles.z;
        if (mainCamera)
        {
            Vector2 mScreenPos = RectTransformUtility.WorldToScreenPoint(mainCamera, transform.position);
            rotateTo = Mathf.Atan2(cursorPos.y - mScreenPos.y, cursorPos.x - mScreenPos.x) * 180.00f / Mathf.PI;
            chaState.OrderRotateTo(rotateTo);
        }

        if (ix != 0 || iy != 0)
        {
            float mSpd = chaState.moveSpeed;
            Vector3 mInfo = new Vector3(ix * mSpd, iy * mSpd, 0);
            chaState.OrderMove(mInfo);
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
                Debug.Log("skill === " + bindings[i].skillId);
                SkillCastResult castResult = chaState.TryCastSkill(bindings[i].skillId);
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
        chaState.charging = btnHolding;
    }
}
