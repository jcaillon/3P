using System.ComponentModel;

namespace _3PA.MainFeatures.CodeExplorer {
    partial class CodeExplorerForm {
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
            this._codeExplorerPage = new CodeExplorerPage();
            this.SuspendLayout();
            // 
            // _codeExplorerPage
            // 
            this._codeExplorerPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this._codeExplorerPage.Location = new System.Drawing.Point(1, 1);
            this._codeExplorerPage.Name = "_codeExplorerPage";
            this._codeExplorerPage.Size = new System.Drawing.Size(375, 302);
            this._codeExplorerPage.TabIndex = 5;
            // 
            // DockableExplorerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(377, 304);
            this.Controls.Add(this._codeExplorerPage);
            this.Name = "DockableExplorerForm";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "DockableExplorerForm";
            this.ResumeLayout(false);

        }

        #endregion

        private CodeExplorerPage _codeExplorerPage;
    }
}