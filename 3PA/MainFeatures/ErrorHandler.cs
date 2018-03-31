#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Yamui.Framework.Themes;
using _3PA.Lib;
using _3PA.NppCore;

// ReSharper disable LocalizableElement

namespace _3PA.MainFeatures {
    internal static class ErrorHandler {
        /// <summary>
        /// Allows to keep track of the messages already displayed to the user
        /// </summary>
        private static HashSet<string> _catchedErrors = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

        private static int _nbErrors;

        /// <summary>
        /// Shows a Messagebox informing the user that something went wrong with a file,
        /// renames said file with the suffix "_errors"
        /// </summary>
        public static void ShowErrors(Exception e, string message, string fileName) {
            UserCommunication.Notify("An error has occurred while loading the following file :<br>" + (fileName + "_errors").ToHtmlLink() + "<br><br>The file has been suffixed with '_errors' to avoid further problems.",
                MessageImg.MsgPoison, "File load error", message,
                args => {
                    Npp.Goto(args.Link);
                    args.Handled = true;
                });
            Utils.DeleteFile(fileName + "_errors");
            Utils.MoveFile(fileName, fileName + "_errors");
            ShowErrors(e, message); // show initial error
        }

        /// <summary>
        /// Shows an error to the user
        /// </summary>
        /// <param name="e"></param>
        /// <param name="message"></param>
        public static void ShowErrors(Exception e, string message = null) {
            if (LogError(e, message, false)) {
                // show it to the user
                UserCommunication.Notify("The last action you started has triggered an error and has been canceled.<div class='ToolTipcodeSnippet'>" + e.Message + "</div><br>1. If you didn't ask anything from 3P then you can probably ignore this message.<br>2. Otherwise, you might want to check out the error log below for more details :" + (File.Exists(Config.FileErrorLog) ? "<br>" + Config.FileErrorLog.ToHtmlLink("Link to the error log") : "no .log found!") + "<br>Consider opening an issue on GitHub :<br>" + Config.UrlIssues.ToHtmlLink() + "<br><br>If needed, try to restart Notepad++ and see if things are better!</b>",
                    MessageImg.MsgPoison, "An error has occurred", message,
                    args => {
                        if (args.Link.EndsWith(".log")) {
                            Npp.Goto(args.Link);
                            args.Handled = true;
                        }
                    });
            }
        }

        /// <summary>
        /// Log a piece of information
        /// returns false if the error already occurred during the session, true otherwise
        /// </summary>
        public static bool LogError(Exception e, string message = null, bool showIfDev = true) {
            if (e == null)
                return false;

            if (showIfDev && Config.IsDeveloper) {
                ShowErrors(e, message);
                return true;
            }

            try {
                var info = GetExceptionInfo(e);

                // make sure that this error actually concerns 3P!
                var libNs = typeof(ErrorHandler).Namespace;
                var frameworkNs = typeof(YamuiTheme).Namespace;
                if (string.IsNullOrEmpty(libNs))
                    libNs = "_3PA.";
                else
                    libNs = libNs.Substring(0, libNs.IndexOf(".", StringComparison.Ordinal) + 1);
                if (string.IsNullOrEmpty(frameworkNs))
                    frameworkNs = "YamuiFramework.";
                else
                    frameworkNs = frameworkNs.Substring(0, frameworkNs.IndexOf(".", StringComparison.Ordinal) + 1);

                if (e.Source != null) {
                    if (!info.fullException.ContainsFast(libNs) &&
                        !info.fullException.ContainsFast(frameworkNs))
                        return false;
                }

                // don't show the same error twice in a session
                var excepUniqueId = info.originMethod + info.originLine;
                if (!_catchedErrors.Contains(excepUniqueId))
                    _catchedErrors.Add(excepUniqueId);
                else
                    return false;

                if (message != null)
                    info.message = message + " : " + info.message;

                // write in the log
                var toAppend = new StringBuilder();
                toAppend.AppendLine("============================================================");
                toAppend.AppendLine("WHAT : " + info.message);
                toAppend.AppendLine("WHEN : " + DateTime.Now.ToString(CultureInfo.CurrentCulture));
                toAppend.AppendLine("WHERE : " + info.originMethod + ", line " + info.originLine);
                toAppend.AppendLine("DETAILS : ");
                foreach (var line in info.fullException.Split('\n')) {
                    toAppend.AppendLine("    " + line.Trim());
                }
                toAppend.AppendLine("");
                toAppend.AppendLine("");
                Utils.FileAppendAllText(Config.FileErrorLog, toAppend.ToString());

                // send the report
                if (!Config.IsDeveloper)
                    SendBugReport(info);
            } catch (Exception x) {
                if (Config.IsDeveloper)
                    ShowErrors(x, message);
            }

            return true;
        }

        /// <summary>
        /// Sends the given report to the web service of 3P
        /// </summary>
        private static void SendBugReport(ExceptionInfo bugReport) {
            var wb = new WebServiceJson(WebServiceJson.WebRequestMethod.Post, Config.PostBugsWebWervice);
            wb.OnInitHttpWebRequest += request => {
                request.Proxy = Config.Instance.GetWebClientProxy();
                request.UserAgent = Config.GitHubUserAgent;
            };
            wb.Serialize(bugReport);

            // save the request in a file
            var fileName = Path.Combine(Config.FolderLog, "unreported_" + DateTime.Now.ToString("yy.MM.dd_HH-mm-ss_") + _nbErrors++ + ".json");
            Utils.FileWriteAllText(fileName, wb.JsonRequest.ToString());

            wb.OnRequestEnded += webServ => {
                // request ok -> delete the json
                if (webServ.StatusCodeResponse == HttpStatusCode.OK)
                    Utils.DeleteFile(fileName);
            };
            wb.Execute();
        }

        /// <summary>
        /// Returns info on an exception 
        /// </summary>
        private static ExceptionInfo GetExceptionInfo(Exception e) {
            ExceptionInfo output = null;
            var frame = new StackTrace(e, true).GetFrame(0);
            if (frame != null) {
                var method = frame.GetMethod();
                output = new ExceptionInfo {
                    originMethod = (method != null ? (method.DeclaringType != null ? method.DeclaringType.ToString() : "?") + "." + method.Name : "?") + "()",
                    originLine = frame.GetFileLineNumber(),
                    originVersion = AssemblyInfo.Version,
                    UUID = User.UniqueId,
                    message = e.Message,
                    fullException = e.ToString()
                };
            }
            if (output == null)
                output = new ExceptionInfo {
                    originMethod = Utils.CalculateMd5Hash(e.Message),
                    originVersion = AssemblyInfo.Version,
                    UUID = User.UniqueId,
                    message = e.Message,
                    fullException = e.ToString()
                };
            return output;
        }

        #region global error handler callbacks

        public static void UnhandledErrorHandler(object sender, UnhandledExceptionEventArgs e) {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
                ShowErrors(ex, "Error not handled");
        }

        public static void ThreadErrorHandler(object sender, ThreadExceptionEventArgs e) {
            ShowErrors(e.Exception, "Thread error");
        }

        public static void UnobservedErrorHandler(object sender, UnobservedTaskExceptionEventArgs e) {
            ShowErrors(e.Exception, "Unobserved task error");
        }

        #endregion
    }

    #region ExceptionInfo

    /// <summary>
    /// Corresponds to the 3P webservice
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class ExceptionInfo {
        public string originVersion { get; set; }
        public string originMethod { get; set; }
        public int originLine { get; set; }
        public string receptionTime { get; set; }
        public string nbReceived { get; set; }
        public string UUID { get; set; }
        public string message { get; set; }
        public string fullException { get; set; }
    }

    #endregion
}