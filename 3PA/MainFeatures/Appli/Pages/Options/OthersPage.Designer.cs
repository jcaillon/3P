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
            this.fl_encodingfilter = new YamuiFramework.Controls.YamuiTextBox();
            this.htmlLabel5 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel9 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.htmlLabel6 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.cbEncoding = new YamuiFramework.Controls.YamuiComboBox();
            this.yamuiLabel1 = new YamuiFramework.Controls.YamuiLabel();
            this.btSave = new YamuiFramework.Controls.YamuiButton();
            this.btCancel = new YamuiFramework.Controls.YamuiButton();
            this.toolTip = new YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip();
            this.scrollPanel.ContentPanel.SuspendLayout();
            this.scrollPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // scrollPanel
            // 
            // 
            // scrollPanel.ContentPanel
            // 
            this.scrollPanel.ContentPanel.Controls.Add(this.fl_encodingfilter);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel5);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel9);
            this.scrollPanel.ContentPanel.Controls.Add(this.htmlLabel6);
            this.scrollPanel.ContentPanel.Controls.Add(this.cbEncoding);
            this.scrollPanel.ContentPanel.Controls.Add(this.yamuiLabel1);
            this.scrollPanel.ContentPanel.Controls.Add(this.btSave);
            this.scrollPanel.ContentPanel.Controls.Add(this.btCancel);
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
            // fl_encodingfilter
            // 
            this.fl_encodingfilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fl_encodingfilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.fl_encodingfilter.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fl_encodingfilter.CustomBackColor = System.Drawing.Color.Empty;
            this.fl_encodingfilter.CustomForeColor = System.Drawing.Color.Empty;
            this.fl_encodingfilter.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.fl_encodingfilter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.fl_encodingfilter.Location = new System.Drawing.Point(179, 74);
            this.fl_encodingfilter.Name = "fl_encodingfilter";
            this.fl_encodingfilter.Size = new System.Drawing.Size(713, 20);
            this.fl_encodingfilter.TabIndex = 80;
            this.fl_encodingfilter.WaterMark = null;
            // 
            // htmlLabel5
            // 
            this.htmlLabel5.AutoSize = false;
            this.htmlLabel5.AutoSizeHeightOnly = true;
            this.htmlLabel5.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel5.BaseStylesheet = null;
            this.htmlLabel5.IsSelectionEnabled = false;
            this.htmlLabel5.Location = new System.Drawing.Point(25, 74);
            this.htmlLabel5.Name = "htmlLabel5";
            this.htmlLabel5.Size = new System.Drawing.Size(148, 15);
            this.htmlLabel5.TabIndex = 79;
            this.htmlLabel5.TabStop = false;
            this.htmlLabel5.Text = "File name filter";
            // 
            // htmlLabel9
            // 
            this.htmlLabel9.AutoSize = false;
            this.htmlLabel9.AutoSizeHeightOnly = true;
            this.htmlLabel9.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel9.BaseStylesheet = null;
            this.htmlLabel9.IsSelectionEnabled = false;
            this.htmlLabel9.Location = new System.Drawing.Point(25, 25);
            this.htmlLabel9.Name = "htmlLabel9";
            this.htmlLabel9.Size = new System.Drawing.Size(367, 15);
            this.htmlLabel9.TabIndex = 78;
            this.htmlLabel9.TabStop = false;
            this.htmlLabel9.Text = "<b>Automatically change file encoding on opening</b>";
            // 
            // htmlLabel6
            // 
            this.htmlLabel6.AutoSize = false;
            this.htmlLabel6.AutoSizeHeightOnly = true;
            this.htmlLabel6.BackColor = System.Drawing.Color.Transparent;
            this.htmlLabel6.BaseStylesheet = null;
            this.htmlLabel6.IsSelectionEnabled = false;
            this.htmlLabel6.Location = new System.Drawing.Point(25, 46);
            this.htmlLabel6.Name = "htmlLabel6";
            this.htmlLabel6.Size = new System.Drawing.Size(148, 15);
            this.htmlLabel6.TabIndex = 77;
            this.htmlLabel6.TabStop = false;
            this.htmlLabel6.Text = "Encoding to apply";
            // 
            // cbEncoding
            // 
            this.cbEncoding.BackGrndImage = null;
            this.cbEncoding.GreyScaleBackGrndImage = null;
            this.cbEncoding.IsFocused = false;
            this.cbEncoding.IsHovered = false;
            this.cbEncoding.IsPressed = false;
            this.cbEncoding.Location = new System.Drawing.Point(179, 335);
            this.cbEncoding.Name = "cbEncoding";
            this.cbEncoding.SetImgSize = new System.Drawing.Size(0, 0);
            this.cbEncoding.Size = new System.Drawing.Size(375, 21);
            this.cbEncoding.TabIndex = 76;
            // 
            // yamuiLabel1
            // 
            this.yamuiLabel1.AutoSize = true;
            this.yamuiLabel1.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel1.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel1.Margin = new System.Windows.Forms.Padding(5, 18, 5, 3);
            this.yamuiLabel1.Name = "yamuiLabel1";
            this.yamuiLabel1.Size = new System.Drawing.Size(187, 19);
            this.yamuiLabel1.TabIndex = 74;
            this.yamuiLabel1.Text = "MISCELLANEOUS OPTIONS";
            // 
            // btSave
            // 
            this.btSave.BackGrndImage = null;
            this.btSave.GreyScaleBackGrndImage = null;
            this.btSave.IsFocused = false;
            this.btSave.IsHovered = false;
            this.btSave.IsPressed = false;
            this.btSave.Location = new System.Drawing.Point(25, 140);
            this.btSave.Name = "btSave";
            this.btSave.SetImgSize = new System.Drawing.Size(20, 20);
            this.btSave.Size = new System.Drawing.Size(80, 24);
            this.btSave.TabIndex = 64;
            this.btSave.Text = "Save all";
            // 
            // btCancel
            // 
            this.btCancel.BackGrndImage = null;
            this.btCancel.GreyScaleBackGrndImage = null;
            this.btCancel.IsFocused = false;
            this.btCancel.IsHovered = false;
            this.btCancel.IsPressed = false;
            this.btCancel.Location = new System.Drawing.Point(111, 140);
            this.btCancel.Name = "btCancel";
            this.btCancel.SetImgSize = new System.Drawing.Size(20, 20);
            this.btCancel.Size = new System.Drawing.Size(98, 24);
            this.btCancel.TabIndex = 65;
            this.btCancel.Text = "Cancel  all";
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
            this.Size = new System.Drawing.Size(900, 650);
            this.scrollPanel.ContentPanel.ResumeLayout(false);
            this.scrollPanel.ContentPanel.PerformLayout();
            this.scrollPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiScrollPanel scrollPanel;
        private YamuiButton btCancel;
        private YamuiButton btSave;
        private HtmlToolTip toolTip;
        private YamuiLabel yamuiLabel1;
        private HtmlLabel htmlLabel6;
        private YamuiComboBox cbEncoding;
        private YamuiTextBox fl_encodingfilter;
        private HtmlLabel htmlLabel5;
        private HtmlLabel htmlLabel9;
    }
}
