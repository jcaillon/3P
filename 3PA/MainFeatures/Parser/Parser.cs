using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using _3PA.Lib;
using _3PA.MainFeatures.DockableExplorer;

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
        private ParseContext _context = new ParseContext();

        /// <summary>
        /// Contains the information of each line parsed
        /// </summary>
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
        /// Path to the file being parsed (is added to the parseItem info)
        /// </summary>
        private string _filePathBeingParsed;

        /// <summary>
        /// dictionnay of *line, line info*
        /// </summary>
        public Dictionary<int, LineInfo> GetLineInfo { get { return _lineInfo; } }

        private bool _lastTokenWasSpace;

        /// <summary>
        /// Useful to remember where the function prototype was defined (Point is line, column)
        /// </summary>
        private Dictionary<string, Point> _functionPrototype = new Dictionary<string, Point>();

        private bool _foundPrototypeBlock;

        /// <summary>
        /// Parses a text into a list of parsedItems
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filePathBeingParsed"></param>
        /// <param name="defaultOwnerName">The default scope to use (before we enter a func/proc/mainblock...)</param>
        public Parser(string data, string filePathBeingParsed, string defaultOwnerName = null) {
            defaultOwnerName = string.IsNullOrEmpty(defaultOwnerName) ? RootScopeName : defaultOwnerName;
            _context.OwnerName = defaultOwnerName;
            _filePathBeingParsed = filePathBeingParsed;

            // create root item
            AddParsedItem(new ParsedBlock(defaultOwnerName, 0, 0, ExplorerType.Root) { IsRoot = true });

            // parse
            _lexer = new Lexer(data);
            _lexer.Tokenize();
            while (MoveNext()) {
                Analyze();
            }

            // add missing values to the line dictionnary
            var current = new LineInfo(0, ParsedScope.File, defaultOwnerName);
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
                            if (CreateParsedFunction(token)) {
                                //if (_context.BlockDepth != 0) throw new Exception("We should be at _context.BlockDepth == 0! and we are at _context.BlockDepth = " + _context.BlockDepth);
                                _context.BlockDepth = 0;
                                _increaseDepthAtNextStatement = true;
                            }
                            break;
                        case "procedure":
                            // parse a procedure definition
                            if (CreateParsedProcedure(token)) {
                                _context.BlockDepth = 0;
                                _increaseDepthAtNextStatement = true;
                            }
                            break;
                        case "on":
                            // parse a ON statement
                            if (CreateParsedOnEvent(token)) {
                                _context.BlockDepth = 0;
                                _increaseDepthAtNextStatement = true;
                            }
                            break;
                        case "run":
                            // Parse a run statement
                            CreateParsedRun(token);
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
                            _increaseDepthAtNextStatement = true;
                            break;
                        case "end":
                            _context.BlockDepth--;
                            if (_context.BlockDepth == 0) {
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
                _lineInfo.Add(_context.StatementStartLine, new LineInfo((_lastStatementOneTimeIncrease ? 1 : 0) + _context.BlockDepth, _context.Scope, _context.OwnerName));
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
        /// Call this method instead adding the items directly in the list,
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
        /// Creates a parsed item for RUN statements
        /// </summary>
        /// <param name="runToken"></param>
        private void CreateParsedRun(Token runToken) {
            // info we will extract from the current statement :
            string name = "";
            _lastTokenWasSpace = true;
            StringBuilder leftStr = new StringBuilder();
            int state = 0;
            do {
                var token = PeekAt(1); // next token
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                switch (state) {
                    case 0:
                        // matching proc name (or VALUE)
                        if (token is TokenWord && string.IsNullOrEmpty(name)) {
                            name = token.Value;
                            if (!name.ToLower().Equals("value"))
                                state++;
                        } else if (token is TokenSymbol && token.Value.Equals(")"))
                            state++;
                        break;
                    case 1:
                        // matching the rest of run
                        AddTokenToStringBuilder(leftStr, token);
                        break;
                }
            } while (MoveNext());

            if (state == 0) return;
            AddParsedItem(new ParsedRun(name, runToken.Line, runToken.Column, leftStr.ToString()));
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
                        _context.OwnerName = "Main Block";
                        AddParsedItem(new ParsedBlock(_context.OwnerName, token.Line, token.Column, ExplorerType.MainBlock) { IsRoot = true });
                    } 
                    else if (toParse.ContainsFast("_DEFINITIONS")) {
                        _context.OwnerName = "Definition Block";
                        AddParsedItem(new ParsedBlock(_context.OwnerName, token.Line, token.Column, ExplorerType.Block));
                    } 
                    else if (toParse.ContainsFast("_UIB-PREPROCESSOR-BLOCK")) {
                        _context.OwnerName = "Preprocessor Block";
                        AddParsedItem(new ParsedBlock(_context.OwnerName, token.Line, token.Column, ExplorerType.Block));
                    } 
                    else if (toParse.ContainsFast("_XFTR")) {
                        _context.OwnerName = "Xtfr";
                        AddParsedItem(new ParsedBlock(_context.OwnerName, token.Line, token.Column, ExplorerType.Block));
                    } 
                    else if (toParse.ContainsFast("_PROCEDURE-SETTINGS")) {
                        _context.OwnerName = "Procedure settings";
                        AddParsedItem(new ParsedBlock(_context.OwnerName, token.Line, token.Column, ExplorerType.Block));
                    } 
                    else if (toParse.ContainsFast("_CREATE-WINDOW")) {
                        _context.OwnerName = "Create window";
                        AddParsedItem(new ParsedBlock(_context.OwnerName, token.Line, token.Column, ExplorerType.Block));
                    } 
                    else if (toParse.ContainsFast("_RUN-TIME-ATTRIBUTES")) {
                        _context.OwnerName = "Run-time attributes";
                        AddParsedItem(new ParsedBlock(_context.OwnerName, token.Line, token.Column, ExplorerType.Block));
                    } 
                    else if (!_foundPrototypeBlock && toParse.ContainsFast("_FUNCTION-FORWARD")) {
                        _foundPrototypeBlock = true;
                        _context.OwnerName = "Function prototypes";
                        AddParsedItem(new ParsedBlock(_context.OwnerName, token.Line, token.Column, ExplorerType.Block));
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
            _lastTokenWasSpace = true;
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
                AddTokenToStringBuilder(leftStr, token);
            } while (MoveNext());
            if (state != 1) return false;
            AddParsedItem(new ParsedProcedure(name, procToken.Line, procToken.Column, leftStr.ToString()));
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
        public bool OneTimeIndent;
        public int StatementStartLine = -1;
        public List<Token> StatementTokenList = new List<Token>();
        public int StatementWordCount;
        public int BlockDepth;
        public ParsedScope Scope = ParsedScope.File;
        public string OwnerName = "";
    }

    /// <summary>
    /// Contains the info of a specific line number (built during the parsing)
    /// </summary>
    public class LineInfo {
        public int BlockDepth;
        public ParsedScope Scope;
        /// <summary>
        /// Name of the current procedure/part of main, definitions, preproc
        /// all in lower case
        /// </summary>
        public string CurrentScopeName;

        public LineInfo(int blockDepth, ParsedScope scope, string currentScopeName) {
            BlockDepth = blockDepth;
            Scope = scope;
            CurrentScopeName = currentScopeName;
        }
    }
}
