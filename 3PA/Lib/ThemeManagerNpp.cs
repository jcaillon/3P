using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using YamuiFramework.Themes;
using _3PA.Data;

namespace _3PA.Lib {
    /// <summary>
    /// This class handles the theme colors for all the control drawn in the npp interface
    /// i.e. autocompletion and docked window
    /// </summary>
    class ThemeManagerNpp {

        private static string _filePath;
        private static string _location = Npp.GetConfigDir();
        private static string _fileName = "ThemeNppInterface.xml";
        private static Theme _currentTheme;
        private static List<Theme> _listOfThemes = new List<Theme>();
        public static int CurrentThemeIndex;

        /// <summary>
        /// Default theme id to use when the ThemeManager is first called, 
        /// should be set before calling anyelse in the ThemeManager
        /// </summary>
        public static int CurrentThemeIdToUse { get; set; }

        /// <summary>
        /// Return the current Theme object 
        /// </summary>
        public static Theme Current {
            set {
                _currentTheme = value;
                CurrentThemeIndex = _listOfThemes.IndexOf(_currentTheme);
            }
            get {
                if (_currentTheme != null)
                    return _currentTheme;
                // instanciation of current theme
                _currentTheme = GetThemesList().Find(theme => theme.UniqueId == CurrentThemeIdToUse) ?? GetThemesList()[0];
                CurrentThemeIndex = _listOfThemes.IndexOf(_currentTheme);
                return _currentTheme;
            }
        }

        /// <summary>
        /// Returns the list of all available themes
        /// </summary>
        /// <returns></returns>
        public static List<Theme> GetThemesList() {
            if (_listOfThemes.Count == 0) {
                _filePath = Path.Combine(_location, _fileName);
                if (!Directory.Exists(_location))
                    Directory.CreateDirectory(_location);
                if (!File.Exists(_filePath))
                    File.WriteAllBytes(_filePath, DataResources.ThemeNppInterface);
                if (File.Exists(_filePath)) {
                    try {
                        Object2Xml<Theme>.LoadFromFile(_listOfThemes, _filePath, true);
                    } catch (Exception e) {
                        Plug.ShowErrors(e, "Error when loading settings", _filePath);
                    }
                } else {
                    _listOfThemes.Add(new Theme());
                    Object2Xml<Theme>.SaveToFile(_listOfThemes, _filePath, true);
                }
            }
            return _listOfThemes;
        }

        public static class ButtonColors {
            public static Color BackGround(Color controlBackColor, bool useCustomBackColor, bool isFocused, bool isHovered, bool isPressed, bool enabled) {
                Color backColor;

                if (!enabled)
                    backColor = Current.ButtonColorsDisabledBackColor;
                else if (isPressed)
                    backColor = ThemeManager.AccentColor;
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
                    borderColor = ThemeManager.AccentColor;
                else if (isFocused)
                    borderColor = ThemeManager.AccentColor;
                else if (isHovered)
                    borderColor = Current.ButtonColorsHoverBorderColor;
                else
                    borderColor = Current.ButtonColorsNormalBorderColor;

                return borderColor;
            }
        }
    }

    public class Theme {

        /* public members will be exported to the xml configuration file (Class2xml) */
        /* This is the classic theme : */
        public string ThemeName = "Classic theme";
        public int UniqueId = 0;

        public Color FormColorBackColor = Color.FromArgb(230, 230, 230);
        public Color FormColorForeColor = Color.FromArgb(30, 30, 30);
        public Color LabelsColorsNormalForeColor = Color.FromArgb(30, 30, 30);
        public Color LabelsColorsPressForeColor = Color.FromArgb(0, 0, 0);
        public Color LabelsColorsDisabledForeColor = Color.FromArgb(150, 150, 150);
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

        // special for 3PA, autocompletion colors
        public Color AutoCompletionNormalBackColor = Color.FromArgb(250, 250, 250);
        public Color AutoCompletionNormalAlternateBackColor = Color.FromArgb(230, 230, 230);
        public Color AutoCompletionNormalForeColor = Color.FromArgb(30, 30, 30);
        public Color AutoCompletionNormalSubTypeForeColor = Color.FromArgb(100, 154, 209);
        public Color AutoCompletionHoverBackColor = Color.FromArgb(206, 226, 252);
        public Color AutoCompletionHoverForeColor = Color.FromArgb(0, 0, 0);
        public Color AutoCompletionFocusBackColor = Color.FromArgb(154, 194, 249);
        public Color AutoCompletionFocusForeColor = Color.FromArgb(0, 0, 0);
        public Color AutoCompletionHighlightBack = Color.FromArgb(254, 228, 101);
        public Color AutoCompletionHighlightBorder = Color.FromArgb(255, 171, 0);
    }
}
