#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (DifferentialDeploymentHandler.cs) is part of 3P.
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using _3PA.Lib;

namespace _3PA.MainFeatures.Pro.Deploy {

    internal class DifferentialDeploymentHandler : DeploymentHandler {

        #region Properties

        /// <summary>
        /// When true, we activate the log just before compiling with FileId active + we generate a file that list referenced table in the .r
        /// </summary>
        public override bool IsAnalysisMode { get { return true; } }

        /// <summary>
        /// This returns a serializable list of files that were compiled/deployed during this deployment 
        /// </summary>
        public List<FileDeployed> FilesDeployed {
            get {
                var output = new List<FileDeployed>();
                foreach (var kpv in _filesToDeployPerStep.Where(kpv => kpv.Key <= 1)) {
                    var step = kpv.Key;
                    foreach (var fileToDeploy in kpv.Value) {
                        var compiledFile = step > 0 ? null : _proCompilation.ListFilesToCompile.Find(compile => compile.SourcePath.Equals(fileToDeploy.Origin));
                        var fileToDeployInPack = fileToDeploy as FileToDeployInPack;
                        FileDeployed newFile;
                        if (compiledFile != null) {
                            newFile = new FileDeployedCompiled();
                            var newCompiledFile = (FileDeployedCompiled) newFile;
                            newCompiledFile.RequiredFiles = compiledFile.RequiredFiles == null ? null : compiledFile.RequiredFiles.Select(s => s.Replace(_currentProfile.SourceDirectory, "").TrimStart('\\')).ToList();
                            newCompiledFile.RequiredTables = compiledFile.RequiredTables;
                        } else {
                            newFile = new FileDeployed();
                        }
                        newFile.SourcePath = fileToDeploy.Origin.Replace(_currentProfile.SourceDirectory, "").TrimStart('\\');
                        newFile.TargetPath = fileToDeploy.To.Replace(_proEnv.BaseCompilationPath, "").TrimStart('\\');
                        newFile.TargetPackPath = fileToDeployInPack == null ? null : fileToDeployInPack.PackPath.Replace(_currentProfile.SourceDirectory, "").TrimStart('\\'); ;
                        newFile.TargetPathInPack = fileToDeployInPack == null ? null : fileToDeployInPack.RelativePathInPack;
                        output.Add(newFile);
                    }
                }
                return output;
            }
        }

        /// <summary>
        /// Optionnal list of files compiled/deployed during the last deployment, needed
        /// if you want to be able to compute the difference with the current source state
        /// </summary>
        public List<FileDeployed> PreviouslyDeployedFiles { get; set; }

        public bool ForceFullDeploy { get; set; }

        #endregion

        #region Life and death

        public DifferentialDeploymentHandler(ProEnvironment.ProEnvironmentObject proEnv, DeploymentProfile currentProfile) : base(proEnv, currentProfile) {
            ForceFullDeploy = true;
        }

        #endregion

        #region Override

        /// <summary>
        /// Make the list of all the files that need to be (re)compiled
        /// </summary>
        /// <returns></returns>
        protected override List<FileToCompile> GetFilesToCompileInStepZero() {
            // Full deployment? 
            if (ForceFullDeploy || PreviouslyDeployedFiles == null)
                return base.GetFilesToCompileInStepZero();
            
            // Otherwise, we will only compile the new stuff
            var filesToCompile = new HashSet<string>();

            // first, list the files that need to be recompiled because of table CRC change
            FilesToCompileBecauseOfTableCrcChanges(ref filesToCompile);

            // then add the list of files that need to be recompiled simply because the source changed
            FilesToCompileBecauseOfSourceModification(ref filesToCompile);
            
            return filesToCompile.Select(s => new FileToCompile(s)).ToList();
        }

        protected override List<FileToDeploy> GetFilesToDeployInStepOne() {
            return base.GetFilesToDeployInStepOne();
        }

        #endregion

        /// <summary>
        /// the list of files that need to be recompiled simply because the source changed
        /// </summary>
        /// <param name="filesToCompile"></param>
        private void FilesToCompileBecauseOfSourceModification(ref HashSet<string> filesToCompile) {
            var fullListFromSource = GetFilteredFilesList(_currentProfile.SourceDirectory, 0, _currentProfile.ExploreRecursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly, Config.Instance.FilesPatternCompilable);
        }

        /// <summary>
        /// list the files that need to be recompiled because of table CRC change
        /// </summary>
        /// <param name="filesToCompile"></param>
        private void FilesToCompileBecauseOfTableCrcChanges(ref HashSet<string> filesToCompile) {
            var exec = new ProExecutionTableCrc {
                NeedDatabaseConnection = true
            };
            exec.Start();
            exec.WaitForProcessExit(0);
            var currentTables = exec.GetTableCrc();
            if (currentTables != null) {
                foreach (var prevCompFile in PreviouslyDeployedFiles.Where(file => file is FileDeployedCompiled).Cast<FileDeployedCompiled>().Where(file => file.RequiredTables != null)) {
                    foreach (var tableRequired in prevCompFile.RequiredTables) {
                        var foundCorrespondance = currentTables.Find(table => table.QualifiedTableName.EqualsCi(tableRequired.QualifiedTableName));

                        // the file uses an unknown table or uses a table which CRC changed?
                        if (foundCorrespondance == null || !foundCorrespondance.Crc.Equals(tableRequired.Crc)) {
                            if (!filesToCompile.Contains(prevCompFile.SourcePath))
                                filesToCompile.Add(prevCompFile.SourcePath);
                        }
                    }
                }
            }
        }
    }

}
