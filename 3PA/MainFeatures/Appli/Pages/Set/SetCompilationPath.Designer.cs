using System.ComponentModel;
using YamuiFramework.Controls;

namespace _3PA.MainFeatures.Appli.Pages.Set {
    partial class SetCompilationPath {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetCompilationPath));
            this.dockedPanel = new YamuiFramework.Controls.YamuiScrollPage();
            this.bt_import = new YamuiFramework.Controls.YamuiButton();
            this.bt_modify = new YamuiFramework.Controls.YamuiButton();
            this.html_list = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.yamuiLabel2 = new YamuiFramework.Controls.YamuiLabel();
            this.lbl_about = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.yamuiLabel1 = new YamuiFramework.Controls.YamuiLabel();
            this.toolTip = new YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip();
            this.dockedPanel.ContentPanel.SuspendLayout();
            this.dockedPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // dockedPanel
            // 
            // 
            // dockedPanel.ContentPanel
            // 
            this.dockedPanel.ContentPanel.Controls.Add(this.bt_import);
            this.dockedPanel.ContentPanel.Controls.Add(this.bt_modify);
            this.dockedPanel.ContentPanel.Controls.Add(this.html_list);
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
            // bt_import
            // 
            this.bt_import.Location = new System.Drawing.Point(111, 102);
            this.bt_import.Name = "bt_import";
            this.bt_import.Size = new System.Drawing.Size(101, 23);
            this.bt_import.TabIndex = 56;
            this.bt_import.Text = "Read changes";
            // 
            // bt_modify
            // 
            this.bt_modify.Location = new System.Drawing.Point(30, 102);
            this.bt_modify.Name = "bt_modify";
            this.bt_modify.Size = new System.Drawing.Size(75, 23);
            this.bt_modify.TabIndex = 55;
            this.bt_modify.Text = "Modify";
            // 
            // html_list
            // 
            this.html_list.AutoSize = false;
            this.html_list.AutoSizeHeightOnly = true;
            this.html_list.BackColor = System.Drawing.Color.Transparent;
            this.html_list.BaseStylesheet = null;
            this.html_list.Location = new System.Drawing.Point(30, 179);
            this.html_list.Name = "html_list";
            this.html_list.Size = new System.Drawing.Size(682, 15);
            this.html_list.TabIndex = 54;
            this.html_list.TabStop = false;
            this.html_list.Text = " ?";
            // 
            // yamuiLabel2
            // 
            this.yamuiLabel2.AutoSize = true;
            this.yamuiLabel2.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel2.Location = new System.Drawing.Point(0, 146);
            this.yamuiLabel2.Margin = new System.Windows.Forms.Padding(5, 18, 5, 3);
            this.yamuiLabel2.Name = "yamuiLabel2";
            this.yamuiLabel2.Size = new System.Drawing.Size(298, 19);
            this.yamuiLabel2.TabIndex = 53;
            this.yamuiLabel2.Text = "CURRENT COMPILATION PATH REROUTING";
            // 
            // lbl_about
            // 
            this.lbl_about.AutoSize = false;
            this.lbl_about.AutoSizeHeightOnly = true;
            this.lbl_about.BackColor = System.Drawing.Color.Transparent;
            this.lbl_about.BaseStylesheet = null;
            this.lbl_about.Location = new System.Drawing.Point(30, 29);
            this.lbl_about.Name = "lbl_about";
            this.lbl_about.Size = new System.Drawing.Size(653, 60);
            this.lbl_about.TabIndex = 52;
            this.lbl_about.TabStop = false;
            this.lbl_about.Text = resources.GetString("lbl_about.Text");
            // 
            // yamuiLabel1
            // 
            this.yamuiLabel1.AutoSize = true;
            this.yamuiLabel1.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel1.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel1.Margin = new System.Windows.Forms.Padding(5, 18, 5, 3);
            this.yamuiLabel1.Name = "yamuiLabel1";
            this.yamuiLabel1.Size = new System.Drawing.Size(154, 19);
            this.yamuiLabel1.TabIndex = 51;
            this.yamuiLabel1.Text = "ABOUT THIS FEATURE";
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
            this.toolTip.TooltipCssClass = "htmltooltip";
            // 
            // SetCompilationPath
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dockedPanel);
            this.Name = "SetCompilationPath";
            this.Size = new System.Drawing.Size(720, 550);
            this.dockedPanel.ContentPanel.ResumeLayout(false);
            this.dockedPanel.ContentPanel.PerformLayout();
            this.dockedPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiScrollPage dockedPanel;
        private YamuiLabel yamuiLabel1;
        private YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip toolTip;
        private YamuiFramework.HtmlRenderer.WinForms.HtmlLabel lbl_about;
        private YamuiLabel yamuiLabel2;
        private YamuiButton bt_modify;
        private YamuiFramework.HtmlRenderer.WinForms.HtmlLabel html_list;
        private YamuiButton bt_import;
    }
}
