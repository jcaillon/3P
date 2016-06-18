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

        public delegate Image ImageGetter(string imageToLoad);
        public static ImageGetter OnImageNeeded;

        /// <summary>
        /// This field is dynamic and can be modified by the calling program
        /// </summary>
        public Color AccentColor = Color.Fuchsia;

        #region Stored in the config file

        /// <summary>
        /// Theme's name
        /// </summary>
        public string ThemeName;

        public string PageBackGroundImage = "";

        public Color ThemeAccentColor = Color.DarkSlateGray;

        public Color FormBack = Color.FromArgb(230, 230, 230);
        public Color FormAltBack = Color.FromArgb(200, 200, 200);
        public Color FormFore = Color.FromArgb(30, 30, 30);
        public Color FormBorder = Color.FromArgb(100, 118, 135);
        public Color SubTextFore = Color.FromArgb(100, 154, 209);

        public Color ScrollNormalBack = Color.FromArgb(204, 204, 204);
        public Color ScrollNormalFore = Color.FromArgb(102, 102, 102);
        public Color ScrollFocusedBack = Color.FromArgb(204, 204, 204);
        public Color ScrollFocusedFore = Color.FromArgb(37, 37, 38);
        public Color ScrollHoverBack = Color.FromArgb(204, 204, 204);
        public Color ScrollHoverFore = Color.FromArgb(37, 37, 38);
        public Color ScrollPressedBack = Color.FromArgb(204, 204, 204);
        public Color ScrollPressedFore = Color.FromArgb(37, 37, 38);
        public Color ScrollDisabledBack = Color.FromArgb(230, 230, 230);
        public Color ScrollDisabledFore = Color.FromArgb(179, 179, 179);

        public Color LabelNormalFore = Color.FromArgb(30, 30, 30);
        public Color LabelFocusedFore = Color.FromArgb(0, 0, 0);
        public Color LabelHoverFore = Color.FromArgb(0, 0, 0);
        public Color LabelPressedFore = Color.FromArgb(0, 0, 0);
        public Color LabelDisabledFore = Color.FromArgb(150, 150, 150);

        public Color TabNormalFore = Color.FromArgb(110, 110, 110);
        public Color TabHoverFore = Color.FromArgb(60, 60, 60);
        public Color TabActiveFore = Color.FromArgb(30, 30, 30);

        public Color ButtonNormalBack = Color.FromArgb(230, 230, 230);
        public Color ButtonNormalFore = Color.FromArgb(30, 30, 30);
        public Color ButtonNormalBorder = Color.FromArgb(190, 190, 190);
        public Color ButtonHoverBack = Color.FromArgb(210, 210, 210);
        public Color ButtonHoverFore = Color.FromArgb(30, 30, 30);
        public Color ButtonHoverBorder = Color.FromArgb(190, 190, 190);
        public Color ButtonFocusedBack = Color.FromArgb(210, 210, 210);
        public Color ButtonFocusedFore = Color.FromArgb(30, 30, 30);
        public Color ButtonFocusedBorder = Color.FromArgb(190, 190, 190);
        public Color ButtonPressedBack = Color.FromArgb(210, 210, 210);
        public Color ButtonPressedFore = Color.FromArgb(30, 30, 30);
        public Color ButtonPressedBorder = Color.FromArgb(190, 190, 190);
        public Color ButtonDisabledBack = Color.FromArgb(230, 230, 230);
        public Color ButtonDisabledFore = Color.FromArgb(100, 100, 100);
        public Color ButtonDisabledBorder = Color.FromArgb(190, 190, 190);
        public Color ButtonCheckedBack = Color.FromArgb(210, 210, 210);
        public Color ButtonCheckedFore = Color.FromArgb(30, 30, 30);
        public Color ButtonCheckedBorder = Color.FromArgb(190, 190, 190);

        public Color ButtonImageNormalBack = Color.FromArgb(190, 190, 190);
        public Color ButtonImageHoverBack = Color.FromArgb(190, 190, 190);
        public Color ButtonImagePressedBack = Color.FromArgb(190, 190, 190);
        public Color ButtonImageFocusedIndicator = Color.FromArgb(190, 190, 190);

        public Color MenuNormalBack = Color.FromArgb(230, 230, 230);
        public Color MenuNormalFore = Color.FromArgb(30, 30, 30);
        public Color MenuHoverBack = Color.FromArgb(206, 226, 252);
        public Color MenuHoverFore = Color.FromArgb(0, 0, 0);
        public Color MenuFocusedBack = Color.FromArgb(154, 194, 249);
        public Color MenuFocusedFore = Color.FromArgb(0, 0, 0);

        #endregion

        #region Get colors

        /// <summary>
        /// Gets the background image for the current theme
        /// </summary>
        public Image GetThemeImage() {
            var yamuiImage = (!string.IsNullOrEmpty(PageBackGroundImage) ? (Image) Resources.Resources.ResourceManager.GetObject(PageBackGroundImage) : null);

            // can't find the image locally (in Yamui), use the event to try and find one from the user program
            if (yamuiImage == null && OnImageNeeded != null) {
                yamuiImage = OnImageNeeded(PageBackGroundImage);
            }

            return yamuiImage;
        }

        /// <summary>
        /// This class is used for sliders as well as scrollbars
        /// </summary>
        public Color ScrollBarsBg(bool isFocused, bool isHovered, bool isPressed, bool enabled) {
            Color backColor;
            if (!enabled)
                backColor = ScrollDisabledBack;
            else if (isPressed)
                backColor = ScrollPressedBack;
            else if (isFocused)
                backColor = ScrollFocusedBack;
            else if (isHovered)
                backColor = ScrollHoverBack;
            else
                backColor = ScrollNormalBack;
            return backColor;
        }

        public Color ScrollBarsFg(bool isFocused, bool isHovered, bool isPressed, bool enabled) {
            Color foreColor;
            if (!enabled)
                foreColor = ScrollDisabledFore;
            else if (isPressed)
                foreColor = ScrollPressedFore;
            else if (isFocused)
                foreColor = ScrollFocusedFore;
            else if (isHovered)
                foreColor = ScrollHoverFore;
            else
                foreColor = ScrollNormalFore;
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
                foreColor = LabelDisabledFore;
            else if (isPressed)
                foreColor = LabelPressedFore;
            else if (isFocused)
                foreColor = LabelFocusedFore;
            else if (isHovered)
                foreColor = LabelHoverFore;
            else
                foreColor = LabelNormalFore;
            return foreColor;
        }

        public Color LabelsBg(Color controlBackColor, bool useCustomBackColor) {
            return !useCustomBackColor ? Color.Transparent : controlBackColor;
        }
        
        /// <summary>
        ///     This class is used for tab controls (back color is also used for tab pages)
        /// </summary>
        public Color TabsFg(bool isHovered, bool isSelected) {
            Color foreColor;
            if (isSelected)
                foreColor = TabActiveFore;
            else if (isHovered)
                foreColor = TabHoverFore;
            else
                foreColor = TabNormalFore;
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
        public Color ButtonBg(Color controlBackColor, bool useCustomBackColor, bool isFocused, bool isHovered, bool isPressed, bool enabled, bool isChecked = false) {
            Color backColor;
            if (useCustomBackColor)
                backColor = controlBackColor;
            else if (!enabled)
                backColor = ButtonDisabledBack;
            else if (isPressed)
                backColor = ButtonPressedBack;
            else if (isChecked)
                backColor = ButtonCheckedBack;
            else if (isFocused)
                backColor = ButtonFocusedBack;
            else if (isHovered)
                backColor = ButtonHoverBack;
            else
                backColor = ButtonNormalBack;
            return backColor;
        }

        public Color ButtonFg(Color controlForeColor, bool useCustomForeColor, bool isFocused, bool isHovered, bool isPressed, bool enabled, bool isChecked = false) {
            Color foreColor;
            if (useCustomForeColor)
                foreColor = controlForeColor;
            else if (!enabled)
                foreColor = ButtonDisabledFore;
            else if (isPressed)
                foreColor = ButtonPressedFore;
            else if (isChecked)
                foreColor = ButtonCheckedFore;
            else if (isFocused)
                foreColor = ButtonFocusedFore;
            else if (isHovered)
                foreColor = ButtonHoverFore;
            else
                foreColor = ButtonNormalFore;
            return foreColor;
        }

        public Color ButtonBorder(bool isFocused, bool isHovered, bool isPressed, bool enabled, bool isChecked = false) {
            Color borderColor;
            if (!enabled)
                borderColor = ButtonDisabledBorder;
            else if (isPressed)
                borderColor = ButtonPressedBorder;
            else if (isChecked)
                borderColor = ButtonCheckedBorder;
            else if (isFocused)
                borderColor = ButtonFocusedBorder;
            else if (isHovered)
                borderColor = ButtonHoverBorder;
            else
                borderColor = ButtonNormalBorder;
            return borderColor;
        }

        public Color ButtonImageBg(bool isHovered, bool isPressed) {
            Color backColor;
            if (isPressed)
                backColor = ButtonImagePressedBack;
            else if (isHovered)
                backColor = ButtonImageHoverBack;
            else
                backColor = ButtonImageNormalBack;
            return backColor;
        }

        public Color MenuBg(bool isFocused, bool isHovered) {
            Color backColor;
            if (isFocused)
                backColor = MenuFocusedBack;
            else if (isHovered)
                backColor = MenuHoverBack;
            else
                backColor = MenuNormalBack;
            return backColor;
        }

        public Color MenuFg(bool isFocused, bool isHovered) {
            Color foreColor;
            if (isFocused)
                foreColor = MenuFocusedFore;
            else if (isHovered)
                foreColor = MenuHoverFore;
            else
                foreColor = MenuNormalFore;
            return foreColor;
        }

        #endregion

    }
}
