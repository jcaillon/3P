#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (ShortCutsPage.cs) is part of 3P.
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
using Yamui.Framework.Animations.Transitions;
using Yamui.Framework.Controls;
using Yamui.Framework.HtmlRenderer.WinForms;
using _3PA.Lib;
using _3PA.NppCore;
using _3PA.WindowsCore;
using _3PA._Resource;

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
            foreach (Control control in Controls) {
                if (!control.Name.StartsWith("static"))
                    control.Dispose();
            }

            // build the interface
            var yPos = static_name.Location.Y + 35;
            foreach (var item in AppliMenu.Instance.ShortcutableItemList.OrderBy(item => item.DisplayText)) {
                // icon
                var imgButton = new YamuiPictureBox {
                    BackGrndImage = item.ItemImage,
                    Size = new Size(20, 20),
                    Location = new Point(static_name.Location.X - 30, yPos),
                    Tag = item.ItemId,
                    TabStop = false
                };
                Controls.Add(imgButton);

                // name
                var label = new HtmlLabel {
                    AutoSizeHeightOnly = true,
                    BackColor = Color.Transparent,
                    Location = new Point(static_name.Location.X, yPos + 2),
                    Size = new Size(340, 10),
                    IsSelectionEnabled = false,
                    Text = item.DisplayText
                };
                Controls.Add(label);

                // keys
                var button = new YamuiButton {
                    Anchor = AnchorStyles.Right | AnchorStyles.Top,
                    Location = new Point(static_keys.Location.X + static_keys.Width - 220, yPos - 1),
                    Size = new Size(220, 24),
                    Tag = item.ItemId,
                    Text = item.ItemSpec ?? "",
                    Name = "bt" + item.ItemId,
                    TabStop = true,
                    BackGrndImage = item.ItemImage
                };
                Controls.Add(button);
                button.Click += ButtonOnButtonPressed;
                tooltip.SetToolTip(button, "<b>" + item.DisplayText + "</b><br><br>Click to modify this shortcut<br><i>You can press ESCAPE to cancel the changes</i>");

                // reset
                button = new YamuiButtonImage {
                    Anchor = AnchorStyles.Right | AnchorStyles.Top,
                    BackGrndImage = ImageResources.UndoUserAction,
                    Size = new Size(20, 20),
                    Location = new Point(button.Location.X + button.Width + 10, yPos),
                    Tag = item.ItemId,
                    TabStop = false
                };
                Controls.Add(button);
                button.ButtonPressed += UndoButtonOnButtonPressed;
                tooltip.SetToolTip(button, "Click this button to reset the shortcut to its default value");

                // delete
                button = new YamuiButtonImage {
                    Anchor = AnchorStyles.Right | AnchorStyles.Top,
                    BackGrndImage = ImageResources.Delete,
                    Size = new Size(20, 20),
                    Location = new Point(button.Location.X + button.Width, yPos),
                    Tag = item.ItemId,
                    TabStop = false
                };
                Controls.Add(button);
                button.ButtonPressed += ButtonDeleteOnButtonPressed;
                tooltip.SetToolTip(button, "Click this button to clear this shortcut");

                yPos += label.Height + 15;
            }
            
            // dynamically reorder the controls for a correct tab order on notepad++
            SetTabOrder.RemoveAndAddForTabOrder(this);
        }

        #endregion

        #region events

        private void UndoButtonOnButtonPressed(object sender, EventArgs eventArgs) {
            _currentItemId = (string) ((YamuiButtonImage) sender).Tag;

            if (Config.Instance.ShortCuts.ContainsKey(_currentItemId))
                Config.Instance.ShortCuts.Remove(_currentItemId);

            // take into account the changes
            NotificationsPublisher.SetHooks();

            ((YamuiButton) Controls["bt" + _currentItemId]).Text = (Config.Instance.ShortCuts.ContainsKey(_currentItemId)) ? Config.Instance.ShortCuts[_currentItemId] : "";
        }

        private void ButtonOnButtonPressed(object sender, EventArgs eventArgs) {
            _currentItemId = (string) ((YamuiButton) sender).Tag;

            if (_waitingInput)
                return;

            _waitingInput = true;
            KeyboardMonitor.Instance.KeyDownByPass += OnNewShortcutPressed;

            var button = ((YamuiButton) Controls["bt" + _currentItemId]);

            button.Text = @"Enter a new shortcut (or press ESCAPE)";
            button.UseCustomBackColor = true;
            button.BackColor = ThemeManager.Current.AccentColor;
        }

        private void ButtonDeleteOnButtonPressed(object sender, EventArgs eventArgs) {
            _currentItemId = (string) ((YamuiButtonImage) sender).Tag;
            if (Config.Instance.ShortCuts.ContainsKey(_currentItemId))
                Config.Instance.ShortCuts[_currentItemId] = "";
            else
                Config.Instance.ShortCuts.Add(_currentItemId, "");

            // take into account the changes
            NotificationsPublisher.SetHooks();

            ((YamuiButton) Controls["bt" + _currentItemId]).Text = "";
        }

        private bool OnNewShortcutPressed(KeyEventArgs e) {
            bool stopListening = true;
            var button = (YamuiButton) Controls["bt" + _currentItemId];

            // the user presses escape to cancel the current shortcut modification
            if (e.KeyCode == Keys.Escape) {
                button.Text = Config.Instance.ShortCuts[_currentItemId];
            } else if (e.KeyCode != Keys.ControlKey && e.KeyCode != Keys.ShiftKey && e.KeyCode != Keys.Menu) {
                var newSpec = (new ShortcutKey(e.Control, e.Alt, e.Shift, e.KeyCode)).ToString();

                // don't override an existing shortcut
                if (Config.Instance.ShortCuts.ContainsValue(newSpec)) {
                    UserCommunication.Notify("Sorry, this shortcut is already used by the following function :<br>" + AppliMenu.Instance.ShortcutableItemList.First(item => item.ItemSpec.Equals(newSpec)).DisplayText, MessageImg.MsgInfo, "Modifying shortcut", "Existing key", 3);
                    return true;
                }

                // change the shortcut in the settings
                if (Config.Instance.ShortCuts.ContainsKey(_currentItemId))
                    Config.Instance.ShortCuts[_currentItemId] = newSpec;
                else
                    Config.Instance.ShortCuts.Add(_currentItemId, newSpec);

                // take into account the changes
                NotificationsPublisher.SetHooks();
                button.Text = Config.Instance.ShortCuts[_currentItemId];
            } else {
                stopListening = false;
            }

            // stop listening to button pressed
            if (stopListening) {
                _waitingInput = false;
                KeyboardMonitor.Instance.KeyDownByPass -= OnNewShortcutPressed;
                BlinkButton(button, ThemeManager.Current.ThemeAccentColor);
            }

            return true;
        }

        #endregion

        #region private methods

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