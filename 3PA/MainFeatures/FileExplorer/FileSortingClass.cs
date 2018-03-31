#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (FileSortingClass.cs) is part of 3P.
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
using Yamui.Framework.Controls.YamuiList;

#pragma warning disable 1570

namespace _3PA.MainFeatures.FileExplorer {
    /// <summary>
    /// Class used in objectlist.Sort method
    /// </summary>
    internal class FileSortingClass<T> : IComparer<T> where T : ListItem {
        #region Singleton

        private static FileSortingClass<T> _instance;

        public static FileSortingClass<T> Instance {
            get { return _instance ?? (_instance = new FileSortingClass<T>()); }
            set { _instance = value; }
        }

        #endregion

        #region Compare

        /// <summary>
        /// to sort ascending : if x > y then return 1 if x < y then return -1; and
        /// x.compareTo(y) -> if x > y then return 1 if x < y then return -1;
        /// </summary>
        public int Compare(T x1, T y1) {
            var x = x1 as FileListItem;
            var y = y1 as FileListItem;

            if (x == null || y == null)
                return 0;

            // first, the favourite
            int compare = x.Flags.HasFlag(FileFlag.Favourite).CompareTo(y.Flags.HasFlag(FileFlag.Favourite));
            if (compare != 0) return compare;

            // then the folders
            compare = y.Type.Equals(FileExt.Folder).CompareTo(x.Type.Equals(FileExt.Folder));
            if (compare != 0) return compare;

            // then the non read only
            compare = y.Flags.HasFlag(FileFlag.ReadOnly).CompareTo(x.Flags.HasFlag(FileFlag.ReadOnly));
            if (compare != 0) return compare;

            // sort by FileName
            return string.Compare(x.DisplayText, y.DisplayText, StringComparison.CurrentCultureIgnoreCase);
        }

        #endregion
    }
}