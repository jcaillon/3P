#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (ErrorHandler.cs) is part of 3P.
// 
// 3P is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// 3P is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with 3P. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using _3PA.Html;
using _3PA.Lib;
// ReSharper disable LocalizableElement

namespace _3PA.MainFeatures {

    internal static class ErrorHandler {

        /// <summary>
        /// Allows to keep track of the messages already displayed to the user
        /// </summary>
        private static HashSet<string> _catchedErrors = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Shows a Messagebox informing the user that something went wrong with a file,
        /// renames said file with the suffix "_errors"
        /// </summary>
        /// <param name="e"></param>
        /// <param name="message"></param>
        /// <param name="fileName"></param>
        public static void ShowErrors(Exception e, string message, string fileName) {
            Log(e.ToString());
            MessageBox.Show("Attention user! An error has occurred while loading the following file :" + "\n\n"
                + fileName +
                "\n\n" + "The file has been suffixed with '_errors' to avoid further problems.", AssemblyInfo.AssemblyProduct + " error message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (File.Exists(fileName + "_errors"))
                File.Delete(fileName + "_errors");
            File.Move(fileName, fileName + "_errors");
        }

        /// <summary>
        /// Shows an error to the user
        /// </summary>
        /// <param name="e"></param>
        /// <param name="message"></param>
        public static void ShowErrors(Exception e, string message) {
            // log the error into a file
            if (Log(message + "\r\n" + e)) {
                // show it to the user
                if (Config.Instance.UserGetsPreReleases)
                    UserCommunication.Notify("The last action you started has triggered an error and has been cancelled.<br><br>1. If you didn't ask anything from 3P then you can probably ignore this message and go on with your work.<br>2. Otherwise, you might want to check out the error log below :" +
                        (File.Exists(Config.FileErrorLog) ? "<br><a href='" + Config.FileErrorLog + "'>Link to the error log</a>" : "") +
                        "<br>Consider opening an issue on GitHub :<br><a href='" + Config.IssueUrl + "'>" + Config.IssueUrl + "</a>" + "<br><br><b>Level 0 support : restart Notepad++ and see if things are getting better!</b>",
                        MessageImg.MsgPoison, "Unexpected error", message,
                        args => {
                            Npp.Goto(args.Link);
                            args.Handled = true;
                        },
                        0, 500);
                else
                    UserCommunication.Notify("The last action you started has triggered an error and has been cancelled.<br>If you didn't ask anything from 3P then you can probably ignore this message and go on with your work.<br>Otherwise, another try will probably fail as well.<br>Consider restarting Notepad++ as it might solve this problem.<br>Finally, you can use the link below to open an issue on GitHub and thus help programmers debugging 3P :<br><a href='" + Config.IssueUrl + "'>" + Config.IssueUrl + "</a>", MessageImg.MsgPoison, "Unexpected error", message, 0, 500);
            }
        }

        /// <summary>
        /// Log a piece of information
        /// returns false if the error already occured during the session, true otherwise
        /// </summary>
        public static bool Log(string message, bool offlineLogOnly = false) {

            // don't show/store the same error twice in a session
            if (_catchedErrors.Contains(message))
                return false;
            _catchedErrors.Add(message);

            var toAppend = new StringBuilder("***************************\r\n");

            try {
                StackFrame frame = new StackFrame(1);
                var method = frame.GetMethod();
                var callingClass = method.DeclaringType;
                var callingMethod = method.Name;

                toAppend.AppendLine("**" + DateTime.Now.ToString("yy-MM-dd HH:mm:ss") + "**");
                if (method.DeclaringType != null && !method.DeclaringType.Name.Equals("ErrorHandler"))
                    toAppend.AppendLine("*From " + callingClass + "." + callingMethod + "()*");
                toAppend.AppendLine("```");
                toAppend.AppendLine(message);
                toAppend.AppendLine("```\r\n");

                File.AppendAllText(Config.FileErrorLog, toAppend.ToString());
            } catch (Exception) {
                // nothing to do
            }

            if (!offlineLogOnly) {
                try {
                    File.AppendAllText(Config.FileErrorToSend, toAppend.ToString());

                    // send to github
                    Task.Factory.StartNew(() => {
                        if (Config.Instance.GlobalDontAutoPostLog || UserCommunication.SendIssue(File.ReadAllText(Config.FileErrorToSend), Config.SendLogApi)) {
                            Utils.DeleteFile(Config.FileErrorToSend);
                        }
                    });
                } catch (Exception) {
                    // nothing to do
                }
            }

            return true;
        }

        public static void UnhandledErrorHandler(object sender, UnhandledExceptionEventArgs args) {
            ShowErrors((Exception)args.ExceptionObject, "Unhandled error");
        }

        public static void ThreadErrorHandler(object sender, ThreadExceptionEventArgs e) {
            ShowErrors(e.Exception, "Thread error");
        }

        public static void UnobservedErrorHandler(object sender, UnobservedTaskExceptionEventArgs e) {
            ShowErrors(e.Exception, "Unobserved task error");
        }
    }
}
