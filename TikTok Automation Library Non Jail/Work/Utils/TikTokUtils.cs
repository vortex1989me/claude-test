using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using WDA_Framework;
using WDA_Framework.Utils;

namespace TikTok_Automation_Library_Non_Jail.Work.Utils
{
	// Token: 0x0200004D RID: 77
	public class TikTokUtils
	{
		// Token: 0x060000CC RID: 204 RVA: 0x0002F114 File Offset: 0x0002D314
		public async Task TerminateTikTok()
		{
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Terminate TikTok");
			await this.device.TerminateApp("com.zhiliaoapp.musically");
		}

		// Token: 0x060000CD RID: 205 RVA: 0x0002F158 File Offset: 0x0002D358
		public async Task ActivateTikTok()
		{
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Activate TikTok");
			await this.device.ActivateApp("com.zhiliaoapp.musically");
		}

		// Token: 0x060000CE RID: 206 RVA: 0x0002F19C File Offset: 0x0002D39C
		public async Task GoHome(bool DoubleHome = false)
		{
			try
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Press Home GHU");
				string text = await this.Method0("home", 10000);
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Press Home Done GHU");
				if (string.IsNullOrEmpty(text))
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Press Home Empty GHU");
					try
					{
						await this.TerminateTikTok();
					}
					catch
					{
					}
				}
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Press Home Next GHU");
				if (DoubleHome)
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Second Press Home");
					TaskAwaiter<string> taskAwaiter = this.Method0("home", 10000).GetAwaiter();
					if (!taskAwaiter.IsCompleted)
					{
						await taskAwaiter;
						TaskAwaiter<string> taskAwaiter2;
						taskAwaiter = taskAwaiter2;
						taskAwaiter2 = default(TaskAwaiter<string>);
					}
					if (string.IsNullOrEmpty(taskAwaiter.GetResult()))
					{
						try
						{
							await this.TerminateTikTok();
						}
						catch
						{
						}
					}
				}
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Press Home OVER GHU");
			}
			catch (Exception ex)
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: GHU Exception: " + ex.ToString());
				throw ex;
			}
		}

		// Token: 0x060000CF RID: 207 RVA: 0x0002F1E8 File Offset: 0x0002D3E8
		public async Task RunTikTokOpenUrl(TikTokUtils.TikTokOpenUrl tikTokOpenUrl)
		{
			switch (tikTokOpenUrl)
			{
			case TikTokUtils.TikTokOpenUrl.Profile:
				await this.device.OpenUrl("snssdk1233://user/homepage");
				break;
			case TikTokUtils.TikTokOpenUrl.Edit:
				await this.device.OpenUrl("snssdk1233://user/profile/edit");
				break;
			case TikTokUtils.TikTokOpenUrl.Setting:
				await this.device.OpenUrl("snssdk1233://setting");
				break;
			case TikTokUtils.TikTokOpenUrl.Botification:
				await this.device.OpenUrl("snssdk1233://notification");
				break;
			}
		}

		// Token: 0x060000D0 RID: 208 RVA: 0x0002F234 File Offset: 0x0002D434
		public async Task RunShortCut(TikTokUtils.ShortcutName shortcutName)
		{
			switch (shortcutName)
			{
			case TikTokUtils.ShortcutName.Reset:
				LogsUtils.WriteLog("[" + this.phoneUID + "]: Activate ShortCut Reset Script");
				await this.device.OpenUrl("shortcuts://run-shortcut?name=Reset");
				break;
			case TikTokUtils.ShortcutName.Date:
				LogsUtils.WriteLog("[" + this.phoneUID + "]: Activate ShortCut Date Script");
				await this.device.OpenUrl("shortcuts://run-shortcut?name=Date");
				break;
			case TikTokUtils.ShortcutName.Region:
				LogsUtils.WriteLog("[" + this.phoneUID + "]: Activate ShortCut Region Script");
				await this.device.OpenUrl("shortcuts://run-shortcut?name=Region");
				break;
			case TikTokUtils.ShortcutName.Wifi:
				LogsUtils.WriteLog("[" + this.phoneUID + "]: Activate ShortCut Wifi Script");
				await this.device.OpenUrl("shortcuts://run-shortcut?name=Wifi");
				break;
			}
		}

		// Token: 0x060000D1 RID: 209 RVA: 0x0002F280 File Offset: 0x0002D480
		public async Task ConfigurateShortcuts()
		{
			await this.device.ConfigurateAppium(25);
			List<string> list = new List<string> { "https://www.icloud.com/shortcuts/3db2c2ea3bb24c93a9d94668b2fbfb4d", "https://www.icloud.com/shortcuts/654b09a6ebb24c1e92eeb2ab5169caa4", "https://www.icloud.com/shortcuts/c2a2bd677340437592b6a6400a65fff5", "https://www.icloud.com/shortcuts/4b7d9c0b336f4d679a95a43da0b7f1dd" };
			foreach (string text in list)
			{
				await this.device.OpenUrl(text);
				await (await this.device.WaitForElementBy(0, "Add Shortcut", 30.0)).Click(false);
				try
				{
					await (await this.device.WaitForElementBy(0, "Replace", 3.0)).Click(false);
					await Task.Delay(TimeSpan.FromSeconds(2.0));
				}
				catch
				{
				}
			}
			List<string>.Enumerator enumerator = default(List<string>.Enumerator);
			Position position = null;
			await this.RunShortCut(TikTokUtils.ShortcutName.Wifi);
			position = await this.ClickPos(position);
			await this.RunShortCut(TikTokUtils.ShortcutName.Reset);
			position = await this.ClickPos(position);
			await this.RunShortCut(TikTokUtils.ShortcutName.Date);
			position = await this.ClickPos(position);
			await this.RunShortCut(TikTokUtils.ShortcutName.Region);
			position = await this.ClickPos(position);
		}

		// Token: 0x060000D2 RID: 210 RVA: 0x0002F2C4 File Offset: 0x0002D4C4
		private async Task<Position> ClickPos(Position pos)
		{
			await Task.Delay(TimeSpan.FromSeconds(3.0));
			if (pos == null)
			{
				await this.GoHome(false);
			}
			try
			{
				if (pos != null)
				{
					await this.device.Tap(pos.x, pos.y);
					return pos;
				}
				Element element = await this.device.WaitForElementBy(0, "Always Allow", 10.0);
				if (element != null)
				{
					pos = await element.GetPosition();
					await this.device.Tap(pos.x, pos.y);
					return pos;
				}
			}
			catch
			{
			}
			return null;
		}

		// Token: 0x060000D3 RID: 211 RVA: 0x0002F310 File Offset: 0x0002D510
		public async Task<bool> ActivateChecks()
		{
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for TikTok Allerts");
			if (this.device.IsElementInXml("Got it", null))
			{
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Got it Button");
					await (await this.device.WaitForElementBy(0, "Got it", 10.0)).Click(false);
					return true;
				}
				catch
				{
				}
			}
			if (this.device.IsElementInXmlContains("Verify to continue", null) || this.device.IsElementInXmlContains("Drag the puzzle", null))
			{
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Close captcha!");
					await (await this.device.WaitForElementBy(0, "Close", 10.0)).Click(false);
					return true;
				}
				catch
				{
				}
			}
			if (this.device.IsElementInXmlContains("Get started", null))
			{
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Get started / Click Close Button");
					await (await this.device.WaitForElementBy(0, "Close", 10.0)).Click(false);
					return true;
				}
				catch
				{
				}
			}
			if (this.device.IsElementInXmlContains("You can allow the Passwords app to manage your passkeys", null) || this.device.IsElementInXml("close", null))
			{
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Close Button");
					await (await this.device.WaitForElementBy(0, "close", 10.0)).Click(false);
					return true;
				}
				catch
				{
				}
			}
			if (this.device.IsElementInXml("Skip", null))
			{
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Skip Button");
					await (await this.device.WaitForElementBy(0, "Skip", 10.0)).Click(false);
					return true;
				}
				catch
				{
				}
			}
			if (this.device.IsElementInXmlContains("allow access to your photos", null) || this.device.IsElementInXml("Allow Full Access", null) || this.device.IsElementInXmlContains("would like full access to your Photo Library", null))
			{
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Allow Button");
					await (await this.device.WaitForElementBy(0, "Allow Full Access", 20.0)).Click(false);
					return true;
				}
				catch
				{
				}
			}
			if (this.device.IsElementInXmlContains("Would Like to Send You Notifications", null) || this.device.IsElementInXmlContains("would like to access the Camera", null) || this.device.IsElementInXmlContains("would like to access the Microphone", null))
			{
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Allow Button");
					await (await this.device.WaitForElementBy(0, "Allow", 10.0)).Click(false);
					return true;
				}
				catch
				{
				}
			}
			if (this.device.IsElementInXmlContains("Start watching", null))
			{
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Start watching Button");
					await (await this.device.WaitForElementBy(3, "Start watching", 10.0)).Click(false);
					return true;
				}
				catch
				{
				}
			}
			if (this.device.IsElementInXmlContains("to track your activity across", null) || this.device.IsElementInXml("Ask App Not to Track", null))
			{
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Ask App Not to Track Button");
					await (await this.device.WaitForElementBy(0, "Ask App Not to Track", 10.0)).Click(false);
					return true;
				}
				catch
				{
				}
			}
			if (this.device.IsElementInXmlNotVisible("Swipe up for more", null))
			{
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Swipe");
					await this.device.Swipe(150, 600, 150, 10, 0.0);
					return true;
				}
				catch
				{
				}
			}
			if (this.device.IsElementInXml("Not now", null))
			{
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Not now Button");
					await (await this.device.WaitForElementBy(0, "Not now", 10.0)).Click(false);
					return true;
				}
				catch
				{
				}
			}
			if (this.device.IsElementInXml("Save", null))
			{
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Save Button");
					await (await this.device.WaitForElementBy(0, "Save", 10.0)).Click(false);
					return true;
				}
				catch
				{
				}
			}
			if (this.device.IsElementInXmlContains("t allow", null) || this.device.IsElementInXmlContains("would like to access your Contacts", null))
			{
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Don't allow Button");
					await (await this.device.WaitForElementBy(3, "t allow", 10.0)).Click(false);
					return true;
				}
				catch
				{
				}
			}
			if (this.device.IsElementInXmlContains("t Allow", null))
			{
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Don't Allow Button");
					await (await this.device.WaitForElementBy(3, "t Allow", 10.0)).Click(false);
					return true;
				}
				catch
				{
				}
			}
			if (this.device.IsElementInXmlContains("Accept", null))
			{
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Accept Button");
					await (await this.device.WaitForElementBy(3, "Accept", 10.0)).Click(false);
					return true;
				}
				catch
				{
				}
			}
			if (this.device.IsElementInXmlContains("Agree and continue", null))
			{
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Check for Agree and continue Allert");
					foreach (Element element in await this.device.WaitForElementsBy(3, "Agree and continue", 3.0))
					{
						try
						{
							TaskAwaiter<bool> taskAwaiter = element.Visible().GetAwaiter();
							if (!taskAwaiter.IsCompleted)
							{
								await taskAwaiter;
								TaskAwaiter<bool> taskAwaiter2;
								taskAwaiter = taskAwaiter2;
								taskAwaiter2 = default(TaskAwaiter<bool>);
							}
							if (taskAwaiter.GetResult())
							{
								await element.Click(false);
							}
						}
						catch
						{
						}
						element = null;
					}
					List<Element>.Enumerator enumerator = default(List<Element>.Enumerator);
					return true;
				}
				catch
				{
				}
			}
			return false;
		}

		// Token: 0x060000D4 RID: 212 RVA: 0x0002F354 File Offset: 0x0002D554
		public async Task<TikTokUtils.ActivateResult> ActivateTikTokGotoProfile()
		{
			bool flag = false;
			int num = 0;
			Element element2;
			string text;
			for (;;)
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: APPIUM DEF || ACTIVATEGOPROFILE");
				await this.device.ConfigurateAppium(17);
				await this.RunTikTokOpenUrl(TikTokUtils.TikTokOpenUrl.Profile);
				await Task.Delay(TimeSpan.FromSeconds(3.0));
				for (;;)
				{
					num++;
					if (num > 15)
					{
						break;
					}
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting source page ATGTP");
					await this.device.GetSourceXml();
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for boxes");
					if (this.device.IsElementInXmlContains("Find contacts", null) || this.device.IsElementInXmlContains("Open settings", null))
					{
						try
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Don't Allow Button");
							foreach (Element element in await this.device.WaitForElementsBy(3, "t Allow", 3.0))
							{
								try
								{
									await element.Click(false);
								}
								catch
								{
								}
							}
							List<Element>.Enumerator enumerator = default(List<Element>.Enumerator);
							continue;
						}
						catch
						{
						}
					}
					if ((this.device.IsElementInXmlContains("Allow", null) || this.device.IsElementInXmlContains("Would Like to Send You Notifications", null)) && !this.device.IsElementInXmlContains("to track your activity across", null) && !this.device.IsElementInXml("Ask App Not to Track", null))
					{
						try
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Allow Button");
							await (await this.device.WaitForElementBy(3, "Allow", 10.0)).Click(false);
							continue;
						}
						catch
						{
						}
					}
					if (this.device.IsElementInXml("Add another account", null) || this.device.IsElementInXmlContains("have an account? Sign up", null) || this.device.IsElementInXml("Log in to TikTok", null) || this.device.IsElementInXml("Continue with Facebook", null) || this.device.IsElementInXmlContains("have an account? Log in", null) || this.device.IsElementInXml("Sign up for TikTok", null) || this.device.IsElementInXmlContains("Continue with email", null) || this.device.IsElementInXmlContains("Create an account", null))
					{
						goto IL_08BA;
					}
					if (this.device.IsElementInXml("Relaunch", null) || this.device.IsElementInXml("Relaunch TikTok", null))
					{
						try
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Relaunch!");
							await (await this.device.WaitForElementBy(0, "Relaunch", 10.0)).Click(false);
							await Task.Delay(TimeSpan.FromSeconds(1.0));
							await this.ActivateTikTok();
						}
						catch
						{
						}
					}
					if (this.device.IsElementInXml("Dismiss", null) || this.device.IsElementInXmlContains("account was banned", null))
					{
						try
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Dismiss Button");
							await (await this.device.WaitForElementBy(0, "Dismiss", 10.0)).Click(false);
							return TikTokUtils.ActivateResult.Blocked;
						}
						catch
						{
						}
					}
					if (this.device.IsElementInXml("OK", null) || this.device.IsElementInXmlContains("Account status", null) || this.device.IsElementInXmlContains("has been locked", null))
					{
						try
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: LOCKED | Account Status | Click OK Button");
							try
							{
								await (await this.device.WaitForElementBy(0, "OK", 10.0)).Click(false);
								await Task.Delay(TimeSpan.FromSeconds(5.0));
								await this.RemoveAccount();
							}
							catch
							{
							}
							return TikTokUtils.ActivateResult.Blocked;
						}
						catch
						{
						}
					}
					if (this.device.IsElementInXml("OK", null) || this.device.IsElementInXmlContains("Account status", null) || this.device.IsElementInXmlContains("Your account may be at risk", null))
					{
						try
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: RISK | Account Status | Click OK Button");
							try
							{
								await (await this.device.WaitForElementBy(0, "OK", 10.0)).Click(false);
								await Task.Delay(TimeSpan.FromSeconds(5.0));
								await this.RemoveAccount();
							}
							catch
							{
							}
							return TikTokUtils.ActivateResult.AccountStatus;
						}
						catch
						{
						}
					}
					TaskAwaiter<bool> taskAwaiter = this.ActivateChecks().GetAwaiter();
					if (!taskAwaiter.IsCompleted)
					{
						await taskAwaiter;
						TaskAwaiter<bool> taskAwaiter2;
						taskAwaiter = taskAwaiter2;
						taskAwaiter2 = default(TaskAwaiter<bool>);
					}
					if (!taskAwaiter.GetResult() && (this.device.IsElementInXml("Profile", null) || this.device.IsElementInXml("a11y_vo_profile", null)))
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Profile button Y");
						element2 = null;
						if (this.device.IsElementInXml("Profile", null))
						{
							element2 = await this.device.WaitForElementBy(0, "Profile", 10.0);
						}
						else
						{
							if (!this.device.IsElementInXml("a11y_vo_profile", null))
							{
								text = this.phoneUID;
								WorkUtils.WriteLog("[" + text + "]: PROFILE BUTTON ERROR: " + await this.device.GetSource());
								text = null;
								continue;
							}
							element2 = await this.device.WaitForElementBy(0, "a11y_vo_profile", 10.0);
						}
						if (element2 == null)
						{
							text = this.phoneUID;
							WorkUtils.WriteLog("[" + text + "]: PROFILE BUTTON IS NULL: " + await this.device.GetSource());
							text = null;
						}
						else
						{
							bool flag2 = await element2.Visible();
							if (flag2)
							{
								flag2 = await element2.Enabled();
							}
							if (flag2)
							{
								goto Block_32;
							}
						}
					}
				}
				if (flag)
				{
					break;
				}
				text = this.phoneUID;
				WorkUtils.WriteLog("[" + text + "]: ATGTP Unknown Source Page: " + await this.device.GetSource());
				text = null;
				await this.SaveScreenShot(true, TikTokUtils.ScreenshotType.Working);
				flag = true;
				num = 0;
				await this.GoHome(false);
				await this.TerminateTikTok();
			}
			text = this.phoneUID;
			WorkUtils.WriteLog("[" + text + "]: UNKNOWN ALLER SOURCE: " + await this.device.GetSource());
			text = null;
			return TikTokUtils.ActivateResult.Unsuccess;
			IL_08BA:
			return TikTokUtils.ActivateResult.UnAuth;
			Block_32:
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Profile Button");
			await element2.Click(false);
			await Task.Delay(TimeSpan.FromSeconds(2.0));
			await this.device.GetSourceXml();
			if (this.device.IsElementInXmlContains("t Allow", null))
			{
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Don't Allow Button");
					await (await this.device.WaitForElementBy(3, "t Allow", 3.0)).Click(false);
				}
				catch
				{
				}
			}
			if (this.device.IsElementInXmlContains("OK", null))
			{
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Check for Ok Allert");
					await (await this.device.WaitForElementBy(0, "OK", 3.0)).Click(false);
				}
				catch
				{
				}
			}
			if (this.device.IsElementInXmlContains("close", null))
			{
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Save Password Alert || Close");
					await (await this.device.WaitForElementBy(0, "close", 3.0)).Click(false);
				}
				catch
				{
				}
			}
			if (this.device.IsElementInXmlContains("Close", null))
			{
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Close Button");
					await (await this.device.WaitForElementBy(0, "Close", 3.0)).Click(false);
				}
				catch
				{
				}
			}
			if (this.device.IsElementInXmlContains("Save", null))
			{
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Save");
					await (await this.device.WaitForElementBy(0, "Save", 3.0)).Click(false);
				}
				catch
				{
				}
			}
			return TikTokUtils.ActivateResult.Success;
		}

		// Token: 0x060000D5 RID: 213 RVA: 0x0002F398 File Offset: 0x0002D598
		public async Task<bool> FirstActivateTikTok(bool IsMake, string PhoneModel)
		{
			int num = 0;
			bool flag = false;
			Exception ex2;
			for (;;)
			{
				IL_0029:
				int num2 = 0;
				try
				{
					if (IsMake)
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Making account start");
					}
					else
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Signing start");
					}
					await this.TerminateTikTok();
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Appium DEF | S1");
					await this.device.ConfigurateAppium(17);
					await this.ActivateTikTok();
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Waiting 2 sec for loading");
					await Task.Delay(TimeSpan.FromSeconds(2.0));
					for (;;)
					{
						int num3 = num++;
						if (num > 30)
						{
							break;
						}
						WorkUtils.WriteLog("[" + this.phoneUID + "]: FA Getting Source");
						await this.device.GetSourceXml();
						if (this.device.IsElementInXmlContains("Verify to continue", null) || this.device.IsElementInXmlContains("Drag the puzzle", null))
						{
							num3 = 0;
							try
							{
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Close captcha!");
								await (await this.device.WaitForElementBy(0, "Close", 10.0)).Click(false);
								await Task.Delay(TimeSpan.FromSeconds(1.0));
								continue;
							}
							catch
							{
								num3 = 1;
							}
							if (num3 == 1)
							{
								goto Block_10;
							}
						}
						if (this.device.IsElementInXmlContains("Continue as", null))
						{
							try
							{
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click More Button");
								await (await this.device.WaitForElementBy(3, "More", 10.0)).Click(false);
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove account Button");
								await (await this.device.WaitForElementBy(3, "Remove account", 10.0)).Click(false);
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button");
								await (await this.device.WaitForElementBy(0, "Remove", 10.0)).Click(false);
								continue;
							}
							catch
							{
							}
						}
						if (this.device.IsElementInXmlContains("account was banned", null) || this.device.IsElementInXml("Dismiss", null))
						{
							try
							{
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Account Was Banned error!");
								await (await this.device.WaitForElementBy(0, "Dismiss", 10.0)).Click(false);
								continue;
							}
							catch
							{
							}
						}
						if (this.device.IsElementInXml("OK", null) || this.device.IsElementInXmlContains("Account status", null) || this.device.IsElementInXmlContains("Your account may be at risk", null))
						{
							try
							{
								WorkUtils.WriteLog("[" + this.phoneUID + "]: RISK | Account Status | Click OK Button");
								try
								{
									await (await this.device.WaitForElementBy(0, "OK", 10.0)).Click(false);
									await Task.Delay(TimeSpan.FromSeconds(5.0));
									await this.RemoveAccount();
								}
								catch
								{
								}
								continue;
							}
							catch
							{
							}
						}
						if (this.device.IsElementInXml("Relaunch", null) || this.device.IsElementInXml("Relaunch TikTok", null))
						{
							try
							{
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Relaunch!");
								await (await this.device.WaitForElementBy(0, "Relaunch", 10.0)).Click(false);
								await Task.Delay(TimeSpan.FromSeconds(1.0));
								goto IL_0029;
							}
							catch
							{
							}
						}
						if ((this.device.IsElementInXmlContains("Allow", null) || this.device.IsElementInXmlContains("Would Like to Send You Notifications", null)) && !this.device.IsElementInXmlContains("to track your activity across", null) && !this.device.IsElementInXml("Ask App Not to Track", null))
						{
							try
							{
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Allow Button");
								await (await this.device.WaitForElementBy(3, "Allow", 10.0)).Click(false);
								continue;
							}
							catch
							{
							}
						}
						TaskAwaiter<bool> taskAwaiter = this.ActivateChecks().GetAwaiter();
						TaskAwaiter<bool> taskAwaiter2;
						if (!taskAwaiter.IsCompleted)
						{
							await taskAwaiter;
							taskAwaiter = taskAwaiter2;
							taskAwaiter2 = default(TaskAwaiter<bool>);
						}
						if (!taskAwaiter.GetResult())
						{
							if (this.device.IsElementInXml("Add another account", null) || this.device.IsElementInXmlContains("have an account? Sign up", null) || this.device.IsElementInXml("Log in to TikTok", null) || this.device.IsElementInXmlContains("have an account? Log in", null) || this.device.IsElementInXmlContains("Create an account", null) || this.device.IsElementInXmlContains("Continue with email", null))
							{
								if (IsMake)
								{
									if (this.device.IsElementInXmlContains("have an account? Sign up", null))
									{
										goto Block_30;
									}
									if (this.device.IsElementInXmlContains("Create an account", null))
									{
										goto Block_31;
									}
									if (this.device.IsElementInXmlContains("have an account? Log in", null) || this.device.IsElementInXml("Add another account", null))
									{
										goto IL_132C;
									}
								}
								else
								{
									if (this.device.IsElementInXmlContains("have an account? Log in", null))
									{
										goto Block_33;
									}
									if (this.device.IsElementInXmlContains("Continue with email", null))
									{
										goto Block_34;
									}
									if (this.device.IsElementInXml("Add another account", null) || this.device.IsElementInXmlContains("have an account? Sign up", null))
									{
										goto IL_156D;
									}
								}
							}
							if (this.device.IsElementInXml("Profile", null))
							{
								try
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Profile Button");
									Element element = await this.device.WaitForElementBy(0, "Profile", 4.0);
									if (element == null)
									{
										continue;
									}
									taskAwaiter = element.Visible().GetAwaiter();
									if (!taskAwaiter.IsCompleted)
									{
										await taskAwaiter;
										taskAwaiter = taskAwaiter2;
										taskAwaiter2 = default(TaskAwaiter<bool>);
									}
									bool flag2 = !taskAwaiter.GetResult();
									if (!flag2)
									{
										taskAwaiter = element.Enabled().GetAwaiter();
										if (!taskAwaiter.IsCompleted)
										{
											await taskAwaiter;
											taskAwaiter = taskAwaiter2;
											taskAwaiter2 = default(TaskAwaiter<bool>);
										}
										flag2 = !taskAwaiter.GetResult();
									}
									if (flag2)
									{
										continue;
									}
									await element.Click(false);
									await Task.Delay(TimeSpan.FromSeconds(1.0));
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking source");
									await this.device.GetSourceXml();
									if (this.device.IsElementInXmlContains("For You", null) || this.device.IsElementInXmlContains("top_tabs_recomend", null) || this.device.IsElementInXmlContains("following", null) || this.device.IsElementInXmlContains("Following", null) || this.device.IsElementInXmlContains("Community", null))
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Goto CA again");
										continue;
									}
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Appium 25 | XZA1");
									await this.device.ConfigurateAppium(25);
									for (;;)
									{
										int num4 = 0;
										try
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Close Button");
											await (await this.device.WaitForElementBy(0, "Close", 3.0)).TapAlert(0, 0, false);
										}
										catch
										{
											num4 = 1;
										}
										num3 = num4;
										if (num3 == 1)
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Close Button by Coords");
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Phone Model: " + PhoneModel);
											if (PhoneModel.Contains("11"))
											{
												await this.device.Tap(385.0, 84.0);
											}
											else
											{
												await this.device.Tap(361.0, 83.0);
											}
										}
										try
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Login Button Q");
											await (await this.device.WaitForElementBy(0, "Login", 3.0)).Click(false);
										}
										catch
										{
										}
										try
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Alerts Q");
											await (await this.device.WaitForElementBy(0, "Cancel", 3.0)).Click(false);
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Go To NC");
											continue;
										}
										catch
										{
										}
										break;
									}
									for (;;)
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking source");
										await this.device.GetSourceXml();
										if (this.device.IsElementInXmlContains("Continue as", null))
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Remove Old Account");
											try
											{
												WorkUtils.WriteLog("[" + this.phoneUID + "]: Click More Button");
												await (await this.device.WaitForElementBy(3, "More", 10.0)).Click(false);
												WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove account Button");
												await (await this.device.WaitForElementBy(3, "Remove account", 10.0)).Click(false);
												WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button");
												await (await this.device.WaitForElementBy(0, "Remove", 10.0)).Click(false);
												await Task.Delay(TimeSpan.FromSeconds(1.0));
												continue;
											}
											catch
											{
											}
										}
										if (this.device.IsElementInXmlContains("Remove account", null))
										{
											try
											{
												WorkUtils.WriteLog("[" + this.phoneUID + "]: Check for Remove Account Button");
												await (await this.device.WaitForElementBy(0, "Remove account", 3.0)).Click(false);
												WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button");
												await (await this.device.WaitForElementBy(0, "Remove", 10.0)).Click(false);
												await Task.Delay(TimeSpan.FromSeconds(1.0));
												continue;
											}
											catch
											{
											}
										}
										if (this.device.IsElementInXmlContains("Manage accounts", null))
										{
											try
											{
												WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Manage accounts Button");
												await (await this.device.WaitForElementBy(0, "Manage accounts", 3.0)).Click(false);
												await Task.Delay(TimeSpan.FromSeconds(2.0));
												WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Account Button");
												num3 = 0;
												try
												{
													this.device.WaitForElementBy(0, "Remove account", 10.0).ConfigureAwait(false).GetAwaiter()
														.GetResult()
														.Click(false)
														.ConfigureAwait(false)
														.GetAwaiter()
														.GetResult();
												}
												catch
												{
													num3 = 1;
												}
												if (num3 == 1)
												{
													WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Account Button By COORDS");
													if (this.device.PhoneModel.Contains("12"))
													{
														await this.device.Tap(350.0, 252.0);
													}
													else
													{
														await this.device.Tap(365.0, 257.0);
													}
												}
												WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button");
												await (await this.device.WaitForElementBy(0, "Remove", 10.0)).Click(false);
												await Task.Delay(TimeSpan.FromSeconds(1.0));
												continue;
											}
											catch
											{
											}
											break;
										}
										break;
									}
									try
									{
										if (IsMake)
										{
											if (this.device.IsElementInXmlContains("have an account? Sign up", null))
											{
												WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Sign Up Button Q");
												await (await this.device.WaitForElementBy(3, "have an account? Sign up", 10.0)).Click(false);
											}
											else if (this.device.IsElementInXmlContains("Create an account", null))
											{
												WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Create an account Button Q");
												await (await this.device.WaitForElementBy(3, "Create an account", 10.0)).Click(false);
											}
										}
										else if (this.device.IsElementInXmlContains("have an account? Log in", null))
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Log In Button Q");
											await (await this.device.WaitForElementBy(3, "have an account? Log in", 3.0)).Click(false);
										}
										else if (this.device.IsElementInXmlContains("Continue with email", null))
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Continue With Email Button Q");
											await (await this.device.WaitForElementBy(3, "Continue with email", 10.0)).Click(false);
										}
									}
									catch
									{
									}
									return true;
								}
								catch (Exception ex)
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: RT With Exception: " + ex.ToString());
									continue;
								}
							}
							await Task.Delay(TimeSpan.FromSeconds(1.0));
						}
					}
					if (!flag)
					{
						num = 0;
						flag = true;
						await this.GoHome(false);
						continue;
					}
					return false;
					Block_10:
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Captcha Error!");
					await this.TerminateTikTok();
					return false;
					Block_30:
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Sign Up Button W");
					await (await this.device.WaitForElementBy(3, "have an account? Sign up", 10.0)).Click(false);
					return true;
					Block_31:
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Create an account Button W");
					await (await this.device.WaitForElementBy(3, "Create an account", 10.0)).Click(false);
					return true;
					IL_132C:
					return true;
					Block_33:
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Log In Button E");
					await (await this.device.WaitForElementBy(3, "have an account? Log in", 3.0)).Click(false);
					return true;
					Block_34:
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Continue With Email Button E");
					await (await this.device.WaitForElementBy(3, "Continue with email", 10.0)).Click(false);
					return true;
					IL_156D:
					return true;
				}
				catch (Exception obj)
				{
					num2 = 1;
				}
				if (num2 != 1)
				{
					bool flag3;
					return flag3;
				}
				object obj;
				ex2 = (Exception)obj;
				if (flag)
				{
					break;
				}
				flag = true;
				await this.GoHome(false);
			}
			throw ex2;
		}

		// Token: 0x060000D6 RID: 214 RVA: 0x0002F3EC File Offset: 0x0002D5EC
		public async Task LogOutFromAccount(string username = null, bool relogin = false, bool skipActivate = false)
		{
			bool flag = false;
			object obj2;
			for (;;)
			{
				int num = 0;
				string text;
				try
				{
					TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter2;
					if (!skipActivate)
					{
						TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter = this.ActivateTikTokGotoProfile().GetAwaiter();
						if (!taskAwaiter.IsCompleted)
						{
							await taskAwaiter;
							taskAwaiter = taskAwaiter2;
							taskAwaiter2 = default(TaskAwaiter<TikTokUtils.ActivateResult>);
						}
						if (taskAwaiter.GetResult() != TikTokUtils.ActivateResult.Success)
						{
							return;
						}
					}
					try
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking Save Draft");
						await (await this.device.WaitForElementBy(0, "Save draft", 3.0)).Click(false);
						await Task.Delay(TimeSpan.FromSeconds(2.0));
					}
					catch
					{
					}
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Menu Button");
					int num2 = 0;
					try
					{
						await (await this.device.WaitForElementBy(0, "nav_bar_end_settings", 10.0)).Click(false);
					}
					catch
					{
						num2 = 1;
					}
					if (num2 == 1)
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Appium 25 | 3Q");
						await this.device.ConfigurateAppium(25);
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Remove Account Button");
						Element element = await this.device.WaitForElementBy(0, "Remove account", 3.0);
						if (element != null)
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Account Button");
							await element.Click(false);
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button");
							await (await this.device.WaitForElementBy(0, "Remove", 10.0)).Click(false);
							await Task.Delay(TimeSpan.FromSeconds(4.0));
							await this.TerminateTikTok();
							await this.GoHome(true);
							return;
						}
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Manage accounts Button");
						element = await this.device.WaitForElementBy(0, "Manage accounts", 3.0);
						if (element != null)
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Manage accounts Button");
							await element.Click(false);
							await Task.Delay(TimeSpan.FromSeconds(2.0));
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Account Button By COORDS");
							if (this.device.PhoneModel.Contains("12"))
							{
								await this.device.Tap(350.0, 252.0);
							}
							else
							{
								await this.device.Tap(365.0, 257.0);
							}
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button");
							await (await this.device.WaitForElementBy(0, "Remove", 10.0)).Click(false);
							await Task.Delay(TimeSpan.FromSeconds(4.0));
							await this.TerminateTikTok();
							await this.GoHome(true);
							return;
						}
					}
					await Task.Delay(TimeSpan.FromSeconds(1.0));
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Appium 25 | 3QW");
					await this.device.ConfigurateAppium(25);
					num2 = 0;
					try
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Settings and privacy Button");
						await (await this.device.WaitForElementBy(0, "Settings and privacy", 4.0)).Click(false);
					}
					catch
					{
						num2 = 1;
					}
					if (num2 == 1)
					{
						int num3 = 0;
						try
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Settings Button");
							await (await this.device.WaitForElementBy(0, "TTKProfileMenuSearchSettingsEntranceComponent", 4.0)).Click(false);
						}
						catch
						{
							num3 = 1;
						}
						if (num3 == 1)
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Settings and privacy By COORDS");
							if (this.PhoneModel.Contains("11"))
							{
								await this.device.Tap(30.0, 800.0);
							}
							else
							{
								await this.device.Tap(78.0, 761.0);
							}
						}
					}
					await Task.Delay(TimeSpan.FromSeconds(1.0));
					try
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Save allert");
						await (await this.device.WaitForElementBy(0, "close", 3.0)).Click(false);
					}
					catch
					{
					}
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Swipe");
					await this.device.Swipe(150, 800, 150, 100, 0.0);
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Log Out Button");
					await (await this.device.WaitForElementBy(0, "Log out", 10.0)).Click(false);
					await Task.Delay(TimeSpan.FromSeconds(1.0));
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Second Log Out Button");
					await (await this.device.WaitForElementByXPath("XCUIElementTypeStaticText", 0, "Log out", 10.0)).Click(false);
					await Task.Delay(TimeSpan.FromSeconds(1.0));
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Appium DEF | 2");
					await this.device.ConfigurateAppium(17);
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Log Out Allert");
					await (await this.device.WaitForElementByXPath("XCUIElementTypeStaticText", 1, "Log out", 10.0)).Click(false);
					num2 = 0;
					try
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Appium 17 | 3Z");
						await this.device.ConfigurateAppium(17);
						try
						{
							await (await this.device.WaitForElementBy(0, "Accept", 3.0)).Click(false);
						}
						catch
						{
						}
						if (relogin)
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Profile Button");
							await (await this.device.WaitForElementBy(0, "Profile", 10.0)).Click(false);
							try
							{
								await (await this.device.WaitForElementBy(3, "Login", 10.0)).Click(false);
							}
							catch
							{
							}
							try
							{
								await (await this.device.WaitForElementBy(0, "Close", 10.0)).Click(false);
								await (await this.device.WaitForElementBy(3, "Login", 10.0)).Click(false);
								await (await this.device.WaitForElementBy(0, username, 10.0)).Click(false);
							}
							catch
							{
							}
							return;
						}
						await this.RemoveAccount();
					}
					catch (Exception obj)
					{
						num2 = 1;
					}
					object obj;
					if (num2 == 1)
					{
						Exception ex = (Exception)obj;
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Got Exception: " + ex.ToString());
						text = this.phoneUID;
						WorkUtils.WriteLog("[" + text + "]: Source: " + await this.device.GetSource());
						text = null;
						await this.GoHome(true);
						await this.TerminateTikTok();
						TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter = this.ActivateTikTokGotoProfile().GetAwaiter();
						if (!taskAwaiter.IsCompleted)
						{
							await taskAwaiter;
							taskAwaiter = taskAwaiter2;
							taskAwaiter2 = default(TaskAwaiter<TikTokUtils.ActivateResult>);
						}
						if (taskAwaiter.GetResult() != TikTokUtils.ActivateResult.Success)
						{
							return;
						}
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Appium 17 | 3Q");
						await this.device.ConfigurateAppium(17);
						try
						{
							await (await this.device.WaitForElementBy(3, "Login", 10.0)).Click(false);
						}
						catch
						{
						}
						if (relogin)
						{
							try
							{
								await (await this.device.WaitForElementBy(0, "Close", 10.0)).Click(false);
								await (await this.device.WaitForElementBy(3, "Login", 10.0)).Click(false);
								await (await this.device.WaitForElementBy(0, username, 10.0)).Click(false);
							}
							catch
							{
							}
							return;
						}
					}
					obj = null;
				}
				catch (Exception obj2)
				{
					num = 1;
				}
				if (num != 1)
				{
					break;
				}
				Exception ex2 = (Exception)obj2;
				WorkUtils.WriteLog("[" + this.phoneUID + "]: LogOut F Excepition: " + ex2.ToString());
				text = this.phoneUID;
				WorkUtils.WriteLog("[" + text + "]: Source: " + await this.device.GetSource());
				text = null;
				if (flag)
				{
					break;
				}
				flag = true;
				await this.GoHome(true);
				await this.TerminateTikTok();
				skipActivate = false;
			}
			obj2 = null;
		}

		// Token: 0x060000D7 RID: 215 RVA: 0x0002F448 File Offset: 0x0002D648
		public async Task RemoveAccount()
		{
			int num = 0;
			TaskAwaiter<Element> taskAwaiter2;
			try
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Appium 17 | 3ZRMW");
				await this.device.ConfigurateAppium(17);
				bool flag = false;
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Remove Account Button");
				Element element = await this.device.WaitForElementBy(0, "Remove account", 3.0);
				if (element != null)
				{
					int num2 = 0;
					try
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Account Button");
						await element.Click(false);
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button");
						await (await this.device.WaitForElementBy(0, "Remove", 10.0)).Click(false);
						flag = true;
					}
					catch
					{
						num2 = 1;
					}
					if (num2 != 1)
					{
						goto IL_1D9B;
					}
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Account Button BY COORDS QW1");
					await this.device.Tap(340.0, 232.0);
					await Task.Delay(TimeSpan.FromSeconds(1.0));
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button BY COORDS");
					await this.device.Tap(210.0, 500.0);
					try
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Cancel Button");
						await (await this.device.WaitForElementBy(0, "Cancel", 3.0)).Click(false);
						goto IL_1D9B;
					}
					catch
					{
						goto IL_1D9B;
					}
				}
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Searching for Continue as");
				TaskAwaiter<Element> taskAwaiter = this.device.WaitForElementBy(3, "Continue as", 10.0).GetAwaiter();
				if (!taskAwaiter.IsCompleted)
				{
					await taskAwaiter;
					taskAwaiter = taskAwaiter2;
					taskAwaiter2 = default(TaskAwaiter<Element>);
				}
				if (taskAwaiter.GetResult() != null)
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click More Button");
					await (await this.device.WaitForElementBy(3, "More", 10.0)).Click(false);
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove account Button");
					await (await this.device.WaitForElementBy(3, "Remove account", 10.0)).Click(false);
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button");
					await (await this.device.WaitForElementBy(0, "Remove", 10.0)).Click(false);
					flag = true;
				}
				else
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Manage accounts Button");
					element = await this.device.WaitForElementBy(0, "Manage accounts", 3.0);
					if (element != null)
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Manage accounts Button");
						await element.Click(false);
						await Task.Delay(TimeSpan.FromSeconds(2.0));
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Account Button By COORDS");
						if (this.device.PhoneModel.Contains("12"))
						{
							await this.device.Tap(350.0, 252.0);
						}
						else
						{
							await this.device.Tap(365.0, 257.0);
						}
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button");
						await (await this.device.WaitForElementBy(0, "Remove", 10.0)).Click(false);
						flag = true;
					}
					else
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Profile Button Z");
						Element element2 = await this.device.WaitForElementBy(0, "a11y_vo_profile", 3.0);
						if (element2 != null)
						{
							bool flag2 = await element2.Visible();
							if (flag2)
							{
								flag2 = await element2.Enabled();
							}
							if (flag2)
							{
								await element2.Click(false);
								await this.device.ConfigurateAppium(18);
								element = await this.device.WaitForElementBy(0, "Manage accounts", 3.0);
								if (element == null)
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Login Button");
									await (await this.device.WaitForElementBy(0, "Login", 10.0)).Click(false);
									element = await this.device.WaitForElementBy(0, "Manage accounts", 3.0);
								}
								if (element != null)
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Manage accounts Button");
									await element.Click(false);
									await Task.Delay(TimeSpan.FromSeconds(2.0));
									int num2 = 0;
									try
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove account Button");
										await (await this.device.WaitForElementBy(0, "Remove account", 3.0)).Click(false);
									}
									catch
									{
										num2 = 1;
									}
									if (num2 == 1)
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Account Button By COORDS");
										if (this.device.PhoneModel.Contains("12"))
										{
											await this.device.Tap(350.0, 252.0);
										}
										else
										{
											await this.device.Tap(365.0, 257.0);
										}
									}
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button");
									await (await this.device.WaitForElementBy(0, "Remove", 10.0)).Click(false);
									flag = true;
								}
								else
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Account Button BY COORDS QWEW");
									await this.device.Tap(340.0, 232.0);
									await Task.Delay(TimeSpan.FromSeconds(1.0));
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button BY COORDS");
									await this.device.Tap(210.0, 500.0);
									try
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Cancel Button");
										await (await this.device.WaitForElementBy(0, "Cancel", 3.0)).Click(false);
									}
									catch
									{
									}
								}
								await this.device.ConfigurateAppium(17);
							}
						}
						else
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Account Button BY COORDS QWW");
							await this.device.Tap(340.0, 232.0);
							await Task.Delay(TimeSpan.FromSeconds(1.0));
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button BY COORDS");
							await this.device.Tap(210.0, 500.0);
							try
							{
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Cancel Button");
								await (await this.device.WaitForElementBy(0, "Cancel", 3.0)).Click(false);
							}
							catch
							{
							}
						}
						element2 = null;
					}
				}
				IL_1D9B:
				await Task.Delay(TimeSpan.FromSeconds(2.0));
				await this.GoHome(true);
				await this.TerminateTikTok();
				if (!flag)
				{
					await this.ActivateTikTok();
					await Task.Delay(TimeSpan.FromSeconds(5.0));
					DateTime dateTime = DateTime.Now.AddSeconds(10.0);
					while (dateTime > DateTime.Now)
					{
						await this.device.GetSourceXml();
						if (this.device.IsElementInXml("Remove account", null))
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Remove Account Button");
							await (await this.device.WaitForElementBy(0, "Remove account", 3.0)).Click(false);
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button");
							await (await this.device.WaitForElementBy(0, "Remove", 10.0)).Click(false);
						}
						else if (this.device.IsElementInXml("Manage accounts", null))
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Manage accounts Button");
							await (await this.device.WaitForElementBy(0, "Manage accounts", 3.0)).Click(false);
							await Task.Delay(TimeSpan.FromSeconds(2.0));
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Account Button By COORDS");
							if (this.device.PhoneModel.Contains("12"))
							{
								await this.device.Tap(350.0, 252.0);
							}
							else
							{
								await this.device.Tap(365.0, 257.0);
							}
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button");
							await (await this.device.WaitForElementBy(0, "Remove", 10.0)).Click(false);
						}
						else
						{
							if (this.device.IsElementInXml("Add another account", null))
							{
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Account Button BY COORDS QWW2");
								await this.device.Tap(340.0, 232.0);
								await Task.Delay(TimeSpan.FromSeconds(1.0));
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button BY COORDS");
								await this.device.Tap(210.0, 500.0);
								try
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Cancel Button");
									await (await this.device.WaitForElementBy(0, "Cancel", 3.0)).Click(false);
									continue;
								}
								catch
								{
									continue;
								}
							}
							if (this.device.IsElementInXml("Continue with Facebook", null) || this.device.IsElementInXmlContains("have an account", null))
							{
								return;
							}
							if (this.device.IsElementInXml("Profile", null))
							{
								try
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Profile Button");
									await (await this.device.WaitForElementBy(0, "Profile", 10.0)).Click(false);
									await this.device.ConfigurateAppium(18);
									element = await this.device.WaitForElementBy(0, "Manage accounts", 3.0);
									if (element == null)
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Login Button");
										await (await this.device.WaitForElementBy(0, "Login", 10.0)).Click(false);
										element = await this.device.WaitForElementBy(0, "Manage accounts", 3.0);
									}
									if (element != null)
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Manage accounts Button");
										await element.Click(false);
										await Task.Delay(TimeSpan.FromSeconds(2.0));
										int num2 = 0;
										try
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove account Button");
											await (await this.device.WaitForElementBy(0, "Remove account", 3.0)).Click(false);
										}
										catch
										{
											num2 = 1;
										}
										if (num2 == 1)
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Account Button By COORDS");
											if (this.device.PhoneModel.Contains("12"))
											{
												await this.device.Tap(350.0, 252.0);
											}
											else
											{
												await this.device.Tap(365.0, 257.0);
											}
										}
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button");
										await (await this.device.WaitForElementBy(0, "Remove", 10.0)).Click(false);
										flag = true;
									}
									await this.device.ConfigurateAppium(17);
								}
								catch
								{
								}
							}
						}
					}
				}
			}
			catch (Exception obj)
			{
				num = 1;
			}
			object obj;
			if (num == 1)
			{
				Exception ex = (Exception)obj;
				int num2 = 0;
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Got Exception: " + ex.ToString());
					string text = this.phoneUID;
					WorkUtils.WriteLog("[" + text + "]: Source: " + await this.device.GetSource());
					text = null;
					await this.GoHome(true);
					await this.TerminateTikTok();
					TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter3 = this.ActivateTikTokGotoProfile().GetAwaiter();
					if (!taskAwaiter3.IsCompleted)
					{
						await taskAwaiter3;
						TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter4;
						taskAwaiter3 = taskAwaiter4;
						taskAwaiter4 = default(TaskAwaiter<TikTokUtils.ActivateResult>);
					}
					if (taskAwaiter3.GetResult() != TikTokUtils.ActivateResult.Success)
					{
						return;
					}
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Appium 17 | 3Q");
					await this.device.ConfigurateAppium(17);
					try
					{
						await (await this.device.WaitForElementBy(3, "Login", 10.0)).Click(false);
					}
					catch
					{
					}
					bool flag = false;
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Searching for Continue as");
					TaskAwaiter<Element> taskAwaiter = this.device.WaitForElementBy(3, "Continue as", 10.0).GetAwaiter();
					if (!taskAwaiter.IsCompleted)
					{
						await taskAwaiter;
						taskAwaiter = taskAwaiter2;
						taskAwaiter2 = default(TaskAwaiter<Element>);
					}
					if (taskAwaiter.GetResult() != null)
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Click More Button");
						await (await this.device.WaitForElementBy(3, "More", 10.0)).Click(false);
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove account Button");
						await (await this.device.WaitForElementBy(3, "Remove account", 10.0)).Click(false);
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button");
						await (await this.device.WaitForElementBy(0, "Remove", 10.0)).Click(false);
						flag = true;
					}
					else
					{
						for (;;)
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Remove Account Button");
							Element element3 = await this.device.WaitForElementBy(0, "Remove account", 3.0);
							if (element3 != null)
							{
								int num3 = 0;
								try
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Account Button");
									await element3.Click(false);
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button");
									await (await this.device.WaitForElementBy(0, "Remove", 10.0)).Click(false);
									flag = true;
								}
								catch
								{
									num3 = 1;
								}
								if (num3 != 1)
								{
									break;
								}
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Account Button BY COORDS Q");
								await this.device.Tap(340.0, 232.0);
								await Task.Delay(TimeSpan.FromSeconds(1.0));
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button BY COORDS");
								await this.device.Tap(210.0, 500.0);
								try
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Cancel Button");
									await (await this.device.WaitForElementBy(0, "Cancel", 3.0)).Click(false);
									break;
								}
								catch
								{
									break;
								}
							}
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Manage accounts Button");
							element3 = await this.device.WaitForElementBy(0, "Manage accounts", 3.0);
							if (element3 != null)
							{
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Manage accounts Button");
								await element3.Click(false);
								await Task.Delay(TimeSpan.FromSeconds(2.0));
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Account Button By COORDS");
								if (this.device.PhoneModel.Contains("12"))
								{
									await this.device.Tap(350.0, 252.0);
								}
								else
								{
									await this.device.Tap(365.0, 257.0);
								}
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button");
								await (await this.device.WaitForElementBy(0, "Remove", 10.0)).Click(false);
								flag = true;
							}
							else
							{
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Account Button BY COORDS QQ");
								await this.device.Tap(340.0, 232.0);
								await Task.Delay(TimeSpan.FromSeconds(1.0));
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button BY COORDS");
								await this.device.Tap(210.0, 500.0);
								try
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Cancel Button");
									await (await this.device.WaitForElementBy(0, "Cancel", 3.0)).Click(false);
								}
								catch
								{
								}
							}
							if (!flag)
							{
								flag = true;
								try
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Profile Button");
									await (await this.device.WaitForElementBy(0, "Profile", 3.0)).Click(false);
									continue;
								}
								catch
								{
								}
								break;
							}
							break;
						}
					}
					await Task.Delay(TimeSpan.FromSeconds(2.0));
					await this.GoHome(true);
					await this.TerminateTikTok();
				}
				catch (Exception obj2)
				{
					num2 = 1;
				}
				object obj2;
				if (num2 == 1)
				{
					Exception ex2 = (Exception)obj2;
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Second Log Out Exception: " + ex2.ToString());
					await this.GoHome(true);
					await this.TerminateTikTok();
				}
				obj2 = null;
			}
			obj = null;
		}

		// Token: 0x060000D8 RID: 216 RVA: 0x0002F48C File Offset: 0x0002D68C
		public async Task SaveScreenShot(bool IsException, TikTokUtils.ScreenshotType type = TikTokUtils.ScreenshotType.Working)
		{
			string text = "ScreenShots";
			if (IsException)
			{
				text = "ScreenShotsExceptions";
			}
			if (type == TikTokUtils.ScreenshotType.Makign)
			{
				text = "ScreenShots_Making_Exceptions";
			}
			else if (type == TikTokUtils.ScreenshotType.Signing)
			{
				text = "ScreenShots_Signing_Exceptions";
			}
			else if (type == TikTokUtils.ScreenshotType.Working)
			{
				text = "ScreenShots_Working_Exceptions";
			}
			else if (type == TikTokUtils.ScreenshotType.Posting)
			{
				text = "ScreenShots_Posting_Exceptions";
			}
			try
			{
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
			}
			catch
			{
			}
			try
			{
				string text2 = string.Format("{0}/{1}_{2}", text, this.screenID, this.phoneUID);
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Do ScreenShot: " + text2);
				await this.DoScreenShot(text2);
				Interlocked.Increment(ref this.screenID);
			}
			catch
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Save screenshot Exception, Why?");
			}
		}

		// Token: 0x060000D9 RID: 217 RVA: 0x0002F4E0 File Offset: 0x0002D6E0
		private async Task DoScreenShot(string SaveLocate)
		{
			if (!string.IsNullOrEmpty(SaveLocate))
			{
				if (!SaveLocate.Contains(".jpg") && !SaveLocate.Contains(".png"))
				{
					SaveLocate += ".png";
				}
				try
				{
					if (File.Exists(SaveLocate))
					{
						File.Delete(SaveLocate);
					}
				}
				catch
				{
				}
				string text = await this.device.GetScreenshot();
				if (!string.IsNullOrEmpty(text))
				{
					File.WriteAllBytes(SaveLocate, Convert.FromBase64String(text));
				}
			}
		}

		// Token: 0x060000DB RID: 219 RVA: 0x0002F52C File Offset: 0x0002D72C
		[CompilerGenerated]
		private async Task<string> Method0(string command, int timeoutMilliseconds)
		{
			Task<string> task = this.device.Press(command);
			Task task2 = Task.Delay(timeoutMilliseconds);
			TaskAwaiter<Task> taskAwaiter = Task.WhenAny(new Task[] { task, task2 }).GetAwaiter();
			if (!taskAwaiter.IsCompleted)
			{
				await taskAwaiter;
				TaskAwaiter<Task> taskAwaiter2;
				taskAwaiter = taskAwaiter2;
				taskAwaiter2 = default(TaskAwaiter<Task>);
			}
			if (taskAwaiter.GetResult() == task2)
			{
				throw new TimeoutException(string.Format("Команда device.Press(\"{0}\") зависла более чем на {1} секунд.", command, timeoutMilliseconds / 1000));
			}
			return await task;
		}

		// Token: 0x0400028A RID: 650
		public iDevice device;

		// Token: 0x0400028B RID: 651
		public string phoneUID = "";

		// Token: 0x0400028C RID: 652
		public string PhoneModel = "";

		// Token: 0x0400028D RID: 653
		public int screenID;

		// Token: 0x0200004E RID: 78
		public enum TikTokOpenUrl
		{
			// Token: 0x0400028F RID: 655
			Profile,
			// Token: 0x04000290 RID: 656
			Edit,
			// Token: 0x04000291 RID: 657
			Setting,
			// Token: 0x04000292 RID: 658
			Botification
		}

		// Token: 0x0200004F RID: 79
		public enum ShortcutName
		{
			// Token: 0x04000294 RID: 660
			Reset,
			// Token: 0x04000295 RID: 661
			Date,
			// Token: 0x04000296 RID: 662
			Region,
			// Token: 0x04000297 RID: 663
			Wifi
		}

		// Token: 0x02000050 RID: 80
		public enum ActivateResult
		{
			// Token: 0x04000299 RID: 665
			Success,
			// Token: 0x0400029A RID: 666
			Unsuccess,
			// Token: 0x0400029B RID: 667
			UnAuth,
			// Token: 0x0400029C RID: 668
			Blocked,
			// Token: 0x0400029D RID: 669
			AccountStatus
		}

		// Token: 0x02000051 RID: 81
		public enum ScreenshotType
		{
			// Token: 0x0400029F RID: 671
			Makign,
			// Token: 0x040002A0 RID: 672
			Signing,
			// Token: 0x040002A1 RID: 673
			Posting,
			// Token: 0x040002A2 RID: 674
			Working
		}
	}
}
