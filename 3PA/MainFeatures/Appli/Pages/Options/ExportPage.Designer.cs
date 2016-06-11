using System.ComponentModel;
using YamuiFramework.Controls;
using YamuiFramework.HtmlRenderer.WinForms;

namespace _3PA.MainFeatures.Appli.Pages.Options {
    partial class ExportPage {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportPage));
            this.tooltip = new YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip();
            this.dockedPanel = new YamuiFramework.Controls.YamuiScrollPage();
            this.btDownloadAll = new YamuiFramework.Controls.YamuiButtonImage();
            this.btRefresh = new YamuiFramework.Controls.YamuiButtonImage();
            this.htmlLabel2 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel1 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel3 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.btHistoric = new YamuiFramework.Controls.YamuiButtonImage();
            this.btOpen = new YamuiFramework.Controls.YamuiButtonImage();
            this.fl_directory = new YamuiFramework.Controls.YamuiTextBox();
            this.btBrowse = new YamuiFramework.Controls.YamuiButtonImage();
            this.yamuiLabel2 = new YamuiFramework.Controls.YamuiLabel();
            this.lbl_about = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.yamuiLabel1 = new YamuiFramework.Controls.YamuiLabel();
            this.htmlLabel4 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
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
            this.dockedPanel.ContentPanel.Controls.Add(this.htmlLabel4);
            this.dockedPanel.ContentPanel.Controls.Add(this.btDownloadAll);
            this.dockedPanel.ContentPanel.Controls.Add(this.btRefresh);
            this.dockedPanel.ContentPanel.Controls.Add(this.htmlLabel2);
            this.dockedPanel.ContentPanel.Controls.Add(this.htmlLabel1);
            this.dockedPanel.ContentPanel.Controls.Add(this.htmlLabel3);
            this.dockedPanel.ContentPanel.Controls.Add(this.btHistoric);
            this.dockedPanel.ContentPanel.Controls.Add(this.btOpen);
            this.dockedPanel.ContentPanel.Controls.Add(this.fl_directory);
            this.dockedPanel.ContentPanel.Controls.Add(this.btBrowse);
            this.dockedPanel.ContentPanel.Controls.Add(this.yamuiLabel2);
            this.dockedPanel.ContentPanel.Controls.Add(this.lbl_about);
            this.dockedPanel.ContentPanel.Controls.Add(this.yamuiLabel1);
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
            // btDownloadAll
            // 
            this.btDownloadAll.BackGrndImage = null;
            this.btDownloadAll.Location = new System.Drawing.Point(240, 189);
            this.btDownloadAll.Margin = new System.Windows.Forms.Padding(0);
            this.btDownloadAll.Name = "btDownloadAll";
            this.btDownloadAll.Size = new System.Drawing.Size(20, 20);
            this.btDownloadAll.TabIndex = 117;
            this.btDownloadAll.Text = "yamuiImageButton1";
            // 
            // btRefresh
            // 
            this.btRefresh.BackGrndImage = null;
            this.btRefresh.Location = new System.Drawing.Point(691, 192);
            this.btRefresh.Margin = new System.Windows.Forms.Padding(0);
            this.btRefresh.Name = "btRefresh";
            this.btRefresh.Size = new System.Drawing.Size(20, 20);
            this.btRefresh.TabIndex = 116;
            this.btRefresh.Text = "yamuiImageButton1";
            // 
            // htmlLabel2
            // 
            this.htmlLabel2.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel2.BaseStylesheet = null;
            this.htmlLabel2.Location = new System.Drawing.Point(510, 192);
            this.htmlLabel2.Name = "htmlLabel2";
            this.htmlLabel2.Size = new System.Drawing.Size(89, 15);
            this.htmlLabel2.TabIndex = 115;
            this.htmlLabel2.TabStop = false;
            this.htmlLabel2.Text = "<b>Distant file date</b>";
            // 
            // htmlLabel1
            // 
            this.htmlLabel1.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel1.BaseStylesheet = null;
            this.htmlLabel1.Location = new System.Drawing.Point(270, 192);
            this.htmlLabel1.Name = "htmlLabel1";
            this.htmlLabel1.Size = new System.Drawing.Size(77, 15);
            this.htmlLabel1.TabIndex = 114;
            this.htmlLabel1.TabStop = false;
            this.htmlLabel1.Text = "<b>Local file date</b>";
            // 
            // htmlLabel3
            // 
            this.htmlLabel3.AutoSize = false;
            this.htmlLabel3.AutoSizeHeightOnly = true;
            this.htmlLabel3.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel3.BaseStylesheet = null;
            this.htmlLabel3.IsSelectionEnabled = false;
            this.htmlLabel3.Location = new System.Drawing.Point(30, 155);
            this.htmlLabel3.Name = "htmlLabel3";
            this.htmlLabel3.Size = new System.Drawing.Size(186, 15);
            this.htmlLabel3.TabIndex = 113;
            this.htmlLabel3.TabStop = false;
            this.htmlLabel3.Text = "Path to the shared directory";
            // 
            // btHistoric
            // 
            this.btHistoric.BackGrndImage = null;
            this.btHistoric.Location = new System.Drawing.Point(691, 153);
            this.btHistoric.Margin = new System.Windows.Forms.Padding(0);
            this.btHistoric.Name = "btHistoric";
            this.btHistoric.Size = new System.Drawing.Size(20, 20);
            this.btHistoric.TabIndex = 6;
            this.btHistoric.Text = "yamuiImageButton1";
            // 
            // btOpen
            // 
            this.btOpen.BackGrndImage = null;
            this.btOpen.Location = new System.Drawing.Point(671, 153);
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
            this.fl_directory.Location = new System.Drawing.Point(240, 153);
            this.fl_directory.Name = "fl_directory";
            this.fl_directory.Size = new System.Drawing.Size(428, 20);
            this.fl_directory.TabIndex = 4;
            this.fl_directory.WaterMark = "Path to the shared directory";
            // 
            // btBrowse
            // 
            this.btBrowse.BackGrndImage = null;
            this.btBrowse.Location = new System.Drawing.Point(217, 153);
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
            this.yamuiLabel2.Location = new System.Drawing.Point(0, 124);
            this.yamuiLabel2.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel2.Name = "yamuiLabel2";
            this.yamuiLabel2.Size = new System.Drawing.Size(308, 19);
            this.yamuiLabel2.TabIndex = 2;
            this.yamuiLabel2.Text = "EXPORT AND SHARE YOUR CONFIGURATION";
            // 
            // lbl_about
            // 
            this.lbl_about.AutoSize = false;
            this.lbl_about.AutoSizeHeightOnly = true;
            this.lbl_about.BackColor = System.Drawing.Color.Transparent;
            this.lbl_about.BaseStylesheet = null;
            this.lbl_about.Location = new System.Drawing.Point(30, 29);
            this.lbl_about.Name = "lbl_about";
            this.lbl_about.Size = new System.Drawing.Size(673, 74);
            this.lbl_about.TabIndex = 1;
            this.lbl_about.TabStop = false;
            this.lbl_about.Text = resources.GetString("lbl_about.Text");
            // 
            // yamuiLabel1
            // 
            this.yamuiLabel1.AutoSize = true;
            this.yamuiLabel1.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel1.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel1.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel1.Name = "yamuiLabel1";
            this.yamuiLabel1.Size = new System.Drawing.Size(154, 19);
            this.yamuiLabel1.TabIndex = 0;
            this.yamuiLabel1.Text = "ABOUT THIS FEATURE";
            // 
            // htmlLabel4
            // 
            this.htmlLabel4.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel4.BaseStylesheet = null;
            this.htmlLabel4.Location = new System.Drawing.Point(198, 192);
            this.htmlLabel4.Name = "htmlLabel4";
            this.htmlLabel4.Size = new System.Drawing.Size(32, 15);
            this.htmlLabel4.TabIndex = 118;
            this.htmlLabel4.TabStop = false;
            this.htmlLabel4.Text = "<b>Auto?</b>";
            // 
            // ExportPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.dockedPanel);
            this.Name = "ExportPage";
            this.Size = new System.Drawing.Size(720, 550);
            this.dockedPanel.ContentPanel.ResumeLayout(false);
            this.dockedPanel.ContentPanel.PerformLayout();
            this.dockedPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private HtmlToolTip tooltip;
        private YamuiScrollPage dockedPanel;
        private YamuiLabel yamuiLabel1;
        private HtmlLabel lbl_about;
        private YamuiLabel yamuiLabel2;
        private YamuiButtonImage btHistoric;
        private YamuiButtonImage btOpen;
        private YamuiTextBox fl_directory;
        private YamuiButtonImage btBrowse;
        private HtmlLabel htmlLabel3;
        private HtmlLabel htmlLabel2;
        private HtmlLabel htmlLabel1;
        private YamuiButtonImage btDownloadAll;
        private YamuiButtonImage btRefresh;
        private HtmlLabel htmlLabel4;
    }
}
