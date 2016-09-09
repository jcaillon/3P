using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using YamuiFramework.Fonts;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls.YamuiList {

    public class YamuiFilteredTypeList : YamuiFilteredList {

        #region constants

        protected const int DefaultBottomHeight = 28;

        protected const int MinItemLabelWidth = 45;

        protected const float DefaultSubTextOpacity = 0.3f;

        protected const float DefaultFlagImagesOpacity = 0.5f;

        protected const int BottomPadding = 2;

        protected const int TypeButtonWidth = 22;

        #endregion

        #region public properties

        /// <summary>
        /// Should return the image to use for the corresponding item
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<int, Image> GetObjectTypeImage { get; set; }

        /// <summary>
        /// Should return the sub text to use for the corresponding item
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<FilteredTypeItem, string> GetObjectSubText { get; set; }

        /// <summary>
        /// Should return a list of image to display on the right of the row
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<FilteredTypeItem, List<Image>> GetObjectTagImages { get; set; }

        /// <summary>
        /// The image to display for the button "display more type"
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image MoreTypesImage { get; set; }

        /// <summary>
        /// The padding to apply to display the list
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Padding ListPadding {
            get {
                var pad = base.ListPadding;
                pad.Bottom = pad.Bottom.ClampMin(BottomHeight);
                return pad;
            }
            set { base.ListPadding = value; }
        }

        /// <summary>
        /// Space reserved to write the showing x items label
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BottomHeight {
            get { return _bottomHeight; }
            set { _bottomHeight = value; }
        }

        /// <summary>
        /// Set the opacity for the sub text
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float SubTextOpacity {
            get { return _subTextOpacity; }
            set { _subTextOpacity = value; }
        }

        /// <summary>
        /// Set the opacity for the tag images
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float FlagImagesOpacity {
            get { return _flagImagesOpacity; }
            set { _flagImagesOpacity = value; }
        }

        /// <summary>
        /// Predicate to filter the items, only items meeting the predicate requirements will be displayed (applied in addition to the default string filter)
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Predicate<FilteredItem> FilterPredicate {
            get {
                // add a layer of filter which is a type filter
                return item => {
                    var typeItem = item as FilteredTypeItem;
                    if (typeItem != null) {
                        return (!_typeButtons.ContainsKey(typeItem.ItemType) || _typeButtons[typeItem.ItemType] == null || _typeButtons[typeItem.ItemType].Activated) && 
                            (_filterPredicate != null ? _filterPredicate(item) : true);
                    }
                    return false;
                };
            }
            set { _filterPredicate = value; }
        }

        #endregion

        #region private fields

        protected Dictionary<int, SelectorButton> _typeButtons = new Dictionary<int, SelectorButton>();

        protected List<int> _typeList = new List<int>();

        protected const TextFormatFlags TextRightFlags = TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.Right | TextFormatFlags.NoPadding;

        private int _bottomHeight = DefaultBottomHeight;

        private int _itemsNbLabelWidth = 45;

        private float _subTextOpacity = DefaultSubTextOpacity;

        private float _flagImagesOpacity = DefaultFlagImagesOpacity;

        private int _currentButtonIndex;

        private int _nbDisplayableTypeButton;

        private SelectorButton _moreButton;

        #endregion

        #region Set

        /// <summary>
        /// Constructor to initialize stuff
        /// </summary>
        public YamuiFilteredTypeList() {}

        /// <summary>
        /// Set the items that will be displayed in the list
        /// </summary>
        public override void SetItems(List<ListItem> listItems) {
            var firstItem = listItems.FirstOrDefault();
            if (firstItem != null && !(firstItem is FilteredTypeItem))
                throw new Exception("listItems shoud contain objects of type FilteredTypeItem");

            // measure the space taken by the label "showing x items"
            using (var gImg = new Bitmap(1, 1))
            using (var g = Graphics.FromImage(gImg)) {
                _itemsNbLabelWidth = TextRenderer.MeasureText(g, _nbItems + " items", FontManager.GetFont(FontFunction.Small), ClientSize, TextRightFlags).Width.ClampMin(MinItemLabelWidth);
            }

            // set the type buttons needed
            ResetButtons();
            _typeList = listItems.Select(x => ((FilteredTypeItem) x).ItemType).Distinct().ToList();
            foreach (var type in _typeList)
                if (!_typeButtons.ContainsKey(type))
                    _typeButtons.Add(type, null);

            base.SetItems(listItems);
        }

        #endregion

        #region DrawButtons

        protected override void DrawButtons() {
            base.DrawButtons();

            int maxWidthForTypeButtons = Width - BottomPadding * 3 - _itemsNbLabelWidth - 10;
            _nbDisplayableTypeButton = (int) Math.Floor((decimal) maxWidthForTypeButtons / TypeButtonWidth);

            // for each distinct type of items, create the buttons
            int xPos = BottomPadding;
            int nBut = 0;
            foreach (var type in _typeList) {
                if (_typeButtons[type] == null) {
                    _typeButtons[type] = new SelectorButton {
                        BackGrndImage = GetObjectTypeImage(type),
                        Activated = true,
                        Size = new Size(TypeButtonWidth, DefaultBottomHeight),
                        TabStop = false,
                        Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
                        Type = type,
                        AcceptsRightClick = true,
                        HideFocusedIndicator = true
                    };
                    _typeButtons[type].ButtonPressed += HandleTypeClick;
                    //htmlToolTip.SetToolTip(but, "The <b>" + type + "</b> category:<br><br><b>Left click</b> to toggle on/off this filter<br><b>Right click</b> to filter for this category only<br><i>(a consecutive right click reactivate all the categories)</i><br><br><i>You can use <b>ALT+RIGHT ARROW KEY</b> (and LEFT ARROW KEY)<br>to quickly activate one category</i>");
                }

                _typeButtons[type].Location = new Point(xPos, Height - BottomHeight / 2 - _typeButtons[type].Height / 2);
                nBut++;

                // who as many button as we can show - 1
                if (nBut < _nbDisplayableTypeButton) {
                    xPos += TypeButtonWidth;
                    if (!Controls.Contains(_typeButtons[type]))
                        Controls.Add(_typeButtons[type]);
                } else {
                    if (Controls.Contains(_typeButtons[type]))
                        Controls.Remove(_typeButtons[type]);
                }
            }

            if (nBut > 0) { 

                // if we have enough space to display the last button, display it
                if (nBut <= _nbDisplayableTypeButton) {
                    var lastButton = _typeButtons.LastOrDefault();
                    if (lastButton.Value != null) {
                        if (!Controls.Contains(lastButton.Value))
                            Controls.Add(lastButton.Value);
                    }

                    // remove the more button if it exists
                    if (_moreButton != null) {
                        if (Controls.Contains(_moreButton))
                            Controls.Remove(_moreButton);
                    }
                } else {
                    // otherwise, we display a "more button" that opens a small interface to show the extra buttons
                    if (_moreButton == null) { 
                        _moreButton = new SelectorButton {
                            BackGrndImage = MoreTypesImage,
                            Activated = true,
                            Size = new Size(TypeButtonWidth, DefaultBottomHeight),
                            TabStop = false,
                            Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
                            HideFocusedIndicator = true
                        };
                        _moreButton.ButtonPressed += HandleTypeClick;
                        //htmlToolTip.SetToolTip(but, "The <b>" + type + "</b> category:<br><br><b>Left click</b> to toggle on/off this filter<br><b>Right click</b> to filter for this category only<br><i>(a consecutive right click reactivate all the categories)</i><br><br><i>You can use <b>ALT+RIGHT ARROW KEY</b> (and LEFT ARROW KEY)<br>to quickly activate one category</i>");
                    }
                    _moreButton.Location = new Point(xPos, Height - BottomHeight / 2 - _moreButton.Height / 2);
                    if (!Controls.Contains(_moreButton))
                        Controls.Add(_moreButton);
                }
            }
        }

        #endregion

        #region Draw list

        protected override void OnPaintBackground(PaintEventArgs e) {
            e.Graphics.Clear(!UseCustomBackColor ? YamuiThemeManager.Current.MenuNormalBack : BackColor);

            // text
            var textHeight = (BottomHeight - BottomPadding * 2) /2;
            TextRenderer.DrawText(e.Graphics, "Showing", FontManager.GetFont(FontFunction.Small), new Rectangle(Width - BottomPadding - _itemsNbLabelWidth, Height - BottomHeight + BottomPadding, _itemsNbLabelWidth, textHeight), YamuiThemeManager.Current.MenuNormalFore, TextRightFlags);
            TextRenderer.DrawText(e.Graphics, _nbItems + " items", FontManager.GetFont(FontFunction.Small), new Rectangle(Width - BottomPadding - _itemsNbLabelWidth, Height - textHeight - BottomPadding, _itemsNbLabelWidth, textHeight), YamuiThemeManager.Current.MenuNormalFore, TextRightFlags);
           
        }

        /// <summary>
        /// Called by default to paint the row if no OnRowPaint is defined
        /// </summary>
        protected override void RowPaint(ListItem item, YamuiListRow row, PaintEventArgs e) {
            var backColor = YamuiThemeManager.Current.MenuBg(row.IsSelected, row.IsHovered, !item.IsDisabled);
            var foreColor = YamuiThemeManager.Current.MenuFg(row.IsSelected, row.IsHovered, !item.IsDisabled);

            // background
            e.Graphics.Clear(backColor);

            var curItem = item as FilteredTypeItem;
            if (curItem != null) {

                Image img = null;
                if (GetObjectTypeImage != null)
                    img = GetObjectTypeImage(curItem.ItemType);

                // Image icon
                if (img != null) {
                    var recImg = new Rectangle(new Point(1, (RowHeight - img.Height) / 2), new Size(img.Width, img.Height));
                    e.Graphics.DrawImage(img, recImg);
                }

                // tag images
                var xPos = 1;

                var listImg = GetObjectTagImages(curItem);
                if (listImg != null) {
                    listImg.Reverse();
                    foreach (var image in listImg) {
                        xPos += image.Width;

                        // draw the image with a given opacity
                        ColorMatrix imgColor = new ColorMatrix();
                        imgColor.Matrix33 = FlagImagesOpacity;
                        using (ImageAttributes imgAttrib = new ImageAttributes()) {
                            imgAttrib.SetColorMatrix(imgColor);
                            e.Graphics.DrawImage(image, new Rectangle(new Point(row.Width - xPos, (RowHeight - image.Height)/2), new Size(image.Width, image.Height)), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imgAttrib);
                        }
                    }
                }

                // sub text 
                var subText = GetObjectSubText(curItem);
                if (!string.IsNullOrEmpty(subText)) {
                    var textFont = FontManager.GetFont(FontStyle.Bold, 10);
                    var textSize = TextRenderer.MeasureText(subText, textFont);
                    var subColor = Enabled ? YamuiThemeManager.Current.SubTextFore : foreColor;

                    var drawPoint = new PointF(row.Width - xPos - textSize.Width - 3, (RowHeight / 2) - (textSize.Height / 2) - 1);
                    e.Graphics.DrawString(subText, textFont, new SolidBrush(Color.FromArgb((int)(SubTextOpacity * 255), subColor)), drawPoint);

                    using (var pen = new Pen(Color.FromArgb((int)(SubTextOpacity * 0.8 * 255), subColor), 1) { Alignment = PenAlignment.Left }) {
                        e.Graphics.DrawPath(pen, Utilities.GetRoundedRect(drawPoint.X - 2, drawPoint.Y - 1, textSize.Width + 2, textSize.Height + 3, 3f));
                    }
                }
                
               
                var textRectangle = new Rectangle(3 + (img != null ? img.Width : 0), 0, row.ClientRectangle.Width, RowHeight);
                textRectangle.Width -= textRectangle.X;

                // letter highlight
                if (!item.IsDisabled)
                    DrawTextHighlighting(e.Graphics, curItem.FilterMatchedRanges, textRectangle, item.DisplayText, TextFlags);

                // text
                TextRenderer.DrawText(e.Graphics, item.DisplayText, FontManager.GetStandardFont(), textRectangle, foreColor, TextFlags);
            }
        }

        #endregion

        #region OnKeyDown

        public override bool OnKeyDown(Keys pressedKey) {
            switch (pressedKey) {
                case Keys.Left:
                    LeftRight(true);
                    return true;
                case Keys.Right:
                    LeftRight(false);
                    return true;
            }
            return base.OnKeyDown(pressedKey);
        }

        /// <summary>
        /// Handles the left/right buttons
        /// </summary>
        private void LeftRight(bool isLeft) {
            // only 1 type is active
            if (_typeButtons.Count(b => b.Value.Activated) == 1) {
                _currentButtonIndex = 0;
                foreach (var button in _typeButtons) {
                    if (button.Value.Activated)
                        break;
                    _currentButtonIndex++;
                }
                _currentButtonIndex = _currentButtonIndex + (isLeft ? -1 : 1);
            }
            if (_currentButtonIndex > _typeButtons.Count - 1) _currentButtonIndex = 0;
            if (_currentButtonIndex < 0) _currentButtonIndex = _typeButtons.Count - 1;
            SetActiveType(new List<int> { _typeButtons.ElementAt(_currentButtonIndex).Key });
        }

        #endregion

        #region Active types

        /// <summary>
        /// handles click on a type
        /// </summary>
        private void HandleTypeClick(object sender, EventArgs args) {
            var mouseEvent = args as MouseEventArgs;
            int clickedType = ((SelectorButton)sender).Type;

            // on right click
            if (mouseEvent != null && mouseEvent.Button == MouseButtons.Right) {
                // everything is unactive but this one
                if (_typeButtons.Count(b => b.Value.Activated) == 1 && _typeButtons.First(b => b.Value.Activated).Key == clickedType) {
                    SetUnActiveType(null);
                } else {
                    SetActiveType(new List<int> {clickedType});
                }
            } else {
                // left click is only a toggle
                _typeButtons[clickedType].Activated = !_typeButtons[clickedType].Activated;
                ApplyFilterPredicate();
            }
        }

        /// <summary>
        /// use this to programmatically uncheck any type that is not in the given list
        /// </summary>
        public void SetActiveType(List<int> allowedType) {
            this.SafeInvoke(form => {
                if (allowedType == null)
                    allowedType = new List<int>();
                foreach (var selectorButton in _typeButtons) {
                    selectorButton.Value.Activated = allowedType.IndexOf(selectorButton.Value.Type) >= 0;
                }
                ApplyFilterPredicate();
            });
        }

        /// <summary>
        /// use this to programmatically check any type that is not in the given list
        /// </summary>
        public void SetUnActiveType(List<int> allowedType) {
            this.SafeInvoke(form => {
                if (allowedType == null)
                    allowedType = new List<int>();
                foreach (var selectorButton in _typeButtons) {
                    selectorButton.Value.Activated = allowedType.IndexOf(selectorButton.Value.Type) < 0;
                }
                ApplyFilterPredicate();
            });
        }

        /// <summary>
        /// reset all the button Types to activated
        /// </summary>
        public void ResetActiveType() {
            this.SafeInvoke(form => {
                foreach (var selectorButton in _typeButtons) {
                    selectorButton.Value.Activated = true;
                }
                ApplyFilterPredicate();
            });
        }

        #endregion

        #region Misc

        /// <summary>
        /// Reset all the rows + selector buttons
        /// </summary>
        protected override void ResetButtons() {
            base.ResetButtons();

            foreach (var button in _typeButtons)
                button.Value.Dispose();
            _typeButtons.Clear();

            foreach (var type in _typeList)
                if (!_typeButtons.ContainsKey(type))
                    _typeButtons.Add(type, null);

            if (_moreButton != null) {
                _moreButton.Dispose();
                _moreButton = null;
            }
        }

        #endregion
    }

    #region SelectorButtons

    /// <summary>
    /// Button for the type selection
    /// </summary>
    public class SelectorButton : YamuiButtonImage {

        #region Fields

        private bool _activated;

        public bool Activated {
            get { return _activated; }
            set {
                _activated = value;
                UseGreyScale = !_activated;
            }
        }

        public int Type { get; set; }

        #endregion

    }

    #endregion

}
