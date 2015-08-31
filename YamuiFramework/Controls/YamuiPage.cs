using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls
{
    public class YamuiPage : UserControl
    {
        #region Fields
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseCustomBackColor { get; set; }
        #endregion

        #region Paint
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
                if (DesignMode) {
                    e.Graphics.Clear(ThemeManager.Current.FormColorBackColor);
                    return;
                }
                if (UseCustomBackColor) 
                    e.Graphics.Clear(BackColor);
                else
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

        protected virtual void OnPaintForeground(PaintEventArgs e) {}

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }
        #endregion
    }
}
