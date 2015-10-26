using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using YamuiFramework.Controls;
using YamuiFramework.Helper;
using YamuiFramework.HtmlRenderer.WinForms;
using YamuiFramework.Themes;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;

namespace _3PA.MainFeatures.Appli.Pages {
    public partial class SettingAppearance : YamuiPage {

        private static YamuiColorRadioButton _checkButton;

        public SettingAppearance() {
            InitializeComponent();

            // AccentColors picker
            int x = 0;
            int y = 0;
            foreach (var accentColor in ThemeManager.GetAccentColors) {
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
                if (ThemeManager.AccentColor == accentColor) {
                    _checkButton = newColorPicker;
                    newColorPicker.Checked = true;
                }
            }

            // themes comob box
            comboTheme.DataSource = ThemeManager.GetThemesList().Select(theme => theme.ThemeName).ToList();
            comboTheme.SelectedIndex = ThemeManager.CurrentThemeIndex;

            comboTheme.SelectedIndexChanged += ComboThemeOnSelectedIndexChanged;
        }

        /// <summary>
        /// Changing theme
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void ComboThemeOnSelectedIndexChanged(object sender, EventArgs eventArgs) {
            try {
                ThemeManager.Current = ThemeManager.GetThemesList()[comboTheme.SelectedIndex];
                if (!ThemeManager.Current.UseCurrentAccentColor)
                    _checkButton.Checked = false;
            } catch (Exception) {
                // ignored
            } finally {
                ThemeManager.ImageName = ThemeManager.Current.PageBackGroundImage;
                Config.Instance.ThemeId = ThemeManager.Current.UniqueId;
                PlsRefresh();
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
                ThemeManager.AccentColor = rb.BackColor;
                _checkButton = rb;
                Config.Instance.AccentColor = ThemeManager.AccentColor;
                PlsRefresh();
            }
        }

        /// <summary>
        /// force all the html panel/label to refresh and try to refresh the main window
        /// </summary>
        private void PlsRefresh() {
            var thisForm = FindForm();
            if (thisForm != null) {
                // Refresh panels and labels (html)
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
                    //ignored
                }

                // force the autocomplete to redraw
                AutoComplete.ForceClose();

                // force the dockable to redraw
                DockableExplorer.DockableExplorer.Redraw();

                Application.DoEvents();
                thisForm.Invalidate();
                Application.DoEvents();
                thisForm.Update();
                Application.DoEvents();
                thisForm.Refresh();

                Appli.Form.UpdateTitle();
            }
        }
    }
}
