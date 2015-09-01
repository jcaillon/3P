using System;
using System.IO;
using System.Reflection;
using BrightIdeasSoftware;
using _3PA.Properties;

namespace _3PA.Lib {
    class LibLoader {

        private static string _rootDir;
        private static string _pathToYamui;
        private static string _pathToOLV;

        public static void Init() {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            _rootDir = Path.Combine(Npp.GetThisAssemblyPath(), Resources.PluginFolderName);
            _pathToYamui = Path.Combine(_rootDir, @"YamuiFramework.dll");
            _pathToOLV = Path.Combine(_rootDir, @"ObjectListView.dll");

            
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
                    if (!File.Exists(_pathToOLV))
                        File.WriteAllBytes(_pathToOLV, Resources.ObjectListView);
                    return Assembly.LoadFrom(_pathToOLV);
                }
            } catch (Exception e) {
                Plug.ShowErrors(e, "Error in LibLoader");
            }
            return null;
        }
    }
}
