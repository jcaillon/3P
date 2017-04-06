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

        /// <summary>
        /// Allows us to keep track of the opened zip needed for this deployment
        /// </summary>
        private Dictionary<string, ZipStorer> _openedZip = new Dictionary<string, ZipStorer>();

        private string _envName;
        private string _envSuffix;
        private bool _compileLocally;
        private string _deploymentDirectory;
        ProcessIo _prolibExe;

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
            _prolibExe = new ProcessIo(proEnv.ProlibPath);

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

            Dictionary<string, string> dicPlToTempFolder = null;
            try {
                if (cancelToken == null) {
                    cancelToken = new CancellationTokenSource();
                }
                ParallelOptions parallelOptions = new ParallelOptions {
                    CancellationToken = cancelToken.Token,
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };
                
                int[] totalFile = {0};
                int[] nbFilesDone = {0};

                // make sure to transfer a given file only once at the same place (happens with .cls file since a source
                // can have several .r files generated if it is used in another classes)
                deployToDo = deployToDo
                    .GroupBy(deploy => deploy.To)
                    .Select(group => group.FirstOrDefault(move => Path.GetFileNameWithoutExtension(move.From ?? "").Equals(Path.GetFileNameWithoutExtension(move.Origin))) ?? group.First())
                    .ToList();

                totalFile[0] = deployToDo.Count;

                // check that every target dir exist (for copy/move deployments)
                deployToDo
                    .Where(deploy => deploy.DeployType == DeployType.Copy || deploy.DeployType == DeployType.Move)
                    .GroupBy(deploy => Path.GetDirectoryName(deploy.To))
                    .Select(group => group.First())
                    .ToNonNullList()
                    .ForEach(deploy => CreateDirectory(Path.GetDirectoryName(deploy.To), deploy));

                // Special preparation for archives, we compute the archive path and the relative path of the file in the archive
                var archiveDeployList = deployToDo
                    .Where(deploy => deploy is FileToDeployArchive)
                    .Cast<FileToDeployArchive>()
                    .ToNonNullList();
                archiveDeployList.ForEach(toArchiveFile => toArchiveFile.PreDeployment());

                // also make sure that the folder to the archive file exists
                archiveDeployList
                    .GroupBy(deploy => deploy.ArchivePath)
                    .Select(group => group.First())
                    .ToNonNullList()
                    .ForEach(deploy => CreateDirectory(Path.GetDirectoryName(deploy.ArchivePath), deploy));

                // canceled?
                cancelToken.Token.ThrowIfCancellationRequested();

                #region for .zip we do everything here

                var zipDeployments = archiveDeployList
                    .Where(deploy => deploy is FileToDeployZip)
                    .Cast<FileToDeployZip>()
                    .ToNonNullList();

                if (zipDeployments.Count > 0) {

                    // if we add a file in a zip and said file already exists in the zip, then it will appear twice!
                    // so we remove any existing file before adding the new ones
                    var filesToRemoveFromZip = new Dictionary<string, HashSet<string>>();

                    foreach (var file in zipDeployments) {
                        // for .zip, open the zip stream
                        if (!_openedZip.ContainsKey(file.ArchivePath)) {
                            try {
                                if (!File.Exists(file.ArchivePath)) {
                                    _openedZip.Add(file.ArchivePath, ZipStorer.Create(file.ArchivePath, "Created with 3P @ " + DateTime.Now + "\r\n" + Config.UrlWebSite));
                                } else {
                                    _openedZip.Add(file.ArchivePath, ZipStorer.Open(file.ArchivePath, FileAccess.Write));
                                    filesToRemoveFromZip.Add(file.ArchivePath, new HashSet<string>());
                                }
                            } catch (Exception e) {
                                ErrorHandler.ShowErrors(e, "Couldn't create/open the .zip file");
                            }
                        }

                        // we didn't create the zip? then we need to remove this file if it exists
                        if (filesToRemoveFromZip.ContainsKey(file.ArchivePath))
                            filesToRemoveFromZip[file.ArchivePath].Add(file.RelativePathInArchive.Replace('\\', '/'));
                    }
                    
                    // if we add a file in a zip that already exists in said zip, it will appear twice, here we delete existing file
                    // that will be replaced during the deployment
                    // remove the files that are already in the zip file or they will appear twice when we add them
                    foreach (var kpv in filesToRemoveFromZip) {
                        ZipStorer zip = _openedZip[kpv.Key];
                        var filesToDelete = zip.ReadCentralDir().Where(zipFileEntry => kpv.Value.Contains(zipFileEntry.FilenameInZip)).ToList();
                        _openedZip.Remove(kpv.Key);
                        ZipStorer.RemoveEntries(ref zip, filesToDelete);
                        _openedZip.Add(kpv.Key, zip);
                    }

                    foreach (var file in zipDeployments) {
                        // canceled?
                        cancelToken.Token.ThrowIfCancellationRequested();

                        if (file.DeploySelf(_openedZip[file.ArchivePath]))
                            nbFilesDone[0]++;
                        if (updateDeploymentPercentage != null)
                        updateDeploymentPercentage((float)nbFilesDone[0] / totalFile[0] * 100);
                    }
                }

                #endregion

                // canceled?
                cancelToken.Token.ThrowIfCancellationRequested();

                #region for .pl deployments, we treat them before anything else

                // for PL, we need to MOVE each file into a temporary folder with the internal structure of the .pl file,
                // then move it back where it was for further deploys...

                var plDeployments = archiveDeployList
                    .Where(deploy => deploy is FileToDeployProlib)
                    .Cast<FileToDeployProlib>()
                    .ToNonNullList();

                if (plDeployments.Count > 0) {

                    // then we create a unique temporary folder for each .pl
                    dicPlToTempFolder = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
                    foreach (var pathPl in plDeployments.Where(deploy => !string.IsNullOrEmpty(deploy.ArchivePath)).Select(deploy => deploy.ArchivePath).Distinct()) {
                        // create a unique temp folder for this .pl
                        if (!dicPlToTempFolder.ContainsKey(pathPl)) {
                            var plDirPath = Path.GetDirectoryName(pathPl);
                            if (plDirPath != null) {
                                var uniqueTempFolder = Path.Combine(plDirPath, Path.GetFileName(pathPl) + "~" + Path.GetRandomFileName());
                                dicPlToTempFolder.Add(pathPl, uniqueTempFolder);
                                CreateDirectory(uniqueTempFolder, plDeployments.Find(deploy => !string.IsNullOrEmpty(deploy.ArchivePath) && deploy.ArchivePath.Equals(pathPl)), FileAttributes.Hidden);
                            }
                        }
                    }

                    // for each .pl that needs to be created...
                    foreach (var pl in dicPlToTempFolder) {
                        var pl1 = pl;
                        var onePlDeployments = plDeployments
                            .Where(deploy => !string.IsNullOrEmpty(deploy.ArchivePath) && deploy.ArchivePath.Equals(pl1.Key))
                            .ToNonNullList();
                        if (onePlDeployments.Count == 0)
                            continue;

                        //  we set the temporary folder on which each file will be copied..
                        // Tuple : <(base) temp directory, relative path in pl, path to .pl>
                        var dicTempFolderToPl = new Dictionary<string, Tuple<string, string, string>>(StringComparer.CurrentCultureIgnoreCase);
                        foreach (var fileToDeploy in onePlDeployments) {
                            if (string.IsNullOrEmpty(fileToDeploy.ArchivePath))
                                continue;

                            if (dicPlToTempFolder.ContainsKey(fileToDeploy.ArchivePath)) {
                                fileToDeploy.ToTemp = Path.Combine(
                                    dicPlToTempFolder[fileToDeploy.ArchivePath],
                                    fileToDeploy.To.Replace(fileToDeploy.ArchivePath, "").TrimStart('\\')
                                );

                                // If not already done, remember that the *.r code in this temp folder must be integrated to this .pl file
                                var tempSubFolder = Path.GetDirectoryName(fileToDeploy.ToTemp);
                                if (!string.IsNullOrEmpty(tempSubFolder) && !dicTempFolderToPl.ContainsKey(tempSubFolder)) {
                                    dicTempFolderToPl.Add(
                                        tempSubFolder,
                                        new Tuple<string, string, string>(
                                            dicPlToTempFolder[fileToDeploy.ArchivePath], // path of the temp dir
                                            Path.GetDirectoryName(fileToDeploy.To.Replace(fileToDeploy.ArchivePath, "").TrimStart('\\')), // relative path in .pl
                                            fileToDeploy.ArchivePath) // path to the .pl file
                                    );

                                    // also, create the folder
                                    CreateDirectory(tempSubFolder, fileToDeploy);
                                }
                            }
                        }
                        
                        // for each subfolder in the .pl
                        foreach (var plSubFolder in dicTempFolderToPl) {
                            var onePlSubFolderDeployments = onePlDeployments
                                .Where(deploy => plSubFolder.Key.Equals(Path.GetDirectoryName(deploy.ToTemp)))
                                .ToNonNullList();
                            if (onePlSubFolderDeployments.Count == 0)
                                continue;

                            // move files to the temp subfolder
                            Parallel.ForEach(onePlSubFolderDeployments, parallelOptions, deploy => {
                                // canceled?
                                parallelOptions.CancellationToken.ThrowIfCancellationRequested();

                                deploy.IsOk = MoveFile(deploy.From, deploy.ToTemp, deploy);
                                if (updateDeploymentPercentage != null)
                                    updateDeploymentPercentage((float) nbFilesDone[0] / totalFile[0] * 100);
                            });

                            // now we just need to add the content of temp folders into the .pl
                            _prolibExe.StartInfo.WorkingDirectory = plSubFolder.Value.Item1; // base temp dir
                            _prolibExe.Arguments = plSubFolder.Value.Item3.ProQuoter() + " -create -nowarn -add " + Path.Combine(plSubFolder.Value.Item2, "*").ProQuoter();
                            var prolibOk = _prolibExe.TryDoWait(true);

                            // move files from the temp subfolder
                            Parallel.ForEach(onePlSubFolderDeployments, parallelOptions, deploy => {
                                // canceled?
                                parallelOptions.CancellationToken.ThrowIfCancellationRequested();

                                deploy.IsOk = deploy.IsOk && MoveFile(deploy.ToTemp, deploy.From, deploy);
                                if (!prolibOk) {
                                    deploy.DeployError = _prolibExe.ErrorOutput.ToString();
                                    deploy.IsOk = false;
                                }
                                if (deploy.IsOk)
                                    nbFilesDone[0]++;
                            });
                        }

                        // compress .pl
                        _prolibExe.StartInfo.WorkingDirectory = Path.GetDirectoryName(pl.Key) ?? "";
                        _prolibExe.Arguments = pl.Key.ProQuoter() + " -compress -nowarn";
                        _prolibExe.TryDoWait(true);
                    }
                }

                #endregion

                // do a deployment action for each file
                Parallel.ForEach(deployToDo.Where(deploy => deploy.CanParallelizeDeploy && !deploy.IsOk), parallelOptions, file => {
                    // canceled?
                    parallelOptions.CancellationToken.ThrowIfCancellationRequested();

                    if (file.DeploySelf())
                        nbFilesDone[0]++;
                    if (updateDeploymentPercentage != null)
                        updateDeploymentPercentage((float) nbFilesDone[0] / totalFile[0] * 100);
                });

                // don't use parallel for the others
                foreach (var file in deployToDo.Where(deploy => !deploy.CanParallelizeDeploy && !deploy.IsOk)) {
                    // canceled?
                    cancelToken.Token.ThrowIfCancellationRequested();

                    if (file.DeploySelf())
                        nbFilesDone[0]++;
                    if (updateDeploymentPercentage != null)
                        updateDeploymentPercentage((float) nbFilesDone[0] / totalFile[0] * 100);
                }

            } catch (OperationCanceledException) {
                // we expect this exception if the task has been canceled
            } finally {

                #region dispose .zip resources

                // need to dispose of the object/stream here
                foreach (var zipStorer in _openedZip)
                    zipStorer.Value.Close();
                _openedZip.Clear();

                #endregion

                #region Delete temp directories for .pl

                if (dicPlToTempFolder != null) {
                    // for each .pl that needs to be created...
                    foreach (var pl in dicPlToTempFolder) {
                        // delete temp folder
                        DeleteDirectory(pl.Value, null, true);
                    }
                }

                #endregion
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
        
        /// <summary>
        /// Creates the directory, can apply attributes
        /// </summary>
        private bool CreateDirectory(string path, FileToDeploy file, FileAttributes attributes = FileAttributes.Directory) {
            try {
                if (Directory.Exists(path))
                    return true;
                var dirInfo = Directory.CreateDirectory(path);
                dirInfo.Attributes |= attributes;
            } catch (Exception e) {
                if (file != null)
                    file.DeployError = "Couldn't create directory " + path.ProQuoter() + " : \"" + e.Message + "\"";
                return false;
            }
            return true;
        }

        /// <summary>
        /// Delete a dir, recursively
        /// </summary>
        private bool DeleteDirectory(string path, FileToDeploy file, bool recursive) {
            try {
                if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                    return true;
                Directory.Delete(path, recursive);
            } catch (Exception e) {
                if (file != null)
                    file.DeployError = "Couldn't delete the directory " + path.ProQuoter() + " : \"" + e.Message + "\"";
                return false;
            }
            return true;
        }

        /// <summary>
        /// Move a file, return true if ok, false otherwise
        /// </summary>
        private bool MoveFile(string sourceFile, string targetFile, FileToDeploy file) {
            try {
                if (sourceFile.Equals(targetFile))
                    return true;
                if (!File.Exists(sourceFile)) {
                    file.DeployError = "The source file " + sourceFile.ProQuoter() + " doesn't exist";
                    return false;
                }
                File.Delete(targetFile);
                File.Move(sourceFile, targetFile);
            } catch (Exception e) {
                file.DeployError = "Couldn't move " + sourceFile.ProQuoter() + " to  " + targetFile.ProQuoter() + " : \"" + e.Message + "\"";
                return false;
            }
            return true;
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
                case DeployType.Archive:
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

    #region DeployTransferRuleProlib

    public class DeployTransferRuleProlib : DeployTransferRule {

        public override DeployType Type { get { return DeployType.Prolib; } }

        public override bool IsValid(out string error) {
            if (!DeployTarget.ContainsFast(".pl")) {
                error = "Line " + Line + " : The PROLIB rule has an incorrect deployment target, a .pl should be found";
                return false;
            }
            return base.IsValid(out error);
        }
    }

    #endregion

    #region DeployTransferRuleZip

    public class DeployTransferRuleZip : DeployTransferRule {

        public override DeployType Type { get { return DeployType.Zip; } }

        public override bool IsValid(out string error) {
            if (!DeployTarget.ContainsFast(".zip")) {
                error = "Line " + Line + " : The ZIP rule has an incorrect deployment target, a .zip should be found";
                return false;
            }
            return base.IsValid(out error);
        }
    }

    #endregion

    #region DeployTransferRuleDeleteInProlib

    public class DeployTransferRuleDeleteInProlib : DeployTransferRule {

        public override DeployType Type { get { return DeployType.DeleteInProlib; } }
    }

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
        DeleteInProlib = 3,
        Archive = 10, // all types <= Archive have ArchivePath set after the deployment

        Ftp = 15,
        Delete = 16,

        // Copy / move should always be last
        Copy = 30,
        Move = 31
    }

    #endregion

    #region FileToDeploy

    /// <summary>
    /// Represents a file that needs to be deployed
    /// </summary>
    public class FileToDeploy {

        #region Properties

        /// <summary>
        /// The path of input file that was originally compiled to trigger this move
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// Need to move this file FROM this path
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Need to move this file TO this path
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// true if the transfer went fine
        /// </summary>
        public bool IsOk { get; set; }

        /// <summary>
        /// target directory for the deployment
        /// </summary>
        public string TargetPath { get; set; }

        /// <summary>
        /// Type of transfer
        /// </summary>
        public virtual DeployType DeployType { get { return DeployType.Copy; } }

        /// <summary>
        /// Null if no errors, otherwise it contains the description of the error that occurred for this file
        /// </summary>
        public string DeployError { get; set; }

        /// <summary>
        /// Return true if the deployment type can be parallelized
        /// </summary>
        public virtual bool CanParallelizeDeploy { get { return false; } }

        /// <summary>
        /// Directory name of To or path of the archive file
        /// </summary>
        public virtual string DestinationBasePath { get { return Path.GetDirectoryName(To); } }

        #endregion

        #region Life and death

        public FileToDeploy(string targetPath) {
            TargetPath = targetPath;
        }

        #endregion

        #region Methods

        public FileToDeploy Set(string origin, string from, string to) {
            Origin = origin;
            From = from;
            To = to;
            return this;
        }

        /// <summary>
        /// Returns a copy if this object, setting properties in the meantime
        /// </summary>
        public FileToDeploy Copy(string origin, string from, string to) {
            return new FileToDeploy(TargetPath) {
                Origin = origin,
                From = from,
                To = to
            };
        }

        /// <summary>
        /// Deploy this file
        /// </summary>
        public virtual bool DeploySelf() {
            if (IsOk)
                return false;
            IsOk = TryDeploy();
            return IsOk;
        }

        /// <summary>
        /// Deploy this file
        /// </summary>
        public virtual bool TryDeploy() {
            return true;
        }

        #endregion

        #region Factory

        public static FileToDeploy New(DeployType deployType, string targetPath) {
            switch (deployType) {
                case DeployType.Prolib:
                    return new FileToDeployProlib(targetPath);
                case DeployType.Zip:
                case DeployType.Archive:
                    return new FileToDeployZip(targetPath);
                case DeployType.DeleteInProlib:
                    return new FileToDeployDeleteInProlib(targetPath);
                case DeployType.Ftp:
                    return new FileToDeployFtp(targetPath);
                case DeployType.Delete:
                    return new FileToDeployDelete(targetPath);
                case DeployType.Copy:
                    return new FileToDeployCopy(targetPath);
                case DeployType.Move:
                    return new FileToDeployMove(targetPath);
                default:
                    throw new ArgumentOutOfRangeException("deployType", deployType, null);
            }
        }

        #endregion
    }

    #endregion

    #region FileToDeployArchive

    internal abstract class FileToDeployArchive : FileToDeploy {

        /// <summary>
        /// Path to the .pl or .zip file in which we need to include this file
        /// </summary>
        public string ArchivePath { get; set; }

        /// <summary>
        /// The relative path of the file within the archive
        /// </summary>
        public string RelativePathInArchive { get; set; }

        /// <summary>
        /// Path to the archive file
        /// </summary>
        public override string DestinationBasePath { get { return ArchivePath ?? To; } }

        public virtual string ArchiveExt { get { return ".zip"; } }

        protected FileToDeployArchive(string targetPath) : base(targetPath) {}

        public virtual void PreDeployment() {
            var pos = To.LastIndexOf(ArchiveExt, StringComparison.CurrentCultureIgnoreCase);
            if (pos >= 0) {
                var posEnd = pos + ArchiveExt.Length;
                ArchivePath = To.Substring(0, posEnd);
                RelativePathInArchive = To.Substring(posEnd + 1);
            }
        }

    }

    #endregion

    #region FileToDeployProlib

    internal class FileToDeployProlib : FileToDeployArchive {


        /// <summary>
        /// Temporary folder in which to copy the file before including it into a .pl
        /// </summary>
        public string ToTemp { get; set; }

        /// <summary>
        /// Type of transfer
        /// </summary>
        public override DeployType DeployType { get { return DeployType.Prolib; } }

        public override string ArchiveExt { get { return ".pl"; } }

        public FileToDeployProlib(string targetPath) : base(targetPath) {}
    }

    #endregion

    #region FileToDeployZip

    internal class FileToDeployZip : FileToDeployArchive {

        /// <summary>
        /// Type of transfer
        /// </summary>
        public override DeployType DeployType { get { return DeployType.Zip; } }

        public override string ArchiveExt { get { return ".zip"; } }

        public FileToDeployZip(string targetPath) : base(targetPath) {}

        public bool DeploySelf(ZipStorer zip) {
            try {
                zip.AddFile(ZipStorer.Compression.Deflate, From, RelativePathInArchive, "Added @ " + DateTime.Now);
                IsOk = true;
            } catch (Exception e) {
                DeployError = "Couldn't zip " + From.ProQuoter() + " to " + ArchivePath.ProQuoter() + " : \"" + e.Message + "\"";
                return false;
            }
            return true;
        }
        
    }

    #endregion

    #region FileToDeployDeleteInProlib

    internal class FileToDeployDeleteInProlib : FileToDeployProlib {

        /// <summary>
        /// Type of transfer
        /// </summary>
        public override DeployType DeployType { get { return DeployType.DeleteInProlib; } }

        public FileToDeployDeleteInProlib(string targetPath) : base(targetPath) {}
    }

    #endregion

    #region FileToDeployDelete

    internal class FileToDeployDelete : FileToDeploy {

        /// <summary>
        /// Type of transfer
        /// </summary>
        public override DeployType DeployType { get { return DeployType.Delete; } }

        public FileToDeployDelete(string targetPath) : base(targetPath) { }

        public override bool TryDeploy() {
            try {
                if (string.IsNullOrEmpty(To) || !File.Exists(To))
                    return true;
                File.Delete(To);
            } catch (Exception e) {
                DeployError = "Couldn't delete " + To.ProQuoter() + " : \"" + e.Message + "\"";
                return false;
            }
            return true;
        }

    }

    #endregion

    #region FileToDeployCopy

    internal class FileToDeployCopy : FileToDeploy {

        /// <summary>
        /// Return true if the deployment type can be parallelized
        /// </summary>
        public override bool CanParallelizeDeploy { get { return true; } }

        /// <summary>
        /// This can be set to true for a file deployed during step 0 (compilation), if the last 
        /// deployment is a Copy, we make it a Move because this allows us to directly compile were
        /// we need to finally move it instead of compiling then copying...
        /// </summary>
        public bool FinalDeploy { get; set; }

        /// <summary>
        /// Type of transfer
        /// </summary>
        public override DeployType DeployType { get { return FinalDeploy ? DeployType.Move : DeployType.Copy; } }

        public FileToDeployCopy(string targetPath) : base(targetPath) {}

        public override bool TryDeploy() {
            try {
                if (From.Equals(To))
                    return true;
                if (!File.Exists(From)) {
                    DeployError = "The source file " + From.ProQuoter() + " doesn't exist";
                    return false;
                }
                File.Delete(To);
                File.Copy(From, To);
            } catch (Exception e) {
                DeployError = "Couldn't copy " + From.ProQuoter() + " to  " + To.ProQuoter() + " : \"" + e.Message + "\"";
                return false;
            }
            return true;
        }
    }

    #endregion

    #region FileToDeployMove

    internal class FileToDeployMove : FileToDeploy {

        /// <summary>
        /// Return true if the deployment type can be parallelized
        /// </summary>
        public override bool CanParallelizeDeploy { get { return true; } }

        /// <summary>
        /// Type of transfer
        /// </summary>
        public override DeployType DeployType { get { return DeployType.Move; } }

        public FileToDeployMove(string targetPath) : base(targetPath) {}
        
        public override bool TryDeploy() {
            try {
                if (From.Equals(To))
                    return true;
                if (!File.Exists(From)) {
                    DeployError = "The source file " + From.ProQuoter() + " doesn't exist";
                    return false;
                }
                File.Delete(To);
                File.Move(From, To);
            } catch (Exception e) {
                DeployError = "Couldn't move " + From.ProQuoter() + " to  " + To.ProQuoter() + " : \"" + e.Message + "\"";
                return false;
            }
            return true;
        }
    }

    #endregion

    #region FileToDeployFtp

    internal class FileToDeployFtp : FileToDeploy {

        /// <summary>
        /// Type of transfer
        /// </summary>
        public override DeployType DeployType { get { return DeployType.Ftp; } }

        /// <summary>
        /// Path to the archive file
        /// </summary>
        public override string DestinationBasePath { get { return _serverUri ?? To; } }

        private string _serverUri;

        public FileToDeployFtp(string targetPath) : base(targetPath) { }

        /// <summary>
        /// Sends a file to a ftp(s) server : EASY MODE, connects, create the directories...
        /// Utils.SendFileToFtp(@"D:\Profiles\jcaillon\Downloads\function_forward_sample.p", "ftp://cnaf049:sopra100@rs28.lyon.fr.sopra/cnaf/users/cnaf049/vm/jca/derp/yolo/test.p");
        /// </summary>
        public override bool TryDeploy() {
            if (string.IsNullOrEmpty(From) || !File.Exists(From)) {
                DeployError = "The source file " + From.ProQuoter() + " doesn't exist";
                return false;
            }
            try {
                // parse our uri
                var regex = new Regex(@"^(ftps?:\/\/([^:\/@]*)?(:[^:\/@]*)?(@[^:\/@]*)?(:[^:\/@]*)?)(\/.*)$");
                var match = regex.Match(To.Replace("\\", "/"));
                if (!match.Success) {
                    DeployError = "Invalid URI for the targeted FTP : " + To.ProQuoter();
                    return false;
                }

                _serverUri = match.Groups[1].Value;
                var distantPath = match.Groups[6].Value;
                string userName = null;
                string passWord = null;
                string server;
                int port;
                if (!string.IsNullOrEmpty(match.Groups[4].Value)) {
                    userName = match.Groups[2].Value;
                    passWord = match.Groups[3].Value.Trim(':');
                    server = match.Groups[4].Value.Trim('@');
                    if (!int.TryParse(match.Groups[5].Value.Trim(':'), out port))
                        port = -1;
                } else {
                    server = match.Groups[2].Value;
                    if (!int.TryParse(match.Groups[3].Value.Trim(':'), out port))
                        port = -1;
                }

                FtpsClient ftp;
                if (!_ftpClients.ContainsKey(_serverUri))
                    _ftpClients.Add(_serverUri, new FtpsClient());
                ftp = _ftpClients[_serverUri];

                // try to connect!
                if (!ftp.Connected) {
                    if (!ConnectFtp(ftp, userName, passWord, server, port))
                        return false;
                }

                // dispose of the ftp on shutdown
                Plug.OnShutDown += DisconnectFtp;

                try {
                    ftp.PutFile(From, distantPath);
                } catch (Exception) {

                    // might be disconnected??
                    try {
                        ftp.GetCurrentDirectory();
                    } catch (Exception) {
                        if (!ConnectFtp(ftp, userName, passWord, server, port))
                            return false;
                    }

                    // try to create the directory and then push the file again
                    ftp.MakeDir((Path.GetDirectoryName(distantPath) ?? "").Replace('\\', '/'), true);
                    ftp.PutFile(From, distantPath);
                }
            } catch (Exception e) {
                DeployError = "Error sending " + From.ProQuoter() + " to FTP " + To.ProQuoter() + " : \"" + e.Message + "\"";
                return false;
            }

            return true;
        }

        private bool ConnectFtp(FtpsClient ftp, string userName, string passWord, string server, int port) {
            NetworkCredential credential = null;
            if (!string.IsNullOrEmpty(userName))
                credential = new NetworkCredential(userName, passWord);

            var modes = new List<EsslSupportMode>();
            typeof(EsslSupportMode).ForEach<EsslSupportMode>((s, l) => { modes.Add((EsslSupportMode)l); });

            ftp.DataConnectionMode = EDataConnectionMode.Passive;
            while (!ftp.Connected && ftp.DataConnectionMode == EDataConnectionMode.Passive) {
                foreach (var mode in modes.OrderByDescending(mode => mode)) {
                    try {
                        var curPort = port > -1 ? port : ((mode & EsslSupportMode.Implicit) == EsslSupportMode.Implicit ? 990 : 21);
                        ftp.Connect(server, curPort, credential, mode, 1800);
                        ftp.Connected = true;
                        break;
                    } catch (Exception) {
                        //ignored
                    }
                }
                ftp.DataConnectionMode = EDataConnectionMode.Active;
            }

            // failed?
            if (!ftp.Connected) {
                DeployError = "Failed to connect to a FTP server with : " + string.Format(@"Username : {0}, Password : {1}, Host : {2}, Port : {3}", userName ?? "none", passWord ?? "none", server, port == -1 ? 21 : port);
                return false;
            }

            return true;
        }

        #region Static for FTP

        private static Dictionary<string, FtpsClient> _ftpClients = new Dictionary<string, FtpsClient>();

        private static void DisconnectFtp() {
            foreach (var ftpsClient in _ftpClients) {
                ftpsClient.Value.Close();
            }
            _ftpClients.Clear();
        }

        #endregion

    }

    #endregion

}