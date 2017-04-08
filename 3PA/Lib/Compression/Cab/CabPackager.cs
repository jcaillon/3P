using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WixToolset.Dtf.Compression;
using WixToolset.Dtf.Compression.Cab;
using _3PA.MainFeatures.Pro;

namespace _3PA.Lib.Compression.Cab {

    /// <summary>
    /// Allows to pack files into a cab
    /// </summary>
    internal class CabPackager : CabInfo, IPackager {

        public CabPackager(string path) : base(path) {}

        public void PackFileSet(IDictionary<string, FileToDeployInPack> files, CompressionLevel compLevel, EventHandler<ArchiveProgressEventArgs> progressHandler) {
            var filesDic = files.ToDictionary(kpv => kpv.Key, kpv => kpv.Value.From);
            PackFileSet(filesDic, compLevel, progressHandler);
        }
    }

}
