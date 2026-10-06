using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TikTok_Automation_Library_Non_Jail.Properties;
using TikTok_Automation_Library_Non_Jail.Utils;
using TikTok_Automation_Library_Non_Jail.Utils.TempMail;
using TikTok_Automation_Library_Non_Jail.Work.Utils;
using WDA_Framework;
using WDA_Framework.Utils;

namespace TikTok_Automation_Library_Non_Jail.Work.Making
{
	// Token: 0x02000077 RID: 119
	public class Making
	{
		// Token: 0x0600012E RID: 302 RVA: 0x0005515C File Offset: 0x0005335C
		public async Task<bool> SetUpEmail(AnyMessageClient anyMessageClient, bool ChangeDomain)
		{
			try
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Email Button");
				await (await this.device.WaitForElementBy(0, "Email", 3.0)).Click(false);
			}
			catch
			{
			}
			bool flag = false;
			Element element;
			for (;;)
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Searching for Email Address Input Box");
				element = await this.device.WaitForElementBy(1, "Email address", 10.0);
				if (element != null)
				{
					goto IL_036C;
				}
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Not found");
				try
				{
					await this.ClickNext();
				}
				catch
				{
				}
				if (flag)
				{
					break;
				}
				flag = true;
				await this.device.GetSourceXml();
				if (!this.device.IsElementInXmlContains("verification code via email", null))
				{
					goto IL_0386;
				}
				await this.device.ConfigurateAppium(18);
			}
			return false;
			IL_036C:
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Found");
			IL_0386:
			int num = 0;
			AnyMessageClient.EmailData emailDataC;
			for (;;)
			{
				emailDataC = anyMessageClient.emailDataC;
				if (string.IsNullOrEmpty(emailDataC.email))
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Generate New Email");
					string text = Settings.Default.emailDomain;
					if (ChangeDomain)
					{
						if (text.Contains("long_hotmail"))
						{
							text = "long_outlook.com";
						}
						else if (text.Contains("long_outlook"))
						{
							text = "long_hotmail.com";
						}
					}
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Generate New Email With Domain: " + text);
					try
					{
						await anyMessageClient.OrderEmailAsync(text, "tiktok.com", null, null);
						goto IL_0513;
					}
					catch (Exception ex)
					{
						if (num > 3)
						{
							throw ex;
						}
						if (ex.ToString().Contains("no emails"))
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: No emails error");
							ChangeDomain = true;
							num++;
							continue;
						}
						throw ex;
					}
					goto IL_04F9;
				}
				goto IL_04F9;
				IL_0513:
				if (string.IsNullOrEmpty(emailDataC.email))
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Email is empty: " + emailDataC.Exception);
					await Task.Delay(TimeSpan.FromSeconds(30.0));
					continue;
				}
				break;
				IL_04F9:
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Email");
				goto IL_0513;
			}
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Write Email: " + emailDataC.email);
			await element.SetText(emailDataC.email);
			try
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Check for Save login info");
				foreach (Element element2 in await this.device.WaitForElementsBy(0, "Save login info", 3.0))
				{
					try
					{
						TaskAwaiter<bool> taskAwaiter = element2.Visible().GetAwaiter();
						if (!taskAwaiter.IsCompleted)
						{
							await taskAwaiter;
							TaskAwaiter<bool> taskAwaiter2;
							taskAwaiter = taskAwaiter2;
							taskAwaiter2 = default(TaskAwaiter<bool>);
						}
						if (taskAwaiter.GetResult())
						{
							TaskAwaiter<string> taskAwaiter3 = element2.Value().GetAwaiter();
							if (!taskAwaiter3.IsCompleted)
							{
								await taskAwaiter3;
								TaskAwaiter<string> taskAwaiter4;
								taskAwaiter3 = taskAwaiter4;
								taskAwaiter4 = default(TaskAwaiter<string>);
							}
							if (taskAwaiter3.GetResult().Contains("Checkbox checked"))
							{
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Save login info Button");
								await element2.Click(false);
							}
						}
					}
					catch
					{
					}
					element2 = null;
				}
				List<Element>.Enumerator enumerator = default(List<Element>.Enumerator);
			}
			catch (Exception ex2)
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: SLI 1 Exception: " + ex2.ToString());
			}
			await this.ClickNext();
			await Task.Delay(TimeSpan.FromSeconds(1.0));
			return true;
		}

		// Token: 0x0600012F RID: 303 RVA: 0x000551B0 File Offset: 0x000533B0
		private async Task ClickNext()
		{
			await this.device.GetSourceXml();
			if (this.device.IsElementInXml("Continue", null))
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Continue Button");
				foreach (Element element in await this.device.WaitForElementsBy(0, "Continue", 10.0))
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
			}
			else if (this.device.IsElementInXml("Next", null))
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Next Button");
				foreach (Element element in await this.device.WaitForElementsBy(0, "Next", 20.0))
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
			}
			else
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Continue Button");
				List<Element> list = await this.device.WaitForElementsBy(0, "Continue", 10.0);
				if (list != null && list.Count > 0)
				{
					foreach (Element element in list)
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
				}
				else
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Next Button");
					List<Element> list2 = await this.device.WaitForElementsBy(0, "Next", 20.0);
					if (list2 != null && list2.Count > 0)
					{
						foreach (Element element in list2)
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
					}
				}
			}
		}

		// Token: 0x06000130 RID: 304 RVA: 0x000551F4 File Offset: 0x000533F4
		private async Task<bool> SetUpBirthday()
		{
			bool flag = false;
			for (;;)
			{
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Selecting birthday");
				List<Element> list = await this.device.WaitForElementsByXPath("XCUIElementTypePickerWheel", 9, "true", 10.0);
				foreach (Element element in list)
				{
					Position position = await element.GetPosition();
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Swipe weel");
					await this.device.Swipe((int)position.x + 2, (int)position.y - 10, (int)position.x + 2, (int)position.y + ConstParams.rand.Next(200, 450), 0.0);
				}
				List<Element>.Enumerator enumerator = default(List<Element>.Enumerator);
				await Task.Delay(TimeSpan.FromSeconds(2.0));
				await this.ClickNext();
				await Task.Delay(TimeSpan.FromSeconds(5.0));
				TaskAwaiter<Element> taskAwaiter = this.device.WaitForElementBy(3, "Something went wrong", 3.0).GetAwaiter();
				if (!taskAwaiter.IsCompleted)
				{
					await taskAwaiter;
					TaskAwaiter<Element> taskAwaiter2;
					taskAwaiter = taskAwaiter2;
					taskAwaiter2 = default(TaskAwaiter<Element>);
				}
				if (taskAwaiter.GetResult() == null)
				{
					goto IL_0546;
				}
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Birthday something went wrong error");
				if (flag)
				{
					break;
				}
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Click OK Button");
				try
				{
					await (await this.device.WaitForElementBy(0, "OK", 3.0)).Click(false);
				}
				catch
				{
				}
				flag = true;
			}
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Birthday selecting antispam error");
			return false;
			IL_0546:
			return true;
		}

		// Token: 0x06000131 RID: 305 RVA: 0x00055238 File Offset: 0x00053438
		public async Task SetUpPassword()
		{
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Enter Password Box");
			await (await this.device.WaitForElementBy(1, "Enter password", 5.0)).SetText(ConstParams.CONST_PASSWORD);
			if (this.device.IsElementInXml("Save login info", null))
			{
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Check for Save login info");
					foreach (Element element in await this.device.WaitForElementsBy(0, "Save login info", 3.0))
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
								TaskAwaiter<string> taskAwaiter3 = element.Value().GetAwaiter();
								if (!taskAwaiter3.IsCompleted)
								{
									await taskAwaiter3;
									TaskAwaiter<string> taskAwaiter4;
									taskAwaiter3 = taskAwaiter4;
									taskAwaiter4 = default(TaskAwaiter<string>);
								}
								if (taskAwaiter3.GetResult().Contains("Checkbox checked"))
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Save login info Button");
									await element.Click(false);
								}
							}
						}
						catch
						{
						}
						element = null;
					}
					List<Element>.Enumerator enumerator = default(List<Element>.Enumerator);
				}
				catch (Exception ex)
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: SLI 2 Exception: " + ex.ToString());
				}
			}
			await this.ClickNext();
		}

		// Token: 0x06000132 RID: 306 RVA: 0x0005527C File Offset: 0x0005347C
		public async Task<Making.MakeAccountResult> MakeAccount(AnyMessageClient anyMessageClient, bool EmailConfirmed, TikTokUtils tikTokUtils, string PhoneModel)
		{
			bool flag = false;
			for (;;)
			{
				IL_001A:
				try
				{
					await tikTokUtils.FirstActivateTikTok(true, PhoneModel);
					DateTime dateTime = DateTime.Now.AddSeconds(15.0);
					TaskAwaiter<bool> taskAwaiter2;
					while (DateTime.Now < dateTime)
					{
						WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Source XML | SU1");
						await this.device.GetSourceXml();
						if (this.device.IsElementInXml("Use phone or email", null))
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Use Phone Or Email Button");
							await (await this.device.WaitForElementBy(0, "Use phone or email", 10.0)).Click(false);
							break;
						}
						if (this.device.IsElementInXmlContains("with Email", null))
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Continue with Email Button");
							await (await this.device.WaitForElementBy(3, "with Email", 10.0)).Click(false);
							break;
						}
						if (this.device.IsElementInXmlContains("Use email instead", null))
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Use email instead Button");
							await (await this.device.WaitForElementBy(0, "Use email instead", 10.0)).Click(false);
							break;
						}
						if (this.device.IsElementInXmlContains("Use phone instead", null))
						{
							break;
						}
						if (this.device.IsElementInXml("Birthday", null))
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]:  Birthday Element Found");
							TaskAwaiter<bool> taskAwaiter = this.SetUpBirthday().GetAwaiter();
							if (!taskAwaiter.IsCompleted)
							{
								await taskAwaiter;
								taskAwaiter = taskAwaiter2;
								taskAwaiter2 = default(TaskAwaiter<bool>);
							}
							if (!taskAwaiter.GetResult())
							{
								return Making.MakeAccountResult.SomethingWrong;
							}
							DateTime dateTime2 = DateTime.Now.AddSeconds(10.0);
							while (dateTime2 <= DateTime.Now)
							{
								WorkUtils.WriteLog("[" + this.phoneUID + "]:  Checking for sending Birthday");
								await this.device.GetSourceXml();
								if (!this.device.IsElementInXml("Birthday", null))
								{
									dateTime = DateTime.Now.AddSeconds(5.0);
								}
								else
								{
									await Task.Delay(TimeSpan.FromSeconds(1.0));
									dateTime = DateTime.Now.AddSeconds(7.0);
								}
							}
						}
						await tikTokUtils.ActivateChecks();
						await Task.Delay(500);
					}
					bool flag2 = false;
					bool flag3 = false;
					int i = 0;
					while (i < 14)
					{
						int num = 0;
						int num3;
						try
						{
							flag3 = false;
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Source XML | SU2");
							await this.device.GetSourceXml();
							if (this.device.IsElementInXmlContains("Verify to continue", null) || this.device.IsElementInXml("verify captcha", null) || this.device.IsElementInXml("Drag the puzzle piece into place", null))
							{
								flag3 = true;
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Captcha Founded");
								await this.device.Tap(182.0, 59.0);
								await Task.Delay(TimeSpan.FromSeconds(2.0));
								try
								{
									await this.ClickNext();
								}
								catch
								{
								}
								goto IL_2BF4;
							}
							if (this.device.IsElementInXmlContains("Use email instead", null))
							{
								flag3 = true;
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Use email instead Button");
								await (await this.device.WaitForElementBy(0, "Use email instead", 10.0)).Click(false);
							}
							if (this.device.IsElementInXml("Add another account", null) || this.device.IsElementInXmlContains("have an account? Sign up", null) || this.device.IsElementInXml("Log in to TikTok", null) || this.device.IsElementInXmlContains("have an account? Log in", null) || this.device.IsElementInXmlContains("Create an account", null) || this.device.IsElementInXmlContains("Continue with email", null))
							{
								flag3 = true;
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Don't sign FIRST");
								goto IL_001A;
							}
							if (this.device.IsElementInXmlContains("Some iCloud Data Isn", null))
							{
								flag3 = true;
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Not Now Button");
								this.device.WaitForElementBy(0, "Not Now", 5.0).ConfigureAwait(false).GetAwaiter()
									.GetResult()
									.Click(false)
									.ConfigureAwait(false)
									.GetAwaiter()
									.GetResult();
								Task.Delay(500).ConfigureAwait(false).GetAwaiter()
									.GetResult();
								goto IL_2BF4;
							}
							if (this.device.IsElementInXml("Allow", null))
							{
								flag3 = true;
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Allow Button");
								await (await this.device.WaitForElementBy(0, "Allow", 5.0)).Click(false);
								Task.Delay(500).ConfigureAwait(false).GetAwaiter()
									.GetResult();
								goto IL_2BF4;
							}
							if (this.device.IsElementInXml("Maximum number of attempts reached", null))
							{
								flag3 = true;
								if (flag)
								{
									return Making.MakeAccountResult.Maximum;
								}
								flag = true;
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Left Button");
								await (await this.device.WaitForElementBy(0, "IconChevronLeftLTR", 10.0)).Click(false);
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Go Back Button");
								await (await this.device.WaitForElementBy(0, "Go back", 10.0)).Click(false);
								await Task.Delay(TimeSpan.FromSeconds(1.0));
								await this.ClickNext();
							}
							if (this.device.IsElementInXml("Session expired. Log in to continue.", null))
							{
								flag3 = true;
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Exception Session Expired");
								return Making.MakeAccountResult.SomethingWrong;
							}
							if (this.device.IsElementInXml("Continue with Face ID or Touch ID", null) || this.device.IsElementInXml("Use another method", null))
							{
								flag3 = true;
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Use Another Method Button");
								this.device.WaitForElementBy(0, "Use another method", 10.0).ConfigureAwait(false).GetAwaiter()
									.GetResult()
									.Click(false)
									.ConfigureAwait(false)
									.GetAwaiter()
									.GetResult();
							}
							if (this.device.IsElementInXmlContains("ve already signed up", null) || this.device.IsElementInXmlContains("Tap Continue to log in to your account", null))
							{
								flag3 = true;
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Check for Save login info");
								foreach (Element element in this.device.WaitForElementsBy(0, "Save login info", 3.0).ConfigureAwait(false).GetAwaiter()
									.GetResult())
								{
									try
									{
										if (element.Visible().ConfigureAwait(false).GetAwaiter()
											.GetResult() && element.Value().ConfigureAwait(false).GetAwaiter()
											.GetResult()
											.Contains("Checkbox checked"))
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Save login info Button");
											element.Click(false).ConfigureAwait(false).GetAwaiter()
												.GetResult();
										}
									}
									catch
									{
									}
								}
								this.ClickNext().ConfigureAwait(false).GetAwaiter()
									.GetResult();
							}
							if (this.device.IsElementInXmlContains("t change settings for security reasons", null))
							{
								flag3 = true;
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Can't change password || Click Skip Button");
								await (await this.device.WaitForElementBy(0, "Skip", 5.0)).Click(false);
							}
							if (this.device.IsElementInXml("Birthday", null))
							{
								flag3 = true;
								WorkUtils.WriteLog("[" + this.phoneUID + "]:  Birthday Element Found");
								TaskAwaiter<bool> taskAwaiter = this.SetUpBirthday().GetAwaiter();
								if (!taskAwaiter.IsCompleted)
								{
									await taskAwaiter;
									taskAwaiter = taskAwaiter2;
									taskAwaiter2 = default(TaskAwaiter<bool>);
								}
								if (!taskAwaiter.GetResult())
								{
									return Making.MakeAccountResult.SomethingWrong;
								}
								DateTime dateTime2 = DateTime.Now.AddSeconds(10.0);
								while (dateTime2 <= DateTime.Now)
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]:  Checking for sending Birthday");
									await this.device.GetSourceXml();
									if (!this.device.IsElementInXml("Birthday", null))
									{
										break;
									}
									await Task.Delay(TimeSpan.FromSeconds(1.0));
								}
							}
							TaskAwaiter<AuthUtils.VerifyEmailResult> taskAwaiter4;
							if (this.device.IsElementInXml("Verify your email", null) || this.device.IsElementInXml("Verify email", null) || this.device.IsElementInXml("Resend code", null) || this.device.IsElementInXmlContains("Send code", null) || this.device.IsElementInXmlContains("digit code", null) || this.device.IsElementInXml("Verify identity", null))
							{
								flag3 = true;
								WorkUtils.WriteLog("[" + this.phoneUID + "]:  VE Found");
								TaskAwaiter<AuthUtils.VerifyEmailResult> taskAwaiter3 = AuthUtils.VerifyEmail(anyMessageClient, this.device, this.phoneUID, Settings.Default.emailDomain.Contains("long_")).GetAwaiter();
								if (!taskAwaiter3.IsCompleted)
								{
									await taskAwaiter3;
									taskAwaiter3 = taskAwaiter4;
									taskAwaiter4 = default(TaskAwaiter<AuthUtils.VerifyEmailResult>);
								}
								switch (taskAwaiter3.GetResult())
								{
								case AuthUtils.VerifyEmailResult.TimeOut:
									return Making.MakeAccountResult.VerifyException;
								case AuthUtils.VerifyEmailResult.Exception:
									return Making.MakeAccountResult.VerifyException;
								case AuthUtils.VerifyEmailResult.Banned:
									return Making.MakeAccountResult.VerifyException;
								}
								DateTime dateTime2 = DateTime.Now.AddSeconds(10.0);
								while (dateTime2 <= DateTime.Now)
								{
									await this.device.GetSourceXml();
									if (!this.device.IsElementInXml("Email", null) && !this.device.IsElementInXml("Verify your email", null) && !this.device.IsElementInXml("Verify email", null) && !this.device.IsElementInXml("Resend code", null) && !this.device.IsElementInXmlContains("Send code", null) && !this.device.IsElementInXmlContains("digit code", null) && !this.device.IsElementInXml("Verify identity", null) && !this.device.IsElementInXml("Enter email", null) && !this.device.IsElementInXml("Email address", null) && !this.device.IsElementInXmlContains("verification code via email", null))
									{
										break;
									}
									await Task.Delay(TimeSpan.FromSeconds(1.0));
								}
							}
							if (this.device.IsElementInXml("Email", null) || this.device.IsElementInXml("Enter email", null) || this.device.IsElementInXml("Email address", null) || this.device.IsElementInXmlContains("verification code via email", null))
							{
								flag3 = true;
								WorkUtils.WriteLog("[" + this.phoneUID + "]:  EE Found");
								await this.SetUpEmail(anyMessageClient, flag2);
								DateTime dateTime2 = DateTime.Now.AddSeconds(10.0);
								while (dateTime2 <= DateTime.Now)
								{
									await this.device.GetSourceXml();
									if (!this.device.IsElementInXml("Email", null) && !this.device.IsElementInXml("Enter email", null) && !this.device.IsElementInXml("Email address", null) && !this.device.IsElementInXmlContains("verification code via email", null))
									{
										break;
									}
									if (this.device.IsElementInXmlContains("For security purposes, link your phone number to your account.", null) || this.device.IsElementInXmlContains("To continue, update to the latest version of the app", null))
									{
										return Making.MakeAccountResult.Maximum;
									}
									WorkUtils.WriteLog("[" + this.phoneUID + "]:  EC W");
									await Task.Delay(TimeSpan.FromSeconds(1.0));
								}
								TaskAwaiter<AuthUtils.VerifyEmailResult> taskAwaiter3 = AuthUtils.VerifyEmail(anyMessageClient, this.device, this.phoneUID, Settings.Default.emailDomain.Contains("long_")).GetAwaiter();
								if (!taskAwaiter3.IsCompleted)
								{
									await taskAwaiter3;
									taskAwaiter3 = taskAwaiter4;
									taskAwaiter4 = default(TaskAwaiter<AuthUtils.VerifyEmailResult>);
								}
								switch (taskAwaiter3.GetResult())
								{
								case AuthUtils.VerifyEmailResult.Success:
									WorkUtils.WriteLog("[" + this.phoneUID + "]:  VE Success");
									break;
								case AuthUtils.VerifyEmailResult.Unsuccess:
									WorkUtils.WriteLog("[" + this.phoneUID + "]:  VE Unsuccess");
									break;
								case AuthUtils.VerifyEmailResult.TimeOut:
									WorkUtils.WriteLog("[" + this.phoneUID + "]:  VE Timeout");
									return Making.MakeAccountResult.VerifyException;
								case AuthUtils.VerifyEmailResult.Exception:
									WorkUtils.WriteLog("[" + this.phoneUID + "]:  VE Exception");
									return Making.MakeAccountResult.VerifyException;
								case AuthUtils.VerifyEmailResult.Banned:
									return Making.MakeAccountResult.VerifyException;
								}
								dateTime2 = DateTime.Now.AddSeconds(10.0);
								while (dateTime2 <= DateTime.Now)
								{
									await this.device.GetSourceXml();
									if (!this.device.IsElementInXml("Email", null) && !this.device.IsElementInXml("Verify your email", null) && !this.device.IsElementInXml("Verify email", null) && !this.device.IsElementInXml("Resend code", null) && !this.device.IsElementInXmlContains("Send code", null) && !this.device.IsElementInXmlContains("digit code", null) && !this.device.IsElementInXml("Verify identity", null) && !this.device.IsElementInXml("Enter email", null) && !this.device.IsElementInXml("Email address", null) && !this.device.IsElementInXmlContains("verification code via email", null))
									{
										break;
									}
									await Task.Delay(TimeSpan.FromSeconds(1.0));
								}
								goto IL_2BF4;
							}
							if (this.device.IsElementInXml("Enter password", null) || this.device.IsElementInXml("Password", null))
							{
								flag3 = true;
								await this.SetUpPassword();
								DateTime dateTime2 = DateTime.Now.AddSeconds(10.0);
								while (dateTime2 <= DateTime.Now)
								{
									await this.device.GetSourceXml();
									if (!this.device.IsElementInXml("Enter password", null) && !this.device.IsElementInXml("Password", null))
									{
										break;
									}
									await Task.Delay(TimeSpan.FromSeconds(1.0));
								}
							}
							if (this.device.IsElementInXml("Account Status", null) || this.device.IsElementInXmlContains("account may be at risk", null))
							{
								flag3 = true;
								WorkUtils.WriteLog("[" + this.phoneUID + "]: RISK | Click OK Button");
								int num2 = 0;
								try
								{
									await (await this.device.WaitForElementBy(0, "OK", 10.0)).Click(false);
									await Task.Delay(TimeSpan.FromSeconds(5.0));
								}
								catch
								{
									num2 = 1;
								}
								num3 = num2;
								if (num3 == 1)
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: RISK2 | Click OK Button");
									await (await this.device.WaitForElementBy(0, "Ok", 5.0)).Click(false);
									await Task.Delay(TimeSpan.FromSeconds(5.0));
								}
								await tikTokUtils.RemoveAccount();
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Making Account Unsuccess || RISK");
								return Making.MakeAccountResult.RISK;
							}
							if (this.device.IsElementInXml("Add your nickname", null) || this.device.IsElementInXml("Profile", null))
							{
								flag3 = true;
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Skip Button");
								try
								{
									await (await this.device.WaitForElementByXPath("XCUIElementTypeButton", 0, "Skip", 5.0)).Click(false);
								}
								catch
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Skip Button not found");
								}
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Appium Configurate DEF");
								await this.device.ConfigurateAppium(17);
								num3 = 0;
								TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter6;
								try
								{
									Element element2 = await this.device.WaitForElementBy(0, "Profile", 4.0);
									TaskAwaiter<bool> taskAwaiter = element2.Visible().GetAwaiter();
									if (!taskAwaiter.IsCompleted)
									{
										await taskAwaiter;
										taskAwaiter = taskAwaiter2;
										taskAwaiter2 = default(TaskAwaiter<bool>);
									}
									if (taskAwaiter.GetResult())
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Profile Button");
										await element2.Click(false);
										return Making.MakeAccountResult.Success;
									}
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Profile button not visible");
									TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter5 = tikTokUtils.ActivateTikTokGotoProfile().GetAwaiter();
									if (!taskAwaiter5.IsCompleted)
									{
										await taskAwaiter5;
										taskAwaiter5 = taskAwaiter6;
										taskAwaiter6 = default(TaskAwaiter<TikTokUtils.ActivateResult>);
									}
									if (taskAwaiter5.GetResult() != TikTokUtils.ActivateResult.Success)
									{
										return Making.MakeAccountResult.Unsuccess;
									}
									return Making.MakeAccountResult.Success;
								}
								catch
								{
									num3 = 1;
								}
								if (num3 == 1)
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Don't have profile button");
									await tikTokUtils.GoHome(false);
									await tikTokUtils.TerminateTikTok();
									TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter5 = tikTokUtils.ActivateTikTokGotoProfile().GetAwaiter();
									if (!taskAwaiter5.IsCompleted)
									{
										await taskAwaiter5;
										taskAwaiter5 = taskAwaiter6;
										taskAwaiter6 = default(TaskAwaiter<TikTokUtils.ActivateResult>);
									}
									if (taskAwaiter5.GetResult() != TikTokUtils.ActivateResult.Success)
									{
										return Making.MakeAccountResult.Unsuccess;
									}
									return Making.MakeAccountResult.Success;
								}
							}
						}
						catch (Exception obj)
						{
							num = 1;
						}
						goto IL_2A25;
						IL_2BF4:
						num3 = i++;
						continue;
						IL_2A25:
						num3 = num;
						object obj;
						if (num3 == 1)
						{
							Exception ex = (Exception)obj;
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Maker Exception: " + ex.ToString());
							if (ex.ToString().Contains("no emails"))
							{
								flag2 = true;
							}
							await Task.Delay(TimeSpan.FromSeconds(3.0));
							goto IL_2BF4;
						}
						obj = null;
						if (!flag3)
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Tap By COORDS || JUST TAP");
							await this.device.Tap(35.0, 120.0);
						}
						await Task.Delay(500);
						goto IL_2BF4;
					}
					string text = this.phoneUID;
					WorkUtils.WriteLog("[" + text + "]: Making Unsuccess: " + await this.device.GetSource());
					text = null;
				}
				catch (Exception ex2)
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Making Account Exception: " + ex2.ToString());
				}
				break;
			}
			WorkUtils.WriteLog("[" + this.phoneUID + "]: Making Account Unsuccess");
			return Making.MakeAccountResult.Unsuccess;
		}

		// Token: 0x04000408 RID: 1032
		public iDevice device;

		// Token: 0x04000409 RID: 1033
		public string phoneUID = "";

		// Token: 0x02000078 RID: 120
		public enum MakeAccountResult
		{
			// Token: 0x0400040B RID: 1035
			Success,
			// Token: 0x0400040C RID: 1036
			Unsuccess,
			// Token: 0x0400040D RID: 1037
			Maximum,
			// Token: 0x0400040E RID: 1038
			Exception,
			// Token: 0x0400040F RID: 1039
			VerifyException,
			// Token: 0x04000410 RID: 1040
			RISK,
			// Token: 0x04000411 RID: 1041
			SomethingWrong
		}
	}
}
