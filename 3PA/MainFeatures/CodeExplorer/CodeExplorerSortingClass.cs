using System.Collections.Generic;
using YamuiFramework.Controls.YamuiList;
using _3PA.MainFeatures.AutoCompletionFeature;

#pragma warning disable 1570

namespace _3PA.MainFeatures.CodeExplorer {

    /// <summary>
    /// Class used in objectlist.Sort method
    /// </summary>
    internal class CodeExplorerSortingClass<T> : IComparer<T> where T : ListItem {

        #region private

        private static List<int> _explorerBranchTypePriority;

        private CodeExplorerForm.SortingType _sortingType;

        #endregion

        #region Fields

        public CodeExplorerForm.SortingType SortingType {
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
            _explorerBranchTypePriority = Config.GetPriorityList(typeof(CompletionType), "CodeExplorerPriorityList");
        }

        #endregion

        #region Compare

        /// <summary>
        /// to sort ascending : if x > y then return 1 if x < y then return -1; and
        /// x.compareTo(y) -> if x > y then return 1 if x < y then return -1;
        /// </summary>
        public int Compare(T x1, T y1) {

            //var x = x1 as CodeExplorerItem;
            //var y = y1 as CodeExplorerItem;
            //
            //
            //if (x == null || y == null)
                return 0;
            //
            //// compare first by BranchType
            //int compare = GetPriorityList[(int)x.Branch].CompareTo(GetPriorityList[(int)y.Branch]);
            //if (compare != 0) return compare;
            //
            //// compare by IconType
            //compare = x.IconType.CompareTo(y.IconType);
            //if (compare != 0) return compare;
            //
            //// sort by display text
            //if (_sortingType == CodeExplorerForm.SortingType.Alphabetical) {
            //    compare = String.Compare(x.DisplayText, y.DisplayText, StringComparison.CurrentCultureIgnoreCase);
            //    if (compare != 0) return compare;
            //}
            //
            //return x.GoToLine.CompareTo(y.GoToLine);

        }

        #endregion

        #region Sort type

        internal enum Type {
            NaturalOrder = 0,
            Alphabetical = 1,
            Unsorted = 2
        }

        #endregion

    }

}
