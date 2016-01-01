#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
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
using _3PA.Html;
using _3PA.Lib;
using _3PA.MainFeatures.Parser;
using _3PA.MainFeatures.ProgressExecutionNs;

namespace _3PA.MainFeatures.AutoCompletion {
    public class DataBase {

        #region fields

        /// <summary>
        /// List of Databases (each of which contains list of tables > list of fields/indexes/triggers)
        /// </summary>
        private static List<ParsedDataBase> _dataBases = new List<ParsedDataBase>();

        /// <summary>
        /// File path to the output file of the dumpdatabase program
        /// </summary>
        public static string OutputFileName { get; private set; }

        /// <summary>
        /// Gets the directory where database info are stored
        /// </summary>
        public static string DatabaseDir {
            get {
                var dir = Path.Combine(Npp.GetConfigDir(), "DatabaseInfo");
                if (!Directory.Exists(dir))
                    try {
                        Directory.CreateDirectory(dir);
                    } catch (Exception e) {
                        ErrorHandler.ShowErrors(e, "Permission denied when creating " + dir);
                    }
                return dir;
            }
        }

        /// <summary>
        /// Action called when an extraction is done
        /// </summary>
        private static Action _onExtractionDone;

        #endregion

        #region public methods

        /// <summary>
        /// Tries to load the database information of the current ProgressEnv, 
        /// returns false the info is not available
        /// </summary>
        /// <returns></returns>
        public static bool TryToLoadDatabaseInfo() {
            var fileName = GetOutputName();

            // read
            if (File.Exists(Path.Combine(DatabaseDir, fileName))) {
                if (Plug.PluginIsFullyLoaded) {
                    Config.Instance.EnvLastDbInfoUsed = fileName;
                    Init();
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Deletes the file corresponding to the current database (if it exists)
        /// </summary>
        public static void DeleteCurrentDbInfo() {
            var filePath = Path.Combine(DatabaseDir, GetOutputName());
            if (File.Exists(filePath)) {
                try {
                    File.Delete(filePath);
                } catch (Exception) {
                    UserCommunication.Notify("Couldn't delete the following files:<br><a href='" + filePath + "'>" + filePath + "</a>", MessageImg.MsgError, "Delete failed", "Current database info");
                }
            }
        }

        /// <summary>
        /// Should be called to extract the database info from the current environnement
        /// </summary>
        public static void FetchCurrentDbInfo(Action onExtractionDone) {
            try {
                // dont extract 2 db at once
                if (!string.IsNullOrEmpty(OutputFileName)) {
                    UserCommunication.Notify("Already fetching info for another environment, please wait the end of the previous execution!", MessageImg.MsgWarning, "Database info", "Extracting database structure", 5);
                    return;
                }

                // save the filename of the output database info file for this environment
                OutputFileName = GetOutputName();

                UserCommunication.Notify("Now fetching info on all the connected databases for the current environment<br>You will be warned when the process is over", MessageImg.MsgInfo, "Database info", "Extracting database structure", 5);

                var exec = new ProExecution();
                exec.ProcessExited += ExtractionDone;
                _onExtractionDone = onExtractionDone;
                if (exec.Do(ExecutionType.Database))
                    return;
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "FetchCurrentDbInfo");
            }
            OutputFileName = null;
        }

        /// <summary>
        /// Method called after the execution of the program extracting the db info
        /// </summary>
        private static void ExtractionDone(object sender, ProcessOnExitEventArgs processOnExitEventArgs) {
            try {

                if (!File.Exists(processOnExitEventArgs.ProgressExecution.ExtractDbOutputPath) || new FileInfo(processOnExitEventArgs.ProgressExecution.ExtractDbOutputPath).Length == 0) {
                    UserCommunication.Notify("Something went wrong while extracting the database info, verify the connection info and try again<br><br><i>Also, make sure that the database for the current environment is connected!</i><br><br>Below is the progress error returned while trying to dump the database : " + Utils.ReadAndFormatLogToHtml(processOnExitEventArgs.ProgressExecution.LogPath), MessageImg.MsgRip, "Database info", "Extracting database structure");
                } else {
                    var targetFile = Path.Combine(DatabaseDir, OutputFileName);
                    if (File.Exists(targetFile))
                        File.Delete(targetFile);

                    // copy to database dir
                    File.Copy(processOnExitEventArgs.ProgressExecution.ExtractDbOutputPath, targetFile);

                    // read
                    Config.Instance.EnvLastDbInfoUsed = OutputFileName;
                    Init();

                    UserCommunication.Notify("Database structure extracted with success! The auto-completion has been updated with the latest info, enjoy!", MessageImg.MsgOk, "Database info", "Extracting database structure", 10);

                    if (_onExtractionDone != null) {
                        _onExtractionDone();
                        _onExtractionDone = null;
                    }

                    AutoComplete.ParseCurrentDocument(true);
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "FetchCurrentDbInfo");
            }
            OutputFileName = null;
        }

        /// <summary>
        /// Read last used database info file
        /// </summary>
        public static void Init() {
            if (string.IsNullOrEmpty(Config.Instance.EnvLastDbInfoUsed))
                return;

            // read file, extract info
            Read(Path.Combine(DatabaseDir, Config.Instance.EnvLastDbInfoUsed));

            // Update autocompletion
            AutoComplete.FillStaticItems(false);
            AutoComplete.ParseCurrentDocument();
        }

        #endregion

        #region private methods

        /// <summary>
        /// Returns the output file name for the current appli/env
        /// </summary>
        /// <returns></returns>
        private static string GetOutputName() {
            return (Config.Instance.EnvName + "_" + Config.Instance.EnvSuffix + "_" + Config.Instance.EnvDatabase).ToValidFileName().ToLower() + ".dump";
        }

        /// <summary>
        /// This method parses the output of the .p procedure that exports the database info
        /// and fills _dataBases
        /// It then updates the parser with the new info
        /// </summary>
        private static void Read(string filePath) {
            if (!File.Exists(filePath)) return;
            _dataBases.Clear();
            try {
                ParsedDataBase currentDb = null;
                ParsedTable currentTable = null;
                foreach (var items in File.ReadAllLines(filePath, TextEncodingDetect.GetFileEncoding(filePath)).Where(items => items.Length > 1 && !items[0].Equals('#'))) {
                    var splitted = items.Split('\t');
                    switch (items[0]) {
                        case 'H':
                            // base
                            //#H|<Dump date ISO 8601>|<Dump time>|<Logical DB name>|<Physical DB name>|<Progress version>
                            if (splitted.Count() != 6) continue;
                            currentDb = new ParsedDataBase(
                                splitted[3].ToUpper(),
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
                                splitted[1].ToUpper(),
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
                                splitted[2].ToUpper(),
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
                ErrorHandler.ShowErrors(e, "Error while loading database info!", filePath);
            }
        }

        #endregion

        #region get list

        /// <summary>
        /// Exposes the databases info
        /// </summary>
        /// <returns></returns>
        public static List<ParsedDataBase> Get() {
            return _dataBases;
        }

        /// <summary>
        /// Get db info by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ParsedDataBase GetDb(string name) {
            return _dataBases.FirstOrDefault(@base => @base.LogicalName.EqualsCi(name));
        }

        /// <summary>
        /// returns a dictionary containing all the table names of each database, 
        /// each table is present 2 times, as "TABLE" and "DATABASE.TABLE"
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, CompletionType> GetDbDictionnary() {
            var output = new Dictionary<string, CompletionType>(StringComparer.CurrentCultureIgnoreCase);
            _dataBases.ForEach(@base => @base.Tables.ForEach(table => {
                if (!output.ContainsKey(table.Name))
                    output.Add(table.Name, CompletionType.Table);
                if (!output.ContainsKey(string.Join(".", @base.LogicalName, table.Name)))
                    output.Add(string.Join(".", @base.LogicalName, table.Name), CompletionType.Table);
            }));
            return output;
        }

        /// <summary>
        /// returns the list of databases
        /// </summary>
        public static List<CompletionData> GetDbList() {
            if (_dataBases.Count <= 0) return new List<CompletionData>();
            return _dataBases.Select(@base => new CompletionData {
                DisplayText = @base.LogicalName,
                Type = CompletionType.Database,
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
            output.AddRange(dataBase.Tables.Select(table => new CompletionData {
                DisplayText = table.Name,
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
            output.AddRange(table.Fields.Select(field => new CompletionData {
                DisplayText = field.Name,
                Type = (field.Flag.HasFlag(ParsedFieldFlag.Primary)) ? CompletionType.FieldPk : CompletionType.Field,
                FromParser = false,
                SubString = field.Type.ToString(),
                Ranking = ParserHandler.FindRankingOfDatabaseItem(field.Name),
                Flag = (field.Flag.HasFlag(ParsedFieldFlag.Mandatory) ? ParseFlag.Mandatory : 0) |
                       (field.Flag.HasFlag(ParsedFieldFlag.Index) ? ParseFlag.Index : 0) |
                       (field.Flag.HasFlag(ParsedFieldFlag.Extent) ? ParseFlag.Extent : 0),
                ParsedItem = table
            }));
            return output;
        }

        #endregion

        #region find item

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

        #endregion

    }
}
