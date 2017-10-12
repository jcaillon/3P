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
            this.components = new System.ComponentModel.Container();
            this.tooltip = new YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip();
            this.scrollPanel = new YamuiFramework.Controls.YamuiScrollPanel();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.scrollPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
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
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // OptionPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.scrollPanel);
            this.Name = "OptionPage";
            this.Size = new System.Drawing.Size(900, 650);
            this.scrollPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private HtmlToolTip tooltip;
        private YamuiScrollPanel scrollPanel;
        private System.Windows.Forms.ErrorProvider errorProvider;
    }
}
