#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (CompletionSortingClass.cs) is part of 3P.
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

namespace _3PA.MainFeatures.AutoCompletionFeature {
    /// <summary>
    /// Class used in objectlist.Sort method
    /// </summary>
    internal class CompletionSortingClass<T> : IComparer<T> where T : ListItem {
        #region private

        // holds the display order of the CompletionType
        private List<int> _completionTypePriority;

        #endregion

        #region Singleton

        private static CompletionSortingClass<T> _instance;

        public static CompletionSortingClass<T> Instance {
            get { return _instance ?? (_instance = new CompletionSortingClass<T>()); }
            set { _instance = value; }
        }

        #endregion

        #region Life

        private CompletionSortingClass() {
            _completionTypePriority = Config.GetPriorityList(typeof(CompletionType), "AutoCompletionItemPriorityList");
        }

        #endregion

        #region Compare

        /// <summary>
        /// to sort ascending : if x > y then return 1 if x < y then return -1; and
        /// x.compareTo(y) -> if x > y then return 1 if x < y then return -1;
        /// </summary>
        public int Compare(T x1, T y1) {
            var x = x1 as CompletionItem;
            var y = y1 as CompletionItem;

            if (x == null || y == null)
                return 0;

            // compare first by CompletionType
            int compare = _completionTypePriority[x.ItemType].CompareTo(_completionTypePriority[y.ItemType]);
            if (compare != 0) return compare;

            // then by ranking
            compare = y.Ranking.CompareTo(x.Ranking);
            if (compare != 0) return compare;

            // then sort by parsed items first
            if (x is TableCompletionItem) {
                compare = y.FromParser.CompareTo(x.FromParser);
                if (compare != 0) return compare;
            }

            // then sort by scope type (descending, smaller scope first)
            if (x.ParsedBaseItem != null && y.ParsedBaseItem != null) {
                compare = ((int) y.ParsedBaseItem.GetScopeType()).CompareTo((int) x.ParsedBaseItem.GetScopeType());
                if (compare != 0) return compare;
            }

            // if keyword (ascending)
            var xk = x as KeywordCompletionItem;
            var yk = y as KeywordCompletionItem;
            if (xk != null && yk != null) {
                compare = ((int)xk.KeywordType).CompareTo(((int) yk.KeywordType));
                if (compare != 0) return compare;
            }

            // sort by display text in last
            return string.Compare(x.DisplayText, y.DisplayText, StringComparison.CurrentCultureIgnoreCase);
        }

        #endregion
    }
}