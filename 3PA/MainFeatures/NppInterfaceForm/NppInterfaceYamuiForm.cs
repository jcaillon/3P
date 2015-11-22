using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Security;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Forms;
using YamuiFramework.Helper;
using YamuiFramework.Themes;
using _3PA.Interop;
using _3PA.Lib;

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
