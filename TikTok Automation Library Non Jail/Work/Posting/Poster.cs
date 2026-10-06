using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TikTok_Automation_Library_Non_Jail.Forms;
using TikTok_Automation_Library_Non_Jail.Properties;
using TikTok_Automation_Library_Non_Jail.Utils;
using TikTok_Automation_Library_Non_Jail.Work.Utils;
using WDA_Framework;

namespace TikTok_Automation_Library_Non_Jail.Work.Posting
{
	// Token: 0x02000068 RID: 104
	internal class Poster
	{
		// Token: 0x06000106 RID: 262 RVA: 0x0004A6AC File Offset: 0x000488AC
		public async Task<bool> RefreshPictures3(string originalPictureUrl)
		{
			bool flag;
			try
			{
				int picturesPostCount = Settings.Default.PicturesPostCount;
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting random IMAGE_1");
				string text = FileSystemUtils.GetRandomPosting_IMAGE_1();
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Pushing IMAGE_1");
				await FileSystemUtils.RefreshPicture(this.phoneUID, text, originalPictureUrl, this.device.PhoneModel.Contains("X"));
				if (picturesPostCount == 1)
				{
					flag = true;
				}
				else
				{
					string[] array = Form1.iPhoneImages[this.phoneUID].Split(new char[] { ':' });
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting random IMAGE_2 #1");
					text = FileSystemUtils.GetRandomPosting_IMAGE_2();
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Pushing IMAGE_2 #1");
					await FileSystemUtils.RefreshPicture(this.phoneUID, text, array[0], this.device.PhoneModel.Contains("X"));
					if (picturesPostCount == 2)
					{
						flag = true;
					}
					else
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting random IMAGE_2 #2");
						text = FileSystemUtils.GetRandomPosting_IMAGE_2();
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Pushing IMAGE_2 #2");
						await FileSystemUtils.RefreshPicture(this.phoneUID, text, array[1], this.device.PhoneModel.Contains("X"));
						flag = true;
					}
				}
			}
			catch (Exception ex)
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Refresh Images Exception: " + ex.ToString());
				flag = false;
			}
			return flag;
		}

		// Token: 0x06000107 RID: 263 RVA: 0x0004A6F8 File Offset: 0x000488F8
		public async Task<Poster.PostingResult> PostVideo(string phoneUID, string originalVideoUrl, string originalPictureUrl, bool IsFirst, List<string> UsedMentions, TikTokUtils tiktokUtils, bool CheckBoxesOk)
		{
			int num;
			for (;;)
			{
				WorkUtils.WriteLog("[" + phoneUID + "]: Posting");
				num = 0;
				try
				{
					await tiktokUtils.GoHome(true);
					if (!Settings.Default.PostSameVideo)
					{
						await tiktokUtils.TerminateTikTok();
					}
					TaskAwaiter<bool> taskAwaiter2;
					if (Settings.Default.PostPictures)
					{
						TaskAwaiter<bool> taskAwaiter = this.RefreshPictures3(originalPictureUrl).GetAwaiter();
						if (!taskAwaiter.IsCompleted)
						{
							await taskAwaiter;
							taskAwaiter = taskAwaiter2;
							taskAwaiter2 = default(TaskAwaiter<bool>);
						}
						if (!taskAwaiter.GetResult())
						{
							continue;
						}
					}
					else if ((Settings.Default.PostSameVideo && IsFirst) || !Settings.Default.PostSameVideo)
					{
						WorkUtils.WriteLog("[" + phoneUID + "]: GRV");
						string randomVideo = FileSystemUtils.GetRandomVideo(this.VideoFolder);
						if (string.IsNullOrEmpty(randomVideo))
						{
							this.VideoFolder = null;
							return Poster.PostingResult.NoHaveVideo;
						}
						if (Settings.Default.VideosFrom1Folder && this.VideoFolder == null)
						{
							string[] array = randomVideo.Split(new char[] { '\\' });
							this.VideoFolder = randomVideo.Replace(array[array.Length - 2], "");
						}
						string[] array2 = randomVideo.Split(new char[] { '\\' });
						string[] array3 = array2[array2.Length - 1].Split(new char[] { '.' });
						string text = randomVideo.Replace(array2[array2.Length - 1], Guid.NewGuid().ToString().Replace("-", "") + "." + array3[array3.Length - 1]);
						File.Move(randomVideo, text);
						File.Delete(randomVideo);
						WorkUtils.WriteLog("[" + phoneUID + "]: RV");
						TaskAwaiter<bool> taskAwaiter = this.RefreshVideo(phoneUID, text, originalVideoUrl, true).GetAwaiter();
						if (!taskAwaiter.IsCompleted)
						{
							await taskAwaiter;
							taskAwaiter = taskAwaiter2;
							taskAwaiter2 = default(TaskAwaiter<bool>);
						}
						if (!taskAwaiter.GetResult())
						{
							continue;
						}
						await Task.Delay(TimeSpan.FromSeconds(2.0));
						File.Delete(text);
						text = null;
					}
					WorkUtils.WriteLog("[" + phoneUID + "]: Posting start");
					TikTokUtils.ActivateResult activateResult = await tiktokUtils.ActivateTikTokGotoProfile();
					if (activateResult != TikTokUtils.ActivateResult.Success)
					{
						switch (activateResult)
						{
						case TikTokUtils.ActivateResult.Unsuccess:
							return Poster.PostingResult.Unsuccess;
						case TikTokUtils.ActivateResult.UnAuth:
							return Poster.PostingResult.UnAuth;
						case TikTokUtils.ActivateResult.Blocked:
							return Poster.PostingResult.Blocked;
						case TikTokUtils.ActivateResult.AccountStatus:
							return Poster.PostingResult.AccountStatus;
						}
					}
					await this.device.GetSourceXml();
					if (this.device.IsElementInXml("Post not created due to a potential violation of our Community Guidelines.", null) || this.device.IsElementInXmlContains("Post not created due to a potential violation of our Community Guidelines", null))
					{
						return Poster.PostingResult.NotCreateError;
					}
					Exception ex;
					for (;;)
					{
						TaskAwaiter<bool> taskAwaiter = this.RecordVideo(IsFirst, tiktokUtils).GetAwaiter();
						if (!taskAwaiter.IsCompleted)
						{
							await taskAwaiter;
							taskAwaiter = taskAwaiter2;
							taskAwaiter2 = default(TaskAwaiter<bool>);
						}
						if (!taskAwaiter.GetResult())
						{
							break;
						}
						bool flag = false;
						for (;;)
						{
							int num2 = 0;
							try
							{
								if (Settings.Default.AddTags)
								{
									await this.WriteTags();
								}
								if (Settings.Default.PlusMention)
								{
									string mentionFromList = FormUtils.GetMentionFromList(UsedMentions);
									UsedMentions.Add(mentionFromList);
									await this.PlusMention(mentionFromList);
								}
							}
							catch (Exception obj)
							{
								num2 = 1;
							}
							int num3 = num2;
							object obj;
							if (num3 == 1)
							{
								ex = (Exception)obj;
								WorkUtils.WriteLog("[" + phoneUID + "]: WT OR PM Exception: " + ex.ToString());
								if (flag)
								{
									goto IL_0A97;
								}
								flag = true;
								num3 = 0;
								try
								{
									WorkUtils.WriteLog("[" + phoneUID + "]: Click Next button");
									await (await this.device.WaitForElementByXPath("XCUIElementTypeButton", 0, "Next", 10.0)).TapAlert(0, 0, false);
									try
									{
										await (await this.device.WaitForElementBy(0, "Next", 3.0)).Click(false);
									}
									catch
									{
									}
									continue;
								}
								catch
								{
									num3 = 1;
								}
								if (num3 == 1)
								{
									break;
								}
							}
							obj = null;
							WorkUtils.WriteLog(string.Format("[{0}]: Check Boxes Status: {1}", phoneUID, CheckBoxesOk));
							if (!CheckBoxesOk)
							{
								CheckBoxesOk = await this.TurnOffCheckBoxes();
							}
							try
							{
								Poster.PostingResult postingResult = await this.PostAndBackToProfile(UsedMentions, originalPictureUrl, tiktokUtils);
								if (postingResult == Poster.PostingResult.CheckAgain)
								{
									continue;
								}
								await this.device.GetSourceXml();
								if (this.device.IsElementInXml("Post not created due to a potential violation of our Community Guidelines.", null) || this.device.IsElementInXmlContains("Post not created due to a potential violation of our Community Guidelines", null))
								{
									return Poster.PostingResult.NotCreateError;
								}
								return postingResult;
							}
							catch
							{
							}
							goto IL_0C6A;
						}
						WorkUtils.WriteLog("[" + phoneUID + "]: Click Camera button");
						await (await this.device.WaitForElementBy(0, "Camera,", 10.0)).TapAlert(0, 0, false);
					}
					return Poster.PostingResult.Unsuccess;
					IL_0A97:
					throw ex;
					IL_0C6A:
					return Poster.PostingResult.Unsuccess;
				}
				catch (Exception obj2)
				{
					num = 1;
				}
				break;
			}
			Poster.PostingResult postingResult2;
			if (num == 1)
			{
				object obj2;
				WorkUtils.WriteLog("[" + phoneUID + "]: Posting exception: " + ((Exception)obj2).ToString());
				string text = phoneUID;
				WorkUtils.WriteLog("[" + text + "]: Source: " + await this.device.GetSource());
				text = null;
				try
				{
					await tiktokUtils.TerminateTikTok();
				}
				catch
				{
				}
				await tiktokUtils.GoHome(true);
				postingResult2 = Poster.PostingResult.Unsuccess;
			}
			return postingResult2;
		}

		// Token: 0x06000108 RID: 264 RVA: 0x0004A778 File Offset: 0x00048978
		public async Task<bool> RecordVideo(bool IsFirst, TikTokUtils tikTokUtils)
		{
			Element element;
			bool flag;
			TaskAwaiter<bool> taskAwaiter;
			TaskAwaiter<bool> taskAwaiter2;
			for (;;)
			{
				try
				{
					element = await this.device.WaitForElementBy(0, "Create", 10.0);
					Element element2 = element;
					if (element2 != null)
					{
						flag = await element2.Enabled();
						if (flag)
						{
							flag = await element2.Visible();
						}
						if (flag)
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Create button A");
							await element2.Click(false);
							await Task.Delay(TimeSpan.FromSeconds(2.0));
						}
						else
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Create button hidden by some element");
							await this.device.GetSourceXml();
							taskAwaiter = tikTokUtils.ActivateChecks().GetAwaiter();
							if (!taskAwaiter.IsCompleted)
							{
								await taskAwaiter;
								taskAwaiter = taskAwaiter2;
								taskAwaiter2 = default(TaskAwaiter<bool>);
							}
							if (taskAwaiter.GetResult())
							{
								continue;
							}
						}
					}
					else
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Create button is not found");
					}
					element2 = null;
				}
				catch
				{
				}
				break;
			}
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Searching for Take Photo button");
			element = await this.device.WaitForElementBy(0, "recordPageUploadButton", 4.0);
			Element element3 = element;
			if (element3 == null)
			{
				try
				{
					DateTime dateTime = DateTime.Now.AddSeconds(3.0);
					while (DateTime.Now < dateTime)
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Element is null, checking source");
						await this.device.GetSourceXml();
						if (this.device.IsElementInXml("Allow", null))
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Allow Button E");
							await (await this.device.WaitForElementBy(0, "Allow", 3.0)).Click(false);
							dateTime = DateTime.Now.AddSeconds(3.0);
						}
						else if (this.device.IsElementInXmlContains("Allow Full", null))
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Allow Full Button E");
							await (await this.device.WaitForElementBy(3, "Allow Full", 3.0)).Click(false);
							dateTime = DateTime.Now.AddSeconds(3.0);
						}
						else if (this.device.IsElementInXmlContains("Continue", null))
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Check for Continue Button E");
							await (await this.device.WaitForElementBy(0, "Continue", 3.0)).Click(false);
							dateTime = DateTime.Now.AddSeconds(3.0);
						}
					}
					goto IL_1429;
				}
				catch
				{
					goto IL_1429;
				}
			}
			taskAwaiter = element3.Visible().GetAwaiter();
			if (!taskAwaiter.IsCompleted)
			{
				await taskAwaiter;
				taskAwaiter = taskAwaiter2;
				taskAwaiter2 = default(TaskAwaiter<bool>);
			}
			flag = !taskAwaiter.GetResult();
			if (!flag)
			{
				taskAwaiter = element3.Enabled().GetAwaiter();
				if (!taskAwaiter.IsCompleted)
				{
					await taskAwaiter;
					taskAwaiter = taskAwaiter2;
					taskAwaiter2 = default(TaskAwaiter<bool>);
				}
				flag = !taskAwaiter.GetResult();
			}
			int i;
			object obj;
			if (flag)
			{
				i = 0;
				try
				{
					DateTime dateTime = DateTime.Now.AddSeconds(3.0);
					while (DateTime.Now < dateTime)
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Element is not avalible, checking source");
						await this.device.GetSourceXml();
						if (this.device.IsElementInXml("Allow", null))
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Allow Button W");
							await (await this.device.WaitForElementBy(0, "Allow", 3.0)).Click(false);
							dateTime = DateTime.Now.AddSeconds(3.0);
						}
						else if (this.device.IsElementInXmlContains("Allow Full", null))
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Allow Full Button W");
							await (await this.device.WaitForElementBy(3, "Allow Full", 3.0)).Click(false);
							dateTime = DateTime.Now.AddSeconds(3.0);
						}
						else if (this.device.IsElementInXmlContains("Continue", null))
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Check for Continue Button W");
							await (await this.device.WaitForElementBy(0, "Continue", 3.0)).Click(false);
							dateTime = DateTime.Now.AddSeconds(3.0);
						}
					}
				}
				catch (Exception obj)
				{
					i = 1;
				}
				if (i == 1)
				{
					Exception ex = (Exception)obj;
					string text = this.phoneUID;
					string text2 = ex.ToString();
					WorkUtils.WriteLog(string.Concat(new string[]
					{
						"[",
						text,
						"]: Exception: ",
						text2,
						" Source: ",
						await this.device.GetSource()
					}));
					text = null;
					text2 = null;
					DateTime dateTime = DateTime.Now.AddSeconds(3.0);
					while (DateTime.Now < dateTime)
					{
						await this.device.GetSourceXml();
						if (this.device.IsElementInXml("Allow", null))
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Allow Button Q");
							await (await this.device.WaitForElementBy(0, "Allow", 3.0)).Click(false);
							dateTime = DateTime.Now.AddSeconds(3.0);
						}
						else if (this.device.IsElementInXmlContains("Allow Full", null))
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Allow Full Button Q");
							await (await this.device.WaitForElementBy(3, "Allow Full", 3.0)).Click(false);
							dateTime = DateTime.Now.AddSeconds(3.0);
						}
						else if (this.device.IsElementInXmlContains("Continue", null))
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Check for Continue Button Q");
							await (await this.device.WaitForElementBy(0, "Continue", 3.0)).Click(false);
							dateTime = DateTime.Now.AddSeconds(3.0);
						}
					}
				}
				obj = null;
			}
			else
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Element is ok");
			}
			IL_1429:
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Upload Button");
			i = 0;
			try
			{
				await (await this.device.WaitForElementByXPath("XCUIElementTypeButton", 0, "recordPageUploadButton", 10.0)).Click(false);
			}
			catch
			{
				i = 1;
			}
			if (i == 1)
			{
				await (await this.device.WaitForElementBy(5, "Upload", 10.0)).Click(false);
			}
			try
			{
				if (IsFirst)
				{
					await this.device.ConfigurateAppium(30);
					await Task.Delay(TimeSpan.FromSeconds(2.0));
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Allow Button");
					await (await this.device.WaitForElementBy(0, "Allow Full Access", 20.0)).Click(false);
					await this.device.ConfigurateAppium(17);
				}
			}
			catch
			{
			}
			if (Settings.Default.PostPictures)
			{
				await (await this.device.WaitForElementBy(0, "Photos", 5.0)).Click(false);
			}
			else
			{
				await (await this.device.WaitForElementBy(0, "Videos", 5.0)).Click(false);
				await Task.Delay(TimeSpan.FromSeconds(1.0));
				for (i = 0; i < 10; i++)
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Swipe");
					await this.device.Swipe(300, 300, 300, 800, 0.0);
					await Task.Delay(100);
				}
			}
			await Task.Delay(TimeSpan.FromSeconds(1.0));
			if (Settings.Default.PostPictures)
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Select Picture 1");
				await this.device.Tap(35.0, 175.0);
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Select Button");
				await (await this.device.WaitForElementBy(0, "Select", 5.0)).Click(false);
				if (Settings.Default.PicturesPostCount >= 2)
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Swipe");
					await this.device.Swipe(300, 150, 0, 150, 0.0);
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Select Button");
					await (await this.device.WaitForElementBy(0, "Select", 5.0)).Click(false);
					if (Settings.Default.PicturesPostCount >= 3)
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Swipe");
						await this.device.Swipe(300, 150, 0, 150, 0.0);
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Select Button");
						await (await this.device.WaitForElementBy(0, "Select", 5.0)).Click(false);
					}
				}
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Appium 100 | DSZXA");
				await this.device.ConfigurateAppium(100);
				await Task.Delay(TimeSpan.FromSeconds(2.0));
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Next button");
				await (await this.device.WaitForElementBy(3, "Next", 10.0)).Click(false);
			}
			else
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Select Video");
				await this.device.Tap(35.0, 175.0);
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Appium 100 | DSFDDA1");
				await this.device.ConfigurateAppium(100);
				await Task.Delay(TimeSpan.FromSeconds(2.0));
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Next button");
				await (await this.device.WaitForElementBy(0, "Next", 10.0)).Click(false);
			}
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Check for Your Story button");
			TaskAwaiter<Element> taskAwaiter3 = this.device.WaitForElementBy(0, "Your Story", 3.0).GetAwaiter();
			TaskAwaiter<Element> taskAwaiter4;
			if (!taskAwaiter3.IsCompleted)
			{
				await taskAwaiter3;
				taskAwaiter3 = taskAwaiter4;
				taskAwaiter4 = default(TaskAwaiter<Element>);
			}
			if (taskAwaiter3.GetResult() == null)
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Next button");
				i = 0;
				try
				{
					await (await this.device.WaitForElementBy(0, "Next", 10.0)).Click(false);
				}
				catch
				{
					i = 1;
				}
				if (i == 1)
				{
					try
					{
						await (await this.device.WaitForElementBy(3, "Next", 10.0)).Click(false);
					}
					catch
					{
					}
				}
			}
			await Task.Delay(TimeSpan.FromSeconds(2.0));
			if (Settings.Default.AddMusic)
			{
				await this.AddSound();
			}
			else if (Settings.Default.AddRandomMusic)
			{
				await this.AddRecomendSoundAndMute();
			}
			i = 0;
			try
			{
				if (Settings.Default.PostPictures)
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Next button");
					await (await this.device.WaitForElementBy(0, "Next", 10.0)).Click(false);
				}
				else
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Next button");
					await (await this.device.WaitForElementByXPath("XCUIElementTypeButton", 0, "Next", 10.0)).Click(false);
				}
			}
			catch (Exception obj)
			{
				i = 1;
			}
			if (i == 1)
			{
				Exception ex2 = (Exception)obj;
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Next button Error: " + ex2.ToString());
				int num = 0;
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Second tried to click Next Button");
					await (await this.device.WaitForElementBy(0, "Next", 3.0)).Click(false);
				}
				catch (Exception obj2)
				{
					num = 1;
				}
				object obj2;
				if (num == 1)
				{
					Exception ex3 = (Exception)obj2;
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Next button SECOND Error: " + ex2.ToString());
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Next button by COORDS");
					await this.device.Tap(290.0, 830.0);
				}
				obj2 = null;
				ex2 = null;
			}
			obj = null;
			try
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Check for Your Story button");
				taskAwaiter3 = this.device.WaitForElementBy(0, "Your Story", 3.0).GetAwaiter();
				if (!taskAwaiter3.IsCompleted)
				{
					await taskAwaiter3;
					taskAwaiter3 = taskAwaiter4;
					taskAwaiter4 = default(TaskAwaiter<Element>);
				}
				if (taskAwaiter3.GetResult() != null)
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Next button");
					i = 0;
					try
					{
						await (await this.device.WaitForElementBy(0, "Next", 10.0)).Click(false);
					}
					catch
					{
						i = 1;
					}
					if (i == 1)
					{
						try
						{
							await (await this.device.WaitForElementBy(3, "Next", 10.0)).Click(false);
						}
						catch
						{
						}
					}
				}
			}
			catch
			{
			}
			return true;
		}

		// Token: 0x06000109 RID: 265 RVA: 0x0004A7CC File Offset: 0x000489CC
		public async Task<Poster.PostingResult> PostAndBackToProfile(List<string> UserMentions, string originalPictureUrl, TikTokUtils tiktokUtils)
		{
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Appium DEF | 2");
			await this.device.ConfigurateAppium(17);
			int num = 0;
			try
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Post button");
				await (await this.device.WaitForElementBy(0, "Post", 10.0)).Click(false);
			}
			catch (Exception obj)
			{
				num = 1;
			}
			int num2 = num;
			object obj;
			if (num2 == 1)
			{
				Exception ex = (Exception)obj;
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Check for Your Story button");
				TaskAwaiter<Element> taskAwaiter = this.device.WaitForElementBy(0, "Your Story", 3.0).GetAwaiter();
				if (!taskAwaiter.IsCompleted)
				{
					await taskAwaiter;
					TaskAwaiter<Element> taskAwaiter2;
					taskAwaiter = taskAwaiter2;
					taskAwaiter2 = default(TaskAwaiter<Element>);
				}
				if (taskAwaiter.GetResult() != null)
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Next button");
					num2 = 0;
					try
					{
						await (await this.device.WaitForElementBy(0, "Next", 10.0)).Click(false);
						return Poster.PostingResult.CheckAgain;
					}
					catch
					{
						num2 = 1;
					}
					if (num2 != 1)
					{
						goto IL_06AE;
					}
					try
					{
						await (await this.device.WaitForElementBy(3, "Next", 10.0)).Click(false);
						return Poster.PostingResult.CheckAgain;
					}
					catch
					{
						goto IL_06AE;
					}
				}
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Next button WE");
				num2 = 0;
				try
				{
					await (await this.device.WaitForElementBy(0, "Next", 10.0)).Click(false);
					return Poster.PostingResult.CheckAgain;
				}
				catch
				{
					num2 = 1;
				}
				if (num2 == 1)
				{
					try
					{
						await (await this.device.WaitForElementBy(3, "Next", 10.0)).Click(false);
						return Poster.PostingResult.CheckAgain;
					}
					catch
					{
					}
				}
				IL_06AE:
				throw ex;
			}
			obj = null;
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Post Now Button");
			try
			{
				await (await this.device.WaitForElementBy(0, "Post Now", 5.0)).TapAlert(0, 0, false);
			}
			catch
			{
			}
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Don't Allow Alert");
			try
			{
				await (await this.device.WaitForElementBy(3, "t allow", 10.0)).TapAlert(0, 0, false);
			}
			catch
			{
			}
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for profile button");
			Element element = await this.device.WaitForElementBy(0, "Profile", 10.0);
			bool flag = await element.Enabled();
			if (flag)
			{
				flag = await element.Visible();
			}
			if (flag)
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Profile button");
				await element.Click(false);
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Appium 25 | 2");
				await this.device.ConfigurateAppium(25);
			}
			else
			{
				TikTokUtils.ActivateResult activateResult = await tiktokUtils.ActivateTikTokGotoProfile();
				if (activateResult != TikTokUtils.ActivateResult.Success)
				{
					switch (activateResult)
					{
					case TikTokUtils.ActivateResult.Unsuccess:
						return Poster.PostingResult.Unsuccess;
					case TikTokUtils.ActivateResult.UnAuth:
						return Poster.PostingResult.UnAuth;
					case TikTokUtils.ActivateResult.Blocked:
						return Poster.PostingResult.Blocked;
					case TikTokUtils.ActivateResult.AccountStatus:
						return Poster.PostingResult.AccountStatus;
					}
				}
			}
			for (;;)
			{
				try
				{
					Element element2 = await this.device.WaitForElementBy(3, "%", 4.0);
					if (element2 != null)
					{
						try
						{
							string text = await element2.Value();
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Check Uploading: " + text);
						}
						catch
						{
						}
						continue;
					}
				}
				catch
				{
				}
				break;
			}
			if (Settings.Default.AddComments)
			{
				await this.AddCommentOnVideo(UserMentions, originalPictureUrl, tiktokUtils);
			}
			return Poster.PostingResult.Success;
		}

		// Token: 0x0600010A RID: 266 RVA: 0x0004A828 File Offset: 0x00048A28
		private async Task AddCommentOnVideo(List<string> UsedMentions, string originalPictureUrl, TikTokUtils tiktokUtils)
		{
			string text = "";
			if (Settings.Default.AddMentionToComment)
			{
				FormUtils.GetMentionFromList(UsedMentions);
				UsedMentions.Add(text);
			}
			int num = 0;
			object obj;
			TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter;
			do
			{
				int num2 = 0;
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Swipe");
					await this.device.Swipe(150, 400, 150, 700, 0.0);
					await Task.Delay(TimeSpan.FromSeconds(3.0));
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Appium 18 | 1");
					await this.device.ConfigurateAppium(18);
					await (await this.device.WaitForElementBy(0, "profile_video", 10.0)).Click(false);
					try
					{
						await (await this.device.WaitForElementBy(0, "Not now", 5.0)).Click(false);
					}
					catch
					{
					}
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Comments Button");
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Appium DEF | Q1");
					await this.device.ConfigurateAppium(17);
					await (await this.device.WaitForElementBy(0, "feedCommentButton", 10.0)).Click(false);
					if (Settings.Default.AddMentionToComment)
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Tap Comments 0 by COORDS");
						await this.device.Tap(20.0, 259.0);
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Write @");
						await (await this.device.WaitForElementBy(0, "Add comment", 10.0)).SetText("@");
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Write username: " + text);
						await (await this.device.WaitForElementBy(0, "Add comment", 10.0)).SetText(text);
						await Task.Delay(TimeSpan.FromSeconds(5.0));
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Searching for found users");
						int num3 = 0;
						try
						{
							await (await this.device.WaitForElementBy(0, text, 10.0)).Click(false);
						}
						catch
						{
							num3 = 1;
						}
						if (num3 == 1)
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Select user by COORDS");
							await this.device.Tap(70.0, 285.0);
						}
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Post Comment Button");
						await (await this.device.WaitForElementBy(2, "Post comment", 10.0)).Click(false);
					}
					if (Settings.Default.AddTextToComment)
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Add Text Comment");
						await (await this.device.WaitForElementBy(3, "Add comment", 10.0)).SetText(Form1.Settings.SecondComments[ConstParams.rand.Next(Form1.Settings.SecondComments.Count)]);
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Post Comment Button");
						await (await this.device.WaitForElementBy(2, "Post comment", 10.0)).Click(false);
					}
					if (Settings.Default.AddImageToComment)
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting random picture");
						string randomCommentPicture = FileSystemUtils.GetRandomCommentPicture();
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Pushing picture");
						await FileSystemUtils.RefreshPicture(this.phoneUID, randomCommentPicture, originalPictureUrl, false);
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Photos button");
						await (await this.device.WaitForElementBy(0, "Photos", 10.0)).Click(false);
						await Task.Delay(TimeSpan.FromSeconds(2.0));
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Click First Image button by COORDS");
						await this.device.Tap(35.0, 130.0);
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Post Comment Button");
						await (await this.device.WaitForElementBy(2, "Post comment", 10.0)).Click(false);
					}
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Close Comment Button");
					await (await this.device.WaitForElementBy(0, "Close comment section", 10.0)).Click(false);
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Return Button");
					await (await this.device.WaitForElementBy(0, "returnButton", 10.0)).Click(false);
				}
				catch (Exception obj)
				{
					num2 = 1;
				}
				if (num2 != 1)
				{
					goto IL_1464;
				}
				Exception ex = (Exception)obj;
				await tiktokUtils.SaveScreenShot(true, TikTokUtils.ScreenshotType.Working);
				num++;
				if (num > 3)
				{
					break;
				}
				await tiktokUtils.TerminateTikTok();
				taskAwaiter = tiktokUtils.ActivateTikTokGotoProfile().GetAwaiter();
				if (!taskAwaiter.IsCompleted)
				{
					await taskAwaiter;
					TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter2;
					taskAwaiter = taskAwaiter2;
					taskAwaiter2 = default(TaskAwaiter<TikTokUtils.ActivateResult>);
				}
			}
			while (taskAwaiter.GetResult() == TikTokUtils.ActivateResult.Success);
			return;
			IL_1464:
			obj = null;
		}

		// Token: 0x0600010B RID: 267 RVA: 0x0004A884 File Offset: 0x00048A84
		public async Task AddSound()
		{
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Add Sound button");
			int num;
			try
			{
				num = 0;
				try
				{
					int num2 = 0;
					try
					{
						await (await this.device.WaitForElementBy(0, "Add sound", 3.0)).Click(false);
					}
					catch
					{
						num2 = 1;
					}
					if (num2 == 1)
					{
						await this.device.Tap(130.0, 80.0);
					}
				}
				catch
				{
					num = 1;
				}
				if (num == 1)
				{
					await (await this.device.WaitForElementByXPath("XCUIElementTypeButton", 0, "Sounds", 5.0)).Click(false);
				}
			}
			catch
			{
				throw new Exception();
			}
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Favorites button");
			num = 0;
			try
			{
				await (await this.device.WaitForElementBy(0, "Favorites", 10.0)).Click(false);
			}
			catch
			{
				num = 1;
			}
			if (num == 1)
			{
				await (await this.device.WaitForElementBy(0, "Favourites", 10.0)).Click(false);
			}
			WorkUtils.WriteLog("[" + this.phoneUID + "]: NEW // Click on first Music button");
			await this.device.Tap(300.0, 530.0);
			if (Settings.Default.TurnOffOriginalSound)
			{
				await this.MuteOriginalSound();
			}
			else if (Settings.Default.TurnOffAdditionalSound)
			{
				await this.MuteAdditionalSound();
			}
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Close music window");
			if (this.device.PhoneModel.Contains("11"))
			{
				await this.device.Tap(73.0, 200.0);
			}
			else
			{
				await this.device.Tap(150.0, 150.0);
			}
			TaskAwaiter<Element> taskAwaiter = this.device.WaitForElementBy(0, "Search sounds", 3.0).GetAwaiter();
			if (!taskAwaiter.IsCompleted)
			{
				await taskAwaiter;
				TaskAwaiter<Element> taskAwaiter2;
				taskAwaiter = taskAwaiter2;
				taskAwaiter2 = default(TaskAwaiter<Element>);
			}
			if (taskAwaiter.GetResult() != null)
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Close music window again");
				if (this.device.PhoneModel.Contains("11"))
				{
					await this.device.Tap(73.0, 200.0);
				}
				else
				{
					await this.device.Tap(150.0, 150.0);
				}
			}
		}

		// Token: 0x0600010C RID: 268 RVA: 0x0004A8C8 File Offset: 0x00048AC8
		private async Task MuteOriginalSound()
		{
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Volume");
			int num = 0;
			try
			{
				await (await this.device.WaitForElementBy(0, "IconSpeaker2LTR", 10.0)).Click(false);
			}
			catch
			{
				num = 1;
			}
			if (num == 1)
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Volume Button");
				await (await this.device.WaitForElementBy(0, "Volume", 10.0)).Click(false);
			}
			await Task.Delay(TimeSpan.FromSeconds(1.0));
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Swipe");
			await this.device.Swipe(187, 700, 0, 700, 0.0);
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Done Button");
			await (await this.device.WaitForElementBy(0, "Done", 10.0)).Click(false);
		}

		// Token: 0x0600010D RID: 269 RVA: 0x0004A90C File Offset: 0x00048B0C
		private async Task MuteAdditionalSound()
		{
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Volume");
			int num = 0;
			try
			{
				await (await this.device.WaitForElementBy(0, "IconSpeaker2LTR", 10.0)).Click(false);
			}
			catch
			{
				num = 1;
			}
			if (num == 1)
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Volume Button");
				await (await this.device.WaitForElementBy(0, "Volume", 10.0)).Click(false);
			}
			await Task.Delay(TimeSpan.FromSeconds(1.0));
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Swipe");
			await this.device.Swipe(187, 800, 0, 800, 0.0);
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Done Button");
			await (await this.device.WaitForElementBy(0, "Done", 10.0)).Click(false);
		}

		// Token: 0x0600010E RID: 270 RVA: 0x0004A950 File Offset: 0x00048B50
		private async Task AddRecomendSoundAndMute()
		{
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Add Sound button");
			try
			{
				int num = 0;
				try
				{
					int num2 = 0;
					try
					{
						List<Element> list = await this.device.WaitForElementsBy(0, "Add sound", 3.0);
						if (list.Count >= 2)
						{
							await list[0].Click(false);
						}
						else
						{
							if (list.Count == 1)
							{
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Random Music Already Added!");
								return;
							}
							throw new Exception();
						}
					}
					catch
					{
						num2 = 1;
					}
					if (num2 == 1)
					{
						await this.device.Tap(130.0, 80.0);
					}
				}
				catch
				{
					num = 1;
				}
				if (num == 1)
				{
					await (await this.device.WaitForElementByXPath("XCUIElementTypeButton", 0, "Sounds", 5.0)).Click(false);
				}
			}
			catch
			{
				throw new Exception();
			}
			try
			{
				await (await this.device.WaitForElementBy(0, "For You", 10.0)).Click(false);
			}
			catch
			{
			}
			await Task.Delay(TimeSpan.FromSeconds(2.0));
			WorkUtils.WriteLog("[" + this.phoneUID + "]: NEW // Click on first Music button");
			await this.device.Tap(300.0, 530.0);
			await Task.Delay(TimeSpan.FromSeconds(2.0));
			if (Settings.Default.TurnOffAdditionalSound)
			{
				await this.MuteAdditionalSound();
			}
			else if (Settings.Default.TurnOffOriginalSound)
			{
				await this.MuteOriginalSound();
			}
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Close music window");
			if (this.device.PhoneModel.Contains("11"))
			{
				await this.device.Tap(73.0, 200.0);
			}
			else
			{
				await this.device.Tap(150.0, 150.0);
			}
			TaskAwaiter<Element> taskAwaiter = this.device.WaitForElementBy(0, "Search sounds", 3.0).GetAwaiter();
			if (!taskAwaiter.IsCompleted)
			{
				await taskAwaiter;
				TaskAwaiter<Element> taskAwaiter2;
				taskAwaiter = taskAwaiter2;
				taskAwaiter2 = default(TaskAwaiter<Element>);
			}
			if (taskAwaiter.GetResult() != null)
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Close music window again");
				if (this.device.PhoneModel.Contains("11"))
				{
					await this.device.Tap(73.0, 200.0);
				}
				else
				{
					await this.device.Tap(150.0, 150.0);
				}
			}
		}

		// Token: 0x0600010F RID: 271 RVA: 0x0004A994 File Offset: 0x00048B94
		private async Task<bool> TurnOffCheckBoxes()
		{
			int num = 0;
			try
			{
				if (Settings.Default.PostPictures)
				{
					if (Settings.Default.DontSwitchCheckBoxes)
					{
						return true;
					}
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click More Options Button +50");
					await (await this.device.WaitForElementBy(0, "More options", 10.0)).TapAlert(0, 230, false);
				}
				else
				{
					int num2 = 0;
					try
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Click More Options Button");
						await (await this.device.WaitForElementBy(0, "More options.", 3.0)).Click(false);
					}
					catch
					{
						num2 = 1;
					}
					if (num2 == 1)
					{
						string text = this.phoneUID;
						WorkUtils.WriteLog("[" + text + "]: More Options Button Not Found: " + await this.device.GetSource());
						text = null;
						int num3 = 0;
						try
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click More Options Button By Coords");
							if (this.device.PhoneModel.Contains("11"))
							{
								await (await this.device.WaitForElementBy(0, "Mention", 10.0)).TapAlert(0, 150, false);
							}
							else
							{
								await (await this.device.WaitForElementBy(0, "Mention", 10.0)).TapAlert(0, 250, false);
							}
						}
						catch
						{
							num3 = 1;
						}
						if (num3 == 1)
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click More Options Button By Coords 2");
							await (await this.device.WaitForElementBy(0, "More options", 10.0)).TapAlert(0, 50, false);
						}
					}
				}
				await Task.Delay(TimeSpan.FromSeconds(2.0));
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Swipe");
				await this.device.Swipe(300, 600, 300, 100, 0.0);
				await Task.Delay(TimeSpan.FromSeconds(1.0));
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Save to device Element");
				Element element = await this.device.WaitForElementBy(3, "Save to device", 3.0);
				if (element != null)
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Value");
					TaskAwaiter<string> taskAwaiter = element.Value().GetAwaiter();
					TaskAwaiter<string> taskAwaiter2;
					if (!taskAwaiter.IsCompleted)
					{
						await taskAwaiter;
						taskAwaiter = taskAwaiter2;
						taskAwaiter2 = default(TaskAwaiter<string>);
					}
					if (taskAwaiter.GetResult() == "on")
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Save To Device Element");
						await element.TapAlert(150, -20, false);
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Changes");
						taskAwaiter = element.Value().GetAwaiter();
						if (!taskAwaiter.IsCompleted)
						{
							await taskAwaiter;
							taskAwaiter = taskAwaiter2;
							taskAwaiter2 = default(TaskAwaiter<string>);
						}
						if (taskAwaiter.GetResult() == "on")
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Searching for CheckBoxes W");
							foreach (Element element2 in await this.device.WaitForElementsBy(1, "on", 10.0))
							{
								TaskAwaiter<bool> taskAwaiter3 = element2.Visible().GetAwaiter();
								if (!taskAwaiter3.IsCompleted)
								{
									await taskAwaiter3;
									TaskAwaiter<bool> taskAwaiter4;
									taskAwaiter3 = taskAwaiter4;
									taskAwaiter4 = default(TaskAwaiter<bool>);
								}
								if (taskAwaiter3.GetResult())
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click CheckBox");
									try
									{
										await element2.Click(false);
									}
									catch
									{
									}
								}
								element2 = null;
							}
							List<Element>.Enumerator enumerator = default(List<Element>.Enumerator);
						}
					}
				}
				else
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Searching for CheckBoxes E");
					foreach (Element element2 in await this.device.WaitForElementsBy(1, "on", 10.0))
					{
						TaskAwaiter<bool> taskAwaiter3 = element2.Visible().GetAwaiter();
						if (!taskAwaiter3.IsCompleted)
						{
							await taskAwaiter3;
							TaskAwaiter<bool> taskAwaiter4;
							taskAwaiter3 = taskAwaiter4;
							taskAwaiter4 = default(TaskAwaiter<bool>);
						}
						if (taskAwaiter3.GetResult())
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click CheckBox");
							try
							{
								await element2.Click(false);
							}
							catch
							{
							}
						}
						element2 = null;
					}
					List<Element>.Enumerator enumerator = default(List<Element>.Enumerator);
				}
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Back Button");
				List<Element> list = await this.device.WaitForElementsBy(0, "Back", 10.0);
				await list[list.Count - 1].Click(false);
				return true;
			}
			catch (Exception obj)
			{
				num = 1;
			}
			bool flag;
			if (num == 1)
			{
				object obj;
				Exception ex = (Exception)obj;
				WorkUtils.WriteLog("[" + this.phoneUID + "]: TurnOffCheckBoxes exception: " + ex.ToString());
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Back Button");
					List<Element> list2 = await this.device.WaitForElementsBy(0, "Back", 10.0);
					await list2[list2.Count - 1].Click(false);
				}
				catch
				{
				}
				flag = false;
			}
			return flag;
		}

		// Token: 0x06000110 RID: 272 RVA: 0x0004A9D8 File Offset: 0x00048BD8
		public async Task PlusMention(string username)
		{
			int num = 0;
			try
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Click @Mention Button");
				await (await this.device.WaitForElementBy(0, "Mention", 10.0)).Click(false);
			}
			catch
			{
				num = 1;
			}
			if (num == 1)
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Second click @Mention Button");
				await (await this.device.WaitForElementBy(0, "Mention friends in your post", 10.0)).Click(false);
			}
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Write Username: " + username);
			num = 0;
			try
			{
				await (await this.device.WaitForElementBy(1, "Search users", 10.0)).SetText(username);
			}
			catch
			{
				num = 1;
			}
			if (num == 1)
			{
				await (await this.device.WaitForElementByXPath("XCUIElementTypeTextField", 2, "", 10.0)).SetText(username);
			}
			await Task.Delay(TimeSpan.FromSeconds(4.0));
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Just tap");
			await this.device.Tap(84.0, 188.0);
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Trying to click username " + username);
			try
			{
				await (await this.device.WaitForElementBy(3, username + ",", 3.0)).Click(false);
			}
			catch
			{
			}
			TaskAwaiter<Element> taskAwaiter = this.device.WaitForElementBy(0, "Edit cover", 3.0).GetAwaiter();
			if (!taskAwaiter.IsCompleted)
			{
				await taskAwaiter;
				TaskAwaiter<Element> taskAwaiter2;
				taskAwaiter = taskAwaiter2;
				taskAwaiter2 = default(TaskAwaiter<Element>);
			}
			if (taskAwaiter.GetResult() == null)
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Click First Username");
				await this.device.Tap(84.0, 230.0);
			}
			await Task.Delay(TimeSpan.FromSeconds(1.0));
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Just tap");
			await this.device.Tap(150.0, 500.0);
		}

		// Token: 0x06000111 RID: 273 RVA: 0x0004AA24 File Offset: 0x00048C24
		public async Task WriteTags()
		{
			List<string> list = new List<string>();
			int num = Settings.Default.TagsCount;
			if (Form1.Settings.TagsList.Count < num)
			{
				num = Form1.Settings.TagsList.Count;
			}
			int num2 = 0;
			int i = 0;
			while (i < num)
			{
				string text;
				do
				{
					text = Form1.Settings.TagsList[ConstParams.rand.Next(Form1.Settings.TagsList.Count)];
					if (!list.Contains(text))
					{
						goto IL_00AF;
					}
					num2++;
				}
				while (num2 < 40);
				IL_00D8:
				i++;
				continue;
				goto IL_00D8;
				IL_00AF:
				list.Add(text.Contains("#") ? text : ("#" + text));
				goto IL_00D8;
			}
			int num3 = 0;
			try
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Write Tags");
				Element element = await this.device.WaitForElementByXPath("XCUIElementTypeTextView", 9, "true", 10.0);
				if (element == null)
				{
					element = await this.device.WaitForElementBy(3, "Add description", 5.0);
				}
				if (element == null)
				{
					element = await this.device.WaitForElementBy(1, "Describe your post", 5.0);
				}
				if (element == null)
				{
					element = await this.device.WaitForElementBy(1, "Describe your video", 5.0);
				}
				if (element == null)
				{
					element = await this.device.WaitForElementBy(0, "Add a hashtag to your post", 5.0);
				}
				if (element == null)
				{
					element = await this.device.WaitForElementBy(1, "Describe your post", 5.0);
				}
				await element.SetText(string.Join(" ", list));
			}
			catch
			{
				num3 = 1;
			}
			if (num3 == 1)
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Write Tags Error, Screening");
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking again");
				await (await this.device.WaitForElementBy(0, "Share your thoughts within 500 characters", 10.0)).SetText(string.Join(" ", list) + " ");
			}
			await Task.Delay(TimeSpan.FromSeconds(1.0));
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Just tap");
			await this.device.Tap(150.0, 500.0);
		}

		// Token: 0x06000112 RID: 274 RVA: 0x0004AA68 File Offset: 0x00048C68
		public async Task<bool> RefreshVideo(string phoneUID, string videoUrl, string originalVideoUrl, bool Remove = true)
		{
			bool flag;
			try
			{
				await FileSystemUtils.RefreshVideo(phoneUID, videoUrl, originalVideoUrl, this.device.PhoneModel.Contains("X"), Remove);
				flag = true;
			}
			catch (Exception ex)
			{
				WorkUtils.WriteLog("[" + phoneUID + "]: Refresh video exception: " + ex.ToString());
				flag = false;
			}
			return flag;
		}

		// Token: 0x04000368 RID: 872
		public iDevice device;

		// Token: 0x04000369 RID: 873
		public string phoneUID = "";

		// Token: 0x0400036A RID: 874
		public string VideoFolder;

		// Token: 0x02000069 RID: 105
		public enum PostingResult
		{
			// Token: 0x0400036C RID: 876
			Success,
			// Token: 0x0400036D RID: 877
			Unsuccess,
			// Token: 0x0400036E RID: 878
			Blocked,
			// Token: 0x0400036F RID: 879
			UnAuth,
			// Token: 0x04000370 RID: 880
			AccountStatus,
			// Token: 0x04000371 RID: 881
			NotCreateError,
			// Token: 0x04000372 RID: 882
			CheckAgain,
			// Token: 0x04000373 RID: 883
			NoHaveVideo
		}
	}
}
