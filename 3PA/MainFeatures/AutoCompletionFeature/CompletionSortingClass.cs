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
            int compare = _completionTypePriority[(int) x.Type].CompareTo(_completionTypePriority[(int) y.Type]);
            if (compare != 0) return compare;

            // then by ranking
            compare = y.Ranking.CompareTo(x.Ranking);
            if (compare != 0) return compare;

            // then sort by parsed items first
            if (x.Type == CompletionType.Table) {
                compare = y.FromParser.CompareTo(x.FromParser);
                if (compare != 0) return compare;
            }

            // then sort by scope type (descending, smaller scope first)
            if (x.ParsedItem != null && y.ParsedItem != null && x.ParsedItem.Scope != null && y.ParsedItem.Scope != null) {
                compare = ((int) y.ParsedItem.Scope.ScopeType).CompareTo(((int) x.ParsedItem.Scope.ScopeType));
                if (compare != 0) return compare;
            }

            // if keyword (ascending)
            if (x.Type == CompletionType.Keyword || x.Type == CompletionType.KeywordObject) {
                compare = ((int) x.KeywordType).CompareTo(((int) y.KeywordType));
                if (compare != 0) return compare;
            }

            // sort by display text in last resort
            return string.Compare(x.DisplayText, y.DisplayText, StringComparison.CurrentCultureIgnoreCase);

        }

        #endregion

    }

}
