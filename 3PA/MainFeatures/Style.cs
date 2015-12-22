#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
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
using _3PA.Data;
using _3PA.Html;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures.FilesInfoNs;

namespace _3PA.MainFeatures {

    /// <summary>
    /// This class handles the STYLENEEDED notification of scintilla
    /// </summary>
    public class Style {

        #region fields

        /// <summary>
        /// You can set this property to read the theme.xml file from a local path instead of
        /// the embedded ressource file
        /// </summary>
        public static string ThemeXmlPath;

        /// <summary>
        /// List of themes
        /// </summary>
        private static List<StyleTheme> _listOfThemes = new List<StyleTheme>();

        private static StyleTheme _currentTheme;

        #endregion

        #region Current theme

        /// <summary>
        /// handles the current theme
        /// </summary>
        public static StyleTheme CurrentTheme {
            set { _currentTheme = value; }
            get {
                if (_currentTheme != null)
                    return _currentTheme;
                // instanciation of current theme
                _currentTheme = GetThemesList().ElementAt(Config.Instance.SyntaxHighlightThemeId) ?? GetThemesList()[0];
                return _currentTheme;
            }
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
            var udlFilePath = Path.Combine(Npp.GetConfigDir(), @"../../../userDefineLang.xml");
            var fileContent = File.Exists(udlFilePath) ? File.ReadAllText(udlFilePath, Encoding.Default) : @"<NotepadPlus />";
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
                UserCommunication.Notify("It seems to be the first time that you use this plugin.<br>In order to activate the syntax highlighting, you must restart notepad++.<br><br><i>Please note that if a document is opened at the next start, you will have to manually close/reopen it to see the changes.</i><br><br><b>Sorry for the inconvenience</b>!", MessageImg.MsgInfo, "Information", "Installing syntax highlighting");
            }
            if (fileContent.ContainsFast(@"<NotepadPlus />"))
                fileContent = fileContent.Replace(@"<NotepadPlus />", "<NotepadPlus>\r\n" + DataResources.UDL + "\r\n</NotepadPlus>");
            else
                fileContent = fileContent.Replace(@"<NotepadPlus>", "<NotepadPlus>\r\n" + DataResources.UDL);
            // write to userDefinedLang.xml
            try {
                File.WriteAllText(udlFilePath, fileContent, Encoding.Default);
            } catch (Exception e) {
                if (e is UnauthorizedAccessException)
                    UserCommunication.Notify("<b>Couldn't access the file :</b><br>" + udlFilePath + "<br><br>This means i couldn't correctly applied the syntax highlighting feature!<br><br><i>Please make sure to allow write access to this file (Right click on file > Security > Check what's needed to allow total control to current user)</i>", MessageImg.MsgError, "Syntax highlighting", "Can't access userDefineLang.xml");
                else
                    ErrorHandler.ShowErrors(e, "Error while accessing userDefineLang.xml");
                return false;
            }
            return true;
        }

        #endregion

        #region set styles

        public static Npp.Style StyleDefault = Npp.GetStyle((byte)UdlStyles.Default);
        public static Npp.Style StyleComment = Npp.GetStyle((byte)UdlStyles.Comment);
        public static Npp.Style StyleCommentLine = Npp.GetStyle((byte)UdlStyles.CommentLine);
        public static Npp.Style StyleNumber = Npp.GetStyle((byte)UdlStyles.Number);
        public static Npp.Style StyleJumpInCode = Npp.GetStyle((byte)UdlStyles.KeyWordsList1);
        public static Npp.Style StyleWordStatement = Npp.GetStyle((byte)UdlStyles.KeyWordsList2);
        public static Npp.Style StyleVarType = Npp.GetStyle((byte)UdlStyles.KeyWordsList3);
        public static Npp.Style StyleOtherKeyword = Npp.GetStyle((byte)UdlStyles.KeyWordsList4);
        public static Npp.Style StylePreprocessed = Npp.GetStyle((byte)UdlStyles.KeyWordsList5);
        public static Npp.Style StyleVariablesPrefix = Npp.GetStyle((byte)UdlStyles.KeyWordsList6);
        public static Npp.Style StyleAbbreviation = Npp.GetStyle((byte)UdlStyles.KeyWordsList7);
        public static Npp.Style StyleTrigram = Npp.GetStyle((byte)UdlStyles.KeyWordsList8);
        public static Npp.Style StyleOperators = Npp.GetStyle((byte)UdlStyles.Operators);
        public static Npp.Style StyleBlockStartKeyword = Npp.GetStyle((byte)UdlStyles.FolderInCode2);
        public static Npp.Style StyleStrDoubleQuote = Npp.GetStyle((byte)UdlStyles.Delimiter1);
        public static Npp.Style StyleStrSimple = Npp.GetStyle((byte)UdlStyles.Delimiter2);
        public static Npp.Style StyleInclude = Npp.GetStyle((byte)UdlStyles.Delimiter3);
        public static Npp.Style StyleStrDoubleQuoteComm = Npp.GetStyle((byte)UdlStyles.Delimiter4);
        public static Npp.Style StyleStrSimpleComm = Npp.GetStyle((byte)UdlStyles.Delimiter5);
        public static Npp.Style StyleNestedComment = Npp.GetStyle((byte)UdlStyles.Delimiter8);

        public static List<Color> BgErrorLevelColors;
        public static List<Color> FgErrorLevelColors;

        public static void SetSyntaxStyles() {

            var curTheme = CurrentTheme;

            // setting styles for the syntax highlighting
            StyleDefault.ForeColor = curTheme.FgDefault;
            StyleComment.ForeColor = curTheme.FgComment;
            StyleCommentLine.ForeColor = curTheme.FgLineComment;
            StyleNumber.ForeColor = curTheme.FgNumbers;
            StyleJumpInCode.ForeColor = curTheme.FgKeyword1;
            StyleWordStatement.ForeColor = curTheme.FgKeyword2;
            StyleVarType.ForeColor = curTheme.FgKeyword3;
            StyleOtherKeyword.ForeColor = curTheme.FgKeyword4;
            StylePreprocessed.ForeColor = curTheme.FgKeyword5;
            StyleVariablesPrefix.ForeColor = curTheme.FgKeyword6;
            StyleAbbreviation.ForeColor = curTheme.FgKeyword7;
            StyleTrigram.ForeColor = curTheme.FgKeyword8;
            StyleOperators.ForeColor = curTheme.FgOperators;
            StyleBlockStartKeyword.ForeColor = curTheme.FgFoldInCode2;
            StyleStrDoubleQuote.ForeColor = curTheme.FgDelimiters1;
            StyleStrSimple.ForeColor = curTheme.FgDelimiters2;
            StyleInclude.ForeColor = curTheme.FgDelimiters3;
            StyleStrDoubleQuoteComm.ForeColor = curTheme.FgDelimiters4;
            StyleStrSimpleComm.ForeColor = curTheme.FgDelimiters5;
            StyleNestedComment.ForeColor = curTheme.FgDelimiters8;

            StyleDefault.BackColor = curTheme.BgDefault;
            StyleJumpInCode.BackColor = curTheme.BgKeyword1;
            StyleTrigram.BackColor = curTheme.BgKeyword8;
            StyleInclude.BackColor = curTheme.BgDelimiters3;
            
            // set style 33 for the margin with line numbers
        }

        public static void SetGeneralStyles() {

            var curTheme = CurrentTheme;
            
            // Setting styles for errors 
            SetErrorStyles((byte)ErrorLevel.Information, curTheme.BgError0, curTheme.FgError0);
            SetErrorStyles((byte)ErrorLevel.Warning, curTheme.BgError1, curTheme.FgError1);
            SetErrorStyles((byte)ErrorLevel.StrongWarning, curTheme.BgError2, curTheme.FgError2);
            SetErrorStyles((byte)ErrorLevel.Error, curTheme.BgError3, curTheme.FgError3);
            SetErrorStyles((byte)ErrorLevel.Critical, curTheme.BgError4, curTheme.FgError4);
            BgErrorLevelColors = new List<Color> {
                curTheme.BgNoError,
                curTheme.BgError0,
                curTheme.BgError1,
                curTheme.BgError2,
                curTheme.BgError3,
                curTheme.BgError4
            };
            FgErrorLevelColors = new List<Color> {
                curTheme.FgNoError,
                curTheme.FgError0,
                curTheme.FgError1,
                curTheme.FgError2,
                curTheme.FgError3,
                curTheme.FgError4
            };
        }

        /// <summary>
        /// Sets a style for an Error annotation (reduced font + segoe ui) and for markers
        /// </summary>
        /// <param name="errorLevel"></param>
        /// <param name="bgColor"></param>
        /// <param name="fgColor"></param>
        private static void SetErrorStyles(byte errorLevel, Color bgColor, Color fgColor) {
            int curFontSize = Npp.GetStyle(0).Size;

            var normalStyle = Npp.GetStyle(FilesInfo.GetStyleOf((ErrorLevel)errorLevel, ErrorFontWeight.Normal));
            normalStyle.Font = "Segoe ui";
            normalStyle.Size = (int)(curFontSize * 0.9);
            normalStyle.ForeColor = fgColor;
            normalStyle.BackColor = bgColor;

            var boldStyle = Npp.GetStyle(FilesInfo.GetStyleOf((ErrorLevel)errorLevel, ErrorFontWeight.Bold));
            boldStyle.Font = "Segoe ui";
            boldStyle.Size = (int)(curFontSize * 0.9);
            boldStyle.Bold = true;
            boldStyle.ForeColor = fgColor;
            boldStyle.BackColor = bgColor;

            var italicStyle = Npp.GetStyle(FilesInfo.GetStyleOf((ErrorLevel)errorLevel, ErrorFontWeight.Italic));
            italicStyle.Font = "Segoe ui";
            italicStyle.Size = (int)(curFontSize * 0.9);
            italicStyle.Italic = true;
            italicStyle.ForeColor = fgColor;
            italicStyle.BackColor = bgColor;

            var markerStyle = Npp.GetMarker(errorLevel);
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
                var curContext = (UdlStyles) Npp.GetStyleAt(curPos);
                if (curPos <= 0) return true;
                if (IsNormalContext(curContext)) return true;
                var prevContext = (UdlStyles) Npp.GetStyleAt(curPos - 1);
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
                    && context != UdlStyles.Delimiter8);
        }

        /// <summary>
        /// Returns the list of all available themes
        /// </summary>
        /// <returns></returns>
        public static List<StyleTheme> GetThemesList() {
            if (_listOfThemes.Count == 0) {
                if (string.IsNullOrEmpty(ThemeXmlPath) || !File.Exists(ThemeXmlPath)) {
                    Object2Xml<StyleTheme>.LoadFromString(_listOfThemes, DataResources.SyntaxHighlighting, true);
                    if (!string.IsNullOrEmpty(ThemeXmlPath))
                        Object2Xml<StyleTheme>.SaveToFile(_listOfThemes, ThemeXmlPath, true);
                } else
                    Object2Xml<StyleTheme>.LoadFromFile(_listOfThemes, ThemeXmlPath, true);
            }

            if (Config.Instance.SyntaxHighlightThemeId < 0 || Config.Instance.SyntaxHighlightThemeId >= _listOfThemes.Count)
                Config.Instance.SyntaxHighlightThemeId = 0;

            return _listOfThemes;
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
        Delimiter7 = 22, // 
        Delimiter8 = 23, // nested comment
        Idk = 24 // ??
    }

    #endregion

    #region StyleTheme class

    public class StyleTheme {
        public string Name = "Default";
        public int UniqueId = 0;

        public Color BgDefault = ColorTranslator.FromHtml("#FFFFFF");
        public Color FgDefault = ColorTranslator.FromHtml("#4D4D4C");
        public Color FgComment = ColorTranslator.FromHtml("#718C00");
        public Color FgLineComment = ColorTranslator.FromHtml("#EAB700");
        public Color FgNumbers = ColorTranslator.FromHtml("#C82829");

        public Color FgKeyword1 = ColorTranslator.FromHtml("#183E56");
        public Color BgKeyword1 = ColorTranslator.FromHtml("#bbdaff");
        public Color FgKeyword2 = ColorTranslator.FromHtml("#183E56");
        public Color FgKeyword3 = ColorTranslator.FromHtml("#2F5E9B");
        public Color FgKeyword4 = ColorTranslator.FromHtml("#2F5E9B");
        public Color FgKeyword5 = ColorTranslator.FromHtml("#EAB700");
        public Color FgKeyword6 = ColorTranslator.FromHtml("#3E999F");
        public Color FgKeyword7 = ColorTranslator.FromHtml("#C82829");
        public Color FgKeyword8 = ColorTranslator.FromHtml("#000000");
        public Color BgKeyword8 = ColorTranslator.FromHtml("#ADFF2F");

        public Color FgOperators = ColorTranslator.FromHtml("#000000");
        public Color FgFoldInCode1 = ColorTranslator.FromHtml("#183E56");
        public Color FgFoldInCode2 = ColorTranslator.FromHtml("#183E56");
        public Color FgFoldInComment = ColorTranslator.FromHtml("#000000");

        public Color FgDelimiters1 = ColorTranslator.FromHtml("#8959A8");
        public Color FgDelimiters2 = ColorTranslator.FromHtml("#CA81B5");
        public Color FgDelimiters3 = ColorTranslator.FromHtml("#4D4D4C");
        public Color BgDelimiters3 = ColorTranslator.FromHtml("#FFEEAD");
        public Color FgDelimiters4 = ColorTranslator.FromHtml("#8959A8");
        public Color FgDelimiters5 = ColorTranslator.FromHtml("#CA81B5");
        public Color FgDelimiters6 = ColorTranslator.FromHtml("#000000");
        public Color FgDelimiters7 = ColorTranslator.FromHtml("#000000");
        public Color FgDelimiters8 = ColorTranslator.FromHtml("#8E908C");

        public Color FgNoError = ColorTranslator.FromHtml("#007B74");
        public Color BgNoError = ColorTranslator.FromHtml("#C6EFCE");
        public Color FgError0 = ColorTranslator.FromHtml("#3F3F3F");
        public Color BgError0 = ColorTranslator.FromHtml("#F2F2F2");
        public Color FgError1 = ColorTranslator.FromHtml("#9C6500");
        public Color BgError1 = ColorTranslator.FromHtml("#FFEB9C");
        public Color FgError2 = ColorTranslator.FromHtml("#833C0C");
        public Color BgError2 = ColorTranslator.FromHtml("#FFCC99");
        public Color FgError3 = ColorTranslator.FromHtml("#9C0006");
        public Color BgError3 = ColorTranslator.FromHtml("#FFC7CE");
        public Color FgError4 = ColorTranslator.FromHtml("#58267E");
        public Color BgError4 = ColorTranslator.FromHtml("#CC99FF");
    }

    #endregion

}
