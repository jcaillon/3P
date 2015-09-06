using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace _3PA.MainFeatures {
    class ProgressHandler {

        public void run_ext(string command, string currentFile) {
            // should be in config :
            string pathProgress = @"C:\Progress\client\v1110_dv\dlc\bin\prowin32.exe";
            string pathNppTool = @"D:\4GL-npp-plugin-master\ProgressFiles";

            string baseini = @"P:\appli\sac1\sacdev.ini";
            string basepf = @"P:\base\tmaprogress\newtmap.pf";
            string assemblies = @"C:\Progress\proparse.net";
            string tempfolder = @"C:\Temp";
            string registryPath = @"HKEY_CURRENT_USER\Software\NppTool\";

            // save the path to the progress files in the registry
            Registry.SetValue(registryPath, "scriptLoc", pathNppTool, RegistryValueKind.String);

            StringBuilder args = new StringBuilder();

            args.Append(
                " -cpinternal ISO8859-1" +
                " -inp 20000 -tok 2048 -numsep 46" +
                " -p " + quoter("nppTool.p"));

            if (Directory.Exists(tempfolder)) {
                args.Append(" -T " + quoter(tempfolder));
            }

            StringBuilder param = new StringBuilder();
            param.Append(command + "," + currentFile);

            if (File.Exists(baseini) && File.Exists(basepf)) {
                args.Append(
                " -ini " + quoter(baseini) +
                " -pf " + quoter(basepf));
                param.Append(",1");
            } else {
                param.Append(",0");
            }

            if (Directory.Exists(assemblies)) {
                args.Append(" -assemblies " + quoter(assemblies));
                param.Append(",1");
            } else {
                param.Append(",0");
            }

            args.Append(" -param " + quoter(param.ToString()));

            // execute
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = pathProgress;
            process.StartInfo.Arguments = args.ToString();
            process.StartInfo.WorkingDirectory = pathNppTool;
            process.EnableRaisingEvents = true;
            process.Exited += new EventHandler(afterProgressExecution);
            process.Start();

            MessageBox.Show(args.ToString(), "yo", MessageBoxButtons.OK);
        }

        private string quoter(string inString) {
            return "\"" + inString + "\"";
        }

        // Handle Exited event and display process information. 
        private void afterProgressExecution(object sender, System.EventArgs e) {
            MessageBox.Show("ok", "yo", MessageBoxButtons.OK);
        }
    }
}
