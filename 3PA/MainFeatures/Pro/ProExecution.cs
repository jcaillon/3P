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
using System.ComponentModel;
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

namespace _3PA.MainFeatures.Pro {
    
    #region ProExecution

    /// <summary>
    /// Base class for all the progress execution (i.e. when we need to start a prowin process and do something)
    /// </summary>
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
                case ExecutionType.GenerateDebugfile:
                    return new ProExecutionGenerateDebugfile();
                default:
                    throw new Exception("Factory : the type " + executionType + " does not exist");
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// The action to execute just after the end of a prowin process
        /// </summary>
        public event Action<ProExecution> OnExecutionEnd;

        /// <summary>
        /// The action to execute at the end of the process if it went well = we found a .log and the database is connected or is not mandatory
        /// </summary>
        public event Action<ProExecution> OnExecutionOk;

        /// <summary>
        /// The action to execute at the end of the process if something went wrong (no .log or database down)
        /// </summary>
        public event Action<ProExecution> OnExecutionFailed;

        #endregion
        
        #region Options

        /// <summary>
        /// set to true if a valid database connection is mandatory (the compilation will not be done if a db can't be connected
        /// </summary>
        public bool NeedDatabaseConnection { get; set; }

        /// <summary>
        /// Set to true to not use the batch mode
        /// </summary>
        public bool NoBatch { get; set; }

        #endregion

        #region Properties
        
        /// <summary>
        /// Copy of the pro env to use
        /// </summary>
        public ProEnvironment.ProEnvironmentObject ProEnv { get; private set; }

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
        /// Execution type of the current class
        /// </summary>
        public virtual ExecutionType ExecutionType { get { return ExecutionType.CheckSyntax; } }

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

        protected bool _useBatchMode;

        protected string _runnerPath;

        #endregion

        #region Life and death

        /// <summary>
        /// Deletes temp directory and everything in it
        /// </summary>
        ~ProExecution() {
            Clean();
        }

        /// <summary>
        /// Construct with the current env
        /// </summary>
        public ProExecution() : this (null) {}

        public ProExecution(ProEnvironment.ProEnvironmentObject proEnv) {

            ProEnv = proEnv == null ? new ProEnvironment.ProEnvironmentObject(ProEnvironment.Current) : proEnv;

            _preprocessedVars = new Dictionary<string, string> {
                {"LogPath", "\"\""},
                {"DbLogPath", "\"\""},
                {"PropathToUse", "\"\""},
                {"DbConnectString", "\"\""},
                {"ExecutionType", "\"\""},
                {"CurrentFilePath", "\"\""},
                {"OutputPath", "\"\""},
                {"ToCompileListFile", "\"\""},
                {"CompileProgressionFile", "\"\""},
                {"DbConnectionMandatory", "false"},
                {"NotificationOutputPath", "\"\""},
                {"PreExecutionProgram", "\"\""},
                {"PostExecutionProgram", "\"\""},
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
        public bool Start() {

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
            SetPreprocessedVar("DbConnectString", ProEnv.ConnectionString.ProQuoter());
            SetPreprocessedVar("DbLogPath", _dbLogPath.ProQuoter());
            SetPreprocessedVar("DbConnectionMandatory", NeedDatabaseConnection.ToString());
            SetPreprocessedVar("NotificationOutputPath", _notifPath.ProQuoter());
            SetPreprocessedVar("PreExecutionProgram", ProEnv.PreExecutionProgram.Trim().ProQuoter());
            SetPreprocessedVar("PostExecutionProgram", ProEnv.PostExecutionProgram.Trim().ProQuoter());

            // prepare the .p runner
            _runnerPath = Path.Combine(_localTempDir, "run_" + DateTime.Now.ToString("HHmmssfff") + ".p");
            StringBuilder runnerProgram = new StringBuilder();
            foreach (var @var in _preprocessedVars) {
                runnerProgram.AppendLine("&SCOPED-DEFINE " + @var.Key + " " + @var.Value);
            }
            runnerProgram.Append(Encoding.Default.GetString(DataResources.ProgressRun));
            Utils.FileWriteAllText(_runnerPath, runnerProgram.ToString(), Encoding.Default);
            
            // no batch mode option?
            _useBatchMode = !Config.Instance.NeverUseProwinInBatchMode && !NoBatch && CanUseBatchMode();

            // Parameters
            _exeParameters = new StringBuilder();
           if (_useBatchMode) {
                _exeParameters.Append(" -b");
            } else {
                // we suppress the splashscreen
                if (ProEnv.CanProwinUseNoSplash) {
                    _exeParameters.Append(" -nosplash");
                } else {
                    MoveSplashScreenNoError(Path.Combine(Path.GetDirectoryName(ProEnv.ProwinPath) ?? "", "splashscreen.bmp"), Path.Combine(Path.GetDirectoryName(ProEnv.ProwinPath) ?? "", "splashscreen-3p-disabled.bmp"));
                }
            }
            _exeParameters.Append(" -p " + _runnerPath.ProQuoter());
            if (!string.IsNullOrWhiteSpace(ProEnv.CmdLineParameters))
                _exeParameters.Append(" " + ProEnv.CmdLineParameters.Trim());
            AppendProgressParameters(_exeParameters);

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

            //UserCommunication.Notify("New process starting...<br><br><b>FileName :</b><br>" + ProEnv.ProwinPath + "<br><br><b>Parameters :</b><br>" + _exeParameters + "<br><br><b>Temporary directory :</b><br><a href='" + _localTempDir + "'>" + _localTempDir + "</a>");

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

        protected virtual void Clean() {
            try {
                if (_process != null)
                    _process.Close();

                // delete temp dir
                if (_localTempDir != null)
                    Utils.DeleteDirectory(_localTempDir, true);

                // restore splashscreen
                if (!string.IsNullOrEmpty(ProEnv.ProwinPath))
                    MoveSplashScreenNoError(Path.Combine(Path.GetDirectoryName(ProEnv.ProwinPath) ?? "", "splashscreen-3p-disabled.bmp"), Path.Combine(Path.GetDirectoryName(ProEnv.ProwinPath) ?? "", "splashscreen.bmp"));

            } catch (Exception) {
                // dont care
            }
        }

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
        protected virtual void StartProcess() {
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
            try {

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

                } else if (new FileInfo(_logPath).Length > 0) {
                    // else if the log isn't empty, something went wrong

                    var logContent = Utils.ReadAllText(_logPath, Encoding.Default).Trim();
                    if (!string.IsNullOrEmpty(logContent)) {
                        UserCommunication.NotifyUnique("ExecutionFailed", "An error occurred in the progress execution, details :<div class='ToolTipcodeSnippet'>" + logContent + "</div>", MessageImg.MsgError, "Progress execution", "Critical error", null, 0, 600);
                        ExecutionFailed = true;
                    }
                }

                // if the db log file exists, then the connect statement failed, warn the user
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

                // display a custom post execution notification if needed
                DisplayPostExecutionNotification();

            } finally {

                PublishExecutionEndEvents();
            }
        }

        /// <summary>
        /// publish the end of execution events
        /// </summary>
        protected virtual void PublishExecutionEndEvents() {

            // end of successful/unsuccessful execution action
            if (ExecutionFailed || (ConnectionFailed && NeedDatabaseConnection)) {
                if (OnExecutionFailed != null) {
                    OnExecutionFailed(this);
                }
            } else {
                if (OnExecutionOk != null) {
                    OnExecutionOk(this);
                }
            }

            // end of execution action
            if (OnExecutionEnd != null) {
                OnExecutionEnd(this);
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

        /// <summary>
        /// move a file, catch the errors
        /// </summary>
        private void MoveSplashScreenNoError(string from, string to) {
            if (File.Exists(from)) {
                try {
                    File.Move(from, to);
                } catch (Exception) {
                    // if it fails it is not really a problem
                }
            }
        }

        #endregion

    }

    #endregion

    #region ProExecutionHandleCompilation

    internal abstract class ProExecutionHandleCompilation : ProExecution {

        #region Static events

        /// <summary>
        /// The action to execute at the end of the compilation if it went well
        /// - the list of all the files that needed to be compiled,
        /// - the errors for each file compiled (if any)
        /// - the list of all the deployments needed for the files compiled (move the .r but also .dbg and so on...)
        /// </summary>
        public static event Action<ProExecutionHandleCompilation, List<FileToCompile>, Dictionary<string, List<FileError>>, List<FileToDeploy>> OnEachCompilationOk;

        #endregion        

        #region Events

        /// <summary>
        /// The action to execute at the end of the compilation if it went well. It sends :
        /// - the list of all the files that needed to be compiled,
        /// - the errors for each file compiled (if any)
        /// - the list of all the deployments needed for the files compiled (move the .r but also .dbg and so on...)
        /// </summary>
        public event Action<ProExecutionHandleCompilation, List<FileToCompile>, Dictionary<string, List<FileError>>, List<FileToDeploy>> OnCompilationOk;

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

        public bool CompileWithDebugList { get; set; }

        public bool CompileWithListing { get; set; }

        public bool CompileWithXref { get; set; }

        public bool UseXmlXref { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Temp directory located in the deployment dir
        /// </summary>
        public string DistantTempDir { get; private set; }

        #endregion

        #region Constants

        public const string ExtR = ".r";
        public const string ExtDbg = ".dbg";
        public const string ExtLis = ".lis";
        public const string ExtXrf = ".xrf";
        public const string ExtXrfXml = ".xrf.xml";
        public const string ExtCls = ".cls";

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

            DistantTempDir = Path.Combine(ProEnv.BaseCompilationPath, "~3p-tmp-" + DateTime.Now.ToString("HHmmss") + "-" + Path.GetRandomFileName());
        }

        #endregion

        #region Override

        protected override void Clean() {
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

                // we set where the *.lst and *.r files will be generated by the COMPILE command
                var baseFileName = Path.GetFileNameWithoutExtension(fileToCompile.SourcePath);
                var lastDeployment = ProEnv.Deployer.GetTargetsNeededForFile(fileToCompile.SourcePath, 0).Last();

                // for *.cls files, as many *.r files are generated, we need to compile in a temp directory
                // we need to know which *.r files were generated for each input file
                // so each file gets his own sub tempDir
                if ((lastDeployment.DeployType != DeployType.Move) ||
                    Config.Instance.CompileForceUseOfTemp ||
                    Path.GetExtension(fileToCompile.SourcePath).Equals(ExtCls)) {
                    var subTempDir = Path.Combine(_localTempDir, count.ToString());

                    // if the deployment dir is not on the same disk as the temp folder, we create a temp dir
                    // as close to the final deployment as possible (= in the deployment base dir!)
                    if (lastDeployment.DeployType != DeployType.Ftp && !string.IsNullOrEmpty(ProEnv.BaseCompilationPath) && ProEnv.BaseCompilationPath.Length > 2 && !ProEnv.BaseCompilationPath.Substring(0, 2).EqualsCi(_localTempDir.Substring(0, 2))) {
                        subTempDir = Path.Combine(DistantTempDir, count.ToString());
                        if (!Utils.CreateDirectory(DistantTempDir, FileAttributes.Hidden))
                            return false;
                    }
                    
                    fileToCompile.CompilationOutputDir = subTempDir;
                } else {
                    // if we want to move the r-code somewhere during the deployment, then we will compile the r-code
                    // directly there, because it's faster than generating it in a temp folder and moving it afterward
                    fileToCompile.CompilationOutputDir = lastDeployment.TargetPath;
                }

                fileToCompile.CompOutputR = Path.Combine(fileToCompile.CompilationOutputDir, baseFileName + ExtR);
                if (CompileWithListing)
                    fileToCompile.CompOutputLis = Path.Combine(fileToCompile.CompilationOutputDir, baseFileName + ExtLis);
                if (CompileWithXref)
                    fileToCompile.CompOutputXrf = Path.Combine(fileToCompile.CompilationOutputDir, baseFileName + (UseXmlXref ? ExtXrfXml : ExtXrf));
                if (CompileWithDebugList)
                    fileToCompile.CompOutputDbg = Path.Combine(fileToCompile.CompilationOutputDir, baseFileName + ExtDbg);

                // if current file and the file has unsaved modif, we copy the content to a temp file, otherwise we just use the input path (also use the input path for .cls files!)
                if (fileToCompile.SourcePath.Equals(Npp.CurrentFile.Path) &&
                    (Sci.GetModify || (fileToCompile.BaseFileName ?? "").StartsWith("_")) &&
                    !Path.GetExtension(fileToCompile.SourcePath).Equals(ExtCls)) {

                    fileToCompile.CompiledSourcePath = Path.Combine(_localTempDir, Path.GetFileName(fileToCompile.SourcePath));
                    Utils.FileWriteAllText(fileToCompile.CompiledSourcePath, Sci.Text, Encoding.Default);
                } else {
                    fileToCompile.CompiledSourcePath = fileToCompile.SourcePath;
                }

                if (!Utils.CreateDirectory(fileToCompile.CompilationOutputDir))
                    return false;

                // feed files list
                filesListcontent.AppendLine(fileToCompile.CompiledSourcePath.ProQuoter() + " " + fileToCompile.CompilationOutputDir.ProQuoter() + " " + (fileToCompile.CompOutputLis ?? "?").ProQuoter() + " " + (fileToCompile.CompOutputXrf ?? "?").ProQuoter() + " " + (fileToCompile.CompOutputDbg ?? "?").ProQuoter());

                count++;
            }
            var filesListPath = Path.Combine(_localTempDir, "files.list");
            Utils.FileWriteAllText(filesListPath, filesListcontent.ToString(), Encoding.Default);
            
            _progressionFilePath = Path.Combine(_localTempDir, "compile.progression");
            _compilationLog = Path.Combine(_localTempDir, "compilation.log");

            SetPreprocessedVar("ToCompileListFile", filesListPath.ProQuoter());
            SetPreprocessedVar("CompileProgressionFile", _progressionFilePath.ProQuoter());
            SetPreprocessedVar("OutputPath", _compilationLog.ProQuoter());

            return base.SetExecutionInfo();
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
            
            // end of successful/unsuccessful execution action
            if (!ExecutionFailed && (!ConnectionFailed || !NeedDatabaseConnection)) {

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

                if (OnCompilationOk != null) {
                    OnCompilationOk(this, Files, errorsList, deployList);
                }

                if (OnEachCompilationOk != null) {
                    OnEachCompilationOk(this, Files, errorsList, deployList);
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
                return unchecked((int) (File.Exists(_progressionFilePath) ? (new FileInfo(_progressionFilePath)).Length : 0));
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

            Utils.ForEachLine(fullPath, null, (i, line) => {
                var fields = line.Split('\t').ToList();
                if (fields.Count == 8) {
                    // new file
                    // the path of the file that triggered the compiler error, it can be empty so we make sure to set it
                    var compilerFailPath = String.IsNullOrEmpty(fields[1]) ? fields[0] : fields[1];
                    var filePath = (permutePaths.ContainsKey(compilerFailPath) ? permutePaths[compilerFailPath] : compilerFailPath);
                    if (!output.ContainsKey(filePath)) {
                        output.Add(filePath, new List<FileError>());
                        lastLineNbCouple = new[] { -10, -10 };
                    }

                    ErrorLevel errorLevel;
                    if (!Enum.TryParse(fields[2], true, out errorLevel))
                        errorLevel = ErrorLevel.Error;

                    // we store the line/error number couple because we don't want two identical messages to appear
                    var thisLineNbCouple = new[] { (int)fields[3].ConvertFromStr(typeof(int)), (int)fields[5].ConvertFromStr(typeof(int)) };

                    if (thisLineNbCouple[0] == lastLineNbCouple[0] && thisLineNbCouple[1] == lastLineNbCouple[1]) {
                        // same line/error number as previously
                        if (output[filePath].Count > 0) {
                            var lastFileError = output[filePath].Last();
                            if (lastFileError != null)
                                lastFileError.Times = (lastFileError.Times == 0) ? 2 : lastFileError.Times + 1;
                        }
                        return;
                    }
                    lastLineNbCouple = thisLineNbCouple;

                    var baseFileName = Path.GetFileName(filePath);

                    // add error
                    output[filePath].Add(new FileError {
                        SourcePath = filePath,
                        Level = errorLevel,
                        Line = Math.Max(0, lastLineNbCouple[0] - 1),
                        Column = Math.Max(0, (int)fields[4].ConvertFromStr(typeof(int)) - 1),
                        ErrorNumber = lastLineNbCouple[1],
                        Message = fields[6].Replace("<br>", "\n").Replace(compilerFailPath, baseFileName).Replace(filePath, baseFileName).Trim(),
                        Help = fields[7].Replace("<br>", "\n").Trim(),
                        FromProlint = fromProlint,
                        CompiledFilePath = (permutePaths.ContainsKey(fields[0]) ? permutePaths[fields[0]] : fields[0])
                    });
                }
            });

            return output;
        }

        #endregion
    }
    
    #region FileToCompile

    internal class FileToCompile {

        /// <summary>
        /// The path to the source that needs to be compiled
        /// </summary>
        public string SourcePath { get; set; }

        /// <summary>
        /// Size of the file to compile (set in constructor)
        /// </summary>
        public long Size { get; private set; }

        // stores temporary path used during the compilation
        public string CompiledSourcePath { get; set; }
        public string CompilationOutputDir { get; set; }
        public string CompOutputR { get; set; }
        public string CompOutputXrf { get; set; }
        public string CompOutputLis { get; set; }
        public string CompOutputDbg { get; set; }

        /// <summary>
        /// Returns the base file name (set in constructor)
        /// </summary>
        public string BaseFileName { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public FileToCompile(string sourcePath) {
            SourcePath = sourcePath;
            BaseFileName = Path.GetFileNameWithoutExtension(sourcePath);
            try {
                Size = new FileInfo(sourcePath).Length;
            } catch (Exception) {
                Size = 0;
            }
        }
    }

    #endregion

    #region FileError

    /// <summary>
    /// Errors found for this file, either from compilation or from prolint
    /// </summary>
    internal class FileError {

        /// <summary>
        /// Path of the file in which we found the error
        /// </summary>
        public string SourcePath { get; set; }
        public ErrorLevel Level { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public int ErrorNumber { get; set; }
        public string Message { get; set; }
        public string Help { get; set; }
        public bool FromProlint { get; set; }

        /// <summary>
        /// indicates if the error appears several times
        /// </summary>
        public int Times { get; set; }

        /// <summary>
        /// The path to the file that was compiled to generate this error (you can compile a .p and have the error on a .i)
        /// </summary>
        public string CompiledFilePath { get; set; }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("<div>");
            sb.Append("<img height='15px' src='"); sb.Append(Level > ErrorLevel.StrongWarning ? "MsgError" : "MsgWarning"); sb.Append("'>");
            if (!CompiledFilePath.Equals(SourcePath)) {
                sb.Append("in "); sb.Append(SourcePath.ToHtmlLink(Path.GetFileName(SourcePath))); sb.Append(", ");
            }
            sb.Append((SourcePath + "|" + Line).ToHtmlLink("line " + (Line + 1))); sb.Append(" (n°" + ErrorNumber + ")");
            if (Times > 0) {
                sb.Append(" (x" + Times + ")");
            }
            sb.Append(" " + Message);
            sb.Append("</div>");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Describes the error level, the num is also used for MARKERS in scintilla
    /// and thus must start at 0
    /// </summary>
    internal enum ErrorLevel {
        [Description("Error(s), good!")]
        NoErrors,
        
        [Description("Info")]
        Information,

        [Description("Warning(s)")]
        Warning,

        [Description("Huge warning(s)")]
        StrongWarning,

        [Description("Error(s)")]
        Error, // while compiling, from this level, the file doesn't compile

        [Description("Critical error(s)!")]
        Critical
    }

    #endregion

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

        /// <summary>
        /// Construct with the current env
        /// </summary>
        public ProExecutionCompile() : this(null) {}

        public ProExecutionCompile(ProEnvironment.ProEnvironmentObject proEnv) : base(proEnv) {}

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
        /// Allows to format a small text to explain the errors found in a file and the generated files...
        /// </summary>
        public static string FormatCompilationResultForSingleFile(string sourceFilePath, List<FileError> listErrorFiles, List<FileToDeploy> listDeployedFiles) {
            var line = new StringBuilder();

            line.Append("<div style='padding-bottom: 5px;'><b>" + sourceFilePath.ToHtmlLink(Path.GetFileName(sourceFilePath), true) + "</b> in " + Path.GetDirectoryName(sourceFilePath).ToHtmlLink() + "</div>");
            line.Append("<div style='padding-left: 10px'>");

            if (listErrorFiles != null) {
                foreach (var fileError in listErrorFiles) {
                    line.Append(fileError);
                }
            }

            if (listDeployedFiles != null) {
                foreach (var fileToDeploy in listDeployedFiles) {
                    line.Append(fileToDeploy);
                }
            }

            line.Append("</div>");

            return line.ToString();
        }
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

        protected override bool SetExecutionInfo() {

            if (!base.SetExecutionInfo())
                return false;

            // prolint, we need to copy the StartProlint program
            var fileToExecute = "prolint_" + DateTime.Now.ToString("yyMMdd_HHmmssfff") + ".p";
            _prolintOutputPath = Path.Combine(_localTempDir, "prolint.log");

            StringBuilder prolintProgram = new StringBuilder();
            prolintProgram.AppendLine("&SCOPED-DEFINE PathFileToProlint " + Files.First().CompiledSourcePath.ProQuoter());
            prolintProgram.AppendLine("&SCOPED-DEFINE PathProlintOutputFile " + _prolintOutputPath.ProQuoter());
            prolintProgram.AppendLine("&SCOPED-DEFINE PathToStartProlintProgram " + Config.FileStartProlint.ProQuoter());
            prolintProgram.AppendLine("&SCOPED-DEFINE UserName " + Config.Instance.UserName.ProQuoter());
            prolintProgram.AppendLine("&SCOPED-DEFINE PathActualFilePath " + Files.First().SourcePath.ProQuoter());
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
            if (!changePaths.ContainsKey(treatedFile.CompiledSourcePath))
                changePaths.Add(treatedFile.CompiledSourcePath, treatedFile.SourcePath);
            return ReadErrorsFromFile(_prolintOutputPath, true, changePaths);
        }
    }

    #endregion

    #region ProExecutionProVersion

    internal class ProExecutionProVersion : ProExecution {

        public override ExecutionType ExecutionType { get { return ExecutionType.ProVersion; } }

        public string ProVersion { get { return Utils.ReadAllText(_outputPath, Encoding.Default); } }

        private string _outputPath;

        protected override bool SetExecutionInfo() {

            _outputPath = Path.Combine(_localTempDir, "pro.version");
            SetPreprocessedVar("OutputPath", _outputPath.ProQuoter());

            return true;
        }

        protected override void AppendProgressParameters(StringBuilder sb) {
            sb.Clear();
            _exeParameters.Append(" -b -p " + _runnerPath.ProQuoter());
        }

        protected override bool CanUseBatchMode() {
            return true;
        }
    }

    #endregion

    #region ProExecutionDatabase

    internal class ProExecutionDatabase : ProExecution {

        public override ExecutionType ExecutionType { get { return ExecutionType.Database; } }

        public string OutputPath { get; set; }

        protected override bool SetExecutionInfo() {

            OutputPath = Path.Combine(_localTempDir, "db.extract");
            SetPreprocessedVar("OutputPath", OutputPath.ProQuoter());

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

    #endregion

    #region ProExecutionAppbuilder

    internal class ProExecutionAppbuilder : ProExecution {

        public override ExecutionType ExecutionType { get { return ExecutionType.Appbuilder; } }

        public string CurrentFile { get; set; }

        protected override bool SetExecutionInfo() {

            SetPreprocessedVar("CurrentFilePath", CurrentFile.ProQuoter());

            return true;
        }
    }

    #endregion

    #region ProExecutionDictionary

    internal class ProExecutionDictionary : ProExecution {
        public override ExecutionType ExecutionType { get { return ExecutionType.Dictionary; } }
    }

    #endregion

    #region ProExecutionDataDigger

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
                    if (string.IsNullOrEmpty(Config.Instance.InstalledDataDiggerVersion)) {
                        UserCommunication.Notify("A new version of datadigger has been installed : " + Config.EmbeddedDataDiggerVersion + "<br><br>Check out the release notes " + Config.DataDiggerVersionUrl.ToHtmlLink("here"), MessageImg.MsgInfo, "DataDigger updated", "To " + Config.EmbeddedDataDiggerVersion, 5);
                    }
                    Config.Instance.InstalledDataDiggerVersion = Config.EmbeddedDataDiggerVersion;
                    try {
                        // delete all previous r code
                        foreach (FileInfo file in new DirectoryInfo(Config.FolderDataDigger).GetFiles("*.r", SearchOption.TopDirectoryOnly)) {
                            File.Delete(file.FullName);
                        }
                    } catch(Exception) {
                        // ignored
                    }
                }
            }
            // add the datadigger folder to the propath
            _propath = Config.FolderDataDigger + "," + _propath;
            _processStartDir = Config.FolderDataDigger;

            return true;
        }

        protected override void AppendProgressParameters(StringBuilder sb) {
            sb.Append(" -basekey \"INI\" -s 10000 -d dmy -E -rereadnolock -h 255 -Bt 4000 -tmpbsize 8 ");
            sb.Append(" -T " + _localTempDir.Trim('\\').ProQuoter());
        }
    }

    #endregion

    #region ProExecutionDataReader

    internal class ProExecutionDataReader : ProExecutionDataDigger {
        public override ExecutionType ExecutionType { get { return ExecutionType.DataReader; } }
    }

    #endregion

    #region ProExecutionDbAdmin

    internal class ProExecutionDbAdmin : ProExecution {
        public override ExecutionType ExecutionType { get { return ExecutionType.DbAdmin; } }
    }

    #endregion

    #region ProExecutionProDesktop

    internal class ProExecutionProDesktop : ProExecution {
        public override ExecutionType ExecutionType { get { return ExecutionType.ProDesktop; } }
    }

    #endregion

    #region ProExecutionDeploymentHook

    internal class ProExecutionDeploymentHook : ProExecution {

        /// <summary>
        /// Construct with the current env
        /// </summary>
        public ProExecutionDeploymentHook() : this(null) { }

        public ProExecutionDeploymentHook(ProEnvironment.ProEnvironmentObject proEnv) : base(proEnv) { }

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

    #endregion

    #region ExecutionType

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
        DeploymentHook = 17,
        ProVersion = 18,
    }

    #endregion

}