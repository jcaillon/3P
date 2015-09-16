using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using _3PA.Data;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;

namespace _3PA.MainFeatures {

    /// <summary>
    /// this class handles the static keywords of progress
    /// </summary>
    public class Keywords {

        private static List<CompletionData> _keywords = new List<CompletionData>();
        private static string _filePath;
        private static string _location = Npp.GetConfigDir();
        private static string _fileName = "keywords.data";

        public static void Init() {
            _filePath = Path.Combine(_location, _fileName);
            if (!File.Exists(_filePath))
                File.WriteAllBytes(_filePath, DataResources.keywords);
            _keywords.Clear();
            try {
                Load();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error while loading keywords!", _filePath);
            }
        }

        private static void Load() {
            foreach (var items in File.ReadAllLines(_filePath).Select(line => line.Split('\t')).Where(items => items.Count() == 4)) {
                _keywords.Add(new CompletionData {
                    DisplayText = items[0],
                    Type = CompletionType.Keyword,
                    Ranking = int.Parse(items[3]),
                    SubType = items[1],
                    Flag = new List<CompletionFlag> {
                        (items[2] == "1") ? CompletionFlag.Reserved : CompletionFlag.None
                    }
                });
            }
        }

        public static void Save() {
            var strBuilder = new StringBuilder();
            foreach (var keyword in _keywords) {
                strBuilder.AppendLine(keyword.DisplayText + "\t" + keyword.SubType + "\t" + ((keyword.Flag != null && keyword.Flag[0] == CompletionFlag.Reserved) ? "1" : "0") + "\t" + keyword.Ranking);
            }
            File.WriteAllText(_filePath, strBuilder.ToString());
        }

        public static List<CompletionData> Get {
            get { return _keywords; }
        }

        public static bool Contains(string keyword) {
            var x = _keywords.Find(data => data.DisplayText == keyword);
            return x != null;
        }

        public static void RemberUseOf(string keyword) {
            var x = _keywords.Find(data => data.DisplayText == keyword);
            if (x != null) x.Ranking++;
        }

    }
}