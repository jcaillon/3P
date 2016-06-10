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
            this.dockedPanel = new YamuiFramework.Controls.YamuiScrollPage();
            this.bt_export = new YamuiFramework.Controls.YamuiButton();
            this.btReset = new YamuiFramework.Controls.YamuiButton();
            this.htmlLabel6 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel5 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.fl_exclude = new YamuiFramework.Controls.YamuiTextBox();
            this.fl_include = new YamuiFramework.Controls.YamuiTextBox();
            this.fl_nbProcess = new YamuiFramework.Controls.YamuiTextBox();
            this.htmlLabel4 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.toggleMono = new YamuiFramework.Controls.YamuiToggle();
            this.htmlLabel1 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.btCancel = new YamuiFramework.Controls.YamuiButton();
            this.toggleRecurs = new YamuiFramework.Controls.YamuiToggle();
            this.htmlLabel2 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.btUndo = new YamuiFramework.Controls.YamuiImageButton();
            this.lbl_report = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.btStart = new YamuiFramework.Controls.YamuiButton();
            this.progressBar = new YamuiFramework.Controls.YamuiProgressBar();
            this.htmlLabel3 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.btHistoric = new YamuiFramework.Controls.YamuiImageButton();
            this.btOpen = new YamuiFramework.Controls.YamuiImageButton();
            this.fl_directory = new YamuiFramework.Controls.YamuiTextBox();
            this.btBrowse = new YamuiFramework.Controls.YamuiImageButton();
            this.yamuiLabel2 = new YamuiFramework.Controls.YamuiLabel();
            this.dockedPanel.ContentPanel.SuspendLayout();
            this.dockedPanel.SuspendLayout();
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
            this.tooltip.TooltipCssClass = "htmltooltip";
            this.tooltip.UseAnimation = false;
            this.tooltip.UseFading = false;
            // 
            // dockedPanel
            // 
            // 
            // dockedPanel.ContentPanel
            // 
            this.dockedPanel.ContentPanel.Controls.Add(this.bt_export);
            this.dockedPanel.ContentPanel.Controls.Add(this.btReset);
            this.dockedPanel.ContentPanel.Controls.Add(this.htmlLabel6);
            this.dockedPanel.ContentPanel.Controls.Add(this.htmlLabel5);
            this.dockedPanel.ContentPanel.Controls.Add(this.fl_exclude);
            this.dockedPanel.ContentPanel.Controls.Add(this.fl_include);
            this.dockedPanel.ContentPanel.Controls.Add(this.fl_nbProcess);
            this.dockedPanel.ContentPanel.Controls.Add(this.htmlLabel4);
            this.dockedPanel.ContentPanel.Controls.Add(this.toggleMono);
            this.dockedPanel.ContentPanel.Controls.Add(this.htmlLabel1);
            this.dockedPanel.ContentPanel.Controls.Add(this.btCancel);
            this.dockedPanel.ContentPanel.Controls.Add(this.toggleRecurs);
            this.dockedPanel.ContentPanel.Controls.Add(this.htmlLabel2);
            this.dockedPanel.ContentPanel.Controls.Add(this.btUndo);
            this.dockedPanel.ContentPanel.Controls.Add(this.lbl_report);
            this.dockedPanel.ContentPanel.Controls.Add(this.btStart);
            this.dockedPanel.ContentPanel.Controls.Add(this.progressBar);
            this.dockedPanel.ContentPanel.Controls.Add(this.htmlLabel3);
            this.dockedPanel.ContentPanel.Controls.Add(this.btHistoric);
            this.dockedPanel.ContentPanel.Controls.Add(this.btOpen);
            this.dockedPanel.ContentPanel.Controls.Add(this.fl_directory);
            this.dockedPanel.ContentPanel.Controls.Add(this.btBrowse);
            this.dockedPanel.ContentPanel.Controls.Add(this.yamuiLabel2);
            this.dockedPanel.ContentPanel.Location = new System.Drawing.Point(0, 0);
            this.dockedPanel.ContentPanel.Name = "ContentPanel";
            this.dockedPanel.ContentPanel.OwnerPage = this.dockedPanel;
            this.dockedPanel.ContentPanel.Size = new System.Drawing.Size(720, 550);
            this.dockedPanel.ContentPanel.TabIndex = 0;
            this.dockedPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dockedPanel.Location = new System.Drawing.Point(0, 0);
            this.dockedPanel.Name = "dockedPanel";
            this.dockedPanel.Size = new System.Drawing.Size(720, 550);
            this.dockedPanel.TabIndex = 0;
            // 
            // bt_export
            // 
            this.bt_export.Location = new System.Drawing.Point(281, 189);
            this.bt_export.Name = "bt_export";
            this.bt_export.Size = new System.Drawing.Size(148, 23);
            this.bt_export.TabIndex = 132;
            this.bt_export.Text = "Export report to html";
            // 
            // btReset
            // 
            this.btReset.Location = new System.Drawing.Point(175, 189);
            this.btReset.Name = "btReset";
            this.btReset.Size = new System.Drawing.Size(100, 23);
            this.btReset.TabIndex = 131;
            this.btReset.Text = "Reset options";
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
            // toggleMono
            // 
            this.toggleMono.Checked = false;
            this.toggleMono.Location = new System.Drawing.Point(268, 81);
            this.toggleMono.Name = "toggleMono";
            this.toggleMono.Size = new System.Drawing.Size(40, 16);
            this.toggleMono.TabIndex = 124;
            this.toggleMono.Text = " ";
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
            this.htmlLabel1.Text = "Force to mono-process compilation?";
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(30, 189);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(73, 23);
            this.btCancel.TabIndex = 122;
            this.btCancel.Text = "Cancel";
            // 
            // toggleRecurs
            // 
            this.toggleRecurs.Checked = false;
            this.toggleRecurs.Location = new System.Drawing.Point(268, 55);
            this.toggleRecurs.Name = "toggleRecurs";
            this.toggleRecurs.Size = new System.Drawing.Size(40, 16);
            this.toggleRecurs.TabIndex = 121;
            this.toggleRecurs.Text = " ";
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
            // btUndo
            // 
            this.btUndo.BackGrndImage = null;
            this.btUndo.Location = new System.Drawing.Point(245, 29);
            this.btUndo.Margin = new System.Windows.Forms.Padding(0);
            this.btUndo.Name = "btUndo";
            this.btUndo.Size = new System.Drawing.Size(20, 20);
            this.btUndo.TabIndex = 118;
            this.btUndo.Text = "yamuiImageButton1";
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
            // btStart
            // 
            this.btStart.Location = new System.Drawing.Point(30, 189);
            this.btStart.Name = "btStart";
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
            // btHistoric
            // 
            this.btHistoric.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btHistoric.BackGrndImage = null;
            this.btHistoric.Location = new System.Drawing.Point(691, 29);
            this.btHistoric.Margin = new System.Windows.Forms.Padding(0);
            this.btHistoric.Name = "btHistoric";
            this.btHistoric.Size = new System.Drawing.Size(20, 20);
            this.btHistoric.TabIndex = 6;
            this.btHistoric.Text = "yamuiImageButton1";
            // 
            // btOpen
            // 
            this.btOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btOpen.BackGrndImage = null;
            this.btOpen.Location = new System.Drawing.Point(671, 29);
            this.btOpen.Margin = new System.Windows.Forms.Padding(0);
            this.btOpen.Name = "btOpen";
            this.btOpen.Size = new System.Drawing.Size(20, 20);
            this.btOpen.TabIndex = 5;
            this.btOpen.Text = "yamuiImageButton1";
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
            // btBrowse
            // 
            this.btBrowse.BackGrndImage = null;
            this.btBrowse.Location = new System.Drawing.Point(225, 29);
            this.btBrowse.Margin = new System.Windows.Forms.Padding(0);
            this.btBrowse.Name = "btBrowse";
            this.btBrowse.Size = new System.Drawing.Size(20, 20);
            this.btBrowse.TabIndex = 3;
            this.btBrowse.Text = "yamuiImageButton1";
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
            // CompilePage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.dockedPanel);
            this.Name = "CompilePage";
            this.Size = new System.Drawing.Size(720, 550);
            this.dockedPanel.ContentPanel.ResumeLayout(false);
            this.dockedPanel.ContentPanel.PerformLayout();
            this.dockedPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private HtmlToolTip tooltip;
        private YamuiScrollPage dockedPanel;
        private YamuiLabel yamuiLabel2;
        private YamuiImageButton btHistoric;
        private YamuiImageButton btOpen;
        private YamuiTextBox fl_directory;
        private YamuiImageButton btBrowse;
        private HtmlLabel htmlLabel3;
        private YamuiProgressBar progressBar;
        private YamuiButton btStart;
        private HtmlLabel lbl_report;
        private YamuiImageButton btUndo;
        private HtmlLabel htmlLabel2;
        private YamuiToggle toggleRecurs;
        private YamuiButton btCancel;
        private HtmlLabel htmlLabel1;
        private YamuiToggle toggleMono;
        private HtmlLabel htmlLabel4;
        private YamuiTextBox fl_nbProcess;
        private HtmlLabel htmlLabel6;
        private HtmlLabel htmlLabel5;
        private YamuiTextBox fl_exclude;
        private YamuiTextBox fl_include;
        private YamuiButton btReset;
        private YamuiButton bt_export;
    }
}
