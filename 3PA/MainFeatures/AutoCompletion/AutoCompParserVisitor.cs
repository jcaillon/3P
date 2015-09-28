using _3PA.Lib;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.AutoCompletion {

    /// <summary>
    /// This class sustains the autocompletion list AND the code explorer list
    /// by visiting the parser and creating new completionData
    /// </summary>
    class AutoCompParserVisitor : IParserVisitor{

        /// <summary>
        /// Run statement
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedRun pars) {
            // we only want the RUN that point to external procedures
        }

        /// <summary>
        /// Functions
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedFunction pars) {
            ParserHandler.ParsedItemsList.Add(new CompletionData() {
                DisplayText = pars.Name,
                Type = CompletionType.Function,
                SubString = "",
                Flag = pars.IsPrivate ? ParseFlag.Private : 0,
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
            ParserHandler.ParsedItemsList.Add(new CompletionData() {
                DisplayText = pars.Name,
                Type = CompletionType.Procedure,
                SubString = "",
                Flag = 0,
                Ranking = ParserHandler.FindRankingOfParsedItem(pars.Name),
                ParsedItem = pars,
                FromParser = true
            });
        }

        /// <summary>
        /// ON events
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedOnEvent pars) {
            // To code explorer
        }

        /// <summary>
        /// Include files
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedIncludeFile pars) {
           // To code explorer

            // Parse the include file, dont forget to flag the items as External

        }

        /// <summary>
        /// Preprocessed variables
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedPreProc pars) {
            ParserHandler.ParsedItemsList.Add(new CompletionData() {
                DisplayText = pars.Name,
                Type = CompletionType.Preprocessed,
                SubString = "",
                Flag = pars.Scope == ParsedScope.File ? ParseFlag.FileScope : ParseFlag.LocalScope,
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

            // find primitive type
            var hasPrimitive = !string.IsNullOrEmpty(pars.TempPrimitiveType);
            if (hasPrimitive)
                pars.PrimitiveType = ParserHandler.ConvertStringToParsedPrimitiveType(pars.TempPrimitiveType, !pars.LcAsLike.Equals("as"));

            // which completionData type is it?
            CompletionType type;
            string subString;
            // special case for buffers, they go into the temptable or table section
            if (pars.PrimitiveType == ParsedPrimitiveType.Buffer || pars.Type == ParseDefineType.Buffer) {
                flag = flag | ParseFlag.Buffer;
                subString = "?";
                type = CompletionType.TempTable;

                // find the table or temp table that the buffer is FOR
                var foundTable = ParserHandler.FindAnyTableByName(pars.LcAsLike);
                if (foundTable != null) {
                    subString = Abl.AutoCaseToUserLiking(foundTable.Name);
                    type = foundTable.IsTempTable ? CompletionType.TempTable : CompletionType.Table;
                }
                
            } else {
                // match type for everything else
                subString = hasPrimitive ? pars.PrimitiveType.ToString() : pars.Type.ToString();
                switch (pars.Type) {
                    case ParseDefineType.Parameter:
                        type = CompletionType.VariablePrimitive;
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
                parsedField.Type = ParserHandler.ConvertStringToParsedPrimitiveType(parsedField.TempType, !parsedField.LcAsLike.Equals("as"));

            // temp table is LIKE another table? copy fields, minus the isPrimary,
            if (!string.IsNullOrEmpty(pars.LcLikeTable)) {
                var foundTable = ParserHandler.FindAnyTableByName(pars.LcLikeTable);
                if (foundTable != null) {
                    // add the fields of the found table
                    subStr = @"Like " + foundTable.Name;
                    foreach (var field in foundTable.Fields) {
                        pars.Fields.Add(new ParsedField(field.Name, "", field.Format, field.Order, field.Flag.HasFlag(ParsedFieldFlag.Mandatory) ? ParsedFieldFlag.Mandatory : 0, field.InitialValue, field.Description, field.LcAsLike) {
                            Type = field.Type
                        });
                    }
                    // handles the use-index, for now only add the isPrimary flag to the field...
                    if (!string.IsNullOrEmpty(pars.Description)) {
                        foreach (var index in pars.Description.Split(',')) {
                            if (index.ContainsFast("!")) {
                                // we found a primary index
                                var foundIndex = foundTable.Indexes.Find(index2 => index2.Name.EqualsCi(index.Replace("!", "")));
                                if (foundIndex != null)
                                    foreach (var fieldName in foundIndex.FieldsList) {
                                        // then the field is primary
                                        var foundfield = pars.Fields.Find(field => field.Name.EqualsCi(fieldName.Replace("+", "").Replace("-", "")));
                                        if (foundfield != null) foundfield.Flag = foundfield.Flag | ParsedFieldFlag.Primary;
                                    }
                            }
                        }
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
    }
}
