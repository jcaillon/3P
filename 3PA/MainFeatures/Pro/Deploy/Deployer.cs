#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (Deployer.cs) is part of 3P.
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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WixToolset.Dtf.Compression;
using _3PA.Lib;

namespace _3PA.MainFeatures.Pro.Deploy {

    /// <summary>
    /// This class is responsible for deploying FileToDeploy following DeploymentRules
    /// </summary>
    internal class Deployer {
        
        #region Fields

        private bool _compileLocally;
        private string _deploymentDirectory;
        private string _sourceDirectory;
        private int _totalNbFilesToDeploy;
        private int _nbFilesDeployed;
        private bool _compileUnmatchedProgressFilesToDeployDir;
        private CompressionLevel _compressionLevel;

        #endregion

        #region Life and death

        /// <summary>
        /// Constructor
        /// </summary>
        public Deployer(List<DeployRule> deployRules, ProEnvironment.ProEnvironmentObject proEnv) {
            _compileLocally = proEnv.CompileLocally;
            _deploymentDirectory = proEnv.BaseCompilationPath;
            _sourceDirectory = (string.IsNullOrEmpty(proEnv.BaseLocalPath) ? "" : Path.GetFullPath(proEnv.BaseLocalPath).TrimEnd('\\'));
            _compressionLevel = CompressionLevel.Normal;
            _compileUnmatchedProgressFilesToDeployDir = true;
            ProlibPath = proEnv.ProlibPath;

            DeployRules = deployRules.ToNonNullList();
            DeployVarList = DeployRules.OfType<DeployVariableRule>().ToNonNullList();
        }

        #endregion

        #region Properties

        public string ProlibPath { get; set; }

        /// <summary>
        /// List of deployment rules filtered + sorted for this env
        /// </summary>
        public List<DeployRule> DeployRules { get; private set; }

        /// <summary>
        /// List of var rules filtered + sorted for this env
        /// </summary>
        public List<DeployVariableRule> DeployVarList { get; private set; }

        /// <summary>
        /// List of deployment rules filtered + sorted for this env
        /// </summary>
        public List<DeployTransferRule> DeployTransferRules {
            get { return DeployRules.OfType<DeployTransferRule>().ToNonNullList(); }
        }

        /// <summary>
        /// List of deployment rules filtered + sorted for this env
        /// </summary>
        public List<DeployFilterRule> DeployFilterRules {
            get { return DeployRules.OfType<DeployFilterRule>().ToNonNullList(); }
        }

        #endregion

        #region public methods

        /// <summary>
        /// returns the list of transfers needed for a given file
        /// </summary>
        public List<FileToDeploy> GetTransfersNeededForFile(string sourcePath, int step) {
            try { 
                var fileName = Path.GetFileName(sourcePath);
                if (fileName != null)
                    return GetTargetsNeeded(sourcePath, step, DeployTransferRuleTarget.File).Select(deploy => deploy.Set(sourcePath, Path.Combine(deploy.TargetBasePath, fileName))).ToList();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "GetTransfersNeededForFile");
            }
            return new List<FileToDeploy>();
        }

        /// <summary>
        /// returns the list of transfers needed for a given folder
        /// </summary>
        public List<FileToDeploy> GetTransfersNeededForFolders(string sourcePath, int step) {
            try {
                return GetTargetsNeeded(sourcePath, step, DeployTransferRuleTarget.Folder).Select(deploy => deploy.Set(sourcePath, deploy.TargetBasePath)).ToList();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "GetTransfersNeededForFolders");
            }
            return new List<FileToDeploy>();
        }

        /// <summary>
        /// Returns a list of files in the given folders (recursively or not depending on the option),
        /// this list is filtered thanks to the filtered rules given
        /// </summary>
        public IEnumerable<string> GetFilteredList(IEnumerable<string> list, int step) {
            // construct the filters list
            var includeFiltersList = DeployFilterRules.Where(rule => rule.Step == step && rule.Include).ToList();
            var excludeFiltersList = DeployFilterRules.Where(rule => rule.Step == step && !rule.Include).ToList();

            foreach (var file in list) {
                if (IsFilePassingFilters(file, includeFiltersList, excludeFiltersList))
                    yield return file;
            }
        }

        /// <summary>
        /// Returns true if the given path passes the include + exclude filters
        /// </summary>
        private bool IsFilePassingFilters(string path, List<DeployFilterRule> includeFiltersList, List<DeployFilterRule> excludeFiltersList) {
            bool passing = true;

            // test include filters
            if (includeFiltersList.Count > 0) {
                var hasMatch = includeFiltersList.Any(rule => path.RegexMatch(rule.RegexSourcePattern));
                passing = hasMatch;
            }

            // test exclude filters
            if (excludeFiltersList.Count > 0) {
                var hasNoMatch = excludeFiltersList.All(rule => !path.RegexMatch(rule.RegexSourcePattern));
                passing = passing && hasNoMatch;
            }

            return passing;
        }


        /// <summary>
        /// This method returns the target directories (or pl, zip or ftp) for the given source path, for each :
        /// If CompileLocally, returns the directory of the source
        /// If the deployment dir is empty and we didn't match an absolute compilation path, returns the source directory as well
        /// </summary>
        private List<FileToDeploy> GetTargetsNeeded(string sourcePath, int step, DeployTransferRuleTarget targetType) {

            // local compilation? return only one path, MOVE next to the source
            if (step == 0 && _compileLocally) {
                return new List<FileToDeploy> {
                    FileToDeploy.New(DeployType.Move, sourcePath, Path.GetDirectoryName(sourcePath), null)
                };
            }

            var outList = new List<FileToDeploy>();

            // for each transfer rule that match the source pattern
            foreach (var rule in DeployTransferRules.Where(rule => sourcePath.RegexMatch(GetRegexAndReplaceVariablesIn(rule.SourcePattern)) && rule.Step == step && rule.TargetType == targetType)) {
                string targetBasePath;

                var deployTarget = ReplaceVariablesIn(rule.DeployTarget ?? sourcePath);

                if (rule.ShouldDeployTargetReplaceDollar) {
                    string matchedPath;
                    try {
                        matchedPath = sourcePath.RegexFind(GetRegexAndReplaceVariablesIn(rule.SourcePattern))[0].Groups[0].Value;
                    } catch (Exception) {
                        matchedPath = sourcePath;
                    }
                    targetBasePath = matchedPath.RegexReplace(GetRegexAndReplaceVariablesIn(rule.SourcePattern), deployTarget);
                } else {
                    targetBasePath = deployTarget;
                }

                if (rule.Type != DeployType.Ftp && !Path.IsPathRooted(deployTarget)) {
                    targetBasePath = Path.Combine(_deploymentDirectory, targetBasePath);
                } else if (deployTarget.Equals("\\")) {
                    targetBasePath = _deploymentDirectory;
                }

                if (!outList.Exists(needed => needed.TargetBasePath.EqualsCi(targetBasePath))) {
                    outList.Add(FileToDeploy.New(rule.Type, sourcePath, targetBasePath, rule));
                }

                // stop ?
                if (!rule.ContinueAfterThisRule || rule.Type == DeployType.Move)
                    break;
            }

            // for the compilation step
            if (step == 0) {
                if (outList.Count == 0 && _compileUnmatchedProgressFilesToDeployDir) {
                    // move to deployment directory by default
                    outList.Add(FileToDeploy.New(DeployType.Move, sourcePath, _deploymentDirectory, null));
                } else {
                    var lastCopy = outList.LastOrDefault() as FileToDeployCopy;
                    if (lastCopy != null) {
                        // if the last deploy is a copy, make it a move
                        lastCopy.FinalDeploy = true;
                    }
                }
            }

            return outList;
        }

        #endregion

        #region Static GetFilesToDeployAfterCompilation

        /// <summary>
        /// Creates a list of files to deploy after a compilation,
        /// for each Origin file will correspond one (or more if it's a .cls) .r file,
        /// and one .lst if the option has been checked
        /// </summary>
        public static List<FileToDeploy> GetFilesToDeployAfterCompilation(ProExecutionCompile execution) {
            var outputList = new List<FileToDeploy>();
            try { 
                var filesCompiled = execution.Files.ToList();

                // Handle the case of .cls files, for which several .r code are compiled
                foreach (var clsFile in execution.Files.Where(file => file.SourcePath.EndsWith(ProExecutionHandleCompilation.ExtCls, StringComparison.CurrentCultureIgnoreCase))) {

                    // if the file we compiled inherits from another class or if another class inherits of our file, 
                    // there is more than 1 *.r file generated. Moreover, they are generated in their package folders

                    // for each *.r file in the compilation output directory
                    foreach (var rCodeFilePath in Directory.EnumerateFiles(clsFile.CompilationOutputDir, "*" + ProExecutionHandleCompilation.ExtR, SearchOption.AllDirectories)) {
                        try {
                            // find the path of the source
                            var relativePath = rCodeFilePath.Replace(clsFile.CompilationOutputDir, "").TrimStart('\\');

                            // if this is actually the .cls file we want to compile, the .r file isn't necessary directly in the compilation dir like we expect,
                            // it can be in folders corresponding to the package of the class
                            if (Path.GetFileNameWithoutExtension(clsFile.SourcePath ?? "").Equals(Path.GetFileNameWithoutExtension(relativePath))) {
                                // correct .r path
                                clsFile.CompOutputR = rCodeFilePath;
                                continue;
                            }

                            // otherwise, try to get the source .cls for this .r
                            var sourcePath = execution.ProEnv.FindFirstFileInPropath(Path.ChangeExtension(relativePath, ProExecutionHandleCompilation.ExtCls));

                            // if the source isn't already in the files that needed to be compiled, we add it
                            if (!string.IsNullOrEmpty(sourcePath) && !filesCompiled.Exists(compiledFile => compiledFile.SourcePath.Equals(sourcePath))) {
                                filesCompiled.Add(new FileToCompile(sourcePath) {
                                    CompilationOutputDir = clsFile.CompilationOutputDir,
                                    CompiledSourcePath = sourcePath,
                                    CompOutputR = rCodeFilePath
                                });
                            }
                        } catch (Exception e) {
                            ErrorHandler.LogError(e);
                        }
                    }
                }

                // for each .r
                foreach (var compiledFile in filesCompiled) {
                    if (string.IsNullOrEmpty(compiledFile.CompOutputR))
                        continue;
                    foreach (var deployNeeded in execution.ProEnv.Deployer.GetTargetsNeeded(compiledFile.SourcePath, 0, DeployTransferRuleTarget.File)) {

                        string targetRPath;
                        if (execution.ProEnv.CompileLocally)
                            targetRPath = Path.Combine(deployNeeded.TargetBasePath, Path.GetFileName(compiledFile.CompOutputR));
                        else
                            targetRPath = Path.Combine(deployNeeded.TargetBasePath, compiledFile.CompOutputR.Replace(compiledFile.CompilationOutputDir, "").TrimStart('\\'));

                        // add .r and .lst (if needed) to the list of files to deploy
                        outputList.Add(deployNeeded.Set(compiledFile.CompOutputR, targetRPath));

                        // listing
                        if (execution.CompileWithListing && !string.IsNullOrEmpty(compiledFile.CompOutputLis)) {
                            outputList.Add(deployNeeded.Copy(compiledFile.CompOutputLis, Path.ChangeExtension(targetRPath, ProExecutionHandleCompilation.ExtLis)));
                        }

                        // xref
                        if (execution.CompileWithXref && !string.IsNullOrEmpty(compiledFile.CompOutputXrf)) {
                            outputList.Add(deployNeeded.Copy(compiledFile.CompOutputXrf, Path.ChangeExtension(targetRPath, execution.UseXmlXref ? ProExecutionHandleCompilation.ExtXrfXml : ProExecutionHandleCompilation.ExtXrf)));
                        }

                        // debug-list
                        if (execution.CompileWithDebugList && !string.IsNullOrEmpty(compiledFile.CompOutputDbg)) {
                            outputList.Add(deployNeeded.Copy(compiledFile.CompOutputDbg, Path.ChangeExtension(targetRPath, ProExecutionHandleCompilation.ExtDbg)));
                        }
                    }
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Failed to find .r codes to deploy");
            }
            return outputList;
        }

        #endregion

        #region Deploy Files

        /// <summary>
        /// Deploy a given list of files (can reduce the list if there are duplicated items so it returns it)
        /// </summary>
        public List<FileToDeploy> DeployFiles(List<FileToDeploy> deployToDo, Action<float> updateDeploymentPercentage, CancellationTokenSource cancelToken) {
            try {
                if (cancelToken == null) {
                    cancelToken = new CancellationTokenSource();
                }

                // make sure to transfer a given file only once at the same place (happens with .cls file since a source
                // can have several .r files generated if it is used in another classes)
                deployToDo = deployToDo
                    .GroupBy(deploy => deploy.To)
                    .Select(group => group.FirstOrDefault(move => Path.GetFileNameWithoutExtension(move.From ?? "").Equals(Path.GetFileNameWithoutExtension(move.Origin))) ?? group.First())
                    .ToList();

                // create directories that must exist to be able to deploy
                deployToDo
                    .Where(deploy => !string.IsNullOrEmpty(deploy.DirectoryThatMustExist))
                    .GroupBy(deploy => deploy.DirectoryThatMustExist)
                    .Select(group => group.First())
                    .ToNonNullList()
                    .ForEach(deploy => {
                        try {
                            if (!Directory.Exists(deploy.DirectoryThatMustExist)) {
                                Directory.CreateDirectory(deploy.DirectoryThatMustExist);
                            }
                        } catch (Exception e) {
                            deploy.DeployError = "Couldn't create directory " + deploy.DirectoryThatMustExist.Quoter() + " : " + e.Message.Quoter();
                        }
                    });

                _nbFilesDeployed = 0;
                _totalNbFilesToDeploy = deployToDo.Count;

                #region for packs we do everything here

                // Create the list of each pack / files in pack
                // path of pack -> (ArchiveInfo, Dictionary<relativePathInPack, FileToDeploy>)
                var packs = new Dictionary<string, Tuple<IPackager, Dictionary<string, FileToDeployInPack>>>();
                deployToDo
                    .Where(deploy => deploy is FileToDeployInPack)
                    .Cast<FileToDeployInPack>()
                    .ToNonNullList()
                    .ForEach(deployPack => {
                        if (deployPack.IfFromFileExists()) {

                            // add new pack
                            if (!packs.ContainsKey(deployPack.PackPath)) {
                                packs.Add(
                                    deployPack.PackPath,
                                    new Tuple<IPackager, Dictionary<string, FileToDeployInPack>>(deployPack.NewArchive(this), new Dictionary<string, FileToDeployInPack>())
                                );
                            }

                            // add new file in archive
                            if (!packs[deployPack.PackPath].Item2.ContainsKey(deployPack.From)) {
                                packs[deployPack.PackPath].Item2.Add(deployPack.RelativePathInPack, deployPack);
                            }
                        }
                    });

                // package each pack (can't do it in parallel because of .pl packs, for which we MOVE the files to temp folders so they wouldn't be available for other packs
                foreach (var pack in packs) {
                    // canceled?
                    cancelToken.Token.ThrowIfCancellationRequested();

                    try {
                        var currentPack = pack;
                        pack.Value.Item1.PackFileSet(pack.Value.Item2, _compressionLevel, (sender, args) => {
                            // canceled?
                            if (!args.CannotCancel)
                                cancelToken.Token.ThrowIfCancellationRequested();

                            if (args.ProgressType == ArchiveProgressType.FinishFile) {
                                // set the FileToDeploy state
                                if (currentPack.Value.Item2.ContainsKey(args.CurrentFileName)) {

                                    if (!currentPack.Value.Item2[args.CurrentFileName].IsOk) {
                                        _nbFilesDeployed++;
                                        if (updateDeploymentPercentage != null) {
                                            updateDeploymentPercentage((float) _nbFilesDeployed / _totalNbFilesToDeploy * 100);
                                        }
                                    }

                                    if (args.TreatmentException != null) {
                                        currentPack.Value.Item2[args.CurrentFileName].RegisterArchiveException(args.TreatmentException);
                                    } else {
                                        currentPack.Value.Item2[args.CurrentFileName].IsOk = true;
                                    }
                                }
                            }
                        });
                    } catch (OperationCanceledException) {
                        throw;
                    } catch (Exception e) {
                        // set the deploy error for each file
                        foreach (var kpv in pack.Value.Item2) {
                            kpv.Value.RegisterArchiveException(e);
                        }
                    }
                }

                #endregion

                // do a deployment action for each file
                ParallelOptions parallelOptions = new ParallelOptions { CancellationToken = cancelToken.Token };
                Parallel.ForEach(deployToDo.Where(deploy => !(deploy is FileToDeployInPack) && deploy.CanBeParallelized), parallelOptions, file => {
                    // canceled?
                    parallelOptions.CancellationToken.ThrowIfCancellationRequested();

                    if (file.DeploySelf())
                        _nbFilesDeployed++;
                    if (updateDeploymentPercentage != null) {
                        updateDeploymentPercentage((float) _nbFilesDeployed / _totalNbFilesToDeploy * 100);
                    }
                });

                foreach (var file in deployToDo.Where(deploy => !(deploy is FileToDeployInPack) && !deploy.CanBeParallelized)) {
                    // canceled?
                    cancelToken.Token.ThrowIfCancellationRequested();

                    if (file.DeploySelf())
                        _nbFilesDeployed++;
                    if (updateDeploymentPercentage != null) {
                        updateDeploymentPercentage((float)_nbFilesDeployed / _totalNbFilesToDeploy * 100);
                    }
                }

            } catch (OperationCanceledException) {
                // we expect this exception if the task has been canceled
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error during the deployment");
            }

            return deployToDo;
        }

        #endregion

        #region Private / Utils

        /// <summary>
        /// Replace the variables &lt;XXX&gt; in the string
        /// </summary>
        private string ReplaceVariablesIn(string input) {
            if (input.ContainsFast("<")) {
                var fr = new FastReplacer("<", ">", false);
                fr.Append(input);
                // special replacement
                fr.Replace(@"<ROOT>", input.StartsWith(":") ? Regex.Escape(_sourceDirectory) : _sourceDirectory);
                foreach (var variableRule in DeployVarList) {
                    fr.Replace(variableRule.VariableName, variableRule.Path);
                }
                return fr.ToString();
            }
            return input;
        }

        private string GetRegexAndReplaceVariablesIn(string input) {
            input = ReplaceVariablesIn(input);
            return input.StartsWith(":") ? input.Remove(0, 1) : input.Replace('/', '\\').WildCardToRegex();
        }

        #endregion
        
    }

    #region IPackager

    internal interface IPackager {

        /// <summary>
        /// Compresses files into a pack, specifying the names used to store the files in the pack
        /// </summary>
        /// <param name="files">A mapping from internal file paths to external file paths.</param>
        /// <param name="compLevel">The compression level used when creating the pack</param>
        /// <param name="progressHandler">Handler for receiving progress information; this may be null if progress is not desired.</param>
        void PackFileSet(IDictionary<string, FileToDeployInPack> files, CompressionLevel compLevel, EventHandler<ArchiveProgressEventArgs> progressHandler);

    }

    #endregion

}