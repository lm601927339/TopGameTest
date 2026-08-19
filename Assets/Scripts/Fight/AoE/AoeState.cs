using System.Collections.Generic;
using UnityEngine;


public enum AoeType { 
    Circle  = 0,
    Rect    = 1,
}

///<summary>
///AoE的状态控制器
///</summary>
public class AoeState : MonoBehaviour{
    ///<summary>
    ///要释放的aoe
    ///</summary>
    public AoeModel model;

    ///<summary>
    ///是否被视作刚创建
    ///</summary>
    public bool justCreated = true;

    ///<summary>
    ///aoe的半径，单位：米
    ///目前这游戏的设计中，aoe只有圆形，所以只有一个半径，也不存在角度一说，如果需要可以扩展
    ///</summary>
    public float radius;

    ///<summary>
    ///aoe的长宽，单位：米 （扩展）
    ///</summary>
    public Vector2 sizePos;
    ///<summary>
    ///aoe的施法者
    ///</summary>
    public GameObject caster;

    ///<summary>
    ///aoe存在的时间，单位：秒
    ///</summary>
    public float duration;

    ///<summary>
    ///aoe已经存在过的时间，单位：秒
    ///</summary>
    public float timeElapsed = 0;

    ///<summary>
    ///aoe移动轨迹函数
    ///</summary>
    public AoeTween tween;

    ///<summary>
    ///aoe的轨迹运行了多少时间了，单位：秒
    ///<summary>
    public float tweenRunnedTime = 0;

    ///<summary>
    ///创建时角色的属性
    ///</summary>
    public ChaProperty propWhileCreate;

    ///<summary>
    ///aoe的传入参数，比如可以吸收次数之类的
    ///</summary>
    public Dictionary<string, object> param = new Dictionary<string, object>();

    ///<summary>
    ///现在aoe范围内的所有角色的gameobject
    ///</summary>
    public List<GameObject> characterInRange = new List<GameObject>();

    ///<summary>
    ///现在aoe范围内的所有子弹的gameobject
    ///</summary>
    public List<GameObject> bulletInRange = new List<GameObject>();

    ///<summary>
    ///Tween函数的参数
    ///</summary>
    public object[] tweenParam;

    ///<summary>
    ///移动信息
    ///</summary>
    public Vector2 velocity{
        get{ return this._velo;}
    }
    private Vector2 _velo = new Vector2();

    private UnitMove unitMove;
    private UnitRotate unitRotate;
    private GameObject viewContainer;

    private void Start() {
        // this.unitMove = this.gameObject.GetComponent<UnitMove>();
        // this.unitRotate = this.gameObject.GetComponent<UnitRotate>();
        synchronizedUnits();
    }

    ///<summary>
    ///设置移动和旋转的信息，用于执行
    ///</summary>
    public void SetMoveAndRotate(AoeMoveInfo aoeMoveInfo){
        if (aoeMoveInfo != null){
            if (unitMove){
                unitMove.moveType = aoeMoveInfo.moveType;
                unitMove.bodyRadius = this.radius;
                _velo = aoeMoveInfo.velocity / Time.fixedDeltaTime;
                unitMove.MoveBy(_velo);
            }
            if (unitRotate){
                unitRotate.RotateTo(aoeMoveInfo.rotateToDegree);
            }
        }
    }

    public bool HitObstacle(){
        return unitMove == null ? false : unitMove.hitObstacle;
    }

    private void synchronizedUnits(){
        if (!unitMove) unitMove = this.gameObject.GetComponent<UnitMove>();
        if (!unitRotate) unitRotate = this.gameObject.GetComponent<UnitRotate>();
        if (!viewContainer) viewContainer = this.gameObject.GetComponentInChildren<ViewContainer>().gameObject;
        unitMove.bodyRadius = this.radius;
        unitMove.smoothMove = !model.removeOnObstacle;
    }

    public void InitByAoeLauncher(AoeLauncher aoe){
        this.model = aoe.model;
        this.radius = aoe.radius;
        this.duration = aoe.duration;
        this.timeElapsed = 0;
        this.tween = aoe.tween;
        this.tweenParam = aoe.tweenParam;
        this.tweenRunnedTime = 0;
        this.param = new Dictionary<string, object>();
        foreach (KeyValuePair<string, object> kv in aoe.param){
            this.param[kv.Key] = kv.Value;
        }//aoe.param;
        this.caster = aoe.caster;
        this.propWhileCreate = aoe.caster ? aoe.caster.GetComponent<ChaState>().property : ChaProperty.zero;
        
        this.transform.position = aoe.position;
        this.transform.eulerAngles.Set(0,  0,aoe.degree);

        synchronizedUnits();

        //把视觉特效给aoe
        if (aoe.model.prefab != ""){
            GameObject aoePrefab = GameRuntime.LoadPrefab(aoe.model.prefab);
            if (aoePrefab)
            {
                GameObject aoeEffect = Instantiate<GameObject>(
                    aoePrefab,
                    new Vector2(),
                    Quaternion.identity,
                    viewContainer.transform
                );
                
                aoeEffect.transform.localPosition = new Vector2(0, this.gameObject.transform.position.y);
                aoeEffect.transform.localRotation = Quaternion.identity;
            }
        }
        this.gameObject.transform.position = new Vector2(
            this.gameObject.transform.position.x,
            this.gameObject.transform.position.y
        );
    }

    ///<summary>
    ///改变aoe视觉的尺寸
    ///</summary>
    public void SetViewScale(float scaleX = 1, float scaleY = 1, float scaleZ = 1){
        synchronizedUnits();
        viewContainer.transform.localScale.Set(scaleX, scaleY, scaleZ);
    }

    ///<summary>
    ///改变图形的y高度
    ///</summary>
    public void ModViewY(float toY){
        this.viewContainer.transform.position = new Vector3(
            viewContainer.transform.position.x,
            toY,
            viewContainer.transform.position.z
        );
    }

    public void FixedUpdate()
    {
        float timePassed = Time.fixedDeltaTime;
        var aoeState = gameObject.GetComponent<AoeState>();

        //首先是aoe的移动
        if (aoeState.duration > 0 && aoeState.tween != null)
        {
            AoeMoveInfo aoeMoveInfo = aoeState.tween(gameObject, aoeState.tweenRunnedTime);
            aoeState.tweenRunnedTime += timePassed;
            aoeState.SetMoveAndRotate(aoeMoveInfo);
        }
        if (aoeState.justCreated == true)
        {
            //刚创建的，走onCreate
            aoeState.justCreated = false;
            //执行OnCreate
            if (aoeState.model.onCreate != null)
            {
                aoeState.model.onCreate(gameObject);
            }

        }
        else {
            //已经创建完成的onEnter onlevel 处理放在 碰撞里

            //然后是aoe的duration
            aoeState.duration -= timePassed;
            aoeState.timeElapsed += timePassed;

            if (aoeState.duration <= 0 || aoeState.HitObstacle() == true)
            {
                if (aoeState.model.onRemoved != null)
                {
                    aoeState.model.onRemoved(gameObject);
                }
                Destroy(gameObject);
            }
            else
            {
                //最后是onTick，remove
                if (
                    aoeState.model.tickTime > 0 && aoeState.model.onTick != null &&
                    Mathf.RoundToInt(aoeState.duration * 1000) % Mathf.RoundToInt(aoeState.model.tickTime * 1000) == 0
                )
                {
                    aoeState.model.onTick(gameObject);
                }
            }
        }
    }
}

/// <summary>阶段四环境体积类型，环境规则与视觉预制体解耦。</summary>
public enum EnvironmentVolumeKind{
    Cloud = 0,
    Tornado = 1,
    MagneticField = 2,
    ToxicGas = 3,
    Radiation = 4,
    FireOil = 5,
    IceWall = 6,
    Lightning = 7
}

/// <summary>
/// 通用环境体积。通常与 AoeState 挂在同一个对象上，由碰撞范围提供角色列表。
/// </summary>
public class EnvironmentVolume : MonoBehaviour{
    public EnvironmentVolumeKind volumeKind = EnvironmentVolumeKind.Cloud;
    public string sourceId = "environment";
    public float radius = 2f;
    public Vector2 direction = Vector2.zero;
    public float strength = 2f;
    public float maxSpeed = 8f;
    public float tickInterval = 0.1f;
    public float duration = -1f;
    public float statusDuration = 0.25f;
    public int priority = 10;
    public bool affectEnemies = true;
    public bool affectAllies = false;
    /// <summary>独立环境体积的施法者；为空时从 AoeState.caster 读取。</summary>
    public GameObject caster;
    /// <summary>毒气、辐射、火油和闪电的每次采样伤害。</summary>
    public int damagePerTick = 5;
    public bool applyDamage = true;

    private AoeState aoeState;
    private float timeElapsed;
    private float timeToTick;
    private readonly List<ChaState> affectedCharacters = new List<ChaState>();

    private void Start(){
        aoeState = GetComponent<AoeState>();
        if (caster == null && aoeState != null) caster = aoeState.caster;
        if (string.IsNullOrEmpty(sourceId)) sourceId = "environment-" + GetInstanceID();
        if (aoeState != null && aoeState.caster != null){
            ChaState casterState = aoeState.caster.GetComponent<ChaState>();
            if (casterState != null && radius <= 0f) radius = aoeState.radius;
        }
    }

    private void FixedUpdate(){
        timeElapsed += Time.fixedDeltaTime;
        if (duration >= 0f && timeElapsed >= duration){
            enabled = false;
            RemoveEffects();
            return;
        }

        ApplyMotionEffects();
        timeToTick -= Time.fixedDeltaTime;
        if (timeToTick <= 0f){
            timeToTick = Mathf.Max(0.02f, tickInterval);
            ApplyNow();
        }
    }

    /// <summary>立即采样一次范围内的环境效果，天气控制器会调用此接口做局部环境逻辑。</summary>
    public void ApplyNow(){
        List<ChaState> targets = GetTargets();
        for (int i = 0; i < targets.Count; i++) ApplyToTarget(targets[i]);
    }

    /// <summary>收集 AoeState 碰撞列表；独立体积没有碰撞状态时退化为距离扫描。</summary>
    private List<ChaState> GetTargets(){
        affectedCharacters.Clear();
        if (aoeState != null && aoeState.characterInRange != null && aoeState.characterInRange.Count > 0){
            for (int i = 0; i < aoeState.characterInRange.Count; i++){
                GameObject target = aoeState.characterInRange[i];
                ChaState state = target ? target.GetComponent<ChaState>() : null;
                if (state != null && !state.dead && IsAllowed(state) && !affectedCharacters.Contains(state)) affectedCharacters.Add(state);
            }
            return affectedCharacters;
        }

        Vector2 center = transform.position;
        IReadOnlyList<ChaState> allCharacters = ChaState.ActiveCharacters;
        for (int i = 0; i < allCharacters.Count; i++){
            ChaState state = allCharacters[i];
            if (state == null || state.dead || (radius > 0f && Vector2.Distance(center, state.transform.position) > radius)) continue;
            if (IsAllowed(state)) affectedCharacters.Add(state);
        }
        return affectedCharacters;
    }

    /// <summary>按施法者有效阵营过滤友军和敌军，伪装/傀儡状态因此可以正确影响环境。</summary>
    private bool IsAllowed(ChaState target){
        if (target == null) return false;
        ChaState casterState = caster ? caster.GetComponent<ChaState>() : (aoeState != null && aoeState.caster ? aoeState.caster.GetComponent<ChaState>() : null);
        if (casterState == null) return affectEnemies || affectAllies;
        bool ally = casterState.EffectiveSide == target.EffectiveSide;
        return ally ? affectAllies : affectEnemies;
    }

    /// <summary>持续施加龙卷风和磁力等运动效果；状态类效果在采样时处理。</summary>
    private void ApplyMotionEffects(){
        if (volumeKind != EnvironmentVolumeKind.Tornado && volumeKind != EnvironmentVolumeKind.MagneticField) return;
        List<ChaState> targets = GetTargets();
        Vector2 center = transform.position;
        for (int i = 0; i < targets.Count; i++){
            ChaState target = targets[i];
            Vector2 offset = (Vector2)target.transform.position - center;
            Vector2 force = direction;
            if (volumeKind == EnvironmentVolumeKind.Tornado){
                Vector2 radial = offset.sqrMagnitude > 0.001f ? offset.normalized : Vector2.up;
                force = new Vector2(-radial.y, radial.x) * strength + Vector2.up * strength;
            }
            if (force.sqrMagnitude <= 0.0001f) continue;
            force = Vector2.ClampMagnitude(force, Mathf.Max(0.01f, maxSpeed));
            target.AddOrRefreshMotionInfluence(new MotionInfluence(sourceId, force, 0.15f, priority, MotionInfluenceBlendMode.Additive, true));
        }
    }

    /// <summary>采样一次状态和持续伤害，伤害统一排入 DamageManager。</summary>
    private void ApplyToTarget(ChaState target){
        string stateSource = sourceId + "-state";
        switch (volumeKind){
            case EnvironmentVolumeKind.ToxicGas:
                target.AddState(ChaAbilityStateType.Poisoned, stateSource, statusDuration, priority);
                QueueDamage(target, DamageInfoTag.periodDamage);
                break;
            case EnvironmentVolumeKind.Radiation:
                target.AddState(ChaAbilityStateType.Radiated, stateSource, statusDuration, priority);
                QueueDamage(target, DamageInfoTag.periodDamage);
                break;
            case EnvironmentVolumeKind.FireOil:
                target.AddState(ChaAbilityStateType.Burning, stateSource, statusDuration, priority);
                QueueDamage(target, DamageInfoTag.periodDamage);
                break;
            case EnvironmentVolumeKind.IceWall:
                target.AddState(ChaAbilityStateType.Frozen, stateSource, statusDuration, priority);
                break;
            case EnvironmentVolumeKind.Lightning:
                QueueDamage(target, DamageInfoTag.directDamage);
                break;
        }
    }

    private void QueueDamage(ChaState target, DamageInfoTag tag){
        if (!applyDamage || damagePerTick <= 0 || target == null) return;
        GameObject attacker = caster != null ? caster : (aoeState != null ? aoeState.caster : null);
        SceneVariants.CreateDamage(attacker, target.gameObject, new Damage(damagePerTick), 0f, 0f, new DamageInfoTag[]{tag});
    }

    /// <summary>移除环境来源留下的运动影响和状态，避免清除后仍残留控制。</summary>
    private void RemoveEffects(){
        IReadOnlyList<ChaState> allCharacters = ChaState.ActiveCharacters;
        string stateSource = sourceId + "-state";
        for (int i = 0; i < allCharacters.Count; i++){
            ChaState state = allCharacters[i];
            if (state == null) continue;
            state.RemoveForceMove(sourceId);
            state.RemoveState(ChaAbilityStateType.Poisoned, stateSource);
            state.RemoveState(ChaAbilityStateType.Radiated, stateSource);
            state.RemoveState(ChaAbilityStateType.Burning, stateSource);
            state.RemoveState(ChaAbilityStateType.Frozen, stateSource);
        }
    }

    private void OnDestroy(){
        RemoveEffects();
    }}