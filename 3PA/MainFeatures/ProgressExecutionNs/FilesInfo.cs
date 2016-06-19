#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
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
using _3PA.MainFeatures.SyntaxHighlighting;

namespace _3PA.MainFeatures.ProgressExecutionNs {

    /// <summary>
    /// Keeps info on the files currently opened in notepad++
    /// </summary>
    internal static class FilesInfo {

        #region event

        public delegate void UpdatedOperation(UpdatedOperationEventArgs args);
        public static event UpdatedOperation OnUpdatedOperation;

        public delegate void UpdatedErrors(UpdatedErrorsEventArgs args);
        public static event UpdatedErrors OnUpdatedErrors;

        #endregion

        #region fields

        /// <summary>
        /// Dictionnary, file info for each file opened
        /// </summary>
        private static Dictionary<string, FileInfoObject> _sessionInfo = new Dictionary<string, FileInfoObject>(StringComparer.CurrentCultureIgnoreCase);

        #endregion

        #region const

        public const int ErrorMarginWidth = 10;
        public const int ErrorMarginNumber = 3;

        /// <summary>
        /// Mask for the first 6 markers : 1 + 2 + 4 + 8 + 16 + 32
        /// </summary>
        public const int EveryMarkersMask = 63;

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

            // Update info on the current file
            if (fullPath.EqualsCi(Plug.CurrentFilePath))
                UpdateErrorsInScintilla();
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

        /// <summary>
        /// Get style index of given error + error style
        /// </summary>
        /// <param name="errorLevel"></param>
        /// <param name="fontWeight"></param>
        /// <returns></returns>
        public static byte GetStyleOf(ErrorLevel errorLevel, ErrorFontWeight fontWeight) {
            switch (fontWeight) {
                case ErrorFontWeight.Bold:
                    return (byte)(errorLevel + Style.ErrorAnnotBoldStyleOffset);
                case ErrorFontWeight.Italic:
                    return (byte)(errorLevel + Style.ErrorAnnotItalicStyleOffset);
                default:
                    return (byte)(errorLevel + Style.ErrorAnnotStandardStyleOffset);
            }
        }

        /// <summary>
        /// Updates the number of errors in the FileExplorer form and the file status
        /// </summary>
        public static void UpdateFileStatus() {
            var currentFilePath = Plug.CurrentFilePath;

            // UpdatedOperation event
            if (OnUpdatedOperation != null) {
                if (_sessionInfo.ContainsKey(currentFilePath))
                    OnUpdatedOperation(new UpdatedOperationEventArgs(_sessionInfo[currentFilePath].CurrentOperation));
                else
                    OnUpdatedOperation(new UpdatedOperationEventArgs(0));
            }

            // UpdatedErrors event
            if (OnUpdatedErrors != null) {
                if (_sessionInfo.ContainsKey(currentFilePath) && _sessionInfo[currentFilePath].FileErrors != null) {
                    // find max error
                    ErrorLevel maxLvl = ErrorLevel.NoErrors;
                    if (_sessionInfo[currentFilePath].FileErrors.Any()) {
                        maxLvl = _sessionInfo[currentFilePath].FileErrors.OrderByDescending(error => error.Level).First().Level;
                    }
                    OnUpdatedErrors(new UpdatedErrorsEventArgs(maxLvl, _sessionInfo[currentFilePath].FileErrors.Count));
                } else
                    OnUpdatedErrors(new UpdatedErrorsEventArgs(ErrorLevel.NoErrors, 0));
            }
        }

        /// <summary>
        /// Displays the errors for the current file (if any)
        /// display an annotation with the message below the line + display a marker in the margin
        /// </summary>
        public static void UpdateErrorsInScintilla() {

            // Updates the number of errors in the FileExplorer form and the file status
            UpdateFileStatus();

            var currentFilePath = Plug.CurrentFilePath;
            var marginError = Npp.GetMargin(ErrorMarginNumber);

            // need to clear scintilla for this file?
            if (_sessionInfo.ContainsKey(currentFilePath) && _sessionInfo[currentFilePath].NeedToCleanScintilla) {
                ClearAnnotationsAndMarkers();
                _sessionInfo[currentFilePath].NeedToCleanScintilla = false;
            }

            // check if current file is a progress and if we got info on it 
            if (!Plug.IsCurrentFileProgress || !_sessionInfo.ContainsKey(currentFilePath) || _sessionInfo[currentFilePath].FileErrors == null || _sessionInfo[currentFilePath].FileErrors.Count == 0) {
                if (marginError.Width > 0) {
                    marginError.Width = 1;
                    marginError.Width = 0;
                }
                return;
            }

            // show margin
            if (marginError.Sensitive == false)
                marginError.Sensitive = true;
            if (marginError.Type != MarginType.Symbol)
                marginError.Type = MarginType.Symbol;
            if (marginError.Mask != EveryMarkersMask)
                marginError.Mask = EveryMarkersMask;

            StylerHelper stylerHelper = new StylerHelper();
            int lastLine = -2;
            StringBuilder lastMessage = new StringBuilder();
            foreach (var fileError in _sessionInfo[currentFilePath].FileErrors) {

                // new line
                if (lastLine != fileError.Line) {
                    stylerHelper.Clear();
                    lastMessage.Clear();
                    // set marker style now (the first error encountered for a given line is the highest anyway)
                    if (!((int)Npp.GetLine(fileError.Line).MarkerGet()).IsBitSet((int)fileError.Level))
                        Npp.GetLine(fileError.Line).MarkerAdd((int)fileError.Level);
                } else {
                    // append to existing annotation
                    stylerHelper.Style("\n", (byte)fileError.Level);
                    lastMessage.Append("\n");
                }

                lastLine = fileError.Line;

                var mess = fileError.FromProlint ? "Prolint (level " + fileError.ErrorNumber : ("Compilation " + (fileError.Level == ErrorLevel.Critical ? "error" : "warning") + " (n°" + fileError.ErrorNumber);
                mess += fileError.FromProlint ? "): " : ", col " + fileError.Column + "): ";
                stylerHelper.Style(mess, (byte)(Style.ErrorAnnotBoldStyleOffset + fileError.Level));
                lastMessage.Append(mess);

                mess = fileError.Message.BreakText(140);
                stylerHelper.Style(mess, (byte)(Style.ErrorAnnotStandardStyleOffset + fileError.Level));
                lastMessage.Append(mess);

                if (Config.Instance.GlobalShowDetailedHelpForErrors && !string.IsNullOrEmpty(fileError.Help)) {
                    mess = "\nDetailed help: " + fileError.Help.BreakText(140);
                    stylerHelper.Style(mess, (byte)(Style.ErrorAnnotItalicStyleOffset + fileError.Level));
                    lastMessage.Append(mess);
                }

                if (fileError.Times > 0) {
                    mess = "\nThis message above appeared " + fileError.Times + " times in the compiler log";
                    stylerHelper.Style(mess, (byte)(Style.ErrorAnnotBoldStyleOffset + fileError.Level));
                    lastMessage.Append(mess);
                }

                // set annotation
                Npp.GetLine(lastLine).AnnotationText = lastMessage.ToString();
                Npp.GetLine(lastLine).AnnotationStyles = stylerHelper.GetStyleArray();
            }

            marginError.Width = ErrorMarginWidth + 1;
            marginError.Width = ErrorMarginWidth;
        }

        /// <summary>
        /// Clear all the errors for the given document and then update the view (immediatly if current doc, delay on doc change otherwise),
        /// returns true if it actually cleared something, false it there was no errors
        /// 
        /// if clearForCompil = true, then it also the clear the errors the include files that were in errors
        /// because of the compilation of the given file...
        /// </summary>
        public static bool ClearAllErrors(string filePath, bool clearForCompil = false) {

            if (string.IsNullOrEmpty(filePath))
                return false;

            bool jobDone = false;

            if (_sessionInfo.ContainsKey(filePath) && _sessionInfo[filePath].FileErrors != null) {
                _sessionInfo[filePath].FileErrors.Clear();
                jobDone = true;

                if (filePath.Equals(Plug.CurrentFilePath)) {
                    ClearAnnotationsAndMarkers();
                    UpdateFileStatus();
                } else
                    _sessionInfo[filePath].NeedToCleanScintilla = true;
            }

            if (clearForCompil) { 
            // for each file info that has an error generated when compiling the "filePath"
            foreach (var kpv in _sessionInfo.Where(pair => pair.Value.FileErrors != null && pair.Value.FileErrors.Exists(error => error.CompiledFilePath.EqualsCi(filePath)))) {
                kpv.Value.FileErrors.Clear();
                jobDone = true;

                if (kpv.Key.Equals(Plug.CurrentFilePath)) {
                    ClearAnnotationsAndMarkers();
                    UpdateFileStatus();
                } else
                    kpv.Value.NeedToCleanScintilla = true;
            }
}

            return jobDone;
        }

        /// <summary>
        /// Clears the errors in line 'line' both in the errorsList and visually cleaning annoation + margin symbol
        /// when a user click on the maring symbol, it clears the errors for the line,
        /// returns true if it actually cleans something, false otherwise
        /// Will always clear the line "visually", but if the user added lines then our references are messed up so it
        /// won't work
        /// </summary>
        /// <param name="line"></param>
        public static bool ClearLineErrors(int line) {
            if (!_sessionInfo.ContainsKey(Plug.CurrentFilePath))
                return false;

            bool jobDone = false;
            if (_sessionInfo[Plug.CurrentFilePath].FileErrors.Exists(error => error.Line == line)) {
                _sessionInfo[Plug.CurrentFilePath].FileErrors.RemoveAll(error => error.Line == line);
                jobDone = true;
            }

            // visually clear line
            ClearLine(line);
            if (jobDone) {
                UpdateErrorsInScintilla();
            } else {
                // we didn't manage to clear the error, (only visually, not in our records), 
                // so clear everything, the user will have to compile again
                _sessionInfo[Plug.CurrentFilePath].FileErrors.Clear();
                _sessionInfo[Plug.CurrentFilePath].NeedToCleanScintilla = true;
            }
            return jobDone;
        }

        /// <summary>
        /// Visually clear all the annotations and markers from a line
        /// </summary>
        /// <param name="line"></param>
        public static void ClearLine(int line) {
            var lineObj = Npp.GetLine(line);

            lineObj.AnnotationText = null;
            foreach (var errorLevelMarker in Enum.GetValues(typeof(ErrorLevel)))
                if (((int)lineObj.MarkerGet()).IsBitSet((int)errorLevelMarker))
                    lineObj.MarkerDelete((int)errorLevelMarker);
        }

        /// <summary>
        /// Clears all the annotations and the markers from scintilla
        /// </summary>
        public static void ClearAnnotationsAndMarkers() {
            if (Npp.GetLine(0).MarkerGet() != 0) {
                if (!_sessionInfo.ContainsKey(Plug.CurrentFilePath) ||
                    _sessionInfo[Plug.CurrentFilePath].FileErrors == null ||
                    !_sessionInfo[Plug.CurrentFilePath].FileErrors.Exists(error => error.Line == 0)) {
                    // The line 0 has an error marker when it shouldn't
                    ClearLine(0);
                }
            }
            int nextLine = Npp.GetLine(0).MarkerNext(EveryMarkersMask);
            while (nextLine > -1) {
                if (!_sessionInfo.ContainsKey(Plug.CurrentFilePath) ||
                    _sessionInfo[Plug.CurrentFilePath].FileErrors == null ||
                    !_sessionInfo[Plug.CurrentFilePath].FileErrors.Exists(error => error.Line == nextLine)) {
                    // The line 0 has an error marker when it shouldn't
                    ClearLine(nextLine);
                }
                nextLine = Npp.GetLine(nextLine + 1).MarkerNext(EveryMarkersMask);
            }
            // We should be able to use the lines below, but the experience proves that they sometimes crash...
            /*
            Npp.AnnotationClearAll(); // <- this shit tends to be unstable for some reasons
            foreach (var errorLevelMarker in Enum.GetValues(typeof(ErrorLevel)))
                Npp.Marker.MarkerDeleteAll((int)errorLevelMarker);
            */
        }

        /// <summary>
        /// When the user click the error margin but no error is cleaned, it goes to the next line with error instead
        /// </summary>
        public static void GoToNextError(int line) {
            var currentFilePath = Plug.CurrentFilePath;
            if (!_sessionInfo.ContainsKey(currentFilePath))
                return;
            int nextLine = Npp.GetLine(line).MarkerNext(EveryMarkersMask);
            if (nextLine == -1 && _sessionInfo[currentFilePath].FileErrors.Exists(error => error.Line == 0))
                nextLine = 0;
            if (nextLine == -1)
                nextLine = Npp.GetLine(0).MarkerNext(EveryMarkersMask);
            if (nextLine != -1) {
                var errInfo = _sessionInfo[currentFilePath].FileErrors.FirstOrDefault(error => error.Line == nextLine);
                if (errInfo != null)
                    Npp.Goto(currentFilePath, errInfo.Line, errInfo.Column);
                else
                    Npp.Goto(currentFilePath, nextLine, 0);
            }
        }

        /// <summary>
        /// Go to the previous error
        /// </summary>
        public static void GoToPrevError(int line) {
            var currentFilePath = Plug.CurrentFilePath;
            var nbLines = Npp.Line.Count;
            if (!_sessionInfo.ContainsKey(currentFilePath))
                return;
            int prevLine = Npp.GetLine(line).MarkerPrevious(EveryMarkersMask);
            if (prevLine == -1 && _sessionInfo[currentFilePath].FileErrors.Exists(error => error.Line == nbLines))
                prevLine = nbLines;
            if (prevLine == -1)
                prevLine = Npp.GetLine(nbLines).MarkerPrevious(EveryMarkersMask);
            if (prevLine != -1) {
                var errInfo = _sessionInfo[currentFilePath].FileErrors.FirstOrDefault(error => error.Line == prevLine);
                if (errInfo != null)
                    Npp.Goto(currentFilePath, errInfo.Line, errInfo.Column);
                else
                    Npp.Goto(currentFilePath, prevLine, 0);
            }
        }

        /// <summary>
        /// Reads an error log file, format :
        /// filepath \t ErrorLevel \t line \t column \t error number \t message \t help
        /// (column and line can be equals to "?" in that case, they will be forced to 0)
        /// fromProlint = true allows to set FromProlint to true in the object,
        /// permutePaths allows to replace a path with another, useful when we compiled from a tempdir but we want the errors
        /// to appear for the "real" file
        /// </summary>
        public static Dictionary<string, List<FileError>> ReadErrorsFromFile(string fullPath, bool fromProlint, Dictionary<string, string> permutePaths) {
            var output = new Dictionary<string, List<FileError>>(StringComparer.CurrentCultureIgnoreCase);

            if (!File.Exists(fullPath))
                return output;

            var lastLineNbCouple = new[] { -10, -10 };
            foreach (var lineFields in File.ReadAllLines(fullPath, TextEncodingDetect.GetFileEncoding(fullPath)).Select(line => line.Split('\t')).Where(items => items.Count() == 7 || items.Count() == 8)) {
                var fields = lineFields.ToList();

                // the first field should be the file that was compiled, it might not be there for PROLINT.log 
                // because at the time i didn't specify it in the interface contract... So it is added here if it is missing
                if (fields.Count() == 7) {
                    fields.Insert(0, permutePaths.Count > 0 ? permutePaths.First().Value : "");
                }

                // new file
                var filePath = (permutePaths.ContainsKey(fields[1]) ? permutePaths[fields[1]] : fields[1]);
                if (!output.ContainsKey(filePath)) {
                    output.Add(filePath, new List<FileError>());
                    lastLineNbCouple = new[] { -10, -10 };
                }

                ErrorLevel errorLevel;
                if (!Enum.TryParse(fields[2], true, out errorLevel))
                    errorLevel = ErrorLevel.Error;

                // we store the line/error number couple because we don't want two identical messages to appear
                var thisLineNbCouple = new[] { fields[3].Equals("?") ? 0 : int.Parse(fields[3]) - 1, fields[5].Equals("?") ? 0 : int.Parse(fields[5]) };

                if (thisLineNbCouple[0] == lastLineNbCouple[0] && thisLineNbCouple[1] == lastLineNbCouple[1]) {
                    // same line/error number as previously
                    if (output[filePath].Count > 0) {
                        var lastFileError = output[filePath].Last();
                        if (lastFileError != null)
                            if (lastFileError.Times == 0)
                                lastFileError.Times = 2;
                            else
                                lastFileError.Times++;
                    }
                    continue;
                }
                lastLineNbCouple = thisLineNbCouple;

                int column;
                if (!int.TryParse(fields[4], out column)) column = 0;
                if (column > 0) column--;

                var baseFileName = Path.GetFileName(filePath);

                // add error
                output[filePath].Add(new FileError {
                    SourcePath = filePath,
                    Level = errorLevel,
                    Line = lastLineNbCouple[0],
                    Column = column,
                    ErrorNumber = lastLineNbCouple[1],
                    Message = fields[6].Replace("<br>", "\n").Replace(fields[1], baseFileName).Replace(filePath, baseFileName).Trim(),
                    Help = fields[7].Replace("<br>", "\n").Trim(),
                    FromProlint = fromProlint,
                    CompiledFilePath = (permutePaths.ContainsKey(fields[0]) ? permutePaths[fields[0]] : fields[0])
                });
            }
            return output;
        }

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
    internal class FileInfoObject {

        private CurrentOperation _currentOperation;

        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        public CurrentOperation CurrentOperation {
            get {
                CurrentOperation output = 0;
                if (!_lock.TryEnterWriteLock(500)) return output;
                try {
                    output = _currentOperation;
                } finally {
                    _lock.ExitWriteLock();
                }
                return output;
            }
            set {
                if (!_lock.TryEnterWriteLock(500)) return;
                try {
                    _currentOperation = value;
                } finally {
                    _lock.ExitWriteLock();
                }
            }
        }

        public List<FileError> FileErrors { get; set; }
        public bool WarnedTooLong { get; set; }
        public ProExecution ProgressExecution { get; set; }
        public bool SavedSinceLastCompilation { get; set; }
        public string FileFullPath { get; set; }
        public bool NeedToCleanScintilla { get; set; }
    }

    /// <summary>
    /// Current undergoing operation on the file
    /// Retrieve the DisplayText value with ((CurrentOperationAttr)currentOperation.GetAttributes()).DisplayText
    /// </summary>
    [Flags]
    internal enum CurrentOperation {
        [DisplayAttr(Name = "Editing")]
        Default = 0,
        [DisplayAttr(Name = "Appbuilder section!")]
        AppbuilderSection = 32,
        [DisplayAttr(Name = "Linting", ActionText = "prolint-ing")]
        Prolint = 64,
        [DisplayAttr(Name = "Checking syntax", ActionText = "checking the syntax of")]
        CheckSyntax = 128,
        [DisplayAttr(Name = "Compiling", ActionText = "compiling")]
        Compile = 216,
        [DisplayAttr(Name = "Executing", ActionText = "executing")]
        Run = 512
    }

    /// <summary>
    /// Errors found for this file, either from compilation or from prolint
    /// </summary>
    internal class FileError {
        public string SourcePath { get; set; }
        public ErrorLevel Level { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public int ErrorNumber { get; set; }
        public string Message { get; set; }
        public string Help { get; set; }
        public bool FromProlint { get; set; }
        /// <summary>
        /// indicates if the error appears several times
        /// </summary>
        public int Times { get; set; }
        // the path to the file that was compiled to generate this error
        public string CompiledFilePath { get; set; }
    }

    /// <summary>
    /// Sort FileError
    /// </summary>
    internal class FileErrorSortingClass : IComparer<FileError> {
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
            // then sort by column
            compare = x.Column.CompareTo(y.Column);
            if (compare != 0) return compare;
            // compare Column
            return y.ErrorNumber.CompareTo(x.ErrorNumber);
        }
    }

    /// <summary>
    /// Describes the error level, the num is also used for MARKERS in scintilla
    /// and thus must start at 0
    /// </summary>
    internal enum ErrorLevel {
        [DisplayAttr(Name = "Error(s), good!")]
        NoErrors,
        [DisplayAttr(Name = "Info")]
        Information,
        [DisplayAttr(Name = "Warning(s)")]
        Warning,
        [DisplayAttr(Name = "Huge warning(s)")]
        StrongWarning,
        [DisplayAttr(Name = "Error(s)")]
        Error,
        [DisplayAttr(Name = "Critical error(s)!")]
        Critical
    }

    internal enum ErrorFontWeight {
        Normal,
        Bold,
        Italic
    }

    #endregion

    #region EventArgs

    internal class UpdatedOperationEventArgs : EventArgs {
        public CurrentOperation CurrentOperation { get; private set; }
        public UpdatedOperationEventArgs(CurrentOperation currentOperation) {
            CurrentOperation = currentOperation;
        }
    }

    internal class UpdatedErrorsEventArgs : EventArgs {
        public ErrorLevel ErrorLevel { get; private set; }
        public int NbErrors { get; private set; }
        public UpdatedErrorsEventArgs(ErrorLevel errorLevel, int nbErrors) {
            ErrorLevel = errorLevel;
            NbErrors = nbErrors;
        }
    }

    #endregion

}
