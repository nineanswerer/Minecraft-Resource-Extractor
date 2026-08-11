using mre.view;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
				if (info.JarPaths.Count > 0)
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
				var parts = entry.Split('/');
				string accumulated = "";
				for (int i = 0; i < System.Math.Min(parts.Length, maxDepth); i++)
				{
					accumulated += (i == 0 ? "" : "/") + parts[i];
					if (i < parts.Length - 1 || entry.EndsWith("/"))
					{
						dirs.Add(accumulated + "/");
					}
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
			List<string> jarPaths = ResourceTypes.GetJarPathsForType(type);
			foreach (string jarPath in jarPaths)
			{
				ExtractPath(jarPath, settings);
			}
		}

		public void ExtractPaths(List<string> paths, Settings settings)
		{
			foreach (string path in paths)
			{
				ExtractPath(path, settings);
			}
		}

		private void ExtractPath(string jarPath, Settings settings)
		{
			Directory.CreateDirectory(settings.GetEffectiveOutputPath() + "\\" + Name);
			Process javaProcess = new Process();
			javaProcess.StartInfo.FileName = settings.JavaPath;
			javaProcess.StartInfo.Arguments = "-xvf \"" + Path + "\" \"" + jarPath + "\"";
			javaProcess.StartInfo.UseShellExecute = false;
			javaProcess.StartInfo.CreateNoWindow = true;
			javaProcess.StartInfo.WorkingDirectory = settings.GetEffectiveOutputPath() + "\\" + Name;
			javaProcess.Start();
			javaProcess.WaitForExit();
			javaProcess.Close();
		}
	}
}
