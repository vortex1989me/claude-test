using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TikTok_Automation_Library_Non_Jail.Forms
{
	// Token: 0x020000AB RID: 171
	public partial class LogsForm : Form
	{
		// Token: 0x06000234 RID: 564 RVA: 0x00003216 File Offset: 0x00001416
		public LogsForm()
		{
			this.InitializeComponent();
			Logs.OpenNew(this.textBox1);
		}
	}
}
