using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using YamuiFramework.Fonts;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls.YamuiList {

    public class YamuiFilteredTypeList : YamuiFilteredList {

        #region public properties

        /// <summary>
        /// Action that will be called each time a row needs to be painted
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<FilteredItem, int> GetObjectType { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<FilteredItem, Image> GetObjectImage { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<FilteredItem, string> GetObjectSubText { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<FilteredItem, List<Image>> GetObjectTagImages { get; set; }

        /// <summary>
        /// The padding to apply to display the list
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Padding ListPadding {
            get { return _listPadding; }
            set { base.ListPadding = value; }
        }

        #endregion

        #region private fields

        protected new Padding _listPadding = new Padding(0, 0, 0, 30);

        protected const TextFormatFlags TextRightFlags = TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.Right | TextFormatFlags.NoPadding;

        #endregion

        #region Draw list

        /// <summary>
        /// Constructor to initialize stuff
        /// </summary>
        public YamuiFilteredTypeList() {
            ListPadding = ListPadding;
        }

        protected override void OnPaintBackground(PaintEventArgs e) {
            e.Graphics.Clear(!UseCustomBackColor ? YamuiThemeManager.Current.MenuNormalBack : BackColor);

            var maxWidth = TextRenderer.MeasureText(e.Graphics, _nbItems + " items", FontManager.GetFont(FontFunction.Small), ClientSize, TextRightFlags).Width.ClampMin(45);

            // text
            TextRenderer.DrawText(e.Graphics, "Showing", FontManager.GetFont(FontFunction.Small), new Rectangle(Width - maxWidth, Height - 28, maxWidth, 15), YamuiThemeManager.Current.MenuNormalFore, TextRightFlags);
            TextRenderer.DrawText(e.Graphics, _nbItems + " items", FontManager.GetFont(FontFunction.Small), new Rectangle(Width - maxWidth, Height - 15, maxWidth, 15), YamuiThemeManager.Current.MenuNormalFore, TextRightFlags);
        }

        #endregion

    }

}
