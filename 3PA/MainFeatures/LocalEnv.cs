#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (LocalEnv.cs) is part of 3P.
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
                return ini.GetValue("Trigram", "");
            }
            return "";
        }

    }
}
