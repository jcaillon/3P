#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (UiThread.cs) is part of 3P.
// 
// 3P is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// 3P is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with 3P. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System;
using System.Drawing;
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