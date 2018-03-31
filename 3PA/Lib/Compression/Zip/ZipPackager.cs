#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (ZipPackager.cs) is part of 3P.
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
using System.Text;
using _3PA.MainFeatures.Pro;
using _3PA.MainFeatures.Pro.Deploy;

namespace _3PA.Lib.Compression.Zip {

    /// <summary>
    /// Allows to pack files into zip
    /// </summary>
    internal class ZipPackager : ZipInfo, IPackager {

        public ZipPackager(string path) : base(path) {}

        public void PackFileSet(IDictionary<string, FileToDeployInPack> files, CompressionLevel compLevel, EventHandler<ArchiveProgressEventArgs> progressHandler) {
            var filesDic = files.ToDictionary(kpv => kpv.Key, kpv => kpv.Value.From);
            PackFileSet(filesDic, compLevel, progressHandler);
        }

    }
}
