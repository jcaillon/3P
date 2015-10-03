using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _3PA.Lib;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.AutoCompletion {
    public class DataBase {

        private static List<ParsedDataBase> _dataBases = new List<ParsedDataBase>();
        private static string _filePath;
        private static string _location = Npp.GetConfigDir();
        private static string _fileName = "database_out.txt";

        /// <summary>
        /// Should be called to extract the database info from the current environnement database_out file
        /// if the database_out file doesn't exists, start a progress program to extract it
        /// </summary>
        public static void FetchCurrentDbInfo() {
            //TODO: read the .txt that matchs with the current db connected
            _filePath = Path.Combine(_location, _fileName);
            Read();

            // Update autocompletion
            AutoComplete.FillStaticItems(false);
        }

        /// <summary>
        /// This method parses the output of the .p procedure that exports the database info
        /// and fills _dataBases
        /// </summary>
        private static void Read() {
            if (!File.Exists(_filePath)) return;
            _dataBases.Clear();
            try {
                ParsedDataBase currentDb = null;
                ParsedTable currentTable = null;
                foreach (var items in File.ReadAllLines(_filePath).Where(items => items.Length > 1 && !items[0].Equals('#'))) {
                    var splitted = items.Split('\t');
                    switch (items[0]) {
                        case 'H':
                            // base
                            //#H|<Dump date ISO 8601>|<Dump time>|<Logical DB name>|<Physical DB name>|<Progress version>
                            if (splitted.Count() != 6) continue;
                            currentDb = new ParsedDataBase(
                                splitted[3],
                                splitted[4],
                                splitted[5],
                                new List<ParsedTable>());
                            _dataBases.Add(currentDb);
                            break;
                        case 'T':
                            // table
                            //#T|<Table name>|<Table ID>|<Table CRC>|<Dump name>|<Description>
                            if (splitted.Count() != 6 || currentDb == null) continue;
                            currentTable = new ParsedTable(
                                splitted[1],
                                0, 0,
                                splitted[2],
                                splitted[3],
                                splitted[4],
                                splitted[5],
                                "", false,
                                new List<ParsedField>(),
                                new List<ParsedIndex>(),
                                new List<ParsedTrigger>()
                                , "", "");
                            currentDb.Tables.Add(currentTable);
                            break;
                        case 'X':
                            // trigger
                            //#X|<Parent table>|<Event>|<Proc name>|<Trigger CRC>
                            if (splitted.Count() != 5 || currentTable == null) continue;
                            currentTable.Triggers.Add(new ParsedTrigger(
                                splitted[2],
                                splitted[3]));
                            break;
                        case 'I':
                            // index
                            //#I|<Parent table>|<Index name>|<Primary? 0/1>|<Unique? 0/1>|<Index CRC>|<Fileds separated with %>
                            if (splitted.Count() != 7 || currentTable == null) continue;
                            var flag = splitted[3].Equals("1") ? ParsedIndexFlag.Primary : ParsedIndexFlag.None;
                            if (splitted[4].Equals("1")) flag = flag | ParsedIndexFlag.Unique;
                            currentTable.Indexes.Add(new ParsedIndex(
                                splitted[2],
                                flag,
                                splitted[6].Split('%').ToList()));
                            break;
                        case 'F':
                            // field
                            //#F|<Parent table>|<Field name>|<Type>|<Format>|<Order #>|<Mandatory? 0/1>|<Extent? 0/1>|<Part of index? 0/1>|<Part of PK? 0/1>|<Initial value>|<Desription>
                            if (splitted.Count() != 12 || currentTable == null) continue;
                            var flag2 = splitted[6].Equals("1") ? ParsedFieldFlag.Mandatory : ParsedFieldFlag.None;
                            if (splitted[7].Equals("1")) flag2 = flag2 | ParsedFieldFlag.Extent;
                            if (splitted[8].Equals("1")) flag2 = flag2 | ParsedFieldFlag.Index;
                            if (splitted[9].Equals("1")) flag2 = flag2 | ParsedFieldFlag.Primary;
                            var curField = new ParsedField(
                                splitted[2],
                                splitted[3],
                                splitted[4],
                                int.Parse(splitted[5]),
                                flag2,
                                splitted[10],
                                splitted[11],
                                ParsedAsLike.None);
                            curField.Type = ParserHandler.ConvertStringToParsedPrimitiveType(curField.TempType, false);
                            currentTable.Fields.Add(curField);
                            break;
                    }
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error while loading database info!", _filePath);
            }
        }

        /// <summary>
        /// Exposes the databases info
        /// </summary>
        /// <returns></returns>
        public static List<ParsedDataBase> Get() {
            return _dataBases;
        }

        /// <summary>
        /// returns the list of databases
        /// </summary>
        public static List<CompletionData> GetDbList() {
            if (_dataBases.Count <= 0) return new List<CompletionData>();
            return _dataBases.Select(@base => new CompletionData() {
                DisplayText = @base.LogicalName.ToUpper(),
                Type = CompletionType.Databases,
                FromParser = false,
                Ranking = ParserHandler.FindRankingOfDatabaseItem(@base.LogicalName),
                Flag = 0
            }).ToList();
        }

        /// <summary>
        /// returns the list tables of each database
        /// </summary>
        /// <returns></returns>
        public static List<CompletionData> GetTablesList() {
            var output = new List<CompletionData>();
            foreach (var dataBase in _dataBases)
                output.AddRange(GetTablesList(dataBase));
            return output;
        }

        /// <summary>
        /// Returns the list of tables for a given database
        /// </summary>
        /// <param name="dataBase"></param>
        /// <returns></returns>
        public static List<CompletionData> GetTablesList(ParsedDataBase dataBase) {
            var output = new List<CompletionData>();
            if (dataBase == null || dataBase.Tables == null || dataBase.Tables.Count == 0) return output;
            output.AddRange(dataBase.Tables.Select(table => new CompletionData() {
                DisplayText = table.Name.ToUpper(),
                SubString = dataBase.LogicalName.AutoCaseToUserLiking(),
                Type = CompletionType.Table,
                FromParser = false,
                Ranking = ParserHandler.FindRankingOfDatabaseItem(table.Name),
                Flag = 0
            }).ToList());
            return output;
        }

        /// <summary>
        /// Returns the list of fields for a given table (it can also be a temp table!)
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<CompletionData> GetFieldsList(ParsedTable table) {
            var output = new List<CompletionData>();
            if (table == null) return output;
            output.AddRange(table.Fields.Select(field => new CompletionData() {
                DisplayText = field.Name.ToUpper(),
                Type = (field.Flag.HasFlag(ParsedFieldFlag.Primary)) ? CompletionType.FieldPk : CompletionType.Field, 
                FromParser = false,
                SubString = field.Type.ToString(),
                Ranking = ParserHandler.FindRankingOfDatabaseItem(field.Name),
                Flag = (field.Flag.HasFlag(ParsedFieldFlag.Mandatory) ? ParseFlag.Mandatory : 0) |
                    (field.Flag.HasFlag(ParsedFieldFlag.Index) ? ParseFlag.Index : 0) |
                    (field.Flag.HasFlag(ParsedFieldFlag.Extent) ? ParseFlag.Extent : 0)
            }));
            return output;
        }

        public static ParsedDataBase FindDatabaseByName(string name) {
            return _dataBases.Find(@base => @base.LogicalName.EqualsCi(name));
        }

        public static ParsedTable FindTableByName(string name, ParsedDataBase db) {
            return db.Tables.Find(table => table.Name.EqualsCi(name));
        }

        public static ParsedTable FindTableByName(string name) {
            return _dataBases.Select(dataBase => FindTableByName(name, dataBase)).FirstOrDefault(found => found != null);
        }

        public static ParsedField FindFieldByName(string name, ParsedTable table) {
            return table.Fields.Find(field => field.Name.EqualsCi(name));
        }

        public static ParsedField FindFieldByName(string name) {
            return (from dataBase in _dataBases where dataBase.Tables != null from table in dataBase.Tables select FindFieldByName(name, table)).FirstOrDefault(found => found != null);
        }
    }
}
