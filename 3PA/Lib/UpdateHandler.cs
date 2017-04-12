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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using _3PA.Lib._3pUpdater;
using _3PA.MainFeatures;
using _3PA.NppCore;

namespace _3PA.Lib {
    /// <summary>
    /// Handles the update of this software
    /// </summary>
    internal static class UpdateHandler {
        #region fields

        /// <summary>
        /// Holds the info about the latest release found on the distant update server
        /// </summary>
        private static ReleaseInfo _latestReleaseInfo;

        private static RecurentAction _checkEveryHourAction;

        private static volatile bool _isChecking;
        private static volatile bool _displayResultNotif;
        private static bool _updateAvailableOnRestart;

        #endregion

        #region public

        /// <summary>
        /// Method to call when the user starts notepad++,
        /// check if an update has been done since the last time notepad was closed
        /// </summary>
        public static void CheckForUpdateDone() {
            // an update has been done
            if (File.Exists(Config.FileVersionLog)) {
                // The dll is still in the update dir, something went wrong
                if (File.Exists(Config.FileDownloadedPlugin)) {
                    UserCommunication.Notify(@"<h2>I require your attention!</h2><br>
                        <div>
                        The update didn't go as expected, i couldn't replace the old plugin file by the new one!<br>
                        It is very likely because i didn't get the rights to write a file in your /plugins/ folder, don't panic!<br>
                        You will have to manually copy the new file and delete the old file :<br><br>
                        <b>MOVE (delete the source and replace the target)</b> this file : <div>" + Path.GetDirectoryName(Config.FileDownloadedPlugin).ToHtmlLink(Config.FileDownloadedPlugin) + @"</div><br>
                        <b>In this folder</b> (replacing the old file) : <div>" + Path.GetDirectoryName(AssemblyInfo.Location).ToHtmlLink() + @"</div><br><br>
                        Please do it as soon as possible, as i will stop checking for more updates until this problem is fixed.<br>
                        <i>(n.b. : this message will be shown at startup as long as the above-mentioned file exists!)</i><br>
                        Thank you for your patience!</div>", MessageImg.MsgUpdate, "Update", "Problem during the update!");
                    return;
                }
                
                UserCommunication.Message(("# What's new in this version? #\n\n" + Utils.ReadAllText(Config.FileVersionLog, Encoding.Default)).MdToHtml(),
                    MessageImg.MsgUpdate,
                    "A new version has been installed!",
                    "Updated to version " + AssemblyInfo.Version,
                    new List<string> {"ok"},
                    false);

                // Special actions to take depending on the previous version?
                if (File.Exists(Config.FilePreviousVersion))
                    UpdateDoneFromVersion(Utils.ReadAllText(Config.FilePreviousVersion, Encoding.Default));

                // delete update related files/folders
                Utils.DeleteDirectory(Config.FolderUpdate, true);
                Utils.DeleteFile(Config.FileVersionLog);

                // reset the log files
                Utils.DeleteDirectory(Config.FolderLog, true);

                // update UDL
                if (!Config.Instance.GlobalDontUpdateUdlOnUpdate)
                    Style.InstallUdl();
            }
        }

        /// <summary>
        /// ASYNC - Call this method to start checking for updates every 2 hours, also check once immediately if 
        /// Config.Instance.TechnicalCheckUpdateEveryXMin condition is met
        /// </summary>
        public static void StartCheckingForUpdate() {
            // check for updates every now and then (2h)
            DateTime lastCheck;
            if (!DateTime.TryParseExact(Config.Instance.TechnicalLastCheckUpdate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out lastCheck)) {
                lastCheck = DateTime.MinValue;
            }
            _checkEveryHourAction = RecurentAction.StartNew(() => {
                // Check for new updates
                if (!Config.Instance.GlobalDontCheckUpdates)
                    CheckForUpdate(true);
            }, 1000 * 60 * 120, 0, DateTime.Now.Subtract(lastCheck).TotalMinutes > Config.Instance.TechnicalCheckUpdateEveryXMin);
        }

        /// <summary>
        /// To call when the user click on an update button
        /// </summary>
        public static void CheckForUpdate() {
            if (!Utils.IsSpamming("updates", 1000)) {
                UserCommunication.Notify("Now checking for updates, you will be notified when it's done", MessageImg.MsgInfo, "Update", "Update check", 5);
                Task.Factory.StartNew(() => { CheckForUpdate(false); });
            }
        }

        /// <summary>
        /// Gets an object with the latest release info
        /// </summary>
        public static void CheckForUpdate(bool periodicCheck) {
            _displayResultNotif = !periodicCheck;

            if (_isChecking)
                return;

            if (_updateAvailableOnRestart && _latestReleaseInfo != null && _displayResultNotif) {
                // we already checked and there is a new version
                NotifyUpdateAvailable();
                return;
            }
            _isChecking = true;
            try {
                var wb = new WebServiceJson(WebServiceJson.WebRequestMethod.Get, Config.ReleasesApi) {
                    TimeOut = 3000
                };
                wb.OnInitHttpWebRequest += request => { request.Headers.Add("Authorization", "Basic M3BVc2VyOnJhbmRvbXBhc3N3b3JkMTIz"); };
                wb.OnRequestEnded += WbOnOnRequestEnded;
                wb.Execute();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error when checking for updates");
                _isChecking = false;
            }

            Config.Instance.TechnicalLastCheckUpdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Called when the gitub api for releases responses
        /// </summary>
        private static void WbOnOnRequestEnded(WebServiceJson webServiceJson) {
            try {
                if (webServiceJson.StatusCodeResponse == HttpStatusCode.OK && webServiceJson.ResponseException == null) {
                    Config.Instance.TechnicalLastCheckUpdateOk = true;

                    // get the releases
                    var releases = webServiceJson.DeserializeArray<ReleaseInfo>();
                    if (releases != null && releases.Count > 0) {
                        // sort
                        releases.Sort((o, o2) => o.tag_name.IsHigherVersionThan(o2.tag_name) ? -1 : 1);

                        var localVersion = AssemblyInfo.Version;
                        var outputBody = new StringBuilder();
                        foreach (var release in releases) {
                            if (string.IsNullOrEmpty(release.tag_name))
                                continue;

                            // For each version higher than the local one, append to the release body
                            // Will be used to display the version log to the user
                            if (release.tag_name.IsHigherVersionThan(localVersion) &&
                                (Config.Instance.UserGetsPreReleases || AssemblyInfo.IsPreRelease || !release.prerelease) &&
                                release.assets != null && release.assets.Count > 0 && release.assets.Exists(asset => asset.name.EqualsCi(Config.FileGitHubAssetName))) {
                                // in case something is undefined (but shouldn't happen)
                                if (string.IsNullOrEmpty(release.tag_name)) release.tag_name = "vX.X.X.X";
                                if (string.IsNullOrEmpty(release.name)) release.name = "unknown";
                                if (string.IsNullOrEmpty(release.body)) release.body = "...";
                                if (string.IsNullOrEmpty(release.published_at)) release.published_at = DateTime.Now.ToString(CultureInfo.CurrentCulture);

                                // h1
                                outputBody.AppendLine("## " + release.tag_name + " : " + release.name + " ##\n\n");
                                // body
                                outputBody.AppendLine(release.body + "\n\n");

                                // the first higher release encountered is the latest
                                if (_latestReleaseInfo == null)
                                    _latestReleaseInfo = release;
                            }
                        }

                        // There is a distant version higher than the local one
                        if (_latestReleaseInfo != null) {
                            // to display all the release notes
                            _latestReleaseInfo.body = outputBody.ToString();

                            // delete existing dir
                            Utils.DeleteDirectory(Config.FolderUpdate, true);

                            Utils.DownloadFile(_latestReleaseInfo.assets.First(asset => asset.name.EqualsCi(Config.FileGitHubAssetName)).browser_download_url, Config.FileLatestReleaseZip, OnDownloadFileCompleted);
                            return;
                        }

                        if (_displayResultNotif) {
                            UserCommunication.NotifyUnique("UpdateChecked", "Congratulations! You already possess the latest <b>" + (!AssemblyInfo.IsPreRelease ? "stable" : "beta") + "</b> version of 3P!", MessageImg.MsgOk, "Update check", "You own the version " + AssemblyInfo.Version, null);
                        }
                    }
                } else {
                    // failed to retrieve the list
                    if (_displayResultNotif || Config.Instance.TechnicalLastCheckUpdateOk)
                        UserCommunication.NotifyUnique("ReleaseListDown", "For your information, I couldn't manage to retrieve the latest published version on GITHUB.<br><br>A request has been sent to :<br>" + Config.ReleasesApi.ToHtmlLink() + "<br>but was unsuccessful, you might have to check for a new version manually if this happens again.", MessageImg.MsgHighImportance, "Couldn't reach GITHUB", "Connection failed", null);

                    Config.Instance.TechnicalLastCheckUpdateOk = false;

                    // check if there is an update available in the Shared config folder
                    if (!string.IsNullOrEmpty(Config.Instance.SharedConfFolder) && Directory.Exists(Config.Instance.SharedConfFolder)) {
                        var potentialUpdate = Path.Combine(Config.Instance.SharedConfFolder, AssemblyInfo.AssemblyName);

                        // if the .dll exists, is higher version and (the user get beta releases or it's a stable release)
                        if (File.Exists(potentialUpdate) &&
                            Utils.GetDllVersion(potentialUpdate).IsHigherVersionThan(AssemblyInfo.Version) &&
                            (Config.Instance.UserGetsPreReleases || AssemblyInfo.IsPreRelease || Utils.GetDllVersion(potentialUpdate).EndsWith(".0"))) {
                            // copy to local update folder and warn the user 
                            if (Utils.CopyFile(potentialUpdate, Config.FileDownloadedPlugin)) {
                                _latestReleaseInfo = new ReleaseInfo {
                                    name = "Updated from shared directory",
                                    tag_name = Utils.GetDllVersion(Config.FileDownloadedPlugin),
                                    prerelease = Utils.GetDllVersion(Config.FileDownloadedPlugin).EndsWith(".1"),
                                    published_at = "???",
                                    html_url = Config.UrlCheckReleases
                                };

                                // set up the update so the .dll file downloaded replaces the current .dll
                                _3PUpdater.Instance.AddFileToMove(Config.FileDownloadedPlugin, AssemblyInfo.Location);

                                // write the version log
                                Utils.FileWriteAllText(Config.FileVersionLog, @"This version has been updated from the shared directory" + Environment.NewLine + Environment.NewLine + @"Find more information on this release [here](" + Config.UrlCheckReleases + @")", Encoding.Default);
                                Utils.FileWriteAllText(Config.FilePreviousVersion, AssemblyInfo.Version, Encoding.Default);

                                NotifyUpdateAvailable();
                            }
                        }
                    }
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error when checking the latest release");
            }
            _isChecking = false;
        }

        #endregion

        #region private

        /// <summary>
        /// Called when the latest release download is done
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="asyncCompletedEventArgs"></param>
        private static void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs) {
            try {
                // Extract the .zip file
                if (Utils.ExtractAll(Config.FileLatestReleaseZip, Config.FolderUpdate)) {
                    // check the presence of the plugin file
                    if (File.Exists(Config.FileDownloadedPlugin)) {
                        // set up the update so the .dll file downloaded replaces the current .dll
                        _3PUpdater.Instance.AddFileToMove(Config.FileDownloadedPlugin, AssemblyInfo.Location);

                        // if the release was containing a .pdb file, we want to copied it as well
                        if (File.Exists(Config.FileDownloadedPdb)) {
                            _3PUpdater.Instance.AddFileToMove(Config.FileDownloadedPdb, Path.Combine(Path.GetDirectoryName(AssemblyInfo.Location) ?? "", Path.GetFileName(Config.FileDownloadedPdb) ?? ""));
                        }

                        // write the version log
                        Utils.FileWriteAllText(Config.FileVersionLog, _latestReleaseInfo.body, Encoding.Default);
                        Utils.FileWriteAllText(Config.FilePreviousVersion, AssemblyInfo.Version, Encoding.Default);

                        NotifyUpdateAvailable();
                    } else {
                        Utils.DeleteDirectory(Config.FolderUpdate, true);
                    }
                } else {
                    UserCommunication.Notify("I failed to unzip the following file : <br>" + Config.FileLatestReleaseZip + "<br>It contains the update for 3P, you will have to do a manual update.", MessageImg.MsgError, "Unzip", "Failed");
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "On Download File Completed");
            }
            _isChecking = false;
        }

        private static void NotifyUpdateAvailable() {
            if (_latestReleaseInfo != null) {
                _updateAvailableOnRestart = true;

                UserCommunication.NotifyUnique("UpdateAvailable", @"Dear user, <br>
                    <br>
                    A new version of 3P has been downloaded!<br>
                    It will be automatically installed the next time you restart Notepad++<br>
                    <br>                   
                    Your version: <b>" + AssemblyInfo.Version + @"</b><br>
                    Distant version: <b>" + _latestReleaseInfo.tag_name + @"</b><br>
                    Release name: <b>" + _latestReleaseInfo.name + @"</b><br>
                    Available since: <b>" + _latestReleaseInfo.published_at + @"</b><br>" +
                                                                  "Release URL: <b>" + _latestReleaseInfo.html_url.ToHtmlLink() + @"</b><br>" +
                                                                  (_latestReleaseInfo.prerelease ? "<i>This distant release is a beta version</i><br>" : "") +
                                                                  (_3PUpdater.Instance.IsAdminRightsNeeded ? "<br><span class='SubTextColor'><i><b>3pUpdater.exe</b> will need administrator rights to replace your current 3P.dll file by the new release,<br>please click yes when you are asked to execute it</i></span>" : "") +
                                                                  "<br><br><b>" + "Restart".ToHtmlLink("Click here to restart now!") + @"</b>",
                    MessageImg.MsgUpdate, "Update check", "An update is available",
                    args => {
                        if (args.Link.Equals("Restart")) {
                            args.Handled = true;
                            Npp.Restart();
                        }
                    });

                // stop checking for more updates :)
                _checkEveryHourAction.Dispose();
            }
            _isChecking = false;
        }

        /// <summary>
        /// Called on an update, allows to do special stuff according to the version updated
        /// </summary>
        private static void UpdateDoneFromVersion(string fromVersion) {
            if (!fromVersion.IsHigherVersionThan("1.7.3")) {
                Utils.DeleteDirectory(Path.Combine(Npp.ConfigDirectory, "Libraries"), true);
            }
        }

        #endregion

        #region ReleaseInfo

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public class Asset {
            public string name { get; set; }
            public object label { get; set; }
            public string content_type { get; set; }
            public string state { get; set; }
            public int download_count { get; set; }
            public string created_at { get; set; }
            public string updated_at { get; set; }
            public string browser_download_url { get; set; }
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public class ReleaseInfo {
            public string html_url { get; set; }

            /// <summary>
            /// Release version
            /// </summary>
            public string tag_name { get; set; }

            /// <summary>
            /// Targeted branch
            /// </summary>
            public string target_commitish { get; set; }

            /// <summary>
            /// Release name
            /// </summary>
            public string name { get; set; }

            public bool prerelease { get; set; }
            public string published_at { get; set; }
            public List<Asset> assets { get; set; }

            /// <summary>
            /// content of the release text
            /// </summary>
            public string body { get; set; }
        }

        #endregion
    }
}