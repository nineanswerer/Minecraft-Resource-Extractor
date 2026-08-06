using mre.model;
using mre.view;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
				view.Log("无法自动定位您的 .minecraft 文件夹。请手动提供路径。", "DarkRed");
				FolderBrowserDialog browse = new FolderBrowserDialog
				{
					Description = "请选择您的 .minecraft 文件夹"
				};
				if (browse.ShowDialog() == DialogResult.OK)
				{
					if (((Minecraft)target).FindMcPath(browse.SelectedPath) && ((Minecraft)target).FindMcVersions(browse.SelectedPath))
					{
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
				view.Log("已找到 jar 文件夹，请选择您想要提取的文件夹。");
				StepsController.Step4(view);
			}
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
				view.Log("已找到 jar 文件夹，请选择您想要提取的文件夹。");
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
				view.Log("已找到 jar 文件夹，请选择您想要提取的文件夹。");
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
				string objPath = settings.MreDirPath + "\\mre-output\\" + mc.TargetVersion + "-assets\\" + obj.Key;
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
				view.Log("成功复制 " + (mc.AssetsFiles.Hashes.Count - missingFiles) + " 个资源文件！您的文件位于 \"mre-output\" 文件夹中。", "DarkGreen");
			view.Status("操作完成！");
			view.Log("感谢您使用 Minecraft 资源提取器！", "DarkGreen");
			Process.Start("explorer.exe", settings.MreDirPath + "\\mre-output");
		}
	}
}