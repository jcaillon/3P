#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
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
using _3PA.Data;
using _3PA.Lib;
using _3PA.MainFeatures.FileExplorer;

namespace _3PA.MainFeatures.Pro {

    /// <summary>
    /// Class used for the mass compiler
    /// </summary>
    internal class ProCompilation {

        #region public fields

        /// <summary>
        /// Is the compilation mono process?
        /// </summary>
        public bool MonoProcess { get; set; }

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

        // remember the time when the compilation started
        public DateTime StartingTime { get; private set; }

        #endregion

        #region private fields

        // list of all the started processes
        private List<CompilationProcess> _listOfCompilationProcesses = new List<CompilationProcess>();

        // total number of processes still running
        private int _processesRunning;

        private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private long _nbFilesTransfered;

        private bool _hasBeenKilled;

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
                        if (!filesToCompile.Contains(filePath)) {
                            bool toAdd = true;

                            // test include filters
                            if (!string.IsNullOrEmpty(Config.Instance.CompileIncludeList)) {
                                var hasMatch = false;
                                foreach (var pattern in Config.Instance.CompileIncludeList.Split(',')) {
                                    if (filePath.RegexMatch(pattern.WildCardToRegex()))
                                        hasMatch = true;
                                }
                                toAdd = hasMatch;
                            }

                            // test exclude filters
                            if (!string.IsNullOrEmpty(Config.Instance.CompileExcludeList)) {
                                var hasNoMatch = true;
                                foreach (var pattern in Config.Instance.CompileExcludeList.Split(',')) {
                                    if (filePath.RegexMatch(pattern.WildCardToRegex()))
                                        hasNoMatch = false;
                                }
                                toAdd = toAdd && hasNoMatch;
                            }

                            if (toAdd)
                                filesToCompile.Add(filePath);
                        }
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
                return (float)_nbFilesTransfered / TransferedFiles.Count * 100;

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
            TimeSpan t = TimeSpan.FromMilliseconds(DateTime.Now.Subtract(StartingTime).TotalMilliseconds);
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
        public static string FormatCompilationResult(FileToCompile fileToCompile, List<FileError> listErrorFiles, List<FileToDeploy> listDeployedFiles) {

            var line = new StringBuilder();
            var nbErrors = 0;

            line.Append("<div style='padding-bottom: 5px;'><b>" + string.Format("<a class='SubTextColor' href='{0}'>{1}</a>", fileToCompile.InputPath, Path.GetFileName(fileToCompile.InputPath)) + "</b> in " + Path.GetDirectoryName(fileToCompile.InputPath).ToHtmlLink() + "</div>");

            foreach (var fileError in listErrorFiles) {
                nbErrors += fileError.Level > ErrorLevel.StrongWarning ? 1 : 0;
                line.Append("<div style='padding-left: 10px'>" + "<img src='" + (fileError.Level > ErrorLevel.StrongWarning ? "MsgError" : "MsgWarning") + "' height='15px'>" + (!fileError.CompiledFilePath.Equals(fileError.SourcePath) ? "in " + string.Format("<a class='SubTextColor' href='{0}'>{1}</a>", fileError.SourcePath, Path.GetFileName(fileError.SourcePath)) + ", " : "") + (fileError.SourcePath + "|" + fileError.Line).ToHtmlLink("line " + (fileError.Line + 1)) + " (n°" + fileError.ErrorNumber + ") " + (fileError.Times > 0 ? "(x" + fileError.Times + ") " : "") + fileError.Message + "</div>");
            }

            foreach (var file in listDeployedFiles) {
                var ext = (Path.GetExtension(file.To) ?? "").Replace(".", "");
                var transferMsg = file.DeployType == DeployType.Copy && file.FinalDeploy ? "" : "(" + file.DeployType + ") ";
                if (file.IsOk && (nbErrors == 0 || !ext.Equals("r"))) {
                    line.Append("<div style='padding-left: 10px'>" + "<img src='" + ext.ToTitleCase() + "Type' height='15px'>" + transferMsg + (ext.EqualsCi("lst") ? file.To.ToHtmlLink() : Path.GetDirectoryName(file.To).ToHtmlLink(file.To)) + "</div>");
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
                TransferedFiles = Deployer.DeployFiles(TransferedFiles, obj.ProEnv.ProlibPath, i => _nbFilesTransfered = i);

                // Read all the log files stores the errors
                foreach (var compilationProcess in _listOfCompilationProcesses) {
                    var errorList = compilationProcess.ProExecutionObject.LoadErrorLog();
                    foreach (var keyValue in errorList) {
                        ErrorsList.AddRange(keyValue.Value);
                    }
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

        #region Single : Compilation, Check syntax, Run, Prolint

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
                OnExecutionEnd = OnSingleExecutionEnd,
                OnExecutionOk = OnSingleExecutionOk
            };
            if (!Plug.CurrentFileObject.ProgressExecution.Do(executionType))
                return;

            // change file object current operation, set flag
            Plug.CurrentFileObject.CurrentOperation |= currentOperation;
            FilesInfo.UpdateFileStatus();

            // clear current errors (updates the current file info)
            FilesInfo.ClearAllErrors(Plug.CurrentFilePath, true);

        }

        /// <summary>
        /// Allows to kill the process of the currently running Progress.exe (if any, for the current file)
        /// </summary>
        public static void KillCurrentProcess() {
            if (Plug.CurrentFileObject.ProgressExecution != null) {
                Plug.CurrentFileObject.ProgressExecution.KillProcess();
                UserCommunication.CloseUniqueNotif("KillExistingProcess");
                OnSingleExecutionEnd(Plug.CurrentFileObject.ProgressExecution);
            }
        }

        /// <summary>
        /// Called after the execution of run/compile/check/prolint, clear the current operation from the file
        /// </summary>
        public static void OnSingleExecutionEnd(ProExecution lastExec) {
            try {
                var treatedFile = lastExec.ListToCompile.First();
                CurrentOperation currentOperation;
                if (!Enum.TryParse(lastExec.ExecutionType.ToString(), true, out currentOperation))
                    currentOperation = CurrentOperation.Run;

                // Clear flag or we can't do any other actions on this file
                FilesInfo.GetFileInfo(treatedFile.InputPath).CurrentOperation &= ~currentOperation;
                var isCurrentFile = treatedFile.InputPath.EqualsCi(Plug.CurrentFilePath);
                if (isCurrentFile)
                    FilesInfo.UpdateFileStatus();

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in OnExecutionEnd");
            }
        }

        /// <summary>
        /// Called after the execution of run/compile/check/prolint
        /// </summary>
        public static void OnSingleExecutionOk(ProExecution lastExec) {
            try {
                var treatedFile = lastExec.ListToCompile.First();
                CurrentOperation currentOperation;
                if (!Enum.TryParse(lastExec.ExecutionType.ToString(), true, out currentOperation))
                    currentOperation = CurrentOperation.Run;

                var isCurrentFile = treatedFile.InputPath.EqualsCi(Plug.CurrentFilePath);
                var otherFilesInError = false;
                int nbWarnings = 0;
                int nbErrors = 0;

                // Read log info
                var errorList = lastExec.LoadErrorLog();

                if (!errorList.Any()) {
                    // the compiler messages are empty
                    var fileInfo = new FileInfo(lastExec.LogPath);
                    if (fileInfo.Length > 0) {
                        // the .log is not empty, maybe something went wrong in the runner, display errors
                        UserCommunication.Notify(
                            "Something went wrong while " + currentOperation.GetAttribute<CurrentOperationAttr>().ActionText + " the following file:<br>" + treatedFile.InputPath.ToHtmlLink() + "<br>The progress compiler didn't return any errors but the log isn't empty, here is the content :" +
                            Utils.ReadAndFormatLogToHtml(lastExec.LogPath), MessageImg.MsgError,
                            "Critical error", "Action failed");
                        return;
                    }
                } else {
                    // count number of warnings/errors, loop through files > loop through errors in each file
                    foreach (var keyValue in errorList) {
                        foreach (var fileError in keyValue.Value) {
                            if (fileError.Level <= ErrorLevel.StrongWarning) nbWarnings++;
                            else nbErrors++;
                        }
                        otherFilesInError = otherFilesInError || !treatedFile.InputPath.EqualsCi(keyValue.Key);
                    }
                }

                // Prepare the notification content
                var notifTitle = currentOperation.GetAttribute<CurrentOperationAttr>().Name;
                var notifImg = (nbErrors > 0) ? MessageImg.MsgError : ((nbWarnings > 0) ? MessageImg.MsgWarning : MessageImg.MsgOk);
                var notifTimeOut = (nbErrors > 0) ? 0 : ((nbWarnings > 0) ? 10 : 5);
                var notifSubtitle = lastExec.ExecutionType == ExecutionType.Prolint ? (nbErrors + nbWarnings) + " problem" + ((nbErrors + nbWarnings) > 1 ? "s" : "") + " detected" :
                    (nbErrors > 0) ? nbErrors + " error" + (nbErrors > 1 ? "s" : "") + " found" :
                        ((nbWarnings > 0) ? nbWarnings + " warning" + (nbWarnings > 1 ? "s" : "") + " found" :
                            "Syntax correct");

                // build the error list
                var errorsList = new List<FileError>();
                foreach (var keyValue in errorList) {
                    errorsList.AddRange(keyValue.Value);
                }

                // when compiling, transfering .r/.lst to compilation dir
                var listTransferFiles = new List<FileToDeploy>();
                if (lastExec.ExecutionType == ExecutionType.Compile) {
                    listTransferFiles = lastExec.CreateListOfFilesToDeploy();
                    listTransferFiles = Deployer.DeployFiles(listTransferFiles, lastExec.ProEnv.ProlibPath);
                }

                // Notify the user, or not
                if (Config.Instance.CompileAlwaysShowNotification || !isCurrentFile || !Npp.GetFocus() || otherFilesInError)
                    UserCommunication.NotifyUnique(treatedFile.InputPath, "Was " + currentOperation.GetAttribute<CurrentOperationAttr>().ActionText + " :<br>" + FormatCompilationResult(treatedFile, errorsList, listTransferFiles), notifImg, notifTitle, notifSubtitle, null, notifTimeOut);

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in OnExecutionOk");
            }
        }

        #endregion


    }
}
