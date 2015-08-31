using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Forms;
using YamuiFramework.Themes;

namespace _3PA.Appli.Pages.control {
    public partial class Classic : YamuiPage {
        public Classic() {
            InitializeComponent();
        }

        private void yamuiButton5_Click(object sender, EventArgs e) {
            yamuiButton4.UseCustomBackColor = true;
            Transition.run(yamuiButton4, "BackColor", ThemeManager.Current.ButtonColorsNormalBackColor, ThemeManager.AccentColor, new TransitionType_Flash(3, 300), (o, args) => { yamuiButton4.UseCustomBackColor = false;  });
        }

        private void yamuiButton1_Click(object sender, EventArgs e) {
            ThemeManager.TabAnimationAllowed = false;
        }

        private void yamuiButton4_Click(object sender, EventArgs e) {
            // We create a transition to animate all four properties at the same time...
            Transition t = new Transition(new TransitionType_Linear(1000));
            //t.add(yamuiButton5, "Text", "What the hell???");
            t.add(yamuiButton5, "Text", (yamuiButton5.Text == @"What the hell???") ? "Holy molly" : "What the hell???");
            t.run();
        }

        private void yamuiCharButton3_Click(object sender, EventArgs e) {
            //var smk = new SmokeScreen(FindForm());
            YamuiFormMessageBox.ShwDlg(FindForm().Handle, MsgType.Error, "Erreur", @"Wtf did you do you fool!?<br>This is a new line with <b>BOLD</b> stuff<br><br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction. <br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<br>WARNING	D:\Work\ProgressFiles\compiler\sc20sdan.w	?	?	?	214	lWARNING--le mot clé TRANSACTION est utilisé à l'intérieur du niveau réel de transaction.<a href='efzefzef'>test a link</a>", new List<string> { "fu", "ok" }, true, (o, args) => {
                MessageBox.Show(args.Link);
                var x = (YamuiFormMessageBox) o; x.Close();
            }, true);
        }

        private void yamuiCharButton4_Click(object sender, EventArgs e) {
            // take a screenshot of the form and darken it:
            Bitmap bmp = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);
            using (Graphics G = Graphics.FromImage(bmp)) {
                G.CompositingMode = CompositingMode.SourceOver;
                G.CopyFromScreen(PointToScreen(new Point(0, 0)), new Point(0, 0), ClientRectangle.Size);
                double percent = 0.60;
                Color darken = Color.FromArgb((int)(255 * percent), Color.Black);
                using (Brush brsh = new SolidBrush(darken)) {
                    G.FillRectangle(brsh, ClientRectangle);
                }
            }

            // put the darkened screenshot into a Panel and bring it to the front:
            using (Panel p = new Panel()) {
                p.Location = new Point(0, 0);
                p.Size = ClientRectangle.Size;
                p.BackgroundImage = bmp;
                Controls.Add(p);
                p.BringToFront();

                // display your dialog somehow:
                Form frm = new Form();
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.ShowDialog(this);
            } // panel will be disposed and the form will "lighten" again...
        }

        private void yamuiCharButton5_Click(object sender, EventArgs e) {
            var x = (YamuiTabPage)Parent;
            var t = new Transition(new TransitionType_Acceleration(3000));
            var newSM = new YamuiTabAnimation(FindForm(), x);
            t.add(newSM, "Opacity", 0d);
            t.TransitionCompletedEvent += (o, args) => {
                newSM.Close();
            };
            t.run();
        }
    }
}
