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
			InitializeModsPanel();
			InitializeOutputOptions();
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
			rtbHelp.SelectionColor = Color.FromName(color);
			rtbHelp.AppendText("- " + msg + '\n');
		}

		public void Status(string msg)
		{
			slbStatusLabel.Text = msg;
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
			Cursor = Cursor.Current == Cursors.Default ? Cursors.WaitCursor : Cursors.Default;
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

				bool hasResourceTypeSelection = false;
				for (int i = 0; i < chkResourceTypes.Items.Count; i++)
				{
					if (chkResourceTypes.GetItemChecked(i))
					{
						hasResourceTypeSelection = true;
						ResourceType type = (ResourceType)chkResourceTypes.Items[i];
						controller.ExtractResourceType(type);
					}
				}

				if (!hasResourceTypeSelection)
				{
					for (int i = 0; i < chkExtFolders.Items.Count; i++)
					{
						if (chkExtFolders.GetItemChecked(i))
						{
							controller.ExtractJarFolder(chkExtFolders.Items[i].ToString());
						}
					}
				}

				Log("成功从 jar 文件中提取内容！您的文件位于 " + controller.GetOutputPath() + " 文件夹中。", "DarkGreen");
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
