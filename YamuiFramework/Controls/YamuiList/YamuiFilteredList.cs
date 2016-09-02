using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using YamuiFramework.Fonts;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls.YamuiList {

    public class YamuiFilteredList : YamuiScrollList {

        #region public properties

        /// <summary>
        /// Set this to filter the list with the given text
        /// </summary>
        public string FilterString {
            set {
                _filterString = value.ToLower().Trim();
                
                if (_initialItems != null && _nbInitialItems > 0) {

                    // apply the filter on each item to compute internal properties
                    _initialItems.ForEach(data => data.FilterApply(_filterString));

                    if (!string.IsNullOrEmpty(_filterString)) {

                        // apply filter + sort
                        if (FilterPredicate != null)
                            _items = _initialItems.Where(item => item.FilterFullyMatch && FilterPredicate(item)).OrderBy(data => data.FilterDispertionLevel).Cast<ListItem>().ToList(); 
                        else
                            _items = _initialItems.Where(item => item.FilterFullyMatch).OrderBy(data => data.FilterDispertionLevel).Cast<ListItem>().ToList();

                    } else {
                        _items = _initialItems.Cast<ListItem>().ToList();
                    }

                    // base setItems
                    SetItems(_items);
                }
            }
        }
        
        /// <summary>
        /// Action that will be called each time a row needs to be painted
        /// </summary>
        public new Action<ListItem, YamuiListRow, PaintEventArgs> OnRowPaint {
            get { return _newOnRowPaint ?? RowPaintFilter; }
            set { _newOnRowPaint = value; }
        }

        /// <summary>
        /// Predicate to filter the items, only items meeting the predicate requirements will be displayed (applied in addition to the default string filter)
        /// </summary>
        public Predicate<FilteredItem> FilterPredicate {
            get { return _filterPredicate; }
            set { _filterPredicate = value; }
        }

        #endregion

        #region private fields

        protected Action<ListItem, YamuiListRow, PaintEventArgs> _newOnRowPaint;

        protected Predicate<FilteredItem> _filterPredicate;

        protected List<FilteredItem> _initialItems;

        protected int _nbInitialItems;

        protected new int _rowHeight = 22;

        protected string _filterString;

        private Brush _fillBrush;

        private Pen _framePen;
        
        const TextFormatFlags TextFlags = TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding;

        #endregion

        #region Draw list

        /// <summary>
        /// Constructor to initialize stuff
        /// </summary>
        public YamuiFilteredList() {
            _fillBrush = new SolidBrush(YamuiThemeManager.Current.AutoCompletionHighlightBack);
            _framePen = new Pen(YamuiThemeManager.Current.AutoCompletionHighlightBorder);

            // "override" base OnRowPaint
            base.OnRowPaint = OnRowPaint;
        }

        /// <summary>
        /// Set the items that will be displayed in the list
        /// </summary>
        public void SetItems(List<FilteredItem> listItems) {
            _initialItems = listItems;
            _nbInitialItems = _initialItems.Count;
            
            // base setItems
            SetItems(_initialItems.Cast<ListItem>().ToList());
        }

        /// <summary>
        /// Called by default to paint the row if no OnRowPaint is defined
        /// </summary>
        private void RowPaintFilter(ListItem item, YamuiListRow row, PaintEventArgs e) {
            var backColor = YamuiThemeManager.Current.MenuBg(row.IsSelected, row.IsHovered, !item.IsDisabled);
            var foreColor = YamuiThemeManager.Current.MenuFg(row.IsSelected, row.IsHovered, !item.IsDisabled);

            // background
            e.Graphics.Clear(backColor);

            // foreground
            // left line
            if (row.IsSelected && !item.IsDisabled) {
                using (SolidBrush b = new SolidBrush(YamuiThemeManager.Current.AccentColor)) {
                    e.Graphics.FillRectangle(b, new Rectangle(0, 0, 3, row.ClientRectangle.Height));
                }
            }

            var textRectangle = new Rectangle(7, 0, row.ClientRectangle.Width - 7, RowHeight);

            // letter highlight
            if (!item.IsDisabled)
                DrawGdiTextHighlighting(e.Graphics, ((FilteredItem) item).FilterMatchedRanges, textRectangle, item.DisplayText, TextFlags);

            // text
            TextRenderer.DrawText(e.Graphics, item.DisplayText, FontManager.GetStandardFont(), textRectangle, foreColor, TextFlags);
        }

        #region Letter(s) highlighting

        /// <summary>
        /// Draw a highlight patch for each matched ranges
        /// </summary>
        protected void DrawGdiTextHighlighting(Graphics g, List<CharacterRange> filterMatchedRanges, Rectangle r, string txt, TextFormatFlags flags) {
            if (filterMatchedRanges != null) {
                foreach (CharacterRange range in filterMatchedRanges) {
                    // Measure the text that comes before our substring
                    Size precedingTextSize = Size.Empty;
                    if (range.First > 0) {
                        string precedingText = txt.Substring(0, range.First);
                        precedingTextSize = TextRenderer.MeasureText(g, precedingText, FontManager.GetStandardFont(), r.Size, flags);
                    }

                    // Measure the length of our substring (may be different each time due to case differences)
                    string highlightText = txt.Substring(range.First, range.Length);
                    Size textToHighlightSize = TextRenderer.MeasureText(g, highlightText, FontManager.GetStandardFont(), r.Size, flags);
                    float textToHighlightLeft = r.X + precedingTextSize.Width;
                    float textToHighlightTop = r.Top + ((r.Height - textToHighlightSize.Height) / 2);

                    // Draw a filled frame around our substring
                    DrawSubstringFrame(g, textToHighlightLeft, textToHighlightTop, textToHighlightSize.Width, textToHighlightSize.Height);
                }
            }
        }

        /// <summary>
        /// Draw an indication around the given letter(s) that shows a text match
        /// </summary>
        protected void DrawSubstringFrame(Graphics g, float x, float y, float width, float height, bool useRoundedRectangle = true) {
            if (useRoundedRectangle) {
                using (GraphicsPath path = Utilities.GetRoundedRect(x, y, width, height, 3.0f)) {
                    if (_fillBrush != null)
                        g.FillPath(_fillBrush, path);
                    if (_framePen != null)
                        g.DrawPath(_framePen, path);
                }
            } else {
                if (_fillBrush != null)
                    g.FillRectangle(_fillBrush, x, y, width, height);
                if (_framePen != null)
                    g.DrawRectangle(_framePen, x, y, width, height);
            }
        }

        #endregion

        #endregion

        #region Utilities

        /// <summary>
        /// Associate the TextChanged event of a text box to this method to filter this list with the input text of the textbox
        /// </summary>
        public void OnTextChangedEvent(object sender, EventArgs eventArgs) {
            var textBox = sender as TextBox;
            if (textBox != null)
                FilterString = textBox.Text;
        }

        #endregion

    }
}
