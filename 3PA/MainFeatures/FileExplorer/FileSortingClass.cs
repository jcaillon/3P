using System;
using System.Collections.Generic;
using YamuiFramework.Controls.YamuiList;

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
            int compare = x.Flag.HasFlag(FileFlag.Favourite).CompareTo(y.Flag.HasFlag(FileFlag.Favourite));
            if (compare != 0) return compare;

            // then the folders
            compare = y.Type.Equals(FileType.Folder).CompareTo(x.Type.Equals(FileType.Folder));
            if (compare != 0) return compare;

            // then the non read only
            compare = y.Flag.HasFlag(FileFlag.ReadOnly).CompareTo(x.Flag.HasFlag(FileFlag.ReadOnly));
            if (compare != 0) return compare;

            // sort by FileName
            return string.Compare(x.DisplayText, y.DisplayText, StringComparison.CurrentCultureIgnoreCase);

    }

        #endregion

    }

}
