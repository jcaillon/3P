#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (SettingAppearance.cs) is part of 3P.
// 
// 3P is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// 3P is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with 3P. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System;
using System.Drawing;
using System.Linq;
using YamuiFramework.Controls;
using _3PA.Lib;
using _3PA.MainFeatures.Pro;
using _3PA.NppCore;

namespace _3PA.MainFeatures.Appli.Pages.Options {
    internal partial class SettingAppearance : YamuiPage {
        #region private fields

        private static YamuiColorRadioButton _checkButton;

        #endregion

        #region constructor

        public SettingAppearance() {
            InitializeComponent();

            // AccentColors picker
            int x = 0;
            int y = 0;
            foreach (var accentColor in ThemeManager.GetAccentColors) {
                var newColorPicker = new YamuiColorRadioButton();
                _simplePanelAccentColor.Controls.Add(newColorPicker);
                newColorPicker.CheckedChanged += NewColorPickerOnCheckedChanged;
                newColorPicker.BackColor = accentColor;
                newColorPicker.Location = new Point(x, y);
                newColorPicker.Size = new Size(50, 50);
                //newColorPicker.Bounds = new Rectangle(x, y, 50, 50);
                if (y + 2 * newColorPicker.Height > _simplePanelAccentColor.Height) {
                    x += newColorPicker.Width;
                    y = 0;
                } else
                    y += newColorPicker.Height;
                if (ThemeManager.Current.AccentColor == accentColor) {
                    _checkButton = newColorPicker;
                    newColorPicker.Checked = true;
                }
                toolTip.SetToolTip(newColorPicker, "Click me to set a new accent color for the current theme");
            }

            // toggle
            tg_colorOn.ButtonPressed += TgOnCheckedChanged;
            tg_colorOn.Checked = Config.Instance.UseSyntaxHighlightTheme;
            UpdateToggle();

            // tooltips
            toolTip.SetToolTip(cbApplication, "Choose the theme you wish to use for the software");
            toolTip.SetToolTip(cbSyntax, "Choose the theme you wish to use for the syntax highlighting");
            toolTip.SetToolTip(tg_colorOn, "Toggle this option OFF if you are using your own User Defined Language<br><br>By default, 3P created a new UDL called 'OpenEdgeABL' and applies the selected theme below<br>each time the user switches the current document<br>By toggling this OFF, you will prevent this behavior and you can define your own UDL<br><br><i>If you toggle this OFF, select the UDL to use from the Language menu before you can see any changes</i>");

            linkurl.Text = @"<img src='Help'><a href='" + Config.UrlHelpCustomThemes + @"'>How to customize the look of 3P?</a>";

            // dynamically reorder the controls for a correct tab order on notepad++
            SetTabOrder.RemoveAndAddForTabOrder(scrollPanel);
        }

        #endregion

        public override void OnShow() {
            // themes combo box
            cbApplication.SelectedIndexChanged -= CbApplicationOnSelectedIndexChanged;
            cbApplication.DataSource = ThemeManager.GetThemesList.Select(theme => theme.ThemeName).ToList();
            cbApplication.SelectedIndex = Config.Instance.ThemeId;
            cbApplication.SelectedIndexChanged += CbApplicationOnSelectedIndexChanged;

            // syntax combo
            cbSyntax.SelectedIndexChanged -= CbSyntaxSelectedIndexChanged;
            cbSyntax.DataSource = Style.GetThemesList.Select(theme => theme.ThemeName).ToList();
            cbSyntax.SelectedIndex = Config.Instance.SyntaxHighlightThemeId;
            cbSyntax.SelectedIndexChanged += CbSyntaxSelectedIndexChanged;
        }

        private void TgOnCheckedChanged(object sender, EventArgs eventArgs) {
            Config.Instance.UseSyntaxHighlightTheme = tg_colorOn.Checked;
            UpdateToggle();
        }

        private void UpdateToggle() {
            if (tg_colorOn.Checked) {
                tg_colorOn.Text = @"Use the themes provided by 3P, select one below : ";
                cbSyntax.Show();
            } else {
                tg_colorOn.Text = @"Use a custom User Defined Language";
                cbSyntax.Hide();
            }
        }

        /// <summary>
        /// Changing theme
        /// </summary>
        private void CbApplicationOnSelectedIndexChanged(object sender, EventArgs eventArgs) {
            var theme = ThemeManager.GetThemesList[cbApplication.SelectedIndex];
            theme.AccentColor = Color.Empty;
            Config.Instance.ThemeId = cbApplication.SelectedIndex;
            if (_checkButton != null)
                _checkButton.Checked = false;
            ThemeManager.RefreshApplicationWithTheme(theme);
            cbApplication.Refresh();
        }

        /// <summary>
        /// Changing accent Color
        /// </summary>
        private void NewColorPickerOnCheckedChanged(object sender, EventArgs eventArgs) {
            YamuiColorRadioButton rb = sender as YamuiColorRadioButton;
            if (rb != null && rb.Checked) {
                ThemeManager.Current.AccentColor = rb.BackColor;
                ThemeManager.RefreshApplicationWithTheme(ThemeManager.Current);
                _checkButton = rb;
            }
        }

        /// <summary>
        /// Changing syntax theme
        /// </summary>
        private void CbSyntaxSelectedIndexChanged(object sender, EventArgs eventArgs) {
            Style.Current = Style.GetThemesList[cbSyntax.SelectedIndex];
            Config.Instance.SyntaxHighlightThemeId = cbSyntax.SelectedIndex;
            if (Npp.CurrentFile.IsProgress) {
                Style.SetSyntaxStyles();
                Plug.ApplyOptionsForScintilla();
                OpenedFilesInfo.UpdateFileStatus();
            }
        }
    }
}