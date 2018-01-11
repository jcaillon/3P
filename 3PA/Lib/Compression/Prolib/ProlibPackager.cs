#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProlibPackager.cs) is part of 3P.
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
using System.Threading.Tasks;
using WixToolset.Dtf.Compression;
using _3PA.MainFeatures;
using _3PA.MainFeatures.Pro.Deploy;

namespace _3PA.Lib.Compression.Prolib {

    /// <summary>
    /// Allows to pack files into a prolib file
    /// </summary>
    internal class ProlibPackager : IPackager {

        #region Private

        private ProcessIo _prolibExe;
        private string _archivePath;

        #endregion

        #region Life and death

        public ProlibPackager(string archivePath, string prolibPath) {
            _archivePath = archivePath;
            _prolibExe = new ProcessIo(prolibPath);
        }

        #endregion

        #region Methods

        public void PackFileSet(IDictionary<string, FileToDeployInPack> files, CompressionLevel compLevel, EventHandler<ArchiveProgressEventArgs> progressHandler) {

            // check that the folder to the archive exists
            var archiveFolder = Path.GetDirectoryName(_archivePath);
            if (!string.IsNullOrEmpty(archiveFolder) && !Directory.Exists(archiveFolder)) {
                Directory.CreateDirectory(archiveFolder);
            }
            if (string.IsNullOrEmpty(archiveFolder)) {
                throw new Exception("Couldn't find the folder for the targeted archive");
            }

            // create a unique temp folder for this .pl
            var uniqueTempFolder = Path.Combine(archiveFolder, Path.GetFileName(_archivePath) + "~" + Path.GetRandomFileName());
            var dirInfo = Directory.CreateDirectory(uniqueTempFolder);
            dirInfo.Attributes |= FileAttributes.Hidden;

            var subFolders = new Dictionary<string, List<FilesToMove>>();

            foreach (var file in files) {
                var subFolderPath = Path.GetDirectoryName(Path.Combine(uniqueTempFolder, file.Key));
                if (!string.IsNullOrEmpty(subFolderPath)) {
                    if (!subFolders.ContainsKey(subFolderPath)) {
                        subFolders.Add(subFolderPath, new List<FilesToMove>());
                        if (!Directory.Exists(subFolderPath)) {
                            Directory.CreateDirectory(subFolderPath);
                        }
                    }
                    subFolders[subFolderPath].Add(new FilesToMove(file.Value.From, Path.Combine(uniqueTempFolder, file.Key), file.Key));
                }
            }

            _prolibExe.StartInfo.WorkingDirectory = uniqueTempFolder;

            foreach (var subFolder in subFolders) {

                _prolibExe.Arguments = _archivePath.Quoter() + " -create -nowarn -add " + Path.Combine(subFolder.Key.Replace(uniqueTempFolder, "").TrimStart('\\'), "*").Quoter();

                // move files to the temp subfolder
                Parallel.ForEach(subFolder.Value, file => {
                    try {
                        if (file.Move) {
                            File.Move(file.Origin, file.Temp);
                        } else {
                            File.Copy(file.Origin, file.Temp);
                        }
                    } catch (Exception) {
                        // ignore
                    }
                });

                // now we just need to add the content of temp folders into the .pl
                bool prolibOk = false;
                try {
                    prolibOk = _prolibExe.TryDoWait(true);
                } catch (Exception e) {
                    ErrorHandler.LogError(e);
                }

                // move files from the temp subfolder
                Parallel.ForEach(subFolder.Value, file => {
                    Exception ex = null;
                    try {
                        if (file.Move) {
                            File.Move(file.Temp, file.Origin);
                        } else if (!File.Exists(file.Temp)) {
                            throw new Exception("Couldn't find temp file for " + file.Origin);
                        }
                    } catch (Exception e) {
                        ex = e;
                    }
                    try {
                        if (progressHandler != null) {
                            progressHandler(this, new ArchiveProgressEventArgs(ArchiveProgressType.FinishFile, file.RelativePath, ex ?? (prolibOk ? null : new Exception(_prolibExe.ErrorOutput.ToString()))));
                        }
                    } catch (Exception) {
                        // ignored
                    }
                });
            }

            // compress .pl
            _prolibExe.Arguments = _archivePath.Quoter() + " -compress -nowarn";
            _prolibExe.TryDoWait(true);

            // delete temp folder
            Directory.Delete(uniqueTempFolder, true);
        }

        #endregion

        #region FilesToMove

        private class FilesToMove {
            public string Origin { get; private set; }
            public string Temp { get; private set; }
            public string RelativePath { get; private set; }
            public bool Move { get; private set; }
            public FilesToMove(string origin, string temp, string relativePath) {
                Origin = origin;
                Temp = temp;
                RelativePath = relativePath;
                Move = origin.Length > 2 && temp.Length > 2 && origin.Substring(0, 2).EqualsCi(temp.Substring(0, 2));
            }
        }

        #endregion
        
    }
}
