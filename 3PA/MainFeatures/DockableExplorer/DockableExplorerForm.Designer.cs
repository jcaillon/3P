namespace _3PA.MainFeatures.DockableExplorer {
    partial class DockableExplorerForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.textBoxFilter = new YamuiFramework.Controls.YamuiTextBox();
            this.ovlTree = new BrightIdeasSoftware.TreeListView();
            this.DisplayText = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.buttonExpandRetract = new YamuiFramework.Controls.YamuiImageButton();
            this.buttonCleanText = new YamuiFramework.Controls.YamuiImageButton();
            this.panelBottom = new YamuiFramework.Controls.YamuiPanel();
            this.buttonRefresh = new YamuiFramework.Controls.YamuiImageButton();
            this.buttonSort = new YamuiFramework.Controls.YamuiImageButton();
            this.toolTipHtml = new YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip();
            ((System.ComponentModel.ISupportInitialize)(this.ovlTree)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxFilter
            // 
            this.textBoxFilter.Lines = new string[0];
            this.textBoxFilter.Location = new System.Drawing.Point(110, 4);
            this.textBoxFilter.MaxLength = 32767;
            this.textBoxFilter.Name = "textBoxFilter";
            this.textBoxFilter.PasswordChar = '\0';
            this.textBoxFilter.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textBoxFilter.SelectedText = "";
            this.textBoxFilter.Size = new System.Drawing.Size(177, 25);
            this.textBoxFilter.TabIndex = 0;
            this.textBoxFilter.TabStop = false;
            this.textBoxFilter.WaterMark = "Filter here!";
            this.textBoxFilter.TextChanged += new System.EventHandler(this.textBoxFilter_TextChanged);
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
            this.ovlTree.FullRowSelect = true;
            this.ovlTree.HeaderMaximumHeight = 0;
            this.ovlTree.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.ovlTree.Location = new System.Drawing.Point(4, 35);
            this.ovlTree.MultiSelect = false;
            this.ovlTree.Name = "ovlTree";
            this.ovlTree.OwnerDraw = true;
            this.ovlTree.RowHeight = 20;
            this.ovlTree.ShowGroups = false;
            this.ovlTree.Size = new System.Drawing.Size(283, 494);
            this.ovlTree.TabIndex = 1;
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
            // buttonExpandRetract
            // 
            this.buttonExpandRetract.BackGrndImage = null;
            this.buttonExpandRetract.Location = new System.Drawing.Point(4, 4);
            this.buttonExpandRetract.Name = "buttonExpandRetract";
            this.buttonExpandRetract.Size = new System.Drawing.Size(25, 25);
            this.buttonExpandRetract.TabIndex = 2;
            this.buttonExpandRetract.TabStop = false;
            this.buttonExpandRetract.Text = "yamuiImageButton1";
            this.toolTipHtml.SetToolTip(this.buttonExpandRetract, "Toggle <b>Expand/Collapse</b>");
            this.buttonExpandRetract.Click += new System.EventHandler(this.buttonExpandRetract_Click);
            // 
            // buttonCleanText
            // 
            this.buttonCleanText.BackGrndImage = null;
            this.buttonCleanText.Location = new System.Drawing.Point(79, 4);
            this.buttonCleanText.Name = "buttonCleanText";
            this.buttonCleanText.Size = new System.Drawing.Size(25, 25);
            this.buttonCleanText.TabIndex = 3;
            this.buttonCleanText.TabStop = false;
            this.buttonCleanText.Text = "yamuiImageButton2";
            this.toolTipHtml.SetToolTip(this.buttonCleanText, "<b>Clean</b> the current text filter");
            this.buttonCleanText.Click += new System.EventHandler(this.buttonCleanText_Click);
            // 
            // panelBottom
            // 
            this.panelBottom.AutoScroll = true;
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.HorizontalScrollbar = true;
            this.panelBottom.HorizontalScrollbarHighlightOnWheel = false;
            this.panelBottom.HorizontalScrollbarSize = 10;
            this.panelBottom.Location = new System.Drawing.Point(1, 535);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(289, 47);
            this.panelBottom.TabIndex = 4;
            this.panelBottom.VerticalScrollbar = true;
            this.panelBottom.VerticalScrollbarHighlightOnWheel = false;
            this.panelBottom.VerticalScrollbarSize = 10;
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.BackGrndImage = null;
            this.buttonRefresh.Location = new System.Drawing.Point(29, 4);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(25, 25);
            this.buttonRefresh.TabIndex = 5;
            this.buttonRefresh.TabStop = false;
            this.buttonRefresh.Text = "yamuiImageButton2";
            this.toolTipHtml.SetToolTip(this.buttonRefresh, "Click to <b>Refresh</b> the tree");
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // buttonSort
            // 
            this.buttonSort.BackGrndImage = null;
            this.buttonSort.Location = new System.Drawing.Point(54, 4);
            this.buttonSort.Name = "buttonSort";
            this.buttonSort.Size = new System.Drawing.Size(25, 25);
            this.buttonSort.TabIndex = 6;
            this.buttonSort.TabStop = false;
            this.buttonSort.Text = "yamuiImageButton2";
            this.toolTipHtml.SetToolTip(this.buttonSort, "Toggle <b>Categories/Code order sorting</b>");
            this.buttonSort.Click += new System.EventHandler(this.buttonSort_Click);
            // 
            // toolTipHtml
            // 
            this.toolTipHtml.AllowLinksHandling = true;
            this.toolTipHtml.BaseStylesheet = null;
            this.toolTipHtml.MaximumSize = new System.Drawing.Size(0, 0);
            this.toolTipHtml.OwnerDraw = true;
            this.toolTipHtml.TooltipCssClass = "htmltooltip";
            // 
            // DockableExplorerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(291, 583);
            this.Controls.Add(this.buttonSort);
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.buttonCleanText);
            this.Controls.Add(this.buttonExpandRetract);
            this.Controls.Add(this.ovlTree);
            this.Controls.Add(this.textBoxFilter);
            this.Name = "DockableExplorerForm";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.Text = "DockableExplorerForm";
            ((System.ComponentModel.ISupportInitialize)(this.ovlTree)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiFramework.Controls.YamuiTextBox textBoxFilter;
        private BrightIdeasSoftware.TreeListView ovlTree;
        private BrightIdeasSoftware.OLVColumn DisplayText;
        private YamuiFramework.Controls.YamuiImageButton buttonExpandRetract;
        private YamuiFramework.Controls.YamuiImageButton buttonCleanText;
        private YamuiFramework.Controls.YamuiPanel panelBottom;
        private YamuiFramework.Controls.YamuiImageButton buttonRefresh;
        private YamuiFramework.Controls.YamuiImageButton buttonSort;
        private YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip toolTipHtml;
    }
}