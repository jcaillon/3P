#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiFilterBox.cs) is part of YamuiFramework.
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
using System.Linq;
using System.Windows.Forms;
using YamuiFramework.Helper;
using YamuiFramework.HtmlRenderer.WinForms;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls.YamuiList {

    /// <summary>
    /// This usercontrol displays a filter box (textbox + buttons) that should be associated
    /// to a yamuiFilteredTypeList or treeList
    /// </summary>
    public class YamuiFilterBox : UserControl {

        #region Constant

        private const string ModeButtonSearchTooltip = "The current mode is <b>Search mode</b><br>Click to <b>switch to filter mode</b>";
        private const string ModeButtonFilterTooltip = "The current mode is <b>Filter mode</b><br>Click to <b>switch to search mode</b>";
        private const string EraserButtonTooltip = "Click to <b>erase the filter</b> text";
        private const string TextBoxTooltip = "Start typing to <b>filter or search</b> the list!";
        private const string DefaultWatermarkFilterText = "Filter here!";
        private const string DefaultWatermarkSearchText = "Search here!";

        #endregion
        
        #region Properties

        /// <summary>
        /// Extra buttons to display on the left side of the filter text box
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<YamuiFilterBoxButton> ExtraButtons { get; set; }

        /// <summary>
        /// List associated with this filterbox, set it through Initialize method
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public YamuiFilteredTypeList AssociatedList { get; private set; }

        /// <summary>
        /// True to use the backColor property instead of the default one
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool UseCustomBackColor { get; set; }

        /// <summary>
        /// Text to display as a watermark on the text box when search mode is "filter"
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string WatermarkSearchText {
            get { return _watermarkSearchText; }
            set { _watermarkSearchText = value; }
        }

        /// <summary>
        /// Text to display as a watermark on the text box when search mode is "seach"
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string WatermarkFilterText {
            get { return _watermarkFilterText; }
            set { _watermarkFilterText = value; }
        }

        /// <summary>
        /// Image to use for the erase button
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image EraserButtonImage {
            get { return _eraserButtonImage; }
            set { _eraserButtonImage = value; }
        }

        /// <summary>
        /// Image to use for the search mode button
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image SearchButtonImage {
            get { return _searchButtonImage; }
            set { _searchButtonImage = value; }
        }

        /// <summary>
        /// Image to use for the filter mode button
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image FilterButtonImage {
            get { return _filterButtonImage; }
            set { _filterButtonImage = value; }
        }

        /// <summary>
        /// Accessor for the list of buttons displayed on the left of the textbox
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<YamuiButtonImage> ExtraButtonsList { get; private set; }

        /// <summary>
        /// Set the value of the filter textbox
        /// </summary>
        public override string Text {
            set {
                this.SafeInvoke(thisBox => {
                    _filterBox.Text = value;
                    ActiveControl = _filterBox;
                });
            }
        }

        #endregion
        
        #region Private

        private YamuiTextBox _filterBox = new YamuiTextBox();
        private string _watermarkSearchText = DefaultWatermarkSearchText;
        private string _watermarkFilterText = DefaultWatermarkFilterText;
        private Image _eraserButtonImage = Resources.Resources.Erase;
        private Image _searchButtonImage = Resources.Resources.Search;
        private Image _filterButtonImage = Resources.Resources.Filter;
        private HtmlToolTip _tooltip = new HtmlToolTip();

        #endregion
        
        #region Life and death

        public YamuiFilterBox() {

            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.Opaque, true);

            // this usercontrol should not be able to get the focus
            SetStyle(ControlStyles.Selectable, false);

        }

        protected override void Dispose(bool disposing) {
            if (AssociatedList != null)
                _filterBox.TextChanged -= AssociatedList.OnTextChangedEvent;
            _filterBox.KeyDown -= FilterBoxOnKeyDown;
            base.Dispose(disposing);
        }

        #endregion

        #region Paint

        protected override void OnPaint(PaintEventArgs e) {
            OnPaintBackground(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e) {
            e.Graphics.Clear(!UseCustomBackColor ? YamuiThemeManager.Current.FormBack : BackColor);
        }

        #endregion

        #region Initalize

        /// <summary>
        /// Call this method to initialize the usercontrol
        /// </summary>
        public void Initialize(YamuiFilteredTypeList list) {
            AssociatedList = list;
            this.SafeInvoke(thisBox => {
                DrawControl();

                // link events of the textbox to the list
                _filterBox.TextChanged += AssociatedList.OnTextChangedEvent;
                _filterBox.KeyDown += FilterBoxOnKeyDown;
            });
        }

        private void DrawControl() {

            Controls.Clear();

            var treeList = AssociatedList as YamuiFilteredTypeTreeList;
            var thisHeight = Height - Padding.Top - Padding.Bottom;

            // draw buttons on the left of the filter textbox
            var xLeftPos = Padding.Left;
            if (ExtraButtons != null && ExtraButtons.Count > 0) {
                ExtraButtonsList = new List<YamuiButtonImage>();
                foreach (var button in ExtraButtons.Where(button => button.Image != null)) {
                    var extraButton = new YamuiButtonImage {
                        BackGrndImage = button.Image,
                        Size = button.Image.Size,
                        Location = new Point(xLeftPos, Padding.Top + (thisHeight - button.Image.Height)/2),
                        Anchor = AnchorStyles.Top | AnchorStyles.Left,
                        TabStop = false
                    };
                    var button1 = button;
                    extraButton.ButtonPressed += (sender, args) => button1.OnClic((YamuiButtonImage) sender);
                    Controls.Add(extraButton);
                    ExtraButtonsList.Add(extraButton);
                    if (button.ToolTip != null)
                       _tooltip.SetToolTip(extraButton, button.ToolTip);
                    xLeftPos += button.Image.Width;
                }
                xLeftPos += 5;
            }

            // draw the default buttons on the right of the filter textbox
            var xRightPos = Width - Padding.Right;

            // filter mode for tree list
            if (treeList != null) {
                var img = treeList.SearchMode == YamuiFilteredTypeTreeList.SearchModeOption.SearchSortWithNoParent ? SearchButtonImage : FilterButtonImage;
                xRightPos -= img.Width;
                var modeButton = new YamuiButtonImage {
                    BackGrndImage = img,
                    Size = img.Size,
                    Location = new Point(xRightPos, Padding.Top + (thisHeight - img.Height)/2),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    TabStop = false
                };
                modeButton.ButtonPressed += OnClickMode;
                Controls.Add(modeButton);
                _tooltip.SetToolTip(modeButton, treeList.SearchMode == YamuiFilteredTypeTreeList.SearchModeOption.SearchSortWithNoParent ? ModeButtonSearchTooltip : ModeButtonFilterTooltip);
            }
            // eraser
            xRightPos -= EraserButtonImage.Width;
            var eraserButton = new YamuiButtonImage {
                BackGrndImage = EraserButtonImage,
                Size = EraserButtonImage.Size,
                Location = new Point(xRightPos, Padding.Top + (thisHeight - EraserButtonImage.Height)/2),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TabStop = false
            };
            eraserButton.ButtonPressed += OnClickEraser;
            Controls.Add(eraserButton);
            _tooltip.SetToolTip(eraserButton, EraserButtonTooltip);
            xRightPos -= 5;

            // textbox
            _filterBox.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left;
            _filterBox.Location = new Point(xLeftPos, Padding.Top);
            _filterBox.Size = new Size(xRightPos - xLeftPos, thisHeight);
            _filterBox.Text = AssociatedList.FilterString ?? string.Empty;
            _filterBox.WaterMark = treeList == null || treeList.SearchMode == YamuiFilteredTypeTreeList.SearchModeOption.SearchSortWithNoParent ? WatermarkSearchText : WatermarkFilterText;
            Controls.Add(_filterBox);
            _tooltip.SetToolTip(_filterBox, TextBoxTooltip);

        }

        #endregion

        #region Event handlers

        private void FilterBoxOnKeyDown(object sender, KeyEventArgs keyEventArgs) {
            keyEventArgs.Handled = AssociatedList.PerformKeyDown(new KeyEventArgs(keyEventArgs.KeyCode));
        }

        /// <summary>
        /// Clic on the switch mode button
        /// </summary>
        private void OnClickMode(object sender, EventArgs e) {
            var treeList = AssociatedList as YamuiFilteredTypeTreeList;
            var button = sender as YamuiButtonImage;
            if (treeList == null || button == null)
                return;

            // switch mode
            if (treeList.SearchMode == YamuiFilteredTypeTreeList.SearchModeOption.SearchSortWithNoParent) {
                button.BackGrndImage = FilterButtonImage;
                treeList.SearchMode = YamuiFilteredTypeTreeList.SearchModeOption.FilterOnlyAndIncludeParent;
                _tooltip.SetToolTip(button, ModeButtonFilterTooltip);
            } else {
                button.BackGrndImage = SearchButtonImage;
                treeList.SearchMode = YamuiFilteredTypeTreeList.SearchModeOption.SearchSortWithNoParent;
                _tooltip.SetToolTip(button, ModeButtonSearchTooltip);
            }

            ActiveControl = _filterBox;
        }

        /// <summary>
        /// Clic on the eraser button
        /// </summary>
        private void OnClickEraser(object sender, EventArgs e) {
            ClearAndFocusFilter();
        }

        #endregion

        #region Public methods

        public void ClearAndFocusFilter() {
            this.SafeInvoke(thisBox => {
                _filterBox.Text = "";
                ActiveControl = _filterBox;
            });
        }

        #endregion

        #region YamuiFilterBoxButton

        public struct YamuiFilterBoxButton {

            /// <summary>
            /// Image used for the button
            /// </summary>
            public Image Image { get; set; }

            /// <summary>
            /// Action associated to the clic on the button
            /// </summary>
            public Action<YamuiButtonImage> OnClic { get; set; }

            /// <summary>
            /// Tooltip to display for the button (can be null)
            /// </summary>
            public string ToolTip { get; set; }
        }

        #endregion

    }


}
