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
            this.yamuiProgressSpinner3 = new YamuiFramework.Controls.YamuiProgressSpinner();
            this.yamuiProgressSpinner2 = new YamuiFramework.Controls.YamuiProgressSpinner();
            this.yamuiProgressSpinner1 = new YamuiFramework.Controls.YamuiProgressSpinner();
            this.yamuiLabel9 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiScrollBar1 = new YamuiFramework.Controls.YamuiScrollBar();
            this.yamuiLabel5 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiProgressBar3 = new YamuiFramework.Controls.YamuiProgressBar();
            this.yamuiProgressBar2 = new YamuiFramework.Controls.YamuiProgressBar();
            this.yamuiLabel4 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiProgressBar1 = new YamuiFramework.Controls.YamuiProgressBar();
            this.yamuiPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // yamuiPanel1
            // 
            this.yamuiPanel1.Controls.Add(this.yamuiProgressSpinner3);
            this.yamuiPanel1.Controls.Add(this.yamuiProgressSpinner1);
            this.yamuiPanel1.Controls.Add(this.yamuiProgressSpinner2);
            this.yamuiPanel1.Controls.Add(this.yamuiLabel4);
            this.yamuiPanel1.Controls.Add(this.yamuiLabel9);
            this.yamuiPanel1.Controls.Add(this.yamuiProgressBar1);
            this.yamuiPanel1.Controls.Add(this.yamuiScrollBar1);
            this.yamuiPanel1.Controls.Add(this.yamuiProgressBar2);
            this.yamuiPanel1.Controls.Add(this.yamuiLabel5);
            this.yamuiPanel1.Controls.Add(this.yamuiProgressBar3);
            this.yamuiPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.yamuiPanel1.HorizontalScrollbarHighlightOnWheel = false;
            this.yamuiPanel1.HorizontalScrollbarSize = 10;
            this.yamuiPanel1.Location = new System.Drawing.Point(0, 0);
            this.yamuiPanel1.Name = "yamuiPanel1";
            this.yamuiPanel1.Size = new System.Drawing.Size(715, 315);
            this.yamuiPanel1.TabIndex = 0;
            this.yamuiPanel1.VerticalScrollbarHighlightOnWheel = false;
            this.yamuiPanel1.VerticalScrollbarSize = 10;
            // 
            // yamuiProgressSpinner3
            // 
            this.yamuiProgressSpinner3.Location = new System.Drawing.Point(172, 207);
            this.yamuiProgressSpinner3.Maximum = 100;
            this.yamuiProgressSpinner3.Name = "yamuiProgressSpinner3";
            this.yamuiProgressSpinner3.Size = new System.Drawing.Size(80, 80);
            this.yamuiProgressSpinner3.Speed = 2F;
            this.yamuiProgressSpinner3.TabIndex = 21;
            this.yamuiProgressSpinner3.Value = 50;
            // 
            // yamuiProgressSpinner2
            // 
            this.yamuiProgressSpinner2.Backwards = true;
            this.yamuiProgressSpinner2.Location = new System.Drawing.Point(86, 207);
            this.yamuiProgressSpinner2.Maximum = 75;
            this.yamuiProgressSpinner2.Minimum = 25;
            this.yamuiProgressSpinner2.Name = "yamuiProgressSpinner2";
            this.yamuiProgressSpinner2.Size = new System.Drawing.Size(80, 80);
            this.yamuiProgressSpinner2.TabIndex = 20;
            this.yamuiProgressSpinner2.Value = 25;
            // 
            // yamuiProgressSpinner1
            // 
            this.yamuiProgressSpinner1.Location = new System.Drawing.Point(0, 207);
            this.yamuiProgressSpinner1.Maximum = 100;
            this.yamuiProgressSpinner1.Name = "yamuiProgressSpinner1";
            this.yamuiProgressSpinner1.Size = new System.Drawing.Size(80, 80);
            this.yamuiProgressSpinner1.TabIndex = 19;
            // 
            // yamuiLabel9
            // 
            this.yamuiLabel9.AutoSize = true;
            this.yamuiLabel9.Function = LabelFunction.Heading;
            this.yamuiLabel9.Location = new System.Drawing.Point(0, 185);
            this.yamuiLabel9.Name = "yamuiLabel9";
            this.yamuiLabel9.Size = new System.Drawing.Size(131, 19);
            this.yamuiLabel9.TabIndex = 18;
            this.yamuiLabel9.Text = "WAIT ANIMATION";
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
            // yamuiLabel5
            // 
            this.yamuiLabel5.AutoSize = true;
            this.yamuiLabel5.Function = LabelFunction.Heading;
            this.yamuiLabel5.Location = new System.Drawing.Point(0, 123);
            this.yamuiLabel5.Name = "yamuiLabel5";
            this.yamuiLabel5.Size = new System.Drawing.Size(92, 19);
            this.yamuiLabel5.TabIndex = 16;
            this.yamuiLabel5.Text = "SCROLL BAR";
            // 
            // yamuiProgressBar3
            // 
            this.yamuiProgressBar3.HideProgressText = false;
            this.yamuiProgressBar3.Location = new System.Drawing.Point(0, 80);
            this.yamuiProgressBar3.Name = "yamuiProgressBar3";
            this.yamuiProgressBar3.ProgressBarStyle = System.Windows.Forms.ProgressBarStyle.Blocks;
            this.yamuiProgressBar3.Size = new System.Drawing.Size(540, 23);
            this.yamuiProgressBar3.TabIndex = 15;
            // 
            // yamuiProgressBar2
            // 
            this.yamuiProgressBar2.Location = new System.Drawing.Point(0, 51);
            this.yamuiProgressBar2.Name = "yamuiProgressBar2";
            this.yamuiProgressBar2.ProgressBarStyle = System.Windows.Forms.ProgressBarStyle.Blocks;
            this.yamuiProgressBar2.Size = new System.Drawing.Size(540, 23);
            this.yamuiProgressBar2.TabIndex = 14;
            // 
            // yamuiLabel4
            // 
            this.yamuiLabel4.AutoSize = true;
            this.yamuiLabel4.Function = LabelFunction.Heading;
            this.yamuiLabel4.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel4.Name = "yamuiLabel4";
            this.yamuiLabel4.Size = new System.Drawing.Size(112, 19);
            this.yamuiLabel4.TabIndex = 13;
            this.yamuiLabel4.Text = "PROGRESS BAR";
            // 
            // yamuiProgressBar1
            // 
            this.yamuiProgressBar1.Location = new System.Drawing.Point(0, 22);
            this.yamuiProgressBar1.Name = "yamuiProgressBar1";
            this.yamuiProgressBar1.Size = new System.Drawing.Size(540, 23);
            this.yamuiProgressBar1.TabIndex = 12;
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
        private YamuiProgressSpinner yamuiProgressSpinner3;
        private YamuiProgressSpinner yamuiProgressSpinner1;
        private YamuiProgressSpinner yamuiProgressSpinner2;
        private YamuiLabel yamuiLabel4;
        private YamuiLabel yamuiLabel9;
        private YamuiProgressBar yamuiProgressBar1;
        private YamuiScrollBar yamuiScrollBar1;
        private YamuiProgressBar yamuiProgressBar2;
        private YamuiLabel yamuiLabel5;
        private YamuiProgressBar yamuiProgressBar3;
    }
}
