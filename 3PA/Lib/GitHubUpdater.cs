#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (GitHubUpdater.cs) is part of 3P.
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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;

namespace _3PA.Lib {

    internal class GitHubUpdater {

        #region fields

        /// <summary>
        /// Github release api URL
        /// </summary>
        public string GitHubReleaseApi { get; set; }

        /// <summary>
        /// Basic authen for github api
        /// </summary>
        public string BasicAuthenticationToken { get; set; }

        /// <summary>
        /// Local version of the soft that will be compared to the distant one
        /// </summary>
        public string LocalVersion { get; set; }

        /// <summary>
        /// Set to false to ignore releases marked as pre-release on github
        /// </summary>
        public bool GetPreReleases { get; set; }

        /// <summary>
        /// Folder path in which to download the asset file
        /// </summary>
        public string AssetDownloadFolder { get; set; }

        /// <summary>
        /// Name of the file to download from the github release (if you don't set GetDownloadUrl, 
        /// it will try to find this asset name online and it will download it with this filename on your computer
        /// </summary>
        public string AssetName { get; set; }

        /// <summary>
        /// You can overload the method used to find the url of the file to download from the latest release
        /// </summary>
        public Func<ReleaseInfo, string> GetDownloadUrl { get; set; }

        /// <summary>
        /// A concatenation of all the version log from the version updated to the latest version
        /// </summary>
        public StringBuilder VersionLog { get; set; }

        /// <summary>
        /// Info on the latest release when the github webservice responded
        /// </summary>
        public ReleaseInfo LatestReleaseInfo { get; set; }

        /// <summary>
        /// Called after the download, path to the downloaded file
        /// </summary>
        public event Action<GitHubUpdater, string> NewReleaseDownloaded;

        /// <summary>
        /// Called when the soft is up to date
        /// </summary>
        public event Action<GitHubUpdater, ReleaseInfo> AlreadyUpdated;

        /// <summary>
        /// Called when the soft is being updated because we found a newer distant version,
        /// called before the download starts, you can cancel the download through the event
        /// </summary>
        public event Action<GitHubUpdater, ReleaseInfo, StartingDownloadEvent> StartingUpdate;

        /// <summary>
        /// Called on error
        /// </summary>
        public event Action<GitHubUpdater, Exception, GitHubUpdaterFailReason> ErrorOccured;

        #endregion

        #region public

        /// <summary>
        /// Gets an object with the latest release info
        /// </summary>
        public void CheckForUpdates() {
            try {
                var wb = new WebServiceJson(WebServiceJson.WebRequestMethod.Get, GitHubReleaseApi) {
                    TimeOut = 3000
                };
                wb.OnInitHttpWebRequest += request => {
                    request.Headers.Add("Authorization", "Basic " + BasicAuthenticationToken);
                    request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

                };
                wb.OnRequestEnded += OnGithubResponse;
                wb.Execute();
            } catch (Exception e) {
                if (ErrorOccured != null)
                    ErrorOccured(this, e, GitHubUpdaterFailReason.RequestFailed);
            }
        }

        /// <summary>
        /// Called when the gitub api for releases responses
        /// </summary>
        private void OnGithubResponse(WebServiceJson webServiceJson) {
            try {
                if (webServiceJson.StatusCodeResponse == HttpStatusCode.OK && webServiceJson.ResponseException == null) {

                    LatestReleaseInfo = null;

                    // get the releases
                    var releases = webServiceJson.DeserializeArray<ReleaseInfo>();

                    // case where we put the url for the latest release only
                    if (releases != null && releases.Count == 0) {
                        var latestRelease = webServiceJson.Deserialize<ReleaseInfo>();
                        if (latestRelease != null) {
                            releases = new List<ReleaseInfo> { latestRelease };
                        }
                    }

                    if (releases != null && releases.Count > 0) {
                        // sort descending
                        releases.Sort((o, o2) => o.tag_name.IsHigherVersionThan(o2.tag_name) ? -1 : 1);

                        VersionLog = new StringBuilder();
                        foreach (var release in releases) {
                            if (string.IsNullOrEmpty(release.tag_name))
                                continue;

                            // For each version higher than the local one, append to the release body
                            // Will be used to display the version log to the user
                            if (release.tag_name.IsHigherVersionThan(LocalVersion) && (GetPreReleases || !release.prerelease)) {
                                // h1
                                VersionLog.AppendLine("## " + (release.tag_name ?? "?") + " : " + (release.name ?? "") + " ##\n\n");
                                // body
                                VersionLog.AppendLine((release.body ?? "") + "\n\n");

                                // the first higher release encountered is the latest
                                if (LatestReleaseInfo == null)
                                    LatestReleaseInfo = release;
                            }
                        }

                        // There is a distant version higher than the local one
                        if (LatestReleaseInfo != null) {

                            var url = GetDownloadUrl != null ? GetDownloadUrl(LatestReleaseInfo) : DefaultGetDownloadUrl(LatestReleaseInfo);
                            if (!string.IsNullOrEmpty(url)) {

                                var downloadFile = Path.Combine(AssetDownloadFolder, AssetName);
                                Utils.CreateDirectory(AssetDownloadFolder);
                                Utils.DeleteFile(downloadFile);

                                var e = new StartingDownloadEvent();

                                if (StartingUpdate != null)
                                    StartingUpdate(this, LatestReleaseInfo, e);

                                if (!e.CancelDownload) {
                                    Utils.DownloadFile(url, downloadFile, OnAssetDownloaded);
                                }
                            } else {
                                if (ErrorOccured != null)
                                    ErrorOccured(this, new Exception("Asset not found"), GitHubUpdaterFailReason.NoAssetOnLatestRelease);
                            }
                            
                        } else {
                            if (AlreadyUpdated != null)
                                AlreadyUpdated(this, releases.First());
                        }
                    }
                } else {
                    if (ErrorOccured != null)
                        ErrorOccured(this, new Exception("Update error", webServiceJson.ResponseException), GitHubUpdaterFailReason.ReleaseApiUnreachable);
                }
            } catch (Exception e) {
                if (ErrorOccured != null)
                    ErrorOccured(this, e, GitHubUpdaterFailReason.AnalyseReleasesFailed);
            }
        }

        #endregion

        #region private

        /// <summary>
        /// Called when the asset download is done
        /// </summary>
        private void OnAssetDownloaded(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs) {
            try {
                if (asyncCompletedEventArgs.Error != null) {
                    if (ErrorOccured != null)
                        ErrorOccured(this, new Exception("Release download error", asyncCompletedEventArgs.Error), GitHubUpdaterFailReason.AssetDownloadFailed);
                } else {
                    var downloadFile = Path.Combine(AssetDownloadFolder, AssetName);
                    if (File.Exists(downloadFile)) {
                        if (NewReleaseDownloaded != null)
                            NewReleaseDownloaded(this, downloadFile);
                        Utils.DeleteFile(downloadFile);
                    }
                }
            } catch (Exception e) {
                if (ErrorOccured != null)
                    ErrorOccured(this, e, GitHubUpdaterFailReason.AssetDownloadFailed);
            }
        }

        /// <summary>
        /// default way to find the url to download
        /// </summary>
        private string DefaultGetDownloadUrl(ReleaseInfo release) {
            if (release.assets != null && release.assets.Count > 0 && release.assets.Exists(asset => asset.name.EqualsCi(AssetName))) {
                return release.assets.First(asset => asset.name.EqualsCi(AssetName)).browser_download_url;
            }
            return null;
        }

        #endregion

        #region GitHubUpdaterFailReason

        public enum GitHubUpdaterFailReason {
            RequestFailed,
            ReleaseApiUnreachable,
            AnalyseReleasesFailed,
            AssetDownloadFailed,
            NoAssetOnLatestRelease,
        }

        #endregion

        #region StartingDownloadEvent

        public class StartingDownloadEvent : EventArgs {
            public bool CancelDownload { get; set; }
        }

        #endregion

        #region ReleaseInfo

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
            /// Url of the zip containing the source code
            /// </summary>
            public string zipball_url { get; set; }

            /// <summary>
            /// content of the release text
            /// </summary>
            public string body { get; set; }

        }

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

        #endregion

    }
}
