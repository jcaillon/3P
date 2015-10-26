using System;
using System.Collections.Generic;
using System.Windows.Forms;
using YamuiFramework.Forms;

namespace _3PA.MainFeatures {
    public class UserCommunication {
        public static void MessageUser(string text) {
            Notify(text);
        }

        /// <summary>
        /// Displays a notification on the bottom right of the screen
        /// </summary>
        /// <param name="html"></param>
        /// <param name="duration"></param>
        /// <param name="width"></param>
        public static void Notify(string html, int duration = 0, int width = 300) {
            Appli.Appli.Form.BeginInvoke((Action) delegate {
                var toastNotification = new YamuiNotifications(html, duration, width);
                toastNotification.Show();
            });
        }

        public static void NotifyUserAboutNppDefaultAutoComp() {
            
        }

        public static void MessageToUser() {
            Appli.Appli.Form.BeginInvoke((Action)delegate {
                YamuiFormMessageBox.ShwDlg(Npp.HandleNpp, MsgType.Error, "Erreur", @"Wtf did you do you fool!?<br>This is a new line with <b>BOLD</b> stuff<br><br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction. <br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<a href='efzefzef'>test a link</a>", new List<string> { "fu", "ok" }, true, (o, args) => {
                    MessageBox.Show(args.Link);
                    var x = (YamuiFormMessageBox)o; x.Close();
                }, true);
            });
        }
    }
}
