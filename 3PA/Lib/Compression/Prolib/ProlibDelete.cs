using System;
using System.Collections.Generic;
using System.IO;
using WixToolset.Dtf.Compression;
using _3PA.MainFeatures.Pro;

namespace _3PA.Lib.Compression.Prolib {

    /// <summary>
    /// Allows to pack files into a prolib file
    /// </summary>
    internal class ProlibDelete : IPackager {

        #region Private

        private ProcessIo _prolibExe;
        private string _archivePath;

        #endregion

        #region Life and death

        public ProlibDelete(string archivePath, string prolibPath) {
            _archivePath = archivePath;
            _prolibExe = new ProcessIo(prolibPath);
        }

        #endregion

        #region Methods

        public void PackFileSet(IDictionary<string, FileToDeployInPack> files, CompressionLevel compLevel, EventHandler<ArchiveProgressEventArgs> progressHandler) {
            var archiveFolder = Path.GetDirectoryName(_archivePath);
            if (!string.IsNullOrEmpty(archiveFolder))
                _prolibExe.StartInfo.WorkingDirectory = archiveFolder;

            foreach (var file in files.Values) {
                _prolibExe.Arguments = _archivePath.ProQuoter() + " -delete " + file.RelativePathInPack;
                var isOk = _prolibExe.TryDoWait(true);
                if (progressHandler != null) {
                    progressHandler(this, new ArchiveProgressEventArgs(ArchiveProgressType.FinishFile, file.RelativePathInPack, isOk ? null : new Exception(_prolibExe.ErrorOutput.ToString())));
                }
            }

        }

        #endregion

    }
}
