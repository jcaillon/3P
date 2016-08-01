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
            this.scrollPanel = new YamuiFramework.Controls.YamuiScrollPanel();
            this.static_name = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.static_keys = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.static_title = new YamuiFramework.Controls.YamuiLabel();
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
            this.scrollPanel.ContentPanel.Controls.Add(this.static_name);
            this.scrollPanel.ContentPanel.Controls.Add(this.static_keys);
            this.scrollPanel.ContentPanel.Controls.Add(this.static_title);
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
            // static_name
            // 
            this.static_name.BackColor = System.Drawing.Color.Transparent;
            this.static_name.BaseStylesheet = null;
            this.static_name.Location = new System.Drawing.Point(60, 29);
            this.static_name.Name = "static_name";
            this.static_name.Size = new System.Drawing.Size(33, 15);
            this.static_name.TabIndex = 118;
            this.static_name.TabStop = false;
            this.static_name.Text = "<b>Name</b>";
            // 
            // static_keys
            // 
            this.static_keys.BackColor = System.Drawing.Color.Transparent;
            this.static_keys.BaseStylesheet = null;
            this.static_keys.Location = new System.Drawing.Point(420, 29);
            this.static_keys.Name = "static_keys";
            this.static_keys.Size = new System.Drawing.Size(103, 15);
            this.static_keys.TabIndex = 114;
            this.static_keys.TabStop = false;
            this.static_keys.Text = "<b>Keyboard shortcut</b>";
            // 
            // static_title
            // 
            this.static_title.AutoSize = true;
            this.static_title.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.static_title.Location = new System.Drawing.Point(0, 0);
            this.static_title.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.static_title.Name = "static_title";
            this.static_title.Size = new System.Drawing.Size(203, 19);
            this.static_title.TabIndex = 0;
            this.static_title.Text = "SHORTCUT CUSTOMIZATION";
            // 
            // ShortCutsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.scrollPanel);
            this.Name = "ShortCutsPage";
            this.Size = new System.Drawing.Size(900, 650);
            this.scrollPanel.ContentPanel.ResumeLayout(false);
            this.scrollPanel.ContentPanel.PerformLayout();
            this.scrollPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private HtmlToolTip tooltip;
        private YamuiScrollPanel scrollPanel;
        private YamuiLabel static_title;
        private HtmlLabel static_keys;
        private HtmlLabel static_name;
    }
}
