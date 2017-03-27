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
using YamuiFramework.Controls.YamuiList;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletionFeature;
using _3PA.MainFeatures.CodeExplorer;
using _3PA.MainFeatures.Pro;
using _3PA.NppCore;

namespace _3PA.MainFeatures.Parser {
    /// <summary>
    /// This class sustains the auto completion list AND the code explorer list
    /// by visiting the parser and creating new completionData
    /// </summary>
    internal class ParserVisitor : IParserVisitor {
        #region static

        /// <summary>
        /// We keep tracks of the parsed files, to avoid parsing the same file twice
        /// </summary>
        private static HashSet<string> RunPersistentFiles {
            get { return ParserHandler.RunPersistentFiles; }
        }

        /// <summary>
        /// Instead of parsing the persistent files each time we store the results of the parsing to use them when we need it
        /// </summary>
        private static Dictionary<string, ParserVisitor> SavedPersistent {
            get { return ParserHandler.SavedPersistent; }
        }

        #endregion

        #region private fields

        private const string BlockTooLongString = "> Appbuilder max length";

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
        /// Reference of the parser being visited
        /// </summary>
        private Parser _parser;

        #endregion

        #region Code explorer list

        /// <summary>
        /// Contains the list of explorer items for the current file, updated by the parser's visitor class
        /// </summary>
        private List<CodeItem> _parsedExplorerItemsList = new List<CodeItem>();

        private Dictionary<string, CodeItem> _nodeDictionary = new Dictionary<string, CodeItem>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// nodeId should be composed of the CodeItem.Type and CodeItem.DisplayText
        /// </summary>
        private CodeItem GetExplorerListNode(string nodeId, CodeExplorerIconType type, CodeItem parentNode = null) {
            if (_nodeDictionary.ContainsKey(nodeId)) {
                return _nodeDictionary[nodeId];
            }
            var newItem = new BranchCodeItem {
                DisplayText = nodeId,
                Type = type
            };
            _nodeDictionary.Add(nodeId, newItem);
            PushToExplorerList(parentNode, newItem);
            return newItem;
        }

        /// <summary>
        /// Add an item as a child of "parent", parent can be null and it will be added to the root node
        /// </summary>
        private void PushToExplorerList(CodeItem parent, CodeItem newChild, bool addAsNode = false) {
            if (parent == null) {
                _parsedExplorerItemsList.Add(newChild);
            } else {
                if (parent.Children == null)
                    parent.Children = new List<FilteredTypeTreeListItem>();
                parent.Children.Add(newChild);
            }
            if (addAsNode) {
                var nodeId = newChild.Type + newChild.DisplayText;
                if (!_nodeDictionary.ContainsKey(nodeId)) {
                    _nodeDictionary.Add(nodeId, newChild);
                }
            }
        }

        #endregion

        #region public accessors

        /// <summary>
        /// Contains the list of explorer items for the current file, updated by the parser's visitor class
        /// </summary>
        public List<CodeItem> ParsedExplorerItemsList {
            get { return _parsedExplorerItemsList; }
        }

        /// <summary>
        /// contains the list of items that depend on the current file, that list
        /// is updated by the parser's visitor class
        /// </summary>
        public List<CompletionItem> ParsedCompletionItemsList {
            get { return _parsedCompletionItemsList; }
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
                RunPersistentFiles.Clear();
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
                if (SavedPersistent.ContainsKey(_currentParsedFilePath))
                    SavedPersistent.Remove(_currentParsedFilePath);
            }
        }

        /// <summary>
        /// To be executed after the visit ends
        /// </summary>
        public void PostVisit() {
            // save the info for uses in an another file, where this file is run in persistent
            if (!SavedPersistent.ContainsKey(_currentParsedFilePath))
                SavedPersistent.Add(_currentParsedFilePath, this);
            else
                SavedPersistent[_currentParsedFilePath] = this;

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
                    if (String.IsNullOrEmpty(fullFilePath))
                        fullFilePath = ProEnvironment.Current.FindFirstFileInEnv(pars.Name + ".w");
                } else
                    fullFilePath = ProEnvironment.Current.FindFirstFileInEnv(pars.Name);

                if (String.IsNullOrEmpty(fullFilePath))
                    pars.Flags |= ParseFlag.NotFound;
                else {
                    // if the run is PERSISTENT, we need to load the functions/proc of the program
                    // ensure to not parse the same file twice in a parser session!
                    if (!RunPersistentFiles.Contains(fullFilePath)) {
                        RunPersistentFiles.Add(fullFilePath);
                        LoadProcPersistent(fullFilePath, pars.Scope);
                    }
                }
            }

            // to code explorer
            var internalRun = _parser.ParsedItemsList.Exists(item => {
                var proc = item as ParsedProcedure;
                return proc != null && proc.Name.EqualsCi(pars.Name);
            });
            var parentNode = internalRun ? GetExplorerListNode("Run internal routine", CodeExplorerIconType.RunInternal) : GetExplorerListNode("Run external procedure", CodeExplorerIconType.RunExternal);
            var newNode = CodeItem.Factory.New(internalRun ? CodeExplorerIconType.RunInternal : CodeExplorerIconType.RunExternal);
            newNode.DisplayText = pars.Name;
            newNode.Flags = pars.Flags;
            newNode.SubText = pars.Scope.Name;
            newNode.DocumentOwner = pars.FilePath;
            newNode.GoToLine = pars.Line;
            newNode.GoToColumn = pars.Column;
            PushToExplorerList(parentNode, newNode);
        }

        /// <summary>
        /// Dynamic-function
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedFunctionCall pars) {
            // To code explorer
            var parentNode = pars.ExternalCall ? GetExplorerListNode("External dynamic function calls", CodeExplorerIconType.DynamicFunctionCallExternal) : (pars.StaticCall ? GetExplorerListNode("Static function calls", CodeExplorerIconType.StaticFunctionCall) : GetExplorerListNode("Internal dynamic function calls", CodeExplorerIconType.DynamicFunctionCall));
            var newNode = CodeItem.Factory.New(pars.ExternalCall ? CodeExplorerIconType.DynamicFunctionCallExternal : (pars.StaticCall ? CodeExplorerIconType.StaticFunctionCall : CodeExplorerIconType.DynamicFunctionCall));
            newNode.DisplayText = pars.Name;
            newNode.Flags = pars.Flags;
            newNode.SubText = pars.Scope.Name;
            newNode.DocumentOwner = pars.FilePath;
            newNode.GoToLine = pars.Line;
            newNode.GoToColumn = pars.Column;
            PushToExplorerList(parentNode, newNode);
        }

        /// <summary>
        /// Tables used in the program
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedFoundTableUse pars) {
            bool missingDbName = pars.Name.IndexOf('.') < 0;

            // to code explorer
            var parentNode = pars.IsTempTable ? GetExplorerListNode("Temp-tables used", CodeExplorerIconType.TempTableUsed) : GetExplorerListNode("Tables used", CodeExplorerIconType.TableUsed);
            var newNode = CodeItem.Factory.New(pars.IsTempTable ? CodeExplorerIconType.TempTable : CodeExplorerIconType.Table);
            newNode.DisplayText = missingDbName ? pars.Name : pars.Name.Split('.')[1];
            newNode.Flags = (missingDbName && !pars.IsTempTable ? ParseFlag.MissingDbName : 0) | pars.Flags;
            newNode.SubText = null;
            newNode.DocumentOwner = pars.FilePath;
            newNode.GoToLine = pars.Line;
            newNode.GoToColumn = pars.Column;
            PushToExplorerList(parentNode, newNode);
        }

        public void Visit(ParsedEvent pars) {
            // to code explorer
            var parentNode = pars.Type == ParsedEventType.Subscribe ? GetExplorerListNode("Subscribe", CodeExplorerIconType.Subscribe) : (pars.Type == ParsedEventType.Unsubscribe ? GetExplorerListNode("Unsubscribe", CodeExplorerIconType.Unsubscribe) : GetExplorerListNode("Publish", CodeExplorerIconType.Publish));
            var newNode = CodeItem.Factory.New(pars.Type == ParsedEventType.Subscribe ? CodeExplorerIconType.Subscribe : (pars.Type == ParsedEventType.Unsubscribe ? CodeExplorerIconType.Unsubscribe : CodeExplorerIconType.Publish));
            newNode.DisplayText = pars.Name;
            newNode.Flags = pars.Flags;
            newNode.SubText = null;
            newNode.DocumentOwner = pars.FilePath;
            newNode.GoToLine = pars.Line;
            newNode.GoToColumn = pars.Column;
            PushToExplorerList(parentNode, newNode);
        }

        /// <summary>
        /// Include files
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedIncludeFile pars) {
            // To code explorer
            PushToExplorerList(
                GetExplorerListNode("Includes", CodeExplorerIconType.Include),
                new IncludeCodeItem {
                    DisplayText = pars.Name,
                    Flags = pars.Flags,
                    SubText = null,
                    DocumentOwner = pars.FilePath,
                    GoToLine = pars.Line,
                    GoToColumn = pars.Column
                });
        }

        /// <summary>
        /// Root file block
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedFile pars) {
            // to code explorer
            PushToExplorerList(
                null,
                new RootCodeItem {
                    DisplayText = "Root",
                    SubText = null,
                    DocumentOwner = pars.FilePath,
                    GoToLine = pars.Line,
                    GoToColumn = pars.Column
                });
        }

        /// <summary>
        /// Main block, definitions block...
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedPreProcBlock pars) {
            if (pars.Flags.HasFlag(ParseFlag.FromInclude))
                return;

            CodeExplorerIconType type;
            switch (pars.Type) {
                case ParsedPreProcBlockType.MainBlock:
                    type = CodeExplorerIconType.MainBlock;
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
            CodeItem parentNode = type == CodeExplorerIconType.MainBlock ? null : GetExplorerListNode("AppBuilder blocks", CodeExplorerIconType.Block);
            CodeItem newNode = CodeItem.Factory.New(type);
            newNode.DisplayText = pars.Name;
            newNode.Flags = pars.Flags;
            newNode.SubText = null;
            newNode.DocumentOwner = pars.FilePath;
            newNode.GoToLine = pars.Line;
            newNode.GoToColumn = pars.Column;
            if (type != CodeExplorerIconType.MainBlock) {
                newNode.Type = type;
            }
            PushToExplorerList(parentNode, newNode);
        }

        /// <summary>
        /// ON events
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedOnStatement pars) {
            // check length of block
            CheckForTooMuchChar(pars);

            // To code explorer
            PushToExplorerList(
                GetExplorerListNode("ON events", CodeExplorerIconType.OnEvent),
                new OnEventCodeItem {
                    DisplayText = pars.Name,
                    Flags = pars.Flags,
                    SubText = pars.TooLongForAppbuilder ? BlockTooLongString + " (+" + NbExtraCharBetweenLines(pars.Line, pars.EndBlockLine) + ")" : null,
                    DocumentOwner = pars.FilePath,
                    GoToLine = pars.Line,
                    GoToColumn = pars.Column
                });
        }

        public void Visit(ParsedImplementation pars) {
            // check length of block
            CheckForTooMuchChar(pars);

            // to code explorer
            PushToExplorerList(
                GetExplorerListNode("Functions", CodeExplorerIconType.Function),
                new FunctionCodeItem {
                    DisplayText = pars.Name,
                    Flags = pars.Flags,
                    SubText = pars.TooLongForAppbuilder ? BlockTooLongString + " (+" + NbExtraCharBetweenLines(pars.Line, pars.EndBlockLine) + ")" : null,
                    DocumentOwner = pars.FilePath,
                    GoToLine = pars.Line,
                    GoToColumn = pars.Column
                });

            // to completion data
            pars.ReturnType = ParserUtils.ConvertStringToParsedPrimitiveType(pars.ParsedReturnType, false, _parser.ParsedItemsList);
            _parsedCompletionItemsList.Add(new FunctionCompletionItem {
                DisplayText = pars.Name,
                SubText = pars.ReturnType.ToString(),
                Flags = pars.Flags,
                Ranking = AutoCompletion.FindRankingOfParsedItem(pars.Name),
                ParsedBaseItem = pars,
                FromParser = true
            });
        }

        public void Visit(ParsedPrototype pars) {
            // only visit IN prototypes
            if (pars.SimpleForward)
                return;

            // to code explorer
            PushToExplorerList(
                GetExplorerListNode("Function prototypes", CodeExplorerIconType.Prototype),
                new PrototypeCodeItem {
                    DisplayText = pars.Name,
                    Flags = pars.Flags,
                    SubText = null,
                    DocumentOwner = pars.FilePath,
                    GoToLine = pars.Line,
                    GoToColumn = pars.Column
                });

            // to completion data
            pars.ReturnType = ParserUtils.ConvertStringToParsedPrimitiveType(pars.ParsedReturnType, false, _parser.ParsedItemsList);
            _parsedCompletionItemsList.Add(new FunctionCompletionItem {
                DisplayText = pars.Name,
                Flags = pars.Flags,
                Ranking = AutoCompletion.FindRankingOfParsedItem(pars.Name),
                ParsedBaseItem = pars,
                FromParser = true,
                SubText = pars.ReturnType.ToString()
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
            var parentNode = pars.Flags.HasFlag(ParseFlag.External) ? GetExplorerListNode("External procedures", CodeExplorerIconType.ExternalProcedure) : GetExplorerListNode("Procedures", CodeExplorerIconType.Procedure);
            var newNode = CodeItem.Factory.New(pars.Flags.HasFlag(ParseFlag.External) ? CodeExplorerIconType.ExternalProcedure : CodeExplorerIconType.Procedure);
            newNode.DisplayText = pars.Name;
            newNode.Flags = pars.Flags;
            newNode.SubText = pars.Flags.HasFlag(ParseFlag.External) ? pars.ExternalDllName : pars.TooLongForAppbuilder ? BlockTooLongString + " (+" + NbExtraCharBetweenLines(pars.Line, pars.EndBlockLine) + ")" : null;
            newNode.DocumentOwner = pars.FilePath;
            newNode.GoToLine = pars.Line;
            newNode.GoToColumn = pars.Column;
            PushToExplorerList(parentNode, newNode);

            // to completion data
            var proc = CompletionItem.Factory.New(pars.Flags.HasFlag(ParseFlag.External) ? CompletionType.ExternalProcedure : CompletionType.Procedure);
            proc.DisplayText = pars.Name;
            proc.ParsedBaseItem = pars;
            proc.FromParser = true;
            proc.SubText = pars.Flags.HasFlag(ParseFlag.External) ? pars.ExternalDllName : null;
            proc.Ranking = AutoCompletion.FindRankingOfParsedItem(pars.Name);
            proc.Flags = pars.Flags;
        }

        /// <summary>
        /// Preprocessed variables
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedPreProcVariable pars) {
            if (pars.Flags.HasFlag(ParseFlag.Global) || !pars.Flags.HasFlag(ParseFlag.FromInclude))
                // to completion data
                _parsedCompletionItemsList.Add(new PreprocessedCompletionItem {
                    DisplayText = "&" + pars.Name,
                    Flags = pars.Flags,
                    Ranking = AutoCompletion.FindRankingOfParsedItem(pars.Name),
                    ParsedBaseItem = pars,
                    FromParser = true,
                    SubText = null
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
            _parsedCompletionItemsList.Add(new LabelCompletionItem {
                DisplayText = pars.Name,
                Flags = pars.Flags,
                Ranking = AutoCompletion.FindRankingOfParsedItem(pars.Name),
                ParsedBaseItem = pars,
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
            var hasPrimitive = !String.IsNullOrEmpty(pars.TempPrimitiveType);
            if (hasPrimitive)
                pars.PrimitiveType = ParserUtils.ConvertStringToParsedPrimitiveType(pars.TempPrimitiveType, pars.AsLike == ParsedAsLike.Like, _parser.ParsedItemsList);

            // which completionData type is it?
            CompletionType type;
            string subString;
            // special case for buffers, they go into the temp-table or table section
            if (pars.PrimitiveType == ParsedPrimitiveType.Buffer) {
                pars.Flags |= ParseFlag.Buffer;
                subString = "?";
                type = CompletionType.TempTable;

                // find the table or temp table that the buffer is FOR
                var foundTable = ParserUtils.FindAnyTableByName(pars.BufferFor, _parser.ParsedItemsList);
                if (foundTable != null) {
                    subString = foundTable.Name;
                    type = foundTable.IsTempTable ? CompletionType.TempTable : CompletionType.Table;

                    // extra flags
                    pars.Flags |= !pars.BufferFor.Contains(".") && !foundTable.IsTempTable ? ParseFlag.MissingDbName : 0;

                    // To code explorer, list buffers and associated tables
                    var parentNode = foundTable.IsTempTable ? GetExplorerListNode("Temp-tables used", CodeExplorerIconType.TempTableUsed) : GetExplorerListNode("Tables used", CodeExplorerIconType.TableUsed);
                    var newNode = CodeItem.Factory.New(foundTable.IsTempTable ? CodeExplorerIconType.TempTable : CodeExplorerIconType.Table);
                    newNode.DisplayText = pars.Name;
                    newNode.Flags = pars.Flags;
                    newNode.SubText = null;
                    newNode.DocumentOwner = pars.FilePath;
                    newNode.GoToLine = pars.Line;
                    newNode.GoToColumn = pars.Column;
                    PushToExplorerList(parentNode, newNode);
                }
            } else {
                // match type for everything else
                subString = hasPrimitive ? pars.PrimitiveType.ToString() : pars.Type.ToString();
                switch (pars.Type) {
                    case ParseDefineType.Parameter:
                        type = CompletionType.VariablePrimitive;

                        // To code explorer, program parameters
                        if (_isBaseFile && pars.Scope is ParsedFile) {
                            PushToExplorerList(
                                GetExplorerListNode("Program parameters", CodeExplorerIconType.ProgramParameter),
                                new ParameterCodeItem {
                                    DisplayText = pars.Name,
                                    Flags = pars.Flags,
                                    SubText = subString,
                                    DocumentOwner = pars.FilePath,
                                    GoToLine = pars.Line,
                                    GoToColumn = pars.Column
                                });
                        }
                        break;
                    case ParseDefineType.Variable:
                        if (!String.IsNullOrEmpty(pars.ViewAs))
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
                PushToExplorerList(
                    GetExplorerListNode("Browse definitions", CodeExplorerIconType.Browse),
                    new BrowseCodeItem {
                        DisplayText = pars.Name,
                        Flags = pars.Flags,
                        SubText = subString,
                        DocumentOwner = pars.FilePath,
                        GoToLine = pars.Line,
                        GoToColumn = pars.Column
                    });
            }

            // to completion data
            var curItem = CompletionItem.Factory.New(type);
            curItem.DisplayText = pars.Name;
            curItem.Flags = pars.Flags;
            curItem.Ranking = AutoCompletion.FindRankingOfParsedItem(pars.Name);
            curItem.ParsedBaseItem = pars;
            curItem.FromParser = true;
            curItem.SubText = subString;
            _parsedCompletionItemsList.Add(curItem);
        }

        /// <summary>
        /// Defined Temptables
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedTable pars) {
            string subStr = "";

            // find all primitive types
            foreach (var parsedField in pars.Fields)
                parsedField.Type = ParserUtils.ConvertStringToParsedPrimitiveType(parsedField.TempType, parsedField.AsLike == ParsedAsLike.Like, _parser.ParsedItemsList);

            // temp table is LIKE another table? copy fields
            if (!String.IsNullOrEmpty(pars.LcLikeTable)) {
                var foundLikeTable = ParserUtils.FindAnyTableByName(pars.LcLikeTable, _parser.ParsedItemsList);
                if (foundLikeTable != null) {
                    // add the fields of the found table (minus the primary information)
                    subStr = @"Like " + foundLikeTable.Name;
                    foreach (var field in foundLikeTable.Fields) {
                        pars.Fields.Add(
                            new ParsedField(field.Name, "", field.Format, field.Order, 0, field.InitialValue, field.Description, field.AsLike) {
                                Type = field.Type
                            });
                    }

                    // handles the use-index
                    if (!String.IsNullOrEmpty(pars.UseIndex)) {
                        // add only the indexes that are used
                        foreach (var index in pars.UseIndex.Split(',')) {
                            var foundIndex = foundLikeTable.Indexes.Find(index2 => index2.Name.EqualsCi(index.Replace("!", "")));
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
                        pars.Indexes = foundLikeTable.Indexes.ToList();
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

            // to auto completion
            var parsedTable = new TempTableCompletionItem {
                DisplayText = pars.Name,
                Flags = pars.Flags,
                Ranking = AutoCompletion.FindRankingOfParsedItem(pars.Name),
                ParsedBaseItem = pars,
                FromParser = true,
                SubText = subStr,
                ChildSeparator = '.'
            };
            parsedTable.Children = pars.Fields.Select(field => {
                var curField = CompletionItem.Factory.New(field.Flags.HasFlag(ParseFlag.Primary) ? CompletionType.FieldPk : CompletionType.Field);
                curField.DisplayText = field.Name.ConvertCase(Config.Instance.AutoCompleteDatabaseWordCaseMode);
                curField.ParsedBaseItem = field;
                curField.FromParser = true;
                curField.SubText = field.Type.ToString();
                curField.Ranking = AutoCompletion.FindRankingOfParsedItem(field.Name);
                curField.Flags = field.Flags | ~ParseFlag.Primary;
                curField.ParentItem = parsedTable;
                return curField;
            }).ToList();
            _parsedCompletionItemsList.Add(parsedTable);

            // to code explorer
            PushToExplorerList(
                GetExplorerListNode("Defined temp-tables", CodeExplorerIconType.DefinedTempTable),
                new TempTableCodeItem {
                    DisplayText = pars.Name,
                    Flags = pars.Flags,
                    SubText = subStr,
                    DocumentOwner = pars.FilePath,
                    GoToLine = pars.Line,
                    GoToColumn = pars.Column
                });
        }

        #endregion

        #region helper

        /// <summary>
        /// Parses given file and load its function + procedures has persistent so they are
        /// accessible from the autocompletion list
        /// Set runPersistentIsInFile = false (default) to add items only to the completion list,
        /// set to true to also display proc/func in the code explorer tree if asked
        /// </summary>
        private void LoadProcPersistent(string fileName, ParsedScopeItem scopeItem) {
            ParserVisitor parserVisitor = ParseFile(fileName, scopeItem);

            // add info to the completion list
            var listToAdd = parserVisitor._parsedCompletionItemsList.Where(data => data is FunctionCompletionItem || data is ProcedureCompletionItem).ToList();
            foreach (var completionData in listToAdd) {
                completionData.Flags = completionData.Flags | ParseFlag.Persistent;
            }
            _parsedCompletionItemsList.AddRange(listToAdd);

            // add info to the code explorer
            if (Config.Instance.CodeExplorerDisplayExternalItems) {
                foreach (var codeExplorerItem in parserVisitor._parsedExplorerItemsList.SelectMany(item => item.Children ?? new List<FilteredTypeTreeListItem>()).Cast<CodeItem>().Where(item => item is FunctionCodeItem || item is ProcedureCodeItem)) {
                    codeExplorerItem.Flags = codeExplorerItem.Flags | ParseFlag.Persistent;
                    PushToExplorerList(codeExplorerItem is FunctionCodeItem ? GetExplorerListNode("Functions", CodeExplorerIconType.Function) : GetExplorerListNode("Procedures", CodeExplorerIconType.Procedure), codeExplorerItem);
                }
            }
        }

        /// <summary>
        /// Parses a file.
        /// Remarks : it doesn't parse the document against known words since this is only useful for
        /// the CURRENT document and not for the others
        /// </summary>
        private static ParserVisitor ParseFile(string filePath, ParsedScopeItem scopeItem) {
            ParserVisitor parserVisitor;

            // did we already parsed this file in a previous parse session? (if we are in CodeExplorerDisplayExternalItems mode we need to parse it again anyway)
            if (SavedPersistent.ContainsKey(filePath)) {
                parserVisitor = SavedPersistent[filePath];
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
            // check length of block
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
            return (Sci.StartBytePosOfLine(endLine) - Sci.StartBytePosOfLine(startLine)) - Config.Instance.GlobalMaxNbCharInBlock;
        }

        #endregion
    }
}