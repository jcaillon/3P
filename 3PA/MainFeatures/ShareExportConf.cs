#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (ShareExportConf.cs) is part of 3P.
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
using _3PA.Data;
using _3PA.Lib;
using _3PA.MainFeatures.Appli;
using _3PA.MainFeatures.AutoCompletionFeature;
using _3PA.MainFeatures.Parser;
using _3PA.MainFeatures.Pro;

namespace _3PA.MainFeatures {
    internal static class ShareExportConf {
        #region fields

        private static List<ConfLine> _list;

        private static bool _silentUpdate;

        #endregion

        #region public

        /// <summary>
        /// List of the exported/shared items, this allows to automatically build the Export/share config page
        /// Each item corresponds to a line
        /// </summary>
        public static List<ConfLine> List {
            get {
                if (_list == null) {
                    _list = new List<ConfLine> {
                        new ConfLine {
                            Label = "List of environments",
                            HandledItem = Config.FileProEnv,
                            OnFetch = DoFetch,
                            OnPush = DoPush,
                            OnImport = line => ProEnvironment.Import()
                        },
                        new ConfLine {
                            Label = "Deployment profiles",
                            HandledItem = Config.FileDeployProfiles,
                            OnImport = line => DeployProfile.Import(),
                            OnDelete = DoDelete,
                            OnFetch = DoFetch,
                            OnPush = DoPush
                        },
                        new ConfLine {
                            Label = "Deployment rules",
                            HandledItem = Config.FileDeploymentRules,
                            OnImport = line => Deployer.Import(),
                            OnExport = line => Deployer.Export(),
                            OnDelete = DoDelete,
                            OnFetch = DoFetch,
                            OnPush = DoPush
                        },
                        new ConfLine {
                            Label = "Deployment hook procedure",
                            HandledItem = Config.FileDeploymentHook,
                            OnExport = line => Utils.FileWriteAllBytes(Config.FileDeploymentHook, DataResources.DeploymentHook),
                            OnDelete = DoDelete,
                            OnFetch = DoFetch,
                            OnPush = DoPush
                        },
                        new ConfLine {
                            Label = "Prolint startup procedure",
                            HandledItem = Config.FileStartProlint,
                            OnExport = line => Utils.FileWriteAllBytes(Config.FileStartProlint, DataResources.StartProlint),
                            OnDelete = DoDelete,
                            OnFetch = DoFetch,
                            OnPush = DoPush
                        },
                        new ConfLine {
                            Label = "Syntax highlighting themes list",
                            HandledItem = Config.FileSyntaxThemes,
                            OnExport = line => Utils.FileWriteAllBytes(Config.FileSyntaxThemes, DataResources.SyntaxThemes),
                            OnImport = line => Style.ImportList(),
                            OnDelete = DoDelete,
                            OnFetch = DoFetch,
                            OnPush = DoPush
                        },
                        new ConfLine {
                            Label = "Application themes list",
                            HandledItem = Config.FileApplicationThemes,
                            OnExport = line => Utils.FileWriteAllBytes(Config.FileApplicationThemes, DataResources.ApplicationThemes),
                            OnImport = line => ThemeManager.ImportList(),
                            OnDelete = DoDelete,
                            OnFetch = DoFetch,
                            OnPush = DoPush
                        }
                        //new ConfLine {
                        //    Label = "4GL keywords list",
                        //    HandledItem = Config.FileKeywordsList,
                        //    OnImport = ImportKeywords,
                        //    OnDelete = DoDelete,
                        //    OnFetch = DoFetch,
                        //    OnPush = DoPush
                        //},
                        //new ConfLine {
                        //    Label = "4GL keywords help list",
                        //    HandledItem = Config.FileKeywordsHelp,
                        //    OnImport = ImportKeywords,
                        //    OnDelete = DoDelete,
                        //    OnFetch = DoFetch,
                        //    OnPush = DoPush
                        //},
                        //new ConfLine {
                        //    Label = "4GL abbreviations list",
                        //    HandledItem = Config.FileAbbrev,
                        //    OnImport = ImportKeywords,
                        //    OnDelete = DoDelete,
                        //    OnFetch = DoFetch,
                        //    OnPush = DoPush
                        //},
                        //new ConfLine {
                        //    Label = "List of snippets",
                        //    HandledItem = Config.FileSnippets
                        //},
                        //new ConfLine {
                        //    Label = "Templates for new files",
                        //    HandledItem = Config.FolderTemplates,
                        //    IsDir = true,
                        //    OnFetch = DoFetch,
                        //    OnPush = DoPush
                        //}
                    };
                }
                return _list;
            }
        }

        /// <summary>
        /// Returns true if the file is a configuration file listed here
        /// </summary>
        public static bool IsFileExportedConf(string filePath) {
            return List.Exists(line => line.HandledItem.Equals(filePath));
        }

        /// <summary>
        /// Try to import the given configuration file
        /// </summary>
        public static bool TryToImportFile(string filePath) {
            if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath)) {
                var item = List.FirstOrDefault(line => line.HandledItem.Equals(filePath));
                if (item != null) {
                    if (item.OnImport != null)
                        item.OnImport(item);
                    UserCommunication.NotifyUnique("Importedconf", "The latest changes to <b>" + item.Label + "</b> have been saved and taken into account!", MessageImg.MsgInfo, "Configuration imported", item.Label, null, 5);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// ASYNC - Call this method to start checking for updates every xx min, also check once immediatly
        /// </summary>
        public static void StartCheckingForUpdates() {
            // check for updates every now and then (15min)
            // ReSharper disable once ObjectCreationAsStatement
            new ReccurentAction(() => { UpdateList(Config.Instance.SharedConfFolder); }, 1000*60*15);
        }

        /// <summary>
        /// Update the information of the conf list, using the given share directory
        /// </summary>
        public static void UpdateList(string distantShareDirectory) {
            try {
                // We get the latest info for each line
                bool sharedDirOk = false;
                if (!string.IsNullOrEmpty(distantShareDirectory) && Directory.Exists(distantShareDirectory)) {
                    sharedDirOk = true;
                    Config.Instance.SharedConfFolder = distantShareDirectory;
                }

                StringBuilder updateMessage = new StringBuilder();

                // update each line of the list
                foreach (var confLine in List) {
                    // read the autoupdate status from the config
                    confLine.AutoUpdate = Config.Instance.AutoUpdateConfList.ContainsFast(confLine.Label);

                    confLine.LocalPath = confLine.HandledItem;
                    confLine.DistantPath = sharedDirOk ? Path.Combine(distantShareDirectory, confLine.HandledItem.Replace(Npp.GetConfigDir(), "").Trim('\\')) : "";

                    confLine.LocalTime = DateTime.Now;
                    confLine.DistantTime = DateTime.Now;

                    if (confLine.IsDir) {
                        confLine.LocalExists = Directory.Exists(confLine.LocalPath);
                        confLine.DistantExists = !string.IsNullOrEmpty(confLine.DistantPath) && Directory.Exists(confLine.DistantPath);

                        if (confLine.LocalExists) {
                            confLine.LocalNbFiles = 0;
                            foreach (var file in Directory.GetFiles(confLine.LocalPath)) {
                                if (confLine.LocalNbFiles == 0)
                                    confLine.LocalTime = File.GetLastWriteTime(file);
                                else if (File.GetLastWriteTime(file).CompareTo(confLine.LocalTime) > 0)
                                    confLine.LocalTime = File.GetLastWriteTime(file);
                                confLine.LocalNbFiles++;
                            }
                        }

                        if (!string.IsNullOrEmpty(confLine.DistantPath) && confLine.DistantExists) {
                            confLine.DistantNbFiles = 0;
                            foreach (var file in Directory.GetFiles(confLine.DistantPath)) {
                                if (confLine.DistantNbFiles == 0)
                                    confLine.DistantTime = File.GetLastWriteTime(file);
                                else if (File.GetLastWriteTime(file).CompareTo(confLine.DistantTime) > 0)
                                    confLine.DistantTime = File.GetLastWriteTime(file);
                                confLine.DistantNbFiles++;
                            }
                        }
                    } else {
                        confLine.LocalExists = !string.IsNullOrEmpty(confLine.LocalPath) && File.Exists(confLine.LocalPath);
                        confLine.DistantExists = !string.IsNullOrEmpty(confLine.DistantPath) && File.Exists(confLine.DistantPath);

                        if (confLine.LocalExists) {
                            confLine.LocalTime = File.GetLastWriteTime(confLine.LocalPath);
                        }

                        if (!string.IsNullOrEmpty(confLine.DistantPath) && confLine.DistantExists) {
                            confLine.DistantTime = File.GetLastWriteTime(confLine.DistantPath);
                        }
                    }

                    // if the difference between the two dates are small, correct it (it sometimes happen, even when the files are strictly identical)
                    if (Math.Abs(confLine.LocalTime.Subtract(confLine.DistantTime).TotalSeconds) < 2) {
                        confLine.LocalTime = confLine.DistantTime;
                    }

                    confLine.NeedUpdate = confLine.OnFetch != null && ((confLine.DistantExists && !confLine.LocalExists) || (confLine.LocalExists && confLine.DistantExists && confLine.DistantTime.CompareTo(confLine.LocalTime) > 0));

                    // the line needs to be autoupdated
                    if (confLine.AutoUpdate && confLine.NeedUpdate && confLine.OnFetch != null) {
                        _silentUpdate = true;
                        confLine.OnFetch(confLine);
                        confLine.LocalExists = true;
                        confLine.LocalTime = confLine.DistantTime;
                        confLine.LocalNbFiles = confLine.DistantNbFiles;
                        confLine.NeedUpdate = false;
                        _silentUpdate = false;

                        if (updateMessage.Length == 0)
                            updateMessage.Append("The following configuration files have been updated from the shared folder:<br><br>");
                        updateMessage.Append("<div><b>" + confLine.Label + "</b></div>");
                    }
                }

                if (updateMessage.Length > 0) {
                    updateMessage.Append("<br><br><i>You can set which config file gets auto-updated in <a href='go'>the option page</a></i>");
                    UserCommunication.NotifyUnique("ExportConfUpdate", updateMessage.ToString(), MessageImg.MsgInfo, "Update notification", "Configuration auto-update", args => {
                        Appli.Appli.GoToPage(PageNames.ExportShareConf);
                        UserCommunication.CloseUniqueNotif("ExportConfUpdate");
                        args.Handled = true;
                    }, 10);
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error while fetching info on the distant files");
            }
        }

        #endregion

        #region generic methods

        private static void DoDelete(ConfLine conf) {
            var answ = UserCommunication.Message("Do you really want to delete this file?", MessageImg.MsgQuestion, "Delete", "Confirmation", new List<string> {"Yes I do", "No, Cancel"}, true);
            if (answ == 0) {
                Utils.DeleteFile(conf.LocalPath);
                if (conf.OnImport != null)
                    conf.OnImport(conf);
            }
        }

        private static bool _dontWarnFetch;

        private static void DoFetch(ConfLine conf) {
            if (!string.IsNullOrEmpty(conf.DistantPath)) {
                var answ = (_dontWarnFetch || _silentUpdate) ? 0 : UserCommunication.Message("This will <b>replace your local</b> configuration with the distant one.<br><br>Do you wish to continue?", MessageImg.MsgInfo, "Fetch", "Confirmation", new List<string> {"Yes I do", "Yes don't ask again", "No, Cancel"}, true);
                if (answ == 0 || answ == 1) {
                    if (answ == 1)
                        _dontWarnFetch = true;
                    if (conf.IsDir)
                        Utils.CopyDirectory(conf.DistantPath, conf.LocalPath);
                    else {
                        Utils.CopyFile(conf.DistantPath, conf.LocalPath);
                        if (conf.OnImport != null)
                            conf.OnImport(conf);
                    }
                }
            }
        }

        private static bool _dontWarnPush;

        private static void DoPush(ConfLine conf) {
            if (!string.IsNullOrEmpty(conf.LocalPath)) {
                var answ = _dontWarnPush ? 0 : UserCommunication.Message("This will <b>replace the distant configuration <i>(for everyone!)</i></b> with your local configuration.<br><br>Do you wish to continue?", MessageImg.MsgWarning, "Push", "Confirmation", new List<string> {"Yes I do", "Yes don't ask again", "No, Cancel"}, true);
                if (answ == 0 || answ == 1) {
                    if (answ == 1)
                        _dontWarnPush = true;
                    if (conf.IsDir)
                        Utils.CopyDirectory(conf.LocalPath, conf.DistantPath);
                    else
                        Utils.CopyFile(conf.LocalPath, conf.DistantPath);
                }
            }
        }

        private static void ImportKeywords(ConfLine conf) {
            Keywords.Import();
            // Update autocompletion
            AutoCompletion.RefreshStaticItems();
            ParserHandler.ParseCurrentDocument();
        }

        #endregion
    }

    #region ConfLine class

    internal class ConfLine {
        public string HandledItem { get; set; }
        public string Label { get; set; }

        /// <summary>
        /// Action executed when the user click on delete
        /// </summary>
        public Action<ConfLine> OnDelete { get; set; }

        /// <summary>
        /// Action executed when the user click on export
        /// </summary>
        public Action<ConfLine> OnExport { get; set; }

        /// <summary>
        /// Action executed when the user click on fetch
        /// </summary>
        public Action<ConfLine> OnFetch { get; set; }

        /// <summary>
        /// Action executed when the user click on push
        /// </summary>
        public Action<ConfLine> OnPush { get; set; }

        /// <summary>
        /// Action executed when the user click on import
        /// </summary>
        public Action<ConfLine> OnImport { get; set; }

        public DateTime LocalTime { get; set; }
        public DateTime DistantTime { get; set; }
        public bool LocalExists { get; set; }
        public bool DistantExists { get; set; }
        public string LocalPath { get; set; }
        public string DistantPath { get; set; }

        /// <summary>
        /// true if the conf line is actually a directory
        /// </summary>
        public bool IsDir { get; set; }

        public int LocalNbFiles { get; set; }
        public int DistantNbFiles { get; set; }

        /// <summary>
        /// true if the user checked this option to automatically update this conf line
        /// </summary>
        public bool AutoUpdate { get; set; }

        public bool NeedUpdate { get; set; }
    }

    #endregion
}