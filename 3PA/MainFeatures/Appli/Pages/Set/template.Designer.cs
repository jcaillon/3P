using System.ComponentModel;
using YamuiFramework.Controls;

namespace _3PA.MainFeatures.Appli.Pages {
    partial class FuckingPage {
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
            this.yamuiButton1 = new YamuiFramework.Controls.YamuiButton();
            this.yamuiTextBox1 = new YamuiFramework.Controls.YamuiTextBox();
            this.yamuiPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // yamuiPanel1
            // 
            this.yamuiPanel1.Controls.Add(this.yamuiTextBox1);
            this.yamuiPanel1.Controls.Add(this.yamuiButton1);
            this.yamuiPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.yamuiPanel1.HorizontalScrollbarHighlightOnWheel = false;
            this.yamuiPanel1.HorizontalScrollbarSize = 10;
            this.yamuiPanel1.Location = new System.Drawing.Point(0, 0);
            this.yamuiPanel1.Name = "yamuiPanel1";
            this.yamuiPanel1.Size = new System.Drawing.Size(200, 39);
            this.yamuiPanel1.TabIndex = 0;
            this.yamuiPanel1.VerticalScrollbarHighlightOnWheel = false;
            this.yamuiPanel1.VerticalScrollbarSize = 10;
            // 
            // yamuiButton1
            // 
            this.yamuiButton1.Location = new System.Drawing.Point(4, 4);
            this.yamuiButton1.Name = "yamuiButton1";
            this.yamuiButton1.Size = new System.Drawing.Size(75, 23);
            this.yamuiButton1.TabIndex = 2;
            this.yamuiButton1.Text = "yamuiButton1";
            // 
            // yamuiTextBox1
            // 
            this.yamuiTextBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.yamuiTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.yamuiTextBox1.CustomBackColor = System.Drawing.Color.Empty;
            this.yamuiTextBox1.CustomForeColor = System.Drawing.Color.Empty;
            this.yamuiTextBox1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.yamuiTextBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.yamuiTextBox1.Location = new System.Drawing.Point(86, 4);
            this.yamuiTextBox1.Name = "yamuiTextBox1";
            this.yamuiTextBox1.Size = new System.Drawing.Size(100, 20);
            this.yamuiTextBox1.TabIndex = 3;
            this.yamuiTextBox1.Text = "yamuiTextBox1";
            this.yamuiTextBox1.WaterMark = null;
            // 
            // FuckingPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.yamuiPanel1);
            this.Name = "FuckingPage";
            this.Size = new System.Drawing.Size(200, 39);
            this.yamuiPanel1.ResumeLayout(false);
            this.yamuiPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiPanel yamuiPanel1;
        private YamuiButton yamuiButton1;
        private YamuiTextBox yamuiTextBox1;
    }
}
