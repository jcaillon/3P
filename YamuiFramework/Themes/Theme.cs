#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (Theme.cs) is part of YamuiFramework.
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
using System.Drawing;

namespace YamuiFramework.Themes {
    public class Theme {
        
        /* public members will be exported to the xml configuration file (Class2xml) */
        /* This is the classic theme : */
        public string ThemeName = "Classic theme";

        public int UniqueId = 0;
        public int RankNeeded = 0; /* rank needed by the user to access this theme */
        public string PageBackGroundImage = "";

        public bool UseCurrentAccentColor = true;
        public Color AccentColor = ThemeManager.AccentColor; /* default accent color for this theme, read only when loading theme */

		public Color FormColorBackColor                    = Color.FromArgb(230, 230, 230);
		public Color FormColorForeColor                    = Color.FromArgb(30, 30, 30);
		public Color ScrollBarsColorsNormalBackColor       = Color.FromArgb(204, 204, 204);
		public Color ScrollBarsColorsNormalForeColor       = Color.FromArgb(102, 102, 102);
		public Color ScrollBarsColorsHoverBackColor        = Color.FromArgb(204, 204, 204);
		public Color ScrollBarsColorsHoverForeColor        = Color.FromArgb(37, 37, 38);
		public Color ScrollBarsColorsDisabledBackColor     = Color.FromArgb(230, 230, 230);
		public Color ScrollBarsColorsDisabledForeColor     = Color.FromArgb(179, 179, 179);
		public Color LabelsColorsNormalForeColor           = Color.FromArgb(30, 30, 30);
		public Color LabelsColorsPressForeColor            = Color.FromArgb(0, 0, 0);
		public Color LabelsColorsDisabledForeColor         = Color.FromArgb(150, 150, 150);
		public Color TabsColorsNormalBackColor             = Color.FromArgb(230, 230, 230);
		public Color TabsColorsNormalForeColor             = Color.FromArgb(110, 110, 110);
		public Color TabsColorsHoverForeColor              = Color.FromArgb(60, 60, 60);
		public Color TabsColorsPressForeColor              = Color.FromArgb(30, 30, 30);
		public Color ButtonColorsNormalBackColor           = Color.FromArgb(230, 230, 230);
		public Color ButtonColorsNormalForeColor           = Color.FromArgb(30, 30, 30);
		public Color ButtonColorsNormalBorderColor         = Color.FromArgb(190, 190, 190);
		public Color ButtonColorsHoverBackColor            = Color.FromArgb(210, 210, 210);
		public Color ButtonColorsHoverForeColor            = Color.FromArgb(30, 30, 30);
		public Color ButtonColorsHoverBorderColor          = Color.FromArgb(190, 190, 190);
		public Color ButtonColorsPressForeColor            = Color.FromArgb(30, 30, 30);
		public Color ButtonColorsDisabledBackColor         = Color.FromArgb(230, 230, 230);
		public Color ButtonColorsDisabledForeColor         = Color.FromArgb(100, 100, 100);
		public Color ButtonColorsDisabledBorderColor       = Color.FromArgb(190, 190, 190);

		// special for 3P
		public Color AutoCompletionNormalBackColor         = Color.FromArgb(250, 250, 250);
		public Color AutoCompletionNormalAlternateBackColor= Color.FromArgb(230, 230, 230);
		public Color AutoCompletionNormalForeColor         = Color.FromArgb(30, 30, 30);
		public Color AutoCompletionNormalSubTypeForeColor  = Color.FromArgb(100, 154, 209);
		public Color AutoCompletionHoverBackColor          = Color.FromArgb(206, 226, 252);
		public Color AutoCompletionHoverForeColor          = Color.FromArgb(0, 0, 0);
		public Color AutoCompletionFocusBackColor          = Color.FromArgb(154, 194, 249);
		public Color AutoCompletionFocusForeColor          = Color.FromArgb(0, 0, 0);
		public Color AutoCompletionHighlightBack           = Color.FromArgb(254, 228, 101);
		public Color AutoCompletionHighlightBorder         = Color.FromArgb(255, 171, 0);

        public Color GenericLinkColor = Color.FromArgb(95, 158, 142);
        public Color GenericErrorColor = Color.OrangeRed;
    }

}
