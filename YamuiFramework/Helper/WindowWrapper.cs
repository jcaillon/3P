using System;
using System.Windows.Forms;

namespace YamuiFramework.Helper {
    public class WindowWrapper : IWin32Window {
        public WindowWrapper(IntPtr handle) {
            _hwnd = handle;
        }

        public IntPtr Handle {
            get { return _hwnd; }
        }

        private IntPtr _hwnd;
    }
}
