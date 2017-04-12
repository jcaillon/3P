#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProMisc.cs) is part of 3P.
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
using YamuiFramework.Forms;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletionFeature;
using _3PA.MainFeatures.Parser;
using _3PA.MainFeatures.Pro.Deploy;
using _3PA.NppCore;
using _3PA.WindowsCore;
using _3PA._Resource;

namespace _3PA.MainFeatures.Pro {
    internal static class ProMisc {
        #region Toggle comment

        /// <summary>
        /// If no selection, comment the line of the caret
        /// If selection, comment the selection as a block
        /// </summary>
        public static void ToggleComment() {
            Sci.BeginUndoAction();

            // for each selection (limit selection number)
            for (var i = 0; i < Sci.Selection.Count; i++) {
                var selection = Sci.GetSelection(i);

                int startPos;
                int endPos;
                bool singleLineComm = false;
                if (selection.Caret == selection.Anchor) {
                    // comment line
                    var thisLine = new Sci.Line(Sci.LineFromPosition(selection.Caret));
                    startPos = thisLine.IndentationPosition;
                    endPos = thisLine.EndPosition;
                    singleLineComm = true;
                } else {
                    startPos = selection.Start;
                    endPos = selection.End;
                }

                var toggleMode = ToggleCommentOnRange(startPos, endPos);
                if (toggleMode == 3)
                    selection.SetPosition(startPos + 3);

                // correct selection...
                if (!singleLineComm && toggleMode == 2) {
                    selection.End += 2;
                }
            }

            Sci.EndUndoAction();
        }

        /// <summary>
        /// Toggle comment on the specified range, returns a value indicating what has been done
        /// 0: null, 1: toggle off; 2: toggle on, 3: added
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns></returns>
        private static int ToggleCommentOnRange(int startPos, int endPos) {
            // the line is essentially empty
            if ((endPos - startPos) == 0) {
                Sci.SetTextByRange(startPos, startPos, "/*  */");
                return 3;
            }

            // line is surrounded by /* */
            if (Sci.GetTextOnRightOfPos(startPos, 2).Equals("/*") && Sci.GetTextOnLeftOfPos(endPos, 2).Equals("*/")) {
                if (Sci.GetTextByRange(startPos, endPos).Equals("/*  */")) {
                    // delete an empty comment
                    Sci.SetTextByRange(startPos, endPos, String.Empty);
                } else {
                    // delete /* */
                    Sci.SetTextByRange(endPos - 2, endPos, String.Empty);
                    Sci.SetTextByRange(startPos, startPos + 2, String.Empty);
                }
                return 1;
            }

            Sci.SetTextByRange(endPos, endPos, "*/");
            Sci.SetTextByRange(startPos, startPos, "/*");
            return 2;
        }

        #endregion

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
                    InfoToolTip.InfoToolTip.Cloak();
                    return;
                }
                InfoToolTip.InfoToolTip.Cloak();
            }

            // try to go to the definition of the selected word
            var position = fromMouseClick ? Sci.GetPositionFromMouseLocation() : Sci.CurrentPosition;
            if (fromMouseClick && position <= 0)
                return;
            var curWord = Sci.GetWordAtPosition(position, AutoCompletion.CurrentLangAdditionalChars);

            if (string.IsNullOrEmpty(curWord))
                return;

            // match a word in the autocompletion? go to definition
            var listKeywords = AutoCompletion.FindInCompletionData(curWord, Sci.LineFromPosition(position));
            if (listKeywords != null) {
                var listItems = listKeywords.Where(item => item.FromParser && item.ParsedBaseItem is ParsedItem).ToList();
                if (listItems.Count > 0) {
                    // only one match, then go to the definition
                    if (listItems.Count == 1) {
                        var pItem = listItems.First().ParsedBaseItem as ParsedItem;
                        if (pItem != null) {
                            Npp.Goto(pItem.FilePath, pItem.Line, pItem.Column);
                            return;
                        }
                    }
                    if (listItems.Count > 1) {
                        // otherwise, list the items and notify the user
                        var output = new StringBuilder(@"Found several matching items, please choose the correct one :<br>");
                        foreach (var cData in listItems) {
                            var pItem = listItems.First().ParsedBaseItem as ParsedItem;
                            if (pItem != null) {
                                output.Append("<div>" + (pItem.FilePath + "|" + pItem.Line + "|" + pItem.Column).ToHtmlLink("In " + Path.GetFileName(pItem.FilePath) + " (line " + pItem.Line + ")"));
                                cData.DoForEachFlag((s, flag) => { output.Append("<img style='padding-right: 0px; padding-left: 5px;' src='" + s + "' height='15px'>"); });
                                output.Append("</div>");
                            }
                        }
                        UserCommunication.NotifyUnique("GoToDefinition", output.ToString(), MessageImg.MsgQuestion, "Question", "Go to the definition", args => {
                            Utils.OpenPathClickHandler(null, args);
                            UserCommunication.CloseUniqueNotif("GoToDefinition");
                        }, 0, 500);
                        return;
                    }
                }
            }

            // last resort, try to find a matching file in the propath

            // if in a string, read the whole string

            // try to read all the . and \

            // first look in the propath
            var fullPaths = ProEnvironment.Current.FindFiles(curWord, Config.Instance.FilesPatternProgress);
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

            HtmlHelpInterop.DisplayIndex(0, helpPath, searchWord ?? Sci.GetWordAtPosition(Sci.CurrentPosition, AutoCompletion.CurrentLangAdditionalChars));
        }

        #endregion

        #region OpenCompilationOptions

        /// <summary>
        /// Allow the user to modify the compilation options
        /// </summary>
        public static void OpenCompilationOptions() {
            object input = new CompilationOptions();
            var res = (CompilationOptions)input;
            res.CompileWithDebugList = Config.Instance.CompileWithDebugList;
            res.CompileWithXref = Config.Instance.CompileWithXref;
            res.CompileWithListing = Config.Instance.CompileWithListing;
            res.UseXrefXml = Config.Instance.CompileUseXmlXref; 
            if (UserCommunication.Input(ref input, "Choose the compilation options below", MessageImg.MsgQuestion, "Compilation options", "Set new options") != 0)
                return;
            res = (CompilationOptions) input;
            Config.Instance.CompileWithDebugList = res.CompileWithDebugList;
            Config.Instance.CompileWithXref = res.CompileWithXref;
            Config.Instance.CompileWithListing = res.CompileWithListing;
            Config.Instance.CompileUseXmlXref = res.UseXrefXml;
        }

        internal class CompilationOptions {
            [YamuiInput("Compile with DEBUG-LIST option", Order = 0)]
            public bool CompileWithDebugList{ get; set; }

            [YamuiInput("Compile with LISTING option", Order = 1)]
            public bool CompileWithListing { get; set; }

            [YamuiInput("Compile with XREF option", Order = 2)]
            public bool CompileWithXref { get; set; }

            [YamuiInput("Use XREF-XML instead of XREF", Order = 3)]
            public bool UseXrefXml { get; set; }
        }

        #endregion

        #region Open appbuilder / dictionary / Datadigger etc...

        /// <summary>
        /// Opens the current file in the appbuilder
        /// </summary>
        public static void OpenCurrentInAppbuilder() {
            new ProExecutionAppbuilder {
                CurrentFile = Npp.CurrentFile.Path
            }.Start();
        }

        public static void OpenProDesktop() {
            new ProExecutionProDesktop().Start();
        }

        public static void OpenDictionary() {
            new ProExecutionDictionary().Start();
        }

        public static void OpenDbAdmin() {
            new ProExecutionDbAdmin().Start();
        }

        public static void OpenDataDigger() {
            new ProExecutionDataDigger().Start();
        }

        public static void OpenDataReader() {
            new ProExecutionDataReader().Start();
        }

        #endregion

        #region Single : Compilation, Check syntax, Run, Prolint

        private static bool _dontWarnAboutRCode;

        /// <summary>
        /// Called to run/compile/check/prolint the current program
        /// </summary>
        public static void StartProgressExec(ExecutionType executionType, Action<ProExecutionHandleCompilation> execSetter = null) {
            CurrentOperation currentOperation;
            if (!Enum.TryParse(executionType.ToString(), true, out currentOperation))
                currentOperation = CurrentOperation.Run;

            // process already running?
            if (FilesInfo.CurrentFileInfoObject.CurrentOperation >= CurrentOperation.Prolint) {
                UserCommunication.NotifyUnique("KillExistingProcess", "This file is already being compiled, run or lint-ed.<br>Please wait the end of the previous action,<br>or click the link below to interrupt the previous action :<br><a href='#'>Click to kill the associated prowin process</a>", MessageImg.MsgRip, currentOperation.GetAttribute<CurrentOperationAttr>().Name, "Already being compiled/run", args => {
                    KillCurrentProcess();
                    StartProgressExec(executionType);
                    args.Handled = true;
                }, 5);
                return;
            }
            if (!Npp.CurrentFile.IsProgress) {
                UserCommunication.Notify("Can only compile and run progress files!", MessageImg.MsgWarning, "Invalid file type", "Progress files only", 10);
                return;
            }
            if (string.IsNullOrEmpty(Npp.CurrentFile.Path) || !File.Exists(Npp.CurrentFile.Path)) {
                UserCommunication.Notify("Couldn't find the following file :<br>" + Npp.CurrentFile.Path, MessageImg.MsgError, "Execution error", "File not found", 10);
                return;
            }
            if (!Npp.CurrentFile.IsCompilable) {
                UserCommunication.Notify("Sorry, the file extension " + Path.GetExtension(Npp.CurrentFile.Path).ProQuoter() + " isn't a valid extension for this action!<br><i>You can change the list of valid extensions in the settings window</i>", MessageImg.MsgWarning, "Invalid file extension", "Not an executable", 10);
                return;
            }
                    
            // when running a procedure, check that a .r is not hiding the program, if that's the case we warn the user
            if (executionType == ExecutionType.Run && !_dontWarnAboutRCode) {
                if (File.Exists(Path.ChangeExtension(Npp.CurrentFile.Path, ".r"))) {
                    UserCommunication.NotifyUnique("rcodehide", "Friendly warning, an <b>r-code</b> <i>(i.e. *.r file)</i> is hiding the current program<br>If you modified it since the last compilation you might not have the expected behavior...<br><br><i>" + "stop".ToHtmlLink("Click here to not show this message again for this session") + "</i>", MessageImg.MsgWarning, "Progress execution", "An Rcode hides the program", args => {
                        _dontWarnAboutRCode = true;
                        UserCommunication.CloseUniqueNotif("rcodehide");
                    }, 5);
                }
            }

            // update function prototypes
            ProGenerateCode.Factory.UpdateFunctionPrototypesIfNeeded(true);

            // prolint? check that the StartProlint.p program is created, or do it
            if (executionType == ExecutionType.Prolint) {
                if (!File.Exists(Config.FileStartProlint))
                    if (!Utils.FileWriteAllBytes(Config.FileStartProlint, DataResources.StartProlint))
                        return;
            }

            // launch the compile process for the current file
            FilesInfo.CurrentFileInfoObject.ProgressExecution = (ProExecutionHandleCompilation) ProExecution.Factory(executionType);
            FilesInfo.CurrentFileInfoObject.ProgressExecution.Files = new List<FileToCompile> {
                new FileToCompile(Npp.CurrentFile.Path)
            };
            FilesInfo.CurrentFileInfoObject.ProgressExecution.OnExecutionEnd += OnSingleExecutionEnd;
            if (execSetter != null) {
                execSetter(FilesInfo.CurrentFileInfoObject.ProgressExecution);
                FilesInfo.CurrentFileInfoObject.ProgressExecution.OnCompilationOk += OnGenerateDebugFileOk;
            } else {
                FilesInfo.CurrentFileInfoObject.ProgressExecution.OnCompilationOk += OnSingleExecutionOk;
            }
            if (!FilesInfo.CurrentFileInfoObject.ProgressExecution.Start())
                return;

            // change file object current operation, set flag
            FilesInfo.CurrentFileInfoObject.CurrentOperation |= currentOperation;
            FilesInfo.UpdateFileStatus();

            // clear current errors (updates the current file info)
            FilesInfo.ClearAllErrors(Npp.CurrentFile.Path, true);
        }
        
        /// <summary>
        /// Allows to kill the process of the currently running Progress.exe (if any, for the current file)
        /// </summary>
        public static void KillCurrentProcess() {
            if (FilesInfo.CurrentFileInfoObject.ProgressExecution != null) {
                FilesInfo.CurrentFileInfoObject.ProgressExecution.KillProcess();
                UserCommunication.CloseUniqueNotif("KillExistingProcess");
                OnSingleExecutionEnd(FilesInfo.CurrentFileInfoObject.ProgressExecution);
            }
        }

        /// <summary>
        /// Called after the execution of run/compile/check/prolint, clear the current operation from the file
        /// </summary>
        public static void OnSingleExecutionEnd(ProExecution lastExec) {
            try {
                var exec = (ProExecutionHandleCompilation) lastExec;
                var treatedFile = exec.Files.First();
                CurrentOperation currentOperation;
                if (!Enum.TryParse(exec.ExecutionType.ToString(), true, out currentOperation))
                    currentOperation = CurrentOperation.Run;

                // Clear flag or we can't do any other actions on this file
                FilesInfo.GetFileInfo(treatedFile.SourcePath).CurrentOperation &= ~currentOperation;
                var isCurrentFile = treatedFile.SourcePath.EqualsCi(Npp.CurrentFile.Path);
                if (isCurrentFile)
                    FilesInfo.UpdateFileStatus();

                FilesInfo.CurrentFileInfoObject.ProgressExecution = null;
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in OnExecutionEnd");
            }
        }


        private static void OnGenerateDebugFileOk(ProExecutionHandleCompilation lastExec, List<FileToCompile> fileToCompiles, List<FileToDeploy> filesToDeploy) {
            var exec = (ProExecutionGenerateDebugfile) lastExec;
            if (!string.IsNullOrEmpty(exec.GeneratedFilePath) && File.Exists(exec.GeneratedFilePath)) {
                if (exec.CompileWithDebugList) {
                    //make the .dbg file more readable
                    var output = new StringBuilder();
                    Utils.ForEachLine(exec.GeneratedFilePath, new byte[0], (i, line) => {
                        output.AppendLine(line.Length > 12 ? line.Substring(12) : string.Empty);
                    });
                    Utils.FileWriteAllText(exec.GeneratedFilePath, output.ToString());
                }
                Npp.Goto(exec.GeneratedFilePath);
            }
            OnSingleExecutionOk(lastExec, fileToCompiles, filesToDeploy);
        }

        /// <summary>
        /// Called after the execution of run/compile/check/prolint
        /// </summary>
        public static void OnSingleExecutionOk(ProExecutionHandleCompilation lastExec, List<FileToCompile> filesToCompile, List<FileToDeploy> filesToDeploy) {
            try {
                var treatedFile = lastExec.Files.First();
                CurrentOperation currentOperation;
                if (!Enum.TryParse(lastExec.ExecutionType.ToString(), true, out currentOperation))
                    currentOperation = CurrentOperation.Run;

                var isCurrentFile = treatedFile.SourcePath.EqualsCi(Npp.CurrentFile.Path);
                var otherFilesInError = false;
                int nbWarnings = 0;
                int nbErrors = 0;
                
                // count number of warnings/errors, loop through files > loop through errors in each file
                foreach (var fileInError in filesToCompile.Where(file => file.Errors != null)) {
                    foreach (var error in fileInError.Errors) {
                        if (error.Level <= ErrorLevel.StrongWarning) nbWarnings++;
                        else nbErrors++;
                    }
                    otherFilesInError = otherFilesInError || !treatedFile.SourcePath.EqualsCi(fileInError.SourcePath);
                }

                // Prepare the notification content
                var notifTitle = currentOperation.GetAttribute<CurrentOperationAttr>().Name;
                var notifImg = (nbErrors > 0) ? MessageImg.MsgError : ((nbWarnings > 0) ? MessageImg.MsgWarning : MessageImg.MsgOk);
                var notifTimeOut = (nbErrors > 0) ? 0 : ((nbWarnings > 0) ? 10 : 5);
                var notifSubtitle = lastExec.ExecutionType == ExecutionType.Prolint ? (nbErrors + nbWarnings) + " problem" + ((nbErrors + nbWarnings) > 1 ? "s" : "") + " detected" :
                    (nbErrors > 0) ? nbErrors + " error" + (nbErrors > 1 ? "s" : "") + " found" :
                    ((nbWarnings > 0) ? nbWarnings + " warning" + (nbWarnings > 1 ? "s" : "") + " found" :
                        "Syntax correct");

                // when compiling, transferring .r/.lst to compilation dir
                if (filesToDeploy != null) {
                    filesToDeploy = lastExec.ProEnv.Deployer.DeployFiles(filesToDeploy, null, null);
                }

                // Notify the user, or not
                if (Config.Instance.CompileAlwaysShowNotification || !isCurrentFile || !Sci.GetFocus() || otherFilesInError)
                    UserCommunication.NotifyUnique(treatedFile.SourcePath, "<div style='padding-bottom: 5px;'>Was " + currentOperation.GetAttribute<CurrentOperationAttr>().ActionText + " :</div>" + ProExecutionCompile.FormatCompilationResultForSingleFile(treatedFile.SourcePath, treatedFile, filesToDeploy), notifImg, notifTitle, notifSubtitle, null, notifTimeOut);
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
            if (Npp.CurrentFile.IsCompilable) {
                // then that's just a link to compilation
                StartProgressExec(ExecutionType.Compile);

                UserCommunication.Notify("Deploying a compilable file is strictly equal as compiling it<br>The deployment rules for step 0 are applied in both case!", MessageImg.MsgInfo, "Deploy a file", "Bypass to compilation", 2);
            } else {
                var currentDeployer = ProEnvironment.Current.Deployer;
                if (currentDeployer.GetFilteredList(new List<string> { Npp.CurrentFile.Path }, 1).Any()) {
                    // deploy the file for STEP 1
                    var deployedFiles = currentDeployer.DeployFiles(currentDeployer.GetTransfersNeededForFile(Npp.CurrentFile.Path, 1), null, null);
                    if (deployedFiles == null || deployedFiles.Count == 0) {
                        UserCommunication.Notify("The current file doesn't match any transfer rules for the current environment and <b>step 1</b><br>You can modify the rules " + "here".ToHtmlLink(), MessageImg.MsgInfo, "Deploy a file", "No transfer rules", args => {
                            DeploymentRules.EditRules();
                            args.Handled = true;
                        }, 5);
                    } else {
                        var hasError = deployedFiles.Exists(deploy => !deploy.IsOk);
                        UserCommunication.NotifyUnique(Npp.CurrentFile.Path, "Rules applied for <b>step 1</b>, was deploying :<br>" + ProExecutionCompile.FormatCompilationResultForSingleFile(Npp.CurrentFile.Path, null, deployedFiles), hasError ? MessageImg.MsgError : MessageImg.MsgOk, "Deploy a file", "Transfer results", null, hasError ? 0 : 5);
                    }
                } else {
                    UserCommunication.Notify("The current file didn't pass the deployment filters for the current environment and <b>step 1</b><br>You can modify the rules " + "here".ToHtmlLink(), MessageImg.MsgInfo, "Deploy a file", "Filtered by deployment rules", args => {
                        DeploymentRules.EditRules();
                        args.Handled = true;
                    }, 5);
                }
            }
        }

        #endregion
    }
}