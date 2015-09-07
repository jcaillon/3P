using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BrightIdeasSoftware;
using BrightIdeasSoftware.Utilities;
using YamuiFramework.Fonts;
using YamuiFramework.Themes;
using _3PA.Images;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;

namespace _3PA.MainFeatures.DockableExplorer {
    public partial class DockableExplorerForm : Form {

        /// <summary>
        /// Tracks toggle state of the expand/collapse
        /// </summary>
        private bool _isExpanded;

        /// <summary>
        /// tracks if we want to display the "normal" list, with folders and stuff, or the
        /// unsorted list, which is the list in code order
        /// </summary>
        private bool _displayUnSorted;

        /// <summary>
        /// Use alternative back color... or not
        /// </summary>
        public bool UseAlternativeBackColor { set { ovlTree.UseAlternatingBackColors = value; } }

        public DockableExplorerForm() {
            InitializeComponent();

            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);

            // Can the given object be expanded?
            ovlTree.CanExpandGetter = delegate(Object x) {
                return (x is ExplorerCategories) && ((ExplorerCategories)x).HasChildren;
            };

            // What objects should belong underneath the given model object?
            ovlTree.ChildrenGetter = delegate(Object x) {
                if (x is ExplorerCategories)
                    return ((ExplorerCategories)x).Items;
                throw new ArgumentException("??");
            };

            // set the image list to use for the keywords
            ovlTree.SmallImageList = ExplorerContent.GetImageList();
            DisplayText.ImageGetter += rowObject => {
                if (rowObject is ExplorerCategories) {
                    var x = (ExplorerCategories)rowObject;
                    return (int) x.MyIcon;
                }
                var y = (ExplorerItems)rowObject;
                return (int)y.MyIcon;
            };


            // Style the control
            StyleOvlTree();

            buttonCleanText.BackGrndImage = ImageResources.eraser;
            buttonExpandRetract.BackGrndImage = ImageResources.collapse;
            buttonRefresh.BackGrndImage = ImageResources.refresh;
            buttonSort.BackGrndImage = ImageResources.numerical_sorting_12;
            _isExpanded = true;
        }

        #region Paint Methods
        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e) {
            var backColor = ThemeManager.Current.FormColorBackColor;
            e.Graphics.Clear(backColor);
        }
        #endregion

        /// <summary>
        /// Apply thememanager theme to the treeview
        /// </summary>
        public void StyleOvlTree() {
            // Style the control
            ovlTree.OwnerDraw = true;
            ovlTree.Font = FontManager.GetLabelFont(LabelFunction.AutoCompletion);
            ovlTree.BackColor = ThemeManager.Current.AutoCompletionNormalBackColor;
            ovlTree.AlternateRowBackColor = ThemeManager.Current.AutoCompletionNormalAlternateBackColor;
            ovlTree.ForeColor = ThemeManager.Current.AutoCompletionNormalForeColor;
            ovlTree.HighlightBackgroundColor = ThemeManager.Current.AutoCompletionFocusBackColor;
            ovlTree.HighlightForegroundColor = ThemeManager.Current.AutoCompletionFocusForeColor;
            ovlTree.UnfocusedHighlightBackgroundColor = ovlTree.HighlightBackgroundColor;
            ovlTree.UnfocusedHighlightForegroundColor = ovlTree.HighlightForegroundColor;

            // Decorate and configure hot item
            ovlTree.UseHotItem = true;
            ovlTree.HotItemStyle = new HotItemStyle();
            ovlTree.HotItemStyle.BackColor = ThemeManager.Current.AutoCompletionHoverBackColor;
            ovlTree.HotItemStyle.ForeColor = ThemeManager.Current.AutoCompletionHoverForeColor;

            // overlay of empty list :
            ovlTree.EmptyListMsg = "Nothing to display!";
            TextOverlay textOverlay = ovlTree.EmptyListMsgOverlay as TextOverlay;
            if (textOverlay != null) {
                textOverlay.TextColor = ThemeManager.Current.AutoCompletionNormalForeColor;
                textOverlay.BackColor = ThemeManager.Current.AutoCompletionNormalAlternateBackColor;
                textOverlay.BorderColor = ThemeManager.Current.AutoCompletionNormalForeColor;
                textOverlay.BorderWidth = 4.0f;
                textOverlay.Font = FontManager.GetFont(FontStyle.Bold, 30f);
                textOverlay.Rotation = -5;
            }
        }

        /// <summary>
        /// Check/uncheck the menu depending on this form visibility
        /// </summary>
        /// <param name="e"></param>
        protected override void OnVisibleChanged(EventArgs e) {
            DockableExplorer.UpdateMenuItemChecked();
            base.OnVisibleChanged(e);
        }

        /// <summary>
        /// Called on resize, to make the treeview match the whole width
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e) {
            DisplayText.Width = Width - 2;
            base.OnResize(e);
        }

        /// <summary>
        /// Call this method to refresh the content of the tree view
        /// </summary>
        public void RefreshExplorer() {
            ovlTree.SetObjects(_displayUnSorted ? ExplorerContent.UnsortedCategory : ExplorerContent.Categories);
        }

        public void ExpandAll() {
            try {
                ovlTree.ExpandAll();
            } catch (Exception) {
                // ignored
            }
        }

        /// <summary>
        /// if we want to remember which categories were expanded to restore them, use that
        /// </summary>
        /// <param name="items"></param>
        public void ExpandItems(IEnumerable items) {
            ovlTree.ExpandedObjects = items;
        }

        /// <summary>
        /// See method ExpandItems
        /// </summary>
        /// <returns></returns>
        public IEnumerable GetExpandedItems() {
            return ovlTree.ExpandedObjects;
        }

        /// <summary>
        /// Method to call to rebuild the tree
        /// </summary>
        public void ForceRebuildAll() {
            ovlTree.RebuildAll(true);
        }

        private void buttonCleanText_Click(object sender, EventArgs e) {
            textBoxFilter.Text = "";
            textBoxFilter.Invalidate();
            CleanTextRenderer();
        }

        private void buttonExpandRetract_Click(object sender, EventArgs e) {
            _isExpanded = !_isExpanded;
            if (_isExpanded)
                ovlTree.ExpandAll();
            else
                ovlTree.CollapseAll();
            buttonExpandRetract.BackGrndImage = _isExpanded ? ImageResources.collapse : ImageResources.expand;
            buttonExpandRetract.Invalidate();
        }

        private void CleanTextRenderer() {
            DisplayText.Renderer = new TreeListView.TreeRenderer();
        }

        private void textBoxFilter_TextChanged(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace(textBoxFilter.Text)) {
                textBoxFilter.Text = "";
                CleanTextRenderer();
                ovlTree.ModelFilter = null;
                return;
            }
            ovlTree.ModelFilter = new ModelFilter((o => o is ExplorerItems && ((ExplorerItems)o).DisplayText.Contains(textBoxFilter.Text, StringComparison.InvariantCultureIgnoreCase)));
            ovlTree.AdditionalFilter = TextMatchFilter.Contains(ovlTree, textBoxFilter.Text);
            DisplayText.Renderer = new CustomHighlightTextRenderer(ovlTree, textBoxFilter.Text);
        }

        private void buttonRefresh_Click(object sender, EventArgs e) {
            textBoxFilter.Text = "";
            CleanTextRenderer();
            ovlTree.ModelFilter = null;
            DockableExplorer.RefreshContent();
        }

        private void buttonSort_Click(object sender, EventArgs e) {
            _displayUnSorted = !_displayUnSorted;
            RefreshExplorer();
            ExpandAll();
            buttonSort.BackGrndImage = _displayUnSorted ? ImageResources.clear_filters : ImageResources.numerical_sorting_12;
            buttonSort.Invalidate();
        }
    }
}
