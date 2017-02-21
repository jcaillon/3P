#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Linq;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletionFeature;
using _3PA.MainFeatures.CodeExplorer;
using _3PA.MainFeatures.Pro;

namespace _3PA.MainFeatures.Parser {

    /// <summary>
    /// This class sustains the autocompletion list AND the code explorer list
    /// by visiting the parser and creating new completionData
    /// </summary>
    internal class ParserVisitor : IParserVisitor {

        #region static

        /// <summary>
        /// We keep tracks of the parsed files, to avoid parsing the same file twice
        /// </summary>
        private static HashSet<string> _runPersistentFiles = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Instead of parsing the persistent files each time we store the results of the parsing to use them when we need it
        /// </summary>
        private static Dictionary<string, ParserVisitor> _savedPersistent = new Dictionary<string, ParserVisitor>();

        #endregion

        #region private fields

        private const string BlockTooLongString = "> Appbuilder max length";

        private bool _prototypeAdded;

        /// <summary>
        /// Are we currently visiting the current file opened in npp or
        /// is it a include?
        /// </summary>
        private bool _isBaseFile;

        /// <summary>
        /// Stores the file path of the file currently visited/parsed
        /// </summary>
        private string _currentParsedFilePath;

        /// <summary>
        /// this dictionary is used to reference the procedures defined
        /// in the program we are parsing, dictionary is faster that list when it comes to
        /// test if a procedure/function exists in the program
        /// </summary>
        private HashSet<string> _definedProcedures = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// contains the list of items that depend on the current file, that list
        /// is updated by the parser's visitor class
        /// </summary>
        private List<CompletionItem> _parsedCompletionItemsList = new List<CompletionItem>();

        /// <summary>
        /// Contains the list of explorer items for the current file, updated by the parser's visitor class
        /// </summary>
        private List<CodeExplorerItem> _parsedExplorerItemsList = new List<CodeExplorerItem>();

        /// <summary>
        /// Reference of the parser being visited
        /// </summary>
        private Parser _parser;

        #endregion

        #region public accessors

        /// <summary>
        /// Contains the list of explorer items for the current file, updated by the parser's visitor class
        /// </summary>
        public List<CodeExplorerItem> ParsedExplorerItemsList {
            get { return _parsedExplorerItemsList; }
        }

        /// <summary>
        /// contains the list of items that depend on the current file, that list
        /// is updated by the parser's visitor class
        /// </summary>
        public List<CompletionItem> ParsedCompletionItemsList {
            get { return _parsedCompletionItemsList; }
        }

        /// <summary>
        /// this dictionary is used to reference the procedures defined
        /// in the program we are parsing, dictionary is faster that list when it comes to
        /// test if a procedure/function exists in the program
        /// </summary>
        public HashSet<string> DefinedProcedures {
            get { return _definedProcedures; }
            set { _definedProcedures = value; }
        }

        #endregion

        #region constructor

        public ParserVisitor() {}

        /// <summary>
        /// Constructor
        /// </summary>
        public ParserVisitor(bool isBaseFile) {
            _isBaseFile = isBaseFile;

            if (_isBaseFile) {
                // resets the parsed files for this parsing session
                _runPersistentFiles.Clear();
            }
        }

        #endregion

        #region visit implementation

        /// <summary>
        /// To be executed before the visit starts
        /// </summary>
        public void PreVisit(Parser parser) {
            _parser = parser;
            _currentParsedFilePath = parser.FilePathBeingParsed;

            // if this document is in the Saved parsed visitors, we remove it now and we will add it back when it is parsed
            if (_isBaseFile) {
                if (_savedPersistent.ContainsKey(_currentParsedFilePath))
                    _savedPersistent.Remove(_currentParsedFilePath);
            }
        }

        /// <summary>
        /// To be executed after the visit ends
        /// </summary>
        public void PostVisit() {
            if (_isBaseFile) {
                // correct the internal/external type of run statements :
                foreach (var item in _parsedExplorerItemsList.Where(item => item.Branch == CodeExplorerBranch.RunExternal && _definedProcedures.Contains(item.DisplayText))) {
                    item.Branch = CodeExplorerBranch.Run;
                }
            }

            // save the info for uses in an another file, where this file is run in persistent
            if (!_savedPersistent.ContainsKey(_currentParsedFilePath))
                _savedPersistent.Add(_currentParsedFilePath, this);
            else
                _savedPersistent[_currentParsedFilePath] = this;

            // lose parser reference
            _parser = null;
        }

        /// <summary>
        /// Run statement,
        /// a second pass will be done after the visit is over to determine if a run is
        /// internal or external (calling internal proc or programs)
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedRun pars) {

            // try to find the file in the propath
            if (pars.Flags.HasFlag(ParseFlag.Persistent) && !pars.Flags.HasFlag(ParseFlag.Uncertain)) {
                string procName = pars.Name;
                string fullFilePath;
                if (!procName.EndsWith(".p") && !procName.EndsWith(".w")) {
                    fullFilePath = ProEnvironment.Current.FindFirstFileInEnv(pars.Name + ".p");
                    if (string.IsNullOrEmpty(fullFilePath))
                        fullFilePath = ProEnvironment.Current.FindFirstFileInEnv(pars.Name + ".w");
                } else
                    fullFilePath = ProEnvironment.Current.FindFirstFileInEnv(pars.Name);

                if (string.IsNullOrEmpty(fullFilePath))
                    pars.Flags |= ParseFlag.NotFound;
                else {
                    // if the run is PERSISTENT, we need to load the functions/proc of the program
                    // ensure to not parse the same file twice in a parser session!
                    if (!_runPersistentFiles.Contains(fullFilePath)) {
                        _runPersistentFiles.Add(fullFilePath);
                        LoadProcPersistent(fullFilePath, pars.Scope);
                    }
                }
            }

            // to code explorer
            _parsedExplorerItemsList.Add(new CodeExplorerItem {
                DisplayText = pars.Name,
                Branch = CodeExplorerBranch.RunExternal,
                IsNotBlock = true,
                Flags = pars.Flags,
                DocumentOwner = pars.FilePath,
                GoToLine = pars.Line,
                GoToColumn = pars.Column,
                SubString = pars.Scope.Name
            });

        }

        /// <summary>
        /// Dynamic-function
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedFunctionCall pars) {
            // To code explorer
            _parsedExplorerItemsList.Add(new CodeExplorerItem {
                DisplayText = pars.Name,
                Branch = pars.ExternalCall ? CodeExplorerBranch.DynamicFunctionCallExternal : (pars.StaticCall ? CodeExplorerBranch.StaticFunctionCall : CodeExplorerBranch.DynamicFunctionCall),
                IsNotBlock = true,
                Flags = pars.Flags,
                DocumentOwner = pars.FilePath,
                GoToLine = pars.Line,
                GoToColumn = pars.Column,
                SubString = pars.Scope.Name
            });
        }

        /// <summary>
        /// Tables used in the program
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedFoundTableUse pars) {
            bool missingDbName = pars.Name.IndexOf('.') < 0;

            // to code explorer
            _parsedExplorerItemsList.Add(new CodeExplorerItem {
                DisplayText = missingDbName ? pars.Name : pars.Name.Split('.')[1],
                Branch = pars.IsTempTable ? CodeExplorerBranch.TempTableUsed : CodeExplorerBranch.TableUsed,
                IconType = pars.IsTempTable ? CodeExplorerIconType.TempTable : CodeExplorerIconType.Table,
                Flags = (missingDbName && !pars.IsTempTable ? ParseFlag.MissingDbName : 0) | pars.Flags,
                IsNotBlock = true,
                DocumentOwner = pars.FilePath,
                GoToLine = pars.Line,
                GoToColumn = pars.Column,
                SubString = null
            });
        }

        public void Visit(ParsedEvent pars) {
            // to code explorer
            _parsedExplorerItemsList.Add(new CodeExplorerItem {
                DisplayText = pars.Name,
                Branch = pars.Type == ParsedEventType.Subscribe ? CodeExplorerBranch.Subscribe : (pars.Type == ParsedEventType.Publish ? CodeExplorerBranch.Publish : CodeExplorerBranch.Unsubscribe),
                Flags = pars.Flags,
                IsNotBlock = true,
                DocumentOwner = pars.FilePath,
                GoToLine = pars.Line,
                GoToColumn = pars.Column,
                SubString = null
            });
        }

        /// <summary>
        /// Include files
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedIncludeFile pars) {
            // To code explorer
            _parsedExplorerItemsList.Add(new CodeExplorerItem {
                DisplayText = pars.Name,
                Branch = CodeExplorerBranch.Include,
                IsNotBlock = true,
                Flags = pars.Flags,
                DocumentOwner = pars.FilePath,
                GoToLine = pars.Line,
                GoToColumn = pars.Column,
                SubString = null
            });
        }

        /// <summary>
        /// Root file block
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedFile pars) {
            // to code explorer
            _parsedExplorerItemsList.Add(new CodeExplorerItem {
                DisplayText = pars.Name,
                Branch = CodeExplorerBranch.Root,
                IconType = CodeExplorerIconType.BranchIcon,
                Flags = 0,
                DocumentOwner = pars.FilePath,
                GoToLine = pars.Line,
                GoToColumn = pars.Column,
                SubString = null
            });
        }

        /// <summary>
        /// Main block, definitions block...
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedPreProcBlock pars) {

            if (pars.Flags.HasFlag(ParseFlag.FromInclude))
                return;

            // add the prototype block only once, for the first proto
            if (pars.Type == ParsedPreProcBlockType.FunctionForward) {
                if (_prototypeAdded) return;
                _prototypeAdded = true;
            }

            CodeExplorerIconType type;
            switch (pars.Type) {
                case ParsedPreProcBlockType.MainBlock:
                    type = CodeExplorerIconType.BranchIcon;
                    break;
                case ParsedPreProcBlockType.FunctionForward:
                    type = CodeExplorerIconType.Prototype;
                    break;
                case ParsedPreProcBlockType.Definitions:
                    type = CodeExplorerIconType.DefinitionBlock;
                    break;
                case ParsedPreProcBlockType.UibPreprocessorBlock:
                    type = CodeExplorerIconType.PreprocessorBlock;
                    break;
                case ParsedPreProcBlockType.Xftr:
                    type = CodeExplorerIconType.XtfrBlock;
                    break;
                case ParsedPreProcBlockType.ProcedureSettings:
                    type = CodeExplorerIconType.SettingsBlock;
                    break;
                case ParsedPreProcBlockType.CreateWindow:
                    type = CodeExplorerIconType.CreateWindowBlock;
                    break;
                case ParsedPreProcBlockType.RunTimeAttributes:
                    type = CodeExplorerIconType.RuntimeBlock;
                    break;
                default:
                    return;
            }

            // to code explorer
            _parsedExplorerItemsList.Add(new CodeExplorerItem {
                DisplayText = pars.Name,
                Branch = pars.Type == ParsedPreProcBlockType.MainBlock ? CodeExplorerBranch.MainBlock : CodeExplorerBranch.Block,
                IconType = type,
                Flags = pars.Flags,
                DocumentOwner = pars.FilePath,
                GoToLine = pars.Line,
                GoToColumn = pars.Column,
                SubString = null
            });

        }

        /// <summary>
        /// ON events
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedOnStatement pars) {
            // check length of block
            CheckForTooMuchChar(pars);

            // To code explorer
            _parsedExplorerItemsList.Add(new CodeExplorerItem {
                DisplayText = pars.Name,
                Branch = CodeExplorerBranch.OnEvent,
                Flags = pars.Flags,
                DocumentOwner = pars.FilePath,
                GoToLine = pars.Line,
                GoToColumn = pars.Column,
                SubString = pars.TooLongForAppbuilder ? BlockTooLongString + " (+" + NbExtraCharBetweenLines(pars.Line, pars.EndBlockLine) + ")" : null
            });
        }

        public void Visit(ParsedImplementation pars) {
            // check length of block
            CheckForTooMuchChar(pars);

            // to code explorer
            _parsedExplorerItemsList.Add(new CodeExplorerItem {
                DisplayText = pars.Name,
                Branch = CodeExplorerBranch.Function,
                Flags = pars.Flags,
                DocumentOwner = pars.FilePath,
                GoToLine = pars.Line,
                GoToColumn = pars.Column,
                SubString = pars.TooLongForAppbuilder ? BlockTooLongString + " (+" + NbExtraCharBetweenLines(pars.Line, pars.EndBlockLine) + ")" : null
            });

            // to completion data
            pars.ReturnType = ConvertStringToParsedPrimitiveType(pars.ParsedReturnType, false);
            _parsedCompletionItemsList.Add(new CompletionItem {
                DisplayText = pars.Name,
                Type = CompletionType.Function,
                SubString = pars.ReturnType.ToString(),
                Flags = pars.Flags,
                Ranking = AutoCompletion.FindRankingOfParsedItem(pars.Name),
                ParsedItem = pars,
                FromParser = true
            });
        }

        public void Visit(ParsedPrototype pars) {
            // only visit IN prototypes
            if (pars.SimpleForward)
                return;

            // to code explorer
            _parsedExplorerItemsList.Add(new CodeExplorerItem {
                DisplayText = pars.Name,
                Branch = CodeExplorerBranch.Function,
                Flags = pars.Flags,
                DocumentOwner = pars.FilePath,
                GoToLine = pars.Line,
                GoToColumn = pars.Column,
                SubString = null
            });

            // to completion data
            pars.ReturnType = ConvertStringToParsedPrimitiveType(pars.ParsedReturnType, false);
            _parsedCompletionItemsList.Add(new CompletionItem {
                DisplayText = pars.Name,
                Type = CompletionType.Function,
                Flags = pars.Flags,
                Ranking = AutoCompletion.FindRankingOfParsedItem(pars.Name),
                ParsedItem = pars,
                FromParser = true,
                SubString = pars.ReturnType.ToString(),
            });
        }

        /// <summary>
        /// Procedures
        /// </summary>
        public void Visit(ParsedProcedure pars) {
            // check lenght of block
            CheckForTooMuchChar(pars);

            // fill dictionary containing the name of all procedures defined
            if (!_definedProcedures.Contains(pars.Name))
                _definedProcedures.Add(pars.Name);

            // to code explorer
            _parsedExplorerItemsList.Add(new CodeExplorerItem {
                DisplayText = pars.Name,
                Branch = pars.Flags.HasFlag(ParseFlag.External) ? CodeExplorerBranch.ExternalProcedure : CodeExplorerBranch.Procedure,
                Flags = pars.Flags,
                DocumentOwner = pars.FilePath,
                GoToLine = pars.Line,
                GoToColumn = pars.Column,
                SubString = pars.Flags.HasFlag(ParseFlag.External) ? pars.ExternalDllName : pars.TooLongForAppbuilder ? BlockTooLongString + " (+" + NbExtraCharBetweenLines(pars.Line, pars.EndBlockLine) + ")" : null
            });

            // to completion data
            _parsedCompletionItemsList.Add(new CompletionItem {
                DisplayText = pars.Name,
                Type = CompletionType.Procedure,
                ItemImage = pars.Flags.HasFlag(ParseFlag.External) ? Utils.GetImageFromStr(CodeExplorerBranch.ExternalProcedure.ToString()) : null,
                Flags = pars.Flags,
                Ranking = AutoCompletion.FindRankingOfParsedItem(pars.Name),
                ParsedItem = pars,
                FromParser = true,
                SubString = pars.Flags.HasFlag(ParseFlag.External) ? pars.ExternalDllName : null,
            });
        }

        /// <summary>
        /// Preprocessed variables
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedPreProcVariable pars) {
            if (pars.Flags.HasFlag(ParseFlag.Global) || !pars.Flags.HasFlag(ParseFlag.FromInclude))
                // to completion data
                _parsedCompletionItemsList.Add(new CompletionItem {
                    DisplayText = "&" + pars.Name,
                    Type = CompletionType.Preprocessed,
                    Flags = pars.Flags,
                    Ranking = AutoCompletion.FindRankingOfParsedItem(pars.Name),
                    ParsedItem = pars,
                    FromParser = true,
                    SubString = null,
                });
        }

        /// <summary>
        /// Labels
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedLabel pars) {

            // find the end line of the labeled block
            var line = pars.Line + 1;
            var depth = _parser.LineInfo.ContainsKey(pars.Line) ? _parser.LineInfo[pars.Line].BlockDepth : 0;
            bool wentIntoBlock = false;
            while (_parser.LineInfo.ContainsKey(line)) {
                if (!wentIntoBlock && _parser.LineInfo[line].BlockDepth > depth) {
                    wentIntoBlock = true;
                    depth = _parser.LineInfo[line].BlockDepth;
                } else if (wentIntoBlock && _parser.LineInfo[line].BlockDepth < depth)
                    break;
                line++;
            }
            pars.UndefinedLine = line;

            // to completion data
            _parsedCompletionItemsList.Add(new CompletionItem {
                DisplayText = pars.Name,
                Type = CompletionType.Label,
                Flags = pars.Flags,
                Ranking = AutoCompletion.FindRankingOfParsedItem(pars.Name),
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
            pars.Flags |= pars.Scope is ParsedFile ? ParseFlag.FileScope : ParseFlag.LocalScope;
            if (pars.Type == ParseDefineType.Parameter)
                pars.Flags |= ParseFlag.Parameter;

            // find primitive type
            var hasPrimitive = !string.IsNullOrEmpty(pars.TempPrimitiveType);
            if (hasPrimitive)
                pars.PrimitiveType = ConvertStringToParsedPrimitiveType(pars.TempPrimitiveType, pars.AsLike == ParsedAsLike.Like);

            // which completionData type is it?
            CompletionType type;
            string subString;
            // special case for buffers, they go into the temptable or table section
            if (pars.PrimitiveType == ParsedPrimitiveType.Buffer) {
                pars.Flags |= ParseFlag.Buffer;
                subString = "?";
                type = CompletionType.TempTable;

                // find the table or temp table that the buffer is FOR
                var foundTable = FindAnyTableByName(pars.BufferFor);
                if (foundTable != null) {
                    subString = foundTable.Name;
                    type = foundTable.IsTempTable ? CompletionType.TempTable : CompletionType.Table;

                    // extra flags
                    pars.Flags |= !pars.BufferFor.Contains(".") && !foundTable.IsTempTable ?  ParseFlag.MissingDbName : 0;

                    // To code explorer, list buffers and associated tables
                    _parsedExplorerItemsList.Add(new CodeExplorerItem {
                        DisplayText = foundTable.Name,
                        Branch = foundTable.IsTempTable ? CodeExplorerBranch.TempTableUsed : CodeExplorerBranch.TableUsed,
                        IconType = foundTable.IsTempTable ? CodeExplorerIconType.TempTable : CodeExplorerIconType.Table,
                        Flags = pars.Flags,
                        IsNotBlock = true,
                        DocumentOwner = pars.FilePath,
                        GoToLine = pars.Line,
                        GoToColumn = pars.Column,
                        SubString = null
                    });
                }

            } else {
                // match type for everything else
                subString = hasPrimitive ? pars.PrimitiveType.ToString() : pars.Type.ToString();
                switch (pars.Type) {
                    case ParseDefineType.Parameter:
                        type = CompletionType.VariablePrimitive;

                        // To code explorer, program parameters
                        if (_isBaseFile && pars.Scope is ParsedFile)
                            _parsedExplorerItemsList.Add(new CodeExplorerItem {
                                DisplayText = pars.Name,
                                Branch = CodeExplorerBranch.ProgramParameter,
                                IconType = CodeExplorerIconType.Parameter,
                                IsNotBlock = true,
                                Flags = pars.Flags,
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
                _parsedExplorerItemsList.Add(new CodeExplorerItem {
                    DisplayText = pars.Name,
                    Branch = CodeExplorerBranch.Browse,
                    Flags = pars.Flags,
                    DocumentOwner = pars.FilePath,
                    GoToLine = pars.Line,
                    GoToColumn = pars.Column,
                    SubString = subString
                });
            }

            // to completion data
            _parsedCompletionItemsList.Add(new CompletionItem {
                DisplayText = pars.Name,
                Type = type,
                Flags = pars.Flags,
                Ranking = AutoCompletion.FindRankingOfParsedItem(pars.Name),
                ParsedItem = pars,
                FromParser = true,
                SubString = subString,
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
                parsedField.Type = ConvertStringToParsedPrimitiveType(parsedField.TempType, parsedField.AsLike == ParsedAsLike.Like);

            // temp table is LIKE another table? copy fields
            if (!string.IsNullOrEmpty(pars.LcLikeTable)) {
                var foundTable = FindAnyTableByName(pars.LcLikeTable);
                if (foundTable != null) {
                    // add the fields of the found table (minus the primary information)
                    subStr = @"Like " + foundTable.Name;
                    foreach (var field in foundTable.Fields) {
                        pars.Fields.Add(
                            new ParsedField(field.Name, "", field.Format, field.Order, 0, field.InitialValue, field.Description, field.AsLike) {
                                Type = field.Type
                            });
                    }
                    
                    // handles the use-index
                    if (!string.IsNullOrEmpty(pars.UseIndex)) {
                        // add only the indexes that are used
                        foreach (var index in pars.UseIndex.Split(',')) {
                            var foundIndex = foundTable.Indexes.Find(index2 => index2.Name.EqualsCi(index.Replace("!", "")));
                            if (foundIndex != null) {
                                pars.Indexes.Add(new ParsedIndex(foundIndex.Name, foundIndex.Flag, foundIndex.FieldsList.ToList()));
                                // if one of the index used is marked as primary
                                if (index.ContainsFast("!")) {
                                    pars.Indexes.ForEach(parsedIndex => parsedIndex.Flag &= ~ParsedIndexFlag.Primary);
                                }
                                pars.Indexes.Last().Flag |= ParsedIndexFlag.Primary;
                            }
                        }
                    } else {
                        // if there is no "use index", the tt uses the same index as the original table
                        pars.Indexes = foundTable.Indexes.ToList();
                    }
                } else {
                    subStr = "Like ??";
                }
            }

            // browse all the indexes and set the according flags to each field of the index
            foreach (var index in pars.Indexes) {
                foreach (var fieldName in index.FieldsList) {
                    var foundfield = pars.Fields.Find(field => field.Name.EqualsCi(fieldName.Substring(0, fieldName.Length - 1)));
                    if (foundfield != null) {
                        if (index.Flag.HasFlag(ParsedIndexFlag.Primary))
                            foundfield.Flags |= ParseFlag.Primary;
                        foundfield.Flags |= ParseFlag.Index;
                    }
                }
            }

            // to autocompletion
            _parsedCompletionItemsList.Add(new CompletionItem {
                DisplayText = pars.Name,
                Type = CompletionType.TempTable,
                Flags = pars.Flags,
                Ranking = AutoCompletion.FindRankingOfParsedItem(pars.Name),
                ParsedItem = pars,
                FromParser = true,
                SubString = subStr,
            });

            // to code explorer
            _parsedExplorerItemsList.Add(new CodeExplorerItem {
                DisplayText = pars.Name,
                Branch = CodeExplorerBranch.DefinedTempTable,
                Flags = pars.Flags,
                DocumentOwner = pars.FilePath,
                GoToLine = pars.Line,
                GoToColumn = pars.Column,
                SubString = subStr
            });
        }

        #endregion

        #region find table, buffer, temptable

        /// <summary>
        /// finds a ParsedTable for the input name, it can either be a database table,
        /// a temptable, or a buffer name (in which case we return the associated table)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ParsedTable FindAnyTableOrBufferByName(string name) {
            // temptable or table
            var foundTable = FindAnyTableByName(name);
            if (foundTable != null)
                return foundTable;
            // for buffer, we return the referenced temptable/table (stored in CompletionItem.SubString)
            var foundParsedItem = _parsedCompletionItemsList.Find(data => (data.Type == CompletionType.Table || data.Type == CompletionType.TempTable) && data.DisplayText.EqualsCi(name));
            return foundParsedItem != null ? FindAnyTableByName(foundParsedItem.SubString) : null;
        }

        /// <summary>
        /// Find the table referenced among database and defined temp tables; 
        /// name is the table's name (can also be BASE.TABLE)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ParsedTable FindAnyTableByName(string name) {
            if (name.CountOccurences(".") > 0) {
                var splitted = name.Split('.');
                // find db then find table
                var foundDb = DataBase.FindDatabaseByName(splitted[0]);
                return foundDb == null ? null : DataBase.FindTableByName(splitted[1], foundDb);
            }
            // search in databse then in temp tables
            var foundTable = DataBase.FindTableByName(name);
            return foundTable ?? FindTempTableByName(name);
        }

        /// <summary>
        /// Find a temptable by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private ParsedTable FindTempTableByName(string name) {
            var foundTable = _parsedCompletionItemsList.FirstOrDefault(data => data.Type == CompletionType.TempTable && data.DisplayText.EqualsCi(name));
            if (foundTable != null && foundTable.ParsedItem is ParsedTable)
                return (ParsedTable) foundTable.ParsedItem;
            return null;
        }

        #endregion

        #region find primitive type

        /// <summary>
        /// convertion
        /// </summary>
        /// <param name="str"></param>
        /// <param name="analyseLike"></param>
        /// <returns></returns>
        public ParsedPrimitiveType ConvertStringToParsedPrimitiveType(string str, bool analyseLike) {
            str = str.ToLower();
            // LIKE
            if (analyseLike)
                return FindPrimitiveTypeOfLike(str);

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
                    var token1 = str;
                    foreach (var typ in Enum.GetNames(typeof(ParsedPrimitiveType)).Where(typ => token1.Equals(typ.ToLower()))) {
                        return (ParsedPrimitiveType)Enum.Parse(typeof(ParsedPrimitiveType), typ, true);
                    }
                    break;
            }

            // try to find the complete word in abbreviations list
            var completeStr = Keywords.GetFullKeyword(str);
            if (completeStr != null)
                foreach (var typ in Enum.GetNames(typeof(ParsedPrimitiveType)).Where(typ => completeStr.ToLower().Equals(typ.ToLower()))) {
                    return (ParsedPrimitiveType)Enum.Parse(typeof(ParsedPrimitiveType), typ, true);
                }
            return ParsedPrimitiveType.Unknow;
        }

        /// <summary>
        /// Search through the available completionData to find the primitive type of a 
        /// "like xx" phrase
        /// </summary>
        /// <param name="likeStr"></param>
        /// <returns></returns>
        private ParsedPrimitiveType FindPrimitiveTypeOfLike(string likeStr) {
            // determines the format
            var nbPoints = likeStr.CountOccurences(".");
            var splitted = likeStr.Split('.');

            // if it's another var
            if (nbPoints == 0) {
                var foundVar = _parsedCompletionItemsList.Find(data =>
                    (data.Type == CompletionType.VariablePrimitive ||
                     data.Type == CompletionType.VariableComplex) && data.DisplayText.EqualsCi(likeStr));
                return foundVar != null ? ((ParsedDefine)foundVar.ParsedItem).PrimitiveType : ParsedPrimitiveType.Unknow;
            }

            var tableName = splitted[nbPoints == 2 ? 1 : 0];
            var fieldName = splitted[nbPoints == 2 ? 2 : 1];

            // Search through database
            if (DataBase.List.Count > 0) {
                ParsedDataBase foundDb = DataBase.List.First();
                if (nbPoints == 2)
                    // find database
                    foundDb = DataBase.FindDatabaseByName(splitted[0]) ?? DataBase.List.First();
                if (foundDb == null) return ParsedPrimitiveType.Unknow;

                // find table
                var foundTable = DataBase.FindTableByName(tableName, foundDb);
                if (foundTable != null) {

                    // find field
                    var foundField = DataBase.FindFieldByName(fieldName, foundTable);
                    if (foundField != null) return foundField.Type;
                }
            }

            // Search in temp tables
            if (nbPoints != 1) return ParsedPrimitiveType.Unknow;
            var foundTtable = FindAnyTableOrBufferByName(tableName);
            if (foundTtable == null) return ParsedPrimitiveType.Unknow;

            var foundTtField = foundTtable.Fields.Find(field => field.Name.EqualsCi(fieldName));
            return foundTtField == null ? ParsedPrimitiveType.Unknow : foundTtField.Type;
        }

        #endregion

        #region helper

        /// <summary>
        /// Parses given file and load its function + procedures has persistent so they are
        /// accessible from the autocompletion list
        /// Set runPersistentIsInFile = false (default) to add items only to the completion list,
        /// set to true to also display proc/func in the code explorer tree if asked
        /// </summary>
        public void LoadProcPersistent(string fileName, ParsedScopeItem scopeItem) {

            ParserVisitor parserVisitor = ParseFile(fileName, scopeItem);

            // add info to the completion list
            var listToAdd = parserVisitor._parsedCompletionItemsList.Where(data => (data.Type == CompletionType.Function || data.Type == CompletionType.Procedure)).ToList();
            foreach (var completionData in listToAdd) {
                completionData.Flags = completionData.Flags | ParseFlag.Persistent;
            }
            _parsedCompletionItemsList.AddRange(listToAdd);

            // add info to the code explorer
            if (Config.Instance.CodeExplorerDisplayExternalItems) {
                var listExpToAdd = parserVisitor._parsedExplorerItemsList.Where(item => item.Branch == CodeExplorerBranch.Procedure || item.Branch == CodeExplorerBranch.Function).ToList();
                foreach (var codeExplorerItem in listExpToAdd) {
                    codeExplorerItem.Flags = codeExplorerItem.Flags | ParseFlag.Persistent;
                }
                _parsedExplorerItemsList.AddRange(listExpToAdd);
            }
        }

        /// <summary>
        /// Returns a parserVisitor for an existing object, or it create a new one
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static ParserVisitor GetParserVisitor(string filePath) {
            return ParseFile(filePath, null);
        }

        /// <summary>
        /// Parses a file.
        /// Remarks : it doesn't parse the document against known words since this is only useful for
        /// the CURRENT document and not for the others
        /// </summary>
        private static ParserVisitor ParseFile(string filePath, ParsedScopeItem scopeItem) {
            ParserVisitor parserVisitor;

            // did we already parsed this file in a previous parse session? (if we are in CodeExplorerDisplayExternalItems mode we need to parse it again anyway)
            if (_savedPersistent.ContainsKey(filePath)) {
                parserVisitor = _savedPersistent[filePath];
            } else {
                // Parse it
                var ablParser = new Parser(Utils.ReadAllText(filePath), filePath, scopeItem, false);
                parserVisitor = new ParserVisitor(false);
                ablParser.Accept(parserVisitor);
            }

            return parserVisitor;
        }

        /// <summary>
        /// Check the parse scope has too much char to allow it to be displayed in the appbuilder
        /// </summary>
        /// <param name="pars"></param>
        private void CheckForTooMuchChar(ParsedScopeItem pars) {
            // check lenght of block
            if (!pars.Flags.HasFlag(ParseFlag.FromInclude)) {
                pars.TooLongForAppbuilder = NbExtraCharBetweenLines(pars.Line, pars.EndBlockLine) > 0;
                if (pars.TooLongForAppbuilder)
                    pars.Flags |= ParseFlag.IsTooLong;
            }
        }

        /// <summary>
        /// returns the number of chars between two lines in the current document
        /// </summary>
        private static int NbExtraCharBetweenLines(int startLine, int endLine) {
            return (Npp.StartBytePosOfLine(endLine) - Npp.StartBytePosOfLine(startLine)) - Config.Instance.GlobalMaxNbCharInBlock;
        }

        #endregion

        #region others

        /// <summary>
        /// Allows the clear the SavedParserVisitors (not static because we want to control when
        /// it can be called
        /// </summary>
        public void ClearSavedParserVisitors() {
            _savedPersistent.Clear();
        }

        #endregion

    }
}
