using System.ComponentModel;
using YamuiFramework.Controls;
using _3PA.MainFeatures.Appli.Pages;
using _3PA.MainFeatures.Appli.Pages.control;
using _3PA.MainFeatures.Appli.Pages.Navigation;

namespace _3PA.MainFeatures.Appli {
    partial class AppliForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.yamuiTabControlMain = new YamuiFramework.Controls.YamuiTabControl();
            this.pageClassic = new YamuiFramework.Controls.YamuiTabPage();
            this.yamuiTabControl2 = new YamuiFramework.Controls.YamuiTabControl();
            this.yamuiTabPage2 = new YamuiFramework.Controls.YamuiTabPage();
            this.classic1 = new _3PA.MainFeatures.Appli.Pages.control.Classic();
            this.yamuiTabPage6 = new YamuiFramework.Controls.YamuiTabPage();
            this.itemControl1 = new _3PA.MainFeatures.Appli.Pages.control.ItemControl();
            this.yamuiTabPage7 = new YamuiFramework.Controls.YamuiTabPage();
            this.text1 = new _3PA.MainFeatures.Appli.Pages.control.Text();
            this.yamuiTabPage4 = new YamuiFramework.Controls.YamuiTabPage();
            this.progress1 = new _3PA.MainFeatures.Appli.Pages.control.Progress();
            this.yamuiTabPage3 = new YamuiFramework.Controls.YamuiTabPage();
            this.pageFormAndNav = new YamuiFramework.Controls.YamuiTabPage();
            this.yamuiTabControl4 = new YamuiFramework.Controls.YamuiTabControl();
            this.yamuiTabPage12 = new YamuiFramework.Controls.YamuiTabPage();
            this.navigation1 = new _3PA.MainFeatures.Appli.Pages.PageTemplate();
            this.yamuiTabPage11 = new YamuiFramework.Controls.YamuiTabPage();
            this.other1 = new _3PA.MainFeatures.Appli.Pages.Navigation.Other();
            this.yamuiTabPage10 = new YamuiFramework.Controls.YamuiTabPage();
            this.yamuiTabMainSetting = new YamuiFramework.Controls.YamuiTabPage();
            this.yamuiTabControlSecSetting = new YamuiFramework.Controls.YamuiTabControl();
            this.yamuiTabSecAppearance = new YamuiFramework.Controls.YamuiTabPage();
            this.settingAppearance1 = new _3PA.MainFeatures.Appli.Pages.SettingAppearance();
            this.yamuiPanel3 = new YamuiFramework.Controls.YamuiPanel();
            this.yamuiLabel24 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLink6 = new YamuiFramework.Controls.YamuiLink();
            this.yamuiLabel25 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLink7 = new YamuiFramework.Controls.YamuiLink();
            this.yamuiLink8 = new YamuiFramework.Controls.YamuiLink();
            this.yamuiLabel19 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLabel23 = new YamuiFramework.Controls.YamuiLabel();
            this.statusLabel = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiTabControlMain.SuspendLayout();
            this.pageClassic.SuspendLayout();
            this.yamuiTabControl2.SuspendLayout();
            this.yamuiTabPage2.SuspendLayout();
            this.yamuiTabPage6.SuspendLayout();
            this.yamuiTabPage7.SuspendLayout();
            this.yamuiTabPage4.SuspendLayout();
            this.pageFormAndNav.SuspendLayout();
            this.yamuiTabControl4.SuspendLayout();
            this.yamuiTabPage12.SuspendLayout();
            this.yamuiTabPage11.SuspendLayout();
            this.yamuiTabMainSetting.SuspendLayout();
            this.yamuiTabControlSecSetting.SuspendLayout();
            this.yamuiTabSecAppearance.SuspendLayout();
            this.yamuiPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // yamuiTabControlMain
            // 
            this.yamuiTabControlMain.Controls.Add(this.pageClassic);
            this.yamuiTabControlMain.Controls.Add(this.pageFormAndNav);
            this.yamuiTabControlMain.Controls.Add(this.yamuiTabMainSetting);
            this.yamuiTabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.yamuiTabControlMain.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.yamuiTabControlMain.ItemSize = new System.Drawing.Size(0, 32);
            this.yamuiTabControlMain.Location = new System.Drawing.Point(40, 40);
            this.yamuiTabControlMain.Margin = new System.Windows.Forms.Padding(0);
            this.yamuiTabControlMain.Name = "yamuiTabControlMain";
            this.yamuiTabControlMain.Padding = new System.Drawing.Point(0, 0);
            this.yamuiTabControlMain.SelectedIndex = 0;
            this.yamuiTabControlMain.SelectIndex = 0;
            this.yamuiTabControlMain.ShowNormallyHiddenTabs = false;
            this.yamuiTabControlMain.Size = new System.Drawing.Size(755, 418);
            this.yamuiTabControlMain.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
            this.yamuiTabControlMain.TabIndex = 2;
            this.yamuiTabControlMain.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.yamuiTabControlMain.SelectedIndexChanged += new System.EventHandler(this.yamuiTabControl1_SelectedIndexChanged);
            // 
            // pageClassic
            // 
            this.pageClassic.Controls.Add(this.yamuiTabControl2);
            this.pageClassic.Location = new System.Drawing.Point(4, 36);
            this.pageClassic.Name = "pageClassic";
            this.pageClassic.Size = new System.Drawing.Size(747, 378);
            this.pageClassic.TabIndex = 0;
            this.pageClassic.Text = "&control";
            // 
            // yamuiTabControl2
            // 
            this.yamuiTabControl2.Controls.Add(this.yamuiTabPage2);
            this.yamuiTabControl2.Controls.Add(this.yamuiTabPage6);
            this.yamuiTabControl2.Controls.Add(this.yamuiTabPage7);
            this.yamuiTabControl2.Controls.Add(this.yamuiTabPage4);
            this.yamuiTabControl2.Controls.Add(this.yamuiTabPage3);
            this.yamuiTabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.yamuiTabControl2.Function = YamuiFramework.Fonts.TabFunction.Secondary;
            this.yamuiTabControl2.ItemSize = new System.Drawing.Size(0, 18);
            this.yamuiTabControl2.Location = new System.Drawing.Point(0, 0);
            this.yamuiTabControl2.Name = "yamuiTabControl2";
            this.yamuiTabControl2.Padding = new System.Drawing.Point(6, 0);
            this.yamuiTabControl2.SelectedIndex = 0;
            this.yamuiTabControl2.SelectIndex = 0;
            this.yamuiTabControl2.ShowNormallyHiddenTabs = false;
            this.yamuiTabControl2.Size = new System.Drawing.Size(747, 378);
            this.yamuiTabControl2.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
            this.yamuiTabControl2.TabIndex = 2;
            // 
            // yamuiTabPage2
            // 
            this.yamuiTabPage2.BackColor = System.Drawing.Color.Transparent;
            this.yamuiTabPage2.Controls.Add(this.classic1);
            this.yamuiTabPage2.Function = YamuiFramework.Fonts.TabFunction.Secondary;
            this.yamuiTabPage2.Location = new System.Drawing.Point(4, 22);
            this.yamuiTabPage2.Name = "yamuiTabPage2";
            this.yamuiTabPage2.Padding = new System.Windows.Forms.Padding(30, 25, 0, 0);
            this.yamuiTabPage2.Size = new System.Drawing.Size(739, 352);
            this.yamuiTabPage2.TabIndex = 0;
            this.yamuiTabPage2.Text = "CLASSIC";
            // 
            // classic1
            // 
            this.classic1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.classic1.Location = new System.Drawing.Point(30, 25);
            this.classic1.Name = "classic1";
            this.classic1.Size = new System.Drawing.Size(709, 327);
            this.classic1.TabIndex = 0;
            this.classic1.Load += new System.EventHandler(this.classic1_Load);
            // 
            // yamuiTabPage6
            // 
            this.yamuiTabPage6.Controls.Add(this.itemControl1);
            this.yamuiTabPage6.Function = YamuiFramework.Fonts.TabFunction.Secondary;
            this.yamuiTabPage6.Location = new System.Drawing.Point(4, 22);
            this.yamuiTabPage6.Name = "yamuiTabPage6";
            this.yamuiTabPage6.Padding = new System.Windows.Forms.Padding(30, 25, 0, 0);
            this.yamuiTabPage6.Size = new System.Drawing.Size(739, 352);
            this.yamuiTabPage6.TabIndex = 3;
            this.yamuiTabPage6.Text = "ITEM CONTROL";
            // 
            // itemControl1
            // 
            this.itemControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.itemControl1.Location = new System.Drawing.Point(30, 25);
            this.itemControl1.Name = "itemControl1";
            this.itemControl1.Size = new System.Drawing.Size(709, 327);
            this.itemControl1.TabIndex = 0;
            // 
            // yamuiTabPage7
            // 
            this.yamuiTabPage7.Controls.Add(this.text1);
            this.yamuiTabPage7.Function = YamuiFramework.Fonts.TabFunction.Secondary;
            this.yamuiTabPage7.Location = new System.Drawing.Point(4, 22);
            this.yamuiTabPage7.Name = "yamuiTabPage7";
            this.yamuiTabPage7.Padding = new System.Windows.Forms.Padding(30, 25, 0, 0);
            this.yamuiTabPage7.Size = new System.Drawing.Size(739, 352);
            this.yamuiTabPage7.TabIndex = 4;
            this.yamuiTabPage7.Text = "TEXT";
            // 
            // text1
            // 
            this.text1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.text1.Location = new System.Drawing.Point(30, 25);
            this.text1.Name = "text1";
            this.text1.Size = new System.Drawing.Size(709, 327);
            this.text1.TabIndex = 0;
            this.text1.Load += new System.EventHandler(this.text1_Load);
            // 
            // yamuiTabPage4
            // 
            this.yamuiTabPage4.Controls.Add(this.progress1);
            this.yamuiTabPage4.Function = YamuiFramework.Fonts.TabFunction.Secondary;
            this.yamuiTabPage4.Location = new System.Drawing.Point(4, 22);
            this.yamuiTabPage4.Name = "yamuiTabPage4";
            this.yamuiTabPage4.Padding = new System.Windows.Forms.Padding(30, 25, 0, 0);
            this.yamuiTabPage4.Size = new System.Drawing.Size(739, 352);
            this.yamuiTabPage4.TabIndex = 2;
            this.yamuiTabPage4.Text = "PROGRESS / SCROLL BAR";
            // 
            // progress1
            // 
            this.progress1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progress1.Location = new System.Drawing.Point(30, 25);
            this.progress1.Name = "progress1";
            this.progress1.Size = new System.Drawing.Size(709, 327);
            this.progress1.TabIndex = 0;
            // 
            // yamuiTabPage3
            // 
            this.yamuiTabPage3.Function = YamuiFramework.Fonts.TabFunction.Secondary;
            this.yamuiTabPage3.Location = new System.Drawing.Point(4, 22);
            this.yamuiTabPage3.Name = "yamuiTabPage3";
            this.yamuiTabPage3.Padding = new System.Windows.Forms.Padding(30, 25, 0, 0);
            this.yamuiTabPage3.Size = new System.Drawing.Size(739, 352);
            this.yamuiTabPage3.TabIndex = 1;
            this.yamuiTabPage3.Text = "GRID";
            // 
            // pageFormAndNav
            // 
            this.pageFormAndNav.Controls.Add(this.yamuiTabControl4);
            this.pageFormAndNav.Location = new System.Drawing.Point(4, 36);
            this.pageFormAndNav.Name = "pageFormAndNav";
            this.pageFormAndNav.Size = new System.Drawing.Size(747, 378);
            this.pageFormAndNav.TabIndex = 2;
            this.pageFormAndNav.Text = "form and navigation";
            // 
            // yamuiTabControl4
            // 
            this.yamuiTabControl4.Controls.Add(this.yamuiTabPage12);
            this.yamuiTabControl4.Controls.Add(this.yamuiTabPage11);
            this.yamuiTabControl4.Controls.Add(this.yamuiTabPage10);
            this.yamuiTabControl4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.yamuiTabControl4.Function = YamuiFramework.Fonts.TabFunction.Secondary;
            this.yamuiTabControl4.ItemSize = new System.Drawing.Size(244, 18);
            this.yamuiTabControl4.Location = new System.Drawing.Point(0, 0);
            this.yamuiTabControl4.Margin = new System.Windows.Forms.Padding(0);
            this.yamuiTabControl4.Name = "yamuiTabControl4";
            this.yamuiTabControl4.Padding = new System.Drawing.Point(6, 0);
            this.yamuiTabControl4.SelectedIndex = 0;
            this.yamuiTabControl4.SelectIndex = 0;
            this.yamuiTabControl4.ShowNormallyHiddenTabs = false;
            this.yamuiTabControl4.Size = new System.Drawing.Size(747, 378);
            this.yamuiTabControl4.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
            this.yamuiTabControl4.TabIndex = 0;
            // 
            // yamuiTabPage12
            // 
            this.yamuiTabPage12.Controls.Add(this.navigation1);
            this.yamuiTabPage12.Function = YamuiFramework.Fonts.TabFunction.Secondary;
            this.yamuiTabPage12.Location = new System.Drawing.Point(4, 22);
            this.yamuiTabPage12.Name = "yamuiTabPage12";
            this.yamuiTabPage12.Padding = new System.Windows.Forms.Padding(30, 25, 0, 0);
            this.yamuiTabPage12.Size = new System.Drawing.Size(739, 352);
            this.yamuiTabPage12.TabIndex = 2;
            this.yamuiTabPage12.Text = "THIS FORM NAVIGATION";
            // 
            // navigation1
            // 
            this.navigation1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.navigation1.Location = new System.Drawing.Point(30, 25);
            this.navigation1.Name = "navigation1";
            this.navigation1.Size = new System.Drawing.Size(709, 327);
            this.navigation1.TabIndex = 0;
            // 
            // yamuiTabPage11
            // 
            this.yamuiTabPage11.Controls.Add(this.other1);
            this.yamuiTabPage11.Function = YamuiFramework.Fonts.TabFunction.Secondary;
            this.yamuiTabPage11.Location = new System.Drawing.Point(4, 22);
            this.yamuiTabPage11.Name = "yamuiTabPage11";
            this.yamuiTabPage11.Padding = new System.Windows.Forms.Padding(30, 25, 0, 0);
            this.yamuiTabPage11.Size = new System.Drawing.Size(739, 352);
            this.yamuiTabPage11.TabIndex = 1;
            this.yamuiTabPage11.Text = "OTHER";
            // 
            // other1
            // 
            this.other1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.other1.Location = new System.Drawing.Point(30, 25);
            this.other1.Name = "other1";
            this.other1.Size = new System.Drawing.Size(709, 327);
            this.other1.TabIndex = 0;
            // 
            // yamuiTabPage10
            // 
            this.yamuiTabPage10.Function = YamuiFramework.Fonts.TabFunction.Secondary;
            this.yamuiTabPage10.Location = new System.Drawing.Point(4, 22);
            this.yamuiTabPage10.Name = "yamuiTabPage10";
            this.yamuiTabPage10.Padding = new System.Windows.Forms.Padding(30, 25, 0, 0);
            this.yamuiTabPage10.Size = new System.Drawing.Size(739, 352);
            this.yamuiTabPage10.TabIndex = 0;
            this.yamuiTabPage10.Text = "MESSAGE BOX";
            // 
            // yamuiTabMainSetting
            // 
            this.yamuiTabMainSetting.Controls.Add(this.yamuiTabControlSecSetting);
            this.yamuiTabMainSetting.HiddenPage = true;
            this.yamuiTabMainSetting.Location = new System.Drawing.Point(4, 36);
            this.yamuiTabMainSetting.Name = "yamuiTabMainSetting";
            this.yamuiTabMainSetting.Size = new System.Drawing.Size(747, 378);
            this.yamuiTabMainSetting.TabIndex = 1;
            this.yamuiTabMainSetting.Text = "setting";
            // 
            // yamuiTabControlSecSetting
            // 
            this.yamuiTabControlSecSetting.Controls.Add(this.yamuiTabSecAppearance);
            this.yamuiTabControlSecSetting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.yamuiTabControlSecSetting.Function = YamuiFramework.Fonts.TabFunction.Secondary;
            this.yamuiTabControlSecSetting.ItemSize = new System.Drawing.Size(742, 18);
            this.yamuiTabControlSecSetting.Location = new System.Drawing.Point(0, 0);
            this.yamuiTabControlSecSetting.Name = "yamuiTabControlSecSetting";
            this.yamuiTabControlSecSetting.Padding = new System.Drawing.Point(6, 0);
            this.yamuiTabControlSecSetting.SelectedIndex = 0;
            this.yamuiTabControlSecSetting.SelectIndex = 0;
            this.yamuiTabControlSecSetting.ShowNormallyHiddenTabs = false;
            this.yamuiTabControlSecSetting.Size = new System.Drawing.Size(747, 378);
            this.yamuiTabControlSecSetting.TabIndex = 2;
            // 
            // yamuiTabSecAppearance
            // 
            this.yamuiTabSecAppearance.Controls.Add(this.settingAppearance1);
            this.yamuiTabSecAppearance.Function = YamuiFramework.Fonts.TabFunction.Secondary;
            this.yamuiTabSecAppearance.Location = new System.Drawing.Point(4, 22);
            this.yamuiTabSecAppearance.Name = "yamuiTabSecAppearance";
            this.yamuiTabSecAppearance.Padding = new System.Windows.Forms.Padding(30, 25, 0, 0);
            this.yamuiTabSecAppearance.Size = new System.Drawing.Size(739, 352);
            this.yamuiTabSecAppearance.TabIndex = 0;
            this.yamuiTabSecAppearance.Text = "APPEARANCE";
            this.yamuiTabSecAppearance.Click += new System.EventHandler(this.yamuiTabPage9_Click);
            // 
            // settingAppearance1
            // 
            this.settingAppearance1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingAppearance1.Location = new System.Drawing.Point(30, 25);
            this.settingAppearance1.Name = "settingAppearance1";
            this.settingAppearance1.Size = new System.Drawing.Size(709, 327);
            this.settingAppearance1.TabIndex = 0;
            // 
            // yamuiPanel3
            // 
            this.yamuiPanel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.yamuiPanel3.BackColor = System.Drawing.Color.Transparent;
            this.yamuiPanel3.Controls.Add(this.yamuiLabel24);
            this.yamuiPanel3.Controls.Add(this.yamuiLink6);
            this.yamuiPanel3.Controls.Add(this.yamuiLabel25);
            this.yamuiPanel3.Controls.Add(this.yamuiLink7);
            this.yamuiPanel3.Controls.Add(this.yamuiLink8);
            this.yamuiPanel3.HorizontalScrollbarHighlightOnWheel = false;
            this.yamuiPanel3.HorizontalScrollbarSize = 10;
            this.yamuiPanel3.Location = new System.Drawing.Point(552, 0);
            this.yamuiPanel3.Name = "yamuiPanel3";
            this.yamuiPanel3.Size = new System.Drawing.Size(144, 34);
            this.yamuiPanel3.TabIndex = 9;
            this.yamuiPanel3.VerticalScrollbarHighlightOnWheel = false;
            this.yamuiPanel3.VerticalScrollbarSize = 10;
            // 
            // yamuiLabel24
            // 
            this.yamuiLabel24.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.yamuiLabel24.AutoSize = true;
            this.yamuiLabel24.Function = YamuiFramework.Fonts.LabelFunction.Small;
            this.yamuiLabel24.Location = new System.Drawing.Point(56, 12);
            this.yamuiLabel24.Margin = new System.Windows.Forms.Padding(5);
            this.yamuiLabel24.Name = "yamuiLabel24";
            this.yamuiLabel24.Size = new System.Drawing.Size(7, 12);
            this.yamuiLabel24.TabIndex = 13;
            this.yamuiLabel24.Text = "|";
            // 
            // yamuiLink6
            // 
            this.yamuiLink6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.yamuiLink6.Function = YamuiFramework.Fonts.LabelFunction.TopLink;
            this.yamuiLink6.Location = new System.Drawing.Point(1, 10);
            this.yamuiLink6.Name = "yamuiLink6";
            this.yamuiLink6.Size = new System.Drawing.Size(57, 17);
            this.yamuiLink6.TabIndex = 12;
            this.yamuiLink6.TabStop = false;
            this.yamuiLink6.Text = "SETTINGS";
            this.yamuiLink6.Click += new System.EventHandler(this.yamuiLink6_Click);
            // 
            // yamuiLabel25
            // 
            this.yamuiLabel25.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.yamuiLabel25.AutoSize = true;
            this.yamuiLabel25.Function = YamuiFramework.Fonts.LabelFunction.Small;
            this.yamuiLabel25.Location = new System.Drawing.Point(103, 12);
            this.yamuiLabel25.Margin = new System.Windows.Forms.Padding(5);
            this.yamuiLabel25.Name = "yamuiLabel25";
            this.yamuiLabel25.Size = new System.Drawing.Size(7, 12);
            this.yamuiLabel25.TabIndex = 11;
            this.yamuiLabel25.Text = "|";
            // 
            // yamuiLink7
            // 
            this.yamuiLink7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.yamuiLink7.Function = YamuiFramework.Fonts.LabelFunction.TopLink;
            this.yamuiLink7.Location = new System.Drawing.Point(61, 10);
            this.yamuiLink7.Name = "yamuiLink7";
            this.yamuiLink7.Size = new System.Drawing.Size(43, 17);
            this.yamuiLink7.TabIndex = 10;
            this.yamuiLink7.TabStop = false;
            this.yamuiLink7.Text = "ABOUT";
            this.yamuiLink7.Click += new System.EventHandler(this.yamuiLink7_Click);
            // 
            // yamuiLink8
            // 
            this.yamuiLink8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.yamuiLink8.Function = YamuiFramework.Fonts.LabelFunction.TopLink;
            this.yamuiLink8.Location = new System.Drawing.Point(109, 10);
            this.yamuiLink8.Name = "yamuiLink8";
            this.yamuiLink8.Size = new System.Drawing.Size(32, 17);
            this.yamuiLink8.TabIndex = 9;
            this.yamuiLink8.TabStop = false;
            this.yamuiLink8.Text = "HELP";
            this.yamuiLink8.Click += new System.EventHandler(this.yamuiLink8_Click);
            // 
            // yamuiLabel19
            // 
            this.yamuiLabel19.AutoSize = true;
            this.yamuiLabel19.Enabled = false;
            this.yamuiLabel19.Function = YamuiFramework.Fonts.LabelFunction.Heading;
            this.yamuiLabel19.Location = new System.Drawing.Point(8, 10);
            this.yamuiLabel19.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel19.Name = "yamuiLabel19";
            this.yamuiLabel19.Size = new System.Drawing.Size(256, 19);
            this.yamuiLabel19.TabIndex = 10;
            this.yamuiLabel19.Text = "2PB - Progress Programmer\'s Buddy";
            // 
            // yamuiLabel23
            // 
            this.yamuiLabel23.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel23.Name = "yamuiLabel23";
            this.yamuiLabel23.Size = new System.Drawing.Size(100, 23);
            this.yamuiLabel23.TabIndex = 11;
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(1, 452);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(74, 15);
            this.statusLabel.TabIndex = 12;
            this.statusLabel.Text = "yamuiLabel1";
            // 
            // AppliForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(806, 469);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.yamuiLabel19);
            this.Controls.Add(this.yamuiPanel3);
            this.Controls.Add(this.yamuiTabControlMain);
            this.Controls.Add(this.yamuiLabel23);
            this.IsMainForm = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(806, 469);
            this.Name = "AppliForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.yamuiTabControlMain.ResumeLayout(false);
            this.pageClassic.ResumeLayout(false);
            this.yamuiTabControl2.ResumeLayout(false);
            this.yamuiTabPage2.ResumeLayout(false);
            this.yamuiTabPage6.ResumeLayout(false);
            this.yamuiTabPage7.ResumeLayout(false);
            this.yamuiTabPage4.ResumeLayout(false);
            this.pageFormAndNav.ResumeLayout(false);
            this.yamuiTabControl4.ResumeLayout(false);
            this.yamuiTabPage12.ResumeLayout(false);
            this.yamuiTabPage11.ResumeLayout(false);
            this.yamuiTabMainSetting.ResumeLayout(false);
            this.yamuiTabControlSecSetting.ResumeLayout(false);
            this.yamuiTabSecAppearance.ResumeLayout(false);
            this.yamuiPanel3.ResumeLayout(false);
            this.yamuiPanel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private YamuiTabControl yamuiTabControlMain;
        private YamuiTabPage pageClassic;
        private YamuiTabControl yamuiTabControl2;
        private YamuiTabPage yamuiTabPage2;
        private YamuiTabPage yamuiTabPage3;
        private YamuiTabPage yamuiTabPage4;
        private YamuiTabPage yamuiTabMainSetting;
        private YamuiTabPage yamuiTabPage6;
        private YamuiTabPage yamuiTabPage7;
        private YamuiTabPage pageFormAndNav;
        private YamuiTabControl yamuiTabControlSecSetting;
        private YamuiTabPage yamuiTabSecAppearance;
        private YamuiTabControl yamuiTabControl4;
        private YamuiTabPage yamuiTabPage10;
        private YamuiTabPage yamuiTabPage11;
        private YamuiPanel yamuiPanel3;
        private YamuiLabel yamuiLabel24;
        private YamuiLink yamuiLink6;
        private YamuiLabel yamuiLabel25;
        private YamuiLink yamuiLink7;
        private YamuiLink yamuiLink8;
        private YamuiLabel yamuiLabel19;
        private YamuiLabel yamuiLabel23;
        private YamuiTabPage yamuiTabPage12;
        private PageTemplate navigation1;
        private SettingAppearance settingAppearance1;
        private Classic classic1;
        private ItemControl itemControl1;
        private Text text1;
        private Progress progress1;
        private Other other1;
        private YamuiLabel statusLabel;


    }
}

