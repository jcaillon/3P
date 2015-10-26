using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using _3PA.Data;
using _3PA.Lib;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.AutoCompletion {

    //TODO: pour gérer les HANDLE attribute, ajouter une colonne aux keywords qui peut soit être vide soit contenir une liste de nombres qui correspondent à un id de handle:
    // par exemple, on a le Buffer object handle qui a l'id 1, et ben quand on affiche les propriétés d'un keyword qu'on identifie en tant que Buffer object handle, on filtre les propriétés/méthodes qui ont se flag 1 dans la 5eme colonne

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
                foreach (var items in File.ReadAllLines(_filePath, Encoding.Default).Select(line => line.Split('\t')).Where(items => items.Count() == 4)) {
 
                    // find the KeywordType from items[1]
                    KeywordType keywordType = KeywordType.Unknow;
                    var keywordTypeStr = items[1];
                    foreach (var typ in Enum.GetNames(typeof(KeywordType)).Where(typ => keywordTypeStr.EqualsCi(typ)))
                        keywordType = (KeywordType)Enum.Parse(typeof(KeywordType), typ, true);

                    // set flags
                    var flag = (items[2] == "1") ? ParseFlag.Reserved : 0;
                    if (keywordType == KeywordType.Abbreviation) flag = flag | ParseFlag.Abbreviation;

                    _keywords.Add(new CompletionData {
                        DisplayText = items[0],
                        Type = ((int)keywordType < 30) ? CompletionType.Keyword : CompletionType.KeywordObject,
                        Ranking = int.Parse(items[3]),
                        SubString = keywordType.ToString(),
                        Flag = flag,
                        KeywordType = keywordType
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
                foreach (var items in File.ReadAllLines(filePathAbb, Encoding.Default).Select(line => line.Split('\t')).Where(items => items.Count() == 2)) {
                    _abbreviations.Add(new KeywordsAbbreviations() {
                        CompleteText = items[1],
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
                strBuilder.AppendLine(keyword.DisplayText + "\t" + keyword.SubString + "\t" + ((keyword.Flag.HasFlag(ParseFlag.Reserved)) ? "1" : "0") + "\t" + keyword.Ranking);
            }
            File.WriteAllText(_filePath, strBuilder.ToString());
        }

        /// <summary>
        /// returns the list of keywords
        /// </summary>
        public static List<CompletionData> GetList() {
            return _keywords;
        }

        /// <summary>
        /// returns the complete keyword for the given abbreviation
        /// </summary>
        /// <param name="abbreviation"></param>
        /// <returns></returns>
        public static string GetFullKeyword(string abbreviation) {
            var found = _abbreviations.Find(abbreviations => 
                abbreviations.CompleteText.ContainsFast(abbreviation) &&
                abbreviation.ContainsFast(abbreviations.ShortText)
                );
            return found != null ? found.CompleteText : null;
        }
    }

    /// <summary>
    /// Keyword types enumeration
    /// </summary>
    public enum KeywordType {
        // below are the types that go to the Keyword category
        Statement,
        Function,
        Operator,
        Option,
        Type,
        Widget,
        Preprocessor,
        Handle,
        Event,
        Keyboard,
        Abbreviation,
        Appbuilder,
        Unknow,
        // below are the types that go into the KeywordObject category
        Attribute = 30,
        Property,
        Method,
    }

    public class KeywordsAbbreviations {
        public string CompleteText;
        public string ShortText;
    }
}