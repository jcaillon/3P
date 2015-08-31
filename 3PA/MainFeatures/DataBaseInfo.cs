using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Enumerable = System.Linq.Enumerable;

namespace _3PA.MainFeatures {

    public class TableInfo {
        public string Desc { get; set; }
        public Dictionary<string, FieldInfo> MapFields { get; set; }
        public int Rank { get; set; } // ranking when displaying the list
    }

    public class FieldInfo {
        public int Order { get; set; }
        public string DataType { get; set; }
        public string Format { get; set; }
        public string Label { get; set; }
        public string Desc { get; set; }
        public string Initial { get; set; }
        public string Stat { get; set; }
        public int Rank { get; set; } // ranking when displaying the list
    }

    public class DataBaseInfo {
        public static Dictionary<string, TableInfo> MapTables = new Dictionary<string, TableInfo>(StringComparer.OrdinalIgnoreCase);

        private const string FileName = "db_out.data";
        private static int _lastIndexTable;
        private static int _lastIndexField;

        private static string DataBaseExportFile {
            get { return Path.Combine(Npp.GetConfigDir(), FileName); }
        }

        public static List<string> KeysTable {
            get {
                var lst = MapTables.ToList();
                lst.Sort(
                    (firstPair, nextPair) => {
                        if (nextPair.Value.Rank != firstPair.Value.Rank)
                            return nextPair.Value.Rank.CompareTo(firstPair.Value.Rank);
                        return String.Compare(firstPair.Key, nextPair.Key, StringComparison.Ordinal);
                    }
                    );
                return lst.Select(x => x.Key).ToList();
            }
        }

        public static List<string> KeysField(string tableName) {
            List<string> output;
            if (ContainsTable(tableName)) {
                var lst = MapTables[tableName].MapFields.ToList();
                lst.Sort(
                    (firstPair, nextPair) => {
                        if (nextPair.Value.Rank != firstPair.Value.Rank)
                            return nextPair.Value.Rank.CompareTo(firstPair.Value.Rank);
                        return firstPair.Value.Order.CompareTo(nextPair.Value.Order);
                    }
                    );
                output = lst.Select(x => x.Key).ToList();
            }
            else
                output = new List<string>();
            return output;
        }

        public static bool ContainsTable(string tableName) {
            lock (MapTables) {
                return (!string.IsNullOrWhiteSpace(tableName)) && MapTables.ContainsKey(tableName);
            }
        }

        public static bool ContainsField(string tableName, string fieldName) {
            bool output;
            try {
                output = MapTables[tableName].MapFields.ContainsKey(fieldName);
            }
            catch (Exception) {
                output = false;
            }
            return output;
        }

        public static void RememberUseOfTable(string tableName) {
            if (ContainsTable(tableName)) {
                MapTables[tableName].Rank = _lastIndexTable;
                _lastIndexTable++;
            }
        }

        public static void RememberUseOfField(string tableName, string fieldName) {
            if (ContainsTable(tableName)) {
                if (ContainsField(tableName, fieldName)) {
                    MapTables[tableName].MapFields[fieldName].Rank = _lastIndexField;
                    _lastIndexField++;
                }
            }
        }

        public static bool IsFieldInPk(string tableName, string fieldName) {
            bool output;
            try {
                output = MapTables[tableName].MapFields[fieldName].Stat.Contains("PK");
            }
            catch (Exception) {
                output = false;
            }
            return output;
        }

        public static void Init() {
            lock (MapTables) {
                if (File.Exists(DataBaseExportFile)) {
                    try { 
                        int loc = 0;
                        string[] splitted;
                        string[] tabFie;
                        foreach (var line in File.ReadAllLines(DataBaseExportFile)) {
                            if (line.StartsWith("_TABLES_") || line.StartsWith("_FIELDS_")) {
                                loc++;
                                continue;
                            }
                            if (loc == 1) {
                                splitted = line.Split('\t');
                                if (Enumerable.Count(splitted) != 2) continue;
                                if (!MapTables.ContainsKey(splitted[0]))
                                    MapTables.Add(splitted[0],
                                        new TableInfo {
                                            Desc = splitted[1],
                                            MapFields = new Dictionary<string, FieldInfo>(),
                                            Rank = 0
                                        });
                            }
                            if (loc == 2) {
                                splitted = line.Split('\t');
                                if (Enumerable.Count(splitted) != 8) continue;
                                tabFie = splitted[0].Split('.');
                                if (Enumerable.Count(tabFie) != 2) continue;
                                if (!MapTables[tabFie[0]].MapFields.ContainsKey(tabFie[1]))
                                    MapTables[tabFie[0]].MapFields.Add(tabFie[1],
                                        new FieldInfo {
                                            Order = int.Parse(splitted[1]),
                                            DataType = splitted[2],
                                            Format = splitted[3],
                                            Label = splitted[4],
                                            Desc = splitted[5],
                                            Initial = splitted[6],
                                            Stat = splitted[7],
                                            Rank = 0
                                        });
                            }
                        }
                    } catch (Exception e) {
                        Plug.ShowErrors(e, "Error while loading the database info!", DataBaseExportFile);
                    }
                }
            }
        }

        /*
            public static List<FileTag> GetFileTagsList(string filename) {
                lock (Map) {
                    if (Map.ContainsKey(filename)) {
                        return Map[filename];
                    }
                    else
                        return new List<FileTag>();
                }
            }

            public static FileTag GetFileTags(string filename, int nb) {
                return GetFileTagsList(filename).Find(x => (x.nb == nb));
            }
        */
    }
}