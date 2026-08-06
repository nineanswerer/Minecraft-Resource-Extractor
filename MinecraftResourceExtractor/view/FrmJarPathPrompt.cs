using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mre.view
{
	public partial class FrmJarPathPrompt : Form
	{
		public string SelectedPath { get; private set; }

		public FrmJarPathPrompt()
		{
			InitializeComponent();
		}

		private void btnBrowseJar_Click(object sender, EventArgs e)
		{
			using (var dialog = new OpenFileDialog())
			{
				dialog.Title = "选择 jar.exe";
				dialog.Filter = "jar.exe (jar.exe)|jar.exe|可执行文件 (*.exe)|*.exe|所有文件 (*.*)|*.*";
				dialog.CheckFileExists = true;
				dialog.FileName = "jar.exe";
				dialog.Multiselect = false;

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					txtJarPath.Text = dialog.FileName;
					btnJarPathOk.Enabled = true;
				}
			}
		}

		private void btnJarPathOk_Click(object sender, EventArgs e)
		{
			var p = txtJarPath.Text;
			if (string.IsNullOrWhiteSpace(p) || !File.Exists(p))
			{
				MessageBox.Show(this, "所选文件不存在。", "无效路径", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				DialogResult = DialogResult.None;
				return;
			}

			// Ensure user actually selected jar.exe (case-insensitive)
			if (!string.Equals(Path.GetFileName(p), "jar.exe", StringComparison.OrdinalIgnoreCase))
			{
				var res = MessageBox.Show(this, "所选文件不名为 'jar.exe'。您确定要使用它吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (res == DialogResult.No)
				{
					DialogResult = DialogResult.None;
					return;
				}
			}

			SelectedPath = p;
		}
	}
}
