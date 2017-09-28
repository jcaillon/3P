using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
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
        /// Name of the file to download from the github release
        /// </summary>
        public string AssetName { get; set; }

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
                wb.OnInitHttpWebRequest += request => { request.Headers.Add("Authorization", "Basic " + BasicAuthenticationToken); };
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

                    ReleaseInfo latestReleaseInfo = null;

                    // get the releases
                    var releases = webServiceJson.DeserializeArray<ReleaseInfo>();
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
                                if (latestReleaseInfo == null)
                                    latestReleaseInfo = release;
                            }
                        }

                        // There is a distant version higher than the local one
                        if (latestReleaseInfo != null) {

                            if (latestReleaseInfo.assets != null && latestReleaseInfo.assets.Count > 0 && latestReleaseInfo.assets.Exists(asset => asset.name.EqualsCi(AssetName))) {

                                var downloadFile = Path.Combine(AssetDownloadFolder, AssetName);
                                Utils.CreateDirectory(AssetDownloadFolder);
                                Utils.DeleteFile(downloadFile);

                                var e = new StartingDownloadEvent();

                                if (StartingUpdate != null)
                                    StartingUpdate(this, latestReleaseInfo, e);

                                if (!e.CancelDownload) {
                                    Utils.DownloadFile(latestReleaseInfo.assets.First(asset => asset.name.EqualsCi(AssetName)).browser_download_url, downloadFile, OnAssetDownloaded);
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
                        ErrorOccured(this, webServiceJson.ResponseException, GitHubUpdaterFailReason.ReleaseApiUnreachable);
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
                        ErrorOccured(this, asyncCompletedEventArgs.Error, GitHubUpdaterFailReason.AssetDownloadFailed);
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
