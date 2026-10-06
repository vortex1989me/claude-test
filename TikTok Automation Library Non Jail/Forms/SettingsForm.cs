using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using TikTok_Automation_Library_Non_Jail.Properties;

namespace TikTok_Automation_Library_Non_Jail.Forms
{
	// Token: 0x020000AC RID: 172
	public partial class SettingsForm : Form
	{
		// Token: 0x06000237 RID: 567 RVA: 0x00060E40 File Offset: 0x0005F040
		public SettingsForm()
		{
			this.InitializeComponent();
			Task.Run(new Action(this.Method0));
			int num = 688;
			int num2 = 283;
			this.TabControl1.Size = new Size(num, num2);
			this.button1.Location = new Point(2, num2 + 5);
			this.button1.Size = new Size(num, 58);
			base.Size = new Size(num + 20, num2 + 97);
		}

		// Token: 0x06000238 RID: 568 RVA: 0x00060EC4 File Offset: 0x0005F0C4
		private void LoadSettings()
		{
			this.RemoveVideoAfterUse.Checked = Settings.Default.RemoveVideoAfterUse;
			this.NeedToPost.Value = Settings.Default.NeedToPost;
			this.AddMusic.Checked = Settings.Default.AddMusic;
			this.TurnOffAdditionalSound.Checked = Settings.Default.TurnOffAdditionalSound;
			this.TurnOffOriginalSound.Checked = Settings.Default.TurnOffOriginalSound;
			this.AddRandomMusic.Checked = Settings.Default.AddRandomMusic;
			this.UseVPN.Checked = Settings.Default.UseVPN;
			this.UploadPicture.Checked = Settings.Default.UploadPicture;
			this.UpdateBio.Checked = Settings.Default.UpdateBio;
			this.MentionInBio.Checked = Settings.Default.MentionInBio;
			this.WiFiName.Text = Settings.Default.WifiName;
			this.WiFiPassword.Text = Settings.Default.WifiPassword;
			this.AddTags.Checked = Settings.Default.AddTags;
			this.TagsCount.Value = Settings.Default.TagsCount;
			this.PlusMention.Checked = Settings.Default.PlusMention;
			this.AddComments.Checked = Settings.Default.AddComments;
			this.NameFromTxt.Checked = Settings.Default.NameFromTxt;
			this.AddMentionToComment.Checked = Settings.Default.AddMentionToComment;
			this.AddTextToComment.Checked = Settings.Default.AddTextToComment;
			this.AddImageToComment.Checked = Settings.Default.AddImageToComment;
			this.ResetNetworkAfter.Value = Settings.Default.ResetNetworkAfter;
			this.FirstPosting.Checked = Settings.Default.FirstPosting;
			this.ResetNetworkInRun.Checked = Settings.Default.ResetNetworkInRun;
			this.RecordScreen.Checked = Settings.Default.RecordScreen;
			this.UseShadowRocket.Checked = Settings.Default.UseShadowRocket;
			this.ProxyType.Text = Settings.Default.ProxyType;
			this.VpnType.Text = Settings.Default.VpnType;
			this.Use2FA.Checked = Settings.Default.Use2FA;
			this.DelaysBetweenPostingFrom.Value = Settings.Default.DelaysBetweenPostingFrom;
			this.DelaysBetweenPostingTo.Value = Settings.Default.DelaysBetweenPostingTo;
			this.ConnectWifi.Checked = Settings.Default.ConnectWIfi;
			this.BioAfterCreating.Checked = Settings.Default.BioAfterCreating;
			this.BioAfterPosting.Checked = Settings.Default.BioAfterPosting;
			this.Making.Checked = Settings.Default.Making;
			this.Posting.Checked = Settings.Default.Posting;
			this.MakingPlusPosting.Checked = Settings.Default.MakingPlusPosting;
			this.VPNByLine.Checked = Settings.Default.VPNByLine;
			this.VPNRandomLine.Checked = Settings.Default.VPNRandomLine;
			this.AccountsOnVpn.Value = Settings.Default.AccountsOnVpn;
			this.SkipReLogin.Checked = Settings.Default.SkipReLogin;
			this.AnyMessageApiKey.Text = Settings.Default.AnyMessageApiKey;
			this.piaData.Text = Settings.Default.PiaData;
			this.PostSameVideo.Checked = Settings.Default.PostSameVideo;
			this.ClearBioAfterLogin.Checked = Settings.Default.ClearBioAfterLogin;
			this.EmailDomain.Text = Settings.Default.emailDomain;
			this.VideosFrom1Folder.Checked = Settings.Default.VideosFrom1Folder;
			this.Use1BioOn.Value = Settings.Default.Use1BioOn;
			this.AnymessageMinus1Hour.Checked = Settings.Default.AnymessageMinus1Hour;
			this.PicturesPostCount.Value = Settings.Default.PicturesPostCount;
			this.PostPictures.Checked = Settings.Default.PostPictures;
			this.DontSwitchCheckBoxes.Checked = Settings.Default.DontSwitchCheckBoxes;
			if (this.AddComments.Checked)
			{
				this.AddTextToComment.Enabled = true;
				this.AddMentionToComment.Enabled = true;
				this.AddImageToComment.Enabled = true;
			}
			else
			{
				this.AddTextToComment.Enabled = false;
				this.AddMentionToComment.Enabled = false;
				this.AddImageToComment.Enabled = false;
			}
			if (this.AddMusic.Checked)
			{
				this.AddRandomMusic.Checked = false;
			}
			if (this.AddMusic.Checked || this.AddRandomMusic.Checked)
			{
				this.TurnOffAdditionalSound.Enabled = true;
				this.TurnOffOriginalSound.Enabled = true;
			}
			else
			{
				this.TurnOffAdditionalSound.Enabled = false;
				this.TurnOffOriginalSound.Enabled = false;
			}
			if (this.AddTags.Checked)
			{
				this.TagsCount.Enabled = true;
			}
			else
			{
				this.TagsCount.Enabled = false;
			}
			if (this.UpdateBio.Checked)
			{
				this.MentionInBio.Enabled = true;
				this.BioAfterCreating.Enabled = true;
				this.BioAfterPosting.Enabled = true;
				return;
			}
			this.MentionInBio.Enabled = false;
			this.BioAfterCreating.Enabled = false;
			this.BioAfterPosting.Enabled = false;
		}

		// Token: 0x06000239 RID: 569 RVA: 0x00061464 File Offset: 0x0005F664
		private void SaveSettings(string selectedDomain)
		{
			Settings.Default.RemoveVideoAfterUse = this.RemoveVideoAfterUse.Checked;
			Settings.Default.NeedToPost = (int)this.NeedToPost.Value;
			Settings.Default.AddMusic = this.AddMusic.Checked;
			Settings.Default.TurnOffAdditionalSound = this.TurnOffAdditionalSound.Checked;
			Settings.Default.TurnOffOriginalSound = this.TurnOffOriginalSound.Checked;
			Settings.Default.AddRandomMusic = this.AddRandomMusic.Checked;
			Settings.Default.UseVPN = this.UseVPN.Checked;
			Settings.Default.UploadPicture = this.UploadPicture.Checked;
			Settings.Default.UpdateBio = this.UpdateBio.Checked;
			Settings.Default.MentionInBio = this.MentionInBio.Checked;
			Settings.Default.WifiName = this.WiFiName.Text;
			Settings.Default.WifiPassword = this.WiFiPassword.Text;
			Settings.Default.AddTags = this.AddTags.Checked;
			Settings.Default.TagsCount = (int)this.TagsCount.Value;
			Settings.Default.PlusMention = this.PlusMention.Checked;
			Settings.Default.AddComments = this.AddComments.Checked;
			Settings.Default.NameFromTxt = this.NameFromTxt.Checked;
			Settings.Default.AddMentionToComment = this.AddMentionToComment.Checked;
			Settings.Default.AddTextToComment = this.AddTextToComment.Checked;
			Settings.Default.AddImageToComment = this.AddImageToComment.Checked;
			Settings.Default.ResetNetworkAfter = (int)this.ResetNetworkAfter.Value;
			Settings.Default.FirstPosting = this.FirstPosting.Checked;
			Settings.Default.ResetNetworkInRun = this.ResetNetworkInRun.Checked;
			Settings.Default.RecordScreen = this.RecordScreen.Checked;
			Settings.Default.UseShadowRocket = this.UseShadowRocket.Checked;
			Settings.Default.ProxyType = this.ProxyType.Text;
			Settings.Default.VpnType = this.VpnType.Text;
			Settings.Default.Use2FA = this.Use2FA.Checked;
			Settings.Default.DelaysBetweenPostingFrom = (int)this.DelaysBetweenPostingFrom.Value;
			Settings.Default.DelaysBetweenPostingTo = (int)this.DelaysBetweenPostingTo.Value;
			Settings.Default.ConnectWIfi = this.ConnectWifi.Checked;
			Settings.Default.BioAfterCreating = this.BioAfterCreating.Checked;
			Settings.Default.BioAfterPosting = this.BioAfterPosting.Checked;
			Settings.Default.Making = this.Making.Checked;
			Settings.Default.Posting = this.Posting.Checked;
			Settings.Default.MakingPlusPosting = this.MakingPlusPosting.Checked;
			Settings.Default.VPNRandomLine = this.VPNRandomLine.Checked;
			Settings.Default.VPNByLine = this.VPNByLine.Checked;
			Settings.Default.AccountsOnVpn = (int)this.AccountsOnVpn.Value;
			Settings.Default.SkipReLogin = this.SkipReLogin.Checked;
			Settings.Default.AnyMessageApiKey = this.AnyMessageApiKey.Text;
			Settings.Default.PiaData = this.piaData.Text;
			Settings.Default.PostSameVideo = this.PostSameVideo.Checked;
			Settings.Default.ClearBioAfterLogin = this.ClearBioAfterLogin.Checked;
			Settings.Default.emailDomain = selectedDomain;
			Settings.Default.VideosFrom1Folder = this.VideosFrom1Folder.Checked;
			Settings.Default.Use1BioOn = (int)this.Use1BioOn.Value;
			Settings.Default.AnymessageMinus1Hour = this.AnymessageMinus1Hour.Checked;
			Settings.Default.PostPictures = this.PostPictures.Checked;
			Settings.Default.PicturesPostCount = (int)this.PicturesPostCount.Value;
			Settings.Default.DontSwitchCheckBoxes = this.DontSwitchCheckBoxes.Checked;
			Settings.Default.Save();
		}

		// Token: 0x0600023A RID: 570 RVA: 0x000618C8 File Offset: 0x0005FAC8
		private void button1_Click(object sender, EventArgs e)
		{
			SettingsForm.Class20 @class = new SettingsForm.Class20();
			@class.Field0 = this;
			@class.selectedDomain = this.EmailDomain.Text;
			Task task = Task.Run(new Action(@class.Method0));
			Task.WhenAll(new Task[] { task }).ConfigureAwait(false).GetAwaiter()
				.GetResult();
			base.Hide();
		}

		// Token: 0x0600023B RID: 571 RVA: 0x00061930 File Offset: 0x0005FB30
		private void TabControl_OnSelecting(object sender, TabControlCancelEventArgs e)
		{
			string name = this.TabControl1.SelectedTab.Name;
			if (name == "MainSettingsTab")
			{
				int num = 688;
				int num2 = 283;
				this.TabControl1.Size = new Size(num, num2);
				this.button1.Location = new Point(2, num2 + 5);
				this.button1.Size = new Size(num, 58);
				base.Size = new Size(num + 20, num2 + 97);
				return;
			}
			if (name == "FeaturesTab")
			{
				int num3 = 661;
				int num4 = 269;
				this.TabControl1.Size = new Size(num3, num4);
				this.button1.Location = new Point(2, num4 + 5);
				this.button1.Size = new Size(num3, 58);
				base.Size = new Size(num3 + 20, num4 + 97);
				return;
			}
			if (name == "WiFiTab")
			{
				int num5 = 410;
				int num6 = 160;
				this.TabControl1.Size = new Size(num5, num6);
				this.button1.Location = new Point(2, num6 + 5);
				this.button1.Size = new Size(num5, 58);
				base.Size = new Size(num5 + 20, num6 + 97);
			}
		}

		// Token: 0x0600023C RID: 572 RVA: 0x0000324E File Offset: 0x0000144E
		private void AddTags_CheckedChanged_1(object sender, EventArgs e)
		{
			if (this.AddTags.Checked)
			{
				this.TagsCount.Enabled = true;
				return;
			}
			this.TagsCount.Enabled = false;
		}

		// Token: 0x0600023D RID: 573 RVA: 0x00061A8C File Offset: 0x0005FC8C
		private void UpdateBio_CheckedChanged_1(object sender, EventArgs e)
		{
			if (this.UpdateBio.Checked)
			{
				this.MentionInBio.Enabled = true;
				this.BioAfterCreating.Enabled = true;
				this.BioAfterPosting.Enabled = true;
				return;
			}
			this.MentionInBio.Enabled = false;
			this.BioAfterCreating.Enabled = false;
			this.BioAfterPosting.Enabled = false;
		}

		// Token: 0x0600023E RID: 574 RVA: 0x00061AF0 File Offset: 0x0005FCF0
		private void AddMusic_CheckedChanged_1(object sender, EventArgs e)
		{
			if (this.AddMusic.Checked)
			{
				this.AddRandomMusic.Checked = false;
			}
			if (this.AddMusic.Checked || this.AddRandomMusic.Checked)
			{
				this.TurnOffAdditionalSound.Enabled = true;
				this.TurnOffOriginalSound.Enabled = true;
				return;
			}
			this.TurnOffAdditionalSound.Enabled = false;
			this.TurnOffOriginalSound.Enabled = false;
		}

		// Token: 0x0600023F RID: 575 RVA: 0x00061B64 File Offset: 0x0005FD64
		private void AddRandomMusic_CheckedChanged(object sender, EventArgs e)
		{
			if (this.AddRandomMusic.Checked)
			{
				this.AddMusic.Checked = false;
			}
			if (this.AddMusic.Checked || this.AddRandomMusic.Checked)
			{
				this.TurnOffAdditionalSound.Enabled = true;
				this.TurnOffOriginalSound.Enabled = true;
				return;
			}
			this.TurnOffAdditionalSound.Enabled = false;
			this.TurnOffOriginalSound.Enabled = false;
		}

		// Token: 0x06000240 RID: 576 RVA: 0x00061BD8 File Offset: 0x0005FDD8
		private void AddComments_CheckedChanged_1(object sender, EventArgs e)
		{
			if (this.AddComments.Checked)
			{
				this.AddTextToComment.Enabled = true;
				this.AddMentionToComment.Enabled = true;
				this.AddImageToComment.Enabled = true;
				return;
			}
			this.AddTextToComment.Enabled = false;
			this.AddMentionToComment.Enabled = false;
			this.AddImageToComment.Enabled = false;
		}

		// Token: 0x06000241 RID: 577 RVA: 0x00003276 File Offset: 0x00001476
		private void UseVPN_CheckedChanged(object sender, EventArgs e)
		{
			if (this.UseVPN.Checked && this.UseShadowRocket.Checked)
			{
				this.UseShadowRocket.Checked = false;
			}
		}

		// Token: 0x06000242 RID: 578 RVA: 0x0000329E File Offset: 0x0000149E
		private void UseShadowRocket_CheckedChanged(object sender, EventArgs e)
		{
			if (this.UseShadowRocket.Checked && this.UseVPN.Checked)
			{
				this.UseVPN.Checked = false;
			}
		}

		// Token: 0x06000245 RID: 581 RVA: 0x000032E5 File Offset: 0x000014E5
		[CompilerGenerated]
		private void Method0()
		{
			this.LoadSettings();
		}

		// Token: 0x020000AD RID: 173
		[CompilerGenerated]
		private sealed class Class20
		{
			// Token: 0x06000247 RID: 583 RVA: 0x000032ED File Offset: 0x000014ED
			internal void Method0()
			{
				this.Field0.SaveSettings(this.selectedDomain);
			}

			// Token: 0x040005B4 RID: 1460
			public SettingsForm Field0;

			// Token: 0x040005B5 RID: 1461
			public string selectedDomain;
		}
	}
}
