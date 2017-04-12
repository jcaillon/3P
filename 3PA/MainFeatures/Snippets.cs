#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Linq;
using System.Text;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletionFeature;
using _3PA.NppCore;

namespace _3PA.MainFeatures {
    internal class SnippetContext {
        public static int IndicatorId = 8;
        public List<List<Point>> ParametersGroups = new List<List<Point>>();
        public List<Point> Parameters = new List<Point>();
        public Point? CurrentParameter;
        public string CurrentParameterValue = "";
        public string ReplacementString = "";
    }

    internal static class Snippets {
        public static SnippetContext LocSnippetContext;
        static public Dictionary<string, string> Map = new Dictionary<string, string>();

        static public bool InsertionActive {
            get { return LocSnippetContext != null; }
        }

        static public IEnumerable<string> Keys {
            get { return Map.Keys; }
        }

        public static bool Contains(string snippetTag) {
            lock (Map) {
                return (!string.IsNullOrWhiteSpace(snippetTag)) && Map.ContainsKey(snippetTag);
            }
        }

        public static void Init() {}

        public static string GetTemplate(string snippetTag) {
            lock (Map) {
                if (Map.ContainsKey(snippetTag))
                    return Map[snippetTag];
                return null;
            }
        }

        static void Read(string file) {
            //Debug.Assert(false);
            Map.Clear();
            var buffer = new StringBuilder();
            var currentTag = "";

            Utils.ForEachLine(file, null, (i, line) => {
                    if (line.EndsWith("=>") && !line.StartsWith(" ")) {
                        if (currentTag != "") {
                            Map.Add(currentTag, buffer.ToString().Remove(buffer.ToString().LastIndexOf("\r\n", StringComparison.Ordinal)));
                            buffer.Clear();
                        }

                        currentTag = line.Replace("=>", "").Trim();
                    } else
                        buffer.AppendLine(line);
                },
                Encoding.Default);

            if (currentTag != "")
                Map.Add(currentTag, buffer.ToString().Remove(buffer.ToString().LastIndexOf("\r\n", StringComparison.Ordinal)));
        }

        public static bool TriggerCodeSnippetInsertion() {
            if (InsertionActive) return false; // do no insert a snippet within a snippet!

            string token = Sci.GetWord(null, Sci.CurrentPosition);
            var curPos = Sci.CurrentPosition;
            Point tokenPoints = new Point(curPos - token.Length, curPos);

            if (Contains(token)) {
                string replacement = GetTemplate(token);

                if (replacement != null) {
                    int line = Sci.Line.CurrentLine;
                    int lineStartPos = Sci.GetLine(line).Position;

                    int horizontalOffset = tokenPoints.X - lineStartPos;

                    //relative selection in the replacement text
                    PrepareForIncertion(replacement, horizontalOffset, tokenPoints.X);

                    var indic = Sci.GetIndicator(SnippetContext.IndicatorId);
                    indic.IndicatorStyle = IndicatorStyle.Box;
                    indic.ForeColor = Color.Blue;

                    foreach (var point in LocSnippetContext.Parameters) {
                        indic.Add(point.X, point.Y);
                    }

                    if (LocSnippetContext.CurrentParameter.HasValue) {
                        Sci.SetSelection(LocSnippetContext.CurrentParameter.Value.X, LocSnippetContext.CurrentParameter.Value.Y);
                        LocSnippetContext.CurrentParameterValue = Sci.GetTextBetween(LocSnippetContext.CurrentParameter.Value);
                    }

                    AutoCompletion.Cloak();

                    if (LocSnippetContext.Parameters.Count <= 1)
                        FinalizeCurrent();
                }

                return true;
            }
            return false;
        }

        static public void ReplaceTextAtIndicator(string text, Point indicatorRange) {
            Sci.SetTextByRange(indicatorRange.X, indicatorRange.Y, text);

            //restore the indicator
            var indic = Sci.GetIndicator(SnippetContext.IndicatorId);
            indic.IndicatorStyle = IndicatorStyle.Box;
            indic.ForeColor = Color.Blue;
            indic.Add(indicatorRange.X, indicatorRange.X + text.Length);
        }

        static public bool NavigateToNextParam() {
            var indic = Sci.GetIndicator(SnippetContext.IndicatorId);
            indic.IndicatorStyle = IndicatorStyle.Box;
            indic.ForeColor = Color.Blue;

            var indicators = indic.FindRanges().ToArray();

            if (!indicators.Any())
                return false;

            if (LocSnippetContext.CurrentParameter != null) {
                Point currentParam = LocSnippetContext.CurrentParameter.Value;
                string currentParamOriginalText = LocSnippetContext.CurrentParameterValue;

                Sci.SetSelection(currentParam.X, currentParam.X);
                string currentParamDetectedText = Sci.GetWordAtPosition(Sci.CurrentPosition, AutoCompletion.CurrentLangAdditionalChars);

                if (currentParamOriginalText != currentParamDetectedText) {
                    //current parameter is modified, indicator is destroyed so restore the indicator first
                    indic.Add(currentParam.X, currentParam.X + currentParamDetectedText.Length);

                    indicators = indic.FindRanges().ToArray(); //needs refreshing as the document is modified

                    var paramsInfo = indicators.Select(p => new {
                            Index = indicators.IndexOf(p),
                            Text = Sci.GetTextBetween(p),
                            Range = p,
                            Pos = p.X
                        })
                        .OrderBy(x => x.Pos)
                        .ToArray();

                    var paramsToUpdate = paramsInfo.Where(item => item.Text == currentParamOriginalText).ToArray();

                    foreach (var param in paramsToUpdate) {
                        ReplaceTextAtIndicator(currentParamDetectedText, indicators[param.Index]);
                        indicators = indic.FindRanges().ToArray(); //needs refreshing as the document is modified
                    }
                }

                Point? nextParameter = null;

                int currentParamIndex = indicators.FindIndex(x => x.X >= currentParam.X); //can also be logical 'next'
                var prevParamsValues = indicators.Take(currentParamIndex).Select(p => Sci.GetTextBetween(p)).ToList();
                prevParamsValues.Add(currentParamOriginalText);
                prevParamsValues.Add(currentParamDetectedText);
                prevParamsValues.Add(" ");
                prevParamsValues.Add("|");

                foreach (var range in indicators.ToArray()) {
                    if (currentParam.X < range.X && !prevParamsValues.Contains(Sci.GetTextBetween(range))) {
                        nextParameter = range;
                        break;
                    }
                }

                if (!nextParameter.HasValue)
                    nextParameter = indicators.FirstOrDefault();

                LocSnippetContext.CurrentParameter = nextParameter;
            }
            if (LocSnippetContext.CurrentParameter.HasValue) {
                Sci.SetSelection(LocSnippetContext.CurrentParameter.Value.X, LocSnippetContext.CurrentParameter.Value.Y);
                LocSnippetContext.CurrentParameterValue = Sci.GetTextBetween(LocSnippetContext.CurrentParameter.Value);
            }

            return true;
        }

        static public void FinalizeCurrent() {
            var indic = Sci.GetIndicator(SnippetContext.IndicatorId);
            var indicators = indic.FindRanges().ToArray();

            foreach (var range in indicators)
                indic.Clear(range.X, range.Y);

            var caretPoint = indicators.Where(point => {
                    string text = Sci.GetTextBetween(point);
                    return text == " " || text == "|";
                })
                .FirstOrDefault();

            if (caretPoint.X != caretPoint.Y) {
                Sci.SetTextByRange(caretPoint.X, caretPoint.Y, "");
                Sci.SetSelection(caretPoint.X, caretPoint.X);
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

            Sci.ReplaceWordWrapped(LocSnippetContext.ReplacementString, null, -1);

            if (LocSnippetContext.Parameters.Any())
                LocSnippetContext.CurrentParameter = LocSnippetContext.Parameters.FirstOrDefault();
        }

        static public void EditSnippetsConfig() {
            Npp.OpenFile(Config.FileSnippets);
        }
    }
}