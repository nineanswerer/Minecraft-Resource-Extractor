using mre.view;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace mre.model
{
	public class JarFile
	{
		public string Path { get; set; }
		public string FullName { get; set; }
		public string Name { get; set; }
		public List<string> Folders { get; set; }
		public List<string> AllEntries { get; set; }
		/// <summary>条目路径（规范化 '/'）→ 解压后字节数，用于提取大小估算</summary>
		public Dictionary<string, long> EntrySizes { get; set; }
		/// <summary>本次提取实际写出的条目路径（规范化 '/'，逻辑路径去重），供冲突检测只统计真正提取的资产</summary>
		public List<string> ExtractedEntries { get; set; }

		public JarFile(string path)
		{
			Path = path;
			FullName = Path.Substring(Path.LastIndexOf('\\') + 1);
			Name = FullName.Substring(0, FullName.LastIndexOf("."));
			EntrySizes = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>
		/// 设置自定义显示名称（用于批量模式目录树根节点显示）
		/// </summary>
		public void SetDisplayName(string displayName)
		{
			FullName = displayName;
			Name = displayName;
		}

		/// <summary>
		/// 外部设置 AllEntries（用于聚合多个 jar 的条目）
		/// </summary>
		public void SetAllEntries(List<string> entries)
		{
			AllEntries = entries;
			ResourceTypes.UpdateJarPaths(AllEntries);
		}

		/// <summary>
		/// 外部设置 EntrySizes（用于批量聚合多个 jar 的条目大小）
		/// </summary>
		public void SetEntrySizes(Dictionary<string, long> sizes)
		{
			EntrySizes = sizes;
		}

		/// <summary>
		/// 快速列出 jar 文件中的所有条目（使用 ZipArchive，无需 Java 进程）
		/// 比 ListJarFolders() 快 100 倍以上，适用于批量扫描
		/// </summary>
		public void ListJarEntriesFast()
		{
			AllEntries = new List<string>();
			EntrySizes = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
			using (var archive = ZipFile.OpenRead(Path))
			{
				foreach (var entry in archive.Entries)
				{
					// 跳过目录条目（以 / 结尾，entry.Name 为空）
					if (!string.IsNullOrEmpty(entry.Name))
					{
						string fullName = entry.FullName;
						AllEntries.Add(fullName);
						EntrySizes[fullName] = entry.Length;
					}
				}
			}
		}

		/// <summary>
		/// 快速加载 jar 全部内容（文件条目 + 顶层文件夹），并更新资源类型路径。
		/// 等价于 ListJarFolders() 但使用 ZipArchive，避免启动 Java 进程导致卡顿。
		/// </summary>
		public void ListJarContentsFast()
		{
			AllEntries = new List<string>();
			Folders = new List<string>();
			EntrySizes = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

			using (var archive = ZipFile.OpenRead(Path))
			{
				foreach (var entry in archive.Entries)
				{
					// 跳过目录条目（entry.Name 为空）
					if (string.IsNullOrEmpty(entry.Name))
						continue;

					string fullName = entry.FullName;
					AllEntries.Add(fullName);
					EntrySizes[fullName] = entry.Length;

					// 顶层文件夹判定，逻辑与 ListJarFolders 一致
					if (!fullName.Contains("META-INF")
						&& !fullName.Contains(".class")
						&& !fullName.Contains(".mcassetsroot")
						&& !fullName.Contains(".xml")
						&& fullName.Contains('/')
						&& fullName.Contains('.'))
					{
						string folder = fullName.Substring(0, fullName.IndexOf('/'));
						if (!Folders.Contains(folder) && folder.Length < 64)
							Folders.Add(folder);
					}
				}
			}

			ResourceTypes.UpdateJarPaths(AllEntries);
		}

		public List<string> ListJarFolders(string javaPath, FrmMre view)
		{
			Folders = new List<string>();
			AllEntries = new List<string>();
			Process javaProcess = new Process();
			javaProcess.StartInfo.FileName = javaPath;
			javaProcess.StartInfo.Arguments = "-tf \"" + Path + "\"";
			javaProcess.StartInfo.UseShellExecute = false;
			javaProcess.StartInfo.RedirectStandardOutput = true;
			javaProcess.StartInfo.CreateNoWindow = true;
			javaProcess.Start();

			view.Status("Step 3/4 > Loading jar...");
			List<string> strings = javaProcess.StandardOutput.ReadToEnd().Split('\n').ToList();
			for (int i = 0; i < strings.Count; i++)
			{
				string line = strings[i].TrimEnd('\r', '\n');
				if (string.IsNullOrEmpty(line))
					continue;

				AllEntries.Add(line);

				if (!line.Contains("META-INF")
					&& !line.Contains(".class")
					&& !line.Contains(".mcassetsroot")
					&& !line.Contains(".xml")
					&& line.Contains('/')
					&& line.Contains('.'))
				{
					string folder = line.Split('/')[0];
					if (!Folders.Contains(folder) && folder.Length < 64)
					{
						Folders.Add(folder);
					}
				}
			}

			javaProcess.WaitForExit();
			javaProcess.Close();

			// 动态更新资源类型路径（适配不同 Minecraft 命名空间）
			ResourceTypes.UpdateJarPaths(AllEntries);

			view.Status("Step 3/4 > Jar loaded");
			return Folders;
		}

		/// <summary>
		/// 获取 jar 中实际存在的资源类型（根据 jar 内容过滤）
		/// </summary>
		public List<ResourceTypeInfo> GetAvailableResourceTypes()
		{
			if (AllEntries == null || AllEntries.Count == 0)
				return new List<ResourceTypeInfo>(ResourceTypes.AllTypes); // fallback: show all

			// 路径已在 ListJarFolders 中通过 UpdateJarPaths 动态匹配
			var available = new List<ResourceTypeInfo>();
			foreach (var info in ResourceTypes.AllTypes)
			{
				if (info.JarPaths.Count == 0)
					continue;

				// 带文件后缀过滤器的类型：只有当 jar 中确实存在匹配文件时才显示
				if (!string.IsNullOrEmpty(info.FileSuffix) && CountResourceTypeFiles(info.Type) == 0)
					continue;

				available.Add(info);
			}
			return available;
		}

		/// <summary>
		/// 获取 jar 中的目录树结构
		/// </summary>
		public List<string> GetDirectoryTree(int maxDepth = 3)
		{
			if (AllEntries == null || AllEntries.Count == 0)
				return new List<string>();

			var dirs = new HashSet<string>();
			foreach (var entry in AllEntries)
			{
				// 用索引扫描替代 Split()，避免为每条 entry 分配数组
				int depth = 0;
				int pos = 0;
				int len = entry.Length;
				while (pos < len && depth < maxDepth)
				{
					int slashPos = entry.IndexOf('/', pos);
					if (slashPos < 0) break;

					depth++;
					string dir = entry.Substring(0, slashPos + 1);
					dirs.Add(dir);
					pos = slashPos + 1;
				}
			}

			var sorted = dirs.OrderBy(d => d).ToList();
			return sorted;
		}

		public void ExtractJarFolder(string folder, Settings settings)
		{
			ExtractArchive(folder, null, settings, null);
		}

		public void ExtractResourceType(ResourceType type, Settings settings)
		{
			ExtractResourceType(type, settings, null);
		}

		/// <summary>
		/// 按资源类型提取，支持自定义子目录前缀（用于按类型分类输出）
		/// </summary>
		public void ExtractResourceType(ResourceType type, Settings settings, string subDirPrefix)
		{
			ResourceTypeInfo info = ResourceTypes.GetInfo(type);
			string fileSuffix = info != null ? info.FileSuffix : null;
			List<string> jarPaths = ResourceTypes.GetJarPathsForType(type);
			foreach (string jarPath in jarPaths)
			{
				ExtractArchive(jarPath, fileSuffix, settings, subDirPrefix);
			}
		}

		/// <summary>
		/// 获取提取的资源文件数量（估算）
		/// </summary>
		public int CountResourceTypeFiles(ResourceType type)
		{
			if (AllEntries == null) return 0;
			return EnumerateMatchingEntries(type).Count();
		}

		/// <summary>
		/// 获取提取指定资源类型的文件总字节数（解压后大小），用于提取前的大小估算。
		/// 依赖扫描时填充的 EntrySizes；批量模式下由聚合合并而来。
		/// </summary>
		public long CountResourceTypeBytes(ResourceType type)
		{
			if (AllEntries == null) return 0;
			long bytes = 0;
			foreach (string normalized in EnumerateMatchingEntries(type))
			{
				long size;
				if (EntrySizes != null && EntrySizes.TryGetValue(normalized, out size))
					bytes += size;
			}
			return bytes;
		}

		/// <summary>
		/// 一次遍历同时统计指定资源类型的文件数与总字节数（供预览使用，避免二次遍历）。
		/// </summary>
		public void CountResourceTypeSummary(ResourceType type, out int count, out long bytes)
		{
			count = 0;
			bytes = 0;
			if (AllEntries == null) return;
			foreach (string normalized in EnumerateMatchingEntries(type))
			{
				count++;
				long size;
				if (EntrySizes != null && EntrySizes.TryGetValue(normalized, out size))
					bytes += size;
			}
		}

		/// <summary>
		/// 遍历匹配指定资源类型的所有条目（规范化路径）。文件数统计与大小估算共用。
		/// </summary>
		private IEnumerable<string> EnumerateMatchingEntries(ResourceType type)
		{
			ResourceTypeInfo info = ResourceTypes.GetInfo(type);
			string fileSuffix = info != null ? info.FileSuffix : null;
			List<string> jarPaths = ResourceTypes.GetJarPathsForType(type);
			foreach (string jarPath in jarPaths)
			{
				string prefix = jarPath.TrimEnd('/');
				foreach (string entry in AllEntries)
				{
					string normalized = entry.Replace('\\', '/');
					if (!normalized.StartsWith(prefix + "/") && !normalized.StartsWith(prefix))
						continue;
					if (!string.IsNullOrEmpty(fileSuffix) && !normalized.EndsWith(fileSuffix, StringComparison.OrdinalIgnoreCase))
						continue;
					yield return normalized;
				}
			}
		}

		public void ExtractPaths(List<string> paths, Settings settings)
		{
			foreach (string path in paths)
			{
				ExtractArchive(path, null, settings, null);
			}
		}

		/// <summary>
		/// 从 jar 中提取指定目录前缀下的文件到输出目录。
		/// 使用 ZipArchive 逐条提取，替代 Java `jar -xvf` 进程——进程启动开销是批量提取
		/// 极慢（80+ jar 耗时 1 小时+）的根因。fileSuffix 非空时只提取匹配后缀的文件（文件级精确提取），
		/// 为空时提取整个目录。
		/// </summary>
		private void ExtractArchive(string jarPathPrefix, string fileSuffix, Settings settings, string subDirPrefix)
		{
			string outputDir = settings.GetEffectiveOutputPath() + "\\" + Name;
			if (!string.IsNullOrEmpty(subDirPrefix))
				outputDir = settings.GetEffectiveOutputPath() + "\\" + subDirPrefix + "\\" + Name;

			string prefix = jarPathPrefix.Replace('\\', '/').TrimEnd('/');

			using (var archive = ZipFile.OpenRead(Path))
			{
				foreach (var entry in archive.Entries)
				{
					// 跳过目录条目
					if (string.IsNullOrEmpty(entry.Name))
						continue;

					string fullName = entry.FullName.Replace('\\', '/');
					if (!fullName.StartsWith(prefix + "/"))
						continue;
					if (!string.IsNullOrEmpty(fileSuffix) && !fullName.EndsWith(fileSuffix, StringComparison.OrdinalIgnoreCase))
						continue;

					string destPath = System.IO.Path.Combine(outputDir, entry.FullName.Replace('/', '\\'));
					string destDir = System.IO.Path.GetDirectoryName(destPath);
					if (!string.IsNullOrEmpty(destDir))
						Directory.CreateDirectory(destDir);
					entry.ExtractToFile(destPath, true);
				}
			}
		}

		/// <summary>
		/// 单次打开 zip、单次遍历，批量提取多个「(目录前缀, 文件后缀, 子目录前缀)」请求。
		/// 相比对每个请求各调一次 ExtractArchive，避免重复打开 jar（批量提取的主要开销之一）。
		/// 提取时回填 AllEntries / EntrySizes 与 ExtractedEntries（实际写出的条目，供冲突检测复用）。
		/// 同一目标路径只写一次（同名条目被多个重叠前缀命中时不重复写盘）。
		/// </summary>
		public void ExtractMultiple(IList<ExtractRequest> requests, Settings settings, Action<int> onProgress = null)
		{
			string[] prefixes, suffixes, outputRoots;
			PreprocessRequests(requests, settings, out prefixes, out suffixes, out outputRoots);
			int n = prefixes.Length;

			AllEntries = new List<string>();
			EntrySizes = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
			ExtractedEntries = new List<string>();

			if (n == 0)
			{
				onProgress?.Invoke(0);
				return;
			}

			// 去重：同名条目命中多个前缀时（如 textures/ 与 textures/gui/）只写一次
			var written = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			// 实际写出的条目逻辑路径（去重），供冲突检测只统计真正提取的资产
			var extractedSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			int extracted = 0;
			const int REPORT_STEP = 25; // 每写 25 个文件汇报一次进度，避免高频回调拖慢 UI

			using (var archive = ZipFile.OpenRead(Path))
			{
				foreach (var entry in archive.Entries)
				{
					// 跳过目录条目
					if (string.IsNullOrEmpty(entry.Name))
						continue;

					string fullName = entry.FullName.Replace('\\', '/');
					AllEntries.Add(fullName);
					EntrySizes[fullName] = entry.Length;

					for (int i = 0; i < n; i++)
					{
						if (!fullName.StartsWith(prefixes[i] + "/"))
							continue;
						if (suffixes[i] != null && !fullName.EndsWith(suffixes[i], StringComparison.OrdinalIgnoreCase))
							continue;

						string destPath = System.IO.Path.Combine(outputRoots[i], entry.FullName.Replace('/', '\\'));
						if (!written.Add(destPath))
							continue;

						string destDir = System.IO.Path.GetDirectoryName(destPath);
						if (!string.IsNullOrEmpty(destDir))
							Directory.CreateDirectory(destDir);
						entry.ExtractToFile(destPath, true);
						extracted++;
						if (extractedSet.Add(fullName))
							ExtractedEntries.Add(fullName);
						if (extracted % REPORT_STEP == 0)
							onProgress?.Invoke(extracted);
					}
				}
			}
			onProgress?.Invoke(extracted); // 收尾，确保进度归位
		}

		/// <summary>
		/// 统计一次 ExtractMultiple 将会实际写出的文件数（匹配 + 按目标路径去重），
		/// 用于批量提取前预估进度分母，使进度条能精确到 100%。
		/// 匹配/去重逻辑与 ExtractMultiple 完全一致（共用 PreprocessRequests）。
		/// </summary>
		public int CountExtractedFiles(IList<ExtractRequest> requests, Settings settings)
		{
			string[] prefixes, suffixes, outputRoots;
			PreprocessRequests(requests, settings, out prefixes, out suffixes, out outputRoots);
			int n = prefixes.Length;
			if (n == 0) return 0;

			var written = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			int count = 0;
			using (var archive = ZipFile.OpenRead(Path))
			{
				foreach (var entry in archive.Entries)
				{
					if (string.IsNullOrEmpty(entry.Name)) continue;
					string fullName = entry.FullName.Replace('\\', '/');
					for (int i = 0; i < n; i++)
					{
						if (!fullName.StartsWith(prefixes[i] + "/")) continue;
						if (suffixes[i] != null && !fullName.EndsWith(suffixes[i], StringComparison.OrdinalIgnoreCase)) continue;
						string destPath = System.IO.Path.Combine(outputRoots[i], entry.FullName.Replace('/', '\\'));
						if (written.Add(destPath)) count++;
					}
				}
			}
			return count;
		}

		/// <summary>
		/// 预处理提取请求为并行数组（前缀/后缀/输出根目录），供 ExtractMultiple 与 CountExtractedFiles 共用，
		/// 确保两者的匹配与去重逻辑完全一致。
		/// </summary>
		private void PreprocessRequests(IList<ExtractRequest> requests, Settings settings,
			out string[] prefixes, out string[] suffixes, out string[] outputRoots)
		{
			int n = requests == null ? 0 : requests.Count;
			prefixes = new string[n];
			suffixes = new string[n];
			outputRoots = new string[n];
			string baseOut = settings.GetEffectiveOutputPath() + "\\" + Name;
			for (int i = 0; i < n; i++)
			{
				prefixes[i] = requests[i].Prefix.Replace('\\', '/').TrimEnd('/');
				suffixes[i] = string.IsNullOrEmpty(requests[i].FileSuffix) ? null : requests[i].FileSuffix;
				outputRoots[i] = string.IsNullOrEmpty(requests[i].SubDirPrefix)
					? baseOut
					: settings.GetEffectiveOutputPath() + "\\" + requests[i].SubDirPrefix + "\\" + Name;
			}
		}
	}

	/// <summary>
	/// 单次批量提取的请求：提取 Prefix 前缀下的文件（可选 FileSuffix 过滤）到 SubDirPrefix 分类目录。
	/// </summary>
	public struct ExtractRequest
	{
		/// <summary>目录前缀（如 assets/create/textures/，可带尾斜杠）</summary>
		public string Prefix;
		/// <summary>可选文件后缀过滤（如 ".mcmeta"），null/空 = 提取整个目录</summary>
		public string FileSuffix;
		/// <summary>可选输出子目录前缀（按类型分类时用），null/空 = 直接输出到 jar 目录</summary>
		public string SubDirPrefix;
	}
}
