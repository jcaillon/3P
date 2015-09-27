using System;
using System.Collections.Generic;
using System.Linq;
using _3PA.Lib;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.AutoCompletion {

    class ParserHandler {
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
        /// contains the list of items that depend on the current file, that list
        /// is updated by the parser's visitor class
        /// </summary>
        public static List<CompletionData> ParsedItemsList;

        private static Parser.Parser _ablParser;

        /// <summary>
        /// is used to make sure that 2 different threads dont try to access
        /// the same resource (_ablParser) at the same time, which would be problematic
        /// </summary>
        private static object _thisLock = new object();

        /// <summary>
        /// Returns the owner name (currentScopeName) of the caret line
        /// </summary>
        /// <returns></returns>
        public static string GetCarretLineLcOwnerName {
            get {
                var line = Npp.GetCaretLineNumber();
                if (_ablParser == null) return "";
                lock (_thisLock) {
                    return !_ablParser.GetLineInfo.ContainsKey(line) ? "" : _ablParser.GetLineInfo[line].CurrentScopeName;
                }
            }
        }

        /// <summary>
        /// this method should be called to refresh the Items list with all the static items
        /// as well as the dynamic items found by the parser
        /// </summary>
        public static void RefreshParser() {
            if (ParsedItemsList == null) ParsedItemsList = new List<CompletionData>();

            // we launch the parser, that will fill the DynamicItems
            lock (_thisLock) {
                _ablParser = new Parser.Parser(Npp.GetDocumentText(), Npp.GetCurrentFilePath());
                ParsedItemsList.Clear();
                _ablParser.Accept(new AutoCompParserVisitor());
            }

        }

        /// <summary>
        /// Find a temptable by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ParsedTable FindTempTableByName(string name) {
            var foundTable = ParsedItemsList.Find(data => (data.Type == CompletionType.TempTable) && data.DisplayText.EqualsCi(name));
            if (foundTable != null) return (ParsedTable)foundTable.ParsedItem;
            return null;
        }

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
        /// finds a ParsedTable for the input name, it can either be a database table,
        /// a temptable, or a buffer name (in which case we return the associated table)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ParsedTable FindAnyTableOrBufferByName(string name) {
            var foundTable = FindAnyTableByName(name);
            if (foundTable != null)
                return foundTable;
            var foundParsedItem = ParsedItemsList.Find(data => (data.Type == CompletionType.Table || data.Type == CompletionType.TempTable) && data.DisplayText.EqualsCi(name));
            return foundParsedItem != null ? FindAnyTableByName(foundParsedItem.SubString) : null;
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
                var foundVar = ParsedItemsList.Find(data =>
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
            if (nbPoints != 1) return ParsedPrimitiveType.Unknow;
            var foundTtable = FindTempTableByName(likeStr);
            if (foundTtable == null) return ParsedPrimitiveType.Unknow;

            var foundTtField = foundTtable.Fields.Find(field => field.Name.EqualsCi(fieldName));
            return foundTtField == null ? ParsedPrimitiveType.Unknow : foundTtField.Type;
        }

    }
}
