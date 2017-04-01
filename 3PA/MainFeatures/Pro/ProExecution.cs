#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProExecution.cs) is part of 3P.
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using YamuiFramework.Helper;
using _3PA.Lib;
using _3PA.MainFeatures.Appli;
using _3PA.NppCore;
using _3PA._Resource;

// ReSharper disable LocalizableElement

namespace _3PA.MainFeatures.Pro {

    internal abstract class ProExecution {

        #region Factory

        public static ProExecution Factory(ExecutionType executionType) {
            switch (executionType) {
                case ExecutionType.CheckSyntax:
                    return new ProExecutionCheckSyntax();
                case ExecutionType.Compile:
                    return new ProExecutionCompile();
                case ExecutionType.Run:
                    return new ProExecutionRun();
                case ExecutionType.Prolint:
                    return new ProExecutionProlint();
                default:
                    throw new Exception("Factory : the type " + executionType + " does not exist");
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// The action to execute just after the end of a prowin process
        /// </summary>
        public Action<ProExecution> OnExecutionEnd { private get; set; }

        /// <summary>
        /// The action to execute at the end of the process if it went well = we found a .log and the database is connected or is not mandatory
        /// </summary>
        public Action<ProExecution> OnExecutionOk { private get; set; }

        /// <summary>
        /// The action to execute at the end of the process if something went wrong (no .log or database down)
        /// </summary>
        public Action<ProExecution> OnExecutionFailed { private get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// set to true if a the execution process has been killed
        /// </summary>
        public bool HasBeenKilled { get; private set; }

        /// <summary>
        /// Set to true after the process is over if the execution failed
        /// </summary>
        public bool ExecutionFailed { get; private set; }

        /// <summary>
        /// Set to true after the process is over if the database connection has failed
        /// </summary>
        public bool ConnectionFailed { get; private set; }
        
        /// <summary>
        /// set to true if a valid database connection is mandatory
        /// </summary>
        public bool NeedDatabaseConnection { get; set; }

        /// <summary>
        /// Set to true to not use the batch mode
        /// </summary>
        public bool NoBatch { get; set; }

        #endregion

        #region Private fields

        /// <summary>
        /// Full file path to the output file for the custom post-execution notification
        /// </summary>
        protected string _notifPath;

        protected string _tempInifilePath;

        protected Dictionary<string, string> _preprocessedVars;

        /// <summary>
        /// Path to the output .log file (for compilation)
        /// </summary>
        protected string _logPath;

        /// <summary>
        /// log to the database connection log (not existing if everything is ok)
        /// </summary>
        protected string _dbLogPath;

        /// <summary>
        /// Full path to the directory containing all the files needed for the execution
        /// </summary>
        protected string _localTempDir;

        /// <summary>
        /// Full path to the directory used as the working directory to start the prowin process
        /// </summary>
        protected string _processStartDir;

        protected string _propath;

        /// <summary>
        /// Parameters of the .exe call
        /// </summary>
        protected StringBuilder _exeParameters;

        protected Process _process;

        /// <summary>
        /// The pro environment used at the moment the execution was created
        /// </summary>
        public ProEnvironment.ProEnvironmentObject ProEnv;

        protected bool _useBatchMode;

        public virtual ExecutionType ExecutionType { get { return ExecutionType.CheckSyntax; } }

        #endregion

        #region constructors and destructor

        /// <summary>
        /// Deletes temp directory and everything in it
        /// </summary>
        ~ProExecution() {
            try {
                if (_process != null)
                    _process.Close();

                // delete temp dir
                if (_localTempDir != null)
                    Utils.DeleteDirectory(_localTempDir, true);

            } catch (Exception) {
                // dont care
            }
        }

        public ProExecution() {

            // create a copy of the current environment
            ProEnv = new ProEnvironment.ProEnvironmentObject(ProEnvironment.Current);

            _preprocessedVars = new Dictionary<string, string> {
                {"LogPath", "\"\""},
                {"DbLogPath", "\"\""},
                {"PropathToUse", "\"\""},
                {"DbConnectString", "\"\""},
                {"ExecutionType", "\"\""},
                {"CurrentFilePath", "\"\""},
                {"ExtractDbOutputPath", "\"\""},
                {"ToCompileListFile", "\"\""},
                {"CompileProgressionFile", "\"\""},
                {"DbConnectionMandatory", "false"},
                {"NotificationOutputPath", "\"\""},
                {"PreExecutionProgram", "\"\""},
                {"CompilationLogPath", "\"\""},
            };
        }

        #endregion

        #region Do

        /// <summary>
        /// allows to prepare the execution environment by creating a unique temp folder
        /// and copying every critical files into it
        /// Then execute the progress program
        /// </summary>
        /// <returns></returns>
        public bool Do() {

            // check parameters
            var errorString = CheckParameters();
            if (!string.IsNullOrEmpty(errorString)) {
                UserCommunication.NotifyUnique("ProExecutionChecks", errorString, MessageImg.MsgHighImportance, "Progress execution", "Couldn't start execution", args => {
                    Appli.Appli.GoToPage(PageNames.SetEnvironment);
                    UserCommunication.CloseUniqueNotif("ProExecutionChecks");
                    args.Handled = true;
                }, 10);
                return false;
            }

            // create a unique temporary folder
            _localTempDir = Path.Combine(Config.FolderTemp, "exec_" + DateTime.Now.ToString("HHmmssfff") + "_" + Path.GetRandomFileName());
            if (!Utils.CreateDirectory(_localTempDir))
                return false;

            // move .ini file into the execution directory
            if (File.Exists(ProEnv.IniPath)) {
                _tempInifilePath = Path.Combine(_localTempDir, "base.ini");

                // we need to copy the .ini but we must delete the PROPATH= part, as stupid as it sounds, if we leave a huge PROPATH 
                // in this file, it increases the compilation time by a stupid amount... unbelievable i know, but trust me, it does...
                var encoding = TextEncodingDetect.GetFileEncoding(ProEnv.IniPath);
                var fileContent = Utils.ReadAllText(ProEnv.IniPath, encoding);
                var regex = new Regex("^PROPATH=.*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                var matches = regex.Match(fileContent);
                if (matches.Success)
                    fileContent = regex.Replace(fileContent, @"PROPATH=");
                Utils.FileWriteAllText(_tempInifilePath, fileContent, encoding);
            }

            // read .pf file
            var dbConnectionString = "";
            if (File.Exists(ProEnv.GetPfPath())) {
                var connectionString = "";
                Utils.ForEachLine(ProEnv.GetPfPath(), new byte[0], (nb, line) => {
                    var commentPos = line.IndexOf("#", StringComparison.CurrentCultureIgnoreCase);
                    if (commentPos == 0)
                        return;
                    if (commentPos > 0)
                        line = line.Substring(0, commentPos);
                    line = line.Trim();
                    if (!string.IsNullOrEmpty(line))
                        connectionString += " " + line;
                });
                dbConnectionString = connectionString;
            }
            dbConnectionString += " " + ProEnv.ExtraPf.Trim();

            // set common info on the execution
            _processStartDir = _localTempDir;
            _logPath = Path.Combine(_localTempDir, "run.log");
            _dbLogPath = Path.Combine(_localTempDir, "db.ko");
            _notifPath = Path.Combine(_localTempDir, "postExecution.notif");
            _propath = (_localTempDir + "," + string.Join(",", ProEnv.GetProPathDirList)).Trim().Trim(',');

            // Set info
            if (!SetExecutionInfo())
                return false;

            SetPreprocessedVar("ExecutionType", ExecutionType.ToString().ToUpper().ProQuoter());
            SetPreprocessedVar("LogPath", _logPath.ProQuoter());
            SetPreprocessedVar("propathToUse", _propath.ProQuoter());
            SetPreprocessedVar("DbConnectString", dbConnectionString.ProQuoter());
            SetPreprocessedVar("DbLogPath", _dbLogPath.ProQuoter());
            SetPreprocessedVar("DbConnectionMandatory", NeedDatabaseConnection.ToString());
            SetPreprocessedVar("NotificationOutputPath", _notifPath.ProQuoter());
            //SetPreprocessedVar("PreExecutionProgram", ""); // TODO: do -------------------------------------

            // prepare the .p runner
            var runnerPath = Path.Combine(_localTempDir, "run_" + DateTime.Now.ToString("HHmmssfff") + ".p");
            StringBuilder runnerProgram = new StringBuilder();
            foreach (var @var in _preprocessedVars) {
                runnerProgram.AppendLine("&SCOPED-DEFINE " + @var.Key + " " + @var.Value);
            }
            runnerProgram.Append(Encoding.Default.GetString(DataResources.ProgressRun));
            Utils.FileWriteAllText(runnerPath, runnerProgram.ToString(), Encoding.Default);
            
            // no batch mode option?
            _useBatchMode = !Config.Instance.NeverUseProwinInBatchMode && !NoBatch && CanUseBatchMode();

            // Parameters
            _exeParameters = new StringBuilder();
            AppendProgressParameters(_exeParameters);
            _exeParameters.Append(_useBatchMode ? " -b" : " -nosplash");
            _exeParameters.Append(" -p " + runnerPath.ProQuoter());
            if (!string.IsNullOrWhiteSpace(ProEnv.CmdLineParameters))
                _exeParameters.Append(" " + ProEnv.CmdLineParameters.Trim());

            // start the process
            try {
                StartProcess();
            } catch (Exception e) {
                UserCommunication.NotifyUnique("ProwinFailed", "Couldn't start a new Prowin process!<br>Please check that the file path to prowin32.exe is correct in the <a href='go'>set environment page</a>.<br><br>Below is the technical error that occurred :<br><div class='ToolTipcodeSnippet'>" + e.Message + "</div>", MessageImg.MsgError, "Progress execution", "Can't start a Prowin process", args => {
                    Appli.Appli.GoToPage(PageNames.SetEnvironment);
                    UserCommunication.CloseUniqueNotif("ProwinFailed");
                    args.Handled = true;
                }, 10);
            }

            //UserCommunication.Notify("New process starting...<br><br><b>FileName :</b><br>" + ProEnv.ProwinPath + "<br><br><b>Parameters :</b><br>" + ExeParameters + "<br><br><b>Temporary directory :</b><br><a href='" + TempDir + "'>" + TempDir + "</a>");

            return true;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Allows to kill the process of this execution (be careful, the OnExecutionEnd, Ok, Fail events are not executed in that case!) 
        /// </summary>
        public void KillProcess() {
            try {
                _process.Kill();
                _process.Close();
            } catch (Exception) {
                // ignored
            }
            HasBeenKilled = true;
        }

        /// <summary>
        /// Allows to kill the process of this execution (be careful, the OnExecutionEnd, Ok, Fail events are not executed in that case!) 
        /// </summary>
        public void BringProcessToFront() {
            try {
                WinApi.SetForegroundWindow(_process.MainWindowHandle);
            } catch (Exception) {
                // ignored
            }
        }

        public void WaitForProcessExit(int maxWait = 3000) {
            if (maxWait > 0)
                _process.WaitForExit(maxWait);
            else {
                _process.WaitForExit();
            }
        }

        public bool DbConnectionFailedOnMaxUser {
            get { return (Utils.ReadAllText(_dbLogPath, Encoding.Default) ?? "").Contains("(748)"); }
        }

        #endregion

        #region To override

        /// <summary>
        /// Should return null or the message error that indicates which parameter is incorrect
        /// </summary>
        protected virtual string CheckParameters() {

            // check prowin32.exe
            if (!File.Exists(ProEnv.ProwinPath)) {
                return "The file path to Prowin.exe is incorrect : <div class='ToolTipcodeSnippet'>" + ProEnv.ProwinPath + "</div>You must provide a valid path before executing this action<br><i>You can change this path in the <a href='go'>set environment page</a></i>";
            }

            return null;
        }

        /// <summary>
        /// Return true if can use batch mode
        /// </summary>
        protected virtual bool CanUseBatchMode() {
            return false;
        }

        /// <summary>
        /// Return true if can use batch mode
        /// </summary>
        protected virtual bool SetExecutionInfo() {
            return true;
        }

        /// <summary>
        /// Add stuff to the command line
        /// </summary>
        protected virtual void AppendProgressParameters(StringBuilder sb) {
            if (!string.IsNullOrEmpty(_tempInifilePath))
                sb.Append(" -ininame " + _tempInifilePath.ProQuoter() + " -basekey " + "INI".ProQuoter());
        }

        #endregion

        #region private methods

        /// <summary>
        /// set pre-processed variable for the runner program
        /// </summary>
        protected void SetPreprocessedVar(string key, string value) {
            if (!_preprocessedVars.ContainsKey(key))
                _preprocessedVars.Add(key, value);
            else
                _preprocessedVars[key] = value;
        }

        /// <summary>
        /// Start the prowin process with the options defined in this object
        /// </summary>
        private void StartProcess() {
            var pInfo = new ProcessStartInfo {
                FileName = ProEnv.ProwinPath,
                Arguments = _exeParameters.ToString(),
                WorkingDirectory = _processStartDir
            };
            if (_useBatchMode) {
                pInfo.WindowStyle = ProcessWindowStyle.Hidden;
                pInfo.CreateNoWindow = true;
            }
            _process = new Process {
                StartInfo = pInfo,
                EnableRaisingEvents = true
            };
            _process.Exited += ProcessOnExited;
            _process.Start();
        }

        /// <summary>
        /// Called by the process's thread when it is over, execute the ProcessOnExited event
        /// </summary>
        private void ProcessOnExited(object sender, EventArgs eventArgs) {
            // end of execution action
            if (OnExecutionEnd != null) {
                OnExecutionEnd(this);
            }

            // if log not found then something is messed up!
            if (string.IsNullOrEmpty(_logPath) || !File.Exists(_logPath)) {
                UserCommunication.NotifyUnique("ExecutionFailed", "Something went terribly wrong while using progress!<br><div>Below is the <b>command line</b> that was executed:</div><div class='ToolTipcodeSnippet'>" + ProEnv.ProwinPath + " " + _exeParameters + "</div><b>Temporary directory :</b><br>" + _localTempDir.ToHtmlLink() + "<br><br><i>Did you messed up the prowin32.exe command line parameters in the <a href='go'>set environment page</a> page?</i>", MessageImg.MsgError, "Progress execution", "Critical error", args => {
                    if (args.Link.Equals("go")) {
                        Appli.Appli.GoToPage(PageNames.SetEnvironment);
                        UserCommunication.CloseUniqueNotif("ExecutionFailed");
                        args.Handled = true;
                    }
                }, 0, 600);

                ExecutionFailed = true;

            } else {
                var logContent = Utils.ReadAllText(_logPath, Encoding.Default).Trim();
                if (!string.IsNullOrEmpty(logContent)) {
                    UserCommunication.NotifyUnique("ExecutionFailed", "An error occurred in the progress execution, details :<div class='ToolTipcodeSnippet'>" + logContent + "</div>", MessageImg.MsgError, "Progress execution", "Critical error", null, 0, 600);
                    ExecutionFailed = true;
                }
            }

            // if this file exists, then the connect statement failed, warn the user
            if (File.Exists(_dbLogPath) && new FileInfo(_dbLogPath).Length > 0) {
                UserCommunication.NotifyUnique("ConnectFailed", "Failed to connect to the progress database!<br>Verify / correct the connection info <a href='go'>in the environment page</a> and try again<br><br><i>Also, make sure that the database for the current environment is connected!</i><br><br>Below is the error returned while trying to connect to the database : " + Utils.ReadAndFormatLogToHtml(_dbLogPath), MessageImg.MsgRip, "Database connection", "Connection failed", args => {
                    if (args.Link.Equals("go")) {
                        Appli.Appli.GoToPage(PageNames.SetEnvironment);
                        UserCommunication.CloseUniqueNotif("ConnectFailed");
                        args.Handled = true;
                    }
                }, NeedDatabaseConnection ? 0 : 10, 600);

                ConnectionFailed = true;
            }

            // end of successful/unsuccessful execution action
            if (ExecutionFailed || (ConnectionFailed && NeedDatabaseConnection)) {
                if (OnExecutionFailed != null) {
                    OnExecutionFailed(this);
                }
            } else {
                if (OnExecutionOk != null) {
                    OnExecutionOk(this);
                }

                // display a custom post execution notification if needed
                DisplayPostExecutionNotification();
            }
        }

        /// <summary>
        /// Read a file in which each line represents a notification to display to the user,
        /// and displays each notification
        /// </summary>
        private void DisplayPostExecutionNotification() {
            // no notifications?
            if (string.IsNullOrEmpty(_notifPath) || !File.Exists(_notifPath))
                return;

            Utils.ForEachLine(_notifPath, null, (i, line) => {
                var fields = line.Split('\t').ToList();
                if (fields.Count == 6) {
                    MessageImg messageImg;
                    if (!Enum.TryParse(fields[1], true, out messageImg))
                        messageImg = MessageImg.MsgDebug;

                    if (string.IsNullOrEmpty(fields[5]))
                        UserCommunication.Notify(fields[0], messageImg, fields[2], fields[3], (int)fields[4].ConvertFromStr(typeof(int)));
                    else
                        UserCommunication.NotifyUnique(fields[5], fields[0], messageImg, fields[2], fields[3], null, (int)fields[4].ConvertFromStr(typeof(int)));
                }
            });
        }

        #endregion
        
    }
    
    internal abstract class ProExecutionHandleCompilation : ProExecution {

        #region Properties

        /// <summary>
        /// Temp directory located in the deployment dir
        /// </summary>
        public string DistantTempDir { get; private set; }

        /// <summary>
        /// List of the files to compile / run / prolint
        /// </summary>
        public List<FileToCompile> Files { get; set; }
        
        public bool CompileWithDebugList { get; set; }

        public bool CompileWithListing { get; set; }

        public bool CompileWithXref { get; set; }

        public bool UseXmlXref { get; set; }

        #endregion

        #region private fields

        private const string ExtR = ".r";
        private const string ExtDbg = ".dbg";
        private const string ExtLis = ".lis";
        private const string ExtXrf = ".xrf";
        private const string ExtXrfXml = ".xrf.xml";
        private const string ExtCls = ".cls";

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
        /// Deletes temp directory and everything in it
        /// </summary>
        ~ProExecutionHandleCompilation() {
            try {
                // delete temp dir
                Utils.DeleteDirectory(DistantTempDir, true);
            } catch (Exception) {
                // don't care
            }
        }

        #endregion

        #region Override

        protected override string CheckParameters() {
            if (ExecutionType == ExecutionType.Compile && !ProEnv.CompileLocally && !Path.IsPathRooted(ProEnv.BaseCompilationPath)) {
                return "The path for the compilation base directory is incorrect : <div class='ToolTipcodeSnippet'>" + (string.IsNullOrEmpty(ProEnv.BaseCompilationPath) ? "it's empty!" : ProEnv.BaseCompilationPath) + "</div>You must provide a valid path before executing this action :<br><br><i>1. Either change the compilation directory<br>2. Or toggle the option to compile next to the source file!<br><br>The options are configurable in the <a href='go'>set environment page</a></i>";
            }
            return base.CheckParameters();
        }

        protected override bool SetExecutionInfo() {

            if (Files == null)
                Files = new List<FileToCompile>();

            // for each file of the list
            StringBuilder filesListcontent = new StringBuilder();
            var count = 1;
            foreach (var fileToCompile in Files) {
                if (!File.Exists(fileToCompile.InputPath)) {
                    UserCommunication.Notify("Couldn't find the following file :<br>" + fileToCompile.InputPath, MessageImg.MsgError, "Execution error", "File not found", 10);
                    return false;
                }

                // if current file and the file has unsaved modif, we copy the content to a temp file, otherwise we just use the input path (also use the input path for .cls files!)
                if (fileToCompile.InputPath.Equals(Npp.CurrentFile.Path) &&
                    (Sci.GetModify || (fileToCompile.BaseFileName ?? "").StartsWith("_")) &&
                    !Path.GetExtension(fileToCompile.InputPath).Equals(ExtCls)) {
                    fileToCompile.CompInputPath = Path.Combine(_localTempDir, "current_file_" + DateTime.Now.ToString("HHmmssfff") + Path.GetExtension(fileToCompile.InputPath));
                    Utils.FileWriteAllText(fileToCompile.CompInputPath, Sci.Text, Encoding.Default);
                } else {
                    fileToCompile.CompInputPath = fileToCompile.InputPath;
                }

                // we set where the *.lst and *.r files will be generated by the COMPILE command
                var baseFileName = Path.GetFileNameWithoutExtension(fileToCompile.CompInputPath);
                var lastDeployment = ProEnv.Deployer.GetTargetDirsNeededForFile(fileToCompile.InputPath, 0).Last();

                // for *.cls files, as many *.r files are generated, we need to compile in a temp directory
                // we need to know which *.r files were generated for each input file
                // so each file gets his own sub tempDir
                if ((lastDeployment.DeployType != DeployType.Move) ||
                    Config.Instance.CompileForceUseOfTemp ||
                    Path.GetExtension(fileToCompile.InputPath).Equals(ExtCls)
                ) {
                    var subTempDir = Path.Combine(_localTempDir, count.ToString());

                    // if the deployment dir is not on the same disk as the temp folder, we create a temp dir
                    // as close to the final deployment as possible (= in the deployment base dir!)
                    if (lastDeployment.DeployType != DeployType.Ftp && !string.IsNullOrEmpty(ProEnv.BaseCompilationPath) && ProEnv.BaseCompilationPath.Length > 2 && !ProEnv.BaseCompilationPath.Substring(0, 2).EqualsCi(_localTempDir.Substring(0, 2))) {
                        DistantTempDir = Path.Combine(ProEnv.BaseCompilationPath, "~3p-tmp-" + DateTime.Now.ToString("HHmmss") + "-" + Path.GetRandomFileName());
                        if (!Utils.CreateDirectory(DistantTempDir, FileAttributes.Hidden))
                            DistantTempDir = _localTempDir;
                        subTempDir = Path.Combine(DistantTempDir, count.ToString());
                    }
                    
                    fileToCompile.CompOutputDir = subTempDir;
                } else {
                    // if we want to move the r-code somewhere during the deployment, then we will compile the r-code
                    // directly there, because it's faster than generating it in a temp folder and moving it afterward
                    fileToCompile.CompOutputDir = lastDeployment.TargetDir;
                }

                fileToCompile.CompOutputR = Path.Combine(fileToCompile.CompOutputDir, baseFileName + ExtR);
                if (CompileWithListing)
                    fileToCompile.CompOutputLis = Path.Combine(fileToCompile.CompOutputDir, baseFileName + ExtLis);
                if (CompileWithXref)
                    fileToCompile.CompOutputXrf = Path.Combine(fileToCompile.CompOutputDir, baseFileName + (UseXmlXref ? ExtXrfXml : ExtXrf));
                if (CompileWithDebugList)
                    fileToCompile.CompOutputDbg = Path.Combine(fileToCompile.CompOutputDir, baseFileName + ExtDbg);

                if (!Utils.CreateDirectory(fileToCompile.CompOutputDir))
                    return false;

                // feed files list
                filesListcontent.AppendLine(fileToCompile.CompInputPath.ProQuoter() + " " + fileToCompile.CompOutputDir.ProQuoter() + " " + (fileToCompile.CompOutputLis ?? "?").ProQuoter() + " " + (fileToCompile.CompOutputXrf ?? "?").ProQuoter() + " " + (fileToCompile.CompOutputDbg ?? "?").ProQuoter());

                count++;
            }
            var filesListPath = Path.Combine(_localTempDir, "files.list");
            Utils.FileWriteAllText(filesListPath, filesListcontent.ToString(), Encoding.Default);
            
            _progressionFilePath = Path.Combine(_localTempDir, "compile.progression");
            _compilationLog = Path.Combine(_localTempDir, "compilation.log");

            SetPreprocessedVar("ToCompileListFile", filesListPath.ProQuoter());
            SetPreprocessedVar("CompileProgressionFile", _progressionFilePath.ProQuoter());
            SetPreprocessedVar("CompilationLogPath", _compilationLog.ProQuoter());

            return base.SetExecutionInfo();
        }

        protected virtual Dictionary<string, List<FileError>> GetErrorsList(Dictionary<string, string> changePaths) {
            return FilesInfo.ReadErrorsFromFile(_compilationLog, false, changePaths);
        }

        #endregion

        #region public methods

        /// <summary>
        /// Number of files already treated
        /// </summary>
        public long NbFilesTreated {
            get {
                return File.Exists(_progressionFilePath) ? (new FileInfo(_progressionFilePath)).Length : 0;
            }
        }

        /// <summary>
        /// Read the compilation/prolint errors of a given execution through its .log file
        /// update the FilesInfo accordingly so the user can see the errors in npp
        /// </summary>
        public Dictionary<string, List<FileError>> LoadErrorLog() {

            // we need to correct the files path in the log if needed
            var changePaths = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            foreach (var treatedFile in Files.Where(treatedFile => !treatedFile.CompInputPath.Equals(treatedFile.InputPath))) {
                if (!changePaths.ContainsKey(treatedFile.CompInputPath))
                    changePaths.Add(treatedFile.CompInputPath, treatedFile.InputPath);
            }

            // read the log file
            Dictionary<string, List<FileError>> errorsList = GetErrorsList(changePaths);

            // clear errors on each compiled file
            foreach (var fileToCompile in Files) {
                FilesInfo.ClearAllErrors(fileToCompile.InputPath, true);
            }

            // update the errors
            foreach (var keyValue in errorsList) {
                FilesInfo.UpdateFileErrors(keyValue.Key, keyValue.Value);
            }

            return errorsList;
        }

        #endregion

        #region CreateListOfFilesToDeploy

        /// <summary>
        /// Creates a list of files to deploy after a compilation,
        /// for each Origin file will correspond one (or more if it's a .cls) .r file,
        /// and one .lst if the option has been checked
        /// </summary>
        public List<FileToDeploy> CreateListOfFilesToDeploy() {

            var outputList = new List<FileToDeploy>();
            var clsNotFound = new StringBuilder();

            foreach (var compiledFile in Files) {

                // Is the input file a class file?
                if (compiledFile.InputPath.EndsWith(ExtCls, StringComparison.CurrentCultureIgnoreCase)) {
                    // if the file we compiled inherits from another class or if another class inherits of our file, 
                    // there is more than 1 *.r file generated. Moreover, they are generated in their package folders
                    try {
                        // for each *.r file
                        foreach (var rCodeFile in Directory.EnumerateFiles(compiledFile.CompOutputDir, "*" + ExtR, SearchOption.AllDirectories)) {
                            var relativePath = rCodeFile.Replace(compiledFile.CompOutputDir, "").TrimStart('\\');
                            var sourcePath = ProEnv.FindFirstFileInPropath(Path.ChangeExtension(relativePath, ExtCls));

                            if (string.IsNullOrEmpty(sourcePath)) {
                                clsNotFound.Append("<div>" + relativePath + "</div>");
                            } else {
                                foreach (var deployNeeded in ProEnv.Deployer.GetTargetDirsNeededForFile(sourcePath, 0)) {
                                    string outputRPath;

                                    if (ProEnv.CompileLocally) {
                                        // deploy the *.r file next to his source
                                        outputRPath = Path.Combine(deployNeeded.TargetDir, Path.GetFileName(rCodeFile));
                                    } else {
                                        // deploy the *.r file in the compilation directory (create the needed subdirectories...)
                                        outputRPath = Path.Combine(deployNeeded.TargetDir, relativePath);
                                    }

                                    // add .r and .lst (if needed) to the list of files to move
                                    outputList.Add(deployNeeded.Set(compiledFile.InputPath, rCodeFile, outputRPath));

                                    if (Path.GetFileNameWithoutExtension(relativePath).Equals(Path.GetFileNameWithoutExtension(compiledFile.InputPath))) {
                                        if (CompileWithListing)
                                            outputList.Add(deployNeeded.Copy(compiledFile.InputPath, compiledFile.CompOutputLis, Path.ChangeExtension(outputRPath, ExtLis)));
                                        if (CompileWithXref)
                                            outputList.Add(deployNeeded.Copy(compiledFile.InputPath, compiledFile.CompOutputXrf, Path.ChangeExtension(outputRPath, UseXmlXref ? ExtXrfXml : ExtXrf)));
                                        if (CompileWithDebugList)
                                            outputList.Add(deployNeeded.Copy(compiledFile.InputPath, compiledFile.CompOutputDbg, Path.ChangeExtension(outputRPath, ExtDbg)));
                                    }
                                }
                            }
                        }
                    } catch (Exception e) {
                        ErrorHandler.LogError(e);
                    }
                } else {
                    foreach (var deployNeeded in ProEnv.Deployer.GetTargetDirsNeededForFile(compiledFile.InputPath, 0)) {

                        // add .r and .lst (if needed) to the list of files to deploy
                        outputList.Add(deployNeeded.Set(compiledFile.InputPath, compiledFile.CompOutputR, Path.Combine(deployNeeded.TargetDir, compiledFile.BaseFileName + ExtR)));

                        if (CompileWithListing)
                            outputList.Add(deployNeeded.Copy(compiledFile.InputPath, compiledFile.CompOutputLis, Path.Combine(deployNeeded.TargetDir, compiledFile.BaseFileName + ExtLis)));
                        if (CompileWithXref)
                            outputList.Add(deployNeeded.Copy(compiledFile.InputPath, compiledFile.CompOutputXrf, Path.Combine(deployNeeded.TargetDir, compiledFile.BaseFileName + (UseXmlXref ? ExtXrfXml : ExtXrf))));
                        if (CompileWithDebugList)
                            outputList.Add(deployNeeded.Copy(compiledFile.InputPath, compiledFile.CompOutputDbg, Path.Combine(deployNeeded.TargetDir, compiledFile.BaseFileName + ExtDbg)));
                    }
                }
            }

            if (clsNotFound.Length > 0)
                UserCommunication.Notify("Couldn't locate the source file (.cls) for :" + clsNotFound + " in the PROPATH", MessageImg.MsgError, "Post compilation error", "File not found");

            return outputList;
        }

        #endregion
    }
    
    internal class ProExecutionGenerateDebugfile : ProExecutionHandleCompilation {
        public override ExecutionType ExecutionType { get { return ExecutionType.GenerateDebugfile; } }

        protected override bool CanUseBatchMode() {
            return true;
        }
    }
    
    internal class ProExecutionCheckSyntax : ProExecutionHandleCompilation {
        public override ExecutionType ExecutionType { get { return ExecutionType.CheckSyntax; } }

        protected override bool CanUseBatchMode() {
            return true;
        }
    }
    
    internal class ProExecutionCompile : ProExecutionHandleCompilation {

        public override ExecutionType ExecutionType { get { return ExecutionType.Compile; } }

        protected override string CheckParameters() {
            if (!ProEnv.CompileLocally && !Path.IsPathRooted(ProEnv.BaseCompilationPath)) {
                return "The path for the compilation base directory is incorrect : <div class='ToolTipcodeSnippet'>" + (string.IsNullOrEmpty(ProEnv.BaseCompilationPath) ? "it's empty!" : ProEnv.BaseCompilationPath) + "</div>You must provide a valid path before executing this action :<br><br><i>1. Either change the compilation directory<br>2. Or toggle the option to compile next to the source file!<br><br>The options are configurable in the <a href='go'>set environment page</a></i>";
            }
            return base.CheckParameters();
        }

        protected override bool CanUseBatchMode() {
            return true;
        }
    }

    internal class ProExecutionRun : ProExecutionHandleCompilation {

        public override ExecutionType ExecutionType { get { return ExecutionType.Run; } }

        private static bool _dontWarnAboutRCode;

        protected override bool SetExecutionInfo() {

            if (!base.SetExecutionInfo())
                return false;

            _processStartDir = Path.GetDirectoryName(Files.First().InputPath) ?? _localTempDir;

            // when running a procedure, check that a .r is not hiding the program, if that's the case we warn the user
            if (!_dontWarnAboutRCode) {
                if (File.Exists(Path.ChangeExtension(Files.First().InputPath, ".r"))) {
                    UserCommunication.NotifyUnique("rcodehide", "Friendly warning, an <b>r-code</b> <i>(i.e. *.r file)</i> is hiding the current program<br>If you modified it since the last compilation you might not have the expected behavior...<br><br><i>" + "stop".ToHtmlLink("Click here to not show this message again for this session") + "</i>", MessageImg.MsgWarning, "Progress execution", "An Rcode hides the program", args => {
                        _dontWarnAboutRCode = true;
                        UserCommunication.CloseUniqueNotif("rcodehide");
                    }, 5);
                }
            }

            return true;
        }

    }

    internal class ProExecutionProlint : ProExecutionHandleCompilation {

        public override ExecutionType ExecutionType { get { return ExecutionType.Prolint; } }

        private string _prolintOutputPath;

        protected override bool SetExecutionInfo() {

            if (!base.SetExecutionInfo())
                return false;

            // prolint, we need to copy the StartProlint program
            var fileToExecute = "prolint_" + DateTime.Now.ToString("yyMMdd_HHmmssfff") + ".p";
            _prolintOutputPath = Path.Combine(_localTempDir, "prolint.log");

            StringBuilder prolintProgram = new StringBuilder();
            prolintProgram.AppendLine("&SCOPED-DEFINE PathFileToProlint " + Files.First().CompInputPath.ProQuoter());
            prolintProgram.AppendLine("&SCOPED-DEFINE PathProlintOutputFile " + _prolintOutputPath.ProQuoter());
            prolintProgram.AppendLine("&SCOPED-DEFINE PathToStartProlintProgram " + Config.FileStartProlint.ProQuoter());
            prolintProgram.AppendLine("&SCOPED-DEFINE UserName " + Config.Instance.UserName.ProQuoter());
            prolintProgram.AppendLine("&SCOPED-DEFINE PathActualFilePath " + Files.First().InputPath.ProQuoter());
            var filename = Npp.CurrentFile.FileName;
            if (FileTag.Contains(filename)) {
                var fileInfo = FileTag.GetLastFileTag(filename);
                prolintProgram.AppendLine("&SCOPED-DEFINE FileApplicationName " + fileInfo.ApplicationName.ProQuoter());
                prolintProgram.AppendLine("&SCOPED-DEFINE FileApplicationVersion " + fileInfo.ApplicationVersion.ProQuoter());
                prolintProgram.AppendLine("&SCOPED-DEFINE FileWorkPackage " + fileInfo.WorkPackage.ProQuoter());
                prolintProgram.AppendLine("&SCOPED-DEFINE FileBugID " + fileInfo.BugId.ProQuoter());
                prolintProgram.AppendLine("&SCOPED-DEFINE FileCorrectionNumber " + fileInfo.CorrectionNumber.ProQuoter());
                prolintProgram.AppendLine("&SCOPED-DEFINE FileDate " + fileInfo.CorrectionDate.ProQuoter());
            }
            var encoding = TextEncodingDetect.GetFileEncoding(Config.FileStartProlint);
            Utils.FileWriteAllText(Path.Combine(_localTempDir, fileToExecute), Utils.ReadAllText(Config.FileStartProlint, encoding).Replace(@"/*<inserted_3P_values>*/", prolintProgram.ToString()), encoding);

            SetPreprocessedVar("CurrentFilePath", fileToExecute.ProQuoter());

            return true;
        }

        protected override Dictionary<string, List<FileError>> GetErrorsList(Dictionary<string, string> changePaths) {
            var treatedFile = Files.First();
            if (!changePaths.ContainsKey(treatedFile.CompInputPath))
                changePaths.Add(treatedFile.CompInputPath, treatedFile.InputPath);
            return FilesInfo.ReadErrorsFromFile(_prolintOutputPath, true, changePaths);
        }
    }

    internal class ProExecutionDatabase : ProExecution {

        public override ExecutionType ExecutionType { get { return ExecutionType.Database; } }

        public string ExtractDbOutputPath { get; set; }

        protected override bool SetExecutionInfo() {

            ExtractDbOutputPath = Path.Combine(_localTempDir, "db.extract");
            SetPreprocessedVar("ExtractDbOutputPath", ExtractDbOutputPath.ProQuoter());

            var fileToExecute = "db_" + DateTime.Now.ToString("yyMMdd_HHmmssfff") + ".p";
            if (!Utils.FileWriteAllBytes(Path.Combine(_localTempDir, fileToExecute), DataResources.DumpDatabase))
                return false;
            SetPreprocessedVar("CurrentFilePath", fileToExecute.ProQuoter());

            return true;
        }
        
        protected override bool CanUseBatchMode() {
            return true;
        }
    }

    internal class ProExecutionAppbuilder : ProExecution {

        public override ExecutionType ExecutionType { get { return ExecutionType.Appbuilder; } }

        public string CurrentFile { get; set; }

        protected override bool SetExecutionInfo() {

            SetPreprocessedVar("CurrentFilePath", CurrentFile.ProQuoter());

            return true;
        }
    }

    internal class ProExecutionDictionary : ProExecution {
        public override ExecutionType ExecutionType { get { return ExecutionType.Dictionary; } }
    }

    internal class ProExecutionDataDigger : ProExecution {
        public override ExecutionType ExecutionType { get { return ExecutionType.DataDigger; } }

        protected override bool SetExecutionInfo() {

            // need to init datadigger?
            bool needUpdate = !Config.Instance.InstalledDataDiggerVersion.IsHigherOrEqualVersionThan(Config.EmbeddedDataDiggerVersion);

            if (needUpdate) {
                // check the version installed (maybe datadigger updated itself to a higher version)
                var versionFile = Path.Combine(Config.FolderDataDigger, "version.i");
                if (File.Exists(versionFile)) {
                    var realVersion = Utils.ReadAllText(versionFile, Encoding.Default).Trim();
                    Config.Instance.InstalledDataDiggerVersion = realVersion;
                    needUpdate = !Config.Instance.InstalledDataDiggerVersion.IsHigherOrEqualVersionThan(Config.EmbeddedDataDiggerVersion);
                }
            }

            if (needUpdate || !File.Exists(Path.Combine(Config.FolderDataDigger, "DataDigger.p"))) {
                if (!Utils.FileWriteAllBytes(Path.Combine(Config.FolderDataDigger, "DataDigger.zip"), DataResources.DataDigger))
                    return false;
                if (!Utils.ExtractAll(Path.Combine(Config.FolderDataDigger, "DataDigger.zip"), Config.FolderDataDigger))
                    return false;
                if (needUpdate) {
                    if (string.IsNullOrEmpty(Config.Instance.InstalledDataDiggerVersion))
                        UserCommunication.Notify("A new version of datadigger has been installed : " + Config.EmbeddedDataDiggerVersion + "<br><br>Check out the release notes " + Config.DataDiggerVersionUrl.ToHtmlLink("here"), MessageImg.MsgInfo, "DataDigger updated", "To " + Config.EmbeddedDataDiggerVersion, 5);
                    Config.Instance.InstalledDataDiggerVersion = Config.EmbeddedDataDiggerVersion;
                }
            }
            // add the datadigger folder to the propath
            _propath = Config.FolderDataDigger + "," + _propath;
            
            return true;
        }

        protected override void AppendProgressParameters(StringBuilder sb) {
            sb.Append(" -basekey \"INI\" -s 10000 -d dmy -E -rereadnolock -h 255 -Bt 4000 -tmpbsize 8 ");
        }
    }

    internal class ProExecutionDataReader : ProExecutionDataDigger {
        public override ExecutionType ExecutionType { get { return ExecutionType.DataReader; } }
    }

    internal class ProExecutionDbAdmin : ProExecution {
        public override ExecutionType ExecutionType { get { return ExecutionType.DbAdmin; } }
    }

    internal class ProExecutionProDesktop : ProExecution {
        public override ExecutionType ExecutionType { get { return ExecutionType.ProDesktop; } }
    }

    internal class ProExecutionDeploymentHook : ProExecution {

        public override ExecutionType ExecutionType { get { return ExecutionType.DeploymentHook; } }

        protected override bool SetExecutionInfo() {

            var fileToExecute = "hook_" + DateTime.Now.ToString("yyMMdd_HHmmssfff") + ".p";
            StringBuilder hookProc = new StringBuilder();
            hookProc.AppendLine("&SCOPED-DEFINE ApplicationName " + ProEnv.Name.ProQuoter());
            hookProc.AppendLine("&SCOPED-DEFINE ApplicationSuffix " + ProEnv.Suffix.ProQuoter());
            hookProc.AppendLine("&SCOPED-DEFINE StepNumber " + DeploymentStep);
            hookProc.AppendLine("&SCOPED-DEFINE SourceDirectory " + DeploymentSourcePath.ProQuoter());
            hookProc.AppendLine("&SCOPED-DEFINE DeploymentDirectory " + ProEnv.BaseCompilationPath.ProQuoter());
            var encoding = TextEncodingDetect.GetFileEncoding(Config.FileDeploymentHook);
            Utils.FileWriteAllText(Path.Combine(_localTempDir, fileToExecute), Utils.ReadAllText(Config.FileDeploymentHook, encoding).Replace(@"/*<inserted_3P_values>*/", hookProc.ToString()), encoding);

            SetPreprocessedVar("CurrentFilePath", fileToExecute.ProQuoter());

            return true;
        }

        public string DeploymentSourcePath { get; set; }

        public int DeploymentStep { get; set; }
    }

    internal enum ExecutionType {
        CheckSyntax = 0,
        Compile = 1,
        Run = 2,
        GenerateDebugfile = 3,
        Prolint = 4,

        Database = 10,
        Appbuilder = 11,
        Dictionary = 12,
        DataDigger = 13,
        DataReader = 14,
        DbAdmin = 15,
        ProDesktop = 16,
        DeploymentHook = 17
    }

    internal class FileToCompile {
        // stores the path
        public string InputPath { get; set; }

        // stores temporary path used during the compilation
        public string CompInputPath { get; set; }
        public string CompOutputDir { get; set; }
        public string CompOutputR { get; set; }
        public string CompOutputXrf { get; set; }
        public string CompOutputLis { get; set; }
        public string CompOutputDbg { get; set; }

        public string BaseFileName { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public FileToCompile(string inputPath) {
            InputPath = inputPath;
            BaseFileName = Path.GetFileNameWithoutExtension(inputPath);
        }
    }
}