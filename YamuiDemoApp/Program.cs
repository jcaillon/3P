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
using _3PA.MainFeatures.Parser;

namespace YamuiDemoApp {
    static class Program {

        public static YamuiForm MainForm;

        public static IEnumerable<CharacterRange> FindAllMatchedRanges(string input, string filter) {
            var ranges = new List<CharacterRange>();
            int pos = 0;
            int posFilter = 0;
            bool matching = false;
            int startMatch = 0;
            while (pos < input.Length) {
                // remember matching state at the beginning of the loop
                bool wasMatching = matching;
                // we match the current char of the filter
                if (input[pos] == filter[posFilter]) {
                    if (!matching) {
                        matching = true;
                        startMatch = pos;
                    }
                    posFilter++;
                    // we matched the entire filter
                    if (posFilter >= filter.Length) {
                        ranges.Add(new CharacterRange(startMatch, pos - startMatch + 1));
                        break;
                    }
                } else
                    matching = false;
                // we stopped matching, remember matching range
                if (!matching && wasMatching)
                    ranges.Add(new CharacterRange(startMatch, pos - startMatch));
                pos++;
            }
            // we reached the end of the input, if we were matching stuff, remember matching range
            if (pos >= input.Length && matching)
                ranges.Add(new CharacterRange(startMatch, pos - 1 - startMatch));
            return ranges;
        }

        public static bool FullyMatch(string input, string filter) {
            var ranges = new List<CharacterRange>();
            int pos = 0;
            int posFilter = 0;
            while (pos < input.Length) {
                // we match the current char of the filter
                if (input[pos] == filter[posFilter]) {
                    posFilter++;
                    // we matched the entire filter
                    if (posFilter >= filter.Length)
                        return true;
                }
                pos++;
            }
            return false;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {

            string x = "NPPM_RELADILENPPM_RELOADFILE";
            string filter = "radfile";

            var ranges = FindAllMatchedRanges(x.ToLower(), filter.ToLower());
            int matchedLenght = (ranges != null) ? ranges.Sum(item => item.Length) : 0;
            bool fullyMatched = (matchedLenght == filter.Length);
            var output = new StringBuilder();
            output.AppendLine(x);
            output.AppendLine(filter);
            output.AppendLine("fullyMatched = " + FullyMatch(x.ToLower(), filter.ToLower()).ToString());

            if (ranges != null) {
                foreach (var item in ranges) {
                    output.AppendLine(item.First.ToString() + "," + item.Length.ToString());
                }
            }

            // test our function
            //------------
            var watch2 = Stopwatch.StartNew();
            //------------
            filter = filter.ToLower();
            for (int j = 0; j < 99999; j++) {
                var ranges3 = FullyMatch(x.ToLower(), filter);
                var ranges2 = FindAllMatchedRanges(x.ToLower(), filter);
            }
            //--------------
            watch2.Stop();
            output.AppendLine("done in " + watch2.ElapsedMilliseconds + " ms");
            //------------

            File.WriteAllText(@"C:\Users\Julien\Desktop\test.p", output.ToString());

            return;

            //Keywords.Init();

            //------------
            var watch = Stopwatch.StartNew();
            //------------

            Lexer tok = new Lexer(File.ReadAllText(@"C:\Users\Julien\Desktop\in.p"));
            //Lexer tok = new Lexer(File.ReadAllText(@"E:\temp\sac-dev\sac\sac\src\proc_uib\sc42lsdd.w"));
            
            tok.Tokenize();
            OutputLexerVisitor vis = new OutputLexerVisitor();
            tok.Accept(vis);

            File.WriteAllText(@"C:\Users\Julien\Desktop\test.p", vis.output.ToString());

            //--------------
            watch.Stop();
            MessageBox.Show("done in " + watch.ElapsedMilliseconds + " ms");
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

    public class OutputLexerVisitor : ILexerVisitor {
        public StringBuilder output = new StringBuilder();
        public void Visit(TokenComment tok) {
            output.AppendLine("Comment :" + tok.StartPosition + "," + tok.EndPosition + " : " + tok.Value);
        }

        public void Visit(TokenEol tok) {
            
        }

        public void Visit(TokenEos tok) {
            
        }

        public void Visit(TokenInclude tok) {
            //output.AppendLine("Include :" + tok.StartPosition + "," + tok.EndPosition + " : " + tok.Value);
        }

        public void Visit(TokenNumber tok) {
            
        }

        public void Visit(TokenQuotedString tok) {
            //output.AppendLine("String :" + tok.StartPosition + "," + tok.EndPosition + " : " + tok.Value);
        }

        public void Visit(TokenSymbol tok) {
            
        }

        public void Visit(TokenEof tok) {
            
        }

        public void Visit(TokenWord tok) {
            //output.AppendLine("Word :" + tok.StartPosition + "," + tok.EndPosition + " : " + tok.Value);
        }

        public void Visit(TokenWhiteSpace tok) {
            
        }

        public void Visit(TokenUnknown tok) {
            
        }
    }
}
