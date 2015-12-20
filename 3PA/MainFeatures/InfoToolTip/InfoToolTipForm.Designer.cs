using System.ComponentModel;
using YamuiFramework.HtmlRenderer.WinForms;

namespace _3PA.MainFeatures.InfoToolTip {
    partial class InfoToolTipForm {
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
            this.panel = new YamuiFramework.Controls.YamuiPanel();
            this.labelContent = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel
            // 
            this.panel.Controls.Add(this.labelContent);
            this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel.HorizontalScrollbarHighlightOnWheel = false;
            this.panel.HorizontalScrollbarSize = 10;
            this.panel.Location = new System.Drawing.Point(5, 5);
            this.panel.Margin = new System.Windows.Forms.Padding(5);
            this.panel.Name = "panel";
            this.panel.Padding = new System.Windows.Forms.Padding(5);
            this.panel.Size = new System.Drawing.Size(190, 190);
            this.panel.TabIndex = 1;
            this.panel.VerticalScrollbarHighlightOnWheel = false;
            this.panel.VerticalScrollbarSize = 10;
            // 
            // labelContent
            // 
            this.labelContent.AutoSize = false;
            this.labelContent.AutoSizeHeightOnly = true;
            this.labelContent.BackColor = System.Drawing.SystemColors.Window;
            this.labelContent.BaseStylesheet = null;
            this.labelContent.Location = new System.Drawing.Point(0, 0);
            this.labelContent.Name = "labelContent";
            this.labelContent.Size = new System.Drawing.Size(190, 15);
            this.labelContent.TabIndex = 2;
            this.labelContent.Text = "labelContent";
            // 
            // InfoToolTipForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(200, 200);
            this.Controls.Add(this.panel);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "InfoToolTipForm";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.Text = "";
            this.panel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiFramework.Controls.YamuiPanel panel;
        private HtmlLabel labelContent;


    }
}