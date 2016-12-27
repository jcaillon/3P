#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProCodeUtils.cs) is part of 3P.
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
using System.IO;
using System.Linq;
using System.Text;
using _3PA.Data;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;

namespace _3PA.MainFeatures.Pro {
    internal static class ProMisc {

        #region Go to definition

        /// <summary>
        /// This method allows the user to GOTO a word definition, if a tooltip is opened then it tries to 
        /// go to the definition of the displayed word, otherwise it tries to find the declaration of the parsed word under the
        /// caret. At last, it tries to find a file in the propath
        /// </summary>
        public static void GoToDefinition(bool fromMouseClick) {

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
            var position = fromMouseClick ? Npp.GetPositionFromMouseLocation() : Npp.CurrentPosition;
            if (fromMouseClick && position <= 0)
                return;
            var curWord = Npp.GetAblWordAtPosition(position);


            // match a word in the autocompletion? go to definition
            var data = AutoComplete.FindInCompletionData(curWord, position, true);
            if (data != null && data.Count > 0) {

                var nbFound = data.Count(data2 => data2.FromParser);

                // only one match, then go to the definition
                if (nbFound == 1) {
                    var completionData = data.First(data1 => data1.FromParser);
                    Npp.Goto(completionData.ParsedItem.FilePath, completionData.ParsedItem.Line, completionData.ParsedItem.Column);
                    return;
                } 
                if (nbFound > 1) {

                    // otherwise, list the items and notify the user
                    var output = new StringBuilder(@"Found several matching items, please choose the correct one :<br>");
                    foreach (var cData in data.Where(data2 => data2.FromParser)) {
                        output.Append("<div>" + (cData.ParsedItem.FilePath + "|" + cData.ParsedItem.Line + "|" + cData.ParsedItem.Column).ToHtmlLink("In " + Path.GetFileName(cData.ParsedItem.FilePath) + " (line " + cData.ParsedItem.Line + ")"));
                        cData.DoForEachFlag((s, flag) => {
                            output.Append("<img style='padding-right: 0px; padding-left: 5px;' src='" + s + "' height='15px'>");
                        });
                        output.Append("</div>");
                    }
                    UserCommunication.NotifyUnique("GoToDefinition", output.ToString(), MessageImg.MsgQuestion, "Question", "Go to the definition", args => {
                        Utils.OpenPathClickHandler(null, args);
                        UserCommunication.CloseUniqueNotif("GoToDefinition");
                    }, 0, 500);
                    return;
                }
            }

            // last resort, try to find a matching file in the propath

            // if in a string, read the whole string

            // try to read all the . and \

            // first look in the propath
            var fullPaths = ProEnvironment.Current.FindFiles(curWord, Config.Instance.KnownProgressExtension);
            if (fullPaths.Count > 0) {
                if (fullPaths.Count > 1) {
                    var output = new StringBuilder(@"Found several files matching this name, please choose the correct one :<br>");
                    foreach (var fullPath in fullPaths) {
                        output.Append("<div>" + fullPath.ToHtmlLink() + "</div>");
                    }
                    UserCommunication.NotifyUnique("GoToDefinition", output.ToString(), MessageImg.MsgQuestion, "Question", "Open a file", args => {
                        Npp.Goto(args.Link);
                        UserCommunication.CloseUniqueNotif("GoToDefinition");
                        args.Handled = true;
                    }, 0, 500);
                } else
                    Npp.Goto(fullPaths[0]);
                return;
            }

            UserCommunication.Notify("Sorry, couldn't go to the definition of <b>" + curWord + "</b>", MessageImg.MsgInfo, "Information", "Failed to find an origin", 5);
        }

        public static void ForEachFlag(Action<string, ParseFlag> toApplyOnFlag) {
            foreach (var name in Enum.GetNames(typeof(ParseFlag))) {
                ParseFlag flag = (ParseFlag)Enum.Parse(typeof(ParseFlag), name);
                if (flag == 0) continue;
                toApplyOnFlag(name, flag);
            }
        }

        public static void GoToDefinition() {
            GoToDefinition(false);
        }

        #endregion

        #region Open help

        /// <summary>
        /// Opens the lgrfeng.chm file if it can find it in the config
        /// </summary>
        public static void Open4GlHelp() {

            var helpPath = Config.Instance.GlobalHelpFilePath;

            // Try to find the help file from the prowin32.exe location
            if (File.Exists(ProEnvironment.Current.ProwinPath)) {
                var versionHelpPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(ProEnvironment.Current.ProwinPath) ?? "", "..", "prohelp", "lgrfeng.chm"));
                if (File.Exists(versionHelpPath))
                    helpPath = versionHelpPath;
            }

            // set path in config
            if (string.IsNullOrEmpty(Config.Instance.GlobalHelpFilePath)) {
                if (!string.IsNullOrEmpty(helpPath)) {
                    Config.Instance.GlobalHelpFilePath = helpPath;
                    UserCommunication.Notify("I've found an help file here :<br>" + helpPath.ToHtmlLink() + "<br>If you think this is incorrect, you can change the help file path in the settings", MessageImg.MsgInfo, "Opening 4GL help", "Found help file", 10);
                }
            }

            if (string.IsNullOrEmpty(helpPath) || !File.Exists(helpPath) || !Path.GetExtension(helpPath).EqualsCi(".chm")) {
                UserCommunication.Notify("Could not access the help file, please be sure to provide a valid path the the file <b>lgrfeng.chm</b> in the settings window", MessageImg.MsgInfo, "Opening help file", "File not found", 10);
                return;
            }

            if (!helpPath.Equals(Config.Instance.GlobalHelpFilePath) && !string.IsNullOrEmpty(Config.Instance.GlobalHelpFilePath))
                UserCommunication.Notify("Found a different help file for you version of prowin.exe, the following file will be used :<br>" + helpPath.ToHtmlLink(), MessageImg.MsgInfo, "Help file", "New file used", 5);

            // if a tooltip is opened, we search for the displayed word, otherwise take the word at caret
            string searchWord = null;
            if (InfoToolTip.InfoToolTip.IsVisible && !string.IsNullOrEmpty(InfoToolTip.InfoToolTip.CurrentWord))
                searchWord = InfoToolTip.InfoToolTip.CurrentWord;

            HtmlHelpInterop.DisplayIndex(0, helpPath, searchWord ?? Npp.GetAblWordAtPosition(Npp.CurrentPosition));
        }

        #endregion

        #region Open appbuilder / dictionary / Datadigger etc...

        /// <summary>
        /// Opens the current file in the appbuilder
        /// </summary>
        public static void OpenCurrentInAppbuilder() {
            new ProExecution {
                ListToCompile = new List<FileToCompile> {
                    new FileToCompile(Plug.CurrentFilePath)
                },
                OnExecutionOk = execution => {
                    try {
                        if (!string.IsNullOrEmpty(execution.LogPath) && File.Exists(execution.LogPath) && Utils.ReadAllText(execution.LogPath).ContainsFast("_ab")) {
                            UserCommunication.Notify("Failed to start the appbuilder, the following commands both failed :<br><div class='ToolTipcodeSnippet'>RUN adeuib/_uibmain.p.<br>RUN _ab.p.</div><br>Your version of progress might be uncompatible with those statements? If this problem looks anormal to you, please open a new issue on github.", MessageImg.MsgRip, "Start Appbuilder", "The command failed");
                        }
                    } catch (Exception e) {
                        ErrorHandler.ShowErrors(e, "Failed to start the appbuilder");
                    }
                }
            }.Do(ExecutionType.Appbuilder);
        }

        public static void OpenProDesktop() {
            new ProExecution().Do(ExecutionType.ProDesktop);
        }

        public static void OpenDictionary() {
            new ProExecution().Do(ExecutionType.Dictionary);
        }

        public static void OpenDbAdmin() {
            new ProExecution().Do(ExecutionType.DbAdmin);
        }

        public static void OpenDataDigger() {
            new ProExecution().Do(ExecutionType.DataDigger);
        }

        public static void OpenDataReader() {
            new ProExecution().Do(ExecutionType.DataReader);
        }

        #endregion

        #region Single : Compilation, Check syntax, Run, Prolint

        /// <summary>
        /// Called to run/compile/check/prolint the current program
        /// </summary>
        public static void StartProgressExec(ExecutionType executionType) {
            CurrentOperation currentOperation;
            if (!Enum.TryParse(executionType.ToString(), true, out currentOperation))
                currentOperation = CurrentOperation.Run;

            // process already running?
            if (Plug.CurrentFileObject.CurrentOperation >= CurrentOperation.Prolint) {
                UserCommunication.NotifyUnique("KillExistingProcess", "This file is already being compiled, run or lint-ed.<br>Please wait the end of the previous action,<br>or click the link below to interrupt the previous action :<br><a href='#'>Click to kill the associated prowin process</a>", MessageImg.MsgRip, currentOperation.GetAttribute<CurrentOperationAttr>().Name, "Already being compiled/run", args => {
                    KillCurrentProcess();
                    StartProgressExec(executionType);
                    args.Handled = true;
                }, 5);
                return;
            }
            if (!Abl.IsCurrentProgressFile) {
                UserCommunication.Notify("Can only compile and run progress files!", MessageImg.MsgWarning, "Invalid file type", "Progress files only", 10);
                return;
            }
            if (string.IsNullOrEmpty(Plug.CurrentFilePath) || !File.Exists(Plug.CurrentFilePath)) {
                UserCommunication.Notify("Couldn't find the following file :<br>" + Plug.CurrentFilePath, MessageImg.MsgError, "Execution error", "File not found", 10);
                return;
            }
            if (!Config.Instance.CompileKnownExtension.Split(',').Contains(Path.GetExtension(Plug.CurrentFilePath))) {
                UserCommunication.Notify("Sorry, the file extension " + Path.GetExtension(Plug.CurrentFilePath).ProQuoter() + " isn't a valid extension for this action!<br><i>You can change the list of valid extensions in the settings window</i>", MessageImg.MsgWarning, "Invalid file extension", "Not an executable", 10);
                return;
            }

            // update function prototypes
            ProGenerateCode.UpdateFunctionPrototypesIfNeeded(true);

            // prolint? check that the StartProlint.p program is created, or do it
            if (executionType == ExecutionType.Prolint) {
                if (!File.Exists(Config.FileStartProlint))
                    if (!Utils.FileWriteAllBytes(Config.FileStartProlint, DataResources.StartProlint))
                        return;
            }

            // launch the compile process for the current file
            Plug.CurrentFileObject.ProgressExecution = new ProExecution {
                ListToCompile = new List<FileToCompile> {
                    new FileToCompile(Plug.CurrentFilePath)
                },
                OnExecutionEnd = OnSingleExecutionEnd,
                OnExecutionOk = OnSingleExecutionOk
            };
            if (!Plug.CurrentFileObject.ProgressExecution.Do(executionType))
                return;

            // change file object current operation, set flag
            Plug.CurrentFileObject.CurrentOperation |= currentOperation;
            FilesInfo.UpdateFileStatus();

            // clear current errors (updates the current file info)
            FilesInfo.ClearAllErrors(Plug.CurrentFilePath, true);

        }

        /// <summary>
        /// Allows to kill the process of the currently running Progress.exe (if any, for the current file)
        /// </summary>
        public static void KillCurrentProcess() {
            if (Plug.CurrentFileObject.ProgressExecution != null) {
                Plug.CurrentFileObject.ProgressExecution.KillProcess();
                UserCommunication.CloseUniqueNotif("KillExistingProcess");
                OnSingleExecutionEnd(Plug.CurrentFileObject.ProgressExecution);
            }
        }

        /// <summary>
        /// Called after the execution of run/compile/check/prolint, clear the current operation from the file
        /// </summary>
        public static void OnSingleExecutionEnd(ProExecution lastExec) {
            try {
                var treatedFile = lastExec.ListToCompile.First();
                CurrentOperation currentOperation;
                if (!Enum.TryParse(lastExec.ExecutionType.ToString(), true, out currentOperation))
                    currentOperation = CurrentOperation.Run;

                // Clear flag or we can't do any other actions on this file
                FilesInfo.GetFileInfo(treatedFile.InputPath).CurrentOperation &= ~currentOperation;
                var isCurrentFile = treatedFile.InputPath.EqualsCi(Plug.CurrentFilePath);
                if (isCurrentFile)
                    FilesInfo.UpdateFileStatus();

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in OnExecutionEnd");
            }
        }

        /// <summary>
        /// Called after the execution of run/compile/check/prolint
        /// </summary>
        public static void OnSingleExecutionOk(ProExecution lastExec) {
            try {
                var treatedFile = lastExec.ListToCompile.First();
                CurrentOperation currentOperation;
                if (!Enum.TryParse(lastExec.ExecutionType.ToString(), true, out currentOperation))
                    currentOperation = CurrentOperation.Run;

                var isCurrentFile = treatedFile.InputPath.EqualsCi(Plug.CurrentFilePath);
                var otherFilesInError = false;
                int nbWarnings = 0;
                int nbErrors = 0;

                // Read log info
                var errorList = lastExec.LoadErrorLog();

                if (!errorList.Any()) {
                    // the compiler messages are empty
                    var fileInfo = new FileInfo(lastExec.LogPath);
                    if (fileInfo.Length > 0) {
                        // the .log is not empty, maybe something went wrong in the runner, display errors
                        UserCommunication.Notify(
                            "Something went wrong while " + currentOperation.GetAttribute<CurrentOperationAttr>().ActionText + " the following file:<br>" + treatedFile.InputPath.ToHtmlLink() + "<br>The progress compiler didn't return any errors but the log isn't empty, here is the content :" +
                            Utils.ReadAndFormatLogToHtml(lastExec.LogPath), MessageImg.MsgError,
                            "Critical error", "Action failed");
                        return;
                    }
                } else {
                    // count number of warnings/errors, loop through files > loop through errors in each file
                    foreach (var keyValue in errorList) {
                        foreach (var fileError in keyValue.Value) {
                            if (fileError.Level <= ErrorLevel.StrongWarning) nbWarnings++;
                            else nbErrors++;
                        }
                        otherFilesInError = otherFilesInError || !treatedFile.InputPath.EqualsCi(keyValue.Key);
                    }
                }

                // Prepare the notification content
                var notifTitle = currentOperation.GetAttribute<CurrentOperationAttr>().Name;
                var notifImg = (nbErrors > 0) ? MessageImg.MsgError : ((nbWarnings > 0) ? MessageImg.MsgWarning : MessageImg.MsgOk);
                var notifTimeOut = (nbErrors > 0) ? 0 : ((nbWarnings > 0) ? 10 : 5);
                var notifSubtitle = lastExec.ExecutionType == ExecutionType.Prolint ? (nbErrors + nbWarnings) + " problem" + ((nbErrors + nbWarnings) > 1 ? "s" : "") + " detected" :
                    (nbErrors > 0) ? nbErrors + " error" + (nbErrors > 1 ? "s" : "") + " found" :
                        ((nbWarnings > 0) ? nbWarnings + " warning" + (nbWarnings > 1 ? "s" : "") + " found" :
                            "Syntax correct");

                // build the error list
                var errorsList = new List<FileError>();
                foreach (var keyValue in errorList) {
                    errorsList.AddRange(keyValue.Value);
                }

                // when compiling, transfering .r/.lst to compilation dir
                var listTransferFiles = new List<FileToDeploy>();
                if (lastExec.ExecutionType == ExecutionType.Compile) {
                    listTransferFiles = lastExec.CreateListOfFilesToDeploy();
                    listTransferFiles = lastExec.ProEnv.Deployer.DeployFiles(listTransferFiles);
                }

                // Notify the user, or not
                if (Config.Instance.CompileAlwaysShowNotification || !isCurrentFile || !Npp.GetFocus() || otherFilesInError)
                    UserCommunication.NotifyUnique(treatedFile.InputPath, "Was " + currentOperation.GetAttribute<CurrentOperationAttr>().ActionText + " :<br>" + ProCompilation.FormatCompilationResult(treatedFile.InputPath, errorsList, listTransferFiles), notifImg, notifTitle, notifSubtitle, null, notifTimeOut);

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in OnExecutionOk");
            }
        }

        #endregion
        
        #region DeployCurrentFile

        /// <summary>
        /// Deploy the current file, if it's a progress file then compile it, otherwise follow the transer rules of step 1
        /// </summary>
        public static void DeployCurrentFile() {
            if (Abl.IsCurrentFileCompilable) {
                // then that's just a link to compilation
                StartProgressExec(ExecutionType.Compile);

                UserCommunication.Notify("Deploying a compilable file is strictly equal as compiling it<br>The deployment rules for step 0 are applied in both case!", MessageImg.MsgInfo, "Deploy a file", "Bypass to compilation", 2);
            } else {
                if (ProEnvironment.Current.Deployer.IsFilePassingFilters(
                    Plug.CurrentFilePath,
                    ProEnvironment.Current.Deployer.DeployFilterRules.Where(rule => rule.Step == 1 && rule.Include).ToList(),
                    ProEnvironment.Current.Deployer.DeployFilterRules.Where(rule => rule.Step == 1 && !rule.Include).ToList())) {

                    // deploy the file for STEP 1
                    var deployedFiles = ProEnvironment.Current.Deployer.DeployFiles(ProEnvironment.Current.Deployer.GetTransfersNeededForFile(Plug.CurrentFilePath, 1));
                    if (deployedFiles == null || deployedFiles.Count == 0) {
                        UserCommunication.Notify("The current file doesn't match any transfer rules for the current environment and <b>step 1</b><br>You can modify the rules " + "here".ToHtmlLink(), MessageImg.MsgInfo, "Deploy a file", "No transfer rules", args => {
                            Deployer.EditRules();
                            args.Handled = true;
                        }, 5);
                    } else {
                        var hasError = deployedFiles.Exists(deploy => !deploy.IsOk);
                        UserCommunication.NotifyUnique(Plug.CurrentFilePath, "Rules applied for <b>step 1</b>, was deploying :<br>" + ProCompilation.FormatCompilationResult(Plug.CurrentFilePath, null, deployedFiles), hasError ? MessageImg.MsgError : MessageImg.MsgOk, "Deploy a file", "Transfer results", null, hasError ? 0 : 5);
                    }
                } else { 
                    UserCommunication.Notify("The current file didn't pass the deployment filters for the current environment and <b>step 1</b><br>You can modify the rules " + "here".ToHtmlLink(), MessageImg.MsgInfo, "Deploy a file", "Filtered by deployment rules", args => {
                        Deployer.EditRules();
                        args.Handled = true;
                    }, 5);
                }
            }
        }

        #endregion

    }
}
