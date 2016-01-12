using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using YamuiFramework.Controls;
using YamuiFramework.Helper;
using YamuiFramework.HtmlRenderer.WinForms;
using YamuiFramework.Themes;

namespace YamuiDemoApp.Pages {
    public partial class SettingAppearance : YamuiPage {

        private static YamuiColorRadioButton _checkButton;

        public SettingAppearance() {
            InitializeComponent();

            var colorList = new[] {
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

            // AccentColors picker
            int x = 0;
            int y = 0;
            foreach (var accentColor in colorList) {
                var newColorPicker = new YamuiColorRadioButton();
                PanelAccentColor.Controls.Add(newColorPicker);
                newColorPicker.CheckedChanged += NewColorPickerOnCheckedChanged;
                newColorPicker.BackColor = accentColor;
                newColorPicker.Bounds = new Rectangle(x, y, 50, 50);
                if (y + 2*newColorPicker.Height > PanelAccentColor.Height) {
                    x += newColorPicker.Width;
                    y = 0;
                } else
                    y += newColorPicker.Height;
                if (YamuiThemeManager.Current.AccentColor == accentColor) {
                    _checkButton = newColorPicker;
                    newColorPicker.Checked = true;
                }
            }

            // themes comob box
            comboTheme.DataSource = YamuiThemeManager.GetThemesList().Select(theme => theme.ThemeName).ToList();
            comboTheme.SelectedIndex = YamuiThemeManager.GetThemesList().IndexOf(YamuiThemeManager.Current);

            comboTheme.SelectedIndexChanged += ComboThemeOnSelectedIndexChanged;
        }

        /// <summary>
        /// Changing theme
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void ComboThemeOnSelectedIndexChanged(object sender, EventArgs eventArgs) {
            try {
                YamuiThemeManager.Current = YamuiThemeManager.GetThemesList()[comboTheme.SelectedIndex];
                YamuiThemeManager.Current.AccentColor = YamuiThemeManager.Current.ThemeAccentColor;
                _checkButton.Checked = false;
            } catch (Exception) {
                // ignored
            } finally {
                if (Program.MainForm != null) PlsRefresh();
            }
            
        }

        /// <summary>
        /// Changing accent Color
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void NewColorPickerOnCheckedChanged(object sender, EventArgs eventArgs) {
            YamuiColorRadioButton rb = sender as YamuiColorRadioButton;
            if (rb != null && rb.Checked) {
                YamuiThemeManager.Current.AccentColor = rb.BackColor;
                _checkButton = rb;
                if (Program.MainForm != null) PlsRefresh();
            }
        }

        private void PlsRefresh() {
            Application.DoEvents();
            Program.MainForm.Refresh();
        }
    }
}
