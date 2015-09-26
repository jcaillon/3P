using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _3PA.Lib;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.AutoCompletion {
    public class DataBase {

        private static List<ParsedDataBase> _dataBases = new List<ParsedDataBase>();
        private static string _filePath = @"C:\LiberKey\Apps\Notepad++\App\Notepad++\plugins\Config\3PA\more\database_out.txt";
        private static string _location = Npp.GetConfigDir();
        private static string _fileName = "database_out.txt";

        public static void FetchCurrentDbInfo() {
            //_filePath = Path.Combine(_location, _fileName);
            Read();
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
                                "", 0, false,
                                new List<ParsedField>(),
                                new List<ParsedIndex>(),
                                new List<ParsedTrigger>());
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
                            currentTable.Fields.Add(new ParsedField(
                                splitted[2],
                                splitted[3],
                                splitted[4],
                                int.Parse(splitted[5]),
                                flag2,
                                splitted[10],
                                splitted[11],
                                "", 0));
                            break;
                    }
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error while loading database info!", _filePath);
            }
        }

        /// <summary>
        /// returns the list of keywords
        /// </summary>
        public static List<CompletionData> GetDbList() {
            return _dataBases.Select(@base => new CompletionData() {
                DisplayText = @base.LogicalName
            }).ToList();
        }

        /*

        /// <summary>
        /// returns the list of keywords
        /// </summary>
        public static List<CompletionData> Get {
            get { return _keywords; }
        }

        /// <summary>
        /// Is the keyword known to the plugin?
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public static bool Contains(string keyword) {
            var x = _keywords.Find(data => data.DisplayText.EqualsCi(keyword));
            return x != null;
        }

        /// <summary>
        /// increase ranking of input keyword
        /// </summary>
        /// <param name="keyword"></param>
        public static void RemberUseOf(string keyword) {
            var x = _keywords.Find(data => data.DisplayText == keyword);
            if (x != null) x.Ranking++;
        }
         * */
    }
}
