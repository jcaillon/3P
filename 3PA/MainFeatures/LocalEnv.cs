using System;
using System.IO;
using Microsoft.Win32;
using _3PA.Lib;

namespace _3PA.MainFeatures {
    public class LocalEnv {

        public static LocalEnv Instance { get { return _instance ?? (_instance = new LocalEnv()); } }
        private static LocalEnv _instance;

        public string LastUsedEnvironment;
        public string Env;
        public string Proj;

        LocalEnv() {
            LastUsedEnvironment = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\ProgressAssist", "LastUsedEnvironment", "");
            if (!String.IsNullOrWhiteSpace(LastUsedEnvironment)) {
                Proj = LastUsedEnvironment.Substring(LastUsedEnvironment.Length - 1, 1);
                Env = LastUsedEnvironment.Substring(0, LastUsedEnvironment.Length - 1);
            }
        }

        public string GetTrigramFromPa() {
            // default values
            string paIniPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ProgressAssist", "pa.ini");
            if (File.Exists(paIniPath)) {
                IniReader ini = new IniReader(paIniPath);
                return ini.GetValue("Trigram");
            }
            return "";
        }

    }
}
