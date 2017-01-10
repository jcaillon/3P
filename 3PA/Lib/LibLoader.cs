#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using _3PA.MainFeatures;
using _3PA.Properties;

namespace _3PA.Lib {

    internal static class LibLoader {

        /// <summary>
        /// Called when the resolution of an assembly fails, gives us the opportunity to feed the required asssembly
        /// to the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Assembly AssemblyResolver(object sender, ResolveEventArgs args) {
            try {
                var requestedAssembly = new AssemblyName(args.Name);

                // new library request!
                if (!requestedAssembly.Name.Contains(".")) {

                    var pathToLib = Path.Combine(Config.FolderLibrary, requestedAssembly.Name + ".dll");

                    // replace the library if outdated or if it doesn't exist
                    if (string.IsNullOrEmpty(pathToLib) || !File.Exists(pathToLib) || requestedAssembly.Version.ToString().IsHigherVersionThan(GetAssemblyVersionFromPath(pathToLib))) {
                        var lib = (byte[])Resources.ResourceManager.GetObject(requestedAssembly.Name);
                        if (lib != null) {
                            if (Npp.NumberOfNppStarted <= 1)
                                Utils.FileWriteAllBytes(pathToLib, lib);
                        } else {
                            // the library doesn't exist in 3P!
                            return null;
                        }
                    }

                    return Assembly.LoadFrom(pathToLib);
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in LibLoader");
            }
            return null;
        }

        /// <summary>
        /// Get the version of the given library, if it fails it returns an empty string
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetAssemblyVersion(string name) {
            var path = Path.Combine(Config.FolderLibrary, name + ".dll");
            return (!string.IsNullOrEmpty(path) && File.Exists(path)) ? GetAssemblyVersionFromPath(path) : string.Empty;
        }

        private static string GetAssemblyVersionFromPath(string path) {
            return FileVersionInfo.GetVersionInfo(path).FileVersion;
        }

    }
}
