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

using System.Linq;
using System.Text;
using _3PA.MainFeatures.AutoCompletionFeature;

namespace _3PA.MainFeatures.Parser.Pro.Parse {

    internal partial class Parser {

        #region Analyze

        private void Analyze() {

            var token = PeekAt(0);

            // reached end of file
            if (token is TokenEof)
                return;

            // matching a word
            if (token is TokenWord) {
                var lowerTok = token.Value.ToLower();

                // first word of a statement
                if (_context.CurrentStatement.WordCount == 1 || _context.ReadNextWordAsStatementStart) {

                    if (_context.ReadNextWordAsStatementStart) {
                        _context.ReadNextWordAsStatementStart = false;

                        // remember the line of the next token after a then/else/otherwise (useful for indenting statements)
                        var lastOnIndent = GetCurrentBlock<ParsedScopeOneStatementIndentBlock>();
                        if (lastOnIndent != null)
                            lastOnIndent.LineOfNextWord = token.Line;
                    }
                    
                    // matches a definition statement at the beginning of a statement
                    switch (lowerTok) {
                        case "else":
                            // after a else, we need to consider the next word like it's the start of a new statement
                            // but we consider the whole else STATEMENT. as one single statement to correctly indent it!
                            _context.ReadNextWordAsStatementStart = true;
                            PushNewOneStatementIndentBlock(token);
                            break;
                        case "otherwise":
                            _context.ReadNextWordAsStatementStart = true;
                            PushNewOneStatementIndentBlock(token);
                            break;
                        case "for":
                        case "repeat":
                        case "do":
                        case "case":
                        case "catch":
                        case "class":
                        case "property":
                        case "constructor":
                        case "destructor":
                        case "finally":
                        case "interface":
                        case "method":
                            // case of a THEN DO: for instance, we dont want to keep 2 block (=2 indent), we pop the THEN block
                            // and only keep the DO block
                            var topScope = _context.BlockStack.Peek() as ParsedScopeOneStatementIndentBlock;
                            if (topScope != null && _parsedStatementList.Count == topScope.StatementNumber ) {
                                _context.BlockStack.Pop();
                            }
                            // increase block depth with a simple block
                            var newBlock = new ParsedScopeSimpleBlock(token.Value, token);
                            _context.BlockStack.Push(newBlock);
                            // for labels, we need to know which block they are a header of; this is what we do below
                            if (_context.LastLabel != null) {
                                _context.LastLabel.Block = newBlock;
                                _context.LastLabel = null;
                            }
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
                            _context.LastOnBlock = CreateParsedOnEvent(token);
                            // dont immediatly push it to the block stack because it doesn't necessarily have a trigger block (can only be one statement)
                            break;
                        case "trigger": 
                            // trigger procedure
                            // TODO handle trigger procedure
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
                            _context.CurrentStatement.UnknownFirstWord = true;
                            break;
                    }
                } else {

                    // not the first word of a statement
                    switch (lowerTok) {
                        case "then":
                            // after a then, we need to consider the next word like it's the start of a new statement
                            // but we consider the whole if then STATEMENT. as one single statement to correctly indent it!
                            var firstTokenLower = _context.CurrentStatement.FirstToken.Value.ToLower();
                            switch (firstTokenLower) {
                                case "if":
                                case "when":
                                case "else":
                                case "otherwise":
                                    // only indent after a THEN if it was not used as a ternary expression
                                    _context.ReadNextWordAsStatementStart = true;
                                    PushNewOneStatementIndentBlock(token);
                                    break;
                            }
                            break;
                        case "editing":
                        case "do":
                            // matches a do in the middle of a statement (after a ON CHOOSE OF xx DO:) or an EDITING phrase
                            var newBlock = new ParsedScopeSimpleBlock(token.Value, token);
                            var lastOnblock = _context.LastOnBlock as ParsedOnStatement;
                            // last ON scope started during the current statement, we are on the case of a ON CHOOSE OF xx DO:
                            if (lastOnblock != null && lastOnblock.Position == _context.CurrentStatement.FirstToken.StartPosition) {
                                lastOnblock.TriggerBlock = newBlock;
                                _context.BlockStack.Push(lastOnblock);
                            } else {
                                _context.BlockStack.Push(newBlock);
                            }
                            break;
                        case "dynamic-function":
                            CreateParsedDynamicFunction(token);
                            break;
                        case "triggers":
                            // Trigger phrase : this is, for instance, to handle a block in a "CREATE MENU-ITEM" statement
                            if (PeekAtNextNonType<TokenWhiteSpace>(0) is TokenEos)
                                _context.BlockStack.Push(new ParsedScopeSimpleBlock(token.Value, token));
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

            // pre processed directive (&analyze-suspend, &analyze-resume, &scop/glob-define, &undefine)
            else if (token is TokenPreProcDirective) {

                if (token.Value.ToLower().Equals("&analyze-resume")) {

                    // end of a preprocessed (UIB / appbuilder) directive
                    if (token.OwnerNumber == 0) {
                        // it marks the end of an appbuilder block, it can only be at a root/File level
                        if (!(GetCurrentBlock<ParsedScopeBlock>() is ParsedFile)) {
                            _parserErrors.Add(new ParserError(ParserErrorType.NotAllowedUibBlockEnd, token, _context.BlockStack.Count, _parsedIncludes));
                        }

                        if (!CloseBlock<ParsedScopePreProcBlock>(token)) {
                            // we match an end w/o beggining, flag a mismatch
                            _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedUibBlockEnd, token, _context.BlockStack.Count, _parsedIncludes));
                        }
                    }
                }

                // should be the first word of a statement (otherwise this probably doesn't compile anyway!)
                if (token.OwnerNumber == 0 && _context.CurrentStatement.WordCount > 0) {
                    _parserErrors.Add(new ParserError(ParserErrorType.UibBlockStartMustBeNewStatement, token, _context.BlockStack.Count, _parsedIncludes));
                }

                // can create a new block if it is a UIB directive (&analyse-suspend) or create a preproc variable (and &undefine a variable)
                var newBlock = CreateParsedPreProcDirective(token);
                if (newBlock != null) {
                    _context.BlockStack.Push(newBlock);
                }
                    
                NewStatement(PeekAt(0)); // new statement is done on the EOL
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
                if (_context.CurrentStatement.UnknownFirstWord && _context.CurrentStatement.WordCount == 1 && token.Value.Equals(":")) {
                    _context.LastLabel = CreateParsedLabel(token);
                }

                NewStatement(token);
            }
        }

        /// <summary>
        /// before actually analysing the current token, we can do special stuff
        /// </summary>
        /// <param name="token"></param>
        private void AnalyseForEachToken(Token token) {

            // replace a pre proc var {&x} / include at current pos + 2
            ReplaceIncludeAndPreprocVariablesAhead(2); // (why 2? because for for statement parsing, we need to peek at position +2)

            var tokenIsWord = token is TokenWord;
            var tokenIsPreProcDirective = token is TokenPreProcDirective;

            // starting a new statement, we need to remember its starting line
            if (_context.CurrentStatementIsEnded && (tokenIsWord || tokenIsPreProcDirective)) {
                _context.CurrentStatementIsEnded = false;
                _context.CurrentStatement = new ParsedStatement(token);
                _parsedStatementList.Add(_context.CurrentStatement);

                if (tokenIsWord) {
                    // if we are not continuing a IF ... THEN with a ELSE we should pop all single indent block
                    if (!token.Value.ToLower().Equals("else")) {
                        PopOneStatementIndentBlock(0);
                    }
                }
            }

            // word
            if (tokenIsWord) {
                _context.CurrentStatement.WordCount++;
            }

            // pre processed if statement
            else if (tokenIsPreProcDirective) {


                var directiveLower = token.Value.ToLower();
                bool matched = true;
                switch (directiveLower) {
                    case "&endif":
                    case "&else":
                    case "&elseif":
                        // pop a block stack
                        var currentIfBlock = _context.BlockStack.Peek() as ParsedScopePreProcIfBlock;
                        if (!CloseBlock<ParsedScopePreProcIfBlock>(token)) {
                            // we match an end w/o beggining, flag a mismatch
                            _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedPreProcEndIf, token, _context.BlockStack.Count, _parsedIncludes));
                        }
                        switch (directiveLower) {
                            case "&endif":
                                NewStatement(token);
                                _context.LastPreprocIfwasTrue = false;
                                break;
                            case "&else":
                                // push a block to stack
                                var newElse = CreateParsedIfEndIfPreProc(token, _context.CurrentStatement.WordCount > 0);
                                _context.BlockStack.Push(newElse);
                                if (!_context.LastPreprocIfwasTrue && currentIfBlock != null) {
                                    // fill info on the else from the previous if/elseif
                                    newElse.ExpressionResult = true;
                                }
                                NewStatement(token);
                                break;
                            case "&elseif":
                                // push a block to stack
                                var newElseIf = CreateParsedIfEndIfPreProc(token, _context.CurrentStatement.WordCount > 0);
                                _context.BlockStack.Push(newElseIf);
                                _context.LastPreprocIfwasTrue = _context.LastPreprocIfwasTrue || newElseIf.ExpressionResult;
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
                        var newIf = CreateParsedIfEndIfPreProc(token, _context.CurrentStatement.WordCount > 0);
                        _context.BlockStack.Push(newIf);
                        _context.LastPreprocIfwasTrue = newIf.ExpressionResult;
                        break;

                    default:
                        matched = false;
                        break;
                }
                if (matched)
                    MoveNext();

            // End of line
            } else if (token is TokenEol) {
                AddLineInfo(token);

                // keywords like then/otherwise only increase the indentation for a single statement,
                // if the statement it indented is over then pop the indent block
                PopOneStatementIndentBlock(1);
            }

        }

        #endregion

        #region Utilities

        /// <summary>
        /// called when a Eos token is found, store information on the statement's line
        /// </summary>
        private void NewStatement(Token token) {
            AddLineInfo(token);
            _context.CurrentStatementIsEnded = true;
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
            while (_context.BlockStack.Peek() is ParsedScopeOneStatementIndentBlock) {
                _context.BlockStack.Pop();
            }
            var currentBlock = _context.BlockStack.Peek() as T;
            if (currentBlock != null) {
                currentBlock.EndBlockLine = token.Line;
                currentBlock.EndBlockPosition = token.EndPosition;
                _context.BlockStack.Pop();
                return true;
            }
            return false;
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
        /// Add info on the line of the given token
        /// </summary>
        /// <param name="token"></param>
        private void AddLineInfo(Token token) {
            // OwnerNumber > 0 to only consider lines in the base file (not in includes)...
            if (token.OwnerNumber > 0)
                return;

            var curLine = token.Line;
            if (!_lineInfo.ContainsKey(curLine)) {

                // fill lines info for the current statement
                if (!_context.CurrentStatementIsEnded) {

                    // fill missing info on the lines of the statement
                    for (int i = _context.CurrentStatement.FirstToken.Line; i <= curLine; i++) {
                        if (!_lineInfo.ContainsKey(i)) {
                            _lineInfo.Add(i, new ParsedLineInfo(_context.BlockStack, _context.CurrentStatement.FirstToken.Line));
                        }
                    }
                } else {
                    _lineInfo.Add(curLine, new ParsedLineInfo(_context.BlockStack, curLine));
                }
            }
        }

        /// <summary>
        /// Push a new one statement indent block with a condition
        /// </summary>
        private void PushNewOneStatementIndentBlock(Token token) {
            var currentBlock = _context.BlockStack.Peek() as ParsedScopeOneStatementIndentBlock;
            // safety to not indent twice if there are two THEN in the same line 
            // IF TRUE THEN IF TRUE THEN
            //     MESSAGE "ok".
            if (currentBlock != null && currentBlock.Line == token.Line)
                return;
            _context.BlockStack.Push(new ParsedScopeOneStatementIndentBlock(token.Value, token, _parsedStatementList.Count));
        }

        /// <summary>
        /// Pop one statement indent block if needed
        /// </summary>
        private void PopOneStatementIndentBlock(int maxPop) {
            // keywords like then/otherwise only increase the indentation for a single statement,
            // if the statement it indented is over then pop the indent block
            int i = 0;
            var topScope = _context.BlockStack.Peek() as ParsedScopeOneStatementIndentBlock;
            while (topScope != null && (_context.CurrentStatementIsEnded || _parsedStatementList.Count > topScope.StatementNumber) && (maxPop == 0 || i < maxPop)) {
                _context.BlockStack.Pop();
                topScope = _context.BlockStack.Peek() as ParsedScopeOneStatementIndentBlock;
                i++;
            }
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
            if (token is TokenString && !string.IsNullOrEmpty(token.Value)) {
                var beginsWithQuote = token.Value.First().Equals('"') || token.Value.First().Equals('\'');
                var endsWithQuote = token.Value.Last().Equals('"') || token.Value.Last().Equals('\'');
                if (token.Value.Length == 2 && beginsWithQuote && endsWithQuote)
                    return string.Empty;
                if (token.Value.Length == 1 && (beginsWithQuote || endsWithQuote))
                    return string.Empty;
                return token.Value.Substring(beginsWithQuote ? 1 : 0, token.Value.Length - (endsWithQuote ? 1 : 0) - (beginsWithQuote ? 1 : 0));
            }
            return token.Value;
        }
       
        #endregion
    }
}