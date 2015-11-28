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

        private const string DllName = "3P.dll";
        static void Main(string[] args) {

            // wait till notepad++ is closed
            while (IsProcessOpen("notepad++"))
                Thread.Sleep(200);

            var assPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (assPath == null)
                return;

            if (File.Exists(Path.Combine(assPath, DllName))) {
                // delete current plugin
                if (File.Exists(Path.Combine(assPath, "..\\..\\..\\", DllName)))
                    File.Delete(Path.Combine(assPath, "..\\..\\..\\", DllName));

                // replace with update
                File.Move(Path.Combine(assPath, DllName), Path.Combine(assPath, "..\\..\\..\\", DllName));
            }
        }

        private static bool IsProcessOpen(string name) {
            return Process.GetProcesses().Any(clsProcess => clsProcess.ProcessName.Contains(name));
        }
    }
}
