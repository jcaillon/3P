#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiWaterfallMenu.cs) is part of YamuiFramework.
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
    public sealed class YamuiWaterfallMenu : YamuiFormBase {
        #region static fields

        /// <summary>
        /// We keep a list of the menu currently opened so we can know if a menu is still in focus
        /// </summary>
        public static List<IntPtr> ListOfOpenededMenuHandle { get; set; }

        #endregion

        #region public fields

        public bool IamMain = true;

        public float SubTextOpacity = 0.3f;

        /// <summary>
        /// When an item is clicked, it will be fed to this method that should, in term, be calling .OnClic of said item
        /// Use this as a wrapper to handle errors for instance
        /// </summary>
        public Action<YamuiMenuItem> ClicItemWrapper {
            get {
                return _do ?? (item => {
                    if (item.OnClic != null) {
                        item.OnClic(item);
                    }
                });
            }
            set { _do = value; }
        }

        #endregion

        #region private fields

        private Action<YamuiMenuItem> _do;

        private YamuiWaterfallMenu _parentMenu;
        private YamuiWaterfallMenu _childMenu;

        private bool _closing;

        private const int LineHeight = 20;
        private const int SeparatorLineHeight = 8;

        private List<YamuiMenuItem> _content = new List<YamuiMenuItem>();

        private List<int> _yPosOfSeparators = new List<int>();

        private int _selectedIndex;

        #endregion

        #region Don't show in ATL+TAB

        protected override CreateParams CreateParams {
            get {
                var Params = base.CreateParams;
                Params.ExStyle |= (int) WinApi.WindowStylesEx.WS_EX_TOOLWINDOW;
                return Params;
            }
        }

        #endregion

        #region Life and death

        public YamuiWaterfallMenu(Point location, List<YamuiMenuItem> content, string htmlTitle = null, int minSize = 150) {
            if (content == null || content.Count == 0)
                content = new List<YamuiMenuItem> {new YamuiMenuItem {DisplayText = "Empty", IsDisabled = true}};

            // init menu form
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;

            var useImageIcon = content.Exists(item => item.ItemImage != null);
            var noChildren = !content.Exists(item => item.Children != null);
            var maxWidth = content.Select(item => TextRenderer.MeasureText(item.SubText, FontManager.GetFont(FontFunction.Small)).Width).Concat(new[] {0}).Max();
            maxWidth += maxWidth == 0 ? 0 : 15;
            maxWidth += content.Select(item => TextRenderer.MeasureText(item.DisplayText, FontManager.GetStandardFont()).Width).Concat(new[] {0}).Max();
            maxWidth += (useImageIcon ? 35 : 8) + 12 + (noChildren ? 0 : 12);
            maxWidth = Math.Max(minSize, maxWidth);

            int yPos = BorderWidth;

            // title
            HtmlLabel title = null;
            if (htmlTitle != null) {
                title = new HtmlLabel {
                    AutoSizeHeightOnly = true,
                    BackColor = Color.Transparent,
                    Width = maxWidth - BorderWidth*2,
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
            bool lastButtonWasDisabled = true;
            Controls.Clear();
            foreach (var item in content) {
                if (item.IsSeparator) {
                    _yPosOfSeparators.Add(yPos);
                    yPos += SeparatorLineHeight;
                } else {
                    var button = new YamuiMenuButton {
                        Text = item.DisplayText,
                        NoChildren = item.Children == null || !item.Children.Any(),
                        Location = new Point(BorderWidth, yPos),
                        Size = new Size(maxWidth - BorderWidth*2, LineHeight),
                        NoIconImage = !useImageIcon,
                        BackGrndImage = item.ItemImage,
                        SubText = item.SubText,
                        Tag = index,
                        SubTextOpacity = SubTextOpacity,
                        Enabled = !item.IsDisabled
                    };
                    button.Click += ButtonOnPressed;
                    button.KeyDown += ButtonOnKeyDown;
                    Controls.Add(button);
                    _content.Add(item);
                    yPos += LineHeight;

                    // allows to select the correct button at start up
                    if (item.IsSelectedByDefault || lastButtonWasDisabled)
                        _selectedIndex = index;
                    if (lastButtonWasDisabled)
                        lastButtonWasDisabled = item.IsDisabled;
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
            Resizable = false;

            // menu position
            Location = GetBestMenuPosition(location);

            // set focused item
            ActiveControl = Controls[_selectedIndex];

            // register to the opened menu list
            if (ListOfOpenededMenuHandle == null) {
                ListOfOpenededMenuHandle = new List<IntPtr>();
            }
            ListOfOpenededMenuHandle.Add(Handle);

            // So that the OnKeyDown event of this form is executed before the HandleKeyDown event of the control focused
            KeyPreview = true;
        }

        #endregion

        #region Paint Methods

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

            // draw separators
            foreach (var yPosOfSeparator in _yPosOfSeparators) {
                using (SolidBrush b = new SolidBrush(YamuiThemeManager.Current.FormAltBack)) {
                    var width = (int) (Width*0.35);
                    e.Graphics.FillRectangle(b, new Rectangle(width, yPosOfSeparator + SeparatorLineHeight/2 - 1, Width - width*2, 2));
                }
            }
        }

        #endregion

        #region Events

        protected override void OnKeyDown(KeyEventArgs e) {
            e.Handled = HandleKeyDown(e.KeyCode);
            if (!e.Handled)
                base.OnKeyDown(e);
        }

        /// <summary>
        /// A key has been pressed on the menu
        /// </summary>
        private bool HandleKeyDown(Keys pressedKey) {
            var initialIndex = _selectedIndex;
            do {
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
                    default:
                        return false;
                }
                if (_selectedIndex > _content.Count - 1)
                    _selectedIndex = 0;
                if (_selectedIndex < 0)
                    _selectedIndex = _content.Count - 1;
                if (Controls.Count > 0)
                    ActiveControl = Controls[_selectedIndex];
            }
                // do this while the current button is disabled and we didn't already try every button
            while (_content[_selectedIndex].IsDisabled && initialIndex != _selectedIndex);

            return true;
        }

        private void ButtonOnPressed(object sender, EventArgs eventArgs) {
            var button = (YamuiMenuButton) sender;
            if (button != null) {
                _selectedIndex = (int) button.Tag;
                OnItemPressed();
            }
        }

        private void ButtonOnKeyDown(object sender, KeyEventArgs e) {
            OnKeyDown(e);
        }

        /// <summary>
        /// an item has been pressed
        /// </summary>
        private void OnItemPressed() {
            var item = _content[_selectedIndex];
            // item has children, open a new menu
            if (item.Children != null && item.Children.Any()) {
                _childMenu = new YamuiWaterfallMenu(Location, item.Children.Cast<YamuiMenuItem>().ToList()) {
                    IamMain = false,
                    _parentMenu = this
                };
                _childMenu.Location = GetChildBestPosition(new Rectangle(Location.X + Width, Location.Y + Controls[_selectedIndex].Top, _childMenu.Width, _childMenu.Height), LineHeight);
                _childMenu.Show();
            } else {
                // exec action and close the menu
                ClicItemWrapper(item);
                CloseAll();
            }
        }

        /// <summary>
        /// Close the menu when the user clicked elsewhere
        /// </summary>
        protected override void OnDeactivate(EventArgs e) {
            // close if the new active windows isn't a menu
            // ReSharper disable once ObjectCreationAsStatement
            new DelayedAction(30, () => {
                if (!ListOfOpenededMenuHandle.Contains(WinApi.GetForegroundWindow()) && !_closing) {
                    BeginInvoke((Action) CloseAll);
                }
            });
            base.OnDeactivate(e);
        }

        /// <summary>
        /// Close all children when a menu is activated
        /// </summary>
        protected override void OnActivated(EventArgs e) {
            CloseChildren();
            base.OnActivated(e);
        }

        protected override void OnClosing(CancelEventArgs e) {
            _closing = true;
            ListOfOpenededMenuHandle.Remove(Handle);
            base.OnClosing(e);
        }

        #endregion

        #region methods

        private void CloseChildren() {
            if (_childMenu != null) {
                _childMenu.CloseChildren();
                _childMenu.Close();
                _childMenu.Dispose();
            }
        }

        private void CloseParents() {
            if (_parentMenu != null) {
                _parentMenu.CloseParents();
                _parentMenu.Close();
                _parentMenu.Dispose();
            }
        }

        public void CloseAll() {
            CloseChildren();
            CloseParents();
            Close();
            Dispose();
        }

        #endregion

        #region YamuiMenuButton

        private class YamuiMenuButton : YamuiButton {
            public bool NoIconImage { private get; set; }
            public bool NoChildren { private get; set; }
            public string SubText { get; set; }
            public float SubTextOpacity { get; set; }

            protected override void OnPaint(PaintEventArgs e) {
                var backColor = YamuiThemeManager.Current.MenuBg(IsFocused, IsHovered, Enabled);
                var foreColor = YamuiThemeManager.Current.MenuFg(IsFocused, IsHovered, Enabled);

                // background
                e.Graphics.Clear(backColor);

                // foreground
                // left line
                if (IsFocused && Enabled) {
                    using (SolidBrush b = new SolidBrush(YamuiThemeManager.Current.AccentColor)) {
                        e.Graphics.FillRectangle(b, new Rectangle(0, 0, 3, ClientRectangle.Height));
                    }
                }

                // Image icon
                if (BackGrndImage != null) {
                    var recImg = new Rectangle(new Point(8, (ClientRectangle.Height - BackGrndImage.Height)/2), new Size(BackGrndImage.Width, BackGrndImage.Height));
                    e.Graphics.DrawImage((!Enabled || UseGreyScale) ? GreyScaleBackGrndImage : BackGrndImage, recImg);
                }

                // sub text 
                if (!string.IsNullOrEmpty(SubText)) {
                    var textFont = FontManager.GetFont(FontStyle.Bold, 10);
                    var textSize = TextRenderer.MeasureText(SubText, textFont);
                    var subColor = Enabled ? YamuiThemeManager.Current.SubTextFore : foreColor;

                    var drawPoint = new PointF(Width - (NoChildren ? 0 : 12) - textSize.Width - 3, (ClientRectangle.Height/2) - (textSize.Height/2) - 1);
                    // using Drawstring here because TextRender (GDI) can't draw semi transparent text
                    e.Graphics.DrawString(SubText, textFont, new SolidBrush(Color.FromArgb((int) (SubTextOpacity*255), subColor)), drawPoint);

                    using (var pen = new Pen(Color.FromArgb((int) (SubTextOpacity*0.8*255), subColor), 1) {Alignment = PenAlignment.Left}) {
                        e.Graphics.DrawPath(pen, Utilities.GetRoundedRect(drawPoint.X - 2, drawPoint.Y - 1, textSize.Width + 2, textSize.Height + 3, 3f));
                    }
                }

                // text
                TextRenderer.DrawText(e.Graphics, Text, FontManager.GetStandardFont(), new Rectangle(NoIconImage ? 8 : 35, 0, ClientRectangle.Width - (NoIconImage ? 8 : 35), ClientRectangle.Height), foreColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);

                // arrow
                if (!NoChildren) {
                    TextRenderer.DrawText(e.Graphics, ((char) 52).ToString(), FontManager.GetOtherFont("Webdings", FontStyle.Regular, (float) (Height*0.50)), new Rectangle(ClientRectangle.Width - 12, 0, 12, ClientRectangle.Height), IsFocused ? YamuiThemeManager.Current.AccentColor : foreColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
            }
        }

        #endregion
    }
}