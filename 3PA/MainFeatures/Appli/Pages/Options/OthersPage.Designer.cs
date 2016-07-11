using System.ComponentModel;
using YamuiFramework.Controls;
using YamuiFramework.HtmlRenderer.WinForms;

namespace _3PA.MainFeatures.Appli.Pages.Options {
    partial class OthersPage {
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
            this.scrollPanel = new YamuiFramework.Controls.YamuiScrollPanel();
            this.htmlLabel4 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel3 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel2 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel1 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.fl_tagtitle3 = new YamuiFramework.Controls.YamuiTextBox();
            this.fl_tagtitle2 = new YamuiFramework.Controls.YamuiTextBox();
            this.htmlLabel10 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel8 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel7 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.yamuiLabel3 = new YamuiFramework.Controls.YamuiLabel();
            this.fl_tagopen = new YamuiFramework.Controls.YamuiTextBox();
            this.fl_tagclose = new YamuiFramework.Controls.YamuiTextBox();
            this.fl_tagtitle1 = new YamuiFramework.Controls.YamuiTextBox();
            this.bttagsave = new YamuiFramework.Controls.YamuiButton();
            this.bttagcancel = new YamuiFramework.Controls.YamuiButton();
            this.toolTip = new YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip();
            this.scrollPanel.ContentPanel.SuspendLayout();
            this.scrollPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // yamuiScrollPage1
            // 
            // 
            // yamuiScrollPage1.ContentPanel
            // 
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel4);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel3);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel2);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel1);
            this.scrollPanel.ContentPanel.Controls.Add(this.fl_tagtitle3);
            this.scrollPanel.ContentPanel.Controls.Add(this.fl_tagtitle2);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel10);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel8);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel7);
            this.scrollPanel.ContentPanel.Controls.Add(this.yamuiLabel3);
            this.scrollPanel.ContentPanel.Controls.Add(this.fl_tagopen);
            this.scrollPanel.ContentPanel.Controls.Add(this.fl_tagclose);
            this.scrollPanel.ContentPanel.Controls.Add(this.fl_tagtitle1);
            this.scrollPanel.ContentPanel.Controls.Add(this.bttagsave);
            this.scrollPanel.ContentPanel.Controls.Add(this.bttagcancel);
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
            // htmlLabel4
            // 
            this.htmlLabel4.AutoSize = false;
            this.htmlLabel4.AutoSizeHeightOnly = true;
            this.htmlLabel4.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel4.BaseStylesheet = null;
            this.htmlLabel4.Location = new System.Drawing.Point(25, 105);
            this.htmlLabel4.Margin = new System.Windows.Forms.Padding(3, 15, 3, 3);
            this.htmlLabel4.Name = "htmlLabel4";
            this.htmlLabel4.Size = new System.Drawing.Size(148, 15);
            this.htmlLabel4.TabIndex = 73;
            this.htmlLabel4.TabStop = false;
            this.htmlLabel4.Text = "<b>Title block</b>";
            // 
            // htmlLabel3
            // 
            this.htmlLabel3.AutoSize = false;
            this.htmlLabel3.AutoSizeHeightOnly = true;
            this.htmlLabel3.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel3.BaseStylesheet = null;
            this.htmlLabel3.Location = new System.Drawing.Point(25, 25);
            this.htmlLabel3.Name = "htmlLabel3";
            this.htmlLabel3.Size = new System.Drawing.Size(148, 15);
            this.htmlLabel3.TabIndex = 72;
            this.htmlLabel3.TabStop = false;
            this.htmlLabel3.Text = "<b>Modification tag</b>";
            // 
            // htmlLabel2
            // 
            this.htmlLabel2.AutoSize = false;
            this.htmlLabel2.AutoSizeHeightOnly = true;
            this.htmlLabel2.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel2.BaseStylesheet = null;
            this.htmlLabel2.Location = new System.Drawing.Point(25, 218);
            this.htmlLabel2.Name = "htmlLabel2";
            this.htmlLabel2.Size = new System.Drawing.Size(121, 15);
            this.htmlLabel2.TabIndex = 71;
            this.htmlLabel2.TabStop = false;
            this.htmlLabel2.Text = "Title block \"footer\"";
            // 
            // htmlLabel1
            // 
            this.htmlLabel1.AutoSize = false;
            this.htmlLabel1.AutoSizeHeightOnly = true;
            this.htmlLabel1.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel1.BaseStylesheet = null;
            this.htmlLabel1.Location = new System.Drawing.Point(25, 182);
            this.htmlLabel1.Name = "htmlLabel1";
            this.htmlLabel1.Size = new System.Drawing.Size(121, 30);
            this.htmlLabel1.TabIndex = 70;
            this.htmlLabel1.TabStop = false;
            this.htmlLabel1.Text = "Title block repeated description line";
            // 
            // fl_tagtitle3
            // 
            this.fl_tagtitle3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fl_tagtitle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.fl_tagtitle3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fl_tagtitle3.CustomBackColor = System.Drawing.Color.Empty;
            this.fl_tagtitle3.CustomForeColor = System.Drawing.Color.Empty;
            this.fl_tagtitle3.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.fl_tagtitle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.fl_tagtitle3.Location = new System.Drawing.Point(179, 218);
            this.fl_tagtitle3.MultiLines = true;
            this.fl_tagtitle3.Name = "fl_tagtitle3";
            this.fl_tagtitle3.Size = new System.Drawing.Size(533, 50);
            this.fl_tagtitle3.TabIndex = 69;
            this.fl_tagtitle3.WaterMark = null;
            // 
            // fl_tagtitle2
            // 
            this.fl_tagtitle2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fl_tagtitle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.fl_tagtitle2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fl_tagtitle2.CustomBackColor = System.Drawing.Color.Empty;
            this.fl_tagtitle2.CustomForeColor = System.Drawing.Color.Empty;
            this.fl_tagtitle2.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.fl_tagtitle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.fl_tagtitle2.Location = new System.Drawing.Point(179, 182);
            this.fl_tagtitle2.Name = "fl_tagtitle2";
            this.fl_tagtitle2.Size = new System.Drawing.Size(533, 20);
            this.fl_tagtitle2.TabIndex = 68;
            this.fl_tagtitle2.WaterMark = null;
            // 
            // htmlLabel10
            // 
            this.htmlLabel10.AutoSize = false;
            this.htmlLabel10.AutoSizeHeightOnly = true;
            this.htmlLabel10.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel10.BaseStylesheet = null;
            this.htmlLabel10.Location = new System.Drawing.Point(25, 126);
            this.htmlLabel10.Name = "htmlLabel10";
            this.htmlLabel10.Size = new System.Drawing.Size(148, 15);
            this.htmlLabel10.TabIndex = 67;
            this.htmlLabel10.TabStop = false;
            this.htmlLabel10.Text = "Title block \"header\"";
            // 
            // htmlLabel8
            // 
            this.htmlLabel8.AutoSize = false;
            this.htmlLabel8.AutoSizeHeightOnly = true;
            this.htmlLabel8.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel8.BaseStylesheet = null;
            this.htmlLabel8.Location = new System.Drawing.Point(25, 72);
            this.htmlLabel8.Name = "htmlLabel8";
            this.htmlLabel8.Size = new System.Drawing.Size(148, 15);
            this.htmlLabel8.TabIndex = 61;
            this.htmlLabel8.TabStop = false;
            this.htmlLabel8.Text = "Closing modification tag";
            // 
            // htmlLabel7
            // 
            this.htmlLabel7.AutoSize = false;
            this.htmlLabel7.AutoSizeHeightOnly = true;
            this.htmlLabel7.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel7.BaseStylesheet = null;
            this.htmlLabel7.Location = new System.Drawing.Point(25, 46);
            this.htmlLabel7.Name = "htmlLabel7";
            this.htmlLabel7.Size = new System.Drawing.Size(148, 15);
            this.htmlLabel7.TabIndex = 60;
            this.htmlLabel7.TabStop = false;
            this.htmlLabel7.Text = "Opening modification tag";
            // 
            // yamuiLabel3
            // 
            this.yamuiLabel3.AutoSize = true;
            this.yamuiLabel3.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel3.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel3.Margin = new System.Windows.Forms.Padding(5, 18, 5, 3);
            this.yamuiLabel3.Name = "yamuiLabel3";
            this.yamuiLabel3.Size = new System.Drawing.Size(267, 19);
            this.yamuiLabel3.TabIndex = 58;
            this.yamuiLabel3.Text = "SET TAGS AND TITLE BLOCK TEMPLATE";
            // 
            // fl_tagopen
            // 
            this.fl_tagopen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fl_tagopen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.fl_tagopen.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fl_tagopen.CustomBackColor = System.Drawing.Color.Empty;
            this.fl_tagopen.CustomForeColor = System.Drawing.Color.Empty;
            this.fl_tagopen.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.fl_tagopen.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.fl_tagopen.Location = new System.Drawing.Point(179, 46);
            this.fl_tagopen.Name = "fl_tagopen";
            this.fl_tagopen.Size = new System.Drawing.Size(533, 20);
            this.fl_tagopen.TabIndex = 59;
            this.fl_tagopen.WaterMark = null;
            // 
            // fl_tagclose
            // 
            this.fl_tagclose.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fl_tagclose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.fl_tagclose.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fl_tagclose.CustomBackColor = System.Drawing.Color.Empty;
            this.fl_tagclose.CustomForeColor = System.Drawing.Color.Empty;
            this.fl_tagclose.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.fl_tagclose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.fl_tagclose.Location = new System.Drawing.Point(179, 72);
            this.fl_tagclose.Name = "fl_tagclose";
            this.fl_tagclose.Size = new System.Drawing.Size(533, 20);
            this.fl_tagclose.TabIndex = 62;
            this.fl_tagclose.WaterMark = null;
            // 
            // fl_tagtitle1
            // 
            this.fl_tagtitle1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fl_tagtitle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.fl_tagtitle1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fl_tagtitle1.CustomBackColor = System.Drawing.Color.Empty;
            this.fl_tagtitle1.CustomForeColor = System.Drawing.Color.Empty;
            this.fl_tagtitle1.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.fl_tagtitle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.fl_tagtitle1.Location = new System.Drawing.Point(179, 126);
            this.fl_tagtitle1.MultiLines = true;
            this.fl_tagtitle1.Name = "fl_tagtitle1";
            this.fl_tagtitle1.Size = new System.Drawing.Size(533, 50);
            this.fl_tagtitle1.TabIndex = 63;
            this.fl_tagtitle1.WaterMark = null;
            // 
            // bttagsave
            // 
            this.bttagsave.Location = new System.Drawing.Point(179, 274);
            this.bttagsave.Name = "bttagsave";
            this.bttagsave.Size = new System.Drawing.Size(60, 23);
            this.bttagsave.TabIndex = 64;
            this.bttagsave.Text = "Save";
            // 
            // bttagcancel
            // 
            this.bttagcancel.Location = new System.Drawing.Point(245, 274);
            this.bttagcancel.Name = "bttagcancel";
            this.bttagcancel.Size = new System.Drawing.Size(60, 23);
            this.bttagcancel.TabIndex = 65;
            this.bttagcancel.Text = "Cancel";
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
            // OthersPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.scrollPanel);
            this.Name = "OthersPage";
            this.Size = new System.Drawing.Size(720, 550);
            this.scrollPanel.ContentPanel.ResumeLayout(false);
            this.scrollPanel.ContentPanel.PerformLayout();
            this.scrollPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiScrollPanel scrollPanel;
        private YamuiLabel yamuiLabel3;
        private YamuiTextBox fl_tagopen;
        private HtmlLabel htmlLabel7;
        private YamuiTextBox fl_tagclose;
        private HtmlLabel htmlLabel8;
        private YamuiTextBox fl_tagtitle1;
        private YamuiButton bttagcancel;
        private YamuiButton bttagsave;
        private HtmlLabel htmlLabel10;
        private HtmlToolTip toolTip;
        private YamuiTextBox fl_tagtitle3;
        private YamuiTextBox fl_tagtitle2;
        private HtmlLabel htmlLabel2;
        private HtmlLabel htmlLabel1;
        private HtmlLabel htmlLabel4;
        private HtmlLabel htmlLabel3;
    }
}
