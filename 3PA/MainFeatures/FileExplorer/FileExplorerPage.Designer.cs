using System.ComponentModel;
using YamuiFramework.Controls;

namespace _3PA.MainFeatures.FileExplorer {
    partial class FileExplorerPage {
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
            this.yamuiLabel2 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiPanel1 = new YamuiFramework.Controls.YamuiPanel();
            this.yamuiLabel1 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiPanel2 = new YamuiFramework.Controls.YamuiPanel();
            this.yamuiPanel3 = new YamuiFramework.Controls.YamuiPanel();
            this.yamuiLabel3 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiPanel1.SuspendLayout();
            this.yamuiPanel2.SuspendLayout();
            this.yamuiPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // yamuiLabel2
            // 
            this.yamuiLabel2.AutoSize = true;
            this.yamuiLabel2.Function = YamuiFramework.Fonts.LabelFunction.Heading;
            this.yamuiLabel2.Location = new System.Drawing.Point(5, 3);
            this.yamuiLabel2.Margin = new System.Windows.Forms.Padding(5, 3, 5, 7);
            this.yamuiLabel2.Name = "yamuiLabel2";
            this.yamuiLabel2.Size = new System.Drawing.Size(58, 19);
            this.yamuiLabel2.TabIndex = 1;
            this.yamuiLabel2.Text = "Actions";
            // 
            // yamuiPanel1
            // 
            this.yamuiPanel1.Controls.Add(this.yamuiLabel1);
            this.yamuiPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.yamuiPanel1.HorizontalScrollbarHighlightOnWheel = false;
            this.yamuiPanel1.HorizontalScrollbarSize = 10;
            this.yamuiPanel1.Location = new System.Drawing.Point(0, 0);
            this.yamuiPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.yamuiPanel1.Name = "yamuiPanel1";
            this.yamuiPanel1.Size = new System.Drawing.Size(218, 82);
            this.yamuiPanel1.TabIndex = 2;
            this.yamuiPanel1.VerticalScrollbarHighlightOnWheel = false;
            this.yamuiPanel1.VerticalScrollbarSize = 10;
            // 
            // yamuiLabel1
            // 
            this.yamuiLabel1.AutoSize = true;
            this.yamuiLabel1.Function = YamuiFramework.Fonts.LabelFunction.Heading;
            this.yamuiLabel1.Location = new System.Drawing.Point(5, 3);
            this.yamuiLabel1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 7);
            this.yamuiLabel1.Name = "yamuiLabel1";
            this.yamuiLabel1.Size = new System.Drawing.Size(86, 19);
            this.yamuiLabel1.TabIndex = 2;
            this.yamuiLabel1.Text = "Current File";
            // 
            // yamuiPanel2
            // 
            this.yamuiPanel2.Controls.Add(this.yamuiLabel2);
            this.yamuiPanel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.yamuiPanel2.HorizontalScrollbarHighlightOnWheel = false;
            this.yamuiPanel2.HorizontalScrollbarSize = 10;
            this.yamuiPanel2.Location = new System.Drawing.Point(0, 82);
            this.yamuiPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.yamuiPanel2.Name = "yamuiPanel2";
            this.yamuiPanel2.Size = new System.Drawing.Size(218, 100);
            this.yamuiPanel2.TabIndex = 3;
            this.yamuiPanel2.VerticalScrollbarHighlightOnWheel = false;
            this.yamuiPanel2.VerticalScrollbarSize = 10;
            // 
            // yamuiPanel3
            // 
            this.yamuiPanel3.Controls.Add(this.yamuiLabel3);
            this.yamuiPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.yamuiPanel3.HorizontalScrollbarHighlightOnWheel = false;
            this.yamuiPanel3.HorizontalScrollbarSize = 10;
            this.yamuiPanel3.Location = new System.Drawing.Point(0, 182);
            this.yamuiPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.yamuiPanel3.Name = "yamuiPanel3";
            this.yamuiPanel3.Size = new System.Drawing.Size(218, 302);
            this.yamuiPanel3.TabIndex = 4;
            this.yamuiPanel3.VerticalScrollbarHighlightOnWheel = false;
            this.yamuiPanel3.VerticalScrollbarSize = 10;
            // 
            // yamuiLabel3
            // 
            this.yamuiLabel3.AutoSize = true;
            this.yamuiLabel3.Function = YamuiFramework.Fonts.LabelFunction.Heading;
            this.yamuiLabel3.Location = new System.Drawing.Point(5, 3);
            this.yamuiLabel3.Margin = new System.Windows.Forms.Padding(5, 3, 5, 7);
            this.yamuiLabel3.Name = "yamuiLabel3";
            this.yamuiLabel3.Size = new System.Drawing.Size(93, 19);
            this.yamuiLabel3.TabIndex = 1;
            this.yamuiLabel3.Text = "File Explorer";
            // 
            // FileExplorerPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.yamuiPanel3);
            this.Controls.Add(this.yamuiPanel2);
            this.Controls.Add(this.yamuiPanel1);
            this.Name = "FileExplorerPage";
            this.Size = new System.Drawing.Size(218, 484);
            this.yamuiPanel1.ResumeLayout(false);
            this.yamuiPanel1.PerformLayout();
            this.yamuiPanel2.ResumeLayout(false);
            this.yamuiPanel2.PerformLayout();
            this.yamuiPanel3.ResumeLayout(false);
            this.yamuiPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiLabel yamuiLabel2;
        private YamuiPanel yamuiPanel1;
        private YamuiLabel yamuiLabel1;
        private YamuiPanel yamuiPanel2;
        private YamuiPanel yamuiPanel3;
        private YamuiLabel yamuiLabel3;

    }
}
