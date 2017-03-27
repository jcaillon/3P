#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (CodeExplorer.cs) is part of 3P.
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
using System.Collections.Generic;
using System.Linq;
using YamuiFramework.Helper;
using _3PA.Lib;
using _3PA.MainFeatures.Parser;
using _3PA.NppCore;
using _3PA.NppCore.NppInterfaceForm;
using _3PA._Resource;

namespace _3PA.MainFeatures.CodeExplorer {
    internal class CodeExplorer : NppDockableDialog<CodeExplorerForm> {
        #region Core

        #region Singleton

        private static CodeExplorer _instance;

        public static CodeExplorer Instance {
            get { return _instance ?? (_instance = new CodeExplorer()); }
            set { _instance = value; }
        }

        #endregion

        #region Constructor

        private CodeExplorer() {
            _dialogDescription = "Code explorer";
            _formDefaultPos = NppTbMsg.CONT_RIGHT;
            _iconImage = ImageResources.CodeExplorerLogo;
        }

        #endregion

        #region Override method

        protected override void InitForm() {
            Form = new CodeExplorerForm(_fakeForm);
        }

        protected override void OnVisibilityChange(bool visible) {
            Config.Instance.CodeExplorerVisible = visible;
            if (visible) {
                if (NotificationsPublisher.PluginIsReady)
                    ParserHandler.ParseDocumentAsap();
            }
        }

        #endregion

        #endregion

        #region Public

        /// <summary>
        /// Use this to redraw the docked form
        /// </summary>
        public void ApplyColorSettings() {
            if (!IsVisible)
                return;
            Form.SafeInvoke(form => {
                form.ShowTreeBranches(Config.Instance.ShowTreeBranches);
                form.Refresh();
            });
        }

        /// <summary>
        /// update the "selected" scope when the user click in scintilla
        /// </summary>
        public void UpdateCurrentScope() {
            if (!IsVisible)
                return;
            UpdateCurrentScope(Npp.CurrentFile.IsProgress ? ParserHandler.GetScopeOfLine(Sci.Line.CurrentLine) : null);
        }

        public void OnStart() {
            if (!IsVisible)
                return;
            Form.SafeInvoke(form => form.Refreshing = true);
        }

        public void OnParseEnd() {
            if (!IsVisible)
                return;
            Form.SafeInvoke(form => form.Refreshing = false);
        }

        /// <summary>
        /// Call this method to update the code explorer tree with the data from the Parser Handler
        /// </summary>
        public void OnParseEndCodeExplorerItems(List<CodeExplorerItem> codeExplorerItems) {
            if (!IsVisible)
                return;
            Form.SafeInvoke(form => form.UpdateTreeData(UpdateTreeData(codeExplorerItems)));
        }

        public void UpdateTreeData() {
            Form.SafeInvoke(form => form.UpdateTreeData(UpdateTreeData(_initialList)));
        }

        public void OnParseEndParserItems(List<ParserError> parserErrors, Dictionary<int, LineInfo> lineInfo, List<ParsedItem> parsedItems) {
            int curLine = Sci.Line.CurrentLine;
            UpdateCurrentScope(Npp.CurrentFile.IsProgress && lineInfo.ContainsKey(curLine) ? lineInfo[curLine].Scope : null);
        }

        /// <summary>
        /// Contains the list of explorer items for the current file, updated by the parser's visitor class
        /// </summary>
        public List<CodeExplorerItem> ParsedExplorerItemsList {
            get { return _initialList.ToList(); }
        }

        #endregion

        #region Private

        private List<CodeExplorerItem> _initialList = new List<CodeExplorerItem>();

        private void UpdateCurrentScope(ParsedScopeItem currentScope) {
            if (currentScope != null) {
                Form.SafeInvoke(form => form.UpdateCurrentScope(currentScope.Name, Utils.GetImageFromStr(currentScope.ScopeType.ToString())));
            } else {
                Form.SafeInvoke(form => form.UpdateCurrentScope(@"Not applicable", ImageResources.NotApplicable));
            }
        }

        private List<CodeExplorerItem> UpdateTreeData(List<CodeExplorerItem> codeExplorerItems) {
            // get the list of items
            _initialList = codeExplorerItems.ToList();

            _initialList.Sort(CodeExplorerSortingClass<CodeExplorerItem>.GetInstance(Config.Instance.CodeExplorerSortingType));

            return _initialList;
        }

        private void SortAndGroupConsecutiveItems() {
            /*
            List<CodeExplorerItem> outList;

            outList = new List<CodeExplorerItem>();

            // apply custom sorting
            var sortedList = _initialList.ToList();
            sortedList.Sort(CodeExplorerSortingClass<CodeExplorerItem>.GetInstance(Config.Instance.CodeExplorerSortingType));

            HashSet<CodeExplorerBranch> foundBranches = new HashSet<CodeExplorerBranch>();

            // for each distinct type of items, create a branch (if the branchType is not a root item like Root or MainBlock)
            CodeExplorerItem currentLvl1Parent = null;
            var iItem = 0;
            while (iItem < sortedList.Count) {
                var item = sortedList[iItem];

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
                    outList.Add(currentLvl1Parent);
                }

                // Add a child item to the current branch
                if (foundBranches.Contains(item.Branch) && currentLvl1Parent != null) {
                    // For each duplicated item (same Icon and same displayText), we create a new branch
                    var iIdentical = iItem + 1;
                    ParseFlag flags = 0;

                    // while we match identical items
                    while (iIdentical < sortedList.Count &&
                           sortedList[iItem].Type == sortedList[iIdentical].Type &&
                           sortedList[iItem].Branch == sortedList[iIdentical].Branch &&
                           sortedList[iItem].DisplayText.EqualsCi(sortedList[iIdentical].DisplayText)) {
                        flags = flags | sortedList[iIdentical].Flags;
                        iIdentical++;
                    }
                    // if we found identical item
                    if (iIdentical > iItem + 1) {
                        // we create a branch for them
                        var currentLvl2Parent = new CodeExplorerItem {
                            DisplayText = sortedList[iItem].DisplayText,
                            Branch = sortedList[iItem].Branch,
                            Type = sortedList[iItem].Type,
                            IsExpanded = false, // by default, the lvl 2 branches are NOT expanded
                            SubText = "x" + (iIdentical - iItem),
                            IsNotBlock = sortedList[iItem].IsNotBlock,
                            Flags = flags,
                            Children = new List<FilteredTypeTreeListItem>()
                        };
                        currentLvl1Parent.Children.Add(currentLvl2Parent);

                        // add child items to the newly created lvl 2 branch
                        for (int i = iItem; i < iIdentical; i++) {
                            currentLvl2Parent.Children.Add(sortedList[i]);
                        }

                        iItem += (iIdentical - iItem);
                        continue;
                    }

                    // single item, add it normally
                    currentLvl1Parent.Children.Add(item);
                } else {
                    // add existing item as a root item
                    outList.Add(item);
                }

                iItem++;
            }
            */
        }

        #endregion
    }
}