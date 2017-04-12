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
using _3PA.Lib;
using _3PA.MainFeatures.Parser;
using _3PA._Resource;

namespace _3PA.MainFeatures.AutoCompletionFeature {
    //TODO: pour gérer les HANDLE attribute, ajouter une colonne aux keywords qui peut soit être vide soit contenir une liste de nombres qui correspondent à un id de handle:
    // par exemple, on a le Buffer object handle qui a l'id 1, et ben quand on affiche les propriétés d'un keyword qu'on identifie en tant que Buffer object handle, on filtre les propriétés/méthodes qui ont 1 dans la 5eme colonne

    /// <summary>
    /// this class handles the static keywords of progress
    /// </summary>
    internal class Keywords {

        #region Singleton

        private static Keywords _instance;

        public static Keywords Instance {
            get { return _instance ?? (_instance = new Keywords()); }
        }

        #endregion

        #region Event

        public event Action OnImport;

        #endregion

        #region fields

        // Dictionary of id -> keyword
        private Dictionary<string, CompletionItem> _keywordById;
        private Dictionary<string, KeywordHelp> _help;
        private List<KeywordAbbreviation> _abbreviations;
        private List<CompletionItem> _keywords;

        #endregion

        #region Life and death

        public Keywords() {
            _keywordById = new Dictionary<string, CompletionItem>();
            _help = new Dictionary<string, KeywordHelp>(StringComparer.CurrentCultureIgnoreCase);
            _abbreviations = new List<KeywordAbbreviation>();
            Import();
        }

        #endregion

        #region Import / Export

        /// <summary>
        /// To call in order to read all the keywords to the private List CompletionItem
        /// </summary>
        public void Import() {
            _keywords = null;

            // reads keywords
            _keywordById.Clear();
            Utils.ForEachLine(Config.FileKeywordsList, DataResources.KeywordsList, (i, line) => {
                var items = line.Split('\t');
                if (items.Length == 5) {
                    // find the KeywordType from items[1]
                    KeywordType keywordType;
                    if (!Enum.TryParse(items[2], true, out keywordType))
                        keywordType = KeywordType.Unknow;

                    // set flags
                    var flag = (items[3] == "1") ? ParseFlag.Reserved : 0;
                    if (keywordType == KeywordType.Abbreviation) flag = flag | ParseFlag.Abbreviation;

                    if (!_keywordById.ContainsKey(items[0])) {
                        KeywordCompletionItem curKeyword = CompletionItem.Factory.New((int) keywordType < 30 ? CompletionType.Keyword : CompletionType.KeywordObject) as KeywordCompletionItem;
                        if (curKeyword != null) {
                            curKeyword.DisplayText = items[1];
                            curKeyword.Ranking = int.Parse(items[4]);
                            curKeyword.Flags = flag;
                            curKeyword.KeywordType = keywordType;
                            _keywordById.Add(items[0], curKeyword);
                        }
                    }
                }
            }, Encoding.Default);

            // reads keywords rank
            Utils.ForEachLine(Config.FileKeywordsRank, new byte[0], (i, line) => {
                var items = line.Split('\t');
                if (items.Length == 2 && _keywordById.ContainsKey(items[0])) {
                    int val;
                    if (int.TryParse(items[1], out val)) {
                        _keywordById[items[0]].Ranking = val;
                    }
                }
            }, Encoding.Default);

            // reads abbreviations
            _abbreviations.Clear();
            Utils.ForEachLine(Config.FileAbbrev, DataResources.Abbreviations, (i, line) => {
                var items = line.Split('\t');
                if (items.Length == 2) {
                    _abbreviations.Add(new KeywordAbbreviation {
                        CompleteText = items[1],
                        ShortText = items[0]
                    });
                }
            }, Encoding.Default);

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
            }, Encoding.Default);

            if (OnImport != null)
                OnImport();
        }

        /// <summary>
        /// Save the keywords data into a file (to remember the ranking of each keyword)
        /// </summary>
        public void SaveRanking() {
            if (_keywordById.Count == 0) return;
            var strBuilder = new StringBuilder();
            foreach (var kpv in _keywordById) {
                strBuilder.AppendLine(kpv.Key + "\t" + kpv.Value.Ranking);
            }
            Utils.FileWriteAllText(Config.FileKeywordsRank, strBuilder.ToString());
        }

        #endregion

        #region public methods

        /// <summary>
        /// Allows to reset the list (when changing case for instance)
        /// </summary>
        public void ResetCompletionItems() {
            _keywords = _keywordById.Values.ToList();
            foreach (var keyword in _keywords) {
                keyword.DisplayText = keyword.DisplayText.ConvertCase(Config.Instance.AutoCompleteKeywordCaseMode);
            }
            if (OnImport != null)
                OnImport();
        }

        /// <summary>
        /// returns the list of keywords
        /// </summary>
        public List<CompletionItem> CompletionItems {
            get {
                if (_keywords == null) {
                    ResetCompletionItems();
                }
                return _keywords;
            }
        }

        /// <summary>
        /// returns the complete keyword for the given abbreviation
        /// </summary>
        /// <param name="abbreviation"></param>
        /// <returns></returns>
        public string GetFullKeyword(string abbreviation) {
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
        public KeywordHelp GetKeywordHelp(string key) {
            return _help.ContainsKey(key) ? _help[key] : null;
        }

        #endregion
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