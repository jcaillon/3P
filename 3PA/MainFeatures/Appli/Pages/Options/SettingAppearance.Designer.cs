using System.ComponentModel;
using YamuiFramework.Controls;
using YamuiFramework.HtmlRenderer.WinForms;

namespace _3PA.MainFeatures.Appli.Pages.Options {
    partial class SettingAppearance {
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.yamuiScrollPage1 = new YamuiFramework.Controls.YamuiScrollPage();
            this.linkurl = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.tg_override = new YamuiFramework.Controls.YamuiButtonToggle();
            this.tg_colorOn = new YamuiFramework.Controls.YamuiButtonToggle();
            this.htmlLabel1 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel7 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.cbSyntax = new YamuiFramework.Controls.YamuiComboBox();
            this.yamuiLabel1 = new YamuiFramework.Controls.YamuiLabel();
            this.cbApplication = new YamuiFramework.Controls.YamuiComboBox();
            this.PanelAccentColor = new YamuiFramework.Controls.YamuiPanel();
            this.yamuiLabel20 = new YamuiFramework.Controls.YamuiLabel();
            this.toolTip = new YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip();
            this.yamuiScrollPage1.ContentPanel.SuspendLayout();
            this.yamuiScrollPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // yamuiScrollPage1
            // 
            // 
            // yamuiScrollPage1.ContentPanel
            // 
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.linkurl);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.tg_override);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.tg_colorOn);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.htmlLabel1);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.htmlLabel7);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.cbSyntax);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiLabel1);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.cbApplication);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.PanelAccentColor);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiLabel20);
            this.yamuiScrollPage1.ContentPanel.Location = new System.Drawing.Point(0, 0);
            this.yamuiScrollPage1.ContentPanel.Name = "ContentPanel";
            this.yamuiScrollPage1.ContentPanel.OwnerPage = this.yamuiScrollPage1;
            this.yamuiScrollPage1.ContentPanel.Size = new System.Drawing.Size(720, 550);
            this.yamuiScrollPage1.ContentPanel.TabIndex = 0;
            this.yamuiScrollPage1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.yamuiScrollPage1.Location = new System.Drawing.Point(0, 0);
            this.yamuiScrollPage1.Name = "yamuiScrollPage1";
            this.yamuiScrollPage1.Size = new System.Drawing.Size(720, 550);
            this.yamuiScrollPage1.TabIndex = 0;
            // 
            // linkurl
            // 
            this.linkurl.BackColor = System.Drawing.Color.Transparent;
            this.linkurl.BaseStylesheet = null;
            this.linkurl.Location = new System.Drawing.Point(144, 2);
            this.linkurl.Name = "linkurl";
            this.linkurl.Size = new System.Drawing.Size(177, 15);
            this.linkurl.TabIndex = 65;
            this.linkurl.TabStop = false;
            this.linkurl.Text = "How to customize the look of 3P?";
            // 
            // tg_override
            // 
            this.tg_override.Checked = false;
            this.tg_override.Location = new System.Drawing.Point(25, 307);
            this.tg_override.Name = "tg_override";
            this.tg_override.Size = new System.Drawing.Size(692, 16);
            this.tg_override.TabIndex = 64;
            this.tg_override.Text = "Let 3P override notepad++ themes";
            this.tg_override.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.tg_override.ToggleSize = 30;
            // 
            // tg_colorOn
            // 
            this.tg_colorOn.Checked = false;
            this.tg_colorOn.Location = new System.Drawing.Point(25, 251);
            this.tg_colorOn.Name = "tg_colorOn";
            this.tg_colorOn.Size = new System.Drawing.Size(692, 16);
            this.tg_colorOn.TabIndex = 63;
            this.tg_colorOn.Text = "I\'m using my own User Defined Language";
            this.tg_colorOn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.tg_colorOn.ToggleSize = 30;
            // 
            // htmlLabel1
            // 
            this.htmlLabel1.AutoSize = false;
            this.htmlLabel1.AutoSizeHeightOnly = true;
            this.htmlLabel1.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel1.BaseStylesheet = null;
            this.htmlLabel1.Location = new System.Drawing.Point(25, 29);
            this.htmlLabel1.Name = "htmlLabel1";
            this.htmlLabel1.Size = new System.Drawing.Size(157, 15);
            this.htmlLabel1.TabIndex = 62;
            this.htmlLabel1.TabStop = false;
            this.htmlLabel1.Text = "Application theme";
            // 
            // htmlLabel7
            // 
            this.htmlLabel7.AutoSize = false;
            this.htmlLabel7.AutoSizeHeightOnly = true;
            this.htmlLabel7.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel7.BaseStylesheet = null;
            this.htmlLabel7.Location = new System.Drawing.Point(25, 86);
            this.htmlLabel7.Name = "htmlLabel7";
            this.htmlLabel7.Size = new System.Drawing.Size(157, 15);
            this.htmlLabel7.TabIndex = 61;
            this.htmlLabel7.TabStop = false;
            this.htmlLabel7.Text = "Accent color";
            // 
            // cbSyntax
            // 
            this.cbSyntax.ItemHeight = 15;
            this.cbSyntax.Location = new System.Drawing.Point(25, 273);
            this.cbSyntax.Name = "cbSyntax";
            this.cbSyntax.Size = new System.Drawing.Size(180, 21);
            this.cbSyntax.TabIndex = 21;
            // 
            // yamuiLabel1
            // 
            this.yamuiLabel1.AutoSize = true;
            this.yamuiLabel1.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel1.Location = new System.Drawing.Point(0, 222);
            this.yamuiLabel1.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel1.Name = "yamuiLabel1";
            this.yamuiLabel1.Size = new System.Drawing.Size(169, 19);
            this.yamuiLabel1.TabIndex = 20;
            this.yamuiLabel1.Text = "SYNTAX HIGHLIGHTING";
            // 
            // cbApplication
            // 
            this.cbApplication.ItemHeight = 15;
            this.cbApplication.Location = new System.Drawing.Point(25, 50);
            this.cbApplication.Name = "cbApplication";
            this.cbApplication.Size = new System.Drawing.Size(180, 21);
            this.cbApplication.TabIndex = 19;
            // 
            // PanelAccentColor
            // 
            this.PanelAccentColor.Location = new System.Drawing.Point(25, 104);
            this.PanelAccentColor.Margin = new System.Windows.Forms.Padding(0);
            this.PanelAccentColor.Name = "PanelAccentColor";
            this.PanelAccentColor.Size = new System.Drawing.Size(695, 100);
            this.PanelAccentColor.TabIndex = 18;
            // 
            // yamuiLabel20
            // 
            this.yamuiLabel20.AutoSize = true;
            this.yamuiLabel20.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel20.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel20.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel20.Name = "yamuiLabel20";
            this.yamuiLabel20.Size = new System.Drawing.Size(101, 19);
            this.yamuiLabel20.TabIndex = 16;
            this.yamuiLabel20.Text = "APPLICATION";
            // 
            // toolTip
            // 
            this.toolTip.AllowLinksHandling = true;
            this.toolTip.AutoPopDelay = 90000;
            this.toolTip.BaseStylesheet = null;
            this.toolTip.InitialDelay = 300;
            this.toolTip.MaximumSize = new System.Drawing.Size(0, 0);
            this.toolTip.OwnerDraw = true;
            this.toolTip.ReshowDelay = 100;
            this.toolTip.TooltipCssClass = "htmltooltip";
            // 
            // SettingAppearance
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.yamuiScrollPage1);
            this.Name = "SettingAppearance";
            this.Size = new System.Drawing.Size(720, 550);
            this.yamuiScrollPage1.ContentPanel.ResumeLayout(false);
            this.yamuiScrollPage1.ContentPanel.PerformLayout();
            this.yamuiScrollPage1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiScrollPage yamuiScrollPage1;
        private YamuiComboBox cbSyntax;
        private YamuiLabel yamuiLabel1;
        private YamuiComboBox cbApplication;
        private YamuiPanel PanelAccentColor;
        private YamuiLabel yamuiLabel20;
        private HtmlToolTip toolTip;
        private HtmlLabel htmlLabel7;
        private HtmlLabel htmlLabel1;
        private YamuiButtonToggle tg_colorOn;
        private YamuiButtonToggle tg_override;
        private HtmlLabel linkurl;
    }
}
