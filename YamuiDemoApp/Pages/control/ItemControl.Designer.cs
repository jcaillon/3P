using System.ComponentModel;
using YamuiFramework.Controls;

namespace YamuiDemoApp.Pages.control {
    partial class ItemControl {
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
            this.yamuiScrollPage1 = new YamuiFramework.Controls.YamuiScrollPage();
            this.yamuiComboBox2 = new YamuiFramework.Controls.YamuiComboBox();
            this.yamuiLabel11 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiComboBox1 = new YamuiFramework.Controls.YamuiComboBox();
            this.yamuiLabel7 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiTrackBar1 = new YamuiFramework.Controls.YamuiSlider();
            this.yamuiTrackBar2 = new YamuiFramework.Controls.YamuiSlider();
            this.yamuiScrollPage1.ContentPanel.SuspendLayout();
            this.yamuiScrollPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // yamuiScrollPage1
            // 
            // 
            // yamuiScrollPage1.ContentPanel
            // 
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiComboBox2);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiLabel11);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiComboBox1);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiLabel7);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiTrackBar1);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiTrackBar2);
            this.yamuiScrollPage1.ContentPanel.Location = new System.Drawing.Point(0, 0);
            this.yamuiScrollPage1.ContentPanel.Name = "ContentPanel";
            this.yamuiScrollPage1.ContentPanel.OwnerPage = this.yamuiScrollPage1;
            this.yamuiScrollPage1.ContentPanel.Size = new System.Drawing.Size(715, 315);
            this.yamuiScrollPage1.ContentPanel.TabIndex = 0;
            this.yamuiScrollPage1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.yamuiScrollPage1.Location = new System.Drawing.Point(0, 0);
            this.yamuiScrollPage1.Name = "yamuiScrollPage1";
            this.yamuiScrollPage1.Size = new System.Drawing.Size(715, 315);
            this.yamuiScrollPage1.TabIndex = 0;
            // 
            // yamuiComboBox2
            // 
            this.yamuiComboBox2.Enabled = false;
            this.yamuiComboBox2.ItemHeight = 15;
            this.yamuiComboBox2.Location = new System.Drawing.Point(0, 140);
            this.yamuiComboBox2.Name = "yamuiComboBox2";
            this.yamuiComboBox2.Size = new System.Drawing.Size(121, 21);
            this.yamuiComboBox2.TabIndex = 27;
            // 
            // yamuiLabel11
            // 
            this.yamuiLabel11.AutoSize = true;
            this.yamuiLabel11.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel11.Location = new System.Drawing.Point(0, 87);
            this.yamuiLabel11.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel11.Name = "yamuiLabel11";
            this.yamuiLabel11.Size = new System.Drawing.Size(95, 19);
            this.yamuiLabel11.TabIndex = 25;
            this.yamuiLabel11.Text = "COMBO BOX";
            // 
            // yamuiComboBox1
            // 
            this.yamuiComboBox1.ItemHeight = 15;
            this.yamuiComboBox1.Items.AddRange(new object[] {
            "test1",
            "test2",
            "test1",
            "test2",
            "test1",
            "test2",
            "test1",
            "test20",
            "test21",
            "test22",
            "test1",
            "test2",
            "test1",
            "test2"});
            this.yamuiComboBox1.Location = new System.Drawing.Point(0, 109);
            this.yamuiComboBox1.Name = "yamuiComboBox1";
            this.yamuiComboBox1.Size = new System.Drawing.Size(121, 21);
            this.yamuiComboBox1.TabIndex = 26;
            this.yamuiComboBox1.WaterMark = "Water mark !";
            // 
            // yamuiLabel7
            // 
            this.yamuiLabel7.AutoSize = true;
            this.yamuiLabel7.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel7.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel7.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel7.Name = "yamuiLabel7";
            this.yamuiLabel7.Size = new System.Drawing.Size(54, 19);
            this.yamuiLabel7.TabIndex = 22;
            this.yamuiLabel7.Text = "SLIDER";
            // 
            // yamuiTrackBar1
            // 
            this.yamuiTrackBar1.Location = new System.Drawing.Point(0, 22);
            this.yamuiTrackBar1.Name = "yamuiTrackBar1";
            this.yamuiTrackBar1.Size = new System.Drawing.Size(191, 23);
            this.yamuiTrackBar1.TabIndex = 23;
            this.yamuiTrackBar1.Text = "yamuiTrackBar1";
            // 
            // yamuiTrackBar2
            // 
            this.yamuiTrackBar2.Enabled = false;
            this.yamuiTrackBar2.Location = new System.Drawing.Point(0, 51);
            this.yamuiTrackBar2.Name = "yamuiTrackBar2";
            this.yamuiTrackBar2.Size = new System.Drawing.Size(191, 23);
            this.yamuiTrackBar2.TabIndex = 24;
            this.yamuiTrackBar2.Text = "yamuiTrackBar2";
            // 
            // ItemControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.yamuiScrollPage1);
            this.Name = "ItemControl";
            this.Size = new System.Drawing.Size(715, 315);
            this.yamuiScrollPage1.ContentPanel.ResumeLayout(false);
            this.yamuiScrollPage1.ContentPanel.PerformLayout();
            this.yamuiScrollPage1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiScrollPage yamuiScrollPage1;
        private YamuiComboBox yamuiComboBox2;
        private YamuiLabel yamuiLabel11;
        private YamuiComboBox yamuiComboBox1;
        private YamuiLabel yamuiLabel7;
        private YamuiSlider yamuiTrackBar1;
        private YamuiSlider yamuiTrackBar2;

    }
}
