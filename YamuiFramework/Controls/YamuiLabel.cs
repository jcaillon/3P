#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiLabel.cs) is part of YamuiFramework.
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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using YamuiFramework.Fonts;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {

    [Designer("YamuiFramework.Controls.YamuiLabelDesigner")]
    [ToolboxBitmap(typeof(Label))]
    public class YamuiLabel : YamuiControl {

        #region Fields

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseCustomBackColor { get; set; }

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseCustomForeColor { get; set; }

        private FontFunction _function = FontFunction.Normal;

        [DefaultValue(FontFunction.Normal)]
        [Category("Yamui")]
        public FontFunction Function {
            get { return _function; }
            set {
                MeasureTextCache.InvalidateCache();
                _function = value;
                Margin = _function == FontFunction.Heading ? new Padding(5, 18, 5, 7) : new Padding(3, 3, 3, 3);
            }
        }

        private bool _fakeDisabled;

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool FakeDisabled {
            get { return _fakeDisabled; }
            set {
                _fakeDisabled = value;
                Invalidate();
            }
        }

        private bool _wrapToLine;

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool WrapToLine {
            get { return _wrapToLine; }
            set {
                _wrapToLine = value;
                Refresh();
            }
        }

        protected override Padding DefaultPadding {
            get { return new Padding(0); }
        }
        
        /// <devdoc>
        ///    <para>
        ///       Gets or sets the text in the Label. Since we can have multiline support
        ///       this property just overides the base to pluck in the Multiline editor.
        ///    </para>
        /// </devdoc>
        [
            Editor("System.ComponentModel.Design.MultilineStringEditor", typeof(UITypeEditor)),
            SettingsBindable(true)
        ]
        public override string Text {
            get {
                return base.Text;
            }
            set {
                base.Text = value;
                Invalidate();
            }
        }

        [DefaultValue(ContentAlignment.TopLeft)]
        [Category("Yamui")]
        public ContentAlignment TextAlign { get; set; } = ContentAlignment.TopLeft;

        #endregion

        #region Constructor

        public YamuiLabel() {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.Selectable |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.Opaque, 
                true);
            
            SetStyle(
                ControlStyles.FixedHeight |
                ControlStyles.Selectable, 
                false);
            TabStop = false;
        }

        #endregion

        #region Paint Methods
        
        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e) {
            Color backColor = YamuiThemeManager.Current.LabelsBg(BackColor, UseCustomBackColor);
            if (backColor != Color.Transparent) {
                e.Graphics.Clear(backColor);
            } else
                PaintTransparentBackground(e.Graphics, DisplayRectangle);

            Color foreColor = YamuiThemeManager.Current.LabelsFg(ForeColor, UseCustomForeColor, false, false, false, !FakeDisabled);

            var textRect = ClientRectangle;
            textRect.Offset(Padding.Left, Padding.Top);
            textRect.Inflate(-Padding.Left - Padding.Right, -Padding.Top - Padding.Bottom);

            TextRenderer.DrawText(e.Graphics, Text, FontManager.GetFont(Function), textRect, foreColor, FontManager.GetTextFormatFlags(TextAlign, _wrapToLine));
        }

        #endregion

        #region Overridden Methods
        
        public override Size GetPreferredSize(Size proposedSize) {
            //base.GetPreferredSize(proposedSize);
            //proposedSize = new Size(int.MaxValue, int.MaxValue);
            //using (var g = CreateGraphics()) {
            //    preferredSize = TextRenderer.MeasureText(g, Text, FontManager.GetFont(Function), proposedSize, FontManager.GetTextFormatFlags(TextAlign));
            //}

            return MeasureTextCache.GetTextSize(Text, Font, proposedSize, FontManager.GetTextFormatFlags(TextAlign));
        }
        
        protected override void OnEnabledChanged(EventArgs e) {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        [DefaultValue(false), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool TabStop {
            get {
                return base.TabStop;
            }
            set {
                base.TabStop = value;
            }
        }

        #endregion

    }

    #region designer

    internal class YamuiLabelDesigner : ControlDesigner {
        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("ImeMode");
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
            properties.Remove("BorderStyle");

            base.PreFilterProperties(properties);
        }
    }

    #endregion
}