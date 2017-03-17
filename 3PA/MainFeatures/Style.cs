#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (Style.cs) is part of 3P.
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using YamuiFramework.Helper;
using _3PA.Lib;
using _3PA.MainFeatures.Pro;
using _3PA.NppCore;
using _3PA._Resource;

namespace _3PA.MainFeatures {
    /// <summary>
    /// This class handles the Styles of scintilla
    /// </summary>
    internal static class Style {

        #region Current theme

        private static List<StyleTheme> _listOfThemes = new List<StyleTheme>();
        private static StyleTheme _currentTheme;

        /// <summary>
        /// handles the current theme
        /// </summary>
        public static StyleTheme Current {
            get {
                if (_currentTheme == null)
                    Current = GetThemesList.ElementAt(Config.Instance.SyntaxHighlightThemeId);
                return _currentTheme;
            }
            set {
                _currentTheme = value;
                try {
                    _currentTheme.SetColorValues(typeof(StyleTheme));
                } catch (Exception e) {
                    ErrorHandler.ShowErrors(e, "Loading a theme");
                }
            }
        }

        /// <summary>
        /// Returns the list of all available themes
        /// </summary>
        /// <returns></returns>
        public static List<StyleTheme> GetThemesList {
            get {
                // get the list of themes from the user's file or from the ressource by default
                if (_listOfThemes.Count == 0)
                    _listOfThemes = GenericThemeHolder.ReadThemeFile<StyleTheme>(Config.FileSyntaxThemes, DataResources.SyntaxThemes, Encoding.Default);
                if (Config.Instance.SyntaxHighlightThemeId < 0 || Config.Instance.SyntaxHighlightThemeId >= _listOfThemes.Count)
                    Config.Instance.SyntaxHighlightThemeId = 0;
                return _listOfThemes;
            }
        }

        /// <summary>
        /// Called when the list of themes is imported
        /// </summary>
        public static void ImportList() {
            _listOfThemes.Clear();
            _currentTheme = null;
            Plug.ApplyOptionsForScintilla();
        }

        #endregion

        #region const

        /// <summary>
        /// for the Errors we use scintilla's styles, we offset the ErrorLevel by this amount to get the style ID
        /// </summary>
        public const int ErrorAnnotStandardStyleOffset = 240;

        public const int ErrorAnnotBoldStyleOffset = 230;
        public const int ErrorAnnotItalicStyleOffset = 220;

        #endregion

        #region Install UDL

        /// <summary>
        /// check if the User Defined Language for "OpenEdgeABL" exists in the
        /// userDefineLang.xml file, if it does it updates it, if it doesn't exists it creates it and asks the user
        /// to restart Notepad++
        /// Can also only check and not install it by setting onlyCheckInstall to true
        /// </summary>
        public static bool InstallUdl(bool onlyCheckInstall = false) {
            var encoding = TextEncodingDetect.GetFileEncoding(Npp.ConfXml.FileNppUserDefinedLang);
            var fileContent = File.Exists(Npp.ConfXml.FileNppUserDefinedLang) ? Utils.ReadAllText(Npp.ConfXml.FileNppUserDefinedLang, encoding) : @"<NotepadPlus />";
            var regex = new Regex("<UserLang name=\"OpenEdgeABL\".*?</UserLang>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var matches = regex.Match(fileContent);
            if (matches.Success) {
                if (onlyCheckInstall)
                    return true;
                // if it already exists in the file, delete the existing one
                fileContent = regex.Replace(fileContent, @"");
            } else {
                if (onlyCheckInstall)
                    return false;
                // if it doesn't exist in the file
                UserCommunication.Notify("It seems to be the first time that you use this plugin.<br>In order to activate the syntax highlighting, you must restart notepad++.<br><br><i>Please note that if a document is opened at the next start, you will have to manually close/reopen it to see the changes.</i><br><br><b>" + "Restart".ToHtmlLink("Click here to restart now!") + "</b>", MessageImg.MsgInfo, "Information", "Installing syntax highlighting",
                    args => {
                        args.Handled = true;
                        Npp.Restart();
                    });
            }
            if (fileContent.ContainsFast(@"<NotepadPlus />"))
                fileContent = fileContent.Replace(@"<NotepadPlus />", "<NotepadPlus>\r\n" + DataResources.UDL + "\r\n</NotepadPlus>");
            else
                fileContent = fileContent.Replace(@"<NotepadPlus>", "<NotepadPlus>\r\n" + DataResources.UDL);
            // write to userDefinedLang.xml
            if (!Utils.FileWriteAllText(Npp.ConfXml.FileNppUserDefinedLang, fileContent, encoding))
                UserCommunication.Notify("<b>Couldn't access the file :</b><br>" + Npp.ConfXml.FileNppUserDefinedLang + "<br><br>This means i couldn't correctly applied the syntax highlighting feature!<br><br><i>Please make sure to allow write access to this file (Right click on file > Security > Check what's needed to allow total control to current user)</i>", MessageImg.MsgError, "Syntax highlighting", "Can't access userDefineLang.xml");
            return true;
        }

        #endregion

        #region set styles

        /// <summary>
        /// Call this method to set the back/fore color and font type of each type used in 3P according to the 
        /// styles defined in the SyntaxHighlighting file
        /// </summary>
        public static void SetSyntaxStyles() {
            var curTheme = Current;

            if (Config.Instance.UseSyntaxHighlightTheme) {
                // Default
                SetFontStyle((byte) SciMsg.STYLE_DEFAULT, curTheme.Default);
                // Npp.StyleClearAll(); // to apply the default style to all styles
                SetFontStyle((byte) SciMsg.STYLE_CONTROLCHAR, curTheme.Default);
                SetFontStyle((byte) UdlStyles.Idk, curTheme.Default);
                SetFontStyle((byte) UdlStyles.Default, curTheme.Default);

                // categories
                SetFontStyle((byte) UdlStyles.Comment, curTheme.Comment);
                SetFontStyle((byte) UdlStyles.CommentLine, curTheme.PreProcessed);
                SetFontStyle((byte) UdlStyles.Number, curTheme.Numbers);
                SetFontStyle((byte) UdlStyles.KeyWordsList1, curTheme.JumpStatement);
                SetFontStyle((byte) UdlStyles.KeyWordsList2, curTheme.Statement);
                SetFontStyle((byte) UdlStyles.KeyWordsList3, curTheme.VarType);
                SetFontStyle((byte) UdlStyles.KeyWordsList4, curTheme.OtherKeywords);
                SetFontStyle((byte) UdlStyles.KeyWordsList5, curTheme.PreProcessed);
                SetFontStyle((byte) UdlStyles.KeyWordsList6, curTheme.NormedVariables);
                SetFontStyle((byte) UdlStyles.KeyWordsList7, curTheme.Abbreviations);
                SetFontStyle((byte) UdlStyles.KeyWordsList8, curTheme.SpecialWord);
                SetFontStyle((byte) UdlStyles.Operators, curTheme.Operators);
                SetFontStyle((byte) UdlStyles.FolderInCode2, curTheme.Statement);
                SetFontStyle((byte) UdlStyles.Delimiter1, curTheme.DoubleQuote);
                SetFontStyle((byte) UdlStyles.Delimiter2, curTheme.SimpleQuote);
                SetFontStyle((byte) UdlStyles.Delimiter3, curTheme.Includes);
                SetFontStyle((byte) UdlStyles.Delimiter4, curTheme.DoubleQuote);
                SetFontStyle((byte) UdlStyles.Delimiter5, curTheme.SimpleQuote);
                SetFontStyle((byte) UdlStyles.Delimiter7, curTheme.SingleLineComment);
                SetFontStyle((byte) UdlStyles.Delimiter8, curTheme.NestedComment);

                // line numbers
                SetFontStyle((byte) SciMsg.STYLE_LINENUMBER, curTheme.LineNumberMargin);

                // set url as strings
                SetFontStyle(80, curTheme.SimpleQuote);

                // brace highlighting
                SetFontStyle((byte) SciMsg.STYLE_BRACELIGHT, curTheme.BraceHighLight);
                SetFontStyle((byte) SciMsg.STYLE_BRACEBAD, curTheme.BadBraceHighLight);

                // smart highlighting in npp
                Sci.GetIndicator(29).ForeColor = curTheme.SmartHighLighting.ForeColor;
            }

            // Setting styles for errors 
            SetErrorStyles((byte) ErrorLevel.Information, curTheme.Error0.BackColor, curTheme.Error0.ForeColor);
            SetErrorStyles((byte) ErrorLevel.Warning, curTheme.Error1.BackColor, curTheme.Error1.ForeColor);
            SetErrorStyles((byte) ErrorLevel.StrongWarning, curTheme.Error2.BackColor, curTheme.Error2.ForeColor);
            SetErrorStyles((byte) ErrorLevel.Error, curTheme.Error3.BackColor, curTheme.Error3.ForeColor);
            SetErrorStyles((byte) ErrorLevel.Critical, curTheme.Error4.BackColor, curTheme.Error4.ForeColor);
        }

        public static void SetFontStyle(byte styleNumber, StyleThemeItem styleItem) {
            var nppStyle = Sci.GetStyle(styleNumber);

            if (styleItem.BackColor != Color.Transparent)
                nppStyle.BackColor = styleItem.BackColor;

            if (styleItem.ForeColor != Color.Transparent)
                nppStyle.ForeColor = styleItem.ForeColor;

            if (styleItem.FontType > 0) {
                nppStyle.Bold = styleItem.FontType.IsBitSet(1);
                nppStyle.Italic = styleItem.FontType.IsBitSet(2);
            }

            if (!string.IsNullOrEmpty(styleItem.FontName))
                nppStyle.Font = styleItem.FontName;
        }

        /// <summary>
        /// Sets a style for an Error annotation (reduced font + segoe ui) and for markers
        /// </summary>
        /// <param name="errorLevel"></param>
        /// <param name="bgColor"></param>
        /// <param name="fgColor"></param>
        private static void SetErrorStyles(byte errorLevel, Color bgColor, Color fgColor) {
            int curFontSize = Sci.GetStyle(0).Size;

            var normalStyle = Sci.GetStyle(FilesInfo.GetStyleOf((ErrorLevel) errorLevel, ErrorFontWeight.Normal));
            normalStyle.Font = "Segoe ui";
            normalStyle.Size = (int) (curFontSize*0.9);
            normalStyle.ForeColor = fgColor;
            normalStyle.BackColor = bgColor;

            var boldStyle = Sci.GetStyle(FilesInfo.GetStyleOf((ErrorLevel) errorLevel, ErrorFontWeight.Bold));
            boldStyle.Font = "Segoe ui";
            boldStyle.Size = (int) (curFontSize*0.9);
            boldStyle.Bold = true;
            boldStyle.ForeColor = fgColor;
            boldStyle.BackColor = bgColor;

            var italicStyle = Sci.GetStyle(FilesInfo.GetStyleOf((ErrorLevel) errorLevel, ErrorFontWeight.Italic));
            italicStyle.Font = "Segoe ui";
            italicStyle.Size = (int) (curFontSize*0.9);
            italicStyle.Italic = true;
            italicStyle.ForeColor = fgColor;
            italicStyle.BackColor = bgColor;

            var markerStyle = Sci.GetMarker(errorLevel);
            markerStyle.Symbol = MarkerSymbol.SmallRect;
            markerStyle.SetBackColor(bgColor);
            markerStyle.SetForeColor(fgColor);
        }

        #endregion

        #region public methods

        /// <summary>
        /// Is the caret not in : an include, a string, a comment
        /// </summary>
        /// <returns></returns>
        public static bool IsCarretInNormalContext(int curPos) {
            try {
                var curContext = (UdlStyles) Sci.GetStyleAt(curPos);
                if (curPos <= 0) return true;
                if (IsNormalContext(curContext)) return true;
                var prevContext = (UdlStyles) Sci.GetStyleAt(curPos - 1);
                return IsNormalContext(prevContext);
            } catch (Exception) {
                // we can be here if the style ID isn't in the UdlStyles enum
                return true;
            }
        }

        /// <summary>
        /// Is the caret not in : a string, a comment
        /// </summary>
        /// <returns></returns>
        public static bool IsNormalContext(UdlStyles context) {
            return (context != UdlStyles.Comment
                    && context != UdlStyles.Delimiter1
                    && context != UdlStyles.Delimiter2
                    && context != UdlStyles.Delimiter4
                    && context != UdlStyles.Delimiter5
                    && context != UdlStyles.Delimiter7
                    && context != UdlStyles.Delimiter8);
        }

        #endregion
    }

    #region UDL style numbers

    /// <summary>
    /// Enumeration of the style id used by the UDL
    /// </summary>
    public enum UdlStyles {
        Default = 0,
        Comment = 1,
        CommentLine = 2,
        Number = 3,
        KeyWordsList1 = 4, // Jump in code (RUN, RETURN, LEAVE...)
        KeyWordsList2 = 5, // statements (DEFINE VARIABLE..)
        KeyWordsList3 = 6, // VAR types (BLOB, DATE, INTEGER)
        KeyWordsList4 = 7, // all other keywords
        KeyWordsList5 = 8, // preprocessed words (&if &global...)
        KeyWordsList6 = 9, // variables prefix (gc_, li_...)
        KeyWordsList7 = 10, // abbreviations
        KeyWordsList8 = 11, // user trigram
        Operators = 12, // also includes (matches, not...)
        FolderInCode1 = 13,
        FolderInCode2 = 14, // Collapsable blocks, (FOR EACH: END.)
        FolderInComment = 15,
        Delimiter1 = 16, // string double quote
        Delimiter2 = 17, // string simple
        Delimiter3 = 18, // include { }
        Delimiter4 = 19, // string double quote in single line comment (preproc definition)
        Delimiter5 = 20, // string simple quote in single line comment (preproc definition)
        Delimiter6 = 21, // 
        Delimiter7 = 22, // single line comment for Progress 11.6
        Delimiter8 = 23, // nested comment
        Idk = 24 // ??
    }

    #endregion

    #region StyleTheme class

    public class StyleTheme : GenericThemeHolder {
        public StyleThemeItem Default = new StyleThemeItem();
        public StyleThemeItem Comment = new StyleThemeItem();
        public StyleThemeItem NestedComment = new StyleThemeItem();
        public StyleThemeItem SingleLineComment = new StyleThemeItem();
        public StyleThemeItem PreProcessed = new StyleThemeItem();
        public StyleThemeItem JumpStatement = new StyleThemeItem();
        public StyleThemeItem Statement = new StyleThemeItem();
        public StyleThemeItem VarType = new StyleThemeItem();
        public StyleThemeItem OtherKeywords = new StyleThemeItem();
        public StyleThemeItem Operators = new StyleThemeItem();
        public StyleThemeItem Abbreviations = new StyleThemeItem();
        public StyleThemeItem Includes = new StyleThemeItem();
        public StyleThemeItem DoubleQuote = new StyleThemeItem();
        public StyleThemeItem SimpleQuote = new StyleThemeItem();
        public StyleThemeItem NormedVariables = new StyleThemeItem();
        public StyleThemeItem SpecialWord = new StyleThemeItem();
        public StyleThemeItem CaretLine = new StyleThemeItem();
        public StyleThemeItem Selection = new StyleThemeItem();
        public StyleThemeItem WhiteSpace = new StyleThemeItem();
        public StyleThemeItem Numbers = new StyleThemeItem();
        public StyleThemeItem NoError = new StyleThemeItem();
        public StyleThemeItem Error0 = new StyleThemeItem();
        public StyleThemeItem Error1 = new StyleThemeItem();
        public StyleThemeItem Error2 = new StyleThemeItem();
        public StyleThemeItem Error3 = new StyleThemeItem();
        public StyleThemeItem Error4 = new StyleThemeItem();
        public StyleThemeItem CaretColor = new StyleThemeItem();
        public StyleThemeItem LineNumberMargin = new StyleThemeItem();
        public StyleThemeItem FoldMargin = new StyleThemeItem();
        public StyleThemeItem FoldActiveMarker = new StyleThemeItem();
        public StyleThemeItem SmartHighLighting = new StyleThemeItem();
        public StyleThemeItem BraceHighLight = new StyleThemeItem();
        public StyleThemeItem BadBraceHighLight = new StyleThemeItem();

        /// <summary>
        /// Set the values of this instance, using a dictionnary of key -> values, override for this class
        /// </summary>
        public new void SetColorValues(Type thisType) {
            if (SavedStringValues == null)
                return;

            // for each field of this object, try to assign its value with the _savedStringValues dico
            foreach (var fieldInfo in thisType.GetFields().Where(fieldInfo => SavedStringValues.ContainsKey(fieldInfo.Name) && fieldInfo.DeclaringType == thisType && fieldInfo.FieldType == typeof(StyleThemeItem))) {
                try {
                    var value = SavedStringValues[fieldInfo.Name];
                    var items = value.Split('\t');
                    if (items.Length >= 3) {
                        int fontType;
                        if (!int.TryParse(items[2].Trim(), out fontType))
                            fontType = 0;
                        fieldInfo.SetValue(this, new StyleThemeItem {
                            ForeColor = ColorTranslator.FromHtml(GetHtmlColor(items[0].Trim(), 0)),
                            BackColor = ColorTranslator.FromHtml(GetHtmlColor(items[1].Trim(), 1)),
                            FontType = fontType,
                            FontName = items.Length >= 4 ? items[3].Trim() : string.Empty
                        });
                    }
                } catch (Exception e) {
                    throw new Exception("Reading styles, couldn't understand the line : <" + SavedStringValues[fieldInfo.Name] + "> for the field <" + fieldInfo.Name + "> and for the theme <" + ThemeName + "> : " + e);
                }
            }
        }

        /// <summary>
        /// Find the html color behing any property
        /// </summary>
        private string GetHtmlColor(string propertyName, int propNumber) {
            return ReplaceAliases(propertyName, propNumber).ApplyColorFunctions();
        }

        private string ReplaceAliases(string value, int propNumber) {
            while (true) {
                if (value.Contains("@")) {
                    // try to replace a variable name by it's html color value
                    var regex = new Regex(@"@([a-zA-Z]*)", RegexOptions.IgnoreCase);
                    value = regex.Replace(value, match => {
                        if (SavedStringValues.ContainsKey(match.Groups[1].Value))
                            return SavedStringValues[match.Groups[1].Value].Split('\t')[propNumber];
                        throw new Exception("Couldn't find the color " + match.Groups[1].Value + "!");
                    });
                    continue;
                }
                return value;
            }
        }

        private StyleThemeItem GetErrorItem(int errorLevel) {
            switch (errorLevel) {
                case 0:
                    return NoError;
                case 1:
                    return Error0;
                case 2:
                    return Error1;
                case 3:
                    return Error2;
                case 4:
                    return Error3;
                case 5:
                    return Error4;
            }
            return new StyleThemeItem {BackColor = Color.Beige, ForeColor = Color.Black};
        }

        public Color GetErrorBg(int errorLevel) {
            return GetErrorItem(errorLevel).BackColor;
        }

        public Color GetErrorFg(int errorLevel) {
            return GetErrorItem(errorLevel).ForeColor;
        }
    }

    public class StyleThemeItem {
        public Color BackColor = Color.Transparent;
        public Color ForeColor = Color.Transparent;
        public int FontType;
        public string FontName;
    }

    #endregion
}