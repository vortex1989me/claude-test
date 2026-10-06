using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TikTok_Automation_Library_Non_Jail.Forms;
using TikTok_Automation_Library_Non_Jail.Utils.TempMail;
using TikTok_Automation_Library_Non_Jail.Work.Utils;
using WDA_Framework;

namespace TikTok_Automation_Library_Non_Jail.Work.SignIn
{
	// Token: 0x02000062 RID: 98
	internal class SignIn
	{
		// Token: 0x060000FD RID: 253 RVA: 0x0003E270 File Offset: 0x0003C470
		public async Task<SignIn.SignResult> OnlySignIn(string username, string email, string password, string anyMessageID, TikTokUtils tikTokUtils, bool LL, AnyMessageClient anyMessageClient, string FullAccountData)
		{
			int num = 0;
			SignIn.SignResult signResult;
			for (;;)
			{
				int num2 = 0;
				int num4;
				try
				{
					WorkUtils.WriteLog("[" + this.phoneUID + "]: Configurate Appium 17 | DEF SI");
					await this.device.ConfigurateAppium(17);
					await tikTokUtils.ActivateTikTok();
					await Task.Delay(TimeSpan.FromSeconds(2.0));
					DateTime.Now.AddSeconds(15.0);
					bool flag = true;
					int i = 0;
					string text2;
					while (i < 20)
					{
						int num3 = 0;
						try
						{
							WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Source XML | SI0");
							await this.device.GetSourceXml();
							flag = true;
							if (this.device.IsElementInXmlContains("Wants to Use", null))
							{
								try
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Cancel Button");
									this.device.WaitForElementBy(0, "Cancel", 3.0).ConfigureAwait(false).GetAwaiter()
										.GetResult()
										.Click(false)
										.ConfigureAwait(false)
										.GetAwaiter()
										.GetResult();
									goto IL_BB04;
								}
								catch
								{
								}
							}
							if (this.device.IsElementInXmlContains("Verify to continue", null) || this.device.IsElementInXml("verify captcha", null) || this.device.IsElementInXml("Drag the puzzle piece into place", null))
							{
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Captcha Founded");
								await this.device.Tap(182.0, 59.0);
								await Task.Delay(TimeSpan.FromSeconds(2.0));
								goto IL_BB04;
							}
							if (this.device.IsElementInXml("Agree and continue", null))
							{
								try
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Check for Agree and continue Button");
									await (await this.device.WaitForElementBy(0, "Agree and continue", 10.0)).Click(false);
								}
								catch
								{
								}
							}
							if (this.device.IsElementInXmlContains("Maximum number of attempts reached", null))
							{
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Maximum Error Found");
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Need help logging in Button");
								await (await this.device.WaitForElementBy(0, "Need help logging in?", 10.0)).Click(false);
								await Task.Delay(TimeSpan.FromSeconds(2.0));
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click BY COORDS");
								if (this.device.PhoneModel.Contains("12"))
								{
									await this.device.Tap(150.0, 688.0);
								}
								else
								{
									await this.device.Tap(150.0, 700.0);
								}
								await Task.Delay(TimeSpan.FromSeconds(2.0));
								goto IL_BB04;
							}
							if (this.device.IsElementInXml("Add another account", null))
							{
								flag = false;
								if (this.device.IsElementInXml(username, null))
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click By UserName: " + username);
									await (await this.device.WaitForElementBy(0, username, 10.0)).Click(false);
								}
								else
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Remove Account Button");
									Element element = await this.device.WaitForElementBy(0, "Remove account", 3.0);
									if (element != null)
									{
										int j = 0;
										try
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Account Button");
											await element.Click(false);
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button");
											await (await this.device.WaitForElementBy(0, "Remove", 10.0)).Click(false);
										}
										catch
										{
											j = 1;
										}
										num4 = j;
										if (num4 == 1)
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Account Button BY COORDS");
											await this.device.Tap(340.0, 232.0);
											await Task.Delay(TimeSpan.FromSeconds(1.0));
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button BY COORDS");
											await this.device.Tap(210.0, 500.0);
										}
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
										}
										else
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Account Button BY COORDS");
											await this.device.Tap(340.0, 232.0);
											await Task.Delay(TimeSpan.FromSeconds(1.0));
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Remove Button BY COORDS");
											await this.device.Tap(210.0, 500.0);
										}
									}
									Task.Delay(TimeSpan.FromSeconds(3.0)).ConfigureAwait(false).GetAwaiter()
										.GetResult();
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Source XML | SI00");
									await this.device.GetSourceXml();
									if (this.device.IsElementInXml("Enter password", null) || this.device.IsElementInXml("Password hidding", null) || this.device.IsElementInXml("Password display status", null) || this.device.IsElementInXmlContains("s really you", null) || this.device.IsElementInXmlContains("Verify your email", null) || this.device.IsElementInXmlContains("Verify email", null) || this.device.IsElementInXmlContains("step verification", null) || this.device.IsElementInXml("Your code was sent to your authenticator app", null))
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Go back Button");
										await (await this.device.WaitForElementBy(0, "Go back", 10.0)).Click(false);
										await Task.Delay(TimeSpan.FromSeconds(1.0));
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Source XML | SI00");
										await this.device.GetSourceXml();
										if (this.device.IsElementInXmlNotVisible("Log into existing account", null))
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Configurate Appium 25 | DSA1");
											this.device.ConfigurateAppium(25).ConfigureAwait(false).GetAwaiter()
												.GetResult();
											goto IL_BB04;
										}
									}
								}
								if (this.device.IsElementInXml("Add another account", null))
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Add another account Button");
									await (await this.device.WaitForElementBy(0, "Add another account", 10.0)).Click(false);
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
								}
								catch
								{
								}
							}
							TaskAwaiter<string> taskAwaiter2;
							if (this.device.IsElementInXmlContains("Continue with email", null))
							{
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Continue with email Button");
								await (await this.device.WaitForElementBy(3, "Continue with email", 10.0)).Click(false);
								bool flag2 = false;
								int j = 0;
								while (j < 10)
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Source XML | SI2 NEW");
									await this.device.GetSourceXml();
									if (this.device.IsElementInXml("Time out. Please try again.", null))
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Time Out Error!");
										return SignIn.SignResult.TimeOut;
									}
									if (this.device.IsElementInXmlplaceholderValue("Email or username", null))
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Email or username box");
										Element element2 = await this.device.WaitForElementBy(11, "Email or username", 10.0);
										if (element2 != null)
										{
											TaskAwaiter<string> taskAwaiter = element2.Value().GetAwaiter();
											if (!taskAwaiter.IsCompleted)
											{
												await taskAwaiter;
												taskAwaiter = taskAwaiter2;
												taskAwaiter2 = default(TaskAwaiter<string>);
											}
											if (taskAwaiter.GetResult() != "Email or username")
											{
												await element2.ClearText();
											}
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Write username: " + username);
											await element2.SetText(username);
											flag2 = true;
											break;
										}
										element2 = null;
									}
									if (this.device.IsElementInXmlplaceholderValue("Email address", null))
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Email address box");
										Element element2 = await this.device.WaitForElementBy(11, "Email address", 10.0);
										if (element2 != null)
										{
											TaskAwaiter<string> taskAwaiter = element2.Value().GetAwaiter();
											if (!taskAwaiter.IsCompleted)
											{
												await taskAwaiter;
												taskAwaiter = taskAwaiter2;
												taskAwaiter2 = default(TaskAwaiter<string>);
											}
											if (taskAwaiter.GetResult() != "Email address")
											{
												await element2.ClearText();
											}
											await element2.SetText(username);
											flag2 = true;
											break;
										}
										element2 = null;
									}
									if (this.device.IsElementInXml("Email or username", null))
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Email or username box");
										Element element3 = await this.device.WaitForElementBy(1, "Email or username", 10.0);
										if (element3 != null)
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Write username: " + username);
											await element3.SetText(username);
											flag2 = true;
											break;
										}
									}
									if (this.device.IsElementInXml("Email address", null))
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Email address box");
										Element element4 = await this.device.WaitForElementBy(1, "Email address", 10.0);
										if (element4 != null)
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Write username: " + username);
											await element4.SetText(username);
											flag2 = true;
											break;
										}
									}
									if (this.device.IsElementXmlByXpath("XCUIElementTypeTextField", null))
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Email TextField by XPath");
										Element element2 = await this.device.WaitForElementByXPath("XCUIElementTypeTextField", 13, "true", 10.0);
										if (element2 != null)
										{
											string text = await element2.Value();
											if (text != "Email or username" && text != "Email address")
											{
												await element2.ClearText();
											}
											await element2.SetText(username);
											flag2 = true;
											break;
										}
										element2 = null;
									}
									await Task.Delay(TimeSpan.FromSeconds(1.0));
									if (!this.device.IsElementInXml("Enter password", null) && !this.device.IsElementInXml("Password hidding", null))
									{
										this.device.IsElementInXml("Password display status", null);
									}
									num4 = j++;
								}
								if (flag2)
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Log in button");
									await (await this.device.WaitForElementBy(3, "Log in", 10.0)).Click(false);
									await Task.Delay(TimeSpan.FromSeconds(2.0));
									goto IL_BB04;
								}
							}
							TaskAwaiter<bool> taskAwaiter4;
							object obj;
							if (this.device.IsElementInXmlContains("Use phone", null) || this.device.IsElementInXmlplaceholderValue("Email or username", null))
							{
								bool flag2 = false;
								int j = 0;
								List<Element>.Enumerator enumerator;
								try
								{
									flag = false;
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Use phone / email / username box");
									await (await this.device.WaitForElementBy(3, "Use phone ", 10.0)).Click(false);
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Searching for Email / Username Button");
									bool flag3 = false;
									List<Element> list = await this.device.WaitForElementsBy(3, "Email ", 10.0);
									if (list != null)
									{
										foreach (Element element2 in list)
										{
											TaskAwaiter<bool> taskAwaiter3 = element2.Visible().GetAwaiter();
											if (!taskAwaiter3.IsCompleted)
											{
												await taskAwaiter3;
												taskAwaiter3 = taskAwaiter4;
												taskAwaiter4 = default(TaskAwaiter<bool>);
											}
											if (taskAwaiter3.GetResult())
											{
												await element2.Click(false);
												flag3 = true;
											}
											element2 = null;
										}
										enumerator = default(List<Element>.Enumerator);
									}
									else
									{
										list = await this.device.WaitForElementsBy(12, "Email ", 3.0);
										foreach (Element element2 in list)
										{
											TaskAwaiter<bool> taskAwaiter3 = element2.Visible().GetAwaiter();
											if (!taskAwaiter3.IsCompleted)
											{
												await taskAwaiter3;
												taskAwaiter3 = taskAwaiter4;
												taskAwaiter4 = default(TaskAwaiter<bool>);
											}
											if (taskAwaiter3.GetResult())
											{
												await element2.Click(false);
												flag3 = true;
											}
											element2 = null;
										}
										enumerator = default(List<Element>.Enumerator);
									}
									if (!flag3)
									{
										foreach (Element element5 in list)
										{
											await element5.TapAlert(0, 0, false);
										}
										enumerator = default(List<Element>.Enumerator);
									}
									list = null;
								}
								catch (Exception obj)
								{
									j = 1;
								}
								num4 = j;
								if (num4 == 1)
								{
									Exception ex = (Exception)obj;
									text2 = this.phoneUID;
									WorkUtils.WriteLog("[" + text2 + "]: SI2 Source Page: " + await this.device.GetSource());
									text2 = null;
									flag2 = true;
								}
								obj = null;
								j = 0;
								while (j < 10)
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Source XML | SI2");
									await this.device.GetSourceXml();
									if (this.device.IsElementInXml("Time out. Please try again.", null))
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Time Out Error!");
										return SignIn.SignResult.TimeOut;
									}
									if (this.device.IsElementInXmlplaceholderValue("Email or username", null))
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Email or username box");
										Element element2 = await this.device.WaitForElementBy(11, "Email or username", 10.0);
										if (element2 != null)
										{
											TaskAwaiter<string> taskAwaiter = element2.Value().GetAwaiter();
											if (!taskAwaiter.IsCompleted)
											{
												await taskAwaiter;
												taskAwaiter = taskAwaiter2;
												taskAwaiter2 = default(TaskAwaiter<string>);
											}
											if (taskAwaiter.GetResult() != "Email or username")
											{
												await element2.ClearText();
											}
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Write username: " + username);
											await element2.SetText(username);
											break;
										}
										element2 = null;
									}
									if (this.device.IsElementInXmlplaceholderValue("Email address", null))
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Email address box");
										Element element2 = await this.device.WaitForElementBy(11, "Email address", 10.0);
										if (element2 != null)
										{
											TaskAwaiter<string> taskAwaiter = element2.Value().GetAwaiter();
											if (!taskAwaiter.IsCompleted)
											{
												await taskAwaiter;
												taskAwaiter = taskAwaiter2;
												taskAwaiter2 = default(TaskAwaiter<string>);
											}
											if (taskAwaiter.GetResult() != "Email address")
											{
												await element2.ClearText();
											}
											await element2.SetText(username);
											break;
										}
										element2 = null;
									}
									if (this.device.IsElementInXml("Email or username", null))
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Email or username box");
										Element element6 = await this.device.WaitForElementBy(1, "Email or username", 10.0);
										if (element6 != null)
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Write username: " + username);
											await element6.SetText(username);
											break;
										}
									}
									if (this.device.IsElementInXml("Email address", null))
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Email address box");
										Element element7 = await this.device.WaitForElementBy(1, "Email address", 10.0);
										if (element7 != null)
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Write username: " + username);
											await element7.SetText(username);
											break;
										}
									}
									if (this.device.IsElementXmlByXpath("XCUIElementTypeTextField", null))
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Email TextField by XPath");
										Element element2 = await this.device.WaitForElementByXPath("XCUIElementTypeTextField", 13, "true", 10.0);
										if (element2 != null)
										{
											string text3 = await element2.Value();
											if (text3 != "Email or username" && text3 != "Email address")
											{
												await element2.ClearText();
											}
											await element2.SetText(username);
											break;
										}
										element2 = null;
									}
									await Task.Delay(TimeSpan.FromSeconds(1.0));
									if (this.device.IsElementInXml("Enter password", null) || this.device.IsElementInXml("Password hidding", null) || this.device.IsElementInXml("Password display status", null) || flag2)
									{
									}
									num4 = j++;
								}
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Check for Save login info");
								List<Element> list2 = await this.device.WaitForElementsBy(0, "Save login info", 3.0);
								if (list2 != null)
								{
									foreach (Element element2 in list2)
									{
										try
										{
											TaskAwaiter<bool> taskAwaiter3 = element2.Visible().GetAwaiter();
											if (!taskAwaiter3.IsCompleted)
											{
												await taskAwaiter3;
												taskAwaiter3 = taskAwaiter4;
												taskAwaiter4 = default(TaskAwaiter<bool>);
											}
											if (taskAwaiter3.GetResult())
											{
												TaskAwaiter<string> taskAwaiter = element2.Value().GetAwaiter();
												if (!taskAwaiter.IsCompleted)
												{
													await taskAwaiter;
													taskAwaiter = taskAwaiter2;
													taskAwaiter2 = default(TaskAwaiter<string>);
												}
												if (taskAwaiter.GetResult().Contains("Checkbox checked"))
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
									enumerator = default(List<Element>.Enumerator);
								}
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Continue Button");
								foreach (Element element2 in await this.device.WaitForElementsBy(0, "Continue", 10.0))
								{
									Element element2;
									try
									{
										TaskAwaiter<bool> taskAwaiter3 = element2.Visible().GetAwaiter();
										if (!taskAwaiter3.IsCompleted)
										{
											await taskAwaiter3;
											taskAwaiter3 = taskAwaiter4;
											taskAwaiter4 = default(TaskAwaiter<bool>);
										}
										if (taskAwaiter3.GetResult())
										{
											await element2.Click(false);
										}
									}
									catch
									{
									}
									element2 = null;
								}
								enumerator = default(List<Element>.Enumerator);
							}
							TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter8;
							if (this.device.IsElementInXml("Enter password", null) || this.device.IsElementInXml("Password hidding", null) || this.device.IsElementInXml("Password display status", null))
							{
								flag = false;
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Write Password");
								int j = 0;
								try
								{
									await (await this.device.WaitForElementByXPath("XCUIElementTypeSecureTextField", 1, "Password", 3.0)).SetText(password);
								}
								catch
								{
									j = 1;
								}
								num4 = j;
								if (num4 == 1)
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Second Write Password");
									await (await this.device.WaitForElementByXPath("XCUIElementTypeSecureTextField", 9, "true", 3.0)).SetText(password);
								}
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Continue Button");
								foreach (Element element2 in await this.device.WaitForElementsBy(0, "Continue", 10.0))
								{
									Element element2;
									try
									{
										TaskAwaiter<bool> taskAwaiter3 = element2.Visible().GetAwaiter();
										if (!taskAwaiter3.IsCompleted)
										{
											await taskAwaiter3;
											taskAwaiter3 = taskAwaiter4;
											taskAwaiter4 = default(TaskAwaiter<bool>);
										}
										if (taskAwaiter3.GetResult())
										{
											await element2.Click(false);
										}
									}
									catch
									{
									}
									element2 = null;
								}
								List<Element>.Enumerator enumerator = default(List<Element>.Enumerator);
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Waiting for 2 seconds");
								await Task.Delay(TimeSpan.FromSeconds(2.0));
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Check for Trust this device to skip");
								TaskAwaiter<Element> taskAwaiter5 = this.device.WaitForElementBy(3, "Trust this device to skip", 3.0).GetAwaiter();
								TaskAwaiter<Element> taskAwaiter6;
								if (!taskAwaiter5.IsCompleted)
								{
									await taskAwaiter5;
									taskAwaiter5 = taskAwaiter6;
									taskAwaiter6 = default(TaskAwaiter<Element>);
								}
								if (taskAwaiter5.GetResult() != null)
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Trust this device to skip Button");
									try
									{
										await (await this.device.WaitForElementBy(1, "Checkbox checked", 3.0)).Click(false);
										goto IL_6142;
									}
									catch
									{
										goto IL_6142;
									}
								}
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Source XML | SI01Q");
								await this.device.GetSourceXml();
								if (this.device.IsElementInXmlContains("Maximum number of attempts reached", null))
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Maximum Error Found");
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Need help logging in Button");
									await (await this.device.WaitForElementBy(0, "Need help logging in?", 10.0)).Click(false);
									await Task.Delay(TimeSpan.FromSeconds(2.0));
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click BY COORDS");
									if (this.device.PhoneModel.Contains("12"))
									{
										await this.device.Tap(150.0, 688.0);
									}
									else
									{
										await this.device.Tap(150.0, 700.0);
									}
									await Task.Delay(TimeSpan.FromSeconds(2.0));
									goto IL_BB04;
								}
								if (this.device.IsElementInXml("Your account was banned", null) || this.device.IsElementInXmlContains("you can submit an appeal", null))
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Account Blocked!");
									try
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Dismiss Button");
										await (await this.device.WaitForElementBy(0, "Dismiss", 4.0)).Click(false);
									}
									catch
									{
									}
									return SignIn.SignResult.Blocked;
								}
								taskAwaiter5 = this.device.WaitForElementBy(3, "Would Like to Send You Notifications", 3.0).GetAwaiter();
								if (!taskAwaiter5.IsCompleted)
								{
									await taskAwaiter5;
									taskAwaiter5 = taskAwaiter6;
									taskAwaiter6 = default(TaskAwaiter<Element>);
								}
								if (taskAwaiter5.GetResult() != null)
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Logged!");
									TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter7 = tikTokUtils.ActivateTikTokGotoProfile().GetAwaiter();
									if (!taskAwaiter7.IsCompleted)
									{
										await taskAwaiter7;
										taskAwaiter7 = taskAwaiter8;
										taskAwaiter8 = default(TaskAwaiter<TikTokUtils.ActivateResult>);
									}
									if (taskAwaiter7.GetResult() != TikTokUtils.ActivateResult.Success)
									{
										return SignIn.SignResult.Unsuccess;
									}
									return SignIn.SignResult.Success;
								}
								else
								{
									taskAwaiter5 = this.device.WaitForElementBy(3, "s really you", 3.0).GetAwaiter();
									if (!taskAwaiter5.IsCompleted)
									{
										await taskAwaiter5;
										taskAwaiter5 = taskAwaiter6;
										taskAwaiter6 = default(TaskAwaiter<Element>);
									}
									if (taskAwaiter5.GetResult() != null)
									{
										try
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Check for Agree and continue Button");
											await (await this.device.WaitForElementBy(0, "Agree and continue", 4.0)).Click(false);
										}
										catch
										{
										}
										WorkUtils.WriteLog(string.Concat(new string[] { "[", this.phoneUID, "]: Reorder email: ", email, " Id: ", anyMessageID }));
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Click to Email");
										foreach (Element element8 in await this.device.WaitForElementsBy(3, "@", 10.0))
										{
											try
											{
												await element8.Click(false);
											}
											catch
											{
											}
										}
										enumerator = default(List<Element>.Enumerator);
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Next Button");
										try
										{
											await (await this.device.WaitForElementBy(3, "Next", 10.0)).Click(false);
										}
										catch
										{
										}
										AuthUtils.VerifyEmailResult verifyEmailResult = await AuthUtils.VerifyEmail(anyMessageClient, this.device, this.phoneUID, LL);
										if (verifyEmailResult == AuthUtils.VerifyEmailResult.Success)
										{
											j = 0;
											Element element2;
											for (;;)
											{
												num4 = j++;
												if (j > 15)
												{
													text2 = this.phoneUID;
													WorkUtils.WriteLog("[" + text2 + "]: UNKNOWN PAGE SOURCE: " + await this.device.GetSource());
													text2 = null;
													await tikTokUtils.GoHome(false);
													await tikTokUtils.TerminateTikTok();
													TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter7 = tikTokUtils.ActivateTikTokGotoProfile().GetAwaiter();
													if (!taskAwaiter7.IsCompleted)
													{
														await taskAwaiter7;
														taskAwaiter7 = taskAwaiter8;
														taskAwaiter8 = default(TaskAwaiter<TikTokUtils.ActivateResult>);
													}
													if (taskAwaiter7.GetResult() != TikTokUtils.ActivateResult.Success)
													{
														break;
													}
												}
												WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Source XML | SI3");
												await this.device.GetSourceXml();
												TaskAwaiter<bool> taskAwaiter3 = tikTokUtils.ActivateChecks().GetAwaiter();
												if (!taskAwaiter3.IsCompleted)
												{
													await taskAwaiter3;
													taskAwaiter3 = taskAwaiter4;
													taskAwaiter4 = default(TaskAwaiter<bool>);
												}
												if (!taskAwaiter3.GetResult())
												{
													if (this.device.IsElementInXmlContains("Maximum number of attempts reached", null))
													{
														goto Block_105;
													}
													if (this.device.IsElementInXml("Your account was banned", null) || this.device.IsElementInXmlContains("If you believe this was a mistake", null))
													{
														goto IL_5969;
													}
													if (this.device.IsElementInXml("Profile", null) || this.device.IsElementInXml("a11y_vo_profile", null))
													{
														WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Profile button Q");
														element2 = null;
														if (this.device.IsElementInXml("Profile", null))
														{
															element2 = await this.device.WaitForElementBy(0, "Profile", 10.0);
														}
														else
														{
															if (!this.device.IsElementInXml("a11y_vo_profile", null))
															{
																text2 = this.phoneUID;
																WorkUtils.WriteLog("[" + text2 + "]: PROFILE BUTTON ERROR: " + await this.device.GetSource());
																text2 = null;
																continue;
															}
															element2 = await this.device.WaitForElementBy(0, "a11y_vo_profile", 10.0);
														}
														if (element2 != null)
														{
															bool flag4 = await element2.Visible();
															if (flag4)
															{
																flag4 = await element2.Enabled();
															}
															if (flag4)
															{
																goto Block_112;
															}
														}
													}
												}
											}
											return SignIn.SignResult.Unsuccess;
											Block_105:
											return SignIn.SignResult.Maximum;
											IL_5969:
											return SignIn.SignResult.Blocked;
											Block_112:
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Profile Button");
											await element2.Click(false);
											try
											{
												WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Don't Allow Button");
												await (await this.device.WaitForElementBy(3, "t Allow", 3.0)).Click(false);
											}
											catch
											{
											}
											try
											{
												WorkUtils.WriteLog("[" + this.phoneUID + "]: Check for Ok Allert");
												await (await this.device.WaitForElementBy(0, "OK", 3.0)).Click(false);
											}
											catch
											{
											}
											try
											{
												WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Save Password Alert || Close");
												await (await this.device.WaitForElementBy(0, "close", 3.0)).Click(false);
											}
											catch
											{
											}
											return SignIn.SignResult.Success;
										}
										if (verifyEmailResult == AuthUtils.VerifyEmailResult.Banned)
										{
											return SignIn.SignResult.EmailBlocked;
										}
										return SignIn.SignResult.Unsuccess;
									}
									else
									{
										taskAwaiter5 = this.device.WaitForElementBy(0, "Profile", 3.0).GetAwaiter();
										if (!taskAwaiter5.IsCompleted)
										{
											await taskAwaiter5;
											taskAwaiter5 = taskAwaiter6;
											taskAwaiter6 = default(TaskAwaiter<Element>);
										}
										if (taskAwaiter5.GetResult() != null)
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Probably Logged!");
											TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter7 = tikTokUtils.ActivateTikTokGotoProfile().GetAwaiter();
											if (!taskAwaiter7.IsCompleted)
											{
												await taskAwaiter7;
												taskAwaiter7 = taskAwaiter8;
												taskAwaiter8 = default(TaskAwaiter<TikTokUtils.ActivateResult>);
											}
											if (taskAwaiter7.GetResult() != TikTokUtils.ActivateResult.Success)
											{
												return SignIn.SignResult.Unsuccess;
											}
											return SignIn.SignResult.Success;
										}
									}
								}
							}
							IL_6142:
							if (this.device.IsElementInXml("Reset password", null) && (this.device.IsElementInXml("Enter email", null) || this.device.IsElementInXml("Email address", null)))
							{
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Reset Password Feature || Write Email");
								Element element2 = await this.device.WaitForElementBy(11, "Email address", 10.0);
								if (element2 != null)
								{
									TaskAwaiter<string> taskAwaiter = element2.Value().GetAwaiter();
									if (!taskAwaiter.IsCompleted)
									{
										await taskAwaiter;
										taskAwaiter = taskAwaiter2;
										taskAwaiter2 = default(TaskAwaiter<string>);
									}
									if (taskAwaiter.GetResult() != "Email address")
									{
										await element2.ClearText();
									}
									await element2.SetText(email);
								}
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Continue Button");
								foreach (Element element9 in await this.device.WaitForElementsBy(0, "Continue", 10.0))
								{
									Element element9;
									try
									{
										TaskAwaiter<bool> taskAwaiter3 = element9.Visible().GetAwaiter();
										if (!taskAwaiter3.IsCompleted)
										{
											await taskAwaiter3;
											taskAwaiter3 = taskAwaiter4;
											taskAwaiter4 = default(TaskAwaiter<bool>);
										}
										if (taskAwaiter3.GetResult())
										{
											await element9.Click(false);
										}
									}
									catch
									{
									}
									element9 = null;
								}
								List<Element>.Enumerator enumerator = default(List<Element>.Enumerator);
								DateTime dateTime = DateTime.Now.AddSeconds(15.0);
								while (DateTime.Now < dateTime)
								{
									try
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Source XML | SI312Q");
										await this.device.GetSourceXml();
										if (!this.device.IsElementInXml("Reset password", null) || (!this.device.IsElementInXml("Enter email", null) && !this.device.IsElementInXml("Email address", null)))
										{
											break;
										}
										await Task.Delay(TimeSpan.FromSeconds(1.0));
									}
									catch
									{
									}
								}
								AuthUtils.VerifyEmailResult verifyEmailResult2 = await AuthUtils.VerifyEmail(anyMessageClient, this.device, this.phoneUID, LL);
								if (verifyEmailResult2 == AuthUtils.VerifyEmailResult.Success)
								{
									text2 = password;
									password += "1";
									int num5 = 0;
									try
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Write Password");
										int num6 = 0;
										try
										{
											await (await this.device.WaitForElementByXPath("XCUIElementTypeSecureTextField", 1, "Enter password", 7.0)).SetText(password);
										}
										catch
										{
											num6 = 1;
										}
										num4 = num6;
										if (num4 == 1)
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Second Write Password");
											await (await this.device.WaitForElementByXPath("XCUIElementTypeSecureTextField", 9, "true", 3.0)).SetText(password);
										}
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Continue Button");
										foreach (Element element10 in await this.device.WaitForElementsBy(0, "Continue", 10.0))
										{
											try
											{
												TaskAwaiter<bool> taskAwaiter3 = element10.Visible().GetAwaiter();
												if (!taskAwaiter3.IsCompleted)
												{
													await taskAwaiter3;
													taskAwaiter3 = taskAwaiter4;
													taskAwaiter4 = default(TaskAwaiter<bool>);
												}
												if (taskAwaiter3.GetResult())
												{
													await element10.Click(false);
												}
											}
											catch
											{
											}
											element10 = null;
										}
										enumerator = default(List<Element>.Enumerator);
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Waiting for 5 seconds");
										await Task.Delay(TimeSpan.FromSeconds(5.0));
										Form1.OutData.AccountsWithResettedPasswords.Enqueue(FullAccountData.Replace(text2, password));
									}
									catch (Exception obj)
									{
										num5 = 1;
									}
									num4 = num5;
									if (num4 == 1)
									{
										Exception ex2 = (Exception)obj;
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Password Reset Exception: " + ex2.ToString());
										string text4 = this.phoneUID;
										WorkUtils.WriteLog("[" + text4 + "]: Reset Password PAGE SOURCE: " + await this.device.GetSource());
										text4 = null;
									}
									obj = null;
									int j = 0;
									Element element9;
									for (;;)
									{
										num4 = j++;
										if (j > 15)
										{
											string text4 = this.phoneUID;
											WorkUtils.WriteLog("[" + text4 + "]: UNKNOWN PAGE SOURCE: " + await this.device.GetSource());
											text4 = null;
											await tikTokUtils.GoHome(false);
											await tikTokUtils.TerminateTikTok();
											TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter7 = tikTokUtils.ActivateTikTokGotoProfile().GetAwaiter();
											if (!taskAwaiter7.IsCompleted)
											{
												await taskAwaiter7;
												taskAwaiter7 = taskAwaiter8;
												taskAwaiter8 = default(TaskAwaiter<TikTokUtils.ActivateResult>);
											}
											if (taskAwaiter7.GetResult() != TikTokUtils.ActivateResult.Success)
											{
												break;
											}
										}
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Source XML | SI4");
										await this.device.GetSourceXml();
										TaskAwaiter<bool> taskAwaiter3 = tikTokUtils.ActivateChecks().GetAwaiter();
										if (!taskAwaiter3.IsCompleted)
										{
											await taskAwaiter3;
											taskAwaiter3 = taskAwaiter4;
											taskAwaiter4 = default(TaskAwaiter<bool>);
										}
										if (!taskAwaiter3.GetResult())
										{
											if (this.device.IsElementInXmlContains("Maximum number of attempts reached", null))
											{
												goto Block_136;
											}
											if (this.device.IsElementInXml("Profile", null) || this.device.IsElementInXml("a11y_vo_profile", null))
											{
												WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Profile button W");
												element9 = null;
												if (this.device.IsElementInXml("Profile", null))
												{
													element9 = await this.device.WaitForElementBy(0, "Profile", 10.0);
												}
												else
												{
													if (!this.device.IsElementInXml("a11y_vo_profile", null))
													{
														string text4 = this.phoneUID;
														WorkUtils.WriteLog("[" + text4 + "]: PROFILE BUTTON ERROR: " + await this.device.GetSource());
														text4 = null;
														continue;
													}
													element9 = await this.device.WaitForElementBy(0, "a11y_vo_profile", 10.0);
												}
												if (element9 != null)
												{
													bool flag4 = await element9.Visible();
													if (flag4)
													{
														flag4 = await element9.Enabled();
													}
													if (flag4)
													{
														goto Block_142;
													}
												}
											}
										}
									}
									return SignIn.SignResult.Unsuccess;
									Block_136:
									return SignIn.SignResult.Maximum;
									Block_142:
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Profile Button");
									await element9.Click(false);
									try
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Don't Allow Button");
										await (await this.device.WaitForElementBy(3, "t Allow", 3.0)).Click(false);
									}
									catch
									{
									}
									try
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Check for Ok Allert");
										await (await this.device.WaitForElementBy(0, "OK", 3.0)).Click(false);
									}
									catch
									{
									}
									try
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Save Password Alert || Close");
										await (await this.device.WaitForElementBy(0, "close", 3.0)).Click(false);
									}
									catch
									{
									}
									return SignIn.SignResult.Success;
								}
								switch (verifyEmailResult2)
								{
								case AuthUtils.VerifyEmailResult.TimeOut:
									return SignIn.SignResult.VerifyException;
								case AuthUtils.VerifyEmailResult.Exception:
									return SignIn.SignResult.VerifyException;
								case AuthUtils.VerifyEmailResult.Banned:
									return SignIn.SignResult.EmailBlocked;
								default:
									element2 = null;
									break;
								}
							}
							if (this.device.IsElementInXmlContains("s really you", null))
							{
								WorkUtils.WriteLog(string.Concat(new string[] { "[", this.phoneUID, "]: Reorder email: ", email, " Id: ", anyMessageID }));
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click to Email");
								foreach (Element element11 in await this.device.WaitForElementsBy(3, "@", 10.0))
								{
									try
									{
										await element11.Click(false);
									}
									catch
									{
									}
								}
								List<Element>.Enumerator enumerator = default(List<Element>.Enumerator);
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Next Button");
								try
								{
									await (await this.device.WaitForElementBy(3, "Next", 10.0)).Click(false);
								}
								catch
								{
								}
								AuthUtils.VerifyEmailResult verifyEmailResult3 = await AuthUtils.VerifyEmail(anyMessageClient, this.device, this.phoneUID, LL);
								if (verifyEmailResult3 == AuthUtils.VerifyEmailResult.Success)
								{
									int j = 0;
									Element element2;
									for (;;)
									{
										num4 = j++;
										if (j > 15)
										{
											text2 = this.phoneUID;
											WorkUtils.WriteLog("[" + text2 + "]: UNKNOWN PAGE SOURCE: " + await this.device.GetSource());
											text2 = null;
											await tikTokUtils.GoHome(false);
											await tikTokUtils.TerminateTikTok();
											TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter7 = tikTokUtils.ActivateTikTokGotoProfile().GetAwaiter();
											if (!taskAwaiter7.IsCompleted)
											{
												await taskAwaiter7;
												taskAwaiter7 = taskAwaiter8;
												taskAwaiter8 = default(TaskAwaiter<TikTokUtils.ActivateResult>);
											}
											if (taskAwaiter7.GetResult() != TikTokUtils.ActivateResult.Success)
											{
												break;
											}
										}
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Source XML | SI4");
										await this.device.GetSourceXml();
										TaskAwaiter<bool> taskAwaiter3 = tikTokUtils.ActivateChecks().GetAwaiter();
										if (!taskAwaiter3.IsCompleted)
										{
											await taskAwaiter3;
											taskAwaiter3 = taskAwaiter4;
											taskAwaiter4 = default(TaskAwaiter<bool>);
										}
										if (!taskAwaiter3.GetResult())
										{
											if (this.device.IsElementInXmlContains("Maximum number of attempts reached", null))
											{
												goto Block_156;
											}
											if (this.device.IsElementInXml("Profile", null) || this.device.IsElementInXml("a11y_vo_profile", null))
											{
												WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Profile button E");
												element2 = null;
												if (this.device.IsElementInXml("Profile", null))
												{
													element2 = await this.device.WaitForElementBy(0, "Profile", 10.0);
												}
												else
												{
													if (!this.device.IsElementInXml("a11y_vo_profile", null))
													{
														text2 = this.phoneUID;
														WorkUtils.WriteLog("[" + text2 + "]: PROFILE BUTTON ERROR: " + await this.device.GetSource());
														text2 = null;
														continue;
													}
													element2 = await this.device.WaitForElementBy(0, "a11y_vo_profile", 10.0);
												}
												if (element2 != null)
												{
													bool flag4 = await element2.Visible();
													if (flag4)
													{
														flag4 = await element2.Enabled();
													}
													if (flag4)
													{
														goto Block_162;
													}
												}
											}
										}
									}
									return SignIn.SignResult.Unsuccess;
									Block_156:
									return SignIn.SignResult.Maximum;
									Block_162:
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Profile Button");
									await element2.Click(false);
									try
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Don't Allow Button");
										await (await this.device.WaitForElementBy(3, "t Allow", 3.0)).Click(false);
									}
									catch
									{
									}
									try
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Check for Ok Allert");
										await (await this.device.WaitForElementBy(0, "OK", 3.0)).Click(false);
									}
									catch
									{
									}
									try
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Save Password Alert || Close");
										await (await this.device.WaitForElementBy(0, "close", 3.0)).Click(false);
									}
									catch
									{
									}
									return SignIn.SignResult.Success;
								}
								switch (verifyEmailResult3)
								{
								case AuthUtils.VerifyEmailResult.TimeOut:
									return SignIn.SignResult.VerifyException;
								case AuthUtils.VerifyEmailResult.Exception:
									return SignIn.SignResult.VerifyException;
								case AuthUtils.VerifyEmailResult.Banned:
									return SignIn.SignResult.EmailBlocked;
								}
							}
							if (this.device.IsElementInXmlContains("Verify your email", null) || this.device.IsElementInXmlContains("Verify email", null) || this.device.IsElementInXml("Resend code", null) || this.device.IsElementInXmlContains("Send code", null) || this.device.IsElementInXmlContains("digit code", null) || this.device.IsElementInXml("Verify identity", null))
							{
								flag = false;
								int j = 0;
								try
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Need help logging Button");
									await (await this.device.WaitForElementBy(3, "Need help logging", 5.0)).Click(false);
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Password Button");
									await (await this.device.WaitForElementBy(0, "Choose an option to log in", 5.0)).TapAlert(0, 40, false);
								}
								catch
								{
									j = 1;
								}
								num4 = j;
								if (num4 == 1)
								{
									AuthUtils.VerifyEmailResult verifyEmailResult4 = await AuthUtils.VerifyEmail(anyMessageClient, this.device, this.phoneUID, LL);
									if (verifyEmailResult4 == AuthUtils.VerifyEmailResult.Success)
									{
										int num5 = 0;
										Element element2;
										for (;;)
										{
											num4 = num5++;
											if (num5 > 15)
											{
												text2 = this.phoneUID;
												WorkUtils.WriteLog("[" + text2 + "]: UNKNOWN PAGE SOURCE: " + await this.device.GetSource());
												text2 = null;
												await tikTokUtils.GoHome(false);
												await tikTokUtils.TerminateTikTok();
												TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter7 = tikTokUtils.ActivateTikTokGotoProfile().GetAwaiter();
												if (!taskAwaiter7.IsCompleted)
												{
													await taskAwaiter7;
													taskAwaiter7 = taskAwaiter8;
													taskAwaiter8 = default(TaskAwaiter<TikTokUtils.ActivateResult>);
												}
												if (taskAwaiter7.GetResult() != TikTokUtils.ActivateResult.Success)
												{
													break;
												}
											}
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Source XML | SI4");
											await this.device.GetSourceXml();
											TaskAwaiter<bool> taskAwaiter3 = tikTokUtils.ActivateChecks().GetAwaiter();
											if (!taskAwaiter3.IsCompleted)
											{
												await taskAwaiter3;
												taskAwaiter3 = taskAwaiter4;
												taskAwaiter4 = default(TaskAwaiter<bool>);
											}
											if (!taskAwaiter3.GetResult())
											{
												if (this.device.IsElementInXmlContains("Maximum number of attempts reached", null))
												{
													goto Block_180;
												}
												if (this.device.IsElementInXml("Profile", null) || this.device.IsElementInXml("a11y_vo_profile", null))
												{
													WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Profile button R");
													element2 = null;
													if (this.device.IsElementInXml("Profile", null))
													{
														element2 = await this.device.WaitForElementBy(0, "Profile", 10.0);
													}
													else
													{
														if (!this.device.IsElementInXml("a11y_vo_profile", null))
														{
															text2 = this.phoneUID;
															WorkUtils.WriteLog("[" + text2 + "]: PROFILE BUTTON ERROR: " + await this.device.GetSource());
															text2 = null;
															continue;
														}
														element2 = await this.device.WaitForElementBy(0, "a11y_vo_profile", 10.0);
													}
													if (element2 != null)
													{
														bool flag4 = await element2.Visible();
														if (flag4)
														{
															flag4 = await element2.Enabled();
														}
														if (flag4)
														{
															goto Block_186;
														}
													}
												}
											}
										}
										return SignIn.SignResult.Unsuccess;
										Block_180:
										return SignIn.SignResult.Maximum;
										Block_186:
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Profile Button");
										await element2.Click(false);
										try
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Don't Allow Button");
											await (await this.device.WaitForElementBy(3, "t Allow", 3.0)).Click(false);
										}
										catch
										{
										}
										try
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Check for Ok Allert");
											await (await this.device.WaitForElementBy(0, "OK", 3.0)).Click(false);
										}
										catch
										{
										}
										try
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Save Password Alert || Close");
											await (await this.device.WaitForElementBy(0, "close", 3.0)).Click(false);
										}
										catch
										{
										}
										return SignIn.SignResult.Success;
									}
									switch (verifyEmailResult4)
									{
									case AuthUtils.VerifyEmailResult.TimeOut:
										return SignIn.SignResult.VerifyException;
									case AuthUtils.VerifyEmailResult.Exception:
										return SignIn.SignResult.VerifyException;
									case AuthUtils.VerifyEmailResult.Banned:
										return SignIn.SignResult.EmailBlocked;
									}
								}
							}
							if (this.device.IsElementInXmlContains("already signed up", null))
							{
								flag = false;
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Log In With Password Button");
								await (await this.device.WaitForElementBy(3, "with password", 10.0)).Click(false);
							}
							if (this.device.IsElementInXmlContains("step verification", null) || this.device.IsElementInXml("Your code was sent to your authenticator app", null))
							{
								SignIn.Class15 @class = new SignIn.Class15();
								@class.Field0 = this;
								flag = false;
								text2 = await AuthUtils.FA2GetExistCode(username, this.device, this.phoneUID);
								if (text2 == null)
								{
									return SignIn.SignResult.FA2Exception;
								}
								await tikTokUtils.ActivateTikTok();
								await Task.Delay(TimeSpan.FromSeconds(2.0));
								List<Task> list3 = new List<Task>();
								@class.elements = new Dictionary<char, Element>();
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Code Elements for " + text2);
								string text4 = text2;
								for (int j = 0; j < text4.Length; j++)
								{
									SignIn.Class16 class2 = new SignIn.Class16();
									class2.Field0 = @class;
									class2.d = text4[j];
									list3.Add(Task.Run(new Func<Task>(class2.Method0)));
									await Task.Delay(200);
								}
								text4 = null;
								try
								{
									Task.WhenAll(list3).Wait(TimeSpan.FromSeconds(10.0));
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Elements found");
								}
								catch (TimeoutException)
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Elements not found || TimeOut");
								}
								catch (Exception ex3)
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Elements Exception: " + ex3.ToString());
								}
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Write Code: " + text2);
								foreach (char c in text2)
								{
									WorkUtils.WriteLog(string.Format("[{0}]: Click {1}", this.phoneUID, c));
									int num5 = 0;
									try
									{
										await @class.elements[c].Click(false);
									}
									catch (Exception obj)
									{
										num5 = 1;
									}
									num4 = num5;
									if (num4 == 1)
									{
										Exception ex4 = (Exception)obj;
										int num6 = 0;
										try
										{
											object obj2 = this.phoneUID;
											WorkUtils.WriteLog(string.Format("[{0}]: Checking session: {1}", obj2, await this.device.CheckSession()));
											obj2 = null;
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Appium 25 | SIN1");
											await this.device.ConfigurateAppium(25);
											WorkUtils.WriteLog(string.Format("[{0}]: Exception: Button not found, searching for element {1}", this.phoneUID, c));
											await (await this.device.WaitForElementBy(0, c.ToString(), 3.0)).Click(false);
										}
										catch
										{
											num6 = 1;
										}
										num4 = num6;
										if (num4 == 1)
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Exception: Button not found, tap by coords");
											string text5 = c.ToString();
											uint num7 = Class22.ComputeStringHash(text5);
											if (num7 <= 873244444U)
											{
												if (num7 <= 822911587U)
												{
													if (num7 != 806133968U)
													{
														if (num7 == 822911587U)
														{
															if (text5 == "4")
															{
																await this.device.Tap(67.5, 641.0);
															}
														}
													}
													else if (text5 == "5")
													{
														await this.device.Tap(196.5, 641.0);
													}
												}
												else if (num7 != 839689206U)
												{
													if (num7 != 856466825U)
													{
														if (num7 == 873244444U)
														{
															if (text5 == "1")
															{
																await this.device.Tap(67.5, 587.0);
															}
														}
													}
													else if (text5 == "6")
													{
														await this.device.Tap(325.5, 641.0);
													}
												}
												else if (text5 == "7")
												{
													await this.device.Tap(67.5, 695.0);
												}
											}
											else if (num7 <= 906799682U)
											{
												if (num7 != 890022063U)
												{
													if (num7 == 906799682U)
													{
														if (text5 == "3")
														{
															await this.device.Tap(325.5, 587.0);
														}
													}
												}
												else if (text5 == "0")
												{
													await this.device.Tap(196.5, 749.0);
												}
											}
											else if (num7 != 923577301U)
											{
												if (num7 != 1007465396U)
												{
													if (num7 == 1024243015U)
													{
														if (text5 == "8")
														{
															await this.device.Tap(196.5, 695.0);
														}
													}
												}
												else if (text5 == "9")
												{
													await this.device.Tap(325.5, 695.0);
												}
											}
											else if (text5 == "2")
											{
												await this.device.Tap(196.5, 587.0);
											}
										}
									}
									obj = null;
								}
								text4 = null;
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Continue Button");
								foreach (Element element2 in await this.device.WaitForElementsBy(0, "Continue", 10.0))
								{
									Element element2;
									try
									{
										TaskAwaiter<bool> taskAwaiter3 = element2.Visible().GetAwaiter();
										if (!taskAwaiter3.IsCompleted)
										{
											await taskAwaiter3;
											taskAwaiter3 = taskAwaiter4;
											taskAwaiter4 = default(TaskAwaiter<bool>);
										}
										if (taskAwaiter3.GetResult())
										{
											await element2.Click(false);
										}
									}
									catch
									{
									}
									element2 = null;
								}
								List<Element>.Enumerator enumerator = default(List<Element>.Enumerator);
								WorkUtils.WriteLog("[" + this.phoneUID + "]: Appium DEF | RL2");
								await this.device.ConfigurateAppium(17);
								num4 = 0;
								try
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Check for Errors");
									if (this.device.IsElementInXml("Enter a valid code", null))
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Back Button");
										await (await this.device.WaitForElementBy(0, "Back", 10.0)).Click(false);
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Back Button");
										await (await this.device.WaitForElementBy(0, "Go back", 10.0)).Click(false);
										goto IL_BB04;
									}
									int j = 0;
									Element element2;
									for (;;)
									{
										j++;
										if (j > 15)
										{
											text4 = this.phoneUID;
											WorkUtils.WriteLog("[" + text4 + "]: UNKNOWN PAGE SOURCE: " + await this.device.GetSource());
											text4 = null;
											await tikTokUtils.GoHome(false);
											await tikTokUtils.TerminateTikTok();
											TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter7 = tikTokUtils.ActivateTikTokGotoProfile().GetAwaiter();
											if (!taskAwaiter7.IsCompleted)
											{
												await taskAwaiter7;
												taskAwaiter7 = taskAwaiter8;
												taskAwaiter8 = default(TaskAwaiter<TikTokUtils.ActivateResult>);
											}
											if (taskAwaiter7.GetResult() != TikTokUtils.ActivateResult.Success)
											{
												break;
											}
										}
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Source XML | SI1");
										await this.device.GetSourceXml();
										TaskAwaiter<bool> taskAwaiter3 = tikTokUtils.ActivateChecks().GetAwaiter();
										if (!taskAwaiter3.IsCompleted)
										{
											await taskAwaiter3;
											taskAwaiter3 = taskAwaiter4;
											taskAwaiter4 = default(TaskAwaiter<bool>);
										}
										if (!taskAwaiter3.GetResult() && (this.device.IsElementInXml("Profile", null) || this.device.IsElementInXml("a11y_vo_profile", null)))
										{
											WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Profile button");
											element2 = null;
											if (this.device.IsElementInXml("Profile", null))
											{
												element2 = await this.device.WaitForElementBy(0, "Profile", 10.0);
											}
											else
											{
												if (!this.device.IsElementInXml("a11y_vo_profile", null))
												{
													text4 = this.phoneUID;
													WorkUtils.WriteLog("[" + text4 + "]: PROFILE BUTTON ERROR: " + await this.device.GetSource());
													text4 = null;
													continue;
												}
												element2 = await this.device.WaitForElementBy(0, "a11y_vo_profile", 10.0);
											}
											if (element2 != null)
											{
												bool flag4 = await element2.Visible();
												if (flag4)
												{
													flag4 = await element2.Enabled();
												}
												if (flag4)
												{
													goto Block_403;
												}
											}
										}
									}
									return SignIn.SignResult.Unsuccess;
									Block_403:
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Profile Button");
									await element2.Click(false);
									try
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Don't Allow Button");
										await (await this.device.WaitForElementBy(3, "t Allow", 3.0)).Click(false);
									}
									catch
									{
									}
									try
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Check for Ok Allert");
										await (await this.device.WaitForElementBy(0, "OK", 3.0)).Click(false);
									}
									catch
									{
									}
									try
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Save Password Alert || Close");
										await (await this.device.WaitForElementBy(0, "close", 3.0)).Click(false);
									}
									catch
									{
									}
									return SignIn.SignResult.Success;
								}
								catch (Exception obj3)
								{
									num4 = 1;
								}
								if (num4 == 1)
								{
									object obj3;
									Exception ex5 = (Exception)obj3;
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Exception AL: " + ex5.ToString());
									await tikTokUtils.GoHome(false);
									await tikTokUtils.TerminateTikTok();
									TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter7 = tikTokUtils.ActivateTikTokGotoProfile().GetAwaiter();
									if (!taskAwaiter7.IsCompleted)
									{
										await taskAwaiter7;
										taskAwaiter7 = taskAwaiter8;
										taskAwaiter8 = default(TaskAwaiter<TikTokUtils.ActivateResult>);
									}
									if (taskAwaiter7.GetResult() != TikTokUtils.ActivateResult.Success)
									{
										return SignIn.SignResult.Unsuccess;
									}
								}
								@class = null;
								text2 = null;
								list3 = null;
							}
							if (this.device.IsElementInXml("Profile", null) || this.device.IsElementInXml("a11y_vo_profile", null))
							{
								flag = false;
								Element element2;
								if (this.device.IsElementInXml("Profile", null))
								{
									element2 = await this.device.WaitForElementBy(0, "Profile", 10.0);
								}
								else
								{
									if (!this.device.IsElementInXml("a11y_vo_profile", null))
									{
										text2 = this.phoneUID;
										WorkUtils.WriteLog("[" + text2 + "]: PROFILE BUTTON ERROR: " + await this.device.GetSource());
										text2 = null;
										goto IL_BB04;
									}
									element2 = await this.device.WaitForElementBy(0, "a11y_vo_profile", 10.0);
								}
								if (element2 == null)
								{
									TaskAwaiter<bool> taskAwaiter3 = tikTokUtils.ActivateChecks().GetAwaiter();
									if (!taskAwaiter3.IsCompleted)
									{
										await taskAwaiter3;
										taskAwaiter3 = taskAwaiter4;
										taskAwaiter4 = default(TaskAwaiter<bool>);
									}
									if (taskAwaiter3.GetResult())
									{
										goto IL_BB04;
									}
								}
								bool flag4 = await element2.Visible();
								if (flag4)
								{
									flag4 = await element2.Enabled();
								}
								if (flag4)
								{
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Profile Button");
									await element2.Click(false);
									try
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Click Don't Allow Button");
										await (await this.device.WaitForElementBy(3, "t Allow", 3.0)).Click(false);
									}
									catch
									{
									}
									try
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Check for Ok Allert");
										await (await this.device.WaitForElementBy(0, "OK", 3.0)).Click(false);
									}
									catch
									{
									}
									try
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Checking for Save Password Alert || Close");
										await (await this.device.WaitForElementBy(0, "close", 3.0)).Click(false);
									}
									catch
									{
									}
									WorkUtils.WriteLog("[" + this.phoneUID + "]: Getting Source XML | SI02");
									await this.device.GetSourceXml();
									if (this.device.IsElementInXml("Log in to TikTok", null) || this.device.IsElementInXml("Add another account", null) || this.device.IsElementInXmlContains("have an account? Sign up", null) || this.device.IsElementInXml("Continue with Facebook", null) || this.device.IsElementInXmlNotVisible("Log into existing account", null))
									{
										WorkUtils.WriteLog("[" + this.phoneUID + "]: Configurate Appium 25 | DSA1Q");
										await this.device.ConfigurateAppium(25);
										goto IL_BB04;
									}
									return SignIn.SignResult.Success;
								}
								else
								{
									element2 = null;
								}
							}
							await tikTokUtils.ActivateChecks();
							if (flag)
							{
								await this.device.Tap(50.0, 50.0);
							}
							else
							{
								await Task.Delay(500);
							}
						}
						catch (Exception obj4)
						{
							num3 = 1;
						}
						goto IL_BA27;
						IL_BB04:
						num4 = i++;
						continue;
						IL_BA27:
						num4 = num3;
						object obj4;
						if (num4 == 1)
						{
							Exception ex6 = (Exception)obj4;
							WorkUtils.WriteLog("[" + this.phoneUID + "]: SI0 Exception: " + ex6.ToString());
							text2 = this.phoneUID;
							WorkUtils.WriteLog("[" + text2 + "]: SI0 Source Page: " + await this.device.GetSource());
							text2 = null;
						}
						obj4 = null;
						goto IL_BB04;
					}
					text2 = this.phoneUID;
					WorkUtils.WriteLog("[" + text2 + "]: SI0 DONE UNSUCCESS! Source Page: " + await this.device.GetSource());
					text2 = null;
					return SignIn.SignResult.Unsuccess;
				}
				catch (Exception obj5)
				{
					num2 = 1;
				}
				if (num2 != 1)
				{
					return signResult;
				}
				object obj5;
				Exception ex7 = (Exception)obj5;
				WorkUtils.WriteLog("[" + this.phoneUID + "]: Signing Exception: " + ex7.ToString());
				num4 = num++;
				if (num > 3)
				{
					break;
				}
				await tikTokUtils.TerminateTikTok();
			}
			signResult = SignIn.SignResult.Exception;
			return signResult;
		}

		// Token: 0x04000328 RID: 808
		public iDevice device;

		// Token: 0x04000329 RID: 809
		public string phoneUID = "";

		// Token: 0x02000063 RID: 99
		public enum SignResult
		{
			// Token: 0x0400032B RID: 811
			Success,
			// Token: 0x0400032C RID: 812
			Unsuccess,
			// Token: 0x0400032D RID: 813
			Exception,
			// Token: 0x0400032E RID: 814
			CaptchaError,
			// Token: 0x0400032F RID: 815
			Maximum,
			// Token: 0x04000330 RID: 816
			Blocked,
			// Token: 0x04000331 RID: 817
			VerifyException,
			// Token: 0x04000332 RID: 818
			FA2Exception,
			// Token: 0x04000333 RID: 819
			EmailBlocked,
			// Token: 0x04000334 RID: 820
			TimeOut
		}

		// Token: 0x02000064 RID: 100
		[CompilerGenerated]
		private sealed class Class15
		{
			// Token: 0x04000335 RID: 821
			public Dictionary<char, Element> elements;

			// Token: 0x04000336 RID: 822
			public SignIn Field0;
		}

		// Token: 0x02000065 RID: 101
		[CompilerGenerated]
		private sealed class Class16
		{
			// Token: 0x06000100 RID: 256 RVA: 0x0003E2F8 File Offset: 0x0003C4F8
			internal async Task Method0()
			{
				try
				{
					char c = this.d;
					Element element = await this.Field0.Field0.device.WaitForElementBy(0, c.ToString(), 10.0);
					this.Field0.elements.Add(c, element);
				}
				catch
				{
				}
			}

			// Token: 0x04000337 RID: 823
			public char d;

			// Token: 0x04000338 RID: 824
			public SignIn.Class15 Field0;

			// Token: 0x02000066 RID: 102
			[StructLayout(LayoutKind.Auto)]
			private struct Struct62 : IAsyncStateMachine
			{
				// Token: 0x06000101 RID: 257 RVA: 0x0003E33C File Offset: 0x0003C53C
				void IAsyncStateMachine.MoveNext()
				{
					int num2;
					int num = num2;
					SignIn.Class16 @class = this;
					try
					{
						try
						{
							TaskAwaiter<Element> taskAwaiter;
							if (num != 0)
							{
								c = @class.d;
								taskAwaiter = @class.Field0.Field0.device.WaitForElementBy(0, c.ToString(), 10.0).GetAwaiter();
								if (!taskAwaiter.IsCompleted)
								{
									num2 = 0;
									TaskAwaiter<Element> taskAwaiter2 = taskAwaiter;
									this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter<Element>, SignIn.Class16.Struct62>(ref taskAwaiter, ref this);
									return;
								}
							}
							else
							{
								TaskAwaiter<Element> taskAwaiter2;
								taskAwaiter = taskAwaiter2;
								taskAwaiter2 = default(TaskAwaiter<Element>);
								num2 = -1;
							}
							Element result = taskAwaiter.GetResult();
							@class.Field0.elements.Add(c, result);
						}
						catch
						{
						}
					}
					catch (Exception ex)
					{
						num2 = -2;
						this.Field1.SetException(ex);
						return;
					}
					num2 = -2;
					this.Field1.SetResult();
				}

				// Token: 0x06000102 RID: 258 RVA: 0x000025DB File Offset: 0x000007DB
				[DebuggerHidden]
				void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
				{
					this.Field1.SetStateMachine(stateMachine);
				}

				// Token: 0x04000339 RID: 825
				public int Field0;

				// Token: 0x0400033A RID: 826
				public AsyncTaskMethodBuilder Field1;

				// Token: 0x0400033B RID: 827
				public SignIn.Class16 Field2;

				// Token: 0x0400033C RID: 828
				private char Field3;

				// Token: 0x0400033D RID: 829
				private TaskAwaiter<Element> Field4;
			}
		}
	}
}
