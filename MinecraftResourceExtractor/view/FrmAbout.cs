using System;
using System.Drawing;
using System.Windows.Forms;

namespace mre.view
{
	public partial class FrmAbout : Form
	{
		private static readonly Color C_PRIMARY = Color.FromArgb(74, 144, 217);
		private static readonly Color C_SURFACE = Color.White;

		public FrmAbout()
		{
			InitializeComponent();
			ApplyStyle();
		}

		private void ApplyStyle()
		{
			StyleButton(btnDonate, Color.FromArgb(240, 140, 50));
			StyleButton(btnWebsite, C_PRIMARY);
			StyleButton(btnClose, Color.FromArgb(108, 117, 125));

			lblTitre.ForeColor = Color.FromArgb(30, 58, 95);
			lblVersion.ForeColor = Color.FromArgb(100, 116, 139);
			lblCreator.ForeColor = Color.FromArgb(100, 116, 139);
			lblMaintainer.ForeColor = Color.FromArgb(74, 144, 217);
		}

		private void StyleButton(Button btn, Color baseColor)
		{
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderSize = 0;
			btn.BackColor = baseColor;
			btn.ForeColor = Color.White;
			btn.Font = new Font("Microsoft YaHei", 9F, FontStyle.Regular);
			btn.Cursor = Cursors.Hand;
			btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(baseColor, 0.9f);
		}

		private void BtnDonate_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("https://paypal.me/JKerboeuf");
		}

		private void BtnWebsite_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("https://github.com/nineanswerer/Minecraft-Resource-Extractor");
		}

		private void BtnClose_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
