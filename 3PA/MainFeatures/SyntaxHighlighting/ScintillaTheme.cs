#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (ScintillaTheme.cs) is part of 3P.
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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using YamuiFramework.Helper;
using _3PA.Lib;
using _3PA.MainFeatures.Pro;
using _3PA.MainFeatures.Pro.Deploy;
using _3PA.NppCore;
using _3PA._Resource;

namespace _3PA.MainFeatures.SyntaxHighlighting {

    /// <summary>
    /// This class handles the Styles of scintilla
    /// </summary>
    internal class ScintillaTheme : GenericThemeHolder {

        #region Current theme

        private static List<ScintillaTheme> _listOfThemes = new List<ScintillaTheme>();
        private static ScintillaTheme _currentTheme;

        /// <summary>
        /// handles the current theme
        /// </summary>
        public static ScintillaTheme CurrentTheme {
            get {
                if (_currentTheme == null)
                    CurrentTheme = GetThemesList.ElementAt(Config.Instance.SyntaxHighlightThemeId);
                return _currentTheme;
            }
            set {
                _currentTheme = value;
                try {
                    _currentTheme.SetColorValues(typeof(ScintillaTheme));
                } catch (Exception e) {
                    ErrorHandler.ShowErrors(e, "Loading a theme");
                }
            }
        }

        /// <summary>
        /// Returns the list of all available themes
        /// </summary>
        /// <returns></returns>
        public static List<ScintillaTheme> GetThemesList {
            get {
                // get the list of themes from the user's file or from the ressource by default
                if (_listOfThemes.Count == 0)
                    _listOfThemes = ReadThemeFile<ScintillaTheme>(Config.FileSyntaxThemes, DataResources.SyntaxThemes, Encoding.Default);
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

        #region public static

        public static void SetDefaultScintillaStyles() {
            // read npp's stylers.xml file
            Sci.SetIndentGuideColor(Npp.StylersXml.IndentGuideLineBg, Npp.StylersXml.IndentGuideLineFg);
            Sci.SetWhiteSpaceColor(true, Color.Transparent, Npp.StylersXml.WhiteSpaceFg);
            Sci.SetSelectionColor(true, Npp.StylersXml.SelectionBg, Color.Transparent);
            Sci.CaretLineBackColor = Npp.StylersXml.CaretLineBg;
            Sci.CaretColor = Npp.StylersXml.CaretFg;
            Sci.SetFoldMarginColors(true, Npp.StylersXml.FoldMarginBg, Npp.StylersXml.FoldMarginFg);
            Sci.SetFoldMarginMarkersColor(Npp.StylersXml.FoldMarginMarkerFg, Npp.StylersXml.FoldMarginMarkerBg, Npp.StylersXml.FoldMarginMarkerActiveFg);
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

        #region set styles

        /// <summary>
        /// Call this method to set the back/fore color and font type of each type used in 3P according to the 
        /// styles defined in the SyntaxHighlighting file
        /// </summary>
        public void SetScintillaStyles() {
            // Default
            SetFontStyle((byte) SciMsg.STYLE_DEFAULT, GetStyle(SciStyleId.Default));
            SetFontStyle((byte) SciMsg.STYLE_CONTROLCHAR, GetStyle(SciStyleId.Default));

            foreach (var kpv in Items) {
                SetFontStyle((byte) kpv.Key, kpv.Value);
            }

            // line numbers
            SetFontStyle((byte) SciMsg.STYLE_LINENUMBER, GetStyle(SciStyleId.LineNumberMargin));

            // set url as strings
            SetFontStyle(80, GetStyle(SciStyleId.SimpleQuote));

            // brace highlighting
            SetFontStyle((byte) SciMsg.STYLE_BRACELIGHT, GetStyle(SciStyleId.BraceHighLight));
            SetFontStyle((byte) SciMsg.STYLE_BRACEBAD, GetStyle(SciStyleId.BadBraceHighLight));

            // smart highlighting in npp
            Sci.GetIndicator(29).ForeColor = GetStyle(SciStyleId.SmartHighLighting).ForeColor;

            // Setting styles for errors 
            SetErrorStyles((byte) ErrorLevel.Information, GetStyle(SciStyleId.Error0));
            SetErrorStyles((byte) ErrorLevel.Warning, GetStyle(SciStyleId.Error1));
            SetErrorStyles((byte) ErrorLevel.StrongWarning, GetStyle(SciStyleId.Error2));
            SetErrorStyles((byte) ErrorLevel.Error, GetStyle(SciStyleId.Error3));
            SetErrorStyles((byte) ErrorLevel.Critical, GetStyle(SciStyleId.Error4));

            Sci.SetIndentGuideColor(GetStyle(SciStyleId.WhiteSpace).BackColor, GetStyle(SciStyleId.WhiteSpace).ForeColor);
            Sci.SetWhiteSpaceColor(true, Color.Transparent, GetStyle(SciStyleId.WhiteSpace).ForeColor);
            Sci.SetSelectionColor(true, GetStyle(SciStyleId.Selection).BackColor, Color.Transparent);
            Sci.CaretLineBackColor = GetStyle(SciStyleId.CaretLine).BackColor;
            Sci.CaretColor = GetStyle(SciStyleId.CaretColor).ForeColor;

            // Set colors for all folding markers and margin
            Sci.SetFoldMarginColors(true, GetStyle(SciStyleId.FoldMargin).BackColor, GetStyle(SciStyleId.FoldMargin).BackColor);
            Sci.SetFoldMarginMarkersColor(GetStyle(SciStyleId.FoldMargin).ForeColor, GetStyle(SciStyleId.FoldMargin).BackColor, GetStyle(SciStyleId.FoldActiveMarker).ForeColor);
            
            // Configure folding markers with respective symbols
            Sci.GetMarker(Sci.Marker.FolderEnd).Symbol = MarkerSymbol.BoxPlusConnected;
            Sci.GetMarker(Sci.Marker.Folder).Symbol = MarkerSymbol.BoxPlus;
            Sci.GetMarker(Sci.Marker.FolderMidTail).Symbol = MarkerSymbol.TCorner;
            Sci.GetMarker(Sci.Marker.FolderOpenMid).Symbol = MarkerSymbol.BoxMinusConnected;
            Sci.GetMarker(Sci.Marker.FolderSub).Symbol = MarkerSymbol.VLine;
            Sci.GetMarker(Sci.Marker.FolderTail).Symbol = MarkerSymbol.LCorner;
            Sci.GetMarker(Sci.Marker.FolderOpen).Symbol = MarkerSymbol.BoxMinus;
        }

        private void SetFontStyle(byte styleNumber, StyleThemeItem styleItem) {
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
        private void SetErrorStyles(byte errorLevel, StyleThemeItem styleItem) {
            int curFontSize = Sci.GetStyle(0).Size;
            Color bgColor = styleItem.BackColor;
            Color fgColor = styleItem.ForeColor;

            var normalStyle = Sci.GetStyle(OpenedFilesInfo.GetStyleOf((ErrorLevel) errorLevel, ErrorFontWeight.Normal));
            normalStyle.Font = "Segoe ui";
            normalStyle.Size = (int) (curFontSize * 0.9);
            normalStyle.ForeColor = fgColor;
            normalStyle.BackColor = bgColor;

            var boldStyle = Sci.GetStyle(OpenedFilesInfo.GetStyleOf((ErrorLevel) errorLevel, ErrorFontWeight.Bold));
            boldStyle.Font = "Segoe ui";
            boldStyle.Size = (int) (curFontSize * 0.9);
            boldStyle.Bold = true;
            boldStyle.ForeColor = fgColor;
            boldStyle.BackColor = bgColor;

            var italicStyle = Sci.GetStyle(OpenedFilesInfo.GetStyleOf((ErrorLevel) errorLevel, ErrorFontWeight.Italic));
            italicStyle.Font = "Segoe ui";
            italicStyle.Size = (int) (curFontSize * 0.9);
            italicStyle.Italic = true;
            italicStyle.ForeColor = fgColor;
            italicStyle.BackColor = bgColor;

            var markerStyle = Sci.GetMarker(errorLevel);
            markerStyle.Symbol = MarkerSymbol.SmallRect;
            markerStyle.SetBackColor(bgColor);
            markerStyle.SetForeColor(fgColor);
        }

        #endregion


        /// <summary>
        /// Holds the list of all the style items for this style
        /// </summary>
        public Dictionary<SciStyleId, StyleThemeItem> Items { get; private set; }

        public ScintillaTheme() {
            Items = new Dictionary<SciStyleId, StyleThemeItem>();
        }

        /// <summary>
        /// Set the values of this instance, using a dictionary of key -> values, override for this class
        /// </summary>
        public new void SetColorValues(Type thisType) {
            if (SavedStringValues == null)
                return;

            Items.Clear();
            typeof(SciStyleId).ForEach<SciStyleId>((name, enumVal) => {
                if (!SavedStringValues.ContainsKey(name)) {
                    throw new Exception("Styles definition, couldn't find the styles of the field : <" + name + "> for the theme <" + ThemeName + "> : ");
                }
                try {
                    var properties = SavedStringValues[name].Split('\t');
                    if (properties.Length >= 3) {
                        Items.Add(enumVal, new StyleThemeItem {
                            ForeColor = ColorTranslator.FromHtml(GetHtmlColor(properties[0].Trim(), 0)),
                            BackColor = ColorTranslator.FromHtml(GetHtmlColor(properties[1].Trim(), 1)),
                            FontType = Int32.Parse(properties[2].Trim()),
                            FontName = properties.Length >= 4 ? properties[3].Trim() : String.Empty
                        });
                    }
                } catch (Exception e) {
                    throw new Exception("Reading styles, couldn't understand the line : <" + SavedStringValues[name] + "> for the field <" + name + "> and for the theme <" + ThemeName + "> : " + e);
                }
            });
        }

        /// <summary>
        /// Find the html color behind any property
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

        public StyleThemeItem GetStyle(SciStyleId sciStyleId) {
            return Items.ContainsKey(sciStyleId) ? Items[sciStyleId] : null;
        }

        public Color GetErrorBg(int errorLevel) {
            return GetErrorItem(errorLevel).BackColor;
        }

        public Color GetErrorFg(int errorLevel) {
            return GetErrorItem(errorLevel).ForeColor;
        }

        private StyleThemeItem GetErrorItem(int errorLevel) {
            switch (errorLevel) {
                case 0:
                    return Items[SciStyleId.NoError];
                case 1:
                    return Items[SciStyleId.Error0];
                case 2:
                    return Items[SciStyleId.Error1];
                case 3:
                    return Items[SciStyleId.Error2];
                case 4:
                    return Items[SciStyleId.Error3];
                case 5:
                    return Items[SciStyleId.Error4];
            }
            return new StyleThemeItem { BackColor = Color.Beige, ForeColor = Color.Black };
        }

        #region StyleThemeItem

        public class StyleThemeItem {
            public Color BackColor = Color.Transparent;
            public Color ForeColor = Color.Transparent;
            public int FontType;
            public string FontName;
        }

        #endregion
    }

    #region Scintilla style

    /// <summary>
    /// Enumeration of the style id used by our syntax theme in scintilla
    /// </summary>
    public enum SciStyleId {
        Default = (int)SciMsg.STYLE_LASTPREDEFINED,
        Comment,
        SingleLineComment,
        Preprocessor,
        JumpStatement,
        Statement,
        Type,
        Keyword,
        Operator,
        Abbreviation,
        Include,
        DoubleQuote,
        SimpleQuote,
        NormedVariables,
        Number,
        CaretLine,
        Selection,
        WhiteSpace,
        NoError,
        Error0,
        Error1,
        Error2,
        Error3,
        Error4,
        CaretColor,
        LineNumberMargin,
        FoldMargin,
        FoldActiveMarker,
        SmartHighLighting,
        BraceHighLight,
        BadBraceHighLight
    }

    #endregion

}