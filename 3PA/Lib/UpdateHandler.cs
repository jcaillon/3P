#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (UpdateHandler.cs) is part of 3P.
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
using System.Text;
using System.Threading.Tasks;
using YamuiFramework.Helper;
using _3PA.Lib._3pUpdater;
using _3PA.MainFeatures;
using _3PA.MainFeatures.Appli;
using _3PA.NppCore;

namespace _3PA.Lib {
    /// <summary>
    /// Handles the update of this software
    /// </summary>

    #region UpdaterWrapper

    internal class Updater<T> where T : UpdaterWrapper, new() {

        #region Singleton

        private static T _updaterWrapper;

        public static T Instance {
            get {
                if (_updaterWrapper == null)
                    _updaterWrapper = new T();
                return _updaterWrapper;
            }
        }

        #endregion

    }

    internal abstract class UpdaterWrapper {

        #region Private fields

        private GitHubUpdater _gitHubUpdater;

        private volatile bool _updating;

        #endregion

        #region Public properties

        /// <summary>
        /// Just as an information, name of the software you are updating
        /// </summary>
        public string UpdatedSoftName { get; set; }

        /// <summary>
        /// Set to true if the update needs notepad++ to restart
        /// </summary>
        public bool RestartNeeded { get; set; }

        /// <summary>
        /// Set to true to show the notifications on every update results
        /// </summary>
        public bool AlwaysShowNotifications { get; set; }

        /// <summary>
        /// The object that allows to check for update regularly
        /// </summary>
        public RecurentAction CheckRegularlyAction;

        /// <summary>
        /// Extra action to carry on when the download of the new release is done
        /// </summary>
        public Action<GitHubUpdater> ExtraActionWhenDownloaded { get; set; }

        /// <summary>
        /// The folder to which the downloaded .zip should be unzipped
        /// </summary>
        public string FolderUnzip { get; set; }

        /// <summary>
        /// A small text to describe how the user should do to manually update this software
        /// </summary>
        public string HowToInstallManually { get; set; }

        #endregion

        #region public

        /// <summary>
        /// published when the download +install of a new release is done or simply when the release check is done and nothing is new
        /// </summary>
        public event Action<UpdaterWrapper> OnUpdateDone;

        /// <summary>
        /// ASYNC - Call this method to start checking for updates every 2 hours, also check once immediately if 
        /// Config.Instance.TechnicalCheckUpdateEveryXMin condition is met
        /// </summary>
        public void StartCheckingForUpdate() {
            // check for updates every now and then
            CheckRegularlyAction = RecurentAction.StartNew(() => {
                // Check for new updates
                if (!Config.Instance.GlobalDontCheckUpdates)
                    CheckForUpdate(false);
            }, 1000 * 60 * Config.UpdateCheckEveryXMin, 0, Utils.IsLastCallFromMoreThanXMinAgo(UpdatedSoftName + "update", Config.UpdateCheckEveryXMin));
        }

        /// <summary>
        /// To call when the user click on an update button
        /// </summary>
        public void CheckForUpdate() {
            if (!Utils.IsSpamming(UpdatedSoftName + "update", 3000)) {
                UserCommunication.NotifyUnique("Update" + UpdatedSoftName, "Checking for a new release from " + Updater.GitHubReleaseApi.ToHtmlLink("github.com", true) + ".<br>You will be notified when it's done.", MessageImg.MsgInfo, UpdatedSoftName + " updater", "Checking for updates...", null, 5);
                Task.Factory.StartNew(() => {
                    CheckForUpdate(true);
                });
            }
        }

        /// <summary>
        /// Returns the local version of the soft (v0 if it doesn't exist)
        /// </summary>
        public string LocalVersion {
            get { return Updater.LocalVersion; }
        }

        #endregion

        #region Protected / private

        /// <summary>
        /// Gets an object with the latest release info
        /// </summary>
        private void CheckForUpdate(bool alwaysShowNotifications) {
            if (RestartNeeded && Updater.LatestReleaseInfo != null && (CheckRegularlyAction == null || CheckRegularlyAction.HasBeenDisposed)) {
                // we already checked and there is a new version, we can't do an update until a restart of n++
                if (alwaysShowNotifications)
                    NotifyUpdateAvailable(Updater);
            } else {
                AlwaysShowNotifications = alwaysShowNotifications;
                Updater.GetPreReleases = Config.Instance.UserGetsPreReleases;
                if (!_updating) {
                    _updating = true;
                    Updater.CheckForUpdates();
                } else if (alwaysShowNotifications) {
                    UserCommunication.NotifyUnique("Update" + UpdatedSoftName, "Please wait, " + UpdatedSoftName + " is being updated.<br>You will be informed when it is done", MessageImg.MsgInfo, UpdatedSoftName + " updater", "Update in progress", null, 5);
                }
            }
        }

        /// <summary>
        /// Get githubupdater
        /// </summary>
        protected GitHubUpdater Updater {
            get {
                if (_gitHubUpdater == null) {
                    _gitHubUpdater = new GitHubUpdater {
                        AssetDownloadFolder = Path.Combine(Config.FolderTemp, "downloads"),
                        BasicAuthenticationToken = Config.GitHubBasicAuthenticationToken
                    };
                    _gitHubUpdater.ErrorOccured += OnErrorOccured;
                    _gitHubUpdater.NewReleaseDownloaded += OnNewReleaseDownloaded;
                    _gitHubUpdater.AlreadyUpdated += OnAlreadyUpdated;
                    _gitHubUpdater.StartingUpdate += MainUpdaterOnStartingUpdate;
                }
                return _gitHubUpdater;
            }
            set { _gitHubUpdater = value; }
        }
        
        /// <summary>
        /// Called before the download starts
        /// </summary>
        protected virtual void MainUpdaterOnStartingUpdate(GitHubUpdater gitHubUpdater, GitHubUpdater.ReleaseInfo releaseInfo, GitHubUpdater.StartingDownloadEvent e) {

            Config.Instance.TechnicalLastWebserviceCallOk = true;

            DelayedAction.StartNew(2000, () => {
                if (_updating && !UserCommunication.IsUniqueNotifShown("Update" + UpdatedSoftName)) {
                    UserCommunication.NotifyUnique("Update" + UpdatedSoftName, "A newer version of " + UpdatedSoftName + " (" + releaseInfo.tag_name + ") has been found online.<br>It is being downloaded, you will be notified when the update is available.", MessageImg.MsgUpdate, UpdatedSoftName + " updater", "New version found", null, 5);
                }
            });
        }

        /// <summary>
        /// Called when the soft is already up to date
        /// </summary>
        protected virtual void OnAlreadyUpdated(GitHubUpdater gitHubUpdater, GitHubUpdater.ReleaseInfo releaseInfo) {
            try {
                Config.Instance.TechnicalLastWebserviceCallOk = true;
                
                if (AlwaysShowNotifications) {
                    UserCommunication.NotifyUnique("Update" + UpdatedSoftName, "Congratulations! You already own the latest <b>" + (!gitHubUpdater.GetPreReleases ? "beta" : "stable") + "</b> version of " + UpdatedSoftName + "." + (!gitHubUpdater.GetPreReleases ? "<br><br><i>If you wish to check for beta versions as well, toggle the corresponding option in the update " + "options".ToHtmlLink("options page") + "</i>" : ""), MessageImg.MsgUpdate, UpdatedSoftName + " updater", "Local version is " + gitHubUpdater.LocalVersion, args => {
                        if (args.Link.Equals("options")) {
                            args.Handled = true;
                            Appli.GoToPage(PageNames.OptionsUpdate);
                        }
                    });
                }

                if (OnUpdateDone != null)
                    OnUpdateDone(this);
            } finally {
                _updating = false;
            }
        }

        /// <summary>
        /// Called when a new release has been downloaded
        /// </summary>
        protected virtual void OnNewReleaseDownloaded(GitHubUpdater gitHubUpdater, string downloadedFile) {
            try {
                // Extract the .zip file
                if (Utils.ExtractAll(downloadedFile, FolderUnzip)) {

                    // execute extra actions (for the 3P update for instance)
                    if (ExtraActionWhenDownloaded != null) {
                        ExtraActionWhenDownloaded(gitHubUpdater);
                    }

                    NotifyUpdateAvailable(Updater);

                    if (gitHubUpdater.LatestReleaseInfo != null)
                        Updater.LocalVersion = gitHubUpdater.LatestReleaseInfo.tag_name;

                    if (OnUpdateDone != null)
                        OnUpdateDone(this);
                } else {
                    UserCommunication.NotifyUnique("Update" + UpdatedSoftName, "Failed to unzip the following file : <br>" + downloadedFile.ToHtmlLink() + "<br>It contains the update for " + UpdatedSoftName + ", you will have to do a manual update." + HowToInstallManually, MessageImg.MsgError, UpdatedSoftName + " updater", "Unzip failed", null);
                }

            } finally {
                _updating = false;
            }
        }

        /// <summary>
        /// Called when an error occurred during the update
        /// </summary>
        /// <param name="gitHubUpdater"></param>
        /// <param name="e"></param>
        /// <param name="gitHubUpdaterFailReason"></param>
        protected virtual void OnErrorOccured(GitHubUpdater gitHubUpdater, Exception e, GitHubUpdater.GitHubUpdaterFailReason gitHubUpdaterFailReason) {
            try {
                switch (gitHubUpdaterFailReason) {

                    case GitHubUpdater.GitHubUpdaterFailReason.ReleaseApiUnreachable:

                        if (Config.Instance.TechnicalLastWebserviceCallOk || AlwaysShowNotifications) {
                            // only show this message once in case of repetitive failures
                            UserCommunication.NotifyUnique("Update" + UpdatedSoftName, "For your information, it has not been possible to check for new releases on GITHUB.<br><br>The API requested was :<br>" + gitHubUpdater.GitHubReleaseApi.ToHtmlLink() + "<br>You might want to check your proxy settings on the " + "options".ToHtmlLink("update options page") + HowToInstallManually, MessageImg.MsgHighImportance, UpdatedSoftName + " updater", "Couldn't query GITHUB API", args => {
                                if (args.Link.Equals("options")) {
                                    args.Handled = true;
                                    Appli.GoToPage(PageNames.OptionsUpdate);
                                }
                            });
                        }

                        Config.Instance.TechnicalLastWebserviceCallOk = false;

                        // check if there is an update available in the Shared config folder
                        if (!String.IsNullOrEmpty(Config.Instance.SharedConfFolder) && Directory.Exists(Config.Instance.SharedConfFolder)) {

                            var potentialUpdate = Path.Combine(Config.Instance.SharedConfFolder, AssemblyInfo.AssemblyName);

                            // if the .dll exists, is higher version and (the user get beta releases or it's a stable release)
                            if (File.Exists(potentialUpdate) &&
                                Utils.GetDllVersion(potentialUpdate).IsHigherVersionThan(AssemblyInfo.Version) &&
                                (Config.Instance.UserGetsPreReleases || AssemblyInfo.IsPreRelease || Utils.GetDllVersion(potentialUpdate).EndsWith(".0"))) {

                                // copy to local update folder and warn the user 
                                if (Utils.CopyFile(potentialUpdate, Path.Combine(FolderUnzip, AssemblyInfo.AssemblyName))) {
                                    gitHubUpdater.LatestReleaseInfo = new GitHubUpdater.ReleaseInfo {
                                        tag_name = Utils.GetDllVersion(potentialUpdate),
                                        prerelease = Utils.GetDllVersion(potentialUpdate).EndsWith(".1"),
                                        published_at = "???",
                                        html_url = Config.UrlCheckReleases
                                    };

                                    if (ExtraActionWhenDownloaded != null) {
                                        ExtraActionWhenDownloaded(gitHubUpdater);
                                    }

                                    gitHubUpdater.VersionLog.Append("Version found on the shared folder : \n" + Config.Instance.SharedConfFolder.ToHtmlLink() + "\n\nCheck the official website to learn more about this release");

                                    NotifyUpdateAvailable(Updater);
                                }
                            }
                        }

                        break;

                    default:
                        ErrorHandler.ShowErrors(e, "Update error for " + UpdatedSoftName + " : " + gitHubUpdaterFailReason);
                        break;
                }
            } finally {
                _updating = false;
            }
        }

        /// <summary>
        /// Called when an update is available
        /// </summary>
        protected virtual void NotifyUpdateAvailable(GitHubUpdater updater) {
            if (updater.LatestReleaseInfo != null) {
                UserCommunication.NotifyUnique("Update" + UpdatedSoftName, 
                    @"Dear user, <br>
                    <br>
                    A new version of " + UpdatedSoftName + @" has been downloaded!<br>" +
                    (RestartNeeded ? @"It will be automatically installed the next time you restart Notepad++<br>" : "It has already been installed successfully<br>") + @"
                    <br>                   
                    Your version: <b>" + updater.LocalVersion + @"</b><br>
                    New version: <b>" + updater.LatestReleaseInfo.tag_name + @"</b><br>
                    Release name: <b>" + updater.LatestReleaseInfo.name + @"</b><br>
                    Available since: <b>" + updater.LatestReleaseInfo.published_at + @"</b><br>
                    Release URL: <b>" + updater.LatestReleaseInfo.html_url.ToHtmlLink() + @"</b><br>" +
                    (updater.LatestReleaseInfo.prerelease ? "<i>This new release is a beta version</i><br>" : "") + "<br>" +
                    "log".ToHtmlLink("Click here to see what is new in this version", true) + "<br>" +
                    (RestartNeeded ? (_3PUpdater.Instance.IsAdminRightsNeeded ? "<br><span class='SubTextColor'><i><b>3pUpdater.exe</b> will need administrator rights to replace your current version by the new release,<br>please click yes when you are asked to execute it</i></span><br>" : "") + @"<br><b>" + "Restart".ToHtmlLink("Click here to restart now!") + @"</b>" : ""),
                    MessageImg.MsgUpdate, UpdatedSoftName + " updater", "New update downloaded",
                    args => {
                        if (args.Link.Equals("Restart")) {
                            args.Handled = true;
                            Npp.Restart();
                        } else if (args.Link.Equals("log")) {
                            args.Handled = true;
                            UserCommunication.Message(updater.VersionLog.ToString().MdToHtml(),
                                MessageImg.MsgUpdate,
                                UpdatedSoftName + " updater",
                                "Release notes from " + Updater.LocalVersion + " to " + Updater.LatestReleaseInfo.tag_name,
                                new List<string> { "ok" },
                                false);
                        }
                    });

                if (RestartNeeded && CheckRegularlyAction != null) {
                    // stop checking for more updates :)
                    CheckRegularlyAction.Dispose();
                }
            }
        }

        #endregion
    }

    #endregion

    #region MainUpdaterWrapper

    /// <summary>
    /// The 3P updater
    /// </summary>
    internal class MainUpdaterWrapper : UpdaterWrapper {

        #region Life and death

        public MainUpdaterWrapper() {
            UpdatedSoftName = AssemblyInfo.AssemblyProduct;
            FolderUnzip = Config.UpdateReleaseUnzippedFolder;
            HowToInstallManually = "<br><br><i>If you wish to manually install " + UpdatedSoftName + ", you have to : <br><ul><li>Close notepad++</li><li>Download the latest release on " + "https://github.com/jcaillon/3P/releases".ToHtmlLink("GITHUB") + "</li><li>Extract its content to " + Path.GetDirectoryName(AssemblyInfo.Location).ToHtmlLink() + "</li></ul></i>";
            ExtraActionWhenDownloaded = On3PUpdate;
            RestartNeeded = true;

            Updater.AssetName = Config.UpdateGitHubAssetName;
            Updater.GitHubReleaseApi = Config.UpdateReleasesApi;
            Updater.LocalVersion = AssemblyInfo.Version;
        }

        #endregion

        #region public

        /// <summary>
        /// Method to call when the user starts notepad++,
        /// check if an update has been done since the last time notepad was closed
        /// </summary>
        public void CheckForUpdateDoneAndStartCheckingForUpdates() {

            var previousVersion = File.Exists(Config.UpdatePreviousVersion) ? Utils.ReadAllText(Config.UpdatePreviousVersion, Encoding.Default) : null;

            // an update has been done
            if (!string.IsNullOrEmpty(previousVersion)) {

                // we didn't update to a newer version, something went wrong
                if (!AssemblyInfo.Version.IsHigherVersionThan(previousVersion)) {
                    UserCommunication.Notify(@"<h2>I require your attention!</h2><br>
                        <div>
                        The update didn't go as expected, the old plugin files have not been replaced by the new ones!<br>
                        It is very likely because the updater didn't get the rights to write a file in your /plugins/ folder.<br>
                        You will have to manually copy the new files to replace the existing files :<br><br>
                        <b>MOVE (delete the source and replace the target)</b> all the files in this folder : <div>" + Config.UpdateReleaseUnzippedFolder.ToHtmlLink() + @"</div><br>
                        <b>In this folder</b> (replacing the existing files) : <div>" + Path.GetDirectoryName(AssemblyInfo.Location).ToHtmlLink() + @"</div><br><br>
                        Please do it as soon as possible, as i will stop checking for more updates until this problem is fixed.<br>
                        <i>(n.b. : this message will be shown at startup as long as the above-mentioned folder exists!)</i><br>
                        Thank you for your patience!</div>", MessageImg.MsgUpdate, UpdatedSoftName + " updater", "Problem during the update!");
                    return;
                }

                var versionLog = File.Exists(Config.UpdateVersionLog) ? Utils.ReadAllText(Config.UpdateVersionLog, Encoding.Default) : null;

                UserCommunication.Notify("A new version of the software has just been installed, congratulations!" + (!string.IsNullOrEmpty(versionLog) ? "<br><br>" + "log".ToHtmlLink("Click here to show what is new in this version") : ""), MessageImg.MsgUpdate, UpdatedSoftName + " updater", "Install successful", args => {
                    if (args.Link.Equals("log")) {
                        args.Handled = true;
                        UserCommunication.Message(versionLog.MdToHtml(),
                            MessageImg.MsgUpdate,
                            UpdatedSoftName + " updater",
                            "Release notes from " + previousVersion + " to " + AssemblyInfo.Version,
                            new List<string> { "ok" },
                            false);
                    }
                });

                // Special actions to take depending on the previous version?
                if (!string.IsNullOrEmpty(previousVersion))
                    UpdateDoneFromVersion(previousVersion);

                // delete update related files/folders
                Utils.DeleteDirectory(Config.FolderUpdate, true);

                // update UDL
                if (!Config.Instance.GlobalDontUpdateUdlOnUpdate)
                    Style.InstallUdl();

            }

            StartCheckingForUpdate();
        }

        #endregion

        #region private

        /// <summary>
        /// Called on an update, allows to do special stuff according to the version updated
        /// </summary>
        private void UpdateDoneFromVersion(string fromVersion) {
            // reset the log files
            Utils.DeleteDirectory(Config.FolderLog, true);

            if (!fromVersion.IsHigherVersionThan("1.7.3")) {
                Utils.DeleteDirectory(Path.Combine(Npp.ConfigDirectory, "Libraries"), true);
            }
        }

        /// <summary>
        /// Called when the release file is downloaded and extracted to updater.FolderUnzip
        /// </summary>
        /// <param name="updater"></param>
        private void On3PUpdate(GitHubUpdater updater) {

            // list all the files of the zip, they should be copied in the notepad++ /plugins/ folder by the updater
            foreach (var fullPath in Directory.EnumerateFiles(FolderUnzip, "*", SearchOption.TopDirectoryOnly)) {
                _3PUpdater.Instance.AddFileToMove(fullPath, Path.Combine(Path.GetDirectoryName(AssemblyInfo.Location) ?? "", Path.GetFileName(fullPath) ?? ""));
            }

            // write the version log
            Utils.FileWriteAllText(Config.UpdateVersionLog, updater.VersionLog.ToString(), Encoding.Default);
            Utils.FileWriteAllText(Config.UpdatePreviousVersion, AssemblyInfo.Version, Encoding.Default);
        }

        #endregion

    }

    #endregion

    #region ProlintUpdaterWrapper

    /// <summary>
    /// The prolint updater
    /// </summary>
    internal class ProlintUpdaterWrapper : UpdaterWrapper {

        #region Public properties

        /// <summary>
        /// Returns the folder path in which prolint is installed
        /// </summary>
        public string ApplicationFolder {  get { return Config.ProlintFolder; } }

        #endregion

        #region Life and death

        public ProlintUpdaterWrapper() {

            UpdatedSoftName = "Prolint";
            FolderUnzip = Config.ProlintFolder;
            HowToInstallManually = "<br><br><i>If you wish to manually install " + UpdatedSoftName + ", you have to : <br><ul><li>Download the latest release on " + "https://github.com/jcaillon/prolint/releases".ToHtmlLink("GITHUB") + "</li><li>Extract its content to " + Config.ProlintFolder.ToHtmlLink() + "</li></ul></i>";

            var localVersion = "v0";
            var releasePath = Path.Combine(ApplicationFolder, "prolint", "core", "release.ini");

            if (File.Exists(releasePath)) {
                var prolintRelease = new IniReader(releasePath);
                localVersion = prolintRelease.GetValue(@"prolint", @"0");
            }

            Updater.AssetName = Config.ProlintGitHubAssetName;
            Updater.GitHubReleaseApi = Config.ProlintReleasesApi;
            Updater.LocalVersion = localVersion;
        }

        #endregion

    }

    #endregion

    #region ProparseUpdaterWrapper
    
    /// <summary>
    /// The proparse.net updater
    /// </summary>
    internal class ProparseUpdaterWrapper : UpdaterWrapper {

        #region Public properties

        /// <summary>
        /// Returns the folder path in which prolint is installed
        /// </summary>
        public string ApplicationFolder { get { return Path.Combine(Config.ProlintFolder, "proparse.net"); } }

        #endregion

        #region Life and death

        public ProparseUpdaterWrapper() {

            UpdatedSoftName = "Proparse.net";
            FolderUnzip = Config.ProlintFolder;
            HowToInstallManually = "<br><br><i>If you wish to manually install " + UpdatedSoftName + ", you have to : <br><ul><li>Download the latest release on " + "https://github.com/jcaillon/proparse/releases".ToHtmlLink("GITHUB") + "</li><li>Extract its content to " + Config.ProlintFolder.ToHtmlLink() + "</li></ul></i>";

            var localVersion = "v0";
            var releasePath = Path.Combine(ApplicationFolder, "proparse.net.dll");

            if (File.Exists(releasePath)) {
                localVersion = Utils.GetDllVersion(releasePath);
            }

            Updater.AssetName = Config.ProparseGitHubAssetName;
            Updater.GitHubReleaseApi = Config.ProparseReleasesApi;
            Updater.LocalVersion = localVersion;
        }

        #endregion
    }

    #endregion

}