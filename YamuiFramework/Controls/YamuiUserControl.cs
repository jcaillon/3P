using System.Windows.Forms;

namespace YamuiFramework.Controls {
    public class YamuiUserControl : UserControl, IYamuiControl {
        public void UpdateBoundsPublic() {
            UpdateBounds();
        }
    }
}