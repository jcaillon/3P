using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Forms;
using YamuiFramework.Themes;
using _3PA;
using _3PA.Interop;
using _3PA.MainFeatures.Colorisation;
using _3PA.MainFeatures.Parser;

namespace YamuiDemoApp {
    static class Program {

        public static YamuiForm MainForm;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            //Keywords.Init();

            //------------
            var watch = Stopwatch.StartNew();
            //------------

            List<Token> fu = new List<Token>();

            Lexer tok = new Lexer(File.ReadAllText(@"C:\Users\Julien\Desktop\in.p"));


            Token token;
            do {
                token = tok.GetNext();
                fu.Add(token);
            } while (token.Type != TokenType.Eof);


            // output : 
            StringBuilder output = new StringBuilder();
            foreach (var item in fu) {
                int thisStyle;
                switch (item.Type) {
                    case TokenType.Comment:
                        output.AppendLine(item.Type + " :" + item.StartPosition + "," + item.EndPosition + " : " + item.Value);
                        break;
                    case TokenType.Word:
                        output.AppendLine(item.Type + " :" + item.StartPosition + "," + item.EndPosition + " : " + item.Value);
                        break;
                    default:
                        break;
                }
            }

            // output :
            //StringBuilder output = new StringBuilder();
            //foreach (var item in fu) {
            //    output.AppendLine(item.Type + " :" + item.Value);
            //}

            File.WriteAllText(@"C:\Users\Julien\Desktop\test.p", output.ToString());

            //--------------
            watch.Stop();
            MessageBox.Show(fu.Count + "\ndone in " + watch.ElapsedMilliseconds + " ms");
            //------------


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
            MainForm.Opacity = 0d;
            
            MainForm.Tag = false;
            MainForm.Closing += (sender, args) => {
                if ((bool) MainForm.Tag) return;
                args.Cancel = true;
                MainForm.Tag = true;
                var t = new Transition(new TransitionType_Acceleration(200));
                t.add(MainForm, "Opacity", 0d);
                t.TransitionCompletedEvent += (o, args1) => { MainForm.Close(); };
                t.run();
            };
            Transition.run(MainForm, "Opacity", 1d, new TransitionType_Acceleration(200));
            Application.Run(MainForm);
        }
    }
}
