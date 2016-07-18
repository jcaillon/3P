#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (CodeExplorerForm.cs) is part of 3P.
// 
// 3P is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// 3P is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with 3P. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using YamuiFramework.Fonts;
using _3PA.Images;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.FilteredLists;
using _3PA.MainFeatures.NppInterfaceForm;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.CodeExplorer {

    internal partial class CodeExplorerForm : NppDockableDialog {

        #region fields

        private const string StrEmptyList = "Nothing to display!";

        /// <summary>
        /// Tracks toggle state of the expand/collapse
        /// </summary>
        private bool _isExpanded = true;

        /// <summary>
        /// The filter to apply to the autocompletion form
        /// </summary>
        private string FilterByText {
            get { return _filterByText; }
            set {
                _filterByText = value.ToLower();
                _isFiltering = !string.IsNullOrEmpty(_filterByText);
                ApplyFilter();
            }
        }

        /// <summary>
        /// Lowered case filter string
        /// </summary>
        private string _filterByText = "";

        /// <summary>
        /// Use this to change the image of the refresh button to let the user know the tree is being refreshed
        /// </summary>
        public bool Refreshing {
            get { return _refreshing; }
            set {
                _refreshing = value;
                this.SafeInvoke(form => {
                    if (_refreshing) {
                        buttonRefresh.BackGrndImage = ImageResources.refreshing;
                        buttonRefresh.Invalidate();
                        toolTipHtml.SetToolTip(buttonRefresh, "The tree is being refreshed, please wait");
                    } else {
                        buttonRefresh.BackGrndImage = ImageResources.refresh;
                        buttonRefresh.Invalidate();
                        toolTipHtml.SetToolTip(buttonRefresh, "Click to <b>Refresh</b> the tree");
                    }
                });
            }
        }
        private volatile bool _refreshing;

        private volatile bool _updating;

        private bool _isFiltering;

        /// <summary>
        ///  gets or sets the total items currently displayed in the form
        /// </summary>
        private int TotalItems { get; set; }

        // remember the original list of items
        private List<CodeExplorerItem> _initialObjectsList;

        // to keep track of expanded branches.. tje key is the DisplayText of the branch
        private static Dictionary<string, bool> _expandedBranches = new Dictionary<string, bool>();  

        private OLVColumn _displayText;
        private FastObjectListView fastOLV;

        #endregion

        #region constructor

        public CodeExplorerForm(EmptyForm formToCover) : base(formToCover) {
            InitializeComponent();

            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);

            #region Object list

            // column
            _displayText = new OLVColumn {
                AspectName = "DisplayText",
                FillsFreeSpace = true,
                IsEditable = false,
                ShowTextInHeader = false,
                Text = ""
            };

            // fast ovl
            fastOLV = new FastObjectListView();
            fastOLV.AllColumns.Add(_displayText);
            fastOLV.AutoArrange = false;
            fastOLV.BorderStyle = BorderStyle.None;
            fastOLV.Columns.AddRange(new ColumnHeader[] {_displayText});
            fastOLV.FullRowSelect = true;
            fastOLV.HeaderMaximumHeight = 0;
            fastOLV.HeaderStyle = ColumnHeaderStyle.None;
            fastOLV.HideSelection = false;
            fastOLV.LabelWrap = false;
            fastOLV.MultiSelect = false;
            fastOLV.OwnerDraw = true;
            fastOLV.RowHeight = 20;
            fastOLV.SelectAllOnControlA = false;
            fastOLV.ShowGroups = false;
            fastOLV.ShowHeaderInAllViews = false;
            fastOLV.ShowSortIndicators = false;
            fastOLV.SortGroupItemsByPrimaryColumn = false;
            fastOLV.UseCellFormatEvents = true;
            fastOLV.UseCompatibleStateImageBehavior = false;
            fastOLV.UseFiltering = true;
            fastOLV.UseHotItem = true;
            fastOLV.UseTabAsInput = true;
            fastOLV.View = View.Details;
            fastOLV.VirtualMode = true;
            fastOLV.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            fastOLV.Location = new Point(0, 30);
            fastOLV.Size = new Size(320, 300);

            // add to content panel
            Controls.Add(fastOLV);

            // Image getter
            _displayText.ImageGetter += ImageGetter;

            // Style the control
            StyleOvlTree();

            // Register to events
            fastOLV.KeyDown += FastOlvOnKeyDown;
            fastOLV.Click += FastOlvOnClick;

            // decorate rows
            fastOLV.UseCellFormatEvents = true;
            fastOLV.FormatCell += FastOlvOnFormatCell;

            // problems with the width of the column, set here
            _displayText.Width = fastOLV.Width - 17;
            fastOLV.ClientSizeChanged += (sender, args) => _displayText.Width = fastOLV.Width - 17;

            #endregion

            #region Buttons

            // Buttons images
            buttonCleanText.BackGrndImage = ImageResources.eraser;
            buttonExpandRetract.BackGrndImage = ImageResources.collapse;
            buttonRefresh.BackGrndImage = ImageResources.refresh;
            buttonIncludeExternal.BackGrndImage = ImageResources.External;
            buttonIncludeExternal.UseGreyScale = !Config.Instance.CodeExplorerDisplayExternalItems;

            RefreshSortButton();

            // Register buttons to events
            buttonCleanText.ButtonPressed += buttonCleanText_Click;
            buttonExpandRetract.ButtonPressed += buttonExpandRetract_Click;
            textBoxFilter.TextChanged += textBoxFilter_TextChanged;
            buttonRefresh.ButtonPressed += buttonRefresh_Click;
            buttonSort.ButtonPressed += buttonSort_Click;
            buttonIncludeExternal.ButtonPressed += ButtonIncludeExternalOnButtonPressed;
            textBoxFilter.KeyDown += TextBoxFilterOnKeyDown;

            // tooltips
            toolTipHtml.SetToolTip(buttonExpandRetract, "Toggle <b>Expand/Collapse</b>");
            toolTipHtml.SetToolTip(buttonCleanText, "<b>Clean</b> the current text filter");
            toolTipHtml.SetToolTip(buttonRefresh, "Click to <b>Refresh</b> the tree");
            toolTipHtml.SetToolTip(buttonSort, "Choose the way the items are sorted :<br>- Natural order (code order)<br>-Alphabetical order<br>-Uncategorized, unsorted");
            toolTipHtml.SetToolTip(buttonIncludeExternal, "Toggle on/off <b>the display</b> of external items in the list<br>(i.e. will a 'run' statement defined in a included file (.i) appear in this list or not)");
            toolTipHtml.SetToolTip(textBoxFilter, "Allows to <b>filter</b> the items of the list below");

            #endregion

        }

        #endregion

        #region core

        #region Paint Methods

        protected override void OnPaint(PaintEventArgs e) {
            e.Graphics.Clear(ThemeManager.Current.FormBack);
        }

        #endregion

        #region events

        /// <summary>
        /// Check/uncheck the menu depending on this form visibility
        /// </summary>
        /// <param name="e"></param>
        protected override void OnVisibleChanged(EventArgs e) {
            CodeExplorer.UpdateMenuItemChecked();
            base.OnVisibleChanged(e);
        }

        #endregion

        #endregion

        #region Object list

        #region cell formatting and style

        /// <summary>
        /// Return the image that needs to be display on the left of an item
        /// representing its type
        /// </summary>
        /// <param name="typeStr"></param>
        /// <returns></returns>
        private static Image GetImageFromStr(string typeStr) {
            Image tryImg = (Image)ImageResources.ResourceManager.GetObject(typeStr);
            return tryImg ?? ImageResources.Error;
        }

        /// <summary>
        /// Image getter for object rows
        /// </summary>
        /// <param name="rowObject"></param>
        /// <returns></returns>
        private static object ImageGetter(object rowObject) {
            var obj = (CodeExplorerItem)rowObject;
            if (obj == null) return ImageResources.Error;
            return GetImageFromStr((obj.IconType > 0) ? obj.IconType.ToString() : obj.Branch.ToString());
        }

        /// <summary>
        /// Event on format cell
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void FastOlvOnFormatCell(object sender, FormatCellEventArgs args) {
            CodeExplorerItem obj = (CodeExplorerItem) args.Model;
            var curScope = ParserHandler.GetScopeOfLine(Npp.Line.CurrentLine);

            // currently selected block
            if (curScope != null && !obj.IsNotBlock && obj.DisplayText.Equals(curScope.Name)) {
                RowBorderDecoration rbd = new RowBorderDecoration {
                    FillBrush = new SolidBrush(Color.FromArgb(50, ThemeManager.Current.MenuFocusedBack)),
                    BorderPen = new Pen(Color.FromArgb(128, ThemeManager.Current.MenuFocusedBack.IsColorDark() ? ControlPaint.Light(ThemeManager.Current.MenuFocusedBack, 0.10f) : ControlPaint.Dark(ThemeManager.Current.MenuFocusedBack, 0.10f)), 1),
                    BoundsPadding = new Size(-2, 0),
                    CornerRounding = 6.0f
                };
                args.SubItem.Decoration = rbd;
            }

            // display the flags
            int offset = -5;
            obj.DoForEachFlag((name, flag) => {
                Image tryImg = (Image)ImageResources.ResourceManager.GetObject(name);
                if (tryImg != null) {
                    ImageDecoration decoration = new ImageDecoration(tryImg, 100, ContentAlignment.MiddleRight) {
                        Offset = new Size(offset, 0)
                    };
                    if (args.SubItem.Decoration == null)
                        args.SubItem.Decoration = decoration;
                    else
                        args.SubItem.Decorations.Add(decoration);
                    offset -= 20;
                }
            });

            // display the sub string
            if (offset < -5) offset -= 5;
            if (!string.IsNullOrEmpty(obj.SubString)) {
                TextDecoration decoration = new TextDecoration(obj.SubString, 100) {
                    Alignment = ContentAlignment.MiddleRight,
                    Offset = new Size(offset, 0),
                    Font = FontManager.GetFont(FontStyle.Bold, 10),
                    TextColor = ThemeManager.Current.SubTextFore,
                    CornerRounding = 1f,
                    Rotation = 0,
                    BorderWidth = 1,
                    BorderColor = ThemeManager.Current.SubTextFore
                };
                args.SubItem.Decorations.Add(decoration);
            }
        }

        /// <summary>
        /// Apply thememanager theme to the treeview
        /// </summary>
        public void StyleOvlTree() {
            this.SafeInvoke(form => {
                OlvStyler.StyleIt(fastOLV, StrEmptyList);
                fastOLV.DefaultRenderer = new FilteredItemTreeRenderer();
            });
        }

        #endregion

        #region Update tree data

        /// <summary>
        /// This method uses the items found by the parser to update the code explorer tree (async)
        /// </summary>
        public void UpdateTreeData() {
            Task.Factory.StartNew(() => {
                this.SafeInvoke(form => {
                    try {
                        if (!_updating) {
                            _updating = true;
                            UpdateTreeDataAction();
                        }
                    } catch (Exception e) {
                        ErrorHandler.ShowErrors(e, "Error while getting the code explorer content");
                    } finally {
                        _updating = false;
                        Refreshing = false;
                    }
                });
            });
        }

        private void UpdateTreeDataAction() {

            // get the list of items
            var tempList = ParserHandler.ParserVisitor.ParsedExplorerItemsList.ToList();
            if (tempList.Count == 0)
                return;

            _initialObjectsList = new List<CodeExplorerItem>();

            if (Config.Instance.CodeExplorerSortingType != SortingType.Unsorted) {

                // we built the tree "manually"
                tempList.Sort(new ExplorerObjectSortingClass(Config.Instance.CodeExplorerSortingType));

                HashSet<CodeExplorerBranch> foundBranches = new HashSet<CodeExplorerBranch>();

                // for each distinct type of items, create a branch (if the branchType is not a root item like Root or MainBlock)
                CodeExplorerItem currentLvl1Parent = null;
                var iItem = 0;
                while (iItem < tempList.Count) {
                    var item = tempList[iItem];

                    // add an extra item that will be a new branch
                    if (!item.IsRoot && !foundBranches.Contains(item.Branch)) {
                        var branchDisplayText = item.Branch.GetDescription();

                        currentLvl1Parent = new CodeExplorerItem {
                            DisplayText = branchDisplayText,
                            Branch = item.Branch,
                            CanExpand = true,
                            // by default, the lvl 1 branches are expanded
                            IsExpanded = (!_expandedBranches.ContainsKey(branchDisplayText) ? _isExpanded : _expandedBranches[branchDisplayText])
                        };
                        foundBranches.Add(item.Branch);
                        _initialObjectsList.Add(currentLvl1Parent);
                    }

                    // Add a child item to the current branch
                    if (foundBranches.Contains(item.Branch)) {

                        // For each duplicated item (same Icon and same displayText), we create a new branch
                        var iIdentical = iItem + 1;
                        CodeExplorerFlag flags = 0;

                        // while we match identical items
                        while (iIdentical < tempList.Count &&
                            tempList[iItem].IconType == tempList[iIdentical].IconType &&
                            tempList[iItem].DisplayText.EqualsCi(tempList[iIdentical].DisplayText)) {
                            flags = flags | tempList[iIdentical].Flag;
                            iIdentical++;
                        }
                        // if we found identical item
                        if (iIdentical > iItem + 1) {
                            // we create a branch for them
                            var currentLvl2Parent = new CodeExplorerItem {
                                DisplayText = tempList[iItem].DisplayText,
                                Branch = tempList[iItem].Branch,
                                IconType = tempList[iItem].IconType,
                                CanExpand = true,
                                // by default, the lvl 2 branches are NOT expanded
                                IsExpanded = _expandedBranches.ContainsKey(tempList[iItem].DisplayText) && _expandedBranches[tempList[iItem].DisplayText],
                                Ancestors = new List<FilteredItemTree> { currentLvl1Parent },
                                SubString = "x" + (iIdentical - iItem),
                                IsNotBlock = tempList[iItem].IsNotBlock,
                                Flag = flags
                            };
                            _initialObjectsList.Add(currentLvl2Parent);
                            
                            // add child items to the newly created lvl 2 branch
                            for (int i = iItem; i < iIdentical; i++) {
                                tempList[i].Ancestors = new List<FilteredItemTree> { currentLvl1Parent, currentLvl2Parent };
                                tempList[i].IsNotBlock = true;
                                _initialObjectsList.Add(tempList[i]);
                            }
                            
                            // last child
                            (_initialObjectsList.LastOrDefault() ?? new CodeExplorerItem()).IsLastItem = true;

                            iItem += (iIdentical - iItem);

                            // last child of the branch
                            if (iItem >= tempList.Count - 1 || item.Branch != tempList[iItem].Branch)
                                currentLvl2Parent.IsLastItem = true;

                            continue;

                        }

                        // single item, add it normally
                        item.Ancestors = new List<FilteredItemTree> { currentLvl1Parent };
                        _initialObjectsList.Add(item);

                        // last child of the branch
                        if (iItem == tempList.Count - 1 || item.Branch != tempList[iItem + 1].Branch)
                            item.IsLastItem = true;

                    } else {
                        // add existing item as a root item
                        _initialObjectsList.Add(item);
                    }

                    iItem++;
                }

                // last branch, last item and first item
                (_initialObjectsList.FirstOrDefault() ?? new CodeExplorerItem()).IsFirstItem = true;
                (currentLvl1Parent ?? new CodeExplorerItem()).IsLastItem = true;
                (_initialObjectsList.LastOrDefault() ?? new CodeExplorerItem()).IsLastItem = true;
                
            } else {
                _initialObjectsList = tempList;
            }

            TotalItems = _initialObjectsList.Count;
            ApplyFilter();
        }

        #endregion

        #region events

        /// <summary>
        /// Executed when the user double click an item or press enter
        /// </summary>
        private void OnActivateItem() {
            var curItem = GetCurrentItem();
            if (curItem == null)
                return;

            // Branch clicked : expand/retract
            if (curItem.CanExpand) {
                curItem.IsExpanded = !curItem.IsExpanded;
                if (!_expandedBranches.ContainsKey(curItem.DisplayText)) {
                    _expandedBranches.Add(curItem.DisplayText, curItem.IsExpanded);
                } else {
                    _expandedBranches[curItem.DisplayText] = curItem.IsExpanded;
                }
                ApplyFilter();
                Npp.GrabFocus();
            } else {
                // Item clicked : go to line
                Npp.Goto(curItem.DocumentOwner, curItem.GoToLine, curItem.GoToColumn);
                fastOLV.Invalidate();
            }
        }

        /// <summary>
        /// Handles keydown event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="keyEventArgs"></param>
        private void FastOlvOnKeyDown(object sender, KeyEventArgs keyEventArgs) {
            keyEventArgs.Handled = OnKeyDown(keyEventArgs.KeyCode);
        }

        private void FastOlvOnClick(object sender, EventArgs eventArgs) {
            OnActivateItem();
        }

        #endregion

        #region on key events

        private bool OnKeyDown(Keys key) {
            bool handled = true;
            // down and up change the selection
            if (key == Keys.Up) {
                if (fastOLV.SelectedIndex > 0)
                    fastOLV.SelectedIndex--;
                else
                    fastOLV.SelectedIndex = (TotalItems - 1);
                if (fastOLV.SelectedIndex >= 0)
                    fastOLV.EnsureVisible(fastOLV.SelectedIndex);
            } else if (key == Keys.Down) {
                if (fastOLV.SelectedIndex < (TotalItems - 1))
                    fastOLV.SelectedIndex++;
                else
                    fastOLV.SelectedIndex = 0;
                if (fastOLV.SelectedIndex >= 0)
                    fastOLV.EnsureVisible(fastOLV.SelectedIndex);

                // escape close
            } else if (key == Keys.Escape) {
                Npp.GrabFocus();

                // enter and tab accept the current selection
            } else if (key == Keys.Enter) {
                OnActivateItem();

            } else if (key == Keys.Tab) {
                OnActivateItem();
                GiveFocustoTextBox();

                // else, any other key is unhandled
            } else {
                handled = false;
            }

            // down and up activate the display of tooltip
            if (key == Keys.Up || key == Keys.Down) {
                // TODO
                //InfoToolTip.InfoToolTip.ShowToolTipFromAutocomplete(GetCurrentSuggestion(), new Rectangle(new Point(Location.X, Location.Y), new Size(Width, Height)), _isReversed);
            }
            return handled;
        }

        #endregion

        #region Filter

        /// <summary>
        /// this methods sorts the items to put the best match on top and then filter it with modelFilter
        /// </summary>
        private void ApplyFilter() {
            if (_initialObjectsList == null || _initialObjectsList.Count == 0)
                return;

            // save position in the list
            var curPos = new Point(fastOLV.SelectedIndex, fastOLV.TopItemIndex);

            // apply filter to each item in the list then set the list
            try {
                _initialObjectsList.ForEach(data => data.FilterApply(_filterByText));
            } catch (Exception e) {
                if (!(e is NullReferenceException))
                    ErrorHandler.Log(e.ToString());
            }
            if (!_isFiltering) {
                fastOLV.SetObjects(_initialObjectsList);
            } else {
                fastOLV.SetObjects(_initialObjectsList.OrderBy(data => data.FilterDispertionLevel).ToList());
            }

            // display as tree or flat list?
            ((FilteredItemTreeRenderer)fastOLV.DefaultRenderer).DoNotDrawTree = (Config.Instance.CodeExplorerSortingType == SortingType.Unsorted) || _isFiltering;

            // apply the filter, need to match the filter + need to be an active type (Selector button activated)
            fastOLV.ModelFilter = new ModelFilter(FilterPredicate);

            // update total items
            TotalItems = ((ArrayList)fastOLV.FilteredObjects).Count;

            // reposition the cursor in the list
            if (TotalItems > 0) {
                fastOLV.SelectedIndex = Math.Max(0, Math.Min(curPos.X, TotalItems - 1));
                fastOLV.TopItemIndex = Math.Max(0, Math.Min(curPos.Y, TotalItems - 1));
            }
        }

        /// <summary>
        /// if true, the item isn't filtered
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private bool FilterPredicate(object o) {
            var item = (CodeExplorerItem)o;
            bool output = false;
            if (item != null) {
                // Match filter
                output = item.FilterFullyMatch;

                // when filtering, only display items not branches
                if (Config.Instance.CodeExplorerSortingType == SortingType.Unsorted || _isFiltering)
                    output = output && !item.CanExpand;
                else
                    // branches it belongs to must be expanded
                    output = output && (item.Ancestors == null || !item.Ancestors.Exists(tree => tree.CanExpand && !tree.IsExpanded));
            }
            return output;
        }

        #endregion

        #region Misc

        /// <summary>
        /// Get the current selected item
        /// </summary>
        /// <returns></returns>
        private CodeExplorerItem GetCurrentItem() {
            try {
                if (fastOLV.SelectedItem != null)
                    return (CodeExplorerItem)fastOLV.SelectedItem.RowObject;
            } catch (Exception x) {
                ErrorHandler.Log(x.Message);
            }
            return null;
        }

        public void Redraw() {
            this.SafeInvoke(form => { fastOLV.Invalidate(); });
        }

        /// <summary>
        /// Explicit
        /// </summary>
        private void GiveFocustoTextBox() {
            textBoxFilter.Focus();
        }

        /// <summary>
        /// Explicit
        /// </summary>
        private void ClearFilter() {
            textBoxFilter.Text = "";
            FilterByText = "";
            textBoxFilter.Invalidate();
        }

        public void RefreshParserAndCodeExplorer() {
            this.SafeInvoke(form => {
                ClearFilter();
                ParserHandler.ParserVisitor.ClearSavedParserVisitors();
                Plug.DoNppDocumentSwitched();
            });
        }

        #endregion

        #endregion

        #region Button events

        private void textBoxFilter_TextChanged(object sender, EventArgs e) {
            FilterByText = textBoxFilter.Text;
        }

        private void TextBoxFilterOnKeyDown(object sender, KeyEventArgs keyEventArgs) {
            keyEventArgs.Handled = OnKeyDown(keyEventArgs.KeyCode);
        }

        private void buttonRefresh_Click(object sender, EventArgs e) {
            if (Refreshing) {
                return;
            }
            fastOLV.Refresh();
            fastOLV.BuildList();
            Refresh();
            RefreshParserAndCodeExplorer();

            Npp.GrabFocus();
        }

        private void buttonSort_Click(object sender, EventArgs e) {
            Config.Instance.CodeExplorerSortingType++;
            if (Config.Instance.CodeExplorerSortingType > SortingType.Unsorted)
                Config.Instance.CodeExplorerSortingType = SortingType.NaturalOrder;
            ClearFilter();
            UpdateTreeData();

            RefreshSortButton();

            Npp.GrabFocus();
        }

        private void RefreshSortButton() {
            buttonSort.BackGrndImage = Config.Instance.CodeExplorerSortingType == SortingType.Unsorted ? ImageResources.clear_filters : (Config.Instance.CodeExplorerSortingType == SortingType.Alphabetical ? ImageResources.alphabetical_sorting : ImageResources.numerical_sorting);
            buttonSort.Invalidate();
        }

        private void buttonCleanText_Click(object sender, EventArgs e) {
            ClearFilter();

            GiveFocustoTextBox();
        }

        private void buttonExpandRetract_Click(object sender, EventArgs e) {
            _isExpanded = !_isExpanded;
            _expandedBranches.Clear();
            UpdateTreeData();
            // update button
            buttonExpandRetract.BackGrndImage = _isExpanded ? ImageResources.collapse : ImageResources.expand;
            buttonExpandRetract.Invalidate();

            Npp.GrabFocus();
        }

        private void ButtonIncludeExternalOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            // change option and image
            Config.Instance.CodeExplorerDisplayExternalItems = !Config.Instance.CodeExplorerDisplayExternalItems;
            buttonIncludeExternal.UseGreyScale = !Config.Instance.CodeExplorerDisplayExternalItems;

            // parse document
            Plug.DoNppDocumentSwitched();

            Npp.GrabFocus();
        }

        #endregion

        #region SortingType

        public enum SortingType {
            NaturalOrder = 0,
            Alphabetical = 1,
            Unsorted = 2
        }

        #endregion

    }

    #region Sorting class

    /// <summary>
    /// Class used in objectlist.Sort method
    /// </summary>
    internal class ExplorerObjectSortingClass : IComparer<CodeExplorerItem> {

        /// <summary>
        /// returns the ranking of each BranchType, helps sorting them as we wish
        /// </summary>
        public static List<int> GetPriorityList {
            get {
                if (_explorerBranchTypePriority != null) return _explorerBranchTypePriority;
                _explorerBranchTypePriority = Config.GetPriorityList(typeof(CompletionType), "CodeExplorerPriorityList");
                return _explorerBranchTypePriority;
            }
        }

        private static List<int> _explorerBranchTypePriority;

        private CodeExplorerForm.SortingType _sortingType;

        public ExplorerObjectSortingClass(CodeExplorerForm.SortingType sortingType) {
            _sortingType = sortingType;
        }

        public int Compare(CodeExplorerItem x, CodeExplorerItem y) {

            // compare first by BranchType
            int compare = GetPriorityList[(int)x.Branch].CompareTo(GetPriorityList[(int)y.Branch]);
            if (compare != 0) return compare;

            // compare by IconType
            compare = x.IconType.CompareTo(y.IconType);
            if (compare != 0) return compare;

            // sort by display text
            if (_sortingType == CodeExplorerForm.SortingType.Alphabetical) {
                compare = String.Compare(x.DisplayText, y.DisplayText, StringComparison.CurrentCultureIgnoreCase);
                if (compare != 0) return compare;
            }

            return x.GoToLine.CompareTo(y.GoToLine);
        }
    }

    #endregion

}
