#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (HtmlHelper.cs) is part of 3P.
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

using _3PA.Lib;
using _3PA.MainFeatures.Parser;
using _3PA._Resource;

namespace _3PA.MainFeatures {
    internal static class HtmlHelper {
        /// <summary>
        /// Returns a formatted html content
        /// </summary>
        public static string FormatContent(string content) {
            if (string.IsNullOrEmpty(content))
                return null;
            return @"
                <div style='margin-left: 10px; margin-right: 10px; margin-bottom: 4px;'>
                    " + content + @"
                </div>";
        }

        /// <summary>
        /// Returns a formatted html message with a title, subtitle and icon
        /// </summary>
        public static string FormatTitle(MessageImg image, string title, string subtitle, bool forMessageBox = false) {
            return 
                "<div style=\"background-repeat: no-repeat; background-position: left center; background-image: url('" + image + "'); padding-left: 70px; padding-top: 6px; padding-bottom: 6px;\">" + @"
                    <div class='NotificationTitle'><img src='" + GetLogo + @"' style='padding-right: 10px;'>" + title + @"</div>
                    <div class='NotificationSubTitle'>" + subtitle + @"</div>
                </div>";
        }

        public static string FormatRow(string describe, string result) {
            return "- " + describe + " : <b>" + result + "</b><br>";
        }

        public static string FormatRowWithImg(string image, string text) {
            return "<div class='ToolTipRowWithImg'><img style='padding-right: 2px; padding-left: 5px;' src='" + image + "' height='15px'>" + text + "</div>";
        }

        public static string HtmlFormatRowParam(ParseFlag flags, string text) {
            var image = ParseFlag.Input.ToString();
            if (flags.HasFlag(ParseFlag.InputOutput))
                image = ParseFlag.InputOutput.ToString();
            else if (flags.HasFlag(ParseFlag.Output))
                image = ParseFlag.Output.ToString();
            else if (flags.HasFlag(ParseFlag.Return))
                image = ParseFlag.Return.ToString();
            return "<div class='ToolTipRowWithImg'><img style='padding-right: 2px; padding-left: 5px;' src='" + image + "' height='15px'>" + text + "</div>";
        }

        public static string FormatSubtitle(string text) {
            return "<div class='ToolTipSubTitle'>" + text + "</div>";
        }

        public static string FormatSubString(string text) {
            return "<span class='ToolTipSubString'>" + text + "</span>";
        }

        /// <summary>
        /// Returns the image of the logo (30x30)
        /// </summary>
        /// <returns></returns>
        public static string GetLogo {
            get { return Utils.GetNameOf(() => ImageResources.Logo30x30); }
        }
    }
}