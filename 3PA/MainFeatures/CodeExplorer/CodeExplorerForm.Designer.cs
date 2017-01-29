using System.ComponentModel;
using YamuiFramework.Controls;
using YamuiFramework.HtmlRenderer.WinForms;

namespace _3PA.MainFeatures.CodeExplorer {
    partial class CodeExplorerForm {
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.toolTipHtml = new YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip();
            this.filterbox = new YamuiFramework.Controls.YamuiList.YamuiFilterBox();
            this.yamuiList = new YamuiFramework.Controls.YamuiList.YamuiFilteredTypeTreeList();
            this.SuspendLayout();
            // 
            // toolTipHtml
            // 
            this.toolTipHtml.AllowLinksHandling = true;
            this.toolTipHtml.AutoPopDelay = 90000;
            this.toolTipHtml.BaseStylesheet = null;
            this.toolTipHtml.InitialDelay = 300;
            this.toolTipHtml.MaximumSize = new System.Drawing.Size(0, 0);
            this.toolTipHtml.OwnerDraw = true;
            this.toolTipHtml.ReshowDelay = 100;
            this.toolTipHtml.ShowAlways = true;
            // 
            // filterbox
            // 
            this.filterbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filterbox.Location = new System.Drawing.Point(4, 4);
            this.filterbox.Name = "filterbox";
            this.filterbox.Size = new System.Drawing.Size(327, 20);
            this.filterbox.TabIndex = 0;
            // 
            // yamuiList
            // 
            this.yamuiList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.yamuiList.EmptyListString = "Empty list!";
            this.yamuiList.Location = new System.Drawing.Point(4, 30);
            this.yamuiList.Name = "yamuiList";
            this.yamuiList.ScrollWidth = 10;
            this.yamuiList.Size = new System.Drawing.Size(327, 283);
            this.yamuiList.SortingClass = null;
            this.yamuiList.TabIndex = 1;
            this.yamuiList.UseCustomBackColor = false;
            // 
            // CodeExplorerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 316);
            this.Controls.Add(this.yamuiList);
            this.Controls.Add(this.filterbox);
            this.Name = "CodeExplorerForm";
            this.Text = "CodeExplorerForm";
            this.ResumeLayout(false);

        }

        #endregion

        private HtmlToolTip toolTipHtml;
        private YamuiFramework.Controls.YamuiList.YamuiFilterBox filterbox;
        private YamuiFramework.Controls.YamuiList.YamuiFilteredTypeTreeList yamuiList;

    }
}