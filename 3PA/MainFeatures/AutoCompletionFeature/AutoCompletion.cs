#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (AutoCompletion.cs) is part of 3P.
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
using System.Threading;
using System.Windows.Forms;
using YamuiFramework.Controls.YamuiList;
using YamuiFramework.Helper;
using _3PA.Lib;
using _3PA.MainFeatures.Parser;
using _3PA.NppCore;
using _3PA.WindowsCore;

namespace _3PA.MainFeatures.AutoCompletionFeature {
    /// <summary>
    /// This class handles the AutoCompletionForm
    /// </summary>
    internal static class AutoCompletion {
        #region field

        /// <summary>
        /// Was the auto completion opened naturally or from the user shortkey?
        /// </summary>
        private static bool _openedFromShortCut;

        /// <summary>
        /// position of the caret when the auto completion was opened (from shortcut)
        /// </summary>
        private static int _shownPosition;

        private static int _shownLine;

        private static AutoCompletionForm _form;

        private static bool _needToSetItems;
        private static bool _needToSetActiveTypes;

        private static ActiveTypes _currentActiveTypes;

        private static char[] _additionalWordChar;
        private static char[] _childSeparators;

        // contains the list of items currently display in the form
        private static List<CompletionItem> _currentItems = new List<CompletionItem>();

        // contains the whole list of items to show
        private static List<CompletionItem> _savedAllItems = new List<CompletionItem>();

        // contains the list of items that do not come from the parser
        private static List<CompletionItem> _staticItems = new List<CompletionItem>();

        private static ReaderWriterLockSlim _itemsListLock = new ReaderWriterLockSlim();

        /// <summary>
        /// The enum and fields below allow to know what type of list must be displayed to the user
        /// </summary>
        private enum ActiveTypes {
            Reset,
            All,
            Filtered,
            KeywordObject
        }

        /// <summary>
        /// stores the current value of the type of list displayed
        /// </summary>
        private static ActiveTypes CurrentActiveTypes {
            get { return _currentActiveTypes; }
            set {
                _needToSetActiveTypes = _currentActiveTypes != value;
                _currentActiveTypes = value;
            }
        }

        /// <summary>
        /// List of the current items in the auto completion (thread safe)
        /// </summary>
        private static List<CompletionItem> CurrentItems {
            get { return _currentItems; }
            set {
                _currentItems = value;
                _currentItems.Sort(CompletionSortingClass<CompletionItem>.Instance);
                _needToSetItems = true;
            }
        }

        #endregion

        #region core mechanism

        /// <summary>
        /// Called to refresh the current list of static items (i.e. items not parsed)
        /// </summary>
        public static void SetStaticItems() {
            if (Npp.CurrentFile.IsProgress) {
                _staticItems = Keywords.Instance.CompletionItems.ToList();
                _staticItems.AddRange(DataBase.CompletionItems);
                _additionalWordChar = new[] {'-', '_', '&'};
                _childSeparators = new[] {'.', ':'};
            } else {
                _staticItems = Npp.CurrentFile.Lang.Keywords;
                _additionalWordChar = Npp.CurrentFile.Lang.AdditionalWordChar;
            }

            // we do the sorting (by type and then by ranking), doing it now will reduce the time for the next sort()
            _staticItems.Sort(CompletionSortingClass<CompletionItem>.Instance);
        }

        /// <summary>
        /// Method called when the event OnParseEnded triggers, i.e. when we just parsed the document and
        /// need to refresh the auto completion
        /// </summary>
        public static void OnParseEnded(List<CompletionItem> completionItems) {
            // init with static items
            _savedAllItems = _staticItems.ToList();

            // we add the dynamic items to the list
            _savedAllItems.AddRange(completionItems);

            // update the auto completion (if shown)
            CurrentActiveTypes = ActiveTypes.Reset;
            if (IsVisible)
                UpdateAutocompletion(true);
        }

        /// <summary>
        /// Updates the CURRENT ITEMS LIST,
        /// handles the opening or the closing of the auto completion form on key input, 
        /// it is only called when the user adds or delete a char
        /// 
        /// canChangeListType : if true, it means the auto completion is already visible, we already have loaded
        /// the list of completionItem needed and the user is typing a word, increasing the filter
        /// </summary>
        public static void UpdateAutocompletion(bool canChangeListType) {
            // show autocomp when typing? or not
            if (!Config.Instance.AutoCompleteOnKeyInputShowSuggestions && !_openedFromShortCut)
                return;

            var nppCurrentPosition = Sci.CurrentPosition;
            var nppCurrentLine = Sci.Line.CurrentLine;
            var isVisible = IsVisible;

            // dont show in string/comments..?
            if (!_openedFromShortCut && !isVisible && !Config.Instance.AutoCompleteShowInCommentsAndStrings && !Style.IsCarretInNormalContext(nppCurrentPosition))
                return;

            // the caret changed line
            if (isVisible && nppCurrentLine != _shownLine) {
                Cloak();
                return;
            }

            // get current word
            var strOnLeft = Sci.GetTextOnLeftOfPos(nppCurrentPosition, 61);
            int charPos = 0;
            char? firstSeparator;
            var firstKeyword = GetWord(strOnLeft, ref charPos, out firstSeparator);

            // if the auto completion is hidden or if the user is not continuing to type a word, we might want to 
            // change the list of items in the auto completion
            if (!isVisible || canChangeListType) {
                if (firstSeparator == null) {
                    // we didn't match a known separator just before the keyword;
                    // this means we want to display the entire list of keywords

                    if (CurrentActiveTypes != ActiveTypes.All) {
                        CurrentActiveTypes = ActiveTypes.All;
                        CurrentItems = _savedAllItems;
                    }
                } else {
                    //
                    var outList = GetWordsList(_savedAllItems.ToList(), strOnLeft, charPos, firstSeparator);

                    // if the current word is directly preceded by a :, we are entering an object field/method
                    // for now, we then display the whole list of object keywords
                    if (firstSeparator == ':' && outList == null && !outList.Any()) {
                        if (CurrentActiveTypes != ActiveTypes.KeywordObject) {
                            CurrentActiveTypes = ActiveTypes.KeywordObject;
                            CurrentItems = _savedAllItems;
                        }
                        ShowSuggestionList(firstKeyword);
                        return;
                    }

                    CurrentItems = outList.ToList();
                    CurrentActiveTypes = ActiveTypes.Filtered;

                    // we want to show the list no matter how long the filter keyword
                    ShowSuggestionList(firstKeyword);
                    return;
                }
            }

            // close if there is nothing to suggest
            if ((!_openedFromShortCut || nppCurrentPosition != _shownPosition) && (firstKeyword.Length < Config.Instance.AutoCompleteStartShowingListAfterXChar)) {
                Cloak();
                return;
            }

            ShowSuggestionList(firstKeyword);
        }

        /// <summary>
        /// is the char part of a word in the current lang
        /// </summary>
        public static bool IsCharPartOfWord(char c) {
            return char.IsLetterOrDigit(c) || _additionalWordChar.Contains(c);
        }

        /// <summary>
        /// Reads a a word, either starting from the end (readRightToLeft = true) of the start of the input string
        /// </summary>
        public static string GetQualifiedWord(string input, bool readRightToLeft = true) {
            int pos = 0;
            char? sep;
            var output = GetWord(input, ref pos, out sep, readRightToLeft);
            while (sep != null) {
                if (readRightToLeft) {
                    output = sep + output;
                    output = GetWord(input, ref pos, out sep) + output;
                } else {
                    output += sep;
                    output += GetWord(input, ref pos, out sep, false);
                }
            }
            return output;
        }

        /// <summary>
        /// Reads a a word, either starting from the end (readRightToLeft = true) of the start of the input string
        /// </summary>
        public static string GetWord(string input, bool readRightToLeft = true) {
            int pos = 0;
            char? sep;
            return GetWord(input, ref pos, out sep, readRightToLeft);
        }

        /// <summary>
        /// Read a word from right to left or reverse, stops at the first non word character and 
        /// returns said char if it's a child separator
        /// 
        /// Usage example :
        /// GetKeyword("db.table.field", ref 0, ?)
        /// -> "field" = GetKeyword("db:table.field", ref 6, '.')
        /// GetKeyword("db.table.field", ref 6, ?)
        /// -> "table" = GetKeyword("db:table.field", ref 12, ':')
        /// GetKeyword("db.table.field", ref 12, ?)
        /// -> "db" = GetKeyword("db", ref 14, ?)
        /// GetKeyword(" word1 word2 ", ref 1, ?)
        /// -> "db" = GetKeyword("word2", ref 6, ?)
        /// </summary>
        public static string GetWord(string input, ref int at, out char? separator, bool readRightToLeft = true) {
            var lght = input.Length - 1;
            int wordLenght = 0;
            int pos = 0;
            while (wordLenght <= lght - at) {
                pos = readRightToLeft ? lght - wordLenght - at : wordLenght + at;
                var ch = input[pos];
                // normal word
                if (IsCharPartOfWord(ch))
                    wordLenght++;
                else
                    break;
            }
            separator = null;
            if (_childSeparators.Contains(input[pos]))
                separator = input[pos];
            string outStr = wordLenght == 0 ? string.Empty : input.Substring(readRightToLeft ? input.Length - wordLenght - at : at, wordLenght);
            at += wordLenght + 1;
            return outStr;
        }

        private static IEnumerable<CompletionItem> GetWordsList(List<CompletionItem> inputList, string strOnLeft, int charPos, char? firstSeparator) {
            IEnumerable<CompletionItem> outList = inputList;

            // case of : db.table.field (for instance)
            var keywordStack = new Stack<Tuple<string, char?>>();

            char? latestSeparator = firstSeparator;
            char? separator;
            do {
                var keyword = GetWord(strOnLeft, ref charPos, out separator);
                keywordStack.Push(new Tuple<string, char?>(keyword, latestSeparator));
                latestSeparator = separator;
            } while (separator != null);

            // at this point we have this stack :
            // db .
            // table .

            while (keywordStack.Count > 0) {
                var currentTuple = keywordStack.Pop();
                // filter the whole list to only keep the items matching "db" and "." (then "table" and ".")
                outList = GetFilteredItems(outList, currentTuple.Item1, currentTuple.Item2);
                // now make a new list formed of all the children of the filtered list above (ie children of db then children of table)
                outList = GetAllChildrenItems(outList);
            }

            return outList;
        }

        private static IEnumerable<CompletionItem> GetFilteredItems(IEnumerable<CompletionItem> collection, string keyword, char? chr) {
            foreach (CompletionItem item in collection)
                if (item.ChildSeparator == chr && item.DisplayText.EqualsCi(keyword)) {
                    yield return item;
                }
        }

        private static IEnumerable<CompletionItem> GetAllChildrenItems(IEnumerable<CompletionItem> collection) {
            foreach (CompletionItem item in collection)
                foreach (var completionItem in item.Children)
                    yield return completionItem;
        }

        /// <summary>
        /// This function handles the display of the auto complete form, create or update it
        /// </summary>
        private static void ShowSuggestionList(string keyword) {
            // instantiate the form if needed
            if (_form == null) {
                _form = new AutoCompletionForm {
                    UnfocusedOpacity = Config.Instance.AutoCompleteUnfocusedOpacity,
                    FocusedOpacity = Config.Instance.AutoCompleteFocusedOpacity
                };
                _form.InsertSuggestion += OnInsertSuggestion;
                _form.ResizeBegin += OnResizeBegin;
                _needToSetItems = true;
                _form.Show(Npp.Win32Handle);
                Sci.GrabFocus();
            }

            // If this method has been invoked by the RefreshDynamicItems methods, we are on a different thread than
            // the thread used to create the form
            _form.Keyword = keyword;
            if (_form.InvokeRequired)
                _form.SafeSyncInvoke(ShowSuggestionList);
            else
                ShowSuggestionList(_form);
        }

        /// <summary>
        /// This function handles the display of the autocomplete form, create or update it
        /// </summary>
        private static void ShowSuggestionList(AutoCompletionForm form) {
            // we changed the list of items to display
            if (_needToSetItems) {
                form.SetItems(CurrentItems.Cast<ListItem>().ToList());
                _needToSetItems = false;
            }

            // only activate certain types
            if (_needToSetActiveTypes) {
                switch (CurrentActiveTypes) {
                    case ActiveTypes.All:
                        form.SetUnactiveType(new List<int> {
                            (int) CompletionType.KeywordObject
                        });
                        break;
                    case ActiveTypes.KeywordObject:
                        form.SetActiveType(new List<int> {
                            (int) CompletionType.KeywordObject
                        });
                        break;
                    default:
                        form.SetUnactiveType(null);
                        break;
                }
            }

            // the filter uses the current caret line to know which item should be filtered, set it here
            var nppCurrentLine = Sci.Line.CurrentLine;
            CompletionFilterClass.Instance.UpdateConditions(nppCurrentLine, false);

            // filter with keyword (keyword can be empty)
            form.SetFilterString();

            // close?
            if (!_openedFromShortCut && Config.Instance.AutoCompleteOnKeyInputHideIfEmpty && form.GetNbItems() == 0) {
                form.Cloak();
                return;
            }

            // if the form was already visible, don't go further
            if (IsVisible)
                return;

            _shownLine = nppCurrentLine;
            _shownPosition = Sci.CurrentPosition;

            // update position
            var lineHeight = Sci.TextHeight(nppCurrentLine);
            var point = Sci.GetCaretScreenLocation();
            point.Y += lineHeight;

            form.SetPosition(point, lineHeight + 2);
            form.UnCloak();
            form.SetSelectedIndex(0);
        }

        /// <summary>
        /// Method called by the form when the user accepts a suggestion (tab or enter or double-click)
        /// </summary>
        private static void OnInsertSuggestion(CompletionItem data) {
            InsertSuggestion(data);
        }

        /// <summary>
        /// Call this method to insert the completionItem at the given offset in regards to the current caret position
        /// (it will replace the word found at CaretPosition + Offset by the completionItem.DisplayText)
        /// </summary>
        public static void InsertSuggestion(CompletionItem data, int offset = 0) {
            try {
                if (data == null)
                    return;

                // in case of keyword, replace abbreviation if needed
                var replacementText = data.DisplayText;
                if (Config.Instance.CodeReplaceAbbreviations && (data.Flags & ParseFlag.Abbreviation) != 0) {
                    var fullKeyword = Keywords.Instance.GetFullKeyword(data.DisplayText).ConvertCase(Config.Instance.KeywordChangeCaseMode);
                    replacementText = fullKeyword ?? data.DisplayText;
                }

                Sci.ReplaceKeywordWrapped(replacementText, offset);

                // Remember this item to show it higher in the list later
                RememberUseOf(data);

                if (data.Type == CompletionType.Snippet)
                    Snippets.TriggerCodeSnippetInsertion();

                Cloak();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error during InsertSuggestion");
            }
        }

        /// <summary>
        /// Increase ranking of a given CompletionItem
        /// </summary>
        /// <param name="item"></param>
        private static void RememberUseOf(CompletionItem item) {
            // handles unwanted rank progression (when the user enter several times the same keyword)
            if (item.DisplayText.Equals(_lastRememberedKeyword))
                return;
            _lastRememberedKeyword = item.DisplayText;

            item.Ranking++;
            _form.SafeInvoke(form => form.SortInitialList()); // sort the list of items since the ranking has changed

            if (item.FromParser)
                RememberUseOfParsedItem(item.DisplayText);
        }

        /// <summary>
        /// When the user resizes the autocompletion, we need to hide the tooltip
        /// </summary>
        private static void OnResizeBegin(object sender, EventArgs eventArgs) {
            if (InfoToolTip.InfoToolTip.IsVisible)
                InfoToolTip.InfoToolTip.Cloak();
        }

        #endregion

        #region handling item ranking

        /// <summary>
        /// This dictionary is what is used to remember the ranking of each word for the current session
        /// (otherwise this info is lost since we clear the ParsedItemsList each time we parse!)
        /// </summary>
        private static Dictionary<string, int> _displayTextRankingParsedItems = new Dictionary<string, int>();

        private static string _lastRememberedKeyword = "";

        /// <summary>
        /// Find ranking of a parsed item
        /// </summary>
        /// <param name="displayText"></param>
        /// <returns></returns>
        public static int FindRankingOfParsedItem(string displayText) {
            return _displayTextRankingParsedItems.ContainsKey(displayText) ? _displayTextRankingParsedItems[displayText] : 0;
        }

        /// <summary>
        /// remember the use of a particular item in the completion list
        /// (for dynamic items = parsed items)
        /// </summary>
        /// <param name="displayText"></param>
        public static void RememberUseOfParsedItem(string displayText) {
            if (!_displayTextRankingParsedItems.ContainsKey(displayText))
                _displayTextRankingParsedItems.Add(displayText, 1);
            else
                _displayTextRankingParsedItems[displayText]++;
        }

        #endregion

        #region _form handler

        /// <summary>
        /// Is the form currently visible?
        /// </summary>
        public static bool IsVisible {
            get { return !(_form == null || !_form.IsVisible); }
        }

        /// <summary>
        /// Closes the form
        /// </summary>
        public static void Cloak() {
            try {
                if (IsVisible) {
                    _form.SafeSyncInvoke(form => form.Cloak());
                }
                _openedFromShortCut = false;

                // closing the autocompletion form also closes the tooltip
                InfoToolTip.InfoToolTip.CloseIfOpenedForCompletion();
            } catch (Exception e) {
                ErrorHandler.LogError(e);
            }
        }

        /// <summary>
        /// Use this to redraw the docked form
        /// </summary>
        public static void ApplyColorSettings() {
            if (!IsVisible)
                return;
            _form.SafeInvoke(form => form.Refresh());
        }

        /// <summary>
        /// Forces the form to close, only when leaving npp
        /// </summary>
        public static void ForceClose() {
            try {
                if (_form != null) {
                    _form.InsertSuggestion -= OnInsertSuggestion;
                    _form.ResizeBegin -= OnResizeBegin;
                    _form.ForceClose();
                }
                _form = null;
            } catch (Exception e) {
                ErrorHandler.LogError(e);
            }
        }

        /// <summary>
        /// Passes the OnKey input of the CharAdded or w/e event to the auto completion form
        /// </summary>
        public static bool PerformKeyDown(KeyEventArgs e) {
            return IsVisible && (bool) _form.SafeSyncInvoke(form => form.PerformKeyDown(e));
        }

        /// <summary>
        /// Returns true if the cursor is within the form window
        /// </summary>
        public static bool IsMouseIn() {
            return Win32Api.IsCursorIn(_form.Handle);
        }

        #endregion

        #region public misc

        /// <summary>
        /// Called from CTRL + Space shortcut
        /// </summary>
        public static void OnShowCompleteSuggestionList() {
            ParserHandler.ParseDocumentAsap();
            _openedFromShortCut = true;
            _shownLine = Sci.Line.CurrentLine;
            _shownPosition = Sci.CurrentPosition;
            UpdateAutocompletion(true);
        }

        /// <summary>
        /// Find a list of items in the completion and return it
        /// Uses the position to filter the list the same way the autocompletion form would
        /// </summary>
        /// <returns></returns>
        public static List<CompletionItem> FindInCompletionData(string keyword, int position, bool dontCheckLine = false) {
            var filteredList = GetSortedFilteredSavedList(Sci.LineFromPosition(position), dontCheckLine);
            if (filteredList == null || filteredList.Count <= 0)
                return null;

            int charPos = 0;
            char? firstSeparator;
            var firstKeyword = GetWord(keyword, ref charPos, out firstSeparator);
            if (firstSeparator != null) {
                filteredList = GetWordsList(filteredList, keyword, charPos, firstSeparator).ToList();
            }

            return filteredList.Where(data => data.DisplayText.EqualsCi(firstKeyword)).ToList();
        }

        /// <summary>
        /// Returns a list of "parameters" for a given internal procedure
        /// </summary>
        public static List<CompletionItem> FindProcedureParameters(CompletionItem procedureItem) {
            return _savedAllItems.Where(data => {
                if (!data.FromParser)
                    return false;
                var item = data.ParsedBaseItem as ParsedDefine;
                return item != null && item.Scope.Name.EqualsCi(procedureItem.DisplayText) && item.Type == ParseDefineType.Parameter && (data.Type == CompletionType.VariablePrimitive || data.Type == CompletionType.VariableComplex || data.Type == CompletionType.Widget);
            }).ToList();
        }

        private static List<CompletionItem> GetSortedFilteredSavedList(int lineNumber, bool dontCheckLine) {
            var filterClass = new CompletionFilterClass();
            filterClass.UpdateConditions(lineNumber, dontCheckLine);
            var outList = _savedAllItems.Where(filterClass.FilterPredicate).ToList();
            outList.Sort(CompletionSortingClass<CompletionItem>.Instance);
            return outList;
        }

        /// <summary>
        /// Replace the keywork at given offset with the current suggestion
        /// </summary>
        public static void UseCurrentSuggestion(int offset) {
            if (IsVisible)
                InsertSuggestion(_form.GetCurrentCompletionItem(), offset);
        }

        #endregion
    }
}