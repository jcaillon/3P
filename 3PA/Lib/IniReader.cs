using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _3PA.Lib {
    class IniReader {
        Dictionary<string, Dictionary<string, string>> _ini = new Dictionary<string, Dictionary<string, string>>(StringComparer.InvariantCultureIgnoreCase);

        public IniReader(string file) {
            var txt = File.ReadAllText(file);

            Dictionary<string, string> currentSection = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            _ini[""] = currentSection;

            foreach (var line in txt.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                               .Select(t => t.Trim())) {
                if (line.StartsWith(";"))
                    continue;

                if (line.StartsWith("[") && line.EndsWith("]")) {
                    currentSection = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                    _ini[line.Substring(1, line.LastIndexOf("]", StringComparison.Ordinal) - 1)] = currentSection;
                    continue;
                }

                var idx = line.IndexOf("=", StringComparison.Ordinal);
                if (idx == -1)
                    currentSection[line] = "";
                else
                    currentSection[line.Substring(0, idx)] = line.Substring(idx + 1);
            }
        }

        public string GetValue(string key) {
            return GetValue(key, "", "");
        }

        public string GetValue(string key, string section) {
            return GetValue(key, section, "");
        }

        public string GetValue(string key, string section, string @default) {
            if (!_ini.ContainsKey(section))
                return @default;

            if (!_ini[section].ContainsKey(key))
                return @default;

            return _ini[section][key];
        }

        public string[] GetKeys(string section) {
            if (!_ini.ContainsKey(section))
                return new string[0];

            return _ini[section].Keys.ToArray();
        }

        public string[] GetSections() {
            return _ini.Keys.Where(t => t != "").ToArray();
        }
    }
}
