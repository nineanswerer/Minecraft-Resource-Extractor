using mre.view;

namespace mre.controller
{
	public static class StepsController
	{
		public static void Step1(FrmMre form)
		{
			form.grbStep2.Enabled = false;
			form.grbStep3.Enabled = false;
			form.grbStep4.Enabled = false;
			form.txtPath.Text = string.Empty;
			form.cmbVersions.Items.Clear();
			form.chkExtFolders.Items.Clear();
			form.chkResourceTypes.Items.Clear();
			form.pgbProgress.Value = 0;
			form.btnLocateJar.Focus();
			form.SetCheckAll(form.chkExtGroups, true);

			// 批量模式下恢复 txtPath 显示 Mods 目录路径
			if (form.rdbExMods.Checked)
			{
				if (!string.IsNullOrEmpty(form.controller.settings.LastBatchInputPath))
					form.txtPath.Text = form.controller.settings.LastBatchInputPath;
				form.Status("步骤 1 > 选择 Mods 目录");
			}
			else if (form.rdbExJar.Checked)
			{
				form.Status("步骤 1 > 选择 jar 文件");
			}
			else
			{
				form.Status("步骤 1 > 选择提取模式");
			}
		}

		public static void Step2(FrmMre form)
		{
			form.grbStep2.Enabled = true;
			form.grbStep3.Enabled = false;
			form.grbStep4.Enabled = false;
			form.chkExtFolders.Items.Clear();
			form.chkResourceTypes.Items.Clear();
			form.pgbProgress.Value = 0;

			if (form.rdbExMods.Checked)
			{
				form.cmbVersions.Visible = false;
				form.btnConfirm2.Visible = false;
				if (form.pnlModsDir != null)
					form.pnlModsDir.Visible = true;
				form.Status("步骤 2 > 选择 Mods 目录");
			}
			else
			{
				form.cmbVersions.Visible = true;
				form.btnConfirm2.Visible = true;
				if (form.pnlModsDir != null)
					form.pnlModsDir.Visible = false;
				form.cmbVersions.Focus();
				form.Status("步骤 2 > 选择要提取的版本");
			}
		}

		public static void Step3(FrmMre form)
		{
			form.grbStep3.Enabled = true;
			form.grbStep4.Enabled = false;
			form.chkExtFolders.Items.Clear();
			form.chkResourceTypes.Items.Clear();
			form.btnConfirm3.Focus();
			form.pgbProgress.Value = 0;
			form.Status("步骤 3 > 选择要提取的内容");
		}

		public static void Step4(FrmMre form)
		{
			form.grbStep4.Enabled = true;
			form.btnConfirm4.Focus();
			form.pgbProgress.Value = 0;

			// 批量模式下隐藏文件夹列表（不需要），但显示资源类型选择
			if (form.rdbExMods.Checked)
			{
				form.chkExtFolders.Visible = false;
				form.chkResourceTypes.Visible = true;
				form.Status("步骤 4 > 选择要提取的资源类型");
			}
			else
			{
				form.chkExtFolders.Visible = true;
				form.chkResourceTypes.Visible = true;
				form.Status("步骤 4 > 选择要提取的文件夹或资源类型");
			}
		}
	}
}