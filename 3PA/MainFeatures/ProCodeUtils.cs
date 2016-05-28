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
using System.Text.RegularExpressions;
using _3PA.Data;
using _3PA.Html;
using _3PA.Lib;
using _3PA.MainFeatures.Appli;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.CodeExplorer;
using _3PA.MainFeatures.FilesInfoNs;
using _3PA.MainFeatures.Parser;
using _3PA.MainFeatures.ProgressExecutionNs;

namespace _3PA.MainFeatures {
    internal static class ProCodeUtils {

        #region Go to definition

        /// <summary>
        /// This method allows the user to GOTO a word definition, if a tooltip is opened then it tries to 
        /// go to the definition of the displayed word, otherwise it tries to find the declaration of the parsed word under the
        /// caret. At last, it tries to find a file in the propath
        /// </summary>
        public static void GoToDefinition(bool fromMouseClick) {
            // if a tooltip is opened, try to execute the "go to definition" of the tooltip first
            if (InfoToolTip.InfoToolTip.IsVisible) {
                if (!String.IsNullOrEmpty(InfoToolTip.InfoToolTip.GoToDefinitionFile)) {
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
            var curWord = Npp.GetWordAtPosition(position);


            // match a word in the autocompletion? go to definition
            var data = AutoComplete.FindInCompletionData(curWord, position, true);
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
            var fullPaths = ProEnvironment.Current.FindFiles(curWord, Config.Instance.KnownProgressExtension);
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
        }

        public static void GoToDefinition() {
            GoToDefinition(false);
        }

        #endregion

        #region Toggle comment

        /// <summary>
        /// If no selection, comment the line of the caret
        /// If selection, comment the selection as a block
        /// </summary>
        public static void ToggleComment() {
            Npp.BeginUndoAction();

            // for each selection (limit selection number)
            for (var i = 0; i < Npp.Selection.Count; i++) {
                var selection = Npp.GetSelection(i);

                int startPos;
                int endPos;
                bool singleLineComm = false;
                if (selection.Caret == selection.Anchor) {
                    // comment line
                    var thisLine = new Npp.Line(Npp.LineFromPosition(selection.Caret));
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

            Npp.EndUndoAction();
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
                Npp.SetTextByRange(startPos, startPos, "/*  */");
                return 3;
            }

            // line is surrounded by /* */
            if (Npp.GetTextOnRightOfPos(startPos, 2).Equals("/*") && Npp.GetTextOnLeftOfPos(endPos, 2).Equals("*/")) {
                if (Npp.GetTextByRange(startPos, endPos).Equals("/*  */")) {
                    // delete an empty comment
                    Npp.SetTextByRange(startPos, endPos, String.Empty);
                } else {
                    // delete /* */
                    Npp.SetTextByRange(endPos - 2, endPos, String.Empty);
                    Npp.SetTextByRange(startPos, startPos + 2, String.Empty);
                }
                return 1;
            }

            Npp.SetTextByRange(endPos, endPos, "*/");
            Npp.SetTextByRange(startPos, startPos, "/*");
            return 2;
        }

        #endregion

        #region Open help

        /// <summary>
        /// Opens the lgrfeng.chm file if it can find it in the config
        /// </summary>
        public static void Open4GlHelp() {
            // get path
            if (String.IsNullOrEmpty(Config.Instance.GlobalHelpFilePath)) {
                if (File.Exists(ProEnvironment.Current.ProwinPath)) {
                    // Try to find the help file from the prowin32.exe location
                    var helpPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(ProEnvironment.Current.ProwinPath) ?? "", "..", "prohelp", "lgrfeng.chm"));
                    if (File.Exists(helpPath)) {
                        Config.Instance.GlobalHelpFilePath = helpPath;
                        UserCommunication.Notify("I've found an help file here :<br>" + helpPath.ToHtmlLink() + "<br>If you think this is incorrect, you can change the help file path in the settings", MessageImg.MsgInfo, "Opening 4GL help", "Found help file", 10);
                    }
                }
            }

            if (String.IsNullOrEmpty(Config.Instance.GlobalHelpFilePath) || !File.Exists(Config.Instance.GlobalHelpFilePath) || !Path.GetExtension(Config.Instance.GlobalHelpFilePath).EqualsCi(".chm")) {
                UserCommunication.Notify("Could not access the help file, please be sure to provide a valid path the the file <b>lgrfeng.chm</b> in the settings window", MessageImg.MsgInfo, "Opening help file", "File not found", 10);
                return;
            }

            HtmlHelpInterop.DisplayIndex(0, Config.Instance.GlobalHelpFilePath, 
                (Math.Abs(Npp.SelectionEnd - Npp.SelectionStart) < 15) ? Npp.SelectedText : "");
        }

        #endregion

        #region Compilation, Check syntax, Run, Prolint

        /// <summary>
        /// Called after the execution of run/compile/check/prolint
        /// </summary>
        public static void OnExecutionEnd(ProExecution lastExec) {
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
        public static void OnExecutioOk(ProExecution lastExec) {
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
                var errorList = ProExecution.LoadErrorLog(lastExec);

                if (!errorList.Any()) {
                    // the compiler messages are empty
                    var fileInfo = new FileInfo(lastExec.LogPath);
                    if (fileInfo.Length > 0) {
                        // the .log is not empty, maybe something went wrong in the runner, display errors
                        UserCommunication.Notify(
                            "Something went wrong while " + ((DisplayAttr)currentOperation.GetAttributes()).ActionText + " the following file:<br>" + treatedFile.InputPath.ToHtmlLink() + "<br>The progress compiler didn't return any errors but the log isn't empty, here is the content :" +
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
                var notifTitle = ((DisplayAttr)currentOperation.GetAttributes()).Name;
                var notifImg = (nbErrors > 0) ? MessageImg.MsgError : ((nbWarnings > 0) ? MessageImg.MsgWarning : MessageImg.MsgOk);
                var notifTimeOut = (nbErrors > 0) ? 0 : ((nbWarnings > 0) ? 10 : 5);
                var notifSubtitle = lastExec.ExecutionType == ExecutionType.Prolint ? (nbErrors + nbWarnings) + " problems detected" :
                    (nbErrors > 0) ? nbErrors + " error" + (nbErrors > 1 ? "s" : "") + " found" :
                        ((nbWarnings > 0) ? nbWarnings + " warning" + (nbWarnings > 1 ? "s" : "") + " found" : 
                            "Syntax correct");

                // build the error list
                var errorsList = new List<FileError>();
                foreach (var keyValue in errorList) {
                    errorsList.AddRange(keyValue.Value);
                }

                // when compiling, move .r/.lst to compilation dir
                var fileToMoveList = new List<FileToMove>();
                if (lastExec.ExecutionType == ExecutionType.Compile) {
                    fileToMoveList = ProExecution.CreateListOfFilesToMove(lastExec);
                    foreach (var fileToMove in fileToMoveList) {
                        fileToMove.IsOk = Utils.MoveFile(fileToMove.From, fileToMove.To, true);
                    }
                }

                // Notify the user, or not
                if (Config.Instance.CompileAlwaysShowNotification || !isCurrentFile || !Npp.GetFocus() || otherFilesInError)
                    UserCommunication.Notify("Was compiling :<br>" + ProExecution.FormatCompilationResult(treatedFile, errorsList, fileToMoveList), notifImg, notifTitle, notifSubtitle, notifTimeOut);

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in OnExecutionOk");
            }
        }


        /// <summary>
        /// Called to run/compile/check/prolint the current program
        /// </summary>
        public static void StartProgressExec(ExecutionType executionType) {
            CurrentOperation currentOperation;
            if (!Enum.TryParse(executionType.ToString(), true, out currentOperation))
                currentOperation = CurrentOperation.Run;

            // check conditions
            if (Plug.CurrentFileObject.CurrentOperation.HasFlag(CurrentOperation.CheckSyntax) || 
                Plug.CurrentFileObject.CurrentOperation.HasFlag(CurrentOperation.Compile) || 
                Plug.CurrentFileObject.CurrentOperation.HasFlag(CurrentOperation.Run) ||
                Plug.CurrentFileObject.CurrentOperation.HasFlag(CurrentOperation.Prolint)) {
                UserCommunication.Notify("This file is already being compiled, run or lint-ed.<br>Please wait the end of the previous action,<br>or click the link below to interrupt the previous action :<br><a href='#'>Click to kill the associated prowin process</a><b", MessageImg.MsgRip, ((DisplayAttr)currentOperation.GetAttributes()).Name, "Already being compiled/run", args => {
                    Plug.CurrentFileObject.ProgressExecution.KillProcess();
                    OnExecutionEnd(Plug.CurrentFileObject.ProgressExecution);
                }, 5);
                return;
            }
            if (!Abl.IsCurrentProgressFile()) {
                UserCommunication.Notify("Can only compile and run progress files!", MessageImg.MsgWarning, "Invalid file type", "Progress files only", 10);
                return;
            }
            if (string.IsNullOrEmpty(Plug.CurrentFilePath) || !File.Exists(Plug.CurrentFilePath)) {
                UserCommunication.Notify("Couldn't find the following file :<br>" + Plug.CurrentFilePath, MessageImg.MsgError, "Execution error", "File not found", 10);
                return;
            }
            if (!Config.Instance.CompileKnownExtension.Split(',').Contains(Path.GetExtension(Plug.CurrentFilePath))) {
                UserCommunication.Notify("Sorry, the file extension " + Path.GetExtension(Plug.CurrentFilePath).ProgressQuoter() + " isn't a valid extension for this action!<br><i>You can change the list of valid extensions in the settings window</i>", MessageImg.MsgWarning, "Invalid file extension", "Not an executable", 10);
                return;
            }

            // prolint? check that the StartProlint.p program is created, or do it
            if (executionType == ExecutionType.Prolint) {
                if (!File.Exists(Config.FileStartProlint))
                    if (!Utils.FileWriteAllBytes(Config.FileStartProlint, DataResources.StartProlint))
                        return;
            }

            // launch the compile process for the current file
            Plug.CurrentFileObject.ProgressExecution = new ProExecution {
                ListToCompile = new List<FileToCompile> {
                    new FileToCompile { InputPath = Plug.CurrentFilePath }
                },
                OnExecutionEnd = OnExecutionEnd,
                OnExecutionOk = OnExecutioOk
            };
            if (!Plug.CurrentFileObject.ProgressExecution.Do(executionType))
                return;

            // change file object current operation, set flag
            Plug.CurrentFileObject.CurrentOperation |= currentOperation;
            FilesInfo.UpdateFileStatus();

            // clear current errors (updates the current file info)
            FilesInfo.ClearAllErrors(Plug.CurrentFilePath, true);

            /*
            // clear error on include files as well
            var explorerItemsList = ParserHandler.GetParsedExplorerItemsList();
            if (explorerItemsList != null) {
                foreach (var codeExplorerItem in explorerItemsList.Where(codeExplorerItem => codeExplorerItem.Branch == CodeExplorerBranch.Include && !codeExplorerItem.Flag.HasFlag(CodeExplorerFlag.NotFound))) {
                    FilesInfo.ClearAllErrors(ProEnvironment.Current.FindFirstFileInPropath(codeExplorerItem.DisplayText), false);
                }
            }
             * */
        }


        #endregion

        #region Modification tags

        /// <summary>
        /// Allows the user to surround its selection with custom modification tags
        /// </summary>
        public static void SurroundSelectionWithTag() {
            CommonTagAction(fileInfo => {
                var output = new StringBuilder();

                Npp.TargetFromSelection();
                var indent = new String(' ', Npp.GetLine(Npp.LineFromPosition(Npp.TargetStart)).Indentation);

                var opener = FileTag.ReplaceTokens(fileInfo, Config.Instance.TagModifOpener);
                var eol = Npp.GetEolString;
                output.Append(opener);
                output.Append(eol);
                output.Append(indent);
                output.Append(Npp.SelectedText);
                output.Append(eol);
                output.Append(indent);
                output.Append(FileTag.ReplaceTokens(fileInfo, Config.Instance.TagModifCloser));

                Npp.TargetFromSelection();
                Npp.ReplaceTarget(output.ToString());

                Npp.SetSel(Npp.TargetStart + opener.Length + eol.Length);
            });
        }

        /// <summary>
        /// Allows the user to generate a title block at the caret location, using the current file info
        /// </summary>
        public static void AddTitleBlockAtCaret() {
            CommonTagAction(fileInfo => {
                var output = new StringBuilder();
                var eol = Npp.GetEolString;
                output.Append(FileTag.ReplaceTokens(fileInfo, Config.Instance.TagTitleBlock1));
                output.Append(eol);

                // description
                var regex = new Regex(@"({&de\s*})");
                var match = regex.Match(Config.Instance.TagTitleBlock2);
                if (match.Success && !string.IsNullOrEmpty(fileInfo.CorrectionDecription)) {
                    var matchedStr = match.Groups[1].Value;
                    foreach (var line in fileInfo.CorrectionDecription.BreakText(matchedStr.Length).Split('\n')) {
                        output.Append(Config.Instance.TagTitleBlock2.Replace(matchedStr, string.Format("{0,-" + matchedStr.Length + @"}", line)));
                        output.Append(eol);
                    }
                }

                output.Append(FileTag.ReplaceTokens(fileInfo, Config.Instance.TagTitleBlock3));
                output.Append(eol);

                Npp.SetTextByRange(Npp.CurrentPosition, Npp.CurrentPosition, output.ToString());
                Npp.SetSel(Npp.CurrentPosition + output.Length);
            });
        }

        public static void CommonTagAction(Action<FileTagObject> performAction) {
            var filename = Path.GetFileName(Plug.CurrentFilePath);
            if (FileTag.Contains(filename)) {
                var fileInfo = FileTag.GetLastFileTag(filename);
                Npp.BeginUndoAction();
                performAction(fileInfo);
                Npp.EndUndoAction();
            } else {
                UserCommunication.Notify("No info available for this file, please fill the file info form first!", MessageImg.MsgToolTip, "Insert modification tags", "No info available", 4);
                Appli.Appli.GoToPage(PageNames.FileInfo);
            }
        }

        #endregion

        #region Open in appbuilder / dictionary

        /// <summary>
        /// Opens the current file in the appbuilder
        /// </summary>
        public static void OpenCurrentInAppbuilder() {
            new ProExecution {
                ListToCompile = new List<FileToCompile> {
                    new FileToCompile { InputPath = Plug.CurrentFilePath }
                },
                OnExecutionOk = execution => {
                    try {
                        if (!string.IsNullOrEmpty(execution.LogPath) && File.Exists(execution.LogPath) && File.ReadAllText(execution.LogPath).ContainsFast("_ab")) {
                            UserCommunication.Notify("Faile to start the appbuilder, the following commands both failed :<br><div class='ToolTipcodeSnippet'>RUN adeuib/_uibmain.p.<br>RUN _ab.p.</div><br>Your version of progress might be uncompatible with those statements? If this problem looks anormal to you, please open a new issue on github.", MessageImg.MsgRip, "Start Appbuilder", "The command failed");
                        }
                    } catch (Exception e) {
                        ErrorHandler.ShowErrors(e, "Failed to start the appbuilder");
                    }
                }
            }.Do(ExecutionType.Appbuilder);
        }

        /// <summary>
        /// Opens the current file in the appbuilder
        /// </summary>
        public static void OpenDictionary() {
            new ProExecution().Do(ExecutionType.Dictionary);
        }

        #endregion

    }
}
