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
using _3PA.Data;
using _3PA.Html;
using _3PA.Lib;
using _3PA.MainFeatures.Appli;
using _3PA.MainFeatures.FilesInfoNs;

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
        /// set to true if a valid database connection is mandatory
        /// </summary>
        public bool NeedDatabaseConnection { get; set; }

        /// <summary>
        /// set to true if a the execution process has been killed
        /// </summary>
        public bool HasBeenKilled { get; set; }

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
            } catch (Exception) {
                // it's only a clean up operation, we don't care if it crashes
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

            // check prowin32.exe
            if (!File.Exists(ProEnvironment.Current.ProwinPath)) {
                UserCommunication.Notify("The file path to prowin32.exe is incorrect : <br><br>" + ProEnvironment.Current.ProwinPath + "<br><br>You must provide a valid path before executing this action<br><i>You can change this path in the <a href='go'>set environment page</a></i>", MessageImg.MsgWarning, "Execution error", "Invalid file path", args => {
                    Appli.Appli.GoToPage(PageNames.SetEnvironment); args.Handled = true;
                }, 10);
                return false;
            }

            // create unique temporary folder
            TempDir = Path.Combine(Config.FolderTemp, DateTime.Now.ToString("yyMMdd_HHmmssfff"));
            while (Directory.Exists(TempDir)) TempDir += "_";
            if (!Utils.CreateDirectory(TempDir))
                return false;

            // for each file of the list
            var filesListPath = Path.Combine(TempDir, "files.list");
            StringBuilder filesListcontent = new StringBuilder();
            var count = 1;
            foreach (var fileToCompile in ListToCompile) {

                if (!File.Exists(fileToCompile.InputPath)) {
                    UserCommunication.Notify("Couldn't find the following file :<br>" + fileToCompile.InputPath, MessageImg.MsgError, "Execution error", "File not found", 10);
                    return false;
                }

                // each file gets his own sub tempDir, because for .cls files, there can be several *.r generated, we need to know which *.r files were generated for each input file
                var subTempDir = Path.Combine(TempDir, count.ToString());
                if (!Utils.CreateDirectory(subTempDir))
                    return false;

                // create target directory
                fileToCompile.OutputDir = ProCompilePath.GetCompilationDirectory(fileToCompile.InputPath);
                if (Config.Instance.GlobalCompileFilesLocally || string.IsNullOrEmpty(fileToCompile.OutputDir))
                    fileToCompile.OutputDir = Path.GetDirectoryName(fileToCompile.InputPath) ?? fileToCompile.OutputDir;
                if (!Utils.CreateDirectory(fileToCompile.OutputDir))
                    return false;

                // get base file name and set the output .r and .lst file path
                var baseFileName = Path.GetFileNameWithoutExtension(fileToCompile.InputPath);
                fileToCompile.OutputLst = Path.Combine(fileToCompile.OutputDir, baseFileName + ".lst");
                fileToCompile.OutputR = Path.Combine(fileToCompile.OutputDir, baseFileName + ".r");

                // if current file and the file has unsaved modif, we copy the content to a temp file, otherwise we just use the input path (also use the input path for .cls files!)
                if (fileToCompile.InputPath.Equals(Plug.CurrentFilePath) && 
                    (Npp.GetModify || (baseFileName ?? "").StartsWith("_")) && 
                    !Path.GetExtension(fileToCompile.InputPath).Equals(".cls")) {
                    // for cls, the file name is important so don't change it, otherwise, get a temporary name
                    fileToCompile.TempInputPath = Path.Combine(subTempDir, "tmp_" + DateTime.Now.ToString("yyMMdd_HHmmssfff") + (Path.GetExtension(fileToCompile.InputPath)));
                    File.WriteAllText(fileToCompile.TempInputPath, Npp.Text, Encoding.Default);
                } else {
                    fileToCompile.TempInputPath = fileToCompile.InputPath;
                }
                baseFileName = Path.GetFileNameWithoutExtension(fileToCompile.TempInputPath);
                fileToCompile.TempOutputLst = Path.Combine(subTempDir, baseFileName + ".lst");
                fileToCompile.TempOutputR = Path.Combine(subTempDir, baseFileName + ".r");
                fileToCompile.TempOutputDir = subTempDir;

                // feed files list
                filesListcontent.AppendLine(fileToCompile.TempInputPath.ProgressQuoter() + " " + fileToCompile.TempOutputDir.ProgressQuoter() + " " + fileToCompile.TempOutputLst.ProgressQuoter());

                count++;
            }
            File.WriteAllText(filesListPath, filesListcontent.ToString(), Encoding.Default);


            // Move ini file into the execution dir
            var baseIniPath = "";
            if (File.Exists(ProEnvironment.Current.IniPath)) {
                baseIniPath = Path.Combine(TempDir, "base.ini");
                // we need to copy the .ini but we must delete the PROPATH= part, as stupid as it sounds, if we leave a huge PROPATH 
                // in this file, it increases the compilation time by a stupid amount... unbelievable i know, but trust me, it does...
                var fileContent = File.ReadAllText(ProEnvironment.Current.IniPath, Encoding.Default);
                var regex = new Regex("^PROPATH=.*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                var matches = regex.Match(fileContent);
                if (matches.Success) 
                    fileContent = regex.Replace(fileContent, @"PROPATH=");
                File.WriteAllText(baseIniPath, fileContent, Encoding.Default);
            }

            // Move pf file into the execution dir
            var basePfPath = "";
            if (File.Exists(ProEnvironment.Current.GetPfPath())) {
                basePfPath = Path.Combine(TempDir, "base.pf");
                File.Copy(ProEnvironment.Current.GetPfPath(), basePfPath);
            }

            StringBuilder programContent = new StringBuilder();

            string fileToExecute = "";
            if (executionType == ExecutionType.Appbuilder) {
                fileToExecute = ListToCompile.First().InputPath;

            } else if (executionType == ExecutionType.Database) {

                // for database extraction, we need to copy the DumpDatabase program
                fileToExecute = "db_" + DateTime.Now.ToString("yyMMdd_HHmmssfff") + ".p";
                File.WriteAllBytes(Path.Combine(TempDir, fileToExecute), DataResources.DumpDatabase);

            } else if (executionType == ExecutionType.Prolint) {

                // prolint, we need to copy the StartProlint program
                fileToExecute = "prolint_" + DateTime.Now.ToString("yyMMdd_HHmmssfff") + ".p";
                ProlintOutputPath = Path.Combine(TempDir, "prolint.log");
                programContent.AppendLine("&SCOPED-DEFINE PathFileToProlint " + ListToCompile.First().TempInputPath.ProgressQuoter());
                programContent.AppendLine("&SCOPED-DEFINE PathProlintOutputFile " + ProlintOutputPath.ProgressQuoter());
                programContent.AppendLine("&SCOPED-DEFINE PathToStartProlintProgram " + Config.FileStartProlint.ProgressQuoter());
                programContent.AppendLine("&SCOPED-DEFINE UserName " + Config.Instance.UserName.ProgressQuoter());
                programContent.AppendLine("&SCOPED-DEFINE PathActualFilePath " + ListToCompile.First().InputPath.ProgressQuoter());
                var filename = Path.GetFileName(Plug.CurrentFilePath);
                if (FileTag.Contains(filename)) {
                    var fileInfo = FileTag.GetLastFileTag(filename);
                    programContent.AppendLine("&SCOPED-DEFINE FileApplicationName " + fileInfo.ApplicationName.ProgressQuoter());
                    programContent.AppendLine("&SCOPED-DEFINE FileApplicationVersion " + fileInfo.ApplicationVersion.ProgressQuoter());
                    programContent.AppendLine("&SCOPED-DEFINE FileWorkPackage " + fileInfo.WorkPackage.ProgressQuoter());
                    programContent.AppendLine("&SCOPED-DEFINE FileBugID " + fileInfo.BugId.ProgressQuoter());
                    programContent.AppendLine("&SCOPED-DEFINE FileCorrectionNumber " + fileInfo.CorrectionNumber.ProgressQuoter());
                    programContent.AppendLine("&SCOPED-DEFINE FileDate " + fileInfo.CorrectionDate.ProgressQuoter());
                    programContent.AppendLine("&SCOPED-DEFINE FileCorrectionDescription " + fileInfo.CorrectionDecription.Replace("\r", "").Replace("\n", "~n").ProgressQuoter());
                }
                File.WriteAllText(Path.Combine(TempDir, fileToExecute), File.ReadAllText(Config.FileStartProlint, TextEncodingDetect.GetFileEncoding(Config.FileStartProlint)).Replace(@"/*<inserted_3P_values>*/", programContent.ToString()), Encoding.Default);

            } else {

                if (ListToCompile.Count == 1)
                    fileToExecute = ListToCompile.First().TempInputPath;
            }

            // set info on the execution
            LogPath = Path.Combine(TempDir, "run.log");
            ExecutionType = executionType;
            ProcessStartDir = (ListToCompile.Count == 1) ? Path.GetDirectoryName(ListToCompile.First().InputPath) ?? TempDir : TempDir;
            ProgressWin32 = ProEnvironment.Current.ProwinPath;
            if (executionType == ExecutionType.Database)
                ExtractDbOutputPath = Path.Combine(TempDir, ExtractDbOutputPath);
            ProgressionFilePath = Path.Combine(TempDir, "compile.progression");

            // prepare the .p runner
            var runnerPath = Path.Combine(TempDir, "run_" + DateTime.Now.ToString("yyMMdd_HHmmssfff") + ".p");
            programContent.Clear();
            programContent.AppendLine("&SCOPED-DEFINE ExecutionType " + executionType.ToString().ToUpper().ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE ToExecute " + fileToExecute.ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE LogFile " + LogPath.ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE ExtractDbOutputPath " + ExtractDbOutputPath.ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE propathToUse " + (TempDir + "," + string.Join(",", ProEnvironment.Current.GetProPathDirList)).ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE ExtraPf " + ProEnvironment.Current.ExtraPf.Trim().ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE BasePfPath " + basePfPath.Trim().ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE ToCompileListFile " + filesListPath.ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE CreateFileIfConnectFails " + Path.Combine(TempDir, "db.ko").ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE CompileProgressionFile " + ProgressionFilePath.ProgressQuoter());
            programContent.Append(Encoding.Default.GetString(DataResources.ProgressRun));

            File.WriteAllText(runnerPath, programContent.ToString(), Encoding.Default);


            var batchMode = !(executionType == ExecutionType.Run || executionType == ExecutionType.Prolint || executionType == ExecutionType.Appbuilder || executionType == ExecutionType.Dictionary);

            // Parameters
            StringBuilder Params = new StringBuilder();
            
            Params.Append(" -T " + Path.GetTempPath().Trim('\\').ProgressQuoter());
            if (!string.IsNullOrEmpty(baseIniPath))
                Params.Append(" -ini " + baseIniPath.ProgressQuoter());
            if (batchMode && Config.Instance.UseBatchModeToCompile)
                Params.Append(" -b");
            Params.Append(" -p " + runnerPath.ProgressQuoter());
            if (!string.IsNullOrWhiteSpace(ProEnvironment.Current.CmdLineParameters))
                Params.Append(" " + ProEnvironment.Current.CmdLineParameters.Trim());
            ExeParameters = Params.ToString();


            // we supress the splashscreen
            var splashScreenPath = Path.Combine(Path.GetDirectoryName(ProgressWin32) ?? "", "splashscreen.bmp");
            var splashScreenPathMoved = Path.Combine(Path.GetDirectoryName(ProgressWin32) ?? "", "splashscreen-3p-disabled.bmp");
            MoveSplashScreenNoError(splashScreenPath, splashScreenPathMoved);

            // Start a process
            var pInfo = new ProcessStartInfo {
                FileName = ProEnvironment.Current.ProwinPath,
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
                UserCommunication.Notify("Couldn't start a new prowin process!<br>Please check that the file path to prowin32.exe is correct in the <a href='go'>set environment page</a>.<br><br>Below is the technical error that occured :<br><div class='ToolTipcodeSnippet'>" + e.Message + "</div>", MessageImg.MsgError, "Execution error", "Can't start a prowin process", args => {
                    Appli.Appli.GoToPage(PageNames.SetEnvironment);
                    args.Handled = true;
                }, 10);
            }

            //UserCommunication.Notify("New process starting...<br><br><b>FileName :</b><br>" + ProEnvironment.Current.ProwinPath + "<br><br><b>Parameters :</b><br>" + ExeParameters + "<br><br><b>Temporary directory :</b><br><a href='" + TempDir + "'>" + TempDir + "</a>");

            // restore splashscreen
            MoveSplashScreenNoError(splashScreenPathMoved, splashScreenPath);

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

        #endregion

        #region private methods

        /// <summary>
        /// Called by the process's thread when it is over, execute the ProcessOnExited event
        /// </summary>
        private void ProcessOnExited(object sender, EventArgs eventArgs) {

            bool endedSuccessfully = true;

            // end of execution action
            if (OnExecutionEnd != null) {
                OnExecutionEnd(this);
            }

            // if log not found then something is messed up!
            if (string.IsNullOrEmpty(LogPath) || !File.Exists(LogPath)) {
                UserCommunication.Notify("Something went terribly wrong while using progress!<br><div>Below is the <b>command line</b> that was executed:</div><div class='ToolTipcodeSnippet'>" + ProgressWin32 + " " + ExeParameters + "</div><b>Temporary directory :</b><br>" + TempDir.ToHtmlLink() + "<br><br><i>Did you messed up the prowin32.exe command line parameters in the <a href='go'>set environment page</a> page?</i>", MessageImg.MsgError, "Critical error", "Action failed", args => {
                    if (args.Link.Equals("go")) {
                        Appli.Appli.GoToPage(PageNames.SetEnvironment);
                        args.Handled = true;
                    }
                }, 0, 600);

                endedSuccessfully = false;
            }

            // if this file exists, then the connect statement failed, warn the user
            var dbKoPath = Path.Combine(TempDir, "db.ko");
            if (endedSuccessfully && File.Exists(dbKoPath) && new FileInfo(dbKoPath).Length > 0) {
                UserCommunication.Notify("Failed to connect to the progress database!<br>Verify / correct the connection info <a href='go'>in the environment page</a> and try again<br><br><i>Also, make sure that the database for the current environment is connected!</i><br><br>Below is the error returned while trying to connect to the database : " + Utils.ReadAndFormatLogToHtml(Path.Combine(TempDir, "db.ko")), MessageImg.MsgRip, "Database connection", "Connection failed", args => {
                    if (args.Link.Equals("go")) {
                        Appli.Appli.GoToPage(PageNames.SetEnvironment);
                        args.Handled = true;
                    }
                }, 10, 600);

                if (NeedDatabaseConnection)
                    endedSuccessfully = false;
            }

            // end of successful/unsuccessful execution action
            if (endedSuccessfully) {
                if (OnExecutionOk != null) {
                    OnExecutionOk(this);
                }
            } else {
                if (OnExecutionFailed != null) {
                    OnExecutionFailed(this);
                }
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

        #endregion


    }

    internal enum ExecutionType {
        CheckSyntax = 0,
        Compile = 1,
        Run = 2,
        Prolint = 3,

        Database = 10,
        Appbuilder = 11,
        Dictionary = 12
    }

    internal class FileToCompile {
        // stores the final path for each file
        public string InputPath { get; set; }
        public string OutputDir { get; set; }
        public string OutputR { get; set; }
        public string OutputLst { get; set; }

        // stores temporary path used during the execution
        public string TempInputPath { get; set; }
        public string TempOutputDir { get; set; }
        public string TempOutputR { get; set; }
        public string TempOutputLst { get; set; }
    }
}
