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

            // AccentColors picker
            int x = 0;
            int y = 0;
            foreach (var accentColor in YamuiThemeManager.GetAccentColors) {
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
                if (YamuiThemeManager.AccentColor == accentColor) {
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
                if (!YamuiThemeManager.Current.UseCurrentAccentColor)
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
                YamuiThemeManager.AccentColor = rb.BackColor;
                _checkButton = rb;
                if (Program.MainForm != null) PlsRefresh();
            }
        }

        private void PlsRefresh() {
            try {
                var x = ControlHelper.GetAll(FindForm(), typeof (HtmlLabel));
                if (x != null)
                    foreach (var y in x) {
                        y.Text = y.Text;
                    }
                x = ControlHelper.GetAll(FindForm(), typeof(HtmlPanel));
                if (x != null)
                    foreach (var y in x) {
                        y.Text = y.Text;
                    }
            } catch (Exception) {
                throw;
            }
            Application.DoEvents();
            Program.MainForm.Invalidate();
            Application.DoEvents();
            Program.MainForm.Update();
            Application.DoEvents();
            Program.MainForm.Refresh();
            Application.DoEvents();
            Program.MainForm.Refresh();
        }
    }
}
