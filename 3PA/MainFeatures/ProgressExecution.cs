using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamuiFramework.Forms;
using _3PA.Data;

namespace _3PA.MainFeatures
{
    public class ProgressExecution {
        /// <summary>
        /// Full path to the directory containing all the files needed for the execution
        /// </summary>
        public string ExecutionDir { get; private set; }

        /// <summary>
        /// Full path to the .p file to be executed
        /// </summary>
        public string RunnerPath { get; private set; }

        /// <summary>
        /// Full path to the .p, .w... to compile, run..., copied in the temp execution directory
        /// </summary>
        private string _tempFullFilePathToExecute;

        /// <summary>
        /// Full path to the .p, .w... to compile, run...
        /// </summary>
        private readonly string _fullFilePathToExecute;

        private readonly bool _isCurrentFile;

        public string LogPath { get; private set; }
        public string LstPath { get; private set; }
        public string DotRPath { get; private set; }
        public ExecutionType ExecutionType { get; private set; }

        /// <summary>
        /// Creates a progress execution environnement, to compile or run a program
        /// </summary>
        /// <param name="fullFilePathToExecute"></param>
        public ProgressExecution(string fullFilePathToExecute) {
            _fullFilePathToExecute = fullFilePathToExecute;
            _isCurrentFile = false;
        }

        /// <summary>
        /// Creates a progress execution environnement, to compile or run the current program
        /// </summary>
        public ProgressExecution() {
            _fullFilePathToExecute = Npp.GetCurrentFilePath();
            _isCurrentFile = true;
        }

        /// <summary>
        /// allows to prepare the execution environnement by creating a unique temp foler
        /// and copying every critical files into it
        /// </summary>
        /// <returns></returns>
        private bool Prepare(ExecutionType executionType) {
            // check info
            if (executionType != ExecutionType.Database) {
                if (_isCurrentFile) {
                    if (!Abl.IsCurrentProgressFile()) {
                        UserCommunication.Notify("Can only compile and run progress files!", MessageImage.WarningShield,
                            "Invalid file type", duration: 10);
                        return false;
                    }
                } else {
                    if (string.IsNullOrEmpty(_fullFilePathToExecute) || !File.Exists(_fullFilePathToExecute)) {
                        UserCommunication.Notify("Couldn't find the following file :<br>" + _fullFilePathToExecute,
                        MessageImage.Error, "Execution error", duration: 10);
                        return false;
                    }
                }
            }

            // create unique execution folder
            ExecutionDir = Path.Combine(Plug.TempDir, DateTime.Now.ToString("yy-MM-dd_HH-mm-ss-fff"));
            while (Directory.Exists(ExecutionDir)) ExecutionDir += "_";
            try
            {
                Directory.CreateDirectory(ExecutionDir);
            }
            catch (Exception e)
            {
                ErrorHandler.ShowErrors(e, "Permission denied when creating " + ExecutionDir);
                ExecutionDir = "";
                return false;
            }

            // Move context files into the execution dir
            if (File.Exists(ProgressEnv.Current.GetCurrentPfPath()))
                File.Copy(ProgressEnv.Current.GetCurrentPfPath(), Path.Combine(ExecutionDir, "base.pf"));

            if (!string.IsNullOrEmpty(ProgressEnv.Current.DataBaseConnection))
                File.WriteAllText(Path.Combine(ExecutionDir, "extra.pf"), ProgressEnv.Current.DataBaseConnection);

            if (File.Exists(ProgressEnv.Current.IniPath))
                File.Copy(ProgressEnv.Current.IniPath, Path.Combine(ExecutionDir, "base.ini"));

            if (!string.IsNullOrEmpty(ProgressEnv.Current.DataBaseConnection))
                File.WriteAllText(Path.Combine(ExecutionDir, "extra.pf"), ProgressEnv.Current.DataBaseConnection);

            if (_isCurrentFile) {
                _tempFullFilePathToExecute = Path.Combine(ExecutionDir, Path.GetFileName(_fullFilePathToExecute) ?? "");
                File.WriteAllText(_tempFullFilePathToExecute, Npp.Text);
            }
            else _tempFullFilePathToExecute = _fullFilePathToExecute;

            // set info on the execution
            var baseFileName = Path.GetFileNameWithoutExtension(_fullFilePathToExecute);
            LogPath = Path.Combine(ExecutionDir, baseFileName + ".log");
            LstPath = Path.Combine(ExecutionDir, baseFileName + ".lst");
            DotRPath = Path.Combine(ExecutionDir, baseFileName + ".r");
            ExecutionType = executionType;

            // prepare the preproc variable of the .p runner
            var programContent = new StringBuilder();
            programContent.AppendLine("&SCOPED-DEFINE ExecutionType \"" + executionType.ToString().ToUpper() + "\"");
            programContent.AppendLine("&SCOPED-DEFINE ToCompile \"" + _tempFullFilePathToExecute + "\"");
            programContent.AppendLine("&SCOPED-DEFINE CompilePath \"" + DotRPath + "\"");
            programContent.AppendLine("&SCOPED-DEFINE LogFile \"" + LogPath + "\"");
            programContent.AppendLine("&SCOPED-DEFINE LstFile \"" + LstPath + "\"");
            programContent.AppendLine("&SCOPED-DEFINE ExtractDbOutputPath \"" + executionType.ToString().ToUpper() + "\"");
            programContent.AppendLine("&SCOPED-DEFINE propathToUse \"" + ProgressEnv.Current.ProPath + "\"");
            programContent.Append(DataResources.ProgressRun);

            // progress runner
            RunnerPath = Path.Combine(ExecutionDir, DateTime.Now.ToString("yy-MM-dd_HH-mm-ss-fff") + ".p");

            File.WriteAllText(RunnerPath, programContent.ToString());

            return true;
        }

        
    }

    public enum InternalExecution {
        ExtractDatabase,

    }

    public enum ExecutionType {
        Compile,
        Run,
        Database
    }
}
