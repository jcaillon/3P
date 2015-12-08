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
            this.labelContent = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.SuspendLayout();
            // 
            // labelContent
            // 
            this.labelContent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelContent.AutoSize = false;
            this.labelContent.AutoSizeHeightOnly = true;
            this.labelContent.BackColor = System.Drawing.Color.Transparent;
            this.labelContent.BaseStylesheet = null;
            this.labelContent.Location = new System.Drawing.Point(5, 5);
            this.labelContent.Name = "labelContent";
            this.labelContent.Size = new System.Drawing.Size(190, 15);
            this.labelContent.TabIndex = 0;
            this.labelContent.Text = "ok";
            // 
            // InfoToolTipForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(200, 200);
            this.Controls.Add(this.labelContent);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "InfoToolTipForm";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.Text = "";
            this.ResumeLayout(false);

        }

        #endregion

        private HtmlLabel labelContent;

    }
}