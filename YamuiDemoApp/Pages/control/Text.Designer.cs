using System.ComponentModel;
using YamuiFramework.Controls;
using YamuiFramework.HtmlRenderer.WinForms;

namespace YamuiDemoApp.Pages.control {
    partial class Text {
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
            this.htmlToolTip1 = new YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip();
            this.yamuiTextBox1 = new YamuiFramework.Controls.YamuiTextBox();
            this.yamuiLabel18 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiScrollPage1 = new YamuiFramework.Controls.YamuiScrollPage();
            this.htmlPanel1 = new YamuiFramework.HtmlRenderer.WinForms.HtmlPanel();
            this.htmlLabel1 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.yamuiLabel15 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLink1 = new YamuiFramework.Controls.YamuiLink();
            this.yamuiLink2 = new YamuiFramework.Controls.YamuiLink();
            this.yamuiLabel12 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLabel17 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiTextBox2 = new YamuiFramework.Controls.YamuiTextBox();
            this.yamuiLabel16 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiTextBox3 = new YamuiFramework.Controls.YamuiTextBox();
            this.yamuiLabel14 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiTextBox4 = new YamuiFramework.Controls.YamuiTextBox();
            this.yamuiLabel13 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiTextBox5 = new YamuiFramework.Controls.YamuiTextBox();
            this.yamuiScrollPage1.ContentPanel.SuspendLayout();
            this.yamuiScrollPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // htmlToolTip1
            // 
            this.htmlToolTip1.AllowLinksHandling = true;
            this.htmlToolTip1.AutoPopDelay = 90000;
            this.htmlToolTip1.BaseStylesheet = null;
            this.htmlToolTip1.InitialDelay = 500;
            this.htmlToolTip1.MaximumSize = new System.Drawing.Size(0, 0);
            this.htmlToolTip1.OwnerDraw = true;
            this.htmlToolTip1.ReshowDelay = 100;
            this.htmlToolTip1.TooltipCssClass = "htmltooltip";
            // 
            // yamuiTextBox1
            // 
            this.yamuiTextBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.yamuiTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.yamuiTextBox1.CustomBackColor = System.Drawing.Color.Empty;
            this.yamuiTextBox1.CustomForeColor = System.Drawing.Color.Empty;
            this.yamuiTextBox1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.yamuiTextBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.yamuiTextBox1.Location = new System.Drawing.Point(0, 22);
            this.yamuiTextBox1.Name = "yamuiTextBox1";
            this.yamuiTextBox1.Size = new System.Drawing.Size(157, 23);
            this.yamuiTextBox1.TabIndex = 34;
            this.htmlToolTip1.SetToolTip(this.yamuiTextBox1, "cool! <b>html?</b>");
            this.yamuiTextBox1.WaterMark = "Watermark!";
            // 
            // yamuiLabel18
            // 
            this.yamuiLabel18.AutoSize = true;
            this.yamuiLabel18.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel18.Location = new System.Drawing.Point(203, 100);
            this.yamuiLabel18.Margin = new System.Windows.Forms.Padding(20, 5, 5, 7);
            this.yamuiLabel18.Name = "yamuiLabel18";
            this.yamuiLabel18.Size = new System.Drawing.Size(40, 19);
            this.yamuiLabel18.TabIndex = 43;
            this.yamuiLabel18.Text = "LINK";
            this.htmlToolTip1.SetToolTip(this.yamuiLabel18, "test?");
            // 
            // yamuiScrollPage1
            // 
            // 
            // yamuiScrollPage1.ContentPanel
            // 
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.htmlPanel1);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.htmlLabel1);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiLabel15);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiLink1);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiLink2);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiLabel12);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiLabel18);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiTextBox1);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiLabel17);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiTextBox2);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiLabel16);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiTextBox3);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiLabel14);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiTextBox4);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiLabel13);
            this.yamuiScrollPage1.ContentPanel.Controls.Add(this.yamuiTextBox5);
            this.yamuiScrollPage1.ContentPanel.Location = new System.Drawing.Point(0, 0);
            this.yamuiScrollPage1.ContentPanel.Name = "ContentPanel";
            this.yamuiScrollPage1.ContentPanel.OwnerPage = this.yamuiScrollPage1;
            this.yamuiScrollPage1.ContentPanel.Size = new System.Drawing.Size(709, 327);
            this.yamuiScrollPage1.ContentPanel.TabIndex = 0;
            this.yamuiScrollPage1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.yamuiScrollPage1.Location = new System.Drawing.Point(0, 0);
            this.yamuiScrollPage1.Name = "yamuiScrollPage1";
            this.yamuiScrollPage1.Size = new System.Drawing.Size(709, 327);
            this.yamuiScrollPage1.TabIndex = 0;
            // 
            // htmlPanel1
            // 
            this.htmlPanel1.AutoScroll = true;
            this.htmlPanel1.AutoScrollMinSize = new System.Drawing.Size(338, 30);
            this.htmlPanel1.BackColor = System.Drawing.Color.Transparent;
            this.htmlPanel1.BaseStylesheet = null;
            this.htmlPanel1.Location = new System.Drawing.Point(347, 80);
            this.htmlPanel1.Name = "htmlPanel1";
            this.htmlPanel1.Size = new System.Drawing.Size(338, 224);
            this.htmlPanel1.TabIndex = 48;
            this.htmlPanel1.Text = "htmlPanel1<br>I\'mTesting <b>bold</b> and <i>Italic?</i>";
            // 
            // htmlLabel1
            // 
            this.htmlLabel1.AutoSize = false;
            this.htmlLabel1.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel1.BaseStylesheet = null;
            this.htmlLabel1.Location = new System.Drawing.Point(316, 40);
            this.htmlLabel1.Name = "htmlLabel1";
            this.htmlLabel1.Size = new System.Drawing.Size(271, 63);
            this.htmlLabel1.TabIndex = 47;
            this.htmlLabel1.TabStop = false;
            this.htmlLabel1.Text = "normal label<br>label disabled";
            // 
            // yamuiLabel15
            // 
            this.yamuiLabel15.AutoSize = true;
            this.yamuiLabel15.Location = new System.Drawing.Point(203, 40);
            this.yamuiLabel15.Name = "yamuiLabel15";
            this.yamuiLabel15.Size = new System.Drawing.Size(73, 15);
            this.yamuiLabel15.TabIndex = 46;
            this.yamuiLabel15.Text = "normal label";
            // 
            // yamuiLink1
            // 
            this.yamuiLink1.Function = YamuiFramework.Fonts.FontFunction.Link;
            this.yamuiLink1.Location = new System.Drawing.Point(203, 122);
            this.yamuiLink1.Name = "yamuiLink1";
            this.yamuiLink1.Size = new System.Drawing.Size(118, 23);
            this.yamuiLink1.TabIndex = 44;
            this.yamuiLink1.Text = "This is a classic link";
            this.yamuiLink1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // yamuiLink2
            // 
            this.yamuiLink2.Enabled = false;
            this.yamuiLink2.Function = YamuiFramework.Fonts.FontFunction.Link;
            this.yamuiLink2.Location = new System.Drawing.Point(203, 143);
            this.yamuiLink2.Name = "yamuiLink2";
            this.yamuiLink2.Size = new System.Drawing.Size(118, 23);
            this.yamuiLink2.TabIndex = 45;
            this.yamuiLink2.Text = "This is a disabled link";
            this.yamuiLink2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // yamuiLabel12
            // 
            this.yamuiLabel12.AutoSize = true;
            this.yamuiLabel12.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel12.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel12.Margin = new System.Windows.Forms.Padding(20, 5, 5, 7);
            this.yamuiLabel12.Name = "yamuiLabel12";
            this.yamuiLabel12.Size = new System.Drawing.Size(70, 19);
            this.yamuiLabel12.TabIndex = 33;
            this.yamuiLabel12.Text = "TEXTBOX";
            // 
            // yamuiLabel17
            // 
            this.yamuiLabel17.AutoSize = true;
            this.yamuiLabel17.Location = new System.Drawing.Point(203, 70);
            this.yamuiLabel17.Name = "yamuiLabel17";
            this.yamuiLabel17.Size = new System.Drawing.Size(60, 15);
            this.yamuiLabel17.TabIndex = 42;
            this.yamuiLabel17.Text = "Selectable";
            // 
            // yamuiTextBox2
            // 
            this.yamuiTextBox2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.yamuiTextBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.yamuiTextBox2.CustomBackColor = System.Drawing.Color.Empty;
            this.yamuiTextBox2.CustomForeColor = System.Drawing.Color.Empty;
            this.yamuiTextBox2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.yamuiTextBox2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.yamuiTextBox2.Location = new System.Drawing.Point(0, 51);
            this.yamuiTextBox2.Name = "yamuiTextBox2";
            this.yamuiTextBox2.Size = new System.Drawing.Size(157, 23);
            this.yamuiTextBox2.TabIndex = 35;
            this.yamuiTextBox2.Text = "My text";
            this.yamuiTextBox2.WaterMark = null;
            // 
            // yamuiLabel16
            // 
            this.yamuiLabel16.AutoSize = true;
            this.yamuiLabel16.Enabled = false;
            this.yamuiLabel16.FakeDisabled = true;
            this.yamuiLabel16.Location = new System.Drawing.Point(203, 55);
            this.yamuiLabel16.Name = "yamuiLabel16";
            this.yamuiLabel16.Size = new System.Drawing.Size(79, 15);
            this.yamuiLabel16.TabIndex = 41;
            this.yamuiLabel16.Text = "label disabled";
            // 
            // yamuiTextBox3
            // 
            this.yamuiTextBox3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.yamuiTextBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.yamuiTextBox3.CustomBackColor = System.Drawing.Color.Empty;
            this.yamuiTextBox3.CustomForeColor = System.Drawing.Color.Empty;
            this.yamuiTextBox3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.yamuiTextBox3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.yamuiTextBox3.Location = new System.Drawing.Point(0, 80);
            this.yamuiTextBox3.MultiLines = true;
            this.yamuiTextBox3.Name = "yamuiTextBox3";
            this.yamuiTextBox3.Size = new System.Drawing.Size(157, 80);
            this.yamuiTextBox3.TabIndex = 36;
            this.yamuiTextBox3.Text = "Multiline";
            this.yamuiTextBox3.WaterMark = null;
            // 
            // yamuiLabel14
            // 
            this.yamuiLabel14.AutoSize = true;
            this.yamuiLabel14.Function = YamuiFramework.Fonts.FontFunction.Title;
            this.yamuiLabel14.Location = new System.Drawing.Point(203, 19);
            this.yamuiLabel14.Margin = new System.Windows.Forms.Padding(5);
            this.yamuiLabel14.Name = "yamuiLabel14";
            this.yamuiLabel14.Size = new System.Drawing.Size(50, 25);
            this.yamuiLabel14.TabIndex = 40;
            this.yamuiLabel14.Text = "Title";
            // 
            // yamuiTextBox4
            // 
            this.yamuiTextBox4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.yamuiTextBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.yamuiTextBox4.CustomBackColor = System.Drawing.Color.Empty;
            this.yamuiTextBox4.CustomForeColor = System.Drawing.Color.Empty;
            this.yamuiTextBox4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.yamuiTextBox4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.yamuiTextBox4.Location = new System.Drawing.Point(0, 166);
            this.yamuiTextBox4.Name = "yamuiTextBox4";
            this.yamuiTextBox4.ReadOnly = true;
            this.yamuiTextBox4.Size = new System.Drawing.Size(157, 23);
            this.yamuiTextBox4.TabIndex = 37;
            this.yamuiTextBox4.Text = "Read only";
            this.yamuiTextBox4.WaterMark = null;
            // 
            // yamuiLabel13
            // 
            this.yamuiLabel13.AutoSize = true;
            this.yamuiLabel13.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel13.Location = new System.Drawing.Point(203, 0);
            this.yamuiLabel13.Margin = new System.Windows.Forms.Padding(20, 5, 5, 7);
            this.yamuiLabel13.Name = "yamuiLabel13";
            this.yamuiLabel13.Size = new System.Drawing.Size(72, 19);
            this.yamuiLabel13.TabIndex = 39;
            this.yamuiLabel13.Text = "HEADING";
            // 
            // yamuiTextBox5
            // 
            this.yamuiTextBox5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.yamuiTextBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.yamuiTextBox5.CustomBackColor = System.Drawing.Color.Empty;
            this.yamuiTextBox5.CustomForeColor = System.Drawing.Color.Empty;
            this.yamuiTextBox5.Enabled = false;
            this.yamuiTextBox5.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.yamuiTextBox5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.yamuiTextBox5.Location = new System.Drawing.Point(0, 195);
            this.yamuiTextBox5.Name = "yamuiTextBox5";
            this.yamuiTextBox5.ReadOnly = true;
            this.yamuiTextBox5.Size = new System.Drawing.Size(157, 23);
            this.yamuiTextBox5.TabIndex = 38;
            this.yamuiTextBox5.Text = "Disabled";
            this.yamuiTextBox5.WaterMark = null;
            // 
            // Text
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.yamuiScrollPage1);
            this.Name = "Text";
            this.Size = new System.Drawing.Size(709, 327);
            this.yamuiScrollPage1.ContentPanel.ResumeLayout(false);
            this.yamuiScrollPage1.ContentPanel.PerformLayout();
            this.yamuiScrollPage1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private HtmlToolTip htmlToolTip1;
        private YamuiScrollPage yamuiScrollPage1;
        private HtmlPanel htmlPanel1;
        private HtmlLabel htmlLabel1;
        private YamuiLabel yamuiLabel15;
        private YamuiLink yamuiLink1;
        private YamuiLink yamuiLink2;
        private YamuiLabel yamuiLabel12;
        private YamuiLabel yamuiLabel18;
        private YamuiTextBox yamuiTextBox1;
        private YamuiLabel yamuiLabel17;
        private YamuiTextBox yamuiTextBox2;
        private YamuiLabel yamuiLabel16;
        private YamuiTextBox yamuiTextBox3;
        private YamuiLabel yamuiLabel14;
        private YamuiTextBox yamuiTextBox4;
        private YamuiLabel yamuiLabel13;
        private YamuiTextBox yamuiTextBox5;
    }
}
