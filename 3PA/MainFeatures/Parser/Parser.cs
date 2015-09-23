using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using _3PA.Lib;

namespace _3PA.MainFeatures.Parser {

    /// <summary>
    /// This class is not actually a parser "per say" but it extract important information
    /// from the tokens created by the lexer
    /// </summary>
    public class Parser {
        private List<ParsedItem> _parsedItemList = new List<ParsedItem>();
        private Lexer _lexer;
        private ParseContext _context = new ParseContext();
        private Dictionary<int, LineInfo> _lineInfo = new Dictionary<int, LineInfo>();

        /// <summary>
        /// When we match a word that make us enter a block, we need to increase the blockDepth but only
        /// at the next statement, this bool allows to do just that
        /// </summary>
        private bool _increaseDepthAtNextStatement;
        /// <summary>
        /// when we match a else or a then, we need to increase the blockDepth but only for the one next 
        /// statement, this bool allows to do that
        /// </summary>
        private bool _lastStatementOneTimeIncrease;

        /// <summary>
        /// dictionnay of *line, line info*
        /// </summary>
        public Dictionary<int, LineInfo> GetLineInfo { get { return _lineInfo; } }

        /// <summary>
        /// Parses a text into a list of parsedItems
        /// </summary>
        /// <param name="data"></param>
        /// <param name="defaultLcOwnerName">The default scope to use (before we enter a func/proc/mainblock...)</param>
        public Parser(string data, string defaultLcOwnerName = "") {
            _context.LcOwnerName = defaultLcOwnerName;

            // parse
            _lexer = new Lexer(data);
            _lexer.Tokenize();
            while (MoveNext()) {
                Analyze();
            }

            // add missing values to the line dictionnary
            var current = new LineInfo(0, ParseScope.Global, "");
            for (int i = 0; i < _lexer.MaxLine; i++) {
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
                token is TokenComment || 
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
#warning for debug!
                            if (CreateParsedFunction(token)) {
                                if (_context.BlockDepth != 0) throw new Exception("We should be at _context.BlockDepth == 0! and we are at _context.BlockDepth = " + _context.BlockDepth);
                                _context.BlockDepth = 0;
                                _increaseDepthAtNextStatement = true;
                            }
                            break;
                        case "procedure":
                            // parse a procedure definition
                            if (CreateParsedProcedure(token)) {
                                if (_context.BlockDepth != 0) throw new Exception("We should be at _context.BlockDepth == 0! and we are at _context.BlockDepth = " + _context.BlockDepth);
                                _context.BlockDepth = 0;
                                _increaseDepthAtNextStatement = true;
                            }
                            break;
                        case "define":
                            // add a one time indent after a then or else
                            CreateParsedDefine(token);
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
                            _increaseDepthAtNextStatement = true;
                            break;
                        case "end":
                            _context.BlockDepth--;
                            if (_context.BlockDepth == 0) {
                                if (_context.Scope != ParseScope.Global)
                                    _context.LcOwnerName = "";
                                _context.Scope = ParseScope.Global;
                            }
                            break;
                        case "else":
                            // add a one time indent after a then or else
                            _context.OneTimeIndent = true;
                            break;
                    }
                    
                } else {
                    // matches a do in the middle of a statement (ex: ON CHOOSE OF xx DO:)
                    if (lowerTok.Equals("do") || 
                        (lowerTok.Equals("triggers") && PeekAt(1) is TokenEos)) {
                        _increaseDepthAtNextStatement = true;
                    }

                    // add a one time indent after a then or else
                    if (lowerTok.Equals("then"))
                        _context.OneTimeIndent = true;
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

            // end of statement
            else if (token is TokenEos) 
                NewStatement();
        }

        /// <summary>
        /// called when a Eos token is found, store information on the statement's line
        /// </summary>
        private void NewStatement() {
            // remember the blockDepth of the current token's line (add block depth if the statement started after else of then)
            if (!_lineInfo.ContainsKey(_context.StatementStartLine))
                _lineInfo.Add(_context.StatementStartLine, new LineInfo((_lastStatementOneTimeIncrease ? 1 : 0) + _context.BlockDepth, _context.Scope, _context.LcOwnerName));
            _context.StatementWordCount = 0;
            _context.StatementStartLine = -1;

            // basically, delay the value _context.OneTimeIndent = true of 1 call of NewStatement()
            if (_lastStatementOneTimeIncrease)
                _lastStatementOneTimeIncrease = false;
            if (_context.OneTimeIndent)
                _lastStatementOneTimeIncrease = true;
            _context.OneTimeIndent = false;
            
            // increase depth of next statement?
            if (_increaseDepthAtNextStatement)
                _context.BlockDepth++;
            _increaseDepthAtNextStatement = false;
        }

        /// <summary>
        /// Matches a function definition (not the FORWARD prototype)
        /// </summary>
        private void CreateParsedDefine(Token functionToken) {
            // info we will extract from the current statement :
            string name = "";
            string asLike = "";
            ParseDefineType type = ParseDefineType.None;
            string primitiveType = "";
            StringBuilder left = new StringBuilder();
            StringBuilder strFlags = new StringBuilder();

            bool isTempTable = false;
            var fields = new List<ParsedField>();
            ParsedField currentField = new ParsedField("", "", "", 0, ParseFieldFlag.None, "", "", "", 0);

            int state = 0;
            do {
                var token = PeekAt(1); // next token
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                string lowerToken;
                bool matchedLikeTable = false;
                bool isPrimary = false;
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
                        if (type == ParseDefineType.Variable || type == ParseDefineType.Parameter) state = 10;
                        if (isTempTable) state = 20;
                        if (state != 1) break;
                        state = 99;
                        break;


                    case 10:
                        // define variable : match as or like
                        if (!(token is TokenWord)) break;
                        lowerToken = token.Value.ToLower();
                        if (lowerToken.Equals("as") || lowerToken.Equals("like")) state = 11;
                        if (state != 10) asLike = lowerToken;
                        break;
                    case 11:
                        // define variable : match a primitive type or a field in db
                        if (!(token is TokenWord)) break;
                        primitiveType = token.Value;
                        state = 99;
                        break;


                    case 20:
                        // define temp-table
                        if (!(token is TokenWord)) break;
                        lowerToken = token.Value.ToLower();
                        // matches a LIKE table
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse, resharper doesn't get this one
                        if (lowerToken.Equals("like") && !matchedLikeTable) state = 21;
                        // matches FIELD
                        if (lowerToken.Equals("field")) state = 22;
                        // matches INDEX
                        if (lowerToken.Equals("index")) state = 25;
                        break;
                    case 21:
                        // define temp-table : match a LIKE table, get the table name in asLike
                        // ReSharper disable once RedundantAssignment
                        matchedLikeTable = true;
                        if (!(token is TokenWord)) break;
                        asLike = token.Value;
                        state = 20;
                        break;
                    case 22:
                        // define temp-table : matches a FIELD name
                        if (!(token is TokenWord)) break;
                        currentField = new ParsedField(token.Value, "", "", 0, ParseFieldFlag.None, "", "", "", 0);
                        state = 23;
                        break;
                    case 23:
                        // define temp-table : matches a FIELD AS or LIKE
                        if (!(token is TokenWord)) break;
                        currentField.AsLike = token.Value;
                        state = 24;
                        break;
                    case 24:
                        // define temp-table : match a primitive type or a field in db
                        if (!(token is TokenWord)) break;
                        currentField.Type = token.Value;
                        // push the field to the fields list
                        fields.Add(currentField);
                        state = 20;
                        break;
                    case 25:
                        // define temp-table : match an index definition
                        if (token is TokenWord) break;
                        lowerToken = token.Value.ToLower();
                        if (lowerToken.Equals("primary")) isPrimary = true;
                        var found = fields.Find(field => field.Name.EqualsCi(lowerToken));
                        if (found != null)
                            found.Flag = isPrimary ? ParseFieldFlag.Primary : ParseFieldFlag.None;
                        if (lowerToken.Equals("index"))
                            // ReSharper disable once RedundantAssignment
                            isPrimary = false;
                        break;


                    case 99:
                        // matching the rest of the define
                        left.Append((token is TokenEol || token is TokenWhiteSpace) ? " " : token.Value);
                        break;
                }
            } while (MoveNext());
            if (state <= 1) return;
            if (isTempTable)
                _parsedItemList.Add(new ParsedTable(name, functionToken.Line, functionToken.Column, "", "", name, "", _context.Scope, _context.LcOwnerName, asLike, 0, true, fields, new List<ParsedIndex>(), new List<ParsedTrigger>()));
            else
                _parsedItemList.Add(new ParsedDefine(name, functionToken.Line, functionToken.Column, strFlags.ToString(), asLike, left.ToString(), type, primitiveType, _context.Scope, _context.LcOwnerName));
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
                    _parsedItemList.Add(new ParsedPreProc(name, token.Line, token.Column, 0, ParsedPreProcFlag.Global));
                    break;
                case "&SCOPED-DEFINE":
                case "&SCOPED":
                    _parsedItemList.Add(new ParsedPreProc(name, token.Line, token.Column, 0, ParsedPreProcFlag.Scope));
                    break;
                case "&ANALYZE-SUSPEND":
                    _context.Scope = ParseScope.Global;
                    if (toParse.Contains("_DEFINITIONS", StringComparison.OrdinalIgnoreCase))
                        _context.LcOwnerName = "definitions";
                    else if (toParse.Contains("_UIB-PREPROCESSOR-BLOCK", StringComparison.OrdinalIgnoreCase))
                        _context.LcOwnerName = "preprocessor";
                    else if (toParse.Contains("_MAIN-BLOCK", StringComparison.OrdinalIgnoreCase))
                        _context.LcOwnerName = "mainblock";
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
            StringBuilder leftStr = new StringBuilder();

            int state = 0;
            do {
                var token = PeekAt(1); // next token
                if (token is TokenEos) {
                    if (!token.Value.Equals(":")) state = 0;
                    break;
                }
                if (token is TokenComment) continue;
                if (state == 0) {
                    // matching name
                    if (!(token is TokenWord)) continue;
                    name = token.Value;
                    state++;
                    continue;
                }
                leftStr.Append(token.Value);
            } while (MoveNext());
            if (state != 1) return false;
            _context.Scope = ParseScope.Procedure;
            _context.LcOwnerName = name.ToLower();
            _parsedItemList.Add(new ParsedProcedure(name, procToken.Line, procToken.Column, name.ToLower(), leftStr.ToString()));
            return true;
        }

        /// <summary>
        /// Matches a function definition (not the FORWARD prototype)
        /// </summary>
        private bool CreateParsedFunction(Token functionToken) {
            // info we will extract from the current statement :
            string name = "";
            string returnType = "";
            StringBuilder parameters = new StringBuilder();
            bool isPrivate = false;

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
                        returnType = token.Value;
                        state++;
                        break;
                    case 2:
                        // matching parameters (start)
                        if (token is TokenWord && token.Value.EqualsCi("private")) {
                            isPrivate = true;
                            break;
                        }
                        if (!(token is TokenSymbol)) break;
                        if (token.Value.Equals("(")) state++;
                        break;
                    case 3:
                        // matching parameters (content)
                        if (token is TokenSymbol && token.Value.Equals(")")) {
                            state++;
                            break;
                        }
                        parameters.Append(token.Value);
                        break;
                    case 4:
                        // matching prototype, we dont want to create a ParsedItem for prototype
                        if (token is TokenWord && token.Value.EqualsCi("forward")) state = 0;
                        break;
                }
            } while (MoveNext());
            if (state != 4) return false;
            _context.Scope = ParseScope.Function;
            _context.LcOwnerName = name.ToLower();
            _parsedItemList.Add(new ParsedFunction(name, functionToken.Line, functionToken.Column, name.ToLower(), returnType, parameters.ToString(), isPrivate));
            return true;
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

            _parsedItemList.Add(new ParsedIncludeFile(toParse, token.Line, token.Column, _context.Scope, _context.LcOwnerName));
        }

    }

    /// <summary>
    /// contains the info on the current context (as we move through tokens)
    /// </summary>
    public class ParseContext {
        public bool OneTimeIndent;
        public int StatementStartLine = -1;
        public List<Token> StatementTokenList = new List<Token>();
        public int StatementWordCount;
        public int BlockDepth;
        public ParseScope Scope = ParseScope.Global;
        public string LcOwnerName = "";
    }

    /// <summary>
    /// Contains the info of a specific line number (built during the parsing)
    /// </summary>
    public class LineInfo {
        public int BlockDepth;
        public ParseScope Scope;
        /// <summary>
        /// Name of the current procedure/part of main, definitions, preproc
        /// all in lower case
        /// </summary>
        public string CurrentScopeName;

        public LineInfo(int blockDepth, ParseScope scope, string currentScopeName) {
            BlockDepth = blockDepth;
            Scope = scope;
            CurrentScopeName = currentScopeName;
        }
    }
}
