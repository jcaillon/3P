using System;
using System.Windows.Forms;
using YamuiFramework.Forms;
using YamuiFramework.Themes;

namespace YamuiDemoApp {

    public class ColorScheme {
        public string Name = "Default";
        public int UniqueId = 0;
    }


    static class Program {

        public static YamuiForm MainForm;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            YamuiThemeManager.TabAnimationAllowed = true;
            MainForm = new Form1();

            Application.Run(MainForm);

            //_3PA.Tests.ParserLexerTests.Run();

            /*
            Ship ship = new Ship();
            string value = "5.5";
            PropertyInfo propertyInfo = ship.GetType().GetProperty("Latitude");
            propertyInfo.SetValue(ship, Convert.ChangeType(value, propertyInfo.PropertyType), null);
            */

            ////------------
            //var watch = Stopwatch.StartNew();
            ////------------

            //CompilationPath.Import();

            ////--------------
            //watch.Stop();
            //MessageBox.Show(watch.ElapsedMilliseconds.ToString());
            ////------------

            //var derp = CompilationPath.GetCompilationDirectory(@"D:\Repo\3P\3PA\Data\ProgressEnvironnement.xml");



            //return;
            

            /*
             
            //------------
            var watch = Stopwatch.StartNew();
            //------------

            var test = FileExplorer.ListFileOjectsInDirectory(curDir);

            //--------------
            watch.Stop();
            MessageBox.Show(watch.ElapsedMilliseconds.ToString());
            //------------

            
            Style.Init();
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

        }
    }


}
