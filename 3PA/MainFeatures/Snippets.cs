#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (Snippets.cs) is part of 3P.
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using _3PA.Data;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;

namespace _3PA.MainFeatures {

    public class SnippetContext {
        public static int IndicatorId = 8;
        public List<List<Point>> ParametersGroups = new List<List<Point>>();
        public List<Point> Parameters = new List<Point>();
        public Point? CurrentParameter;
        public string CurrentParameterValue = "";
        public string ReplacementString = "";
    }

    public class Snippets {

        public static SnippetContext LocSnippetContext;
        static public Dictionary<string, string> Map = new Dictionary<string, string>();
        private const string FileName = "snippets.data";

        static public bool InsertionActive { get { return LocSnippetContext != null; } }

        static public IEnumerable<string> Keys {
            get {
                return Map.Keys;
            }
        }

        public static bool Contains(string snippetTag) {
            lock (Map) {
                return (!string.IsNullOrWhiteSpace(snippetTag)) && Map.ContainsKey(snippetTag);
            }
        }

        public static void Init() {
            lock (Map) {
                if (!File.Exists(ConfigFile))
                    File.WriteAllBytes(ConfigFile, DataResources.Snippets);
                try {
                    Read(ConfigFile);
                } catch (Exception e) {
                    ErrorHandler.ShowErrors(e, "Error while loading snippets!", ConfigFile);
                }
                SetupFileWatcher();
            }
        }

        public static string GetTemplate(string snippetTag) {
            lock (Map) {
                if (Map.ContainsKey(snippetTag))
                    return Map[snippetTag];
                return null;
            }
        }

        static string ConfigFile {
            get {
                return Path.Combine(Npp.GetConfigDir(), FileName);
            }
        }

        static void Read(string file) {
            //Debug.Assert(false);
            Map.Clear();
            var buffer = new StringBuilder();
            var currentTag = "";

            foreach (var line in File.ReadAllLines(file, Encoding.Default)) {
                if (line.StartsWith("#"))
                    continue; //comment line

                if (line.EndsWith("=>") && !line.StartsWith(" ")) {
                    if (currentTag != "") {
                        Map.Add(currentTag, buffer.ToString().Remove(buffer.ToString().LastIndexOf("\r\n", StringComparison.Ordinal)));
                        buffer.Clear();
                    }

                    currentTag = line.Replace("=>", "").Trim();
                } else
                    buffer.AppendLine(line);
            }

            if (currentTag != "")
                Map.Add(currentTag, buffer.ToString().Remove(buffer.ToString().LastIndexOf("\r\n", StringComparison.Ordinal)));
        }

        public static bool TriggerCodeSnippetInsertion() {

            if (InsertionActive) return false; // do no insert a snippet within a snippet!

            
            string token = Npp.GetKeyword(Npp.CurrentPosition);
            var curPos = Npp.CurrentPosition;
            Point tokenPoints = new Point(curPos - token.Length, curPos);

            if (Contains(token)) {

                string replacement = GetTemplate(token);

                if (replacement != null) {
                    int line = Npp.Line.CurrentLine;
                    int lineStartPos = Npp.GetLine(line).Position;

                    int horizontalOffset = tokenPoints.X - lineStartPos;

                    //relative selection in the replacement text
                    PrepareForIncertion(replacement, horizontalOffset, tokenPoints.X);

                    var indic = Npp.GetIndicator(SnippetContext.IndicatorId);
                    indic.Style = IndicatorStyle.Box;
                    indic.ForeColor = Color.Blue;

                    foreach (var point in LocSnippetContext.Parameters) {
                        indic.Add(point.X, point.Y);
                    }

                    if (LocSnippetContext.CurrentParameter.HasValue) {
                        Npp.SetSelection(LocSnippetContext.CurrentParameter.Value.X, LocSnippetContext.CurrentParameter.Value.Y);
                        LocSnippetContext.CurrentParameterValue = Npp.GetTextBetween(LocSnippetContext.CurrentParameter.Value);
                    }

                    AutoComplete.Close();

                    if (LocSnippetContext.Parameters.Count <= 1)
                        FinalizeCurrent();
                }

                return true;
            }
            return false;
        }

        static public void ReplaceTextAtIndicator(string text, Point indicatorRange) {
            Npp.SetTextByRange(indicatorRange.X, indicatorRange.Y, text);

            //restore the indicator
            var indic = Npp.GetIndicator(SnippetContext.IndicatorId);
            indic.Style = IndicatorStyle.Box;
            indic.ForeColor = Color.Blue;
            indic.Add(indicatorRange.X, indicatorRange.X + text.Length);
        }

        static public bool NavigateToNextParam() {
            var indic = Npp.GetIndicator(SnippetContext.IndicatorId);
            indic.Style = IndicatorStyle.Box;
            indic.ForeColor = Color.Blue;

            var indicators = indic.FindRanges().ToArray();

            if (!indicators.Any())
                return false;

            if (LocSnippetContext.CurrentParameter != null) {
                Point currentParam = LocSnippetContext.CurrentParameter.Value;
                string currentParamOriginalText = LocSnippetContext.CurrentParameterValue;

                Npp.SetSelection(currentParam.X, currentParam.X);
                string currentParamDetectedText = Npp.GetWordAtPosition(Npp.CurrentPosition);


                if (currentParamOriginalText != currentParamDetectedText) {
                    //current parameter is modified, indicator is destroyed so restore the indicator first
                    indic.Add(currentParam.X, currentParam.X + currentParamDetectedText.Length);

                    indicators = indic.FindRanges().ToArray(); //needs refreshing as the document is modified

                    var paramsInfo = indicators.Select(p => new {
                        Index = indicators.IndexOf(p),
                        Text = Npp.GetTextBetween(p),
                        Range = p,
                        Pos = p.X
                    })
                        .OrderBy(x => x.Pos)
                        .ToArray();

                    var paramsToUpdate = paramsInfo.Where(item => item.Text == currentParamOriginalText).ToArray();

                    foreach (var param in paramsToUpdate) {
                        ReplaceTextAtIndicator(currentParamDetectedText, indicators[param.Index]);
                        indicators = indic.FindRanges().ToArray();//needs refreshing as the document is modified
                    }
                }

                Point? nextParameter = null;

                int currentParamIndex = indicators.FindIndex(x => x.X >= currentParam.X); //can also be logical 'next'
                var prevParamsValues = indicators.Take(currentParamIndex).Select(p => Npp.GetTextBetween(p)).ToList();
                prevParamsValues.Add(currentParamOriginalText);
                prevParamsValues.Add(currentParamDetectedText);
                prevParamsValues.Add(" ");
                prevParamsValues.Add("|");

                foreach (var range in indicators.ToArray()) {
                    if (currentParam.X < range.X && !prevParamsValues.Contains(Npp.GetTextBetween(range))) {
                        nextParameter = range;
                        break;
                    }
                }

                if (!nextParameter.HasValue)
                    nextParameter = indicators.FirstOrDefault();

                LocSnippetContext.CurrentParameter = nextParameter;
            }
            if (LocSnippetContext.CurrentParameter.HasValue) {
                Npp.SetSelection(LocSnippetContext.CurrentParameter.Value.X, LocSnippetContext.CurrentParameter.Value.Y);
                LocSnippetContext.CurrentParameterValue = Npp.GetTextBetween(LocSnippetContext.CurrentParameter.Value);
            }

            return true;
        }

        static public void FinalizeCurrent() {
            var indic = Npp.GetIndicator(SnippetContext.IndicatorId);
            var indicators = indic.FindRanges().ToArray();

            foreach (var range in indicators)
                indic.Clear(range.X, range.Y);

            var caretPoint = indicators.Where(point => {
                string text = Npp.GetTextBetween(point);
                return text == " " || text == "|";
            })
                .FirstOrDefault();

            if (caretPoint.X != caretPoint.Y) {
                Npp.SetTextByRange(caretPoint.X, caretPoint.Y, "");
                Npp.SetSelection(caretPoint.X, caretPoint.X);
            }

            LocSnippetContext = null;
        }

        public static void PrepareForIncertion(string rawText, int charsOffset, int documentOffset = 0) {
            
            LocSnippetContext = new SnippetContext();

            LocSnippetContext.ReplacementString = rawText;

            string offset = new string(' ', charsOffset);
            LocSnippetContext.ReplacementString = LocSnippetContext.ReplacementString.Replace(Environment.NewLine, Environment.NewLine + offset);

            int endPos;
            int startPos = LocSnippetContext.ReplacementString.IndexOf("$", StringComparison.Ordinal);

            while (startPos != -1) {
                endPos = LocSnippetContext.ReplacementString.IndexOf("$", startPos + 1, StringComparison.Ordinal);

                if (endPos != -1) {
                    //'$item$' -> 'item'
                    int newEndPos = endPos - 2;

                    LocSnippetContext.Parameters.Add(new Point(startPos + documentOffset, newEndPos + 1 + documentOffset));

                    string leftText = LocSnippetContext.ReplacementString.Substring(0, startPos);
                    string rightText = LocSnippetContext.ReplacementString.Substring(endPos + 1);
                    string placementValue = LocSnippetContext.ReplacementString.Substring(startPos + 1, endPos - startPos - 1);

                    LocSnippetContext.ReplacementString = leftText + placementValue + rightText;

                    endPos = newEndPos;
                } else
                    break;

                startPos = LocSnippetContext.ReplacementString.IndexOf("$", endPos + 1, StringComparison.Ordinal);
            }

            Npp.ReplaceKeywordWrapped(LocSnippetContext.ReplacementString, -1);

            if (LocSnippetContext.Parameters.Any())
                LocSnippetContext.CurrentParameter = LocSnippetContext.Parameters.FirstOrDefault();

        }

        static public void EditSnippetsConfig() {
            Npp.OpenFile(ConfigFile);
        }

        static FileSystemWatcher _configWatcher;

        static void SetupFileWatcher() {
            string dir = Path.GetDirectoryName(ConfigFile);
            string fileName = Path.GetFileName(ConfigFile);
            if (dir != null) {
                _configWatcher = new FileSystemWatcher(dir, fileName);
                _configWatcher.NotifyFilter = NotifyFilters.LastWrite;
                _configWatcher.Changed += configWatcher_Changed;
                _configWatcher.EnableRaisingEvents = true;
            }
        }

        static void configWatcher_Changed(object sender, FileSystemEventArgs e) {
            Init();
        }
    }
}