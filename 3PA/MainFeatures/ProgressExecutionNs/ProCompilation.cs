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
using _3PA.Html;

namespace _3PA.MainFeatures.ProgressExecutionNs {

    internal class ProCompilation {

        #region private fields

        // list of all the started processes
        private List<CompilationProcess> _listOfCompilationProcess = new List<CompilationProcess>();

        private DateTime _startingTime;

        private int _numberOfProcesses;

        private int _processesRunning;

        private int _nbFilesToCompile;

        #endregion


        #region public methods

        /// <summary>
        /// This method starts a compilation of all the compilable files in the given folders,
        /// it expects a list of path of the folders to compile
        /// </summary>
        /// <param name="listOfFolderPath"></param>
        public void CompileFolders(List<string> listOfFolderPath) {

            // constructs the list of all the files (unique) accross the different folders
            var filesToCompile = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
            foreach (var folderPath in listOfFolderPath) {
                foreach (var filePath in Config.Instance.KnownCompilableExtension.Split(',').SelectMany(s => Directory.EnumerateFiles(folderPath, "*" + s, SearchOption.AllDirectories)).ToList()) {
                    if (!filesToCompile.Contains(filePath))
                        filesToCompile.Add(filePath);
                }
            }

            if (filesToCompile.Count == 0) {
                UserCommunication.Notify("No compilable files found in the input directories,<br>the valid extensions for compilable Progress files are : " + Config.Instance.KnownCompilableExtension, MessageImg.MsgInfo, "Multiple compilation", "No files found", 10);
                return;
            }

            // now we do a list of those files, sorted from the biggest (in size) to the smallest file
            var sizeFileList = new List<ProCompilationFile>();
            foreach (var filePath in filesToCompile) {
                var fileInfo = new FileInfo(filePath);
                sizeFileList.Add(new ProCompilationFile {Path = filePath, Size = fileInfo.Length});
            }
            sizeFileList.Sort((file1, file2) => file2.Size.CompareTo(file1.Size));

            // we want to dispatch all thoses files in a fair way among the Prowin processes we will create...
            _numberOfProcesses = Config.Instance.NbOfProcessesByCore*Environment.ProcessorCount;
            _listOfCompilationProcess.Clear();
            var currentProcess = 0;
            foreach (var file in sizeFileList) {
                // create a new process when needed
                if (currentProcess >= _listOfCompilationProcess.Count)
                    _listOfCompilationProcess.Add(new CompilationProcess());

                // assign the file to the current process
                _listOfCompilationProcess[currentProcess].FilesToCompile.Add(new FileToCompile { InputPath = file.Path });

                // we will assign the next file to the next process...
                currentProcess++;
                if (currentProcess == _numberOfProcesses)
                    currentProcess = 0;
            }

            _nbFilesToCompile = filesToCompile.Count;
            _startingTime = DateTime.Now;
            _processesRunning = _listOfCompilationProcess.Count;

            // lets start the compilation on each process
            foreach (var compilationProcess in _listOfCompilationProcess) {

                // launch the compile process
                compilationProcess.ProExecutionObject = new ProExecution {
                    ListToCompile = compilationProcess.FilesToCompile,
                    OnExecutionEnd = OnExecutionEnded,
                    //OnExecutionEndOk = OnExecutionEndedOk
                };
                if (!compilationProcess.ProExecutionObject.Do(ExecutionType.Compile))
                    return;
            }
        }

        private void OnExecutionEnded(ProExecution lastExecution) {
            _processesRunning--;

            // if this process was the last one running
            if (_processesRunning == 0) {
                UserCommunication.Notify("Compiling X files = " + _nbFilesToCompile + "<br>using X process " + _listOfCompilationProcess.Count + "<br>end in X s = " + DateTime.Now.Subtract(_startingTime).TotalSeconds, 0);
            }
        }

        #endregion


        private struct ProCompilationFile {
            public string Path { get; set; }
            public long Size { get; set; } 
        }

        internal class CompilationProcess {
            public List<FileToCompile> FilesToCompile = new List<FileToCompile>();
            public ProExecution ProExecutionObject;
        }

    }
}
