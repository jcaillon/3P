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
