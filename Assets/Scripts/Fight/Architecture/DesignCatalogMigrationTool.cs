#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class DesignCatalogMigrationTool
{
    private const string ResourcesRoot = "Assets/Resources";
    private const string DesignFolder = "Assets/Resources/Design";
    private const string CatalogAssetPath = "Assets/Resources/Design/GameDesignCatalog.asset";
    private const string ConfigAssetPath = "Assets/Resources/Design/GameDesignConfig.asset";
    private const string MainChainMigrationReportPath = "Assets/Resources/Design/MainChainMigrationReport.md";

    [MenuItem("Tools/BuffTopGame/Design Catalog/Generate Or Refresh All")]
    public static void GenerateOrRefreshAll()
    {
        GameDesignCatalogAsset catalog = RefreshCatalogFromLegacyTablesInternal();
        GameDesignConfig config = RefreshConfigAsset(catalog);
        ValidateCurrentCatalog();
        CompareCurrentCatalogAgainstLegacyTables();
        GenerateMainChainMigrationReport();
        Debug.Log("[DesignCatalogMigrationTool] Full catalog generation completed for " + config.name + ".");
    }

    [MenuItem("Tools/BuffTopGame/Design Catalog/Refresh From Legacy Tables")]
    public static void RefreshCatalogFromLegacyTables()
    {
        RefreshCatalogFromLegacyTablesInternal();
    }

    [MenuItem("Tools/BuffTopGame/Design Catalog/Write Back Current Catalog")]
    public static void WriteBackCurrentCatalog()
    {
        EnsureFolder(DesignFolder);

        GameDesignConfig config = LoadOrCreateAsset<GameDesignConfig>(ConfigAssetPath);
        GameDesignCatalogAsset sourceCatalog = ResolveCurrentCatalog(config);
        if (sourceCatalog == null)
        {
            Debug.LogWarning("[DesignCatalogMigrationTool] No current catalog found. Falling back to legacy tables for the write-back snapshot.");
            sourceCatalog = GameDesignCatalogAssetUtility.CreateLegacyFullCatalog();
        }

        GameDesignCatalogAsset targetCatalog = LoadOrCreateAsset<GameDesignCatalogAsset>(CatalogAssetPath);
        CopyCatalogData(sourceCatalog, targetCatalog);
        GameDesignCatalogAssetUtility.SynchronizeMainChainMigrationData(targetCatalog);
        targetCatalog.ResetCache();
        config.designCatalog = targetCatalog;

        EditorUtility.SetDirty(targetCatalog);
        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        CatalogSnapshot sourceSnapshot = CaptureSnapshot(sourceCatalog);
        CatalogSnapshot targetSnapshot = CaptureSnapshot(targetCatalog);
        LogSnapshotComparison("WriteBack", sourceSnapshot, targetSnapshot);

        if (!AssetDatabase.Contains(sourceCatalog))
        {
            UnityEngine.Object.DestroyImmediate(sourceCatalog);
        }
    }

    [MenuItem("Tools/BuffTopGame/Design Catalog/Create Or Refresh Default Config")]
    public static void RefreshConfigAssetMenu()
    {
        GameDesignCatalogAsset catalog = LoadOrCreateAsset<GameDesignCatalogAsset>(CatalogAssetPath);
        if (IsCatalogEmpty(catalog))
        {
            GameDesignCatalogAsset legacyCatalog = GameDesignCatalogAssetUtility.CreateLegacyFullCatalog();
            CopyCatalogData(legacyCatalog, catalog);
            GameDesignCatalogAssetUtility.SynchronizeMainChainMigrationData(catalog);
            catalog.ResetCache();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEngine.Object.DestroyImmediate(legacyCatalog);
        }

        RefreshConfigAsset(catalog);
    }

    [MenuItem("Tools/BuffTopGame/Design Catalog/Compare Current Catalog Against Legacy Tables")]
    public static void CompareCurrentCatalogAgainstLegacyTables()
    {
        GameDesignConfig config = AssetDatabase.LoadAssetAtPath<GameDesignConfig>(ConfigAssetPath);
        GameDesignCatalogAsset currentCatalog = ResolveCurrentCatalog(config);
        GameDesignCatalogAsset legacyCatalog = GameDesignCatalogAssetUtility.CreateLegacyFullCatalog();

        CompareCatalogs(currentCatalog, legacyCatalog, "CurrentVsLegacy");

        UnityEngine.Object.DestroyImmediate(legacyCatalog);
    }

    [MenuItem("Tools/BuffTopGame/Design Catalog/Validate Current Catalog")]
    public static void ValidateCurrentCatalog()
    {
        GameDesignConfig config = AssetDatabase.LoadAssetAtPath<GameDesignConfig>(ConfigAssetPath);
        if (config == null)
        {
            Debug.LogWarning("[DesignCatalogMigrationTool] Missing config asset: " + ConfigAssetPath);
            return;
        }

        GameStartupValidator.Validate(config);
        LogMainChainCoverage(config.designCatalog);
    }

    [MenuItem("Tools/BuffTopGame/Design Catalog/Generate Main Chain Migration Report")]
    public static void GenerateMainChainMigrationReport()
    {
        EnsureFolder(DesignFolder);

        GameDesignConfig config = AssetDatabase.LoadAssetAtPath<GameDesignConfig>(ConfigAssetPath);
        GameDesignCatalogAsset catalog = ResolveCurrentCatalog(config);
        bool createdFallbackCatalog = false;
        if (catalog == null)
        {
            catalog = GameDesignCatalogAssetUtility.CreateLegacyFullCatalog();
            createdFallbackCatalog = true;
        }

        GameDesignCatalogAssetUtility.SynchronizeMainChainMigrationData(catalog);

        List<string> lines = new List<string>();
        lines.AddRange(GameDesignCatalogAssetUtility.BuildMainChainMigrationSchemaLines());
        lines.Add("");
        lines.AddRange(GameDesignCatalogAssetUtility.BuildMainChainMigrationChecklistLines(catalog));

        File.WriteAllText(MainChainMigrationReportPath, string.Join(Environment.NewLine, lines.ToArray()), new UTF8Encoding(false));

        if (!createdFallbackCatalog)
        {
            EditorUtility.SetDirty(catalog);
        }

        if (config != null && config.designCatalog == catalog)
        {
            EditorUtility.SetDirty(config);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (createdFallbackCatalog)
        {
            UnityEngine.Object.DestroyImmediate(catalog);
        }

        Debug.Log("[DesignCatalogMigrationTool] Wrote main chain migration report at " + MainChainMigrationReportPath);
    }

    private static GameDesignCatalogAsset RefreshCatalogFromLegacyTablesInternal()
    {
        EnsureFolder(DesignFolder);

        GameDesignCatalogAsset catalog = LoadOrCreateAsset<GameDesignCatalogAsset>(CatalogAssetPath);
        GameDesignCatalogAsset legacyCatalog = GameDesignCatalogAssetUtility.CreateLegacyFullCatalog();

        CopyCatalogData(legacyCatalog, catalog);
        GameDesignCatalogAssetUtility.SynchronizeMainChainMigrationData(catalog);
        catalog.ResetCache();

        CatalogSnapshot sourceSnapshot = CaptureSnapshot(legacyCatalog);
        CatalogSnapshot targetSnapshot = CaptureSnapshot(catalog);
        LogSnapshotComparison("RefreshFromLegacy", sourceSnapshot, targetSnapshot);

        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        UnityEngine.Object.DestroyImmediate(legacyCatalog);
        Debug.Log("[DesignCatalogMigrationTool] Refreshed catalog from legacy tables at " + CatalogAssetPath);
        return catalog;
    }

    private static GameDesignConfig RefreshConfigAsset(GameDesignCatalogAsset catalog)
    {
        EnsureFolder(DesignFolder);

        GameDesignConfig config = LoadOrCreateAsset<GameDesignConfig>(ConfigAssetPath);
        config.designCatalog = catalog;
        EditorUtility.SetDirty(config);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[DesignCatalogMigrationTool] Refreshed default config at " + ConfigAssetPath);
        return config;
    }

    private static void CompareCatalogs(GameDesignCatalogAsset currentCatalog, GameDesignCatalogAsset legacyCatalog, string label)
    {
        CatalogSnapshot currentSnapshot = CaptureSnapshot(currentCatalog);
        CatalogSnapshot legacySnapshot = CaptureSnapshot(legacyCatalog);
        LogSnapshotComparison(label, legacySnapshot, currentSnapshot);
        LogMainChainCoverage(currentCatalog);
    }

    private static void LogSnapshotComparison(string label, CatalogSnapshot source, CatalogSnapshot target)
    {
        if (source == null || target == null)
        {
            Debug.LogWarning("[DesignCatalogMigrationTool][" + label + "] Comparison skipped because one snapshot is missing.");
            return;
        }

        if (string.Equals(source.fingerprint, target.fingerprint, StringComparison.Ordinal))
        {
            Debug.Log("[DesignCatalogMigrationTool][" + label + "] Catalog fingerprints match. Idempotency confirmed: " + target.fingerprint);
            return;
        }

        Debug.LogWarning("[DesignCatalogMigrationTool][" + label + "] Catalog fingerprints differ.");
        Debug.LogWarning("[DesignCatalogMigrationTool][" + label + "] Source: " + source.Describe());
        Debug.LogWarning("[DesignCatalogMigrationTool][" + label + "] Target: " + target.Describe());
        LogMissingAndExtraEntries("skills", source.skillIds, target.skillIds);
        LogMissingAndExtraEntries("timelines", source.timelineIds, target.timelineIds);
        LogMissingAndExtraEntries("buffs", source.buffIds, target.buffIds);
        LogMissingAndExtraEntries("bullets", source.bulletIds, target.bulletIds);
        LogMissingAndExtraEntries("aoes", source.aoeIds, target.aoeIds);
        LogMissingAndExtraEntries("mainChainSkills", source.mainChainSkillIds, target.mainChainSkillIds);
        LogMissingAndExtraEntries("mainChainSchema", source.mainChainSchemaNames, target.mainChainSchemaNames);
    }

    private static void LogMissingAndExtraEntries(string category, List<string> sourceIds, List<string> targetIds)
    {
        HashSet<string> sourceSet = BuildSet(sourceIds);
        HashSet<string> targetSet = BuildSet(targetIds);

        List<string> missingFromTarget = new List<string>();
        foreach (string id in sourceSet)
        {
            if (!targetSet.Contains(id))
            {
                missingFromTarget.Add(id);
            }
        }

        List<string> extraInTarget = new List<string>();
        foreach (string id in targetSet)
        {
            if (!sourceSet.Contains(id))
            {
                extraInTarget.Add(id);
            }
        }

        if (missingFromTarget.Count == 0 && extraInTarget.Count == 0)
        {
            return;
        }

        if (missingFromTarget.Count > 0)
        {
            Debug.LogWarning("[DesignCatalogMigrationTool][" + category + "] Missing in target: " + JoinIds(missingFromTarget));
        }

        if (extraInTarget.Count > 0)
        {
            Debug.LogWarning("[DesignCatalogMigrationTool][" + category + "] Extra in target: " + JoinIds(extraInTarget));
        }
    }

    private static HashSet<string> BuildSet(List<string> values)
    {
        HashSet<string> set = new HashSet<string>();
        if (values == null)
        {
            return set;
        }

        for (int i = 0; i < values.Count; i++)
        {
            if (!string.IsNullOrEmpty(values[i]))
            {
                set.Add(values[i]);
            }
        }

        return set;
    }

    private static string JoinIds(List<string> ids)
    {
        if (ids == null || ids.Count <= 0)
        {
            return "<empty>";
        }

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < ids.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }
            builder.Append(ids[i]);
        }

        return builder.ToString();
    }

    private static void LogMainChainCoverage(GameDesignCatalogAsset catalog)
    {
        if (catalog == null)
        {
            Debug.LogWarning("[DesignCatalogMigrationTool] Main chain check skipped because the catalog is missing.");
            return;
        }

        int missingCount = 0;
        for (int i = 0; i < GameDesignCatalogAssetUtility.MainChainSkillIds.Length; i++)
        {
            string skillId = GameDesignCatalogAssetUtility.MainChainSkillIds[i];
            if (!catalog.HasSkill(skillId))
            {
                Debug.LogWarning("[DesignCatalogMigrationTool][MainChain] Missing skill: " + skillId);
                missingCount++;
            }
        }

        if (missingCount == 0)
        {
            Debug.Log("[DesignCatalogMigrationTool][MainChain] All primary skills are present.");
        }
    }

    private static CatalogSnapshot CaptureSnapshot(GameDesignCatalogAsset catalog)
    {
        if (catalog == null)
        {
            return null;
        }

        CatalogSnapshot snapshot = new CatalogSnapshot();
        snapshot.skillCount = catalog.skills != null ? catalog.skills.Length : 0;
        snapshot.timelineCount = catalog.timelines != null ? catalog.timelines.Length : 0;
        snapshot.buffCount = catalog.buffs != null ? catalog.buffs.Length : 0;
        snapshot.bulletCount = catalog.bullets != null ? catalog.bullets.Length : 0;
        snapshot.aoeCount = catalog.aoes != null ? catalog.aoes.Length : 0;
        snapshot.mainChainEntryCount = catalog.mainChainMigrationEntries != null ? catalog.mainChainMigrationEntries.Length : 0;
        snapshot.mainChainSchemaCount = catalog.mainChainMigrationSchema != null ? catalog.mainChainMigrationSchema.Length : 0;
        snapshot.skillIds = ExtractSortedIds(catalog.skills, delegate(SkillDefinition item) { return item.id; }, delegate(SkillDefinition item) { return EditorJsonUtility.ToJson(item, true); });
        snapshot.timelineIds = ExtractSortedIds(catalog.timelines, delegate(TimelineDefinition item) { return item.id; }, delegate(TimelineDefinition item) { return EditorJsonUtility.ToJson(item, true); });
        snapshot.buffIds = ExtractSortedIds(catalog.buffs, delegate(BuffDefinition item) { return item.id; }, delegate(BuffDefinition item) { return EditorJsonUtility.ToJson(item, true); });
        snapshot.bulletIds = ExtractSortedIds(catalog.bullets, delegate(BulletDefinition item) { return item.id; }, delegate(BulletDefinition item) { return EditorJsonUtility.ToJson(item, true); });
        snapshot.aoeIds = ExtractSortedIds(catalog.aoes, delegate(AoeDefinition item) { return item.id; }, delegate(AoeDefinition item) { return EditorJsonUtility.ToJson(item, true); });
        snapshot.mainChainSkillIds = ExtractSortedIds(catalog.mainChainMigrationEntries, delegate(MainChainMigrationEntry item) { return item.skillId; }, delegate(MainChainMigrationEntry item) { return EditorJsonUtility.ToJson(item, true); });
        snapshot.mainChainSchemaNames = ExtractSortedIds(catalog.mainChainMigrationSchema, delegate(MainChainMigrationSchemaRow item) { return item.name; }, delegate(MainChainMigrationSchemaRow item) { return EditorJsonUtility.ToJson(item, true); });
        snapshot.fingerprint = ComputeFingerprint(snapshot.BuildCanonicalText());
        return snapshot;
    }

    private static List<string> ExtractSortedIds<T>(T[] values, Func<T, string> idSelector, Func<T, string> payloadSelector)
    {
        List<string> entries = new List<string>();
        if (values == null || values.Length <= 0)
        {
            return entries;
        }

        for (int i = 0; i < values.Length; i++)
        {
            T item = values[i];
            if (ReferenceEquals(item, null))
            {
                entries.Add("#" + i.ToString() + "=");
                continue;
            }

            string id = idSelector != null ? idSelector(item) : "";
            string payload = payloadSelector != null ? payloadSelector(item) : "";
            entries.Add((string.IsNullOrEmpty(id) ? "#" + i.ToString() : id) + "=" + payload);
        }

        entries.Sort(StringComparer.Ordinal);
        return entries;
    }

    private static string ComputeFingerprint(string text)
    {
        using (SHA1 sha1 = SHA1.Create())
        {
            byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(text ?? string.Empty));
            StringBuilder builder = new StringBuilder(hash.Length * 2);
            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }

    private static void CopyCatalogData(GameDesignCatalogAsset source, GameDesignCatalogAsset target)
    {
        if (source == null || target == null)
        {
            return;
        }

        EditorUtility.CopySerialized(source, target);
    }

    private static bool IsCatalogEmpty(GameDesignCatalogAsset catalog)
    {
        if (catalog == null)
        {
            return true;
        }

        return (catalog.skills == null || catalog.skills.Length <= 0)
            && (catalog.timelines == null || catalog.timelines.Length <= 0)
            && (catalog.buffs == null || catalog.buffs.Length <= 0)
            && (catalog.bullets == null || catalog.bullets.Length <= 0)
            && (catalog.aoes == null || catalog.aoes.Length <= 0);
    }

    private static GameDesignCatalogAsset ResolveCurrentCatalog(GameDesignConfig config)
    {
        if (config != null && config.designCatalog != null)
        {
            return config.designCatalog;
        }

        GameDesignCatalogAsset catalog = AssetDatabase.LoadAssetAtPath<GameDesignCatalogAsset>(CatalogAssetPath);
        if (catalog != null)
        {
            return catalog;
        }

        return null;
    }

    private static void EnsureFolder(string folderPath)
    {
        if (!AssetDatabase.IsValidFolder(ResourcesRoot))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder(ResourcesRoot, "Design");
        }
    }

    private static T LoadOrCreateAsset<T>(string assetPath) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        if (asset != null)
        {
            return asset;
        }

        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, assetPath);
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private sealed class CatalogSnapshot
    {
        public int skillCount;
        public int timelineCount;
        public int buffCount;
        public int bulletCount;
        public int aoeCount;
        public int mainChainEntryCount;
        public int mainChainSchemaCount;
        public List<string> skillIds;
        public List<string> timelineIds;
        public List<string> buffIds;
        public List<string> bulletIds;
        public List<string> aoeIds;
        public List<string> mainChainSkillIds;
        public List<string> mainChainSchemaNames;
        public string fingerprint;

        public string BuildCanonicalText()
        {
            StringBuilder builder = new StringBuilder();
            AppendSection(builder, "skills", skillCount, skillIds);
            AppendSection(builder, "timelines", timelineCount, timelineIds);
            AppendSection(builder, "buffs", buffCount, buffIds);
            AppendSection(builder, "bullets", bulletCount, bulletIds);
            AppendSection(builder, "aoes", aoeCount, aoeIds);
            AppendSection(builder, "mainChainEntries", mainChainEntryCount, mainChainSkillIds);
            AppendSection(builder, "mainChainSchema", mainChainSchemaCount, mainChainSchemaNames);
            return builder.ToString();
        }

        public string Describe()
        {
            return string.Format("skills={0}, timelines={1}, buffs={2}, bullets={3}, aoes={4}, mainChainEntries={5}, mainChainSchema={6}, fingerprint={7}", skillCount, timelineCount, buffCount, bulletCount, aoeCount, mainChainEntryCount, mainChainSchemaCount, fingerprint);
        }

        private static void AppendSection(StringBuilder builder, string label, int count, List<string> entries)
        {
            builder.Append(label).Append(':').Append(count).Append('\n');
            if (entries == null)
            {
                return;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                builder.Append(entries[i]).Append('\n');
            }
        }
    }
}
#endif
