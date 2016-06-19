using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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
