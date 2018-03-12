using System.ComponentModel;
using YamuiFramework.Controls;
using YamuiFramework.HtmlRenderer.WinForms;

namespace _3PA.MainFeatures.Appli.Pages.Home {
    partial class HomePage {
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
            this.html = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.SuspendLayout();
            // 
            // html
            // 
            this.html.AutoSize = false;
            this.html.AutoSizeHeightOnly = true;
            this.html.BackColor = System.Drawing.Color.Transparent;
            this.html.BaseStylesheet = null;
            this.html.Dock = System.Windows.Forms.DockStyle.Top;
            this.html.IsSelectionEnabled = false;
            this.html.Location = new System.Drawing.Point(0, 0);
            this.html.Name = "html";
            this.html.Size = new System.Drawing.Size(903, 15);
            this.html.TabIndex = 3;
            this.html.TabStop = false;
            this.html.Text = "home HTML";
            // 
            // HomePage
            // 
            this.Controls.Add(this.html);
            this.Name = "HomePage";
            this.Size = new System.Drawing.Size(903, 578);
            this.ResumeLayout(false);

        }

        #endregion
        
        private HtmlLabel html;
    }
}
