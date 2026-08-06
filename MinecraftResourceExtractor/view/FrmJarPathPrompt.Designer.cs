namespace mre.view
{
	partial class FrmJarPathPrompt
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.btnBrowseJar = new System.Windows.Forms.Button();
			this.txtJarPath = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.btnJarPathOk = new System.Windows.Forms.Button();
			this.btnJarPathCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnBrowseJar
			// 
			this.btnBrowseJar.Location = new System.Drawing.Point(12, 69);
			this.btnBrowseJar.Name = "btnBrowseJar";
			this.btnBrowseJar.Size = new System.Drawing.Size(73, 23);
			this.btnBrowseJar.TabIndex = 0;
			this.btnBrowseJar.Text = "浏览...";
			this.btnBrowseJar.UseVisualStyleBackColor = true;
			this.btnBrowseJar.Click += new System.EventHandler(this.btnBrowseJar_Click);
			// 
			// txtJarPath
			// 
			this.txtJarPath.Location = new System.Drawing.Point(91, 69);
			this.txtJarPath.Name = "txtJarPath";
			this.txtJarPath.ReadOnly = true;
			this.txtJarPath.Size = new System.Drawing.Size(243, 20);
			this.txtJarPath.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(324, 39);
			this.label1.TabIndex = 2;
			this.label1.Text = "无法自动找到 Java 的 jar.exe。\r\n请先安装 Java JDK 以使用此工具。\r\n如果您有自定义的 Java JDK 路径，可以在下面提供。";
			// 
			// btnJarPathOk
			// 
			this.btnJarPathOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnJarPathOk.Enabled = false;
			this.btnJarPathOk.Location = new System.Drawing.Point(261, 112);
			this.btnJarPathOk.Name = "btnJarPathOk";
			this.btnJarPathOk.Size = new System.Drawing.Size(75, 23);
			this.btnJarPathOk.TabIndex = 3;
			this.btnJarPathOk.Text = "确定";
			this.btnJarPathOk.UseVisualStyleBackColor = true;
			this.btnJarPathOk.Click += new System.EventHandler(this.btnJarPathOk_Click);
			// 
			// btnJarPathCancel
			// 
			this.btnJarPathCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnJarPathCancel.Location = new System.Drawing.Point(180, 112);
			this.btnJarPathCancel.Name = "btnJarPathCancel";
			this.btnJarPathCancel.Size = new System.Drawing.Size(75, 23);
			this.btnJarPathCancel.TabIndex = 4;
			this.btnJarPathCancel.Text = "取消";
			this.btnJarPathCancel.UseVisualStyleBackColor = true;
			// 
			// FrmJarPathPrompt
			// 
			this.AcceptButton = this.btnJarPathOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnJarPathCancel;
			this.ClientSize = new System.Drawing.Size(346, 147);
			this.Controls.Add(this.btnJarPathCancel);
			this.Controls.Add(this.btnJarPathOk);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txtJarPath);
			this.Controls.Add(this.btnBrowseJar);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FrmJarPathPrompt";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "定位 jar.exe";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnBrowseJar;
		private System.Windows.Forms.TextBox txtJarPath;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnJarPathOk;
		private System.Windows.Forms.Button btnJarPathCancel;
	}
}