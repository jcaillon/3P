using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using BrightIdeasSoftware.Utilities;
using YamuiFramework.Controls;
using YamuiFramework.Fonts;
using YamuiFramework.Themes;
using _3PA.Images;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;

namespace _3PA.MainFeatures.DockableExplorer {
    public partial class CodeExplorerPage : YamuiPage {

        #region fields

        private const string EmptyListString = "Nothing to display!";

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
        public bool UseAlternativeBackColor {
            set { ovlTree.UseAlternatingBackColors = value; }
        }

        private Dictionary<string, bool> _expandedBranches = new Dictionary<string, bool>();
        private int _topItemIndex;

        // list of items to display in the tree
        private static List<ExplorerItem> _unsortedItems = new List<ExplorerItem>();
        private static List<ExplorerItem> _rootItems = new List<ExplorerItem>();
        private static List<ExplorerItem> _items = new List<ExplorerItem>();

        #endregion

        #region constructor

        public CodeExplorerPage() {
            InitializeComponent();

            // Can the given object be expanded?
            ovlTree.CanExpandGetter = x => ((ExplorerItem)x).HasChildren;

            // What objects should belong underneath the given model object?
            ovlTree.ChildrenGetter = delegate(object x) {
                var obj = (ExplorerItem) x;
                return (obj != null && obj.HasChildren) ? obj.Items : null;
            };

            // set the image list to use for the keywords (corresponds with IconType)
            var imageListOfTypes = new ImageList {
                TransparentColor = Color.Transparent,
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(20, 20)
            };
            ImagelistAdd.AddFromImage(ImageResources.code, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.root, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.block, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.mainblock, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Procedure, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Function, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.onevents, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.External, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.run, imageListOfTypes);
            ovlTree.SmallImageList = imageListOfTypes;

            // Image getter
            DisplayText.ImageGetter += rowObject => (int) ((ExplorerItem)rowObject).Type;
            
            // Style the control
            StyleOvlTree();

            buttonCleanText.BackGrndImage = ImageResources.eraser;
            buttonExpandRetract.BackGrndImage = ImageResources.collapse;
            buttonRefresh.BackGrndImage = ImageResources.refresh;
            buttonSort.BackGrndImage = ImageResources.numerical_sorting_12;
            _isExpanded = true;

            // Register to events
            buttonCleanText.ButtonPressed += buttonCleanText_Click;
            buttonExpandRetract.ButtonPressed += buttonExpandRetract_Click;
            textBoxFilter.TextChanged += textBoxFilter_TextChanged;
            buttonRefresh.ButtonPressed += buttonRefresh_Click;
            buttonSort.ButtonPressed += buttonSort_Click;
            ovlTree.Click += OvlTreeOnClick;

            // decorate rows
            ovlTree.UseCellFormatEvents = true;
            ovlTree.FormatCell += FastOlvOnFormatCell;

            // tooltips
            toolTipHtml.SetToolTip(buttonExpandRetract, "Toggle <b>Expand/Collapse</b>");
            toolTipHtml.SetToolTip(buttonCleanText, "<b>Clean</b> the current text filter");
            toolTipHtml.SetToolTip(buttonRefresh, "Click to <b>Refresh</b> the tree");
            toolTipHtml.SetToolTip(buttonSort, "Toggle <b>Categories/Code order sorting</b>");

        }

        #endregion

        #region cell formatting

        /// <summary>
        /// Event on format cell
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void FastOlvOnFormatCell(object sender, FormatCellEventArgs args) {
            ExplorerItem obj = (ExplorerItem) args.Model;
            // display the flags
            int offset = -5;
            if (!obj.IsNotBlock && obj.DisplayText.EqualsCi(ParserHandler.GetCarretLineOwnerName)) {
                ImageDecoration decoration = new ImageDecoration(ImageResources.selection, ContentAlignment.MiddleRight) {
                    Offset = new Size(offset, 0)
                };
                args.SubItem.Decoration = decoration;
            }
        }

        #endregion

        #region public methods

        public void Redraw() {
            DisplayText.Width = ovlTree.Width - 17;
            ovlTree.RebuildAll(true);
        }

        /// <summary>
        /// This method uses the items found by the parser to update the code explorer tree
        /// </summary>
        public static void UpdateTreeData() {
            // Fetch items found by the parser
            _unsortedItems = ParserHandler.GetParsedExplorerItemsList();
            _items = ParserHandler.GetParsedExplorerItemsList();
            _items.Sort(new ExplorerObjectSortingClass());

            // init branches
            _rootItems.Clear();

            // add root items first
            foreach (var item in _items.Where(item => item.IsRoot)) {
                _rootItems.Add(item);
            }

            // for each distinct type of items, create a branch
            foreach (var type in _items.Select(x => x.Type).Distinct()) {
                if (_rootItems.Find(item => item.Type == type) != null) continue;
                _rootItems.Add(new ExplorerItem() {
                    DisplayText = ((ExplorerTypeAttr)type.GetAttributes()).DisplayText,
                    Type = type,
                    HasChildren = true,
                    IsRoot = true
                });
            }
        }

        /// <summary>
        /// Call this before updating the list of items to remember which branch is expanded
        /// </summary>
        public void RememberExpandedItems() {
            foreach (var root in ovlTree.Roots) {
                var branch = root as ExplorerItem;
                if (branch == null || !branch.HasChildren) continue;
                if (!_expandedBranches.ContainsKey(branch.DisplayText))
                    _expandedBranches.Add(branch.DisplayText, ovlTree.IsExpanded(root));
                else
                    _expandedBranches[branch.DisplayText] = ovlTree.IsExpanded(root);
            }

            _topItemIndex = ovlTree.TopItemIndex;
        }

        /// <summary>
        /// Call this after updating the list of items to set the remembered expanded branches
        /// </summary>
        public void SetRememberedExpandedItems() {
            foreach (var root in ovlTree.Roots) {
                var branch = root as ExplorerItem;
                if (branch == null || !branch.HasChildren) continue;
                if (_expandedBranches.ContainsKey(branch.DisplayText)) {
                    if (_expandedBranches[branch.DisplayText])
                        ovlTree.Expand(root);
                    else
                        ovlTree.Collapse(root);
                } else {
                    _expandedBranches.Add(branch.DisplayText, true);
                    ovlTree.Expand(root);
                }
            }

            if (_topItemIndex > 0 && _topItemIndex < ovlTree.GetItemCount()) {
                ovlTree.TopItemIndex = _topItemIndex;
                ovlTree.EnsureVisible(_topItemIndex);
            } else {
                ovlTree.TopItemIndex = 0;
                try {
                    ovlTree.EnsureVisible(0);
                } catch (Exception) {
                    // ignored
                }
            }
        }

        /// <summary>
        /// Static method used by items to return the list of their children
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<ExplorerItem> GetItemsFor(ExplorerType type) {
            if (type == ExplorerType.EverythingInCodeOrder)
                return _unsortedItems;
            return _items.Where(item => item.Type == type).ToList();
        }

        /// <summary>
        /// Call this method to initiate the content of the tree view
        /// </summary>
        public void InitSetObjects() {
            var unsortedBranch = new List<ExplorerItem>() {
                new ExplorerItem() {
                    DisplayText = "Everything in code order", 
                    Type = ExplorerType.EverythingInCodeOrder, 
                    HasChildren = true
                }
            };
            ovlTree.SetObjects(!_displayUnSorted ? _rootItems : unsortedBranch);
            DisplayText.Width = ovlTree.Width - 17;
        }

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
            ovlTree.HotItemStyle = new HotItemStyle {
                BackColor = ThemeManager.Current.AutoCompletionHoverBackColor,
                ForeColor = ThemeManager.Current.AutoCompletionHoverForeColor
            };

            // overlay of empty list :
            ovlTree.EmptyListMsg = EmptyListString;
            TextOverlay textOverlay = ovlTree.EmptyListMsgOverlay as TextOverlay;
            if (textOverlay != null) {
                textOverlay.TextColor = ThemeManager.Current.AutoCompletionNormalForeColor;
                textOverlay.BackColor = ThemeManager.Current.AutoCompletionNormalAlternateBackColor;
                textOverlay.BorderColor = ThemeManager.Current.AutoCompletionNormalForeColor;
                textOverlay.BorderWidth = 4.0f;
                textOverlay.Font = FontManager.GetFont(FontStyle.Bold, 30f);
                textOverlay.Rotation = -5;
            }

            CleanFilter();
        }

        public void ExpandAll() {
            try {
                ovlTree.ExpandAll();
            } catch (Exception) {
                // ignored
            }
        }
        #endregion

        #region events

        /// <summary>
        /// When the user double click an item on the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OvlTreeOnClick(object sender, EventArgs eventArgs) {
            // find currently selected item
            var selection = (ExplorerItem) ovlTree.SelectedObject;
            if (selection == null) return;
            // Branch clicked : expand/retract
            if (selection.HasChildren) {
                if (ovlTree.IsExpanded(selection))
                    ovlTree.Collapse(selection);
                else
                    ovlTree.Expand(selection);
                Npp.GrabFocus();
                return;
            }
            // Item clicked : go to line
            Npp.GoToLine(selection.GoToLine);
            Redraw();
        }

        private void textBoxFilter_TextChanged(object sender, EventArgs e) {
            // first char input? we remember the expanded/retracted branches
            if (!string.IsNullOrWhiteSpace(textBoxFilter.Text) && textBoxFilter.Text.Length == 1) {
                RememberExpandedItems();
                ExpandAll();
            }

            ApplyFilter();
        }

        private void buttonRefresh_Click(object sender, EventArgs e) {
            CleanFilter();
            AutoComplete.ParseCurrentDocument(true);
        }

        private void buttonSort_Click(object sender, EventArgs e) {
            _displayUnSorted = !_displayUnSorted;
            CleanFilter();
            InitSetObjects();
            ExpandAll();
            buttonSort.BackGrndImage = _displayUnSorted ? ImageResources.clear_filters : ImageResources.numerical_sorting_12;
            buttonSort.Invalidate();
        }

        private void buttonCleanText_Click(object sender, EventArgs e) {
            CleanFilter();
            textBoxFilter.Invalidate();
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

        #endregion

        #region filter

        public void ReapplyFilter() {
            if (string.IsNullOrWhiteSpace(textBoxFilter.Text)) return;
            string curFilter = textBoxFilter.Text;
            CleanFilter();
            textBoxFilter.Text = curFilter;
        }

        /// <summary>
        /// apply text filter (from textbox)
        /// </summary>
        private void ApplyFilter() {
            if (string.IsNullOrWhiteSpace(textBoxFilter.Text)) {
                CleanFilter();
                return;
            }

            // filter the tree..
            ovlTree.ModelFilter = new ModelFilter(FilterPredicate);
            ovlTree.TreeColumnRenderer = new CustomTreeRenderer(textBoxFilter.Text);
        }

        /// <summary>
        /// Filter predicate
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private bool FilterPredicate(object o) {
            var obj = (ExplorerItem) o;
            return (!obj.HasChildren && obj.DisplayText.ToLower().FullyMatchFilter(textBoxFilter.Text));
        }

        /// <summary>
        /// Update the renderer (the filter)
        /// </summary>
        private void CleanFilter() {
            ovlTree.TreeColumnRenderer = new CustomTreeRenderer("");
            textBoxFilter.Text = "";
            ovlTree.ModelFilter = null;
            SetRememberedExpandedItems();
        }
        #endregion


    }

    /// <summary>
    /// Class used in objectlist.Sort method
    /// </summary>
    public class ExplorerObjectSortingClass : IComparer<ExplorerItem> {
        public int Compare(ExplorerItem x, ExplorerItem y) {
            // compare first by CompletionType
            int compare = x.Type.CompareTo(y.Type);
            if (compare != 0) return compare;
            // sort by display text in last resort
            return string.Compare(x.DisplayText, y.DisplayText, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
