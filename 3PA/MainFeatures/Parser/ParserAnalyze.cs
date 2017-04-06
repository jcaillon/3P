#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (ParserAnalyze.cs) is part of 3P.
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
using System.Text;
using YamuiFramework.Helper;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletionFeature;

namespace _3PA.MainFeatures.Parser {
    internal partial class Parser {
        #region Analyze

        private void Analyze() {
            var token = PeekAt(0);

            // reached end of file
            if (token is TokenEof)
                return;

            // starting a new statement, we need to remember its starting line
            if (_context.StatementFirstToken == null && (
                    token is TokenWord ||
                    token is TokenPreProcDirective ||
                    token is TokenInclude))
                _context.StatementFirstToken = token;

            // matching a word
            if (token is TokenWord) {
                var lowerTok = token.Value.ToLower();

                // Match split keywords...
                var nextToken = PeekAt(1);
                while (nextToken is TokenSymbol && nextToken.Value.Equals("~")) {
                    MoveNext(); // ~
                    MoveNext(); // Eol
                    MoveNext(); // the keyword part
                    token = PeekAt(0);
                    lowerTok += token.Value.ToLower();
                    nextToken = PeekAt(1);
                }

                // first word of a statement
                if (_context.StatementWordCount == 0) {
                    // matches a definition statement at the beginning of a statement
                    switch (lowerTok) {
                        case "case":
                        case "catch":
                        case "class":
                        case "constructor":
                        case "destructor":
                        case "finally":
                        case "interface":
                        case "method":
                        case "for":
                        case "do":
                        case "repeat":
                        case "editing":
                            // increase block depth
                            PushBlockInfoToStack(IndentType.DoEnd, token.Line);
                            break;
                        case "end":
                            if (_context.BlockStack.Count > 0) {
                                // decrease block depth
                                var popped = _context.BlockStack.Pop();

                                // in case of a then do: we have created 2 stacks for actually the same block, pop them both
                                if (_context.BlockStack.Count > 0 &&
                                    _context.BlockStack.Peek().IndentType == IndentType.ThenElse &&
                                    popped.LineTriggerWord == _context.BlockStack.Peek().LineTriggerWord)
                                    _context.BlockStack.Pop();
                            } else
                                _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedBlockEnd, token, 0, _parsedIncludes));
                            break;
                        case "else":
                            // add a one time indent after a then or else
                            PushBlockInfoToStack(IndentType.ThenElse, token.Line);
                            NewStatement(token);
                            break;
                        case "define":
                        case "def":
                            // Parse a define statement
                            CreateParsedDefine(token, false);
                            break;
                        case "create":
                            // Parse a create statement
                            CreateParsedDefine(token, true);
                            break;
                        case "procedure":
                        case "proce":
                            // parse a procedure definition
                            if (CreateParsedProcedure(token)) {
                                if (_context.BlockStack.Count != 0)
                                    _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedBlockStart, token, _context.BlockStack.Count, _parsedIncludes));
                                _context.BlockStack.Clear();
                                PushBlockInfoToStack(IndentType.DoEnd, token.Line);
                            }
                            break;
                        case "function":
                            // parse a function definition
                            if (CreateParsedFunction(token)) {
                                if (_context.BlockStack.Count != 0)
                                    _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedBlockStart, token, _context.BlockStack.Count, _parsedIncludes));
                                _context.BlockStack.Clear();
                                PushBlockInfoToStack(IndentType.DoEnd, token.Line);
                            }
                            break;
                        case "on":
                            // parse a ON statement
                            CreateParsedOnEvent(token);
                            break;
                        case "run":
                            // Parse a run statement
                            CreateParsedRun(token);
                            break;
                        case "dynamic-function":
                            CreateParsedDynamicFunction(token);
                            break;
                        case "subscribe":
                            CreateParsedSubscribe(token);
                            break;
                        case "unsubscribe":
                            CreateParsedSubscribe(token);
                            break;
                        case "publish":
                            CreateParsedPublish(token);
                            break;
                        default:
                            // it's a potential label
                            _context.StatementUnknownFirstWord = true;
                            break;
                    }
                } else {
                    // not the first word of a statement

                    switch (lowerTok) {
                        case "dynamic-function":
                            CreateParsedDynamicFunction(token);
                            break;
                        case "do":
                            // matches a do in the middle of a statement (ex: ON CHOOSE OF xx DO:)
                            PushBlockInfoToStack(IndentType.DoEnd, token.Line);
                            break;
                        case "triggers":
                            if (PeekAtNextNonSpace(0) is TokenEos)
                                PushBlockInfoToStack(IndentType.DoEnd, token.Line);
                            break;
                        case "then":
                            // add a one time indent after a then or else
                            PushBlockInfoToStack(IndentType.ThenElse, token.Line);
                            NewStatement(token);
                            break;
                        default:
                            // try to match a known word
                            if (_matchKnownWords) {
                                if (KnownStaticItems.ContainsKey(lowerTok)) {
                                    // we known the word
                                    if (KnownStaticItems[lowerTok] == CompletionType.Table) {
                                        // it's a table from the database
                                        AddParsedItem(new ParsedFoundTableUse(token.Value, token, false), token.OwnerNumber);
                                    }
                                } else if (_knownWords.ContainsKey(lowerTok)) {
                                    if (_knownWords[lowerTok] == CompletionType.Table) {
                                        // it's a temp table
                                        AddParsedItem(new ParsedFoundTableUse(token.Value, token, true), token.OwnerNumber);
                                    }
                                }
                            }
                            break;
                    }
                }
            }

            // include
            else if (token is TokenInclude) {
                CreateParsedIncludeFile(token);
                NewStatement(PeekAt(1));
            }

            // pre processed statement
            else if (token is TokenPreProcDirective) {
                var directiveLower = token.Value.ToLower();

                switch (directiveLower) {
                    case "&else":
                        NewStatement(token);
                        break;

                    case "&if":
                        _context.PreProcIfStack.Push(CreateParsedIfEndIfPreProc(token));
                        break;

                    case "&elseif":
                    case "&endif":
                        if (_context.PreProcIfStack.Count > 0) {
                            var prevIf = _context.PreProcIfStack.Pop();
                            prevIf.EndBlockLine = token.Line;
                            prevIf.EndBlockPosition = token.EndPosition;
                        } else
                            _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedIfEndIfBlockEnd, token, 0, _parsedIncludes));

                        if (directiveLower == "&elseif") {
                            _context.PreProcIfStack.Push(CreateParsedIfEndIfPreProc(token));
                        } else {
                            NewStatement(token);
                        }
                        break;

                    case "&then":
                        NewStatement(token);
                        break;

                    default:
                        // should be the first word of a statement (otherwise this probably doesn't compile anyway!)
                        // if(_context.StatementWordCount == 0)
                        CreateParsedPreProcDirective(token);
                        NewStatement(PeekAt(0));
                        break;
                }
            }

            // potential function call
            else if (token is TokenSymbol && token.Value.Equals("(")) {
                var prevToken = PeekAtNextNonSpace(0, true);
                if (prevToken is TokenWord && _functionPrototype.ContainsKey(prevToken.Value))
                    AddParsedItem(new ParsedFunctionCall(prevToken.Value, prevToken, false, true), token.OwnerNumber);
            }

            // end of statement
            else if (token is TokenEos) {
                // match a label if there was only one word followed by : in the statement
                if (_context.StatementUnknownFirstWord && _context.StatementWordCount == 1 && token.Value.Equals(":"))
                    CreateParsedLabel(token);
                NewStatement(token);
            }
        }

        #endregion

        #region Read a statement, create Parsed values

        /// <summary>
        /// Creates a subscribe parsed item
        /// </summary>
        private void CreateParsedSubscribe(Token tokenSub) {
            /*
            SUBSCRIBE [ PROCEDURE subscriber-handle] [ TO ] event-name 
                { IN publisher-handle | ANYWHERE }
                [ RUN-PROCEDURE local-internal-procedure ] [ NO-ERROR ].
             * 
            UNSUBSCRIBE [ PROCEDURE subscriber-handle ] [ TO ] { event-name | ALL } 
                [ IN publisher-handle ].
            */

            // info we will extract from the current statement :
            string subscriberHandle = null;
            string eventName = null;
            string publisherHandler = null;
            string runProcedure = null;
            int state = 0;
            do {
                var token = PeekAt(1); // next token
                if (state == 3) break; // stop when the run procedure has been found
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                switch (state) {
                    case 0:
                        if (token is TokenWord) {
                            switch (token.Value.ToLower()) {
                                case "procedure":
                                    state = 20;
                                    break;
                                case "to":
                                    break;
                                default:
                                    // event name
                                    eventName = GetTokenStrippedValue(token);
                                    state++;
                                    break;
                            }
                        } else if (token is TokenString) {
                            // event name
                            eventName = GetTokenStrippedValue(token);
                            state++;
                        }
                        break;
                    case 1:
                        if (!(token is TokenWord))
                            break;
                        switch (token.Value.ToLower()) {
                            case "in":
                                state = 30;
                                break;
                            case "anywhere":
                                publisherHandler = token.Value;
                                break;
                            case "run-procedure":
                                state++;
                                break;
                        }
                        break;
                    case 2:
                        // matching the local procedure 
                        if (token is TokenString || token is TokenWord) {
                            runProcedure = GetTokenStrippedValue(token);
                            state++;
                        }
                        break;
                    case 20:
                        // matching PROCEDURE xx
                        if (!(token is TokenWord))
                            continue;
                        subscriberHandle = token.Value;
                        state = 0;
                        break;
                    case 30:
                        // matching IN publisher-handle
                        if (!(token is TokenWord))
                            continue;
                        publisherHandler = token.Value;
                        state = 1;
                        break;
                }
            } while (MoveNext());
            if (!string.IsNullOrEmpty(eventName))
                AddParsedItem(new ParsedEvent(tokenSub.Value.EqualsCi("subscribe") ? ParsedEventType.Subscribe : ParsedEventType.Unsubscribe, eventName, tokenSub, subscriberHandle, publisherHandler, runProcedure, null), tokenSub.OwnerNumber);
        }

        /// <summary>
        /// Creates a publish parsed item
        /// </summary>
        private void CreateParsedPublish(Token tokenPub) {
            /*
            PUBLISH event-name
              [ FROM publisher-handle ]
              [ ( parameter[ , parameter ]... ) ].
            */

            // info we will extract from the current statement :
            string eventName = null;
            string publisherHandler = null;
            StringBuilder left = new StringBuilder();
            int state = 0;
            do {
                var token = PeekAt(1); // next token
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                switch (state) {
                    case 0:
                        if (token is TokenString || token is TokenWord) {
                            // event name
                            eventName = GetTokenStrippedValue(token);
                            state++;
                        }
                        break;
                    case 1:
                        if (token is TokenWord) {
                            if (token.Value.EqualsCi("from"))
                                state = 10;
                        } else if (token is TokenSymbol && token.Value.Equals("(")) {
                            AddTokenToStringBuilder(left, token);
                            state++;
                        }
                        break;
                    case 2:
                        AddTokenToStringBuilder(left, token);
                        break;
                    case 10:
                        // match publisher handler
                        if (!(token is TokenWord))
                            break;
                        publisherHandler = token.Value;
                        state = 1;
                        break;
                }
            } while (MoveNext());
            if (!string.IsNullOrEmpty(eventName))
                AddParsedItem(new ParsedEvent(ParsedEventType.Publish, eventName, tokenPub, null, publisherHandler, null, left.ToString()), tokenPub.OwnerNumber);
        }

        /// <summary>
        /// Creates a dynamic function parsed item
        /// </summary>
        private void CreateParsedDynamicFunction(Token tokenFun) {
            // info we will extract from the current statement :
            string name = "";
            int state = 0;
            do {
                var token = PeekAt(1); // next token
                if (state == 2) break; // stop after finding the name
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                switch (state) {
                    case 0:
                        if (token is TokenSymbol && token.Value.Equals("("))
                            state++;
                        break;
                    case 1:
                        // matching proc name (or VALUE)
                        if (token is TokenString) {
                            name = GetTokenStrippedValue(token);
                            state++;
                        }
                        break;
                }
            } while (MoveNext());

            if (state == 0) return;

            AddParsedItem(new ParsedFunctionCall(name, tokenFun, !_functionPrototype.ContainsKey(name), false), tokenFun.OwnerNumber);
        }

        /// <summary>
        /// Creates a label parsed item
        /// </summary>
        private void CreateParsedLabel(Token labelToken) {
            AddParsedItem(new ParsedLabel(_context.StatementFirstToken.Value, _context.StatementFirstToken), labelToken.OwnerNumber);
        }

        /// <summary>
        /// Creates a parsed item for RUN statements
        /// </summary>
        /// <param name="runToken"></param>
        private void CreateParsedRun(Token runToken) {
            // info we will extract from the current statement :
            string name = "";
            ParseFlag flag = 0;
            _lastTokenWasSpace = true;
            int state = 0;
            do {
                var token = PeekAt(1); // next token
                if (state == 2) break; // stop after finding the RUN name to be able to match other words in the statement
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                switch (state) {
                    case 0:
                        // matching proc name (or VALUE)
                        if (token is TokenSymbol && token.Value.Equals(")")) {
                            state++;
                        } else if (flag.HasFlag(ParseFlag.Uncertain) && !(token is TokenWhiteSpace || token is TokenSymbol)) {
                            name += GetTokenStrippedValue(token);
                        } else if (token is TokenWord) {
                            if (token.Value.ToLower().Equals("value"))
                                flag |= ParseFlag.Uncertain;
                            else {
                                name += token.Value;
                                state++;
                            }
                        } else if (token is TokenString) {
                            name = GetTokenStrippedValue(token);
                            state++;
                        }
                        break;
                    case 1:
                        // matching PERSISTENT (or a path instead of a file)
                        if (token is TokenSymbol && (token.Value.Equals("/") || token.Value.Equals("\\"))) {
                            // if it's a path, append it to the name of the run
                            name += token.Value;
                            state = 0;
                            break;
                        }
                        if (!(token is TokenWord))
                            break;
                        if (token.Value.EqualsCi("persistent"))
                            flag |= ParseFlag.Persistent;
                        state++;
                        break;
                }
            } while (MoveNext());

            if (state == 0) return;
            AddParsedItem(new ParsedRun(name, runToken, null) {Flags = flag}, runToken.OwnerNumber);
        }

        /// <summary>
        /// Creates parsed item for ON CHOOSE OF XXX events
        /// (choose or anything else)
        /// </summary>
        /// <param name="onToken"></param>
        /// <returns></returns>
        private void CreateParsedOnEvent(Token onToken) {
            // info we will extract from the current statement :
            var eventList = new StringBuilder();
            var widgetList = new StringBuilder();
            int state = 0;
            do {
                var token = PeekAt(1); // next token
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                if (state == 99) break;
                switch (state) {
                    case 0:
                        // matching event type
                        if (token is TokenWord || token is TokenString || token is TokenSymbol) {
                            eventList.Append((eventList.Length == 0 ? "" : ", ") + GetTokenStrippedValue(token));
                            state++;
                        }
                        break;
                    case 1:
                        // matching an event list?
                        if (token is TokenSymbol && token.Value.Equals(",")) {
                            state--;
                            break;
                        }
                        if (!(token is TokenWord)) break;

                        if (token.Value.EqualsCi("anywhere")) {
                            // we match anywhere, need to return to match a block start
                            widgetList.Append("anywhere");
                            var new1 = new ParsedOnStatement(eventList + " " + widgetList, onToken, eventList.ToString(), widgetList.ToString());
                            AddParsedItem(new1, onToken.OwnerNumber);
                            _context.Scope = new1;
                            return;
                        }
                        // if not anywhere, we expect an "of"
                        if (token.Value.EqualsCi("of")) {
                            state++;
                            break;
                        }

                        // we matched a 'ON key-label key-function'
                        widgetList.Append(token.Value);
                        var new3 = new ParsedOnStatement(eventList + " " + widgetList, onToken, eventList.ToString(), widgetList.ToString());
                        AddParsedItem(new3, onToken.OwnerNumber);
                        _context.Scope = new3;
                        return;
                    case 2:
                        // matching widget name
                        if (token is TokenWord || token is TokenString) {
                            // ON * OF FRAME fMain, on ne prend pas en compte le FRAME
                            if (!token.Value.EqualsCi("frame")) {
                                widgetList.Append((widgetList.Length == 0 ? "" : ", ") + GetTokenStrippedValue(token));
                                state++;
                            }
                        }
                        break;
                    case 3:
                        // matching a widget list?
                        if (token is TokenSymbol && token.Value.Equals(",")) {
                            state--;
                            break;
                        }
                        if (!(token is TokenWord)) break;

                        // matching a widget IN FRAME
                        if (token.Value.EqualsCi("in")) {
                            var nextNonSpace = PeekAtNextNonSpace(1);
                            if (!(nextNonSpace is TokenWord && nextNonSpace.Value.Equals("frame"))) {
                                // skip the whole IN FRAME XX
                                MoveNext();
                                MoveNext();
                                MoveNext();
                                MoveNext();
                                break;
                            }
                        }

                        var new2 = new ParsedOnStatement(eventList + " " + widgetList, onToken, eventList.ToString(), widgetList.ToString());
                        AddParsedItem(new2, onToken.OwnerNumber);
                        _context.Scope = new2;

                        // matching a OR
                        if (token.Value.EqualsCi("or")) {
                            widgetList.Clear();
                            eventList.Clear();
                            state = 0;
                            break;
                        }

                        // end here
                        return;
                }
            } while (MoveNext());
        }

        /// <summary>
        /// Matches a new definition
        /// </summary>
        private void CreateParsedDefine(Token defineToken, bool isDynamic) {
            // info we will extract from the current statement :
            string name = "";
            ParseFlag flags = isDynamic ? ParseFlag.Dynamic : 0;
            ParsedAsLike asLike = ParsedAsLike.None;
            ParseDefineType type = ParseDefineType.None;
            string tempPrimitiveType = "";
            string viewAs = "";
            string bufferFor = "";
            _lastTokenWasSpace = true;
            StringBuilder left = new StringBuilder();

            // for temp tables:
            string likeTable = "";
            bool isTempTable = false;
            var fields = new List<ParsedField>();
            ParsedField currentField = new ParsedField("", "", "", 0, 0, "", "", ParsedAsLike.None);
            StringBuilder useIndex = new StringBuilder();

            // for tt indexes
            var indexList = new List<ParsedIndex>();
            string indexName = "";
            var indexFields = new List<string>();
            ParsedIndexFlag indexFlags = ParsedIndexFlag.None;
            var indexSort = "+"; // + for ascending, - for descending

            Token token;
            int state = 0;
            do {
                token = PeekAt(1); // next token
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                string lowerToken;
                bool matchedLikeTable = false;
                switch (state) {
                    case 0:
                        // matching until type of define is found
                        if (!(token is TokenWord)) break;
                        lowerToken = token.Value.ToLower();
                        switch (lowerToken) {
                            case "buffer":
                            case "browse":
                            case "stream":
                            case "button":
                            case "dataset":
                            case "frame":
                            case "query":
                            case "event":
                            case "image":
                            case "menu":
                            case "rectangle":
                            case "property":
                            case "sub-menu":
                            case "parameter":
                                if (!Enum.TryParse(lowerToken, true, out type))
                                    type = ParseDefineType.None;
                                state++;
                                break;
                            case "data-source":
                                type = ParseDefineType.DataSource;
                                state++;
                                break;
                            case "var":
                            case "variable":
                                type = ParseDefineType.Variable;
                                state++;
                                break;
                            case "temp-table":
                            case "work-table":
                            case "workfile":
                                isTempTable = true;
                                state++;
                                break;
                            case "new":
                                flags |= ParseFlag.New;
                                break;
                            case "global":
                                flags |= ParseFlag.Global;
                                break;
                            case "shared":
                                flags |= ParseFlag.Shared;
                                break;
                            case "private":
                                flags |= ParseFlag.Private;
                                break;
                            case "protected":
                                flags |= ParseFlag.Protected;
                                break;
                            case "public":
                                flags |= ParseFlag.Public;
                                break;
                            case "static":
                                flags |= ParseFlag.Static;
                                break;
                            case "abstract":
                                flags |= ParseFlag.Abstract;
                                break;
                            case "override":
                                flags |= ParseFlag.Override;
                                break;
                            case "serializable":
                                flags |= ParseFlag.Serializable;
                                break;
                            case "input":
                                flags |= ParseFlag.Input;
                                break;
                            case "return":
                                flags |= ParseFlag.Return;
                                break;
                            case "output":
                                flags |= ParseFlag.Output;
                                break;
                            case "input-output":
                                flags |= ParseFlag.InputOutput;
                                break;
                            /*default:
                                ParseFlag parsedFlag;
                                if (Enum.TryParse(lowerToken, true, out parsedFlag))
                                    flags |= parsedFlag;
                                break;*/
                        }
                        break;

                    case 1:
                        // matching the name
                        if (!(token is TokenWord)) break;
                        name = token.Value;
                        if (type == ParseDefineType.Variable) state = 10;
                        if (type == ParseDefineType.Buffer) {
                            tempPrimitiveType = "buffer";
                            state = 81;
                        }
                        if (type == ParseDefineType.Parameter) {
                            lowerToken = token.Value.ToLower();
                            switch (lowerToken) {
                                case "buffer":
                                case "table":
                                case "table-handle":
                                case "dataset":
                                case "dataset-handle":
                                    tempPrimitiveType = lowerToken;
                                    state = 80;
                                    break;
                                default:
                                    state = 10;
                                    break;
                            }
                        }
                        if (isTempTable) state = 20;
                        if (state != 1) break;
                        state = 99;
                        break;

                    case 10:
                        // define variable : match as or like
                        if (!(token is TokenWord)) break;
                        lowerToken = token.Value.ToLower();
                        if (lowerToken.Equals("as")) asLike = ParsedAsLike.As;
                        else if (lowerToken.Equals("like")) asLike = ParsedAsLike.Like;
                        if (asLike != ParsedAsLike.None) state = 11;
                        break;
                    case 11:
                        // define variable : match a primitive type or a field in db
                        if (!(token is TokenWord)) break;
                        tempPrimitiveType = token.Value;
                        state = 12;
                        break;
                    case 12:
                        // define variable : match a view-as (or extent)
                        AddTokenToStringBuilder(left, token);
                        if (!(token is TokenWord)) break;
                        lowerToken = token.Value.ToLower();
                        if (lowerToken.Equals("view-as")) state = 13;
                        if (lowerToken.Equals("extent"))
                            flags |= ParseFlag.Extent;
                        break;
                    case 13:
                        // define variable : match a view-as
                        AddTokenToStringBuilder(left, token);
                        if (!(token is TokenWord)) break;
                        viewAs = token.Value;
                        state = 99;
                        break;

                    case 20:
                        // define temp-table
                        if (!(token is TokenWord)) break;
                        lowerToken = token.Value.ToLower();
                        switch (lowerToken) {
                            case "field":
                                // matches FIELD
                                state = 22;
                                break;
                            case "index":
                                // matches INDEX
                                state = 25;
                                break;
                            case "use-index":
                                // matches USE-INDEX (after a like/like-sequential, we can have this keyword)
                                state = 26;
                                break;
                            case "help":
                                // a field has a help text:
                                state = 27;
                                break;
                            case "initial":
                                // a field has an initial value
                                state = 29;
                                break;
                            case "format":
                                // a field has a format
                                state = 30;
                                break;
                            case "extent":
                                // a field is extent:
                                currentField.Flags = currentField.Flags | ParseFlag.Extent;
                                break;
                            default:
                                // matches a LIKE table
                                // ReSharper disable once ConditionIsAlwaysTrueOrFalse, resharper doesn't get this one
                                if ((lowerToken.Equals("like") || lowerToken.Equals("like-sequential")) && !matchedLikeTable)
                                    state = 21;
                                // After a USE-UNDEX and the index name, we can match a AS PRIMARY for the previously defined index
                                if (lowerToken.Equals("primary") && useIndex.Length > 0)
                                    useIndex.Append("!");
                                break;
                        }
                        break;
                    case 21:
                        // define temp-table : match a LIKE table, get the table name in asLike
                        // ReSharper disable once RedundantAssignment
                        matchedLikeTable = true;
                        if (!(token is TokenWord)) break;
                        likeTable = token.Value.ToLower();
                        state = 20;
                        break;

                    case 22:
                        // define temp-table : matches a FIELD name
                        if (!(token is TokenWord)) break;
                        currentField = new ParsedField(token.Value, "", "", 0, 0, "", "", ParsedAsLike.None);
                        state = 23;
                        break;
                    case 23:
                        // define temp-table : matches a FIELD AS or LIKE
                        if (!(token is TokenWord)) break;
                        currentField.AsLike = token.Value.EqualsCi("like") ? ParsedAsLike.Like : ParsedAsLike.As;
                        state = 24;
                        break;
                    case 24:
                        // define temp-table : match a primitive type or a field in db
                        if (!(token is TokenWord)) break;
                        currentField.TempType = token.Value;
                        // push the field to the fields list
                        fields.Add(currentField);
                        state = 20;
                        break;

                    case 25:
                        // define temp-table : match an index name
                        if (!(token is TokenWord)) break;
                        indexName = token.Value;
                        state = 28;
                        break;

                    case 28:
                        // define temp-table : match the definition of the index
                        if (!(token is TokenWord)) break;
                        lowerToken = token.Value.ToLower();
                        if (lowerToken.Equals("unique")) {
                            indexFlags = indexFlags | ParsedIndexFlag.Unique;
                        } else if (lowerToken.Equals("primary")) {
                            indexFlags = indexFlags | ParsedIndexFlag.Primary;
                        } else if (lowerToken.Equals("word-index")) {
                            indexFlags = indexFlags | ParsedIndexFlag.WordIndex;
                        } else if (lowerToken.Equals("ascending")) {
                            // match a sort order for a field
                            indexSort = "+";
                            var lastField = indexFields.LastOrDefault();
                            if (lastField != null) {
                                indexFields.RemoveAt(indexFields.Count - 1);
                                indexFields.Add(lastField.Replace("-", "+"));
                            }
                        } else if (lowerToken.Equals("descending")) {
                            // match a sort order for a field
                            indexSort = "-";
                            var lastField = indexFields.LastOrDefault();
                            if (lastField != null) {
                                indexFields.RemoveAt(indexFields.Count - 1);
                                indexFields.Add(lastField.Replace("+", "-"));
                            }
                        } else if (lowerToken.Equals("index")) {
                            // matching a new index
                            if (!string.IsNullOrEmpty(indexName))
                                indexList.Add(new ParsedIndex(indexName, indexFlags, indexFields.ToList()));

                            indexName = "";
                            indexFields.Clear();
                            indexFlags = ParsedIndexFlag.None;
                            indexSort = "+";

                            state = 25;
                        } else {
                            // Otherwise, it's a field name
                            var found = fields.Find(field => field.Name.EqualsCi(lowerToken));
                            if (found != null) {
                                indexFields.Add(token.Value + indexSort);
                            }
                        }
                        break;

                    case 26:
                        // define temp-table : match a USE-INDEX name
                        if (!(token is TokenWord)) break;
                        useIndex.Append(",");
                        useIndex.Append(token.Value);
                        state = 20;
                        break;

                    case 27:
                        // define temp-table : match HELP for a field
                        if (!(token is TokenString)) break;
                        currentField.Description = GetTokenStrippedValue(token);
                        state = 20;
                        break;
                    case 29:
                        // define temp-table : match INITIAL for a field
                        if (!(token is TokenWhiteSpace)) {
                            currentField.InitialValue = GetTokenStrippedValue(token);
                            state = 20;
                        }
                        break;
                    case 30:
                        // define temp-table : match FORMAT for a field
                        if (!(token is TokenWhiteSpace)) {
                            currentField.Format = GetTokenStrippedValue(token);
                            state = 20;
                        }
                        break;

                    case 80:
                        // define parameter : match a temptable, table, dataset or buffer name
                        if (!(token is TokenWord)) break;
                        if (token.Value.ToLower().Equals("for")) break;
                        name = token.Value;
                        state++;
                        break;
                    case 81:
                        // match the table/dataset name that the buffer or handle is FOR
                        if (!(token is TokenWord)) break;
                        lowerToken = token.Value.ToLower();
                        if (lowerToken.Equals("for") || lowerToken.Equals("temp-table")) break;
                        bufferFor = lowerToken;
                        state = 99;
                        break;

                    case 99:
                        // matching the rest of the define
                        AddTokenToStringBuilder(left, token);
                        break;
                }
            } while (MoveNext());

            if (state <= 1) return;
            if (isTempTable) {
                if (!string.IsNullOrEmpty(indexName))
                    indexList.Add(new ParsedIndex(indexName, indexFlags, indexFields));

                AddParsedItem(new ParsedTable(name, defineToken, "", "", name, "", likeTable, true, fields, indexList, new List<ParsedTrigger>(), useIndex.ToString()) {
                    // = end position of the EOS of the statement
                    EndPosition = token.EndPosition,
                    Flags = flags
                }, defineToken.OwnerNumber);
            } else {
                var newDefine = new ParsedDefine(name, defineToken, asLike, left.ToString(), type, tempPrimitiveType, viewAs, bufferFor) {
                    // = end position of the EOS of the statement
                    EndPosition = token.EndPosition,
                    Flags = flags
                };
                AddParsedItem(newDefine, defineToken.OwnerNumber);

                // case of a parameters, add it to the current scope (if procedure)
                var currentScope = _context.Scope as ParsedProcedure;
                if (type == ParseDefineType.Parameter && currentScope != null) {
                    if (currentScope.Parameters == null)
                        currentScope.Parameters = new List<ParsedDefine>();
                    currentScope.Parameters.Add(newDefine);
                }
            }
        }

        /// <summary>
        /// Analyse a preprocessed directive (analyses the whole statement)
        /// </summary>
        private bool CreateParsedPreProcDirective(Token directiveToken) {
            // info we will extract from the current statement :
            string variableName = null;
            _lastTokenWasSpace = true;
            StringBuilder definition = new StringBuilder();
            List<Token> tokensList = new List<Token>();

            do {
                var token = PeekAt(1);
                if (token is TokenComment) continue;
                // a ~ allows for a eol but we don't control if it's an eol because if it's something else we probably parsed it wrong anyway (in the lexer)
                if (token is TokenSymbol && token.Value == "~") {
                    if (PeekAt(2) is TokenEol)
                        MoveNext();
                    continue;
                }
                if (token is TokenEol) break;

                // read the first word after the directive
                if (string.IsNullOrEmpty(variableName) && token is TokenWord) {
                    variableName = token.Value;
                    continue;
                }
                tokensList.Add(token);
                AddTokenToStringBuilder(definition, token);
            } while (MoveNext());

            ParseFlag flags = 0;

            // match first word of the statement
            switch (directiveToken.Value.ToUpper()) {
                case "&GLOBAL-DEFINE":
                case "&GLOBAL":
                case "&GLOB":
                    flags |= ParseFlag.Global;
                    break;

                case "&SCOPED-DEFINE":
                case "&SCOPED":
                    flags |= ParseFlag.FileScope;
                    break;

                case "&ANALYZE-SUSPEND":
                    // we don't care about the blocks of include files
                    if (directiveToken.OwnerNumber > 0)
                        return false;

                    // it marks the beginning of an appbuilder block, it can only be at a root/File level, otherwise flag error
                    if (!(_context.Scope is ParsedFile)) {
                        _parserErrors.Add(new ParserError(ParserErrorType.NotAllowedUibBlockStart, directiveToken, 0, _parsedIncludes));
                        _context.Scope = _rootScope;
                    }

                    // we match a new block start but we didn't match the previous block end, flag error
                    if (_context.UibBlockStack.Count > 0) {
                        _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedUibBlockStart, directiveToken, _context.UibBlockStack.Count, _parsedIncludes));
                        _context.UibBlockStack.Clear();
                    }

                    // matching different intersting blocks
                    var textAfterDirective = variableName + " " + definition.ToString().Trim();
                    ParsedPreProcBlockType type = ParsedPreProcBlockType.Unknown;
                    string blockName = "Appbuilder block";
                    if (textAfterDirective.ContainsFast("_FUNCTION-FORWARD")) {
                        type = ParsedPreProcBlockType.FunctionForward;
                        blockName = "Function prototype";
                    } else if (textAfterDirective.ContainsFast("_MAIN-BLOCK")) {
                        type = ParsedPreProcBlockType.MainBlock;
                        blockName = "Main block";
                    } else if (textAfterDirective.ContainsFast("_DEFINITIONS")) {
                        type = ParsedPreProcBlockType.Definitions;
                        blockName = "Definitions";
                    } else if (textAfterDirective.ContainsFast("_UIB-PREPROCESSOR-BLOCK")) {
                        type = ParsedPreProcBlockType.UibPreprocessorBlock;
                        blockName = "Pre-processor definitions";
                    } else if (textAfterDirective.ContainsFast("_XFTR")) {
                        type = ParsedPreProcBlockType.Xftr;
                        blockName = "Xtfr";
                    } else if (textAfterDirective.ContainsFast("_PROCEDURE-SETTINGS")) {
                        type = ParsedPreProcBlockType.ProcedureSettings;
                        blockName = "Procedure settings";
                    } else if (textAfterDirective.ContainsFast("_CREATE-WINDOW")) {
                        type = ParsedPreProcBlockType.CreateWindow;
                        blockName = "Window settings";
                    } else if (textAfterDirective.ContainsFast("_RUN-TIME-ATTRIBUTES")) {
                        type = ParsedPreProcBlockType.RunTimeAttributes;
                        blockName = "Runtime attributes";
                    }
                    _context.UibBlockStack.Push(new ParsedPreProcBlock(blockName, directiveToken) {
                        Type = type,
                        BlockDescription = textAfterDirective
                    });

                    // save the block description
                    AddParsedItem(_context.UibBlockStack.Peek(), directiveToken.OwnerNumber);
                    break;

                case "&ANALYZE-RESUME":
                    // we don't care about the blocks of include files
                    if (directiveToken.OwnerNumber > 0)
                        return false;

                    // it marks the end of an appbuilder block, it can only be at a root/File level
                    if (!(_context.Scope is ParsedFile)) {
                        _parserErrors.Add(new ParserError(ParserErrorType.NotAllowedUibBlockEnd, directiveToken, 0, _parsedIncludes));
                        _context.Scope = _rootScope;
                    }

                    if (_context.UibBlockStack.Count == 0) {
                        // we match an end w/o beggining, flag a mismatch
                        _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedUibBlockEnd, directiveToken, 0, _parsedIncludes));
                    } else {
                        // end position of the current appbuilder block
                        var currentBlock = _context.UibBlockStack.Pop();
                        currentBlock.EndBlockLine = directiveToken.Line;
                        currentBlock.EndBlockPosition = directiveToken.EndPosition;
                    }

                    break;

                case "&UNDEFINE":
                    if (variableName != null) {
                        var found = (ParsedPreProcVariable) _parsedItemList.FindLast(item => (item is ParsedPreProcVariable && item.Name.Equals(variableName)));
                        if (found != null)
                            found.UndefinedLine = _context.StatementFirstToken.Line;
                    }
                    break;

                default:
                    return false;
            }

            // We matched a new preprocessed variable?
            if (flags > 0 && !string.IsNullOrEmpty(variableName)) {
                AddParsedItem(new ParsedPreProcVariable(variableName, directiveToken, 0, definition.ToString().Trim()) {
                    Flags = flags
                }, directiveToken.OwnerNumber);

                // add it to the know variables (either to the global scope or to the local scope)
                if (flags.HasFlag(ParseFlag.Global)) {
                    if (_globalPreProcVariables.ContainsKey("&" + variableName))
                        _globalPreProcVariables["&" + variableName] = TrimTokensList(tokensList);
                    else
                        _globalPreProcVariables.Add("&" + variableName, TrimTokensList(tokensList));
                } else {
                    if (_parsedIncludes[directiveToken.OwnerNumber].ScopedPreProcVariables.ContainsKey("&" + variableName))
                        _parsedIncludes[directiveToken.OwnerNumber].ScopedPreProcVariables["&" + variableName] = TrimTokensList(tokensList);
                    else
                        _parsedIncludes[directiveToken.OwnerNumber].ScopedPreProcVariables.Add("&" + variableName, TrimTokensList(tokensList));
                }
            }

            return true;
        }

        /// <summary>
        /// Matches a & IF.. & THEN pre-processed statement
        /// </summary>
        private ParsedPreProcBlock CreateParsedIfEndIfPreProc(Token ifToken) {
            _lastTokenWasSpace = true;
            StringBuilder expression = new StringBuilder();

            do {
                var token = PeekAt(1);
                if (token is TokenPreProcDirective) break;
                if (token is TokenComment) continue;
                AddTokenToStringBuilder(expression, token);
            } while (MoveNext());

            var newIf = new ParsedPreProcBlock(string.Empty, ifToken) {
                Type = ParsedPreProcBlockType.IfEndIf,
                BlockDescription = expression.ToString()
            };
            AddParsedItem(newIf, ifToken.OwnerNumber);

            return newIf;
        }

        /// <summary>
        /// Matches a procedure definition
        /// </summary>
        private bool CreateParsedProcedure(Token procToken) {
            /*
            PROCEDURE proc-name[ PRIVATE ] :
                [procedure-body]

            PROCEDURE proc-name 
                {   EXTERNAL "dllname" [ CDECL | PASCAL | STDCALL ]
                        [ ORDINAL n ][ PERSISTENT ][ THREAD-SAFE ] | IN SUPER } :
                [ procedure-body ]
            */

            // info we will extract from the current statement :
            string name = "";
            ParseFlag flags = 0;
            string externalDllName = null;
            _lastTokenWasSpace = true;
            StringBuilder leftStr = new StringBuilder();

            Token token;
            int state = 0;
            do {
                token = PeekAt(1); // next token
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                switch (state) {
                    case 0:
                        // matching name
                        if (token is TokenWord || token is TokenString) {
                            name = token is TokenWord ? token.Value : GetTokenStrippedValue(token);
                            state++;
                        }
                        continue;
                    case 1:
                        // matching external
                        if (!(token is TokenWord)) continue;
                        switch (token.Value.ToLower()) {
                            case "external":
                                flags |= ParseFlag.External;
                                state++;
                                break;
                            case "private":
                                flags |= ParseFlag.Private;
                                break;
                        }
                        break;
                    case 2:
                        // matching the name of the external dll
                        if (!(token is TokenString)) continue;
                        externalDllName = GetTokenStrippedValue(token);
                        state--;
                        break;
                }
                AddTokenToStringBuilder(leftStr, token);
            } while (MoveNext());

            if (state < 1) return false;
            var newProc = new ParsedProcedure(name, procToken, leftStr.ToString(), externalDllName) {
                // = end position of the EOS of the statement
                EndPosition = token.EndPosition,
                Flags = flags
            };
            AddParsedItem(newProc, procToken.OwnerNumber);
            _context.Scope = newProc;
            return true;
        }

        /// <summary>
        /// Matches a function definition (not the FORWARD prototype)
        /// </summary>
        private bool CreateParsedFunction(Token functionToken) {
            // info we will extract from the current statement :
            string name = null;
            string returnType = null;
            string extend = null;
            ParseFlag flags = 0;
            StringBuilder parameters = new StringBuilder();
            List<ParsedDefine> parametersList = null;

            _lastTokenWasSpace = true;

            Token token;
            int state = 0;
            do {
                token = PeekAt(1); // next token
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                switch (state) {
                    case 0:
                        // matching name
                        if (!(token is TokenWord)) break;
                        name = token.Value;
                        state++;
                        break;
                    case 1:
                        // matching return type
                        if (!(token is TokenWord)) break;
                        if (token.Value.EqualsCi("returns") || token.Value.EqualsCi("class"))
                            continue;

                        returnType = token.Value;

                        state++;
                        break;
                    case 2:
                        // matching parameters (start)
                        if (token is TokenWord) {
                            if (token.Value.EqualsCi("private"))
                                flags |= ParseFlag.Private;
                            if (token.Value.EqualsCi("extent"))
                                flags |= ParseFlag.Extent;

                            // we didn't match any opening (, but we found a forward
                            if (token.Value.EqualsCi("forward"))
                                state = 99;
                            else if (token.Value.EqualsCi("in"))
                                state = 100;
                        } else if (token is TokenSymbol && token.Value.Equals("("))
                            state = 3;
                        else if (flags.HasFlag(ParseFlag.Extent) && token is TokenNumber)
                            extend = token.Value;
                        break;
                    case 3:
                        // read parameters, define a ParsedDefineItem for each
                        parametersList = GetParsedParameters(functionToken, parameters);
                        state = 10;
                        break;
                    case 10:
                        // matching prototype, we dont want to create a ParsedItem for prototype
                        if (token is TokenWord) {
                            if (token.Value.EqualsCi("forward"))
                                state = 99;
                            else if (token.Value.EqualsCi("in"))
                                state = 100;
                        }
                        break;
                }
            } while (MoveNext());
            if (name == null || returnType == null)
                return false;

            // otherwise it needs to ends with : or .
            if (!(token is TokenEos))
                return false;

            // New prototype, we matched a forward or a IN
            if (state >= 99) {
                ParsedPrototype createdProto = new ParsedPrototype(name, functionToken, returnType) {
                    Scope = _context.Scope,
                    FilePath = FilePathBeingParsed,
                    SimpleForward = state == 99, // allows us to know if we expect an implementation in this .p or not
                    EndPosition = token.EndPosition,
                    EndBlockLine = token.Line,
                    EndBlockPosition = token.EndPosition,
                    Flags = flags,
                    Extend = extend ?? string.Empty,
                    ParametersString = parameters.ToString()
                };
                if (!_functionPrototype.ContainsKey(name))
                    _functionPrototype.Add(name, createdProto);

                AddParsedItem(createdProto, functionToken.OwnerNumber);

                // case of a IN
                if (!createdProto.SimpleForward) {
                    // modify context
                    _context.Scope = createdProto;

                    // add the parameters to the list
                    if (parametersList != null) {
                        createdProto.Parameters = new List<ParsedDefine>();
                        foreach (var parsedItem in parametersList) {
                            AddParsedItem(parsedItem, functionToken.OwnerNumber);
                            createdProto.Parameters.Add(parsedItem);
                        }
                    }

                    // reset context
                    _context.Scope = _rootScope;
                }

                return false;
            }

            // New function
            ParsedImplementation createdImp = new ParsedImplementation(name, functionToken, returnType) {
                EndPosition = token.EndPosition,
                Flags = flags,
                Extend = extend ?? string.Empty,
                ParametersString = parameters.ToString()
            };

            // it has a prototype?
            if (_functionPrototype.ContainsKey(name)) {
                // make sure it was a prototype!
                var proto = _functionPrototype[name] as ParsedPrototype;
                if (proto != null && proto.SimpleForward) {
                    createdImp.HasPrototype = true;
                    createdImp.PrototypeLine = proto.Line;
                    createdImp.PrototypeColumn = proto.Column;
                    createdImp.PrototypePosition = proto.Position;
                    createdImp.PrototypeEndPosition = proto.EndPosition;

                    // boolean to know if the implementation matches the prototype
                    createdImp.PrototypeUpdated = (
                        createdImp.Flags == proto.Flags &&
                        createdImp.Extend.Equals(proto.Extend) &&
                        createdImp.ParsedReturnType.Equals(proto.ParsedReturnType) &&
                        createdImp.ParametersString.Equals(proto.ParametersString));
                }
            } else {
                _functionPrototype.Add(name, createdImp);
            }

            AddParsedItem(createdImp, functionToken.OwnerNumber);

            // modify context
            _context.Scope = createdImp;

            // add the parameters to the list
            if (parametersList != null) {
                createdImp.Parameters = new List<ParsedDefine>();
                foreach (var parsedItem in parametersList) {
                    AddParsedItem(parsedItem, functionToken.OwnerNumber);
                    createdImp.Parameters.Add(parsedItem);
                }
            }

            return true;
        }

        /// <summary>
        /// Parses a parameter definition (used in function, class method, class event)
        /// </summary>
        private List<ParsedDefine> GetParsedParameters(Token functionToken, StringBuilder parameters) {
            // info the parameters
            string paramName = "";
            ParseFlag flags = 0;
            ParsedAsLike paramAsLike = ParsedAsLike.None;
            string paramPrimitiveType = "";
            string parameterFor = "";
            var parametersList = new List<ParsedDefine>();

            int state = 0;
            do {
                var token = PeekAt(1); // next token
                if (token is TokenEos) break;
                if (token is TokenSymbol && token.Value.Equals(")")) state = 99;
                if (token is TokenComment) continue;
                switch (state) {
                    case 0:
                        // matching parameters type
                        if (!(token is TokenWord)) break;
                        var lwToken = token.Value.ToLower();
                        switch (lwToken) {
                            case "buffer":
                                paramPrimitiveType = lwToken;
                                state = 10;
                                break;
                            case "table":
                            case "table-handle":
                            case "dataset":
                            case "dataset-handle":
                                paramPrimitiveType = lwToken;
                                state = 20;
                                break;
                            case "input":
                                flags |= ParseFlag.Input;
                                break;
                            case "return":
                                flags |= ParseFlag.Return;
                                break;
                            case "output":
                                flags |= ParseFlag.Output;
                                break;
                            case "input-output":
                                flags |= ParseFlag.InputOutput;
                                break;
                            default:
                                paramName = token.Value;
                                state = 2;
                                break;
                        }
                        break;
                    case 2:
                        // matching parameters as or like
                        if (!(token is TokenWord)) break;
                        var lowerToken = token.Value.ToLower();
                        if (lowerToken.Equals("as")) paramAsLike = ParsedAsLike.As;
                        else if (lowerToken.Equals("like")) paramAsLike = ParsedAsLike.Like;
                        if (paramAsLike != ParsedAsLike.None) state++;
                        break;
                    case 3:
                        // matching parameters primitive type or a field in db
                        if (!(token is TokenWord)) break;
                        paramPrimitiveType = token.Value;
                        state = 99;
                        break;

                    case 10:
                        // match a buffer name
                        if (!(token is TokenWord)) break;
                        paramName = token.Value;
                        state++;
                        break;
                    case 11:
                        // match the table/dataset name that the buffer is FOR
                        if (!(token is TokenWord)) break;
                        lowerToken = token.Value.ToLower();
                        if (token.Value.EqualsCi("for")) break;
                        parameterFor = lowerToken;
                        state = 99;
                        break;

                    case 20:
                        // match a table/dataset name
                        if (!(token is TokenWord)) break;
                        paramName = token.Value;
                        state = 99;
                        break;

                    case 99:
                        // matching parameters "," that indicates a next param
                        if (token is TokenWord && token.Value.EqualsCi("extent"))
                            flags |= ParseFlag.Extent;
                        else if (token is TokenSymbol && (token.Value.Equals(")") || token.Value.Equals(","))) {
                            // create a variable for this function scope
                            if (!String.IsNullOrEmpty(paramName))
                                parametersList.Add(new ParsedDefine(paramName, functionToken, paramAsLike, "", ParseDefineType.Parameter, paramPrimitiveType, "", parameterFor) {
                                    Flags = flags
                                });
                            paramName = "";
                            paramAsLike = ParsedAsLike.None;
                            paramPrimitiveType = "";
                            parameterFor = "";
                            flags = 0;

                            if (token.Value.Equals(","))
                                state = 0;
                            else
                                return parametersList;
                        }
                        break;
                }
                AddTokenToStringBuilder(parameters, token);
            } while (MoveNext());
            return parametersList;
        }

        #region Handle include files

        /// <summary>
        /// matches an include file
        /// </summary>
        private void CreateParsedIncludeFile(Token bracketToken) {
            // This method should handle those cases :
            // {  file.i &name=val &2="value"} -> {&name} and {&2}
            // {file.i val "value"} -> {1} {2}

            var startingPos = _tokenPos;

            // info we will extract from the current statement :
            string fileName = "";
            bool usesNamedArg = false; // true if the arguments used are with the format : &name=""
            bool expectingFirstArg = true;
            string argName = null;
            int argNumber = 1;
            var parameters = new Dictionary<string, List<Token>>(_parsedIncludes[bracketToken.OwnerNumber].ScopedPreProcVariables, StringComparer.CurrentCultureIgnoreCase); // the scoped variable of this procedure will be available in the include file

            var state = 0;
            do {
                var token = PeekAt(1);
                if (token is TokenComment) continue;
                /* {{containsfilename.i}} <- this case is too complex for its use...
                 * if (token is TokenInclude) {
                    MoveNext();
                    CreateParsedIncludeFile(token);
                    continue;
                }*/
                if (token is TokenSymbol && token.Value == "}") {
                    MoveNext();
                    break;
                }
                switch (state) {
                    case 0:
                        // read the file name
                        if (token is TokenWord || token is TokenString) {
                            fileName += GetTokenStrippedValue(token);
                            state++;
                        }
                        break;

                    case 1:
                        if (token is TokenSymbol && (token.Value.Equals("/") || token.Value.Equals("\\"))) {
                            // it's a path, append it to the name of the run
                            fileName += token.Value;
                            state = 0;
                            break;
                        }

                        // read the arguments
                        if (expectingFirstArg) {
                            // case of a {file.i &x="arg1" &x=arg2}
                            if (token is TokenPreProcDirective) {
                                argName = token.Value;
                                usesNamedArg = true;
                                expectingFirstArg = false;
                                // case of a {file.i "arg1" arg2}
                            } else if (!(token is TokenEol || token is TokenWhiteSpace)) {
                                if (!parameters.ContainsKey(argNumber.ToString()))
                                    parameters.Add(argNumber.ToString(), TokenizeString(GetTokenStrippedValue(token)));
                                argNumber++;
                                expectingFirstArg = false;
                            }
                        } else {
                            if (usesNamedArg) {
                                // still waiting to read the argument name
                                if (argName == null) {
                                    if (token is TokenPreProcDirective)
                                        argName = token.Value;
                                } else if (!(token is TokenEol || token is TokenWhiteSpace || token.Value == "=")) {
                                    if (!parameters.ContainsKey(argName))
                                        parameters.Add(argName, TokenizeString(GetTokenStrippedValue(token)));
                                    argName = null;
                                }
                            } else if (!(token is TokenEol || token is TokenWhiteSpace)) {
                                if (!parameters.ContainsKey(argNumber.ToString()))
                                    parameters.Add(argNumber.ToString(), TokenizeString(GetTokenStrippedValue(token)));
                                argNumber++;
                            }
                        }
                        break;
                }
            } while (MoveNext());

            // we matched the include file name
            if (!string.IsNullOrEmpty(fileName)) {
                // try to find the file in the propath
                var fullFilePath = FindIncludeFullPath(fileName);

                // always add the parameter "0" which it the filename
                if (parameters.ContainsKey("0"))
                    parameters["0"] = new List<Token> {new TokenWord(Path.GetFileName(fullFilePath), 0, 0, 0, 0)};
                else
                    parameters.Add("0", new List<Token> {new TokenWord(Path.GetFileName(fullFilePath), 0, 0, 0, 0)});

                var newInclude = new ParsedIncludeFile(fileName, bracketToken, parameters, fullFilePath, _parsedIncludes[bracketToken.OwnerNumber]);

                AddParsedItem(newInclude, bracketToken.OwnerNumber);

                // Parse the include file ?
                if (!string.IsNullOrEmpty(fullFilePath)) {

                    var fileOwnerOfThisInclude = _parsedIncludes[bracketToken.OwnerNumber];
                    while (fileOwnerOfThisInclude != null) {
                        if (fileOwnerOfThisInclude.FullFilePath.Equals(fullFilePath)) {
                            // we are in a bad case of recursive include, stop here before we go out of memory
                            return;
                        }
                        fileOwnerOfThisInclude = fileOwnerOfThisInclude.Parent;
                    } 

                    ProLexer proLexer;

                    // did we already parsed this file in a previous parse session?
                    if (SavedLexerInclude.ContainsKey(fullFilePath)) {
                        proLexer = SavedLexerInclude[fullFilePath];
                    } else {
                        // Parse it
                        proLexer = new ProLexer(Utils.ReadAllText(fullFilePath));
                        SavedLexerInclude.Add(fullFilePath, proLexer);
                    }

                    // add this include to the references and modify each token
                    _parsedIncludes.Add(newInclude);
                    var includeNumber = (ushort) (_parsedIncludes.Count - 1);
                    var tokens = proLexer.GetTokensList.ToList().GetRange(0, proLexer.GetTokensList.Count - 1);
                    tokens.ForEach(token => token.OwnerNumber = includeNumber);

                    // replace the tokens
                    var nbTokens = _tokenPos - startingPos + 1;
                    _tokenPos = startingPos - 1; // reposition the current token before the start of the {include.i}
                    RemoveTokens(1, nbTokens.ClampMax(_tokenCount - (_tokenPos + 1)));
                    InsertTokens(1, tokens);

                    // replaces a preproc var {&x} at the beggining of the include file
                    ReplacePreProcVariablesAhead(1);
                    ReplacePreProcVariablesAhead(2);
                } else {
                    newInclude.Flags |= ParseFlag.NotFound;
                }
            }
        }

        #endregion

        #endregion
    }
}