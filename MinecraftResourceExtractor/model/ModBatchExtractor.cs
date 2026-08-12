using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace mre.model
{
    /// <summary>
    /// Mod 批量提取器：扫描 mods 目录下所有 .jar 文件，
    /// 提取 assets/ 下的资产文件，并检测跨 Mod 的资源重名冲突
    /// </summary>
    public class ModBatchExtractor
    {
        private readonly string _javaPath;
        private readonly Dictionary<string, List<string>> _conflictMap;
        private int _totalAssetsCount;

        public ModBatchExtractor(string javaPath)
        {
            _javaPath = javaPath;
            _conflictMap = new Dictionary<string, List<string>>();
            _totalAssetsCount = 0;
        }

        /// <summary>
        /// 递归扫描目录下所有 .jar 文件
        /// </summary>
        public List<string> ScanJarFiles(string inputDir)
        {
            var jars = new List<string>();
            if (!Directory.Exists(inputDir))
                return jars;

            // 获取顶层 .jar 文件
            jars.AddRange(Directory.GetFiles(inputDir, "*.jar", SearchOption.TopDirectoryOnly));

            // 递归搜索子目录（有些整合包会在 mods/ 下分文件夹）
            jars.AddRange(Directory.GetFiles(inputDir, "*.jar", SearchOption.AllDirectories));

            // 去重并排序
            return jars.Distinct().OrderBy(f => Path.GetFileName(f)).ToList();
        }

        /// <summary>
        /// 判断 jar 条目是否为资产文件
        /// 资产文件：以 assets/ 开头，且不包含 META-INF、.class、.mcassetsroot、.xml
        /// </summary>
        public bool IsAssetEntry(string entry)
        {
            if (string.IsNullOrEmpty(entry))
                return false;

            // 必须包含 assets/ 前缀
            if (!entry.StartsWith("assets/") && !entry.StartsWith("assets\\"))
                return false;

            // 排除非资产文件
            if (entry.Contains("META-INF"))
                return false;
            if (entry.EndsWith(".class"))
                return false;
            if (entry.Contains(".mcassetsroot"))
                return false;
            if (entry.EndsWith(".xml"))
                return false;

            // 必须是文件（不是目录条目）
            // 目录条目以 / 结尾或最后一段不含 '.'
            string normalized = entry.Replace('\\', '/');
            if (normalized.EndsWith("/"))
                return false;

            // 提取文件名部分，检查是否包含 '.'（即是否有文件扩展名）
            string fileName = normalized.Substring(normalized.LastIndexOf('/') + 1);
            if (!fileName.Contains('.'))
                return false;

            return true;
        }

        /// <summary>
        /// 从单个 jar 中提取资产文件
        /// 返回提取的文件数量
        /// </summary>
        public int ExtractAssets(string jarPath, string outputDir)
        {
            int extractedCount = 0;

            // 步骤1：列出 jar 中的所有条目
            List<string> assetEntries = ListAssetEntries(jarPath);
            if (assetEntries.Count == 0)
                return 0;

            // 步骤2：提取 assets/ 下所有内容
            // 使用 jar xvf 提取 "assets" 目录，会自动保留目录结构
            Directory.CreateDirectory(outputDir);

            Process extractProcess = new Process();
            extractProcess.StartInfo.FileName = _javaPath;
            extractProcess.StartInfo.Arguments = "-xvf \"" + jarPath + "\" \"assets\"";
            extractProcess.StartInfo.UseShellExecute = false;
            extractProcess.StartInfo.RedirectStandardOutput = true;
            extractProcess.StartInfo.RedirectStandardError = true;
            extractProcess.StartInfo.CreateNoWindow = true;
            extractProcess.StartInfo.WorkingDirectory = outputDir;
            extractProcess.Start();

            // 读取输出以统计提取的文件数（避免读取所有输出造成阻塞）
            string stdout = extractProcess.StandardOutput.ReadToEnd();
            extractProcess.WaitForExit();
            extractProcess.Close();

            // 统计实际提取的文件行数（jar xvf 每提取一个文件输出一行）
            string[] lines = stdout.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string trimmed = line.TrimEnd('\r', '\n');
                if (!string.IsNullOrEmpty(trimmed) && trimmed.Contains("assets"))
                    extractedCount++;
            }

            return extractedCount;
        }

        /// <summary>
        /// 列出 jar 中所有符合资产条件的条目
        /// </summary>
        private List<string> ListAssetEntries(string jarPath)
        {
            var entries = new List<string>();

            Process listProcess = new Process();
            listProcess.StartInfo.FileName = _javaPath;
            listProcess.StartInfo.Arguments = "-tf \"" + jarPath + "\"";
            listProcess.StartInfo.UseShellExecute = false;
            listProcess.StartInfo.RedirectStandardOutput = true;
            listProcess.StartInfo.RedirectStandardError = true;
            listProcess.StartInfo.CreateNoWindow = true;
            listProcess.Start();

            string output = listProcess.StandardOutput.ReadToEnd();
            listProcess.WaitForExit();
            listProcess.Close();

            string[] lines = output.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string entry = line.TrimEnd('\r', '\n');
                if (IsAssetEntry(entry))
                {
                    // 规范化路径分隔符
                    entries.Add(entry.Replace('\\', '/'));
                }
            }

            return entries;
        }

        /// <summary>
        /// 主入口：扫描、提取、冲突检测、生成报告
        /// </summary>
        public BatchResult Run(
            string inputDir,
            string outputDir,
            Action<string> onJarStart = null,
            Action<string, int> onJarDone = null,
            Action<string, string> onJarError = null)
        {
            var result = new BatchResult
            {
                OutputDirectory = outputDir
            };

            // 1. 扫描 jar 文件
            List<string> jarFiles = ScanJarFiles(inputDir);
            result.TotalJars = jarFiles.Count;

            if (jarFiles.Count == 0)
                return result;

            // 2. 遍历每个 jar
            for (int i = 0; i < jarFiles.Count; i++)
            {
                string jarPath = jarFiles[i];
                string jarName = Path.GetFileName(jarPath);

                onJarStart?.Invoke(jarName);

                try
                {
                    // 先列出资产条目（用于冲突检测）
                    List<string> assetEntries = ListAssetEntries(jarPath);

                    // 更新冲突检测映射表
                    foreach (string entry in assetEntries)
                    {
                        // 标准化路径（统一使用 /）
                        string normalizedEntry = entry.Replace('\\', '/');

                        if (!_conflictMap.ContainsKey(normalizedEntry))
                            _conflictMap[normalizedEntry] = new List<string>();

                        if (!_conflictMap[normalizedEntry].Contains(jarName))
                            _conflictMap[normalizedEntry].Add(jarName);
                    }

                    // 执行提取
                    int fileCount = ExtractAssets(jarPath, outputDir);
                    _totalAssetsCount += fileCount;
                    result.SuccessfulJars++;

                    onJarDone?.Invoke(jarName, fileCount);
                }
                catch (Exception ex)
                {
                    result.FailedJars++;
                    result.FailedJarList.Add(jarName);
                    onJarError?.Invoke(jarName, ex.Message);
                }
            }

            // 3. 统计冲突
            result.TotalAssetsExtracted = _totalAssetsCount;
            foreach (var kvp in _conflictMap)
            {
                if (kvp.Value.Count >= 2)
                {
                    result.Conflicts.Add(new ConflictEntry
                    {
                        AssetPath = kvp.Key,
                        SourceJars = kvp.Value
                    });
                }
            }
            result.ConflictCount = result.Conflicts.Count;

            // 按冲突涉及 jar 数量降序排列
            result.Conflicts = result.Conflicts
                .OrderByDescending(c => c.SourceJars.Count)
                .ThenBy(c => c.AssetPath)
                .ToList();

            // 4. 保存冲突报告到 outputDir
            if (result.Conflicts.Count > 0)
            {
                result.SaveToFile(outputDir);
            }

            return result;
        }
    }
}
