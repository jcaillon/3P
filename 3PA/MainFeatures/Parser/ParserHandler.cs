#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (ParserHandler.cs) is part of 3P.
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

namespace _3PA.MainFeatures.Parser {

    internal static class ParserHandler {

        #region fields

        /// <summary>
        /// This dictionnary is what is used to remember the ranking of each word for the current session
        /// (otherwise this info is lost since we clear the ParsedItemsList each time we parse!)
        /// </summary>
        public static Dictionary<string, int> DisplayTextRankingParsedItems = new Dictionary<string, int>();

        /// <summary>
        /// Same as above but for static stuff (= database)
        /// (it is especially useful for fields because we recreate the list each time!
        /// otherwise it is not that useful indeed)
        /// </summary>
        public static Dictionary<string, int> DisplayTextRankingDatabase = new Dictionary<string, int>();

        /// <summary>
        /// Instead of parsing the include files each time we store the results of the parsing to use them when we need it
        /// </summary>
        public static Dictionary<string, ParserVisitor> SavedParserVisitors = new Dictionary<string, ParserVisitor>();

        public static string LastParsedFilePath;

        private static Parser _ablParser;

        private static ParserVisitor _parserVisitor;
        #endregion

        #region misc

        /// <summary>
        /// Returns the owner name (currentScopeName) of the caret line
        /// </summary>
        /// <returns></returns>
        public static string GetCarretLineOwnerName(int line) {
            if (_ablParser == null) return "";
            return !_ablParser.GetLineInfo.ContainsKey(line) ? string.Empty : _ablParser.GetLineInfo[line].CurrentScopeName;
        }

        /// <summary>
        /// Returns a list of "parameters" for a given internal procedure
        /// </summary>
        /// <param name="procedureData"></param>
        /// <returns></returns>
        public static List<CompletionData> FindProcedureParameters(CompletionData procedureData) {
            var parserVisitor = ParserVisitor.ParseFile(procedureData.ParsedItem.FilePath, "");
            return parserVisitor.ParsedItemsList.Where(data =>
                data.FromParser &&
                data.ParsedItem.OwnerName.EqualsCi(procedureData.DisplayText) &&
                (data.Type == CompletionType.VariablePrimitive || data.Type == CompletionType.VariableComplex || data.Type == CompletionType.Widget) &&
                ((ParsedDefine)data.ParsedItem).Type == ParseDefineType.Parameter).ToList();
        }

        /// <summary>
        /// Returns true if the parser detected a syntax correct enough for it to indent the ABL code of the parsed document
        /// </summary>
        /// <returns></returns>
        public static bool CanIndent() {
            if (_ablParser == null) return false;
            return _ablParser.ParsingOk;
        }

        /// <summary>
        /// Returns true if the parser detected a syntax correct enough for it to indent the ABL code of the parsed document
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, LineInfo> GetLineInfo() {
            if (_ablParser == null) return null;
            return _ablParser.GetLineInfo;
        }

        #endregion

        #region do the parsing and get the results

        /// <summary>
        /// this method should be called to refresh the Items list with all the static items
        /// as well as the dynamic items found by the parser
        /// </summary>
        public static void RefreshParser() {
            // we launch the parser, that will fill the DynamicItems
            try {
                // if this document is in the Saved parsed visitors, we remove it because it might change so
                // we want to re parse it later
                LastParsedFilePath = Plug.CurrentFilePath;
                if (SavedParserVisitors.ContainsKey(LastParsedFilePath))
                    SavedParserVisitors.Remove(LastParsedFilePath);

                // Parse the document
                _ablParser = new Parser(Plug.IsCurrentFileProgress ? Npp.Text : string.Empty, LastParsedFilePath, null, true);

                // visitor
                _parserVisitor = new ParserVisitor(true, Path.GetFileName(LastParsedFilePath), _ablParser.GetLineInfo);
                _ablParser.Accept(_parserVisitor);

                // correct the internal/external type of run statements :
                foreach (var item in _parserVisitor.ParsedExplorerItemsList.Where(item => item.Branch == CodeExplorerBranch.Run)) {
                    if (_parserVisitor.DefinedProcedures.Contains(item.DisplayText))
                        item.IconType = CodeExplorerIconType.RunInternal;
                }

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in RefreshParser");
            }
        }

        /// <summary>
        /// List of parsed items
        /// </summary>
        /// <returns></returns>
        public static List<CompletionData> GetParsedItemsList() {
            return _parserVisitor != null ? _parserVisitor.ParsedItemsList.ToList() : new List<CompletionData>();
        }

        /// <summary>
        /// List of parsed explorer items
        /// </summary>
        /// <returns></returns>
        public static List<CodeExplorerItem> GetParsedExplorerItemsList() {
            return _parserVisitor != null ? _parserVisitor.ParsedExplorerItemsList.ToList() : new List<CodeExplorerItem>();
        }
        #endregion

        #region handling item ranking

        /// <summary>
        /// Find ranking of a parsed item
        /// </summary>
        /// <param name="displayText"></param>
        /// <returns></returns>
        public static int FindRankingOfParsedItem(string displayText) {
            return DisplayTextRankingParsedItems.ContainsKey(displayText) ? DisplayTextRankingParsedItems[displayText] : 0;
        }

        /// <summary>
        /// Find ranking of a database item
        /// </summary>
        /// <param name="displayText"></param>
        /// <returns></returns>
        public static int FindRankingOfDatabaseItem(string displayText) {
            return DisplayTextRankingDatabase.ContainsKey(displayText) ? DisplayTextRankingDatabase[displayText] : 0;
        }

        /// <summary>
        /// remember the use of a particular item in the completion list
        /// (for dynamic items = parsed items)
        /// </summary>
        /// <param name="displayText"></param>
        public static void RememberUseOfParsedItem(string displayText) {
            if (!DisplayTextRankingParsedItems.ContainsKey(displayText))
                DisplayTextRankingParsedItems.Add(displayText, 1);
            else
                DisplayTextRankingParsedItems[displayText]++;
        }

        /// <summary>
        /// remember the use of a particular item in the completion list
        /// (for database items!)
        /// </summary>
        /// <param name="displayText"></param>
        public static void RememberUseOfDatabaseItem(string displayText) {
            if (!DisplayTextRankingDatabase.ContainsKey(displayText))
                DisplayTextRankingDatabase.Add(displayText, 1);
            else
                DisplayTextRankingDatabase[displayText]++;
        }

        #endregion

        #region find table, buffer, temptable

        /// <summary>
        /// finds a ParsedTable for the input name, it can either be a database table,
        /// a temptable, or a buffer name (in which case we return the associated table)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ParsedTable FindAnyTableOrBufferByName(string name) {
            // temptable or table
            var foundTable = FindAnyTableByName(name);
            if (foundTable != null)
                return foundTable;
            // for buffer, we return the referenced temptable/table (stored in CompletionData.SubString)
            if (_parserVisitor != null) {
                var foundParsedItem = _parserVisitor.ParsedItemsList.Find(data => (data.Type == CompletionType.Table || data.Type == CompletionType.TempTable) && data.DisplayText.EqualsCi(name));
                return foundParsedItem != null ? FindAnyTableByName(foundParsedItem.SubString) : null;
            }
            return null;
        }

        /// <summary>
        /// Find the table referenced among database and defined temp tables; 
        /// name is the table's name (can also be BASE.TABLE)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ParsedTable FindAnyTableByName(string name) {
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
        private static ParsedTable FindTempTableByName(string name) {
            if (_parserVisitor != null) {
                var foundTable = _parserVisitor.ParsedItemsList.FirstOrDefault(data => data.Type == CompletionType.TempTable && data.DisplayText.EqualsCi(name));
                if (foundTable != null && foundTable.ParsedItem is ParsedTable)
                    return (ParsedTable) foundTable.ParsedItem;
            }
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
        public static ParsedPrimitiveType ConvertStringToParsedPrimitiveType(string str, bool analyseLike) {
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
                    foreach (var typ in Enum.GetNames(typeof (ParsedPrimitiveType)).Where(typ => token1.Equals(typ.ToLower()))) {
                        return (ParsedPrimitiveType) Enum.Parse(typeof (ParsedPrimitiveType), typ, true);
                    }
                    break;
            }

            // try to find the complete word in abbreviations list
            var completeStr = Keywords.GetFullKeyword(str);
            if (completeStr != null)
                foreach (var typ in Enum.GetNames(typeof (ParsedPrimitiveType)).Where(typ => completeStr.ToLower().Equals(typ.ToLower()))) {
                    return (ParsedPrimitiveType) Enum.Parse(typeof (ParsedPrimitiveType), typ, true);
                }
            return ParsedPrimitiveType.Unknow;
        }

        /// <summary>
        /// Search through the available completionData to find the primitive type of a 
        /// "like xx" phrase
        /// </summary>
        /// <param name="likeStr"></param>
        /// <returns></returns>
        private static ParsedPrimitiveType FindPrimitiveTypeOfLike(string likeStr) {
            // determines the format
            var nbPoints = likeStr.CountOccurences(".");
            var splitted = likeStr.Split('.');

            // if it's another var
            if (nbPoints == 0) {
                var foundVar = _parserVisitor.ParsedItemsList.Find(data =>
                    (data.Type == CompletionType.VariablePrimitive ||
                     data.Type == CompletionType.VariableComplex) && data.DisplayText.EqualsCi(likeStr));
                return foundVar != null ? ((ParsedDefine) foundVar.ParsedItem).PrimitiveType : ParsedPrimitiveType.Unknow;
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

    }
}
