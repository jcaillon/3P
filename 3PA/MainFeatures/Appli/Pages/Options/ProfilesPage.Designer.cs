using System.ComponentModel;
using YamuiFramework.Controls;

namespace _3PA.MainFeatures.Appli.Pages.Options {
    partial class ProfilesPage {
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
            this.yamuiLabel1 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLabel2 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLabel3 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // yamuiPanel1
            // 
            this.yamuiPanel1.Controls.Add(this.yamuiLabel3);
            this.yamuiPanel1.Controls.Add(this.yamuiLabel2);
            this.yamuiPanel1.Controls.Add(this.yamuiLabel1);
            this.yamuiPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.yamuiPanel1.HorizontalScrollbarHighlightOnWheel = false;
            this.yamuiPanel1.HorizontalScrollbarSize = 10;
            this.yamuiPanel1.Location = new System.Drawing.Point(0, 0);
            this.yamuiPanel1.Name = "yamuiPanel1";
            this.yamuiPanel1.Size = new System.Drawing.Size(720, 550);
            this.yamuiPanel1.TabIndex = 0;
            this.yamuiPanel1.VerticalScrollbarHighlightOnWheel = false;
            this.yamuiPanel1.VerticalScrollbarSize = 10;
            // 
            // yamuiLabel1
            // 
            this.yamuiLabel1.AutoSize = true;
            this.yamuiLabel1.Function = YamuiFramework.Fonts.LabelFunction.Heading;
            this.yamuiLabel1.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel1.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel1.Name = "yamuiLabel1";
            this.yamuiLabel1.Size = new System.Drawing.Size(133, 19);
            this.yamuiLabel1.TabIndex = 2;
            this.yamuiLabel1.Text = "SET YOUR PROFILE";
            // 
            // yamuiLabel2
            // 
            this.yamuiLabel2.AutoSize = true;
            this.yamuiLabel2.Function = YamuiFramework.Fonts.LabelFunction.Heading;
            this.yamuiLabel2.Location = new System.Drawing.Point(0, 59);
            this.yamuiLabel2.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel2.Name = "yamuiLabel2";
            this.yamuiLabel2.Size = new System.Drawing.Size(225, 19);
            this.yamuiLabel2.TabIndex = 3;
            this.yamuiLabel2.Text = "EXTERNALISED CONFIGURATION";
            // 
            // yamuiLabel3
            // 
            this.yamuiLabel3.AutoSize = true;
            this.yamuiLabel3.Function = YamuiFramework.Fonts.LabelFunction.Heading;
            this.yamuiLabel3.Location = new System.Drawing.Point(0, 131);
            this.yamuiLabel3.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel3.Name = "yamuiLabel3";
            this.yamuiLabel3.Size = new System.Drawing.Size(183, 19);
            this.yamuiLabel3.TabIndex = 4;
            this.yamuiLabel3.Text = "SHARED CONFIGURATION";
            // 
            // ProfilesPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.yamuiPanel1);
            this.Name = "ProfilesPage";
            this.Size = new System.Drawing.Size(720, 550);
            this.yamuiPanel1.ResumeLayout(false);
            this.yamuiPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiPanel yamuiPanel1;
        private YamuiLabel yamuiLabel3;
        private YamuiLabel yamuiLabel2;
        private YamuiLabel yamuiLabel1;
    }
}
