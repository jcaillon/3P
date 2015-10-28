#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (YamuiTabPage.cs) is part of YamuiFramework.
// 
// // YamuiFramework is a free software: you can redistribute it and/or modify
// // it under the terms of the GNU General Public License as published by
// // the Free Software Foundation, either version 3 of the License, or
// // (at your option) any later version.
// 
// // YamuiFramework is distributed in the hope that it will be useful,
// // but WITHOUT ANY WARRANTY; without even the implied warranty of
// // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// // GNU General Public License for more details.
// 
// // You should have received a copy of the GNU General Public License
// // along with YamuiFramework. If not, see <http://www.gnu.org/licenses/>.
// // ========================================================================
#endregion
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using YamuiFramework.Fonts;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {

    //[Designer("YamuiFramework.Controls.YamuiTabPageDesigner")]
    [ToolboxItem(false)]
    public class YamuiTabPage : TabPage {
        #region Fields
        private TabFunction _function = TabFunction.Main;
        [DefaultValue(TabFunction.Main)]
        [Category("Yamui")]
        public TabFunction Function {
            get { return _function; }
            set {
                _function = value;
                SetStuff();
            }
        }

        /// <summary>
        /// Read only after the initialisation! Set to true to hide this sheet in normal mode
        /// </summary>
        private bool _hiddenPage;
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool HiddenPage {
            get { return _hiddenPage; }
            set {
                _hiddenPage = value;
                HiddenState = _hiddenPage;
            }
        }

        [Category("Yamui")]
        [DefaultValue(false)]
        public bool UseCustomBackColor { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool HiddenState { get; set; }
        #endregion

        #region Constructor
        public YamuiTabPage() {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor, true);

            SetStuff();
        }

        public void SetStuff() {
            Padding = (Function == TabFunction.Main) ? new Padding(0, 0, 0, 0) : new Padding(15, 25, 0, 0);
            UseVisualStyleBackColor = false;
        }
        #endregion

        #region Paint
        /// <summary>
        /// DO NOT forget to change the paint method of YamuiTabAnimation (in forms) if you change this one!!!
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e) {
            Color backColor = UseCustomBackColor ? BackColor : ThemeManager.Current.TabsColorsNormalBackColor;
            e.Graphics.Clear(backColor);
            var img = ThemeManager.ThemePageImage;
            if (img != null) {
                Rectangle rect = new Rectangle(ClientRectangle.Right - img.Width, ClientRectangle.Height - img.Height, img.Width, img.Height);
                e.Graphics.DrawImage(img, rect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
            }
        }
        #endregion
    }

    #region YamuiTabPageDesigner
    internal class YamuiTabPageDesigner : ControlDesigner {
        #region Fields

        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("UseVisualStyleBackColor");
            properties.Remove("Padding");
            properties.Remove("Font");

            base.PreFilterProperties(properties);
        }

        #endregion
    }

    #endregion
}
