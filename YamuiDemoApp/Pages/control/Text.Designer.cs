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
            this.yamuiPanel1 = new YamuiFramework.Controls.YamuiPanel();
            this.htmlPanel1 = new YamuiFramework.HtmlRenderer.WinForms.HtmlPanel();
            this.htmlLabel1 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.yamuiLabel15 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLink1 = new YamuiFramework.Controls.YamuiLink();
            this.yamuiLink2 = new YamuiFramework.Controls.YamuiLink();
            this.yamuiLabel12 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLabel18 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiTextBox1 = new YamuiFramework.Controls.YamuiTextBox();
            this.yamuiLabel17 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiTextBox2 = new YamuiFramework.Controls.YamuiTextBox();
            this.yamuiLabel16 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiTextBox3 = new YamuiFramework.Controls.YamuiTextBox();
            this.yamuiLabel14 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiTextBox4 = new YamuiFramework.Controls.YamuiTextBox();
            this.yamuiLabel13 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiTextBox5 = new YamuiFramework.Controls.YamuiTextBox();
            this.htmlToolTip1 = new YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip();
            this.yamuiPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // yamuiPanel1
            // 
            this.yamuiPanel1.Controls.Add(this.htmlPanel1);
            this.yamuiPanel1.Controls.Add(this.htmlLabel1);
            this.yamuiPanel1.Controls.Add(this.yamuiLabel15);
            this.yamuiPanel1.Controls.Add(this.yamuiLink1);
            this.yamuiPanel1.Controls.Add(this.yamuiLink2);
            this.yamuiPanel1.Controls.Add(this.yamuiLabel12);
            this.yamuiPanel1.Controls.Add(this.yamuiLabel18);
            this.yamuiPanel1.Controls.Add(this.yamuiTextBox1);
            this.yamuiPanel1.Controls.Add(this.yamuiLabel17);
            this.yamuiPanel1.Controls.Add(this.yamuiTextBox2);
            this.yamuiPanel1.Controls.Add(this.yamuiLabel16);
            this.yamuiPanel1.Controls.Add(this.yamuiTextBox3);
            this.yamuiPanel1.Controls.Add(this.yamuiLabel14);
            this.yamuiPanel1.Controls.Add(this.yamuiTextBox4);
            this.yamuiPanel1.Controls.Add(this.yamuiLabel13);
            this.yamuiPanel1.Controls.Add(this.yamuiTextBox5);
            this.yamuiPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.yamuiPanel1.HorizontalScrollbarHighlightOnWheel = false;
            this.yamuiPanel1.HorizontalScrollbarSize = 10;
            this.yamuiPanel1.Location = new System.Drawing.Point(0, 0);
            this.yamuiPanel1.Name = "yamuiPanel1";
            this.yamuiPanel1.Size = new System.Drawing.Size(709, 327);
            this.yamuiPanel1.TabIndex = 0;
            this.yamuiPanel1.VerticalScrollbarHighlightOnWheel = false;
            this.yamuiPanel1.VerticalScrollbarSize = 10;
            // 
            // htmlPanel1
            // 
            this.htmlPanel1.AutoScroll = true;
            this.htmlPanel1.AutoScrollMinSize = new System.Drawing.Size(338, 46);
            this.htmlPanel1.BackColor = System.Drawing.Color.Transparent;
            this.htmlPanel1.BaseStylesheet = null;
            this.htmlPanel1.Location = new System.Drawing.Point(347, 80);
            this.htmlPanel1.Name = "htmlPanel1";
            this.htmlPanel1.Size = new System.Drawing.Size(338, 224);
            this.htmlPanel1.TabIndex = 32;
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
            this.htmlLabel1.TabIndex = 31;
            this.htmlLabel1.TabStop = false;
            this.htmlLabel1.Text = "normal label<br>label disabled";
            // 
            // yamuiLabel15
            // 
            this.yamuiLabel15.AutoSize = true;
            this.yamuiLabel15.Location = new System.Drawing.Point(203, 40);
            this.yamuiLabel15.Name = "yamuiLabel15";
            this.yamuiLabel15.Size = new System.Drawing.Size(73, 15);
            this.yamuiLabel15.TabIndex = 30;
            this.yamuiLabel15.Text = "normal label";
            // 
            // yamuiLink1
            // 
            this.yamuiLink1.Function = YamuiFramework.Fonts.FontFunction.Link;
            this.yamuiLink1.Location = new System.Drawing.Point(203, 122);
            this.yamuiLink1.Name = "yamuiLink1";
            this.yamuiLink1.Size = new System.Drawing.Size(118, 23);
            this.yamuiLink1.TabIndex = 28;
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
            this.yamuiLink2.TabIndex = 29;
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
            this.yamuiLabel12.TabIndex = 17;
            this.yamuiLabel12.Text = "TEXTBOX";
            // 
            // yamuiLabel18
            // 
            this.yamuiLabel18.AutoSize = true;
            this.yamuiLabel18.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel18.Location = new System.Drawing.Point(203, 100);
            this.yamuiLabel18.Margin = new System.Windows.Forms.Padding(20, 5, 5, 7);
            this.yamuiLabel18.Name = "yamuiLabel18";
            this.yamuiLabel18.Size = new System.Drawing.Size(40, 19);
            this.yamuiLabel18.TabIndex = 27;
            this.yamuiLabel18.Text = "LINK";
            this.htmlToolTip1.SetToolTip(this.yamuiLabel18, "test?");
            // 
            // yamuiTextBox1
            // 
            this.yamuiTextBox1.Lines = new string[0];
            this.yamuiTextBox1.Location = new System.Drawing.Point(0, 22);
            this.yamuiTextBox1.MaxLength = 32767;
            this.yamuiTextBox1.Name = "yamuiTextBox1";
            this.yamuiTextBox1.PasswordChar = '\0';
            this.yamuiTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.yamuiTextBox1.SelectedText = "";
            this.yamuiTextBox1.Size = new System.Drawing.Size(157, 23);
            this.yamuiTextBox1.TabIndex = 18;
            this.htmlToolTip1.SetToolTip(this.yamuiTextBox1, "cool! <b>html?</b>");
            this.yamuiTextBox1.WaterMark = "Watermark!";
            // 
            // yamuiLabel17
            // 
            this.yamuiLabel17.AutoSize = true;
            this.yamuiLabel17.Location = new System.Drawing.Point(203, 70);
            this.yamuiLabel17.Name = "yamuiLabel17";
            this.yamuiLabel17.Size = new System.Drawing.Size(60, 15);
            this.yamuiLabel17.TabIndex = 26;
            this.yamuiLabel17.Text = "Selectable";
            // 
            // yamuiTextBox2
            // 
            this.yamuiTextBox2.Lines = new string[] {
        "My text"};
            this.yamuiTextBox2.Location = new System.Drawing.Point(0, 51);
            this.yamuiTextBox2.MaxLength = 32767;
            this.yamuiTextBox2.Name = "yamuiTextBox2";
            this.yamuiTextBox2.PasswordChar = '\0';
            this.yamuiTextBox2.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.yamuiTextBox2.SelectedText = "";
            this.yamuiTextBox2.Size = new System.Drawing.Size(157, 23);
            this.yamuiTextBox2.TabIndex = 19;
            this.yamuiTextBox2.Text = "My text";
            // 
            // yamuiLabel16
            // 
            this.yamuiLabel16.AutoSize = true;
            this.yamuiLabel16.Enabled = false;
            this.yamuiLabel16.FakeDisabled = true;
            this.yamuiLabel16.Location = new System.Drawing.Point(203, 55);
            this.yamuiLabel16.Name = "yamuiLabel16";
            this.yamuiLabel16.Size = new System.Drawing.Size(79, 15);
            this.yamuiLabel16.TabIndex = 25;
            this.yamuiLabel16.Text = "label disabled";
            // 
            // yamuiTextBox3
            // 
            this.yamuiTextBox3.Lines = new string[] {
        "Multiline"};
            this.yamuiTextBox3.Location = new System.Drawing.Point(0, 80);
            this.yamuiTextBox3.MaxLength = 32767;
            this.yamuiTextBox3.MultiLines = true;
            this.yamuiTextBox3.Name = "yamuiTextBox3";
            this.yamuiTextBox3.PasswordChar = '\0';
            this.yamuiTextBox3.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.yamuiTextBox3.SelectedText = "";
            this.yamuiTextBox3.Size = new System.Drawing.Size(157, 80);
            this.yamuiTextBox3.TabIndex = 20;
            this.yamuiTextBox3.Text = "Multiline";
            // 
            // yamuiLabel14
            // 
            this.yamuiLabel14.AutoSize = true;
            this.yamuiLabel14.Function = YamuiFramework.Fonts.FontFunction.Title;
            this.yamuiLabel14.Location = new System.Drawing.Point(203, 19);
            this.yamuiLabel14.Margin = new System.Windows.Forms.Padding(5);
            this.yamuiLabel14.Name = "yamuiLabel14";
            this.yamuiLabel14.Size = new System.Drawing.Size(39, 21);
            this.yamuiLabel14.TabIndex = 24;
            this.yamuiLabel14.Text = "Title";
            // 
            // yamuiTextBox4
            // 
            this.yamuiTextBox4.Lines = new string[] {
        "Read only"};
            this.yamuiTextBox4.Location = new System.Drawing.Point(0, 166);
            this.yamuiTextBox4.MaxLength = 32767;
            this.yamuiTextBox4.Name = "yamuiTextBox4";
            this.yamuiTextBox4.PasswordChar = '\0';
            this.yamuiTextBox4.ReadOnly = true;
            this.yamuiTextBox4.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.yamuiTextBox4.SelectedText = "";
            this.yamuiTextBox4.Size = new System.Drawing.Size(157, 23);
            this.yamuiTextBox4.TabIndex = 21;
            this.yamuiTextBox4.Text = "Read only";
            // 
            // yamuiLabel13
            // 
            this.yamuiLabel13.AutoSize = true;
            this.yamuiLabel13.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel13.Location = new System.Drawing.Point(203, 0);
            this.yamuiLabel13.Margin = new System.Windows.Forms.Padding(20, 5, 5, 7);
            this.yamuiLabel13.Name = "yamuiLabel13";
            this.yamuiLabel13.Size = new System.Drawing.Size(72, 19);
            this.yamuiLabel13.TabIndex = 23;
            this.yamuiLabel13.Text = "HEADING";
            // 
            // yamuiTextBox5
            // 
            this.yamuiTextBox5.Enabled = false;
            this.yamuiTextBox5.Lines = new string[] {
        "Disabled"};
            this.yamuiTextBox5.Location = new System.Drawing.Point(0, 195);
            this.yamuiTextBox5.MaxLength = 32767;
            this.yamuiTextBox5.Name = "yamuiTextBox5";
            this.yamuiTextBox5.PasswordChar = '\0';
            this.yamuiTextBox5.ReadOnly = true;
            this.yamuiTextBox5.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.yamuiTextBox5.SelectedText = "";
            this.yamuiTextBox5.Size = new System.Drawing.Size(157, 23);
            this.yamuiTextBox5.TabIndex = 22;
            this.yamuiTextBox5.Text = "Disabled";
            // 
            // htmlToolTip1
            // 
            this.htmlToolTip1.AllowLinksHandling = true;
            this.htmlToolTip1.BaseStylesheet = null;
            this.htmlToolTip1.MaximumSize = new System.Drawing.Size(0, 0);
            this.htmlToolTip1.OwnerDraw = true;
            this.htmlToolTip1.TooltipCssClass = "htmltooltip";
            // 
            // Text
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.yamuiPanel1);
            this.Name = "Text";
            this.Size = new System.Drawing.Size(709, 327);
            this.yamuiPanel1.ResumeLayout(false);
            this.yamuiPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiPanel yamuiPanel1;
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
        private HtmlLabel htmlLabel1;
        private HtmlPanel htmlPanel1;
        private HtmlToolTip htmlToolTip1;
    }
}
