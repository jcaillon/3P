using System.ComponentModel;
using YamuiFramework.Controls;
using YamuiFramework.HtmlRenderer.WinForms;

namespace _3PA.MainFeatures.Appli.Pages.Options {
    partial class ShortCutsPage {
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
            this.bt_set = new YamuiFramework.Controls.YamuiButton();
            this.lbl_name = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.lbl_keys = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.yamuiLabel1 = new YamuiFramework.Controls.YamuiLabel();
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
            this.dockedPanel.ContentPanel.Controls.Add(this.bt_set);
            this.dockedPanel.ContentPanel.Controls.Add(this.lbl_name);
            this.dockedPanel.ContentPanel.Controls.Add(this.lbl_keys);
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
            // bt_set
            // 
            this.bt_set.Location = new System.Drawing.Point(469, 50);
            this.bt_set.Name = "bt_set";
            this.bt_set.Size = new System.Drawing.Size(142, 23);
            this.bt_set.TabIndex = 119;
            this.bt_set.Text = "Click to set";
            // 
            // lbl_name
            // 
            this.lbl_name.BackColor = System.Drawing.Color.Transparent;
            this.lbl_name.BaseStylesheet = null;
            this.lbl_name.Location = new System.Drawing.Point(30, 29);
            this.lbl_name.Name = "lbl_name";
            this.lbl_name.Size = new System.Drawing.Size(33, 15);
            this.lbl_name.TabIndex = 118;
            this.lbl_name.TabStop = false;
            this.lbl_name.Text = "<b>Name</b>";
            // 
            // lbl_keys
            // 
            this.lbl_keys.BackColor = System.Drawing.Color.Transparent;
            this.lbl_keys.BaseStylesheet = null;
            this.lbl_keys.Location = new System.Drawing.Point(469, 29);
            this.lbl_keys.Name = "lbl_keys";
            this.lbl_keys.Size = new System.Drawing.Size(26, 15);
            this.lbl_keys.TabIndex = 114;
            this.lbl_keys.TabStop = false;
            this.lbl_keys.Text = "<b>Keys</b>";
            // 
            // yamuiLabel1
            // 
            this.yamuiLabel1.AutoSize = true;
            this.yamuiLabel1.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel1.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel1.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel1.Name = "yamuiLabel1";
            this.yamuiLabel1.Size = new System.Drawing.Size(144, 19);
            this.yamuiLabel1.TabIndex = 0;
            this.yamuiLabel1.Text = "LIST OF SHORTCUTS";
            // 
            // ShortCutsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.dockedPanel);
            this.Name = "ShortCutsPage";
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
        private HtmlLabel lbl_keys;
        private HtmlLabel lbl_name;
        private YamuiButton bt_set;
    }
}
