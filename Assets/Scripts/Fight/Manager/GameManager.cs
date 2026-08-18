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

    private GameDesignConfig Config
    {
        get
        {
            return designConfig != null ? designConfig : GameRuntime.Config;
        }
    }

    private void Awake()
    {
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
        mainCharacter = CreateCharacter(
            Config.playerPrefabPath,
            Config.playerSide,
            new Vector3(),
            Config.BuildPlayerProperty(),
            0,
            Config.playerViewAnimInfo
        );

        if (mainCharacter)
        {
            mainCharacter.AddComponent<PlayerController>().mainCamera = GameRuntime.MainCamera;
            mainCharacter.transform.position = playerPos;
            GameRuntime.SetMainActor(mainCharacter);
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
        if (mcs)
        {
            for (int i = 0; i < Config.startingSkills.Length; i++)
            {
                string skillId = Config.startingSkills[i];
                SkillModel skillModel;
                if (GameDatabase.TryGetSkill(skillId, out skillModel))
                {
                    mcs.LearnSkill(skillModel);
                }
            }
        }
    }

    void Update()
    {
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
