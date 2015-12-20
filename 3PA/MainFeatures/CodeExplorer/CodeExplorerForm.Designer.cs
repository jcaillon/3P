using System.ComponentModel;

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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CodeExplorerForm));
            this.buttonSort = new YamuiFramework.Controls.YamuiImageButton();
            this.buttonRefresh = new YamuiFramework.Controls.YamuiImageButton();
            this.buttonCleanText = new YamuiFramework.Controls.YamuiImageButton();
            this.buttonExpandRetract = new YamuiFramework.Controls.YamuiImageButton();
            this.ovlTree = new BrightIdeasSoftware.TreeListView();
            this.DisplayText = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.textBoxFilter = new YamuiFramework.Controls.YamuiTextBox();
            this.toolTipHtml = new YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip();
            this.buttonIncludeExternal = new YamuiFramework.Controls.YamuiImageButton();
            ((System.ComponentModel.ISupportInitialize)(this.ovlTree)).BeginInit();
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
            this.buttonCleanText.Location = new System.Drawing.Point(293, 4);
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
            // ovlTree
            // 
            this.ovlTree.AllColumns.Add(this.DisplayText);
            this.ovlTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ovlTree.AutoArrange = false;
            this.ovlTree.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ovlTree.CausesValidation = false;
            this.ovlTree.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.DisplayText});
            this.ovlTree.ExpandedObjectsList = ((System.Collections.Generic.List<object>)(resources.GetObject("ovlTree.ExpandedObjectsList")));
            this.ovlTree.FullRowSelect = true;
            this.ovlTree.HeaderMaximumHeight = 0;
            this.ovlTree.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.ovlTree.Location = new System.Drawing.Point(1, 27);
            this.ovlTree.MultiSelect = false;
            this.ovlTree.Name = "ovlTree";
            this.ovlTree.OwnerDraw = true;
            this.ovlTree.RowHeight = 20;
            this.ovlTree.ShowGroups = false;
            this.ovlTree.Size = new System.Drawing.Size(315, 262);
            this.ovlTree.TabIndex = 15;
            this.ovlTree.TabStop = false;
            this.ovlTree.UseCompatibleStateImageBehavior = false;
            this.ovlTree.UseFiltering = true;
            this.ovlTree.UseHotItem = true;
            this.ovlTree.View = System.Windows.Forms.View.Details;
            this.ovlTree.VirtualMode = true;
            // 
            // DisplayText
            // 
            this.DisplayText.AspectName = "DisplayText";
            this.DisplayText.CellPadding = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.DisplayText.FillsFreeSpace = true;
            this.DisplayText.Groupable = false;
            this.DisplayText.IsEditable = false;
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
            this.textBoxFilter.Size = new System.Drawing.Size(198, 20);
            this.textBoxFilter.TabIndex = 14;
            this.textBoxFilter.TabStop = false;
            this.textBoxFilter.WaterMark = "Filter here!";
            // 
            // toolTipHtml
            // 
            this.toolTipHtml.AllowLinksHandling = true;
            this.toolTipHtml.BaseStylesheet = null;
            this.toolTipHtml.MaximumSize = new System.Drawing.Size(0, 0);
            this.toolTipHtml.OwnerDraw = true;
            this.toolTipHtml.TooltipCssClass = "htmltooltip";
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
            this.ClientSize = new System.Drawing.Size(317, 291);
            this.Controls.Add(this.buttonSort);
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.buttonCleanText);
            this.Controls.Add(this.buttonExpandRetract);
            this.Controls.Add(this.ovlTree);
            this.Controls.Add(this.textBoxFilter);
            this.Controls.Add(this.buttonIncludeExternal);
            this.Name = "CodeExplorerForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "DockableExplorerForm";
            ((System.ComponentModel.ISupportInitialize)(this.ovlTree)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiFramework.Controls.YamuiImageButton buttonSort;
        private YamuiFramework.Controls.YamuiImageButton buttonRefresh;
        private YamuiFramework.Controls.YamuiImageButton buttonCleanText;
        private YamuiFramework.Controls.YamuiImageButton buttonExpandRetract;
        private BrightIdeasSoftware.TreeListView ovlTree;
        private BrightIdeasSoftware.OLVColumn DisplayText;
        private YamuiFramework.Controls.YamuiTextBox textBoxFilter;
        private YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip toolTipHtml;
        private YamuiFramework.Controls.YamuiImageButton buttonIncludeExternal;

    }
}