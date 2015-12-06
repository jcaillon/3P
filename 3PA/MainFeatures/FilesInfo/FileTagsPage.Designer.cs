namespace _3PA.MainFeatures.FilesInfo {
    partial class FileTagsPage {
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

                yamuiComboBox1.SelectedIndexChanged -= SelectedIndexChanged;
                btok.ButtonPressed -= BtokOnButtonPressed;
                btcancel.ButtonPressed -= BtcancelOnButtonPressed;
                btclear.ButtonPressed -= BtclearOnButtonPressed;
                btdefault.ButtonPressed -= BtdefaultOnButtonPressed;
                bttoday.ButtonPressed -= BttodayOnButtonPressed;
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.yamuiComboBox1 = new YamuiFramework.Controls.YamuiComboBox();
            this.yamuiLabel2 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiTextBox1 = new YamuiFramework.Controls.YamuiTextBox();
            this.yamuiLabel3 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiTextBox2 = new YamuiFramework.Controls.YamuiTextBox();
            this.yamuiLabel4 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLabel5 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiTextBox3 = new YamuiFramework.Controls.YamuiTextBox();
            this.yamuiLabel6 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiTextBox4 = new YamuiFramework.Controls.YamuiTextBox();
            this.yamuiLabel7 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLabel8 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiTextBox5 = new YamuiFramework.Controls.YamuiTextBox();
            this.yamuiTextBox6 = new YamuiFramework.Controls.YamuiTextBox();
            this.yamuiTextBox7 = new YamuiFramework.Controls.YamuiTextBox();
            this.yamuiLabel9 = new YamuiFramework.Controls.YamuiLabel();
            this.btcancel = new YamuiFramework.Controls.YamuiButton();
            this.btok = new YamuiFramework.Controls.YamuiButton();
            this.btclear = new YamuiFramework.Controls.YamuiButton();
            this.btdefault = new YamuiFramework.Controls.YamuiButton();
            this.bttoday = new YamuiFramework.Controls.YamuiButton();
            this.SuspendLayout();
            // 
            // yamuiComboBox1
            // 
            this.yamuiComboBox1.ItemHeight = 19;
            this.yamuiComboBox1.Location = new System.Drawing.Point(0, 25);
            this.yamuiComboBox1.Name = "yamuiComboBox1";
            this.yamuiComboBox1.Size = new System.Drawing.Size(326, 25);
            this.yamuiComboBox1.TabIndex = 0;
            // 
            // yamuiLabel2
            // 
            this.yamuiLabel2.AutoSize = true;
            this.yamuiLabel2.Function = YamuiFramework.Fonts.LabelFunction.Heading;
            this.yamuiLabel2.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel2.Margin = new System.Windows.Forms.Padding(5, 18, 5, 3);
            this.yamuiLabel2.Name = "yamuiLabel2";
            this.yamuiLabel2.Size = new System.Drawing.Size(103, 19);
            this.yamuiLabel2.TabIndex = 2;
            this.yamuiLabel2.Text = "Pre-selections";
            // 
            // yamuiTextBox1
            // 
            this.yamuiTextBox1.Lines = new string[0];
            this.yamuiTextBox1.Location = new System.Drawing.Point(0, 91);
            this.yamuiTextBox1.MaxLength = 32767;
            this.yamuiTextBox1.Name = "yamuiTextBox1";
            this.yamuiTextBox1.PasswordChar = '\0';
            this.yamuiTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.yamuiTextBox1.SelectedText = "";
            this.yamuiTextBox1.Size = new System.Drawing.Size(154, 23);
            this.yamuiTextBox1.TabIndex = 1;
            this.yamuiTextBox1.WaterMark = "Ex: BOI";
            // 
            // yamuiLabel3
            // 
            this.yamuiLabel3.AutoSize = true;
            this.yamuiLabel3.Function = YamuiFramework.Fonts.LabelFunction.Heading;
            this.yamuiLabel3.Location = new System.Drawing.Point(2, 66);
            this.yamuiLabel3.Margin = new System.Windows.Forms.Padding(5, 13, 5, 3);
            this.yamuiLabel3.Name = "yamuiLabel3";
            this.yamuiLabel3.Size = new System.Drawing.Size(127, 19);
            this.yamuiLabel3.TabIndex = 4;
            this.yamuiLabel3.Text = "Application name";
            // 
            // yamuiTextBox2
            // 
            this.yamuiTextBox2.Lines = new string[0];
            this.yamuiTextBox2.Location = new System.Drawing.Point(175, 91);
            this.yamuiTextBox2.MaxLength = 32767;
            this.yamuiTextBox2.Name = "yamuiTextBox2";
            this.yamuiTextBox2.PasswordChar = '\0';
            this.yamuiTextBox2.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.yamuiTextBox2.SelectedText = "";
            this.yamuiTextBox2.Size = new System.Drawing.Size(154, 23);
            this.yamuiTextBox2.TabIndex = 2;
            this.yamuiTextBox2.WaterMark = "Ex: 65.000";
            // 
            // yamuiLabel4
            // 
            this.yamuiLabel4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.yamuiLabel4.AutoSize = true;
            this.yamuiLabel4.Function = YamuiFramework.Fonts.LabelFunction.Heading;
            this.yamuiLabel4.Location = new System.Drawing.Point(270, 66);
            this.yamuiLabel4.Margin = new System.Windows.Forms.Padding(5, 13, 5, 3);
            this.yamuiLabel4.Name = "yamuiLabel4";
            this.yamuiLabel4.Size = new System.Drawing.Size(59, 19);
            this.yamuiLabel4.TabIndex = 6;
            this.yamuiLabel4.Text = "Version";
            this.yamuiLabel4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // yamuiLabel5
            // 
            this.yamuiLabel5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.yamuiLabel5.AutoSize = true;
            this.yamuiLabel5.Function = YamuiFramework.Fonts.LabelFunction.Heading;
            this.yamuiLabel5.Location = new System.Drawing.Point(273, 130);
            this.yamuiLabel5.Margin = new System.Windows.Forms.Padding(5, 13, 5, 3);
            this.yamuiLabel5.Name = "yamuiLabel5";
            this.yamuiLabel5.Size = new System.Drawing.Size(56, 19);
            this.yamuiLabel5.TabIndex = 10;
            this.yamuiLabel5.Text = "JIRA ID";
            this.yamuiLabel5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // yamuiTextBox3
            // 
            this.yamuiTextBox3.Lines = new string[0];
            this.yamuiTextBox3.Location = new System.Drawing.Point(175, 155);
            this.yamuiTextBox3.MaxLength = 32767;
            this.yamuiTextBox3.Name = "yamuiTextBox3";
            this.yamuiTextBox3.PasswordChar = '\0';
            this.yamuiTextBox3.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.yamuiTextBox3.SelectedText = "";
            this.yamuiTextBox3.Size = new System.Drawing.Size(154, 23);
            this.yamuiTextBox3.TabIndex = 4;
            this.yamuiTextBox3.WaterMark = "Ex: INC0999999";
            // 
            // yamuiLabel6
            // 
            this.yamuiLabel6.AutoSize = true;
            this.yamuiLabel6.Function = YamuiFramework.Fonts.LabelFunction.Heading;
            this.yamuiLabel6.Location = new System.Drawing.Point(2, 130);
            this.yamuiLabel6.Margin = new System.Windows.Forms.Padding(5, 13, 5, 3);
            this.yamuiLabel6.Name = "yamuiLabel6";
            this.yamuiLabel6.Size = new System.Drawing.Size(109, 19);
            this.yamuiLabel6.TabIndex = 8;
            this.yamuiLabel6.Text = "Work-package";
            // 
            // yamuiTextBox4
            // 
            this.yamuiTextBox4.Lines = new string[0];
            this.yamuiTextBox4.Location = new System.Drawing.Point(0, 155);
            this.yamuiTextBox4.MaxLength = 32767;
            this.yamuiTextBox4.Name = "yamuiTextBox4";
            this.yamuiTextBox4.PasswordChar = '\0';
            this.yamuiTextBox4.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.yamuiTextBox4.SelectedText = "";
            this.yamuiTextBox4.Size = new System.Drawing.Size(154, 23);
            this.yamuiTextBox4.TabIndex = 3;
            this.yamuiTextBox4.WaterMark = "Ex: 101-33";
            // 
            // yamuiLabel7
            // 
            this.yamuiLabel7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.yamuiLabel7.AutoSize = true;
            this.yamuiLabel7.Function = YamuiFramework.Fonts.LabelFunction.Heading;
            this.yamuiLabel7.Location = new System.Drawing.Point(289, 194);
            this.yamuiLabel7.Margin = new System.Windows.Forms.Padding(5, 13, 5, 3);
            this.yamuiLabel7.Name = "yamuiLabel7";
            this.yamuiLabel7.Size = new System.Drawing.Size(40, 19);
            this.yamuiLabel7.TabIndex = 12;
            this.yamuiLabel7.Text = "Date";
            this.yamuiLabel7.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // yamuiLabel8
            // 
            this.yamuiLabel8.AutoSize = true;
            this.yamuiLabel8.Function = YamuiFramework.Fonts.LabelFunction.Heading;
            this.yamuiLabel8.Location = new System.Drawing.Point(2, 194);
            this.yamuiLabel8.Margin = new System.Windows.Forms.Padding(5, 13, 5, 3);
            this.yamuiLabel8.Name = "yamuiLabel8";
            this.yamuiLabel8.Size = new System.Drawing.Size(136, 19);
            this.yamuiLabel8.TabIndex = 13;
            this.yamuiLabel8.Text = "Correction number";
            // 
            // yamuiTextBox5
            // 
            this.yamuiTextBox5.Lines = new string[0];
            this.yamuiTextBox5.Location = new System.Drawing.Point(0, 219);
            this.yamuiTextBox5.MaxLength = 32767;
            this.yamuiTextBox5.Name = "yamuiTextBox5";
            this.yamuiTextBox5.PasswordChar = '\0';
            this.yamuiTextBox5.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.yamuiTextBox5.SelectedText = "";
            this.yamuiTextBox5.Size = new System.Drawing.Size(154, 23);
            this.yamuiTextBox5.TabIndex = 5;
            this.yamuiTextBox5.WaterMark = "Ex: 9";
            // 
            // yamuiTextBox6
            // 
            this.yamuiTextBox6.Lines = new string[0];
            this.yamuiTextBox6.Location = new System.Drawing.Point(226, 219);
            this.yamuiTextBox6.MaxLength = 32767;
            this.yamuiTextBox6.Name = "yamuiTextBox6";
            this.yamuiTextBox6.PasswordChar = '\0';
            this.yamuiTextBox6.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.yamuiTextBox6.SelectedText = "";
            this.yamuiTextBox6.Size = new System.Drawing.Size(103, 23);
            this.yamuiTextBox6.TabIndex = 8;
            this.yamuiTextBox6.WaterMark = "22/11/2015";
            // 
            // yamuiTextBox7
            // 
            this.yamuiTextBox7.Lines = new string[0];
            this.yamuiTextBox7.Location = new System.Drawing.Point(2, 283);
            this.yamuiTextBox7.MaxLength = 32767;
            this.yamuiTextBox7.Multiline = true;
            this.yamuiTextBox7.Name = "yamuiTextBox7";
            this.yamuiTextBox7.PasswordChar = '\0';
            this.yamuiTextBox7.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.yamuiTextBox7.SelectedText = "";
            this.yamuiTextBox7.Size = new System.Drawing.Size(324, 80);
            this.yamuiTextBox7.TabIndex = 9;
            this.yamuiTextBox7.WaterMark = "Ex: Fixing a small bug";
            // 
            // yamuiLabel9
            // 
            this.yamuiLabel9.AutoSize = true;
            this.yamuiLabel9.Function = YamuiFramework.Fonts.LabelFunction.Heading;
            this.yamuiLabel9.Location = new System.Drawing.Point(2, 258);
            this.yamuiLabel9.Margin = new System.Windows.Forms.Padding(5, 13, 5, 3);
            this.yamuiLabel9.Name = "yamuiLabel9";
            this.yamuiLabel9.Size = new System.Drawing.Size(201, 19);
            this.yamuiLabel9.TabIndex = 16;
            this.yamuiLabel9.Text = "Description of the correction";
            // 
            // btcancel
            // 
            this.btcancel.Location = new System.Drawing.Point(267, 369);
            this.btcancel.Name = "btcancel";
            this.btcancel.Size = new System.Drawing.Size(59, 23);
            this.btcancel.TabIndex = 11;
            this.btcancel.Text = "&Cancel";
            // 
            // btok
            // 
            this.btok.Location = new System.Drawing.Point(223, 369);
            this.btok.Name = "btok";
            this.btok.Size = new System.Drawing.Size(38, 23);
            this.btok.TabIndex = 10;
            this.btok.Text = "&Ok";
            // 
            // btclear
            // 
            this.btclear.Location = new System.Drawing.Point(2, 369);
            this.btclear.Name = "btclear";
            this.btclear.Size = new System.Drawing.Size(46, 23);
            this.btclear.TabIndex = 19;
            this.btclear.TabStop = false;
            this.btclear.Text = "Clear";
            // 
            // btdefault
            // 
            this.btdefault.Location = new System.Drawing.Point(54, 369);
            this.btdefault.Name = "btdefault";
            this.btdefault.Size = new System.Drawing.Size(84, 23);
            this.btdefault.TabIndex = 20;
            this.btdefault.TabStop = false;
            this.btdefault.Text = "Set as default";
            // 
            // bttoday
            // 
            this.bttoday.Location = new System.Drawing.Point(175, 219);
            this.bttoday.Name = "bttoday";
            this.bttoday.Size = new System.Drawing.Size(45, 23);
            this.bttoday.TabIndex = 7;
            this.bttoday.Text = "&Today";
            // 
            // FileTagsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.bttoday);
            this.Controls.Add(this.btdefault);
            this.Controls.Add(this.btclear);
            this.Controls.Add(this.btok);
            this.Controls.Add(this.btcancel);
            this.Controls.Add(this.yamuiLabel9);
            this.Controls.Add(this.yamuiTextBox7);
            this.Controls.Add(this.yamuiTextBox6);
            this.Controls.Add(this.yamuiLabel7);
            this.Controls.Add(this.yamuiLabel8);
            this.Controls.Add(this.yamuiTextBox5);
            this.Controls.Add(this.yamuiLabel5);
            this.Controls.Add(this.yamuiTextBox3);
            this.Controls.Add(this.yamuiLabel6);
            this.Controls.Add(this.yamuiTextBox4);
            this.Controls.Add(this.yamuiLabel4);
            this.Controls.Add(this.yamuiTextBox2);
            this.Controls.Add(this.yamuiLabel3);
            this.Controls.Add(this.yamuiTextBox1);
            this.Controls.Add(this.yamuiLabel2);
            this.Controls.Add(this.yamuiComboBox1);
            this.Name = "FileTagsPage";
            this.Size = new System.Drawing.Size(329, 399);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private YamuiFramework.Controls.YamuiComboBox yamuiComboBox1;
        private YamuiFramework.Controls.YamuiLabel yamuiLabel2;
        private YamuiFramework.Controls.YamuiTextBox yamuiTextBox1;
        private YamuiFramework.Controls.YamuiLabel yamuiLabel3;
        private YamuiFramework.Controls.YamuiTextBox yamuiTextBox2;
        private YamuiFramework.Controls.YamuiLabel yamuiLabel4;
        private YamuiFramework.Controls.YamuiLabel yamuiLabel5;
        private YamuiFramework.Controls.YamuiTextBox yamuiTextBox3;
        private YamuiFramework.Controls.YamuiLabel yamuiLabel6;
        private YamuiFramework.Controls.YamuiTextBox yamuiTextBox4;
        private YamuiFramework.Controls.YamuiLabel yamuiLabel7;
        private YamuiFramework.Controls.YamuiLabel yamuiLabel8;
        private YamuiFramework.Controls.YamuiTextBox yamuiTextBox5;
        private YamuiFramework.Controls.YamuiTextBox yamuiTextBox6;
        private YamuiFramework.Controls.YamuiTextBox yamuiTextBox7;
        private YamuiFramework.Controls.YamuiLabel yamuiLabel9;
        private YamuiFramework.Controls.YamuiButton btcancel;
        private YamuiFramework.Controls.YamuiButton btok;
        private YamuiFramework.Controls.YamuiButton btclear;
        private YamuiFramework.Controls.YamuiButton btdefault;
        private YamuiFramework.Controls.YamuiButton bttoday;
    }
}
