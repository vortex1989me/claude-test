namespace TikTok_Automation_Library_Non_Jail.Forms
{
	// Token: 0x0200009E RID: 158
	public partial class Form1 : global::System.Windows.Forms.Form
	{
		// Token: 0x06000215 RID: 533 RVA: 0x00003084 File Offset: 0x00001284
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000216 RID: 534 RVA: 0x0005EC10 File Offset: 0x0005CE10
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::TikTok_Automation_Library_Non_Jail.Forms.Form1));
			this.TotalCaptchaTimesLabel = new global::System.Windows.Forms.ToolStripStatusLabel();
			this.TotalPostUnsucessLabel = new global::System.Windows.Forms.ToolStripStatusLabel();
			this.TotalPostLabel = new global::System.Windows.Forms.ToolStripStatusLabel();
			this.TotalBadAccountsLabel = new global::System.Windows.Forms.ToolStripStatusLabel();
			this.TotalGoodAccountsLabel = new global::System.Windows.Forms.ToolStripStatusLabel();
			this.statusStrip1 = new global::System.Windows.Forms.StatusStrip();
			this.TotalBadPicturesLabel = new global::System.Windows.Forms.ToolStripStatusLabel();
			this.TotalSlowTimesLabel = new global::System.Windows.Forms.ToolStripStatusLabel();
			this.readAllFilesToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.stopAllDevicesToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.startAllDevicesToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.stopWorkToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.startWorkToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStrip1 = new global::System.Windows.Forms.ContextMenuStrip(this.components);
			this.configurateShortCutsToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.WDAStatus = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Stats = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Status = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Port = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.UUID = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.DeviceName = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Grid = new global::System.Windows.Forms.DataGridView();
			this.menuStrip1 = new global::System.Windows.Forms.MenuStrip();
			this.readDevicesToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.settingsToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.logsToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.setUpToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.tESTToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.downloadBugVideosToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.statusStrip1.SuspendLayout();
			this.contextMenuStrip1.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.Grid).BeginInit();
			this.menuStrip1.SuspendLayout();
			base.SuspendLayout();
			this.TotalCaptchaTimesLabel.ForeColor = global::System.Drawing.Color.White;
			this.TotalCaptchaTimesLabel.Name = "TotalCaptchaTimesLabel";
			this.TotalCaptchaTimesLabel.Size = new global::System.Drawing.Size(125, 17);
			this.TotalCaptchaTimesLabel.Text = "Total Captcha Times: 0";
			this.TotalPostUnsucessLabel.ForeColor = global::System.Drawing.Color.White;
			this.TotalPostUnsucessLabel.Name = "TotalPostUnsucessLabel";
			this.TotalPostUnsucessLabel.Size = new global::System.Drawing.Size(122, 17);
			this.TotalPostUnsucessLabel.Text = "Total Post Unsucess: 0";
			this.TotalPostLabel.ForeColor = global::System.Drawing.Color.White;
			this.TotalPostLabel.Name = "TotalPostLabel";
			this.TotalPostLabel.Size = new global::System.Drawing.Size(70, 17);
			this.TotalPostLabel.Text = "Total Post: 0";
			this.TotalBadAccountsLabel.ForeColor = global::System.Drawing.Color.White;
			this.TotalBadAccountsLabel.Name = "TotalBadAccountsLabel";
			this.TotalBadAccountsLabel.Size = new global::System.Drawing.Size(120, 17);
			this.TotalBadAccountsLabel.Text = "Total Bad Accounts: 0";
			this.TotalGoodAccountsLabel.ForeColor = global::System.Drawing.Color.White;
			this.TotalGoodAccountsLabel.Name = "TotalGoodAccountsLabel";
			this.TotalGoodAccountsLabel.Size = new global::System.Drawing.Size(129, 17);
			this.TotalGoodAccountsLabel.Text = "Total Good Accounts: 0";
			this.statusStrip1.BackColor = global::System.Drawing.Color.Black;
			this.statusStrip1.Items.AddRange(new global::System.Windows.Forms.ToolStripItem[] { this.TotalGoodAccountsLabel, this.TotalBadAccountsLabel, this.TotalPostLabel, this.TotalPostUnsucessLabel, this.TotalBadPicturesLabel, this.TotalSlowTimesLabel, this.TotalCaptchaTimesLabel });
			this.statusStrip1.Location = new global::System.Drawing.Point(0, 428);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new global::System.Drawing.Size(851, 22);
			this.statusStrip1.TabIndex = 6;
			this.statusStrip1.Text = "statusStrip1";
			this.TotalBadPicturesLabel.ForeColor = global::System.Drawing.Color.White;
			this.TotalBadPicturesLabel.Name = "TotalBadPicturesLabel";
			this.TotalBadPicturesLabel.Size = new global::System.Drawing.Size(112, 17);
			this.TotalBadPicturesLabel.Text = "Total Bad Pictures: 0";
			this.TotalSlowTimesLabel.ForeColor = global::System.Drawing.Color.White;
			this.TotalSlowTimesLabel.Name = "TotalSlowTimesLabel";
			this.TotalSlowTimesLabel.Size = new global::System.Drawing.Size(106, 17);
			this.TotalSlowTimesLabel.Text = "Total Slow Times: 0";
			this.readAllFilesToolStripMenuItem.Name = "readAllFilesToolStripMenuItem";
			this.readAllFilesToolStripMenuItem.Size = new global::System.Drawing.Size(192, 22);
			this.readAllFilesToolStripMenuItem.Text = "Read All Files";
			this.readAllFilesToolStripMenuItem.Click += new global::System.EventHandler(this.readAllFilesToolStripMenuItem_Click);
			this.stopAllDevicesToolStripMenuItem.Name = "stopAllDevicesToolStripMenuItem";
			this.stopAllDevicesToolStripMenuItem.Size = new global::System.Drawing.Size(192, 22);
			this.stopAllDevicesToolStripMenuItem.Text = "Stop All Devices";
			this.stopAllDevicesToolStripMenuItem.Click += new global::System.EventHandler(this.stopAllDevicesToolStripMenuItem_Click);
			this.startAllDevicesToolStripMenuItem.Name = "startAllDevicesToolStripMenuItem";
			this.startAllDevicesToolStripMenuItem.Size = new global::System.Drawing.Size(192, 22);
			this.startAllDevicesToolStripMenuItem.Text = "Start All Devices";
			this.startAllDevicesToolStripMenuItem.Click += new global::System.EventHandler(this.startAllDevicesToolStripMenuItem_Click);
			this.stopWorkToolStripMenuItem.Name = "stopWorkToolStripMenuItem";
			this.stopWorkToolStripMenuItem.Size = new global::System.Drawing.Size(192, 22);
			this.stopWorkToolStripMenuItem.Text = "Stop Work";
			this.stopWorkToolStripMenuItem.Click += new global::System.EventHandler(this.stopWorkToolStripMenuItem_Click);
			this.startWorkToolStripMenuItem.Name = "startWorkToolStripMenuItem";
			this.startWorkToolStripMenuItem.Size = new global::System.Drawing.Size(192, 22);
			this.startWorkToolStripMenuItem.Text = "Start Work";
			this.startWorkToolStripMenuItem.Click += new global::System.EventHandler(this.startWorkToolStripMenuItem_Click);
			this.contextMenuStrip1.Items.AddRange(new global::System.Windows.Forms.ToolStripItem[] { this.startWorkToolStripMenuItem, this.stopWorkToolStripMenuItem, this.startAllDevicesToolStripMenuItem, this.stopAllDevicesToolStripMenuItem, this.readAllFilesToolStripMenuItem, this.configurateShortCutsToolStripMenuItem, this.downloadBugVideosToolStripMenuItem });
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new global::System.Drawing.Size(193, 180);
			this.configurateShortCutsToolStripMenuItem.Name = "configurateShortCutsToolStripMenuItem";
			this.configurateShortCutsToolStripMenuItem.Size = new global::System.Drawing.Size(192, 22);
			this.configurateShortCutsToolStripMenuItem.Text = "Configurate ShortCuts";
			this.configurateShortCutsToolStripMenuItem.Click += new global::System.EventHandler(this.configurateShortCutsToolStripMenuItem_Click);
			this.WDAStatus.HeaderText = "WDA Status";
			this.WDAStatus.Name = "WDAStatus";
			this.Stats.HeaderText = "GA/BA/PS/PU/CT/BP/RA/RT";
			this.Stats.Name = "Stats";
			this.Stats.ToolTipText = "Good Accounts / Bad Accounts / Posted Sucess / Posted Unsucess / Captcha Times / Bad Pictures / Total ReWork Accounts / Total ReWork Times";
			this.Status.HeaderText = "Status";
			this.Status.Name = "Status";
			this.Port.HeaderText = "Port";
			this.Port.Name = "Port";
			this.UUID.HeaderText = "UUID";
			this.UUID.Name = "UUID";
			this.DeviceName.HeaderText = "Device Name";
			this.DeviceName.Name = "DeviceName";
			this.Grid.AutoSizeColumnsMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.Grid.BackgroundColor = global::System.Drawing.Color.FromArgb(64, 64, 64);
			this.Grid.ColumnHeadersHeightSizeMode = global::System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.Grid.Columns.AddRange(new global::System.Windows.Forms.DataGridViewColumn[] { this.DeviceName, this.UUID, this.Port, this.Status, this.Stats, this.WDAStatus });
			this.Grid.ContextMenuStrip = this.contextMenuStrip1;
			this.Grid.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.Grid.Location = new global::System.Drawing.Point(0, 24);
			this.Grid.Name = "Grid";
			this.Grid.RowHeadersVisible = false;
			this.Grid.Size = new global::System.Drawing.Size(851, 426);
			this.Grid.TabIndex = 5;
			this.menuStrip1.BackColor = global::System.Drawing.Color.Black;
			this.menuStrip1.Items.AddRange(new global::System.Windows.Forms.ToolStripItem[] { this.readDevicesToolStripMenuItem, this.settingsToolStripMenuItem, this.logsToolStripMenuItem, this.aboutToolStripMenuItem, this.setUpToolStripMenuItem, this.tESTToolStripMenuItem });
			this.menuStrip1.Location = new global::System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new global::System.Drawing.Size(851, 24);
			this.menuStrip1.TabIndex = 7;
			this.menuStrip1.Text = "menuStrip1";
			this.readDevicesToolStripMenuItem.ForeColor = global::System.Drawing.Color.White;
			this.readDevicesToolStripMenuItem.Name = "readDevicesToolStripMenuItem";
			this.readDevicesToolStripMenuItem.Size = new global::System.Drawing.Size(88, 20);
			this.readDevicesToolStripMenuItem.Text = "Read Devices";
			this.readDevicesToolStripMenuItem.Click += new global::System.EventHandler(this.readDevicesToolStripMenuItem_Click);
			this.settingsToolStripMenuItem.ForeColor = global::System.Drawing.Color.White;
			this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
			this.settingsToolStripMenuItem.Size = new global::System.Drawing.Size(61, 20);
			this.settingsToolStripMenuItem.Text = "Settings";
			this.settingsToolStripMenuItem.Click += new global::System.EventHandler(this.settingsToolStripMenuItem_Click);
			this.logsToolStripMenuItem.ForeColor = global::System.Drawing.Color.White;
			this.logsToolStripMenuItem.Name = "logsToolStripMenuItem";
			this.logsToolStripMenuItem.Size = new global::System.Drawing.Size(44, 20);
			this.logsToolStripMenuItem.Text = "Logs";
			this.logsToolStripMenuItem.Click += new global::System.EventHandler(this.logsToolStripMenuItem_Click);
			this.aboutToolStripMenuItem.ForeColor = global::System.Drawing.Color.White;
			this.aboutToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("aboutToolStripMenuItem.Image");
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new global::System.Drawing.Size(68, 20);
			this.aboutToolStripMenuItem.Text = "About";
			this.aboutToolStripMenuItem.Click += new global::System.EventHandler(this.aboutToolStripMenuItem_Click);
			this.setUpToolStripMenuItem.Font = new global::System.Drawing.Font("Segoe UI Black", 9f, global::System.Drawing.FontStyle.Bold, global::System.Drawing.GraphicsUnit.Point, 204);
			this.setUpToolStripMenuItem.ForeColor = global::System.Drawing.Color.Lime;
			this.setUpToolStripMenuItem.Name = "setUpToolStripMenuItem";
			this.setUpToolStripMenuItem.Size = new global::System.Drawing.Size(58, 20);
			this.setUpToolStripMenuItem.Text = "Set Up";
			this.setUpToolStripMenuItem.Click += new global::System.EventHandler(this.setUpToolStripMenuItem_Click);
			this.tESTToolStripMenuItem.Name = "tESTToolStripMenuItem";
			this.tESTToolStripMenuItem.Size = new global::System.Drawing.Size(43, 20);
			this.tESTToolStripMenuItem.Text = "TEST";
			this.tESTToolStripMenuItem.Click += new global::System.EventHandler(this.tESTToolStripMenuItem_Click);
			this.downloadBugVideosToolStripMenuItem.Name = "downloadBugVideosToolStripMenuItem";
			this.downloadBugVideosToolStripMenuItem.Size = new global::System.Drawing.Size(192, 22);
			this.downloadBugVideosToolStripMenuItem.Text = "Download BugVideos";
			this.downloadBugVideosToolStripMenuItem.Click += new global::System.EventHandler(this.downloadBugVideosToolStripMenuItem_Click);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(851, 450);
			base.Controls.Add(this.statusStrip1);
			base.Controls.Add(this.Grid);
			base.Controls.Add(this.menuStrip1);
			base.Icon = (global::System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.MainMenuStrip = this.menuStrip1;
			base.Name = "Form1";
			this.Text = "Form1";
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.contextMenuStrip1.ResumeLayout(false);
			((global::System.ComponentModel.ISupportInitialize)this.Grid).EndInit();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x0400050C RID: 1292
		private global::System.ComponentModel.IContainer components;

		// Token: 0x0400050D RID: 1293
		private global::System.Windows.Forms.ToolStripStatusLabel TotalCaptchaTimesLabel;

		// Token: 0x0400050E RID: 1294
		private global::System.Windows.Forms.ToolStripStatusLabel TotalPostUnsucessLabel;

		// Token: 0x0400050F RID: 1295
		private global::System.Windows.Forms.ToolStripStatusLabel TotalPostLabel;

		// Token: 0x04000510 RID: 1296
		private global::System.Windows.Forms.ToolStripStatusLabel TotalBadAccountsLabel;

		// Token: 0x04000511 RID: 1297
		private global::System.Windows.Forms.ToolStripStatusLabel TotalGoodAccountsLabel;

		// Token: 0x04000512 RID: 1298
		private global::System.Windows.Forms.StatusStrip statusStrip1;

		// Token: 0x04000513 RID: 1299
		private global::System.Windows.Forms.ToolStripStatusLabel TotalBadPicturesLabel;

		// Token: 0x04000514 RID: 1300
		private global::System.Windows.Forms.ToolStripMenuItem readAllFilesToolStripMenuItem;

		// Token: 0x04000515 RID: 1301
		private global::System.Windows.Forms.ToolStripMenuItem stopAllDevicesToolStripMenuItem;

		// Token: 0x04000516 RID: 1302
		private global::System.Windows.Forms.ToolStripMenuItem startAllDevicesToolStripMenuItem;

		// Token: 0x04000517 RID: 1303
		private global::System.Windows.Forms.ToolStripMenuItem stopWorkToolStripMenuItem;

		// Token: 0x04000518 RID: 1304
		private global::System.Windows.Forms.ToolStripMenuItem startWorkToolStripMenuItem;

		// Token: 0x04000519 RID: 1305
		private global::System.Windows.Forms.ContextMenuStrip contextMenuStrip1;

		// Token: 0x0400051A RID: 1306
		private global::System.Windows.Forms.DataGridViewTextBoxColumn WDAStatus;

		// Token: 0x0400051B RID: 1307
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Stats;

		// Token: 0x0400051C RID: 1308
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Status;

		// Token: 0x0400051D RID: 1309
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Port;

		// Token: 0x0400051E RID: 1310
		private global::System.Windows.Forms.DataGridViewTextBoxColumn UUID;

		// Token: 0x0400051F RID: 1311
		private global::System.Windows.Forms.DataGridViewTextBoxColumn DeviceName;

		// Token: 0x04000520 RID: 1312
		private global::System.Windows.Forms.DataGridView Grid;

		// Token: 0x04000521 RID: 1313
		private global::System.Windows.Forms.MenuStrip menuStrip1;

		// Token: 0x04000522 RID: 1314
		private global::System.Windows.Forms.ToolStripMenuItem readDevicesToolStripMenuItem;

		// Token: 0x04000523 RID: 1315
		private global::System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;

		// Token: 0x04000524 RID: 1316
		private global::System.Windows.Forms.ToolStripMenuItem logsToolStripMenuItem;

		// Token: 0x04000525 RID: 1317
		private global::System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;

		// Token: 0x04000526 RID: 1318
		private global::System.Windows.Forms.ToolStripMenuItem setUpToolStripMenuItem;

		// Token: 0x04000527 RID: 1319
		private global::System.Windows.Forms.ToolStripMenuItem tESTToolStripMenuItem;

		// Token: 0x04000528 RID: 1320
		private global::System.Windows.Forms.ToolStripMenuItem configurateShortCutsToolStripMenuItem;

		// Token: 0x04000529 RID: 1321
		private global::System.Windows.Forms.ToolStripStatusLabel TotalSlowTimesLabel;

		// Token: 0x0400052A RID: 1322
		private global::System.Windows.Forms.ToolStripMenuItem downloadBugVideosToolStripMenuItem;
	}
}
