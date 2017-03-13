#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (InfoToolTipForm.cs) is part of 3P.
// 
// 3P is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// 3P is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with 3P. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System;
using System.Drawing;
using System.Windows.Forms;
using YamuiFramework.Controls;
using YamuiFramework.HtmlRenderer.Core.Core.Entities;
using YamuiFramework.HtmlRenderer.WinForms;
using _3PA.NppCore;

namespace _3PA.MainFeatures.InfoToolTip {
    internal sealed class InfoToolTipForm : NppInterfaceForm.NppInterfaceForm {
        #region fields

        private YamuiScrollPanel _panel;
        private HtmlLabel _labelContent;

        #endregion

        #region constructor

        public InfoToolTipForm() {
            Resizable = false;

            Padding = new Padding(5);

            // add scroll page
            _panel = new YamuiScrollPanel {
                Dock = DockStyle.Fill,
                NoBackgroundImage = true
            };
            Controls.Add(_panel);

            // add label
            _labelContent = new HtmlLabel {
                AutoSizeHeightOnly = true,
                Location = new Point(0, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            _panel.ContentPanel.Location = new Point(0, 0);
            _panel.ContentPanel.Controls.Add(_labelContent);

            Size = new Size(50, 50);
        }

        #endregion

        #region public

        /// <summary>
        /// set the html of the label, resize the tooltip
        /// </summary>
        /// <param name="content"></param>
        /// <param name="minimumWidth"></param>
        public void SetText(string content, int minimumWidth = 200) {
            if (Visible)
                Cloack();

            var screen = Npp.NppScreen;

            _labelContent.SetNeededSize(content, minimumWidth, screen.WorkingArea.Width/2 - 20);

            _panel.ContentPanel.Size = _labelContent.Size;
            Size = new Size(_panel.ContentPanel.Width + 10, Math.Min(_labelContent.Height, screen.WorkingArea.Height/2 - 10) + 10);

            // Too tall?
            if (_labelContent.Height > (screen.WorkingArea.Height/2 - 10)) {
                Width = Width + 10; // add scrollbar width
            }
        }

        /// <summary>
        /// Sets the link clicked event for the label
        /// </summary>
        /// <param name="clickHandler"></param>
        public void SetLinkClickedEvent(Action<HtmlLinkClickedEventArgs> clickHandler) {
            _labelContent.LinkClicked += (sender, args) => clickHandler(args);
        }

        #endregion
    }
}