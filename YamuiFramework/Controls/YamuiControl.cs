using System.Windows.Forms;

namespace YamuiFramework.Controls {
    public class YamuiControl : Control, IYamuiControl {
        public void UpdateBoundsPublic() {
            UpdateBounds();
        }
    }
}