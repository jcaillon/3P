using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using YamuiFramework.Forms;
using _3PA.MainFeatures;
using _3PA.MainFeatures.Parser;
using _3PA.Properties;

namespace _3PA.Lib {

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

        private static string PathLatestReleaseBin { get { return Path.Combine(Npp.GetConfigDir(), "Update", "latestRelease.bin"); } }

        /// <summary>
        /// Holds the info about the latest release found on the distant update server
        /// </summary>
        public static ReleaseInfo LatestReleaseInfo { get; set; }

        /// <summary>
        /// Gets an object with the latest release info
        /// </summary>
        /// <returns></returns>
        public static void GetLatestReleaseInfo() {

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
                wc.Proxy = null;

                // Download release list from GITHUB API 
                //var json = wc.DownloadString(Config.ReleasesUrl);

                // Parse the .json
                var parser = new JsonParser(File.ReadAllText(@"C:\Users\Julien\Desktop\releases.json"));
                parser.Tokenize();
                var releasesList = parser.GetList();

                // Releases list empty?
                if (releasesList[0].Count == 0 && releasesList.Count == 1)
                    return;

                var localVersion = AssemblyInfo.Version;

                var outputBody = new StringBuilder();
                var highestVersion = localVersion;
                var highestVersionInt = -1;
                var iCount = 0;
                foreach (var release in releasesList) {

                    var releaseVersionTuple = release.Find(tuple => tuple.Item1.Equals("tag_name"));
                    var prereleaseTuple = release.Find(tuple => tuple.Item1.Equals("prerelease"));

                    if (releaseVersionTuple != null && prereleaseTuple != null) {

                        var releaseVersion = releaseVersionTuple.Item2.StartsWith("v") ? releaseVersionTuple.Item2.Remove(0, 1) : releaseVersionTuple.Item2;

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
                            outputBody.AppendLine("\n\n##Version " + releaseVersion + "##\n");
                            var locBody = release.Find(tuple => tuple.Item1.Equals("body"));
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
                        Directory.Delete(PathUpdateFolder, true);
                    Directory.CreateDirectory(PathUpdateFolder);

                    // Hookup DownloadFileCompleted Event
                    var downloadUriTuple = releasesList[highestVersionInt].Find(tuple => tuple.Item1.Equals("browser_download_url"));
                    if (downloadUriTuple != null) {
                        wc.DownloadFileCompleted += WcOnDownloadFileCompleted;
                        wc.DownloadFileAsync(new Uri(downloadUriTuple.Item2), PathLatestReleaseZip);
                    }

                    // latest release info
                    try {
                        LatestReleaseInfo = new ReleaseInfo(
                            releasesList[highestVersionInt].Find(tuple => tuple.Item1.Equals("tag_name")).Item2,
                            releasesList[highestVersionInt].Find(tuple => tuple.Item1.Equals("name")).Item2,
                            releasesList[highestVersionInt].Find(tuple => tuple.Item1.Equals("prerelease")).Item2.EqualsCi("true"),
                            releasesList[highestVersionInt].Find(tuple => tuple.Item1.Equals("html_url")).Item2,
                            outputBody.ToString(),
                            releasesList[highestVersionInt].Find(tuple => tuple.Item1.Equals("draft")).Item2.EqualsCi("true"),
                            releasesList[highestVersionInt].Find(tuple => tuple.Item1.Equals("updated_at")).Item2.Substring(0, 10)
                            );

                        // Save release info
                        BinWriter.WriteToBinaryFile(PathLatestReleaseBin, LatestReleaseInfo);
                    } catch (Exception) {
                        // ignored
                    }
                }
            }
        }

        /// <summary>
        /// Called when the latest release download is done
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="asyncCompletedEventArgs"></param>
        private static void WcOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs) {

            // copy 7zip.exe
            if (!File.Exists(Path7ZipExe))
                File.WriteAllBytes(Path7ZipExe, Resources._7z);
            if (!File.Exists(Path7ZipDll))
                File.WriteAllBytes(Path7ZipDll, Resources._7zdll);

            // Extract the .zip file
            Run(Path7ZipExe, string.Format("x -y \"-o{0}\" \"{1}\"", Path.Combine(Npp.GetConfigDir(), "Update"), PathLatestReleaseZip));

            // check the presence of the plugin file
            if (!File.Exists(PathDownloadedPlugin)) {
                Directory.Delete(PathUpdateFolder, true);
                return;
            }

            // copy the 3pUpdater.exe, which basically copies the downloaded version of the plugin into the /plugins/ dir
            if (!File.Exists(PathUpdaterExe))
                File.WriteAllBytes(PathUpdaterExe, Resources._3pUpdater);

            UserCommunication.Notify(@"Dear user, <br>
                <br>
                a new version of 3P is available on github and will be automatically installed the next time you restart notepad++<br>
                <br>
                Your version : <b>" + AssemblyInfo.Version + @"</b><br>
                Distant version : <b>" + LatestReleaseInfo.Version + @"</b><br>
                Available since : <b>" + LatestReleaseInfo.ReleaseDate + @"</b><br>
                Release URL : <b><a href='" + LatestReleaseInfo.ReleaseUrl + "'>" + LatestReleaseInfo.ReleaseUrl + @"</a></b><br>" + 
                (!Config.Instance.UserGetsPreReleases ? "" : "Is it a pre-release : "  + LatestReleaseInfo.IsPreRelease) + "<br>", MessageImage.Update, "Update check", null, "An update is available");
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
            // an update has been done
            if (File.Exists(PathLatestReleaseBin)) {
                // load release info
                LatestReleaseInfo = BinWriter.ReadFromBinaryFile<ReleaseInfo>(PathLatestReleaseBin);

                if (LatestReleaseInfo != null) {
                    //TODO :dzdzae
                    UserCommunication.Notify(@"Congratulations user,<br>
                    <br>TOOOODOOOO
                    The latest version of 3P has been successfully installed<br>
                    <b>" + AssemblyInfo.Version + @"</b><br>
                    Distant version : <b>" + LatestReleaseInfo.Version + @"</b><br>
                    Available since : <b>" + LatestReleaseInfo.ReleaseDate + @"</b><br>
                    Release URL : <b><a href='" + LatestReleaseInfo.ReleaseUrl + "'>" + LatestReleaseInfo.ReleaseUrl + @"</a></b><br>" +
                                             (!Config.Instance.UserGetsPreReleases ? "" : "Is it a pre-release : " + LatestReleaseInfo.IsPreRelease) + "<br>", MessageImage.Update, "Update check", null, "An update is available");
                }

            }

            // Check for new updates
            Task.Factory.StartNew(GetLatestReleaseInfo);
        }

        private static void Run(string app, string args) {
            var process = Process.Start(new ProcessStartInfo {
                FileName = app,
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false
            });
            if (process != null)
                process.WaitForExit();
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
                    if (PeekAtToken(1).Value.Equals(":") && PeekAtToken(2) is TokenWhiteSpace && PeekAtToken(3) is TokenWord) {
                        var varName = token.Value;
                        if (varName[0] == '"')
                            varName = varName.Substring(1, varName.Length - 2);
                        var varValue = PeekAtToken(3).Value;
                        if (varValue[0] == '"')
                            varValue = varValue.Substring(1, varValue.Length - 2);
                        releaseJsonontent[outerI].Add(new Tuple<string, string>(varName, varValue));
                        _tokenPos = _tokenPos + 3;
                    }
                }
            } while (!(token is TokenEof));
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
