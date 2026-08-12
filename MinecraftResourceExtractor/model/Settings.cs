using mre.view;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace mre.model
{
	public class Settings
	{
		private static Settings _instance = null;
		public string MreDirPath { get; set; }
		public string JavaPath { get; set; } = null;
		public string OutputPath { get; set; } = null;
		public string LastMcPath { get; set; } = null;
		public string LastBatchInputPath { get; set; } = null;

		private static string ConfigDir
		{
			get
			{
				string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "mre");
				if (!Directory.Exists(dir))
					Directory.CreateDirectory(dir);
				return dir;
			}
		}

		private static string ConfigFile => Path.Combine(ConfigDir, "config.json");

		private Settings()
		{
			MreDirPath = Directory.GetCurrentDirectory();
			LoadConfig();
			if (string.IsNullOrEmpty(JavaPath) || !File.Exists(JavaPath))
			{
				JavaPath = LocateJava();
				SaveConfig();
			}
		}

		public static Settings GetInstance()
		{
			if (_instance == null)
			{
				_instance = new Settings();
			}
			return _instance;
		}

		public void SaveConfig()
		{
			try
			{
				var config = new
				{
					javaPath = JavaPath,
					outputPath = OutputPath,
					lastMcPath = LastMcPath,
					lastBatchInputPath = LastBatchInputPath
				};
				File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(config, Formatting.Indented));
			}
			catch
			{
				// 静默失败，不影响主流程
			}
		}

		private void LoadConfig()
		{
			try
			{
				if (File.Exists(ConfigFile))
				{
					string json = File.ReadAllText(ConfigFile);
					var config = JsonConvert.DeserializeAnonymousType(json, new
					{
						javaPath = "",
						outputPath = "",
						lastMcPath = "",
						lastBatchInputPath = ""
					});
					if (config != null)
					{
						if (!string.IsNullOrEmpty(config.javaPath) && File.Exists(config.javaPath))
							JavaPath = config.javaPath;
						if (!string.IsNullOrEmpty(config.outputPath))
							OutputPath = config.outputPath;
						if (!string.IsNullOrEmpty(config.lastMcPath))
							LastMcPath = config.lastMcPath;
						if (!string.IsNullOrEmpty(config.lastBatchInputPath))
							LastBatchInputPath = config.lastBatchInputPath;
					}
				}
			}
			catch
			{
				// 配置文件损坏时忽略
			}
		}

		private string LocateJava()
		{
			// 1. 先检查环境变量 JAVA_HOME
			string javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
			if (!string.IsNullOrEmpty(javaHome))
			{
				string jarInHome = Path.Combine(javaHome, "bin", "jar.exe");
				if (File.Exists(jarInHome))
					return jarInHome;
			}

			// 2. 检查 PATH 环境变量
			string pathEnv = Environment.GetEnvironmentVariable("PATH");
			if (!string.IsNullOrEmpty(pathEnv))
			{
				foreach (string dir in pathEnv.Split(';'))
				{
					try
					{
						string jarPath = Path.Combine(dir.Trim(), "jar.exe");
						if (File.Exists(jarPath))
							return jarPath;
					}
					catch { }
				}
			}

			// 3. 搜索 Program Files 中的 Java 目录
			DirectoryInfo[] searchResult;
			DirectoryInfo start = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
			DirectoryInfo startX86 = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));

			var javaDirs = Enumerable.Empty<DirectoryInfo>();
			if (start.Exists)
				javaDirs = javaDirs.Concat(start.GetDirectories("Java", SearchOption.TopDirectoryOnly));
			if (startX86.Exists)
				javaDirs = javaDirs.Concat(startX86.GetDirectories("Java", SearchOption.TopDirectoryOnly));

			searchResult = javaDirs.ToArray();

			foreach (var dir in searchResult)
			{
				FileInfo[] foundFiles = dir.GetFiles("jar.exe", SearchOption.AllDirectories);
				if (foundFiles.Length != 0)
				{
					return foundFiles[0].FullName;
				}
			}

			// 4. 手动选择
			using (var dlg = new FrmJarPathPrompt())
			{
				var result = dlg.ShowDialog();
				if (result == DialogResult.OK)
				{
					JavaPath = dlg.SelectedPath;
					SaveConfig();
					return dlg.SelectedPath;
				}
			}
			return null;
		}

		public void SetJavaPath(string path)
		{
			JavaPath = path;
			SaveConfig();
		}

		public void SetOutputPath(string path)
		{
			OutputPath = path;
			SaveConfig();
		}

		public string GetEffectiveOutputPath()
		{
			string outputPath = OutputPath;
			if (string.IsNullOrEmpty(outputPath))
				outputPath = Path.Combine(MreDirPath, "mre-output");
			return outputPath;
		}
	}
}