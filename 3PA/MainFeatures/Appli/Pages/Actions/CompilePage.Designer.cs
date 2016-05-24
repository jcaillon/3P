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
            this.btUndo = new YamuiFramework.Controls.YamuiImageButton();
            this.lbl_rapport = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
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
            this.dockedPanel.ContentPanel.Controls.Add(this.btUndo);
            this.dockedPanel.ContentPanel.Controls.Add(this.lbl_rapport);
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
            // btUndo
            // 
            this.btUndo.BackGrndImage = null;
            this.btUndo.Location = new System.Drawing.Point(217, 29);
            this.btUndo.Margin = new System.Windows.Forms.Padding(0);
            this.btUndo.Name = "btUndo";
            this.btUndo.Size = new System.Drawing.Size(20, 20);
            this.btUndo.TabIndex = 118;
            this.btUndo.Text = "yamuiImageButton1";
            // 
            // lbl_rapport
            // 
            this.lbl_rapport.AutoSize = false;
            this.lbl_rapport.AutoSizeHeightOnly = true;
            this.lbl_rapport.BackColor = System.Drawing.Color.Transparent;
            this.lbl_rapport.BaseStylesheet = null;
            this.lbl_rapport.IsSelectionEnabled = false;
            this.lbl_rapport.Location = new System.Drawing.Point(30, 110);
            this.lbl_rapport.Name = "lbl_rapport";
            this.lbl_rapport.Size = new System.Drawing.Size(681, 45);
            this.lbl_rapport.TabIndex = 117;
            this.lbl_rapport.TabStop = false;
            this.lbl_rapport.Text = "<b>Last compilation report</b><br>Files compiled : <br>Using 16 processes";
            // 
            // btStart
            // 
            this.btStart.Location = new System.Drawing.Point(30, 52);
            this.btStart.Name = "btStart";
            this.btStart.Size = new System.Drawing.Size(139, 23);
            this.btStart.TabIndex = 116;
            this.btStart.Text = "Start the compilation";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(30, 81);
            this.progressBar.MarqueeWidth = 50;
            this.progressBar.Name = "progressBar";
            this.progressBar.Progress = 0F;
            this.progressBar.Size = new System.Drawing.Size(681, 23);
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
            this.fl_directory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.fl_directory.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fl_directory.CustomBackColor = System.Drawing.Color.Empty;
            this.fl_directory.CustomForeColor = System.Drawing.Color.Empty;
            this.fl_directory.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.fl_directory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.fl_directory.Location = new System.Drawing.Point(240, 29);
            this.fl_directory.Name = "fl_directory";
            this.fl_directory.Size = new System.Drawing.Size(428, 20);
            this.fl_directory.TabIndex = 4;
            this.fl_directory.WaterMark = "Path to the directory to compile";
            // 
            // btBrowse
            // 
            this.btBrowse.BackGrndImage = null;
            this.btBrowse.Location = new System.Drawing.Point(197, 29);
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
            this.yamuiLabel2.Size = new System.Drawing.Size(106, 19);
            this.yamuiLabel2.TabIndex = 2;
            this.yamuiLabel2.Text = "COMPILATION";
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
        private HtmlLabel lbl_rapport;
        private YamuiImageButton btUndo;
    }
}
