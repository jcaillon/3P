using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace YamuiFramework.Themes {

    public class YamuiTheme {
        /// <summary>
        /// Theme's name
        /// </summary>
        public string ThemeName;

        public int UniqueId = 0;
        public int RankNeeded = 0;
        public string PageBackGroundImage = "";

        public bool UseCurrentAccentColor = true;
        public Color AccentColor = YamuiThemeManager.AccentColor;

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
    }
}
