#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
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
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletionFeature;
using _3PA.MainFeatures.Pro;

namespace _3PA.MainFeatures.Parser {
    /// <summary>
    /// This class is not actually a parser "per say" but it extracts important information
    /// from the tokens created by the lexer
    /// </summary>
    internal partial class Parser {
        #region static

        /// <summary>
        /// A dictionary of known keywords and database info
        /// </summary>
        private static Dictionary<string, CompletionType> _knownStaticItems = new Dictionary<string, CompletionType>();

        public static void UpdateKnownStaticItems() {
            // Update the known items! (made of BASE.TABLE, TABLE and all the KEYWORDS)
            _knownStaticItems = DataBase.GetDbDictionary();
            foreach (var keyword in Keywords.GetList().Where(keyword => !_knownStaticItems.ContainsKey(keyword.DisplayText))) {
                _knownStaticItems[keyword.DisplayText] = keyword.Type;
            }
        }

        /// <summary>
        /// Set this function to return the full file path of an include (the parameter is the file name of partial path /folder/include.i)
        /// </summary>
        public static Func<string, string> FindIncludeFullPath = s => ProEnvironment.Current.FindFirstFileInPropath(s);

        /// <summary>
        /// Instead of parsing the include files each time we store the results of the lexer to use them when we need it
        /// </summary>
        private static Dictionary<string, Lexer> _savedLexerInclude = new Dictionary<string, Lexer>();

        #endregion

        #region private fields

        /// <summary>
        /// List of the parsed items (output)
        /// </summary>
        private List<ParsedItem> _parsedItemList = new List<ParsedItem>();

        /// <summary>
        /// Represent the FILE LEVEL scope
        /// </summary>
        private ParsedScopeItem _rootScope;

        /// <summary>
        /// Result of the lexer, list of tokens
        /// </summary>
        private GapBuffer<Token> _tokenList;

        private int _tokenCount;
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
        /// In the file being parsed we can have includes, the files included are read, tokenized, and the tokens
        /// are inserted for the current file
        /// But we need to know from which file each token is extracted, this is the purpose of this list :
        /// the [0] will designate the current procedure file, [1] the first include and so on...
        /// </summary>
        private List<ParsedIncludeFile> _parsedIncludes = new List<ParsedIncludeFile>();

        /// <summary>
        /// Contains a dictionary in which each variable name known corresponds to its value tokenized
        /// It can either be parameters from an include, ex: {1}->SHARED, {& name}->_extension
        /// or & DEFINE variables from the current file
        /// </summary>
        private Dictionary<string, List<Token>> _globalPreProcVariables = new Dictionary<string, List<Token>>(StringComparer.CurrentCultureIgnoreCase);

        #endregion

        #region public accessors

        /// <summary>
        /// dictionary of *line, line info*
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

        /// <summary>
        /// Path to the file being parsed (is added to the parseItem info)
        /// </summary>
        public string FilePathBeingParsed {
            get { return _filePathBeingParsed; }
        }

        #endregion

        #region Life and death

        public Parser() {}

        /// <summary>
        /// Constructor with a string instead of a lexer
        /// </summary>
        public Parser(string data, string filePathBeingParsed, ParsedScopeItem defaultScope, bool matchKnownWords) : this(NewLexerFromData(data), filePathBeingParsed, defaultScope, matchKnownWords) {}

        /// <summary>
        /// Parses a text into a list of parsedItems
        /// </summary>
        public Parser(Lexer lexer, string filePathBeingParsed, ParsedScopeItem defaultScope, bool matchKnownWords) {
            // process inputs
            _filePathBeingParsed = filePathBeingParsed;
            _matchKnownWords = matchKnownWords && _knownStaticItems != null;
            // the first of this list represents the file currently being parsed
            _parsedIncludes.Add(
                new ParsedIncludeFile(
                    "root",
                    new TokenEos(null, 0, 0, 0, 0),
                    // the preprocessed variable {0} equals to the filename...
                    new Dictionary<string, List<Token>> {
                        {"0", new List<Token> {new TokenWord(Path.GetFileName(FilePathBeingParsed), 0, 0, 0, 0)}}
                    },
                    _filePathBeingParsed,
                    null)
                );

            // init context
            _context = new ParseContext {
                BlockStack = new Stack<BlockInfo>(),
                PreProcIfStack = new Stack<ParsedPreProcBlock>(),
                UibBlockStack = new Stack<ParsedPreProcBlock>()
            };

            // create root item
            if (defaultScope == null) {
                _rootScope = new ParsedFile("Root", new TokenEos(null, 0, 0, 0, 0));
                AddParsedItem(_rootScope, 0);
            } else
                _rootScope = defaultScope;
            _context.Scope = _rootScope;

            // Analyze
            _tokenList = lexer.GetTokensList;
            _tokenCount = _tokenList.Count;
            ReplacePreProcVariablesAhead(1); // replaces a preproc var {&x} at token position 0
            ReplacePreProcVariablesAhead(2); // replaces a preproc var {&x} at token position 1
            while (MoveNext()) {
                Analyze();
            }

            // add missing values to the line dictionary
            var current = new LineInfo(GetCurrentDepth(), _rootScope);
            for (int i = lexer.MaxLine - 1; i >= 0; i--) {
                if (_lineInfo.ContainsKey(i))
                    current = _lineInfo[i];
                else
                    _lineInfo.Add(i, current);
            }

            // check that we match an &ENDIF for each &IF
            if (_context.PreProcIfStack.Count > 0)
                _parserErrors.Add(new ParserError(ParserErrorType.MismatchNumberOfIfEndIf, PeekAt(0), _context.PreProcIfStack.Count, _parsedIncludes));

            // dispose
            _context.BlockStack.Clear();
            _context.PreProcIfStack.Clear();
            _context.UibBlockStack.Clear();
            _context = null;
            _tokenList = null;

            // if we are parsing an include file that was saved for later use, update it
            if (_savedLexerInclude.ContainsKey(filePathBeingParsed))
                _savedLexerInclude.Remove(filePathBeingParsed);
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
            visitor.PreVisit(this);
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
            return (_tokenPos + x >= _tokenCount || _tokenPos + x < 0) ? new TokenEof("", -1, -1, -1, -1) : _tokenList[_tokenPos + x];
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
            // before moving to the next token, we analyze the current token
            if (!_context.IsTokenIsEos && PeekAt(0) is TokenWord) {
                _context.StatementWordCount++;
            }
            _context.IsTokenIsEos = false;

            // move to the next token
            if (++_tokenPos >= _tokenCount)
                return false;

            // replace a pre proc var {&x} at current pos + 2
            ReplacePreProcVariablesAhead(2);

            return true;
        }

        /// <summary>
        /// Replace the token at the current pos + x by the token given
        /// </summary>
        public void ReplaceToken(int x, Token token) {
            if (_tokenPos + x < _tokenCount)
                _tokenList[_tokenPos + x] = token;
        }

        /// <summary>
        /// Inserts tokens at the current pos + x
        /// </summary>
        public void InsertTokens(int x, List<Token> tokens) {
            if (_tokenPos + x < _tokenCount) {
                _tokenList.InsertRange(_tokenPos + x, tokens);
                _tokenCount = _tokenList.Count;
            }
        }

        public void RemoveTokens(int x, int count) {
            if (_tokenPos + x + count <= _tokenCount) {
                _tokenList.RemoveRange(_tokenPos + x, count);
                _tokenCount = _tokenList.Count;
            }
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

        /// <summary>
        /// Can either be in the procedure parser or in an include file
        /// </summary>
        public string FullFilePath { get; set; }

        public ParserError(ParserErrorType type, Token triggerToken, int stackCount, List<ParsedIncludeFile> includeFiles) {
            Type = type;
            TriggerLine = triggerToken.Line;
            TriggerPosition = triggerToken.StartPosition;
            StackCount = stackCount;
            FullFilePath = includeFiles[triggerToken.OwnerNumber].FullFilePath;
        }
    }

    internal enum ParserErrorType {
        [Description("Unexpected block start, this type of block should be created at root level")] UnexpectedBlockStart,
        [Description("Unexpected block end, the start of this block has not been found")] UnexpectedBlockEnd,
        [Description("Unexpected Appbuilder block start, two consecutive ANALYSE-SUSPEND found (no ANALYSE-RESUME)")] UnexpectedUibBlockStart,
        [Description("Unexpected Appbuilder block end, can not match ANALYSE-SUSPEND for this ANALYSE-RESUME")] UnexpectedUibBlockEnd,
        [Description("Unexpected Appbuilder block start, ANALYSE-SUSPEND should be created at root level")] NotAllowedUibBlockStart,
        [Description("Unexpected Appbuilder block end, ANALYSE-RESUME should be created at root level")] NotAllowedUibBlockEnd,
        [Description("&IF pre-processed statement missing an &ENDIF")] MismatchNumberOfIfEndIf,
        [Description("&ENDIF pre-processed statement matched without the corresponding &IF")] UnexpectedIfEndIfBlockEnd
    }

    #endregion
}