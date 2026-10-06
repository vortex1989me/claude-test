using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using TikTok_Automation_Library_Non_Jail.Forms;
using TikTok_Automation_Library_Non_Jail.Properties;
using TikTok_Automation_Library_Non_Jail.Utils;
using TikTok_Automation_Library_Non_Jail.Work.Utils;
using WDA_Framework;

namespace TikTok_Automation_Library_Non_Jail.Work
{
	// Token: 0x02000018 RID: 24
	internal class VPN
	{
		// Token: 0x06000047 RID: 71 RVA: 0x0000224E File Offset: 0x0000044E
		public VPN(VPN.VPNType vpnType, VPN.ProxyType proxyType)
		{
			this.type = vpnType;
			this.prxType = proxyType;
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00004C14 File Offset: 0x00002E14
		private async Task LoginPia(iDevice _device)
		{
			string[] array = Settings.Default.PiaData.Split(new char[] { ':' });
			string text = array[0];
			string text2 = array[1];
			LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click LOGIN Button");
			await (await this.device.WaitForElementBy(0, "id.login.submit.new", 10.0)).Click(false);
			LogsUtils.WriteLog("[" + _device.phoneUID + "]: Write Username: " + text);
			await (await this.device.WaitForElementBy(0, "id.login.username", 10.0)).SetText(text);
			LogsUtils.WriteLog("[" + _device.phoneUID + "]: Write Password: " + text2);
			await (await this.device.WaitForElementByXPath("XCUIElementTypeSecureTextField", 0, "id.login.submit", 10.0)).SetText(text2);
			LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click LOGIN Button");
			await (await this.device.WaitForElementBy(2, "LOGIN", 10.0)).Click(false);
			LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click Ok Button");
			await (await this.device.WaitForElementBy(0, "id.vpnPermission.ok.button", 10.0)).Click(false);
			try
			{
				LogsUtils.WriteLog("[" + _device.phoneUID + "]: Checking for Allow Button");
				await (await this.device.WaitForElementBy(0, "Allow", 5.0)).Click(false);
			}
			catch
			{
			}
		}

		// Token: 0x06000049 RID: 73 RVA: 0x00004C60 File Offset: 0x00002E60
		private async Task<bool> RemovePiaCert(iDevice _device)
		{
			try
			{
				LogsUtils.WriteLog("[" + _device.phoneUID + "]: Turn Off Preferences");
				await this.device.TerminateApp("com.apple.Preferences");
				LogsUtils.WriteLog("[" + _device.phoneUID + "]: Turn On Preferences");
				await this.device.ActivateApp("com.apple.Preferences");
				await Task.Delay(TimeSpan.FromSeconds(1.0));
				LogsUtils.WriteLog("[" + _device.phoneUID + "]: Swipe");
				await this.device.Swipe(150, 500, 150, 400, 0.0);
				await Task.Delay(TimeSpan.FromSeconds(1.0));
				LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click VPN Button");
				await (await this.device.WaitForElementBy(0, "VPN", 10.0)).Click(false);
				LogsUtils.WriteLog("[" + _device.phoneUID + "]: Getting More Info Elements");
				List<Element> list = await this.device.WaitForElementsBy(0, "More Info", 10.0);
				foreach (Element element in list)
				{
					LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click to More Info Element");
					await element.Click(false);
					LogsUtils.WriteLog("[" + _device.phoneUID + "]: Checking for PIA VPN");
					TaskAwaiter<Element> taskAwaiter = this.device.WaitForElementBy(0, "PIA VPN", 3.0).GetAwaiter();
					if (!taskAwaiter.IsCompleted)
					{
						await taskAwaiter;
						TaskAwaiter<Element> taskAwaiter2;
						taskAwaiter = taskAwaiter2;
						taskAwaiter2 = default(TaskAwaiter<Element>);
					}
					if (taskAwaiter.GetResult() != null)
					{
						LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click Delete VPN Button");
						await (await this.device.WaitForElementBy(0, "Delete VPN", 10.0)).Click(false);
						LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click Delete Button");
						await (await this.device.WaitForElementBy(0, "Delete", 10.0)).Click(false);
						try
						{
							LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click Back Button");
							await (await this.device.WaitForElementBy(0, "BackButton", 10.0)).Click(false);
						}
						catch
						{
						}
						return true;
					}
					try
					{
						LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click Back Butto NF");
						await (await this.device.WaitForElementBy(0, "BackButton", 10.0)).Click(false);
					}
					catch
					{
					}
				}
				List<Element>.Enumerator enumerator = default(List<Element>.Enumerator);
				try
				{
					LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click Back Button NF 2");
					await (await this.device.WaitForElementBy(0, "BackButton", 3.0)).Click(false);
				}
				catch
				{
				}
			}
			catch
			{
			}
			return false;
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00004CAC File Offset: 0x00002EAC
		public async Task<bool> TurnOnVpn(iDevice _device, string rega)
		{
			string text = rega;
			if (rega.Contains(":US"))
			{
				text = rega.Replace(":US", "");
			}
			this.device = _device;
			if (this.type == VPN.VPNType.BelkaVPN)
			{
				await this.device.ConfigurateAppium(25);
				int num = 0;
				for (;;)
				{
					IL_020A:
					try
					{
						num++;
						if (text == "United States")
						{
							text = "US ";
						}
						text = ((text == "US ") ? "US Alabama" : (text ?? ""));
						LogsUtils.WriteLog("[" + _device.phoneUID + "]: Terminate BelkaVPN");
						await this.device.TerminateApp("com.digiapp.belkavpn");
						LogsUtils.WriteLog("[" + _device.phoneUID + "]: Activate BelkaVPN");
						await this.device.ActivateApp("com.digiapp.belkavpn");
						DateTime dateTime = DateTime.Now.AddSeconds(20.0);
						while (DateTime.Now < dateTime)
						{
							LogsUtils.WriteLog("[" + _device.phoneUID + "]: Checking for elements");
							await this.device.GetSourceXml();
							if (this.device.IsElementInXmlContains("switch_on", null))
							{
								LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click Switch_on Button");
								await (await this.device.WaitForElementBy(0, "switch_on", 5.0)).Click(false);
								await Task.Delay(TimeSpan.FromSeconds(2.0));
							}
							else if (this.device.IsElementInXmlContains("switch_off", null))
							{
								if (this.device.IsElementInXmlContains(text, null))
								{
									LogsUtils.WriteLog(string.Concat(new string[] { "[", _device.phoneUID, "]: Region: ", text, " Already Selected || Click Connect Button" }));
									await (await this.device.WaitForElementBy(0, "switch_off", 5.0)).Click(false);
									await Task.Delay(TimeSpan.FromSeconds(2.0));
									this.IsActivated = true;
									return true;
								}
								LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click Right Button");
								await (await this.device.WaitForElementBy(0, "chevron.right", 10.0)).Click(false);
								LogsUtils.WriteLog("[" + _device.phoneUID + "]: Searching for TextBox");
								Element element = await this.device.WaitForElementBy(0, "Search Location", 10.0);
								LogsUtils.WriteLog("[" + _device.phoneUID + "]: Write Region: " + text);
								await element.SetText(text);
								await Task.Delay(TimeSpan.FromSeconds(1.0));
								LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click Right Button");
								Element element2 = await this.device.WaitForElementBy(3, text + ",", 3.0);
								if (element2 == null)
								{
									element2 = await this.device.WaitForElementBy(3, text + " ", 3.0);
								}
								if (element2 == null)
								{
									element2 = await this.device.WaitForElementBy(3, text ?? "", 10.0);
								}
								await element2.Click(false);
								dateTime = DateTime.Now.AddSeconds(10.0);
								while (DateTime.Now < dateTime)
								{
									await this.device.GetSourceXml();
									if (this.device.IsElementInXmlContains("switch_on", null))
									{
										LogsUtils.WriteLog("[" + _device.phoneUID + "]: Connected OK");
										this.IsActivated = true;
										return true;
									}
								}
								LogsUtils.WriteLog("[" + _device.phoneUID + "]: Not Connected");
								goto IL_020A;
							}
							else
							{
								await Task.Delay(TimeSpan.FromSeconds(1.0));
							}
						}
						LogsUtils.WriteLog("[" + _device.phoneUID + "]: Not Connected E");
						continue;
					}
					catch (Exception ex)
					{
						LogsUtils.WriteLog("[" + _device.phoneUID + "]: Exception: " + ex.ToString());
						continue;
					}
					break;
				}
			}
			bool flag;
			if (this.type == VPN.VPNType.PiaVpn)
			{
				await this.device.ConfigurateAppium(25);
				int num = 0;
				for (;;)
				{
					num++;
					if (text == "United States")
					{
						text = "US ";
					}
					LogsUtils.WriteLog("[" + _device.phoneUID + "]: Terminate Pia VPN");
					await this.device.TerminateApp("com.privateinternetaccess.ios.PIA-VPN");
					LogsUtils.WriteLog("[" + _device.phoneUID + "]: Activate Pia VPN");
					await this.device.ActivateApp("com.privateinternetaccess.ios.PIA-VPN");
					for (;;)
					{
						IL_0FA3:
						LogsUtils.WriteLog("[" + _device.phoneUID + "]: Searching for Connection Button");
						Element element3 = await this.device.WaitForElementBy(0, "id.dashboard.connection.button", 10.0);
						if (element3 != null)
						{
							string text2 = await element3.Label();
							LogsUtils.WriteLog("[" + _device.phoneUID + "]: Connection Label: " + text2);
							if (!text2.Contains("disconnected"))
							{
								LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click Disconnect VPN Button");
								await element3.Click(false);
								await Task.Delay(TimeSpan.FromSeconds(3.0));
							}
							LogsUtils.WriteLog("[" + _device.phoneUID + "]: Checking for region");
							List<Element> list = await this.device.WaitForElementsBy(5, text, 3.0);
							if (list != null && list.Count >= 3)
							{
								element3 = await this.device.WaitForElementBy(0, "id.dashboard.connection.button", 10.0);
								text2 = await element3.Label();
								LogsUtils.WriteLog("[" + _device.phoneUID + "]: Connection Label: " + text2);
								if (text2.Contains("disconnected"))
								{
									LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click Connection Button");
									await element3.Click(false);
								}
							}
							else
							{
								LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click VPN SERVER Button");
								await (await this.device.WaitForElementBy(5, "VPN SERVER", 10.0)).Click(false);
								await Task.Delay(TimeSpan.FromSeconds(2.0));
								LogsUtils.WriteLog("[" + _device.phoneUID + "]: Swipe");
								await this.device.Swipe(150, 150, 150, 800, 0.0);
								int num2 = 0;
								Element element4;
								for (;;)
								{
									num2++;
									if (num2 > 10)
									{
										break;
									}
									LogsUtils.WriteLog("[" + _device.phoneUID + "]: Searching for Region TextBox");
									element4 = await this.device.WaitForElementBy(1, "Search for a region", 4.0);
									if (element4 == null)
									{
										LogsUtils.WriteLog("[" + _device.phoneUID + "]: Swipe");
										await this.device.Swipe(150, 150, 150, 800, 0.0);
									}
									else
									{
										TaskAwaiter<bool> taskAwaiter = element4.Visible().GetAwaiter();
										if (!taskAwaiter.IsCompleted)
										{
											await taskAwaiter;
											TaskAwaiter<bool> taskAwaiter2;
											taskAwaiter = taskAwaiter2;
											taskAwaiter2 = default(TaskAwaiter<bool>);
										}
										if (taskAwaiter.GetResult())
										{
											goto IL_1F43;
										}
										LogsUtils.WriteLog("[" + _device.phoneUID + "]: Swipe");
										await this.device.Swipe(150, 150, 150, 800, 0.0);
									}
								}
								LogsUtils.WriteLog("[" + _device.phoneUID + "]: Region TextBox NOT FOUND");
								await this.device.GetSourceXml();
								if (this.device.IsElementInXmlContains("You can try selecting a different", null) || this.device.IsElementInXmlContains("region or letting us", null))
								{
									LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click CLOSE Button");
									await (await this.device.WaitForElementBy(0, "CLOSE", 10.0)).Click(false);
									LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click Menu Button");
									await (await this.device.WaitForElementBy(0, "id.dashboard.menu", 10.0)).Click(false);
									LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click Log out Button");
									LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click Menu Button");
									this.device.WaitForElementBy(0, "id.dashboard.menu", 10.0).ConfigureAwait(false).GetAwaiter()
										.GetResult()
										.Click(false)
										.ConfigureAwait(false)
										.GetAwaiter()
										.GetResult();
									await (await this.device.WaitForElementBy(0, "Log out", 10.0)).Click(false);
									LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click LOGOUT Button");
									await (await this.device.WaitForElementBy(0, "id.dialog.destructive.button", 10.0)).Click(false);
									await this.RemovePiaCert(_device);
									LogsUtils.WriteLog("[" + _device.phoneUID + "]: Activate Pia VPN");
									await this.device.ActivateApp("com.privateinternetaccess.ios.PIA-VPN");
									await this.LoginPia(_device);
									continue;
								}
								goto IL_1CD9;
								IL_1F43:
								LogsUtils.WriteLog("[" + _device.phoneUID + "]: Write Region: " + text);
								await element4.SetText(text);
								await Task.Delay(TimeSpan.FromSeconds(1.0));
								LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click on Region VPN: " + text);
								string text3 = ((text == "US ") ? "US Alabama" : (text + ","));
								int num3 = 0;
								try
								{
									await (await this.device.WaitForElementBy(5, text3, 10.0)).Click(false);
								}
								catch
								{
									num3 = 1;
								}
								if (num3 == 1)
								{
									try
									{
										await (await this.device.WaitForElementBy(5, text, 10.0)).Click(false);
									}
									catch
									{
									}
								}
							}
							DateTime dateTime = DateTime.Now.AddSeconds(15.0);
							while (DateTime.Now < dateTime)
							{
								LogsUtils.WriteLog("[" + _device.phoneUID + "]: Getting Source XML | VPN1");
								await this.device.GetSourceXml();
								if (this.device.IsElementInXml("Connecting...", null) || this.device.IsElementInXml("Still trying to connect...", null) || this.device.IsElementInXml("Please wait...", null))
								{
									LogsUtils.WriteLog("[" + _device.phoneUID + "]: VPN Connecting....");
									await Task.Delay(500);
								}
								else
								{
									if (this.device.IsElementInXmlContains("Protected |", null))
									{
										goto Block_21;
									}
									if (this.device.IsElementInXml("Your automation settings are configured to keep the VPN disconnected under the current network conditions.", null) || this.device.IsElementInXmlContains("Your automation settings are configured to keep", null))
									{
										LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click DISABLE AUTOMATION Button");
										await (await this.device.WaitForElementBy(0, "DISABLE AUTOMATION", 10.0)).Click(false);
										await Task.Delay(1000);
									}
									else
									{
										if (this.device.IsElementInXmlContains("You can try selecting a different", null) || this.device.IsElementInXmlContains("region or letting us", null))
										{
											LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click CLOSE Button");
											await (await this.device.WaitForElementBy(0, "CLOSE", 10.0)).Click(false);
											string[] array = Settings.Default.PiaData.Split(new char[] { ':' });
											string text4 = array[0];
											string text5 = array[1];
											LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click Menu Button");
											await (await this.device.WaitForElementBy(0, "id.dashboard.menu", 10.0)).Click(false);
											LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click Log out Button");
											await (await this.device.WaitForElementBy(0, "Log out", 10.0)).Click(false);
											LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click LOGOUT Button");
											await (await this.device.WaitForElementBy(0, "id.dialog.destructive.button", 10.0)).Click(false);
											await this.RemovePiaCert(_device);
											LogsUtils.WriteLog("[" + _device.phoneUID + "]: Activate Pia VPN");
											await this.device.ActivateApp("com.privateinternetaccess.ios.PIA-VPN");
											await this.LoginPia(_device);
											goto IL_0FA3;
										}
										if (this.device.IsElementInXmlContains("The connection couldn", null) || this.device.IsElementInXml("Not Protected", null))
										{
											break;
										}
									}
								}
							}
							break;
						}
						await this.LoginPia(_device);
					}
					LogsUtils.WriteLog("[" + _device.phoneUID + "]: VPN Connection Exception");
					this.IsActivated = false;
					if (num >= 2)
					{
						goto Block_25;
					}
				}
				IL_1CD9:
				this.IsActivated = false;
				return false;
				Block_21:
				LogsUtils.WriteLog("[" + _device.phoneUID + "]: VPN CONNECTED! C2");
				this.IsActivated = true;
				return true;
				Block_25:
				flag = false;
			}
			else if (this.type == VPN.VPNType.Shadowrocket)
			{
				await this.device.ConfigurateAppium(100);
				string text6;
				for (;;)
				{
					Form1.Settings.ProxyList.TryDequeue(out text6);
					string[] array2 = text6.Split(new char[] { ':' });
					if (array2.Length > 4)
					{
						this.urlChange = text6.Replace(string.Concat(new string[]
						{
							array2[0],
							":",
							array2[1],
							":",
							array2[2],
							":",
							array2[3],
							":"
						}), "");
						int num2 = 0;
						try
						{
							LogsUtils.WriteLog("[" + _device.phoneUID + "]: Change IP by link: " + this.urlChange);
							await Requests.GET(this.urlChange, null);
						}
						catch
						{
							num2 = 1;
						}
						if (num2 == 1)
						{
						}
					}
					TaskAwaiter<bool> taskAwaiter = this.StartProxyShadowRocket(text6).GetAwaiter();
					if (!taskAwaiter.IsCompleted)
					{
						await taskAwaiter;
						TaskAwaiter<bool> taskAwaiter2;
						taskAwaiter = taskAwaiter2;
						taskAwaiter2 = default(TaskAwaiter<bool>);
					}
					if (taskAwaiter.GetResult())
					{
						break;
					}
					Form1.Settings.ProxyList.Enqueue(text6);
				}
				this.proxyData = text6;
				this.IsActivated = true;
				flag = true;
			}
			else
			{
				flag = false;
			}
			return flag;
		}

		// Token: 0x0600004B RID: 75 RVA: 0x00004D00 File Offset: 0x00002F00
		public async Task<bool> TurnOffVpn(iDevice _device, TikTokUtils tiktokUtils)
		{
			this.device = _device;
			await this.device.ConfigurateAppium(25);
			if (this.type == VPN.VPNType.BelkaVPN)
			{
				try
				{
					LogsUtils.WriteLog("[" + _device.phoneUID + "]: Terminate BelkaVPN");
					await this.device.TerminateApp("com.digiapp.belkavpn");
					LogsUtils.WriteLog("[" + _device.phoneUID + "]: Activate BelkaVPN");
					await this.device.ActivateApp("com.digiapp.belkavpn");
					DateTime dateTime = DateTime.Now.AddSeconds(10.0);
					while (DateTime.Now < dateTime)
					{
						LogsUtils.WriteLog("[" + _device.phoneUID + "]: Checking for elements");
						await this.device.GetSourceXml();
						if (this.device.IsElementInXmlContains("switch_on", null))
						{
							LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click Switch_on Button");
							await (await this.device.WaitForElementBy(0, "switch_on", 5.0)).Click(false);
							await Task.Delay(TimeSpan.FromSeconds(2.0));
							this.IsActivated = false;
							return true;
						}
						if (this.device.IsElementInXmlContains("switch_off", null))
						{
							this.IsActivated = false;
							return true;
						}
					}
					LogsUtils.WriteLog("[" + _device.phoneUID + "]: Elements not found");
					return false;
				}
				catch (Exception ex)
				{
					LogsUtils.WriteLog("[" + _device.phoneUID + "]: Exception: " + ex.ToString());
					return false;
				}
			}
			if (this.type == VPN.VPNType.PiaVpn)
			{
				try
				{
					LogsUtils.WriteLog("[" + _device.phoneUID + "]: Activate Pia VPN");
					await this.device.ActivateApp("com.privateinternetaccess.ios.PIA-VPN");
					await Task.Delay(TimeSpan.FromSeconds(4.0));
					await tiktokUtils.SaveScreenShot(false, TikTokUtils.ScreenshotType.Working);
					LogsUtils.WriteLog("[" + _device.phoneUID + "]: Terminate Pia VPN");
					await this.device.TerminateApp("com.privateinternetaccess.ios.PIA-VPN");
					LogsUtils.WriteLog("[" + _device.phoneUID + "]: Activate Pia VPN");
					await this.device.ActivateApp("com.privateinternetaccess.ios.PIA-VPN");
					LogsUtils.WriteLog("[" + _device.phoneUID + "]: Searching for Connection Button");
					Element element = await this.device.WaitForElementBy(0, "id.dashboard.connection.button", 10.0);
					string text = await element.Label();
					int num = 0;
					for (;;)
					{
						num++;
						LogsUtils.WriteLog("[" + _device.phoneUID + "]: Connection Label: " + text);
						if (text.Contains("disconnected"))
						{
							goto IL_0BC0;
						}
						LogsUtils.WriteLog("[" + _device.phoneUID + "]: Click Disconnect VPN Button");
						await element.TapAlert(0, 0, false);
						await Task.Delay(TimeSpan.FromSeconds(3.0));
						element = await this.device.WaitForElementBy(0, "id.dashboard.connection.button", 10.0);
						text = await element.Label();
						if (num > 2)
						{
							TaskAwaiter<Element> taskAwaiter = this.device.WaitForElementBy(1, "Disconnecting...", 10.0).GetAwaiter();
							if (!taskAwaiter.IsCompleted)
							{
								await taskAwaiter;
								TaskAwaiter<Element> taskAwaiter2;
								taskAwaiter = taskAwaiter2;
								taskAwaiter2 = default(TaskAwaiter<Element>);
							}
							if (taskAwaiter.GetResult() != null)
							{
								break;
							}
						}
					}
					LogsUtils.WriteLog("[" + _device.phoneUID + "]: Disconnecting VPN Error // Terminate Pia VPN");
					await this.device.TerminateApp("com.privateinternetaccess.ios.PIA-VPN");
					return true;
					IL_0BC0:
					LogsUtils.WriteLog("[" + _device.phoneUID + "]: VPN Disconnected");
					LogsUtils.WriteLog("[" + _device.phoneUID + "]: Terminate Pia VPN");
					await this.device.TerminateApp("com.privateinternetaccess.ios.PIA-VPN");
					this.IsActivated = false;
					return true;
				}
				catch
				{
					return false;
				}
			}
			bool flag;
			if (this.type == VPN.VPNType.Shadowrocket)
			{
				await this.TurnOffProxyShadowRocket();
				this.IsActivated = false;
				flag = true;
			}
			else
			{
				flag = false;
			}
			return flag;
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00004D54 File Offset: 0x00002F54
		private async Task<bool> AddNewProxyShadowRocket(string proxystring)
		{
			VPN.Class9 @class = new VPN.Class9();
			@class.Field0 = this;
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Terminate Shadowrocket");
			await this.device.TerminateApp("com.liguangming.Shadowrocket");
			await Task.Delay(TimeSpan.FromSeconds(1.0));
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Activate Shadowrocket");
			await this.device.ActivateApp("com.liguangming.Shadowrocket");
			await Task.Delay(TimeSpan.FromSeconds(2.0));
			string[] array = proxystring.Split(new char[] { ':' });
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Click Add Server Button");
			int num = 0;
			try
			{
				await (await this.device.WaitForElementBy(0, "Add Server", 10.0)).Click(false);
			}
			catch
			{
				num = 1;
			}
			if (num == 1)
			{
				await (await this.device.WaitForElementBy(0, "Add", 10.0)).Click(false);
			}
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Click Shadowsocks Button");
			await (await this.device.WaitForElementBy(0, "Shadowsocks", 10.0)).Click(false);
			VPN.ProxyType proxyType = this.prxType;
			if (proxyType != VPN.ProxyType.Socks5)
			{
				if (proxyType == VPN.ProxyType.Http)
				{
					LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Click HTTP Button");
					await (await this.device.WaitForElementBy(0, "HTTP", 10.0)).Click(false);
				}
			}
			else
			{
				LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Click Socks5 Button");
				await (await this.device.WaitForElementBy(0, "Socks5", 10.0)).Click(false);
			}
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Write Address " + array[0]);
			await (await this.device.WaitForElementBy(4, "Domain or IP", 10.0)).SetText(array[0]);
			await Task.Delay(TimeSpan.FromSeconds(1.0));
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Write Port " + array[1]);
			await (await this.device.WaitForElementBy(4, "1-65535", 10.0)).SetText(array[1]);
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Write User " + array[2]);
			await (await this.device.WaitForElementBy(1, "Optional", 10.0)).SetText(array[2]);
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Write Password " + array[3]);
			await (await this.device.WaitForElementBy(4, "Max Length 128", 10.0)).SetText(array[3]);
			await Task.Delay(TimeSpan.FromSeconds(1.0));
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Click Done Button");
			try
			{
				await (await this.device.WaitForElementBy(0, "Done", 10.0)).Click(false);
			}
			catch
			{
			}
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Click Save Button");
			await (await this.device.WaitForElementBy(0, "Save", 10.0)).Click(false);
			await Task.Delay(TimeSpan.FromSeconds(2.0));
			LogsUtils.WriteLog(string.Concat(new string[]
			{
				"[",
				this.device.phoneUID,
				"]: Click ",
				array[0],
				":",
				array[1],
				" Button"
			}));
			await (await this.device.WaitForElementBy(3, array[0] + ":" + array[1], 10.0)).Click(false);
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Click Not Connected Button");
			num = 0;
			try
			{
				await (await this.device.WaitForElementByXPath("XCUIElementTypeSwitch", 1, "0", 10.0)).Click(false);
				await Task.Delay(TimeSpan.FromSeconds(1.0));
			}
			catch
			{
				num = 1;
			}
			if (num == 1)
			{
				TaskAwaiter<Element> taskAwaiter = this.device.WaitForElementByXPath("XCUIElementTypeSwitch", 1, "1", 10.0).GetAwaiter();
				if (!taskAwaiter.IsCompleted)
				{
					await taskAwaiter;
					TaskAwaiter<Element> taskAwaiter2;
					taskAwaiter = taskAwaiter2;
					taskAwaiter2 = default(TaskAwaiter<Element>);
				}
				if (taskAwaiter.GetResult() != null)
				{
					return true;
				}
			}
			@class.cts = new CancellationTokenSource(TimeSpan.FromSeconds(10.0));
			try
			{
				await Task.Run(new Func<Task>(@class.Method0), @class.cts.Token);
			}
			catch (OperationCanceledException)
			{
				LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Operation timed out after 10 seconds.");
			}
			finally
			{
				if (@class.cts != null)
				{
					((IDisposable)@class.cts).Dispose();
				}
			}
			int num2 = 0;
			for (;;)
			{
				Element element = await this.device.WaitForElementByXPath("XCUIElementTypeSwitch", 1, "1", 10.0);
				Element element2 = await this.device.WaitForElementBy(3, "Connecting.", 10.0);
				Element element3 = await this.device.WaitForElementBy(0, "Not Connected", 10.0);
				LogsUtils.WriteLog(string.Format("[{0}]: ADD CHECK: {1} || ADD CHECK 2: {2} || ADD CHECK 3: {3}", new object[]
				{
					this.device.phoneUID,
					element != null,
					element2 != null,
					element3 != null
				}));
				if (element != null && element2 == null && element3 == null)
				{
					break;
				}
				num2++;
				if (num2 >= 2)
				{
					goto Block_14;
				}
				await Task.Delay(TimeSpan.FromSeconds(3.0));
			}
			return true;
			Block_14:
			return false;
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00004DA0 File Offset: 0x00002FA0
		private async Task<bool> AddNewVLESSShadowRocket(string url)
		{
			VPN.Class10 @class = new VPN.Class10();
			@class.Field0 = this;
			Uri uri = new Uri(url);
			string userInfo = uri.UserInfo;
			string host = uri.Host;
			string text = uri.Port.ToString();
			string text2 = "xhttp";
			string text3 = "/x";
			Uri.UnescapeDataString(uri.Fragment.TrimStart(new char[] { '#' }));
			NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(uri.Query);
			foreach (string text4 in nameValueCollection.AllKeys)
			{
				if (text4.Contains("type"))
				{
					text2 = nameValueCollection[text4];
				}
				else if (text4.Contains("path"))
				{
					text3 = nameValueCollection[text4];
				}
			}
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Terminate Shadowrocket");
			await this.device.TerminateApp("com.liguangming.Shadowrocket");
			await Task.Delay(TimeSpan.FromSeconds(1.0));
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Activate Shadowrocket");
			await this.device.ActivateApp("com.liguangming.Shadowrocket");
			await Task.Delay(TimeSpan.FromSeconds(2.0));
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Click Add Server Button");
			int num = 0;
			try
			{
				await (await this.device.WaitForElementBy(0, "Add Server", 10.0)).Click(false);
			}
			catch
			{
				num = 1;
			}
			int i = num;
			if (i == 1)
			{
				await (await this.device.WaitForElementBy(0, "Add", 10.0)).Click(false);
			}
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Click Shadowsocks Button");
			await (await this.device.WaitForElementBy(0, "Shadowsocks", 10.0)).Click(false);
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Click VLESS Button");
			await (await this.device.WaitForElementBy(0, "VLESS", 10.0)).Click(false);
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Write Address " + host);
			await (await this.device.WaitForElementBy(4, "Domain or IP", 10.0)).SetText(host);
			await Task.Delay(TimeSpan.FromSeconds(1.0));
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Write Port " + text);
			await (await this.device.WaitForElementBy(4, "1-65535", 10.0)).SetText(text);
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Write UUID " + userInfo);
			await (await this.device.WaitForElementBy(1, "Required", 10.0)).SetText(userInfo);
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Click Transport Button");
			await (await this.device.WaitForElementBy(0, "Transport", 10.0)).Click(false);
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Click Name Button");
			await (await this.device.WaitForElementBy(0, "Name", 10.0)).Click(false);
			LogsUtils.WriteLog(string.Concat(new string[]
			{
				"[",
				this.device.phoneUID,
				"]: Click ",
				text2,
				" Button"
			}));
			await (await this.device.WaitForElementBy(0, text2, 10.0)).Click(false);
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Write Path " + text3);
			Element element = await this.device.WaitForElementBy(1, "/", 10.0);
			await element.ClearText();
			await element.SetText(text3);
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Click Back Add Server Button");
			await (await this.device.WaitForElementBy(0, "Back Add Server", 10.0)).Click(false);
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Click Done Button");
			try
			{
				await (await this.device.WaitForElementBy(0, "Done", 10.0)).Click(false);
			}
			catch
			{
			}
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Click Save Button");
			await (await this.device.WaitForElementBy(0, "Save", 10.0)).Click(false);
			await Task.Delay(TimeSpan.FromSeconds(2.0));
			LogsUtils.WriteLog(string.Concat(new string[]
			{
				"[",
				this.device.phoneUID,
				"]: Click ",
				host,
				":",
				text,
				" Button"
			}));
			await (await this.device.WaitForElementBy(3, host + ":" + text, 10.0)).Click(false);
			await Task.Delay(TimeSpan.FromSeconds(2.0));
			LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Click Not Connected Button");
			num = 0;
			try
			{
				await (await this.device.WaitForElementByXPath("XCUIElementTypeSwitch", 1, "0", 10.0)).Click(false);
				await Task.Delay(TimeSpan.FromSeconds(1.0));
			}
			catch
			{
				num = 1;
			}
			i = num;
			if (i == 1)
			{
				TaskAwaiter<Element> taskAwaiter = this.device.WaitForElementByXPath("XCUIElementTypeSwitch", 1, "1", 10.0).GetAwaiter();
				if (!taskAwaiter.IsCompleted)
				{
					await taskAwaiter;
					TaskAwaiter<Element> taskAwaiter2;
					taskAwaiter = taskAwaiter2;
					taskAwaiter2 = default(TaskAwaiter<Element>);
				}
				if (taskAwaiter.GetResult() != null)
				{
					return true;
				}
			}
			@class.cts = new CancellationTokenSource(TimeSpan.FromSeconds(10.0));
			try
			{
				await Task.Run(new Func<Task>(@class.Method0), @class.cts.Token);
			}
			catch (OperationCanceledException)
			{
				LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Operation timed out after 10 seconds.");
			}
			finally
			{
				if (@class.cts != null)
				{
					((IDisposable)@class.cts).Dispose();
				}
			}
			int num2 = 0;
			for (;;)
			{
				Element element2 = await this.device.WaitForElementByXPath("XCUIElementTypeSwitch", 1, "1", 10.0);
				Element element3 = await this.device.WaitForElementBy(3, "Connecting.", 10.0);
				Element element4 = await this.device.WaitForElementBy(0, "Not Connected", 10.0);
				LogsUtils.WriteLog(string.Format("[{0}]: ADD CHECK: {1} || ADD CHECK 2: {2} || ADD CHECK 3: {3}", new object[]
				{
					this.device.phoneUID,
					element2 != null,
					element3 != null,
					element4 != null
				}));
				if (element2 != null && element3 == null && element4 == null)
				{
					break;
				}
				i = num2;
				num2 = i + 1;
				if (num2 >= 2)
				{
					goto Block_15;
				}
				await Task.Delay(TimeSpan.FromSeconds(3.0));
			}
			return true;
			Block_15:
			return false;
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00004DEC File Offset: 0x00002FEC
		private async Task<bool> StartProxyShadowRocket(string proxystring)
		{
			bool flag;
			try
			{
				LogsUtils.WriteLog(string.Concat(new string[]
				{
					"[",
					this.device.phoneUID,
					"]: Trying to run ",
					proxystring,
					" proxy"
				}));
				LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Terminate Shadowrocket");
				await this.device.TerminateApp("com.liguangming.Shadowrocket");
				await Task.Delay(TimeSpan.FromSeconds(1.0));
				LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Activate Shadowrocket");
				await this.device.ActivateApp("com.liguangming.Shadowrocket");
				await Task.Delay(TimeSpan.FromSeconds(2.0));
				if (proxystring.Contains("vless") || proxystring.Contains("VLESS"))
				{
					try
					{
						foreach (Element element in await this.device.WaitForElementsBy(0, "Sort", 10.0))
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
						await Task.Delay(TimeSpan.FromSeconds(1.0));
						await this.device.Tap(31.0, 290.0);
						await (await this.device.WaitForElementBy(2, "Delete", 10.0)).Click(false);
						await Task.Delay(TimeSpan.FromSeconds(1.0));
						List<Element> list = await this.device.WaitForElementsByXPath("XCUIElementTypeButton", 2, "Delete", 10.0);
						await list[list.Count - 1].Click(false);
					}
					catch (Exception)
					{
						LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Probably don't have proxy");
					}
					flag = await this.AddNewVLESSShadowRocket(proxystring);
				}
				else
				{
					string[] array = proxystring.Split(new char[] { ':' });
					LogsUtils.WriteLog(string.Concat(new string[]
					{
						"[",
						this.device.phoneUID,
						"]: Click ",
						array[0],
						":",
						array[1],
						" Button"
					}));
					int num = 0;
					try
					{
						await (await this.device.WaitForElementBy(3, array[0] + ":" + array[1], 10.0)).Click(false);
					}
					catch
					{
						num = 1;
					}
					if (num == 1)
					{
						try
						{
							foreach (Element element2 in await this.device.WaitForElementsBy(0, "Sort", 10.0))
							{
								try
								{
									await element2.Click(false);
								}
								catch
								{
								}
							}
							List<Element>.Enumerator enumerator = default(List<Element>.Enumerator);
							await Task.Delay(TimeSpan.FromSeconds(1.0));
							await this.device.Tap(31.0, 290.0);
							await (await this.device.WaitForElementBy(2, "Delete", 10.0)).Click(false);
							await Task.Delay(TimeSpan.FromSeconds(1.0));
							List<Element> list2 = await this.device.WaitForElementsByXPath("XCUIElementTypeButton", 2, "Delete", 10.0);
							await list2[list2.Count - 1].Click(false);
						}
						catch (Exception)
						{
							LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Probably don't have proxy");
						}
						flag = await this.AddNewProxyShadowRocket(proxystring);
					}
					else
					{
						num = 0;
						try
						{
							bool flag2 = false;
							for (int i = 0; i < 3; i++)
							{
								try
								{
									LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Click Not Connected Button");
									await (await this.device.WaitForElementByXPath("XCUIElementTypeSwitch", 1, "0", 3.0)).Click(false);
									flag2 = true;
									await Task.Delay(TimeSpan.FromSeconds(1.0));
								}
								catch (Exception ex)
								{
									if (flag2)
									{
										break;
									}
									throw ex;
								}
							}
						}
						catch
						{
							num = 1;
						}
						if (num == 1)
						{
							LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Checking If");
							TaskAwaiter<Element> taskAwaiter = this.device.WaitForElementByXPath("XCUIElementTypeSwitch", 1, "1", 10.0).GetAwaiter();
							if (!taskAwaiter.IsCompleted)
							{
								await taskAwaiter;
								TaskAwaiter<Element> taskAwaiter2;
								taskAwaiter = taskAwaiter2;
								taskAwaiter2 = default(TaskAwaiter<Element>);
							}
							if (taskAwaiter.GetResult() != null)
							{
								return true;
							}
						}
						VPN.Class11 @class = new VPN.Class11();
						@class.Field0 = this;
						@class.cts = new CancellationTokenSource(TimeSpan.FromSeconds(10.0));
						try
						{
							await Task.Run(new Func<Task>(@class.Method0), @class.cts.Token);
						}
						catch (OperationCanceledException)
						{
							LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Operation timed out after 10 seconds.");
						}
						finally
						{
							if (@class.cts != null)
							{
								((IDisposable)@class.cts).Dispose();
							}
						}
						@class = null;
						Element element3 = await this.device.WaitForElementBy(0, "Reset", 10.0);
						if (element3 != null)
						{
							await element3.TapAlert(0, 0, false);
							await Task.Delay(TimeSpan.FromSeconds(1.0));
							VPN.Class12 class2 = new VPN.Class12();
							class2.Field0 = this;
							class2.cts = new CancellationTokenSource(TimeSpan.FromSeconds(10.0));
							try
							{
								await Task.Run(new Func<Task>(class2.Method0), class2.cts.Token);
							}
							catch (OperationCanceledException)
							{
								LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Operation timed out after 10 seconds.");
							}
							finally
							{
								if (class2.cts != null)
								{
									((IDisposable)class2.cts).Dispose();
								}
							}
							class2 = null;
						}
						int num2 = 0;
						for (;;)
						{
							Element element4 = await this.device.WaitForElementByXPath("XCUIElementTypeSwitch", 1, "1", 2.0);
							Element element5 = await this.device.WaitForElementBy(3, "Connecting.", 2.0);
							Element element6 = await this.device.WaitForElementBy(0, "Not Connected", 2.0);
							LogsUtils.WriteLog(string.Format("[{0}]: CHECK: {1} || CHECK 2: {2} || CHECK 3: {3}", new object[]
							{
								this.device.phoneUID,
								element4 != null,
								element5 != null,
								element6 != null
							}));
							if (element4 != null && element5 == null && element6 == null)
							{
								break;
							}
							num2++;
							if (num2 >= 2)
							{
								goto Block_16;
							}
							LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Click Not Connected Button");
							try
							{
								await (await this.device.WaitForElementByXPath("XCUIElementTypeSwitch", 1, "0", 3.0)).Click(false);
							}
							catch
							{
							}
							await Task.Delay(TimeSpan.FromSeconds(3.0));
						}
						return true;
						Block_16:
						flag = false;
					}
				}
			}
			catch (Exception ex2)
			{
				LogsUtils.WriteLog("[" + this.device.phoneUID + "]: SS Exception: " + ex2.ToString());
				flag = false;
			}
			return flag;
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00004E38 File Offset: 0x00003038
		private async Task TurnOffProxyShadowRocket()
		{
			try
			{
				LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Terminate Shadowrocket");
				await this.device.TerminateApp("com.liguangming.Shadowrocket");
				await Task.Delay(TimeSpan.FromSeconds(1.0));
				LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Activate Shadowrocket");
				await this.device.ActivateApp("com.liguangming.Shadowrocket");
				await Task.Delay(TimeSpan.FromSeconds(2.0));
				LogsUtils.WriteLog("[" + this.device.phoneUID + "]: Click Not Connected Button");
				int num = 0;
				try
				{
					await (await this.device.WaitForElementByXPath("XCUIElementTypeSwitch", 1, "1", 10.0)).Click(false);
					await Task.Delay(TimeSpan.FromSeconds(3.0));
					try
					{
						await (await this.device.WaitForElementBy(3, "t Allow", 3.0)).TapAlert(0, 0, false);
					}
					catch
					{
					}
				}
				catch
				{
					num = 1;
				}
				if (num == 1)
				{
					Element element = await this.device.WaitForElementByXPath("XCUIElementTypeSwitch", 1, "0", 10.0);
					try
					{
						await (await this.device.WaitForElementBy(3, "t Allow", 3.0)).TapAlert(0, 0, false);
					}
					catch
					{
					}
					if (element == null)
					{
						element = null;
					}
				}
			}
			catch (Exception ex)
			{
				LogsUtils.WriteLog("[" + this.device.phoneUID + "]: TO S Exception: " + ex.ToString());
			}
		}

		// Token: 0x04000043 RID: 67
		public bool IsActivated;

		// Token: 0x04000044 RID: 68
		public string proxyData = "";

		// Token: 0x04000045 RID: 69
		public string urlChange = "";

		// Token: 0x04000046 RID: 70
		private VPN.VPNType type;

		// Token: 0x04000047 RID: 71
		private VPN.ProxyType prxType;

		// Token: 0x04000048 RID: 72
		private iDevice device;

		// Token: 0x02000019 RID: 25
		public enum ProxyType
		{
			// Token: 0x0400004A RID: 74
			Socks5,
			// Token: 0x0400004B RID: 75
			Http
		}

		// Token: 0x0200001A RID: 26
		public enum VPNType
		{
			// Token: 0x0400004D RID: 77
			PiaVpn,
			// Token: 0x0400004E RID: 78
			Shadowrocket,
			// Token: 0x0400004F RID: 79
			BelkaVPN
		}

		// Token: 0x0200001F RID: 31
		[CompilerGenerated]
		private sealed class Class9
		{
			// Token: 0x06000059 RID: 89 RVA: 0x00009DBC File Offset: 0x00007FBC
			internal async Task Method0()
			{
				LogsUtils.WriteLog("[" + this.Field0.device.phoneUID + "]: Checking for Connection Allert");
				try
				{
					await (await this.Field0.device.WaitForElementBy(0, "OK", 5.0)).TapAlert(0, 0, false);
				}
				catch
				{
				}
				await Task.Delay(TimeSpan.FromSeconds(1.0), this.cts.Token);
				LogsUtils.WriteLog("[" + this.Field0.device.phoneUID + "]: Checking for Allow Allert");
				try
				{
					await (await this.Field0.device.WaitForElementBy(0, "Allow", 10.0)).TapAlert(0, 0, false);
				}
				catch
				{
				}
				await Task.Delay(TimeSpan.FromSeconds(5.0), this.cts.Token);
			}

			// Token: 0x04000084 RID: 132
			public VPN Field0;

			// Token: 0x04000085 RID: 133
			public CancellationTokenSource cts;

			// Token: 0x02000020 RID: 32
			[StructLayout(LayoutKind.Auto)]
			private struct Struct11 : IAsyncStateMachine
			{
				// Token: 0x0600005A RID: 90 RVA: 0x00009E00 File Offset: 0x00008000
				void IAsyncStateMachine.MoveNext()
				{
					int num2;
					int num = num2;
					VPN.Class9 @class = this;
					try
					{
						TaskAwaiter taskAwaiter;
						TaskAwaiter<string> taskAwaiter4;
						TaskAwaiter<Element> taskAwaiter6;
						switch (num)
						{
						case 0:
						case 1:
							break;
						case 2:
						{
							TaskAwaiter taskAwaiter2;
							taskAwaiter = taskAwaiter2;
							taskAwaiter2 = default(TaskAwaiter);
							num = (num2 = -1);
							goto IL_01A1;
						}
						case 3:
						case 4:
							IL_01CC:
							try
							{
								TaskAwaiter<string> taskAwaiter3;
								TaskAwaiter<Element> taskAwaiter5;
								if (num != 3)
								{
									if (num == 4)
									{
										taskAwaiter3 = taskAwaiter4;
										taskAwaiter4 = default(TaskAwaiter<string>);
										num = (num2 = -1);
										goto IL_02A2;
									}
									taskAwaiter5 = @class.Field0.device.WaitForElementBy(0, "Allow", 10.0).GetAwaiter();
									if (!taskAwaiter5.IsCompleted)
									{
										num = (num2 = 3);
										taskAwaiter6 = taskAwaiter5;
										this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter<Element>, VPN.Class9.Struct11>(ref taskAwaiter5, ref this);
										return;
									}
								}
								else
								{
									taskAwaiter5 = taskAwaiter6;
									taskAwaiter6 = default(TaskAwaiter<Element>);
									num = (num2 = -1);
								}
								taskAwaiter3 = taskAwaiter5.GetResult().TapAlert(0, 0, false).GetAwaiter();
								if (!taskAwaiter3.IsCompleted)
								{
									num = (num2 = 4);
									taskAwaiter4 = taskAwaiter3;
									this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter<string>, VPN.Class9.Struct11>(ref taskAwaiter3, ref this);
									return;
								}
								IL_02A2:
								taskAwaiter3.GetResult();
							}
							catch
							{
							}
							taskAwaiter = Task.Delay(TimeSpan.FromSeconds(5.0), @class.cts.Token).GetAwaiter();
							if (!taskAwaiter.IsCompleted)
							{
								num = (num2 = 5);
								TaskAwaiter taskAwaiter2 = taskAwaiter;
								this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter, VPN.Class9.Struct11>(ref taskAwaiter, ref this);
								return;
							}
							goto IL_031B;
						case 5:
						{
							TaskAwaiter taskAwaiter2;
							taskAwaiter = taskAwaiter2;
							taskAwaiter2 = default(TaskAwaiter);
							num = (num2 = -1);
							goto IL_031B;
						}
						default:
							LogsUtils.WriteLog("[" + @class.Field0.device.phoneUID + "]: Checking for Connection Allert");
							break;
						}
						try
						{
							TaskAwaiter<string> taskAwaiter3;
							TaskAwaiter<Element> taskAwaiter5;
							if (num != 0)
							{
								if (num == 1)
								{
									taskAwaiter3 = taskAwaiter4;
									taskAwaiter4 = default(TaskAwaiter<string>);
									num = (num2 = -1);
									goto IL_0125;
								}
								taskAwaiter5 = @class.Field0.device.WaitForElementBy(0, "OK", 5.0).GetAwaiter();
								if (!taskAwaiter5.IsCompleted)
								{
									num = (num2 = 0);
									taskAwaiter6 = taskAwaiter5;
									this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter<Element>, VPN.Class9.Struct11>(ref taskAwaiter5, ref this);
									return;
								}
							}
							else
							{
								taskAwaiter5 = taskAwaiter6;
								taskAwaiter6 = default(TaskAwaiter<Element>);
								num = (num2 = -1);
							}
							taskAwaiter3 = taskAwaiter5.GetResult().TapAlert(0, 0, false).GetAwaiter();
							if (!taskAwaiter3.IsCompleted)
							{
								num = (num2 = 1);
								taskAwaiter4 = taskAwaiter3;
								this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter<string>, VPN.Class9.Struct11>(ref taskAwaiter3, ref this);
								return;
							}
							IL_0125:
							taskAwaiter3.GetResult();
						}
						catch
						{
						}
						taskAwaiter = Task.Delay(TimeSpan.FromSeconds(1.0), @class.cts.Token).GetAwaiter();
						if (!taskAwaiter.IsCompleted)
						{
							num = (num2 = 2);
							TaskAwaiter taskAwaiter2 = taskAwaiter;
							this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter, VPN.Class9.Struct11>(ref taskAwaiter, ref this);
							return;
						}
						IL_01A1:
						taskAwaiter.GetResult();
						LogsUtils.WriteLog("[" + @class.Field0.device.phoneUID + "]: Checking for Allow Allert");
						goto IL_01CC;
						IL_031B:
						taskAwaiter.GetResult();
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

				// Token: 0x0600005B RID: 91 RVA: 0x000022B2 File Offset: 0x000004B2
				[DebuggerHidden]
				void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
				{
					this.Field1.SetStateMachine(stateMachine);
				}

				// Token: 0x04000086 RID: 134
				public int Field0;

				// Token: 0x04000087 RID: 135
				public AsyncTaskMethodBuilder Field1;

				// Token: 0x04000088 RID: 136
				public VPN.Class9 Field2;

				// Token: 0x04000089 RID: 137
				private TaskAwaiter<Element> Field3;

				// Token: 0x0400008A RID: 138
				private TaskAwaiter<string> Field4;

				// Token: 0x0400008B RID: 139
				private TaskAwaiter Field5;
			}
		}

		// Token: 0x02000022 RID: 34
		[CompilerGenerated]
		private sealed class Class10
		{
			// Token: 0x0600005F RID: 95 RVA: 0x0000B868 File Offset: 0x00009A68
			internal async Task Method0()
			{
				LogsUtils.WriteLog("[" + this.Field0.device.phoneUID + "]: Checking for Connection Allert");
				try
				{
					await (await this.Field0.device.WaitForElementBy(0, "OK", 5.0)).TapAlert(0, 0, false);
				}
				catch
				{
				}
				await Task.Delay(TimeSpan.FromSeconds(1.0), this.cts.Token);
				LogsUtils.WriteLog("[" + this.Field0.device.phoneUID + "]: Checking for Allow Allert");
				try
				{
					await (await this.Field0.device.WaitForElementBy(0, "Allow", 10.0)).TapAlert(0, 0, false);
				}
				catch
				{
				}
				await Task.Delay(TimeSpan.FromSeconds(5.0), this.cts.Token);
			}

			// Token: 0x0400009A RID: 154
			public VPN Field0;

			// Token: 0x0400009B RID: 155
			public CancellationTokenSource cts;

			// Token: 0x02000023 RID: 35
			[StructLayout(LayoutKind.Auto)]
			private struct Struct13 : IAsyncStateMachine
			{
				// Token: 0x06000060 RID: 96 RVA: 0x0000B8AC File Offset: 0x00009AAC
				void IAsyncStateMachine.MoveNext()
				{
					int num2;
					int num = num2;
					VPN.Class10 @class = this;
					try
					{
						TaskAwaiter taskAwaiter;
						TaskAwaiter<string> taskAwaiter4;
						TaskAwaiter<Element> taskAwaiter6;
						switch (num)
						{
						case 0:
						case 1:
							break;
						case 2:
						{
							TaskAwaiter taskAwaiter2;
							taskAwaiter = taskAwaiter2;
							taskAwaiter2 = default(TaskAwaiter);
							num = (num2 = -1);
							goto IL_01A1;
						}
						case 3:
						case 4:
							IL_01CC:
							try
							{
								TaskAwaiter<string> taskAwaiter3;
								TaskAwaiter<Element> taskAwaiter5;
								if (num != 3)
								{
									if (num == 4)
									{
										taskAwaiter3 = taskAwaiter4;
										taskAwaiter4 = default(TaskAwaiter<string>);
										num = (num2 = -1);
										goto IL_02A2;
									}
									taskAwaiter5 = @class.Field0.device.WaitForElementBy(0, "Allow", 10.0).GetAwaiter();
									if (!taskAwaiter5.IsCompleted)
									{
										num = (num2 = 3);
										taskAwaiter6 = taskAwaiter5;
										this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter<Element>, VPN.Class10.Struct13>(ref taskAwaiter5, ref this);
										return;
									}
								}
								else
								{
									taskAwaiter5 = taskAwaiter6;
									taskAwaiter6 = default(TaskAwaiter<Element>);
									num = (num2 = -1);
								}
								taskAwaiter3 = taskAwaiter5.GetResult().TapAlert(0, 0, false).GetAwaiter();
								if (!taskAwaiter3.IsCompleted)
								{
									num = (num2 = 4);
									taskAwaiter4 = taskAwaiter3;
									this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter<string>, VPN.Class10.Struct13>(ref taskAwaiter3, ref this);
									return;
								}
								IL_02A2:
								taskAwaiter3.GetResult();
							}
							catch
							{
							}
							taskAwaiter = Task.Delay(TimeSpan.FromSeconds(5.0), @class.cts.Token).GetAwaiter();
							if (!taskAwaiter.IsCompleted)
							{
								num = (num2 = 5);
								TaskAwaiter taskAwaiter2 = taskAwaiter;
								this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter, VPN.Class10.Struct13>(ref taskAwaiter, ref this);
								return;
							}
							goto IL_031B;
						case 5:
						{
							TaskAwaiter taskAwaiter2;
							taskAwaiter = taskAwaiter2;
							taskAwaiter2 = default(TaskAwaiter);
							num = (num2 = -1);
							goto IL_031B;
						}
						default:
							LogsUtils.WriteLog("[" + @class.Field0.device.phoneUID + "]: Checking for Connection Allert");
							break;
						}
						try
						{
							TaskAwaiter<string> taskAwaiter3;
							TaskAwaiter<Element> taskAwaiter5;
							if (num != 0)
							{
								if (num == 1)
								{
									taskAwaiter3 = taskAwaiter4;
									taskAwaiter4 = default(TaskAwaiter<string>);
									num = (num2 = -1);
									goto IL_0125;
								}
								taskAwaiter5 = @class.Field0.device.WaitForElementBy(0, "OK", 5.0).GetAwaiter();
								if (!taskAwaiter5.IsCompleted)
								{
									num = (num2 = 0);
									taskAwaiter6 = taskAwaiter5;
									this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter<Element>, VPN.Class10.Struct13>(ref taskAwaiter5, ref this);
									return;
								}
							}
							else
							{
								taskAwaiter5 = taskAwaiter6;
								taskAwaiter6 = default(TaskAwaiter<Element>);
								num = (num2 = -1);
							}
							taskAwaiter3 = taskAwaiter5.GetResult().TapAlert(0, 0, false).GetAwaiter();
							if (!taskAwaiter3.IsCompleted)
							{
								num = (num2 = 1);
								taskAwaiter4 = taskAwaiter3;
								this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter<string>, VPN.Class10.Struct13>(ref taskAwaiter3, ref this);
								return;
							}
							IL_0125:
							taskAwaiter3.GetResult();
						}
						catch
						{
						}
						taskAwaiter = Task.Delay(TimeSpan.FromSeconds(1.0), @class.cts.Token).GetAwaiter();
						if (!taskAwaiter.IsCompleted)
						{
							num = (num2 = 2);
							TaskAwaiter taskAwaiter2 = taskAwaiter;
							this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter, VPN.Class10.Struct13>(ref taskAwaiter, ref this);
							return;
						}
						IL_01A1:
						taskAwaiter.GetResult();
						LogsUtils.WriteLog("[" + @class.Field0.device.phoneUID + "]: Checking for Allow Allert");
						goto IL_01CC;
						IL_031B:
						taskAwaiter.GetResult();
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

				// Token: 0x06000061 RID: 97 RVA: 0x000022CE File Offset: 0x000004CE
				[DebuggerHidden]
				void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
				{
					this.Field1.SetStateMachine(stateMachine);
				}

				// Token: 0x0400009C RID: 156
				public int Field0;

				// Token: 0x0400009D RID: 157
				public AsyncTaskMethodBuilder Field1;

				// Token: 0x0400009E RID: 158
				public VPN.Class10 Field2;

				// Token: 0x0400009F RID: 159
				private TaskAwaiter<Element> Field3;

				// Token: 0x040000A0 RID: 160
				private TaskAwaiter<string> Field4;

				// Token: 0x040000A1 RID: 161
				private TaskAwaiter Field5;
			}
		}

		// Token: 0x02000025 RID: 37
		[CompilerGenerated]
		private sealed class Class11
		{
			// Token: 0x06000065 RID: 101 RVA: 0x0000D76C File Offset: 0x0000B96C
			internal async Task Method0()
			{
				LogsUtils.WriteLog("[" + this.Field0.device.phoneUID + "]: Checking for Connection Allert");
				try
				{
					await (await this.Field0.device.WaitForElementBy(0, "OK", 5.0)).TapAlert(0, 0, false);
				}
				catch
				{
				}
				await Task.Delay(TimeSpan.FromSeconds(1.0), this.cts.Token);
				LogsUtils.WriteLog("[" + this.Field0.device.phoneUID + "]: Checking for Allow Allert");
				try
				{
					await (await this.Field0.device.WaitForElementBy(0, "Allow", 10.0)).TapAlert(0, 0, false);
				}
				catch
				{
				}
				await Task.Delay(TimeSpan.FromSeconds(5.0), this.cts.Token);
			}

			// Token: 0x040000B5 RID: 181
			public CancellationTokenSource cts;

			// Token: 0x040000B6 RID: 182
			public VPN Field0;

			// Token: 0x02000026 RID: 38
			[StructLayout(LayoutKind.Auto)]
			private struct Struct15 : IAsyncStateMachine
			{
				// Token: 0x06000066 RID: 102 RVA: 0x0000D7B0 File Offset: 0x0000B9B0
				void IAsyncStateMachine.MoveNext()
				{
					int num2;
					int num = num2;
					VPN.Class11 @class = this;
					try
					{
						TaskAwaiter taskAwaiter;
						TaskAwaiter<string> taskAwaiter4;
						TaskAwaiter<Element> taskAwaiter6;
						switch (num)
						{
						case 0:
						case 1:
							break;
						case 2:
						{
							TaskAwaiter taskAwaiter2;
							taskAwaiter = taskAwaiter2;
							taskAwaiter2 = default(TaskAwaiter);
							num = (num2 = -1);
							goto IL_01A1;
						}
						case 3:
						case 4:
							IL_01CC:
							try
							{
								TaskAwaiter<string> taskAwaiter3;
								TaskAwaiter<Element> taskAwaiter5;
								if (num != 3)
								{
									if (num == 4)
									{
										taskAwaiter3 = taskAwaiter4;
										taskAwaiter4 = default(TaskAwaiter<string>);
										num = (num2 = -1);
										goto IL_02A2;
									}
									taskAwaiter5 = @class.Field0.device.WaitForElementBy(0, "Allow", 10.0).GetAwaiter();
									if (!taskAwaiter5.IsCompleted)
									{
										num = (num2 = 3);
										taskAwaiter6 = taskAwaiter5;
										this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter<Element>, VPN.Class11.Struct15>(ref taskAwaiter5, ref this);
										return;
									}
								}
								else
								{
									taskAwaiter5 = taskAwaiter6;
									taskAwaiter6 = default(TaskAwaiter<Element>);
									num = (num2 = -1);
								}
								taskAwaiter3 = taskAwaiter5.GetResult().TapAlert(0, 0, false).GetAwaiter();
								if (!taskAwaiter3.IsCompleted)
								{
									num = (num2 = 4);
									taskAwaiter4 = taskAwaiter3;
									this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter<string>, VPN.Class11.Struct15>(ref taskAwaiter3, ref this);
									return;
								}
								IL_02A2:
								taskAwaiter3.GetResult();
							}
							catch
							{
							}
							taskAwaiter = Task.Delay(TimeSpan.FromSeconds(5.0), @class.cts.Token).GetAwaiter();
							if (!taskAwaiter.IsCompleted)
							{
								num = (num2 = 5);
								TaskAwaiter taskAwaiter2 = taskAwaiter;
								this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter, VPN.Class11.Struct15>(ref taskAwaiter, ref this);
								return;
							}
							goto IL_031B;
						case 5:
						{
							TaskAwaiter taskAwaiter2;
							taskAwaiter = taskAwaiter2;
							taskAwaiter2 = default(TaskAwaiter);
							num = (num2 = -1);
							goto IL_031B;
						}
						default:
							LogsUtils.WriteLog("[" + @class.Field0.device.phoneUID + "]: Checking for Connection Allert");
							break;
						}
						try
						{
							TaskAwaiter<string> taskAwaiter3;
							TaskAwaiter<Element> taskAwaiter5;
							if (num != 0)
							{
								if (num == 1)
								{
									taskAwaiter3 = taskAwaiter4;
									taskAwaiter4 = default(TaskAwaiter<string>);
									num = (num2 = -1);
									goto IL_0125;
								}
								taskAwaiter5 = @class.Field0.device.WaitForElementBy(0, "OK", 5.0).GetAwaiter();
								if (!taskAwaiter5.IsCompleted)
								{
									num = (num2 = 0);
									taskAwaiter6 = taskAwaiter5;
									this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter<Element>, VPN.Class11.Struct15>(ref taskAwaiter5, ref this);
									return;
								}
							}
							else
							{
								taskAwaiter5 = taskAwaiter6;
								taskAwaiter6 = default(TaskAwaiter<Element>);
								num = (num2 = -1);
							}
							taskAwaiter3 = taskAwaiter5.GetResult().TapAlert(0, 0, false).GetAwaiter();
							if (!taskAwaiter3.IsCompleted)
							{
								num = (num2 = 1);
								taskAwaiter4 = taskAwaiter3;
								this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter<string>, VPN.Class11.Struct15>(ref taskAwaiter3, ref this);
								return;
							}
							IL_0125:
							taskAwaiter3.GetResult();
						}
						catch
						{
						}
						taskAwaiter = Task.Delay(TimeSpan.FromSeconds(1.0), @class.cts.Token).GetAwaiter();
						if (!taskAwaiter.IsCompleted)
						{
							num = (num2 = 2);
							TaskAwaiter taskAwaiter2 = taskAwaiter;
							this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter, VPN.Class11.Struct15>(ref taskAwaiter, ref this);
							return;
						}
						IL_01A1:
						taskAwaiter.GetResult();
						LogsUtils.WriteLog("[" + @class.Field0.device.phoneUID + "]: Checking for Allow Allert");
						goto IL_01CC;
						IL_031B:
						taskAwaiter.GetResult();
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

				// Token: 0x06000067 RID: 103 RVA: 0x000022EA File Offset: 0x000004EA
				[DebuggerHidden]
				void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
				{
					this.Field1.SetStateMachine(stateMachine);
				}

				// Token: 0x040000B7 RID: 183
				public int Field0;

				// Token: 0x040000B8 RID: 184
				public AsyncTaskMethodBuilder Field1;

				// Token: 0x040000B9 RID: 185
				public VPN.Class11 Field2;

				// Token: 0x040000BA RID: 186
				private TaskAwaiter<Element> Field3;

				// Token: 0x040000BB RID: 187
				private TaskAwaiter<string> Field4;

				// Token: 0x040000BC RID: 188
				private TaskAwaiter Field5;
			}
		}

		// Token: 0x02000027 RID: 39
		[CompilerGenerated]
		private sealed class Class12
		{
			// Token: 0x06000069 RID: 105 RVA: 0x0000DB5C File Offset: 0x0000BD5C
			internal async Task Method0()
			{
				LogsUtils.WriteLog("[" + this.Field0.device.phoneUID + "]: Checking for Connection Allert");
				try
				{
					await (await this.Field0.device.WaitForElementBy(0, "OK", 5.0)).TapAlert(0, 0, false);
				}
				catch
				{
				}
				await Task.Delay(TimeSpan.FromSeconds(1.0), this.cts.Token);
				LogsUtils.WriteLog("[" + this.Field0.device.phoneUID + "]: Checking for Allow Allert");
				try
				{
					await (await this.Field0.device.WaitForElementBy(0, "Allow", 10.0)).TapAlert(0, 0, false);
				}
				catch
				{
				}
				await Task.Delay(TimeSpan.FromSeconds(5.0), this.cts.Token);
			}

			// Token: 0x040000BD RID: 189
			public CancellationTokenSource cts;

			// Token: 0x040000BE RID: 190
			public VPN Field0;

			// Token: 0x02000028 RID: 40
			[StructLayout(LayoutKind.Auto)]
			private struct Struct16 : IAsyncStateMachine
			{
				// Token: 0x0600006A RID: 106 RVA: 0x0000DBA0 File Offset: 0x0000BDA0
				void IAsyncStateMachine.MoveNext()
				{
					int num2;
					int num = num2;
					VPN.Class12 @class = this;
					try
					{
						TaskAwaiter taskAwaiter;
						TaskAwaiter<string> taskAwaiter4;
						TaskAwaiter<Element> taskAwaiter6;
						switch (num)
						{
						case 0:
						case 1:
							break;
						case 2:
						{
							TaskAwaiter taskAwaiter2;
							taskAwaiter = taskAwaiter2;
							taskAwaiter2 = default(TaskAwaiter);
							num = (num2 = -1);
							goto IL_01A1;
						}
						case 3:
						case 4:
							IL_01CC:
							try
							{
								TaskAwaiter<string> taskAwaiter3;
								TaskAwaiter<Element> taskAwaiter5;
								if (num != 3)
								{
									if (num == 4)
									{
										taskAwaiter3 = taskAwaiter4;
										taskAwaiter4 = default(TaskAwaiter<string>);
										num = (num2 = -1);
										goto IL_02A2;
									}
									taskAwaiter5 = @class.Field0.device.WaitForElementBy(0, "Allow", 10.0).GetAwaiter();
									if (!taskAwaiter5.IsCompleted)
									{
										num = (num2 = 3);
										taskAwaiter6 = taskAwaiter5;
										this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter<Element>, VPN.Class12.Struct16>(ref taskAwaiter5, ref this);
										return;
									}
								}
								else
								{
									taskAwaiter5 = taskAwaiter6;
									taskAwaiter6 = default(TaskAwaiter<Element>);
									num = (num2 = -1);
								}
								taskAwaiter3 = taskAwaiter5.GetResult().TapAlert(0, 0, false).GetAwaiter();
								if (!taskAwaiter3.IsCompleted)
								{
									num = (num2 = 4);
									taskAwaiter4 = taskAwaiter3;
									this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter<string>, VPN.Class12.Struct16>(ref taskAwaiter3, ref this);
									return;
								}
								IL_02A2:
								taskAwaiter3.GetResult();
							}
							catch
							{
							}
							taskAwaiter = Task.Delay(TimeSpan.FromSeconds(5.0), @class.cts.Token).GetAwaiter();
							if (!taskAwaiter.IsCompleted)
							{
								num = (num2 = 5);
								TaskAwaiter taskAwaiter2 = taskAwaiter;
								this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter, VPN.Class12.Struct16>(ref taskAwaiter, ref this);
								return;
							}
							goto IL_031B;
						case 5:
						{
							TaskAwaiter taskAwaiter2;
							taskAwaiter = taskAwaiter2;
							taskAwaiter2 = default(TaskAwaiter);
							num = (num2 = -1);
							goto IL_031B;
						}
						default:
							LogsUtils.WriteLog("[" + @class.Field0.device.phoneUID + "]: Checking for Connection Allert");
							break;
						}
						try
						{
							TaskAwaiter<string> taskAwaiter3;
							TaskAwaiter<Element> taskAwaiter5;
							if (num != 0)
							{
								if (num == 1)
								{
									taskAwaiter3 = taskAwaiter4;
									taskAwaiter4 = default(TaskAwaiter<string>);
									num = (num2 = -1);
									goto IL_0125;
								}
								taskAwaiter5 = @class.Field0.device.WaitForElementBy(0, "OK", 5.0).GetAwaiter();
								if (!taskAwaiter5.IsCompleted)
								{
									num = (num2 = 0);
									taskAwaiter6 = taskAwaiter5;
									this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter<Element>, VPN.Class12.Struct16>(ref taskAwaiter5, ref this);
									return;
								}
							}
							else
							{
								taskAwaiter5 = taskAwaiter6;
								taskAwaiter6 = default(TaskAwaiter<Element>);
								num = (num2 = -1);
							}
							taskAwaiter3 = taskAwaiter5.GetResult().TapAlert(0, 0, false).GetAwaiter();
							if (!taskAwaiter3.IsCompleted)
							{
								num = (num2 = 1);
								taskAwaiter4 = taskAwaiter3;
								this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter<string>, VPN.Class12.Struct16>(ref taskAwaiter3, ref this);
								return;
							}
							IL_0125:
							taskAwaiter3.GetResult();
						}
						catch
						{
						}
						taskAwaiter = Task.Delay(TimeSpan.FromSeconds(1.0), @class.cts.Token).GetAwaiter();
						if (!taskAwaiter.IsCompleted)
						{
							num = (num2 = 2);
							TaskAwaiter taskAwaiter2 = taskAwaiter;
							this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter, VPN.Class12.Struct16>(ref taskAwaiter, ref this);
							return;
						}
						IL_01A1:
						taskAwaiter.GetResult();
						LogsUtils.WriteLog("[" + @class.Field0.device.phoneUID + "]: Checking for Allow Allert");
						goto IL_01CC;
						IL_031B:
						taskAwaiter.GetResult();
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

				// Token: 0x0600006B RID: 107 RVA: 0x000022F8 File Offset: 0x000004F8
				[DebuggerHidden]
				void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
				{
					this.Field1.SetStateMachine(stateMachine);
				}

				// Token: 0x040000BF RID: 191
				public int Field0;

				// Token: 0x040000C0 RID: 192
				public AsyncTaskMethodBuilder Field1;

				// Token: 0x040000C1 RID: 193
				public VPN.Class12 Field2;

				// Token: 0x040000C2 RID: 194
				private TaskAwaiter<Element> Field3;

				// Token: 0x040000C3 RID: 195
				private TaskAwaiter<string> Field4;

				// Token: 0x040000C4 RID: 196
				private TaskAwaiter Field5;
			}
		}
	}
}
