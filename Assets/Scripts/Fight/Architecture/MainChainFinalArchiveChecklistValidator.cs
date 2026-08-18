using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public sealed class MainChainFinalArchiveChecklistFieldCheck
{
    public string fieldName;
    public bool present;
    public bool filled;
    public string sourceLabel;
    public string valueState;
}

public sealed class MainChainFinalArchiveChecklistValidationEntry
{
    public string skillId;
    public string finalAdvice;
    public string sourceLabel;
    public string diffSummary;
    public string blockingReason;
    public bool directlyWritableToGameDesignCatalog;
    public List<MainChainFinalArchiveChecklistFieldCheck> fieldChecks = new List<MainChainFinalArchiveChecklistFieldCheck>();
}

public sealed class MainChainFinalArchiveChecklistValidationResult
{
    public string schemaVersion = "1.0";
    public string recordType = "MainChainFinalArchiveChecklistValidation";
    public string sourceChecklistPath;
    public string overallStatus;
    public bool coverageComplete;
    public bool fieldKeysComplete;
    public bool directlyWritableToGameDesignCatalog;
    public int expectedSkillCount;
    public int presentSkillCount;
    public List<string> readyToWrite = new List<string>();
    public List<string> needsSupplement = new List<string>();
    public List<string> blocked = new List<string>();
    public List<string> missingSkillIds = new List<string>();
    public List<string> unexpectedSkillIds = new List<string>();
    public List<MainChainFinalArchiveChecklistValidationEntry> entries = new List<MainChainFinalArchiveChecklistValidationEntry>();
}

public static class MainChainFinalArchiveChecklistValidator
{
    public const string DefaultChecklistPath = "Assets/Resources/Design/MainChainFinalArchiveChecklist.json";
    public const string DefaultValidationReportPath = "Assets/Resources/Design/MainChainFinalArchiveChecklistValidation.json";

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

    private static readonly string[] RequiredFields =
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

    public static MainChainFinalArchiveChecklistValidationResult ValidateFile(string checklistPath)
    {
        var text = File.Exists(checklistPath) ? File.ReadAllText(checklistPath, Encoding.UTF8) : string.Empty;
        var result = ValidateText(text);
        result.sourceChecklistPath = checklistPath;
        return result;
    }

    public static void ValidateFileAndWriteReport(string checklistPath, string reportPath)
    {
        var result = ValidateFile(checklistPath);
        var report = BuildValidationJson(result);
        File.WriteAllText(reportPath, report, new UTF8Encoding(false));
    }

    public static MainChainFinalArchiveChecklistValidationResult ValidateText(string checklistJson)
    {
        var result = new MainChainFinalArchiveChecklistValidationResult
        {
            expectedSkillCount = ExpectedSkillIds.Length,
            presentSkillCount = 0,
            coverageComplete = true,
            fieldKeysComplete = true,
            directlyWritableToGameDesignCatalog = true
        };

        var discoveredSkillIds = new List<string>();
        for (var i = 0; i < ExpectedSkillIds.Length; i++)
        {
            var skillId = ExpectedSkillIds[i];
            var entryBlock = ExtractEntryBlock(checklistJson, skillId);
            var entry = new MainChainFinalArchiveChecklistValidationEntry
            {
                skillId = skillId,
                sourceLabel = "template"
            };

            if (entryBlock == null)
            {
                entry.finalAdvice = "阻塞";
                entry.diffSummary = "缺少 skillId 条目";
                entry.blockingReason = "最终归档清单未包含该主链路技能";
                entry.directlyWritableToGameDesignCatalog = false;
                result.coverageComplete = false;
                result.fieldKeysComplete = false;
                result.directlyWritableToGameDesignCatalog = false;
                result.blocked.Add(skillId);
                result.missingSkillIds.Add(skillId);
                result.entries.Add(entry);
                continue;
            }

            discoveredSkillIds.Add(skillId);
            result.presentSkillCount++;

            var missingFields = new List<string>();
            var filledFields = new List<string>();

            for (var f = 0; f < RequiredFields.Length; f++)
            {
                var fieldName = RequiredFields[f];
                var present = ContainsFieldKey(entryBlock, fieldName);
                var filled = IsFilledField(entryBlock, fieldName);
                var valueState = DetermineValueState(entryBlock, fieldName);

                entry.fieldChecks.Add(new MainChainFinalArchiveChecklistFieldCheck
                {
                    fieldName = fieldName,
                    present = present,
                    filled = filled,
                    sourceLabel = "template",
                    valueState = valueState
                });

                if (!present)
                {
                    missingFields.Add(fieldName);
                    result.fieldKeysComplete = false;
                }

                if (filled)
                {
                    filledFields.Add(fieldName);
                }
            }

            if (missingFields.Count > 0)
            {
                entry.finalAdvice = "阻塞";
                entry.diffSummary = "缺少字段: " + string.Join(", ", missingFields);
                entry.blockingReason = "字段不完整，不能生成可落盘资产";
                entry.directlyWritableToGameDesignCatalog = false;
                result.directlyWritableToGameDesignCatalog = false;
                result.blocked.Add(skillId);
            }
            else if (filledFields.Count < RequiredFields.Length)
            {
                entry.finalAdvice = "需要补充";
                entry.diffSummary = "字段齐全，但仍为模板占位";
                entry.blockingReason = "需要补齐实际目录值后方可落盘";
                entry.directlyWritableToGameDesignCatalog = false;
                result.directlyWritableToGameDesignCatalog = false;
                result.needsSupplement.Add(skillId);
            }
            else
            {
                entry.finalAdvice = "通过";
                entry.diffSummary = "字段已填充且结构完整";
                entry.blockingReason = string.Empty;
                entry.directlyWritableToGameDesignCatalog = true;
                result.readyToWrite.Add(skillId);
            }

            result.entries.Add(entry);
        }

        result.overallStatus = result.directlyWritableToGameDesignCatalog
            ? "通过"
            : (result.coverageComplete && result.fieldKeysComplete ? "需要补充" : "阻塞");

        result.unexpectedSkillIds = FindUnexpectedSkillIds(checklistJson, discoveredSkillIds);
        if (result.unexpectedSkillIds.Count > 0)
        {
            result.coverageComplete = false;
            result.overallStatus = "阻塞";
        }

        return result;
    }

    public static string BuildValidationJson(MainChainFinalArchiveChecklistValidationResult result)
    {
        var sb = new StringBuilder(8192);
        AppendLine(sb, "{");
        AppendLine(sb, "  \"schemaVersion\": \"1.0\",");
        AppendLine(sb, "  \"recordType\": \"MainChainFinalArchiveChecklistValidation\",");
        AppendLine(sb, "  \"sourceChecklistPath\": " + Quote(result.sourceChecklistPath) + ",");
        AppendLine(sb, "  \"overallStatus\": " + Quote(result.overallStatus) + ",");
        AppendLine(sb, "  \"coverageComplete\": " + Bool(result.coverageComplete) + ",");
        AppendLine(sb, "  \"fieldKeysComplete\": " + Bool(result.fieldKeysComplete) + ",");
        AppendLine(sb, "  \"directlyWritableToGameDesignCatalog\": " + Bool(result.directlyWritableToGameDesignCatalog) + ",");
        AppendLine(sb, "  \"expectedSkillCount\": " + result.expectedSkillCount + ",");
        AppendLine(sb, "  \"presentSkillCount\": " + result.presentSkillCount + ",");
        AppendLine(sb, "  \"missingSkillIds\": " + JsonArray(result.missingSkillIds) + ",");
        AppendLine(sb, "  \"unexpectedSkillIds\": " + JsonArray(result.unexpectedSkillIds) + ",");
        AppendLine(sb, "  \"summary\": {");
        AppendLine(sb, "    \"readyToWrite\": " + JsonArray(result.readyToWrite) + ",");
        AppendLine(sb, "    \"needsSupplement\": " + JsonArray(result.needsSupplement) + ",");
        AppendLine(sb, "    \"blocked\": " + JsonArray(result.blocked));
        AppendLine(sb, "  },");
        AppendLine(sb, "  \"entries\": [");

        for (var i = 0; i < result.entries.Count; i++)
        {
            var entry = result.entries[i];
            AppendLine(sb, "    {");
            AppendLine(sb, "      \"skillId\": " + Quote(entry.skillId) + ",");
            AppendLine(sb, "      \"finalAdvice\": " + Quote(entry.finalAdvice) + ",");
            AppendLine(sb, "      \"sourceLabel\": " + Quote(entry.sourceLabel) + ",");
            AppendLine(sb, "      \"diffSummary\": " + Quote(entry.diffSummary) + ",");
            AppendLine(sb, "      \"blockingReason\": " + Quote(entry.blockingReason) + ",");
            AppendLine(sb, "      \"directlyWritableToGameDesignCatalog\": " + Bool(entry.directlyWritableToGameDesignCatalog) + ",");
            AppendLine(sb, "      \"fieldChecks\": [");

            for (var f = 0; f < entry.fieldChecks.Count; f++)
            {
                var field = entry.fieldChecks[f];
                AppendLine(sb, "        {");
                AppendLine(sb, "          \"fieldName\": " + Quote(field.fieldName) + ",");
                AppendLine(sb, "          \"present\": " + Bool(field.present) + ",");
                AppendLine(sb, "          \"filled\": " + Bool(field.filled) + ",");
                AppendLine(sb, "          \"sourceLabel\": " + Quote(field.sourceLabel) + ",");
                AppendLine(sb, "          \"valueState\": " + Quote(field.valueState));
                AppendLine(sb, "        }" + (f < entry.fieldChecks.Count - 1 ? "," : string.Empty));
            }

            AppendLine(sb, "      ]");
            AppendLine(sb, "    }" + (i < result.entries.Count - 1 ? "," : string.Empty));
        }

        AppendLine(sb, "  ]");
        AppendLine(sb, "}");
        return sb.ToString();
    }

    private static List<string> FindUnexpectedSkillIds(string checklistJson, List<string> discoveredSkillIds)
    {
        var unexpected = new List<string>();
        var token = "\"skillId\": \"";
        var index = 0;

        while (index >= 0)
        {
            index = checklistJson.IndexOf(token, index, StringComparison.Ordinal);
            if (index < 0)
            {
                break;
            }

            index += token.Length;
            var end = checklistJson.IndexOf('"', index);
            if (end < 0)
            {
                break;
            }

            var skillId = checklistJson.Substring(index, end - index);
            if (ExpectedSkillIds.Contains(skillId) == false && unexpected.Contains(skillId) == false)
            {
                unexpected.Add(skillId);
            }

            index = end + 1;
        }

        return unexpected;
    }

    private static string ExtractEntryBlock(string checklistJson, string skillId)
    {
        if (string.IsNullOrEmpty(checklistJson))
        {
            return null;
        }

        var token = "\"skillId\": \"" + skillId + "\"";
        var start = checklistJson.IndexOf(token, StringComparison.Ordinal);
        if (start < 0)
        {
            return null;
        }

        var nextStart = checklistJson.Length;
        for (var i = 0; i < ExpectedSkillIds.Length; i++)
        {
            if (ExpectedSkillIds[i] == skillId)
            {
                continue;
            }

            var nextToken = "\"skillId\": \"" + ExpectedSkillIds[i] + "\"";
            var candidate = checklistJson.IndexOf(nextToken, start + token.Length, StringComparison.Ordinal);
            if (candidate >= 0 && candidate < nextStart)
            {
                nextStart = candidate;
            }
        }

        return checklistJson.Substring(start, nextStart - start);
    }

    private static bool ContainsFieldKey(string entryBlock, string fieldName)
    {
        return entryBlock.IndexOf("\"" + fieldName + "\"", StringComparison.Ordinal) >= 0;
    }

    private static bool IsFilledField(string entryBlock, string fieldName)
    {
        var nullPattern = "\"" + fieldName + "\"\\s*:\\s*null";
        var emptyArrayPattern = "\"" + fieldName + "\"\\s*:\\s*\\[\\s*\\]";
        var emptyStringPattern = "\"" + fieldName + "\"\\s*:\\s*\"\"";

        var hasNull = System.Text.RegularExpressions.Regex.IsMatch(entryBlock, nullPattern);
        var hasEmptyArray = System.Text.RegularExpressions.Regex.IsMatch(entryBlock, emptyArrayPattern);
        var hasEmptyString = System.Text.RegularExpressions.Regex.IsMatch(entryBlock, emptyStringPattern);
        return !(hasNull || hasEmptyArray || hasEmptyString);
    }

    private static string DetermineValueState(string entryBlock, string fieldName)
    {
        if (!ContainsFieldKey(entryBlock, fieldName))
        {
            return "missing";
        }

        return IsFilledField(entryBlock, fieldName) ? "filled" : "placeholder";
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
        return "[" + string.Join(", ", values.Select(Quote)) + "]";
    }

    private static void AppendLine(StringBuilder sb, string line)
    {
        sb.AppendLine(line);
    }
}
