using System;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using YamuiFramework.Helper;
using _3PA.MainFeatures;

namespace _3PA.Lib {

    internal static class SetTabOrder {

        private static int _curTabIndex;

        private static StringBuilder _result;

        public static void CopyAddingOrderToClipBoard(Control control) {
            _curTabIndex = 0;
            _result = new StringBuilder();
            GetTabOrder(control);
            if (_result.Length > 0) {
                Clipboard.SetText(_result.ToString());
                UserCommunication.Notify("Tab order copied to clipboard");
            } else {
                UserCommunication.Notify("Nothing!");
            }
        }

        public static void GetTabOrder(Control control) {
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
                        _result.AppendLine("this." + control.Name + ".ContentPanel.Controls.Add(this." + c.Name + ");");
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
