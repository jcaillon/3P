using System.ComponentModel;
using YamuiFramework.Controls;
using YamuiFramework.HtmlRenderer.WinForms;

namespace _3PA.MainFeatures.Appli.Pages.Set {
    partial class SetFtp {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetFtp));
            this.yamuiButton1 = new YamuiFramework.Controls.YamuiButton();
            this.bt_reset = new YamuiFramework.Controls.YamuiButton();
            this.yamuiLabel2 = new YamuiFramework.Controls.YamuiLabel();
            this.lbl_about = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.fl_timeout = new YamuiFramework.Controls.YamuiTextBox();
            this.htmlLabel4 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel9 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.btleft1 = new YamuiFramework.Controls.YamuiButtonImage();
            this.htmlLabel8 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.fl_remoteDir = new YamuiFramework.Controls.YamuiTextBox();
            this.htmlLabel7 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.fl_password = new YamuiFramework.Controls.YamuiTextBox();
            this.htmlLabel3 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel2 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel1 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.yamuiLabel1 = new YamuiFramework.Controls.YamuiLabel();
            this.cb_info = new YamuiFramework.Controls.YamuiComboBox();
            this.fl_host = new YamuiFramework.Controls.YamuiTextBox();
            this.fl_port = new YamuiFramework.Controls.YamuiTextBox();
            this.fl_user = new YamuiFramework.Controls.YamuiTextBox();
            this.bt_test = new YamuiFramework.Controls.YamuiButton();
            this.toolTip = new YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip();
            this.SuspendLayout();
            // 
            // scrollPanel
            // 
            // 
            // scrollPanel.ContentPanel
            // 
            this.Controls.Add(this.yamuiButton1);
            this.Controls.Add(this.bt_reset);
            this.Controls.Add(this.yamuiLabel2);
            this.Controls.Add(this.lbl_about);
            this.Controls.Add(this.fl_timeout);
            this.Controls.Add(this.htmlLabel4);
            this.Controls.Add(this.htmlLabel9);
            this.Controls.Add(this.btleft1);
            this.Controls.Add(this.htmlLabel8);
            this.Controls.Add(this.fl_remoteDir);
            this.Controls.Add(this.htmlLabel7);
            this.Controls.Add(this.fl_password);
            this.Controls.Add(this.htmlLabel3);
            this.Controls.Add(this.htmlLabel2);
            this.Controls.Add(this.htmlLabel1);
            this.Controls.Add(this.yamuiLabel1);
            this.Controls.Add(this.cb_info);
            this.Controls.Add(this.fl_host);
            this.Controls.Add(this.fl_port);
            this.Controls.Add(this.fl_user);
            this.Controls.Add(this.bt_test);
            // 
            // yamuiButton1
            // 
            this.yamuiButton1.BackGrndImage = null;
            this.yamuiButton1.Location = new System.Drawing.Point(305, 350);
            this.yamuiButton1.Name = "yamuiButton1";
            this.yamuiButton1.SetImgSize = new System.Drawing.Size(0, 0);
            this.yamuiButton1.Size = new System.Drawing.Size(60, 23);
            this.yamuiButton1.TabIndex = 112;
            this.yamuiButton1.Text = "&Save";
            // 
            // bt_reset
            // 
            this.bt_reset.BackGrndImage = null;
            this.bt_reset.Location = new System.Drawing.Point(371, 350);
            this.bt_reset.Name = "bt_reset";
            this.bt_reset.SetImgSize = new System.Drawing.Size(0, 0);
            this.bt_reset.Size = new System.Drawing.Size(58, 23);
            this.bt_reset.TabIndex = 111;
            this.bt_reset.Text = "&Reset";
            // 
            // yamuiLabel2
            // 
            this.yamuiLabel2.AutoSize = true;
            this.yamuiLabel2.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel2.Location = new System.Drawing.Point(0, 120);
            this.yamuiLabel2.Margin = new System.Windows.Forms.Padding(5, 18, 5, 3);
            this.yamuiLabel2.Name = "yamuiLabel2";
            this.yamuiLabel2.Size = new System.Drawing.Size(179, 19);
            this.yamuiLabel2.TabIndex = 110;
            this.yamuiLabel2.Text = "SET UP FTP CONNECTION";
            // 
            // lbl_about
            // 
            this.lbl_about.AutoSize = false;
            this.lbl_about.AutoSizeHeightOnly = true;
            this.lbl_about.BackColor = System.Drawing.Color.Transparent;
            this.lbl_about.BaseStylesheet = null;
            this.lbl_about.Location = new System.Drawing.Point(30, 25);
            this.lbl_about.Name = "lbl_about";
            this.lbl_about.Size = new System.Drawing.Size(673, 74);
            this.lbl_about.TabIndex = 109;
            this.lbl_about.TabStop = false;
            this.lbl_about.Text = resources.GetString("lbl_about.Text");
            // 
            // fl_timeout
            // 
            this.fl_timeout.Location = new System.Drawing.Point(193, 312);
            this.fl_timeout.Name = "fl_timeout";
            this.fl_timeout.Size = new System.Drawing.Size(93, 20);
            this.fl_timeout.TabIndex = 108;
            this.fl_timeout.WaterMark = null;
            // 
            // htmlLabel4
            // 
            this.htmlLabel4.AutoSize = false;
            this.htmlLabel4.AutoSizeHeightOnly = true;
            this.htmlLabel4.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel4.BaseStylesheet = null;
            this.htmlLabel4.Location = new System.Drawing.Point(30, 312);
            this.htmlLabel4.Name = "htmlLabel4";
            this.htmlLabel4.Size = new System.Drawing.Size(157, 15);
            this.htmlLabel4.TabIndex = 107;
            this.htmlLabel4.TabStop = false;
            this.htmlLabel4.Text = "<b>Connection time out (ms)</b>";
            // 
            // htmlLabel9
            // 
            this.htmlLabel9.AutoSize = false;
            this.htmlLabel9.AutoSizeHeightOnly = true;
            this.htmlLabel9.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel9.BaseStylesheet = null;
            this.htmlLabel9.Location = new System.Drawing.Point(30, 285);
            this.htmlLabel9.Name = "htmlLabel9";
            this.htmlLabel9.Size = new System.Drawing.Size(157, 15);
            this.htmlLabel9.TabIndex = 106;
            this.htmlLabel9.TabStop = false;
            this.htmlLabel9.Text = "<b>SSL/TLS support</b>";
            // 
            // btleft1
            // 
            this.btleft1.BackColor = System.Drawing.Color.Transparent;
            this.btleft1.BackGrndImage = null;
            this.btleft1.Location = new System.Drawing.Point(170, 223);
            this.btleft1.Margin = new System.Windows.Forms.Padding(0);
            this.btleft1.Name = "btleft1";
            this.btleft1.SetImgSize = new System.Drawing.Size(0, 0);
            this.btleft1.Size = new System.Drawing.Size(20, 20);
            this.btleft1.TabIndex = 105;
            this.btleft1.TabStop = false;
            this.btleft1.Text = "yamuiImageButton10";
            // 
            // htmlLabel8
            // 
            this.htmlLabel8.AutoSize = false;
            this.htmlLabel8.AutoSizeHeightOnly = true;
            this.htmlLabel8.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel8.BaseStylesheet = null;
            this.htmlLabel8.Location = new System.Drawing.Point(30, 223);
            this.htmlLabel8.Name = "htmlLabel8";
            this.htmlLabel8.Size = new System.Drawing.Size(157, 30);
            this.htmlLabel8.TabIndex = 77;
            this.htmlLabel8.TabStop = false;
            this.htmlLabel8.Text = "<b>Remote upload<br>directory</b>";
            // 
            // fl_remoteDir
            // 
            this.fl_remoteDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fl_remoteDir.Location = new System.Drawing.Point(193, 223);
            this.fl_remoteDir.Name = "fl_remoteDir";
            this.fl_remoteDir.Size = new System.Drawing.Size(704, 20);
            this.fl_remoteDir.TabIndex = 76;
            this.fl_remoteDir.WaterMark = null;
            // 
            // htmlLabel7
            // 
            this.htmlLabel7.AutoSize = false;
            this.htmlLabel7.AutoSizeHeightOnly = true;
            this.htmlLabel7.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel7.BaseStylesheet = null;
            this.htmlLabel7.Location = new System.Drawing.Point(30, 197);
            this.htmlLabel7.Name = "htmlLabel7";
            this.htmlLabel7.Size = new System.Drawing.Size(157, 15);
            this.htmlLabel7.TabIndex = 74;
            this.htmlLabel7.TabStop = false;
            this.htmlLabel7.Text = "<b>Password</b>";
            // 
            // fl_password
            // 
            this.fl_password.Location = new System.Drawing.Point(193, 197);
            this.fl_password.Name = "fl_password";
            this.fl_password.Size = new System.Drawing.Size(266, 20);
            this.fl_password.TabIndex = 73;
            this.fl_password.WaterMark = null;
            // 
            // htmlLabel3
            // 
            this.htmlLabel3.AutoSize = false;
            this.htmlLabel3.AutoSizeHeightOnly = true;
            this.htmlLabel3.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel3.BaseStylesheet = null;
            this.htmlLabel3.Location = new System.Drawing.Point(30, 171);
            this.htmlLabel3.Name = "htmlLabel3";
            this.htmlLabel3.Size = new System.Drawing.Size(157, 15);
            this.htmlLabel3.TabIndex = 54;
            this.htmlLabel3.TabStop = false;
            this.htmlLabel3.Text = "<b>User</b>";
            // 
            // htmlLabel2
            // 
            this.htmlLabel2.AutoSize = false;
            this.htmlLabel2.AutoSizeHeightOnly = true;
            this.htmlLabel2.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel2.BaseStylesheet = null;
            this.htmlLabel2.Location = new System.Drawing.Point(30, 259);
            this.htmlLabel2.Name = "htmlLabel2";
            this.htmlLabel2.Size = new System.Drawing.Size(157, 15);
            this.htmlLabel2.TabIndex = 53;
            this.htmlLabel2.TabStop = false;
            this.htmlLabel2.Text = "<b>TCP/IP connection port</b>";
            // 
            // htmlLabel1
            // 
            this.htmlLabel1.AutoSize = false;
            this.htmlLabel1.AutoSizeHeightOnly = true;
            this.htmlLabel1.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel1.BaseStylesheet = null;
            this.htmlLabel1.Location = new System.Drawing.Point(30, 145);
            this.htmlLabel1.Name = "htmlLabel1";
            this.htmlLabel1.Size = new System.Drawing.Size(139, 15);
            this.htmlLabel1.TabIndex = 52;
            this.htmlLabel1.TabStop = false;
            this.htmlLabel1.Text = "<b>Host name</b>";
            // 
            // yamuiLabel1
            // 
            this.yamuiLabel1.AutoSize = true;
            this.yamuiLabel1.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel1.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel1.Margin = new System.Windows.Forms.Padding(5, 18, 5, 3);
            this.yamuiLabel1.Name = "yamuiLabel1";
            this.yamuiLabel1.Size = new System.Drawing.Size(150, 19);
            this.yamuiLabel1.TabIndex = 51;
            this.yamuiLabel1.Text = "ABOUT THIS OPTION";
            // 
            // cb_info
            // 
            this.cb_info.Location = new System.Drawing.Point(193, 285);
            this.cb_info.Name = "cb_info";
            this.cb_info.Size = new System.Drawing.Size(394, 21);
            this.cb_info.TabIndex = 50;
            this.cb_info.WaterMark = "Leave empty to determinate it automatically";
            // 
            // fl_host
            // 
            this.fl_host.Location = new System.Drawing.Point(193, 145);
            this.fl_host.Name = "fl_host";
            this.fl_host.Size = new System.Drawing.Size(266, 20);
            this.fl_host.TabIndex = 49;
            this.fl_host.WaterMark = null;
            // 
            // fl_port
            // 
            this.fl_port.Location = new System.Drawing.Point(193, 259);
            this.fl_port.Name = "fl_port";
            this.fl_port.Size = new System.Drawing.Size(57, 20);
            this.fl_port.TabIndex = 48;
            this.fl_port.WaterMark = "Default";
            // 
            // fl_user
            // 
            this.fl_user.Location = new System.Drawing.Point(193, 171);
            this.fl_user.Name = "fl_user";
            this.fl_user.Size = new System.Drawing.Size(266, 20);
            this.fl_user.TabIndex = 47;
            this.fl_user.WaterMark = "Leave empty for an anonymous connection!";
            // 
            // bt_test
            // 
            this.bt_test.BackGrndImage = null;
            this.bt_test.Location = new System.Drawing.Point(193, 350);
            this.bt_test.Margin = new System.Windows.Forms.Padding(3, 15, 3, 3);
            this.bt_test.Name = "bt_test";
            this.bt_test.SetImgSize = new System.Drawing.Size(0, 0);
            this.bt_test.Size = new System.Drawing.Size(106, 23);
            this.bt_test.TabIndex = 42;
            this.bt_test.Text = "&Test connection";
            // 
            // toolTip
            // 
            this.toolTip.AllowLinksHandling = true;
            this.toolTip.AutoPopDelay = 90000;
            this.toolTip.BaseStylesheet = null;
            this.toolTip.InitialDelay = 300;
            this.toolTip.MaximumSize = new System.Drawing.Size(0, 0);
            this.toolTip.OwnerDraw = true;
            this.toolTip.ReshowDelay = 100;
            // 
            // SetFtp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "SetFtp";
            this.Size = new System.Drawing.Size(900, 650);
            this.ResumeLayout(false);

        }

        #endregion
        
        private YamuiComboBox cb_info;
        private YamuiTextBox fl_host;
        private YamuiTextBox fl_port;
        private YamuiTextBox fl_user;
        private YamuiButton bt_test;
        private YamuiLabel yamuiLabel1;
        private HtmlLabel htmlLabel1;
        private HtmlLabel htmlLabel2;
        private HtmlLabel htmlLabel3;
        private HtmlToolTip toolTip;
        private HtmlLabel htmlLabel7;
        private YamuiTextBox fl_password;
        private HtmlLabel htmlLabel8;
        private YamuiTextBox fl_remoteDir;
        private YamuiButtonImage btleft1;
        private HtmlLabel htmlLabel9;
        private YamuiTextBox fl_timeout;
        private HtmlLabel htmlLabel4;
        private HtmlLabel lbl_about;
        private YamuiLabel yamuiLabel2;
        private YamuiButton bt_reset;
        private YamuiButton yamuiButton1;
    }
}
