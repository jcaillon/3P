using System;
using System.Collections.Generic;
using System.Text;
using _3PA.Lib;

namespace _3PA.MainFeatures.Parser {
    public class Parser {
        private List<ParsedItem> _parsedItemList = new List<ParsedItem>();
        private Lexer _lexer;
        private ParseContext _context = new ParseContext();

        /// <summary>
        /// dictionnay of *line, line indentation*
        /// </summary>
        public Dictionary<int, int> LineIndent { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public Parser(string data) {
            LineIndent = new Dictionary<int, int>();

            // parse
            _lexer = new Lexer(data);
            _lexer.Tokenize();
            while (MoveNext()) {
                Analyze();
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
            if (token is TokenEof) {
                return;
            }

            // matching a word
            if (token is TokenWord) {

                // was the last token a "else" or a "then"? then we consider this TokenWord to be the first of a statement
                // TODO: correct the BlockDepth of a statement after else or then
                if (_context.PotentialNewStatement)
                    NewStatement();

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
                                _context.BlockDepth = 1;
                            }
                            break;
                        case "procedure":
                            if (CreateParsedProcedure(token)) {
                                if (_context.BlockDepth != 0) throw new Exception("We should be at _context.BlockDepth == 0! and we are at _context.BlockDepth = " + _context.BlockDepth);
                                _context.BlockDepth = 1;
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
                            _context.BlockDepth++;
                            break;
                        case "end":
                            _context.BlockDepth--;
                            if (_context.BlockDepth == 0)
                                _context.Scope = ParseScope.Global;
                            break;
                    }
                    
                } else {
                    // matches a do in the middle of a statement (ex: ON CHOOSE OF xx DO:)
                    if (lowerTok.Equals("do") || (lowerTok.Equals("triggers") && PeekAt(1) is TokenEos)) {
                        _context.BlockDepth++;
                    }
                }

                // we might need to proceed the next word after a "then" or a "else" as the first word
                // of a statement
                if (lowerTok.Equals("then") || lowerTok.Equals("else"))
                    _context.PotentialNewStatement = true;
            }

            // include
            else if (token is TokenInclude) {
                CreateParsedIncludeFile(token);
            }

            // end of statement
            else if (token is TokenEos) 
                NewStatement();

            // remember the blockDepth of the current token's line
            if (!LineIndent.ContainsKey(token.Line))
                LineIndent.Add(token.Line, _context.BlockDepth);
            else
                LineIndent[token.Line] = _context.BlockDepth;
        }

        /// <summary>
        /// Flushes the current statement into the list, clear the statement
        /// </summary>
        private void NewStatement() {
            _context.StatementWordCount = 0;
            _context.PotentialNewStatement = false;
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

    public class ParseContext {
        public List<Token> StatementTokenList = new List<Token>();
        public int StatementWordCount;
        public int BlockDepth;
        public ParseScope Scope = ParseScope.Global;
        public ParsedScope OwnerIfNotGlobal;
        public bool PotentialNewStatement;
    }
}
