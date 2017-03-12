#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamuiFramework.Controls;
using YamuiFramework.Controls.YamuiList;
using YamuiFramework.Helper;
using _3PA.Images;
using _3PA.Lib;
using _3PA.MainFeatures.NppInterfaceForm;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.CodeExplorer {
    internal partial class CodeExplorerForm : NppDockableDialogForm {
        #region private

        private volatile bool _refreshing;

        private volatile bool _updating;

        // remember the original list of items
        private List<CodeExplorerItem> _initialObjectsList;
        private bool _isExpanded = true;

        #endregion

        #region Fields public

        /// <summary>
        /// Use this to change the image of the refresh button to let the user know the tree is being refreshed
        /// </summary>
        public bool Refreshing {
            get { return _refreshing; }
            set {
                _refreshing = value;
                this.SafeInvoke(form => {
                    var refreshButton = filterbox.ExtraButtonsList != null && filterbox.ExtraButtonsList.Count > 0 ? filterbox.ExtraButtonsList[0] : null;
                    if (refreshButton == null)
                        return;
                    if (_refreshing) {
                        refreshButton.BackGrndImage = ImageResources.Refreshing;
                        refreshButton.Invalidate();
                        toolTipHtml.SetToolTip(refreshButton, "The tree is being refreshed, please wait");
                    } else {
                        refreshButton.BackGrndImage = ImageResources.Refresh;
                        refreshButton.Invalidate();
                        toolTipHtml.SetToolTip(refreshButton, "Click to <b>Refresh</b> the tree");
                    }
                });
            }
        }

        public YamuiFilteredTypeTreeList YamuiList {
            get { return yamuiList; }
        }

        public YamuiFilterBox FilterBox {
            get { return filterbox; }
        }

        #endregion

        #region constructor

        public CodeExplorerForm(NppDockableDialogEmptyForm formToCover) : base(formToCover) {
            InitializeComponent();

            // add the refresh button to the filter box
            filterbox.ExtraButtons = new List<YamuiFilterBox.YamuiFilterBoxButton> {
                new YamuiFilterBox.YamuiFilterBoxButton {
                    Image = ImageResources.Refresh,
                    OnClic = buttonRefresh_Click
                },
                new YamuiFilterBox.YamuiFilterBoxButton {
                    Image = ImageResources.Collapse,
                    OnClic = buttonExpandRetract_Click,
                    ToolTip = "Toggle <b>Expand/Collapse</b>"
                },
                new YamuiFilterBox.YamuiFilterBoxButton {
                    Image = ImageResources.Numerical_sorting,
                    OnClic = buttonSort_Click,
                    ToolTip = "Choose the way the items are sorted :<br>- Natural order (code order)<br>-Alphabetical order<br>-Uncategorized, unsorted"
                },
                new YamuiFilterBox.YamuiFilterBoxButton {
                    Image = ImageResources.FromInclude,
                    OnClic = ButtonFromIncludeOnButtonPressed,
                    ToolTip = "Toggle on/off <b>the display</b> of external items in the list<br>(i.e. will a 'run' statement defined in a included file (.i) appear in this list or not)"
                }
            };
            filterbox.Initialize(yamuiList);

            yamuiList.ShowTreeBranches = Config.Instance.ShowTreeBranches;
            yamuiList.EmptyListString = @"Nothing to display";

            // allows to sort the list when we are in search mode (we then need to sort alphabetically again)
            yamuiList.SortingClass = CodeExplorerSortingClass<ListItem>.Instance;

            Refreshing = false;
            filterbox.ExtraButtonsList[1].BackGrndImage = _isExpanded ? ImageResources.Collapse : ImageResources.Expand;
            filterbox.ExtraButtonsList[3].UseGreyScale = !Config.Instance.CodeExplorerDisplayExternalItems;
            filterbox.ExtraButtonsList[2].BackGrndImage = Config.Instance.CodeExplorerSortingType == SortingType.Unsorted ? ImageResources.Clear_filters : (Config.Instance.CodeExplorerSortingType == SortingType.Alphabetical ? ImageResources.Alphabetical_sorting : ImageResources.Numerical_sorting);

            // list events
            yamuiList.RowClicked += YamuiListOnRowClicked;
            yamuiList.EnterPressed += YamuiListOnEnterPressed;

            //var curScope = ParserHandler.GetScopeOfLine(Npp.Line.CurrentLine);
            //return curScope != null && !IsNotBlock && DisplayText.Equals(curScope.Name);
        }

        #endregion

        #region core

        /// <summary>
        /// Check/uncheck the menu depending on this form visibility
        /// </summary>
        /// <param name="e"></param>
        protected override void OnVisibleChanged(EventArgs e) {
            CodeExplorer.Instance.UpdateMenuItemChecked();
            base.OnVisibleChanged(e);
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
                // apply custom sorting
                tempList.Sort(CodeExplorerSortingClass<CodeExplorerItem>.GetInstance(Config.Instance.CodeExplorerSortingType));

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
                            IsExpanded = true, // by default, expand lvl 1 branch
                            Children = new List<FilteredTypeTreeListItem>()
                        };
                        foundBranches.Add(item.Branch);
                        _initialObjectsList.Add(currentLvl1Parent);
                    }

                    // Add a child item to the current branch
                    if (foundBranches.Contains(item.Branch) && currentLvl1Parent != null) {
                        // For each duplicated item (same Icon and same displayText), we create a new branch
                        var iIdentical = iItem + 1;
                        ParseFlag flags = 0;

                        // while we match identical items
                        while (iIdentical < tempList.Count &&
                               tempList[iItem].IconType == tempList[iIdentical].IconType &&
                               tempList[iItem].Branch == tempList[iIdentical].Branch &&
                               tempList[iItem].DisplayText.EqualsCi(tempList[iIdentical].DisplayText)) {
                            flags = flags | tempList[iIdentical].Flags;
                            iIdentical++;
                        }
                        // if we found identical item
                        if (iIdentical > iItem + 1) {
                            // we create a branch for them
                            var currentLvl2Parent = new CodeExplorerItem {
                                DisplayText = tempList[iItem].DisplayText,
                                Branch = tempList[iItem].Branch,
                                IconType = tempList[iItem].IconType,
                                IsExpanded = false, // by default, the lvl 2 branches are NOT expanded
                                SubString = "x" + (iIdentical - iItem),
                                IsNotBlock = tempList[iItem].IsNotBlock,
                                Flags = flags,
                                Children = new List<FilteredTypeTreeListItem>()
                            };
                            currentLvl1Parent.Children.Add(currentLvl2Parent);

                            // add child items to the newly created lvl 2 branch
                            for (int i = iItem; i < iIdentical; i++) {
                                currentLvl2Parent.Children.Add(tempList[i]);
                            }

                            iItem += (iIdentical - iItem);
                            continue;
                        }

                        // single item, add it normally
                        currentLvl1Parent.Children.Add(item);
                    } else {
                        // add existing item as a root item
                        _initialObjectsList.Add(item);
                    }

                    iItem++;
                }
            } else {
                _initialObjectsList = tempList;
            }

            yamuiList.SetItems(_initialObjectsList.Cast<ListItem>().ToList());

            // also update current scope 
            UpdateCurrentScope();
        }

        #endregion

        #region public

        /// <summary>
        /// Updates the current scope to inform the user in which scope the caret is currently in
        /// </summary>
        public void UpdateCurrentScope() {
            if (Npp.CurrentFile.IsProgress) {
                var currentScope = ParserHandler.GetScopeOfLine(Npp.Line.CurrentLine);
                if (currentScope != null) {
                    pbCurrentScope.BackGrndImage = Utils.GetImageFromStr(currentScope.ScopeType.ToString());
                    pbCurrentScope.Invalidate();
                    lbCurrentScope.Text = currentScope.Name;
                    return;
                }
            }
            pbCurrentScope.BackGrndImage = ImageResources.NotApplicable;
            pbCurrentScope.Invalidate();
            lbCurrentScope.Text = @"Not applicable";
        }

        #endregion

        #region events

        /// <summary>
        /// Redirect mouse wheel to yamuilist?
        /// </summary>
        protected override void OnMouseWheel(MouseEventArgs e) {
            if (ActiveControl is YamuiFilterBox)
                yamuiList.DoScroll(e.Delta);
            base.OnMouseWheel(e);
        }

        /// <summary>
        /// Executed when the user double click an item or press enter
        /// </summary>
        private bool OnActivateItem() {
            var curItem = yamuiList.SelectedItem as CodeExplorerItem;
            if (curItem == null)
                return false;

            if (!curItem.CanExpand) {
                // Item clicked : go to line
                Npp.Goto(curItem.DocumentOwner, curItem.GoToLine, curItem.GoToColumn);
                return true;
            }

            return false;
        }

        #endregion

        #region Button events

        private void YamuiListOnEnterPressed(YamuiScrollList yamuiScrollList, KeyEventArgs keyEventArgs) {
            OnActivateItem();
        }

        private void YamuiListOnRowClicked(YamuiScrollList yamuiScrollList, MouseEventArgs mouseEventArgs) {
            if (OnActivateItem())
                Npp.GrabFocus();
        }

        private void buttonRefresh_Click(YamuiButtonImage sender, EventArgs e) {
            if (Refreshing)
                return;
            ParserHandler.ParserVisitor.ClearSavedParserVisitors();
            Plug.DoNppDocumentSwitched();
            Npp.GrabFocus();
        }

        private void buttonSort_Click(YamuiButtonImage sender, EventArgs e) {
            Config.Instance.CodeExplorerSortingType++;
            if (Config.Instance.CodeExplorerSortingType > SortingType.Unsorted)
                Config.Instance.CodeExplorerSortingType = SortingType.NaturalOrder;
            UpdateTreeData();
            filterbox.ExtraButtonsList[2].BackGrndImage = Config.Instance.CodeExplorerSortingType == SortingType.Unsorted ? ImageResources.Clear_filters : (Config.Instance.CodeExplorerSortingType == SortingType.Alphabetical ? ImageResources.Alphabetical_sorting : ImageResources.Numerical_sorting);
            Npp.GrabFocus();
        }

        private void buttonExpandRetract_Click(YamuiButtonImage sender, EventArgs e) {
            if (_isExpanded)
                yamuiList.ForceAllToCollapse();
            else
                yamuiList.ForceAllToExpand();
            _isExpanded = !_isExpanded;
            filterbox.ExtraButtonsList[1].BackGrndImage = _isExpanded ? ImageResources.Collapse : ImageResources.Expand;
            Npp.GrabFocus();
        }

        private void ButtonFromIncludeOnButtonPressed(YamuiButtonImage sender, EventArgs e) {
            // change option and image
            Config.Instance.CodeExplorerDisplayExternalItems = !Config.Instance.CodeExplorerDisplayExternalItems;
            filterbox.ExtraButtonsList[3].UseGreyScale = !Config.Instance.CodeExplorerDisplayExternalItems;
            // Parse the document
            ParserHandler.ParseCurrentDocument(true);
            Npp.GrabFocus();
        }

        #endregion
    }
}