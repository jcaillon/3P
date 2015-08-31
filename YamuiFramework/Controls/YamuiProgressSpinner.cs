using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {
    [Designer("YamuiFramework.Controls.YamuiProgressSpinnerDesigner")]
    [ToolboxBitmap(typeof(ProgressBar))]
    public class YamuiProgressSpinner : Control {
        #region Fields

        private Timer _timer;
        private int _progress;
        private float _angle = 270;

        [DefaultValue(true)]
        [Category("Yamui")]
        public bool Spinning {
            get { return _timer.Enabled; }
            set { _timer.Enabled = value; }
        }

        [DefaultValue(0)]
        [Category("Yamui")]
        public int Value {
            get { return _progress; }
            set {
                if (value != -1 && (value < _minimum || value > _maximum))
                    throw new ArgumentOutOfRangeException("Progress value must be -1 or between Minimum and Maximum.", (Exception)null);
                _progress = value;
                Refresh();
            }
        }

        private int _minimum;
        [DefaultValue(0)]
        [Category("Yamui")]
        public int Minimum {
            get { return _minimum; }
            set {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Minimum value must be >= 0.", (Exception)null);
                if (value >= _maximum)
                    throw new ArgumentOutOfRangeException("Minimum value must be < Maximum.", (Exception)null);
                _minimum = value;
                if (_progress != -1 && _progress < _minimum)
                    _progress = _minimum;
                Refresh();
            }
        }

        private int _maximum = 100;
        [DefaultValue(0)]
        [Category("Yamui")]
        public int Maximum {
            get { return _maximum; }
            set {
                if (value <= _minimum)
                    throw new ArgumentOutOfRangeException("Maximum value must be > Minimum.", (Exception)null);
                _maximum = value;
                if (_progress > _maximum)
                    _progress = _maximum;
                Refresh();
            }
        }

        private bool _ensureVisible = true;
        [DefaultValue(true)]
        [Category("Yamui")]
        public bool EnsureVisible {
            get { return _ensureVisible; }
            set { _ensureVisible = value; Refresh(); }
        }

        private float _speed;
        [DefaultValue(1f)]
        [Category("Yamui")]
        public float Speed {
            get { return _speed; }
            set {
                if (value <= 0 || value > 10)
                    throw new ArgumentOutOfRangeException("Speed value must be > 0 and <= 10.", (Exception)null);

                _speed = value;
            }
        }

        private bool _backwards;
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool Backwards {
            get { return _backwards; }
            set { _backwards = value; Refresh(); }
        }
        #endregion

        #region Constructor

        public YamuiProgressSpinner() {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);

            _timer = new Timer();
            _timer.Interval = 20;
            _timer.Tick += timer_Tick;
            _timer.Enabled = true;

            Width = 16;
            Height = 16;
            _speed = 1;
            DoubleBuffered = true;
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
            Color foreColor = ThemeManager.AccentColor;

            using (Pen forePen = new Pen(foreColor, (float)Width / 5)) {
                int padding = (int)Math.Ceiling((float)Width / 10);

                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

                if (_progress != -1) {
                    float sweepAngle;
                    float progFrac = (_progress - _minimum) / (float)(_maximum - _minimum);

                    if (_ensureVisible) {
                        sweepAngle = 30 + 300f * progFrac;
                    } else {
                        sweepAngle = 360f * progFrac;
                    }

                    if (_backwards) {
                        sweepAngle = -sweepAngle;
                    }

                    e.Graphics.DrawArc(forePen, padding, padding, Width - 2 * padding - 1, Height - 2 * padding - 1, _angle, sweepAngle);
                } else {
                    const int maxOffset = 180;
                    for (int offset = 0; offset <= maxOffset; offset += 15) {
                        int alpha = 290 - (offset * 290 / maxOffset);

                        if (alpha > 255) {
                            alpha = 255;
                        }
                        if (alpha < 0) {
                            alpha = 0;
                        }

                        Color col = Color.FromArgb(alpha, forePen.Color);
                        using (Pen gradPen = new Pen(col, forePen.Width)) {
                            float startAngle = _angle + (offset - (_ensureVisible ? 30 : 0)) * (_backwards ? 1 : -1);
                            float sweepAngle = 15 * (_backwards ? 1 : -1);
                            e.Graphics.DrawArc(gradPen, padding, padding, Width - 2 * padding - 1, Height - 2 * padding - 1, startAngle, sweepAngle);
                        }
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        public void Reset() {
            _progress = _minimum;
            _angle = 270;
            Refresh();
        }

        #endregion

        #region Management Methods

        private void timer_Tick(object sender, EventArgs e) {
            if (!DesignMode) {
                _angle += 6f * _speed * (_backwards ? -1 : 1);
                Refresh();
            }
        }

        #endregion

    }

    internal class YamuiProgressSpinnerDesigner : ControlDesigner {
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
