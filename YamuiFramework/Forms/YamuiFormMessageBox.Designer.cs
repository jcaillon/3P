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
            contentLabel.LinkClicked -= OnLinkClicked;
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.titleLabel = new YamuiFramework.Controls.YamuiLabel();
            this.contentPanel = new YamuiFramework.Controls.YamuiPanel();
            this.contentLabel = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.contentPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox.Location = new System.Drawing.Point(3, 3);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(50, 50);
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Function = YamuiFramework.Fonts.LabelFunction.Title;
            this.titleLabel.Location = new System.Drawing.Point(55, 16);
            this.titleLabel.Margin = new System.Windows.Forms.Padding(3);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(59, 25);
            this.titleLabel.TabIndex = 1;
            this.titleLabel.Text = "TITLE";
            // 
            // contentPanel
            // 
            this.contentPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.contentPanel.AutoScroll = true;
            this.contentPanel.Controls.Add(this.contentLabel);
            this.contentPanel.HorizontalScrollbar = true;
            this.contentPanel.HorizontalScrollbarHighlightOnWheel = false;
            this.contentPanel.HorizontalScrollbarSize = 10;
            this.contentPanel.Location = new System.Drawing.Point(8, 57);
            this.contentPanel.Name = "contentPanel";
            this.contentPanel.Size = new System.Drawing.Size(239, 88);
            this.contentPanel.TabIndex = 4;
            this.contentPanel.VerticalScrollbar = true;
            this.contentPanel.VerticalScrollbarHighlightOnWheel = false;
            this.contentPanel.VerticalScrollbarSize = 10;
            // 
            // contentLabel
            // 
            this.contentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.contentLabel.AutoSize = false;
            this.contentLabel.AutoSizeHeightOnly = true;
            this.contentLabel.BackColor = System.Drawing.Color.Transparent;
            this.contentLabel.BaseStylesheet = null;
            this.contentLabel.Location = new System.Drawing.Point(3, 3);
            this.contentLabel.Name = "contentLabel";
            this.contentLabel.Size = new System.Drawing.Size(233, 15);
            this.contentLabel.TabIndex = 3;
            this.contentLabel.Text = "htmlLabel1";
            // 
            // YamuiFormMessageBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(255, 188);
            this.Controls.Add(this.contentPanel);
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.pictureBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "YamuiFormMessageBox";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "YamuiFormMessageBox";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.contentPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private PictureBox pictureBox;
        private YamuiLabel titleLabel;
        private YamuiPanel contentPanel;
        private HtmlLabel contentLabel;


    }
}