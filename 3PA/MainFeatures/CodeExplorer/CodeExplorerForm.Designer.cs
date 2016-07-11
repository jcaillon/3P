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
            this.buttonSort = new YamuiFramework.Controls.YamuiButtonImage();
            this.buttonRefresh = new YamuiFramework.Controls.YamuiButtonImage();
            this.buttonCleanText = new YamuiFramework.Controls.YamuiButtonImage();
            this.buttonExpandRetract = new YamuiFramework.Controls.YamuiButtonImage();
            this.textBoxFilter = new YamuiFramework.Controls.YamuiTextBox();
            this.toolTipHtml = new YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip();
            this.buttonIncludeExternal = new YamuiFramework.Controls.YamuiButtonImage();
            this.SuspendLayout();
            // 
            // buttonSort
            // 
            this.buttonSort.BackGrndImage = null;
            this.buttonSort.Location = new System.Drawing.Point(45, 4);
            this.buttonSort.Margin = new System.Windows.Forms.Padding(0);
            this.buttonSort.Name = "buttonSort";
            this.buttonSort.Size = new System.Drawing.Size(20, 20);
            this.buttonSort.TabIndex = 19;
            this.buttonSort.TabStop = false;
            this.buttonSort.Text = "yamuiImageButton2";
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.BackGrndImage = null;
            this.buttonRefresh.Location = new System.Drawing.Point(5, 4);
            this.buttonRefresh.Margin = new System.Windows.Forms.Padding(0);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(20, 20);
            this.buttonRefresh.TabIndex = 18;
            this.buttonRefresh.TabStop = false;
            this.buttonRefresh.Text = "yamuiImageButton2";
            // 
            // buttonCleanText
            // 
            this.buttonCleanText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCleanText.BackGrndImage = null;
            this.buttonCleanText.Location = new System.Drawing.Point(296, 4);
            this.buttonCleanText.Margin = new System.Windows.Forms.Padding(0);
            this.buttonCleanText.Name = "buttonCleanText";
            this.buttonCleanText.Size = new System.Drawing.Size(20, 20);
            this.buttonCleanText.TabIndex = 17;
            this.buttonCleanText.TabStop = false;
            this.buttonCleanText.Text = "yamuiImageButton2";
            // 
            // buttonExpandRetract
            // 
            this.buttonExpandRetract.BackGrndImage = null;
            this.buttonExpandRetract.Location = new System.Drawing.Point(25, 4);
            this.buttonExpandRetract.Name = "buttonExpandRetract";
            this.buttonExpandRetract.Size = new System.Drawing.Size(20, 20);
            this.buttonExpandRetract.TabIndex = 16;
            this.buttonExpandRetract.TabStop = false;
            this.buttonExpandRetract.Text = "yamuiImageButton1";
            // 
            // textBoxFilter
            // 
            this.textBoxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFilter.Lines = new string[0];
            this.textBoxFilter.Location = new System.Drawing.Point(90, 4);
            this.textBoxFilter.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.textBoxFilter.MaxLength = 32767;
            this.textBoxFilter.Name = "textBoxFilter";
            this.textBoxFilter.PasswordChar = '\0';
            this.textBoxFilter.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textBoxFilter.SelectedText = "";
            this.textBoxFilter.Size = new System.Drawing.Size(201, 20);
            this.textBoxFilter.TabIndex = 14;
            this.textBoxFilter.TabStop = false;
            this.textBoxFilter.WaterMark = "Filter here!";
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
            // buttonIncludeExternal
            // 
            this.buttonIncludeExternal.BackGrndImage = null;
            this.buttonIncludeExternal.Location = new System.Drawing.Point(65, 4);
            this.buttonIncludeExternal.Margin = new System.Windows.Forms.Padding(0);
            this.buttonIncludeExternal.Name = "buttonIncludeExternal";
            this.buttonIncludeExternal.Size = new System.Drawing.Size(20, 20);
            this.buttonIncludeExternal.TabIndex = 20;
            this.buttonIncludeExternal.TabStop = false;
            this.buttonIncludeExternal.Text = "yamuiImageButton2";
            // 
            // CodeExplorerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 330);
            this.Controls.Add(this.buttonSort);
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.buttonCleanText);
            this.Controls.Add(this.buttonExpandRetract);
            this.Controls.Add(this.textBoxFilter);
            this.Controls.Add(this.buttonIncludeExternal);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "CodeExplorerForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "DockableExplorerForm";
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiButtonImage buttonSort;
        private YamuiButtonImage buttonRefresh;
        private YamuiButtonImage buttonCleanText;
        private YamuiButtonImage buttonExpandRetract;
        private YamuiTextBox textBoxFilter;
        private HtmlToolTip toolTipHtml;
        private YamuiButtonImage buttonIncludeExternal;

    }
}