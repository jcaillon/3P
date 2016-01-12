#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (YamuiProgressIndicator.cs) is part of YamuiFramework.
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {

    [Designer("YamuiFramework.Controls.YamuiProgressIndicatorDesigner")]
    [ToolboxBitmap(typeof(ProgressBar))]
    public class YamuiProgressIndicator : Control {

        #region fields

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseCustomBackColor { get; set; }

        [Category("Yamui")]
        public int AnimateInterval {
            get { return _tmrAnimate.Interval; }
            set { _tmrAnimate.Interval = value; }
        }

        [Category("Yamui")]
        public int CircleDiameter {
            get { return _circleDiam; }
            set {
                _circleDiam = value;
                SetCirclePoints();
            }
        }

        [Category("Yamui")]
        public int CircleCount {
            get { return _circleCount; }
            set {
                if (value < 3) _circleCount = 3;
                else _circleCount = value;
                SetCirclePoints();

            }
        }

        private int _circleDiam = 5;
        private int _circleIndex;
        private int _circleCount = 9;
        private PointF[] _circlePoints;
        private Size _lastSize;
        private BufferedGraphics _graphicsBuffer;
        private readonly BufferedGraphicsContext _bufferContext = BufferedGraphicsManager.Current;
        private readonly Timer _tmrAnimate = new Timer();
        private UnitVector _unitVector = new UnitVector();
        #endregion

        #region constructor

        public YamuiProgressIndicator() {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint |
                     ControlStyles.Selectable |
                     ControlStyles.AllPaintingInWmPaint, true);

            Size = new Size(30, 30);
            SetCirclePoints();

            _tmrAnimate.Interval = 300;
            _tmrAnimate.Tick += _tmrAnimate_Tick;
            _tmrAnimate.Start();
        }

        #endregion

        #region paint
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

        protected override void OnPaint(PaintEventArgs e) {
            if (!UseCustomBackColor)
                PaintTransparentBackground(_graphicsBuffer.Graphics, DisplayRectangle);
            else
                _graphicsBuffer.Graphics.Clear(BackColor);

            for (int i = 0; i < _circlePoints.Length; i++) {
                _graphicsBuffer.Graphics.FillEllipse(_circleIndex == i ? new SolidBrush(YamuiThemeManager.Current.AccentColor) : new SolidBrush(YamuiThemeManager.Current.ButtonColorsHoverBackColor), _circlePoints[i].X,
                    _circlePoints[i].Y, _circleDiam, _circleDiam);
            }

            _graphicsBuffer.Render(e.Graphics);
        }
        #endregion

        #region private methods
        private void _tmrAnimate_Tick(object sender, EventArgs e) {
            if (_circleIndex.Equals(0)) {
                _circleIndex = _circlePoints.Length - 1;
            } else {
                _circleIndex--;
            }

            Invalidate(false);
        }

        private void UpdateGraphicsBuffer() {
            if (Width > 0 && Height > 0) {
                _bufferContext.MaximumBuffer = new Size(Width + 1, Height + 1);
                _graphicsBuffer = _bufferContext.Allocate(CreateGraphics(), ClientRectangle);
                _graphicsBuffer.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            }
        }

        private void SetCirclePoints() {
            var pointStack = new Stack<PointF>();
            PointF centerPoint = new PointF(Width/2f, Height/2f);

            for (float i = 0; i < 360f; i += 360f/_circleCount) {
                _unitVector.SetValues(centerPoint, Width/2 - _circleDiam, i);
                PointF newPoint = _unitVector.EndPoint;
                newPoint = new PointF(newPoint.X - _circleDiam/2f, newPoint.Y - _circleDiam/2f);
                pointStack.Push(newPoint);
            }

            _circlePoints = pointStack.ToArray();
        }

        protected override void OnSizeChanged(EventArgs e) {
            base.OnSizeChanged(e);
            LockAspectRatio();
            UpdateGraphicsBuffer();
            SetCirclePoints();
            _lastSize = Size;
        }

        private void LockAspectRatio() {
            if (_lastSize.Height != Height) {
                Width = Height;
            } else if (_lastSize.Width != Width) {
                Height = Width;
            }
        }

        protected override void OnEnabledChanged(EventArgs e) {
            base.OnEnabledChanged(e);
            _tmrAnimate.Enabled = Enabled;
        }

        #endregion

    }

    struct UnitVector {
        private double _rise, _run;
        private PointF _startPoint;

        public void SetValues(PointF startPoint, int length, double angleInDegrees) {
            _startPoint = startPoint;

            // Convert degrees to angle
            double radian = Math.PI * angleInDegrees / 180.0;
            if (radian > Math.PI * 2) radian = Math.PI * 2;
            if (radian < 0) radian = 0;

            // Set rise over run
            _rise = _run = length;
            _rise = Math.Sin(radian) * _rise;
            _run = Math.Cos(radian) * _run;
        }

        /// <summary>
        /// Gets the point at the end of the unit vector. It will offset from the start point
        /// by the length of the vector at the specified angle
        /// </summary>
        public PointF EndPoint {
            get {
                float xPos = (float)(_startPoint.Y + _rise);
                float yPos = (float)(_startPoint.X + _run);
                // x and y pos will be swapped because we are working with rise/run
                return new PointF(yPos, xPos);
            }
        }
    }

    internal class YamuiProgressIndicatorDesigner : ControlDesigner {

        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("ImeMode");

            base.PreFilterProperties(properties);
        }
    }
}
