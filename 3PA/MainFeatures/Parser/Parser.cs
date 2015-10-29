#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (Parser.cs) is part of 3P.
// 
// // 3P is a free software: you can redistribute it and/or modify
// // it under the terms of the GNU General Public License as published by
// // the Free Software Foundation, either version 3 of the License, or
// // (at your option) any later version.
// 
// // 3P is distributed in the hope that it will be useful,
// // but WITHOUT ANY WARRANTY; without even the implied warranty of
// // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// // GNU General Public License for more details.
// 
// // You should have received a copy of the GNU General Public License
// // along with 3P. If not, see <http://www.gnu.org/licenses/>.
// // ========================================================================
#endregion
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using _3PA.Lib;
using _3PA.MainFeatures.CodeExplorer;

namespace _3PA.MainFeatures.Parser {

    /// <summary>
    /// This class is not actually a parser "per say" but it extract important information
    /// from the tokens created by the lexer
    /// </summary>
    public class Parser {
        private const string RootScopeName = "Root";
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
        private ParseContext _context = new ParseContext() {
            Scope = ParsedScope.File,
            OwnerName = "",
            FirstWordToken = null,
            BlockStack = new Stack<BlockInfo>(),
            StatementStartLine = -1
        };

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
        /// Is is possible to match (almost) every word found by the lexer against database's table names
        /// to return a list of used table in the program
        /// </summary>
        private Dictionary<string, bool> _databaseTableDictionary;

        private bool _matchDatabaseTables = true;

        /// <summary>
        /// Useful to remember where the function prototype was defined (Point is line, column)
        /// </summary>
        private Dictionary<string, Point> _functionPrototype = new Dictionary<string, Point>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// dictionnay of *line, line info*
        /// </summary>
        public Dictionary<int, LineInfo> GetLineInfo { get { return _lineInfo; } }

        /// <summary>
        /// If true the parsing went ok, if false, it means that we matched too much starting block compared to 
        /// ending block statements (or the opposite), in short, was the parsing OK or not?
        /// Allows to decide if we can reindent the code or not
        /// </summary>
        public bool ParsingOk { get; set; }

        /// <summary>
        /// Parses a text into a list of parsedItems
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filePathBeingParsed"></param>
        /// <param name="defaultOwnerName">The default scope to use (before we enter a func/proc/mainblock...)</param>
        /// <param name="tablesDictionary"></param>
        public Parser(string data, string filePathBeingParsed, string defaultOwnerName, Dictionary<string, bool> tablesDictionary = null) {
            // process inputs
            bool isRootFile = string.IsNullOrEmpty(defaultOwnerName);
            defaultOwnerName = isRootFile ? RootScopeName : defaultOwnerName;
            _context.OwnerName = defaultOwnerName;
            _filePathBeingParsed = filePathBeingParsed;

            if (tablesDictionary == null)
                _matchDatabaseTables = false;
            else
                _databaseTableDictionary = tablesDictionary;

            ParsingOk = true;

            // create root item
            if (isRootFile)
                AddParsedItem(new ParsedBlock(defaultOwnerName, 0, 0, CodeExplorerBranch.Root) { IsRoot = true });

            // parse
            _lexer = new Lexer(data);
            _lexer.Tokenize();
            while (MoveNext()) {
                Analyze();
            }

            // add missing values to the line dictionnary
            var current = new LineInfo(GetCurrentDepth(), ParsedScope.File, defaultOwnerName);
            for (int i = _lexer.MaxLine - 1; i >= 0; i--) {
                if (_lineInfo.ContainsKey(i))
                    current = _lineInfo[i];
                else
                    _lineInfo.Add(i, current);
            }
        }

        /// <summary>
        /// Feed this method with a visitor implementing IParserVisitor to visit all the parsed items
        /// </summary>
        /// <param name="visitor"></param>
        public void Accept(IParserVisitor visitor) {
            foreach (var item in _parsedItemList) {
                item.Accept(visitor);
            }
        }

        /// <summary>
        /// Peek forward x tokens
        /// </summary>
        private Token PeekAt(int x) {
            return _lexer.PeekAtToken(x);
        }

        /// <summary>
        /// Peek forward (or backward if goBackWard = true) until we match a token that is not a space token
        /// return found token
        /// </summary>
        private Token PeekAtNextNonSpace(bool goBackward = false) {
            int x = goBackward ? -1 : 1;
            var tok = _lexer.PeekAtToken(x);
            while (tok is TokenWhiteSpace)
                tok = _lexer.PeekAtToken(goBackward ? x-- : x++);
            return tok;
        }

        /// <summary>
        /// Read to the next token
        /// </summary>
        private bool MoveNext() {
            return _lexer.MoveNextToken();
        }

        private void Analyze() {
            var token = PeekAt(0);

            // reached end of file
            if (token is TokenEof)
                return;

            // starting a new statement, we need to remember its starting line
            if (_context.StatementStartLine == -1 && (
                token is TokenWord || 
                token is TokenPreProcStatement ||
                token is TokenInclude))
                    _context.StatementStartLine = token.Line;

            // matching a word
            if (token is TokenWord) {
                _context.StatementWordCount++;
                var lowerTok = token.Value.ToLower();

                // first word of a statement
                if (_context.StatementWordCount == 1) {

                    // matches a definition statement at the beggining of a statement
                    switch (lowerTok) {
                        case "function":
                            // parse a function definition
                            if (CreateParsedFunction(token)) {
                                if (_context.BlockStack.Count != 0) ParsingOk = false;
                                _context.BlockStack.Clear();
                                PushBlockInfoToStack(BlockType.DoEnd, token.Line);
                            }
                            break;
                        case "procedure":
                            // parse a procedure definition
                            if (CreateParsedProcedure(token)) {
                                if (_context.BlockStack.Count != 0) ParsingOk = false;
                                _context.BlockStack.Clear();
                                PushBlockInfoToStack(BlockType.DoEnd, token.Line);
                            }
                            break;
                        case "on":
                            // parse a ON statement
                            if (CreateParsedOnEvent(token)) {
                                if (_context.BlockStack.Count != 0) ParsingOk = false;
                                _context.BlockStack.Clear();
                                PushBlockInfoToStack(BlockType.DoEnd, token.Line);
                            }
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
                            // increase block depth
                            PushBlockInfoToStack(BlockType.DoEnd, token.Line);
                            break;
                        case "end":
                            if (_context.BlockStack.Count > 0) {
                                // decrease block depth
                                var popped = _context.BlockStack.Pop();

                                // in case of a then do: we have created 2 stacks for actually the same block, pop them both
                                if (_context.BlockStack.Count > 0 &&
                                    _context.BlockStack.Peek().BlockType == BlockType.ThenElse &&
                                    popped.LineTriggerWord == _context.BlockStack.Peek().LineTriggerWord)
                                    _context.BlockStack.Pop();
                            } else
                                ParsingOk = false;

                            if (_context.BlockStack.Count == 0) {
                                // end of a proc, func or on event block
                                if (_context.Scope != ParsedScope.File) {
                                    var parsedScope = (ParsedScopeItem)_parsedItemList.FindLast(item => item is ParsedScopeItem);
                                    if (parsedScope != null) parsedScope.EndLine = token.Line;
                                    _context.OwnerName = RootScopeName;
                                }
                                _context.Scope = ParsedScope.File;
                            }
                            break;
                        case "else":
                            // add a one time indent after a then or else
                            PushBlockInfoToStack(BlockType.ThenElse, token.Line);
                            break;
                        case "run":
                            // Parse a run statement
                            CreateParsedRun(token);
                            break;
                        case "dynamic-function":
                            CreateParsedDynamicFunction(token);
                            break;
                        default:
                            // save first word of the statement (useful for labels)
                            _context.FirstWordToken = token;
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
                            PushBlockInfoToStack(BlockType.DoEnd, token.Line);
                            break;
                        case "triggers":
                            if (PeekAtNextNonSpace() is TokenEos)
                                PushBlockInfoToStack(BlockType.DoEnd, token.Line);
                            break;
                        case "then":
                            // add a one time indent after a then or else
                            PushBlockInfoToStack(BlockType.ThenElse, token.Line);
                            break;
                        default:
                            // try to match with a table's name
                            if (_matchDatabaseTables) {
                                if (_databaseTableDictionary.ContainsKey(lowerTok))
                                    AddParsedItem(new ParsedFoundTableUse(token.Value.AutoCaseToUserLiking(), token.Line, token.Column));
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
                _context.StatementWordCount++;

                // first word of a statement
                if (_context.StatementWordCount == 1)
                    CreateParsedPreProc(token);
            }

            // potential function call
            else if (token is TokenSymbol && token.Value.Equals("(")) {
                var prevToken = PeekAtNextNonSpace(true);
                if (prevToken is TokenWord && _functionPrototype.ContainsKey(prevToken.Value))
                    AddParsedItem(new ParsedFunctionCall(prevToken.Value, prevToken.Line, prevToken.Column, false));
            }

            // end of statement
            else if (token is TokenEos) {
                // match a label if there was only one word followed by : in the statement
                if (_context.StatementWordCount == 1 && _context.FirstWordToken != null && token.Value.Equals(":"))
                    CreateParsedLabel();
                NewStatement(token);
            }
        }

        /// <summary>
        /// called when a Eos token is found, store information on the statement's line
        /// </summary>
        private void NewStatement(Token token) {

            // remember the blockDepth of the current token's line (add block depth if the statement started after else of then)
            var depth = GetCurrentDepth();
            if (!_lineInfo.ContainsKey(_context.StatementStartLine))
                _lineInfo.Add(_context.StatementStartLine, new LineInfo(depth, _context.Scope, _context.OwnerName));

            // add missing values to the line dictionnary
            if (_context.StatementStartLine > -1 && token.Line > _context.StatementStartLine) {
                for (int i = _context.StatementStartLine + 1; i <= token.Line; i++)
                    if (!_lineInfo.ContainsKey(i))
                        _lineInfo.Add(i, new LineInfo(depth + 1, _context.Scope, _context.OwnerName));
            }

            // Pop all the then/else blocks that are on top
            if (_context.BlockStack.Count > 0 && _context.BlockStack.Peek().StatementNumber != _context.StatementCount)
                while (_context.BlockStack.Peek().BlockType == BlockType.ThenElse) {
                    _context.BlockStack.Pop();
                    if (_context.BlockStack.Count == 0)
                        break;
                }

            _context.StatementCount++;
            _context.StatementWordCount = 0;
            _context.StatementStartLine = -1;
            _context.FirstWordToken = null;
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
            if (depth > 0 && _context.BlockStack.Peek().LineStart == _context.StatementStartLine && !lastStackThenDo)
                depth--;
            return depth;
        }

        /// <summary>
        /// Add a block info on top of the block Stack
        /// </summary>
        /// <param name="blockType"></param>
        /// <param name="currentLine"></param>
        private void PushBlockInfoToStack(BlockType blockType, int currentLine) {
            _context.BlockStack.Push(new BlockInfo(_context.StatementStartLine, currentLine, blockType, _context.StatementCount));
        }

        /// <summary>
        /// Call this method instead of adding the items directly in the list,
        /// updates the scope and file name
        /// </summary>
        private void AddParsedItem(ParsedItem item) {
            item.FilePath = _filePathBeingParsed;
            item.Scope = _context.Scope;
            item.OwnerName = _context.OwnerName;
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
                        if (token is TokenQuotedString) {
                            name = token.Value.Substring(1, token.Value.Length - 2);
                            state++;
                        }
                        break;
                }
            } while (MoveNext());

            if (state == 0) return;

            AddParsedItem(new ParsedFunctionCall(name, tokenFun.Line, tokenFun.Column, !_functionPrototype.ContainsKey(name)));
        }

        /// <summary>
        /// Creates a label parsed item
        /// </summary>
        private void CreateParsedLabel() {
            AddParsedItem(new ParsedLabel(_context.FirstWordToken.Value, _context.FirstWordToken.Line, _context.FirstWordToken.Column));
        }

        /// <summary>
        /// Creates a parsed item for RUN statements
        /// </summary>
        /// <param name="runToken"></param>
        private void CreateParsedRun(Token runToken) {
            // info we will extract from the current statement :
            string name = "";
            bool isValue = false;
            _lastTokenWasSpace = true;
            StringBuilder leftStr = new StringBuilder();
            int state = 0;
            do {
                var token = PeekAt(1); // next token
                if (state == 1) break; // stop after finding the RUN name to be able to match other words in the statement
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                switch (state) {
                    case 0:
                        // matching proc name (or VALUE)
                        if (token is TokenWord || token is TokenQuotedString) {
                            name = (token is TokenWord) ? token.Value : token.Value.Substring(1, token.Value.Length - 2);
                            if (!name.ToLower().Equals("value"))
                                state++;
                            else
                                isValue = true;
                        } else if (token is TokenSymbol && token.Value.Equals(")"))
                            state++;
                        break;
                    //case 1:
                    //    // matching the rest of run
                    //    AddTokenToStringBuilder(leftStr, token);
                    //    break;
                }
            } while (MoveNext());

            if (state == 0) return;
            AddParsedItem(new ParsedRun(name, runToken.Line, runToken.Column, leftStr.ToString(), isValue));
        }

        /// <summary>
        /// Creates parsed item for ON CHOOSE OF XXX events
        /// (choose or anything else)
        /// </summary>
        /// <param name="onToken"></param>
        /// <returns></returns>
        private bool CreateParsedOnEvent(Token onToken) {
            // info we will extract from the current statement :
            string name = "";
            string onType = "";

            int state = 0;
            do {
                var token = PeekAt(1); // next token
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                switch (state) {
                    case 0:
                        // matching event type
                        if (!(token is TokenWord)) break;
                        onType = token.Value;
                        state++;
                        break;
                    case 1:
                        // matching "of"
                        if (!(token is TokenWord)) break;
                        if (token.Value.EqualsCi("anywhere")) {
                            name = "anywhere";
                            AddParsedItem(new ParsedOnEvent(name, onToken.Line, onToken.Column, onType));
                            _context.Scope = ParsedScope.Trigger;
                            _context.OwnerName = string.Join(" ", onType.ToUpper(), name);
                            return true;
                        }
                        if (!token.Value.EqualsCi("of")) return false;
                        state++;
                        break;
                    case 2:
                        // matching widget name
                        if (!(token is TokenWord)) break;
                        name = token.Value;
                        state++;
                        break;
                    case 3:
                        // matching "or"
                        if (!(token is TokenWord)) break;
                        AddParsedItem(new ParsedOnEvent(name, onToken.Line, onToken.Column, onType));
                        _context.Scope = ParsedScope.Trigger;
                        _context.OwnerName = string.Join(" ", onType.ToUpper(), name);
                        if (token.Value.EqualsCi("or"))
                            state = 0;
                        else
                            return true;
                        break;
                }
            } while (MoveNext());
            return false;
        }

        /// <summary>
        /// Matches a function definition (not the FORWARD prototype)
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

            int state = 0;
            do {
                var token = PeekAt(1); // next token
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
                                foreach (var typ in Enum.GetNames(typeof(ParseDefineType)).Where(typ => token1.Equals(typ.ToLower()))) {
                                    type = (ParseDefineType)Enum.Parse(typeof(ParseDefineType), typ, true);
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
                AddParsedItem(new ParsedTable(name, functionToken.Line, functionToken.Column, "", "", name, "", likeTable, true, fields, new List<ParsedIndex>(), new List<ParsedTrigger>(), strFlags.ToString(), useIndex.ToString()));
            else
                AddParsedItem(new ParsedDefine(name, functionToken.Line, functionToken.Column, strFlags.ToString(), asLike, left.ToString(), type, tempPrimitiveType, viewAs, bufferFor, isExtended, isDynamic));
        }

        /// <summary>
        /// Analyze a preprocessed statement
        /// </summary>
        /// <param name="token"></param>
        private void CreateParsedPreProc(Token token) {
            var toParse = token.Value;
            int pos;
            for (pos = 1; pos < toParse.Length; pos++)
                if (char.IsWhiteSpace(toParse[pos])) break;

            // extract first word
            var firstWord = toParse.Substring(0, pos);
            int pos2;
            for (pos2 = pos; pos2 < toParse.Length; pos2++)
                if (!char.IsWhiteSpace(toParse[pos2])) break;
            for (pos = pos2; pos < toParse.Length; pos++)
                if (char.IsWhiteSpace(toParse[pos])) break;

            // extract define name
            var name = toParse.Substring(pos2, pos - pos2);

            // match first word of the statement
            switch (firstWord.ToUpper()) {
                case "&GLOBAL-DEFINE":
                case "&GLOBAL":
                case "&GLOB":
                    AddParsedItem(new ParsedPreProc(name, token.Line, token.Column, 0, ParsedPreProcFlag.Global));
                    break;
                case "&SCOPED-DEFINE":
                case "&SCOPED":
                    AddParsedItem(new ParsedPreProc(name, token.Line, token.Column, 0, ParsedPreProcFlag.Scope));
                    break;
                case "&ANALYZE-SUSPEND":
                    _context.Scope = ParsedScope.File;
                    // matching different intersting blocks
                    if (toParse.ContainsFast("_MAIN-BLOCK")) {
                        _context.OwnerName = "Main block";
                        AddParsedItem(new ParsedBlock(_context.OwnerName, token.Line, token.Column, CodeExplorerBranch.MainBlock) { IsRoot = true });
                    } 
                    else if (toParse.ContainsFast("_DEFINITIONS")) {
                        _context.OwnerName = "Definition block";
                        AddParsedItem(new ParsedBlock(_context.OwnerName, token.Line, token.Column, CodeExplorerBranch.Block) { IconIconType = CodeExplorerIconType.DefinitionBlock });
                    } 
                    else if (toParse.ContainsFast("_UIB-PREPROCESSOR-BLOCK")) {
                        _context.OwnerName = "Preprocessor block";
                        AddParsedItem(new ParsedBlock(_context.OwnerName, token.Line, token.Column, CodeExplorerBranch.Block) { IconIconType = CodeExplorerIconType.PreprocessorBlock });
                    } 
                    else if (toParse.ContainsFast("_XFTR")) {
                        _context.OwnerName = "Xtfr";
                        AddParsedItem(new ParsedBlock(_context.OwnerName, token.Line, token.Column, CodeExplorerBranch.Block) { IconIconType = CodeExplorerIconType.XtfrBlock });
                    } 
                    else if (toParse.ContainsFast("_PROCEDURE-SETTINGS")) {
                        _context.OwnerName = "Procedure settings";
                        AddParsedItem(new ParsedBlock(_context.OwnerName, token.Line, token.Column, CodeExplorerBranch.Block) { IconIconType = CodeExplorerIconType.SettingsBlock });
                    } 
                    else if (toParse.ContainsFast("_CREATE-WINDOW")) {
                        _context.OwnerName = "Create window";
                        AddParsedItem(new ParsedBlock(_context.OwnerName, token.Line, token.Column, CodeExplorerBranch.Block) { IconIconType = CodeExplorerIconType.CreateWindowBlock });
                    } 
                    else if (toParse.ContainsFast("_RUN-TIME-ATTRIBUTES")) {
                        _context.OwnerName = "Run-time attributes";
                        AddParsedItem(new ParsedBlock(_context.OwnerName, token.Line, token.Column, CodeExplorerBranch.Block) { IconIconType = CodeExplorerIconType.RuntimeBlock });
                    } 
                    else if (_functionPrototype.Count == 0 && toParse.ContainsFast("_FUNCTION-FORWARD")) {
                        _context.OwnerName = "Function prototypes";
                        AddParsedItem(new ParsedBlock(_context.OwnerName, token.Line, token.Column, CodeExplorerBranch.Block) { IconIconType = CodeExplorerIconType.Prototype });
                    }
                    break;
                case "&UNDEFINE":
                    var found = (ParsedPreProc)_parsedItemList.FindLast(item => (item is ParsedPreProc && item.Name.Equals(name)));
                    if (found != null)
                        found.UndefinedLine = _context.StatementStartLine;
                    break;
            }
        }

        /// <summary>
        /// Matches a procedure definition
        /// </summary>
        /// <param name="procToken"></param>
        private bool CreateParsedProcedure(Token procToken) {
            // info we will extract from the current statement :
            string name = "";
            bool isExternal = false;
            _lastTokenWasSpace = true;
            StringBuilder leftStr = new StringBuilder();

            int state = 0;
            do {
                var token = PeekAt(1); // next token
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
                        state++;
                        break;
                }
                AddTokenToStringBuilder(leftStr, token);
            } while (MoveNext());
            if (state < 1) return false;
            AddParsedItem(new ParsedProcedure(name, procToken.Line, procToken.Column, leftStr.ToString(), isExternal));
            _context.Scope = ParsedScope.Procedure;
            _context.OwnerName = name;
            return true;
        }

        /// <summary>
        /// Matches a function definition (not the FORWARD prototype)
        /// </summary>
        private bool CreateParsedFunction(Token functionToken) {
            // info we will extract from the current statement :
            string name = "";
            _lastTokenWasSpace = true;
            StringBuilder parameters = new StringBuilder();
            bool isPrivate = false;
            ParsedFunction createdFunc = null;
            var parametersList = new List<ParsedItem>();

            int state = 0;
            do {
                var token = PeekAt(1); // next token
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
                        if (token.Value.EqualsCi("returns")) continue;

                        // create the function (returnType = token.Value)
                        createdFunc = new ParsedFunction(name, functionToken.Line, functionToken.Column, token.Value) {
                            FilePath = _filePathBeingParsed,
                            Scope = _context.Scope,
                            OwnerName = _context.OwnerName
                        };
                        state++;
                        break;
                    case 2:
                        // matching parameters (start)
                        if (token is TokenWord) {
                            if (token.Value.EqualsCi("private")) isPrivate = true;
                            if (token.Value.EqualsCi("extent") && createdFunc != null) createdFunc.IsExtended = true;
                        }
                        else if (!(token is TokenSymbol)) break;
                        if (token.Value.Equals("(")) state = 3;
                        break;
                    case 3:
                        // read parameters, define a ParsedDefineItem for each
                        parametersList = GetParsedParameters(functionToken, parameters, name);
                        state = 99;
                        break;
                    case 99:
                        // matching prototype, we dont want to create a ParsedItem for prototype
                        if (token is TokenWord && token.Value.EqualsCi("forward")) {
                            if (!_functionPrototype.ContainsKey(name))
                                _functionPrototype.Add(name, new Point(functionToken.Line, functionToken.Column));
                            createdFunc = null;
                        }
                        break;
                }
            } while (MoveNext());
            if (createdFunc == null) return false;

            // modify context
            _context.Scope = ParsedScope.Function;
            _context.OwnerName = name;

            // complete the info on the function and add it to the parsed list
            createdFunc.IsPrivate = isPrivate;
            createdFunc.Parameters = parameters.ToString();
            if (_functionPrototype.ContainsKey(name)) {
                createdFunc.PrototypeLine = _functionPrototype[name].X;
                createdFunc.PrototypeColumn = _functionPrototype[name].Y;
            } else {
                createdFunc.PrototypeLine = -1;
                _functionPrototype.Add(name, new Point());
            }
            _parsedItemList.Add(createdFunc);

            // add the parameters to the list
            if (parametersList.Count > 0)
                _parsedItemList.AddRange(parametersList);
            return true;
        }

        private List<ParsedItem> GetParsedParameters(Token functionToken, StringBuilder parameters, string ownerName) {
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
                            parametersList.Add(new ParsedDefine(paramName, functionToken.Line, functionToken.Column, strFlags, paramAsLike, "", ParseDefineType.Parameter, paramPrimitiveType, "", parameterFor, isExtended, false) {
                                FilePath = _filePathBeingParsed,
                                Scope = ParsedScope.Function,
                                OwnerName = ownerName
                            });
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
            if (toParse.Length < 2 || toParse[1] == '&') return;
            int pos;
            for (pos = 1; pos < toParse.Length; pos++) {
                if (char.IsWhiteSpace(toParse[pos]) || toParse[pos] == '}') break;
            }
            toParse = toParse.Substring(1, pos - 1);
            // we matched the include file name

            AddParsedItem(new ParsedIncludeFile(toParse, token.Line, token.Column));
        }

    }

    /// <summary>
    /// contains the info on the current context (as we move through tokens)
    /// </summary>
    public class ParseContext {
        public ParsedScope Scope { get; set; }
        public string OwnerName { get; set; }
        /// <summary>
        /// Number of words count in the current statement
        /// </summary>
        public int StatementWordCount { get; set; }
        /// <summary>
        /// the line at which the current statement starts
        /// </summary>
        public int StatementStartLine { get; set; }
        public int StatementCount { get; set; }
        public Token FirstWordToken { get; set; }
        public Stack<BlockInfo> BlockStack { get; set; }
    }

    public struct BlockInfo {
        public int LineStart { get; set; }
        public int LineTriggerWord { get; set; }
        public BlockType BlockType { get; set; }
        public int StatementNumber { get; set; }
        public BlockInfo(int lineStart, int lineTriggerWord, BlockType blockType, int statementNumber) : this() {
            LineStart = lineStart;
            LineTriggerWord = lineTriggerWord;
            BlockType = blockType;
            StatementNumber = statementNumber;
        }
    }

    public enum BlockType {
        DoEnd,
        ThenElse,
    }

    /// <summary>
    /// Contains the info of a specific line number (built during the parsing)
    /// </summary>
    public class LineInfo {
        /// <summary>
        /// Block depth for the current line (= number of indents)
        /// </summary>
        public int BlockDepth { get; set; }
        /// <summary>
        /// Scope for the current line, see ParsedScope Enum
        /// </summary>
        public ParsedScope Scope { get; set; }
        /// <summary>
        /// Name of the current procedure/part of main, definitions, preproc
        /// all in lower case
        /// </summary>
        public string CurrentScopeName { get; set; }

        public LineInfo(int blockDepth, ParsedScope scope, string currentScopeName) {
            BlockDepth = blockDepth;
            Scope = scope;
            CurrentScopeName = currentScopeName;
        }
    }
}
