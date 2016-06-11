#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
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
using System;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using YamuiFramework.Controls;
using YamuiFramework.Helper;
using _3PA.MainFeatures;

namespace _3PA.Lib {

    /// <summary>
    /// Use like this :
    /// (new SetTabOrder()).CopyAddingOrderToClipBoard(Utils.GetControlsOfType<YamuiScrollPage>(Appli.Form).FirstOrDefault());
    /// </summary>
    internal class SetTabOrder {

        private int _curTabIndex;

        private StringBuilder _result;

        private YamuiScrollPage _page;

        public void CopyAddingOrderToClipBoard(YamuiScrollPage scrollPage) {
            _page = scrollPage;
            _curTabIndex = 0;
            _result = new StringBuilder();
            GetTabOrder(scrollPage.ContentPanel);
            if (_result.Length > 0) {
                Clipboard.SetText(_result.ToString());
                UserCommunication.Notify("Tab order copied to clipboard");
            } else {
                UserCommunication.Notify("Nothing!");
            }
        }

        public void GetTabOrder(Control control) {
            // Tab order isn't important enough to ever cause a crash, so replace any exceptions
            // with assertions.
            try {
                _curTabIndex = 0;

                ArrayList controlArraySorted = new ArrayList();
                controlArraySorted.AddRange(control.Controls);
                controlArraySorted.Sort(new TabSchemeComparer(TabOrderManager.TabScheme.AcrossFirst));

                foreach (Control c in controlArraySorted) {
                    c.TabIndex = _curTabIndex++;
                    if (c.Controls.Count > 0) {
                        //GetTabOrder(c);
                    } else {
                        _result.AppendLine("this." + _page.Name + ".ContentPanel.Controls.Add(this." + c.Name + ");");
                    }
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "TabOrder");
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
