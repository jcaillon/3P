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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using _3PA.Html;
using _3PA.Lib;
using _3PA.MainFeatures.FileExplorer;
using _3PA.MainFeatures.FilesInfoNs;

namespace _3PA.MainFeatures.ProgressExecutionNs {

    /// <summary>
    /// Class used for the mass compiler
    /// </summary>
    internal class ProCompilation {

        #region public fields

        /// <summary>
        /// Is the compilation mono process?
        /// </summary>
        public bool MonoProcess { private get; set; }

        /// <summary>
        /// Set to true if you want to explore the folders recursively to find all the compilable files
        /// </summary>
        public bool RecursInDirectories { private get; set; }

        // total number of files being compiled
        public long NbFilesToCompile { get; private set; }

        // total number of processes used
        public int NumberOfProcesses { get; private set; }

        // total number of processes finished ok
        public int NumberOfProcessesEndedOk { get; private set; }

        // has the compilation been canceled / processes killed?
        public bool HasBeenKilled { get; private set; }

        public event Action OnCompilationEnd;

        public string ExecutionTime { get; private set; }

        /// <summary>
        /// After the compilation, each file is moved to its destination folder (distant or source folder)
        /// This list keeps tracks of the moved files and the success of the move command
        /// </summary>
        public List<FileToMove> MovedFiles { get; private set; }

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

        #endregion

        #region private fields

        // list of all the started processes
        private List<CompilationProcess> _listOfCompilationProcesses = new List<CompilationProcess>();

        // remember the time when the compilation started
        private DateTime _startingTime;

        // total number of processes still running
        private int _processesRunning;

        private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private long _nbFilesMoved;

        #endregion


        #region public methods

        /// <summary>
        /// This method starts a compilation of all the compilable files in the given folders,
        /// it expects a list of path of the folders to compile
        /// </summary>
        /// <param name="listOfFolderPath"></param>
        public bool CompileFolders(List<string> listOfFolderPath) {

            var searchOptions = RecursInDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            // constructs the list of all the files (unique) accross the different folders
            var filesToCompile = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
            foreach (var folderPath in listOfFolderPath) {
                if (Directory.Exists(folderPath)) {
                    foreach (var filePath in Config.Instance.CompileKnownExtension.Split(',').SelectMany(s => Directory.EnumerateFiles(folderPath, "*" + s, searchOptions)).ToList()) {
                        if (!filesToCompile.Contains(filePath))
                            filesToCompile.Add(filePath);
                    }
                }
            }

            if (filesToCompile.Count == 0) {
                UserCommunication.Notify("No compilable files found in the input directories,<br>the valid extensions for compilable Progress files are : " + Config.Instance.CompileKnownExtension, MessageImg.MsgInfo, "Multiple compilation", "No files found", 10);
                return false;
            }

            // now we do a list of those files, sorted from the biggest (in size) to the smallest file
            var sizeFileList = new List<ProCompilationFile>();
            foreach (var filePath in filesToCompile) {
                var fileInfo = new FileInfo(filePath);
                sizeFileList.Add(new ProCompilationFile {Path = filePath, Size = fileInfo.Length});
            }
            sizeFileList.Sort((file1, file2) => file2.Size.CompareTo(file1.Size));

            // we want to dispatch all thoses files in a fair way among the Prowin processes we will create...
            NumberOfProcesses = MonoProcess ? 1 : Config.Instance.NbOfProcessesByCore * Environment.ProcessorCount;
            _listOfCompilationProcesses.Clear();
            var currentProcess = 0;
            foreach (var file in sizeFileList) {
                // create a new process when needed
                if (currentProcess >= _listOfCompilationProcesses.Count)
                    _listOfCompilationProcesses.Add(new CompilationProcess());

                // assign the file to the current process
                _listOfCompilationProcesses[currentProcess].FilesToCompile.Add(new FileToCompile { InputPath = file.Path });

                // we will assign the next file to the next process...
                currentProcess++;
                if (currentProcess == NumberOfProcesses)
                    currentProcess = 0;
            }

            // init
            NbFilesToCompile = filesToCompile.Count;
            _startingTime = DateTime.Now;
            _processesRunning = _listOfCompilationProcesses.Count;
            NumberOfProcesses = _listOfCompilationProcesses.Count;
            NumberOfProcessesEndedOk = 0;
            MovedFiles = new List<FileToMove>();
            ErrorsList = new List<FileError>();

            // lets start the compilation on each process
            foreach (var compilationProcess in _listOfCompilationProcesses) {

                // launch the compile process
                compilationProcess.ProExecutionObject = new ProExecution {
                    ListToCompile = compilationProcess.FilesToCompile,
                    OnExecutionEnd = OnExecutionEnd,
                    OnExecutionOk = OnExecutionOk,
                    OnExecutionFailed = OnExecutionFailed
                };
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
                return (float)_nbFilesMoved / MovedFiles.Count * 100;

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
            TimeSpan t = TimeSpan.FromMilliseconds(DateTime.Now.Subtract(_startingTime).TotalMilliseconds);
            if (t.Hours > 0)
                return string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds);
            if (t.Minutes > 0)
                return string.Format("{0:D2}m:{1:D2}s", t.Minutes, t.Seconds);
            if (t.Seconds > 0)
                return string.Format("{0:D2}s", t.Seconds);
            return string.Format("{0:D3}ms", t.Milliseconds);
        }

        /// <summary>
        /// This method "cancel" the compilation by killing the associated processes
        /// </summary>
        public void KillProcesses() {
            HasBeenKilled = true;
            foreach (var compilationProcess in _listOfCompilationProcesses) {
                compilationProcess.ProExecutionObject.KillProcess();
            }
        }

        /// <summary>
        /// returns the list of the processes that have been started
        /// </summary>
        public List<FileToCompile> GetListOfFileToCompile {
            get { return _listOfCompilationProcesses.SelectMany(compProcess => compProcess.ProExecutionObject.ListToCompile).ToList(); }
        }

        /// <summary>
        /// Allows to know how many files of each file type there is
        /// </summary>
        public Dictionary<FileType, int> GetNbFilesPerType() {

            Dictionary<FileType, int> output = new Dictionary<FileType, int>();

            foreach (var fileToCompile in GetListOfFileToCompile) {
                FileType fileType;
                if (!Enum.TryParse((Path.GetExtension(fileToCompile.InputPath) ?? "").Replace(".", ""), true, out fileType))
                    fileType = FileType.Unknow;
                if (output.ContainsKey(fileType))
                    output[fileType]++;
                else
                    output.Add(fileType, 1);
            }

            return output;
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

            // we need to move all the files...
            foreach (var compilationProcess in _listOfCompilationProcesses) {
                MovedFiles.AddRange(ProExecution.CreateListOfFilesToMove(compilationProcess.ProExecutionObject));
            }
            foreach (var fileToMove in MovedFiles) {
                _nbFilesMoved++;
                fileToMove.IsOk = Utils.MoveFile(fileToMove.From, fileToMove.To, true);
            }

            // Read all the log files stores the errors
            foreach (var compilationProcess in _listOfCompilationProcesses) {
                var errorList = ProExecution.LoadErrorLog(compilationProcess.ProExecutionObject);
                foreach (var keyValue in errorList) {
                    ErrorsList.AddRange(keyValue.Value);
                }
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
                    EndOfCompilation();
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
                    EndOfCompilation();
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
