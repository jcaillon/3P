#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (User.cs) is part of 3P.
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
