using mre.controller;
using mre.model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace mre.view
{
	public partial class FrmMre : Form
	{
		public Controller controller { get; }
		public CheckBox chkGroupByType;  // 按资源类型分类输出
		public CheckBox chkGeneratePackMcMeta;  // 生成 pack.mcmeta
		public ComboBox cmbPackFormat;          // 目标 MC 版本
		private Label lblPackFormat;
		private SplitContainer splitHelp;
		private TreeView treeDirectory;
		private Label lblFileCountPreview;
		private bool _previewPending;
		private Dictionary<string, SortedSet<string>> _treeSubdirs;
		private Dictionary<string, SortedSet<string>> _treeFiles;

		// 现代配色方案
		private static readonly Color C_PRIMARY = Color.FromArgb(74, 144, 217);
		private static readonly Color C_PRIMARY_HOVER = Color.FromArgb(56, 120, 190);
		private static readonly Color C_SUCCESS = Color.FromArgb(82, 196, 26);
		private static readonly Color C_SUCCESS_HOVER = Color.FromArgb(62, 160, 18);
		private static readonly Color C_BG = Color.FromArgb(240, 244, 248);
		private static readonly Color C_SURFACE = Color.White;
		private static readonly Color C_TEXT = Color.FromArgb(30, 58, 95);
		private static readonly Color C_TEXT_SEC = Color.FromArgb(100, 116, 139);
		private static readonly Color C_BORDER = Color.FromArgb(208, 215, 222);
		private static readonly Color C_GB_HEADER = Color.FromArgb(30, 58, 95);

		public FrmMre()
		{
			InitializeComponent();

			// btnConfirm4 垂直位置由 LayoutStep4Controls 动态控制；
			// 去掉 Bottom 锚点，避免 grbStep4 高度变化时 Anchor 重定位把按钮移出可视区（批量模式按钮"消失"）
			btnConfirm4.Anchor = AnchorStyles.Top | AnchorStyles.Right;

			InitializeModsPanel();
			InitializeOutputOptions();
			InitializePackMcMetaOptions();
			InitializeSelectAllLinks();
			InitializeDirectoryTree();
			InitializeFileCountPreview();
			InitializeDragDrop();
			ApplyModernStyle();
			controller = new Controller(this);
			if (controller.GetJavaPath() == null)
			{
				MessageBox.Show("错误\n您需要先安装 Java JDK 才能使用此工具", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Load += (s, e) => Close();
				return;
			}
			Log("欢迎使用 Minecraft 资源提取器！");
			txtOutputPath.Text = controller.GetOutputPath();
			if (!string.IsNullOrEmpty(controller.settings.LastMcPath))
				txtPath.Text = controller.settings.LastMcPath;
			StepsController.Step1(this);
			Log("请选择是要从官方 Minecraft 版本提取资源，还是从单独的 jar 文件提取。");
		}

		/// <summary>
		/// 应用现代化扁平风格
		/// </summary>
		private void ApplyModernStyle()
		{
			// 窗体背景
			BackColor = C_BG;
			tlpMain.BackColor = C_BG;

			// 全局字体：优先微软雅黑
			Font modernFont = new Font("Microsoft YaHei", 9F, FontStyle.Regular);
			Font = modernFont;

			// 状态栏
			statusStrip1.BackColor = Color.FromArgb(240, 244, 248);
			statusStrip1.RenderMode = ToolStripRenderMode.Professional;
			statusStrip1.BackColor = C_SURFACE;

			// --- GroupBox 自定义绘制 ---
			StyleGroupBox(grbStep1);
			StyleGroupBox(grbStep2);
			StyleGroupBox(grbStep3);
			StyleGroupBox(grbStep4);
			StyleGroupBox(grbHelp);

			// --- 按钮样式 ---
			StyleButton(btnLocateJar, C_PRIMARY);        // 浏览 - 蓝色
		StyleButton(btnConfirm2, C_SUCCESS);        // 确认 - 绿色
		StyleButton(btnConfirm3, C_SUCCESS);        // 确认 - 绿色
		StyleButton(btnConfirm4, C_SUCCESS);        // 确认 - 绿色
		StyleButton(btnBrowseOutput, Color.FromArgb(108, 117, 125));  // 辅助 - 灰色
		StyleButton(btnBrowseModsDir, C_PRIMARY);   // 浏览 - 蓝色
		StyleButton(btnConfirm2Mods, C_SUCCESS);    // 扫描 - 绿色

			// --- 帮助面板 ---
			grbHelp.BackColor = C_SURFACE;
			rtbHelp.BackColor = Color.FromArgb(248, 250, 252);
			rtbHelp.BorderStyle = BorderStyle.None;
			rtbHelp.Font = new Font("Microsoft YaHei", 9F);

			// --- 链接标签 ---
			lnkAbout.LinkColor = C_PRIMARY;
			lnkAbout.ActiveLinkColor = C_PRIMARY_HOVER;
		}

		/// <summary>
		/// GroupBox 扁平化：去掉 3D 边框，改为细线 + 左侧色条
		/// </summary>
		private void StyleGroupBox(GroupBox gb)
		{
			gb.BackColor = C_SURFACE;
			gb.Paint += (sender, e) =>
			{
				GroupBox box = (GroupBox)sender;
				e.Graphics.Clear(C_SURFACE);

				// 顶部色条
				using (Brush barBrush = new SolidBrush(C_PRIMARY))
				{
					e.Graphics.FillRectangle(barBrush, 0, 0, box.Width, 3);
				}

				// 边框
				using (Pen pen = new Pen(C_BORDER, 1))
				{
					e.Graphics.DrawRectangle(pen, 0, 0, box.Width - 1, box.Height - 1);
				}

				// 标题文字
				SizeF textSize = e.Graphics.MeasureString(box.Text, box.Font);
				using (Brush textBrush = new SolidBrush(C_GB_HEADER))
				{
					e.Graphics.DrawString(box.Text, new Font(box.Font, FontStyle.Bold), textBrush, 10, 6);
				}
			};
		}

		/// <summary>
		/// 按钮扁平化样式
		/// </summary>
		private void StyleButton(Button btn, Color baseColor)
		{
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderSize = 0;
			btn.BackColor = baseColor;
			btn.ForeColor = Color.White;
			btn.Font = new Font(btn.Font, FontStyle.Regular);
			btn.Cursor = Cursors.Hand;
			btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(baseColor, 0.9f);
			btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(baseColor, 0.1f);
		}

		public void Log(string msg, string color = "Black")
		{
			if (rtbHelp.InvokeRequired)
			{
				rtbHelp.BeginInvoke((MethodInvoker)(() => Log(msg, color)));
				return;
			}
			rtbHelp.SelectionColor = Color.FromName(color);
			rtbHelp.AppendText("- " + msg + '\n');
		}

		public void Status(string msg)
		{
			if (InvokeRequired)
			{
				BeginInvoke((MethodInvoker)(() => Status(msg)));
				return;
			}
			slbStatusLabel.Text = msg;
		}

		/// <summary>
		/// 设置进度条值（可跨线程调用）
		/// </summary>
		public void SetProgress(int value)
		{
			if (InvokeRequired)
			{
				BeginInvoke((MethodInvoker)(() => SetProgress(value)));
				return;
			}
			if (value >= pgbProgress.Minimum && value <= pgbProgress.Maximum)
				pgbProgress.Value = value;
		}

		/// <summary>
		/// 设置进度条范围（可跨线程调用）
		/// </summary>
		public void SetProgressRange(int min, int max)
		{
			if (InvokeRequired)
			{
				BeginInvoke((MethodInvoker)(() => SetProgressRange(min, max)));
				return;
			}
			pgbProgress.Minimum = min;
			pgbProgress.Maximum = max;
			pgbProgress.Value = min;
		}

		public void SetCheckAll(CheckedListBox elem, bool value)
		{
			for (int i = 0; i < elem.Items.Count; i++)
			{
				elem.SetItemChecked(i, value);
			}
		}

		public void FillCheckedBox(CheckedListBox elem, List<string> list)
		{
			foreach (var line in list)
				elem.Items.Add(line);
		}

		public void SwitchUiLock(int step)
		{
			// 依据窗体自身光标 toggle（不能用 Cursor.Current 全局光标判断，否则会永远设成等待）
			Cursor = (Cursor == Cursors.WaitCursor) ? Cursors.Default : Cursors.WaitCursor;
			if (step >= 4)
				grbStep4.Enabled = !grbStep4.Enabled;
			if (step >= 3)
				grbStep3.Enabled = !grbStep3.Enabled;
			if (step >= 2)
				grbStep1.Enabled = !grbStep1.Enabled;
			if (step >= 1)
				grbStep2.Enabled = !grbStep2.Enabled;
		}

		private void SwitchExtractionType()
		{
			if (rdbExMods.Checked)
			{
				rdbExMinecraft.Checked = false;
				rdbExJar.Checked = false;
				btnLocateJar.Text = "浏览...";
				SetBatchModeUI(true);
			}
			else if (rdbExMinecraft.Checked)
			{
				rdbExJar.Checked = false;
				btnLocateJar.Text = "定位...";
				SetBatchModeUI(false);
			}
			else
			{
				rdbExMinecraft.Checked = false;
				btnLocateJar.Text = "浏览...";
				SetBatchModeUI(false);
			}
			StepsController.Step1(this);
		}

		/// <summary>
		/// 初始化目录树预览：在右侧帮助面板中创建垂直分割容器
		/// 上方为目录树，下方为日志输出
		/// </summary>
		private void InitializeDirectoryTree()
		{
			// 创建垂直分割容器（上方目录树，下方日志）
			splitHelp = new SplitContainer
			{
				Dock = DockStyle.Fill,
				Orientation = Orientation.Horizontal,
				SplitterDistance = 210,
				Panel1MinSize = 60,
				Panel2MinSize = 60,
				BackColor = C_BORDER
			};

			// Panel1 标题标签
			var lblTreeTitle = new Label
			{
				Text = "  目录结构预览",
				Dock = DockStyle.Top,
				Font = new Font("Microsoft YaHei", 9F, FontStyle.Bold),
				ForeColor = C_GB_HEADER,
				BackColor = C_SURFACE,
				Height = 22,
				TextAlign = ContentAlignment.MiddleLeft
			};

			// Panel1 目录树
			treeDirectory = new TreeView
			{
				Dock = DockStyle.Fill,
				BorderStyle = BorderStyle.None,
				BackColor = Color.FromArgb(248, 250, 252),
				Font = new Font("Microsoft YaHei", 8.5F),
				ShowLines = true,
				ShowPlusMinus = true,
				HideSelection = false
			};
			treeDirectory.AfterSelect += (s, e) =>
			{
				if (e.Node?.Tag is string path && !string.IsNullOrEmpty(path))
					Status(path.EndsWith("/") ? "目录: " + path : "文件: " + path);
			};

			// 懒加载：展开目录时才加载其子项（避免一次加载几十万节点）
			treeDirectory.BeforeExpand += (s, e) =>
			{
				if (e.Node?.Tag is string dirPath
					&& e.Node.Nodes.Count == 1
					&& e.Node.Nodes[0].Text == "...")
				{
					AddChildrenToNode(e.Node, dirPath);
				}
			};

			// 重新组织 grbHelp 控件
			grbHelp.Controls.Remove(rtbHelp);

			splitHelp.Panel1.Controls.Add(treeDirectory);
			splitHelp.Panel1.Controls.Add(lblTreeTitle);

			splitHelp.Panel2.Controls.Add(rtbHelp);
			rtbHelp.Dock = DockStyle.Fill;

			grbHelp.Controls.Add(splitHelp);

			// 更新帮助面板样式
			grbHelp.BackColor = C_SURFACE;
		}

		/// <summary>
		/// 从 jar 条目构建「目录 → 直接子目录/直接文件」索引。
		/// 不涉及 UI，可放在后台线程执行，避免海量条目时卡住界面。
		/// </summary>
		public void BuildDirectoryTreeIndex(JarFile jar)
		{
			// 用 Ordinal 比较器：路径为纯 ASCII，省去文化敏感性比较，构建海量目录树更快
			var subdirs = new Dictionary<string, SortedSet<string>>(StringComparer.Ordinal);
			var files = new Dictionary<string, SortedSet<string>>(StringComparer.Ordinal);

			if (jar?.AllEntries != null)
			{
				foreach (string rawEntry in jar.AllEntries)
				{
					string entry = rawEntry.Replace('\\', '/');
					bool isDir = entry.EndsWith("/");
					string[] parts = entry.Split('/');

					string accumulated = "";
					for (int i = 0; i < parts.Length; i++)
					{
						string part = parts[i];
						if (string.IsNullOrEmpty(part)) continue;

						if (i == parts.Length - 1 && !isDir)
						{
							// 文件
							if (!files.TryGetValue(accumulated, out var fileSet))
							{
								fileSet = new SortedSet<string>(StringComparer.Ordinal);
								files[accumulated] = fileSet;
							}
							fileSet.Add(part);
						}
						else
						{
							// 目录
							string childDir = accumulated + part + "/";
							if (!subdirs.TryGetValue(accumulated, out var dirSet))
							{
								dirSet = new SortedSet<string>(StringComparer.Ordinal);
								subdirs[accumulated] = dirSet;
							}
							dirSet.Add(childDir);
							accumulated = childDir;
						}
					}
				}
			}

			_treeSubdirs = subdirs;
			_treeFiles = files;
		}

		/// <summary>
		/// 根据 jar 文件内容填充目录树（目录 + 文件，懒加载）
		/// 仅操作 TreeView 节点，需在 UI 线程调用；索引应在后台由 BuildDirectoryTreeIndex 预先构建。
		/// </summary>
		public void PopulateDirectoryTree(JarFile jar)
		{
			if (treeDirectory == null) return;

			treeDirectory.Nodes.Clear();

			if (jar == null || jar.AllEntries == null || jar.AllEntries.Count == 0)
			{
				treeDirectory.Nodes.Add(new TreeNode("（加载 jar 后显示目录结构）"));
				return;
			}

			// 兜底：同步调用场景下未预先构建索引，则在此构建（会阻塞 UI，仅旧路径可能触发）
			if (_treeSubdirs == null)
				BuildDirectoryTreeIndex(jar);

			// 根节点
			string rootLabel = jar.FullName ?? "jar 内容";
			var root = new TreeNode(rootLabel) { Tag = "" };
			treeDirectory.Nodes.Add(root);
			AddChildrenToNode(root, "");
			root.Expand();
		}

		/// <summary>
		/// 加载某目录节点的直接子项（子目录 + 文件）
		/// </summary>
		private void AddChildrenToNode(TreeNode node, string dirPath)
		{
			node.Nodes.Clear();

			// 子目录
			if (_treeSubdirs.TryGetValue(dirPath, out var subdirs))
			{
				foreach (string subdir in subdirs)
				{
					string name = subdir.TrimEnd('/');
					int idx = name.LastIndexOf('/');
					name = idx >= 0 ? name.Substring(idx + 1) : name;

					var child = new TreeNode(name + "/") { Tag = subdir };
					if (DirHasChildren(subdir))
						child.Nodes.Add(new TreeNode("...")); // 占位，让展开箭头出现
					node.Nodes.Add(child);
				}
			}

			// 直接文件（最多展示 100 个，避免海量节点卡顿）
			const int MAX_FILES = 100;
			if (_treeFiles.TryGetValue(dirPath, out var files))
			{
				int shown = 0;
				foreach (string f in files)
				{
					if (shown >= MAX_FILES) break;
					node.Nodes.Add(new TreeNode(f) { Tag = dirPath + f });
					shown++;
				}
				if (files.Count > MAX_FILES)
					node.Nodes.Add(new TreeNode("… 还有 " + (files.Count - MAX_FILES) + " 个文件") { ForeColor = C_TEXT_SEC });
			}
		}

		/// <summary>
		/// 判断某目录是否有子项（子目录或文件）
		/// </summary>
		private bool DirHasChildren(string dirPath)
		{
			return (_treeSubdirs.TryGetValue(dirPath, out var d) && d.Count > 0)
				|| (_treeFiles.TryGetValue(dirPath, out var f) && f.Count > 0);
		}

		/// <summary>
		/// 初始化输出结构选项复选框（添加到 grbStep4）
		/// </summary>
		private void InitializeOutputOptions()
		{
			chkGroupByType = new CheckBox
			{
				Text = "按资源类型分类（输出/类型/模组名/资源...）",
				Location = new System.Drawing.Point(6, 186),
				Size = new System.Drawing.Size(350, 17),
				Visible = false,
				Checked = false
			};
			chkGroupByType.CheckedChanged += (s, e) =>
			{
				if (chkGroupByType.Checked)
					Log("输出结构：按资源类型 → 模组名 → 资源内容");
				else
					Log("输出结构：按模组名 → 资源内容");
			};
			grbStep4.Controls.Add(chkGroupByType);
		}

		/// <summary>
		/// 初始化 pack.mcmeta 生成选项（复选框 + 目标 MC 版本下拉框）
		/// </summary>
		private void InitializePackMcMetaOptions()
		{
			chkGeneratePackMcMeta = new CheckBox
			{
				Text = "生成 pack.mcmeta（让输出目录成为可用的资源包）",
				Location = new System.Drawing.Point(6, 208),
				Size = new System.Drawing.Size(360, 17),
				Checked = true
			};
			chkGeneratePackMcMeta.CheckedChanged += (s, e) =>
			{
				bool on = chkGeneratePackMcMeta.Checked;
				if (lblPackFormat != null) lblPackFormat.Enabled = on;
				if (cmbPackFormat != null) cmbPackFormat.Enabled = on;
			};
			grbStep4.Controls.Add(chkGeneratePackMcMeta);

			lblPackFormat = new Label
			{
				Text = "目标 MC 版本：",
				Location = new System.Drawing.Point(24, 230),
				AutoSize = true,
				ForeColor = C_TEXT_SEC
			};
			grbStep4.Controls.Add(lblPackFormat);

			cmbPackFormat = new ComboBox
			{
				Location = new System.Drawing.Point(120, 226),
				Width = 120,
				DropDownStyle = ComboBoxStyle.DropDown
			};
			cmbPackFormat.Items.AddRange(PackMcMeta.SupportedVersions);
			cmbPackFormat.Text = PackMcMeta.DefaultVersion;
			grbStep4.Controls.Add(cmbPackFormat);
		}

		/// <summary>
		/// 设置目标 MC 版本（由 Controller 在加载 jar 后自动检测并填充）
		/// </summary>
		public void SetPackVersion(string version)
		{
			if (cmbPackFormat == null || string.IsNullOrEmpty(version))
				return;
			cmbPackFormat.Text = version;
		}

		/// <summary>
		/// 初始化全选/取消全选链接（添加到 grbStep4，位于资源类型标签右侧）
		/// </summary>
		private void InitializeSelectAllLinks()
		{
			var lnkSelectAll = new LinkLabel
			{
				Text = "全选",
				Location = new System.Drawing.Point(115, 57),
				AutoSize = true,
				LinkColor = C_PRIMARY,
				ActiveLinkColor = C_PRIMARY_HOVER,
				VisitedLinkColor = C_PRIMARY,
				TabStop = false
			};
			lnkSelectAll.LinkClicked += (s, e) =>
			{
				SetCheckAll(chkResourceTypes, true);
				Log("已全选所有资源类型");
			};

			var lnkDeselectAll = new LinkLabel
			{
				Text = "取消全选",
				Location = new System.Drawing.Point(155, 57),
				AutoSize = true,
				LinkColor = C_PRIMARY,
				ActiveLinkColor = C_PRIMARY_HOVER,
				VisitedLinkColor = C_PRIMARY,
				TabStop = false
			};
			lnkDeselectAll.LinkClicked += (s, e) =>
			{
				SetCheckAll(chkResourceTypes, false);
				Log("已取消全选");
			};

			grbStep4.Controls.Add(lnkSelectAll);
			grbStep4.Controls.Add(lnkDeselectAll);
		}

		/// <summary>
		/// 初始化提取文件数预览标签（添加到 grbStep4）
		/// </summary>
		private void InitializeFileCountPreview()
		{
			lblFileCountPreview = new Label
			{
				Text = "",
				AutoSize = true,
				ForeColor = C_TEXT_SEC,
				Font = new Font("Microsoft YaHei", 8.5F),
				Location = new Point(6, 0)
			};
			grbStep4.Controls.Add(lblFileCountPreview);

			// 勾选状态改变时实时刷新预计文件数（去抖：全选会触发大量 ItemCheck，合并为一次计算，避免卡死）
			chkResourceTypes.ItemCheck += (s, e) =>
			{
				if (_previewPending) return;
				_previewPending = true;
				BeginInvoke((MethodInvoker)(() =>
				{
					_previewPending = false;
					UpdateFileCountPreview();
				}));
			};
		}

		/// <summary>
		/// 计算并显示勾选的资源类型预计提取的文件总数
		/// </summary>
		public void UpdateFileCountPreview()
		{
			if (lblFileCountPreview == null) return;

			var jar = controller?.target?.Jar;
			if (jar == null || jar.AllEntries == null || jar.AllEntries.Count == 0)
			{
				lblFileCountPreview.Text = "";
				return;
			}

			int total = 0;
			long totalBytes = 0;
			for (int i = 0; i < chkResourceTypes.Items.Count; i++)
			{
				if (chkResourceTypes.GetItemChecked(i))
				{
					var type = (ResourceType)chkResourceTypes.Items[i];
					int count;
					long bytes;
					jar.CountResourceTypeSummary(type, out count, out bytes);
					total += count;
					totalBytes += bytes;
				}
			}

			if (total <= 0)
			{
				lblFileCountPreview.Text = "请勾选要提取的资源类型";
				return;
			}

			string text = "预计提取 " + total + " 个文件";
			if (totalBytes > 0)
				text += "，约 " + FormatBytes(totalBytes);
			lblFileCountPreview.Text = text;
		}

		/// <summary>
		/// 把字节数格式化为可读大小（B/KB/MB/GB）
		/// </summary>
		private static string FormatBytes(long bytes)
		{
			if (bytes <= 0) return "0 B";
			string[] units = { "B", "KB", "MB", "GB" };
			double d = bytes;
			int unit = 0;
			while (d >= 1024 && unit < units.Length - 1)
			{
				d /= 1024;
				unit++;
			}
			return d.ToString(unit == 0 ? "0" : "0.0") + " " + units[unit];
		}

		/// <summary>
		/// 初始化拖放加载：拖入 .jar 文件或文件夹直接加载
		/// </summary>
		private void InitializeDragDrop()
		{
			// 关键：只给窗体设置 AllowDrop，并递归关闭所有子控件的 AllowDrop。
			// 拖放事件会命中最深的 AllowDrop=true 控件；若给子容器（如 tlpMain）设置
			// 但未绑定处理器，e.Effect 会保持 None，导致出现禁止符号。
			// RichTextBox/TreeView 等控件也可能干扰，统一关闭。
			SetAllowDropRecursively(this, false);
			AllowDrop = true;

			DragEnter += FrmMre_DragEnter;
			DragDrop += FrmMre_DragDrop;
		}

		/// <summary>
		/// 递归设置控件树中所有控件的 AllowDrop
		/// </summary>
		private void SetAllowDropRecursively(Control parent, bool value)
		{
			foreach (Control child in parent.Controls)
			{
				child.AllowDrop = value;
				SetAllowDropRecursively(child, value);
			}
		}

		private void FrmMre_DragEnter(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				e.Effect = DragDropEffects.None;
				return;
			}

			string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
			if (paths == null || paths.Length == 0)
			{
				e.Effect = DragDropEffects.None;
				return;
			}

			// 检查所有拖入项是否都是 .jar 文件或文件夹
			int jarCount = 0, dirCount = 0;
			bool allValid = true;
			foreach (string p in paths)
			{
				if (File.Exists(p) && p.ToLower().EndsWith(".jar"))
					jarCount++;
				else if (Directory.Exists(p))
					dirCount++;
				else
					allValid = false;
			}

			if (allValid && (jarCount > 0 || dirCount > 0))
			{
				e.Effect = DragDropEffects.Copy;
				if (jarCount > 1)
					Status("松开鼠标加载 " + jarCount + " 个 jar 文件（批量模式）");
				else if (jarCount == 1 && dirCount == 0)
					Status("松开鼠标加载 jar 文件: " + paths[0]);
				else
					Status("松开鼠标加载 Mods 目录: " + paths[0]);
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}

		private void FrmMre_DragDrop(object sender, DragEventArgs e)
		{
			string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
			if (paths == null || paths.Length == 0)
				return;

			// 分类：区分 jar 文件和文件夹
			var jarFiles = new List<string>();
			var dirs = new List<string>();
			foreach (string p in paths)
			{
				if (File.Exists(p) && p.ToLower().EndsWith(".jar"))
					jarFiles.Add(p);
				else if (Directory.Exists(p))
					dirs.Add(p);
			}

			if (jarFiles.Count == 1 && dirs.Count == 0)
			{
				// 单个 jar：切到「从 jar 文件提取」模式
				rdbExJar.Checked = true;
				Log("拖放加载 jar 文件: " + jarFiles[0]);
				controller.LoadJarFile(jarFiles[0]);
			}
			else if (jarFiles.Count > 0)
			{
				// 多个 jar：切到「批量提取 Mod」模式，直接加载全部
				rdbExMods.Checked = true;
				Log("拖放加载 " + jarFiles.Count + " 个 jar 文件（批量模式）");
				controller.LoadJarFiles(jarFiles);
			}
			else if (dirs.Count > 0)
			{
				// 文件夹：切到「批量提取 Mod」模式（取第一个文件夹）
				rdbExMods.Checked = true;
				txtModsDir.Text = dirs[0];
				controller.settings.LastBatchInputPath = dirs[0];
				controller.settings.SaveConfig();
				Log("拖放加载 Mods 目录: " + dirs[0]);
				controller.LoadModsDirectory(dirs[0]);
			}
			else
			{
				MessageBox.Show("仅支持拖放 .jar 文件或文件夹。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		/// <summary>
		/// 初始化 Mod 批量模式的动态面板控件（添加到 grbStep2）
		/// </summary>
		private void InitializeModsPanel()
		{
			// 创建面板
			pnlModsDir = new Panel
			{
				Location = new System.Drawing.Point(6, 19),
				Size = new System.Drawing.Size(454, 52),
				Visible = false
			};

			// Mods 目录文本框
			txtModsDir = new TextBox
			{
				Location = new System.Drawing.Point(0, 0),
				Size = new System.Drawing.Size(300, 20),
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				Text = ""
			};

			// 浏览按钮
			btnBrowseModsDir = new Button
			{
				Location = new System.Drawing.Point(306, -1),
				Size = new System.Drawing.Size(68, 23),
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				Text = "浏览...",
				FlatStyle = FlatStyle.Flat,
				UseVisualStyleBackColor = false
			};
			btnBrowseModsDir.Click += BtnBrowseModsDir_Click;

			// 确认扫描按钮
			btnConfirm2Mods = new Button
			{
				Location = new System.Drawing.Point(379, -1),
				Size = new System.Drawing.Size(75, 23),
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				Text = "扫描",
				FlatStyle = FlatStyle.Flat,
				UseVisualStyleBackColor = false
			};
			btnConfirm2Mods.Click += BtnConfirm2Mods_Click;

			pnlModsDir.Controls.Add(txtModsDir);
			pnlModsDir.Controls.Add(btnBrowseModsDir);
			pnlModsDir.Controls.Add(btnConfirm2Mods);

			grbStep2.Controls.Add(pnlModsDir);
		}

		/// <summary>
		/// 切换批量模式 UI 显示/隐藏
		/// </summary>
		private void SetBatchModeUI(bool isBatch)
		{
			// grbStep2 控件切换
			cmbVersions.Visible = !isBatch;
			btnConfirm2.Visible = !isBatch;
			pnlModsDir.Visible = isBatch;

			if (isBatch)
			{
				grbStep2.Text = "步骤 2：选择 Mods 目录";
				// 加载上次使用的 Mods 目录
				if (!string.IsNullOrEmpty(controller.settings.LastBatchInputPath))
					txtModsDir.Text = controller.settings.LastBatchInputPath;
			}
			else
			{
				grbStep2.Text = "步骤 2：选择要提取的版本";
			}

			// grbStep3：批量模式下禁用并更改标题
			if (isBatch)
			{
				grbStep3.Text = "步骤 3：（批量模式已跳过）";
				grbStep3.Enabled = false;
			}
			else
			{
				grbStep3.Text = "步骤 3：选择要提取的分组";
			}

			// 输出结构选项仅在批量模式下显示
			if (chkGroupByType != null)
				chkGroupByType.Visible = isBatch;
		}

		/// <summary>
		/// 浏览 Mods 目录
		/// </summary>
		private void BtnBrowseModsDir_Click(object sender, EventArgs e)
		{
			using (var dialog = new FolderBrowserDialog())
			{
				dialog.Description = "选择包含 Mod .jar 文件的目录（如 .minecraft/mods）";
				if (!string.IsNullOrEmpty(txtModsDir.Text) && Directory.Exists(txtModsDir.Text))
					dialog.SelectedPath = txtModsDir.Text;
				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					txtModsDir.Text = dialog.SelectedPath;
				}
			}
		}

		/// <summary>
		/// 确认 Mods 目录并扫描 jar 文件
		/// </summary>
		private void BtnConfirm2Mods_Click(object sender, EventArgs e)
		{
			string modsDir = txtModsDir.Text.Trim();
			if (string.IsNullOrEmpty(modsDir) || !Directory.Exists(modsDir))
			{
				MessageBox.Show("请先选择一个有效的 Mods 目录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			controller.settings.LastBatchInputPath = modsDir;
			controller.settings.SaveConfig();
			controller.LoadModsDirectory(modsDir);
		}

		private void RdbExMinecraft_CheckedChanged(object sender, EventArgs e)
		{
			SwitchExtractionType();
		}

		private void BtnLocateJar_Click(object sender, EventArgs e)
		{
			if (rdbExMods.Checked)
			{
				// 批量 Mod 模式：打开文件夹选择框
				using (var dialog = new FolderBrowserDialog())
				{
					dialog.Description = "选择包含 Mod .jar 文件的目录（如 .minecraft/mods）";
					if (!string.IsNullOrEmpty(txtPath.Text) && Directory.Exists(txtPath.Text))
						dialog.SelectedPath = txtPath.Text;
					if (dialog.ShowDialog(this) == DialogResult.OK)
					{
						txtPath.Text = dialog.SelectedPath;
						controller.settings.LastBatchInputPath = dialog.SelectedPath;
						controller.settings.SaveConfig();
						StepsController.Step2(this);
						Log("已选择 Mods 目录：" + dialog.SelectedPath);
						Log("请点击[扫描]按钮检测目录中的 Mod 文件。");
					}
				}
			}
			else if (rdbExMinecraft.Checked)
			{
				string manualPath = txtPath.Text.Trim();
				if (!string.IsNullOrEmpty(manualPath) && Directory.Exists(manualPath))
				{
					controller.SetCustomMcPath(manualPath);
				}
				else
				{
					controller.LocateMinecraft();
				}
			}
			else
			{
				// Jar 模式：始终弹出文件选择框
				controller.SelectJarFile();
			}
		}

		private void BtnConfirm2_Click(object sender, EventArgs e)
		{
			if (cmbVersions.Text != string.Empty)
			{
				controller.FindVersionJar(cmbVersions.Text);
			}
		}

		private void BtnConfirm3_Click(object sender, EventArgs e)
		{
			if (chkExtGroups.GetItemChecked(0))
			{
				controller.CheckVersionJar();
			}
			else if (chkExtGroups.GetItemChecked(1))
			{
				controller.GetAssets();
				Status("提取完成！");
				Log("感谢您使用 Minecraft 资源提取器！", "DarkGreen");
				Process.Start("explorer.exe", controller.settings.GetEffectiveOutputPath());
			}
		}

		private void BtnConfirm4_Click(object sender, EventArgs e)
		{
			// 批量 Mod 模式：按选中的资源类型执行批量提取
			if (rdbExMods.Checked)
			{
				string modsDir = txtModsDir.Text.Trim();
				string outputDir = txtOutputPath.Text.Trim();
				if (string.IsNullOrEmpty(outputDir))
					outputDir = controller.GetOutputPath();

				// 收集用户选中的资源类型
				var selectedTypes = new List<ResourceType>();
				for (int i = 0; i < chkResourceTypes.Items.Count; i++)
				{
					if (chkResourceTypes.GetItemChecked(i))
						selectedTypes.Add((ResourceType)chkResourceTypes.Items[i]);
				}

				if (selectedTypes.Count == 0)
				{
					MessageBox.Show("请至少选择一种要提取的资源类型！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				Log("正在开始批量提取 Mod 资源，这可能需要一些时间，请耐心等待...");
				Log("选中的资源类型: " + string.Join("、", selectedTypes.Select(t => ResourceTypes.GetDisplayName(t))));
				Log("Mods 目录: " + modsDir);
				Log("输出目录: " + outputDir);

				controller.RunBatchExtractionFiltered(modsDir, outputDir, selectedTypes);
				return;
			}

			if (rdbExJar.Checked || chkExtGroups.GetItemChecked(0))
			{
				Log("正在开始 jar 提取，这可能需要一些时间，请耐心等待...");

				var selectedTypes = new List<ResourceType>();
				var selectedFolders = new List<string>();

				for (int i = 0; i < chkResourceTypes.Items.Count; i++)
				{
					if (chkResourceTypes.GetItemChecked(i))
						selectedTypes.Add((ResourceType)chkResourceTypes.Items[i]);
				}

				if (selectedTypes.Count == 0)
				{
					for (int i = 0; i < chkExtFolders.Items.Count; i++)
					{
						if (chkExtFolders.GetItemChecked(i))
							selectedFolders.Add(chkExtFolders.Items[i].ToString());
					}
				}

				controller.ExtractSelectedResources(selectedTypes, selectedFolders);
			}
			if (!rdbExJar.Checked && chkExtGroups.GetItemChecked(1))
			{
				controller.GetAssets();
			}
		}

		private void BtnBrowseOutput_Click(object sender, EventArgs e)
		{
			using (var dialog = new FolderBrowserDialog())
			{
				dialog.Description = "选择资源提取输出目录";
				dialog.SelectedPath = txtOutputPath.Text;
				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					controller.SetOutputPath(dialog.SelectedPath);
					txtOutputPath.Text = dialog.SelectedPath;
				}
			}
		}

		private void LnkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			FrmAbout frmAbout = new FrmAbout();
			frmAbout.ShowDialog();
		}

		private void CmbVersions_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (grbStep3.Enabled)
				StepsController.Step2(this);
		}

		private void ChkExtGroups_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (grbStep4.Enabled)
				StepsController.Step3(this);
		}

		/// <summary>
		/// 鼠标悬停资源类型时显示详细信息
		/// </summary>
		private void ChkResourceTypes_MouseMove(object sender, MouseEventArgs e)
		{
			int index = chkResourceTypes.IndexFromPoint(e.Location);
			if (index >= 0 && index < chkResourceTypes.Items.Count)
			{
				ResourceType type = (ResourceType)chkResourceTypes.Items[index];
				var info = ResourceTypes.AllTypes.FirstOrDefault(t => t.Type == type);
				if (info != null && !string.IsNullOrEmpty(info.Description))
				{
					string currentTooltip = toolTip.GetToolTip(chkResourceTypes);
					string newTooltip = info.DisplayName + "：" + info.Description;
					if (currentTooltip != newTooltip)
						toolTip.SetToolTip(chkResourceTypes, newTooltip);
					return;
				}
			}
			toolTip.SetToolTip(chkResourceTypes, "");
		}

		private void ChkResourceTypes_MouseLeave(object sender, EventArgs e)
		{
			toolTip.SetToolTip(chkResourceTypes, "");
		}

		/// <summary>
		/// 根据 chkResourceTypes 的实际高度动态调整步骤4中下方控件的位置
		/// </summary>
		public void LayoutStep4Controls()
		{
			int y = chkResourceTypes.Bottom + 8;

			// 文件数预览标签（如果有）
			if (lblFileCountPreview != null)
			{
				lblFileCountPreview.Top = y - 2;
				y = lblFileCountPreview.Bottom + 4;
			}

			lblOutputPath.Top = y;
			y = lblOutputPath.Bottom + 2;

			txtOutputPath.Top = y;
			btnBrowseOutput.Top = y - 1;

			y = txtOutputPath.Bottom + 8;

			// 输出结构选项（如果有的话）
			if (chkGroupByType != null && chkGroupByType.Visible)
			{
				chkGroupByType.Top = y;
				y = chkGroupByType.Bottom + 8;
			}

			// pack.mcmeta 生成选项
			if (chkGeneratePackMcMeta != null)
			{
				chkGeneratePackMcMeta.Top = y;
				y = chkGeneratePackMcMeta.Bottom + 6;

				if (lblPackFormat != null && cmbPackFormat != null)
				{
					cmbPackFormat.Top = y;
					lblPackFormat.Top = y + 3;
					y = cmbPackFormat.Bottom + 8;
				}
			}

			btnConfirm4.Top = y;

			// 动态调整 grbStep4 的最小高度以适应内容
			int neededGrb4Height = btnConfirm4.Bottom + 10;
			grbStep4.MinimumSize = new System.Drawing.Size(280, neededGrb4Height);

			// 动态调整窗体的最小高度，确保确认按钮不被遮挡
			// tlpMain 各行的总高度: row0(150) + row1(58) + row2(70) + row4(24) + padding(12) + statusStrip(22) ≈ 336
			int neededClientHeight = 336 + neededGrb4Height;
			int neededFormHeight = neededClientHeight + 32; // title bar 补偿
			if (neededFormHeight < 580) neededFormHeight = 580;
			MinimumSize = new System.Drawing.Size(700, neededFormHeight);
			if (Height < neededFormHeight)
				Height = neededFormHeight;
		}

		private void FrmMre_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (Directory.Exists(controller.settings.MreDirPath + "\\mre-tmp"))
				Directory.Delete(controller.settings.MreDirPath + "\\mre-tmp", true);
		}
	}
}
