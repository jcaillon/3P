using System;
using System.Collections.Generic;
using Microsoft.Win32;
using _3PA.MainFeatures;

namespace _3PA.Lib {
    public static class RegistryWrapper {

        /// <summary>
        /// Get the value in the registry
        /// </summary>
        public static object GetValue(RegistryHive registryHive, string nodeName, string keyName) {
            try {
                foreach (RegistryView registryView in new List<RegistryView> { RegistryView.Registry32, RegistryView.Registry64 }) {
                    using (var hive = RegistryKey.OpenBaseKey(registryHive, registryView)) {
                        using (var subKey = hive.OpenSubKey(nodeName, RegistryKeyPermissionCheck.ReadSubTree)) {
                            if (subKey != null && subKey.GetValue(keyName) != null) {
                                return subKey.GetValue(keyName);
                            }
                        }
                    }
                }
            } catch (Exception e) {
                ErrorHandler.LogError(e);
            }
            return null;
        }
    }
}
