#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Threading.Tasks;
using System.Windows.Forms;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures.Parser;
using Timer = System.Windows.Forms.Timer;

namespace _3PA.MainFeatures.AutoCompletion {

    /// <summary>
    /// This class handles the AutoCompletionForm
    /// </summary>
    internal static class AutoComplete {

        #region field

        /// <summary>
        /// Was the autocompletion opened naturally or from the user shortkey?
        /// </summary>
        private static bool _openedFromShortCut;

        /// <summary>
        /// position of the carret when the autocompletion was opened (from shortcut)
        /// </summary>
        private static int _openedFromShortCutPosition;

        private static AutoCompletionForm _form;

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
        private static TypeOfList CurrentTypeOfList {
            get { return _currentTypeOfList; }
            set {
                _needToSetActiveTypes = _currentTypeOfList != TypeOfList.Reset;
                _needToSetItems = true;
                _currentTypeOfList = value;
            }
        }
        private static TypeOfList _currentTypeOfList;
        private static bool _needToSetItems;
        private static bool _needToSetActiveTypes;

        // contains the list of items currently display in the form
        private static List<CompletionData> _currentItems = new List<CompletionData>();

        // contains the whole list of items (minus the fields) to show, can be updated through FillItems() method
        private static List<CompletionData> _savedAllItems = new List<CompletionData>();

        // contains the list of items that do not depend on the current file (keywords, database, snippets)
        private static List<CompletionData> _staticItems = new List<CompletionData>();

        // holds the display order of the CompletionType
        private static List<int> _completionTypePriority;

        /// <summary>
        /// is used to make sure that 2 different threads dont try to access
        /// the same resource (_parserTimer) at the same time, which would be problematic
        /// </summary>
        private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private static ReaderWriterLockSlim _lockShowList = new ReaderWriterLockSlim();

        private static Timer _parserTimer;

        private static string _lastRememberedKeyword = "";

        #endregion

        #region public misc.

        /// <summary>
        /// A dictionnary of known keywords and database info
        /// </summary>
        public static Dictionary<string, CompletionType> KnownStaticItems { get; private set; }

        /// <summary>
        /// returns the ranking of each CompletionType, helps sorting them as we wish
        /// </summary>
        public static List<int> GetPriorityList {
            get {
                if (_completionTypePriority != null) return _completionTypePriority;
                _completionTypePriority = Config.GetPriorityList(typeof(CompletionType), "AutoCompletePriorityList");
                return _completionTypePriority;
            }
        }

        /// <summary>
        /// Returns a keyword from the autocompletion list with the correct case
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="lastWordPos"></param>
        /// <returns></returns>
        public static string CorrectKeywordCase(string keyword, int lastWordPos) {
            string output = null;
            var found = FindInSavedItems(keyword, Npp.Line.CurrentLine);
            if (found != null) {
                RememberUseOf(found);
                output = !found.FromParser ? keyword.AutoCaseToUserLiking() : found.DisplayText;
            } else {
                // search in tables fields
                var tableFound = ParserHandler.FindAnyTableOrBufferByName(Npp.GetFirstWordRightAfterPoint(lastWordPos));
                if (tableFound != null) {
                    var fieldFound = DataBase.FindFieldByName(keyword, tableFound);
                    if (fieldFound != null) {
                        RememberUseOf(new CompletionData {
                            FromParser = false,
                            DisplayText = fieldFound.Name,
                            Type = CompletionType.Field,
                            Ranking = 0
                        });
                        ParserHandler.RememberUseOfDatabaseItem(fieldFound.Name);
                        output = tableFound.IsTempTable ? fieldFound.Name : keyword.AutoCaseToUserLiking();
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// try to match the keyword with an item in the autocomplete list
        /// </summary>
        /// <returns></returns>
        public static CompletionData FindInSavedItems(string keyword, int line) {
            var filteredList = AutoCompletionForm.ExternalFilterItems(_savedAllItems.ToList(), line);
            if (filteredList == null || filteredList.Count <= 0) return null;
            CompletionData found = filteredList.FirstOrDefault(data => data.DisplayText.EqualsCi(keyword));
            return found;
        }

        /// <summary>
        /// Find a list of items in the completion and return it
        /// Uses the position to filter the list the same way the autocompletion form would
        /// </summary>
        /// <returns></returns>
        public static List<CompletionData> FindInCompletionData(string keyword, int position, bool dontCheckLine = false) {
            var filteredList = AutoCompletionForm.ExternalFilterItems(_savedAllItems.ToList(), Npp.LineFromPosition(position), dontCheckLine);
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

        /// <summary>
        /// returns the keyword currently selected in the completion list
        /// </summary>
        /// <returns></returns>
        public static CompletionData GetCurrentSuggestion() {
            return _form.GetCurrentSuggestion();
        }
        #endregion

        #region core mechanism

        /// <summary>
        /// Call this method to parse the current document after a small delay 
        /// (delay that is reset each time this function is called, so if you call it continously, nothing is done)
        /// or set doNow = true to do it without waiting a timer
        /// </summary>
        /// <param name="doNow"></param>
        public static void ParseCurrentDocument(bool doNow = false) {
            // parse immediatly
            if (doNow) {
                ParseCurrentDocumentTick();
                return;
            }

            // parse in 1s, if nothing delays the timer
            if (_lock.TryEnterWriteLock(500)) {
                try {
                    if (_parserTimer == null) {
                        _parserTimer = new Timer {Interval = 800};
                        _parserTimer.Tick += (sender, args) => ParseCurrentDocumentTick();
                        _parserTimer.Start();
                    } else {
                        // reset timer
                        _parserTimer.Stop();
                        _parserTimer.Start();
                    }
                } finally {
                    _lock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Called when the _parserTimer ticks
        /// refresh the Items list with all the static items
        /// as well as the dynamic items found by the parser
        /// </summary>
        private static void ParseCurrentDocumentTick() {
            Task.Factory.StartNew(() => {
                if (_lock.TryEnterWriteLock(500)) {
                    try {
                        // delete timer
                        if (_parserTimer != null) {
                            _parserTimer.Dispose();
                            _parserTimer = null;
                        }

                        //------------
                        //var watch = Stopwatch.StartNew();

                        CodeExplorer.CodeExplorer.Refreshing = true;

                        // init with static items
                        _savedAllItems.Clear();
                        _savedAllItems = _staticItems.ToList();

                        do {

                            // we launch the parser, that will fill the DynamicItems
                            ParserHandler.RefreshParser();

                            // we had the dynamic items to the list
                            _savedAllItems.AddRange(ParserHandler.GetParsedItemsList());

                            // update autocompletion
                            CurrentTypeOfList = TypeOfList.Reset;
                            if (IsVisible)
                                UpdateAutocompletion();

                        } while (!ParserHandler.LastParsedFilePath.Equals(Plug.CurrentFilePath));

                        // ## for the code explorer we ask it to update itself ##
                        CodeExplorer.CodeExplorer.UpdateCodeExplorer();

                        //watch.Stop();
                        //UserCommunication.Notify("Updated in " + watch.ElapsedMilliseconds + " ms", 1);
                        //------------

                    } catch (Exception e) {
                        ErrorHandler.ShowErrors(e, "Error in ParseCurrentDocumentTick");
                    } finally {
                        _lock.ExitWriteLock();
                    }
                }
            });
        }

        /// <summary>
        /// this method should be called at the plugin's start and when we change the current database
        /// It refreshed the "static" items of the autocompletion : keywords, snippets, databases, tables, sequences
        /// </summary>
        public static void RefreshStaticItems(bool initializing = false) {

            _staticItems.Clear();
            _staticItems = Keywords.GetList().ToList();
            _staticItems.AddRange(Snippets.Keys.Select(x => new CompletionData {
                DisplayText = x,
                Type = CompletionType.Snippet,
                Ranking = 0,
                FromParser = false,
                Flag = 0
            }).ToList());

            // add database info?
            if (!initializing) {
                _staticItems.AddRange(DataBase.GetDbList());
                _staticItems.AddRange(DataBase.GetSequencesList());
                _staticItems.AddRange(DataBase.GetTablesList());
            }

            // we do the sorting (by type and then by ranking), doing it now will reduce the time for the next sort()
            _staticItems.Sort(new CompletionDataSortingClass());
            _savedAllItems = _staticItems.ToList();

            // Update the known items!
            KnownStaticItems = DataBase.GetDbDictionnary();
            foreach (var keyword in Keywords.GetList().Where(keyword => !KnownStaticItems.ContainsKey(keyword.DisplayText))) {
                KnownStaticItems[keyword.DisplayText] = keyword.Type;
            }

            // Update the form?
            if (!initializing) {
                CurrentTypeOfList = TypeOfList.Reset;
                if (IsVisible)
                    UpdateAutocompletion();
            }
        }

        /// <summary>
        /// Called from CTRL + Space shortcut
        /// </summary>
        public static void OnShowCompleteSuggestionList() {
            ParseCurrentDocument();
            _openedFromShortCut = true;
            _openedFromShortCutPosition = Npp.CurrentPosition;
            UpdateAutocompletion();
        }

        /// <summary>
        /// handles the opening or the closing of the autocompletion form on key input, 
        /// it is only called when the user adds or delete a char
        /// </summary>
        public static void UpdateAutocompletion() {
            if (_lockShowList.TryEnterWriteLock(500)) {
                try {
                    if (!Config.Instance.AutoCompleteOnKeyInputShowSuggestions && !_openedFromShortCut)
                        return;

                    // dont show in string/comments..?
                    if (!_openedFromShortCut && !IsVisible && !Config.Instance.AutoCompleteShowInCommentsAndStrings && !Style.IsCarretInNormalContext(Npp.CurrentPosition))
                        return;

                    // get current word, current previous word (table or database name)
                    int nbPoints;
                    string previousWord = "";
                    var strOnLeft = Npp.GetTextOnLeftOfPos(Npp.CurrentPosition);
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

                    // list of fields or tables
                    if (!string.IsNullOrEmpty(previousWord)) {
                        // are we entering a field from a known table?
                        var foundTable = ParserHandler.FindAnyTableOrBufferByName(previousWord);
                        if (foundTable != null) {
                            if (CurrentTypeOfList != TypeOfList.Fields) {
                                CurrentTypeOfList = TypeOfList.Fields;
                                _currentItems = DataBase.GetFieldsList(foundTable).ToList();
                            }
                            ShowSuggestionList(keyword);
                            return;
                        }

                        // are we entering a table from a connected database?
                        var foundDatabase = DataBase.FindDatabaseByName(previousWord);
                        if (foundDatabase != null) {
                            if (CurrentTypeOfList != TypeOfList.Tables) {
                                CurrentTypeOfList = TypeOfList.Tables;
                                _currentItems = DataBase.GetTablesList(foundDatabase).ToList();
                            }
                            ShowSuggestionList(keyword);
                            return;
                        }
                    }

                    // close if there is nothing to suggest
                    if ((!_openedFromShortCut || _openedFromShortCutPosition != Npp.CurrentPosition) && (string.IsNullOrEmpty(keyword) || keyword != null && keyword.Length < Config.Instance.AutoCompleteStartShowingListAfterXChar)) {
                        Close();
                        return;
                    }

                    // if the current is directly preceded by a :, we are entering an object field/method
                    if (lastCharBeforeWord.Equals(":")) {
                        if (CurrentTypeOfList != TypeOfList.KeywordObject) {
                            CurrentTypeOfList = TypeOfList.KeywordObject;
                            _currentItems = _savedAllItems;
                        }
                        ShowSuggestionList(keyword);
                        return;
                    }

                    // show normal complete list
                    if (CurrentTypeOfList != TypeOfList.Complete) {
                        CurrentTypeOfList = TypeOfList.Complete;
                        _currentItems = _savedAllItems;
                    }
                    ShowSuggestionList(keyword);

                } catch (Exception e) {
                    ErrorHandler.ShowErrors(e, "Error in UpdateAutocompletion");
                } finally {
                    _lockShowList.ExitWriteLock();
                }
            }
            
        }

        /// <summary>
        /// This function handles the display of the autocomplete form, create or update it
        /// </summary>
        /// <param name="keyword"></param>
        private static void ShowSuggestionList(string keyword) {
            if (_currentItems.Count == 0) {
                Close();
                return;
            }

            // instanciate the form if needed
            if (_form == null) {
                _form = new AutoCompletionForm(keyword) {
                    UnfocusedOpacity = Config.Instance.AutoCompleteUnfocusedOpacity,
                    FocusedOpacity = Config.Instance.AutoCompleteFocusedOpacity
                };
                _form.InsertSuggestion += OnInsertSuggestion;
                _form.Show(Npp.Win32WindowNpp);
                _form.SetItems(_currentItems);
            } else if (_needToSetItems) {
                // we changed the mode, we need to Set the items of the autocompletion
                _form.SetItems(_currentItems, _needToSetActiveTypes || !_form.Visible);
                _needToSetItems = false;
            }

            // only activate certain types
            if (_needToSetActiveTypes || !_form.Visible) {
                // only activate certain types
                switch (CurrentTypeOfList) {
                    case TypeOfList.Complete:
                        _form.SetUnActiveType(new List<CompletionType> {
                            CompletionType.KeywordObject
                        });
                        break;
                    case TypeOfList.KeywordObject:
                        _form.SetActiveType(new List<CompletionType> {
                            CompletionType.KeywordObject
                        });
                        break;
                    default:
                        _form.SetUnActiveType(null);
                        break;
                }
            }

            // filter with keyword (keyword can be empty)
            _form.FilterByText = keyword;

            // close?
            if (!_openedFromShortCut && Config.Instance.AutoCompleteOnKeyInputHideIfEmpty && _form.TotalItems == 0) {
                Close();
                return;
            }

            // if the form was already visible, don't go further
            if (_form.Visible) return;

            _form.SelectFirstItem();

            // update position (and alternate color config)
            var point = Npp.GetCaretScreenLocation();
            var lineHeight = Npp.TextHeight(Npp.Line.CurrentLine);
            point.Y += lineHeight;
            _form.UseAlternateBackColor = Config.Instance.GlobalUseAlternateBackColorOnGrid;
            _form.SetPosition(point, lineHeight + 2);

            _form.UnCloack();
        }

        /// <summary>
        /// Method called by the form when the user accepts a suggestion (tab or enter or doubleclick)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="tabCompletedEventArgs"></param>
        private static void OnInsertSuggestion(object sender, TabCompletedEventArgs tabCompletedEventArgs) {
            try {
                var data = tabCompletedEventArgs.CompletionItem;

                // in case of keyword, replace abbreviation if needed
                var replacementText = data.DisplayText;
                if (Config.Instance.CodeReplaceAbbreviations && (data.Type == CompletionType.Keyword || data.Type == CompletionType.KeywordObject)) {
                    var fullKeyword = Keywords.GetFullKeyword(data.DisplayText);
                    replacementText = fullKeyword ?? data.DisplayText;
                }
                Npp.ReplaceKeywordWrapped(replacementText, 0);

                // Remember this item to show it higher in the list later
                RememberUseOf(data);

                if (data.Type == CompletionType.Snippet)
                    Snippets.TriggerCodeSnippetInsertion();

                Close();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error during AutoCompletionAccepted");
            }
        }

        /// <summary>
        /// Increase ranking of a given CompletionData
        /// </summary>
        /// <param name="data"></param>
        private static void RememberUseOf(CompletionData data) {
            // handles unwanted rank progression (when the user enter several times the same keyword)
            if (data.DisplayText.Equals(_lastRememberedKeyword)) return;
            _lastRememberedKeyword = data.DisplayText;

            data.Ranking++;
            if (data.FromParser)
                ParserHandler.RememberUseOfParsedItem(data.DisplayText);
            else if (data.Type != CompletionType.Keyword && data.Type != CompletionType.Snippet)
                ParserHandler.RememberUseOfDatabaseItem(data.DisplayText);

            // sort the items, to reflect the latest ranking
            if (_form != null)
                _form.SortItems();
        }

        #endregion

        #region _form handler

        /// <summary>
        /// Is the form currently visible?
        /// </summary>
        public static bool IsVisible {
            get { return _form != null && _form.Visible; }
        }

        /// <summary>
        /// Closes the form
        /// </summary>
        public static void Close() {
            try {
                if (_form != null)
                    _form.Cloack();
                _openedFromShortCut = false;

                // closing the autocompletion form also closes the tooltip
                InfoToolTip.InfoToolTip.CloseIfOpenedForCompletion();
            } catch (Exception x) {
                ErrorHandler.Log(x.Message);
            }
        }

        /// <summary>
        /// Forces the form to close, only when leaving npp
        /// </summary>
        public static void ForceClose() {
            try {
                if (_form != null)
                    _form.ForceClose();
                _form = null;
            } catch (Exception x) {
                ErrorHandler.Log(x.Message);
            }
        }

        /// <summary>
        /// Passes the OnKey input of the CharAdded or w/e event to the auto completion form
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool OnKeyDown(Keys key) {
            return IsVisible && _form.OnKeyDown(key);
        }

        /// <summary>
        /// Returns true if the cursor is within the form window
        /// </summary>
        public static bool IsMouseIn() {
            return WinApi.IsCursorIn(_form.Handle);
        }

        #endregion
    }
}