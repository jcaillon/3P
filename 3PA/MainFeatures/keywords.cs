using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _3PA.Data;
using _3PA.Lib;

namespace _3PA.MainFeatures {

    public class Keywords {
        public static SpecialDictionary<int> Map = new SpecialDictionary<int>(StringComparer.OrdinalIgnoreCase);
        private const string FileName = "keywords.data";

        public static List<string> Keys {
            get {
                lock (Map) {
                    var lst = Map.ToList();
                    lst.Sort(
                        (firstPair, nextPair) => {
                            if (nextPair.Value != firstPair.Value)
                                return nextPair.Value.CompareTo(firstPair.Value);
                            return String.Compare(firstPair.Key, nextPair.Key, StringComparison.Ordinal);
                        }
                        );
                    return lst.Select(x => x.Key).ToList();
                }
            }
        }

        public static bool Contains(string keyword) {
            lock (Map) {
                return (!string.IsNullOrWhiteSpace(keyword)) && Map.ContainsKey(keyword.ToUpper());
            }
        }

        public static void RemberUseOf(string keyword) {
            if (Contains(keyword))
                lock (Map) {
                    Map[keyword]++;
                }
        }

        private static string ConfigFile {
            get { return Path.Combine(Npp.GetConfigDir(), FileName); }
        }

        public static void Init() {
            lock (Map) {
                if (!File.Exists(ConfigFile))
                    File.WriteAllBytes(ConfigFile, DataResources.keywords);
                Map.Clear();
                try {
                    Map.Load(ConfigFile);
                } catch (Exception e) {
                    Plug.ShowErrors(e, "Error while loading keywords!", ConfigFile);
                }
            }
        }

        public static void Save() {
            lock (Map) {
                try {
                    Map.Save(ConfigFile);
                } catch (Exception e) {
                    Plug.ShowErrors(e, "Error while saving keywords!");
                }
            }
        }
    }
}