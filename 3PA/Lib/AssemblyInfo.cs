#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (AssemblyInfo.cs) is part of 3P.
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

namespace _3PA.Lib {
    internal static class AssemblyInfo {
        private static Assembly _assembly = Assembly.GetExecutingAssembly();

        /// <summary>
        /// Gets the title property
        /// </summary>
        public static string AssemblyProduct {
            get {
                return GetAttributeValue<AssemblyProductAttribute>(a => a.Product,
                    Path.GetFileNameWithoutExtension(_assembly.CodeBase));
            }
        }

        /// <summary>
        /// Gets the title property
        /// </summary>
        public static string AssemblyTitle {
            get {
                return GetAttributeValue<AssemblyTitleAttribute>(a => a.Title,
                    Path.GetFileNameWithoutExtension(_assembly.CodeBase));
            }
        }

        /// <summary>
        /// Gets the application's version
        /// </summary>
        public static string Version {
            get {
                var v = _assembly.GetName().Version.ToString();
                return "v" + v.Substring(0, v.LastIndexOf(".", StringComparison.CurrentCultureIgnoreCase));
            }
        }

        /// <summary>
        /// Last digit of the assembly allows to tell if the soft is in a pre-release build (1) or
        /// stable build (0)
        /// returns true if the soft is in a pre-release build
        /// </summary>
        public static bool IsPreRelease {
            get { return _assembly.GetName().Version.ToString().EndsWith(".1"); }
        }

        /// <summary>
        /// Gets the description about the application.
        /// </summary>
        public static string Description {
            get { return GetAttributeValue<AssemblyDescriptionAttribute>(a => a.Description); }
        }

        /// <summary>
        /// Gets the copyright information for the product.
        /// </summary>
        public static string Copyright {
            get { return GetAttributeValue<AssemblyCopyrightAttribute>(a => a.Copyright); }
        }

        /// <summary>
        /// Gets the company information for the product.
        /// </summary>
        public static string Company {
            get { return GetAttributeValue<AssemblyCompanyAttribute>(a => a.Company); }
        }

        /// <summary>
        /// Returns the path of the executing assembly
        /// </summary>
        public static string Location {
            get { return _assembly.Location; }
        }

        /// <summary>
        /// Returns the name of the executing assembly
        /// </summary>
        public static string AssemblyName {
            get { return Path.GetFileName(_assembly.Location); }
        }

        public static string GetAttributeValue<TAttr>(Func<TAttr,
            string> resolveFunc, string defaultResult = null) where TAttr : Attribute {
            object[] attributes = _assembly.GetCustomAttributes(typeof(TAttr), false);
            if (attributes.Length > 0)
                return resolveFunc((TAttr) attributes[0]);
            return defaultResult;
        }
    }
}