namespace _3PA.MainFeatures.FilesInfo {
    partial class FileTagsForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.fileTagsPage1 = new FileTagsPage();
            this.lblTitle = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.SuspendLayout();
            // 
            // fileTagsPage1
            // 
            this.fileTagsPage1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileTagsPage1.Location = new System.Drawing.Point(10, 60);
            this.fileTagsPage1.Name = "fileTagsPage1";
            this.fileTagsPage1.Size = new System.Drawing.Size(329, 398);
            this.fileTagsPage1.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTitle.AutoSize = false;
            this.lblTitle.AutoSizeHeightOnly = true;
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.BaseStylesheet = null;
            this.lblTitle.Enabled = false;
            this.lblTitle.Location = new System.Drawing.Point(16, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(281, 15);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "lblTitle";
            // 
            // FileTagsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(349, 468);
            this.ControlBox = true;
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.fileTagsPage1);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "FileTagsForm";
            this.Padding = new System.Windows.Forms.Padding(10, 60, 10, 10);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FileTagsForm";
            this.ResumeLayout(false);

        }

        #endregion

        private FileTagsPage fileTagsPage1;
        private YamuiFramework.HtmlRenderer.WinForms.HtmlLabel lblTitle;
    }
}