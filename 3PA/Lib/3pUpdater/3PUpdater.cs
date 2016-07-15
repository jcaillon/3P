#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (3PUpdater.cs) is part of 3P.
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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using _3PA.MainFeatures;
using _3PA.Properties;

namespace _3PA.Lib._3pUpdater {
    internal class _3PUpdater {

        #region private fields

        private static _3PUpdater _instance;

        private TypeOfExeNeeded _typeOfExeNeeded = TypeOfExeNeeded.None;

        #endregion

        #region singleton

        /// <summary>
        /// singleton
        /// </summary>
        public static _3PUpdater Instance {
            get { return _instance ?? (_instance = new _3PUpdater()); }
        }

        #endregion

        #region methods

        /// <summary>
        /// Use to method to a file that needs to be moved AFTER npp is shutdown
        /// </summary>
        public bool AddFileToMove(string from, string to) {

            if (string.IsNullOrEmpty(from) || !File.Exists(from) || string.IsNullOrEmpty(to) || !Directory.Exists(Path.GetDirectoryName(to) ?? ""))
                return false;

            // configure the update
            File.WriteAllText(Config.FileUpdaterLst, string.Join("\t", from, to), Encoding.Default);

            // subscribe to the Npp shutdown event if it's not already done
            if (_typeOfExeNeeded == TypeOfExeNeeded.None)
                Plug.OnShutDown += ExecuteUpdateAsync;

            // test if the destination directory is writable
            var typeOfExeNeeded = Utils.IsDirectoryWritable(Path.GetDirectoryName(to)) ? TypeOfExeNeeded.UserRights : TypeOfExeNeeded.AdminRights;
            if (typeOfExeNeeded > _typeOfExeNeeded)
                _typeOfExeNeeded = typeOfExeNeeded;
            return true;
        }

        /// <summary>
        /// returns true if a 3pUpdate.exe needing admin rights will be launched on npp shutdown
        /// </summary>
        public bool IsAdminRightsNeeded {
            get { return _typeOfExeNeeded == TypeOfExeNeeded.AdminRights; }
        }

        private void ExecuteUpdateAsync() {
            try {
                // copy the 3pUpdater.exe, one or the other version depending if we need admin rights
                File.WriteAllBytes(Config.FileUpdaterExe, _typeOfExeNeeded == TypeOfExeNeeded.UserRights ? Resources._3pUpdater_user : Resources._3pUpdater);

                // execute it, don't wait for the end
                var process = new Process {
                    StartInfo = {
                        FileName = Config.FileUpdaterExe,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };
                process.Start();

            } catch (Exception e) {
                if (!(e is Win32Exception))
                    ErrorHandler.Log("OnNotepadExit\r\n" + e);
            }
        }

        #endregion


        private enum TypeOfExeNeeded {
            None,
            UserRights,
            AdminRights
        }
    }
}
