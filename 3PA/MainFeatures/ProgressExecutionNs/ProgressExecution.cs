#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProgressExecution.cs) is part of 3P.
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
    public class ProgressExecution {

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
        /// Directory where the file will be moved to after the compilation
        /// </summary>
        public string CompilationDir { get; private set; }

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
        public ProgressExecution(string tempFullFilePathToExecute) {
            FullFilePathToExecute = tempFullFilePathToExecute;
            _isCurrentFile = false;
        }

        /// <summary>
        /// Creates a progress execution environnement, to compile or run the current program
        /// </summary>
        public ProgressExecution() {
            FullFilePathToExecute = Plug.CurrentFilePath;
            _isCurrentFile = true;
        }

        /// <summary>
        /// Deletes temp directory and everything in it
        /// </summary>
        ~ProgressExecution()
        {
            try
            {
                if (Process != null)
                    Process.Close();
                Utils.DeleteDirectory(ExecutionDir, true);
            }
            catch (Exception)
            {
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
            if (!File.Exists(ProgressEnv.Current.ProwinPath)) {
                UserCommunication.Notify("The file path to prowin32.exe is incorrect : <br>" + ProgressEnv.Current.ProwinPath + "<br>You must provide a valid path before executing this action<br><i>You can change this path in the settings window</i>", MessageImg.MsgWarning, "Execution error", "Invalid file path", 10);
                return false;
            }

            // create unique execution folder
            ExecutionDir = Path.Combine(Plug.TempDir, DateTime.Now.ToString("yyMMdd_HHmmssfff"));
            while (Directory.Exists(ExecutionDir)) ExecutionDir += "_";
            try {
                Directory.CreateDirectory(ExecutionDir);
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Permission denied when creating " + ExecutionDir);
                ExecutionDir = "";
                return false;
            }

            // Move context files into the execution dir
            if (File.Exists(ProgressEnv.Current.GetCurrentPfPath()))
                File.Copy(ProgressEnv.Current.GetCurrentPfPath(), Path.Combine(ExecutionDir, "base.pf"));

            if (!string.IsNullOrEmpty(ProgressEnv.Current.DataBaseConnection))
                File.WriteAllText(Path.Combine(ExecutionDir, "extra.pf"), ProgressEnv.Current.DataBaseConnection, Encoding.Default);

            if (File.Exists(ProgressEnv.Current.IniPath))
                File.Copy(ProgressEnv.Current.IniPath, Path.Combine(ExecutionDir, "base.ini"));

            // If current file, copy Npp.Text to a temp file to be executed
            var dumpDbProgramName = "";
            if (executionType != ExecutionType.Database) {
                if (_isCurrentFile) {
                    TempFullFilePathToExecute = Path.Combine(ExecutionDir, (Path.GetFileName(FullFilePathToExecute) ?? "gg"));
                    File.WriteAllText(TempFullFilePathToExecute, Npp.Text, Npp.Encoding);
                } else TempFullFilePathToExecute = FullFilePathToExecute;
            } else {
                // for database extraction, we need the output path and to copy the DumpDatabase program
                dumpDbProgramName = DateTime.Now.ToString("yyMMdd_HHmmssfff_") + ".p";
                File.WriteAllBytes(Path.Combine(ExecutionDir, dumpDbProgramName), DataResources.DumpDatabase);
                ExtractDbOutputPath = Path.Combine(ExecutionDir, DataBase.OutputFileName);
            }

            // set info on the execution
            var baseFileName = Path.GetFileNameWithoutExtension(TempFullFilePathToExecute);
            if (executionType == ExecutionType.Database) baseFileName = "dump";
            LogPath = Path.Combine(ExecutionDir, baseFileName + ".log");
            LstPath = Path.Combine(ExecutionDir, baseFileName + ".lst");
            DotRPath = Path.Combine(ExecutionDir, baseFileName + ".r");
            ExecutionType = executionType;

            // prepare the preproc variable of the .p runner
            var programContent = new StringBuilder();
            programContent.AppendLine("&SCOPED-DEFINE ExecutionType " + executionType.ToString().ToUpper().ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE ToCompile " + TempFullFilePathToExecute.ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE CompilePath " + ExecutionDir.ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE LogFile " + LogPath.ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE LstFile " + LstPath.ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE ExtractDbOutputPath " + ExtractDbOutputPath.ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE propathToUse " + (ExecutionDir + "," + ProgressEnv.Current.ProPath).ProgressQuoter());
            programContent.AppendLine("&SCOPED-DEFINE dumbDataBaseProgram " + dumpDbProgramName.ProgressQuoter());
            programContent.Append(Encoding.Default.GetString(DataResources.ProgressRun));

            // progress runner
            var runnerFileName = DateTime.Now.ToString("yyMMdd_HHmmssfff") + ".p";
            _runnerPath = Path.Combine(ExecutionDir, runnerFileName);
            File.WriteAllText(_runnerPath, programContent.ToString(),Encoding.Default);

            // misc
            ProgressWin32 = ProgressEnv.Current.ProwinPath;
            CompilationDir = ProgressEnv.Current.BaseCompilationPath; //TODO : compilationPath!

            // Parameters
            StringBuilder Params = new StringBuilder();
            //if (executionType != ExecutionType.Run)
            //    Params.Append(" -b");
            if (!string.IsNullOrWhiteSpace(ProgressEnv.Current.CmdLineParameters))
                Params.Append(" " + ProgressEnv.Current.CmdLineParameters.Trim());
            if (File.Exists(Path.Combine(ExecutionDir, "base.ini")))
                Params.Append(" -ini " + ("base.ini").ProgressQuoter());
            //Params.Append(" -cpinternal ISO8859-1");
            //Params.Append(" -cpstream ISO8859-1");
            //Params.Append(" -inp 20000");  /* Max char per instruction */
            //Params.Append(" -tok 2048");  /* Max token per instruction    */
            Params.Append(" -p " + Path.Combine(ExecutionDir, runnerFileName).ProgressQuoter());
            ExeParameters = Params.ToString();

            // Start a process
            Process = new Process {
                StartInfo = {
                    //WindowStyle = (executionType != ExecutionType.Run) ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
                    //CreateNoWindow = (executionType != ExecutionType.Run),
                    FileName = ProgressEnv.Current.ProwinPath,
                    Arguments = ExeParameters,
                    WorkingDirectory = ExecutionDir
                },
                EnableRaisingEvents = true
            };
            Process.Exited += ProcessOnExited;
            Process.Start();
            //UserCommunication.Notify("New process starting...<br><br><b>FileName :</b><br>" + ProgressEnv.Current.ProwinPath + "<br><br><b>Parameters :</b><br>" + ExeParameters + "<br><br><b>Execution directory :</b><br><a href='" + ExecutionDir + "'>" + ExecutionDir + "</a>");

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

    public class ProcessOnExitEventArgs : EventArgs {
        public ProgressExecution ProgressExecution;
        public ProcessOnExitEventArgs(ProgressExecution progressExecution) {
            ProgressExecution = progressExecution;
        }
    }

    public enum ExecutionType {
        CheckSyntax,
        Compile,
        Run,
        Database
    }
}
