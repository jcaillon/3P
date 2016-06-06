using System.ComponentModel;
using YamuiFramework.Controls;
using YamuiFramework.HtmlRenderer.WinForms;

namespace _3PA.MainFeatures.Appli.Pages.Options {
    partial class OptionPage {
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
            // OptionPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.dockedPanel);
            this.Name = "OptionPage";
            this.Size = new System.Drawing.Size(720, 550);
            this.dockedPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private HtmlToolTip tooltip;
        private YamuiScrollPage dockedPanel;
    }
}
