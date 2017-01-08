#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiMenuPopup.cs) is part of YamuiFramework.
// 
// YamuiFramework is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// YamuiFramework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with YamuiFramework. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using YamuiFramework.Controls.YamuiList;
using YamuiFramework.Fonts;
using YamuiFramework.Helper;
using YamuiFramework.HtmlRenderer.WinForms;

namespace YamuiFramework.Forms {

    public class YamuiMenuPopup : YamuiFormBaseShadowFadeIn {

        #region Private

        protected new int _animationDuration = 2000;

        private Action<YamuiMenuItem> _do;
        
        private bool _displayFilterBox = true;
        private Size _formMinSize = new Size(150, 20);

        #endregion

        #region public fields

        /// <summary>
        /// When an item is clicked, it will be fed to this method that should, in term, be calling .OnClic of said item
        /// Use this as a wrapper to handle errors for instance
        /// </summary>
        public Action<YamuiMenuItem> ClicItemWrapper {
            get {
                return _do ?? (item => {
                    if (item.OnClic != null) {
                        item.OnClic();
                    }
                });
            }
            set { _do = value; }
        }

        /// <summary>
        /// Location from where the menu will be generated
        /// </summary>
        public Point SpawnLocation { get; set; }

        /// <summary>
        /// List of the item to display in the menu
        /// </summary>
        public List<YamuiMenuItem> MenuList { get; set; }

        /// <summary>
        /// Title of the menu (or null)
        /// </summary>
        public string HtmlTitle { get; set; }

        /// <summary>
        /// Should we display a filter box?
        /// </summary>
        public bool DisplayFilterBox {
            get { return _displayFilterBox; }
            set { _displayFilterBox = value; }
        }

        /// <summary>
        /// Accessor to the list
        /// </summary>
        public YamuiFilteredTypeList YamuiList { get; private set; }

        /// <summary>
        /// Accessor to the filter box
        /// </summary>
        public YamuiFilterBox FilterBox { get; private set; }

        /// <summary>
        /// Set a minimum size for this menu
        /// </summary>
        public Size FormMinSize {
            get { return _formMinSize; }
            set { _formMinSize = value; }
        }

        /// <summary>
        /// Set a maximum size for this menu
        /// </summary>
        public Size FormMaxSize { get; set; }

        #endregion

        #region Don't show in ATL+TAB

        private const int WsExToolwindow = 0x80;

        /// <summary>
        /// The form should also set ShowInTaskbar = false; for this to work
        /// </summary>
        protected override CreateParams CreateParams {
            get {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= WsExToolwindow;
                return createParams;
            }
        }

        #endregion

        public YamuiMenuPopup(bool useSimpleFilteredList = false) {
            YamuiList = useSimpleFilteredList ? new YamuiFilteredTypeList() : new YamuiFilteredTypeTreeList();
            FilterBox = new YamuiFilterBox();
        }

        #region DrawContent

        private void DrawContent() {
            // init menu form
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;

            Controls.Clear();

            // evaluate the width needed to draw the control
            var useImageIcon = MenuList.Exists(item => item.ItemImage != null);
            var maxWidth = MenuList.Select(item => TextRenderer.MeasureText(item.SubText ?? "", FontManager.GetFont(FontFunction.Small)).Width + TextRenderer.MeasureText(item.DisplayText ?? "", FontManager.GetStandardFont()).Width).Concat(new[] { 0 }).Max();
            maxWidth += 10;
            maxWidth += (useImageIcon ? 35 : 8) + 12;
            if (FormMaxSize.Width > 0)
                maxWidth = maxWidth.ClampMax(FormMaxSize.Width);
            if (FormMinSize.Width > 0)
                maxWidth = maxWidth.ClampMin(FormMinSize.Width);
            Width = maxWidth;
            
            int yPos = BorderWidth;

            // title
            HtmlLabel title = null;
            if (HtmlTitle != null) {
                title = new HtmlLabel {
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    Location = new Point(BorderWidth, yPos),
                    AutoSizeHeightOnly = true,
                    Width = Width - BorderWidth * 2,
                    BackColor = Color.Transparent,
                    Text = HtmlTitle,
                    IsSelectionEnabled = false,
                    IsContextMenuEnabled = false,
                    Enabled = false
                };
                yPos += title.Height;
            }

            // display filter box?
            if (DisplayFilterBox) {
                FilterBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                FilterBox.Location = new Point(BorderWidth, yPos);
                FilterBox.Size = new Size(Width - BorderWidth*2, 30);
                FilterBox.Padding = new Padding(5);
                FilterBox.ExtraButtons = new List<YamuiFilterBox.YamuiFilterBoxButton> {
                    new YamuiFilterBox.YamuiFilterBoxButton {
                        Image = Resources.Resources.More,
                        OnClic = button => button.BackGrndImage = Resources.Resources.Erase
                    }
                };
                yPos += FilterBox.Height;
            }

            // list
            Padding = new Padding(BorderWidth, yPos, BorderWidth, BorderWidth);
            YamuiList.Dock = DockStyle.Fill;
            YamuiList.BottomHeight = 0;

            YamuiList.SetItems(MenuList.Cast<ListItem>().ToList());
            YamuiList.MouseDown += YamuiListOnMouseDown;
            yPos += MenuList.Count.ClampMin(1) * 20;
            
            // add controls
            if (title != null)
                Controls.Add(title);
            if (DisplayFilterBox) {
                FilterBox.Initialize(YamuiList);
                Controls.Add(FilterBox);
            }
            Controls.Add(YamuiList);

            // Size the form
            var height = yPos + BorderWidth;
            if (FormMaxSize.Height > 0)
                height = height.ClampMax(FormMaxSize.Height);
            if (FormMinSize.Height > 0)
                height = height.ClampMin(FormMinSize.Height);
            Size = new Size(maxWidth, height);
            MinimumSize = Size;
            MaximumSize = Size;

            // menu position
            Location = GetBestPosition(SpawnLocation);

            // default focus
            if (DisplayFilterBox)
                FilterBox.ClearAndFocusFilter();
            else
                ActiveControl = YamuiList;

            // So that the OnKeyDown event of this form is executed before the HandleKeyDown event of the control focused
            KeyPreview = true;
        }

        /// <summary>
        /// Allows the user to move the window from the bottom status of the YamuiList (showing x items)
        /// </summary>
        private void YamuiListOnMouseDown(object sender, MouseEventArgs e) {
            var list = sender as YamuiFilteredTypeList;
            if (list != null && Movable && e.Button == MouseButtons.Left && (new Rectangle(0, list.Height - list.BottomHeight, list.Width, list.BottomHeight)).Contains(e.Location)) {
                // do as if the cursor was on the title bar
                WinApi.ReleaseCapture();
                WinApi.SendMessage(Handle, (uint)WinApi.Messages.WM_NCLBUTTONDOWN, new IntPtr((int)WinApi.HitTest.HTCAPTION), new IntPtr(0));
            }
        }

        #endregion

        #region Events

        protected override void OnKeyDown(KeyEventArgs e) {
            if (e.KeyCode == Keys.Escape) {
                Close();
                Dispose();
                e.Handled = true;
            }

            if (!e.Handled)
                base.OnKeyDown(e);
        }

        /// <summary>
        /// Close the menu when the user clicked elsewhere
        /// </summary>
        protected override void OnDeactivate(EventArgs e) {
            Close();
            Dispose();
            base.OnDeactivate(e);
        }

        #endregion

        #region Show

        /// <summary>
        /// Call this method to show the notification
        /// </summary>
        public new void Show() {
            DrawContent();
            base.Show();
        }

        public new void ShowDialog() {
            DrawContent();
            base.ShowDialog();
        }

        public new void Show(IWin32Window owner) {
            DrawContent();
            base.Show(owner);
        }

        public new void ShowDialog(IWin32Window owner) {
            DrawContent();
            base.ShowDialog(owner);
        }

        #endregion

    }

}
