#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Text;
using YamuiFramework.Helper;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletionFeature;
using _3PA.NppCore;

namespace _3PA.MainFeatures.Parser.Pro {

    /// <summary>
    /// This class is not actually a parser "per say" but it extracts important information
    /// from the tokens created by the proLexer
    /// </summary>
    internal partial class Parser {

        #region static

        /// <summary>
        /// A dictionary of known keywords and database info
        /// </summary>
        private Dictionary<string, CompletionType> KnownStaticItems {
            get { return ParserHandler.KnownStaticItems; }
        }

        /// <summary>
        /// Set this function to return the full file path of an include (the parameter is the file name of partial path /folder/include.i)
        /// </summary>
        private Func<string, string> FindIncludeFullPath {
            get { return ParserHandler.FindIncludeFullPath; }
        }

        /// <summary>
        /// Instead of parsing the include files each time we store the results of the proLexer to use them when we need it
        /// </summary>
        private Dictionary<string, ProLexer> SavedLexerInclude {
            get { return ParserHandler.SavedLexerInclude; }
        }

        private static ProLexer NewLexerFromData(string data) {
            return new ProLexer(data);
        }

        #endregion

        #region private fields

        /// <summary>
        /// List of the parsed items (output)
        /// </summary>
        private List<ParsedItem> _parsedItemList = new List<ParsedItem>();

        /// <summary>
        /// Contains the information of each line parsed
        /// </summary>
        private Dictionary<int, LineInfo> _lineInfo = new Dictionary<int, LineInfo>();

        /// <summary>
        /// list of errors found by the parser
        /// </summary>
        private List<ParserError> _parserErrors = new List<ParserError>();

        /// <summary>
        /// Result of the proLexer, list of tokens
        /// </summary>
        private GapBuffer<Token> _tokenList;

        private int _tokenCount;
        private int _tokenPos = -1;

        /// <summary>
        /// Contains the current information of the statement's context (in which proc it is, which scope...)
        /// </summary>
        private ParseContext _context;

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
        /// In the file being parsed we can have includes, the files included are read, tokenized, and the tokens
        /// are inserted for the current file
        /// But we need to know from which file each token is extracted, this is the purpose of this list :
        /// the [0] will designate the current procedure file, [1] the first include and so on...
        /// </summary>
        private List<ParsedIncludeFile> _parsedIncludes = new List<ParsedIncludeFile>();
        
        #endregion

        #region Public properties

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
        /// Returns a string that describes the errors found by the parser (relative to block start/end)
        /// Returns null if no errors were found
        /// </summary>
        public string ParseErrorsInHtml {
            get {
                var error = new StringBuilder();
                if (_parserErrors != null && _parserErrors.Count > 0) {
                    foreach (var parserError in _parserErrors) {
                        error.AppendLine("<div>");
                        error.AppendLine(" - " + (parserError.FullFilePath + "|" + parserError.TriggerLine).ToHtmlLink("Line " + (parserError.TriggerLine + 1)) + ", " + parserError.Type.GetDescription());
                        error.AppendLine("</div>");
                    }
                }
                return error.ToString();
            }
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
        /// Constructor with a string instead of a proLexer
        /// </summary>
        public Parser(string data, string filePathBeingParsed, ParsedScopeBlock defaultScope, bool matchKnownWords) : this(NewLexerFromData(data), filePathBeingParsed, defaultScope, matchKnownWords, null) {}

        public Parser(ProLexer proLexer, string filePathBeingParsed, ParsedScopeBlock defaultScope, bool matchKnownWords, StringBuilder debugListOut) : this(proLexer.GetTokensList, filePathBeingParsed, defaultScope, matchKnownWords, debugListOut) {}

        /// <summary>
        /// Parses a list of tokens into a list of parsedItems
        /// </summary>
        public Parser(GapBuffer<Token> tokens, string filePathBeingParsed, ParsedScopeBlock defaultScope, bool matchKnownWords, StringBuilder debugListOut) {

            // process inputs
            ParsedScopeBlock rootScope;
            _filePathBeingParsed = filePathBeingParsed;
            _matchKnownWords = matchKnownWords && KnownStaticItems != null;

            // the first of this list represents the file currently being parsed
            _parsedIncludes.Add(
                new ParsedIncludeFile(
                    "root",
                    new TokenEos(null, 0, 0, 0, 0),
                    // the preprocessed variable {0} equals to the filename...
                    new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase) {
                        {"0", Path.GetFileName(FilePathBeingParsed)}
                    },
                    _filePathBeingParsed,
                    null)
            );

            // init context
            _context = new ParseContext {
                BlockStack = new Stack<ParsedScope>()
            };

            // create root item
            if (defaultScope == null) {
                var rootToken = new TokenEos(null, 0, 0, 0, 0);
                rootToken.OwnerNumber = 0;
                rootScope = new ParsedFile("Root", rootToken);
            } else
                rootScope = defaultScope;
            _context.BlockStack.Push(rootScope);
            if (defaultScope == null) {
                AddParsedItem(rootScope, 0);
            }

            // Analyze
            _tokenList = tokens;
            _tokenCount = _tokenList.Count;
            _tokenPos = -1;
            ReplaceIncludeAndPreprocVariablesAhead(1); // replaces an include or a preproc var {&x} at token position 0
            ReplaceIncludeAndPreprocVariablesAhead(2); // @position 1
            while (MoveNext()) {
                try {
                    Analyze();
                } catch (Exception e) {
                    ErrorHandler.LogError(e, "Error while parsing the following file : " + filePathBeingParsed);
                }
            }
            
            // add missing values to the line dictionary (each line that correspond to the root scope)
            var defaultLineinfo = new LineInfo(0, rootScope, rootScope);
            for (int i = 0; i <= PeekAt(-1).Line; i++) {
                if (!_lineInfo.ContainsKey(i))
                    _lineInfo.Add(i, defaultLineinfo);
            }


            // check that we match a RESUME for each SUSPEND
            if (GetCurrentBlock<ParsedScopePreProcBlock>() != null)
                _parserErrors.Add(new ParserError(ParserErrorType.MissingUibBlockEnd, PeekAt(-1), _context.BlockStack.Count, _parsedIncludes));

            // check that we match an &ENDIF for each &IF
            if (GetCurrentBlock<ParsedScopePreProcIfBlock>() != null)
                _parserErrors.Add(new ParserError(ParserErrorType.MissingPreprocEndIf, PeekAt(-1), _context.BlockStack.Count, _parsedIncludes));
            
            // check that we match an END. for each block
            if (GetCurrentBlock<ParsedScopeBlock>() != rootScope)
                _parserErrors.Add(new ParserError(ParserErrorType.MissingBlockEnd, PeekAt(-1), _context.BlockStack.Count, _parsedIncludes));
              

            // returns the concatenation of all the tokens once the parsing is done
            if (debugListOut != null) {
                foreach (var token in _tokenList) {
                    debugListOut.Append(token.Value);
                }
            }

            // dispose
            _context.BlockStack.Clear();
            _context = null;
            _tokenList = null;

            // if we are parsing an include file that was saved for later use, update it
            if (SavedLexerInclude.ContainsKey(filePathBeingParsed))
                SavedLexerInclude.Remove(filePathBeingParsed);
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
        /// Peek forward x tokens, returns an TokenEof if out of limits (can be used with negative values)
        /// </summary>
        private Token PeekAt(int x) {
            return (_tokenPos + x >= _tokenCount || _tokenPos + x < 0) ? new TokenEof("", -1, -1, -1, -1) : _tokenList[_tokenPos + x];
        }

        /// <summary>
        /// Peek forward (or backward if goBackWard = true) until we match a token that is not a space token
        /// return found token
        /// </summary>
        private Token PeekAtNextType<T>(int start, bool goBackward = false) where T : Token {
            int x = start + (goBackward ? -1 : 1);
            var tok = PeekAt(x);
            while (!(tok is T) && !(tok is TokenEof))
                tok = PeekAt(goBackward ? x-- : x++);
            return tok;
        }

        /// <summary>
        /// Peek forward (or backward if goBackWard = true) until we match a token that is not a space token
        /// return found token
        /// </summary>
        private Token PeekAtNextNonType<T>(int start, bool goBackward = false) where T : Token {
            int x = start + (goBackward ? -1 : 1);
            var tok = PeekAt(x);
            while (tok is T && !(tok is TokenEof))
                tok = PeekAt(goBackward ? x-- : x++);
            return tok;
        }

        /// <summary>
        /// Move to the next token
        /// </summary>
        private bool MoveNext() {
            // move to the next token
            if (++_tokenPos >= _tokenCount)
                return false;

            //  analyze the current token
            AnalyseForEachToken(PeekAt(0));

            return true;
        }

        /// <summary>
        /// Replace the token at the current pos + x by the token given
        /// </summary>
        private void ReplaceToken(int x, Token token) {
            if (_tokenPos + x < _tokenCount)
                _tokenList[_tokenPos + x] = token;
        }

        /// <summary>
        /// Inserts tokens at the current pos + x
        /// </summary>
        private void InsertTokens(int x, List<Token> tokens) {
            if (_tokenPos + x < _tokenCount) {
                _tokenList.InsertRange(_tokenPos + x, tokens);
                _tokenCount = _tokenList.Count;
            }
        }

        private void RemoveTokens(int x, int count) {
            count = count.ClampMax(_tokenCount - _tokenPos - x);
            if (_tokenPos + x + count <= _tokenCount && count > 0) {
                _tokenList.RemoveRange(_tokenPos + x, count);
                _tokenCount = _tokenList.Count;
            }
        }
        
        #endregion

        #region utils

        #region find primitive type

        /// <summary>
        /// Returns a primitive type from a string
        /// </summary>
        public static ParsedPrimitiveType ConvertStringToParsedPrimitiveType(string str) {
            str = str.ToLower();

            // AS
            switch (str) {
                case "com-handle":
                    return ParsedPrimitiveType.Comhandle;
                case "datetime-tz":
                    return ParsedPrimitiveType.Datetimetz;
                case "unsigned-short":
                    return ParsedPrimitiveType.UnsignedShort;
                case "unsigned-long":
                    return ParsedPrimitiveType.UnsignedLong;
                case "table-handle":
                    return ParsedPrimitiveType.TableHandle;
                case "dataset-handle":
                    return ParsedPrimitiveType.DatasetHandle;
                case "widget-handle":
                    return ParsedPrimitiveType.WidgetHandle;
                default:
                    ParsedPrimitiveType primType;
                    if (Enum.TryParse(str, true, out primType))
                        return primType;
                    break;
            }

            // try to find the complete word in abbreviations list
            var completeStr = Keywords.Instance.GetFullKeyword(str);
            if (completeStr != null) {
                ParsedPrimitiveType primType;
                if (Enum.TryParse(completeStr, true, out primType))
                    return primType;
            }

            return ParsedPrimitiveType.Unknow;
        }

        /// <summary>
        /// conversion
        /// </summary>
        private ParsedPrimitiveType ConvertStringToParsedPrimitiveType(string str, bool analyseLike) {
            if (string.IsNullOrEmpty(str)) {
                return ParsedPrimitiveType.Unknow;
            }

            str = str.ToLower();
            // LIKE
            if (analyseLike)
                return FindPrimitiveTypeOfLike(str);
            return ConvertStringToParsedPrimitiveType(str);
        }

        /// <summary>
        /// Search through the available completionData to find the primitive type of a 
        /// "like xx" phrase
        /// </summary>
        private ParsedPrimitiveType FindPrimitiveTypeOfLike(string likeStr) {
            // determines the format
            var nbPoints = likeStr.CountOccurences(".");
            var splitted = likeStr.Split('.');

            // if it's another var
            if (nbPoints == 0) {
                var foundVar = _parsedItemList.Find(data => {
                    var def = data as ParsedDefine;
                    return def != null && def.Type != ParseDefineType.Buffer && def.PrimitiveType != ParsedPrimitiveType.Unknow && def.Name.EqualsCi(likeStr);
                }) as ParsedDefine;
                return foundVar != null ? foundVar.PrimitiveType : ParsedPrimitiveType.Unknow;
            }

            // Search the databases
            var foundField = DataBase.Instance.FindFieldByName(likeStr);
            if (foundField != null)
                return foundField.Type;

            var tableName = splitted[nbPoints == 2 ? 1 : 0];
            var fieldName = splitted[nbPoints == 2 ? 2 : 1];

            // Search in temp tables
            if (nbPoints != 1)
                return ParsedPrimitiveType.Unknow;

            var foundTtable = FindAnyTableOrBufferByName(tableName);
            if (foundTtable == null)
                return ParsedPrimitiveType.Unknow;

            var foundTtField = foundTtable.Fields.Find(field => field.Name.EqualsCi(fieldName));
            return foundTtField == null ? ParsedPrimitiveType.Unknow : foundTtField.Type;
        }

        #endregion

        #region find table, buffer, temptable

        /// <summary>
        /// finds a ParsedTable for the input name, it can either be a database table,
        /// a temptable, or a buffer name (in which case we return the associated table)
        /// </summary>
        private ParsedTable FindAnyTableOrBufferByName(string name) {
            // temptable or table
            var foundTable = FindAnyTableByName(name);
            if (foundTable != null)
                return foundTable;
            // for buffer, we return the referenced temptable/table (stored in CompletionItem.SubString)
            var foundBuffer = _parsedItemList.Find(data => data is ParsedBuffer && data.Name.EqualsCi(name)) as ParsedBuffer;
            return foundBuffer != null ? FindAnyTableByName(foundBuffer.BufferFor) : null;
        }

        /// <summary>
        /// Find the table referenced among database and defined temp tables; 
        /// name is the table's name (can also be BASE.TABLE)
        /// </summary>
        private ParsedTable FindAnyTableByName(string name) {
            return DataBase.Instance.FindTableByName(name) ?? FindTempTableByName(name);
        }

        /// <summary>
        /// Find a temptable by name
        /// </summary>
        private ParsedTable FindTempTableByName(string name) {
            return _parsedItemList.Find(item => {
                var tt = item as ParsedTable;
                return tt != null && tt.IsTempTable && tt.Name.EqualsCi(name);
            }) as ParsedTable;
        }

        #endregion
        
        #endregion

        #region internal classes

        /// <summary>
        /// contains the info on the current context (as we move through tokens)
        /// </summary>
        internal class ParseContext {

            /// <summary>
            /// Keep tracks on blocks through a stack (a block == an indent)
            /// </summary>
            public Stack<ParsedScope> BlockStack { get; set; }

            /// <summary>
            /// Number of words count in the current statement
            /// </summary>
            public int StatementWordCount { get; set; }

            /// <summary>
            /// A statement can start with a word, pre-proc phrase or an include
            /// </summary>
            public Token StatementFirstToken { get; set; }
            
            /// <summary>
            /// True if the first word of the statement didn't match a known statement
            /// </summary>
            public bool StatementUnknownFirstWord { get; set; }

            /// <summary>
            /// We matched a THEN or ELSE in this statement, it will have the value of the line number of the next
            /// word after a THEN or ELSE
            /// </summary>
            public int StatementFirstWordLineAferThenOrElse { get; set; }

            /// <summary>
            /// Allows to read the next word after a THEN or a ELSE as the first word of a statement to 
            /// correctly read ASSIGN statement for instance...
            /// </summary>
            public bool ReadNextWordAsStatementStart { get; set; }

            /// <summary>
            /// Last ON block matched
            /// </summary>
            public ParsedScopeBlock LastOnBlock { get; set; } 

            /// <summary>
            /// Useful to know if, during the last &amp;if / elseif / else / endif,
            /// one of the if expression hsa been true. If not, we know that the &amp;else block will be true
            /// </summary>
            public bool LastPreprocIfwasTrue { get; set; }
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
        public ParsedScopeSection ExplorerScope { get; set; }

        /// <summary>
        /// Scope for the current line
        /// </summary>
        public ParsedScopeBlock VariableScope { get; set; }

        public LineInfo(int blockDepth, ParsedScopeSection explorerScope, ParsedScopeBlock variableScope) {
            BlockDepth = blockDepth;
            ExplorerScope = explorerScope;
            VariableScope = variableScope;
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
        [Description("Unexpected block start, you can not nest functions, methods or procedures")]
        ForbiddenNestedBlockStart,

        [Description("Unexpected block end, the start of this block has not been found")]
        UnexpectedBlockEnd,

        [Description("A block end seems to be missing")]
        MissingBlockEnd,

        [Description("Unexpected Appbuilder block start, two consecutive ANALYSE-SUSPEND found (no ANALYSE-RESUME)")]
        UnexpectedUibBlockStart,

        [Description("Unexpected Appbuilder block end, can not match ANALYSE-SUSPEND for this ANALYSE-RESUME")]
        UnexpectedUibBlockEnd,

        [Description("Unexpected Appbuilder block start, ANALYSE-SUSPEND should be created at root level")]
        NotAllowedUibBlockStart,

        [Description("Unexpected Appbuilder block end, ANALYSE-RESUME should be created at root level")]
        NotAllowedUibBlockEnd,

        [Description("A preprocessed directive ANALYSE-RESUME seems to be missing")]
        MissingUibBlockEnd,

        [Description("A preprocessed directive should always be at the beggining of a new statement")]
        UibBlockStartMustBeNewStatement,

        [Description("&ENDIF pre-processed statement matched without the corresponding &IF")]
        UnexpectedPreProcEndIf,

        [Description("&THEN pre-processed statement matched without the corresponding &IF")]
        UnexpectedPreprocThen,

        [Description("&IF pre-processed statement missing an &ENDIF")]
        MissingPreprocEndIf,
    }

    #endregion
}