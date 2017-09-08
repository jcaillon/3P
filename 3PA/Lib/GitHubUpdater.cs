using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using _3PA.MainFeatures;

namespace _3PA.Lib {

    internal class GitHubUpdater {

        #region fields

        public string GitHubReleaseApi { get; set; }
        public string BasicAuthenticationToken { get; set; }
        public string LocalVersion { get; set; }
        public bool GetPreReleases { get; set; }
        public string AssetDownloadFolder { get; set; }
        public string AssetName { get; set; }

        public event Action<GitHubUpdater, string> NewReleaseDownloaded;
        public event Action<GitHubUpdater, ReleaseInfo> AlreadyUpdated;
        public event Action<GitHubUpdater, GitHubUpdaterFailReason> ErrorOccured;

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
                ErrorHandler.ShowErrors(e, "Error when checking updates at " + GitHubReleaseApi);
                if (ErrorOccured != null)
                    ErrorOccured(this, GitHubUpdaterFailReason.RequestFailed);
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

                        var outputBody = new StringBuilder();
                        foreach (var release in releases) {
                            if (string.IsNullOrEmpty(release.tag_name))
                                continue;

                            // For each version higher than the local one, append to the release body
                            // Will be used to display the version log to the user
                            if (release.tag_name.IsHigherVersionThan(LocalVersion) && (GetPreReleases || !release.prerelease)) {
                                // h1
                                outputBody.AppendLine("## " + (release.tag_name ?? "?") + " : " + (release.name ?? "") + " ##\n\n");
                                // body
                                outputBody.AppendLine((release.body ?? "") + "\n\n");

                                // the first higher release encountered is the latest
                                if (latestReleaseInfo == null)
                                    latestReleaseInfo = release;
                            }
                        }

                        // There is a distant version higher than the local one
                        if (latestReleaseInfo != null) {
                            // to display all the release notes
                            latestReleaseInfo.body = outputBody.ToString();

                            if (latestReleaseInfo.assets != null && latestReleaseInfo.assets.Count > 0 && latestReleaseInfo.assets.Exists(asset => asset.name.EqualsCi(AssetName))) {

                                var downloadFile = Path.Combine(AssetDownloadFolder, AssetName);
                                Utils.DeleteFile(downloadFile);
                                Utils.DownloadFile(latestReleaseInfo.assets.First(asset => asset.name.EqualsCi(AssetName)).browser_download_url, downloadFile, OnAssetDownloaded);
                            } else {
                                if (ErrorOccured != null)
                                    ErrorOccured(this, GitHubUpdaterFailReason.NoAssetOnLatestRelease);
                            }
                        } else {
                            if (AlreadyUpdated != null)
                                AlreadyUpdated(this, releases.First());
                        }
                    }
                } else {
                    if (ErrorOccured != null)
                        ErrorOccured(this, GitHubUpdaterFailReason.ReleaseApiUnreachable);
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error when checking updates at " + GitHubReleaseApi);
                if (ErrorOccured != null)
                    ErrorOccured(this, GitHubUpdaterFailReason.AnalyseReleasesFailed);
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
                    ErrorHandler.ShowErrors(asyncCompletedEventArgs.Error);
                    if (ErrorOccured != null)
                        ErrorOccured(this, GitHubUpdaterFailReason.AssetDownloadFailed);
                } else {
                    var downloadFile = Path.Combine(AssetDownloadFolder, AssetName);
                    if (File.Exists(downloadFile)) {
                        if (NewReleaseDownloaded != null)
                            NewReleaseDownloaded(this, downloadFile);
                    }
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error when receiving updates from " + GitHubReleaseApi);
                if (ErrorOccured != null)
                    ErrorOccured(this, GitHubUpdaterFailReason.AssetDownloadFailed);
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
            public List<UpdateHandler.Asset> assets { get; set; }

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
