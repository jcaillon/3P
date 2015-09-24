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

        // true if we are using the _savedAllItems in _currentItems, false otherwise (= we are showing fields)
        private static bool _showingAllItems;

        // contains the list of items currently display in the form
        private static List<CompletionData> _currentItems;

        // contains the whole list of items (minus the fields) to show, can be updated through FillItems() method
        private static List<CompletionData> _savedAllItems;

        // contains the list of items that do not depend on the current file (keywords, database, snippets)
        private static List<CompletionData> _staticItems;

        // bool that indicates to the method showing the popup if the _currentItems have changed
        // and therefore as to be reloaded into the list
        private static bool _needToUpdateItemsInTheList;

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
            _savedAllItems.AddRange(ParserHandler.DynamicItems);

            // we do the sorting (by type and then by ranking)
            _savedAllItems.Sort(new CompletionDataSortingClass());

            // update autocompletion
            _currentItems = _savedAllItems;
            if (_form != null && _showingAllItems)
                _form.SetItems(_currentItems);

            _needToUpdateItemsInTheList = true;
        }

        /// <summary>
        /// this method should be called at the plugin's start and when we change the current database
        /// of each features
        /// </summary>
        public static void FillStaticItems() {
            // creates the static items list
            if (_staticItems == null) _staticItems = new List<CompletionData>();
            _staticItems.Clear();
            _staticItems = GetListOf(CompletionType.Keyword).ToList();
            _staticItems.AddRange(GetListOf(CompletionType.Snippet));
            _staticItems.AddRange(GetListOf(CompletionType.Table));
            _currentItems = _staticItems;

            // we do the sorting (by type and then by ranking)
            _staticItems.Sort(new CompletionDataSortingClass());

            if (_savedAllItems == null) {
                _savedAllItems = new List<CompletionData>();
                _savedAllItems = _staticItems.ToList();
            }

            _needToUpdateItemsInTheList = true;
        }


        /// <summary>
        /// This method is called to switch temporarly the items of the completino form, from
        /// a complete list to only the fields of the current table
        /// </summary>
        private static void SwapToFieldsItems() {
            _showingAllItems = false;
            var fieldsItems = GetListOf(CompletionType.Field, Npp.GetCurrentTable());
            _currentItems = fieldsItems;
            if (_form != null)
                _form.SetItems(_currentItems);
            _needToUpdateItemsInTheList = true;
        }

        /// <summary>
        /// this method is called to switch back to the persistentItem of the completion form,
        /// after showing only fields
        /// </summary>
        private static void ResetToCompleteItems() {
            _showingAllItems = true;
            _currentItems = _savedAllItems;
            if (_form != null)
                _form.SetItems(_currentItems);
            _needToUpdateItemsInTheList = true;
        }

        /// <summary>
        /// return a list of CompletionData object to be used in the autocompletion list
        /// </summary>
        /// <param name="compType"></param>
        /// <param name="tableName">Used only when compType is Field</param>
        /// <returns></returns>
        private static List<CompletionData> GetListOf(CompletionType compType, string tableName = "") {
            switch (compType) {
                case CompletionType.Keyword:
                    return Keywords.Get;
                case CompletionType.Table:
                    return DataBaseInfo.KeysTable.Select(x => new CompletionData { DisplayText = x, Type = CompletionType.Table }).ToList();
                case CompletionType.Field:
                    return DataBaseInfo.KeysField(tableName).Select(x => new CompletionData { DisplayText = x, Type = CompletionType.Field }).ToList();
                case CompletionType.Snippet:
                    return Snippets.Keys.Select(x => new CompletionData { DisplayText = x, Type = CompletionType.Snippet }).ToList();
            }
            return null;
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
            if (found.Type == CompletionType.Keyword ||
                found.Type == CompletionType.Field ||
                found.Type == CompletionType.FieldPk ||
                found.Type == CompletionType.Table)
                return Abl.AutoCaseToUserLiking(keyword);
            return found.DisplayText;
        }


        /// <summary>
        /// handles the opening or the closing of the autocompletion form on key input, 
        /// filter by the current input and so on..
        /// </summary>
        public static void ActivatedAutoCompleteIfNeeded() {
            string keyword = Npp.GetKeyword();

            //TODO: Dont show autocompletion if SCI_GETSELECTIONS multiple selections!

            // can we even show it?
            if (keyword.Length >= Config.Instance.AutoCompleteStartShowingListAfterXChar || IsVisible) {
                // the form is already visible
                if (IsVisible) {
                    if (!_openedFromShortCut && keyword.Length < Config.Instance.AutoCompleteStartShowingListAfterXChar)
                        Close();
                    else {
                        // if opened with fields only, show complete if the user deleted the . char
                        _form.FilterByText = keyword;

                        // close it if the list is empty and we the user selected the option to close it
                        if (!_openedFromShortCut && !Config.Instance.AutoCompleteOnKeyInputHideIfEmpty && _form.TotalItems == 0)
                            Close();
                    }
                } else {
                    if (Config.Instance.AutoCompleteShowInCommentsAndStrings || Npp.IsNormalContext(Npp.GetCaretPosition())) {
                        // are we entering a field or a normal keyword?
                        if (Npp.WeAreEnteringAField() && DataBaseInfo.ContainsTable(Npp.GetCurrentTable()))
                            ShowFieldsSuggestions(false);
                        else
                            ShowCompleteSuggestionList(false);
                    }
                }
            } else {
                Close();
            }
        }

        public static void ShowCompleteSuggestionList() {
            ShowCompleteSuggestionList(true);
        }

        public static void ShowCompleteSuggestionList(bool displayFromShortCut) {
            try {
                if (Npp.WeAreEnteringAField() && DataBaseInfo.ContainsTable(Npp.GetCurrentTable()))
                    ShowFieldsSuggestions(displayFromShortCut);
                else {
                    if (!_showingAllItems) ResetToCompleteItems();
                    ShowSuggestionList(displayFromShortCut);
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in ShowCompleteSuggestionList");
            }
        }

        public static void ShowSnippetsList() {
            try {
                if (!_showingAllItems) ResetToCompleteItems();
                ShowSuggestionList(true, new List<CompletionType> { CompletionType.Snippet });
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in ShowSnippetsList");
            }
        }

        public static void ShowFieldsSuggestions(bool displayFromShortCut) {
            try {
                SwapToFieldsItems();
                ShowSuggestionList(displayFromShortCut, new List<CompletionType> { CompletionType.FieldPk, CompletionType.Field });
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in ShowFieldsSuggestions");
            }
        }

        /// <summary>
        /// This function handles the display of the autocomplete form
        /// </summary>
        /// <param name="displayFromShortCut"></param>
        /// <param name="allowedType"></param>
        private static void ShowSuggestionList(bool displayFromShortCut, List<CompletionType> allowedType = null) {
            _openedFromShortCut = displayFromShortCut;

            if (_currentItems == null || !_currentItems.Any()) {
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
            } else if (_needToUpdateItemsInTheList) {
                _form.SetItems(_currentItems);
                _needToUpdateItemsInTheList = false;
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

                    Npp.BeginUndoAction();
                    Npp.ReplaceKeyword(data.DisplayText, keywordPos);
                    Npp.EndUndoAction();

                    if (data.Type == CompletionType.Keyword)
                        Keywords.RemberUseOf(data.DisplayText);

                    if (data.Type == CompletionType.Table)
                        DataBaseInfo.RememberUseOfTable(data.DisplayText);

                    if (data.Type == CompletionType.Field)
                        DataBaseInfo.RememberUseOfField(Npp.GetCurrentTable(), data.DisplayText);

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

    #region sorting

    /// <summary>
    /// Class used in objectlist.Sort method
    /// </summary>
    public class CompletionDataSortingClass : IComparer<CompletionData> {
        public int Compare(CompletionData x, CompletionData y) {
            int compare = x.Type.CompareTo(y.Type);
            if (compare == 0) {
                return y.Ranking.CompareTo(x.Ranking);
            }
            return compare;
        }
    }
    #endregion
}