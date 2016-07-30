using System;
using System.Diagnostics;
using System.Text;

namespace _3PA.Lib {

    public class ProcessIo {

        #region public fields

        public Process Process { get; private set; }

        public string Arguments { get; set; }

        public StringBuilder StandardOutput { get; private set; }

        public StringBuilder ErrorOutput { get; private set; }

        public ProcessStartInfo StartInfo { get; set; }

        #endregion

        #region private fields

        private int _nbExecution;

        #endregion

        #region Life and death

        public ProcessIo(string executable) {
            StandardOutput = new StringBuilder();
            ErrorOutput = new StringBuilder();

            StartInfo = new ProcessStartInfo {
                FileName = executable,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            Process = new Process {
                StartInfo = StartInfo
            };
            Process.OutputDataReceived += (sender, args) => StandardOutput.AppendLine(args.Data);
            Process.ErrorDataReceived += (sender, args) => ErrorOutput.AppendLine(args.Data);
        }

        ~ProcessIo() {
            Kill();
            try {
                Process.Dispose();
            } catch (Exception) {
                // ignored
            }
        }

        #endregion

        #region public methods

        public bool TryDoWait(bool silent = false) {
            try {
                return DoWait(silent);
            } catch (Exception e) {
                ErrorOutput.AppendLine(e.Message);
                return false;
            }
        }

        public bool DoWait(bool silent = false) {

            if (_nbExecution == 0) {
                if (silent) {
                    StartInfo.CreateNoWindow = true;
                    StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                }
            }

            if (!string.IsNullOrEmpty(Arguments))
                StartInfo.Arguments = Arguments;

            if (_nbExecution > 0)
                Kill(); 

            _nbExecution++;
            Process.Start();
            Process.WaitForExit();

            return Process.ExitCode == 0;
        }

        public void Kill() {
            try {
                Process.Kill();
            } catch (Exception) {
                try {
                    Process.Close();
                } catch (Exception) {
                    //ignored
                }
            }

        }

        #endregion

    }
}
