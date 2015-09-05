using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using _3PA.Interop;
using _3PA.Lib;

namespace _3PA.MainFeatures {

    internal class AutoComplete {

        public static bool IsLastWordInDico(string keyword, int curPos, int offset) {
            return Keywords.Contains(keyword)
                   || DataBaseInfo.ContainsTable(keyword)
                   || (Npp.WeAreEnteringAField(curPos - offset)
                       && DataBaseInfo.ContainsField(Npp.GetCurrentTable(curPos - offset), keyword));
        }

        public static bool IsShowingAutocompletion {
            get { return GetForm != null && GetForm.Visible; }
        }

        public static Forms.AutoComplete GetForm { get; private set; }

        public static void CloseSuggestionList() {
            try {
                GetForm.Hide();
                GetForm.Close();
            } catch (Exception) {
                // ignored
            }
        }

        public static bool ActivatedAutoCompleteIfNeeded() {
            bool activated = false;

            string keyword = Npp.GetKeyword();

            // either start to show the suggestion list, or filter an existing one
            if (keyword.Length >= Config.Instance.AutoCompleteStartShowingListAfterXChar || IsShowingAutocompletion) {
                if (IsShowingAutocompletion) {
                    if (!GetForm.DisplayFromShortcut && keyword.Length < Config.Instance.AutoCompleteStartShowingListAfterXChar)
                        CloseSuggestionList();
                    else
                        FilterSuggestionList(keyword);
                } else {
                    if (Config.Instance.AutoCompleteShowInCommentsAndStrings || Npp.IsNormalContext(Npp.GetCaretPosition())) {
                        // are we entering a field or a normal keyword?
                        if (Npp.WeAreEnteringAField() && DataBaseInfo.ContainsTable(Npp.GetCurrentTable()))
                            ShowFieldsSuggestions(false);
                        else
                            ShowCompleteSuggestionList(false);
                    }
                }
                activated = true;
            } else {
                CloseSuggestionList();
            }

            return activated;   
        }

        /// <summary>
        ///  Called to filter the current autocomplete form
        /// </summary>
        /// <param name="keyword"></param>
        public static void FilterSuggestionList(string keyword) {
            try {
                if (IsShowingAutocompletion) {
                    GetForm.FilterFor(keyword);
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

            if (items.Any()) {
                string keyword = Npp.GetKeyword();
                Action<CompletionData> onAccepted = OnAutocompletionAccepted;
                GetForm = new Forms.AutoComplete(onAccepted, items.ToList(), displayFromShortCut);

                var point = Npp.GetCaretScreenLocation();
                GetForm.Left = point.X;
                GetForm.Top = point.Y + Npp.GetTextHeight(Npp.GetCaretLineNumber());

                Dispatcher.Shedule(10, () => {
                    GetForm.Show(Npp.Win32WindowNpp);
                    FilterSuggestionList(keyword);
                    Npp.GrabFocus();
                });
            }
        }

        /// <summary>
        ///     Called when the user triggers the selection of a keyword from the autocomplete form
        /// </summary>
        /// <param name="data"></param>
        private static void OnAutocompletionAccepted(CompletionData data) {
            try {
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
                case CompletionType.UserVariable:
                    return null;
                case CompletionType.Function:
                    return null;
                case CompletionType.Procedure:
                    return null;
                case CompletionType.Special:
                    return null;
            }
            return null;
        }
    }
}