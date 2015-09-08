using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamuiFramework.Helper;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;

namespace _3PA.MainFeatures.ToolTip {
    class InfoToolTip {

        private static InfoToolTipForm _form;

        /// <summary>
        /// Was the form opened because the user left his mouse too long on a word?
        /// </summary>
        private static bool _openedFromDwell;

        public static void ShowToolTip(bool openedFromDwell = false) {
            // remember how this popup was shown
            _openedFromDwell = openedFromDwell;

            // instanciate the form
            if (_form == null) {
                _form = new InfoToolTipForm();
                _form.CurrentForegroundWindow = WinApi.GetForegroundWindow();
                _form.Show(Npp.Win32WindowNpp);
            }

            // update position
            _form.SetPosition();

            _form.SetText("<b>THIS ISSSS</b></br>A simple test :)");

            if (!_form.Visible)
                _form.UnCloack();
        }

        /// <summary>
        /// Closes the form
        /// </summary>
        public static void Close() {
            try {
                _form.Cloack();
                _openedFromDwell = false;
            } catch (Exception) {
                // ignored
            }
        }

        /// <summary>
        /// Forces the form to close, only when leaving npp
        /// </summary>
        public static void ForceClose() {
            try {
                _form.ForceClose();
                _form = null;
            } catch (Exception) {
                // ignored
            }
        }
    }
}
