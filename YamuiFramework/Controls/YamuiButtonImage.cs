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
using System.Windows.Forms;
using YamuiFramework.Helper;
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

        #endregion

        #region Paint Methods

        protected override void OnPaint(PaintEventArgs e) {
            try {
                Color backColor = YamuiThemeManager.Current.ButtonBg(BackColor, false, IsFocused, IsHovered, IsPressed, true);
                Color borderColor = YamuiThemeManager.Current.ButtonBorder(IsFocused, IsHovered, IsPressed, true);
                var img = BackGrndImage;

                if (DesignMode)
                    backColor = Color.Fuchsia;

                // draw background
                using (SolidBrush b = new SolidBrush(backColor)) {
                    e.Graphics.FillRectangle(b, ClientRectangle);
                }

                // draw main image, in greyscale if not activated
                if (!Enabled)
                    img = Utilities.MakeGrayscale3(new Bitmap(img, new Size(BackGrndImage.Width, BackGrndImage.Height)));
                var recImg = new Rectangle(new Point((ClientRectangle.Width - img.Width) / 2, (ClientRectangle.Height - img.Height) / 2), new Size(img.Width, img.Height));
                e.Graphics.DrawImage(img, recImg);

                // border
                if (DrawBorder) {
                    recImg = ClientRectangle;
                    recImg.Inflate(-2, -2);
                    if (borderColor != Color.Transparent) {
                        using (Pen b = new Pen(borderColor, 2f)) {
                            e.Graphics.DrawRectangle(b, recImg);
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
