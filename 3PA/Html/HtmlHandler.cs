#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (HtmlHandler.cs) is part of 3P.
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
using System.Drawing;
using YamuiFramework.HtmlRenderer.Core.Core.Entities;
using _3PA.Images;
using _3PA.Properties;

namespace _3PA.Html {

    /// <summary>
    /// Add css and images specific to this application to the yamui html panels/labels/tooltips
    /// </summary>
    internal static class HtmlHandler {

        public static void YamuiThemeManagerOnOnHtmlImageNeeded(object sender, HtmlImageLoadEventArgs args) {
            Image tryImg = (Image)ImageResources.ResourceManager.GetObject(args.Src);
            if (tryImg != null) {
                args.Handled = true;
                args.Callback(tryImg);
            }
        }

        public static string YamuiThemeManagerOnOnGetCssSheet() {
            var cssStyleSheet = Resources.StyleSheet;
            cssStyleSheet = cssStyleSheet.Replace("%FGCOLOR%", ColorTranslator.ToHtml(ThemeManager.Current.FormFore));
            cssStyleSheet = cssStyleSheet.Replace("%BGCOLOR%", ColorTranslator.ToHtml(ThemeManager.Current.FormBack));
            cssStyleSheet = cssStyleSheet.Replace("%ALTERNATEBGCOLOR%", ColorTranslator.ToHtml(ThemeManager.Current.FormAltBack));
            cssStyleSheet = cssStyleSheet.Replace("%ACCENTCOLOR%", ColorTranslator.ToHtml(ThemeManager.Current.AccentColor));
            cssStyleSheet = cssStyleSheet.Replace("%SUBSTRINGCOLOR%", ColorTranslator.ToHtml(ThemeManager.Current.SubTextFore));
            cssStyleSheet = cssStyleSheet.Replace("%LINKCOLOR%", ColorTranslator.ToHtml(ThemeManager.Current.GenericLinkColor));
            cssStyleSheet = cssStyleSheet.Replace("%FORMBORDER%", ColorTranslator.ToHtml(ThemeManager.Current.FormBorder));
            return cssStyleSheet;
        }

        /// <summary>
        /// Returns a formmatted html message with a title, subtitle and icon
        /// </summary>
        /// <param name="content"></param>
        /// <param name="image"></param>
        /// <param name="title"></param>
        /// <param name="subtitle"></param>
        /// <param name="forMessageBox"></param>
        /// <returns></returns>
        public static string FormatMessage(string content, MessageImg image, string title, string subtitle, bool forMessageBox = false) {
            return @"
            <div style='margin-bottom: 1px;'>
                <table style='margin-bottom: " + (forMessageBox ? "15px" : "5px") + @"; width: 100%'>
                    <tr>
                        <td rowspan='2' style='" + (forMessageBox ? "width: 95px; padding-left: 15px" : "width: 80px") + @"'><img src='" + image + @"' width='64' height='64' /></td>
                        <td class='NotificationTitle'><img src='" + GetLogo + @"' style='padding-right: 10px;'>" + title + @"</td>
                    </tr>
                    <tr>
                        <td class='NotificationSubTitle'>" + subtitle + @"</td>
                    </tr>
                </table>
                <div style='margin-left: 8px; margin-right: 8px; margin-top: 0px;'>
                    " + content + @"
                </div>
            </div>";
        }

        /// <summary>
        /// Returns the image of the logo (30x30)
        /// </summary>
        /// <returns></returns>
        public static string GetLogo {
            get { return "logo30x30"; }
        }

    }

    internal enum MessageImg {
        MsgDebug,
        MsgError,
        MsgHighImportance,
        MsgInfo,
        MsgOk,
        MsgPoison,
        MsgQuestion,
        MsgRip,
        MsgToolTip,
        MsgUpdate,
        MsgWarning
    }
}
