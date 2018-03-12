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
            this.linkurl = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel1 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel7 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.cbSyntax = new YamuiFramework.Controls.YamuiComboBox();
            this.yamuiLabel1 = new YamuiFramework.Controls.YamuiLabel();
            this.cbApplication = new YamuiFramework.Controls.YamuiComboBox();
            this._simplePanelAccentColor = new YamuiFramework.Controls.YamuiSimplePanel();
            this.yamuiLabel20 = new YamuiFramework.Controls.YamuiLabel();
            this.toolTip = new YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip();
            this.SuspendLayout();
            // 
            // scrollPanel
            // 
            // 
            // scrollPanel.ContentPanel
            // 
            this.Controls.Add(this.linkurl);
            this.Controls.Add(this.htmlLabel1);
            this.Controls.Add(this.htmlLabel7);
            this.Controls.Add(this.cbSyntax);
            this.Controls.Add(this.yamuiLabel1);
            this.Controls.Add(this.cbApplication);
            this.Controls.Add(this._simplePanelAccentColor);
            this.Controls.Add(this.yamuiLabel20);
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
            // htmlLabel1
            // 
            this.htmlLabel1.AutoSize = false;
            this.htmlLabel1.AutoSizeHeightOnly = true;
            this.htmlLabel1.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel1.BaseStylesheet = null;
            this.htmlLabel1.IsSelectionEnabled = false;
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
            this.htmlLabel7.IsSelectionEnabled = false;
            this.htmlLabel7.Location = new System.Drawing.Point(25, 86);
            this.htmlLabel7.Name = "htmlLabel7";
            this.htmlLabel7.Size = new System.Drawing.Size(157, 15);
            this.htmlLabel7.TabIndex = 61;
            this.htmlLabel7.TabStop = false;
            this.htmlLabel7.Text = "Accent color";
            // 
            // cbSyntax
            // 
            this.cbSyntax.BackGrndImage = null;
            this.cbSyntax.GreyScaleBackGrndImage = null;
            this.cbSyntax.IsFocused = false;
            this.cbSyntax.IsHovered = false;
            this.cbSyntax.IsPressed = false;
            this.cbSyntax.Location = new System.Drawing.Point(25, 251);
            this.cbSyntax.Name = "cbSyntax";
            this.cbSyntax.SetImgSize = new System.Drawing.Size(0, 0);
            this.cbSyntax.Size = new System.Drawing.Size(277, 21);
            this.cbSyntax.TabIndex = 21;
            // 
            // yamuiLabel1
            // 
            this.yamuiLabel1.AutoSize = true;
            this.yamuiLabel1.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel1.Location = new System.Drawing.Point(0, 222);
            this.yamuiLabel1.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel1.Name = "yamuiLabel1";
            this.yamuiLabel1.Size = new System.Drawing.Size(168, 19);
            this.yamuiLabel1.TabIndex = 20;
            this.yamuiLabel1.Text = "SYNTAX HIGHLIGHTING";
            // 
            // cbApplication
            // 
            this.cbApplication.BackGrndImage = null;
            this.cbApplication.GreyScaleBackGrndImage = null;
            this.cbApplication.IsFocused = false;
            this.cbApplication.IsHovered = false;
            this.cbApplication.IsPressed = false;
            this.cbApplication.Location = new System.Drawing.Point(25, 50);
            this.cbApplication.Name = "cbApplication";
            this.cbApplication.SetImgSize = new System.Drawing.Size(0, 0);
            this.cbApplication.Size = new System.Drawing.Size(277, 21);
            this.cbApplication.TabIndex = 19;
            // 
            // _simplePanelAccentColor
            // 
            this._simplePanelAccentColor.Location = new System.Drawing.Point(25, 104);
            this._simplePanelAccentColor.Margin = new System.Windows.Forms.Padding(0);
            this._simplePanelAccentColor.Name = "_simplePanelAccentColor";
            this._simplePanelAccentColor.Size = new System.Drawing.Size(695, 100);
            this._simplePanelAccentColor.TabIndex = 18;
            // 
            // yamuiLabel20
            // 
            this.yamuiLabel20.AutoSize = true;
            this.yamuiLabel20.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel20.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel20.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel20.Name = "yamuiLabel20";
            this.yamuiLabel20.Size = new System.Drawing.Size(100, 19);
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
            // 
            // SettingAppearance
            // 
            this.Name = "SettingAppearance";
            this.Size = new System.Drawing.Size(900, 650);
            this.ResumeLayout(false);

        }

        #endregion
        
        private YamuiComboBox cbSyntax;
        private YamuiLabel yamuiLabel1;
        private YamuiComboBox cbApplication;
        private YamuiSimplePanel _simplePanelAccentColor;
        private YamuiLabel yamuiLabel20;
        private HtmlToolTip toolTip;
        private HtmlLabel htmlLabel7;
        private HtmlLabel htmlLabel1;
        private HtmlLabel linkurl;
    }
}
