using System.ComponentModel;
using YamuiFramework.Controls;

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
            this.cbSyntax = new YamuiFramework.Controls.YamuiComboBox();
            this.yamuiLabel1 = new YamuiFramework.Controls.YamuiLabel();
            this.comboTheme = new YamuiFramework.Controls.YamuiComboBox();
            this.PanelAccentColor = new YamuiFramework.Controls.YamuiPanel();
            this.yamuiLabel21 = new YamuiFramework.Controls.YamuiLabel();
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
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.cbSyntax);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiLabel1);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.comboTheme);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.PanelAccentColor);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiLabel21);
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
            // cbSyntax
            // 
            this.cbSyntax.ItemHeight = 15;
            this.cbSyntax.Location = new System.Drawing.Point(30, 29);
            this.cbSyntax.Name = "cbSyntax";
            this.cbSyntax.Size = new System.Drawing.Size(180, 21);
            this.cbSyntax.TabIndex = 21;
            // 
            // yamuiLabel1
            // 
            this.yamuiLabel1.AutoSize = true;
            this.yamuiLabel1.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel1.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel1.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel1.Name = "yamuiLabel1";
            this.yamuiLabel1.Size = new System.Drawing.Size(169, 19);
            this.yamuiLabel1.TabIndex = 20;
            this.yamuiLabel1.Text = "SYNTAX HIGHLIGHTING";
            // 
            // comboTheme
            // 
            this.comboTheme.ItemHeight = 15;
            this.comboTheme.Location = new System.Drawing.Point(30, 104);
            this.comboTheme.Name = "comboTheme";
            this.comboTheme.Size = new System.Drawing.Size(180, 21);
            this.comboTheme.TabIndex = 19;
            // 
            // PanelAccentColor
            // 
            this.PanelAccentColor.Location = new System.Drawing.Point(30, 176);
            this.PanelAccentColor.Margin = new System.Windows.Forms.Padding(0);
            this.PanelAccentColor.Name = "PanelAccentColor";
            this.PanelAccentColor.Size = new System.Drawing.Size(690, 136);
            this.PanelAccentColor.TabIndex = 18;
            // 
            // yamuiLabel21
            // 
            this.yamuiLabel21.AutoSize = true;
            this.yamuiLabel21.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel21.Location = new System.Drawing.Point(0, 150);
            this.yamuiLabel21.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel21.Name = "yamuiLabel21";
            this.yamuiLabel21.Size = new System.Drawing.Size(114, 19);
            this.yamuiLabel21.TabIndex = 17;
            this.yamuiLabel21.Text = "ACCENT COLOR";
            // 
            // yamuiLabel20
            // 
            this.yamuiLabel20.AutoSize = true;
            this.yamuiLabel20.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel20.Location = new System.Drawing.Point(0, 75);
            this.yamuiLabel20.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel20.Name = "yamuiLabel20";
            this.yamuiLabel20.Size = new System.Drawing.Size(151, 19);
            this.yamuiLabel20.TabIndex = 16;
            this.yamuiLabel20.Text = "APPLICATION THEME";
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
        private YamuiComboBox comboTheme;
        private YamuiPanel PanelAccentColor;
        private YamuiLabel yamuiLabel21;
        private YamuiLabel yamuiLabel20;
        private YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip toolTip;
    }
}
