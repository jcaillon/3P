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
using System.Text;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletionFeature;
using _3PA.NppCore;

namespace _3PA.MainFeatures.Parser {
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
        /// Represent the FILE LEVEL scope
        /// </summary>
        private ParsedScopeItem _rootScope;

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

        /// <summary>
        /// Contains a dictionary in which each variable name known corresponds to its value tokenized
        /// It can either be parameters from an include, ex: {1}->SHARED, {& name}->_extension
        /// or & DEFINE variables from the current file
        /// </summary>
        private Dictionary<string, List<Token>> _globalPreProcVariables = new Dictionary<string, List<Token>>(StringComparer.CurrentCultureIgnoreCase);

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
        public Parser(string data, string filePathBeingParsed, ParsedScopeItem defaultScope, bool matchKnownWords) : this(NewLexerFromData(data), filePathBeingParsed, defaultScope, matchKnownWords) {}

        /// <summary>
        /// Parses a text into a list of parsedItems
        /// </summary>
        public Parser(ProLexer proLexer, string filePathBeingParsed, ParsedScopeItem defaultScope, bool matchKnownWords) {
            // process inputs
            _filePathBeingParsed = filePathBeingParsed;
            _matchKnownWords = matchKnownWords && KnownStaticItems != null;

            // the first of this list represents the file currently being parsed
            _parsedIncludes.Add(
                new ParsedIncludeFile(
                    "root",
                    new TokenEos(null, 0, 0, 0, 0),
                    // the preprocessed variable {0} equals to the filename...
                    new Dictionary<string, List<Token>>(StringComparer.CurrentCultureIgnoreCase) {
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
                var rootToken = new TokenEos(null, 0, 0, 0, 0);
                rootToken.OwnerNumber = 0;
                _rootScope = new ParsedFile("Root", rootToken);
                AddParsedItem(_rootScope, rootToken.OwnerNumber);
            } else
                _rootScope = defaultScope;
            _context.Scope = _rootScope;

            // Analyze
            _tokenList = proLexer.GetTokensList;
            _tokenCount = _tokenList.Count;
            ReplacePreProcVariablesAhead(1); // replaces a preproc var {&x} at token position 0
            ReplacePreProcVariablesAhead(2); // replaces a preproc var {&x} at token position 1
            while (MoveNext()) {
                try {
                    Analyze();
                } catch (Exception e) {
                    ErrorHandler.LogError(e, "Error while parsing the following file : " + filePathBeingParsed);
                }
            }

            // add missing values to the line dictionary
            var current = new LineInfo(GetCurrentDepth(), _rootScope);
            for (int i = proLexer.MaxLine; i >= 0; i--) {
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
            var lexer = new ProLexer(data);
            var outList = lexer.GetTokensList.ToList();
            outList.RemoveAt(outList.Count - 1);
            return outList;
        }

        #endregion

        #region utils

        /// <summary>
        /// If it is a pre-processed variable, replaces a token at "current position + posAhead" by its value
        /// </summary>
        private void ReplacePreProcVariablesAhead(int posAhead) {
            // we check if the token + posAhead will be a preprocessed variable { & x} that needs to be replaced
            var toReplaceToken = PeekAt(posAhead);

            HashSet<string> replacedVar = null; // keep track of each var replaced here

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
                List<Token> valueTokens = null;

                // make sure to not replace the same var name in the same replacement loop, if we do that
                // this means we will go into an infinite loop
                if (replacedVar == null)
                    replacedVar = new HashSet<string>();
                if (replacedVar.Contains(varName))
                    break;
                replacedVar.Add(varName);

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

                // do we have a definition for the var?
                if (_parsedIncludes[toReplaceToken.OwnerNumber].ScopedPreProcVariables.ContainsKey(varName))
                    valueTokens = _parsedIncludes[toReplaceToken.OwnerNumber].ScopedPreProcVariables[varName].ToList();
                else if (_globalPreProcVariables.ContainsKey(varName))
                    valueTokens = _globalPreProcVariables[varName].ToList();

                if (valueTokens == null) {
                    // if we don't have the definition for the variable, it must be replaced by an empty string
                    valueTokens = new List<Token> {
                        new TokenWhiteSpace("", toReplaceToken.Line, toReplaceToken.Column, toReplaceToken.StartPosition, toReplaceToken.EndPosition)
                    };
                } else {
                    // we have to "merge" the TokenWord at the beginning and end of what we are inserting, this allows to take care of
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
            // store the line information for the base file
            if (_context.StatementFirstToken != null && _context.StatementFirstToken.OwnerNumber == 0) {
                var statementStartLine = _context.StatementFirstToken.Line;

                var currentScope = _context.Scope.ScopeType == ParsedScopeType.Root && _context.UibBlockStack.Count > 0 ? _context.UibBlockStack.Peek() : _context.Scope;

                // remember the blockDepth of the current token's line (add block depth if the statement started after else of then)
                var depth = GetCurrentDepth();
                if (!_lineInfo.ContainsKey(statementStartLine))
                    _lineInfo.Add(statementStartLine, new LineInfo(depth, currentScope));

                // add missing values to the line dictionary (lines from start statement + 1 to end of statement have a depth + 1)
                if (statementStartLine > -1 && token.Line > statementStartLine) {
                    for (int i = statementStartLine + 1; i <= token.Line; i++)
                        if (!_lineInfo.ContainsKey(i))
                            _lineInfo.Add(i, new LineInfo(depth + 1, currentScope));
                }
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
        private void AddParsedItem(ParsedItem item, ushort ownerNumber) {
            // add external flag + include line if needed
            if (ownerNumber > 0 && ownerNumber < _parsedIncludes.Count) {
                item.FilePath = _parsedIncludes[ownerNumber].FullFilePath;
                item.IncludeLine = _parsedIncludes[ownerNumber].Line;
                item.Flags |= ParseFlag.FromInclude;
            } else {
                item.FilePath = _filePathBeingParsed;
            }

            item.Scope = _context.Scope;

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
        private string GetTokenStrippedValue(Token token) {
            if (token is TokenString) {
                var endsWithQuote = token.Value.EndsWith("\"") || token.Value.EndsWith("'");
                return token.Value.Substring(1, token.Value.Length - (endsWithQuote ? 2 : 1));
            }
            return token.Value;
        }

        /// <summary>
        /// Trim whitespaces tokens at the beginning and end of the list
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

        /// <summary>
        /// Create a new parsed define item according to its type
        /// </summary>
        private ParsedDefine NewParsedDefined(string name, ParseFlag flags, Token token, ParsedAsLike asLike, string left, ParseDefineType type, string tempPrimitiveType, string viewAs, string bufferFor) {
            // set flags
            flags |= _context.Scope is ParsedFile ? ParseFlag.FileScope : ParseFlag.LocalScope;

            switch (type) {
                case ParseDefineType.Parameter:
                    flags |= ParseFlag.Parameter;
                    break;
                case ParseDefineType.Buffer:
                    flags |= ParseFlag.Buffer;

                    var newBuffer = new ParsedBuffer(name, token, asLike, left, type, tempPrimitiveType, viewAs, bufferFor, ConvertStringToParsedPrimitiveType(tempPrimitiveType, asLike == ParsedAsLike.Like)) {
                        TargetTable = FindAnyTableByName(bufferFor)
                    };

                    flags |= !bufferFor.Contains(".") && newBuffer.TargetTable != null && !newBuffer.TargetTable.IsTempTable ? ParseFlag.MissingDbName : 0;
                    newBuffer.Flags = flags;

                    return newBuffer;
                    
            }
            var newDefine = new ParsedDefine(name, token, asLike, left, type, tempPrimitiveType, viewAs, ConvertStringToParsedPrimitiveType(tempPrimitiveType, asLike == ParsedAsLike.Like)) {
                Flags = flags
            };
            return newDefine;
        }

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
            if (String.IsNullOrEmpty(str)) {
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
        UnexpectedIfEndIfBlockEnd
    }

    #endregion
}