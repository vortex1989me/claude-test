using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TikTok_Automation_Library_Non_Jail.Forms;
using TikTok_Automation_Library_Non_Jail.Properties;
using TikTok_Automation_Library_Non_Jail.Security;
using TikTok_Automation_Library_Non_Jail.Utils;
using TikTok_Automation_Library_Non_Jail.Utils.TempMail;
using TikTok_Automation_Library_Non_Jail.Work.Making;
using TikTok_Automation_Library_Non_Jail.Work.Posting;
using TikTok_Automation_Library_Non_Jail.Work.SignIn;
using TikTok_Automation_Library_Non_Jail.Work.Utils;
using WDA_Framework;
using WDA_Framework.Utils;

namespace TikTok_Automation_Library_Non_Jail.Work
{
	// Token: 0x0200002B RID: 43
	internal class Worker
	{
		// Token: 0x06000070 RID: 112 RVA: 0x00010028 File Offset: 0x0000E228
		public async Task<string> UploadToDataBase(string data)
		{
			await Task.CompletedTask;
			return "";
		}

		// Token: 0x06000071 RID: 113 RVA: 0x00002322 File Offset: 0x00000522
		public void WriteLog(string log)
		{
			LogsUtils.WriteLog(log);
		}

		// Token: 0x06000072 RID: 114 RVA: 0x00010074 File Offset: 0x0000E274
		private void ChangeRegions(bool WithBug = false)
		{
			string region_POSTING_VPN = this.REGION_POSTING_VPN;
			this.WriteLog("[" + this.phoneUID + "]: Change Regions");
			if (Settings.Default.VPNByLine)
			{
				if (this.VPNRegionsMAKING.Count > 0)
				{
					this.VPNRegionsMAKING.TryDequeue(out this.REGION_MAKING);
				}
				else
				{
					this.VPNRegionsMAKING = new ConcurrentQueue<string>(Form1.Settings.REGIONS_MAKING);
					this.VPNRegionsMAKING.TryDequeue(out this.REGION_MAKING);
				}
				if (this.RegionsPOSTING.Count > 0)
				{
					this.RegionsPOSTING.TryDequeue(out this.REGION_POSTING);
				}
				else
				{
					this.RegionsPOSTING = new ConcurrentQueue<string>(Form1.Settings.REGIONS_POSTING);
					this.RegionsPOSTING.TryDequeue(out this.REGION_POSTING);
				}
				if (this.VPNRegionsPOSTING.Count > 0)
				{
					this.VPNRegionsPOSTING.TryDequeue(out this.REGION_POSTING_VPN);
				}
				else
				{
					this.VPNRegionsPOSTING = new ConcurrentQueue<string>(Form1.Settings.REGIONS_POSTING_VPN);
					this.VPNRegionsPOSTING.TryDequeue(out this.REGION_POSTING_VPN);
				}
			}
			else if (Settings.Default.VPNRandomLine)
			{
				this.REGION_POSTING = Form1.Settings.REGIONS_POSTING[ConstParams.rand.Next(Form1.Settings.REGIONS_POSTING.Count)];
				this.REGION_MAKING = Form1.Settings.REGIONS_MAKING[ConstParams.rand.Next(Form1.Settings.REGIONS_MAKING.Count)];
				this.REGION_POSTING_VPN = Form1.Settings.REGIONS_POSTING_VPN[ConstParams.rand.Next(Form1.Settings.REGIONS_POSTING_VPN.Count)];
			}
			if (region_POSTING_VPN == this.REGION_POSTING_VPN && WithBug)
			{
				this.REGION_POSTING_VPN = "United States";
			}
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00010214 File Offset: 0x0000E414
		public async Task StartWork(int potok)
		{
			LogsUtils.LogTarget = new Action<string>(LogsUtils.WriteLog);
			this.Maker.phoneUID = this.phoneUID;
			this.tiktokUtils.phoneUID = this.phoneUID;
			this.tiktokUtils.PhoneModel = this.PhoneModel;
			this.Signing.phoneUID = this.phoneUID;
			this.Poster.phoneUID = this.phoneUID;
			bool flag = true;
			bool flag2 = true;
			bool flag3 = false;
			bool flag4 = true;
			int num = 0;
			this.potok_ = potok;
			bool flag5 = false;
			string text = "";
			for (;;)
			{
				IL_00C0:
				try
				{
					try
					{
						this.RegionsPOSTING = new ConcurrentQueue<string>(Form1.Settings.REGIONS_POSTING);
						this.VPNRegionsMAKING = new ConcurrentQueue<string>(Form1.Settings.REGIONS_MAKING);
						this.VPNRegionsPOSTING = new ConcurrentQueue<string>(Form1.Settings.REGIONS_POSTING_VPN);
						if (Settings.Default.VPNByLine)
						{
							if (this.VPNRegionsMAKING.Count <= 0)
							{
								this.WriteLog("[" + this.phoneUID + "]: DON'T HAVE REGIONS FOR MAKING");
								await Task.Delay(TimeSpan.FromSeconds(40.0));
								continue;
							}
							this.VPNRegionsMAKING.TryDequeue(out this.REGION_MAKING);
							if (this.RegionsPOSTING.Count <= 0)
							{
								this.WriteLog("[" + this.phoneUID + "]: DON'T HAVE REGIONS FOR POSTING");
								await Task.Delay(TimeSpan.FromSeconds(40.0));
								continue;
							}
							this.RegionsPOSTING.TryDequeue(out this.REGION_POSTING);
							if (this.VPNRegionsPOSTING.Count <= 0)
							{
								this.WriteLog("[" + this.phoneUID + "]: DON'T HAVE VPN REGIONS FOR POSTING");
								await Task.Delay(TimeSpan.FromSeconds(40.0));
								continue;
							}
							this.VPNRegionsPOSTING.TryDequeue(out this.REGION_POSTING_VPN);
						}
						else if (Settings.Default.VPNRandomLine)
						{
							this.REGION_MAKING = Form1.Settings.REGIONS_MAKING[ConstParams.rand.Next(Form1.Settings.REGIONS_MAKING.Count)];
							this.REGION_POSTING = Form1.Settings.REGIONS_POSTING[ConstParams.rand.Next(Form1.Settings.REGIONS_POSTING.Count)];
							this.REGION_POSTING_VPN = Form1.Settings.REGIONS_POSTING_VPN[ConstParams.rand.Next(Form1.Settings.REGIONS_POSTING_VPN.Count)];
						}
						Form1.StatusList[potok] = "Started";
						this.WriteLog("[" + this.phoneUID + "]: Device Model: " + this.PhoneModel);
						this.WriteLog("[" + this.phoneUID + "]: Device Version: " + this.iOSVersion.ToString());
						this.Started = true;
						this.device = new iDevice(this.port, this.PhoneModel, this.iOSVersion, false, this.phoneUID, true, false);
						this.Maker.device = this.device;
						this.tiktokUtils.device = this.device;
						this.Signing.device = this.device;
						this.Poster.device = this.device;
						this.WriteLog("[" + this.phoneUID + "]: Appium 25 | 0");
						await this.device.ConfigurateAppium(25);
					}
					catch (Exception ex)
					{
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Starting Exception: " + ex.ToString());
					}
					LogsUtils.WriteLog("[" + this.phoneUID + "]: SCheck");
					TaskAwaiter<bool> taskAwaiter = Checker.CheckItStatic(this.phoneUID).GetAwaiter();
					TaskAwaiter<bool> taskAwaiter2;
					int num2;
					if (!taskAwaiter.IsCompleted)
					{
						await taskAwaiter;
						taskAwaiter = taskAwaiter2;
						taskAwaiter2 = default(TaskAwaiter<bool>);
						num2 = -1;
					}
					if (!taskAwaiter.GetResult())
					{
						await this.tiktokUtils.GoHome(true);
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Swipe");
						await this.device.Swipe(350, 26, 350, 500, 0.0);
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Lost Phone Connection! Reconnect phone!");
						await Task.Delay(TimeSpan.FromMinutes(10.0));
						continue;
					}
					if (this.ConfigurateOnly)
					{
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Configurate ShortCuts");
						await this.tiktokUtils.ConfigurateShortcuts();
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Configurated!");
						Form1.StatusList[potok] = "Configurated!";
						this.ConfigurateOnly = false;
						try
						{
							this.device.wda.StopAll();
						}
						catch
						{
						}
					}
					else
					{
						this.WorkStarted = true;
						IL_8052:
						while (this.Work)
						{
							string text2 = null;
							int num3 = 0;
							int num6;
							try
							{
								await this.tiktokUtils.GoHome(true);
								LogsUtils.WriteLog("[" + this.phoneUID + "]: Swipe");
								await this.device.Swipe(350, 26, 350, 500, 0.0);
								LogsUtils.WriteLog("[" + this.phoneUID + "]: Check for VPN Element");
								Element element = await this.device.WaitForElementBy(0, "VPN", 1.0);
								await this.tiktokUtils.GoHome(false);
								if (element != null)
								{
									await this.vpn.TurnOffVpn(this.device, this.tiktokUtils);
								}
								if ((Settings.Default.UseVPN || Settings.Default.UseShadowRocket) && this.vpn.IsActivated)
								{
									await this.vpn.TurnOffVpn(this.device, this.tiktokUtils);
								}
								TaskAwaiter<Element> taskAwaiter4;
								if (Settings.Default.RecordScreen)
								{
									await this.tiktokUtils.GoHome(false);
									LogsUtils.WriteLog("[" + this.phoneUID + "]: Checking for Screen Recording element");
									TaskAwaiter<Element> taskAwaiter3 = this.device.WaitForElementBy(3, "Screen recording in progress", 1.0).GetAwaiter();
									if (!taskAwaiter3.IsCompleted)
									{
										await taskAwaiter3;
										taskAwaiter3 = taskAwaiter4;
										taskAwaiter4 = default(TaskAwaiter<Element>);
										num2 = -1;
									}
									if (taskAwaiter3.GetResult() != null)
									{
										await this.ScreenRecorder(true, true);
									}
								}
								await this.CheckSimAlert();
								this.AccountsOnVpn++;
								int num4;
								TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter8;
								if (Settings.Default.FirstPosting && (Settings.Default.Posting || Settings.Default.MakingPlusPosting))
								{
									if (Settings.Default.RecordScreen)
									{
										await this.tiktokUtils.GoHome(false);
										LogsUtils.WriteLog("[" + this.phoneUID + "]: Checking for Screen Recording element");
										TaskAwaiter<Element> taskAwaiter3 = this.device.WaitForElementBy(3, "Screen recording in progress", 1.0).GetAwaiter();
										if (!taskAwaiter3.IsCompleted)
										{
											await taskAwaiter3;
											taskAwaiter3 = taskAwaiter4;
											taskAwaiter4 = default(TaskAwaiter<Element>);
											num2 = -1;
										}
										if (taskAwaiter3.GetResult() == null)
										{
											await this.ScreenRecorder(false, false);
										}
									}
									if (Settings.Default.UseVPN || Settings.Default.UseShadowRocket)
									{
										for (;;)
										{
											taskAwaiter = this.vpn.TurnOnVpn(this.device, this.REGION_POSTING_VPN).GetAwaiter();
											if (!taskAwaiter.IsCompleted)
											{
												await taskAwaiter;
												taskAwaiter = taskAwaiter2;
												taskAwaiter2 = default(TaskAwaiter<bool>);
												num2 = -1;
											}
											if (taskAwaiter.GetResult())
											{
												break;
											}
											this.ChangeRegions(true);
										}
									}
								}
								else
								{
									if (this.AccountsOnVpn > Settings.Default.AccountsOnVpn || flag)
									{
										flag = false;
										this.AccountsOnVpn = 0;
										this.ChangeRegions(false);
										bool flag6 = false;
										if (Settings.Default.Making || Settings.Default.MakingPlusPosting)
										{
											if (text != this.REGION_MAKING)
											{
												bool flag7 = await this.ChangeDateTime(this.REGION_MAKING);
												flag6 = await this.ChangeRegion(this.REGION_MAKING, flag7);
												text = this.REGION_MAKING;
											}
										}
										else if (Settings.Default.Posting && text != this.REGION_POSTING)
										{
											flag6 = await this.ChangeRegion("United States", await this.ChangeDateTime((this.REGION_POSTING == "United States") ? "USA" : this.REGION_POSTING));
											text = this.REGION_POSTING;
										}
										if (flag6)
										{
											LogsUtils.WriteLog("[" + this.phoneUID + "]: Waiting 15 for apply settings");
											await Task.Delay(TimeSpan.FromSeconds(15.0));
											await this.device.wda.StartDriverOnly();
											num4 = 0;
											do
											{
												await this.device.CheckSession();
												int num5 = 0;
												try
												{
													await this.tiktokUtils.GoHome(true);
												}
												catch
												{
													num5 = 1;
												}
												num6 = num5;
												if (num6 != 1)
												{
													goto IL_1964;
												}
												LogsUtils.WriteLog("[" + this.phoneUID + "]: WDA Error");
												await Task.Delay(TimeSpan.FromSeconds(5.0));
												num6 = num4++;
											}
											while (num4 <= 5);
											goto IL_00C0;
										}
										IL_1964:
										LogsUtils.WriteLog("[" + this.phoneUID + "]: Turn Off Preferences");
										await this.device.TerminateApp("com.apple.Preferences");
										await this.tiktokUtils.GoHome(false);
										flag2 = false;
									}
									if ((flag4 && Settings.Default.ResetNetworkInRun) || (num >= Settings.Default.ResetNetworkAfter && Settings.Default.ResetNetworkAfter != 0))
									{
										flag4 = false;
										num = 0;
										this.RemoveTikTok();
										if (flag2)
										{
											bool flag8 = false;
											if (Settings.Default.Making || Settings.Default.MakingPlusPosting)
											{
												if (text != this.REGION_MAKING)
												{
													bool flag9 = await this.ChangeDateTime(this.REGION_MAKING);
													flag8 = await this.ChangeRegion(this.REGION_MAKING, flag9);
													text = this.REGION_MAKING;
												}
											}
											else if (Settings.Default.Posting && text != this.REGION_POSTING)
											{
												flag8 = await this.ChangeRegion("United States", await this.ChangeDateTime((this.REGION_POSTING == "United States") ? "USA" : this.REGION_POSTING));
												text = this.REGION_POSTING;
											}
											if (flag8)
											{
												LogsUtils.WriteLog("[" + this.phoneUID + "]: Waiting 15 for apply settings");
												await Task.Delay(TimeSpan.FromSeconds(15.0));
												await this.device.wda.StartDriverOnly();
												num4 = 0;
												do
												{
													await this.device.CheckSession();
													int num5 = 0;
													try
													{
														await this.tiktokUtils.GoHome(true);
													}
													catch
													{
														num5 = 1;
													}
													num6 = num5;
													if (num6 != 1)
													{
														goto IL_1F63;
													}
													LogsUtils.WriteLog("[" + this.phoneUID + "]: WDA Error");
													await Task.Delay(TimeSpan.FromSeconds(5.0));
													num6 = num4++;
												}
												while (num4 <= 5);
												goto IL_00C0;
											}
											IL_1F63:
											LogsUtils.WriteLog("[" + this.phoneUID + "]: Turn Off Preferences");
											await this.device.TerminateApp("com.apple.Preferences");
											await this.tiktokUtils.GoHome(false);
											try
											{
												await (await this.device.WaitForElementBy(0, "OK", 5.0)).Click(false);
											}
											catch
											{
											}
										}
										await this.ResetNetworkSettings();
										flag5 = false;
										flag3 = true;
										this.device.wda.StopAll();
										LogsUtils.WriteLog("[" + this.phoneUID + "]: Waiting 60 seconds for reset");
										await Task.Delay(TimeSpan.FromSeconds(60.0));
										await this.device.wda.StartWda(false);
										while (!this.device.wda.Runned)
										{
											LogsUtils.WriteLog("[" + this.phoneUID + "]: Waiting for WDA");
											await Task.Delay(TimeSpan.FromSeconds(5.0));
										}
										await this.device.CheckSession();
										await this.tiktokUtils.GoHome(true);
									}
									if (Settings.Default.RecordScreen)
									{
										await this.tiktokUtils.GoHome(false);
										LogsUtils.WriteLog("[" + this.phoneUID + "]: Checking for Screen Recording element");
										TaskAwaiter<Element> taskAwaiter3 = this.device.WaitForElementBy(3, "Screen recording in progress", 1.0).GetAwaiter();
										if (!taskAwaiter3.IsCompleted)
										{
											await taskAwaiter3;
											taskAwaiter3 = taskAwaiter4;
											taskAwaiter4 = default(TaskAwaiter<Element>);
											num2 = -1;
										}
										if (taskAwaiter3.GetResult() == null)
										{
											await this.ScreenRecorder(false, false);
										}
									}
									flag2 = true;
									bool flag10 = flag3;
									if (!flag10)
									{
										taskAwaiter = this.SearchForTikTokIcon(false).GetAwaiter();
										if (!taskAwaiter.IsCompleted)
										{
											await taskAwaiter;
											taskAwaiter = taskAwaiter2;
											taskAwaiter2 = default(TaskAwaiter<bool>);
											num2 = -1;
										}
										flag10 = !taskAwaiter.GetResult();
									}
									if (flag10)
									{
										flag3 = false;
										taskAwaiter = this.InstallTikTok().GetAwaiter();
										if (!taskAwaiter.IsCompleted)
										{
											await taskAwaiter;
											taskAwaiter = taskAwaiter2;
											taskAwaiter2 = default(TaskAwaiter<bool>);
											num2 = -1;
										}
										if (!taskAwaiter.GetResult())
										{
											flag3 = true;
											LogsUtils.WriteLog("[" + this.phoneUID + "]: Can't to install TikTok!");
											continue;
										}
									}
									if (Settings.Default.ConnectWIfi && !flag5)
									{
										taskAwaiter = this.TurnOnWifi().GetAwaiter();
										if (!taskAwaiter.IsCompleted)
										{
											await taskAwaiter;
											taskAwaiter = taskAwaiter2;
											taskAwaiter2 = default(TaskAwaiter<bool>);
											num2 = -1;
										}
										if (!taskAwaiter.GetResult())
										{
											flag3 = true;
											LogsUtils.WriteLog("[" + this.phoneUID + "]: Can't to turn on Wifi!");
											continue;
										}
										flag5 = true;
									}
									if (Settings.Default.UseVPN || Settings.Default.UseShadowRocket)
									{
										bool flag11 = false;
										for (;;)
										{
											if (Settings.Default.Making || Settings.Default.MakingPlusPosting)
											{
												flag11 = await this.vpn.TurnOnVpn(this.device, this.REGION_MAKING);
											}
											else if (Settings.Default.Posting)
											{
												flag11 = await this.vpn.TurnOnVpn(this.device, this.REGION_POSTING_VPN);
											}
											if (flag11)
											{
												break;
											}
											this.ChangeRegions(true);
										}
									}
									if (Settings.Default.Making || Settings.Default.MakingPlusPosting)
									{
										num4 = 0;
										try
										{
											try
											{
												Form1.StatusList[potok] = "Making Account";
											}
											catch
											{
											}
											AnyMessageClient anyMessageClient = new AnyMessageClient();
											object obj = this.phoneUID;
											this.WriteLog(string.Format("[{0}]: AnyMessage Balance: {1}", obj, await anyMessageClient.GetBalanceAsync()));
											obj = null;
											num6 = num++;
											int num5 = 0;
											try
											{
												bool flag12 = false;
												Making.MakeAccountResult makeAccountResult = await this.Maker.MakeAccount(anyMessageClient, flag12, this.tiktokUtils, this.PhoneModel);
												object obj2 = this.phoneUID;
												this.WriteLog(string.Format("[{0}]: AnyMessage Balance: {1}", obj2, await anyMessageClient.GetBalanceAsync()));
												obj2 = null;
												if (makeAccountResult == Making.MakeAccountResult.Success)
												{
													int i = 0;
													while (i < 2)
													{
														num6 = i++;
														TaskAwaiter<Worker.LoginResult> taskAwaiter5 = this.FillProfiile(anyMessageClient, this.REGION_MAKING, flag12).GetAwaiter();
														if (!taskAwaiter5.IsCompleted)
														{
															await taskAwaiter5;
															TaskAwaiter<Worker.LoginResult> taskAwaiter6;
															taskAwaiter5 = taskAwaiter6;
															taskAwaiter6 = default(TaskAwaiter<Worker.LoginResult>);
															num2 = -1;
														}
														switch (taskAwaiter5.GetResult())
														{
														case Worker.LoginResult.GoodAccount:
															this.WriteLog("[" + this.phoneUID + "]: Successfully did account");
															Interlocked.Increment(ref this.TotalGoodAccountss);
															goto IL_66B7;
														case Worker.LoginResult.BadAccount:
														{
															Form1.StatusList[potok] = "Error";
															Interlocked.Increment(ref this.TotalBadAccounts);
															this.WriteLog("[" + this.phoneUID + "]: Profile Filling Error || SIGNED || Some Error || SLOW");
															string text3 = "NO_BACKUP:UNKNOWN:{0}:{1}{2}:{3}:{4}:{5} :2FA {6}:Phone - {7} || Bad Account SLOW?";
															object[] array = new object[8];
															array[0] = ConstParams.CONST_PASSWORD;
															array[1] = (Settings.Default.emailDomain.Contains("long_") ? "long_" : "");
															array[2] = anyMessageClient.emailDataC.email;
															array[3] = this.REGION_MAKING;
															array[4] = anyMessageClient.emailDataC.id;
															array[5] = DateTime.Now;
															int num7 = 6;
															flag10 = Settings.Default.Use2FA;
															array[num7] = flag10.ToString();
															array[7] = this.phoneUID;
															string text4 = string.Format(text3, array);
															Interlocked.Increment(ref this.TotalSlowTimes);
															Form1.OutData.AccountsWithProblems.Enqueue(text4);
															try
															{
																await this.tiktokUtils.TerminateTikTok();
															}
															catch
															{
															}
															this.WriteLog("[" + this.phoneUID + "]: Checking WDA Session");
															taskAwaiter = this.device.CheckSession().GetAwaiter();
															if (!taskAwaiter.IsCompleted)
															{
																await taskAwaiter;
																taskAwaiter = taskAwaiter2;
																taskAwaiter2 = default(TaskAwaiter<bool>);
																num2 = -1;
															}
															if (!taskAwaiter.GetResult())
															{
																this.WriteLog("[" + this.phoneUID + "]: WDA OFFLINE");
															}
															try
															{
																await this.tiktokUtils.LogOutFromAccount(null, false, false);
															}
															catch
															{
															}
															if ((Settings.Default.UseVPN || Settings.Default.UseShadowRocket) && this.vpn.IsActivated)
															{
																await this.vpn.TurnOffVpn(this.device, this.tiktokUtils);
															}
															goto IL_8052;
														}
														case Worker.LoginResult.Risk:
														{
															Form1.StatusList[potok] = "Error";
															Interlocked.Increment(ref this.TotalBadAccounts);
															this.WriteLog("[" + this.phoneUID + "]: Profile Filling Error || Account Status || RISK");
															string text5 = "NO_BACKUP:UNKNOWN:{0}:{1}{2}:{3}:{4}:{5} :2FA {6}:Phone - {7} || SIGNED || RISK";
															object[] array2 = new object[8];
															array2[0] = ConstParams.CONST_PASSWORD;
															array2[1] = (Settings.Default.emailDomain.Contains("long_") ? "long_" : "");
															array2[2] = anyMessageClient.emailDataC.email;
															array2[3] = this.REGION_MAKING;
															array2[4] = anyMessageClient.emailDataC.id;
															array2[5] = DateTime.Now;
															int num8 = 6;
															flag10 = Settings.Default.Use2FA;
															array2[num8] = flag10.ToString();
															array2[7] = this.phoneUID;
															string text6 = string.Format(text5, array2);
															Form1.OutData.AccountsWithRiskOnly.Enqueue(text6);
															if ((Settings.Default.UseVPN || Settings.Default.UseShadowRocket) && this.vpn.IsActivated)
															{
																await this.vpn.TurnOffVpn(this.device, this.tiktokUtils);
															}
															try
															{
																await this.tiktokUtils.TerminateTikTok();
															}
															catch
															{
															}
															this.WriteLog("[" + this.phoneUID + "]: Checking WDA Session");
															taskAwaiter = this.device.CheckSession().GetAwaiter();
															if (!taskAwaiter.IsCompleted)
															{
																await taskAwaiter;
																taskAwaiter = taskAwaiter2;
																taskAwaiter2 = default(TaskAwaiter<bool>);
																num2 = -1;
															}
															if (!taskAwaiter.GetResult())
															{
																this.WriteLog("[" + this.phoneUID + "]: WDA OFFLINE");
															}
															goto IL_8052;
														}
														case Worker.LoginResult.Blocked:
														{
															Form1.StatusList[potok] = "Error";
															Interlocked.Increment(ref this.TotalBadAccounts);
															this.WriteLog("[" + this.phoneUID + "]: Profile Filling Error || BLOCKED");
															string text7 = "NO_BACKUP:UNKNOWN:{0}:{1}{2}:{3}:{4}:{5} :2FA {6}:Phone - {7} || SIGNED ||BLOCKED";
															object[] array3 = new object[8];
															array3[0] = ConstParams.CONST_PASSWORD;
															array3[1] = (Settings.Default.emailDomain.Contains("long_") ? "long_" : "");
															array3[2] = anyMessageClient.emailDataC.email;
															array3[3] = this.REGION_MAKING;
															array3[4] = anyMessageClient.emailDataC.id;
															array3[5] = DateTime.Now;
															int num9 = 6;
															flag10 = Settings.Default.Use2FA;
															array3[num9] = flag10.ToString();
															array3[7] = this.phoneUID;
															string text8 = string.Format(text7, array3);
															Form1.OutData.AccountsWithProblems.Enqueue(text8);
															try
															{
																await this.tiktokUtils.LogOutFromAccount(null, false, false);
															}
															catch
															{
															}
															if ((Settings.Default.UseVPN || Settings.Default.UseShadowRocket) && this.vpn.IsActivated)
															{
																await this.vpn.TurnOffVpn(this.device, this.tiktokUtils);
															}
															try
															{
																await this.tiktokUtils.TerminateTikTok();
															}
															catch
															{
															}
															this.WriteLog("[" + this.phoneUID + "]: Checking WDA Session");
															taskAwaiter = this.device.CheckSession().GetAwaiter();
															if (!taskAwaiter.IsCompleted)
															{
																await taskAwaiter;
																taskAwaiter = taskAwaiter2;
																taskAwaiter2 = default(TaskAwaiter<bool>);
																num2 = -1;
															}
															if (!taskAwaiter.GetResult())
															{
																this.WriteLog("[" + this.phoneUID + "]: WDA OFFLINE");
															}
															goto IL_8052;
														}
														case Worker.LoginResult.Exception:
														{
															this.WriteLog("[" + this.phoneUID + "]: Profile Filling Error || EXCEPTION");
															string text9 = "NO_BACKUP:UNKNOWN:{0}:{1}{2}:{3}:{4}:{5} :2FA {6}:Phone - {7} || SIGNED || EXCEPTION";
															object[] array4 = new object[8];
															array4[0] = ConstParams.CONST_PASSWORD;
															array4[1] = (Settings.Default.emailDomain.Contains("long_") ? "long_" : "");
															array4[2] = anyMessageClient.emailDataC.email;
															array4[3] = this.REGION_MAKING;
															array4[4] = anyMessageClient.emailDataC.id;
															array4[5] = DateTime.Now;
															int num10 = 6;
															flag10 = Settings.Default.Use2FA;
															array4[num10] = flag10.ToString();
															array4[7] = this.phoneUID;
															string text10 = string.Format(text9, array4);
															Form1.OutData.AccountsWithProblems.Enqueue(text10);
															for (;;)
															{
																try
																{
																	await this.tiktokUtils.TerminateTikTok();
																}
																catch
																{
																}
																this.WriteLog("[" + this.phoneUID + "]: Checking WDA Session");
																taskAwaiter = this.device.CheckSession().GetAwaiter();
																if (!taskAwaiter.IsCompleted)
																{
																	await taskAwaiter;
																	taskAwaiter = taskAwaiter2;
																	taskAwaiter2 = default(TaskAwaiter<bool>);
																	num2 = -1;
																}
																if (taskAwaiter.GetResult())
																{
																	break;
																}
																this.WriteLog("[" + this.phoneUID + "]: WDA OFFLINE");
																await Task.Delay(10000);
																this.WriteLog("[" + this.phoneUID + "]: WDA First....Q");
																if (this.device.Started)
																{
																	break;
																}
																this.WriteLog("[" + this.phoneUID + "]: WDA OFFLINE");
																await Task.Delay(10000);
															}
															break;
														}
														default:
															goto IL_454A;
														}
													}
													Form1.StatusList[potok] = "Error";
													Interlocked.Increment(ref this.TotalBadAccounts);
													this.WriteLog("[" + this.phoneUID + "]: Sign Up Error T2");
													try
													{
														await this.tiktokUtils.TerminateTikTok();
													}
													catch
													{
													}
													this.WriteLog("[" + this.phoneUID + "]: Checking WDA Session");
													taskAwaiter = this.device.CheckSession().GetAwaiter();
													if (!taskAwaiter.IsCompleted)
													{
														await taskAwaiter;
														taskAwaiter = taskAwaiter2;
														taskAwaiter2 = default(TaskAwaiter<bool>);
														num2 = -1;
													}
													if (!taskAwaiter.GetResult())
													{
														this.WriteLog("[" + this.phoneUID + "]: WDA OFFLINE");
														continue;
													}
													try
													{
														await this.tiktokUtils.LogOutFromAccount(null, false, false);
													}
													catch
													{
													}
													if ((Settings.Default.UseVPN || Settings.Default.UseShadowRocket) && this.vpn.IsActivated)
													{
														await this.vpn.TurnOffVpn(this.device, this.tiktokUtils);
													}
													if (Settings.Default.RecordScreen)
													{
														await this.tiktokUtils.GoHome(false);
														LogsUtils.WriteLog("[" + this.phoneUID + "]: Checking for Screen Recording element");
														TaskAwaiter<Element> taskAwaiter3 = this.device.WaitForElementBy(3, "Screen recording in progress", 1.0).GetAwaiter();
														if (!taskAwaiter3.IsCompleted)
														{
															await taskAwaiter3;
															taskAwaiter3 = taskAwaiter4;
															taskAwaiter4 = default(TaskAwaiter<Element>);
															num2 = -1;
														}
														if (taskAwaiter3.GetResult() != null)
														{
															await this.ScreenRecorder(true, true);
														}
													}
													continue;
												}
												else
												{
													await this.tiktokUtils.SaveScreenShot(true, TikTokUtils.ScreenshotType.Makign);
													await this.tiktokUtils.TerminateTikTok();
													await this.tiktokUtils.GoHome(false);
													switch (makeAccountResult)
													{
													case Making.MakeAccountResult.Unsuccess:
													{
														this.WriteLog("[" + this.phoneUID + "]: Sign Up Error || NOT SIGNED || Status Unsuccess");
														string text11 = "NO_BACKUP:UNKNOWN:{0}:{1}{2}:{3}:{4}:{5} :2FA {6}:Phone - {7} || NOT SIGNED | MAKE UNSUCCESS";
														object[] array5 = new object[8];
														array5[0] = ConstParams.CONST_PASSWORD;
														array5[1] = (Settings.Default.emailDomain.Contains("long_") ? "long_" : "");
														array5[2] = anyMessageClient.emailDataC.email;
														array5[3] = this.REGION_MAKING;
														array5[4] = anyMessageClient.emailDataC.id;
														array5[5] = DateTime.Now;
														int num11 = 6;
														flag10 = Settings.Default.Use2FA;
														array5[num11] = flag10.ToString();
														array5[7] = this.phoneUID;
														string text12 = string.Format(text11, array5);
														Form1.OutData.AccountsWithProblems.Enqueue(text12);
														break;
													}
													case Making.MakeAccountResult.Maximum:
													{
														this.WriteLog("[" + this.phoneUID + "]: Sign Up Error || NOT SIGNED || Maximum Error");
														string text13 = "NO_BACKUP:UNKNOWN:{0}:{1}{2}:{3}:{4}:{5} :2FA {6}:Phone - {7} || NOT SIGNED | Maximum Error";
														object[] array6 = new object[8];
														array6[0] = ConstParams.CONST_PASSWORD;
														array6[1] = (Settings.Default.emailDomain.Contains("long_") ? "long_" : "");
														array6[2] = anyMessageClient.emailDataC.email;
														array6[3] = this.REGION_MAKING;
														array6[4] = anyMessageClient.emailDataC.id;
														array6[5] = DateTime.Now;
														int num12 = 6;
														flag10 = Settings.Default.Use2FA;
														array6[num12] = flag10.ToString();
														array6[7] = this.phoneUID;
														string text14 = string.Format(text13, array6);
														Form1.OutData.AccountsWithProblems.Enqueue(text14);
														break;
													}
													case Making.MakeAccountResult.Exception:
													{
														this.WriteLog("[" + this.phoneUID + "]: Sign Up Error || NOT SIGNED || Exception Error");
														string text15 = "NO_BACKUP:UNKNOWN:{0}:{1}{2}:{3}:{4}:{5} :2FA {6}:Phone - {7} || NOT SIGNED | Exception";
														object[] array7 = new object[8];
														array7[0] = ConstParams.CONST_PASSWORD;
														array7[1] = (Settings.Default.emailDomain.Contains("long_") ? "long_" : "");
														array7[2] = anyMessageClient.emailDataC.email;
														array7[3] = this.REGION_MAKING;
														array7[4] = anyMessageClient.emailDataC.id;
														array7[5] = DateTime.Now;
														int num13 = 6;
														flag10 = Settings.Default.Use2FA;
														array7[num13] = flag10.ToString();
														array7[7] = this.phoneUID;
														string text16 = string.Format(text15, array7);
														Form1.OutData.AccountsWithProblems.Enqueue(text16);
														break;
													}
													case Making.MakeAccountResult.VerifyException:
													{
														this.WriteLog("[" + this.phoneUID + "]: Sign Up Error || NOT SIGNED || VerifyException Error");
														string text17 = "NO_BACKUP:UNKNOWN:{0}:{1}{2}:{3}:{4}:{5} :2FA {6}:Phone - {7} || NOT SIGNED | Verify Exception - NO SMS CODE";
														object[] array8 = new object[8];
														array8[0] = ConstParams.CONST_PASSWORD;
														array8[1] = (Settings.Default.emailDomain.Contains("long_") ? "long_" : "");
														array8[2] = anyMessageClient.emailDataC.email;
														array8[3] = this.REGION_MAKING;
														array8[4] = anyMessageClient.emailDataC.id;
														array8[5] = DateTime.Now;
														int num14 = 6;
														flag10 = Settings.Default.Use2FA;
														array8[num14] = flag10.ToString();
														array8[7] = this.phoneUID;
														string text18 = string.Format(text17, array8);
														Form1.OutData.AccountsWithProblems.Enqueue(text18);
														break;
													}
													case Making.MakeAccountResult.RISK:
													{
														this.WriteLog("[" + this.phoneUID + "]: Sign Up Error || NOT SIGNED || RISK Error");
														string text19 = "NO_BACKUP:UNKNOWN:{0}:{1}{2}:{3}:{4}:{5} :2FA {6}:Phone - {7} || NOT SIGNED | RISK";
														object[] array9 = new object[8];
														array9[0] = ConstParams.CONST_PASSWORD;
														array9[1] = (Settings.Default.emailDomain.Contains("long_") ? "long_" : "");
														array9[2] = anyMessageClient.emailDataC.email;
														array9[3] = this.REGION_MAKING;
														array9[4] = anyMessageClient.emailDataC.id;
														array9[5] = DateTime.Now;
														int num15 = 6;
														flag10 = Settings.Default.Use2FA;
														array9[num15] = flag10.ToString();
														array9[7] = this.phoneUID;
														string text20 = string.Format(text19, array9);
														Form1.OutData.AccountsWithRiskOnly.Enqueue(text20);
														break;
													}
													case Making.MakeAccountResult.SomethingWrong:
													{
														this.WriteLog("[" + this.phoneUID + "]: Sign Up Error || NOT SIGNED || SomethingWrong Error");
														string text21 = "NO_BACKUP:UNKNOWN:{0}:{1}{2}:{3}:{4}:{5} :2FA {6}:Phone - {7} || NOT SIGNED | SomethingWrong";
														object[] array10 = new object[8];
														array10[0] = ConstParams.CONST_PASSWORD;
														array10[1] = (Settings.Default.emailDomain.Contains("long_") ? "long_" : "");
														array10[2] = anyMessageClient.emailDataC.email;
														array10[3] = this.REGION_MAKING;
														array10[4] = anyMessageClient.emailDataC.id;
														array10[5] = DateTime.Now;
														int num16 = 6;
														flag10 = Settings.Default.Use2FA;
														array10[num16] = flag10.ToString();
														array10[7] = this.phoneUID;
														string text22 = string.Format(text21, array10);
														Form1.OutData.AccountsWithProblems.Enqueue(text22);
														break;
													}
													}
													Form1.StatusList[potok] = "Error";
													Interlocked.Increment(ref this.TotalBadAccounts);
													this.WriteLog("[" + this.phoneUID + "]: Sign Up Error Make Result");
													try
													{
														await anyMessageClient.CancelEmailAsync(anyMessageClient.emailDataC.id);
													}
													catch
													{
													}
													if ((Settings.Default.UseVPN || Settings.Default.UseShadowRocket) && this.vpn.IsActivated)
													{
														await this.vpn.TurnOffVpn(this.device, this.tiktokUtils);
													}
													try
													{
														await this.tiktokUtils.TerminateTikTok();
													}
													catch
													{
													}
													this.WriteLog("[" + this.phoneUID + "]: Checking WDA Session");
													taskAwaiter = this.device.CheckSession().GetAwaiter();
													if (!taskAwaiter.IsCompleted)
													{
														await taskAwaiter;
														taskAwaiter = taskAwaiter2;
														taskAwaiter2 = default(TaskAwaiter<bool>);
														num2 = -1;
													}
													if (!taskAwaiter.GetResult())
													{
														this.WriteLog("[" + this.phoneUID + "]: WDA OFFLINE");
														continue;
													}
													if (Settings.Default.RecordScreen)
													{
														await this.tiktokUtils.GoHome(false);
														LogsUtils.WriteLog("[" + this.phoneUID + "]: Checking for Screen Recording element");
														TaskAwaiter<Element> taskAwaiter3 = this.device.WaitForElementBy(3, "Screen recording in progress", 1.0).GetAwaiter();
														if (!taskAwaiter3.IsCompleted)
														{
															await taskAwaiter3;
															taskAwaiter3 = taskAwaiter4;
															taskAwaiter4 = default(TaskAwaiter<Element>);
															num2 = -1;
														}
														if (taskAwaiter3.GetResult() != null)
														{
															await this.ScreenRecorder(true, true);
														}
													}
													continue;
												}
											}
											catch (Exception obj)
											{
												num5 = 1;
											}
											IL_454A:
											num6 = num5;
											if (num6 == 1)
											{
												Exception ex2 = (Exception)obj;
												try
												{
													await anyMessageClient.CancelEmailAsync(anyMessageClient.emailDataC.id);
												}
												catch
												{
												}
												throw ex2;
											}
											obj = null;
											anyMessageClient = null;
										}
										catch (Exception obj3)
										{
											num4 = 1;
										}
										num6 = num4;
										object obj3;
										if (num6 == 1)
										{
											Exception ex3 = (Exception)obj3;
											this.WriteLog("[" + this.phoneUID + "]: Critical Exception Q: " + ex3.ToString());
											Form1.StatusList[potok] = "Error!";
											try
											{
												await this.tiktokUtils.TerminateTikTok();
											}
											catch
											{
											}
											if ((Settings.Default.UseVPN || Settings.Default.UseShadowRocket) && this.vpn.IsActivated)
											{
												await this.vpn.TurnOffVpn(this.device, this.tiktokUtils);
											}
											if (Settings.Default.RecordScreen)
											{
												await this.tiktokUtils.GoHome(false);
												LogsUtils.WriteLog("[" + this.phoneUID + "]: Checking for Screen Recording element");
												TaskAwaiter<Element> taskAwaiter3 = this.device.WaitForElementBy(3, "Screen recording in progress", 1.0).GetAwaiter();
												if (!taskAwaiter3.IsCompleted)
												{
													await taskAwaiter3;
													taskAwaiter3 = taskAwaiter4;
													taskAwaiter4 = default(TaskAwaiter<Element>);
													num2 = -1;
												}
												if (taskAwaiter3.GetResult() != null)
												{
													await this.ScreenRecorder(true, true);
												}
											}
											this.WriteLog("[" + this.phoneUID + "]: Checking WDA Session");
											taskAwaiter = this.device.CheckSession().GetAwaiter();
											if (!taskAwaiter.IsCompleted)
											{
												await taskAwaiter;
												taskAwaiter = taskAwaiter2;
												taskAwaiter2 = default(TaskAwaiter<bool>);
												num2 = -1;
											}
											if (!taskAwaiter.GetResult())
											{
												this.WriteLog("[" + this.phoneUID + "]: WDA OFFLINE");
												continue;
											}
										}
										obj3 = null;
									}
									else if (Settings.Default.Posting)
									{
										string text23 = this.phoneUID;
										num6 = 0;
										try
										{
											object obj4;
											if (Form1.OutData.ReworkAccounts.Count == 0)
											{
												Form1.StatusList[potok] = "Don't have Accounts for POSTING";
												LogsUtils.WriteLog("[" + this.phoneUID + "]: Don't have Accounts for POSTING");
												obj4 = Worker.sync;
												lock (obj4)
												{
													MessageBox.Show("Don't have Accounts for POSTING?");
												}
												this.Started = false;
												return;
											}
											bool flag12 = false;
											for (;;)
											{
												IL_4AA4:
												Form1.StatusList[potok] = "Signing to Account";
												num++;
												if (num > Settings.Default.ResetNetworkAfter)
												{
													break;
												}
												while (Form1.OutData.ReworkAccounts.Count != 0)
												{
													try
													{
														try
														{
															text2 = Form1.OutData.ReworkAccounts[this.phoneUID][0];
														}
														catch
														{
															this.phoneUID = "NO_BACKUP";
															text2 = Form1.OutData.ReworkAccounts[this.phoneUID][0];
														}
														Form1.OutData.ReworkAccounts[this.phoneUID].RemoveAt(0);
													}
													catch
													{
														Form1.StatusList[potok] = "Don't have Accounts for POSTING?";
														LogsUtils.WriteLog("[" + this.phoneUID + "]: Don't have Accounts for POSTING?");
														obj4 = Worker.sync;
														flag10 = false;
														try
														{
															Monitor.Enter(obj4, ref flag10);
															MessageBox.Show("Don't have Accounts for POSTING?");
														}
														finally
														{
															if (num2 < 0 && flag10)
															{
																Monitor.Exit(obj4);
															}
														}
														continue;
													}
													this.phoneUID = text23;
													string[] array11 = text2.Split(new char[] { ':' });
													string text24 = array11[1];
													string text25 = array11[2];
													string text26 = array11[3];
													string text27 = array11[5];
													if (text27 == "US")
													{
														text27 = array11[6];
													}
													bool flag13 = false;
													if (text26.Contains("long_"))
													{
														flag13 = true;
														text26 = text26.Replace("long_", "");
													}
													AnyMessageClient anyMessageClient = new AnyMessageClient();
													if (flag13)
													{
														anyMessageClient.ReorderLL(text27, text26);
													}
													else
													{
														try
														{
															this.WriteLog("[" + this.phoneUID + "]: Getting AnyMessage Reorder");
															await anyMessageClient.ReorderByIdAsync(text27, null, null);
														}
														catch (Exception)
														{
															if (text2.Contains("2FA False"))
															{
																Form1.OutData.ReWorkNotSigned.Enqueue(text2 + " EMAIL IS BLOCKED || CHECK BEFORE!");
																Interlocked.Increment(ref this.TotalBadAccounts);
																this.WriteLog(string.Concat(new string[] { "[", this.phoneUID, "]: ", text2, " EMAIL IS BLOCKED || CHECK BEFORE!" }));
																continue;
															}
														}
													}
													object obj3;
													try
													{
														obj3 = this.phoneUID;
														this.WriteLog(string.Format("[{0}]: AnyMessage Balance: {1}", obj3, await anyMessageClient.GetBalanceAsync()));
														obj3 = null;
													}
													catch
													{
														this.WriteLog("[" + this.phoneUID + "]: AnyMessage Balance Exception");
													}
													SignIn.SignResult signResult = await this.Signing.OnlySignIn(text24, text26, text25, text27, this.tiktokUtils, flag13, anyMessageClient, text2);
													obj3 = this.phoneUID;
													this.WriteLog(string.Format("[{0}]: AnyMessage Balance: {1}", obj3, await anyMessageClient.GetBalanceAsync()));
													obj3 = null;
													if (signResult == SignIn.SignResult.Success)
													{
														goto Block_224;
													}
													await this.tiktokUtils.SaveScreenShot(true, TikTokUtils.ScreenshotType.Makign);
													await this.tiktokUtils.TerminateTikTok();
													await this.tiktokUtils.GoHome(false);
													switch (signResult)
													{
													case SignIn.SignResult.Unsuccess:
														goto IL_5B54;
													case SignIn.SignResult.Exception:
														this.WriteLog("[" + this.phoneUID + "]: Something wrong, will try again later");
														Form1.OutData.ReWorkNotSigned.Enqueue(text2 + " SOME EXCEPTION");
														Form1.OutData.ReworkAccounts[this.phoneUID].Add(text2);
														await this.tiktokUtils.TerminateTikTok();
														this.WriteLog("[" + this.phoneUID + "]: Checking WDA Session");
														taskAwaiter = this.device.CheckSession().GetAwaiter();
														if (!taskAwaiter.IsCompleted)
														{
															await taskAwaiter;
															taskAwaiter = taskAwaiter2;
															taskAwaiter2 = default(TaskAwaiter<bool>);
															num2 = -1;
														}
														if (!taskAwaiter.GetResult())
														{
															goto Block_242;
														}
														if (!flag12)
														{
															flag12 = true;
															goto IL_4AA4;
														}
														break;
													case SignIn.SignResult.CaptchaError:
														this.WriteLog("[" + this.phoneUID + "]: Something wrong with Captcha, will try again later");
														Form1.OutData.ReWorkNotSigned.Enqueue(text2 + " CAPTCHA EXCEPTION");
														Form1.OutData.ReworkAccounts[this.phoneUID].Add(text2);
														await this.tiktokUtils.TerminateTikTok();
														taskAwaiter = this.device.CheckSession().GetAwaiter();
														if (!taskAwaiter.IsCompleted)
														{
															await taskAwaiter;
															taskAwaiter = taskAwaiter2;
															taskAwaiter2 = default(TaskAwaiter<bool>);
															num2 = -1;
														}
														if (!taskAwaiter.GetResult())
														{
															goto Block_245;
														}
														if (!flag12)
														{
															flag12 = true;
															goto IL_4AA4;
														}
														break;
													case SignIn.SignResult.Maximum:
														goto IL_5C00;
													case SignIn.SignResult.Blocked:
														goto IL_5D11;
													case SignIn.SignResult.VerifyException:
														goto IL_5CB6;
													case SignIn.SignResult.FA2Exception:
														goto IL_5C5B;
													case SignIn.SignResult.EmailBlocked:
														goto IL_5D6C;
													case SignIn.SignResult.TimeOut:
														goto IL_5BA5;
													}
													goto Block_240;
												}
												goto Block_216;
											}
											if ((Settings.Default.UseVPN || Settings.Default.UseShadowRocket) && !this.vpn.IsActivated)
											{
												for (;;)
												{
													taskAwaiter = this.vpn.TurnOnVpn(this.device, this.REGION_POSTING_VPN).GetAwaiter();
													if (!taskAwaiter.IsCompleted)
													{
														await taskAwaiter;
														taskAwaiter = taskAwaiter2;
														taskAwaiter2 = default(TaskAwaiter<bool>);
														num2 = -1;
													}
													if (taskAwaiter.GetResult())
													{
														break;
													}
													this.ChangeRegions(true);
												}
											}
											if (Settings.Default.RecordScreen)
											{
												await this.tiktokUtils.GoHome(false);
												LogsUtils.WriteLog("[" + this.phoneUID + "]: Checking for Screen Recording element");
												TaskAwaiter<Element> taskAwaiter3 = this.device.WaitForElementBy(3, "Screen recording in progress", 1.0).GetAwaiter();
												if (!taskAwaiter3.IsCompleted)
												{
													await taskAwaiter3;
													taskAwaiter3 = taskAwaiter4;
													taskAwaiter4 = default(TaskAwaiter<Element>);
													num2 = -1;
												}
												if (taskAwaiter3.GetResult() != null)
												{
													await this.ScreenRecorder(true, false);
												}
											}
											continue;
											Block_216:
											Form1.StatusList[potok] = "Don't have Accounts for POSTING";
											LogsUtils.WriteLog("[" + this.phoneUID + "]: Don't have Accounts for POSTING");
											obj4 = Worker.sync;
											lock (obj4)
											{
												MessageBox.Show("Don't have Accounts for POSTING?");
											}
											this.Started = false;
											return;
											Block_224:
											this.WriteLog("[" + this.phoneUID + "]: Successfully logged to account");
											Form1.OutData.ReWorkSignedOk.Enqueue(text2);
											Interlocked.Increment(ref this.TotalGoodAccountss);
											if (Settings.Default.ClearBioAfterLogin)
											{
												this.WriteLog("[" + this.phoneUID + "]: Clear Bio Feature");
												TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter7 = this.tiktokUtils.ActivateTikTokGotoProfile().GetAwaiter();
												if (!taskAwaiter7.IsCompleted)
												{
													await taskAwaiter7;
													taskAwaiter7 = taskAwaiter8;
													taskAwaiter8 = default(TaskAwaiter<TikTokUtils.ActivateResult>);
													num2 = -1;
												}
												if (taskAwaiter7.GetResult() != TikTokUtils.ActivateResult.Success)
												{
													this.WriteLog("[" + this.phoneUID + "]: Activate Profile Error");
													goto IL_7B67;
												}
												await this.device.ConfigurateAppium(21);
												num4 = 0;
												try
												{
													await (await this.device.WaitForElementBy(0, "Edit", 5.0)).Click(false);
												}
												catch
												{
													num4 = 1;
												}
												if (num4 == 1)
												{
													try
													{
														await (await this.device.WaitForElementBy(0, "user_info_manage_edit_profile", 5.0)).Click(false);
													}
													catch
													{
													}
												}
												taskAwaiter = this.ClearBio().GetAwaiter();
												if (!taskAwaiter.IsCompleted)
												{
													await taskAwaiter;
													taskAwaiter = taskAwaiter2;
													taskAwaiter2 = default(TaskAwaiter<bool>);
													num2 = -1;
												}
												if (!taskAwaiter.GetResult())
												{
													this.WriteLog("[" + this.phoneUID + "]: Clear bio unsuccess");
													Form1.OutData.ReWorkSignedOk.Enqueue(text2 + " Bio Not Cleared - SLOW");
													Interlocked.Increment(ref this.TotalSlowTimes);
													goto IL_7B67;
												}
												this.WriteLog("[" + this.phoneUID + "]: Clear bio success");
											}
											if ((Settings.Default.MentionInBio || Settings.Default.UpdateBio) && Settings.Default.BioAfterCreating)
											{
												this.WriteLog("[" + this.phoneUID + "]: Update bio Feature || AL");
												TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter7 = this.tiktokUtils.ActivateTikTokGotoProfile().GetAwaiter();
												if (!taskAwaiter7.IsCompleted)
												{
													await taskAwaiter7;
													taskAwaiter7 = taskAwaiter8;
													taskAwaiter8 = default(TaskAwaiter<TikTokUtils.ActivateResult>);
													num2 = -1;
												}
												if (taskAwaiter7.GetResult() != TikTokUtils.ActivateResult.Success)
												{
													this.WriteLog("[" + this.phoneUID + "]: Activate Profile Error");
													goto IL_7B67;
												}
												await this.device.ConfigurateAppium(21);
												this.WriteLog("[" + this.phoneUID + "]: Click Edit Button");
												num4 = 0;
												try
												{
													await (await this.device.WaitForElementBy(0, "Edit", 5.0)).Click(false);
												}
												catch
												{
													num4 = 1;
												}
												if (num4 == 1)
												{
													try
													{
														await (await this.device.WaitForElementBy(0, "user_info_manage_edit_profile", 5.0)).Click(false);
													}
													catch
													{
													}
												}
												taskAwaiter = this.UpdateBio(false).GetAwaiter();
												if (!taskAwaiter.IsCompleted)
												{
													await taskAwaiter;
													taskAwaiter = taskAwaiter2;
													taskAwaiter2 = default(TaskAwaiter<bool>);
													num2 = -1;
												}
												if (!taskAwaiter.GetResult())
												{
													this.WriteLog("[" + this.phoneUID + "]: Mention in bio unsuccess");
													Form1.OutData.ReWorkSignedOk.Enqueue(text2 + " Bio Not Updated - SLOW");
													Interlocked.Increment(ref this.TotalSlowTimes);
													goto IL_7B67;
												}
												this.WriteLog("[" + this.phoneUID + "]: Mention in bio success");
											}
											goto IL_66B7;
											Block_240:
											goto IL_6087;
											IL_5B54:
											Form1.StatusList[potok] = "Error";
											Form1.OutData.ReWorkNotSigned.Enqueue(text2);
											Interlocked.Increment(ref this.TotalBadAccounts);
											this.WriteLog("[" + this.phoneUID + "]: Sign In Error");
											goto IL_6087;
											IL_5BA5:
											Form1.StatusList[potok] = "TimeOut Error";
											Form1.OutData.ReWorkNotSigned.Enqueue(text2 + " TimeOut ERROR");
											Interlocked.Increment(ref this.TotalBadAccounts);
											this.WriteLog("[" + this.phoneUID + "]: TimeOut Sign Up Error");
											goto IL_6087;
											IL_5C00:
											Form1.StatusList[potok] = "Error";
											Form1.OutData.ReWorkNotSigned.Enqueue(text2 + " MAXIMUM ERROR");
											Interlocked.Increment(ref this.TotalBadAccounts);
											this.WriteLog("[" + this.phoneUID + "]: Maximum Error");
											goto IL_6087;
											IL_5C5B:
											Form1.StatusList[potok] = "Error";
											Form1.OutData.ReWorkNotSigned.Enqueue(text2 + " 2FA Exception");
											Interlocked.Increment(ref this.TotalBadAccounts);
											this.WriteLog("[" + this.phoneUID + "]: 2FA Exception");
											goto IL_6087;
											IL_5CB6:
											Form1.StatusList[potok] = "Error";
											Form1.OutData.ReWorkNotSigned.Enqueue(text2 + " SMS EMAIL ERROR");
											Interlocked.Increment(ref this.TotalBadAccounts);
											this.WriteLog("[" + this.phoneUID + "]: SMS EMAIL ERROR");
											goto IL_6087;
											IL_5D11:
											Form1.StatusList[potok] = "Blocked";
											Form1.OutData.ReWorkNotSigned.Enqueue(text2 + " ACCOUNT BLOCKED");
											Interlocked.Increment(ref this.TotalBadAccounts);
											this.WriteLog("[" + this.phoneUID + "]: ACCOUNT BLOCKED");
											goto IL_6087;
											IL_5D6C:
											Form1.StatusList[potok] = "Email Blocked";
											Form1.OutData.ReWorkNotSigned.Enqueue(text2 + " EMAIL BLOCKED");
											Interlocked.Increment(ref this.TotalBadAccounts);
											this.WriteLog("[" + this.phoneUID + "]: EMAIL BLOCKED");
											goto IL_6087;
											Block_242:
											this.WriteLog("[" + this.phoneUID + "]: WDA OFFLINE");
											continue;
											Block_245:
											this.WriteLog("[" + this.phoneUID + "]: WDA OFFLINE");
											continue;
											IL_6087:
											if ((Settings.Default.UseVPN || Settings.Default.UseShadowRocket) && this.vpn.IsActivated)
											{
												await this.vpn.TurnOffVpn(this.device, this.tiktokUtils);
											}
											if (Settings.Default.RecordScreen)
											{
												LogsUtils.WriteLog("[" + this.phoneUID + "]: Checking for Screen Recording element");
												TaskAwaiter<Element> taskAwaiter3 = this.device.WaitForElementBy(3, "Screen recording in progress", 1.0).GetAwaiter();
												if (!taskAwaiter3.IsCompleted)
												{
													await taskAwaiter3;
													taskAwaiter3 = taskAwaiter4;
													taskAwaiter4 = default(TaskAwaiter<Element>);
													num2 = -1;
												}
												if (taskAwaiter3.GetResult() != null)
												{
													await this.ScreenRecorder(true, true);
												}
											}
											await this.tiktokUtils.TerminateTikTok();
											this.WriteLog("[" + this.phoneUID + "]: Checking WDA Session");
											taskAwaiter = this.device.CheckSession().GetAwaiter();
											if (!taskAwaiter.IsCompleted)
											{
												await taskAwaiter;
												taskAwaiter = taskAwaiter2;
												taskAwaiter2 = default(TaskAwaiter<bool>);
												num2 = -1;
											}
											if (!taskAwaiter.GetResult())
											{
												this.WriteLog("[" + this.phoneUID + "]: WDA OFFLINE");
											}
											continue;
										}
										catch (Exception obj5)
										{
											num6 = 1;
										}
										if (num6 == 1)
										{
											object obj5;
											Exception ex2 = (Exception)obj5;
											this.phoneUID = text23;
											if ((Settings.Default.UseVPN || Settings.Default.UseShadowRocket) && this.vpn.IsActivated)
											{
												await this.vpn.TurnOffVpn(this.device, this.tiktokUtils);
											}
											if (Settings.Default.RecordScreen)
											{
												await this.tiktokUtils.GoHome(false);
												LogsUtils.WriteLog("[" + this.phoneUID + "]: Checking for Screen Recording element");
												TaskAwaiter<Element> taskAwaiter3 = this.device.WaitForElementBy(3, "Screen recording in progress", 1.0).GetAwaiter();
												if (!taskAwaiter3.IsCompleted)
												{
													await taskAwaiter3;
													taskAwaiter3 = taskAwaiter4;
													taskAwaiter4 = default(TaskAwaiter<Element>);
													num2 = -1;
												}
												if (taskAwaiter3.GetResult() != null)
												{
													await this.ScreenRecorder(true, true);
												}
											}
											Form1.StatusList[potok] = "Error!";
											this.WriteLog("[" + this.phoneUID + "]: Critical Exception: " + ex2.ToString());
											try
											{
												await this.tiktokUtils.TerminateTikTok();
											}
											catch
											{
											}
											this.WriteLog("[" + this.phoneUID + "]: Checking WDA Session");
											taskAwaiter = this.device.CheckSession().GetAwaiter();
											if (!taskAwaiter.IsCompleted)
											{
												await taskAwaiter;
												taskAwaiter = taskAwaiter2;
												taskAwaiter2 = default(TaskAwaiter<bool>);
												num2 = -1;
											}
											if (!taskAwaiter.GetResult())
											{
												this.WriteLog("[" + this.phoneUID + "]: WDA OFFLINE");
												continue;
											}
											ex2 = null;
										}
										text23 = null;
									}
								}
								IL_66B7:
								this.Poster.VideoFolder = null;
								if (!Settings.Default.Posting && !Settings.Default.MakingPlusPosting)
								{
									try
									{
										await this.tiktokUtils.GoHome(false);
										LogsUtils.WriteLog("[" + this.phoneUID + "]: Checking for Screen Recording element");
										TaskAwaiter<Element> taskAwaiter3 = this.device.WaitForElementBy(3, "Screen recording in progress", 1.0).GetAwaiter();
										if (!taskAwaiter3.IsCompleted)
										{
											await taskAwaiter3;
											taskAwaiter3 = taskAwaiter4;
											taskAwaiter4 = default(TaskAwaiter<Element>);
											num2 = -1;
										}
										if (taskAwaiter3.GetResult() != null)
										{
											await this.ScreenRecorder(true, false);
										}
										goto IL_7B67;
									}
									catch (Exception ex4)
									{
										LogsUtils.WriteLog("[" + this.phoneUID + "]: Exception C: " + ex4.ToString());
										goto IL_7B67;
									}
								}
								if ((Settings.Default.UseVPN || Settings.Default.UseShadowRocket) && !this.vpn.IsActivated)
								{
									for (;;)
									{
										taskAwaiter = this.vpn.TurnOnVpn(this.device, this.REGION_POSTING_VPN).GetAwaiter();
										if (!taskAwaiter.IsCompleted)
										{
											await taskAwaiter;
											taskAwaiter = taskAwaiter2;
											taskAwaiter2 = default(TaskAwaiter<bool>);
											num2 = -1;
										}
										if (taskAwaiter.GetResult())
										{
											break;
										}
										this.ChangeRegions(true);
									}
								}
								await this.tiktokUtils.GoHome(false);
								await this.tiktokUtils.TerminateTikTok();
								this.UsedMentions = new List<string>();
								num4 = 0;
								try
								{
									if (Settings.Default.AddMusic)
									{
										Form1.StatusList[potok] = "Add Music";
										this.WriteLog("[" + this.phoneUID + "]: Add Music to Favorites");
										await this.OpenMusic(Form1.Settings.MusicUrls[ConstParams.rand.Next(Form1.Settings.MusicUrls.Count)]);
										await this.tiktokUtils.GoHome(true);
									}
									int num5 = 0;
									int i = 0;
									int j = 0;
									bool flag13 = true;
									bool flag12 = false;
									bool flag14 = false;
									while (j < Settings.Default.NeedToPost)
									{
										flag14 = false;
										List<string> statusList = Form1.StatusList;
										string text28 = "Posting Trying: ";
										num6 = j + 1;
										statusList[potok] = text28 + num6.ToString();
										Poster.PostingResult postingResult = await this.Poster.PostVideo(this.phoneUID, this.originalVideoUrl, this.originalPictureUrl, flag13, this.UsedMentions, this.tiktokUtils, flag12);
										num6 = j++;
										flag13 = false;
										switch (postingResult)
										{
										case Poster.PostingResult.Success:
											flag14 = true;
											this.WriteLog("[" + this.phoneUID + "]: Success posted");
											Interlocked.Increment(ref this.TotalPosted);
											num6 = num5++;
											if (j >= Settings.Default.NeedToPost)
											{
											}
											break;
										case Poster.PostingResult.Unsuccess:
											this.WriteLog("[" + this.phoneUID + "]: Unsuccess posted");
											num6 = i++;
											Interlocked.Increment(ref this.TotalUnsuccessPosted);
											try
											{
												await this.tiktokUtils.TerminateTikTok();
											}
											catch
											{
											}
											this.WriteLog("[" + this.phoneUID + "]: Checking WDA Session");
											taskAwaiter = this.device.CheckSession().GetAwaiter();
											if (!taskAwaiter.IsCompleted)
											{
												await taskAwaiter;
												taskAwaiter = taskAwaiter2;
												taskAwaiter2 = default(TaskAwaiter<bool>);
												num2 = -1;
											}
											if (!taskAwaiter.GetResult())
											{
												Form1.StatusList[potok] = "Error";
												this.WriteLog("[" + this.phoneUID + "]: WDA OFFLINE");
											}
											if (j >= Settings.Default.NeedToPost)
											{
												goto IL_738F;
											}
											break;
										case Poster.PostingResult.Blocked:
											this.WriteLog("[" + this.phoneUID + "]: Posting Error BLOCKED");
											if (!string.IsNullOrEmpty(text2))
											{
												Form1.OutData.ReWorkSlowProblem.Enqueue(text2 + " Account Status || BLOCKED");
											}
											num6 = i++;
											Interlocked.Increment(ref this.TotalUnsuccessPosted);
											try
											{
												await this.tiktokUtils.TerminateTikTok();
											}
											catch
											{
											}
											goto IL_7CA8;
										case Poster.PostingResult.UnAuth:
											goto IL_6F3F;
										case Poster.PostingResult.AccountStatus:
											this.WriteLog("[" + this.phoneUID + "]: Posting Error Account Status");
											if (!string.IsNullOrEmpty(text2))
											{
												Form1.OutData.ReWorkRisk.Enqueue(text2 + " Account Status || RISK");
											}
											num6 = i++;
											Interlocked.Increment(ref this.TotalUnsuccessPosted);
											try
											{
												await this.tiktokUtils.TerminateTikTok();
											}
											catch
											{
											}
											goto IL_7CA8;
										case Poster.PostingResult.NotCreateError:
											this.WriteLog("[" + this.phoneUID + "]: Posting Error Account Status");
											if (!string.IsNullOrEmpty(text2))
											{
												Form1.OutData.ReWorkSlowProblem.Enqueue(text2 + " NOT CREATE ERROR");
											}
											num6 = i++;
											Interlocked.Increment(ref this.TotalUnsuccessPosted);
											try
											{
												await this.tiktokUtils.TerminateTikTok();
											}
											catch
											{
											}
											goto IL_7B67;
										case Poster.PostingResult.NoHaveVideo:
											this.WriteLog("[" + this.phoneUID + "]: No Have More Videos In Posting Folder");
											num6 = i++;
											Interlocked.Increment(ref this.TotalUnsuccessPosted);
											try
											{
												await this.tiktokUtils.TerminateTikTok();
												goto IL_738F;
											}
											catch
											{
												goto IL_738F;
											}
											goto IL_6F3F;
										}
										if (flag14)
										{
											int num17 = 1;
											if (Settings.Default.DelaysBetweenPostingFrom != 0 && Settings.Default.DelaysBetweenPostingTo != 0 && Settings.Default.DelaysBetweenPostingFrom < Settings.Default.DelaysBetweenPostingTo)
											{
												num17 = ConstParams.rand.Next(Settings.Default.DelaysBetweenPostingFrom, Settings.Default.DelaysBetweenPostingTo);
											}
											this.WriteLog(string.Format("[{0}]: Waiting for {1} seconds before posting", this.phoneUID, num17));
											await Task.Delay(TimeSpan.FromSeconds((double)num17));
											continue;
										}
										continue;
										IL_6F3F:
										this.WriteLog("[" + this.phoneUID + "]: Posting Error UnAuth");
										if (!string.IsNullOrEmpty(text2))
										{
											Form1.OutData.ReWorkSlowProblem.Enqueue(text2 + " UnAuth ERROR");
										}
										num6 = i++;
										Interlocked.Increment(ref this.TotalUnsuccessPosted);
										try
										{
											await this.tiktokUtils.TerminateTikTok();
										}
										catch
										{
										}
										goto IL_7CA8;
									}
									IL_738F:
									this.WriteLog("[" + this.phoneUID + "]: Posting done");
									if (!string.IsNullOrEmpty(text2))
									{
										Form1.OutData.ReWorkStats.Enqueue(text2 + string.Format(" Uploaded || Success: {0} || Unsuccess: {1}", num5, i));
									}
									if ((Settings.Default.MentionInBio || Settings.Default.UpdateBio) && Settings.Default.BioAfterPosting)
									{
										this.WriteLog("[" + this.phoneUID + "]: Update bio Feature || AP");
										TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter7 = this.tiktokUtils.ActivateTikTokGotoProfile().GetAwaiter();
										if (!taskAwaiter7.IsCompleted)
										{
											await taskAwaiter7;
											taskAwaiter7 = taskAwaiter8;
											taskAwaiter8 = default(TaskAwaiter<TikTokUtils.ActivateResult>);
											num2 = -1;
										}
										if (taskAwaiter7.GetResult() != TikTokUtils.ActivateResult.Success)
										{
											this.WriteLog("[" + this.phoneUID + "]: Activate Profile Error");
											goto IL_7B67;
										}
										await this.device.ConfigurateAppium(21);
										try
										{
											this.WriteLog("[" + this.phoneUID + "]: Checking Save Draft");
											await (await this.device.WaitForElementBy(0, "Save draft", 3.0)).Click(false);
											await Task.Delay(TimeSpan.FromSeconds(2.0));
										}
										catch
										{
										}
										this.WriteLog("[" + this.phoneUID + "]: Click Edit Button");
										int num18 = 0;
										try
										{
											await (await this.device.WaitForElementBy(0, "Edit", 5.0)).Click(false);
										}
										catch
										{
											num18 = 1;
										}
										num6 = num18;
										if (num6 == 1)
										{
											try
											{
												await (await this.device.WaitForElementBy(0, "user_info_manage_edit_profile", 5.0)).Click(false);
											}
											catch
											{
											}
										}
										taskAwaiter = this.UpdateBio(false).GetAwaiter();
										if (!taskAwaiter.IsCompleted)
										{
											await taskAwaiter;
											taskAwaiter = taskAwaiter2;
											taskAwaiter2 = default(TaskAwaiter<bool>);
											num2 = -1;
										}
										if (taskAwaiter.GetResult())
										{
											if (!string.IsNullOrEmpty(text2))
											{
												Form1.OutData.ReWorkStats.Enqueue(text2 + " Bio Updated - OK");
											}
											this.WriteLog("[" + this.phoneUID + "]: Mention in bio success");
										}
										else
										{
											if (!string.IsNullOrEmpty(text2))
											{
												Form1.OutData.ReWorkStats.Enqueue(text2 + " Bio Not Updated - SLOW");
												Interlocked.Increment(ref this.TotalSlowTimes);
											}
											this.WriteLog("[" + this.phoneUID + "]: Mention in bio unsuccess");
										}
									}
								}
								catch
								{
									num4 = 1;
								}
								num6 = num4;
								if (num6 == 1)
								{
									try
									{
										await this.tiktokUtils.GoHome(true);
									}
									catch
									{
									}
									try
									{
										await this.tiktokUtils.TerminateTikTok();
									}
									catch
									{
									}
									this.WriteLog("[" + this.phoneUID + "]: Checking WDA Session");
									taskAwaiter = this.device.CheckSession().GetAwaiter();
									if (!taskAwaiter.IsCompleted)
									{
										await taskAwaiter;
										taskAwaiter = taskAwaiter2;
										taskAwaiter2 = default(TaskAwaiter<bool>);
										num2 = -1;
									}
									if (!taskAwaiter.GetResult())
									{
										Form1.StatusList[potok] = "Error";
										this.WriteLog("[" + this.phoneUID + "]: WDA OFFLINE");
									}
								}
								IL_7B67:
								if (Settings.Default.Posting || Settings.Default.MakingPlusPosting)
								{
									try
									{
										await this.MakeScreenShotProfile();
									}
									catch (Exception ex5)
									{
										LogsUtils.WriteLog("[" + this.phoneUID + "]: Exception: " + ex5.ToString());
									}
								}
								try
								{
									await this.tiktokUtils.LogOutFromAccount(null, false, false);
								}
								catch (Exception ex6)
								{
									LogsUtils.WriteLog("[" + this.phoneUID + "]: Exception: " + ex6.ToString());
								}
								IL_7CA8:
								if ((Settings.Default.UseVPN || Settings.Default.UseShadowRocket) && this.vpn.IsActivated)
								{
									await this.vpn.TurnOffVpn(this.device, this.tiktokUtils);
								}
								if (Settings.Default.RecordScreen)
								{
									try
									{
										await this.tiktokUtils.GoHome(false);
										LogsUtils.WriteLog("[" + this.phoneUID + "]: Checking for Screen Recording element");
										TaskAwaiter<Element> taskAwaiter3 = this.device.WaitForElementBy(3, "Screen recording in progress", 1.0).GetAwaiter();
										if (!taskAwaiter3.IsCompleted)
										{
											await taskAwaiter3;
											taskAwaiter3 = taskAwaiter4;
											taskAwaiter4 = default(TaskAwaiter<Element>);
											num2 = -1;
										}
										if (taskAwaiter3.GetResult() != null)
										{
											await this.ScreenRecorder(true, true);
										}
									}
									catch (Exception ex7)
									{
										LogsUtils.WriteLog("[" + this.phoneUID + "]: Exception C: " + ex7.ToString());
									}
								}
								element = null;
							}
							catch (Exception obj6)
							{
								num3 = 1;
							}
							num6 = num3;
							object obj6;
							if (num6 == 1)
							{
								Exception ex8 = (Exception)obj6;
								LogsUtils.WriteLog("[" + this.phoneUID + "]: Exception B: " + ex8.ToString());
								try
								{
									bool flag15 = await this.device.CheckSession();
									LogsUtils.WriteLog(string.Format("[{0}]: Check session result: {1}", this.phoneUID, flag15));
								}
								catch
								{
								}
								await Task.Delay(TimeSpan.FromSeconds(5.0));
							}
							obj6 = null;
							text2 = null;
						}
						Form1.StatusList[potok] = "Stoped";
						this.Started = false;
						this.WriteLog("[" + this.phoneUID + "]: Thread Stoped");
					}
				}
				catch (Exception ex9)
				{
					LogsUtils.WriteLog("[" + this.phoneUID + "]: CRITICAL EXCEPTION: " + ex9.ToString());
					continue;
				}
				break;
			}
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00010260 File Offset: 0x0000E460
		private async Task CheckSimAlert()
		{
			try
			{
				this.WriteLog("[" + this.phoneUID + "]: Checking No SIM alert");
				TaskAwaiter<Element> taskAwaiter = this.device.WaitForElementBy(0, "No SIM Card Installed", 1.0).GetAwaiter();
				if (!taskAwaiter.IsCompleted)
				{
					await taskAwaiter;
					TaskAwaiter<Element> taskAwaiter2;
					taskAwaiter = taskAwaiter2;
					taskAwaiter2 = default(TaskAwaiter<Element>);
				}
				if (taskAwaiter.GetResult() != null)
				{
					this.WriteLog("[" + this.phoneUID + "]: Click OK Button");
					await (await this.device.WaitForElementBy(0, "OK", 10.0)).TapAlert(0, 0, false);
				}
			}
			catch
			{
			}
		}

		// Token: 0x06000075 RID: 117 RVA: 0x000102A4 File Offset: 0x0000E4A4
		private async Task CheckTouchAlert()
		{
			try
			{
				this.WriteLog("[" + this.phoneUID + "]: Checking Touch ID Allert");
				TaskAwaiter<Element> taskAwaiter = this.device.WaitForElementBy(3, "Unable to activate Touch", 1.0).GetAwaiter();
				if (!taskAwaiter.IsCompleted)
				{
					await taskAwaiter;
					TaskAwaiter<Element> taskAwaiter2;
					taskAwaiter = taskAwaiter2;
					taskAwaiter2 = default(TaskAwaiter<Element>);
				}
				if (taskAwaiter.GetResult() != null)
				{
					this.WriteLog("[" + this.phoneUID + "]: Click OK Button");
					await (await this.device.WaitForElementBy(0, "OK", 10.0)).TapAlert(0, 0, false);
				}
			}
			catch
			{
			}
		}

		// Token: 0x06000076 RID: 118 RVA: 0x000102E8 File Offset: 0x0000E4E8
		private async Task<bool> ChangeDateTime(string reg)
		{
			int num = 0;
			int num2 = 0;
			string text = reg;
			if (reg.Contains(":US"))
			{
				text = reg.Replace(":US", "");
			}
			bool flag = text.Contains("USA") || text.Contains("U.S.") || text.Contains("United States") || reg.Contains(":US");
			if (Settings.Default.Posting)
			{
				flag = true;
			}
			this.WriteLog("[" + this.phoneUID + "]: Appium 25 | 1SZX");
			await this.device.ConfigurateAppium(25);
			for (;;)
			{
				int num3 = 0;
				try
				{
					if (num2 > 3)
					{
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Turn Off Preferences");
						await this.device.TerminateApp("com.apple.Preferences");
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Turn On Preferences");
						await this.device.ActivateApp("com.apple.Preferences");
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Swipe");
						await this.device.Swipe(150, 150, 150, 0, 0.0);
						await Task.Delay(TimeSpan.FromSeconds(1.0));
						int num4 = 0;
						try
						{
							LogsUtils.WriteLog("[" + this.phoneUID + "]: Click General");
							await (await this.device.WaitForElementBy(0, "General", 10.0)).Click(false);
						}
						catch (Exception obj)
						{
							num4 = 1;
						}
						object obj;
						if (num4 == 1)
						{
							Exception ex = (Exception)obj;
							num++;
							if (num > 3)
							{
								await this.device.wda.StartWda(true);
								num = 0;
							}
							throw ex;
						}
						obj = null;
						await Task.Delay(TimeSpan.FromSeconds(1.0));
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Swipe");
						await this.device.Swipe(150, 600, 150, 400, 0.0);
						await Task.Delay(TimeSpan.FromSeconds(1.0));
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Click DATE_AND_TIME Button");
						await (await this.device.WaitForElementBy(0, "DATE_AND_TIME", 10.0)).Click(false);
					}
					else
					{
						await this.tiktokUtils.RunShortCut(TikTokUtils.ShortcutName.Date);
						if (num2 > 1)
						{
							await Task.Delay(TimeSpan.FromSeconds(2.0));
						}
					}
					LogsUtils.WriteLog("[" + this.phoneUID + "]: Waiting for Set Automatically Switch");
					TaskAwaiter<string> taskAwaiter = (await this.device.WaitForElementByXPath("XCUIElementTypeSwitch", 0, "Set Automatically", 10.0)).Value().GetAwaiter();
					if (!taskAwaiter.IsCompleted)
					{
						await taskAwaiter;
						TaskAwaiter<string> taskAwaiter2;
						taskAwaiter = taskAwaiter2;
						taskAwaiter2 = default(TaskAwaiter<string>);
					}
					if (taskAwaiter.GetResult() == "1")
					{
						int num4 = 0;
						try
						{
							LogsUtils.WriteLog("[" + this.phoneUID + "]: Searching for Switch Boxes");
							List<Element> list = await this.device.WaitForElementsByXPath("XCUIElementTypeSwitch", 1, "1", 10.0);
							await list[list.Count - 1].TapAlert(0, 0, false);
						}
						catch
						{
							num4 = 1;
						}
						if (num4 == 1)
						{
							LogsUtils.WriteLog("[" + this.phoneUID + "]: Exception || Tapping by coords");
							await this.device.Tap(344.5, 250.0);
						}
					}
					string text2 = await (await this.device.WaitForElementByXPath("XCUIElementTypeSwitch", 0, "24-Hour Time", 10.0)).Value();
					if (flag && text2 == "1")
					{
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Turn Off 24 Hours time");
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Searching for Switch Boxes");
						await (await this.device.WaitForElementsByXPath("XCUIElementTypeSwitch", 1, "1", 10.0))[1].TapAlert(0, 0, false);
					}
					else if (!flag && text2 == "0")
					{
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Turn On 24 Hours time");
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Searching for Switch Boxes");
						await (await this.device.WaitForElementsByXPath("XCUIElementTypeSwitch", 1, "0", 10.0))[2].TapAlert(0, 0, false);
					}
					TaskAwaiter<Element> taskAwaiter3 = this.device.WaitForElementBy(0, text, 3.0).GetAwaiter();
					if (!taskAwaiter3.IsCompleted)
					{
						await taskAwaiter3;
						TaskAwaiter<Element> taskAwaiter4;
						taskAwaiter3 = taskAwaiter4;
						taskAwaiter4 = default(TaskAwaiter<Element>);
					}
					if (taskAwaiter3.GetResult() != null)
					{
						return false;
					}
					LogsUtils.WriteLog("[" + this.phoneUID + "]: Click Time Zone Button");
					await (await this.device.WaitForElementBy(0, "Time Zone", 10.0)).Click(false);
					await Task.Delay(TimeSpan.FromSeconds(2.0));
					for (;;)
					{
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Write Time Zone: " + text);
						await (await this.device.WaitForElementBy(0, "Time Zone", 10.0)).SetText(text);
						int num4 = 0;
						try
						{
							LogsUtils.WriteLog("[" + this.phoneUID + "]: Click Time Zone: " + text);
							await (await this.device.WaitForElementBy(3, text, 10.0)).Click(false);
						}
						catch
						{
							num4 = 1;
						}
						if (num4 != 1)
						{
							break;
						}
						await (await this.device.WaitForElementBy(0, "Time Zone", 10.0)).ClearText();
						if (text == "USA")
						{
							text = "U.S.A";
						}
						else if (text == "U.S.A.")
						{
							text = "United States";
						}
						else if (text == "United States")
						{
							text = "USA";
						}
					}
					try
					{
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Click Back Button");
						await (await this.device.WaitForElementBy(0, "BackButton", 10.0)).Click(false);
						return true;
					}
					catch
					{
						return false;
					}
				}
				catch (Exception obj2)
				{
					num3 = 1;
				}
				if (num3 != 1)
				{
					break;
				}
				object obj2;
				Exception ex2 = (Exception)obj2;
				num2++;
				LogsUtils.WriteLog("[" + this.phoneUID + "]: Change Time Zone Exception Exception: " + ex2.ToString());
				LogsUtils.WriteLog("[" + this.phoneUID + "]: Turn Off Preferences");
				await this.device.TerminateApp("com.apple.Preferences");
				await Task.Delay(TimeSpan.FromSeconds(1.0));
				if (text == "USA")
				{
					text = "U.S.A";
				}
				else if (text == "U.S.A.")
				{
					text = "United States";
				}
				else if (text == "United States")
				{
					text = "USA";
				}
			}
			text = null;
			bool flag2;
			return flag2;
		}

		// Token: 0x06000077 RID: 119 RVA: 0x00010334 File Offset: 0x0000E534
		private async Task<bool> ChangeRegion(string reg, bool TimeZoneChanged)
		{
			string text = reg;
			if (reg.Contains(":US"))
			{
				text = "United States";
			}
			this.WriteLog("[" + this.phoneUID + "]: Appium 25 | VXSGH1");
			await this.device.ConfigurateAppium(25);
			for (;;)
			{
				int num = 0;
				try
				{
					await this.tiktokUtils.RunShortCut(TikTokUtils.ShortcutName.Region);
					TaskAwaiter<Element> taskAwaiter = this.device.WaitForElementBy(2, "Region, " + text, 4.0).GetAwaiter();
					if (!taskAwaiter.IsCompleted)
					{
						await taskAwaiter;
						TaskAwaiter<Element> taskAwaiter2;
						taskAwaiter = taskAwaiter2;
						taskAwaiter2 = default(TaskAwaiter<Element>);
					}
					if (taskAwaiter.GetResult() != null)
					{
						return false;
					}
					LogsUtils.WriteLog("[" + this.phoneUID + "]: Click Region Button 2");
					await (await this.device.WaitForElementBy(0, "Region", 10.0)).Click(false);
					LogsUtils.WriteLog("[" + this.phoneUID + "]: Write Region: " + text);
					await (await this.device.WaitForElementBy(0, "Search", 10.0)).SetText(text);
					LogsUtils.WriteLog("[" + this.phoneUID + "]: Click Region: " + text);
					await (await this.device.WaitForElementBy(0, text, 10.0)).Click(false);
					try
					{
						LogsUtils.WriteLog(string.Concat(new string[] { "[", this.phoneUID, "]: Click Change to ", text, " Button" }));
						await (await this.device.WaitForElementBy(3, "Change to", 10.0)).Click(false);
						return true;
					}
					catch
					{
						return false;
					}
				}
				catch (Exception obj)
				{
					num = 1;
				}
				if (num != 1)
				{
					break;
				}
				object obj;
				Exception ex = (Exception)obj;
				LogsUtils.WriteLog("[" + this.phoneUID + "]: Change Region Exception Exception: " + ex.ToString());
				LogsUtils.WriteLog("[" + this.phoneUID + "]: Turn Off Preferences");
				await this.device.TerminateApp("com.apple.Preferences");
			}
			text = null;
			bool flag;
			return flag;
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00010380 File Offset: 0x0000E580
		private async Task ResetNetworkSettings()
		{
			object obj;
			for (;;)
			{
				int num = 0;
				try
				{
					await this.device.ConfigurateAppium(25);
					await this.tiktokUtils.RunShortCut(TikTokUtils.ShortcutName.Reset);
					LogsUtils.WriteLog("[" + this.phoneUID + "]: Click Reset");
					Task.Delay(TimeSpan.FromSeconds(3.0)).ConfigureAwait(false).GetAwaiter()
						.GetResult();
					await (await this.device.WaitForElementBy(0, "Reset", 10.0)).TapAlert(0, 0, false);
					await Task.Delay(TimeSpan.FromSeconds(1.0));
					LogsUtils.WriteLog("[" + this.phoneUID + "]: Click Reset Network Settings Button");
					int num2 = 0;
					try
					{
						await (await this.device.WaitForElementBy(0, "Reset Network Settings", 10.0)).TapAlert(0, 0, false);
					}
					catch
					{
						num2 = 1;
					}
					if (num2 == 1)
					{
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Click Reset");
						await (await this.device.WaitForElementBy(0, "Reset", 10.0)).TapAlert(0, 0, false);
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Click Reset Network Settings Button");
						await (await this.device.WaitForElementBy(0, "Reset Network Settings", 30.0)).TapAlert(0, 0, false);
					}
					await Task.Delay(TimeSpan.FromSeconds(1.0));
					LogsUtils.WriteLog("[" + this.phoneUID + "]: Click Reset Network Settings Button Again");
					try
					{
						await (await this.device.WaitForElementByXPath("XCUIElementTypeButton", 0, "Reset Network Settings", 30.0)).TapAlert(0, 0, false);
					}
					catch
					{
					}
				}
				catch (Exception obj)
				{
					num = 1;
				}
				if (num != 1)
				{
					break;
				}
				Exception ex = (Exception)obj;
				LogsUtils.WriteLog("[" + this.phoneUID + "]: Reset Network Exception: " + ex.ToString());
				LogsUtils.WriteLog("[" + this.phoneUID + "]: Turn Off Preferences");
				await this.device.TerminateApp("com.apple.Preferences");
			}
			obj = null;
		}

		// Token: 0x06000079 RID: 121 RVA: 0x000103C4 File Offset: 0x0000E5C4
		public async Task ScreenRecorder(bool SaveRecordName, bool BugVideo)
		{
			await this.tiktokUtils.GoHome(false);
			LogsUtils.WriteLog("[" + this.phoneUID + "]: Swipe");
			await this.device.Swipe(350, 26, 350, 500, 0.0);
			Element element;
			for (;;)
			{
				this.WriteLog("[" + this.phoneUID + "]: Searching for Screen Recording element");
				element = await this.device.WaitForElementBy(0, "Screen Recording", 10.0);
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
					break;
				}
				this.WriteLog("[" + this.phoneUID + "]: Screen Recording element invisible");
				this.WriteLog("[" + this.phoneUID + "]: Swipe");
				await this.device.Swipe(150, 350, 150, 0, 0.0);
			}
			this.WriteLog("[" + this.phoneUID + "]: Click Screen Recording element");
			await element.TapAlert(0, 0, false);
			await Task.Delay(TimeSpan.FromSeconds(1.0));
			await this.tiktokUtils.GoHome(false);
			if (SaveRecordName)
			{
				this.WriteLog("[" + this.phoneUID + "]: SRN Start");
				try
				{
					bool flag = false;
					string text = "100APPLE";
					if (this.PhoneModel.Contains("X"))
					{
						text = "101APPLE";
					}
					string[] array;
					for (;;)
					{
						this.WriteLog("[" + this.phoneUID + "]: CMD Tdivece");
						array = CMDUtils.WriteCMD("tidevice -u " + this.phoneUID + " fsync ls /DCIM/" + text, 10000);
						if (array != null || flag)
						{
							break;
						}
						flag = true;
						text = ((!(text == "100APPLE")) ? "100APPLE" : "101APPLE");
					}
					this.WriteLog("[" + this.phoneUID + "]: CMD Done");
					int i = 0;
					while (i < array.Length)
					{
						string text2 = array[i];
						if (!string.IsNullOrEmpty(text2) && !text2.Contains("Microsoft ") && !text2.Contains("tidevice") && !text2.Contains("\\") && (text2.Contains(".MOV") || text2.Contains(".MP4")))
						{
							string[] array2 = text2.Split(new char[] { ' ' });
							string text3 = array2[array2.Length - 1].Trim().Replace("/r", "").Replace("\r", "");
							string text4 = array2[array2.Length - 3].Trim().Replace("/r", "").Replace("\r", "");
							if (BugVideo)
							{
								this.WriteLog(string.Concat(new string[] { "[", this.phoneUID, "]: Bug Video ", text3, " Created: ", text4 }));
								Form1.OutData.BugVideos.Enqueue(string.Concat(new string[] { text3, " Created: ", text4, " PhoneUID: ", this.phoneUID }));
								break;
							}
							this.WriteLog(string.Concat(new string[] { "[", this.phoneUID, "]: Success Video ", text3, " Created: ", text4 }));
							break;
						}
						else
						{
							i++;
						}
					}
					this.WriteLog("[" + this.phoneUID + "]: SRN Checking Done");
				}
				catch (Exception ex)
				{
					this.WriteLog("[" + this.phoneUID + "]: SRN Exception: " + ex.ToString());
				}
			}
			this.WriteLog("[" + this.phoneUID + "]: SRN Done");
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00010418 File Offset: 0x0000E618
		private async Task<bool> SearchForTikTokIcon(bool ForVisible = false)
		{
			bool flag;
			if (!this.device.wda.IsTikTokInstalled())
			{
				flag = false;
			}
			else if (!ForVisible)
			{
				flag = true;
			}
			else
			{
				int num = 0;
				await this.tiktokUtils.GoHome(true);
				for (;;)
				{
					try
					{
						int num2 = 0;
						for (;;)
						{
							this.WriteLog("[" + this.phoneUID + "]: Search for TikTok icon");
							Element element = await this.device.WaitForElementByXPath("XCUIElementTypeIcon", 0, "TikTok", 3.0);
							if (element != null)
							{
								this.WriteLog("[" + this.phoneUID + "]: TikTok icon found");
								if (!ForVisible)
								{
									goto IL_0236;
								}
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
									break;
								}
								this.WriteLog("[" + this.phoneUID + "]: TikTok icon invisible!");
								num2++;
								if (num2 >= 3)
								{
									goto Block_10;
								}
								this.WriteLog("[" + this.phoneUID + "]: Swipe");
								await this.device.Swipe(600, 400, 0, 400, 0.0);
								await Task.Delay(TimeSpan.FromSeconds(1.0));
							}
							else
							{
								this.WriteLog("[" + this.phoneUID + "]: TikTok icon not found");
								if (num2 > 3)
								{
									goto IL_0396;
								}
								num2++;
								this.WriteLog("[" + this.phoneUID + "]: Swipe");
								await this.device.Swipe(600, 400, 0, 400, 0.0);
								await Task.Delay(TimeSpan.FromSeconds(1.0));
							}
						}
						this.WriteLog("[" + this.phoneUID + "]: TikTok icon visible!");
						flag = true;
						break;
						IL_0236:
						this.WriteLog("[" + this.phoneUID + "]: TikTok icon found but invisible!");
						flag = true;
						break;
						IL_0396:
						flag = false;
						break;
						Block_10:
						this.WriteLog("[" + this.phoneUID + "]: TikTok icon not found");
						flag = false;
					}
					catch (Exception ex)
					{
						this.WriteLog("[" + this.phoneUID + "]: Search for TikTok icon Exception: " + ex.ToString());
						num++;
						if (num > 5)
						{
							throw ex;
						}
						continue;
					}
					break;
				}
			}
			return flag;
		}

		// Token: 0x0600007B RID: 123 RVA: 0x0000232A File Offset: 0x0000052A
		private void RemoveTikTok()
		{
			this.device.wda.RemoveTikTok();
		}

		// Token: 0x0600007C RID: 124 RVA: 0x00010464 File Offset: 0x0000E664
		private async Task<bool> TurnOnWifi()
		{
			this.WriteLog("[" + this.phoneUID + "]: Appium 100 | DSA1");
			await this.device.ConfigurateAppium(100);
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			object obj2;
			do
			{
				int num4 = 0;
				int num5;
				try
				{
					await this.tiktokUtils.RunShortCut(TikTokUtils.ShortcutName.Wifi);
					await Task.Delay(TimeSpan.FromSeconds(5.0));
					bool flag = false;
					Element element = new Element("", null);
					element = null;
					for (;;)
					{
						num5 = num2++;
						if (num2 >= 6)
						{
							break;
						}
						TaskAwaiter<string> taskAwaiter = (await this.device.WaitForElementByXPath("XCUIElementTypeApplication", 9, "true", 10.0)).Label().GetAwaiter();
						TaskAwaiter<string> taskAwaiter2;
						if (!taskAwaiter.IsCompleted)
						{
							await taskAwaiter;
							taskAwaiter = taskAwaiter2;
							taskAwaiter2 = default(TaskAwaiter<string>);
						}
						if (taskAwaiter.GetResult() != "Settings")
						{
							goto Block_7;
						}
						TaskAwaiter<bool> taskAwaiter4;
						if (!flag)
						{
							LogsUtils.WriteLog("[" + this.phoneUID + "]: Checking for Wifi: " + Settings.Default.WifiName);
							element = await this.device.WaitForElementBy(3, Settings.Default.WifiName, 3.0);
							if (element != null)
							{
								TaskAwaiter<bool> taskAwaiter3 = element.Visible().GetAwaiter();
								if (!taskAwaiter3.IsCompleted)
								{
									await taskAwaiter3;
									taskAwaiter3 = taskAwaiter4;
									taskAwaiter4 = default(TaskAwaiter<bool>);
								}
								if (taskAwaiter3.GetResult())
								{
									taskAwaiter = element.Traits().GetAwaiter();
									if (!taskAwaiter.IsCompleted)
									{
										await taskAwaiter;
										taskAwaiter = taskAwaiter2;
										taskAwaiter2 = default(TaskAwaiter<string>);
									}
									if (taskAwaiter.GetResult().Contains("Selected"))
									{
										goto Block_13;
									}
									goto IL_0A8D;
								}
								else
								{
									LogsUtils.WriteLog("[" + this.phoneUID + "]: Found but invisible");
								}
							}
							else
							{
								LogsUtils.WriteLog("[" + this.phoneUID + "]: Not found");
							}
						}
						else
						{
							LogsUtils.WriteLog("[" + this.phoneUID + "]: Just Checking for Selected Wifi");
							element = await this.device.WaitForElementBy(3, Settings.Default.WifiName, 3.0);
							if (element != null)
							{
								TaskAwaiter<bool> taskAwaiter3 = element.Visible().GetAwaiter();
								if (!taskAwaiter3.IsCompleted)
								{
									await taskAwaiter3;
									taskAwaiter3 = taskAwaiter4;
									taskAwaiter4 = default(TaskAwaiter<bool>);
								}
								if (taskAwaiter3.GetResult())
								{
									taskAwaiter = element.Traits().GetAwaiter();
									if (!taskAwaiter.IsCompleted)
									{
										await taskAwaiter;
										taskAwaiter = taskAwaiter2;
										taskAwaiter2 = default(TaskAwaiter<string>);
									}
									if (taskAwaiter.GetResult().Contains("Selected"))
									{
										goto Block_18;
									}
									goto IL_0A8D;
								}
								else
								{
									LogsUtils.WriteLog("[" + this.phoneUID + "]: Found but invisible");
								}
							}
							else
							{
								LogsUtils.WriteLog("[" + this.phoneUID + "]: Not found");
							}
						}
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Search for Other... Button");
						Element element2 = await this.device.WaitForElementBy(3, "Other…", 3.0);
						if (element2 != null)
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
								goto IL_114B;
							}
						}
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Swipe");
						await this.device.Swipe(150, 600, 150, 400, 0.0);
						await Task.Delay(TimeSpan.FromSeconds(1.0));
						continue;
						IL_0A8D:
						num5 = 0;
						try
						{
							LogsUtils.WriteLog("[" + this.phoneUID + "]: CW Checking for Wifi: " + Settings.Default.WifiName);
							element = await this.device.WaitForElementBy(3, Settings.Default.WifiName, 3.0);
							LogsUtils.WriteLog("[" + this.phoneUID + "]: Click Button " + Settings.Default.WifiName);
							await element.Click(false);
							await Task.Delay(TimeSpan.FromSeconds(1.0));
							int num6 = 0;
							try
							{
								LogsUtils.WriteLog("[" + this.phoneUID + "]: Write Wifi Password: " + Settings.Default.WifiPassword);
								await (await this.device.WaitForElementByXPath("XCUIElementTypeSecureTextField", 0, "Password", 10.0)).SetText(Settings.Default.WifiPassword);
								await Task.Delay(TimeSpan.FromSeconds(1.0));
							}
							catch
							{
								num6 = 1;
							}
							if (num6 != 1)
							{
								LogsUtils.WriteLog("[" + this.phoneUID + "]: Click Done Button");
								await (await this.device.WaitForElementBy(0, "Done", 10.0)).Click(false);
								await Task.Delay(TimeSpan.FromSeconds(10.0));
								return true;
							}
							element = await this.device.WaitForElementBy(3, Settings.Default.WifiName, 3.0);
							taskAwaiter = element.Traits().GetAwaiter();
							if (!taskAwaiter.IsCompleted)
							{
								await taskAwaiter;
								taskAwaiter = taskAwaiter2;
								taskAwaiter2 = default(TaskAwaiter<string>);
							}
							if (taskAwaiter.GetResult().Contains("Selected"))
							{
								return true;
							}
							continue;
						}
						catch (Exception obj)
						{
							num5 = 1;
						}
						goto IL_1068;
					}
					num2 = 0;
					LogsUtils.WriteLog("[" + this.phoneUID + "]: Turn Off Preferences");
					await this.device.TerminateApp("com.apple.Preferences");
					continue;
					Block_7:
					num2 = 0;
					LogsUtils.WriteLog("[" + this.phoneUID + "]: Turn Off Preferences");
					await this.device.TerminateApp("com.apple.Preferences");
					continue;
					Block_13:
					return true;
					Block_18:
					return true;
					IL_1068:
					object obj;
					if (num5 == 1)
					{
						Exception ex = (Exception)obj;
						LogsUtils.WriteLog("[" + this.phoneUID + "]: CW Exception: " + ex.ToString());
						num++;
						if (num > 5)
						{
							throw new Exception();
						}
						flag = true;
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Turn Off Preferences");
						await this.device.TerminateApp("com.apple.Preferences");
						continue;
					}
					IL_114B:
					num5 = 0;
					try
					{
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Search for Other... Button");
						Element element2 = await this.device.WaitForElementBy(3, "Other…", 3.0);
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Click Other... Button");
						await element2.Click(false);
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Write Name Wifi: " + Settings.Default.WifiName);
						await (await this.device.WaitForElementBy(0, "Name", 10.0)).SetText(Settings.Default.WifiName);
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Write Wifi Password: " + Settings.Default.WifiPassword);
						await (await this.device.WaitForElementBy(0, "Password", 10.0)).SetText(Settings.Default.WifiPassword);
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Click Done Button");
						await (await this.device.WaitForElementBy(0, "Done", 10.0)).Click(false);
						await Task.Delay(TimeSpan.FromSeconds(10.0));
						return true;
					}
					catch (Exception obj)
					{
						num5 = 1;
					}
					if (num5 == 1)
					{
						Exception ex2 = (Exception)obj;
						LogsUtils.WriteLog("[" + this.phoneUID + "]: CO Exception: " + ex2.ToString());
						num++;
						if (num > 5)
						{
							throw new Exception();
						}
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Turn Off Preferences");
						await this.device.TerminateApp("com.apple.Preferences");
						continue;
					}
					else
					{
						element = null;
					}
				}
				catch (Exception obj2)
				{
					num4 = 1;
				}
				num5 = num4;
				if (num5 != 1)
				{
					goto IL_17CF;
				}
				Exception ex3 = (Exception)obj2;
				LogsUtils.WriteLog("[" + this.phoneUID + "]: Turn Off Preferences");
				await this.device.TerminateApp("com.apple.Preferences");
				LogsUtils.WriteLog("[" + this.phoneUID + "]: Exception: " + ex3.ToString());
				num5 = num3++;
			}
			while (num3 <= 5);
			return false;
			IL_17CF:
			obj2 = null;
			bool flag2;
			return flag2;
		}

		// Token: 0x0600007D RID: 125 RVA: 0x000104A8 File Offset: 0x0000E6A8
		private async Task<bool> InstallTikTok()
		{
			object obj;
			for (;;)
			{
				int num = 0;
				TaskAwaiter<Element> taskAwaiter;
				TaskAwaiter<Element> taskAwaiter2;
				try
				{
					this.WriteLog("[" + this.phoneUID + "]: Appium 100 | DSAX1");
					await this.device.ConfigurateAppium(100);
					await this.device.OpenUrl("itms-apps://itunes.apple.com/app/id835599320");
					LogsUtils.WriteLog("[" + this.phoneUID + "]: Click Re-Download Button");
					int num2 = 0;
					try
					{
						await (await this.device.WaitForElementBy(3, "redownload", 10.0)).Click(false);
					}
					catch
					{
						num2 = 1;
					}
					if (num2 == 1)
					{
						taskAwaiter = this.device.WaitForElementBy(2, "Open", 10.0).GetAwaiter();
						if (!taskAwaiter.IsCompleted)
						{
							await taskAwaiter;
							taskAwaiter = taskAwaiter2;
							taskAwaiter2 = default(TaskAwaiter<Element>);
						}
						if (taskAwaiter.GetResult() != null)
						{
							return true;
						}
						await (await this.device.WaitForElementBy(2, "Get", 5.0)).Click(false);
					}
					await Task.Delay(TimeSpan.FromSeconds(5.0));
				}
				catch (Exception obj)
				{
					num = 1;
				}
				if (num != 1)
				{
					goto IL_0765;
				}
				Exception ex = (Exception)obj;
				LogsUtils.WriteLog("[" + this.phoneUID + "]: Exception: " + ex.ToString());
				LogsUtils.WriteLog("[" + this.phoneUID + "]: Seraching for open button");
				taskAwaiter = this.device.WaitForElementBy(2, "Open", 3.0).GetAwaiter();
				if (!taskAwaiter.IsCompleted)
				{
					await taskAwaiter;
					taskAwaiter = taskAwaiter2;
					taskAwaiter2 = default(TaskAwaiter<Element>);
				}
				if (taskAwaiter.GetResult() != null)
				{
					break;
				}
				LogsUtils.WriteLog("[" + this.phoneUID + "]: Terminate App Store");
				await this.device.TerminateApp("com.apple.AppStore");
				LogsUtils.WriteLog("[" + this.phoneUID + "]: Checking for Allerts");
				try
				{
					await (await this.device.WaitForElementBy(3, "Not Now", 10.0)).TapAlert(0, 0, false);
					continue;
				}
				catch
				{
					continue;
				}
				goto IL_0765;
			}
			LogsUtils.WriteLog("[" + this.phoneUID + "]: Success");
			await this.tiktokUtils.GoHome(true);
			return true;
			IL_0765:
			obj = null;
			int num3 = 0;
			for (;;)
			{
				await this.device.GetSourceXml();
				try
				{
					if (this.device.IsElementInXmlContains("downloadProgress", null))
					{
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Checking for download time");
						string text = await (await this.device.WaitForElementBy(3, "downloadProgress", 4.0)).Label();
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Time: " + text);
						if (text.Contains("minut") && int.Parse(text.Split(new char[] { ' ' })[0]) > 3)
						{
							LogsUtils.WriteLog("[" + this.phoneUID + "]: Click Pause Button");
							await (await this.device.WaitForElementBy(3, "offerButton", 10.0)).Click(false);
							LogsUtils.WriteLog("[" + this.phoneUID + "]: Click Resume Button");
							await (await this.device.WaitForElementBy(2, "Resume", 10.0)).Click(false);
							await Task.Delay(TimeSpan.FromSeconds(5.0));
						}
					}
					else if (this.device.IsElementInXml("Open", null))
					{
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Success");
						await this.tiktokUtils.GoHome(true);
						return true;
					}
				}
				catch
				{
				}
				LogsUtils.WriteLog("[" + this.phoneUID + "]: Waiting for downloading");
				await Task.Delay(TimeSpan.FromSeconds(3.0));
				num3++;
				TaskAwaiter<Element> taskAwaiter = this.device.WaitForElementBy(3, "downloading", 10.0).GetAwaiter();
				TaskAwaiter<Element> taskAwaiter2;
				if (!taskAwaiter.IsCompleted)
				{
					await taskAwaiter;
					taskAwaiter = taskAwaiter2;
					taskAwaiter2 = default(TaskAwaiter<Element>);
				}
				if (taskAwaiter.GetResult() == null)
				{
					LogsUtils.WriteLog("[" + this.phoneUID + "]: Seraching for open button");
					taskAwaiter = this.device.WaitForElementBy(2, "Open", 4.0).GetAwaiter();
					if (!taskAwaiter.IsCompleted)
					{
						await taskAwaiter;
						taskAwaiter = taskAwaiter2;
						taskAwaiter2 = default(TaskAwaiter<Element>);
					}
					if (taskAwaiter.GetResult() != null)
					{
						break;
					}
					int num = 0;
					try
					{
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Check for re-download button");
						LogsUtils.WriteLog("[" + this.phoneUID + "]: Click Re-Download Button");
						int num2 = 0;
						try
						{
							await (await this.device.WaitForElementBy(3, "redownload", 10.0)).Click(false);
						}
						catch
						{
							num2 = 1;
						}
						if (num2 == 1)
						{
							await (await this.device.WaitForElementBy(2, "Get", 10.0)).Click(false);
						}
					}
					catch
					{
						num = 1;
					}
					if (num == 1)
					{
						string text2 = this.phoneUID;
						LogsUtils.WriteLog("[" + text2 + "]: E | Check for Download Alert " + await this.device.GetSource());
						text2 = null;
					}
					if (num3 >= 30)
					{
						goto Block_12;
					}
				}
			}
			LogsUtils.WriteLog("[" + this.phoneUID + "]: Success");
			await this.tiktokUtils.GoHome(true);
			return true;
			Block_12:
			return false;
		}

		// Token: 0x0600007E RID: 126 RVA: 0x000104EC File Offset: 0x0000E6EC
		public async Task MakeScreenShotProfile()
		{
			TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter = this.tiktokUtils.ActivateTikTokGotoProfile().GetAwaiter();
			if (!taskAwaiter.IsCompleted)
			{
				await taskAwaiter;
				TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter2;
				taskAwaiter = taskAwaiter2;
				taskAwaiter2 = default(TaskAwaiter<TikTokUtils.ActivateResult>);
			}
			if (taskAwaiter.GetResult() == TikTokUtils.ActivateResult.Success)
			{
				await Task.Delay(TimeSpan.FromSeconds(3.0));
				await this.device.Swipe(250, 100, 250, 600, 0.0);
				await Task.Delay(TimeSpan.FromSeconds(5.0));
				await this.tiktokUtils.SaveScreenShot(false, TikTokUtils.ScreenshotType.Working);
				await this.device.Swipe(250, 600, 250, 100, 0.0);
				await Task.Delay(TimeSpan.FromSeconds(2.0));
				await this.tiktokUtils.SaveScreenShot(false, TikTokUtils.ScreenshotType.Working);
			}
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00010530 File Offset: 0x0000E730
		private async Task<bool> TurnOn2FA(string username, string password)
		{
			int num = 0;
			bool flag;
			for (;;)
			{
				int num2 = 0;
				try
				{
					Worker.Class13 @class = new Worker.Class13();
					@class.Field0 = this;
					await this.tiktokUtils.RunTikTokOpenUrl(TikTokUtils.TikTokOpenUrl.Setting);
					await Task.Delay(TimeSpan.FromSeconds(1.0));
					try
					{
						this.WriteLog("[" + this.phoneUID + "]: Checking for Save allert");
						await (await this.device.WaitForElementBy(0, "close", 3.0)).Click(false);
					}
					catch
					{
					}
					this.WriteLog("[" + this.phoneUID + "]: Click Security Button");
					await (await this.device.WaitForElementBy(3, "Security", 10.0)).Click(false);
					this.WriteLog("[" + this.phoneUID + "]: Click 2 Step Verification Button");
					await (await this.device.WaitForElementBy(3, "step verification", 10.0)).Click(false);
					TaskAwaiter<Element> taskAwaiter = this.device.WaitForElementBy(3, "Turn on", 40.0).GetAwaiter();
					TaskAwaiter<Element> taskAwaiter2;
					if (!taskAwaiter.IsCompleted)
					{
						await taskAwaiter;
						taskAwaiter = taskAwaiter2;
						taskAwaiter2 = default(TaskAwaiter<Element>);
					}
					if (taskAwaiter.GetResult() != null)
					{
						Element element = await this.device.WaitForElementBy(0, "Select at least 2 methods", 10.0);
						this.WriteLog("[" + this.phoneUID + "]: Click Button 2 By Coords");
						await element.TapAlert(0, 150, false);
						this.WriteLog("[" + this.phoneUID + "]: Click Button 3 By Coords");
						await element.TapAlert(0, 250, false);
						element = null;
						this.WriteLog("[" + this.phoneUID + "]: Click Turn On Button");
						await (await this.device.WaitForElementBy(3, "Turn on", 10.0)).Click(false);
						this.WriteLog("[" + this.phoneUID + "]: Checking for Phone number");
						taskAwaiter = this.device.WaitForElementBy(4, "Phone number", 3.0).GetAwaiter();
						if (!taskAwaiter.IsCompleted)
						{
							await taskAwaiter;
							taskAwaiter = taskAwaiter2;
							taskAwaiter2 = default(TaskAwaiter<Element>);
						}
						TaskAwaiter<bool> taskAwaiter4;
						if (taskAwaiter.GetResult() != null)
						{
							this.WriteLog("[" + this.phoneUID + "]: Click Close Button");
							await (await this.device.WaitForElementBy(3, "icon close", 10.0)).Click(false);
							this.WriteLog("[" + this.phoneUID + "]: Click Exit Button");
							await (await this.device.WaitForElementBy(3, "Exit", 10.0)).Click(false);
							element = await this.device.WaitForElementBy(0, "Select at least 2 methods", 10.0);
							this.WriteLog("[" + this.phoneUID + "]: Click Button 1 By Coords");
							await element.TapAlert(0, 50, false);
							this.WriteLog("[" + this.phoneUID + "]: Checking for On Button Enabled");
							Element element2 = await this.device.WaitForElementBy(3, "Turn on", 10.0);
							TaskAwaiter<bool> taskAwaiter3 = element2.Enabled().GetAwaiter();
							if (!taskAwaiter3.IsCompleted)
							{
								await taskAwaiter3;
								taskAwaiter3 = taskAwaiter4;
								taskAwaiter4 = default(TaskAwaiter<bool>);
							}
							if (!taskAwaiter3.GetResult())
							{
								this.WriteLog("[" + this.phoneUID + "]: Click Button 4 By Coords");
								await element.TapAlert(0, 320, false);
							}
							this.WriteLog("[" + this.phoneUID + "]: Click Turn On Button");
							await element2.Click(false);
							element = null;
							element2 = null;
						}
						this.WriteLog("[" + this.phoneUID + "]: Checking for Email Address");
						taskAwaiter = this.device.WaitForElementBy(0, "Enter email address", 3.0).GetAwaiter();
						if (!taskAwaiter.IsCompleted)
						{
							await taskAwaiter;
							taskAwaiter = taskAwaiter2;
							taskAwaiter2 = default(TaskAwaiter<Element>);
						}
						if (taskAwaiter.GetResult() != null)
						{
							this.WriteLog("[" + this.phoneUID + "]: Click Close Button");
							await (await this.device.WaitForElementBy(3, "icon close", 10.0)).Click(false);
							this.WriteLog("[" + this.phoneUID + "]: Click Exit Button");
							await (await this.device.WaitForElementBy(3, "Exit", 10.0)).Click(false);
							Element element2 = await this.device.WaitForElementBy(0, "Select at least 2 methods", 10.0);
							this.WriteLog("[" + this.phoneUID + "]: Click Button 2 By Coords");
							await element2.TapAlert(0, 150, false);
							this.WriteLog("[" + this.phoneUID + "]: Checking for On Button Enabled");
							element = await this.device.WaitForElementBy(3, "Turn on", 10.0);
							TaskAwaiter<bool> taskAwaiter3 = element.Enabled().GetAwaiter();
							if (!taskAwaiter3.IsCompleted)
							{
								await taskAwaiter3;
								taskAwaiter3 = taskAwaiter4;
								taskAwaiter4 = default(TaskAwaiter<bool>);
							}
							if (!taskAwaiter3.GetResult())
							{
								this.WriteLog("[" + this.phoneUID + "]: Click Button 4 By Coords");
								await element2.TapAlert(0, 320, false);
							}
							this.WriteLog("[" + this.phoneUID + "]: Click Turn On Button");
							await element.Click(false);
							element2 = null;
							element = null;
						}
						await Task.Delay(TimeSpan.FromSeconds(2.0));
						string text = "";
						List<Element> list = await this.device.WaitForElementsBy(4, "-", 10.0);
						foreach (Element element3 in list)
						{
							string text2 = await element3.Value();
							if (!text2.Contains("step verification"))
							{
								text = text2;
								this.WriteLog("[" + this.phoneUID + "]: Code found: " + text);
								break;
							}
						}
						List<Element>.Enumerator enumerator = default(List<Element>.Enumerator);
						this.WriteLog("[" + this.phoneUID + "]: Click Next Button");
						await (await this.device.WaitForElementBy(3, "Next", 10.0)).Click(false);
						string text3 = await AuthUtils.FA2AddNewAccountAndGetCode(username, text, this.device, this.phoneUID);
						int num3 = 0;
						for (;;)
						{
							await this.tiktokUtils.ActivateTikTok();
							await Task.Delay(TimeSpan.FromSeconds(4.0));
							List<Task> list2 = new List<Task>();
							@class.elements = new Dictionary<char, Element>();
							this.WriteLog("[" + this.phoneUID + "]: Getting Code Elements for " + text3);
							string text4 = text3;
							for (int i = 0; i < text4.Length; i++)
							{
								Worker.Class14 class2 = new Worker.Class14();
								class2.Field0 = @class;
								class2.d = text4[i];
								list2.Add(Task.Run(new Func<Task>(class2.Method0)));
								await Task.Delay(200);
							}
							text4 = null;
							try
							{
								Task.WhenAll(list2).Wait(TimeSpan.FromSeconds(10.0));
								this.WriteLog("[" + this.phoneUID + "]: Elements found");
							}
							catch (TimeoutException)
							{
								this.WriteLog("[" + this.phoneUID + "]: Elements not found || TimeOut");
							}
							catch (Exception ex)
							{
								this.WriteLog("[" + this.phoneUID + "]: Elements Exception: " + ex.ToString());
							}
							this.WriteLog("[" + this.phoneUID + "]: Write Code: " + text3);
							foreach (char c in text3)
							{
								this.WriteLog(string.Format("[{0}]: Click {1}", this.phoneUID, c));
								int num4 = 0;
								try
								{
									await @class.elements[c].Click(false);
								}
								catch (Exception obj)
								{
									num4 = 1;
								}
								object obj;
								if (num4 == 1)
								{
									Exception ex2 = (Exception)obj;
									int num5 = 0;
									try
									{
										object obj2 = this.phoneUID;
										this.WriteLog(string.Format("[{0}]: Checking session: {1}", obj2, await this.device.CheckSession()));
										obj2 = null;
										this.WriteLog("[" + this.phoneUID + "]: Appium 25 | RL1");
										await this.device.ConfigurateAppium(25);
										this.WriteLog(string.Format("[{0}]: Exception: Button not found, searching for element {1}", this.phoneUID, c));
										await (await this.device.WaitForElementBy(0, c.ToString(), 3.0)).Click(false);
									}
									catch
									{
										num5 = 1;
									}
									if (num5 == 1)
									{
										this.WriteLog("[" + this.phoneUID + "]: Exception: Button not found, tap by coords");
										string text5 = c.ToString();
										uint num6 = Class22.ComputeStringHash(text5);
										if (num6 <= 873244444U)
										{
											if (num6 <= 822911587U)
											{
												if (num6 != 806133968U)
												{
													if (num6 == 822911587U)
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
											else if (num6 != 839689206U)
											{
												if (num6 != 856466825U)
												{
													if (num6 == 873244444U)
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
										else if (num6 <= 906799682U)
										{
											if (num6 != 890022063U)
											{
												if (num6 == 906799682U)
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
										else if (num6 != 923577301U)
										{
											if (num6 != 1007465396U)
											{
												if (num6 == 1024243015U)
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
							this.WriteLog("[" + this.phoneUID + "]: Appium DEF | RL2");
							await this.device.ConfigurateAppium(17);
							await Task.Delay(TimeSpan.FromSeconds(5.0));
							this.WriteLog("[" + this.phoneUID + "]: Checking for errors");
							taskAwaiter = this.device.WaitForElementBy(3, "Install an authenticator app", 10.0).GetAwaiter();
							if (!taskAwaiter.IsCompleted)
							{
								await taskAwaiter;
								taskAwaiter = taskAwaiter2;
								taskAwaiter2 = default(TaskAwaiter<Element>);
							}
							if (taskAwaiter.GetResult() == null)
							{
								goto IL_2895;
							}
							num3++;
							if (num3 > 10)
							{
								break;
							}
							this.WriteLog("[" + this.phoneUID + "]: Click Next Button");
							await (await this.device.WaitForElementBy(3, "Next", 10.0)).Click(false);
							text3 = await AuthUtils.FA2GetExistCode(username, this.device, this.phoneUID);
						}
						try
						{
							await this.tiktokUtils.TerminateTikTok();
						}
						catch
						{
						}
						await Task.Delay(TimeSpan.FromSeconds(3.0));
						await AuthUtils.FA2RemoveExistAccount(username, this.device, this.phoneUID);
						continue;
						IL_2895:
						this.WriteLog("[" + this.phoneUID + "]: Click Skip Button");
						await (await this.device.WaitForElementBy(0, "Skip", 10.0)).Click(false);
						await Task.Delay(TimeSpan.FromSeconds(1.0));
						this.WriteLog("[" + this.phoneUID + "]: Click Add Button");
						await (await this.device.WaitForElementBy(0, "Add", 10.0)).Click(false);
						return true;
					}
					throw new Exception();
				}
				catch (Exception obj3)
				{
					num2 = 1;
				}
				if (num2 != 1)
				{
					return flag;
				}
				object obj3;
				Exception ex3 = (Exception)obj3;
				this.WriteLog("[" + this.phoneUID + "]: Exception: " + ex3.ToString());
				try
				{
					await this.tiktokUtils.TerminateTikTok();
				}
				catch
				{
				}
				await Task.Delay(TimeSpan.FromSeconds(10.0));
				await AuthUtils.FA2RemoveExistAccount(username, this.device, this.phoneUID);
				num++;
				if (num > 3)
				{
					break;
				}
				TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter5 = this.tiktokUtils.ActivateTikTokGotoProfile().GetAwaiter();
				if (!taskAwaiter5.IsCompleted)
				{
					await taskAwaiter5;
					TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter6;
					taskAwaiter5 = taskAwaiter6;
					taskAwaiter6 = default(TaskAwaiter<TikTokUtils.ActivateResult>);
				}
				if (taskAwaiter5.GetResult() != TikTokUtils.ActivateResult.Success)
				{
					goto Block_6;
				}
			}
			return false;
			Block_6:
			flag = false;
			return flag;
		}

		// Token: 0x06000080 RID: 128 RVA: 0x0001057C File Offset: 0x0000E77C
		private async Task WriteFromKeyBoard(string text)
		{
			text = text.ToLower();
			List<string> list = new List<string>
			{
				"1", "2", "3", "4", "5", "6", "7", "8", "9", "0",
				"-", "/", ":", ";", "(", ")", "$", "&", "@", "\"",
				".", ",", "?", "!", "'"
			};
			List<string> list2 = new List<string>
			{
				"q", "w", "e", "r", "t", "y", "u", "i", "o", "p",
				"a", "s", "d", "f", "g", "h", "j", "k", "l", "z",
				"x", "c", "v", "b", "n", "m"
			};
			try
			{
				await (await this.device.WaitForElementBy(2, "letters", 5.0)).Click(false);
			}
			catch
			{
			}
			foreach (char c in text)
			{
				if (list.Contains(c.ToString()))
				{
					await (await this.device.WaitForElementBy(2, "numbers", 10.0)).Click(false);
					await (await this.device.WaitForElementBy(2, c.ToString(), 10.0)).Click(false);
					await (await this.device.WaitForElementBy(2, "letters", 10.0)).Click(false);
				}
				else if (list2.Contains(c.ToString()))
				{
					Element element = await this.device.WaitForElementBy(0, "shift", 10.0);
					TaskAwaiter<string> taskAwaiter = element.Value().GetAwaiter();
					if (!taskAwaiter.IsCompleted)
					{
						await taskAwaiter;
						TaskAwaiter<string> taskAwaiter2;
						taskAwaiter = taskAwaiter2;
						taskAwaiter2 = default(TaskAwaiter<string>);
					}
					if (taskAwaiter.GetResult() == "1")
					{
						await element.Click(false);
					}
					await (await this.device.WaitForElementBy(2, c.ToString(), 10.0)).Click(false);
					element = null;
				}
			}
			string text2 = null;
			try
			{
				await (await this.device.WaitForElementBy(2, "return", 10.0)).Click(false);
			}
			catch
			{
			}
		}

		// Token: 0x06000081 RID: 129 RVA: 0x000105C8 File Offset: 0x0000E7C8
		public async Task OpenMusic(string url)
		{
			for (int i = 0; i < 3; i++)
			{
				try
				{
					this.WriteLog("[" + this.phoneUID + "]: Activate Notes");
					await this.device.ActivateApp("com.apple.mobilenotes");
					this.WriteLog("[" + this.phoneUID + "]: Checking for Continue Button");
					try
					{
						await (await this.device.WaitForElementBy(0, "Continue", 5.0)).Click(false);
					}
					catch
					{
					}
					this.WriteLog("[" + this.phoneUID + "]: Checking for Not Now Button");
					try
					{
						await (await this.device.WaitForElementBy(0, "Not Now", 5.0)).TapAlert(0, 0, false);
					}
					catch
					{
					}
					this.WriteLog("[" + this.phoneUID + "]: Click New Note");
					try
					{
						await (await this.device.WaitForElementBy(0, "New note", 10.0)).Click(false);
					}
					catch
					{
					}
					this.WriteLog("[" + this.phoneUID + "]: Write Link Text");
					int num = 0;
					try
					{
						await (await this.device.WaitForElementByXPath("XCUIElementTypeTextView", 0, "Note", 4.0)).SetText(url);
					}
					catch
					{
						num = 1;
					}
					if (num == 1)
					{
						await (await this.device.WaitForElementByXPath("XCUIElementTypeTextView", 0, "note", 4.0)).SetText(url);
					}
					this.WriteLog("[" + this.phoneUID + "]: Click Done");
					await (await this.device.WaitForElementBy(0, "Done", 10.0)).Click(false);
					await Task.Delay(TimeSpan.FromSeconds(1.0));
					this.WriteLog("[" + this.phoneUID + "]: Click on Link");
					await (await this.device.WaitForElementByXPath("XCUIElementTypeLink", 9, "true", 10.0)).Click(false);
					await Task.Delay(TimeSpan.FromSeconds(5.0));
					TaskAwaiter<bool> taskAwaiter = this.AddToFavorites().GetAwaiter();
					if (!taskAwaiter.IsCompleted)
					{
						await taskAwaiter;
						TaskAwaiter<bool> taskAwaiter2;
						taskAwaiter = taskAwaiter2;
						taskAwaiter2 = default(TaskAwaiter<bool>);
					}
					if (taskAwaiter.GetResult())
					{
						this.WriteLog("[" + this.phoneUID + "]: Music Added Successfull!");
						break;
					}
					this.WriteLog("[" + this.phoneUID + "]: MUSIC ADDED UNSUCCESSFULL IDK WHY");
				}
				catch (Exception ex)
				{
					this.WriteLog("[" + this.phoneUID + "]: CRITICAL MUSIC EXCEPTION: " + ex.ToString());
				}
				await this.tiktokUtils.GoHome(true);
				await Task.Delay(TimeSpan.FromSeconds(2.0));
				this.WriteLog("[" + this.phoneUID + "]: Terminate Notes");
				await this.device.TerminateApp("com.apple.mobilenotes");
				await this.tiktokUtils.TerminateTikTok();
			}
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00010614 File Offset: 0x0000E814
		public async Task<bool> AddToFavorites()
		{
			int num = 0;
			try
			{
				int num2 = 0;
				try
				{
					this.WriteLog("[" + this.phoneUID + "]: Click Add Favorites button");
					int num3 = 0;
					try
					{
						await (await this.device.WaitForElementBy(0, "Add to Favorites", 10.0)).TapAlert(0, 0, false);
					}
					catch
					{
						num3 = 1;
					}
					if (num3 == 1)
					{
						await (await this.device.WaitForElementBy(0, "Add to Favourites", 10.0)).TapAlert(0, 0, false);
					}
					this.WriteLog("[" + this.phoneUID + "]: Search for Got It button");
					try
					{
						await (await this.device.WaitForElementBy(0, "Got it", 3.0)).TapAlert(0, 0, false);
					}
					catch
					{
						this.WriteLog("[" + this.phoneUID + "]: Don't Button");
					}
				}
				catch
				{
					num2 = 1;
				}
				if (num2 == 1)
				{
					this.WriteLog("[" + this.phoneUID + "]: Check if added already");
					Element element = await this.device.WaitForElementBy(0, "Added to Favorites", 5.0);
					if (element == null)
					{
						element = await this.device.WaitForElementBy(0, "Added to Favourites", 5.0);
					}
					if (element != null)
					{
						this.WriteLog("[" + this.phoneUID + "]: Already added");
						return true;
					}
					this.WriteLog("[" + this.phoneUID + "]: Search for Got It button");
					try
					{
						await (await this.device.WaitForElementBy(0, "Got it", 10.0)).TapAlert(0, 0, false);
					}
					catch
					{
						this.WriteLog("[" + this.phoneUID + "]: Don't Button");
					}
					this.WriteLog("[" + this.phoneUID + "]: Click Add Favorites button");
					int num3 = 0;
					try
					{
						await (await this.device.WaitForElementBy(0, "Add to Favorites", 10.0)).TapAlert(0, 0, false);
					}
					catch
					{
						num3 = 1;
					}
					if (num3 == 1)
					{
						await (await this.device.WaitForElementBy(0, "Add to Favourites", 10.0)).TapAlert(0, 0, false);
					}
				}
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
				this.WriteLog("[" + this.phoneUID + "]: Eception: " + ex.ToString());
				this.WriteLog("[" + this.phoneUID + "]: Probably already addedhecking...");
				try
				{
					this.WriteLog("[" + this.phoneUID + "]: Check if added already");
					Element element2 = await this.device.WaitForElementBy(0, "Added to Favorites", 5.0);
					if (element2 == null)
					{
						element2 = await this.device.WaitForElementBy(0, "Added to Favourites", 5.0);
					}
					if (element2 != null)
					{
						this.WriteLog("[" + this.phoneUID + "]: Already added");
						flag = true;
					}
					else
					{
						string text = this.phoneUID;
						this.WriteLog("[" + text + "]: Something wrong..." + await element2.Label());
						text = null;
						flag = false;
					}
				}
				catch
				{
					this.WriteLog("[" + this.phoneUID + "]: Add Music to Favorites Exception!");
					flag = false;
				}
			}
			return flag;
		}

		// Token: 0x06000083 RID: 131 RVA: 0x00010658 File Offset: 0x0000E858
		public async Task<bool> ChangePicture()
		{
			int num = 0;
			try
			{
				this.WriteLog("[" + this.phoneUID + "]: Getting random picture");
				string randomProfilePicture = FileSystemUtils.GetRandomProfilePicture();
				this.WriteLog("[" + this.phoneUID + "]: Pushing picture");
				await FileSystemUtils.RefreshPicture(this.phoneUID, randomProfilePicture, this.originalPictureUrl, this.device.PhoneModel.Contains("X"));
				this.WriteLog("[" + this.phoneUID + "]: Click Change button");
				await (await this.device.WaitForElementBy(3, "Change", 10.0)).Click(false);
				await Task.Delay(TimeSpan.FromSeconds(2.0));
				this.WriteLog("[" + this.phoneUID + "]: Click Upload photo button");
				await this.device.Tap(150.0, 760.0);
				this.WriteLog("[" + this.phoneUID + "]: Checking for Allow Allert button");
				try
				{
					await (await this.device.WaitForElementBy(3, "Allow Full Access", 5.0)).Click(false);
				}
				catch
				{
				}
				await Task.Delay(TimeSpan.FromSeconds(4.0));
				this.WriteLog("[" + this.phoneUID + "]: Selecting first picture");
				await this.device.Tap(30.0, 120.0);
				this.WriteLog("[" + this.phoneUID + "]: Click Save button");
				int i = 0;
				try
				{
					await (await this.device.WaitForElementBy(3, "Save", 10.0)).Click(false);
				}
				catch
				{
					i = 1;
				}
				if (i == 1)
				{
					this.WriteLog("[" + this.phoneUID + "]: Selecting first picture again");
					await this.device.Tap(30.0, 120.0);
					this.WriteLog("[" + this.phoneUID + "]: Click Save button");
					await (await this.device.WaitForElementBy(3, "Save", 10.0)).Click(false);
				}
				await Task.Delay(TimeSpan.FromSeconds(2.0));
				try
				{
					for (i = 0; i < 10; i++)
					{
						this.WriteLog("[" + this.phoneUID + "]: Second click Save button");
						await (await this.device.WaitForElementBy(3, "Save", 3.0)).Click(false);
					}
				}
				catch
				{
				}
				await Task.Delay(TimeSpan.FromSeconds(7.0));
				this.WriteLog("[" + this.phoneUID + "]: Check if picture saved");
				TaskAwaiter<Element> taskAwaiter = this.device.WaitForElementBy(0, "Crop", 4.0).GetAwaiter();
				if (!taskAwaiter.IsCompleted)
				{
					await taskAwaiter;
					TaskAwaiter<Element> taskAwaiter2;
					taskAwaiter = taskAwaiter2;
					taskAwaiter2 = default(TaskAwaiter<Element>);
				}
				if (taskAwaiter.GetResult() == null)
				{
					this.WriteLog("[" + this.phoneUID + "]: Picture saved successfully!");
					return true;
				}
				this.WriteLog("[" + this.phoneUID + "]: Picture was not saved!");
				Interlocked.Increment(ref this.BadPictures);
				return false;
			}
			catch
			{
				num = 1;
			}
			bool flag;
			if (num == 1)
			{
				this.WriteLog("[" + this.phoneUID + "]: Exception!");
				await this.tiktokUtils.SaveScreenShot(true, TikTokUtils.ScreenshotType.Working);
				flag = false;
			}
			return flag;
		}

		// Token: 0x06000084 RID: 132 RVA: 0x0001069C File Offset: 0x0000E89C
		public async Task<string> CheckUserName()
		{
			this.WriteLog("[" + this.phoneUID + "]: Search for Username");
			string text = "Unknown";
			int num = 0;
			try
			{
				text = await (await this.device.WaitForElementBy(3, "tiktok.com", 10.0)).Value();
				try
				{
					text = text.Split(new char[] { '@' })[1].Replace("/", "").Replace("\\", "");
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
				try
				{
					this.WriteLog("[" + this.phoneUID + "]: Error! Search for username again");
					this.WriteLog("[" + this.phoneUID + "]: Click Username button");
					await (await this.device.WaitForElementBy(3, "Username", 10.0)).Click(false);
					text = await (await this.device.WaitForElementByXPath("XCUIElementTypeTextField", 2, "", 10.0)).Value();
					try
					{
						text = text.Split(new char[] { '@' })[1].Replace("/", "").Replace("\\", "");
					}
					catch
					{
					}
					this.WriteLog("[" + this.phoneUID + "]: Click Cancel button");
					await (await this.device.WaitForElementBy(3, "Cancel", 10.0)).Click(false);
				}
				catch
				{
				}
			}
			return text;
		}

		// Token: 0x06000085 RID: 133 RVA: 0x000106E0 File Offset: 0x0000E8E0
		public async Task<bool> ChangeName()
		{
			bool flag;
			try
			{
				this.WriteLog("[" + this.phoneUID + "]: Click Username button");
				await (await this.device.WaitForElementBy(3, "Name", 10.0)).Click(false);
				string text;
				Form1.ProfileSettings.Names.TryDequeue(out text);
				for (;;)
				{
					this.WriteLog("[" + this.phoneUID + "]: Search for textview");
					Element element = await this.device.WaitForElementByXPath("XCUIElementTypeTextField", 8, "0", 10.0);
					this.WriteLog("[" + this.phoneUID + "]: Writing new username");
					await element.ClearText();
					await element.SetText(text);
					this.WriteLog("[" + this.phoneUID + "]: Checking if username is avalible");
					TaskAwaiter<Element> taskAwaiter = this.device.WaitForElementBy(3, "This username isn", 7.0).GetAwaiter();
					if (!taskAwaiter.IsCompleted)
					{
						await taskAwaiter;
						TaskAwaiter<Element> taskAwaiter2;
						taskAwaiter = taskAwaiter2;
						taskAwaiter2 = default(TaskAwaiter<Element>);
					}
					if (taskAwaiter.GetResult() == null)
					{
						break;
					}
					this.WriteLog("[" + this.phoneUID + "]: Username is not avalible");
					Form1.ProfileSettings.Names.TryDequeue(out text);
				}
				this.WriteLog("[" + this.phoneUID + "]: Click Save button");
				await (await this.device.WaitForElementBy(3, "Save", 10.0)).Click(false);
				await Task.Delay(TimeSpan.FromSeconds(1.0));
				this.WriteLog("[" + this.phoneUID + "]: Click Confirm Alert");
				List<Element> list = await this.device.WaitForElementsBy(0, "Confirm", 10.0);
				foreach (Element element2 in list)
				{
					try
					{
						await element2.TapAlert(0, 0, false);
					}
					catch
					{
					}
				}
				List<Element>.Enumerator enumerator = default(List<Element>.Enumerator);
				flag = true;
			}
			catch
			{
				flag = false;
			}
			return flag;
		}

		// Token: 0x06000086 RID: 134 RVA: 0x00010724 File Offset: 0x0000E924
		public async Task<bool> ClearBio()
		{
			bool flag = false;
			int num2;
			for (;;)
			{
				int num = 0;
				try
				{
					this.WriteLog("[" + this.phoneUID + "]: Click Bio button");
					await (await this.device.WaitForElementBy(0, "Bio", 10.0)).Click(false);
				}
				catch
				{
					num = 1;
				}
				num2 = num;
				if (num2 == 1)
				{
					await this.tiktokUtils.RunTikTokOpenUrl(TikTokUtils.TikTokOpenUrl.Edit);
					this.WriteLog("[" + this.phoneUID + "]: Click Bio button");
					await (await this.device.WaitForElementBy(0, "Bio", 15.0)).Click(false);
				}
				this.WriteLog("[" + this.phoneUID + "]: Checking for bio");
				TaskAwaiter<Element> taskAwaiter = this.device.WaitForElementBy(3, "0\\/160", 4.0).GetAwaiter();
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
				num = 0;
				try
				{
					this.WriteLog("[" + this.phoneUID + "]: Searching for Bio TextBox");
					await (await this.device.WaitForElementByXPath("XCUIElementTypeTextView", 9, "true", 10.0)).ClearText();
				}
				catch
				{
					num = 1;
				}
				num2 = num;
				if (num2 != 1)
				{
					goto IL_051D;
				}
				this.WriteLog("[" + this.phoneUID + "]: Error!");
				if (flag)
				{
					goto IL_04B1;
				}
				flag = true;
			}
			this.WriteLog("[" + this.phoneUID + "]: Bio is empty");
			return true;
			IL_04B1:
			await this.tiktokUtils.SaveScreenShot(true, TikTokUtils.ScreenshotType.Working);
			return false;
			IL_051D:
			num2 = 0;
			try
			{
				this.WriteLog("[" + this.phoneUID + "]: Click Save button");
				await (await this.device.WaitForElementBy(0, "Save", 10.0)).Click(false);
				await Task.Delay(TimeSpan.FromSeconds(2.0));
				DateTime dateTime = DateTime.Now.AddSeconds(10.0);
				for (;;)
				{
					this.WriteLog("[" + this.phoneUID + "]: Checking for success...");
					await this.device.GetSourceXml();
					if (!this.device.IsElementInXml("Save", null) && !this.device.IsElementInXmlNotVisible("Save", null))
					{
						break;
					}
					if (DateTime.Now > dateTime)
					{
						goto Block_17;
					}
				}
				this.WriteLog("[" + this.phoneUID + "]: Bio Updated Successfully!");
				return true;
				Block_17:
				this.WriteLog("[" + this.phoneUID + "]: Bio Update Error || SLOW!");
				return false;
			}
			catch
			{
				num2 = 1;
			}
			bool flag2;
			if (num2 == 1)
			{
				this.WriteLog("[" + this.phoneUID + "]: Error!");
				await this.tiktokUtils.SaveScreenShot(true, TikTokUtils.ScreenshotType.Working);
				flag2 = false;
			}
			return flag2;
		}

		// Token: 0x06000087 RID: 135 RVA: 0x00010768 File Offset: 0x0000E968
		public async Task<bool> UpdateBio(bool Making)
		{
			bool flag = false;
			string text;
			bool flag3;
			for (;;)
			{
				int num = 0;
				try
				{
					this.WriteLog("[" + this.phoneUID + "]: Click Bio button");
					List<Element> list = await this.device.WaitForElementsBy(0, "Bio", 10.0);
					foreach (Element element in list)
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
				}
				catch
				{
					num = 1;
				}
				int num2 = num;
				if (num2 == 1)
				{
					this.WriteLog("[" + this.phoneUID + "]: Reload TikTok Edit Page");
					await this.tiktokUtils.RunTikTokOpenUrl(TikTokUtils.TikTokOpenUrl.Edit);
					this.WriteLog("[" + this.phoneUID + "]: Click Bio button");
					await (await this.device.WaitForElementBy(0, "Bio", 15.0)).Click(false);
				}
				num = 0;
				try
				{
					this.WriteLog("[" + this.phoneUID + "]: Searching for Bio TextBox");
					for (;;)
					{
						text = FileSystemUtils.GetBio(Making);
						if (string.IsNullOrEmpty("Making"))
						{
							object obj = Worker.sync;
							lock (obj)
							{
								string text2 = "Posting";
								if (Making)
								{
									text2 = "Making";
								}
								MessageBox.Show("Bio for " + text2 + " is Empty!");
								continue;
							}
							break;
						}
						break;
					}
					if (Settings.Default.MentionInBio)
					{
						text += " ";
					}
					this.WriteLog("[" + this.phoneUID + "]: Write Bio: " + text);
					if (text.Contains("#BREAK#"))
					{
						text = text.Replace("#BREAK#", "#");
						string[] array = text.Split(new char[] { '#' });
						int num3 = array.Length;
						int num4 = 1;
						foreach (string text3 in array)
						{
							this.WriteLog("[" + this.phoneUID + "]: Write: " + num4.ToString());
							string text3;
							await (await this.device.WaitForElementByXPath("XCUIElementTypeTextView", 9, "true", 10.0)).SetText(text3);
							if (num3 > num4)
							{
								this.WriteLog("[" + this.phoneUID + "]: Click Return");
								int num5 = 0;
								try
								{
									await (await this.device.WaitForElementBy(0, "Return", 10.0)).Click(false);
								}
								catch
								{
									num5 = 1;
								}
								num2 = num5;
								if (num2 == 1)
								{
									await (await this.device.WaitForElementBy(0, "return", 10.0)).Click(false);
								}
								num2 = num4++;
							}
							text3 = null;
						}
						string[] array2 = null;
						if (Settings.Default.MentionInBio)
						{
							string text3 = FormUtils.GetMentionFromList(this.UsedMentions);
							this.UsedMentions.Add(text3);
							this.WriteLog("[" + this.phoneUID + "]: Click on Mention");
							await (await this.device.WaitForElementBy(0, "Mention", 10.0)).Click(false);
							this.WriteLog("[" + this.phoneUID + "]: Write Mention Username: " + text3);
							await (await this.device.WaitForElementByXPath("XCUIElementTypeTextView", 9, "true", 10.0)).SetText(text3);
							await Task.Delay(TimeSpan.FromSeconds(5.0));
							int i = 0;
							try
							{
								this.WriteLog("[" + this.phoneUID + "]: Serach by Images");
								await (await this.device.WaitForElementsByXPath("XCUIElementTypeImage", 9, "true", 10.0))[0].Click(false);
							}
							catch
							{
								i = 1;
							}
							num2 = i;
							if (num2 == 1)
							{
								this.WriteLog("[" + this.phoneUID + "]: Click By Coords");
								await this.device.Tap(20.0, 515.0);
							}
							text3 = null;
						}
					}
					else
					{
						Element element2 = await this.device.WaitForElementByXPath("XCUIElementTypeTextView", 9, "true", 10.0);
						try
						{
							await element2.ClearText();
						}
						catch
						{
						}
						await element2.SetText(text);
						if (Settings.Default.MentionInBio)
						{
							string text3 = FormUtils.GetMentionFromList(this.UsedMentions);
							this.UsedMentions.Add(text3);
							this.WriteLog("[" + this.phoneUID + "]: Click on Mention");
							await (await this.device.WaitForElementBy(0, "Mention", 10.0)).Click(false);
							this.WriteLog("[" + this.phoneUID + "]: Write Mention Username: " + text3);
							await (await this.device.WaitForElementByXPath("XCUIElementTypeTextView", 9, "true", 10.0)).SetText(text3);
							await Task.Delay(TimeSpan.FromSeconds(5.0));
							int num4 = 0;
							try
							{
								this.WriteLog("[" + this.phoneUID + "]: Serach by Images");
								await (await this.device.WaitForElementsByXPath("XCUIElementTypeImage", 9, "true", 10.0))[0].Click(false);
							}
							catch
							{
								num4 = 1;
							}
							num2 = num4;
							if (num2 == 1)
							{
								this.WriteLog("[" + this.phoneUID + "]: Click By Coords");
								await this.device.Tap(20.0, 515.0);
							}
							text3 = null;
						}
						element2 = null;
					}
					text = null;
				}
				catch
				{
					num = 1;
				}
				num2 = num;
				if (num2 == 1)
				{
					break;
				}
				num2 = 0;
				try
				{
					this.WriteLog("[" + this.phoneUID + "]: Click Save button");
					await (await this.device.WaitForElementBy(0, "Save", 10.0)).Click(false);
					await Task.Delay(TimeSpan.FromSeconds(2.0));
					DateTime dateTime = DateTime.Now.AddSeconds(10.0);
					for (;;)
					{
						this.WriteLog("[" + this.phoneUID + "]: Checking for success...");
						await this.device.GetSourceXml();
						if (!this.device.IsElementInXml("Save", null) && !this.device.IsElementInXmlNotVisible("Save", null))
						{
							break;
						}
						if (DateTime.Now > dateTime)
						{
							goto Block_49;
						}
					}
					this.WriteLog("[" + this.phoneUID + "]: Bio Updated Successfully!");
					return true;
					Block_49:
					this.WriteLog("[" + this.phoneUID + "]: Bio Update Error || SLOW!");
					return false;
				}
				catch
				{
					num2 = 1;
				}
				if (num2 != 1)
				{
					return flag3;
				}
				if (flag)
				{
					goto IL_184C;
				}
				flag = true;
				await this.tiktokUtils.TerminateTikTok();
				await Task.Delay(TimeSpan.FromSeconds(2.0));
				this.WriteLog("[" + this.phoneUID + "]: Reload TikTok Edit Page");
				await this.tiktokUtils.RunTikTokOpenUrl(TikTokUtils.TikTokOpenUrl.Edit);
				await Task.Delay(TimeSpan.FromSeconds(2.0));
			}
			text = this.phoneUID;
			this.WriteLog("[" + text + "]: Error! Source: " + await this.device.GetSource());
			text = null;
			await this.tiktokUtils.SaveScreenShot(true, TikTokUtils.ScreenshotType.Working);
			return false;
			IL_184C:
			text = this.phoneUID;
			this.WriteLog("[" + text + "]: Error! Source: " + await this.device.GetSource());
			text = null;
			await this.tiktokUtils.SaveScreenShot(true, TikTokUtils.ScreenshotType.Working);
			flag3 = false;
			return flag3;
		}

		// Token: 0x06000088 RID: 136 RVA: 0x000107B4 File Offset: 0x0000E9B4
		private async Task<Worker.LoginResult> FillProfiile(AnyMessageClient anyMessageClient, string MAKIGN_REGION, bool EmailConfirmed)
		{
			bool flag = false;
			Worker.LoginResult loginResult;
			for (;;)
			{
				int num = 0;
				try
				{
					TikTokUtils.ActivateResult activateResult = await this.tiktokUtils.ActivateTikTokGotoProfile();
					if (activateResult != TikTokUtils.ActivateResult.Success)
					{
						switch (activateResult)
						{
						case TikTokUtils.ActivateResult.Unsuccess:
							return Worker.LoginResult.BadAccount;
						case TikTokUtils.ActivateResult.UnAuth:
							return Worker.LoginResult.Risk;
						case TikTokUtils.ActivateResult.Blocked:
							return Worker.LoginResult.BadAccount;
						case TikTokUtils.ActivateResult.AccountStatus:
							return Worker.LoginResult.Risk;
						default:
							return Worker.LoginResult.BadAccount;
						}
					}
					else
					{
						await this.device.ConfigurateAppium(21);
						this.WriteLog("[" + this.phoneUID + "]: Click Edit Button");
						int num2 = 0;
						try
						{
							await (await this.device.WaitForElementBy(0, "Edit", 5.0)).Click(false);
						}
						catch
						{
							num2 = 1;
						}
						int num3 = num2;
						if (num3 == 1)
						{
							int num4 = 0;
							try
							{
								await (await this.device.WaitForElementBy(0, "user_info_manage_edit_profile", 5.0)).Click(false);
							}
							catch
							{
								num4 = 1;
							}
							num3 = num4;
							if (num3 == 1)
							{
								await this.tiktokUtils.RunTikTokOpenUrl(TikTokUtils.TikTokOpenUrl.Edit);
							}
						}
						bool flag2 = false;
						string text = await this.CheckUserName();
						if (Settings.Default.UploadPicture)
						{
							flag2 = await this.ChangePicture();
						}
						if ((Settings.Default.MentionInBio || Settings.Default.UpdateBio) && Settings.Default.BioAfterCreating)
						{
							bool flag3 = await this.UpdateBio(true);
							if (flag3)
							{
								this.WriteLog("[" + this.phoneUID + "]: Mention in bio success");
							}
							else
							{
								this.WriteLog("[" + this.phoneUID + "]: Mention in bio unsuccess");
							}
							if (!flag2)
							{
								flag2 = flag3;
							}
						}
						else if (!Settings.Default.UploadPicture)
						{
							flag2 = true;
						}
						if (!flag2)
						{
							this.WriteLog("[" + this.phoneUID + "]: Click Back");
							try
							{
								await (await this.device.WaitForElementBy(0, "Back", 10.0)).Click(false);
							}
							catch
							{
							}
							try
							{
								this.WriteLog("[" + this.phoneUID + "]: Checking for Save Password Alert || Close Q");
								await (await this.device.WaitForElementBy(0, "close", 3.0)).Click(false);
								goto IL_1769;
							}
							catch
							{
								goto IL_1769;
							}
						}
						TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter5;
						TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter6;
						if (!EmailConfirmed && !Settings.Default.SkipReLogin)
						{
							this.WriteLog("[" + this.phoneUID + "]: Click Back | !EC");
							try
							{
								await (await this.device.WaitForElementBy(0, "Back", 10.0)).Click(false);
							}
							catch
							{
							}
							try
							{
								this.WriteLog("[" + this.phoneUID + "]: Checking for Save Password Alert || Close W");
								await (await this.device.WaitForElementBy(0, "close", 3.0)).Click(false);
							}
							catch
							{
							}
							if (!Settings.Default.emailDomain.Contains("long_"))
							{
								await anyMessageClient.ReorderByIdAsync(anyMessageClient.emailDataC.id, null, null);
							}
							await this.tiktokUtils.LogOutFromAccount(text, true, false);
							this.WriteLog("[" + this.phoneUID + "]: Getting Source");
							await this.device.GetSourceXml();
							if (this.device.IsElementInXmlContains("Use phone ", null))
							{
								this.WriteLog("[" + this.phoneUID + "]: Click Use Phone Button");
								await (await this.device.WaitForElementBy(3, "Use phone ", 10.0)).Click(false);
								this.WriteLog("[" + this.phoneUID + "]: Maker Set up Email");
								this.WriteLog("[" + this.phoneUID + "]: Maker Set up Email F");
								await this.Maker.SetUpEmail(anyMessageClient, false);
								await Task.Delay(TimeSpan.FromSeconds(2.0));
								this.WriteLog("[" + this.phoneUID + "]: Getting Source");
								await this.device.GetSourceXml();
								if (this.device.IsElementInXml("Enter password", null) || this.device.IsElementInXml("Password", null))
								{
									await this.Maker.SetUpPassword();
								}
							}
							this.WriteLog("[" + this.phoneUID + "]: Waiting 5 secounds for loading");
							await Task.Delay(TimeSpan.FromSeconds(5.0));
							for (;;)
							{
								TaskAwaiter<AuthUtils.VerifyEmailResult> taskAwaiter = AuthUtils.VerifyEmail(anyMessageClient, this.device, this.phoneUID, Settings.Default.emailDomain.Contains("long_")).GetAwaiter();
								if (!taskAwaiter.IsCompleted)
								{
									await taskAwaiter;
									TaskAwaiter<AuthUtils.VerifyEmailResult> taskAwaiter2;
									taskAwaiter = taskAwaiter2;
									taskAwaiter2 = default(TaskAwaiter<AuthUtils.VerifyEmailResult>);
								}
								switch (taskAwaiter.GetResult())
								{
								case AuthUtils.VerifyEmailResult.Success:
								{
									TaskAwaiter<Element> taskAwaiter3 = this.device.WaitForElementBy(3, "Verification code is expired", 4.0).GetAwaiter();
									if (!taskAwaiter3.IsCompleted)
									{
										await taskAwaiter3;
										TaskAwaiter<Element> taskAwaiter4;
										taskAwaiter3 = taskAwaiter4;
										taskAwaiter4 = default(TaskAwaiter<Element>);
									}
									if (taskAwaiter3.GetResult() != null)
									{
										if (!Settings.Default.emailDomain.Contains("long_"))
										{
											await anyMessageClient.ReorderByIdAsync(anyMessageClient.emailDataC.id, null, null);
										}
										this.WriteLog("[" + this.phoneUID + "]: Click Back Button");
										await (await this.device.WaitForElementBy(3, "back", 10.0)).Click(false);
										this.WriteLog(string.Concat(new string[] { "[", this.phoneUID, "]: Click Username: ", text, " Button" }));
										await (await this.device.WaitForElementBy(0, text, 10.0)).Click(false);
										continue;
									}
									goto IL_1451;
								}
								case AuthUtils.VerifyEmailResult.TimeOut:
									goto IL_159D;
								case AuthUtils.VerifyEmailResult.Exception:
									goto IL_1683;
								}
								break;
							}
							goto IL_1769;
							IL_1451:
							EmailConfirmed = true;
							taskAwaiter5 = this.tiktokUtils.ActivateTikTokGotoProfile().GetAwaiter();
							if (!taskAwaiter5.IsCompleted)
							{
								await taskAwaiter5;
								taskAwaiter5 = taskAwaiter6;
								taskAwaiter6 = default(TaskAwaiter<TikTokUtils.ActivateResult>);
							}
							if (taskAwaiter5.GetResult() != TikTokUtils.ActivateResult.Success)
							{
								return Worker.LoginResult.BadAccount;
							}
							await (await this.device.WaitForElementBy(0, "Edit", 5.0)).Click(false);
							goto IL_1769;
							IL_159D:
							try
							{
								await this.tiktokUtils.TerminateTikTok();
							}
							catch
							{
							}
							try
							{
								await this.tiktokUtils.GoHome(false);
							}
							catch
							{
							}
							return Worker.LoginResult.Exception;
							IL_1683:
							try
							{
								await this.tiktokUtils.TerminateTikTok();
							}
							catch
							{
							}
							try
							{
								await this.tiktokUtils.GoHome(false);
							}
							catch
							{
							}
							return Worker.LoginResult.Exception;
						}
						IL_1769:
						if (flag2)
						{
							this.WriteLog("[" + this.phoneUID + "]: Account is ok");
							if (Settings.Default.NameFromTxt)
							{
								await this.ChangeName();
							}
							bool flag4 = false;
							if (Settings.Default.AddComments)
							{
								if (!flag4)
								{
									this.WriteLog("[" + this.phoneUID + "]: Click Back");
									try
									{
										await (await this.device.WaitForElementBy(0, "Back", 10.0)).Click(false);
										flag4 = true;
									}
									catch
									{
									}
								}
								try
								{
									await this.FilterComments();
								}
								catch
								{
								}
							}
							bool flag5 = true;
							if (Settings.Default.Use2FA)
							{
								if (!flag4)
								{
									this.WriteLog("[" + this.phoneUID + "]: Click Back");
									try
									{
										await (await this.device.WaitForElementBy(0, "Back", 10.0)).Click(false);
									}
									catch
									{
									}
								}
								try
								{
									flag5 = await this.TurnOn2FA(text, ConstParams.CONST_PASSWORD);
									Form1.OutData.FA2Accounts.Enqueue(string.Format("{0}:{1}:{2}:{3}{4}:{5}:{6}:{7}{8}", new object[]
									{
										this.phoneUID,
										text,
										ConstParams.CONST_PASSWORD,
										Settings.Default.emailDomain.Contains("long_") ? "long_" : "",
										anyMessageClient.emailDataC.email,
										MAKIGN_REGION,
										anyMessageClient.emailDataC.id,
										DateTime.Now,
										flag5 ? "" : "  - 2FA ERROR"
									}));
								}
								catch
								{
								}
							}
							num2 = 0;
							for (;;)
							{
								num3 = 0;
								try
								{
									string text2 = string.Format("NO_BACKUP:{0}:{1}:{2}{3}:{4}:{5}:{6} :{7}:Phone - {8}", new object[]
									{
										text,
										ConstParams.CONST_PASSWORD,
										Settings.Default.emailDomain.Contains("long_") ? "long_" : "",
										anyMessageClient.emailDataC.email,
										MAKIGN_REGION,
										anyMessageClient.emailDataC.id,
										DateTime.Now,
										flag5 ? ("2FA " + Settings.Default.Use2FA.ToString()) : "  - 2FA ERROR",
										this.phoneUID
									});
									Form1.OutData.AccountsWithData.Enqueue(text2);
									Form1.OutData.UserNamesOnly.Enqueue(text);
									await this.UploadToDataBase(text2);
									if (!flag4)
									{
										this.WriteLog("[" + this.phoneUID + "]: Click Back");
										try
										{
											await (await this.device.WaitForElementBy(0, "Back", 10.0)).Click(false);
											flag4 = true;
										}
										catch
										{
										}
									}
									return Worker.LoginResult.GoodAccount;
								}
								catch (Exception obj)
								{
									num3 = 1;
								}
								if (num3 == 1)
								{
									object obj;
									Exception ex = (Exception)obj;
									this.WriteLog("[" + this.phoneUID + "]: Signing exeption: " + ex.ToString());
									num2++;
									if (num2 >= 3)
									{
										break;
									}
									await Task.Delay(TimeSpan.FromSeconds(5.0));
								}
							}
							this.WriteLog("[" + this.phoneUID + "]: Some error. Going to make new account");
							if (!flag4)
							{
								this.WriteLog("[" + this.phoneUID + "]: Click Back");
								try
								{
									await (await this.device.WaitForElementBy(0, "Back", 10.0)).Click(false);
									flag4 = true;
								}
								catch
								{
								}
							}
							return Worker.LoginResult.BadAccount;
						}
						this.WriteLog("[" + this.phoneUID + "]: Antispam error");
						await this.tiktokUtils.GoHome(true);
						try
						{
							await this.tiktokUtils.TerminateTikTok();
						}
						catch
						{
							this.WriteLog("[" + this.phoneUID + "]: Terminate TikTok Error");
						}
						taskAwaiter5 = this.tiktokUtils.ActivateTikTokGotoProfile().GetAwaiter();
						if (!taskAwaiter5.IsCompleted)
						{
							await taskAwaiter5;
							taskAwaiter5 = taskAwaiter6;
							taskAwaiter6 = default(TaskAwaiter<TikTokUtils.ActivateResult>);
						}
						if (taskAwaiter5.GetResult() != TikTokUtils.ActivateResult.Success)
						{
							return Worker.LoginResult.BadAccount;
						}
						try
						{
							await this.tiktokUtils.LogOutFromAccount(null, false, true);
						}
						catch
						{
						}
						return Worker.LoginResult.BadAccount;
					}
				}
				catch (Exception obj2)
				{
					num = 1;
				}
				if (num != 1)
				{
					return loginResult;
				}
				object obj2;
				Exception ex2 = (Exception)obj2;
				this.WriteLog("[" + this.phoneUID + "]: Filling profile exception: " + ex2.ToString());
				if (!flag)
				{
					flag = true;
					try
					{
						await this.tiktokUtils.TerminateTikTok();
					}
					catch
					{
					}
					try
					{
						await this.tiktokUtils.GoHome(false);
						continue;
					}
					catch
					{
						continue;
					}
					break;
				}
				break;
			}
			loginResult = Worker.LoginResult.Exception;
			return loginResult;
		}

		// Token: 0x06000089 RID: 137 RVA: 0x00010810 File Offset: 0x0000EA10
		public async Task FilterComments()
		{
			int num = 0;
			object obj2;
			TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter3;
			do
			{
				int num2 = 0;
				try
				{
					await this.tiktokUtils.RunTikTokOpenUrl(TikTokUtils.TikTokOpenUrl.Setting);
					int num3 = 0;
					object obj;
					Exception ex;
					for (;;)
					{
						int num4 = 0;
						try
						{
							try
							{
								this.WriteLog("[" + this.phoneUID + "]: Click Privacy");
								await (await this.device.WaitForElementBy(0, "Privacy", 10.0)).Click(false);
								await Task.Delay(TimeSpan.FromSeconds(1.0));
								this.WriteLog("[" + this.phoneUID + "]: Swipe");
								await this.device.Swipe(250, 400, 250, 250, 0.0);
								await Task.Delay(TimeSpan.FromSeconds(1.0));
							}
							catch
							{
							}
							try
							{
								this.WriteLog("[" + this.phoneUID + "]: Click Comments");
								Position position = await (await this.device.WaitForElementBy(0, "Interactions", 10.0)).GetPosition();
								await this.device.Tap(position.x, position.y + 33.0);
							}
							catch
							{
							}
							await Task.Delay(TimeSpan.FromSeconds(2.0));
							this.WriteLog("[" + this.phoneUID + "]: Click Allow Comments From by coords");
							await this.device.Tap(15.0, 120.0);
							await Task.Delay(TimeSpan.FromSeconds(3.0));
							int num5 = 0;
							try
							{
								this.WriteLog("[" + this.phoneUID + "]: Click Friends Element");
								await (await this.device.WaitForElementBy(3, "Followers you follow back", 10.0)).TapAlert(0, 20, false);
							}
							catch
							{
								num5 = 1;
							}
							if (num5 == 1)
							{
								this.WriteLog("[" + this.phoneUID + "]: Click Friends by coords");
								await this.device.Tap(15.0, 750.0);
							}
							await Task.Delay(TimeSpan.FromSeconds(5.0));
							this.WriteLog("[" + this.phoneUID + "]: Click Back Button");
							await (await this.device.WaitForElementBy(0, "back", 10.0)).Click(false);
							this.WriteLog("[" + this.phoneUID + "]: Click Back Button");
							await (await this.device.WaitForElementBy(0, "back", 10.0)).Click(false);
							this.WriteLog("[" + this.phoneUID + "]: Click Back Button");
							try
							{
								await (await this.device.WaitForElementBy(0, "back", 10.0)).Click(false);
							}
							catch
							{
							}
							TaskAwaiter<Element> taskAwaiter = this.device.WaitForElementBy(0, "Reuse of content setting", 3.0).GetAwaiter();
							if (!taskAwaiter.IsCompleted)
							{
								await taskAwaiter;
								TaskAwaiter<Element> taskAwaiter2;
								taskAwaiter = taskAwaiter2;
								taskAwaiter2 = default(TaskAwaiter<Element>);
							}
							if (taskAwaiter.GetResult() != null)
							{
								await (await this.device.WaitForElementBy(3, "Allow others to reuse these", 10.0)).TapAlert(0, 60, false);
								await (await this.device.WaitForElementBy(0, "Confirm", 10.0)).Click(false);
								this.WriteLog("[" + this.phoneUID + "]: Click Back Button");
								try
								{
									await (await this.device.WaitForElementBy(0, "back", 10.0)).Click(false);
								}
								catch
								{
								}
							}
							this.WriteLog("[" + this.phoneUID + "]: Click Back Button");
							try
							{
								await (await this.device.WaitForElementBy(0, "Back", 10.0)).Click(false);
							}
							catch
							{
							}
						}
						catch (Exception obj)
						{
							num4 = 1;
						}
						if (num4 != 1)
						{
							goto IL_1221;
						}
						ex = (Exception)obj;
						this.WriteLog("[" + this.phoneUID + "]: Exception: " + ex.ToString());
						try
						{
							await (await this.device.WaitForElementBy(0, "close", 3.0)).Click(false);
						}
						catch
						{
						}
						if (num3 >= 1)
						{
							break;
						}
						this.WriteLog("[" + this.phoneUID + "]: Trying again");
						num3++;
					}
					this.WriteLog("[" + this.phoneUID + "]: Its Error");
					throw ex;
					IL_1221:
					obj = null;
				}
				catch (Exception obj2)
				{
					num2 = 1;
				}
				if (num2 != 1)
				{
					goto IL_1342;
				}
				Exception ex2 = (Exception)obj2;
				num++;
				await this.tiktokUtils.TerminateTikTok();
				if (num > 3)
				{
					break;
				}
				taskAwaiter3 = this.tiktokUtils.ActivateTikTokGotoProfile().GetAwaiter();
				if (!taskAwaiter3.IsCompleted)
				{
					await taskAwaiter3;
					TaskAwaiter<TikTokUtils.ActivateResult> taskAwaiter4;
					taskAwaiter3 = taskAwaiter4;
					taskAwaiter4 = default(TaskAwaiter<TikTokUtils.ActivateResult>);
				}
			}
			while (taskAwaiter3.GetResult() == TikTokUtils.ActivateResult.Success);
			return;
			IL_1342:
			obj2 = null;
		}

		// Token: 0x040000E1 RID: 225
		private static object sync = new object();

		// Token: 0x040000E2 RID: 226
		public iDevice device;

		// Token: 0x040000E3 RID: 227
		public string PhoneModel = "";

		// Token: 0x040000E4 RID: 228
		public double iOSVersion = 13.0;

		// Token: 0x040000E5 RID: 229
		public string phoneUID;

		// Token: 0x040000E6 RID: 230
		public string port;

		// Token: 0x040000E7 RID: 231
		public string originalPictureUrl;

		// Token: 0x040000E8 RID: 232
		public string originalVideoUrl;

		// Token: 0x040000E9 RID: 233
		public int TotalPosted;

		// Token: 0x040000EA RID: 234
		public int TotalUnsuccessPosted;

		// Token: 0x040000EB RID: 235
		public int TotalGoodAccountss;

		// Token: 0x040000EC RID: 236
		public int TotalBadAccounts;

		// Token: 0x040000ED RID: 237
		public int TotalSlowTimes;

		// Token: 0x040000EE RID: 238
		public int TotalCaptchaTimes;

		// Token: 0x040000EF RID: 239
		public int BadPictures;

		// Token: 0x040000F0 RID: 240
		public int TotalReWorkTimes;

		// Token: 0x040000F1 RID: 241
		public int TotalReWorkAccounts;

		// Token: 0x040000F2 RID: 242
		public bool ConfigurateOnly;

		// Token: 0x040000F3 RID: 243
		public int AccountsOnVpn;

		// Token: 0x040000F4 RID: 244
		private int potok_;

		// Token: 0x040000F5 RID: 245
		public bool Work;

		// Token: 0x040000F6 RID: 246
		public bool Started;

		// Token: 0x040000F7 RID: 247
		public ConcurrentQueue<string> VPNRegionsMAKING = new ConcurrentQueue<string>();

		// Token: 0x040000F8 RID: 248
		public ConcurrentQueue<string> RegionsPOSTING = new ConcurrentQueue<string>();

		// Token: 0x040000F9 RID: 249
		public ConcurrentQueue<string> VPNRegionsPOSTING = new ConcurrentQueue<string>();

		// Token: 0x040000FA RID: 250
		public SignIn Signing = new SignIn();

		// Token: 0x040000FB RID: 251
		public Making Maker = new Making();

		// Token: 0x040000FC RID: 252
		public Poster Poster = new Poster();

		// Token: 0x040000FD RID: 253
		public TikTokUtils tiktokUtils = new TikTokUtils();

		// Token: 0x040000FE RID: 254
		private VPN vpn = new VPN(Settings.Default.UseVPN ? (Settings.Default.VpnType.Contains("Pia") ? VPN.VPNType.PiaVpn : VPN.VPNType.BelkaVPN) : VPN.VPNType.Shadowrocket, Settings.Default.ProxyType.Contains("Socks5") ? VPN.ProxyType.Socks5 : VPN.ProxyType.Http);

		// Token: 0x040000FF RID: 255
		public List<string> UsedMentions = new List<string>();

		// Token: 0x04000100 RID: 256
		private List<Task> Tasks = new List<Task>();

		// Token: 0x04000101 RID: 257
		private bool WorkStarted;

		// Token: 0x04000102 RID: 258
		private string REGION_MAKING = "";

		// Token: 0x04000103 RID: 259
		private string REGION_POSTING = "United States";

		// Token: 0x04000104 RID: 260
		private string REGION_POSTING_VPN = "";

		// Token: 0x0200002C RID: 44
		private enum LoginResult
		{
			// Token: 0x04000106 RID: 262
			GoodAccount,
			// Token: 0x04000107 RID: 263
			BadAccount,
			// Token: 0x04000108 RID: 264
			Risk,
			// Token: 0x04000109 RID: 265
			Blocked,
			// Token: 0x0400010A RID: 266
			Exception
		}

		// Token: 0x02000039 RID: 57
		[CompilerGenerated]
		private sealed class Class13
		{
			// Token: 0x040001A4 RID: 420
			public Dictionary<char, Element> elements;

			// Token: 0x040001A5 RID: 421
			public Worker Field0;
		}

		// Token: 0x0200003A RID: 58
		[CompilerGenerated]
		private sealed class Class14
		{
			// Token: 0x060000A6 RID: 166 RVA: 0x0001F83C File Offset: 0x0001DA3C
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

			// Token: 0x040001A6 RID: 422
			public char d;

			// Token: 0x040001A7 RID: 423
			public Worker.Class13 Field0;

			// Token: 0x0200003B RID: 59
			[StructLayout(LayoutKind.Auto)]
			private struct Struct31 : IAsyncStateMachine
			{
				// Token: 0x060000A7 RID: 167 RVA: 0x0001F880 File Offset: 0x0001DA80
				void IAsyncStateMachine.MoveNext()
				{
					int num2;
					int num = num2;
					Worker.Class14 @class = this;
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
									this.Field1.AwaitUnsafeOnCompleted<TaskAwaiter<Element>, Worker.Class14.Struct31>(ref taskAwaiter, ref this);
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

				// Token: 0x060000A8 RID: 168 RVA: 0x000023F0 File Offset: 0x000005F0
				[DebuggerHidden]
				void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
				{
					this.Field1.SetStateMachine(stateMachine);
				}

				// Token: 0x040001A8 RID: 424
				public int Field0;

				// Token: 0x040001A9 RID: 425
				public AsyncTaskMethodBuilder Field1;

				// Token: 0x040001AA RID: 426
				public Worker.Class14 Field2;

				// Token: 0x040001AB RID: 427
				private char Field3;

				// Token: 0x040001AC RID: 428
				private TaskAwaiter<Element> Field4;
			}
		}
	}
}
