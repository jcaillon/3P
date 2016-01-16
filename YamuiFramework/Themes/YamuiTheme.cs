#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiTheme.cs) is part of YamuiFramework.
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
using System.Drawing;

namespace YamuiFramework.Themes {

    public class YamuiTheme {

        public delegate Image ImageGetter();
        public static event ImageGetter OnImageNeeded;

        /// <summary>
        /// This field is dynamic and can be modified by the calling program
        /// </summary>
        public Color AccentColor = Color.DarkSlateGray;


        #region Stored in the config file

        /// <summary>
        /// Theme's name
        /// </summary>
        public string ThemeName;

        public int UniqueId = 0;

        public int RankNeeded = 0;
        public string PageBackGroundImage = "";

        public Color ThemeAccentColor = Color.DarkSlateGray;
        public Color FormColorBackColor = Color.FromArgb(230, 230, 230);
        public Color FormColorForeColor = Color.FromArgb(30, 30, 30);
        public Color ScrollBarsColorsNormalBackColor = Color.FromArgb(204, 204, 204);
        public Color ScrollBarsColorsNormalForeColor = Color.FromArgb(102, 102, 102);
        public Color ScrollBarsColorsHoverBackColor = Color.FromArgb(204, 204, 204);
        public Color ScrollBarsColorsHoverForeColor = Color.FromArgb(37, 37, 38);
        public Color ScrollBarsColorsDisabledBackColor = Color.FromArgb(230, 230, 230);
        public Color ScrollBarsColorsDisabledForeColor = Color.FromArgb(179, 179, 179);
        public Color LabelsColorsNormalForeColor = Color.FromArgb(30, 30, 30);
        public Color LabelsColorsPressForeColor = Color.FromArgb(0, 0, 0);
        public Color LabelsColorsDisabledForeColor = Color.FromArgb(150, 150, 150);
        public Color TabsColorsNormalBackColor = Color.FromArgb(230, 230, 230);
        public Color TabsColorsNormalForeColor = Color.FromArgb(110, 110, 110);
        public Color TabsColorsHoverForeColor = Color.FromArgb(60, 60, 60);
        public Color TabsColorsPressForeColor = Color.FromArgb(30, 30, 30);
        public Color ButtonColorsNormalBackColor = Color.FromArgb(230, 230, 230);
        public Color ButtonColorsNormalForeColor = Color.FromArgb(30, 30, 30);
        public Color ButtonColorsNormalBorderColor = Color.FromArgb(190, 190, 190);
        public Color ButtonColorsHoverBackColor = Color.FromArgb(210, 210, 210);
        public Color ButtonColorsHoverForeColor = Color.FromArgb(30, 30, 30);
        public Color ButtonColorsHoverBorderColor = Color.FromArgb(190, 190, 190);
        public Color ButtonColorsPressForeColor = Color.FromArgb(30, 30, 30);
        public Color ButtonColorsDisabledBackColor = Color.FromArgb(230, 230, 230);
        public Color ButtonColorsDisabledForeColor = Color.FromArgb(100, 100, 100);
        public Color ButtonColorsDisabledBorderColor = Color.FromArgb(190, 190, 190);

        #endregion

        /// <summary>
        /// Gets the background image for the current theme
        /// </summary>
        public Image GetThemeImage() {
            var yamuiImage = (!string.IsNullOrEmpty(PageBackGroundImage) ? (Image)Resources.Resources.ResourceManager.GetObject(PageBackGroundImage) : null);
            // can't find the image locally (in Yamui), use the event to try and find one from the user program
            if (yamuiImage == null && OnImageNeeded != null) {
                yamuiImage = OnImageNeeded();
            }
            return yamuiImage;
        }

        /// <summary>
        /// This class is used for sliders as well as scrollbars
        /// </summary>
        public Color ScrollBarsBg(bool isFocused, bool isHovered, bool isPressed, bool enabled) {
            Color backColor;
            if (!enabled)
                backColor = ScrollBarsColorsDisabledBackColor;
            else if (isPressed)
                backColor = ScrollBarsColorsNormalBackColor;
            else if (isHovered || isFocused)
                backColor = ScrollBarsColorsHoverBackColor;
            else
                backColor = ScrollBarsColorsNormalBackColor;

            return backColor;
        }

        public Color ScrollBarsFg(bool isFocused, bool isHovered, bool isPressed, bool enabled) {
            Color foreColor;

            if (!enabled)
                foreColor = ScrollBarsColorsDisabledForeColor;
            else if (isPressed)
                foreColor = AccentColor;
            else if (isHovered || isFocused)
                foreColor = ScrollBarsColorsHoverForeColor;
            else
                foreColor = ScrollBarsColorsNormalForeColor;

            return foreColor;
        }

        /// <summary>
        ///     This class is used for labels as well as links
        /// </summary>
        public Color LabelsFg(Color controlForeColor, bool useCustomForeColor, bool isFocused, bool isHovered, bool isPressed, bool enabled) {
            Color foreColor;
                
            if (useCustomForeColor)
                foreColor = controlForeColor;
            else if (!enabled)
                foreColor = LabelsColorsDisabledForeColor;
            else if (isPressed)
                foreColor = LabelsColorsPressForeColor;
            else if (isHovered || isFocused)
                foreColor = AccentColor;
            else
                foreColor = LabelsColorsNormalForeColor;

            return foreColor;
        }

        public Color LabelsBg(Color controlBackColor, bool useCustomBackColor) {
            return !useCustomBackColor ? Color.Transparent : controlBackColor;
        }

        /// <summary>
        ///     This class is used for tab controls (back color is also used for tab pages)
        /// </summary>
        public Color TabsFg(bool isFocused, bool isHovered, bool isSelected) {
            Color foreColor;

            if (isFocused && isSelected)
                foreColor = AccentColor;
            else if (isSelected)
                foreColor = TabsColorsPressForeColor;
            else if (isHovered)
                foreColor = TabsColorsHoverForeColor;
            else
                foreColor = TabsColorsNormalForeColor;

            return foreColor;
        }

        /// <summary>
        ///     This class is used for :
        ///     - Buttons
        ///     - CheckBoxes
        ///     - ComboBoxes
        ///     - DatePicker
        ///     - RadioButtons
        /// </summary>
        public Color ButtonBg(Color controlBackColor, bool useCustomBackColor, bool isFocused, bool isHovered, bool isPressed, bool enabled) {
            Color backColor;

            if (useCustomBackColor)
                backColor = controlBackColor;
            else if (!enabled)
                backColor = ButtonColorsDisabledBackColor;
            else if (isPressed)
                backColor = AccentColor;
            else if (isHovered)
                backColor = ButtonColorsHoverBackColor;
            else
                backColor = ButtonColorsNormalBackColor;

            return backColor;
        }

        public Color ButtonFg(Color controlForeColor, bool useCustomForeColor, bool isFocused, bool isHovered, bool isPressed, bool enabled) {
            Color foreColor;

            if (useCustomForeColor)
                foreColor = controlForeColor;
            else if (!enabled)
                foreColor = ButtonColorsDisabledForeColor;
            else if (isPressed)
                foreColor = ButtonColorsPressForeColor;
            else if (isHovered)
                foreColor = ButtonColorsHoverForeColor;
            else
                foreColor = ButtonColorsNormalForeColor;

            return foreColor;
        }

        public Color ButtonBorder(bool isFocused, bool isHovered, bool isPressed, bool enabled) {
            Color borderColor;

            if (!enabled)
                borderColor = ButtonColorsDisabledBorderColor;
            else if (isPressed)
                borderColor = AccentColor;
            else if (isFocused)
                borderColor = AccentColor;
            else if (isHovered)
                borderColor = ButtonColorsHoverBorderColor;
            else
                borderColor = ButtonColorsNormalBorderColor;

            return borderColor;
        }
    }
}
