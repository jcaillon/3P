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
using System.Reflection;
using YamuiFramework.Themes;
using _3PA.MainFeatures;
using _3PA._Resource;

namespace _3PA.Lib {
    internal static class LibLoader {
        /// <summary>
        /// Called when the resolution of an assembly fails, gives us the opportunity to feed the required asssembly
        /// to the program
        /// Subscribe to the following event on start :
        /// AppDomain.CurrentDomain.AssemblyResolve += LibLoader.AssemblyResolver;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Assembly AssemblyResolver(object sender, ResolveEventArgs args) {
            try {
                var assName = args.Name.Substring(0, args.Name.IndexOf(",", StringComparison.CurrentCultureIgnoreCase));
                switch (assName) {
                    case "YamuiFramework":
                        return Assembly.Load(DependenciesResources.YamuiFramework);
                    default:
                        return null;
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