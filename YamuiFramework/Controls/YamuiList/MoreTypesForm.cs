#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (MoreTypesForm.cs) is part of YamuiFramework.
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
using System.Windows.Forms;
using YamuiFramework.Forms;
using YamuiFramework.Helper;
using YamuiFramework.HtmlRenderer.WinForms;

namespace YamuiFramework.Controls.YamuiList {

    /// <summary>
    /// A class to display a cool custom context menu
    /// </summary>
    public sealed class MoreTypesForm : YamuiFormBaseShadow {

        #region private fields

        private HtmlToolTip _tooltip = new HtmlToolTip();
        private YamuiSimplePanel _panel;
        private const int MousePaddingInsideForm = 10;
        private YamuiFilteredTypeList _parentFilteredList;

        #endregion

        #region Don't show in ATL+TAB

        protected override CreateParams CreateParams {
            get {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= (int)WinApi.WindowStylesEx.WS_EX_TOPMOST;
                createParams.ExStyle |= (int)WinApi.WindowStylesEx.WS_EX_TOOLWINDOW;
                return createParams;
            }
        }

        #endregion

        #region Life and death

        public MoreTypesForm() {

            // init menu form
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);

            ControlBox = false;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.Manual;
            Movable = false;
            Resizable = false;

            SuspendLayout();
            _panel = new YamuiSimplePanel {
                Dock = DockStyle.Fill,
                DontUseTransparentBackGround = true,
                Location = new Point(0, 0)
            };
            Controls.Add(_panel);
            Padding = new Padding(2);
            ResumeLayout();
        }

        public void Build(Point location, List<int> typeList, Action<object, EventArgs> clickHandler, YamuiFilteredTypeList parentList) {

            _parentFilteredList = parentList;

            var buttonSize = new Size(YamuiFilteredTypeList.TypeButtonWidth, parentList.BottomHeight);

            // for each distinct type of items, create the buttons
            int xPos = -buttonSize.Width;
            int yPos = 0;
            int nBut = 0;
            int maxNbButPerRow = (int)Math.Ceiling(Math.Sqrt(typeList.Count));
            foreach (var type in typeList) {

                xPos += buttonSize.Width;
                if (nBut >= maxNbButPerRow) {
                    yPos += buttonSize.Height;
                    xPos = 0;
                    nBut = 0;
                }

                var button = new SelectorButton {
                    Size = buttonSize,
                    TabStop = false,
                    Anchor = AnchorStyles.Left | AnchorStyles.Top,
                    AcceptsAnyClick = true,
                    HideFocusedIndicator = true,
                    Activated = true,
                    BackGrndImage = parentList.TypeImages.ContainsKey(type) ? parentList.TypeImages[type] : null,
                    Type = type,
                    Location = new Point(xPos, yPos)
                };
                button.ButtonPressed += (sender, args) => {
                    clickHandler(sender, args);
                    foreach (SelectorButton control in _panel.Controls) {
                        control.Activated = _parentFilteredList.IsTypeActivated(control.Type);
                    }
                };
                button.LostFocus += (sender, args) => CloseIfMouseOut();
                button.Activated = _parentFilteredList.IsTypeActivated(type);
                _tooltip.SetToolTip(button, (parentList.TypeText.ContainsKey(type) && parentList.TypeText[type] != null ? parentList.TypeText[type] + "<br>" : "") + YamuiFilteredTypeList.TypeButtonTooltipText);
                
                if (!_panel.Controls.Contains(button))
                    _panel.Controls.Add(button);

                nBut++;
            }


            // Size the form
            Size = new Size(maxNbButPerRow * buttonSize.Width + BorderWidth * 2, (int)Math.Ceiling((double) typeList.Count / maxNbButPerRow) * buttonSize.Height + BorderWidth * 2);
            MinimumSize = Size;
            MaximumSize = Size;

            // menu position
            var screen = Screen.FromPoint(location);
            if (location.X - MousePaddingInsideForm > screen.WorkingArea.X + screen.WorkingArea.Width / 2) {
                location.X = location.X - Width + MousePaddingInsideForm;
            } else
                location.X -= MousePaddingInsideForm;
            if (location.Y - MousePaddingInsideForm > screen.WorkingArea.Y + screen.WorkingArea.Height / 2) {
                location.Y = location.Y - Height + MousePaddingInsideForm;
            } else
                location.Y -= MousePaddingInsideForm;

            Location = location;
        }

        #endregion

        #region Events
        
        protected override void OnMouseLeave(EventArgs e) {
            base.OnMouseLeave(e);
            Close();
            Dispose();
        }
        
        protected override void OnLeave(EventArgs e) {
            base.OnLeave(e);
            Close();
            Dispose();
        }

        protected override void OnLostFocus(EventArgs e) {
            base.OnLostFocus(e);
            CloseIfMouseOut();
        }

        private void CloseIfMouseOut() {
            var rect = ClientRectangle;
            rect.Offset(Location);
            if (!rect.Contains(MousePosition)) {
                Close();
                Dispose();
            }
        }

        #endregion

    }

}
