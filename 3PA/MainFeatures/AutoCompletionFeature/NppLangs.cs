using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using _3PA.Lib;

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
                    || Utils.HasFileChanged(Config.FileNppUserDefinedLang)
                    || Utils.HasFileChanged(NppConfig.Instance.FileNppStylersPath)) {
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
            FillNameDictionary(XDocument.Load(Config.FileNppUserDefinedLang).Descendants("UserLang").ToList(), true);

            // from langs.xml
            FillNameDictionary(XDocument.Load(Config.FileNppLangsXml).Descendants("Language").ToList());

            // from stylers.xml
            FillNameDictionary(XDocument.Load(NppConfig.Instance.FileNppStylersPath).Descendants("LexerType").ToList());
        }

        private void FillNameDictionary(List<XElement> elements, bool fromUserDefinedLang = false) {
            foreach (var lang in elements) {
                var nameAttr = lang.Attribute("name");
                var extAttr = lang.Attribute("ext");
                if (nameAttr != null && extAttr != null) {
                    if (!_langDescriptions.ContainsKey(nameAttr.Value)) {
                        _langDescriptions.Add(nameAttr.Value, new LangDescription { LangName = nameAttr.Value, IsUserLang = fromUserDefinedLang });
                        foreach (var ext in extAttr.Value.Split(' ')) {
                            if (!_langNames.ContainsKey("." + ext))
                                _langNames.Add("." + ext, nameAttr.Value);
                        }
                    }
                }
            }
        }

        #endregion

        #region public

        /// <summary>
        /// Returns a language description for the given extension (or null)
        /// </summary>
        public LangDescription GetLangDescription(string fileExtention) {
            var langName = GetLangName(fileExtention);
            if (string.IsNullOrEmpty(langName) || !_langDescriptions.ContainsKey(langName))
                return null;
            return _langDescriptions[langName];
        }

        /// <summary>
        /// Returns a language name for the given extension (or null)
        /// </summary>
        public string GetLangName(string fileExtention) {
            return _langNames.ContainsKey(fileExtention) ? _langNames[fileExtention] : string.Empty;
        }

        #endregion

        #region LangDescription

        internal class LangDescription {

            private List<NppKeyword> _keywords;

            /// <summary>
            /// Language name
            /// </summary>
            public string LangName { get; set; }

            /// <summary>
            /// A list of keywords for the language
            /// </summary>
            public List<NppKeyword> Keywords {
                get {
                    var apiFilePath = Path.Combine(Config.FolderNppAutocompApis, LangName + ".xml");
                    if (_keywords != null 
                        && !Utils.HasFileChanged(apiFilePath)
                        && (!IsUserLang || !Utils.HasFileChanged(Config.FileNppUserDefinedLang)))
                        return _keywords;

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
                        if (File.Exists(apiFilePath))
                            foreach (var keywordElmt in XDocument.Load(apiFilePath).Descendants("KeyWord")) {
                                var attr = keywordElmt.Attribute("name");
                                if (attr == null)
                                    continue;
                                var keyword = attr.Value;

                                if (!uniqueKeywords.Contains(keyword)) {
                                    uniqueKeywords.Add(keyword);
                                    List<NppKeyword.NppOverload> overloads = null;
                                    foreach (var overload in keywordElmt.Descendants("Overload")) {
                                        if (overloads == null)
                                            overloads = new List<NppKeyword.NppOverload>();
                                        var xAttribute = overload.Attribute("retVal");
                                        var retVal = xAttribute != null ? xAttribute.Value : string.Empty;
                                        xAttribute = overload.Attribute("descr");
                                        var descr = xAttribute != null ? xAttribute.Value : string.Empty;
                                        var parameters = new List<string>();
                                        foreach (var para in overload.Descendants("Param")) {
                                            var attrname = para.Attribute("name");
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

                                    _keywords.Add(new NppKeyword {
                                        Name = keyword,
                                        Overloads = overloads
                                    });
                                }
                            }
                    } catch (Exception) {
                        // ignored
                    }

                    // get core keywords from langs.xml or userDefinedLang.xml
                    try {
                        if (IsUserLang) {

                            // get the list of keywords from userDefinedLang.xml
                            var langElement = XDocument.Load(Config.FileNppUserDefinedLang).Descendants("UserLang").FirstOrDefault(x => x.Attribute("name").Value.EqualsCi(LangName));
                            if (langElement != null)
                                foreach (var descendant in langElement.Descendants("Keywords")) {
                                    var xAttribute = descendant.Attribute(@"name");
                                    if (xAttribute != null && xAttribute.Value.ToLower().StartsWith("keywords"))
                                        foreach (var keyword in WebUtility.HtmlDecode(descendant.Value).Replace('\r', ' ').Replace('\n', ' ').Split(' ')) {
                                            if (!string.IsNullOrEmpty(keyword) && !uniqueKeywords.Contains(keyword)) {
                                                uniqueKeywords.Add(keyword);
                                                _keywords.Add(new NppKeyword {
                                                    Name = keyword
                                                });
                                            }
                                        }
                                }
                        } else {

                            // get the list of keywords from langs.xml
                            var langElement = XDocument.Load(Config.FileNppLangsXml).Descendants("Language").FirstOrDefault(x => x.Attribute("name").Value.EqualsCi(LangName));
                            if (langElement != null)
                                foreach (var descendant in langElement.Descendants("Keywords")) {
                                    foreach (var keyword in WebUtility.HtmlDecode(descendant.Value).Split(' ')) {
                                        if (!string.IsNullOrEmpty(keyword) && !uniqueKeywords.Contains(keyword)) {
                                            uniqueKeywords.Add(keyword);
                                            _keywords.Add(new NppKeyword {
                                                Name = keyword
                                            });
                                        }
                                    }
                                }
                        }
                    } catch (Exception) {
                        // ignored
                    }

                    return _keywords;
                }
            }

            public char[] AdditionalWordChar { get; set; }

            /// <summary>
            /// Language read from userDefinedLang.xml
            /// </summary>
            public bool IsUserLang { get; set; }
        }
        
        #endregion

        #region NppKeyword

        /// <summary>
        /// As described in the plugins/APIs/ files
        /// </summary>
        internal class NppKeyword {
            public string Name { get; set; }
            public List<NppOverload> Overloads { get; set; }

            internal class NppOverload {
                public string Description { get; set; }
                public string ReturnValue { get; set; }
                public List<string> Params { get; set; }
            }
        }

        #endregion


    }
}
