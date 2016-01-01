#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
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

namespace _3PA.MainFeatures {
    public class ErrorHandler {

        private static string PathLogFolder { get { return Path.Combine(Npp.GetConfigDir(), "Log"); } }
        private static string PathErrorfile { get { return Path.Combine(PathLogFolder, "error.log"); } }
        public static string PathErrorToSend { get { return Path.Combine(PathLogFolder, "error_.log"); } }
        private static string PathDirtyErrorsfile { get { return Path.Combine(PathLogFolder, "dirty_errors.log"); } }

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
            MessageBox.Show(@"Attention user! An error has occurred while loading in the following file :" + "\n\n"
                + fileName +
                "\n\n" + @"The file has been suffixed with '_errors' to avoid further problems.", AssemblyInfo.ProductTitle + " error message", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            var errorToStr = e.ToString();

            // don't show/store the same error twice in a session
            if (_catchedErrors.Contains(errorToStr))
                return;
            _catchedErrors.Add(errorToStr);

            // log the error into a file
            if (Log(message + "\r\n" + e)) {
                Task.Factory.StartNew(() => {
                    try {
                        if (Config.Instance.LogError) {
                            if (!Config.Instance.GlobalDontAutoPostLog && UserCommunication.SendIssue(File.ReadAllText(PathErrorToSend), Config.SendLogUrl)) {
                                if (File.Exists(PathErrorToSend))
                                    File.Delete(PathErrorToSend);
                            }
                        }
                    } catch (Exception exception) {
                        Log(exception.ToString());
                    }
                });
            }

            try {
                // show it to the user, conditionally
                if (Config.Instance.UserGetsPreReleases)
                    UserCommunication.Notify("The last action you started has triggered an error and has been cancelled.<br><br>1. If you didn't ask anything from 3P then you can probably ignore this message and go on with your work.<br>2. Otherwise, you might want to check out the error log below :" +
                        (File.Exists(PathErrorfile) ? "<br><a href='" + PathErrorfile + "'>Link to the error log</a>" : "") +
                        "<br>Consider opening an issue on GitHub :<br><a href='https://github.com/jcaillon/3P/issues'>https://github.com/jcaillon/3P/issues</a>" + "<br><br><b>Level 0 support : restart Notepad++ and see if things are getting better!</b>",
                        MessageImg.MsgPoison, "An error has occurred", message,
                        args => {
                            Npp.Goto(args.Link);
                            args.Handled = true;
                        },
                        0, 500);
                else
                    UserCommunication.Notify("The last action you started has triggered an error and has been cancelled.<br>If you didn't ask anything from 3P then you can probably ignore this message and go on with your work.<br>Otherwise, another try will probably fail as well.<br>Consider restarting Notepad++ as it might solve this problem.<br>Finally, you can use the link below to open an issue on GitHub and thus help programmers debugging 3P :<br><a href='https://github.com/jcaillon/3P/issues'>https://github.com/jcaillon/3P/issues</a>",
                        MessageImg.MsgPoison, "An error has occurred", message,
                        args => {
                            Npp.Goto(args.Link);
                            args.Handled = true;
                        },
                        0, 500);
            } catch (Exception x) {
                DirtyLog(x);
                // display the error message the old way
                MessageBox.Show("An unidentified error has occurred, probably while loading the plugin.\n\nThere is a hugh probability that it will cause the plugin to not operate normally.\n\nTry to restart Notepad++, consider opening an issue on : https://github.com/jcaillon/3P/issues if the problem persists.", AssemblyInfo.ProductTitle + " error message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Log a piece of information
        /// </summary>
        /// <param name="message"></param>
        public static bool Log(string message) {
            bool success = true;
            var toAppend = new StringBuilder("***************************\r\n");

            try {
                StackFrame frame = new StackFrame(1);
                var method = frame.GetMethod();
                var callingClass = method.DeclaringType;
                var callingMethod = method.Name;

                if (!Directory.Exists(PathLogFolder))
                    Directory.CreateDirectory(PathLogFolder);

                toAppend.AppendLine("**" + DateTime.UtcNow.ToString("yy-MM-dd HH:mm:ss.fff zzz") + "**");
                if (method.DeclaringType != null && !method.DeclaringType.Name.Equals("ErrorHandler"))
                    toAppend.AppendLine("*From " + callingClass + "." + callingMethod + "()*");
                toAppend.AppendLine("```");
                toAppend.AppendLine(message);
                toAppend.AppendLine("```\r\n");

                File.AppendAllText(PathErrorfile, toAppend.ToString());
            } catch (Exception x) {
                DirtyLog(x);
                success = false;
            }

            try {
                File.AppendAllText(PathErrorToSend, toAppend.ToString());
            } catch (Exception) {
                // hm it's ok..
            }

            return success;
        }

        /// <summary>
        /// Log a piece of information
        /// </summary>
        public static void DirtyLog(Exception e) {
            if (File.Exists(PathDirtyErrorsfile)) {
                FileInfo f = new FileInfo(PathDirtyErrorsfile);
                if (f.Length > 10000000)
                    return;
            }
            var toAppend = new StringBuilder("***************************\r\n");
            try {
                StackFrame frame = new StackFrame(1);
                var method = frame.GetMethod();
                var callingClass = method.DeclaringType;
                var callingMethod = method.Name;

                if (!Directory.Exists(PathLogFolder))
                    Directory.CreateDirectory(PathLogFolder);

                toAppend.AppendLine("**" + DateTime.UtcNow.ToString("yy-MM-dd HH:mm:ss.fff zzz") + "**");
                if (method.DeclaringType != null)
                    toAppend.AppendLine("*From " + callingClass + "." + callingMethod + "()*");
                toAppend.AppendLine("```");
                toAppend.AppendLine(e.ToString());
                toAppend.AppendLine("```\r\n");

                File.AppendAllText(PathDirtyErrorsfile, toAppend.ToString());
            } catch (Exception) {
                // ok
            }
        }

        public static void UnhandledErrorHandler(object sender, UnhandledExceptionEventArgs args) {
            ShowErrors((Exception)args.ExceptionObject, "Unhandled error!");
        }

        public static void ThreadErrorHandler(object sender, ThreadExceptionEventArgs e) {
            ShowErrors(e.Exception, "Thread error!");
        }

        public static void UnobservedErrorHandler(object sender, UnobservedTaskExceptionEventArgs e) {
            ShowErrors(e.Exception, "Unobserved task error!");
        }
    }
}
