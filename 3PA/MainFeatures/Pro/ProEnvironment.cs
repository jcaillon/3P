#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using _3PA.Lib;
using _3PA.NppCore;

namespace _3PA.MainFeatures.Pro {
    internal class ProEnvironment {

        #region ProEnvironmentObject

        [Serializable]
        internal class ProEnvironmentObject {

            #region Exported fields

            /// <summary>
            /// IF YOU ADD A FIELD, DO NOT FORGET TO ALSO ADD THEM IN THE HARD COPY CONSTRUCTOR!!!
            /// </summary>

            // primary key
            public string Name = "";

            public string Suffix = "";
            
            // label
            public string Label = "";

            // pf
            public Dictionary<string, string> DbConnectionInfo = new Dictionary<string, string>();
            public string CurrentDb = "";
            public string ExtraPf = "";

            // propath
            public string IniPath = "";
            public string ExtraProPath = "";

            /// <summary>
            /// Path to the workarea, we can find the .p, .t, .w there
            /// </summary>
            public string BaseLocalPath = "";

            /// <summary>
            /// Deployment directory
            /// </summary>
            [XmlElement(ElementName = "BaseCompilationPath")]
            public string BaseCompilationPath = "";

            public string ProwinPath = "";
            public string CmdLineParameters = "";
            public string LogFilePath = "";

            public bool CompileLocally = true;

            public string PreExecutionProgram = "";
            public string PostExecutionProgram = "";

            #endregion

            #region private fields

            /// <summary>
            /// Finding files in directories is actually a task that can take a long time,
            /// if we get a match, we save it here so the next time we look for the file,
            /// we already know its full path
            /// </summary>
            private Dictionary<string, string> _savedFoundFiles = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

            private List<string> _currentProPathDirList;

            private Deployer _deployer;

            #endregion

            #region constructor

            public ProEnvironmentObject() {
                // need to erase the stored ProPath (and re-compute when needed) when the current environment is modified
                OnEnvironmentChange += ReComputeProPath;
            }

            /// <summary>
            /// To create a hard copy of this object
            /// </summary>
            public ProEnvironmentObject(ProEnvironmentObject toCopy) {
                Name = toCopy.Name;
                Suffix = toCopy.Suffix;
                Label = toCopy.Label;
                DbConnectionInfo = toCopy.DbConnectionInfo;
                CurrentDb = toCopy.CurrentDb;
                ExtraPf = toCopy.ExtraPf;
                IniPath = toCopy.IniPath;
                ExtraProPath = toCopy.ExtraProPath;

                BaseLocalPath = toCopy.BaseLocalPath;
                BaseCompilationPath = toCopy.BaseCompilationPath;

                ProwinPath = toCopy.ProwinPath;
                CmdLineParameters = toCopy.CmdLineParameters;
                LogFilePath = toCopy.LogFilePath;

                CompileLocally = toCopy.CompileLocally;

                PostExecutionProgram = toCopy.PostExecutionProgram;
                PreExecutionProgram = toCopy.PreExecutionProgram;

                _currentProPathDirList = toCopy._currentProPathDirList;

                // deployer copy
                _deployer = toCopy.Deployer;
            }

            #endregion

            #region Handle connection string

            public string GetCurrentDb() {
                if (string.IsNullOrEmpty(CurrentDb) && DbConnectionInfo.Count > 0) {
                    CurrentDb = DbConnectionInfo.First().Key;
                }
                return CurrentDb;
            }

            /// <summary>
            /// Returns the currently selected database's .pf for the current environment
            /// </summary>
            public string GetPfPath() {
                CurrentDb = GetCurrentDb();
                if (!string.IsNullOrEmpty(CurrentDb)) {
                    if (DbConnectionInfo.ContainsKey(CurrentDb))
                        return DbConnectionInfo[CurrentDb];

                    CurrentDb = DbConnectionInfo.First().Key;
                    return DbConnectionInfo[CurrentDb];
                }
                return string.Empty;
            }

            public bool RemoveCurrentPfPath() {
                if (!string.IsNullOrEmpty(CurrentDb) && DbConnectionInfo.ContainsKey(CurrentDb)) {
                    DbConnectionInfo.Remove(CurrentDb);
                    SetCurrent(null, null, null);
                    return true;
                }
                return false;
            }

            public bool AddPfPath(string name, string path) {
                if (!string.IsNullOrEmpty(name) && !DbConnectionInfo.ContainsKey(name)) {
                    DbConnectionInfo.Add(name, path);
                    SetCurrent(null, null, name);
                    return true;
                }
                return false;
            }

            public bool ModifyPfPath(string name, string path) {
                if (!string.IsNullOrEmpty(name) && (!DbConnectionInfo.ContainsKey(name) || name.Equals(CurrentDb))) {
                    DbConnectionInfo.Remove(CurrentDb);
                    DbConnectionInfo.Add(name, path);
                    SetCurrent(null, null, name);
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Returns the database connection string (complete with .pf + extra)
            /// </summary>
            public string ConnectionString {
                get {
                    var connectionString = new StringBuilder();
                    if (File.Exists(GetPfPath())) {
                        Utils.ForEachLine(GetPfPath(), new byte[0], (nb, line) => {
                            var commentPos = line.IndexOf("#", StringComparison.CurrentCultureIgnoreCase);
                            if (commentPos == 0)
                                return;
                            if (commentPos > 0)
                                line = line.Substring(0, commentPos);
                            line = line.Trim();
                            if (!string.IsNullOrEmpty(line)) {
                                connectionString.Append(" ");
                                connectionString.Append(line);
                            }
                        });
                        connectionString.Append(" ");
                    }
                    connectionString.Append(ExtraPf.Trim());
                    return connectionString.ToString().Replace("\n", " ").Replace("\r", "");
                }
            }
            
            /// <summary>
            /// Use this method to know if the CONNECT define for the current environment connects the database in
            /// single user mode (returns false if not or if no database connection is set)
            /// </summary>
            /// <returns></returns>
            public bool IsDatabaseSingleUser {
                get { return ConnectionString.RegexMatch(@"\s-1", RegexOptions.Singleline); }
            }

            #endregion

            #region Get ProPath

            /// <summary>
            /// Call this method to compute the propath again the next time we call GetProPathFileList
            /// </summary>
            public void ReComputeProPath() {
                _currentProPathDirList = null;
                _savedFoundFiles.Clear();
            }

            /// <summary>
            /// List the existing directories as they are listed in the .ini file + in the custom ProPath field,
            /// this returns an exhaustive list of EXISTING folders and .pl files and ensure each item is present only once
            /// It also take into account the relative path, using the BaseLocalPath (or currentFileFolder)
            /// </summary>
            public List<string> GetProPathDirList {
                get {
                    if (_currentProPathDirList == null) {
                        var curFilePath = Npp.CurrentFile.DirectoryName;
                        var basePath = (!string.IsNullOrEmpty(BaseLocalPath) && Directory.Exists(BaseLocalPath)) ? BaseLocalPath : curFilePath;

                        // get full propath (from .ini + from user custom field + current file folder)
                        IniReader ini = new IniReader(IniPath);
                        var completeProPath = ini.GetValue("PROPATH", "");
                        completeProPath = curFilePath + "," + completeProPath + "," + ExtraProPath;

                        var uniqueDirList = new HashSet<string>();
                        foreach (var item in completeProPath.Split(',', '\n', ';')) {
                            var propath = item.Trim();
                            if (!string.IsNullOrEmpty(propath)) {
                                // need to take into account relative paths
                                if (!Path.IsPathRooted(propath))
                                    try {
                                        if (propath.Contains("%"))
                                            propath = Environment.ExpandEnvironmentVariables(propath);
                                        propath = Path.GetFullPath(Path.Combine(basePath, propath));
                                    } catch (Exception e) {
                                        ErrorHandler.LogError(e);
                                    }
                                if (Directory.Exists(propath) || File.Exists(propath))
                                    if (!uniqueDirList.Contains(propath))
                                        uniqueDirList.Add(propath);
                            }
                        }
                        _currentProPathDirList = uniqueDirList.ToList();
                    }
                    return _currentProPathDirList;
                }
            }

            #endregion

            #region Find file

            /// <summary>
            /// tries to find the specified file in the current propath
            /// returns an empty string if nothing is found, otherwise returns the fullpath of the file
            /// </summary>
            public string FindFirstFileInPropath(string fileName) {
                if (_savedFoundFiles.ContainsKey(fileName))
                    return _savedFoundFiles[fileName];

                try {
                    foreach (var item in GetProPathDirList) {
                        var curPath = Path.Combine(item, fileName);
                        if (File.Exists(curPath)) {
                            _savedFoundFiles.Add(fileName, curPath);
                            return curPath;
                        }
                    }
                } catch (Exception) {
                    // The path in invalid, well we don't really care
                }
                return "";
            }

            /// <summary>
            /// Find a file in the propath and if it can't find it, in the env base local path
            /// </summary>
            public string FindFirstFileInEnv(string fileToFind) {
                if (_savedFoundFiles.ContainsKey(fileToFind))
                    return _savedFoundFiles[fileToFind];

                // find in propath
                var propathRes = FindFirstFileInPropath(fileToFind);
                if (!string.IsNullOrEmpty(propathRes))
                    return propathRes;

                // find in local files
                var listFilePathLoc = FindAllFiles(BaseLocalPath, fileToFind);
                if (listFilePathLoc.Any()) {
                    _savedFoundFiles.Add(fileToFind, listFilePathLoc.First());
                    return listFilePathLoc.First();
                }

                return "";
            }

            /// <summary>
            /// Returns the fullpath of all the files with the name fileName present either
            /// in the propath (they would be on top of the list) or in the environment local
            /// base path
            /// You can specify comma separated extensions (ex: .p,.w,.i,.lst) and specify an extension-less
            /// fileName to match several files
            /// </summary>
            public List<string> FindFiles(string fileName, string extensions = null) {
                var output = new List<string>();
                try {
                    // search in propath
                    if (string.IsNullOrEmpty(extensions)) {
                        var propathFile = FindFirstFileInPropath((fileName));
                        if (!string.IsNullOrEmpty(propathFile))
                            output.Add(propathFile);
                    } else {
                        output.AddRange(extensions.Split(',').Select(s => FindFirstFileInPropath(fileName + s)).Where(s => !string.IsNullOrEmpty(s)).ToList());
                    }

                    // search in local folder, do not add the same file twice
                    output.AddRange(FindAllFiles(BaseLocalPath, fileName, extensions).Where(file => !output.Contains(file, StringComparer.CurrentCultureIgnoreCase)));
                } catch (Exception e) {
                    if (!(e is DirectoryNotFoundException))
                        ErrorHandler.LogError(e);
                }
                return output;
            }

            /// <summary>
            /// Returns the fullpath of all files named fileName in the dirPath
            /// You can specify comma separated extensions (ex: .p,.w,.i,.lst) and specify an extension-less
            /// fileName to match several files
            /// </summary>
            public List<string> FindAllFiles(string dirPath, string fileName, string extensions = null) {
                var output = new List<string>();
                try {
                    if (!Directory.Exists(dirPath))
                        return output;
                    if (string.IsNullOrEmpty(extensions)) {
                        output.AddRange(Directory.EnumerateFiles(dirPath, fileName, SearchOption.AllDirectories));
                    } else {
                        output.AddRange(extensions.Split(',').SelectMany(s => Directory.EnumerateFiles(dirPath, fileName + s, SearchOption.AllDirectories)).ToList());
                    }
                } catch (Exception e) {
                    if (!(e is DirectoryNotFoundException))
                        ErrorHandler.LogError(e);
                }
                return output;
            }

            #endregion

            #region Deploy

            public string DeploymentRulesFilePath {
                get { return Config.FileDeploymentRules; }
            }

            /// <summary>
            /// The deployer for this environment (can either be a new one, or a copy of this proenv is, itself, a copy)
            /// </summary>
            public Deployer Deployer {
                get { return _deployer != null ? _deployer : new Deployer(DeploymentRulesFilePath, this); }
            }

            #endregion

            #region Misc

            /// <summary>
            /// Returns the path to prolib.exe considering the path to prowin.exe
            /// </summary>
            public string ProlibPath {
                get { return string.IsNullOrEmpty(ProwinPath) ? "" : Path.Combine(Path.GetDirectoryName(ProwinPath) ?? "", @"prolib.exe"); }
            }

            // use C:\progress\client\v1160_dv\dlc\version to know the version? -> MESSAGE PROVERSION
            public bool CanProwinUseNoSplash {
                get { return (ProwinPath ?? "").Contains("1160"); }
            }

            #endregion
        }

        #endregion

        #region Static

        #region events

        /// <summary>
        /// Subscribe to this event, published when the current environment changes (and when the list / env is modified)
        /// </summary>
        public static event Action OnEnvironmentChange;

        #endregion

        #region fields

        private static ProEnvironmentObject _currentEnv;
        private static List<ProEnvironmentObject> _listOfEnv = new List<ProEnvironmentObject>();

        #endregion

        #region public manage env

        /// <summary>
        /// To call when the .xml file has changed and you want to reload it
        /// </summary>
        public static void Import() {
            _listOfEnv.Clear();
            _currentEnv = null;
            Current.ReComputeProPath();
        }

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
        public static void Modify(ProEnvironmentObject before, ProEnvironmentObject after) {
            if (before != null) {
                var index = _listOfEnv.FindIndex(environnement =>
                    environnement.Name.EqualsCi(before.Name) &&
                    environnement.Suffix.EqualsCi(before.Suffix));
                if (index > -1) {
                    _listOfEnv.RemoveAt(index);
                }
            }
            _listOfEnv.Add(after);
            SetCurrent(after.Name, after.Suffix, null);
        }

        /// <summary>
        /// Deletes the current environment from the list
        /// </summary>
        public static void DeleteCurrent() {
            var index = _listOfEnv.FindIndex(environnement =>
                environnement.Name.EqualsCi(Current.Name) &&
                environnement.Suffix.EqualsCi(Current.Suffix));
            if (index > -1) {
                _listOfEnv.RemoveAt(index);
            }
            SetCurrent(null, null, null);
        }

        /// <summary>
        /// Return the current ProgressEnvironnement object (null if the list is empty!)
        /// </summary>
        public static ProEnvironmentObject Current {
            get {
                if (_currentEnv == null)
                    SetCurrent(null, null, null);
                return _currentEnv;
            }
        }

        /// <summary>
        /// Change the current environment
        /// </summary>
        public static void SetCurrent(string name, string suffix, string database) {
            // determines the current item selected in the envList
            var envList = GetList;
            _currentEnv = envList.FirstOrDefault(environnement =>
                environnement.Name.EqualsCi(name ?? Config.Instance.EnvName) &&
                environnement.Suffix.EqualsCi(suffix ?? Config.Instance.EnvSuffix));
            if (_currentEnv == null) {
                _currentEnv = envList.FirstOrDefault(environnement =>
                    environnement.Name.EqualsCi(name ?? Config.Instance.EnvName));
            }
            if (_currentEnv == null) {
                _currentEnv = envList.Count > 0 ? envList[0] : new ProEnvironmentObject();
            }

            Config.Instance.EnvName = _currentEnv.Name;
            Config.Instance.EnvSuffix = _currentEnv.Suffix;

            // set database
            if (!string.IsNullOrEmpty(database) && _currentEnv.DbConnectionInfo.ContainsKey(database))
                _currentEnv.CurrentDb = database;

            if (OnEnvironmentChange != null)
                OnEnvironmentChange();
        }

        #endregion

        #endregion

    }
}