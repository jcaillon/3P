#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (LocalHtmlHandler.cs) is part of 3P.
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
using YamuiFramework.Forms;
using YamuiFramework.Themes;
using _3PA.Images;
using _3PA.Lib;

namespace _3PA.Html {

    /// <summary>
    /// Add css and images specific to this application to the yamui html panels/labels/tooltips
    /// </summary>
    class LocalHtmlHandler {

        /// <summary>
        /// Call this when initializing the plugin feed the html renderer
        /// </summary>
        public static void Init() {
            ProvideCssSheet();

            HtmlHandler.ImageNeeded += (sender, args) => {
                Image tryImg = (Image)ImageResources.ResourceManager.GetObject(args.Src);
                if (tryImg == null) return;
                args.Handled = true;
                args.Callback(tryImg);
            };
        }

        /// <summary>
        /// Updates the colors of the css sheet and feeds it to the html renderer
        /// </summary>
        public static void ProvideCssSheet() {
            var cssStyleSheet = Properties.Resources.StyleSheet;
            cssStyleSheet = cssStyleSheet.Replace("%FGCOLOR%", ColorTranslator.ToHtml(ThemeManager.Current.AutoCompletionNormalForeColor));
            cssStyleSheet = cssStyleSheet.Replace("%BGCOLOR%", ColorTranslator.ToHtml(ThemeManager.Current.AutoCompletionNormalBackColor));
            cssStyleSheet = cssStyleSheet.Replace("%ALTERNATEBGCOLOR%", ColorTranslator.ToHtml(ThemeManager.Current.AutoCompletionNormalAlternateBackColor));
            cssStyleSheet = cssStyleSheet.Replace("%ACCENTCOLOR%", ColorTranslator.ToHtml(ThemeManager.AccentColor));
            cssStyleSheet = cssStyleSheet.Replace("%SUBSTRINGCOLOR%", ColorTranslator.ToHtml(ThemeManager.Current.AutoCompletionNormalSubTypeForeColor));
            cssStyleSheet = cssStyleSheet.Replace("%LINKCOLOR%", ColorTranslator.ToHtml(ThemeManager.Current.GenericLinkColor));
            HtmlHandler.ExtraCssSheet = cssStyleSheet;
            HtmlHandler.UpdateBaseCssData();
        }

        /// <summary>
        /// Returns a formmatted html message with a title, subtitle and icon
        /// </summary>
        /// <param name="content"></param>
        /// <param name="image"></param>
        /// <param name="title"></param>
        /// <param name="subtitle"></param>
        /// <returns></returns>
        public static string FormatMessage(string content, MessageImage image, string title, string subtitle) {
            return @"
            <table style='margin-bottom: 7px;>
                <tr>
                    <td rowspan='2'><img style='padding-right: 10px;' src='" + image + @"' width='64' height='64' /></td>
                    <td class='NotificationTitle'><span class='AccentColor'>" + AssemblyInfo.ProductTitle + @" : </span>" + title + @"</td>
                </tr>
                <tr>
                    <td class='NotificationSubTitle'>" + subtitle + @"</td>
                </tr>
            </table><br>" + content;
        }
    }
}
