using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using TikTok_Automation_Library_Non_Jail.Security;
using TikTok_Automation_Library_Non_Jail.Utils;
using TikTok_Automation_Library_Non_Jail.Utils.TempMail;
using TikTok_Automation_Library_Non_Jail.Work;

namespace TikTok_Automation_Library_Non_Jail.Forms
{
	// Token: 0x0200009E RID: 158
	public partial class Form1 : Form
	{
		// Token: 0x06000206 RID: 518 RVA: 0x0005DBD0 File Offset: 0x0005BDD0
		private void Timer_Tick(object sender, EventArgs e)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			for (int i = 0; i < this.iPhonesList.Count; i++)
			{
				try
				{
					if (Form1.Started[i] && !this.Workers[i].Started)
					{
						Form1.Started[i] = false;
						if (this.Workers[i].port == null)
						{
							goto IL_027D;
						}
						try
						{
							Form1.Class17 @class = new Form1.Class17();
							@class.Field0 = this;
							@class.potok = i;
							Form1.tasks.Add(Task.Run(new Func<Task>(@class.Method0)));
							goto IL_027D;
						}
						catch
						{
							goto IL_027D;
						}
					}
					if (this.Workers[i].Started)
					{
						try
						{
							this.Grid.SetNums(i, string.Format("{0}/{1}/{2}/{3}/{4}/{5}/{6}/{7}", new object[]
							{
								this.Workers[i].TotalGoodAccountss,
								this.Workers[i].TotalBadAccounts,
								this.Workers[i].TotalPosted,
								this.Workers[i].TotalUnsuccessPosted,
								this.Workers[i].TotalCaptchaTimes,
								this.Workers[i].BadPictures,
								this.Workers[i].TotalReWorkAccounts,
								this.Workers[i].TotalReWorkTimes
							}));
						}
						catch
						{
						}
						num += this.Workers[i].TotalGoodAccountss;
						num2 += this.Workers[i].TotalBadAccounts;
						num3 += this.Workers[i].TotalPosted;
						num4 += this.Workers[i].TotalUnsuccessPosted;
						num5 += this.Workers[i].TotalSlowTimes;
						num6 += this.Workers[i].TotalCaptchaTimes;
						num7 += this.Workers[i].BadPictures;
						try
						{
							this.Grid.SetWDAStatus(i, this.Workers[i].device.wda.Runned ? "Online" : "Offline");
						}
						catch
						{
						}
					}
					IL_027D:;
				}
				catch
				{
				}
			}
			this.TotalGoodAccountsLabel.Text = "Total Good Accounts: " + num.ToString();
			this.TotalBadAccountsLabel.Text = "Total Bad Accounts: " + num2.ToString();
			this.TotalPostLabel.Text = "Total Post: " + num3.ToString();
			this.TotalPostUnsucessLabel.Text = "Total Post Unsucess: " + num4.ToString();
			this.TotalSlowTimesLabel.Text = "Total Slow Times: " + num5.ToString();
			this.TotalCaptchaTimesLabel.Text = "Total Captcha Times: " + num6.ToString();
			this.TotalBadPicturesLabel.Text = "Total Bad Pictures: " + num7.ToString();
			for (int j = 0; j < Form1.StatusList.Count; j++)
			{
				try
				{
					this.Grid.SetStatus(j, Form1.StatusList[j]);
				}
				catch
				{
				}
			}
			if (Form1.OutData.UserNamesOnly.Count > 0)
			{
				ConcurrentQueue<string> userNamesOnly = Form1.OutData.UserNamesOnly;
				Form1.OutData.UserNamesOnly = new ConcurrentQueue<string>();
				File.AppendAllLines("OutData/UserNamesOnly.txt", userNamesOnly);
			}
			if (Form1.OutData.AccountsWithData.Count > 0)
			{
				ConcurrentQueue<string> accountsWithData = Form1.OutData.AccountsWithData;
				Form1.OutData.AccountsWithData = new ConcurrentQueue<string>();
				File.AppendAllLines("OutData/Accounts/" + DateTime.Now.Date.ToShortDateString() + ".txt", accountsWithData);
			}
			if (Form1.OutData.AccountsWithRiskOnly.Count > 0)
			{
				ConcurrentQueue<string> accountsWithRiskOnly = Form1.OutData.AccountsWithRiskOnly;
				Form1.OutData.AccountsWithRiskOnly = new ConcurrentQueue<string>();
				File.AppendAllLines("OutData/Accounts/RISK.txt", accountsWithRiskOnly);
			}
			if (Form1.OutData.AccountsWithProblems.Count > 0)
			{
				ConcurrentQueue<string> accountsWithProblems = Form1.OutData.AccountsWithProblems;
				Form1.OutData.AccountsWithProblems = new ConcurrentQueue<string>();
				File.AppendAllLines("OutData/Accounts/WITH PROBLEMS.txt", accountsWithProblems);
			}
			if (Form1.OutData.AccountsWithResettedPasswords.Count > 0)
			{
				ConcurrentQueue<string> accountsWithResettedPasswords = Form1.OutData.AccountsWithResettedPasswords;
				Form1.OutData.AccountsWithResettedPasswords = new ConcurrentQueue<string>();
				File.AppendAllLines("OutData/AccountsWithResettedPasswords.txt", accountsWithResettedPasswords);
			}
			if (Form1.OutData.FA2Accounts.Count > 0)
			{
				ConcurrentQueue<string> fa2Accounts = Form1.OutData.FA2Accounts;
				Form1.OutData.FA2Accounts = new ConcurrentQueue<string>();
				File.AppendAllLines("OutData/Accounts/2FA " + DateTime.Now.Date.ToShortDateString() + ".txt", fa2Accounts);
			}
			if (Form1.OutData.GoodFirstMails.Count > 0)
			{
				ConcurrentQueue<string> goodFirstMails = Form1.OutData.GoodFirstMails;
				Form1.OutData.GoodFirstMails = new ConcurrentQueue<string>();
				File.AppendAllLines("OutData/GoodFirstMails.txt", goodFirstMails);
			}
			if (Form1.OutData.BadFirstMails.Count > 0)
			{
				ConcurrentQueue<string> badFirstMails = Form1.OutData.BadFirstMails;
				Form1.OutData.BadFirstMails = new ConcurrentQueue<string>();
				File.AppendAllLines("OutData/BadFirstMails.txt", badFirstMails);
			}
			string text = "Profile/BioCreating.txt";
			try
			{
				File.Delete(text);
			}
			catch
			{
			}
			File.WriteAllLines(text, Form1.ProfileSettings.BioCreating);
			text = "Profile/BioPosting.txt";
			try
			{
				File.Delete(text);
			}
			catch
			{
			}
			File.WriteAllLines(text, Form1.ProfileSettings.BioPosting);
			List<string> list = new List<string>();
			foreach (List<string> list2 in Form1.OutData.ReworkAccounts.Values)
			{
				list.AddRange(list2);
			}
			if (File.Exists("OutData/BugVideos.txt"))
			{
				File.Delete("OutData/BugVideos.txt");
			}
			File.WriteAllLines("OutData/BugVideos.txt", Form1.OutData.BugVideos);
			if (File.Exists("OutData/ReworkAccounts.txt"))
			{
				File.Delete("OutData/ReworkAccounts.txt");
			}
			File.WriteAllLines("OutData/ReworkAccounts.txt", list);
			if (File.Exists("OutData/ReWork/ReWorkSignedOk.txt"))
			{
				File.Delete("OutData/ReWork/ReWorkSignedOk.txt");
			}
			File.WriteAllLines("OutData/ReWork/ReWorkSignedOk.txt", Form1.OutData.ReWorkSignedOk);
			if (File.Exists("OutData/ReWork/ReWorkNotSigned.txt"))
			{
				File.Delete("OutData/ReWork/ReWorkNotSigned.txt");
			}
			File.WriteAllLines("OutData/ReWork/ReWorkNotSigned.txt", Form1.OutData.ReWorkNotSigned);
			if (File.Exists("OutData/ReWork/ReWorkSlowProblem.txt"))
			{
				File.Delete("OutData/ReWork/ReWorkSlowProblem.txt");
			}
			File.WriteAllLines("OutData/ReWork/ReWorkSlowProblem.txt", Form1.OutData.ReWorkSlowProblem);
			if (File.Exists("OutData/ReWork/ReWorkRisk.txt"))
			{
				File.Delete("OutData/ReWork/ReWorkRisk.txt");
			}
			File.WriteAllLines("OutData/ReWork/ReWorkRisk.txt", Form1.OutData.ReWorkRisk);
			if (File.Exists("OutData/ReWork/ReWorkStats.txt"))
			{
				File.Delete("OutData/ReWork/ReWorkStats.txt");
			}
			File.WriteAllLines("OutData/ReWork/ReWorkStats.txt", Form1.OutData.ReWorkStats);
			if (File.Exists("Settings/ProxyList.txt"))
			{
				File.Delete("Settings/ProxyList.txt");
			}
			File.WriteAllLines("Settings/ProxyList.txt", Form1.Settings.ProxyList);
			if (File.Exists("Settings/TempMailProxies.txt"))
			{
				File.Delete("Settings/TempMailProxies.txt");
			}
			File.WriteAllLines("Settings/TempMailProxies.txt", Form1.Settings.TempMailProxies);
			if (File.Exists("Settings/FirstMails.txt"))
			{
				File.Delete("Settings/FirstMails.txt");
			}
			File.WriteAllLines("Settings/FirstMails.txt", Form1.Settings.FirstMails);
			if (File.Exists("Profile/Names.txt"))
			{
				File.Delete("Profile/Names.txt");
			}
			File.WriteAllLines("Profile/Names.txt", Form1.ProfileSettings.Names);
		}

		// Token: 0x06000207 RID: 519 RVA: 0x0005E3E0 File Offset: 0x0005C5E0
		public Form1()
		{
			this.InitializeComponent();
			string[] array = Application.ProductName.Split(new char[] { ' ' });
			this.Text = string.Concat(new string[]
			{
				array[0],
				" ",
				array[1],
				" ",
				Application.ProductVersion,
				" - NON JAIL"
			});
			Task.Run(new Func<Task>(Form1.Class18.Field0.Method0));
			this.timer = new Timer();
			this.timer.Interval = 5000;
			this.timer.Tick += this.Timer_Tick;
		}

		// Token: 0x06000208 RID: 520 RVA: 0x0005E4E0 File Offset: 0x0005C6E0
		private async void readDevicesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Form1.Class19 @class = new Form1.Class19();
			@class.Field0 = this;
			@class.uids = new List<string>();
			foreach (iPhoneData iPhoneData in this.iPhonesList)
			{
				@class.uids.Add(iPhoneData.UUID);
			}
			@class.NewUIDS = new List<string>();
			foreach (string text in CMDUtils.WriteCMD("tidevice list", 10000))
			{
				if (text.Contains("iPhone"))
				{
					string text2 = text;
					while (text2.Contains("  "))
					{
						text2 = text2.Replace("  ", " ");
					}
					string[] array2 = text2.Split(new char[] { ' ' });
					iPhoneData iPhoneData2 = new iPhoneData();
					iPhoneData2.UUID = array2[0];
					if (!@class.uids.Contains(iPhoneData2.UUID))
					{
						@class.NewUIDS.Add(iPhoneData2.UUID);
						iPhoneData2.DeviceName = array2[2];
						if (array2[3].Contains("iPhone") && array2[4].Contains("iPhone"))
						{
							iPhoneData2.DeviceName = iPhoneData2.DeviceName + " " + array2[3];
							iPhoneData2.MarketName = array2[4] + (array2[5].Contains(".") ? "" : (" " + array2[5]));
						}
						else if (array2[3].Contains("iPhone"))
						{
							iPhoneData2.MarketName = array2[3] + (array2[4].Contains(".") ? "" : (" " + array2[4]));
						}
						else
						{
							iPhoneData2.DeviceName = iPhoneData2.DeviceName + " " + array2[3];
							iPhoneData2.MarketName = array2[4] + (array2[5].Contains(".") ? "" : (" " + array2[5]));
						}
						iPhoneData2.ProductVersion = array2[array2.Length - 2];
						this.iPhonesList.Add(iPhoneData2);
					}
				}
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (string text3 in new List<string>(File.ReadAllLines("Settings/Devices/iPhoneData.txt")))
			{
				try
				{
					string[] array3 = text3.Split(new char[] { ':' });
					dictionary.Add(array3[0], array3[3]);
				}
				catch
				{
				}
			}
			foreach (iPhoneData iPhoneData3 in this.iPhonesList)
			{
				if (dictionary.ContainsKey(iPhoneData3.UUID))
				{
					iPhoneData3.Port = dictionary[iPhoneData3.UUID];
				}
			}
			List<iPhoneData> list = this.iPhonesList.OrderBy<iPhoneData, string>(new Func<iPhoneData, string>(Form1.Class18.Field0.Method1)).ToList<iPhoneData>();
			this.iPhonesList = list;
			if (this.Grid.Rows.Count < 2)
			{
				this.Grid.Rows.Clear();
				this.Grid.Rows.Add(this.iPhonesList.Count);
			}
			else
			{
				while (this.Grid.Rows.Count < this.iPhonesList.Count + 1)
				{
					this.Grid.Rows.Add();
				}
			}
			this.Grid.Columns[0].HeaderText = string.Format("Device Name [{0}]", this.iPhonesList.Count);
			await Task.Run(new Action(@class.Method0));
			foreach (string text4 in new List<string>(File.ReadAllLines("Settings/Devices/iPhoneData.txt")))
			{
				string[] array4 = text4.Split(new char[] { ':' });
				int num = 0;
				using (List<Worker>.Enumerator enumerator3 = this.Workers.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						if (enumerator3.Current.phoneUID == array4[0])
						{
							this.Workers[num].originalPictureUrl = array4[1];
							this.Workers[num].originalVideoUrl = array4[2];
							this.Workers[num].port = array4[3];
							this.Grid.SetDevicePort(num, array4[3]);
							break;
						}
						num++;
					}
				}
			}
			this.timer.Start();
		}

		// Token: 0x06000209 RID: 521 RVA: 0x0005E518 File Offset: 0x0005C718
		private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				this.settingsForm.Show();
			}
			catch
			{
				this.settingsForm = new SettingsForm();
				this.settingsForm.Show();
			}
		}

		// Token: 0x0600020A RID: 522 RVA: 0x0005E55C File Offset: 0x0005C75C
		private void logsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				this.logs.Show();
			}
			catch
			{
				this.logs = new LogsForm();
				this.logs.Show();
			}
		}

		// Token: 0x0600020B RID: 523 RVA: 0x00003077 File Offset: 0x00001277
		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
		}

		// Token: 0x0600020C RID: 524 RVA: 0x0005E5A0 File Offset: 0x0005C7A0
		private void startWorkToolStripMenuItem_Click(object sender, EventArgs e)
		{
			int rowIndex = this.Grid.CurrentCell.RowIndex;
			for (;;)
			{
				try
				{
					if (!string.IsNullOrEmpty(this.Workers[rowIndex].port))
					{
						if (this.Workers[rowIndex].port != "0")
						{
							Form1.Started[rowIndex] = true;
							this.Workers[rowIndex].Work = true;
							this.Grid.SetStatus(rowIndex, "Running!");
							break;
						}
						MessageBox.Show("Port is 0");
					}
					else
					{
						MessageBox.Show("Port is 0");
					}
					continue;
				}
				catch
				{
					continue;
				}
				break;
			}
		}

		// Token: 0x0600020D RID: 525 RVA: 0x0005E658 File Offset: 0x0005C858
		private void stopWorkToolStripMenuItem_Click(object sender, EventArgs e)
		{
			int rowIndex = this.Grid.CurrentCell.RowIndex;
			for (;;)
			{
				try
				{
					this.Workers[rowIndex].Work = false;
					this.Workers[rowIndex].Started = false;
				}
				catch
				{
					continue;
				}
				break;
			}
		}

		// Token: 0x0600020E RID: 526 RVA: 0x0005E6B0 File Offset: 0x0005C8B0
		private void readAllFilesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			int rowIndex = this.Grid.CurrentCell.RowIndex;
			try
			{
				string text = "100APPLE";
				if (this.Workers[rowIndex].PhoneModel.Contains("X"))
				{
					text = "101APPLE";
				}
				foreach (string text2 in CMDUtils.WriteCMD("tidevice -u " + this.Workers[rowIndex].phoneUID + " fsync ls /DCIM/" + text, 10000))
				{
					if (!string.IsNullOrEmpty(text2) && !text2.Contains("Microsoft ") && !text2.Contains("tidevice") && !text2.Contains("\\"))
					{
						LogsUtils.WriteLog(text2);
					}
				}
			}
			catch
			{
			}
		}

		// Token: 0x0600020F RID: 527 RVA: 0x0005E788 File Offset: 0x0005C988
		private void startAllDevicesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < Form1.Started.Count; i++)
			{
				try
				{
					if (!string.IsNullOrEmpty(this.Workers[i].port) && this.Workers[i].port != "0")
					{
						Form1.Started[i] = true;
						this.Workers[i].Work = true;
						this.Grid.SetStatus(i, "Running!");
					}
				}
				catch
				{
				}
			}
		}

		// Token: 0x06000210 RID: 528 RVA: 0x0005E824 File Offset: 0x0005CA24
		private void stopAllDevicesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < Form1.Started.Count; i++)
			{
				try
				{
					this.Workers[i].Work = false;
					this.Workers[i].Started = false;
				}
				catch
				{
				}
			}
		}

		// Token: 0x06000211 RID: 529 RVA: 0x0005E880 File Offset: 0x0005CA80
		private async void setUpToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await Task.Run(new Action(Form1.Class18.Field0.Method2));
		}

		// Token: 0x06000212 RID: 530 RVA: 0x0005E8B0 File Offset: 0x0005CAB0
		private async void tESTToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await new AnyMessageClient().OrderEmailAsync("long_outlook.com", "tiktok.com", null, null);
		}

		// Token: 0x06000213 RID: 531 RVA: 0x0005E8E0 File Offset: 0x0005CAE0
		private void configurateShortCutsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			int rowIndex = this.Grid.CurrentCell.RowIndex;
			for (;;)
			{
				try
				{
					if (!string.IsNullOrEmpty(this.Workers[rowIndex].port))
					{
						if (this.Workers[rowIndex].port != "0")
						{
							Form1.Started[rowIndex] = true;
							this.Workers[rowIndex].ConfigurateOnly = true;
							this.Workers[rowIndex].Work = true;
							this.Grid.SetStatus(rowIndex, "Running!");
							break;
						}
						MessageBox.Show("Port is 0");
					}
					else
					{
						MessageBox.Show("Port is 0");
					}
					continue;
				}
				catch
				{
					continue;
				}
				break;
			}
		}

		// Token: 0x06000214 RID: 532 RVA: 0x0005E9AC File Offset: 0x0005CBAC
		private void downloadBugVideosToolStripMenuItem_Click(object sender, EventArgs e)
		{
			int rowIndex = this.Grid.CurrentCell.RowIndex;
			foreach (string text in Form1.OutData.BugVideos)
			{
				try
				{
					string text2 = "100APPLE";
					if (this.Workers[rowIndex].PhoneModel.Contains("X"))
					{
						text2 = "101APPLE";
					}
					string[] array = text.Split(new char[] { ' ' });
					string text3 = array[0];
					string text4 = array[2];
					string text5 = array[array.Length - 1];
					if (this.Workers[rowIndex].phoneUID == text5)
					{
						if (!Directory.Exists("BugVideos"))
						{
							Directory.CreateDirectory("BugVideos");
						}
						List<string> list = new List<string>
						{
							"cd /d " + Environment.CurrentDirectory + "/BugVideos",
							string.Concat(new string[]
							{
								"tidevice -u ",
								this.Workers[rowIndex].phoneUID,
								" fsync pull /DCIM/",
								text2,
								"/",
								text3,
								" ",
								Environment.CurrentDirectory,
								"/BugVideos/",
								text3,
								"_",
								text4
							})
						};
						LogsUtils.WriteLog(string.Concat(new string[] { "[", text5, "]: Downloading Video: ", text3, " Created: ", text4 }));
						CMDUtils.WriteCMD(list);
						LogsUtils.WriteLog(string.Concat(new string[] { "[", text5, "]: Success Downloaded Video: ", text3, " Created: ", text4 }));
					}
				}
				catch (Exception ex)
				{
					LogsUtils.WriteLog(string.Concat(new string[]
					{
						"[",
						this.Workers[rowIndex].phoneUID,
						"]: Download Video ",
						text,
						" Exception: ",
						ex.ToString()
					}));
				}
			}
		}

		// Token: 0x040004FF RID: 1279
		private Timer timer;

		// Token: 0x04000500 RID: 1280
		private Checker checker = null;

		// Token: 0x04000501 RID: 1281
		private List<iPhoneData> iPhonesList = new List<iPhoneData>();

		// Token: 0x04000502 RID: 1282
		private List<Worker> Workers = new List<Worker>();

		// Token: 0x04000503 RID: 1283
		public static Dictionary<string, string> iPhoneImages = new Dictionary<string, string>();

		// Token: 0x04000504 RID: 1284
		public static List<string> iPhoneData = new List<string>();

		// Token: 0x04000505 RID: 1285
		public static List<bool> Started = new List<bool>();

		// Token: 0x04000506 RID: 1286
		public static List<Task> tasks = new List<Task>();

		// Token: 0x04000507 RID: 1287
		public static List<string> StatusList = new List<string>();

		// Token: 0x04000508 RID: 1288
		public static int BioUsed = 0;

		// Token: 0x04000509 RID: 1289
		public static int BioCount = 0;

		// Token: 0x0400050A RID: 1290
		private SettingsForm settingsForm = new SettingsForm();

		// Token: 0x0400050B RID: 1291
		private LogsForm logs = new LogsForm();

		// Token: 0x0200009F RID: 159
		public struct Settings
		{
			// Token: 0x0400052B RID: 1323
			public static List<string> TempMailProxies = new List<string>();

			// Token: 0x0400052C RID: 1324
			public static List<string> MusicUrls = new List<string>();

			// Token: 0x0400052D RID: 1325
			public static List<string> TagsList = new List<string>();

			// Token: 0x0400052E RID: 1326
			public static List<string> SecondComments = new List<string>();

			// Token: 0x0400052F RID: 1327
			public static List<string> MentionsList = new List<string>();

			// Token: 0x04000530 RID: 1328
			public static ConcurrentQueue<string> ProxyList = new ConcurrentQueue<string>();

			// Token: 0x04000531 RID: 1329
			public static ConcurrentQueue<string> FirstMails = new ConcurrentQueue<string>();

			// Token: 0x04000532 RID: 1330
			public static List<string> REGIONS_MAKING = new List<string>();

			// Token: 0x04000533 RID: 1331
			public static List<string> REGIONS_POSTING = new List<string>();

			// Token: 0x04000534 RID: 1332
			public static List<string> REGIONS_POSTING_VPN = new List<string>();
		}

		// Token: 0x020000A0 RID: 160
		public struct OutData
		{
			// Token: 0x04000535 RID: 1333
			public static ConcurrentQueue<string> AccountsWithData = new ConcurrentQueue<string>();

			// Token: 0x04000536 RID: 1334
			public static ConcurrentQueue<string> UserNamesOnly = new ConcurrentQueue<string>();

			// Token: 0x04000537 RID: 1335
			public static ConcurrentQueue<string> FA2Accounts = new ConcurrentQueue<string>();

			// Token: 0x04000538 RID: 1336
			public static ConcurrentQueue<string> AccountsWithRiskOnly = new ConcurrentQueue<string>();

			// Token: 0x04000539 RID: 1337
			public static ConcurrentQueue<string> AccountsWithProblems = new ConcurrentQueue<string>();

			// Token: 0x0400053A RID: 1338
			public static ConcurrentQueue<string> AccountsWithResettedPasswords = new ConcurrentQueue<string>();

			// Token: 0x0400053B RID: 1339
			public static ConcurrentDictionary<string, List<string>> ReworkAccounts = new ConcurrentDictionary<string, List<string>>();

			// Token: 0x0400053C RID: 1340
			public static ConcurrentQueue<string> ReWorkSignedOk = new ConcurrentQueue<string>();

			// Token: 0x0400053D RID: 1341
			public static ConcurrentQueue<string> ReWorkNotSigned = new ConcurrentQueue<string>();

			// Token: 0x0400053E RID: 1342
			public static ConcurrentQueue<string> ReWorkSlowProblem = new ConcurrentQueue<string>();

			// Token: 0x0400053F RID: 1343
			public static ConcurrentQueue<string> ReWorkRisk = new ConcurrentQueue<string>();

			// Token: 0x04000540 RID: 1344
			public static ConcurrentQueue<string> ReWorkStats = new ConcurrentQueue<string>();

			// Token: 0x04000541 RID: 1345
			public static ConcurrentQueue<string> GoodFirstMails = new ConcurrentQueue<string>();

			// Token: 0x04000542 RID: 1346
			public static ConcurrentQueue<string> BadFirstMails = new ConcurrentQueue<string>();

			// Token: 0x04000543 RID: 1347
			public static ConcurrentQueue<string> BugVideos = new ConcurrentQueue<string>();
		}

		// Token: 0x020000A1 RID: 161
		public struct ProfileSettings
		{
			// Token: 0x04000544 RID: 1348
			public static ConcurrentQueue<string> Pictures = new ConcurrentQueue<string>();

			// Token: 0x04000545 RID: 1349
			public static ConcurrentQueue<string> Names = new ConcurrentQueue<string>();

			// Token: 0x04000546 RID: 1350
			public static List<string> BioCreating = new List<string>();

			// Token: 0x04000547 RID: 1351
			public static ConcurrentQueue<string> BioPosting = new ConcurrentQueue<string>();
		}

		// Token: 0x020000A2 RID: 162
		public struct Content
		{
			// Token: 0x04000548 RID: 1352
			public static List<string> Videos = new List<string>();

			// Token: 0x04000549 RID: 1353
			public static ConcurrentQueue<string> CommentPictures = new ConcurrentQueue<string>();

			// Token: 0x0400054A RID: 1354
			public static ConcurrentQueue<string> Posting_IMAGE_1 = new ConcurrentQueue<string>();

			// Token: 0x0400054B RID: 1355
			public static ConcurrentQueue<string> Posting_IMAGE_2 = new ConcurrentQueue<string>();
		}

		// Token: 0x020000A3 RID: 163
		[CompilerGenerated]
		private sealed class Class17
		{
			// Token: 0x0600021D RID: 541 RVA: 0x00003137 File Offset: 0x00001337
			internal Task Method0()
			{
				return this.Field0.Workers[this.potok].StartWork(this.potok);
			}

			// Token: 0x0400054C RID: 1356
			public int potok;

			// Token: 0x0400054D RID: 1357
			public Form1 Field0;
		}

		// Token: 0x020000A4 RID: 164
		[CompilerGenerated]
		[Serializable]
		private sealed class Class18
		{
			// Token: 0x06000220 RID: 544 RVA: 0x00003166 File Offset: 0x00001366
			internal Task Method0()
			{
				return FormUtils.UploadAllData();
			}

			// Token: 0x06000221 RID: 545 RVA: 0x0000316D File Offset: 0x0000136D
			internal string Method1(iPhoneData o)
			{
				return o.Port;
			}

			// Token: 0x06000222 RID: 546 RVA: 0x0005F9BC File Offset: 0x0005DBBC
			internal void Method2()
			{
				try
				{
					CMDUtils.WriteCMD("pip install tidevice==0.12.8)", 10000);
				}
				catch
				{
				}
				try
				{
					CMDUtils.WriteCMD("pip3 install tidevice==0.12.8", 10000);
				}
				catch
				{
				}
				try
				{
					CMDUtils.WriteCMD("pip install --upgrade pip", 10000);
				}
				catch
				{
				}
				try
				{
					CMDUtils.WriteCMD("pip3 install --upgrade pip", 10000);
				}
				catch
				{
				}
			}

			// Token: 0x0400054E RID: 1358
			public static readonly Form1.Class18 Field0 = new Form1.Class18();

			// Token: 0x0400054F RID: 1359
			public static Func<Task> Field1;

			// Token: 0x04000550 RID: 1360
			public static Func<iPhoneData, string> Field2;

			// Token: 0x04000551 RID: 1361
			public static Action Field3;
		}

		// Token: 0x020000A5 RID: 165
		[CompilerGenerated]
		private sealed class Class19
		{
			// Token: 0x06000224 RID: 548 RVA: 0x0005FB7C File Offset: 0x0005DD7C
			internal void Method0()
			{
				int num = this.uids.Count;
				foreach (iPhoneData iPhoneData in this.Field0.iPhonesList)
				{
					if (this.NewUIDS.Contains(iPhoneData.UUID))
					{
						this.Field0.Grid.SetDeviceName(num, iPhoneData.MarketName + " | " + iPhoneData.ProductVersion);
						this.Field0.Grid.SetDeviceUUID(num, iPhoneData.UUID);
						this.Field0.Grid.SetDevicePort(num, "0");
						this.Field0.Grid.SetStatus(num, "Ready");
						this.Field0.Grid.SetNums(num, "0/0/0/0/0/0");
						this.Field0.Grid.SetWDAStatus(num, "Closed");
						Form1.Started.Add(false);
						this.Field0.Workers.Add(new Worker());
						this.Field0.Workers[num].phoneUID = iPhoneData.UUID;
						this.Field0.Workers[num].PhoneModel = iPhoneData.MarketName;
						string[] array = iPhoneData.ProductVersion.Split(new char[] { '.' });
						if (array.Length >= 2)
						{
							iPhoneData.ProductVersion = array[0] + "," + array[1];
						}
						this.Field0.Workers[num].iOSVersion = double.Parse(iPhoneData.ProductVersion);
						Form1.StatusList.Add("Ready");
						num++;
					}
				}
			}

			// Token: 0x04000552 RID: 1362
			public List<string> uids;

			// Token: 0x04000553 RID: 1363
			public Form1 Field0;

			// Token: 0x04000554 RID: 1364
			public List<string> NewUIDS;
		}
	}
}
