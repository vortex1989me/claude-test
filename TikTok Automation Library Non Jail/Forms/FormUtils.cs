using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TikTok_Automation_Library_Non_Jail.Utils;

namespace TikTok_Automation_Library_Non_Jail.Forms
{
	// Token: 0x020000A9 RID: 169
	internal class FormUtils
	{
		// Token: 0x0600022B RID: 555 RVA: 0x0000319F File Offset: 0x0000139F
		private static void UpdateDirectory(string DirectoryPath)
		{
			if (!Directory.Exists(DirectoryPath))
			{
				Directory.CreateDirectory(DirectoryPath);
			}
		}

		// Token: 0x0600022C RID: 556 RVA: 0x000031B0 File Offset: 0x000013B0
		private static void UpdateLists(ref List<string> list, string Path, string[] DefaultData)
		{
			if (File.Exists(Path))
			{
				list = new List<string>(File.ReadAllLines(Path));
				return;
			}
			File.WriteAllLines(Path, DefaultData);
			list = new List<string>(File.ReadAllLines(Path));
		}

		// Token: 0x0600022D RID: 557 RVA: 0x000031DC File Offset: 0x000013DC
		private static void UpdateLists(ref ConcurrentQueue<string> list, string Path, string[] DefaultData)
		{
			if (File.Exists(Path))
			{
				list = new ConcurrentQueue<string>(File.ReadAllLines(Path));
				return;
			}
			File.WriteAllLines(Path, DefaultData);
			list = new ConcurrentQueue<string>(File.ReadAllLines(Path));
		}

		// Token: 0x0600022E RID: 558 RVA: 0x00060540 File Offset: 0x0005E740
		public static async Task UploadAllData()
		{
			FormUtils.UpdateDirectory("Settings");
			FormUtils.UpdateDirectory("Settings/Devices");
			FormUtils.UpdateDirectory("Temp");
			FormUtils.UpdateDirectory("ScreenShots");
			FormUtils.UpdateDirectory("ScreenShotsExceptions");
			FormUtils.UpdateDirectory("Content");
			FormUtils.UpdateDirectory("Content/Videos");
			FormUtils.UpdateDirectory("Content/CommentsImages");
			FormUtils.UpdateDirectory("Content/IMAGE_1");
			FormUtils.UpdateDirectory("Content/IMAGE_2");
			FormUtils.UpdateDirectory("Profile");
			FormUtils.UpdateDirectory("Profile/Pictures");
			FormUtils.UpdateDirectory("OutData");
			FormUtils.UpdateDirectory("OutData/Accounts");
			FormUtils.UpdateDirectory("OutData/ReWork");
			FormUtils.UpdateDirectory("Certifitats");
			Form1.ProfileSettings.Pictures = FileSystemUtils.GetAllPictures("Profile/Pictures");
			Form1.Content.Videos = FileSystemUtils.GetAllVideos("Content/Videos");
			Form1.Content.CommentPictures = FileSystemUtils.GetAllPictures("Content/CommentsImages");
			FormUtils.UpdateLists(ref Form1.iPhoneData, "Settings/Devices/iPhoneData.txt", new string[] { "63f50bc67ae1e5ce577a40bef57974e8bad28808:IMG_0010.HEIC:IMG_0011.MOV:8110", "deviceUID:imgName:videoName:PortNumber" });
			if (File.Exists("Settings/Devices/iPhoneImages.txt"))
			{
				using (List<string>.Enumerator enumerator = new List<string>(File.ReadAllLines("Settings/Devices/iPhoneImages.txt")).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string text = enumerator.Current;
						string[] array = text.Split(new char[] { ':' });
						try
						{
							Form1.iPhoneImages.Add(array[0], array[1] + ":" + array[2]);
						}
						catch
						{
						}
					}
					goto IL_0182;
				}
			}
			File.WriteAllText("Settings/Devices/iPhoneImages.txt", "uid:pic1:pic2");
			IL_0182:
			FormUtils.UpdateLists(ref Form1.Settings.ProxyList, "Settings/ProxyList.txt", new string[] { "ip:port", "ip:port:login:password" });
			FormUtils.UpdateLists(ref Form1.Settings.TagsList, "Settings/TagsList.txt", new string[] { "#girl", "#dance", "#bikini", "#hotgirl", "#fyp" });
			FormUtils.UpdateLists(ref Form1.Settings.SecondComments, "Settings/SecondComments.txt", new string[] { "TAP HERE!", "JOIN US!" });
			FormUtils.UpdateLists(ref Form1.Settings.MusicUrls, "Settings/MusicUrls.txt", new string[] { "https://www.tiktok.com/music/-7238878454710536966" });
			FormUtils.UpdateLists(ref Form1.Settings.MentionsList, "Settings/MentionsList.txt", new string[] { "balmizapola", "balmiz" });
			FormUtils.UpdateLists(ref Form1.Settings.TempMailProxies, "Settings/TempMailProxies.txt", new string[] { "" });
			FormUtils.UpdateLists(ref Form1.Settings.FirstMails, "Settings/FirstMails.txt", new string[] { "" });
			FormUtils.UpdateLists(ref Form1.Settings.REGIONS_MAKING, "Settings/REGIONS_MAKING.txt", new string[] { "Kazakhstan", "Armenia", "Albania", "Georgia" });
			FormUtils.UpdateLists(ref Form1.Settings.REGIONS_POSTING, "Settings/REGIONS_POSTING.txt", new string[] { "Baltimore" });
			FormUtils.UpdateLists(ref Form1.Settings.REGIONS_POSTING_VPN, "Settings/REGIONS_POSTING_VPN.txt", new string[] { "Texas", "Alabama", "Oregon", "Washington DC" });
			FormUtils.UpdateLists(ref Form1.ProfileSettings.BioCreating, "Profile/BioCreating.txt", new string[] { "Some Bio 1", "Some bio 2" });
			FormUtils.UpdateLists(ref Form1.ProfileSettings.BioPosting, "Profile/BioPosting.txt", new string[] { "Some Bio 1", "Some bio 2" });
			FormUtils.UpdateLists(ref Form1.ProfileSettings.Names, "Profile/Names.txt", new string[] { "Alba", "Anny" });
			FormUtils.UpdateLists(ref Form1.OutData.BugVideos, "OutData/BugVideos.txt", new string[] { "" });
			FormUtils.UpdateLists(ref Form1.OutData.ReWorkSignedOk, "OutData/ReWork/ReWorkSignedOk.txt", new string[] { "" });
			FormUtils.UpdateLists(ref Form1.OutData.ReWorkNotSigned, "OutData/ReWork/ReWorkNotSigned.txt", new string[] { "" });
			FormUtils.UpdateLists(ref Form1.OutData.ReWorkSlowProblem, "OutData/ReWork/ReWorkSlowProblem.txt", new string[] { "" });
			FormUtils.UpdateLists(ref Form1.OutData.ReWorkRisk, "OutData/ReWork/ReWorkRisk.txt", new string[] { "" });
			FormUtils.UpdateLists(ref Form1.OutData.ReWorkStats, "OutData/ReWork/ReWorkStats.txt", new string[] { "" });
			Form1.OutData.ReworkAccounts = new ConcurrentDictionary<string, List<string>>();
			string text2 = "OutData/ReworkAccounts.txt";
			if (File.Exists(text2))
			{
				List<string> list = new List<string>(File.ReadAllLines(text2));
				List<string> list2 = new List<string>();
				foreach (string text3 in list)
				{
					try
					{
						string[] array2 = text3.Split(new char[] { ':' });
						if (!list2.Contains(array2[0]))
						{
							list2.Add(array2[0]);
						}
					}
					catch
					{
					}
				}
				using (List<string>.Enumerator enumerator = list2.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string text4 = enumerator.Current;
						List<string> list3 = new List<string>();
						foreach (string text5 in list)
						{
							try
							{
								if (text5.Split(new char[] { ':' })[0] == text4)
								{
									list3.Add(text5);
								}
							}
							catch
							{
							}
						}
						Form1.OutData.ReworkAccounts.TryAdd(text4, list3);
					}
					return;
				}
			}
			File.Create(text2);
		}

		// Token: 0x0600022F RID: 559 RVA: 0x0006057C File Offset: 0x0005E77C
		public static string GetMentionFromList(List<string> usedMentions)
		{
			if (FormUtils.CheckTime <= DateTime.Now)
			{
				try
				{
					FormUtils.CheckTime = DateTime.Now.AddMinutes(2.0);
					List<string> list = new List<string>(File.ReadAllLines("Settings/MentionsList.txt"));
					if (list.Count != Form1.Settings.MentionsList.Count)
					{
						object obj = FormUtils.sync;
						lock (obj)
						{
							Form1.Settings.MentionsList = list;
						}
					}
				}
				catch
				{
				}
			}
			if (usedMentions != null)
			{
				int num = 0;
				string text;
				for (;;)
				{
					text = Form1.Settings.MentionsList[ConstParams.rand.Next(Form1.Settings.MentionsList.Count)];
					if (!usedMentions.Contains(text))
					{
						break;
					}
					if (usedMentions.Count >= Form1.Settings.MentionsList.Count)
					{
						return text;
					}
					num++;
					if (num >= 20)
					{
						return text;
					}
				}
				return text;
			}
			return Form1.Settings.MentionsList[ConstParams.rand.Next(Form1.Settings.MentionsList.Count)];
		}

		// Token: 0x0400055F RID: 1375
		private static object sync = new object();

		// Token: 0x04000560 RID: 1376
		private static DateTime CheckTime = DateTime.Now.AddMinutes(2.0);
	}
}
