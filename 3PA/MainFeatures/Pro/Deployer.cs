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
using System.Threading.Tasks;
using _3PA.Data;
using _3PA.Lib;

namespace _3PA.MainFeatures.Pro {

    internal class Deployer {

        #region Static

        #region public event

        /// <summary>
        /// Called when the list of DeployTransfers is updated
        /// </summary>
        public static event Action OnDeployConfigurationUpdate;

        #endregion

        #region public fields

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

            var outputMessage = new StringBuilder();

            // get all the rules
            _fullDeployRulesList = new List<DeployRule>();
            Utils.ForEachLine(Config.FileDeploymentRules, new byte[0], (lineNb, lineString) => {
                var items = lineString.Split('\t');

                int step = 0;
                if (items.Length > 1 && !int.TryParse(items[0].Trim(), out step))
                    step = 0;

                // new transfer rule
                if (items.Length == 7) {

                    DeployType type;
                    if (!Enum.TryParse(items[3].Trim(), true, out type))
                        type = DeployType.Copy;

                    var obj = new DeployTransferRule {
                        Step = step, 
                        NameFilter = items[1].Trim(), 
                        SuffixFilter = items[2].Trim(), 
                        Type = type,
                        ContinueAfterThisRule = items[4].Trim().EqualsCi("yes") || items[4].Trim().EqualsCi("true"), 
                        Line = lineNb + 1,
                        SourcePattern = items[5].Trim(),
                        DeployTarget = items[6].Trim().Replace('/', '\\')
                    };
                    
                    obj.ShouldDeployTargetReplaceDollar = obj.DeployTarget.StartsWith(":");
                    if (obj.ShouldDeployTargetReplaceDollar)
                        obj.DeployTarget = obj.DeployTarget.Remove(0, 1);

                    if (obj.Type == DeployType.Ftp && !obj.DeployTarget.IsValidFtpAdress()) {
                        outputMessage.Append("- The FTP rule line n°" + obj.Line + " has an incorrect deployment target, it should follow the pattern ftp://user:pass@server:port/distantpath/ (with user/pass/port being optionnal)<br>");
                        return;
                    }

                    if (obj.Type == DeployType.Zip && !obj.DeployTarget.ContainsFast(".zip")) {
                        outputMessage.Append("- The ZIP rule line n°" + obj.Line + " has an incorrect deployment target, a .zip should be found<br>");
                        return;
                    }

                    if (obj.Type == DeployType.Prolib && !obj.DeployTarget.ContainsFast(".pl")) {
                        outputMessage.Append("- The Prolib rule line n°" + obj.Line + " has an incorrect deployment target, a .pl should be found<br>");
                        return;
                    }

                    if (!string.IsNullOrEmpty(obj.SourcePattern) && !string.IsNullOrEmpty(obj.DeployTarget))
                        _fullDeployRulesList.Add(obj);

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
                        _fullDeployRulesList.Add(obj);


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
                        _fullDeployRulesList.Add(obj);
                }
            }, Encoding.Default);

            if (outputMessage.Length > 0)
                UserCommunication.NotifyUnique("deployRulesErrors", "The following rules are incorrect :<br><br>" + outputMessage + "<br><br>Please correct them " + Config.FileDeploymentRules.ToHtmlLink("here"), MessageImg.MsgHighImportance, "Error(s) reading rules file", "Rules incorrect", args => {
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

        #endregion

        #region Object
        
        #region Fields

        /// <summary>
        /// IF YOU ADD A FIELD, DO NOT FORGET TO ALSO ADD THEM IN THE HARD COPY CONSTRUCTOR!!!
        /// </summary>
        
        private List<DeployRule> _deployRulesList;

        private List<DeployVariableRule> _deployVarList;


        /// <summary>
        /// Allows us to keep track of the opened zip needed for this deployment
        /// </summary>
        private Dictionary<string, ZipStorer> _openedZip = new Dictionary<string, ZipStorer>();

        /// <summary>
        /// Allows us to know which file to remove in which zip when they are not freshly created
        /// </summary>
        private Dictionary<string, HashSet<string>> _filesToRemoveFromZip = new Dictionary<string, HashSet<string>>();

        #endregion

        #region Life and death

        /// <summary>
        /// Constructor
        /// </summary>
        public Deployer(ProEnvironment.ProEnvironmentObject proEnv) {
            ProEnv = proEnv;

            // we need to filter/sort the list of computation path when it changes
            OnDeployConfigurationUpdate += () => _deployRulesList = null;
        }

        /// <summary>
        /// Hard copy
        /// </summary>
        public Deployer(ProEnvironment.ProEnvironmentObject proEnv, Deployer deployer) : this(proEnv) {
            _deployRulesList = deployer.DeployRules;
            _deployVarList = deployer._deployVarList;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The deployer works for a specific environment
        /// </summary>
        private ProEnvironment.ProEnvironmentObject ProEnv { get; set; }

        /// <summary>
        /// List of deployment rules filtered + sorted for this env
        /// </summary>
        public List<DeployRule> DeployRules {
            get {
                if (_deployRulesList == null) {

                    // Need to match the application name / suffix filter with the current env
                    _deployRulesList = GetFullDeployRulesList.Where(item => ProEnv.Name.RegexMatch(item.NameFilter.WildCardToRegex()) && ProEnv.Suffix.RegexMatch(item.SuffixFilter.WildCardToRegex())).ToNonNullList();

                    // sort the rules
                    _deployRulesList.Sort((item1, item2) => {

                        // exact name match first
                        int compare = item2.NameFilter.EqualsCi(ProEnv.Name).CompareTo(item1.NameFilter.EqualsCi(ProEnv.Name));
                        if (compare != 0)
                            return compare;

                        // longer name filter first
                        compare = item2.NameFilter.Length.CompareTo(item1.NameFilter.Length);
                        if (compare != 0)
                            return compare;

                        // exact suffix match first
                        compare = item2.SuffixFilter.EqualsCi(ProEnv.Suffix).CompareTo(item1.SuffixFilter.EqualsCi(ProEnv.Suffix));
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

                    _deployVarList = _deployRulesList.OfType<DeployVariableRule>().ToList();
                }
                return _deployRulesList;
            }
            set { _deployRulesList = value; }
        }

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
        /// This method returns the transfer directories for the given source path, for each :
        /// If CompileLocally, returns the directory of the source
        /// If the deployment dir is empty and we didn't match an absolute compilation path, returns the source directoy as well
        /// </summary>
        public List<FileToDeploy> GetTargetDirsNeededForFile(string sourcePath, int step) {

            // local compilation? return only one path, MOVE next to the source
            if (step == 0 && ProEnv.CompileLocally)
                return new List<FileToDeploy> { new FileToDeploy(Path.GetDirectoryName(sourcePath), DeployType.Move, true) };

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
                    outPath = Path.Combine(ProEnv.BaseCompilationPath, outPath);
                }

                if (!outList.Exists(needed => needed.TargetDir.EqualsCi(outPath)))
                    outList.Add(new FileToDeploy(outPath, rule.Type, !rule.ContinueAfterThisRule));

                // stop ?
                if (!rule.ContinueAfterThisRule)
                    break;
            }

            // nothing matched?
            if (outList.Count == 0) {

                // for the compilation, move to deployment directory
                if (step == 0)
                    outList.Add(new FileToDeploy(ProEnv.BaseCompilationPath, DeployType.Move, true));
            } else {
                var lastDeploy = outList.LastOrDefault();
                if (lastDeploy != null) {
                    // flag last deploy
                    lastDeploy.FinalDeploy = true;

                    // for the compilation step, if the last deploy is a copy, make it a move
                    if (step == 0 && lastDeploy.DeployType == DeployType.Copy)
                        lastDeploy.DeployType = DeployType.Move;
                }
            }

            return outList;
        }

        /// <summary>
        /// returns the list of transfers needed for a given file
        /// </summary>
        public List<FileToDeploy> GetTransfersNeededForFile(string file, int step) {
            var fileName = Path.GetFileName(file);
            if (fileName != null)
                return ProEnv.Deployer.GetTargetDirsNeededForFile(file, step).Select(deploy => deploy.Set(file, file, Path.Combine(deploy.TargetDir, fileName))).ToList();
            return new List<FileToDeploy>();
        }

        /// <summary>
        /// Returns a list of files in the given folders (recursively or not depending on the option),
        /// this list is filtered thanks to the rules given (also, for step == 0, only progress files are listed)
        /// </summary>
        public HashSet<string> GetFilesList(List<string> listOfFolderPath, SearchOption searchOptions, int step) {

            // constructs the list of all the files (unique) accross the different folders
            var filesToCompile = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

            // case of step 0 (compilation) we list only compilable files
            var fileExtensionFilter = step == 0 ? Config.Instance.CompilableFilesPattern : "*";

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

        #endregion

        #region Deploy Files

        /// <summary>
        /// Creates a list of files to deploy after a compilation,
        /// for each Origin file will correspond one (or more if it's a .cls) .r file,
        /// and one .lst if the option has been checked
        /// </summary>
        public List<FileToDeploy> DeployFilesForStep(int step, List<string> listOfSourceDir, SearchOption searchOptions, Action<float> updateDeploymentPercentage = null) {

            var outputList = new List<FileToDeploy>();

            // list the files to deploy
            foreach (var file in GetFilesList(listOfSourceDir, searchOptions, step)) {
                outputList.AddRange(GetTransfersNeededForFile(file, step));
            }

            // do deploy
            return DeployFiles(outputList, updateDeploymentPercentage);
        }

        /// <summary>
        /// Deploy a given list of files (can reduce the list if there are duplicated items so it returns it)
        /// </summary>
        public List<FileToDeploy> DeployFiles(List<FileToDeploy> deployToDo, Action<float> updateDeploymentPercentage = null) {

            int[] totalFile = { 0 };
            int[] nbFilesDone = { 0 };

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
                .ForEach(deploy => Utils.CreateDirectory(Path.GetDirectoryName(deploy.To)));

            #region for archives (zip/pl)

            // for archives, compute the path to the archive file (+ make sure the directory of the archive exists)
            deployToDo.Where(deploy => deploy.DeployType <= DeployType.Archive).ToNonNullList().ForEach(deploy => {
                var ext = deploy.DeployType == DeployType.Prolib ? ".pl" : ".zip";
                var pos = deploy.To.LastIndexOf(ext, StringComparison.CurrentCultureIgnoreCase);
                if (pos >= 0) {
                    var posEnd = pos + ext.Length;
                    deploy.ArchivePath = deploy.To.Substring(0, posEnd);
                    deploy.RelativePathInArchive = deploy.To.Substring(posEnd + 1);

                    // ensure that the folder to the .archive file exists
                    Utils.CreateDirectory(Path.GetDirectoryName(deploy.ArchivePath));

                    // for .zip, open the zip stream for later usage
                    if (deploy.DeployType > DeployType.Prolib) {
                        if (!_openedZip.ContainsKey(deploy.ArchivePath)) {
                            try {
                                if (!File.Exists(deploy.ArchivePath)) {
                                    _openedZip.Add(deploy.ArchivePath, ZipStorer.Create(deploy.ArchivePath, "Created with 3P @ " + DateTime.Now + "\r\n" + Config.UrlWebSite));
                                } else {
                                    _openedZip.Add(deploy.ArchivePath, ZipStorer.Open(deploy.ArchivePath, FileAccess.Write));
                                    _filesToRemoveFromZip.Add(deploy.ArchivePath, new HashSet<string>());
                                }
                            } catch (Exception e) {
                                ErrorHandler.ShowErrors(e, "Couldn't create/open the .zip file");
                            }

                        }

                        // we didn't create the zip? then we need to remove this file if it exists
                        if (_filesToRemoveFromZip.ContainsKey(deploy.ArchivePath)) 
                            _filesToRemoveFromZip[deploy.ArchivePath].Add(deploy.RelativePathInArchive.Replace('\\', '/'));
                    }
                }
            });

            #endregion
            
            #region for .pl deployments, we treat them before anything else

            // for PL, we need to MOVE each file into a temporary folder with the internal structure of the .pl file,
            // then move it back where it was for further deploys...

            var plDeployments = deployToDo
                .Where(deploy => deploy.DeployType == DeployType.Prolib)
                .ToNonNullList();

            if (plDeployments.Count > 0) {

                // then we create a unique temporary folder for each .pl
                var dicPlToTempFolder = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
                foreach (var pathPl in plDeployments.Where(deploy => !string.IsNullOrEmpty(deploy.ArchivePath)).Select(deploy => deploy.ArchivePath).Distinct()) {

                    // create a unique temp folder for this .pl
                    if (!dicPlToTempFolder.ContainsKey(pathPl)) {
                        var plDirPath = Path.GetDirectoryName(pathPl);
                        if (plDirPath != null) {
                            var uniqueTempFolder = Path.Combine(plDirPath, Path.GetFileName(pathPl) + "~" + Path.GetRandomFileName());
                            dicPlToTempFolder.Add(pathPl, uniqueTempFolder);
                            Utils.CreateDirectory(uniqueTempFolder, FileAttributes.Hidden);
                        }
                    }
                }

                var prolibMessage = new StringBuilder();

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
                                Utils.CreateDirectory(tempSubFolder);
                            }
                        }
                    }

                    var prolibExe = new ProcessIo(ProEnv.ProlibPath);

                    // for each subfolder in the .pl
                    foreach (var plSubFolder in dicTempFolderToPl) {

                        var onePlSubFolderDeployments = onePlDeployments
                            .Where(deploy => plSubFolder.Key.Equals(Path.GetDirectoryName(deploy.ToTemp)))
                            .ToNonNullList();
                        if (onePlSubFolderDeployments.Count == 0)
                            continue;

                        Parallel.ForEach(onePlSubFolderDeployments, deploy => {
                            if (File.Exists(deploy.From))
                                deploy.IsOk = !string.IsNullOrEmpty(deploy.ToTemp) && Utils.MoveFile(deploy.From, deploy.ToTemp);
                            if (deploy.IsOk)
                                nbFilesDone[0]++;
                            if (updateDeploymentPercentage != null)
                                updateDeploymentPercentage((float)nbFilesDone[0] / totalFile[0] * 100);
                        });

                        // now we just need to add the content of temp folders into the .pl
                        prolibExe.StartInfo.WorkingDirectory = plSubFolder.Value.Item1; // base temp dir
                        prolibExe.Arguments = plSubFolder.Value.Item3.ProQuoter() + " -create -nowarn -add " + Path.Combine(plSubFolder.Value.Item2, "*").ProQuoter();
                        if (!prolibExe.TryDoWait(true))
                            prolibMessage.Append(prolibExe.ErrorOutput);

                        Parallel.ForEach(onePlSubFolderDeployments, deploy => {
                            deploy.IsOk = deploy.IsOk && Utils.MoveFile(deploy.ToTemp, deploy.From);
                        });

                    }

                    // compress .pl
                    prolibExe.StartInfo.WorkingDirectory = Path.GetDirectoryName(pl.Key) ?? "";
                    prolibExe.Arguments = pl.Key.ProQuoter() + " -compress -nowarn";
                    if (!prolibExe.TryDoWait(true))
                        prolibMessage.Append(prolibExe.ErrorOutput);

                    // delete temp folders
                    Utils.DeleteDirectory(pl.Value, true);
                }

                if (prolibMessage.Length > 0)
                    UserCommunication.Notify("Errors occured when trying to create/add files to the .pl file :<br>" + prolibMessage, MessageImg.MsgError, "Prolib output", "Errors");
            }

            #endregion

            #region for zip

            // remove the files that are already in the zip file or they will appear twice when we add them
            foreach (var kpv in _filesToRemoveFromZip) {
                ZipStorer zip = _openedZip[kpv.Key];
                var filesToDelete = zip.ReadCentralDir().Where(zipFileEntry => kpv.Value.Contains(zipFileEntry.FilenameInZip)).ToList();
                _openedZip.Remove(kpv.Key);
                ZipStorer.RemoveEntries(ref zip, filesToDelete);
                _openedZip.Add(kpv.Key, zip);
            }

            #endregion


            // do a deployment action for each file (parallel for MOVE and COPY)
            Parallel.ForEach(deployToDo.Where(deploy => deploy.DeployType >= DeployType.Copy), file => {
                if (DeploySingleFile(file))
                    nbFilesDone[0]++;
                if (updateDeploymentPercentage != null)
                    updateDeploymentPercentage((float)nbFilesDone[0] / totalFile[0] * 100);
            });
            // don't use parallel for the other types
            foreach (var file in deployToDo.Where(deploy => deploy.DeployType < DeployType.Copy)) {
                if (DeploySingleFile(file))
                    nbFilesDone[0]++;
                if (updateDeploymentPercentage != null)
                    updateDeploymentPercentage((float) nbFilesDone[0]/totalFile[0]*100);
            }

            #region for zip, dispose of zipStorers

            // also, need to dispose of the object/stream here
            foreach (var zipStorer in _openedZip)
                zipStorer.Value.Close();
            _openedZip.Clear();

            #endregion
            
            return deployToDo;
        }

        /// <summary>
        /// Transfer a single file
        /// </summary>
        private bool DeploySingleFile(FileToDeploy file) {
            if (!file.IsOk) {
                if (File.Exists(file.From)) {
                    switch (file.DeployType) {

                        case DeployType.Copy:
                            file.IsOk = Utils.CopyFile(file.From, file.To);
                            break;

                        case DeployType.Move:
                            file.IsOk = Utils.MoveFile(file.From, file.To, true);
                            break;

                        case DeployType.Ftp:
                            file.IsOk = Utils.SendFileToFtp(file.From, file.To);
                            break;

                        case DeployType.Zip:
                            try {
                                ZipStorer zip = _openedZip[file.ArchivePath];
                                zip.AddFile(ZipStorer.Compression.Deflate, file.From, file.RelativePathInArchive, "Added @ " + DateTime.Now);
                                file.IsOk = true;
                            } catch (Exception e) {
                                ErrorHandler.ShowErrors(e, "Zipping during deployment");
                                file.IsOk = false;
                            }
                            break;

                    }
                }
                return true;
            }
            return false;
        }

        #endregion

        #region private /utils

        private string ReplaceVariablesIn(string input) {
            if (input.ContainsFast("<")) {
                foreach (var variableRule in _deployVarList) {
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

    public class DeployTransferRule : DeployRule {

        /// <summary>
        /// The type of transfer that should occur for this compilation path
        /// </summary>
        public DeployType Type { get; set; }

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

    #endregion

    #region DeployType

    public enum DeployType {
        Prolib = 1,
        Zip = 2,
        Archive = 10, // all types <= Archive have ArchivePath set after the deployment

        Ftp = 15,
        // Copy / move should always be last
        Copy = 20,
        Move = 21
    }

    #endregion

    #region TransferNeeded

    public class TransferNeeded {

        /// <summary>
        /// target directory for the deployment
        /// </summary>
        public string TargetDir { get; set; }

        /// <summary>
        /// Type de transfer
        /// </summary>
        public DeployType DeployType { get; set; }

        /// <summary>
        /// true if this is the last deploy action for the file
        /// </summary>
        public bool FinalDeploy { get; set; }

        public TransferNeeded(string targetDir, DeployType deployType, bool finalDeploy) {
            TargetDir = targetDir;
            DeployType = deployType;
            FinalDeploy = finalDeploy;
        }
    }

    #endregion

    #region FileToDeploy

    public class FileToDeploy {

        /// <summary>
        /// The path of input file that was originally compiled to trigger this move
        /// </summary>
        public string Origin { get; set; }

        public string From { get; set; }
        public string To { get; set; }

        /// <summary>
        /// true if the transfer went fine
        /// </summary>
        public bool IsOk { get; set; }

        /// <summary>
        /// target directory for the deployment
        /// </summary>
        public string TargetDir { get; set; }

        /// <summary>
        /// Type de transfer
        /// </summary>
        public DeployType DeployType { get; set; }

        /// <summary>
        /// true if this is the last deploy action for the file
        /// </summary>
        public bool FinalDeploy { get; set; }

        #region for .pl / .zip deploy type

        /// <summary>
        /// Temporary folder in which to copy the file before including it into a .pl
        /// </summary>
        public string ToTemp { get; set; }

        /// <summary>
        /// Path to the .pl or .zip file in which we need to include this file
        /// </summary>
        public string ArchivePath { get; set; }

        /// <summary>
        /// The relative path of the file within the archive
        /// </summary>
        public string RelativePathInArchive { get; set; }

        #endregion

        public FileToDeploy(string targetDir, DeployType deployType, bool finalDeploy) {
            TargetDir = targetDir;
            DeployType = deployType;
            FinalDeploy = finalDeploy;
        }

        public FileToDeploy Set(string origin, string @from, string to) {
            Origin = origin;
            From = @from;
            To = to;
            return this;
        }

        /// <summary>
        /// Returns a copy if this object, setting properties in the meantime
        /// </summary>
        public FileToDeploy Copy(string origin, string @from, string to) {
            return new FileToDeploy(TargetDir, DeployType, FinalDeploy) {
                Origin = origin,
                From = @from,
                To = to,
            };
        }
    }

    #endregion

}
