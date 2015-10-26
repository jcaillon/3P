using System;
using System.Drawing;
using System.Windows.Forms;

namespace _3PA.MainFeatures.InfoToolTip {
    public partial class InfoToolTipForm : NppInterfaceForm.NppInterfaceForm {

        #region fields
        //Very important to keep it. It prevents the form from stealing the focus
        protected override bool ShowWithoutActivation {
            get { return true; }
        }
        #endregion


        #region constructor
        public InfoToolTipForm() {
            InitializeComponent();
        }
        #endregion

        #region public
        public void SetText(string content) {
            // find max height taken by the html
            Width = Screen.PrimaryScreen.WorkingArea.Width / 3;
            labelContent.Text = content;
            var prefHeight = Math.Min(labelContent.Height, Screen.PrimaryScreen.WorkingArea.Height/3) + 10;

            // now we got the final height, resize width until height changes
            int j = 0;
            int detla = 100;
            int curWidth = Width;
            do {
                curWidth -= detla;
                Width = Math.Min(Screen.PrimaryScreen.WorkingArea.Width / 3, curWidth);
                labelContent.Text = content;
                if (labelContent.Height > prefHeight) {
                    curWidth += detla;
                    detla /= 2;
                }
                j++;
            } while (j < 10);
            Width = curWidth;
            Height = Math.Min(labelContent.Height, Screen.PrimaryScreen.WorkingArea.Height / 3) + 10;
        }

        public void SetPosition(Point position, int lineHeight) {
            // position the window smartly
            if (position.X > Screen.PrimaryScreen.WorkingArea.X + 2 * Screen.PrimaryScreen.WorkingArea.Width / 3)
                position.X = position.X - Width;
            if (position.Y > Screen.PrimaryScreen.WorkingArea.Y + 3 * Screen.PrimaryScreen.WorkingArea.Height / 5)
                position.Y = position.Y - Height - lineHeight;
            Location = position;
        }
        #endregion

    }
}
