using System.Drawing;
using YamuiFramework.Themes;
using _3PA.Images;

namespace _3PA.Html {

    /// <summary>
    /// Add css and images specific to this application to the yamui html panels/labels/tooltips
    /// </summary>
    class RegisterCssAndImages {

        public static void Init() {
            HtmlHandler.ExtraCssSheet = Properties.Resources.StyleSheet;

            HtmlHandler.ImageNeeded += (sender, args) => {
                Image tryImg = (Image)ImageResources.ResourceManager.GetObject(args.Src);
                if (tryImg == null) return;
                args.Handled = true;
                args.Callback(tryImg);
            };
        }
    }
}
