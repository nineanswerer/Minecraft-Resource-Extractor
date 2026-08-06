using mre.controller;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace mre.view
{
	public partial class FrmMre : Form
	{
		private Controller controller { get; }

		public FrmMre()
		{
			InitializeComponent();
			controller = new Controller(this);
			if (controller.GetJavaPath() == null)
			{
				MessageBox.Show("错误\n您需要先安装 Java JDK 才能使用此工具", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Load += (s, e) => Close();
				return;
			}
			Log("欢迎使用 Minecraft 资源提取器！");
			StepsController.Step1(this);
			Log("请选择是要从官方 Minecraft 版本提取资源，还是从单独的 jar 文件提取。");
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

		private void BtnBrowse_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog browse = new FolderBrowserDialog
			{
				Description = "选择 .minecraft 文件夹"
			};
			if (browse.ShowDialog() == DialogResult.OK)
			{
				txtPath.Text = browse.SelectedPath;
				controller.SetCustomMcPath(browse.SelectedPath);
			}
		}

		private void BtnLocateJar_Click(object sender, EventArgs e)
		{
			if (rdbExMinecraft.Checked)
			{
				controller.LocateMinecraft();
			}
			else
			{
				controller.LocateJarFile();
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
				Status("Job completed !");
				Log("感谢您使用 Minecraft 资源提取器！", "DarkGreen");
				Process.Start("explorer.exe", controller.settings.MreDirPath + "\\mre-output");
			}
		}

		private void BtnConfirm4_Click(object sender, EventArgs e)
		{
			if (rdbExJar.Checked || chkExtGroups.GetItemChecked(0))
			{
				Log("正在开始 jar 提取，这可能需要一些时间，请耐心等待...");
				for (int i = 0; i < chkExtFolders.Items.Count; i++)
				{
					if (chkExtFolders.GetItemChecked(i))
					{
						controller.ExtractJarFolder(chkExtFolders.Items[i].ToString());
					}
				}
				Log("成功从 jar 文件中提取内容！您的文件位于 \"mre-output\" 文件夹中。", "DarkGreen");
			}
			if (!rdbExJar.Checked && chkExtGroups.GetItemChecked(1))
			{
				controller.GetAssets();
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

		private void FrmMre_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (Directory.Exists(controller.settings.MreDirPath + "\\mre-tmp"))
				Directory.Delete(controller.settings.MreDirPath + "\\mre-tmp", true);
		}
	}
}