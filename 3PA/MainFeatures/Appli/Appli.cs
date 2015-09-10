using System;
using YamuiFramework.Helper;

namespace _3PA.MainFeatures.Appli {
    class Appli {

        private static AppliForm _form;

        public static void ToggleView() {
            // create the form
            if (_form == null) {
                _form = new AppliForm();
                _form.CurrentForegroundWindow = Npp.HandleNpp;
                _form.Show(Npp.Win32WindowNpp);
                _form.DoShow();
                return;
            }

            // toggle visibility
            if (_form.Visible && !_form.HasModalOpened)
                _form.Cloack();
            else
                _form.UnCloack();
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
