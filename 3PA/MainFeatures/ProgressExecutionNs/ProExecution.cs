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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using _3PA.Data;
using _3PA.Html;
using _3PA.Lib;
using _3PA.MainFeatures.FilesInfoNs;

// ReSharper disable LocalizableElement

namespace _3PA.MainFeatures.ProgressExecutionNs {

    internal class ProExecution {

        #region public fields

        /// <summary>
        /// The action to execute at the end of the execution
        /// </summary>
        public Action<ProExecution> OnExecutionEnd { private get; set; }

        /// <summary>
        /// Full path to the directory containing all the files needed for the execution
        /// </summary>
        public string TempDir { get; private set; }

        /// <summary>
        /// Full path to the directory used as the working directory to start the prowin process
        /// </summary>
        public string ExecutionDir { get; private set; }

        /// <summary>
        /// Full path to the .p, .w... to compile, run...
        /// </summary>
        public string FullFilePathToExecute { get; private set; }

        /// <summary>
        /// Path to the output .log file (for compilation)
        /// </summary>
        public string LogPath { get; private set; }

        /// <summary>
        /// Path to the output .lst file (for compilation)
        /// </summary>
        public string LstPath { get; private set; }

        /// <summary>
        /// Path to the output .r file (for compilation)
        /// </summary>
        public string DotRPath { get; private set; }

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
        /// Full file path to the output file of Prolint
        /// </summary>
        public string ProlintOutputPath { get; set; }

        #endregion

        #region private fields

        /// <summary>
        /// path to temp ProgressRun.p
        /// </summary>
        private string _runnerPath;

        /// <summary>
        /// Path to the file to compile/run, can either be equal to FullFilePathToExecute, or have the same file name
        /// but be located in the temp directory (if we compile the current file for example)
        /// </summary>
        public string TempFullFilePathToExecute { get; private set; }

        #endregion

        #region constructors and destructor

        /// <summary>
        /// Creates a progress execution environnement, to compile or run the current program
        /// </summary>
        public ProExecution() {
            FullFilePathToExecute = Plug.CurrentFilePath;
        }

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

            // check info
            if (executionType != ExecutionType.Database) {
                if (!Abl.IsCurrentProgressFile()) {
                    UserCommunication.Notify("Can only compile and run progress files!", MessageImg.MsgWarning, "Invalid file type", "Progress files only", 10);
                    return false;
                }
                if (string.IsNullOrEmpty(FullFilePathToExecute) || !File.Exists(FullFilePathToExecute)) {
                    UserCommunication.Notify("Couldn't find the following file :<br>" + FullFilePathToExecute, MessageImg.MsgError, "Execution error", "File not found", 10);
                    return false;
                }
                if (!Config.Instance.GlobalCompilableExtension.Split(',').Contains(Path.GetExtension(FullFilePathToExecute))) {
                    UserCommunication.Notify("Sorry, the file extension " + Path.GetExtension(FullFilePathToExecute).ProgressQuoter() + " isn't a valid extension for this action!<br><i>You can change the list of valid extensions in the settings window</i>", MessageImg.MsgWarning, "Invalid file extension", "Not an executable", 10);
                    return false;
                }
            }
            if (executionType == ExecutionType.Prolint && !File.Exists(Config.FileStartProlint)) {
                UserCommunication.Notify("Couldn't find the interface program for prolint :<br>" + Config.FileStartProlint, MessageImg.MsgError, "Prolint error", "File not found", 10);
                return false;
            }
            if (!File.Exists(ProEnvironment.Current.ProwinPath)) {
                UserCommunication.Notify("The file path to prowin32.exe is incorrect : <br>" + ProEnvironment.Current.ProwinPath + "<br>You must provide a valid path before executing this action<br><i>You can change this path in the settings window</i>", MessageImg.MsgWarning, "Execution error", "Invalid file path", 10);
                return false;
            }

            var noBatchMode = (executionType == ExecutionType.Run || executionType == ExecutionType.Prolint);
            StringBuilder programContent;

            // create unique temporary folder
            TempDir = Path.Combine(Config.FolderTemp, DateTime.Now.ToString("yyMMdd_HHmmssfff"));
            while (Directory.Exists(TempDir)) TempDir += "_";
            if (!Utils.CreateDirectory(TempDir))
                return false;

            // Move context files into the execution dir
            var baseIniPath = "";
            if (File.Exists(ProEnvironment.Current.IniPath)) {
                baseIniPath = Path.Combine(TempDir, "base.ini");
                File.Copy(ProEnvironment.Current.IniPath, baseIniPath);
            }

            var basePfPath = "";
            if (File.Exists(ProEnvironment.Current.GetPfPath())) {
                basePfPath = Path.Combine(TempDir, "base.pf");
                File.Copy(ProEnvironment.Current.GetPfPath(), basePfPath);
            }

            string fileToExecute;
            if (executionType == ExecutionType.Database) {
                // for database extraction, we need to copy the DumpDatabase program
                TempFullFilePathToExecute = "db_" + DateTime.Now.ToString("yyMMdd_HHmmssfff") + ".p";
                File.WriteAllBytes(Path.Combine(TempDir, TempFullFilePathToExecute), DataResources.DumpDatabase);
                fileToExecute = TempFullFilePathToExecute;

            } else {
                // Copy Npp.Text to a temp file to be executed
                TempFullFilePathToExecute = Path.Combine(TempDir, "tmp_" + DateTime.Now.ToString("yyMMdd_HHmmssfff") + (Path.GetExtension(FullFilePathToExecute) ?? ".p"));
                File.WriteAllText(TempFullFilePathToExecute, Npp.Text, Encoding.Default);
                fileToExecute = TempFullFilePathToExecute;
                

                if (executionType == ExecutionType.Prolint) {
                    // prolint, we need to copy the StartProlint program
                    fileToExecute = "run_" + DateTime.Now.ToString("yyMMdd_HHmmssfff") + ".p";
                    ProlintOutputPath = Path.Combine(TempDir, "prolint.log");
                    programContent = new StringBuilder();
                    programContent.AppendLine("&SCOPED-DEFINE PathFileToProlint " + TempFullFilePathToExecute.ProgressQuoter());
                    programContent.AppendLine("&SCOPED-DEFINE PathProlintOutputFile " + ProlintOutputPath.ProgressQuoter());
                    programContent.AppendLine("&SCOPED-DEFINE PathToStartProlintProgram " + Config.FileStartProlint.ProgressQuoter());
                    programContent.AppendLine("&SCOPED-DEFINE UserName " + Config.Instance.UserName.ProgressQuoter());
                    programContent.AppendLine("&SCOPED-DEFINE PathActualFilePath " + FullFilePathToExecute.ProgressQuoter());
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
                    File.WriteAllText(Path.Combine(TempDir, fileToExecute), 
                        File.ReadAllText(Config.FileStartProlint, TextEncodingDetect.GetFileEncoding(Config.FileStartProlint)).Replace(@"/*<inserted_3P_values>*/", programContent.ToString()), 
                        Encoding.Default);
                }
            }

            // set info on the execution
            var baseFileName = Path.GetFileNameWithoutExtension(TempFullFilePathToExecute);
            LogPath = Path.Combine(TempDir, baseFileName + ".log");
            LstPath = Path.Combine(TempDir, baseFileName + ".lst");
            DotRPath = Path.Combine(TempDir, baseFileName + ".r");
            ExecutionType = executionType;
            ExecutionDir = Path.GetDirectoryName(FullFilePathToExecute) ?? TempDir;
            ProgressWin32 = ProEnvironment.Current.ProwinPath;
            if (executionType == ExecutionType.Database)
                ExtractDbOutputPath = Path.Combine(ExecutionDir, ExtractDbOutputPath);

            // prepare the .p runner
            var runnerFileName = DateTime.Now.ToString("yyMMdd_HHmmssfff") + ".p";
            _runnerPath = Path.Combine(TempDir, runnerFileName);

            programContent = new StringBuilder();
            programContent.AppendLine("&SCOPED-DEFINE ExecutionType " + executionType.ToString().ToUpper().ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE ToExecute " + fileToExecute.ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE TempDir " + TempDir.ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE LogFile " + LogPath.ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE LstFile " + LstPath.ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE ExtractDbOutputPath " + ExtractDbOutputPath.ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE propathToUse " + string.Join(",", ProEnvironment.Current.GetProPathDirList).ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE ExtraPf " + ProEnvironment.Current.ExtraPf.ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE BasePfPath " + basePfPath.ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE BaseIniPath " + baseIniPath.ProgressQuoter());
            programContent.Append(Encoding.Default.GetString(DataResources.ProgressRun));

            File.WriteAllText(_runnerPath, programContent.ToString(), Encoding.Default);

            // Parameters
            StringBuilder Params = new StringBuilder();
            Params.Append(" -T " + Path.GetTempPath().Trim('\\').ProgressQuoter());
            if (!noBatchMode && Config.Instance.UseBatchModeToCompile)
                Params.Append(" -b");
            Params.Append(" -p " + _runnerPath.ProgressQuoter());
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
                WorkingDirectory = ExecutionDir
            };
            if (!noBatchMode) {
                pInfo.WindowStyle = ProcessWindowStyle.Hidden;
                pInfo.CreateNoWindow = true;
            }
            Process = new Process {
                StartInfo = pInfo,
                EnableRaisingEvents = true
            };
            Process.Exited += ProcessOnExited;
            Process.Start();
            //UserCommunication.Notify("New process starting...<br><br><b>FileName :</b><br>" + ProEnvironment.Current.ProwinPath + "<br><br><b>Parameters :</b><br>" + ExeParameters + "<br><br><b>Temporary directory :</b><br><a href='" + TempDir + "'>" + TempDir + "</a>");

            // restore splashscreen
            MoveSplashScreenNoError(splashScreenPathMoved, splashScreenPath);

            return true;
        }

        /// <summary>
        /// Called by the process's thread when it is over, execute the ProcessOnExited event
        /// </summary>
        private void ProcessOnExited(object sender, EventArgs eventArgs) {
            if (OnExecutionEnd != null) {
                OnExecutionEnd(this);
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
        CheckSyntax,
        Compile,
        Run,
        Database,
        Prolint
    }
}
