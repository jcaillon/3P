#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProEnvironment.cs) is part of 3P.
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
using _3PA.Lib;

namespace _3PA.MainFeatures.ProgressExecutionNs {

    internal static class ProEnvironment {

        #region fields

        private static ProEnvironmentObject _currentEnv;
        private static List<ProEnvironmentObject> _listOfEnv = new List<ProEnvironmentObject>();

        #endregion

        #region public manage env

        /// <summary>
        /// Returns the list of all the progress envrionnements configured
        /// </summary>
        /// <returns></returns>
        public static List<ProEnvironmentObject> GetList {
            get {
                if (_listOfEnv.Count == 0) {
                    if (!File.Exists(Config.FileProEnv)) {
                        _listOfEnv = new List<ProEnvironmentObject> {new ProEnvironmentObject {Name = "Default", Label = "A default environment (empty)"}};
                    } else
                        Object2Xml<ProEnvironmentObject>.LoadFromFile(_listOfEnv, Config.FileProEnv);
                }
                return _listOfEnv;
            }
        }

        /// <summary>
        /// Saves the list of environnement
        /// </summary>
        public static void SaveList() {
            // sort by appli then envletter
            _listOfEnv.Sort((env1, env2) => {
                var comp = string.Compare(env1.Name, env2.Name, StringComparison.CurrentCultureIgnoreCase);
                return comp == 0 ? string.Compare(env1.Suffix, env2.Suffix, StringComparison.CurrentCultureIgnoreCase) : comp;
            });
            if (!string.IsNullOrEmpty(Config.FileProEnv)) {
                try {
                    Object2Xml<ProEnvironmentObject>.SaveToFile(_listOfEnv, Config.FileProEnv);
                } catch (Exception e) {
                    ErrorHandler.ShowErrors(e, "Error when saving ProgressEnvironnement.xml");
                }
            }
        }

        /// <summary>
        /// Saves an environment either by creating a new one (before == null) or 
        /// replacing an old one
        /// </summary>
        /// <param name="before"></param>
        /// <param name="after"></param>
        public static void SaveEnv(ProEnvironmentObject before, ProEnvironmentObject after) {
            if (before != null) {
                var index = _listOfEnv.FindIndex(environnement =>
                    environnement.Name.EqualsCi(before.Name) &&
                    environnement.Suffix.EqualsCi(before.Suffix));
                if (index > -1) {
                    _listOfEnv.RemoveAt(index);
                }
            }
            _listOfEnv.Add(after);
        }

        /// <summary>
        /// Deletes the current environment from the list
        /// </summary>
        public static void DeleteCurrentEnv() {
            var index = _listOfEnv.FindIndex(environnement =>
                environnement.Name.EqualsCi(Current.Name) &&
                environnement.Suffix.EqualsCi(Current.Suffix));
            if (index > -1) {
                _listOfEnv.RemoveAt(index);
            }
        }

        /// <summary>
        /// Return the current ProgressEnvironnement object (null if the list is empty!)
        /// </summary>
        public static ProEnvironmentObject Current {
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
            var envList = GetList;
            _currentEnv = envList.FirstOrDefault(environnement =>
                environnement.Name.EqualsCi(Config.Instance.EnvName) &&
                environnement.Suffix.EqualsCi(Config.Instance.EnvSuffix));
            if (_currentEnv == null) {
                _currentEnv = envList.FirstOrDefault(environnement =>
                    environnement.Name.EqualsCi(Config.Instance.EnvName));
            }
            if (_currentEnv == null) {
                _currentEnv = envList.Count > 0 ? envList[0] : new ProEnvironmentObject();
            }
            // set database
            if (!_currentEnv.DbConnectionInfo.ContainsKey(Config.Instance.EnvDatabase))
                Config.Instance.EnvDatabase = (_currentEnv.DbConnectionInfo.Count > 0) ? _currentEnv.DbConnectionInfo.First().Key : String.Empty;
            // need to compute the propath again
            Current.ReComputeProPath();
        }

        #endregion

        #region Find file

        /// <summary>
        /// tries to find the specified file in the current propath
        /// returns an empty string if nothing is found, otherwise returns the fullpath of the file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string FindFileInPropath(string fileName) {
            try {
                foreach (var item in Current.GetProPathDirList) {
                    var curPath = Path.Combine(item, fileName);
                    if (File.Exists(curPath))
                        return curPath;
                }
            } catch (Exception x) {
                ErrorHandler.DirtyLog(x);
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
            } catch (Exception x) {
                ErrorHandler.DirtyLog(x);
            }
            return "";
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
            } catch (Exception x) {
                ErrorHandler.DirtyLog(x);
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
            } catch (Exception x) {
                ErrorHandler.DirtyLog(x);
            }
            return output;
        }

        #endregion

        #region ProEnvironmentObject

        public class ProEnvironmentObject {

            // prim key
            public string Name = "";
            public string Suffix = "";

            // label
            public string Label = "";

            // pf
            public Dictionary<string, string> DbConnectionInfo = new Dictionary<string, string>();
            public string ExtraPf = "";

            // propath
            public string IniPath = "";
            public string ExtraProPath = "";

            public string CmdLineParameters = "";

            /// <summary>
            /// Path to the workarea, we can find the .p, .t, .w there
            /// </summary>
            public string BaseLocalPath = "";

            public string BaseCompilationPath = "";
            public string ProwinPath = "";
            public string LogFilePath = "";

            /// <summary>

            #region Handle pf

            /// Returns the currently selected database's .pf for the current environment
            /// </summary>
            /// <returns></returns>
            public string GetPfPath() {
                return DbConnectionInfo.ContainsKey(Config.Instance.EnvDatabase) ?
                    DbConnectionInfo[Config.Instance.EnvDatabase] :
                    string.Empty;
            }

            public bool RemoveCurrentPfPath() {
                if (DbConnectionInfo.ContainsKey(Config.Instance.EnvDatabase)) {
                    DbConnectionInfo.Remove(Config.Instance.EnvDatabase);
                    return true;
                }
                return false;
            }

            public bool AddPfPath(string name, string path) {
                if (!DbConnectionInfo.ContainsKey(name)) {
                    DbConnectionInfo.Add(name, path);
                    return true;
                }
                return false;
            }

            #endregion

            #region Get ProPath

            /// <summary>
            /// List the existing directories as they are listed in the .ini file + in the custom ProPath field
            /// </summary>
            public List<string> GetProPathDirList {
                get {
                    if (_currentProPathDirList != null) return _currentProPathDirList;

                    // get full propath (from .ini + from user custom field
                    IniReader ini = new IniReader(IniPath);
                    var completeProPath = ini.GetValue("PROPATH", "");
                    completeProPath = (!string.IsNullOrEmpty(completeProPath) ? completeProPath + "," : string.Empty) + ExtraProPath;
                    // also add the source file base path
                    completeProPath = completeProPath + ",.";

                    _currentProPathDirList = new List<string>();
                    var curFilePath = Npp.GetCurrentFileFolder();
                    foreach (var item in completeProPath.Split(',', '\n', ';')) {
                        var propath = item.Trim();
                        // need to take into account relative paths
                        if (propath.StartsWith("."))
                            try {
                                propath = Path.GetFullPath(Path.Combine(curFilePath, propath));
                            } catch (Exception x) {
                                ErrorHandler.DirtyLog(x);
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

        #endregion

    }

    
}
