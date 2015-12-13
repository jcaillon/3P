#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (FileExplorerPage.cs) is part of 3P.
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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using YamuiFramework.Controls;
using YamuiFramework.Fonts;
using YamuiFramework.Themes;
using _3PA.Images;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.CodeExplorer;
using _3PA.MainFeatures.Parser;
using _3PA.MainFeatures.ProgressExecution;

namespace _3PA.MainFeatures.FileExplorer {
    public partial class FileExplorerPage : YamuiPage {

        #region Fields
        private const string StrEmptyList = "No files found!";
        private const string StrItems = " items";

        /// <summary>
        /// The filter to apply to the autocompletion form
        /// </summary>
        public string FilterByText {
            get { return _filterByText; }
            set { _filterByText = value.ToLower(); ApplyFilter(); }
        }
        private static string _filterByText = "";

        public bool UseAlternateBackColor {
            set { ovl.UseAlternatingBackColors = value; }
        }

        /// <summary>
        ///  gets or sets the total items currently displayed in the form
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Returns the value of the directory type checkbox
        /// </summary>
        public CheckState CheckBoxDirState {
            get { return cbDirectory.CheckState; }
        }


        // the private fields below are used for the filter function
        private static Dictionary<FileType, SelectorButton<FileType>> _displayedTypes;
        private static bool _useTypeFiltering;
        private static string _filterString = "";
        private static string _currentOwnerName = "";
        private static int _currentLineNumber;

        private int _currentType;

        // check the npp window rect, if it has changed from a previous state, close this form (poll every 500ms)
        private int _normalWidth;

        // remember the list that was passed to the autocomplete form when we set the items, we need this
        // because we reorder the list each time the user filters stuff, but we need the original order
        private List<FileObject> _initialObjectsList;

        #endregion

        #region Constructor

        public FileExplorerPage() {
            InitializeComponent();

            for (int i = 0; i < 20; i++) {
                var derp = new YamuiImageButton {
                    Size = new Size(20,20),
                    BackGrndImage = ImageResources.External,
                    Margin = new Padding(0)
                };
                flowLayoutPanel.Controls.Add(derp);
                if (i==2) flowLayoutPanel.SetFlowBreak(derp, true);
            }

            #region object view list

            // Image getter
            FileName.ImageGetter += ImageGetter;

            // Style the control
            StyleOvlTree();

            //buttonCleanText.BackGrndImage = ImageResources.eraser;
            //buttonRefresh.BackGrndImage = ImageResources.refresh;

            // Register to events
            //ovlCol.Click += OvlTreeOnClick;

            // decorate rows
            ovl.UseCellFormatEvents = true;
            ovl.FormatCell += ovlOnFormatCell;

            // tooltips
            //toolTipHtml.SetToolTip(buttonExpandRetract, "Toggle <b>Expand/Collapse</b>");

            // problems with the width of the column, set here
            FileName.Width = ovl.Width - 17;
            ovl.SizeChanged += (sender, args) => {
                FileName.Width = ovl.Width - 17;
                ovl.Invalidate();
            };

            #endregion

            btErase.BackGrndImage = ImageResources.eraser;
            btRefresh.BackGrndImage = ImageResources.refresh;

            cbDirectory.ThreeState = true;
        }



        #endregion

        #region cell formatting and style ovl
        /// <summary>
        /// Return the image that needs to be display on the left of an item
        /// representing its type
        /// </summary>
        /// <param name="typeStr"></param>
        /// <returns></returns>
        private static Image GetImageFromStr(string typeStr) {
            Image tryImg = (Image)ImageResources.ResourceManager.GetObject(typeStr);
            return tryImg ?? ImageResources.Error;
        }

        /// <summary>
        /// Image getter for object rows
        /// </summary>
        /// <param name="rowObject"></param>
        /// <returns></returns>
        private static object ImageGetter(object rowObject) {
            var obj = (FileObject) rowObject;
            if (obj == null) return ImageResources.Error;
            return GetImageFromStr(obj.Type.ToString());
        }

        /// <summary>
        /// Event on format cell
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void ovlOnFormatCell(object sender, FormatCellEventArgs args) {
            FileObject obj = (FileObject)args.Model;

            // currently selected block
            //if (!obj.IsNotBlock && obj.FileName.EqualsCi(ParserHandler.GetCarretLineOwnerName(Npp.Line.CurrentLine))) {
            //    RowBorderDecoration rbd = new RowBorderDecoration {
            //        FillBrush = new SolidBrush(Color.FromArgb(50, ThemeManager.Current.AutoCompletionFocusBackColor)),
            //        BorderPen = new Pen(Color.FromArgb(128, ThemeManager.Current.AutoCompletionFocusForeColor), 1),
            //        BoundsPadding = new Size(-2, 0),
            //        CornerRounding = 6.0f
            //    };
            //    args.SubItem.Decoration = rbd;
            //}

            // display the flags
            int offset = -5;
            foreach (var name in Enum.GetNames(typeof(FileFlag))) {
                FileFlag flag = (FileFlag)Enum.Parse(typeof(FileFlag), name);
                if (flag == 0) continue;
                if (!obj.Flags.HasFlag(flag)) continue;
                Image tryImg = (Image)ImageResources.ResourceManager.GetObject(name);
                if (tryImg == null) continue;
                ImageDecoration decoration = new ImageDecoration(tryImg, 100, ContentAlignment.MiddleRight) {
                    Offset = new Size(offset, 0)
                };
                if (args.SubItem.Decoration == null)
                    args.SubItem.Decoration = decoration;
                else
                    args.SubItem.Decorations.Add(decoration);
                offset -= 20;
            }

            // display the sub string
            if (offset < -5) offset -= 5;
            if (!string.IsNullOrEmpty(obj.SubString)) {
                TextDecoration decoration = new TextDecoration(obj.SubString, 100) {
                    Alignment = ContentAlignment.MiddleRight,
                    Offset = new Size(offset, 0),
                    Font = FontManager.GetFont(FontStyle.Bold, 11),
                    TextColor = ThemeManager.Current.AutoCompletionNormalSubTypeForeColor,
                    CornerRounding = 1f,
                    Rotation = 0,
                    BorderWidth = 1,
                    BorderColor = ThemeManager.Current.AutoCompletionNormalSubTypeForeColor
                };
                args.SubItem.Decorations.Add(decoration);
            }
        }

        /// <summary>
        /// Apply thememanager theme to the treeview
        /// </summary>
        public void StyleOvlTree() {
            // Style the control
            ovl.OwnerDraw = true;
            ovl.Font = FontManager.GetLabelFont(LabelFunction.AutoCompletion);
            ovl.BackColor = ThemeManager.Current.AutoCompletionNormalBackColor;
            ovl.AlternateRowBackColor = ThemeManager.Current.AutoCompletionNormalAlternateBackColor;
            ovl.ForeColor = ThemeManager.Current.AutoCompletionNormalForeColor;
            ovl.HighlightBackgroundColor = ThemeManager.Current.AutoCompletionFocusBackColor;
            ovl.HighlightForegroundColor = ThemeManager.Current.AutoCompletionFocusForeColor;
            ovl.UnfocusedHighlightBackgroundColor = ovl.HighlightBackgroundColor;
            ovl.UnfocusedHighlightForegroundColor = ovl.HighlightForegroundColor;

            // Decorate and configure hot item
            ovl.UseHotItem = true;
            ovl.HotItemStyle = new HotItemStyle {
                BackColor = ThemeManager.Current.AutoCompletionHoverBackColor,
                ForeColor = ThemeManager.Current.AutoCompletionHoverForeColor
            };

            // overlay of empty list :
            ovl.EmptyListMsg = StrEmptyList;
            TextOverlay textOverlay = ovl.EmptyListMsgOverlay as TextOverlay;
            if (textOverlay != null) {
                textOverlay.TextColor = ThemeManager.Current.AutoCompletionNormalForeColor;
                textOverlay.BackColor = ThemeManager.Current.AutoCompletionNormalAlternateBackColor;
                textOverlay.BorderColor = ThemeManager.Current.AutoCompletionNormalForeColor;
                textOverlay.BorderWidth = 4.0f;
                textOverlay.Font = FontManager.GetFont(FontStyle.Bold, 30f);
                textOverlay.Rotation = -5;
            }
        }
        #endregion

        /// <summary>
        /// Call this method to completly refresh the object view list, 
        /// </summary>
        public void RefreshOvl() {
            // get the list of FileObjects
            _initialObjectsList = new List<FileObject>();
            switch (CheckBoxDirState) {
                case CheckState.Checked:
                    _initialObjectsList = FileExplorer.ListFileOjectsInDirectory(ProgressEnv.Current.BaseLocalPath);
                    break;
                case CheckState.Unchecked:
                    _initialObjectsList = FileExplorer.ListFileOjectsInDirectory(ProgressEnv.Current.BaseCompilationPath);
                    break;
                default:
                    foreach (var dir in ProgressEnv.Current.GetProPathFileList) {
                        _initialObjectsList.AddRange(FileExplorer.ListFileOjectsInDirectory(dir, false));
                    }
                    break;
            }

        }

        /// <summary>
        /// set the items of the object view list, correct the width and height of the form and the list
        /// create a button for each type of completion present in the list of items
        /// </summary>
        /// <param name="objectsList"></param>
        /// <param name="resetSelectorButtons"></param>
        public void SetItems(List<FileObject> objectsList, bool resetSelectorButtons = true) {
            objectsList.Sort(new FilesSortingClass());
            _initialObjectsList = objectsList;

            // set the default height / width
            ovl.Height = 21 * Config.Instance.AutoCompleteShowListOfXSuggestions;
            Height = ovl.Height + 32;
            //Width = 280;

            if (resetSelectorButtons) {
                // delete any existing buttons
                if (_displayedTypes != null) {
                    foreach (var selectorButton in _displayedTypes) {
                        selectorButton.Value.ButtonPressed -= HandleTypeClick;
                        if (Controls.Contains(selectorButton.Value))
                            Controls.Remove(selectorButton.Value);
                        selectorButton.Value.Dispose();
                    }
                }

                // get distinct types, create a button for each
                int xPos = 65;
                _displayedTypes = new Dictionary<FileType, SelectorButton<FileType>>();
                foreach (var type in objectsList.Select(x => x.Type).Distinct()) {
                    var but = new SelectorButton<FileType> {
                        BackGrndImage = GetImageFromStr(type.ToString()),
                        Activated = true,
                        Size = new Size(24, 24),
                        TabStop = false,
                        Location = new Point(xPos, Height - 28),
                        Type = type,
                        AcceptsRightClick = true
                    };
                    but.ButtonPressed += HandleTypeClick;
                    toolTipHtml.SetToolTip(but, "<b>" + type + "</b>:<br><br><b>Left click</b> to toggle on/off this filter<br><b>Right click</b> to filter for this type only");
                    _displayedTypes.Add(type, but);
                    Controls.Add(but);
                    xPos += but.Width;
                }
            }

            // label for the number of items
            TotalItems = objectsList.Count;
            nbitems.Text = TotalItems + StrItems;

            ovl.SetObjects(objectsList);
        }

        /// <summary>
        /// Call this method before showing the list when you don't use SetItems to sort the
        /// items (it is already called by SetItems())
        /// </summary>
        public void SortItems() {
            _initialObjectsList.Sort(new FilesSortingClass());
            ovl.SetObjects(_initialObjectsList);
        }

        /// <summary>
        /// use this to programmatically uncheck any type that is not in the given list
        /// </summary>
        /// <param name="allowedType"></param>
        public void SetActiveType(List<FileType> allowedType) {
            if (_displayedTypes == null) return;
            if (allowedType == null) allowedType = new List<FileType>();
            foreach (var selectorButton in _displayedTypes) {
                selectorButton.Value.Activated = allowedType.IndexOf(selectorButton.Value.Type) >= 0;
                selectorButton.Value.Invalidate();
            }
        }

        /// <summary>
        /// use this to programmatically check any type that is not in the given list
        /// </summary>
        /// <param name="allowedType"></param>
        public void SetUnActiveType(List<FileType> allowedType) {
            if (_displayedTypes == null) return;
            if (allowedType == null) allowedType = new List<FileType>();
            foreach (var selectorButton in _displayedTypes) {
                selectorButton.Value.Activated = allowedType.IndexOf(selectorButton.Value.Type) < 0;
                selectorButton.Value.Invalidate();
            }
        }

        /// <summary>
        /// reset all the button Types to activated
        /// </summary>
        public void ResetActiveType() {
            if (_displayedTypes == null) return;
            foreach (var selectorButton in _displayedTypes) {
                selectorButton.Value.Activated = true;
                selectorButton.Value.Invalidate();
            }
        }

        /// <summary>
        /// allows to programmatically select the first item of the list
        /// </summary>
        public void SelectFirstItem() {
            if (TotalItems > 0) ovl.SelectedIndex = 0;
        }


        #region events

        /// <summary>
        /// handles click on a type
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HandleTypeClick(object sender, ButtonPressedEventArgs args) {
            var mouseEvent = args.OriginalEventArgs as MouseEventArgs;
            FileType clickedType = ((SelectorButton<FileType>)sender).Type;

            // on right click
            if (mouseEvent != null && mouseEvent.Button == MouseButtons.Right) {
                // everything is unactive but this one
                if (_displayedTypes.Count(b => b.Value.Activated) == 1 && _displayedTypes.First(b => b.Value.Activated).Key == clickedType) {
                    SetUnActiveType(null);
                } else {
                    SetActiveType(new List<FileType> {clickedType});
                }
            } else
            // left click is only a toggle
                _displayedTypes[clickedType].Activated = !_displayedTypes[clickedType].Activated;

            _displayedTypes[clickedType].Invalidate();
            ApplyFilter();
        }

        #endregion

        #region Filter
        /// <summary>
        /// this methods sorts the items to put the best match on top and then filter it with modelFilter
        /// </summary>
        private void ApplyFilter() {
            // order the list, first the ones that are equals to the filter, then the
            // ones that start with the filter, then the rest
            if (string.IsNullOrEmpty(_filterByText)) {
                    ovl.SetObjects(_initialObjectsList);
            } else {
                char firstChar = char.ToUpperInvariant(_filterByText[0]);
                ovl.SetObjects(_initialObjectsList.OrderBy(
                x => {
                    if (x.FileName.Length < 1 || char.ToUpperInvariant(x.FileName[0]) != firstChar) return 2;
                    return x.FileName.Equals(_filterByText, StringComparison.CurrentCultureIgnoreCase) ? 0 : 1;
                }).ToList());
            }

            // apply the filter, need to match the filter + need to be an active type (Selector button activated)
            // + need to be in the right scope for variables
            _currentLineNumber = Npp.Line.CurrentLine;
            _currentOwnerName = ParserHandler.GetCarretLineOwnerName(_currentLineNumber);
            _filterString = _filterByText;
            _useTypeFiltering = true;
            ovl.ModelFilter = new ModelFilter(FilterPredicate);

            ovl.DefaultRenderer = new CustomHighlightTextRenderer(_filterByText);

            // update total items
            TotalItems = ((ArrayList)ovl.FilteredObjects).Count;
            nbitems.Text = TotalItems + StrItems;

            // if the selected row is > to number of items, then there will be a unselect
            if (TotalItems <= Config.Instance.AutoCompleteShowListOfXSuggestions)
                FileName.Width = _normalWidth;
            if (ovl.SelectedIndex == -1) ovl.SelectedIndex = 0;
            if (ovl.SelectedIndex >= 0)
                ovl.EnsureVisible(ovl.SelectedIndex);
        }

        /// <summary>
        /// if true, the item isn't filtered
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private static bool FilterPredicate(object o) {
            var compData = (FileObject)o;
            // check for the filter match, the activated category,
            bool output = compData.FileName.ToLower().FullyMatchFilter(_filterString);
            if (_useTypeFiltering && _displayedTypes.ContainsKey(compData.Type))
                output = output && _displayedTypes[compData.Type].Activated;

            return output;
        }

        #endregion
    }

    #region sorting
    /// <summary>
    /// Class used in objectlist.Sort method
    /// </summary>
    public class FilesSortingClass : IComparer<FileObject> {
        public int Compare(FileObject x, FileObject y) {

            // compare first by FileType
            int compare = AutoComplete.GetPriorityList[(int)x.Type].CompareTo(AutoComplete.GetPriorityList[(int)y.Type]);
            if (compare != 0) return compare;

            // then by ranking
            //compare = y.Ranking.CompareTo(x.Ranking);
            //if (compare != 0) return compare;

            // sort by display text in last resort
            return string.Compare(x.FileName, y.FileName, StringComparison.CurrentCultureIgnoreCase);
        }
    }
    #endregion
}
