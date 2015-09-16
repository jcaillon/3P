using System;
using System.Collections.Generic;
using System.IO;
using YamuiFramework.Themes;
using _3PA.Data;
using _3PA.Lib;

namespace _3PA.MainFeatures {
    public class ProgressEnv {

        private static string _filePath;
        private static string _location = Npp.GetConfigDir();
        private static string _fileName = "ProgressEnvironnement.xml";
        private static ProgressEnvironnement _currentEnv;
        private static List<ProgressEnvironnement> _listOfEnv = new List<ProgressEnvironnement>();

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
                        ErrorHandler.ShowErrors(e, "Error when loading ProgressEnvironnement.xls", _filePath);
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
                    ErrorHandler.ShowErrors(e, "Error when saving ProgressEnvironnement.xls", _filePath);
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
                    if (list.Count > 0) _currentEnv = list[0];
                } else _currentEnv = list[Config.Instance.GlobalCurrentEnvironnement];
                return _currentEnv;
            }
        }
    }

    public class ProgressEnvironnement {
        public string Label = "";
        public string Appli = "";
        public string EnvLetter = "";
        public string IniPath = "";
        public string PfPath = "";
        public string ProPath = "";
        public string DataBaseConnection = "";
        public string CmdLineParameters = "";
        public string BaseLocalPath = "";
        public string BaseCompilationPath = "";
        public string LogFilePath = "";
        public string VersionId = "";
        public string ProwinPath = "";
        public Dictionary<string, string> CompPath = new Dictionary<string, string>();
    }
}
