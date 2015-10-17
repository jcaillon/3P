using System.ComponentModel;
using YamuiFramework.Controls;
using _3PA.MainFeatures.Appli.Pages;

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
            this.yamuiPanel3 = new YamuiFramework.Controls.YamuiPanel();
            this.yamuiLabel24 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLink6 = new YamuiFramework.Controls.YamuiLink();
            this.yamuiLabel25 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLink7 = new YamuiFramework.Controls.YamuiLink();
            this.yamuiLink8 = new YamuiFramework.Controls.YamuiLink();
            this.statusLabel = new YamuiFramework.Controls.YamuiLabel();
            this.labelTitle = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.yamuiPanel3.SuspendLayout();
            this.SuspendLayout();
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
            this.yamuiPanel3.Location = new System.Drawing.Point(446, 0);
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
            // statusLabel
            // 
            this.statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.statusLabel.Location = new System.Drawing.Point(1, 580);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(760, 17);
            this.statusLabel.TabIndex = 12;
            this.statusLabel.Text = "yamuiLabel1";
            // 
            // labelTitle
            // 
            this.labelTitle.BackColor = System.Drawing.Color.Transparent;
            this.labelTitle.BaseStylesheet = null;
            this.labelTitle.Location = new System.Drawing.Point(5, 4);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(59, 15);
            this.labelTitle.TabIndex = 13;
            this.labelTitle.TabStop = false;
            this.labelTitle.Text = "htmlLabel1";
            // 
            // AppliForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 600);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.yamuiPanel3);
            this.IsMainForm = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(700, 600);
            this.Name = "AppliForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.yamuiPanel3.ResumeLayout(false);
            this.yamuiPanel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private YamuiPanel yamuiPanel3;
        private YamuiLabel yamuiLabel24;
        private YamuiLink yamuiLink6;
        private YamuiLabel yamuiLabel25;
        private YamuiLink yamuiLink7;
        private YamuiLink yamuiLink8;
        private YamuiLabel statusLabel;
        private YamuiFramework.HtmlRenderer.WinForms.HtmlLabel labelTitle;


    }
}

