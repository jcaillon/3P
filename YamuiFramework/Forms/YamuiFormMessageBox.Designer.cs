using System.ComponentModel;
using System.Windows.Forms;
using YamuiFramework.Controls;
using YamuiFramework.HtmlRenderer.WinForms;

namespace YamuiFramework.Forms {
    partial class YamuiFormMessageBox {
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
            this.contentPanel = new YamuiFramework.Controls.YamuiScrollPanel();
            this.contentLabel = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.contentPanel.ContentPanel.SuspendLayout();
            this.contentPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // contentPanel
            // 
            // 
            // contentPanel.ContentPanel
            // 
            this.contentPanel.ContentPanel.Controls.Add(this.contentLabel);
            this.contentPanel.ContentPanel.Location = new System.Drawing.Point(0, 0);
            this.contentPanel.ContentPanel.Name = "ContentPanel";
            this.contentPanel.ContentPanel.OwnerPanel = this.contentPanel;
            this.contentPanel.ContentPanel.Size = new System.Drawing.Size(245, 113);
            this.contentPanel.ContentPanel.TabIndex = 0;
            this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentPanel.Location = new System.Drawing.Point(5, 25);
            this.contentPanel.Name = "contentPanel";
            this.contentPanel.Size = new System.Drawing.Size(245, 113);
            this.contentPanel.TabIndex = 4;
            // 
            // contentLabel
            // 
            this.contentLabel.AutoSize = false;
            this.contentLabel.AutoSizeHeightOnly = true;
            this.contentLabel.BackColor = System.Drawing.Color.Transparent;
            this.contentLabel.BaseStylesheet = null;
            this.contentLabel.Location = new System.Drawing.Point(0, 0);
            this.contentLabel.Name = "contentLabel";
            this.contentLabel.Size = new System.Drawing.Size(245, 15);
            this.contentLabel.TabIndex = 4;
            this.contentLabel.TabStop = false;
            this.contentLabel.Text = "htmlLabel1";
            // 
            // YamuiFormMessageBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(255, 188);
            this.Controls.Add(this.contentPanel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "YamuiFormMessageBox";
            this.Padding = new System.Windows.Forms.Padding(5, 25, 5, 50);
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "YamuiFormMessageBox";
            this.contentPanel.ContentPanel.ResumeLayout(false);
            this.contentPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiScrollPanel contentPanel;
        private HtmlLabel contentLabel;



    }
}