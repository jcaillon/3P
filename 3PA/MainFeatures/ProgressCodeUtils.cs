#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProgressCodeUtils.cs) is part of 3P.
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
using System.Linq;
using System.Text;
using _3PA.Html;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.FilesInfoNs;
using _3PA.MainFeatures.ProgressExecutionNs;

namespace _3PA.MainFeatures {
    class ProgressCodeUtils {

        #region Go to definition

        /// <summary>
        /// This method allows the user to GOTO a word definition, if a tooltip is opened then it tries to 
        /// go to the definition of the displayed word, otherwise it tries to find the declaration of the parsed word under the
        /// caret. At last, it tries to find a file in the propath
        /// </summary>
        public static void GoToDefinition() {
            try {
                // if a tooltip is opened, try to execute the "go to definition" of the tooltip first
                if (InfoToolTip.InfoToolTip.IsVisible) {
                    if (!string.IsNullOrEmpty(InfoToolTip.InfoToolTip.GoToDefinitionFile)) {
                        Npp.Goto(InfoToolTip.InfoToolTip.GoToDefinitionFile, InfoToolTip.InfoToolTip.GoToDefinitionPoint.X, InfoToolTip.InfoToolTip.GoToDefinitionPoint.Y);
                        InfoToolTip.InfoToolTip.Close();
                        return;
                    }
                    InfoToolTip.InfoToolTip.Close();
                }

                // try to go to the definition of the selected word
                var position = Npp.CurrentPosition;
                var curWord = Npp.GetWordAtPosition(position);


                // match a word in the autocompletion? go to definition
                var data = AutoComplete.FindInCompletionData(curWord, position);
                if (data != null && data.Count > 0) {
                    foreach (var completionData in data) {
                        if (completionData.FromParser) {
                            Npp.Goto(completionData.ParsedItem.FilePath, completionData.ParsedItem.Line, completionData.ParsedItem.Column);
                            return;
                        }
                    }
                }

                // last resort, try to find a matching file in the propath
                // first look in the propath
                var fullPaths = ProgressEnv.FindLocalFiles(curWord, Config.Instance.GlobalProgressExtension);
                if (fullPaths.Count > 0) {
                    if (fullPaths.Count > 1) {
                        var output = new StringBuilder(@"Found several files matching this name, please choose the correct one and i will open it for you :<br>");
                        foreach (var fullPath in fullPaths) {
                            output.Append("<br><a class='ToolGotoDefinition' href='" + fullPath + "'>" + fullPath + "</a>");
                        }
                        UserCommunication.Notify(output.ToString(), MessageImg.MsgQuestion, "Question", "Open a file", args => {
                            Npp.Goto(args.Link);
                            args.Handled = true;
                        }, 0, 500);
                    } else
                        Npp.Goto(fullPaths[0]);
                    return;
                }

                UserCommunication.Notify("Sorry pal, couldn't go to the definition of <b>" + curWord + "</b>", MessageImg.MsgInfo, "information", "Failed to find an origin", 5);
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in GoToDefinition");
            }
        }

        #endregion

        #region Toggle comment

        /// <summary>
        /// Toggle comments on and off on selected lines
        /// </summary>
        public static void ToggleComment() {
            try {
                int mode = 0; // 0: null, 1: toggle off; 2: toggle on
                var startLine = Npp.LineFromPosition(Npp.SelectionStart);
                var endLine = Npp.LineFromPosition(Npp.SelectionEnd);

                Npp.BeginUndoAction();

                // for each line in the selection
                for (int iLine = startLine; iLine <= endLine; iLine++) {
                    var thisLine = new Npp.Line(iLine);
                    var startPos = thisLine.IndentationPosition;
                    var endPos = thisLine.EndPosition;

                    // the line is essentially empty
                    if ((endPos - startPos) == 0) {
                        // only one line selected, 
                        if (startLine == endLine) {
                            Npp.SetTextByRange(startPos, startPos, "/*  */");
                            Npp.SetSel(startPos + 3);
                        }
                        continue;
                    }

                    // line is surrounded by /* */
                    if (Npp.GetTextOnRightOfPos(startPos, 2).Equals("/*") && Npp.GetTextOnLeftOfPos(endPos, 2).Equals("*/")) {
                        if (mode == 0) mode = 1; // toggle off

                        // add /* */ ?
                        if (mode == 2 && (endPos - 4 - startPos) > 0) {

                            Npp.SetTextByRange(endPos, endPos, "*/");
                            Npp.SetTextByRange(startPos, startPos, "/*");
                        } else {
                            // delete /* */
                            Npp.SetTextByRange(endPos - 2, endPos, string.Empty);
                            Npp.SetTextByRange(startPos, startPos + 2, string.Empty);
                        }

                    } else {
                        if (mode == 0) mode = 2; // toggle on

                        // there are no /* */ but we need to put some
                        if (mode == 2) {
                            Npp.SetTextByRange(endPos, endPos, "*/");
                            Npp.SetTextByRange(startPos, startPos, "/*");
                        }
                    }
                }

                // correct selection...
                if (mode == 2) {
                    if (Npp.SelectionEnd - Npp.SelectionStart > 0) {
                        if (Npp.AnchorPosition > Npp.CurrentPosition)
                            Npp.SetSel(Npp.SelectionEnd + 2, Npp.SelectionStart);
                        else
                            Npp.SetSel(Npp.SelectionStart, Npp.SelectionEnd + 2);
                    }
                }

                Npp.EndUndoAction();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error when commenting");
            }
        }

        #endregion

        #region Open help

        /// <summary>
        /// Opens the lgrfeng.chm file if it can find it in the config
        /// </summary>
        public static void Open4GlHelp() {
            // get path
            if (string.IsNullOrEmpty(Config.Instance.GlobalHelpFilePath)) {
                if (File.Exists(ProgressEnv.Current.ProwinPath)) {
                    // Try to find the help file from the prowin32.exe location
                    var helpPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(ProgressEnv.Current.ProwinPath) ?? "", "..", "prohelp", "lgrfeng.chm"));
                    if (File.Exists(helpPath)) {
                        Config.Instance.GlobalHelpFilePath = helpPath;
                        UserCommunication.Notify("I've found an help file here :<br><a href='" + helpPath + "'>" + helpPath + "</a><br>If you think this is incorrect, you can change the help file path in the settings", MessageImg.MsgInfo, "Opening 4GL help", "Found help file", 10);
                    }
                }
            }

            if (string.IsNullOrEmpty(Config.Instance.GlobalHelpFilePath) || !File.Exists(Config.Instance.GlobalHelpFilePath) || !Path.GetExtension(Config.Instance.GlobalHelpFilePath).EqualsCi(".chm")) {
                UserCommunication.Notify("Could not access the help file, please be sure to provide a valid path the the file <b>lgrfeng.chm</b> in the settings window", MessageImg.MsgInfo, "Opening help file", "File not found", 10);
                return;
            }

            Process.Start(new ProcessStartInfo {
                FileName = "hh.exe",
                Arguments = ("ms-its:" + Config.Instance.GlobalHelpFilePath).ProgressQuoter(), // append ::/folder/page.htm
                UseShellExecute = true
            });
        }

        #endregion

        #region Compilation, Check syntax, Run

        /// <summary>
        /// Check current file syntax
        /// </summary>
        public static void CheckSyntaxCurrent() {
            try { StartProgressExec(ExecutionType.CheckSyntax); } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in CheckSyntaxCurrent");
            }
        }

        /// <summary>
        /// Compile the current file
        /// </summary>
        public static void CompileCurrent() {
            try { StartProgressExec(ExecutionType.Compile); } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in CompileCurrent");
            }
        }

        /// <summary>
        /// Run the current file
        /// </summary>
        public static void RunCurrent() {
            try { StartProgressExec(ExecutionType.Run); } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in RunCurrent");
            }
        }

        /// <summary>
        /// Called after the compilation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="processOnExitEventArgs"></param>
        public static void OnCompileEnded(object sender, ProcessOnExitEventArgs processOnExitEventArgs) {
            try {
                var lastExec = processOnExitEventArgs.ProgressExecution;

                CurrentOperation currentOperation;
                if (!Enum.TryParse(lastExec.ExecutionType.ToString(), true, out currentOperation))
                    currentOperation = CurrentOperation.Run;

                // Clear flag or we can't do any other actions on this file
                FilesInfo.GetFileInfo(lastExec.FullFilePathToExecute).CurrentOperation &= ~currentOperation;

                var isCurrentFile = lastExec.FullFilePathToExecute.EqualsCi(Plug.CurrentFilePath);

                if (isCurrentFile)
                    FilesInfo.DisplayCurrentFileInfo();

                // if log not found then something is messed up!
                if (string.IsNullOrEmpty(lastExec.LogPath) ||
                    !File.Exists(lastExec.LogPath)) {
                    UserCommunication.Notify("Something went terribly wrong while " + ((CurrentOperationAttr)currentOperation.GetAttributes()).ActionText + " the following file:<div><a href='" + lastExec.FullFilePathToExecute + "'>" + lastExec.FullFilePathToExecute + "</a></div><br><div>Below is the <b>command line</b> that was executed:</div><div class='ToolTipcodeSnippet'>" + lastExec.ProgressWin32 + " " + lastExec.ExeParameters + "</div><b>Execution directory :</b><br><a href='" + lastExec.ExecutionDir + "'>" + lastExec.ExecutionDir + "</a><br><br><i>Did you messed up the prowin32.exe command line parameters in your config?<br>Is it possible that i don't have the rights to write in your %temp% directory?</i>", MessageImg.MsgError, "Critical error", "Action failed");
                    return;
                }

                int nbWarnings = 0;
                int nbErrors = 0;

                // Read log info
                // correct file path...
                var changePaths = new Dictionary<string, string> {
                    { lastExec.TempFullFilePathToExecute, lastExec.FullFilePathToExecute }
                };
                var errorList = FilesInfo.ReadErrorsFromFile(lastExec.LogPath, false, changePaths);

                if (!errorList.Any()) {
                    // the compiler messages are empty
                    var fileInfo = new FileInfo(lastExec.LogPath);
                    if (fileInfo.Length > 0) {
                        // the .log is not empty, maybe something went wrong in the runner, display errors
                        UserCommunication.Notify(
                            "Something went wrong while " + ((CurrentOperationAttr)currentOperation.GetAttributes()).ActionText + " the following file:<br><br><a href='" +
                            lastExec.FullFilePathToExecute + "'>" +
                            lastExec.FullFilePathToExecute +
                            "</a><br><br>The progress compiler didn't return any errors but the log isn't empty, here is the content :" +
                            Utils.ReadAndFormatLogToHtml(lastExec.LogPath), MessageImg.MsgError,
                            "Critical error", "Action failed");
                        return;
                    }
                } else {
                    // count number of warnings/errors
                    // loop through files
                    foreach (var keyValue in errorList) {
                        // loop through errors in said file
                        foreach (var fileError in keyValue.Value) {
                            if (fileError.Level == ErrorLevel.Warning) nbWarnings++;
                            else nbErrors++;
                        }
                    }
                }

                // Prepare the notification content
                var notifTitle = ((CurrentOperationAttr)currentOperation.GetAttributes()).DisplayText;
                var notifImg = (nbErrors > 0) ? MessageImg.MsgError : ((nbWarnings > 0) ? MessageImg.MsgWarning : MessageImg.MsgOk);
                var notifTimeOut = (nbErrors > 0) ? 0 : ((nbWarnings > 0) ? 10 : 5);
                var notifSubtitle = (nbErrors > 0) ? nbErrors + " critical error(s) found" : ((nbWarnings > 0) ? nbWarnings + " compilation warning(s) found" : "No errors, no warnings!");
                var notifMessage = new StringBuilder((!errorList.Any()) ? "<b>Initial source file :</b><div><a href='" + lastExec.FullFilePathToExecute + "#-1'>" + lastExec.FullFilePathToExecute + "</a></div>" : string.Empty);

                // has errors
                if (errorList.Any()) {
                    notifMessage.Append("<b>Find the incriminated files below :</b>");
                    foreach (var keyValue in errorList) {
                        notifMessage.Append("<div><b>[x" + keyValue.Value.Count() + "]</b> <a href='" + keyValue.Key + "#" + keyValue.Value.First().Line + "'>" + keyValue.Key + "</a></div>");

                        // feed FilesInfo
                        FilesInfo.UpdateFileErrors(keyValue.Key, keyValue.Value);
                    }
                }

                // Update info on the current file
                if (isCurrentFile)
                    FilesInfo.DisplayCurrentFileInfo();

                // when compiling, if no errors, move .r to compilation dir
                if (lastExec.ExecutionType == ExecutionType.Compile && !errorList.Any()) {
                    // copy to compilation dir
                    if (!string.IsNullOrEmpty(lastExec.DotRPath) && !string.IsNullOrEmpty(lastExec.LstPath)) {
                        var success = true;
                        var targetDir = CompilationPath.GetCompilationDirectory(lastExec.FullFilePathToExecute);
                        var targetFile = Path.Combine(targetDir, Path.GetFileName(lastExec.DotRPath));
                        try {
                            File.Delete(targetFile);
                            File.Move(lastExec.DotRPath, targetFile);
                        } catch (Exception) {
                            UserCommunication.Notify("There was a problem when i tried to write the following file:<br>" + targetFile + "<br>The compiled file couldn't been moved to this location!<br><br><i>Please make sure that you have the privileges to write in the targeted compilation directory</i>", MessageImg.MsgError, notifTitle, "Couldn't write target file");
                            success = false;
                        }
                        try {
                            targetFile = Path.Combine(targetDir, Path.GetFileName(lastExec.LstPath));
                            File.Delete(targetFile);
                            File.Move(lastExec.LstPath, targetFile);
                        } catch (Exception) {
                            UserCommunication.Notify("There was a problem when i tried to write the following file:<br>" + targetFile + "<br>The compiled file couldn't been moved to this location!<br><br><i>Please make sure that you have the privileges to write in the targeted compilation directory</i>", MessageImg.MsgError, notifTitle, "Couldn't write target file");
                            success = false;
                        }
                        if (success) {
                            // TODO notif?

                        }
                    }
                }

                // Notify the user, or not
                if (Config.Instance.CompileAlwaysShowNotification || !isCurrentFile || !Npp.GetFocus())
                    UserCommunication.Notify(notifMessage.ToString(), notifImg, notifTitle, notifSubtitle, args => {
                        var splitted = args.Link.Split('#');
                        Npp.Goto(splitted[0], int.Parse(splitted[1]));
                    }, notifTimeOut);
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in OnCompileEnded");
            }
        }

        public static void StartProgressExec(ExecutionType executionType) {
            CurrentOperation currentOperation;
            if (!Enum.TryParse(executionType.ToString(), true, out currentOperation))
                currentOperation = CurrentOperation.Run;

            // Can't compile and check syntax the same file at the same time
            if (Plug.CurrentFileObject.CurrentOperation.HasFlag(CurrentOperation.CheckSyntax) || Plug.CurrentFileObject.CurrentOperation.HasFlag(CurrentOperation.Compile) || Plug.CurrentFileObject.CurrentOperation.HasFlag(CurrentOperation.Run)) {
                UserCommunication.Notify("This file is already being compiled or run,<br>please wait the end of the previous action!", MessageImg.MsgRip, ((CurrentOperationAttr)currentOperation.GetAttributes()).DisplayText, "Already being compiled/run", 5);
                return;
            }

            // launch the compile process for the current file
            Plug.CurrentFileObject.ProgressExecution = new ProgressExecution();
            Plug.CurrentFileObject.ProgressExecution.ProcessExited += OnCompileEnded;
            if (!Plug.CurrentFileObject.ProgressExecution.Do(executionType))
                return;

            // change file object current operation, set flag
            Plug.CurrentFileObject.CurrentOperation |= currentOperation;

            // clear current errors (updates the current file info)
            if (!FilesInfo.ClearAllErrors())
                FilesInfo.DisplayCurrentFileInfo();
        }


        #endregion


        public static void NotImplemented() {
            UserCommunication.Notify(
                "<b>Oops!</b><br><br>This function is not implemented yet, come back later mate ;)<br><br><i>Sorry for the disappointment though!</i>",
                MessageImg.MsgRip, "New action", "Function not implemented yet", 5);
        }
    }
}
