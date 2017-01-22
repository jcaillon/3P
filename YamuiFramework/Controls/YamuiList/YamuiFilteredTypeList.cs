#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiFilteredTypeList.cs) is part of YamuiFramework.
// 
// YamuiFramework is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// YamuiFramework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with YamuiFramework. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
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
using YamuiFramework.HtmlRenderer.WinForms;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls.YamuiList {

    /// <summary>
    /// Display a filterable list in which each item as a type
    /// </summary>
    public class YamuiFilteredTypeList : YamuiFilteredList {

        #region constants

        public const string TypeButtonTooltipText = @"Left click to <b>filter on/off</b> this type<br>Right click to <b>filter for this type only</b><br><i>(A consecutive right click reactivate all the types)</i><br><i>You can use <b>ALT+RIGHT/LEFT ARROW</b> key to quickly activate one type</i>";

        protected const string MoreButtonTooltipText = @"Click to show more item types";

        protected const string PaintShowingText = @"Showing";
        protected const string PaintItemsText = @" items";

        protected const int DefaultBottomHeight = 28;

        protected const int MinItemLabelWidth = 45;

        protected const float DefaultSubTextOpacity = 0.3f;

        protected const float DefaultFlagImagesOpacity = 0.5f;

        protected const int BottomPadding = 2;

        public const int TypeButtonWidth = 22;

        #endregion

        #region private fields

        protected HtmlToolTip _tooltip = new HtmlToolTip();

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

        private MoreTypesForm _moreForm;

        #endregion

        #region public properties

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
        /// Space reserved to write the showing x items label,
        /// set it to 0 to not display the bottom of the list
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
        public override Predicate<FilteredListItem> FilterPredicate {
            get {
                // add a layer of filter which is a type filter
                return item => {
                    var typeItem = item as FilteredTypeListItem;
                    if (typeItem != null) {
                        return (!_typeButtons.ContainsKey(typeItem.ItemType) || _typeButtons[typeItem.ItemType] == null || _typeButtons[typeItem.ItemType].Activated) && 
                            (_filterPredicate == null || _filterPredicate(item));
                    }
                    return false;
                };
            }
            set { _filterPredicate = value; }
        }

        /// <summary>
        /// Stores a correspondance between type number and image to use for the button
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Dictionary<int, Image> TypeImages { get; private set; }

        /// <summary>
        /// Stores a correspondance between type number and text to use for the tooltip of the button
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Dictionary<int, string> TypeText { get; private set; }

        #endregion

        #region Life

        public YamuiFilteredTypeList() {
            _tooltip.ShowAlways = true;
        }

        #endregion
        
        #region Paint

        protected override void OnPaintBackground(PaintEventArgs e) {
            e.Graphics.Clear(!UseCustomBackColor ? YamuiThemeManager.Current.MenuNormalBack : BackColor);

            // text
            if (BottomHeight > 0) {
                var textHeight = (BottomHeight - BottomPadding*2)/2;
                TextRenderer.DrawText(e.Graphics, PaintShowingText, FontManager.GetFont(FontFunction.Small), new Rectangle(Width - BottomPadding - _itemsNbLabelWidth, Height - BottomHeight + BottomPadding, _itemsNbLabelWidth, textHeight), YamuiThemeManager.Current.MenuNormalFore, TextRightFlags);
                TextRenderer.DrawText(e.Graphics, _nbItems + PaintItemsText, FontManager.GetFont(FontFunction.Small), new Rectangle(Width - BottomPadding - _itemsNbLabelWidth, Height - textHeight - BottomPadding, _itemsNbLabelWidth, textHeight), YamuiThemeManager.Current.MenuNormalFore, TextRightFlags);
            }
        }

        #endregion
        
        #region Set

        /// <summary>
        /// Set the items that will be displayed in the list
        /// </summary>
        public override void SetItems(List<ListItem> listItems) {

            var firstItem = listItems.FirstOrDefault();
            if (firstItem != null && !(firstItem is FilteredTypeListItem))
                throw new Exception("listItems shoud contain objects of type FilteredTypeItem");

            // measure the space taken by the label "showing x items"
            using (var gImg = new Bitmap(1, 1))
            using (var g = Graphics.FromImage(gImg)) {
                _itemsNbLabelWidth = TextRenderer.MeasureText(g, _nbItems + PaintItemsText, FontManager.GetFont(FontFunction.Small), ClientSize, TextRightFlags).Width.ClampMin(MinItemLabelWidth);
            }

            // set the type buttons needed
            ComputeTypeButtonsNeeded(listItems);

            base.SetItems(listItems);
        }

        protected void ComputeTypeButtonsNeeded(List<ListItem> listItems) {
            // set the type buttons needed
            if (TypeImages == null)
                TypeImages = new Dictionary<int, Image>();
            else
                TypeImages.Clear();
            if (TypeText == null)
                TypeText = new Dictionary<int, string>();
            else
                TypeText.Clear();
            _typeList.Clear();
            foreach (var item in listItems.Cast<FilteredTypeListItem>().Where(item => item.ItemType >= 0)) {
                if (!TypeImages.ContainsKey(item.ItemType)) {
                    _typeList.Add(item.ItemType);
                    TypeImages.Add(item.ItemType, item.ItemTypeImage);
                    TypeText.Add(item.ItemType, item.ItemTypeText);
                }
            }            
        }

        #endregion

        #region DrawButtons

        /// <summary>
        /// Overring DrawButtons to add the Type selector buttons
        /// </summary>
        protected override void DrawButtons() {
            base.DrawButtons();

            if (BottomHeight == 0)
                return;

            int maxWidthForTypeButtons = Width - BottomPadding * 3 - _itemsNbLabelWidth - 10;
            _nbDisplayableTypeButton = (int) Math.Floor((decimal) maxWidthForTypeButtons / TypeButtonWidth);
            _nbDisplayableTypeButton = _nbDisplayableTypeButton.ClampMax(_typeList.Count);
            var buttonsToDisplay = _typeList.Count;

            // for each distinct type of items, create the buttons
            int xPos = BottomPadding;
            int nBut = 0;
            foreach (var type in _typeList) {

                // new type, add a button for it
                if (!_typeButtons.ContainsKey(type)) {
                    _typeButtons.Add(type, new SelectorButton {
                        Size = new Size(TypeButtonWidth, DefaultBottomHeight),
                        TabStop = false,
                        AcceptsRightClick = true,
                        HideFocusedIndicator = true,
                        Activated = true,
                        BackGrndImage = TypeImages.ContainsKey(type) ? TypeImages[type] : null
                    });
                    _typeButtons[type].ButtonPressed += HandleTypeClick;
                    _tooltip.SetToolTip(_typeButtons[type], (TypeText.ContainsKey(type) && TypeText[type] != null ? TypeText[type] + "<br>" : "") + TypeButtonTooltipText);
                }

                _typeButtons[type].Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
                _typeButtons[type].Type = type;
                _typeButtons[type].Activated = _typeButtons[type].Activated;
                _typeButtons[type].Location = new Point(xPos, Height - BottomHeight / 2 - _typeButtons[type].Height / 2);
                nBut++;

                // show as many button as we can show - 1
                if (nBut < _nbDisplayableTypeButton || buttonsToDisplay == _nbDisplayableTypeButton) {
                    xPos += TypeButtonWidth;
                    if (!Controls.Contains(_typeButtons[type]))
                        Controls.Add(_typeButtons[type]);
                } else {
                    if (Controls.Contains(_typeButtons[type]))
                        Controls.Remove(_typeButtons[type]);
                }
            }
            
            // remove buttons that are no longer used
            var tmpDic = new Dictionary<int, SelectorButton>();
            foreach (int key in _typeButtons.Keys) {
                if (!_typeList.Contains(key)) {
                    if (Controls.Contains(_typeButtons[key]))
                        Controls.Remove(_typeButtons[key]);
                } else {
                    tmpDic.Add(key, _typeButtons[key]);
                }
            }
            _typeButtons = tmpDic;

            if (nBut > 0) { 

                // if we have enough space to display the last button, hide the more button
                if (buttonsToDisplay == _nbDisplayableTypeButton) {

                    // remove the more button if it exists
                    if (_moreButton != null) {
                        if (Controls.Contains(_moreButton))
                            Controls.Remove(_moreButton);
                    }
                } else {
                    // otherwise, we display a "more button" that opens a small interface to show the extra buttons
                    if (_moreButton == null) { 
                        _moreButton = new SelectorButton {
                            BackGrndImage = MoreTypesImage ?? Resources.Resources.More,
                            Activated = true,
                            Size = new Size(TypeButtonWidth, DefaultBottomHeight),
                            TabStop = false,
                            Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
                            HideFocusedIndicator = true
                        };
                        _moreButton.ButtonPressed += HandleMoreTypeClick;
                        _tooltip.SetToolTip(_moreButton, MoreButtonTooltipText);
                    }

                    _moreButton.Location = new Point(xPos, Height - BottomHeight / 2 - _moreButton.Height / 2);
                    if (!Controls.Contains(_moreButton))
                        Controls.Add(_moreButton);
                }
            }
        }

        #endregion

        #region Draw list

        /// <summary>
        /// Called by default to paint the row if no OnRowPaint is defined
        /// </summary>
        protected override void RowPaint(ListItem item, YamuiListRow row, PaintEventArgs e) {
            
            // background
            var backColor = YamuiThemeManager.Current.MenuBg(row.IsSelected, row.IsHovered, !item.IsDisabled);
            e.Graphics.Clear(backColor);

            var curItem = item as FilteredTypeListItem;
            if (curItem != null) {
                var drawRect = row.ClientRectangle;
                drawRect.Height = RowHeight;

                // case of a separator
                if (item.IsSeparator)
                    RowPaintSeparator(e.Graphics, drawRect);
                else
                    DrawFilteredTypeRow(e.Graphics, curItem, drawRect, row);
            }
        }

        protected virtual void DrawFilteredTypeRow(Graphics g, FilteredTypeListItem item, Rectangle drawRect, YamuiListRow row) {

            var foreColor = YamuiThemeManager.Current.MenuFg(row.IsSelected, row.IsHovered, !item.IsDisabled);

            // Highlighted row
            if (item.IsRowHighlighted) {
                using (SolidBrush b = new SolidBrush(YamuiThemeManager.Current.ButtonImageFocusedIndicator)) {
                    GraphicsPath path = new GraphicsPath();
                    path.AddLines(new[] { new Point(drawRect.X, drawRect.Y), new Point(drawRect.X + drawRect.Height / 2, drawRect.Y), new Point(drawRect.X, drawRect.Y + drawRect.Height / 2), new Point(drawRect.X, drawRect.Y) });
                    g.FillPath(b, path);
                }
            }

            // Image icon
            Image img = item.ItemImage;
            if (img == null && item.ItemTypeImage != null)
                img = item.ItemTypeImage;
            if (img != null) {
                var recImg = new Rectangle(new Point(drawRect.X + 1, drawRect.Y + (drawRect.Height - img.Height) / 2), new Size(img.Width, img.Height));
                g.DrawImage(img, recImg);
            }

            // tag images
            var xPos = 1;

            var listImg = item.TagImages;
            if (listImg != null) {
                listImg.Reverse();

                // draw the image with a given opacity
                ColorMatrix imgColor = new ColorMatrix();
                imgColor.Matrix33 = FlagImagesOpacity;
                using (ImageAttributes imgAttrib = new ImageAttributes()) {
                    imgAttrib.SetColorMatrix(imgColor);

                    foreach (var image in listImg) {
                        xPos += image.Width;
                        g.DrawImage(image, new Rectangle(new Point(drawRect.X + drawRect.Width - xPos, (drawRect.Height - image.Height) / 2), new Size(image.Width, image.Height)), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imgAttrib);
                    }
                }
            }

            // sub text 
            var subText = item.SubText;
            if (!string.IsNullOrEmpty(subText)) {
                var textFont = FontManager.GetFont(FontStyle.Bold, 10);
                var textSize = TextRenderer.MeasureText(subText, textFont);
                var subColor = !item.IsDisabled ? YamuiThemeManager.Current.SubTextFore : foreColor;

                var drawPoint = new PointF(drawRect.X + drawRect.Width - xPos - textSize.Width - 3, (drawRect.Height / 2) - (textSize.Height / 2) - 1);
                // using Drawstring here because TextRender (GDI) can't draw semi transparent text
                g.DrawString(subText, textFont, new SolidBrush(Color.FromArgb((int)(SubTextOpacity * 255), subColor)), drawPoint);

                using (var pen = new Pen(Color.FromArgb((int)(SubTextOpacity * 0.8 * 255), subColor), 1) { Alignment = PenAlignment.Left }) {
                    g.DrawPath(pen, Utilities.GetRoundedRect(drawPoint.X - 2, drawPoint.Y - 1, textSize.Width + 2, textSize.Height + 3, 3f));
                }
            }

            var textRectangle = new Rectangle(drawRect.X + 3 + (img != null ? img.Width : 0), 0, drawRect.Width - 3 - (img != null ? img.Width : 0), drawRect.Height);

            // letter highlight
            if (!item.IsDisabled)
                DrawTextHighlighting(g, item.InternalFilterMatchedRanges, textRectangle, item.DisplayText, TextFlags);

            // text
            TextRenderer.DrawText(g, item.DisplayText, FontManager.GetStandardFont(), textRectangle, foreColor, TextFlags);
        }

        #endregion

        #region HandleKeyDown

        /// <summary>
        /// Called when a key is pressed
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e) {
            switch (e.KeyCode) {
                case Keys.Left:
                    if (ModifierKeys.HasFlag(Keys.Alt))
                        e.Handled = LeftRight(true);
                    break;

                case Keys.Right:
                    if (ModifierKeys.HasFlag(Keys.Alt))
                        e.Handled = LeftRight(false);
                    break;
            }
            if (!e.Handled)
                base.OnKeyDown(e);
        }

        /// <summary>
        /// Handles the left/right buttons
        /// </summary>
        protected bool LeftRight(bool isLeft) {
            if (_typeButtons.Count <= 0)
                return false;

            // only 1 type is active
            if (_typeButtons.Count(b => b.Value.Activated) == 1) {
                _currentButtonIndex = 0;
                foreach (var type in _typeList) {
                    if (_typeButtons.ContainsKey(type) && _typeButtons[type].Activated)
                        break;
                    _currentButtonIndex++;
                }
                _currentButtonIndex = _currentButtonIndex + (isLeft ? -1 : 1);
            }

            if (_currentButtonIndex > _typeButtons.Count - 1) _currentButtonIndex = 0;
            if (_currentButtonIndex < 0) _currentButtonIndex = _typeButtons.Count - 1;
            SetActiveType(new List<int> { _typeButtons.ElementAt(_currentButtonIndex).Key });

            return true;
        }

        #endregion

        #region More types

        /// <summary>
        /// Handles the click on the "more" button
        /// </summary>
        private void HandleMoreTypeClick(object sender, EventArgs args) {
            // dispose of an existing form
            CloseMoreForm();

            // list of the types to display on the form
            List<int> typesSubList = new List<int>();
            int nBut = 0;
            foreach (var type in _typeList) {
                if (nBut >= _nbDisplayableTypeButton - 1)
                    typesSubList.Add(type);
                nBut++;
            }

            _moreForm = new MoreTypesForm();
            _moreForm.Build(MousePosition, typesSubList, HandleTypeClick, this);
            _moreForm.Show();
        }

        private void CloseMoreForm() {
            // dispose of an existing form
            if (_moreForm != null) {
                if (_moreForm.Visible)
                    _moreForm.Close();
                _moreForm.Dispose();
            }
        }

        #endregion
        
        #region Active types

        /// <summary>
        /// Returns true if the given type is activated
        /// </summary>
        public bool IsTypeActivated(int type) {
            return _typeButtons.ContainsKey(type) && _typeButtons[type].Activated;
        }

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
                    SetUnactiveType(null);
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
        public void SetUnactiveType(List<int> notAllowedType) {
            this.SafeInvoke(form => {
                if (notAllowedType == null)
                    notAllowedType = new List<int>();
                foreach (var selectorButton in _typeButtons) {
                    selectorButton.Value.Activated = notAllowedType.IndexOf(selectorButton.Value.Type) < 0;
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
