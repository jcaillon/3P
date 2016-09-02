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

using System;
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
                var de = new List<FilteredItem>();
                for (int i = 0; i < 100; i++) {
                    de.Add(new FilteredItem { DisplayText = "new" + i });
                }
                for (int i = 101; i < 1000; i++) {
                    de.Add(new FilteredItem { DisplayText = "new" + i, IsDisabled = true});
                }
                for (int i = 1001; i < 100000; i++) {
                    de.Add(new FilteredItem { DisplayText = "new" + i });
                }
                YamuiFilteredList1.SetItems(de);
                YamuiFilteredList1.GrabFocus();
            };

            yamuiButton2.ButtonPressed += (sender, args) => {
                var de = new List<FilteredItem>();
                YamuiFilteredList1.SetItems(de);
            };

            yamuiButton3.ButtonPressed += (sender, args) => {
                YamuiFilteredList1.SetItems(new List<FilteredItem> {
                    new FilteredItem { DisplayText = "fuck1", IsDisabled = true},
                    new FilteredItem { DisplayText = "fuck2", IsDisabled = true},
                    new FilteredItem { DisplayText = "fuck3", IsDisabled = true},
                    new FilteredItem { DisplayText = "fuck4", IsDisabled = true},
                    new FilteredItem { DisplayText = "fuck5", IsDisabled = true},
                    new FilteredItem { DisplayText = "fuck6", IsDisabled = true},
                    new FilteredItem { DisplayText = "fuck7", IsDisabled = false},
                    new FilteredItem { DisplayText = "fuck8", IsDisabled = true},
                    new FilteredItem { DisplayText = "fuck9", IsDisabled = true},
                    new FilteredItem { DisplayText = "fuck10", IsDisabled = false},
                });
            };

            yamuiButton4.ButtonPressed += (sender, args) => {
                YamuiFilteredList1.SetItems(new List<FilteredItem> {
                    new FilteredItem { DisplayText = "fuck1", IsDisabled = true},
                    new FilteredItem { DisplayText = "fuck2", IsDisabled = true},
                    new FilteredItem { DisplayText = "fuck3", IsDisabled = true},
                    new FilteredItem { DisplayText = "fuck4", IsDisabled = true},
                    new FilteredItem { DisplayText = "fuck5", IsDisabled = true},
                    new FilteredItem { DisplayText = "fuck6", IsDisabled = true},
                    new FilteredItem { DisplayText = "fuck7", IsDisabled = true},
                    new FilteredItem { DisplayText = "fuck8", IsDisabled = true},
                    new FilteredItem { DisplayText = "fuck9", IsDisabled = true},
                    new FilteredItem { DisplayText = "fuck10", IsDisabled = true},
                });
            };
            YamuiFilteredList1.EnterPressed += list => {
                UserCommunication.Notify(list.SelectedItemIndex + " = " + (list.SelectedItem ?? new FilteredItem { DisplayText = "null"}).DisplayText);
            };
            YamuiFilteredList1.TabPressed += list => {
                YamuiFilteredList1.ListPadding = new Padding(5, 10, 15, 25);
                list.SelectedItemIndex = 1115;
            };
            YamuiFilteredList1.RowClicked += (list, args) => {
                UserCommunication.Notify(list.SelectedItemIndex + " " + args.Button);
            };


            yamuiButton5.ButtonPressed += (sender, args) => {
                YamuiFilteredList1.Height += 10;
            };
            yamuiButton6.ButtonPressed += (sender, args) => {
                YamuiFilteredList1.Height -= 10;
            };

            YamuiFilteredList1.SetItems(new List<FilteredItem> {
                new FilteredItem { DisplayText = "CODAPP"},
                new FilteredItem { DisplayText = "NOMOBJ"},
                new FilteredItem { DisplayText = "CODORG"},
                new FilteredItem { DisplayText = "PROFIL"},
                new FilteredItem { DisplayText = "CODSOUFUCK"},
                new FilteredItem { DisplayText = "CODORGA"},
                new FilteredItem { DisplayText = "SRVEDIN"},
                new FilteredItem { DisplayText = "NOMIMPR"},
                new FilteredItem { DisplayText = "CHRONHA"},
                new FilteredItem { DisplayText = "FUCK0"},
            });

            YamuiFilteredList1.RowHeight = 22;

            yamuiTextBox1.TextChanged += YamuiFilteredList1.OnTextChangedEvent;

            // dynamically reorder the controls for a correct tab order on notepad++
            SetTabOrder.RemoveAndAddForTabOrder(scrollPanel);
        }

        #endregion

    }
}
