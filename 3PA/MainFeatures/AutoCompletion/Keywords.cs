using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using _3PA.Data;
using _3PA.Lib;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.AutoCompletion {

    /// <summary>
    /// this class handles the static keywords of progress
    /// </summary>
    public class Keywords {

        private static List<CompletionData> _keywords = new List<CompletionData>();
        private static List<KeywordsAbbreviations> _abbreviations = new List<KeywordsAbbreviations>(); 
        private static string _filePath;
        private static string _location = Npp.GetConfigDir();
        private static string _fileNameKeywords = "keywords.data";
        private static string _fileNameAbbrev = "abbreviations.data";

        /// <summary>
        /// To call in order to read all the keywords to the private List CompletionData
        /// </summary>
        public static void Init() {
            _filePath = Path.Combine(_location, _fileNameKeywords);
            if (!File.Exists(_filePath))
                File.WriteAllBytes(_filePath, DataResources.keywords);
            _keywords.Clear();
            try {
                foreach (var items in File.ReadAllLines(_filePath).Select(line => line.Split('\t')).Where(items => items.Count() == 4)) {
                    _keywords.Add(new CompletionData {
                        DisplayText = items[0],
                        Type = CompletionType.Keyword,
                        Ranking = int.Parse(items[3]),
                        SubType = items[1],
                        Flag = (items[2] == "1") ? ParseFlag.Reserved : ParseFlag.None
                    });
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error while loading keywords!", _filePath);
            }
            var filePathAbb = Path.Combine(_location, _fileNameAbbrev);
            if (!File.Exists(filePathAbb))
                File.WriteAllBytes(filePathAbb, DataResources.abbreviations);
            _abbreviations.Clear();
            try {
                foreach (var items in File.ReadAllLines(filePathAbb).Select(line => line.Split('\t')).Where(items => items.Count() == 2)) {
                    _abbreviations.Add(new KeywordsAbbreviations() {
                        DisplayText = items[1],
                        ShortText = items[0]
                    });
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error while loading abbreviations!", filePathAbb);
            }
        }

        /// <summary>
        /// Save the keywords data into the file (to remember the ranking of each keyword)
        /// </summary>
        public static void Save() {
            var strBuilder = new StringBuilder();
            foreach (var keyword in _keywords) {
                strBuilder.AppendLine(keyword.DisplayText + "\t" + keyword.SubType + "\t" + ((keyword.Flag.HasFlag(ParseFlag.Reserved)) ? "1" : "0") + "\t" + keyword.Ranking);
            }
            File.WriteAllText(_filePath, strBuilder.ToString());
        }

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

    }

    public class KeywordsAbbreviations {
        public string DisplayText;
        public string ShortText;
    }
}