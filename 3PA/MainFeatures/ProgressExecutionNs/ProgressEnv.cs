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

namespace _3PA.MainFeatures.ProgressExecutionNs {
    public class ProgressEnv {

        private static string _filePath;
        private static readonly string Location = Npp.GetConfigDir();
        private static string _fileName = "ProgressEnvironnement.xml";
        private static ProgressEnvironnement _currentEnv;
        private static List<ProgressEnvironnement> _listOfEnv = new List<ProgressEnvironnement>();
        

        /// <summary>
        /// Returns the list of all the progress envrionnements configured
        /// </summary>
        /// <returns></returns>
        public static List<ProgressEnvironnement> GetList() {
            _filePath = Path.Combine(Location, _fileName);
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
            Current.ReComputeProPath();
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

        /// <summary>
        /// Set .Current object from the values read in Config.Instance.Env...
        /// </summary>
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

            // need to compute the propath again
            Current.ReComputeProPath();
        }

        #region Find file

        /// <summary>
        /// tries to find the specified file in the current propath
        /// returns an empty string if nothing is found, otherwise returns the fullpath of the file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string FindFileInPropath(string fileName) {
            try {
                foreach (var item in Current.GetProPathFileList) {
                    var curPath = Path.Combine(item, fileName);
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
            if (!string.IsNullOrEmpty(propathRes)) return propathRes;
            if (!Directory.Exists(Current.BaseLocalPath)) return "";
            try {
                var fileList = new DirectoryInfo(Current.BaseLocalPath).GetFiles(fileToFind, SearchOption.AllDirectories);
                return fileList.Any() ? fileList.Select(fileInfo => fileInfo.FullName).First() : "";
            } catch (Exception) {
                return "";
            }
        }

        /// <summary>
        /// Returns the fullpath of all the files with the name fileName present either
        /// in the propath (they would be on top of the list) or in the environnement local
        /// base path
        /// You can specify comma separated extensions (ex: .p,.w,.i,.lst) and specifiy an extension-less
        /// fileName to match several files
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="extensions"></param>
        /// <returns></returns>
        public static List<string> FindLocalFiles(string fileName, string extensions = null) {
            var output = new List<string>();
            try {
                if (string.IsNullOrEmpty(extensions)) {
                    var propathFile = FindFileInPropath((fileName));
                    if (!string.IsNullOrEmpty(propathFile))
                        output.Add(propathFile);
                } else {
                    output.AddRange(extensions.Split(',').Select(s => FindFileInPropath(fileName + s)).Where(s => !string.IsNullOrEmpty(s)).ToList());
                }
                output.AddRange(FindAllFiles(Current.BaseLocalPath, fileName, extensions).Where(file => !output.Contains(file, StringComparer.CurrentCultureIgnoreCase)));
            } catch (Exception) {
                // ignored
            }
            return output;
        }

        /// <summary>
        /// Returns the fullpath of all files names fileName in the dirPath
        /// You can specify comma separated extensions (ex: .p,.w,.i,.lst) and specifiy an extension-less
        /// fileName to match several files
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="fileName"></param>
        /// <param name="extensions"></param>
        /// <returns></returns>
        public static List<string> FindAllFiles(string dirPath, string fileName, string extensions = null) {
            var output = new List<string>();
            if (!Directory.Exists(dirPath))
                return output;
            try {
                if (string.IsNullOrEmpty(extensions)) {
                    output.AddRange(new DirectoryInfo(Current.BaseLocalPath).GetFiles(fileName, SearchOption.AllDirectories).Select(info => info.FullName).ToList());
                } else {
                    var dirInfo = new DirectoryInfo(Current.BaseLocalPath);
                    var filesInfo = extensions.Split(',').SelectMany(s => dirInfo.GetFiles(fileName + s, SearchOption.AllDirectories));
                    output.AddRange(filesInfo.Select(info => info.FullName));
                }
            } catch (Exception) {
                // ignored
            }
            return output;
        }

        #endregion

    }

    public class ProgressEnvironnement {
        public string Label = "";
        public string Appli = "";
        public string EnvLetter = "";
        public string IniPath = "";
        public Dictionary<string, string> PfPath = new Dictionary<string, string>();
        /// <summary>
        /// Propath for compilation time, DONT USE THIS ONE THO, use GetProPathFileList instead,
        /// this only returns the extra propath defined by the user!
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


        #region Get ProPath
        /// <summary>
        /// List the existing directories as they are listed in the .ini file + in the custom ProPath field
        /// </summary>
        public List<string> GetProPathFileList {
            get {
                if (_currentProPathDirList != null) return _currentProPathDirList;

                // get full propath (from .ini + from user custom field
                IniReader ini = new IniReader(IniPath);
                var completeProPath = ini.GetValue("PROPATH", "");
                completeProPath = (!string.IsNullOrEmpty(completeProPath) ? completeProPath + "," : string.Empty) + ProPath;

                _currentProPathDirList = new List<string>();
                var curFilePath = Npp.GetCurrentFilePath();
                foreach (var item in completeProPath.Split(',', '\n', ';')) {
                    var propath = item.Trim();
                    // need to take into account relative paths
                    if (propath.StartsWith("."))
                        try {
                            propath = Path.GetFullPath(Path.Combine(curFilePath, propath));
                        } catch (Exception) {
                            // ignored
                        }
                    if (Directory.Exists(propath))
                        _currentProPathDirList.Add(propath);
                }
                return _currentProPathDirList;
            }
        }

        private List<string> _currentProPathDirList;

        /// <summary>
        /// Call this method to compute the propath again the next time we call GetProPathFileList
        /// </summary>
        public void ReComputeProPath() {
            _currentProPathDirList = null;
        }

        #endregion

    }
}
