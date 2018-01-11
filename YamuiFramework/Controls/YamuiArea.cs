#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiArea.cs) is part of YamuiFramework.
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

namespace YamuiFramework.Controls {
    public class YamuiArea : UserControl {
        #region constructor

        public YamuiArea() {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.Opaque, true);

            // this usercontrol should not be able to get the focus
            SetStyle(ControlStyles.Selectable, false);

            // this control is only visible in design mode
            Visible = DesignMode;
            Enabled = DesignMode;
        }

        #endregion

        #region Paint

        protected void PaintTransparentBackground(Graphics graphics, Rectangle clipRect) {
            graphics.Clear(Color.Transparent);
            if ((Parent != null)) {
                clipRect.Offset(Location);
                var e = new PaintEventArgs(graphics, clipRect);
                var state = graphics.Save();
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
                try {
                    graphics.TranslateTransform(-Location.X, -Location.Y);
                    InvokePaintBackground(Parent, e);
                    InvokePaint(Parent, e);
                } finally {
                    graphics.Restore(state);
                    clipRect.Offset(-Location.X, -Location.Y);
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e) {}

        protected override void OnPaint(PaintEventArgs e) {
            // background
            PaintTransparentBackground(e.Graphics, DisplayRectangle);

            // border?
            using (var p = new Pen(BackColor, 1)) {
                var borderRect = new Rectangle(0, 0, Width - 1, Height - 1);
                e.Graphics.DrawRectangle(p, borderRect);
            }
        }

        #endregion

        #region public

        public void SetPropertyOnArea(string propertyName, object value) {
            if (Parent == null)
                return;
            var rect = new Rectangle(Location, Size);
            foreach (Control control in Parent.Controls) {
                // the control is within the border of this Area? (based on the top/left corner)
                if (rect.Contains(control.Location) && !(control is YamuiArea)) {
                    try {
                        // set the property requested
                        PropertyInfo propertyInfo = control.GetType().GetProperty(propertyName);
                        if (propertyInfo != null)
                            propertyInfo.SetValue(control, value, null);
                    } catch (Exception) {
                        // ignored
                    }
                }
            }
        }

        #endregion
    }
}