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

using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                        case "property":
                        case "constructor":
                        case "destructor":
                        case "finally":
                        case "interface":
                        case "method":
                        case "for":
                        case "do":
                        case "repeat":
                        case "editing":
                        case "trigger": // trigger procedure
                            // increase block depth with a simple block
                            _context.BlockStack.Push(new ParsedScopeSimpleBlock(token.Value, token));
                            break;
                        case "end":
                            // decrease the block stack if we just finished an END statement
                            // The end of the block is not on the END token however, it is on the next EOS, find it
                            var nextEos = PeekAtNextType<TokenEos>(0);
                            if (nextEos is TokenEos) {
                                if (_context.BlockStack.Count <= 1 || (!CloseBlock<ParsedScopeBlock>(nextEos) && !CloseBlock<ParsedScopeSimpleBlock>(nextEos))) {
                                    // we matched an end with no begin
                                    _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedBlockEnd, token, _context.BlockStack.Count, _parsedIncludes));
                                }
                            }
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
                            var newProc = CreateParsedProcedure(token);
                            if (newProc != null) {
                                if (_context.BlockStack.Any(info => info.ScopeType == ParsedScopeType.Procedure || info.ScopeType == ParsedScopeType.Function))
                                    _parserErrors.Add(new ParserError(ParserErrorType.ForbiddenNestedBlockStart, token, _context.BlockStack.Count, _parsedIncludes));
                                _context.BlockStack.Push(newProc);
                            }

                            break;
                        case "function":
                            // parse a function definition
                            var newFunc = CreateParsedFunction(token);
                            if (newFunc != null) {
                                if (_context.BlockStack.Any(info => info.ScopeType == ParsedScopeType.Procedure || info.ScopeType == ParsedScopeType.Function))
                                    _parserErrors.Add(new ParserError(ParserErrorType.ForbiddenNestedBlockStart, token, _context.BlockStack.Count, _parsedIncludes));
                                _context.BlockStack.Push(newFunc);

                                // add the parameters to the list
                                if (newFunc.Parameters != null) {
                                    foreach (var parsedParameter in newFunc.Parameters) {
                                        AddParsedItem(parsedParameter, token.OwnerNumber);
                                    }
                                }
                            }

                            break;
                        case "on":
                            // parse a ON statement
                            var newOn = CreateParsedOnEvent(token);
                            if (newOn != null) {
                                // dont immediatly push it to the block stack because it doesn't necessarily have a trigger block (can only be a statement)
                                _context.LastOnBlock = newOn;
                            }
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
                            // matches a do in the middle of a statement (after a ON CHOOSE OF xx DO:)
                            var lastOnblock = _context.LastOnBlock as ParsedOnStatement;
                            // last ON scope started during the current statement, we are on the case of a ON CHOOSE OF xx DO:
                            if (lastOnblock != null && lastOnblock.Position == _context.StatementFirstToken.StartPosition) {
                                lastOnblock.HasTriggerBlock = true;
                                _context.BlockStack.Push(lastOnblock);
                            } else {
                                _context.BlockStack.Push(new ParsedScopeSimpleBlock(token.Value, token));
                            }
                            break;
                        case "triggers":
                            // Trigger phrase : this is, for instance, to handle a block in a "CREATE MENU-ITEM" statement
                            if (PeekAtNextNonType<TokenWhiteSpace>(0) is TokenEos)
                                _context.BlockStack.Push(new ParsedScopeSimpleBlock(token.Value, token));
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
                        if (!CloseBlock<ParsedScopePreProcIfBlock>(token)) {
                            // we match an end w/o beggining, flag a mismatch
                            _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedPreProcEndIf, token, _context.BlockStack.Count, _parsedIncludes));
                        }
                        switch (directiveLower) {
                            case "&endif":
                                NewStatement(token);
                                break;
                            case "&else":
                                // push a block to stack
                                _context.BlockStack.Push(CreateParsedIfEndIfPreProc(token));
                                NewStatement(token);
                                break;
                            case "&elseif":
                                // push a block to stack
                                _context.BlockStack.Push(CreateParsedIfEndIfPreProc(token));
                                break;
                        }
                        break;

                    case "&then":
                        // &then without a &if or &elseif
                        if (GetCurrentBlock<ParsedScopePreProcIfBlock>() == null) {
                            _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedPreprocThen, token, _context.BlockStack.Count, _parsedIncludes));
                        }
                        NewStatement(token);
                        break;

                    case "&if":
                        // push a block to stack
                        _context.BlockStack.Push(CreateParsedIfEndIfPreProc(token));
                        break;

                    default:
                        // should be the first word of a statement (otherwise this probably doesn't compile anyway!)
                        // if(_context.StatementWordCount == 1)
                        var newBlock = CreateParsedPreProcDirective(token);
                        if (newBlock != null) {
                            _context.BlockStack.Push(newBlock);
                        } else if (token.OwnerNumber == 0 && directiveLower.Equals("&analyze-resume")) {
                            if (!CloseBlock<ParsedScopePreProcBlock>(token)) {
                                // we match an end w/o beggining, flag a mismatch
                                _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedUibBlockEnd, token, _context.BlockStack.Count, _parsedIncludes));
                            }
                        }
                        NewStatement(PeekAt(0));
                        break;
                }
            }

            // potential function call
            else if (token is TokenSymbol && token.Value.Equals("(")) {
                var prevToken = PeekAtNextNonType<TokenWhiteSpace>(0, true);
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

            if (_context.StatementFirstToken != null) {
                               
                // store the line information for each line affected by the current statement
                if (_context.StatementFirstToken.OwnerNumber == 0) { // OwnerNumber == 0 to only consider blocks in the base file (not in includes)...
                    
                    // current indentation, the preproc directives do not increase the indentation
                    var depth = GetBlockDepth(0);
                    // the line below allows to not indent the first sentence that triggers the block...
                    if (depth > 0) {
                        if (_context.StatementFirstToken != null && _context.StatementFirstToken.Line == _context.BlockStack.Peek().Line)
                            depth = GetBlockDepth(1);
                    }

                    var statementStartLine = _context.StatementFirstToken.Line;
                    
                    if (!_lineInfo.ContainsKey(statementStartLine))
                        _lineInfo.Add(statementStartLine, new LineInfo(depth, GetCurrentBlock<ParsedScopeSection>(), GetCurrentBlock<ParsedScopeBlock>()));

                    // add missing values to the line dictionary 
                    // (lines from start statement + 1 to end of statement have a depth + 1)
                    // (can have a depth + 2 if the statement contains a ELSE or THEN)
                    if (statementStartLine > -1 && token.Line > statementStartLine) {
                        for (int i = statementStartLine + 1; i <= token.Line; i++) {
                            if (!_lineInfo.ContainsKey(i)) {
                                _lineInfo.Add(i, new LineInfo(depth + (_context.StatementFirstWordLineAferThenOrElse > 0 && i > _context.StatementFirstWordLineAferThenOrElse ? 2 : 1), GetCurrentBlock<ParsedScopeSection>(), GetCurrentBlock<ParsedScopeBlock>()));
                            }
                        }
                    }
                }
            }

            _context.StatementUnknownFirstWord = false;
            _context.StatementWordCount = 0;
            _context.StatementFirstToken = null;
            _context.StatementFirstWordLineAferThenOrElse = 0;
        }

        /// <summary>
        /// Allows to specify that the topmost block of the given type for the current stack must be closed.
        /// We fill in info on the block (ending position/line) as well as setting the correct depth for each line
        /// of this block
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="token"></param>
        /// <returns></returns>
        private bool CloseBlock<T>(Token token) where T : ParsedScope {

            var currentBlock = _context.BlockStack.Peek() as T;
            if (currentBlock != null) {
                currentBlock.EndBlockLine = token.Line;
                currentBlock.EndBlockPosition = token.EndPosition;

                int depth = GetBlockDepth(0);
                int closeStatementLine = (_context.StatementFirstToken != null ? _context.StatementFirstToken.Line : token.Line);

                // we just closed a block, fill the line info for all the lines of this block
                if (currentBlock.IncludeLine <= 0) { // lastBlock.IncludeLine <= 0 to only consider blocks in the base file (not in includes)...

                    for (int i = currentBlock.Line + 1; i <= closeStatementLine - 1; i++) {
                        if (!_lineInfo.ContainsKey(i)) {
                            _lineInfo.Add(i, new LineInfo(depth, GetCurrentBlock<ParsedScopeSection>(), GetCurrentBlock<ParsedScopeBlock>()));
                        }
                    }
                }
                
                _context.BlockStack.Pop();
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Returns the current block depth if initialIndex = 0. Or the parent block depth if = 1.
        /// (we do not count PreProc block directives like analyze-suspend/resume)
        /// </summary>
        /// <returns></returns>
        private int GetBlockDepth(int initialIndex) {
            // -1 because we always have a block that represents the current file
            var depth = -1;
            var i = initialIndex;
            while (i < _context.BlockStack.Count) {
                if (_context.BlockStack.ElementAt(i).ScopeType != ParsedScopeType.PreProcBlock) {
                    depth++;
                }
                i++;
            }
            return depth;
        }
        
        /// <summary>
        /// Would be the same as BlockStack.Peek() but only returns the topmost block of given type
        /// </summary>
        /// <returns></returns>
        private T GetCurrentBlock<T>() where T : ParsedScope {
            T output = _context.BlockStack.Peek() as T;
            var i = 0;
            while (output == null) {
                i++;
                if (i >= _context.BlockStack.Count)
                    break;
                output = _context.BlockStack.ElementAt(i) as T;
            }
            return output;
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

            item.Scope = GetCurrentBlock<ParsedScopeBlock>();
           
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
            flags |=  GetCurrentBlock<ParsedScopeBlock>() is ParsedFile ? ParseFlag.FileScope : ParseFlag.LocalScope;

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
        
        #endregion
    }
}