namespace TikTok_Automation_Library_Non_Jail.Forms
{
	// Token: 0x020000AC RID: 172
	public partial class SettingsForm : global::System.Windows.Forms.Form
	{
		// Token: 0x06000243 RID: 579 RVA: 0x000032C6 File Offset: 0x000014C6
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000244 RID: 580 RVA: 0x00061C3C File Offset: 0x0005FE3C
		private void InitializeComponent()
		{
			this.button1 = new global::System.Windows.Forms.Button();
			this.TabControl1 = new global::System.Windows.Forms.TabControl();
			this.MainSettingsTab = new global::System.Windows.Forms.TabPage();
			this.AnymessageMinus1Hour = new global::System.Windows.Forms.CheckBox();
			this.label13 = new global::System.Windows.Forms.Label();
			this.EmailDomain = new global::System.Windows.Forms.ComboBox();
			this.label11 = new global::System.Windows.Forms.Label();
			this.AnyMessageApiKey = new global::System.Windows.Forms.TextBox();
			this.SkipReLogin = new global::System.Windows.Forms.CheckBox();
			this.MakingPlusPosting = new global::System.Windows.Forms.RadioButton();
			this.Posting = new global::System.Windows.Forms.RadioButton();
			this.Making = new global::System.Windows.Forms.RadioButton();
			this.ClearGallery = new global::System.Windows.Forms.CheckBox();
			this.Use2FA = new global::System.Windows.Forms.CheckBox();
			this.RecordScreen = new global::System.Windows.Forms.CheckBox();
			this.ResetNetworkInRun = new global::System.Windows.Forms.CheckBox();
			this.FirstPosting = new global::System.Windows.Forms.CheckBox();
			this.label9 = new global::System.Windows.Forms.Label();
			this.ResetNetworkAfter = new global::System.Windows.Forms.NumericUpDown();
			this.label8 = new global::System.Windows.Forms.Label();
			this.NameFromTxt = new global::System.Windows.Forms.CheckBox();
			this.RemoveVideoAfterUse = new global::System.Windows.Forms.CheckBox();
			this.UploadPicture = new global::System.Windows.Forms.CheckBox();
			this.FeaturesTab = new global::System.Windows.Forms.TabPage();
			this.groupBox5 = new global::System.Windows.Forms.GroupBox();
			this.DontSwitchCheckBoxes = new global::System.Windows.Forms.CheckBox();
			this.PicturesPostCount = new global::System.Windows.Forms.NumericUpDown();
			this.label16 = new global::System.Windows.Forms.Label();
			this.PostPictures = new global::System.Windows.Forms.CheckBox();
			this.VideosFrom1Folder = new global::System.Windows.Forms.CheckBox();
			this.PostSameVideo = new global::System.Windows.Forms.CheckBox();
			this.label7 = new global::System.Windows.Forms.Label();
			this.DelaysBetweenPostingTo = new global::System.Windows.Forms.NumericUpDown();
			this.label6 = new global::System.Windows.Forms.Label();
			this.DelaysBetweenPostingFrom = new global::System.Windows.Forms.NumericUpDown();
			this.label5 = new global::System.Windows.Forms.Label();
			this.NeedToPost = new global::System.Windows.Forms.NumericUpDown();
			this.label4 = new global::System.Windows.Forms.Label();
			this.groupBox4 = new global::System.Windows.Forms.GroupBox();
			this.label15 = new global::System.Windows.Forms.Label();
			this.label14 = new global::System.Windows.Forms.Label();
			this.Use1BioOn = new global::System.Windows.Forms.NumericUpDown();
			this.ClearBioAfterLogin = new global::System.Windows.Forms.CheckBox();
			this.BioAfterPosting = new global::System.Windows.Forms.RadioButton();
			this.BioAfterCreating = new global::System.Windows.Forms.RadioButton();
			this.UpdateBio = new global::System.Windows.Forms.CheckBox();
			this.MentionInBio = new global::System.Windows.Forms.CheckBox();
			this.groupBox3 = new global::System.Windows.Forms.GroupBox();
			this.AddTags = new global::System.Windows.Forms.CheckBox();
			this.label3 = new global::System.Windows.Forms.Label();
			this.TagsCount = new global::System.Windows.Forms.NumericUpDown();
			this.PlusMention = new global::System.Windows.Forms.CheckBox();
			this.groupBox2 = new global::System.Windows.Forms.GroupBox();
			this.AddRandomMusic = new global::System.Windows.Forms.CheckBox();
			this.AddMusic = new global::System.Windows.Forms.CheckBox();
			this.TurnOffOriginalSound = new global::System.Windows.Forms.CheckBox();
			this.TurnOffAdditionalSound = new global::System.Windows.Forms.CheckBox();
			this.groupBox1 = new global::System.Windows.Forms.GroupBox();
			this.AddComments = new global::System.Windows.Forms.CheckBox();
			this.AddTextToComment = new global::System.Windows.Forms.CheckBox();
			this.AddImageToComment = new global::System.Windows.Forms.CheckBox();
			this.AddMentionToComment = new global::System.Windows.Forms.CheckBox();
			this.WiFiTab = new global::System.Windows.Forms.TabPage();
			this.VpnType = new global::System.Windows.Forms.DomainUpDown();
			this.label12 = new global::System.Windows.Forms.Label();
			this.piaData = new global::System.Windows.Forms.TextBox();
			this.AccountsOnVpn = new global::System.Windows.Forms.NumericUpDown();
			this.label10 = new global::System.Windows.Forms.Label();
			this.VPNByLine = new global::System.Windows.Forms.RadioButton();
			this.VPNRandomLine = new global::System.Windows.Forms.RadioButton();
			this.ConnectWifi = new global::System.Windows.Forms.CheckBox();
			this.ProxyType = new global::System.Windows.Forms.DomainUpDown();
			this.UseShadowRocket = new global::System.Windows.Forms.CheckBox();
			this.UseVPN = new global::System.Windows.Forms.CheckBox();
			this.label2 = new global::System.Windows.Forms.Label();
			this.label1 = new global::System.Windows.Forms.Label();
			this.WiFiPassword = new global::System.Windows.Forms.TextBox();
			this.WiFiName = new global::System.Windows.Forms.TextBox();
			this.TabControl1.SuspendLayout();
			this.MainSettingsTab.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.ResetNetworkAfter).BeginInit();
			this.FeaturesTab.SuspendLayout();
			this.groupBox5.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.PicturesPostCount).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.DelaysBetweenPostingTo).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.DelaysBetweenPostingFrom).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.NeedToPost).BeginInit();
			this.groupBox4.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.Use1BioOn).BeginInit();
			this.groupBox3.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.TagsCount).BeginInit();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.WiFiTab.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.AccountsOnVpn).BeginInit();
			base.SuspendLayout();
			this.button1.Location = new global::System.Drawing.Point(4, 313);
			this.button1.Name = "button1";
			this.button1.Size = new global::System.Drawing.Size(688, 81);
			this.button1.TabIndex = 0;
			this.button1.Text = "Save";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new global::System.EventHandler(this.button1_Click);
			this.TabControl1.Controls.Add(this.MainSettingsTab);
			this.TabControl1.Controls.Add(this.FeaturesTab);
			this.TabControl1.Controls.Add(this.WiFiTab);
			this.TabControl1.Location = new global::System.Drawing.Point(0, 2);
			this.TabControl1.Name = "TabControl1";
			this.TabControl1.SelectedIndex = 0;
			this.TabControl1.Size = new global::System.Drawing.Size(660, 305);
			this.TabControl1.TabIndex = 1;
			this.TabControl1.Selecting += new global::System.Windows.Forms.TabControlCancelEventHandler(this.TabControl_OnSelecting);
			this.MainSettingsTab.Controls.Add(this.AnymessageMinus1Hour);
			this.MainSettingsTab.Controls.Add(this.label13);
			this.MainSettingsTab.Controls.Add(this.EmailDomain);
			this.MainSettingsTab.Controls.Add(this.label11);
			this.MainSettingsTab.Controls.Add(this.AnyMessageApiKey);
			this.MainSettingsTab.Controls.Add(this.SkipReLogin);
			this.MainSettingsTab.Controls.Add(this.MakingPlusPosting);
			this.MainSettingsTab.Controls.Add(this.Posting);
			this.MainSettingsTab.Controls.Add(this.Making);
			this.MainSettingsTab.Controls.Add(this.ClearGallery);
			this.MainSettingsTab.Controls.Add(this.Use2FA);
			this.MainSettingsTab.Controls.Add(this.RecordScreen);
			this.MainSettingsTab.Controls.Add(this.ResetNetworkInRun);
			this.MainSettingsTab.Controls.Add(this.FirstPosting);
			this.MainSettingsTab.Controls.Add(this.label9);
			this.MainSettingsTab.Controls.Add(this.ResetNetworkAfter);
			this.MainSettingsTab.Controls.Add(this.label8);
			this.MainSettingsTab.Controls.Add(this.NameFromTxt);
			this.MainSettingsTab.Controls.Add(this.RemoveVideoAfterUse);
			this.MainSettingsTab.Controls.Add(this.UploadPicture);
			this.MainSettingsTab.Location = new global::System.Drawing.Point(4, 22);
			this.MainSettingsTab.Name = "MainSettingsTab";
			this.MainSettingsTab.Padding = new global::System.Windows.Forms.Padding(3);
			this.MainSettingsTab.Size = new global::System.Drawing.Size(652, 279);
			this.MainSettingsTab.TabIndex = 0;
			this.MainSettingsTab.Text = "Main";
			this.MainSettingsTab.UseVisualStyleBackColor = true;
			this.AnymessageMinus1Hour.AutoSize = true;
			this.AnymessageMinus1Hour.Location = new global::System.Drawing.Point(255, 151);
			this.AnymessageMinus1Hour.Name = "AnymessageMinus1Hour";
			this.AnymessageMinus1Hour.Size = new global::System.Drawing.Size(120, 17);
			this.AnymessageMinus1Hour.TabIndex = 29;
			this.AnymessageMinus1Hour.Text = "Check Last 3 Hours";
			this.AnymessageMinus1Hour.UseVisualStyleBackColor = true;
			this.label13.AutoSize = true;
			this.label13.Location = new global::System.Drawing.Point(9, 152);
			this.label13.Name = "label13";
			this.label13.Size = new global::System.Drawing.Size(74, 13);
			this.label13.TabIndex = 28;
			this.label13.Text = "Email Domain:";
			this.EmailDomain.FormattingEnabled = true;
			this.EmailDomain.Items.AddRange(new object[]
			{
				"mail.com", "email.com", "outlook.com", "hotmail.com", "long_outlook.com", "long_hotmail.com", "gmx.com", "mail.com", "yandex.ru", "rambler.ru",
				"mail.ru", "gmail.com", "email.com", "outlook.com", "hotmail.com", "gmx.de", "gmx.us", "caramail.com", "caramail.fr", "gmx.ca",
				"gmx.cn", "gmx.co.in", "gmx.co.uk", "gmx.com.br", "gmx.com.my", "gmx.com.tr", "gmx.es", "gmx.fr", "gmx.hk", "gmx.ie",
				"gmx.it", "gmx.li", "gmx.ph", "gmx.pt", "gmx.ru", "gmx.se", "gmx.sg", "gmx.tm", "gmx.tw", "ya.ru",
				"yandex.com", "yandex.md", "yandex.pl", "yandex.tj", "yandex.az", "yandex.by", "yandex.ee", "yandex.lt", "yandex.lv", "yandex.uz",
				"yandex.tm", "yandex.kz", "yandex.fi", "yandex.com.tr", "yandex.com.ge", "yandex.com.am", "yandex.co.il", "yandex.kg", "yandex.fr"
			});
			this.EmailDomain.Location = new global::System.Drawing.Point(89, 149);
			this.EmailDomain.Name = "EmailDomain";
			this.EmailDomain.Size = new global::System.Drawing.Size(157, 21);
			this.EmailDomain.TabIndex = 27;
			this.label11.AutoSize = true;
			this.label11.Location = new global::System.Drawing.Point(9, 127);
			this.label11.Name = "label11";
			this.label11.Size = new global::System.Drawing.Size(110, 13);
			this.label11.TabIndex = 26;
			this.label11.Text = "Any Message ApiKey:";
			this.AnyMessageApiKey.Location = new global::System.Drawing.Point(125, 124);
			this.AnyMessageApiKey.Name = "AnyMessageApiKey";
			this.AnyMessageApiKey.Size = new global::System.Drawing.Size(266, 20);
			this.AnyMessageApiKey.TabIndex = 25;
			this.SkipReLogin.AutoSize = true;
			this.SkipReLogin.Location = new global::System.Drawing.Point(160, 101);
			this.SkipReLogin.Name = "SkipReLogin";
			this.SkipReLogin.Size = new global::System.Drawing.Size(90, 17);
			this.SkipReLogin.TabIndex = 24;
			this.SkipReLogin.Text = "Skip ReLogin";
			this.SkipReLogin.UseVisualStyleBackColor = true;
			this.MakingPlusPosting.AutoSize = true;
			this.MakingPlusPosting.Location = new global::System.Drawing.Point(284, 78);
			this.MakingPlusPosting.Name = "MakingPlusPosting";
			this.MakingPlusPosting.Size = new global::System.Drawing.Size(107, 17);
			this.MakingPlusPosting.TabIndex = 23;
			this.MakingPlusPosting.TabStop = true;
			this.MakingPlusPosting.Text = "Making + Posting";
			this.MakingPlusPosting.UseVisualStyleBackColor = true;
			this.Posting.AutoSize = true;
			this.Posting.Location = new global::System.Drawing.Point(284, 55);
			this.Posting.Name = "Posting";
			this.Posting.Size = new global::System.Drawing.Size(60, 17);
			this.Posting.TabIndex = 22;
			this.Posting.TabStop = true;
			this.Posting.Text = "Posting";
			this.Posting.UseVisualStyleBackColor = true;
			this.Making.AutoSize = true;
			this.Making.Location = new global::System.Drawing.Point(283, 32);
			this.Making.Name = "Making";
			this.Making.Size = new global::System.Drawing.Size(60, 17);
			this.Making.TabIndex = 21;
			this.Making.TabStop = true;
			this.Making.Text = "Making";
			this.Making.UseVisualStyleBackColor = true;
			this.ClearGallery.AutoSize = true;
			this.ClearGallery.Location = new global::System.Drawing.Point(11, 101);
			this.ClearGallery.Name = "ClearGallery";
			this.ClearGallery.Size = new global::System.Drawing.Size(85, 17);
			this.ClearGallery.TabIndex = 20;
			this.ClearGallery.Text = "Clear Gallery";
			this.ClearGallery.UseVisualStyleBackColor = true;
			this.Use2FA.AutoSize = true;
			this.Use2FA.Location = new global::System.Drawing.Point(160, 78);
			this.Use2FA.Name = "Use2FA";
			this.Use2FA.Size = new global::System.Drawing.Size(67, 17);
			this.Use2FA.TabIndex = 19;
			this.Use2FA.Text = "Use 2FA";
			this.Use2FA.UseVisualStyleBackColor = true;
			this.RecordScreen.AutoSize = true;
			this.RecordScreen.Location = new global::System.Drawing.Point(283, 6);
			this.RecordScreen.Name = "RecordScreen";
			this.RecordScreen.Size = new global::System.Drawing.Size(98, 17);
			this.RecordScreen.TabIndex = 18;
			this.RecordScreen.Text = "Record Screen";
			this.RecordScreen.UseVisualStyleBackColor = true;
			this.ResetNetworkInRun.AutoSize = true;
			this.ResetNetworkInRun.Location = new global::System.Drawing.Point(11, 6);
			this.ResetNetworkInRun.Name = "ResetNetworkInRun";
			this.ResetNetworkInRun.Size = new global::System.Drawing.Size(154, 17);
			this.ResetNetworkInRun.TabIndex = 17;
			this.ResetNetworkInRun.Text = "Reset Network In First Run";
			this.ResetNetworkInRun.UseVisualStyleBackColor = true;
			this.FirstPosting.AutoSize = true;
			this.FirstPosting.Location = new global::System.Drawing.Point(171, 6);
			this.FirstPosting.Name = "FirstPosting";
			this.FirstPosting.Size = new global::System.Drawing.Size(83, 17);
			this.FirstPosting.TabIndex = 16;
			this.FirstPosting.Text = "First Posting";
			this.FirstPosting.UseVisualStyleBackColor = true;
			this.label9.AutoSize = true;
			this.label9.Location = new global::System.Drawing.Point(192, 31);
			this.label9.Name = "label9";
			this.label9.Size = new global::System.Drawing.Size(51, 13);
			this.label9.TabIndex = 15;
			this.label9.Text = "accounts";
			this.ResetNetworkAfter.Location = new global::System.Drawing.Point(120, 29);
			this.ResetNetworkAfter.Name = "ResetNetworkAfter";
			this.ResetNetworkAfter.Size = new global::System.Drawing.Size(66, 20);
			this.ResetNetworkAfter.TabIndex = 13;
			this.label8.AutoSize = true;
			this.label8.Location = new global::System.Drawing.Point(8, 31);
			this.label8.Name = "label8";
			this.label8.Size = new global::System.Drawing.Size(106, 13);
			this.label8.TabIndex = 14;
			this.label8.Text = "Reset Network After:";
			this.NameFromTxt.AutoSize = true;
			this.NameFromTxt.Location = new global::System.Drawing.Point(113, 55);
			this.NameFromTxt.Name = "NameFromTxt";
			this.NameFromTxt.Size = new global::System.Drawing.Size(98, 17);
			this.NameFromTxt.TabIndex = 12;
			this.NameFromTxt.Text = "Name From Txt";
			this.NameFromTxt.UseVisualStyleBackColor = true;
			this.RemoveVideoAfterUse.AutoSize = true;
			this.RemoveVideoAfterUse.Location = new global::System.Drawing.Point(11, 78);
			this.RemoveVideoAfterUse.Name = "RemoveVideoAfterUse";
			this.RemoveVideoAfterUse.Size = new global::System.Drawing.Size(143, 17);
			this.RemoveVideoAfterUse.TabIndex = 7;
			this.RemoveVideoAfterUse.Text = "Remove Video After Use";
			this.RemoveVideoAfterUse.UseVisualStyleBackColor = true;
			this.UploadPicture.AutoSize = true;
			this.UploadPicture.Location = new global::System.Drawing.Point(11, 55);
			this.UploadPicture.Name = "UploadPicture";
			this.UploadPicture.Size = new global::System.Drawing.Size(96, 17);
			this.UploadPicture.TabIndex = 0;
			this.UploadPicture.Text = "Upload Picture";
			this.UploadPicture.UseVisualStyleBackColor = true;
			this.FeaturesTab.Controls.Add(this.groupBox5);
			this.FeaturesTab.Controls.Add(this.groupBox4);
			this.FeaturesTab.Controls.Add(this.groupBox3);
			this.FeaturesTab.Controls.Add(this.groupBox2);
			this.FeaturesTab.Controls.Add(this.groupBox1);
			this.FeaturesTab.Location = new global::System.Drawing.Point(4, 22);
			this.FeaturesTab.Name = "FeaturesTab";
			this.FeaturesTab.Padding = new global::System.Windows.Forms.Padding(3);
			this.FeaturesTab.Size = new global::System.Drawing.Size(652, 279);
			this.FeaturesTab.TabIndex = 2;
			this.FeaturesTab.Text = "Features";
			this.FeaturesTab.UseVisualStyleBackColor = true;
			this.groupBox5.Controls.Add(this.DontSwitchCheckBoxes);
			this.groupBox5.Controls.Add(this.PicturesPostCount);
			this.groupBox5.Controls.Add(this.label16);
			this.groupBox5.Controls.Add(this.PostPictures);
			this.groupBox5.Controls.Add(this.VideosFrom1Folder);
			this.groupBox5.Controls.Add(this.PostSameVideo);
			this.groupBox5.Controls.Add(this.label7);
			this.groupBox5.Controls.Add(this.DelaysBetweenPostingTo);
			this.groupBox5.Controls.Add(this.label6);
			this.groupBox5.Controls.Add(this.DelaysBetweenPostingFrom);
			this.groupBox5.Controls.Add(this.label5);
			this.groupBox5.Controls.Add(this.NeedToPost);
			this.groupBox5.Controls.Add(this.label4);
			this.groupBox5.Location = new global::System.Drawing.Point(3, 6);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new global::System.Drawing.Size(319, 114);
			this.groupBox5.TabIndex = 27;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Posting";
			this.DontSwitchCheckBoxes.AutoSize = true;
			this.DontSwitchCheckBoxes.Location = new global::System.Drawing.Point(63, 93);
			this.DontSwitchCheckBoxes.Name = "DontSwitchCheckBoxes";
			this.DontSwitchCheckBoxes.Size = new global::System.Drawing.Size(149, 17);
			this.DontSwitchCheckBoxes.TabIndex = 21;
			this.DontSwitchCheckBoxes.Text = "Don't Switch CheckBoxes";
			this.DontSwitchCheckBoxes.UseVisualStyleBackColor = true;
			this.PicturesPostCount.Location = new global::System.Drawing.Point(247, 75);
			global::System.Windows.Forms.NumericUpDown picturesPostCount = this.PicturesPostCount;
			int[] array = new int[4];
			array[0] = 3;
			picturesPostCount.Maximum = new decimal(array);
			global::System.Windows.Forms.NumericUpDown picturesPostCount2 = this.PicturesPostCount;
			int[] array2 = new int[4];
			array2[0] = 1;
			picturesPostCount2.Minimum = new decimal(array2);
			this.PicturesPostCount.Name = "PicturesPostCount";
			this.PicturesPostCount.Size = new global::System.Drawing.Size(52, 20);
			this.PicturesPostCount.TabIndex = 20;
			global::System.Windows.Forms.NumericUpDown picturesPostCount3 = this.PicturesPostCount;
			int[] array3 = new int[4];
			array3[0] = 1;
			picturesPostCount3.Value = new decimal(array3);
			this.label16.AutoSize = true;
			this.label16.Location = new global::System.Drawing.Point(162, 77);
			this.label16.Name = "label16";
			this.label16.Size = new global::System.Drawing.Size(79, 13);
			this.label16.TabIndex = 19;
			this.label16.Text = "Pictures Count:";
			this.PostPictures.AutoSize = true;
			this.PostPictures.Location = new global::System.Drawing.Point(221, 94);
			this.PostPictures.Name = "PostPictures";
			this.PostPictures.Size = new global::System.Drawing.Size(88, 17);
			this.PostPictures.TabIndex = 18;
			this.PostPictures.Text = "Post Pictures";
			this.PostPictures.UseVisualStyleBackColor = true;
			this.VideosFrom1Folder.AutoSize = true;
			this.VideosFrom1Folder.Location = new global::System.Drawing.Point(160, 26);
			this.VideosFrom1Folder.Name = "VideosFrom1Folder";
			this.VideosFrom1Folder.Size = new global::System.Drawing.Size(125, 17);
			this.VideosFrom1Folder.TabIndex = 17;
			this.VideosFrom1Folder.Text = "Videos From 1 Folder";
			this.VideosFrom1Folder.UseVisualStyleBackColor = true;
			this.PostSameVideo.AutoSize = true;
			this.PostSameVideo.Location = new global::System.Drawing.Point(160, 8);
			this.PostSameVideo.Name = "PostSameVideo";
			this.PostSameVideo.Size = new global::System.Drawing.Size(123, 17);
			this.PostSameVideo.TabIndex = 16;
			this.PostSameVideo.Text = "1 Video = 1 Account";
			this.PostSameVideo.UseVisualStyleBackColor = true;
			this.label7.AutoSize = true;
			this.label7.Location = new global::System.Drawing.Point(302, 51);
			this.label7.Name = "label7";
			this.label7.Size = new global::System.Drawing.Size(12, 13);
			this.label7.TabIndex = 16;
			this.label7.Text = "s";
			this.DelaysBetweenPostingTo.Location = new global::System.Drawing.Point(247, 49);
			global::System.Windows.Forms.NumericUpDown delaysBetweenPostingTo = this.DelaysBetweenPostingTo;
			int[] array4 = new int[4];
			array4[0] = 99999999;
			delaysBetweenPostingTo.Maximum = new decimal(array4);
			this.DelaysBetweenPostingTo.Name = "DelaysBetweenPostingTo";
			this.DelaysBetweenPostingTo.Size = new global::System.Drawing.Size(49, 20);
			this.DelaysBetweenPostingTo.TabIndex = 15;
			this.label6.AutoSize = true;
			this.label6.Location = new global::System.Drawing.Point(218, 51);
			this.label6.Name = "label6";
			this.label6.Size = new global::System.Drawing.Size(23, 13);
			this.label6.TabIndex = 14;
			this.label6.Text = "To:";
			this.DelaysBetweenPostingFrom.Location = new global::System.Drawing.Point(160, 49);
			global::System.Windows.Forms.NumericUpDown delaysBetweenPostingFrom = this.DelaysBetweenPostingFrom;
			int[] array5 = new int[4];
			array5[0] = 99999999;
			delaysBetweenPostingFrom.Maximum = new decimal(array5);
			this.DelaysBetweenPostingFrom.Name = "DelaysBetweenPostingFrom";
			this.DelaysBetweenPostingFrom.Size = new global::System.Drawing.Size(52, 20);
			this.DelaysBetweenPostingFrom.TabIndex = 13;
			this.label5.AutoSize = true;
			this.label5.Location = new global::System.Drawing.Point(6, 51);
			this.label5.Name = "label5";
			this.label5.Size = new global::System.Drawing.Size(148, 13);
			this.label5.TabIndex = 12;
			this.label5.Text = "Delays Between Posting from:";
			this.NeedToPost.Location = new global::System.Drawing.Point(88, 19);
			this.NeedToPost.Name = "NeedToPost";
			this.NeedToPost.Size = new global::System.Drawing.Size(66, 20);
			this.NeedToPost.TabIndex = 10;
			this.label4.AutoSize = true;
			this.label4.Location = new global::System.Drawing.Point(6, 21);
			this.label4.Name = "label4";
			this.label4.Size = new global::System.Drawing.Size(76, 13);
			this.label4.TabIndex = 11;
			this.label4.Text = "Need To Post:";
			this.groupBox4.Controls.Add(this.label15);
			this.groupBox4.Controls.Add(this.label14);
			this.groupBox4.Controls.Add(this.Use1BioOn);
			this.groupBox4.Controls.Add(this.ClearBioAfterLogin);
			this.groupBox4.Controls.Add(this.BioAfterPosting);
			this.groupBox4.Controls.Add(this.BioAfterCreating);
			this.groupBox4.Controls.Add(this.UpdateBio);
			this.groupBox4.Controls.Add(this.MentionInBio);
			this.groupBox4.Location = new global::System.Drawing.Point(12, 126);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new global::System.Drawing.Size(331, 74);
			this.groupBox4.TabIndex = 26;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Bio";
			this.label15.AutoSize = true;
			this.label15.Location = new global::System.Drawing.Point(239, 43);
			this.label15.Name = "label15";
			this.label15.Size = new global::System.Drawing.Size(51, 13);
			this.label15.TabIndex = 29;
			this.label15.Text = "accounts";
			this.label14.AutoSize = true;
			this.label14.Location = new global::System.Drawing.Point(106, 43);
			this.label14.Name = "label14";
			this.label14.Size = new global::System.Drawing.Size(73, 13);
			this.label14.TabIndex = 28;
			this.label14.Text = "Use 1 Bio On:";
			this.Use1BioOn.Location = new global::System.Drawing.Point(182, 41);
			global::System.Windows.Forms.NumericUpDown use1BioOn = this.Use1BioOn;
			int[] array6 = new int[4];
			array6[0] = 99999999;
			use1BioOn.Maximum = new decimal(array6);
			this.Use1BioOn.Name = "Use1BioOn";
			this.Use1BioOn.Size = new global::System.Drawing.Size(52, 20);
			this.Use1BioOn.TabIndex = 28;
			this.ClearBioAfterLogin.AutoSize = true;
			this.ClearBioAfterLogin.Location = new global::System.Drawing.Point(83, 19);
			this.ClearBioAfterLogin.Name = "ClearBioAfterLogin";
			this.ClearBioAfterLogin.Size = new global::System.Drawing.Size(77, 17);
			this.ClearBioAfterLogin.TabIndex = 16;
			this.ClearBioAfterLogin.Text = "Clear Bio L";
			this.ClearBioAfterLogin.UseVisualStyleBackColor = true;
			this.BioAfterPosting.AutoSize = true;
			this.BioAfterPosting.Location = new global::System.Drawing.Point(240, 18);
			this.BioAfterPosting.Name = "BioAfterPosting";
			this.BioAfterPosting.Size = new global::System.Drawing.Size(85, 17);
			this.BioAfterPosting.TabIndex = 15;
			this.BioAfterPosting.TabStop = true;
			this.BioAfterPosting.Text = "After Posting";
			this.BioAfterPosting.UseVisualStyleBackColor = true;
			this.BioAfterCreating.AutoSize = true;
			this.BioAfterCreating.Location = new global::System.Drawing.Point(166, 18);
			this.BioAfterCreating.Name = "BioAfterCreating";
			this.BioAfterCreating.Size = new global::System.Drawing.Size(68, 17);
			this.BioAfterCreating.TabIndex = 14;
			this.BioAfterCreating.TabStop = true;
			this.BioAfterCreating.Text = "After C/L";
			this.BioAfterCreating.UseVisualStyleBackColor = true;
			this.UpdateBio.AutoSize = true;
			this.UpdateBio.Location = new global::System.Drawing.Point(6, 19);
			this.UpdateBio.Name = "UpdateBio";
			this.UpdateBio.Size = new global::System.Drawing.Size(79, 17);
			this.UpdateBio.TabIndex = 1;
			this.UpdateBio.Text = "Update Bio";
			this.UpdateBio.UseVisualStyleBackColor = true;
			this.UpdateBio.CheckedChanged += new global::System.EventHandler(this.UpdateBio_CheckedChanged_1);
			this.MentionInBio.AutoSize = true;
			this.MentionInBio.Location = new global::System.Drawing.Point(6, 42);
			this.MentionInBio.Name = "MentionInBio";
			this.MentionInBio.Size = new global::System.Drawing.Size(94, 17);
			this.MentionInBio.TabIndex = 13;
			this.MentionInBio.Text = "Mention In Bio";
			this.MentionInBio.UseVisualStyleBackColor = true;
			this.groupBox3.Controls.Add(this.AddTags);
			this.groupBox3.Controls.Add(this.label3);
			this.groupBox3.Controls.Add(this.TagsCount);
			this.groupBox3.Controls.Add(this.PlusMention);
			this.groupBox3.Location = new global::System.Drawing.Point(349, 126);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new global::System.Drawing.Size(200, 67);
			this.groupBox3.TabIndex = 25;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Tags";
			this.AddTags.AutoSize = true;
			this.AddTags.Location = new global::System.Drawing.Point(6, 19);
			this.AddTags.Name = "AddTags";
			this.AddTags.Size = new global::System.Drawing.Size(72, 17);
			this.AddTags.TabIndex = 15;
			this.AddTags.Text = "Add Tags";
			this.AddTags.UseVisualStyleBackColor = true;
			this.AddTags.CheckedChanged += new global::System.EventHandler(this.AddTags_CheckedChanged_1);
			this.label3.AutoSize = true;
			this.label3.Location = new global::System.Drawing.Point(84, 20);
			this.label3.Name = "label3";
			this.label3.Size = new global::System.Drawing.Size(65, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Tags Count:";
			this.TagsCount.Location = new global::System.Drawing.Point(155, 16);
			global::System.Windows.Forms.NumericUpDown tagsCount = this.TagsCount;
			int[] array7 = new int[4];
			array7[0] = 5;
			tagsCount.Maximum = new decimal(array7);
			global::System.Windows.Forms.NumericUpDown tagsCount2 = this.TagsCount;
			int[] array8 = new int[4];
			array8[0] = 1;
			tagsCount2.Minimum = new decimal(array8);
			this.TagsCount.Name = "TagsCount";
			this.TagsCount.Size = new global::System.Drawing.Size(36, 20);
			this.TagsCount.TabIndex = 6;
			global::System.Windows.Forms.NumericUpDown tagsCount3 = this.TagsCount;
			int[] array9 = new int[4];
			array9[0] = 1;
			tagsCount3.Value = new decimal(array9);
			this.PlusMention.AutoSize = true;
			this.PlusMention.Location = new global::System.Drawing.Point(6, 42);
			this.PlusMention.Name = "PlusMention";
			this.PlusMention.Size = new global::System.Drawing.Size(161, 17);
			this.PlusMention.TabIndex = 8;
			this.PlusMention.Text = "Plus Mention (In Description)";
			this.PlusMention.UseVisualStyleBackColor = true;
			this.groupBox2.Controls.Add(this.AddRandomMusic);
			this.groupBox2.Controls.Add(this.AddMusic);
			this.groupBox2.Controls.Add(this.TurnOffOriginalSound);
			this.groupBox2.Controls.Add(this.TurnOffAdditionalSound);
			this.groupBox2.Location = new global::System.Drawing.Point(493, 6);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new global::System.Drawing.Size(156, 114);
			this.groupBox2.TabIndex = 24;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Music";
			this.AddRandomMusic.AutoSize = true;
			this.AddRandomMusic.Location = new global::System.Drawing.Point(6, 42);
			this.AddRandomMusic.Name = "AddRandomMusic";
			this.AddRandomMusic.Size = new global::System.Drawing.Size(119, 17);
			this.AddRandomMusic.TabIndex = 16;
			this.AddRandomMusic.Text = "Add Random Music";
			this.AddRandomMusic.UseVisualStyleBackColor = true;
			this.AddRandomMusic.CheckedChanged += new global::System.EventHandler(this.AddRandomMusic_CheckedChanged);
			this.AddMusic.AutoSize = true;
			this.AddMusic.Location = new global::System.Drawing.Point(6, 19);
			this.AddMusic.Name = "AddMusic";
			this.AddMusic.Size = new global::System.Drawing.Size(76, 17);
			this.AddMusic.TabIndex = 17;
			this.AddMusic.Text = "Add Music";
			this.AddMusic.UseVisualStyleBackColor = true;
			this.AddMusic.CheckedChanged += new global::System.EventHandler(this.AddMusic_CheckedChanged_1);
			this.TurnOffOriginalSound.AutoSize = true;
			this.TurnOffOriginalSound.Location = new global::System.Drawing.Point(6, 65);
			this.TurnOffOriginalSound.Name = "TurnOffOriginalSound";
			this.TurnOffOriginalSound.Size = new global::System.Drawing.Size(137, 17);
			this.TurnOffOriginalSound.TabIndex = 2;
			this.TurnOffOriginalSound.Text = "Turn Off Original Sound";
			this.TurnOffOriginalSound.UseVisualStyleBackColor = true;
			this.TurnOffAdditionalSound.AutoSize = true;
			this.TurnOffAdditionalSound.Location = new global::System.Drawing.Point(6, 88);
			this.TurnOffAdditionalSound.Name = "TurnOffAdditionalSound";
			this.TurnOffAdditionalSound.Size = new global::System.Drawing.Size(148, 17);
			this.TurnOffAdditionalSound.TabIndex = 3;
			this.TurnOffAdditionalSound.Text = "Turn Off Additional Sound";
			this.TurnOffAdditionalSound.UseVisualStyleBackColor = true;
			this.groupBox1.Controls.Add(this.AddComments);
			this.groupBox1.Controls.Add(this.AddTextToComment);
			this.groupBox1.Controls.Add(this.AddImageToComment);
			this.groupBox1.Controls.Add(this.AddMentionToComment);
			this.groupBox1.Location = new global::System.Drawing.Point(328, 6);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new global::System.Drawing.Size(159, 114);
			this.groupBox1.TabIndex = 23;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Comments";
			this.AddComments.AutoSize = true;
			this.AddComments.Location = new global::System.Drawing.Point(6, 18);
			this.AddComments.Name = "AddComments";
			this.AddComments.Size = new global::System.Drawing.Size(97, 17);
			this.AddComments.TabIndex = 20;
			this.AddComments.Text = "Add Comments";
			this.AddComments.UseVisualStyleBackColor = true;
			this.AddComments.CheckedChanged += new global::System.EventHandler(this.AddComments_CheckedChanged_1);
			this.AddTextToComment.AutoSize = true;
			this.AddTextToComment.Location = new global::System.Drawing.Point(6, 64);
			this.AddTextToComment.Name = "AddTextToComment";
			this.AddTextToComment.Size = new global::System.Drawing.Size(132, 17);
			this.AddTextToComment.TabIndex = 14;
			this.AddTextToComment.Text = "Add Text To Comment";
			this.AddTextToComment.UseVisualStyleBackColor = true;
			this.AddImageToComment.AutoSize = true;
			this.AddImageToComment.Location = new global::System.Drawing.Point(6, 87);
			this.AddImageToComment.Name = "AddImageToComment";
			this.AddImageToComment.Size = new global::System.Drawing.Size(140, 17);
			this.AddImageToComment.TabIndex = 19;
			this.AddImageToComment.Text = "Add Image To Comment";
			this.AddImageToComment.UseVisualStyleBackColor = true;
			this.AddMentionToComment.AutoSize = true;
			this.AddMentionToComment.Location = new global::System.Drawing.Point(6, 41);
			this.AddMentionToComment.Name = "AddMentionToComment";
			this.AddMentionToComment.Size = new global::System.Drawing.Size(149, 17);
			this.AddMentionToComment.TabIndex = 18;
			this.AddMentionToComment.Text = "Add Mention To Comment";
			this.AddMentionToComment.UseVisualStyleBackColor = true;
			this.WiFiTab.Controls.Add(this.VpnType);
			this.WiFiTab.Controls.Add(this.label12);
			this.WiFiTab.Controls.Add(this.piaData);
			this.WiFiTab.Controls.Add(this.AccountsOnVpn);
			this.WiFiTab.Controls.Add(this.label10);
			this.WiFiTab.Controls.Add(this.VPNByLine);
			this.WiFiTab.Controls.Add(this.VPNRandomLine);
			this.WiFiTab.Controls.Add(this.ConnectWifi);
			this.WiFiTab.Controls.Add(this.ProxyType);
			this.WiFiTab.Controls.Add(this.UseShadowRocket);
			this.WiFiTab.Controls.Add(this.UseVPN);
			this.WiFiTab.Controls.Add(this.label2);
			this.WiFiTab.Controls.Add(this.label1);
			this.WiFiTab.Controls.Add(this.WiFiPassword);
			this.WiFiTab.Controls.Add(this.WiFiName);
			this.WiFiTab.Location = new global::System.Drawing.Point(4, 22);
			this.WiFiTab.Name = "WiFiTab";
			this.WiFiTab.Padding = new global::System.Windows.Forms.Padding(3);
			this.WiFiTab.Size = new global::System.Drawing.Size(652, 279);
			this.WiFiTab.TabIndex = 3;
			this.WiFiTab.Text = "WiFi Settings";
			this.WiFiTab.UseVisualStyleBackColor = true;
			this.VpnType.Items.Add("PiaVpn");
			this.VpnType.Items.Add("BelkaVpn");
			this.VpnType.Location = new global::System.Drawing.Point(277, 83);
			this.VpnType.Name = "VpnType";
			this.VpnType.Size = new global::System.Drawing.Size(120, 20);
			this.VpnType.TabIndex = 19;
			this.VpnType.Text = "VpnType";
			this.label12.AutoSize = true;
			this.label12.Location = new global::System.Drawing.Point(8, 110);
			this.label12.Name = "label12";
			this.label12.Size = new global::System.Drawing.Size(51, 13);
			this.label12.TabIndex = 18;
			this.label12.Text = "Pia Data:";
			this.piaData.Location = new global::System.Drawing.Point(65, 107);
			this.piaData.Name = "piaData";
			this.piaData.Size = new global::System.Drawing.Size(174, 20);
			this.piaData.TabIndex = 17;
			this.AccountsOnVpn.Location = new global::System.Drawing.Point(115, 81);
			this.AccountsOnVpn.Name = "AccountsOnVpn";
			this.AccountsOnVpn.Size = new global::System.Drawing.Size(57, 20);
			this.AccountsOnVpn.TabIndex = 16;
			this.label10.AutoSize = true;
			this.label10.Location = new global::System.Drawing.Point(8, 83);
			this.label10.Name = "label10";
			this.label10.Size = new global::System.Drawing.Size(101, 13);
			this.label10.TabIndex = 15;
			this.label10.Text = "Accounts on 1 VPN";
			this.VPNByLine.AutoSize = true;
			this.VPNByLine.Location = new global::System.Drawing.Point(202, 58);
			this.VPNByLine.Name = "VPNByLine";
			this.VPNByLine.Size = new global::System.Drawing.Size(60, 17);
			this.VPNByLine.TabIndex = 14;
			this.VPNByLine.TabStop = true;
			this.VPNByLine.Text = "By Line";
			this.VPNByLine.UseVisualStyleBackColor = true;
			this.VPNRandomLine.AutoSize = true;
			this.VPNRandomLine.Location = new global::System.Drawing.Point(108, 58);
			this.VPNRandomLine.Name = "VPNRandomLine";
			this.VPNRandomLine.Size = new global::System.Drawing.Size(88, 17);
			this.VPNRandomLine.TabIndex = 13;
			this.VPNRandomLine.TabStop = true;
			this.VPNRandomLine.Text = "Random Line";
			this.VPNRandomLine.UseVisualStyleBackColor = true;
			this.ConnectWifi.AutoSize = true;
			this.ConnectWifi.Location = new global::System.Drawing.Point(9, 57);
			this.ConnectWifi.Name = "ConnectWifi";
			this.ConnectWifi.Size = new global::System.Drawing.Size(93, 17);
			this.ConnectWifi.TabIndex = 12;
			this.ConnectWifi.Text = "Connect Wi-Fi";
			this.ConnectWifi.UseVisualStyleBackColor = true;
			this.ProxyType.Items.Add("Socks5");
			this.ProxyType.Items.Add("Http");
			this.ProxyType.Location = new global::System.Drawing.Point(277, 54);
			this.ProxyType.Name = "ProxyType";
			this.ProxyType.Size = new global::System.Drawing.Size(120, 20);
			this.ProxyType.TabIndex = 11;
			this.ProxyType.Text = "domainUpDown1";
			this.UseShadowRocket.AutoSize = true;
			this.UseShadowRocket.Location = new global::System.Drawing.Point(275, 31);
			this.UseShadowRocket.Name = "UseShadowRocket";
			this.UseShadowRocket.Size = new global::System.Drawing.Size(122, 17);
			this.UseShadowRocket.TabIndex = 10;
			this.UseShadowRocket.Text = "Use ShadowRocket";
			this.UseShadowRocket.UseVisualStyleBackColor = true;
			this.UseShadowRocket.CheckedChanged += new global::System.EventHandler(this.UseShadowRocket_CheckedChanged);
			this.UseVPN.AutoSize = true;
			this.UseVPN.Location = new global::System.Drawing.Point(275, 9);
			this.UseVPN.Name = "UseVPN";
			this.UseVPN.Size = new global::System.Drawing.Size(67, 17);
			this.UseVPN.TabIndex = 9;
			this.UseVPN.Text = "UseVPN";
			this.UseVPN.UseVisualStyleBackColor = true;
			this.UseVPN.CheckedChanged += new global::System.EventHandler(this.UseVPN_CheckedChanged);
			this.label2.AutoSize = true;
			this.label2.Location = new global::System.Drawing.Point(6, 35);
			this.label2.Name = "label2";
			this.label2.Size = new global::System.Drawing.Size(83, 13);
			this.label2.TabIndex = 8;
			this.label2.Text = "Wi-Fi Password:";
			this.label1.AutoSize = true;
			this.label1.Location = new global::System.Drawing.Point(24, 9);
			this.label1.Name = "label1";
			this.label1.Size = new global::System.Drawing.Size(65, 13);
			this.label1.TabIndex = 7;
			this.label1.Text = "Wi-Fi Name:";
			this.WiFiPassword.Location = new global::System.Drawing.Point(95, 32);
			this.WiFiPassword.Name = "WiFiPassword";
			this.WiFiPassword.Size = new global::System.Drawing.Size(174, 20);
			this.WiFiPassword.TabIndex = 6;
			this.WiFiName.Location = new global::System.Drawing.Point(95, 6);
			this.WiFiName.Name = "WiFiName";
			this.WiFiName.Size = new global::System.Drawing.Size(174, 20);
			this.WiFiName.TabIndex = 5;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(800, 450);
			base.Controls.Add(this.TabControl1);
			base.Controls.Add(this.button1);
			base.Name = "SettingsForm";
			this.Text = "SettingsForm";
			this.TabControl1.ResumeLayout(false);
			this.MainSettingsTab.ResumeLayout(false);
			this.MainSettingsTab.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.ResetNetworkAfter).EndInit();
			this.FeaturesTab.ResumeLayout(false);
			this.groupBox5.ResumeLayout(false);
			this.groupBox5.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.PicturesPostCount).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.DelaysBetweenPostingTo).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.DelaysBetweenPostingFrom).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.NeedToPost).EndInit();
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.Use1BioOn).EndInit();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.TagsCount).EndInit();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.WiFiTab.ResumeLayout(false);
			this.WiFiTab.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.AccountsOnVpn).EndInit();
			base.ResumeLayout(false);
		}

		// Token: 0x04000565 RID: 1381
		private global::System.ComponentModel.IContainer components;

		// Token: 0x04000566 RID: 1382
		private global::System.Windows.Forms.Button button1;

		// Token: 0x04000567 RID: 1383
		private global::System.Windows.Forms.TabControl TabControl1;

		// Token: 0x04000568 RID: 1384
		private global::System.Windows.Forms.TabPage MainSettingsTab;

		// Token: 0x04000569 RID: 1385
		private global::System.Windows.Forms.TabPage FeaturesTab;

		// Token: 0x0400056A RID: 1386
		private global::System.Windows.Forms.CheckBox UploadPicture;

		// Token: 0x0400056B RID: 1387
		private global::System.Windows.Forms.CheckBox RemoveVideoAfterUse;

		// Token: 0x0400056C RID: 1388
		private global::System.Windows.Forms.CheckBox NameFromTxt;

		// Token: 0x0400056D RID: 1389
		private global::System.Windows.Forms.GroupBox groupBox2;

		// Token: 0x0400056E RID: 1390
		private global::System.Windows.Forms.CheckBox AddRandomMusic;

		// Token: 0x0400056F RID: 1391
		private global::System.Windows.Forms.CheckBox AddMusic;

		// Token: 0x04000570 RID: 1392
		private global::System.Windows.Forms.CheckBox TurnOffOriginalSound;

		// Token: 0x04000571 RID: 1393
		private global::System.Windows.Forms.CheckBox TurnOffAdditionalSound;

		// Token: 0x04000572 RID: 1394
		private global::System.Windows.Forms.GroupBox groupBox1;

		// Token: 0x04000573 RID: 1395
		private global::System.Windows.Forms.CheckBox AddComments;

		// Token: 0x04000574 RID: 1396
		private global::System.Windows.Forms.CheckBox AddTextToComment;

		// Token: 0x04000575 RID: 1397
		private global::System.Windows.Forms.CheckBox AddImageToComment;

		// Token: 0x04000576 RID: 1398
		private global::System.Windows.Forms.CheckBox AddMentionToComment;

		// Token: 0x04000577 RID: 1399
		private global::System.Windows.Forms.GroupBox groupBox3;

		// Token: 0x04000578 RID: 1400
		private global::System.Windows.Forms.CheckBox AddTags;

		// Token: 0x04000579 RID: 1401
		private global::System.Windows.Forms.Label label3;

		// Token: 0x0400057A RID: 1402
		private global::System.Windows.Forms.NumericUpDown TagsCount;

		// Token: 0x0400057B RID: 1403
		private global::System.Windows.Forms.CheckBox PlusMention;

		// Token: 0x0400057C RID: 1404
		private global::System.Windows.Forms.GroupBox groupBox4;

		// Token: 0x0400057D RID: 1405
		private global::System.Windows.Forms.CheckBox UpdateBio;

		// Token: 0x0400057E RID: 1406
		private global::System.Windows.Forms.CheckBox MentionInBio;

		// Token: 0x0400057F RID: 1407
		private global::System.Windows.Forms.GroupBox groupBox5;

		// Token: 0x04000580 RID: 1408
		private global::System.Windows.Forms.Label label7;

		// Token: 0x04000581 RID: 1409
		private global::System.Windows.Forms.NumericUpDown DelaysBetweenPostingTo;

		// Token: 0x04000582 RID: 1410
		private global::System.Windows.Forms.Label label6;

		// Token: 0x04000583 RID: 1411
		private global::System.Windows.Forms.NumericUpDown DelaysBetweenPostingFrom;

		// Token: 0x04000584 RID: 1412
		private global::System.Windows.Forms.Label label5;

		// Token: 0x04000585 RID: 1413
		private global::System.Windows.Forms.NumericUpDown NeedToPost;

		// Token: 0x04000586 RID: 1414
		private global::System.Windows.Forms.Label label4;

		// Token: 0x04000587 RID: 1415
		private global::System.Windows.Forms.TabPage WiFiTab;

		// Token: 0x04000588 RID: 1416
		private global::System.Windows.Forms.CheckBox UseVPN;

		// Token: 0x04000589 RID: 1417
		private global::System.Windows.Forms.Label label2;

		// Token: 0x0400058A RID: 1418
		private global::System.Windows.Forms.Label label1;

		// Token: 0x0400058B RID: 1419
		private global::System.Windows.Forms.TextBox WiFiPassword;

		// Token: 0x0400058C RID: 1420
		private global::System.Windows.Forms.TextBox WiFiName;

		// Token: 0x0400058D RID: 1421
		private global::System.Windows.Forms.CheckBox FirstPosting;

		// Token: 0x0400058E RID: 1422
		private global::System.Windows.Forms.Label label9;

		// Token: 0x0400058F RID: 1423
		private global::System.Windows.Forms.NumericUpDown ResetNetworkAfter;

		// Token: 0x04000590 RID: 1424
		private global::System.Windows.Forms.Label label8;

		// Token: 0x04000591 RID: 1425
		private global::System.Windows.Forms.CheckBox ResetNetworkInRun;

		// Token: 0x04000592 RID: 1426
		private global::System.Windows.Forms.CheckBox RecordScreen;

		// Token: 0x04000593 RID: 1427
		private global::System.Windows.Forms.CheckBox UseShadowRocket;

		// Token: 0x04000594 RID: 1428
		private global::System.Windows.Forms.DomainUpDown ProxyType;

		// Token: 0x04000595 RID: 1429
		private global::System.Windows.Forms.CheckBox Use2FA;

		// Token: 0x04000596 RID: 1430
		private global::System.Windows.Forms.CheckBox ConnectWifi;

		// Token: 0x04000597 RID: 1431
		private global::System.Windows.Forms.CheckBox ClearGallery;

		// Token: 0x04000598 RID: 1432
		private global::System.Windows.Forms.RadioButton BioAfterPosting;

		// Token: 0x04000599 RID: 1433
		private global::System.Windows.Forms.RadioButton BioAfterCreating;

		// Token: 0x0400059A RID: 1434
		private global::System.Windows.Forms.RadioButton MakingPlusPosting;

		// Token: 0x0400059B RID: 1435
		private global::System.Windows.Forms.RadioButton Posting;

		// Token: 0x0400059C RID: 1436
		private global::System.Windows.Forms.RadioButton Making;

		// Token: 0x0400059D RID: 1437
		private global::System.Windows.Forms.RadioButton VPNByLine;

		// Token: 0x0400059E RID: 1438
		private global::System.Windows.Forms.RadioButton VPNRandomLine;

		// Token: 0x0400059F RID: 1439
		private global::System.Windows.Forms.NumericUpDown AccountsOnVpn;

		// Token: 0x040005A0 RID: 1440
		private global::System.Windows.Forms.Label label10;

		// Token: 0x040005A1 RID: 1441
		private global::System.Windows.Forms.CheckBox SkipReLogin;

		// Token: 0x040005A2 RID: 1442
		private global::System.Windows.Forms.Label label11;

		// Token: 0x040005A3 RID: 1443
		private global::System.Windows.Forms.TextBox AnyMessageApiKey;

		// Token: 0x040005A4 RID: 1444
		private global::System.Windows.Forms.Label label12;

		// Token: 0x040005A5 RID: 1445
		private global::System.Windows.Forms.TextBox piaData;

		// Token: 0x040005A6 RID: 1446
		private global::System.Windows.Forms.CheckBox PostSameVideo;

		// Token: 0x040005A7 RID: 1447
		private global::System.Windows.Forms.CheckBox ClearBioAfterLogin;

		// Token: 0x040005A8 RID: 1448
		private global::System.Windows.Forms.Label label13;

		// Token: 0x040005A9 RID: 1449
		private global::System.Windows.Forms.ComboBox EmailDomain;

		// Token: 0x040005AA RID: 1450
		private global::System.Windows.Forms.CheckBox VideosFrom1Folder;

		// Token: 0x040005AB RID: 1451
		private global::System.Windows.Forms.Label label14;

		// Token: 0x040005AC RID: 1452
		private global::System.Windows.Forms.NumericUpDown Use1BioOn;

		// Token: 0x040005AD RID: 1453
		private global::System.Windows.Forms.Label label15;

		// Token: 0x040005AE RID: 1454
		private global::System.Windows.Forms.CheckBox AnymessageMinus1Hour;

		// Token: 0x040005AF RID: 1455
		private global::System.Windows.Forms.NumericUpDown PicturesPostCount;

		// Token: 0x040005B0 RID: 1456
		private global::System.Windows.Forms.Label label16;

		// Token: 0x040005B1 RID: 1457
		private global::System.Windows.Forms.CheckBox PostPictures;

		// Token: 0x040005B2 RID: 1458
		private global::System.Windows.Forms.CheckBox DontSwitchCheckBoxes;

		// Token: 0x040005B3 RID: 1459
		private global::System.Windows.Forms.DomainUpDown VpnType;
	}
}
