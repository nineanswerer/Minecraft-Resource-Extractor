namespace mre.view
{
	partial class FrmMre
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Code généré par le Concepteur Windows Form

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMre));
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.pgbProgress = new System.Windows.Forms.ToolStripProgressBar();
			this.slbStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
			this.grbStep1 = new System.Windows.Forms.GroupBox();
			this.rdbExMinecraft = new System.Windows.Forms.RadioButton();
			this.rdbExJar = new System.Windows.Forms.RadioButton();
			this.rdbExMods = new System.Windows.Forms.RadioButton();
			this.txtPath = new System.Windows.Forms.TextBox();
			this.btnLocateJar = new System.Windows.Forms.Button();
			this.grbStep2 = new System.Windows.Forms.GroupBox();
			this.btnConfirm2 = new System.Windows.Forms.Button();
			this.cmbVersions = new System.Windows.Forms.ComboBox();
			this.grbStep3 = new System.Windows.Forms.GroupBox();
			this.btnConfirm3 = new System.Windows.Forms.Button();
			this.chkExtGroups = new System.Windows.Forms.CheckedListBox();
			this.grbStep4 = new System.Windows.Forms.GroupBox();
			this.chkExtFolders = new System.Windows.Forms.CheckedListBox();
			this.lblResourceTypes = new System.Windows.Forms.Label();
			this.chkResourceTypes = new System.Windows.Forms.CheckedListBox();
			this.lblOutputPath = new System.Windows.Forms.Label();
			this.txtOutputPath = new System.Windows.Forms.TextBox();
			this.btnBrowseOutput = new System.Windows.Forms.Button();
			this.btnConfirm4 = new System.Windows.Forms.Button();
			this.grbHelp = new System.Windows.Forms.GroupBox();
			this.rtbHelp = new System.Windows.Forms.RichTextBox();
			this.lnkAbout = new System.Windows.Forms.LinkLabel();
			this.toolTip = new System.Windows.Forms.ToolTip();
			this.statusStrip1.SuspendLayout();
			this.tlpMain.SuspendLayout();
			this.grbStep1.SuspendLayout();
			this.grbStep2.SuspendLayout();
			this.grbStep3.SuspendLayout();
			this.grbStep4.SuspendLayout();
			this.grbHelp.SuspendLayout();
			this.SuspendLayout();
			//
			// statusStrip1
			//
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.pgbProgress,
			this.slbStatusLabel});
			this.statusStrip1.Location = new System.Drawing.Point(0, 478);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(734, 22);
			this.statusStrip1.SizingGrip = false;
			this.statusStrip1.TabIndex = 0;
			this.statusStrip1.Text = "statusStrip1";
			//
			// pgbProgress
			//
			this.pgbProgress.Name = "pgbProgress";
			this.pgbProgress.Size = new System.Drawing.Size(100, 16);
			this.pgbProgress.Step = 1;
			this.pgbProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			//
			// slbStatusLabel
			//
			this.slbStatusLabel.Name = "slbStatusLabel";
			this.slbStatusLabel.Size = new System.Drawing.Size(118, 17);
			this.slbStatusLabel.Text = "";
			//
			// tlpMain
			//
			this.tlpMain.ColumnCount = 2;
			this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65F));
			this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
			this.tlpMain.Controls.Add(this.grbStep1, 0, 0);
			this.tlpMain.Controls.Add(this.grbStep2, 0, 1);
			this.tlpMain.Controls.Add(this.grbStep3, 0, 2);
			this.tlpMain.Controls.Add(this.grbStep4, 0, 3);
			this.tlpMain.Controls.Add(this.grbHelp, 1, 0);
			this.tlpMain.Controls.Add(this.lnkAbout, 0, 4);
			this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tlpMain.Location = new System.Drawing.Point(0, 0);
			this.tlpMain.Name = "tlpMain";
			this.tlpMain.Padding = new System.Windows.Forms.Padding(8, 8, 8, 4);
			this.tlpMain.RowCount = 5;
			this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
			this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
			this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
			this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
			this.tlpMain.Size = new System.Drawing.Size(734, 478);
			this.tlpMain.TabIndex = 10;
			//
			// grbStep1
			//
			this.grbStep1.Controls.Add(this.rdbExMinecraft);
			this.grbStep1.Controls.Add(this.rdbExJar);
			this.grbStep1.Controls.Add(this.rdbExMods);
			this.grbStep1.Controls.Add(this.txtPath);
			this.grbStep1.Controls.Add(this.btnLocateJar);
			this.grbStep1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grbStep1.Location = new System.Drawing.Point(11, 11);
			this.grbStep1.MinimumSize = new System.Drawing.Size(280, 145);
			this.grbStep1.Name = "grbStep1";
			this.grbStep1.Size = new System.Drawing.Size(466, 124);
			this.grbStep1.TabIndex = 3;
			this.grbStep1.TabStop = false;
			this.grbStep1.Text = "步骤 1：定位 Minecraft";
			//
			// rdbExMinecraft
			//
			this.rdbExMinecraft.AutoSize = true;
			this.rdbExMinecraft.Checked = true;
			this.rdbExMinecraft.Location = new System.Drawing.Point(6, 19);
			this.rdbExMinecraft.Name = "rdbExMinecraft";
			this.rdbExMinecraft.Size = new System.Drawing.Size(119, 17);
			this.rdbExMinecraft.TabIndex = 4;
			this.rdbExMinecraft.TabStop = true;
			this.rdbExMinecraft.Text = "提取 Minecraft 资源";
			this.rdbExMinecraft.UseVisualStyleBackColor = true;
			this.rdbExMinecraft.CheckedChanged += new System.EventHandler(this.RdbExMinecraft_CheckedChanged);
			//
			// rdbExJar
			//
			this.rdbExJar.AutoSize = true;
			this.rdbExJar.Location = new System.Drawing.Point(6, 42);
			this.rdbExJar.Name = "rdbExJar";
			this.rdbExJar.Size = new System.Drawing.Size(100, 17);
			this.rdbExJar.TabIndex = 5;
			this.rdbExJar.Text = "从 jar 文件提取";
			this.rdbExJar.UseVisualStyleBackColor = true;
			this.rdbExJar.CheckedChanged += new System.EventHandler(this.RdbExMinecraft_CheckedChanged);
			//
			// rdbExMods
			//
			this.rdbExMods.AutoSize = true;
			this.rdbExMods.Location = new System.Drawing.Point(6, 65);
			this.rdbExMods.Name = "rdbExMods";
			this.rdbExMods.Size = new System.Drawing.Size(130, 17);
			this.rdbExMods.TabIndex = 6;
			this.rdbExMods.Text = "批量提取 Mod 资源";
			this.rdbExMods.UseVisualStyleBackColor = true;
			this.rdbExMods.CheckedChanged += new System.EventHandler(this.RdbExMinecraft_CheckedChanged);
			//
			// txtPath
			//
			this.txtPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.txtPath.Location = new System.Drawing.Point(6, 90);
			this.txtPath.Name = "txtPath";
			this.txtPath.Size = new System.Drawing.Size(374, 20);
			this.txtPath.TabIndex = 2;
			//
			// btnLocateJar
			//
			this.btnLocateJar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.btnLocateJar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnLocateJar.Location = new System.Drawing.Point(6, 116);
			this.btnLocateJar.Name = "btnLocateJar";
			this.btnLocateJar.Size = new System.Drawing.Size(454, 23);
			this.btnLocateJar.TabIndex = 1;
			this.btnLocateJar.Text = "定位...";
			this.btnLocateJar.UseVisualStyleBackColor = false;
			this.btnLocateJar.Click += new System.EventHandler(this.BtnLocateJar_Click);
			//
			// grbStep2
			//
			this.grbStep2.Controls.Add(this.btnConfirm2);
			this.grbStep2.Controls.Add(this.cmbVersions);
			this.grbStep2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grbStep2.Enabled = false;
			this.grbStep2.Location = new System.Drawing.Point(11, 141);
			this.grbStep2.MinimumSize = new System.Drawing.Size(280, 51);
			this.grbStep2.Name = "grbStep2";
			this.grbStep2.Size = new System.Drawing.Size(466, 52);
			this.grbStep2.TabIndex = 7;
			this.grbStep2.TabStop = false;
			this.grbStep2.Text = "步骤 2：选择要提取的版本";
			//
			// btnConfirm2
			//
			this.btnConfirm2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnConfirm2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnConfirm2.Location = new System.Drawing.Point(385, 17);
			this.btnConfirm2.Name = "btnConfirm2";
			this.btnConfirm2.Size = new System.Drawing.Size(75, 23);
			this.btnConfirm2.TabIndex = 1;
			this.btnConfirm2.Text = "确认";
			this.btnConfirm2.UseVisualStyleBackColor = false;
			this.btnConfirm2.Click += new System.EventHandler(this.BtnConfirm2_Click);
			//
			// cmbVersions
			//
			this.cmbVersions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.cmbVersions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbVersions.FormattingEnabled = true;
			this.cmbVersions.Location = new System.Drawing.Point(6, 19);
			this.cmbVersions.Name = "cmbVersions";
			this.cmbVersions.Size = new System.Drawing.Size(373, 21);
			this.cmbVersions.TabIndex = 0;
			this.cmbVersions.SelectedIndexChanged += new System.EventHandler(this.CmbVersions_SelectedIndexChanged);
			//
			// grbStep3
			//
			this.grbStep3.Controls.Add(this.btnConfirm3);
			this.grbStep3.Controls.Add(this.chkExtGroups);
			this.grbStep3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grbStep3.Enabled = false;
			this.grbStep3.Location = new System.Drawing.Point(11, 199);
			this.grbStep3.MinimumSize = new System.Drawing.Size(280, 62);
			this.grbStep3.Name = "grbStep3";
			this.grbStep3.Size = new System.Drawing.Size(466, 64);
			this.grbStep3.TabIndex = 8;
			this.grbStep3.TabStop = false;
			this.grbStep3.Text = "步骤 3：选择要提取的分组";
			//
			// btnConfirm3
			//
			this.btnConfirm3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnConfirm3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnConfirm3.Location = new System.Drawing.Point(385, 30);
			this.btnConfirm3.Name = "btnConfirm3";
			this.btnConfirm3.Size = new System.Drawing.Size(75, 23);
			this.btnConfirm3.TabIndex = 3;
			this.btnConfirm3.Text = "确认";
			this.btnConfirm3.UseVisualStyleBackColor = false;
			this.btnConfirm3.Click += new System.EventHandler(this.BtnConfirm3_Click);
			//
			// chkExtGroups
			//
			this.chkExtGroups.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.chkExtGroups.CheckOnClick = true;
			this.chkExtGroups.FormattingEnabled = true;
			this.chkExtGroups.Items.AddRange(new object[] {
			"minecraft.jar 文件",
			"assets 文件"});
			this.chkExtGroups.Location = new System.Drawing.Point(6, 19);
			this.chkExtGroups.Name = "chkExtGroups";
			this.chkExtGroups.Size = new System.Drawing.Size(373, 34);
			this.chkExtGroups.TabIndex = 0;
			this.chkExtGroups.SelectedIndexChanged += new System.EventHandler(this.ChkExtGroups_SelectedIndexChanged);
			//
			// grbStep4
			//
			this.grbStep4.Controls.Add(this.chkExtFolders);
			this.grbStep4.Controls.Add(this.lblResourceTypes);
			this.grbStep4.Controls.Add(this.chkResourceTypes);
			this.grbStep4.Controls.Add(this.lblOutputPath);
			this.grbStep4.Controls.Add(this.txtOutputPath);
			this.grbStep4.Controls.Add(this.btnBrowseOutput);
			this.grbStep4.Controls.Add(this.btnConfirm4);
			this.grbStep4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grbStep4.Enabled = false;
			this.grbStep4.Location = new System.Drawing.Point(11, 269);
			this.grbStep4.MinimumSize = new System.Drawing.Size(280, 250);
			this.grbStep4.Name = "grbStep4";
			this.grbStep4.Size = new System.Drawing.Size(466, 180);
			this.grbStep4.TabIndex = 9;
			this.grbStep4.TabStop = false;
			this.grbStep4.Text = "步骤 4：选择要提取的内容";
			//
			// chkExtFolders
			//
			this.chkExtFolders.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.chkExtFolders.CheckOnClick = true;
			this.chkExtFolders.Cursor = System.Windows.Forms.Cursors.Default;
			this.chkExtFolders.FormattingEnabled = true;
			this.chkExtFolders.Location = new System.Drawing.Point(6, 19);
			this.chkExtFolders.Name = "chkExtFolders";
			this.chkExtFolders.Size = new System.Drawing.Size(454, 34);
			this.chkExtFolders.TabIndex = 0;
			//
			// lblResourceTypes
			//
			this.lblResourceTypes.AutoSize = true;
			this.lblResourceTypes.Location = new System.Drawing.Point(6, 58);
			this.lblResourceTypes.Name = "lblResourceTypes";
			this.lblResourceTypes.Size = new System.Drawing.Size(88, 13);
			this.lblResourceTypes.TabIndex = 2;
			this.lblResourceTypes.Text = "资源类型选择：";
			//
			// chkResourceTypes
			//
			this.chkResourceTypes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.chkResourceTypes.CheckOnClick = true;
			this.chkResourceTypes.Cursor = System.Windows.Forms.Cursors.Default;
			this.chkResourceTypes.FormattingEnabled = true;
			this.chkResourceTypes.Location = new System.Drawing.Point(6, 73);
			this.chkResourceTypes.Name = "chkResourceTypes";
			this.chkResourceTypes.Size = new System.Drawing.Size(454, 64);
			this.chkResourceTypes.TabIndex = 1;
			this.chkResourceTypes.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ChkResourceTypes_MouseMove);
			this.chkResourceTypes.MouseLeave += new System.EventHandler(this.ChkResourceTypes_MouseLeave);
			//
			// lblOutputPath
			//
			this.lblOutputPath.AutoSize = true;
			this.lblOutputPath.Location = new System.Drawing.Point(6, 143);
			this.lblOutputPath.Name = "lblOutputPath";
			this.lblOutputPath.Size = new System.Drawing.Size(58, 13);
			this.lblOutputPath.TabIndex = 4;
			this.lblOutputPath.Text = "输出目录：";
			//
			// txtOutputPath
			//
			this.txtOutputPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.txtOutputPath.Location = new System.Drawing.Point(6, 159);
			this.txtOutputPath.Name = "txtOutputPath";
			this.txtOutputPath.ReadOnly = true;
			this.txtOutputPath.Size = new System.Drawing.Size(373, 20);
			this.txtOutputPath.TabIndex = 5;
			//
			// btnBrowseOutput
			//
			this.btnBrowseOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnBrowseOutput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnBrowseOutput.Location = new System.Drawing.Point(385, 157);
			this.btnBrowseOutput.Name = "btnBrowseOutput";
			this.btnBrowseOutput.Size = new System.Drawing.Size(75, 23);
			this.btnBrowseOutput.TabIndex = 6;
			this.btnBrowseOutput.Text = "浏览...";
			this.btnBrowseOutput.UseVisualStyleBackColor = false;
			this.btnBrowseOutput.Click += new System.EventHandler(this.BtnBrowseOutput_Click);
			//
			// btnConfirm4
			//
			this.btnConfirm4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnConfirm4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnConfirm4.Location = new System.Drawing.Point(385, 190);
			this.btnConfirm4.Name = "btnConfirm4";
			this.btnConfirm4.Size = new System.Drawing.Size(75, 23);
			this.btnConfirm4.TabIndex = 3;
			this.btnConfirm4.Text = "确认";
			this.btnConfirm4.UseVisualStyleBackColor = false;
			this.btnConfirm4.Click += new System.EventHandler(this.BtnConfirm4_Click);
			//
			// grbHelp
			//
			this.grbHelp.Controls.Add(this.rtbHelp);
			this.grbHelp.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grbHelp.Location = new System.Drawing.Point(486, 11);
			this.grbHelp.Name = "grbHelp";
			this.tlpMain.SetRowSpan(this.grbHelp, 5);
			this.grbHelp.Size = new System.Drawing.Size(237, 456);
			this.grbHelp.TabIndex = 9;
			this.grbHelp.TabStop = false;
			this.grbHelp.Text = "帮助";
			//
			// rtbHelp
			//
			this.rtbHelp.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rtbHelp.Location = new System.Drawing.Point(3, 16);
			this.rtbHelp.Name = "rtbHelp";
			this.rtbHelp.ReadOnly = true;
			this.rtbHelp.Size = new System.Drawing.Size(231, 437);
			this.rtbHelp.TabIndex = 0;
			this.rtbHelp.Text = "";
			//
			// lnkAbout
			//
			this.lnkAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.lnkAbout.AutoSize = true;
			this.lnkAbout.Location = new System.Drawing.Point(438, 470);
			this.lnkAbout.Name = "lnkAbout";
			this.lnkAbout.Size = new System.Drawing.Size(39, 8);
			this.lnkAbout.TabIndex = 6;
			this.lnkAbout.TabStop = true;
			this.lnkAbout.Text = "About...";
			this.lnkAbout.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkAbout_LinkClicked);
			//
			// FrmMre
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(734, 620);
			this.Controls.Add(this.tlpMain);
			this.Controls.Add(this.statusStrip1);
			this.Icon = new System.Drawing.Icon(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "Resources\\mre.ico"));
			this.MinimumSize = new System.Drawing.Size(700, 600);
			this.Name = "FrmMre";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Minecraft 资源提取器";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmMre_FormClosed);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.tlpMain.ResumeLayout(false);
			this.tlpMain.PerformLayout();
			this.grbStep1.ResumeLayout(false);
			this.grbStep1.PerformLayout();
			this.grbStep2.ResumeLayout(false);
			this.grbStep3.ResumeLayout(false);
			this.grbStep4.ResumeLayout(false);
			this.grbStep4.PerformLayout();
			this.grbHelp.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.LinkLabel lnkAbout;
		public System.Windows.Forms.GroupBox grbHelp;
		private System.Windows.Forms.TableLayoutPanel tlpMain;
		public System.Windows.Forms.GroupBox grbStep1;
		public System.Windows.Forms.RadioButton rdbExMinecraft;
		public System.Windows.Forms.TextBox txtPath;
		public System.Windows.Forms.RadioButton rdbExJar;
		public System.Windows.Forms.RadioButton rdbExMods;
		public System.Windows.Forms.Panel pnlModsDir;
		public System.Windows.Forms.TextBox txtModsDir;
		public System.Windows.Forms.Button btnBrowseModsDir;
		public System.Windows.Forms.Button btnConfirm2Mods;
		public System.Windows.Forms.GroupBox grbStep2;
		public System.Windows.Forms.ComboBox cmbVersions;
		public System.Windows.Forms.GroupBox grbStep3;
		public System.Windows.Forms.GroupBox grbStep4;
		public System.Windows.Forms.ToolStripProgressBar pgbProgress;
		public System.Windows.Forms.ToolStripStatusLabel slbStatusLabel;
		public System.Windows.Forms.CheckedListBox chkExtGroups;
		public System.Windows.Forms.CheckedListBox chkExtFolders;
		public System.Windows.Forms.RichTextBox rtbHelp;
		public System.Windows.Forms.Button btnConfirm2;
		public System.Windows.Forms.Button btnConfirm3;
		public System.Windows.Forms.Button btnConfirm4;
		public System.Windows.Forms.Button btnLocateJar;
		public System.Windows.Forms.CheckedListBox chkResourceTypes;
		private System.Windows.Forms.Label lblResourceTypes;
		private System.Windows.Forms.Label lblOutputPath;
		public System.Windows.Forms.TextBox txtOutputPath;
		private System.Windows.Forms.Button btnBrowseOutput;
		private System.Windows.Forms.ToolTip toolTip;
	}
}
