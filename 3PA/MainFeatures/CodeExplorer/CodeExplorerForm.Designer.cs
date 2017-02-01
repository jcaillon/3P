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
            this.yamuiLabel2 = new YamuiFramework.Controls.YamuiLabel();
            this.lbCurrentScope = new YamuiFramework.Controls.YamuiLabel();
            this.pbCurrentScope = new YamuiFramework.Controls.YamuiPictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbCurrentScope)).BeginInit();
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
            this.filterbox.Location = new System.Drawing.Point(4, 23);
            this.filterbox.Name = "filterbox";
            this.filterbox.Size = new System.Drawing.Size(343, 20);
            this.filterbox.TabIndex = 0;
            // 
            // yamuiList
            // 
            this.yamuiList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.yamuiList.EmptyListString = "Empty list!";
            this.yamuiList.Location = new System.Drawing.Point(4, 49);
            this.yamuiList.Name = "yamuiList";
            this.yamuiList.ScrollWidth = 10;
            this.yamuiList.Size = new System.Drawing.Size(343, 519);
            this.yamuiList.SortingClass = null;
            this.yamuiList.TabIndex = 1;
            this.yamuiList.UseCustomBackColor = false;
            // 
            // yamuiLabel2
            // 
            this.yamuiLabel2.AutoSize = true;
            this.yamuiLabel2.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel2.Location = new System.Drawing.Point(2, 1);
            this.yamuiLabel2.Margin = new System.Windows.Forms.Padding(5, 3, 5, 7);
            this.yamuiLabel2.Name = "yamuiLabel2";
            this.yamuiLabel2.Size = new System.Drawing.Size(118, 19);
            this.yamuiLabel2.TabIndex = 38;
            this.yamuiLabel2.Text = "CODE EXPLORER";
            // 
            // lbCurrentScope
            // 
            this.lbCurrentScope.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbCurrentScope.Function = YamuiFramework.Fonts.FontFunction.Small;
            this.lbCurrentScope.Location = new System.Drawing.Point(128, 1);
            this.lbCurrentScope.Margin = new System.Windows.Forms.Padding(3);
            this.lbCurrentScope.Name = "lbCurrentScope";
            this.lbCurrentScope.Size = new System.Drawing.Size(193, 20);
            this.lbCurrentScope.TabIndex = 39;
            this.lbCurrentScope.Text = "Not applicable";
            this.lbCurrentScope.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pbCurrentScope
            // 
            this.pbCurrentScope.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pbCurrentScope.BackGrndImage = null;
            this.pbCurrentScope.Location = new System.Drawing.Point(327, 1);
            this.pbCurrentScope.Name = "pbCurrentScope";
            this.pbCurrentScope.Size = new System.Drawing.Size(20, 20);
            this.pbCurrentScope.TabIndex = 40;
            this.pbCurrentScope.TabStop = false;
            // 
            // CodeExplorerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 571);
            this.Controls.Add(this.pbCurrentScope);
            this.Controls.Add(this.lbCurrentScope);
            this.Controls.Add(this.yamuiLabel2);
            this.Controls.Add(this.yamuiList);
            this.Controls.Add(this.filterbox);
            this.Name = "CodeExplorerForm";
            this.Text = "CodeExplorerForm";
            ((System.ComponentModel.ISupportInitialize)(this.pbCurrentScope)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private HtmlToolTip toolTipHtml;
        private YamuiFramework.Controls.YamuiList.YamuiFilterBox filterbox;
        private YamuiFramework.Controls.YamuiList.YamuiFilteredTypeTreeList yamuiList;
        private YamuiLabel yamuiLabel2;
        private YamuiLabel lbCurrentScope;
        private YamuiPictureBox pbCurrentScope;
    }
}