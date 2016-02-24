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
            this.yamuiScrollPage1 = new YamuiFramework.Controls.YamuiScrollPage();
            this.html = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.yamuiTextBox1 = new YamuiFramework.Controls.YamuiTextBox();
            this.yamuiScrollPage1.ContentPanel.SuspendLayout();
            this.yamuiScrollPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // yamuiScrollPage1
            // 
            // 
            // yamuiScrollPage1.ContentPanel
            // 
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.html);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiTextBox1);
            this.yamuiScrollPage1.ContentPanel.Location = new System.Drawing.Point(0, 0);
            this.yamuiScrollPage1.ContentPanel.Name = "ContentPanel";
            this.yamuiScrollPage1.ContentPanel.OwnerPage = this.yamuiScrollPage1;
            this.yamuiScrollPage1.ContentPanel.Size = new System.Drawing.Size(750, 600);
            this.yamuiScrollPage1.ContentPanel.TabIndex = 0;
            this.yamuiScrollPage1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.yamuiScrollPage1.Location = new System.Drawing.Point(0, 0);
            this.yamuiScrollPage1.Name = "yamuiScrollPage1";
            this.yamuiScrollPage1.Size = new System.Drawing.Size(750, 600);
            this.yamuiScrollPage1.TabIndex = 0;
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
            this.html.Location = new System.Drawing.Point(0, 0);
            this.html.Name = "html";
            this.html.Size = new System.Drawing.Size(744, 15);
            this.html.TabIndex = 3;
            this.html.TabStop = false;
            this.html.Text = "home HTML";
            // 
            // yamuiTextBox1
            // 
            this.yamuiTextBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.yamuiTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.yamuiTextBox1.CustomBackColor = System.Drawing.Color.Empty;
            this.yamuiTextBox1.CustomForeColor = System.Drawing.Color.Empty;
            this.yamuiTextBox1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.yamuiTextBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.yamuiTextBox1.Location = new System.Drawing.Point(3, 3);
            this.yamuiTextBox1.Name = "yamuiTextBox1";
            this.yamuiTextBox1.Size = new System.Drawing.Size(20, 24);
            this.yamuiTextBox1.TabIndex = 4;
            this.yamuiTextBox1.WaterMark = null;
            // 
            // HomePage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.yamuiScrollPage1);
            this.Name = "HomePage";
            this.Size = new System.Drawing.Size(750, 600);
            this.yamuiScrollPage1.ContentPanel.ResumeLayout(false);
            this.yamuiScrollPage1.ContentPanel.PerformLayout();
            this.yamuiScrollPage1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiScrollPage yamuiScrollPage1;
        private HtmlLabel html;
        private YamuiTextBox yamuiTextBox1;
    }
}
