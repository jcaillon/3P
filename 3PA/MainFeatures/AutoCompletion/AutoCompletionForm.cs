#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (AutoCompletionForm.cs) is part of 3P.
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
using _3PA.Images;
using _3PA.Lib;
using _3PA.MainFeatures.FilteredLists;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.AutoCompletion {

    /// <summary>
    /// This class create an autocompletion window
    /// </summary>
    internal partial class AutoCompletionForm : NppInterfaceForm.NppInterfaceForm {

        #region fields
        private const string StrEmptyList = "No suggestions!";
        private const string StrItems = " items";

        /// <summary>
        /// The filter to apply to the autocompletion form
        /// </summary>
        public string FilterByText {
            get { return _filterByText; }
            set {
                _filterByText = value.ToLower();
                ApplyFilter();
            }
        }

        /// <summary>
        /// lowercased Filter string
        /// </summary>
        private static string _filterByText = "";

        /// <summary>
        ///  gets or sets the total items currently displayed in the form
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Raised when the user presses TAB or ENTER or double click
        /// </summary>
        public event EventHandler<TabCompletedEventArgs> InsertSuggestion;

        // the private fields below are used for the filter function
        private static Dictionary<CompletionType, SelectorButton<CompletionType>> _displayedTypes;
        private static bool _useTypeFiltering;
        private static bool _useTextFiltering;
        private static ParsedScopeItem _currrentScope;
        private static int _currentLineNumber;

        private int _currentType;

        // check the npp window rect, if it has changed from a previous state, close this form (poll every 500ms)
        private int _normalWidth;

        // remember the list that was passed to the autocomplete form when we set the items, we need this
        // because we reorder the list each time the user filters stuff, but we need the original order
        private List<CompletionItem> _initialObjectsList;

        /// <summary>
        /// True if the form is ABOVE the text it autocompletes
        /// </summary>
        private bool _isReversed;

        /// <summary>
        /// returns the ranking of each CompletionType, helps sorting them as we wish
        /// </summary>
        public static List<int> GetPriorityList {
            get {
                if (_completionTypePriority != null) return _completionTypePriority;
                _completionTypePriority = Config.GetPriorityList(typeof(CompletionType), "AutoCompletePriorityList");
                return _completionTypePriority;
            }
        }

        // holds the display order of the CompletionType
        private static List<int> _completionTypePriority;

        #endregion

        #region constructor

        /// <summary>
        /// Constructor for the autocompletion form
        /// </summary>
        /// <param name="initialFilter"></param>
        public AutoCompletionForm(string initialFilter) {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);

            InitializeComponent();

            // Style the control
            OlvStyler.StyleIt(fastOLV, StrEmptyList);
            fastOLV.DefaultRenderer = new FilteredItemTextRenderer();

            // set the image list to use for the keywords
            Keyword.ImageGetter += rowObject => {
                var x = (CompletionItem)rowObject;
                if (x == null) return ImageResources.Error;
                return GetTypeImageFromStr(x.Type.ToString());
            };

            // decorate rows
            fastOLV.UseCellFormatEvents = true;
            fastOLV.FormatCell += FastOlvOnFormatCell;

            // we prevent further sorting
            fastOLV.BeforeSorting += FastOlvOnBeforeSorting;
            fastOLV.KeyDown += FastOlvOnKeyDown;

            fastOLV.UseTabAsInput = true;
            _filterByText = initialFilter.ToLower();

            // handles mouse leave/mouse enter
            MouseLeave += CustomOnMouseLeave;
            fastOLV.MouseLeave += CustomOnMouseLeave;
            fastOLV.DoubleClick += FastOlvOnDoubleClick;
        }

        #endregion

        #region cell formatting
        /// <summary>
        /// Return the image that needs to be display on the left of an item
        /// representing its type
        /// </summary>
        /// <param name="typeStr"></param>
        /// <returns></returns>
        private static Image GetTypeImageFromStr(string typeStr) {
            Image tryImg = (Image)ImageResources.ResourceManager.GetObject(typeStr);
            return tryImg ?? ImageResources.Error;
        }

        /// <summary>
        /// Event on format cell
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void FastOlvOnFormatCell(object sender, FormatCellEventArgs args) {
            CompletionItem item = (CompletionItem)args.Model;
            if (item == null)
                return;

            // display the flags
            int offset = -5;
            item.DoForEachFlag((name, flag) => {
                Image tryImg = (Image)ImageResources.ResourceManager.GetObject(name);
                if (tryImg != null) {
                    ImageDecoration decoration = new ImageDecoration(tryImg, 100, ContentAlignment.MiddleRight) {
                        Offset = new Size(offset, 0)
                    };
                    if (args.SubItem.Decoration == null)
                        args.SubItem.Decoration = decoration;
                    else
                        args.SubItem.Decorations.Add(decoration);
                    offset -= 20;
                }
            });

            // display the sub string
            if (offset < -5) offset -= 5; 
            if (!String.IsNullOrEmpty(item.SubString)) {
                TextDecoration decoration = new TextDecoration(item.SubString, 100) {
                    Alignment = ContentAlignment.MiddleRight,
                    Offset = new Size(offset, 0),
                    Font = FontManager.GetFont(FontStyle.Bold, 10),
                    TextColor = ThemeManager.Current.SubTextFore,
                    CornerRounding = 1f,
                    Rotation = 0,
                    BorderWidth = 1,
                    BorderColor = ThemeManager.Current.SubTextFore
                };
                args.SubItem.Decorations.Add(decoration);
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// set the items of the object view list, correct the width and height of the form and the list
        /// create a button for each type of completion present in the list of items
        /// </summary>
        /// <param name="objectsList"></param>
        /// <param name="resetSelectorButtons"></param>
        public void SetItems(List<CompletionItem> objectsList, bool resetSelectorButtons = true) {
            objectsList.Sort(new CompletionDataSortingClass());
            _initialObjectsList = objectsList;

            // set the default height / width
            fastOLV.Height = 21 * Config.Instance.AutoCompleteShowListOfXSuggestions;
            Height = fastOLV.Height + 32;
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
                int xPos = 4;
                _displayedTypes = new Dictionary<CompletionType, SelectorButton<CompletionType>>();
                foreach (var type in objectsList.Select(x => x.Type).Distinct()) {
                    var but = new SelectorButton<CompletionType> {
                        BackGrndImage = GetTypeImageFromStr(type.ToString()),
                        Activated = true,
                        Size = new Size(24, 24),
                        TabStop = false,
                        Location = new Point(xPos, Height - 28),
                        Type = type,
                        AcceptsRightClick = true,
                        HideFocusedIndicator = true
                    };
                    but.ButtonPressed += HandleTypeClick;
                    htmlToolTip.SetToolTip(but, "The <b>" + type + "</b> category:<br><br><b>Left click</b> to toggle on/off this filter<br><b>Right click</b> to filter for this category only<br><i>(a consecutive right click reactivate all the categories)</i><br><br><i>You can use <b>ALT+RIGHT ARROW KEY</b> (and LEFT ARROW KEY)<br>to quickly activate one category</i>");
                    _displayedTypes.Add(type, but);
                    Controls.Add(but);
                    xPos += but.Width;
                }
                xPos += 65;

                // correct width
                var neededWidth = Math.Max(280, xPos);
                if (neededWidth != Width) {
                    Width = Math.Max(310, xPos);
                    _normalWidth = Width - 2;
                    Keyword.Width = _normalWidth - (Config.Instance.AutoCompleteHideScrollBar ? 0 : 17);
                }
            }

            // label for the number of items
            TotalItems = objectsList.Count;
            ExecuteOnThread(() => {
                nbitems.Text = TotalItems + StrItems;
            });
        }

        /// <summary>
        /// Call this method before showing the list when you don't use SetItems to sort the
        /// items (it is already called by SetItems())
        /// </summary>
        public void SortItems() {
            _initialObjectsList.Sort(new CompletionDataSortingClass());
        }

        /// <summary>
        /// use this to programmatically uncheck any type that is not in the given list
        /// </summary>
        /// <param name="allowedType"></param>
        public void SetActiveType(List<CompletionType> allowedType) {
            if (_displayedTypes == null) return;
            if (allowedType == null) allowedType = new List<CompletionType>();
            foreach (var selectorButton in _displayedTypes) {
                selectorButton.Value.Activated = allowedType.IndexOf(selectorButton.Value.Type) >= 0;
            }
        }

        /// <summary>
        /// use this to programmatically check any type that is not in the given list
        /// </summary>
        /// <param name="allowedType"></param>
        public void SetUnActiveType(List<CompletionType> allowedType) {
            if (_displayedTypes == null) return;
            if (allowedType == null) allowedType = new List<CompletionType>();
            foreach (var selectorButton in _displayedTypes) {
                selectorButton.Value.Activated = allowedType.IndexOf(selectorButton.Value.Type) < 0;
            }
        }

        /// <summary>
        /// reset all the button Types to activated
        /// </summary>
        public void ResetActiveType() {
            if (_displayedTypes == null) return;
            foreach (var selectorButton in _displayedTypes) {
                selectorButton.Value.Activated = true;
            }
        }

        /// <summary>
        /// allows to programmatically select the first item of the list
        /// </summary>
        public void SelectFirstItem() {
            if (TotalItems > 0) {
                fastOLV.TopItemIndex = 0;
                fastOLV.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Position the window in a smart way according to the Point in input
        /// </summary>
        /// <param name="position"></param>
        /// <param name="lineHeight"></param>
        public void SetPosition(Point position, int lineHeight) {
            var screen = Screen.FromPoint(position);
            // position the window smartly
            if (position.X > screen.WorkingArea.X + screen.WorkingArea.Width / 2)
                position.X = position.X - Width;
            if (position.Y > screen.WorkingArea.Y + screen.WorkingArea.Height / 2) {
                position.Y = position.Y - Height - lineHeight;
                _isReversed = true;
            } else
                _isReversed = false;
            Location = position;
        }

        /// <summary>
        /// autocomplete with the currently selected item
        /// </summary>
        public void AcceptCurrentSuggestion() {
            var obj = GetCurrentSuggestion();
            if (obj != null)
                OnTabCompleted(new TabCompletedEventArgs(obj));
        }

        /// <summary>
        /// Get the current selected item
        /// </summary>
        /// <returns></returns>
        public CompletionItem GetCurrentSuggestion() {
            try {
                return (CompletionItem) fastOLV.SelectedItem.RowObject;
            } catch (Exception x) {
                if (!(x is NullReferenceException))
                    ErrorHandler.Log(x.Message);
            }
            return null;
        }

        #endregion

        #region events
        /// <summary>
        /// handles double click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void FastOlvOnDoubleClick(object sender, EventArgs eventArgs) {
            AcceptCurrentSuggestion();
        }

        /// <summary>
        /// Handles keydown event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="keyEventArgs"></param>
        private void FastOlvOnKeyDown(object sender, KeyEventArgs keyEventArgs) {
            keyEventArgs.Handled = OnKeyDown(keyEventArgs.KeyCode);
        }

        /// <summary>
        /// cancel any sort of.. sorting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="beforeSortingEventArgs"></param>
        private void FastOlvOnBeforeSorting(object sender, BeforeSortingEventArgs beforeSortingEventArgs) {
            beforeSortingEventArgs.Canceled = true;
        }

        /// <summary>
        /// handles click on a type
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HandleTypeClick(object sender, EventArgs args) {
            var mouseEvent = args as MouseEventArgs;
            CompletionType clickedType = ((SelectorButton<CompletionType>)sender).Type;

            // on right click
            if (mouseEvent != null && mouseEvent.Button == MouseButtons.Right) {
                // everything is unactive but this one
                if (_displayedTypes.Count(b => b.Value.Activated) == 1 && _displayedTypes.First(b => b.Value.Activated).Key == clickedType) {
                    SetUnActiveType(null);
                } else {
                    SetActiveType(new List<CompletionType> { clickedType });
                }
            } else
                // left click is only a toggle
                _displayedTypes[clickedType].Activated = !_displayedTypes[clickedType].Activated;

            ApplyFilter();
            // give focus back
            GiveFocusBack();
        }

        protected void CustomOnMouseLeave(object sender, EventArgs e) {
            if (IsActivated) GiveFocusBack();
        }

        protected virtual void OnTabCompleted(TabCompletedEventArgs e) {
            var handler = InsertSuggestion;
            if (handler != null) handler(this, e);
        }
        #endregion

        #region on key events

        public bool OnKeyDown(Keys key) {
            bool handled = true;
            // down and up change the selection
            if (key == Keys.Up) {
                if (fastOLV.SelectedIndex > 0)
                    fastOLV.SelectedIndex--;
                else
                    fastOLV.SelectedIndex = (TotalItems - 1);
                if (fastOLV.SelectedIndex >= 0)
                    fastOLV.EnsureVisible(fastOLV.SelectedIndex);
            } else if (key == Keys.Down) {
                if (fastOLV.SelectedIndex < (TotalItems - 1))
                    fastOLV.SelectedIndex++;
                else
                    fastOLV.SelectedIndex = 0;
                if (fastOLV.SelectedIndex >= 0)
                    fastOLV.EnsureVisible(fastOLV.SelectedIndex);

                // escape close
            } else if (key == Keys.Escape) {
                Close();
                InfoToolTip.InfoToolTip.Close();

                // left and right keys
            } else if (key == Keys.Left) {
                LeftRight(true);

            } else if (key == Keys.Right) {
                LeftRight(false);

                // enter and tab accept the current selection
            } else if ((key == Keys.Enter && Config.Instance.AutoCompleteUseEnterToAccept) || (key == Keys.Tab && Config.Instance.AutoCompleteUseTabToAccept)) {
                AcceptCurrentSuggestion();

                // else, any other key needs to be analysed by Npp
            } else {
                handled = false;
            }

            // down and up activate the display of tooltip
            if (key == Keys.Up || key == Keys.Down) {
                InfoToolTip.InfoToolTip.ShowToolTipFromAutocomplete(GetCurrentSuggestion(), new Rectangle(new Point(Location.X, Location.Y), new Size(Width, Height)), _isReversed);
            }
            return handled;
        }

        private void LeftRight(bool isLeft) {
            // only 1 type is active
            if (_displayedTypes.Count(b => b.Value.Activated) == 1) {
                _currentType = _displayedTypes.FindIndex(pair => pair.Value.Activated) + (isLeft ? -1 : 1);
            }
            if (_currentType > _displayedTypes.Count - 1) _currentType = 0;
            if (_currentType < 0) _currentType = _displayedTypes.Count - 1;
            SetActiveType(new List<CompletionType> { _displayedTypes.ElementAt(_currentType).Key });
            ApplyFilter();
        }
        #endregion

        #region Filter
        /// <summary>
        /// this methods sorts the items to put the best match on top and then filter it with modelFilter
        /// </summary>
        private void ApplyFilter() {
            Keyword.Width = _normalWidth - (Config.Instance.AutoCompleteHideScrollBar ? 0 : 17);

            // save position in the list
            var curPos = new Point(fastOLV.SelectedIndex, fastOLV.TopItemIndex);

            // apply filter to each item in the list then set the list
            try {
                _initialObjectsList.ForEach(data => data.FilterApply(_filterByText));
            } catch (Exception e) {
                if (!(e is NullReferenceException))
                    ErrorHandler.Log(e.ToString());
            }
            if (String.IsNullOrEmpty(_filterByText)) {
                fastOLV.SetObjects(_initialObjectsList);
            } else {
                fastOLV.SetObjects(_initialObjectsList.OrderBy(data => data.FilterDispertionLevel).ToList());
            }

            // apply the filter, need to match the filter + need to be an active type (Selector button activated)
            // + need to be in the right scope for variables
            _currentLineNumber = Npp.Line.CurrentLine;
            _currrentScope = ParserHandler.GetScopeOfLine(_currentLineNumber);
            if (!Config.Instance.AutoCompleteOnlyShowDefinedVar) {
                _currentLineNumber = -1;
            }
            _useTypeFiltering = true;
            _useTextFiltering = true;
            fastOLV.ModelFilter = new ModelFilter(FilterPredicate);

            // update total items
            TotalItems = ((ArrayList) fastOLV.FilteredObjects).Count;
            nbitems.Text = TotalItems + StrItems;

            if (TotalItems <= Config.Instance.AutoCompleteShowListOfXSuggestions)
                Keyword.Width = _normalWidth;

            // reposition the cursor in the list
            if (TotalItems > 0) {
                fastOLV.SelectedIndex = Math.Max(0, Math.Min(curPos.X, TotalItems - 1));
                fastOLV.TopItemIndex = Math.Max(0, Math.Min(curPos.Y, TotalItems - 1));
            }
        }

        /// <summary>
        /// if true, the item isn't filtered
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private static bool FilterPredicate(object o) {
            var compData = (CompletionItem)o;
            if (compData == null)
                return false;

            // check for the filter match
            bool output = !_useTextFiltering || compData.FilterFullyMatch;

            // check for activated categories
            if (_useTypeFiltering && _displayedTypes.ContainsKey(compData.Type))
                output = output && _displayedTypes[compData.Type].Activated;

            // if the item isn't a parsed item, it is available no matter where we are in the code
            if (!compData.FromParser) return output;

            // case of Parsed define or temp table define
            if (compData.ParsedItem is ParsedDefine || compData.ParsedItem is ParsedTable || compData.ParsedItem is ParsedLabel) {
                // check for scope
                if (_currrentScope != null && !(compData.ParsedItem.Scope is ParsedFile)) {
                    output = output && compData.ParsedItem.Scope.ScopeType == _currrentScope.ScopeType;
                    output = output && compData.ParsedItem.Scope.Name.Equals(_currrentScope.Name);
                }

                if (_currentLineNumber >= 0) {
                    // check for the definition line
                    output = output && _currentLineNumber >= (compData.ParsedItem.IncludeLine >= 0 ? compData.ParsedItem.IncludeLine : compData.ParsedItem.Line);

                    // for labels, only dislay them in the block which they label
                    if (compData.ParsedItem is ParsedLabel)
                        output = output && _currentLineNumber <= ((ParsedLabel) compData.ParsedItem).UndefinedLine;
                }

            } else if (compData.ParsedItem is ParsedPreProc) {
                if (_currentLineNumber >= 0) {
                    // if preproc, check line of definition and undefine
                    var parsedItem = (ParsedPreProc) compData.ParsedItem;

                    output = output && _currentLineNumber >= (parsedItem.IncludeLine >= 0 ? parsedItem.IncludeLine : parsedItem.Line);
                    if (parsedItem.UndefinedLine > 0)
                        output = output && _currentLineNumber <= parsedItem.UndefinedLine;
                }
            }

            return output;
        }

        /// <summary>
        /// Applies the same sorting / filtering as the autocompletion form to a given list
        /// of items
        /// </summary>
        public static List<CompletionItem> ExternalFilterItems(List<CompletionItem> objectsList, int line, bool dontCheckLine = false) {
            objectsList.Sort(new CompletionDataSortingClass());
            if (_displayedTypes == null)
                _displayedTypes = new Dictionary<CompletionType, SelectorButton<CompletionType>>();
            _useTypeFiltering = false;
            _useTextFiltering = false;
            _currrentScope = ParserHandler.GetScopeOfLine(line);
            _currentLineNumber = (!Config.Instance.AutoCompleteOnlyShowDefinedVar || dontCheckLine) ? -1 : line;
            return objectsList.Where(FilterPredicate).ToList();
        }

        #endregion

        #region misc

        private void ExecuteOnThread(Action action) {
            if (IsHandleCreated)
                BeginInvoke(action);
        }

        #endregion

    }

    #region sorting
    /// <summary>
    /// Class used in objectlist.Sort method
    /// </summary>
    internal class CompletionDataSortingClass : IComparer<CompletionItem> {

        /// <summary>
        /// to sort ascending : if x > y then return 1 if x < y then return -1; and
        /// x.compareTo(y) -> if x > y then return 1 if x < y then return -1;
        /// </summary>
        public int Compare(CompletionItem x, CompletionItem y) {

            // compare first by CompletionType
            int compare = AutoCompletionForm.GetPriorityList[(int)x.Type].CompareTo(AutoCompletionForm.GetPriorityList[(int)y.Type]);
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
            if (x.ParsedItem != null && y.ParsedItem != null) {
                compare = ((int)y.ParsedItem.Scope.ScopeType).CompareTo(((int)x.ParsedItem.Scope.ScopeType));
                if (compare != 0) return compare;
            }

            // if keyword (ascending)
            if (x.Type == CompletionType.Keyword || x.Type == CompletionType.KeywordObject) {
                compare = ((int)x.KeywordType).CompareTo(((int)y.KeywordType));
                if (compare != 0) return compare;
            }

            // sort by display text in last resort
            return string.Compare(x.DisplayText, y.DisplayText, StringComparison.CurrentCultureIgnoreCase);
        }
    }
    #endregion

    #region SelectorButtons

    public class SelectorButton<T> : YamuiButtonImage {
        private bool _activated;

        #region Fields

        public bool Activated {
            get { return _activated; }
            set {
                _activated = value;
                UseGreyScale = !_activated;
            }
        }

        public T Type { get; set; }
        #endregion

    }

    #endregion

    #region TabCompletedEventArgs

    internal sealed class TabCompletedEventArgs : EventArgs {
        /// <summary>
        /// the link href that was clicked
        /// </summary>
        public CompletionItem CompletionItem;

        public TabCompletedEventArgs(CompletionItem completionItem) {
            CompletionItem = completionItem;
        }
    }

    #endregion

}
