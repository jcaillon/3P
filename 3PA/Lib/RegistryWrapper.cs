#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (RegistryWrapper.cs) is part of 3P.
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
namespace _3PA.Lib {
    public static class RegistryWrapper {
        /*
        /// <summary>
        /// Get the value in the registry MAKE AN APP CRASH
        /// </summary>
        private static object GetValue(RegistryHive registryHive, string nodeName, string keyName) {
            try {
                using (var hive = RegistryKey.OpenBaseKey(registryHive, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32)) {
                    using (var subKey = hive.OpenSubKey(nodeName, RegistryKeyPermissionCheck.ReadSubTree)) {
                        if (subKey != null && subKey.GetValue(keyName) != null) {
                            return subKey.GetValue(keyName);
                        }
                    }
                }
            } catch (Exception e) {
                ErrorHandler.LogError(e);
            }
            

            return null;
        }
        */
    }
}
