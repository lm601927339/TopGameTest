using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameDesignConfig designConfig;

    public GameObject mainActor
    {
        get { return mainCharacter; }
    }

    private GameObject mainCharacter;
    private GameObject root;
    private readonly BattleRunStateMachine battleRun = new BattleRunStateMachine();
    private BattleResultPanel resultPanel;

    /// <summary>当前单局状态；波次系统和后续结算系统只能在 InProgress 阶段推进。</summary>
    public BattleRunState BattleState { get { return battleRun.State; } }
    public bool IsBattleInProgress { get { return battleRun.IsInProgress; } }

    private GameDesignConfig Config
    {
        get
        {
            return designConfig != null ? designConfig : GameRuntime.Config;
        }
    }

    private void Awake()
    {
        // 声音警报只服务于当前战局，场景切换时不能让新生成的敌人调查上一局的位置。
        SoundAlertService.Clear();
        GameRuntime.Initialize(this, designConfig);
    }

    void Start()
    {
        GameStartupValidator.Validate(Config);
        root = GameRuntime.FindSceneObject(Config.rootObjectName);
        GameRuntime.SetRoot(root);

        SceneVariants.RandomMap(Config.mapWidth, Config.mapHeight, new Vector2(Config.tileSpacing, Config.tileSpacing), Config.waterline);
        CreateMapGameObjects();

        Vector3 playerPos = SceneVariants.map.GetRandomPosForCharacter(new RectInt(0, 0, SceneVariants.map.MapWidth(), SceneVariants.map.MapHeight()));
        HeroDefinition selectedHero;
        bool useHeroDefinition = Config.TryGetSelectedHero(out selectedHero);
        mainCharacter = CreateCharacter(
            useHeroDefinition ? selectedHero.prefabPath : Config.playerPrefabPath,
            Config.playerSide,
            new Vector3(),
            useHeroDefinition ? selectedHero.baseProperty : Config.BuildPlayerProperty(),
            0,
            useHeroDefinition ? selectedHero.viewAnimInfo : Config.playerViewAnimInfo
        );

        if (mainCharacter)
        {
            mainCharacter.AddComponent<PlayerController>().mainCamera = GameRuntime.MainCamera;
            mainCharacter.transform.position = playerPos;
            GameRuntime.SetMainActor(mainCharacter);

            // 英雄模板负责属性、技能和状态视觉绑定；旧配置无法解析时仍走原有技能列表。
            if (useHeroDefinition)
            {
                HeroRuntime heroRuntime = mainCharacter.GetComponent<HeroRuntime>();
                if (heroRuntime == null) heroRuntime = mainCharacter.AddComponent<HeroRuntime>();
                if (!heroRuntime.Configure(selectedHero))
                {
                    Debug.LogWarning("[GameManager] Hero configuration failed. Falling back to legacy starting skills.");
                    useHeroDefinition = false;
                }
            }
        }

        GameObject mainCameraObj = GameRuntime.FindSceneObject(Config.mainCameraObjectName);
        if (mainCameraObj)
        {
            CamFollow camFollow = mainCameraObj.GetComponent<CamFollow>();
            if (camFollow)
            {
                camFollow.SetFollowCharacter(mainCharacter);
            }
        }

        GameObject playerHpObj = GameRuntime.FindSceneObject(Config.playerHpObjectName);
        if (playerHpObj)
        {
            PlayerStateListener listener = playerHpObj.GetComponent<PlayerStateListener>();
            if (listener)
            {
                listener.playerGameObject = mainCharacter;
            }
        }

        ChaState mcs = mainCharacter ? mainCharacter.GetComponent<ChaState>() : null;
        if (mcs && !useHeroDefinition)
        {
            LearnLegacyStartingSkills(mcs);
        }

        // 选择面板只负责保存下一局英雄并重载当前场景，不在战斗中直接替换角色，避免残留技能和 Buff。
        if (Config.showHeroSelectionPanel) HeroSelectionPanel.Ensure(Config);
        // 结算面板同样由代码创建，当前阶段不要求制作正式预制体。
        resultPanel = BattleResultPanel.Ensure(Config);

        // 奖励服务订阅统一死亡事件；当前只记录局内经验和货币，不要求制作拾取物预制体。
        BattleRewardService rewardService = GetComponent<BattleRewardService>();
        if (rewardService == null) rewardService = gameObject.AddComponent<BattleRewardService>();
        rewardService.ConfigureForBattle(Config, mainCharacter);

        // 随机强化共享统一暂停服务；当前阶段由代码创建选择面板，不制作正式预制体。
        BattlePauseService pauseService = GetComponent<BattlePauseService>();
        if (pauseService == null) pauseService = gameObject.AddComponent<BattlePauseService>();
        BattleUpgradeService upgradeService = GetComponent<BattleUpgradeService>();
        if (upgradeService == null) upgradeService = gameObject.AddComponent<BattleUpgradeService>();
        upgradeService.ConfigureForBattle(
            Config,
            mainCharacter,
            useHeroDefinition ? selectedHero : null
        );
        if (Config.enableBattleUpgrades) BattleUpgradePanel.Ensure(Config, upgradeService);

        // 旧场景可能没有手动放置刷怪器；自动补齐后仍使用同一套设计配置。
        MobSpawnManager mobSpawner = FindObjectOfType<MobSpawnManager>();
        if (mobSpawner == null) mobSpawner = gameObject.AddComponent<MobSpawnManager>();
        mobSpawner.ConfigureForBattle(Config);
    }

    /// <summary>保留旧场景的玩家技能装配，供没有选择有效英雄时安全回退。</summary>
    private void LearnLegacyStartingSkills(ChaState state)
    {
        if (state == null || Config.startingSkills == null) return;
        for (int i = 0; i < Config.startingSkills.Length; i++)
        {
            string skillId = Config.startingSkills[i];
            SkillModel skillModel;
            if (GameDatabase.TryGetSkill(skillId, out skillModel)) state.LearnSkill(skillModel);
        }
    }

    void Update()
    {
        if (!battleRun.IsInProgress) return;

        ChaState playerState = mainCharacter ? mainCharacter.GetComponent<ChaState>() : null;
        if (playerState != null && playerState.dead)
        {
            ResolveDefeat();
        }
    }

    /// <summary>
    /// 由波次/Boss 系统在目标完成时调用。若玩家在同一帧已经死亡，结果以失败为准。
    /// </summary>
    public void ResolveVictory(string detail = "已完成本局目标")
    {
        ChaState playerState = mainCharacter ? mainCharacter.GetComponent<ChaState>() : null;
        bool playerAlive = playerState != null && !playerState.dead;
        if (!battleRun.TryVictory(playerAlive)) return;

        StopBattleActors(false);
        if (resultPanel == null) resultPanel = BattleResultPanel.Ensure(Config);
        if (resultPanel != null) resultPanel.Show(BattleRunState.Victory, detail);
        Debug.Log("[GameManager] Battle resolved: victory.");
    }

    /// <summary>玩家死亡后的唯一失败入口，重复的伤害/死亡检查不会重复弹出界面或清场。</summary>
    private void ResolveDefeat()
    {
        if (!battleRun.TryDefeat()) return;

        StopBattleActors(true);
        if (resultPanel == null) resultPanel = BattleResultPanel.Ensure(Config);
        if (resultPanel != null) resultPanel.Show(BattleRunState.Defeat, "英雄已倒下，重新集结后再战。");
        Debug.Log("[GameManager] Battle resolved: defeat.");
    }

    /// <summary>
    /// 停止当前局的控制与刷怪。失败会额外清理仍存活的敌人；胜利则保留最后死亡瞬间用于后续表现扩展。
    /// </summary>
    private void StopBattleActors(bool clearLivingEnemies)
    {
        BattleUpgradeService upgradeService = GetComponent<BattleUpgradeService>();
        if (upgradeService != null) upgradeService.CloseSelection();
        BattlePauseService pauseService = GetComponent<BattlePauseService>();
        if (pauseService != null) pauseService.ForceResume();
        PlayerController playerController = mainCharacter ? mainCharacter.GetComponent<PlayerController>() : null;
        if (playerController != null) playerController.enabled = false;

        MobSpawnManager mobSpawner = FindObjectOfType<MobSpawnManager>();
        if (mobSpawner != null) mobSpawner.enabled = false;
        if (clearLivingEnemies) GameRuntime.ClearCharactersBySide(Config.enemySide);

        // 胜利后可能仍残留一颗已经发射的敌方子弹；给存活玩家无敌避免结算被覆盖。
        ChaState playerState = mainCharacter ? mainCharacter.GetComponent<ChaState>() : null;
        if (playerState != null && !playerState.dead && !clearLivingEnemies)
        {
            playerState.immuneTime = 99999f;
        }
    }

    private GameObject CreateFromPrefab(string prefabPath, Vector3 position = new Vector3(), float rotation = 0.00f)
    {
        GameObject prefab = GameRuntime.LoadPrefab(prefabPath);
        if (!prefab)
        {
            return null;
        }

        GameObject go = Instantiate<GameObject>(
            prefab,
            position,
            Quaternion.identity
        );
        if (rotation != 0)
        {
            go.transform.Rotate(new Vector3(0, rotation, 0));
        }

        Transform parent = root ? root.transform : GameRuntime.RootTransform;
        if (parent)
        {
            go.transform.SetParent(parent);
        }
        return go;
    }

    private void CreateMapGameObjects()
    {
        for (var i = 0; i < SceneVariants.map.MapWidth(); i++)
        {
            for (var j = 0; j < SceneVariants.map.MapHeight(); j++)
            {
                CreateFromPrefab(SceneVariants.map.grid[i, j].prefabPath, new Vector3(i * SceneVariants.map.gridSize.x, j * SceneVariants.map.gridSize.y, Config.tileDepth));
            }
        }
    }

    private void CreateWallGameObjects()
    {
        for (int i = 0; i < SceneVariants.wallList.Count; i++)
        {
            var wallInfo = SceneVariants.wallList[i];
            GameObject go = CreateFromPrefab(
                Config.wallPrefabPath,
                new Vector3(wallInfo.wallRect.x + wallInfo.wallRect.width / 2, 1.5f, wallInfo.wallRect.y + wallInfo.wallRect.height / 2)
            );
            if (!go) continue;
            go.transform.localScale = new Vector3(wallInfo.wallScale.x, 3, wallInfo.wallScale.y);
        }
    }

    public void CreateBullet(BulletLauncher bulletLauncher)
    {
        GameObject bulletObj = CreateFromPrefab(Config.bulletPrefabPath, bulletLauncher.firePosition);
        if (!bulletObj) return;

        // 只在子弹对象实际生成后发出开火警报，避免资源缺失时产生不存在的声音线索。
        SoundAlertService.ReportGunfire(bulletLauncher.firePosition, bulletLauncher.caster);

        bulletObj.transform.RotateAround(bulletObj.transform.position, Vector3.forward, bulletLauncher.fireDegree);
        BulletState bulletState = bulletObj.GetComponent<BulletState>();
        if (bulletState)
        {
            bulletState.InitByBulletLauncher(
                bulletLauncher,
                GameObject.FindGameObjectsWithTag(Config.heroTag)
            );
        }
    }

    public void RemoveBullet(GameObject bullet, bool immediately = false)
    {
        if (!bullet) return;
        BulletState bulletState = bullet.GetComponent<BulletState>();
        if (!bulletState) return;
        bulletState.duration = 0;
        if (immediately == true)
        {
            if (bulletState.model.onRemoved != null)
            {
                bulletState.model.onRemoved(bullet);
            }
            Destroy(bullet);
        }
    }

    public void CreateAoE(AoeLauncher aoeLauncher)
    {
        GameObject aoeObj = CreateFromPrefab(Config.aoePrefabPath, aoeLauncher.position);
        if (!aoeObj) return;

        // 目前只有策划表中的炸弹爆炸属于调查型爆炸声，毒雾、黑洞等 AoE 不应被误报为爆炸。
        if (aoeLauncher.model.id == "BoomExplosive")
        {
            SoundAlertService.ReportExplosion(aoeLauncher.position, aoeLauncher.caster);
        }

        AoeState aoeState = aoeObj.GetComponent<AoeState>();
        if (aoeState)
        {
            aoeState.InitByAoeLauncher(aoeLauncher);
        }
    }

    public void RemoveAoE(GameObject aoe, bool immediately = false)
    {
        if (!aoe) return;
        AoeState aoeState = aoe.GetComponent<AoeState>();
        if (!aoeState) return;
        aoeState.duration = 0;
        if (immediately == true)
        {
            if (aoeState.model.onRemoved != null)
            {
                aoeState.model.onRemoved(aoe);
            }
            Destroy(aoe);
        }
    }

    public GameObject CreateCharacter(string prefab, int side, Vector3 pos, ChaProperty baseProp, float degree, string unitAnimInfo = "Default_Gunner", string[] tags = null)
    {
        GameObject chaObj = CreateFromPrefab(Config.characterRootPrefab);
        if (!chaObj) return null;

        ChaState cs = chaObj.GetComponent<ChaState>();
        if (cs)
        {
            cs.InitBaseProp(baseProp);
            cs.side = side;
            Dictionary<string, AnimInfo> aInfo = new Dictionary<string, AnimInfo>();
            if (unitAnimInfo != "")
            {
                Dictionary<string, AnimInfo> loadedAnimInfo;
                if (GameDatabase.TryGetUnitAnimInfo(unitAnimInfo, out loadedAnimInfo))
                {
                    aInfo = loadedAnimInfo;
                }
            }
            cs.SetView(CreateFromPrefab(prefab), aInfo);
            if (tags != null) cs.tags = tags;
        }

        chaObj.name = prefab;
        chaObj.transform.position = pos;
        chaObj.transform.RotateAround(chaObj.transform.position, Vector3.up, degree);
        return chaObj;
    }
}
