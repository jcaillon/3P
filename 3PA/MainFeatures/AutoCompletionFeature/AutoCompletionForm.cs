#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (AutoCompletionForm.cs) is part of 3P.
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
using YamuiFramework.Controls.YamuiList;
using YamuiFramework.Helper;

namespace _3PA.MainFeatures.AutoCompletionFeature {
    internal class AutoCompletionForm : NppInterfaceForm.NppInterfaceForm {
        #region public fields

        /// <summary>
        /// Accessor to the list
        /// </summary>
        public YamuiFilteredTypeList YamuiList { get; private set; }

        /// <summary>
        /// Stores the current word to use a filter in the autocompletion
        /// </summary>
        public string Keyword { get; set; }

        #endregion

        #region events

        /// <summary>
        /// Raised when the user presses TAB or ENTER or double click
        /// </summary>
        public event Action<CompletionItem> InsertSuggestion;

        #endregion

        #region Life and death

        public AutoCompletionForm() {
            YamuiList = new YamuiFilteredTypeList();
        }

        protected override void Dispose(bool disposing) {
            if (YamuiList != null && disposing) {
                YamuiList.MouseDown -= YamuiListOnMouseDown;
                YamuiList.EnterPressed -= YamuiListOnEnterPressed;
                YamuiList.TabPressed -= YamuiListOnTabPressed;
                YamuiList.KeyPressed -= YamuiListOnKeyPressed;
                YamuiList.RowClicked -= YamuiListOnRowClicked;
                YamuiList.MouseLeft -= YamuiListOnMouseLeft;
            }
            base.Dispose(disposing);
        }

        #endregion

        #region DrawContent

        private void DrawContent() {
            // init menu form
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;

            Controls.Clear();

            // list
            Padding = new Padding(BorderWidth, BorderWidth, BorderWidth, BorderWidth);
            YamuiList.Dock = DockStyle.Fill;
            YamuiList.MouseDown += YamuiListOnMouseDown;
            YamuiList.EnterPressed += YamuiListOnEnterPressed;
            YamuiList.TabPressed += YamuiListOnTabPressed;
            YamuiList.RowClicked += YamuiListOnRowClicked;
            YamuiList.KeyPressed += YamuiListOnKeyPressed;
            YamuiList.MouseLeft += YamuiListOnMouseLeft;
            YamuiList.IndexChanged += YamuiListOnIndexChanged;

            // add control
            Controls.Add(YamuiList);

            // Size the form
            Height = BorderWidth*2 + Config.Instance.AutoCompleteShowListOfXSuggestions*YamuiList.RowHeight + YamuiList.BottomHeight;
            Width = Config.Instance.AutoCompleteWidth;

            // Set minimum size
            MinimumSize = new Size(200, BorderWidth*2 + 2*YamuiList.RowHeight + YamuiList.BottomHeight);

            // So that the OnKeyDown event of this form is executed before the HandleKeyDown event of the control focused
            KeyPreview = true;
        }

        private void YamuiListOnIndexChanged(YamuiScrollList yamuiScrollList) {
            InfoToolTip.InfoToolTip.ShowToolTipFromAutocomplete(YamuiList.SelectedItem as CompletionItem, this);
        }

        #endregion

        /// <summary>
        /// Position the window in a smart way according to the Point in input
        /// </summary>
        /// <param name="position"></param>
        /// <param name="lineHeight"></param>
        public void SetPosition(Point position, int lineHeight) {
            this.SafeInvoke(form => {
                Location = GetBestAutocompPosition(position, lineHeight);
                ResizeFormToFitScreen();
            });
        }

        #region Events

        /// <summary>
        /// Resize to always a int number of rows displayed (and save user size)
        /// </summary>
        protected override void OnResizeEnd(EventArgs e) {
            Config.Instance.AutoCompleteWidth = Width;
            var nbRows = (int) Math.Floor((float) (Height - BorderWidth*2 - YamuiList.BottomHeight)/YamuiList.RowHeight);
            Config.Instance.AutoCompleteShowListOfXSuggestions = nbRows;
            Height = BorderWidth*2 + Config.Instance.AutoCompleteShowListOfXSuggestions*YamuiList.RowHeight + YamuiList.BottomHeight;
            base.OnResizeEnd(e);
        }

        private void YamuiListOnMouseLeft(YamuiScrollList yamuiScrollList) {
            GiveFocusBack();
        }

        private void YamuiListOnKeyPressed(YamuiScrollList yamuiScrollList, KeyEventArgs e) {
            if (e.KeyCode == Keys.Escape) {
                Cloack();
                InfoToolTip.InfoToolTip.Cloak();
                e.Handled = true;
            }
        }

        private void YamuiListOnRowClicked(YamuiScrollList yamuiScrollList, MouseEventArgs e) {
            if (e.Clicks >= 2 && InsertSuggestion != null)
                InsertSuggestion(yamuiScrollList.SelectedItem as CompletionItem);
        }

        private void YamuiListOnEnterPressed(YamuiScrollList yamuiScrollList, KeyEventArgs e) {
            if (InsertSuggestion != null && Config.Instance.AutoCompleteUseEnterToAccept) {
                InsertSuggestion(yamuiScrollList.SelectedItem as CompletionItem);
                e.Handled = true;
            }
        }

        private void YamuiListOnTabPressed(YamuiScrollList yamuiScrollList, KeyEventArgs e) {
            if (InsertSuggestion != null && Config.Instance.AutoCompleteUseTabToAccept) {
                InsertSuggestion(yamuiScrollList.SelectedItem as CompletionItem);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Allows the user to move the window from the bottom status of the YamuiList (showing x items)
        /// </summary>
        private void YamuiListOnMouseDown(object sender, MouseEventArgs e) {
            var list = sender as YamuiFilteredTypeList;
            if (list != null && Movable && e.Button == MouseButtons.Left && (new Rectangle(0, list.Height - list.BottomHeight, list.Width, list.BottomHeight)).Contains(e.Location)) {
                // do as if the cursor was on the title bar
                WinApi.ReleaseCapture();
                WinApi.SendMessage(Handle, (uint) WinApi.Messages.WM_NCLBUTTONDOWN, new IntPtr((int) WinApi.HitTest.HTCAPTION), new IntPtr(0));
            }
        }

        #endregion

        #region Show

        public new void Show(IWin32Window owner) {
            DrawContent();
            base.Show(owner);
        }

        #endregion
    }
}