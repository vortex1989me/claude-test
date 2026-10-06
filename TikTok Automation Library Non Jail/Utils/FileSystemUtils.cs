using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TikTok_Automation_Library_Non_Jail.Forms;
using TikTok_Automation_Library_Non_Jail.Properties;

namespace TikTok_Automation_Library_Non_Jail.Utils
{
	// Token: 0x02000081 RID: 129
	public class FileSystemUtils
	{
		// Token: 0x06000144 RID: 324 RVA: 0x0005A210 File Offset: 0x00058410
		public static void RemoveFile(string file)
		{
			try
			{
				File.Delete(file);
			}
			catch
			{
			}
		}

		// Token: 0x06000145 RID: 325 RVA: 0x0005A238 File Offset: 0x00058438
		public static string GetBio(bool IsMaking)
		{
			string text = null;
			if (IsMaking)
			{
				for (;;)
				{
					try
					{
						if (Form1.BioUsed < Settings.Default.Use1BioOn)
						{
							text = Form1.ProfileSettings.BioCreating[Form1.BioCount];
							Interlocked.Increment(ref Form1.BioUsed);
							return text;
						}
						for (;;)
						{
							try
							{
								Form1.BioUsed = 0;
							}
							catch
							{
								continue;
							}
							break;
						}
						Interlocked.Increment(ref Form1.BioCount);
						if (Form1.BioCount < Form1.ProfileSettings.BioCreating.Count)
						{
							continue;
						}
					}
					catch
					{
					}
					break;
				}
				return Form1.ProfileSettings.BioCreating[ConstParams.rand.Next(Form1.ProfileSettings.BioCreating.Count)];
			}
			if (Form1.ProfileSettings.BioPosting.Count == 0)
			{
				object obj = FileSystemUtils.sync;
				lock (obj)
				{
					MessageBox.Show("Bio for Posting is empty!\nFill the list and click 'OK'");
				}
				Form1.ProfileSettings.BioPosting = new ConcurrentQueue<string>(File.ReadAllLines("Profile/BioPosting.txt"));
			}
			Form1.ProfileSettings.BioPosting.TryDequeue(out text);
			return text;
		}

		// Token: 0x06000146 RID: 326 RVA: 0x0005A350 File Offset: 0x00058550
		public static string GetTempVideo(string PhoneUID, string Username)
		{
			string text = "Temp/" + PhoneUID + "/" + Username;
			if (Directory.Exists(text))
			{
				string[] files = Directory.GetFiles(text, "*.mp4");
				if (files.Length != 0)
				{
					return files[0];
				}
			}
			return null;
		}

		// Token: 0x06000147 RID: 327 RVA: 0x0005A38C File Offset: 0x0005858C
		public static string GetRandomVideo(string VideoFolder)
		{
			for (;;)
			{
				try
				{
					Form1.Content.Videos = FileSystemUtils.GetAllVideos(VideoFolder);
					if (Form1.Content.Videos.Count == 0)
					{
						if (VideoFolder != null && Settings.Default.VideosFrom1Folder)
						{
							return null;
						}
						object obj = FileSystemUtils.sync;
						lock (obj)
						{
							MessageBox.Show("Don't have video's!");
						}
						continue;
					}
				}
				catch
				{
				}
				break;
			}
			return Form1.Content.Videos[ConstParams.rand.Next(Form1.Content.Videos.Count)];
		}

		// Token: 0x06000148 RID: 328 RVA: 0x0005A430 File Offset: 0x00058630
		public static async Task RefreshVideo(string phoneUID, string videoUrl, string originalVideoUrl, bool IsX, bool Remove = true)
		{
			try
			{
				string text = "100APPLE";
				if (IsX)
				{
					text = "101APPLE";
				}
				CMDUtils.WriteCMD(string.Concat(new string[] { "tidevice -u ", phoneUID, " fsync push \"", videoUrl, "\" /DCIM/", text, "/", originalVideoUrl }), 10000);
				await Task.Delay(TimeSpan.FromSeconds(2.0));
				if (Settings.Default.RemoveVideoAfterUse && Remove)
				{
					FileSystemUtils.RemoveFile(videoUrl);
				}
			}
			catch
			{
			}
		}

		// Token: 0x06000149 RID: 329 RVA: 0x0005A494 File Offset: 0x00058694
		public static async Task RefreshPicture(string phoneUID, string pictureUrl, string originalPictureUrl, bool IsX)
		{
			try
			{
				string text = "100APPLE";
				if (IsX)
				{
					text = "101APPLE";
				}
				CMDUtils.WriteCMD(string.Concat(new string[] { "tidevice -u ", phoneUID, " fsync push \"", pictureUrl, "\" /DCIM/", text, "/", originalPictureUrl }), 10000);
				await Task.Delay(TimeSpan.FromSeconds(5.0));
				FileSystemUtils.RemoveFile(pictureUrl);
			}
			catch
			{
			}
		}

		// Token: 0x0600014A RID: 330 RVA: 0x0005A4F0 File Offset: 0x000586F0
		public static string GetRandomProfilePicture()
		{
			string text;
			while (!Form1.ProfileSettings.Pictures.TryDequeue(out text))
			{
				object obj;
				if (Form1.ProfileSettings.Pictures.Count == 0)
				{
					Form1.ProfileSettings.Pictures = FileSystemUtils.GetAllPictures("Profile/Pictures");
					if (Form1.ProfileSettings.Pictures.Count != 0)
					{
						continue;
					}
					obj = FileSystemUtils.sync;
					lock (obj)
					{
						MessageBox.Show("Please Fill Up Profile Pictures Directory!");
						continue;
					}
				}
				obj = FileSystemUtils.sync;
				lock (obj)
				{
					MessageBox.Show("Profile Picture read error!");
				}
			}
			return text;
		}

		// Token: 0x0600014B RID: 331 RVA: 0x0005A5A0 File Offset: 0x000587A0
		public static string GetRandomCommentPicture()
		{
			string text;
			while (!Form1.Content.CommentPictures.TryDequeue(out text))
			{
				object obj;
				if (Form1.Content.CommentPictures.Count == 0)
				{
					Form1.Content.CommentPictures = FileSystemUtils.GetAllPictures("Content/CommentsImages");
					if (Form1.Content.CommentPictures.Count != 0)
					{
						continue;
					}
					obj = FileSystemUtils.sync;
					lock (obj)
					{
						MessageBox.Show("Please Fill Up Content/CommentsImages Directory!");
						continue;
					}
				}
				obj = FileSystemUtils.sync;
				lock (obj)
				{
					MessageBox.Show("Content/CommentsImages read error!");
				}
			}
			return text;
		}

		// Token: 0x0600014C RID: 332 RVA: 0x0005A650 File Offset: 0x00058850
		public static string GetRandomPosting_IMAGE_1()
		{
			string text;
			while (!Form1.Content.Posting_IMAGE_1.TryDequeue(out text))
			{
				object obj;
				if (Form1.Content.Posting_IMAGE_1.Count == 0)
				{
					Form1.Content.Posting_IMAGE_1 = FileSystemUtils.GetAllPictures("Content/IMAGE_1");
					if (Form1.Content.Posting_IMAGE_1.Count != 0)
					{
						continue;
					}
					obj = FileSystemUtils.sync;
					lock (obj)
					{
						MessageBox.Show("Please Fill Up Content/IMAGE_1 Directory!");
						continue;
					}
				}
				obj = FileSystemUtils.sync;
				lock (obj)
				{
					MessageBox.Show("Content/IMAGE_1 read error!");
				}
			}
			return text;
		}

		// Token: 0x0600014D RID: 333 RVA: 0x0005A700 File Offset: 0x00058900
		public static string GetRandomPosting_IMAGE_2()
		{
			string text;
			while (!Form1.Content.Posting_IMAGE_2.TryDequeue(out text))
			{
				object obj;
				if (Form1.Content.Posting_IMAGE_2.Count == 0)
				{
					Form1.Content.Posting_IMAGE_2 = FileSystemUtils.GetAllPictures("Content/IMAGE_2");
					if (Form1.Content.Posting_IMAGE_2.Count != 0)
					{
						continue;
					}
					obj = FileSystemUtils.sync;
					lock (obj)
					{
						MessageBox.Show("Please Fill Up Content/IMAGE_2 Directory!");
						continue;
					}
				}
				obj = FileSystemUtils.sync;
				lock (obj)
				{
					MessageBox.Show("Content/IMAGE_2 read error!");
				}
			}
			return text;
		}

		// Token: 0x0600014E RID: 334 RVA: 0x0005A7B0 File Offset: 0x000589B0
		public static ConcurrentQueue<string> GetAllPictures(string dir = "Profile/Pictures")
		{
			List<string> list;
			ConcurrentQueue<string> concurrentQueue;
			for (;;)
			{
				Thread.Sleep(new Random(DateTime.Now.Millisecond).Next(1000));
				list = new List<string>(Directory.GetDirectories(dir));
				list.Add(dir);
				concurrentQueue = new ConcurrentQueue<string>();
				if (list.Count > 0)
				{
					break;
				}
				MessageBox.Show("Don't have folders with pictures\nPlease fill and press ok");
			}
			foreach (string text in list)
			{
				string[] files = Directory.GetFiles(text, "*.jpg");
				if (files.Length != 0)
				{
					foreach (string text2 in files)
					{
						string text3 = text2;
						string[] array2 = text2.Split(new char[] { '\\' });
						string text4 = array2[array2.Length - 1];
						if (text4.Contains(" "))
						{
							text3 = text2.Replace(text4, text4.Trim().Replace(" ", ""));
							File.Move(text2, text3);
						}
						concurrentQueue.Enqueue(text3);
					}
				}
				else
				{
					try
					{
						if (!text.Contains("Profile/Pictures") && !text.Contains("Profile\\Pictures") && text != dir)
						{
							Directory.Delete(text);
						}
					}
					catch
					{
					}
				}
			}
			Console.WriteLine("Found " + concurrentQueue.Count.ToString() + " pictures");
			return concurrentQueue;
		}

		// Token: 0x0600014F RID: 335 RVA: 0x0005A948 File Offset: 0x00058B48
		public static List<string> GetAllVideos(string dir = "Content/Videos")
		{
			if (dir == null)
			{
				dir = "Content/Videos";
			}
			List<string> list3;
			for (;;)
			{
				try
				{
					Thread.Sleep(new Random(DateTime.Now.Millisecond).Next(1000));
					List<string> list = new List<string>(Directory.GetDirectories(dir));
					list.Add(dir);
					List<string> list2 = new List<string>();
					if (list.Count <= 0)
					{
						MessageBox.Show("Don't have folder \"" + dir + "\" with videos \nPlease fill and continue");
						continue;
					}
					foreach (string text in list)
					{
						try
						{
							string[] files = Directory.GetFiles(text, "*.mp4");
							if (files.Length != 0)
							{
								foreach (string text2 in files)
								{
									string text3 = text2;
									string[] array2 = text2.Split(new char[] { '\\' });
									string text4 = array2[array2.Length - 1];
									if (text4.Contains(" "))
									{
										text3 = text2.Replace(text4, text4.Trim().Replace(" ", ""));
										File.Move(text2, text3);
									}
									list2.Add(text3);
								}
							}
							else if (text != "Content/Videos")
							{
								Directory.Delete(text);
							}
						}
						catch
						{
						}
					}
					Console.WriteLine("Found " + list2.Count.ToString() + " video's");
					list3 = list2;
				}
				catch
				{
					continue;
				}
				break;
			}
			return list3;
		}

		// Token: 0x0400045E RID: 1118
		private static object sync = new object();
	}
}
