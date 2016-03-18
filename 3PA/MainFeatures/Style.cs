#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Windows.Forms;
using _3PA.Data;
using _3PA.Html;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures.FilesInfoNs;

namespace _3PA.MainFeatures {

    /// <summary>
    /// This class handles the STYLENEEDED notification of scintilla
    /// </summary>
    internal static class Style {

        #region fields

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

        /// <summary>
        /// Returns the list of all available themes
        /// </summary>
        /// <returns></returns>
        public static List<StyleTheme> GetThemesList() {
            if (_listOfThemes.Count == 0) {
                StyleTheme curTheme = null;
                ConfLoader.ForEachLine(Config.FileSyntaxThemes, DataResources.SyntaxHighlighting, Encoding.Default, s => {
                    // beggining of a new theme, read its name
                    if (s.Length > 2 && s[0] == '>') {
                        _listOfThemes.Add(new StyleTheme());
                        curTheme = _listOfThemes.Last();
                        curTheme.Name = s.Substring(2).Trim();
                    }
                    if (curTheme == null)
                        return;
                    // fill the theme
                    var items = s.Split('\t');
                    if (items.Count() == 4) {
                        curTheme.SetValueOf(items[0], new StyleThemeItem {
                            ForeColor = ColorTranslator.FromHtml(items[1].Trim()),
                            BackColor = ColorTranslator.FromHtml(items[2].Trim()),
                            FontType = int.Parse(items[3].Trim())
                        });
                    }
                });
            }

            if (Config.Instance.SyntaxHighlightThemeId < 0 || Config.Instance.SyntaxHighlightThemeId >= _listOfThemes.Count)
                Config.Instance.SyntaxHighlightThemeId = 0;

            return _listOfThemes;
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
            var fileContent = File.Exists(Config.FileUdl) ? File.ReadAllText(Config.FileUdl, Encoding.Default) : @"<NotepadPlus />";
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
                File.WriteAllText(Config.FileUdl, fileContent, Encoding.Default);
            } catch (Exception e) {
                if (e is UnauthorizedAccessException)
                    UserCommunication.Notify("<b>Couldn't access the file :</b><br>" + Config.FileUdl + "<br><br>This means i couldn't correctly applied the syntax highlighting feature!<br><br><i>Please make sure to allow write access to this file (Right click on file > Security > Check what's needed to allow total control to current user)</i>", MessageImg.MsgError, "Syntax highlighting", "Can't access userDefineLang.xml");
                else
                    ErrorHandler.ShowErrors(e, "Error while accessing userDefineLang.xml");
                return false;
            }
            return true;
        }

        #endregion

        #region set styles

        private static bool _needReset;
        private static StyleThemeItem _defIndentGuide = new StyleThemeItem();
        private static StyleThemeItem _defCaretLine = new StyleThemeItem();

        /// <summary>
        /// Call this method to reset the styles to their default value when moving to a non progress file
        /// </summary>
        public static void ResetSyntaxStyles() {

            if (!_needReset)
                return;

            Npp.SetWhiteSpaceColor(false, Color.Transparent, Color.Transparent);
            SetFontStyle((byte)SciMsg.STYLE_INDENTGUIDE, _defIndentGuide);
            Npp.SetSelectionColor(true, _defCaretLine.BackColor, Color.Transparent);
            Npp.SetAdditionalSelectionColor(true, _defCaretLine.BackColor, Color.Transparent);
            //Npp.SelectionBackAlpha = 120;
            Npp.CaretLineBackColor = ControlPaint.Light(_defCaretLine.BackColor, 1.2f);
            //Npp.CaretLineBackAlpha = 60;

            Npp.StyleResetDefault();
            
        }

        /// <summary>
        /// Call this method to set the back/fore color and font type of each type used in 3P according to the 
        /// styles defined in the SyntaxHighlighting file
        /// </summary>
        public static void SetSyntaxStyles() {

            // save current values, to reset them when we switch on a non progress file
            if (!_needReset) {
                var nppStyle = Npp.GetStyle((byte)SciMsg.STYLE_INDENTGUIDE);
                _defIndentGuide.BackColor = nppStyle.BackColor;
                _defIndentGuide.ForeColor = nppStyle.ForeColor;
                _defCaretLine.BackColor = Npp.CaretLineBackColor;
                _needReset = true;
            }

            var curTheme = CurrentTheme;

            // Default
            SetFontStyle((byte)SciMsg.STYLE_DEFAULT, curTheme.Default);
            SetFontStyle((byte)SciMsg.STYLE_CONTROLCHAR, curTheme.Default);
            SetFontStyle((byte)UdlStyles.Idk, curTheme.Default);
            SetFontStyle((byte)UdlStyles.Default, curTheme.Default);
            Npp.SetWhiteSpaceColor(true, curTheme.WhiteSpace.BackColor, curTheme.WhiteSpace.ForeColor);
            SetFontStyle((byte)SciMsg.STYLE_INDENTGUIDE, curTheme.WhiteSpace);

            // categories
            SetFontStyle((byte)UdlStyles.Comment, curTheme.Comment);
            SetFontStyle((byte)UdlStyles.CommentLine, curTheme.PreProcessed);
            SetFontStyle((byte)UdlStyles.Number, curTheme.Numbers);
            SetFontStyle((byte)UdlStyles.KeyWordsList1, curTheme.JumpStatement);
            SetFontStyle((byte)UdlStyles.KeyWordsList2, curTheme.Statement);
            SetFontStyle((byte)UdlStyles.KeyWordsList3, curTheme.VarType);
            SetFontStyle((byte)UdlStyles.KeyWordsList4, curTheme.OtherKeywords);
            SetFontStyle((byte)UdlStyles.KeyWordsList5, curTheme.PreProcessed);
            SetFontStyle((byte)UdlStyles.KeyWordsList6, curTheme.NormedVariables);
            SetFontStyle((byte)UdlStyles.KeyWordsList7, curTheme.Abbreviations);
            SetFontStyle((byte)UdlStyles.KeyWordsList8, curTheme.SpecialWord);
            SetFontStyle((byte)UdlStyles.Operators, curTheme.Operators);
            SetFontStyle((byte)UdlStyles.FolderInCode2, curTheme.Statement);
            SetFontStyle((byte)UdlStyles.Delimiter1, curTheme.DoubleQuote);
            SetFontStyle((byte)UdlStyles.Delimiter2, curTheme.SimpleQuote);
            SetFontStyle((byte)UdlStyles.Delimiter3, curTheme.Includes);
            SetFontStyle((byte)UdlStyles.Delimiter4, curTheme.DoubleQuote);
            SetFontStyle((byte)UdlStyles.Delimiter5, curTheme.SimpleQuote);
            SetFontStyle((byte)UdlStyles.Delimiter7, curTheme.SingleLineComment);
            SetFontStyle((byte)UdlStyles.Delimiter8, curTheme.NestedComment);

            // Extra
            Npp.SetSelectionColor(true, curTheme.Selection.BackColor, Color.Transparent);
            Npp.SetAdditionalSelectionColor(true, curTheme.Selection.BackColor, Color.Transparent);
            //Npp.SelectionBackAlpha = 120;
            Npp.CaretLineBackColor = curTheme.CaretLine.BackColor;
            //Npp.CaretLineBackAlpha = 70;
            //Npp.CaretLineVisible = true;

        }

        private static void SetFontStyle(byte styleNumber, StyleThemeItem styleItem) {
            var nppStyle = Npp.GetStyle(styleNumber);
            nppStyle.BackColor = styleItem.BackColor;
            nppStyle.ForeColor = styleItem.ForeColor;
            nppStyle.Bold = styleItem.FontType.IsBitSet(1);
            nppStyle.Italic = styleItem.FontType.IsBitSet(2);
        }

        public static List<Color> BgErrorLevelColors;
        public static List<Color> FgErrorLevelColors;

        public static void SetGeneralStyles() {

            var curTheme = CurrentTheme;
            
            // Setting styles for errors 
            SetErrorStyles((byte)ErrorLevel.Information, curTheme.Error0.BackColor, curTheme.Error0.ForeColor);
            SetErrorStyles((byte)ErrorLevel.Warning, curTheme.Error1.BackColor, curTheme.Error1.ForeColor);
            SetErrorStyles((byte)ErrorLevel.StrongWarning, curTheme.Error2.BackColor, curTheme.Error2.ForeColor);
            SetErrorStyles((byte)ErrorLevel.Error, curTheme.Error3.BackColor, curTheme.Error3.ForeColor);
            SetErrorStyles((byte)ErrorLevel.Critical, curTheme.Error4.BackColor, curTheme.Error4.ForeColor);

            BgErrorLevelColors = new List<Color> {
                curTheme.NoError.BackColor,
                curTheme.Error0.BackColor,
                curTheme.Error1.BackColor,
                curTheme.Error2.BackColor,
                curTheme.Error3.BackColor,
                curTheme.Error4.BackColor
            };
            FgErrorLevelColors = new List<Color> {
                curTheme.NoError.ForeColor,
                curTheme.Error0.ForeColor,
                curTheme.Error1.ForeColor,
                curTheme.Error2.ForeColor,
                curTheme.Error3.ForeColor,
                curTheme.Error4.ForeColor
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

    public class StyleTheme {

        public string Name = "Default";
        public StyleThemeItem Default;
        public StyleThemeItem Comment;
        public StyleThemeItem NestedComment;
        public StyleThemeItem SingleLineComment;
        public StyleThemeItem PreProcessed;
        public StyleThemeItem JumpStatement;
        public StyleThemeItem Statement;
        public StyleThemeItem VarType;
        public StyleThemeItem OtherKeywords;
        public StyleThemeItem Operators;
        public StyleThemeItem Abbreviations;
        public StyleThemeItem Includes;
        public StyleThemeItem DoubleQuote;
        public StyleThemeItem SimpleQuote;
        public StyleThemeItem NormedVariables;
        public StyleThemeItem SpecialWord;
        public StyleThemeItem CaretLine;
        public StyleThemeItem Selection;
        public StyleThemeItem WhiteSpace;
        public StyleThemeItem Numbers;
        public StyleThemeItem NoError;
        public StyleThemeItem Error0;
        public StyleThemeItem Error1;
        public StyleThemeItem Error2;
        public StyleThemeItem Error3;
        public StyleThemeItem Error4;

        /// <summary>
        /// Set a value to this instance, by its property name
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetValueOf(string propertyName, object value) {
            var property = typeof(StyleTheme).GetFields().FirstOrDefault(info => info.Name.Equals(propertyName));
            if (property == null) {
                return false;
            }
            property.SetValue(this, value);
            return true;
        }
    }

    public class StyleThemeItem {
        public Color BackColor = Color.Azure;
        public Color ForeColor = Color.Black;
        public int FontType;
    }

    #endregion

}
