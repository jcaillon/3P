#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
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
using YamuiFramework.Helper;
using _3PA.NppCore;
using _3PA.WindowsCore;

namespace _3PA.MainFeatures.Appli {
    /// <summary>
    /// Handles the application main window
    /// </summary>
    internal static class Appli {
        public static AppliForm Form {
            get { return _form; }
        }

        private static AppliForm _form;
        private static bool _hasBeenShownOnce;

        /// <summary>
        /// Call this method to toggle on/off the application
        /// </summary>
        public static void ToggleView() {
            try {
                if (_form == null) {
                    Init();
                }
                if (_form != null) {
                    // create the form
                    if (!_hasBeenShownOnce) {
                        _hasBeenShownOnce = true;
                        _form.Show(Npp.Win32Handle);
                    } else {
                        // toggle visibility
                        if (_form.Visible && !_form.HasModalOpened)
                            _form.Cloack();
                        else
                            _form.UnCloack();
                    }
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error while loading the main window");
            }
        }

        /// <summary>
        /// Opens the application window and go to a specific page
        /// </summary>
        /// <param name="pageName"></param>
        public static void GoToPage(PageNames pageName) {
            try {
                if (!_hasBeenShownOnce || !_form.Visible) {
                    ToggleView();
                }
                _form.ShowPage(pageName.ToString());
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in GoToPage");
            }
        }

        /// <summary>
        /// True if the form is focused 
        /// </summary>
        public static bool IsFocused() {
            return IsVisible && WinApi.GetForegroundWindow().Equals(_form.Handle);
        }

        /// <summary>
        /// Initializes the main application, since other windows uses this Form reference, 
        /// it must be called pretty soon in the plugin initialization
        /// </summary>
        public static void Init() {
            ForceClose();
            _form = new AppliForm {
                CurrentForegroundWindow = Npp.HandleNpp
            };
        }

        /// <summary>
        /// Forces the form to close, only when leaving npp
        /// </summary>
        public static void ForceClose() {
            try {
                if (_form != null)
                    _form.ForceClose();
                _form = null;
            } catch (Exception e) {
                ErrorHandler.LogError(e);
            }
        }

        public static void Refresh() {
            if (_form != null)
                _form.Refresh();
        }

        /// <summary>
        /// Returns true if the cursor is within the form window
        /// </summary>
        public static bool IsMouseIn() {
            return Win32Api.IsCursorIn(_form.Handle);
        }

        /// <summary>
        /// Is the form currently visible?
        /// </summary>
        public static bool IsVisible {
            get { return !(_form == null || !(bool) _form.SafeSyncInvoke(form => form.Visible)); }
        }
    }
}