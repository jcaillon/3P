#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (DataBase.cs) is part of 3P.
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
using _3PA.MainFeatures.Parser;
using _3PA.MainFeatures.Pro;

namespace _3PA.MainFeatures.AutoCompletionFeature {
    internal static class DataBase {

        #region events

        /// <summary>
        /// Event published when the current database information is updated
        /// </summary>
        public static event Action OnDatabaseUpdate;

        #endregion

        #region fields

        /// <summary>
        /// List of Databases (each of which contains list of tables > list of fields/indexes/triggers)
        /// </summary>
        private static List<ParsedDataBase> _dataBases = new List<ParsedDataBase>();

        /// <summary>
        /// List of sequences of the database
        /// </summary>
        private static List<ParsedSequence> _sequences = new List<ParsedSequence>();

        private static List<CompletionItem> _dbItems;

        private static bool _isExtracting;

        /// <summary>
        /// Action called when an extraction is done
        /// </summary>
        private static Action _onExtractionDone;

        #endregion

        #region public methods

        /// <summary>
        /// returns the path of the current dump file
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentDumpPath {
            get { return Path.Combine(Config.FolderDatabase, GetOutputName); }
        }

        /// <summary>
        /// returns true if the database info is available
        /// </summary>
        /// <returns></returns>
        public static bool IsDbInfoAvailable {
            get { return File.Exists(GetCurrentDumpPath); }
        }

        /// <summary>
        /// Tries to load the database information of the current ProgressEnv, 
        /// returns false the info is not available
        /// </summary>
        /// <returns></returns>
        public static void UpdateDatabaseInfo() {
            _dbItems = null;
            if (IsDbInfoAvailable) {
                // read file, extract info
                Read(GetCurrentDumpPath);
            } else {
                // reset
                _dataBases.Clear();
                _sequences.Clear();
            }

            if (OnDatabaseUpdate != null)
                OnDatabaseUpdate();
        }

        /// <summary>
        /// Deletes the file corresponding to the current database (if it exists)
        /// </summary>
        public static void DeleteCurrentDbInfo() {
            if (!Utils.DeleteFile(GetCurrentDumpPath))
                UserCommunication.Notify("Couldn't delete the current database info stored in the file :<br>" + GetCurrentDumpPath.ToHtmlLink(), MessageImg.MsgError, "Delete failed", "Current database info");
            UpdateDatabaseInfo();
        }

        /// <summary>
        /// Should be called to extract the database info from the current environnement
        /// </summary>
        public static void FetchCurrentDbInfo(Action onExtractionDone) {
            try {
                // dont extract 2 db at once
                if (_isExtracting) {
                    UserCommunication.Notify("Already fetching info for another environment, please wait the end of the previous execution!", MessageImg.MsgWarning, "Database info", "Extracting database structure", 5);
                    return;
                }

                // save the filename of the output database info file for this environment
                UserCommunication.Notify("Now fetching info on all the connected databases for the current environment<br>You will be warned when the process is over", MessageImg.MsgInfo, "Database info", "Extracting database structure", 5);

                var exec = new ProExecution {
                    OnExecutionEnd = execution => _isExtracting = false,
                    OnExecutionOk = ExtractionDoneOk,
                    NeedDatabaseConnection = true,
                    ExtractDbOutputPath = GetOutputName
                };
                _onExtractionDone = onExtractionDone;
                _isExtracting = exec.Do(ExecutionType.Database);
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "FetchCurrentDbInfo");
            }
        }

        /// <summary>
        /// Method called after the execution of the program extracting the db info
        /// </summary>
        private static void ExtractionDoneOk(ProExecution lastExec) {
            // copy the dump to the folder database
            if (Utils.CopyFile(lastExec.ExtractDbOutputPath, Path.Combine(Config.FolderDatabase, Path.GetFileName(lastExec.ExtractDbOutputPath) ?? ""))) {
                // update info
                UpdateDatabaseInfo();
                UserCommunication.Notify("Database structure extracted with success!<br>The auto-completion has been updated with the latest info, enjoy!", MessageImg.MsgOk, "Database info", "Extracting database structure", 10);
                if (_onExtractionDone != null) {
                    _onExtractionDone();
                    _onExtractionDone = null;
                }
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Returns the output file name for the current appli/env
        /// </summary>
        /// <returns></returns>
        private static string GetOutputName {
            get { return (Config.Instance.EnvName + "_" + Config.Instance.EnvSuffix + "_" + Config.Instance.EnvDatabase).ToValidFileName().ToLower() + ".dump"; }
        }

        /// <summary>
        /// This method parses the output of the .p procedure that exports the database info
        /// and fills _dataBases
        /// It then updates the parser with the new info
        /// </summary>
        private static void Read(string filePath) {
            if (!File.Exists(filePath)) return;
            _dataBases.Clear();
            _sequences.Clear();

            var defaultToken = new TokenEos(null, 0, 0, 0, 0);
            ParsedDataBase currentDb = null;
            ParsedTable currentTable = null;

            Utils.ForEachLine(filePath, null, (i, items) => {
                var splitted = items.Split('\t');
                switch (items[0]) {
                    case 'H':
                        // base
                        //#H|<Dump date ISO 8601>|<Dump time>|<Logical DB name>|<Physical DB name>|<Progress version>
                        if (splitted.Length != 6)
                            return;
                        currentDb = new ParsedDataBase(
                            splitted[3],
                            splitted[4],
                            splitted[5],
                            new List<ParsedTable>());
                        _dataBases.Add(currentDb);
                        break;
                    case 'S':
                        if (splitted.Length != 3 || currentDb == null)
                            return;
                        _sequences.Add(new ParsedSequence {
                            SeqName = splitted[1],
                            DbName = currentDb.Name
                        });
                        break;
                    case 'T':
                        // table
                        //#T|<Table name>|<Table ID>|<Table CRC>|<Dump name>|<Description>
                        if (splitted.Length != 6 || currentDb == null)
                            return;
                        currentTable = new ParsedTable(
                            splitted[1],
                            defaultToken,
                            splitted[2],
                            splitted[3],
                            splitted[4],
                            splitted[5],
                            "",
                            false,
                            new List<ParsedField>(),
                            new List<ParsedIndex>(),
                            new List<ParsedTrigger>(),
                            "");
                        currentDb.Tables.Add(currentTable);
                        break;
                    case 'X':
                        // trigger
                        //#X|<Parent table>|<Event>|<Proc name>|<Trigger CRC>
                        if (splitted.Length != 5 || currentTable == null)
                            return;
                        currentTable.Triggers.Add(new ParsedTrigger(
                            splitted[2],
                            splitted[3]));
                        break;
                    case 'I':
                        // index
                        //#I|<Parent table>|<Index name>|<Primary? 0/1>|<Unique? 0/1>|<Index CRC>|<Fileds separated with %>
                        if (splitted.Length != 7 || currentTable == null)
                            return;
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
                        if (splitted.Length != 12 || currentTable == null)
                            return;
                        var flags = splitted[6].Equals("1") ? ParseFlag.Mandatory : 0;
                        if (splitted[7].Equals("1")) flags = flags | ParseFlag.Extent;
                        if (splitted[8].Equals("1")) flags = flags | ParseFlag.Index;
                        if (splitted[9].Equals("1")) flags = flags | ParseFlag.Primary;
                        var curField = new ParsedField(
                            splitted[2],
                            splitted[3],
                            splitted[4],
                            int.Parse(splitted[5]),
                            flags,
                            splitted[10],
                            splitted[11],
                            ParsedAsLike.None);
                        curField.Type = ParserUtils.ConvertStringToParsedPrimitiveType(curField.TempType);
                        currentTable.Fields.Add(curField);
                        break;
                }
            });
        }

        #endregion

        #region get list
        
        /// <summary>
        /// returns a dictionary containing all the table names of each database, 
        /// each table is present 2 times, as "TABLE" and "DATABASE.TABLE"
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, CompletionType> GetDbDictionary() {
            var output = new Dictionary<string, CompletionType>(StringComparer.CurrentCultureIgnoreCase);
            _dataBases.ForEach(@base => @base.Tables.ForEach(table => {
                if (!output.ContainsKey(table.Name))
                    output.Add(table.Name, CompletionType.Table);
                if (!output.ContainsKey(string.Join(".", @base.Name, table.Name)))
                    output.Add(string.Join(".", @base.Name, table.Name), CompletionType.Table);
            }));
            return output;
        }

        /// <summary>
        /// returns the list of databases
        /// </summary>
        public static List<CompletionItem> GetDbList() {
            if (_dataBases.Count <= 0)
                return new List<CompletionItem>();
            return _dataBases.Select(@base => new CompletionItem {
                DisplayText = @base.Name.ConvertCase(Config.Instance.DatabaseChangeCaseMode),
                Type = CompletionType.Database,
                FromParser = false,
                Ranking = 0,
                Flags = 0
            }).ToList();
        }

        /// <summary>
        /// returns the list of keywords
        /// </summary>
        public static List<CompletionItem> GetSequencesList() {
            return _sequences.Select(item => new CompletionItem {
                DisplayText = item.SeqName.ConvertCase(Config.Instance.DatabaseChangeCaseMode),
                Type = CompletionType.Sequence,
                SubString = item.DbName
            }).ToList();
        }

        /// <summary>
        /// returns the list tables of each database
        /// </summary>
        /// <returns></returns>
        public static List<CompletionItem> GetTablesList() {
            var output = new List<CompletionItem>();
            foreach (var dataBase in _dataBases)
                output.AddRange(GetTablesList(dataBase));
            return output;
        }

        /// <summary>
        /// Returns the list of tables for a given database
        /// </summary>
        /// <param name="dataBase"></param>
        /// <returns></returns>
        public static List<CompletionItem> GetTablesList(ParsedDataBase dataBase) {
            var output = new List<CompletionItem>();
            if (dataBase == null || dataBase.Tables == null || dataBase.Tables.Count == 0) return output;
            output.AddRange(dataBase.Tables.Select(table => new CompletionItem {
                DisplayText = table.Name.ConvertCase(Config.Instance.DatabaseChangeCaseMode),
                SubString = dataBase.Name,
                Type = CompletionType.Table,
                FromParser = false,
                Ranking = 0,
                Flags = 0
            }).ToList());
            return output;
        }

        /// <summary>
        /// Returns the list of fields for a given table (it can also be a temp table!)
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<CompletionItem> GetFieldsList(ParsedTable table) {
            var output = new List<CompletionItem>();
            if (table == null)
                return output;
            output.AddRange(table.Fields.Select(field => new CompletionItem {
                DisplayText = field.Name.ConvertCase(Config.Instance.DatabaseChangeCaseMode),
                Type = field.Flags.HasFlag(ParseFlag.Primary) ? CompletionType.FieldPk : CompletionType.Field,
                FromParser = false,
                SubString = field.Type.ToString(),
                Ranking = 0,
                Flags = (field.Flags.HasFlag(ParseFlag.Mandatory) ? ParseFlag.Mandatory : 0) |
                        (field.Flags.HasFlag(ParseFlag.Index) ? ParseFlag.Index : 0) |
                        (field.Flags.HasFlag(ParseFlag.Extent) ? ParseFlag.Extent : 0),
                ParsedItem = table
            }));
            return output;
        }

        /// <summary>
        /// Allows to recompute the database list of completion item (when changing case for instance)
        /// </summary>
        public static void ResetCompletionItems() {
            _dbItems = GetCompletionItems();
        }

        /// <summary>
        /// List of items for the autocompletion
        /// </summary>
        public static List<CompletionItem> CompletionItems {
            get { return _dbItems ?? (_dbItems = GetCompletionItems()); }
        }

        private static List<CompletionItem> GetCompletionItems() {
            // Sequences
            var output = _sequences.Select(item => new CompletionItem {
                DisplayText = item.SeqName.ConvertCase(Config.Instance.DatabaseChangeCaseMode),
                Type = CompletionType.Sequence,
                SubString = item.DbName
            }).ToList();

            // Databases
            foreach (var db in _dataBases) {
                var curDb = new CompletionItem {
                    DisplayText = db.Name.ConvertCase(Config.Instance.DatabaseChangeCaseMode),
                    Type = CompletionType.Database,
                    FromParser = false,
                    ParsedItem = db,
                    Ranking = 0,
                    Flags = 0,
                    Children = new List<CompletionItem>(),
                    ChildSeparator = '.'
                };
                output.Add(curDb);

                // Tables
                foreach (var table in db.Tables) {
                    var curTable = new CompletionItem {
                        DisplayText = table.Name.ConvertCase(Config.Instance.DatabaseChangeCaseMode),
                        Type = CompletionType.Table,
                        SubString = db.Name,
                        FromParser = false,
                        ParsedItem = table,
                        Ranking = 0,
                        Flags = 0,
                        Children = new List<CompletionItem>(),
                        ChildSeparator = '.',
                        ParentItem = curDb
                    };
                    curDb.Children.Add(curTable); // add the table as a child of db
                    output.Add(curTable); // but also as an item

                    // Fields
                    foreach (var field in table.Fields) {
                        curTable.Children.Add(new CompletionItem {
                            DisplayText = field.Name.ConvertCase(Config.Instance.DatabaseChangeCaseMode),
                            Type = field.Flags.HasFlag(ParseFlag.Primary) ? CompletionType.FieldPk : CompletionType.Field,
                            SubString = field.Type.ToString(),
                            FromParser = false,
                            ParsedItem = field,
                            Ranking = 0,
                            Flags = field.Flags | ~ParseFlag.Primary,
                            ParentItem = curTable
                        });
                    }
                }
            }
            return output;
        }

        #endregion

        #region find item

        /// <summary>
        /// Get db info by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ParsedDataBase GetDb(string name) {
            return _dataBases.FirstOrDefault(@base => @base.Name.EqualsCi(name));
        }

        public static ParsedDataBase FindDatabaseByName(string name) {
            return _dataBases.Find(@base => @base.Name.EqualsCi(name));
        }

        public static ParsedTable FindTableByName(string name, ParsedDataBase db) {
            return db.Tables.Find(table => table.Name.EqualsCi(name));
        }

        /// <summary>
        /// Find the table referenced among database and defined temp tables; 
        /// name is the table's name (can also be BASE.TABLE)
        /// </summary>
        public static ParsedTable FindTableByName(string name) {
            if (name.CountOccurences(".") > 0) {
                var splitted = name.Split('.');
                // find db then find table
                var foundDb = FindDatabaseByName(splitted[0]);
                return foundDb == null ? null : FindTableByName(splitted[1], foundDb);
            }
            return _dataBases.Select(dataBase => FindTableByName(name, dataBase)).FirstOrDefault(found => found != null);
        }

        public static ParsedField FindFieldByName(string name, ParsedTable table) {
            return table.Fields.Find(field => field.Name.EqualsCi(name));
        }

        /// <summary>
        /// Returns the field corresponding to the input TABLE.FIELD or DB.TABLE.FIELD
        /// </summary>
        public static ParsedField FindFieldByName(string name) {
            var splitted = name.Split('.');
            if (splitted.Length == 1)
                return null;

            var tableName = splitted[splitted.Length == 3 ? 1 : 0];
            var fieldName = splitted[splitted.Length == 3 ? 2 : 1];

            ParsedTable foundTable;

            if (splitted.Length == 3) {
                // find db
                var foundDb = FindDatabaseByName(splitted[0]);
                if (foundDb == null)
                    return null;

                // find table
                foundTable = FindTableByName(tableName, foundDb);
            } else {
                // find table
                foundTable = FindTableByName(tableName);
            }

            // find field
            return foundTable == null ? null : FindFieldByName(fieldName, foundTable);
        }

        #endregion

    }
}