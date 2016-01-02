using System.ComponentModel;
using YamuiFramework.Controls;
using YamuiFramework.Fonts;

namespace YamuiDemoApp.Pages.control {
    partial class Progress {
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
            this.yamuiProgressIndicator2 = new YamuiFramework.Controls.YamuiProgressIndicator();
            this.yamuiProgressIndicator1 = new YamuiFramework.Controls.YamuiProgressIndicator();
            this.yamuiLabel4 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLabel9 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiProgressBar1 = new YamuiFramework.Controls.YamuiProgressBar();
            this.yamuiScrollBar1 = new YamuiFramework.Controls.YamuiScrollBar();
            this.yamuiProgressBar2 = new YamuiFramework.Controls.YamuiProgressBar();
            this.yamuiLabel5 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiProgressBar3 = new YamuiFramework.Controls.YamuiProgressBar();
            this.yamuiPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // yamuiPanel1
            // 
            this.yamuiPanel1.Controls.Add(this.yamuiProgressIndicator2);
            this.yamuiPanel1.Controls.Add(this.yamuiProgressIndicator1);
            this.yamuiPanel1.Controls.Add(this.yamuiLabel4);
            this.yamuiPanel1.Controls.Add(this.yamuiLabel9);
            this.yamuiPanel1.Controls.Add(this.yamuiProgressBar1);
            this.yamuiPanel1.Controls.Add(this.yamuiScrollBar1);
            this.yamuiPanel1.Controls.Add(this.yamuiProgressBar2);
            this.yamuiPanel1.Controls.Add(this.yamuiLabel5);
            this.yamuiPanel1.Controls.Add(this.yamuiProgressBar3);
            this.yamuiPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.yamuiPanel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.yamuiPanel1.HorizontalScrollbarHighlightOnWheel = false;
            this.yamuiPanel1.HorizontalScrollbarSize = 10;
            this.yamuiPanel1.Location = new System.Drawing.Point(0, 0);
            this.yamuiPanel1.Name = "yamuiPanel1";
            this.yamuiPanel1.Size = new System.Drawing.Size(715, 315);
            this.yamuiPanel1.TabIndex = 0;
            this.yamuiPanel1.VerticalScrollbarHighlightOnWheel = false;
            this.yamuiPanel1.VerticalScrollbarSize = 10;
            // 
            // yamuiProgressIndicator2
            // 
            this.yamuiProgressIndicator2.AnimateInterval = 300;
            this.yamuiProgressIndicator2.CircleCount = 9;
            this.yamuiProgressIndicator2.CircleDiameter = 5;
            this.yamuiProgressIndicator2.Location = new System.Drawing.Point(40, 225);
            this.yamuiProgressIndicator2.Name = "yamuiProgressIndicator2";
            this.yamuiProgressIndicator2.Size = new System.Drawing.Size(30, 30);
            this.yamuiProgressIndicator2.TabIndex = 20;
            this.yamuiProgressIndicator2.Text = "yamuiProgressIndicator2";
            // 
            // yamuiProgressIndicator1
            // 
            this.yamuiProgressIndicator1.AnimateInterval = 300;
            this.yamuiProgressIndicator1.BackColor = System.Drawing.Color.Transparent;
            this.yamuiProgressIndicator1.CircleCount = 6;
            this.yamuiProgressIndicator1.CircleDiameter = 15;
            this.yamuiProgressIndicator1.Location = new System.Drawing.Point(470, 213);
            this.yamuiProgressIndicator1.Name = "yamuiProgressIndicator1";
            this.yamuiProgressIndicator1.Size = new System.Drawing.Size(70, 70);
            this.yamuiProgressIndicator1.TabIndex = 19;
            this.yamuiProgressIndicator1.Text = "yamuiProgressIndicator1";
            // 
            // yamuiLabel4
            // 
            this.yamuiLabel4.AutoSize = true;
            this.yamuiLabel4.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel4.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel4.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel4.Name = "yamuiLabel4";
            this.yamuiLabel4.Size = new System.Drawing.Size(112, 19);
            this.yamuiLabel4.TabIndex = 13;
            this.yamuiLabel4.Text = "PROGRESS BAR";
            // 
            // yamuiLabel9
            // 
            this.yamuiLabel9.AutoSize = true;
            this.yamuiLabel9.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel9.Location = new System.Drawing.Point(0, 185);
            this.yamuiLabel9.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel9.Name = "yamuiLabel9";
            this.yamuiLabel9.Size = new System.Drawing.Size(131, 19);
            this.yamuiLabel9.TabIndex = 18;
            this.yamuiLabel9.Text = "WAIT ANIMATION";
            // 
            // yamuiProgressBar1
            // 
            this.yamuiProgressBar1.Location = new System.Drawing.Point(0, 22);
            this.yamuiProgressBar1.MarqueeWidth = 50;
            this.yamuiProgressBar1.Name = "yamuiProgressBar1";
            this.yamuiProgressBar1.Progress = 30F;
            this.yamuiProgressBar1.Size = new System.Drawing.Size(540, 23);
            this.yamuiProgressBar1.TabIndex = 12;
            // 
            // yamuiScrollBar1
            // 
            this.yamuiScrollBar1.LargeChange = 10;
            this.yamuiScrollBar1.Location = new System.Drawing.Point(0, 145);
            this.yamuiScrollBar1.Maximum = 100;
            this.yamuiScrollBar1.Minimum = 0;
            this.yamuiScrollBar1.MouseWheelBarPartitions = 10;
            this.yamuiScrollBar1.Name = "yamuiScrollBar1";
            this.yamuiScrollBar1.Orientation = YamuiFramework.Controls.ScrollOrientation.Horizontal;
            this.yamuiScrollBar1.ScrollbarSize = 10;
            this.yamuiScrollBar1.Size = new System.Drawing.Size(540, 10);
            this.yamuiScrollBar1.TabIndex = 17;
            // 
            // yamuiProgressBar2
            // 
            this.yamuiProgressBar2.Location = new System.Drawing.Point(0, 51);
            this.yamuiProgressBar2.MarqueeWidth = 50;
            this.yamuiProgressBar2.Name = "yamuiProgressBar2";
            this.yamuiProgressBar2.Progress = 0F;
            this.yamuiProgressBar2.Size = new System.Drawing.Size(540, 23);
            this.yamuiProgressBar2.TabIndex = 14;
            this.yamuiProgressBar2.UseMarquee = true;
            // 
            // yamuiLabel5
            // 
            this.yamuiLabel5.AutoSize = true;
            this.yamuiLabel5.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel5.Location = new System.Drawing.Point(0, 123);
            this.yamuiLabel5.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel5.Name = "yamuiLabel5";
            this.yamuiLabel5.Size = new System.Drawing.Size(92, 19);
            this.yamuiLabel5.TabIndex = 16;
            this.yamuiLabel5.Text = "SCROLL BAR";
            // 
            // yamuiProgressBar3
            // 
            this.yamuiProgressBar3.CenterText = YamuiFramework.Controls.CenterElement.Percent;
            this.yamuiProgressBar3.Location = new System.Drawing.Point(0, 80);
            this.yamuiProgressBar3.MarqueeWidth = 50;
            this.yamuiProgressBar3.Name = "yamuiProgressBar3";
            this.yamuiProgressBar3.Progress = 30F;
            this.yamuiProgressBar3.Size = new System.Drawing.Size(540, 23);
            this.yamuiProgressBar3.Style = YamuiFramework.Controls.ProgressStyle.Outwards;
            this.yamuiProgressBar3.TabIndex = 15;
            // 
            // Progress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.yamuiPanel1);
            this.Name = "Progress";
            this.Size = new System.Drawing.Size(715, 315);
            this.yamuiPanel1.ResumeLayout(false);
            this.yamuiPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiPanel yamuiPanel1;
        private YamuiLabel yamuiLabel4;
        private YamuiLabel yamuiLabel9;
        private YamuiProgressBar yamuiProgressBar1;
        private YamuiScrollBar yamuiScrollBar1;
        private YamuiProgressBar yamuiProgressBar2;
        private YamuiLabel yamuiLabel5;
        private YamuiProgressBar yamuiProgressBar3;
        private YamuiProgressIndicator yamuiProgressIndicator1;
        private YamuiProgressIndicator yamuiProgressIndicator2;
    }
}
