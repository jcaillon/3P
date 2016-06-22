#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (CompilationPath.cs) is part of 3P.
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
using System.Linq;
using System.Text;
using _3PA.Lib;

namespace _3PA.MainFeatures.ProgressExecutionNs {

    internal class CompilationPath {

        #region OBJECT

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

        #endregion


        #region public event

        /// <summary>
        /// Called when the list of CompilationPath is updated
        /// </summary>
        public static event Action OnCompilationPathUpdate;

        #endregion


        #region private fields

        private static List<CompilationPath> _compilationPathList;

        #endregion


        #region public methods

        /// <summary>
        /// Read the list of compilation Path Items,
        /// if the file is present in the Config dir, use it
        /// </summary>
        public static void Import() {
            _compilationPathList = new List<CompilationPath>();
            Utils.ForEachLine(Config.FileCompilPath, new byte[0], Encoding.Default, s => {
                var items = s.Split('\t');
                if (items.Count() == 4) {
                    var obj = new CompilationPath {
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

            if (OnCompilationPathUpdate != null)
                OnCompilationPathUpdate();
        }

        /// <summary>
        /// Get the compilation path list
        /// </summary>
        public static List<CompilationPath> GetCompilationPathList {
            get {
                if (_compilationPathList == null)
                    Import();
                return _compilationPathList;
            }
        }

        /// <summary>
        /// returns a string containing an html representation of the compilation path table
        /// </summary>
        public static string BuildHtmlTable() {
            var strBuilder = new StringBuilder();

            if (GetCompilationPathList.Any()) {

                strBuilder.Append("<table width='100%;'>");
                strBuilder.Append("<tr><td class='CompPathHead' align='center' style='padding-right: 15px; padding-right: 15px;'>Application</td><td class='CompPathHead' align='center' style='padding-right: 15px; padding-right: 15px;'>Suffix</td><td class='CompPathHead' width='40%'>Input path pattern</td><td class='CompPathHead' width='40%'>Append to output path</td></tr>");
                foreach (var compLine in GetCompilationPathList) {
                    strBuilder.Append("<tr><td align='center'>" + (string.IsNullOrEmpty(compLine.ApplicationFilter) ? "*" : compLine.ApplicationFilter) + "</td><td align='center'>" + (string.IsNullOrEmpty(compLine.EnvLetterFilter) ? "*" : compLine.EnvLetterFilter) + "</td><td>" + (compLine.InputPathPattern.Length > 40 ? "..." + compLine.InputPathPattern.Substring(compLine.InputPathPattern.Length - 40) : compLine.InputPathPattern) + "</td><td>" + (compLine.OutputPathAppend.Length > 40 ? "..." + compLine.InputPathPattern.Substring(compLine.OutputPathAppend.Length - 40) : compLine.OutputPathAppend) + "</td></tr>");
                }
                strBuilder.Append("</table>");

            } else {
                strBuilder.Append("<b>Start by clicking the <i>modify</i> button</b><br>When you are done modying the file, save it and click the <i>read changes</i> button to import it into 3P");
            }

            return strBuilder.ToString();
        }

        #endregion

    }
}
