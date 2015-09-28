using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _3PA.Data;
using _3PA.Lib;

namespace _3PA.MainFeatures {
    public class ProgressEnv {

        private static string _filePath;
        private static string _location = Npp.GetConfigDir();
        private static string _fileName = "ProgressEnvironnement.xml";
        private static ProgressEnvironnement _currentEnv;
        private static List<ProgressEnvironnement> _listOfEnv = new List<ProgressEnvironnement>();
        private static string _currentProPath;

        /// <summary>
        /// Returns the list of all the progress envrionnements configured
        /// </summary>
        /// <returns></returns>
        public static List<ProgressEnvironnement> GetList() {
            _filePath = Path.Combine(_location, _fileName);
            if (_listOfEnv.Count == 0) {
                if (!File.Exists(_filePath)) {
                    try {
                        Object2Xml<ProgressEnvironnement>.LoadFromString(_listOfEnv, DataResources.ProgressEnvironnement);
                    } catch (Exception e) {
                        ErrorHandler.ShowErrors(e, "Error when loading ProgressEnvironnement.xml", _filePath);
                    }
                } else
                    Object2Xml<ProgressEnvironnement>.LoadFromFile(_listOfEnv, _filePath);
            }
            return _listOfEnv;
        }

        /// <summary>
        /// Saves the list of environnement
        /// </summary>
        public static void Save() {
            if (!string.IsNullOrEmpty(_filePath))
                try {
                    Object2Xml<ProgressEnvironnement>.SaveToFile(_listOfEnv, _filePath);
                } catch (Exception e) {
                    ErrorHandler.ShowErrors(e, "Error when saving ProgressEnvironnement.xml", _filePath);
                }
        }

        /// <summary>
        /// Return the current ProgressEnvironnement object (null if the list is empty!)
        /// </summary>
        public static ProgressEnvironnement Current {
            set {
                _currentEnv = value;
            }
            get {
                if (_currentEnv != null)
                    return _currentEnv;
                var list = GetList();
                _currentEnv = null;
                if (Config.Instance.GlobalCurrentEnvironnement + 1 > list.Count) {
                    _currentEnv = list.Count > 0 ? list[0] : new ProgressEnvironnement();
                } else _currentEnv = list[Config.Instance.GlobalCurrentEnvironnement];
                return _currentEnv;
            }
        }

        /// <summary>
        /// returns the content of the .ini for the current env
        /// </summary>
        public string GetIniContent {
            get { return @"PROPATH=" + GetProPath; }
        }

        /// <summary>
        /// returns the propath value for the current env
        /// </summary>
        public string GetProPath {
            get {
                if (_currentProPath != null) return _currentProPath;
                IniReader ini = new IniReader(Current.IniPath);
                _currentProPath = Current.ProPath + "," + ini.GetValue("PROPATH");
                return _currentProPath;
            }
        }

        /// <summary>
        /// tries to find the specified file in the current propath
        /// returns an empty string if nothing is found
        /// </summary>
        /// <param name="fileTofind"></param>
        /// <returns></returns>
        public string FindFileInPropath(string fileTofind) {
            foreach (var item in GetProPath.Split(',')) {
                string curPath = item;
                // need to take into account relative paths
                if (curPath.StartsWith("."))
                    curPath = Path.GetFullPath(Path.Combine(Npp.GetCurrentFilePath(), curPath));
                curPath = Path.GetFullPath(Path.Combine(curPath, fileTofind));
                if (File.Exists(curPath))
                    return curPath;
            }
            return "";
        }

        public static IEnumerable<string> FilterFiles(string path, params string[] exts) {
            return exts.Select(x => "*." + x) // turn into globs
                .SelectMany(x => Directory.EnumerateFiles(path, x));
        }
    }

    public class ProgressEnvironnement {
        public string Label = "";
        public string Appli = "";
        public string EnvLetter = "";
        public string IniPath = "";
        public string PfPath = "";
        /// <summary>
        /// Propath for compilation time, we can find the .i there
        /// </summary>
        public string ProPath = "";
        public string DataBaseConnection = "";
        public string CmdLineParameters = "";
        /// <summary>
        /// Path to the workarea, we can find the .p, .t, .w there
        /// </summary>
        public string BaseLocalPath = "";
        public string BaseCompilationPath = "";
        public string LogFilePath = "";
        public string VersionId = "";
        public string ProwinPath = "";
        public Dictionary<string, string> CompPath = new Dictionary<string, string>();
    }
}
