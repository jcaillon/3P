using System.ComponentModel;
using YamuiFramework.Controls;
using YamuiFramework.HtmlRenderer.WinForms;

namespace _3PA.MainFeatures.Appli.Pages.Set {
    partial class SetEnvironment {
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
            this.mainPanel = new YamuiFramework.Controls.YamuiPanel();
            this.htmlLabel7 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel6 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel5 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel4 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel3 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel2 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel1 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.lblLocally = new YamuiFramework.Controls.YamuiLabel();
            this.multibox3 = new YamuiFramework.Controls.YamuiTextBox();
            this.tgCompilLocl = new YamuiFramework.Controls.YamuiToggle();
            this.btDownload = new YamuiFramework.Controls.YamuiImageButton();
            this.btcontrol2 = new YamuiFramework.Controls.YamuiButton();
            this.btcontrol1 = new YamuiFramework.Controls.YamuiButton();
            this.btright6 = new YamuiFramework.Controls.YamuiImageButton();
            this.btleft6 = new YamuiFramework.Controls.YamuiImageButton();
            this.textbox6 = new YamuiFramework.Controls.YamuiTextBox();
            this.envLabel = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.cbDatabase = new YamuiFramework.Controls.YamuiComboBox();
            this.btright1 = new YamuiFramework.Controls.YamuiImageButton();
            this.btleft1 = new YamuiFramework.Controls.YamuiImageButton();
            this.textbox1 = new YamuiFramework.Controls.YamuiTextBox();
            this.btright5 = new YamuiFramework.Controls.YamuiImageButton();
            this.btleft5 = new YamuiFramework.Controls.YamuiImageButton();
            this.textbox5 = new YamuiFramework.Controls.YamuiTextBox();
            this.btright4 = new YamuiFramework.Controls.YamuiImageButton();
            this.btleft4 = new YamuiFramework.Controls.YamuiImageButton();
            this.textbox4 = new YamuiFramework.Controls.YamuiTextBox();
            this.btright3 = new YamuiFramework.Controls.YamuiImageButton();
            this.btleft3 = new YamuiFramework.Controls.YamuiImageButton();
            this.textbox3 = new YamuiFramework.Controls.YamuiTextBox();
            this.btright2 = new YamuiFramework.Controls.YamuiImageButton();
            this.btleft2 = new YamuiFramework.Controls.YamuiImageButton();
            this.multitextbox1 = new YamuiFramework.Controls.YamuiTextBox();
            this.multibox2 = new YamuiFramework.Controls.YamuiTextBox();
            this.textbox2 = new YamuiFramework.Controls.YamuiTextBox();
            this.yamuiLabel4 = new YamuiFramework.Controls.YamuiLabel();
            this.cbEnvLetter = new YamuiFramework.Controls.YamuiComboBox();
            this.cbAppli = new YamuiFramework.Controls.YamuiComboBox();
            this.yamuiLabel1 = new YamuiFramework.Controls.YamuiLabel();
            this.toolTip = new YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip();
            this.mainPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainPanel
            // 
            this.mainPanel.Controls.Add(this.htmlLabel7);
            this.mainPanel.Controls.Add(this.htmlLabel6);
            this.mainPanel.Controls.Add(this.htmlLabel5);
            this.mainPanel.Controls.Add(this.htmlLabel4);
            this.mainPanel.Controls.Add(this.htmlLabel3);
            this.mainPanel.Controls.Add(this.htmlLabel2);
            this.mainPanel.Controls.Add(this.htmlLabel1);
            this.mainPanel.Controls.Add(this.lblLocally);
            this.mainPanel.Controls.Add(this.multibox3);
            this.mainPanel.Controls.Add(this.tgCompilLocl);
            this.mainPanel.Controls.Add(this.btDownload);
            this.mainPanel.Controls.Add(this.btcontrol2);
            this.mainPanel.Controls.Add(this.btcontrol1);
            this.mainPanel.Controls.Add(this.btright6);
            this.mainPanel.Controls.Add(this.btleft6);
            this.mainPanel.Controls.Add(this.textbox6);
            this.mainPanel.Controls.Add(this.envLabel);
            this.mainPanel.Controls.Add(this.cbDatabase);
            this.mainPanel.Controls.Add(this.btright1);
            this.mainPanel.Controls.Add(this.btleft1);
            this.mainPanel.Controls.Add(this.textbox1);
            this.mainPanel.Controls.Add(this.btright5);
            this.mainPanel.Controls.Add(this.btleft5);
            this.mainPanel.Controls.Add(this.textbox5);
            this.mainPanel.Controls.Add(this.btright4);
            this.mainPanel.Controls.Add(this.btleft4);
            this.mainPanel.Controls.Add(this.textbox4);
            this.mainPanel.Controls.Add(this.btright3);
            this.mainPanel.Controls.Add(this.btleft3);
            this.mainPanel.Controls.Add(this.textbox3);
            this.mainPanel.Controls.Add(this.btright2);
            this.mainPanel.Controls.Add(this.btleft2);
            this.mainPanel.Controls.Add(this.multitextbox1);
            this.mainPanel.Controls.Add(this.multibox2);
            this.mainPanel.Controls.Add(this.textbox2);
            this.mainPanel.Controls.Add(this.yamuiLabel4);
            this.mainPanel.Controls.Add(this.cbEnvLetter);
            this.mainPanel.Controls.Add(this.cbAppli);
            this.mainPanel.Controls.Add(this.yamuiLabel1);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.HorizontalScrollbarHighlightOnWheel = false;
            this.mainPanel.HorizontalScrollbarSize = 10;
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(720, 550);
            this.mainPanel.TabIndex = 0;
            this.mainPanel.VerticalScrollbarHighlightOnWheel = false;
            this.mainPanel.VerticalScrollbarSize = 10;
            // 
            // htmlLabel7
            // 
            this.htmlLabel7.AutoSize = false;
            this.htmlLabel7.AutoSizeHeightOnly = true;
            this.htmlLabel7.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel7.BaseStylesheet = null;
            this.htmlLabel7.IsSelectionEnabled = false;
            this.htmlLabel7.Location = new System.Drawing.Point(30, 465);
            this.htmlLabel7.Name = "htmlLabel7";
            this.htmlLabel7.Size = new System.Drawing.Size(122, 15);
            this.htmlLabel7.TabIndex = 59;
            this.htmlLabel7.Text = "<b>Path to server.log file</b>";
            // 
            // htmlLabel6
            // 
            this.htmlLabel6.AutoSize = false;
            this.htmlLabel6.AutoSizeHeightOnly = true;
            this.htmlLabel6.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel6.BaseStylesheet = null;
            this.htmlLabel6.IsSelectionEnabled = false;
            this.htmlLabel6.Location = new System.Drawing.Point(30, 409);
            this.htmlLabel6.Name = "htmlLabel6";
            this.htmlLabel6.Size = new System.Drawing.Size(122, 30);
            this.htmlLabel6.TabIndex = 58;
            this.htmlLabel6.Text = "<b>Command line extra parameters</b>";
            // 
            // htmlLabel5
            // 
            this.htmlLabel5.AutoSize = false;
            this.htmlLabel5.AutoSizeHeightOnly = true;
            this.htmlLabel5.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel5.BaseStylesheet = null;
            this.htmlLabel5.IsSelectionEnabled = false;
            this.htmlLabel5.Location = new System.Drawing.Point(30, 378);
            this.htmlLabel5.Name = "htmlLabel5";
            this.htmlLabel5.Size = new System.Drawing.Size(122, 15);
            this.htmlLabel5.TabIndex = 57;
            this.htmlLabel5.Text = "<b>Prowin32.exe path</b>";
            // 
            // htmlLabel4
            // 
            this.htmlLabel4.AutoSize = false;
            this.htmlLabel4.AutoSizeHeightOnly = true;
            this.htmlLabel4.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel4.BaseStylesheet = null;
            this.htmlLabel4.IsSelectionEnabled = false;
            this.htmlLabel4.Location = new System.Drawing.Point(30, 324);
            this.htmlLabel4.Name = "htmlLabel4";
            this.htmlLabel4.Size = new System.Drawing.Size(122, 30);
            this.htmlLabel4.TabIndex = 56;
            this.htmlLabel4.Text = "<b>Compilation base directory</b>";
            // 
            // htmlLabel3
            // 
            this.htmlLabel3.AutoSize = false;
            this.htmlLabel3.AutoSizeHeightOnly = true;
            this.htmlLabel3.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel3.BaseStylesheet = null;
            this.htmlLabel3.IsSelectionEnabled = false;
            this.htmlLabel3.Location = new System.Drawing.Point(30, 292);
            this.htmlLabel3.Name = "htmlLabel3";
            this.htmlLabel3.Size = new System.Drawing.Size(122, 15);
            this.htmlLabel3.TabIndex = 55;
            this.htmlLabel3.Text = "<b>Project local directory</b>";
            // 
            // htmlLabel2
            // 
            this.htmlLabel2.AutoSize = false;
            this.htmlLabel2.AutoSizeHeightOnly = true;
            this.htmlLabel2.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel2.BaseStylesheet = null;
            this.htmlLabel2.IsSelectionEnabled = false;
            this.htmlLabel2.Location = new System.Drawing.Point(30, 211);
            this.htmlLabel2.Name = "htmlLabel2";
            this.htmlLabel2.Size = new System.Drawing.Size(122, 60);
            this.htmlLabel2.TabIndex = 54;
            this.htmlLabel2.Text = "<b>ProPath</b><br>Reads the value from the .ini file and adds the directories lis" +
    "ted";
            // 
            // htmlLabel1
            // 
            this.htmlLabel1.AutoSize = false;
            this.htmlLabel1.AutoSizeHeightOnly = true;
            this.htmlLabel1.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel1.BaseStylesheet = null;
            this.htmlLabel1.IsSelectionEnabled = false;
            this.htmlLabel1.Location = new System.Drawing.Point(30, 106);
            this.htmlLabel1.Name = "htmlLabel1";
            this.htmlLabel1.Size = new System.Drawing.Size(122, 30);
            this.htmlLabel1.TabIndex = 53;
            this.htmlLabel1.Text = "<b>Database connection</b> informations";
            // 
            // lblLocally
            // 
            this.lblLocally.AutoSize = true;
            this.lblLocally.Function = YamuiFramework.Fonts.LabelFunction.Small;
            this.lblLocally.Location = new System.Drawing.Point(223, 347);
            this.lblLocally.Margin = new System.Windows.Forms.Padding(3);
            this.lblLocally.Name = "lblLocally";
            this.lblLocally.Size = new System.Drawing.Size(90, 12);
            this.lblLocally.TabIndex = 52;
            this.lblLocally.Text = "Compile files locally";
            this.lblLocally.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // multibox3
            // 
            this.multibox3.DisplayIcon = false;
            this.multibox3.Lines = new string[0];
            this.multibox3.Location = new System.Drawing.Point(190, 409);
            this.multibox3.MaxLength = 32767;
            this.multibox3.Multiline = true;
            this.multibox3.Name = "multibox3";
            this.multibox3.PasswordChar = '\0';
            this.multibox3.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.multibox3.SelectedText = "";
            this.multibox3.Size = new System.Drawing.Size(438, 45);
            this.multibox3.TabIndex = 51;
            this.multibox3.WaterMark = "Appended to the prowin command line when running or compiling";
            // 
            // tgCompilLocl
            // 
            this.tgCompilLocl.AutoSize = true;
            this.tgCompilLocl.Location = new System.Drawing.Point(190, 346);
            this.tgCompilLocl.Name = "tgCompilLocl";
            this.tgCompilLocl.Size = new System.Drawing.Size(52, 15);
            this.tgCompilLocl.TabIndex = 49;
            this.tgCompilLocl.Text = " ";
            // 
            // btDownload
            // 
            this.btDownload.BackColor = System.Drawing.Color.Transparent;
            this.btDownload.BackGrndImage = null;
            this.btDownload.Location = new System.Drawing.Point(631, 128);
            this.btDownload.Margin = new System.Windows.Forms.Padding(0);
            this.btDownload.Name = "btDownload";
            this.btDownload.Size = new System.Drawing.Size(20, 20);
            this.btDownload.TabIndex = 48;
            this.btDownload.Text = "yamuiImageButtonDB";
            this.btDownload.Click += new System.EventHandler(this.btDownload_Click);
            // 
            // btcontrol2
            // 
            this.btcontrol2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btcontrol2.Location = new System.Drawing.Point(521, 524);
            this.btcontrol2.Name = "btcontrol2";
            this.btcontrol2.Size = new System.Drawing.Size(94, 23);
            this.btcontrol2.TabIndex = 47;
            this.btcontrol2.Text = "Modify";
            // 
            // btcontrol1
            // 
            this.btcontrol1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btcontrol1.Location = new System.Drawing.Point(621, 524);
            this.btcontrol1.Name = "btcontrol1";
            this.btcontrol1.Size = new System.Drawing.Size(94, 23);
            this.btcontrol1.TabIndex = 46;
            this.btcontrol1.Text = "Add new";
            // 
            // btright6
            // 
            this.btright6.BackColor = System.Drawing.Color.Transparent;
            this.btright6.BackGrndImage = null;
            this.btright6.Location = new System.Drawing.Point(631, 465);
            this.btright6.Margin = new System.Windows.Forms.Padding(0);
            this.btright6.Name = "btright6";
            this.btright6.Size = new System.Drawing.Size(20, 20);
            this.btright6.TabIndex = 45;
            this.btright6.Text = "yamuiImageButton11";
            // 
            // btleft6
            // 
            this.btleft6.BackColor = System.Drawing.Color.Transparent;
            this.btleft6.BackGrndImage = null;
            this.btleft6.Location = new System.Drawing.Point(167, 465);
            this.btleft6.Margin = new System.Windows.Forms.Padding(0);
            this.btleft6.Name = "btleft6";
            this.btleft6.Size = new System.Drawing.Size(20, 20);
            this.btleft6.TabIndex = 44;
            this.btleft6.Text = "yamuiImageButton12";
            // 
            // textbox6
            // 
            this.textbox6.Lines = new string[0];
            this.textbox6.Location = new System.Drawing.Point(190, 465);
            this.textbox6.MaxLength = 32767;
            this.textbox6.Name = "textbox6";
            this.textbox6.PasswordChar = '\0';
            this.textbox6.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textbox6.SelectedText = "";
            this.textbox6.Size = new System.Drawing.Size(438, 20);
            this.textbox6.TabIndex = 43;
            this.textbox6.Tag = "log file (*.log)|*.log";
            // 
            // envLabel
            // 
            this.envLabel.AutoSize = false;
            this.envLabel.AutoSizeHeightOnly = true;
            this.envLabel.BackColor = System.Drawing.Color.Transparent;
            this.envLabel.BaseStylesheet = null;
            this.envLabel.Location = new System.Drawing.Point(269, 30);
            this.envLabel.Name = "envLabel";
            this.envLabel.Size = new System.Drawing.Size(506, 15);
            this.envLabel.TabIndex = 41;
            this.envLabel.Text = "?";
            // 
            // cbDatabase
            // 
            this.cbDatabase.ItemHeight = 19;
            this.cbDatabase.Location = new System.Drawing.Point(190, 97);
            this.cbDatabase.Name = "cbDatabase";
            this.cbDatabase.Size = new System.Drawing.Size(242, 25);
            this.cbDatabase.TabIndex = 40;
            this.cbDatabase.SelectionChangeCommitted += new System.EventHandler(this.cbDatabase_SelectedIndexChanged);
            // 
            // btright1
            // 
            this.btright1.BackColor = System.Drawing.Color.Transparent;
            this.btright1.BackGrndImage = null;
            this.btright1.Location = new System.Drawing.Point(611, 128);
            this.btright1.Margin = new System.Windows.Forms.Padding(0);
            this.btright1.Name = "btright1";
            this.btright1.Size = new System.Drawing.Size(20, 20);
            this.btright1.TabIndex = 39;
            this.btright1.Text = "yamuiImageButton9";
            // 
            // btleft1
            // 
            this.btleft1.BackColor = System.Drawing.Color.Transparent;
            this.btleft1.BackGrndImage = null;
            this.btleft1.Location = new System.Drawing.Point(167, 128);
            this.btleft1.Margin = new System.Windows.Forms.Padding(0);
            this.btleft1.Name = "btleft1";
            this.btleft1.Size = new System.Drawing.Size(20, 20);
            this.btleft1.TabIndex = 38;
            this.btleft1.Text = "yamuiImageButton10";
            // 
            // textbox1
            // 
            this.textbox1.Lines = new string[0];
            this.textbox1.Location = new System.Drawing.Point(190, 128);
            this.textbox1.MaxLength = 32767;
            this.textbox1.Name = "textbox1";
            this.textbox1.PasswordChar = '\0';
            this.textbox1.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textbox1.SelectedText = "";
            this.textbox1.Size = new System.Drawing.Size(418, 20);
            this.textbox1.TabIndex = 37;
            this.textbox1.Tag = "pf file (*.pf)|*.pf";
            // 
            // btright5
            // 
            this.btright5.BackColor = System.Drawing.Color.Transparent;
            this.btright5.BackGrndImage = null;
            this.btright5.Location = new System.Drawing.Point(631, 378);
            this.btright5.Margin = new System.Windows.Forms.Padding(0);
            this.btright5.Name = "btright5";
            this.btright5.Size = new System.Drawing.Size(20, 20);
            this.btright5.TabIndex = 36;
            this.btright5.Text = "yamuiImageButton7";
            // 
            // btleft5
            // 
            this.btleft5.BackColor = System.Drawing.Color.Transparent;
            this.btleft5.BackGrndImage = null;
            this.btleft5.Location = new System.Drawing.Point(167, 378);
            this.btleft5.Margin = new System.Windows.Forms.Padding(0);
            this.btleft5.Name = "btleft5";
            this.btleft5.Size = new System.Drawing.Size(20, 20);
            this.btleft5.TabIndex = 35;
            this.btleft5.Text = "yamuiImageButton8";
            // 
            // textbox5
            // 
            this.textbox5.Lines = new string[0];
            this.textbox5.Location = new System.Drawing.Point(190, 378);
            this.textbox5.MaxLength = 32767;
            this.textbox5.Name = "textbox5";
            this.textbox5.PasswordChar = '\0';
            this.textbox5.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textbox5.SelectedText = "";
            this.textbox5.Size = new System.Drawing.Size(438, 20);
            this.textbox5.TabIndex = 34;
            this.textbox5.Tag = "prowin32 (*.exe)|*.exe";
            // 
            // btright4
            // 
            this.btright4.BackColor = System.Drawing.Color.Transparent;
            this.btright4.BackGrndImage = null;
            this.btright4.Location = new System.Drawing.Point(631, 323);
            this.btright4.Margin = new System.Windows.Forms.Padding(0);
            this.btright4.Name = "btright4";
            this.btright4.Size = new System.Drawing.Size(20, 20);
            this.btright4.TabIndex = 33;
            this.btright4.Text = "yamuiImageButton5";
            // 
            // btleft4
            // 
            this.btleft4.BackColor = System.Drawing.Color.Transparent;
            this.btleft4.BackGrndImage = null;
            this.btleft4.Location = new System.Drawing.Point(167, 323);
            this.btleft4.Margin = new System.Windows.Forms.Padding(0);
            this.btleft4.Name = "btleft4";
            this.btleft4.Size = new System.Drawing.Size(20, 20);
            this.btleft4.TabIndex = 32;
            this.btleft4.Text = "yamuiImageButton6";
            // 
            // textbox4
            // 
            this.textbox4.Lines = new string[0];
            this.textbox4.Location = new System.Drawing.Point(190, 323);
            this.textbox4.MaxLength = 32767;
            this.textbox4.Name = "textbox4";
            this.textbox4.PasswordChar = '\0';
            this.textbox4.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textbox4.SelectedText = "";
            this.textbox4.Size = new System.Drawing.Size(438, 20);
            this.textbox4.TabIndex = 31;
            this.textbox4.Tag = "true";
            // 
            // btright3
            // 
            this.btright3.BackColor = System.Drawing.Color.Transparent;
            this.btright3.BackGrndImage = null;
            this.btright3.Location = new System.Drawing.Point(631, 292);
            this.btright3.Margin = new System.Windows.Forms.Padding(0);
            this.btright3.Name = "btright3";
            this.btright3.Size = new System.Drawing.Size(20, 20);
            this.btright3.TabIndex = 30;
            this.btright3.Text = "yamuiImageButton3";
            // 
            // btleft3
            // 
            this.btleft3.BackColor = System.Drawing.Color.Transparent;
            this.btleft3.BackGrndImage = null;
            this.btleft3.Location = new System.Drawing.Point(167, 292);
            this.btleft3.Margin = new System.Windows.Forms.Padding(0);
            this.btleft3.Name = "btleft3";
            this.btleft3.Size = new System.Drawing.Size(20, 20);
            this.btleft3.TabIndex = 29;
            this.btleft3.Text = "yamuiImageButton4";
            // 
            // textbox3
            // 
            this.textbox3.Lines = new string[0];
            this.textbox3.Location = new System.Drawing.Point(190, 292);
            this.textbox3.MaxLength = 32767;
            this.textbox3.Name = "textbox3";
            this.textbox3.PasswordChar = '\0';
            this.textbox3.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textbox3.SelectedText = "";
            this.textbox3.Size = new System.Drawing.Size(438, 20);
            this.textbox3.TabIndex = 28;
            this.textbox3.Tag = "true";
            // 
            // btright2
            // 
            this.btright2.BackColor = System.Drawing.Color.Transparent;
            this.btright2.BackGrndImage = null;
            this.btright2.Location = new System.Drawing.Point(631, 210);
            this.btright2.Margin = new System.Windows.Forms.Padding(0);
            this.btright2.Name = "btright2";
            this.btright2.Size = new System.Drawing.Size(20, 20);
            this.btright2.TabIndex = 27;
            this.btright2.Text = "yamuiImageButton2";
            // 
            // btleft2
            // 
            this.btleft2.BackColor = System.Drawing.Color.Transparent;
            this.btleft2.BackGrndImage = null;
            this.btleft2.Location = new System.Drawing.Point(167, 210);
            this.btleft2.Margin = new System.Windows.Forms.Padding(0);
            this.btleft2.Name = "btleft2";
            this.btleft2.Size = new System.Drawing.Size(20, 20);
            this.btleft2.TabIndex = 26;
            this.btleft2.Text = "yamuiImageButton1";
            // 
            // multitextbox1
            // 
            this.multitextbox1.DisplayIcon = false;
            this.multitextbox1.Lines = new string[0];
            this.multitextbox1.Location = new System.Drawing.Point(190, 154);
            this.multitextbox1.MaxLength = 32767;
            this.multitextbox1.Multiline = true;
            this.multitextbox1.Name = "multitextbox1";
            this.multitextbox1.PasswordChar = '\0';
            this.multitextbox1.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.multitextbox1.SelectedText = "";
            this.multitextbox1.Size = new System.Drawing.Size(438, 45);
            this.multitextbox1.TabIndex = 15;
            this.multitextbox1.WaterMark = "Appended to the .pf file";
            // 
            // multibox2
            // 
            this.multibox2.DisplayIcon = false;
            this.multibox2.Lines = new string[0];
            this.multibox2.Location = new System.Drawing.Point(190, 236);
            this.multibox2.MaxLength = 32767;
            this.multibox2.Multiline = true;
            this.multibox2.Name = "multibox2";
            this.multibox2.PasswordChar = '\0';
            this.multibox2.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.multibox2.SelectedText = "";
            this.multibox2.Size = new System.Drawing.Size(438, 45);
            this.multibox2.TabIndex = 11;
            this.multibox2.WaterMark = "Appended to the .ini PROPATH (comma separated list)";
            // 
            // textbox2
            // 
            this.textbox2.Lines = new string[0];
            this.textbox2.Location = new System.Drawing.Point(190, 210);
            this.textbox2.MaxLength = 32767;
            this.textbox2.Name = "textbox2";
            this.textbox2.PasswordChar = '\0';
            this.textbox2.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textbox2.SelectedText = "";
            this.textbox2.Size = new System.Drawing.Size(438, 20);
            this.textbox2.TabIndex = 9;
            this.textbox2.Tag = "ini file (*.ini)|*.ini";
            // 
            // yamuiLabel4
            // 
            this.yamuiLabel4.AutoSize = true;
            this.yamuiLabel4.Function = YamuiFramework.Fonts.LabelFunction.Heading;
            this.yamuiLabel4.Location = new System.Drawing.Point(0, 71);
            this.yamuiLabel4.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel4.Name = "yamuiLabel4";
            this.yamuiLabel4.Size = new System.Drawing.Size(63, 19);
            this.yamuiLabel4.TabIndex = 7;
            this.yamuiLabel4.Text = "DETAILS";
            // 
            // cbEnvLetter
            // 
            this.cbEnvLetter.ItemHeight = 19;
            this.cbEnvLetter.Location = new System.Drawing.Point(205, 25);
            this.cbEnvLetter.Name = "cbEnvLetter";
            this.cbEnvLetter.Size = new System.Drawing.Size(51, 25);
            this.cbEnvLetter.TabIndex = 6;
            this.cbEnvLetter.SelectionChangeCommitted += new System.EventHandler(this.cbEnvLetter_SelectedIndexChanged);
            // 
            // cbAppli
            // 
            this.cbAppli.ItemHeight = 19;
            this.cbAppli.Location = new System.Drawing.Point(30, 25);
            this.cbAppli.Name = "cbAppli";
            this.cbAppli.Size = new System.Drawing.Size(169, 25);
            this.cbAppli.TabIndex = 3;
            this.cbAppli.SelectionChangeCommitted += new System.EventHandler(this.cbAppli_SelectedIndexChanged);
            // 
            // yamuiLabel1
            // 
            this.yamuiLabel1.AutoSize = true;
            this.yamuiLabel1.Function = YamuiFramework.Fonts.LabelFunction.Heading;
            this.yamuiLabel1.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel1.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel1.Name = "yamuiLabel1";
            this.yamuiLabel1.Size = new System.Drawing.Size(81, 19);
            this.yamuiLabel1.TabIndex = 2;
            this.yamuiLabel1.Text = "SELECTION";
            // 
            // toolTip
            // 
            this.toolTip.AllowLinksHandling = true;
            this.toolTip.BaseStylesheet = null;
            this.toolTip.MaximumSize = new System.Drawing.Size(0, 0);
            this.toolTip.OwnerDraw = true;
            this.toolTip.TooltipCssClass = "htmltooltip";
            // 
            // SetEnvironment
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mainPanel);
            this.Name = "SetEnvironment";
            this.Size = new System.Drawing.Size(720, 550);
            this.mainPanel.ResumeLayout(false);
            this.mainPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiPanel mainPanel;
        private YamuiLabel yamuiLabel1;
        private YamuiComboBox cbEnvLetter;
        private YamuiComboBox cbAppli;
        private YamuiLabel yamuiLabel4;
        private YamuiTextBox textbox2;
        private YamuiTextBox multibox2;
        private YamuiTextBox multitextbox1;
        private YamuiImageButton btright2;
        private YamuiImageButton btleft2;
        private YamuiImageButton btright1;
        private YamuiImageButton btleft1;
        private YamuiTextBox textbox1;
        private YamuiImageButton btright5;
        private YamuiImageButton btleft5;
        private YamuiTextBox textbox5;
        private YamuiImageButton btright4;
        private YamuiImageButton btleft4;
        private YamuiTextBox textbox4;
        private YamuiImageButton btright3;
        private YamuiImageButton btleft3;
        private YamuiTextBox textbox3;
        private YamuiComboBox cbDatabase;
        private HtmlLabel envLabel;
        private YamuiImageButton btright6;
        private YamuiImageButton btleft6;
        private YamuiTextBox textbox6;
        private YamuiButton btcontrol2;
        private YamuiButton btcontrol1;
        private HtmlToolTip toolTip;
        private YamuiImageButton btDownload;
        private YamuiToggle tgCompilLocl;
        private YamuiTextBox multibox3;
        private YamuiLabel lblLocally;
        private HtmlLabel htmlLabel1;
        private HtmlLabel htmlLabel2;
        private HtmlLabel htmlLabel3;
        private HtmlLabel htmlLabel4;
        private HtmlLabel htmlLabel5;
        private HtmlLabel htmlLabel6;
        private HtmlLabel htmlLabel7;
    }
}
