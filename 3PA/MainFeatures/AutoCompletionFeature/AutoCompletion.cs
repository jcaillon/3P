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
using System.Text;
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

        #region Events

        public static event Action<List<CompletionItem>> OnUpdateStaticItems;

        #endregion

        #region field

        private static AutoCompletionForm _form;

        /// <summary>
        /// Was the auto completion opened naturally or from the user shortkey?
        /// </summary>
        private static bool _openedFromShortCut;

        /// <summary>
        /// position of the caret when the auto completion was shown
        /// </summary>
        private static int _shownPosition;

        /// <summary>
        /// Line of the caret when the auto completion was opened from the shortcut
        /// </summary>
        private static int _openedFromShortcutLine;

        /// <summary>
        /// Current word being typed by the user
        /// </summary>
        private static string _currentWord;
        
        private static char[] _additionalWordChar;
        private static char[] _childSeparators;


        // contains the list of all static + dynamic items
        private static List<CompletionItem> _savedAllItems = new List<CompletionItem>();

        // contains the list of items that do not come from the parser
        private static List<CompletionItem> _staticItems = new List<CompletionItem>();

        private static object _lock = new object();

        /// <summary>
        /// The enum and fields below allow to know what type of list must be displayed to the user
        /// </summary>
        private enum ActiveTypes {
            Reset,
            All,
            Filtered,
            KeywordObject
        }

        private static ActiveTypes _currentActiveTypes;
        private static bool _needToSetActiveTypes;

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

        private static List<CompletionItem> _currentItems = new List<CompletionItem>();
        private static bool _needToSetItems;

        /// <summary>
        /// List of the current items in the auto completion
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
            DoInLock(() => {
                if (Npp.CurrentFile.IsProgress) {
                    _staticItems = Keywords.Instance.CompletionItems.ToList();
                    _staticItems.AddRange(DataBase.Instance.CompletionItems);
                    _additionalWordChar = new[] { '_', '&', '-' };
                    _childSeparators = new[] { ':' };
                } else {
                    _staticItems = Npp.CurrentFile.Lang.Keywords;
                    _additionalWordChar = Npp.CurrentFile.Lang.AdditionalWordChar;
                }

                // make sure the additional chars list isn't null (and contains at least '_')
                if (_additionalWordChar == null)
                    _additionalWordChar = new[] { '_' };
                if (!_additionalWordChar.Contains('_')) {
                    var list = _additionalWordChar.ToList();
                    list.Add('_');
                    _additionalWordChar = list.ToArray();
                }

                // we do the sorting (by type and then by ranking), doing it now will reduce the time for the next sort()
                _staticItems.Sort(CompletionSortingClass<CompletionItem>.Instance);

                if (OnUpdateStaticItems != null)
                    OnUpdateStaticItems(_staticItems);
            });
        }

        /// <summary>
        /// Method called when the event OnParseEnded triggers, i.e. when we just parsed the document and
        /// need to refresh the auto completion
        /// </summary>
        public static void OnParseEnded(List<CompletionItem> completionItems) {
            if (Monitor.TryEnter(_lock)) {
                try {

                    // init with static items
                    _savedAllItems = _staticItems.ToList();

                    // we add the dynamic items to the list
                    _savedAllItems.AddRange(completionItems);

                    // compute childSeparator if needed
                    var listSep = _childSeparators != null ? new HashSet<char>(_childSeparators) : new HashSet<char>();
                    foreach (var item in _savedAllItems) {
                        if (item.ChildSeparator != null) {
                            var c = (char)item.ChildSeparator;
                            if (!listSep.Contains(c))
                                listSep.Add(c);
                        }
                    }
                    if (listSep.Count > 0)
                        _childSeparators = listSep.ToArray();

                } finally {
                    Monitor.Exit(_lock);
                }
            }

            // update the auto completion (if shown)
            CurrentActiveTypes = ActiveTypes.Reset;
            if (IsVisible)
                UpdateAutocompletion();
        }

        /// <summary>
        /// Updates the CURRENT ITEMS LIST,
        /// handles the opening or the closing of the auto completion form on key input
        /// </summary>
        public static void UpdateAutocompletion(char c = char.MinValue) {
            
            var typing = IsCharPartOfWord(c);
            var isVisible = IsVisible;

            // currently continuing to type a word
            if (typing && isVisible) {
                // the auto completion is already visible, this means the _currentWord is set
                // we only have to filter the current list even more
                ShowSuggestionList(_currentWord + c);
                return;
            }

            var nppCurrentPosition = Sci.CurrentPosition;
            var nppCurrentLine = Sci.Line.CurrentLine;
            var isNormalContext = Style.IsCarretInNormalContext(nppCurrentPosition);
            string strOnLeft = null;

            // we finished entering a word (we typed a char that is not part of a word, a space of new line or separator...)
            if (c != char.MinValue && !typing && isNormalContext) {

                strOnLeft = Sci.GetTextOnLeftOfPos(nppCurrentPosition, 61);
                var strOnLeftLength = strOnLeft.Length;

                // we finished entering a word, find the offset at which we can find said word
                int offset;
                if (c == '\n' || c == '\r') {
                    offset = nppCurrentPosition - Sci.GetLine(nppCurrentLine).Position;
                    if (offset > 40) {
                        // the user inserted a new line which is extremely indented, make sure to get the last word by taking a longer string
                        strOnLeft = Sci.GetTextOnLeftOfPos(nppCurrentPosition, 250);
                        strOnLeftLength = strOnLeft.Length;
                    }
                    offset += strOnLeftLength - offset - 2 >= 0 && strOnLeft.Substring(strOnLeftLength - offset - 2, 2).Equals("\r\n") ? 2 : 1;
                } else
                    offset = 1;

                // automatically insert selected keyword of the completion list?
                if (isVisible && Config.Instance.AutoCompleteInsertSelectedSuggestionOnWordEnd) {

                    // make sure at least 1 char of the word we want to replace is a letter or digit
                    for (int i = strOnLeft.Length - 1 - offset; i >= 0; i--) {
                        if (!IsCharPartOfWord(strOnLeft[i]))
                            break;
                        if (char.IsLetterOrDigit(strOnLeft[i])) {
                            var replacementWord = InsertSuggestion(_form.GetCurrentCompletionItem(), -offset);
                            // also need to update the strOnLeft since we use it again in this method
                            int replacePos = offset;
                            char? replaceSep;
                            var toReplace = GetWord(strOnLeft, ref replacePos, out replaceSep);
                            strOnLeft = (strOnLeftLength - offset - toReplace.Length > 0 ? strOnLeft.Substring(0, strOnLeftLength - offset - toReplace.Length) : "") +
                                replacementWord + 
                                (strOnLeftLength - offset >= 0 ? strOnLeft.Substring(strOnLeftLength - offset) : "");
                            break;
                        }
                    }
                }

                // replace semicolon by a point
                if (c == ';' && Npp.CurrentFile.IsProgress && Config.Instance.CodeReplaceSemicolon)
                    Sci.ModifyTextAroundCaret(-offset, 0, ".");
            }

            // We are here if the auto completion is hidden or if the user is not continuing to type a word, 
            // We check if we need to change the list of items in the auto completion

            if (!_openedFromShortCut) {
                // show autocomp when typing? or not
                if (!Config.Instance.AutoCompleteOnKeyInputShowSuggestions)
                    return;

                // dont show in string/comments..?
                if (!isVisible && !isNormalContext && !Config.Instance.AutoCompleteShowInCommentsAndStrings)
                    return;
            } else {
                // the caret changed line (happens when we trigger the auto comp manually on the first pos of a line
                // and we press backspace, we return to the previous line but the form would still be visible)
                if (isVisible && nppCurrentLine != _openedFromShortcutLine) {
                    Cloak();
                    return;
                }
            }
            
            // get current word
            if (strOnLeft == null)
                strOnLeft = Sci.GetTextOnLeftOfPos(nppCurrentPosition, 61);
            int charPos = 0;
            char? firstSeparator;
            var firstKeyword = GetWord(strOnLeft, ref charPos, out firstSeparator);

            if (firstSeparator == null) {
                // we didn't match a known separator just before the keyword;
                // this means we want to display the entire list of keywords

                if (CurrentActiveTypes != ActiveTypes.All) {
                    CurrentActiveTypes = ActiveTypes.All;
                    DoInLock(() => {
                        CurrentItems = _savedAllItems;
                    });
                }

            } else {
                // return the list that should be used in the auto completion, filtered by the previous keywords
                IEnumerable<CompletionItem> outList = null;
                DoInLock(() => {
                    outList = GetWordsList(_savedAllItems, strOnLeft, charPos, firstSeparator);
                });

                // if the current word is directly preceded by a :, we are entering an object field/method
                // for now, we then display the whole list of object keywords
                if (firstSeparator == ':' && (outList == null || !outList.Any())) {
                    if (CurrentActiveTypes != ActiveTypes.KeywordObject) {
                        CurrentActiveTypes = ActiveTypes.KeywordObject;
                        DoInLock(() => {
                            CurrentItems = _savedAllItems;
                        });
                    }
                    ShowSuggestionList(firstKeyword);
                    return;
                }

                CurrentActiveTypes = ActiveTypes.Filtered;
                DoInLock(() => {
                    CurrentItems = outList.ToList();
                });
                
                // we want to show the list no matter how long the filter keyword
                ShowSuggestionList(firstKeyword);
                return;
            }

            // close if there is nothing to suggest
            if ((!_openedFromShortCut || nppCurrentPosition != _shownPosition) && firstKeyword.Length < Config.Instance.AutoCompleteStartShowingListAfterXChar) {
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
        public static string GetWord(string input, ref int at, out char? separator) {
            separator = null;
            var max = input.Length - 1 - at;
            int pos = max;
            while (pos >= 0) {
                var ch = input[pos];
                if (!IsCharPartOfWord(ch))
                    break;
                pos--;
            }
            if (pos >= 0 && _childSeparators != null && _childSeparators.Contains(input[pos]))
                separator = input[pos];
            var wordLenght = Math.Max(0, max - pos);
            at += wordLenght + 1;
            return wordLenght > 0 ? input.Substring(pos + 1, wordLenght) : string.Empty;
        }

        /// <summary>
        /// Will return a list of items that are the possible inputs for the "fullyQualifiedKeyword"
        /// For instance, if fullyQualifiedKeyword = "FOR db.table.field", this will return the list of all the fields for
        /// the database "db" and the table "table"
        /// It filters the inputList with "db", then takes all the children of "db"
        /// On the children of "db", it filters with "table" then take all the children of "table"
        /// it returns this final list of all the children for db.table
        /// </summary>
        private static IEnumerable<CompletionItem> GetWordsList(List<CompletionItem> inputList, string fullyQualifiedKeyword, int charPos, char? firstSeparator) {
            IEnumerable<CompletionItem> outList = inputList;

            // case of : db.table.field (for instance)
            var keywordStack = new Stack<Tuple<string, char?>>();

            char? latestSeparator = firstSeparator;
            char? separator;
            do {
                var keyword = GetWord(fullyQualifiedKeyword, ref charPos, out separator);
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

        /// <summary>
        /// Yields all the items in the given collection that match exactly the keyword and have the given child separator
        /// </summary>
        private static IEnumerable<CompletionItem> GetFilteredItems(IEnumerable<CompletionItem> collection, string keyword, char? childSeparator) {
            foreach (CompletionItem item in collection)
                if (item.ChildSeparator == childSeparator && item.DisplayText.EqualsCi(keyword)) {
                    yield return item;
                }
        }

        /// <summary>
        /// Yields all the childs of a list of item
        /// </summary>
        private static IEnumerable<CompletionItem> GetAllChildrenItems(IEnumerable<CompletionItem> collection) {
            foreach (CompletionItem item in collection)
                foreach (var completionItem in item.Children)
                    yield return completionItem;
        }

        /// <summary>
        /// This function handles the display of the auto complete form, create or update it
        /// </summary>
        private static void ShowSuggestionList(string filter) {
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
            _currentWord = filter;
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
                DoInLock(() => {
                    form.SetItems(CurrentItems.Cast<ListItem>().ToList());
                });
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
            int nppCurrentLine = Sci.Line.CurrentLine;
            CompletionFilterClass.Instance.UpdateConditions(nppCurrentLine, false);

            // filter with keyword (keyword can be empty)
            form.SetFilterString(_currentWord);

            // close if the list ends up empty after the filter
            if (!_openedFromShortCut && Config.Instance.AutoCompleteOnKeyInputHideIfEmpty && form.GetNbItems() == 0) {
                Cloak();
                return;
            }

            // if the form was already visible, don't go further
            if (IsVisible)
                return;

            // update form position
            var lineHeight = Sci.TextHeight(nppCurrentLine);
            var point = Sci.GetCaretScreenLocation();
            point.Y += lineHeight;

            form.SetPosition(point, lineHeight + 2);
            form.UnCloak();
            form.SetSelectedIndex(0);

            _shownPosition = Sci.CurrentPosition;
        }

        /// <summary>
        /// Execute the action behind the lock
        /// </summary>
        private static void DoInLock(Action toDo) {
            if (Monitor.TryEnter(_lock)) {
                try {
                    toDo();
                } finally {
                    Monitor.Exit(_lock);
                }
            }
        }

        #region Form events

        /// <summary>
        /// Method called by the form when the user accepts a suggestion (tab or enter or double-click)
        /// </summary>
        private static void OnInsertSuggestion(CompletionItem data) {
            InsertSuggestion(data);
        }

        /// <summary>
        /// When the user resizes the autocompletion, we need to hide the tooltip
        /// </summary>
        private static void OnResizeBegin(object sender, EventArgs eventArgs) {
            if (InfoToolTip.InfoToolTip.IsVisible)
                InfoToolTip.InfoToolTip.Cloak();
        }

        #endregion

        #region Insert suggestion

        /// <summary>
        /// Call this method to insert the completionItem at the given offset in regards to the current caret position
        /// (it will replace the word found at CaretPosition + Offset by the completionItem.DisplayText)
        /// </summary>
        public static string InsertSuggestion(CompletionItem data, int offset = 0) {
            string replacementText = null;
            try {
                if (data != null) {

                    // in case of keyword, replace abbreviation if needed
                    replacementText = data.DisplayText;
                    if (Config.Instance.CodeReplaceAbbreviations && (data.Flags & ParseFlag.Abbreviation) != 0) {
                        var fullKeyword = Keywords.Instance.GetFullKeyword(data.DisplayText).ConvertCase(Config.Instance.KeywordChangeCaseMode);
                        replacementText = fullKeyword ?? data.DisplayText;
                    }

                    Sci.ReplaceWordWrapped(replacementText, _additionalWordChar, offset);

                    // Remember this item to show it higher in the list later
                    RememberUseOf(data);

                    if (data is SnippetCompletionItem)
                        Snippets.TriggerCodeSnippetInsertion();

                    Cloak();
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error during InsertSuggestion");
            }
            return replacementText;
        }

        #endregion

        #region handling item ranking

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

        #endregion

        #region public misc

        /// <summary>
        /// Called from CTRL + Space shortcut
        /// </summary>
        public static void OnShowCompleteSuggestionList() {
            ParserHandler.ParseDocumentAsap();
            _openedFromShortCut = true;
            _openedFromShortcutLine = Sci.Line.CurrentLine;
            _shownPosition = Sci.CurrentPosition;
            UpdateAutocompletion();
        }

        /// <summary>
        /// Find a list of items in the completion and return it
        /// Uses the position to filter the list the same way the auto completion form would
        /// </summary>
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

        private static List<CompletionItem> GetSortedFilteredSavedList(int lineNumber, bool dontCheckLine) {
            var filterClass = new CompletionFilterClass();
            filterClass.UpdateConditions(lineNumber, dontCheckLine);
            List<CompletionItem> outList = null;
            DoInLock(() => {
                outList = _savedAllItems.Where(filterClass.FilterPredicate).ToList();
                outList.Sort(CompletionSortingClass<CompletionItem>.Instance);
            });
            return outList;
        }

        public static char[] CurrentLangAdditionalChars {
            get { return _additionalWordChar; }
        }

        public static char[] CurrentLangAllChars {
            get {
                if (_childSeparators == null)
                    return _additionalWordChar;
                var outList = _additionalWordChar.ToList();
                outList.AddRange(_childSeparators.ToList());
                return outList.ToArray();
            }
        }

        #endregion
    }
}