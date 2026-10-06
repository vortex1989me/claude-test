using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TikTok_Automation_Library_Non_Jail.Utils.TempMail;
using WDA_Framework;
using WDA_Framework.Utils;

namespace TikTok_Automation_Library_Non_Jail.Work.Utils
{
	// Token: 0x02000047 RID: 71
	internal class AuthUtils
	{
		// Token: 0x060000BF RID: 191 RVA: 0x0002BF40 File Offset: 0x0002A140
		public static async Task<AuthUtils.VerifyEmailResult> VerifyEmail(AnyMessageClient anyMessageClient, iDevice device, string phoneUID, bool LL)
		{
			int num = 0;
			try
			{
				int num2 = 0;
				bool flag = false;
				Exception ex;
				for (;;)
				{
					WorkUtils.WriteLog("[" + phoneUID + "]: Getting Source XML | VE1");
					await device.GetSourceXml();
					WorkUtils.WriteLog("[" + phoneUID + "]: Checking for Verify your email message");
					if (!device.IsElementInXml("Verify your email", null) && !device.IsElementInXml("Verify email", null) && !device.IsElementInXml("Resend code", null) && !device.IsElementInXmlContains("Send code", null) && !device.IsElementInXmlContains("digit code", null) && !device.IsElementInXml("Verify identity", null))
					{
						goto IL_0AD8;
					}
					if (device.IsElementInXml("Save login info", null))
					{
						foreach (Element element in await device.WaitForElementsBy(0, "Save login info", 3.0))
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
										WorkUtils.WriteLog("[" + phoneUID + "]: Click Save login info Button");
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
					if (device.IsElementInXml("Verify identity", null))
					{
						int num3 = 0;
						try
						{
							WorkUtils.WriteLog("[" + phoneUID + "]: Click Send Code Button");
							await (await device.WaitForElementBy(3, "Send code", 3.0)).TapAlert(0, -20, false);
						}
						catch
						{
							num3 = 1;
						}
						if (num3 == 1)
						{
							int num4 = 0;
							try
							{
								WorkUtils.WriteLog("[" + phoneUID + "]: Click Resend Code Button");
								await (await device.WaitForElementBy(0, "Resend code", 3.0)).TapAlert(0, -20, false);
							}
							catch (Exception obj)
							{
								num4 = 1;
							}
							if (num4 == 1)
							{
								object obj;
								ex = (Exception)obj;
								num2++;
								if (num2 > 3)
								{
									break;
								}
								continue;
							}
							else
							{
								object obj = null;
							}
						}
					}
					DateTime dateTime = DateTime.Now.AddSeconds(70.0);
					while (DateTime.Now < dateTime)
					{
						WorkUtils.WriteLog("[" + phoneUID + "]: Waiting for code");
						try
						{
							AnyMessageClient.MessageData messageData = await anyMessageClient.GetMessageAsync(anyMessageClient.emailDataC.id, LL, false);
							if (messageData.value != null)
							{
								if (messageData.value != "wait message" && messageData.message != null)
								{
									await device.SendKeys(messageData.value);
									WorkUtils.WriteLog("[" + phoneUID + "]: Email Confirm Done!");
									await device.ConfigurateAppium(17);
									await Task.Delay(TimeSpan.FromSeconds(5.0));
									return AuthUtils.VerifyEmailResult.Success;
								}
								WorkUtils.WriteLog("[" + phoneUID + "]: Status: wait message");
							}
							else
							{
								WorkUtils.WriteLog("[" + phoneUID + "]: Don't have messages");
							}
						}
						catch (Exception ex2) when (ex2.Message.Contains("wait message"))
						{
							WorkUtils.WriteLog("[" + phoneUID + "]: Any Message Status: Wait Message...");
						}
						catch (Exception ex3) when (ex3.Message.Contains("email banned"))
						{
							WorkUtils.WriteLog("[" + phoneUID + "]: Any Message Status: Email Banned!");
							return AuthUtils.VerifyEmailResult.Banned;
						}
						catch (Exception ex4)
						{
							WorkUtils.WriteLog("[" + phoneUID + "]: Any Message Exception: " + ex4.ToString());
						}
						await Task.Delay(TimeSpan.FromSeconds(5.0));
					}
					TaskAwaiter<Element> taskAwaiter5 = device.WaitForElementBy(3, "was logged out", 3.0).GetAwaiter();
					if (!taskAwaiter5.IsCompleted)
					{
						await taskAwaiter5;
						TaskAwaiter<Element> taskAwaiter6;
						taskAwaiter5 = taskAwaiter6;
						taskAwaiter6 = default(TaskAwaiter<Element>);
					}
					if (taskAwaiter5.GetResult() != null)
					{
						try
						{
							WorkUtils.WriteLog("[" + phoneUID + "]: Logged out | Click OK Button");
							await (await device.WaitForElementBy(0, "OK", 3.0)).Click(false);
							return AuthUtils.VerifyEmailResult.TimeOut;
						}
						catch
						{
							goto IL_0D6A;
						}
					}
					if (flag)
					{
						goto IL_0D63;
					}
					flag = true;
					WorkUtils.WriteLog("[" + phoneUID + "]: Click Resend Button");
					await (await device.WaitForElementBy(0, "Resend code", 3.0)).Click(false);
				}
				string text = phoneUID;
				WorkUtils.WriteLog("[" + text + "]: AU VI Exception Source: " + await device.GetSource());
				text = null;
				throw ex;
				IL_0AD8:
				return AuthUtils.VerifyEmailResult.Unsuccess;
				IL_0D63:
				return AuthUtils.VerifyEmailResult.TimeOut;
				IL_0D6A:;
			}
			catch (Exception obj4)
			{
				num = 1;
			}
			AuthUtils.VerifyEmailResult verifyEmailResult;
			if (num == 1)
			{
				object obj4;
				WorkUtils.WriteLog("[" + phoneUID + "]: Verify Email Exception: " + ((Exception)obj4).ToString());
				try
				{
					string text = phoneUID;
					WorkUtils.WriteLog("[" + text + "]: SOURCE CODE: " + await device.GetSource());
					text = null;
				}
				catch
				{
				}
				verifyEmailResult = AuthUtils.VerifyEmailResult.Exception;
			}
			else
			{
				object obj4 = null;
				verifyEmailResult = AuthUtils.VerifyEmailResult.TimeOut;
			}
			return verifyEmailResult;
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x0002BF9C File Offset: 0x0002A19C
		public static async Task<string> FA2GetExistCode(string phoneNumber, iDevice device, string phoneUID)
		{
			string text2;
			for (;;)
			{
				try
				{
					LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Terminate Authenticator");
					await device.TerminateApp("com.google.Authenticator");
					LogsUtils.WriteLog("[" + phoneUID + "]: Click Home Button");
					await device.Press("home");
					await Task.Delay(TimeSpan.FromSeconds(1.0));
					await device.Press("home");
					await Task.Delay(TimeSpan.FromSeconds(1.0));
					LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Activate Authenticator");
					await device.ActivateApp("com.google.Authenticator");
					await Task.Delay(TimeSpan.FromSeconds(1.0));
					bool flag = false;
					Element element;
					for (;;)
					{
						LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Searching for Code");
						element = await device.WaitForElementBy(5, phoneNumber + ", ", 3.0);
						if (element != null)
						{
							break;
						}
						if (flag)
						{
							goto IL_054E;
						}
						flag = true;
						LogsUtils.WriteLog(string.Concat(new string[] { "[iDevice - ", phoneUID, "]: Write: ", phoneNumber, " to SearchBox" }));
						await (await device.WaitForElementBy(3, "Search", 5.0)).SetText(phoneNumber);
					}
					string text;
					for (;;)
					{
						text = await element.Label();
						if (text.Contains("26 seconds") || text.Contains("27 seconds") || text.Contains("28 seconds"))
						{
							break;
						}
						LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Waiting for secret code update");
						await Task.Delay(500);
					}
					text = text.Split(new char[] { ',' })[1].Trim().Replace(" ", "");
					LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Code is " + text);
					text2 = text;
					break;
					IL_054E:
					text2 = null;
				}
				catch (Exception ex)
				{
					LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Exception: " + ex.ToString());
					continue;
				}
				break;
			}
			return text2;
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x0002BFF0 File Offset: 0x0002A1F0
		public static async Task FA2RemoveExistAccount(string phoneNumber, iDevice device, string phoneUID)
		{
			for (;;)
			{
				try
				{
					LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Terminate Authenticator");
					await device.TerminateApp("com.google.Authenticator");
					LogsUtils.WriteLog("[" + phoneUID + "]: Click Home Button");
					await device.Press("home");
					await Task.Delay(TimeSpan.FromSeconds(1.0));
					await device.Press("home");
					await Task.Delay(TimeSpan.FromSeconds(1.0));
					LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Activate Authenticator");
					await device.ActivateApp("com.google.Authenticator");
					await Task.Delay(TimeSpan.FromSeconds(1.0));
					int num = 0;
					bool flag = false;
					bool flag2 = false;
					Element element;
					for (;;)
					{
						LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Searching for Code");
						element = await device.WaitForElementBy(5, phoneNumber + ", ", 3.0);
						if (element == null)
						{
							if (!flag2)
							{
								flag2 = true;
								LogsUtils.WriteLog(string.Concat(new string[] { "[iDevice - ", phoneUID, "]: Write: ", phoneNumber, " to SearchBox" }));
								await (await device.WaitForElementBy(3, "Search", 5.0)).SetText(phoneNumber);
							}
							else
							{
								if (!flag)
								{
									break;
								}
								num++;
								LogsUtils.WriteLog("[" + phoneUID + "]: Swipe");
								await device.Swipe(150, 600, 150, 400, 0.0);
							}
						}
						else
						{
							TaskAwaiter<bool> taskAwaiter = element.Visible().GetAwaiter();
							if (!taskAwaiter.IsCompleted)
							{
								await taskAwaiter;
								TaskAwaiter<bool> taskAwaiter2;
								taskAwaiter = taskAwaiter2;
								taskAwaiter2 = default(TaskAwaiter<bool>);
							}
							if (!taskAwaiter.GetResult())
							{
								LogsUtils.WriteLog("[" + phoneUID + "]: Swipe");
								await device.Swipe(150, 600, 150, 400, 0.0);
							}
							else
							{
								if (!flag2 || flag)
								{
									goto IL_0828;
								}
								await (await device.WaitForElementBy(0, "Back", 10.0)).Click(false);
								flag = true;
							}
						}
					}
					break;
					IL_0828:
					Position position = await element.GetPosition();
					LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Swipe");
					await device.Swipe(200, (int)position.y, 0, (int)position.y, 0.0);
					LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Click Remove Account Button");
					await (await device.WaitForElementBy(0, "Remove account", 10.0)).Click(false);
					await Task.Delay(TimeSpan.FromSeconds(1.0));
					await (await device.WaitForElementBy(0, "Remove account", 10.0)).Click(false);
					element = null;
				}
				catch (Exception ex)
				{
					LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Exception: " + ex.ToString());
					continue;
				}
				break;
			}
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x0002C044 File Offset: 0x0002A244
		public static async Task<string> FA2AddNewAccountAndGetCode(string phoneNumber, string code, iDevice device, string phoneUID)
		{
			string text2;
			for (;;)
			{
				try
				{
					LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Terminate Authenticator");
					await device.TerminateApp("com.google.Authenticator");
					LogsUtils.WriteLog("[" + phoneUID + "]: Click Home Button");
					await device.Press("home");
					await Task.Delay(TimeSpan.FromSeconds(1.0));
					await device.Press("home");
					await Task.Delay(TimeSpan.FromSeconds(1.0));
					LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Activate Authenticator");
					await device.ActivateApp("com.google.Authenticator");
					LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Click Add Button");
					int num = 0;
					try
					{
						await (await device.WaitForElementBy(0, "Add", 10.0)).TapAlert(0, 0, false);
					}
					catch
					{
						num = 1;
					}
					if (num == 1)
					{
						await (await device.WaitForElementBy(0, "Add a code", 10.0)).TapAlert(0, 0, false);
					}
					LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Click Enter a setup key");
					await (await device.WaitForElementBy(0, "Enter a setup key", 10.0)).TapAlert(0, 0, false);
					LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Searching for input boxes");
					List<Element> list = await device.WaitForElementsByXPath("XCUIElementTypeTextField", 2, "", 10.0);
					Element element = list[0];
					Element element2 = list[1];
					LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Write Name " + phoneNumber);
					await element.SetText(phoneNumber);
					LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Write Code " + code);
					await element2.SetText(code);
					await Task.Delay(TimeSpan.FromSeconds(1.0));
					LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Click Add Button");
					await (await device.WaitForElementBy(0, "Add", 10.0)).TapAlert(0, 0, false);
					await Task.Delay(TimeSpan.FromSeconds(1.0));
					bool flag = false;
					Element element3;
					for (;;)
					{
						LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Searching for Code");
						element3 = await device.WaitForElementBy(5, phoneNumber + ", ", 3.0);
						if (element3 != null)
						{
							break;
						}
						if (flag)
						{
							goto IL_0B75;
						}
						flag = true;
						LogsUtils.WriteLog(string.Concat(new string[] { "[iDevice - ", phoneUID, "]: Write: ", phoneNumber, " to SearchBox" }));
						await (await device.WaitForElementBy(3, "Search", 5.0)).SetText(phoneNumber);
					}
					string text;
					for (;;)
					{
						text = await element3.Label();
						if (text.Contains("27 seconds") || text.Contains("26 seconds") || text.Contains("28 seconds"))
						{
							break;
						}
						LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Waiting for secret code update");
						await Task.Delay(500);
					}
					text = text.Split(new char[] { ',' })[1].Trim().Replace(" ", "");
					LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Code is " + text);
					text2 = text;
					break;
					IL_0B75:
					text2 = null;
				}
				catch (Exception ex)
				{
					LogsUtils.WriteLog("[iDevice - " + phoneUID + "]: Exception: " + ex.ToString());
					continue;
				}
				break;
			}
			return text2;
		}

		// Token: 0x02000048 RID: 72
		public enum VerifyEmailResult
		{
			// Token: 0x04000243 RID: 579
			Success,
			// Token: 0x04000244 RID: 580
			Unsuccess,
			// Token: 0x04000245 RID: 581
			TimeOut,
			// Token: 0x04000246 RID: 582
			Exception,
			// Token: 0x04000247 RID: 583
			Banned
		}
	}
}
