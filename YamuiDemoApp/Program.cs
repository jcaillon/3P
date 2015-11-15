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
            /*
            Application.EnableVisualStyles();
           
            Keywords.Init();

            var testAutoComp = new DockableExplorerForm();
            ExplorerContent.Init();
            testAutoComp.InitSetObjects();
            testAutoComp.ShowDialog();

            return;

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
             */
            ThemeManager.TabAnimationAllowed = true;
            MainForm = new Form1();

            Application.Run(MainForm);
        }
    }


}
