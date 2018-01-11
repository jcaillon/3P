#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (NppConf.cs) is part of 3P.
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
using YamuiFramework.Forms;
using _3PA.Lib;
using _3PA.Lib._3pUpdater;
using _3PA.MainFeatures;
using _3PA._Resource;

namespace _3PA.NppCore {

    internal static partial class Npp {

        private static NppConf _nppconfig;

        /// <summary>
        /// Get instance
        /// </summary>
        public static NppConf ConfXml {
            get {
                if (_nppconfig == null || Utils.HasFileChanged(_nppconfig.FileNppConfigXml)) {
                    _nppconfig = new NppConf();
                }
                return _nppconfig;
            }
        }

        /// <summary>
        /// This class allows to get properties read from several config files of npp :
        /// config.xml
        /// and stylers.xml (or whichever style .xml is used at the moment)
        /// </summary>
        internal class NppConf {

            #region Private

            private string _fileNppStylersXml;

            /// <summary>
            /// location of all the basic configuration files, they can either be...
            /// in the installation folder if doLocalConf.xml is here
            /// in %appdata% otherwise
            /// or in the folder described in cloud/choice
            /// </summary>
            private string FolderBaseConf { get; set; }

            #endregion

            #region Properties

            /// <summary>
            /// Equals 0 if the default autocompletion is deactivated
            /// </summary>
            public int AutocompletionMode { get; private set; }

            /// <summary>
            /// Equals 0 if deactivated, 1 if simple backup, 2 for verbose backup
            /// </summary>
            public int BackupMode { get; private set; }

            /// <summary>
            /// Use a custom directory for backup or not
            /// </summary>
            public bool BackupUseCustomDir { get; private set; }

            /// <summary>
            /// Path to the backup directory
            /// </summary>
            public string BackupDirectory { get { return Path.Combine(FolderBaseConf, "backup"); } }

            /// <summary>
            /// Path to the backup directory
            /// </summary>
            public string CustomBackupDirectory { get; private set; }

            /// <summary>
            /// is the multi selection enabled or not?
            /// </summary>
            public bool MultiSelectionEnabled { get; private set; }

            public string FileNppUserDefinedLang {
                get { return Path.Combine(FolderBaseConf, @"userDefineLang.xml"); }
            }

            public string FileNppConfigXml {
                get { return Path.Combine(FolderBaseConf, @"config.xml"); }
            }

            public string FileNppStylersXml {
                get { return _fileNppStylersXml ?? Path.Combine(FolderBaseConf, @"stylers.xml"); }
                private set { _fileNppStylersXml = value; }
            }

            public string FileNppLangsXml {
                get { return Path.Combine(FolderBaseConf, @"langs.xml"); }
            }

            public string WordCharList { get; set; }

            #endregion

            #region Life and death

            /// <summary>
            /// Constructor
            /// </summary>
            public NppConf() {
                Reload();
            }

            #endregion

            #region public

            /// <summary>
            /// Allows to reload the npp configuration from the files
            /// </summary>
            public void Reload() {

                // get the base folder
                FolderBaseConf = FolderNppDefaultBaseConf;
                if (File.Exists(FileNppCloudChoice)) {
                    var cloudpath = Utils.ReadAllText(FileNppCloudChoice, Encoding.Default);
                    if (Directory.Exists(cloudpath)) {
                        FolderBaseConf = cloudpath;
                    }
                }

                // Get info from the config.xml
                FileNppStylersXml = null;

                if (File.Exists(FileNppConfigXml)) {
                    try {
                        var configs = new NanoXmlDocument(Utils.ReadAllText(FileNppConfigXml)).RootNode["GUIConfigs"].SubNodes;
                        FileNppStylersXml = configs.FirstOrDefault(x => x.GetAttribute("name").Value.Equals("stylerTheme")).GetAttribute("path").Value;
                        AutocompletionMode = int.Parse(configs.FirstOrDefault(x => x.GetAttribute("name").Value.Equals("auto-completion")).GetAttribute("autoCAction").Value);
                        CustomBackupDirectory = configs.FirstOrDefault(x => x.GetAttribute("name").Value.Equals("Backup")).GetAttribute("dir").Value;
                        BackupUseCustomDir = configs.FirstOrDefault(x => x.GetAttribute("name").Value.Equals("Backup")).GetAttribute("useCustumDir").Value.EqualsCi("yes");
                        BackupMode = int.Parse(configs.FirstOrDefault(x => x.GetAttribute("name").Value.Equals("Backup")).GetAttribute("action").Value);
                        MultiSelectionEnabled = configs.FirstOrDefault(x => x.GetAttribute("name").Value.Equals("ScintillaGlobalSettings")).GetAttribute("enableMultiSelection").Value.EqualsCi("yes");

                        var wordCharListCfg = configs.FirstOrDefault(x => x.GetAttribute("name").Value.Equals("wordCharList"));
                        if (wordCharListCfg != null && wordCharListCfg.GetAttribute("useDefault").Value.EqualsCi("no")) {
                            WordCharList = wordCharListCfg.GetAttribute("charsAdded").Value;
                        }
                    } catch (Exception e) {
                        ErrorHandler.LogError(e, "Error parsing " + FileNppConfigXml);
                    }
                } else {
                    UserCommunication.Notify("Couldn't find the config.xml file.<br>If this is not your first use of notepad++, please consider opening an issue on 3P", MessageImg.MsgHighImportance, "Reading config.xml", "File not found");
                }

                if (!string.IsNullOrEmpty(FileNppStylersXml) && !File.Exists(FileNppStylersXml))
                    FileNppStylersXml = null;
            }

            /// <summary>
            /// Called when the plugin is first used, suggests default options for npp
            /// </summary>
            internal void FinishPluginInstall() {
                object options = new NppConfigXmlOptions {
                    EnableMultiSelection = true,
                    DisableDefaultAutocompletion = true,
                    EnableBackupOnSave = true
                };
                ModifyingNppConfig(options, true);
            }

            /// <summary>
            /// Can be called at anytime to let the user modify notepad++ options
            /// </summary>
            internal void ModifyingNppConfig() {
                object options = new NppConfigXmlOptions {
                    EnableMultiSelection = ConfXml.MultiSelectionEnabled,
                    DisableDefaultAutocompletion = ConfXml.AutocompletionMode == 0,
                    EnableBackupOnSave = ConfXml.BackupMode != 0
                };
                ModifyingNppConfig(options, false);
            }

            private void ModifyingNppConfig(object opts, bool installMode) {
                var options = opts as NppConfigXmlOptions;
                if (options != null) {
                    var buttons = installMode ? new List<string> { "Apply changes now" } : new List<string> { "Apply changes now (restart)", "Cancel" };

                    var awnser = UserCommunication.Input(ref opts, (installMode ? "You are almost done with the installation!<br>" : "") + "You can now setup some configurations for notepad++.<br><b>It is highly recommended to " + (installMode ? "let all the options toggled ON" : "toggle ON all the options") + "</b> :<br><br>", MessageImg.MsgUpdate, "3P setup", "Modifying notepad++ options", buttons);

                    if (installMode || awnser == 0) {
                        ApplyNewOptions(options);
                    }

                }
            }
            
            /// <summary>
            /// Applies new options to the config.xml and restart npp to take them into account
            /// </summary>
            /// <param name="options"></param>
            public void ApplyNewOptions(NppConfigXmlOptions options) {
                if (options == null)
                    return;

                var encoding = TextEncodingDetect.GetFileEncoding(FileNppConfigXml);
                var fileContent = Utils.ReadAllText(FileNppConfigXml, encoding);
                fileContent = fileContent.Replace("autoCAction=\"" + AutocompletionMode + "\"", "autoCAction=\"" + (options.DisableDefaultAutocompletion ? 0 : 3) + "\"");
                fileContent = fileContent.Replace("name=\"Backup\" action=\"" + BackupMode + "\"", "name=\"Backup\" action=\"" + (options.EnableBackupOnSave ? 2 : 0) + "\"");
                fileContent = fileContent.Replace("enableMultiSelection=\"" + (MultiSelectionEnabled ? "yes" : "no") + "\"", "enableMultiSelection=\"" + (options.EnableMultiSelection ? "yes" : "no") + "\"");
                if (options.EnableBackupOnSave && (string.IsNullOrEmpty(CustomBackupDirectory) || !BackupUseCustomDir)) {
                    fileContent = fileContent.Replace("options.EnableBackupOnSave", "enableMultiSelection=\"" + (options.EnableMultiSelection ? "yes" : "no") + "\"");
                    var regex = new Regex(@"useCustumDir=""\w*?""\s+dir=""[^""]*?""", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    var matches = regex.Match(fileContent);
                    if (matches.Success) {
                        fileContent = regex.Replace(fileContent, "useCustumDir=\"yes\" dir=\"" + Path.Combine(BackupDirectory, "saved") + "\"");
                    }
                }
                var configCopyPath = Path.Combine(Config.FolderUpdate, "config.xml");
                if (!Utils.FileWriteAllText(configCopyPath, fileContent, encoding))
                    return;

                // replace default config by its copy on npp shutdown
                _3PUpdater.Instance.AddFileToMove(configCopyPath, FileNppConfigXml);

                Restart();
            }

            #endregion

            #region NppConfigXmlOptions

            internal class NppConfigXmlOptions {

                [YamuiInput("Enable multi-selection", Order = 0, Tooltip = "Allows multi-selection in the editor(CTRL+Click)")]
                public bool EnableMultiSelection { get; set; }

                [YamuiInput("Use 3P auto-completion", Order = 1, Tooltip = "3P replaces the default auto-completion of notepad++ by an improved version")]
                public bool DisableDefaultAutocompletion { get; set; }

                [YamuiInput("Enable backup on save", Order = 2, Tooltip = "Backup your files as you save them, avoid bad surprises after a notepad++ crash")]
                public bool EnableBackupOnSave { get; set; }
            }

            #endregion

        }
        
    }
}
