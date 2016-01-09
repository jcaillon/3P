#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (ConfLoader.cs) is part of 3P.
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
using System.IO;
using System.Text;
using _3PA.MainFeatures;

namespace _3PA.Lib {
    /// <summary>
    /// This class allows to easily import/export into .conf files
    /// </summary>
    public static class ConfLoader {

        /// <summary>
        /// Reads all the line of either the filePath (if the file exists) or from byte array dataResources,
        /// Apply the action toApplyOnEachLine to each line
        /// Uses encoding as the Encoding to read the file or convert the byte array to a string
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="dataResources"></param>
        /// <param name="toApplyOnEachLine"></param>
        /// <param name="encoding"></param>
        public static void ForEachLine(string filePath, byte[] dataResources, Encoding encoding, Action<string> toApplyOnEachLine) {
            // TODO: check file size
            try {
                using (StringReader reader = new StringReader((!string.IsNullOrEmpty(filePath) && File.Exists(filePath)) ? File.ReadAllText(filePath, encoding) : encoding.GetString(dataResources))) {
                    string line;
                    while ((line = reader.ReadLine()) != null) {
                        toApplyOnEachLine(line);
                    }
                }
            } catch (Exception e) {
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    ErrorHandler.ShowErrors(e, "Error reading file", filePath);
                else
                    ErrorHandler.ShowErrors(e, "Error data resource for " + filePath);
            }
        }
    }
}
