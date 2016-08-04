using System.ComponentModel;
using YamuiFramework.Controls;
using YamuiFramework.HtmlRenderer.WinForms;
using _3PA.MainFeatures.Appli.Pages.Options;

namespace _3PA.MainFeatures.Appli.Pages.Actions {
    partial class DoDeployPage {
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
            this.tooltip = new YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip();
            this.scrollPanel = new YamuiFramework.Controls.YamuiScrollPanel();
            this.htmlLabel11 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.toggleOnlyGenerateRcode = new YamuiFramework.Controls.YamuiButtonToggle();
            this.btSeeRules = new YamuiFramework.Controls.YamuiButton();
            this.lblCurEnv = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel9 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel8 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.btDelete = new YamuiFramework.Controls.YamuiButton();
            this.btSaveAs = new YamuiFramework.Controls.YamuiButton();
            this.btSave = new YamuiFramework.Controls.YamuiButton();
            this.cbName = new YamuiFramework.Controls.YamuiComboBox();
            this.btRules = new YamuiFramework.Controls.YamuiButton();
            this.lbl_rules = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.lbl_linkrules = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.lbl_report = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.linkurl = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.yamuiLabel2 = new YamuiFramework.Controls.YamuiLabel();
            this.btBrowse = new YamuiFramework.Controls.YamuiButtonImage();
            this.btUndo = new YamuiFramework.Controls.YamuiButtonImage();
            this.fl_directory = new YamuiFramework.Controls.YamuiTextBox();
            this.btOpen = new YamuiFramework.Controls.YamuiButtonImage();
            this.btHistoric = new YamuiFramework.Controls.YamuiButtonImage();
            this.htmlLabel3 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel2 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.toggleRecurs = new YamuiFramework.Controls.YamuiButtonToggle();
            this.htmlLabel1 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.toggleMono = new YamuiFramework.Controls.YamuiButtonToggle();
            this.htmlLabel4 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.fl_nbProcess = new YamuiFramework.Controls.YamuiTextBox();
            this.htmlLabel5 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.fl_include = new YamuiFramework.Controls.YamuiTextBox();
            this.htmlLabel6 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.fl_exclude = new YamuiFramework.Controls.YamuiTextBox();
            this.btCancel = new YamuiFramework.Controls.YamuiButton();
            this.btStart = new YamuiFramework.Controls.YamuiButton();
            this.btReset = new YamuiFramework.Controls.YamuiButton();
            this.bt_export = new YamuiFramework.Controls.YamuiButton();
            this.progressBar = new YamuiFramework.Controls.YamuiProgressBar();
            this.scrollPanel.ContentPanel.SuspendLayout();
            this.scrollPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tooltip
            // 
            this.tooltip.AllowLinksHandling = true;
            this.tooltip.AutomaticDelay = 50;
            this.tooltip.AutoPopDelay = 90000;
            this.tooltip.BaseStylesheet = null;
            this.tooltip.InitialDelay = 50;
            this.tooltip.MaximumSize = new System.Drawing.Size(0, 0);
            this.tooltip.OwnerDraw = true;
            this.tooltip.ReshowDelay = 10;
            this.tooltip.UseAnimation = false;
            this.tooltip.UseFading = false;
            // 
            // scrollPanel
            // 
            // 
            // scrollPanel.ContentPanel
            // 
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel11);
            this.scrollPanel.ContentPanel.Controls.Add(this.toggleOnlyGenerateRcode);
            this.scrollPanel.ContentPanel.Controls.Add(this.btSeeRules);
            this.scrollPanel.ContentPanel.Controls.Add(this.lblCurEnv);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel9);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel8);
            this.scrollPanel.ContentPanel.Controls.Add(this.btDelete);
            this.scrollPanel.ContentPanel.Controls.Add(this.btSaveAs);
            this.scrollPanel.ContentPanel.Controls.Add(this.btSave);
            this.scrollPanel.ContentPanel.Controls.Add(this.cbName);
            this.scrollPanel.ContentPanel.Controls.Add(this.btRules);
            this.scrollPanel.ContentPanel.Controls.Add(this.lbl_rules);
            this.scrollPanel.ContentPanel.Controls.Add(this.lbl_linkrules);
            this.scrollPanel.ContentPanel.Controls.Add(this.lbl_report);
            this.scrollPanel.ContentPanel.Controls.Add(this.linkurl);
            this.scrollPanel.ContentPanel.Controls.Add(this.yamuiLabel2);
            this.scrollPanel.ContentPanel.Controls.Add(this.btBrowse);
            this.scrollPanel.ContentPanel.Controls.Add(this.btUndo);
            this.scrollPanel.ContentPanel.Controls.Add(this.fl_directory);
            this.scrollPanel.ContentPanel.Controls.Add(this.btOpen);
            this.scrollPanel.ContentPanel.Controls.Add(this.btHistoric);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel3);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel2);
            this.scrollPanel.ContentPanel.Controls.Add(this.toggleRecurs);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel1);
            this.scrollPanel.ContentPanel.Controls.Add(this.toggleMono);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel4);
            this.scrollPanel.ContentPanel.Controls.Add(this.fl_nbProcess);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel5);
            this.scrollPanel.ContentPanel.Controls.Add(this.fl_include);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel6);
            this.scrollPanel.ContentPanel.Controls.Add(this.fl_exclude);
            this.scrollPanel.ContentPanel.Controls.Add(this.btCancel);
            this.scrollPanel.ContentPanel.Controls.Add(this.btStart);
            this.scrollPanel.ContentPanel.Controls.Add(this.btReset);
            this.scrollPanel.ContentPanel.Controls.Add(this.bt_export);
            this.scrollPanel.ContentPanel.Controls.Add(this.progressBar);
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
            // htmlLabel11
            // 
            this.htmlLabel11.AutoSize = false;
            this.htmlLabel11.AutoSizeHeightOnly = true;
            this.htmlLabel11.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel11.BaseStylesheet = null;
            this.htmlLabel11.IsSelectionEnabled = false;
            this.htmlLabel11.Location = new System.Drawing.Point(30, 167);
            this.htmlLabel11.Name = "htmlLabel11";
            this.htmlLabel11.Size = new System.Drawing.Size(223, 15);
            this.htmlLabel11.TabIndex = 161;
            this.htmlLabel11.TabStop = false;
            this.htmlLabel11.Text = "Only generate R-code during compilation";
            // 
            // toggleOnlyGenerateRcode
            // 
            this.toggleOnlyGenerateRcode.BackGrndImage = null;
            this.toggleOnlyGenerateRcode.Location = new System.Drawing.Point(268, 167);
            this.toggleOnlyGenerateRcode.Name = "toggleOnlyGenerateRcode";
            this.toggleOnlyGenerateRcode.SetImgSize = new System.Drawing.Size(0, 0);
            this.toggleOnlyGenerateRcode.Size = new System.Drawing.Size(40, 16);
            this.toggleOnlyGenerateRcode.TabIndex = 162;
            this.toggleOnlyGenerateRcode.ToggleSize = 30;
            // 
            // btSeeRules
            // 
            this.btSeeRules.BackGrndImage = null;
            this.btSeeRules.Location = new System.Drawing.Point(453, 361);
            this.btSeeRules.Name = "btSeeRules";
            this.btSeeRules.SetImgSize = new System.Drawing.Size(20, 20);
            this.btSeeRules.Size = new System.Drawing.Size(283, 24);
            this.btSeeRules.TabIndex = 160;
            this.btSeeRules.Text = "View deployment rules for this environment";
            // 
            // lblCurEnv
            // 
            this.lblCurEnv.AutoSize = false;
            this.lblCurEnv.AutoSizeHeightOnly = true;
            this.lblCurEnv.BackColor = System.Drawing.Color.Transparent;
            this.lblCurEnv.BaseStylesheet = null;
            this.lblCurEnv.IsSelectionEnabled = false;
            this.lblCurEnv.Location = new System.Drawing.Point(268, 67);
            this.lblCurEnv.Name = "lblCurEnv";
            this.lblCurEnv.Size = new System.Drawing.Size(161, 15);
            this.lblCurEnv.TabIndex = 159;
            this.lblCurEnv.TabStop = false;
            this.lblCurEnv.Text = "my env (switch)";
            // 
            // htmlLabel9
            // 
            this.htmlLabel9.AutoSize = false;
            this.htmlLabel9.AutoSizeHeightOnly = true;
            this.htmlLabel9.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel9.BaseStylesheet = null;
            this.htmlLabel9.IsSelectionEnabled = false;
            this.htmlLabel9.Location = new System.Drawing.Point(30, 67);
            this.htmlLabel9.Name = "htmlLabel9";
            this.htmlLabel9.Size = new System.Drawing.Size(161, 15);
            this.htmlLabel9.TabIndex = 158;
            this.htmlLabel9.TabStop = false;
            this.htmlLabel9.Text = "Current environment";
            // 
            // htmlLabel8
            // 
            this.htmlLabel8.AutoSize = false;
            this.htmlLabel8.AutoSizeHeightOnly = true;
            this.htmlLabel8.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel8.BaseStylesheet = null;
            this.htmlLabel8.IsSelectionEnabled = false;
            this.htmlLabel8.Location = new System.Drawing.Point(30, 29);
            this.htmlLabel8.Name = "htmlLabel8";
            this.htmlLabel8.Size = new System.Drawing.Size(207, 15);
            this.htmlLabel8.TabIndex = 157;
            this.htmlLabel8.TabStop = false;
            this.htmlLabel8.Text = "<b>Select the deployment configuration</b>";
            // 
            // btDelete
            // 
            this.btDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btDelete.BackGrndImage = null;
            this.btDelete.Location = new System.Drawing.Point(687, 29);
            this.btDelete.Name = "btDelete";
            this.btDelete.SetImgSize = new System.Drawing.Size(20, 20);
            this.btDelete.Size = new System.Drawing.Size(73, 24);
            this.btDelete.TabIndex = 156;
            this.btDelete.Text = "Delete";
            this.btDelete.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btSaveAs
            // 
            this.btSaveAs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btSaveAs.BackGrndImage = null;
            this.btSaveAs.Location = new System.Drawing.Point(595, 29);
            this.btSaveAs.Name = "btSaveAs";
            this.btSaveAs.SetImgSize = new System.Drawing.Size(20, 20);
            this.btSaveAs.Size = new System.Drawing.Size(86, 24);
            this.btSaveAs.TabIndex = 155;
            this.btSaveAs.Text = "Save as...";
            this.btSaveAs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btSave
            // 
            this.btSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btSave.BackGrndImage = null;
            this.btSave.Location = new System.Drawing.Point(527, 29);
            this.btSave.Name = "btSave";
            this.btSave.SetImgSize = new System.Drawing.Size(20, 20);
            this.btSave.Size = new System.Drawing.Size(62, 24);
            this.btSave.TabIndex = 154;
            this.btSave.Text = "Save";
            this.btSave.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbName
            // 
            this.cbName.ItemHeight = 15;
            this.cbName.Location = new System.Drawing.Point(268, 29);
            this.cbName.Name = "cbName";
            this.cbName.Size = new System.Drawing.Size(253, 21);
            this.cbName.TabIndex = 153;
            // 
            // btRules
            // 
            this.btRules.BackGrndImage = null;
            this.btRules.Location = new System.Drawing.Point(266, 361);
            this.btRules.Name = "btRules";
            this.btRules.SetImgSize = new System.Drawing.Size(20, 20);
            this.btRules.Size = new System.Drawing.Size(181, 24);
            this.btRules.TabIndex = 152;
            this.btRules.Text = "Modify deployment rules";
            // 
            // lbl_rules
            // 
            this.lbl_rules.AutoSize = false;
            this.lbl_rules.AutoSizeHeightOnly = true;
            this.lbl_rules.BackColor = System.Drawing.Color.Transparent;
            this.lbl_rules.BaseStylesheet = null;
            this.lbl_rules.IsSelectionEnabled = false;
            this.lbl_rules.Location = new System.Drawing.Point(266, 340);
            this.lbl_rules.Name = "lbl_rules";
            this.lbl_rules.Size = new System.Drawing.Size(415, 15);
            this.lbl_rules.TabIndex = 149;
            this.lbl_rules.TabStop = false;
            this.lbl_rules.Text = "0 rules for step 1, 0 rules for step 2";
            // 
            // lbl_linkrules
            // 
            this.lbl_linkrules.AutoSize = false;
            this.lbl_linkrules.AutoSizeHeightOnly = true;
            this.lbl_linkrules.BackColor = System.Drawing.Color.Transparent;
            this.lbl_linkrules.BaseStylesheet = null;
            this.lbl_linkrules.IsSelectionEnabled = false;
            this.lbl_linkrules.Location = new System.Drawing.Point(28, 340);
            this.lbl_linkrules.Name = "lbl_linkrules";
            this.lbl_linkrules.Size = new System.Drawing.Size(207, 15);
            this.lbl_linkrules.TabIndex = 148;
            this.lbl_linkrules.TabStop = false;
            this.lbl_linkrules.Text = "Current environment rules";
            // 
            // lbl_report
            // 
            this.lbl_report.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_report.AutoSize = false;
            this.lbl_report.AutoSizeHeightOnly = true;
            this.lbl_report.BackColor = System.Drawing.Color.Transparent;
            this.lbl_report.BaseStylesheet = null;
            this.lbl_report.Location = new System.Drawing.Point(28, 421);
            this.lbl_report.Name = "lbl_report";
            this.lbl_report.Size = new System.Drawing.Size(861, 15);
            this.lbl_report.TabIndex = 147;
            this.lbl_report.TabStop = false;
            this.lbl_report.Text = "lbl_report";
            // 
            // linkurl
            // 
            this.linkurl.BackColor = System.Drawing.Color.Transparent;
            this.linkurl.BaseStylesheet = null;
            this.linkurl.Location = new System.Drawing.Point(245, 0);
            this.linkurl.Name = "linkurl";
            this.linkurl.Size = new System.Drawing.Size(161, 15);
            this.linkurl.TabIndex = 146;
            this.linkurl.TabStop = false;
            this.linkurl.Text = "Learn more about this feature?";
            // 
            // yamuiLabel2
            // 
            this.yamuiLabel2.AutoSize = true;
            this.yamuiLabel2.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel2.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel2.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel2.Name = "yamuiLabel2";
            this.yamuiLabel2.Size = new System.Drawing.Size(198, 19);
            this.yamuiLabel2.TabIndex = 2;
            this.yamuiLabel2.Text = "COMPILE AND DEPLOY FILES";
            // 
            // btBrowse
            // 
            this.btBrowse.BackGrndImage = null;
            this.btBrowse.Location = new System.Drawing.Point(225, 88);
            this.btBrowse.Margin = new System.Windows.Forms.Padding(0);
            this.btBrowse.Name = "btBrowse";
            this.btBrowse.SetImgSize = new System.Drawing.Size(0, 0);
            this.btBrowse.Size = new System.Drawing.Size(20, 20);
            this.btBrowse.TabIndex = 3;
            this.btBrowse.Text = "yamuiImageButton1";
            // 
            // btUndo
            // 
            this.btUndo.BackGrndImage = null;
            this.btUndo.Location = new System.Drawing.Point(245, 88);
            this.btUndo.Margin = new System.Windows.Forms.Padding(0);
            this.btUndo.Name = "btUndo";
            this.btUndo.SetImgSize = new System.Drawing.Size(0, 0);
            this.btUndo.Size = new System.Drawing.Size(20, 20);
            this.btUndo.TabIndex = 118;
            this.btUndo.Text = "yamuiImageButton1";
            // 
            // fl_directory
            // 
            this.fl_directory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fl_directory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.fl_directory.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fl_directory.CustomBackColor = System.Drawing.Color.Empty;
            this.fl_directory.CustomForeColor = System.Drawing.Color.Empty;
            this.fl_directory.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.fl_directory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.fl_directory.Location = new System.Drawing.Point(268, 88);
            this.fl_directory.Name = "fl_directory";
            this.fl_directory.Size = new System.Drawing.Size(580, 20);
            this.fl_directory.TabIndex = 4;
            this.fl_directory.WaterMark = "Path to the directory to compile";
            // 
            // btOpen
            // 
            this.btOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btOpen.BackGrndImage = null;
            this.btOpen.Location = new System.Drawing.Point(851, 88);
            this.btOpen.Margin = new System.Windows.Forms.Padding(0);
            this.btOpen.Name = "btOpen";
            this.btOpen.SetImgSize = new System.Drawing.Size(0, 0);
            this.btOpen.Size = new System.Drawing.Size(20, 20);
            this.btOpen.TabIndex = 5;
            this.btOpen.Text = "yamuiImageButton1";
            // 
            // btHistoric
            // 
            this.btHistoric.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btHistoric.BackGrndImage = null;
            this.btHistoric.Location = new System.Drawing.Point(871, 88);
            this.btHistoric.Margin = new System.Windows.Forms.Padding(0);
            this.btHistoric.Name = "btHistoric";
            this.btHistoric.SetImgSize = new System.Drawing.Size(0, 0);
            this.btHistoric.Size = new System.Drawing.Size(20, 20);
            this.btHistoric.TabIndex = 6;
            this.btHistoric.Text = "yamuiImageButton1";
            // 
            // htmlLabel3
            // 
            this.htmlLabel3.AutoSize = false;
            this.htmlLabel3.AutoSizeHeightOnly = true;
            this.htmlLabel3.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel3.BaseStylesheet = null;
            this.htmlLabel3.IsSelectionEnabled = false;
            this.htmlLabel3.Location = new System.Drawing.Point(30, 90);
            this.htmlLabel3.Name = "htmlLabel3";
            this.htmlLabel3.Size = new System.Drawing.Size(161, 15);
            this.htmlLabel3.TabIndex = 113;
            this.htmlLabel3.TabStop = false;
            this.htmlLabel3.Text = "Select your source directory";
            // 
            // htmlLabel2
            // 
            this.htmlLabel2.AutoSize = false;
            this.htmlLabel2.AutoSizeHeightOnly = true;
            this.htmlLabel2.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel2.BaseStylesheet = null;
            this.htmlLabel2.IsSelectionEnabled = false;
            this.htmlLabel2.Location = new System.Drawing.Point(30, 116);
            this.htmlLabel2.Name = "htmlLabel2";
            this.htmlLabel2.Size = new System.Drawing.Size(161, 15);
            this.htmlLabel2.TabIndex = 120;
            this.htmlLabel2.TabStop = false;
            this.htmlLabel2.Text = "Explore folders recursively?";
            // 
            // toggleRecurs
            // 
            this.toggleRecurs.BackGrndImage = null;
            this.toggleRecurs.Location = new System.Drawing.Point(268, 116);
            this.toggleRecurs.Name = "toggleRecurs";
            this.toggleRecurs.SetImgSize = new System.Drawing.Size(0, 0);
            this.toggleRecurs.Size = new System.Drawing.Size(40, 16);
            this.toggleRecurs.TabIndex = 121;
            this.toggleRecurs.ToggleSize = 30;
            // 
            // htmlLabel1
            // 
            this.htmlLabel1.AutoSize = false;
            this.htmlLabel1.AutoSizeHeightOnly = true;
            this.htmlLabel1.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel1.BaseStylesheet = null;
            this.htmlLabel1.IsSelectionEnabled = false;
            this.htmlLabel1.Location = new System.Drawing.Point(30, 142);
            this.htmlLabel1.Name = "htmlLabel1";
            this.htmlLabel1.Size = new System.Drawing.Size(207, 15);
            this.htmlLabel1.TabIndex = 123;
            this.htmlLabel1.TabStop = false;
            this.htmlLabel1.Text = "Force to single-process compilation?";
            // 
            // toggleMono
            // 
            this.toggleMono.BackGrndImage = null;
            this.toggleMono.Location = new System.Drawing.Point(268, 142);
            this.toggleMono.Name = "toggleMono";
            this.toggleMono.SetImgSize = new System.Drawing.Size(0, 0);
            this.toggleMono.Size = new System.Drawing.Size(40, 16);
            this.toggleMono.TabIndex = 124;
            this.toggleMono.ToggleSize = 30;
            // 
            // htmlLabel4
            // 
            this.htmlLabel4.AutoSize = false;
            this.htmlLabel4.AutoSizeHeightOnly = true;
            this.htmlLabel4.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel4.BaseStylesheet = null;
            this.htmlLabel4.IsSelectionEnabled = false;
            this.htmlLabel4.Location = new System.Drawing.Point(30, 192);
            this.htmlLabel4.Name = "htmlLabel4";
            this.htmlLabel4.Size = new System.Drawing.Size(207, 15);
            this.htmlLabel4.TabIndex = 125;
            this.htmlLabel4.TabStop = false;
            this.htmlLabel4.Text = "Number of processes for each core";
            // 
            // fl_nbProcess
            // 
            this.fl_nbProcess.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.fl_nbProcess.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fl_nbProcess.CustomBackColor = System.Drawing.Color.Empty;
            this.fl_nbProcess.CustomForeColor = System.Drawing.Color.Empty;
            this.fl_nbProcess.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.fl_nbProcess.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.fl_nbProcess.Location = new System.Drawing.Point(268, 192);
            this.fl_nbProcess.Name = "fl_nbProcess";
            this.fl_nbProcess.Size = new System.Drawing.Size(34, 20);
            this.fl_nbProcess.TabIndex = 126;
            this.fl_nbProcess.WaterMark = null;
            // 
            // htmlLabel5
            // 
            this.htmlLabel5.AutoSize = false;
            this.htmlLabel5.AutoSizeHeightOnly = true;
            this.htmlLabel5.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel5.BaseStylesheet = null;
            this.htmlLabel5.IsSelectionEnabled = false;
            this.htmlLabel5.Location = new System.Drawing.Point(30, 218);
            this.htmlLabel5.Name = "htmlLabel5";
            this.htmlLabel5.Size = new System.Drawing.Size(215, 30);
            this.htmlLabel5.TabIndex = 129;
            this.htmlLabel5.TabStop = false;
            this.htmlLabel5.Text = "Filter for files to <b>include</b> in the compilation";
            // 
            // fl_include
            // 
            this.fl_include.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fl_include.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.fl_include.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fl_include.CustomBackColor = System.Drawing.Color.Empty;
            this.fl_include.CustomForeColor = System.Drawing.Color.Empty;
            this.fl_include.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.fl_include.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.fl_include.Location = new System.Drawing.Point(268, 218);
            this.fl_include.MultiLines = true;
            this.fl_include.Name = "fl_include";
            this.fl_include.Size = new System.Drawing.Size(580, 55);
            this.fl_include.TabIndex = 127;
            this.fl_include.WaterMark = "No filter, every file is considered";
            // 
            // htmlLabel6
            // 
            this.htmlLabel6.AutoSize = false;
            this.htmlLabel6.AutoSizeHeightOnly = true;
            this.htmlLabel6.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel6.BaseStylesheet = null;
            this.htmlLabel6.IsSelectionEnabled = false;
            this.htmlLabel6.Location = new System.Drawing.Point(30, 279);
            this.htmlLabel6.Name = "htmlLabel6";
            this.htmlLabel6.Size = new System.Drawing.Size(207, 30);
            this.htmlLabel6.TabIndex = 130;
            this.htmlLabel6.TabStop = false;
            this.htmlLabel6.Text = "Filter for files to <b>exclude</b> from the compilation";
            // 
            // fl_exclude
            // 
            this.fl_exclude.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fl_exclude.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.fl_exclude.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fl_exclude.CustomBackColor = System.Drawing.Color.Empty;
            this.fl_exclude.CustomForeColor = System.Drawing.Color.Empty;
            this.fl_exclude.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.fl_exclude.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.fl_exclude.Location = new System.Drawing.Point(268, 279);
            this.fl_exclude.MultiLines = true;
            this.fl_exclude.Name = "fl_exclude";
            this.fl_exclude.Size = new System.Drawing.Size(580, 55);
            this.fl_exclude.TabIndex = 128;
            this.fl_exclude.WaterMark = "No filter, no files will be excluded";
            // 
            // btCancel
            // 
            this.btCancel.BackGrndImage = null;
            this.btCancel.Location = new System.Drawing.Point(28, 391);
            this.btCancel.Name = "btCancel";
            this.btCancel.SetImgSize = new System.Drawing.Size(0, 0);
            this.btCancel.Size = new System.Drawing.Size(73, 23);
            this.btCancel.TabIndex = 122;
            this.btCancel.Text = "Cancel";
            // 
            // btStart
            // 
            this.btStart.BackGrndImage = null;
            this.btStart.Location = new System.Drawing.Point(28, 391);
            this.btStart.Name = "btStart";
            this.btStart.SetImgSize = new System.Drawing.Size(0, 0);
            this.btStart.Size = new System.Drawing.Size(117, 23);
            this.btStart.TabIndex = 116;
            this.btStart.Text = "Start deploying";
            // 
            // btReset
            // 
            this.btReset.BackGrndImage = null;
            this.btReset.Location = new System.Drawing.Point(151, 391);
            this.btReset.Name = "btReset";
            this.btReset.SetImgSize = new System.Drawing.Size(0, 0);
            this.btReset.Size = new System.Drawing.Size(100, 23);
            this.btReset.TabIndex = 131;
            this.btReset.Text = "Reset options";
            // 
            // bt_export
            // 
            this.bt_export.BackGrndImage = null;
            this.bt_export.Location = new System.Drawing.Point(257, 391);
            this.bt_export.Name = "bt_export";
            this.bt_export.SetImgSize = new System.Drawing.Size(0, 0);
            this.bt_export.Size = new System.Drawing.Size(148, 23);
            this.bt_export.TabIndex = 132;
            this.bt_export.Text = "Export report to html";
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.GradientIntensity = 5;
            this.progressBar.Location = new System.Drawing.Point(107, 391);
            this.progressBar.MarqueeWidth = 50;
            this.progressBar.Name = "progressBar";
            this.progressBar.Progress = 0F;
            this.progressBar.Size = new System.Drawing.Size(782, 23);
            this.progressBar.TabIndex = 114;
            // 
            // DoDeployPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.scrollPanel);
            this.Name = "DoDeployPage";
            this.Size = new System.Drawing.Size(900, 650);
            this.scrollPanel.ContentPanel.ResumeLayout(false);
            this.scrollPanel.ContentPanel.PerformLayout();
            this.scrollPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private HtmlToolTip tooltip;
        private YamuiScrollPanel scrollPanel;
        private YamuiLabel yamuiLabel2;
        private YamuiButtonImage btHistoric;
        private YamuiButtonImage btOpen;
        private YamuiTextBox fl_directory;
        private YamuiButtonImage btBrowse;
        private HtmlLabel htmlLabel3;
        private YamuiProgressBar progressBar;
        private YamuiButton btStart;
        private YamuiButtonImage btUndo;
        private HtmlLabel htmlLabel2;
        private YamuiButtonToggle toggleRecurs;
        private YamuiButton btCancel;
        private HtmlLabel htmlLabel1;
        private YamuiButtonToggle toggleMono;
        private HtmlLabel htmlLabel4;
        private YamuiTextBox fl_nbProcess;
        private HtmlLabel htmlLabel6;
        private HtmlLabel htmlLabel5;
        private YamuiTextBox fl_exclude;
        private YamuiTextBox fl_include;
        private YamuiButton btReset;
        private YamuiButton bt_export;
        private HtmlLabel linkurl;
        private HtmlLabel lbl_report;
        private HtmlLabel lbl_rules;
        private HtmlLabel lbl_linkrules;
        private YamuiButton btRules;
        private YamuiComboBox cbName;
        private YamuiButton btSave;
        private YamuiButton btDelete;
        private YamuiButton btSaveAs;
        private HtmlLabel htmlLabel8;
        private HtmlLabel lblCurEnv;
        private HtmlLabel htmlLabel9;
        private YamuiButton btSeeRules;
        private HtmlLabel htmlLabel11;
        private YamuiButtonToggle toggleOnlyGenerateRcode;
    }
}
