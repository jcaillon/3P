using System;
using YamuiFramework.Helper;

namespace _3PA.MainFeatures.Appli {
    class Appli {
        public static AppliForm Form;
        private static bool _hasBeenShownOnce;

        public static void ToggleView() {
            // create the form
            if (!_hasBeenShownOnce) {
                _hasBeenShownOnce = true;
                Form.Show(Npp.Win32WindowNpp);
                Form.DoShow();
                return;
            }

            // toggle visibility
            if (Form.Visible && !Form.HasModalOpened)
                Form.Cloack();
            else
                Form.UnCloack();
        }

        public static void Init() {
            if (Form != null) ForceClose();
            Form = new AppliForm {
                CurrentForegroundWindow = Npp.HandleNpp
            };
        }

        /// <summary>
        /// Forces the form to close, only when leaving npp
        /// </summary>
        public static void ForceClose() {
            try {
                Form.ForceClose();
                Form = null;
            } catch (Exception) {
                // ignored
            }
        }
    }
}
