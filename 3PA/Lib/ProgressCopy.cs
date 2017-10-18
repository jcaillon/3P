#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProgressCopy.cs) is part of 3P.
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
using System.ComponentModel;
using System.IO;
using System.Net;

namespace _3PA.Lib {

    /// <summary>
    /// This class allows to copy a file or a folder asynchronously, with the possibility to cancel and see the progress
    /// </summary>
    internal class ProgressCopy {

        /// <summary>
        /// Copies the file asynchronously, you can cancel it by using the returns object and its method cancel
        /// </summary>
        public static ProgressCopy CopyAsync(string source, string destination, EventHandler<EndEventArgs> completed, EventHandler<ProgressEventArgs> progressChanged) {
            var stack = new Stack<FileToCopy>();
            stack.Push(new FileToCopy(source, destination));
            return new ProgressCopy(stack, completed, progressChanged);
        }

        /// <summary>
        /// Copies the folder asynchronously, you can cancel it by using the returns object and its method cancel
        /// </summary>
        public static ProgressCopy CopyDirectory(string source, string target, EventHandler<EndEventArgs> completed, EventHandler<ProgressEventArgs> progressChanged) {
            var fileStack = new Stack<FileToCopy>();
            var folderStack = new Stack<FolderToCopy>();
            try {
                folderStack.Push(new FolderToCopy(source, target));
                while (folderStack.Count > 0) {
                    var folders = folderStack.Pop();
                    Directory.CreateDirectory(folders.Target);
                    foreach (var file in Directory.GetFiles(folders.Source, "*.*")) {
                        fileStack.Push(new FileToCopy(file, Path.Combine(folders.Target, Path.GetFileName(file))));
                    }
                    foreach (var folder in Directory.GetDirectories(folders.Source)) {
                        folderStack.Push(new FolderToCopy(folder, Path.Combine(folders.Target, Path.GetFileName(folder))));
                    }
                }
            } catch (Exception e) {
                if (completed != null)
                    completed(null, new EndEventArgs(CopyCompletedType.Exception, e));
            }
            return new ProgressCopy(fileStack, completed, progressChanged);
        }

        private long _total;

        private WebClient _wc;

        private Stack<FileToCopy> _filesToCopy;

        private ProgressCopy(Stack<FileToCopy> stack, EventHandler<EndEventArgs> completed, EventHandler<ProgressEventArgs> progressChanged) {
            _filesToCopy = stack;
            Completed += completed;
            ProgressChanged += progressChanged;
            _total = _filesToCopy.Count;

            _wc = new WebClient();
            _wc.DownloadProgressChanged += WebClientOnDownloadProgressChanged;
            _wc.DownloadFileCompleted += WcOnDownloadFileCompleted;

            // start treating the next file
            Next();
        }

        /// <summary>
        /// Aborts the copy asynchronously and throws Completed event when done
        /// </summary>
        public void AbortCopyAsync() {
            _wc.CancelAsync();
        }

        /// <summary>
        /// Event which will notify the subscribers if the copy gets completed
        /// There are three scenarios in which completed event will be thrown when
        /// 1.Copy succeeded
        /// 2.Copy aborted
        /// 3.Any exception occurred
        /// </summary>
        private event EventHandler<EndEventArgs> Completed;

        /// <summary>
        /// Event which will notify the subscribers if there is any progress change while copying
        /// </summary>
        private event EventHandler<ProgressEventArgs> ProgressChanged;

        /// <summary>
        /// Copies the next file in the stack
        /// </summary>
        private void Next() {
            if (_filesToCopy.Count > 0) {
                var currentFile = _filesToCopy.Pop();
                _wc.DownloadFileAsync(new Uri(currentFile.Source), currentFile.Target);
            } else {
                if (Completed != null)
                    Completed(this, new EndEventArgs(CopyCompletedType.Succeeded, null));
            }
        }

        private void WcOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs args) {
            if (args.Cancelled || args.Error != null) {
                if (Completed != null) {
                    Completed(this, new EndEventArgs(args.Cancelled ? CopyCompletedType.Aborted : CopyCompletedType.Exception, args.Error));
                }
            } else {
                Next();
            }
        }

        private void WebClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs downloadProgressChangedEventArgs) {
            if (ProgressChanged != null)
                ProgressChanged(this, new ProgressEventArgs((_total - _filesToCopy.Count) / (double)_total * 100.0, downloadProgressChangedEventArgs.ProgressPercentage));
        }


        private class FolderToCopy {
            public string Source { get; private set; }
            public string Target { get; private set; }
            public FolderToCopy(string source, string target) {
                Source = source;
                Target = target;
            }
        }

        private class FileToCopy {
            public string Source { get; private set; }
            public string Target { get; private set; }
            public FileToCopy(string source, string target) {
                Source = source;
                Target = target;
            }
        }

        /// <summary>
        /// Type indicates how the copy gets completed
        /// </summary>
        internal enum CopyCompletedType {
            Succeeded,
            Aborted,
            Exception
        }

        /// <summary>
        /// Event arguments for file copy 
        /// </summary>
        internal class EndEventArgs : EventArgs {

            /// <summary>
            /// Constructor
            /// </summary>
            public EndEventArgs(CopyCompletedType type, Exception exception) {
                Type = type;
                Exception = exception;
            }

            /// <summary>
            /// Type of the copy completed type
            /// </summary>
            public CopyCompletedType Type { get; private set; }

            /// <summary>
            /// Exception if any happened during copy
            /// </summary>
            public Exception Exception { get; private set; }

        }
        
        /// <summary>
        /// Event arguments for file copy 
        /// </summary>
        internal class ProgressEventArgs : EventArgs {

            /// <summary>
            /// Constructor
            /// </summary>
            public ProgressEventArgs(double total, double current) {
                TotalFiles = total;
                CurrentFile = current;
            }

            /// <summary>
            /// Percent of total files done
            /// </summary>
            public double TotalFiles { get; private set; }

            /// <summary>
            /// Percent done for current file
            /// </summary>
            public double CurrentFile { get; private set; }

        }
    }
    
}
