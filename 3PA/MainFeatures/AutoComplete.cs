using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using YamuiFramework.Helper;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;

namespace _3PA.MainFeatures {

    /// <summary>
    /// This class manipulate the AutoCompletionForm
    /// </summary>
    internal class AutoComplete {

        /// <summary>
        /// Was the autocompletion opened naturally or from the user shortkey?
        /// </summary>
        private static bool _openedFromShortCut;

        public static bool IsLastWordInDico(string keyword, int curPos, int offset) {
            return Keywords.Contains(keyword)
                   || DataBaseInfo.ContainsTable(keyword)
                   || (Npp.WeAreEnteringAField(curPos - offset)
                       && DataBaseInfo.ContainsField(Npp.GetCurrentTable(curPos - offset), keyword));
        }

        public static bool IsShowingAutocompletion {
            get { return GetForm != null && GetForm.Visible; }
        }

        public static AutoCompletionForm GetForm { get; private set; }

        public static void CloseSuggestionList() {
            try {
                GetForm.Close();
                GetForm = null;
                _openedFromShortCut = false;
            } catch (Exception) {
                // ignored
            }
        }

        public static void ActivatedAutoCompleteIfNeeded() {
            string keyword = Npp.GetKeyword();

            // either start to show the suggestion list, or filter an existing one
            if (keyword.Length >= Config.Instance.AutoCompleteStartShowingListAfterXChar || IsShowingAutocompletion) {
                if (IsShowingAutocompletion) {
                    if (!_openedFromShortCut && keyword.Length < Config.Instance.AutoCompleteStartShowingListAfterXChar)
                        CloseSuggestionList();
                    else
                        FilterSuggestionList(keyword);
                } else {
                    if (Config.Instance.AutoCompleteShowInCommentsAndStrings || Npp.IsNormalContext(Npp.GetCaretPosition())) {
                        _openedFromShortCut = false;

                        // are we entering a field or a normal keyword?
                        if (Npp.WeAreEnteringAField() && DataBaseInfo.ContainsTable(Npp.GetCurrentTable()))
                            ShowFieldsSuggestions(false);
                        else
                            ShowCompleteSuggestionList(false);
                    }
                }
            } else {
                CloseSuggestionList();
            }
        }

        /// <summary>
        ///  Called to filter the current autocomplete form
        /// </summary>
        /// <param name="keyword"></param>
        public static void FilterSuggestionList(string keyword) {
            try {
                if (IsShowingAutocompletion) {
                    GetForm.FilterByText = keyword;
                }
            } catch (Exception e) {
                Plug.ShowErrors(e, "Error in FilterSuggestionList");
            }
        }

        public static void ShowCompleteSuggestionList() {
            ShowCompleteSuggestionList(true);
        }

        public static void ShowCompleteSuggestionList(bool displayFromShortCut) {
            try {
                if (Npp.IsCurrentProgressFile()) {
                    List<CompletionData> items;

                    if (Npp.WeAreEnteringAField() && DataBaseInfo.ContainsTable(Npp.GetCurrentTable())) {
                        // list of fields
                        items = GetListOf(CompletionType.Field ,Npp.GetCurrentTable());
                    } else {
                        items = GetListOf(CompletionType.Keyword);
                        items.AddRange(GetListOf(CompletionType.Snippet));
                        items.AddRange(GetListOf(CompletionType.Table));
                    }
                    ShowSuggestionList(items, displayFromShortCut);
                } else {
                    //Win32.SendMessage(Npp.HandleNpp, (NppMsg) WinMsg.WM_COMMAND, (int) NppMenuCmd.IDM_EDIT_AUTOCOMPLETE, 0);
                }
            } catch (Exception e) {
                Plug.ShowErrors(e, "Error in ShowCompleteSuggestionList");
            }
        }

        public static void ShowSnippetsList() {
            try {
                if (Npp.IsCurrentProgressFile()) {
                    ShowSuggestionList(GetListOf(CompletionType.Snippet), true);
                } else {
                    // triggers the usual command from notepad ++ since we intercepted it
                    //Win32.SendMessage(Npp.HandleNpp, (NppMsg) WinMsg.WM_COMMAND, (int) NppMenuCmd.IDM_EDIT_FUNCCALLTIP, 0);
                }
            } catch (Exception e) {
                Plug.ShowErrors(e, "Error in ShowSnippetsList");
            }
        }

        public static void ShowFieldsSuggestions(bool displayFromShortCut) {
            try {
                if (Npp.IsCurrentProgressFile()) {
                    ShowSuggestionList(GetListOf(CompletionType.Field ,Npp.GetCurrentTable()), displayFromShortCut);
                }
            } catch (Exception e) {
                Plug.ShowErrors(e, "Error in ShowFieldsSuggestions");
            }
        }

        public static void ShowTablesSuggestions() {
            try {
                if (Npp.IsCurrentProgressFile()) {
                    ShowSuggestionList(GetListOf(CompletionType.Table), true);
                }
            } catch (Exception e) {
                Plug.ShowErrors(e, "Error in ShowTablesSuggestions");
            }
        }

        /// <summary>
        /// This function handles the display of the autocomplete form
        /// </summary>
        /// <param name="items"></param>
        /// <param name="displayFromShortCut"></param>
        private static void ShowSuggestionList(List<CompletionData> items, bool displayFromShortCut) {
            CloseSuggestionList();
            _openedFromShortCut = displayFromShortCut;

            if (items.Any()) {
                string keyword = Npp.GetKeyword();
                var point = Npp.GetCaretScreenLocation();
                var lineHeight = Npp.GetTextHeight(Npp.GetCaretLineNumber());
                point.Y += lineHeight;
                GetForm = new AutoCompletionForm(items, point, lineHeight, keyword, Config.Instance.AutoCompleteOpacityUnfocused, Config.Instance.AutoCompleteShowListOfXSuggestions);

                GetForm.TabCompleted += GetFormOnTabCompleted;
                GetForm.CurrentForegroundWindow = WinApi.GetForegroundWindow();
                GetForm.Show(Npp.Win32WindowNpp);

                //Dispatcher.Shedule(10, () => {
                //    GetForm.Show(Npp.Win32WindowNpp);
                //    FilterSuggestionList(keyword);
                //    Npp.GrabFocus();
                //});
            }
        }

        private static void GetFormOnTabCompleted(object sender, TabCompletedEventArgs tabCompletedEventArgs) {
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

                CloseSuggestionList();
            } catch (Exception e) {
                Plug.ShowErrors(e, "Error during AutoCompletionAccepted");
            }
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
                    return Keywords.Keys.Select(x => new CompletionData { DisplayText = x, Type = CompletionType.Keyword }).ToList();
                case CompletionType.Table:
                    return DataBaseInfo.KeysTable.Select(x => new CompletionData { DisplayText = x, Type = CompletionType.Table }).ToList();
                case CompletionType.Field:
                    return DataBaseInfo.KeysField(tableName).Select(x => new CompletionData { DisplayText = x, Type = CompletionType.Field }).ToList();
                case CompletionType.Snippet:
                    return Snippets.Keys.Select(x => new CompletionData { DisplayText = x, Type = CompletionType.Snippet }).ToList();
            }
            return null;
        }
    }
}