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
using _3PA.MainFeatures.AutoCompletion;

namespace _3PA.MainFeatures.ProgressExecutionNs {

    internal class ProExecution {

        #region public fields

        private event EventHandler<ProcessOnExitEventArgs> OnProcessExited;

        /// <summary>
        /// You should register to this event to know when the button has been pressed (clicked or enter or space)
        /// </summary>
        public event EventHandler<ProcessOnExitEventArgs> ProcessExited {
            add { OnProcessExited += value; }
            remove { OnProcessExited -= value; }
        }

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
        public string ExtractDbOutputPath { get; private set; }

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

        private readonly bool _isCurrentFile;

        #endregion

        #region constructors and destructor

        /// <summary>
        /// Creates a progress execution environnement, to compile or run a program
        /// </summary>
        /// <param name="tempFullFilePathToExecute"></param>
        public ProExecution(string tempFullFilePathToExecute) {
            FullFilePathToExecute = tempFullFilePathToExecute;
            _isCurrentFile = false;
        }

        /// <summary>
        /// Creates a progress execution environnement, to compile or run the current program
        /// </summary>
        public ProExecution() {
            FullFilePathToExecute = Plug.CurrentFilePath;
            _isCurrentFile = true;
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
                if (_isCurrentFile && !Abl.IsCurrentProgressFile()) {
                    UserCommunication.Notify("Can only compile and run progress files!", MessageImg.MsgWarning,
                        "Invalid file type", "Progress files only", 10);
                    return false;
                }
                if (string.IsNullOrEmpty(FullFilePathToExecute) || !File.Exists(FullFilePathToExecute)) {
                    UserCommunication.Notify("Couldn't find the following file :<br>" + FullFilePathToExecute,
                        MessageImg.MsgError, "Execution error", "File not found", 10);
                    return false;
                }
                if (!Config.Instance.GlobalCompilableExtension.Split(',').Contains(Path.GetExtension(FullFilePathToExecute))) {
                    UserCommunication.Notify("Sorry, the file extension " + Path.GetExtension(FullFilePathToExecute).ProgressQuoter() + " isn't a valid extension for this action!<br><i>You can change the list of valid extensions in the settings window</i>", MessageImg.MsgWarning, "Invalid file extension", "Not an executable", 10);
                    return false;
                }
            }
            if (!File.Exists(ProEnvironment.Current.ProwinPath)) {
                UserCommunication.Notify("The file path to prowin32.exe is incorrect : <br>" + ProEnvironment.Current.ProwinPath + "<br>You must provide a valid path before executing this action<br><i>You can change this path in the settings window</i>", MessageImg.MsgWarning, "Execution error", "Invalid file path", 10);
                return false;
            }

            // create unique temporary folder
            TempDir = Path.Combine(Config.FolderTemp, DateTime.Now.ToString("yyMMdd_HHmmssfff"));
            while (Directory.Exists(TempDir)) TempDir += "_";
            try {
                Directory.CreateDirectory(TempDir);
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Permission denied when creating " + TempDir);
                TempDir = "";
                return false;
            }

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

            // If current file, copy Npp.Text to a temp file to be executed
            if (executionType != ExecutionType.Database) {
                if (_isCurrentFile) {
                    TempFullFilePathToExecute = Path.Combine(TempDir, "tmp" + (Path.GetExtension(FullFilePathToExecute) ?? ".p"));
                    File.WriteAllText(TempFullFilePathToExecute, Npp.Text, Encoding.Default);
                } else TempFullFilePathToExecute = FullFilePathToExecute;
            } else {
                // for database extraction, we need the output path and to copy the DumpDatabase program
                TempFullFilePathToExecute = DateTime.Now.ToString("yyMMdd_HHmmssfff_") + ".p";
                File.WriteAllBytes(Path.Combine(TempDir, TempFullFilePathToExecute), DataResources.DumpDatabase);
                ExtractDbOutputPath = Path.Combine(TempDir, DataBase.OutputFileName);
            }

            // set info on the execution
            var baseFileName = Path.GetFileNameWithoutExtension(TempFullFilePathToExecute);
            LogPath = Path.Combine(TempDir, baseFileName + ".log");
            LstPath = Path.Combine(TempDir, baseFileName + ".lst");
            DotRPath = Path.Combine(TempDir, baseFileName + ".r");
            ExecutionType = executionType;
            ExecutionDir = Path.GetDirectoryName(FullFilePathToExecute) ?? TempDir;
            ProgressWin32 = ProEnvironment.Current.ProwinPath;

            // prepare the .p runner
            var runnerFileName = DateTime.Now.ToString("yyMMdd_HHmmssfff") + ".p";
            _runnerPath = Path.Combine(TempDir, runnerFileName);

            var programContent = new StringBuilder();
            programContent.AppendLine("&SCOPED-DEFINE ExecutionType " + executionType.ToString().ToUpper().ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE ToExecute " + TempFullFilePathToExecute.ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE TempDir " + TempDir.ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE LogFile " + LogPath.ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE LstFile " + LstPath.ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE ExtractDbOutputPath " + ExtractDbOutputPath.ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE propathToUse " + ((executionType != ExecutionType.Database) ? string.Join(",", ProEnvironment.Current.GetProPathDirList) : TempDir).ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE ExtraPf " + ProEnvironment.Current.ExtraPf.ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE BasePfPath " + basePfPath.ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE BaseIniPath " + baseIniPath.ProgressQuoter());
            programContent.Append(Encoding.Default.GetString(DataResources.ProgressRun));

            File.WriteAllText(_runnerPath, programContent.ToString(), Encoding.Default);

            // Parameters
            StringBuilder Params = new StringBuilder();
            if (executionType != ExecutionType.Run)
                Params.Append(" -b");
            if (!string.IsNullOrWhiteSpace(ProEnvironment.Current.CmdLineParameters))
                Params.Append(" " + ProEnvironment.Current.CmdLineParameters.Trim());
            //Params.Append(" -cpinternal ISO8859-1");
            //Params.Append(" -cpstream ISO8859-1");
            Params.Append(" -p " + _runnerPath.ProgressQuoter());
            ExeParameters = Params.ToString();

            // Start a process
            Process = new Process {
                StartInfo = {
                    WindowStyle = (executionType != ExecutionType.Run) ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
                    CreateNoWindow = (executionType != ExecutionType.Run),
                    FileName = ProEnvironment.Current.ProwinPath,
                    Arguments = ExeParameters,
                    WorkingDirectory = ExecutionDir
                },
                EnableRaisingEvents = true
            };
            Process.Exited += ProcessOnExited;
            Process.Start();
            //UserCommunication.Notify("New process starting...<br><br><b>FileName :</b><br>" + ProEnvironment.Current.ProwinPath + "<br><br><b>Parameters :</b><br>" + ExeParameters + "<br><br><b>Temporary directory :</b><br><a href='" + TempDir + "'>" + TempDir + "</a>");

            return true;
        }

        /// <summary>
        /// Called by the process's thread when it is over, execute the ProcessOnExited event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void ProcessOnExited(object sender, EventArgs eventArgs) {
            if (OnProcessExited != null) {
                OnProcessExited(this, new ProcessOnExitEventArgs(this));
                Process.Close();
            }
        }

        #endregion

    }

    internal class ProcessOnExitEventArgs : EventArgs {
        public ProExecution ProgressExecution;
        public ProcessOnExitEventArgs(ProExecution progressExecution) {
            ProgressExecution = progressExecution;
        }
    }

    internal enum ExecutionType {
        CheckSyntax,
        Compile,
        Run,
        Database
    }
}
