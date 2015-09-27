using System.Collections.Generic;
using System.Drawing;
using System.IO;
using YamuiFramework.Resources;

namespace YamuiFramework.Themes {

    public static class ThemeManager {

        private static Color _accentColor = GetAccentColors[13];
        private static int _currentThemeId = 2;
        private static Theme _currentTheme;
        private static List<Theme> _listOfThemes = new List<Theme>();
        public static string ImageName;
        public static int CurrentThemeIndex;

        /// <summary>
        /// You can set this property to read the theme.xml file from a local path instead of
        /// the embedded ressource file
        /// </summary>
        public static string ThemeXmlPath;

        /// <summary>
        /// Default theme id to use when the ThemeManager is first called, 
        /// should be set before calling anyelse in the ThemeManager
        /// </summary>
        public static int CurrentThemeIdToUse {
            get { return _currentThemeId; }
            set { _currentThemeId = value; }
        }

        /// <summary>
        /// Return the current Theme object 
        /// </summary>
        public static Theme Current {
            set { 
                _currentTheme = value;
                UpdatedTheme();
            }
            get {
                if (_currentTheme != null)
                    return _currentTheme;
                // instanciation of current theme
                _currentTheme = GetThemesList().Find(theme => theme.UniqueId == CurrentThemeIdToUse) ?? GetThemesList()[0];
                UpdatedTheme();
                return _currentTheme;
            }
        }

        private static void UpdatedTheme() {
            if (!_currentTheme.UseCurrentAccentColor)
                _accentColor = _currentTheme.AccentColor;
            HtmlHandler.UpdateBaseCssData();
            ImageName = _currentTheme.PageBackGroundImage;
            CurrentThemeIndex = _listOfThemes.IndexOf(_currentTheme);
        }

        /// <summary>
        /// Set/get the accent color
        /// </summary>
        public static Color AccentColor {
            get { return _accentColor; }
            set { _accentColor = value; }
        }

        /// <summary>
        /// get the Image of the page background
        /// </summary>
        public static Image ThemePageImage {
            get {
                return !string.IsNullOrEmpty(ImageName) ? ImageGetter.GetInstance().Get(ImageName) : null;
            }
        }

        /// <summary>
        /// Set to false to deactivate tab animation
        /// </summary>
        public static bool TabAnimationAllowed { get; set; }

        /// <summary>
        /// Returns a list of accent colors to choose from
        /// </summary>
        public static Color[] GetAccentColors {
            get {
                return new[] {
                    Color.FromArgb(164, 196, 0),
                    Color.FromArgb(96, 169, 23),
                    Color.FromArgb(0, 138, 0),
                    Color.FromArgb(0, 171, 169),
                    Color.FromArgb(27, 161, 226),
                    Color.FromArgb(0, 80, 239),
                    Color.FromArgb(106, 0, 255),
                    Color.FromArgb(170, 0, 255),
                    Color.FromArgb(244, 114, 208),
                    Color.FromArgb(216, 0, 115),
                    Color.FromArgb(162, 0, 37),
                    Color.FromArgb(229, 20, 0),
                    Color.FromArgb(250, 104, 0),
                    Color.FromArgb(240, 163, 10),
                    Color.FromArgb(227, 200, 0),
                    Color.FromArgb(130, 90, 44),
                    Color.FromArgb(109, 135, 100),
                    Color.FromArgb(100, 118, 135),
                    Color.FromArgb(118, 96, 138),
                    Color.FromArgb(135, 121, 78)
                };
            }
        }

        /// <summary>
        /// Returns the list of all available themes
        /// </summary>
        /// <returns></returns>
        public static List<Theme> GetThemesList() {
            if (_listOfThemes.Count == 0) {
                if (string.IsNullOrEmpty(ThemeXmlPath) || !File.Exists(ThemeXmlPath)) {
                    Class2Xml<Theme>.LoadFromRaw(_listOfThemes, Resources.Resources.themesXml, true);
                    if (!string.IsNullOrEmpty(ThemeXmlPath))
                        Class2Xml<Theme>.SaveToFile(_listOfThemes, ThemeXmlPath, true);
                } else
                    Class2Xml<Theme>.LoadFromFile(_listOfThemes, ThemeXmlPath, true);
            }
            return _listOfThemes;
        }

        /// <summary>
        ///     This class is used for sliders as well as scrollbars
        /// </summary>
        public static class ScrollBarsColors {
            public static Color BackGround(bool isFocused, bool isHovered, bool isPressed, bool enabled) {
                Color backColor;
                if (!enabled)
                    backColor = Current.ScrollBarsColorsDisabledBackColor;
                else if (isPressed)
                    backColor = Current.ScrollBarsColorsNormalBackColor;
                else if (isHovered || isFocused)
                    backColor = Current.ScrollBarsColorsHoverBackColor;
                else
                    backColor = Current.ScrollBarsColorsNormalBackColor;

                return backColor;
            }

            public static Color ForeGround(bool isFocused, bool isHovered, bool isPressed, bool enabled) {
                Color foreColor;

                if (!enabled)
                    foreColor = Current.ScrollBarsColorsDisabledForeColor;
                else if (isPressed)
                    foreColor = AccentColor;
                else if (isHovered || isFocused)
                    foreColor = Current.ScrollBarsColorsHoverForeColor;
                else
                    foreColor = Current.ScrollBarsColorsNormalForeColor;

                return foreColor;
            }
        }

        /// <summary>
        ///     This class is used for labels as well as links
        /// </summary>
        public static class LabelsColors {
            public static Color ForeGround(Color controlForeColor, bool useCustomForeColor, bool isFocused, bool isHovered, bool isPressed, bool enabled) {
                Color foreColor;

                if (!enabled)
                    foreColor = Current.LabelsColorsDisabledForeColor;
                else if (isPressed)
                    foreColor = Current.LabelsColorsPressForeColor;
                else if (isHovered || isFocused)
                    foreColor = AccentColor;
                else
                    foreColor = useCustomForeColor ? controlForeColor : Current.LabelsColorsNormalForeColor;

                return foreColor;
            }

            public static Color BackGround(Color controlBackColor, bool useCustomBackColor) {
                return !useCustomBackColor ? Color.Transparent : controlBackColor;
            }
        }

        /// <summary>
        ///     This class is used for tab controls (back color is also used for tab pages)
        /// </summary>
        public static class TabsColors {
            public static Color ForeGround(bool isFocused, bool isHovered, bool isSelected) {
                Color foreColor;

                if (isFocused && isSelected)
                    foreColor = AccentColor;
                else if (isSelected)
                    foreColor = Current.TabsColorsPressForeColor;
                else if (isHovered)
                    foreColor = Current.TabsColorsHoverForeColor;
                else
                    foreColor = Current.TabsColorsNormalForeColor;

                return foreColor;
            }
        }

        /// <summary>
        ///     This class is used for :
        ///     - Buttons
        ///     - CheckBoxes
        ///     - ComboBoxes
        ///     - DatePicker
        ///     - RadioButtons
        /// </summary>
        public static class ButtonColors {
            public static Color BackGround(Color controlBackColor, bool useCustomBackColor, bool isFocused, bool isHovered, bool isPressed, bool enabled) {
                Color backColor;

                if (!enabled)
                    backColor = Current.ButtonColorsDisabledBackColor;
                else if (isPressed)
                    backColor = AccentColor;
                else if (isHovered)
                    backColor = Current.ButtonColorsHoverBackColor;
                else
                    backColor = useCustomBackColor ? controlBackColor : Current.ButtonColorsNormalBackColor;

                return backColor;
            }

            public static Color ForeGround(Color controlForeColor, bool useCustomForeColor, bool isFocused, bool isHovered, bool isPressed, bool enabled) {
                Color foreColor;

                if (!enabled)
                    foreColor = Current.ButtonColorsDisabledForeColor;
                else if (isPressed)
                    foreColor = Current.ButtonColorsPressForeColor;
                else if (isHovered)
                    foreColor = Current.ButtonColorsHoverForeColor;
                else
                    foreColor = useCustomForeColor ? controlForeColor : Current.ButtonColorsNormalForeColor;

                return foreColor;
            }

            public static Color BorderColor(bool isFocused, bool isHovered, bool isPressed, bool enabled) {
                Color borderColor;

                if (!enabled)
                    borderColor = Current.ButtonColorsDisabledBorderColor;
                else if (isPressed)
                    borderColor = AccentColor;
                else if (isFocused)
                    borderColor = AccentColor;
                else if (isHovered)
                    borderColor = Current.ButtonColorsHoverBorderColor;
                else
                    borderColor = Current.ButtonColorsNormalBorderColor;

                return borderColor;
            }
        }
    }
}