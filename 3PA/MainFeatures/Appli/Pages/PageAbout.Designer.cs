using System.ComponentModel;
using YamuiFramework.Controls;

namespace _3PA.MainFeatures.Appli.Pages {
    partial class PageAbout {
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
            this.aboutHtml = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.yamuiPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // yamuiPanel1
            // 
            this.yamuiPanel1.BackColor = System.Drawing.Color.Gray;
            this.yamuiPanel1.Controls.Add(this.aboutHtml);
            this.yamuiPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.yamuiPanel1.HorizontalScrollbarHighlightOnWheel = false;
            this.yamuiPanel1.HorizontalScrollbarSize = 10;
            this.yamuiPanel1.Location = new System.Drawing.Point(0, 0);
            this.yamuiPanel1.Name = "yamuiPanel1";
            this.yamuiPanel1.Size = new System.Drawing.Size(750, 600);
            this.yamuiPanel1.TabIndex = 0;
            this.yamuiPanel1.VerticalScrollbarHighlightOnWheel = false;
            this.yamuiPanel1.VerticalScrollbarSize = 10;
            // 
            // aboutHtml
            // 
            this.aboutHtml.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.aboutHtml.AutoSize = false;
            this.aboutHtml.AutoSizeHeightOnly = true;
            this.aboutHtml.BackColor = System.Drawing.Color.Transparent;
            this.aboutHtml.BaseStylesheet = null;
            this.aboutHtml.Location = new System.Drawing.Point(3, 3);
            this.aboutHtml.Name = "aboutHtml";
            this.aboutHtml.Size = new System.Drawing.Size(744, 15);
            this.aboutHtml.TabIndex = 2;
            this.aboutHtml.Text = "aboutHtml";
            // 
            // PageAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.yamuiPanel1);
            this.Name = "PageAbout";
            this.Size = new System.Drawing.Size(750, 600);
            this.yamuiPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiPanel yamuiPanel1;
        private YamuiFramework.HtmlRenderer.WinForms.HtmlLabel aboutHtml;
    }
}
