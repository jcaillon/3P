using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using YamuiFramework.HtmlRenderer.Core.Core;
using _3PA.Lib;
using _3PA.MainFeatures.Parser;

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
            Complete,
            Fields,
            Tables,
            ObjectKeywords
        }
        private static TypeOfList _currentTypeOfList;
        private static ParsedDataBase _currentDataBase;
        private static ParsedTable _currentTable;

        // true if we are using the _savedAllItems in _currentItems, false otherwise (= we are showing fields)
        private static bool _showingAllItems;

        // contains the list of items currently display in the form
        private static List<CompletionData> _currentItems;

        // contains the whole list of items (minus the fields) to show, can be updated through FillItems() method
        private static List<CompletionData> _savedAllItems;

        // contains the list of items that do not depend on the current file (keywords, database, snippets)
        private static List<CompletionData> _staticItems;

        // holds the display order of the CompletionType
        private static List<int> _completionTypePriority;

        /// <summary>
        /// returns the ranking of each CompletionType, helps sorting them as we wish
        /// </summary>
        public static List<int> GetPriorityList  {
            get {
                if (_completionTypePriority != null) return _completionTypePriority;
                _completionTypePriority = new List<int>();
                var temp = Config.Instance.AutoCompletePriorityList.Split(',').Select(int.Parse).ToList();
                for (int i = 0; i < Enum.GetNames(typeof(CompletionType)).Length; i++) {
                    _completionTypePriority.Add(temp.IndexOf(i));
                }
                return _completionTypePriority;
            }
        }
        
        /// <summary>
        /// this method should be called to refresh the Items list with all the static items
        /// as well as the dynamic items found by the parser
        /// </summary>
        public static void FillItems() {
            if (_savedAllItems == null) _savedAllItems = new List<CompletionData>();

            // init with static items
            _savedAllItems.Clear();
            _savedAllItems = _staticItems.ToList();

            // we launch the parser, that will fill the DynamicItems
            ParserHandler.RefreshParser();

            // we had the dynamic items to the list
            _savedAllItems.AddRange(ParserHandler.ParsedItemsList);

            // update autocompletion
            _currentItems = _savedAllItems;
            if (_showingAllItems)
                UpdateFormWithCurrentItemList();
        }

        /// <summary>
        /// this method should be called at the plugin's start and when we change the current database
        /// of each features
        /// </summary>
        public static void FillStaticItems() {
            // creates the static items list
            if (_staticItems == null) _staticItems = new List<CompletionData>();
            _staticItems.Clear();
            _staticItems =Keywords.GetList().ToList();
            _staticItems.AddRange(Snippets.Keys.Select(x => new CompletionData {
                DisplayText = x, 
                Type = CompletionType.Snippet,
                Ranking = 0,
                FromParser = false,
                Flag = 0
            }).ToList());
            _staticItems.AddRange(DataBase.GetDbList());
            _staticItems.AddRange(DataBase.GetTablesList());

            // we do the sorting (by type and then by ranking), doing it now will reduce the time for the next sort()
            _staticItems.Sort(new CompletionDataSortingClass());
            _currentItems = _staticItems;

            if (_savedAllItems == null) {
                _savedAllItems = new List<CompletionData>();
                _savedAllItems = _staticItems.ToList();
            }

            if (_showingAllItems)
                UpdateFormWithCurrentItemList();
        }

        /// <summary>
        /// Returns a keyword from the autocompletion list with the correct case
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public static string CorrectKeywordCase(string keyword) {
            if (_currentItems == null) return null;
            CompletionData found = _currentItems.Find(data => data.DisplayText.EqualsCi(keyword));
            if (found == null) return null;
            if (!found.FromParser) return Abl.AutoCaseToUserLiking(keyword);
            return found.DisplayText;
        }


        /// <summary>
        /// handles the opening or the closing of the autocompletion form on key input, 
        /// it is only called when the user adds or delete a char
        /// </summary>
        public static void UpdateAutocompletion() {
            try {
                string keyword = Npp.GetKeyword();

                //TODO: Dont show autocompletion if SCI_GETSELECTIONS multiple selections! No better, autocomplete on each line..

                // overwrite for . and : we need to show the list
                var lastChar = Npp.GetCharBeforeWord(Npp.GetCaretPosition());
                if ((lastChar.Equals(".") || lastChar.Equals(":")) && IsNeedToUpdateTypeOfList()) {
                    if (IsVisible) {
                        // update the filter
                        _form.FilterByText = keyword;
                        UpdateTypeOfList(true);
                    } else
                        ShowSuggestionList(UpdateTypeOfList(true));
                    return;
                }

                // can we even show it?
                if (keyword.Length >= Config.Instance.AutoCompleteStartShowingListAfterXChar || IsVisible) {
                    // the form is already visible
                    if (IsVisible) {
                        // close if the current keyword isn't long enough
                        if (!_openedFromShortCut && keyword.Length < Config.Instance.AutoCompleteStartShowingListAfterXChar) {
                            Close();
                            return;
                        }
                        
                        // update the filter
                        _form.FilterByText = keyword;

                        // close it if the list is empty and we the user selected the option to close it
                        if (!_openedFromShortCut && !Config.Instance.AutoCompleteOnKeyInputHideIfEmpty && _form.TotalItems == 0)
                            Close();                       
                    } else {
                        // show it
                        if (Config.Instance.AutoCompleteShowInCommentsAndStrings || Npp.IsNormalContext(Npp.GetCaretPosition()))
                            ShowSuggestionList(UpdateTypeOfList());
                    }
                } else {
                    Close();
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in ShowCompleteSuggestionList");
            }
        }

        /// <summary>
        /// Called from CTRL + Space shortcut
        /// </summary>
        public static void OnShowCompleteSuggestionList() {
            _openedFromShortCut = true;
            _currentTypeOfList = TypeOfList.Complete;
            try {
                ShowSuggestionList(UpdateTypeOfList());
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in ShowCompleteSuggestionList");
            }
        }

        /// <summary>
        /// Display the autocomp list, uses the TypeOfList _currentTypeOfList,
        /// can be called even if the List is already visible! will just update it!
        /// </summary>
        public static List<CompletionType> UpdateTypeOfList(bool updateTypeOfList) {
            // activated completionType
            List<CompletionType> activateTypesList = null;

            // Do we need to change the current type of list? then update item list
            if (updateTypeOfList) {
                switch (_currentTypeOfList) {
                    case TypeOfList.Complete:
                        ResetToCompleteItems();
                        // filterList = 
                        break;
                    case TypeOfList.Fields:
                        SwapToFieldsItems(_currentTable);
                        break;
                    case TypeOfList.ObjectKeywords:
                        ResetToCompleteItems();
                        activateTypesList = new List<CompletionType> {
                            CompletionType.KeywordObject,
                        };
                        break;
                    case TypeOfList.Tables:
                        SwapToTablesItems(_currentDataBase);
                        break;
                }
            }
            return activateTypesList;
        }

        /// <summary>
        /// Display the autocomp list, uses the TypeOfList _currentTypeOfList,
        /// can be called even if the List is already visible! will just update it!
        /// </summary>
        public static List<CompletionType> UpdateTypeOfList() {
            return UpdateTypeOfList(IsNeedToUpdateTypeOfList());
        }

        /// <summary>
        /// is there a need to update the type of list?
        /// </summary>
        /// <returns></returns>
        public static bool IsNeedToUpdateTypeOfList() {
            var currentTypeOfList = _currentTypeOfList;
            _currentTypeOfList = GetListToShow();
            return currentTypeOfList != _currentTypeOfList;
        }

        /// <summary>
        /// Output the type of list we need to display to the user, also store the found dataBase or 
        /// foundTable for later use (avoid calculating it twice)
        /// </summary>
        /// <returns></returns>
        private static TypeOfList GetListToShow() {
            var curPos = Npp.GetCaretPosition();
            var previousWord = Npp.GetFirstWordRightAfterPoint(curPos);

            if (!string.IsNullOrEmpty(previousWord)) {
                // are we entering a field from a known table?
                var foundTable = ParserHandler.FindAnyTableOrBufferByName(previousWord);
                if (foundTable != null) {
                    _currentTable = foundTable;
                    return TypeOfList.Fields;
                }

                // are we entering a table from a connected database?
                var foundDatabase = DataBase.FindDatabaseByName(previousWord);
                if (foundDatabase != null) {
                    _currentDataBase = foundDatabase;
                    return TypeOfList.Tables;
                }
            }

            // We want to access the methods/properties/attributes keywords?
            if (Npp.GetCharBeforeWord(curPos).Equals(":"))
                return TypeOfList.ObjectKeywords;

            return TypeOfList.Complete;
        }

        /// <summary>
        /// This method is called to switch temporarly the items of the completion form, from
        /// a complete list to only the fields of the current table
        /// </summary>
        private static void SwapToFieldsItems(ParsedTable currentTable) {
            _showingAllItems = false;
            _currentItems = DataBase.GetFieldsList(currentTable).ToList();
            UpdateFormWithCurrentItemList();
        }

        /// <summary>
        /// This method is called to switch temporarly the items of the completion form, from
        /// a complete list to only the tables of the current database
        /// </summary>
        private static void SwapToTablesItems(ParsedDataBase currentDataBase) {
            _showingAllItems = false;
            _currentItems = DataBase.GetTablesList(currentDataBase).ToList();
            UpdateFormWithCurrentItemList();
        }

        /// <summary>
        /// this method is called to switch back to the persistentItem of the completion form,
        /// after showing only fields
        /// </summary>
        private static void ResetToCompleteItems() {
            if (_showingAllItems) return;
            _showingAllItems = true;
            _currentItems = _savedAllItems;
            UpdateFormWithCurrentItemList();
        }

        /// <summary>
        /// updates the form with the _currentItem List
        /// </summary>
        private static void UpdateFormWithCurrentItemList() {
            if (_form != null)
                _form.SetItems(_currentItems);
        }

        /// <summary>
        /// This function handles the display of the autocomplete form, create or update it
        /// </summary>
        /// <param name="allowedType"></param>
        private static void ShowSuggestionList(List<CompletionType> allowedType = null) {
            if (_currentItems == null) {
                Close();
                return;
            }

            string keyword = Npp.GetKeyword();
            var point = Npp.GetCaretScreenLocation();
            var lineHeight = Npp.GetTextHeight(Npp.GetCaretLineNumber());
            point.Y += lineHeight;

            // instanciate the form if needed
            if (_form == null) {
                _form = new AutoCompletionForm(keyword) {
                    UnfocusedOpacity = Config.Instance.AutoCompleteUnfocusedOpacity,
                    FocusedOpacity = Config.Instance.AutoCompleteFocusedOpacity
                };
                _form.TabCompleted += OnTabCompleted;
                _form.Show(Npp.Win32WindowNpp);
                _form.SetItems(_currentItems);
            } else {
                // else, just sort the items, to reflect the latest ranking
                _form.SortItems();
            }

            // update position (and alternate color config)
            _form.UseAlternateBackColor = Config.Instance.AutoCompleteAlternateBackColor;
            _form.SetPosition(point, lineHeight + 2);

            // only activate certain types
            if (allowedType != null)
                _form.SetActiveType(allowedType);
            else
                _form.ResetActiveType();

            // filter with keyword (keyword can be empty)
            _form.FilterByText = keyword;
            _form.SelectFirstItem();

            if (!_form.Visible) {
                // we uncloak conditionnally
                if (!_openedFromShortCut && !Config.Instance.AutoCompleteOnKeyInputHideIfEmpty && _form.TotalItems == 0) 
                    return;

                _form.UnCloack();
            }
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
                        string.IsNullOrWhiteSpace(keywordToRep)) {
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

                    if (data.Type == CompletionType.Snippet)
                        Snippets.TriggerCodeSnippetInsertion();
                }

                Close();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error during AutoCompletionAccepted");
            }
        }

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
    }
}