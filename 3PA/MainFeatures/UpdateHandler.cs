#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MarkdownDeep;
using _3PA.Html;
using _3PA.Lib;
using _3PA.MainFeatures.Parser;
using _3PA.Properties;
using Token = _3PA.MainFeatures.Parser.Token;
using Utils = _3PA.Lib.Utils;

namespace _3PA.MainFeatures {

    /// <summary>
    /// Handles the update of this software
    /// </summary>
    public class UpdateHandler {

        private static string PathUpdateFolder { get { return Path.Combine(Npp.GetConfigDir(), "Update"); } }

        private static string PathDownloadedPlugin { get { return Path.Combine(Npp.GetConfigDir(), "Update", "3P.dll"); } }

        private static string Path7ZipExe { get { return Path.Combine(Npp.GetConfigDir(), "Update", "7z.exe"); } }

        private static string Path7ZipDll { get { return Path.Combine(Npp.GetConfigDir(), "Update", "7z.dll"); } }

        private static string PathUpdaterExe { get { return Path.Combine(Npp.GetConfigDir(), "Update", "3pUpdater.exe"); } }

        private static string PathLatestReleaseZip { get { return Path.Combine(Npp.GetConfigDir(), "Update", "latestRelease.zip"); } }

        private static string PathToVersionLog { get { return Path.Combine(Npp.GetConfigDir(), "version.log"); } }

        /// <summary>
        /// Holds the info about the latest release found on the distant update server
        /// </summary>
        public static ReleaseInfo LatestReleaseInfo { get; set; }

        /// <summary>
        /// Gets an object with the latest release info
        /// </summary>
        public static void GetLatestReleaseInfo() {
            GetLatestReleaseInfo(false);
        }

        /// <summary>
        /// Gets an object with the latest release info
        /// </summary>
        public static void GetLatestReleaseInfo(bool alwaysGetFeedBack) {

            try {
                using (WebClient wc = new WebClient()) {

                    /* Need a proxy for sopra?
                WebProxy proxy = new WebProxy();
                proxy.Address = new Uri("mywebproxyserver.com");
                proxy.Credentials = new NetworkCredential("usernameHere", "pa****rdHere"); 
                proxy.UseDefaultCredentials = false;
                proxy.BypassProxyOnLocal = false;
                wc.Proxy = proxy;
                */

                    wc.Headers.Add ("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                    //wc.Proxy = null;

                    // Download release list from GITHUB API 
                    string json = "";
                    try {
                        json = wc.DownloadString(Config.ReleasesUrl);
                    } catch (WebException e) {
                        UserCommunication.Notify("For your information, I couldn't manage to retrieve the latest published version on github.<br><br>A request has been sent to :<br><a href='" + Config.ReleasesUrl + "'>" + Config.ReleasesUrl + "</a><br>but was unsuccessul, you might have to check for a new version manually if this happens again.", MessageImg.MsgHighImportance, "Couldn't reach github", "Connection failed");
                        ErrorHandler.Log(e.ToString());
                    }

                    // Parse the .json
                    var parser = new JsonParser(json);
                    parser.Tokenize();
                    var releasesList = parser.GetList();

                    // Releases list empty?
                    if (releasesList == null)
                        return;

                    var localVersion = AssemblyInfo.Version;

                    var outputBody = new StringBuilder();
                    var highestVersion = localVersion;
                    var highestVersionInt = -1;
                    var iCount = 0;
                    foreach (var release in releasesList) {
                        var releaseVersionTuple = release.FirstOrDefault(tuple => tuple.Item1.Equals("tag_name"));
                        var prereleaseTuple = release.FirstOrDefault(tuple => tuple.Item1.Equals("prerelease"));
                        var releaseNameTuple = release.FirstOrDefault(tuple => tuple.Item1.Equals("name"));

                        if (releaseVersionTuple != null && prereleaseTuple != null) {

                            var releaseVersion = releaseVersionTuple.Item2;

                            // is it the highest version ? for prereleases or full releases depending on the user config
                            if (((Config.Instance.UserGetsPreReleases && prereleaseTuple.Item2.EqualsCi("true"))
                                 || (!Config.Instance.UserGetsPreReleases && prereleaseTuple.Item2.EqualsCi("false")))
                                && releaseVersion.IsHigherVersionThan(highestVersion)) {
                                highestVersion = releaseVersion;
                                highestVersionInt = iCount;
                            }

                            // For each version higher than the local one, append to the release body
                            // Will be used to display the version log to the user
                            if (releaseVersion.IsHigherVersionThan(localVersion)) {
                                outputBody.AppendLine("\n\n## " + releaseVersion + ((releaseNameTuple != null) ? " : " + releaseNameTuple.Item2 : "") + " ##\n\n");
                                var locBody = release.FirstOrDefault(tuple => tuple.Item1.Equals("body"));
                                if (locBody != null)
                                    outputBody.AppendLine(locBody.Item2);
                            }
                        }
                        iCount++;
                    }

                    // There is a distant version higher than the local one
                    if (highestVersionInt > -1) {
                        // Update dir
                        if (Directory.Exists(PathUpdateFolder))
                            Utils.DeleteDirectory(PathUpdateFolder, true);
                        Directory.CreateDirectory(PathUpdateFolder);

                        // latest release info
                        try {
                            LatestReleaseInfo = new ReleaseInfo(
                                releasesList[highestVersionInt].First(tuple => tuple.Item1.Equals("tag_name")).Item2,
                                releasesList[highestVersionInt].First(tuple => tuple.Item1.Equals("name")).Item2,
                                releasesList[highestVersionInt].First(tuple => tuple.Item1.Equals("prerelease")).Item2.EqualsCi("true"),
                                releasesList[highestVersionInt].First(tuple => tuple.Item1.Equals("html_url")).Item2,
                                outputBody.ToString().Replace("\\r\\n", "\n").Replace("\\n", "\n"),
                                releasesList[highestVersionInt].First(tuple => tuple.Item1.Equals("draft")).Item2.EqualsCi("true"),
                                releasesList[highestVersionInt].First(tuple => tuple.Item1.Equals("updated_at")).Item2.Substring(0, 10)
                                );
                        } catch (Exception) {
                            // ignored
                        }

                        // Hookup DownloadFileCompleted Event, download the release .zip
                        var downloadUriTuple = releasesList[highestVersionInt].FirstOrDefault(tuple => tuple.Item1.Equals("browser_download_url"));
                        if (downloadUriTuple != null) {
                            wc.DownloadFileCompleted += WcOnDownloadFileCompleted;
                            wc.DownloadFileAsync(new Uri(downloadUriTuple.Item2), PathLatestReleaseZip);
                        }

                    } else if (alwaysGetFeedBack) {
                        UserCommunication.Notify("You already possess the latest version of 3P!", MessageImg.MsgOk, "Update check", "You own the " + AssemblyInfo.Version);
                    }
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "GetLatestReleaseInfo");
            }
        }

        /// <summary>
        /// Called when the latest release download is done
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="asyncCompletedEventArgs"></param>
        private static void WcOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs) {
            try {
                // copy 7zip.exe
                if (!File.Exists(Path7ZipExe))
                    File.WriteAllBytes(Path7ZipExe, Resources._7z);
                if (!File.Exists(Path7ZipDll))
                    File.WriteAllBytes(Path7ZipDll, Resources._7zdll);

                // Extract the .zip file
                var process = Process.Start(new ProcessStartInfo {
                    FileName = Path7ZipExe,
                    Arguments = string.Format("x -y \"-o{0}\" \"{1}\"", Path.Combine(Npp.GetConfigDir(), "Update"), PathLatestReleaseZip),
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
                if (process != null)
                    process.WaitForExit();

                // check the presence of the plugin file
                if (!File.Exists(PathDownloadedPlugin)) {
                    Utils.DeleteDirectory(PathUpdateFolder, true);
                    return;
                }

                // copy the 3pUpdater.exe, which basically copies the downloaded version of the plugin into the /plugins/ dir
                if (!File.Exists(PathUpdaterExe))
                    File.WriteAllBytes(PathUpdaterExe, Resources._3pUpdater);

                // write the version log
                File.WriteAllText(PathToVersionLog, LatestReleaseInfo.Body);

                UserCommunication.Notify(@"Dear user, <br>
                <br>
                a new version of 3P is available on github and will be automatically installed the next time you restart notepad++<br>
                <br>
                Your version : <b>" + AssemblyInfo.Version + @"</b><br>
                Distant version : <b>" + LatestReleaseInfo.Version + @"</b><br>
                Release name : <b>" + LatestReleaseInfo.Name + @"</b><br>
                Available since : <b>" + LatestReleaseInfo.ReleaseDate + @"</b><br>
                Release URL : <b><a href='" + LatestReleaseInfo.ReleaseUrl + "'>" + LatestReleaseInfo.ReleaseUrl + @"</a></b><br>" +
                                         (!Config.Instance.UserGetsPreReleases ? "" : "Is it a pre-release : " + LatestReleaseInfo.IsPreRelease) + "<br>", MessageImg.MsgUpdate, "Update check", "An update is available");
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "WcOnDownloadFileCompleted");
            }
        }

        /// <summary>
        /// Method to call when the user leaves notepad++,
        /// check if there is an update to make
        /// </summary>
        public static void OnNotepadExit() {
            if (File.Exists(PathUpdaterExe))
                Process.Start(PathUpdaterExe);
        }

        /// <summary>
        /// Method to call when the user starts notepad++,
        /// check if an update has been done since the last time notepad was closed
        /// </summary>
        public static void OnNotepadStart() {
            try {
                // an update has been done
                if (File.Exists(PathToVersionLog)) {

                    if (File.Exists(PathDownloadedPlugin)) {
                        UserCommunication.Notify(@"<h2>I require your attention!</h2><br>
                        The update didn't go as expected, i couldn't replace the old plugin file by the new one!<br>
                        It is very likely because i didn't get the rights to write a file in your /plugins/ folder, don't panic!<br>
                        You will have to manually copy the new file and delete the old file :<br><br>
                        Copy this file : <b><a href='" + PathDownloadedPlugin + "'>" + PathDownloadedPlugin + @"</a></b><br>" + @"
                        In this folder (replacing the old file) : <b><a href='" + Path.GetFullPath(Path.Combine(Npp.GetConfigDir(), "../")) + "'>" + Path.GetFullPath(Path.Combine(Npp.GetConfigDir(), "../")) + @"</a></b><br>
                        Please do it as soon as possible, as i will stop checking for more updates until this problem is fixed.<br>
                        Thank you for your patience!<br>", MessageImg.MsgUpdate, "Update", "Problem during the update!");
                        return;
                    }

                    var md = new Markdown();
                    UserCommunication.Message(md.Transform("# What's new in this version? #\n\n" + File.ReadAllText(PathToVersionLog, TextEncodingDetect.GetFileEncoding(PathToVersionLog))),
                        MessageImg.MsgUpdate, 
                        "A new version has been installed!",
                        "Updated to version " + AssemblyInfo.Version,
                        new List<string> { "ok" }, 
                        false,
                        null,
                        false);

                    File.Delete(PathToVersionLog);

                    if (Directory.Exists(PathUpdateFolder))
                        Utils.DeleteDirectory(PathUpdateFolder, true);
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "OnNotepadStart");
            }

            // Check for new updates
            Task.Factory.StartNew(GetLatestReleaseInfo);
        }

    }

    /// <summary>
    /// Contains info about the latest release
    /// </summary>
    public class ReleaseInfo {
        public string Version { private set; get; }
        public string Name { private set; get; }
        public bool IsPreRelease { private set; get; }
        public bool IsDraft { private set; get; }
        public string ReleaseUrl { private set; get; }
        public string Body { private set; get; }
        public string ReleaseDate { private set; get; }

        public ReleaseInfo(string version, string name, bool isPreRelease, string releaseUrl, string body, bool isDraft, string releaseDate) {
            Version = version;
            Name = name;
            IsPreRelease = isPreRelease;
            ReleaseUrl = releaseUrl;
            Body = body;
            IsDraft = isDraft;
            ReleaseDate = releaseDate;
        }
    }


    /// <summary>
    /// TODO: This class is too spcific and must be refactored later... for now it will do
    /// </summary>
    public class JsonParser {
        private const char Eof = (char)0;
        private string _data;
        private int _pos;
        private int _startPos;
        private int _tokenPos;
        private char[] _symbolChars;
        private List<Token> _tokenList = new List<Token>();

        /// <summary>
        /// constructor, data is the input string to tokenize
        /// call Tokenize() to do the work
        /// </summary>
        /// <param name="data"></param>
        public JsonParser(string data) {
            if (data == null)
                throw new ArgumentNullException("data");
            _data = data;
            _pos = 0;
            _tokenPos = 0;
            _symbolChars = new[] { '[', ']', '{', '}', ',', ':' };
        }

        /// <summary>
        /// Call this method to actually tokenize the string
        /// </summary>
        public void Tokenize() {
            Token token;
            do {
                token = GetNext();
                _tokenList.Add(token);
            } while (!(token is TokenEof));
        }

        /// <summary>
        /// Returns a List of list of key/value pairs...
        /// </summary>
        /// <returns></returns>
        public List<List<Tuple<string, string>>> GetList() {
            var releaseJsonontent = new List<List<Tuple<string, string>>> { new List<Tuple<string, string>>() };
            var outerI = 0;
            var bracketCount = 0;
            Token token;
            do {
                _tokenPos++;
                token = PeekAtToken(0);
                if (token is TokenSymbol) {
                    if (token.Value.Equals("{"))
                        bracketCount++;
                    if (token.Value.Equals("}")) {
                        bracketCount--;
                        if (bracketCount == 0) {
                            outerI++;
                            releaseJsonontent.Add(new List<Tuple<string, string>>());
                        }
                    }
                }
                if (token is TokenWord) {

                    if (PeekAtToken(1).Value.Equals(":")) {
                        var nextWordPos = -1;
                        if (PeekAtToken(2) is TokenWhiteSpace && PeekAtToken(3) is TokenWord)
                            nextWordPos = 3;
                        else if (PeekAtToken(2) is TokenWord)
                            nextWordPos = 2;
                        if (nextWordPos > 0) {
                            var varName = token.Value;
                            if (varName[0] == '"')
                                varName = varName.Substring(1, varName.Length - 2);
                            var varValue = PeekAtToken(nextWordPos).Value;
                            if (varValue[0] == '"')
                                varValue = varValue.Substring(1, varValue.Length - 2);
                            releaseJsonontent[outerI].Add(new Tuple<string, string>(varName, varValue));
                            _tokenPos = _tokenPos + nextWordPos;
                        }
                    }
                }
            } while (!(token is TokenEof));
            if (outerI == 0 && releaseJsonontent[outerI].Count == 0)
                return null;
            return releaseJsonontent;
        }

        /// <summary>
        /// To use this lexer as an enumerator,
        /// peek at the current pos + x token of the list, returns a new TokenEof if can't find
        /// </summary>
        /// <returns></returns>
        public Token PeekAtToken(int x) {
            return (_tokenPos + x >= _tokenList.Count || _tokenPos + x < 0) ? new TokenEof("", 0, 0, 0, 0) : _tokenList[_tokenPos + x];
        }

        /// <summary>
        /// Peek forward x chars
        /// </summary>
        private char PeekAt(int x) {
            return _pos + x >= _data.Length ? Eof : _data[_pos + x];
        }

        /// <summary>
        /// Read to the next char,
        /// indirectly adding the current char (_data[_pos]) to the current token
        /// </summary>
        private void Read() {
            _pos++;
        }

        /// <summary>
        /// Returns the current value of the token
        /// </summary>
        /// <returns></returns>
        private string GetTokenValue() {
            return _data.Substring(_startPos, _pos - _startPos);
        }

        /// <summary>
        /// returns the next token of the string
        /// </summary>
        /// <returns></returns>
        private Token GetNext() {
            _startPos = _pos;

            var ch = PeekAt(0);

            // END OF FILE reached
            if (ch == Eof)
                return new TokenEof(GetTokenValue(), 0, 0, _startPos, _pos);

            switch (ch) {
                case '"':
                    ReadString(ch);
                    return new TokenWord(GetTokenValue(), 0, 0, _startPos, _pos);

                case ' ':
                case '\t':
                case '\n':
                case '\r':
                    // whitespaces or tab
                    return CreateWhitespaceToken();

                default:
                    // keyword
                    if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '-') {
                        ReadWord();
                        return new TokenWord(GetTokenValue(), 0, 0, _startPos, _pos);
                    }

                    // symbol
                    if (_symbolChars.Any(t => t == ch))
                        return CreateSymbolToken();
                    // unknown char
                    Read();
                    return new TokenUnknown(GetTokenValue(), 0, 0, _startPos, _pos);
            }
        }

        private Token CreateSymbolToken() {
            Read();
            return new TokenSymbol(GetTokenValue(), 0, 0, _startPos, _pos);
        }

        /// <summary>
        /// create a whitespace token (successions of either ' ' or '\t')
        /// </summary>
        /// <returns></returns>
        private Token CreateWhitespaceToken() {
            Read();
            while (true) {
                var ch = PeekAt(0);
                if (ch == '\t' || ch == ' ' || ch == '\r' || ch == '\n')
                    Read();
                else
                    break;
            }
            return new TokenWhiteSpace(GetTokenValue(), 0, 0, _startPos, _pos);
        }


        /// <summary>
        /// reads a word with this format : [a-Z_&]+[\w_-]*((\.[\w_-]*)?){1,}
        /// </summary>
        private void ReadWord() {
            Read();
            while (true) {
                var ch = PeekAt(0);
                // normal word
                if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '-')
                    Read();
                else {
                    // reads a base.table.field as a single word
                    if (ch == '.') {
                        var car = PeekAt(1);
                        if (char.IsLetterOrDigit(car) || car == '_' || car == '-') {
                            Read();
                            continue;
                        }
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// reads a string " "
        /// </summary>
        /// <param name="strChar"></param>
        private void ReadString(char strChar) {
            Read();
            while (true) {
                var ch = PeekAt(0);
                if (ch == Eof)
                    break;
                // quote char
                if (ch == strChar) {
                    Read();
                    break; // done reading
                }
                // escape char (read anything as part of the string after that)
                if (ch == '\\')
                    Read();
                Read();
            }
        }
    }
}
