#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProgressEnv.cs) is part of 3P.
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
                    if (Config.Instance.UserFromSopra) {
                        try {
                            Object2Xml<ProgressEnvironnement>.LoadFromString(_listOfEnv, DataResources.ProgressEnvironnement);
                        } catch (Exception e) {
                            ErrorHandler.ShowErrors(e, "Error when loading ProgressEnvironnement.xml", _filePath);
                        }
                    }
                    if (_listOfEnv == null || _listOfEnv.Count == 0) {
                        _listOfEnv = new List<ProgressEnvironnement>();
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
                    ErrorHandler.ShowErrors(e, "Error when saving ProgressEnvironnement.xml");
                }

            // need to compute the propath again
            ResetPropath();
        }

        /// <summary>
        /// Return the current ProgressEnvironnement object (null if the list is empty!)
        /// </summary>
        public static ProgressEnvironnement Current {
            get {
                if (_currentEnv != null)
                    return _currentEnv;
                SetCurrent();
                return _currentEnv;
            }
        }

        public static void SetCurrent() {
            // determines the current item selected in the envList
            var envList = GetList();
            try {
                try {
                    _currentEnv = envList.First(environnement =>
                        environnement.Appli.EqualsCi(Config.Instance.EnvCurrentAppli) &&
                        environnement.EnvLetter.EqualsCi(Config.Instance.EnvCurrentEnvLetter));
                } catch (Exception) {
                    _currentEnv = envList.First(environnement =>
                        environnement.Appli.EqualsCi(Config.Instance.EnvCurrentAppli));
                }
            } catch (Exception) {
                _currentEnv = envList.Count > 0 ? envList[0] : new ProgressEnvironnement();
            }
            ResetPropath();
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
        public static string GetProPath {
            get {
                if (_currentProPath != null) return _currentProPath;
                IniReader ini = new IniReader(Current.IniPath);
                var propath = ini.GetValue("PROPATH", "");
                _currentProPath = (!string.IsNullOrEmpty(propath) ? propath + "," : String.Empty) + Current.ProPath;
                return _currentProPath;
            }
        }

        /// <summary>
        /// Call the is method to recompute the propath next time we need it
        /// </summary>
        public static void ResetPropath() {
            _currentProPath = null;
        }

        /// <summary>
        /// tries to find the specified file in the current propath
        /// returns an empty string if nothing is found
        /// </summary>
        /// <param name="fileTofind"></param>
        /// <returns></returns>
        public static string FindFileInPropath(string fileTofind) {
            try {
                foreach (var item in GetProPath.Split(',')) {
                    string curPath = item;
                    // need to take into account relative paths
                    if (curPath.StartsWith("."))
                        curPath = Path.GetFullPath(Path.Combine(Npp.GetCurrentFilePath(), curPath));
                    curPath = Path.GetFullPath(Path.Combine(curPath, fileTofind));
                    if (File.Exists(curPath))
                        return curPath;
                }
            } catch (Exception) {
                return "";
            }
            return "";
        }

        /// <summary>
        /// Find a file in the propath and if it can't find it, in the env base local path
        /// </summary>
        /// <param name="fileToFind"></param>
        /// <returns></returns>
        public static string FindFirstFileInEnv(string fileToFind) {
            var propathRes = FindFileInPropath(fileToFind);
            try {
                if (string.IsNullOrEmpty(propathRes) && Directory.Exists(Current.BaseLocalPath)) {
                    var fileList = new DirectoryInfo(Current.BaseLocalPath).GetFiles(fileToFind, SearchOption.AllDirectories);
                    if (fileList.Any())
                        return fileList.Select(fileInfo => fileInfo.FullName).First();
                    return "";
                }
            } catch (Exception) {
                return "";
            }
            return propathRes;
        } 

        public static List<string> FilterFiles(string path, params string[] exts) {
            return exts.Select(x => "*." + x) // turn into globs
                .SelectMany(x => Directory.EnumerateFiles(path, x)).ToList();
        }
    }

    public class ProgressEnvironnement {
        public string Label = "";
        public string Appli = "";
        public string EnvLetter = "";
        public string IniPath = "";
        public Dictionary<string, string> PfPath = new Dictionary<string, string>();
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

        /// <summary>
        /// Returns the currently selected database's .pf for the current environment
        /// </summary>
        /// <returns></returns>
        public string GetCurrentPfPath() {
            return PfPath.ContainsKey(Config.Instance.EnvCurrentDatabase) ?
                PfPath[Config.Instance.EnvCurrentDatabase] :
                PfPath.FirstOrDefault().Value;
        }
    }
}
