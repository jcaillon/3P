using System.ComponentModel;

namespace _3PA.MainFeatures.FileExplorer {
    partial class FileExplorerForm {
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
            this._fileExplorerPage = new FileExplorerPage();
            this.SuspendLayout();
            // 
            // _fileExplorerPage
            // 
            this._fileExplorerPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this._fileExplorerPage.Location = new System.Drawing.Point(0, 0);
            this._fileExplorerPage.Name = "_fileExplorerPage";
            this._fileExplorerPage.Size = new System.Drawing.Size(242, 583);
            this._fileExplorerPage.TabIndex = 0;
            // 
            // FileExplorerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(242, 583);
            this.Controls.Add(this._fileExplorerPage);
            this.Name = "FileExplorerForm";
            this.Text = "FileExplorerForm";
            this.ResumeLayout(false);

        }

        #endregion

        private FileExplorerPage _fileExplorerPage;
    }
}