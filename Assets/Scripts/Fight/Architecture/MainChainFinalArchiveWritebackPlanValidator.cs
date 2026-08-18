using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public sealed class MainChainFinalArchiveWritebackFieldCheck
{
    public string fieldName;
    public int priority;
    public string source;
    public string action;
}

public sealed class MainChainFinalArchiveWritebackPlanEntry
{
    public string skillId;
    public int supplementPriority;
    public List<string> missingFields = new List<string>();
    public List<string> dependencies = new List<string>();
    public bool canAutoFill;
    public bool meetsWriteConditions;
    public List<string> writeBlockers = new List<string>();
    public List<MainChainFinalArchiveWritebackFieldCheck> fieldFillPlan = new List<MainChainFinalArchiveWritebackFieldCheck>();
}

public sealed class MainChainFinalArchiveWritebackValidationResult
{
    public string schemaVersion = "1.0";
    public string recordType = "MainChainFinalArchiveWritebackValidation";
    public string sourceWritebackPlanPath;
    public string overallStatus;
    public bool canDirectlyWriteToGameDesignCatalog;
    public bool allEntriesMeetWriteConditions;
    public bool anyAutoFill;
    public int totalSkills;
    public int writeableSkills;
    public List<string> readyToWrite = new List<string>();
    public List<string> needsSupplement = new List<string>();
    public List<string> blocked = new List<string>();
    public List<MainChainFinalArchiveWritebackPlanEntry> entries = new List<MainChainFinalArchiveWritebackPlanEntry>();
}

public static class MainChainFinalArchiveWritebackPlanValidator
{
    public const string DefaultWritebackPlanPath = "Assets/Resources/Design/MainChainFinalArchiveWritebackPlan.json";
    public const string DefaultValidationPath = "Assets/Resources/Design/MainChainFinalArchiveWritebackValidation.json";

    private static readonly string[] ExpectedSkillIds =
    {
        "fire",
        "fireMultiple",
        "spaceMonkeyBall",
        "grenade",
        "explosiveBarrel",
        "cloakBoomerang",
        "teleportBullet"
    };

    public static MainChainFinalArchiveWritebackValidationResult ValidateFile(string planPath)
    {
        var text = File.Exists(planPath) ? File.ReadAllText(planPath, Encoding.UTF8) : string.Empty;
        var result = ValidateText(text);
        result.sourceWritebackPlanPath = planPath;
        return result;
    }

    public static MainChainFinalArchiveWritebackValidationResult ValidateText(string planJson)
    {
        var result = new MainChainFinalArchiveWritebackValidationResult
        {
            totalSkills = ExpectedSkillIds.Length
        };

        var allMeet = true;
        var anyAutoFill = false;

        for (var i = 0; i < ExpectedSkillIds.Length; i++)
        {
            var skillId = ExpectedSkillIds[i];
            var entryBlock = ExtractEntryBlock(planJson, skillId);
            var entry = new MainChainFinalArchiveWritebackPlanEntry
            {
                skillId = skillId
            };

            if (entryBlock == null)
            {
                entry.meetsWriteConditions = false;
                entry.canAutoFill = false;
                entry.writeBlockers.Add("缺少 skillId 条目");
                entry.missingFields.AddRange(GetDefaultMissingFields());
                allMeet = false;
                result.blocked.Add(skillId);
                result.entries.Add(entry);
                continue;
            }

            var missingFields = ExtractMissingFields(entryBlock);
            entry.missingFields.AddRange(missingFields);
            entry.dependencies.Add("MainChainFinalArchiveChecklistValidation.json");
            entry.dependencies.Add("GameDesignCatalogAsset");
            entry.dependencies.Add("legacy mirror static tables");

            entry.canAutoFill = false;
            entry.meetsWriteConditions = missingFields.Count == 0;
            if (entry.meetsWriteConditions)
            {
                entry.writeBlockers.Add("无阻塞项");
                result.readyToWrite.Add(skillId);
                result.writeableSkills++;
            }
            else
            {
                allMeet = false;
                entry.writeBlockers.Add("仍存在未补齐字段");
                result.needsSupplement.Add(skillId);
                result.blocked.Add(skillId);
            }

            result.entries.Add(entry);
        }

        result.anyAutoFill = anyAutoFill;
        result.allEntriesMeetWriteConditions = allMeet;
        result.canDirectlyWriteToGameDesignCatalog = allMeet && !anyAutoFill;
        result.overallStatus = result.canDirectlyWriteToGameDesignCatalog ? "通过" : "需要补充";
        return result;
    }

    public static string BuildValidationJson(MainChainFinalArchiveWritebackValidationResult result)
    {
        var sb = new StringBuilder(8192);
        AppendLine(sb, "{");
        AppendLine(sb, "  \"schemaVersion\": \"1.0\",");
        AppendLine(sb, "  \"recordType\": \"MainChainFinalArchiveWritebackValidation\",");
        AppendLine(sb, "  \"sourceWritebackPlanPath\": " + Quote(result.sourceWritebackPlanPath) + ",");
        AppendLine(sb, "  \"overallStatus\": " + Quote(result.overallStatus) + ",");
        AppendLine(sb, "  \"canDirectlyWriteToGameDesignCatalog\": " + Bool(result.canDirectlyWriteToGameDesignCatalog) + ",");
        AppendLine(sb, "  \"allEntriesMeetWriteConditions\": " + Bool(result.allEntriesMeetWriteConditions) + ",");
        AppendLine(sb, "  \"anyAutoFill\": " + Bool(result.anyAutoFill) + ",");
        AppendLine(sb, "  \"totalSkills\": " + result.totalSkills + ",");
        AppendLine(sb, "  \"writeableSkills\": " + result.writeableSkills + ",");
        AppendLine(sb, "  \"readyToWrite\": " + JsonArray(result.readyToWrite) + ",");
        AppendLine(sb, "  \"needsSupplement\": " + JsonArray(result.needsSupplement) + ",");
        AppendLine(sb, "  \"blocked\": " + JsonArray(result.blocked) + ",");
        AppendLine(sb, "  \"entries\": [");

        for (var i = 0; i < result.entries.Count; i++)
        {
            var entry = result.entries[i];
            AppendLine(sb, "    {");
            AppendLine(sb, "      \"skillId\": " + Quote(entry.skillId) + ",");
            AppendLine(sb, "      \"supplementPriority\": " + entry.supplementPriority + ",");
            AppendLine(sb, "      \"missingFields\": " + JsonArray(entry.missingFields) + ",");
            AppendLine(sb, "      \"dependencies\": " + JsonArray(entry.dependencies) + ",");
            AppendLine(sb, "      \"canAutoFill\": " + Bool(entry.canAutoFill) + ",");
            AppendLine(sb, "      \"meetsWriteConditions\": " + Bool(entry.meetsWriteConditions) + ",");
            AppendLine(sb, "      \"writeBlockers\": " + JsonArray(entry.writeBlockers) + ",");
            AppendLine(sb, "      \"fieldFillPlan\": [");

            for (var f = 0; f < entry.fieldFillPlan.Count; f++)
            {
                var field = entry.fieldFillPlan[f];
                AppendLine(sb, "        {");
                AppendLine(sb, "          \"fieldName\": " + Quote(field.fieldName) + ",");
                AppendLine(sb, "          \"priority\": " + field.priority + ",");
                AppendLine(sb, "          \"source\": " + Quote(field.source) + ",");
                AppendLine(sb, "          \"action\": " + Quote(field.action));
                AppendLine(sb, "        }" + (f < entry.fieldFillPlan.Count - 1 ? "," : string.Empty));
            }

            AppendLine(sb, "      ]");
            AppendLine(sb, "    }" + (i < result.entries.Count - 1 ? "," : string.Empty));
        }

        AppendLine(sb, "  ]");
        AppendLine(sb, "}");
        return sb.ToString();
    }

    public static void ValidateFileAndWriteReport(string planPath, string validationPath)
    {
        var result = ValidateFile(planPath);
        File.WriteAllText(validationPath, BuildValidationJson(result), new UTF8Encoding(false));
    }

    private static string[] GetDefaultMissingFields()
    {
        return new[]
        {
            "timelineId",
            "bulletId",
            "aoeId",
            "buffId",
            "targetMode",
            "cost",
            "cooldown",
            "tags",
            "notes"
        };
    }

    private static List<string> ExtractMissingFields(string entryBlock)
    {
        var missing = new List<string>();
        var fields = GetDefaultMissingFields();

        for (var i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            if (entryBlock.IndexOf("\"" + field + "\"", StringComparison.Ordinal) < 0)
            {
                missing.Add(field);
            }
        }

        return missing;
    }

    private static string ExtractEntryBlock(string json, string skillId)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        var token = "\"skillId\": \"" + skillId + "\"";
        var start = json.IndexOf(token, StringComparison.Ordinal);
        if (start < 0)
        {
            return null;
        }

        var nextStart = json.Length;
        for (var i = 0; i < ExpectedSkillIds.Length; i++)
        {
            var nextToken = "\"skillId\": \"" + ExpectedSkillIds[i] + "\"";
            if (ExpectedSkillIds[i] == skillId)
            {
                continue;
            }

            var candidate = json.IndexOf(nextToken, start + token.Length, StringComparison.Ordinal);
            if (candidate >= 0 && candidate < nextStart)
            {
                nextStart = candidate;
            }
        }

        return json.Substring(start, nextStart - start);
    }

    private static void AppendLine(StringBuilder sb, string line)
    {
        sb.AppendLine(line);
    }

    private static string Quote(string value)
    {
        if (value == null)
        {
            return "null";
        }

        return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }

    private static string Bool(bool value)
    {
        return value ? "true" : "false";
    }

    private static string JsonArray(IEnumerable<string> values)
    {
        var list = new List<string>();
        foreach (var value in values)
        {
            list.Add(Quote(value));
        }

        return "[" + string.Join(", ", list.ToArray()) + "]";
    }
}
