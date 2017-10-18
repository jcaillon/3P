#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (NppStylers.cs) is part of 3P.
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
using _3PA.Lib;
using _3PA.MainFeatures;

namespace _3PA.NppCore {

    internal static partial class Npp {

        private static NppStylers _nppStylers;

        /// <summary>
        /// Get instance
        /// </summary>
        public static NppStylers StylersXml {
            get {
                if (_nppStylers == null || Utils.HasFileChanged(ConfXml.FileNppConfigXml)) {
                    _nppStylers = new NppStylers();
                }
                return _nppStylers;
            }
        }

        /// <summary>
        /// Class that holds some properties extracted from the stylers.xml file
        /// </summary>
        internal class NppStylers {

            #region Fields

            private static bool _warnedAboutFailStylers;

            #endregion

            #region Properties

            public Color WhiteSpaceFg { get; set; }
            public Color IndentGuideLineBg { get; set; }
            public Color IndentGuideLineFg { get; set; }
            public Color SelectionBg { get; set; }
            public Color CaretLineBg { get; set; }
            public Color CaretFg { get; set; }
            public Color FoldMarginBg { get; set; }
            public Color FoldMarginFg { get; set; }
            public Color FoldMarginMarkerFg { get; set; }
            public Color FoldMarginMarkerActiveFg { get; set; }
            public Color FoldMarginMarkerBg { get; set; }

            #endregion

            #region Life and death

            public NppStylers() {
                Reload();
            }

            #endregion

            #region Public

            public void Reload() {
                // read npp's stylers.xml file
                try {
                    var widgetStyle = new NanoXmlDocument(Utils.ReadAllText(ConfXml.FileNppStylersXml)).RootNode["GlobalStyles"].SubNodes;
                    WhiteSpaceFg = GetColorInStylers(widgetStyle, "White space symbol", "fgColor");
                    IndentGuideLineBg = GetColorInStylers(widgetStyle, "Indent guideline style", "bgColor");
                    IndentGuideLineFg = GetColorInStylers(widgetStyle, "Indent guideline style", "fgColor");
                    SelectionBg = GetColorInStylers(widgetStyle, "Selected text colour", "bgColor");
                    CaretLineBg = GetColorInStylers(widgetStyle, "Current line background colour", "bgColor");
                    CaretFg = GetColorInStylers(widgetStyle, "Caret colour", "fgColor");
                    FoldMarginBg = GetColorInStylers(widgetStyle, "Fold margin", "bgColor");
                    FoldMarginFg = GetColorInStylers(widgetStyle, "Fold margin", "fgColor");
                    FoldMarginMarkerFg = GetColorInStylers(widgetStyle, "Fold", "fgColor");
                    FoldMarginMarkerBg = GetColorInStylers(widgetStyle, "Fold", "bgColor");
                    FoldMarginMarkerActiveFg = GetColorInStylers(widgetStyle, "Fold active", "fgColor");
                } catch (Exception e) {
                    ErrorHandler.LogError(e, "Error parsing " + ConfXml.FileNppStylersXml);
                    if (!_warnedAboutFailStylers) {
                        _warnedAboutFailStylers = true;
                        UserCommunication.Notify("Error while reading one of Notepad++ file :<div>" + ConfXml.FileNppStylersXml.ToHtmlLink() + "</div><br>The xml isn't correctly formatted, Npp manages to read anyway but you should correct it.", MessageImg.MsgError, "Error reading stylers.xml", "Xml read error");
                    }
                }
            }

            #endregion

            #region Private

            private static Color GetColorInStylers(List<NanoXmlNode> widgetStyle, string attributeName, string attributeToGet) {
                try {
                    return ColorTranslator.FromHtml("#" + widgetStyle.First(x => x.GetAttribute("name").Value.EqualsCi(attributeName)).GetAttribute(attributeToGet).Value);
                } catch (Exception) {
                    return Color.Transparent;
                }
            }

            #endregion
        }

    }
}
