#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (HtmlHelpApi.cs) is part of 3P.
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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace _3PA.WindowsCore {
    public static class HtmlHelpInterop {
        [Flags]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum HTMLHelpCommand : uint {
            HH_DISPLAY_TOPIC = 0,
            HH_DISPLAY_TOC = 1,
            HH_DISPLAY_INDEX = 2,
            HH_DISPLAY_SEARCH = 3,
            HH_DISPLAY_TEXT_POPUP = 0x000E,
            HH_HELP_CONTEXT = 0x000F,
            HH_CLOSE_ALL = 0x0012
        }

        public const int HhDisplayIndex = 0x0002;

        // This overload is for passing a string as the dwData parameter (for example, for the HH_DISPLAY_INDEX command)
        [DllImport("hhctrl.ocx", CharSet = CharSet.Unicode, EntryPoint = "HtmlHelpW")]
        public static extern int HtmlHelp(int caller, string file, uint command, string str);

        public static int DisplayIndex(int caller, string file, string index) {
            return HtmlHelp(caller, file, (uint) HTMLHelpCommand.HH_DISPLAY_INDEX, index);
        }
    }
}