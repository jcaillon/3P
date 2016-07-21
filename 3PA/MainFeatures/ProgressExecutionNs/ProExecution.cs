#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProExecution.cs) is part of 3P.
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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YamuiFramework.Helper;
using _3PA.Data;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures.Appli;

// ReSharper disable LocalizableElement

namespace _3PA.MainFeatures.ProgressExecutionNs {

    internal class ProExecution {

        #region public fields

        /// <summary>
        /// The action to execute just after the end of a prowin process
        /// </summary>
        public Action<ProExecution> OnExecutionEnd { private get; set; }

        /// <summary>
        /// The action to execute at the end of the process if it went well = we found a .log and the database is connected or is not mandatory
        /// </summary>
        public Action<ProExecution> OnExecutionOk { private get; set; }

        /// <summary>
        /// The action to execute at the end of the process if something went wrong (no .log or database down)
        /// </summary>
        public Action<ProExecution> OnExecutionFailed { private get; set; }

        /// <summary>
        /// Full path to the directory containing all the files needed for the execution
        /// </summary>
        public string TempDir { get; private set; }

        /// <summary>
        /// Full path to the directory used as the working directory to start the prowin process
        /// </summary>
        public string ProcessStartDir { get; private set; }

        public List<FileToCompile> ListToCompile { get; set; }

        /// <summary>
        /// Path to the output .log file (for compilation)
        /// </summary>
        public string LogPath { get; private set; }

        /// <summary>
        /// progress32.exe used for the execution/compilation
        /// </summary>
        public string ProgressWin32 { get; private set; }

        public ExecutionType ExecutionType { get; private set; }

        /// <summary>
        /// Parameters of the .exe call
        /// </summary>
        public string ExeParameters { get; private set; }

        public Process Process { get; private set; }

        /// <summary>
        /// Full file path to the output file of the DumpDatabase program
        /// </summary>
        public string ExtractDbOutputPath { get; set; }

        /// <summary>
        /// Path to a file used to determine the progression of a compilation (useful when compiling multiple programs)
        /// 1 byte = 1 file treated
        /// </summary>
        public string ProgressionFilePath { get; set; }

        /// <summary>
        /// Full file path to the output file of Prolint
        /// </summary>
        public string ProlintOutputPath { get; set; }

        /// <summary>
        /// Full file path to the output file for the custom post-execution notification
        /// </summary>
        public string NotificationOutputPath { get; set; }

        /// <summary>
        /// set to true if a valid database connection is mandatory
        /// </summary>
        public bool NeedDatabaseConnection { get; set; }

        /// <summary>
        /// log to the database connection log
        /// </summary>
        public string DatabaseConnectionLog { get; private set; }

        /// <summary>
        /// set to true if a the execution process has been killed
        /// </summary>
        public bool HasBeenKilled { get; set; }

        /// <summary>
        /// Set to true after the process is over if the execution failed
        /// </summary>
        public bool ExecutionFailed { get; private set; }

        /// <summary>
        /// Set to true after the process is over if the database connection has failed
        /// </summary>
        public bool ConnectionFailed { get; private set; }

        /// <summary>
        /// The pro environment used at the moment the execution was created
        /// </summary>
        public ProEnvironment.ProEnvironmentObject ProEnv { get; private set; }

        #endregion

        #region static

        /// <summary>
        /// Keep a counter of the number of executions in the current session
        /// </summary>
        private static int _proExecutionCounter;

        private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private static bool _dontWarnAboutRCode;

        #endregion

        #region constructors and destructor

        /// <summary>
        /// Deletes temp directory and everything in it
        /// </summary>
        ~ProExecution() {
            try {
                if (Process != null)
                    Process.Close();
                
                Utils.DeleteDirectory(TempDir, true);

                // restore splashscreen
                if (!string.IsNullOrEmpty(ProgressWin32))
                    MoveSplashScreenNoError(Path.Combine(Path.GetDirectoryName(ProgressWin32) ?? "", "splashscreen-3p-disabled.bmp"), Path.Combine(Path.GetDirectoryName(ProgressWin32) ?? "", "splashscreen.bmp"));

            } catch (Exception) {
                // it's only a clean up operation, we don't care if it crashes
            }
        }

        public ProExecution() {
            // create a copy of the current environment
            ProEnv = new ProEnvironment.ProEnvironmentObject(ProEnvironment.Current);

            if (_lock.TryEnterWriteLock(-1)) {
                _proExecutionCounter++;
                _lock.ExitWriteLock();
            } else {
                throw new Exception("Couln't increase the execution counter...");
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// allows to prepare the execution environnement by creating a unique temp folder
        /// and copying every critical files into it
        /// Then execute the progress program
        /// </summary>
        /// <returns></returns>
        public bool Do(ExecutionType executionType) {

            if (ListToCompile == null)
                ListToCompile = new List<FileToCompile>();

            ExecutionType = executionType;

            // check prowin32.exe
            if (!File.Exists(ProEnv.ProwinPath)) {
                UserCommunication.NotifyUnique("ProExecutionChecks", "The file path to Prowin.exe is incorrect : <div class='ToolTipcodeSnippet'>" + ProEnv.ProwinPath + "</div>You must provide a valid path before executing this action<br><i>You can change this path in the <a href='go'>set environment page</a></i>", MessageImg.MsgWarning, "Execution error", "Invalid file path", args => {
                    Appli.Appli.GoToPage(PageNames.SetEnvironment);
                    UserCommunication.CloseUniqueNotif("ProExecutionChecks");
                    args.Handled = true;
                }, 10);
                return false;
            }

            // check compilation dir
            if (executionType == ExecutionType.Compile && !ProEnv.CompileLocally && (!Path.IsPathRooted(ProEnv.BaseCompilationPath))) {
                UserCommunication.NotifyUnique("ProExecutionChecks", "The path for the compilation base directory is incorrect : <div class='ToolTipcodeSnippet'>" + (string.IsNullOrEmpty(ProEnv.BaseCompilationPath) ? "it's empty!" : ProEnv.BaseCompilationPath) + "</div>You must provide a valid path before executing this action :<br><br><i>1. Either change the compilation directory<br>2. Or toggle the option to compile next to the source file!<br><br>The options are configurable in the <a href='go'>set environment page</a></i>", MessageImg.MsgWarning, "Execution error", "Invalid file path", args => {
                    Appli.Appli.GoToPage(PageNames.SetEnvironment);
                    UserCommunication.CloseUniqueNotif("ProExecutionChecks");
                    args.Handled = true;
                }, 10);
                return false;
            }

            // create unique temporary folder
            TempDir = Path.Combine(Config.FolderTemp, _proExecutionCounter + "-" + DateTime.Now.ToString("yyMMdd_HHmmssfff"));
            if (!Utils.CreateDirectory(TempDir))
                return false;
            
            // for each file of the list (there can be none)
            var filesListPath = Path.Combine(TempDir, "files.list");
            StringBuilder filesListcontent = new StringBuilder();
            var count = 1;
            foreach (var fileToCompile in ListToCompile) {

                if (!File.Exists(fileToCompile.InputPath)) {
                    UserCommunication.Notify("Couldn't find the following file :<br>" + fileToCompile.InputPath, MessageImg.MsgError, "Execution error", "File not found", 10);
                    return false;
                }

                // create target directory
                fileToCompile.OutputDir = ProEnv.GetTransfersNeeded(fileToCompile.InputPath).Last().Key;
                if (!Utils.CreateDirectory(fileToCompile.OutputDir))
                    return false;

                // if current file and the file has unsaved modif, we copy the content to a temp file, otherwise we just use the input path (also use the input path for .cls files!)
                if (fileToCompile.InputPath.Equals(Plug.CurrentFilePath) && 
                    (Npp.GetModify || (fileToCompile.BaseFileName ?? "").StartsWith("_")) && 
                    !Path.GetExtension(fileToCompile.InputPath).Equals(".cls")) {
                    fileToCompile.TempInputPath = Path.Combine(TempDir, "tmp_" + DateTime.Now.ToString("yyMMdd_HHmmssfff_") + count + (Path.GetExtension(fileToCompile.InputPath)));
                    File.WriteAllText(fileToCompile.TempInputPath, Npp.Text, Encoding.Default);
                } else {
                    fileToCompile.TempInputPath = fileToCompile.InputPath;
                }

                // we set where the *.lst and *.r files will be generated by the COMPILE command
                var baseFileName = Path.GetFileNameWithoutExtension(fileToCompile.TempInputPath);
                if (Config.Instance.CompileForceUseOfTemp || Path.GetExtension(fileToCompile.InputPath).Equals(".cls")) {
                    // for *.cls files, as many *.r files are generated, we need to compile in the temp directory
                    // we need to know which *.r files were generated for each input file
                    // so each file gets his own sub tempDir
                    var subTempDir = Path.Combine(TempDir, count.ToString());
                    if (!Utils.CreateDirectory(subTempDir))
                        return false;
                    fileToCompile.TempOutputDir = subTempDir;
                    fileToCompile.TempOutputLst = Path.Combine(subTempDir, baseFileName + ".lst");
                    fileToCompile.TempOutputR = Path.Combine(subTempDir, baseFileName + ".r");
                } else {
                    // for anything but *.cls files, we generated where we want to compile, it's the fastest way...
                    fileToCompile.TempOutputDir = fileToCompile.OutputDir;
                    fileToCompile.TempOutputLst = Path.Combine(fileToCompile.OutputDir, baseFileName + ".lst");
                    fileToCompile.TempOutputR = Path.Combine(fileToCompile.OutputDir, baseFileName + ".r");
                }

                // feed files list
                filesListcontent.AppendLine(fileToCompile.TempInputPath.ProQuoter() + " " + fileToCompile.TempOutputDir.ProQuoter() + " " + fileToCompile.TempOutputLst.ProQuoter());

                // when running a procedure, check that a .r is not hiding the program, if that's the case we warn the user
                if (executionType == ExecutionType.Run && !_dontWarnAboutRCode) {
                    if (File.Exists(Path.Combine(Path.GetDirectoryName(fileToCompile.InputPath) ?? fileToCompile.TempOutputDir, baseFileName + ".r"))) {
                        UserCommunication.NotifyUnique("rcodehide", "Friendly warning, an <b>r-code</b> <i>(i.e. *.r file)</i> is hiding the current program<br>If you modified it since the last compilation you might not have the expected behavior...<br><br><i>" + "stop".ToHtmlLink("Click here to not show this message again for this session") + "</i>", MessageImg.MsgWarning, "Execution warning", "An Rcode hides the program", args => { _dontWarnAboutRCode = true; UserCommunication.CloseUniqueNotif("rcodehide"); }, 5);
                    }
                }

                count++;
            }
            File.WriteAllText(filesListPath, filesListcontent.ToString(), Encoding.Default);

            // Move ini file into the execution dir
            var baseIniPath = "";
            if (File.Exists(ProEnv.IniPath)) {
                baseIniPath = Path.Combine(TempDir, "base.ini");
                // we need to copy the .ini but we must delete the PROPATH= part, as stupid as it sounds, if we leave a huge PROPATH 
                // in this file, it increases the compilation time by a stupid amount... unbelievable i know, but trust me, it does...
                var encoding = TextEncodingDetect.GetFileEncoding(ProEnv.IniPath);
                var fileContent = Utils.ReadAllText(ProEnv.IniPath, encoding);
                var regex = new Regex("^PROPATH=.*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                var matches = regex.Match(fileContent);
                if (matches.Success) 
                    fileContent = regex.Replace(fileContent, @"PROPATH=");
                File.WriteAllText(baseIniPath, fileContent, encoding);
            }

            // Move pf file into the execution dir
            var basePfPath = "";
            if (File.Exists(ProEnv.GetPfPath())) {
                basePfPath = Path.Combine(TempDir, "base.pf");
                File.Copy(ProEnv.GetPfPath(), basePfPath);
            }

            // set common info on the execution
            LogPath = Path.Combine(TempDir, "run.log");
            ProcessStartDir = (ListToCompile.Count == 1) ? Path.GetDirectoryName(ListToCompile.First().InputPath) ?? TempDir : TempDir;
            ProgressWin32 = ProEnv.ProwinPath;
            if (executionType == ExecutionType.Database)
                ExtractDbOutputPath = Path.Combine(TempDir, ExtractDbOutputPath);
            ProgressionFilePath = Path.Combine(TempDir, "compile.progression");
            DatabaseConnectionLog = Path.Combine(TempDir, "db.ko");
            NotificationOutputPath = Path.Combine(TempDir, "postExecution.notif");
            var propathToUse = (TempDir + "," + string.Join(",", ProEnv.GetProPathDirList));
            string fileToExecute = "";


            if (executionType == ExecutionType.Appbuilder) {
                fileToExecute = ListToCompile.First().InputPath;

            } else if (executionType == ExecutionType.Database) {

                // for database extraction, we need to copy the DumpDatabase program
                fileToExecute = "db_" + DateTime.Now.ToString("yyMMdd_HHmmssfff") + ".p";
                if (!Utils.FileWriteAllBytes(Path.Combine(TempDir, fileToExecute), DataResources.DumpDatabase))
                    return false;

            } else if (executionType == ExecutionType.Prolint) {

                // prolint, we need to copy the StartProlint program
                fileToExecute = "prolint_" + DateTime.Now.ToString("yyMMdd_HHmmssfff") + ".p";
                ProlintOutputPath = Path.Combine(TempDir, "prolint.log");
                StringBuilder prolintProgram = new StringBuilder();
                prolintProgram.AppendLine("&SCOPED-DEFINE PathFileToProlint " + ListToCompile.First().TempInputPath.ProQuoter());
                prolintProgram.AppendLine("&SCOPED-DEFINE PathProlintOutputFile " + ProlintOutputPath.ProQuoter());
                prolintProgram.AppendLine("&SCOPED-DEFINE PathToStartProlintProgram " + Config.FileStartProlint.ProQuoter());
                prolintProgram.AppendLine("&SCOPED-DEFINE UserName " + Config.Instance.UserName.ProQuoter());
                prolintProgram.AppendLine("&SCOPED-DEFINE PathActualFilePath " + ListToCompile.First().InputPath.ProQuoter());
                var filename = Path.GetFileName(Plug.CurrentFilePath);
                if (FileTag.Contains(filename)) {
                    var fileInfo = FileTag.GetLastFileTag(filename);
                    prolintProgram.AppendLine("&SCOPED-DEFINE FileApplicationName " + fileInfo.ApplicationName.ProQuoter());
                    prolintProgram.AppendLine("&SCOPED-DEFINE FileApplicationVersion " + fileInfo.ApplicationVersion.ProQuoter());
                    prolintProgram.AppendLine("&SCOPED-DEFINE FileWorkPackage " + fileInfo.WorkPackage.ProQuoter());
                    prolintProgram.AppendLine("&SCOPED-DEFINE FileBugID " + fileInfo.BugId.ProQuoter());
                    prolintProgram.AppendLine("&SCOPED-DEFINE FileCorrectionNumber " + fileInfo.CorrectionNumber.ProQuoter());
                    prolintProgram.AppendLine("&SCOPED-DEFINE FileDate " + fileInfo.CorrectionDate.ProQuoter());
                    prolintProgram.AppendLine("&SCOPED-DEFINE FileCorrectionDescription " + fileInfo.CorrectionDecription.Replace("\r", "").Replace("\n", "~n").ProQuoter());
                }
                var encoding = TextEncodingDetect.GetFileEncoding(Config.FileStartProlint);
                File.WriteAllText(Path.Combine(TempDir, fileToExecute), Utils.ReadAllText(Config.FileStartProlint, encoding).Replace(@"/*<inserted_3P_values>*/", prolintProgram.ToString()), encoding);

            } else if (executionType == ExecutionType.DataDigger) {
                // need to init datadigger?
                if (!File.Exists(Path.Combine(Config.FolderDataDigger, "DataDigger.p"))) {
                    if (!Utils.FileWriteAllBytes(Path.Combine(Config.FolderDataDigger, "DataDigger.zip"), DataResources.DataDigger))
                        return false;
                    if (!Utils.ExtractAll(Path.Combine(Config.FolderDataDigger, "DataDigger.zip"), Config.FolderDataDigger))
                        return false;
                }
                // add the datadigger folder to the propath
                propathToUse = Config.FolderDataDigger + "," + propathToUse;

            } else {

                if (ListToCompile.Count == 1)
                    fileToExecute = ListToCompile.First().TempInputPath;
            }

            // prepare the .p runner
            var runnerPath = Path.Combine(TempDir, "run_" + DateTime.Now.ToString("yyMMdd_HHmmssfff") + ".p");
            StringBuilder runnerProgram = new StringBuilder();
            runnerProgram.AppendLine("&SCOPED-DEFINE ExecutionType " + executionType.ToString().ToUpper().ProQuoter());
            runnerProgram.AppendLine("&SCOPED-DEFINE ToExecute " + fileToExecute.ProQuoter());
            runnerProgram.AppendLine("&SCOPED-DEFINE LogFile " + LogPath.ProQuoter());
            runnerProgram.AppendLine("&SCOPED-DEFINE ExtractDbOutputPath " + ExtractDbOutputPath.ProQuoter());
            runnerProgram.AppendLine("&SCOPED-DEFINE propathToUse " + propathToUse.ProQuoter());
            runnerProgram.AppendLine("&SCOPED-DEFINE ExtraPf " + ProEnv.ExtraPf.Trim().ProQuoter());
            runnerProgram.AppendLine("&SCOPED-DEFINE BasePfPath " + basePfPath.Trim().ProQuoter());
            runnerProgram.AppendLine("&SCOPED-DEFINE CompileWithLst " + ProEnv.CompileWithListing);
            runnerProgram.AppendLine("&SCOPED-DEFINE ToCompileListFile " + filesListPath.ProQuoter());
            runnerProgram.AppendLine("&SCOPED-DEFINE CreateFileIfConnectFails " + DatabaseConnectionLog.ProQuoter());
            runnerProgram.AppendLine("&SCOPED-DEFINE CompileProgressionFile " + ProgressionFilePath.ProQuoter());
            runnerProgram.AppendLine("&SCOPED-DEFINE DbConnectionMandatory " + NeedDatabaseConnection);
            runnerProgram.AppendLine("&SCOPED-DEFINE NotificationOutputPath " + NotificationOutputPath.ProQuoter());
            runnerProgram.Append(Encoding.Default.GetString(DataResources.ProgressRun));
            File.WriteAllText(runnerPath, runnerProgram.ToString(), Encoding.Default);

            // preferably, we use the batch mode because it's faster than the client mode
            var batchMode = (executionType == ExecutionType.CheckSyntax || executionType == ExecutionType.Compile || executionType == ExecutionType.Database);

            // no batch mode option?
            batchMode = batchMode && !Config.Instance.NeverUseProwinInBatchMode;

            // multiple compilation, we don't want to show all those Prowin in the task bar...
            batchMode = batchMode && (ListToCompile.Count <= 1);

            // Parameters
            StringBuilder Params = new StringBuilder();

            if (executionType == ExecutionType.DataDigger)
                Params.Append(" -s 1000 -d dmy -E -rereadnolock -h 255 -Bt 4000 -tmpbsize 8");
            if (executionType != ExecutionType.Run)
                Params.Append(" -T " + TempDir.Trim('\\').ProQuoter());
            if (!string.IsNullOrEmpty(baseIniPath))
                Params.Append(" -ini " + baseIniPath.ProQuoter());
            if (batchMode)
                Params.Append(" -b");
            Params.Append(" -p " + runnerPath.ProQuoter());
            if (!string.IsNullOrWhiteSpace(ProEnv.CmdLineParameters))
                Params.Append(" " + ProEnv.CmdLineParameters.Trim());
            ExeParameters = Params.ToString();

            // we supress the splashscreen
            if (!batchMode)
                MoveSplashScreenNoError(Path.Combine(Path.GetDirectoryName(ProgressWin32) ?? "", "splashscreen.bmp"), Path.Combine(Path.GetDirectoryName(ProgressWin32) ?? "", "splashscreen-3p-disabled.bmp"));

            // Start a process
            var pInfo = new ProcessStartInfo {
                FileName = ProEnv.ProwinPath,
                Arguments = ExeParameters,
                WorkingDirectory = ProcessStartDir
            };
            if (batchMode) {
                pInfo.WindowStyle = ProcessWindowStyle.Hidden;
                pInfo.CreateNoWindow = true;
            }
            Process = new Process {
                StartInfo = pInfo,
                EnableRaisingEvents = true
            };
            Process.Exited += ProcessOnExited;
            try {
                Process.Start();
            } catch (Exception e) {
                UserCommunication.NotifyUnique("ProwinFailed", "Couldn't start a new prowin process!<br>Please check that the file path to prowin32.exe is correct in the <a href='go'>set environment page</a>.<br><br>Below is the technical error that occured :<br><div class='ToolTipcodeSnippet'>" + e.Message + "</div>", MessageImg.MsgError, "Execution error", "Can't start a prowin process", args => {
                    Appli.Appli.GoToPage(PageNames.SetEnvironment);
                    UserCommunication.CloseUniqueNotif("ProwinFailed");
                    args.Handled = true;
                }, 10);
            }

            //UserCommunication.Notify("New process starting...<br><br><b>FileName :</b><br>" + ProEnv.ProwinPath + "<br><br><b>Parameters :</b><br>" + ExeParameters + "<br><br><b>Temporary directory :</b><br><a href='" + TempDir + "'>" + TempDir + "</a>");

            return true;
        }

        /// <summary>
        /// Allows to kill the process of this execution (be careful, the OnExecutionEnd, Ok, Fail events are not executed in that case!) 
        /// </summary>
        public void KillProcess() {
            HasBeenKilled = true;
            try {
                Process.Kill();
                Process.Close();
            } catch (Exception) {
                // ignored
            }
        }

        /// <summary>
        /// Allows to kill the process of this execution (be careful, the OnExecutionEnd, Ok, Fail events are not executed in that case!) 
        /// </summary>
        public void BringProcessToFront() {
            HasBeenKilled = true;
            try {
                WinApi.SetForegroundWindow(Process.MainWindowHandle);
            } catch (Exception) {
                // ignored
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Called by the process's thread when it is over, execute the ProcessOnExited event
        /// </summary>
        private void ProcessOnExited(object sender, EventArgs eventArgs) {

            // end of execution action
            if (OnExecutionEnd != null) {
                OnExecutionEnd(this);
            }

            // if log not found then something is messed up!
            if (string.IsNullOrEmpty(LogPath) || !File.Exists(LogPath)) {
                UserCommunication.NotifyUnique("ExecutionFailed", "Something went terribly wrong while using progress!<br><div>Below is the <b>command line</b> that was executed:</div><div class='ToolTipcodeSnippet'>" + ProgressWin32 + " " + ExeParameters + "</div><b>Temporary directory :</b><br>" + TempDir.ToHtmlLink() + "<br><br><i>Did you messed up the prowin32.exe command line parameters in the <a href='go'>set environment page</a> page?</i>", MessageImg.MsgError, "Critical error", "Action failed", args => {
                    if (args.Link.Equals("go")) {
                        Appli.Appli.GoToPage(PageNames.SetEnvironment);
                        UserCommunication.CloseUniqueNotif("ExecutionFailed");
                        args.Handled = true;
                    }
                }, 0, 600);

                ExecutionFailed = true;
            }

            // if this file exists, then the connect statement failed, warn the user
            if (File.Exists(DatabaseConnectionLog) && new FileInfo(DatabaseConnectionLog).Length > 0) {
                UserCommunication.NotifyUnique("ConnectFailed", "Failed to connect to the progress database!<br>Verify / correct the connection info <a href='go'>in the environment page</a> and try again<br><br><i>Also, make sure that the database for the current environment is connected!</i><br><br>Below is the error returned while trying to connect to the database : " + Utils.ReadAndFormatLogToHtml(DatabaseConnectionLog), MessageImg.MsgRip, "Database connection", "Connection failed", args => {
                    if (args.Link.Equals("go")) {
                        Appli.Appli.GoToPage(PageNames.SetEnvironment);
                        UserCommunication.CloseUniqueNotif("ConnectFailed");
                        args.Handled = true;
                    }
                }, NeedDatabaseConnection ? 0 : 10, 600);

                ConnectionFailed = true;
            }

            // end of successful/unsuccessful execution action
            if (ExecutionFailed || (ConnectionFailed && NeedDatabaseConnection)) {
                if (OnExecutionFailed != null) {
                    OnExecutionFailed(this);
                }
            } else {
                if (OnExecutionOk != null) {
                    OnExecutionOk(this);
                }

                // display a custom post execution notification if needed
                DisplayPostExecutionNotification();
            }
        }

        /// <summary>
        /// move a file, catch the errors
        /// </summary>
        private void MoveSplashScreenNoError(string from, string to) {
            if (File.Exists(from)) {
                try {
                    File.Move(from, to);
                } catch (Exception) {
                    // if it fails it is not really a problem
                }
            }
        }

        /// <summary>
        /// Read a file in which each line represents a notification to display to the user,
        /// and displays each notification
        /// </summary>
        private void DisplayPostExecutionNotification() {

            // no notifications?
            if (string.IsNullOrEmpty(NotificationOutputPath) || !File.Exists(NotificationOutputPath))
                return;

            Utils.ForEachLine(NotificationOutputPath, null, line => {
                var fields = line.Split('\t').ToList();
                if (fields.Count == 6) {

                    MessageImg messageImg;
                    if (!Enum.TryParse(fields[1], true, out messageImg))
                        messageImg = MessageImg.MsgDebug;

                    if (string.IsNullOrEmpty(fields[5]))
                        UserCommunication.Notify(fields[0], messageImg, fields[2], fields[3], (int)fields[4].ConvertFromStr(typeof(int)));
                    else
                        UserCommunication.NotifyUnique(fields[5], fields[0], messageImg, fields[2], fields[3], args => {
                            UserCommunication.CloseUniqueNotif(fields[5]);
                        }, (int)fields[4].ConvertFromStr(typeof(int)));
                }
            });
        }

        #endregion

        #region public

        /// <summary>
        /// Read the compilation/prolint errors of a given execution through its .log file
        /// update the FilesInfo accordingly so the user can see the errors in npp
        /// </summary>
        public Dictionary<string, List<FileError>> LoadErrorLog() {

            // we need to correct the files path in the log if needed
            var changePaths = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            foreach (var treatedFile in ListToCompile.Where(treatedFile => !treatedFile.TempInputPath.Equals(treatedFile.InputPath))) {
                if (!changePaths.ContainsKey(treatedFile.TempInputPath))
                    changePaths.Add(treatedFile.TempInputPath, treatedFile.InputPath);
            }

            // read the log file
            Dictionary<string, List<FileError>> errorsList;
            if (ExecutionType == ExecutionType.Prolint) {
                var treatedFile = ListToCompile.First();
                if (!changePaths.ContainsKey(treatedFile.TempInputPath))
                    changePaths.Add(treatedFile.TempInputPath, treatedFile.InputPath);
                errorsList = FilesInfo.ReadErrorsFromFile(ProlintOutputPath, true, changePaths);
            } else
                errorsList = FilesInfo.ReadErrorsFromFile(LogPath, false, changePaths);

            // clear errors on each compiled file
            foreach (var fileToCompile in ListToCompile) {
                FilesInfo.ClearAllErrors(fileToCompile.InputPath, true);
            }

            // update the errors
            foreach (var keyValue in errorsList) {
                FilesInfo.UpdateFileErrors(keyValue.Key, keyValue.Value);
            }

            return errorsList;
        }

        /// <summary>
        /// Creates a list of files to transfer after a compilation,
        /// for each Origin file will correspond one (or more if it's a .cls) .r file,
        /// and one .lst if the option has been checked
        /// </summary>
        public List<FileToTransfer> CreateListOfFilesToTransfer() {

            var outputList = new List<FileToTransfer>();
            var clsNotFound = new StringBuilder();

            foreach (var treatedFile in ListToCompile) {

                // Is the input file a class file?
                if (treatedFile.InputPath.EndsWith(".cls", StringComparison.CurrentCultureIgnoreCase)) {

                    // if the file we compiled inherits from another class or if another class inherits of our file, 
                    // there is more than 1 *.r file generated. Moreover, they are generated in their package folders
                    List<string> listOfRFiles = null;
                    try {
                        listOfRFiles = Directory.EnumerateFiles(treatedFile.TempOutputDir, "*.r", SearchOption.AllDirectories).ToList();
                    } catch (Exception x) {
                        ErrorHandler.Log(x.ToString());
                    }
                    if (listOfRFiles != null) {

                        // for each *.r file
                        foreach (var file in listOfRFiles) {

                            var relativePath = file.Replace(treatedFile.TempOutputDir, "").TrimStart('\\');
                            var sourcePath = ProEnv.FindFirstFileInPropath(Path.ChangeExtension(relativePath, ".cls"));

                            if (string.IsNullOrEmpty(sourcePath)) {
                                clsNotFound.Append("<div>" + relativePath + "</div>");
                            } else {
                                foreach (KeyValuePair<string, CompilationPath.TransferType> pair in ProEnv.GetTransfersNeeded(sourcePath)) {
                                    string outputRPath;
                                    if (!ProEnv.CompileLocally) {
                                        // transfer the *.r file in the compilation directory (create the needed subdirectories...)
                                        outputRPath = Path.Combine(pair.Key, relativePath);
                                        Utils.CreateDirectory(Path.GetDirectoryName(outputRPath));
                                    } else {
                                        // transfer the *.r file next to his source
                                        outputRPath = Path.Combine(pair.Key, Path.GetFileName(file));
                                    }

                                    // add .r and .lst (if needed) to the list of files to move
                                    outputList.Add(new FileToTransfer(treatedFile.InputPath, file, outputRPath, pair.Value));
                                    if (ProEnv.CompileWithListing && Path.GetFileNameWithoutExtension(relativePath).Equals(Path.GetFileNameWithoutExtension(treatedFile.InputPath))) {
                                        outputList.Add(new FileToTransfer(treatedFile.InputPath, treatedFile.TempOutputLst, Path.ChangeExtension(outputRPath, ".lst"), pair.Value));
                                    }
                                }
                            }
                        }
                    }

                } else {
                    foreach (KeyValuePair<string, CompilationPath.TransferType> pair in ProEnv.GetTransfersNeeded(treatedFile.InputPath)) {
                        // add .r and .lst (if needed) to the list of files to transfer
                        outputList.Add(new FileToTransfer(treatedFile.InputPath, treatedFile.TempOutputR, Path.Combine(pair.Key, treatedFile.BaseFileName + ".r"), pair.Value));
                        if (ProEnv.CompileWithListing) {
                            outputList.Add(new FileToTransfer(treatedFile.InputPath, treatedFile.TempOutputLst, Path.Combine(pair.Key, treatedFile.BaseFileName + ".lst"), pair.Value
                            ));
                        }
                    }
                }
            }

            if (clsNotFound.Length > 0)
                UserCommunication.Notify("Couldn't locate the source file (.cls) for :" + clsNotFound + "in the propath", MessageImg.MsgError, "Post compilation error", "File not found");

            return outputList;
        }

        /// <summary>
        /// Transfer a given list of files (can reduce the list if there are duplicated items so it returns it)
        /// </summary>
        public static List<FileToTransfer> TransferFiles(List<FileToTransfer> transfersNeeded, Action<int> onOneFileDone = null) {

            // make sure to transfer a given file only once at the same place
            transfersNeeded = transfersNeeded.GroupBy(trans => trans.To).Select(group => group.FirstOrDefault(move => Path.GetFileNameWithoutExtension(move.From ?? "").Equals(Path.GetFileNameWithoutExtension(move.Origin))) ?? group.First()).ToList();

            // check that every target dir exist
            transfersNeeded.GroupBy(transfer => Path.GetDirectoryName(transfer.To)).Select(group => group.First()).ToNonNullList().ForEach(transfer => Utils.CreateDirectory(Path.GetDirectoryName(transfer.To)));

            int[] nbFilesDone = { 0 };
            try {
                Parallel.ForEach(transfersNeeded, file => {
                    TransferFile(file);
                    nbFilesDone[0]++;
                    if (onOneFileDone != null)
                        onOneFileDone(nbFilesDone[0]);
                });
            } catch (Exception) {
                nbFilesDone[0] = 0;
                foreach (var file in transfersNeeded) {
                    TransferFile(file);
                    nbFilesDone[0]++;
                    if (onOneFileDone != null)
                        onOneFileDone(nbFilesDone[0]);
                }
            }

            return transfersNeeded;
        }

        /// <summary>
        /// Transfer a single file
        /// </summary>
        private static void TransferFile(FileToTransfer file) {
            if (!file.IsOk) {
                if (File.Exists(file.From)) {
                    switch (file.TransferType) {
                        case CompilationPath.TransferType.Copy:
                            file.IsOk = Utils.CopyFile(file.From, file.To);
                            break;
                        case CompilationPath.TransferType.Ftp:

                            break;
                        case CompilationPath.TransferType.Library:

                            break;
                        default: // move
                            file.IsOk = Utils.MoveFile(file.From, file.To, true);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Allows to format a small text to explain the errors found in a file and the generated files...
        /// </summary>
        public static string FormatCompilationResult(FileToCompile fileToCompile, List<FileError> listErrorFiles, List<FileToTransfer> listTransferFiles) {

            var line = new StringBuilder();
            var nbErrors = 0;

            line.Append("<div style='padding-bottom: 5px;'><b>" + string.Format("<a class='SubTextColor' href='{0}'>{1}</a>", fileToCompile.InputPath, Path.GetFileName(fileToCompile.InputPath)) + "</b> in " + Path.GetDirectoryName(fileToCompile.InputPath).ToHtmlLink() + "</div>");

            foreach (var fileError in listErrorFiles) {
                nbErrors += fileError.Level > ErrorLevel.StrongWarning ? 1 : 0;
                line.Append("<div style='padding-left: 10px'>" + "<img src='" + (fileError.Level > ErrorLevel.StrongWarning ? "MsgError" : "MsgWarning") + "' height='15px'>" + (!fileError.CompiledFilePath.Equals(fileError.SourcePath) ? "in " + string.Format("<a class='SubTextColor' href='{0}'>{1}</a>", fileError.SourcePath, Path.GetFileName(fileError.SourcePath)) + ", " : "") + (fileError.SourcePath + "|" + fileError.Line).ToHtmlLink("line " + (fileError.Line + 1)) + " (n°" + fileError.ErrorNumber + ") " + (fileError.Times > 0 ? "(x" + fileError.Times + ") " : "") + fileError.Message + "</div>");
            }

            foreach (var file in listTransferFiles) {
                var ext = (Path.GetExtension(file.To) ?? "").Replace(".", "");
                var transferMsg = file.TransferType == CompilationPath.TransferType.Move ? "" : "(" + file.TransferType + ") ";
                if (file.IsOk && (nbErrors == 0 || !ext.Equals("r"))) {
                    line.Append("<div style='padding-left: 10px'>" + "<img src='" + ext.ToTitleCase() + "Type' height='15px'>" + transferMsg + (ext.EqualsCi("lst") ? file.To.ToHtmlLink() : Path.GetDirectoryName(file.To).ToHtmlLink(file.To)) + "</div>");
                } else if (nbErrors == 0) {
                    line.Append("<div style='padding-left: 10px'>" + "<img src='MsgError' height='15px'>Transfer error " + transferMsg + Path.GetDirectoryName(file.To).ToHtmlLink(file.To) + "</div>");
                }

            }

            return line.ToString();
        }

        #endregion

    }

    internal enum ExecutionType {
        CheckSyntax = 0,
        Compile = 1,
        Run = 2,
        Prolint = 3,

        Database = 10,
        Appbuilder = 11,
        Dictionary = 12,
        DataDigger = 13
    }

    internal class FileToCompile {
        // stores the path
        public string InputPath { get; set; }
        public string OutputDir { get; set; }
        
        // stores temporary path used during the execution
        public string TempInputPath { get; set; }
        public string TempOutputDir { get; set; }
        public string TempOutputR { get; set; }
        public string TempOutputLst { get; set; }

        public string BaseFileName { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public FileToCompile(string inputPath) {
            InputPath = inputPath;
            BaseFileName = Path.GetFileNameWithoutExtension(inputPath);
        }
    }

    internal class FileToTransfer {

        /// <summary>
        /// The path of input file that was originally compiled to trigger this move
        /// </summary>
        public string Origin { get; set; }
        public string From { get; set; }
        public string To { get; set; }

        /// <summary>
        /// true if the transfer went fine
        /// </summary>
        public bool IsOk { get; set; }

        /// <summary>
        /// Type de transfer
        /// </summary>
        public CompilationPath.TransferType TransferType { get; set; }

        public FileToTransfer(string origin, string @from, string to, CompilationPath.TransferType transferType) {
            Origin = origin;
            From = @from;
            To = to;
            TransferType = transferType;

        }
    }

}
