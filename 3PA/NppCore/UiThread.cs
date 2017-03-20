using System;
using System.Drawing;
using YamuiFramework.Helper;
using _3PA.NppCore.NppInterfaceForm;

namespace _3PA.NppCore {

    internal class UiThread {

        private static NppEmptyForm _anchorForm;
        
        /// <summary>
        /// init an empty form, this gives us a Form to hook onto if we want to do stuff on the UI thread
        /// from a back groundthread, use : BeginInvoke()
        /// </summary>
        public static void Init() {
            _anchorForm = new NppEmptyForm {
                Location = new Point(-10000, -10000)
            };
            _anchorForm.Show(Npp.Win32Handle);
            _anchorForm.Visible = false;
        }

        public static void Close() {
            if (Ready) {
                _anchorForm.Close();
            }
        }

        /// <summary>
        /// Get true if the notifications are ready to be used
        /// </summary>
        public static bool Ready {
            get { return _anchorForm != null && _anchorForm.IsHandleCreated; }
        }

        /// <summary>
        /// BeginInvoke (async) on the UI thread
        /// </summary>
        public static bool BeginInvoke(Action todo) {
            if (Ready) {
                if (_anchorForm.InvokeRequired)
                    _anchorForm.BeginInvoke(todo);
                else
                    todo();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Invoke (sync) on the UI thread
        /// </summary>
        public static bool Invoke(Action todo) {
            if (Ready) {
                if (_anchorForm.InvokeRequired)
                    _anchorForm.Invoke(todo);
                else
                    todo();
                return true;
            }
            return false;
        }


    }
}
