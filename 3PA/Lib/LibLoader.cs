#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
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
using Yamui.Framework.Themes;
using _3PA.MainFeatures;

namespace _3PA.Lib {
    internal static class LibLoader {

        /// <summary>
        /// Called when the resolution of an assembly fails, gives us the opportunity to feed the required asssembly
        /// to the program
        /// Subscribe to the following event on start :
        /// AppDomain.CurrentDomain.AssemblyResolve += LibLoader.AssemblyResolver;
        /// </summary>
        public static Assembly AssemblyResolver(object sender, ResolveEventArgs args) {
            // see code https://msdn.microsoft.com/en-us/library/d4tc2453(v=vs.110).aspx
            try {
                var commaIdx = args.Name.IndexOf(",", StringComparison.CurrentCultureIgnoreCase);
                if (commaIdx > 0) {
                    var assName = args.Name.Substring(0, commaIdx);
                    var dllSourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{nameof(_3PA)}.{assName}.dll");
                    var pdbSourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{nameof(_3PA)}.{assName}.pdb");
                    if (dllSourceStream == null)
                        return null;
                    using (var dllMemoryStream = new MemoryStream()) {
                        dllSourceStream.CopyTo(dllMemoryStream);
                        if (pdbSourceStream == null) {
                            return Assembly.Load(dllMemoryStream.ToArray());
                        } else {
                            using (var pdbMemoryStream = new MemoryStream()) {
                                pdbSourceStream.CopyTo(pdbMemoryStream);
                                return Assembly.Load(dllMemoryStream.ToArray(), pdbMemoryStream.ToArray());
                            }
                        }
                    }
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in LibLoader");
            }
            return null;
        }

        public static string GetYamuiAssemblyVersion() {
            var yamuiAssembly = Assembly.GetAssembly(typeof(YamuiTheme));
            var v = yamuiAssembly.GetName().Version.ToString();
            return "v" + v;
        }
    }
}