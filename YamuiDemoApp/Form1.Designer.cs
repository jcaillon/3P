using System.ComponentModel;
using YamuiDemoApp.Pages;
using YamuiDemoApp.Pages.control;
using YamuiDemoApp.Pages.Navigation;
using YamuiFramework.Controls;

namespace YamuiDemoApp {
    partial class Form1 {
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
            this.yamuiLabel19 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLabel23 = new YamuiFramework.Controls.YamuiLabel();
            this.SuspendLayout();
            // 
            // yamuiLabel19
            // 
            this.yamuiLabel19.AutoSize = true;
            this.yamuiLabel19.Enabled = false;
            this.yamuiLabel19.Function = YamuiFramework.Fonts.FontFunction.FormTitle;
            this.yamuiLabel19.Location = new System.Drawing.Point(8, 10);
            this.yamuiLabel19.Margin = new System.Windows.Forms.Padding(5);
            this.yamuiLabel19.Name = "yamuiLabel19";
            this.yamuiLabel19.Size = new System.Drawing.Size(354, 17);
            this.yamuiLabel19.TabIndex = 10;
            this.yamuiLabel19.Text = "Yet Another Modern U.I. Framework - Demo application";
            // 
            // yamuiLabel23
            // 
            this.yamuiLabel23.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel23.Name = "yamuiLabel23";
            this.yamuiLabel23.Size = new System.Drawing.Size(100, 23);
            this.yamuiLabel23.TabIndex = 11;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(806, 469);
            this.Controls.Add(this.yamuiLabel19);
            this.Controls.Add(this.yamuiLabel23);
            this.MinimumSize = new System.Drawing.Size(806, 469);
            this.Name = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private YamuiLabel yamuiLabel19;
        private YamuiLabel yamuiLabel23;


    }
}

