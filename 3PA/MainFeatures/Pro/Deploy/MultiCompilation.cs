#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (MultiCompilation.cs) is part of 3P.
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
using System.Linq;
using System.Threading;
using _3PA.Lib;

namespace _3PA.MainFeatures.Pro.Deploy {

    /// <summary>
    /// This class starts several ProExecution to compile a lot of progress files in parallel 
    /// It starts multiple processes
    /// </summary>
    internal class MultiCompilation {

        #region Events

        /// <summary>
        /// Event fired when the compilation ends
        /// </summary>
        public event Action<MultiCompilation> OnCompilationEnd;

        /// <summary>
        /// The action to execute at the end of the compilation if it went well
        /// - the list of all the files that needed to be compiled,
        /// - the errors for each file compiled (if any)
        /// - the list of all the deployments needed for the files compiled (move the .r but also .dbg and so on...)
        /// </summary>
        public event Action<MultiCompilation, List<FileToCompile>, List<FileToDeploy>> OnCompilationOk;

        /// <summary>
        /// Event fired when the compilation ends
        /// </summary>
        public event Action<MultiCompilation> OnCompilationFailed;

        #endregion

        #region Options

        /// <summary>
        /// Is the compilation mono process?
        /// </summary>
        public bool MonoProcess { get; set; }

        /// <summary>
        /// Set the number of process to be used for each core
        /// </summary>
        public int NumberOfProcessesPerCore { get; set; }

        /// <summary>
        /// true to only generate r code during the compilation
        /// </summary>
        public bool RFilesOnly { get; set; }

        /// <summary>
        /// If true, don't actually do anything, just test it
        /// </summary>
        public bool IsTestMode { get; set; }

        /// <summary>
        /// When true, we activate the log just before compiling with FileId active + we generate a file that list referenced table in the .r
        /// </summary>
        public bool IsAnalysisMode { get; set; }

        /// <summary>
        /// Pro environment to use
        /// </summary>
        public ProEnvironment.ProEnvironmentObject ProEnv { get; set; }

        #endregion

        #region public fields

        /// <summary>
        /// total number of files being compiled
        /// </summary>
        public int NbFilesToCompile { get; private set; }

        /// <summary>
        /// has the compilation been canceled / processes killed?
        /// </summary>
        public bool HasBeenCancelled { get; private set; }

        /// <summary>
        /// Human readable amount of time needed for this compilation
        /// </summary>
        public string TotalCompilationTime { get; private set; }

        /// <summary>
        /// remember the time when the compilation started
        /// </summary>
        public DateTime StartingTime { get; private set; }

        /// <summary>
        /// Returns true if the compilation is done correctly
        /// </summary>
        public bool CompilationDone {
            get { return _processesRunning == 0 && !_hasBeenKilled; }
        }

        /// <summary>
        /// total number of processes used
        /// </summary>
        public int TotalNumberOfProcesses {
            get { return _processes.Count; }
        }

        /// <summary>
        /// Returns the number of processes currently running
        /// </summary>
        public int CurrentNumberOfProcesses { get { return _processesRunning; } }

        /// <summary>
        /// Returns true if at least one process failed because of a failed database connection
        /// and one of the error was the error number 748 (lack of resources) 
        /// this error is caused by too much connection on the same database (too much processes started!)
        /// </summary>
        public bool CompilationFailedOnMaxUser {
            get { return _processes.Any(proc => proc.ConnectionFailed && proc.DbConnectionFailedOnMaxUser); }
        }

        /// <summary>
        /// Use this method to get the overall progression of the compilation (from 0 to 100)
        /// </summary>
        public float CompilationProgression {
            get {
                if (NbFilesToCompile == 0)
                    return 0;
                if (CompilationDone)
                    return 100;
                return (float) NumberOfFilesTreated / NbFilesToCompile * 100;
            }
        }

        /// <summary>
        /// total of files that have already been compiled
        /// </summary>
        public int NumberOfFilesTreated {
            get {
                if (IsTestMode)
                    return NbFilesToCompile;
                int nbFilesDone = 0;
                foreach (var proc in _processes) {
                    nbFilesDone += proc.NbFilesTreated;
                }
                return nbFilesDone;
            }
        }

        /// <summary>
        /// List of all the files that needed to be compiled (should be used after the execution)
        /// </summary>
        public List<FileToCompile> ListFilesToCompile {
            get { return _listFilesToCompile; }
        }

        /// <summary>
        /// List of all the files that need to be deployed after the compilation (should be used after the execution)
        /// </summary>
        public List<FileToDeploy> ListFilesToDeploy {
            get { return _listFilesToDeploy; }
        }

        #endregion

        #region private fields

        private static object _lock = new object();

        // list of all the started processes
        private List<ProExecutionCompile> _processes = new List<ProExecutionCompile>();

        // total number of processes still running
        private int _processesRunning = -1;
        
        private bool _hasBeenKilled;

        private List<FileToCompile> _listFilesToCompile = new List<FileToCompile>();

        private List<FileToDeploy> _listFilesToDeploy = new List<FileToDeploy>();

        /// <summary>
        /// Get the time elapsed since the beginning of the compilation in a human readable format
        /// </summary>
        private string ElapsedTime {
            get { return Utils.ConvertToHumanTime(TimeSpan.FromMilliseconds(DateTime.Now.Subtract(StartingTime).TotalMilliseconds)); }
        }

        #endregion

        #region Life and death

        /// <summary>
        /// Uses the current environment
        /// </summary>
        public MultiCompilation() : this(null) {}

        public MultiCompilation(ProEnvironment.ProEnvironmentObject proEnv) {
            ProEnv = proEnv == null ? new ProEnvironment.ProEnvironmentObject(ProEnvironment.Current) : proEnv;
            StartingTime = DateTime.Now;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Compiles the list of files given
        /// </summary>
        public bool CompileFiles(List<FileToCompile> filesToCompile) {

            if (filesToCompile == null || filesToCompile.Count == 0) {
                EndOfCompilation();
                return true;
            }

            // init
            StartingTime = DateTime.Now;
            NbFilesToCompile = filesToCompile.Count;

            // now we do a list of those files, sorted from the biggest (in size) to the smallest file
            filesToCompile.Sort((file1, file2) => file2.Size.CompareTo(file1.Size));

            // we want to dispatch all those files in a fair way among the Prowin processes we will create...
            var numberOfProcesses = MonoProcess ? 1 : Math.Min(NumberOfProcessesPerCore, 1) * Environment.ProcessorCount;

            var fileLists = new List<List<FileToCompile>>();
            var currentProcess = 0;
            foreach (var file in filesToCompile) {
                // create a new process when needed
                if (currentProcess >= fileLists.Count)
                    fileLists.Add(new List<FileToCompile>());

                // assign the file to the current process
                fileLists[currentProcess].Add(file);

                // we will assign the next file to the next process...
                currentProcess++;
                if (currentProcess == numberOfProcesses)
                    currentProcess = 0;
            }

            _processesRunning = fileLists.Count;

            // init the compilation on each process
            _processes.Clear();
            for (int i = 0; i < fileLists.Count; i++) {
                var exec = new ProExecutionCompile(ProEnv) {
                    Files = fileLists[i],
                    NeedDatabaseConnection = true,
                    NoBatch = true,
                    IsTestMode = IsTestMode,
                    IsAnalysisMode = IsAnalysisMode
                };
                exec.OnExecutionOk += OnExecutionOk;
                exec.OnExecutionFailed += OnExecutionFailed;
                exec.OnCompilationOk += OnExecCompilationOk;

                if (RFilesOnly) {
                    exec.CompileWithDebugList = false;
                    exec.CompileWithXref = false;
                    exec.CompileWithListing = false;
                }

                _processes.Add(exec);
            }

            // launch the compile process
            return _processes.All(exec => exec.Start());
        }

        /// <summary>
        /// This method "cancel" the compilation by killing the associated processes
        /// </summary>
        public void CancelCompilation() {
            HasBeenCancelled = true;
            KillProcesses();
        }

        /// <summary>
        /// Kill all the processes that were started
        /// </summary>
        public void KillProcesses() {
            _hasBeenKilled = true;
            if (_processesRunning > 0) {
                foreach (var proc in _processes.Where(proc => proc != null)) {
                    proc.KillProcess();
                }
            }
            _processesRunning = 0;
        }

        /// <summary>
        /// returns the list of all the files that are being compiled
        /// </summary>
        public List<FileToCompile> GetListOfFileToCompile {
            get {
                return (_processes ?? new List<ProExecutionCompile>()).SelectMany(proc => {
                    return proc.Files != null && proc.NbFilesTreated > 0 ? proc.Files.GetRange(0, proc.NbFilesTreated) : new List<FileToCompile>();
                }).ToList();
            }
        }

        /// <summary>
        /// Clean the temporary directories created for the compilation (call this after the deployment)
        /// </summary>
        public void Clean() {
            foreach (var process in _processes) {
                process.Clean();
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// This method is executed when the overall compilation is over and allows to do more treatments
        /// </summary>
        private void EndOfCompilation() {
            // only do stuff we have reached the last running process
            if (_processesRunning > 0)
                return;

            TotalCompilationTime = ElapsedTime;

            try {
                if (!_hasBeenKilled) {
                    if (OnCompilationOk != null)
                        OnCompilationOk(this, _listFilesToCompile, _listFilesToDeploy);
                } else {
                    if (OnCompilationFailed != null)
                        OnCompilationFailed(this);
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e);
            }

            try { 
                if (OnCompilationEnd != null)
                    OnCompilationEnd(this);
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e);
            }
        }

        /// <summary>
        /// Called when a process has finished
        /// </summary>
        private void OnExecutionOk(ProExecution lastExecution) {
            DoInLock(() => {
                _processesRunning--;
                EndOfCompilation();
            });
        }

        /// <summary>
        /// When a process has finished compiling correctly
        /// </summary>
        private void OnExecCompilationOk(ProExecutionHandleCompilation proc, List<FileToCompile> fileToCompiles, List<FileToDeploy> filesToDeploy) {
            DoInLock(() => {
                // aggregate the info on each process
                if (fileToCompiles != null)
                    _listFilesToCompile.AddRange(fileToCompiles);
                if (filesToDeploy != null)
                    _listFilesToDeploy.AddRange(filesToDeploy);
            });
        }

        /// <summary>
        /// Called when a process has finished UNsuccessfully
        /// </summary>
        private void OnExecutionFailed(ProExecution lastExecution) {
            DoInLock(() => {
                KillProcesses();
                EndOfCompilation();
            });
        }

        /// <summary>
        /// Execute the action behind the lock
        /// </summary>
        private static void DoInLock(Action toDo) {
            Monitor.Enter(_lock);
            try {
                toDo();
            } finally {
                Monitor.Exit(_lock);
            }
        }

        #endregion

    }
}