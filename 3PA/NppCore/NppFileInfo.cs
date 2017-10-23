#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (NppFileInfo.cs) is part of 3P.
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
using System.Collections.Generic;
using System.Text;
using _3PA.Lib;
using _3PA.MainFeatures;
using _3PA.WindowsCore;

namespace _3PA.NppCore {

    internal static partial class Npp {

        private static NppFileInfo _currentFile;

        /// <summary>
        /// Keeps track of the files that were forced to be progress files and also files that were forced not to 
        /// </summary>
        public static Dictionary<string, string> ProgressFileExeptions {
            // <path, 0/1 = is progress or not>
            get { return Config.Instance.ProgressFileExeptions; }
        }

        /// <summary>
        /// Get info and do stuff on the current file
        /// </summary>
        public static NppFileInfo CurrentFileInfo {
            get { return _currentFile ?? (_currentFile = new NppFileInfo()); }
        }

        /// <summary>
        /// We don't want to recompute those values all the time so we store them when the buffer (document) changes
        /// </summary>
        internal class NppFileInfo {

            #region Private fields

            private string _path;
            private NppLangs.LangDescription _lang;

            #endregion

            #region Public

            /// <summary>
            /// Stores the file path when switching document
            /// </summary>
            public string Path {
                get { return _path; }
                set {
                    _path = value;
                    IsProgress = (_path.TestAgainstListOfPatterns(Config.Instance.FilesPatternProgress) && CanReadAsProgress) || MustReadAsProgress;
                    _lang = null;
                }
            }

            /// <summary>
            /// true if the file is a progress file, false otherwise
            /// </summary>
            public bool IsProgress { get; private set; }

            /// <summary>
            /// Lang description for the current language
            /// </summary>
            public NppLangs.LangDescription Lang {
                get {
                    if (_lang == null)
                        _lang = NppLangs.Instance.GetLangDescription(CurrentInternalLangName);
                    return _lang;
                }
                set { _lang = value; }
            }

            /// <summary>
            /// Is the file a progress + compilable file?
            /// </summary>
            public bool IsCompilable {
                get { return Path.TestAgainstListOfPatterns(Config.Instance.FilesPatternCompilable); }
            }

            /// <summary>
            /// file name
            /// </summary>
            public string FileName {
                get { return System.IO.Path.GetFileName(Path); }
            }

            /// <summary>
            /// Directory of file
            /// </summary>
            public string DirectoryName {
                get { return System.IO.Path.GetDirectoryName(Path); }
            }

            /// <summary>
            /// Extension of file
            /// </summary>
            public string Extension {
                get { return System.IO.Path.GetExtension(Path); }
            }

            /// <summary>
            /// Gets the path of the current document
            /// </summary>
            /// <returns></returns>
            public static string GetFullPathApi {
                get {
                    var path = new StringBuilder(Win32Api.MaxPath);
                    Win32Api.SendMessage(Handle, NppMsg.NPPM_GETFULLCURRENTPATH, 0, path);
                    return path.ToString();
                }
            }

            /// <summary>
            /// Saves the current document
            /// </summary>
            public void Save() {
                Win32Api.SendMessage(Handle, NppMsg.NPPM_SAVECURRENTFILE, 0, 0);
            }

            /// <summary>
            /// Saves current document as...
            /// </summary>
            public void SaveAs(string path) {
                Win32Api.SendMessage(Handle, NppMsg.NPPM_SAVECURRENTFILEAS, 0, path);
            }

            /// <summary>
            /// Saves a copy of the current document
            /// </summary>
            public void SaveAsCopy(string path) {
                Win32Api.SendMessage(Handle, NppMsg.NPPM_SAVECURRENTFILEAS, 1, path);
            }

            /// <summary>
            /// Reload current file
            /// </summary>
            public void Reload(bool askConfirmation) {
                Npp.Reload(Path, askConfirmation);
            }

            /// <summary>
            /// Returns true if the file can be considered as a progress file
            /// </summary>
            /// <returns></returns>
            private bool CanReadAsProgress {
                get { return !ProgressFileExeptions.ContainsKey(Path) || ProgressFileExeptions[Path] == "1"; }
            }

            /// <summary>
            /// Returns true if the file must be considered as a progress file
            /// </summary>
            /// <returns></returns>
            private bool MustReadAsProgress {
                get { return ProgressFileExeptions.ContainsKey(Path) && ProgressFileExeptions[Path] == "1"; }
            }

            /// <summary>
            /// Flag current file as non progress (force it even if the extension matches)
            /// </summary>
            public void SetAsNonProgress() {
                if (IsProgress) {
                    if (ProgressFileExeptions.ContainsKey(Path)) {
                        ProgressFileExeptions[Path] = "0";
                    } else {
                        ProgressFileExeptions.Add(Path, "0");
                    }
                }
            }

            /// <summary>
            /// Flag the current file as a progress file even if the extension doesn't match
            /// </summary>
            public void SetAsProgress() {
                if (!IsProgress) {
                    if (ProgressFileExeptions.ContainsKey(Path)) {
                        ProgressFileExeptions[Path] = "1";
                    } else {
                        ProgressFileExeptions.Add(Path, "1");
                    }
                }
            }

            #endregion
        }

        private static NppFileInfo _previousFile;

        /// <summary>
        /// Info on the previous file
        /// </summary>
        public static NppFileInfo PreviousFileInfo {
            get { return _previousFile ?? (_previousFile = new NppFileInfo()); }
        }
    }
}
