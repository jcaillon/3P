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
using _3PA.MainFeatures.ProgressExecutionNs;

namespace _3PA.MainFeatures {
    internal static class ProUtils {

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

            UserCommunication.Notify("Sorry pal, couldn't go to the definition of <b>" + curWord + "</b>", MessageImg.MsgInfo, "information", "Failed to find an origin", 5);
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
            // get path
            if (string.IsNullOrEmpty(Config.Instance.GlobalHelpFilePath)) {
                if (File.Exists(ProEnvironment.Current.ProwinPath)) {
                    // Try to find the help file from the prowin32.exe location
                    var helpPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(ProEnvironment.Current.ProwinPath) ?? "", "..", "prohelp", "lgrfeng.chm"));
                    if (File.Exists(helpPath)) {
                        Config.Instance.GlobalHelpFilePath = helpPath;
                        UserCommunication.Notify("I've found an help file here :<br>" + helpPath.ToHtmlLink() + "<br>If you think this is incorrect, you can change the help file path in the settings", MessageImg.MsgInfo, "Opening 4GL help", "Found help file", 10);
                    }
                }
            }

            if (string.IsNullOrEmpty(Config.Instance.GlobalHelpFilePath) || !File.Exists(Config.Instance.GlobalHelpFilePath) || !Path.GetExtension(Config.Instance.GlobalHelpFilePath).EqualsCi(".chm")) {
                UserCommunication.Notify("Could not access the help file, please be sure to provide a valid path the the file <b>lgrfeng.chm</b> in the settings window", MessageImg.MsgInfo, "Opening help file", "File not found", 10);
                return;
            }

            // if a tooltip is opened, we search for the displayed word, otherwise take the word at caret
            string searchWord = null;
            if (InfoToolTip.InfoToolTip.IsVisible && !string.IsNullOrEmpty(InfoToolTip.InfoToolTip.CurrentWord))
                searchWord = InfoToolTip.InfoToolTip.CurrentWord;

            HtmlHelpInterop.DisplayIndex(0, Config.Instance.GlobalHelpFilePath, searchWord ?? Npp.GetAblWordAtPosition(Npp.CurrentPosition));
        }

        #endregion

        #region Compilation, Check syntax, Run, Prolint
        
        /// <summary>
        /// Called to run/compile/check/prolint the current program
        /// </summary>
        public static void StartProgressExec(ExecutionType executionType) {
            CurrentOperation currentOperation;
            if (!Enum.TryParse(executionType.ToString(), true, out currentOperation))
                currentOperation = CurrentOperation.Run;

            // process already running?
            if (Plug.CurrentFileObject.CurrentOperation > CurrentOperation.Prolint) {
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
                OnExecutionEnd = ProCompilation.OnSingleExecutionEnd,
                OnExecutionOk = ProCompilation.OnSingleExecutionOk
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
            var explorerItemsList = ParserHandler.CodeExplorerItemsList();
            if (explorerItemsList != null) {
                foreach (var codeExplorerItem in explorerItemsList.Where(codeExplorerItem => codeExplorerItem.Branch == CodeExplorerBranch.Include && !codeExplorerItem.Flag.HasFlag(CodeExplorerFlag.NotFound))) {
                    FilesInfo.ClearAllErrors(ProEnvironment.Current.FindFirstFileInPropath(codeExplorerItem.DisplayText), false);
                }
            }
             * */
        }

        /// <summary>
        /// Allows to kill the process of the currently running Progress.exe (if any, for the current file)
        /// </summary>
        public static void KillCurrentProcess() {
            if (Plug.CurrentFileObject.ProgressExecution != null) {
                Plug.CurrentFileObject.ProgressExecution.KillProcess();
                UserCommunication.CloseUniqueNotif("KillExistingProcess");
                ProCompilation.OnSingleExecutionEnd(Plug.CurrentFileObject.ProgressExecution);
            }
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
    }
}
