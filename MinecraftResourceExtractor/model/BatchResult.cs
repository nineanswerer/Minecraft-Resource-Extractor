using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace mre.model
{
    /// <summary>
    /// 批量提取结果数据模型，包含统计信息和冲突检测结果
    /// </summary>
    public class BatchResult
    {
        [JsonProperty("output_directory")]
        public string OutputDirectory { get; set; }

        [JsonProperty("total_jars")]
        public int TotalJars { get; set; }

        [JsonProperty("successful_jars")]
        public int SuccessfulJars { get; set; }

        [JsonProperty("failed_jars")]
        public int FailedJars { get; set; }

        [JsonProperty("total_assets_extracted")]
        public int TotalAssetsExtracted { get; set; }

        [JsonProperty("conflict_count")]
        public int ConflictCount { get; set; }

        [JsonProperty("conflicts")]
        public List<ConflictEntry> Conflicts { get; set; }

        [JsonProperty("failed_jar_list")]
        public List<string> FailedJarList { get; set; }

        public BatchResult()
        {
            Conflicts = new List<ConflictEntry>();
            FailedJarList = new List<string>();
        }

        /// <summary>
        /// 保存冲突报告为 JSON 文件
        /// </summary>
        public void SaveToFile(string outputDir)
        {
            string reportPath = Path.Combine(outputDir, "conflict_report.json");
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(reportPath, json, Encoding.UTF8);
        }

        /// <summary>
        /// 生成人类可读的摘要文本（用于日志面板显示）
        /// </summary>
        public string ToSummaryString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine("  批量提取完成");
            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine("  处理 Jar 总数: " + TotalJars);
            sb.AppendLine("  成功提取: " + SuccessfulJars);
            sb.AppendLine("  提取失败: " + FailedJars);
            sb.AppendLine("  提取资产总数: " + TotalAssetsExtracted);
            sb.AppendLine("  资源冲突数: " + ConflictCount);

            if (FailedJarList.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("  失败的 Jar 文件:");
                foreach (var jar in FailedJarList)
                    sb.AppendLine("    ✗ " + jar);
            }

            if (Conflicts.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("  资源冲突详情:");
                // 最多显示前 20 条冲突，避免日志过长
                int displayCount = Conflicts.Count > 20 ? 20 : Conflicts.Count;
                for (int i = 0; i < displayCount; i++)
                {
                    var entry = Conflicts[i];
                    sb.AppendLine("    ⚠ " + entry.AssetPath);
                    sb.AppendLine("      涉及 Mod: " + string.Join(", ", entry.SourceJars));
                }
                if (Conflicts.Count > 20)
                    sb.AppendLine("    ... 还有 " + (Conflicts.Count - 20) + " 条冲突，详见 conflict_report.json");
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine("  ✓ 未检测到资源冲突");
            }

            sb.AppendLine();
            sb.AppendLine("  详细报告已保存至: conflict_report.json");
            sb.AppendLine("═══════════════════════════════════════");

            return sb.ToString();
        }
    }

    /// <summary>
    /// 单条冲突记录：同一资源路径在多个 jar 中出现
    /// </summary>
    public class ConflictEntry
    {
        [JsonProperty("asset_path")]
        public string AssetPath { get; set; }

        [JsonProperty("source_jars")]
        public List<string> SourceJars { get; set; }

        public ConflictEntry()
        {
            SourceJars = new List<string>();
        }
    }
}
