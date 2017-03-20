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
using System.Text;
using System.Threading;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletionFeature;
using _3PA.MainFeatures.CodeExplorer;
using _3PA.MainFeatures.Pro;
using _3PA.NppCore;

namespace _3PA.MainFeatures.Parser {

    internal static class ParserHandler {

        #region Core

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
        public static event Action<List<CodeExplorerItem>> OnEndSendCodeExplorerItems;

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

        private static Dictionary<int, LineInfo> _lineInfo = new Dictionary<int, LineInfo>();
        private static List<ParserError> _parserErrors = new List<ParserError>();
        private static List<ParsedItem> _parsedItemsList = new List<ParsedItem>();

        private static AsapButDelayableAction _parseAction;

        private static AsapButDelayableAction ParseAction {
            get {
                return _parseAction ?? (_parseAction = new AsapButDelayableAction(800, DoParse) {
                    MsToDoTimeout = 2000
                });
            }
        }

        private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        #endregion

        #region do the parsing and get the results

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

        /// <summary>
        /// Wait for the latest parsing action to be done
        /// </summary>
        public static void WaitForParserEnd() {
            //ParseAction.WaitLatestTask();
        }

        /// <summary>
        /// Parses the document synchronously (handle with care!!!!)
        /// </summary>
        public static void ParseDocumentSync() {
            ParseAction.DoSync();
        }

        private static void DoParse() {
            string lastParsedFilePath = null;
            try {
                if (OnStart != null)
                    OnStart();
                
                Parser parser = null;
                bool lastParsedFileIsProgress;

                // make sure to always parse the current file
                do {

                    lastParsedFilePath = Npp.CurrentFile.Path;
                    lastParsedFileIsProgress = Npp.CurrentFile.IsProgress;

                    if (lastParsedFileIsProgress) {
                        parser = new Parser(Sci.Text, lastParsedFilePath, null, true);
                    } else {
                        //
                    }

                } while (!lastParsedFilePath.Equals(Npp.CurrentFile.Path));


                if (lastParsedFileIsProgress) {
                    // visitor
                    var visitor = new ParserVisitor(true);
                    parser.Accept(visitor);

                    // send completionItems
                    if (OnEndSendCompletionItems != null)
                        OnEndSendCompletionItems(visitor.ParsedCompletionItemsList);

                    // send codeExplorerItems
                    if (OnEndSendCodeExplorerItems != null)
                        OnEndSendCodeExplorerItems(visitor.ParsedExplorerItemsList);
                }


                if (_lock.TryEnterWriteLock(-1)) {
                    try {
                        if (lastParsedFileIsProgress) {
                            _parserErrors = parser.ParserErrors;
                            _lineInfo = parser.LineInfo;
                            _parsedItemsList = parser.ParsedItemsList;
                        } else {
                            _parserErrors = new List<ParserError>();
                            _lineInfo = new Dictionary<int, LineInfo>();
                            _parsedItemsList = new List<ParsedItem>();
                        }

                    } finally {
                        _lock.ExitWriteLock();
                    }
                }


                // send parserItems
                if (OnEndSendParserItems != null)
                    OnEndSendParserItems(_parserErrors, _lineInfo, _parsedItemsList);

                if (OnEnd != null)
                    OnEnd();

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in DoParse " + (lastParsedFilePath ?? "?"));
            }
        }

        #endregion

        #endregion

        #region Static data
        
        /// <summary>
        /// Set this function to return the full file path of an include (the parameter is the file name of partial path /folder/include.i)
        /// </summary>
        public static Func<string, string> FindIncludeFullPath = s => ProEnvironment.Current.FindFirstFileInPropath(s);

        /// <summary>
        /// Instead of parsing the include files each time we store the results of the lexer to use them when we need it
        /// </summary>
        public static Dictionary<string, Lexer> SavedLexerInclude = new Dictionary<string, Lexer>(StringComparer.CurrentCultureIgnoreCase);

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
        /// Clear the static data to save up some memory
        /// </summary>
        public static void ClearStaticData() {
            WaitForParserEnd();
            RunPersistentFiles.Clear();
            SavedPersistent.Clear();
            SavedLexerInclude.Clear();
        }

        public static void UpdateKnownStaticItems() {
            WaitForParserEnd();
            // Update the known items! (made of BASE.TABLE, TABLE and all the KEYWORDS)
            KnownStaticItems = DataBase.GetDbDictionary();
            foreach (var keyword in Keywords.Instance.CompletionItems.Where(keyword => !KnownStaticItems.ContainsKey(keyword.DisplayText))) {
                KnownStaticItems[keyword.DisplayText] = keyword.Type;
            }
        }

        #endregion

        #region Public

        /// <summary>
        /// dictionary of *line, line info*
        /// </summary>
        public static Dictionary<int, LineInfo> LineInfo {
            get {
                if (_lock.TryEnterReadLock(-1)) {
                    try {
                        return new Dictionary<int, LineInfo>(_lineInfo);
                    } finally {
                        _lock.ExitReadLock();
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// returns the list of the parsed items
        /// </summary>
        public static List<ParsedItem> ParsedItemsList {
            get {
                if (_lock.TryEnterReadLock(-1)) {
                    try {
                        return _parsedItemsList.ToList();
                    } finally {
                        _lock.ExitReadLock();
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Returns Scope of the given line
        /// </summary>
        /// <returns></returns>
        public static ParsedScopeItem GetScopeOfLine(int line) {
            if (_lock.TryEnterReadLock(-1)) {
                try {
                    return !_lineInfo.ContainsKey(line) ? null : _lineInfo[line].Scope;
                } finally {
                    _lock.ExitReadLock();
                }
            }
            return null;
        }


        /// <summary>
        /// finds a ParsedTable for the input name, it can either be a database table,
        /// a temp table, or a buffer name (in which case we return the associated table)
        /// </summary>
        public static ParsedTable FindAnyTableOrBufferByName(string name) {
            if (_lock.TryEnterReadLock(-1)) {
                try {
                    return ParserUtils.FindAnyTableOrBufferByName(name, _parsedItemsList);
                } finally {
                    _lock.ExitReadLock();
                }
            }
            return null;
        }

        /// <summary>
        /// Returns a string that describes the errors found by the parser (relative to block start/end)
        /// Returns null if no errors were found
        /// </summary>
        public static string GetLastParseErrorsInHtml() {
            WaitForParserEnd();
            if (_parserErrors == null || _parserErrors.Count == 0)
                return null;
            var error = new StringBuilder();
            foreach (var parserError in _parserErrors) {
                error.AppendLine("<div>");
                error.AppendLine("- " + (parserError.FullFilePath + "|" + parserError.TriggerLine).ToHtmlLink("Line " + (parserError.TriggerLine + 1)) + ", " + parserError.Type.GetDescription());
                error.AppendLine("</div>");
            }
            return error.ToString();
        }

        #endregion
    }
}