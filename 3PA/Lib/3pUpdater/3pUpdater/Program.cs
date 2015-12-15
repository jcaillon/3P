using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace _3pUpdater {

    /// <summary>
    /// The dumbest program i've ever written.
    /// </summary>
    class Program {

        static void Main(string[] args) {

            var from = args[0].Trim('"');
            var to = args[1].Trim('"');

            // wait till notepad++ is closed
            while (IsProcessOpen("notepad++"))
                Thread.Sleep(200);

            // delete current plugin
            if (File.Exists(to))
                File.Delete(to);

            // replace with update
            File.Move(from, to);
        }

        private static bool IsProcessOpen(string name) {
            return Process.GetProcesses().Any(clsProcess => clsProcess.ProcessName.Contains(name));
        }
    }
}
