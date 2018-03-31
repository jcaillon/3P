#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
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
using Yamui.Framework.Controls.YamuiList;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletionFeature;
using _3PA.MainFeatures.CodeExplorer;
using _3PA.MainFeatures.Pro;

namespace _3PA.MainFeatures.Parser.Pro.Visit {

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
        
        /// <summary>
        /// Are we currently visiting the current file opened in npp or
        /// is it a include?
        /// </summary>
        private bool _isBaseFile;

        /// <summary>
        /// Reference of the parser being visited
        /// </summary>
        private Parse.Parser _parser;

        #endregion

        #region AutoCompletion list

        /// <summary>
        /// contains the list of items that depend on the current file, that list
        /// is updated by the parser's visitor class
        /// </summary>
        private List<CompletionItem> _parsedCompletionItemsList = new List<CompletionItem>();

        private void PushToAutoCompletion(CompletionItem item, ParsedBaseItem parsedItem) {
            item.FromParser = true;
            item.ParsedBaseItem = parsedItem;
            item.Ranking = AutoCompletion.FindRankingOfParsedItem(item.DisplayText);
            ParsedCompletionItemsList.Add(item);
        }

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
            PushToCodeExplorer(parentNode, newItem);
            return newItem;
        }

        /// <summary>
        /// Add an item as a child of "parent", parent can be null and it will be added to the root node
        /// </summary>
        private void PushToCodeExplorer(CodeItem parent, CodeItem newChild, bool addAsNode = false) {
            if (!Config.Instance.CodeExplorerDisplayItemsFromInclude && newChild.Flags.HasFlag(ParseFlag.FromInclude)) {
                return;
            }
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
        public void PreVisit(Parse.Parser parser) {
            _parser = parser;

            // if this document is in the Saved parsed visitors, we remove it now and we will add it back when it is parsed
            if (_isBaseFile) {
                if (SavedPersistent.ContainsKey(_parser.FilePathBeingParsed))
                    SavedPersistent.Remove(_parser.FilePathBeingParsed);
            }
        }

        /// <summary>
        /// To be executed after the visit ends
        /// </summary>
        public void PostVisit() {
            // save the info for uses in an another file, where this file is run in persistent
            if (!SavedPersistent.ContainsKey(_parser.FilePathBeingParsed))
                SavedPersistent.Add(_parser.FilePathBeingParsed, this);
            else
                SavedPersistent[_parser.FilePathBeingParsed] = this;

            // lose parser reference
            _parser = null;
        }

        /// <summary>
        /// All the references of {&amp;variable} found
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedUsedPreProcVariable pars) {
            var preproc = new PreprocVarCodeItem {
                DisplayText = pars.Name,
                Flags = pars.Flags,
                SubText = null,
                DocumentOwner = pars.FilePath,
                GoToLine = pars.Line,
                GoToColumn = pars.Column
            };

            // To code explorer
            if (pars.Flags.HasFlag(ParseFlag.NotFound)) {
                PushToCodeExplorer(
                    GetExplorerListNode("Missing includes/variables", CodeExplorerIconType.MissingInclude),
                    preproc);
            } else {
                // To code explorer
                PushToCodeExplorer(
                    GetExplorerListNode("Preprocessed variables usage", CodeExplorerIconType.PreprocessedVariable),
                    preproc);
            }
        }

        /// <summary>
        /// Unknow word found in the program
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedWord pars) {

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
                string procName = pars.Name.ToLower();
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
            PushToCodeExplorer(parentNode, newNode);
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
            PushToCodeExplorer(parentNode, newNode);
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
            PushToCodeExplorer(parentNode, newNode);
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
            PushToCodeExplorer(parentNode, newNode);
        }

        /// <summary>
        /// Include files
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedIncludeFile pars) {

            var inc = new IncludeCodeItem {
                DisplayText = pars.Name,
                Flags = pars.Flags,
                SubText = null,
                DocumentOwner = pars.FilePath,
                GoToLine = pars.Line,
                GoToColumn = pars.Column
            };

            if (pars.Flags.HasFlag(ParseFlag.NotFound)) {
                PushToCodeExplorer(
                    GetExplorerListNode("Missing includes/variables", CodeExplorerIconType.MissingInclude),
                    inc);
            } else {
                // To code explorer
                PushToCodeExplorer(
                    GetExplorerListNode("Includes", CodeExplorerIconType.Include),
                    inc);
            }
        }

        /// <summary>
        /// Root file block
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedFile pars) {
            // to code explorer
            PushToCodeExplorer(
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
        public void Visit(ParsedScopePreProcBlock pars) {
            if (pars.Flags.HasFlag(ParseFlag.FromInclude))
                return;

            // only display special blocks on the explorer
            if (pars.Type == ParsedPreProcBlockType.Prototype || pars.Type == ParsedPreProcBlockType.Block)
                return;

            // convert to explorer type
            CodeExplorerIconType type;
            if (!Enum.TryParse(pars.Type.ToString(), out type))
                return;

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
            PushToCodeExplorer(parentNode, newNode);
        }
        
        /// <summary>
        /// ON events
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedOnStatement pars) {
            // To code explorer
            PushToCodeExplorer(
                GetExplorerListNode("ON events", CodeExplorerIconType.OnEvent),
                new OnEventCodeItem {
                    DisplayText = pars.Name,
                    Flags = pars.Flags,
                    SubText = null,
                    DocumentOwner = pars.FilePath,
                    GoToLine = pars.Line,
                    GoToColumn = pars.Column
                });
        }

        public void Visit(ParsedImplementation pars) {

            pars.ReturnType = ConvertStringToParsedPrimitiveType(pars.TempReturnType, false);

            // to code explorer
            PushToCodeExplorer(
                GetExplorerListNode("Functions", CodeExplorerIconType.Function),
                new FunctionCodeItem {
                    DisplayText = pars.Name,
                    Flags = pars.Flags,
                    SubText = null,
                    DocumentOwner = pars.FilePath,
                    GoToLine = pars.Line,
                    GoToColumn = pars.Column
                });

            // to completion data
            PushToAutoCompletion(new FunctionCompletionItem {
                DisplayText = pars.Name,
                SubText = pars.ReturnType.ToString(),
                Flags = pars.Flags,
            }, pars);
        }

        public void Visit(ParsedPrototype pars) {
            
            pars.ReturnType = ConvertStringToParsedPrimitiveType(pars.TempReturnType, false);

            // only visit IN prototypes (not FORWARD)
            if (pars.SimpleForward)
                return;
            
            // to code explorer
            PushToCodeExplorer(
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
            PushToAutoCompletion(new FunctionCompletionItem {
                DisplayText = pars.Name,
                Flags = pars.Flags,
                SubText = pars.ReturnType.ToString()
            }, pars);
        }

        /// <summary>
        /// Procedures
        /// </summary>
        public void Visit(ParsedProcedure pars) {
            // to code explorer
            var parentNode = pars.Flags.HasFlag(ParseFlag.External) ? GetExplorerListNode("External procedures", CodeExplorerIconType.ExternalProcedure) : GetExplorerListNode("Procedures", CodeExplorerIconType.Procedure);
            var newNode = CodeItem.Factory.New(pars.Flags.HasFlag(ParseFlag.External) ? CodeExplorerIconType.ExternalProcedure : CodeExplorerIconType.Procedure);
            newNode.DisplayText = pars.Name;
            newNode.Flags = pars.Flags;
            newNode.SubText = pars.Flags.HasFlag(ParseFlag.External) ? pars.ExternalDllName : null;
            newNode.DocumentOwner = pars.FilePath;
            newNode.GoToLine = pars.Line;
            newNode.GoToColumn = pars.Column;
            PushToCodeExplorer(parentNode, newNode);

            // to completion data
            var proc = CompletionItem.Factory.New(pars.Flags.HasFlag(ParseFlag.External) ? CompletionType.ExternalProcedure : CompletionType.Procedure);
            proc.DisplayText = pars.Name;
            proc.ParsedBaseItem = pars;
            proc.FromParser = true;
            proc.SubText = pars.Flags.HasFlag(ParseFlag.External) ? pars.ExternalDllName : null;
            proc.Ranking = AutoCompletion.FindRankingOfParsedItem(pars.Name);
            proc.Flags = pars.Flags;
            PushToAutoCompletion(proc, pars);
        }

        /// <summary>
        /// Preprocessed variables
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedPreProcVariable pars) {
            if (pars.Flags.HasFlag(ParseFlag.Global) || !pars.Flags.HasFlag(ParseFlag.FromInclude))
                // to completion data
                PushToAutoCompletion(new PreprocessedCompletionItem {
                    DisplayText = "&" + pars.Name,
                    Flags = pars.Flags,
                    SubText = null
                }, pars);
        }

        /// <summary>
        /// Labels
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedLabel pars) {
            // find the end line of the labeled 
            if (pars.Block != null) {
                pars.UndefinedLine = pars.Block.EndBlockLine;
            }

            // to completion data
            PushToAutoCompletion(new LabelCompletionItem {
                DisplayText = pars.Name,
                Flags = pars.Flags,
            }, pars);
        }

        /// <summary>
        /// Defined variables
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedDefine pars) {

            pars.PrimitiveType = ConvertStringToParsedPrimitiveType(pars.TempPrimitiveType, pars.AsLike == ParsedAsLike.Like);

            var subString = pars.PrimitiveType == ParsedPrimitiveType.Unknow ? pars.Type.ToString() : pars.PrimitiveType.ToString();
            CompletionType type;
            switch (pars.Type) {
                case ParseDefineType.Parameter:
                    type = CompletionType.VariablePrimitive;

                    // To code explorer, program parameters
                    PushToCodeExplorerAsParameter(pars);
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
                case ParseDefineType.Frame:
                case ParseDefineType.Image:
                case ParseDefineType.SubMenu:
                case ParseDefineType.Menu:
                case ParseDefineType.Rectangle:
                    type = CompletionType.Widget;
                    break;
                case ParseDefineType.Browse:
                    type = CompletionType.Widget;

                    // To explorer code for browse
                    PushToCodeExplorer(
                        GetExplorerListNode("Browse definitions", CodeExplorerIconType.Browse),
                        new BrowseCodeItem {
                            DisplayText = pars.Name,
                            Flags = pars.Flags,
                            SubText = subString,
                            DocumentOwner = pars.FilePath,
                            GoToLine = pars.Line,
                            GoToColumn = pars.Column
                        });
                    break;
                default:
                    type = CompletionType.VariableComplex;
                    break;
            }

            // to completion data
            var curItem = CompletionItem.Factory.New(type);
            curItem.DisplayText = pars.Name;
            curItem.Flags = pars.Flags;
            curItem.SubText = subString;
            PushToAutoCompletion(curItem, pars);

        }

        /// <summary>
        /// Buffers
        /// </summary>
        public void Visit(ParsedBuffer pars) {
            
            pars.TargetTable = FindAnyTableByName(pars.BufferFor);
            pars.Flags |= !pars.BufferFor.Contains(".") && pars.TargetTable != null && !pars.TargetTable.IsTempTable ? ParseFlag.MissingDbName : 0;
            
            var subString = "?";
            var type = CompletionType.TempTable;

            if (pars.TargetTable != null) {
                subString = pars.TargetTable.Name;
                type = pars.TargetTable.IsTempTable ? CompletionType.TempTable : CompletionType.Table;

                // To code explorer, list buffers and associated tables
                var parentNode = pars.TargetTable.IsTempTable ? GetExplorerListNode("Temp-tables used", CodeExplorerIconType.TempTableUsed) : GetExplorerListNode("Tables used", CodeExplorerIconType.TableUsed);
                var newNode = CodeItem.Factory.New(pars.TargetTable.IsTempTable ? CodeExplorerIconType.TempTable : CodeExplorerIconType.Table);
                newNode.DisplayText = pars.Name;
                newNode.Flags = pars.Flags;
                newNode.SubText = null;
                newNode.DocumentOwner = pars.FilePath;
                newNode.GoToLine = pars.Line;
                newNode.GoToColumn = pars.Column;
                PushToCodeExplorer(parentNode, newNode);
            }

            // parameter buffer?
            if (pars.Flags.HasFlag(ParseFlag.Parameter))
                PushToCodeExplorerAsParameter(pars);

            // to completion data
            var curItem = CompletionItem.Factory.New(type) as TableCompletionItem;
            if (curItem != null) {
                if (pars.TargetTable != null) {
                    curItem.ChildSeparator = '.';
                    curItem.Children = GetTableCompletionItemChildren(curItem, pars.TargetTable.Fields);
                }
                curItem.DisplayText = pars.Name;
                curItem.Flags = pars.Flags;
                curItem.SubText = subString;
                PushToAutoCompletion(curItem, pars);
            }
        }

        private void PushToCodeExplorerAsParameter(ParsedItem pars) {
            // To code explorer, program parameters
            if (_isBaseFile && pars.Scope is ParsedFile) {
                string subText = null;
                var parsDefine = pars as ParsedDefine;
                if (parsDefine != null)
                    subText = parsDefine.PrimitiveType == ParsedPrimitiveType.Unknow ? parsDefine.Type.ToString() : parsDefine.PrimitiveType.ToString();
                PushToCodeExplorer(
                    GetExplorerListNode("Program parameters", CodeExplorerIconType.ProgramParameter),
                    new ParameterCodeItem {
                        DisplayText = pars.Name,
                        Flags = pars.Flags,
                        SubText = subText,
                        DocumentOwner = pars.FilePath,
                        GoToLine = pars.Line,
                        GoToColumn = pars.Column
                    });
            }
        }

        /// <summary>
        /// Defined Temptables
        /// </summary>
        public void Visit(ParsedTable pars) {
            
            if (!string.IsNullOrEmpty(pars.TempLikeTable)) {
                pars.LikeTable = FindAnyTableByName(pars.TempLikeTable);
            }
            
            foreach (var field in pars.Fields) {
                field.PrimitiveType = ConvertStringToParsedPrimitiveType(field.TempPrimitiveType, field.AsLike == ParsedAsLike.Like);
            }

            // temp table is LIKE another table? copy fields
            if (pars.LikeTable != null) {

                // add the fields of the found table (minus the primary information)
                foreach (var field in pars.LikeTable.Fields) {
                    pars.Fields.Add(
                        new ParsedField(field.Name, field.TempPrimitiveType, field.Format, field.Order, 0, field.InitialValue, field.Description, field.AsLike) {
                            PrimitiveType = field.PrimitiveType
                        });
                }

                // handles the use-index
                if (!string.IsNullOrEmpty(pars.UseIndex)) {
                    // add only the indexes that are used
                    foreach (var index in pars.UseIndex.Split(',')) {
                        var foundIndex = pars.LikeTable.Indexes.Find(index2 => index2.Name.EqualsCi(index.Replace("!", "")));
                        if (foundIndex != null) {
                            pars.Indexes.Add(new ParsedIndex(foundIndex.Name, foundIndex.Flag, foundIndex.FieldsList.ToList()));
                            // if one of the index used is marked as primary
                            if (index.ContainsFast("!")) {
                                pars.Indexes.ForEach(parsedIndex => parsedIndex.Flag &= ~ParsedIndexFlag.Primary);
                                pars.Indexes.Last().Flag |= ParsedIndexFlag.Primary;
                            }
                        }
                    }
                } else {
                    // if there is no "use index" and we didn't define new indexes, the tt uses the same index as the original table
                    if (pars.Indexes == null || pars.Indexes.Count == 0)
                        pars.Indexes = pars.LikeTable.Indexes.ToList();
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
            

            string subStr = string.IsNullOrEmpty(pars.TempLikeTable) ? "" : (pars.LikeTable != null ? @"Like " + pars.LikeTable.Name : @"Like ??");

            // to auto completion
            var parsedTable = new TempTableCompletionItem {
                DisplayText = pars.Name,
                Flags = pars.Flags,
                SubText = subStr,
                ChildSeparator = '.'
            };
            parsedTable.Children = pars.Fields.Select(field => {
                var curField = CompletionItem.Factory.New(field.Flags.HasFlag(ParseFlag.Primary) ? CompletionType.FieldPk : CompletionType.Field);
                curField.DisplayText = field.Name.ConvertCase(Config.Instance.AutoCompleteDatabaseWordCaseMode);
                curField.ParsedBaseItem = field;
                curField.FromParser = true;
                curField.SubText = field.PrimitiveType.ToString();
                curField.Ranking = AutoCompletion.FindRankingOfParsedItem(field.Name);
                curField.Flags = field.Flags & ~ParseFlag.Primary;
                curField.ParentItem = parsedTable;
                return curField;
            }).ToList();
            PushToAutoCompletion(parsedTable, pars);

            // to code explorer
            PushToCodeExplorer(
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
                var foundVar = _parser.ParsedItemsList.Find(data => {
                    var def = data as ParsedDefine;
                    return def != null && def.Type != ParseDefineType.Buffer && def.PrimitiveType != ParsedPrimitiveType.Unknow && def.Name.EqualsCi(likeStr);
                }) as ParsedDefine;
                return foundVar != null ? foundVar.PrimitiveType : ParsedPrimitiveType.Unknow;
            }

            // Search the databases
            var foundField = DataBase.Instance.FindFieldByName(likeStr);
            if (foundField != null)
                return foundField.PrimitiveType;

            var tableName = splitted[nbPoints == 2 ? 1 : 0];
            var fieldName = splitted[nbPoints == 2 ? 2 : 1];

            // Search in temp tables
            if (nbPoints != 1)
                return ParsedPrimitiveType.Unknow;

            var foundTtable = FindAnyTableOrBufferByName(tableName);
            if (foundTtable == null)
                return ParsedPrimitiveType.Unknow;

            var foundTtField = foundTtable.Fields.Find(field => field.Name.EqualsCi(fieldName));
            return foundTtField == null ? ParsedPrimitiveType.Unknow : foundTtField.PrimitiveType;
        }

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
            var foundBuffer = _parser.ParsedItemsList.Find(data => data is ParsedBuffer && data.Name.EqualsCi(name)) as ParsedBuffer;
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
            return _parser.ParsedItemsList.Find(item => {
                var tt = item as ParsedTable;
                return tt != null && tt.IsTempTable && tt.Name.EqualsCi(name);
            }) as ParsedTable;
        }

        private static List<CompletionItem> GetTableCompletionItemChildren(TableCompletionItem table, List<ParsedField> parsedFields) {
            return parsedFields.Select(field => {
                var curField = CompletionItem.Factory.New(field.Flags.HasFlag(ParseFlag.Primary) ? CompletionType.FieldPk : CompletionType.Field);
                curField.DisplayText = field.Name.ConvertCase(Config.Instance.AutoCompleteDatabaseWordCaseMode);
                curField.ParsedBaseItem = field;
                curField.FromParser = true;
                curField.SubText = field.PrimitiveType.ToString();
                curField.Ranking = AutoCompletion.FindRankingOfParsedItem(field.Name);
                curField.Flags = field.Flags & ~ParseFlag.Primary;
                curField.ParentItem = table;
                return curField;
            }).ToList();
        }

        /// <summary>
        /// Parses given file and load its function + procedures has persistent so they are
        /// accessible from the autocompletion list
        /// Set runPersistentIsInFile = false (default) to add items only to the completion list,
        /// set to true to also display proc/func in the code explorer tree if asked
        /// </summary>
        private void LoadProcPersistent(string fileName, ParsedScopeBlock scopeItem) {
            ParserVisitor parserVisitor = ParseFile(fileName, scopeItem);

            // add info to the completion list
            var listToAdd = parserVisitor.ParsedCompletionItemsList.Where(data => data is FunctionCompletionItem || data is ProcedureCompletionItem).ToList();
            foreach (var completionData in listToAdd) {
                completionData.Flags = completionData.Flags | ParseFlag.Persistent;
            }
            _parsedCompletionItemsList.AddRange(listToAdd);

            // add info to the code explorer
            if (Config.Instance.CodeExplorerDisplayPersistentItems) {
                foreach (var codeExplorerItem in parserVisitor.ParsedExplorerItemsList.SelectMany(item => item.Children ?? new List<FilteredTypeTreeListItem>()).Cast<CodeItem>().Where(item => item is FunctionCodeItem || item is ProcedureCodeItem)) {
                    codeExplorerItem.Flags = codeExplorerItem.Flags | ParseFlag.Persistent;
                    PushToCodeExplorer(codeExplorerItem is FunctionCodeItem ? GetExplorerListNode("Functions", CodeExplorerIconType.Function) : GetExplorerListNode("Procedures", CodeExplorerIconType.Procedure), codeExplorerItem);
                }
            }
        }

        /// <summary>
        /// Parses a file.
        /// Remarks : it doesn't parse the document against known words since this is only useful for
        /// the CURRENT document and not for the others
        /// </summary>
        private static ParserVisitor ParseFile(string filePath, ParsedScopeBlock scopeItem) {
            ParserVisitor parserVisitor;

            // did we already parsed this file in a previous parse session? (if we are in CodeExplorerDisplayExternalItems mode we need to parse it again anyway)
            if (SavedPersistent.ContainsKey(filePath)) {
                parserVisitor = SavedPersistent[filePath];
            } else {
                // Parse it
                var ablParser = new Parse.Parser(Utils.ReadAllText(filePath), filePath, scopeItem, false);
                parserVisitor = new ParserVisitor(false);
                ablParser.Accept(parserVisitor);
            }

            return parserVisitor;
        }

        #endregion
    }
}