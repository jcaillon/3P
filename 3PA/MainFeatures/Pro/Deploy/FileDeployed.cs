#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (FileDeployed.cs) is part of 3P.
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
using System.Xml.Serialization;

namespace _3PA.MainFeatures.Pro.Deploy {

    [Serializable]
    [XmlInclude(typeof(FileDeployed))]
    [XmlInclude(typeof(FileDeployedCompiled))]
    public class FileSourceInfo {

        /// <summary>
        /// The relative path of the source file
        /// </summary>
        public string SourcePath { get; set; }

        public DateTime LastWriteTime { get; set; }

        public long Size { get; set; }

        /// <summary>
        /// MD5 
        /// </summary>
        public byte[] Md5 { get; set; }

    }

    [Serializable]
    [XmlInclude(typeof(FileDeployedCompiled))]
    public class FileDeployed : FileSourceInfo {

        /// <summary>
        /// a list of the targets for this deployment
        /// </summary>
        public List<DeploymentTarget> Targets { get; set; }

        /// <summary>
        /// The action done for this file
        /// </summary>
        public DeploymentAction Action { get; set; }

    }

    [Serializable]
    public class FileDeployedCompiled : FileDeployed {

        /// <summary>
        /// represents the source file (i.e. includes) used to generate a given .r code file
        /// </summary>
        public List<FileSourceInfo> RequiredFiles { get; set; }

        /// <summary>
        /// represent the tables that were referenced in a given .r code file
        /// </summary>
        public List<TableCrc> RequiredTables { get; set; }

    }

    [Serializable]
    public class DeploymentTarget {

        /// <summary>
        /// Relative target path (relative to the target directory)
        /// </summary>
        public string TargetPath { get; set; }

        /// <summary>
        /// The type of deployment done for this target
        /// </summary>
        public DeployType DeployType { get; set; }

        /// <summary>
        /// Relative path of the pack in which this file is deployed (if any)
        /// </summary>
        public string TargetPackPath { get; set; }

        /// <summary>
        /// Relative path within the pack (if any)
        /// </summary>
        public string TargetPathInPack { get; set; }
    }

    public enum DeploymentAction {
        Added,
        Replaced,
        Deleted,
        Existing,
    }

}
