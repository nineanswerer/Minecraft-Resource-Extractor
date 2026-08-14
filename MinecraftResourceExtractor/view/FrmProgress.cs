using System.Drawing;
using System.Windows.Forms;

namespace mre.view
{
	/// <summary>
	/// 加载/扫描进度弹窗（模态），显示进度条与提示文字。
	/// 由 Controller 在后台线程通过 BeginInvoke 更新，完成后关闭。
	/// </summary>
	public class FrmProgress : Form
	{
		private readonly Label _lblMessage;
		private readonly ProgressBar _pgb;

		public FrmProgress(string title)
		{
			Text = title;
			FormBorderStyle = FormBorderStyle.FixedDialog;
			ControlBox = false;
			MaximizeBox = false;
			MinimizeBox = false;
			StartPosition = FormStartPosition.CenterParent;
			ShowInTaskbar = false;
			// 注意：不设 TopMost。TopMost + 无模态 Show(owner) 关闭后会导致 owner 主窗体
			// 被 Windows 压到所有窗口最底层。作为 owner 的子窗体本来就会显示在 owner 之上。
			ClientSize = new Size(400, 100);
			BackColor = Color.FromArgb(240, 244, 248);

			_lblMessage = new Label
			{
				Location = new Point(16, 14),
				Size = new Size(368, 22),
				Text = "",
				AutoSize = false,
				Font = new Font("Microsoft YaHei", 9F),
				ForeColor = Color.FromArgb(51, 65, 85)
			};

			_pgb = new ProgressBar
			{
				Location = new Point(16, 52),
				Size = new Size(368, 20),
				Minimum = 0,
				Maximum = 100,
				Style = ProgressBarStyle.Continuous
			};

			Controls.Add(_lblMessage);
			Controls.Add(_pgb);
		}

		public void SetMessage(string msg)
		{
			_lblMessage.Text = msg;
		}

		public void SetRange(int min, int max)
		{
			_pgb.Minimum = min;
			_pgb.Maximum = max;
			_pgb.Value = min;
		}

		public void SetValue(int value)
		{
			if (value >= _pgb.Minimum && value <= _pgb.Maximum)
				_pgb.Value = value;
		}

		public void SetMarquee(bool marquee)
		{
			_pgb.Style = marquee ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
		}
	}
}
