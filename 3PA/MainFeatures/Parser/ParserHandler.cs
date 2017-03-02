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
using System.Threading.Tasks;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletionFeature;
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
        
        private static Timer _parserTimer;

        /// <summary>
        /// I could, and maybe i should, use a lock on those 2 booleans
        /// At least make it volatile so the compiler always takes the most updated value...
        /// </summary>
        private static volatile bool _parseRequestedWhenBusy;

        private static volatile bool _parsing;

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
        /// Returns Scope of the given line
        /// </summary>
        /// <returns></returns>
        public static ParsedScopeItem GetScopeOfLine(int line) {
            return !AblParser.LineInfo.ContainsKey(line) ? null : AblParser.LineInfo[line].Scope;
        }

        /// <summary>
        /// Returns a list of "parameters" for a given internal procedure
        /// </summary>
        /// <param name="procedureItem"></param>
        /// <returns></returns>
        public static List<CompletionItem> FindProcedureParameters(CompletionItem procedureItem) {
            var parserVisitor = ParserVisitor.GetParserVisitor(procedureItem.ParsedItem.FilePath);
            return parserVisitor.ParsedCompletionItemsList.Where(data => {
                if (data.FromParser && data.ParsedItem.Scope.Name.EqualsCi(procedureItem.DisplayText)) {
                    var item = data.ParsedItem as ParsedDefine;
                    return (item != null && item.Type == ParseDefineType.Parameter && (data.Type == CompletionType.VariablePrimitive || data.Type == CompletionType.VariableComplex || data.Type == CompletionType.Widget));
                }
                return false;
            }).ToList();
        }

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

        #region do the parsing and get the results

        /// <summary>
        /// Call this method to parse the current document after a small delay 
        /// (delay that is reset each time this function is called, so if you call it continously, nothing is done)
        /// or set doNow = true to do it without waiting a timer
        /// (you can also set forceAsync = true if doNow = true to do the parsing asynchronously, BE CAREFUL!!)
        /// </summary>
        public static void ParseCurrentDocument(bool doNow = false, bool forceAsync = false) {

            // parse immediatly
            if (doNow) {
                ParseCurrentDocumentTick(forceAsync);
                return;
            }

            // parse in 800ms, if nothing delays the timer
            if (_timerLock.TryEnterWriteLock(50)) {
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
        private static void ParseCurrentDocumentTick(bool forceAsync = false) {
            if (_parsing) {
                _parseRequestedWhenBusy = true;
                return;
            }
            _parseRequestedWhenBusy = false;
            _parsing = true;
            if (forceAsync)
                DoParse();
            else
                Task.Factory.StartNew(DoParse);
        }

        private static void DoParse() {
            try {
                if (OnParseStarted != null)
                    OnParseStarted();

                if (_parserLock.TryEnterWriteLock(200)) {
                    try {
                        // make sure to always parse the current file
                        do {
                            //var watch = Stopwatch.StartNew();

                            _lastParsedFilePath = Plug.CurrentFilePath;

                            // Parse the document
                            _ablParser = new Parser(Plug.IsCurrentFileProgress ? Npp.Text : string.Empty, _lastParsedFilePath, null, true);

                            // visitor
                            _parserVisitor = new ParserVisitor(true);
                            _ablParser.Accept(_parserVisitor);

                            //watch.Stop();
                            //UserCommunication.Notify("Updated in " + watch.ElapsedMilliseconds + " ms", 1);

                        } while (!_lastParsedFilePath.Equals(Plug.CurrentFilePath));

                    } finally {
                        _parserLock.ExitWriteLock();
                    }
                }

                if (OnParseEnded != null)
                    OnParseEnded();

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in ParseCurrentDocumentTick");
            } finally {
                _parsing = false;
                if (_parseRequestedWhenBusy)
                    ParseCurrentDocumentTick();
            }
        }

        #endregion

    }
}
