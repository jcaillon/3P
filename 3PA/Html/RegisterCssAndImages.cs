using System.Drawing;
using YamuiFramework.Themes;
using _3PA.Images;

namespace _3PA.Html {

    /// <summary>
    /// Add css and images specific to this application to the yamui html panels/labels/tooltips
    /// </summary>
    class RegisterCssAndImages {

        public static void Init() {
            var cssStyleSheet = Properties.Resources.StyleSheet;
            cssStyleSheet = cssStyleSheet.Replace("%FGcolor%", ColorTranslator.ToHtml(ThemeManager.Current.LabelsColorsNormalForeColor));
            cssStyleSheet = cssStyleSheet.Replace("%BGcolor%", ColorTranslator.ToHtml(ThemeManager.Current.FormColorBackColor));
            HtmlHandler.ExtraCssSheet = cssStyleSheet;
            HtmlHandler.UpdateBaseCssData();

            HtmlHandler.ImageNeeded += (sender, args) => {
                Image tryImg = (Image)ImageResources.ResourceManager.GetObject(args.Src);
                if (tryImg == null) return;
                args.Handled = true;
                args.Callback(tryImg);
            };
        }
    }
}
