#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProExecutionHandleCompilation.cs) is part of 3P.
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
using System.Text;
using System.Threading.Tasks;
using Yamui.Framework.Helper;
using _3PA.Lib;
using _3PA.MainFeatures.Appli;
using _3PA.MainFeatures.ModificationsTag;
using _3PA.NppCore;
using _3PA._Resource;

namespace _3PA.MainFeatures.Pro.Deploy {

    #region ProExecutionHandleCompilation

    internal abstract class ProExecutionHandleCompilation : ProExecution {

        #region Static events

        /// <summary>
        /// The action to execute at the end of the compilation if it went well
        /// - the list of all the files that needed to be compiled,
        /// - the errors for each file compiled (if any)
        /// - the list of all the deployments needed for the files compiled (move the .r but also .dbg and so on...)
        /// </summary>
        public static event Action<ProExecutionHandleCompilation, List<FileToCompile>, List<FileToDeploy>> OnEachCompilationOk;

        #endregion        

        #region Events

        /// <summary>
        /// The action to execute at the end of the compilation if it went well. It sends :
        /// - the list of all the files that needed to be compiled,
        /// - the errors for each file compiled (if any)
        /// - the list of all the deployments needed for the files compiled (move the .r but also .dbg and so on...)
        /// </summary>
        public event Action<ProExecutionHandleCompilation, List<FileToCompile>, List<FileToDeploy>> OnCompilationOk;

        #endregion

        #region Options

        /// <summary>
        /// List of the files to compile / run / prolint
        /// </summary>
        public List<FileToCompile> Files { get; set; }

        /// <summary>
        /// If true, don't actually do anything, just test it
        /// </summary>
        public bool IsTestMode { get; set; }

        /// <summary>
        /// Compile with DEBUG-LIST option
        /// </summary>
        public bool CompileWithDebugList { get; set; }

        /// <summary>
        /// Compile with LISTING option
        /// </summary>
        public bool CompileWithListing { get; set; }

        /// <summary>
        /// Compile with XREF option
        /// </summary>
        public bool CompileWithXref { get; set; }

        /// <summary>
        /// Must generate the xref in xml format instead of normal txt
        /// </summary>
        public bool UseXmlXref { get; set; }

        /// <summary>
        /// When true, we activate the log just before compiling with FileId active + we generate a file that list referenced table in the .r
        /// </summary>
        public bool IsAnalysisMode { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Temp directory located in the deployment dir
        /// </summary>
        public string DistantTempDir { get; private set; }

        /// <summary>
        /// In analysis mode, allows to affect the crc to each used table
        /// </summary>
        public List<TableCrc> TablesCrcs { get; set; }

        #endregion

        #region Constants

        public const string ExtR = ".r";
        public const string ExtDbg = ".dbg";
        public const string ExtLis = ".lis";
        public const string ExtXrf = ".xrf";
        public const string ExtXrfXml = ".xrf.xml";
        public const string ExtCls = ".cls";
        public const string ExtFileIdLog = ".fileidlog";
        public const string ExtRefTables = ".reftables";

        #endregion

        #region private fields

        /// <summary>
        /// Path to the file containing the COMPILE output
        /// </summary>
        protected string _compilationLog;

        /// <summary>
        /// Path to a file used to determine the progression of a compilation (useful when compiling multiple programs)
        /// 1 byte = 1 file treated
        /// </summary>
        private string _progressionFilePath;

        #endregion

        #region constructors and destructor

        /// <summary>
        /// Construct with the current env
        /// </summary>
        public ProExecutionHandleCompilation() : this(null) { }

        public ProExecutionHandleCompilation(ProEnvironment.ProEnvironmentObject proEnv) : base(proEnv) {
            // set some options
            CompileWithDebugList = Config.Instance.CompileWithDebugList;
            CompileWithListing = Config.Instance.CompileWithListing;
            CompileWithXref = Config.Instance.CompileWithXref;
            UseXmlXref = Config.Instance.CompileUseXmlXref;
            IsAnalysisMode = false;

            DistantTempDir = Path.Combine(ProEnv.BaseCompilationPath, "~3p-tmp-" + DateTime.Now.ToString("HHmmss") + "-" + Path.GetRandomFileName());
        }

        #endregion

        #region Override

        /// <summary>
        /// Allows to clean the temporary directories
        /// </summary>
        public override void Clean() {
            try {
                // delete temp dir
                Utils.DeleteDirectory(DistantTempDir, true);
            } catch (Exception) {
                // don't care
            }
            base.Clean();
        }

        protected override bool SetExecutionInfo() {

            if (Files == null)
                Files = new List<FileToCompile>();

            // for each file of the list
            StringBuilder filesListcontent = new StringBuilder();
            var count = 1;
            foreach (var fileToCompile in Files) {
                if (!File.Exists(fileToCompile.SourcePath)) {
                    UserCommunication.Notify("Couldn't find the following file :<br>" + fileToCompile.SourcePath, MessageImg.MsgError, "Execution error", "File not found", 10);
                    return false;
                }

                var localSubTempDir = Path.Combine(_localTempDir, count.ToString());
                var baseFileName = Path.GetFileNameWithoutExtension(fileToCompile.SourcePath);

                // get the output directory that will be use to generate the .r (and listing debug-list...)
                if (!ComputeOutputDir(fileToCompile, localSubTempDir, count))
                    return false;
                if (!Utils.CreateDirectory(fileToCompile.CompilationOutputDir))
                    return false;

                fileToCompile.CompOutputR = Path.Combine(fileToCompile.CompilationOutputDir, baseFileName + ExtR);
                if (CompileWithListing)
                    fileToCompile.CompOutputLis = Path.Combine(fileToCompile.CompilationOutputDir, baseFileName + ExtLis);
                if (CompileWithXref)
                    fileToCompile.CompOutputXrf = Path.Combine(fileToCompile.CompilationOutputDir, baseFileName + (UseXmlXref ? ExtXrfXml : ExtXrf));
                if (CompileWithDebugList)
                    fileToCompile.CompOutputDbg = Path.Combine(fileToCompile.CompilationOutputDir, baseFileName + ExtDbg);

                if (IsAnalysisMode) {
                    if (!Utils.CreateDirectory(localSubTempDir))
                        return false;
                    fileToCompile.CompOutputFileIdLog = Path.Combine(localSubTempDir, baseFileName + ExtFileIdLog);
                    fileToCompile.CompOutputRefTables = Path.Combine(localSubTempDir, baseFileName + ExtRefTables);
                }

                // if current file and the file has unsaved modif, we copy the content to a temp file, otherwise we just use the input path (also use the input path for .cls files!)
                if (fileToCompile.SourcePath.Equals(Npp.CurrentFileInfo.Path) &&
                    (Sci.GetModify || (fileToCompile.BaseFileName ?? "").StartsWith("_")) &&
                    !Path.GetExtension(fileToCompile.SourcePath).Equals(ExtCls)) {

                    fileToCompile.CompiledSourcePath = Path.Combine(_localTempDir, Path.GetFileName(fileToCompile.SourcePath));
                    Utils.FileWriteAllText(fileToCompile.CompiledSourcePath, Sci.Text, Encoding.Default);
                } else {
                    fileToCompile.CompiledSourcePath = fileToCompile.SourcePath;
                }

                // feed files list
                filesListcontent.AppendLine(fileToCompile.CompiledSourcePath.Quoter() + " " + fileToCompile.CompilationOutputDir.Quoter() + " " + (fileToCompile.CompOutputLis ?? "?").Quoter() + " " + (fileToCompile.CompOutputXrf ?? "?").Quoter() + " " + (fileToCompile.CompOutputDbg ?? "?").Quoter() + " " + (fileToCompile.CompOutputFileIdLog ?? "").Quoter() + " " + (fileToCompile.CompOutputRefTables ?? "").Quoter());

                count++;
            }
            var filesListPath = Path.Combine(_localTempDir, "files.list");
            Utils.FileWriteAllText(filesListPath, filesListcontent.ToString(), Encoding.Default);

            _progressionFilePath = Path.Combine(_localTempDir, "compile.progression");
            _compilationLog = Path.Combine(_localTempDir, "compilation.log");

            SetPreprocessedVar("ToCompileListFile", filesListPath.PreProcQuoter());
            SetPreprocessedVar("CompileProgressionFile", _progressionFilePath.PreProcQuoter());
            SetPreprocessedVar("OutputPath", _compilationLog.PreProcQuoter());
            SetPreprocessedVar("AnalysisMode", IsAnalysisMode.ToString());

            return base.SetExecutionInfo();
        }

        /// <summary>
        /// get the output directory that will be use to generate the .r (and listing debug-list...) 
        /// </summary>
        protected virtual bool ComputeOutputDir(FileToCompile fileToCompile, string localSubTempDir, int count) {
            fileToCompile.CompilationOutputDir = localSubTempDir;
            return true;
        }

        /// <summary>
        /// In test mode, we do as if everything went ok but we don't actually start the process
        /// </summary>
        protected override void StartProcess() {
            if (IsTestMode) {
                PublishExecutionEndEvents();
            } else {
                base.StartProcess();
            }
        }

        /// <summary>
        /// Also publish the end of compilation events
        /// </summary>
        protected override void PublishExecutionEndEvents() {

            // Analysis mode, read output files
            if (IsAnalysisMode) {
                try {
                    // do a deployment action for each file
                    Parallel.ForEach(Files, file => {
                        file.ReadAnalysisResults();
                    });
                } catch (Exception e) {
                    ErrorHandler.ShowErrors(e, "Erreur durant l'analyse de résultats de compilation");
                }
            }

            // end of successful execution action
            if (!ExecutionFailed && (!ConnectionFailed || !NeedDatabaseConnection)) {
                try { 
                    var errorsList = LoadErrorLog();
                    var deployList = GetFilesToDeployAfterCompilation();

                    // don't try to deploy files with errors...
                    if (deployList != null) {
                        foreach (var kpv in errorsList) {
                            if (kpv.Value != null && kpv.Value.Exists(error => error.Level >= ErrorLevel.Error)) {
                                // the file has errors, it was not generated, we don't deploy it
                                deployList.RemoveAll(deploy => deploy.Origin.Equals(kpv.Key));
                            }
                        }
                    }

                    foreach (var kpv in errorsList) {
                        var find = Files.Find(file => file.SourcePath.Equals(kpv.Key));
                        if (find != null) {
                            find.Errors = kpv.Value;
                        }
                    }

                    if (OnCompilationOk != null) {
                        OnCompilationOk(this, Files, deployList);
                    }

                    if (OnEachCompilationOk != null) {
                        OnEachCompilationOk(this, Files, deployList);
                    }
                } catch (Exception e) {
                    ErrorHandler.ShowErrors(e, "Error during the analyze of the compilation output");
                }
            }

            base.PublishExecutionEndEvents();
        }


        #endregion

        #region public methods

        /// <summary>
        /// Number of files already treated
        /// </summary>
        public int NbFilesTreated {
            get {
                return unchecked((int)(File.Exists(_progressionFilePath) ? (new FileInfo(_progressionFilePath)).Length : 0));
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Creates a list of files to deploy after a compilation,
        /// for each Origin file will correspond one (or more if it's a .cls) .r file,
        /// and one .lst if the option has been checked
        /// </summary>
        protected virtual List<FileToDeploy> GetFilesToDeployAfterCompilation() {
            return null;
        }

        /// <summary>
        /// Read the compilation/prolint errors of a given execution through its .log file
        /// update the FilesInfo accordingly so the user can see the errors in npp
        /// </summary>
        private Dictionary<string, List<FileError>> LoadErrorLog() {

            // we need to correct the files path in the log if needed
            var changePaths = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            foreach (var treatedFile in Files.Where(treatedFile => !treatedFile.CompiledSourcePath.Equals(treatedFile.SourcePath))) {
                if (!changePaths.ContainsKey(treatedFile.CompiledSourcePath))
                    changePaths.Add(treatedFile.CompiledSourcePath, treatedFile.SourcePath);
            }

            // read the log file
            return GetErrorsList(changePaths);
        }

        protected virtual Dictionary<string, List<FileError>> GetErrorsList(Dictionary<string, string> changePaths) {
            return ReadErrorsFromFile(_compilationLog, false, changePaths);
        }

        /// <summary>
        /// Reads an error log file, format :
        /// filepath \t ErrorLevel \t line \t column \t error number \t message \t help
        /// (column and line can be equals to "?" in that case, they will be forced to 0)
        /// fromProlint = true allows to set FromProlint to true in the object,
        /// permutePaths allows to replace a path with another, useful when we compiled from a tempdir but we want the errors
        /// to appear for the "real" file
        /// </summary>
        protected static Dictionary<string, List<FileError>> ReadErrorsFromFile(string fullPath, bool fromProlint, Dictionary<string, string> permutePaths) {

            var output = new Dictionary<string, List<FileError>>(StringComparer.CurrentCultureIgnoreCase);

            if (!File.Exists(fullPath))
                return output;

            var lastLineNbCouple = new[] { -10, -10 };

            Utils.ForEachLine(fullPath, new byte[0], (i, line) => {
                var fields = line.Split('\t').ToList();
                if (fields.Count == 8) {
                    var compiledPath = permutePaths.ContainsKey(fields[0]) ? permutePaths[fields[0]] : fields[0];
                    // new file
                    // the path of the file that triggered the compiler error, it can be empty so we make sure to set it
                    var sourcePath = string.IsNullOrEmpty(fields[1]) ? fields[0] : fields[1];
                    sourcePath = permutePaths.ContainsKey(sourcePath) ? permutePaths[sourcePath] : sourcePath;
                    if (!output.ContainsKey(compiledPath)) {
                        output.Add(compiledPath, new List<FileError>());
                        lastLineNbCouple = new[] { -10, -10 };
                    }

                    ErrorLevel errorLevel;
                    if (!Enum.TryParse(fields[2], true, out errorLevel))
                        errorLevel = ErrorLevel.Error;

                    // we store the line/error number couple because we don't want two identical messages to appear
                    var thisLineNbCouple = new[] { (int)fields[3].ConvertFromStr(typeof(int)), (int)fields[5].ConvertFromStr(typeof(int)) };

                    if (thisLineNbCouple[0] == lastLineNbCouple[0] && thisLineNbCouple[1] == lastLineNbCouple[1]) {
                        // same line/error number as previously
                        if (output[compiledPath].Count > 0) {
                            var lastFileError = output[compiledPath].Last();
                            if (lastFileError != null)
                                lastFileError.Times = lastFileError.Times == 0 ? 2 : lastFileError.Times + 1;
                        }
                        return;
                    }
                    lastLineNbCouple = thisLineNbCouple;

                    var baseFileName = Path.GetFileName(sourcePath);

                    // add error
                    output[compiledPath].Add(new FileError {
                        CompiledFilePath = compiledPath,
                        SourcePath = sourcePath,
                        Level = errorLevel,
                        Line = Math.Max(0, lastLineNbCouple[0] - 1),
                        Column = Math.Max(0, (int)fields[4].ConvertFromStr(typeof(int)) - 1),
                        ErrorNumber = lastLineNbCouple[1],
                        Message = fields[6].Replace("<br>", "\n").Replace(compiledPath, baseFileName).Replace(sourcePath, baseFileName).Trim(),
                        Help = fields[7].Replace("<br>", "\n").Trim(),
                        FromProlint = fromProlint,
                    });
                }
            });

            return output;
        }

        #endregion

    }

    #endregion

    #region ProExecutionGenerateDebugfile

    internal class ProExecutionGenerateDebugfile : ProExecutionHandleCompilation {

        public override ExecutionType ExecutionType { get { return ExecutionType.GenerateDebugfile; } }

        public string GeneratedFilePath {
            get {
                if (CompileWithListing)
                    return Files.First().CompOutputLis;
                if (CompileWithXref)
                    return Files.First().CompOutputXrf;
                return Files.First().CompOutputDbg;
            }
        }

        public ProExecutionGenerateDebugfile() {
            CompileWithDebugList = false;
            CompileWithXref = false;
            CompileWithListing = false;
            UseXmlXref = false;
        }

        protected override bool CanUseBatchMode() {
            return true;
        }

    }

    #endregion

    #region ProExecutionCheckSyntax

    internal class ProExecutionCheckSyntax : ProExecutionHandleCompilation {
        public override ExecutionType ExecutionType { get { return ExecutionType.CheckSyntax; } }

        protected override bool CanUseBatchMode() {
            return true;
        }
    }

    #endregion

    #region ProExecutionCompile

    internal class ProExecutionCompile : ProExecutionHandleCompilation {

        #region Life and death

        /// <summary>
        /// Construct with the current env
        /// </summary>
        public ProExecutionCompile() : this(null) { }

        public ProExecutionCompile(ProEnvironment.ProEnvironmentObject proEnv) : base(proEnv) { }

        #endregion

        #region Override

        public override ExecutionType ExecutionType { get { return ExecutionType.Compile; } }

        /// <summary>
        /// Creates a list of files to deploy after a compilation,
        /// for each Origin file will correspond one (or more if it's a .cls) .r file,
        /// and one .lst if the option has been checked
        /// </summary>
        protected override List<FileToDeploy> GetFilesToDeployAfterCompilation() {
            return Deployer.GetFilesToDeployAfterCompilation(this);
        }

        protected override string CheckParameters() {
            if (!ProEnv.CompileLocally && !Path.IsPathRooted(ProEnv.BaseCompilationPath)) {
                return "The path for the compilation base directory is incorrect : <div class='ToolTipcodeSnippet'>" + (String.IsNullOrEmpty(ProEnv.BaseCompilationPath) ? "it's empty!" : ProEnv.BaseCompilationPath) + "</div>You must provide a valid path before executing this action :<br><br><i>1. Either change the compilation directory<br>2. Or toggle the option to compile next to the source file!<br><br>The options are configurable in the <a href='go'>set environment page</a></i>";
            }
            return base.CheckParameters();
        }

        protected override bool CanUseBatchMode() {
            return true;
        }

        /// <summary>
        /// get the output directory that will be use to generate the .r (and listing debug-list...) 
        /// </summary>
        protected override bool ComputeOutputDir(FileToCompile fileToCompile, string localSubTempDir, int count) {

            // for *.cls files, as many *.r files are generated, we need to compile in a temp directory
            // we need to know which *.r files were generated for each input file
            // so each file gets his own sub tempDir
            var lastDeployment = ProEnv.Deployer.GetTransfersNeededForFile(fileToCompile.SourcePath, 0).Last();
            if (lastDeployment.DeployType != DeployType.Move ||
                Config.Instance.CompileForceUseOfTemp ||
                Path.GetExtension(fileToCompile.SourcePath ?? "").Equals(ExtCls)) {

                // if the deployment dir is not on the same disk as the temp folder, we create a temp dir
                // as close to the final deployment as possible (= in the deployment base dir!)
                if (lastDeployment.DeployType != DeployType.Ftp &&
                    !string.IsNullOrEmpty(ProEnv.BaseCompilationPath) && ProEnv.BaseCompilationPath.Length > 2 && !ProEnv.BaseCompilationPath.Substring(0, 2).EqualsCi(_localTempDir.Substring(0, 2))) {

                    if (!Utils.CreateDirectory(DistantTempDir, FileAttributes.Hidden))
                        return false;
                    fileToCompile.CompilationOutputDir = Path.Combine(DistantTempDir, count.ToString());
                } else {
                    fileToCompile.CompilationOutputDir = localSubTempDir;
                }

            } else {
                // if we want to move the r-code somewhere during the deployment, then we will compile the r-code
                // directly there, because it's faster than generating it in a temp folder and moving it afterward
                fileToCompile.CompilationOutputDir = lastDeployment.TargetBasePath;
            }

            return true;
        }

        #endregion

        #region Static

        /// <summary>
        /// Allows to format a small text to explain the errors found in a file and the generated files...
        /// </summary>
        public static string FormatCompilationResultForSingleFile(string sourceFilePath, FileToCompile fileToCompile, List<FileToDeploy> listDeployedFiles) {
            var line = new StringBuilder();

            line.Append("<div style='padding-bottom: 5px;'>");
            line.Append("<img height='15px' src='" + Utils.GetExtensionImage((Path.GetExtension(sourceFilePath) ?? "").Replace(".", "")) + "'>");
            line.Append("<b>" + sourceFilePath.ToHtmlLink(Path.GetFileName(sourceFilePath), true) + "</b> in " + Path.GetDirectoryName(sourceFilePath).ToHtmlLink());
            line.Append("</div>");

            if (fileToCompile != null && fileToCompile.Errors != null) {
                line.Append("<div style='padding-left: 10px; padding-bottom: 5px;'>");
                foreach (var error in fileToCompile.Errors.OrderBy(error => error.Line)) {
                    line.Append(error.ToStringDescription());
                }
                line.Append("</div>");
            }

            if (listDeployedFiles != null) {
                line.Append("<div>");
                // group either by directory name or by pack name
                var groupDirectory = listDeployedFiles.GroupBy(deploy => deploy.GroupKey).Select(deploys => deploys.ToList()).ToList();
                foreach (var group in groupDirectory.OrderByDescending(list => list.First().DeployType).ThenBy(list => list.First().GroupKey)) {
                    line.Append(group.First().ToStringGroupHeader());
                    foreach (var fileToDeploy in group.OrderBy(deploy => deploy.To)) {
                        line.Append(fileToDeploy.ToStringDescription());
                    }
                }
                line.Append("</div>");
            }

            return line.ToString();

        }

        #endregion
    }

    #endregion

    #region ProExecutionRun

    internal class ProExecutionRun : ProExecutionHandleCompilation {

        public override ExecutionType ExecutionType { get { return ExecutionType.Run; } }

        protected override bool SetExecutionInfo() {

            if (!base.SetExecutionInfo())
                return false;

            _processStartDir = Path.GetDirectoryName(Files.First().SourcePath) ?? _localTempDir;

            return true;
        }

    }

    #endregion

    #region ProExecutionProlint

    internal class ProExecutionProlint : ProExecutionHandleCompilation {

        public override ExecutionType ExecutionType { get { return ExecutionType.Prolint; } }

        private string _prolintOutputPath;

        protected override string CheckParameters() {

            // Check if the startprolint procedure exists or create it from resources
            if (!File.Exists(Config.ProlintStartProcedure))
                if (!Utils.FileWriteAllBytes(Config.ProlintStartProcedure, DataResources.StartProlint))
                    return "Could not write the prolint entry point procedure, check reading rights for the file : " + Config.ProlintStartProcedure.ToHtmlLink();

            return base.CheckParameters();
        }

        protected override bool SetExecutionInfo() {

            if (!base.SetExecutionInfo())
                return false;

            if (!Config.Instance.GlobalDontCheckProlintUpdates && (!Updater<ProlintUpdaterWrapper>.Instance.LocalVersion.IsHigherVersionThan("v0") || !Updater<ProparseUpdaterWrapper>.Instance.LocalVersion.IsHigherVersionThan("v0"))) {
                UserCommunication.NotifyUnique("NeedProlint", 
                    "The Prolint installation folder could not be found in 3P.<br>This is normal if it is the first time that you are using this feature.<br><br>" + "download".ToHtmlLink("Please click here to download the latest release of Prolint automatically") + "<br><br><i>You will be informed when it is installed and you will be able to use this feature immediately after.<br><br>If you do not wish to download it and see this message again :<br> toggle off automatic updates for Prolint in the " + "options".ToHtmlLink("update options page") + ".<br>Please note that in that case, you will need to configure Prolint yourself</i>", 
                    MessageImg.MsgQuestion, "Prolint execution", "Prolint installation not found", args => {
                        if (args.Link.Equals("options")) {
                            args.Handled = true;
                            Appli.Appli.GoToPage(PageNames.OptionsUpdate);
                        } else if (args.Link.Equals("download")) {
                            args.Handled = true;
                            Updater<ProlintUpdaterWrapper>.Instance.CheckForUpdate();
                            Updater<ProparseUpdaterWrapper>.Instance.CheckForUpdate();
                        }
                        if (args.Handled)
                            UserCommunication.CloseUniqueNotif("NeedProlint");
                    });
                return false;
            }

            // prolint, we need to copy the StartProlint program
            var fileToExecute = "prolint_" + DateTime.Now.ToString("yyMMdd_HHmmssfff") + ".p";
            _prolintOutputPath = Path.Combine(_localTempDir, "prolint.log");

            StringBuilder prolintProgram = new StringBuilder();
            prolintProgram.AppendLine("&SCOPED-DEFINE PathFileToProlint " + Files.First().CompiledSourcePath.PreProcQuoter());
            prolintProgram.AppendLine("&SCOPED-DEFINE PathProlintOutputFile " + _prolintOutputPath.PreProcQuoter());
            prolintProgram.AppendLine("&SCOPED-DEFINE PathToStartProlintProgram " + Config.ProlintStartProcedure.PreProcQuoter());
            prolintProgram.AppendLine("&SCOPED-DEFINE UserName " + Config.Instance.UserName.PreProcQuoter());
            prolintProgram.AppendLine("&SCOPED-DEFINE PathActualFilePath " + Files.First().SourcePath.PreProcQuoter());
            var filename = Npp.CurrentFileInfo.FileName;
            if (FileCustomInfo.Contains(filename)) {
                var fileInfo = FileCustomInfo.GetLastFileTag(filename);
                prolintProgram.AppendLine("&SCOPED-DEFINE FileApplicationName " + fileInfo.ApplicationName.PreProcQuoter());
                prolintProgram.AppendLine("&SCOPED-DEFINE FileApplicationVersion " + fileInfo.ApplicationVersion.PreProcQuoter());
                prolintProgram.AppendLine("&SCOPED-DEFINE FileWorkPackage " + fileInfo.WorkPackage.PreProcQuoter());
                prolintProgram.AppendLine("&SCOPED-DEFINE FileBugID " + fileInfo.BugId.PreProcQuoter());
                prolintProgram.AppendLine("&SCOPED-DEFINE FileCorrectionNumber " + fileInfo.CorrectionNumber.PreProcQuoter());
                prolintProgram.AppendLine("&SCOPED-DEFINE FileDate " + fileInfo.CorrectionDate.PreProcQuoter());

                prolintProgram.AppendLine("&SCOPED-DEFINE ModificationTagOpening " + ModificationTag.ReplaceTokens(fileInfo, ModificationTagTemplate.Instance.TagOpener).PreProcQuoter());
                prolintProgram.AppendLine("&SCOPED-DEFINE ModificationTagEnding " + ModificationTag.ReplaceTokens(fileInfo, ModificationTagTemplate.Instance.TagCloser).PreProcQuoter());
            }
            prolintProgram.AppendLine("&SCOPED-DEFINE PathDirectoryToProlint " + Updater<ProlintUpdaterWrapper>.Instance.ApplicationFolder.PreProcQuoter());
            prolintProgram.AppendLine("&SCOPED-DEFINE PathDirectoryToProparseAssemblies " + Updater<ProparseUpdaterWrapper>.Instance.ApplicationFolder.PreProcQuoter());
            var encoding = TextEncodingDetect.GetFileEncoding(Config.ProlintStartProcedure);
            Utils.FileWriteAllText(Path.Combine(_localTempDir, fileToExecute), Utils.ReadAllText(Config.ProlintStartProcedure, encoding).Replace(@"/*<inserted_3P_values>*/", prolintProgram.ToString()), encoding);

            SetPreprocessedVar("CurrentFilePath", fileToExecute.PreProcQuoter());

            return true;
        }

        protected override Dictionary<string, List<FileError>> GetErrorsList(Dictionary<string, string> changePaths) {
            return ReadErrorsFromFile(_prolintOutputPath, true, changePaths);
        }
    }

    #endregion

}
