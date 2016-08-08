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
            this.toolTip = new YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip();
            this.scrollPanel = new YamuiFramework.Controls.YamuiScrollPanel();
            this.htmlLabel10 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel9 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel1 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.btDbView = new YamuiFramework.Controls.YamuiButton();
            this.btDbDeleteDownload = new YamuiFramework.Controls.YamuiButton();
            this.btDbDownload = new YamuiFramework.Controls.YamuiButton();
            this.yamuiLabel1 = new YamuiFramework.Controls.YamuiLabel();
            this.linkurl = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.flSuffix = new YamuiFramework.Controls.YamuiTextBox();
            this.flLabel = new YamuiFramework.Controls.YamuiTextBox();
            this.txLabel = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.yamuiLabel4 = new YamuiFramework.Controls.YamuiLabel();
            this.lbl_listdb = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.flDatabase = new YamuiFramework.Controls.YamuiTextBox();
            this.cbDatabase = new YamuiFramework.Controls.YamuiComboBox();
            this.btleft1 = new YamuiFramework.Controls.YamuiButtonImage();
            this.textbox1 = new YamuiFramework.Controls.YamuiTextBox();
            this.btright1 = new YamuiFramework.Controls.YamuiButtonImage();
            this.yamuiLabel3 = new YamuiFramework.Controls.YamuiLabel();
            this.htmlLabel8 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.flExtraPf = new YamuiFramework.Controls.YamuiTextBox();
            this.htmlLabel2 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.btleft2 = new YamuiFramework.Controls.YamuiButtonImage();
            this.btright2 = new YamuiFramework.Controls.YamuiButtonImage();
            this.textbox2 = new YamuiFramework.Controls.YamuiTextBox();
            this.flExtraProPath = new YamuiFramework.Controls.YamuiTextBox();
            this.htmlLabel3 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.btleft3 = new YamuiFramework.Controls.YamuiButtonImage();
            this.textbox3 = new YamuiFramework.Controls.YamuiTextBox();
            this.btright3 = new YamuiFramework.Controls.YamuiButtonImage();
            this.htmlLabel4 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.btleft4 = new YamuiFramework.Controls.YamuiButtonImage();
            this.textbox4 = new YamuiFramework.Controls.YamuiTextBox();
            this.btright4 = new YamuiFramework.Controls.YamuiButtonImage();
            this.tgCompLocally = new YamuiFramework.Controls.YamuiButtonToggle();
            this.tgCompWithLst = new YamuiFramework.Controls.YamuiButtonToggle();
            this.lblLocally = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLabel5 = new YamuiFramework.Controls.YamuiLabel();
            this.htmlLabel5 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.btleft5 = new YamuiFramework.Controls.YamuiButtonImage();
            this.textbox5 = new YamuiFramework.Controls.YamuiTextBox();
            this.btright5 = new YamuiFramework.Controls.YamuiButtonImage();
            this.htmlLabel6 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.flCmdLine = new YamuiFramework.Controls.YamuiTextBox();
            this.htmlLabel7 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.btleft6 = new YamuiFramework.Controls.YamuiButtonImage();
            this.textbox6 = new YamuiFramework.Controls.YamuiTextBox();
            this.btright6 = new YamuiFramework.Controls.YamuiButtonImage();
            this.btDelete = new YamuiFramework.Controls.YamuiButton();
            this.flName = new YamuiFramework.Controls.YamuiTextBox();
            this.btEdit = new YamuiFramework.Controls.YamuiButton();
            this.btAdd = new YamuiFramework.Controls.YamuiButton();
            this.btCopy = new YamuiFramework.Controls.YamuiButton();
            this.btDbEdit = new YamuiFramework.Controls.YamuiButton();
            this.btDbAdd = new YamuiFramework.Controls.YamuiButton();
            this.btDbDelete = new YamuiFramework.Controls.YamuiButton();
            this.btSave = new YamuiFramework.Controls.YamuiButton();
            this.btCancel = new YamuiFramework.Controls.YamuiButton();
            this.btDbCancel = new YamuiFramework.Controls.YamuiButton();
            this.btDbSave = new YamuiFramework.Controls.YamuiButton();
            this.areaDb = new YamuiFramework.Controls.YamuiArea();
            this.cbSuffix = new YamuiFramework.Controls.YamuiComboBox();
            this.cbName = new YamuiFramework.Controls.YamuiComboBox();
            this.areaLeftButtons = new YamuiFramework.Controls.YamuiArea();
            this.areaEnv = new YamuiFramework.Controls.YamuiArea();
            this.areaPf = new YamuiFramework.Controls.YamuiArea();
            this.scrollPanel.ContentPanel.SuspendLayout();
            this.scrollPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolTip
            // 
            this.toolTip.AllowLinksHandling = true;
            this.toolTip.AutomaticDelay = 100;
            this.toolTip.AutoPopDelay = 90000;
            this.toolTip.BaseStylesheet = null;
            this.toolTip.InitialDelay = 100;
            this.toolTip.MaximumSize = new System.Drawing.Size(0, 0);
            this.toolTip.OwnerDraw = true;
            this.toolTip.ReshowDelay = 20;
            // 
            // scrollPanel
            // 
            // 
            // scrollPanel.ContentPanel
            // 
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel10);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel9);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel1);
            this.scrollPanel.ContentPanel.Controls.Add(this.btDbView);
            this.scrollPanel.ContentPanel.Controls.Add(this.btDbDeleteDownload);
            this.scrollPanel.ContentPanel.Controls.Add(this.btDbDownload);
            this.scrollPanel.ContentPanel.Controls.Add(this.yamuiLabel1);
            this.scrollPanel.ContentPanel.Controls.Add(this.linkurl);
            this.scrollPanel.ContentPanel.Controls.Add(this.flSuffix);
            this.scrollPanel.ContentPanel.Controls.Add(this.flLabel);
            this.scrollPanel.ContentPanel.Controls.Add(this.txLabel);
            this.scrollPanel.ContentPanel.Controls.Add(this.yamuiLabel4);
            this.scrollPanel.ContentPanel.Controls.Add(this.lbl_listdb);
            this.scrollPanel.ContentPanel.Controls.Add(this.flDatabase);
            this.scrollPanel.ContentPanel.Controls.Add(this.cbDatabase);
            this.scrollPanel.ContentPanel.Controls.Add(this.btleft1);
            this.scrollPanel.ContentPanel.Controls.Add(this.textbox1);
            this.scrollPanel.ContentPanel.Controls.Add(this.btright1);
            this.scrollPanel.ContentPanel.Controls.Add(this.yamuiLabel3);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel8);
            this.scrollPanel.ContentPanel.Controls.Add(this.flExtraPf);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel2);
            this.scrollPanel.ContentPanel.Controls.Add(this.btleft2);
            this.scrollPanel.ContentPanel.Controls.Add(this.btright2);
            this.scrollPanel.ContentPanel.Controls.Add(this.textbox2);
            this.scrollPanel.ContentPanel.Controls.Add(this.flExtraProPath);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel3);
            this.scrollPanel.ContentPanel.Controls.Add(this.btleft3);
            this.scrollPanel.ContentPanel.Controls.Add(this.textbox3);
            this.scrollPanel.ContentPanel.Controls.Add(this.btright3);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel4);
            this.scrollPanel.ContentPanel.Controls.Add(this.btleft4);
            this.scrollPanel.ContentPanel.Controls.Add(this.textbox4);
            this.scrollPanel.ContentPanel.Controls.Add(this.btright4);
            this.scrollPanel.ContentPanel.Controls.Add(this.tgCompLocally);
            this.scrollPanel.ContentPanel.Controls.Add(this.tgCompWithLst);
            this.scrollPanel.ContentPanel.Controls.Add(this.lblLocally);
            this.scrollPanel.ContentPanel.Controls.Add(this.yamuiLabel5);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel5);
            this.scrollPanel.ContentPanel.Controls.Add(this.btleft5);
            this.scrollPanel.ContentPanel.Controls.Add(this.textbox5);
            this.scrollPanel.ContentPanel.Controls.Add(this.btright5);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel6);
            this.scrollPanel.ContentPanel.Controls.Add(this.flCmdLine);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel7);
            this.scrollPanel.ContentPanel.Controls.Add(this.btleft6);
            this.scrollPanel.ContentPanel.Controls.Add(this.textbox6);
            this.scrollPanel.ContentPanel.Controls.Add(this.btright6);
            this.scrollPanel.ContentPanel.Controls.Add(this.btDelete);
            this.scrollPanel.ContentPanel.Controls.Add(this.flName);
            this.scrollPanel.ContentPanel.Controls.Add(this.btEdit);
            this.scrollPanel.ContentPanel.Controls.Add(this.btAdd);
            this.scrollPanel.ContentPanel.Controls.Add(this.btCopy);
            this.scrollPanel.ContentPanel.Controls.Add(this.btDbEdit);
            this.scrollPanel.ContentPanel.Controls.Add(this.btDbAdd);
            this.scrollPanel.ContentPanel.Controls.Add(this.btDbDelete);
            this.scrollPanel.ContentPanel.Controls.Add(this.btSave);
            this.scrollPanel.ContentPanel.Controls.Add(this.btCancel);
            this.scrollPanel.ContentPanel.Controls.Add(this.btDbCancel);
            this.scrollPanel.ContentPanel.Controls.Add(this.btDbSave);
            this.scrollPanel.ContentPanel.Controls.Add(this.areaDb);
            this.scrollPanel.ContentPanel.Controls.Add(this.cbSuffix);
            this.scrollPanel.ContentPanel.Controls.Add(this.cbName);
            this.scrollPanel.ContentPanel.Controls.Add(this.areaLeftButtons);
            this.scrollPanel.ContentPanel.Controls.Add(this.areaEnv);
            this.scrollPanel.ContentPanel.Controls.Add(this.areaPf);
            this.scrollPanel.ContentPanel.Location = new System.Drawing.Point(0, 0);
            this.scrollPanel.ContentPanel.Name = "ContentPanel";
            this.scrollPanel.ContentPanel.OwnerPanel = this.scrollPanel;
            this.scrollPanel.ContentPanel.Size = new System.Drawing.Size(900, 650);
            this.scrollPanel.ContentPanel.TabIndex = 0;
            this.scrollPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scrollPanel.Location = new System.Drawing.Point(0, 0);
            this.scrollPanel.Name = "scrollPanel";
            this.scrollPanel.Size = new System.Drawing.Size(900, 650);
            this.scrollPanel.TabIndex = 0;
            // 
            // htmlLabel10
            // 
            this.htmlLabel10.AutoSize = false;
            this.htmlLabel10.AutoSizeHeightOnly = true;
            this.htmlLabel10.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel10.BaseStylesheet = null;
            this.htmlLabel10.IsSelectionEnabled = false;
            this.htmlLabel10.Location = new System.Drawing.Point(173, 29);
            this.htmlLabel10.Name = "htmlLabel10";
            this.htmlLabel10.Size = new System.Drawing.Size(122, 15);
            this.htmlLabel10.TabIndex = 171;
            this.htmlLabel10.TabStop = false;
            this.htmlLabel10.Text = "<b>Application suffix</b>";
            // 
            // htmlLabel9
            // 
            this.htmlLabel9.AutoSize = false;
            this.htmlLabel9.AutoSizeHeightOnly = true;
            this.htmlLabel9.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel9.BaseStylesheet = null;
            this.htmlLabel9.IsSelectionEnabled = false;
            this.htmlLabel9.Location = new System.Drawing.Point(30, 29);
            this.htmlLabel9.Name = "htmlLabel9";
            this.htmlLabel9.Size = new System.Drawing.Size(122, 15);
            this.htmlLabel9.TabIndex = 170;
            this.htmlLabel9.TabStop = false;
            this.htmlLabel9.Text = "<b>Application name</b>";
            // 
            // htmlLabel1
            // 
            this.htmlLabel1.AutoSize = false;
            this.htmlLabel1.AutoSizeHeightOnly = true;
            this.htmlLabel1.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel1.BaseStylesheet = null;
            this.htmlLabel1.IsSelectionEnabled = false;
            this.htmlLabel1.Location = new System.Drawing.Point(30, 361);
            this.htmlLabel1.Name = "htmlLabel1";
            this.htmlLabel1.Size = new System.Drawing.Size(137, 15);
            this.htmlLabel1.TabIndex = 169;
            this.htmlLabel1.TabStop = false;
            this.htmlLabel1.Text = "<b>Compilation options</b>";
            // 
            // btDbView
            // 
            this.btDbView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btDbView.BackGrndImage = null;
            this.btDbView.Location = new System.Drawing.Point(812, 159);
            this.btDbView.Name = "btDbView";
            this.btDbView.SetImgSize = new System.Drawing.Size(20, 20);
            this.btDbView.Size = new System.Drawing.Size(59, 24);
            this.btDbView.TabIndex = 164;
            this.btDbView.Text = "View";
            this.btDbView.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btDbDeleteDownload
            // 
            this.btDbDeleteDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btDbDeleteDownload.BackGrndImage = null;
            this.btDbDeleteDownload.Location = new System.Drawing.Point(737, 159);
            this.btDbDeleteDownload.Name = "btDbDeleteDownload";
            this.btDbDeleteDownload.SetImgSize = new System.Drawing.Size(20, 20);
            this.btDbDeleteDownload.Size = new System.Drawing.Size(69, 24);
            this.btDbDeleteDownload.TabIndex = 163;
            this.btDbDeleteDownload.Text = "Delete";
            this.btDbDeleteDownload.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btDbDownload
            // 
            this.btDbDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btDbDownload.BackGrndImage = null;
            this.btDbDownload.Location = new System.Drawing.Point(679, 159);
            this.btDbDownload.Name = "btDbDownload";
            this.btDbDownload.SetImgSize = new System.Drawing.Size(20, 20);
            this.btDbDownload.Size = new System.Drawing.Size(52, 24);
            this.btDbDownload.TabIndex = 162;
            this.btDbDownload.Text = "Get";
            this.btDbDownload.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // yamuiLabel1
            // 
            this.yamuiLabel1.AutoSize = true;
            this.yamuiLabel1.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel1.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel1.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel1.Name = "yamuiLabel1";
            this.yamuiLabel1.Size = new System.Drawing.Size(186, 19);
            this.yamuiLabel1.TabIndex = 94;
            this.yamuiLabel1.Text = "ENVIRONMENT SELECTION";
            // 
            // linkurl
            // 
            this.linkurl.BackColor = System.Drawing.Color.Transparent;
            this.linkurl.BaseStylesheet = null;
            this.linkurl.Location = new System.Drawing.Point(232, 2);
            this.linkurl.Name = "linkurl";
            this.linkurl.Size = new System.Drawing.Size(184, 15);
            this.linkurl.TabIndex = 145;
            this.linkurl.TabStop = false;
            this.linkurl.Text = "How to set up a new environment?";
            // 
            // flSuffix
            // 
            this.flSuffix.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.flSuffix.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.flSuffix.CustomBackColor = System.Drawing.Color.Empty;
            this.flSuffix.CustomForeColor = System.Drawing.Color.Empty;
            this.flSuffix.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.flSuffix.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.flSuffix.Location = new System.Drawing.Point(173, 48);
            this.flSuffix.Name = "flSuffix";
            this.flSuffix.Size = new System.Drawing.Size(133, 21);
            this.flSuffix.TabIndex = 135;
            this.flSuffix.WaterMark = "Suffix";
            // 
            // flLabel
            // 
            this.flLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.flLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.flLabel.CustomBackColor = System.Drawing.Color.Empty;
            this.flLabel.CustomForeColor = System.Drawing.Color.Empty;
            this.flLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.flLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.flLabel.Location = new System.Drawing.Point(312, 48);
            this.flLabel.Name = "flLabel";
            this.flLabel.Size = new System.Drawing.Size(566, 21);
            this.flLabel.TabIndex = 134;
            this.flLabel.WaterMark = "Label for this environment";
            // 
            // txLabel
            // 
            this.txLabel.AutoSize = false;
            this.txLabel.AutoSizeHeightOnly = true;
            this.txLabel.BackColor = System.Drawing.Color.Transparent;
            this.txLabel.BaseStylesheet = null;
            this.txLabel.Location = new System.Drawing.Point(312, 51);
            this.txLabel.Name = "txLabel";
            this.txLabel.Size = new System.Drawing.Size(386, 15);
            this.txLabel.TabIndex = 106;
            this.txLabel.TabStop = false;
            this.txLabel.Text = "?";
            // 
            // yamuiLabel4
            // 
            this.yamuiLabel4.AutoSize = true;
            this.yamuiLabel4.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel4.Location = new System.Drawing.Point(0, 83);
            this.yamuiLabel4.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel4.Name = "yamuiLabel4";
            this.yamuiLabel4.Size = new System.Drawing.Size(191, 19);
            this.yamuiLabel4.TabIndex = 95;
            this.yamuiLabel4.Text = "DETAILS OF THE SELECTION";
            // 
            // lbl_listdb
            // 
            this.lbl_listdb.AutoSize = false;
            this.lbl_listdb.AutoSizeHeightOnly = true;
            this.lbl_listdb.BackColor = System.Drawing.Color.Transparent;
            this.lbl_listdb.BaseStylesheet = null;
            this.lbl_listdb.IsSelectionEnabled = false;
            this.lbl_listdb.Location = new System.Drawing.Point(30, 113);
            this.lbl_listdb.Name = "lbl_listdb";
            this.lbl_listdb.Size = new System.Drawing.Size(122, 45);
            this.lbl_listdb.TabIndex = 110;
            this.lbl_listdb.TabStop = false;
            this.lbl_listdb.Text = "<b>List of databases (.pf)</b><br>associated to the current environment";
            // 
            // flDatabase
            // 
            this.flDatabase.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.flDatabase.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.flDatabase.CustomBackColor = System.Drawing.Color.Empty;
            this.flDatabase.CustomForeColor = System.Drawing.Color.Empty;
            this.flDatabase.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.flDatabase.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.flDatabase.Location = new System.Drawing.Point(193, 113);
            this.flDatabase.Name = "flDatabase";
            this.flDatabase.Size = new System.Drawing.Size(273, 21);
            this.flDatabase.TabIndex = 133;
            this.flDatabase.WaterMark = "Set a name for this database connection";
            // 
            // cbDatabase
            // 
            this.cbDatabase.ItemHeight = 15;
            this.cbDatabase.Location = new System.Drawing.Point(193, 113);
            this.cbDatabase.Name = "cbDatabase";
            this.cbDatabase.Size = new System.Drawing.Size(273, 21);
            this.cbDatabase.TabIndex = 139;
            // 
            // btleft1
            // 
            this.btleft1.BackColor = System.Drawing.Color.Transparent;
            this.btleft1.BackGrndImage = null;
            this.btleft1.Location = new System.Drawing.Point(170, 136);
            this.btleft1.Margin = new System.Windows.Forms.Padding(0);
            this.btleft1.Name = "btleft1";
            this.btleft1.SetImgSize = new System.Drawing.Size(0, 0);
            this.btleft1.Size = new System.Drawing.Size(20, 20);
            this.btleft1.TabIndex = 104;
            this.btleft1.TabStop = false;
            this.btleft1.Text = "yamuiImageButton10";
            // 
            // textbox1
            // 
            this.textbox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textbox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.textbox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textbox1.CustomBackColor = System.Drawing.Color.Empty;
            this.textbox1.CustomForeColor = System.Drawing.Color.Empty;
            this.textbox1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.textbox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.textbox1.Location = new System.Drawing.Point(193, 136);
            this.textbox1.Name = "textbox1";
            this.textbox1.Size = new System.Drawing.Size(678, 20);
            this.textbox1.TabIndex = 132;
            this.textbox1.Tag = "pf file (*.pf)|*.pf";
            this.textbox1.WaterMark = "Path to your .pf file (containing database connection info)";
            // 
            // btright1
            // 
            this.btright1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btright1.BackColor = System.Drawing.Color.Transparent;
            this.btright1.BackGrndImage = null;
            this.btright1.Location = new System.Drawing.Point(874, 136);
            this.btright1.Margin = new System.Windows.Forms.Padding(0);
            this.btright1.Name = "btright1";
            this.btright1.SetImgSize = new System.Drawing.Size(0, 0);
            this.btright1.Size = new System.Drawing.Size(20, 20);
            this.btright1.TabIndex = 105;
            this.btright1.TabStop = false;
            this.btright1.Text = "yamuiImageButton9";
            // 
            // yamuiLabel3
            // 
            this.yamuiLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.yamuiLabel3.AutoSize = true;
            this.yamuiLabel3.Function = YamuiFramework.Fonts.FontFunction.Small;
            this.yamuiLabel3.Location = new System.Drawing.Point(610, 165);
            this.yamuiLabel3.Margin = new System.Windows.Forms.Padding(3);
            this.yamuiLabel3.Name = "yamuiLabel3";
            this.yamuiLabel3.Size = new System.Drawing.Size(64, 12);
            this.yamuiLabel3.TabIndex = 152;
            this.yamuiLabel3.Text = "DB structure :";
            this.yamuiLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // htmlLabel8
            // 
            this.htmlLabel8.AutoSize = false;
            this.htmlLabel8.AutoSizeHeightOnly = true;
            this.htmlLabel8.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel8.BaseStylesheet = null;
            this.htmlLabel8.IsSelectionEnabled = false;
            this.htmlLabel8.Location = new System.Drawing.Point(30, 187);
            this.htmlLabel8.Name = "htmlLabel8";
            this.htmlLabel8.Size = new System.Drawing.Size(154, 60);
            this.htmlLabel8.TabIndex = 142;
            this.htmlLabel8.TabStop = false;
            this.htmlLabel8.Text = "<b>Extra connection</b><br>Set another connection<br>(independant from the databa" +
    "se chosen)";
            // 
            // flExtraPf
            // 
            this.flExtraPf.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flExtraPf.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.flExtraPf.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.flExtraPf.CustomBackColor = System.Drawing.Color.Empty;
            this.flExtraPf.CustomForeColor = System.Drawing.Color.Empty;
            this.flExtraPf.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.flExtraPf.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.flExtraPf.Location = new System.Drawing.Point(193, 187);
            this.flExtraPf.Margin = new System.Windows.Forms.Padding(1);
            this.flExtraPf.MultiLines = true;
            this.flExtraPf.Name = "flExtraPf";
            this.flExtraPf.Size = new System.Drawing.Size(678, 60);
            this.flExtraPf.TabIndex = 131;
            this.flExtraPf.WaterMark = "Extra connection info";
            // 
            // htmlLabel2
            // 
            this.htmlLabel2.AutoSize = false;
            this.htmlLabel2.AutoSizeHeightOnly = true;
            this.htmlLabel2.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel2.BaseStylesheet = null;
            this.htmlLabel2.IsSelectionEnabled = false;
            this.htmlLabel2.Location = new System.Drawing.Point(30, 252);
            this.htmlLabel2.Name = "htmlLabel2";
            this.htmlLabel2.Size = new System.Drawing.Size(122, 60);
            this.htmlLabel2.TabIndex = 111;
            this.htmlLabel2.TabStop = false;
            this.htmlLabel2.Text = "<b>Ini and ProPath</b><br>Use an .ini file and add extra directories / .pl to the" +
    " ProPath";
            // 
            // btleft2
            // 
            this.btleft2.BackColor = System.Drawing.Color.Transparent;
            this.btleft2.BackGrndImage = null;
            this.btleft2.Location = new System.Drawing.Point(170, 252);
            this.btleft2.Margin = new System.Windows.Forms.Padding(0);
            this.btleft2.Name = "btleft2";
            this.btleft2.SetImgSize = new System.Drawing.Size(0, 0);
            this.btleft2.Size = new System.Drawing.Size(20, 20);
            this.btleft2.TabIndex = 96;
            this.btleft2.TabStop = false;
            this.btleft2.Text = "yamuiImageButton1";
            // 
            // btright2
            // 
            this.btright2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btright2.BackColor = System.Drawing.Color.Transparent;
            this.btright2.BackGrndImage = null;
            this.btright2.Location = new System.Drawing.Point(874, 252);
            this.btright2.Margin = new System.Windows.Forms.Padding(0);
            this.btright2.Name = "btright2";
            this.btright2.SetImgSize = new System.Drawing.Size(0, 0);
            this.btright2.Size = new System.Drawing.Size(20, 20);
            this.btright2.TabIndex = 97;
            this.btright2.TabStop = false;
            this.btright2.Text = "yamuiImageButton2";
            // 
            // textbox2
            // 
            this.textbox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textbox2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.textbox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textbox2.CustomBackColor = System.Drawing.Color.Empty;
            this.textbox2.CustomForeColor = System.Drawing.Color.Empty;
            this.textbox2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.textbox2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.textbox2.Location = new System.Drawing.Point(193, 253);
            this.textbox2.Name = "textbox2";
            this.textbox2.Size = new System.Drawing.Size(678, 20);
            this.textbox2.TabIndex = 130;
            this.textbox2.Tag = "ini file (*.ini)|*.ini";
            this.textbox2.WaterMark = null;
            // 
            // flExtraProPath
            // 
            this.flExtraProPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flExtraProPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.flExtraProPath.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.flExtraProPath.CustomBackColor = System.Drawing.Color.Empty;
            this.flExtraProPath.CustomForeColor = System.Drawing.Color.Empty;
            this.flExtraProPath.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.flExtraProPath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.flExtraProPath.Location = new System.Drawing.Point(193, 279);
            this.flExtraProPath.MultiLines = true;
            this.flExtraProPath.Name = "flExtraProPath";
            this.flExtraProPath.Size = new System.Drawing.Size(678, 50);
            this.flExtraProPath.TabIndex = 129;
            this.flExtraProPath.WaterMark = "Appended to the .ini PROPATH (comma separated list)";
            // 
            // htmlLabel3
            // 
            this.htmlLabel3.AutoSize = false;
            this.htmlLabel3.AutoSizeHeightOnly = true;
            this.htmlLabel3.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel3.BaseStylesheet = null;
            this.htmlLabel3.IsSelectionEnabled = false;
            this.htmlLabel3.Location = new System.Drawing.Point(30, 335);
            this.htmlLabel3.Name = "htmlLabel3";
            this.htmlLabel3.Size = new System.Drawing.Size(122, 15);
            this.htmlLabel3.TabIndex = 112;
            this.htmlLabel3.TabStop = false;
            this.htmlLabel3.Text = "<b>Source directory</b>";
            // 
            // btleft3
            // 
            this.btleft3.BackColor = System.Drawing.Color.Transparent;
            this.btleft3.BackGrndImage = null;
            this.btleft3.Location = new System.Drawing.Point(170, 335);
            this.btleft3.Margin = new System.Windows.Forms.Padding(0);
            this.btleft3.Name = "btleft3";
            this.btleft3.SetImgSize = new System.Drawing.Size(0, 0);
            this.btleft3.Size = new System.Drawing.Size(20, 20);
            this.btleft3.TabIndex = 98;
            this.btleft3.TabStop = false;
            this.btleft3.Text = "yamuiImageButton4";
            // 
            // textbox3
            // 
            this.textbox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textbox3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.textbox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textbox3.CustomBackColor = System.Drawing.Color.Empty;
            this.textbox3.CustomForeColor = System.Drawing.Color.Empty;
            this.textbox3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.textbox3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.textbox3.Location = new System.Drawing.Point(193, 335);
            this.textbox3.Name = "textbox3";
            this.textbox3.Size = new System.Drawing.Size(678, 20);
            this.textbox3.TabIndex = 128;
            this.textbox3.Tag = "true";
            this.textbox3.WaterMark = null;
            // 
            // btright3
            // 
            this.btright3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btright3.BackColor = System.Drawing.Color.Transparent;
            this.btright3.BackGrndImage = null;
            this.btright3.Location = new System.Drawing.Point(874, 335);
            this.btright3.Margin = new System.Windows.Forms.Padding(0);
            this.btright3.Name = "btright3";
            this.btright3.SetImgSize = new System.Drawing.Size(0, 0);
            this.btright3.Size = new System.Drawing.Size(20, 20);
            this.btright3.TabIndex = 99;
            this.btright3.TabStop = false;
            this.btright3.Tag = "dir";
            this.btright3.Text = "yamuiImageButton3";
            // 
            // htmlLabel4
            // 
            this.htmlLabel4.AutoSize = false;
            this.htmlLabel4.AutoSizeHeightOnly = true;
            this.htmlLabel4.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel4.BaseStylesheet = null;
            this.htmlLabel4.IsSelectionEnabled = false;
            this.htmlLabel4.Location = new System.Drawing.Point(30, 387);
            this.htmlLabel4.Name = "htmlLabel4";
            this.htmlLabel4.Size = new System.Drawing.Size(137, 15);
            this.htmlLabel4.TabIndex = 113;
            this.htmlLabel4.TabStop = false;
            this.htmlLabel4.Text = "<b>Deployment directory</b>";
            // 
            // btleft4
            // 
            this.btleft4.BackColor = System.Drawing.Color.Transparent;
            this.btleft4.BackGrndImage = null;
            this.btleft4.Location = new System.Drawing.Point(170, 387);
            this.btleft4.Margin = new System.Windows.Forms.Padding(0);
            this.btleft4.Name = "btleft4";
            this.btleft4.SetImgSize = new System.Drawing.Size(0, 0);
            this.btleft4.Size = new System.Drawing.Size(20, 20);
            this.btleft4.TabIndex = 100;
            this.btleft4.TabStop = false;
            this.btleft4.Text = "yamuiImageButton6";
            // 
            // textbox4
            // 
            this.textbox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textbox4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.textbox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textbox4.CustomBackColor = System.Drawing.Color.Empty;
            this.textbox4.CustomForeColor = System.Drawing.Color.Empty;
            this.textbox4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.textbox4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.textbox4.Location = new System.Drawing.Point(193, 387);
            this.textbox4.Name = "textbox4";
            this.textbox4.Size = new System.Drawing.Size(678, 20);
            this.textbox4.TabIndex = 127;
            this.textbox4.Tag = "true";
            this.textbox4.WaterMark = null;
            // 
            // btright4
            // 
            this.btright4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btright4.BackColor = System.Drawing.Color.Transparent;
            this.btright4.BackGrndImage = null;
            this.btright4.Location = new System.Drawing.Point(874, 387);
            this.btright4.Margin = new System.Windows.Forms.Padding(0);
            this.btright4.Name = "btright4";
            this.btright4.SetImgSize = new System.Drawing.Size(0, 0);
            this.btright4.Size = new System.Drawing.Size(20, 20);
            this.btright4.TabIndex = 101;
            this.btright4.TabStop = false;
            this.btright4.Tag = "dir";
            this.btright4.Text = "yamuiImageButton5";
            // 
            // tgCompLocally
            // 
            this.tgCompLocally.BackGrndImage = null;
            this.tgCompLocally.Location = new System.Drawing.Point(298, 363);
            this.tgCompLocally.Name = "tgCompLocally";
            this.tgCompLocally.SetImgSize = new System.Drawing.Size(0, 0);
            this.tgCompLocally.Size = new System.Drawing.Size(35, 16);
            this.tgCompLocally.TabIndex = 137;
            this.tgCompLocally.ToggleSize = 30;
            // 
            // tgCompWithLst
            // 
            this.tgCompWithLst.BackGrndImage = null;
            this.tgCompWithLst.Location = new System.Drawing.Point(481, 363);
            this.tgCompWithLst.Name = "tgCompWithLst";
            this.tgCompWithLst.SetImgSize = new System.Drawing.Size(0, 0);
            this.tgCompWithLst.Size = new System.Drawing.Size(35, 16);
            this.tgCompWithLst.TabIndex = 155;
            this.tgCompWithLst.ToggleSize = 30;
            // 
            // lblLocally
            // 
            this.lblLocally.AutoSize = true;
            this.lblLocally.Function = YamuiFramework.Fonts.FontFunction.Small;
            this.lblLocally.Location = new System.Drawing.Point(193, 364);
            this.lblLocally.Margin = new System.Windows.Forms.Padding(3);
            this.lblLocally.Name = "lblLocally";
            this.lblLocally.Size = new System.Drawing.Size(99, 12);
            this.lblLocally.TabIndex = 138;
            this.lblLocally.Text = "Compile near source?";
            this.lblLocally.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // yamuiLabel5
            // 
            this.yamuiLabel5.AutoSize = true;
            this.yamuiLabel5.Function = YamuiFramework.Fonts.FontFunction.Small;
            this.yamuiLabel5.Location = new System.Drawing.Point(343, 364);
            this.yamuiLabel5.Margin = new System.Windows.Forms.Padding(3);
            this.yamuiLabel5.Name = "yamuiLabel5";
            this.yamuiLabel5.Size = new System.Drawing.Size(132, 12);
            this.yamuiLabel5.TabIndex = 154;
            this.yamuiLabel5.Text = "Compile with debug list (.lst)?";
            this.yamuiLabel5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // htmlLabel5
            // 
            this.htmlLabel5.AutoSize = false;
            this.htmlLabel5.AutoSizeHeightOnly = true;
            this.htmlLabel5.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel5.BaseStylesheet = null;
            this.htmlLabel5.IsSelectionEnabled = false;
            this.htmlLabel5.Location = new System.Drawing.Point(30, 413);
            this.htmlLabel5.Name = "htmlLabel5";
            this.htmlLabel5.Size = new System.Drawing.Size(122, 15);
            this.htmlLabel5.TabIndex = 114;
            this.htmlLabel5.TabStop = false;
            this.htmlLabel5.Text = "<b>Prowin.exe path</b>";
            // 
            // btleft5
            // 
            this.btleft5.BackColor = System.Drawing.Color.Transparent;
            this.btleft5.BackGrndImage = null;
            this.btleft5.Location = new System.Drawing.Point(170, 413);
            this.btleft5.Margin = new System.Windows.Forms.Padding(0);
            this.btleft5.Name = "btleft5";
            this.btleft5.SetImgSize = new System.Drawing.Size(0, 0);
            this.btleft5.Size = new System.Drawing.Size(20, 20);
            this.btleft5.TabIndex = 102;
            this.btleft5.TabStop = false;
            this.btleft5.Text = "yamuiImageButton8";
            // 
            // textbox5
            // 
            this.textbox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textbox5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.textbox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textbox5.CustomBackColor = System.Drawing.Color.Empty;
            this.textbox5.CustomForeColor = System.Drawing.Color.Empty;
            this.textbox5.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.textbox5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.textbox5.Location = new System.Drawing.Point(193, 413);
            this.textbox5.Name = "textbox5";
            this.textbox5.Size = new System.Drawing.Size(678, 20);
            this.textbox5.TabIndex = 126;
            this.textbox5.Tag = "prowin32 (*.exe)|*.exe";
            this.textbox5.WaterMark = null;
            // 
            // btright5
            // 
            this.btright5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btright5.BackColor = System.Drawing.Color.Transparent;
            this.btright5.BackGrndImage = null;
            this.btright5.Location = new System.Drawing.Point(874, 413);
            this.btright5.Margin = new System.Windows.Forms.Padding(0);
            this.btright5.Name = "btright5";
            this.btright5.SetImgSize = new System.Drawing.Size(0, 0);
            this.btright5.Size = new System.Drawing.Size(20, 20);
            this.btright5.TabIndex = 103;
            this.btright5.TabStop = false;
            this.btright5.Text = "yamuiImageButton7";
            // 
            // htmlLabel6
            // 
            this.htmlLabel6.AutoSize = false;
            this.htmlLabel6.AutoSizeHeightOnly = true;
            this.htmlLabel6.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel6.BaseStylesheet = null;
            this.htmlLabel6.IsSelectionEnabled = false;
            this.htmlLabel6.Location = new System.Drawing.Point(30, 439);
            this.htmlLabel6.Name = "htmlLabel6";
            this.htmlLabel6.Size = new System.Drawing.Size(154, 15);
            this.htmlLabel6.TabIndex = 115;
            this.htmlLabel6.TabStop = false;
            this.htmlLabel6.Text = "<b>Prowin extra params</b>";
            // 
            // flCmdLine
            // 
            this.flCmdLine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flCmdLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.flCmdLine.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.flCmdLine.CustomBackColor = System.Drawing.Color.Empty;
            this.flCmdLine.CustomForeColor = System.Drawing.Color.Empty;
            this.flCmdLine.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.flCmdLine.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.flCmdLine.Location = new System.Drawing.Point(193, 439);
            this.flCmdLine.Name = "flCmdLine";
            this.flCmdLine.Size = new System.Drawing.Size(678, 20);
            this.flCmdLine.TabIndex = 125;
            this.flCmdLine.WaterMark = "Appended to the prowin command line when running or compiling";
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
            this.htmlLabel7.TabIndex = 116;
            this.htmlLabel7.TabStop = false;
            this.htmlLabel7.Text = "<b>Path to server.log file</b>";
            // 
            // btleft6
            // 
            this.btleft6.BackColor = System.Drawing.Color.Transparent;
            this.btleft6.BackGrndImage = null;
            this.btleft6.Location = new System.Drawing.Point(170, 465);
            this.btleft6.Margin = new System.Windows.Forms.Padding(0);
            this.btleft6.Name = "btleft6";
            this.btleft6.SetImgSize = new System.Drawing.Size(0, 0);
            this.btleft6.Size = new System.Drawing.Size(20, 20);
            this.btleft6.TabIndex = 107;
            this.btleft6.TabStop = false;
            this.btleft6.Text = "yamuiImageButton12";
            // 
            // textbox6
            // 
            this.textbox6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textbox6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.textbox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textbox6.CustomBackColor = System.Drawing.Color.Empty;
            this.textbox6.CustomForeColor = System.Drawing.Color.Empty;
            this.textbox6.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.textbox6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.textbox6.Location = new System.Drawing.Point(193, 465);
            this.textbox6.Name = "textbox6";
            this.textbox6.Size = new System.Drawing.Size(678, 20);
            this.textbox6.TabIndex = 124;
            this.textbox6.Tag = "log file (*.log)|*.log";
            this.textbox6.WaterMark = null;
            // 
            // btright6
            // 
            this.btright6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btright6.BackColor = System.Drawing.Color.Transparent;
            this.btright6.BackGrndImage = null;
            this.btright6.Location = new System.Drawing.Point(874, 465);
            this.btright6.Margin = new System.Windows.Forms.Padding(0);
            this.btright6.Name = "btright6";
            this.btright6.SetImgSize = new System.Drawing.Size(0, 0);
            this.btright6.Size = new System.Drawing.Size(20, 20);
            this.btright6.TabIndex = 108;
            this.btright6.TabStop = false;
            this.btright6.Text = "yamuiImageButton11";
            // 
            // btDelete
            // 
            this.btDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btDelete.BackGrndImage = null;
            this.btDelete.Location = new System.Drawing.Point(394, 491);
            this.btDelete.Name = "btDelete";
            this.btDelete.SetImgSize = new System.Drawing.Size(20, 20);
            this.btDelete.Size = new System.Drawing.Size(70, 24);
            this.btDelete.TabIndex = 121;
            this.btDelete.Text = "Delete";
            this.btDelete.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flName
            // 
            this.flName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.flName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.flName.CustomBackColor = System.Drawing.Color.Empty;
            this.flName.CustomForeColor = System.Drawing.Color.Empty;
            this.flName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.flName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.flName.Location = new System.Drawing.Point(30, 48);
            this.flName.Name = "flName";
            this.flName.Size = new System.Drawing.Size(137, 21);
            this.flName.TabIndex = 136;
            this.flName.WaterMark = "Application name";
            // 
            // btEdit
            // 
            this.btEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btEdit.BackGrndImage = null;
            this.btEdit.Location = new System.Drawing.Point(193, 491);
            this.btEdit.Name = "btEdit";
            this.btEdit.SetImgSize = new System.Drawing.Size(20, 20);
            this.btEdit.Size = new System.Drawing.Size(57, 24);
            this.btEdit.TabIndex = 123;
            this.btEdit.Text = "Edit";
            this.btEdit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btAdd
            // 
            this.btAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btAdd.BackGrndImage = null;
            this.btAdd.Location = new System.Drawing.Point(256, 491);
            this.btAdd.Name = "btAdd";
            this.btAdd.SetImgSize = new System.Drawing.Size(20, 20);
            this.btAdd.Size = new System.Drawing.Size(59, 24);
            this.btAdd.TabIndex = 122;
            this.btAdd.Text = "Add";
            this.btAdd.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btCopy
            // 
            this.btCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btCopy.BackGrndImage = null;
            this.btCopy.Location = new System.Drawing.Point(321, 491);
            this.btCopy.Name = "btCopy";
            this.btCopy.SetImgSize = new System.Drawing.Size(20, 20);
            this.btCopy.Size = new System.Drawing.Size(67, 24);
            this.btCopy.TabIndex = 146;
            this.btCopy.Text = "Copy";
            this.btCopy.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btDbEdit
            // 
            this.btDbEdit.BackGrndImage = null;
            this.btDbEdit.Location = new System.Drawing.Point(193, 159);
            this.btDbEdit.Name = "btDbEdit";
            this.btDbEdit.SetImgSize = new System.Drawing.Size(20, 20);
            this.btDbEdit.Size = new System.Drawing.Size(57, 24);
            this.btDbEdit.TabIndex = 157;
            this.btDbEdit.Text = "Edit";
            this.btDbEdit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btDbAdd
            // 
            this.btDbAdd.BackGrndImage = null;
            this.btDbAdd.Location = new System.Drawing.Point(256, 159);
            this.btDbAdd.Name = "btDbAdd";
            this.btDbAdd.SetImgSize = new System.Drawing.Size(20, 20);
            this.btDbAdd.Size = new System.Drawing.Size(59, 24);
            this.btDbAdd.TabIndex = 158;
            this.btDbAdd.Text = "Add";
            this.btDbAdd.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btDbDelete
            // 
            this.btDbDelete.BackGrndImage = null;
            this.btDbDelete.Location = new System.Drawing.Point(321, 159);
            this.btDbDelete.Name = "btDbDelete";
            this.btDbDelete.SetImgSize = new System.Drawing.Size(20, 20);
            this.btDbDelete.Size = new System.Drawing.Size(70, 24);
            this.btDbDelete.TabIndex = 156;
            this.btDbDelete.Text = "Delete";
            this.btDbDelete.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btSave
            // 
            this.btSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btSave.BackGrndImage = null;
            this.btSave.Location = new System.Drawing.Point(193, 491);
            this.btSave.Name = "btSave";
            this.btSave.SetImgSize = new System.Drawing.Size(20, 20);
            this.btSave.Size = new System.Drawing.Size(62, 24);
            this.btSave.TabIndex = 160;
            this.btSave.Text = "Save";
            this.btSave.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btCancel.BackGrndImage = null;
            this.btCancel.Location = new System.Drawing.Point(261, 491);
            this.btCancel.Name = "btCancel";
            this.btCancel.SetImgSize = new System.Drawing.Size(20, 20);
            this.btCancel.Size = new System.Drawing.Size(71, 24);
            this.btCancel.TabIndex = 159;
            this.btCancel.Text = "Cancel";
            this.btCancel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btDbCancel
            // 
            this.btDbCancel.BackGrndImage = null;
            this.btDbCancel.Location = new System.Drawing.Point(261, 159);
            this.btDbCancel.Name = "btDbCancel";
            this.btDbCancel.SetImgSize = new System.Drawing.Size(20, 20);
            this.btDbCancel.Size = new System.Drawing.Size(71, 24);
            this.btDbCancel.TabIndex = 143;
            this.btDbCancel.Text = "Cancel";
            this.btDbCancel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btDbSave
            // 
            this.btDbSave.BackGrndImage = null;
            this.btDbSave.Location = new System.Drawing.Point(193, 159);
            this.btDbSave.Name = "btDbSave";
            this.btDbSave.SetImgSize = new System.Drawing.Size(20, 20);
            this.btDbSave.Size = new System.Drawing.Size(62, 24);
            this.btDbSave.TabIndex = 144;
            this.btDbSave.Text = "Save";
            this.btDbSave.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // areaDb
            // 
            this.areaDb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.areaDb.BackColor = System.Drawing.Color.Lime;
            this.areaDb.Enabled = false;
            this.areaDb.Location = new System.Drawing.Point(601, 158);
            this.areaDb.Name = "areaDb";
            this.areaDb.Size = new System.Drawing.Size(277, 24);
            this.areaDb.TabIndex = 167;
            this.areaDb.Visible = false;
            // 
            // cbSuffix
            // 
            this.cbSuffix.ItemHeight = 15;
            this.cbSuffix.Location = new System.Drawing.Point(173, 48);
            this.cbSuffix.Name = "cbSuffix";
            this.cbSuffix.Size = new System.Drawing.Size(133, 21);
            this.cbSuffix.TabIndex = 140;
            // 
            // cbName
            // 
            this.cbName.ItemHeight = 15;
            this.cbName.Location = new System.Drawing.Point(30, 48);
            this.cbName.Name = "cbName";
            this.cbName.Size = new System.Drawing.Size(137, 21);
            this.cbName.TabIndex = 141;
            // 
            // areaLeftButtons
            // 
            this.areaLeftButtons.BackColor = System.Drawing.Color.Red;
            this.areaLeftButtons.Enabled = false;
            this.areaLeftButtons.Location = new System.Drawing.Point(166, 133);
            this.areaLeftButtons.Name = "areaLeftButtons";
            this.areaLeftButtons.Size = new System.Drawing.Size(16, 347);
            this.areaLeftButtons.TabIndex = 168;
            this.areaLeftButtons.Visible = false;
            // 
            // areaEnv
            // 
            this.areaEnv.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.areaEnv.BackColor = System.Drawing.Color.Yellow;
            this.areaEnv.Enabled = false;
            this.areaEnv.Location = new System.Drawing.Point(3, 185);
            this.areaEnv.Name = "areaEnv";
            this.areaEnv.Size = new System.Drawing.Size(897, 290);
            this.areaEnv.TabIndex = 165;
            this.areaEnv.Visible = false;
            // 
            // areaPf
            // 
            this.areaPf.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.areaPf.BackColor = System.Drawing.Color.Aqua;
            this.areaPf.Enabled = false;
            this.areaPf.Location = new System.Drawing.Point(2, 109);
            this.areaPf.Name = "areaPf";
            this.areaPf.Size = new System.Drawing.Size(898, 43);
            this.areaPf.TabIndex = 166;
            this.areaPf.Visible = false;
            // 
            // SetEnvironment
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.scrollPanel);
            this.Name = "SetEnvironment";
            this.Size = new System.Drawing.Size(900, 650);
            this.scrollPanel.ContentPanel.ResumeLayout(false);
            this.scrollPanel.ContentPanel.PerformLayout();
            this.scrollPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private HtmlToolTip toolTip;
        private YamuiScrollPanel scrollPanel;
        private YamuiLabel lblLocally;
        private YamuiComboBox cbName;
        private YamuiComboBox cbSuffix;
        private YamuiComboBox cbDatabase;
        private YamuiTextBox flName;
        private YamuiTextBox flSuffix;
        private YamuiTextBox flLabel;
        private YamuiTextBox flDatabase;
        private YamuiTextBox textbox1;
        private YamuiTextBox flExtraPf;
        private YamuiTextBox textbox2;
        private YamuiButtonToggle tgCompLocally;
        private YamuiTextBox flExtraProPath;
        private YamuiTextBox textbox3;
        private YamuiTextBox textbox4;
        private YamuiTextBox textbox5;
        private YamuiTextBox flCmdLine;
        private YamuiTextBox textbox6;
        private HtmlLabel txLabel;
        private YamuiButtonImage btright6;
        private YamuiButtonImage btleft6;
        private YamuiButtonImage btright1;
        private YamuiButtonImage btleft1;
        private YamuiButtonImage btright5;
        private YamuiButtonImage btleft5;
        private YamuiButtonImage btright4;
        private YamuiButtonImage btleft4;
        private YamuiButtonImage btright3;
        private YamuiButtonImage btleft3;
        private YamuiButtonImage btright2;
        private YamuiButtonImage btleft2;
        private HtmlLabel htmlLabel7;
        private HtmlLabel htmlLabel6;
        private HtmlLabel htmlLabel5;
        private HtmlLabel htmlLabel4;
        private HtmlLabel htmlLabel3;
        private HtmlLabel htmlLabel2;
        private HtmlLabel lbl_listdb;
        private YamuiLabel yamuiLabel4;
        private YamuiLabel yamuiLabel1;
        private YamuiButton btDelete;
        private HtmlLabel htmlLabel8;
        private YamuiButton btDbSave;
        private YamuiButton btDbCancel;
        private HtmlLabel linkurl;
        private YamuiButton btCopy;
        private YamuiLabel yamuiLabel3;
        private YamuiLabel yamuiLabel5;
        private YamuiButtonToggle tgCompWithLst;
        private YamuiButton btDbDelete;
        private YamuiButton btDbAdd;
        private YamuiButton btDbEdit;
        private YamuiButton btSave;
        private YamuiButton btCancel;
        private YamuiButton btEdit;
        private YamuiButton btAdd;
        private YamuiButton btDbView;
        private YamuiButton btDbDeleteDownload;
        private YamuiButton btDbDownload;
        private YamuiArea areaEnv;
        private YamuiArea areaPf;
        private YamuiArea areaDb;
        private YamuiArea areaLeftButtons;
        private HtmlLabel htmlLabel1;
        private HtmlLabel htmlLabel10;
        private HtmlLabel htmlLabel9;
    }
}
