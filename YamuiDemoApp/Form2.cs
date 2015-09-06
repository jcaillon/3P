using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YamuiFramework.Controls;
using YamuiFramework.Helper;
using _3PA.Lib;
using _3PA.MainFeatures;
using _3PA.MainFeatures.AutoCompletion;

namespace YamuiDemoApp {
    public partial class Form2 : Form {

        private AutoCompletionForm _testAutoComp;

        public Form2() {
            InitializeComponent();
        }

        private void yamuiTextBox1_TextChanged(object sender2, EventArgs e) {
            var y = (YamuiTextBox)sender2;

            if (_testAutoComp != null && !_testAutoComp.Visible)
                try {
                    _testAutoComp.Close();
                    _testAutoComp = null;
                } catch (Exception) {
                    // ignored
                }
            if (_testAutoComp == null) {
                var derp = new Random();
                _testAutoComp = new AutoCompletionForm(Keywords.Keys.Select(x => new CompletionData {DisplayText = x, Type = (CompletionType) derp.Next(0, 9), Ranking = derp.Next(0, 1000), Flag = ((CompletionFlag) derp.Next(0, 5))}).ToList(), new Point(Screen.PrimaryScreen.Bounds.Width - 200, Screen.PrimaryScreen.Bounds.Height - 200), 15, y.Text, 0.8d, 12);
            } else {
                _testAutoComp.FilterByText = y.Text;
            }

            _testAutoComp.TabCompleted += (sender, args) => {
                MessageBox.Show(args.CompletionItem.DisplayText);
            };

            _testAutoComp.CurrentForegroundWindow = WinApi.GetForegroundWindow();
            _testAutoComp.Show(this);
        }
    }
}
