#region header

// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (UoeDatabaseAdministrator.cs) is part of Oetools.Utilities.
//
// Oetools.Utilities is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Oetools.Utilities is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Oetools.Utilities. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using _3PA.Lib;
using _3PA.MainFeatures.Pro;

namespace _3PA.MainFeatures.Parser.Pro.Parse {
    /// <summary>
    /// Administrate an openedge database.
    /// </summary>
    public class UoePreprocessedExpressionEvaluator : IDisposable {
        private static readonly Dictionary<string, bool> ExpressionResults = new Dictionary<string, bool>();

        /// <summary>
        /// Returns true if the given pre-processed expression evaluates to true.
        /// </summary>
        /// <param name="preprocExpression"></param>
        /// <param name="definedFunc"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static bool IsExpressionTrue(string preprocExpression, Func<string, int> definedFunc = null) {
            preprocExpression = preprocExpression.Trim().ToLower();
            preprocExpression = ReplaceDefinedFunction(preprocExpression, definedFunc, out bool usedDefinedProc);
            
            if (ExpressionResults.ContainsKey(preprocExpression)) {
                return ExpressionResults[preprocExpression];
            }

            if (CanEvaluateFromString(preprocExpression, out bool result)) {
                if (!usedDefinedProc) { // defined() depends on the current context (which var is defined at this line), so don't store.
                    ExpressionResults.Add(preprocExpression, result);
                }
                return result;
            }
            
            if (!string.IsNullOrEmpty(ProEnvironment.Current?.ProwinExePath)) {
                var dlcPath = Path.Combine(Path.GetDirectoryName(ProEnvironment.Current.ProwinExePath) ?? "", "..");
                using (var ev = new UoePreprocessedExpressionEvaluator(dlcPath)) {
                    result = ev.IsTrue(preprocExpression);
                    ExpressionResults.Add(preprocExpression, result);
                    return result;
                }
            }

            throw new Exception($"Could not evaluate expression: {preprocExpression}.");
        }

        /// <summary>
        /// Path to the openedge installation folder
        /// </summary>
        protected string DlcPath { get; }

        private UoeProcessIo _progres;

        private UoeProcessIo Progres {
            get {
                if (_progres == null) {
                    _progres = new UoeProcessIo(DlcPath, true, Encoding.Default);
                }

                return _progres;
            }
        }

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="dlcPath"></param>
        public UoePreprocessedExpressionEvaluator(string dlcPath) {
            DlcPath = dlcPath;
            if (string.IsNullOrEmpty(dlcPath) || !Directory.Exists(dlcPath)) {
                throw new ArgumentException($"Invalid dlc path {dlcPath.Quoter()}.");
            }
        }

        /// <inheritdoc />
        public void Dispose() {
            _progres?.Dispose();
            _progres = null;
        }

        /// <summary>
        /// Returns true if the given pre-processed expression evaluates to true.
        /// </summary>
        /// <param name="preProcExpression"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool IsTrue(string preProcExpression) {
            var content = $"&IF {preProcExpression} &THEN\nPUT UNFORMATTED \"true\".\n&ELSE\nPUT UNFORMATTED \"false\".\n&ENDIF";
            var procedurePath = Path.Combine(Config.FolderTemp, $"preproc_eval_{Path.GetRandomFileName()}.p");
            File.WriteAllText(procedurePath, content, Encoding.Default);

            try {
                var args = new ProcessArgs().Append("-p").Append(procedurePath);
                Progres.WorkingDirectory = Config.FolderTemp;
                var executionOk = Progres.TryExecute(args);
                if (!executionOk || !bool.TryParse(Progres.StandardOutput.ToString(), out bool result)) {
                    throw new Exception(Progres.BatchOutputString);
                }
                return result;
            } finally {
                File.Delete(procedurePath);
            }
        }

        public static bool CanEvaluateFromString(string preProcExpression, out bool isExpressionTrue) {
            if (preProcExpression.Equals("true", StringComparison.Ordinal)) {
                isExpressionTrue = true;
                return true;
            }

            if (preProcExpression.Equals("false", StringComparison.Ordinal)) {
                isExpressionTrue = false;
                return true;
            }
            
            if (int.TryParse(preProcExpression, out int result)) {
                isExpressionTrue = result > 0;
                return true;
            }           
                
            var splitEqual = preProcExpression.Split('=');
            if (splitEqual.Length == 2 && splitEqual[0].TrimEnd().Equals(splitEqual[1].TrimStart(), StringComparison.Ordinal)) {
                isExpressionTrue = true;
                return true;
            }
            
            
            isExpressionTrue = false;
            return false;
        }

        public static string ReplaceDefinedFunction(string preProcExpression, Func<string, int> definedFunc, out bool usedDefinedProc) {
            usedDefinedProc = false;
                
            // DEFINED(EXCLUDE-btGetSessionNomPasoeCourant) = 0
            if (definedFunc != null && preProcExpression.StartsWith("defined(", StringComparison.Ordinal)) {
                var closingParenthesisIdx = preProcExpression.IndexOf(')', 8);
                if (closingParenthesisIdx > 0) {
                    var varName = preProcExpression.Substring(8, closingParenthesisIdx - 8).Trim();
                    preProcExpression = $"{definedFunc($"&{varName}")}{preProcExpression.Substring(closingParenthesisIdx + 1)}";
                    usedDefinedProc = true;
                }
            }
            
            return preProcExpression;
        }

        /// <summary>
        /// Represents a progress process
        /// </summary>
        /// <remarks>
        /// - progress returns an exit different of 0 only if it actually failed to start,
        /// if your procedure return error or quit, it is still an exit code of 0
        /// - in batch mode (-b) and GUI mode, even if we set CreateNoWindow and WindowStyle to Hidden,
        /// the window still appears in the taskbar. All the code between #if WINDOWSONLYBUILD in this class
        /// is made to hide this window from the taskbar in that case
        /// </remarks>
        private class UoeProcessIo : ProcessIoAsync {
            /// <summary>
            /// DLC path to use
            /// </summary>
            public string DlcPath { get; }

            /// <summary>
            /// Whether or not to use character mode (_progres) instead of GUI (prowin)
            /// </summary>
            public bool UseCharacterMode { get; }

            /// <summary>
            /// Whether or not the executable can use the -nosplash parameter
            /// </summary>
            public bool CanUseNoSplash { get; }

            /// <summary>
            /// Constructor
            /// </summary>
            public UoeProcessIo(string dlcPath, bool useCharacterModeOfProgress, Encoding redirectedOutputEncoding = null) : base(null) {
                DlcPath = dlcPath;
                UseCharacterMode = useCharacterModeOfProgress;
                CanUseNoSplash = false;
                ExecutablePath = GetProExecutableFromDlc(DlcPath, UseCharacterMode);
                RedirectedOutputEncoding = redirectedOutputEncoding;
            }

            /// <inheritdoc />
            protected override bool WaitUntilProcessExits(int timeoutMs) {
                RestoreSplashScreen();
                return base.WaitUntilProcessExits(timeoutMs);
            }

            /// <inheritdoc />
            protected override void PrepareStart(ProcessArgs arguments, bool silent) {
                if (silent) {
                    arguments.Append("-b");
                }

                if (!UseCharacterMode) {
                    if (CanUseNoSplash) {
                        arguments.Append("-nosplash");
                    } else {
                        DisableSplashScreen();
                    }
                }

                // we can only redirect output in -b batch mode
                RedirectOutput = silent;

                base.PrepareStart(arguments, silent);

                // in character mode, we need to execute _progress in a console!
                if (UseCharacterMode && !silent) {
                    _startInfo.UseShellExecute = true;
                }
            }

            protected override void ProcessOnExited(object sender, EventArgs e) {
                base.ProcessOnExited(sender, e);
                RestoreSplashScreen();
            }

            private void DisableSplashScreen() {
                try {
                    File.Move(Path.Combine(DlcPath, "bin", "splashscreen.bmp"), Path.Combine(DlcPath, "bin", "splashscreen-disabled.bmp"));
                } catch (Exception) {
                    // if it fails it is not really a problem
                }
            }

            private void RestoreSplashScreen() {
                try {
                    File.Move(Path.Combine(DlcPath, "bin", "splashscreen-disabled.bmp"), Path.Combine(DlcPath, "bin", "splashscreen.bmp"));
                } catch (Exception) {
                    // if it fails it is not really a problem
                }
            }

            /// <summary>
            /// Returns the pro executable full path from the dlc path
            /// </summary>
            /// <param name="dlcPath"></param>
            /// <param name="useCharacterModeOfProgress"></param>
            /// <exception cref="Exception">invalid mode (gui/char) or path not found</exception>
            /// <returns></returns>
            public static string GetProExecutableFromDlc(string dlcPath, bool useCharacterModeOfProgress = false) {
                string outputPath;

                outputPath = Path.Combine(dlcPath, "bin", useCharacterModeOfProgress ? "_progres.exe" : "prowin32.exe");
                if (!File.Exists(outputPath)) {
                    if (useCharacterModeOfProgress) {
                        throw new Exception($"Could not find the progress executable for character mode in {dlcPath}, check your DLC path or switch to graphical mode; the file searched was {outputPath}.");
                    }

                    outputPath = Path.Combine(dlcPath, "bin", "prowin.exe");
                }

                if (!File.Exists(outputPath)) {
                    throw new Exception($"Could not find the progress executable in {dlcPath}, check your DLC path; the file searched was {outputPath}.");
                }

                return outputPath;
            }
        }

        /// <summary>
        /// Wrapper for async process.
        /// </summary>
        private class ProcessIoAsync : ProcessIo, IDisposable {
            public ProcessIoAsync(string executablePath) : base(executablePath) { }

            /// <summary>
            /// Start the process, use <see cref="ProcessIo.OnProcessExit"/> event to know when the process is done
            /// </summary>
            ///  <param name="arguments">Each argument is expected to be quoted if necessary and double quotes escaped with a second double quote (use quoter).</param>
            /// <param name="silent"></param>
            public void ExecuteAsync(ProcessArgs arguments = null, bool silent = true) {
                ExecuteAsyncProcess(arguments, silent);
            }

            /// <summary>
            /// Wait for a process to end
            /// Returns true if the process has exited (can be false if timeout was reached)
            /// </summary>
            /// <param name="timeoutMs"></param>
            public bool WaitForExit(int timeoutMs = 0) {
                return WaitUntilProcessExits(timeoutMs);
            }

            public void Dispose() {
                _process?.Close();
                _process?.Dispose();
                _process = null;
            }
        }

        /// <summary>
        /// Wrapper around process.
        /// </summary>
        private class ProcessIo {
            /// <summary>
            /// The full path to the executable used
            /// </summary>
            public string ExecutablePath { get; set; }

            /// <summary>
            /// Subscribe to this event called when the process exits
            /// </summary>
            public event EventHandler<EventArgs> OnProcessExit;

            /// <summary>
            /// The working directory to use for this process
            /// </summary>
            public string WorkingDirectory { get; set; }

            /// <summary>
            /// Choose to redirect the standard/error output or no, default to true
            /// </summary>
            public bool RedirectOutput { get; set; } = true;

            /// <summary>
            /// Choose the encoding for the standard/error output
            /// </summary>
            public Encoding RedirectedOutputEncoding { get; set; }

            /// <summary>
            /// Cancellation token.
            /// </summary>
            public CancellationToken? CancelToken { get; set; }

            /// <summary>
            /// Use <see cref="string.Trim()"/> on each output line.
            /// </summary>
            public bool TrimOutputLine { get; set; }

            /// <summary>
            /// Standard output, to be called after the process exits
            /// </summary>
            public StringBuilder StandardOutput {
                get {
                    if (_standardOutput == null || _process != null && !_process.HasExited) {
                        _standardOutput = new StringBuilder();
                        foreach (var s in StandardOutputArray) {
                            _standardOutput.AppendLine(TrimOutputLine ? s.Trim() : s);
                        }

                        _standardOutput.TrimEnd();
                    }

                    return _standardOutput;
                }
            }

            /// <summary>
            /// Error output, to be called after the process exits
            /// </summary>
            public StringBuilder ErrorOutput {
                get {
                    if (_errorOutput == null || _process != null && !_process.HasExited) {
                        _errorOutput = new StringBuilder();
                        foreach (var s in ErrorOutputArray) {
                            _errorOutput.AppendLine(TrimOutputLine ? s.Trim() : s);
                        }

                        _errorOutput.TrimEnd();
                    }

                    return _errorOutput;
                }
            }

            /// <summary>
            /// Returns all the messages sent to the standard or error output, should be used once the process has exited
            /// </summary>
            public StringBuilder BatchOutput {
                get {
                    if (_batchModeOutput == null || _process != null && !_process.HasExited) {
                        _batchModeOutput = new StringBuilder();
                        foreach (var s in ErrorOutputArray) {
                            _batchModeOutput.AppendLine(TrimOutputLine ? s.Trim() : s);
                        }

                        _batchModeOutput.TrimEnd();

                        foreach (var s in StandardOutputArray) {
                            _batchModeOutput.AppendLine(TrimOutputLine ? s.Trim() : s);
                        }

                        _batchModeOutput.TrimEnd();
                    }

                    return _batchModeOutput;
                }
            }

            /// <summary>
            /// Returns all the messages sent to the standard or error output, should be used once the process has exited
            /// </summary>
            public string BatchOutputString => _batchOutputString ?? (_batchOutputString = BatchOutput.ToString());

            /// <summary>
            /// Standard output, to be called after the process exits
            /// </summary>
            public List<string> StandardOutputArray { get; private set; } = new List<string>();

            /// <summary>
            /// Error output, to be called after the process exits
            /// </summary>
            public List<string> ErrorOutputArray { get; private set; } = new List<string>();

            /// <summary>
            /// Returns the command line used for the execution.
            /// </summary>
            public string ExecutedCommandLine => $"{ProcessArgs.ToCliArg(ExecutablePath)} {UsedArguments}";

            /// <summary>
            /// The complete arguments used to start the process.
            /// </summary>
            public string UsedArguments => _startInfo?.Arguments;

            private int? _exitCode;

            /// <summary>
            /// Exit code of the process
            /// </summary>
            public int ExitCode {
                get {
                    if (!_exitCode.HasValue && _process != null) {
                        _process.WaitForExit();
                        _exitCode = _process.ExitCode;
                    }

                    return _exitCode ?? 0;
                }
                set { _exitCode = value; }
            }

            /// <summary>
            /// Whether or not this process has been killed
            /// </summary>
            public bool Killed { get; private set; }

            protected ProcessStartInfo _startInfo;

            protected Process _process;

            private string _batchOutputString;
            private StringBuilder _standardOutput;
            private StringBuilder _batchModeOutput;
            private StringBuilder _errorOutput;

            private bool _exitedEventPublished;
            private CancellationTokenRegistration? _cancelRegistration;

            /// <summary>
            /// Constructor
            /// </summary>
            public ProcessIo(string executablePath) {
                ExecutablePath = executablePath;
            }

            /// <summary>
            /// Start the process synchronously, catch the exceptions
            /// </summary>
            /// <param name="arguments">Each argument is expected to be quoted if necessary and double quotes escaped with a second double quote (use quoter).</param>
            /// <param name="silent"></param>
            /// <returns></returns>
            public virtual bool TryExecute(ProcessArgs arguments = null, bool silent = true) {
                try {
                    return Execute(arguments, silent) && ErrorOutputArray.Count == 0;
                } catch (Exception e) {
                    ErrorOutputArray.Add(e.ToString());
                    return false;
                }
            }

            /// <summary>
            /// Start the process synchronously
            /// </summary>
            /// <param name="arguments">Each argument is expected to be quoted if necessary and double quotes escaped with a second double quote (use quoter).</param>
            /// <param name="silent"></param>
            /// <param name="timeoutMs"></param>
            /// <returns></returns>
            public virtual bool Execute(ProcessArgs arguments = null, bool silent = true, int timeoutMs = 0) {
                ExecuteAsyncProcess(arguments, silent);

                if (!WaitUntilProcessExits(timeoutMs)) {
                    return false;
                }

                return ExitCode == 0;
            }

            /// <summary>
            /// Start the process asynchronously, use <see cref="OnProcessExit"/> event to know when the process is done
            /// </summary>
            /// <param name="arguments">Each argument is expected to be quoted if necessary and double quotes escaped with a second double quote (use quoter).</param>
            /// <param name="silent"></param>
            protected virtual void ExecuteAsyncProcess(ProcessArgs arguments = null, bool silent = true) {
                PrepareStart(arguments, silent);

                _cancelRegistration = CancelToken?.Register(OnCancellation);
                _process.Start();

                if (RedirectOutput) {
                    // Asynchronously read the standard output of the spawned process
                    // This raises OutputDataReceived events for each line of output
                    _process.BeginOutputReadLine();
                    _process.BeginErrorReadLine();
                }
            }

            private void OnCancellation() {
                Kill();
            }

            /// <summary>
            /// Kill the process
            /// </summary>
            public void Kill() {
                Killed = true;
                if (!_process?.HasExited ?? false) {
                    _process?.Kill();
                }
            }

            /// <summary>
            /// Returns true if the process has exited (can be false if timeout was reached)
            /// </summary>
            /// <param name="timeoutMs"></param>
            /// <returns></returns>
            protected virtual bool WaitUntilProcessExits(int timeoutMs) {
                if (_process == null) {
                    return true;
                }

                if (timeoutMs > 0) {
                    var exited = _process.WaitForExit(timeoutMs);
                    if (!exited) {
                        return false;
                    }
                } else {
                    _process.WaitForExit();
                }

                ExitCode = _process.ExitCode;

                _process?.Close();
                _process?.Dispose();
                _process = null;

                return true;
            }

            protected virtual void PrepareStart(ProcessArgs arguments, bool silent) {
                _exitedEventPublished = false;
                StandardOutputArray.Clear();
                _standardOutput = null;
                ErrorOutputArray.Clear();
                _errorOutput = null;
                _batchModeOutput = null;
                _batchOutputString = null;
                Killed = false;
                ExitCode = 0;

                _startInfo = new ProcessStartInfo {
                    FileName = ExecutablePath, UseShellExecute = false
                };

                if (arguments != null) {
                    _startInfo.Arguments = arguments.ToCliArgs();
                }

                if (!string.IsNullOrEmpty(WorkingDirectory)) {
                    _startInfo.WorkingDirectory = WorkingDirectory;
                }

                if (silent) {
                    _startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    _startInfo.CreateNoWindow = true;
                }

                if (RedirectOutput) {
                    _startInfo.RedirectStandardError = true;
                    _startInfo.RedirectStandardOutput = true;
                    if (RedirectedOutputEncoding != null) {
                        _startInfo.StandardErrorEncoding = RedirectedOutputEncoding;
                        _startInfo.StandardOutputEncoding = RedirectedOutputEncoding;
                    }
                }

                _process = new Process {
                    StartInfo = _startInfo
                };

                if (RedirectOutput) {
                    _process.OutputDataReceived += OnProcessOnOutputDataReceived;
                    _process.ErrorDataReceived += OnProcessOnErrorDataReceived;
                }

                if (OnProcessExit != null) {
                    _process.EnableRaisingEvents = true;
                    _process.Exited += ProcessOnExited;
                }
            }

            protected virtual void OnProcessOnErrorDataReceived(object sender, DataReceivedEventArgs args) {
                if (!string.IsNullOrEmpty(args.Data)) {
                    ErrorOutputArray.Add(args.Data);
                }
            }

            protected virtual void OnProcessOnOutputDataReceived(object sender, DataReceivedEventArgs args) {
                if (!string.IsNullOrEmpty(args.Data)) {
                    StandardOutputArray.Add(args.Data);
                }
            }

            protected virtual void ProcessOnExited(object sender, EventArgs e) {
                if (!_exitedEventPublished) {
                    // this boolean does not seem useful but i have seen weird behaviors where the
                    // exited event is called twice when we WaitForExit(), better safe than sorry
                    _exitedEventPublished = true;
                    _cancelRegistration?.Dispose();
                    OnProcessExit?.Invoke(sender, e);
                }
            }
        }

        /// <summary>
        /// A collection of arguments for a process.
        /// The main point of this class is to provide a clean way to feed arguments to a <see cref="ProcessIo"/>.
        /// Using this class as an input instead of a string, we are able to control how the arguments are sent to the called program and
        /// escape them correctly depending on the executing platform.
        /// Also, there is no need to worry about how to write the argument string prior to using it in a <see cref="ProcessIo"/>.
        /// </summary>
        private class ProcessArgs : IEnumerable<string> {
            protected IList<string> items = new List<string>();

            /// <summary>
            /// New process args.
            /// </summary>
            public ProcessArgs() { }

            /// <summary>
            /// Append a new argument.
            /// </summary>
            /// <param name="arg"></param>
            /// <returns></returns>
            public virtual ProcessArgs Append(string arg) {
                if (arg != null) {
                    items.Add(arg);
                }

                return this;
            }

            /// <summary>
            /// Append a collection of arguments.
            /// </summary>
            /// <param name="processArgs"></param>
            /// <returns></returns>
            public virtual ProcessArgs Append(ProcessArgs processArgs) {
                if (processArgs != null) {
                    foreach (var arg in processArgs) {
                        Append(arg);
                    }
                }

                return this;
            }

            /// <summary>
            /// Append one or more arguments.
            /// </summary>
            /// <param name="args"></param>
            /// <returns></returns>
            public virtual ProcessArgs Append(params object[] args) {
                if (args != null) {
                    foreach (var o in args.Where(o => o != null)) {
                        switch (o) {
                            case ProcessArgs processArgs:
                                Append(processArgs);
                                break;
                            case string stringArg:
                                Append(stringArg);
                                break;
                            default:
                                Append(o.ToString());
                                break;
                        }
                    }
                }

                return this;
            }

            /// <summary>
            /// Append an array of arguments.
            /// </summary>
            /// <param name="args"></param>
            /// <returns></returns>
            public virtual ProcessArgs Append(string[] args) {
                if (args != null) {
                    foreach (var stringArg in args) {
                        Append(stringArg);
                    }
                }

                return this;
            }

            /// <summary>
            /// Prepare a string representing the arguments of a command line application.
            /// Format each argument so that it is correctly interpreted by the receiving program.
            /// </summary>
            /// <returns></returns>
            public string ToCliArgs() {
                return string.Join(" ", this.Where(a => a != null).Select(a => ToCliArg(a)));
            }

            /// <summary>
            /// String representation of those arguments. Arguments with spaces are quoted.
            /// </summary>
            /// <returns></returns>
            public override string ToString() => string.Join(" ", this.Where(a => a != null).Select(a => ToCliArg(a)));

            /// <inheritdoc />
            public IEnumerator<string> GetEnumerator() => items.GetEnumerator();

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /// <summary>
            /// Prepare a string representing an argument of a cmd line interface so that it is interpreted as a single argument.
            /// Uses double quote when the string contains whitespaces.
            /// </summary>
            /// <param name="text"></param>
            /// <param name="isWindows"></param>
            /// <remarks>
            /// In linux, you escape double quotes with \"
            /// In windows, you escape doubles quotes with \" ("" is also correct but we don't do that here)
            /// In linux, you need to escape \ with \\ but not in windows
            /// </remarks>
            /// <returns></returns>
            public static string ToCliArg(string text, bool? isWindows = null) {
                if (text == null) {
                    return null;
                }

                if (string.IsNullOrEmpty(text)) {
                    return @"""""";
                }

                var hasWhiteSpaces = false;

                var sb = new StringBuilder();

                // https://blogs.msdn.microsoft.com/twistylittlepassagesallalike/2011/04/23/everyone-quotes-command-line-arguments-the-wrong-way/
                var textLength = text.Length;
                for (int i = 0; i < textLength; ++i) {
                    var backslashes = 0;

                    // Consume all backslashes
                    while (i < textLength && text[i] == '\\') {
                        backslashes++;
                        i++;
                    }

                    if (i == textLength) {
                        if (hasWhiteSpaces) {
                            // Escape any backslashes at the end of the arg when the argument is also quoted.
                            // This ensures the outside quote is interpreted as an argument delimiter
                            sb.Append('\\', 2 * backslashes);
                        } else {
                            // At then end of the arg, which isn't quoted,
                            // just add the backslashes, no need to escape
                            sb.Append('\\', backslashes);
                        }
                    } else if (text[i] == '"') {
                        // Escape any preceding backslashes and the quote
                        sb.Append('\\', 2 * backslashes + 1);
                        sb.Append('"');
                    } else {
                        if (char.IsWhiteSpace(text[i])) {
                            hasWhiteSpaces = true;
                        }

                        // Output any consumed backslashes and the character
                        sb.Append('\\', backslashes);
                        sb.Append(text[i]);
                    }
                }

                return hasWhiteSpaces ? $"\"{sb.Append('"')}" : sb.ToString();
            }
        }
    }
}