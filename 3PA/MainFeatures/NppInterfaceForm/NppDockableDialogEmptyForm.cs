#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (NppDockableDialogEmptyForm.cs) is part of 3P.
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
using System.ComponentModel;
using System.Windows.Forms;
using _3PA.NppCore;

namespace _3PA.MainFeatures.NppInterfaceForm {
    /// <summary>
    /// An empty form that does absolutely nothing
    /// </summary>
    internal partial class NppDockableDialogEmptyForm : Form {
        protected override bool ShowWithoutActivation {
            get { return true; }
        }

        public NppDockableDialogEmptyForm() {
            InitializeComponent();

            // register to Npp
            Npp.RegisterToNpp(Handle);
        }

        protected override void OnClosing(CancelEventArgs e) {
            // register to Npp
            Npp.UnRegisterToNpp(Handle);
            base.OnClosing(e);
        }
    }
}