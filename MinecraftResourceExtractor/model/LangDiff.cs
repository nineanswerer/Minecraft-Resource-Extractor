using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace mre.model
{
	/// <summary>
	/// 一条未翻译文本记录：某个 mod 的某个 lang key 缺失、照抄英文、或疑似缩写未翻译。
	/// </summary>
	public class LangDiffEntry
	{
		/// <summary>mod 名（不含 .jar 后缀）</summary>
		public string Jar;
		/// <summary>资源命名空间（assets/&lt;ns&gt;/lang/ 中的 ns）</summary>
		public string Namespace;
		/// <summary>翻译 key</summary>
		public string Key;
		/// <summary>基准语言原文（默认英文）</summary>
		public string Source;
		/// <summary>目标语言现状（缺失则为空）</summary>
		public string Target;
		/// <summary>"missing"（缺失）| "copied"（照抄英文）| "maybe"（疑似缩写/专有名词）</summary>
		public string Status;
	}

	/// <summary>
	/// 未翻译文本对比结果：未翻译 key 明细 + 完全缺失目标语言的 mod 清单。
	/// </summary>
	public class LangDiffResult
	{
		/// <summary>未翻译 key 明细（含缺失/照抄/疑似）</summary>
		public List<LangDiffEntry> Entries = new List<LangDiffEntry>();
		/// <summary>完全没有目标语言文件的 mod 名（供日志聚合提示，逐条明细仍在 Entries 里）</summary>
		public List<string> CompletelyMissingJars = new List<string>();
	}

	/// <summary>
	/// 未翻译文本对比：对比基准语言（默认 en_us）与目标语言（默认 zh_cn）的 assets/&lt;ns&gt;/lang/*.json，
	/// 找出「缺失 key」「照抄英文」与「疑似缩写」三类未翻译项，导出 CSV + JSON 供翻译软件导入。
	/// </summary>
	public static class LangDiff
	{
		/// <summary>
		/// 扫描所有 jar，找出目标语言中缺失、照抄基准语言、或疑似缩写未翻译的 key。
		/// </summary>
		/// <param name="jarPaths">jar 文件路径列表</param>
		/// <param name="baseLang">基准语言（如 en_us）</param>
		/// <param name="targetLang">目标语言（如 zh_cn）</param>
		/// <param name="onProgress">进度回调 (已完成 jar 数, 总 jar 数)</param>
		public static LangDiffResult FindMissingKeys(
			List<string> jarPaths, string baseLang, string targetLang, Action<int, int> onProgress = null)
		{
			var result = new LangDiffResult();
			if (jarPaths == null || jarPaths.Count == 0)
				return result;

			int total = jarPaths.Count;
			int done = 0;
			foreach (string jarPath in jarPaths)
			{
				try
				{
					var jar = new JarFile(jarPath);
					jar.ListJarEntriesFast();
					if (jar.AllEntries == null || jar.AllEntries.Count == 0)
						continue;

					string jarName = jar.Name;

					// 收集该 jar 中基准/目标语言的 lang json，按命名空间分组
					var baseByNs = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
					var targetByNs = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

					foreach (string entry in jar.AllEntries)
					{
						string normalized = entry.Replace('\\', '/');
						if (!normalized.StartsWith("assets/", StringComparison.OrdinalIgnoreCase))
							continue;
						if (!normalized.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
							continue;

						// 匹配 assets/<ns>/lang/<lang>.json
						string rest = normalized.Substring("assets/".Length);
						string[] parts = rest.Split('/');
						if (parts.Length != 3)
							continue;
						if (!string.Equals(parts[1], "lang", StringComparison.OrdinalIgnoreCase))
							continue;

						string ns = parts[0];
						string langFile = parts[2];
						string lang = langFile.Substring(0, langFile.Length - ".json".Length);

						if (string.Equals(lang, baseLang, StringComparison.OrdinalIgnoreCase))
							baseByNs[ns] = jar.ReadEntryBytes(normalized);
						else if (string.Equals(lang, targetLang, StringComparison.OrdinalIgnoreCase))
							targetByNs[ns] = jar.ReadEntryBytes(normalized);
					}

					// 完全没有目标语言文件：日志聚合用（逐条明细仍会写入 Entries）
					if (baseByNs.Count > 0 && targetByNs.Count == 0)
						result.CompletelyMissingJars.Add(jarName);

					foreach (var kv in baseByNs)
					{
						string ns = kv.Key;
						byte[] targetBytes;
						targetByNs.TryGetValue(ns, out targetBytes);

						JObject baseObj;
						try
						{
							baseObj = JObject.Parse(Encoding.UTF8.GetString(kv.Value));
						}
						catch
						{
							continue; // 非法 json，跳过该命名空间
						}

						JObject targetObj = null;
						if (targetBytes != null)
						{
							try
							{
								targetObj = JObject.Parse(Encoding.UTF8.GetString(targetBytes));
							}
							catch
							{
								targetObj = null;
							}
						}

						foreach (var prop in baseObj)
						{
							string key = prop.Key;
							string source = TokenToString(prop.Value);

							// 纯符号/数字/格式串（§a、100%、%s、"-" 等）中英本就相同，不需要翻译，跳过
							if (!IsTranslatableText(source))
								continue;

							// 整个目标语言文件缺失：该 ns 所有 key 都算缺失
							if (targetObj == null)
							{
								result.Entries.Add(new LangDiffEntry
								{
									Jar = jarName,
									Namespace = ns,
									Key = key,
									Source = source,
									Target = "",
									Status = "missing"
								});
								continue;
							}

							JToken targetToken;
							if (!targetObj.TryGetValue(key, out targetToken))
							{
								result.Entries.Add(new LangDiffEntry
								{
									Jar = jarName,
									Namespace = ns,
									Key = key,
									Source = source,
									Target = "",
									Status = "missing"
								});
							}
							else
							{
								string targetVal = TokenToString(targetToken);
								// 目标值照抄英文原文（且原文非空）
								if (!string.IsNullOrEmpty(source) && string.Equals(source, targetVal, StringComparison.Ordinal))
								{
									result.Entries.Add(new LangDiffEntry
									{
										Jar = jarName,
										Namespace = ns,
										Key = key,
										Source = source,
										Target = targetVal,
										// 全大写缩写/单位等可能是专有名词或有意保留，标「疑似」供人工复核
										Status = LooksLikeAbbreviation(source) ? "maybe" : "copied"
									});
								}
							}
						}
					}
				}
				catch
				{
					// 单个 jar 读取失败不阻断整体扫描
				}
				finally
				{
					done++;
					onProgress?.Invoke(done, total);
				}
			}

			return result;
		}

		/// <summary>
		/// 导出 CSV（UTF-8 BOM，Excel 直接打开不乱码）。
		/// </summary>
		public static void WriteCsv(string path, List<LangDiffEntry> entries)
		{
			var sb = new StringBuilder();
			sb.AppendLine("mod,namespace,key,英文原文,中文现状,状态");
			foreach (var e in entries)
			{
				sb.AppendLine(string.Join(",",
					EscapeCsv(e.Jar),
					EscapeCsv(e.Namespace),
					EscapeCsv(e.Key),
					EscapeCsv(e.Source),
					EscapeCsv(e.Target),
					EscapeCsv(e.Status)));
			}
			File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
		}

		/// <summary>
		/// 导出 JSON（缩进格式）。
		/// </summary>
		public static void WriteJson(string path, List<LangDiffEntry> entries)
		{
			File.WriteAllText(path, JsonConvert.SerializeObject(entries, Formatting.Indented));
		}

		/// <summary>lang 值一般为字符串；非字符串 token（罕见）降级为紧凑 JSON 文本。</summary>
		private static string TokenToString(JToken token)
		{
			if (token == null) return "";
			if (token.Type == JTokenType.String) return (string)token;
			return token.ToString(Formatting.None);
		}

		/// <summary>CSV 字段转义（含逗号/引号/换行时加引号）。</summary>
		private static string EscapeCsv(string value)
		{
			if (string.IsNullOrEmpty(value)) return "";
			if (value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
				return "\"" + value.Replace("\"", "\"\"") + "\"";
			return value;
		}

		/// <summary>
		/// 判断值是否「像可翻译文本」：去掉 § 格式代码、% 占位符、数字、标点后是否仍含字母/汉字。
		/// 纯符号/数字/格式串（§a、100%、%s、"-"）返回 false，跳过不参与对比。
		/// </summary>
		private static bool IsTranslatableText(string value)
		{
			if (string.IsNullOrEmpty(value)) return false;
			int i = 0;
			while (i < value.Length)
			{
				char c = value[i];
				if (c == '§')
				{
					i += 2; // 跳过 § 及紧跟的格式字符
					continue;
				}
				if (c == '%')
				{
					i++; // 跳过 %
					while (i < value.Length && (char.IsLetterOrDigit(value[i]) || value[i] == '$' || value[i] == '.'))
						i++;
					continue;
				}
				if (char.IsLetter(c)) return true;
				i++;
			}
			return false;
		}

		/// <summary>
		/// 判断「照抄英文」的值是否更像缩写/专有名词（全大写缩写、单位等），这类可能有意保留，标「疑似」。
		/// </summary>
		private static bool LooksLikeAbbreviation(string value)
		{
			string s = StripFormatCodes(value).Trim();
			if (s.Length == 0 || s.Length > 8) return false;

			bool hasLower = false;
			bool hasUpper = false;
			foreach (char c in s)
			{
				if (char.IsLower(c)) hasLower = true;
				else if (char.IsUpper(c)) hasUpper = true;
			}

			// 全大写缩写（HP / TNT / FPS / OK）
			if (hasUpper && !hasLower) return true;

			// 极短 token（≤4 字符、无空格）：单位/缩写（ms / kg / RF/t）
			if (s.Length <= 4 && !s.Contains(" ")) return true;

			return false;
		}

		/// <summary>去掉 Minecraft 格式代码（§ + 单字符）。</summary>
		private static string StripFormatCodes(string value)
		{
			if (string.IsNullOrEmpty(value)) return value;
			var sb = new StringBuilder(value.Length);
			for (int i = 0; i < value.Length; i++)
			{
				if (value[i] == '§' && i + 1 < value.Length)
				{
					i++; // 跳过 § 及紧跟的格式字符
					continue;
				}
				sb.Append(value[i]);
			}
			return sb.ToString();
		}
	}
}
