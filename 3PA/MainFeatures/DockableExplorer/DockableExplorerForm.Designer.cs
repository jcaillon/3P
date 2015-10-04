namespace _3PA.MainFeatures.DockableExplorer {
    partial class DockableExplorerForm {
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
            this.panelBottom = new YamuiFramework.Controls.YamuiPanel();
            this.codeExplorer = new _3PA.MainFeatures.DockableExplorer.CodeExplorer();
            this.SuspendLayout();
            // 
            // panelBottom
            // 
            this.panelBottom.AutoScroll = true;
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.HorizontalScrollbar = true;
            this.panelBottom.HorizontalScrollbarHighlightOnWheel = false;
            this.panelBottom.HorizontalScrollbarSize = 10;
            this.panelBottom.Location = new System.Drawing.Point(1, 256);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(375, 47);
            this.panelBottom.TabIndex = 4;
            this.panelBottom.VerticalScrollbar = true;
            this.panelBottom.VerticalScrollbarHighlightOnWheel = false;
            this.panelBottom.VerticalScrollbarSize = 10;
            // 
            // codeExplorer
            // 
            this.codeExplorer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.codeExplorer.Location = new System.Drawing.Point(5, 4);
            this.codeExplorer.Name = "codeExplorer";
            this.codeExplorer.Size = new System.Drawing.Size(368, 246);
            this.codeExplorer.TabIndex = 5;
            // 
            // DockableExplorerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(377, 304);
            this.Controls.Add(this.codeExplorer);
            this.Controls.Add(this.panelBottom);
            this.Name = "DockableExplorerForm";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "DockableExplorerForm";
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiFramework.Controls.YamuiPanel panelBottom;
        private CodeExplorer codeExplorer;
    }
}