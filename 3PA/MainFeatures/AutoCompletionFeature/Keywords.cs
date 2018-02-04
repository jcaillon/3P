#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
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
using _3PA.MainFeatures.Parser.Pro;
using _3PA.MainFeatures.SyntaxHighlighting;
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
        private Dictionary<string, KeywordCompletionItem> _keywordById = new Dictionary<string, KeywordCompletionItem>();
        private Dictionary<string, KeywordHelp> _helpByKey = new Dictionary<string, KeywordHelp>(StringComparer.CurrentCultureIgnoreCase);
        private List<CompletionItem> _keywords;
        private Dictionary<string, List<KeywordCompletionItem>> _keywordByName = new Dictionary<string, List<KeywordCompletionItem>>(StringComparer.CurrentCultureIgnoreCase);

        #endregion

        #region Life and death

        public Keywords() {
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
                if (items.Length == 7) {
                    // 0            1       2                       3               4               5
                    // keyword_id	keyword	minimal_abbreviation	keyword_type	0/1_is_reserved	integer_initial_ranking
                    // find the KeywordType from items[1]
                    KeywordType keywordType;
                    if (!Enum.TryParse(items[3].Trim(), true, out keywordType))
                        keywordType = KeywordType.Unknow;

                    SciStyleId styleId;
                    if (!Enum.TryParse(items[6].Trim(), true, out styleId))
                        styleId = SciStyleId.Default;

                    // set flags
                    var flag = items[4].Trim() == "1" ? ParseFlag.Reserved : 0;
                    var id = items[0].Trim();
                    var keyword = items[1].Trim();
                    var abbr = items[2].Trim();

                    if (!_keywordById.ContainsKey(id)) {
                        KeywordCompletionItem curKeyword = CompletionItem.Factory.New((int) keywordType < 30 ? CompletionType.Keyword : CompletionType.KeywordObject) as KeywordCompletionItem;
                        if (curKeyword != null) {
                            curKeyword.DisplayText = keyword;
                            curKeyword.Ranking = int.Parse(items[5].Trim());
                            curKeyword.Flags = flag;
                            curKeyword.KeywordType = keywordType;
                            curKeyword.KeywordSyntaxStyle = styleId;
                            _keywordById.Add(id, curKeyword);
                        }
                    }

                    // add abbreviation?
                    if (!string.IsNullOrEmpty(abbr)) {

                        if (!_keywordById.ContainsKey(abbr)) {
                            KeywordCompletionItem curKeyword = CompletionItem.Factory.New((int)keywordType < 30 ? CompletionType.Keyword : CompletionType.KeywordObject) as KeywordCompletionItem;
                            if (curKeyword != null) {
                                curKeyword.DisplayText = abbr;
                                curKeyword.FullWord = keyword;
                                curKeyword.Ranking = int.Parse(items[5].Trim());
                                curKeyword.Flags = flag | ParseFlag.Abbreviation;
                                curKeyword.KeywordType = keywordType;
                                curKeyword.KeywordSyntaxStyle = SciStyleId.Abbreviation;
                                _keywordById.Add(abbr, curKeyword);
                            }
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

            // reads keywords help
            _helpByKey.Clear();
            Utils.ForEachLine(Config.FileKeywordsHelp, DataResources.KeywordsHelp, (lineNb, line) => {
                var items = line.Split('\t');
                if (items.Length > 2) {
                    var listSynthax = new List<string>();
                    for (int i = 2; i < items.Length; i++) {
                        listSynthax.Add(items[i]);
                    }
                    _helpByKey.Add(items[0], new KeywordHelp {
                        Description = items[1],
                        Synthax = listSynthax
                    });
                }
            }, Encoding.Default);

            // add all the known keywords to a dictionary, adding also all intermediate abbreviations (vara vari varia variabl -> for var)
            _keywordByName.Clear();
            foreach (var keyword in _keywordById.Values.OrderBy(item => item.KeywordType)) {
                if (!_keywordByName.ContainsKey(keyword.DisplayText)) {
                    _keywordByName.Add(keyword.DisplayText, new List<KeywordCompletionItem> {keyword});
                } else {
                    _keywordByName[keyword.DisplayText].Add(keyword);
                }
                if (keyword.Flags.HasFlag(ParseFlag.Abbreviation)) {
                    var offset = 1;
                    while (keyword.FullWord.Length - offset > keyword.DisplayText.Length) {
                        var intermediateAbbreviation = keyword.FullWord.Substring(0, keyword.FullWord.Length - offset);
                        if (!_keywordByName.ContainsKey(intermediateAbbreviation)) {
                            _keywordByName.Add(intermediateAbbreviation, new List<KeywordCompletionItem> { keyword });
                        } else {
                            _keywordByName[intermediateAbbreviation].Add(keyword);
                        }
                        offset++;
                    }
                }
            }

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
            _keywords = _keywordById.Values.Cast<CompletionItem>().ToList();
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
            return _keywordByName.ContainsKey(abbreviation) ? _keywordByName[abbreviation].First().FullWord : null;
        }

        /// <summary>
        /// Returns the help for a specified key (KEYWORD KEYWORDTYPE) for instance : "RELEASE Statement"
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeywordHelp GetKeywordHelp(string key) {
            return _helpByKey.ContainsKey(key) ? _helpByKey[key] : null;
        }

        /// <summary>
        /// Returns a list of keywords corresponding to a name (i.e. variable will return the keywords with the display text variable; varia will return the keyword with the display text var which is the abbreviation of the keyword var)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<KeywordCompletionItem> GetKeywordsByName(string name) {
            return _keywordByName.ContainsKey(name) ? _keywordByName[name] : null;
        }

        #endregion

    }

    internal class KeywordHelp {
        public string Description;
        public List<string> Synthax;
    }
}