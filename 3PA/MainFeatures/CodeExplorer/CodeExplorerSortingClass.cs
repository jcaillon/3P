#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (CodeExplorerSortingClass.cs) is part of 3P.
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
using YamuiFramework.Controls.YamuiList;

#pragma warning disable 1570

namespace _3PA.MainFeatures.CodeExplorer {
    /// <summary>
    /// Class used in objectlist.Sort method
    /// </summary>
    internal class CodeExplorerSortingClass<T> : IComparer<T> where T : ListItem {
        #region private

        private Dictionary<int, int> _explorerBranchTypePriority;

        private SortingType _sortingType;

        #endregion

        #region Fields

        public SortingType Type {
            get { return _sortingType; }
            set { _sortingType = value; }
        }

        #endregion

        #region Singleton

        private static CodeExplorerSortingClass<T> _instance;

        public static CodeExplorerSortingClass<T> Instance {
            get { return _instance ?? (_instance = new CodeExplorerSortingClass<T>()); }
            set { _instance = value; }
        }

        #endregion

        #region Life

        private CodeExplorerSortingClass() {
            _explorerBranchTypePriority = Config.GetPriorityList<CodeExplorerIconType>("CodeExplorerItemPriorityList");
        }

        #endregion

        #region public

        public static CodeExplorerSortingClass<T> GetInstance(SortingType type) {
            var inst = Instance;
            inst.Type = type;
            return inst;
        }

        #endregion

        #region Compare

        /// <summary>
        /// to sort ascending : if x > y then return 1 if x < y then return -1; and
        /// x.compareTo(y) -> if x > y then return 1 if x < y then return -1;
        /// </summary>
        public int Compare(T x1, T y1) {
            var x = x1 as CodeItem;
            var y = y1 as CodeItem;

            if (x == null || y == null)
                return 0;

            // compare first by CompletionType
            var compare = _explorerBranchTypePriority[(int) x.Type].CompareTo(_explorerBranchTypePriority[(int) y.Type]);
            if (compare != 0) return compare;

            // sort by display text
            if (_sortingType == SortingType.Alphabetical) {
                compare = string.Compare(x.DisplayText, y.DisplayText, StringComparison.CurrentCultureIgnoreCase);
                if (compare != 0)
                    return compare;
            }

            // sort by line (code order)
            return x.GoToLine.CompareTo(y.GoToLine);
        }

        #endregion
    }

    #region Sort type

    internal enum SortingType {
        NaturalOrder = 0,
        Alphabetical = 1
    }

    #endregion
}