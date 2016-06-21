#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (ParserVisitor.cs) is part of 3P.
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
using System.IO;
using System.Linq;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.CodeExplorer;
using _3PA.MainFeatures.ProgressExecutionNs;

namespace _3PA.MainFeatures.Parser {

    /// <summary>
    /// This class sustains the autocompletion list AND the code explorer list
    /// by visiting the parser and creating new completionData
    /// </summary>
    internal class ParserVisitor : IParserVisitor {

        #region Fields

        private const string BlockTooLongString = "Too long!";

        /// <summary>
        /// Are we currently visiting the current file opened in npp or
        /// is it a include?
        /// </summary>
        private bool _isBaseFile;

        /// <summary>
        /// Stores the file name of the file currently visited/parsed
        /// </summary>
        private string _currentParsedFile;

        /// <summary>
        /// this dictionnary is used to reference the procedures defined
        /// in the program we are parsing, dictionnary is faster that list when it comes to
        /// test if a procedure/function exists in the program
        /// </summary>
        public HashSet<string> DefinedProcedures = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Line info from the parser
        /// </summary>
        private Dictionary<int, LineInfo> _lineInfo;

        /// <summary>
        /// contains the list of items that depend on the current file, that list
        /// is updated by the parser's visitor class
        /// </summary>
        public List<CompletionData> ParsedItemsList = new List<CompletionData>();

        /// <summary>
        /// Contains the list of explorer items for the current file, updated by the parser's visitor class
        /// </summary>
        public List<CodeExplorerItem> ParsedExplorerItemsList = new List<CodeExplorerItem>();

        /// <summary>
        /// We keep tracks of the parsed files, to avoid parsing the same file twice
        /// </summary>
        private static HashSet<string> _parsedFiles = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

        #endregion

        #region constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isBaseFile"></param>
        /// <param name="currentParsedFile"></param>
        /// <param name="lineInfo"></param>
        public ParserVisitor(bool isBaseFile, string currentParsedFile, Dictionary<int, LineInfo> lineInfo) {
            _isBaseFile = isBaseFile;
            _currentParsedFile = currentParsedFile;
            _lineInfo = lineInfo;

            // reset the parsed files for the session
            if (isBaseFile)
                _parsedFiles.Clear();
        }

        #endregion

        #region visit implementation

        /// <summary>
        /// Run statement,
        /// a second pass will be done after the visit is over to determine if a run is
        /// internal or external (calling internal proc or programs)
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedRun pars) {

            // try to find the file in the propath
            string fullFilePath = "";
            if (pars.HasPersistent && !pars.IsEvaluateValue) {
                string procName = pars.Name;
                if (!procName.EndsWith(".p") && !procName.EndsWith(".w")) {
                    fullFilePath = ProEnvironment.Current.FindFirstFileInEnv(pars.Name + ".p");
                    if (string.IsNullOrEmpty(fullFilePath))
                        fullFilePath = ProEnvironment.Current.FindFirstFileInEnv(pars.Name + ".w");
                } else
                    fullFilePath = ProEnvironment.Current.FindFirstFileInEnv(pars.Name);
            }

            // to code explorer
            ParsedExplorerItemsList.Add(new CodeExplorerItem {
                DisplayText = pars.Name,
                Branch = CodeExplorerBranch.Run,
                IconType = CodeExplorerIconType.RunExternal,
                IsNotBlock = true,
                Flag = AddExternalFlag((pars.IsEvaluateValue ? CodeExplorerFlag.Uncertain : 0) | (pars.HasPersistent ? CodeExplorerFlag.LoadPersistent : 0) | ((pars.HasPersistent && string.IsNullOrEmpty(fullFilePath)) ? CodeExplorerFlag.NotFound : 0)),
                DocumentOwner = pars.FilePath,
                GoToLine = pars.Line,
                GoToColumn = pars.Column,
                SubString = SetExternalInclude(pars.OwnerName)
            });

            // if the run is PERSISTENT, we need to load the functions/proc of the program
            if (pars.HasPersistent && !string.IsNullOrEmpty(fullFilePath)) {

                // ensure to not parse the same file twice in a parser session!
                if (_parsedFiles.Contains(fullFilePath))
                    return;
                _parsedFiles.Add(fullFilePath);

                LoadProcPersistent(fullFilePath, pars.OwnerName, true);
            }
        }

        /// <summary>
        /// Dynamic-function
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedFunctionCall pars) {
            // To code explorer
            ParsedExplorerItemsList.Add(new CodeExplorerItem {
                DisplayText = pars.Name,
                Branch = CodeExplorerBranch.DynamicFunctionCall,
                IconType = pars.ExternalCall ? CodeExplorerIconType.FunctionCallExternal : CodeExplorerIconType.FunctionCallInternal,
                IsNotBlock = true,
                Flag = AddExternalFlag((CodeExplorerFlag)0),
                DocumentOwner = pars.FilePath,
                GoToLine = pars.Line,
                GoToColumn = pars.Column,
                SubString = SetExternalInclude(pars.OwnerName)
            });
        }

        /// <summary>
        /// Tables used in the program
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedFoundTableUse pars) {
            bool missingDbName = pars.Name.IndexOf('.') < 0;
            var name = pars.Name.Split('.');

            // to code explorer
            ParsedExplorerItemsList.Add(new CodeExplorerItem {
                DisplayText = missingDbName ? pars.Name : name[1],
                Branch = pars.IsTempTable ? CodeExplorerBranch.TempTableUsed : CodeExplorerBranch.TableUsed,
                IconType = pars.IsTempTable ? CodeExplorerIconType.TempTable : CodeExplorerIconType.Table,
                Flag = AddExternalFlag((missingDbName && !pars.IsTempTable) ? CodeExplorerFlag.MissingDbName : 0),
                IsNotBlock = true,
                DocumentOwner = pars.FilePath,
                GoToLine = pars.Line,
                GoToColumn = pars.Column,
                SubString = SetExternalInclude(null)
            });
        }

        /// <summary>
        /// Include files
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedIncludeFile pars) {

            // try to find the file in the propath
            var fullFilePath = ProEnvironment.Current.FindFirstFileInPropath(pars.Name);

            // To code explorer
            ParsedExplorerItemsList.Add(new CodeExplorerItem {
                DisplayText = pars.Name,
                Branch = CodeExplorerBranch.Include,
                IsNotBlock = true,
                Flag = AddExternalFlag(string.IsNullOrEmpty(fullFilePath) ? CodeExplorerFlag.NotFound : 0),
                DocumentOwner = pars.FilePath,
                GoToLine = pars.Line,
                GoToColumn = pars.Column,
                SubString = SetExternalInclude(null)
            });

            // Parse the include file ?
            if (string.IsNullOrEmpty(fullFilePath)) return;

            // ensure to not parse the same file twice in a parser session!
            if (_parsedFiles.Contains(fullFilePath))
                return;
            _parsedFiles.Add(fullFilePath);

            ParserVisitor parserVisitor = ParseFile(fullFilePath, pars.OwnerName);
            var parserItemList = parserVisitor.ParsedItemsList.ToList();

            // correct the line number of each parsed element, so we can filter the items correctly in the completion list
            parserItemList.ForEach(data => { if (data.FromParser) data.ParsedItem.IncludeLine = pars.Line; });

            // add info from the parser
            ParsedItemsList.AddRange(parserItemList);
            if (Config.Instance.CodeExplorerDisplayExternalItems)
                ParsedExplorerItemsList.AddRange(parserVisitor.ParsedExplorerItemsList.ToList());

            // fill the defined procedures dictionnary
            foreach (var definedProcedure in parserVisitor.DefinedProcedures.Where(definedProcedure => !DefinedProcedures.Contains(definedProcedure))) {
                DefinedProcedures.Add(definedProcedure);
            }
        }

        /// <summary>
        /// Main block, definitions block...
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedBlock pars) {
            // to code explorer
            ParsedExplorerItemsList.Add(new CodeExplorerItem {
                DisplayText = pars.Name,
                Branch = pars.Branch,
                IconType = pars.IconIconType,
                IsRoot = pars.IsRoot,
                Flag = AddExternalFlag((CodeExplorerFlag)0),
                DocumentOwner = pars.FilePath,
                GoToLine = pars.Line,
                GoToColumn = pars.Column,
                SubString = SetExternalInclude(null)
            });
        }

        /// <summary>
        /// ON events
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedOnEvent pars) {
            // check lenght of block
            pars.TooLongForAppbuilder = HasTooMuchChar(pars.Line, pars.EndLine);

            // To code explorer
            ParsedExplorerItemsList.Add(new CodeExplorerItem {
                DisplayText = string.Join(" ", pars.On.ToUpper(), pars.Name),
                Branch = CodeExplorerBranch.OnEvent,
                Flag = AddExternalFlag(pars.TooLongForAppbuilder ? CodeExplorerFlag.IsTooLong : 0),
                DocumentOwner = pars.FilePath,
                GoToLine = pars.Line,
                GoToColumn = pars.Column,
                SubString = SetExternalInclude(pars.TooLongForAppbuilder ? BlockTooLongString + " (+" + NbExtraCharBetweenLines(pars.Line, pars.EndLine) + ")" : null)
            });
        }

        /// <summary>
        /// Functions
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedFunction pars) {
            // check lenght of block
            pars.TooLongForAppbuilder = HasTooMuchChar(pars.Line, pars.EndLine);

            // to code explorer
            ParsedExplorerItemsList.Add(new CodeExplorerItem {
                DisplayText = pars.Name,
                Branch = CodeExplorerBranch.Function,
                Flag = AddExternalFlag(pars.TooLongForAppbuilder ? CodeExplorerFlag.IsTooLong : 0),
                DocumentOwner = pars.FilePath,
                GoToLine = pars.Line,
                GoToColumn = pars.Column,
                SubString = SetExternalInclude(pars.TooLongForAppbuilder ? BlockTooLongString + " (+" + NbExtraCharBetweenLines(pars.Line, pars.EndLine) + ")" : null)
            });

            // to completion data
            pars.ReturnType = ParserHandler.ConvertStringToParsedPrimitiveType(pars.ParsedReturnType, false);
            ParsedItemsList.Add(new CompletionData {
                DisplayText = pars.Name,
                Type = CompletionType.Function,
                SubString = pars.ReturnType.ToString(),
                Flag = AddExternalFlag((pars.IsPrivate ? ParseFlag.Private : 0) | (pars.IsExtended ? ParseFlag.Extent : 0)),
                Ranking = ParserHandler.FindRankingOfParsedItem(pars.Name),
                ParsedItem = pars,
                FromParser = true
            });
        }

        /// <summary>
        /// Procedures
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedProcedure pars) {
            // check lenght of block
            pars.TooLongForAppbuilder = HasTooMuchChar(pars.Line, pars.EndLine);

            // fill dictionnary containing the name of all procedures defined
            if (!DefinedProcedures.Contains(pars.Name))
                DefinedProcedures.Add(pars.Name);

            // to code explorer
            ParsedExplorerItemsList.Add(new CodeExplorerItem {
                DisplayText = pars.Name,
                Branch = CodeExplorerBranch.Procedure,
                Flag = AddExternalFlag((pars.IsPrivate ? CodeExplorerFlag.Private : 0) | (pars.TooLongForAppbuilder ? CodeExplorerFlag.IsTooLong : 0)),
                DocumentOwner = pars.FilePath,
                GoToLine = pars.Line,
                GoToColumn = pars.Column,
                SubString = SetExternalInclude(pars.TooLongForAppbuilder ? BlockTooLongString + " (+" + NbExtraCharBetweenLines(pars.Line, pars.EndLine) + ")" : null)
            });

            // to completion data
            ParsedItemsList.Add(new CompletionData {
                DisplayText = pars.Name,
                Type = CompletionType.Procedure,
                SubString = !_isBaseFile ? _currentParsedFile : string.Empty,
                Flag = AddExternalFlag((pars.IsExternal ? ParseFlag.ExternalProc : 0) | (pars.IsPrivate ? ParseFlag.Private : 0)),
                Ranking = ParserHandler.FindRankingOfParsedItem(pars.Name),
                ParsedItem = pars,
                FromParser = true
            });
        }

        /// <summary>
        /// Preprocessed variables
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedPreProc pars) {
            // to completion data
            ParsedItemsList.Add(new CompletionData {
                DisplayText = "&" + pars.Name,
                Type = CompletionType.Preprocessed,
                SubString = !_isBaseFile ? _currentParsedFile : string.Empty,
                Flag = AddExternalFlag(pars.Scope == ParsedScope.File ? ParseFlag.FileScope : ParseFlag.LocalScope),
                Ranking = ParserHandler.FindRankingOfParsedItem(pars.Name),
                ParsedItem = pars,
                FromParser = true
            });
        }

        /// <summary>
        /// Labels
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedLabel pars) {

            if (!_isBaseFile) return;

            // find the end line of the labelled block
            var line = pars.Line + 1;
            var depth = (_lineInfo.ContainsKey(pars.Line)) ? _lineInfo[pars.Line].BlockDepth : 0;
            bool wentIntoBlock = false;
            while (_lineInfo.ContainsKey(line)) {
                if (!wentIntoBlock && _lineInfo[line].BlockDepth > depth) {
                    wentIntoBlock = true;
                    depth = _lineInfo[line].BlockDepth;
                } else if (wentIntoBlock && _lineInfo[line].BlockDepth < depth)
                    break;
                line++;
            }
            pars.UndefinedLine = line;

            // to completion data
            ParsedItemsList.Add(new CompletionData {
                DisplayText = pars.Name,
                Type = CompletionType.Label,
                Flag = 0,
                Ranking = ParserHandler.FindRankingOfParsedItem(pars.Name),
                ParsedItem = pars,
                FromParser = true
            });
        }

        /// <summary>
        /// Defined variables
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedDefine pars) {
            // set flags
            var flag = pars.Scope == ParsedScope.File ? ParseFlag.FileScope : ParseFlag.LocalScope;
            if (pars.Type == ParseDefineType.Parameter) flag = flag | ParseFlag.Parameter;
            if (pars.IsExtended) flag = flag | ParseFlag.Extent;

            // find primitive type
            var hasPrimitive = !string.IsNullOrEmpty(pars.TempPrimitiveType);
            if (hasPrimitive)
                pars.PrimitiveType = ParserHandler.ConvertStringToParsedPrimitiveType(pars.TempPrimitiveType, pars.AsLike == ParsedAsLike.Like);

            // which completionData type is it?
            CompletionType type;
            string subString;
            // special case for buffers, they go into the temptable or table section
            if (pars.PrimitiveType == ParsedPrimitiveType.Buffer) {
                flag = flag | ParseFlag.Buffer;
                subString = "?";
                type = CompletionType.TempTable;

                // find the table or temp table that the buffer is FOR
                var foundTable = ParserHandler.FindAnyTableByName(pars.BufferFor);
                if (foundTable != null) {
                    subString = foundTable.Name;
                    type = foundTable.IsTempTable ? CompletionType.TempTable : CompletionType.Table;

                    // To code explorer, list buffers and associated tables
                    ParsedExplorerItemsList.Add(new CodeExplorerItem {
                        DisplayText = foundTable.Name,
                        Branch = foundTable.IsTempTable ? CodeExplorerBranch.TempTableUsed : CodeExplorerBranch.TableUsed,
                        IconType = foundTable.IsTempTable ? CodeExplorerIconType.TempTable : CodeExplorerIconType.Table,
                        Flag = AddExternalFlag(((!pars.BufferFor.Contains(".") && !foundTable.IsTempTable) ?  CodeExplorerFlag.MissingDbName : 0) | CodeExplorerFlag.Buffer),
                        IsNotBlock = true,
                        DocumentOwner = pars.FilePath,
                        GoToLine = pars.Line,
                        GoToColumn = pars.Column,
                        SubString = SetExternalInclude(null)
                    });
                }

            } else {
                // match type for everything else
                subString = hasPrimitive ? pars.PrimitiveType.ToString() : pars.Type.ToString();
                switch (pars.Type) {
                    case ParseDefineType.Parameter:
                        type = CompletionType.VariablePrimitive;

                        // To code explorer, program parameters
                        if (_isBaseFile && pars.Scope == ParsedScope.File)
                            ParsedExplorerItemsList.Add(new CodeExplorerItem {
                                DisplayText = pars.Name,
                                Branch = CodeExplorerBranch.ProgramParameter,
                                IconType = CodeExplorerIconType.Parameter,
                                IsNotBlock = true,
                                Flag = AddExternalFlag((CodeExplorerFlag)0),
                                DocumentOwner = pars.FilePath,
                                GoToLine = pars.Line,
                                GoToColumn = pars.Column,
                                SubString = subString
                            });
                        break;
                    case ParseDefineType.Variable:
                        if (!string.IsNullOrEmpty(pars.ViewAs))
                            type = CompletionType.Widget;
                        else if ((int) pars.PrimitiveType < 30)
                            type = CompletionType.VariablePrimitive;
                        else
                            type = CompletionType.VariableComplex;
                        break;
                    case ParseDefineType.Button:
                    case ParseDefineType.Browse:
                    case ParseDefineType.Frame:
                    case ParseDefineType.Image:
                    case ParseDefineType.SubMenu:
                    case ParseDefineType.Menu:
                    case ParseDefineType.Rectangle:
                        type = CompletionType.Widget;
                        break;
                    default:
                        type = CompletionType.VariableComplex;
                        break;
                }
            }

            // To explorer code for browse
            if (pars.Type == ParseDefineType.Browse) {
                ParsedExplorerItemsList.Add(new CodeExplorerItem {
                    DisplayText = pars.Name,
                    Branch = CodeExplorerBranch.Browse,
                    Flag = AddExternalFlag((CodeExplorerFlag)0),
                    DocumentOwner = pars.FilePath,
                    GoToLine = pars.Line,
                    GoToColumn = pars.Column,
                    SubString = SetExternalInclude(null)
                });
            }

            // to completion data
            ParsedItemsList.Add(new CompletionData {
                DisplayText = pars.Name,
                Type = type,
                SubString = subString,
                Flag = AddExternalFlag(SetFlags(flag, pars.LcFlagString)),
                Ranking = ParserHandler.FindRankingOfParsedItem(pars.Name),
                ParsedItem = pars,
                FromParser = true
            });
        }

        /// <summary>
        /// Defined Temptables
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedTable pars) {
            string subStr = "";

            // find all primitive types
            foreach (var parsedField in pars.Fields)
                parsedField.Type = ParserHandler.ConvertStringToParsedPrimitiveType(parsedField.TempType, parsedField.AsLike == ParsedAsLike.Like);

            // temp table is LIKE another table? copy fields
            if (!string.IsNullOrEmpty(pars.LcLikeTable)) {
                var foundTable = ParserHandler.FindAnyTableByName(pars.LcLikeTable);
                if (foundTable != null) {
                    // add the fields of the found table (minus the primary information)
                    subStr = @"Like " + foundTable.Name;

                    // handles the use-index, for now only add the isPrimary flag to the field...
                    if (!string.IsNullOrEmpty(pars.UseIndex)) {
                        foreach (var field in foundTable.Fields) {
                            pars.Fields.Add(new ParsedField(field.Name, "", field.Format, field.Order, field.Flag.HasFlag(ParsedFieldFlag.Mandatory) ? ParsedFieldFlag.Mandatory : 0, field.InitialValue, field.Description, field.AsLike) {
                                Type = field.Type
                            });
                        }
                        foreach (var index in pars.UseIndex.Split(',')) {
                            // we found a primary index
                            var foundIndex = foundTable.Indexes.Find(index2 => index2.Name.EqualsCi(index.Replace("!", "")));
                            // if the index is a primary
                            if (foundIndex != null && (foundIndex.Flag.HasFlag(ParsedIndexFlag.Primary) || index.ContainsFast("!")))
                                foreach (var fieldName in foundIndex.FieldsList) {
                                    // then the field is primary
                                    var foundfield = pars.Fields.Find(field => field.Name.EqualsCi(fieldName.Replace("+", "").Replace("-", "")));
                                    if (foundfield != null) foundfield.Flag = foundfield.Flag | ParsedFieldFlag.Primary;
                                }
                        }
                    } else {
                        // if there is no "use index", the tt uses the same index as the original table
                        pars.Fields.AddRange(foundTable.Fields.ToList());
                    }
                } else {
                    subStr = "Like ??";
                }
            }

            ParsedItemsList.Add(new CompletionData {
                DisplayText = pars.Name,
                Type = CompletionType.TempTable,
                SubString = subStr,
                Flag = AddExternalFlag(SetFlags(0, pars.LcFlagString)),
                Ranking = ParserHandler.FindRankingOfParsedItem(pars.Name),
                ParsedItem = pars,
                FromParser = true
            });
        }

        #endregion

        #region helper

        /// <summary>
        /// Parses a file.
        /// Remarks : it doesn't parse the document against known words since this is only useful for
        /// the CURRENT document and not for the others
        /// </summary>
        public static ParserVisitor ParseFile(string fileName, string ownerName) {
            ParserVisitor parserVisitor;
            
            // did we already parsed this file in a previous parse session?
            if (ParserHandler.SavedParserVisitors.ContainsKey(fileName)) {
                parserVisitor = ParserHandler.SavedParserVisitors[fileName];
            } else {
                // Parse it
                var ablParser = new Parser(File.ReadAllText(fileName, TextEncodingDetect.GetFileEncoding(fileName)), fileName, ownerName);

                parserVisitor = new ParserVisitor(false, Path.GetFileName(fileName), ablParser.GetLineInfo);
                ablParser.Accept(parserVisitor);

                // save it for future uses
                ParserHandler.SavedParserVisitors.Add(fileName, parserVisitor);
            }

            return parserVisitor;
        }

        /// <summary>
        /// Parses given file and load its function + procedures has persistent so they are
        /// accessible from the autocompletion list
        /// Set runPersistentIsInFile = false (default) to add items only to the completion list,
        /// set to true to also display proc/func in the code explorer tree if asked
        /// </summary>
        public void LoadProcPersistent(string fileName, string ownerName, bool runPersistentIsInFile = false) {

            ParserVisitor parserVisitor = ParseFile(fileName, ownerName);

            // add info to the completion list
            var listToAdd = parserVisitor.ParsedItemsList.Where(data => (data.Type == CompletionType.Function || data.Type == CompletionType.Procedure)).ToList();
            foreach (var completionData in listToAdd) {
                completionData.Flag = completionData.Flag | ParseFlag.Persistent;
            }
            ParsedItemsList.AddRange(listToAdd);

            // add info to the code explorer
            if (runPersistentIsInFile && Config.Instance.CodeExplorerDisplayExternalItems) {
                var listExpToAdd = parserVisitor.ParsedExplorerItemsList.Where(item => item.Branch == CodeExplorerBranch.Procedure || item.Branch == CodeExplorerBranch.Function).ToList();
                foreach (var codeExplorerItem in listExpToAdd) {
                    codeExplorerItem.Flag = codeExplorerItem.Flag | CodeExplorerFlag.Persistent;
                }
                ParsedExplorerItemsList.AddRange(listExpToAdd);

            }
        }

        /// <summary>
        /// Adds the "external" flag if needed
        /// </summary>
        private ParseFlag AddExternalFlag(ParseFlag flag) {
            if (_isBaseFile) return flag;
            return flag | ParseFlag.External;
        }

        private CodeExplorerFlag AddExternalFlag(CodeExplorerFlag flag) {
            if (_isBaseFile) return flag;
            return flag | CodeExplorerFlag.External;
        }

        private string SetExternalInclude(string subString) {
            return _isBaseFile ? subString : subString ?? _currentParsedFile;
        }

        /// <summary>
        /// Determines flags
        /// </summary>
        private static ParseFlag SetFlags(ParseFlag flag, string lcFlagString) {
            if (lcFlagString.Contains("global")) flag = flag | ParseFlag.Global;
            if (lcFlagString.Contains("shared")) flag = flag | ParseFlag.Shared;
            if (lcFlagString.Contains("private")) flag = flag | ParseFlag.Private;
            if (lcFlagString.Contains("new")) flag = flag | ParseFlag.Private;
            return flag;
        }

        /// <summary>
        /// To test if a proc or a function has too much char in it, because this would make the
        /// appbuilder unable to open it correctly
        /// </summary>
        private bool HasTooMuchChar(int startLine, int endLine) {
            if (!_isBaseFile) return false;
            return NbExtraCharBetweenLines(startLine, endLine) > 0;
        }

        /// <summary>
        /// returns the number of chars between two lines in the current document
        /// </summary>
        private static int NbExtraCharBetweenLines(int startLine, int endLine) {
            return (Npp.StartBytePosOfLine(endLine) - Npp.StartBytePosOfLine(startLine)) - Config.Instance.GlobalMaxNbCharInBlock;
        }

        #endregion

    }
}
