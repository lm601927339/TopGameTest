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
