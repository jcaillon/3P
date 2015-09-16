using System;
using _3PA.Lib;

namespace _3PA.MainFeatures.InfoToolTip {
    class InfoToolTip {

        private static InfoToolTipForm _form;

        /// <summary>
        /// Was the form opened because the user left his mouse too long on a word?
        /// </summary>
        private static bool _openedFromDwell;

        public static void ShowToolTip(bool openedFromDwell = false) {
            if (Config.Instance.ToolTipDeactivate) return;

            // remember if the popup was opened because of the dwell time
            _openedFromDwell = openedFromDwell;

            // instanciate the form
            if (_form == null) {
                _form = new InfoToolTipForm {
                    UnfocusedOpacity = Config.Instance.ToolTipUnfocusedOpacity
                };
                _form.Show(Npp.Win32WindowNpp);
            }

            // update position
            var position = Npp.GetPositionFromMouseLocation();
            var point = Npp.GetPointXyFromPosition(position);
            point.Offset(Npp.GetWindowRect().Location);
            var lineHeight = Npp.GetTextHeight(Npp.GetCaretLineNumber());
            point.Y += lineHeight + 5;
            _form.SetPosition(point, lineHeight + 5);

            _form.SetText("<div class='InfoToolTip'><b>THIS ISSSS</b><br>A simple test :)<br><img src='wink'>hey<br>" + Npp.GetWordAtPosition(position) + "</div>");

            if (!_form.Visible)
                _form.UnCloack();
        }

        /// <summary>
        /// Closes the form
        /// </summary>
        public static void Close(bool calledFromDwellEnd = false) {
            try {
                if (calledFromDwellEnd && !_openedFromDwell) return;
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
