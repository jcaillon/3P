using System;
using System.Diagnostics;
using System.Text;

namespace _3PA.Lib {

    public class ProcessIo {

        #region public fields

        public string Arguments { get; set; }

        public StringBuilder StandardOutput { get; private set; }

        public StringBuilder ErrorOutput { get; private set; }

        public int ExitCode { get; private set; }

        public ProcessStartInfo StartInfo { get; set; }

        #endregion

        #region private fields

        private Process _process;

        private int _nbExecution;

        #endregion

        #region Life and death

        /// <summary>
        /// Constructor
        /// </summary>
        public ProcessIo(string executable) {

            StandardOutput = new StringBuilder();
            ErrorOutput = new StringBuilder();

            StartInfo = new ProcessStartInfo {
                FileName = executable,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };
        }

        /// <summary>
        /// Desctructor
        /// </summary>
        ~ProcessIo() {
            Kill();
            Close();
            try {
                _process.Dispose();
            } catch (Exception) {
                // ignored
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Start the process synchronously, catch the exceptions
        /// </summary>
        public bool TryDoWait(bool hidden = false) {
            try {
                return DoWait(hidden);
            } catch (Exception e) {
                ErrorOutput.AppendLine(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Start the process synchronously
        /// </summary>
        public bool DoWait(bool hidden = false) {

            if (hidden) {
                StartInfo.CreateNoWindow = true;
                StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }

            if (!string.IsNullOrEmpty(Arguments))
                StartInfo.Arguments = Arguments;

            if (_nbExecution > 0)
                Kill(); 

            _nbExecution++;

            if (_process == null) {
                _process = new Process {
                    StartInfo = StartInfo
                };
                _process.OutputDataReceived += (sender, args) => StandardOutput.AppendLine(args.Data);
                _process.ErrorDataReceived += (sender, args) => ErrorOutput.AppendLine(args.Data);
            }

            _process.Start();
            _process.WaitForExit();

            ExitCode = _process.ExitCode;

            return ExitCode == 0;
        }

        public void Kill() {
            try {
                _process.Kill();
            } catch (Exception) {
                //ignored
            }
        }

        public void Close() {
            try {
                _process.Close();
            } catch (Exception) {
                //ignored
            }
        }

        #endregion

    }
}
