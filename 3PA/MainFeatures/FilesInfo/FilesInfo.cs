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
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures.SyntaxHighlighting;

namespace _3PA.MainFeatures.FilesInfo {

    /// <summary>
    /// Keeps info on the files currently opened in notepad++
    /// </summary>
    public static class FilesInfo {

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
        /// <param name="fileName"></param>
        /// <param name="errorsList"></param>
        public static void UpdateFileErrors(string fileName, List<FileError> errorsList) {
            AddIfNew(fileName);
            if (_sessionInfo[fileName].FileErrors != null)
                _sessionInfo[fileName].FileErrors.Clear();
            _sessionInfo[fileName].FileErrors = errorsList.ToList();
            _sessionInfo[fileName].FileErrors.Sort(new FileErrorSortingClass());
        }

        /// <summary>
        /// Returns the FileInfoObject of given file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static FileInfoObject GetFileInfo(string fileName) {
            AddIfNew(fileName);
            return _sessionInfo[fileName];
        }

        /// <summary>
        /// Returns current file FileInfoObject
        /// </summary>
        /// <returns></returns>
        public static FileInfoObject GetFileInfo() {
            return GetFileInfo(Npp.GetCurrentFilePath());
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
            foreach (var errorLevelMarker in Enum.GetValues(typeof (ErrorLevel)))
                Npp.Marker.MarkerDeleteAll((int) errorLevelMarker);

            // default operation


            // check if current file is a progress and if we got info on it
            var currentFilePath = Npp.GetCurrentFilePath();
            if (!Abl.IsCurrentProgressFile() || !_sessionInfo.ContainsKey(currentFilePath))
                return;

            // update current operation


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
                    Npp.GetLine(fileError.Line).MarkerAdd((int) fileError.Level);
                    //Npp.SetAnnotationStyle(fileError.Line, ErrorAnnotationStyleOffset + (int)fileError.Level);
                } else {
                    stylerHelper.Style("\n", (byte) fileError.Level);
                    lastMessage.Append("\n");
                }

                lastLine = fileError.Line;

                var mess = (fileError.FromProlint ? "Prolint (level " + fileError.ErrorNumber + "): " : "Compilation " + (fileError.Level == ErrorLevel.Critical ? "error" : "warning") + " (n°" + fileError.ErrorNumber + "): ");
                stylerHelper.Style(mess, (byte) (ErrorAnnotBoldStyleOffset + fileError.Level));
                lastMessage.Append(mess);

                mess = fileError.Message.BreakText(140);
                stylerHelper.Style(mess, (byte) (ErrorAnnotStandardStyleOffset + fileError.Level));
                lastMessage.Append(mess);

                if (!string.IsNullOrEmpty(fileError.Help)) {
                    mess = "\nDetailed help: " + fileError.Help.BreakText(140);
                    stylerHelper.Style(mess, (byte) (ErrorAnnotItalicStyleOffset + fileError.Level));
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
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fromProlint"></param>
        /// <returns></returns>
        public static Dictionary<string, List<FileError>> ReadErrorsFromFile(string fileName, bool fromProlint) {
            var output = new Dictionary<string, List<FileError>>();
            foreach (var items in File.ReadAllLines(fileName, TextEncodingDetect.GetFileEncoding(fileName)).Select(line => line.Split('\t')).Where(items => items.Count() == 7)) {

                ErrorLevel errorLevel = ErrorLevel.Error;
                var errorLevelStr = items[1];
                foreach (var typ in Enum.GetNames(typeof (ErrorLevel)).Where(typ => errorLevelStr.EqualsCi(typ)))
                    errorLevel = (ErrorLevel) Enum.Parse(typeof (ErrorLevel), typ, true);

                if (!output.ContainsKey(items[0]))
                    output.Add(items[0], new List<FileError>());
                output[items[0]].Add(new FileError {
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
        /// <param name="fileName"></param>
        private static void AddIfNew(string fileName) {
            if (!_sessionInfo.ContainsKey(fileName))
                _sessionInfo.Add(fileName, new FileInfoObject());
        }

        #endregion

    }

    #region FileInfoObject

    /// <summary>
    /// This class allows to keep info on a particular file loaded in npp's session
    /// </summary>
    public class FileInfoObject {
        public CurrentOperation CurrOperation { get; set; }
        public List<FileError> FileErrors { get; set; }
        public bool WarnedTooLong { get; set; }
    }

    /// <summary>
    /// Current undergoing operation on the file
    /// </summary>
    public enum CurrentOperation {
        None,
        CheckSynthax,
        Compile,
        Prolint
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


}
