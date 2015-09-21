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
        private Dictionary<int, LineInfo> _outputLineInfo = new Dictionary<int, LineInfo>();
        private Dictionary<int, TempLineInfo> _lineInfo = new Dictionary<int, TempLineInfo>();
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
        public Dictionary<int, LineInfo> GetLineInfo { get { return _outputLineInfo; } }
        //public Dictionary<int, TempLineInfo> GetLineInfo { get { return _lineInfo; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public Parser(string data) {
            // parse
            _lexer = new Lexer(data);
            _lexer.Tokenize();
            while (MoveNext()) {
                Analyze();
            }

            // create a better line info dictionnary
            int currentBlockDepth = 0;
            int outerCount = 1;
            List<int> scopeDefinition = new List<int>() { 1 };
            string currentScopeName = "";
            for (int i = 0; i < _lexer.MaxLine; i++) {
                if (_lineInfo.ContainsKey(i)) {
                    while (_lineInfo[i].BlockDepth != currentBlockDepth) {
                        // scoping (we need to update info of this line only for the next line!)
                        if (_lineInfo[i].BlockDepth > currentBlockDepth) {
                            scopeDefinition.Add(outerCount);
                            outerCount = 1;
                            currentBlockDepth++;
                        } else {
                            // unscoping
                            var x = scopeDefinition.Count - 1;
                            outerCount = scopeDefinition[x];
                            outerCount++;
                            scopeDefinition.RemoveAt(x);
                            currentBlockDepth--;
                        }
                    }
                    currentScopeName = _lineInfo[i].CurrentScopeName;
                }
                _outputLineInfo.Add(i, new LineInfo(string.Concat(scopeDefinition.Select(i1 => i1.ToString() + ",")), currentScopeName));
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
#warning for debug!
                            if (CreateParsedFunction(token)) {
                                if (_context.BlockDepth != 0) throw new Exception("We should be at _context.BlockDepth == 0! and we are at _context.BlockDepth = " + _context.BlockDepth);
                                _context.BlockDepth = 0;
                                _increaseDepthAtNextStatement = true;
                            }
                            break;
                        case "procedure":
                            if (CreateParsedProcedure(token)) {
                                if (_context.BlockDepth != 0) throw new Exception("We should be at _context.BlockDepth == 0! and we are at _context.BlockDepth = " + _context.BlockDepth);
                                _context.BlockDepth = 0;
                                _increaseDepthAtNextStatement = true;
                            }
                            break;
                    }

                    // matches start of block at the beggining of a statement
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
                            _increaseDepthAtNextStatement = true;
                            break;
                        case "end":
                            _context.BlockDepth--;
                            if (_context.BlockDepth == 0) {
                                _context.Scope = ParseScope.Global;
                                if (!(_context.OwnerIfNotGlobal is ParsedGlobal))
                                    _context.OwnerIfNotGlobal = new ParsedGlobal("");
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
        /// Flushes the current statement into the list, clear the statement
        /// </summary>
        private void NewStatement() {
            // remember the blockDepth of the current token's line (add block depth if the statement started after else of then)
            if (!_lineInfo.ContainsKey(_context.StatementStartLine))
                _lineInfo.Add(_context.StatementStartLine, new TempLineInfo((_lastStatementOneTimeIncrease ? 1 : 0) + _context.BlockDepth, (_context.OwnerIfNotGlobal != null) ? _context.OwnerIfNotGlobal.Name : ""));
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
                    _parsedItemList.Add(new ParsedPreProc(name, ParseFlag.Global, token.Line, token.Column, 0));
                    break;
                case "&SCOPED-DEFINE":
                case "&SCOPED":
                    _parsedItemList.Add(new ParsedPreProc(name, ParseFlag.Scope, token.Line, token.Column, 0));
                    break;
                case "&ANALYZE-SUSPEND":
                    _context.Scope = ParseScope.Global;
                    if (toParse.Contains("_DEFINITIONS", StringComparison.OrdinalIgnoreCase))
                        _context.OwnerIfNotGlobal = new ParsedGlobal("definitions");
                    else if (toParse.Contains("_UIB-PREPROCESSOR-BLOCK", StringComparison.OrdinalIgnoreCase))
                        _context.OwnerIfNotGlobal = new ParsedGlobal("preprocessor");
                    else if (toParse.Contains("_MAIN-BLOCK", StringComparison.OrdinalIgnoreCase))
                        _context.OwnerIfNotGlobal = new ParsedGlobal("mainblock");
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
            var x = new ParsedProcedure(name, ParseFlag.None, procToken.Line, procToken.Column, leftStr.ToString());
            _context.Scope = ParseScope.Procedure;
            _context.OwnerIfNotGlobal = x;
            _parsedItemList.Add(x);
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
            var x = new ParsedFunction(name, isPrivate ? ParseFlag.Private : ParseFlag.None, functionToken.Line, functionToken.Column, returnType, parameters.ToString());
            _context.Scope = ParseScope.Function;
            _context.OwnerIfNotGlobal = x;
            _parsedItemList.Add(x);
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

            _parsedItemList.Add(new ParsedIncludeFile(toParse, ParseFlag.None, token.Line, token.Column));
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
        public ParsedScope OwnerIfNotGlobal;
    }

    /// <summary>
    /// Contains the info of a specific line number (built during the parsing)
    /// </summary>
    public class TempLineInfo {
        public int BlockDepth;
        public string CurrentScopeName;
        public TempLineInfo(int blockDepth, string currentScopeName) {
            BlockDepth = blockDepth;
            CurrentScopeName = currentScopeName;
        }
    }

    /// <summary>
    /// Contains the info of a specific line number (built AFTER the parsing)
    /// </summary>
    public class LineInfo {
        public string ScopeDefinition;
        public string CurrentScopeName;
        public LineInfo(string scopeDefinition, string currentScopeName) {
            ScopeDefinition = scopeDefinition;
            CurrentScopeName = currentScopeName;
        }
    }
}
