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
            this.btOpenHook = new YamuiFramework.Controls.YamuiButton();
            this.htmlLabel5 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.lbl_deployDir = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.btStart = new YamuiFramework.Controls.YamuiButton();
            this.btReset = new YamuiFramework.Controls.YamuiButton();
            this.htmlLabel7 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.toggleAutoUpdateSourceDir = new YamuiFramework.Controls.YamuiButtonToggle();
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
            this.lbl_report = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.linkurl = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.title = new YamuiFramework.Controls.YamuiLabel();
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
            this.btCancel = new YamuiFramework.Controls.YamuiButton();
            this.btReport = new YamuiFramework.Controls.YamuiButton();
            this.progressBar = new YamuiFramework.Controls.YamuiProgressBar();
            this.btTest = new YamuiFramework.Controls.YamuiButton();
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
            this.scrollPanel.ContentPanel.Controls.Add(this.btTest);
            this.scrollPanel.ContentPanel.Controls.Add(this.btOpenHook);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel5);
            this.scrollPanel.ContentPanel.Controls.Add(this.lbl_deployDir);
            this.scrollPanel.ContentPanel.Controls.Add(this.btStart);
            this.scrollPanel.ContentPanel.Controls.Add(this.btReset);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel7);
            this.scrollPanel.ContentPanel.Controls.Add(this.toggleAutoUpdateSourceDir);
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
            this.scrollPanel.ContentPanel.Controls.Add(this.lbl_report);
            this.scrollPanel.ContentPanel.Controls.Add(this.linkurl);
            this.scrollPanel.ContentPanel.Controls.Add(this.title);
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
            this.scrollPanel.ContentPanel.Controls.Add(this.btCancel);
            this.scrollPanel.ContentPanel.Controls.Add(this.btReport);
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
            // btOpenHook
            // 
            this.btOpenHook.BackGrndImage = null;
            this.btOpenHook.GreyScaleBackGrndImage = null;
            this.btOpenHook.IsFocused = false;
            this.btOpenHook.IsHovered = false;
            this.btOpenHook.IsPressed = false;
            this.btOpenHook.Location = new System.Drawing.Point(268, 228);
            this.btOpenHook.Name = "btOpenHook";
            this.btOpenHook.SetImgSize = new System.Drawing.Size(20, 20);
            this.btOpenHook.Size = new System.Drawing.Size(282, 24);
            this.btOpenHook.TabIndex = 169;
            this.btOpenHook.Text = "Modify the procedure executed after each step";
            // 
            // htmlLabel5
            // 
            this.htmlLabel5.AutoSize = false;
            this.htmlLabel5.AutoSizeHeightOnly = true;
            this.htmlLabel5.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel5.BaseStylesheet = null;
            this.htmlLabel5.IsSelectionEnabled = false;
            this.htmlLabel5.Location = new System.Drawing.Point(30, 230);
            this.htmlLabel5.Name = "htmlLabel5";
            this.htmlLabel5.Size = new System.Drawing.Size(207, 15);
            this.htmlLabel5.TabIndex = 167;
            this.htmlLabel5.TabStop = false;
            this.htmlLabel5.Text = "Progress hook procedure";
            // 
            // lbl_deployDir
            // 
            this.lbl_deployDir.AutoSize = false;
            this.lbl_deployDir.AutoSizeHeightOnly = true;
            this.lbl_deployDir.BackColor = System.Drawing.Color.Transparent;
            this.lbl_deployDir.BaseStylesheet = null;
            this.lbl_deployDir.IsSelectionEnabled = false;
            this.lbl_deployDir.Location = new System.Drawing.Point(268, 279);
            this.lbl_deployDir.Name = "lbl_deployDir";
            this.lbl_deployDir.Size = new System.Drawing.Size(621, 15);
            this.lbl_deployDir.TabIndex = 165;
            this.lbl_deployDir.TabStop = false;
            this.lbl_deployDir.Text = "deployment directory";
            // 
            // btStart
            // 
            this.btStart.BackGrndImage = null;
            this.btStart.GreyScaleBackGrndImage = null;
            this.btStart.IsFocused = false;
            this.btStart.IsHovered = false;
            this.btStart.IsPressed = false;
            this.btStart.Location = new System.Drawing.Point(30, 356);
            this.btStart.Name = "btStart";
            this.btStart.SetImgSize = new System.Drawing.Size(20, 20);
            this.btStart.Size = new System.Drawing.Size(117, 24);
            this.btStart.TabIndex = 116;
            this.btStart.Text = "Start deploying";
            // 
            // btReset
            // 
            this.btReset.BackGrndImage = null;
            this.btReset.GreyScaleBackGrndImage = null;
            this.btReset.IsFocused = false;
            this.btReset.IsHovered = false;
            this.btReset.IsPressed = false;
            this.btReset.Location = new System.Drawing.Point(292, 356);
            this.btReset.Name = "btReset";
            this.btReset.SetImgSize = new System.Drawing.Size(20, 20);
            this.btReset.Size = new System.Drawing.Size(112, 24);
            this.btReset.TabIndex = 131;
            this.btReset.Text = "Reset options";
            // 
            // htmlLabel7
            // 
            this.htmlLabel7.AutoSize = false;
            this.htmlLabel7.AutoSizeHeightOnly = true;
            this.htmlLabel7.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel7.BaseStylesheet = null;
            this.htmlLabel7.IsSelectionEnabled = false;
            this.htmlLabel7.Location = new System.Drawing.Point(30, 90);
            this.htmlLabel7.Name = "htmlLabel7";
            this.htmlLabel7.Size = new System.Drawing.Size(207, 30);
            this.htmlLabel7.TabIndex = 163;
            this.htmlLabel7.TabStop = false;
            this.htmlLabel7.Text = "Auto update source directory from the current environment";
            // 
            // toggleAutoUpdateSourceDir
            // 
            this.toggleAutoUpdateSourceDir.BackGrndImage = null;
            this.toggleAutoUpdateSourceDir.GreyScaleBackGrndImage = null;
            this.toggleAutoUpdateSourceDir.IsFocused = false;
            this.toggleAutoUpdateSourceDir.IsHovered = false;
            this.toggleAutoUpdateSourceDir.IsPressed = false;
            this.toggleAutoUpdateSourceDir.Location = new System.Drawing.Point(268, 90);
            this.toggleAutoUpdateSourceDir.Margin = new System.Windows.Forms.Padding(5);
            this.toggleAutoUpdateSourceDir.Name = "toggleAutoUpdateSourceDir";
            this.toggleAutoUpdateSourceDir.SetImgSize = new System.Drawing.Size(0, 0);
            this.toggleAutoUpdateSourceDir.Size = new System.Drawing.Size(40, 16);
            this.toggleAutoUpdateSourceDir.TabIndex = 164;
            this.toggleAutoUpdateSourceDir.ToggleSize = 30;
            // 
            // htmlLabel11
            // 
            this.htmlLabel11.AutoSize = false;
            this.htmlLabel11.AutoSizeHeightOnly = true;
            this.htmlLabel11.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel11.BaseStylesheet = null;
            this.htmlLabel11.IsSelectionEnabled = false;
            this.htmlLabel11.Location = new System.Drawing.Point(30, 178);
            this.htmlLabel11.Name = "htmlLabel11";
            this.htmlLabel11.Size = new System.Drawing.Size(223, 15);
            this.htmlLabel11.TabIndex = 161;
            this.htmlLabel11.TabStop = false;
            this.htmlLabel11.Text = "Only generate R-code during compilation";
            // 
            // toggleOnlyGenerateRcode
            // 
            this.toggleOnlyGenerateRcode.BackGrndImage = null;
            this.toggleOnlyGenerateRcode.GreyScaleBackGrndImage = null;
            this.toggleOnlyGenerateRcode.IsFocused = false;
            this.toggleOnlyGenerateRcode.IsHovered = false;
            this.toggleOnlyGenerateRcode.IsPressed = false;
            this.toggleOnlyGenerateRcode.Location = new System.Drawing.Point(268, 178);
            this.toggleOnlyGenerateRcode.Margin = new System.Windows.Forms.Padding(5);
            this.toggleOnlyGenerateRcode.Name = "toggleOnlyGenerateRcode";
            this.toggleOnlyGenerateRcode.SetImgSize = new System.Drawing.Size(0, 0);
            this.toggleOnlyGenerateRcode.Size = new System.Drawing.Size(40, 16);
            this.toggleOnlyGenerateRcode.TabIndex = 162;
            this.toggleOnlyGenerateRcode.ToggleSize = 30;
            // 
            // btSeeRules
            // 
            this.btSeeRules.BackGrndImage = null;
            this.btSeeRules.GreyScaleBackGrndImage = null;
            this.btSeeRules.IsFocused = false;
            this.btSeeRules.IsHovered = false;
            this.btSeeRules.IsPressed = false;
            this.btSeeRules.Location = new System.Drawing.Point(455, 321);
            this.btSeeRules.Name = "btSeeRules";
            this.btSeeRules.SetImgSize = new System.Drawing.Size(20, 20);
            this.btSeeRules.Size = new System.Drawing.Size(275, 24);
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
            this.lblCurEnv.Location = new System.Drawing.Point(268, 258);
            this.lblCurEnv.Name = "lblCurEnv";
            this.lblCurEnv.Size = new System.Drawing.Size(621, 15);
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
            this.htmlLabel9.Location = new System.Drawing.Point(30, 258);
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
            this.htmlLabel8.Text = "<b>Select the deployment profile</b>";
            // 
            // btDelete
            // 
            this.btDelete.BackGrndImage = null;
            this.btDelete.GreyScaleBackGrndImage = null;
            this.btDelete.IsFocused = false;
            this.btDelete.IsHovered = false;
            this.btDelete.IsPressed = false;
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
            this.btSaveAs.BackGrndImage = null;
            this.btSaveAs.GreyScaleBackGrndImage = null;
            this.btSaveAs.IsFocused = false;
            this.btSaveAs.IsHovered = false;
            this.btSaveAs.IsPressed = false;
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
            this.btSave.BackGrndImage = null;
            this.btSave.GreyScaleBackGrndImage = null;
            this.btSave.IsFocused = false;
            this.btSave.IsHovered = false;
            this.btSave.IsPressed = false;
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
            this.cbName.BackGrndImage = null;
            this.cbName.GreyScaleBackGrndImage = null;
            this.cbName.IsFocused = false;
            this.cbName.IsHovered = false;
            this.cbName.IsPressed = false;
            this.cbName.Location = new System.Drawing.Point(268, 29);
            this.cbName.Name = "cbName";
            this.cbName.SetImgSize = new System.Drawing.Size(0, 0);
            this.cbName.Size = new System.Drawing.Size(253, 21);
            this.cbName.TabIndex = 153;
            // 
            // btRules
            // 
            this.btRules.BackGrndImage = null;
            this.btRules.GreyScaleBackGrndImage = null;
            this.btRules.IsFocused = false;
            this.btRules.IsHovered = false;
            this.btRules.IsPressed = false;
            this.btRules.Location = new System.Drawing.Point(268, 321);
            this.btRules.Name = "btRules";
            this.btRules.SetImgSize = new System.Drawing.Size(20, 20);
            this.btRules.Size = new System.Drawing.Size(181, 24);
            this.btRules.TabIndex = 152;
            this.btRules.Text = "Modify deployment rules";
            // 
            // lbl_rules
            // 
            this.lbl_rules.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_rules.AutoSize = false;
            this.lbl_rules.AutoSizeHeightOnly = true;
            this.lbl_rules.BackColor = System.Drawing.Color.Transparent;
            this.lbl_rules.BaseStylesheet = null;
            this.lbl_rules.IsSelectionEnabled = false;
            this.lbl_rules.Location = new System.Drawing.Point(268, 300);
            this.lbl_rules.Name = "lbl_rules";
            this.lbl_rules.Size = new System.Drawing.Size(623, 15);
            this.lbl_rules.TabIndex = 149;
            this.lbl_rules.TabStop = false;
            this.lbl_rules.Text = "0 rules for step 1, 0 rules for step 2";
            // 
            // lbl_report
            // 
            this.lbl_report.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_report.AutoSize = false;
            this.lbl_report.AutoSizeHeightOnly = true;
            this.lbl_report.BackColor = System.Drawing.Color.Transparent;
            this.lbl_report.BaseStylesheet = null;
            this.lbl_report.Location = new System.Drawing.Point(30, 386);
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
            // title
            // 
            this.title.AutoSize = true;
            this.title.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.title.Location = new System.Drawing.Point(0, 0);
            this.title.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(198, 19);
            this.title.TabIndex = 2;
            this.title.Text = "COMPILE AND DEPLOY FILES";
            // 
            // btBrowse
            // 
            this.btBrowse.BackGrndImage = null;
            this.btBrowse.GreyScaleBackGrndImage = null;
            this.btBrowse.IsFocused = false;
            this.btBrowse.IsHovered = false;
            this.btBrowse.IsPressed = false;
            this.btBrowse.Location = new System.Drawing.Point(225, 62);
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
            this.btUndo.GreyScaleBackGrndImage = null;
            this.btUndo.IsFocused = false;
            this.btUndo.IsHovered = false;
            this.btUndo.IsPressed = false;
            this.btUndo.Location = new System.Drawing.Point(245, 62);
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
            this.fl_directory.Location = new System.Drawing.Point(268, 62);
            this.fl_directory.Name = "fl_directory";
            this.fl_directory.Size = new System.Drawing.Size(580, 20);
            this.fl_directory.TabIndex = 4;
            this.fl_directory.WaterMark = "Path to the directory to compile";
            // 
            // btOpen
            // 
            this.btOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btOpen.BackGrndImage = null;
            this.btOpen.GreyScaleBackGrndImage = null;
            this.btOpen.IsFocused = false;
            this.btOpen.IsHovered = false;
            this.btOpen.IsPressed = false;
            this.btOpen.Location = new System.Drawing.Point(851, 62);
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
            this.btHistoric.GreyScaleBackGrndImage = null;
            this.btHistoric.IsFocused = false;
            this.btHistoric.IsHovered = false;
            this.btHistoric.IsPressed = false;
            this.btHistoric.Location = new System.Drawing.Point(871, 62);
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
            this.htmlLabel3.Location = new System.Drawing.Point(30, 64);
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
            this.htmlLabel2.Location = new System.Drawing.Point(30, 126);
            this.htmlLabel2.Name = "htmlLabel2";
            this.htmlLabel2.Size = new System.Drawing.Size(161, 15);
            this.htmlLabel2.TabIndex = 120;
            this.htmlLabel2.TabStop = false;
            this.htmlLabel2.Text = "Explore folders recursively?";
            // 
            // toggleRecurs
            // 
            this.toggleRecurs.BackGrndImage = null;
            this.toggleRecurs.GreyScaleBackGrndImage = null;
            this.toggleRecurs.IsFocused = false;
            this.toggleRecurs.IsHovered = false;
            this.toggleRecurs.IsPressed = false;
            this.toggleRecurs.Location = new System.Drawing.Point(268, 126);
            this.toggleRecurs.Margin = new System.Windows.Forms.Padding(5);
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
            this.htmlLabel1.Location = new System.Drawing.Point(30, 152);
            this.htmlLabel1.Name = "htmlLabel1";
            this.htmlLabel1.Size = new System.Drawing.Size(207, 15);
            this.htmlLabel1.TabIndex = 123;
            this.htmlLabel1.TabStop = false;
            this.htmlLabel1.Text = "Force to single-process compilation?";
            // 
            // toggleMono
            // 
            this.toggleMono.BackGrndImage = null;
            this.toggleMono.GreyScaleBackGrndImage = null;
            this.toggleMono.IsFocused = false;
            this.toggleMono.IsHovered = false;
            this.toggleMono.IsPressed = false;
            this.toggleMono.Location = new System.Drawing.Point(268, 152);
            this.toggleMono.Margin = new System.Windows.Forms.Padding(5);
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
            this.htmlLabel4.Location = new System.Drawing.Point(30, 204);
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
            this.fl_nbProcess.Location = new System.Drawing.Point(268, 202);
            this.fl_nbProcess.Name = "fl_nbProcess";
            this.fl_nbProcess.Size = new System.Drawing.Size(34, 20);
            this.fl_nbProcess.TabIndex = 126;
            this.fl_nbProcess.WaterMark = null;
            // 
            // btCancel
            // 
            this.btCancel.BackGrndImage = null;
            this.btCancel.GreyScaleBackGrndImage = null;
            this.btCancel.IsFocused = false;
            this.btCancel.IsHovered = false;
            this.btCancel.IsPressed = false;
            this.btCancel.Location = new System.Drawing.Point(30, 356);
            this.btCancel.Name = "btCancel";
            this.btCancel.SetImgSize = new System.Drawing.Size(20, 20);
            this.btCancel.Size = new System.Drawing.Size(73, 24);
            this.btCancel.TabIndex = 122;
            this.btCancel.Text = "Cancel";
            // 
            // btReport
            // 
            this.btReport.BackGrndImage = null;
            this.btReport.GreyScaleBackGrndImage = null;
            this.btReport.IsFocused = false;
            this.btReport.IsHovered = false;
            this.btReport.IsPressed = false;
            this.btReport.Location = new System.Drawing.Point(410, 356);
            this.btReport.Name = "btReport";
            this.btReport.SetImgSize = new System.Drawing.Size(20, 20);
            this.btReport.Size = new System.Drawing.Size(148, 24);
            this.btReport.TabIndex = 132;
            this.btReport.Text = "Export report to html";
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.GradientIntensity = 5;
            this.progressBar.Location = new System.Drawing.Point(109, 356);
            this.progressBar.MarqueeWidth = 50;
            this.progressBar.Name = "progressBar";
            this.progressBar.Progress = 0F;
            this.progressBar.Size = new System.Drawing.Size(782, 24);
            this.progressBar.TabIndex = 114;
            // 
            // btTest
            // 
            this.btTest.BackGrndImage = null;
            this.btTest.GreyScaleBackGrndImage = null;
            this.btTest.IsFocused = false;
            this.btTest.IsHovered = false;
            this.btTest.IsPressed = false;
            this.btTest.Location = new System.Drawing.Point(153, 356);
            this.btTest.Name = "btTest";
            this.btTest.SetImgSize = new System.Drawing.Size(20, 20);
            this.btTest.Size = new System.Drawing.Size(133, 24);
            this.btTest.TabIndex = 170;
            this.btTest.Text = "Test deployment";
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
        private YamuiLabel title;
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
        private YamuiButton btReset;
        private YamuiButton btReport;
        private HtmlLabel linkurl;
        private HtmlLabel lbl_report;
        private HtmlLabel lbl_rules;
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
        private HtmlLabel htmlLabel7;
        private YamuiButtonToggle toggleAutoUpdateSourceDir;
        private HtmlLabel lbl_deployDir;
        private HtmlLabel htmlLabel5;
        private YamuiButton btOpenHook;
        private YamuiButton btTest;
    }
}
