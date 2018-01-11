#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (DeploymentProfile.cs) is part of 3P.
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
using System.IO;
using System.Linq;
using _3PA.Lib;

namespace _3PA.MainFeatures.Pro.Deploy {

    internal class DeploymentProfile {

        #region public static event

        /// <summary>
        /// Called when the list is updated
        /// </summary>
        public static event Action OnDeployProfilesUpdate;

        #endregion

        #region Fields

        /// <summary>
        /// IF YOU ADD A FIELD, DO NOT FORGET TO ALSO ADD THEM IN THE HARD COPY CONSTRUCTOR!!!
        /// </summary>
        public string Name = "";

        public string SourceDirectory = "";

        public bool ExploreRecursively = true;

        public bool AutoUpdateSourceDir = true;

        public bool ForceSingleProcess;

        public bool OnlyGenerateRcode = true;

        public int NumberProcessPerCore = 3;

        #endregion

        #region Life and death

        public DeploymentProfile() {}

        /// <summary>
        /// Allows to do a hard copy
        /// </summary>
        public DeploymentProfile(DeploymentProfile profile) {
            Name = profile.Name;
            SourceDirectory = profile.SourceDirectory;
            ExploreRecursively = profile.ExploreRecursively;
            AutoUpdateSourceDir = profile.AutoUpdateSourceDir;
            ForceSingleProcess = profile.ForceSingleProcess;
            OnlyGenerateRcode = profile.OnlyGenerateRcode;
            NumberProcessPerCore = profile.NumberProcessPerCore;
        }

        #endregion

        #region Static

        private static List<DeploymentProfile> _list;

        private static DeploymentProfile _current;

        /// <summary>
        /// Get a list of profiles
        /// </summary>
        public static List<DeploymentProfile> List {
            get {
                if (_list == null) {
                    if (File.Exists(Config.FileDeployProfiles)) {
                        _list = new List<DeploymentProfile>();
                        try {
                            Object2Xml<DeploymentProfile>.LoadFromFile(_list, Config.FileDeployProfiles);
                        } catch (Exception e) {
                            ErrorHandler.ShowErrors(e, "Error when loading settings", Config.FileDeployProfiles);
                        }
                    }
                    if (_list == null || _list.Count == 0)
                        _list = new List<DeploymentProfile> {new DeploymentProfile()};
                    if (OnDeployProfilesUpdate != null)
                        OnDeployProfilesUpdate();
                }
                return _list;
            }
            set { _list = value; }
        }

        /// <summary>
        /// Get the current profile
        /// </summary>
        public static DeploymentProfile Current {
            get {
                if (_current == null) {
                    _current = List.FirstOrDefault(profile => profile.Name.Equals(Config.Instance.CurrentDeployProfile));
                    if (_current == null)
                        _current = List.First();
                    Config.Instance.CurrentDeployProfile = _current.Name;
                }
                return _current;
            }
            set { _current = value; }
        }

        /// <summary>
        /// Resets the list to read it again
        /// </summary>
        public static void Import() {
            List = null;
            List = List;
        }

        public static void Save() {
            try {
                Object2Xml<DeploymentProfile>.SaveToFile(List, Config.FileDeployProfiles);
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error while saving the deployment profiles");
            }
        }

        #endregion
    }
}