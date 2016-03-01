#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (LibLoader.cs) is part of 3P.
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
using System.Reflection;
using _3PA.MainFeatures;
using _3PA.Properties;

namespace _3PA.Lib {
    internal static class LibLoader {

        private static string _pathToYamui;
        private static string _pathToOlv;

        /// <summary>
        /// Should be called when the dll loads so it can call its dependencies freely
        /// </summary>
        public static void Init() {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            _pathToYamui = Path.Combine(Config.FolderLibrary, @"YamuiFramework.dll");
            _pathToOlv = Path.Combine(Config.FolderLibrary, @"ObjectListView.dll");

            // delete existing libs so we are sure to use up to date libs
            Utils.DeleteFile(_pathToYamui);
            Utils.DeleteFile(_pathToOlv);
        }

        /// <summary>
        /// Called when the resolution of an assembly fails, gives us the opportunity to feed the required asssembly
        /// to the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            try {
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
