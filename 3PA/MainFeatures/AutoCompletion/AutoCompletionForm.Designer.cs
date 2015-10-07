using System;

namespace _3PA.MainFeatures.AutoCompletion {
    partial class AutoCompletionForm {
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
            try {
                fastOLV.FormatCell -= FastOlvOnFormatCell;
                fastOLV.BeforeSorting -= FastOlvOnBeforeSorting;
                fastOLV.KeyDown -= FastOlvOnKeyDown;
                MouseLeave -= CustomOnMouseLeave;
                fastOLV.MouseLeave -= CustomOnMouseLeave;
                fastOLV.DoubleClick -= FastOlvOnDoubleClick;

            } catch (Exception) {
                // ignored
            }
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.fastOLV = new FastObjectListViewWoScrolls();
            this.Keyword = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.Type = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.nbitems = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLabel1 = new YamuiFramework.Controls.YamuiLabel();
            this.htmlToolTip = new YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip();
            ((System.ComponentModel.ISupportInitialize)(this.fastOLV)).BeginInit();
            this.SuspendLayout();
            // 
            // fastOLV
            // 
            this.fastOLV.Activation = System.Windows.Forms.ItemActivation.TwoClick;
            this.fastOLV.AllColumns.Add(this.Keyword);
            this.fastOLV.AllColumns.Add(this.Type);
            this.fastOLV.AutoArrange = false;
            this.fastOLV.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fastOLV.CausesValidation = false;
            this.fastOLV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Keyword});
            this.fastOLV.CopySelectionOnControlC = false;
            this.fastOLV.CopySelectionOnControlCUsesDragSource = false;
            this.fastOLV.Cursor = System.Windows.Forms.Cursors.Default;
            this.fastOLV.Dock = System.Windows.Forms.DockStyle.Top;
            this.fastOLV.FullRowSelect = true;
            this.fastOLV.HasCollapsibleGroups = false;
            this.fastOLV.HeaderMaximumHeight = 0;
            this.fastOLV.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.fastOLV.HideSelection = false;
            this.fastOLV.Location = new System.Drawing.Point(1, 1);
            this.fastOLV.MultiSelect = false;
            this.fastOLV.Name = "fastOLV";
            this.fastOLV.OwnerDraw = true;
            this.fastOLV.PersistentCheckBoxes = false;
            this.fastOLV.RowHeight = 20;
            this.fastOLV.SelectAllOnControlA = false;
            this.fastOLV.SelectColumnsOnRightClick = false;
            this.fastOLV.SelectColumnsOnRightClickBehaviour = BrightIdeasSoftware.ObjectListView.ColumnSelectBehaviour.None;
            this.fastOLV.ShowFilterMenuOnRightClick = false;
            this.fastOLV.ShowGroups = false;
            this.fastOLV.ShowHeaderInAllViews = false;
            this.fastOLV.ShowSortIndicators = false;
            this.fastOLV.Size = new System.Drawing.Size(392, 400);
            this.fastOLV.TabIndex = 0;
            this.fastOLV.TriggerCellOverEventsWhenOverHeader = false;
            this.fastOLV.UpdateSpaceFillingColumnsWhenDraggingColumnDivider = false;
            this.fastOLV.UseCompatibleStateImageBehavior = false;
            this.fastOLV.UseFiltering = true;
            this.fastOLV.UseTabAsInput = false;
            this.fastOLV.View = System.Windows.Forms.View.Details;
            this.fastOLV.VirtualMode = true;
            // 
            // Keyword
            // 
            this.Keyword.AspectName = "DisplayText";
            this.Keyword.AutoCompleteEditor = false;
            this.Keyword.AutoCompleteEditorMode = System.Windows.Forms.AutoCompleteMode.None;
            this.Keyword.CellVerticalAlignment = System.Drawing.StringAlignment.Center;
            this.Keyword.FillsFreeSpace = true;
            this.Keyword.Groupable = false;
            this.Keyword.HeaderCheckBoxUpdatesRowCheckBoxes = false;
            this.Keyword.Hideable = false;
            this.Keyword.IsEditable = false;
            this.Keyword.ShowTextInHeader = false;
            this.Keyword.Text = "Keywords";
            this.Keyword.Width = 25;
            // 
            // Type
            // 
            this.Type.AspectName = "Type";
            this.Type.IsVisible = false;
            // 
            // nbitems
            // 
            this.nbitems.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.nbitems.Enabled = false;
            this.nbitems.Function = YamuiFramework.Fonts.LabelFunction.Small;
            this.nbitems.Location = new System.Drawing.Point(330, 432);
            this.nbitems.Margin = new System.Windows.Forms.Padding(3);
            this.nbitems.Name = "nbitems";
            this.nbitems.Size = new System.Drawing.Size(60, 12);
            this.nbitems.TabIndex = 3;
            this.nbitems.Text = "yamuiLabel1";
            this.nbitems.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // yamuiLabel1
            // 
            this.yamuiLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.yamuiLabel1.Enabled = false;
            this.yamuiLabel1.Function = YamuiFramework.Fonts.LabelFunction.Small;
            this.yamuiLabel1.Location = new System.Drawing.Point(330, 419);
            this.yamuiLabel1.Margin = new System.Windows.Forms.Padding(3);
            this.yamuiLabel1.Name = "yamuiLabel1";
            this.yamuiLabel1.Size = new System.Drawing.Size(60, 12);
            this.yamuiLabel1.TabIndex = 4;
            this.yamuiLabel1.Text = "Showing";
            this.yamuiLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // htmlToolTip
            // 
            this.htmlToolTip.AllowLinksHandling = true;
            this.htmlToolTip.BaseStylesheet = null;
            this.htmlToolTip.MaximumSize = new System.Drawing.Size(0, 0);
            this.htmlToolTip.OwnerDraw = true;
            this.htmlToolTip.TooltipCssClass = "htmltooltip";
            // 
            // AutoCompletionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(394, 449);
            this.Controls.Add(this.yamuiLabel1);
            this.Controls.Add(this.nbitems);
            this.Controls.Add(this.fastOLV);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "AutoCompletionForm";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "AutoCompletionForm";
            ((System.ComponentModel.ISupportInitialize)(this.fastOLV)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private FastObjectListViewWoScrolls fastOLV;
        private BrightIdeasSoftware.OLVColumn Keyword;
        private BrightIdeasSoftware.OLVColumn Type;
        private YamuiFramework.Controls.YamuiLabel nbitems;
        private YamuiFramework.Controls.YamuiLabel yamuiLabel1;
        private YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip htmlToolTip;
    }
}