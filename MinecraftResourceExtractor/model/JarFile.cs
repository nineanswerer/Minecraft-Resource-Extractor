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

		public JarFile(string path)
		{
			Path = path;
			FullName = Path.Substring(Path.LastIndexOf('\\') + 1);
			Name = FullName.Substring(0, FullName.LastIndexOf("."));
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
		/// 快速列出 jar 文件中的所有条目（使用 ZipArchive，无需 Java 进程）
		/// 比 ListJarFolders() 快 100 倍以上，适用于批量扫描
		/// </summary>
		public void ListJarEntriesFast()
		{
			AllEntries = new List<string>();
			using (var archive = ZipFile.OpenRead(Path))
			{
				foreach (var entry in archive.Entries)
				{
					// 跳过目录条目（以 / 结尾）
					if (!string.IsNullOrEmpty(entry.Name))
						AllEntries.Add(entry.FullName);
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

			using (var archive = ZipFile.OpenRead(Path))
			{
				foreach (var entry in archive.Entries)
				{
					// 跳过目录条目（entry.Name 为空）
					if (string.IsNullOrEmpty(entry.Name))
						continue;

					string fullName = entry.FullName;
					AllEntries.Add(fullName);

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
			Directory.CreateDirectory(settings.GetEffectiveOutputPath() + "\\" + Name);
			Process javaProcess = new Process();
			javaProcess.StartInfo.FileName = settings.JavaPath;
			javaProcess.StartInfo.Arguments = "-xvf \"" + Path + "\" \"" + folder + "\"";
			javaProcess.StartInfo.UseShellExecute = false;
			javaProcess.StartInfo.CreateNoWindow = true;
			javaProcess.StartInfo.WorkingDirectory = settings.GetEffectiveOutputPath() + "\\" + Name;
			javaProcess.Start();
			javaProcess.WaitForExit();
			javaProcess.Close();
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
			List<string> jarPaths = ResourceTypes.GetJarPathsForType(type);
			foreach (string jarPath in jarPaths)
			{
				if (info != null && !string.IsNullOrEmpty(info.FileSuffix))
				{
					// 文件级精确提取：只取该目录下匹配后缀的文件
					ExtractFilteredFiles(jarPath, info.FileSuffix, settings, subDirPrefix);
				}
				else
				{
					ExtractPath(jarPath, settings, subDirPrefix);
				}
			}
		}

		/// <summary>
		/// 获取提取的资源文件数量（估算）
		/// </summary>
		public int CountResourceTypeFiles(ResourceType type)
		{
			if (AllEntries == null) return 0;
			ResourceTypeInfo info = ResourceTypes.GetInfo(type);
			string fileSuffix = info != null ? info.FileSuffix : null;
			List<string> jarPaths = ResourceTypes.GetJarPathsForType(type);
			int count = 0;
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
					count++;
				}
			}
			return count;
		}

		public void ExtractPaths(List<string> paths, Settings settings)
		{
			foreach (string path in paths)
			{
				ExtractPath(path, settings, null);
			}
		}

		/// <summary>
		/// 文件级精确提取：从 jar 中只提取指定目录前缀下、匹配后缀的文件（用于 .mcmeta 等场景）。
		/// 使用 ZipArchive 逐条提取，弥补 `jar -xvf` 只能按整目录提取的局限。
		/// </summary>
		private void ExtractFilteredFiles(string jarPathPrefix, string fileSuffix, Settings settings, string subDirPrefix)
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
					if (!fullName.StartsWith(prefix + "/") || !fullName.EndsWith(fileSuffix, StringComparison.OrdinalIgnoreCase))
						continue;

					string destPath = System.IO.Path.Combine(outputDir, entry.FullName.Replace('/', '\\'));
					string destDir = System.IO.Path.GetDirectoryName(destPath);
					if (!string.IsNullOrEmpty(destDir))
						Directory.CreateDirectory(destDir);
					entry.ExtractToFile(destPath, true);
				}
			}
		}

		private void ExtractPath(string jarPath, Settings settings, string subDirPrefix = null)
		{
			string outputDir = settings.GetEffectiveOutputPath() + "\\" + Name;
			if (!string.IsNullOrEmpty(subDirPrefix))
				outputDir = settings.GetEffectiveOutputPath() + "\\" + subDirPrefix + "\\" + Name;
			Directory.CreateDirectory(outputDir);
			Process javaProcess = new Process();
			javaProcess.StartInfo.FileName = settings.JavaPath;
			javaProcess.StartInfo.Arguments = "-xvf \"" + Path + "\" \"" + jarPath + "\"";
			javaProcess.StartInfo.UseShellExecute = false;
			javaProcess.StartInfo.CreateNoWindow = true;
			javaProcess.StartInfo.WorkingDirectory = outputDir;
			javaProcess.Start();
			javaProcess.WaitForExit();
			javaProcess.Close();
		}
	}
}
