using System.ComponentModel;
using Yamui.Framework.Controls;
using Yamui.Framework.Fonts;
using Yamui.Framework.HtmlRenderer.WinForms;

namespace _3PA.MainFeatures.Appli.Pages.Set {
    partial class SetFileCustomInfo {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetFileCustomInfo));
            this.lbl_about = new HtmlLabel();
            this.yamuiLabel2 = new YamuiLabel();
            this.bt_delete = new YamuiButton();
            this.htmlLabel7 = new HtmlLabel();
            this.lb_FileName = new HtmlLabel();
            this.lb_SaveState = new HtmlLabel();
            this.htmlLabel9 = new HtmlLabel();
            this.htmlLabel6 = new HtmlLabel();
            this.htmlLabel5 = new HtmlLabel();
            this.htmlLabel4 = new HtmlLabel();
            this.htmlLabel3 = new HtmlLabel();
            this.htmlLabel2 = new HtmlLabel();
            this.htmlLabel1 = new HtmlLabel();
            this.yamuiLabel1 = new YamuiLabel();
            this.cb_info = new YamuiComboBox();
            this.fl_appliName = new YamuiTextBox();
            this.fl_appliVersion = new YamuiTextBox();
            this.fl_workPackage = new YamuiTextBox();
            this.fl_bugId = new YamuiTextBox();
            this.fl_correctionNb = new YamuiTextBox();
            this.fl_correctionDate = new YamuiTextBox();
            this.fl_correctionDesc = new YamuiTextBox();
            this.bt_ok = new YamuiButton();
            this.bt_cancel = new YamuiButton();
            this.bt_today = new YamuiButton();
            this.bt_default = new YamuiButton();
            this.bt_clear = new YamuiButton();
            this.toolTip = new HtmlToolTip();
            this.btTemplate = new YamuiButton();
            this.bt_SaveState = new YamuiPictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.bt_SaveState)).BeginInit();
            this.SuspendLayout();
            // 
            // scrollPanel
            // 
            // 
            // scrollPanel.ContentPanel
            // 
            this.Controls.Add(this.btTemplate);
            this.Controls.Add(this.lbl_about);
            this.Controls.Add(this.yamuiLabel2);
            this.Controls.Add(this.bt_delete);
            this.Controls.Add(this.htmlLabel7);
            this.Controls.Add(this.lb_FileName);
            this.Controls.Add(this.bt_SaveState);
            this.Controls.Add(this.lb_SaveState);
            this.Controls.Add(this.htmlLabel9);
            this.Controls.Add(this.htmlLabel6);
            this.Controls.Add(this.htmlLabel5);
            this.Controls.Add(this.htmlLabel4);
            this.Controls.Add(this.htmlLabel3);
            this.Controls.Add(this.htmlLabel2);
            this.Controls.Add(this.htmlLabel1);
            this.Controls.Add(this.yamuiLabel1);
            this.Controls.Add(this.cb_info);
            this.Controls.Add(this.fl_appliName);
            this.Controls.Add(this.fl_appliVersion);
            this.Controls.Add(this.fl_workPackage);
            this.Controls.Add(this.fl_bugId);
            this.Controls.Add(this.fl_correctionNb);
            this.Controls.Add(this.fl_correctionDate);
            this.Controls.Add(this.fl_correctionDesc);
            this.Controls.Add(this.bt_ok);
            this.Controls.Add(this.bt_cancel);
            this.Controls.Add(this.bt_today);
            this.Controls.Add(this.bt_default);
            this.Controls.Add(this.bt_clear);
            // 
            // lbl_about
            // 
            this.lbl_about.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_about.AutoSize = false;
            this.lbl_about.AutoSizeHeightOnly = true;
            this.lbl_about.BackColor = System.Drawing.Color.Transparent;
            this.lbl_about.BaseStylesheet = null;
            this.lbl_about.IsSelectionEnabled = false;
            this.lbl_about.Location = new System.Drawing.Point(30, 29);
            this.lbl_about.Name = "lbl_about";
            this.lbl_about.Size = new System.Drawing.Size(862, 30);
            this.lbl_about.TabIndex = 74;
            this.lbl_about.TabStop = false;
            this.lbl_about.Text = resources.GetString("lbl_about.Text");
            // 
            // yamuiLabel2
            // 
            this.yamuiLabel2.AutoSize = true;
            this.yamuiLabel2.Function = FontFunction.Heading;
            this.yamuiLabel2.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel2.Margin = new System.Windows.Forms.Padding(5, 18, 5, 3);
            this.yamuiLabel2.Name = "yamuiLabel2";
            this.yamuiLabel2.Size = new System.Drawing.Size(153, 19);
            this.yamuiLabel2.TabIndex = 73;
            this.yamuiLabel2.Text = "ABOUT THIS FEATURE";
            // 
            // bt_delete
            // 
            this.bt_delete.BackGrndImage = null;
            this.bt_delete.GreyScaleBackGrndImage = null;
            this.bt_delete.IsFocused = false;
            this.bt_delete.IsHovered = false;
            this.bt_delete.IsPressed = false;
            this.bt_delete.Location = new System.Drawing.Point(463, 508);
            this.bt_delete.Name = "bt_delete";
            this.bt_delete.SetImgSize = new System.Drawing.Size(20, 20);
            this.bt_delete.Size = new System.Drawing.Size(67, 23);
            this.bt_delete.TabIndex = 72;
            this.bt_delete.TabStop = false;
            this.bt_delete.Text = "Delete";
            // 
            // htmlLabel7
            // 
            this.htmlLabel7.AutoSize = false;
            this.htmlLabel7.AutoSizeHeightOnly = true;
            this.htmlLabel7.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel7.BaseStylesheet = null;
            this.htmlLabel7.IsSelectionEnabled = false;
            this.htmlLabel7.Location = new System.Drawing.Point(30, 207);
            this.htmlLabel7.Name = "htmlLabel7";
            this.htmlLabel7.Size = new System.Drawing.Size(157, 30);
            this.htmlLabel7.TabIndex = 71;
            this.htmlLabel7.TabStop = false;
            this.htmlLabel7.Text = "<b>Current file</b><br>and file info state";
            // 
            // lb_FileName
            // 
            this.lb_FileName.AutoSize = false;
            this.lb_FileName.AutoSizeHeightOnly = true;
            this.lb_FileName.BackColor = System.Drawing.Color.Transparent;
            this.lb_FileName.BaseStylesheet = null;
            this.lb_FileName.IsSelectionEnabled = false;
            this.lb_FileName.Location = new System.Drawing.Point(193, 207);
            this.lb_FileName.Name = "lb_FileName";
            this.lb_FileName.Size = new System.Drawing.Size(519, 15);
            this.lb_FileName.TabIndex = 70;
            this.lb_FileName.TabStop = false;
            this.lb_FileName.Text = "<b>Filename</b>";
            // 
            // lb_SaveState
            // 
            this.lb_SaveState.AutoSize = false;
            this.lb_SaveState.AutoSizeHeightOnly = true;
            this.lb_SaveState.BackColor = System.Drawing.Color.Transparent;
            this.lb_SaveState.BaseStylesheet = null;
            this.lb_SaveState.Location = new System.Drawing.Point(220, 231);
            this.lb_SaveState.Name = "lb_SaveState";
            this.lb_SaveState.Size = new System.Drawing.Size(492, 15);
            this.lb_SaveState.TabIndex = 68;
            this.lb_SaveState.TabStop = false;
            this.lb_SaveState.Text = "<b>!</b>";
            // 
            // htmlLabel9
            // 
            this.htmlLabel9.AutoSize = false;
            this.htmlLabel9.AutoSizeHeightOnly = true;
            this.htmlLabel9.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel9.BaseStylesheet = null;
            this.htmlLabel9.IsSelectionEnabled = false;
            this.htmlLabel9.Location = new System.Drawing.Point(30, 139);
            this.htmlLabel9.Name = "htmlLabel9";
            this.htmlLabel9.Size = new System.Drawing.Size(152, 60);
            this.htmlLabel9.TabIndex = 66;
            this.htmlLabel9.TabStop = false;
            this.htmlLabel9.Text = "<b>Auto-fill info</b><br>select an item to automatically pre-fill the info for th" +
    "e current file";
            // 
            // htmlLabel6
            // 
            this.htmlLabel6.AutoSize = false;
            this.htmlLabel6.AutoSizeHeightOnly = true;
            this.htmlLabel6.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel6.BaseStylesheet = null;
            this.htmlLabel6.IsSelectionEnabled = false;
            this.htmlLabel6.Location = new System.Drawing.Point(30, 394);
            this.htmlLabel6.Name = "htmlLabel6";
            this.htmlLabel6.Size = new System.Drawing.Size(157, 15);
            this.htmlLabel6.TabIndex = 57;
            this.htmlLabel6.TabStop = false;
            this.htmlLabel6.Text = "<b>Correction date</b>";
            // 
            // htmlLabel5
            // 
            this.htmlLabel5.AutoSize = false;
            this.htmlLabel5.AutoSizeHeightOnly = true;
            this.htmlLabel5.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel5.BaseStylesheet = null;
            this.htmlLabel5.IsSelectionEnabled = false;
            this.htmlLabel5.Location = new System.Drawing.Point(30, 368);
            this.htmlLabel5.Name = "htmlLabel5";
            this.htmlLabel5.Size = new System.Drawing.Size(157, 15);
            this.htmlLabel5.TabIndex = 56;
            this.htmlLabel5.TabStop = false;
            this.htmlLabel5.Text = "<b>Correction number</b>";
            // 
            // htmlLabel4
            // 
            this.htmlLabel4.AutoSize = false;
            this.htmlLabel4.AutoSizeHeightOnly = true;
            this.htmlLabel4.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel4.BaseStylesheet = null;
            this.htmlLabel4.IsSelectionEnabled = false;
            this.htmlLabel4.Location = new System.Drawing.Point(30, 342);
            this.htmlLabel4.Name = "htmlLabel4";
            this.htmlLabel4.Size = new System.Drawing.Size(157, 15);
            this.htmlLabel4.TabIndex = 55;
            this.htmlLabel4.TabStop = false;
            this.htmlLabel4.Text = "<b>Bug ID</b>";
            // 
            // htmlLabel3
            // 
            this.htmlLabel3.AutoSize = false;
            this.htmlLabel3.AutoSizeHeightOnly = true;
            this.htmlLabel3.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel3.BaseStylesheet = null;
            this.htmlLabel3.IsSelectionEnabled = false;
            this.htmlLabel3.Location = new System.Drawing.Point(30, 316);
            this.htmlLabel3.Name = "htmlLabel3";
            this.htmlLabel3.Size = new System.Drawing.Size(157, 15);
            this.htmlLabel3.TabIndex = 54;
            this.htmlLabel3.TabStop = false;
            this.htmlLabel3.Text = "<b>Work package</b>";
            // 
            // htmlLabel2
            // 
            this.htmlLabel2.AutoSize = false;
            this.htmlLabel2.AutoSizeHeightOnly = true;
            this.htmlLabel2.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel2.BaseStylesheet = null;
            this.htmlLabel2.IsSelectionEnabled = false;
            this.htmlLabel2.Location = new System.Drawing.Point(30, 290);
            this.htmlLabel2.Name = "htmlLabel2";
            this.htmlLabel2.Size = new System.Drawing.Size(157, 15);
            this.htmlLabel2.TabIndex = 53;
            this.htmlLabel2.TabStop = false;
            this.htmlLabel2.Text = "<b>Version of the application</b>";
            // 
            // htmlLabel1
            // 
            this.htmlLabel1.AutoSize = false;
            this.htmlLabel1.AutoSizeHeightOnly = true;
            this.htmlLabel1.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel1.BaseStylesheet = null;
            this.htmlLabel1.IsSelectionEnabled = false;
            this.htmlLabel1.Location = new System.Drawing.Point(30, 254);
            this.htmlLabel1.Name = "htmlLabel1";
            this.htmlLabel1.Size = new System.Drawing.Size(139, 30);
            this.htmlLabel1.TabIndex = 52;
            this.htmlLabel1.TabStop = false;
            this.htmlLabel1.Text = "<b>Name of the application</b><br> this file belongs to";
            // 
            // yamuiLabel1
            // 
            this.yamuiLabel1.AutoSize = true;
            this.yamuiLabel1.Function = FontFunction.Heading;
            this.yamuiLabel1.Location = new System.Drawing.Point(0, 105);
            this.yamuiLabel1.Margin = new System.Windows.Forms.Padding(5, 18, 5, 3);
            this.yamuiLabel1.Name = "yamuiLabel1";
            this.yamuiLabel1.Size = new System.Drawing.Size(259, 19);
            this.yamuiLabel1.TabIndex = 51;
            this.yamuiLabel1.Text = "INFORMATION ON THE CURRENT FILE";
            // 
            // cb_info
            // 
            this.cb_info.BackGrndImage = null;
            this.cb_info.GreyScaleBackGrndImage = null;
            this.cb_info.IsFocused = false;
            this.cb_info.IsHovered = false;
            this.cb_info.IsPressed = false;
            this.cb_info.Location = new System.Drawing.Point(193, 139);
            this.cb_info.Name = "cb_info";
            this.cb_info.SetImgSize = new System.Drawing.Size(0, 0);
            this.cb_info.Size = new System.Drawing.Size(394, 21);
            this.cb_info.TabIndex = 50;
            // 
            // fl_appliName
            // 
            this.fl_appliName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.fl_appliName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fl_appliName.CustomBackColor = System.Drawing.Color.Empty;
            this.fl_appliName.CustomForeColor = System.Drawing.Color.Empty;
            this.fl_appliName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.fl_appliName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.fl_appliName.Location = new System.Drawing.Point(193, 254);
            this.fl_appliName.Name = "fl_appliName";
            this.fl_appliName.Size = new System.Drawing.Size(266, 20);
            this.fl_appliName.TabIndex = 49;
            this.fl_appliName.WaterMark = "E.g. : BOI";
            // 
            // fl_appliVersion
            // 
            this.fl_appliVersion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.fl_appliVersion.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fl_appliVersion.CustomBackColor = System.Drawing.Color.Empty;
            this.fl_appliVersion.CustomForeColor = System.Drawing.Color.Empty;
            this.fl_appliVersion.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.fl_appliVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.fl_appliVersion.Location = new System.Drawing.Point(193, 290);
            this.fl_appliVersion.Name = "fl_appliVersion";
            this.fl_appliVersion.Size = new System.Drawing.Size(266, 20);
            this.fl_appliVersion.TabIndex = 48;
            this.fl_appliVersion.WaterMark = "E.g. : 65.000";
            // 
            // fl_workPackage
            // 
            this.fl_workPackage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.fl_workPackage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fl_workPackage.CustomBackColor = System.Drawing.Color.Empty;
            this.fl_workPackage.CustomForeColor = System.Drawing.Color.Empty;
            this.fl_workPackage.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.fl_workPackage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.fl_workPackage.Location = new System.Drawing.Point(193, 316);
            this.fl_workPackage.Name = "fl_workPackage";
            this.fl_workPackage.Size = new System.Drawing.Size(266, 20);
            this.fl_workPackage.TabIndex = 47;
            this.fl_workPackage.WaterMark = "E.g. : 101-33";
            // 
            // fl_bugId
            // 
            this.fl_bugId.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.fl_bugId.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fl_bugId.CustomBackColor = System.Drawing.Color.Empty;
            this.fl_bugId.CustomForeColor = System.Drawing.Color.Empty;
            this.fl_bugId.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.fl_bugId.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.fl_bugId.Location = new System.Drawing.Point(193, 342);
            this.fl_bugId.Name = "fl_bugId";
            this.fl_bugId.Size = new System.Drawing.Size(266, 20);
            this.fl_bugId.TabIndex = 46;
            this.fl_bugId.WaterMark = "E.g. : INC0999999";
            // 
            // fl_correctionNb
            // 
            this.fl_correctionNb.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.fl_correctionNb.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fl_correctionNb.CustomBackColor = System.Drawing.Color.Empty;
            this.fl_correctionNb.CustomForeColor = System.Drawing.Color.Empty;
            this.fl_correctionNb.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.fl_correctionNb.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.fl_correctionNb.Location = new System.Drawing.Point(193, 368);
            this.fl_correctionNb.Name = "fl_correctionNb";
            this.fl_correctionNb.Size = new System.Drawing.Size(266, 20);
            this.fl_correctionNb.TabIndex = 45;
            this.fl_correctionNb.WaterMark = "E.g. : 9";
            // 
            // fl_correctionDate
            // 
            this.fl_correctionDate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.fl_correctionDate.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fl_correctionDate.CustomBackColor = System.Drawing.Color.Empty;
            this.fl_correctionDate.CustomForeColor = System.Drawing.Color.Empty;
            this.fl_correctionDate.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.fl_correctionDate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.fl_correctionDate.Location = new System.Drawing.Point(266, 394);
            this.fl_correctionDate.Name = "fl_correctionDate";
            this.fl_correctionDate.Size = new System.Drawing.Size(103, 20);
            this.fl_correctionDate.TabIndex = 44;
            this.fl_correctionDate.WaterMark = null;
            // 
            // fl_correctionDesc
            // 
            this.fl_correctionDesc.AcceptsReturn = true;
            this.fl_correctionDesc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.fl_correctionDesc.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fl_correctionDesc.CustomBackColor = System.Drawing.Color.Empty;
            this.fl_correctionDesc.CustomForeColor = System.Drawing.Color.Empty;
            this.fl_correctionDesc.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.fl_correctionDesc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.fl_correctionDesc.Location = new System.Drawing.Point(193, 420);
            this.fl_correctionDesc.Multiline = true;
            this.fl_correctionDesc.Name = "fl_correctionDesc";
            this.fl_correctionDesc.Size = new System.Drawing.Size(519, 82);
            this.fl_correctionDesc.TabIndex = 43;
            this.fl_correctionDesc.WaterMark = "E.g. : Fixing a small bug";
            // 
            // bt_ok
            // 
            this.bt_ok.BackGrndImage = null;
            this.bt_ok.GreyScaleBackGrndImage = null;
            this.bt_ok.IsFocused = false;
            this.bt_ok.IsHovered = false;
            this.bt_ok.IsPressed = false;
            this.bt_ok.Location = new System.Drawing.Point(193, 508);
            this.bt_ok.Name = "bt_ok";
            this.bt_ok.SetImgSize = new System.Drawing.Size(20, 20);
            this.bt_ok.Size = new System.Drawing.Size(113, 23);
            this.bt_ok.TabIndex = 42;
            this.bt_ok.Text = "&Save and close";
            // 
            // bt_cancel
            // 
            this.bt_cancel.BackGrndImage = null;
            this.bt_cancel.GreyScaleBackGrndImage = null;
            this.bt_cancel.IsFocused = false;
            this.bt_cancel.IsHovered = false;
            this.bt_cancel.IsPressed = false;
            this.bt_cancel.Location = new System.Drawing.Point(312, 508);
            this.bt_cancel.Name = "bt_cancel";
            this.bt_cancel.SetImgSize = new System.Drawing.Size(20, 20);
            this.bt_cancel.Size = new System.Drawing.Size(124, 23);
            this.bt_cancel.TabIndex = 41;
            this.bt_cancel.Text = "&Cancel and close";
            // 
            // bt_today
            // 
            this.bt_today.BackGrndImage = null;
            this.bt_today.GreyScaleBackGrndImage = null;
            this.bt_today.IsFocused = false;
            this.bt_today.IsHovered = false;
            this.bt_today.IsPressed = false;
            this.bt_today.Location = new System.Drawing.Point(193, 394);
            this.bt_today.Name = "bt_today";
            this.bt_today.SetImgSize = new System.Drawing.Size(20, 20);
            this.bt_today.Size = new System.Drawing.Size(67, 20);
            this.bt_today.TabIndex = 33;
            this.bt_today.TabStop = false;
            this.bt_today.Text = "&Today";
            // 
            // bt_default
            // 
            this.bt_default.BackGrndImage = null;
            this.bt_default.GreyScaleBackGrndImage = null;
            this.bt_default.IsFocused = false;
            this.bt_default.IsHovered = false;
            this.bt_default.IsPressed = false;
            this.bt_default.Location = new System.Drawing.Point(607, 508);
            this.bt_default.Name = "bt_default";
            this.bt_default.SetImgSize = new System.Drawing.Size(20, 20);
            this.bt_default.Size = new System.Drawing.Size(105, 23);
            this.bt_default.TabIndex = 37;
            this.bt_default.TabStop = false;
            this.bt_default.Text = "Set as &default";
            // 
            // bt_clear
            // 
            this.bt_clear.BackGrndImage = null;
            this.bt_clear.GreyScaleBackGrndImage = null;
            this.bt_clear.IsFocused = false;
            this.bt_clear.IsHovered = false;
            this.bt_clear.IsPressed = false;
            this.bt_clear.Location = new System.Drawing.Point(536, 508);
            this.bt_clear.Name = "bt_clear";
            this.bt_clear.SetImgSize = new System.Drawing.Size(20, 20);
            this.bt_clear.Size = new System.Drawing.Size(65, 23);
            this.bt_clear.TabIndex = 36;
            this.bt_clear.TabStop = false;
            this.bt_clear.Text = "Clear";
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
            // btTemplate
            // 
            this.btTemplate.BackGrndImage = null;
            this.btTemplate.GreyScaleBackGrndImage = null;
            this.btTemplate.IsFocused = false;
            this.btTemplate.IsHovered = false;
            this.btTemplate.IsPressed = false;
            this.btTemplate.Location = new System.Drawing.Point(30, 65);
            this.btTemplate.Name = "btTemplate";
            this.btTemplate.SetImgSize = new System.Drawing.Size(20, 20);
            this.btTemplate.Size = new System.Drawing.Size(152, 23);
            this.btTemplate.TabIndex = 75;
            this.btTemplate.Text = "Modify the template";
            // 
            // bt_SaveState
            // 
            this.bt_SaveState.BackGrndImage = null;
            this.bt_SaveState.Location = new System.Drawing.Point(193, 228);
            this.bt_SaveState.Name = "bt_SaveState";
            this.bt_SaveState.Size = new System.Drawing.Size(20, 20);
            this.bt_SaveState.TabIndex = 69;
            this.bt_SaveState.TabStop = false;
            this.bt_SaveState.Text = "yamuiImageButton1";
            // 
            // SetFileCustomInfo
            // 
            this.Name = "SetFileCustomInfo";
            this.Size = new System.Drawing.Size(900, 650);
            ((System.ComponentModel.ISupportInitialize)(this.bt_SaveState)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        
        private YamuiComboBox cb_info;
        private YamuiTextBox fl_appliName;
        private YamuiTextBox fl_appliVersion;
        private YamuiTextBox fl_workPackage;
        private YamuiTextBox fl_bugId;
        private YamuiTextBox fl_correctionNb;
        private YamuiTextBox fl_correctionDate;
        private YamuiTextBox fl_correctionDesc;
        private YamuiButton bt_ok;
        private YamuiButton bt_cancel;
        private YamuiButton bt_today;
        private YamuiButton bt_default;
        private YamuiButton bt_clear;
        private YamuiLabel yamuiLabel1;
        private HtmlLabel htmlLabel1;
        private HtmlLabel htmlLabel2;
        private HtmlLabel htmlLabel3;
        private HtmlLabel htmlLabel4;
        private HtmlLabel htmlLabel6;
        private HtmlLabel htmlLabel5;
        private HtmlLabel htmlLabel9;
        private HtmlLabel lb_SaveState;
        private YamuiPictureBox bt_SaveState;
        private HtmlLabel lb_FileName;
        private HtmlLabel htmlLabel7;
        private HtmlToolTip toolTip;
        private YamuiButton bt_delete;
        private HtmlLabel lbl_about;
        private YamuiLabel yamuiLabel2;
        private YamuiButton btTemplate;
    }
}
