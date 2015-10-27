using System.Drawing;
using YamuiFramework.Forms;
using YamuiFramework.Themes;
using _3PA.Images;

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
            cssStyleSheet = cssStyleSheet.Replace("%FGCOLOR%", ColorTranslator.ToHtml(ThemeManager.Current.LabelsColorsNormalForeColor));
            cssStyleSheet = cssStyleSheet.Replace("%BGCOLOR%", ColorTranslator.ToHtml(ThemeManager.Current.FormColorBackColor));
            cssStyleSheet = cssStyleSheet.Replace("%ACCENTCOLOR%", ColorTranslator.ToHtml(ThemeManager.AccentColor));
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
                    <td class='NotificationTitle'><span class='AccentColor'>3P: </span>" + title + @"</td>
                </tr>
                <tr>
                    <td class='NotificationSubTitle'>" + subtitle + @"</td>
                </tr>
            </table><br>" + content;
        }
    }
}
