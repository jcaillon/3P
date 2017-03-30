#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (NppLang.cs) is part of 3P.
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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using _3PA.Lib;
using _3PA.NppCore;

namespace _3PA.MainFeatures.AutoCompletionFeature {
    /// <summary>
    /// This class allows to read the file $NPPDIR/langs.xml that contains the different languages
    /// supported by npp; this file list the extensions for each lang as well as the keywords
    /// Once this is read, we can then read the file in $NPPINSTALL/plugins/APIs/ named "language_name.xml"
    /// that contains the extra keywords for the autocompletion
    /// Documentation here http://docs.notepad-plus-plus.org/index.php/Auto_Completion
    /// </summary>
    internal class NppLangs {
        #region Singleton

        private static NppLangs _instance;

        public static NppLangs Instance {
            get {
                if (_instance == null
                    //|| Utils.HasFileChanged(Config.FileNppLangsXml) 
                    || Utils.HasFileChanged(Npp.ConfXml.FileNppUserDefinedLang)
                    //|| Utils.HasFileChanged(Npp.Config.FileNppStylersPath)
                ) {
                    _instance = new NppLangs();
                }
                return _instance;
            }
        }

        #endregion

        #region Private fields

        /// <summary>
        /// To get from a file extension to a language name
        /// </summary>
        private Dictionary<string, string> _langNames = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// To get the description of a given language name
        /// </summary>
        private Dictionary<string, LangDescription> _langDescriptions = new Dictionary<string, LangDescription>(StringComparer.CurrentCultureIgnoreCase);

        #endregion

        #region Life and death

        /// <summary>
        /// Constructor
        /// </summary>
        public NppLangs() {
            // fill the dictionary extension -> lang name

            // from userDefinedLang.xml
            try {
                FillDictionaries(new NanoXmlDocument(Utils.ReadAllText(Npp.ConfXml.FileNppUserDefinedLang)).RootNode.SubNodes, true);
            } catch (Exception e) {
                ErrorHandler.LogError(e, "Error parsing " + Npp.ConfXml.FileNppUserDefinedLang);
            }

            // from langs.xml
            try {
                FillDictionaries(new NanoXmlDocument(Utils.ReadAllText(Npp.ConfXml.FileNppLangsXml)).RootNode["Languages"].SubNodes);
            } catch (Exception e) {
                ErrorHandler.LogError(e, "Error parsing " + Npp.ConfXml.FileNppLangsXml);
            }

            // from stylers.xml
            try {
                FillDictionaries(new NanoXmlDocument(Utils.ReadAllText(Npp.ConfXml.FileNppStylersXml)).RootNode["LexerStyles"].SubNodes);
            } catch (Exception e) {
                ErrorHandler.LogError(e, "Error parsing " + Npp.ConfXml.FileNppStylersXml);
            }
        }

        /// <summary>
        /// fill the _langNames and _langDescriptions dictionaries
        /// </summary>
        private void FillDictionaries(List<NanoXmlNode> elements, bool fromUserDefinedLang = false) {
            foreach (var lang in elements) {
                var nameAttr = lang.GetAttribute(@"name");
                var extAttr = lang.GetAttribute(@"ext");
                if (nameAttr != null && extAttr != null) {
                    var langName = nameAttr.Value.ToLower();
                    if (!_langDescriptions.ContainsKey(langName)) {
                        _langDescriptions.Add(langName, new LangDescription {
                            LangName = langName,
                            IsUserLang = fromUserDefinedLang
                        });
                        foreach (var ext in extAttr.Value.Split(' ')) {
                            var langExt = "." + ext.ToLower();
                            if (!_langNames.ContainsKey(langExt))
                                _langNames.Add(langExt, langName);
                        }
                    }
                }
            }
        }

        #endregion

        #region public

        /// <summary>
        /// Returns a language description for the given lang name
        /// </summary>
        public LangDescription GetLangDescription(string langName) {
            if (string.IsNullOrEmpty(langName) || !_langDescriptions.ContainsKey(langName))
                return null;
            return _langDescriptions[langName].ReadApiFileIfNeeded();
        }

        /// <summary>
        /// Returns a language description for the given extension (or null)
        /// </summary>
        public LangDescription GetLangDescriptionFromExtension(string fileExtention) {
            var langName = GetLangName(fileExtention);
            if (string.IsNullOrEmpty(langName) || !_langDescriptions.ContainsKey(langName))
                return null;
            return _langDescriptions[langName].ReadApiFileIfNeeded();
        }

        /// <summary>
        /// Returns a language name for the given extension (or null)
        /// </summary>
        public string GetLangName(string fileExtention) {
            return _langNames.ContainsKey(fileExtention) ? _langNames[fileExtention] : "normal";
        }

        #endregion

        #region LangDescription

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        internal class LangDescription {

            private List<CompletionItem> _autoCompletionItems;

            private List<NppKeyword> _keywords;

            /// <summary>
            /// Language name
            /// </summary>
            public string LangName { get; set; }

            /// <summary>
            /// Language read from userDefinedLang.xml
            /// </summary>
            public bool IsUserLang { get; set; }

            public string commentLine { get; set; }
            public string commentStart { get; set; }
            public string commentEnd { get; set; }
            public string ignoreCase { get; set; }
            public string startFunc { get; set; }
            public string stopFunc { get; set; }
            public string paramSeparator { get; set; }
            public string terminal { get; set; }
            public string additionalWordChar { get; set; }
            public char[] AdditionalWordChar { get; set; }

            /// <summary>
            /// A list of keywords for the language
            /// </summary>
            public List<CompletionItem> AutoCompletionItems {
                get {
                    if (_autoCompletionItems == null) {
                        _autoCompletionItems = _keywords.Select(keyword => {
                            var item = CompletionItem.Factory.New(keyword.Overloads != null ? CompletionType.LangFunction : CompletionType.LangWord) as LangCompletionItem;
                            if (item != null) {
                                item.DisplayText = keyword.Value;
                                item.NppKeyword = keyword;
                            }
                            return (CompletionItem) item;
                        }).ToList();
                    }
                    return _autoCompletionItems;
                }
            }

            /// <summary>
            /// Returns this after checking if we need to read the Api xml file for this language
            /// </summary>
            public LangDescription ReadApiFileIfNeeded() {

                var apiFilePath = Path.Combine(Npp.FolderNppAutocompApis, LangName + ".xml");
                if (_keywords != null
                    && !Utils.HasFileChanged(apiFilePath)
                    && (!IsUserLang || !Utils.HasFileChanged(Npp.ConfXml.FileNppUserDefinedLang)))
                    return this;

                _autoCompletionItems = null;
                _keywords = new List<NppKeyword>();
                var uniqueKeywords = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

                // get keywords from plugins/Apis/
                // FORMAT :
                // <AutoComplete language="C++">
                //    <Environment ignoreCase="no" startFunc="(" stopFunc=")" paramSeparator="," terminal=";" additionalWordChar = "."/>
                //    <KeyWord name="abs" func="yes">
                //        <Overload retVal="int" descr="Returns absolute value of given integer">
                //            <Param name="int number" />
                //        </Overload>
                //    </KeyWord>
                // </AutoComplete>
                try {
                    if (File.Exists(apiFilePath)) {
                        var xml = new NanoXmlDocument(Utils.ReadAllText(apiFilePath));
                        foreach (var keywordElmt in xml.RootNode["AutoComplete"].SubNodes.Where(node => node.Name.Equals("KeyWord"))) {
                            var attr = keywordElmt.GetAttribute("name");
                            if (attr == null)
                                continue;
                            var keyword = attr.Value;

                            if (!uniqueKeywords.Contains(keyword)) {
                                uniqueKeywords.Add(keyword);
                                List<NppKeyword.NppOverload> overloads = null;
                                foreach (var overload in keywordElmt.SubNodes.Where(node => node.Name.Equals("Overload"))) {
                                    if (overloads == null)
                                        overloads = new List<NppKeyword.NppOverload>();
                                    var xAttribute = overload.GetAttribute("retVal");
                                    var retVal = xAttribute != null ? xAttribute.Value : string.Empty;
                                    xAttribute = overload.GetAttribute("descr");
                                    var descr = xAttribute != null ? xAttribute.Value : string.Empty;
                                    var parameters = new List<string>();
                                    foreach (var para in overload.SubNodes.Where(node => node.Name.Equals("Param"))) {
                                        var attrname = para.GetAttribute("name");
                                        if (attrname == null)
                                            continue;
                                        parameters.Add(attrname.Value);
                                    }
                                    overloads.Add(new NppKeyword.NppOverload {
                                        ReturnValue = retVal,
                                        Description = descr,
                                        Params = parameters
                                    });
                                }

                                _keywords.Add(new NppKeyword(keyword) {
                                    Overloads = overloads,
                                    Origin = NppKeywordOrigin.AutoCompApiXml
                                });
                            }
                        }

                        // get other info on the language
                        var envElement = xml.RootNode["AutoComplete"]["Environment"];
                        if (envElement != null) {
                            LoadFromAttributes(this, envElement);
                            if (!string.IsNullOrEmpty(additionalWordChar))
                                AdditionalWordChar = additionalWordChar.ToArray();
                        }
                    }
                } catch (Exception e) {
                    ErrorHandler.LogError(e, "Error parsing " + apiFilePath);
                }

                // get core keywords from langs.xml or userDefinedLang.xml

                if (IsUserLang) {
                    try {
                        var langElement = new NanoXmlDocument(Utils.ReadAllText(Npp.ConfXml.FileNppUserDefinedLang)).RootNode.SubNodes.FirstOrDefault(x => x.GetAttribute("name").Value.EqualsCi(LangName));
                        if (langElement != null) {
                            // get the list of keywords from userDefinedLang.xml
                            foreach (var descendant in langElement["KeywordLists"].SubNodes) {
                                var xAttribute = descendant.GetAttribute(@"name");
                                if (xAttribute != null && xAttribute.Value.StartsWith("keywords", StringComparison.CurrentCultureIgnoreCase)) {
                                    foreach (var keyword in WebUtility.HtmlDecode(descendant.Value).Replace('\r', ' ').Replace('\n', ' ').Split(' ')) {
                                        if (!string.IsNullOrEmpty(keyword) && !uniqueKeywords.Contains(keyword)) {
                                            uniqueKeywords.Add(keyword);
                                            _keywords.Add(new NppKeyword(keyword) {
                                                Origin = NppKeywordOrigin.UserLangXml
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    } catch (Exception e) {
                        ErrorHandler.LogError(e, "Error parsing " + Npp.ConfXml.FileNppUserDefinedLang);
                    }
                } else {
                    try {
                        var langElement = new NanoXmlDocument(Utils.ReadAllText(Npp.ConfXml.FileNppLangsXml)).RootNode["Languages"].SubNodes.FirstOrDefault(x => x.GetAttribute("name").Value.EqualsCi(LangName));
                        if (langElement != null) {
                            // get the list of keywords from langs.xml
                            foreach (var descendant in langElement.SubNodes) {
                                foreach (var keyword in WebUtility.HtmlDecode(descendant.Value).Split(' ')) {
                                    if (!string.IsNullOrEmpty(keyword) && !uniqueKeywords.Contains(keyword)) {
                                        uniqueKeywords.Add(keyword);
                                        _keywords.Add(new NppKeyword(keyword) {
                                            Origin = NppKeywordOrigin.LangsXml
                                        });
                                    }
                                }
                            }

                            // get other info on the language (comentLine, commentStart, commentEnd)
                            LoadFromAttributes(this, langElement);
                        }
                    } catch (Exception e) {
                        ErrorHandler.LogError(e, "Error parsing " + Npp.ConfXml.FileNppLangsXml);
                    }
                }

                return this;
            }

            private void LoadFromAttributes(LangDescription item, NanoXmlNode itemElement) {
                var properties = typeof(LangDescription).GetProperties();

                /* loop through fields */
                foreach (var property in properties) {
                    if (property.PropertyType == typeof(string)) {
                        var attr = itemElement.GetAttribute(property.Name);
                        if (attr != null) {
                            var val = TypeDescriptor.GetConverter(property.PropertyType).ConvertFromInvariantString(attr.Value);
                            property.SetValue(item, val, null);
                        }
                    }
                }
            }
        }

        #endregion

        #region NppKeyword

        /// <summary>
        /// As described in the plugins/APIs/ files
        /// </summary>
        internal class NppKeyword {

            public string Value { get; set; }

            public NppKeywordOrigin Origin { get; set; }
            public List<NppOverload> Overloads { get; set; }

            internal class NppOverload {
                public string Description { get; set; }
                public string ReturnValue { get; set; }
                public List<string> Params { get; set; }
            }

            public NppKeyword(string value) {
                Value = value;
            }
        }

        internal enum NppKeywordOrigin {
            LangsXml,
            UserLangXml,
            AutoCompApiXml
        }

        #endregion
    }
}