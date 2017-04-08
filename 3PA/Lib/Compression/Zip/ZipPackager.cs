using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WixToolset.Dtf.Compression;
using WixToolset.Dtf.Compression.Zip;
using _3PA.MainFeatures.Pro;

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
