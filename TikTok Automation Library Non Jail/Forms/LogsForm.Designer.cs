namespace TikTok_Automation_Library_Non_Jail.Forms
{
	// Token: 0x020000AB RID: 171
	public partial class LogsForm : global::System.Windows.Forms.Form
	{
		// Token: 0x06000235 RID: 565 RVA: 0x0000322F File Offset: 0x0000142F
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000236 RID: 566 RVA: 0x00060D28 File Offset: 0x0005EF28
		private void InitializeComponent()
		{
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::TikTok_Automation_Library_Non_Jail.Forms.LogsForm));
			this.textBox1 = new global::System.Windows.Forms.TextBox();
			base.SuspendLayout();
			this.textBox1.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.textBox1.Location = new global::System.Drawing.Point(0, 0);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ScrollBars = global::System.Windows.Forms.ScrollBars.Vertical;
			this.textBox1.Size = new global::System.Drawing.Size(800, 450);
			this.textBox1.TabIndex = 2;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(800, 450);
			base.Controls.Add(this.textBox1);
			base.Icon = (global::System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "LogsForm";
			this.Text = "Logs";
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x04000563 RID: 1379
		private global::System.ComponentModel.IContainer components;

		// Token: 0x04000564 RID: 1380
		private global::System.Windows.Forms.TextBox textBox1;
	}
}
