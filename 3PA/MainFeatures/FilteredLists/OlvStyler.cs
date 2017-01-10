#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (OlvStyler.cs) is part of 3P.
// 
// 3P is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// 3P is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with 3P. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System.Drawing;
using BrightIdeasSoftware;
using YamuiFramework.Fonts;

namespace _3PA.MainFeatures.FilteredLists {
    internal static class OlvStyler {

        /// <summary>
        /// Apply thememanager theme to the treeview
        /// </summary>
        public static void StyleIt(FastObjectListView fastOlv, string emptyListString) {

            // Style the control
            fastOlv.OwnerDraw = true;
            fastOlv.Font = FontManager.GetFont(FontFunction.AutoCompletion);

            fastOlv.BackColor = ThemeManager.Current.MenuNormalBack;
            fastOlv.AlternateRowBackColor = ThemeManager.Current.MenuNormalAltBack;
            fastOlv.ForeColor = ThemeManager.Current.MenuNormalFore;

            fastOlv.HighlightBackgroundColor = ThemeManager.Current.MenuFocusedBack;
            fastOlv.HighlightForegroundColor = ThemeManager.Current.MenuFocusedFore;

            fastOlv.UnfocusedHighlightBackgroundColor = ThemeManager.Current.MenuFocusedBack;
            fastOlv.UnfocusedHighlightForegroundColor = ThemeManager.Current.MenuFocusedFore;

            // Decorate and configure hot item
            fastOlv.UseHotItem = true;
            fastOlv.HotItemStyle = new HotItemStyle {
                BackColor = ThemeManager.Current.MenuHoverBack,
                ForeColor = ThemeManager.Current.MenuHoverFore
            };

            // overlay of empty list :
            fastOlv.EmptyListMsg = emptyListString;
            TextOverlay textOverlay = fastOlv.EmptyListMsgOverlay as TextOverlay;
            if (textOverlay != null) {
                textOverlay.TextColor = ThemeManager.Current.ButtonNormalFore;
                textOverlay.BackColor = ThemeManager.Current.ButtonNormalBack;
                textOverlay.BorderColor = ThemeManager.Current.ButtonNormalBorder;
                textOverlay.BorderWidth = 4.0f;
                textOverlay.Font = FontManager.GetFont(FontStyle.Bold, 30f);
                textOverlay.Rotation = -5;
            }

            fastOlv.UseAlternatingBackColors = Config.Instance.GlobalUseAlternateBackColorOnGrid;
        }
    }
}
