#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (LibLoader.cs) is part of 3P.
// 
// // 3P is a free software: you can redistribute it and/or modify
// // it under the terms of the GNU General Public License as published by
// // the Free Software Foundation, either version 3 of the License, or
// // (at your option) any later version.
// 
// // 3P is distributed in the hope that it will be useful,
// // but WITHOUT ANY WARRANTY; without even the implied warranty of
// // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// // GNU General Public License for more details.
// 
// // You should have received a copy of the GNU General Public License
// // along with 3P. If not, see <http://www.gnu.org/licenses/>.
// // ========================================================================
#endregion
using System;
using System.IO;
using System.Reflection;
using _3PA.Properties;

namespace _3PA.Lib {
    class LibLoader {

        private static string _rootDir;
        private static string _pathToYamui;
        private static string _pathToOlv;

        /// <summary>
        /// Should be called when the dll loads so it can call its dependencies freely
        /// </summary>
        public static void Init() {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            //var assPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _rootDir = Path.Combine(Npp.GetConfigDir(), "Librairies");
            _pathToYamui = Path.Combine(_rootDir, @"YamuiFramework.dll");
            _pathToOlv = Path.Combine(_rootDir, @"ObjectListView.dll");  
          
            // TODO: only for debug! need to handle this when we do an update tho :/ we want to replace old files
            if (true) {
                try {
                    File.Delete(_pathToYamui);
                    File.Delete(_pathToOlv);
                } catch (Exception) {
                    // ignored
                }
            }
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            try {
                if (!Directory.Exists(_rootDir))
                    Directory.CreateDirectory(_rootDir);

                if (args.Name.StartsWith("YamuiFramework,")) {
                    if (!File.Exists(_pathToYamui))
                        File.WriteAllBytes(_pathToYamui, Resources.YamuiFramework);
                    return Assembly.LoadFrom(_pathToYamui);
                }

                if (args.Name.StartsWith("ObjectListView,")) {
                    if (!File.Exists(_pathToOlv))
                        File.WriteAllBytes(_pathToOlv, Resources.ObjectListView);
                    return Assembly.LoadFrom(_pathToOlv);
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in LibLoader");
            }
            return null;
        }
    }
}
