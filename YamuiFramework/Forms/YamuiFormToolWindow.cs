#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiFormToolWindow.cs) is part of YamuiFramework.
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
using System.Windows.Forms;
using YamuiFramework.Helper;

namespace YamuiFramework.Forms {
    /// <summary>
    /// Form class that does not take focus on show
    /// </summary>
    public class YamuiFormToolWindow : YamuiFormButtons {
        #region ShowWithoutActivation & Don't show in ATL+TAB

        /// <summary>
        /// This indicates that the form should not take focus when shown
        /// However, if you specify TopMost = true, then this doesn't work anymore, hence why we
        /// specify it through the CreateParams
        /// </summary>
        protected override bool ShowWithoutActivation {
            get { return true; }
        }

        protected override CreateParams CreateParams {
            get {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= (int) WinApi.WindowStylesEx.WS_EX_TOPMOST;
                createParams.ExStyle |= (int) WinApi.WindowStylesEx.WS_EX_TOOLWINDOW;
                return createParams;
            }
        }

        #endregion
    }
}