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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Forms;
using YamuiFramework.HtmlRenderer.WinForms;
using _3PA.Images;
using _3PA.Interop;
using _3PA.Lib;

namespace _3PA.MainFeatures.Appli.Pages.Options {

    /// <summary>
    /// This page is built programatically
    /// </summary>
    internal partial class ShortCutsPage : YamuiPage {

        #region fields

        private bool _isCheckingDistant;

        #endregion

        #region constructor
        public ShortCutsPage() {
            InitializeComponent();

            // build the interface
            var iNbLine = 0;
            var yPos = lbl_name.Location.Y + 35;
            foreach (var confLine in AppliMenu.ListOfItems) {

                // label
                var label = new HtmlLabel {
                    AutoSizeHeightOnly = true,
                    BackColor = Color.Transparent,
                    Location = new Point(lbl_name.Location.X, yPos + 2),
                    Size = new Size(400, 10),
                    IsSelectionEnabled = false,
                    Text = confLine.Item2
                };
                dockedPanel.ContentPanel.Controls.Add(label);

                // label
                label = new HtmlLabel {
                    AutoSizeHeightOnly = true,
                    BackColor = Color.Transparent,
                    Location = new Point(lbl_keys.Location.X, yPos + 2),
                    Size = new Size(130, 10),
                    IsSelectionEnabled = false,
                    Text = confLine.Item3 ?? ""
                };
                dockedPanel.ContentPanel.Controls.Add(label);

                // local open
                var strButton = new YamuiImageButton {
                    BackGrndImage = ImageResources.OpenInExplorer,
                    Size = new Size(20, 20),
                    Location = new Point(lbl_keys.Location.X + 150, yPos),
                    Tag = confLine,
                    TabStop = false,
                    Enabled = false,
                    Name = "bto_" + iNbLine
                };
                //strButton.ButtonPressed += OpenFileOnButtonPressed;
                dockedPanel.ContentPanel.Controls.Add(strButton);
                tooltip.SetToolTip(strButton, "Left click to <b>open</b> this file in notepad++<br>Right click to <b>open</b> this file / folder in the explorer");
                
                yPos += label.Height + 15;
                iNbLine++;
            }

            bt_set.ButtonPressed += BtSetOnButtonPressed;
        }

        private void BtSetOnButtonPressed(object sender, EventArgs eventArgs) {
            KeyboardMonitor.Instance.KeyDownByPass += OnKeyDownByPass;
        }

        private void OnKeyDownByPass(Keys key, KeyModifiers modifiers, ref bool handled) {
            bool stopListening = true;
            
            // the user presses escape to cancel the current shortcut modification
            if (key == Keys.Escape) {
                UserCommunication.Notify("Escape!");
            } else if (key != Keys.ControlKey && key != Keys.ShiftKey && key != Keys.Menu) {
                // register the new shortcut chosen by the user
                UserCommunication.Notify(key.ToString() + " " + modifiers.IsAlt + " " + modifiers.IsCtrl + " " + modifiers.IsShift);

                // change the shortcut in the settings
                Config.Instance.ShortCuts["Test"] = (new ShortcutKey(modifiers.IsCtrl, modifiers.IsAlt, modifiers.IsShift, key)).ToString();

                Config.Instance.ShortCuts.Remove("Test");

                // take into account the changes
                Plug.SetHooks();
            } else {
                stopListening = false;
            }

            // stop listening to button pressed
            if (stopListening) {
                KeyboardMonitor.Instance.KeyDownByPass -= OnKeyDownByPass;
            }
        }

        #endregion

        #region events


        #endregion

        #region private methods

        /// <summary>
        /// Makes the given textbox blink
        /// </summary>
        private void BlinkTextBox(YamuiTextBox textBox, Color blinkColor) {
            textBox.UseCustomBackColor = true;
            Transition.run(textBox, "CustomBackColor", ThemeManager.Current.ButtonNormalBack, blinkColor, new TransitionType_Flash(3, 300), (o, args) => { textBox.UseCustomBackColor = false; });
        }

        #endregion

    }


}
