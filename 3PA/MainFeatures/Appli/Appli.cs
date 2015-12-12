#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (Appli.cs) is part of 3P.
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

namespace _3PA.MainFeatures.Appli {
    /// <summary>
    /// Handles the application main window
    /// </summary>
    class Appli {
        public static AppliForm Form;
        private static bool _hasBeenShownOnce;

        /// <summary>
        /// Call this method to toggle on/off the application
        /// </summary>
        public static void ToggleView() {
            // create the form
            if (!_hasBeenShownOnce) {
                _hasBeenShownOnce = true;
                Form.Show(Npp.Win32WindowNpp);
                Form.DoShow();
                return;
            }

            // toggle visibility
            if (Form.Visible && !Form.HasModalOpened)
                Form.Cloack();
            else
                Form.UnCloack();
        }

        /// <summary>
        /// Opens the application window and go to a specific page
        /// </summary>
        /// <param name="pageName"></param>
        public static void GoToPage(string pageName) {
            if (!_hasBeenShownOnce || !Form.Visible) {
                ToggleView();
            }
            Form.GoToPage(pageName);
        }

        /// <summary>
        /// Opens the application window and go to the about page
        /// </summary>
        public static void GoToAboutPage() {
            GoToPage("soft_info");
        }

        /// <summary>
        /// Initializes the main application, since other windows uses this Form reference, 
        /// it must be called pretty soon in the plugin initialization
        /// </summary>
        public static void Init() {
            if (Form != null) ForceClose();
            Form = new AppliForm {
                CurrentForegroundWindow = Npp.HandleNpp
            };
        }

        /// <summary>
        /// Forces the form to close, only when leaving npp
        /// </summary>
        public static void ForceClose() {
            try {
                Form.ForceClose();
                Form = null;
            } catch (Exception) {
                // ignored
            }
        }
    }
}
