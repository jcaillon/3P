#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (IniReader.cs) is part of 3P.
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
using System.IO;
using System.Linq;

namespace _3PA.Lib {

    /// <summary>
    /// This classes reads a .ini file
    /// TODO: make it able to write the .ini as well?
    /// </summary>
    public class IniReader {

        /// <summary>
        /// Dictonnary of sections, each containing a dictionnary of key/value
        /// </summary>
        Dictionary<string, Dictionary<string, string>> _ini = new Dictionary<string, Dictionary<string, string>>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Constructor, call it with the .ini to parse
        /// </summary>
        /// <param name="file"></param>
        public IniReader(string file) {
            var currentSection = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            if (!File.Exists(file)) return;
            _ini[""] = currentSection;
            Utils.ForEachLine(file, null, line => {
                line = line.Trim();
                if (line.StartsWith(";"))
                    return;
                if (line.StartsWith("[") && line.EndsWith("]")) {
                    currentSection = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                    _ini[line.Substring(1, line.LastIndexOf("]", StringComparison.CurrentCultureIgnoreCase) - 1)] = currentSection;
                    return;
                }
                var idx = line.IndexOf("=", StringComparison.CurrentCultureIgnoreCase);
                if (idx == -1)
                    currentSection[line] = "";
                else
                    currentSection[line.Substring(0, idx)] = line.Substring(idx + 1);
            });
        }

        /// <summary>
        /// Returns the value of given key, independently of the section 
        /// (returns the first occurence found)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public string GetValue(string key, string @default) {
            foreach (var section in _ini.Where(section => section.Value.ContainsKey(key))) {
                return section.Value[key];
            }
            return @default;
        }

        /// <summary>
        /// Returns the value of the given key, search in the given section
        /// </summary>
        /// <param name="key"></param>
        /// <param name="section"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public string GetValue(string key, string section, string @default) {
            if (!_ini.ContainsKey(section))
                return @default;

            if (!_ini[section].ContainsKey(key))
                return @default;

            return _ini[section][key];
        }
    }
}
