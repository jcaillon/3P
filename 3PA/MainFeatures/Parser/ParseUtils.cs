#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (ParseUtils.cs) is part of 3P.
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
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletionFeature;

namespace _3PA.MainFeatures.Parser {

    /// <summary>
    /// Static class that provide utilities to the parser
    /// </summary>
    internal static class ParserUtils {

        #region find primitive type

        /// <summary>
        /// conversion
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
            var foundField = DataBase.Instance.FindFieldByName(likeStr);
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

        #region find table, buffer, temptable

        /// <summary>
        /// finds a ParsedTable for the input name, it can either be a database table,
        /// a temptable, or a buffer name (in which case we return the associated table)
        /// </summary>
        private static ParsedTable FindAnyTableOrBufferByName(string name, List<ParsedItem> parsedItems) {
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
        public static ParsedTable FindAnyTableByName(string name, List<ParsedItem> parsedItems) {
            return DataBase.Instance.FindTableByName(name) ?? FindTempTableByName(name, parsedItems);
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
    }
}