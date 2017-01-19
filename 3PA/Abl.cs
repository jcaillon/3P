#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (Abl.cs) is part of 3P.
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
using _3PA.Lib;
using _3PA.MainFeatures;

namespace _3PA {

    internal static class Abl {

        /// <summary>
        /// is the char allowed in a variable's name?
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsCharAllowedInVariables(char c) {
            return char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '&';
        }

        /// <summary>
        /// Is the current file (in npp) a progress file? (allowed extensions defined in Config)
        /// </summary>
        public static bool IsCurrentProgressFile {
            get { return Npp.GetCurrentFilePath().TestAgainstListOfPatterns(Config.Instance.ProgressFilesPattern); }
        }

        /// <summary>
        /// Is the current file compilable in progress
        /// </summary>
        public static bool IsCurrentFileCompilable {
            get { return IsFileCompilable(Npp.GetCurrentFilePath()); }
        }

        /// <summary>
        /// Returns true if the file is compilable in progress
        /// </summary>
        public static bool IsFileCompilable(string fileName) {
            return Npp.GetCurrentFilePath().TestAgainstListOfPatterns(Config.Instance.CompilableFilesPattern);
        }

        /// <summary>
        /// Reads a a word, either starting from the end (readRightToLeft = true) of the start of the input string 
        /// stopAtPoint or not, if not, output the nbPoints found
        /// </summary>
        /// <param name="input"></param>
        /// <param name="stopAtPoint"></param>
        /// <param name="nbPoints"></param>
        /// <param name="readRightToLeft"></param>
        /// <returns></returns>
        public static string ReadAblWord(string input, bool stopAtPoint, out int nbPoints, bool readRightToLeft = true) {
            nbPoints = 0;
            var max = input.Length - 1;
            int count = 0;
            while (count <= max) {
                var pos = readRightToLeft ? max - count : count;
                var ch = input[pos];
                // normal word
                if (IsCharAllowedInVariables(ch))
                    count++;
                else if (ch == '.' && !stopAtPoint) {
                    count++;
                    nbPoints++;
                } else break;
            }
            return count == 0 ? string.Empty : input.Substring(readRightToLeft ? input.Length - count : 0, count);
        }

        /// <summary>
        /// Overload,
        /// Reads a a word, either starting from the end (readRightToLeft = true) of the start of the input string 
        /// stopAtPoint or not
        /// </summary>
        /// <param name="input"></param>
        /// <param name="stopAtPoint"></param>
        /// <param name="readRightToLeft"></param>
        /// <returns></returns>
        public static string ReadAblWord(string input, bool stopAtPoint, bool readRightToLeft = true) {
            int nb;
            return ReadAblWord(input, stopAtPoint, out nb, readRightToLeft);
        }

        /// <summary>
        /// Returns true if the document starts with & ANALYZE-SUSPEND _VERSION-NUMBER
        /// which indicates that it will be opened as a structured proc in the appbuilder
        /// </summary>
        /// <returns></returns>
        public static bool IsCurrentFileFromAppBuilder {
            get {
                if (!Npp.GetLine(0).Text.Trim().StartsWith("&ANALYZE-SUSPEND _VERSION-NUMBER", StringComparison.CurrentCultureIgnoreCase))
                    return false;
                return true;
            }
        }

    }
}
