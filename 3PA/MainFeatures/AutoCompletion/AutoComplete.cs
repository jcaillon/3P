using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using _3PA.Lib;

namespace _3PA.MainFeatures.AutoCompletion {

    /// <summary>
    /// This class manipulate the AutoCompletionForm
    /// </summary>
    internal class AutoComplete {

        /// <summary>
        /// Was the autocompletion opened naturally or from the user shortkey?
        /// </summary>
        private static bool _openedFromShortCut;

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
                _needToSetItems = true;
                _currentTypeOfList = value;
            }
        }
        private static TypeOfList _currentTypeOfList;
        private static bool _needToSetItems;

        // contains the list of items currently display in the form
        private static List<CompletionData> _currentItems;

        // contains the whole list of items (minus the fields) to show, can be updated through FillItems() method
        private static List<CompletionData> _savedAllItems;

        // contains the list of items that do not depend on the current file (keywords, database, snippets)
        private static List<CompletionData> _staticItems;

        // holds the display order of the CompletionType
        private static List<int> _completionTypePriority;

        /// <summary>
        /// is used to make sure that 2 different threads dont try to access
        /// the same resource (_parserTimer) at the same time, which would be problematic
        /// </summary>
        private static object _thisLock = new object();
        private static Timer _parserTimer;

        #region public misc.

        /// <summary>
        /// returns the ranking of each CompletionType, helps sorting them as we wish
        /// </summary>
        public static List<int> GetPriorityList {
            get {
                if (_completionTypePriority != null) return _completionTypePriority;
                _completionTypePriority = new List<int>();
                var temp = Config.Instance.AutoCompletePriorityList.Split(',').Select(Int32.Parse).ToList();
                for (int i = 0; i < Enum.GetNames(typeof (CompletionType)).Length; i++)
                    _completionTypePriority.Add(temp.IndexOf(i));
                return _completionTypePriority;
            }
        }

        /// <summary>
        /// Returns a keyword from the autocompletion list with the correct case
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public static string CorrectKeywordCase(string keyword) {
            if (_savedAllItems == null) return null;
            CompletionData found = _savedAllItems.Find(data => data.DisplayText.EqualsCi(keyword));
            if (found != null)
                return !found.FromParser ? Abl.AutoCaseToUserLiking(keyword) : found.DisplayText;
            // search in tables' fields
            var previousWord = Npp.GetFirstWordRightAfterPoint(Npp.GetCaretPosition());
            if (!String.IsNullOrEmpty(previousWord) && ParserHandler.FindAnyTableOrBufferByName(previousWord) != null)
                return Abl.AutoCaseToUserLiking(keyword);
            return null;
        }

        #endregion

        /// <summary>
        /// Call this method to asynchronously parse the current document 
        /// (or set doNow = true to do it synchonously)
        /// </summary>
        /// <param name="doNow"></param>
        public static void ParseCurrentDocument(bool doNow = false) {
            // parse immediatly
            if (doNow) {
                ParseCurrentDocumentTick();
                return;
            }

            lock (_thisLock) {
                // parse in 1s, if nothing delays the timer
                if (_parserTimer == null) {
                    _parserTimer = new Timer { Interval = 1000 };
                    _parserTimer.Tick += (sender, args) => ParseCurrentDocumentTick();
                    _parserTimer.Start();
                } else {
                    // reset timer
                    _parserTimer.Stop();
                    _parserTimer.Start();
                }
            }
        }

        /// <summary>
        /// Called when the _parserTimer ticks
        /// </summary>
        private static void ParseCurrentDocumentTick() {
            lock (_thisLock) {
                try {
                    if (_parserTimer != null) {
                        _parserTimer.Dispose();
                        _parserTimer = null;
                    }
                    FillItems();
                } catch (Exception e) {
                    ErrorHandler.ShowErrors(e, "Error in ParseCurrentDocumentTick");
                }
            }
        }

        /// <summary>
        /// this method should be called to refresh the Items list with all the static items
        /// as well as the dynamic items found by the parser
        /// </summary>
        public static void FillItems() {
            if (_savedAllItems == null) 
                _savedAllItems = new List<CompletionData>();

            // init with static items
            _savedAllItems.Clear();
            _savedAllItems = _staticItems.ToList();

            // we launch the parser, that will fill the DynamicItems
            ParserHandler.RefreshParser();

            // we had the dynamic items to the list
            _savedAllItems.AddRange(ParserHandler.GetParsedItemList());

            // update autocompletion
            CurrentTypeOfList = TypeOfList.Reset;
            if (IsVisible)
                UpdateAutocompletion();
        }

        /// <summary>
        /// this method should be called at the plugin's start and when we change the current database
        /// of each features
        /// </summary>
        public static void FillStaticItems(bool initializing) {
            if (_savedAllItems == null)
                _savedAllItems = new List<CompletionData>();

            // creates the static items list
            if (_staticItems == null) 
                _staticItems = new List<CompletionData>();
            _staticItems.Clear();
            _staticItems =Keywords.GetList().ToList();
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
                _staticItems.AddRange(DataBase.GetTablesList());
            }

            // we do the sorting (by type and then by ranking), doing it now will reduce the time for the next sort()
            _staticItems.Sort(new CompletionDataSortingClass());
            _savedAllItems = _staticItems.ToList();

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
            try {
                UpdateAutocompletion();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in ShowCompleteSuggestionList");
            }
        }

        /// <summary>
        /// handles the opening or the closing of the autocompletion form on key input, 
        /// it is only called when the user adds or delete a char
        /// </summary>
        public static void UpdateAutocompletion() {
            try {
                if (!Config.Instance.AutoCompleteOnKeyInputShowSuggestions && !_openedFromShortCut) 
                    return;

                // get current word, current previous word (table or database name)
                int nbPoints;
                string previousWord = "";
                var strOnLeft = Npp.GetTextOnLeftOfPos(Npp.GetCaretPosition());
                var keyword = Abl.ReadAblWord(strOnLeft, false, out nbPoints);
                var splitted = keyword.Split('.');
                string lastCharBeforeWord = "";
                switch (nbPoints) {
                    case 0:
                        int startPos = strOnLeft.Length - 1 - keyword.Length;
                        lastCharBeforeWord = startPos >= 0 ? strOnLeft.Substring(startPos, 1) : String.Empty;
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
                if (!String.IsNullOrEmpty(previousWord)) {
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

                // if the current is directly preceded by a :, we are entering an object field/method
                if (lastCharBeforeWord.Equals(":")) {
                    if (CurrentTypeOfList != TypeOfList.KeywordObject) {
                        CurrentTypeOfList = TypeOfList.KeywordObject;
                        _currentItems = _savedAllItems;
                    }
                    ShowSuggestionList(keyword);
                    return;
                }

                // close if there is nothing to suggest
                if (!_openedFromShortCut && (String.IsNullOrEmpty(keyword) || keyword != null && keyword.Length < Config.Instance.AutoCompleteStartShowingListAfterXChar)) {
                    Close();
                    return;
                }

                // show normal complete list
                if (CurrentTypeOfList != TypeOfList.Complete) {
                    CurrentTypeOfList = TypeOfList.Complete;
                    _currentItems = _savedAllItems;
                }
                ShowSuggestionList(keyword);

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in ShowCompleteSuggestionList");
            }
        }

        /// <summary>
        /// This function handles the display of the autocomplete form, create or update it
        /// </summary>
        /// <param name="keyword"></param>
        private static void ShowSuggestionList(string keyword) {
            if (_currentItems == null) {
                Close();
                return;
            }

            // instanciate the form if needed
            if (_form == null) {
                _form = new AutoCompletionForm(keyword) {
                    UnfocusedOpacity = Config.Instance.AutoCompleteUnfocusedOpacity,
                    FocusedOpacity = Config.Instance.AutoCompleteFocusedOpacity
                };
                _form.TabCompleted += OnTabCompleted;
                _form.Show(Npp.Win32WindowNpp);
                _form.SetItems(_currentItems);
            } else if (_needToSetItems) {
                // we changed the mode, we need to Set the items of the autocompletion
                _form.SetItems(_currentItems);
            }

            // only activate certain types
            if (_needToSetItems || !_form.Visible) { 
                // only activate certain types
                switch (CurrentTypeOfList) {
                    case TypeOfList.Complete:
                        _form.SetUnActiveType(new List<CompletionType> {
                            CompletionType.KeywordObject,
                        });
                        break;
                    case TypeOfList.KeywordObject:
                        _form.SetActiveType(new List<CompletionType> {
                            CompletionType.KeywordObject,
                        });
                        break;
                }
                // we just Set the items so we don't need to reset the active types
                _needToSetItems = false;
            }

            // filter with keyword (keyword can be empty)
            _form.FilterByText = keyword;
            _form.SelectFirstItem();

            // close?
            if (!_openedFromShortCut && !Config.Instance.AutoCompleteOnKeyInputHideIfEmpty && _form.TotalItems == 0) {
                Close();
                return;
            }


            // if the form was already visible, don't go further
            if (_form.Visible) return;

            // update position (and alternate color config)
            var point = Npp.GetCaretScreenLocation();
            var lineHeight = Npp.GetTextHeight(Npp.GetCaretLineNumber());
            point.Y += lineHeight;
            _form.UseAlternateBackColor = Config.Instance.AutoCompleteAlternateBackColor;
            _form.SetPosition(point, lineHeight + 2);

            _form.UnCloack();
        }

        /// <summary>
        /// Method called by the form when the user accepts a suggestion (tab or enter or doubleclick)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="tabCompletedEventArgs"></param>
        private static void OnTabCompleted(object sender, TabCompletedEventArgs tabCompletedEventArgs) {
            try {
                var data = tabCompletedEventArgs.CompletionItem;
                if (data.DisplayText != "" && data.DisplayText != "<empty>") {
                    Point keywordPos;
                    var keywordToRep = Npp.GetKeywordOnLeftOfPosition(Npp.GetCaretPosition(), out keywordPos);

                    // if the "hint" is empty, don't replace the current hint, just add text at the carret position
                    if ((data.Type == CompletionType.Field && Npp.TextBeforeCaret(2).EndsWith(".")) ||
                        String.IsNullOrWhiteSpace(keywordToRep)) {
                        var curPos = Npp.GetCaretPosition();
                        keywordPos.X = curPos;
                        keywordPos.Y = curPos;
                    }

                    //TODO config to replace all abbrev by complete word

                    Npp.BeginUndoAction();
                    Npp.ReplaceKeyword(data.DisplayText, keywordPos);
                    Npp.EndUndoAction();

                    // Remember this item to show it higher in the list later
                    data.Ranking++;
                    if (data.FromParser)
                        ParserHandler.RememberUseOfParsedItem(data.DisplayText);
                    else if (data.Type != CompletionType.Keyword && data.Type != CompletionType.Snippet)
                        ParserHandler.RememberUseOfDatabaseItem(data.DisplayText);

                    // sort the items, to reflect the latest ranking
                    _form.SortItems();

                    if (data.Type == CompletionType.Snippet)
                        Snippets.TriggerCodeSnippetInsertion();
                }

                Close();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error during AutoCompletionAccepted");
            }
        }

        #region _form handler

        /// <summary>
        /// Is the autocompletion currently visible?
        /// </summary>
        public static bool IsVisible {
            get { return _form != null && _form.Visible; }
        }

        /// <summary>
        /// Closes the form
        /// </summary>
        public static void Close() {
            try {
                _form.Cloack();
                _openedFromShortCut = false;
            } catch (Exception) {
                // ignored
            }
        }

        /// <summary>
        /// Forces the form to close, only when leaving npp
        /// </summary>
        public static void ForceClose() {
            try {
                _form.ForceClose();
                _form = null;
            } catch (Exception) {
                // ignored
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

        #endregion

    }
}