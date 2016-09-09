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
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using YamuiFramework.Controls;
using YamuiFramework.Controls.YamuiList;
using YamuiFramework.Fonts;
using YamuiFramework.Themes;
using _3PA.Images;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;

namespace _3PA.MainFeatures.Appli.Pages.Options {
    internal partial class ProfilesPage : YamuiPage {

        #region fields

        #endregion

        #region constructor
        public ProfilesPage() {
            InitializeComponent();

            // scroll list
            var de = new List<ListItem>();
            for (int i = 0; i < 100; i++) {
                de.Add(new ListItem { DisplayText = "new" + i });
            }
            for (int i = 101; i < 1000; i++) {
                de.Add(new ListItem { DisplayText = "new" + i, IsDisabled = true });
            }
            for (int i = 1001; i < 100000; i++) {
                de.Add(new ListItem { DisplayText = "new" + i });
            }
            yamuiScrollList1.SetItems(de);

            // filtered list
            YamuiFilteredList1.SetItems(new List<ListItem> {
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

            flFilter1.TextChanged += YamuiFilteredList1.OnTextChangedEvent;
            flFilter1.KeyDown += (sender, args) => args.Handled = YamuiFilteredList1.OnKeyDown(args.KeyCode);

            // autocompletion list
            flFilter1.TextChanged += yamuiFilteredTypeList1.OnTextChangedEvent;
            flFilter1.KeyDown += (sender, args) => args.Handled = yamuiFilteredTypeList1.OnKeyDown(args.KeyCode);

            yamuiFilteredTypeList1.GetObjectTypeImage += type => {
                Image tryImg = (Image) ImageResources.ResourceManager.GetObject(((CompletionType) type).ToString());
                return tryImg ?? ImageResources.Error;
            };
            yamuiFilteredTypeList1.GetObjectSubText += item => { return ((FuckItem) item).SubString; };
            yamuiFilteredTypeList1.GetObjectTagImages += item => {
                var curItem = item as FuckItem;
                var outList = new List<Image>();
                foreach (var name in Enum.GetNames(typeof(ParseFlag))) {
                    ParseFlag flag = (ParseFlag)Enum.Parse(typeof(ParseFlag), name);
                    if (flag == 0 || !curItem.Flag.HasFlag(flag)) continue;

                    Image tryImg = (Image)ImageResources.ResourceManager.GetObject(name);
                    if (tryImg != null) {
                        outList.Add(tryImg);
                    }
                }
                return outList;
            };
            //yamuiFilteredTypeList1.GetMoreTypeImage = () => ImageResources.More;
            yamuiFilteredTypeList1.MoreTypesImage = ImageResources.More;

            yamuiButton1.ButtonPressed += (sender, args) => {
                yamuiFilteredTypeList1.SetItems(AutoComplete.CurrentItems.Select(item => new FuckItem {
                    DisplayText = item.DisplayText,
                    Type = item.Type,
                    SubString = item.SubString,
                    Flag = item.Flag
                }).Cast<ListItem>().ToList());
            };

            yamuiButton2.ButtonPressed += (sender, args) => {
            };

            yamuiButton3.ButtonPressed += (sender, args) => {
                YamuiFilteredList1.SetItems(new List<ListItem> {
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
                YamuiFilteredList1.SetItems(new List<ListItem> {
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


            btPlus.ButtonPressed += (sender, args) => {
                YamuiFilteredList1.Height += 10;
                yamuiScrollList1.Height += 10;
                yamuiFilteredTypeList1.Height += 10;
            };
            btMinus.ButtonPressed += (sender, args) => {
                YamuiFilteredList1.Height -= 10;
                yamuiScrollList1.Height -= 10;
                yamuiFilteredTypeList1.Height -= 10;
            };

            // dynamically reorder the controls for a correct tab order on notepad++
            SetTabOrder.RemoveAndAddForTabOrder(scrollPanel);
        }

        #endregion

    }

    internal class FuckItem : FilteredTypeItem {
        public CompletionType Type;
        public string SubString;
        public ParseFlag Flag;

        public override int ItemType {
            get { return (int) Type; }
        }
    }
}
