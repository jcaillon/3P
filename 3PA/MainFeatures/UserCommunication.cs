#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (UserCommunication.cs) is part of 3P.
// 
// // 3P is a free software: you can redistribute it and/or modify
// // it under the terms of the GNU General Public License as published by
// // the Free Software Foundation, either version 3 of the License, or
// // (at your option) any later version.
// 
// // 3P is distributed in the hope that it will be useful,
// // but WITHOUT ANY WARRANTY; without even the implied warranty of
// // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// // GNU General Public License for more details.
// 
// // You should have received a copy of the GNU General Public License
// // along with 3P. If not, see <http://www.gnu.org/licenses/>.
// // ========================================================================
#endregion
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using YamuiFramework.Forms;
using _3PA.Html;

namespace _3PA.MainFeatures {
    public class UserCommunication {

        /// <summary>
        /// Displays a notification on the bottom right of the screen
        /// </summary>
        /// <param name="html"></param>
        /// <param name="duration"></param>
        /// <param name="width"></param>
        public static void Notify(string html, int duration = 0, int width = 300) {
            Notify(html, MessageImage.Ant, "Debug message", "=)", duration, width);
        }

        public static void Notify(string html, MessageImage imageType, string title, string subTitle = "", int duration = 0, int width = 300) {
            if (Appli.Appli.Form != null)
                Appli.Appli.Form.BeginInvoke((Action)delegate {
                    var toastNotification = new YamuiNotifications(
                        LocalHtmlHandler.FormatMessage(html, imageType, title.ToUpper(), subTitle)
                        , duration, width);
                    toastNotification.Show();
                });
        }

        public static void NotifyUserAboutNppDefaultAutoComp() {
            
        }

        public static void MessageToUser() {
            Appli.Appli.Form.BeginInvoke((Action)delegate {
                YamuiFormMessageBox.ShwDlg(Npp.HandleNpp, MessageImage.Error, "Erreur", @"Wtf did you do you fool!?<br>This is a new line with <b>BOLD</b> stuff<br><br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction. <br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<a href='efzefzef'>test a link</a>", new List<string> { "fu", "ok" }, true, (o, args) => {
                    MessageBox.Show(args.Link);
                    var x = (YamuiFormMessageBox)o; x.Close();
                }, true);
            });
        }
    }
}
