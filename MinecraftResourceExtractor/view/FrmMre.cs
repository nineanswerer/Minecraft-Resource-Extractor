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
		private Controller controller { get; }

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
			StyleButton(btnLocateJar, C_PRIMARY);
			StyleButton(btnConfirm2, C_PRIMARY);
			StyleButton(btnConfirm3, C_PRIMARY);
			StyleButton(btnConfirm4, C_SUCCESS);
			StyleButton(btnBrowseOutput, Color.FromArgb(108, 117, 125));

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
			if (rdbExMinecraft.Checked)
			{
				rdbExJar.Checked = false;
				btnLocateJar.Text = "定位...";
			}
			else
			{
				rdbExMinecraft.Checked = false;
				btnLocateJar.Text = "浏览...";
			}
			StepsController.Step1(this);
		}

		private void RdbExMinecraft_CheckedChanged(object sender, EventArgs e)
		{
			SwitchExtractionType();
		}

		private void BtnLocateJar_Click(object sender, EventArgs e)
		{
			if (rdbExMinecraft.Checked)
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

			btnConfirm4.Top = txtOutputPath.Bottom + 8;
		}

		private void FrmMre_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (Directory.Exists(controller.settings.MreDirPath + "\\mre-tmp"))
				Directory.Delete(controller.settings.MreDirPath + "\\mre-tmp", true);
		}
	}
}
