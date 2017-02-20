#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Linq;
using System.Text;
using _3PA.Data;
using _3PA.Lib;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.AutoCompletionFeature {

    //TODO: pour gérer les HANDLE attribute, ajouter une colonne aux keywords qui peut soit être vide soit contenir une liste de nombres qui correspondent à un id de handle:
    // par exemple, on a le Buffer object handle qui a l'id 1, et ben quand on affiche les propriétés d'un keyword qu'on identifie en tant que Buffer object handle, on filtre les propriétés/méthodes qui ont se flag 1 dans la 5eme colonne

    /// <summary>
    /// this class handles the static keywords of progress
    /// </summary>
    internal static class Keywords {

        #region fields

        // Dictionnay of id -> keyword
        private static Dictionary<string, KeywordDefinition> _keywordList = new Dictionary<string, KeywordDefinition>(); 
        private static Dictionary<string, KeywordHelp> _help = new Dictionary<string, KeywordHelp>(StringComparer.CurrentCultureIgnoreCase);
        private static List<KeywordAbbreviation> _abbreviations = new List<KeywordAbbreviation>();

        #endregion
        
        #region Import / Export

        /// <summary>
        /// To call in order to read all the keywords to the private List CompletionItem
        /// </summary>
        public static void Import() {
            // reads keywords
            _keywordList.Clear();
            Utils.ForEachLine(Config.FileKeywordsList, DataResources.KeywordsList, (i, line) => {
                var items = line.Split('\t');
                if (items.Count() == 5) {
                    // find the KeywordType from items[1]
                    KeywordType keywordType;
                    if (!Enum.TryParse(items[2], true, out keywordType))
                        keywordType = KeywordType.Unknow;

                    // set flags
                    var flag = (items[3] == "1") ? ParseFlag.Reserved : 0;
                    if (keywordType == KeywordType.Abbreviation) flag = flag | ParseFlag.Abbreviation;

                    if (!_keywordList.ContainsKey(items[0])) {
                        _keywordList.Add(items[0], new KeywordDefinition {
                            DisplayText = items[1],
                            Type = ((int) keywordType < 30) ? CompletionType.Keyword : CompletionType.KeywordObject,
                            Ranking = int.Parse(items[4]),
                            Flag = flag,
                            KeywordType = keywordType
                        });
                    }
                }
            }, 
            Encoding.Default);

            // reads keywords rank
            Utils.ForEachLine(Config.FileKeywordsRank, new byte[0], (i, line) => {
                var items = line.Split('\t');
                if (items.Count() == 2 && _keywordList.ContainsKey(items[0])) {
                    int val;
                    if (int.TryParse(items[1], out val)) {
                        _keywordList[items[0]].Ranking = val;
                    }
                }
            }, 
            Encoding.Default);

            // reads abbreviations
            _abbreviations.Clear();
            Utils.ForEachLine(Config.FileAbbrev, DataResources.Abbreviations, (i, line) => {
                var items = line.Split('\t');
                if (items.Count() == 2) {
                    _abbreviations.Add(new KeywordAbbreviation {
                        CompleteText = items[1],
                        ShortText = items[0]
                    });
                }
            }, 
            Encoding.Default);

            // reads keywords help
            _help.Clear();
            Utils.ForEachLine(Config.FileKeywordsHelp, DataResources.KeywordsHelp, (lineNb, line) => {
                var items = line.Split('\t');
                if (items.Count() > 2) {
                    var listSynthax = new List<string>();
                    for (int i = 2; i < items.Length; i++) {
                        listSynthax.Add(items[i]);
                    }
                    _help.Add(items[0], new KeywordHelp {
                        Description = items[1],
                        Synthax = listSynthax
                    });
                }
            }, 
            Encoding.Default);
        }

        /// <summary>
        /// Save the keywords data into a file (to remember the ranking of each keyword)
        /// </summary>
        public static void SaveRanking() {
            if (_keywordList.Count == 0) return;
            var strBuilder = new StringBuilder();
            foreach (var kpv in _keywordList) {
                strBuilder.AppendLine(kpv.Key + "\t" + kpv.Value.Ranking);
            }
            Utils.FileWriteAllText(Config.FileKeywordsRank, strBuilder.ToString());
        }

        #endregion

        #region public methods

        /// <summary>
        /// returns the list of keywords
        /// </summary>
        public static List<CompletionItem> GetList() {
            return _keywordList.Values.Select(keyword => new CompletionItem {
                DisplayText = keyword.DisplayText.ConvertCase(Config.Instance.KeywordChangeCaseMode),
                Type = keyword.Type,
                Ranking = keyword.Ranking,
                SubString = keyword.KeywordType.ToString(),
                Flag = keyword.Flag,
                KeywordType = keyword.KeywordType
            }).ToList();
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
        public static KeywordHelp GetKeywordHelp(string key) {
            return _help.ContainsKey(key) ? _help[key] : null;
        }

        #endregion

    }

    internal class KeywordDefinition {
        public string DisplayText { get; set; }
        public CompletionType Type { get; set; }
        public int Ranking{ get; set; }
        public KeywordType KeywordType { get; set; }
        public ParseFlag Flag { get; set; }
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

    internal class KeywordAbbreviation {
        public string CompleteText;
        public string ShortText;
    }

    internal class KeywordHelp {
        public string Description;
        public List<string> Synthax;
    }

}