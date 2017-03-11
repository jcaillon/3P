#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (ContextMenuAdapter.cs) is part of YamuiFramework.
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
using System.Windows.Forms;
using YamuiFramework.HtmlRenderer.Core.Adapters;
using YamuiFramework.HtmlRenderer.Core.Adapters.Entities;
using YamuiFramework.HtmlRenderer.Core.Core.Utils;
using YamuiFramework.HtmlRenderer.WinForms.Utilities;

namespace YamuiFramework.HtmlRenderer.WinForms.Adapters {
    /// <summary>
    /// Adapter for WinForms context menu for core.
    /// </summary>
    internal sealed class ContextMenuAdapter : RContextMenu {
        #region Fields and Consts

        /// <summary>
        /// the underline win forms context menu
        /// </summary>
        private readonly ContextMenuStrip _contextMenu;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        public ContextMenuAdapter() {
            _contextMenu = new ContextMenuStrip();
            _contextMenu.ShowImageMargin = false;
        }

        public override int ItemsCount {
            get { return _contextMenu.Items.Count; }
        }

        public override void AddDivider() {
            _contextMenu.Items.Add("-");
        }

        public override void AddItem(string text, bool enabled, EventHandler onClick) {
            ArgChecker.AssertArgNotNullOrEmpty(text, "text");
            ArgChecker.AssertArgNotNull(onClick, "onClick");

            var item = _contextMenu.Items.Add(text, null, onClick);
            item.Enabled = enabled;
        }

        public override void RemoveLastDivider() {
            if (_contextMenu.Items[_contextMenu.Items.Count - 1].Text == string.Empty)
                _contextMenu.Items.RemoveAt(_contextMenu.Items.Count - 1);
        }

        public override void Show(RControl parent, RPoint location) {
            _contextMenu.Show(((ControlAdapter) parent).Control, Utils.ConvertRound(location));
        }

        public override void Dispose() {
            _contextMenu.Dispose();
        }
    }
}