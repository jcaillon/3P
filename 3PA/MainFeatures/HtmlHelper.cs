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
            return @"
            <table style='width: 100%'>
                <tr>
                    <td rowspan='2' style='width: 70px'><img src='" + image + @"' width='64' height='64' /></td>
                    <td class='NotificationTitle'><img src='" + GetLogo + @"' style='padding-right: 10px;'>" + title + @"</td>
                </tr>
                <tr>
                    <td class='NotificationSubTitle'>" + subtitle + @"</td>
                </tr>
            </table>";
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
