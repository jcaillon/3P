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
        /// current lexer
        /// </summary>
        private Lexer _lexer;

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
        public Parser(string data, string filePathBeingParsed, ParsedScopeItem defaultScope, bool matchKnownWords = false) : this(NewLexerFromData(data), filePathBeingParsed, defaultScope, matchKnownWords) { }

        /// <summary>
        /// Parses a text into a list of parsedItems
        /// </summary>
        public Parser(Lexer lexer, string filePathBeingParsed, ParsedScopeItem defaultScope, bool matchKnownWords = false) {

            // process inputs
            _filePathBeingParsed = filePathBeingParsed;
            _matchKnownWords = matchKnownWords && _knownStaticItems != null;

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

            // parse
            _lexer = lexer;
            while (MoveNext()) {
                Analyze();
            }

            // add missing values to the line dictionnary
            var current = new LineInfo(GetCurrentDepth(), _rootScope);
            for (int i = _lexer.MaxLine - 1; i >= 0; i--) {
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
            _lexer = null;
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

        #region Explore lexer

        /// <summary>
        /// Peek forward x tokens, returns an TokenEof if out of limits
        /// </summary>
        private Token PeekAt(int x) {
            return _lexer.PeekAtToken(x);
        }

        /// <summary>
        /// Peek forward (or backward if goBackWard = true) until we match a token that is not a space token
        /// return found token
        /// </summary>
        private Token PeekAtNextNonSpace(int start, bool goBackward = false) {
            int x = start + (goBackward ? -1 : 1);
            var tok = _lexer.PeekAtToken(x);
            while (tok is TokenWhiteSpace)
                tok = _lexer.PeekAtToken(goBackward ? x-- : x++);
            return tok;
        }

        /// <summary>
        /// Go to the next token
        /// </summary>
        private bool MoveNext() {
            
            // before moving to the next token, we can choose to analyse the current token
            var currentToken = PeekAt(0);
            if (currentToken is TokenWord) {
                _context.StatementWordCount++;
            }

            return _lexer.MoveNextToken();
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
                token is TokenPreProcStatement ||
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
                            break;
                        case "run":
                            // Parse a run statement
                            CreateParsedRun(token);
                            break;
                        case "dynamic-function":
                            CreateParsedDynamicFunction(token);
                            break;
                        case "&if":
                            _context.PreProcIfStack.Push(CreateParsedIfEndIfPreProc(token));
                            break;
                        case "&endif":
                            if (_context.PreProcIfStack.Count > 0) {
                                var prevIf = _context.PreProcIfStack.Pop();
                                prevIf.EndBlockLine = token.Line;
                                prevIf.EndBlockPosition = token.EndPosition;
                            } else
                                _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedIfEndIfBlockEnd, token, 0));
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
                CreateParsedIncludeFile(token);
            }

            // pre processed
            else if (token is TokenPreProcStatement) {

                // first word of a statement
                if (_context.StatementWordCount == 0)
                    CreateParsedPreProc(token);
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
        /// <param name="strBuilder"></param>
        /// <param name="token"></param>
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
        /// <param name="token"></param>
        /// <returns></returns>
        private string GetTokenStrippedValue(Token token) {
            return (token is TokenString) ? token.Value.Substring(1, token.Value.Length - 2) : token.Value;
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
                        if (!(token is TokenWord) && !(token is TokenString)) break;
                        eventList.Append((eventList.Length == 0 ? "" : ", ") + GetTokenStrippedValue(token));
                        state++;
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
                        if (token is TokenWord || token is TokenInclude || token is TokenString) {
                            widgetList.Append((widgetList.Length == 0 ? "" : ", ") + GetTokenStrippedValue(token));

                            // we can match several widget name separated by a comma or resume to next state
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
            bool isPrimary = false;

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
                            state = 31;
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
                                    state = 30;
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
                        // define temp-table : match an index definition
                        if (!(token is TokenWord)) break;
                        lowerToken = token.Value.ToLower();
                        if (lowerToken.Equals("primary")) {
                            // ReSharper disable once RedundantAssignment
                            isPrimary = true;
                            break;
                        }
                        var found = fields.Find(field => field.Name.EqualsCi(lowerToken));
                        if (found != null)
                            found.Flag = isPrimary ? ParsedFieldFlag.Primary : ParsedFieldFlag.None;
                        if (lowerToken.Equals("index"))
                            // ReSharper disable once RedundantAssignment
                            isPrimary = false;
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

                    case 30:
                        // define parameter : match a temptable, table, dataset or buffer name
                        if (!(token is TokenWord)) break;
                        if (token.Value.ToLower().Equals("for")) break;
                        name = token.Value;
                        state++;
                        break;
                    case 31:
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
            if (isTempTable)
                AddParsedItem(new ParsedTable(name, functionToken, "", "", name, "", likeTable, true, fields, new List<ParsedIndex>(), new List<ParsedTrigger>(), strFlags.ToString(), useIndex.ToString()) {
                    // = end position of the EOS of the statement
                    EndPosition = token.EndPosition
                });
            else
                AddParsedItem(new ParsedDefine(name, functionToken, strFlags.ToString(), asLike, left.ToString(), type, tempPrimitiveType, viewAs, bufferFor, isExtended, isDynamic) {
                    // = end position of the EOS of the statement
                    EndPosition = token.EndPosition
                });
        }

        /// <summary>
        /// Analyze a preprocessed statement
        /// </summary>
        /// <param name="token"></param>
        private void CreateParsedPreProc(Token token) {

            var toParse = token.Value;
            int pos;
            for (pos = 1; pos < toParse.Length; pos++)
                if (Char.IsWhiteSpace(toParse[pos])) break;

            // extract first word
            var firstWord = toParse.Substring(0, pos);
            int pos2;
            for (pos2 = pos; pos2 < toParse.Length; pos2++)
                if (!Char.IsWhiteSpace(toParse[pos2])) break;
            for (pos = pos2; pos < toParse.Length; pos++)
                if (Char.IsWhiteSpace(toParse[pos])) break;

            // extract define name
            var name = toParse.Substring(pos2, pos - pos2);

            // match first word of the statement
            switch (firstWord.ToUpper()) {
                case "&GLOBAL-DEFINE":
                case "&GLOBAL":
                case "&GLOB":
                    AddParsedItem(new ParsedPreProc(name, token, 0, ParsedPreProcType.Global, toParse.Substring(pos, toParse.Length - pos)));
                    break;

                case "&SCOPED-DEFINE":
                case "&SCOPED":
                    AddParsedItem(new ParsedPreProc(name, token, 0, ParsedPreProcType.Scope, toParse.Substring(pos, toParse.Length - pos)));
                    break;

                case "&ANALYZE-SUSPEND":
                    // it marks the beggining of an appbuilder block, it can only be at a root/File level, otherwise flag error
                    if (!(_context.Scope is ParsedFile)) {
                        _parserErrors.Add(new ParserError(ParserErrorType.NotAllowedUibBlockStart, token, 0));
                        _context.Scope = _rootScope;
                    }

                    // we match a new block start but we didn't match the previous block end, flag error
                    if (_context.UibBlockStack.Count > 0) {
                        _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedUibBlockStart, token, _context.UibBlockStack.Count));
                        _context.UibBlockStack.Clear();
                    }

                    // matching different intersting blocks
                    ParsedPreProcBlockType type = ParsedPreProcBlockType.Unknown;
                    string blockName = "Appbuilder block";
                    if (toParse.ContainsFast("_FUNCTION-FORWARD")) {
                        type = ParsedPreProcBlockType.FunctionForward;
                        blockName = "Function prototype";
                    } else if (toParse.ContainsFast("_MAIN-BLOCK")) {
                        type = ParsedPreProcBlockType.MainBlock;
                        blockName = "Main block";
                    } else if (toParse.ContainsFast("_DEFINITIONS")) {
                        type = ParsedPreProcBlockType.Definitions;
                        blockName = "Definitions";
                    } else if (toParse.ContainsFast("_UIB-PREPROCESSOR-BLOCK")) {
                        type = ParsedPreProcBlockType.UibPreprocessorBlock;
                        blockName = "Pre-processor definitions";
                    } else if (toParse.ContainsFast("_XFTR")) {
                        type = ParsedPreProcBlockType.Xftr;
                        blockName = "Xtfr";
                    } else if (toParse.ContainsFast("_PROCEDURE-SETTINGS")) {
                        type = ParsedPreProcBlockType.ProcedureSettings;
                        blockName = "Procedure settings";
                    } else if (toParse.ContainsFast("_CREATE-WINDOW")) {
                        type = ParsedPreProcBlockType.CreateWindow;
                        blockName = "Window settings";
                    } else if (toParse.ContainsFast("_RUN-TIME-ATTRIBUTES")) {
                        type = ParsedPreProcBlockType.RunTimeAttributes;
                        blockName = "Runtime attributes";
                    }
                    _context.UibBlockStack.Push(new ParsedPreProcBlock(blockName, token) {
                        Type = type,
                        BlockDescription = toParse.Substring(pos2, toParse.Length - pos2),
                    });

                    // save the block description
                    AddParsedItem(_context.UibBlockStack.Peek());
                    break;

                case "&ANALYZE-RESUME":
                    // it marks the end of an appbuilder block, it can only be at a root/File level
                    if (!(_context.Scope is ParsedFile)) {
                        _parserErrors.Add(new ParserError(ParserErrorType.NotAllowedUibBlockEnd, token, 0));
                        _context.Scope = _rootScope;
                    }
                    
                    if (_context.UibBlockStack.Count == 0) {
                        // we match an end w/o beggining, flag a mismatch
                        _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedUibBlockEnd, token, 0));
                    } else {
                        // end position of the current appbuilder block
                        var currentBlock = _context.UibBlockStack.Pop();
                        currentBlock.EndBlockLine = token.Line;
                        currentBlock.EndBlockPosition = token.EndPosition;
                    }

                    break;

                case "&UNDEFINE":
                    var found = (ParsedPreProc) _parsedItemList.FindLast(item => (item is ParsedPreProc && item.Name.Equals(name)));
                    if (found != null)
                        found.UndefinedLine = _context.StatementFirstToken.Line;
                    break;
            }
        }

        /// <summary>
        /// Matches a & IF.. & THEN pre-processed statement
        /// </summary>
        private ParsedPreProcBlock CreateParsedIfEndIfPreProc(Token ifToken) {
            _lastTokenWasSpace = true;
            StringBuilder expression = new StringBuilder();

            do {
                var token = PeekAt(1);
                if (token is TokenEos) break;
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
        /// <param name="procToken"></param>
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
                        if (!(token is TokenWord)) continue;
                        name = token.Value;
                        state++;
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
        /// matches a include file
        /// </summary>
        /// <param name="token"></param>
        private void CreateParsedIncludeFile(Token token) {
            var toParse = token.Value;

            // skip whitespaces
            int startPos = 1;
            while (startPos < toParse.Length) {
                if (!Char.IsWhiteSpace(toParse[startPos])) break;
                startPos++;
            }
            if (toParse[startPos] == '&') return;

            // read first word as the filename
            int curPos = startPos;
            while (curPos < toParse.Length) {
                if (Char.IsWhiteSpace(toParse[curPos]) || toParse[curPos] == '}') break;
                curPos++;
            }
            toParse = toParse.Substring(startPos, curPos - startPos);

            if (!toParse.ContainsFast("."))
                return;

            // we matched the include file name
            AddParsedItem(new ParsedIncludeFile(toParse, token));
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
