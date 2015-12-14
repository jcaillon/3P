#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (FilesInfo.cs) is part of 3P.
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
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures.ProgressExecutionNs;
using _3PA.MainFeatures.SyntaxHighlighting;

namespace _3PA.MainFeatures.FilesInfoNs {

    /// <summary>
    /// Keeps info on the files currently opened in notepad++
    /// </summary>
    public static class FilesInfo {

        #region event

        private static event EventHandler<UpdatedOperationEventArgs> OnUpdatedOperation;

        /// <summary>
        /// You should register to this event to know when the button has been pressed (clicked or enter or space)
        /// </summary>
        public static event EventHandler<UpdatedOperationEventArgs> UpdatedOperation {
            add { OnUpdatedOperation += value; }
            remove { OnUpdatedOperation -= value; }
        }

        #endregion

        #region fields

        /// <summary>
        /// Dictionnary, file info for each file opened
        /// </summary>
        private static Dictionary<string, FileInfoObject> _sessionInfo = new Dictionary<string, FileInfoObject>();

        #endregion

        #region const

        public const int ErrorMarginWidth = 10;
        public const int ErrorMarginNumber = 3;

        /// <summary>
        /// Mask for the first 5 markers : 1 + 2 + 4 + 8 + 16
        /// </summary>
        public const int EveryMarkersMask = 31;

        /// <summary>
        /// for the annotations we use scintilla's styles, we offset the ErrorLevel by this amount to get the style ID
        /// </summary>
        public const int ErrorAnnotStandardStyleOffset = 250;
        public const int ErrorAnnotBoldStyleOffset = 245;
        public const int ErrorAnnotItalicStyleOffset = 240;

        #endregion

        #region public methods

        /// <summary>
        /// update errors list for a file
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="errorsList"></param>
        public static void UpdateFileErrors(string fullPath, List<FileError> errorsList) {
            AddIfNew(fullPath);
            if (_sessionInfo[fullPath].FileErrors != null)
                _sessionInfo[fullPath].FileErrors.Clear();
            _sessionInfo[fullPath].FileErrors = errorsList.ToList();
            _sessionInfo[fullPath].FileErrors.Sort(new FileErrorSortingClass());
        }

        /// <summary>
        /// Returns the FileInfoObject of given file
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static FileInfoObject GetFileInfo(string fullPath) {
            AddIfNew(fullPath);
            return _sessionInfo[fullPath];
        }

        #region user interface methods

        /// <summary>
        /// Get style index of given error + error style
        /// </summary>
        /// <param name="errorLevel"></param>
        /// <param name="fontWeight"></param>
        /// <returns></returns>
        public static byte GetStyleOf(ErrorLevel errorLevel, ErrorFontWeight fontWeight) {
            switch (fontWeight) {
                case ErrorFontWeight.Bold:
                    return (byte)(errorLevel + ErrorAnnotBoldStyleOffset);
                case ErrorFontWeight.Italic:
                    return (byte)(errorLevel + ErrorAnnotItalicStyleOffset);
                default:
                    return (byte)(errorLevel + ErrorAnnotStandardStyleOffset);
            }
        }

        /// <summary>
        /// Update the current operation for the file, update the state
        /// Displays the errors for the current file (if any)
        /// display an annotation with the message below the line + display a marker in the margin
        /// </summary>
        public static void DisplayCurrentFileInfo() {

            var marginError = Npp.GetMargin(ErrorMarginNumber);

            // reset margin and annotations
            if (marginError.Width > 0)
                marginError.Width = 0;
            marginError.Sensitive = true;
            marginError.Type = MarginType.Symbol;

            // set mask so markers from 0 to 3 are displayed in this margin
            marginError.Mask = EveryMarkersMask;

            Npp.AnnotationClearAll();
            foreach (var errorLevelMarker in Enum.GetValues(typeof(ErrorLevel)))
                Npp.Marker.MarkerDeleteAll((int)errorLevelMarker);

            // check if current file is a progress and if we got info on it
            var currentFilePath = Npp.GetCurrentFilePath();
            if (!Abl.IsCurrentProgressFile() || !_sessionInfo.ContainsKey(currentFilePath))
                return;

            // UpdatedOperation event
            if (OnUpdatedOperation != null) {
                if (_sessionInfo.ContainsKey(currentFilePath))
                    OnUpdatedOperation(new object(), new UpdatedOperationEventArgs(_sessionInfo[currentFilePath].CurrentOperation));
                else
                    OnUpdatedOperation(new object(), new UpdatedOperationEventArgs(0));}

            // got error info on it?
            if (_sessionInfo[currentFilePath].FileErrors == null || _sessionInfo[currentFilePath].FileErrors.Count == 0)
                return;

            // show margin
            marginError.Width = ErrorMarginWidth;

            StylerHelper stylerHelper = new StylerHelper();
            int lastLine = -2;
            StringBuilder lastMessage = new StringBuilder();
            foreach (var fileError in _sessionInfo[currentFilePath].FileErrors) {
                if (lastLine != fileError.Line) {
                    stylerHelper.Clear();
                    lastMessage.Clear();
                    // set marker style now (the first error encountered for a given line is the highest anyway)
                    Npp.GetLine(fileError.Line).MarkerAdd((int)fileError.Level);
                    //Npp.SetAnnotationStyle(fileError.Line, ErrorAnnotationStyleOffset + (int)fileError.Level);
                } else {
                    stylerHelper.Style("\n", (byte)fileError.Level);
                    lastMessage.Append("\n");
                }

                lastLine = fileError.Line;

                var mess = (fileError.FromProlint ? "Prolint (level " + fileError.ErrorNumber + "): " : "Compilation " + (fileError.Level == ErrorLevel.Critical ? "error" : "warning") + " (n°" + fileError.ErrorNumber + "): ");
                stylerHelper.Style(mess, (byte)(ErrorAnnotBoldStyleOffset + fileError.Level));
                lastMessage.Append(mess);

                mess = fileError.Message.BreakText(140);
                stylerHelper.Style(mess, (byte)(ErrorAnnotStandardStyleOffset + fileError.Level));
                lastMessage.Append(mess);

                if (!string.IsNullOrEmpty(fileError.Help)) {
                    mess = "\nDetailed help: " + fileError.Help.BreakText(140);
                    stylerHelper.Style(mess, (byte)(ErrorAnnotItalicStyleOffset + fileError.Level));
                    lastMessage.Append(mess);
                }

                // set annotation
                Npp.GetLine(lastLine).AnnotationText = lastMessage.ToString();
                Npp.GetLine(lastLine).AnnotationStyles = stylerHelper.GetStyleArray();
            }
        }

        /// <summary>
        /// Clears the errors in line 'line' both in the errorsList and visually cleaning annoation + margin symbol
        /// when a user click on the maring symbol, it clears the errors for the line,
        /// returns true if it actually cleans something, false otherwise
        /// </summary>
        /// <param name="line"></param>
        public static bool ClearError(int line) {
            var currentFilePath = Npp.GetCurrentFilePath();
            if (!_sessionInfo.ContainsKey(currentFilePath))
                return false;
            bool jobDone = false;
            try {
                if (_sessionInfo[currentFilePath].FileErrors.Exists(error => error.Line == line)) {
                    _sessionInfo[currentFilePath].FileErrors.RemoveAll(error => error.Line == line);
                    jobDone = true;
                }
            } catch (Exception) {
                // ignored
            }
            if (jobDone) {
                Npp.GetLine(line).AnnotationText = null;
                Npp.GetLine(line).MarkerDelete(-1);

                // hide margin is there is nothing to display
                if (_sessionInfo[currentFilePath].FileErrors.Count == 0)
                    Npp.GetMargin(ErrorMarginNumber).Width = 0;
            }
            // hide margin if no errors
            return jobDone;
        }

        /// <summary>
        /// When the user click the error margin but no error is cleaned, it goes to the next line with error instead
        /// </summary>
        public static void GoToNextError(int line) {
            var currentFilePath = Npp.GetCurrentFilePath();
            if (!_sessionInfo.ContainsKey(currentFilePath))
                return;
            int nextLine = Npp.GetLine(line).MarkerNext(EveryMarkersMask);
            if (nextLine == -1 && _sessionInfo[currentFilePath].FileErrors.Exists(error => error.Line == 0))
                nextLine = 0;
            if (nextLine == -1)
                nextLine = Npp.GetLine(0).MarkerNext(EveryMarkersMask);
            if (nextLine != -1) {
                try {
                    var errInfo = _sessionInfo[currentFilePath].FileErrors.First(error => error.Line == nextLine);
                    Npp.Goto(currentFilePath, errInfo.Line, errInfo.Column);
                } catch (Exception) {
                    Npp.GoToLine(nextLine);
                }
            }
        }

        /// <summary>
        /// Go to the previous error
        /// </summary>
        public static void GoToPrevError(int line) {
            var currentFilePath = Npp.GetCurrentFilePath();
            var nbLines = Npp.Lines.Count;
            if (!_sessionInfo.ContainsKey(currentFilePath))
                return;
            int prevLine = Npp.GetLine(line).MarkerPrevious(EveryMarkersMask);
            if (prevLine == -1 && _sessionInfo[currentFilePath].FileErrors.Exists(error => error.Line == nbLines))
                prevLine = nbLines;
            if (prevLine == -1)
                prevLine = Npp.GetLine(nbLines).MarkerPrevious(EveryMarkersMask);
            if (prevLine != -1) {
                try {
                    var errInfo = _sessionInfo[currentFilePath].FileErrors.First(error => error.Line == prevLine);
                    Npp.Goto(currentFilePath, errInfo.Line, errInfo.Column);
                } catch (Exception) {
                    Npp.GoToLine(prevLine);
                }
            }
        }

        /// <summary>
        /// Reads an error log file, format :
        /// filepath \t ErrorLevel \t line \t column \t error number \t message \t help
        /// fromProlint = true allows to set FromProlint to true in the object,
        /// permutePaths allows to replace a path with another, useful when we compiled from a tempdir but we want the errors
        /// to appear for the "real" file
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="fromProlint"></param>
        /// <param name="permutePaths"></param>
        /// <returns></returns>
        public static Dictionary<string, List<FileError>> ReadErrorsFromFile(string fullPath, bool fromProlint, Dictionary<string, string> permutePaths) {
            var output = new Dictionary<string, List<FileError>>();
            foreach (var items in File.ReadAllLines(fullPath, TextEncodingDetect.GetFileEncoding(fullPath)).Select(line => line.Split('\t')).Where(items => items.Count() == 7)) {

                ErrorLevel errorLevel;
                if (!Enum.TryParse(items[1], true, out errorLevel))
                    errorLevel = ErrorLevel.Error;

                var filePath = (permutePaths.ContainsKey(items[0]) ? permutePaths[items[0]] : items[0]);
                if (!output.ContainsKey(filePath))
                    output.Add(filePath, new List<FileError>());
                output[filePath].Add(new FileError {
                    Level = errorLevel,
                    Line = items[2].Equals("?") ? 0 : int.Parse(items[2]) - 1,
                    Column = items[3].Equals("?") ? 0 : int.Parse(items[3]),
                    ErrorNumber = items[4].Equals("?") ? 0 : int.Parse(items[4]),
                    Message = items[5],
                    Help = items[6],
                    FromProlint = fromProlint
                });
            }
            return output;
        }

        #endregion

        #endregion

        #region private methods

        /// <summary>
        /// Just add the file to the session info if it doesn't exist
        /// </summary>
        /// <param name="fullPath"></param>
        private static void AddIfNew(string fullPath) {
            if (!_sessionInfo.ContainsKey(fullPath))
                _sessionInfo.Add(fullPath, new FileInfoObject());
        }

        #endregion
    }

    #region FileInfoObject

    /// <summary>
    /// This class allows to keep info on a particular file loaded in npp's session
    /// </summary>
    public class FileInfoObject {

        #region event

        private static event EventHandler<UpdatedOperationEventArgs> OnUpdatedOperation;

        /// <summary>
        /// You should register to this event to know when the button has been pressed (clicked or enter or space)
        /// </summary>
        public static event EventHandler<UpdatedOperationEventArgs> UpdatedOperation {
            add { OnUpdatedOperation += value; }
            remove { OnUpdatedOperation -= value; }
        }

        #endregion

        private CurrentOperation _currentOperation;
        private static object _lock = new object();
        public CurrentOperation CurrentOperation {
            get {
                CurrentOperation output = 0;
                bool lockTaken = false;
                try {
                    Monitor.TryEnter(_lock, 1500, ref lockTaken);
                    if (lockTaken) output = _currentOperation;
                } catch (Exception e) {
                    ErrorHandler.Log("Couldn't get the lock on CurrentOperation??! Exception is : " + e);
                } finally {
                    if (lockTaken) Monitor.Exit(_lock);
                }
                return output;
            }
            set {
                bool lockTaken = false;
                try {
                    Monitor.TryEnter(_lock, 1500, ref lockTaken);
                    if (lockTaken) {
                        _currentOperation = value;

                        // publish UpdatedOperation event
                        if (OnUpdatedOperation != null && Plug.CurrentFilePath.EqualsCi(FileFullPath)) {
                            OnUpdatedOperation(this, new UpdatedOperationEventArgs(_currentOperation));
                        }
                    }
                } catch (Exception e) {
                    ErrorHandler.Log("Couldn't get the lock on CurrentOperation??! Exception is : " + e);
                } finally {
                    if (lockTaken) Monitor.Exit(_lock);
                }
            }
        }

        public List<FileError> FileErrors { get; set; }
        public bool WarnedTooLong { get; set; }
        public ProgressExecution ProgressExecution { get; set; }
        public bool SavedSinceLastCompilation { get; set; }
        public string FileFullPath { get; set; }
    }

    /// <summary>
    /// Current undergoing operation on the file
    /// </summary>
    [Flags]
    public enum CurrentOperation {
        CheckSyntax = 1,
        Compile = 2,
        Run = 4,
        Prolint = 8,

    }

    /// <summary>
    /// Errors found for this file, either from compilation or from prolint
    /// </summary>
    public class FileError {
        public ErrorLevel Level { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public int ErrorNumber { get; set; }
        public string Message { get; set; }
        public string Help { get; set; }
        public bool FromProlint { get; set; }
    }

    /// <summary>
    /// Sort FileError
    /// </summary>
    public class FileErrorSortingClass : IComparer<FileError> {
        public int Compare(FileError x, FileError y) {
            // compare first by line
            int compare = x.Line.CompareTo(y.Line);
            if (compare != 0) return compare;
            // then sort by error level
            compare = y.Level.CompareTo(x.Level);
            if (compare != 0) return compare;
            // then sort by from prolint
            compare = y.FromProlint.CompareTo(x.FromProlint);
            if (compare != 0) return compare;
            // compare Column
            return x.Column.CompareTo(y.Column);
        }
    }

    /// <summary>
    /// Describes the error level, the num is also used for MARKERS in scintilla
    /// and thus must start at 0
    /// </summary>
    public enum ErrorLevel {
        Information = 0,
        Warning = 1,
        StrongWarning = 2,
        Error = 3,
        Critical = 4
    }

    public enum ErrorFontWeight {
        Normal,
        Bold,
        Italic
    }

    #endregion

    #region UpdateOperationEventArgs

    public class UpdatedOperationEventArgs : EventArgs {
        public CurrentOperation CurrentOperation { get; private set; }
        public UpdatedOperationEventArgs(CurrentOperation currentOperation) {
            CurrentOperation = currentOperation;
        }
    }

    #endregion

}
