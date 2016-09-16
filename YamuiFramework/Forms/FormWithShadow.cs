using System;
using System.Windows.Forms;
using YamuiFramework.Helper;

namespace YamuiFramework.Forms {

    public class FormWithShadow : Form {

        #region Shadows

        private bool _mAeroEnabled; // variables for box shadow
        private const int CsDropshadow = 0x00020000;
        private const int WmNcpaint = 0x0085;

        protected override CreateParams CreateParams {
            get {
                _mAeroEnabled = CheckAeroEnabled();
                var cp = base.CreateParams;
                if (!_mAeroEnabled)
                    cp.ClassStyle |= CsDropshadow;
                return cp;
            }
        }

        private bool CheckAeroEnabled() {
            if (Environment.OSVersion.Version.Major >= 6) {
                var enabled = 0;
                DwmApi.DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1);
            }
            return false;
        }

        protected override void WndProc(ref Message m) {
            if (!DesignMode) {
                switch (m.Msg) {
                    case WmNcpaint: // box shadow
                        if (_mAeroEnabled) {
                            var v = 2;
                            DwmApi.DwmSetWindowAttribute(Handle, 2, ref v, 4);
                            var margins = new DwmApi.MARGINS(1, 1, 1, 1);
                            DwmApi.DwmExtendFrameIntoClientArea(Handle, ref margins);
                        }
                        break;
                }
            }

            base.WndProc(ref m);
        }

        #endregion
    }
}
