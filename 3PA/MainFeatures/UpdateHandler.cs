#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using _3PA.Html;
using _3PA.Lib;
using _3PA.Lib._3pUpdater;

namespace _3PA.MainFeatures {

    /// <summary>
    /// Handles the update of this software
    /// </summary>
    internal static class UpdateHandler {

        #region fields

        /// <summary>
        /// Holds the info about the latest release found on the distant update server
        /// </summary>
        private static ReleaseInfo _latestReleaseInfo;
        private static bool _warnedUserAboutUpdateAvail;
        private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private static ReccurentAction _checkEveryHourAction;

        #endregion

        #region constants to read the JSON

        /// <summary>
        /// name of the fields in the GITHUB json
        /// </summary>
        private const string ReleaseVersionName = "tag_name";

        private const string BoolIsBetaName = "prerelease";
        private const string ReleaseName = "name";
        private const string ReleaseUrlName = "html_url";
        private const string BoolIsDraftName = "draft";
        private const string ReleaseDateName = "updated_at";
        private const string ReleaseDownloadUrlName = "browser_download_url";

        #endregion

        #region public

        /// <summary>
        /// Method to call when the user starts notepad++,
        /// check if an update has been done since the last time notepad was closed
        /// </summary>
        public static void OnNotepadStart() {

            // if the UDL is not installed
            if (!Style.InstallUdl(true)) {
                Style.InstallUdl();
            } else {
                // first use message?
                if (Config.Instance.UserFirstUse) {
                    UserCommunication.NotifyUnique("welcome", "<div>Dear user,<br><br>Thank you for installing 3P, you are awesome!<br><br>If this is your first look at 3P I invite you to read the <b>Getting started</b> section of the home page by clicking <a href='go'>on this link right here</a>.<br><br></div><div align='right'>Enjoy!</div>", MessageImg.MsgInfo, "Information", "Hello and welcome aboard!", args => {
                        Appli.Appli.ToggleView();
                        UserCommunication.CloseUniqueNotif("welcome");
                        args.Handled = true;
                    });
                    Config.Instance.UserFirstUse = false;
                }
            }

            // check Npp version, 3P requires version 6.8 or higher
            if (!string.IsNullOrEmpty(Npp.GetNppVersion()) && !Npp.GetNppVersion().IsHigherVersionThan("6.7")) {
                UserCommunication.Notify("Dear user,<br><br>Your version of notepad++ (" + Npp.GetNppVersion() + ") is outdated.<br>3P <b>requires</b> the version <b>6.8</b> or above, <b>there are known issues with inferior versions</b>. Please upgrade to an up-to-date version of Notepad++ or use 3P at your own risks.<br><br><a href='https://notepad-plus-plus.org/download/'>Download the lastest version of Notepad++ here</a>", MessageImg.MsgError, "Outdated version", "3P requirements are not met");
            }

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
                        <i>(n.b. : this message will be shown at startup as long as the above-mentionned file exists!)</i><br>
                        Thank you for your patience!</div>", MessageImg.MsgUpdate, "Update", "Problem during the update!");
                    return;
                }

                UserCommunication.Message(("# What's new in this version? #\n\n" + File.ReadAllText(Config.FileVersionLog, TextEncodingDetect.GetFileEncoding(Config.FileVersionLog))).MdToHtml(),
                    MessageImg.MsgUpdate,
                    "A new version has been installed!",
                    "Updated to version " + AssemblyInfo.Version,
                    new List<string> { "ok" },
                    false);

                // delete update related files/folders
                Utils.DeleteFile(Config.FileVersionLog);
                Utils.DeleteDirectory(Config.FolderUpdate, true);

                // reset the log files
                Utils.DeleteDirectory(Config.FolderLog, true);

                // update UDL
                if (!Config.Instance.GlobalDontUpdateUdlOnUpdate)
                    Style.InstallUdl();
            }

            // check for updates every now and then (2h)
            _checkEveryHourAction = new ReccurentAction(() => {
                // Check for new updates
                if (!Config.Instance.GlobalDontCheckUpdates)
                    GetLatestReleaseInfo(false);
            }, 1000 * 60 * 120);
        }

        /// <summary>
        /// To call when the user click on an update button
        /// </summary>
        public static void CheckForUpdates() {
            if (!Utils.IsSpamming("updates", 1000)) {
                UserCommunication.Notify("Now checking for updates, you will be notified when it's done", MessageImg.MsgInfo, "Update", "Update check", 5);
                Task.Factory.StartNew(() => {
                    GetLatestReleaseInfo(true);
                });
            }
        }

        /// <summary>
        /// Gets an object with the latest release info
        /// </summary>
        public static void GetLatestReleaseInfo(bool alwaysGetFeedBack) {

            if (_latestReleaseInfo != null) {
                // we already checked and there is a new version
                if (!_warnedUserAboutUpdateAvail || alwaysGetFeedBack)
                    NotifyUpdateAvailable();
                return;
            }

            if (!_lock.TryEnterWriteLock(100))
                return;

            try {
                using (WebClient wc = new WebClient()) {
                    wc.Proxy = Config.Instance.GetWebClientProxy();
                    wc.Headers.Add("user-agent", Config.GetUserAgent);
                    //wc.Proxy = null;

                    // Download release list from GITHUB API 
                    string json = "";
                    try {
                        json = wc.DownloadString(Config.ReleasesApi);
                        Config.Instance.LastCheckUpdateOk = true;
                    } catch (WebException e) {
                        if (alwaysGetFeedBack || Config.Instance.LastCheckUpdateOk) {
                            UserCommunication.NotifyUnique("ReleaseListDown", "For your information, I couldn't manage to retrieve the latest published version on github.<br><br>A request has been sent to :<br>" + Config.ReleasesApi.ToHtmlLink() + "<br>but was unsuccessul, you might have to check for a new version manually if this happens again.", MessageImg.MsgHighImportance, "Couldn't reach github", "Connection failed", null);
                            Config.Instance.LastCheckUpdateOk = false;
                        }
                        ErrorHandler.Log(e.ToString(), true);
                    }

                    // Parse the .json
                    var parser = new JsonParser(json);
                    parser.Tokenize();
                    var releasesList = parser.GetList();

                    // Releases list not empty?
                    if (releasesList != null) {
                        
                        var localVersion = AssemblyInfo.Version;
                        var outputBody = new StringBuilder();
                        var highestVersion = localVersion;
                        var highestVersionInt = -1;
                        var iCount = -1;

                        // for each release in the list...
                        foreach (var release in releasesList) {
                            iCount++;

                            var releaseVersionTuple = release.FirstOrDefault(tuple => tuple.Item1.Equals(ReleaseVersionName));
                            var prereleaseTuple = release.FirstOrDefault(tuple => tuple.Item1.Equals(BoolIsBetaName));
                            var releaseNameTuple = release.FirstOrDefault(tuple => tuple.Item1.Equals(ReleaseName));
                            var isDraftTuple = release.FirstOrDefault(tuple => tuple.Item1.Equals(BoolIsDraftName));

                            // don't care about draft release
                            if (isDraftTuple != null && isDraftTuple.Item2.EqualsCi("true"))
                                continue;
                            
                            if (releaseVersionTuple != null && prereleaseTuple != null) {

                                var releaseVersion = releaseVersionTuple.Item2;

                                // is it the highest version ? for prereleases or full releases depending on the user config
                                if (releaseVersion.IsHigherVersionThan(highestVersion) && (Config.Instance.UserGetsPreReleases || prereleaseTuple.Item2.EqualsCi("false"))) {
                                    highestVersion = releaseVersion;
                                    highestVersionInt = iCount;
                                }

                                // For each version higher than the local one, append to the release body
                                // Will be used to display the version log to the user
                                if (releaseVersion.IsHigherVersionThan(localVersion)) {
                                    // h1
                                    outputBody.AppendLine("\n\n## " + releaseVersion + ((releaseNameTuple != null) ? " : " + releaseNameTuple.Item2 : "") + " ##\n\n");
                                    // body
                                    var locBody = release.FirstOrDefault(tuple => tuple.Item1.Equals("body"));
                                    if (locBody != null)
                                        outputBody.AppendLine(locBody.Item2);
                                }
                            }
                        }

                        // There is a distant version higher than the local one
                        if (highestVersionInt > -1) {

                            // Update dir
                            Utils.DeleteDirectory(Config.FolderUpdate, true);
                            Utils.CreateDirectory(Config.FolderUpdate);

                            // latest release info
                            _latestReleaseInfo = new ReleaseInfo() {
                                Name = releasesList[highestVersionInt].First(tuple => tuple.Item1.Equals(ReleaseName)).Item2,
                                Version = releasesList[highestVersionInt].First(tuple => tuple.Item1.Equals(ReleaseVersionName)).Item2,
                                IsBeta = releasesList[highestVersionInt].First(tuple => tuple.Item1.Equals(BoolIsBetaName)).Item2.EqualsCi("true"),
                                ReleaseDate = releasesList[highestVersionInt].First(tuple => tuple.Item1.Equals(ReleaseDateName)).Item2.Substring(0, 10),
                                ReleaseUrl = releasesList[highestVersionInt].First(tuple => tuple.Item1.Equals(ReleaseUrlName)).Item2,
                                Body = outputBody.ToString().Replace("\\r\\n", "\n").Replace("\\n", "\n")
                            };

                            // Hookup DownloadFileCompleted Event, download the release .zip
                            var downloadUriTuple = releasesList[highestVersionInt].FirstOrDefault(tuple => tuple.Item1.Equals(ReleaseDownloadUrlName));
                            if (downloadUriTuple != null) {
                                _latestReleaseInfo.DownloadUrl = downloadUriTuple.Item2;
                                wc.DownloadFileCompleted += WcOnDownloadFileCompleted;
                                wc.DownloadFileAsync(new Uri(_latestReleaseInfo.DownloadUrl), Config.FileLatestReleaseZip);
                            }

                        } else if (alwaysGetFeedBack) {
                            UserCommunication.Notify("Congratulations! You already possess the latest version of 3P!", MessageImg.MsgOk, "Update check", "You own the version " + AssemblyInfo.Version);
                        }
                    }
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "GetLatestReleaseInfo");
            } finally {
                _lock.ExitWriteLock();
            }
        }

        #endregion

        #region private

        /// <summary>
        /// Called when the latest release download is done
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="asyncCompletedEventArgs"></param>
        private static void WcOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs) {
            try {
                // Extract the .zip file
                if (!Utils.ExtractAll(Config.FileLatestReleaseZip, Config.FolderUpdate)) {

                    // check the presence of the plugin file
                    if (File.Exists(Config.FileDownloadedPlugin)) {

                        // set up the update so the .dll file downloaded replaces the current .dll
                        _3PUpdater.Instance.AddFileToMove(Config.FileDownloadedPlugin, AssemblyInfo.Location);

                        // write the version log
                        File.WriteAllText(Config.FileVersionLog, _latestReleaseInfo.Body, Encoding.Default);
                        
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
        }

        private static void NotifyUpdateAvailable() {
            if (_latestReleaseInfo != null) {
                UserCommunication.NotifyUnique("UpdateAvailable", @"Dear user, <br>
                    <br>
                    A new version of 3P is available on github and will be automatically installed the next time you restart Notepad++<br>
                    <br>
                    Your version: <b>" + AssemblyInfo.Version + @"</b><br>
                    Distant version: <b>" + _latestReleaseInfo.Version + @"</b><br>
                    Release name: <b>" + _latestReleaseInfo.Name + @"</b><br>
                    Available since: <b>" + _latestReleaseInfo.ReleaseDate + @"</b><br>
                    Release URL: <b>" + _latestReleaseInfo.ReleaseUrl.ToHtmlLink()+ @"</b><br>" +
                    ((Config.Instance.UserGetsPreReleases && _latestReleaseInfo.IsBeta) ? "This distant release is not flagged as a stable version<br>" : "") +
                    ((_3PUpdater.Instance.IsAdminRightsNeeded) ? "<br><span class='SubTextColor'><i><b>3pUpdater.exe</b> will need admin rights to replace your current 3P.dll file by the new release,<br>please click yes when you are asked to execute it</i></span>" : ""), MessageImg.MsgUpdate, "Update check", "An update is available", null);

                _warnedUserAboutUpdateAvail = true;

                // stop checking for more updates :)
                _checkEveryHourAction.Stop();
                _checkEveryHourAction = null;
            }
        }

        #endregion

        #region ReleaseInfo

        /// <summary>
        /// Contains info about the latest release
        /// </summary>
        public class ReleaseInfo {
            public string Version { get; set; }
            public string Name { get; set; }
            public bool IsBeta { get; set; }
            public string ReleaseUrl { get; set; }
            public string Body { get; set; }
            public string ReleaseDate { get; set; }
            public string DownloadUrl { get; set; }
        }

        #endregion

    }

}
