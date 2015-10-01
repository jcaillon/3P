using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using BrightIdeasSoftware.Utilities;
using YamuiFramework.Controls;
using YamuiFramework.Fonts;
using YamuiFramework.Themes;
using _3PA.Images;
using _3PA.Lib;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.AutoCompletion {

    /// <summary>
    /// This class create an autocompletion window
    /// </summary>
    public partial class AutoCompletionForm : NppInterfaceForm.NppInterfaceForm {

        #region fields
        private const string StrEmptyList = "No suggestions!";
        private const string StrItems = " items";

        /// <summary>
        /// The filter to apply to the autocompletion form
        /// </summary>
        public string FilterByText {
            get { return _filterString; }
            set { _filterString = value.ToLower(); ApplyFilter(); }
        }

        public bool UseAlternateBackColor {
            set { fastOLV.UseAlternatingBackColors = value; }
        }

        /// <summary>
        ///  gets or sets the total items currently displayed in the form
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Raised when the user presses TAB or ENTER or double click
        /// </summary>
        public event EventHandler<TabCompletedEventArgs> TabCompleted;

        // the private fields below are used for the filter function
        private Dictionary<CompletionType, SelectorButton> _displayedTypes;
        private string _filterString;
        private string _currentLcOwnerName = "";
        private int _currentLineNumber;

        private int _currentType;

        // check the npp window rect, if it has changed from a previous state, close this form (poll every 500ms)
        private int _normalWidth;

        // remember the list that was passed to the autocomplete form when we set the items, we need this
        // because we reorder the list each time the user filters stuff, but we need the original order
        private List<CompletionData> _initialObjectsList;
        
        // contains the list of images, one for each type of completionType
        private ImageList _imageListOfTypes;
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
            fastOLV.OwnerDraw = true;
            fastOLV.Font = FontManager.GetLabelFont(LabelFunction.AutoCompletion);
            fastOLV.BackColor = ThemeManager.Current.AutoCompletionNormalBackColor;
            fastOLV.AlternateRowBackColor = ThemeManager.Current.AutoCompletionNormalAlternateBackColor;
            fastOLV.ForeColor = ThemeManager.Current.AutoCompletionNormalForeColor;
            fastOLV.HighlightBackgroundColor = ThemeManager.Current.AutoCompletionFocusBackColor;
            fastOLV.HighlightForegroundColor = ThemeManager.Current.AutoCompletionFocusForeColor;
            fastOLV.UnfocusedHighlightBackgroundColor = fastOLV.HighlightBackgroundColor;
            fastOLV.UnfocusedHighlightForegroundColor = fastOLV.HighlightForegroundColor;

            // Decorate and configure hot item
            fastOLV.UseHotItem = true;
            fastOLV.HotItemStyle = new HotItemStyle {
                BackColor = ThemeManager.Current.AutoCompletionHoverBackColor,
                ForeColor = ThemeManager.Current.AutoCompletionHoverForeColor
            };

            // set the image list to use for the keywords
            _imageListOfTypes = new ImageList {
                TransparentColor = Color.Transparent,
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(20, 20)
            };
            ImagelistAdd.AddFromImage(ImageResources.FieldPk, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Field, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Snippet, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.TempTable, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.UserVariablePrimitive, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.UserVariableOther, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Table, _imageListOfTypes);            
            ImagelistAdd.AddFromImage(ImageResources.Function, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Procedure, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Preprocessed, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Keyword, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Database, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Widget, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.KeywordObject, _imageListOfTypes);
            fastOLV.SmallImageList = _imageListOfTypes;
            Keyword.ImageGetter += rowObject => {
                var x = (CompletionData) rowObject;
                return (int) x.Type;
            };

            // overlay of empty list :
            fastOLV.EmptyListMsg = StrEmptyList;
            TextOverlay textOverlay = fastOLV.EmptyListMsgOverlay as TextOverlay;
            if (textOverlay != null) {
                textOverlay.TextColor = ThemeManager.Current.AutoCompletionNormalForeColor;
                textOverlay.BackColor = ThemeManager.Current.AutoCompletionNormalAlternateBackColor;
                textOverlay.BorderColor = ThemeManager.Current.AutoCompletionNormalForeColor;
                textOverlay.BorderWidth = 4.0f;
                textOverlay.Font = FontManager.GetFont(FontStyle.Bold, 30f);
                textOverlay.Rotation = -5;
            }

            // decorate rows
            fastOLV.UseCellFormatEvents = true;
            fastOLV.FormatCell += FastOlvOnFormatCell;

            // we prevent further sorting
            fastOLV.BeforeSorting += FastOlvOnBeforeSorting;
            fastOLV.KeyDown += FastOlvOnKeyDown;

            fastOLV.UseTabAsInput = true;
            _filterString = initialFilter.ToLower();

            // handles mouse leave/mouse enter
            MouseLeave += CustomOnMouseLeave;
            fastOLV.MouseLeave += CustomOnMouseLeave;
            fastOLV.DoubleClick += FastOlvOnDoubleClick;
        }

        #endregion

        #region cell formatting

        /// <summary>
        /// Event on format cell
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void FastOlvOnFormatCell(object sender, FormatCellEventArgs args) {
            CompletionData data = (CompletionData)args.Model;
            // display the flags
            int offset = -5;
            foreach (var name in Enum.GetNames(typeof(ParseFlag))) {
                ParseFlag flag = (ParseFlag)Enum.Parse(typeof(ParseFlag), name);
                if (flag == 0) continue;
                if (!data.Flag.HasFlag(flag)) continue;
                Image tryImg = (Image)ImageResources.ResourceManager.GetObject(name);
                if (tryImg == null) continue;
                ImageDecoration decoration = new ImageDecoration(tryImg, ContentAlignment.MiddleRight) {
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
            if (!string.IsNullOrEmpty(data.SubString)) {
                TextDecoration decoration = new TextDecoration(data.SubString, 95);
                decoration.Alignment = ContentAlignment.MiddleRight;
                decoration.Offset = new Size(offset, 0);
                decoration.Font = FontManager.GetFont(FontStyle.Bold, 11);
                decoration.TextColor = ThemeManager.Current.AutoCompletionNormalSubTypeForeColor;
                decoration.CornerRounding = 1f;
                decoration.Rotation = 0;
                decoration.BorderWidth = 1;
                decoration.BorderColor = ThemeManager.Current.AutoCompletionNormalSubTypeForeColor;
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
        public void SetItems(List<CompletionData> objectsList) {
            objectsList.Sort(new CompletionDataSortingClass());
            _initialObjectsList = objectsList;

            // set the default height / width
            fastOLV.Height = 21 * Config.Instance.AutoCompleteShowListOfXSuggestions;
            Height = fastOLV.Height + 32;
            //Width = 280;

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
            _displayedTypes = new Dictionary<CompletionType, SelectorButton>();
            foreach (var type in objectsList.Select(x => x.Type).Distinct()) {
                var but = new SelectorButton();
                but.BackGrndImage = _imageListOfTypes.Images[(int) type];
                but.Activated = true;
                but.Size = new Size(24, 24);
                but.TabStop = false;
                but.Location = new Point(xPos, Height - 28);
                but.Type = type;
                but.AcceptsRightClick = true;
                but.ButtonPressed += HandleTypeClick;
                htmlToolTip.SetToolTip(but, "<b>" + type + "</b>:<br><br><b>Left click</b> to toggle on/off this filter<br><b>Right click</b> to filter for this type only");
                _displayedTypes.Add(type, but);
                Controls.Add(but);
                xPos += but.Width;
            }
            xPos += 65;

            // correct width
            var neededWidth = Math.Max(280, xPos);
            if (neededWidth != Width) {
                Width = Math.Max(280, xPos);
                _normalWidth = Width - 2;
                Keyword.Width = _normalWidth - 17;
            }

            // label for the number of items
            TotalItems = objectsList.Count;
            nbitems.Text = TotalItems + StrItems;

            fastOLV.SetObjects(objectsList);
        }

        /// <summary>
        /// Call this method before showing the list when you don't use SetItems to sort the
        /// items (it is already called by SetItems())
        /// </summary>
        public void SortItems() {
            _initialObjectsList.Sort(new CompletionDataSortingClass());
            fastOLV.SetObjects(_initialObjectsList);
        }

        /// <summary>
        /// use this to programmatically uncheck any type that is not in the given list
        /// </summary>
        /// <param name="allowedType"></param>
        public void SetActiveType(List<CompletionType> allowedType) {
            if (_displayedTypes == null) return;
            foreach (var selectorButton in _displayedTypes) {
                selectorButton.Value.Activated = allowedType.IndexOf(selectorButton.Value.Type) >= 0;
                selectorButton.Value.Invalidate();
            }
        }

        /// <summary>
        /// use this to programmatically check any type that is not in the given list
        /// </summary>
        /// <param name="allowedType"></param>
        public void SetUnActiveType(List<CompletionType> allowedType) {
            if (_displayedTypes == null) return;
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
            try {
                if (TotalItems > 0)
                    fastOLV.SelectedIndex = 0;
            } catch (Exception) {
                // ignored
            }
        }

        /// <summary>
        /// Position the window in a smart way according to the Point in input
        /// </summary>
        /// <param name="position"></param>
        /// <param name="lineHeight"></param>
        public void SetPosition(Point position, int lineHeight) {
            // position the window smartly
            if (position.X > Screen.PrimaryScreen.WorkingArea.X + 2*Screen.PrimaryScreen.WorkingArea.Width/3)
                position.X = position.X - Width;
            if (position.Y > Screen.PrimaryScreen.WorkingArea.Y + 3*Screen.PrimaryScreen.WorkingArea.Height/5)
                position.Y = position.Y - Height - lineHeight;
            Location = position;
        }

        /// <summary>
        /// autocomplete with the currently selected item
        /// </summary>
        public void AcceptCurrentSuggestion() {
            var obj = (CompletionData) fastOLV.SelectedItem.RowObject;
            if (obj != null)
                OnTabCompleted(new TabCompletedEventArgs(obj));
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
        private void HandleTypeClick(object sender, ButtonPressedEventArgs args) {
            var mouseEvent = args.OriginalEventArgs as MouseEventArgs;
            CompletionType clickedType = ((SelectorButton) sender).Type;

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

            _displayedTypes[clickedType].Invalidate();
            ApplyFilter();
            // give focus back
            GiveFocusBack();
        }

        protected void CustomOnMouseLeave(object sender, EventArgs e) {
            if (IsActivated) GiveFocusBack();
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            fastOLV.SelectedIndex = 0;
            //if (!string.IsNullOrEmpty(_filterString)) ApplyFilter();
        }

        protected virtual void OnTabCompleted(TabCompletedEventArgs e) {
            var handler = TabCompleted;
            if (handler != null) handler(this, e);
        }
        #endregion

        #region "on key events"

        public bool OnKeyDown(Keys key) {
            bool handled = true;
            // down and up change the selection
            if (key == Keys.Up) {
                if (fastOLV.SelectedIndex > 0)
                    fastOLV.SelectedIndex--;
                else
                    fastOLV.SelectedIndex = (TotalItems - 1);
                fastOLV.EnsureVisible(fastOLV.SelectedIndex);
            } else if (key == Keys.Down) {
                if (fastOLV.SelectedIndex < (TotalItems - 1))
                    fastOLV.SelectedIndex++;
                else
                    fastOLV.SelectedIndex = 0;
                fastOLV.EnsureVisible(fastOLV.SelectedIndex);

                // escape close
            } else if (key == Keys.Escape) {
                Close();

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
            return handled;
        }

        private void LeftRight(bool isLeft) {
            // only 1 type is active
            if (_displayedTypes.Count(b => b.Value.Activated) == 1)
                _currentType = _currentType + (isLeft ? -1 : 1);
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
            // order the list, first the ones that are equals to the filter, then the
            // ones that start with the filter, then the rest
            if (string.IsNullOrEmpty(_filterString)) {
                fastOLV.SetObjects(_initialObjectsList);
            } else {
                char firstChar = char.ToUpperInvariant(_filterString[0]);
                fastOLV.SetObjects(_initialObjectsList.OrderBy(
                x => {
                    if (x.DisplayText.Length < 1 || char.ToUpperInvariant(x.DisplayText[0]) != firstChar) return 2;
                    return x.DisplayText.Equals(_filterString, StringComparison.CurrentCultureIgnoreCase) ? 0 : 1;
                }).ToList());
            }

            // apply the filter, need to match the filter + need to be an active type (Selector button activated)
            // + need to be in the right scope for variables
            _currentLcOwnerName = ParserHandler.GetCarretLineLcOwnerName;
            _currentLineNumber = Npp.GetCurrentLineNumber();
            fastOLV.ModelFilter = new ModelFilter(FilterPredicate);
            //((CompletionData) o).DisplayText.ToLower().FullyMatchFilter(_filterString) && _activeTypes[((CompletionData) o).Type].Activate

            fastOLV.DefaultRenderer = new CustomHighlightTextRenderer(_filterString);

            // update total items
            TotalItems = ((ArrayList) fastOLV.FilteredObjects).Count;
            nbitems.Text = TotalItems + StrItems;

            // if the selected row is > to number of items, then there will be a unselect
            try {
                Keyword.Width = _normalWidth - ((TotalItems <= Config.Instance.AutoCompleteShowListOfXSuggestions) ? 0 : 17);
                if (fastOLV.SelectedIndex == - 1) fastOLV.SelectedIndex = 0;
                fastOLV.EnsureVisible(fastOLV.SelectedIndex);
            } catch (Exception) {
                // ignored
            }
        }

        /// <summary>
        /// if true, the item isn't filtered
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private bool FilterPredicate(object o) {
            var compData = (CompletionData)o;
            // check for the filter match, the activated category,
            bool output = compData.DisplayText.ToLower().FullyMatchFilter(_filterString) &&
                _displayedTypes[compData.Type].Activated;

            // if the item isn't a parsed item, it is avaiable no matter where we are in the code
            if (!compData.FromParser) return output;

            // case of Parsed define or temp table define
            if (compData.ParsedItem is ParsedDefine || compData.ParsedItem is ParsedTable) {
                // check for scope
                if (compData.ParsedItem.Scope != ParsedScope.File)
                    output = output && compData.ParsedItem.LcOwnerName.Equals(_currentLcOwnerName);
                // check for the definition line
                output = output && _currentLineNumber >= compData.ParsedItem.Line;

            } else if (compData.ParsedItem is ParsedPreProc) {
                // if preproc, check line of definition and undefine
                var parsedItem = (ParsedPreProc)compData.ParsedItem;
                output = output && _currentLineNumber >= parsedItem.Line;
                if (parsedItem.UndefinedLine > 0)
                    output = output && _currentLineNumber <= parsedItem.UndefinedLine;
            }

            return output;
        }
        #endregion
    }

    #region sorting
    /// <summary>
    /// Class used in objectlist.Sort method
    /// </summary>
    public class CompletionDataSortingClass : IComparer<CompletionData> {
        public int Compare(CompletionData x, CompletionData y) {
            // compare first by CompletionType
            int compare = AutoComplete.GetPriorityList[(int)x.Type].CompareTo(AutoComplete.GetPriorityList[(int)y.Type]);
            if (compare != 0) return compare;
            // then by ranking
            compare = y.Ranking.CompareTo(x.Ranking);
            if (compare != 0) return compare;
            // then sort by parsed items first
            if (x.Type == CompletionType.Table) {
                compare = y.FromParser.CompareTo(x.FromParser);
                if (compare != 0) return compare;
            }
            // then sort by scope
            if (x.ParsedItem != null && y.ParsedItem != null) {
                compare = ((int)y.ParsedItem.Scope).CompareTo(((int)x.ParsedItem.Scope));
                if (compare != 0) return compare;
            }
            // if keyword
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
    public class SelectorButton : YamuiButton {

        #region Fields
        public Image BackGrndImage { get; set; }

        public bool Activated { get; set; }

        public CompletionType Type { get; set; }
        #endregion

        #region Paint Methods
        protected override void OnPaint(PaintEventArgs e) {
            try {
                Color backColor = ThemeManager.ButtonColors.BackGround(BackColor, false, IsFocused, IsHovered, IsPressed, true);
                Color borderColor = ThemeManager.ButtonColors.BorderColor(IsFocused, IsHovered, IsPressed, true);
                var img = BackGrndImage;

                // draw background
                using (SolidBrush b = new SolidBrush(backColor)) {
                    e.Graphics.FillRectangle(b, ClientRectangle);
                }

                // draw main image, in greyscale if not activated
                if (!Activated)
                    img = Utils.MakeGrayscale3(new Bitmap(img, new Size(BackGrndImage.Width, BackGrndImage.Height)));
                var recImg = new Rectangle(new Point((ClientRectangle.Width - img.Width)/2, (ClientRectangle.Height - img.Height)/2), new Size(img.Width, img.Height));
                e.Graphics.DrawImage(img, recImg);

                // border
                recImg = ClientRectangle;
                recImg.Inflate(-2, -2);
                if (borderColor != Color.Transparent) {
                    using (Pen b = new Pen(borderColor, 2f)) {
                        e.Graphics.DrawRectangle(b, recImg);
                    }
                }
            } catch {
                // ignored
            }
        }

        #endregion
    }

    #endregion

    #region TabCompletedEventArgs

    public sealed class TabCompletedEventArgs : EventArgs {
        /// <summary>
        /// the link href that was clicked
        /// </summary>
        public CompletionData CompletionItem;

        public TabCompletedEventArgs(CompletionData completionItem) {
            CompletionItem = completionItem;
        }
    }

    #endregion

}
