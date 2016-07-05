#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.CodeExplorer;
using Timer = System.Timers.Timer;

namespace _3PA.MainFeatures.Parser {

    internal static class ParserHandler {

        #region event
        
        /// <summary>
        /// Event published when the parser starts doing its job
        /// </summary>
        public static event Action OnParseStarted;

        /// <summary>
        /// Event published when the parser has done its job and it's time to get the results
        /// </summary>
        public static event Action OnParseEnded;
        
        #endregion

        #region fields

        private static string _lastParsedFilePath;

        private static Parser _ablParser = new Parser();

        private static ParserVisitor _parserVisitor = new ParserVisitor();

        private static ReaderWriterLockSlim _parserLock = new ReaderWriterLockSlim();

        private static ReaderWriterLockSlim _timerLock = new ReaderWriterLockSlim();

        private static ReaderWriterLockSlim _parsingLock = new ReaderWriterLockSlim();

        private static Timer _parserTimer;

        private static volatile bool _parseRequestedWhenBusy;

        #endregion

        #region public accessors (thread safe)

        /// <summary>
        /// Access the parser
        /// </summary>
        public static Parser AblParser {
            get {
                if (_parserLock.TryEnterReadLock(-1)) {
                    try {
                        return _ablParser;
                    } finally {
                        _parserLock.ExitReadLock();
                    }
                }
                return new Parser();
            }
        }

        /// <summary>
        /// Access the parser visitor
        /// </summary>
        public static ParserVisitor ParserVisitor {
            get {
                if (_parserLock.TryEnterReadLock(-1)) {
                    try {
                        return _parserVisitor;
                    } finally {
                        _parserLock.ExitReadLock();
                    }
                }
                return new ParserVisitor();
            }
        }

        #endregion

        #region Public

        /// <summary>
        /// List of completion item found
        /// </summary>
        /// <returns></returns>
        public static List<CompletionItem> CompletionItemsList {
            get { return ParserVisitor.ParsedItemsList.ToList(); }
        }

        /// <summary>
        /// List of parsed explorer items
        /// </summary>
        /// <returns></returns>
        public static List<CodeExplorerItem> CodeExplorerItemsList {
            get { return ParserVisitor.ParsedExplorerItemsList.ToList(); }
        }

        /// <summary>
        /// List of parsed items
        /// </summary>
        public static List<ParsedItem> ParsedItemsList {
            get { return AblParser.ParsedItemsList.ToList(); }
        }

        /// <summary>
        /// Returns true if the parser detected a syntax correct enough for it to indent the ABL code of the parsed document
        /// </summary>
        /// <returns></returns>
        public static bool CanIndent {
            get { return AblParser.ParsingOk; }
        }

        /// <summary>
        /// Returns true if the parser detected a syntax correct enough for it to indent the ABL code of the parsed document
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, LineInfo> GetLineInfo {
            get { return AblParser.LineInfo; }
        }

        /// <summary>
        /// Returns the owner name (currentScopeName) of the caret line
        /// </summary>
        /// <returns></returns>
        public static string GetCarretLineOwnerName(int line) {
            return !AblParser.LineInfo.ContainsKey(line) ? string.Empty : AblParser.LineInfo[line].CurrentScopeName;
        }

        /// <summary>
        /// Returns a list of "parameters" for a given internal procedure
        /// </summary>
        /// <param name="procedureItem"></param>
        /// <returns></returns>
        public static List<CompletionItem> FindProcedureParameters(CompletionItem procedureItem) {
            var parserVisitor = ParserVisitor.ParseFile(procedureItem.ParsedItem.FilePath, "");
            return parserVisitor.ParsedItemsList.Where(data =>
                data.FromParser &&
                data.ParsedItem.OwnerName.EqualsCi(procedureItem.DisplayText) &&
                (data.Type == CompletionType.VariablePrimitive || data.Type == CompletionType.VariableComplex || data.Type == CompletionType.Widget) &&
                ((ParsedDefine)data.ParsedItem).Type == ParseDefineType.Parameter).ToList();
        }

        #endregion

        #region do the parsing and get the results

        /// <summary>
        /// Call this method to parse the current document after a small delay 
        /// (delay that is reset each time this function is called, so if you call it continously, nothing is done)
        /// or set doNow = true to do it without waiting a timer
        /// </summary>
        public static void ParseCurrentDocument(bool doNow = false) {

            // parse immediatly
            if (doNow) {
                ParseCurrentDocumentTick();
                return;
            }

            // parse in 800ms, if nothing delays the timer
            if (_timerLock.TryEnterWriteLock(100)) {
                try {
                    if (_parserTimer == null) {
                        _parserTimer = new Timer { AutoReset = false, Interval = 800 };
                        _parserTimer.Elapsed += (sender, args) => ParseCurrentDocumentTick();
                        _parserTimer.Start();
                    } else {
                        // reset timer
                        _parserTimer.Stop();
                        _parserTimer.Start();
                    }
                } finally {
                    _timerLock.ExitWriteLock();
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
                if (_parsingLock.TryEnterWriteLock(100)) {
                    _parseRequestedWhenBusy = false;
                    try {
                        if (OnParseStarted != null)
                            OnParseStarted();

                        // make sure to always parse the current file
                        do {
                            // we launch the parser, that will fill the DynamicItems
                            if (_parserLock.TryEnterWriteLock(200)) {
                                try {
                                    RefreshParser();
                                } finally {
                                    _parserLock.ExitWriteLock();
                                }
                            } else {
                                _parseRequestedWhenBusy = true;
                            }
                        } while (!_lastParsedFilePath.Equals(Plug.CurrentFilePath) || _parseRequestedWhenBusy);

                        if (OnParseEnded != null)
                            OnParseEnded();

                    } catch (Exception e) {
                        ErrorHandler.ShowErrors(e, "Error in ParseCurrentDocumentTick");
                    } finally {
                        _parsingLock.ExitWriteLock();
                    }
                } else {
                    _parseRequestedWhenBusy = true;
                }
            });
        }

        /// <summary>
        /// this method should be called to refresh the Items list with all the static items
        /// as well as the dynamic items found by the parser
        /// </summary>
        private static void RefreshParser() {

            //var watch = Stopwatch.StartNew();

            // if this document is in the Saved parsed visitors, we remove it because it might change so we want to re parse it later
            _lastParsedFilePath = Plug.CurrentFilePath;
            if (ParserVisitor.SavedParserVisitors.ContainsKey(_lastParsedFilePath))
                ParserVisitor.SavedParserVisitors.Remove(_lastParsedFilePath);

            // Parse the document
            _ablParser = new Parser(Plug.IsCurrentFileProgress ? Npp.Text : string.Empty, _lastParsedFilePath, null, true);

            // visitor
            _parserVisitor = new ParserVisitor(true, Path.GetFileName(_lastParsedFilePath), _ablParser.LineInfo);
            _ablParser.Accept(_parserVisitor);

            // correct the internal/external type of run statements :
            foreach (var item in _parserVisitor.ParsedExplorerItemsList.Where(item => item.Branch == CodeExplorerBranch.Run)) {
                if (_parserVisitor.DefinedProcedures.Contains(item.DisplayText))
                    item.IconType = CodeExplorerIconType.RunInternal;
            }

            // save the info for uses in an another file, where this file is run in persistent or included
            if (!ParserVisitor.SavedParserVisitors.ContainsKey(_lastParsedFilePath))
                ParserVisitor.SavedParserVisitors.Add(_lastParsedFilePath, _parserVisitor);
            else
                ParserVisitor.SavedParserVisitors[_lastParsedFilePath] = _parserVisitor;

            //watch.Stop();
            //UserCommunication.Notify("Updated in " + watch.ElapsedMilliseconds + " ms", 1);
        }

        #endregion

        #region find table, buffer, temptable

        /// <summary>
        /// finds a ParsedTable for the input name, it can either be a database table,
        /// a temptable, or a buffer name (in which case we return the associated table)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ParsedTable FindAnyTableOrBufferByName(string name) {
            return ParserVisitor.FindAnyTableOrBufferByName(name);
        }

        #endregion

        #region find primitive type

        /// <summary>
        /// convertion
        /// </summary>
        public static ParsedPrimitiveType ConvertStringToParsedPrimitiveType(string str, bool analyseLike) {
            return ParserVisitor.ConvertStringToParsedPrimitiveType(str, analyseLike);
        }

        #endregion
    }
}
