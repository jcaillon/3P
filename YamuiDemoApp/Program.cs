using System;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Forms;
using YamuiFramework.Themes;

namespace YamuiDemoApp {
    static class Program {

        public static YamuiForm MainForm;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            ThemeManager.TabAnimationAllowed = true;
            MainForm = new Form1();
            MainForm.Opacity = 0d;
            /*
            MainForm.Tag = false;
            MainForm.Closing += (sender, args) => {
                if ((bool)MainForm.Tag) return;
                args.Cancel = true;
                MainForm.Tag = true;
                var t = new Transition(new TransitionType_Acceleration(200));
                t.add(MainForm, "Opacity", 0d);
                t.TransitionCompletedEvent += (o, args1) => { MainForm.Close(); };
                t.run();
            }*/
            Transition.run(MainForm, "Opacity", 1d, new TransitionType_Acceleration(200));
            Application.Run(MainForm);
        }
    }
}
