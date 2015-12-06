using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _3PA.Interop;
using _3PA.Lib;

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

        #region public 

        public const int ErrorMarginNumber = 4;
        public const int ErrorMarginWidth = 10;
        /// <summary>
        /// for the annotations we use scintilla's styles, we offset the ErrorLevel by this amount to get the style ID
        /// </summary>
        public const int ErrorAnnotationStyleOffset = 250;

        /// <summary>
        /// Update the current operation for the file, update the state
        /// Displays the errors for the current file (if any)
        /// display an annotation with the message below the line + display a marker in the margin
        /// </summary>
        public static void DisplayCurrentFileInfo() {

            // set mask so markers from 0 to 3 are displayed in this margin
            Npp.SetMarginMask(ErrorMarginNumber, 31);

            // reset margin and annotations
            Npp.SetMargin(ErrorMarginNumber, SciMsg.SC_MARGIN_SYMBOL, 0, 1);
            Npp.DeleteAllAnnotations();
            Npp.DeleteAllMarker((int)ErrorLevel.Information);
            Npp.DeleteAllMarker((int)ErrorLevel.Warning);
            Npp.DeleteAllMarker((int)ErrorLevel.StrongWarning);
            Npp.DeleteAllMarker((int)ErrorLevel.Error);
            Npp.DeleteAllMarker((int)ErrorLevel.Critical);

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
            Npp.SetMargin(ErrorMarginNumber, SciMsg.SC_MARGIN_SYMBOL, 12, 1);

            int lastLine = -2;
            string lastMessage = "";
            foreach (var fileError in _sessionInfo[currentFilePath].FileErrors) {
                if (lastLine != fileError.Line) {
                    lastMessage = "";
                    // set marker style
                    Npp.AddMarker(fileError.Line, (int)fileError.Level);
                    // set annotation style
                    Npp.SetAnnotationStyle(fileError.Line, ErrorAnnotationStyleOffset + (int)fileError.Level);
                } else
                    lastMessage += "\n";
                lastLine = fileError.Line;
                lastMessage += (fileError.FromProlint ? "Prolint (level " + fileError.ErrorNumber + "): " : "Compilation " + (fileError.Level == ErrorLevel.Critical ? "error" : "warning") + " (n°" + fileError.ErrorNumber + "): ") +
                    fileError.Message.BreakText(140) + (!string.IsNullOrEmpty(fileError.Help) ? "\nDetailed help: " + fileError.Help.BreakText(140) : "");

                // set annotation
                Npp.AddAnnotation(lastLine, lastMessage);
            }
        }

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
        /// Update current operation
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="operation"></param>
        public static void UpdateFileOperation(string fileName, CurrentOperation operation) {
            AddIfNew(fileName);
            _sessionInfo[fileName].CurrOperation = operation;
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
                Npp.DeleteAnnotation(line);
                Npp.DeleteMarker(line, -1);

                // hide margin is there is nothing to display
                if (_sessionInfo[currentFilePath].FileErrors.Count == 0)
                    Npp.SetMargin(ErrorMarginNumber, SciMsg.SC_MARGIN_SYMBOL, 0, 1);
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
            int nextLine = Npp.GetNextMarkerLine(line, 31);
            if (nextLine == -1 && _sessionInfo[currentFilePath].FileErrors.Exists(error => error.Line == 0))
                nextLine = 0;
            if (nextLine == -1)
                nextLine = Npp.GetNextMarkerLine(0, 31);
            if (nextLine != -1) {
                UserCommunication.Notify(nextLine.ToString());
                try {
                    var errInfo = _sessionInfo[currentFilePath].FileErrors.First(error => error.Line == nextLine);
                    UserCommunication.Notify(errInfo.Line + " " + errInfo.Column);
                    Npp.Goto(currentFilePath, errInfo.Line, errInfo.Column);
                } catch (Exception) {
                    Npp.GoToLine(nextLine);
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
                foreach (var typ in Enum.GetNames(typeof(ErrorLevel)).Where(typ => errorLevelStr.EqualsCi(typ)))
                    errorLevel = (ErrorLevel)Enum.Parse(typeof(ErrorLevel), typ, true);

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
    /// <summary>
    /// This class allows to keep info on a particular file loaded in npp's session
    /// </summary>
    public class FileInfoObject {

        public CurrentOperation CurrOperation { get; set; }
        public List<FileError> FileErrors { get; set; }
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

}
