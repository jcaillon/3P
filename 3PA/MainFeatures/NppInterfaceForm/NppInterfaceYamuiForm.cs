#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (NppInterfaceYamuiForm.cs) is part of 3P.
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
using System.Security;
using System.Windows.Forms;
using YamuiFramework.Forms;
using YamuiFramework.Helper;
using _3PA.Interop;

namespace _3PA.MainFeatures.NppInterfaceForm {
    public partial class NppInterfaceYamuiForm : NppInterfaceForm {

        #region fields
        private const int BorderWidth = 1;

        public Action OncloseAction;
        #endregion

        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        public NppInterfaceYamuiForm() {
            InitializeComponent();

            // register to Npp
            FormIntegration.RegisterToNpp(Handle);

            Opacity = 0;
            Visible = false;
            Tag = false;
            KeyPreview = true;
        }

        #endregion


        #region events
        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            switch (StartPosition) {
                case FormStartPosition.CenterParent:
                    CenterToParent();
                    break;
                case FormStartPosition.CenterScreen:
                    if (IsMdiChild) {
                        CenterToParent();
                    } else {
                        CenterToScreen();
                    }
                    break;
            }

            RemoveCloseButton();
            if (ControlBox) {
                var newButton = new YamuiForm.YamuiFormButton {
                    Text = @"r",
                    Size = new Size(25, 20),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    TabStop = false
                };

                //remove the form controls from the tab stop
                newButton.Click += WindowButton_Click;
                Controls.Add(newButton);

                newButton.Location = new Point(ClientRectangle.Width - BorderWidth - 25, BorderWidth);
                Refresh();
            }
        }

        #endregion

        #region Public methods

        #endregion

        #region Helper Methods

        [SecuritySafeCritical]
        public void RemoveCloseButton() {
            var hMenu = WinApi.GetSystemMenu(Handle, false);
            if (hMenu == IntPtr.Zero) return;

            var n = WinApi.GetMenuItemCount(hMenu);
            if (n <= 0) return;

            WinApi.RemoveMenu(hMenu, (uint)(n - 1), WinApi.MfByposition | WinApi.MfRemove);
            WinApi.RemoveMenu(hMenu, (uint)(n - 2), WinApi.MfByposition | WinApi.MfRemove);
            WinApi.DrawMenuBar(Handle);
        }
        #endregion

        #region Window Buttons

        private void WindowButton_Click(object sender, EventArgs e) {
            if (OncloseAction != null)
                OncloseAction();
            else
                Close();
        }

        #endregion

    }
}
