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

namespace _3PA.MainFeatures.AutoCompletion {

    /// <summary>
    /// This class create an autocompletion window
    /// </summary>
    public partial class AutoCompletionForm : NppInterfaceForm.NppInterfaceForm {

        #region fields
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

        private Dictionary<CompletionType, SelectorButton> _activeTypes;
        private string _filterString;
        // check the npp window rect, if it has changed from a previous state, close this form (poll every 500ms)
        private int _normalWidth;
        private List<CompletionData> _initialObjectsList;
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
            fastOLV.HotItemStyle = new HotItemStyle();
            fastOLV.HotItemStyle.BackColor = ThemeManager.Current.AutoCompletionHoverBackColor;
            fastOLV.HotItemStyle.ForeColor = ThemeManager.Current.AutoCompletionHoverForeColor;

            // set the image list to use for the keywords
            _imageListOfTypes = new ImageList {
                TransparentColor = Color.Transparent,
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(20, 20)
            };
            ImagelistAdd.AddFromImage(ImageResources.Keyword, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Table, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.TempTable, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Field, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.FieldPk, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Snippet, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Function, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Procedure, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.UserVariablePrimitive, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.UserVariableOther, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Preprocessed, _imageListOfTypes);
            fastOLV.SmallImageList = _imageListOfTypes;
            Keyword.ImageGetter += rowObject => {
                var x = (CompletionData) rowObject;
                return (int) x.Type;
            };

            // overlay of empty list :
            fastOLV.EmptyListMsg = "No suggestions!";
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
            if (((CompletionData)args.Model).Flag.HasFlag(CompletionFlag.None)) {
                TextDecoration decoration = new TextDecoration("R", 100);
                decoration.Alignment = ContentAlignment.MiddleRight;
                decoration.Offset = new Size(-5, 0);
                decoration.Font = FontManager.GetFont(FontStyle.Bold, 11);
                decoration.TextColor = ThemeManager.Current.AutoCompletionNormalSubTypeForeColor;
                decoration.CornerRounding = 1f;
                decoration.Rotation = 0;
                decoration.BorderWidth = 1;
                decoration.BorderColor = ThemeManager.Current.AutoCompletionNormalSubTypeForeColor;
                args.SubItem.Decoration = decoration; //NB. Sets Decoration
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
            // we do the sorting (by type and then by ranking)
            objectsList.Sort(new CompletionDataSortingClass());
            _initialObjectsList = objectsList;

            // set the default height / width
            fastOLV.Height = 21*Config.Instance.AutoCompleteShowListOfXSuggestions;
            Height = fastOLV.Height + 32;
            Width = 280;

            // delete any existing buttons
            if (_activeTypes != null) {
                foreach (var selectorButton in _activeTypes) {
                    selectorButton.Value.ButtonPressed -= HandleTypeClick;
                    if (Controls.Contains(selectorButton.Value))
                        Controls.Remove(selectorButton.Value);
                    selectorButton.Value.Dispose();
                }
            }

            // get distinct types, create a button for each
            int xPos = 4;
            _activeTypes = new Dictionary<CompletionType, SelectorButton>();
            foreach (var type in objectsList.Select(x => x.Type).Distinct()) {
                var but = new SelectorButton();
                but.BackGrndImage = _imageListOfTypes.Images[(int) type];
                but.Activated = true;
                but.Size = new Size(24, 24);
                but.TabStop = false;
                but.Location = new Point(xPos, Height - 28);
                but.Type = type;
                but.ButtonPressed += HandleTypeClick;
                _activeTypes.Add(type, but);
                Controls.Add(but);
                xPos += but.Width;
            }
            xPos += 65;

            // correct width
            Width = Math.Max(Width, xPos);
            _normalWidth = Width - 2;
            Keyword.Width = _normalWidth - 17;

            // label for the number of items
            TotalItems = objectsList.Count;
            nbitems.Text = TotalItems + " items";

            fastOLV.SetObjects(objectsList);
        }

        /// <summary>
        /// use this to programmatically uncheck any type that is not in the given list
        /// </summary>
        /// <param name="allowedType"></param>
        public void SetActiveType(List<CompletionType> allowedType) {
            if (_activeTypes == null) return;
            foreach (var selectorButton in _activeTypes) {
                if (allowedType.IndexOf(selectorButton.Value.Type) < 0) {
                    selectorButton.Value.Activated = false;
                    selectorButton.Value.Invalidate();
                }
            }
        }

        /// <summary>
        /// reset all the button Types to activated
        /// </summary>
        public void ResetActiveType() {
            if (_activeTypes == null) return;
            foreach (var selectorButton in _activeTypes) {
                selectorButton.Value.Activated = true;
                selectorButton.Value.Invalidate();
            }
        }

        /// <summary>
        /// allows to programmatically select the first item of the list
        /// </summary>
        public void SelectFirstItem() {
            try {
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
        private void HandleTypeClick(object sender, EventArgs args) {
            CompletionType clickedType = ((SelectorButton) sender).Type;
            if (_activeTypes[clickedType].Activated) {
                // if everything is active, what we want to do is make everything but this one inactive
                if (_activeTypes.Count(b => !b.Value.Activated) == 0) {
                    foreach (CompletionType key in _activeTypes.Keys.ToList()) {
                        _activeTypes[key].Activated = false;
                        _activeTypes[key].Invalidate();
                    }
                    _activeTypes[clickedType].Activated = true;
                } else if (_activeTypes.Count(b => b.Value.Activated) == 1) {
                    foreach (CompletionType key in _activeTypes.Keys.ToList()) {
                        _activeTypes[key].Activated = true;
                        _activeTypes[key].Invalidate();
                    }
                } else
                    _activeTypes[clickedType].Activated = !_activeTypes[clickedType].Activated;
            } else
                _activeTypes[clickedType].Activated = !_activeTypes[clickedType].Activated;
            _activeTypes[clickedType].Invalidate();
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

                // enter and tab accept the current selection
            } else if ((key == Keys.Enter && Config.Instance.AutoCompleteUseEnterToAccept) || (key == Keys.Tab && Config.Instance.AutoCompleteUseTabToAccept)) {
                AcceptCurrentSuggestion();

                // else, any other key needs to be analysed by Npp
            } else {
                handled = false;
            }
            return handled;
        }

        #endregion

        #region private methods
        /// <summary>
        /// this methods sorts the items to put the best match on top and then filter it with modelFilter
        /// </summary>
        private void ApplyFilter() {
            fastOLV.SetObjects(_initialObjectsList.OrderBy(
                x => {
                    if (!x.DisplayText.StartsWith(_filterString, StringComparison.OrdinalIgnoreCase))
                        return 2;
                    if (x.DisplayText.Equals(_filterString, StringComparison.OrdinalIgnoreCase)) {
                        return 0;
                    }   
                    return 1;
            }).ToList());

            fastOLV.ModelFilter = new ModelFilter((o => ((CompletionData) o).DisplayText.ToLower().FullyMatchFilter(_filterString) && _activeTypes[((CompletionData) o).Type].Activated));
            fastOLV.DefaultRenderer = new CustomHighlightTextRenderer(_filterString);

            // update total items
            TotalItems = ((ArrayList) fastOLV.FilteredObjects).Count;
            nbitems.Text = TotalItems + " items";

            // if the selected row is > to number of items, then there will be a unselect
            try {
                Keyword.Width = _normalWidth - ((TotalItems <= Config.Instance.AutoCompleteShowListOfXSuggestions) ? 0 : 17);
                if (fastOLV.SelectedIndex == - 1) fastOLV.SelectedIndex = 0;
                fastOLV.EnsureVisible(fastOLV.SelectedIndex);
            } catch (Exception) {
                // ignored
            }
        }
        #endregion
    }

    #region sorting

    /// <summary>
    /// Class used in objectlist.Sort method
    /// </summary>
    public class CompletionDataSortingClass : IComparer<CompletionData> {
        public int Compare(CompletionData x, CompletionData y) {
            int compare = x.Type.CompareTo(y.Type);
            if (compare == 0) {
                return y.Ranking.CompareTo(x.Ranking);
            }
            return compare;
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
