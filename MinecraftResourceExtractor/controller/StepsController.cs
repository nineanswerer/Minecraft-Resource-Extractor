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
			form.Status("步骤 1 > 选择提取模式");
		}

		public static void Step2(FrmMre form)
		{
			form.grbStep2.Enabled = true;
			form.grbStep3.Enabled = false;
			form.grbStep4.Enabled = false;
			form.cmbVersions.Focus();
			form.chkExtFolders.Items.Clear();
			form.chkResourceTypes.Items.Clear();
			form.pgbProgress.Value = 0;
			form.Status("步骤 2 > 选择要提取的版本");
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
			form.Status("步骤 4 > 选择要提取的文件夹或资源类型");
		}
	}
}