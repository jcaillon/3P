#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiFormBaseShadow.cs) is part of YamuiFramework.
// 
// YamuiFramework is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// YamuiFramework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with YamuiFramework. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System;
using System.Windows.Forms;
using YamuiFramework.Helper;

namespace YamuiFramework.Forms {
    
    /// <summary>
    /// Form class that implements interesting utilities + shadow + onpaint + movable borderless
    /// </summary>
    public class YamuiFormBaseShadow : YamuiFormBase {

        #region Constants
        
        // for the shadow
        protected const int CsDropshadow = 0x00020000;

        #endregion
        
        #region private fields

        private bool _mAeroEnabled;

        #endregion
        
        #region CreateParams

        protected override CreateParams CreateParams {
            get {
                var cp = base.CreateParams;

                // activate shadows
                if (Environment.OSVersion.Version.Major >= 6) {
                    var enabled = 0;
                    DwmApi.DwmIsCompositionEnabled(ref enabled);
                    _mAeroEnabled = enabled == 1;
                }
                if (!_mAeroEnabled)
                    cp.ClassStyle |= CsDropshadow;
                
                return cp;
            }
        }

        #endregion
        
        #region WndProc
        
        protected override void WndProc(ref Message m) {
            if (DesignMode) {
                base.WndProc(ref m);
                return;
            }

            base.WndProc(ref m);

            switch (m.Msg) {
                case (int)WinApi.Messages.WM_NCPAINT:
                    // Allow to display the shadows
                    if (_mAeroEnabled) {
                        var v = 2;
                        DwmApi.DwmSetWindowAttribute(Handle, 2, ref v, 4);
                        var margins = new DwmApi.MARGINS(1, 1, 1, 1);
                        DwmApi.DwmExtendFrameIntoClientArea(Handle, ref margins);
                    }
                    break;

            }
        }

        #endregion

    }
}
