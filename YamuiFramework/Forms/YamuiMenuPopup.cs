#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiMenuPopup.cs) is part of YamuiFramework.
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YamuiFramework.Themes;

namespace YamuiFramework.Forms {

    public class YamuiMenuPopup : YamuiFormBaseShadowFadeIn {

        #region Private

        protected new int _animationDuration = 150;

        private Action<YamuiMenuItem> _do;

        private int _minimumWidth = 150;

        #endregion

        #region public fields

        /// <summary>
        /// When an item is clicked, it will be fed to this method that should, in term, be calling .OnClic of said item
        /// Use this as a wrapper to handle errors for instance
        /// </summary>
        public Action<YamuiMenuItem> ClicItemWrapper {
            get {
                return _do ?? (item => {
                    if (item.OnClic != null) {
                        item.OnClic();
                    }
                });
            }
            set { _do = value; }
        }

        public Point SpawnLocation { get; set; }

        public List<YamuiMenuItem> MenuList { get; set; }

        public string TitleString { get; set; }

        public Image TitleImage { get; set; }

        public int MinimumWidth {
            get { return _minimumWidth; }
            set { _minimumWidth = value; }
        }

        #endregion

        #region Don't show in ATL+TAB

        private const int WsExToolwindow = 0x00000008;

        protected override CreateParams CreateParams {
            get {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= WsExToolwindow;
                return createParams;
            }
        }

        #endregion

    }

}
