using System.Collections;
using System.Collections.Generic;
using UnityEngine;


///<summary>
///瑙掕壊鐨勨€滅姸鎬佲€濓紝鐢ㄦ潵绠＄悊褰撳墠搴旇鎬庝箞绉诲姩銆佸簲璇ユ€庝箞鏃嬭浆銆佸簲璇ユ€庝箞鎾斁鍔ㄧ敾鐨勩€?
///鏄竴涓鑹茬殑鎬荤殑鈥滆皟鎺т腑蹇冣€濄€?
///</summary>
/// <summary>角色能力状态类型，阶段一先统一状态语义，后续技能只负责申请状态。</summary>
public enum ChaAbilityStateType{
    Invisible = 0,
    Invincible = 1,
    Underground = 2,
    Floating = 3,
    Flying = 3,
    Airborne = 3,
    WallClimbing = 4,
    Stunned = 5,
    Disguised = 6,
    MindControlled = 7,
    Controlled = 7,
    Poisoned = 8,
    Radiated = 9,
    Burning = 10,
    Frozen = 11,
    // 首批变羊只实现统一控制锁，不直接替换角色模型；视觉替换留给后续角色视图阶段。
    Polymorphed = 12
}

/// <summary>一个带来源、优先级和时长的角色能力状态。</summary>
[System.Serializable]
public class ChaAbilityState{
    public ChaAbilityStateType stateType;
    public string sourceId;
    public int priority;
    /// <summary>伪装状态使用的阵营覆盖值，未设置时保持原阵营。</summary>
    public int sideOverride;
    /// <summary>小于零表示永久状态，否则每个固定帧递减。</summary>
    public float duration;

    public ChaAbilityState(ChaAbilityStateType stateType, string sourceId, float duration, int priority, int sideOverride = int.MinValue){
        this.stateType = stateType;
        this.sourceId = sourceId;
        this.duration = duration;
        this.priority = priority;
        this.sideOverride = sideOverride;
    }

    public bool IsExpired{
        get { return duration >= 0f && duration <= 0f; }
    }
}

/// <summary>控制权来源，玩家、普通 AI、傀儡玩家和脚本强控互相独立。</summary>
public enum ControlAuthorityKind{
    Player = 0,
    AI = 1,
    Puppet = 2,
    Script = 3
}

/// <summary>当前控制权记录，支持优先级、来源和自动释放。</summary>
[System.Serializable]
public class ControlAuthorityState{
    public ControlAuthorityKind kind;
    public string sourceId;
    public int priority;
    public float duration;
    public int controllingSide;

    public ControlAuthorityState(ControlAuthorityKind kind, string sourceId, int priority, float duration, int controllingSide){
        this.kind = kind;
        this.sourceId = sourceId;
        this.priority = priority;
        this.duration = duration;
        this.controllingSide = controllingSide;
    }

    public bool IsExpired{
        get { return duration >= 0f && duration <= 0f; }
    }
}


public class ChaState:MonoBehaviour{
    ///<summary>
    //瑙掕壊鏈€缁堢殑鍙搷浣滄€х姸鎬?
    ///</summary>
    private ChaControlState _controlState = new ChaControlState(true, true, true);

    ///<summary>
    ///GameTimeline涓撲韩鐨凜haControlState
    ///</summary>
    public ChaControlState timelineControlState = new ChaControlState(true,true,true);

    public ChaControlState controlState{
        get{
            ChaControlState result = this._controlState + this.timelineControlState;
            // 眩晕是状态系统的控制锁，不直接改写外部时间轴状态。
            if (HasState(ChaAbilityStateType.Stunned) || HasState(ChaAbilityStateType.Frozen) || HasState(ChaAbilityStateType.Polymorphed)) result += ChaControlState.stun;
            return result;
        }
    }

    ///<summary>
    ///瑙掕壊鐨勬棤鏁岀姸鎬佹寔缁椂闂达紝濡傛灉鍦ㄦ棤鏁岀姸鎬佷腑锛屽瓙寮逛笉浼氱鎾烇紝DamageInfo澶勭悊鏃犳晥鍖?
    ///鍗曚綅锛氱
    ///</summary>
    public float immuneTime{
        get{
            return _immuneTime;
        }
        set{
            _immuneTime = Mathf.Max(_immuneTime, value);
        }
    }
    private float _immuneTime = 0.00f;

    ///<summary>
    ///瑙掕壊鏄惁澶勪簬涓€绉嶈搫鍔涚殑鐘舵€?
    ///</summary>
    public bool charging = false;

    ///<summary>
    ///瑙掕壊涓诲姩鏈熸湜鐨勭Щ鍔ㄦ柟鍚?
    ///</summary>
    public float moveDegree{
        get{
            return _wishToMoveDegree;
        }
    }
    private float _wishToMoveDegree = 0.00f;

    ///<summary>
    ///瑙掕壊涓诲姩鏈熸湜鐨勯潰鍚?
    ///</summary>
    public float faceDegree{
        get{
            return _wishToFaceDegree;
        }
    }
    private float _wishToFaceDegree = 0.00f;

    ///<summary>
    ///瑙掕壊鏄惁宸茬粡姝讳簡锛岃繖涓嶇敱鎴戣繖涓郴缁熷垽鏂紝鍏朵粬绯荤粺搴旇鍛婅瘔鎴?
    ///</summary>
    public bool dead = false;

    /// <summary>
    /// 鏉ヨ嚜鎿嶄綔鎴栬€卆i鐨勭Щ鍔ㄨ姹備俊鎭?
    /// </summary>
    private Vector3 moveOrder = new Vector3();

    /// <summary>
    /// 鏉ヨ嚜寮哄埗鍙戠敓鐨勪綅绉讳俊鎭紝閫氬父鏄妧鑳芥晥鏋滅瓑瀵艰嚧鐨勶紝姣斿缈绘粴銆佽鎺ㄥ紑绛?
    /// </summary>
    // 所有击退、吸附、龙卷风等外力统一进入结算器。
    private List<MotionInfluence> forceMove = new List<MotionInfluence>();

    /// <summary>角色当前能力状态列表，状态效果不再散落在各个技能脚本中。</summary>
    private List<ChaAbilityState> abilityStates = new List<ChaAbilityState>();
    private MoveType baseMoveType = MoveType.ground;

    private ControlAuthorityState controlAuthority;
    private int originalSide = int.MinValue;
    private static readonly List<ChaState> activeCharacters = new List<ChaState>();

    /// <summary>当前场景中可供天气和目标服务查询的角色快照。</summary>
    public static IReadOnlyList<ChaState> ActiveCharacters{ get { return activeCharacters; } }

    /// <summary>
    /// 鏀跺埌鐨勬潵鑷悇鏂圭殑鎾斁鍔ㄧ敾鐨勮姹?
    /// </summary>
    private List<string> animOrder = new List<string>();

    /// <summary>
    /// 鏉ヨ嚜鎿嶄綔鎴栬€卆i鐨勬棆杞搴﹁姹?
    /// </summary>
    private float rotateToOrder;

    /// <summary>
    /// 鏉ヨ嚜寮哄埗鎵ц鐨勬棆杞搴?
    /// </summary>
    private List<float> forceRotate = new List<float>();

    ///<summary>
    ///瑙掕壊鐜版湁鐨勮祫婧愶紝姣斿hp涔嬬被鐨?
    ///</summary>
    public ChaResource resource = new ChaResource(1);

    [Tooltip("瑙掕壊鎵€澶勯樀钀ワ紝闃佃惀涓嶅悓灏变細瀵规墦")]
    ///<summary>
    ///瑙掕壊鎵€澶勯樀钀ワ紝闃佃惀涓嶅悓灏变細瀵规墦
    ///</summary>
    public int side = 0;

    ///<summary>
    ///鏍规嵁tags鍙互鍒ゆ柇鍑鸿繖鏄粈涔堟牱鐨勪汉
    ///</summary>
    public string[] tags = new string[0];

    [SerializeField]
    ///<summary>
    ///瑙掕壊褰撳墠鐨勫睘鎬?
    ///</summary>
    public ChaProperty property{get{
        return _prop;
    }}
    private ChaProperty _prop = ChaProperty.zero;

    ///<summary>
    ///瑙掕壊绉诲姩鍔涳紝鍗曚綅锛氱背/绉?
    ///</summary>
    public float moveSpeed{get{
        //杩欎釜鍏紡涔熷彲浠ラ€氳繃缁欑瓥鍒掕剼鏈帴鍙ｈ幏寰楋紝杩欓噷灏卞啓浠ｇ爜閲屼簡锛屼笉璧扮瓥鍒掕剼鏈簡
        //璁惧畾锛屽€?0.2+5.6*x/(x+100)锛屽垵濮嬮€熷害鏄?00锛岀Щ鍔ㄥ姏3绫?绉掞紝鏈€灏忓€?.2绫?绉掋€?
        return this._prop.moveSpeed * 5.600f / (this._prop.moveSpeed + 100.000f) + 0.200f;
    }}

    ///<summary>
    ///瑙掕壊琛屽姩閫熷害锛屾槸涓€涓猼imescale锛屾渶灏?.1锛屽垵濮嬭鍔ㄩ€熷害鍊间篃鏄?00銆?
    ///</summary>
    public float actionSpeed{
        get{
            return this._prop.actionSpeed * 4.90f / (_prop.actionSpeed + 390.00f) + 0.100f;
        }
    }

    ///<summary>
    ///瑙掕壊鐨勫熀纭€灞炴€э紝涔熷氨鏄瘡涓鑹测€滆８浣撯€濅笖涓嶅甫浠讳綍buff鐨勨€滅函绮圭殑灞炴€р€?
    ///鍏堝啓姝伙紝姝ｅ紡鐨勫簲璇ヨ琛?
    ///</summary>
    public ChaProperty baseProp = new ChaProperty(
        100, 100, 0, 20, 100
    );

    ///<summary>
    ///瑙掕壊鏉ヨ嚜buff鐨勫睘鎬?
    ///杩欎釜鏁扮粍骞朵笉鏄姣忎釜buff鍙互鍗犵敤涓€鏉℃暟鎹紝鑰屾槸鍒嗙被鎬诲拰
    ///鍦ㄨ繖涓父鎴忛噷buff甯︽潵鐨勫睘鎬ф€诲叡鏈?绫伙紝plus鍜宼imes锛岀敤绛栧垝璁捐鐨勫叕寮忓氨鏄痯lus鐨勫睘鎬у姞瀹屼箣鍚庝箻浠imes鐨勫睘鎬?
    ///鎵€浠ユ暟缁勯暱搴﹀叾瀹炲彧鏈?锛歔0]buffPlus, [1]buffTimes
    ///</summary>
    public ChaProperty[] buffProp = new ChaProperty[2]{ChaProperty.zero, ChaProperty.zero};

    ///<summary>
    ///鏉ヨ嚜瑁呭鐨勫睘鎬?
    ///</summary>
    public ChaProperty equipmentProp = ChaProperty.zero;

    ///<summary>
    ///瑙掕壊鐨勬妧鑳?
    ///</summary>
    public List<SkillObj> skills = new List<SkillObj>();

    ///<summary>
    ///瑙掕壊韬笂鐨刡uff
    ///</summary>
    public List<BuffObj> buffs = new List<BuffObj>();

    private const string LearnSkillSourceSkillKey = "__learnSkillId";

    public long BuffCount;

    [SerializeField]
    private bool debugInvincible = false;

    private UnitMove unitMove;
    private UnitAnim unitAnim;
    private UnitRotate unitRotate;
    private Animator animator;
    private UnitBindManager bindPoints;
    private GameObject viewContainer;

    private void OnEnable(){
        if (!activeCharacters.Contains(this)) activeCharacters.Add(this);
    }

    private void OnDisable(){
        activeCharacters.Remove(this);
    }

    void Start() {
        rotateToOrder = transform.rotation.eulerAngles.z;

        synchronizedUnits();
        if (originalSide == int.MinValue) originalSide = side;
        if (unitMove) baseMoveType = unitMove.GetMoveType();

        AttrRecheck();
    }

    void FixedUpdate() {
        float timePassed = Time.fixedDeltaTime;
        TickControlAuthority(timePassed);
        TickAbilityStates(timePassed);
        if (dead == false){
            //濡傛灉瑙掕壊娌℃锛屽仛杩欎簺浜嬫儏锛?

            //鏃犳晫鏃堕棿鍑忓皯
            if (_immuneTime > 0) _immuneTime -= timePassed;

            //鎶€鑳藉喎鍗存椂闂?
            for (int i = 0; i < this.skills.Count; i++){
                if (this.skills[i].cooldown > 0){
                    this.skills[i].cooldown -= timePassed;
                }
            }

            //瀵硅韩涓婄殑buff杩涜绠＄悊
            List<BuffObj> toRemove = new List<BuffObj>();
            for (int i = 0; i < this.buffs.Count; i++){
                if (buffs[i].permanent == false) buffs[i].duration -= timePassed;
                buffs[i].timeElapsed += timePassed;

                if (buffs[i].model.tickTime > 0 && buffs[i].model.onTick != null){
                    //float鍙栨ā涓嶇簿鍑嗭紝鎵€浠ョ敤x1000鍚庣殑鏁存暟鏉?
                    if (Mathf.RoundToInt(buffs[i].timeElapsed * 1000) % Mathf.RoundToInt(buffs[i].model.tickTime * 1000) == 0){
                        buffs[i].model.onTick(buffs[i]);
                        buffs[i].ticked += 1;
                    }
                }

                //鍙duration <= 0锛屼笉绠℃槸鍚︽槸permanent閮界Щ闄ゆ帀
                if (buffs[i].duration <= 0 || buffs[i].stack <= 0){
                    if (buffs[i].model.onRemoved != null){
                        buffs[i].model.onRemoved(buffs[i]);
                    }
                    toRemove.Add(buffs[i]);
                }
            }
            if (toRemove.Count > 0){
                for (int i = 0; i < toRemove.Count; i++){
                    this.buffs.Remove(toRemove[i]);
                }
                AttrRecheck();
            }
            BuffCount = buffs.Count;

            toRemove = null;

            //缁欏悇涓郴缁熷彂娑堟伅
            bool wishToMove = moveOrder != Vector3.zero;
            if (wishToMove == true) 
            _wishToMoveDegree = Mathf.Atan2(moveOrder.x, moveOrder.y) * 180 / Mathf.PI;
            
            ChaControlState curCS = this.controlState;// _controlState + timelineControlState;

            // 先合并自主移动和阶段二外力，再交给 UnitMove 执行。
            Vector3 autonomousMove = curCS.canMove ? moveOrder : Vector3.zero;
            Vector3 resolvedMove = MotionResolver.Resolve(
                autonomousMove, forceMove, timePassed, curCS.canMove
            );
            bool tryRun = resolvedMove != Vector3.zero;
            float tryMoveDegree = Mathf.Atan2(resolvedMove.x, resolvedMove.y) * 180 / Mathf.PI;
            if (tryMoveDegree > 180) tryMoveDegree -= 360;
            if (unitMove) unitMove.MoveBy(resolvedMove);
            moveOrder = Vector3.zero;

            _wishToFaceDegree = rotateToOrder;
            if (wishToMove == false) _wishToMoveDegree = _wishToFaceDegree;
            //鐒跺悗鏄棆杞俊鎭?
            if (unitRotate){
                if (curCS.canRotate == false) rotateToOrder = transform.rotation.eulerAngles.z;
                for (int i = 0; i < forceRotate.Count; i++){
                    //杩欓噷鍏ㄦ槸澧為噺锛岃€屼笉鏄瀹氫负锛屾墍浠ュ彲浠ョ洿鎺ュ姞
                    rotateToOrder += forceRotate[i];
                }
                unitRotate.RotateTo(rotateToOrder);
                forceRotate.Clear();
            }
            //鍐嶆槸鍔ㄧ敾澶勭悊
            if (unitAnim){
                unitAnim.timeScale = this.actionSpeed;
                //鍏堣绠楅粯璁わ紙瑙勫垯涓嬶級鐨勫姩鐢伙紝骞朵笖娣诲姞鍒板姩鐢荤粍
                if (tryRun == false) {
                    animOrder.Add("Stand");    //濡傛灉娌℃湁瑕佹眰绉诲姩锛屽氨鐢ㄧ珯绔?
                }else{
                    string tt = Utils.GetTailStringByDegree(transform.rotation.eulerAngles.z, tryMoveDegree);
                    animOrder.Add("Move" + tt);
                }
                //閫佺粰鍔ㄧ敾绯荤粺澶勭悊
                for (int i = 0; i < animOrder.Count; i++){
                    unitAnim.Play(animOrder[i]);
                }
                animOrder.Clear();
            }
            if (animator){
                animator.speed = this.actionSpeed;
            }
        }else{
            _wishToFaceDegree = transform.rotation.eulerAngles.z * 180.00f / Mathf.PI;
            _wishToMoveDegree = _wishToFaceDegree;
        }
    }

    private void synchronizedUnits(){
        if (!unitMove) unitMove = this.gameObject.GetComponent<UnitMove>();
        if (!unitAnim) unitAnim = this.gameObject.GetComponent<UnitAnim>();
        if (!unitRotate) unitRotate = this.gameObject.GetComponent<UnitRotate>();  
        if (!animator) animator = this.gameObject.GetComponent<Animator>();  
        if (!bindPoints) bindPoints = this.gameObject.GetComponent<UnitBindManager>();
        if (!viewContainer) viewContainer = this.gameObject.GetComponentInChildren<ViewContainer>().gameObject;
    }

    ///<summary>
    ///鍛戒护绉诲姩
    ///<param name="move">绉诲姩鍔?/param>
    ///</summary>
    public void OrderMove(Vector3 move){
        this.moveOrder.x = move.x;
        this.moveOrder.y = move.y;
    }

    ///<summary>
    ///寮哄埗绉诲姩
    ///<param name="moveInfo">绉诲姩淇℃伅</param>
    ///</summary>
    public void AddForceMove(MotionInfluence move){
        if (move != null) this.forceMove.Add(move);
    }

    /// <summary>同一来源的环境力只保留一条，避免天气每帧堆积影响对象。</summary>
    public void AddOrRefreshMotionInfluence(MotionInfluence move){
        if (move == null) return;
        for (int i = 0; i < forceMove.Count; i++){
            MotionInfluence current = forceMove[i];
            if (current != null && current.sourceId == move.sourceId && current.priority == move.priority && current.blendMode == move.blendMode){
                current.velocity = move.velocity;
                current.duration = Mathf.Max(current.duration, move.duration);
                current.ignoreControlState = move.ignoreControlState;
                return;
            }
        }
        forceMove.Add(move);
    }

    /// <summary>旧版击退接口适配到新的运动影响系统。</summary>
    public void AddForceMove(MovePreorder move){
        if (move == null) return;
        this.forceMove.Add(new MotionInfluence(
            "legacy", move.velocity, move.duration, 0,
            MotionInfluenceBlendMode.Additive, true
        ));
    }

    ///<summary>
    ///鍛戒护鏃嬭浆鍒板灏戝害
    ///<param name="degree">鏃嬭浆鐩爣</param>
    ///</summary>
    public void OrderRotateTo(float degree){
        this.rotateToOrder = degree;
    }

    ///<summary>
    ///寮哄埗鏃嬭浆鐨勫姏閲?
    ///<param name="degree">鍋忕Щ瑙掑害</param>
    ///</summary>
    public void AddForceRotate(float degree){
        this.forceRotate.Add(degree);
    }

    ///<summary>
    ///娣诲姞瑙掕壊瑕佸仛鐨勫姩浣滆姹?
    ///<param name="animName">瑕佸仛鐨勫姩浣?/param>
    ///</summary>
    public void Play(string animName){
        animOrder.Add(animName);
    }

    ///<summary>
    ///鏉€姝昏繖涓鑹?
    ///</summary>
    /// <summary>
    /// 将角色标记为死亡并发布统一死亡事件。重复调用只保留第一次，避免掉落、计数和 Boss 进度重复触发。
    /// </summary>
    public void Kill(GameObject killer = null){
        if (this.dead) return;
        this.dead = true;
        CharacterDeathService.Publish(this, killer);
        if (unitAnim){
            unitAnim.Play("Dead");
        }
        //濡傛灉涓嶆槸涓昏锛屽案浣撳氨浼氭秷澶?
        if (this.gameObject != SceneVariants.MainActor())
            this.gameObject.AddComponent<UnitRemover>().duration = 5.0f;
    }

    ///<summary>
    ///閲嶆柊璁＄畻鎵€鏈夊睘鎬э紝骞朵笖鑾峰緱涓€涓渶缁堝睘鎬?
    ////鍏跺疄杩欎釜搴旇璧拌剼鏈嚱鏁拌繑鍥烇紝鎶涚粰鑴氭湰鍑芥暟澶氫釜ChaProperty锛岀敱鑴氭湰鍑芥暟杩愪綔浠栦滑鐨勮繍绠楀叧绯伙紝骞惰繑鍥炵粨鏋?
    ///</summary>
    private void AttrRecheck(){
        _controlState.Origin();
        for (var i = 0; i < buffProp.Length; i++) buffProp[i].Zero();
        for (int i = 0; i < this.buffs.Count; i++){
            for (int j = 0; j < Mathf.Min(buffProp.Length, buffs[i].model.propMod.Length); j++){
                buffProp[j] += buffs[i].model.propMod[j] * buffs[i].stack;
            }
            _controlState += buffs[i].model.stateMod;
        }
        
        this._prop = CharacterStatsUtility.BuildFinalProperty(this.baseProp, this.equipmentProp, this.buffProp);

        if (unitMove){
            unitMove.bodyRadius = this._prop.bodyRadius;
        }
    }

    ///<summary>
    ///澧炲姞瑙掕壊鐨勮閲忕瓑璧勬簮锛岀洿鎺ユ敼鍙樻暟瀛楃殑锛屽睘浜庢渶鍚庝竴姝ユ搷浣滀簡
    ///<param name="value">瑕佹敼鍙樼殑閲忥紝璐熸暟涓哄噺灏?/param>
    ///</summary>
    /// <summary>修改角色资源；killer 只在本次修改导致死亡时传给统一死亡事件。</summary>
    public void ModResource(ChaResource value, GameObject killer = null){
        this.resource = CharacterStatsUtility.ClampResource(this.resource + value, this._prop);
        if (this.resource.hp <= 0){
            this.Kill(killer);
        }
    }


    ///<summary>
    ///鍦ㄨ鑹茶韩涓婃斁涓€涓壒鏁堬紝鍏跺疄鏄寕鍦ㄤ竴涓猤ameObject鑰屽凡
    ///<param name="bindPointKey">缁戠偣鍚嶇О锛岃鑹叉湁Muzzle/Head/Body杩?涓紝闇€瑕佸啀鍔?/param>
    ///<param name="effect">瑕佹挱鏀剧殑鐗规晥鏂囦欢鍚嶏紝缁熶竴璧癙refabs/涓嬫嬁</param>
    ///<param name="effectKey">杩欎釜鐗规晥鐨刱ey锛岃鍒犻櫎鐨勬椂鍊欏氨鏈夌敤浜?/param>
    ///<param name="effect">瑕佹挱鏀剧殑鐗规晥</param>
    ///</summary>
    public void PlaySightEffect(string bindPointKey, string effect, string effectKey = "", bool loop = false){
        bindPoints.AddBindGameObject(bindPointKey, "Prefabs/" + effect, effectKey, loop);
    }

    ///<summary>
    ///鍒犻櫎瑙掕壊韬笂鐨勪竴涓壒鏁?
    ///<param name="bindPointKey">缁戠偣鍚嶇О锛岃鑹叉湁Muzzle/Head/Body杩?涓紝闇€瑕佸啀鍔?/param>
    ///<param name="effectKey">杩欎釜鐗规晥鐨刱ey锛岃鍒犻櫎鐨勬椂鍊欏氨鏈夌敤浜?/param>
    ///</summary>
    public void StopSightEffect(string bindPointKey, string effectKey){
        bindPoints.RemoveBindGameObject(bindPointKey, effectKey);
    }

    ///<summary>
    ///鍒ゆ柇杩欎釜瑙掕壊鏄惁浼氳杩欎釜damageInfo鎵€鏉€
    ///<param name="dInfo">瑕佸垽鏂殑damageInfo</param>
    ///<return>濡傛灉鏄痶rue浠ｈ〃瑙掕壊鍙兘浼氳杩欐浼ゅ鎵€鏉€</return>
    ///</summary>
    public bool CanBeKilledByDamageInfo(DamageInfo damageInfo){
          if (IsDamageBlocked(damageInfo)) return false;
          return CharacterStatsUtility.CanBeKilledByDamageInfo(this.resource, this.immuneTime, damageInfo);
    }

    ///<summary>
    ///涓鸿鑹叉坊鍔燽uff锛屽綋鐒讹紝鍒犻櫎涔熸槸璧拌繖涓殑
    ///</summary>
    public void AddBuff(AddBuffInfo buff){
        List<GameObject> bCaster = new List<GameObject>();
        if (buff.caster) bCaster.Add(buff.caster);
        List<BuffObj> hasOnes = GetBuffById(buff.buffModel.id, bCaster);
        int modStack = Mathf.Min(buff.addStack, buff.buffModel.maxStack);
        bool toRemove = false;
        BuffObj toAddBuff = null;
        if (hasOnes.Count > 0){
            //宸茬粡瀛樺湪
            hasOnes[0].buffParam = new Dictionary<string, object>();
            //杞瓨param
            if (buff.buffParam != null){
                foreach (KeyValuePair<string, object> kv in buff.buffParam){hasOnes[0].buffParam[kv.Key] = kv.Value;};
            }
            
            hasOnes[0].duration = (buff.durationSetTo == true) ? buff.duration : (buff.duration + hasOnes[0].duration);
            int afterAdd = hasOnes[0].stack + modStack;
            modStack = afterAdd >= hasOnes[0].model.maxStack ? 
                (hasOnes[0].model.maxStack - hasOnes[0].stack) : 
                (afterAdd <= 0 ? (0 - hasOnes[0].stack) : modStack);
            hasOnes[0].stack += modStack;
            hasOnes[0].permanent = buff.permanent;
            toAddBuff = hasOnes[0];
            toRemove = hasOnes[0].stack <= 0;
        }else{
            //鏂板缓
            toAddBuff = new BuffObj(
                buff.buffModel,
                buff.caster,
                this.gameObject,
                buff.duration,
                buff.addStack,
                buff.permanent,
                buff.buffParam
            );
            buffs.Add(toAddBuff);
            buffs.Sort((a, b)=>{
                return a.model.priority.CompareTo(b.model.priority);
            });
        }
        if (toRemove == false && buff.buffModel.onOccur != null){
            buff.buffModel.onOccur(toAddBuff, modStack);
        }
        AttrRecheck();
    }

    ///<summary>
    ///鑾峰彇瑙掕壊韬笂瀵瑰簲鐨刡uffObj
    ///<param name="id">buff鐨刴odel鐨刬d</param>
    ///<param name="caster">濡傛灉caster涓嶆槸绌猴紝閭ｄ箞灏变唬琛ㄥ彧鏈塨uffObj.caster鍦╟aster閲岄潰鐨勬墠绗﹀悎鏉′欢</param>
    ///<return>绗﹀悎鏉′欢鐨刡uffObj鏁扮粍</return>
    ///</summary>
    public List<BuffObj> GetBuffById(string id, List<GameObject> caster = null){
        List<BuffObj> res = new List<BuffObj>();
        for (int i = 0; i < this.buffs.Count;  i++){
            if (buffs[i].model.id == id && (caster == null || caster.Count <= 0 || caster.Contains(buffs[i].caster) == true)){
                res.Add(buffs[i]);
            }
        }
        return res;
    }

    ///<summary>
    ///鏍规嵁id鑾峰緱瑙掕壊瀛︿細鐨勬妧鑳斤紙skillObj锛夛紝濡傛灉娌℃湁鍒欒繑鍥瀗ull
    ///<param name="id">鎶€鑳界殑id</param>
    ///<return>skillObj or null</return>
    ///</summary>
    public SkillObj GetSkillById(string id){
        for (int i = 0; i < skills.Count; i++ ){
            if (skills[i].model.id == id){
                return skills[i];
            }
        }
        return null;
    }
    
    ///<summary>
    ///閲婃斁涓€涓妧鑳斤紝閲婃斁鎶€鑳藉苟涓嶆€绘槸鎴愬姛鐨勶紝濡傛灉浣犱竴鐩村彂閲婃斁鎶€鑳界殑鍛戒护锛岄偅澶辫触鐜囧簲璇ユ槸楠ゅ鐨?
    ///<param name="id">瑕侀噴鏀剧殑鎶€鑳界殑id</param>
    ///<return>鏄惁閲婃斁鎴愬姛</return>
    ///</summary>
    public SkillCastResult TryCastSkill(string id){
        return SkillExecutionService.TryCast(this, id);
    }

    public SkillCastResult TryCastSkill(SkillCastRequest request){
        return SkillExecutionService.TryCast(request);
    }

    public bool CastSkill(string id){
        return TryCastSkill(id).success;
    }

    public void InitBaseProp(ChaProperty cProp){
        this.baseProp = cProp;
        this.AttrRecheck();
        this.resource.hp = this._prop.hp;
        this.resource.ammo = this._prop.ammo;
        this.resource.stamina = 100;
    }

    ///<summary>
    ///瀛︿範鏌愪釜鎶€鑳?
    ///<param name="skillModel">鎶€鑳界殑妯℃澘</param>
    ///<param name="level">鎶€鑳界瓑绾?/param>
    ///</summary>
    public bool LearnSkill(SkillModel skillModel, int level = 1){
        if (string.IsNullOrEmpty(skillModel.id))
        {
            Debug.LogWarning("[ChaState] Cannot learn a skill with an empty id.");
            return false;
        }

        SkillObj existing = GetSkillById(skillModel.id);
        if (existing != null)
        {
            existing.level = Mathf.Max(existing.level, level);
            return true;
        }

        this.skills.Add(new SkillObj(skillModel, level));
        if (skillModel.buff != null){
            for (int i = 0; i < skillModel.buff.Length; i++){
                AddBuffInfo abi = skillModel.buff[i];
                if (abi.buffParam == null)
                {
                    abi.buffParam = new Dictionary<string, object>();
                }
                abi.buffParam[LearnSkillSourceSkillKey] = skillModel.id;
                abi.permanent = true;
                abi.duration = 10;
                abi.durationSetTo = true;
                this.AddBuff(abi);
            }
        }
        return true;
    }

    public bool RemoveSkill(string skillId)
    {
        if (string.IsNullOrEmpty(skillId))
        {
            return false;
        }

        SkillObj skill = GetSkillById(skillId);
        if (skill == null)
        {
            return false;
        }

        this.skills.Remove(skill);
        RemoveLearnedSkillBuffs(skillId);
        return true;
    }

    public bool HasSkill(string skillId)
    {
        return GetSkillById(skillId) != null;
    }

    public bool IsInvincible{
        get{
            return debugInvincible || HasState(ChaAbilityStateType.Invincible) || _immuneTime > 0f;
        }
    }

    // 目标筛选系统会通过反射读取这两个小驼峰属性，保留兼容命名。
    public bool isInvincible{ get { return IsInvincible; } }
    public bool isInvisible{ get { return HasState(ChaAbilityStateType.Invisible); } }
    public bool IsInvisible{ get { return isInvisible; } }
    public bool IsFrozen{ get { return HasState(ChaAbilityStateType.Frozen); } }
    public bool IsPoisoned{ get { return HasState(ChaAbilityStateType.Poisoned); } }
    public bool IsRadiated{ get { return HasState(ChaAbilityStateType.Radiated); } }
    public bool IsBurning{ get { return HasState(ChaAbilityStateType.Burning); } }


    public bool IsMoveControlEnabled
    {
        get
        {
            return controlState.canMove;
        }
    }

    /// <summary>按来源移除阶段二外力，环境效果结束时调用。</summary>
    public int RemoveForceMove(string sourceId){
        int removed = 0;
        for (int i = forceMove.Count - 1; i >= 0; i--){
            MotionInfluence influence = forceMove[i];
            if (influence != null && influence.sourceId == sourceId){
                forceMove.RemoveAt(i);
                removed++;
            }
        }
        return removed;
    }

    /// <summary>原始阵营，不随傀儡控制改变。</summary>
    public int OriginalSide{ get { return originalSide == int.MinValue ? side : originalSide; } }

    /// <summary>有效阵营，目标关系只读取这个值。</summary>
    public int EffectiveSide{
        get{
            if (controlAuthority != null && !controlAuthority.IsExpired && controlAuthority.controllingSide != int.MinValue) return controlAuthority.controllingSide;
            int disguisedSide = int.MinValue;
            int disguisedPriority = int.MinValue;
            for (int i = 0; i < abilityStates.Count; i++){
                ChaAbilityState state = abilityStates[i];
                if (state != null && !state.IsExpired && state.stateType == ChaAbilityStateType.Disguised &&
                    state.sideOverride != int.MinValue && state.priority >= disguisedPriority){
                    disguisedSide = state.sideOverride;
                    disguisedPriority = state.priority;
                }
            }
            if (disguisedSide != int.MinValue) return disguisedSide;
            return side;
        }
    }

    /// <summary>当前是否被外部控制；被控时普通 AI 不再提交移动和攻击意图。</summary>
    public bool IsControlled{ get { return controlAuthority != null && !controlAuthority.IsExpired; } }
    public ControlAuthorityState CurrentControlAuthority{ get { return controlAuthority; } }

    /// <summary>申请控制权，低优先级控制不能覆盖高优先级控制。</summary>
    public bool AcquireControl(ControlAuthorityKind kind, string sourceId, int priority, float duration = -1f, int controllingSide = int.MinValue){
        if (string.IsNullOrEmpty(sourceId)) sourceId = "control";
        if (controlAuthority != null && !controlAuthority.IsExpired && priority < controlAuthority.priority) return false;
        controlAuthority = new ControlAuthorityState(kind, sourceId, priority, duration, controllingSide);
        AddState(ChaAbilityStateType.MindControlled, sourceId, duration, priority);
        return true;
    }

    /// <summary>释放指定来源的控制权并清理对应状态。</summary>
    public bool ReleaseControl(string sourceId){
        if (controlAuthority == null || (sourceId != null && controlAuthority.sourceId != sourceId)) return false;
        string releasedSource = controlAuthority.sourceId;
        controlAuthority = null;
        RemoveState(ChaAbilityStateType.MindControlled, releasedSource);
        return true;
    }

    private void TickControlAuthority(float deltaTime){
        if (controlAuthority == null) return;
        if (controlAuthority.duration >= 0f) controlAuthority.duration -= Mathf.Max(0f, deltaTime);
        if (controlAuthority.IsExpired) ReleaseControl(controlAuthority.sourceId);
    }


    /// <summary>查询尚未过期的能力状态。</summary>
    public bool HasState(ChaAbilityStateType stateType, string sourceId = null){
        for (int i = 0; i < abilityStates.Count; i++){
            ChaAbilityState state = abilityStates[i];
            if (state != null && !state.IsExpired && state.stateType == stateType &&
                (sourceId == null || state.sourceId == sourceId)) return true;
        }
        return false;
    }

    /// <summary>新增或刷新一个带来源的能力状态，duration 小于零表示永久。</summary>
    public ChaAbilityState AddState(ChaAbilityStateType stateType, string sourceId = "manual", float duration = -1f, int priority = 0, int sideOverride = int.MinValue){
        float resolvedDuration = duration;
        AbilityStateResistanceProfile resistanceProfile = GetComponent<AbilityStateResistanceProfile>();
        if (resistanceProfile != null && !resistanceProfile.TryAdjustDuration(stateType, ref resolvedDuration)) return null;
        for (int i = 0; i < abilityStates.Count; i++){
            ChaAbilityState state = abilityStates[i];
            if (state != null && state.stateType == stateType && state.sourceId == sourceId){
                state.duration = resolvedDuration;
                state.priority = priority;
                state.sideOverride = sideOverride;
                RefreshMovementState();
                if (stateType == ChaAbilityStateType.Polymorphed) AbilityVisualStateBinder.EnsureFor(this);
                return state;
            }
        }
        ChaAbilityState added = new ChaAbilityState(stateType, sourceId, resolvedDuration, priority, sideOverride);
        abilityStates.Add(added);
        RefreshMovementState();
        if (stateType == ChaAbilityStateType.Polymorphed) AbilityVisualStateBinder.EnsureFor(this);
        return added;
    }

    /// <summary>返回指定状态剩余的最长时间；负数代表永久状态，零代表当前不存在。</summary>
    public float GetStateRemaining(ChaAbilityStateType stateType){
        float remaining = 0f;
        for (int i = 0; i < abilityStates.Count; i++){
            ChaAbilityState state = abilityStates[i];
            if (state == null || state.IsExpired || state.stateType != stateType) continue;
            if (state.duration < 0f) return -1f;
            remaining = Mathf.Max(remaining, state.duration);
        }
        return remaining;
    }

    /// <summary>移除指定来源的状态；sourceId 为空时移除该类型全部状态。</summary>
    public int RemoveState(ChaAbilityStateType stateType, string sourceId = null){
        int removed = 0;
        for (int i = abilityStates.Count - 1; i >= 0; i--){
            ChaAbilityState state = abilityStates[i];
            if (state != null && state.stateType == stateType && (sourceId == null || state.sourceId == sourceId)){
                abilityStates.RemoveAt(i);
                removed++;
            }
        }
        if (removed > 0) RefreshMovementState();
        return removed;
    }

    /// <summary>设置没有能力状态覆盖时使用的基础移动模式。</summary>
    public void SetMoveType(MoveType moveType){
        baseMoveType = moveType;
        RefreshMovementState();
    }

    private void TickAbilityStates(float deltaTime){
        bool movementChanged = false;
        for (int i = abilityStates.Count - 1; i >= 0; i--){
            ChaAbilityState state = abilityStates[i];
            if (state == null){ abilityStates.RemoveAt(i); continue; }
            if (state.duration >= 0f) state.duration -= Mathf.Max(0f, deltaTime);
            if (state.IsExpired){
                movementChanged = movementChanged || IsMovementState(state.stateType);
                abilityStates.RemoveAt(i);
            }
        }
        if (movementChanged) RefreshMovementState();
    }

    private bool IsMovementState(ChaAbilityStateType stateType){
        return stateType == ChaAbilityStateType.Underground || stateType == ChaAbilityStateType.Floating || stateType == ChaAbilityStateType.WallClimbing;
    }

    private void RefreshMovementState(){
        if (!unitMove) return;
        MoveType selected = baseMoveType;
        int selectedPriority = int.MinValue;
        for (int i = 0; i < abilityStates.Count; i++){
            ChaAbilityState state = abilityStates[i];
            if (state == null || state.IsExpired || !IsMovementState(state.stateType) || state.priority < selectedPriority) continue;
            selectedPriority = state.priority;
            if (state.stateType == ChaAbilityStateType.Underground) selected = MoveType.underground;
            else if (state.stateType == ChaAbilityStateType.WallClimbing) selected = MoveType.wallClimbing;
            else selected = MoveType.fly;
        }
        unitMove.SetMoveType(selected);
    }

    public void SetDebugInvincible(bool enabled)
    {
        debugInvincible = enabled;
        if (!enabled)
        {
            SetImmuneTime(0f, true);
        }
    }

    public void SetMoveEnabled(bool enabled)
    {
        synchronizedUnits();
        _controlState.canMove = enabled;
        if (!enabled)
        {
            moveOrder = Vector3.zero;
            forceMove.Clear();
            if (unitMove)
            {
                unitMove.DisableMove();
            }
        }
        else if (unitMove)
        {
            unitMove.EnableRotate();
        }
    }

    public void SetImmuneTime(float time, bool force = false)
    {
        if (force)
        {
            _immuneTime = Mathf.Max(0f, time);
            return;
        }

        this.immuneTime = time;
    }

    public bool IsDamageBlocked(DamageInfo damageInfo)
    {
        return IsInvincible && damageInfo != null && damageInfo.isHeal() == false;
    }

    private void RemoveLearnedSkillBuffs(string skillId)
    {
        bool removedAny = false;
        for (int i = this.buffs.Count - 1; i >= 0; i--)
        {
            BuffObj buff = this.buffs[i];
            if (buff == null || buff.buffParam == null)
            {
                continue;
            }

            object sourceSkillId;
            if (!buff.buffParam.TryGetValue(LearnSkillSourceSkillKey, out sourceSkillId))
            {
                continue;
            }

            if (!(sourceSkillId is string) || (string)sourceSkillId != skillId)
            {
                continue;
            }

            if (buff.model.onRemoved != null)
            {
                buff.model.onRemoved(buff);
            }
            this.buffs.RemoveAt(i);
            removedAny = true;
        }

        if (removedAny)
        {
            AttrRecheck();
        }
    }

    ///<summary>
    ///璁剧疆瑙嗚鍏冪礌
    ///</summary>
    public void SetView(GameObject view, Dictionary<string, AnimInfo> animInfo){
        if (view == null) return;
        synchronizedUnits();
        view.transform.SetParent(viewContainer.transform);
        view.transform.position = new Vector3(0, this.gameObject.transform.position.y, 0);
        this.gameObject.transform.position = new Vector3(
            this.gameObject.transform.position.x,
            this.gameObject.transform.position.y,
            0
        );
        this.gameObject.GetComponent<UnitAnim>().animInfo = animInfo;
    }

    ///<summary>
    ///鏄惁鎷ユ湁鏌愪釜tag
    ///</summary>
    public bool HasTag(string tag){
        if (this.tags == null || this.tags.Length <= 0) return false;
        for (int i = 0; i < this.tags.Length; i++){
            if(tags[i] == tag){
                return true;
            }
        }
        return false;
    }
}

