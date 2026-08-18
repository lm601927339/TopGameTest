using System.Collections.Generic;
using UnityEngine;

public static class GameRuntime
{
    private static GameManager gameManager;
    private static TimelineManager timelineManager;
    private static DamageManager damageManager;
    private static EffectManager effectManager;
    private static PopTextManager popTextManager;
    private static Camera mainCamera;
    private static GameObject mainActor;
    private static GameObject rootObject;
    private static GameDesignConfig config;
    private static readonly Dictionary<string, GameObject> sceneObjectCache = new Dictionary<string, GameObject>();
    private static readonly Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();

    public static GameDesignConfig Config
    {
        get
        {
            if (config == null)
            {
                config = CreateDefaultConfigAsset();
            }
            return config;
        }
    }

    public static void Initialize(GameManager bootstrap, GameDesignConfig designConfig)
    {
        gameManager = bootstrap;
        config = designConfig != null ? designConfig : CreateDefaultConfigAsset();
        EnsureCatalogAsset(config);
        sceneObjectCache.Clear();
        prefabCache.Clear();
        rootObject = null;
        mainActor = null;
    }

    public static void SetMainActor(GameObject actor)
    {
        mainActor = actor;
    }

    public static void SetRoot(GameObject root)
    {
        rootObject = root;
    }

    public static GameManager Manager
    {
        get
        {
            if (!gameManager)
            {
                gameManager = Object.FindObjectOfType<GameManager>();
            }
            return gameManager;
        }
    }

    public static TimelineManager Timeline
    {
        get
        {
            if (!timelineManager)
            {
                timelineManager = Object.FindObjectOfType<TimelineManager>();
            }
            return timelineManager;
        }
    }

    public static DamageManager Damage
    {
        get
        {
            if (!damageManager)
            {
                damageManager = Object.FindObjectOfType<DamageManager>();
            }
            return damageManager;
        }
    }

    public static EffectManager Effect
    {
        get
        {
            if (!effectManager)
            {
                effectManager = Object.FindObjectOfType<EffectManager>();
            }
            return effectManager;
        }
    }

    public static PopTextManager PopText
    {
        get
        {
            if (!popTextManager)
            {
                popTextManager = Object.FindObjectOfType<PopTextManager>();
            }
            return popTextManager;
        }
    }

    public static Camera MainCamera
    {
        get
        {
            if (!mainCamera)
            {
                mainCamera = Camera.main;
                if (!mainCamera)
                {
                    mainCamera = Object.FindObjectOfType<Camera>();
                }
            }
            return mainCamera;
        }
    }

    public static GameObject MainActor
    {
        get
        {
            if (!mainActor && Manager)
            {
                mainActor = Manager.mainActor;
            }
            return mainActor;
        }
    }

    public static ChaState MainActorState
    {
        get
        {
            GameObject actor = MainActor;
            return actor ? actor.GetComponent<ChaState>() : null;
        }
    }

    public static GameObject RootObject
    {
        get
        {
            if (!rootObject && !string.IsNullOrEmpty(Config.rootObjectName))
            {
                rootObject = FindSceneObject(Config.rootObjectName);
            }
            return rootObject;
        }
    }

    public static Transform RootTransform
    {
        get
        {
            return RootObject ? RootObject.transform : null;
        }
    }

    public static string[] GetSkillIds()
    {
        return GameDatabase.GetSkillIds();
    }

    public static GameObject[] GetCharactersBySide(int side, bool aliveOnly = true)
    {
        if (string.IsNullOrEmpty(Config.characterTag))
        {
            return new GameObject[0];
        }

        GameObject[] characters = GameObject.FindGameObjectsWithTag(Config.characterTag);
        List<GameObject> matches = new List<GameObject>();
        for (int i = 0; i < characters.Length; i++)
        {
            GameObject character = characters[i];
            if (!character)
            {
                continue;
            }

            ChaState chaState = character.GetComponent<ChaState>();
            if (!chaState || chaState.side != side)
            {
                continue;
            }

            if (aliveOnly && chaState.dead)
            {
                continue;
            }

            matches.Add(character);
        }

        return matches.ToArray();
    }

    public static int CountCharactersBySide(int side, bool aliveOnly = true)
    {
        return GetCharactersBySide(side, aliveOnly).Length;
    }

    public static int ClearCharactersBySide(int side)
    {
        GameObject[] characters = GetCharactersBySide(side, false);
        Debug.Log("[GameRuntime] Clearing characters by side=" + side + " count=" + characters.Length);
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i])
            {
                Object.Destroy(characters[i]);
            }
        }

        return characters.Length;
    }

    public static GameObject SpawnEnemy()
    {
        return SpawnEnemy(GetEnemySpawnPosition(), 0f);
    }

    public static GameObject SpawnEnemy(Vector3 position, float degree = 0f)
    {
        GameDesignConfig cfg = Config;
        int spawnedCount = CountCharactersBySide(cfg.enemySide, true);
        Debug.Log("[GameRuntime] SpawnEnemy side=" + cfg.enemySide + " count=" + spawnedCount + " position=" + position);
        GameObject enemy = SceneVariants.CreateCharacter(
            cfg.enemyPrefabPath,
            cfg.enemySide,
            position,
            cfg.BuildEnemyProperty(spawnedCount),
            degree,
            cfg.enemyViewAnimInfo
        );

        if (enemy)
        {
            enemy.AddComponent<SimpleAI>();
        }

        return enemy;
    }

    public static GameObject FindSceneObject(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
        {
            return null;
        }

        if (sceneObjectCache.TryGetValue(objectName, out GameObject cached) && cached)
        {
            return cached;
        }

        GameObject found = GameObject.Find(objectName);
        sceneObjectCache[objectName] = found;
        return found;
    }

    private static Vector3 GetEnemySpawnPosition()
    {
        if (SceneVariants.map != null)
        {
            return SceneVariants.map.GetRandomPosForCharacter(
                new RectInt(0, 0, SceneVariants.map.MapWidth(), SceneVariants.map.MapHeight())
            );
        }

        GameObject actor = MainActor;
        if (actor)
        {
            return actor.transform.position + new Vector3(1f, 0f, 0f);
        }

        return Vector3.zero;
    }

    public static GameObject LoadPrefab(string prefabPath)
    {
        GameObject prefab;
        if (TryLoadPrefab(prefabPath, out prefab))
        {
            return prefab;
        }

        Debug.LogError("Prefab not found in Resources: " + prefabPath);
        return null;
    }

    public static bool HasPrefab(string prefabPath)
    {
        GameObject prefab;
        return TryLoadPrefab(prefabPath, out prefab);
    }

    private static bool TryLoadPrefab(string prefabPath, out GameObject prefab)
    {
        prefab = null;
        if (string.IsNullOrEmpty(prefabPath))
        {
            return false;
        }

        if (prefabCache.TryGetValue(prefabPath, out GameObject cached) && cached)
        {
            prefab = cached;
            return true;
        }

        string[] candidates = BuildPrefabCandidates(prefabPath);
        for (int i = 0; i < candidates.Length; i++)
        {
            GameObject loaded = Resources.Load<GameObject>(candidates[i]);
            if (loaded)
            {
                prefabCache[prefabPath] = loaded;
                prefabCache[candidates[i]] = loaded;
                prefab = loaded;
                return true;
            }
        }

        return false;
    }

    private static string[] BuildPrefabCandidates(string prefabPath)
    {
        List<string> candidates = new List<string>();
        candidates.Add(prefabPath);

        if (!prefabPath.StartsWith("Prefab/") && !prefabPath.StartsWith("Prefabs/"))
        {
            candidates.Add("Prefab/" + prefabPath);
            candidates.Add("Prefabs/" + prefabPath);
        }

        return candidates.ToArray();
    }

    private static GameDesignConfig CreateDefaultConfigAsset()
    {
        GameDesignConfig loadedConfig = LoadDefaultConfigAsset();
        if (loadedConfig != null)
        {
            EnsureCatalogAsset(loadedConfig);
            return loadedConfig;
        }

        GameDesignConfig createdConfig = ScriptableObject.CreateInstance<GameDesignConfig>();
        EnsureCatalogAsset(createdConfig);
        return createdConfig;
    }

    private static void EnsureCatalogAsset(GameDesignConfig targetConfig)
    {
        if (targetConfig == null || targetConfig.designCatalog != null)
        {
            return;
        }

        GameDesignCatalogAsset loadedCatalog = LoadDefaultCatalogAsset();
        if (loadedCatalog != null)
        {
            targetConfig.designCatalog = loadedCatalog;
            return;
        }

        // Migration bridge: if the scene has not assigned an authoring asset yet, seed the
        // catalog with runtime mirrors of the legacy bullet/aoe tables so the new data path is
        // immediately usable without changing existing content.
        targetConfig.designCatalog = GameDesignCatalogAssetUtility.CreateLegacyBulletAoeCatalog();
    }

    private static GameDesignConfig LoadDefaultConfigAsset()
    {
        GameDesignConfig loaded = Resources.Load<GameDesignConfig>("Design/GameDesignConfig");
        return loaded;
    }

    private static GameDesignCatalogAsset LoadDefaultCatalogAsset()
    {
        GameDesignCatalogAsset loaded = Resources.Load<GameDesignCatalogAsset>("Design/GameDesignCatalog");
        return loaded;
    }
}
