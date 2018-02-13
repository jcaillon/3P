#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
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
using _3PA.MainFeatures.Pro.Deploy;
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

        protected string _propathFilePath;

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
                {"PropathFilePath", "\"\""},
                {"DbConnectString", "\"\""},
                {"ExecutionType", "\"\""},
                {"CurrentFilePath", "\"\""},
                {"OutputPath", "\"\""},
                {"ToCompileListFile", "\"\""},
                {"AnalysisMode", "false"},
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
            _propath = (_localTempDir + "," + string.Join(",", ProEnv.GetProPathDirList)).Trim().Trim(',') + "\r\n";
            _propathFilePath = Path.Combine(_localTempDir, "progress.propath");
            Utils.FileWriteAllText(_propathFilePath, _propath, Encoding.Default);

            // Set info
            if (!SetExecutionInfo())
                return false;

            SetPreprocessedVar("ExecutionType", ExecutionType.ToString().ToUpper().PreProcQuoter());
            SetPreprocessedVar("LogPath", _logPath.PreProcQuoter());
            SetPreprocessedVar("PropathFilePath", _propathFilePath.PreProcQuoter());
            SetPreprocessedVar("DbConnectString", ProEnv.ConnectionString.PreProcQuoter());
            SetPreprocessedVar("DbLogPath", _dbLogPath.PreProcQuoter());
            SetPreprocessedVar("DbConnectionMandatory", NeedDatabaseConnection.ToString());
            SetPreprocessedVar("NotificationOutputPath", _notifPath.PreProcQuoter());
            SetPreprocessedVar("PreExecutionProgram", ProEnv.PreExecutionProgram.Trim().PreProcQuoter());
            SetPreprocessedVar("PostExecutionProgram", ProEnv.PostExecutionProgram.Trim().PreProcQuoter());

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
            _exeParameters.Append(" -p " + _runnerPath.Quoter());
            if (!string.IsNullOrWhiteSpace(ProEnv.CmdLineParameters))
                _exeParameters.Append(" " + ProEnv.CmdLineParameters.Trim());
            AppendProgressParameters(_exeParameters);

            // start the process
            try {
                StartProcess();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e);
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
        /// Return false to cancel the start of the process
        /// </summary>
        protected virtual bool SetExecutionInfo() {
            return true;
        }

        /// <summary>
        /// Add stuff to the command line
        /// </summary>
        protected virtual void AppendProgressParameters(StringBuilder sb) {
            if (!string.IsNullOrEmpty(_tempInifilePath))
                sb.Append(" -ininame " + _tempInifilePath.Quoter() + " -basekey " + "INI".Quoter());
        }

        #endregion

        #region private methods

        /// <summary>
        /// Allows to clean the temporary directories
        /// </summary>
        public virtual void Clean() {
            try {
                if (_process != null)
                    _process.Close();

                // delete temp dir
                if (_localTempDir != null) {
                    try {
                        Directory.Delete(_localTempDir, true);
                    } catch (Exception) {
                        // ignored
                    }
                }

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
            try {
                if (ExecutionFailed || (ConnectionFailed && NeedDatabaseConnection)) {
                    if (OnExecutionFailed != null) {
                        OnExecutionFailed(this);
                    }
                } else {
                    if (OnExecutionOk != null) {
                        OnExecutionOk(this);
                    }
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e);
            }

            // end of execution action
            try { 
                if (OnExecutionEnd != null) {
                    OnExecutionEnd(this);
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e);
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

    #region ProExecutionProVersion

    internal class ProExecutionProVersion : ProExecution {

        public override ExecutionType ExecutionType { get { return ExecutionType.ProVersion; } }

        public string ProVersion { get { return Utils.ReadAllText(_outputPath, Encoding.Default); } }

        private string _outputPath;

        protected override bool SetExecutionInfo() {

            _outputPath = Path.Combine(_localTempDir, "pro.version");
            SetPreprocessedVar("OutputPath", _outputPath.PreProcQuoter());

            return true;
        }

        protected override void AppendProgressParameters(StringBuilder sb) {
            sb.Clear();
            _exeParameters.Append(" -b -p " + _runnerPath.Quoter());
        }

        protected override bool CanUseBatchMode() {
            return true;
        }
    }

    #endregion

    #region ProExecutionDatabase

    /// <summary>
    /// Allows to output a file containing the structure of the database
    /// </summary>
    internal class ProExecutionDatabase : ProExecution {

        #region Properties

        public override ExecutionType ExecutionType { get { return ExecutionType.Database; } }

        /// <summary>
        /// File to the output path that contains the structure of the database
        /// </summary>
        public string OutputPath { get; set; }

        #endregion

        #region Override

        protected override bool SetExecutionInfo() {

            OutputPath = Path.Combine(_localTempDir, "db.extract");
            SetPreprocessedVar("OutputPath", OutputPath.PreProcQuoter());

            var fileToExecute = "db_" + DateTime.Now.ToString("yyMMdd_HHmmssfff") + ".p";
            if (!Utils.FileWriteAllBytes(Path.Combine(_localTempDir, fileToExecute), DataResources.DumpDatabase))
                return false;
            SetPreprocessedVar("CurrentFilePath", fileToExecute.PreProcQuoter());

            return true;
        }
        
        protected override bool CanUseBatchMode() {
            return true;
        }

        #endregion
    }

    #endregion

    #region ProExecutionTableCrc

    /// <summary>
    /// Allows to output a file containing the structure of the database
    /// </summary>
    internal class ProExecutionTableCrc : ProExecution {

        #region Properties

        public override ExecutionType ExecutionType { get { return ExecutionType.TableCrc; } }

        /// <summary>
        /// File to the output path that contains the CRC of each table
        /// </summary>
        public string OutputPath { get; set; }

        #endregion

        #region Override

        protected override bool SetExecutionInfo() {

            OutputPath = Path.Combine(_localTempDir, "db.extract");
            SetPreprocessedVar("OutputPath", OutputPath.PreProcQuoter());

            var fileToExecute = "db_" + DateTime.Now.ToString("yyMMdd_HHmmssfff") + ".p";
            if (!Utils.FileWriteAllBytes(Path.Combine(_localTempDir, fileToExecute), DataResources.DumpTableCrc))
                return false;
            SetPreprocessedVar("CurrentFilePath", fileToExecute.PreProcQuoter());

            return true;
        }

        protected override bool CanUseBatchMode() {
            return true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get a list with all the tables + CRC
        /// </summary>
        /// <returns></returns>
        public List<TableCrc> GetTableCrc() {
            var output = new List<TableCrc>();
            Utils.ForEachLine(OutputPath, new byte[0], (i, line) => {
                var split = line.Split('\t');
                if (split.Length == 2) {
                    output.Add(new TableCrc {
                        QualifiedTableName = split[0],
                        Crc = split[1]
                    });
                }
            }, Encoding.Default);
            return output;
        }

        #endregion
    }

    #endregion

    #region ProExecutionAppbuilder

    internal class ProExecutionAppbuilder : ProExecution {

        public override ExecutionType ExecutionType { get { return ExecutionType.Appbuilder; } }

        public string CurrentFile { get; set; }

        protected override bool SetExecutionInfo() {

            SetPreprocessedVar("CurrentFilePath", CurrentFile.PreProcQuoter());

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

            if (!Updater<DataDiggerUpdaterWrapper>.Instance.LocalVersion.IsHigherVersionThan("v0")) {
                UserCommunication.NotifyUnique("NeedDataDigger",
                    "The DataDigger installation folder could not be found in 3P.<br>This is normal if it is the first time that you are using this feature.<br><br>" + "download".ToHtmlLink("Please click here to download the latest release of DataDigger automatically") + "<br><br><i>You will be informed when it is installed and you will be able to use this feature immediately after.</i>",
                    MessageImg.MsgQuestion, "DataDigger execution", "DataDigger installation not found", args => {
                        if (args.Link.Equals("download")) {
                            args.Handled = true;
                            Updater<DataDiggerUpdaterWrapper>.Instance.CheckForUpdate();
                            UserCommunication.CloseUniqueNotif("NeedDataDigger");
                        }
                    });
                return false;
            }

            // add the datadigger folder to the propath
            _propath = Config.DataDiggerFolder + "," + _propath;
            _processStartDir = Config.DataDiggerFolder;

            return true;
        }

        protected override void AppendProgressParameters(StringBuilder sb) {
            sb.Append(" -basekey \"INI\" -s 10000 -d dmy -E -rereadnolock -h 255 -Bt 4000 -tmpbsize 8 ");
            sb.Append(" -T " + _localTempDir.Trim('\\').Quoter());
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
            hookProc.AppendLine("&SCOPED-DEFINE ApplicationName " + ProEnv.Name.PreProcQuoter());
            hookProc.AppendLine("&SCOPED-DEFINE ApplicationSuffix " + ProEnv.Suffix.PreProcQuoter());
            hookProc.AppendLine("&SCOPED-DEFINE StepNumber " + DeploymentStep);
            hookProc.AppendLine("&SCOPED-DEFINE SourceDirectory " + DeploymentSourcePath.PreProcQuoter());
            hookProc.AppendLine("&SCOPED-DEFINE DeploymentDirectory " + ProEnv.BaseCompilationPath.PreProcQuoter());
            var encoding = TextEncodingDetect.GetFileEncoding(Config.FileDeploymentHook);
            Utils.FileWriteAllText(Path.Combine(_localTempDir, fileToExecute), Utils.ReadAllText(Config.FileDeploymentHook, encoding).Replace(@"/*<inserted_3P_values>*/", hookProc.ToString()), encoding);

            SetPreprocessedVar("CurrentFilePath", fileToExecute.PreProcQuoter());

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
        TableCrc = 19,
    }

    #endregion

}