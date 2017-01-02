#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using YamuiFramework.Helper;

namespace YamuiFramework.Themes {

    /// <summary>
    /// Holds a theme configuration for the YamuiFramework
    /// </summary>
    public class YamuiTheme : GenericThemeHolder {

        #region Stored in the config file

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
        public Color ButtonWatermarkFore = Color.FromArgb(100, 100, 100);

        public Color ButtonImageNormalBack = Color.FromArgb(190, 190, 190);
        public Color ButtonImageHoverBack = Color.FromArgb(190, 190, 190);
        public Color ButtonImagePressedBack = Color.FromArgb(190, 190, 190);
        public Color ButtonImageFocusedIndicator = Color.FromArgb(190, 190, 190);

        public Color MenuNormalAltBack = Color.FromArgb(200, 200, 200);
        public Color MenuNormalBack = Color.FromArgb(230, 230, 230);
        public Color MenuNormalFore = Color.FromArgb(30, 30, 30);
        public Color MenuHoverBack = Color.FromArgb(206, 226, 252);
        public Color MenuHoverFore = Color.FromArgb(0, 0, 0);
        public Color MenuFocusedBack = Color.FromArgb(154, 194, 249);
        public Color MenuFocusedFore = Color.FromArgb(0, 0, 0);
        public Color MenuDisabledBack = Color.FromArgb(230, 230, 230);
        public Color MenuDisabledFore = Color.FromArgb(100, 100, 100);

        public Color AutoCompletionHighlightBack = Color.FromArgb(254, 228, 101);
        public Color AutoCompletionHighlightBorder = Color.FromArgb(255, 171, 0);

        #endregion

        #region Accent color

        private Color _accentColor;

        /// <summary>
        /// Accent color of the theme, can be set to Color.Empty in order to make it equal to the ThemeAccentColor
        /// </summary>
        public Color AccentColor {
            get { return _accentColor; }
            set {
                if (value == Color.Empty) {
                    var foundPairColor = SavedStringValues.FirstOrDefault(pair => pair.Key.Equals("ThemeAccentColor"));
                    if (!foundPairColor.Equals(new KeyValuePair<string, string>()))
                        _accentColor = ColorTranslator.FromHtml(foundPairColor.Value);
                    else
                        _accentColor = Color.Fuchsia;
                } else
                    _accentColor = value;
                SetStringValues("AccentColor", ColorTranslator.ToHtml(_accentColor));
            }
        }

        #endregion

        #region Get colors

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
            else if (isFocused)
                backColor = ButtonFocusedBack;
            else if (isHovered)
                backColor = ButtonHoverBack;
            else if (isChecked)
                backColor = ButtonCheckedBack;
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
            else if (isFocused)
                foreColor = ButtonFocusedFore;
            else if (isHovered)
                foreColor = ButtonHoverFore;
            else if (isChecked)
                foreColor = ButtonCheckedFore;
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
            else if (isFocused)
                borderColor = ButtonFocusedBorder;
            else if (isHovered)
                borderColor = ButtonHoverBorder;
            else if (isChecked)
                borderColor = ButtonCheckedBorder;
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

        public Color MenuBg(bool isFocused, bool isHovered, bool enabled) {
            Color backColor;
            if (!enabled)
                backColor = MenuDisabledBack;
            else if (isFocused)
                backColor = MenuFocusedBack;
            else if (isHovered)
                backColor = MenuHoverBack;
            else
                backColor = MenuNormalBack;
            return backColor;
        }

        public Color MenuFg(bool isFocused, bool isHovered, bool enabled) {
            Color foreColor;
            if (!enabled)
                foreColor = MenuDisabledFore;
            else if (isFocused)
                foreColor = MenuFocusedFore;
            else if (isHovered)
                foreColor = MenuHoverFore;
            else
                foreColor = MenuNormalFore;
            return foreColor;
        }

        #endregion

        #region base class overload

        /// <summary>
        /// Set the values of this instance, using a dictionnary of key -> values
        /// </summary>
        public new void SetColorValues(Type type) {
            // add the accent color if it doesn't exist! it means that we didn't set it, so it must be equal to ThemeAccentColor
            if (!SavedStringValues.ContainsKey("AccentColor"))
                SetStringValues("AccentColor", "@ThemeAccentColor");
            base.SetColorValues(type);
        }

        #endregion
        
    }
}
