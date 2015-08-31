using System.Drawing;

namespace YamuiFramework.Themes {
    public class Theme {
        
        /* public members will be exported to the xml configuration file (Class2xml) */
        /* This is the classic theme : */
        public string ThemeName = "Classique theme";

        public int UniqueId = 0;
        public int RankNeeded = 0; /* rank needed by the user to access this theme */
        public string PageBackGroundImage;

        public bool UseCurrentAccentColor = true;
        public Color AccentColor = ThemeManager.AccentColor; /* default accent color for this theme, read only when loading theme */

		public Color FormColorBackColor                = Color.FromArgb(230, 230, 230);
		public Color FormColorForeColor                = Color.FromArgb(30, 30, 30);
		public Color ScrollBarsColorsNormalBackColor   = Color.FromArgb(204, 204, 204);
		public Color ScrollBarsColorsNormalForeColor   = Color.FromArgb(102, 102, 102);
		public Color ScrollBarsColorsHoverBackColor    = Color.FromArgb(204, 204, 204);
		public Color ScrollBarsColorsHoverForeColor    = Color.FromArgb(37, 37, 38);
		public Color ScrollBarsColorsDisabledBackColor = Color.FromArgb(230, 230, 230);
		public Color ScrollBarsColorsDisabledForeColor = Color.FromArgb(179, 179, 179);
		public Color LabelsColorsNormalForeColor       = Color.FromArgb(30, 30, 30);
		public Color LabelsColorsPressForeColor        = Color.FromArgb(0, 0, 0);
		public Color LabelsColorsDisabledForeColor     = Color.FromArgb(150, 150, 150);
		public Color TabsColorsNormalBackColor         = Color.FromArgb(230, 230, 230);
		public Color TabsColorsNormalForeColor         = Color.FromArgb(110, 110, 110);
		public Color TabsColorsHoverForeColor          = Color.FromArgb(60, 60, 60);
		public Color TabsColorsPressForeColor          = Color.FromArgb(30, 30, 30);
		public Color ButtonColorsNormalBackColor       = Color.FromArgb(230, 230, 230);
		public Color ButtonColorsNormalForeColor       = Color.FromArgb(30, 30, 30);
		public Color ButtonColorsNormalBorderColor     = Color.FromArgb(190, 190, 190);
		public Color ButtonColorsHoverBackColor        = Color.FromArgb(210, 210, 210);
		public Color ButtonColorsHoverForeColor        = Color.FromArgb(30, 30, 30);
		public Color ButtonColorsHoverBorderColor      = Color.FromArgb(190, 190, 190);
		public Color ButtonColorsPressForeColor        = Color.FromArgb(30, 30, 30);
		public Color ButtonColorsDisabledBackColor     = Color.FromArgb(230, 230, 230);
		public Color ButtonColorsDisabledForeColor     = Color.FromArgb(100, 100, 100);
		public Color ButtonColorsDisabledBorderColor   = Color.FromArgb(190, 190, 190);
        /*
        public Color FormColorBackColor = Color.FromArgb(37, 37, 38);
        public Color FormColorForeColor = Color.FromArgb(210, 210, 210);
        public Color ScrollBarsColorsNormalBackColor = Color.FromArgb(51, 51, 51);
        public Color ScrollBarsColorsNormalForeColor = Color.FromArgb(153, 153, 153);
        public Color ScrollBarsColorsHoverBackColor = Color.FromArgb(51, 51, 51);
        public Color ScrollBarsColorsHoverForeColor = Color.FromArgb(204, 204, 204);
        public Color ScrollBarsColorsDisabledBackColor = Color.FromArgb(34, 34, 34);
        public Color ScrollBarsColorsDisabledForeColor = Color.FromArgb(85, 85, 85);
        public Color LabelsColorsNormalForeColor = Color.FromArgb(180, 180, 181);
        public Color LabelsColorsPressForeColor = Color.FromArgb(93, 93, 93);
        public Color LabelsColorsDisabledForeColor = Color.FromArgb(80, 80, 80);
        public Color TabsColorsNormalBackColor = Color.FromArgb(37, 37, 38);
        public Color TabsColorsNormalForeColor = Color.FromArgb(80, 80, 80);
        public Color TabsColorsHoverForeColor = Color.FromArgb(140, 140, 140);
        public Color TabsColorsPressForeColor = Color.FromArgb(193, 193, 194);
        public Color ButtonColorsNormalBackColor = Color.FromArgb(51, 51, 51);
        public Color ButtonColorsNormalForeColor = Color.FromArgb(209, 209, 209);
        public Color ButtonColorsNormalBorderColor = Color.FromArgb(51, 51, 51);
        public Color ButtonColorsHoverBackColor = Color.FromArgb(62, 62, 66);
        public Color ButtonColorsHoverForeColor = Color.FromArgb(209, 209, 209);
        public Color ButtonColorsHoverBorderColor = Color.FromArgb(62, 62, 66);
        public Color ButtonColorsPressForeColor = Color.FromArgb(209, 209, 209);
        public Color ButtonColorsDisabledBackColor = Color.FromArgb(51, 51, 51);
        public Color ButtonColorsDisabledForeColor = Color.FromArgb(84, 84, 84);
        public Color ButtonColorsDisabledBorderColor = Color.FromArgb(51, 51, 51);
         * */
    }

}
