using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using BrightIdeasSoftware.Utilities;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Fonts;
using YamuiFramework.Helper;
using YamuiFramework.Themes;
using _3PA.Images;
using _3PA.Lib;

namespace _3PA.MainFeatures.AutoCompletion {
    public partial class AutoCompletionForm : Form {

        #region fields
        /// <summary>
        /// The filter to apply to the autocompletion form
        /// </summary>
        public string FilterByText {
            get { return _filterString; }
            set { _filterString = value; ApplyFilter(); }
        }

        /// <summary>
        /// Raised when the user clicks on a link in the html.<br/>
        /// Allows canceling the execution of the link.
        /// </summary>
        public event EventHandler<TabCompletedEventArgs> TabCompleted;

        /// <summary>
        /// Set this to the parent form handle, this gives him back the focus when needed
        /// </summary>
        public IntPtr CurrentForegroundWindow;

        /// <summary>
        /// Number of items to display
        /// </summary>
        public int NbOfItemsToDisplay = 10;

        /// <summary>
        /// self explaining (no implementeed?)
        /// </summary>
        public bool DisplayScrollBars = false;

        private Dictionary<CompletionType, SelectorButton> _activeTypes;
        private string _filterString = "";
        private int _totalItems;
        private bool _focusAllowed;
        // check the npp window rect, if it has changed from a previous state, close this form (poll every 500ms)
        private Rectangle? _nppRect;
        private Timer timer1;
        private bool _iGotActivated;
        #endregion

        #region constructor

        public AutoCompletionForm(List<CompletionData> objectsList, Point position, int lineHeight, string initialFilter) {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);

            InitializeComponent();

            // Style the control
            fastOLV.OwnerDraw = true;
            fastOLV.UseAlternatingBackColors = true;
            fastOLV.Font = FontManager.GetLabelFont(LabelFunction.AutoCompletion);
            fastOLV.BackColor = ThemeManager.Current.AutoCompletionNormalBackColor;
            fastOLV.AlternateRowBackColor = ThemeManager.Current.AutoCompletionNormalAlternateBackColor;
            fastOLV.ForeColor = ThemeManager.Current.AutoCompletionNormalForeColor;
            fastOLV.HighlightBackgroundColor = ThemeManager.Current.AutoCompletionFocusBackColor;
            fastOLV.HighlightForegroundColor = ThemeManager.Current.AutoCompletionFocusForeColor;
            fastOLV.UnfocusedHighlightBackgroundColor = fastOLV.HighlightBackgroundColor;
            fastOLV.UnfocusedHighlightForegroundColor = fastOLV.HighlightForegroundColor;

            // set the image list to use for the keywords
            var imageListOfTypes = new ImageList {
                TransparentColor = Color.Transparent,
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(20, 20)
            };
            ImagelistAdd.AddFromImage(ImageResources.Keyword, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Table, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.TempTable, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Field, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.FieldPk, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Snippet, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Function, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Procedure, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.UserVariablePrimitive, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.UserVariableOther, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Preprocessed, imageListOfTypes);
            fastOLV.SmallImageList = imageListOfTypes;
            Keyword.ImageGetter += rowObject => {
                var x = (CompletionData) rowObject;
                return (int) x.Type;
            };

            // Decorate and configure hot item
            fastOLV.UseHotItem = true;
            fastOLV.HotItemStyle = new HotItemStyle();
            fastOLV.HotItemStyle.BackColor = ThemeManager.Current.AutoCompletionHoverBackColor;
            fastOLV.HotItemStyle.ForeColor = ThemeManager.Current.AutoCompletionHoverForeColor;

            // decorate rows
            fastOLV.UseCellFormatEvents = true;
            fastOLV.FormatCell += (sender, args) => {
                var type = ((CompletionData) args.Model).Flag;
                if (type != CompletionFlag.None) {
                    // Add a opaque, rotated text decoration
                    TextDecoration decoration = new TextDecoration(Enum.GetName(typeof (CompletionFlag), type), 100);
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
            };

            // overlay of empty list :
            fastOLV.EmptyListMsg = "No suggestions!";
            TextOverlay textOverlay = fastOLV.EmptyListMsgOverlay as TextOverlay;
            textOverlay.TextColor = ThemeManager.Current.AutoCompletionNormalSubTypeForeColor;
            textOverlay.BackColor = ThemeManager.Current.AutoCompletionNormalAlternateBackColor;
            textOverlay.BorderColor = ThemeManager.Current.AutoCompletionNormalSubTypeForeColor;
            textOverlay.BorderWidth = 4.0f;
            textOverlay.Font = FontManager.GetFont(FontStyle.Bold, 30f);
            textOverlay.Rotation = -5;

            // we do the sorting, and prevent further sorting
            objectsList.Sort(new CompletionDataSortingClass());
            fastOLV.BeforeSorting += (sender, args) => { args.Canceled = true; };

            // set the height
            fastOLV.Height = 21 * NbOfItemsToDisplay;
            Height = fastOLV.Height + 32;

            // get distinct types, create a button for each
            int xPos = 4;
            _activeTypes = new Dictionary<CompletionType, SelectorButton>();
            foreach (var type in objectsList.Select(x => x.Type).Distinct()) {
                var but = new SelectorButton();
                but.BackGrndImage = imageListOfTypes.Images[(int) type];
                but.Activated = true;
                but.Size = new Size(24, 24);
                but.TabStop = false;
                but.Location = new Point(xPos, Height - 28);
                but.Type = type;
                but.ButtonPressed += (sender, args) => { HandleTypeClick(but.Type); };
                Controls.Add(but);
                _activeTypes.Add(type, but);
                xPos += but.Width;
            }
            xPos += 65;

            // label for the number of items
            _totalItems = objectsList.Count;
            nbitems.Text = _totalItems + " items";

            // set the width of the form
            using (var g = nbitems.CreateGraphics()) {
                var widthObj = objectsList.Select(x => (int) g.MeasureString(x.DisplayText, FontManager.GetLabelFont(LabelFunction.AutoCompletion)).Width).Max(x => x);
                Width = Math.Max(widthObj, xPos);
            }

            // position the window smartly
            if (position.X > Screen.PrimaryScreen.WorkingArea.X + 2*Screen.PrimaryScreen.WorkingArea.Width/3)
                position.X = position.X - Width;
            if (position.Y > Screen.PrimaryScreen.WorkingArea.Y + 3*Screen.PrimaryScreen.WorkingArea.Height/5)
                position.Y = position.Y - Height - lineHeight;
            Location = position;

            fastOLV.KeyDown += (sender, args) => args.Handled = OnKeyDown(args.KeyCode);
            fastOLV.UseTabAsInput = true;
            _filterString = initialFilter;

            fastOLV.SetObjects(objectsList);

            // timer to check if the npp window changed
            timer1 = new Timer();
            timer1.Enabled = true;
            timer1.Interval = 500;
            timer1.Tick += timer1_Tick;

            // handles mouse leave/mouse enter
            MouseLeave += CustomOnMouseLeave;
            fastOLV.MouseLeave += CustomOnMouseLeave;
            //MouseMove += CustomOnMouseLeave;
            //fastOLV.MouseMove += CustomOnMouseLeave;

            // fade out animation
            Opacity = 0d;
            Tag = false;
            Closing += (sender, args) => {
                if ((bool)Tag) return;
                args.Cancel = true;
                Tag = true;
                var t = new Transition(new TransitionType_Acceleration(200));
                t.add(this, "Opacity", 0d);
                t.TransitionCompletedEvent += (o, args1) => { Close(); };
                t.run();
            };
            // fade in animation
            Transition.run(this, "Opacity", 0.9d, new TransitionType_Acceleration(200));
        }
        #endregion

        #region events

        private void GiveFocusBack() {
            WinApi.SetForegroundWindow(CurrentForegroundWindow);
            _iGotActivated = !_iGotActivated;
            Opacity = 0.9;
        }

        protected void CustomOnMouseLeave(object sender, EventArgs e) {
            if (_iGotActivated) GiveFocusBack();
        }

        protected override void OnActivated(EventArgs e) {
            // Activate the window that previously had focus
            if (!_focusAllowed)
                GiveFocusBack();
            else {
                _iGotActivated = true;
                Opacity = 1;
            }
                
            base.OnActivated(e);
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            fastOLV.SelectedIndex = 0;
            if (!string.IsNullOrEmpty(_filterString)) ApplyFilter();
        }

        protected override void OnShown(EventArgs e) {
            _focusAllowed = true;
            base.OnShown(e);
        }

        protected virtual void OnTabCompleted(TabCompletedEventArgs e) {
            var handler = TabCompleted;
            if (handler != null) handler(this, e);
        }

        private void timer1_Tick(object sender, EventArgs e) {
            try {
                var rect = Npp.GetWindowRect();
                if (_nppRect.HasValue && _nppRect.Value != rect)
                    Close();
                _nppRect = rect;
            } catch (Exception) {
                // ignored
            }
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
                    fastOLV.SelectedIndex = (_totalItems - 1);
                fastOLV.EnsureVisible(fastOLV.SelectedIndex);
            } else if (key == Keys.Down) {
                if (fastOLV.SelectedIndex < (_totalItems - 1))
                    fastOLV.SelectedIndex++;
                else
                    fastOLV.SelectedIndex = 0;
                fastOLV.EnsureVisible(fastOLV.SelectedIndex);

                // left and right key change the selector
            //} else if (key == Keys.Left || key == Keys.Right) {

                // escape close
            } else if (key == Keys.Escape) {
                Close();

                // enter and tab accept the current selection
            } else if ((key == Keys.Enter) || (key == Keys.Tab)) {
                OnTabCompleted(new TabCompletedEventArgs(((CompletionData)fastOLV.SelectedItem.RowObject)));

                // else, any other key needs to be analysed by Npp
            } else {
                handled = false;
            }
            return handled;
        }

        #endregion

        #region Paint Methods

        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e) {
            var backColor = ThemeManager.Current.FormColorBackColor;
            var borderColor = ThemeManager.AccentColor;
            var borderWidth = 1;

            e.Graphics.Clear(backColor);

            // draw the border with Style color
            var rect = new Rectangle(new Point(0, 0), new Size(Width - borderWidth, Height - borderWidth));
            var pen = new Pen(borderColor, borderWidth);
            e.Graphics.DrawRectangle(pen, rect);
        }

        #endregion

        #region private methods

        private void ApplyFilter() {
            fastOLV.ModelFilter = new ModelFilter((o => ((CompletionData) o).DisplayText.Contains(_filterString, StringComparison.InvariantCultureIgnoreCase) && _activeTypes[((CompletionData) o).Type].Activated));
            fastOLV.DefaultRenderer = new CustomHighlightTextRenderer(fastOLV, _filterString);

            // update total items
            _totalItems = ((ArrayList) fastOLV.FilteredObjects).Count;
            nbitems.Text = _totalItems + " items";

            // if the selected row is > to number of items, then there will be a unselect
            try {
                if (fastOLV.SelectedIndex == - 1) fastOLV.SelectedIndex = 0;
                fastOLV.EnsureVisible(fastOLV.SelectedIndex);
            } catch (Exception) {
                // ignored
            }
        }

        private void HandleTypeClick(CompletionType clickedType) {
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
                return x.Ranking.CompareTo(y.Ranking);
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


    #region CustomHighlightRenderer

    /// <summary>
    /// This renderer highlights substrings that match a given text filter. 
    /// </summary>
    public class CustomHighlightTextRenderer : BaseRenderer {
        #region Life and death

        /// <summary>
        /// Create a HighlightTextRenderer
        /// </summary>
        public CustomHighlightTextRenderer() {
            FillBrush = new SolidBrush(ThemeManager.Current.AutoCompletionHighlightBack);
            FramePen = new Pen(ThemeManager.Current.AutoCompletionHighlightBorder);
        }

        /// <summary>
        /// Create a HighlightTextRenderer
        /// </summary>
        /// <param name="filter"></param>
        public CustomHighlightTextRenderer(ObjectListView fastOvl, string filterStr)
            : this() {
            Filter = new TextMatchFilter(fastOvl, filterStr, StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        #region Configuration properties

        /// <summary>
        /// Gets or set how rounded will be the corners of the text match frame
        /// </summary>
        [Category("Appearance"),
         DefaultValue(3.0f),
         Description("How rounded will be the corners of the text match frame?")]
        public float CornerRoundness {
            get { return cornerRoundness; }
            set { cornerRoundness = value; }
        }

        private float cornerRoundness = 4.0f;

        /// <summary>
        /// Gets or set the brush will be used to paint behind the matched substrings.
        /// Set this to null to not fill the frame.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Brush FillBrush {
            get { return fillBrush; }
            set { fillBrush = value; }
        }

        private Brush fillBrush;

        /// <summary>
        /// Gets or sets the filter that is filtering the ObjectListView and for
        /// which this renderer should highlight text
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TextMatchFilter Filter {
            get { return filter; }
            set { filter = value; }
        }

        private TextMatchFilter filter;

        /// <summary>
        /// Gets or set the pen will be used to frame the matched substrings.
        /// Set this to null to not draw a frame.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Pen FramePen {
            get { return framePen; }
            set { framePen = value; }
        }

        private Pen framePen;

        /// <summary>
        /// Gets or sets whether the frame around a text match will have rounded corners
        /// </summary>
        [Category("Appearance"),
         DefaultValue(true),
         Description("Will the frame around a text match will have rounded corners?")]
        public bool UseRoundedRectangle {
            get { return useRoundedRectangle; }
            set { useRoundedRectangle = value; }
        }

        private bool useRoundedRectangle = true;

        #endregion

        #region IRenderer interface overrides

        /// <summary>
        /// Handle a HitTest request after all state information has been initialized
        /// </summary>
        /// <param name="g"></param>
        /// <param name="cellBounds"></param>
        /// <param name="item"></param>
        /// <param name="subItemIndex"></param>
        /// <param name="preferredSize"> </param>
        /// <returns></returns>
        protected override Rectangle HandleGetEditRectangle(Graphics g, Rectangle cellBounds, OLVListItem item, int subItemIndex, Size preferredSize) {
            return StandardGetEditRectangle(g, cellBounds, preferredSize);
        }

        #endregion

        #region Rendering

        // This class has two implement two highlighting schemes: one for GDI, another for GDI+.
        // Naturally, GDI+ makes the task easier, but we have to provide something for GDI
        // since that it is what is normally used.

        /// <summary>
        /// Draw text using GDI
        /// </summary>
        /// <param name="g"></param>
        /// <param name="r"></param>
        /// <param name="txt"></param>
        protected override void DrawTextGdi(Graphics g, Rectangle r, string txt) {
            if (ShouldDrawHighlighting)
                DrawGdiTextHighlighting(g, r, txt);

            base.DrawTextGdi(g, r, txt);
        }

        /// <summary>
        /// Draw the highlighted text using GDI
        /// </summary>
        /// <param name="g"></param>
        /// <param name="r"></param>
        /// <param name="txt"></param>
        protected virtual void DrawGdiTextHighlighting(Graphics g, Rectangle r, string txt) {
            TextFormatFlags flags = TextFormatFlags.NoPrefix |
                                    TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsTranslateTransform;

            // TextRenderer puts horizontal padding around the strings, so we need to take
            // that into account when measuring strings
            int paddingAdjustment = 6;

            // Cache the font
            Font f = Font;

            foreach (CharacterRange range in Filter.FindAllMatchedRanges(txt)) {
                // Measure the text that comes before our substring
                Size precedingTextSize = Size.Empty;
                if (range.First > 0) {
                    string precedingText = txt.Substring(0, range.First);
                    precedingTextSize = TextRenderer.MeasureText(g, precedingText, f, r.Size, flags);
                    precedingTextSize.Width -= paddingAdjustment;
                }

                // Measure the length of our substring (may be different each time due to case differences)
                string highlightText = txt.Substring(range.First, range.Length);
                Size textToHighlightSize = TextRenderer.MeasureText(g, highlightText, f, r.Size, flags);
                textToHighlightSize.Width -= paddingAdjustment;

                float textToHighlightLeft = r.X + precedingTextSize.Width + 1;
                float textToHighlightTop = AlignVertically(r, textToHighlightSize.Height);

                // Draw a filled frame around our substring
                DrawSubstringFrame(g, textToHighlightLeft, textToHighlightTop, textToHighlightSize.Width, textToHighlightSize.Height);
            }
        }

        /// <summary>
        /// Draw an indication around the given frame that shows a text match
        /// </summary>
        /// <param name="g"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        protected virtual void DrawSubstringFrame(Graphics g, float x, float y, float width, float height) {
            if (UseRoundedRectangle) {
                using (GraphicsPath path = GetRoundedRect(x, y, width, height, 3.0f)) {
                    if (FillBrush != null)
                        g.FillPath(FillBrush, path);
                    if (FramePen != null)
                        g.DrawPath(FramePen, path);
                }
            } else {
                if (FillBrush != null)
                    g.FillRectangle(FillBrush, x, y, width, height);
                if (FramePen != null)
                    g.DrawRectangle(FramePen, x, y, width, height);
            }
        }

        /// <summary>
        /// Draw the text using GDI+
        /// </summary>
        /// <param name="g"></param>
        /// <param name="r"></param>
        /// <param name="txt"></param>
        protected override void DrawTextGdiPlus(Graphics g, Rectangle r, string txt) {
            if (ShouldDrawHighlighting)
                DrawGdiPlusTextHighlighting(g, r, txt);

            base.DrawTextGdiPlus(g, r, txt);
        }

        /// <summary>
        /// Draw the highlighted text using GDI+
        /// </summary>
        /// <param name="g"></param>
        /// <param name="r"></param>
        /// <param name="txt"></param>
        protected virtual void DrawGdiPlusTextHighlighting(Graphics g, Rectangle r, string txt) {
            // Find the substrings we want to highlight
            List<CharacterRange> ranges = new List<CharacterRange>(Filter.FindAllMatchedRanges(txt));

            if (ranges.Count == 0)
                return;

            using (StringFormat fmt = StringFormatForGdiPlus) {
                RectangleF rf = r;
                fmt.SetMeasurableCharacterRanges(ranges.ToArray());
                Region[] stringRegions = g.MeasureCharacterRanges(txt, Font, rf, fmt);

                foreach (Region region in stringRegions) {
                    RectangleF bounds = region.GetBounds(g);
                    DrawSubstringFrame(g, bounds.X - 1, bounds.Y - 1, bounds.Width + 2, bounds.Height);
                }
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets whether the renderer should actually draw highlighting
        /// </summary>
        protected bool ShouldDrawHighlighting {
            get { return Column == null || (Column.Searchable && Filter != null && Filter.HasComponents); }
        }

        /// <summary>
        /// Return a GraphicPath that is a round cornered rectangle
        /// </summary>
        /// <returns>A round cornered rectagle path</returns>
        /// <remarks>If I could rely on people using C# 3.0+, this should be
        /// an extension method of GraphicsPath.</remarks>        
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="diameter"></param>
        protected GraphicsPath GetRoundedRect(float x, float y, float width, float height, float diameter) {
            return GetRoundedRect(new RectangleF(x, y, width, height), diameter);
        }

        /// <summary>
        /// Return a GraphicPath that is a round cornered rectangle
        /// </summary>
        /// <param name="rect">The rectangle</param>
        /// <param name="diameter">The diameter of the corners</param>
        /// <returns>A round cornered rectagle path</returns>
        /// <remarks>If I could rely on people using C# 3.0+, this should be
        /// an extension method of GraphicsPath.</remarks>
        protected GraphicsPath GetRoundedRect(RectangleF rect, float diameter) {
            GraphicsPath path = new GraphicsPath();

            if (diameter > 0) {
                RectangleF arc = new RectangleF(rect.X, rect.Y, diameter, diameter);
                path.AddArc(arc, 180, 90);
                arc.X = rect.Right - diameter;
                path.AddArc(arc, 270, 90);
                arc.Y = rect.Bottom - diameter;
                path.AddArc(arc, 0, 90);
                arc.X = rect.Left;
                path.AddArc(arc, 90, 90);
                path.CloseFigure();
            } else {
                path.AddRectangle(rect);
            }

            return path;
        }

        #endregion
    }

    #endregion

}
