using System.ComponentModel;
using YamuiFramework.HtmlRenderer.WinForms;

namespace YamuiFramework.Forms {
    partial class YamuiNotifications {
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
            Load -= YamuiNotificationsLoad;
            Activated -= YamuiNotificationsActivated;
            Shown -= YamuiNotificationsShown;
            FormClosed -= YamuiNotificationsFormClosed;
            contentLabel.LinkClicked -= OnLinkClicked;
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.contentLabel = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.SuspendLayout();
            // 
            // contentLabel
            // 
            this.contentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.contentLabel.AutoSize = false;
            this.contentLabel.AutoSizeHeightOnly = true;
            this.contentLabel.BackColor = System.Drawing.Color.Transparent;
            this.contentLabel.BaseStylesheet = null;
            this.contentLabel.Location = new System.Drawing.Point(8, 8);
            this.contentLabel.Name = "contentLabel";
            this.contentLabel.Size = new System.Drawing.Size(221, 15);
            this.contentLabel.TabIndex = 0;
            this.contentLabel.Text = "htmlLabel1";
            // 
            // YamuiNotifications
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(256, 51);
            this.Controls.Add(this.contentLabel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Movable = false;
            this.Name = "YamuiNotifications";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.Resizable = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "YamuiNotifications";
            this.ResumeLayout(false);

        }

        #endregion

        private HtmlLabel contentLabel;
    }
}