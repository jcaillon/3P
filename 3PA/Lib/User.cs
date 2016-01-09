using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using _3PA.MainFeatures;

namespace _3PA.Lib {

    // https://freegeoip.net/cvs/
    public static class User {

        public static void Ping() {
            // unique id : Environment.UserName + Environment.MachineName + GetMacAddress()
            // send : pays / version 3P / version Npp / date-time /
        }

        public static string GetMacAddress() {
            string mac = string.Empty;
            try {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration where IPEnabled=true");
                IEnumerable<ManagementObject> objects = searcher.Get().Cast<ManagementObject>();
                mac = (from o in objects orderby o["IPConnectionMetric"] select o["MACAddress"].ToString()).FirstOrDefault();
            } catch (Exception e) {
                if (!(e is ArgumentNullException)) {
                    ErrorHandler.DirtyLog(e);
                }
            }
            return mac;
        }

    }
}
