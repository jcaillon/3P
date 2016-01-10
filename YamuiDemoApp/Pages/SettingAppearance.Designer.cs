using System.ComponentModel;
using YamuiFramework.Controls;

namespace YamuiDemoApp.Pages {
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
            this.comboTheme = new YamuiFramework.Controls.YamuiComboBox();
            this.yamuiComboBox3 = new YamuiFramework.Controls.YamuiComboBox();
            this.yamuiLabel22 = new YamuiFramework.Controls.YamuiLabel();
            this.PanelAccentColor = new YamuiFramework.Controls.YamuiPanel();
            this.yamuiLabel21 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLabel20 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiScrollPage1.ContentPanel.SuspendLayout();
            this.yamuiScrollPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // yamuiScrollPage1
            // 
            // 
            // yamuiScrollPage1.ContentPanel
            // 
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.comboTheme);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiComboBox3);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiLabel22);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.PanelAccentColor);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiLabel21);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiLabel20);
            this.yamuiScrollPage1.ContentPanel.Location = new System.Drawing.Point(0, 0);
            this.yamuiScrollPage1.ContentPanel.Name = "ContentPanel";
            this.yamuiScrollPage1.ContentPanel.OwnerPage = this.yamuiScrollPage1;
            this.yamuiScrollPage1.ContentPanel.Size = new System.Drawing.Size(715, 315);
            this.yamuiScrollPage1.ContentPanel.TabIndex = 0;
            this.yamuiScrollPage1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.yamuiScrollPage1.Location = new System.Drawing.Point(0, 0);
            this.yamuiScrollPage1.Name = "yamuiScrollPage1";
            this.yamuiScrollPage1.Size = new System.Drawing.Size(715, 315);
            this.yamuiScrollPage1.TabIndex = 0;
            // 
            // comboTheme
            // 
            this.comboTheme.ItemHeight = 15;
            this.comboTheme.Location = new System.Drawing.Point(0, 29);
            this.comboTheme.Name = "comboTheme";
            this.comboTheme.Size = new System.Drawing.Size(180, 21);
            this.comboTheme.TabIndex = 19;
            // 
            // yamuiComboBox3
            // 
            this.yamuiComboBox3.ItemHeight = 15;
            this.yamuiComboBox3.Location = new System.Drawing.Point(0, 256);
            this.yamuiComboBox3.Name = "yamuiComboBox3";
            this.yamuiComboBox3.Size = new System.Drawing.Size(121, 21);
            this.yamuiComboBox3.TabIndex = 18;
            // 
            // yamuiLabel22
            // 
            this.yamuiLabel22.AutoSize = true;
            this.yamuiLabel22.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel22.Location = new System.Drawing.Point(0, 227);
            this.yamuiLabel22.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel22.Name = "yamuiLabel22";
            this.yamuiLabel22.Size = new System.Drawing.Size(78, 19);
            this.yamuiLabel22.TabIndex = 17;
            this.yamuiLabel22.Text = "FONT SIZE";
            // 
            // PanelAccentColor
            // 
            this.PanelAccentColor.Location = new System.Drawing.Point(0, 101);
            this.PanelAccentColor.Margin = new System.Windows.Forms.Padding(0);
            this.PanelAccentColor.Name = "PanelAccentColor";
            this.PanelAccentColor.Size = new System.Drawing.Size(715, 108);
            this.PanelAccentColor.TabIndex = 16;
            // 
            // yamuiLabel21
            // 
            this.yamuiLabel21.AutoSize = true;
            this.yamuiLabel21.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel21.Location = new System.Drawing.Point(0, 75);
            this.yamuiLabel21.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel21.Name = "yamuiLabel21";
            this.yamuiLabel21.Size = new System.Drawing.Size(114, 19);
            this.yamuiLabel21.TabIndex = 15;
            this.yamuiLabel21.Text = "ACCENT COLOR";
            // 
            // yamuiLabel20
            // 
            this.yamuiLabel20.AutoSize = true;
            this.yamuiLabel20.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel20.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel20.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel20.Name = "yamuiLabel20";
            this.yamuiLabel20.Size = new System.Drawing.Size(55, 19);
            this.yamuiLabel20.TabIndex = 14;
            this.yamuiLabel20.Text = "THEME";
            // 
            // SettingAppearance
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.yamuiScrollPage1);
            this.Name = "SettingAppearance";
            this.Size = new System.Drawing.Size(715, 315);
            this.yamuiScrollPage1.ContentPanel.ResumeLayout(false);
            this.yamuiScrollPage1.ContentPanel.PerformLayout();
            this.yamuiScrollPage1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiScrollPage yamuiScrollPage1;
        private YamuiComboBox comboTheme;
        private YamuiComboBox yamuiComboBox3;
        private YamuiLabel yamuiLabel22;
        private YamuiPanel PanelAccentColor;
        private YamuiLabel yamuiLabel21;
        private YamuiLabel yamuiLabel20;


    }
}
