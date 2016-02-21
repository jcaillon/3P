using System.ComponentModel;
using YamuiFramework.Controls;

namespace _3PA.MainFeatures.Appli.Pages {
    partial class template {
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
            this.scrollPage = new YamuiFramework.Controls.YamuiScrollPage();
            this.scrollPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // scrollPage
            // 
            // 
            // scrollPage.ContentPanel
            // 
            this.scrollPage.ContentPanel.Location = new System.Drawing.Point(0, 0);
            this.scrollPage.ContentPanel.Name = "ContentPanel";
            this.scrollPage.ContentPanel.OwnerPage = this.scrollPage;
            this.scrollPage.ContentPanel.Size = new System.Drawing.Size(720, 550);
            this.scrollPage.ContentPanel.TabIndex = 0;
            this.scrollPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scrollPage.Location = new System.Drawing.Point(0, 0);
            this.scrollPage.Name = "scrollPage";
            this.scrollPage.Size = new System.Drawing.Size(720, 550);
            this.scrollPage.TabIndex = 0;
            // 
            // template
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.scrollPage);
            this.Name = "template";
            this.Size = new System.Drawing.Size(720, 550);
            this.scrollPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiScrollPage scrollPage;

    }
}
