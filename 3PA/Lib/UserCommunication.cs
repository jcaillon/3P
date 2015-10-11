using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YamuiFramework.Forms;
using _3PA.MainFeatures.Appli;
using _3PA.MainFeatures.SynthaxHighlighting;

namespace _3PA.Lib {
    class UserCommunication {
        public static void MessageUser(string text) {
            Notify(text);
        }

        public static void Notify(string html, int duration = 0) {
            Appli.Form.BeginInvoke((Action) delegate {
                var toastNotification = new YamuiNotifications(html, duration);
                toastNotification.Show();
            });
        }
    }
}
