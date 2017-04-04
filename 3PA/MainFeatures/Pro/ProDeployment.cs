using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using _3PA.Lib;

namespace _3PA.MainFeatures.Pro {

    internal class ProDeployment {

        #region Events

        /// <summary>
        /// The action to execute just after the end of a prowin process
        /// </summary>
        public Action<ProDeployment> OnExecutionEnd { private get; set; }

        /// <summary>
        /// The action to execute at the end of the process if it went well = we found a .log and the database is connected or is not mandatory
        /// </summary>
        public Action<ProDeployment> OnExecutionOk { private get; set; }

        /// <summary>
        /// The action to execute at the end of the process if something went wrong (no .log or database down)
        /// </summary>
        public Action<ProDeployment> OnExecutionFailed { private get; set; }

        #endregion

        #region Options

        /// <summary>
        /// If true, don't actually do anything, just test it
        /// </summary>
        public bool IsTestMode { get; set; }

        #endregion

        #region Public properties

        /// <summary>
        /// max step composing this deployment
        /// </summary>
        public int MaxStep { get; private set; }

        /// <summary>
        /// Total number of steps composing this deployment
        /// </summary>
        public int TotalNumberOfSteps { get { return MaxStep + 2; } }

        /// <summary>
        /// Current deployment step
        /// </summary>
        public int CurrentStep { get; private set; }

        /// <summary>
        /// 0 -> 100% progression for the deployment
        /// </summary>
        public float ProgressionPercentage {
            get {
                float totalPerc = _proCompilation == null ? 0 : _proCompilation.CompilationProgression;
                if (CurrentStep > 0) {
                    totalPerc += CurrentStep * 100;
                }
                totalPerc += _currentStepDeployPercentage;
                return totalPerc / (TotalNumberOfSteps + 1);
            }
        }

        /// <summary>
        /// remember the time when the compilation started
        /// </summary>
        public DateTime StartingTime { get; private set; }

        /// <summary>
        /// Human readable amount of time needed for this execution
        /// </summary>
        public string TotalDeploymentTime { get; private set; }

        public bool CompilationHasFailed { get; private set; }

        public bool HasBeenCancelled { get; private set; }

        #endregion

        #region Private fields

        private Dictionary<int, List<FileToDeploy>> _filesToDeployPerStep = new Dictionary<int, List<FileToDeploy>>();

        private DeployProfile _currentProfile;

        private ProEnvironment.ProEnvironmentObject _proEnv;

        private volatile float _currentStepDeployPercentage;

        // Stores the current compilation info
        private ProCompilation _proCompilation;
        
        #endregion

        #region Life and death

        /// <summary>
        /// Constructor
        /// </summary>
        public ProDeployment(ProEnvironment.ProEnvironmentObject proEnv, DeployProfile currentProfile) {
            _proEnv = new ProEnvironment.ProEnvironmentObject(proEnv);
            _currentProfile = new DeployProfile(currentProfile);
        }

        #endregion

        #region Public

        /// <summary>
        /// Start the deployment
        /// </summary>
        public bool Start() {

            StartingTime = DateTime.Now;
            MaxStep = _proEnv.Deployer.DeployTransferRules.Count > 0 ? _proEnv.Deployer.DeployTransferRules.Max(rule => rule.Step) : 0;
            _filesToDeployPerStep.Clear();

            // new mass compilation
            _proCompilation = new ProCompilation(_proEnv) {
                // check if we need to force the compiler to only use 1 process 
                // (either because the user want to, or because we have a single user mode database)
                MonoProcess = _currentProfile.ForceSingleProcess || _proEnv.IsDatabaseSingleUser,
                NumberOfProcessesPerCore = _currentProfile.NumberProcessPerCore,
                RFilesOnly = _currentProfile.OnlyGenerateRcode
            };

            _proCompilation.OnCompilationOk += OnCompilationOk;
            _proCompilation.OnCompilationFailed += OnCompilationFailed;

            return _proCompilation.CompileFiles(GetFilesToCompileInStepZero());
        }

        /// <summary>
        /// Call this method to cancel the execution of this deployment
        /// </summary>
        public void Cancel() {
            HasBeenCancelled = true;
        }

        /// <summary>
        /// Get the time elapsed since the beginning of the compilation in a human readable format
        /// </summary>
        public string GetElapsedTime() {
            return Utils.ConvertToHumanTime(TimeSpan.FromMilliseconds(DateTime.Now.Subtract(StartingTime).TotalMilliseconds));
        }

        #endregion

        /// <summary>
        /// List all the compilable files in the source directory
        /// </summary>
        protected virtual List<FileToCompile> GetFilesToCompileInStepZero() {
            return 
                _proEnv.Deployer.GetFilesList(
                    new List<string> { _currentProfile.SourceDirectory },
                    _currentProfile.ExploreRecursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly,
                    0,
                    Config.Instance.FilesPatternCompilable
                ).Select(s => new FileToCompile(s)).ToList();
        }

        /// <summary>
        /// List all the files that should be deployed from the source directory
        /// </summary>
        protected virtual List<FileToDeploy> GetFilesToDeployInStepOne() {
            return _proEnv.Deployer.GetFilesToDeployForStep(1,
                new List<string> { _currentProfile.SourceDirectory },
                _currentProfile.ExploreRecursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
            );
        }

        #region Private

        /// <summary>
        /// Called when the compilation step 0 failed
        /// </summary>
        private void OnCompilationFailed(ProCompilation proCompilation) {
            CompilationHasFailed = true;
            EndOfDeployment();
        }

        /// <summary>
        /// Called when the compilation step 0 ended correctly
        /// </summary>
        private void OnCompilationOk(ProCompilation comp, List<FileToCompile> fileToCompiles, Dictionary<string, List<FileError>> compilationErrors, List<FileToDeploy> filesToDeploy) {

            // Make the deployment for the compilation step (0)
            _filesToDeployPerStep.Add(0, _proEnv.Deployer.DeployFiles(filesToDeploy, f => _currentStepDeployPercentage = f));

            // Make the deployment for the step 1 and >=
            ExecuteDeploymentHook(0);
        }


        /// <summary>
        /// Deployment for the step 1 and >=
        /// </summary>
        private void DeployStepOneAndMore(int currentStep) {

            _currentStepDeployPercentage = 0;
            CurrentStep = currentStep;

            if (currentStep <= MaxStep) {

                List<FileToDeploy> filesToDeploy;

                if (currentStep == 1) {
                    filesToDeploy = GetFilesToDeployInStepOne();
                } else {
                    filesToDeploy = _proEnv.Deployer.GetFilesToDeployForStep(currentStep,
                        new List<string> {_proEnv.BaseCompilationPath},
                        _currentProfile.ExploreRecursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
                    );
                }

                _filesToDeployPerStep.Add(currentStep, _proEnv.Deployer.DeployFiles(filesToDeploy, f => _currentStepDeployPercentage = f));

                // hook
                ExecuteDeploymentHook(currentStep);

            } else {
                
                EndOfDeployment();
            }
        }

        /// <summary>
        /// Execute the hook procedure for the step 0+
        /// </summary>
        private void ExecuteDeploymentHook(int currentStep) {
            // launch the compile process for the current file
            if (File.Exists(Config.FileDeploymentHook)) {
                var hookExec = new ProExecutionDeploymentHook(_proEnv) {
                    DeploymentStep = currentStep,
                    DeploymentSourcePath = _currentProfile.SourceDirectory,
                    NoBatch = true,
                    NeedDatabaseConnection = true
                };
                currentStep++;
                hookExec.OnExecutionEnd += execution => {
                    DeployStepOneAndMore(currentStep);
                };
                if (!hookExec.Start()) {
                    DeployStepOneAndMore(currentStep);
                }
            } else {
                DeployStepOneAndMore(++currentStep);
            }
        }

        /// <summary>
        /// This method is executed when the overall execution is over
        /// </summary>
        private void EndOfDeployment() {

            TotalDeploymentTime = GetElapsedTime();

            if (!HasBeenCancelled && !CompilationHasFailed) {
                if (OnExecutionOk != null)
                    OnExecutionOk(this);
            } else {
                if (OnExecutionFailed != null)
                    OnExecutionFailed(this);
            }

            if (OnExecutionEnd != null)
                OnExecutionEnd(this);
        }

        #endregion
        
        #region FormatDeploymentReport

        /// <summary>
        /// Generate an html report for the current deployment
        /// </summary>
        public string FormatDeploymentReport() {

            StringBuilder currentReport = new StringBuilder();

            currentReport.Append("<div class='NormalBackColor'>");
            currentReport.Append(@"
                <table class='ToolTipName' style='margin-bottom: 0px; width: 100%'>
                    <tr>
                        <td rowspan='2' style='width: 95px; padding-left: 10px'><img src='Report_64x64' width='64' height='64' /></td>
                        <td class='NotificationTitle'>Deployment report</td>
                    </tr>
                    <tr>
                        <td class='NotificationSubTitle'>" + (HasBeenCancelled ? "<img style='padding-right: 2px;' src='Warning30x30' height='25px'>Canceled by the user" : (!CompilationHasFailed ? "<img style='padding-right: 2px;' src='Ok30x30' height='25px'>Done!" : "<img style='padding-right: 2px;' src='Error30x30' height='25px'>An error has occurred...")) + @"</td>
                    </tr>
                </table>");

            currentReport.Append(@"<h2 style='margin-top: 8px; margin-bottom: 8px;'>Parameters :</h2>");

            currentReport.Append(@"                     
                <div style='margin-left: 8px; margin-right: 8px;'>
                    <table style='width: 100%' class='NormalBackColor'>
                        <tr><td style='width: 40%; padding-right: 20px'>Compilation starting time :</td><td><b>" + _proCompilation.StartingTime + @"</b></td></tr>
                        <tr><td style='padding-right: 20px'>Number of cores detected on this computer :</td><td><b>" + Environment.ProcessorCount + @" cores</b></td></tr>
                        <tr><td style='padding-right: 20px'>Number of Prowin processes used for the compilation :</td><td><b>" + _proCompilation.TotalNumberOfProcesses + @" processes</b></td></tr>
                        <tr><td style='padding-right: 20px'>Forced to mono process? :</td><td><b>" + _proCompilation.MonoProcess + (_proEnv.IsDatabaseSingleUser ? " (connected to database in single user mode!)" : "") + @"</b></td></tr>
                        <tr><td style='width: 40%; padding-right: 20px'>Total number of files being compile :</td><td><b>" + _proCompilation.NbFilesToCompile + @" files</b></td></tr>
                        <tr><td style='width: 40%; padding-right: 20px'>Source directory :</td><td><b>" + _currentProfile.SourceDirectory.ToHtmlLink() + @"</b></td></tr>
                        <tr><td style='width: 40%; padding-right: 20px'>Target deployment directory :</td><td><b>" + _proEnv.BaseCompilationPath.ToHtmlLink() + @"</b></td></tr>
                    </table>           
                </div>");

            currentReport.Append(@"<h2 style='margin-top: 8px; margin-bottom: 8px;'>Results :</h2>");

            if (HasBeenCancelled) {
                // the process has been canceled
                currentReport.Append(@"<div><img style='padding-right: 20px; padding-left: 5px;' src='Warning30x30' height='15px'>The deployment has been canceled by the user</div>");

            } else if (CompilationHasFailed) {

                // provide info on the possible error!
                currentReport.Append(@"<div><img style='padding-right: 20px; padding-left: 5px;' src='Error30x30' height='15px'>At least one process has ended in error, the compilation has been canceled</div>");

                if (_proCompilation.CompilationFailedOnMaxUser) {
                    currentReport.Append(@"<div><img style='padding-right: 20px; padding-left: 5px;' src='Help' height='15px'>One or more processes started for this compilation tried to connect to the database and failed because the maximum number of connection has been reached (error 748). To correct this problem, you can either :<br><li>reduce the number of processes to use for each core of your computer</li><li>or increase the maximum of connections for your database (-n parameter in the PROSERVE command)</li></div>");
                }
            }
            

            var listLinesCompilation = new List<Tuple<int, string>>();
            StringBuilder line = new StringBuilder();

            var totalDeployedFiles = 0;
            var nbDeploymentError = 0;
            var nbCompilationError = 0;
            var nbCompilationWarning = 0;

            // compiled files
            foreach (var fileToCompile in _proCompilation.GetListOfFileToCompile.OrderBy(compile => Path.GetFileName(compile.InputPath))) {

                var toCompile = fileToCompile;
                var errorsOfTheFile = _proCompilation.ListErrors.ContainsKey(toCompile.InputPath) ? _proCompilation.ListErrors[toCompile.InputPath] : new List<FileError>();
                bool hasError = errorsOfTheFile.Count > 0 && errorsOfTheFile.Exists(error => error.Level > ErrorLevel.StrongWarning);
                bool hasWarning = errorsOfTheFile.Count > 0 && errorsOfTheFile.Exists(error => error.Level <= ErrorLevel.StrongWarning);

                if (hasError || hasWarning) {
                    // only add compilation errors
                    line.Clear();
                    line.Append("<div %ALTERNATE%style=\"background-repeat: no-repeat; background-image: url('" + (hasError ? "Error30x30" : "Warning30x30") + "'); padding-left: 40px; padding-top: 6px; padding-bottom: 6px;\">");
                    line.Append(ProDeploymentHtml.FormatCompilationResultForSingleFile(fileToCompile.InputPath, errorsOfTheFile, null));
                    line.Append("</div>");
                    listLinesCompilation.Add(new Tuple<int, string>(hasError ? 3 : 2, line.ToString()));
                }

                if (hasError) {
                    nbCompilationError++;
                    // if compilation errors, delete all transfer records for this file since they obviously didn't happen
                    _filesToDeployPerStep[0].RemoveAll(move => move.Origin.Equals(toCompile.InputPath));
                } else if (hasWarning)
                    nbCompilationWarning++;
            }


            var listLinesByStep = new Dictionary<int, List<Tuple<int, string>>> {
                    {0, new List<Tuple<int, string>>()}
                };

            // for each deploy step
            foreach (var kpv in _filesToDeployPerStep) {
                // group by transfer type
                foreach (var groupType in kpv.Value.GroupBy(deploy => deploy.DeployType).Select(deploys => deploys.ToList()).ToList().OrderBy(list => list.First().DeployType)) {
                    // group either by directory name or by archive name
                    var groupDirectory = groupType.First().DeployType <= DeployType.Zip ?
                        groupType.GroupBy(deploy => deploy.ArchivePath).Select(deploys => deploys.ToList()).ToList().OrderBy(list => list.First().ArchivePath) :
                        groupType.GroupBy(deploy => Path.GetDirectoryName(deploy.To)).Select(deploys => deploys.ToList()).ToList().OrderBy(list => Path.GetDirectoryName(list.First().To));

                    foreach (var group in groupDirectory) {
                        var deployFailed = group.Exists(deploy => !deploy.IsOk);
                        var first = group.First();

                        line.Clear();
                        line.Append("<div %ALTERNATE%style=\"background-repeat: no-repeat; background-image: url('" + (deployFailed ? "Error30x30" : "Ok30x30") + "'); padding-left: 40px; padding-top: 6px; padding-bottom: 6px;\">");

                        string groupBase;
                        if (first.DeployType < DeployType.Archive) {
                            groupBase = first.ArchivePath;
                            var dirPath = Path.GetDirectoryName(first.TargetDir);
                            line.Append("<div style='padding-bottom: 5px;'><img src='" + Utils.GetExtensionImage(first.DeployType == DeployType.Prolib ? "Pl" : "Zip", true) + "' height='15px'><b>" + groupBase.ToHtmlLink(Path.GetFileName(groupBase)) + "</b> in " + string.Format("<a class='SubTextColor' href='{0}'>{1}</a>", dirPath, dirPath) + "</div>");
                        } else {
                            groupBase = Path.GetDirectoryName(first.To);
                            line.Append("<div style='padding-bottom: 5px;'><img src='" + Utils.GetExtensionImage(first.DeployType == DeployType.Ftp ? "Ftp" : "Folder", true) + "' height='15px'><b>" + groupBase.ToHtmlLink() + "</div>");
                        }

                        foreach (var file in group.OrderBy(deploy => deploy.To)) {
                            var ext = (Path.GetExtension(file.To) ?? "").Replace(".", "");
                            var transferMsg = file.DeployType == DeployType.Move ? "" : "(" + file.DeployType + ") ";
                            line.Append("<div style='padding-left: 10px'>");
                            if (file.IsOk) {
                                line.Append("<img src='" + Utils.GetExtensionImage(ext) + "' height='15px'>");
                            } else {
                                line.Append("<img src='Error30x30' height='15px'>Transfer failed for ");
                            }
                            line.Append(transferMsg + file.To.ToHtmlLink(file.To.Replace(groupBase, "").TrimStart('\\')));
                            line.Append(" <span style='padding-left: 8px; padding-right: 8px;'>from</span> " + string.Format("<a class='SubTextColor' href='{0}'>{1}</a>", Path.GetDirectoryName(file.Origin), file.Origin.Replace(kpv.Key <= 1 ? _proEnv.BaseLocalPath : _proEnv.BaseCompilationPath, "").TrimStart('\\')));
                            if (!file.IsOk) {
                                line.Append("<br>Reason : " + file.DeployError);
                            }
                            line.Append("</div>");
                        }

                        line.Append("</div>");

                        if (!listLinesByStep.ContainsKey(kpv.Key))
                            listLinesByStep.Add(kpv.Key, new List<Tuple<int, string>>());

                        listLinesByStep[kpv.Key].Add(new Tuple<int, string>(deployFailed ? 3 : 1, line.ToString()));

                        if (deployFailed)
                            nbDeploymentError += group.Count(deploy => !deploy.IsOk);
                        else
                            totalDeployedFiles += group.Count;
                    }
                }
            }

            // compilation
            currentReport.Append(@"<div style='padding-top: 7px; padding-bottom: 7px;'>Compiling <b>" + _proCompilation.NbFilesToCompile + "</b> files : <b>" + Utils.GetNbFilesPerType(_proCompilation.GetListOfFileToCompile.Select(compile => compile.InputPath).ToList()).Aggregate("", (current, kpv) => current + (@"<img style='padding-right: 5px;' src='" + Utils.GetExtensionImage(kpv.Key.ToString(), true) + "' height='15px'><span style='padding-right: 12px;'>x" + kpv.Value + "</span>")) + "</b></div>");

            // compilation time
            currentReport.Append(@"<div><img style='padding-right: 20px; padding-left: 5px;' src='Time' height='15px'>Total elapsed time for the compilation : <b>" + _proCompilation.TotalCompilationTime + @"</b></div>");

            if (nbCompilationError > 0)
                currentReport.Append("<div><img style='padding-right: 20px; padding-left: 5px;' src='Error30x30' height='15px'>" + nbCompilationError + " files with compilation error(s)</div>");
            if (nbCompilationWarning > 0)
                currentReport.Append("<div><img style='padding-right: 20px; padding-left: 5px;' src='Warning30x30' height='15px'>" + nbCompilationWarning + " files with compilation warning(s)</div>");
            if (_proCompilation.NbFilesToCompile - nbCompilationError - nbCompilationWarning > 0)
                currentReport.Append("<div><img style='padding-right: 20px; padding-left: 5px;' src='Ok30x30' height='15px'>" + (_proCompilation.NbFilesToCompile - nbCompilationError - nbCompilationWarning) + " files compiled correctly</div>");

            // deploy
            currentReport.Append(@"<div style='padding-top: 7px; padding-bottom: 7px;'>Deploying <b>" + totalDeployedFiles + "</b> files : <b>" + Utils.GetNbFilesPerType(_filesToDeployPerStep.SelectMany(pair => pair.Value).Select(deploy => deploy.To).ToList()).Aggregate("", (current, kpv) => current + (@"<img style='padding-right: 5px;' src='" + Utils.GetExtensionImage(kpv.Key.ToString(), true) + "' height='15px'><span style='padding-right: 12px;'>x" + kpv.Value + "</span>")) + "</b></div>");

            // deployment time
            currentReport.Append(@"<div><img style='padding-right: 20px; padding-left: 5px;' src='Time' height='15px'>Total elapsed time for the deployment : <b>" + TotalDeploymentTime + @"</b></div>");

            if (nbDeploymentError > 0)
                currentReport.Append("<div><img style='padding-right: 20px; padding-left: 5px;' src='Error30x30' height='15px'>" + nbDeploymentError + " files not deployed</div>");
            if (totalDeployedFiles - nbDeploymentError > 0)
                currentReport.Append("<div><img style='padding-right: 20px; padding-left: 5px;' src='Ok30x30' height='15px'>" + (totalDeployedFiles - nbDeploymentError) + " files deployed correctly</div>");

            // compilation
            if (listLinesCompilation.Count > 0) {
                currentReport.Append("<h3 style='margin-top: 7px; margin-bottom: 7px;'>Compilation error details :</h3>");
                var boolAlternate = false;
                foreach (var listLine in listLinesCompilation.OrderByDescending(tuple => tuple.Item1)) {
                    currentReport.Append(listLine.Item2.Replace("%ALTERNATE%", boolAlternate ? "class='AlternatBackColor' " : "class='NormalBackColor' "));
                    boolAlternate = !boolAlternate;
                }
            }

            // deployment steps
            foreach (var listLinesKpv in listLinesByStep) {
                currentReport.Append("<h3 style='margin-top: 7px; margin-bottom: 7px;'>Deployment step " + listLinesKpv.Key + " :</h3>");

                var boolAlternate2 = false;
                foreach (var listLine in listLinesKpv.Value.OrderByDescending(tuple => tuple.Item1)) {
                    currentReport.Append(listLine.Item2.Replace("%ALTERNATE%", boolAlternate2 ? "class='AlternatBackColor' " : "class='NormalBackColor' "));
                    boolAlternate2 = !boolAlternate2;
                }
            }

            currentReport.Append("</div>");

            return currentReport.ToString();
        }

        #endregion

    }

    internal static class ProDeploymentHtml {

        /// <summary>
        /// Allows to format a small text to explain the errors found in a file and the generated files...
        /// </summary>
        public static string FormatCompilationResultForSingleFile(string sourceFilePath, List<FileError> listErrorFiles, List<FileToDeploy> listDeployedFiles) {
            var line = new StringBuilder();
            var nbErrors = 0;

            line.Append("<div style='padding-bottom: 5px;'><b>" + string.Format("<a class='SubTextColor' href='{0}'>{1}</a>", sourceFilePath, Path.GetFileName(sourceFilePath)) + "</b> in " + Path.GetDirectoryName(sourceFilePath).ToHtmlLink() + "</div>");

            if (listErrorFiles != null)
                foreach (var fileError in listErrorFiles) {
                    nbErrors += fileError.Level > ErrorLevel.StrongWarning ? 1 : 0;
                    line.Append("<div style='padding-left: 10px'>" + "<img src='" + (fileError.Level > ErrorLevel.StrongWarning ? "MsgError" : "MsgWarning") + "' height='15px'>" + (!fileError.CompiledFilePath.Equals(fileError.SourcePath) ? "in " + string.Format("<a class='SubTextColor' href='{0}'>{1}</a>", fileError.SourcePath, Path.GetFileName(fileError.SourcePath)) + ", " : "") + (fileError.SourcePath + "|" + fileError.Line).ToHtmlLink("line " + (fileError.Line + 1)) + " (n°" + fileError.ErrorNumber + ") " + (fileError.Times > 0 ? "(x" + fileError.Times + ") " : "") + fileError.Message + "</div>");
                }

            if (listDeployedFiles != null)
                foreach (var file in listDeployedFiles) {
                    var ext = (Path.GetExtension(file.To) ?? "").Replace(".", "");
                    var transferMsg = file.DeployType == DeployType.Move ? "" : "(" + file.DeployType + ") ";
                    if (file.IsOk && (nbErrors == 0 || !ext.Equals("r"))) {
                        line.Append("<div style='padding-left: 10px'>" + "<img src='" + Utils.GetExtensionImage(ext) + "' height='15px'>" + transferMsg + file.To.ToHtmlLink() + "</div>");
                    } else if (nbErrors == 0) {
                        line.Append("<div style='padding-left: 10px'>" + "<img src='MsgError' height='15px'>Transfer error " + transferMsg + Path.GetDirectoryName(file.To).ToHtmlLink(file.To) + "</div>");
                    }
                }

            return line.ToString();
        }

    }
}
