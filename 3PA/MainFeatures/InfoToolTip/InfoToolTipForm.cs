#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (InfoToolTipForm.cs) is part of 3P.
// 
// // 3P is a free software: you can redistribute it and/or modify
// // it under the terms of the GNU General Public License as published by
// // the Free Software Foundation, either version 3 of the License, or
// // (at your option) any later version.
// 
// // 3P is distributed in the hope that it will be useful,
// // but WITHOUT ANY WARRANTY; without even the implied warranty of
// // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// // GNU General Public License for more details.
// 
// // You should have received a copy of the GNU General Public License
// // along with 3P. If not, see <http://www.gnu.org/licenses/>.
// // ========================================================================
#endregion
using System;
using System.Drawing;
using System.Windows.Forms;
using YamuiFramework.HtmlRenderer.Core.Core.Entities;

namespace _3PA.MainFeatures.InfoToolTip {
    public partial class InfoToolTipForm : NppInterfaceForm.NppInterfaceForm {

        #region fields
        // prevents the form from stealing the focus
        //protected override bool ShowWithoutActivation {
        //    get { return true; }
        //}
        #endregion

        #region constructor
        public InfoToolTipForm() {
            InitializeComponent();
        }
        #endregion

        #region public
        /// <summary>
        /// set the html of the label, resize the tooltip
        /// </summary>
        /// <param name="content"></param>
        public void SetText(string content) {
            // find max height taken by the html
            Width = Screen.PrimaryScreen.WorkingArea.Width / 2;
            labelContent.Text = content;
            var prefHeight = Math.Min(labelContent.Height, Screen.PrimaryScreen.WorkingArea.Height / 2) + 10;

            // now we got the final height, resize width until height changes
            int j = 0;
            int detla = 100;
            int curWidth = Width;
            do {
                curWidth -= detla;
                Width = Math.Min(Screen.PrimaryScreen.WorkingArea.Width / 2, curWidth);
                labelContent.Text = content;
                if (labelContent.Height > prefHeight) {
                    curWidth += detla;
                    detla /= 2;
                }
                j++;
            } while (j < 10);
            Width = curWidth < 50 ? 150 : curWidth;
            Height = Math.Min(labelContent.Height, Screen.PrimaryScreen.WorkingArea.Height / 2) + 10;
        }

        /// <summary>
        /// Position the tooltip relatively to a point
        /// </summary>
        /// <param name="position"></param>
        /// <param name="lineHeight"></param>
        public void SetPosition(Point position, int lineHeight) {
            // position the window smartly
            if (position.X > Screen.PrimaryScreen.WorkingArea.X + Screen.PrimaryScreen.WorkingArea.Width / 2)
                position.X = position.X - Width;
            if (position.Y > Screen.PrimaryScreen.WorkingArea.Y + Screen.PrimaryScreen.WorkingArea.Height / 2)
                position.Y = position.Y - Height - lineHeight;
            Location = position;
        }

        /// <summary>
        /// Sets the link clicked event for the label
        /// </summary>
        /// <param name="clickHandler"></param>
        public void SetLinkClickedEvent(Action<HtmlLinkClickedEventArgs> clickHandler) {
            labelContent.LinkClicked += (sender, args) => clickHandler(args);
        }
        #endregion

    }
}
