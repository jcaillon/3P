using System;
using System.IO;
using System.Reflection;
using BrightIdeasSoftware;
using _3PA.Properties;

namespace _3PA.Lib {
    class LibLoader {

        private static string _rootDir;
        private static string _pathToYamui;
        private static string _pathToOlv;

        /// <summary>
        /// Should be called when the dll loads so it can call its dependencies freely
        /// </summary>
        public static void Init() {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            _rootDir = Path.Combine(Npp.GetThisAssemblyPath(), Resources.PluginFolderName);
            _pathToYamui = Path.Combine(_rootDir, @"YamuiFramework.dll");
            _pathToOlv = Path.Combine(_rootDir, @"ObjectListView.dll");  
          
            // TODO: only for debug! need to handle this when we do an update tho :/ we want to replace old files
            if (true) {
                try {
                    File.Delete(_pathToYamui);
                    File.Delete(_pathToOlv);
                } catch (Exception) {
                    // ignored
                }
            }
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            try {
                if (!Directory.Exists(_rootDir))
                    Directory.CreateDirectory(_rootDir);

                if (args.Name.StartsWith("YamuiFramework,")) {
                    if (!File.Exists(_pathToYamui))
                        File.WriteAllBytes(_pathToYamui, Resources.YamuiFramework);
                    return Assembly.LoadFrom(_pathToYamui);
                }

                if (args.Name.StartsWith("ObjectListView,")) {
                    if (!File.Exists(_pathToOlv))
                        File.WriteAllBytes(_pathToOlv, Resources.ObjectListView);
                    return Assembly.LoadFrom(_pathToOlv);
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in LibLoader");
            }
            return null;
        }
    }
}
