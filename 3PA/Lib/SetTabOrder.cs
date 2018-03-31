#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (SetTabOrder.cs) is part of 3P.
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
using System.Collections;
using System.Windows.Forms;
using Yamui.Framework.Controls;
using Yamui.Framework.Helper;

namespace _3PA.Lib {

    /// <summary>
    /// This little class allows to remove and immedialty add all the direct children of a given scrollPanel.ContentPanel
    /// Allowing to set the correct tab order for them
    /// In Npp, the classic TabOrder is ignored for some reasons, what matters is the order in which the
    /// control were added to their parent, hence this class...
    /// </summary>
    internal static class SetTabOrder {
        public static void RemoveAndAddForTabOrder(YamuiPage scrollPanel) {
            ArrayList controlArraySorted = new ArrayList();
            controlArraySorted.AddRange(scrollPanel.Controls);
            controlArraySorted.Sort(new TabSchemeComparer(TabOrderManager.TabScheme.AcrossFirst));
            foreach (Control c in controlArraySorted) {
                if (c.Controls.Count == 0) {
                    scrollPanel.Controls.Remove(c);
                    scrollPanel.Controls.Add(c);
                }
            }
        }
    }

    internal class TabSchemeComparer : IComparer {
        private TabOrderManager.TabScheme _comparisonScheme;

        #region IComparer Members

        public int Compare(object x, object y) {
            Control control1 = x as Control;
            Control control2 = y as Control;

            if (control1 == null || control2 == null) {
                return 0;
            }

            if (_comparisonScheme == TabOrderManager.TabScheme.None) {
                // Nothing to do.
                return 0;
            }

            if (_comparisonScheme == TabOrderManager.TabScheme.AcrossFirst) {
                // The primary direction to sort is the y direction (using the Top property).
                // If two controls have the same y coordination, then we sort them by their x's.
                if (control1.Top < control2.Top) {
                    return -1;
                }
                if (control1.Top > control2.Top) {
                    return 1;
                }
                return (control1.Left.CompareTo(control2.Left));
            }
            // The primary direction to sort is the x direction (using the Left property).
            // If two controls have the same x coordination, then we sort them by their y's.
            if (control1.Left < control2.Left) {
                return -1;
            }
            if (control1.Left > control2.Left) {
                return 1;
            }
            return (control1.Top.CompareTo(control2.Top));
        }

        #endregion

        /// <summary>
        /// Create a tab scheme comparer that compares using the given scheme.
        /// </summary>
        /// <param name="scheme"></param>
        public TabSchemeComparer(TabOrderManager.TabScheme scheme) {
            _comparisonScheme = scheme;
        }
    }
}