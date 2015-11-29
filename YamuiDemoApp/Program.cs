using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Forms;
using YamuiFramework.Themes;
using _3PA.Lib;
using _3PA.MainFeatures.Appli;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.Parser;
using _3PA.MainFeatures.SyntaxHighlighting;

namespace YamuiDemoApp {
    static class Program {

        public static YamuiForm MainForm;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {

            // Parse the .json
            var parser = new JsonParser(File.ReadAllText(@"C:\Users\Julien\Desktop\releases.json"));
            parser.Tokenize();
            var releasesList = parser.GetList();

            // Releases list empty?
            if (releasesList == null)
                return;

            var localVersion = "v1.0";

            var outputBody = new StringBuilder();
            var highestVersion = localVersion;
            var highestVersionInt = -1;
            var iCount = 0;
            foreach (var release in releasesList) {
                var releaseVersionTuple = release.First(tuple => tuple.Item1.Equals("tag_name"));
                var prereleaseTuple = release.First(tuple => tuple.Item1.Equals("prerelease"));
                var releaseNameTuple = release.First(tuple => tuple.Item1.Equals("name"));

                if (releaseVersionTuple != null && prereleaseTuple != null) {

                    var releaseVersion = releaseVersionTuple.Item2;

                    // is it the highest version ? for prereleases or full releases depending on the user config
                    if (((Config.Instance.UserGetsPreReleases && prereleaseTuple.Item2.EqualsCi("true"))
                            || (!Config.Instance.UserGetsPreReleases && prereleaseTuple.Item2.EqualsCi("false")))
                        && releaseVersion.IsHigherVersionThan(highestVersion)) {
                        highestVersion = releaseVersion;
                        highestVersionInt = iCount;
                    }

                    // For each version higher than the local one, append to the release body
                    // Will be used to display the version log to the user
                    if (releaseVersion.IsHigherVersionThan(localVersion)) {
                        outputBody.AppendLine("\n\n## " + releaseVersion + ((releaseNameTuple != null) ? " : " + releaseNameTuple.Item2 : "") + " ##\n\n");
                        var locBody = release.First(tuple => tuple.Item1.Equals("body"));
                        if (locBody != null)
                            outputBody.AppendLine(locBody.Item2);
                    }
                }
                iCount++;
            }


            /*
            Highlight.Init();
            return;

            CreateHelpConfigFile.Do();
            
            TextTests.Run();

            //Keywords.Init();

            ParserLexerTests.Run();

            return;

            ThemeManager.TabAnimationAllowed = true;
            var fuck = new AppliForm();
            fuck.DoShow();
            fuck.UnCloack();
            Application.Run(fuck);

            return;
            
            Application.EnableVisualStyles();
           
            Keywords.Init();

            var testAutoComp = new DockableExplorerForm();
            ExplorerContent.Init();
            testAutoComp.InitSetObjects();
            testAutoComp.ShowDialog();

            return;
            */
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
             
            ThemeManager.TabAnimationAllowed = true;
            MainForm = new Form1();

            Application.Run(MainForm);
        }
    }


}
