using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace _3pUpdater {

    /// <summary>
    /// The dumbest program i've ever written.
    /// !!!!!!!!!!!!!!!!!
    /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    /// 
    /// THERE MUST BE 2 VERSIONS OF THIS STUFF, change :
    /// <requestedExecutionLevel level="highestAvailable" uiAccess="false" />
    /// in app.manifest
    /// 
    /// We need one version that requires the ADMIN rights, and the other that doesn't
    /// 
    /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    /// !!!!!!!!!!!!!!!!!
    ///
    /// </summary>
    class Program {

        static void Main() {
            // wait till notepad++ is closed
            while (IsProcessOpen("notepad++"))
                Thread.Sleep(200);
            
            // read 3pUpdater.lst, each lines contains :
            // full_path_to_file_to_move_from \t full_path_to_file_to_move_to
            // 
            // can also contain a line like : Start \t full_path_to_exe
            // it will execute the given program at the end of the update
            var progToStart  = new List<string>();
            var list = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase); 
            try {
                var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3pUpdater.lst");
                if (!File.Exists(path))
                    return;
                using (StringReader reader = new StringReader(File.ReadAllText(path, Encoding.Default))) {
                    string line;
                    while ((line = reader.ReadLine()) != null) {
                        if (line.Contains("\t")) {
                            var from = line.Split('\t')[0];
                            var to = line.Split('\t')[1];
                            if (from.Equals(@"Start")) {
                                progToStart.Add(to);
                            } else if (!list.ContainsKey(from)) {
                                list.Add(from.Trim(), to.Trim());
                            }
                        }
                    }
                }

                // update files
                foreach (var kvp in list) {
                    try {
                        // delete current plugin
                        if (File.Exists(kvp.Value))
                            File.Delete(kvp.Value);

                        // replace with update
                        File.Move(kvp.Key, kvp.Value);
                    } catch (Exception e) {
                        MessageBox.Show("Error while moving the file :\n" + kvp.Key + "\nto :" + kvp.Value + "\nDetails : " + e);
                    }
                }

                // start programs?
                foreach (var prog in progToStart) {
                    Process.Start(prog);
                }

            } catch (Exception e) {
                MessageBox.Show("Unexpected error :\n" + e + "\nPlease contact the author of this program to correct it");
            }
        }

        private static bool IsProcessOpen(string name) {
            return Process.GetProcesses().Any(clsProcess => clsProcess.ProcessName.Contains(name));
        }
    }
}
