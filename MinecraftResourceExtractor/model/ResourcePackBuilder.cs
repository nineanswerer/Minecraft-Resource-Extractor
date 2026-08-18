using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;

namespace mre.model
{
    /// <summary>汉化资源包生成结果统计。</summary>
    public class ResourcePackResult
    {
        /// <summary>命中的命名空间数（assets/&lt;ns&gt; 的 ns 去重后）</summary>
        public int NamespaceCount;
        /// <summary>读入的 lang 文件数（每个 mod 的 assets/&lt;ns&gt;/lang/&lt;lang&gt;.json 各算一个）</summary>
        public int FileCount;
        /// <summary>合并后的 key 总数</summary>
        public int KeyCount;
        /// <summary>跨 mod 同名 key 冲突次数（后加载覆盖，仅统计提示）</summary>
        public int ConflictCount;
        /// <summary>实际使用的 MC 版本（写入 pack.mcmeta）</summary>
        public string Version;
    }

    /// <summary>
    /// 汉化资源包合成：读取「语言包」目录（一键语言包提纯的产物），把多个 mod 的 lang 文件
    /// 按命名空间合并成一个 Minecraft 资源包（pack.mcmeta + pack.png + assets/&lt;ns&gt;/lang/*.json）。
    /// </summary>
    public static class ResourcePackBuilder
    {
        /// <summary>
        /// 合成资源包。sourceDir 形如 &lt;output&gt;\语言包\<jar名>\assets\<ns>\lang\<lang>.json，
        /// 按命名空间合并目标语言后写到 outputDir（若已存在会先清空重建）。
        /// </summary>
        public static ResourcePackResult Build(
            string sourceDir, string targetLang, string outputDir, string mcVersion, string description)
        {
            var result = new ResourcePackResult { Version = mcVersion };

            if (Directory.Exists(outputDir))
                Directory.Delete(outputDir, true);
            Directory.CreateDirectory(outputDir);

            var merged = new Dictionary<string, JObject>(StringComparer.OrdinalIgnoreCase);
            var order = new List<string>(); // 记录命名空间首次出现顺序，保证输出稳定

            if (Directory.Exists(sourceDir))
            {
                string langFileName = targetLang + ".json";
                foreach (string file in Directory.EnumerateFiles(sourceDir, langFileName, SearchOption.AllDirectories))
                {
                    string ns = TryGetNamespace(file);
                    if (ns == null)
                        continue;

                    JObject obj;
                    try
                    {
                        obj = JObject.Parse(File.ReadAllText(file));
                    }
                    catch
                    {
                        continue; // 非法 json，跳过
                    }

                    JObject target;
                    if (!merged.TryGetValue(ns, out target))
                    {
                        target = new JObject();
                        merged[ns] = target;
                        order.Add(ns);
                    }

                    foreach (var prop in obj)
                    {
                        if (target.ContainsKey(prop.Key))
                            result.ConflictCount++;
                        target[prop.Key] = prop.Value.DeepClone(); // 同名 key 后加载覆盖
                    }
                    result.FileCount++;
                }
            }

            foreach (string ns in order)
            {
                string langDir = Path.Combine(outputDir, "assets", ns, "lang");
                Directory.CreateDirectory(langDir);
                JObject obj = merged[ns];
                File.WriteAllText(Path.Combine(langDir, targetLang + ".json"), obj.ToString(Formatting.Indented));
                result.KeyCount += obj.Count;
            }
            result.NamespaceCount = order.Count;

            PackMcMeta.Generate(outputDir, mcVersion, description);
            GenerateDefaultIcon(Path.Combine(outputDir, "pack.png"));

            return result;
        }

        /// <summary>从文件路径提取命名空间；路径形如 …/assets/&lt;ns&gt;/lang/&lt;lang&gt;.json，不符合返回 null。</summary>
        private static string TryGetNamespace(string file)
        {
            string normalized = file.Replace('\\', '/');
            int langIdx = normalized.LastIndexOf("/lang/", StringComparison.OrdinalIgnoreCase);
            if (langIdx < 0)
                return null;
            const string assetsMarker = "/assets/";
            int assetsIdx = normalized.LastIndexOf(assetsMarker, langIdx, StringComparison.OrdinalIgnoreCase);
            if (assetsIdx < 0)
                return null;
            int nsStart = assetsIdx + assetsMarker.Length;
            string ns = normalized.Substring(nsStart, langIdx - nsStart);
            if (ns.Length == 0 || ns.IndexOf('/') >= 0)
                return null;
            return ns;
        }

        /// <summary>从 jar 名（或任意文本）列表检测 MC 版本，取出现频率最高者；检测不到返回 null。</summary>
        public static string DetectVersion(IEnumerable<string> names)
        {
            if (names == null)
                return null;
            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (string name in names)
            {
                string v = PackMcMeta.DetectMcVersion(name);
                if (string.IsNullOrEmpty(v))
                    continue;
                int c;
                counts.TryGetValue(v, out c);
                counts[v] = c + 1;
            }
            if (counts.Count == 0)
                return null;
            return counts.OrderByDescending(kv => kv.Value).First().Key;
        }

        /// <summary>生成 128×128 默认 pack.png（深色底 + 绿色块 + "MRE" 字样）。失败静默忽略（图标非必需）。</summary>
        private static void GenerateDefaultIcon(string path)
        {
            try
            {
                using (var bmp = new Bitmap(128, 128))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.Clear(Color.FromArgb(40, 44, 52));
                        using (var block = new SolidBrush(Color.FromArgb(96, 200, 120)))
                            g.FillRectangle(block, 8, 8, 112, 112);
                        using (var font = new Font("Segoe UI", 26, FontStyle.Bold))
                        using (var text = new SolidBrush(Color.White))
                        using (var fmt = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        })
                        {
                            g.DrawString("MRE", font, text, new RectangleF(8, 8, 112, 112), fmt);
                        }
                    }
                    bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            catch
            {
                // 图标生成失败不影响资源包本身
            }
        }
    }
}
