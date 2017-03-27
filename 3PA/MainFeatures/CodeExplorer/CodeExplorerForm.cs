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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using YamuiFramework.Controls;
using YamuiFramework.Controls.YamuiList;
using _3PA.MainFeatures.Parser;
using _3PA.NppCore;
using _3PA.NppCore.NppInterfaceForm;
using _3PA._Resource;

namespace _3PA.MainFeatures.CodeExplorer {
    internal partial class CodeExplorerForm : NppDockableDialogForm {
        #region private

        private volatile bool _refreshing;

        private bool _isExpanded = true;

        #endregion

        #region constructor

        public CodeExplorerForm(NppEmptyForm formToCover) : base(formToCover) {
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
                    ToolTip = "Toggle on/off <b>the display</b>, in the explorer, the functions and procedures loaded in persistent in this file"
                }
            };
            filterbox.Initialize(yamuiList);
            filterbox.ExtraButtonsList[1].BackGrndImage = _isExpanded ? ImageResources.Collapse : ImageResources.Expand;
            filterbox.ExtraButtonsList[3].UseGreyScale = !Config.Instance.CodeExplorerDisplayExternalItems;
            filterbox.ExtraButtonsList[2].BackGrndImage = Config.Instance.CodeExplorerSortingType == SortingType.Alphabetical ? ImageResources.Alphabetical_sorting : ImageResources.Numerical_sorting;

            Refreshing = false;

            yamuiList.ShowTreeBranches = Config.Instance.ShowTreeBranches;
            yamuiList.EmptyListString = @"Nothing to display";

            // list events
            yamuiList.RowClicked += YamuiListOnRowClicked;
            yamuiList.EnterPressed += YamuiListOnEnterPressed;
        }

        #endregion

        #region Public

        /// <summary>
        /// Use this to change the image of the refresh button to let the user know the tree is being refreshed
        /// </summary>
        public bool Refreshing {
            get { return _refreshing; }
            set {
                _refreshing = value;
                var refreshButton = filterbox.ExtraButtonsList != null && filterbox.ExtraButtonsList.Count > 0 ? filterbox.ExtraButtonsList[0] : null;
                if (refreshButton == null)
                    return;
                if (_refreshing) {
                    refreshButton.BackGrndImage = ImageResources.Refreshing;
                    toolTipHtml.SetToolTip(refreshButton, "The tree is being refreshed, please wait");
                } else {
                    refreshButton.BackGrndImage = ImageResources.Refresh;
                    toolTipHtml.SetToolTip(refreshButton, "Click to <b>Refresh</b> the tree");
                }
            }
        }

        public void ShowTreeBranches(bool show) {
            yamuiList.ShowTreeBranches = show;
        }

        /// <summary>
        /// This method uses the items found by the parser to update the code explorer tree (async)
        /// </summary>
        /// <param name="codeExplorerItems"></param>
        public void UpdateTreeData(List<CodeItem> codeExplorerItems) {
            yamuiList.SetItems(codeExplorerItems.Cast<ListItem>().ToList());
        }

        /// <summary>
        /// Updates the current scope to inform the user in which scope the caret is currently in
        /// </summary>
        public void UpdateCurrentScope(string text, Image image) {
            pbCurrentScope.BackGrndImage = image;
            lbCurrentScope.Text = text;
        }

        #endregion

        #region Private

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
            var curItem = yamuiList.SelectedItem as CodeItem;
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
                Sci.GrabFocus();
        }

        private void buttonRefresh_Click(YamuiButtonImage sender, EventArgs e) {
            if (Refreshing)
                return;
            ParserHandler.ClearStaticData();
            Npp.CurrentSci.Lines.Reset();
            ParserHandler.ParseDocumentNow();
            Sci.GrabFocus();
        }

        private void buttonSort_Click(YamuiButtonImage sender, EventArgs e) {
            Config.Instance.CodeExplorerSortingType++;
            if (Config.Instance.CodeExplorerSortingType > SortingType.Alphabetical)
                Config.Instance.CodeExplorerSortingType = SortingType.NaturalOrder;
            filterbox.ExtraButtonsList[2].BackGrndImage = Config.Instance.CodeExplorerSortingType == SortingType.Alphabetical ? ImageResources.Alphabetical_sorting : ImageResources.Numerical_sorting;
            ParserHandler.ParseDocumentNow();
            Sci.GrabFocus();
        }

        private void buttonExpandRetract_Click(YamuiButtonImage sender, EventArgs e) {
            if (_isExpanded)
                yamuiList.ForceAllToCollapse();
            else
                yamuiList.ForceAllToExpand();
            _isExpanded = !_isExpanded;
            filterbox.ExtraButtonsList[1].BackGrndImage = _isExpanded ? ImageResources.Collapse : ImageResources.Expand;
            Sci.GrabFocus();
        }

        private void ButtonFromIncludeOnButtonPressed(YamuiButtonImage sender, EventArgs e) {
            // change option and image
            Config.Instance.CodeExplorerDisplayExternalItems = !Config.Instance.CodeExplorerDisplayExternalItems;
            filterbox.ExtraButtonsList[3].UseGreyScale = !Config.Instance.CodeExplorerDisplayExternalItems;
            // Parse the document
            ParserHandler.ParseDocumentNow();
            Sci.GrabFocus();
        }

        #endregion
    }
}