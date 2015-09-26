using System;
using System.Linq;
using _3PA.Lib;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.AutoCompletion {

    /// <summary>
    /// This class sustains the autocompletion list AND the code explorer list
    /// by visiting the parser and creating new completionData
    /// </summary>
    class AutoCompParserVisitor : IParserVisitor{

        /// <summary>
        /// Functions
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedFunction pars) {
            var flag = ParseFlag.None;
            if (pars.IsPrivate) flag = flag | ParseFlag.Private;
            ParserHandler.DynamicItems.Add(new CompletionData() {
                DisplayText = pars.Name,
                Type = CompletionType.Function,
                SubType = "",
                Flag = flag,
                Ranking = ParserHandler.FindRankingOfDynamic(pars.Name),
                ParsedItem = pars,
                FromParser = true
            });
        }

        /// <summary>
        /// Procedures
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedProcedure pars) {
            ParserHandler.DynamicItems.Add(new CompletionData() {
                DisplayText = pars.Name,
                Type = CompletionType.Procedure,
                SubType = "",
                Flag = ParseFlag.None,
                Ranking = ParserHandler.FindRankingOfDynamic(pars.Name),
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

            // Parse the include file
        }

        /// <summary>
        /// Preprocessed variables
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedPreProc pars) {
            var flag = ParseFlag.None;
            flag = flag | (pars.Scope == ParsedScope.Global ? ParseFlag.FileScope : ParseFlag.LocalScope);
            ParserHandler.DynamicItems.Add(new CompletionData() {
                DisplayText = pars.Name,
                Type = CompletionType.Preprocessed,
                SubType = "",
                Flag = flag,
                Ranking = ParserHandler.FindRankingOfDynamic(pars.Name),
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
            var flag = ParseFlag.None;
            if (pars.Scope == ParsedScope.Global) flag = flag | ParseFlag.FileScope;
            else flag = flag | ParseFlag.LocalScope;
            if (pars.Type == ParseDefineType.Parameter) flag = flag | ParseFlag.Parameter;

            // find primitive type
            var hasPrimitive = !string.IsNullOrEmpty(pars.TempPrimitiveType);
            if (hasPrimitive)
                pars.PrimitiveType = ConvertStringToParsedPrimitiveType(pars.TempPrimitiveType, !pars.LcAsLike.Equals("as"));

            // which completionData type is it?
            CompletionType type;
            if (pars.Type == ParseDefineType.Parameter)
                type = CompletionType.UserVariablePrimitive;
            else if (pars.Type == ParseDefineType.Variable) {
                if ((int)pars.PrimitiveType < 30)
                    type = CompletionType.UserVariablePrimitive;
                else if (!string.IsNullOrEmpty(pars.ViewAs))
                    type = CompletionType.Widget;
                else
                    type = CompletionType.UserVariableOther;
            }
            else if (pars.Type == ParseDefineType.Button ||
                pars.Type == ParseDefineType.Browse ||
                pars.Type == ParseDefineType.Frame ||
                pars.Type == ParseDefineType.Image ||
                pars.Type == ParseDefineType.SubMenu ||
                pars.Type == ParseDefineType.Menu ||
                pars.Type == ParseDefineType.Rectangle)
                type = CompletionType.Widget;
            else
                type = CompletionType.UserVariableOther;

            ParserHandler.DynamicItems.Add(new CompletionData() {
                DisplayText = pars.Name,
                Type = type,
                SubType = hasPrimitive ? pars.PrimitiveType.ToString() : pars.Type.ToString(),
                Flag = SetFlags(flag, pars.LcFlagString),
                Ranking = ParserHandler.FindRankingOfDynamic(pars.Name),
                ParsedItem = pars,
                FromParser = true
            });
        }

        /// <summary>
        /// Defined Temptables
        /// </summary>
        /// <param name="pars"></param>
        public void Visit(ParsedTable pars) {

            // find all primitive types
            foreach (var parsedField in pars.Fields)
                parsedField.Type = ConvertStringToParsedPrimitiveType(parsedField.TempType, !parsedField.LcAsLike.Equals("as"));

            ParserHandler.DynamicItems.Add(new CompletionData() {
                DisplayText = pars.Name,
                Type = CompletionType.TempTable,
                SubType = "",
                Flag = SetFlags(ParseFlag.None, pars.LcFlagString),
                Ranking = ParserHandler.FindRankingOfDynamic(pars.Name),
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
        private ParseFlag SetFlags(ParseFlag flag, string lcFlagString) {
            if (lcFlagString.Contains("global")) flag = flag | ParseFlag.Global;
            if (lcFlagString.Contains("shared")) flag = flag | ParseFlag.Shared;
            if (lcFlagString.Contains("private")) flag = flag | ParseFlag.Private;
            if (lcFlagString.Contains("new")) flag = flag | ParseFlag.Private;
            return flag;
        }

        /// <summary>
        /// convertion
        /// </summary>
        /// <param name="str"></param>
        /// <param name="analyseLike"></param>
        /// <returns></returns>
        public static ParsedPrimitiveType ConvertStringToParsedPrimitiveType(string str, bool analyseLike) {
            str = str.ToLower();
            // LIKE
            if (analyseLike) {
                return FindPrimitiveTypeOfLike(str);
            }

            // AS
            switch (str) {
                case "com-handle":
                    return ParsedPrimitiveType.Comhandle;
                case "datetime-tz":
                    return ParsedPrimitiveType.Comhandle;
                case "unsigned-short":
                    return ParsedPrimitiveType.UnsignedShort;
                case "unsigned-long":
                    return ParsedPrimitiveType.UnsignedLong;
                case "table-handle":
                    return ParsedPrimitiveType.TableHandle;
                case "dataset-handle":
                    return ParsedPrimitiveType.DatasetHandle;
                default:
                    var token1 = str;
                    foreach (var typ in Enum.GetNames(typeof(ParsedPrimitiveType)).Where(typ => token1.Equals(typ.ToLower()))) {
                        return (ParsedPrimitiveType)Enum.Parse(typeof(ParsedPrimitiveType), typ, true);
                    }
                    break;
            }
            // try to find the complete word in abbreviations list
            var completeStr = Keywords.GetFullKeyword(str).ToLower();
            if (completeStr != str)
                foreach (var typ in Enum.GetNames(typeof(ParsedPrimitiveType)).Where(typ => completeStr.Equals(typ.ToLower()))) {
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
        public static ParsedPrimitiveType FindPrimitiveTypeOfLike(string likeStr) {
            // determines the format
            var nbPoints = likeStr.CountOccurences(".");
            var splitted = likeStr.Split('.');

            // if it's another var
            if (nbPoints == 0) {
                var foundVar = ParserHandler.DynamicItems.Find(data =>
                    (data.Type == CompletionType.UserVariablePrimitive ||
                     data.Type == CompletionType.UserVariableOther) && data.DisplayText.EqualsCi(likeStr));
                return foundVar != null ? ((ParsedDefine)foundVar.ParsedItem).PrimitiveType : ParsedPrimitiveType.Unknow;
            }

            var tableName = splitted[nbPoints == 2 ? 1 : 0];
            var fieldName = splitted[nbPoints == 2 ? 2 : 1];

            // Search through database
            if (DataBase.Get().Count > 0) {
                ParsedDataBase foundDb = DataBase.Get().First();
                if (nbPoints == 2)
                    // find database
                    foundDb = DataBase.FindDatabaseByName(splitted[0]) ?? DataBase.Get().First();
                if (foundDb == null) return ParsedPrimitiveType.Unknow;

                // find table
                var foundTable = DataBase.FindTableByName(tableName, foundDb);
                if (foundTable == null) return ParsedPrimitiveType.Unknow;

                // find field
                var foundField = DataBase.FindFieldByName(fieldName, foundTable);
                return foundField == null ? ParsedPrimitiveType.Unknow : foundField.Type;
            } 

            // Search in temp tables
            if (nbPoints == 1) {
                var foundTtable = ParserHandler.DynamicItems.Find(data =>
                    (data.Type == CompletionType.TempTable) && data.DisplayText.EqualsCi(likeStr));
                if (foundTtable == null) return ParsedPrimitiveType.Unknow;

                var foundTtField = ((ParsedTable)foundTtable.ParsedItem).Fields.Find(field => field.Name.EqualsCi(fieldName));
                return foundTtField == null ? ParsedPrimitiveType.Unknow : foundTtField.Type;
            }

            return ParsedPrimitiveType.Unknow;
        }
    }
}
