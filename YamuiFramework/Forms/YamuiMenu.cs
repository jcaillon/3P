#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiMenu.cs) is part of YamuiFramework.
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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using YamuiFramework.Controls;
using YamuiFramework.Fonts;
using YamuiFramework.Helper;
using YamuiFramework.HtmlRenderer.WinForms;
using YamuiFramework.Themes;

namespace YamuiFramework.Forms {

    /// <summary>
    /// A class to display a cool custom context menu
    /// </summary>
    public sealed class YamuiMenu : Form {

        #region public fields

        public bool IamMain = true;

        public int SubTextOpacity = 100;

        #endregion

        #region private fields

        private YamuiMenu _parentMenu;
        private YamuiMenu _childMenu;

        private bool _closing;

        /// <summary>
        /// We keep a list of the menu currently opened so we can know if a menu is still in focus
        /// </summary>
        public static List<IntPtr> ListOfOpenededMenuHandle { get; set; }

        private const int LineHeight = 22;
        private const int SeparatorLineHeight = 8;

        private bool _reverseX;
        private bool _reverseY;

        private List<YamuiMenuItem> _content = new List<YamuiMenuItem>();

        private List<int> _yPosOfSeparators = new List<int>(); 

        private int _selectedIndex;

        private const int BorderWidth = 2;

        #endregion

        #region Don't show in ATL+TAB

        protected override CreateParams CreateParams {
            get {
                var Params = base.CreateParams;
                Params.ExStyle |= 0x80;
                return Params;
            }
        }

        #endregion

        #region Life and death

        public YamuiMenu(Point location, List<YamuiMenuItem> content, string htmlTitle = null) {
            if (content == null || content.Count == 0)
                return;

            // init menu form
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);
            ControlBox = false;
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.Manual;

            var useImageIcon = content.Exists(item => item.ItemImage != null);
            var noChildren = !content.Exists(item => item.Children != null);
            var maxWidth = content.Select(item => TextRenderer.MeasureText(item.SubText, FontManager.GetFont(FontFunction.Small)).Width).Concat(new[] { 0 }).Max();
            maxWidth += maxWidth == 0 ? 0 : 15;
            maxWidth += content.Select(item => TextRenderer.MeasureText(item.ItemName, FontManager.GetStandardFont()).Width).Concat(new[] { 0 }).Max();
            maxWidth += (useImageIcon ? 35 : 8) + 12 + (noChildren ? 0 : 12);

            int yPos = BorderWidth;

            // title
            HtmlLabel title = null;
            if (htmlTitle != null) {
                title = new HtmlLabel {
                    AutoSizeHeightOnly = true,
                    BackColor = Color.Transparent,
                    Width = maxWidth - BorderWidth * 2,
                    Text = htmlTitle,
                    Location = new Point(BorderWidth, BorderWidth),
                    IsSelectionEnabled = false,
                    IsContextMenuEnabled = false,
                    Enabled = false
                };
                yPos += title.Height;
            }

            // insert buttons
            int index = 0;
            Controls.Clear();
            foreach (var item in content) {
                if (item.IsSeparator) {
                    _yPosOfSeparators.Add(yPos);
                    yPos += SeparatorLineHeight;
                } else {
                    var button = new YamuiMenuButton {
                        Text = item.ItemName,
                        NoChildren = item.Children == null || !item.Children.Any(),
                        Location = new Point(BorderWidth, yPos),
                        Size = new Size(maxWidth - BorderWidth * 2, LineHeight),
                        NoIconImage = !useImageIcon,
                        IconImage = item.ItemImage,
                        SubText = item.SubText,
                        Tag = index,
                        SubTextOpacity = SubTextOpacity
                    };
                    button.Click += ButtonOnButtonPressed;
                    button.PreviewKeyDown += OnPreviewKeyDown;
                    Controls.Add(button);
                    _content.Add(item);
                    yPos += LineHeight;
                    index++;
                }
            }

            // add title if needed
            if (title != null) {
                Controls.Add(title);
            }

            // Size the form
            Size = new Size(maxWidth, yPos + BorderWidth);
            MinimumSize = Size;
            MaximumSize = Size;

            // menu position
            var screen = Screen.FromPoint(location);
            if (location.X > screen.WorkingArea.X + screen.WorkingArea.Width/2) {
                location.X = location.X - Width;
                _reverseX = true;
            }
            if (location.Y > screen.WorkingArea.Y + screen.WorkingArea.Height/2) {
                location.Y = location.Y - Height;
                _reverseY = true;
            }
            Location = location;

            // events
            Deactivate += OnDeactivate;
            Activated += OnActivated;
            Closing += OnClosing;

            if (ListOfOpenededMenuHandle == null) {
                ListOfOpenededMenuHandle = new List<IntPtr>();
            }
            ListOfOpenededMenuHandle.Add(Handle);

            // keydown
            KeyPreview = true;
            PreviewKeyDown += OnPreviewKeyDown;
        }

        #endregion

        #region Paint Methods

        protected override void OnPaint(PaintEventArgs e) {
            var backColor = YamuiThemeManager.Current.FormBack;
            var borderColor = YamuiThemeManager.Current.FormBorder;

            e.Graphics.Clear(backColor);

            // draw the border with Style color
            var rect = new Rectangle(new Point(0, 0), new Size(Width, Height));
            var pen = new Pen(borderColor, BorderWidth) { Alignment = PenAlignment.Inset };
            e.Graphics.DrawRectangle(pen, rect);

            // draw separators
            foreach (var yPosOfSeparator in _yPosOfSeparators) {
                using (SolidBrush b = new SolidBrush(YamuiThemeManager.Current.ButtonHoverBack)) {
                    var width = (int) (Width*0.35);
                    e.Graphics.FillRectangle(b, new Rectangle(width, yPosOfSeparator + SeparatorLineHeight / 2 - 1, Width - width * 2, 2));
                }
            }
        }

        #endregion

        #region Events

        private void OnPreviewKeyDown(object sender, PreviewKeyDownEventArgs previewKeyDownEventArgs) {
            OnKeyDown(previewKeyDownEventArgs.KeyCode);
            previewKeyDownEventArgs.IsInputKey = true;
        }


        private void ButtonOnButtonPressed(object sender, EventArgs eventArgs) {
            var button = (YamuiMenuButton)sender;
            if (button != null) {
                _selectedIndex = (int)button.Tag;
                OnItemPressed();
            }
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs) {
            _closing = true;
            ListOfOpenededMenuHandle.Remove(Handle);

            Deactivate -= OnDeactivate;
            Activated -= OnActivated;
            Closing -= OnClosing;
            PreviewKeyDown -= OnPreviewKeyDown;
        }

        /// <summary>
        /// an item has been pressed
        /// </summary>
        private void OnItemPressed() {
            var item = _content[_selectedIndex];
            // item has children, open a new menu
            if (item.Children != null && item.Children.Any()) {
                _childMenu = new YamuiMenu(Location, item.Children) {
                    IamMain = false,
                    _parentMenu = this
                };
                _childMenu.SetPosition(new Rectangle(Location.X + Width, Location.Y + Controls[_selectedIndex].Top, Width, Height), _reverseX, _reverseY);
                _childMenu.Show();
            } else {
                // exec action and close the menu
                item.Do();
                CloseAll();
            }
        }

        /// <summary>
        /// Close the menu when the user clicked elsewhere
        /// </summary>
        private void OnDeactivate(object sender, EventArgs eventArgs) {
            // close if the new active windows isn't a menu
            // ReSharper disable once ObjectCreationAsStatement
            new DelayedAction(30, () => {
                if (!ListOfOpenededMenuHandle.Contains(WinApi.GetForegroundWindow()) && !_closing) {
                    BeginInvoke((Action) CloseAll);
                }
            });
        }

        /// <summary>
        /// Close all children when a menu is activated
        /// </summary>
        private void OnActivated(object sender, EventArgs eventArgs) {
            CloseChildren();
        }

        #endregion

        #region methods

        /// <summary>
        /// A key has been pressed on the menu
        /// </summary>
        public void OnKeyDown(Keys pressedKey) {
            switch (pressedKey) {
                case Keys.Left:
                case Keys.Escape:
                    if (_parentMenu != null) {
                        WinApi.SetForegroundWindow(_parentMenu.Handle);
                    }
                    Close();
                    break;
                case Keys.Right:
                case Keys.Space:
                case Keys.Enter:
                    OnItemPressed();
                    break;
                case Keys.Up:
                    _selectedIndex--;
                    break;
                case Keys.Down:
                    _selectedIndex++;
                    break;
                case Keys.PageDown:
                    _selectedIndex = _content.Count - 1;
                    break;
                case Keys.PageUp:
                    _selectedIndex = 0;
                    break;
            }
            if (_selectedIndex > _content.Count - 1)
                _selectedIndex = 0;
            if (_selectedIndex < 0)
                _selectedIndex = _content.Count - 1;
            if (Controls.Count > 0)
                ActiveControl = Controls[_selectedIndex];
        }

        /// <summary>
        /// Position the menu relativly to a parent menu
        /// </summary>
        private void SetPosition(Rectangle rect, bool reverseX, bool reverseY) {
            Location = new Point(reverseX ? (rect.X - rect.Width - Width) : rect.X, reverseY ? (rect.Y - (Height - 2) + LineHeight) : rect.Y);
        }

        private void CloseChildren() {
            if (_childMenu != null) {
                _childMenu.CloseChildren();
                _childMenu.Close();
            }
        }

        private void CloseParents() {
            if (_parentMenu != null) {
                _parentMenu.CloseParents();
                _parentMenu.Close();
            }
        }

        public void CloseAll() {
            CloseChildren();
            CloseParents();
            Close();
        }

        #endregion

        #region YamuiMenuButton

        private class YamuiMenuButton : YamuiButton {

            public bool NoIconImage { private get; set; }
            public bool NoChildren { private get; set; }
            public Image IconImage { private get; set; }
            public string SubText { get; set; }
            public int SubTextOpacity { get; set; }

            protected override void OnPaint(PaintEventArgs e) {

                // background
                var backColor = YamuiThemeManager.Current.MenuBg(IsFocused, IsHovered);
                e.Graphics.Clear(backColor);

                // foreground
                var foreColor = YamuiThemeManager.Current.MenuFg(IsFocused, IsHovered);

                // left line
                if (IsFocused) {
                    using (SolidBrush b = new SolidBrush(YamuiThemeManager.Current.AccentColor)) {
                        e.Graphics.FillRectangle(b, new Rectangle(0, 0, 3, ClientRectangle.Height));
                    }
                }

                // Image icon
                Image img = IconImage;
                if (img != null && !NoIconImage) {
                    e.Graphics.DrawImage(img, new Rectangle(8, 1, 20, 20));
                }

                // sub text 
                if (!string.IsNullOrEmpty(SubText)) {
                    var textFont = FontManager.GetFont(FontStyle.Bold, 10);
                    var textSize = TextRenderer.MeasureText(SubText, textFont);
                    var drawPoint = new PointF(Width - (NoChildren ? 0 : 12) - textSize.Width - 3, (ClientRectangle.Height / 2) - (textSize.Height / 2) - 1);
                    e.Graphics.DrawString(SubText, textFont, new SolidBrush(Color.FromArgb(SubTextOpacity, YamuiThemeManager.Current.SubTextFore)), drawPoint);

                    var pen = new Pen(Color.FromArgb((int)(SubTextOpacity * 0.8), YamuiThemeManager.Current.SubTextFore), 1) { Alignment = PenAlignment.Left };
                    e.Graphics.DrawRectangle(pen, drawPoint.X - 2, drawPoint.Y - 1, textSize.Width + 2, textSize.Height + 3);
                }

                // text
                TextRenderer.DrawText(e.Graphics, Text, FontManager.GetStandardFont(), new Rectangle(NoIconImage ? 8 : 35, 0, ClientRectangle.Width - (NoIconImage ? 8 : 35), ClientRectangle.Height), foreColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);

                // arrow
                if (!NoChildren) {
                    TextRenderer.DrawText(e.Graphics, ((char) 52).ToString(), new Font("Webdings", (float) (Height*0.50)), new Rectangle(ClientRectangle.Width - 12, 0, 12, ClientRectangle.Height), IsFocused ? YamuiThemeManager.Current.AccentColor : foreColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
            }
        }

        #endregion
    }

    #region YamuiMenuItem

    public class YamuiMenuItem {
        public Image ItemImage { get; set; }
        public string ItemName { get; set; }
        public Action OnClic { get; set; }
        public bool IsSeparator { get; set; }
        public List<YamuiMenuItem> Children { get; set; }
        public string SubText { get; set; }

        private Action _do;

        public Action Do { 
            get { 
                return _do ?? (() => {
                    if (OnClic != null) {
                        OnClic();
                    }
                }) ; 
            }
            set { _do = value; }
        }
    }

    #endregion

}
