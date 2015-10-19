using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {

    [Designer("YamuiFramework.Controls.YamuiImageButtonDesigner")]
    [ToolboxBitmap(typeof(Button))]
    [DefaultEvent("Click")]
    public class YamuiImageButton : YamuiButton {
       
        #region Fields
        [Category("Yamui")]
        public Image BackGrndImage { get; set; }

        private bool _fakeDisabled;

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool FakeDisabled {
            get { return _fakeDisabled; }
            set { _fakeDisabled = value; Invalidate(); }
        }

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool DrawBorder { get; set; }

        #endregion

        #region Paint Methods

        protected override void OnPaint(PaintEventArgs e) {
            try {
                Color backColor = ThemeManager.ButtonColors.BackGround(BackColor, false, IsFocused, IsHovered, IsPressed, true);
                Color borderColor = ThemeManager.ButtonColors.BorderColor(IsFocused, IsHovered, IsPressed, true);
                var img = BackGrndImage;

                if (DesignMode)
                    backColor = Color.Fuchsia;

                // draw background
                using (SolidBrush b = new SolidBrush(backColor)) {
                    e.Graphics.FillRectangle(b, ClientRectangle);
                }

                // draw main image, in greyscale if not activated
                if (_fakeDisabled)
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
    internal class YamuiImageButtonDesigner : ControlDesigner {

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
