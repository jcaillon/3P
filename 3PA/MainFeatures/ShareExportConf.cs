using System;
using System.Collections.Generic;
using System.IO;
using _3PA.Html;
using _3PA.Lib;
using _3PA.MainFeatures.ProgressExecutionNs;

namespace _3PA.MainFeatures {

    internal static class ShareExportConf {

        #region fields

        private static List<ConfLine> _list;

        #endregion

        #region public

        /// <summary>
        /// List of the exported/shared items
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
                            Label = "List of snippets",
                            HandledItem = Config.FileSnippets
                        },
                        new ConfLine {
                            Label = "Compilation path rerouting",
                            HandledItem = Config.FileCompilPath
                        },
                        new ConfLine {
                            Label = "Start prolint procedure",
                            HandledItem = Config.FileStartProlint,
                            OnDelete = DoDelete,
                            OnFetch = DoFetch,
                            OnPush = DoPush
                        },
                        new ConfLine {
                            Label = "4GL keywords list",
                            HandledItem = Config.FileKeywordsList
                        },
                        new ConfLine {
                            Label = "4GL keywords help list",
                            HandledItem = Config.FileKeywordsHelp
                        },
                        new ConfLine {
                            Label = "4GL abbreviations list",
                            HandledItem = Config.FileAbbrev
                        },
                        new ConfLine {
                            Label = "Templates for new files",
                            HandledItem = Config.FolderTemplates,
                            IsDir = true,
                            OnFetch = DoFetch,
                            OnPush = DoPush
                        }
                    };
                }
                return _list;
            }
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

                // update each line of the list
                foreach (var confLine in List) {
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
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error while fetching info on the distant files");
            }
        }

        #endregion


        #region generic methods

        private static void DoDelete(ConfLine conf) {
            var answ = UserCommunication.Message("Do you really want to delete this file, or did your finger slipped?", MessageImg.MsgQuestion, "Delete", "Confirmation", new List<string> { "Yes I do", "No, Cancel" }, true);
            if (answ == 0) {
                Utils.DeleteFile(conf.LocalPath);
            }
        }

        private static void DoFetch(ConfLine conf) {
            if (!string.IsNullOrEmpty(conf.DistantPath)) {
                if (conf.IsDir)
                    Utils.CopyDirectory(conf.DistantPath, conf.LocalPath);
                else {
                    Utils.CopyFile(conf.DistantPath, conf.LocalPath);
                    if (conf.OnImport != null)
                        conf.OnImport(conf);
                }
            }
        }

        private static void DoPush(ConfLine conf) {
            if (!string.IsNullOrEmpty(conf.LocalPath)) {
                if (conf.IsDir)
                    Utils.CopyDirectory(conf.LocalPath, conf.DistantPath);
                else
                    Utils.CopyFile(conf.LocalPath, conf.DistantPath);
            }
        }

        #endregion

    }

    #region ConfLine class

    internal class ConfLine {
        public string HandledItem { get; set; }
        public string Label { get; set; }
        public Action<ConfLine> OnDelete { get; set; }
        public Action<ConfLine> OnExport { get; set; }
        public Action<ConfLine> OnFetch { get; set; }
        public Action<ConfLine> OnPush { get; set; }
        public Action<ConfLine> OnImport { get; set; }
        public DateTime LocalTime { get; set; }
        public DateTime DistantTime { get; set; }
        public bool LocalExists { get; set; }
        public bool DistantExists { get; set; }
        public string LocalPath { get; set; }
        public string DistantPath { get; set; }
        public bool IsDir { get; set; }
        public int LocalNbFiles { get; set; }
        public int DistantNbFiles { get; set; }
    }

    #endregion
}
