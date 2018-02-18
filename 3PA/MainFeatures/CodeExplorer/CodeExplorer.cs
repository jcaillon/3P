#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
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
using YamuiFramework.Controls.YamuiList;
using YamuiFramework.Helper;
using _3PA.Lib;
using _3PA.MainFeatures.Parser.Pro;
using _3PA.MainFeatures.Parser.Pro.Parse;
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
            _formDefaultPos = NppTbMsg.DWS_DF_CONT_RIGHT;
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
            UpdateCurrentScope(Npp.CurrentFileInfo.IsProgress ? ParserHandler.GetScopeOfLine<ParsedScopeSection>(Sci.Line.CurrentLine) : null);
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
        /// Called when the parser ends
        /// </summary>
        public void OnParseEndCodeExplorerItems(List<CodeItem> codeExplorerItems) {
            if (!IsVisible)
                return;
            var list = codeExplorerItems != null ? SortAndGroupConsecutiveItems(codeExplorerItems.ToList()) : new List<CodeItem>();
            Form.SafeInvoke(form => form.UpdateTreeData(list));
        }

        /// <summary>
        /// Called when the parser ends
        /// </summary>
        public void OnParseEndParserItems(List<ParserError> parserErrors, Dictionary<int, ParsedLineInfo> lineInfo, List<ParsedItem> parsedItems) {
            if (!IsVisible)
                return;
            int curLine = Sci.Line.CurrentLine;
            UpdateCurrentScope(Npp.CurrentFileInfo.IsProgress && lineInfo != null && lineInfo.ContainsKey(curLine) ? lineInfo[curLine].GetTopMostBlock<ParsedScopeSection>() : null);
        }

        #endregion

        #region Private

        /// <summary>
        /// Update the current scope in the form
        /// </summary>
        private void UpdateCurrentScope(ParsedScopeSection currentScope) {
            if (currentScope != null) {
                var preprocScope = currentScope as ParsedScopePreProcBlock;
                Form.SafeInvoke(form => form.UpdateCurrentScope(currentScope.Name, Utils.GetImageFromStr(preprocScope != null ? preprocScope.Type.ToString() :  currentScope.ScopeType.ToString())));
            } else {
                Form.SafeInvoke(form => form.UpdateCurrentScope(@"Not applicable", ImageResources.NotApplicable));
            }
        }

        /// <summary>
        /// Sort the given list and group similar consecutive items into sub groups
        /// </summary>
        private List<CodeItem> SortAndGroupConsecutiveItems(List<CodeItem> list) {
            var outList = new List<CodeItem>();

            // apply custom sorting
            list.Sort(CodeExplorerSortingClass<CodeItem>.GetInstance(Config.Instance.CodeExplorerSortingType));

            var iItem = 0;
            while (iItem < list.Count) {
                var item = list[iItem];

                if (item.Children != null) {
                    item.Children = SortAndGroupConsecutiveItems(item.Children.Cast<CodeItem>().ToList()).Cast<FilteredTypeTreeListItem>().ToList();
                }

                // For each duplicated item (same Icon and same displayText), we create a new branch
                var iIdentical = iItem + 1;
                ParseFlag flags = 0;

                // while we match identical items
                while (iIdentical < list.Count &&
                       list[iItem].Type == list[iIdentical].Type &&
                       list[iItem].DisplayText.EqualsCi(list[iIdentical].DisplayText)) {
                    flags = flags | list[iIdentical].Flags;
                    iIdentical++;
                }
                // if we found identical item
                if (iIdentical > iItem + 1) {
                    // we create a branch for them
                    var group = new BranchCodeItem {
                        DisplayText = list[iItem].DisplayText,
                        Type = list[iItem].Type,
                        IsExpanded = false, // by default, the group are NOT expanded
                        SubText = "x" + (iIdentical - iItem),
                        Flags = flags,
                        Children = new List<FilteredTypeTreeListItem>()
                    };
                    outList.Add(group);

                    // add child items to the newly created group branch
                    for (int i = iItem; i < iIdentical; i++) {
                        group.Children.Add(list[i]);
                    }

                    iItem += (iIdentical - iItem);
                    continue;
                }

                // single item, add it normally
                outList.Add(item);

                iItem++;
            }

            return outList;
        }

        #endregion
    }
}