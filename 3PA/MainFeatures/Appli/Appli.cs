using YamuiFramework.Helper;

namespace _3PA.MainFeatures.Appli {
    class Appli {

        private static AppliForm _form;

        public static void ToggleView() {
            // create the form
            if (_form == null) {
                _form = new AppliForm();
                _form.CurrentForegroundWindow = WinApi.GetForegroundWindow();
                _form.Show(Npp.Win32WindowNpp);
                _form.DoShow();
                return;
            }

            // toggle visibility
            if (_form.Visible)
                _form.Cloack();
            else
                _form.UnCloack();
        }

        public static void ForceClose() {
            _form.ForceClose();
        }
    }
}
