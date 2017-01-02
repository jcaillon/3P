#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiPictureBox.cs) is part of YamuiFramework.
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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {

    [Designer("YamuiFramework.Controls.YamuiImageDesigner")]
    public class YamuiPictureBox : PictureBox {
       
        #region Fields

        [Category("Yamui")]
        public Image BackGrndImage { get; set; }

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool DrawBorder { get; set; }

        #endregion

        #region Paint Methods

        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e) {
            try {
                // draw background
                using (SolidBrush b = new SolidBrush(DesignMode ? Color.Fuchsia : YamuiThemeManager.Current.FormBack)) {
                    e.Graphics.FillRectangle(b, ClientRectangle);
                }

                // draw main image, in greyscale if not activated
                var recImg = new Rectangle(new Point((ClientRectangle.Width - BackGrndImage.Width) / 2, (ClientRectangle.Height - BackGrndImage.Height) / 2), new Size(BackGrndImage.Width, BackGrndImage.Height));
                e.Graphics.DrawImage(BackGrndImage, recImg);

                // border
                if (DrawBorder) {
                    recImg = ClientRectangle;
                    recImg.Inflate(-2, -2);
                    using (Pen b = new Pen(YamuiThemeManager.Current.ButtonNormalBorder, 2f)) {
                        e.Graphics.DrawRectangle(b, recImg);
                    }
                }
            } catch {
                // ignored
            }
        }
        #endregion
    }
    internal class YamuiImageDesigner : ControlDesigner {

        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("ImeMode");
            properties.Remove("Padding");
            properties.Remove("FlatAppearance");
            properties.Remove("FlatStyle");
            properties.Remove("AutoEllipsis");
            properties.Remove("UseCompatibleTextRendering");
            properties.Remove("Image");
            properties.Remove("ImageAlign");
            properties.Remove("ImageIndex");
            properties.Remove("ImageKey");
            properties.Remove("ImageList");
            properties.Remove("TextImageRelation");
            properties.Remove("UseVisualStyleBackColor");
            properties.Remove("Font");
            properties.Remove("RightToLeft");
            base.PreFilterProperties(properties);
        }
    }
}
