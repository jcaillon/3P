#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (ControlAdapter.cs) is part of YamuiFramework.
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
using System.Windows.Forms;
using YamuiFramework.HtmlRenderer.Core.Adapters;
using YamuiFramework.HtmlRenderer.Core.Adapters.Entities;
using YamuiFramework.HtmlRenderer.Core.Core.Utils;
using YamuiFramework.HtmlRenderer.WinForms.Utilities;

namespace YamuiFramework.HtmlRenderer.WinForms.Adapters {
    /// <summary>
    /// Adapter for WinForms Control for core.
    /// </summary>
    internal sealed class ControlAdapter : RControl {
        /// <summary>
        /// the underline win forms control.
        /// </summary>
        private readonly Control _control;

        /// <summary>
        /// Use GDI+ text rendering to measure/draw text.
        /// </summary>
        private readonly bool _useGdiPlusTextRendering;

        /// <summary>
        /// Init.
        /// </summary>
        public ControlAdapter(Control control, bool useGdiPlusTextRendering)
            : base(WinFormsAdapter.Instance) {
            ArgChecker.AssertArgNotNull(control, "control");

            _control = control;
            _useGdiPlusTextRendering = useGdiPlusTextRendering;
        }

        /// <summary>
        /// Get the underline win forms control
        /// </summary>
        public Control Control {
            get { return _control; }
        }

        public override RPoint MouseLocation {
            get { return Utils.Convert(_control.PointToClient(Control.MousePosition)); }
        }

        public override bool LeftMouseButton {
            get { return (Control.MouseButtons & MouseButtons.Left) != 0; }
        }

        public override bool RightMouseButton {
            get { return (Control.MouseButtons & MouseButtons.Right) != 0; }
        }

        public override void SetCursorDefault() {
            _control.Cursor = Cursors.Default;
        }

        public override void SetCursorHand() {
            _control.Cursor = Cursors.Hand;
        }

        public override void SetCursorIBeam() {
            _control.Cursor = Cursors.IBeam;
        }

        public override void DoDragDropCopy(object dragDropData) {
            _control.DoDragDrop(dragDropData, DragDropEffects.Copy);
        }

        public override void MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth) {
            using (var g = new GraphicsAdapter(_control.CreateGraphics(), _useGdiPlusTextRendering, true)) {
                g.MeasureString(str, font, maxWidth, out charFit, out charFitWidth);
            }
        }

        public override void Invalidate() {
            _control.Invalidate();
        }
    }
}