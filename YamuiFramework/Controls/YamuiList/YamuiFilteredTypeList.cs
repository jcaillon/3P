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
        
        #region public properties

        /// <summary>
        /// Should return the image to use for the corresponding item
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<FilteredTypeItem, Image> GetObjectImage { get; set; }

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
        /// The padding to apply to display the list
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Padding ListPadding {
            get {
                var pad = base.ListPadding;
                pad.Bottom = pad.Bottom.ClampMin(BottomPadding);
                return pad;
            }
            set { base.ListPadding = value; }
        }

        /// <summary>
        /// Space reserved to write the showing x items label
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BottomPadding {
            get { return _bottomPadding; }
            set { _bottomPadding = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float SubTextOpacity {
            get { return _subTextOpacity; }
            set { _subTextOpacity = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float TagImagesOpacity {
            get { return _tagImagesOpacity; }
            set { _tagImagesOpacity = value; }
        }

        #endregion

        #region private fields

        protected const TextFormatFlags TextRightFlags = TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.Right | TextFormatFlags.NoPadding;

        private int _bottomPadding = 30;

        private int _itemsNbLabelWidth = 45;

        private float _subTextOpacity = 0.3f;

        private float _tagImagesOpacity = 0.5f;

        #endregion

        #region Draw list

        /// <summary>
        /// Constructor to initialize stuff
        /// </summary>
        public YamuiFilteredTypeList() {
        }

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
                _itemsNbLabelWidth = TextRenderer.MeasureText(g, _nbItems + " items", FontManager.GetFont(FontFunction.Small), ClientSize, TextRightFlags).Width.ClampMin(45);
            }

            base.SetItems(listItems);
        }

        protected override void OnPaintBackground(PaintEventArgs e) {
            e.Graphics.Clear(!UseCustomBackColor ? YamuiThemeManager.Current.MenuNormalBack : BackColor);

            const int rightPadding = 3;

            // text
            var textHeight = (BottomPadding - 4)/2;
            TextRenderer.DrawText(e.Graphics, "Showing", FontManager.GetFont(FontFunction.Small), new Rectangle(Width - rightPadding - _itemsNbLabelWidth, Height - BottomPadding + 2, _itemsNbLabelWidth, textHeight), YamuiThemeManager.Current.MenuNormalFore, TextRightFlags);
            TextRenderer.DrawText(e.Graphics, _nbItems + " items", FontManager.GetFont(FontFunction.Small), new Rectangle(Width - rightPadding - _itemsNbLabelWidth, Height - textHeight - 2, _itemsNbLabelWidth, textHeight), YamuiThemeManager.Current.MenuNormalFore, TextRightFlags);
           
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
                if (GetObjectImage != null)
                    img = GetObjectImage(curItem);

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
                        imgColor.Matrix33 = TagImagesOpacity;
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

    }

}
