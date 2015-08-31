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
    [Designer("YamuiFramework.Controls.YamuiProgressBarDesigner")]
    [ToolboxBitmap(typeof(ProgressBar))]
    public class YamuiProgressBar : Control {
        #region Fields

        private ContentAlignment _textAlign = ContentAlignment.MiddleRight;
        [DefaultValue(ContentAlignment.MiddleRight)]
        [Category("Yamui")]
        public ContentAlignment TextAlign {
            get { return _textAlign; }
            set { _textAlign = value; }
        }

        private bool _hideProgressText = true;
        [DefaultValue(true)]
        [Category("Yamui")]
        public bool HideProgressText {
            get { return _hideProgressText; }
            set { _hideProgressText = value; }
        }

        private ProgressBarStyle _progressBarStyle = ProgressBarStyle.Continuous;
        [DefaultValue(ProgressBarStyle.Continuous)]
        [Category("Yamui")]
        public ProgressBarStyle ProgressBarStyle {
            get { return _progressBarStyle; }
            set { _progressBarStyle = value; }
        }

        [DefaultValue(0)]
        [Category("Yamui")]
        public int Maximum { get; set; }

        private int _currentValue;
        [DefaultValue(0)]
        [Category("Yamui")]
        public int CurrentValue {
            get { return _currentValue; }
            set { _currentValue = Math.Max(value, Maximum); Invalidate(); }
        }

        [Browsable(false)]
        public double ProgressTotalPercent {
            get { return ((1 - (double)(Maximum - CurrentValue) / Maximum) * 100); }
        }

        [Browsable(false)]
        public double ProgressTotalValue {
            get { return (1 - (double)(Maximum - CurrentValue) / Maximum); }
        }

        [Browsable(false)]
        public string ProgressPercentText {
            get { return (string.Format("{0}%", Math.Round(ProgressTotalPercent))); }
        }

        private double ProgressBarWidth {
            get { return (((double)CurrentValue / Maximum) * ClientRectangle.Width); }
        }

        private int ProgressBarMarqueeWidth {
            get { return (ClientRectangle.Width / 3); }
        }

        #endregion

        #region Constructor

        public YamuiProgressBar() {
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
                Color backColor = ThemeManager.ButtonColors.BackGround(BackColor, false, false, false, false, Enabled);
                if (backColor != Color.Transparent)
                    e.Graphics.Clear(backColor);
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

        protected virtual void OnPaintForeground(PaintEventArgs e) {
            if (_progressBarStyle == ProgressBarStyle.Continuous) {
                if (!DesignMode) StopTimer();

                DrawProgressContinuous(e.Graphics);
            } else if (_progressBarStyle == ProgressBarStyle.Blocks) {
                if (!DesignMode) StopTimer();

                DrawProgressContinuous(e.Graphics);
            } else if (_progressBarStyle == ProgressBarStyle.Marquee) {
                if (!DesignMode && Enabled) StartTimer();
                if (!Enabled) StopTimer();

                if (CurrentValue == Maximum) {
                    StopTimer();
                    DrawProgressContinuous(e.Graphics);
                } else {
                    DrawProgressMarquee(e.Graphics);
                }
            }

            DrawProgressText(e.Graphics);

            using (Pen p = new Pen(ThemeManager.Current.ButtonColorsNormalBorderColor)) {
                Rectangle borderRect = new Rectangle(0, 0, Width - 1, Height - 1);
                e.Graphics.DrawRectangle(p, borderRect);
            }
        }

        private void DrawProgressContinuous(Graphics graphics) {
            graphics.FillRectangle(new SolidBrush(ThemeManager.AccentColor), 0, 0, (int)ProgressBarWidth, ClientRectangle.Height);
        }

        private int _marqueeX;

        private void DrawProgressMarquee(Graphics graphics) {
            graphics.FillRectangle(new SolidBrush(ThemeManager.AccentColor), _marqueeX, 0, ProgressBarMarqueeWidth, ClientRectangle.Height);
        }

        private void DrawProgressText(Graphics graphics) {
            if (HideProgressText) return;

            Color foreColor = ThemeManager.ButtonColors.ForeGround(ForeColor, false, false, false, false, Enabled);

            TextRenderer.DrawText(graphics, ProgressPercentText, FontManager.GetStandardFont(), ClientRectangle, foreColor, FontManager.GetTextFormatFlags(TextAlign));
        }

        #endregion

        #region Overridden Methods

        public override Size GetPreferredSize(Size proposedSize) {
            Size preferredSize;
            base.GetPreferredSize(proposedSize);

            using (var g = CreateGraphics()) {
                proposedSize = new Size(int.MaxValue, int.MaxValue);
                preferredSize = TextRenderer.MeasureText(g, ProgressPercentText, FontManager.GetStandardFont(), proposedSize, FontManager.GetTextFormatFlags(TextAlign));
            }

            return preferredSize;
        }

        #endregion

        #region Private Methods

        private Timer _marqueeTimer;
        private bool MarqueeTimerEnabled {
            get {
                return _marqueeTimer != null && _marqueeTimer.Enabled;
            }
        }

        private void StartTimer() {
            if (MarqueeTimerEnabled) return;

            if (_marqueeTimer == null) {
                _marqueeTimer = new Timer { Interval = 10 };
                _marqueeTimer.Tick += marqueeTimer_Tick;
            }

            _marqueeX = -ProgressBarMarqueeWidth;

            _marqueeTimer.Stop();
            _marqueeTimer.Start();

            _marqueeTimer.Enabled = true;

            Invalidate();
        }
        private void StopTimer() {
            if (_marqueeTimer == null) return;

            _marqueeTimer.Stop();

            Invalidate();
        }

        private void marqueeTimer_Tick(object sender, EventArgs e) {
            _marqueeX++;

            if (_marqueeX > ClientRectangle.Width) {
                _marqueeX = -ProgressBarMarqueeWidth;
            }

            Invalidate();
        }

        #endregion
    }

    internal class YamuiProgressBarDesigner : ControlDesigner {

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

            properties.Remove("BackColor");
            properties.Remove("BackgroundImage");
            properties.Remove("BackgroundImageLayout");
            properties.Remove("UseVisualStyleBackColor");

            properties.Remove("Font");
            properties.Remove("ForeColor");
            properties.Remove("RightToLeft");
            properties.Remove("Text");

            base.PreFilterProperties(properties);
        }
    }
}
