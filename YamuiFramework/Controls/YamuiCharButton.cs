using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {

    [Designer("YamuiFramework.Controls.YamuiGoBackButtonDesigner")]
    [ToolboxBitmap(typeof(Button))]
    [DefaultEvent("Click")]
    public class YamuiCharButton : YamuiButton {
       
        #region Fields
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseWingdings { get; set; }

        [DefaultValue("ç")]
        [Category("Yamui")]
        public string ButtonChar { get; set; }

        private bool _fakeDisabled;

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool FakeDisabled {
            get { return _fakeDisabled; }
            set { _fakeDisabled = value; Invalidate(); }
        }

        #endregion

        #region Paint Methods
        protected override void OnPaint(PaintEventArgs e) {
            try {
                Color backColor = ThemeManager.ButtonColors.BackGround(BackColor, UseCustomBackColor, IsFocused, IsHovered, IsPressed, Enabled && !FakeDisabled);
                Color borderColor = ThemeManager.ButtonColors.BorderColor(IsFocused, IsHovered, IsPressed, Enabled && !FakeDisabled);
                Color foreColor = ThemeManager.ButtonColors.ForeGround(ForeColor, UseCustomForeColor, IsFocused, IsHovered, IsPressed, Enabled && !FakeDisabled);

                var designRect = ClientRectangle;
                designRect.Width -= 2;
                designRect.Height -= 2;

                PaintTransparentBackground(e.Graphics, DisplayRectangle);
                if (backColor != Color.Transparent) {
                    e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                    using (SolidBrush b = new SolidBrush(backColor)) {
                        e.Graphics.FillEllipse(b, designRect);
                    }
                    e.Graphics.SmoothingMode = SmoothingMode.Default;
                }

                if (borderColor != Color.Transparent) {
                    e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                    using (Pen b = new Pen(borderColor)) {
                        e.Graphics.DrawEllipse(b, designRect);
                    }
                    e.Graphics.SmoothingMode = SmoothingMode.Default;
                }

                designRect.Width += 2;
                designRect.Height += 2;
                TextRenderer.DrawText(e.Graphics, ButtonChar, new Font((UseWingdings) ? "Wingdings" : "Webdings", (float)(Height*0.45)), designRect, foreColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            } catch {
                // ignored
            }
        }
        #endregion
    }
    internal class YamuiGoBackButtonDesigner : ControlDesigner {

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
