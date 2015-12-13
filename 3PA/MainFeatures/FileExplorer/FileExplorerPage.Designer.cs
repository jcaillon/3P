using System.ComponentModel;
using YamuiFramework.Controls;

namespace _3PA.MainFeatures.FileExplorer {
    partial class FileExplorerPage {
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
            this.yamuiLabel2 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiPanel1 = new YamuiFramework.Controls.YamuiPanel();
            this.yamuiLabel1 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiPanel2 = new YamuiFramework.Controls.YamuiPanel();
            this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.yamuiPanel3 = new YamuiFramework.Controls.YamuiPanel();
            this.btErase = new YamuiFramework.Controls.YamuiImageButton();
            this.textFilter = new YamuiFramework.Controls.YamuiTextBox();
            this.btRefresh = new YamuiFramework.Controls.YamuiImageButton();
            this.nbitems = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLabel4 = new YamuiFramework.Controls.YamuiLabel();
            this.ovl = new BrightIdeasSoftware.FastObjectListView();
            this.FileName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.yamuiLabel3 = new YamuiFramework.Controls.YamuiLabel();
            this.toolTipHtml = new YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip();
            this.cbDirectory = new YamuiFramework.Controls.YamuiCheckBox();
            this.yamuiPanel1.SuspendLayout();
            this.yamuiPanel2.SuspendLayout();
            this.yamuiPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ovl)).BeginInit();
            this.SuspendLayout();
            // 
            // yamuiLabel2
            // 
            this.yamuiLabel2.AutoSize = true;
            this.yamuiLabel2.Function = YamuiFramework.Fonts.LabelFunction.Heading;
            this.yamuiLabel2.Location = new System.Drawing.Point(5, 3);
            this.yamuiLabel2.Margin = new System.Windows.Forms.Padding(5, 3, 5, 7);
            this.yamuiLabel2.Name = "yamuiLabel2";
            this.yamuiLabel2.Size = new System.Drawing.Size(70, 19);
            this.yamuiLabel2.TabIndex = 1;
            this.yamuiLabel2.Text = "ACTIONS";
            // 
            // yamuiPanel1
            // 
            this.yamuiPanel1.Controls.Add(this.yamuiLabel1);
            this.yamuiPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.yamuiPanel1.HorizontalScrollbarHighlightOnWheel = false;
            this.yamuiPanel1.HorizontalScrollbarSize = 10;
            this.yamuiPanel1.Location = new System.Drawing.Point(0, 0);
            this.yamuiPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.yamuiPanel1.Name = "yamuiPanel1";
            this.yamuiPanel1.Size = new System.Drawing.Size(412, 82);
            this.yamuiPanel1.TabIndex = 2;
            this.yamuiPanel1.VerticalScrollbarHighlightOnWheel = false;
            this.yamuiPanel1.VerticalScrollbarSize = 10;
            // 
            // yamuiLabel1
            // 
            this.yamuiLabel1.AutoSize = true;
            this.yamuiLabel1.Function = YamuiFramework.Fonts.LabelFunction.Heading;
            this.yamuiLabel1.Location = new System.Drawing.Point(5, 3);
            this.yamuiLabel1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 7);
            this.yamuiLabel1.Name = "yamuiLabel1";
            this.yamuiLabel1.Size = new System.Drawing.Size(101, 19);
            this.yamuiLabel1.TabIndex = 2;
            this.yamuiLabel1.Text = "CURRENT FILE";
            // 
            // yamuiPanel2
            // 
            this.yamuiPanel2.AutoSize = true;
            this.yamuiPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.yamuiPanel2.Controls.Add(this.flowLayoutPanel);
            this.yamuiPanel2.Controls.Add(this.yamuiLabel2);
            this.yamuiPanel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.yamuiPanel2.HorizontalScrollbarHighlightOnWheel = false;
            this.yamuiPanel2.HorizontalScrollbarSize = 10;
            this.yamuiPanel2.Location = new System.Drawing.Point(0, 82);
            this.yamuiPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.yamuiPanel2.Name = "yamuiPanel2";
            this.yamuiPanel2.Size = new System.Drawing.Size(412, 41);
            this.yamuiPanel2.TabIndex = 3;
            this.yamuiPanel2.VerticalScrollbarHighlightOnWheel = false;
            this.yamuiPanel2.VerticalScrollbarSize = 10;
            // 
            // flowLayoutPanel
            // 
            this.flowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel.AutoSize = true;
            this.flowLayoutPanel.BackColor = System.Drawing.Color.Transparent;
            this.flowLayoutPanel.Location = new System.Drawing.Point(3, 27);
            this.flowLayoutPanel.Name = "flowLayoutPanel";
            this.flowLayoutPanel.Size = new System.Drawing.Size(406, 11);
            this.flowLayoutPanel.TabIndex = 2;
            // 
            // yamuiPanel3
            // 
            this.yamuiPanel3.Controls.Add(this.cbDirectory);
            this.yamuiPanel3.Controls.Add(this.btErase);
            this.yamuiPanel3.Controls.Add(this.textFilter);
            this.yamuiPanel3.Controls.Add(this.btRefresh);
            this.yamuiPanel3.Controls.Add(this.nbitems);
            this.yamuiPanel3.Controls.Add(this.yamuiLabel4);
            this.yamuiPanel3.Controls.Add(this.ovl);
            this.yamuiPanel3.Controls.Add(this.yamuiLabel3);
            this.yamuiPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.yamuiPanel3.HorizontalScrollbarHighlightOnWheel = false;
            this.yamuiPanel3.HorizontalScrollbarSize = 10;
            this.yamuiPanel3.Location = new System.Drawing.Point(0, 123);
            this.yamuiPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.yamuiPanel3.Name = "yamuiPanel3";
            this.yamuiPanel3.Size = new System.Drawing.Size(412, 361);
            this.yamuiPanel3.TabIndex = 4;
            this.yamuiPanel3.VerticalScrollbarHighlightOnWheel = false;
            this.yamuiPanel3.VerticalScrollbarSize = 10;
            // 
            // btErase
            // 
            this.btErase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btErase.BackGrndImage = null;
            this.btErase.Location = new System.Drawing.Point(387, 53);
            this.btErase.Name = "btErase";
            this.btErase.Size = new System.Drawing.Size(20, 20);
            this.btErase.TabIndex = 7;
            this.btErase.Text = "yamuiImageButton2";
            // 
            // textFilter
            // 
            this.textFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textFilter.Lines = new string[0];
            this.textFilter.Location = new System.Drawing.Point(31, 53);
            this.textFilter.MaxLength = 32767;
            this.textFilter.Name = "textFilter";
            this.textFilter.PasswordChar = '\0';
            this.textFilter.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textFilter.SelectedText = "";
            this.textFilter.Size = new System.Drawing.Size(350, 20);
            this.textFilter.TabIndex = 6;
            this.textFilter.WaterMark = "Filter here!";
            // 
            // btRefresh
            // 
            this.btRefresh.BackGrndImage = null;
            this.btRefresh.Location = new System.Drawing.Point(5, 53);
            this.btRefresh.Name = "btRefresh";
            this.btRefresh.Size = new System.Drawing.Size(20, 20);
            this.btRefresh.TabIndex = 5;
            this.btRefresh.Text = "yamuiImageButton1";
            // 
            // nbitems
            // 
            this.nbitems.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nbitems.AutoSize = true;
            this.nbitems.Function = YamuiFramework.Fonts.LabelFunction.Small;
            this.nbitems.Location = new System.Drawing.Point(2, 347);
            this.nbitems.Margin = new System.Windows.Forms.Padding(3);
            this.nbitems.Name = "nbitems";
            this.nbitems.Size = new System.Drawing.Size(43, 12);
            this.nbitems.TabIndex = 4;
            this.nbitems.Text = "Showing";
            this.nbitems.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // yamuiLabel4
            // 
            this.yamuiLabel4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.yamuiLabel4.AutoSize = true;
            this.yamuiLabel4.Function = YamuiFramework.Fonts.LabelFunction.Small;
            this.yamuiLabel4.Location = new System.Drawing.Point(2, 334);
            this.yamuiLabel4.Margin = new System.Windows.Forms.Padding(0);
            this.yamuiLabel4.Name = "yamuiLabel4";
            this.yamuiLabel4.Size = new System.Drawing.Size(43, 12);
            this.yamuiLabel4.TabIndex = 3;
            this.yamuiLabel4.Text = "Showing";
            this.yamuiLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ovl
            // 
            this.ovl.AllColumns.Add(this.FileName);
            this.ovl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ovl.AutoArrange = false;
            this.ovl.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ovl.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.FileName});
            this.ovl.FullRowSelect = true;
            this.ovl.HeaderMaximumHeight = 0;
            this.ovl.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.ovl.LabelWrap = false;
            this.ovl.Location = new System.Drawing.Point(0, 79);
            this.ovl.MultiSelect = false;
            this.ovl.Name = "ovl";
            this.ovl.OwnerDraw = true;
            this.ovl.RowHeight = 20;
            this.ovl.SelectAllOnControlA = false;
            this.ovl.ShowGroups = false;
            this.ovl.ShowHeaderInAllViews = false;
            this.ovl.ShowSortIndicators = false;
            this.ovl.Size = new System.Drawing.Size(412, 252);
            this.ovl.SortGroupItemsByPrimaryColumn = false;
            this.ovl.TabIndex = 2;
            this.ovl.UseCellFormatEvents = true;
            this.ovl.UseCompatibleStateImageBehavior = false;
            this.ovl.UseFiltering = true;
            this.ovl.UseHotItem = true;
            this.ovl.UseTabAsInput = false;
            this.ovl.View = System.Windows.Forms.View.Details;
            this.ovl.VirtualMode = true;
            // 
            // FileName
            // 
            this.FileName.FillsFreeSpace = true;
            this.FileName.IsEditable = false;
            this.FileName.ShowTextInHeader = false;
            this.FileName.Text = "";
            // 
            // yamuiLabel3
            // 
            this.yamuiLabel3.AutoSize = true;
            this.yamuiLabel3.Function = YamuiFramework.Fonts.LabelFunction.Heading;
            this.yamuiLabel3.Location = new System.Drawing.Point(5, 3);
            this.yamuiLabel3.Margin = new System.Windows.Forms.Padding(5, 3, 5, 7);
            this.yamuiLabel3.Name = "yamuiLabel3";
            this.yamuiLabel3.Size = new System.Drawing.Size(114, 19);
            this.yamuiLabel3.TabIndex = 1;
            this.yamuiLabel3.Text = "FILES EXPLORER";
            // 
            // toolTipHtml
            // 
            this.toolTipHtml.AllowLinksHandling = true;
            this.toolTipHtml.BaseStylesheet = null;
            this.toolTipHtml.MaximumSize = new System.Drawing.Size(0, 0);
            this.toolTipHtml.OwnerDraw = true;
            this.toolTipHtml.TooltipCssClass = "htmltooltip";
            // 
            // cbDirectory
            // 
            this.cbDirectory.AutoSize = true;
            this.cbDirectory.Location = new System.Drawing.Point(5, 32);
            this.cbDirectory.Name = "cbDirectory";
            this.cbDirectory.Size = new System.Drawing.Size(28, 15);
            this.cbDirectory.TabIndex = 8;
            this.cbDirectory.Text = "?";
            // 
            // FileExplorerPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.yamuiPanel3);
            this.Controls.Add(this.yamuiPanel2);
            this.Controls.Add(this.yamuiPanel1);
            this.Name = "FileExplorerPage";
            this.Size = new System.Drawing.Size(412, 484);
            this.yamuiPanel1.ResumeLayout(false);
            this.yamuiPanel1.PerformLayout();
            this.yamuiPanel2.ResumeLayout(false);
            this.yamuiPanel2.PerformLayout();
            this.yamuiPanel3.ResumeLayout(false);
            this.yamuiPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ovl)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private YamuiLabel yamuiLabel2;
        private YamuiPanel yamuiPanel1;
        private YamuiLabel yamuiLabel1;
        private YamuiPanel yamuiPanel2;
        private YamuiPanel yamuiPanel3;
        private YamuiLabel yamuiLabel3;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
        private BrightIdeasSoftware.FastObjectListView ovl;
        private BrightIdeasSoftware.OLVColumn FileName;
        private YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip toolTipHtml;
        private YamuiLabel nbitems;
        private YamuiLabel yamuiLabel4;
        private YamuiImageButton btRefresh;
        private YamuiImageButton btErase;
        private YamuiTextBox textFilter;
        private YamuiCheckBox cbDirectory;

    }
}
