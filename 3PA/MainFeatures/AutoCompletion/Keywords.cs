#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (Keywords.cs) is part of 3P.
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

        #region fields

        private static List<CompletionData> _keywords = new List<CompletionData>();
        private static List<KeywordsAbbreviations> _abbreviations = new List<KeywordsAbbreviations>();
        private static Dictionary<string, KeywordsHelp> _help = new Dictionary<string, KeywordsHelp>(StringComparer.OrdinalIgnoreCase);
        private static string _filePath;
        private static string _location = Npp.GetConfigDir();
        private static string _fileNameKeywords = "keywords.data";
        private static string _fileNameKeywordsHelp = "keywordsHelp.data";
        private static string _fileNameAbbrev = "abbreviations.data";

        #endregion

        #region Init

        /// <summary>
        /// To call in order to read all the keywords to the private List CompletionData
        /// </summary>
        public static void Init() {
            /* keywords */
            _filePath = Path.Combine(_location, _fileNameKeywords);
            if (!File.Exists(_filePath))
                File.WriteAllBytes(_filePath, DataResources.keywords);
            _keywords.Clear();
            try {
                foreach (var items in File.ReadAllLines(_filePath, Encoding.Default).Select(line => line.Split('\t')).Where(items => items.Count() == 4)) {

                    // find the KeywordType from items[1]
                    KeywordType keywordType = KeywordType.Unknow;
                    var keywordTypeStr = items[1];
                    foreach (var typ in Enum.GetNames(typeof (KeywordType)).Where(typ => keywordTypeStr.EqualsCi(typ)))
                        keywordType = (KeywordType) Enum.Parse(typeof (KeywordType), typ, true);

                    // set flags
                    var flag = (items[2] == "1") ? ParseFlag.Reserved : 0;
                    if (keywordType == KeywordType.Abbreviation) flag = flag | ParseFlag.Abbreviation;

                    _keywords.Add(new CompletionData {
                        DisplayText = items[0],
                        Type = ((int) keywordType < 30) ? CompletionType.Keyword : CompletionType.KeywordObject,
                        Ranking = int.Parse(items[3]),
                        SubString = keywordType.ToString(),
                        Flag = flag,
                        KeywordType = keywordType
                    });
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error while loading keywords!", _filePath);
            }

            /* abbreviations */
            var filePathAbb = Path.Combine(_location, _fileNameAbbrev);
            if (!File.Exists(filePathAbb))
                File.WriteAllBytes(filePathAbb, DataResources.abbreviations);
            _abbreviations.Clear();
            try {
                foreach (var items in File.ReadAllLines(filePathAbb, Encoding.Default).Select(line => line.Split('\t')).Where(items => items.Count() == 2)) {
                    _abbreviations.Add(new KeywordsAbbreviations {
                        CompleteText = items[1],
                        ShortText = items[0]
                    });
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error while loading abbreviations!", filePathAbb);
            }

            /* keyword Help */
            var filePathHelp = Path.Combine(_location, _fileNameKeywordsHelp);
            if (!File.Exists(filePathHelp))
                File.WriteAllBytes(filePathHelp, DataResources.keywordsHelp);
            _help.Clear();
            try {
                foreach (var items in File.ReadAllLines(filePathHelp, Encoding.Default).Select(line => line.Split('\t')).Where(items => items.Length > 2)) {
                    var listSynthax = new List<string>();
                    var i = 2;
                    while (i < items.Length) {
                        listSynthax.Add(items[i]);
                        i++;
                    }
                    _help.Add(items[0], new KeywordsHelp {
                        Description = items[1],
                        Synthax = listSynthax
                    });
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error while loading keywords help!", filePathHelp);
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Save the keywords data into the file (to remember the ranking of each keyword)
        /// </summary>
        public static void Save() {
            if (_keywords.Count == 0) return;
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

        /// <summary>
        /// Returns the help for a specified keyword
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static KeywordsHelp GetKeywordHelp(string key) {
            return _help.ContainsKey(key) ? _help[key] : null;
        }

        #endregion

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
        Method
    }

    public class KeywordsAbbreviations {
        public string CompleteText;
        public string ShortText;
    }

    public class KeywordsHelp {
        public string Description;
        public List<string> Synthax;
    }
}