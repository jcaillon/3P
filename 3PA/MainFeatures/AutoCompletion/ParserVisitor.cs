using System;
using System.Collections.Generic;
using System.Linq;
using _3PA.Lib;
using _3PA.MainFeatures.DockableExplorer;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.AutoCompletion {

    /// <summary>
    /// This class sustains the autocompletion list AND the code explorer list
    /// by visiting the parser and creating new completionData
    /// </summary>
    class ParserVisitor : IParserVisitor {

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
        public Dictionary<string, bool> DefinedProcedures = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Line info from the parser
        /// </summary>
        private Dictionary<int, LineInfo> _lineInfo;
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
            // to code explorer
            if (_isBaseFile)
                ParserHandler.ParsedExplorerItemsList.Add(new CodeExplorerItem() {
                    DisplayText = pars.Name,
                    Branch = CodeExplorerBranch.Run,
                    IconType = CodeExplorerIconType.RunExternal,
                    GoToLine = pars.Line,
                    IsNotBlock = true,
                    Flag = pars.IsEvaluateValue ? CodeExplorerFlag.Uncertain : 0
                });
        }

        /// <summary>
        /// Dynamic-function
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedFunctionCall pars) {
            // To code explorer
            if (_isBaseFile)
                ParserHandler.ParsedExplorerItemsList.Add(new CodeExplorerItem() {
                    DisplayText = pars.Name,
                    Branch = CodeExplorerBranch.DynamicFunctionCall,
                    IconType = pars.ExternalCall ? CodeExplorerIconType.FunctionCallExternal : CodeExplorerIconType.FunctionCallInternal,
                    GoToLine = pars.Line,
                    IsNotBlock = true
                });
        }

        /// <summary>
        /// Tables used in the program
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedFoundTableUse pars) {
            if (_isBaseFile) {
                bool missingDbName = pars.Name.IndexOf('.') < 0;
                var name = pars.Name.Split('.');
                ParserHandler.ParsedExplorerItemsList.Add(new CodeExplorerItem() {
                    DisplayText = missingDbName ? pars.Name : name[1],
                    Branch = CodeExplorerBranch.TableUsed,
                    IconType = CodeExplorerIconType.Table,
                    GoToLine = pars.Line,
                    Flag = missingDbName ? CodeExplorerFlag.MissingDbName : 0,
                    IsNotBlock = true
                });
            }
        }

        /// <summary>
        /// Include files
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedIncludeFile pars) {
            // To code explorer
            if (_isBaseFile)
                ParserHandler.ParsedExplorerItemsList.Add(new CodeExplorerItem() {
                    DisplayText = pars.Name,
                    Branch = CodeExplorerBranch.Include,
                    GoToLine = pars.Line,
                    IsNotBlock = true
                });

            // Parse the include file, dont forget to flag the items as External
            // dont forget to fill DefinedProcedures
        }



        /// <summary>
        /// Main block, definitions block...
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedBlock pars) {
            // to code explorer
            if (_isBaseFile)
                ParserHandler.ParsedExplorerItemsList.Add(new CodeExplorerItem {
                    DisplayText = pars.Name,
                    Branch = pars.Branch,
                    IconType = pars.IconIconType,
                    GoToLine = pars.Line,
                    Level = pars.IsRoot ? 0 : 1
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
            if (_isBaseFile)
                ParserHandler.ParsedExplorerItemsList.Add(new CodeExplorerItem() {
                    DisplayText = string.Join(" ", pars.On.ToUpper(), pars.Name),
                    Branch = CodeExplorerBranch.OnEvent,
                    GoToLine = pars.Line,
                    Flag = pars.TooLongForAppbuilder ? CodeExplorerFlag.IsTooLong : 0,
                    SubString = pars.TooLongForAppbuilder ? BlockTooLongString + " (+" + NbExtraCharBetweenLines(pars.Line, pars.EndLine) + ")" : null
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
            if (_isBaseFile)
                ParserHandler.ParsedExplorerItemsList.Add(new CodeExplorerItem() {
                    DisplayText = pars.Name,
                    Branch = CodeExplorerBranch.Function,
                    GoToLine = pars.Line,
                    Flag = pars.TooLongForAppbuilder ? CodeExplorerFlag.IsTooLong : 0,
                    SubString = pars.TooLongForAppbuilder ? BlockTooLongString + " (+" + NbExtraCharBetweenLines(pars.Line, pars.EndLine) + ")" : null
                });

            // to completion data
            pars.ReturnType = ParserHandler.ConvertStringToParsedPrimitiveType(pars.ParsedReturnType, false);
            ParserHandler.ParsedItemsList.Add(new CompletionData() {
                DisplayText = pars.Name,
                Type = CompletionType.Function,
                SubString = pars.ReturnType.ToString(),
                Flag = (pars.IsPrivate ? ParseFlag.Private : 0) | (pars.IsExtended ? ParseFlag.Extent : 0),
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
            if (!DefinedProcedures.ContainsKey(pars.Name))
                DefinedProcedures.Add(pars.Name, false);

            // to code explorer
            if (_isBaseFile)
                ParserHandler.ParsedExplorerItemsList.Add(new CodeExplorerItem() {
                    DisplayText = pars.Name,
                    Branch = CodeExplorerBranch.Procedure,
                    GoToLine = pars.Line,
                    Flag = pars.TooLongForAppbuilder ? CodeExplorerFlag.IsTooLong : 0,
                    SubString = pars.TooLongForAppbuilder ? BlockTooLongString + " (+" + NbExtraCharBetweenLines(pars.Line, pars.EndLine) + ")" : null
                });

            // to completion data
            ParserHandler.ParsedItemsList.Add(new CompletionData() {
                DisplayText = pars.Name,
                Type = CompletionType.Procedure,
                SubString = !_isBaseFile ? _currentParsedFile : string.Empty,
                Flag = pars.IsExternal ? ParseFlag.ExternalProc : 0,
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
            ParserHandler.ParsedItemsList.Add(new CompletionData() {
                DisplayText = "&" + pars.Name,
                Type = CompletionType.Preprocessed,
                SubString = !_isBaseFile ? _currentParsedFile : string.Empty,
                Flag = pars.Scope == ParsedScope.File ? ParseFlag.FileScope : ParseFlag.LocalScope,
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

            ParserHandler.ParsedItemsList.Add(new CompletionData() {
                DisplayText = pars.Name,
                Type = CompletionType.Label,
                SubString = !_isBaseFile ? _currentParsedFile : string.Empty,
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
                    subString = foundTable.Name.AutoCaseToUserLiking();
                    type = foundTable.IsTempTable ? CompletionType.TempTable : CompletionType.Table;

                    // To code explorer, list buffers and associated tables
                    if (_isBaseFile)
                        ParserHandler.ParsedExplorerItemsList.Add(new CodeExplorerItem() {
                            DisplayText = foundTable.Name,
                            Branch = CodeExplorerBranch.TableUsed,
                            IconType = CodeExplorerIconType.TempTable,
                            Flag = pars.BufferFor.IndexOf('.') >= 0 ? 0 : CodeExplorerFlag.MissingDbName,
                            GoToLine = pars.Line,
                            IsNotBlock = true
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
                            ParserHandler.ParsedExplorerItemsList.Add(new CodeExplorerItem() {
                                DisplayText = pars.Name,
                                Branch = CodeExplorerBranch.ProgramParameter,
                                IconType = CodeExplorerIconType.Parameter,
                                SubString = subString,
                                GoToLine = pars.Line,
                                IsNotBlock = true
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
                if (_isBaseFile)
                    ParserHandler.ParsedExplorerItemsList.Add(new CodeExplorerItem() {
                        DisplayText = pars.Name,
                        Branch = CodeExplorerBranch.Browse,
                        GoToLine = pars.Line,
                    });
            }

            ParserHandler.ParsedItemsList.Add(new CompletionData() {
                DisplayText = pars.Name,
                Type = type,
                SubString = subString,
                Flag = SetFlags(flag, pars.LcFlagString),
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
                    // handles the use-index, for now only add the isPrimary flag to the field...
                    if (!string.IsNullOrEmpty(pars.UseIndex)) {
                        // add the fields of the found table (minus the primary information)
                        subStr = @"Like " + foundTable.Name;
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
                        pars.Fields = foundTable.Fields.ToList();
                    }
                }
            }

            ParserHandler.ParsedItemsList.Add(new CompletionData() {
                DisplayText = pars.Name,
                Type = CompletionType.TempTable,
                SubString = subStr,
                Flag = SetFlags(0, pars.LcFlagString),
                Ranking = ParserHandler.FindRankingOfParsedItem(pars.Name),
                ParsedItem = pars,
                FromParser = true
            });
        }

        #endregion


        #region helper

        /// <summary>
        /// Determines flags
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="lcFlagString"></param>
        /// <returns></returns>
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
        /// <param name="startLine"></param>
        /// <param name="endLine"></param>
        /// <returns></returns>
        private bool HasTooMuchChar(int startLine, int endLine) {
            if (!_isBaseFile) return false;
            return NbExtraCharBetweenLines(startLine, endLine) > 0;
        }

        /// <summary>
        /// returns the number of chars between two lines in the current document
        /// </summary>
        /// <param name="startLine"></param>
        /// <param name="endLine"></param>
        /// <returns></returns>
        private static int NbExtraCharBetweenLines(int startLine, int endLine) {
            return (Npp.GetPositionFromLine(endLine) - Npp.GetPositionFromLine(startLine)) - Config.Instance.GlobalMaxNbCharInBlock;
        }

        #endregion

    }
}
