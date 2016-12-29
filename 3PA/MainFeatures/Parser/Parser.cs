#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (Parser.cs) is part of 3P.
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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;

namespace _3PA.MainFeatures.Parser {

    /// <summary>
    /// This class is not actually a parser "per say" but it extracts important information
    /// from the tokens created by the lexer
    /// </summary>
    internal class Parser {

        #region static

        /// <summary>
        /// A dictionnary of known keywords and database info
        /// </summary>
        private static Dictionary<string, CompletionType> _knownStaticItems = new Dictionary<string, CompletionType>();

        public static void UpdateKnownStaticItems() {

            // Update the known items! (made of BASE.TABLE, TABLE and all the KEYWORDS)
            _knownStaticItems = DataBase.GetDbDictionnary();
            foreach (var keyword in Keywords.GetList().Where(keyword => !_knownStaticItems.ContainsKey(keyword.DisplayText))) {
                _knownStaticItems[keyword.DisplayText] = keyword.Type;
            }
        }

        #endregion

        #region private fields

        /// <summary>
        /// Represent the FILE LEVEL scope
        /// </summary>
        private ParsedScopeItem _rootScope;

        /// <summary>
        /// List of the parsed items (output)
        /// </summary>
        private List<ParsedItem> _parsedItemList = new List<ParsedItem>();

        /// <summary>
        /// Result of the lexer, list of tokens
        /// </summary>
        private GapBuffer<Token> _tokenList;
        private int _tokenPos = -1;

        /// <summary>
        /// Contains the current information of the statement's context (in which proc it is, which scope...)
        /// </summary>
        private ParseContext _context;

        /// <summary>
        /// Contains the information of each line parsed
        /// </summary>
        private Dictionary<int, LineInfo> _lineInfo = new Dictionary<int, LineInfo>();

        /// <summary>
        /// Path to the file being parsed (is added to the parseItem info)
        /// </summary>
        private string _filePathBeingParsed;

        private bool _lastTokenWasSpace;

        /// <summary>
        /// Contains all the words parsed
        /// </summary>
        private Dictionary<string, CompletionType> _knownWords = new Dictionary<string, CompletionType>(StringComparer.CurrentCultureIgnoreCase);

        private bool _matchKnownWords;

        /// <summary>
        /// Useful to remember where the function prototype was defined (Point is line, column)
        /// </summary>
        private Dictionary<string, ParsedFunction> _functionPrototype = new Dictionary<string, ParsedFunction>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// list of errors found by the parser
        /// </summary>
        private List<ParserError> _parserErrors = new List<ParserError>();

        /// <summary>
        /// Contains a dictionnary in which each variable name known corresponds to its value tokenized
        /// It can either be parameters from an include, ex: {1}->SHARED, {& name}->_extension
        /// or & DEFINE variables from the current file
        /// </summary>
        private Dictionary<string, List<Token>> _preProcVariables;

        #endregion

        #region public accessors

        /// <summary>
        /// dictionnay of *line, line info*
        /// </summary>
        public Dictionary<int, LineInfo> LineInfo {
            get { return _lineInfo; }
        }

        /// <summary>
        /// Returns the list of errors found by the parser
        /// </summary>
        public List<ParserError> ParserErrors {
            get { return _parserErrors; }
        }

        /// <summary>
        /// returns the list of the parsed items
        /// </summary>
        public List<ParsedItem> ParsedItemsList {
            get { return _parsedItemList; }
        }

        /// <summary>
        /// returns the list of the prototypes
        /// </summary>
        public Dictionary<string, ParsedFunction> ParsedPrototypes {
            get { return _functionPrototype; }
        }

        #endregion

        #region Life and death

        public Parser() {}

        /// <summary>
        /// Constructor with a string instead of a lexer
        /// </summary>
        public Parser(string data, string filePathBeingParsed, ParsedScopeItem defaultScope, Dictionary<string, List<Token>> includeParameters, bool matchKnownWords) : this(NewLexerFromData(data), filePathBeingParsed, defaultScope, includeParameters, matchKnownWords) { }

        /// <summary>
        /// Parses a text into a list of parsedItems
        /// </summary>
        public Parser(Lexer lexer, string filePathBeingParsed, ParsedScopeItem defaultScope, Dictionary<string, List<Token>> includeParameters, bool matchKnownWords) {

            // process inputs
            _filePathBeingParsed = filePathBeingParsed;
            _matchKnownWords = matchKnownWords && _knownStaticItems != null;
            _preProcVariables = includeParameters ?? new Dictionary<string, List<Token>>(StringComparer.CurrentCultureIgnoreCase);
            
            // the proprocessed variable {0} equals to the filename...
            _preProcVariables.Add("0", new List<Token> { new TokenWord(Path.GetFileName(_filePathBeingParsed), 0, 0, 0, 0) });

            // init context
            _context = new ParseContext {
                BlockStack = new Stack<BlockInfo>(),
                PreProcIfStack = new Stack<ParsedPreProcBlock>(),
                UibBlockStack = new Stack<ParsedPreProcBlock>()
            };

            // create root item
            if (defaultScope == null) {
                _rootScope = new ParsedFile("Root", new TokenEos(null, 0, 0, 0, 0));
                AddParsedItem(_rootScope);
            } else
                _rootScope = defaultScope;
            _context.Scope = _rootScope;

            // Analyse
            _tokenList = lexer.GetTokensList;
            ReplacePreProcVariablesAhead(1); // replaces a preproc var {&x} at token position 0
            ReplacePreProcVariablesAhead(2); // replaces a preproc var {&x} at token position 1
            while (MoveNext()) {
                Analyze();
            }

            // add missing values to the line dictionnary
            var current = new LineInfo(GetCurrentDepth(), _rootScope);
            for (int i = lexer.MaxLine - 1; i >= 0; i--) {
                if (_lineInfo.ContainsKey(i))
                    current = _lineInfo[i];
                else
                    _lineInfo.Add(i, current);
            }

            // check that we match an &ENDIF for each &IF
            if (_context.PreProcIfStack.Count > 0)
                _parserErrors.Add(new ParserError(ParserErrorType.MismatchNumberOfIfEndIf, PeekAt(0), _context.PreProcIfStack.Count));

            // dispose
            _context.BlockStack.Clear();
            _context.PreProcIfStack.Clear();
            _context.UibBlockStack.Clear();
            _context = null;
            _tokenList = null;
        }

        private static Lexer NewLexerFromData(string data) {
            return new Lexer(data);
        }

        #endregion

        #region Visitor implementation

        /// <summary>
        /// Feed this method with a visitor implementing IParserVisitor to visit all the parsed items
        /// </summary>
        /// <param name="visitor"></param>
        public void Accept(IParserVisitor visitor) {
            visitor.PreVisit();
            foreach (var item in _parsedItemList) {
                item.Accept(visitor);
            }
            visitor.PostVisit();
        }

        #endregion

        #region Explore tokens list
        
        /// <summary>
        /// Peek forward x tokens, returns an TokenEof if out of limits
        /// </summary>
        private Token PeekAt(int x) {
            return (_tokenPos + x >= _tokenList.Count || _tokenPos + x < 0) ? new TokenEof("", -1, -1, -1, -1) : _tokenList[_tokenPos + x];
        }

        /// <summary>
        /// Peek forward (or backward if goBackWard = true) until we match a token that is not a space token
        /// return found token
        /// </summary>
        private Token PeekAtNextNonSpace(int start, bool goBackward = false) {
            int x = start + (goBackward ? -1 : 1);
            var tok = PeekAt(x);
            while (tok is TokenWhiteSpace)
                tok = PeekAt(goBackward ? x-- : x++);
            return tok;
        }

        /// <summary>
        /// Move to the next token
        /// </summary>
        private bool MoveNext() {

            // before moving to the next token, we analyse the current token
            if (!_context.IsTokenIsEos && PeekAt(0) is TokenWord) {
                _context.StatementWordCount++;
            }
            _context.IsTokenIsEos = false;

            // move to the next token
            if (++_tokenPos >= _tokenList.Count)
                return false;

            // replace a pre proc var {&x} at current pos + 2
            ReplacePreProcVariablesAhead(2);

            return true;
        }

        /// <summary>
        /// Replace the token at the current pos + x by the token given
        /// </summary>
        public void ReplaceToken(int x, Token token) {
            if (_tokenPos + x < _tokenList.Count)
                _tokenList[_tokenPos + x] = token;
        }

        /// <summary>
        /// Inserts tokens at the current pos + x
        /// </summary>
        public void InsertTokens(int x, List<Token> tokens) {
            if (_tokenPos + x < _tokenList.Count)
                _tokenList.InsertRange(_tokenPos + x, tokens);
        }

        public void RemoveTokens(int x, int count) {
            if (_tokenPos + x + count <= _tokenList.Count)
                _tokenList.RemoveRange(_tokenPos + x, count);
        }

        /// <summary>
        /// Returns a list of tokens for a given string
        /// </summary>
        public List<Token> TokenizeString(string data) {
            var lexer = new Lexer(data);
            var outList = lexer.GetTokensList.ToList();
            outList.RemoveAt(outList.Count - 1);
            return outList;
        }

        #endregion

        #region Do the job 

        #region Analyse

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

                // Match splitted keywords...
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

                    // matches a definition statement at the beggining of a statement
                    switch (lowerTok) {
                        case "function":
                            // parse a function definition
                            if (CreateParsedFunction(token)) {
                                if (_context.BlockStack.Count != 0)
                                    _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedBlockStart, token, _context.BlockStack.Count));
                                _context.BlockStack.Clear();
                                PushBlockInfoToStack(IndentType.DoEnd, token.Line);
                            }
                            break;
                        case "procedure":
                        case "proce":
                            // parse a procedure definition
                            if (CreateParsedProcedure(token)) {
                                if (_context.BlockStack.Count != 0)
                                    _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedBlockStart, token, _context.BlockStack.Count));
                                _context.BlockStack.Clear();
                                PushBlockInfoToStack(IndentType.DoEnd, token.Line);
                            }
                            break;
                        case "on":
                            // parse a ON statement
                            CreateParsedOnEvent(token);
                            break;
                        case "def":
                        case "define":
                            // Parse a define statement
                            CreateParsedDefine(token, false);
                            break;
                        case "create":
                            // Parse a create statement
                            CreateParsedDefine(token, true);
                            break;
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
                                _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedBlockEnd, token, 0));
                            break;
                        case "else":
                            // add a one time indent after a then or else
                            PushBlockInfoToStack(IndentType.ThenElse, token.Line);
                            NewStatement(token);
                            break;
                        case "run":
                            // Parse a run statement
                            CreateParsedRun(token);
                            break;
                        case "dynamic-function":
                            CreateParsedDynamicFunction(token);
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
                            // try to match a known keyword
                            if (_matchKnownWords) {
                                if (_knownStaticItems.ContainsKey(lowerTok)) {
                                    // we known the word
                                    if (_knownStaticItems[lowerTok] == CompletionType.Table) {
                                        // it's a table from the database
                                        AddParsedItem(new ParsedFoundTableUse(token.Value, token, false));
                                    }
                                } else if (_knownWords.ContainsKey(lowerTok)) {
                                    if (_knownWords[lowerTok] == CompletionType.Table) {
                                        // it's a temp table
                                        AddParsedItem(new ParsedFoundTableUse(token.Value, token, true));
                                    }
                                }
                            }
                            break;
                    }
                }
            }

            // include
            else if (token is TokenInclude) {
                if (CreateParsedIncludeFile(token))
                    NewStatement(PeekAt(0));
            }

            // pre processed statement
            else if (token is TokenPreProcDirective) {

                var directiveLower = token.Value.ToLower();

                // should be the first word of a statement (otherwise this probably doesn't compile anyway!)
                if (_context.StatementWordCount == 0) {

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
                                _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedIfEndIfBlockEnd, token, 0));
                            
                            if (directiveLower == "&elseif") {
                                _context.PreProcIfStack.Push(CreateParsedIfEndIfPreProc(token));
                            } else {
                                NewStatement(token);
                            }
                            break;

                        default:
                            if (CreateParsedPreProcDirective(token))
                                NewStatement(PeekAt(0));
                            break;
                    }

                } else {
                    switch (directiveLower) {
                        case "&then":
                            NewStatement(token);
                            break;
                    }
                    
                }
            }

            // potential function call
            else if (token is TokenSymbol && token.Value.Equals("(")) {
                var prevToken = PeekAtNextNonSpace(0, true);
                if (prevToken is TokenWord && _functionPrototype.ContainsKey(prevToken.Value))
                    AddParsedItem(new ParsedFunctionCall(prevToken.Value, prevToken, false));
            }

            // end of statement
            else if (token is TokenEos) {
                // match a label if there was only one word followed by : in the statement
                if (_context.StatementUnknownFirstWord && _context.StatementWordCount == 1 && token.Value.Equals(":"))
                    CreateParsedLabel();
                NewStatement(token);
            }
        }

        #endregion

        #region utils

        /// <summary>
        /// If it is a pre-processed variable, replaces a token at "current position + posAhead" by its value
        /// </summary>
        private void ReplacePreProcVariablesAhead(int posAhead) {

            // we check if the token + posAhead will be a proprocessed variable { & x} that needs to be replaced
            var toReplaceToken = PeekAt(posAhead);
            
            while (toReplaceToken is TokenPreProcVariable) {

                // replace the {&var} present within this {&var}
                var count = 1;
                while (true) {
                    var curToken = PeekAt(posAhead + count);
                    if (curToken is TokenSymbol || curToken is TokenEof) break;
                    ReplacePreProcVariablesAhead(posAhead + count);
                    count++;
                }

                var nameToken = PeekAt(posAhead + 1);
                var varName = (toReplaceToken.Value == "{" ? "" : "&") + nameToken.Value;
                List<Token> valueTokens;

                // count nb of tokens composing this |{&|name|  |}| (will 3 or more depending if there are spaces after the name)
                count = 1;
                while (true) {
                    var curToken = PeekAt(posAhead + count);
                    if (curToken is TokenSymbol || curToken is TokenEof) break;
                    count++;
                }
                count++;

                // remove the tokens composing |{&|name|  |}|
                RemoveTokens(posAhead, count);


                if (!_preProcVariables.ContainsKey(varName)) {
                    // if we don't have the definition for the variable, it must be replaced by an empty string
                    valueTokens = new List<Token> {
                        new TokenWhiteSpace("", toReplaceToken.Line, toReplaceToken.Column, toReplaceToken.StartPosition, toReplaceToken.EndPosition)
                    };
                } else {
                    valueTokens = _preProcVariables[varName].ToList();

                    // we have to "merge" the TokenWord at the beggining and end of what we are inserting, this allows to take care of
                    // cases like : DEF VAR lc_truc{&extension} AS CHAR NO-UNDO.
                    var prevToken = PeekAt(posAhead - 1);
                    if (valueTokens.FirstOrDefault() is TokenWord && prevToken is TokenWord) {
                        // append previous word with the first word of the value tokens
                        ReplaceToken(posAhead - 1, new TokenWord(prevToken.Value + valueTokens.First().Value, prevToken.Line, prevToken.Column, prevToken.StartPosition, prevToken.EndPosition));
                        valueTokens.RemoveAt(0);
                    }
                    var nextToken = PeekAt(posAhead);
                    if (valueTokens.LastOrDefault() is TokenWord && nextToken is TokenWord) {
                        ReplaceToken(posAhead, new TokenWord(valueTokens.Last().Value + nextToken.Value, nextToken.Line, nextToken.Column, nextToken.StartPosition, nextToken.EndPosition));
                        valueTokens.RemoveAt(valueTokens.Count - 1);
                    }
                }

                // if we have tokens insert, do it
                if (valueTokens.Count > 0)
                    InsertTokens(posAhead, valueTokens);
                else {
                    // otherwise, make sure we don't have two TokenWord following each other
                    var prevToken = PeekAt(posAhead - 1);
                    var nextToken = PeekAt(posAhead);
                    if (prevToken is TokenWord && PeekAt(posAhead) is TokenWord) {
                        ReplaceToken(posAhead - 1, new TokenWord(prevToken.Value + nextToken.Value, prevToken.Line, prevToken.Column, prevToken.StartPosition, nextToken.EndPosition));
                        RemoveTokens(posAhead, 1);
                    }
                }
                toReplaceToken = PeekAt(posAhead);
            }
        }

        /// <summary>
        /// called when a Eos token is found, store information on the statement's line
        /// </summary>
        private void NewStatement(Token token) {

            var statementStartLine = _context.StatementFirstToken != null ? _context.StatementFirstToken.Line : 0;

            // remember the blockDepth of the current token's line (add block depth if the statement started after else of then)
            var depth = GetCurrentDepth();
            if (!_lineInfo.ContainsKey(statementStartLine))
                _lineInfo.Add(statementStartLine, new LineInfo(depth, _context.Scope));

            // add missing values to the line dictionnary
            if (statementStartLine > -1 && token.Line > statementStartLine) {
                for (int i = statementStartLine + 1; i <= token.Line; i++)
                    if (!_lineInfo.ContainsKey(i))
                        _lineInfo.Add(i, new LineInfo(depth + 1, _context.Scope));
            }

            // Pop all the then/else blocks that are on top
            if (_context.BlockStack.Count > 0 && _context.BlockStack.Peek().StatementNumber != _context.StatementCount)
                while (_context.BlockStack.Peek().IndentType == IndentType.ThenElse) {
                    _context.BlockStack.Pop();
                    if (_context.BlockStack.Count == 0)
                        break;
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
            _context.IsTokenIsEos = true;
        }

        /// <summary>
        /// Returns the current block depth
        /// </summary>
        /// <returns></returns>
        private int GetCurrentDepth() {
            var depth = 0;
            var lastLine = -1;
            bool lastStackThenDo = false;
            foreach (var blockInfo in _context.BlockStack) {
                if (blockInfo.LineTriggerWord != lastLine)
                    depth++;
                else if (depth == 1)
                    lastStackThenDo = true;
                lastLine = blockInfo.LineTriggerWord;
            }
            if (depth > 0 && _context.StatementFirstToken != null && _context.StatementFirstToken.Line == _context.BlockStack.Peek().LineStart && !lastStackThenDo)
                depth--;
            return depth;
        }

        /// <summary>
        /// Add a block info on top of the block Stack
        /// </summary>
        /// <param name="indentType"></param>
        /// <param name="currentLine"></param>
        private void PushBlockInfoToStack(IndentType indentType, int currentLine) {
            _context.BlockStack.Push(new BlockInfo(_context.StatementFirstToken != null ? _context.StatementFirstToken.Line : 0, currentLine, indentType, _context.StatementCount));
        }

        /// <summary>
        /// Call this method instead of adding the items directly in the list,
        /// updates the scope and file name
        /// </summary>
        private void AddParsedItem(ParsedItem item) {

            item.FilePath = _filePathBeingParsed;
            item.Scope = _context.Scope;

            // add the item name's to the known words
            if (!_knownWords.ContainsKey(item.Name)) {
                _knownWords.Add(item.Name, (item is ParsedTable) ? CompletionType.Table : CompletionType.Keyword);
            }

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
        private string GetTokenStrippedValue(Token token) {
            return (token is TokenString) ? token.Value.Substring(1, token.Value.Length - 2) : token.Value;
        }

        /// <summary>
        /// Trim whitespaces tokens at the beggining and end of the list
        /// </summary>
        private List<Token> TrimTokensList(List<Token> tokensList) {

            while (tokensList.Count > 0 && tokensList[0] is TokenWhiteSpace) {
                tokensList.RemoveAt(0);
            }
            while (tokensList.Count > 0 && tokensList[tokensList.Count - 1] is TokenWhiteSpace) {
                tokensList.RemoveAt(tokensList.Count - 1);
            }

            return tokensList;
        } 

        #endregion

        #region Read a statement, create Parsed values

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

            AddParsedItem(new ParsedFunctionCall(name, tokenFun, !_functionPrototype.ContainsKey(name)));
        }

        /// <summary>
        /// Creates a label parsed item
        /// </summary>
        private void CreateParsedLabel() {
            AddParsedItem(new ParsedLabel(_context.StatementFirstToken.Value, _context.StatementFirstToken));
        }

        /// <summary>
        /// Creates a parsed item for RUN statements
        /// </summary>
        /// <param name="runToken"></param>
        private void CreateParsedRun(Token runToken) {
            // info we will extract from the current statement :
            string name = "";
            bool isValue = false;
            bool hasPersistent = false;
            _lastTokenWasSpace = true;
            StringBuilder leftStr = new StringBuilder();
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
                        } else if (isValue && !(token is TokenWhiteSpace || token is TokenSymbol)) {
                            name += GetTokenStrippedValue(token);
                        } else if (token is TokenWord) {
                            if (token.Value.ToLower().Equals("value"))
                                isValue = true;
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
                            hasPersistent = true;
                        state++;
                        break;
                }
            } while (MoveNext());

            if (state == 0) return;
            AddParsedItem(new ParsedRun(name, runToken, leftStr.ToString(), isValue, hasPersistent));
        }

        /// <summary>
        /// Creates parsed item for ON CHOOSE OF XXX events
        /// (choose or anything else)
        /// </summary>
        /// <param name="onToken"></param>
        /// <returns></returns>
        private void CreateParsedOnEvent(Token onToken) {
            // info we will extract from the current statement :
            var widgetList = new StringBuilder();
            var eventList = new StringBuilder();
            int state = 0;
            do {
                var token = PeekAt(1); // next token
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                switch (state) {
                    case 0:
                        // matching event type
                        if (token is TokenWord || token is TokenString || (token is TokenSymbol && token.Value == "*")) {
                            eventList.Append((eventList.Length == 0 ? "" : ", ") + GetTokenStrippedValue(token));
                            state++;
                        }
                        break;
                    case 1:
                        // matching "of"
                        if (token is TokenSymbol && token.Value.Equals(",")) {
                            state--;
                            break;
                        }
                        if (!(token is TokenWord)) break;
                        if (token.Value.EqualsCi("anywhere")) {
                            // we match anywhere, need to return to match a block start
                            widgetList.Append("anywhere");
                            var new1 = new ParsedOnStatement(eventList + " " + widgetList, onToken, eventList.ToString(), widgetList.ToString());
                            AddParsedItem(new1);
                            _context.Scope = new1;
                            return;
                        }
                        // if not anywhere, we expect an "of"
                        if (token.Value.EqualsCi("of")) {
                            state++;
                            break;
                        }
                        // otherwise, return
                        return;
                    case 2:
                        // matching widget name
                        if (token is TokenWord || token is TokenString) {
                            widgetList.Append((widgetList.Length == 0 ? "" : ", ") + GetTokenStrippedValue(token));

                            // we can match several widget name separated by a comma or resume to next state
                            if (token.Value.ToLower() == "frame") {
                                state = 4;
                                break;
                            }

                            var nextNonSpace = PeekAtNextNonSpace(1);
                            if (!(nextNonSpace is TokenSymbol && nextNonSpace.Value.Equals(",")))
                                state++;
                        }
                        break;
                    case 4:
                        if (token is TokenWord || token is TokenString) {
                            widgetList.Append(" ").Append(token.Value);
                            state = 2;

                            var nextNonSpace = PeekAtNextNonSpace(1);
                            if (!(nextNonSpace is TokenSymbol && nextNonSpace.Value.Equals(",")))
                                state++;
                        }
                        break;
                    case 3:
                        // matching "or", create another parsed item, otherwise leave to match a block start
                        if (!(token is TokenWord)) break;
                        var new2 = new ParsedOnStatement(eventList + " " + widgetList, onToken, eventList.ToString(), widgetList.ToString());
                        AddParsedItem(new2);
                        _context.Scope = new2;
                        if (token.Value.EqualsCi("or")) {
                            state = 0;
                            widgetList.Clear();
                            eventList.Clear();
                        } else
                            return;
                        break;
                }
            } while (MoveNext());
        }

        /// <summary>
        /// Matches a new definition
        /// </summary>
        private void CreateParsedDefine(Token functionToken, bool isDynamic) {

            // info we will extract from the current statement :
            string name = "";
            ParsedAsLike asLike = ParsedAsLike.None;
            ParseDefineType type = ParseDefineType.None;
            string tempPrimitiveType = "";
            string viewAs = "";
            string bufferFor = "";
            bool isExtended = false;
            _lastTokenWasSpace = true;
            StringBuilder left = new StringBuilder();
            StringBuilder strFlags = new StringBuilder();

            // for temp tables:
            string likeTable = "";
            bool isTempTable = false;
            var fields = new List<ParsedField>();
            ParsedField currentField = new ParsedField("", "", "", 0, ParsedFieldFlag.None, "", "", ParsedAsLike.None);
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
                                var token1 = lowerToken;
                                foreach (var typ in Enum.GetNames(typeof (ParseDefineType)).Where(typ => token1.Equals(typ.ToLower()))) {
                                    type = (ParseDefineType) Enum.Parse(typeof (ParseDefineType), typ, true);
                                    break;
                                }
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
                            case "global":
                            case "shared":
                            case "private":
                            case "protected":
                            case "public":
                            case "static":
                            case "abstract":
                            case "override":
                                // flags found before the type
                                if (strFlags.Length > 0)
                                    strFlags.Append(" ");
                                strFlags.Append(lowerToken);
                                break;
                            case "input":
                            case "output":
                            case "input-output":
                            case "return":
                                // flags found before the type in case of a define parameter
                                strFlags.Append(lowerToken);
                                break;
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
                        if (lowerToken.Equals("extent")) isExtended = true;
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
                                currentField.Flag = currentField.Flag | ParsedFieldFlag.Extent;
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
                        currentField = new ParsedField(token.Value, "", "", 0, ParsedFieldFlag.None, "", "", ParsedAsLike.None);
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

                AddParsedItem(new ParsedTable(name, functionToken, "", "", name, "", likeTable, true, fields, indexList, new List<ParsedTrigger>(), strFlags.ToString(), useIndex.ToString()) {
                    // = end position of the EOS of the statement
                    EndPosition = token.EndPosition
                });
            } else
                AddParsedItem(new ParsedDefine(name, functionToken, strFlags.ToString(), asLike, left.ToString(), type, tempPrimitiveType, viewAs, bufferFor, isExtended, isDynamic) {
                    // = end position of the EOS of the statement
                    EndPosition = token.EndPosition
                });
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
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                // a ~ allows for a eol but we don't control if it's an eol because if it's something else we probably parsed it wrong anyway (in the lexer)
                if (token is TokenSymbol && token.Value == "~") {
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

            ParsedPreProcVariableType newPreProcVarType = 0;

            // match first word of the statement
            switch (directiveToken.Value.ToUpper()) {
                case "&GLOBAL-DEFINE":
                case "&GLOBAL":
                case "&GLOB":
                    newPreProcVarType = ParsedPreProcVariableType.Global;
                    break;

                case "&SCOPED-DEFINE":
                case "&SCOPED":
                    newPreProcVarType = ParsedPreProcVariableType.Scope;
                    break;

                case "&ANALYZE-SUSPEND":
                    // it marks the beggining of an appbuilder block, it can only be at a root/File level, otherwise flag error
                    if (!(_context.Scope is ParsedFile)) {
                        _parserErrors.Add(new ParserError(ParserErrorType.NotAllowedUibBlockStart, directiveToken, 0));
                        _context.Scope = _rootScope;
                    }

                    // we match a new block start but we didn't match the previous block end, flag error
                    if (_context.UibBlockStack.Count > 0) {
                        _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedUibBlockStart, directiveToken, _context.UibBlockStack.Count));
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
                        BlockDescription = textAfterDirective,
                    });

                    // save the block description
                    AddParsedItem(_context.UibBlockStack.Peek());
                    break;

                case "&ANALYZE-RESUME":
                    // it marks the end of an appbuilder block, it can only be at a root/File level
                    if (!(_context.Scope is ParsedFile)) {
                        _parserErrors.Add(new ParserError(ParserErrorType.NotAllowedUibBlockEnd, directiveToken, 0));
                        _context.Scope = _rootScope;
                    }
                    
                    if (_context.UibBlockStack.Count == 0) {
                        // we match an end w/o beggining, flag a mismatch
                        _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedUibBlockEnd, directiveToken, 0));
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
            if (newPreProcVarType > 0 && !string.IsNullOrEmpty(variableName)) {
                AddParsedItem(new ParsedPreProcVariable(variableName, directiveToken, 0, ParsedPreProcVariableType.Global, definition.ToString().Trim()));

                // add it to the know variables
                if (_preProcVariables.ContainsKey("&" + variableName))
                    _preProcVariables["&" + variableName] = TrimTokensList(tokensList);
                else
                    _preProcVariables.Add("&" + variableName, TrimTokensList(tokensList));
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
                BlockDescription = expression.ToString(),
            };
            AddParsedItem(newIf);

            return newIf;

        }

        /// <summary>
        /// Matches a procedure definition
        /// </summary>
        private bool CreateParsedProcedure(Token procToken) {
            // info we will extract from the current statement :
            string name = "";
            bool isExternal = false;
            bool isPrivate = false;
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
                        if (token.Value.EqualsCi("external")) isExternal = true;
                        if (token.Value.EqualsCi("private")) isPrivate = true;
                        break;
                }
                AddTokenToStringBuilder(leftStr, token);
            } while (MoveNext());

            if (state < 1) return false;
            var newProc = new ParsedProcedure(name, procToken, leftStr.ToString(), isExternal, isPrivate) {
                // = end position of the EOS of the statement
                EndPosition = token.EndPosition
            };
            AddParsedItem(newProc);
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
            bool isExtent = false;
            _lastTokenWasSpace = true;
            StringBuilder parameters = new StringBuilder();
            bool isPrivate = false;
            var parametersList = new List<ParsedItem>();

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
                                isPrivate = true;
                            if (token.Value.EqualsCi("extent"))
                                isExtent = true;

                            // we didn't match any opening (, but we found a forward
                            if (token.Value.EqualsCi("forward"))
                                state = 99;
                            else if (token.Value.EqualsCi("in"))
                                state = 100;

                        } else if (token is TokenSymbol && token.Value.Equals("("))
                            state = 3;
                        else if (isExtent && token is TokenNumber)
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
                    FilePath = _filePathBeingParsed,
                    SimpleForward = state == 99, // allows us to know if we expect an implementation in this .p or not
                    EndPosition = token.EndPosition,
                    EndBlockLine = token.Line,
                    EndBlockPosition = token.EndPosition,
                    IsPrivate = isPrivate,
                    IsExtended = isExtent,
                    Extend = extend ?? string.Empty,
                    Parameters = parameters.ToString()
                };
                if (!_functionPrototype.ContainsKey(name))
                    _functionPrototype.Add(name, createdProto);

                // case of a IN, we add it to the list of item
                if (!createdProto.SimpleForward) {
                    
                    AddParsedItem(createdProto);

                    // modify context
                    _context.Scope = createdProto;

                    // add the parameters to the list
                    if (parametersList.Count > 0) {
                        foreach (var parsedItem in parametersList) {
                            AddParsedItem(parsedItem);
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
                IsPrivate = isPrivate,
                IsExtended = isExtent,
                Extend = extend ?? string.Empty,
                Parameters = parameters.ToString()
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
                        createdImp.IsExtended == proto.IsExtended &&
                        createdImp.IsPrivate == proto.IsPrivate &&
                        createdImp.Extend.Equals(proto.Extend) &&
                        createdImp.ParsedReturnType.Equals(proto.ParsedReturnType) &&
                        createdImp.Parameters.Equals(proto.Parameters));
                }
            } else {
                _functionPrototype.Add(name, createdImp);
            }

            AddParsedItem(createdImp);

            // modify context
            _context.Scope = createdImp;
            
            // add the parameters to the list
            if (parametersList.Count > 0) {
                foreach (var parsedItem in parametersList) {
                    AddParsedItem(parsedItem);
                }
            }

            return true;
        }

        /// <summary>
        /// Parses a parameter definition (used in function, class method, class event)
        /// </summary>
        private List<ParsedItem> GetParsedParameters(Token functionToken, StringBuilder parameters) {
            // info the parameters
            string paramName = "";
            ParsedAsLike paramAsLike = ParsedAsLike.None;
            string paramPrimitiveType = "";
            string strFlags = "";
            string parameterFor = "";
            bool isExtended = false;
            var parametersList = new List<ParsedItem>();

            int state = 0;
            do {
                var token = PeekAt(1); // next token
                if (token is TokenEos) break;
                if (token is TokenSymbol && (token.Value.Equals(")"))) state = 99;
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
                            case "return":
                            case "input":
                            case "output":
                            case "input-output":
                                // flags found before the type in case of a define parameter
                                strFlags = lwToken;
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
                        if (token is TokenWord && token.Value.EqualsCi("extent")) isExtended = true;
                        else if (token is TokenSymbol && (token.Value.Equals(")") || token.Value.Equals(","))) {
                            // create a variable for this function scope
                            if (!String.IsNullOrEmpty(paramName))
                                parametersList.Add(new ParsedDefine(paramName, functionToken, strFlags, paramAsLike, "", ParseDefineType.Parameter, paramPrimitiveType, "", parameterFor, isExtended, false));
                            paramName = "";
                            paramAsLike = ParsedAsLike.None;
                            paramPrimitiveType = "";
                            strFlags = "";
                            parameterFor = "";
                            isExtended = false;

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


        /// <summary>
        /// matches an include file
        /// </summary>
        private bool CreateParsedIncludeFile(Token bracketToken) {

            // This method should handle those cases :
            // {  file.i &name=val &2="value"} -> {&name} and {&2}
            // {file.i val "value"} -> {1} {2}

            // info we will extract from the current statement :
            string fileName = "";
            bool usesNamedArg = false; // true if the arguments used are with the format : &name=""
            bool expectingFirstArg = true;
            string argName = null;
            int argNumber = 1;
            var parameters = new Dictionary<string, List<Token>>(StringComparer.CurrentCultureIgnoreCase);

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
                if (token is TokenSymbol && token.Value == "}") break;
                switch (state) {
                    case 0:
                        // read the file name
                        if (token is TokenWord) {
                            fileName += token.Value;
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
            if (!string.IsNullOrEmpty(fileName))
                AddParsedItem(new ParsedIncludeFile(fileName, bracketToken, parameters));

            return true;
        }

        #endregion

        #endregion

        #region internal classes

        /// <summary>
        /// contains the info on the current context (as we move through tokens)
        /// </summary>
        internal class ParseContext {

            /// <summary>
            /// Keep information on the current scope (file, procedure, function, trigger)
            /// </summary>
            public ParsedScopeItem Scope { get; set; }

            /// <summary>
            /// Number of words count in the current statement
            /// </summary>
            public int StatementWordCount { get; set; }

            /// <summary>
            /// The total number of statements found
            /// </summary>
            public int StatementCount { get; set; }

            /// <summary>
            /// A statement can start with a word, pre-proc phrase or an include
            /// </summary>
            public Token StatementFirstToken { get; set; }

            /// <summary>
            /// True if the first word of the statement didn't match a known statement
            /// </summary>
            public bool StatementUnknownFirstWord { get; set; }

            /// <summary>
            /// True if the current token (PeekAt(0)) should be considered as an end of statement
            /// </summary>
            public bool IsTokenIsEos { get; set; }

            /// <summary>
            /// Keep tracks on blocks through a stack (a block == an indent)
            /// </summary>
            public Stack<BlockInfo> BlockStack { get; set; }

            /// <summary>
            /// Stack of ANALYSE-SUSPEND/RESUME blocks
            /// </summary>
            public Stack<ParsedPreProcBlock> UibBlockStack { get; set; }

            /// <summary>
            /// To know the current depth for IF ENDIF pre-processed statement, allows us
            /// to know if the document is correct or not
            /// </summary>
            public Stack<ParsedPreProcBlock> PreProcIfStack { get; set; }

        }

        /// <summary>
        /// Contains info on a block
        /// </summary>
        internal struct BlockInfo {

            /// <summary>
            /// The line of the first token of the statement that contains the "trigger word"
            /// </summary>
            public int LineStart { get; set; }

            /// <summary>
            /// The trigger word is the word token that creates a new block (e.g. FUNCTION or DO)
            /// In case of a DO, it's necesseraly on the same line of the statement starting token
            /// </summary>
            public int LineTriggerWord { get; set; }
            public IndentType IndentType { get; set; }

            /// <summary>
            /// the total statement count at the moment this block was created
            /// </summary>
            public int StatementNumber { get; set; }

            public BlockInfo(int lineStart, int lineTriggerWord, IndentType indentType, int statementNumber)
                : this() {
                LineStart = lineStart;
                LineTriggerWord = lineTriggerWord;
                IndentType = indentType;
                StatementNumber = statementNumber;
            }
        }

        internal enum IndentType {
            /// <summary>
            /// A do-end means that the indent extends from the line with the DO to the line with the END
            /// </summary>
            DoEnd,
            /// <summary>
            /// A then/else means the indent is only applied until the next first statement ends
            /// </summary>
            ThenElse
        }

        #endregion

    }

    #region LineInfo

    /// <summary>
    /// Contains the info of a specific line number (built during the parsing)
    /// </summary>
    internal class LineInfo {

        /// <summary>
        /// Block depth for the current line (= number of indents)
        /// </summary>
        public int BlockDepth { get; set; }

        /// <summary>
        /// Scope for the current line
        /// </summary>
        public ParsedScopeItem Scope { get; set; }


        public LineInfo(int blockDepth, ParsedScopeItem scope) {
            BlockDepth = blockDepth;
            Scope = scope;
        }
    }

    #endregion

    #region ParserError

    internal class ParserError {

        /// <summary>
        /// Type of the error
        /// </summary>
        public ParserErrorType Type { get; set; }

        /// <summary>
        /// Line at which the error happened
        /// </summary>
        public int TriggerLine { get; set; }

        /// <summary>
        /// Position at which the error happened
        /// </summary>
        public int TriggerPosition { get; set; }

        /// <summary>
        /// Stack count at the moment of the error (the type of stack will depend on the error)
        /// </summary>
        public int StackCount { get; set; }

        public ParserError(ParserErrorType type, Token triggerToken, int stackCount) {
            Type = type;
            TriggerLine = triggerToken.Line;
            TriggerPosition = triggerToken.StartPosition;
            StackCount = stackCount;
        }
    }

    internal enum ParserErrorType {
        [Description("Unexpected block start, this type of block should be created at root level")]
        UnexpectedBlockStart,
        [Description("Unexpected block end, the start of this block has not been found")]
        UnexpectedBlockEnd,
        [Description("Unexpected Appbuilder block start, two consecutive ANALYSE-SUSPEND found (no ANALYSE-RESUME)")]
        UnexpectedUibBlockStart,
        [Description("Unexpected Appbuilder block end, can not match ANALYSE-SUSPEND for this ANALYSE-RESUME")]
        UnexpectedUibBlockEnd,
        [Description("Unexpected Appbuilder block start, ANALYSE-SUSPEND should be created at root level")]
        NotAllowedUibBlockStart,
        [Description("Unexpected Appbuilder block end, ANALYSE-RESUME should be created at root level")]
        NotAllowedUibBlockEnd,
        [Description("&IF pre-processed statement missing an &ENDIF")]
        MismatchNumberOfIfEndIf,
        [Description("&ENDIF pre-processed statement matched without the corresponding &IF")]
        UnexpectedIfEndIfBlockEnd,
    }

    #endregion


}
