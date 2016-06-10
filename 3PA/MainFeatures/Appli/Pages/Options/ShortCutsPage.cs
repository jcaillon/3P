#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (ExportPage.cs) is part of 3P.
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
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.HtmlRenderer.WinForms;
using _3PA.Html;
using _3PA.Images;
using _3PA.Interop;
using _3PA.Lib;

namespace _3PA.MainFeatures.Appli.Pages.Options {

    /// <summary>
    /// This page is built programatically
    /// </summary>
    internal partial class ShortCutsPage : YamuiPage {

        #region fields

        private string _currentItemId;

        private bool _waitingInput;

        #endregion

        #region constructor
        public ShortCutsPage() {
            InitializeComponent();
        }

        #endregion

        #region on show

        public override void OnShow() {

            foreach (Control control in dockedPanel.ContentPanel.Controls) {
                if (!control.Name.StartsWith("static"))
                    control.Dispose();
            }

            // build the interface
            var yPos = static_name.Location.Y + 35;
            foreach (var item in AppliMenu.CompleteMenuList) {

                // icon
                var imgButton = new YamuiImage {
                    BackGrndImage = item.ItemImage,
                    Size = new Size(20, 20),
                    Location = new Point(static_name.Location.X - 30, yPos),
                    Tag = item.ItemId,
                    TabStop = false
                };
                dockedPanel.ContentPanel.Controls.Add(imgButton);

                // name
                var label = new HtmlLabel {
                    AutoSizeHeightOnly = true,
                    BackColor = Color.Transparent,
                    Location = new Point(static_name.Location.X, yPos + 2),
                    Size = new Size(340, 10),
                    IsSelectionEnabled = false,
                    Text = item.ItemName
                };
                dockedPanel.ContentPanel.Controls.Add(label);

                // keys
                var button = new YamuiButton {
                    Location = new Point(static_keys.Location.X, yPos - 1),
                    Size = new Size(220, 23),
                    Tag = item.ItemId,
                    Text = item.ItemSpec ?? "",
                    Name = "bt" + item.ItemId,
                    TabStop = false
                };
                dockedPanel.ContentPanel.Controls.Add(button);
                button.Click += ButtonOnButtonPressed;
                tooltip.SetToolTip(button, "Click to modify this shortcut<br><i>You can press ESCAPE to cancel the changes</i>");

                // reset
                var undoButton = new YamuiImageButton {
                    BackGrndImage = ImageResources.UndoUserAction,
                    Size = new Size(20, 20),
                    Location = new Point(button.Location.X + button.Width + 10, yPos),
                    Tag = item.ItemId,
                    TabStop = false,
                };
                dockedPanel.ContentPanel.Controls.Add(undoButton);
                undoButton.ButtonPressed += UndoButtonOnButtonPressed;
                tooltip.SetToolTip(undoButton, "Click this button to reset the shortcut to its default value");

                yPos += label.Height + 15;
            }

            // Activate scrollbars
            dockedPanel.ContentPanel.Height = yPos + 20;
            Height = yPos;
        }

        #endregion


        #region events

        private void UndoButtonOnButtonPressed(object sender, EventArgs eventArgs) {
            _currentItemId = (string)((YamuiImageButton)sender).Tag;

            if (Config.Instance.ShortCuts.ContainsKey(_currentItemId))
                Config.Instance.ShortCuts.Remove(_currentItemId);

            // take into account the changes
            Plug.SetHooks();

            ((YamuiButton)dockedPanel.ContentPanel.Controls["bt" + _currentItemId]).Text = (Config.Instance.ShortCuts.ContainsKey(_currentItemId)) ? Config.Instance.ShortCuts[_currentItemId] : "";
        }

        private void ButtonOnButtonPressed(object sender, EventArgs eventArgs) {
            _currentItemId = (string)((YamuiButton)sender).Tag;

            if (_waitingInput)
                return;

            _waitingInput = true;
            KeyboardMonitor.Instance.KeyDownByPass += OnKeyDownByPass;

            var button = ((YamuiButton)dockedPanel.ContentPanel.Controls["bt" + _currentItemId]);

            button.Text = @"Enter a new shortcut (or press ESCAPE)";
            button.UseCustomBackColor = true;
            button.BackColor = ThemeManager.Current.AccentColor;
        }

        #endregion

        #region private methods

        private void OnKeyDownByPass(Keys key, KeyModifiers modifiers, ref bool handled) {
            bool stopListening = true;
            var button = (YamuiButton) dockedPanel.ContentPanel.Controls["bt" + _currentItemId];

            // the user presses escape to cancel the current shortcut modification
            if (key == Keys.Escape) {
                button.Text = Config.Instance.ShortCuts[_currentItemId];
            } else if (key != Keys.ControlKey && key != Keys.ShiftKey && key != Keys.Menu) {
                var newSpec = (new ShortcutKey(modifiers.IsCtrl, modifiers.IsAlt, modifiers.IsShift, key)).ToString();

                // don't override an existing shortcut
                if (Config.Instance.ShortCuts.ContainsValue(newSpec)) {
                    UserCommunication.Notify("Sorry, this shortcut is already used by the following function :<br>" + AppliMenu.CompleteMenuList.First(item => item.ItemSpec.Equals(newSpec)).ItemName, MessageImg.MsgInfo, "Modifying shortcut", "Existing key", 3);
                    return;
                }

                // change the shortcut in the settings
                if (Config.Instance.ShortCuts.ContainsKey(_currentItemId))
                    Config.Instance.ShortCuts[_currentItemId] = newSpec;
                else
                    Config.Instance.ShortCuts.Add(_currentItemId, newSpec);

                // take into account the changes
                Plug.SetHooks();
                button.Text = Config.Instance.ShortCuts[_currentItemId];
            } else {
                stopListening = false;
            }

            // stop listening to button pressed
            if (stopListening) {
                _waitingInput = false;
                KeyboardMonitor.Instance.KeyDownByPass -= OnKeyDownByPass;
                BlinkButton(button, ThemeManager.Current.ThemeAccentColor);
            }
        }

        /// <summary>
        /// Makes the given button blink
        /// </summary>
        private void BlinkButton(YamuiButton button, Color blinkColor) {
            button.UseCustomBackColor = true;
            Transition.run(button, "BackColor", ThemeManager.Current.ButtonNormalBack, blinkColor, new TransitionType_Flash(3, 300), (o, args) => { button.UseCustomBackColor = false; });
        }

        #endregion

    }


}
