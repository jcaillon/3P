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
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WixToolset.Dtf.Compression;
using WixToolset.Dtf.Compression.Zip;
using _3PA.Lib;
using _3PA.Lib.Ftp;
using _3PA.NppCore;
using _3PA._Resource;

namespace _3PA.MainFeatures.Pro {

    internal class Deployer {

        #region Static

        #region public event

        /// <summary>
        /// Called when the list of DeployTransfers is updated
        /// </summary>
        public static event Action OnDeployConfigurationUpdate;

        #endregion

        #region public Properties

        /// <summary>
        /// Get the compilation path list
        /// </summary>
        public static List<DeployRule> GetFullDeployRulesList {
            get {
                if (_fullDeployRulesList == null)
                    Import();
                return _fullDeployRulesList;
            }
        }

        #endregion

        #region private fields

        private static List<DeployRule> _fullDeployRulesList;

        #endregion

        #region Import/export

        public static void EditRules() {
            Export();
            Npp.OpenFile(Config.FileDeploymentRules);
        }

        public static void Export() {
            if (!File.Exists(Config.FileDeploymentRules))
                Utils.FileWriteAllBytes(Config.FileDeploymentRules, DataResources.DeploymentRules);
        }

        /// <summary>
        /// Read the list of compilation Path Items,
        /// if the file is present in the Config dir, use it
        /// </summary>
        public static void Import() {
            string outputMessage;
            _fullDeployRulesList = ReadConfigurationFile(Config.FileDeploymentRules, out outputMessage);

            if (outputMessage.Length > 0)
                UserCommunication.NotifyUnique("deployRulesErrors", "The following rules are incorrect :<br><br>" + outputMessage + "<br><br>They have been ignored, please correct them " + Config.FileDeploymentRules.ToHtmlLink("here"), MessageImg.MsgHighImportance, "Error(s) reading rules file", "Rules incorrect", args => {
                    EditRules();
                    args.Handled = true;
                });
            else
                UserCommunication.CloseUniqueNotif("deployRulesErrors");

            if (OnDeployConfigurationUpdate != null)
                OnDeployConfigurationUpdate();
        }
        
        #endregion

        #region BuildHtmlTableForRules

        /// <summary>
        /// returns a string containing an html representation of the compilation path table
        /// </summary>
        public static string BuildHtmlTableForRules(List<DeployRule> rules) {
            var strBuilder = new StringBuilder();

            if (rules.Any()) {
                if (rules.Exists(rule => rule is DeployVariableRule)) {
                    strBuilder.Append("<h2 style='padding-top: 0px; margin-top: 0px;'>Path variables</h2>");
                    strBuilder.Append("<table width='100%;'>");
                    strBuilder.Append("<tr class='CompPathHead'><td align='center' width='9%'>Application<br>Name</td><td align='center' width='9%'>Application<br>Suffix</td><td align='center' width='13%'>Var<br>Name</td><td width='69%' align='right'>Path</td></tr>");

                    var alt = false;
                    foreach (var rule in rules.OfType<DeployVariableRule>()) {
                        strBuilder.Append("<tr><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + (string.IsNullOrEmpty(rule.NameFilter) ? "*" : rule.NameFilter) + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + (string.IsNullOrEmpty(rule.SuffixFilter) ? "*" : rule.SuffixFilter) + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + WebUtility.HtmlEncode(rule.VariableName) + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='right'>" + (rule.Path.Length > 45 ? "..." + rule.Path.Substring(rule.Path.Length - 45) : rule.Path) + "</td></tr>");
                        alt = !alt;
                    }

                    strBuilder.Append("</table>");
                }

                if (rules.Exists(rule => rule is DeployFilterRule)) {
                    strBuilder.Append("<h2 style='padding-top: 0px; margin-top: 0px;'>Filter rules</h2>");
                    strBuilder.Append("<table width='100%;'>");
                    strBuilder.Append("<tr class='CompPathHead'><td align='center' width='5%'>Step</td><td align='center' width='9%'>Application<br>Name</td><td align='center' width='9%'>Application<br>Suffix</td><td align='center' width='8%'>Rule<br>Type</td><td width='69%' align='right'>Source path pattern</td></tr>");

                    var alt = false;
                    foreach (var rule in rules.OfType<DeployFilterRule>()) {
                        strBuilder.Append("<tr><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + rule.Step + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + (string.IsNullOrEmpty(rule.NameFilter) ? "*" : rule.NameFilter) + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + (string.IsNullOrEmpty(rule.SuffixFilter) ? "*" : rule.SuffixFilter) + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + (rule.Include ? "Include" : "Exclude") + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='right'>" + (rule.SourcePattern.Length > 45 ? "..." + rule.SourcePattern.Substring(rule.SourcePattern.Length - 45) : rule.SourcePattern) + "</td></tr>");
                        alt = !alt;
                    }

                    strBuilder.Append("</table>");
                }

                if (rules.Exists(rule => rule is DeployTransferRule)) {
                    strBuilder.Append("<h2>Transfer rules</h2>");
                    strBuilder.Append("<table width='100%;'>");
                    strBuilder.Append("<tr class='CompPathHead'><td align='center' width='5%'>Step</td><td align='center' width='9%'>Application<br>Name</td><td align='center' width='9%'>Application<br>Suffix</td><td align='center' width='6%'>Rule<br>Type</td><td align='center' width='5%'>Next?</td><td width='33%'>Source path pattern</td><td width='33%' align='right'>Deployment target</td></tr>");

                    var alt = false;
                    foreach (var rule in rules.OfType<DeployTransferRule>()) {
                        strBuilder.Append("<tr><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + rule.Step + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + (string.IsNullOrEmpty(rule.NameFilter) ? "*" : rule.NameFilter) + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + (string.IsNullOrEmpty(rule.SuffixFilter) ? "*" : rule.SuffixFilter) + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + rule.Type + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + (rule.ContinueAfterThisRule ? "Yes" : "No") + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + ">" + WebUtility.HtmlEncode(rule.SourcePattern.Length > 50 ? "..." + rule.SourcePattern.Substring(rule.SourcePattern.Length - 50) : rule.SourcePattern) + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='right'>" + WebUtility.HtmlEncode(rule.DeployTarget.Length > 50 ? "..." + rule.DeployTarget.Substring(rule.DeployTarget.Length - 50) : rule.DeployTarget) + "</td></tr>");
                        alt = !alt;
                    }

                    strBuilder.Append("</table>");
                }
            } else {
                strBuilder.Append("<b>No rules defined yet!</b><br>Modify the rules file to get started");
            }

            return strBuilder.ToString();
        }

        #endregion
        
        #region Private ReadConfigurationFile

        /// <summary>
        /// Reads the given rule file
        /// </summary>
        private static List<DeployRule> ReadConfigurationFile(string path, out string readingErrorsHtml) {

            var outputMessage = new StringBuilder();

            // get all the rules
            var list = new List<DeployRule>();
            Utils.ForEachLine(path, new byte[0], (lineNb, lineString) => {
                var items = lineString.Split('\t');

                int step = 0;
                if (items.Length > 1 && !int.TryParse(items[0].Trim(), out step))
                    return;

                // new transfer rule
                if (items.Length == 7) {
                    DeployType type;
                    if (!Enum.TryParse(items[3].Trim(), true, out type))
                        return;

                    var obj = DeployTransferRule.New(type);
                    obj.Step = step;
                    obj.NameFilter = items[1].Trim();
                    obj.SuffixFilter = items[2].Trim();
                    obj.ContinueAfterThisRule = items[4].Trim().EqualsCi("yes") || items[4].Trim().EqualsCi("true");
                    obj.Line = lineNb + 1;
                    obj.SourcePattern = items[5].Trim();
                    obj.DeployTarget = items[6].Trim().Replace('/', '\\');

                    obj.ShouldDeployTargetReplaceDollar = obj.DeployTarget.StartsWith(":");
                    if (obj.ShouldDeployTargetReplaceDollar)
                        obj.DeployTarget = obj.DeployTarget.Remove(0, 1);

                    string errorMsg;
                    if (obj.IsValid(out errorMsg))
                        list.Add(obj);
                    if (!string.IsNullOrEmpty(errorMsg)) {
                        outputMessage.Append(errorMsg);
                        outputMessage.Append("<br>");
                    }

                } else if (items.Length == 5) {
                    // new filter rule

                    var obj = new DeployFilterRule {
                        Step = step,
                        NameFilter = items[1].Trim(),
                        SuffixFilter = items[2].Trim(),
                        Include = items[3].Trim().EqualsCi("+") || items[3].Trim().EqualsCi("Include"),
                        SourcePattern = items[4].Trim()
                    };
                    obj.RegexSourcePattern = obj.SourcePattern.StartsWith(":") ? obj.SourcePattern.Remove(0, 1) : obj.SourcePattern.Replace('/', '\\').WildCardToRegex();

                    if (!string.IsNullOrEmpty(obj.SourcePattern))
                        list.Add(obj);

                } else if (items.Length == 4) {
                    // new variable

                    var obj = new DeployVariableRule {
                        NameFilter = items[0].Trim(),
                        SuffixFilter = items[1].Trim(),
                        VariableName = items[2].Trim(),
                        Path = items[3].Trim()
                    };

                    if (!obj.VariableName.StartsWith("<") || !obj.VariableName.EndsWith(">")) {
                        outputMessage.Append("- The variable rule line n°" + (lineNb + 1) + " is incorrect, the variable should have the format <b>&lt;XXX&gt;</b><br>");
                        return;
                    }

                    if (!string.IsNullOrEmpty(obj.Path))
                        list.Add(obj);
                }
            }, Encoding.Default);

            readingErrorsHtml = outputMessage.ToString();

            return list;
        }


        #endregion

        #endregion

        #region Object

        #region Fields

        private string _envName;
        private string _envSuffix;
        private bool _compileLocally;
        private string _deploymentDirectory;
        private int _totalNbFilesToDeploy;
        private int _nbFilesDeployed;

        #endregion

        #region Life and death

        /// <summary>
        /// Constructor
        /// </summary>
        public Deployer(string confFilePath, ProEnvironment.ProEnvironmentObject proEnv) {
            _envName = proEnv.Name;
            _envSuffix = proEnv.Suffix;
            _compileLocally = proEnv.CompileLocally;
            _deploymentDirectory = proEnv.BaseCompilationPath;
            ProlibPath = proEnv.ProlibPath;

            Init(confFilePath);
        }

        private void Init(string confFilePath) {

            string outputMessage;
            var rulesList = ReadConfigurationFile(confFilePath, out outputMessage);

            // Need to match the application name / suffix filter with the current env
            DeployRules = rulesList.Where(item => _envName.RegexMatch(item.NameFilter.WildCardToRegex()) && _envSuffix.RegexMatch(item.SuffixFilter.WildCardToRegex())).ToNonNullList();

            // sort the rules
            DeployRules.Sort((item1, item2) => {
                // exact name match first
                int compare = item2.NameFilter.EqualsCi(_envName).CompareTo(item1.NameFilter.EqualsCi(_envName));
                if (compare != 0)
                    return compare;

                // longer name filter first
                compare = item2.NameFilter.Length.CompareTo(item1.NameFilter.Length);
                if (compare != 0)
                    return compare;

                // exact suffix match first
                compare = item2.SuffixFilter.EqualsCi(_envSuffix).CompareTo(item1.SuffixFilter.EqualsCi(_envSuffix));
                if (compare != 0)
                    return compare;

                // longer suffix filter first
                compare = item2.SuffixFilter.Length.CompareTo(item1.SuffixFilter.Length);
                if (compare != 0)
                    return compare;


                // lower step first
                compare = item1.Step.CompareTo(item2.Step);
                if (compare != 0)
                    return compare;

                var itemTransfer1 = item1 as DeployTransferRule;
                var itemTransfer2 = item2 as DeployTransferRule;

                if (itemTransfer1 != null && itemTransfer2 != null) {
                    // continue first
                    compare = itemTransfer2.ContinueAfterThisRule.CompareTo(itemTransfer1.ContinueAfterThisRule);
                    if (compare != 0)
                        return compare;

                    // copy last
                    compare = itemTransfer1.Type.CompareTo(itemTransfer2.Type);
                    if (compare != 0)
                        return compare;

                    // first line in first in
                    return itemTransfer1.Line.CompareTo(itemTransfer2.Line);
                }

                // filter before transfer
                return itemTransfer1 == null ? 1 : -1;
            });

            DeployVarList = DeployRules.OfType<DeployVariableRule>().ToList();
        }

        #endregion

        #region Properties

        /// <summary>
        /// List of deployment rules filtered + sorted for this env
        /// </summary>
        public List<DeployRule> DeployRules { get; set; }

        /// <summary>
        /// List of var rules filtered + sorted for this env
        /// </summary>
        public List<DeployVariableRule> DeployVarList { get; set; }

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

        public string ProlibPath { get; set; }

        #endregion

        #region public methods

        /// <summary>
        /// Creates a list of files to deploy for the given step
        /// </summary>
        public List<FileToDeploy> GetFilesToDeployForStep(int step, List<string> listOfSourceDir, SearchOption searchOptions, string fileExtensionFilter = "*") {
            var outputList = new List<FileToDeploy>();

            // list the files to deploy
            foreach (var file in GetFilesList(listOfSourceDir, searchOptions, step)) {
                outputList.AddRange(GetTransfersNeededForFile(file, step));
            }

            return outputList;
        }

        /// <summary>
        /// returns the list of transfers needed for a given file
        /// </summary>
        public List<FileToDeploy> GetTransfersNeededForFile(string file, int step) {
            var fileName = Path.GetFileName(file);
            if (fileName != null)
                return GetTargetsNeededForFile(file, step).Select(deploy => deploy.Set(file, file, Path.Combine(deploy.TargetPath, fileName))).ToList();
            return new List<FileToDeploy>();
        }

        /// <summary>
        /// This method returns the target directories (or pl, zip or ftp) for the given source path, for each :
        /// If CompileLocally, returns the directory of the source
        /// If the deployment dir is empty and we didn't match an absolute compilation path, returns the source directory as well
        /// </summary>
        public List<FileToDeploy> GetTargetsNeededForFile(string sourcePath, int step) {

            // local compilation? return only one path, MOVE next to the source
            if (step == 0 && _compileLocally) {
                return new List<FileToDeploy> {
                    FileToDeploy.New(DeployType.Move, Path.GetDirectoryName(sourcePath))
                };
            }

            var outList = new List<FileToDeploy>();

            // for each transfer rule that match the source pattern
            foreach (var rule in DeployTransferRules.Where(rule => sourcePath.RegexMatch(GetRegexAndReplaceVariablesIn(rule.SourcePattern)) && rule.Step == step)) {
                string outPath;

                var deployTarget = ReplaceVariablesIn(rule.DeployTarget);

                if (rule.ShouldDeployTargetReplaceDollar) {
                    outPath = sourcePath.RegexReplace(GetRegexAndReplaceVariablesIn(rule.SourcePattern), deployTarget);
                } else {
                    outPath = deployTarget;
                }

                if (rule.Type != DeployType.Ftp && !Path.IsPathRooted(deployTarget)) {
                    outPath = Path.Combine(_deploymentDirectory, outPath);
                }

                if (!outList.Exists(needed => needed.TargetPath.EqualsCi(outPath))) {
                    outList.Add(FileToDeploy.New(rule.Type, outPath));
                }

                // stop ?
                if (!rule.ContinueAfterThisRule)
                    break;
            }

            // for the compilation step
            if (step == 0) {
                if (outList.Count == 0) {
                    // move to deployment directory by default
                    outList.Add(FileToDeploy.New(DeployType.Move, _deploymentDirectory));
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

        /// <summary>
        /// Returns a list of files in the given folders (recursively or not depending on the option),
        /// this list is filtered thanks to the filtered rules given
        /// </summary>
        public HashSet<string> GetFilesList(List<string> listOfFolderPath, SearchOption searchOptions, int step, string fileExtensionFilter = "*") {
            // constructs the list of all the files (unique) across the different folders
            var filesToCompile = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

            // construct the filters list
            var includeFiltersList = DeployFilterRules.Where(rule => rule.Step == step && rule.Include).ToList();
            var excludeFiltersList = DeployFilterRules.Where(rule => rule.Step == step && !rule.Include).ToList();

            foreach (var folderPath in listOfFolderPath.Where(Directory.Exists)) {
                foreach (var filePath in fileExtensionFilter.Split(',').SelectMany(searchPattern => Directory.EnumerateFiles(folderPath, searchPattern, searchOptions))) {
                    if (!filesToCompile.Contains(filePath) && IsFilePassingFilters(filePath, includeFiltersList, excludeFiltersList))
                        filesToCompile.Add(filePath);
                }
            }

            return filesToCompile;
        }

        /// <summary>
        /// Returns true if the given file path passes the include + exclude filters
        /// </summary>
        public bool IsFilePassingFilters(string filePath, List<DeployFilterRule> includeFiltersList, List<DeployFilterRule> excludeFiltersList) {
            bool passing = true;

            // test include filters
            if (includeFiltersList.Count > 0) {
                var hasMatch = includeFiltersList.Any(rule => filePath.RegexMatch(rule.RegexSourcePattern));
                passing = hasMatch;
            }

            // test exclude filters
            if (excludeFiltersList.Count > 0) {
                var hasNoMatch = excludeFiltersList.All(rule => !filePath.RegexMatch(rule.RegexSourcePattern));
                passing = passing && hasNoMatch;
            }

            return passing;
        }

        /// <summary>
        /// Creates a list of files to deploy after a compilation,
        /// for each Origin file will correspond one (or more if it's a .cls) .r file,
        /// and one .lst if the option has been checked
        /// </summary>
        public static List<FileToDeploy> GetFilesToDeployAfterCompilation(ProExecutionCompile execution) {

            var outputList = new List<FileToDeploy>();
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
                foreach (var deployNeeded in execution.ProEnv.Deployer.GetTargetsNeededForFile(compiledFile.SourcePath, 0)) {

                    string targetRPath;
                    if (execution.ProEnv.CompileLocally)
                        targetRPath = Path.Combine(deployNeeded.TargetPath, Path.GetFileName(compiledFile.CompOutputR));
                    else
                        targetRPath = Path.Combine(deployNeeded.TargetPath, compiledFile.CompOutputR.Replace(compiledFile.CompilationOutputDir, "").TrimStart('\\'));

                    // add .r and .lst (if needed) to the list of files to deploy
                    outputList.Add(deployNeeded.Set(compiledFile.SourcePath, compiledFile.CompOutputR, targetRPath));

                    // listing
                    if (execution.CompileWithListing && !string.IsNullOrEmpty(compiledFile.CompOutputLis)) {
                        outputList.Add(deployNeeded.Copy(compiledFile.SourcePath, compiledFile.CompOutputLis, Path.ChangeExtension(targetRPath, ProExecutionHandleCompilation.ExtLis)));
                    }

                    // xref
                    if (execution.CompileWithXref && !string.IsNullOrEmpty(compiledFile.CompOutputXrf)) {
                        outputList.Add(deployNeeded.Copy(compiledFile.SourcePath, compiledFile.CompOutputXrf, Path.ChangeExtension(targetRPath, execution.UseXmlXref ? ProExecutionHandleCompilation.ExtXrfXml : ProExecutionHandleCompilation.ExtXrf)));
                    }

                    // debug-list
                    if (execution.CompileWithDebugList && !string.IsNullOrEmpty(compiledFile.CompOutputDbg)) {
                        outputList.Add(deployNeeded.Copy(compiledFile.SourcePath, compiledFile.CompOutputDbg, Path.ChangeExtension(targetRPath, ProExecutionHandleCompilation.ExtDbg)));
                    }
                }
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
                ParallelOptions parallelOptions = new ParallelOptions {
                    CancellationToken = cancelToken.Token,
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };
               
                // make sure to transfer a given file only once at the same place (happens with .cls file since a source
                // can have several .r files generated if it is used in another classes)
                deployToDo = deployToDo
                    .GroupBy(deploy => deploy.To)
                    .Select(group => group.FirstOrDefault(move => Path.GetFileNameWithoutExtension(move.From ?? "").Equals(Path.GetFileNameWithoutExtension(move.Origin))) ?? group.First())
                    .ToList();

                _totalNbFilesToDeploy = deployToDo.Count;

                #region for archive types we do everything here

                // Create the list of each archive + each file in each archive
                // path of archive -> (ArchiveInfo, Dictionary<relativeArchivePath, sourcePath>, FileToDeploy)
                var archives = new Dictionary<string, Tuple<IArchiveInfo, Dictionary<string, string>, Dictionary<string, FileToDeployArchive>>>();
                deployToDo
                    .Where(deploy => deploy is FileToDeployArchive)
                    .Cast<FileToDeployArchive>()
                    .ToNonNullList()
                    .ForEach(deployArchive => {
                        if (deployArchive.IfFromFileExists()) {

                            // add new archive
                            if (!archives.ContainsKey(deployArchive.ArchivePath)) {
                                archives.Add(
                                    deployArchive.ArchivePath,
                                    new Tuple<IArchiveInfo, Dictionary<string, string>, Dictionary<string, FileToDeployArchive>>(deployArchive.NewArchive(this), new Dictionary<string, string>(), new Dictionary<string, FileToDeployArchive>())
                                );
                            }

                            // add new file in archive
                            if (!archives[deployArchive.ArchivePath].Item2.ContainsKey(deployArchive.From)) {
                                archives[deployArchive.ArchivePath].Item2.Add(deployArchive.RelativePathInArchive, deployArchive.From);
                                archives[deployArchive.ArchivePath].Item3.Add(deployArchive.RelativePathInArchive, deployArchive);
                            }
                        }
                    });

                // pack each archive
                foreach (var archive in archives) {
                    // canceled?
                    cancelToken.Token.ThrowIfCancellationRequested();
                    try {
                        var currentArchive = archive;
                        archive.Value.Item1.PackFileSet(archive.Value.Item2, CompressionLevel.None, (sender, args) => {
                            // canceled?
                            if (!args.CannotCancel)
                                cancelToken.Token.ThrowIfCancellationRequested();

                            if (args.ProgressType == ArchiveProgressType.FinishFile) {
                                // set the FileToDeploy state
                                if (currentArchive.Value.Item3.ContainsKey(args.CurrentFileName)) {
                                    if (args.TreatmentException != null) {
                                        currentArchive.Value.Item3[args.CurrentFileName].RegisterArchiveException(args.TreatmentException);
                                    } else {
                                        currentArchive.Value.Item3[args.CurrentFileName].IsOk = true;
                                    }
                                }

                                _nbFilesDeployed++;
                                if (updateDeploymentPercentage != null)
                                    updateDeploymentPercentage((float) _nbFilesDeployed / _totalNbFilesToDeploy * 100);
                            }
                        });
                    } catch (OperationCanceledException) {
                        throw;
                    } catch (Exception e) {
                        // set the deploy error for each file
                        foreach (var kpv in archive.Value.Item3) {
                            kpv.Value.RegisterArchiveException(e);
                        }
                    }
                }

                #endregion

                // do a deployment action for each file
                Parallel.ForEach(deployToDo.Where(deploy => !(deploy is FileToDeployArchive) && deploy.CanParallelizeDeploy), parallelOptions, file => {
                    // canceled?
                    parallelOptions.CancellationToken.ThrowIfCancellationRequested();

                    if (file.DeploySelf())
                        _nbFilesDeployed++;
                    if (updateDeploymentPercentage != null)
                        updateDeploymentPercentage((float) _nbFilesDeployed / _totalNbFilesToDeploy * 100);
                });

                // don't use parallel for the others
                foreach (var file in deployToDo.Where(deploy => !(deploy is FileToDeployArchive) && !deploy.CanParallelizeDeploy)) {
                    // canceled?
                    cancelToken.Token.ThrowIfCancellationRequested();

                    if (file.DeploySelf())
                        _nbFilesDeployed++;
                    if (updateDeploymentPercentage != null)
                        updateDeploymentPercentage((float) _nbFilesDeployed / _totalNbFilesToDeploy * 100);
                }

            } catch (OperationCanceledException) {
                // we expect this exception if the task has been canceled
            }

            return deployToDo;
        }

        #endregion

        #region private /utils

        /// <summary>
        /// Replace the variables &lt;XXX&gt; in the string
        /// </summary>
        private string ReplaceVariablesIn(string input) {
            if (input.ContainsFast("<")) {
                foreach (var variableRule in DeployVarList) {
                    input = input.Replace(variableRule.VariableName, variableRule.Path);
                }
            }
            return input;
        }

        private string GetRegexAndReplaceVariablesIn(string input) {
            input = ReplaceVariablesIn(input);
            return input.StartsWith(":") ? input.Remove(0, 1) : input.Replace('/', '\\').WildCardToRegex();
        }

        #endregion

        #endregion

    }

    #region DeployRule

    public abstract class DeployRule {
        /// <summary>
        /// Step to which the rule applies : 0 = compilation, 1 = deployment of all files, 2+ = extra
        /// </summary>
        public int Step { get; set; }

        /// <summary>
        /// This compilation path applies to a given application (can be empty)
        /// </summary>
        public string NameFilter { get; set; }

        /// <summary>
        /// This compilation path applies to a given Env letter (can be empty)
        /// </summary>
        public string SuffixFilter { get; set; }
    }
    
    public class DeployVariableRule : DeployRule {
        /// <summary>
        /// the name of the variable, format &lt;XXX&gt;
        /// </summary>
        public string VariableName { get; set; }

        /// <summary>
        /// The path that should replace the variable &lt;XXX&gt;
        /// </summary>
        public string Path { get; set; }
    }

    public class DeployFilterRule : DeployRule {
        /// <summary>
        /// true if the rule is about including a file (+) false if about excluding (-)
        /// </summary>
        public bool Include { get; set; }

        /// <summary>
        /// Pattern to match in the source path
        /// </summary>
        public string SourcePattern { get; set; }

        /// <summary>
        /// Pattern to match in the source (as a regular expression)
        /// </summary>
        public string RegexSourcePattern { get; set; }
    }

    #endregion

    #region DeployTransferRule

    public abstract class DeployTransferRule : DeployRule {

        #region Properties

        /// <summary>
        /// The type of transfer that should occur for this compilation path
        /// </summary>
        public virtual DeployType Type { get { return DeployType.Copy; } }

        /// <summary>
        /// if true, this should be the last rule applied to this file
        /// </summary>
        public bool ContinueAfterThisRule { get; set; }

        /// <summary>
        /// Pattern to match in the source path
        /// </summary>
        public string SourcePattern { get; set; }

        /// <summary>
        /// deploy target depending on the deploytype of this rule
        /// </summary>
        public string DeployTarget { get; set; }

        /// <summary>
        /// True if the rule is directly written as a regex and we want to replace matches in the source directory in the deploy target (in that case it must start with ":")
        /// </summary>
        public bool ShouldDeployTargetReplaceDollar { get; set; }

        /// <summary>
        /// The line from which we read this info, allows to sort by line
        /// </summary>
        public int Line { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Should return true if the rule is valid
        /// </summary>
        /// <param name="error"></param>
        public virtual bool IsValid(out string error) {
            error = null;
            if (!string.IsNullOrEmpty(SourcePattern) && !string.IsNullOrEmpty(DeployTarget)) {
                return true;
            }
            error = "Line " + Line + " : Source pattern or deploy target path null";
            return false;
        }

        #endregion

        #region Factory

        public static DeployTransferRule New(DeployType type) {
            switch (type) {
                case DeployType.Prolib:
                    return new DeployTransferRuleProlib();
                case DeployType.Cab:
                    return new DeployTransferRuleCab();
                case DeployType.Zip:
                    return new DeployTransferRuleZip();
                case DeployType.DeleteInProlib:
                    return new DeployTransferRuleProlib();
                case DeployType.Ftp:
                    return new DeployTransferRuleFtp();
                case DeployType.Delete:
                    return new DeployTransferRuleDelete();
                case DeployType.Copy:
                    return new DeployTransferRuleCopy();
                case DeployType.Move:
                    return new DeployTransferRuleMove();
                default:
                    throw new ArgumentOutOfRangeException("type", type, null);
            }
        }

        #endregion
    }

    #region DeployTransferRuleArchive

    public abstract class DeployTransferRuleArchive : DeployTransferRule {

        public virtual string ArchiveExt { get { return ".arc"; } }

        public override bool IsValid(out string error) {
            if (!DeployTarget.ContainsFast(ArchiveExt)) {
                error = "Line " + Line + " : The rule has an incorrect deployment target, a " + ArchiveExt + " should be found";
                return false;
            }
            return base.IsValid(out error);
        }
    }

    #region DeployTransferRuleProlib

    public class DeployTransferRuleProlib : DeployTransferRuleArchive {

        public override DeployType Type { get { return DeployType.Prolib; } }

        public override string ArchiveExt { get { return ".pl"; } }
    }

    #endregion

    #region DeployTransferRuleZip

    public class DeployTransferRuleZip : DeployTransferRuleArchive {

        public override DeployType Type { get { return DeployType.Zip; } }

        public override string ArchiveExt { get { return ".zip"; } }
    }

    #endregion

    #region DeployTransferRuleCab

    public class DeployTransferRuleCab : DeployTransferRuleArchive {

        public override DeployType Type { get { return DeployType.Cab; } }

        public override string ArchiveExt { get { return ".cab"; } }
    }

    #endregion

    #region DeployTransferRuleDeleteInProlib

    public class DeployTransferRuleDeleteInProlib : DeployTransferRuleArchive {

        public override DeployType Type { get { return DeployType.DeleteInProlib; } }

        public override string ArchiveExt { get { return ".pl"; } }
    }

    #endregion

    #endregion

    #region DeployTransferRuleFtp

    public class DeployTransferRuleFtp : DeployTransferRule {

        public override DeployType Type { get { return DeployType.Ftp; } }

        public override bool IsValid(out string error) {
            if (!DeployTarget.IsValidFtpAdress()) {
                error = "Line " + Line + " : The FTP rule has an incorrect deployment target, it should follow the pattern ftp://user:pass@server:port/distantpath/ (with user/pass/port being optional)";
                return false;
            }
            return base.IsValid(out error);
        }
    }

    #endregion

    #region DeployTransferRuleDelete

    public class DeployTransferRuleDelete : DeployTransferRule {
        public override DeployType Type { get { return DeployType.Delete; } }
    }

    #endregion

    #region DeployTransferRuleCopy

    public class DeployTransferRuleCopy : DeployTransferRule {
        public override DeployType Type { get { return DeployType.Copy; } }
    }

    #endregion

    #region DeployTransferRuleMove

    public class DeployTransferRuleMove : DeployTransferRule {
        public override DeployType Type { get { return DeployType.Move; } }
    }

    #endregion

    #endregion

    #region DeployType

    /// <summary>
    /// Types of deploy, used during rules sorting
    /// </summary>
    public enum DeployType {
        Prolib = 1,
        Zip = 2,
        Cab = 3,
        DeleteInProlib = 4,
        Archive = 10,

        Ftp = 15,
        Delete = 16,

        // Copy / move should always be last
        Copy = 30,
        Move = 31
    }

    #endregion


}