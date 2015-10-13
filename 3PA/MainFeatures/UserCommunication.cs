using System;
using YamuiFramework.Forms;

namespace _3PA.MainFeatures {
    public class UserCommunication {
        public static void MessageUser(string text) {
            Notify(text);
        }

        public static void Notify(string html, int duration = 0) {
            Appli.Appli.Form.BeginInvoke((Action) delegate {
                var toastNotification = new YamuiNotifications(html, duration);
                toastNotification.Show();
            });
        }

        public static void NotifyUserAboutNppDefaultAutoComp() {
            
        }
    }
}
