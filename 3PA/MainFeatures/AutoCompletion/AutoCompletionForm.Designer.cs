using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BrightIdeasSoftware;
using YamuiFramework.Controls;
using YamuiFramework.Fonts;
using YamuiFramework.HtmlRenderer.WinForms;
using _3PA.MainFeatures.FilteredLists;

namespace _3PA.MainFeatures.AutoCompletion {
    partial class AutoCompletionForm {
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
            try {
                fastOLV.FormatCell -= FastOlvOnFormatCell;
                fastOLV.BeforeSorting -= FastOlvOnBeforeSorting;
                fastOLV.KeyDown -= FastOlvOnKeyDown;
                MouseLeave -= CustomOnMouseLeave;
                fastOLV.MouseLeave -= CustomOnMouseLeave;
                fastOLV.DoubleClick -= FastOlvOnDoubleClick;

            } catch (Exception e) {
                ErrorHandler.LogError(e);
            }
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.fastOLV = new FastObjectListViewWoScrolls();
            this.Keyword = ((OLVColumn)(new OLVColumn()));
            this.Type = ((OLVColumn)(new OLVColumn()));
            this.nbitems = new YamuiLabel();
            this.yamuiLabel1 = new YamuiLabel();
            this.htmlToolTip = new HtmlToolTip();
            ((ISupportInitialize)(this.fastOLV)).BeginInit();
            this.SuspendLayout();
            // 
            // fastOLV
            // 
            this.fastOLV.Activation = ItemActivation.TwoClick;
            this.fastOLV.AllColumns.Add(this.Keyword);
            this.fastOLV.AllColumns.Add(this.Type);
            this.fastOLV.AutoArrange = false;
            this.fastOLV.BorderStyle = BorderStyle.None;
            this.fastOLV.CausesValidation = false;
            this.fastOLV.Columns.AddRange(new ColumnHeader[] {
            this.Keyword});
            this.fastOLV.CopySelectionOnControlC = false;
            this.fastOLV.CopySelectionOnControlCUsesDragSource = false;
            this.fastOLV.Cursor = Cursors.Default;
            this.fastOLV.Dock = DockStyle.Top;
            this.fastOLV.FullRowSelect = true;
            this.fastOLV.HasCollapsibleGroups = false;
            this.fastOLV.HeaderMaximumHeight = 0;
            this.fastOLV.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            this.fastOLV.HideSelection = false;
            this.fastOLV.Location = new Point(2, 2);
            this.fastOLV.MultiSelect = false;
            this.fastOLV.Name = "fastOLV";
            this.fastOLV.OwnerDraw = true;
            this.fastOLV.PersistentCheckBoxes = false;
            this.fastOLV.RowHeight = 20;
            this.fastOLV.SelectAllOnControlA = false;
            this.fastOLV.SelectColumnsOnRightClick = false;
            this.fastOLV.SelectColumnsOnRightClickBehaviour = ObjectListView.ColumnSelectBehaviour.None;
            this.fastOLV.ShowFilterMenuOnRightClick = false;
            this.fastOLV.ShowGroups = false;
            this.fastOLV.ShowHeaderInAllViews = false;
            this.fastOLV.ShowSortIndicators = false;
            this.fastOLV.Size = new Size(390, 400);
            this.fastOLV.TabIndex = 0;
            this.fastOLV.TriggerCellOverEventsWhenOverHeader = false;
            this.fastOLV.UpdateSpaceFillingColumnsWhenDraggingColumnDivider = false;
            this.fastOLV.UseCompatibleStateImageBehavior = false;
            this.fastOLV.UseFiltering = true;
            this.fastOLV.UseTabAsInput = false;
            this.fastOLV.View = View.Details;
            this.fastOLV.VirtualMode = true;
            // 
            // Keyword
            // 
            this.Keyword.AspectName = "DisplayText";
            this.Keyword.AutoCompleteEditor = false;
            this.Keyword.AutoCompleteEditorMode = AutoCompleteMode.None;
            this.Keyword.CellVerticalAlignment = StringAlignment.Center;
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
            this.nbitems.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.nbitems.Enabled = false;
            this.nbitems.Function = FontFunction.Small;
            this.nbitems.Location = new Point(329, 433);
            this.nbitems.Margin = new Padding(3);
            this.nbitems.Name = "nbitems";
            this.nbitems.Size = new Size(60, 12);
            this.nbitems.TabIndex = 3;
            this.nbitems.Text = "yamuiLabel1";
            this.nbitems.TextAlign = ContentAlignment.MiddleRight;
            // 
            // yamuiLabel1
            // 
            this.yamuiLabel1.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.yamuiLabel1.Enabled = false;
            this.yamuiLabel1.Function = FontFunction.Small;
            this.yamuiLabel1.Location = new Point(329, 420);
            this.yamuiLabel1.Margin = new Padding(3);
            this.yamuiLabel1.Name = "yamuiLabel1";
            this.yamuiLabel1.Size = new Size(60, 12);
            this.yamuiLabel1.TabIndex = 4;
            this.yamuiLabel1.Text = "Showing";
            this.yamuiLabel1.TextAlign = ContentAlignment.MiddleRight;
            // 
            // htmlToolTip
            // 
            this.htmlToolTip.AllowLinksHandling = true;
            this.htmlToolTip.AutoPopDelay = 90000;
            this.htmlToolTip.BaseStylesheet = null;
            this.htmlToolTip.InitialDelay = 300;
            this.htmlToolTip.MaximumSize = new Size(0, 0);
            this.htmlToolTip.OwnerDraw = true;
            this.htmlToolTip.ReshowDelay = 100;
            this.htmlToolTip.ShowAlways = true;
            // 
            // AutoCompletionForm
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(394, 449);
            this.Controls.Add(this.yamuiLabel1);
            this.Controls.Add(this.nbitems);
            this.Controls.Add(this.fastOLV);
            this.Location = new Point(0, 0);
            this.Name = "AutoCompletionForm";
            this.Padding = new Padding(2);
            this.StartPosition = FormStartPosition.Manual;
            this.Text = "AutoCompletionForm";
            ((ISupportInitialize)(this.fastOLV)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private FastObjectListViewWoScrolls fastOLV;
        private OLVColumn Keyword;
        private OLVColumn Type;
        private YamuiLabel nbitems;
        private YamuiLabel yamuiLabel1;
        private HtmlToolTip htmlToolTip;
    }
}