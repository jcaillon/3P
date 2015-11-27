using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using _3PA.MainFeatures.Parser;

namespace _3PA.Lib {
    public class UpdateHandler {

        public static ReleaseInfo GetLatestReleaseInfo() {

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

                // Download release list from GITHUB API Config.ReleasesUrl
                //var json = wc.DownloadString(@"https://api.github.com/repos/jcaillon/battle-code/releases");

                // Parse the .json
                var parser = new JsonParser(File.ReadAllText(@"C:\Users\Julien\Desktop\releases.json"));
                parser.Tokenize();

                // For each version higher than the local one, append to the release body
                // Will be used to display the version log to the user
                var localVersion = AssemblyInfo.Version;
                var outputBody = new StringBuilder();
                foreach (var release in parser.GetList()) {
                    var releaseVersion = release.Find(tuple => tuple.Item1.Equals("tag_name"));
                    if (releaseVersion != null && releaseVersion.Item1.IsHigherVersionThan(localVersion)) {
                        outputBody.AppendLine("**" + releaseVersion.Item1 + "**\n");
                        var locBody = release.Find(tuple => tuple.Item1.Equals("body"));
                        if (locBody != null)
                            outputBody.AppendLine(locBody.Item2);
                    }
                }
                
                


                /*
                // Hookup DownloadFileCompleted Event
                wc.DownloadFileCompleted +=    new AsyncCompletedEventHandler(client_DownloadFileCompleted);

                // Start the download and copy the file to c:\temp
                wc.DownloadFileAsync(new Uri(url), @"c:\temp\image35.png");
                 * */
            }

            File.WriteAllText(@"C:\Users\Julien\Desktop\tt.p", AssemblyInfo.Version);

            return null;
        }
    }

    

    public class ReleaseInfo {
        public string Version { private set; get; }
        public string Name { private set; get; }
        public bool IsPreRelease { private set; get; }
        public bool IsDraft { private set; get; }
        public string AssetUrl { private set; get; }
        public string Body { private set; get; }

        public ReleaseInfo(string version, string name, bool isPreRelease, string assetUrl, string body, bool isDraft) {
            Version = version;
            Name = name;
            IsPreRelease = isPreRelease;
            AssetUrl = assetUrl;
            Body = body;
            IsDraft = isDraft;
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
                        releaseJsonontent[outerI].Add(new Tuple<string, string>(varName, PeekAtToken(3).Value));
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
