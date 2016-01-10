using System.ComponentModel;
using YamuiFramework.Controls;
using YamuiFramework.HtmlRenderer.WinForms;

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
            this.labelTitle = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.SuspendLayout();
            // 
            // labelTitle
            // 
            this.labelTitle.BackColor = System.Drawing.Color.Transparent;
            this.labelTitle.BaseStylesheet = null;
            this.labelTitle.Enabled = false;
            this.labelTitle.IsContextMenuEnabled = false;
            this.labelTitle.IsSelectionEnabled = false;
            this.labelTitle.Location = new System.Drawing.Point(11, 6);
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
            this.ClientSize = new System.Drawing.Size(950, 700);
            this.Controls.Add(this.labelTitle);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(950, 700);
            this.Name = "AppliForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private HtmlLabel labelTitle;


    }
}

