using System;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TikTok_Automation_Library_Non_Jail.Properties
{
	// Token: 0x0200009D RID: 157
	[CompilerGenerated]
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.10.0.0")]
	internal sealed partial class Settings : ApplicationSettingsBase
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600019B RID: 411 RVA: 0x000028F1 File Offset: 0x00000AF1
		public static Settings Default
		{
			get
			{
				return Settings.defaultInstance;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600019C RID: 412 RVA: 0x000028F8 File Offset: 0x00000AF8
		// (set) Token: 0x0600019D RID: 413 RVA: 0x0000290A File Offset: 0x00000B0A
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool RemoveVideoAfterUse
		{
			get
			{
				return (bool)this["RemoveVideoAfterUse"];
			}
			set
			{
				this["RemoveVideoAfterUse"] = value;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600019E RID: 414 RVA: 0x0000291D File Offset: 0x00000B1D
		// (set) Token: 0x0600019F RID: 415 RVA: 0x0000292F File Offset: 0x00000B2F
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("6")]
		public int NeedToPost
		{
			get
			{
				return (int)this["NeedToPost"];
			}
			set
			{
				this["NeedToPost"] = value;
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x060001A0 RID: 416 RVA: 0x00002942 File Offset: 0x00000B42
		// (set) Token: 0x060001A1 RID: 417 RVA: 0x00002954 File Offset: 0x00000B54
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool AddMusic
		{
			get
			{
				return (bool)this["AddMusic"];
			}
			set
			{
				this["AddMusic"] = value;
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x060001A2 RID: 418 RVA: 0x00002967 File Offset: 0x00000B67
		// (set) Token: 0x060001A3 RID: 419 RVA: 0x00002979 File Offset: 0x00000B79
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool TurnOffAdditionalSound
		{
			get
			{
				return (bool)this["TurnOffAdditionalSound"];
			}
			set
			{
				this["TurnOffAdditionalSound"] = value;
			}
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x060001A4 RID: 420 RVA: 0x0000298C File Offset: 0x00000B8C
		// (set) Token: 0x060001A5 RID: 421 RVA: 0x0000299E File Offset: 0x00000B9E
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool TurnOffOriginalSound
		{
			get
			{
				return (bool)this["TurnOffOriginalSound"];
			}
			set
			{
				this["TurnOffOriginalSound"] = value;
			}
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x060001A6 RID: 422 RVA: 0x000029B1 File Offset: 0x00000BB1
		// (set) Token: 0x060001A7 RID: 423 RVA: 0x000029C3 File Offset: 0x00000BC3
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool AddRandomMusic
		{
			get
			{
				return (bool)this["AddRandomMusic"];
			}
			set
			{
				this["AddRandomMusic"] = value;
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x060001A8 RID: 424 RVA: 0x000029D6 File Offset: 0x00000BD6
		// (set) Token: 0x060001A9 RID: 425 RVA: 0x000029E8 File Offset: 0x00000BE8
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("True")]
		public bool UseVPN
		{
			get
			{
				return (bool)this["UseVPN"];
			}
			set
			{
				this["UseVPN"] = value;
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x060001AA RID: 426 RVA: 0x000029FB File Offset: 0x00000BFB
		// (set) Token: 0x060001AB RID: 427 RVA: 0x00002A0D File Offset: 0x00000C0D
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool UploadPicture
		{
			get
			{
				return (bool)this["UploadPicture"];
			}
			set
			{
				this["UploadPicture"] = value;
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x060001AC RID: 428 RVA: 0x00002A20 File Offset: 0x00000C20
		// (set) Token: 0x060001AD RID: 429 RVA: 0x00002A32 File Offset: 0x00000C32
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("True")]
		public bool UpdateBio
		{
			get
			{
				return (bool)this["UpdateBio"];
			}
			set
			{
				this["UpdateBio"] = value;
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x060001AE RID: 430 RVA: 0x00002A45 File Offset: 0x00000C45
		// (set) Token: 0x060001AF RID: 431 RVA: 0x00002A57 File Offset: 0x00000C57
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool MentionInBio
		{
			get
			{
				return (bool)this["MentionInBio"];
			}
			set
			{
				this["MentionInBio"] = value;
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x060001B0 RID: 432 RVA: 0x00002A6A File Offset: 0x00000C6A
		// (set) Token: 0x060001B1 RID: 433 RVA: 0x00002A7C File Offset: 0x00000C7C
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("YOUR_WIFI_NAME")]
		public string WifiName
		{
			get
			{
				return (string)this["WifiName"];
			}
			set
			{
				this["WifiName"] = value;
			}
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x060001B2 RID: 434 RVA: 0x00002A8A File Offset: 0x00000C8A
		// (set) Token: 0x060001B3 RID: 435 RVA: 0x00002A9C File Offset: 0x00000C9C
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("YOUR_WIFI_PASSWORD")]
		public string WifiPassword
		{
			get
			{
				return (string)this["WifiPassword"];
			}
			set
			{
				this["WifiPassword"] = value;
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x060001B4 RID: 436 RVA: 0x00002AAA File Offset: 0x00000CAA
		// (set) Token: 0x060001B5 RID: 437 RVA: 0x00002ABC File Offset: 0x00000CBC
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool AddTags
		{
			get
			{
				return (bool)this["AddTags"];
			}
			set
			{
				this["AddTags"] = value;
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x060001B6 RID: 438 RVA: 0x00002ACF File Offset: 0x00000CCF
		// (set) Token: 0x060001B7 RID: 439 RVA: 0x00002AE1 File Offset: 0x00000CE1
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("3")]
		public int TagsCount
		{
			get
			{
				return (int)this["TagsCount"];
			}
			set
			{
				this["TagsCount"] = value;
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x060001B8 RID: 440 RVA: 0x00002AF4 File Offset: 0x00000CF4
		// (set) Token: 0x060001B9 RID: 441 RVA: 0x00002B06 File Offset: 0x00000D06
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool PlusMention
		{
			get
			{
				return (bool)this["PlusMention"];
			}
			set
			{
				this["PlusMention"] = value;
			}
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x060001BA RID: 442 RVA: 0x00002B19 File Offset: 0x00000D19
		// (set) Token: 0x060001BB RID: 443 RVA: 0x00002B2B File Offset: 0x00000D2B
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool AddComments
		{
			get
			{
				return (bool)this["AddComments"];
			}
			set
			{
				this["AddComments"] = value;
			}
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x060001BC RID: 444 RVA: 0x00002B3E File Offset: 0x00000D3E
		// (set) Token: 0x060001BD RID: 445 RVA: 0x00002B50 File Offset: 0x00000D50
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool NameFromTxt
		{
			get
			{
				return (bool)this["NameFromTxt"];
			}
			set
			{
				this["NameFromTxt"] = value;
			}
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x060001BE RID: 446 RVA: 0x00002B63 File Offset: 0x00000D63
		// (set) Token: 0x060001BF RID: 447 RVA: 0x00002B75 File Offset: 0x00000D75
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool AddMentionToComment
		{
			get
			{
				return (bool)this["AddMentionToComment"];
			}
			set
			{
				this["AddMentionToComment"] = value;
			}
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x060001C0 RID: 448 RVA: 0x00002B88 File Offset: 0x00000D88
		// (set) Token: 0x060001C1 RID: 449 RVA: 0x00002B9A File Offset: 0x00000D9A
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool AddTextToComment
		{
			get
			{
				return (bool)this["AddTextToComment"];
			}
			set
			{
				this["AddTextToComment"] = value;
			}
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x060001C2 RID: 450 RVA: 0x00002BAD File Offset: 0x00000DAD
		// (set) Token: 0x060001C3 RID: 451 RVA: 0x00002BBF File Offset: 0x00000DBF
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool AddImageToComment
		{
			get
			{
				return (bool)this["AddImageToComment"];
			}
			set
			{
				this["AddImageToComment"] = value;
			}
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x060001C4 RID: 452 RVA: 0x00002BD2 File Offset: 0x00000DD2
		// (set) Token: 0x060001C5 RID: 453 RVA: 0x00002BE4 File Offset: 0x00000DE4
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("1")]
		public int ResetNetworkAfter
		{
			get
			{
				return (int)this["ResetNetworkAfter"];
			}
			set
			{
				this["ResetNetworkAfter"] = value;
			}
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x060001C6 RID: 454 RVA: 0x00002BF7 File Offset: 0x00000DF7
		// (set) Token: 0x060001C7 RID: 455 RVA: 0x00002C09 File Offset: 0x00000E09
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool FirstPosting
		{
			get
			{
				return (bool)this["FirstPosting"];
			}
			set
			{
				this["FirstPosting"] = value;
			}
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x060001C8 RID: 456 RVA: 0x00002C1C File Offset: 0x00000E1C
		// (set) Token: 0x060001C9 RID: 457 RVA: 0x00002C2E File Offset: 0x00000E2E
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("True")]
		public bool ResetNetworkInRun
		{
			get
			{
				return (bool)this["ResetNetworkInRun"];
			}
			set
			{
				this["ResetNetworkInRun"] = value;
			}
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x060001CA RID: 458 RVA: 0x00002C41 File Offset: 0x00000E41
		// (set) Token: 0x060001CB RID: 459 RVA: 0x00002C53 File Offset: 0x00000E53
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool RecordScreen
		{
			get
			{
				return (bool)this["RecordScreen"];
			}
			set
			{
				this["RecordScreen"] = value;
			}
		}

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x060001CC RID: 460 RVA: 0x00002C66 File Offset: 0x00000E66
		// (set) Token: 0x060001CD RID: 461 RVA: 0x00002C78 File Offset: 0x00000E78
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool UseShadowRocket
		{
			get
			{
				return (bool)this["UseShadowRocket"];
			}
			set
			{
				this["UseShadowRocket"] = value;
			}
		}

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x060001CE RID: 462 RVA: 0x00002C8B File Offset: 0x00000E8B
		// (set) Token: 0x060001CF RID: 463 RVA: 0x00002C9D File Offset: 0x00000E9D
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("Socks5")]
		public string ProxyType
		{
			get
			{
				return (string)this["ProxyType"];
			}
			set
			{
				this["ProxyType"] = value;
			}
		}

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x060001D0 RID: 464 RVA: 0x00002CAB File Offset: 0x00000EAB
		// (set) Token: 0x060001D1 RID: 465 RVA: 0x00002CBD File Offset: 0x00000EBD
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool Use2FA
		{
			get
			{
				return (bool)this["Use2FA"];
			}
			set
			{
				this["Use2FA"] = value;
			}
		}

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x060001D2 RID: 466 RVA: 0x00002CD0 File Offset: 0x00000ED0
		// (set) Token: 0x060001D3 RID: 467 RVA: 0x00002CE2 File Offset: 0x00000EE2
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("30")]
		public int DelaysBetweenPostingFrom
		{
			get
			{
				return (int)this["DelaysBetweenPostingFrom"];
			}
			set
			{
				this["DelaysBetweenPostingFrom"] = value;
			}
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x060001D4 RID: 468 RVA: 0x00002CF5 File Offset: 0x00000EF5
		// (set) Token: 0x060001D5 RID: 469 RVA: 0x00002D07 File Offset: 0x00000F07
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("35")]
		public int DelaysBetweenPostingTo
		{
			get
			{
				return (int)this["DelaysBetweenPostingTo"];
			}
			set
			{
				this["DelaysBetweenPostingTo"] = value;
			}
		}

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x060001D6 RID: 470 RVA: 0x00002D1A File Offset: 0x00000F1A
		// (set) Token: 0x060001D7 RID: 471 RVA: 0x00002D2C File Offset: 0x00000F2C
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool ConnectWIfi
		{
			get
			{
				return (bool)this["ConnectWIfi"];
			}
			set
			{
				this["ConnectWIfi"] = value;
			}
		}

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x060001D8 RID: 472 RVA: 0x00002D3F File Offset: 0x00000F3F
		// (set) Token: 0x060001D9 RID: 473 RVA: 0x00002D51 File Offset: 0x00000F51
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool ClearGallery
		{
			get
			{
				return (bool)this["ClearGallery"];
			}
			set
			{
				this["ClearGallery"] = value;
			}
		}

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x060001DA RID: 474 RVA: 0x00002D64 File Offset: 0x00000F64
		// (set) Token: 0x060001DB RID: 475 RVA: 0x00002D76 File Offset: 0x00000F76
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool BioAfterCreating
		{
			get
			{
				return (bool)this["BioAfterCreating"];
			}
			set
			{
				this["BioAfterCreating"] = value;
			}
		}

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x060001DC RID: 476 RVA: 0x00002D89 File Offset: 0x00000F89
		// (set) Token: 0x060001DD RID: 477 RVA: 0x00002D9B File Offset: 0x00000F9B
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("True")]
		public bool BioAfterPosting
		{
			get
			{
				return (bool)this["BioAfterPosting"];
			}
			set
			{
				this["BioAfterPosting"] = value;
			}
		}

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x060001DE RID: 478 RVA: 0x00002DAE File Offset: 0x00000FAE
		// (set) Token: 0x060001DF RID: 479 RVA: 0x00002DC0 File Offset: 0x00000FC0
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool MakingPlusPosting
		{
			get
			{
				return (bool)this["MakingPlusPosting"];
			}
			set
			{
				this["MakingPlusPosting"] = value;
			}
		}

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x060001E0 RID: 480 RVA: 0x00002DD3 File Offset: 0x00000FD3
		// (set) Token: 0x060001E1 RID: 481 RVA: 0x00002DE5 File Offset: 0x00000FE5
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("True")]
		public bool Making
		{
			get
			{
				return (bool)this["Making"];
			}
			set
			{
				this["Making"] = value;
			}
		}

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x060001E2 RID: 482 RVA: 0x00002DF8 File Offset: 0x00000FF8
		// (set) Token: 0x060001E3 RID: 483 RVA: 0x00002E0A File Offset: 0x0000100A
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool Posting
		{
			get
			{
				return (bool)this["Posting"];
			}
			set
			{
				this["Posting"] = value;
			}
		}

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x060001E4 RID: 484 RVA: 0x00002E1D File Offset: 0x0000101D
		// (set) Token: 0x060001E5 RID: 485 RVA: 0x00002E2F File Offset: 0x0000102F
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool VPNRandomLine
		{
			get
			{
				return (bool)this["VPNRandomLine"];
			}
			set
			{
				this["VPNRandomLine"] = value;
			}
		}

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x060001E6 RID: 486 RVA: 0x00002E42 File Offset: 0x00001042
		// (set) Token: 0x060001E7 RID: 487 RVA: 0x00002E54 File Offset: 0x00001054
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("True")]
		public bool VPNByLine
		{
			get
			{
				return (bool)this["VPNByLine"];
			}
			set
			{
				this["VPNByLine"] = value;
			}
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x060001E8 RID: 488 RVA: 0x00002E67 File Offset: 0x00001067
		// (set) Token: 0x060001E9 RID: 489 RVA: 0x00002E79 File Offset: 0x00001079
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("2")]
		public int AccountsOnVpn
		{
			get
			{
				return (int)this["AccountsOnVpn"];
			}
			set
			{
				this["AccountsOnVpn"] = value;
			}
		}

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x060001EA RID: 490 RVA: 0x00002E8C File Offset: 0x0000108C
		// (set) Token: 0x060001EB RID: 491 RVA: 0x00002E9E File Offset: 0x0000109E
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("True")]
		public bool SkipReLogin
		{
			get
			{
				return (bool)this["SkipReLogin"];
			}
			set
			{
				this["SkipReLogin"] = value;
			}
		}

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x060001EC RID: 492 RVA: 0x00002EB1 File Offset: 0x000010B1
		// (set) Token: 0x060001ED RID: 493 RVA: 0x00002EC3 File Offset: 0x000010C3
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("YOUR_ANYMESSAGE_API_KEY")]
		public string AnyMessageApiKey
		{
			get
			{
				return (string)this["AnyMessageApiKey"];
			}
			set
			{
				this["AnyMessageApiKey"] = value;
			}
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x060001EE RID: 494 RVA: 0x00002ED1 File Offset: 0x000010D1
		// (set) Token: 0x060001EF RID: 495 RVA: 0x00002EE3 File Offset: 0x000010E3
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("YOUR_PIA_LOGIN:YOUR_PIA_PASSWORD")]
		public string PiaData
		{
			get
			{
				return (string)this["PiaData"];
			}
			set
			{
				this["PiaData"] = value;
			}
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x060001F0 RID: 496 RVA: 0x00002EF1 File Offset: 0x000010F1
		// (set) Token: 0x060001F1 RID: 497 RVA: 0x00002F03 File Offset: 0x00001103
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool PostSameVideo
		{
			get
			{
				return (bool)this["PostSameVideo"];
			}
			set
			{
				this["PostSameVideo"] = value;
			}
		}

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x060001F2 RID: 498 RVA: 0x00002F16 File Offset: 0x00001116
		// (set) Token: 0x060001F3 RID: 499 RVA: 0x00002F28 File Offset: 0x00001128
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("True")]
		public bool ClearBioAfterLogin
		{
			get
			{
				return (bool)this["ClearBioAfterLogin"];
			}
			set
			{
				this["ClearBioAfterLogin"] = value;
			}
		}

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x060001F4 RID: 500 RVA: 0x00002F3B File Offset: 0x0000113B
		// (set) Token: 0x060001F5 RID: 501 RVA: 0x00002F4D File Offset: 0x0000114D
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("long_hotmail.com")]
		public string emailDomain
		{
			get
			{
				return (string)this["emailDomain"];
			}
			set
			{
				this["emailDomain"] = value;
			}
		}

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x060001F6 RID: 502 RVA: 0x00002F5B File Offset: 0x0000115B
		// (set) Token: 0x060001F7 RID: 503 RVA: 0x00002F6D File Offset: 0x0000116D
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool VideosFrom1Folder
		{
			get
			{
				return (bool)this["VideosFrom1Folder"];
			}
			set
			{
				this["VideosFrom1Folder"] = value;
			}
		}

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x060001F8 RID: 504 RVA: 0x00002F80 File Offset: 0x00001180
		// (set) Token: 0x060001F9 RID: 505 RVA: 0x00002F92 File Offset: 0x00001192
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("0")]
		public int Use1BioOn
		{
			get
			{
				return (int)this["Use1BioOn"];
			}
			set
			{
				this["Use1BioOn"] = value;
			}
		}

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x060001FA RID: 506 RVA: 0x00002FA5 File Offset: 0x000011A5
		// (set) Token: 0x060001FB RID: 507 RVA: 0x00002FB7 File Offset: 0x000011B7
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("True")]
		public bool AnymessageMinus1Hour
		{
			get
			{
				return (bool)this["AnymessageMinus1Hour"];
			}
			set
			{
				this["AnymessageMinus1Hour"] = value;
			}
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x060001FC RID: 508 RVA: 0x00002FCA File Offset: 0x000011CA
		// (set) Token: 0x060001FD RID: 509 RVA: 0x00002FDC File Offset: 0x000011DC
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool PostPictures
		{
			get
			{
				return (bool)this["PostPictures"];
			}
			set
			{
				this["PostPictures"] = value;
			}
		}

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x060001FE RID: 510 RVA: 0x00002FEF File Offset: 0x000011EF
		// (set) Token: 0x060001FF RID: 511 RVA: 0x00003001 File Offset: 0x00001201
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("3")]
		public int PicturesPostCount
		{
			get
			{
				return (int)this["PicturesPostCount"];
			}
			set
			{
				this["PicturesPostCount"] = value;
			}
		}

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x06000200 RID: 512 RVA: 0x00003014 File Offset: 0x00001214
		// (set) Token: 0x06000201 RID: 513 RVA: 0x00003026 File Offset: 0x00001226
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool DontSwitchCheckBoxes
		{
			get
			{
				return (bool)this["DontSwitchCheckBoxes"];
			}
			set
			{
				this["DontSwitchCheckBoxes"] = value;
			}
		}

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x06000202 RID: 514 RVA: 0x00003039 File Offset: 0x00001239
		// (set) Token: 0x06000203 RID: 515 RVA: 0x0000304B File Offset: 0x0000124B
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("PiaVpn")]
		public string VpnType
		{
			get
			{
				return (string)this["VpnType"];
			}
			set
			{
				this["VpnType"] = value;
			}
		}

		// Token: 0x040004FE RID: 1278
		private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());
	}
}
