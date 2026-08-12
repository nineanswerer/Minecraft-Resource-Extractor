using mre.model;
using mre.view;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace mre.controller
{
	public class Controller
	{
		public Settings settings { get; }
		private FrmMre view { get; }
		public Target target { get; set; }

		public Controller(FrmMre view)
		{
			this.view = view;
			settings = Settings.GetInstance();
		}

		public void SetCustomMcPath(string path)
		{
			target = new Minecraft();
			((Minecraft)target).McPath = path;
			settings.LastMcPath = path;
			settings.SaveConfig();
			view.txtPath.Text = path;
			if (((Minecraft)target).FindMcVersions(path))
			{
				view.cmbVersions.Items.Clear();
				foreach (var ver in ((Minecraft)target).Versions)
				{
					view.cmbVersions.Items.Add(ver);
				}
				view.Log("已找到 .minecraft 文件夹。请选择要提取资源的版本。");
				StepsController.Step2(view);
			}
			else
			{
				MessageBox.Show(
					"错误：未找到 versions 文件夹！\n\n" +
					"请确保选择的文件夹是正确的 .minecraft 文件夹，且游戏至少启动过一次。",
					"错误",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error
				);
				view.Log("未找到正确的文件夹，请确保选择的 .minecraft 文件夹正确且游戏至少启动过一次。", "DarkRed");
			}
		}

		public string GetJavaPath()
		{
			return settings.JavaPath;
		}

		public List<string> GetJarFolders()
		{
			return target.Jar.ListJarFolders(settings.JavaPath, view);
		}

		public void LocateMinecraft()
		{
			target = new Minecraft();

			// 优先使用记忆的路径
			if (!string.IsNullOrEmpty(settings.LastMcPath) && Directory.Exists(settings.LastMcPath))
			{
				view.txtPath.Text = settings.LastMcPath;
				if (((Minecraft)target).FindMcVersions(settings.LastMcPath))
				{
					((Minecraft)target).McPath = settings.LastMcPath;
					view.cmbVersions.Items.Clear();
					foreach (var ver in ((Minecraft)target).Versions)
					{
						view.cmbVersions.Items.Add(ver);
					}
					view.Log("已找到 .minecraft 文件夹（使用上次路径）。请选择要提取资源的版本。");
					StepsController.Step2(view);
					return;
				}
			}

			// 尝试默认路径
			if (((Minecraft)target).McPath != null && ((Minecraft)target).Versions != null)
			{
				view.txtPath.Text = ((Minecraft)target).McPath;
				view.cmbVersions.Items.Clear();
				foreach (var ver in ((Minecraft)target).Versions)
				{
					view.cmbVersions.Items.Add(ver);
				}
				view.Log("请选择要提取资源的版本。");
				StepsController.Step2(view);
			}
			else
			{
				view.Log("无法自动定位您的 .minecraft 文件夹。请手动提供路径或直接输入路径后点击按钮。", "DarkRed");
				// 如果有保存的路径但无效，在 txtPath 中显示
				if (!string.IsNullOrEmpty(settings.LastMcPath))
					view.txtPath.Text = settings.LastMcPath;

				FolderBrowserDialog browse = new FolderBrowserDialog
				{
					Description = "请选择您的 .minecraft 文件夹"
				};
				if (browse.ShowDialog() == DialogResult.OK)
				{
					if (((Minecraft)target).FindMcPath(browse.SelectedPath) && ((Minecraft)target).FindMcVersions(browse.SelectedPath))
					{
						settings.LastMcPath = ((Minecraft)target).McPath;
						settings.SaveConfig();
						view.txtPath.Text = ((Minecraft)target).McPath;
						view.cmbVersions.Items.Clear();
						foreach (var ver in ((Minecraft)target).Versions)
						{
							view.cmbVersions.Items.Add(ver);
						}
						view.Log("已成功定位 .minecraft 文件夹。请选择要提取资源的版本。");
						StepsController.Step2(view);
					}
					else
					{
						MessageBox.Show(
							"错误：未找到 versions 文件夹！\n\n" +
							"请确保您选择了正确的文件夹，且游戏至少启动过一次。\n\n" +
							"您的 .minecraft 文件夹通常位于 \"C:\\Users\\您的用户名\\AppData\\Roaming\\.minecraft\"",
							"错误",
							MessageBoxButtons.OK,
							MessageBoxIcon.Error
						);
						view.Log("您似乎没有提供正确的文件夹，请确保选择了 .minecraft 文件夹且游戏至少启动过一次。", "DarkRed");
					}
				}
			}
		}

		public void LocateJarFile()
		{
			target = new Target();
			OpenFileDialog browse = new OpenFileDialog
			{
				Filter = "Jar 文件 (*.jar)|*.jar",
				Title = "请选择一个 .jar 文件"
			};
			if (browse.ShowDialog() == DialogResult.OK)
			{
				target.Jar = new JarFile(browse.FileName);
				view.txtPath.Text = browse.FileName;
				view.FillCheckedBox(view.chkExtFolders, target.Jar.ListJarFolders(settings.JavaPath, view));
				view.SetCheckAll(view.chkExtFolders, true);
				FillResourceTypes();
				view.Log("已找到 jar 文件夹，请选择您想要提取的文件夹或资源类型。");
				StepsController.Step4(view);
			}
		}

		public void SelectJarFile()
		{
			target = new Target();
			OpenFileDialog browse = new OpenFileDialog
			{
				Filter = "Jar 文件 (*.jar)|*.jar",
				Title = "请选择一个 .jar 文件"
			};
			if (browse.ShowDialog() == DialogResult.OK)
			{
				LoadJarFile(browse.FileName);
			}
		}

		/// <summary>
		/// 加载指定的 jar 文件（支持手动输入路径）
		/// </summary>
		public void LoadJarFile(string filePath)
		{
			target = new Target();
			target.Jar = new JarFile(filePath);
			view.txtPath.Text = filePath;
			view.FillCheckedBox(view.chkExtFolders, target.Jar.ListJarFolders(settings.JavaPath, view));
			view.SetCheckAll(view.chkExtFolders, true);
			FillResourceTypes();
			view.Log("已加载 jar 文件，请选择要提取的文件夹或资源类型。");
			view.Log("找到 " + target.Jar.AllEntries.Count + " 个文件，可用资源类型 " + view.chkResourceTypes.Items.Count + " 种。");
			StepsController.Step4(view);
		}

		public void FindVersionJar(string version)
		{
			((Minecraft)target).TargetVersion = version;
			target.Jar = new JarFile(((Minecraft)target).McPath
				+ "\\versions\\"
				+ version + "\\"
				+ version + ".jar");
			StepsController.Step3(view);
		}

		public void CheckVersionJar()
		{
			if (!File.Exists(target.Jar.Path))
			{
				Directory.CreateDirectory(settings.MreDirPath + "\\mre-tmp");
				string jsonPath = target.Jar.Path.Replace(".jar", ".json");
				string jsonString = File.ReadAllText(jsonPath);
				string jarUrl = (string)JObject.Parse(jsonString).SelectToken("downloads.client.url");
				target.Jar.Path = settings.MreDirPath + "\\mre-tmp\\" + target.Jar.FullName;
				view.Log("未找到版本 " + ((Minecraft)target).TargetVersion + " 的 jar 文件。正在下载...");
				StartDownload(jarUrl, target.Jar.Path, JarDownloadCompleteEvent);
			}
			else
			{
				view.FillCheckedBox(view.chkExtFolders, GetJarFolders());
				view.SetCheckAll(view.chkExtFolders, true);
				FillResourceTypes();
				view.Log("已找到 jar 文件夹，请选择您想要提取的文件夹或资源类型。");
				StepsController.Step4(view);
			}
		}

		private void StartDownload(string url, string fileName, AsyncCompletedEventHandler completedHandler)
		{
			Thread thread = new Thread(() =>
			{
				WebClient client = new WebClient();
				client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressEvent);
				client.DownloadFileCompleted += new AsyncCompletedEventHandler(completedHandler);
				client.DownloadFileAsync(new Uri(url), fileName);
				client.Dispose();
			});
			thread.Start();
		}

		private void DownloadProgressEvent(object sender, DownloadProgressChangedEventArgs e)
		{
			view.BeginInvoke((MethodInvoker)delegate
			{
				double bytesIn = double.Parse(e.BytesReceived.ToString());
				double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
				double percentage = bytesIn / totalBytes * 100;
				view.Status("正在下载 " + e.BytesReceived / 1000 + "/" + e.TotalBytesToReceive / 1000 + " KB");
				view.pgbProgress.Value = int.Parse(Math.Truncate(percentage).ToString());
			});
		}

		private void JarDownloadCompleteEvent(object sender, AsyncCompletedEventArgs e)
		{
			view.BeginInvoke((MethodInvoker)delegate
			{
				view.Log("Jar 文件下载完成。");
				view.FillCheckedBox(view.chkExtFolders, GetJarFolders());
				view.SetCheckAll(view.chkExtFolders, true);
				FillResourceTypes();
				view.Log("已找到 jar 文件夹，请选择您想要提取的文件夹或资源类型。");
				StepsController.Step4(view);
			});
		}

		private void IndexDownloadCompleteEvent(object sender, AsyncCompletedEventArgs e)
		{
			view.BeginInvoke((MethodInvoker)delegate
			{
				view.Status("完成");
				view.Log("索引文件下载完成。");
				ReadAssets();
				MoveAssets();
			});
		}

		public void ExtractJarFolder(string folder)
		{
			view.Status("正在从 jar 中提取文件夹 " + folder + " ...");
			view.SwitchUiLock(4);
			target.Jar.ExtractJarFolder(folder, settings);
			view.SwitchUiLock(4);
			view.Status("Jar 提取完成");
		}

		public void ExtractResourceType(ResourceType type)
		{
			string displayName = ResourceTypes.GetDisplayName(type);
			view.Status("正在提取 " + displayName + " ...");
			view.SwitchUiLock(4);
			try
			{
				target.Jar.ExtractResourceType(type, settings);
			}
			finally
			{
				view.SwitchUiLock(4);
			}
			view.Status(displayName + " 提取完成");
		}

		public void FillResourceTypes()
		{
			view.chkResourceTypes.Items.Clear();

			// 根据 jar 内容过滤：只显示 jar 中实际存在的资源类型
			List<ResourceTypeInfo> availableTypes;
			if (target != null && target.Jar != null)
				availableTypes = target.Jar.GetAvailableResourceTypes();
			else
				availableTypes = new List<ResourceTypeInfo>(ResourceTypes.AllTypes);

			foreach (var info in availableTypes)
			{
				view.chkResourceTypes.Items.Add(info.Type);
			}

			// 根据项目数量自动调整列表高度
			int itemCount = view.chkResourceTypes.Items.Count;
			int itemHeight = view.chkResourceTypes.ItemHeight;
			int desiredHeight = itemCount * itemHeight + 4; // +4 for border
			int maxHeight = 130; // 最大高度，确保下方控件可见
			int minHeight = 34;  // 最小高度（约2行）

			if (desiredHeight > maxHeight)
				desiredHeight = maxHeight;
			if (desiredHeight < minHeight)
				desiredHeight = minHeight;

			view.chkResourceTypes.Height = desiredHeight;

			// 动态调整下方控件位置，防止覆盖
			view.LayoutStep4Controls();
		}

		public string GetOutputPath()
		{
			string outputPath = settings.OutputPath;
			if (string.IsNullOrEmpty(outputPath))
				outputPath = Path.Combine(settings.MreDirPath, "mre-output");
			if (!Directory.Exists(outputPath))
				Directory.CreateDirectory(outputPath);
			return outputPath;
		}

		public void SetOutputPath(string path)
		{
			settings.SetOutputPath(path);
		}

		public void GetAssets()
		{
			Minecraft assetsTarget = (Minecraft)target;
			assetsTarget.AssetsFiles = new Assets();
			if (!assetsTarget.AssetsFiles.FindAssetsIndex(assetsTarget.TargetVersion, assetsTarget.McPath))
			{
				view.Log("未找到索引文件，将会下载，但这意味着某些文件可能会缺失！您应该先启动一次这个版本以确保您有所需的全部文件。", "DarkRed");
				if (!Directory.Exists(settings.MreDirPath + "\\mre-tmp"))
					Directory.CreateDirectory(settings.MreDirPath + "\\mre-tmp");
				string jsonPath = assetsTarget.McPath + "\\versions\\" + assetsTarget.TargetVersion + "\\" + assetsTarget.TargetVersion + ".json";
				string jsonString = File.ReadAllText(jsonPath);
				string jarUrl = (string)JObject.Parse(jsonString).SelectToken("assetIndex.url");
				assetsTarget.AssetsFiles.IndexPath = settings.MreDirPath + "\\mre-tmp\\" + assetsTarget.AssetsFiles.Version + ".json";
				StartDownload(jarUrl, assetsTarget.AssetsFiles.IndexPath, IndexDownloadCompleteEvent);
			}
			else
			{
				ReadAssets();
				MoveAssets();
			}
		}

		public void ReadAssets()
		{
			Assets assets = ((Minecraft)target).AssetsFiles;
			string jsonString = File.ReadAllText(assets.IndexPath);
			var obj = JObject.Parse(jsonString).SelectToken("objects").ToObject<JObject>();
			foreach (var x in obj)
			{
				assets.Hashes.Add(x.Key.Replace('/', '\\'), x.Value.SelectToken("hash").Value<string>());
			}
		}

		public void MoveAssets()
		{
			Minecraft mc = (Minecraft)target;
			int missingFiles = 0;
			view.Log("开始提取资源文件，这可能需要一些时间，请耐心等待...");
			foreach (var obj in mc.AssetsFiles.Hashes)
			{
				string folder = obj.Value.Substring(0, 2);
				string hashPath = mc.McPath + "\\assets\\objects\\" + folder + "\\" + obj.Value;
				string objPath = settings.GetEffectiveOutputPath() + "\\" + mc.TargetVersion + "-assets\\" + obj.Key;
				Directory.CreateDirectory(objPath.Substring(0, objPath.LastIndexOf('\\')));
				try
				{
					File.Copy(hashPath, objPath, true);
				}
				catch (FileNotFoundException)
				{
					missingFiles++;
				}
			}
			if (missingFiles > 0)
				view.Log("成功复制 " + (mc.AssetsFiles.Hashes.Count - missingFiles) + " 个资源文件，但有 " + missingFiles + " 个文件缺失！", "DarkRed");
			else
				view.Log("成功复制 " + (mc.AssetsFiles.Hashes.Count - missingFiles) + " 个资源文件！您的文件位于 " + settings.GetEffectiveOutputPath() + " 文件夹中。", "DarkGreen");
			view.Status("操作完成！");
			view.Log("感谢您使用 Minecraft 资源提取器！", "DarkGreen");
			Process.Start("explorer.exe", settings.GetEffectiveOutputPath());
		}

		/// <summary>
		/// 加载 Mods 目录：扫描 jar 文件并准备批量提取
		/// </summary>
		public void LoadModsDirectory(string dir)
		{
			var extractor = new ModBatchExtractor(settings.JavaPath);
			List<string> jars = extractor.ScanJarFiles(dir);

			if (jars.Count == 0)
			{
				view.Log("在目录 " + dir + " 中未找到 .jar 文件。请检查目录是否正确。", "DarkRed");
				return;
			}

			view.Log("在 " + dir + " 中发现 " + jars.Count + " 个 .jar 文件。");

			// 存储扫描结果供后续提取使用
			_currentBatchDir = dir;
			_currentBatchJars = jars;

			// 从所有 jar 中聚合资源类型信息，填充步骤4的选择列表
			AggregateResourceTypesFromJars(jars);
			FillResourceTypes();

			view.Log("输出目录: " + GetOutputPath());
			view.Log("请在上方勾选要提取的资源类型，然后点击[确认]按钮开始批量提取。");

			StepsController.Step4(view);
		}

		/// <summary>
		/// 从扫描到的所有 jar 文件中聚合资源类型路径信息
		/// </summary>
		private void AggregateResourceTypesFromJars(List<string> jarPaths)
		{
			// 收集所有 jar 的条目
			var allEntries = new List<string>();
			foreach (string jarPath in jarPaths)
			{
				try
				{
					var jar = new JarFile(jarPath);
					// 只列出条目，不完整加载
					jar.ListJarFolders(settings.JavaPath, view);
					if (jar.AllEntries != null)
						allEntries.AddRange(jar.AllEntries);
				}
				catch
				{
					// 跳过无法读取的 jar
				}
			}

			// 更新全局资源类型路径
			if (allEntries.Count > 0)
				ResourceTypes.UpdateJarPaths(allEntries);

			// 同时存储聚合信息到第一个有效的 jar（供 FillResourceTypes 使用）
			if (target == null)
				target = new Target();
			if (target.Jar == null && jarPaths.Count > 0)
			{
				try
				{
					target.Jar = new JarFile(jarPaths[0]);
					target.Jar.ListJarFolders(settings.JavaPath, view);
					// 使用聚合条目覆盖
					target.Jar.SetAllEntries(allEntries);
				}
				catch { }
			}
		}

		/// <summary>
		/// 执行批量提取
		/// </summary>
		public void RunBatchExtraction(string inputDir, string outputDir)
		{
			view.SwitchUiLock(4);
			view.pgbProgress.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			view.Status("正在批量提取 Mod 资源...");

			try
			{
				var extractor = new ModBatchExtractor(settings.JavaPath);

				BatchResult result = extractor.Run(
					inputDir,
					outputDir,
					onJarStart: (jarName) =>
					{
						view.Status("正在处理: " + jarName + " ...");
					},
					onJarDone: (jarName, fileCount) =>
					{
						view.Log("  ✓ " + jarName + " 提取完成 (" + fileCount + " 个文件)");
					},
					onJarError: (jarName, error) =>
					{
						view.Log("  ✗ " + jarName + " 提取失败: " + error, "DarkRed");
					}
				);

				// 保存冲突报告
				if (result.ConflictCount > 0)
				{
					result.SaveToFile(outputDir);
				}

				// 输出摘要
				LogBatchResult(result);

				view.Status("批量提取完成！");
				Process.Start("explorer.exe", outputDir);
			}
			catch (Exception ex)
			{
				view.Log("批量提取发生错误: " + ex.Message, "DarkRed");
				view.Status("批量提取失败");
			}
			finally
			{
				view.pgbProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
				view.pgbProgress.Value = 0;
				view.SwitchUiLock(4);
			}
		}

		/// <summary>
		/// 格式化输出批量提取结果摘要到日志面板
		/// </summary>
		private void LogBatchResult(BatchResult result)
		{
			view.Log("═══════════════════════════════════════");
			view.Log("  批量提取完成");
			view.Log("═══════════════════════════════════════");
			view.Log("  处理 Jar 总数: " + result.TotalJars);
			view.Log("  成功提取: " + result.SuccessfulJars, "DarkGreen");
			if (result.FailedJars > 0)
				view.Log("  提取失败: " + result.FailedJars, "DarkRed");
			view.Log("  提取资产总数: " + result.TotalAssetsExtracted);
			view.Log("  资源冲突数: " + result.ConflictCount, result.ConflictCount > 0 ? "Orange" : "DarkGreen");

			if (result.FailedJarList.Count > 0)
			{
				view.Log("  失败的 Jar 文件:", "DarkRed");
				foreach (var jar in result.FailedJarList)
					view.Log("    ✗ " + jar, "DarkRed");
			}

			if (result.Conflicts.Count > 0)
			{
				view.Log("  资源冲突详情 (详见 conflict_report.json):", "Orange");
				int displayCount = result.Conflicts.Count > 15 ? 15 : result.Conflicts.Count;
				for (int i = 0; i < displayCount; i++)
				{
					var entry = result.Conflicts[i];
					view.Log("    ⚠ " + entry.AssetPath, "Orange");
					view.Log("      涉及: " + string.Join(", ", entry.SourceJars));
				}
				if (result.Conflicts.Count > 15)
					view.Log("    ... 还有 " + (result.Conflicts.Count - 15) + " 条冲突");
			}
			else
			{
				view.Log("  ✓ 未检测到资源冲突", "DarkGreen");
			}

			view.Log("  详细报告已保存至: " + System.IO.Path.Combine(result.OutputDirectory, "conflict_report.json"));
			view.Log("═══════════════════════════════════════");
		}

		private string _currentBatchDir;
		private List<string> _currentBatchJars;

		/// <summary>
		/// 按选中的资源类型执行批量提取
		/// </summary>
		public void RunBatchExtractionFiltered(string inputDir, string outputDir, List<ResourceType> selectedTypes)
		{
			if (_currentBatchJars == null || _currentBatchJars.Count == 0)
			{
				view.Log("没有可提取的 jar 文件，请先扫描 Mods 目录。", "DarkRed");
				return;
			}

			bool groupByType = view.chkGroupByType != null && view.chkGroupByType.Checked;
			view.SwitchUiLock(4);
			try
			{
			int totalJars = _currentBatchJars.Count;
			int totalSteps = totalJars * selectedTypes.Count;
			int currentStep = 0;
			int successCount = 0;
			var conflicts = new Dictionary<string, List<string>>();

			view.pgbProgress.Style = ProgressBarStyle.Blocks;
			view.pgbProgress.Minimum = 0;
			view.pgbProgress.Maximum = totalSteps;
			view.pgbProgress.Value = 0;

			for (int i = 0; i < _currentBatchJars.Count; i++)
			{
				string jarPath = _currentBatchJars[i];
				string jarName = System.IO.Path.GetFileName(jarPath);
				view.Status("正在处理: " + jarName + " (" + (i + 1) + "/" + totalJars + ") ...");

				try
				{
					var jar = new JarFile(jarPath);
					jar.ListJarFolders(settings.JavaPath, view);

					// 检测冲突
					if (jar.AllEntries != null)
					{
						foreach (string entry in jar.AllEntries)
						{
							string normalized = entry.Replace('\\', '/');
							if (!conflicts.ContainsKey(normalized))
								conflicts[normalized] = new List<string>();
							if (!conflicts[normalized].Contains(jarName))
								conflicts[normalized].Add(jarName);
						}
					}

					// 按选中的资源类型提取
					foreach (ResourceType type in selectedTypes)
					{
						string typeName = ResourceTypes.GetDisplayName(type);
						view.Status("正在提取: " + jarName + " → " + typeName + " (" + (currentStep + 1) + "/" + totalSteps + ")");

						if (groupByType)
							jar.ExtractResourceType(type, settings, typeName);
						else
							jar.ExtractResourceType(type, settings);

						currentStep++;
						view.pgbProgress.Value = currentStep;
					}

					successCount++;
					view.Log("  ✓ " + jarName + " 提取完成", "DarkGreen");
				}
				catch (System.Exception ex)
				{
					// 跳过已失败的类型，但仍更新进度
					currentStep += selectedTypes.Count;
					if (currentStep > totalSteps) currentStep = totalSteps;
					try { view.pgbProgress.Value = currentStep; } catch { }
					view.Log("  ✗ " + jarName + " 提取失败: " + ex.Message, "DarkRed");
				}
			}

			// 统计冲突
			var actualConflicts = new List<ConflictEntry>();
			foreach (var kvp in conflicts)
			{
				if (kvp.Value.Count >= 2)
				{
					actualConflicts.Add(new ConflictEntry
					{
						AssetPath = kvp.Key,
						SourceJars = kvp.Value
					});
				}
			}
			actualConflicts = actualConflicts
				.OrderByDescending(c => c.SourceJars.Count)
				.ThenBy(c => c.AssetPath)
				.ToList();

			// 保存冲突报告
			if (actualConflicts.Count > 0)
			{
				var result = new BatchResult
				{
					OutputDirectory = outputDir,
					TotalJars = totalJars,
					SuccessfulJars = successCount,
					FailedJars = totalJars - successCount,
					TotalAssetsExtracted = currentStep,
					ConflictCount = actualConflicts.Count,
					Conflicts = actualConflicts
				};
				result.SaveToFile(outputDir);
				view.Log("⚠ 检测到 " + actualConflicts.Count + " 个资源冲突，详情见 conflict_report.json", "Orange");
			}
			else
			{
				view.Log("✓ 未检测到资源冲突", "DarkGreen");
			}

			view.Log("═══════════════════════════════════════");
			view.Log("  批量提取完成！成功: " + successCount + "/" + totalJars, "DarkGreen");
			view.Log("═══════════════════════════════════════");

			view.pgbProgress.Style = ProgressBarStyle.Continuous;
			view.pgbProgress.Value = 0;
			view.Status("批量提取完成！");

			System.Diagnostics.Process.Start("explorer.exe", outputDir);
			}
			finally
			{
				view.SwitchUiLock(4);
				view.pgbProgress.Style = ProgressBarStyle.Continuous;
				view.Cursor = System.Windows.Forms.Cursors.Default;
			}
		}
	}
}