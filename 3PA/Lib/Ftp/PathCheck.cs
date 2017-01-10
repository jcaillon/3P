#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (PathCheck.cs) is part of 3P.
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
using System.IO;
using System.Text;

namespace _3PA.Lib.Ftp {
    /*
     *  Copyright 2008 Alessandro Pilotti
     *
     *  This program is free software; you can redistribute it and/or modify
     *  it under the terms of the GNU Lesser General Public License as published by
     *  the Free Software Foundation; either version 2.1 of the License, or
     *  (at your option) any later version.
     *
     *  This program is distributed in the hope that it will be useful,
     *  but WITHOUT ANY WARRANTY; without even the implied warranty of
     *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
     *  GNU Lesser General Public License for more details.
     *
     *  You should have received a copy of the GNU Lesser General Public License
     *  along with this program; if not, write to the Free Software
     *  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA 
     */

    public static class PathCheck {
        static char _replacementChar = '_';

        /// <summary>
        /// Replaces all invalid characters found in the provided name
        /// </summary>
        /// <param name="fileName">A file name without directory information</param>
        /// <returns></returns>
        public static string GetValidLocalFileName(string fileName) {
            return ReplaceAllChars(fileName, Path.GetInvalidFileNameChars(), _replacementChar);
        }

        private static string ReplaceAllChars(string str, char[] oldChars, char newChar) {
            StringBuilder sb = new StringBuilder(str);
            foreach (char c in oldChars)
                sb.Replace(c, newChar);
            return sb.ToString();
        }
    }
}
