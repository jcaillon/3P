using System.ComponentModel;
using YamuiFramework.Controls;
using YamuiFramework.HtmlRenderer.WinForms;
using _3PA.MainFeatures.Appli.Pages.Options;

namespace _3PA.MainFeatures.Appli.Pages.Actions {
    partial class CompilePage {
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
            this.progressBar = new YamuiFramework.Controls.YamuiProgressBar();
            this.btReset = new YamuiFramework.Controls.YamuiButton();
            this.bt_export = new YamuiFramework.Controls.YamuiButton();
            this.lbl_report = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
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
            this.scrollPanel.ContentPanel.Controls.Add(this.progressBar);
            this.scrollPanel.ContentPanel.Controls.Add(this.btReset);
            this.scrollPanel.ContentPanel.Controls.Add(this.bt_export);
            this.scrollPanel.ContentPanel.Controls.Add(this.lbl_report);
            this.scrollPanel.ContentPanel.Location = new System.Drawing.Point(0, 0);
            this.scrollPanel.ContentPanel.Name = "ContentPanel";
            this.scrollPanel.ContentPanel.OwnerPanel = this.scrollPanel;
            this.scrollPanel.ContentPanel.Size = new System.Drawing.Size(720, 550);
            this.scrollPanel.ContentPanel.TabIndex = 0;
            this.scrollPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scrollPanel.Location = new System.Drawing.Point(0, 0);
            this.scrollPanel.Name = "scrollPanel";
            this.scrollPanel.Size = new System.Drawing.Size(720, 550);
            this.scrollPanel.TabIndex = 0;
            // 
            // linkurl
            // 
            this.linkurl.BackColor = System.Drawing.Color.Transparent;
            this.linkurl.BaseStylesheet = null;
            this.linkurl.Location = new System.Drawing.Point(257, 2);
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
            this.yamuiLabel2.Size = new System.Drawing.Size(209, 19);
            this.yamuiLabel2.TabIndex = 2;
            this.yamuiLabel2.Text = "COMPILE ENTIRE DIRECTORIES";
            // 
            // btBrowse
            // 
            this.btBrowse.BackGrndImage = null;
            this.btBrowse.HideFocusedIndicator = false;
            this.btBrowse.Location = new System.Drawing.Point(225, 29);
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
            this.btUndo.HideFocusedIndicator = false;
            this.btUndo.Location = new System.Drawing.Point(245, 29);
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
            this.fl_directory.Location = new System.Drawing.Point(268, 29);
            this.fl_directory.Name = "fl_directory";
            this.fl_directory.Size = new System.Drawing.Size(400, 20);
            this.fl_directory.TabIndex = 4;
            this.fl_directory.WaterMark = "Path to the directory to compile";
            // 
            // btOpen
            // 
            this.btOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btOpen.BackGrndImage = null;
            this.btOpen.HideFocusedIndicator = false;
            this.btOpen.Location = new System.Drawing.Point(671, 29);
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
            this.btHistoric.HideFocusedIndicator = false;
            this.btHistoric.Location = new System.Drawing.Point(691, 29);
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
            this.htmlLabel3.Location = new System.Drawing.Point(30, 31);
            this.htmlLabel3.Name = "htmlLabel3";
            this.htmlLabel3.Size = new System.Drawing.Size(161, 15);
            this.htmlLabel3.TabIndex = 113;
            this.htmlLabel3.TabStop = false;
            this.htmlLabel3.Text = "Select a directory to compile";
            // 
            // htmlLabel2
            // 
            this.htmlLabel2.AutoSize = false;
            this.htmlLabel2.AutoSizeHeightOnly = true;
            this.htmlLabel2.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel2.BaseStylesheet = null;
            this.htmlLabel2.IsSelectionEnabled = false;
            this.htmlLabel2.Location = new System.Drawing.Point(30, 55);
            this.htmlLabel2.Name = "htmlLabel2";
            this.htmlLabel2.Size = new System.Drawing.Size(161, 15);
            this.htmlLabel2.TabIndex = 120;
            this.htmlLabel2.TabStop = false;
            this.htmlLabel2.Text = "Explore files recursively?";
            // 
            // toggleRecurs
            // 
            this.toggleRecurs.BackGrndImage = null;
            this.toggleRecurs.Location = new System.Drawing.Point(268, 55);
            this.toggleRecurs.Name = "toggleRecurs";
            this.toggleRecurs.SetImgSize = new System.Drawing.Size(0, 0);
            this.toggleRecurs.Size = new System.Drawing.Size(40, 16);
            this.toggleRecurs.TabIndex = 121;
            // 
            // htmlLabel1
            // 
            this.htmlLabel1.AutoSize = false;
            this.htmlLabel1.AutoSizeHeightOnly = true;
            this.htmlLabel1.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel1.BaseStylesheet = null;
            this.htmlLabel1.IsSelectionEnabled = false;
            this.htmlLabel1.Location = new System.Drawing.Point(30, 81);
            this.htmlLabel1.Name = "htmlLabel1";
            this.htmlLabel1.Size = new System.Drawing.Size(207, 15);
            this.htmlLabel1.TabIndex = 123;
            this.htmlLabel1.TabStop = false;
            this.htmlLabel1.Text = "Force to single-process compilation?";
            // 
            // toggleMono
            // 
            this.toggleMono.BackGrndImage = null;
            this.toggleMono.Location = new System.Drawing.Point(268, 81);
            this.toggleMono.Name = "toggleMono";
            this.toggleMono.SetImgSize = new System.Drawing.Size(0, 0);
            this.toggleMono.Size = new System.Drawing.Size(40, 16);
            this.toggleMono.TabIndex = 124;
            // 
            // htmlLabel4
            // 
            this.htmlLabel4.AutoSize = false;
            this.htmlLabel4.AutoSizeHeightOnly = true;
            this.htmlLabel4.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel4.BaseStylesheet = null;
            this.htmlLabel4.IsSelectionEnabled = false;
            this.htmlLabel4.Location = new System.Drawing.Point(30, 107);
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
            this.fl_nbProcess.Location = new System.Drawing.Point(268, 107);
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
            this.htmlLabel5.Location = new System.Drawing.Point(30, 133);
            this.htmlLabel5.Name = "htmlLabel5";
            this.htmlLabel5.Size = new System.Drawing.Size(207, 15);
            this.htmlLabel5.TabIndex = 129;
            this.htmlLabel5.TabStop = false;
            this.htmlLabel5.Text = "Filter for files to <b>include</b>";
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
            this.fl_include.Location = new System.Drawing.Point(268, 133);
            this.fl_include.Name = "fl_include";
            this.fl_include.Size = new System.Drawing.Size(400, 20);
            this.fl_include.TabIndex = 127;
            this.fl_include.WaterMark = "No filter";
            // 
            // htmlLabel6
            // 
            this.htmlLabel6.AutoSize = false;
            this.htmlLabel6.AutoSizeHeightOnly = true;
            this.htmlLabel6.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel6.BaseStylesheet = null;
            this.htmlLabel6.IsSelectionEnabled = false;
            this.htmlLabel6.Location = new System.Drawing.Point(30, 159);
            this.htmlLabel6.Name = "htmlLabel6";
            this.htmlLabel6.Size = new System.Drawing.Size(207, 15);
            this.htmlLabel6.TabIndex = 130;
            this.htmlLabel6.TabStop = false;
            this.htmlLabel6.Text = "Filter for files to <b>exclude</b>";
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
            this.fl_exclude.Location = new System.Drawing.Point(268, 159);
            this.fl_exclude.Name = "fl_exclude";
            this.fl_exclude.Size = new System.Drawing.Size(400, 20);
            this.fl_exclude.TabIndex = 128;
            this.fl_exclude.WaterMark = "No filter";
            // 
            // btCancel
            // 
            this.btCancel.BackGrndImage = null;
            this.btCancel.Location = new System.Drawing.Point(30, 189);
            this.btCancel.Name = "btCancel";
            this.btCancel.SetImgSize = new System.Drawing.Size(0, 0);
            this.btCancel.Size = new System.Drawing.Size(73, 23);
            this.btCancel.TabIndex = 122;
            this.btCancel.Text = "Cancel";
            // 
            // btStart
            // 
            this.btStart.BackGrndImage = null;
            this.btStart.Location = new System.Drawing.Point(30, 189);
            this.btStart.Name = "btStart";
            this.btStart.SetImgSize = new System.Drawing.Size(0, 0);
            this.btStart.Size = new System.Drawing.Size(139, 23);
            this.btStart.TabIndex = 116;
            this.btStart.Text = "Start the compilation";
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.GradientIntensity = 5;
            this.progressBar.Location = new System.Drawing.Point(109, 189);
            this.progressBar.MarqueeWidth = 50;
            this.progressBar.Name = "progressBar";
            this.progressBar.Progress = 0F;
            this.progressBar.Size = new System.Drawing.Size(602, 23);
            this.progressBar.TabIndex = 114;
            // 
            // btReset
            // 
            this.btReset.BackGrndImage = null;
            this.btReset.Location = new System.Drawing.Point(175, 189);
            this.btReset.Name = "btReset";
            this.btReset.SetImgSize = new System.Drawing.Size(0, 0);
            this.btReset.Size = new System.Drawing.Size(100, 23);
            this.btReset.TabIndex = 131;
            this.btReset.Text = "Reset options";
            // 
            // bt_export
            // 
            this.bt_export.BackGrndImage = null;
            this.bt_export.Location = new System.Drawing.Point(281, 189);
            this.bt_export.Name = "bt_export";
            this.bt_export.SetImgSize = new System.Drawing.Size(0, 0);
            this.bt_export.Size = new System.Drawing.Size(148, 23);
            this.bt_export.TabIndex = 132;
            this.bt_export.Text = "Export report to html";
            // 
            // lbl_report
            // 
            this.lbl_report.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_report.AutoSize = false;
            this.lbl_report.AutoSizeHeightOnly = true;
            this.lbl_report.BackColor = System.Drawing.Color.Transparent;
            this.lbl_report.BaseStylesheet = null;
            this.lbl_report.Location = new System.Drawing.Point(30, 223);
            this.lbl_report.Name = "lbl_report";
            this.lbl_report.Size = new System.Drawing.Size(681, 45);
            this.lbl_report.TabIndex = 117;
            this.lbl_report.TabStop = false;
            this.lbl_report.Text = "<b>Last compilation report</b><br>Files compiled : <br>Using 16 processes";
            // 
            // CompilePage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.scrollPanel);
            this.Name = "CompilePage";
            this.Size = new System.Drawing.Size(720, 550);
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
        private HtmlLabel lbl_report;
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
    }
}
