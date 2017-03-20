using System;
using System.Collections.Generic;
using System.Linq;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletionFeature;

namespace _3PA.MainFeatures.Parser {

    internal static class ParserUtils {


        #region find table, buffer, temptable

        /// <summary>
        /// finds a ParsedTable for the input name, it can either be a database table,
        /// a temptable, or a buffer name (in which case we return the associated table)
        /// </summary>
        public static ParsedTable FindAnyTableOrBufferByName(string name, List<ParsedItem> parsedItems) {
            // temptable or table
            var foundTable = FindAnyTableByName(name, parsedItems);
            if (foundTable != null)
                return foundTable;
            // for buffer, we return the referenced temptable/table (stored in CompletionItem.SubString)
            var foundBuffer = parsedItems.Find(data => {
                var def = data as ParsedDefine;
                return def != null && def.Type == ParseDefineType.Buffer && def.Name.EqualsCi(name);
            }) as ParsedDefine;
            return foundBuffer != null ? FindAnyTableByName(foundBuffer.BufferFor, parsedItems) : null;
        }

        /// <summary>
        /// Find the table referenced among database and defined temp tables; 
        /// name is the table's name (can also be BASE.TABLE)
        /// </summary>
        private static ParsedTable FindAnyTableByName(string name, List<ParsedItem> parsedItems) {
            return DataBase.FindTableByName(name) ?? FindTempTableByName(name, parsedItems);
        }

        /// <summary>
        /// Find a temptable by name
        /// </summary>
        private static ParsedTable FindTempTableByName(string name, List<ParsedItem> parsedItems) {
            return parsedItems.Find(item => {
                var tt = item as ParsedTable;
                return tt != null && tt.IsTempTable && tt.Name.EqualsCi(name);
            }) as ParsedTable;
        }

        #endregion

        #region find primitive type

        /// <summary>
        /// convertion
        /// </summary>
        public static ParsedPrimitiveType ConvertStringToParsedPrimitiveType(string str, bool analyseLike, List<ParsedItem> parsedItems) {
            str = str.ToLower();
            // LIKE
            if (analyseLike)
                return FindPrimitiveTypeOfLike(str, parsedItems);
            return ConvertStringToParsedPrimitiveType(str);
        }

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
                    var token1 = str;
                    foreach (var typ in Enum.GetNames(typeof(ParsedPrimitiveType)).Where(typ => token1.Equals(typ.ToLower()))) {
                        return (ParsedPrimitiveType)Enum.Parse(typeof(ParsedPrimitiveType), typ, true);
                    }
                    break;
            }

            // try to find the complete word in abbreviations list
            var completeStr = Keywords.Instance.GetFullKeyword(str);
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
        private static ParsedPrimitiveType FindPrimitiveTypeOfLike(string likeStr, List<ParsedItem> parsedItems) {
            // determines the format
            var nbPoints = likeStr.CountOccurences(".");
            var splitted = likeStr.Split('.');

            // if it's another var
            if (nbPoints == 0) {

                var foundVar = parsedItems.Find(data => {
                    var def = data as ParsedDefine;
                    return def != null && def.Type != ParseDefineType.Buffer && def.PrimitiveType != ParsedPrimitiveType.Unknow && def.Name.EqualsCi(likeStr);
                }) as ParsedDefine;
                return foundVar != null ? foundVar.PrimitiveType : ParsedPrimitiveType.Unknow;
            }

            // Search the databases
            var foundField = DataBase.FindFieldByName(likeStr);
            if (foundField != null)
                return foundField.Type;

            var tableName = splitted[nbPoints == 2 ? 1 : 0];
            var fieldName = splitted[nbPoints == 2 ? 2 : 1];

            // Search in temp tables
            if (nbPoints != 1)
                return ParsedPrimitiveType.Unknow;

            var foundTtable = FindAnyTableOrBufferByName(tableName, parsedItems);
            if (foundTtable == null)
                return ParsedPrimitiveType.Unknow;

            var foundTtField = foundTtable.Fields.Find(field => field.Name.EqualsCi(fieldName));
            return foundTtField == null ? ParsedPrimitiveType.Unknow : foundTtField.Type;
        }

        #endregion
    }
}
