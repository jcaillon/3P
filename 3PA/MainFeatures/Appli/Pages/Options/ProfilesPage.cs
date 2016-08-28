#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProfilesPage.cs) is part of 3P.
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

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using YamuiFramework.Controls;
using YamuiFramework.Controls.YamuiList;
using YamuiFramework.Fonts;
using YamuiFramework.Themes;
using _3PA.Lib;

namespace _3PA.MainFeatures.Appli.Pages.Options {
    internal partial class ProfilesPage : YamuiPage {

        #region fields

        #endregion

        #region constructor
        public ProfilesPage() {
            InitializeComponent();

            
            yamuiButton1.ButtonPressed += (sender, args) => {
                var de = new List<ListItem>();
                for (int i = 0; i < 100; i++) {
                    de.Add(new ListItem { DisplayText = "new" + i });
                }
                for (int i = 101; i < 1000; i++) {
                    de.Add(new ListItem { DisplayText = "new" + i, IsDisabled = true});
                }
                for (int i = 1001; i < 100000; i++) {
                    de.Add(new ListItem { DisplayText = "new" + i });
                }
                yamuiScrollList1.SetItems(de);
                yamuiScrollList1.GrabFocus();
            };

            yamuiButton2.ButtonPressed += (sender, args) => {
                var de = new List<ListItem>();
                yamuiScrollList1.SetItems(de);
            };

            yamuiButton3.ButtonPressed += (sender, args) => {
                yamuiScrollList1.SetItems(new List<ListItem> {
                    new ListItem { DisplayText = "fuck1", IsDisabled = true},
                    new ListItem { DisplayText = "fuck2", IsDisabled = true},
                    new ListItem { DisplayText = "fuck3", IsDisabled = true},
                    new ListItem { DisplayText = "fuck4", IsDisabled = true},
                    new ListItem { DisplayText = "fuck5", IsDisabled = true},
                    new ListItem { DisplayText = "fuck6", IsDisabled = true},
                    new ListItem { DisplayText = "fuck7", IsDisabled = false},
                    new ListItem { DisplayText = "fuck8", IsDisabled = true},
                    new ListItem { DisplayText = "fuck9", IsDisabled = true},
                    new ListItem { DisplayText = "fuck10", IsDisabled = false},
                });
            };

            yamuiButton4.ButtonPressed += (sender, args) => {
                yamuiScrollList1.SetItems(new List<ListItem> {
                    new ListItem { DisplayText = "fuck1", IsDisabled = true},
                    new ListItem { DisplayText = "fuck2", IsDisabled = true},
                    new ListItem { DisplayText = "fuck3", IsDisabled = true},
                    new ListItem { DisplayText = "fuck4", IsDisabled = true},
                    new ListItem { DisplayText = "fuck5", IsDisabled = true},
                    new ListItem { DisplayText = "fuck6", IsDisabled = true},
                    new ListItem { DisplayText = "fuck7", IsDisabled = true},
                    new ListItem { DisplayText = "fuck8", IsDisabled = true},
                    new ListItem { DisplayText = "fuck9", IsDisabled = true},
                    new ListItem { DisplayText = "fuck10", IsDisabled = true},
                });
            };
            yamuiScrollList1.EnterPressed += list => {
                UserCommunication.Notify(list.SelectedItemIndex + " = " + (list.SelectedItem ?? new ListItem {DisplayText = "null"}).DisplayText);
            };
            yamuiScrollList1.TabPressed += list => {
                yamuiScrollList1.ListPadding = new Padding(5, 10, 15, 25);
                list.SelectedItemIndex = 1115;
            };
            yamuiScrollList1.RowClicked += (list, args) => {
                UserCommunication.Notify(list.SelectedItemIndex + " " + args.Button);
            };


            yamuiButton5.ButtonPressed += (sender, args) => {
                yamuiScrollList1.Height += 10;
            };
            yamuiButton6.ButtonPressed += (sender, args) => {
                yamuiScrollList1.Height -= 10;
            };

            yamuiScrollList1.SetItems(new List<ListItem> {
                new ListItem { DisplayText = "fuck1"},
                new ListItem { DisplayText = "fuck2"},
                new ListItem { DisplayText = "fuck3"},
                new ListItem { DisplayText = "fuck4"},
                new ListItem { DisplayText = "fuck5"},
                new ListItem { DisplayText = "fuck6"},
                new ListItem { DisplayText = "fuck7"},
                new ListItem { DisplayText = "fuck8"},
                new ListItem { DisplayText = "fuck9"},
                new ListItem { DisplayText = "fuck10"},
            });

            // dynamically reorder the controls for a correct tab order on notepad++
            SetTabOrder.RemoveAndAddForTabOrder(scrollPanel);
        }
        #endregion

    }
}
