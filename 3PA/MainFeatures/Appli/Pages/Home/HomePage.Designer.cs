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
            this.yamuiPanel1 = new YamuiFramework.Controls.YamuiPanel();
            this.html = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.yamuiPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // yamuiPanel1
            // 
            this.yamuiPanel1.AutoScroll = true;
            this.yamuiPanel1.BackColor = System.Drawing.Color.Gray;
            this.yamuiPanel1.Controls.Add(this.html);
            this.yamuiPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.yamuiPanel1.HorizontalScrollbar = true;
            this.yamuiPanel1.HorizontalScrollbarHighlightOnWheel = false;
            this.yamuiPanel1.HorizontalScrollbarSize = 10;
            this.yamuiPanel1.Location = new System.Drawing.Point(0, 0);
            this.yamuiPanel1.Name = "yamuiPanel1";
            this.yamuiPanel1.Size = new System.Drawing.Size(750, 600);
            this.yamuiPanel1.TabIndex = 0;
            this.yamuiPanel1.VerticalScrollbar = true;
            this.yamuiPanel1.VerticalScrollbarHighlightOnWheel = true;
            this.yamuiPanel1.VerticalScrollbarSize = 10;
            // 
            // html
            // 
            this.html.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.html.AutoSize = false;
            this.html.AutoSizeHeightOnly = true;
            this.html.BackColor = System.Drawing.Color.Transparent;
            this.html.BaseStylesheet = null;
            this.html.IsSelectionEnabled = false;
            this.html.Location = new System.Drawing.Point(3, 3);
            this.html.Name = "html";
            this.html.Size = new System.Drawing.Size(744, 15);
            this.html.TabIndex = 2;
            this.html.Text = "home HTML";
            // 
            // HomePage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.yamuiPanel1);
            this.Name = "HomePage";
            this.Size = new System.Drawing.Size(750, 600);
            this.yamuiPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiPanel yamuiPanel1;
        private HtmlLabel html;
    }
}
