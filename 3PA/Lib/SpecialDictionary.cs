#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (SpecialDictionary.cs) is part of 3P.
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
using System.Web.Script.Serialization;

namespace _3PA.Lib {

    public class SpecialDictionary<T> : Dictionary<string, T> {

        public SpecialDictionary(StringComparer t) : base(t) { }

        public SpecialDictionary() { }

        public void Load(string configFile) {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            foreach (var line in File.ReadAllLines(configFile)) {
                var index = line.IndexOf("\t", StringComparison.CurrentCultureIgnoreCase);
                var key = line.Substring(0, index).Trim();
                if (!ContainsKey(key))
                    Add(line.Substring(0, index).Trim(), serializer.Deserialize<T>(line.Substring(index + 1)));
            }
        }

        public void Save(string configFile) {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            TextWriter tw = new StreamWriter(configFile);
            foreach (var entry in this) {
                tw.WriteLine(entry.Key + "\t" + serializer.Serialize(entry.Value));
            }
            tw.Close();
        }
    }
}
