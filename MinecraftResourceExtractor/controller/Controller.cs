using mre.model;
using mre.view;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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
				view.PopulateDirectoryTree(target.Jar);
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
		/// 加载指定的 jar 文件（支持手动输入路径，后台线程扫描避免 UI 卡死）
		/// </summary>
		public void LoadJarFile(string filePath)
		{
			target = new Target();
			target.Jar = new JarFile(filePath);
			view.txtPath.Text = filePath;

			view.Cursor = Cursors.WaitCursor;
			Task.Run(() =>
			{
				try
				{
					// 快速扫描（ZipArchive，不启动 Java 进程）
					target.Jar.ListJarContentsFast();

					// 后台构建目录树索引，避免在 UI 线程遍历海量条目
					view.Status("正在生成目录树...");
					view.BuildDirectoryTreeIndex(target.Jar);

					view.BeginInvoke((MethodInvoker)(() =>
					{
						view.FillCheckedBox(view.chkExtFolders, target.Jar.Folders);
						view.SetCheckAll(view.chkExtFolders, true);
						FillResourceTypes();

						// 自动检测 MC 版本并填充 pack 版本下拉框
						string detected = PackMcMeta.DetectMcVersion(target.Jar.FullName);
						if (!string.IsNullOrEmpty(detected))
						{
							view.SetPackVersion(detected);
							view.Log("已自动检测到 MC 版本 " + detected + "，将生成对应 pack.mcmeta。");
						}

						view.Log("已加载 jar 文件，请选择要提取的文件夹或资源类型。");
						view.Log("找到 " + target.Jar.AllEntries.Count + " 个文件，可用资源类型 " + view.chkResourceTypes.Items.Count + " 种。");
						view.PopulateDirectoryTree(target.Jar);
						StepsController.Step4(view);
					}));
				}
				catch (Exception ex)
				{
					view.BeginInvoke((MethodInvoker)(() =>
						view.Log("加载 jar 文件失败: " + ex.Message, "DarkRed")));
				}
				finally
				{
					view.BeginInvoke((MethodInvoker)(() => view.Cursor = Cursors.Default));
				}
			});
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
				view.PopulateDirectoryTree(target.Jar);
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
				view.PopulateDirectoryTree(target.Jar);
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

		/// <summary>
		/// 异步提取选中的资源类型和文件夹（后台线程，避免 UI 卡死）
		/// </summary>
		public void ExtractSelectedResources(List<ResourceType> types, List<string> folders)
		{
			view.SwitchUiLock(4);
			view.pgbProgress.Style = ProgressBarStyle.Marquee;

			// 在 UI 线程读取 pack.mcmeta 生成选项（后台线程只用副本，避免跨线程访问控件）
			bool generatePack = view.chkGeneratePackMcMeta != null && view.chkGeneratePackMcMeta.Checked;
			string packVersion = (view.cmbPackFormat != null && view.cmbPackFormat.SelectedItem != null)
				? view.cmbPackFormat.SelectedItem.ToString()
				: PackMcMeta.DefaultVersion;

			Task.Run(() =>
			{
				try
				{
					if (types != null && types.Count > 0)
					{
						foreach (ResourceType type in types)
						{
							string displayName = ResourceTypes.GetDisplayName(type);
							view.Status("正在提取 " + displayName + " ...");
							target.Jar.ExtractResourceType(type, settings);
						}
					}

					if (folders != null && folders.Count > 0)
					{
						foreach (string folder in folders)
						{
							view.Status("正在从 jar 中提取文件夹 " + folder + " ...");
							target.Jar.ExtractJarFolder(folder, settings);
						}
					}

					// 生成 pack.mcmeta（可选，让输出目录成为可用资源包）
					if (generatePack)
					{
						string packDir = Path.Combine(GetOutputPath(), target.Jar.Name);
						PackMcMeta.Generate(packDir, packVersion);
						view.Log("已生成 pack.mcmeta（" + PackMcMeta.DescribeFormat(packVersion) + "）", "DarkGreen");
					}

					view.Log("成功从 jar 文件中提取内容！您的文件位于 " + GetOutputPath() + " 文件夹中。", "DarkGreen");
				}
				catch (Exception ex)
				{
					view.Log("提取发生错误: " + ex.Message, "DarkRed");
				}
				finally
				{
					view.BeginInvoke((MethodInvoker)(() =>
					{
						view.pgbProgress.Style = ProgressBarStyle.Continuous;
						view.pgbProgress.Value = 0;
						view.SwitchUiLock(4);
						view.Cursor = Cursors.Default;
						view.Status("提取完成");
					}));
				}
			});
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

			// 刷新预计提取文件数
			view.UpdateFileCountPreview();
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

			_currentBatchDir = dir;
			view.Log("在 " + dir + " 中发现 " + jars.Count + " 个 .jar 文件。");
			LoadJarFiles(jars);
		}

		/// <summary>
		/// 加载一组 jar 文件并准备批量提取（供 Mods 目录扫描或拖放多个文件使用）
		/// 弹出进度窗口，后台并行扫描 + 构建目录树索引，完成后回填 UI。
		/// </summary>
		public void LoadJarFiles(List<string> jarPaths)
		{
			if (jarPaths == null || jarPaths.Count == 0)
			{
				view.Log("没有可加载的 jar 文件。", "DarkRed");
				return;
			}

			_currentBatchJars = jarPaths;

			// 弹出进度窗口
			var dlg = new FrmProgress("正在加载 Mod 资源");
			dlg.SetRange(0, Math.Max(1, jarPaths.Count));
			dlg.SetMessage("正在扫描 " + jarPaths.Count + " 个 jar 文件...");

			view.Enabled = false;
			dlg.Show(view);

			// 后台执行扫描 + 目录树索引构建（不阻塞 UI）
			Task.Run(() =>
			{
				try
				{
					AggregateResourceTypesFromJars(jarPaths, dlg);

					dlg.BeginInvoke((MethodInvoker)(() =>
					{
						dlg.SetMarquee(true);
						dlg.SetMessage("正在生成目录树...");
					}));
					view.BuildDirectoryTreeIndex(target?.Jar);
				}
				catch (Exception ex)
				{
					view.Log("加载 jar 失败: " + ex.Message, "DarkRed");
				}
				finally
				{
					dlg.BeginInvoke((MethodInvoker)(() =>
					{
						dlg.Close();
						dlg.Dispose();
						view.Enabled = true;

						FillResourceTypes();

						// 自动检测 MC 版本（用第一个 jar 的文件名）
						string detected = PackMcMeta.DetectMcVersion(System.IO.Path.GetFileName(jarPaths[0]));
						if (!string.IsNullOrEmpty(detected))
						{
							view.SetPackVersion(detected);
							view.Log("已自动检测到 MC 版本 " + detected + "，将生成对应 pack.mcmeta。");
						}

						// 批量模式目录树根节点显示为聚合信息
						if (target?.Jar != null)
							target.Jar.SetDisplayName("批量 Mod（" + jarPaths.Count + " 个 jar）");

						view.Log("输出目录: " + GetOutputPath());
						view.Log("请在上方勾选要提取的资源类型，然后点击[确认]按钮开始批量提取。");

						view.PopulateDirectoryTree(target?.Jar);
						StepsController.Step4(view);
					}));
				}
			});
		}

		/// <summary>
		/// 从扫描到的所有 jar 文件中聚合资源类型路径信息（并行 + ZipArchive 快速扫描）
		/// 性能：200 个 jar 从 ~60 秒降至 ~2 秒；进度通过 dlg 回传到进度弹窗
		/// </summary>
		private void AggregateResourceTypesFromJars(List<string> jarPaths, FrmProgress dlg)
		{
			var resultLists = new ConcurrentBag<List<string>>();
			int scanned = 0;
			int failed = 0;
			int total = jarPaths.Count;

			// 预留一个核给 UI 线程，避免全核并行导致界面冻结
			int maxParallel = Math.Max(1, Environment.ProcessorCount - 1);

			Parallel.ForEach(jarPaths, new ParallelOptions { MaxDegreeOfParallelism = maxParallel }, jarPath =>
			{
				try
				{
					var jar = new JarFile(jarPath);
					jar.ListJarEntriesFast();
					if (jar.AllEntries != null && jar.AllEntries.Count > 0)
						resultLists.Add(jar.AllEntries); // 整个列表一次添加，减少同步开销
				}
				catch
				{
					Interlocked.Increment(ref failed);
				}

				int current = Interlocked.Increment(ref scanned);
				if (current == 1 || current % 5 == 0 || current == total)
				{
					int c = current;
					dlg.BeginInvoke((MethodInvoker)(() =>
					{
						dlg.SetValue(c);
						dlg.SetMessage("正在扫描 " + c + "/" + total + " 个 jar ...");
					}));
				}
			});

			// 合并所有列表（单线程，快速）
			var allEntriesList = new List<string>(resultLists.Sum(l => l.Count));
			foreach (var list in resultLists)
				allEntriesList.AddRange(list);
			if (failed > 0)
				view.Log(failed + " 个 jar 文件无法读取，已跳过", "Orange");

			view.Log("扫描完成！共发现 " + allEntriesList.Count + " 个文件条目（来自 " + (total - failed) + " 个 jar）");

			// 分析资源类型（SetAllEntries 内部会调用 UpdateJarPaths）
			dlg.BeginInvoke((MethodInvoker)(() =>
			{
				dlg.SetMarquee(true);
				dlg.SetMessage("正在分析资源类型...");
			}));

			// 存储聚合信息（供 FillResourceTypes 和目录树使用）
			// 注意：必须无条件覆盖 target.Jar，否则之前加载过单 jar 后 target.Jar 非空，
			// 再拖入文件夹时目录树不会更新成聚合条目
			if (target == null)
				target = new Target();
			if (jarPaths.Count > 0)
			{
				try
				{
					target.Jar = new JarFile(jarPaths[0]);
					target.Jar.SetAllEntries(allEntriesList);
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
		/// 按选中的资源类型执行批量提取（后台线程，避免 UI 卡死）
		/// </summary>
		public void RunBatchExtractionFiltered(string inputDir, string outputDir, List<ResourceType> selectedTypes)
		{
			if (_currentBatchJars == null || _currentBatchJars.Count == 0)
			{
				view.Log("没有可提取的 jar 文件，请先扫描 Mods 目录。", "DarkRed");
				return;
			}

			bool groupByType = view.chkGroupByType != null && view.chkGroupByType.Checked;
			bool generatePack = view.chkGeneratePackMcMeta != null && view.chkGeneratePackMcMeta.Checked;
			string packVersion = (view.cmbPackFormat != null && view.cmbPackFormat.SelectedItem != null)
				? view.cmbPackFormat.SelectedItem.ToString()
				: PackMcMeta.DefaultVersion;
			int totalJars = _currentBatchJars.Count;
			int totalSteps = totalJars * selectedTypes.Count;

			view.SwitchUiLock(4);
			view.pgbProgress.Style = ProgressBarStyle.Blocks;
			view.SetProgressRange(0, Math.Max(1, totalSteps));

			Task.Run(() =>
			{
				try
				{
					int currentStep = 0;
					int successCount = 0;
					var conflicts = new Dictionary<string, List<string>>();

					for (int i = 0; i < _currentBatchJars.Count; i++)
					{
						string jarPath = _currentBatchJars[i];
						string jarName = System.IO.Path.GetFileName(jarPath);
						view.Status("正在处理: " + jarName + " (" + (i + 1) + "/" + totalJars + ") ...");

						try
						{
							var jar = new JarFile(jarPath);
							jar.ListJarEntriesFast();

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
								view.SetProgress(currentStep);
							}

							successCount++;

							// 未分类输出时，每个 jar 目录都是一个资源包，生成 pack.mcmeta
							if (generatePack && !groupByType)
								PackMcMeta.Generate(System.IO.Path.Combine(outputDir, jar.Name), packVersion);

							view.Log("  ✓ " + jarName + " 提取完成", "DarkGreen");
						}
						catch (System.Exception ex)
						{
							// 跳过已失败的类型，但仍更新进度
							currentStep += selectedTypes.Count;
							if (currentStep > totalSteps) currentStep = totalSteps;
							view.SetProgress(currentStep);
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

					view.Status("批量提取完成！");

					System.Diagnostics.Process.Start("explorer.exe", outputDir);
				}
				catch (Exception ex)
				{
					view.Log("批量提取发生错误: " + ex.Message, "DarkRed");
				}
				finally
				{
					view.BeginInvoke((MethodInvoker)(() =>
					{
						view.pgbProgress.Style = ProgressBarStyle.Continuous;
						view.pgbProgress.Value = 0;
						view.SwitchUiLock(4);
						view.Cursor = System.Windows.Forms.Cursors.Default;
					}));
				}
			});
		}
	}
}