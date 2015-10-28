#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (YamuiLabel.cs) is part of YamuiFramework.
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
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using YamuiFramework.Fonts;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {
    #region Enums

    public enum LabelMode {
        Default,
        Selectable
    }

    #endregion

    [Designer("YamuiFramework.Controls.YamuiLabelDesigner")]
    [ToolboxBitmap(typeof(Label))]
    public class YamuiLabel : Label {

        #region Fields
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseCustomBackColor { get; set; }

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseCustomForeColor { get; set; }

        private LabelFunction _function = LabelFunction.Normal;
        [DefaultValue(LabelFunction.Normal)]
        [Category("Yamui")]
        public LabelFunction Function {
            get { return _function; }
            set {
                _function = value;
                Margin = _function == LabelFunction.Heading ? new Padding(5, 18, 5, 7) : new Padding(3, 3, 3, 3);
            }
        }

        private bool _fakeDisabled;

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool FakeDisabled {
            get { return _fakeDisabled; }
            set { _fakeDisabled = value; Invalidate(); }
        }

        private bool _wrapToLine;
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool WrapToLine {
            get { return _wrapToLine; }
            set { _wrapToLine = value; Refresh(); }
        }
        #endregion

        #region Constructor

        public YamuiLabel() {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);
        }

        #endregion

        #region Paint Methods

        protected void PaintTransparentBackground(Graphics graphics, Rectangle clipRect) {
            graphics.Clear(Color.Transparent);
            if ((Parent != null)) {
                clipRect.Offset(Location);
                PaintEventArgs e = new PaintEventArgs(graphics, clipRect);
                GraphicsState state = graphics.Save();
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

        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected void CustomOnPaintBackground(PaintEventArgs e) {
            try {
                Color backColor = ThemeManager.LabelsColors.BackGround(BackColor, UseCustomBackColor);
                if (backColor != Color.Transparent) {
                    e.Graphics.Clear(backColor);
                } else
                    PaintTransparentBackground(e.Graphics, DisplayRectangle);
            } catch {
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            try {
                CustomOnPaintBackground(e);
                OnPaintForeground(e);
            } catch {
                Invalidate();
            }
        }

        protected virtual void OnPaintForeground(PaintEventArgs e) {

            Color foreColor = ThemeManager.LabelsColors.ForeGround(ForeColor, UseCustomForeColor, false, false, false, !FakeDisabled);

            TextRenderer.DrawText(e.Graphics, Text, FontManager.GetLabelFont(Function), ClientRectangle, foreColor, FontManager.GetTextFormatFlags(TextAlign, _wrapToLine));
        }

        #endregion

        #region Overridden Methods
        public override Size GetPreferredSize(Size proposedSize) {
            Size preferredSize;
            base.GetPreferredSize(proposedSize);

            using (var g = CreateGraphics()) {
                proposedSize = new Size(int.MaxValue, int.MaxValue);
                preferredSize = TextRenderer.MeasureText(g, Text, FontManager.GetLabelFont(Function), proposedSize, FontManager.GetTextFormatFlags(TextAlign));
            }

            return preferredSize;
        }

        protected override void OnEnabledChanged(EventArgs e) {
            base.OnEnabledChanged(e);
            Invalidate();
        }
        #endregion
    }

    #region designer

    internal class YamuiLabelDesigner : ControlDesigner {

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

    #endregion

}
