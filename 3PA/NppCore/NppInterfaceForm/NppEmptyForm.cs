#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (NppEmptyForm.cs) is part of 3P.
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
using System.Drawing;
using System.Windows.Forms;
using Yamui.Framework.Helper;

namespace _3PA.NppCore.NppInterfaceForm {
    /// <summary>
    /// An empty form that does absolutely nothing
    /// </summary>
    internal class NppEmptyForm : Form {
        #region ShowWithoutActivation & Don't show in ATL+TAB

        /// <summary>
        /// This indicates that the form should not take focus when shown
        /// specify it through the CreateParams
        /// </summary>
        protected override bool ShowWithoutActivation {
            get { return true; }
        }

        /// <summary>
        /// Don't show in ATL+TAB
        /// </summary>
        protected override CreateParams CreateParams {
            get {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= (int) WinApi.WindowStylesEx.WS_EX_TOOLWINDOW;
                return createParams;
            }
        }

        #endregion

        public NppEmptyForm() {
            Visible = false;
            ControlBox = false;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            ClientSize = new Size(1, 1);
        }
    }
}