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
            this.yamuiPanel1 = new YamuiFramework.Controls.YamuiPanel();
            this.cbSyntax = new YamuiFramework.Controls.YamuiComboBox();
            this.yamuiLabel1 = new YamuiFramework.Controls.YamuiLabel();
            this.comboTheme = new YamuiFramework.Controls.YamuiComboBox();
            this.PanelAccentColor = new YamuiFramework.Controls.YamuiPanel();
            this.yamuiLabel21 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLabel20 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // yamuiPanel1
            // 
            this.yamuiPanel1.Controls.Add(this.cbSyntax);
            this.yamuiPanel1.Controls.Add(this.yamuiLabel1);
            this.yamuiPanel1.Controls.Add(this.comboTheme);
            this.yamuiPanel1.Controls.Add(this.PanelAccentColor);
            this.yamuiPanel1.Controls.Add(this.yamuiLabel21);
            this.yamuiPanel1.Controls.Add(this.yamuiLabel20);
            this.yamuiPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.yamuiPanel1.HorizontalScrollbarHighlightOnWheel = false;
            this.yamuiPanel1.HorizontalScrollbarSize = 10;
            this.yamuiPanel1.Location = new System.Drawing.Point(0, 0);
            this.yamuiPanel1.Name = "yamuiPanel1";
            this.yamuiPanel1.Size = new System.Drawing.Size(720, 550);
            this.yamuiPanel1.TabIndex = 0;
            this.yamuiPanel1.VerticalScrollbarHighlightOnWheel = false;
            this.yamuiPanel1.VerticalScrollbarSize = 10;
            // 
            // cbSyntax
            // 
            this.cbSyntax.ItemHeight = 19;
            this.cbSyntax.Location = new System.Drawing.Point(0, 29);
            this.cbSyntax.Name = "cbSyntax";
            this.cbSyntax.Size = new System.Drawing.Size(180, 25);
            this.cbSyntax.TabIndex = 15;
            // 
            // yamuiLabel1
            // 
            this.yamuiLabel1.AutoSize = true;
            this.yamuiLabel1.Function = YamuiFramework.Fonts.LabelFunction.Heading;
            this.yamuiLabel1.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel1.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel1.Name = "yamuiLabel1";
            this.yamuiLabel1.Size = new System.Drawing.Size(169, 19);
            this.yamuiLabel1.TabIndex = 14;
            this.yamuiLabel1.Text = "SYNTAX HIGHLIGHTING";
            // 
            // comboTheme
            // 
            this.comboTheme.ItemHeight = 19;
            this.comboTheme.Location = new System.Drawing.Point(0, 104);
            this.comboTheme.Name = "comboTheme";
            this.comboTheme.Size = new System.Drawing.Size(180, 25);
            this.comboTheme.TabIndex = 13;
            // 
            // PanelAccentColor
            // 
            this.PanelAccentColor.HorizontalScrollbarHighlightOnWheel = false;
            this.PanelAccentColor.HorizontalScrollbarSize = 10;
            this.PanelAccentColor.Location = new System.Drawing.Point(0, 176);
            this.PanelAccentColor.Margin = new System.Windows.Forms.Padding(0);
            this.PanelAccentColor.Name = "PanelAccentColor";
            this.PanelAccentColor.Size = new System.Drawing.Size(809, 136);
            this.PanelAccentColor.TabIndex = 10;
            this.PanelAccentColor.VerticalScrollbarHighlightOnWheel = false;
            this.PanelAccentColor.VerticalScrollbarSize = 10;
            // 
            // yamuiLabel21
            // 
            this.yamuiLabel21.AutoSize = true;
            this.yamuiLabel21.Function = YamuiFramework.Fonts.LabelFunction.Heading;
            this.yamuiLabel21.Location = new System.Drawing.Point(0, 150);
            this.yamuiLabel21.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel21.Name = "yamuiLabel21";
            this.yamuiLabel21.Size = new System.Drawing.Size(114, 19);
            this.yamuiLabel21.TabIndex = 9;
            this.yamuiLabel21.Text = "ACCENT COLOR";
            // 
            // yamuiLabel20
            // 
            this.yamuiLabel20.AutoSize = true;
            this.yamuiLabel20.Function = YamuiFramework.Fonts.LabelFunction.Heading;
            this.yamuiLabel20.Location = new System.Drawing.Point(0, 75);
            this.yamuiLabel20.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel20.Name = "yamuiLabel20";
            this.yamuiLabel20.Size = new System.Drawing.Size(151, 19);
            this.yamuiLabel20.TabIndex = 8;
            this.yamuiLabel20.Text = "APPLICATION THEME";
            // 
            // SettingAppearance
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.yamuiPanel1);
            this.Name = "SettingAppearance";
            this.Size = new System.Drawing.Size(720, 550);
            this.yamuiPanel1.ResumeLayout(false);
            this.yamuiPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiPanel yamuiPanel1;
        private YamuiPanel PanelAccentColor;
        private YamuiLabel yamuiLabel21;
        private YamuiLabel yamuiLabel20;
        private YamuiComboBox comboTheme;
        private YamuiLabel yamuiLabel1;
        private YamuiComboBox cbSyntax;

    }
}
