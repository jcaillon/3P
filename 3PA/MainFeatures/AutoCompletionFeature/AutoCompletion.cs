#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (AutoComplete.cs) is part of 3P.
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
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.AutoCompletionFeature {

    /// <summary>
    /// This class handles the AutoCompletionForm
    /// </summary>
    internal static class AutoCompletion {

        #region events

        /// <summary>
        /// published when the list of static items (keywords, database info, snippets) is updated
        /// </summary>
        public static event Action OnUpdatedStaticItems;

        #endregion

        #region field

        /// <summary>
        /// Was the autocompletion opened naturally or from the user shortkey?
        /// </summary>
        private static bool _openedFromShortCut;

        /// <summary>
        /// position of the carret when the autocompletion was opened (from shortcut)
        /// </summary>
        private static int _shownPosition;

        private static int _shownLine;

        private static AutoCompletionForm _form;
        
        private static bool _needToSetItems;
        private static bool _needToSetActiveTypes;

        // contains the list of items currently display in the form
        private static List<CompletionItem> _currentItems = new List<CompletionItem>();

        // contains the whole list of items (minus the fields) to show
        private static List<CompletionItem> _savedAllItems = new List<CompletionItem>();

        // contains the list of items that do not depend on the current file (keywords, database, snippets)
        private static List<CompletionItem> _staticItems = new List<CompletionItem>();

        /// <summary>
        /// This dictionnary is what is used to remember the ranking of each word for the current session
        /// (otherwise this info is lost since we clear the ParsedItemsList each time we parse!)
        /// </summary>
        private static Dictionary<string, int> _displayTextRankingParsedItems = new Dictionary<string, int>();

        /// <summary>
        /// Same as above but for static stuff (= database)
        /// (it is especially useful for fields because we recreate the list each time!
        /// otherwise it is not that useful indeed)
        /// </summary>
        private static Dictionary<string, int> _displayTextRankingDatabase = new Dictionary<string, int>();

        private static ReaderWriterLockSlim _itemsListLock = new ReaderWriterLockSlim();

        private static string _lastRememberedKeyword = "";

        private static bool _initialized;


        /// <summary>
        /// The enum and fields below allow to know what type of list must be displayed to the user
        /// </summary>
        private enum TypeOfList {
            Reset,
            Complete,
            Fields,
            Tables,
            KeywordObject
        }

        /// <summary>
        /// stores the current value of the type of list displayed
        /// </summary>
        private static TypeOfList CurrentTypeOfList
        {
            get { return _currentTypeOfList; }
            set
            {
                _needToSetActiveTypes = _currentTypeOfList != value;
                _currentTypeOfList = value;
            }
        }
        private static TypeOfList _currentTypeOfList;

        #endregion

        #region public accessors (locked)

        /// <summary>
        /// List of the current items in the autocompletion (thread safe)
        /// </summary>
        public static List<CompletionItem> CurrentItems {
            get {
                if (_itemsListLock.TryEnterReadLock(-1)) {
                    try {
                        return _currentItems;
                    } finally {
                        _itemsListLock.ExitReadLock();
                    }
                }
                return new List<CompletionItem>();
            }
            private set {
                if (_itemsListLock.TryEnterWriteLock(-1)) {
                    try {
                        _currentItems = value;
                        _currentItems.Sort(CompletionSortingClass<CompletionItem>.Instance);
                        _needToSetItems = true;
                    } finally {
                        _itemsListLock.ExitWriteLock();
                    }
                }
            }
        }

        /// <summary>
        /// List of all the items (minus fields) (thread safe)
        /// </summary>
        public static List<CompletionItem> SavedAllItems {
            get {
                if (_itemsListLock.TryEnterReadLock(-1)) {
                    try {
                        return _savedAllItems;
                    } finally {
                        _itemsListLock.ExitReadLock();
                    }
                }
                return new List<CompletionItem>();
            }
        }

        /// <summary>
        /// List of static items (thread safe)
        /// </summary>
        public static List<CompletionItem> StaticItems {
            get {
                if (_itemsListLock.TryEnterReadLock(-1)) {
                    try {
                        return _staticItems;
                    } finally {
                        _itemsListLock.ExitReadLock();
                    }
                }
                return new List<CompletionItem>();
            }
        }

        #endregion

        #region public misc

        /// <summary>
        /// Called from CTRL + Space shortcut
        /// </summary>
        public static void OnShowCompleteSuggestionList() {
            ParserHandler.ParseCurrentDocument();
            _openedFromShortCut = true;
            _shownLine = Npp.Line.CurrentLine;
            _shownPosition = Npp.CurrentPosition;
            UpdateAutocompletion(true);
        }
        
        /// <summary>
        /// try to match the keyword with an item in the autocomplete list
        /// </summary>
        /// <returns></returns>
        public static CompletionItem FindInSavedItems(string keyword, int line) {
            var filteredList = GetSortedFilteredSavedList(line, false);
            if (filteredList == null || filteredList.Count <= 0) return null;
            CompletionItem found = filteredList.FirstOrDefault(data => data.DisplayText.EqualsCi(keyword));
            return found;
        }

        /// <summary>
        /// Find a list of items in the completion and return it
        /// Uses the position to filter the list the same way the autocompletion form would
        /// </summary>
        /// <returns></returns>
        public static List<CompletionItem> FindInCompletionData(string keyword, int position, bool dontCheckLine = false) {
            var filteredList = GetSortedFilteredSavedList(Npp.LineFromPosition(position), dontCheckLine);
            if (filteredList == null || filteredList.Count <= 0) return null;
            var found = filteredList.Where(data => data.DisplayText.EqualsCi(keyword)).ToList();
            if (found.Count > 0)
            return found;

            // search in tables fields
            var tableFound = ParserHandler.FindAnyTableOrBufferByName(Npp.GetFirstWordRightAfterPoint(position));
            if (tableFound == null) return null;

            var listOfFields = DataBase.GetFieldsList(tableFound).ToList();
            return listOfFields.Where(data => data.DisplayText.EqualsCi(keyword)).ToList();
        }

        public static List<CompletionItem> GetSortedFilteredSavedList(int lineNumber, bool dontCheckLine) {
            var filterClass = new CompletionFilterClass();
            filterClass.UpdateConditions(lineNumber, dontCheckLine);
            var outList = SavedAllItems.Where(filterClass.FilterPredicate).ToList();
            outList.Sort(CompletionSortingClass<CompletionItem>.Instance);
            return outList;
        }

        /// <summary>
        /// Replace the keywork at given offset with the current suggestion
        /// </summary>
        public static void UseCurrentSuggestion(int offset) {
            InsertSuggestion(_form.YamuiList.SelectedItem as CompletionItem, offset);
        }

        #endregion

        #region core mechanism

        /// <summary>
        /// this method should be called at the plugin's start and when we change the current database
        /// It refreshed the "static" items of the autocompletion : keywords, snippets, databases, tables, sequences
        /// </summary>
        public static void RefreshStaticItems() {
            if (_itemsListLock.TryEnterWriteLock(-1)) {
                try {

                    _staticItems.Clear();
                    _staticItems = Keywords.GetList().ToList();
                    _staticItems.AddRange(Snippets.Keys.Select(x => new CompletionItem {
                        DisplayText = x,
                        Type = CompletionType.Snippet,
                        Ranking = 0,
                        FromParser = false,
                        Flag = 0
                    }).ToList());
                    _staticItems.AddRange(DataBase.GetDbList());
                    _staticItems.AddRange(DataBase.GetSequencesList());
                    _staticItems.AddRange(DataBase.GetTablesList());

                    // we do the sorting (by type and then by ranking), doing it now will reduce the time for the next sort()
                    _staticItems.Sort(CompletionSortingClass<CompletionItem>.Instance);

                } finally {
                    _itemsListLock.ExitWriteLock();
                }
            }

            // update parser?
            if (_initialized)
                ParserHandler.ParseCurrentDocument();

            _initialized = true;

            // OnUpdatedStaticItems
            if (OnUpdatedStaticItems != null)
                OnUpdatedStaticItems();

            // refresh the list of all the saved items (static + dynamic)
            RefreshDynamicItems();
        }

        /// <summary>
        /// Method called when the event OnParseEnded triggers, i.e. when we just parsed the document and
        /// need to refresh the autocompletion
        /// </summary>
        public static void RefreshDynamicItems() {
            if (_itemsListLock.TryEnterWriteLock(-1)) {
                try {

                    // init with static items
                    _savedAllItems.Clear();
                    _savedAllItems = _staticItems.ToList();

                    // we add the dynamic items to the list
                    _savedAllItems.AddRange(ParserHandler.ParserVisitor.ParsedCompletionItemsList.ToList());

                } finally {
                    _itemsListLock.ExitWriteLock();
                }
            }

            // update the autocompletion (if shown)
            CurrentTypeOfList = TypeOfList.Reset;
            if (IsVisible)
                UpdateAutocompletion(true);
        }

        /// <summary>
        /// Updates the CURRENT ITEMS LIST,
        /// handles the opening or the closing of the autocompletion form on key input, 
        /// it is only called when the user adds or delete a char
        /// 
        /// canChangeListType : if true, it means the autocompletion is already visible, we already have loaded
        /// the list of completionItem needed and the user is typing a word, increasing the filter
        /// </summary>
        public static void UpdateAutocompletion(bool canChangeListType) {

            if (!Config.Instance.AutoCompleteOnKeyInputShowSuggestions && !_openedFromShortCut)
                return;

            var nppCurrentPosition = Npp.CurrentPosition;
            var nppCurrentLine = Npp.Line.CurrentLine;
            var isAutocompVisible = IsVisible;

            // dont show in string/comments..?
            if (!_openedFromShortCut && !isAutocompVisible && !Config.Instance.AutoCompleteShowInCommentsAndStrings && !Style.IsCarretInNormalContext(nppCurrentPosition))
                return;

            // the caret changed line
            if (isAutocompVisible && nppCurrentLine != _shownLine) {
                Cloak();
                return;
            }

            // get current word, current previous word (table or database name)
            int nbPoints;
            string previousWord = "";
            var strOnLeft = Npp.GetTextOnLeftOfPos(nppCurrentPosition);
            var keyword = Abl.ReadAblWord(strOnLeft, false, out nbPoints);
            var splitted = keyword.Split('.');
            string lastCharBeforeWord = "";
            switch (nbPoints) {
                case 0:
                    int startPos = strOnLeft.Length - 1 - keyword.Length;
                    lastCharBeforeWord = startPos >= 0 ? strOnLeft.Substring(startPos, 1) : string.Empty;
                    break;
                case 1:
                    previousWord = splitted[0];
                    keyword = splitted[1];
                    break;
                case 2:
                    previousWord = splitted[1];
                    keyword = splitted[2];
                    break;
                default:
                    keyword = splitted[nbPoints];
                    break;
            }

            // if the autocompletion is hidden or if the user is not continuing to type a word, we might want to 
            // change the list of items in the autocompletion
            if (!isAutocompVisible || canChangeListType) {

                // list of fields or tables
                if (!string.IsNullOrEmpty(previousWord)) {

                    // are we entering a field from a known table?
                    var foundTable = ParserHandler.FindAnyTableOrBufferByName(previousWord);
                    if (foundTable != null) {
                        if (CurrentTypeOfList != TypeOfList.Fields) {
                            CurrentTypeOfList = TypeOfList.Fields;
                            CurrentItems = DataBase.GetFieldsList(foundTable).ToList();
                        }
                        ShowSuggestionList(keyword);
                        return;
                    }

                    // are we entering a table from a connected database?
                    var foundDatabase = DataBase.FindDatabaseByName(previousWord);
                    if (foundDatabase != null) {
                        if (CurrentTypeOfList != TypeOfList.Tables) {
                            CurrentTypeOfList = TypeOfList.Tables;
                            CurrentItems = DataBase.GetTablesList(foundDatabase).ToList();
                        }
                        ShowSuggestionList(keyword);
                        return;
                    }
                }

                // if the current is directly preceded by a :, we are entering an object field/method
                if (lastCharBeforeWord.Equals(":")) {
                    if (CurrentTypeOfList != TypeOfList.KeywordObject) {
                        CurrentTypeOfList = TypeOfList.KeywordObject;
                        CurrentItems = SavedAllItems;
                    }
                    ShowSuggestionList(keyword);
                    return;
                }

                // show normal complete list
                if (CurrentTypeOfList != TypeOfList.Complete) {
                    CurrentTypeOfList = TypeOfList.Complete;
                    CurrentItems = SavedAllItems;
                }
            }

            // close if there is nothing to suggest
            if ((!_openedFromShortCut || nppCurrentPosition != _shownPosition) && (keyword == null || keyword.Length < Config.Instance.AutoCompleteStartShowingListAfterXChar)) {
                Cloak();
                return;
            }
            
            ShowSuggestionList(keyword);
        }

        /// <summary>
        /// This function handles the display of the autocomplete form, create or update it
        /// </summary>
        private static void ShowSuggestionList(string keyword) {

            if (CurrentItems.Count == 0) {
                Cloak();
                return;
            }

            // instanciate the form if needed
            if (_form == null) {
                _form = new AutoCompletionForm {
                    UnfocusedOpacity = Config.Instance.AutoCompleteUnfocusedOpacity,
                    FocusedOpacity = Config.Instance.AutoCompleteFocusedOpacity
                };
                // Set the filter and the sorting class
                _form.YamuiList.SortingClass = CompletionSortingClass<ListItem>.Instance;
                _form.YamuiList.FilterPredicate = CompletionFilterClass.Instance.FilterPredicate;

                _form.YamuiList.EmptyListString = @"No suggestions!";

                _form.InsertSuggestion += OnInsertSuggestion;
                _form.ResizeBegin += OnResizeBegin;

                _form.YamuiList.SetItems(CurrentItems.Cast<ListItem>().ToList());
                _needToSetItems = false;
                _form.Show(Npp.Win32WindowNpp);
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
                form.YamuiList.SetItems(CurrentItems.Cast<ListItem>().ToList());
                _needToSetItems = false;
            }

            // only activate certain types
            if (_needToSetActiveTypes) {
                switch (CurrentTypeOfList) {
                    case TypeOfList.Complete:
                        form.YamuiList.SetUnactiveType(new List<int> {
                            (int)CompletionType.KeywordObject
                        }, false);
                        break;
                    case TypeOfList.KeywordObject:
                        form.YamuiList.SetActiveType(new List<int> {
                            (int)CompletionType.KeywordObject
                        }, false);
                        break;
                    default:
                        form.YamuiList.SetUnactiveType(null, false);
                        break;
                }
            }

            // the filter uses the current caret line to know which item should be filtered, set it here
            var nppCurrentLine = Npp.Line.CurrentLine;
            CompletionFilterClass.Instance.UpdateConditions(nppCurrentLine, false);

            // filter with keyword (keyword can be empty)
            form.YamuiList.FilterString = form.Keyword;

            // close?
            if (!_openedFromShortCut && Config.Instance.AutoCompleteOnKeyInputHideIfEmpty && form.YamuiList.NbItems == 0) {
                Cloak();
                return;
            }

            // if the form was already visible, don't go further
            if (form.Visible)
                return;

            _shownLine = nppCurrentLine;
            _shownPosition = Npp.CurrentPosition;

            // update position
            var point = Npp.GetCaretScreenLocation();
            var lineHeight = Npp.TextHeight(nppCurrentLine);
            point.Y += lineHeight;

            form.SetPosition(point, lineHeight + 2);
            form.UnCloack();
            form.YamuiList.SelectedItemIndex = 0;

        }

        /// <summary>
        /// Method called by the form when the user accepts a suggestion (tab or enter or doubleclick)
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
                if (Config.Instance.CodeReplaceAbbreviations && (data.Flag & ParseFlag.Abbreviation) != 0) {
                    var fullKeyword = Keywords.GetFullKeyword(data.DisplayText);
                    replacementText = fullKeyword ?? data.DisplayText;
                }

                Npp.ReplaceKeywordWrapped(replacementText, offset);

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
            if (item.DisplayText.Equals(_lastRememberedKeyword)) return;
            _lastRememberedKeyword = item.DisplayText;

            item.Ranking++;
            _form.YamuiList.SortInitialList(); // sort the list of items since the ranking has changed

            if (item.FromParser)
                RememberUseOfParsedItem(item.DisplayText);
            else if (item.Type == CompletionType.Database || item.Type == CompletionType.Field || item.Type == CompletionType.FieldPk || item.Type == CompletionType.Table)
                RememberUseOfDatabaseItem(item.DisplayText);                
        }

        /// <summary>
        /// When the user resizes the form, we need to hide the autocompletion
        /// </summary>
        private static void OnResizeBegin(object sender, EventArgs eventArgs) {
            if (InfoToolTip.InfoToolTip.IsVisible)
                InfoToolTip.InfoToolTip.Cloak();
        }

        #endregion

        #region handling item ranking

        /// <summary>
        /// Find ranking of a parsed item
        /// </summary>
        /// <param name="displayText"></param>
        /// <returns></returns>
        public static int FindRankingOfParsedItem(string displayText) {
            return _displayTextRankingParsedItems.ContainsKey(displayText) ? _displayTextRankingParsedItems[displayText] : 0;
        }

        /// <summary>
        /// Find ranking of a database item
        /// </summary>
        /// <param name="displayText"></param>
        /// <returns></returns>
        public static int FindRankingOfDatabaseItem(string displayText) {
            return _displayTextRankingDatabase.ContainsKey(displayText) ? _displayTextRankingDatabase[displayText] : 0;
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

        /// <summary>
        /// remember the use of a particular item in the completion list
        /// (for database items!)
        /// </summary>
        /// <param name="displayText"></param>
        public static void RememberUseOfDatabaseItem(string displayText) {
            if (!_displayTextRankingDatabase.ContainsKey(displayText))
                _displayTextRankingDatabase.Add(displayText, 1);
            else
                _displayTextRankingDatabase[displayText]++;
        }

        #endregion
        
        #region _form handler

        /// <summary>
        /// Is the form currently visible?
        /// </summary>
        public static bool IsVisible {
            get { return !(_form == null || !(bool) _form.SafeSyncInvoke(form => form.Visible)); }
        }

        /// <summary>
        /// Closes the form
        /// </summary>
        public static void Cloak() {
            try {
                if (_form != null)
                    _form.SafeSyncInvoke(form => form.Cloack());
                _openedFromShortCut = false;

                // closing the autocompletion form also closes the tooltip
                InfoToolTip.InfoToolTip.CloseIfOpenedForCompletion();
            } catch (Exception e) {
                ErrorHandler.LogError(e);
            }
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
            } catch (Exception e) {
                ErrorHandler.LogError(e);
            }
        }

        /// <summary>
        /// Passes the OnKey input of the CharAdded or w/e event to the auto completion form
        /// </summary>
        public static bool PerformKeyDown(KeyEventArgs e) {
            return IsVisible && (bool)_form.YamuiList.SafeSyncInvoke(list => list.PerformKeyDown(e));
        }

        /// <summary>
        /// Returns true if the cursor is within the form window
        /// </summary>
        public static bool IsMouseIn() {
            return Win32Api.IsCursorIn(_form.Handle);
        }

        #endregion

    }
}