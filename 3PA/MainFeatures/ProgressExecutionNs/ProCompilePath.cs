#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProCompilePath.cs) is part of 3P.
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using _3PA.Lib;

namespace _3PA.MainFeatures.ProgressExecutionNs {

    internal static class ProCompilePath {

        #region fields

        private static List<CompilationPathItem> _compilationPathList = new List<CompilationPathItem>();

        #endregion

        #region Read and Export

        /// <summary>
        /// Read the list of compilation Path Items,
        /// if the file is present in the Config dir, use it
        /// </summary>
        public static void Import() {
            _compilationPathList.Clear();
            ConfLoader.ForEachLine(Config.FileCompilPath, new byte[0], Encoding.Default, s => {
                var items = s.Split('\t');
                if (items.Count() == 4) {
                    var obj = new CompilationPathItem {
                        ApplicationFilter = items[0].Trim(),
                        EnvLetterFilter = items[1].Trim(),
                        InputPathPattern = items[2].Trim().Replace('/', '\\'),
                        OutputPathAppend = items[3].Trim().Replace('/', '\\')
                    };
                    if (!string.IsNullOrEmpty(obj.InputPathPattern) && !string.IsNullOrEmpty(obj.OutputPathAppend)) {
                        if (obj.ApplicationFilter.Equals("*"))
                            obj.ApplicationFilter = "";
                        if (obj.EnvLetterFilter.Equals("*"))
                            obj.EnvLetterFilter = "";
                        if (!_compilationPathList.Exists(item => 
                            item.ApplicationFilter.Equals(obj.ApplicationFilter) &&
                            item.EnvLetterFilter.Equals(obj.EnvLetterFilter) &&
                            item.InputPathPattern.Equals(obj.InputPathPattern)))
                        _compilationPathList.Add(obj);
                    }
                }
            });
        }

        #endregion

        #region public methods

        /// <summary>
        /// returns a string containing an html representation of the compilation path table
        /// </summary>
        public static string BuildHtmlTable() {
            var strBuilder = new StringBuilder();

            if (_compilationPathList.Any()) {

                strBuilder.Append("<table width='100%;'>");
                strBuilder.Append("<tr><td class='CompPathHead' align='center' style='padding-right: 15px; padding-right: 15px;'>Application</td><td class='CompPathHead' align='center' style='padding-right: 15px; padding-right: 15px;'>Suffix</td><td class='CompPathHead' width='40%'>Input path pattern</td><td class='CompPathHead' width='40%'>Append to output path</td></tr>");
                foreach (var compLine in _compilationPathList) {
                    strBuilder.Append("<tr><td align='center'>" + (string.IsNullOrEmpty(compLine.ApplicationFilter) ? "*" : compLine.ApplicationFilter) + "</td><td align='center'>" + (string.IsNullOrEmpty(compLine.EnvLetterFilter) ? "*" : compLine.EnvLetterFilter) + "</td><td>" + (compLine.InputPathPattern.Length > 40 ? "..." + compLine.InputPathPattern.Substring(compLine.InputPathPattern.Length - 40) : compLine.InputPathPattern) + "</td><td>" + (compLine.OutputPathAppend.Length > 40 ? "..." + compLine.InputPathPattern.Substring(compLine.OutputPathAppend.Length - 40) : compLine.OutputPathAppend) + "</td></tr>");
                }
                strBuilder.Append("</table>");

            } else {
                strBuilder.Append("<b>Start by clicking the <i>modify</i> button</b><br>When you are done modying the file, save it and click the <i>read changes</i> button to import it into 3P");
            }

            return strBuilder.ToString();
        }

        /// <summary>
        /// This method returns the correct compilation directory for the given source path,
        /// returns null if invalid sourcePath
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <returns></returns>
        public static string GetCompilationDirectory(string sourcePath) {
            if (string.IsNullOrEmpty(sourcePath))
                return null;

            var baseComp = ProEnvironment.Current.BaseCompilationPath;

            // filter and sort the list
            var filteredList = _compilationPathList.Where(FilterPredicate).ToList();
            filteredList.Sort(SortComparison);

            // try to find the first item that match the input pattern
            if (filteredList.Count > 0) {
                var canFind = filteredList.FirstOrDefault(item => sourcePath.Contains(item.InputPathPattern));
                if (canFind != null) {
                    if (Path.IsPathRooted(canFind.OutputPathAppend)) {
                        baseComp = canFind.OutputPathAppend;
                    } else {
                        baseComp = Path.Combine(baseComp, canFind.OutputPathAppend);
                    }
                }
            }
            return baseComp;
        }

        private static int SortComparison(CompilationPathItem compilationPathItem, CompilationPathItem pathItem) {
            int compare = string.IsNullOrWhiteSpace(compilationPathItem.ApplicationFilter).CompareTo(string.IsNullOrWhiteSpace(pathItem.ApplicationFilter));
            if (compare != 0) return compare;

            compare = string.IsNullOrWhiteSpace(compilationPathItem.EnvLetterFilter).CompareTo(string.IsNullOrWhiteSpace(pathItem.EnvLetterFilter));
            return compare;
        }

        private static bool FilterPredicate(CompilationPathItem compilationPathItem) {
            // returns true if (appli is "" or (appli is currentAppli and (envletter is currentEnvletter or envletter = "")))
            return string.IsNullOrWhiteSpace(compilationPathItem.ApplicationFilter) || (compilationPathItem.ApplicationFilter.EqualsCi(Config.Instance.EnvName) && (compilationPathItem.EnvLetterFilter.EqualsCi(Config.Instance.EnvSuffix) || string.IsNullOrWhiteSpace(compilationPathItem.EnvLetterFilter)));
        }

        #endregion
    }

    /// <summary>
    /// The compilation path item
    /// </summary>
    internal class CompilationPathItem {
        /// <summary>
        /// This compilation path applies to a given application (can be empty)
        /// </summary>
        public string ApplicationFilter { get; set; }
        /// <summary>
        /// This compilation path applies to a given Env letter (can be empty)
        /// </summary>
        public string EnvLetterFilter { get; set; }
        /// <summary>
        /// Pattern to match in the source path
        /// </summary>
        public string InputPathPattern { get; set; }
        /// <summary>
        /// String to append to the compilation directory if the match is true
        /// </summary>
        public string OutputPathAppend { get; set; }
    }
}
