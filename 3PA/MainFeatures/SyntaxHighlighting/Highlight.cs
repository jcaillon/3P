#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (Highlight.cs) is part of 3P.
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
using YamuiFramework.Forms;
using _3PA.Data;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures.FilesInfo;

namespace _3PA.MainFeatures.SyntaxHighlighting {

    /// <summary>
    /// This class handles the STYLENEEDED notification of scintilla
    /// </summary>
    public class Highlight {

        #region fields

        /// <summary>
        /// You can set this property to read the theme.xml file from a local path instead of
        /// the embedded ressource file
        /// </summary>
        public static string ThemeXmlPath;

        /// <summary>
        /// List of themes
        /// </summary>
        private static List<HighlightTheme> _listOfThemes = new List<HighlightTheme>();

        private static HighlightTheme _currentTheme;

        #endregion

        #region Current theme

        /// <summary>
        /// handles the current theme
        /// </summary>
        public static HighlightTheme CurrentTheme {
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

        #region init

        /// <summary>
        /// The role of this method is to make sure that the User Defined Language for "OpenEdgeABL" exists in the
        /// userDefineLang.xml file, if it does it updates it, if it doesn't exists it creates it and asks the user
        /// to restart Notepad++
        /// </summary>
        public static void Init() {
            var udlFilePath = Path.Combine(Npp.GetConfigDir(), @"../../../userDefineLang.xml");

            if (!File.Exists(udlFilePath)) {
                File.WriteAllText(udlFilePath, @"<NotepadPlus />", Encoding.Default);
            }

            var fileContent = File.ReadAllText(udlFilePath, Encoding.Default);

            var regex = new Regex("<UserLang name=\"OpenEdgeABL\".*?</UserLang>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var matches = regex.Match(fileContent);
            if (matches.Success) {
                // if it already exists in the file, delete the existing one
                fileContent = regex.Replace(fileContent, @"");
            } else {
                // if it doesn't exist in the file
                UserCommunication.Notify("It seems to be the first time that you use this plugin.<br>In order to activate the syntax highlighting, you must restart notepad++.<br><br><i>Please note that if a document is opened at the next start, you will have to manually close/reopen it to see the changes.</i><br><br><b>Sorry for the inconvenience</b>!", MessageImage.Info, "Information", "Installing syntax highlighting");
            }
            if (fileContent.ContainsFast(@"<NotepadPlus />"))
                fileContent = fileContent.Replace(@"<NotepadPlus />", "<NotepadPlus>\r\n" + DataResources.UDL + "\r\n</NotepadPlus>");
            else
                fileContent = fileContent.Replace(@"<NotepadPlus>", "<NotepadPlus>\r\n" + DataResources.UDL);
            File.WriteAllText(udlFilePath, fileContent, Encoding.Default);
        }

        #endregion

        #region public methods

        public static void SetCustomStyles() {
            var curTheme = CurrentTheme;

            //Npp.SetDefaultStyle(curTheme.BgDefault, curTheme.FgDefault);
            Npp.SetStyle((byte)UdlStyles.Default, curTheme.BgDefault, curTheme.FgDefault);
            Npp.SetStyle((byte)UdlStyles.Comment, curTheme.BgDefault, curTheme.FgComment);
            Npp.SetStyle((byte)UdlStyles.CommentLine, curTheme.BgDefault, curTheme.FgLineComment);
            Npp.SetStyle((byte)UdlStyles.Number, curTheme.BgDefault, curTheme.FgNumbers);
            Npp.SetStyle((byte)UdlStyles.KeyWordsList1, curTheme.BgKeyword1, curTheme.FgKeyword1);
            Npp.SetStyle((byte)UdlStyles.KeyWordsList2, curTheme.BgDefault, curTheme.FgKeyword2);
            Npp.SetStyle((byte)UdlStyles.KeyWordsList3, curTheme.BgDefault, curTheme.FgKeyword3);
            Npp.SetStyle((byte)UdlStyles.KeyWordsList4, curTheme.BgDefault, curTheme.FgKeyword4);
            Npp.SetStyle((byte)UdlStyles.KeyWordsList5, curTheme.BgDefault, curTheme.FgKeyword5);
            Npp.SetStyle((byte)UdlStyles.KeyWordsList6, curTheme.BgDefault, curTheme.FgKeyword6);
            Npp.SetStyle((byte)UdlStyles.KeyWordsList7, curTheme.BgDefault, curTheme.FgKeyword7);
            Npp.SetStyle((byte)UdlStyles.KeyWordsList8, curTheme.BgKeyword8, curTheme.FgKeyword8);
            Npp.SetStyle((byte)UdlStyles.FolderInCode2, curTheme.BgDefault, curTheme.FgFoldInCode2);
            Npp.SetStyle((byte)UdlStyles.Delimiter1, curTheme.BgDefault, curTheme.FgDelimiters1);
            Npp.SetStyle((byte)UdlStyles.Delimiter2, curTheme.BgDefault, curTheme.FgDelimiters2);
            Npp.SetStyle((byte)UdlStyles.Delimiter3, curTheme.BgDelimiters3, curTheme.FgDelimiters3);
            Npp.SetStyle((byte)UdlStyles.Delimiter4, curTheme.BgDefault, curTheme.FgDelimiters4);
            Npp.SetStyle((byte)UdlStyles.Delimiter5, curTheme.BgDefault, curTheme.FgDelimiters5);
            Npp.SetStyle((byte)UdlStyles.Delimiter6, curTheme.BgDefault, curTheme.FgDelimiters6);
            Npp.SetStyle((byte)UdlStyles.Delimiter7, curTheme.BgDefault, curTheme.FgDelimiters7);
            Npp.SetStyle((byte)UdlStyles.Delimiter8, curTheme.BgDefault, curTheme.FgDelimiters8);
            Npp.SetStyle((byte)UdlStyles.Operators, curTheme.BgDefault, curTheme.FgOperators);

            // for annotations :
            SetAnnotationStyleDefinition((byte)ErrorLevel.Information, curTheme.BgAnnotation0, curTheme.FgAnnotation0);
            SetAnnotationStyleDefinition((byte)ErrorLevel.Warning, curTheme.BgAnnotation1, curTheme.FgAnnotation1);
            SetAnnotationStyleDefinition((byte)ErrorLevel.StrongWarning, curTheme.BgAnnotation2, curTheme.FgAnnotation2);
            SetAnnotationStyleDefinition((byte)ErrorLevel.Error, curTheme.BgAnnotation3, curTheme.FgAnnotation3);
            SetAnnotationStyleDefinition((byte)ErrorLevel.Critical, curTheme.BgAnnotation4, curTheme.FgAnnotation4);

            // for markers :
            Npp.SetMarkerStyle((byte)ErrorLevel.Information, curTheme.BgAnnotation0, curTheme.FgAnnotation0, SciMarkerStyle.SC_MARK_SMALLRECT);
            Npp.SetMarkerStyle((byte)ErrorLevel.Warning, curTheme.BgAnnotation1, curTheme.FgAnnotation1, SciMarkerStyle.SC_MARK_SMALLRECT);
            Npp.SetMarkerStyle((byte)ErrorLevel.StrongWarning, curTheme.BgAnnotation2, curTheme.FgAnnotation2, SciMarkerStyle.SC_MARK_SMALLRECT);
            Npp.SetMarkerStyle((byte)ErrorLevel.Error, curTheme.BgAnnotation3, curTheme.FgAnnotation3, SciMarkerStyle.SC_MARK_SMALLRECT);
            Npp.SetMarkerStyle((byte)ErrorLevel.Critical, curTheme.BgAnnotation4, curTheme.FgAnnotation4, SciMarkerStyle.SC_MARK_SMALLRECT);

            // set style 33 for the margin with line numbers
        }

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
                    && context != UdlStyles.CommentLine
                    && context != UdlStyles.Delimiter8);
        }

        /// <summary>
        /// Returns the list of all available themes
        /// </summary>
        /// <returns></returns>
        public static List<HighlightTheme> GetThemesList() {
            if (_listOfThemes.Count == 0) {
                if (string.IsNullOrEmpty(ThemeXmlPath) || !File.Exists(ThemeXmlPath)) {
                    Object2Xml<HighlightTheme>.LoadFromString(_listOfThemes, DataResources.SyntaxHighlighting, true);
                    if (!string.IsNullOrEmpty(ThemeXmlPath))
                        Object2Xml<HighlightTheme>.SaveToFile(_listOfThemes, ThemeXmlPath, true);
                } else
                    Object2Xml<HighlightTheme>.LoadFromFile(_listOfThemes, ThemeXmlPath, true);
            }

            if (Config.Instance.SyntaxHighlightThemeId < 0 || Config.Instance.SyntaxHighlightThemeId >= _listOfThemes.Count)
                Config.Instance.SyntaxHighlightThemeId = 0;

            return _listOfThemes;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Sets a style for an annotation (reduced font + segoe ui)
        /// </summary>
        /// <param name="style"></param>
        /// <param name="bgColor"></param>
        /// <param name="fgColor"></param>
        private static void SetAnnotationStyleDefinition(byte style, Color bgColor, Color fgColor) {
            int curFontSize = Npp.GetFontSize(0);

            Npp.SetStyleFont((byte)(style + FilesInfo.FilesInfo.ErrorAnnotStandardStyleOffset), "Segoe ui", (int)(curFontSize * 0.9));
            Npp.SetStyle((byte)(style + FilesInfo.FilesInfo.ErrorAnnotStandardStyleOffset), bgColor, fgColor);

            Npp.SetStyleFont((byte)(style + FilesInfo.FilesInfo.ErrorAnnotBoldStyleOffset), "Segoe ui", (int)(curFontSize * 0.9));
            Npp.SetStyle((byte)(style + FilesInfo.FilesInfo.ErrorAnnotBoldStyleOffset), bgColor, fgColor);
            Npp.SetStyleFontBold((byte)(style + FilesInfo.FilesInfo.ErrorAnnotBoldStyleOffset), true);

            Npp.SetStyleFont((byte)(style + FilesInfo.FilesInfo.ErrorAnnotItalicStyleOffset), "Segoe ui", (int)(curFontSize * 0.9));
            Npp.SetStyle((byte)(style + FilesInfo.FilesInfo.ErrorAnnotItalicStyleOffset), bgColor, fgColor);
            Npp.SetStyleFontItalic((byte)(style + FilesInfo.FilesInfo.ErrorAnnotItalicStyleOffset), true);
        }

        #endregion

        #region real colorization todo

        // the class doesn't correctly handle text that is not encoded on 8 bits because we just do pos++, need to fix this 
        // GetByteCount();
        /*
         * Encoding.UTF8.GetByteCount(text);
         * 
         * byte[] bytes = Encoding.Default.GetBytes(myString);
            myString = Encoding.UTF8.GetString(bytes);
         * 
         * Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            Encoding utf8 = Encoding.UTF8;
            byte[] utfBytes = utf8.GetBytes(Message);
            byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            string msg = iso.GetString(isoBytes);
         * 
         * File.ReadAllText(file, Encoding.GetEncoding(codePage));
         */
        /*
        /// <summary>
        /// Called on STYLENEEDED notification
        /// </summary>
        /// <param name="endPos"></param>
        public static void Colorize(int startPos, int endPos) {
            //------------
            var watch = Stopwatch.StartNew();
            //------------

            // redefine the styles
            SetCustomStyles();

            Lexer tok = new Lexer(Npp.GetDocumentText());
            tok.Tokenize();
            SynthaxHighlightVisitor vis = new SynthaxHighlightVisitor {
                FromLine = Npp.LineFromPosition(startPos),
                ToLine = Npp.LineFromPosition(endPos)
            };
            tok.Accept(vis);

            //--------------
            watch.Stop();
            Npp.SetStatusbarLabel("derp = " + derp + "startPos = " + startPos + ", endPos = " + endPos + ", done in " + watch.ElapsedMilliseconds + " ms");
            //------------
            derp++;
        }
        */

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
        KeyWordsList1 = 4, /* Jump in code (RUN, RETURN, LEAVE...) */
        KeyWordsList2 = 5, /* statements (DEFINE VARIABLE..) */
        KeyWordsList3 = 6, /* VAR types (BLOB, DATE, INTEGER) */
        KeyWordsList4 = 7, /* all other keywords */
        KeyWordsList5 = 8, /* preprocessed words (&if &global...) */
        KeyWordsList6 = 9, /* variables prefix (gc_, li_...) */
        KeyWordsList7 = 10, /* abbreviations */
        KeyWordsList8 = 11, /* user trigram */
        Operators = 12, /* also includes (matches, not...) */
        FolderInCode1 = 13,
        FolderInCode2 = 14, /* Collapsable blocks, (FOR EACH: END.) */
        FolderInComment = 15,
        Delimiter1 = 16, /* string double quote */
        Delimiter2 = 17, /* string simple */
        Delimiter3 = 18, /* include { } */
        Delimiter4 = 19, /* string double quote in single line comment (preproc definition) */
        Delimiter5 = 20, /* string simple quote in single line comment (preproc definition) */
        Delimiter6 = 21, /*  */
        Delimiter7 = 22, /*  */
        Delimiter8 = 23, /* nested comment */
        Idk = 24, /* ?? */
    }

    #endregion

    #region Theme class

    public class HighlightTheme {
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

        public Color FgAnnotation0 = ColorTranslator.FromHtml("#3F3F3F");
        public Color BgAnnotation0 = ColorTranslator.FromHtml("#F2F2F2");
        public Color FgAnnotation1 = ColorTranslator.FromHtml("#9C6500");
        public Color BgAnnotation1 = ColorTranslator.FromHtml("#FFEB9C");
        public Color FgAnnotation2 = ColorTranslator.FromHtml("#833C0C");
        public Color BgAnnotation2 = ColorTranslator.FromHtml("#FFCC99");
        public Color FgAnnotation3 = ColorTranslator.FromHtml("#9C0006");
        public Color BgAnnotation3 = ColorTranslator.FromHtml("#FFC7CE");
        public Color FgAnnotation4 = ColorTranslator.FromHtml("#58267E");
        public Color BgAnnotation4 = ColorTranslator.FromHtml("#CC99FF");
    }

    #endregion

}
