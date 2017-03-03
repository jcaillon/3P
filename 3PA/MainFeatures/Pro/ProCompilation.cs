#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProCompilation.cs) is part of 3P.
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
using System.Threading;
using _3PA.Lib;

namespace _3PA.MainFeatures.Pro {

    /// <summary>
    /// Class used for the mass compiler
    /// </summary>
    internal class ProCompilation {

        #region public fields

        #region options

        /// <summary>
        /// Is the compilation mono process?
        /// </summary>
        public bool MonoProcess { get; set; }

        /// <summary>
        /// Set the nmber of process to be used for each core
        /// </summary>
        public int NumberOfProcessesPerCore { get; set; }

        /// <summary>
        /// true to only generate r code during the compilation
        /// </summary>
        public bool RFilesOnly { get; set; }

        #endregion

        // total number of files being compiled
        public long NbFilesToCompile { get; private set; }

        // total number of processes used
        public int NumberOfProcesses { get; private set; }

        // total number of processes finished ok
        public int NumberOfProcessesEndedOk { get; private set; }

        // has the compilation been canceled / processes killed?
        public bool HasBeenCancelled { get; private set; }

        public event Action OnCompilationEnd;

        public string ExecutionTime { get; private set; }

        /// <summary>
        /// After the compilation, each file is moved to its destination folder (distant or source folder)
        /// This list keeps tracks of the moved files and the success of the move command
        /// </summary>
        public List<FileToDeploy> TransferedFiles { get; private set; }

        /// <summary>
        /// After the compilation, stores each compilation errors found here
        /// </summary>
        public List<FileError> ErrorsList { get; private set; }

        /// <summary>
        /// Returns true if the compilation is done (the move of files can still be on going tho)
        /// </summary>
        public bool CompilationDone {
            get { return _processesRunning == 0; }
        }

        public bool DeploymentDone;

        // remember the time when the compilation started
        public DateTime StartingTime { get; private set; }

        #endregion

        #region private fields

        // list of all the started processes
        private List<CompilationProcess> _listOfCompilationProcesses = new List<CompilationProcess>();

        // total number of processes still running
        private int _processesRunning;

        private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private float _deployPercentage;

        private bool _hasBeenKilled;

        #endregion

        #region public methods

        /// <summary>
        /// Compiles the list of files given
        /// </summary>
        public bool CompileFiles(HashSet<string> filesToCompile) {

            // now we do a list of those files, sorted from the biggest (in size) to the smallest file
            var sizeFileList = new List<ProCompilationFile>();
            foreach (var filePath in filesToCompile) {
                var fileInfo = new FileInfo(filePath);
                sizeFileList.Add(new ProCompilationFile { Path = filePath, Size = fileInfo.Length });
            }
            sizeFileList.Sort((file1, file2) => file2.Size.CompareTo(file1.Size));

            // we want to dispatch all thoses files in a fair way among the Prowin processes we will create...
            NumberOfProcesses = MonoProcess ? 1 : NumberOfProcessesPerCore * Environment.ProcessorCount;
            _listOfCompilationProcesses.Clear();
            var currentProcess = 0;
            foreach (var file in sizeFileList) {
                // create a new process when needed
                if (currentProcess >= _listOfCompilationProcesses.Count)
                    _listOfCompilationProcesses.Add(new CompilationProcess());

                // assign the file to the current process
                _listOfCompilationProcesses[currentProcess].FilesToCompile.Add(new FileToCompile(file.Path));

                // we will assign the next file to the next process...
                currentProcess++;
                if (currentProcess == NumberOfProcesses)
                    currentProcess = 0;
            }

            // init
            NbFilesToCompile = filesToCompile.Count;
            StartingTime = DateTime.Now;
            _processesRunning = _listOfCompilationProcesses.Count;
            NumberOfProcesses = _listOfCompilationProcesses.Count;
            NumberOfProcessesEndedOk = 0;
            TransferedFiles = new List<FileToDeploy>();
            ErrorsList = new List<FileError>();

            // lets start the compilation on each process
            foreach (var compilationProcess in _listOfCompilationProcesses) {

                // launch the compile process
                compilationProcess.ProExecutionObject = new ProExecution {
                    ListToCompile = compilationProcess.FilesToCompile,
                    NeedDatabaseConnection = true,
                    NoBatch = true,
                    OnExecutionEnd = OnExecutionEnd,
                    OnExecutionOk = OnExecutionOk,
                    OnExecutionFailed = OnExecutionFailed
                };
                if (RFilesOnly)
                    compilationProcess.ProExecutionObject.ProEnv.CompileWithListing = false;
                if (!compilationProcess.ProExecutionObject.Do(ExecutionType.Compile))
                    return false;
            }
            return true;
        }


        /// <summary>
        /// Use this method to get the overall progression of the compilation (from 0 to 100)
        /// </summary>
        public float GetOverallProgression() {

            // if the compilation is over, we need to dipslay the progression of the files being moved...
            if (CompilationDone)
                return _deployPercentage;

            // else we find the total of files that have already been compiled by ready the size of compilation.progress files...
            long nbFilesDone = 0;
            foreach (var compilationProcess in _listOfCompilationProcesses) {
                if (File.Exists(compilationProcess.ProExecutionObject.ProgressionFilePath))
                    nbFilesDone += (new FileInfo(compilationProcess.ProExecutionObject.ProgressionFilePath)).Length;
            }
            return (float) nbFilesDone / NbFilesToCompile * 100;
        }

        /// <summary>
        /// Get the time elapsed since the beggining of the compilation in a human readable format
        /// </summary>
        public string GetElapsedTime() {
            return Utils.ConvertToHumanTime(TimeSpan.FromMilliseconds(DateTime.Now.Subtract(StartingTime).TotalMilliseconds));
        }

        /// <summary>
        /// This method "cancel" the compilation by killing the associated processes
        /// </summary>
        public void CancelCompilation() {
            HasBeenCancelled = true;
            KillProcesses();
        }

        public void KillProcesses() {
            foreach (var compilationProcess in _listOfCompilationProcesses.Where(compilationProcess => compilationProcess.ProExecutionObject != null)) {
                compilationProcess.ProExecutionObject.KillProcess();
            }
            _processesRunning = 0;
            EndOfCompilation(_listOfCompilationProcesses.First().ProExecutionObject);
            _hasBeenKilled = true;
        }

        /// <summary>
        /// returns the list of the processes that have been started
        /// </summary>
        public List<FileToCompile> GetListOfFileToCompile {
            get { return (_listOfCompilationProcesses ?? new List<CompilationProcess>()).SelectMany(compProcess => compProcess.ProExecutionObject.ListToCompile).ToList(); }
        }

        /// <summary>
        /// Returns true if at least one process failed because of a failed database connection
        /// and one of the error was the error number 748 (lack of ressources) 
        /// this error is caused by too much connection on the same database (too much processes started!)
        /// </summary>
        /// <returns></returns>
        public bool CompilationFailedOnMaxUser() {
            return _listOfCompilationProcesses.Any(compilationProcess => compilationProcess.ProExecutionObject.ConnectionFailed && Utils.ReadAllText(compilationProcess.ProExecutionObject.DatabaseConnectionLog).Contains("(748)"));
        }


        /// <summary>
        /// Allows to format a small text to explain the errors found in a file and the generated files...
        /// </summary>
        public static string FormatCompilationResult(string sourceFilePath, List<FileError> listErrorFiles, List<FileToDeploy> listDeployedFiles) {

            var line = new StringBuilder();
            var nbErrors = 0;

            line.Append("<div style='padding-bottom: 5px;'><b>" + string.Format("<a class='SubTextColor' href='{0}'>{1}</a>", sourceFilePath, Path.GetFileName(sourceFilePath)) + "</b> in " + Path.GetDirectoryName(sourceFilePath).ToHtmlLink() + "</div>");

            if (listErrorFiles != null)
                foreach (var fileError in listErrorFiles) {
                    nbErrors += fileError.Level > ErrorLevel.StrongWarning ? 1 : 0;
                    line.Append("<div style='padding-left: 10px'>" + "<img src='" + (fileError.Level > ErrorLevel.StrongWarning ? "MsgError" : "MsgWarning") + "' height='15px'>" + (!fileError.CompiledFilePath.Equals(fileError.SourcePath) ? "in " + string.Format("<a class='SubTextColor' href='{0}'>{1}</a>", fileError.SourcePath, Path.GetFileName(fileError.SourcePath)) + ", " : "") + (fileError.SourcePath + "|" + fileError.Line).ToHtmlLink("line " + (fileError.Line + 1)) + " (n°" + fileError.ErrorNumber + ") " + (fileError.Times > 0 ? "(x" + fileError.Times + ") " : "") + fileError.Message + "</div>");
                }

            if (listDeployedFiles != null)
                foreach (var file in listDeployedFiles) {
                    var ext = (Path.GetExtension(file.To) ?? "").Replace(".", "");
                    var transferMsg = file.DeployType == DeployType.Move ? "" : "(" + file.DeployType + ") ";
                    if (file.IsOk && (nbErrors == 0 || !ext.Equals("r"))) {
                        line.Append("<div style='padding-left: 10px'>" + "<img src='" + Utils.GetExtensionImage(ext) + "' height='15px'>" + transferMsg + (ext.EqualsCi("lst") ? file.To.ToHtmlLink() : Path.GetDirectoryName(file.To).ToHtmlLink(file.To)) + "</div>");
                    } else if (nbErrors == 0) {
                        line.Append("<div style='padding-left: 10px'>" + "<img src='MsgError' height='15px'>Transfer error " + transferMsg + Path.GetDirectoryName(file.To).ToHtmlLink(file.To) + "</div>");
                    }
                }

            return line.ToString();
        }

        #endregion

        #region private methods

        /// <summary>
        /// This method is executed when the overall compilation is over and allows to do more treatments
        /// </summary>
        private void EndOfCompilation(ProExecution obj) {

            // only do stuff we have reached the last running process
            if (_processesRunning > 0 || _hasBeenKilled)
                return;

            // everything ended ok, we do postprocess actions
            if (NumberOfProcesses == NumberOfProcessesEndedOk) {

                // we need to transfer all the files... (keep only distinct target files)
                foreach (var compilationProcess in _listOfCompilationProcesses) {
                    TransferedFiles.AddRange(compilationProcess.ProExecutionObject.CreateListOfFilesToDeploy());
                }
                TransferedFiles = obj.ProEnv.Deployer.DeployFiles(TransferedFiles, f => _deployPercentage = f);

                // Read all the log files stores the errors
                foreach (var compilationProcess in _listOfCompilationProcesses) {
                    var errorList = compilationProcess.ProExecutionObject.LoadErrorLog();
                    foreach (var keyValue in errorList) {
                        ErrorsList.AddRange(keyValue.Value);
                    }
                }

                DeploymentDone = true;
            }

            ExecutionTime = GetElapsedTime();

            if (OnCompilationEnd != null)
                OnCompilationEnd();
        }

        /// <summary>
        /// Called when a process has finished
        /// </summary>
        private void OnExecutionEnd(ProExecution lastExecution) {
            if (_lock.TryEnterWriteLock(500)) {
                try {
                    _processesRunning--;
                } finally {
                    _lock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Called when a process has finished successfully
        /// </summary>
        private void OnExecutionOk(ProExecution obj) {
            if (_lock.TryEnterWriteLock(500)) {
                try {
                    NumberOfProcessesEndedOk++;
                    EndOfCompilation(obj);
                } finally {
                    _lock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Called when a process has finished UNsuccessfully
        /// </summary>
        private void OnExecutionFailed(ProExecution obj) {
            if (_lock.TryEnterWriteLock(500)) {
                try {
                    // we kill all the processes we don't want to do anything more...
                    KillProcesses();
                } finally {
                    _lock.ExitWriteLock();
                }
            }
        }

        #endregion

        #region internal class

        private struct ProCompilationFile {
            public string Path { get; set; }
            public long Size { get; set; }
        }

        internal class CompilationProcess {
            public List<FileToCompile> FilesToCompile = new List<FileToCompile>();
            public ProExecution ProExecutionObject;
        }

        #endregion

    }
}
