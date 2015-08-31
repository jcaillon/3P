using System.Drawing;

namespace YamuiFramework.Resources {
    class ImageGetter {

        private static ImageGetter _instance;

        public static ImageGetter GetInstance() {
            return _instance ?? (_instance = new ImageGetter());
        }

        public Image Get(string filename) {
            return (Image)Resources.ResourceManager.GetObject(filename);
        }
    }
}
