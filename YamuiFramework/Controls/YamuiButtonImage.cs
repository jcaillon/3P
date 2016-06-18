#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiImageButton.cs) is part of YamuiFramework.
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

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {

    [Designer("YamuiFramework.Controls.YamuiButtonDesigner")]
    [ToolboxBitmap(typeof(Button))]
    [DefaultEvent("ButtonPressed")]
    public class YamuiButtonImage : YamuiButton {
       
        #region Fields

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool DrawBorder { get; set; }

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool HideFocusedIndicator { get; set; }

        #endregion

        #region Paint Methods

        protected override void OnPaint(PaintEventArgs e) {
            try {
                Color backColor = DesignMode ? Color.Fuchsia : YamuiThemeManager.Current.ButtonImageBg(IsHovered, IsPressed);
                Color borderColor = YamuiThemeManager.Current.ButtonBorder(IsFocused, IsHovered, IsPressed, Enabled);

                // draw background
                using (SolidBrush b = new SolidBrush(backColor)) {
                    e.Graphics.FillRectangle(b, ClientRectangle);
                }

                // draw an indicator to know the image is focused
                if (!HideFocusedIndicator && IsFocused)
                    using (SolidBrush b = new SolidBrush(YamuiThemeManager.Current.ButtonImageFocusedIndicator)) {
                        GraphicsPath path = new GraphicsPath();
                        path.AddLines(new[] { new Point(0, 0), new Point(ClientRectangle.Width / 2, 0), new Point(0, ClientRectangle.Height / 2), new Point(0, 0), });
                        e.Graphics.FillPath(b, path);
                    }

                // draw main image, in greyscale if not activated
                if (BackGrndImage != null) {
                    var recImg = new Rectangle(new Point((ClientRectangle.Width - BackGrndImage.Width) / 2, (ClientRectangle.Height - BackGrndImage.Height) / 2), new Size(BackGrndImage.Width, BackGrndImage.Height));
                    e.Graphics.DrawImage((!Enabled || UseGreyScale) ? GreyScaleBackGrndImage : BackGrndImage, recImg);
                }

                // border
                if (DrawBorder) {
                    var recBorder = ClientRectangle;
                    recBorder.Inflate(-1, -1);
                    if (borderColor != Color.Transparent) {
                        using (Pen b = new Pen(borderColor, 1f)) {
                            e.Graphics.DrawRectangle(b, recBorder);
                        }
                    }
                }
            } catch {
                // ignored
            }
        }
        #endregion
    }
}
