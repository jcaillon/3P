#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Text;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletionFeature;

namespace _3PA.MainFeatures.Parser.Pro {

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
                    token is TokenInclude)) {
                _context.StatementFirstToken = token;
                _context.StatementFirstTokenPosition = _tokenPos;
            }

            // matching a word
            if (token is TokenWord) {
                var lowerTok = token.Value.ToLower();

                // first word of a statement
                if (_context.StatementWordCount == 1 || _context.ReadNextWordAsStatementStart) {
                    if (_context.ReadNextWordAsStatementStart) {
                        _context.ReadNextWordAsStatementStart = false;
                        _context.StatementFirstWordLineAferThenOrElse = token.Line;
                    }

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
                            PushBlockInfoToStack(token.Line);
                            break;
                        case "end":
                            if (_context.BlockStack.Count > 0) {
                                // decrease block depth
                                _context.BlockStack.Pop();
                            } else
                                _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedBlockEnd, token, 0, _parsedIncludes));
                            break;
                        case "else":
                            // after a else, we need to consider the next word like it's the start of a new statement
                            // but we consider the whole else STATEMENT. as one single statement to correctly indent it!
                            _context.ReadNextWordAsStatementStart = true;
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
                                PushBlockInfoToStack(token.Line);
                            }
                            break;
                        case "function":
                            // parse a function definition
                            if (CreateParsedFunction(token)) {
                                if (_context.BlockStack.Count != 0)
                                    _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedBlockStart, token, _context.BlockStack.Count, _parsedIncludes));
                                _context.BlockStack.Clear();
                                PushBlockInfoToStack(token.Line);
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
                            PushBlockInfoToStack(token.Line);
                            break;
                        case "triggers":
                            if (PeekAtNextNonSpace(0) is TokenEos)
                                PushBlockInfoToStack(token.Line);
                            break;
                        case "then":
                            // after a then, we need to consider the next word like it's the start of a new statement
                            // but we consider the whole if then STATEMENT. as one single statement to correctly indent it!
                            var firstTokenLower = _context.StatementFirstToken.Value.ToLower();
                            if (firstTokenLower.Equals("if") || firstTokenLower.Equals("case"))
                                _context.ReadNextWordAsStatementStart = true;
                            break;
                        case "otherwise":
                            _context.ReadNextWordAsStatementStart = true;
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

            // pre processed statement
            else if (token is TokenPreProcDirective) {
                var directiveLower = token.Value.ToLower();
                switch (directiveLower) {
                    case "&endif":
                    case "&else":
                    case "&elseif":
                        // pop a block stack
                        if (_context.PreProcIfStack.Count > 0) {
                            var prevIf = _context.PreProcIfStack.Pop();
                            prevIf.EndBlockLine = token.Line;
                            prevIf.EndBlockPosition = token.EndPosition;
                        } else
                            _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedPreProcEndIf, token, 0, _parsedIncludes));

                        switch (directiveLower) {
                            case "&endif":
                                NewStatement(token);
                                break;
                            case "&else":
                                // push a block to stack
                                _context.PreProcIfStack.Push(CreateParsedIfEndIfPreProc(token));
                                NewStatement(token);
                                break;
                        }
                        break;
                        
                    case "&then":
                        var firstTokenLower = _context.StatementFirstToken.Value.ToLower();
                        if (!(firstTokenLower.Equals("&if") || firstTokenLower.Equals("&elseif"))) {
                            _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedPreprocThen, token, 0, _parsedIncludes));
                        }
                        // push a block to stack
                        _context.PreProcIfStack.Push(CreateParsedIfEndIfPreProc(_context.StatementFirstToken));
                        NewStatement(token);
                        break;

                    case "&if":
                        break;

                    default:
                        // should be the first word of a statement (otherwise this probably doesn't compile anyway!)
                        // if(_context.StatementWordCount == 1)
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
                if (_context.StatementUnknownFirstWord && _context.StatementWordCount == 2 && token.Value.Equals(":"))
                    CreateParsedLabel(token);
                NewStatement(token);
            }
        }

        /// <summary>
        /// before actually analysing the current token, we can do special stuff
        /// </summary>
        /// <param name="token"></param>
        private void AnalyseForEachToken(Token token) {
            if (token is TokenWord) {
                _context.StatementWordCount++;
            }
            
            // replace a pre proc var {&x} / include at current pos + 2
            // (why 2? because for for statement parsing, we need to peek at position +2)
            ReplaceIncludeAndPreprocVariablesAhead(2);
        }

        #endregion
        
        #region Utilities

                /// <summary>
        /// called when a Eos token is found, store information on the statement's line
        /// </summary>
        private void NewStatement(Token token) {

            // store the line information for the base file
            if (_context.StatementFirstToken != null && _context.StatementFirstToken.OwnerNumber == 0) {
                var statementStartLine = _context.StatementFirstToken.Line;

                var currentScope = _context.Scope.ScopeType == ParsedScopeType.Root && _context.UibBlockStack.Count > 0 ? _context.UibBlockStack.Peek() : _context.Scope;

                // remember the blockDepth of the current token's lin
                var depth = GetCurrentBlockDepth() + GetCurrentPreProcBlockDepth();
                if (!_lineInfo.ContainsKey(statementStartLine))
                    _lineInfo.Add(statementStartLine, new LineInfo(depth, currentScope));

                // add missing values to the line dictionary 
                // (lines from start statement + 1 to end of statement have a depth + 1)
                // (can have a depth + 2 if the statement contains a ELSE or THEN)
                if (statementStartLine > -1 && token.Line > statementStartLine) {
                    for (int i = statementStartLine + 1; i <= token.Line; i++)
                        if (!_lineInfo.ContainsKey(i))
                            _lineInfo.Add(i, new LineInfo(depth + (_context.StatementFirstWordLineAferThenOrElse > 0 && i > _context.StatementFirstWordLineAferThenOrElse ? 2 : 1), currentScope));
                }
            }
            
            // This statement made the BlockState count go to 0
            if (_context.BlockStack.Count == 0) {
                // did we match an end of a proc, func or on event block?
                if (!(_context.Scope is ParsedFile)) {
                    var parsedScope = (ParsedScopeItem) _parsedItemList.FindLast(item => item is ParsedScopeItem && !(item is ParsedPreProcBlock));
                    if (parsedScope != null) {
                        parsedScope.EndBlockLine = token.Line;
                        parsedScope.EndBlockPosition = token.EndPosition;
                    }
                    _context.Scope = _rootScope;
                }
            }

            _context.StatementUnknownFirstWord = false;
            _context.StatementCount++;
            _context.StatementWordCount = 0;
            _context.StatementFirstToken = null;
            _context.StatementFirstTokenPosition = 0;
            _context.StatementFirstWordLineAferThenOrElse = 0;
        }

        /// <summary>
        /// Returns the current block depth
        /// </summary>
        /// <returns></returns>
        private int GetCurrentBlockDepth() {
            var depth = 0;
            var lastLine = -1;
            foreach (var blockInfo in _context.BlockStack) {
                if (blockInfo.LineTriggerWord != lastLine)
                    depth++;
                lastLine = blockInfo.LineTriggerWord;
            }
            if (depth > 0 && _context.StatementFirstToken != null &&  _context.StatementFirstToken.Line == _context.BlockStack.Peek().LineStart)
                depth--;
            return depth;
        }

        /// <summary>
        /// Returns the current preproc &amp;if / &amp;endif block depth
        /// </summary>
        /// <returns></returns>
        private int GetCurrentPreProcBlockDepth() {
            var depth = 0;
            var lastLine = -1;
            foreach (var blockInfo in _context.PreProcIfStack) {
                if (blockInfo.Line != lastLine)
                    depth++;
                lastLine = blockInfo.Line;
            }
            if (depth > 0 && _context.StatementFirstToken != null && _context.StatementFirstToken.Line == _context.PreProcIfStack.Peek().Line)
                depth--;
            return depth;
        }

        /// <summary>
        /// Add a block info on top of the block Stack
        /// </summary>
        /// <param name="currentLine"></param>
        private void PushBlockInfoToStack(int currentLine) {
            _context.BlockStack.Push(new BlockInfo(_context.StatementFirstToken != null ? _context.StatementFirstToken.Line : 0, currentLine, _context.StatementCount));
        }

        /// <summary>
        /// Call this method instead of adding the items directly in the list,
        /// updates the scope and file name
        /// </summary>
        private void AddParsedItem(ParsedItem item, ushort ownerNumber) {
            // add external flag + include line if needed
            if (ownerNumber > 0 && ownerNumber < _parsedIncludes.Count) {
                item.FilePath = _parsedIncludes[ownerNumber].FullFilePath;
                item.IncludeLine = _parsedIncludes[ownerNumber].Line;
                item.Flags |= ParseFlag.FromInclude;
            } else {
                item.FilePath = _filePathBeingParsed;
            }

            item.Scope = _context.Scope;

            // add the item name's to the known temp tables?
            if (!_knownWords.ContainsKey(item.Name) && item is ParsedTable)
                _knownWords.Add(item.Name, CompletionType.Table);

            _parsedItemList.Add(item);
        }

        /// <summary>
        /// Append a token value to the StringBuilder, avoid adding too much spaces and new lines
        /// </summary>
        private void AddTokenToStringBuilder(StringBuilder strBuilder, Token token) {
            if ((token is TokenEol || token is TokenWhiteSpace)) {
                if (!_lastTokenWasSpace) {
                    _lastTokenWasSpace = true;
                    strBuilder.Append(" ");
                }
            } else {
                _lastTokenWasSpace = false;
                strBuilder.Append(token.Value);
            }
        }

        /// <summary>
        /// Returns token value or token value minus starting/ending quote of the token is a string
        /// </summary>
        private static string GetTokenStrippedValue(Token token) {
            if (token is TokenString) {
                var endsWithQuote = token.Value.EndsWith("\"") || token.Value.EndsWith("'");
                return token.Value.Substring(1, token.Value.Length - (endsWithQuote ? 2 : 1));
            }
            return token.Value;
        }

        /// <summary>
        /// Trim whitespaces tokens at the beginning and end of the list
        /// </summary>
        private static List<Token> TrimTokensList(List<Token> tokensList) {
            while (tokensList.Count > 0 && tokensList[0] is TokenWhiteSpace) {
                tokensList.RemoveAt(0);
            }
            while (tokensList.Count > 0 && tokensList[tokensList.Count - 1] is TokenWhiteSpace) {
                tokensList.RemoveAt(tokensList.Count - 1);
            }

            return tokensList;
        }

        /// <summary>
        /// Create a new parsed define item according to its type
        /// </summary>
        private ParsedDefine NewParsedDefined(string name, ParseFlag flags, Token token, ParsedAsLike asLike, string left, ParseDefineType type, string tempPrimitiveType, string viewAs, string bufferFor) {
            // set flags
            flags |= _context.Scope is ParsedFile ? ParseFlag.FileScope : ParseFlag.LocalScope;

            switch (type) {
                case ParseDefineType.Parameter:
                    flags |= ParseFlag.Parameter;
                    break;
                case ParseDefineType.Buffer:
                    flags |= ParseFlag.Buffer;

                    var newBuffer = new ParsedBuffer(name, token, asLike, left, type, tempPrimitiveType, viewAs, bufferFor, ConvertStringToParsedPrimitiveType(tempPrimitiveType, asLike == ParsedAsLike.Like)) {
                        TargetTable = FindAnyTableByName(bufferFor)
                    };

                    flags |= !bufferFor.Contains(".") && newBuffer.TargetTable != null && !newBuffer.TargetTable.IsTempTable ? ParseFlag.MissingDbName : 0;
                    newBuffer.Flags = flags;

                    return newBuffer;
                    
            }
            var newDefine = new ParsedDefine(name, token, asLike, left, type, tempPrimitiveType, viewAs, ConvertStringToParsedPrimitiveType(tempPrimitiveType, asLike == ParsedAsLike.Like)) {
                Flags = flags
            };
            return newDefine;
        }
        
        /// <summary>
        /// Parses a parameter definition (used in function, class method, class event)
        /// Returns a list of define parsed items representing the parameters of the function
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
                        if (lowerToken.Equals("for")) break;
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
                            if (!String.IsNullOrEmpty(paramName)) {
                                parametersList.Add(NewParsedDefined(paramName, flags, functionToken, paramAsLike, "", ParseDefineType.Parameter, paramPrimitiveType, "", parameterFor));
                            }
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

        #endregion
    }
}