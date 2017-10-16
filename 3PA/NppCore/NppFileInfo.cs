﻿using System.Text;
using _3PA.Lib;
using _3PA.MainFeatures;
using _3PA.MainFeatures.AutoCompletionFeature;
using _3PA.WindowsCore;

namespace _3PA.NppCore {

    internal static partial class Npp {

        private static NppFileInfo _currentFile;

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
                    var currentInternalLang = CurrentInternalLangName;
                    IsProgress = _path.TestAgainstListOfPatterns(Config.Instance.FilesPatternProgress) || currentInternalLang.Equals("openedgeabl");
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