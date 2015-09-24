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
