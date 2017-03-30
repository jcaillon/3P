#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (ParserHandler.cs) is part of 3P.
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
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletionFeature;
using _3PA.MainFeatures.CodeExplorer;
using _3PA.MainFeatures.Pro;
using _3PA.NppCore;

namespace _3PA.MainFeatures.Parser {

    internal static class ParserHandler {

        #region event

        /// <summary>
        /// Event published when the parser starts doing its job
        /// </summary>
        public static event Action OnStart;

        /// <summary>
        /// Event published when the parser has done its job and it's time to get the results
        /// </summary>
        public static event Action OnEnd;

        /// <summary>
        /// Event published when the parser has done its job and it's time to get the results
        /// </summary>
        public static event Action<List<CodeItem>> OnEndSendCodeExplorerItems;

        /// <summary>
        /// Event published when the parser has done its job and it's time to get the results
        /// </summary>
        public static event Action<List<CompletionItem>> OnEndSendCompletionItems;

        /// <summary>
        /// Event published when the parser has done its job and it's time to get the results
        /// </summary>
        public static event Action<List<ParserError>, Dictionary<int, LineInfo>, List<ParsedItem>> OnEndSendParserItems;

        #endregion

        #region Private fields

        private static string _lastFilePathParsed;
        private static Dictionary<int, LineInfo> _lineInfo = new Dictionary<int, LineInfo>();

        private static AsapButDelayableAction _parseAction;

        private static AsapButDelayableAction ParseAction {
            get { return _parseAction ?? (_parseAction = new AsapButDelayableAction(800, DoParse)); }
        }

        private static object _lock = new object();

        #endregion

        #region Public

        /// <summary>
        /// Returns Scope of the given line
        /// </summary>
        /// <returns></returns>
        public static ParsedScopeItem GetScopeOfLine(int line) {
            ParsedScopeItem output = null;
            DoInLock(() => { output = _lineInfo != null && _lineInfo.ContainsKey(line) ? _lineInfo[line].Scope : null; });
            return output;
        }

        /// <summary>
        /// Call this method to parse the current document after a small delay 
        /// (delay that is reset each time this function is called, so if you call it continuously, nothing is done)
        /// </summary>
        public static void ParseDocumentAsap() {
            ParseAction.DoDelayable();
        }

        /// <summary>
        /// Parse the document now (still async, just skip the timer)
        /// </summary>
        public static void ParseDocumentNow() {
            ParseAction.DoTaskNow();
        }

        private static void DoParse() {
            try {
                if (OnStart != null)
                    OnStart();

                DoInLock(() => {
                    // make sure to always parse the current file
                    Parser parser = null;
                    do {
                        _lastFilePathParsed = Npp.CurrentFile.Path;

                        if (Npp.CurrentFile.IsProgress) {
                            parser = new Parser(Sci.Text, _lastFilePathParsed, null, true);

                            // visitor
                            var visitor = new ParserVisitor(true);
                            parser.Accept(visitor);

                            // send completionItems
                            if (OnEndSendCompletionItems != null)
                                OnEndSendCompletionItems(visitor.ParsedCompletionItemsList);

                            // send codeExplorerItems
                            if (OnEndSendCodeExplorerItems != null)
                                OnEndSendCodeExplorerItems(visitor.ParsedExplorerItemsList);

                        } else {
                            var textLexer = new TextLexer(Sci.GetTextAroundFirstVisibleLine(Config.Instance.NppAutoCompleteMaxLengthToParse), AutoCompletion.CurrentLangAdditionalChars);
                            var textVisitor = new TextLexerVisitor() {
                                IgnoreNumbers = Config.Instance.NppAutoCompleteIgnoreNumbers,
                                MinWordLengthRequired = Config.Instance.NppAutoCompleteMinWordLengthRequired,
                                KnownWords = KnownWords != null ? new HashSet<string>(KnownWords, AutoCompletion.ParserStringComparer) : new HashSet<string>(AutoCompletion.ParserStringComparer)
                            };
                            textLexer.Accept(textVisitor);

                            // send completionItems
                            if (OnEndSendCompletionItems != null)
                                OnEndSendCompletionItems(textVisitor.ParsedCompletionItemsList);

                            // send codeExplorerItems
                            if (OnEndSendCodeExplorerItems != null)
                                OnEndSendCodeExplorerItems(null);
                        }
                    } while (!_lastFilePathParsed.Equals(Npp.CurrentFile.Path));

                    if (parser != null) {
                        _lineInfo = new Dictionary<int, LineInfo>(parser.LineInfo);
                    }

                    // send parserItems
                    if (OnEndSendParserItems != null) {
                        if (parser != null)
                            OnEndSendParserItems(parser.ParserErrors, parser.LineInfo, parser.ParsedItemsList);
                        else
                            OnEndSendParserItems(null, null, null);
                    }
                });

                if (OnEnd != null)
                    OnEnd();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error while analyzing the current document");
            }
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

        #endregion

        #region Static data

        /// <summary>
        /// Set this function to return the full file path of an include (the parameter is the file name of partial path /folder/include.i)
        /// </summary>
        public static Func<string, string> FindIncludeFullPath = s => ProEnvironment.Current.FindFirstFileInPropath(s);

        /// <summary>
        /// Instead of parsing the include files each time we store the results of the lexer to use them when we need it
        /// </summary>
        public static Dictionary<string, ProLexer> SavedLexerInclude = new Dictionary<string, ProLexer>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// A dictionary of known keywords and database info
        /// </summary>
        public static Dictionary<string, CompletionType> KnownStaticItems = new Dictionary<string, CompletionType>();

        /// <summary>
        /// We keep tracks of the parsed files, to avoid parsing the same file twice
        /// </summary>
        public static HashSet<string> RunPersistentFiles = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Instead of parsing the persistent files each time we store the results of the parsing to use them when we need it
        /// </summary>
        public static Dictionary<string, ParserVisitor> SavedPersistent = new Dictionary<string, ParserVisitor>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// List of known words for the current language
        /// </summary>
        public static HashSet<string> KnownWords;

        /// <summary>
        /// Clear the static data to save up some memory
        /// </summary>
        public static void ClearStaticData() {
            DoInLock(() => {
                RunPersistentFiles.Clear();
                SavedPersistent.Clear();
                SavedLexerInclude.Clear();
            });
        }

        public static void UpdateKnownStaticItems(List<CompletionItem> staticItems) {
            DoInLock(() => {
                // Update the known items! (made of BASE.TABLE, TABLE and all the KEYWORDS)
                KnownStaticItems = new Dictionary<string, CompletionType>(StringComparer.CurrentCultureIgnoreCase);
                KnownStaticItems = DataBase.Instance.GetDbDictionary(KnownStaticItems);
                foreach (var keyword in Keywords.Instance.CompletionItems.Where(keyword => !KnownStaticItems.ContainsKey(keyword.DisplayText))) {
                    KnownStaticItems.Add(keyword.DisplayText, keyword.Type);
                }

                // Non progress files, known words
                if (staticItems != null) {
                    KnownWords = new HashSet<string>(AutoCompletion.ParserStringComparer);
                    foreach (var item in staticItems.Where(item => !KnownWords.Contains(item.DisplayText))) {
                        KnownWords.Add(item.DisplayText);
                    }
                }
            });
        }

        #endregion
    }
}