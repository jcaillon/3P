using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace _3PA.Lib {

    /// <summary>
    /// http://the.earth.li/~sgtatham/putty/0.52/htmldoc/Chapter6.html
    /// </summary>
    internal class Sftp {

        private string _host;

        private string _user;

        private string _password;

        private string _batchFilePath = "batch.txt";

        private string _psftpPath = @"C:\psftp"; // Change this to the location of the PSTFP app.  Do not include the ‘.exe’ file extension.

        public string Outputs = ""; // Stores the outputs and errors of PSFTP

        /* Object Constructor for standard usage */

        /* Object Constructor for standard usage */

        public Sftp(string host, string user, string password) {
            _host = host;

            _user = user;

            _password = password;
        }

        /* Retrieve files from the server */

        public void Get(string[] remote, string[] local) {
            /* Format the commands */

            string[] commands = new string[remote.Count()];

            for (int i = 0; i < remote.Count(); i++) {
                commands[i] = @"get " + remote[i] + @" " + local[i];
            }

            GenerateBatchFile(commands);

            Run();
        }

        /* Send files from your computer to the server */

        public void Put(string[] remote, string[] local) {
            /* Format the commands */

            string[] commands = new string[remote.Count()];

            for (int i = 0; i < remote.Count(); i++) {
                commands[i] = @"put " + remote[i] + @" " + local[i];
            }

            GenerateBatchFile(commands);

            Run();
        }

        /* Use this to send other SFTP commands (CD, DIR, etc.) */

        public void SendCommands(string[] commands) {
            GenerateBatchFile(commands);

            Run();
        }

        /* Create a text file with a list of commands to be fed into PSFTP */

        private void GenerateBatchFile(string[] commands) {
            try {
                StreamWriter batchWriter = new StreamWriter(_batchFilePath);

                /* Write each command to the batch file */

                for (int i = 0; i < commands.Count(); i++) {
                    batchWriter.WriteLine(commands[i]);
                }

                /* Command to close the connection */

                batchWriter.WriteLine(@"bye")
                ;

                batchWriter.Close();
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }

        /* Run the commands, store the outputs */

        private void Run() {
            /* Execute PSFTP as a System.Diagnostics.Process using the supplied login info and generated batch file */

            try {
                ProcessStartInfo psftpStartInfo = new ProcessStartInfo(_psftpPath, _user + @"@" + _host + @" -pw " + _password + @" -batch -be -b " + _batchFilePath);
                /* Allows redirecting inputs/outputs of PSFTP to your app */

                psftpStartInfo.RedirectStandardInput = true;

                psftpStartInfo.RedirectStandardOutput = true;

                psftpStartInfo.RedirectStandardError = true;

                psftpStartInfo.UseShellExecute = false;

                Process psftpProcess = new Process();

                psftpProcess.StartInfo = psftpStartInfo;

                psftpProcess.Start();

                /* Streams for capturing outputs and errors as well as taking ownership of the input */

                StreamReader psftpOutput = psftpProcess.StandardOutput;

                StreamReader psftpError = psftpProcess.StandardError;

                StreamWriter psftpInput = psftpProcess.StandardInput;

                while (!psftpOutput.EndOfStream) {
                    try {
                        /* This is usefule for commands other than ‘put’ or ‘get’ and for catching errors. */

                        Outputs += psftpOutput.ReadLine();

                        Outputs += psftpError.ReadLine();
                    } catch (Exception ex) {
                        Console.WriteLine(ex.ToString());
                    }
                }

                psftpOutput.Close();

                psftpError.Close();

                psftpInput.Close();

                psftpProcess.WaitForExit();


            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }

            /* Delete the batch file */

            try {
                File.Delete(_batchFilePath);
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }
    }

}
