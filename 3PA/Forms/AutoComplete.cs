using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using _3PA.Images;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures;

namespace _3PA.Forms {

    public partial class AutoComplete : Form {
        public bool DisplayFromShortcut { get; set; }
        private static Dictionary<int, bool> _selectorDataRem;
        private string _currentTable;
        private bool _displayWholeList; // was the form displayed from a shortcut, or is the option to show full list active?
        private int _imgSize = 20;
        private int _itemHeight;
        private bool _mouseIsInForm;
        // check the npp window rect, if it has changed from a previous state, close this form (poll every 500ms)
        private Rectangle? _nppRect;
        private Action<CompletionData> _onAutocompletionAccepted;
        private List<CompletionData> _rawItems;
        private int _scrollBarSize = 20;
        // tuple containing the data for the selector (bool activated, Image, rect position)
        private Dictionary<int, SelectorItem> _selectorData;
        private Timer _timer;
        private int _verticalSpacing = 4;
        private double _unfocusOpacity = 0.9;
        private double _focusOpacity = 1.0;
        private double _timerOpacityStep;
        private double _timerOpacityFinal;
        private int _timerHeightStep;
        private int _timerHeightFinal;
        private int _timerWidthStep;
        private int _timerWidthFinal;
        private int _curSelectorPos;
        private string _lastKeyword;
        private bool _isFormActivated;
        private int _extraHeight; // space for the selector if it is present

        //Very important to keep it. It prevents the form from stealing the focus
        protected override bool ShowWithoutActivation {
            get { return true; }
        }

        public AutoComplete(Action<CompletionData> onAutocompletionAccepted, List<CompletionData> items, bool displayFromShortCut) {
            InitializeComponent();
            _onAutocompletionAccepted = onAutocompletionAccepted;
            _rawItems = items;
            DisplayFromShortcut = displayFromShortCut;
            _displayWholeList = displayFromShortCut || Lib.Config.Instance.AutoCompleteShowCompleteListOnKeyInput;

            _selectorData = new Dictionary<int, SelectorItem>();
            _selectorDataRem = new Dictionary<int, bool>();

            if (displayFromShortCut) Cursor.Current = Cursors.WaitCursor;

            // register to npp
            FormIntegration.RegisterToNpp(Handle);
        }

        // on load
        private void AutocompleteForm_Load(object sender, EventArgs e) {
            var g = listBox1.CreateGraphics();
            _itemHeight = (int)g.MeasureString("T", listBox1.Font).Height;

            listBox1.Sorted = false;
            listBox1.DrawMode = DrawMode.OwnerDrawVariable;
            listBox1.DrawItem += listBox1_DrawItem;
            listBox1.MeasureItem += listBox1_MeasureItem;
            listBox1.HorizontalScrollbar = false;

            Height = 0; // hide the form until the listbox is fully loaded!
            Width = 20;
            Opacity = 0;
            listBox1.Height = 0;

            Capture = true;
            MouseDown += AutocompleteForm_MouseDown;
            FormClosing += Autocomplete_FormClosing;

            listBox1.MouseEnter += Autocomplete_MouseEnter;
            listBox1.MouseLeave += Autocomplete_MouseLeave;
            listBox1.MouseMove += Autocomplete_Activated;
            listBox1.MouseDown += AutocompleteForm_MouseDown;

            pictureBox1.MouseEnter += Autocomplete_MouseEnter;
            pictureBox1.MouseLeave += Autocomplete_MouseLeave;
            pictureBox1.MouseMove += Autocomplete_Activated;
            pictureBox1.MouseDown += PictureBox1OnMouseClick;
            pictureBox1.Paint += InitializeSelector;

            pictureBox1.MouseMove += (o, args) => {
                Point pos = new Point(args.X, args.Y);
                foreach (var entry in _selectorData) {
                    if (PointIsInRectangle(pos, entry.Value.Position)) {
                        //toolTi.Show(entry.Key.ToString(), this, new Point(args.X + 20, Height));
                        //toolTi.ReshowDelay = 1000;
                    }
                }
            };
            timer1.Enabled = true;
        }
        

        #region " Selector "

        /// Called when the user click on the selector bar
        private void PictureBox1OnMouseClick(object sender, MouseEventArgs mouseEventArgs) {
            if ((mouseEventArgs.Button & MouseButtons.Left) == MouseButtons.Left) {
                int cnt = 0;
                int clickedOn = -1;
                Point mousePos = new Point(mouseEventArgs.X, mouseEventArgs.Y);
                foreach (var entry in _selectorData) {
                    if (PointIsInRectangle(mousePos, entry.Value.Position)) {
                        // we clicked one of the selector's image!
                        entry.Value.Active = !entry.Value.Active;
                        clickedOn = cnt;
                    }
                    cnt++;
                }
                if (clickedOn == 0) {
                    foreach (var entry in _selectorData) {
                        entry.Value.Active = _selectorData[0].Active;
                    }
                }
                if (clickedOn > -1) {
                    RememberSelector();
                    FilterFor();
                }

            }
        }

        private bool PointIsInRectangle(Point point, Rectangle rect) {
            return rect.X <= point.X && point.X <= rect.X + rect.Width && rect.Y <= point.Y && point.Y <= rect.Y + rect.Height;
        }

        /// Remember which selector is activated
        private void RememberSelector() {
            foreach (var entry in _selectorData) {
                if (_selectorDataRem.ContainsKey(entry.Key))
                    _selectorDataRem[entry.Key] = entry.Value.Active;
                else
                    _selectorDataRem.Add(entry.Key, entry.Value.Active);
            }
        }

        /// Initialize the selector bar entirely
        private void InitializeSelector(object sender, PaintEventArgs e) {
            e.Graphics.FillRectangle(new SolidBrush(Color.White), e.Graphics.ClipBounds);
            foreach (var entry in _selectorData) {
                if (_selectorDataRem.ContainsKey(entry.Key))
                    _selectorDataRem[entry.Key] = entry.Value.Active;
                else
                    _selectorDataRem.Add(entry.Key, entry.Value.Active);
                PaintSelectorImage(e.Graphics, entry.Key);
            }
            
        }

        ///  Paint one item of the selector bar
        private void PaintSelectorImage(Graphics zoneGraph, int iToPaint) {
            var img = _selectorData[iToPaint].Img;

            if (!_selectorData[iToPaint].Active)
                img = Utils.MakeGrayscale3(new Bitmap(img, new Size(_imgSize, _imgSize)));

            zoneGraph.DrawImage(img, _selectorData[iToPaint].Position);

            if (!_selectorData[iToPaint].Active) {
                var rec = _selectorData[iToPaint].Position;
                rec.Offset(2, 2);
                rec.Width = 16;
                rec.Height = 16;
                zoneGraph.DrawImage(ImageResources.autocompletion_cancel, rec);
            }
        }
        #endregion

        #region "misc events"


        /// When the mouse hover the form when the form has been activated by a click on it
        private void Autocomplete_Activated(object sender, EventArgs e) {
            // unsuscribe from thie event
            listBox1.MouseMove -= Autocomplete_Activated;
            _isFormActivated = true;
            Opacity = _focusOpacity;
            Npp.GrabFocus();
            _mouseIsInForm = true;
        }

        /// when the cursor enter the form
        private void Autocomplete_MouseEnter(object sender, EventArgs e) {
            if (!_mouseIsInForm) {
                Npp.GrabFocus();
                _mouseIsInForm = true;
            }
        }

        ///  when the cursor leave
        private void Autocomplete_MouseLeave(object sender, EventArgs e) {
            if (_mouseIsInForm) {
                Npp.GrabFocus();
                _mouseIsInForm = false;
            }
        }

        private void AutocompleteForm_FormClosed(object sender, FormClosedEventArgs e) {
            Capture = false;
        }

        // called on closing
        private void Autocomplete_FormClosing(object sender, FormClosingEventArgs e) {
            FormIntegration.UnRegisterToNpp(Handle);
        }

        private void listBox1_DoubleClick(object sender, EventArgs e) {
            _onAutocompletionAccepted((CompletionData) listBox1.SelectedItem);
        }

        private void AutocompleteForm_MouseDown(object sender, MouseEventArgs e) {
            Npp.GrabFocus();
            if (e.Location.X < 0 || e.Location.Y < 0 || e.Location.X > Width || e.Location.Y > Height) {
                 Close();
            }
        }

        #endregion


        #region "on key events"

        public bool OnKeyDown(Keys key) {
            bool handled = true;
            // down and up change the selection
            if (key == Keys.Up) {
                if (listBox1.SelectedIndex > 0)
                    listBox1.SelectedIndex--;
                else
                    listBox1.SelectedIndex = (listBox1.Items.Count - 1);
            } else if (key == Keys.Down) {
                if (listBox1.SelectedIndex < (listBox1.Items.Count - 1))
                    listBox1.SelectedIndex++;
                else
                    listBox1.SelectedIndex = 0;

                // left and right key change the selector
            } else if (key == Keys.Left || key == Keys.Right) {
                if (key == Keys.Left) {
                    _curSelectorPos--;
                    if (_curSelectorPos < 0) _curSelectorPos = _selectorData.Count;
                } else {
                    _curSelectorPos++;
                    if (_curSelectorPos >= _selectorData.Count) _curSelectorPos = 0;
                }

                int cnt = 0;
                foreach (var entry in _selectorData) {
                    if (_curSelectorPos == 0) {
                        entry.Value.Active = true;
                    } else if (_curSelectorPos == cnt) {
                        entry.Value.Active = true;
                    } else {
                        entry.Value.Active = false;
                    }
                    cnt++;
                }

                // filter
                RememberSelector();
                FilterFor();

                // escape close
            } else if (key == Keys.Escape) {
                Close();

                // enter and tab accept the current selection
            } else if ((key == Keys.Enter && Lib.Config.Instance.AutoCompleteUseEnterToAccept)
                       || (key == Keys.Tab && Lib.Config.Instance.AutoCompleteUseTabToAccept)) {
                _onAutocompletionAccepted((CompletionData) listBox1.SelectedItem);

                // else, any other key needs to be analysed by Npp
            } else {
                handled = false;
            }
            return handled;
        }

        #endregion


        #region "filter for"

        public void FilterFor() {
            FilterFor(_lastKeyword);
        }

        public void FilterFor(string partialName) {
            if (!_rawItems.Any()) return;

            _currentTable = Npp.GetCurrentTable();
            _lastKeyword = partialName;

            List<CompletionData> items;
            List<CompletionData> itemsBeforeFilter;

            // Apply a filter to the list of keyword provided
            if (string.IsNullOrWhiteSpace(partialName)) {
                itemsBeforeFilter = _rawItems;
            } else {
                // filter by "containing partialName"
                itemsBeforeFilter = _rawItems.Where(x => x.DisplayText.Contains(partialName, StringComparison.OrdinalIgnoreCase)).ToList();
                // order by "equals" then "starting with" and finally "containing" 
                if (itemsBeforeFilter.Any())
                    itemsBeforeFilter = itemsBeforeFilter.OrderBy(
                        x => {
                            if (!x.DisplayText.StartsWith(partialName, StringComparison.OrdinalIgnoreCase))
                                return 2;
                            if (x.DisplayText.Equals(partialName, StringComparison.OrdinalIgnoreCase)) {
                                return 0;
                            }
                            return 1;
                        }).ToList();
            }

            // if the selector data are defined, we need to filter further more but categories allowed
            if (_selectorDataRem.Count > 0) {
                List<int> listOfActiveType = (from entry in _selectorDataRem where entry.Value select entry.Key).ToList();
                items = itemsBeforeFilter.Where(x => listOfActiveType.Contains((int) x.Type)).ToList();
            } else {
                items = itemsBeforeFilter;
            }

            // if the list is empty and the user didn't force to show the completion list
            if (!items.Any() && !DisplayFromShortcut && !Lib.Config.Instance.AutoCompleteAlwaysShowEmptyList && !_isFormActivated)
                Close();

            else {
                listBox1.BeginUpdate();
                if (!items.Any()) {
                    // list is empty, we notify it to the user
                    items.Add(new CompletionData {DisplayText = "<empty>", Type = CompletionType.Empty});
                }
                listBox1.DataSource = _displayWholeList ? items : items.Take(Lib.Config.Instance.AutoCompleteNumberOfItemsToShowOnKeyInput).ToList();
                listBox1.EndUpdate();
                listBox1.DisplayMember = "DisplayText";

                // Store selector data, to be used when we draw it
                _selectorData.Clear();
                var imgRect = new Rectangle(2, 1, _imgSize, _imgSize);
                _selectorData.Add(0, new SelectorItem { Active = (!_selectorDataRem.ContainsKey(0) || _selectorDataRem[0]), Img = ImageResources.autocompletion_all, Position = imgRect });
                imgRect.X = imgRect.X + _imgSize + 3;
                foreach (CompletionType compType in Enum.GetValues(typeof (CompletionType))) {
                    // for each type of completion, check if there is at least 1 item of this type in the list
                    var item = itemsBeforeFilter.FirstOrDefault(x => x.Type == compType);
                    if (item != null && !item.Equals(new CompletionData())) {
                        bool remState = true;
                        if (!_selectorDataRem.ContainsKey((int) compType))
                            _selectorDataRem[(int) compType] = true;
                        else
                            remState = _selectorDataRem[(int) compType];
                        _selectorData.Add((int) compType, new SelectorItem {Active = remState, Img = GetImageFor(item), Position = imgRect});
                        imgRect.X = imgRect.X + _imgSize + 3;
                    }
                }
                var activateSelector = ((_selectorData.Count > 0) && _displayWholeList);
                _extraHeight = (activateSelector) ? _imgSize + _verticalSpacing : 0; // height of the suggestion type selector

                // listbox
                var wideItem = items.Select(x => (int) listBox1.CreateGraphics().MeasureString(x.DisplayText, listBox1.Font).Width).Max(x => x);

                var nbItemsTodisp = Math.Min(listBox1.Items.Count, Lib.Config.Instance.AutoCompleteShowListOfXSuggestions);
                if (Lib.Config.Instance.AutoCompleteFixedHeight) nbItemsTodisp = Lib.Config.Instance.AutoCompleteShowListOfXSuggestions;
                if (!_displayWholeList) nbItemsTodisp = Math.Min(nbItemsTodisp, Lib.Config.Instance.AutoCompleteNumberOfItemsToShowOnKeyInput);

                listBox1.SelectedIndex = 0;

                // form
                var thisWidth = Math.Min(wideItem + _imgSize + ((_displayWholeList) ? _scrollBarSize : 0) + 30, 270); // +30 extra to look good

                // picturebox (if needed)
                pictureBox1.Enabled = activateSelector;

                // animate listbox 
                listBox1.ShowScrollbar = _displayWholeList;
                AnimateForm(_isFormActivated ? _focusOpacity : _unfocusOpacity, (_itemHeight + _verticalSpacing) * nbItemsTodisp + 5, thisWidth);
            }
        }

        #endregion


        #region "draw main form"

        private void AnimateForm(double finalOpacity, int finalHeight, int finalWidth) {
            _timerOpacityFinal = finalOpacity;
            _timerOpacityStep = 0.1;//(finalOpacity - Opacity)/10;
            _timerHeightFinal = finalHeight;
            _timerHeightStep = Math.Sign((finalHeight - listBox1.Height))*15;//(finalHeight - listBox1.Height)/10;
            _timerWidthFinal = finalWidth;
            _timerWidthStep = Math.Sign((finalWidth - Width)) * 15;//(finalWidth - Width)/10;
            
            // start the animation?
            if ((_timerHeightStep != 0 || _timerWidthStep != 0) && Lib.Config.Instance.AutoCompleteAnimate) {
                pictureBox1.Visible = false;
                KillAnimTimer();
                _timer = new Timer {Interval = 1};
                _timer.Tick += (sender, args) => {
                    var heightNotEnded = (_timerHeightStep > 0 && listBox1.Height + _timerHeightStep < _timerHeightFinal) || (_timerHeightStep < 0 && listBox1.Height + _timerHeightStep > _timerHeightFinal);
                    var widthNotEnded = (_timerWidthStep > 0 && Width + _timerWidthStep < _timerWidthFinal) || (_timerWidthStep < 0 && Width + _timerWidthStep > _timerWidthFinal);
                    var opacityNotEnded = (_timerOpacityStep > 0 && Opacity + _timerOpacityStep < _timerOpacityFinal) || (_timerOpacityStep < 0 && Opacity + _timerOpacityStep > _timerOpacityFinal);
                    if (heightNotEnded || widthNotEnded || opacityNotEnded) {
                        // animation in progress
                        listBox1.Height = (heightNotEnded) ? listBox1.Height + _timerHeightStep : _timerHeightFinal;
                        Height = listBox1.Height + _extraHeight + 8;
                        Width = (widthNotEnded) ? Width + _timerWidthStep : _timerWidthFinal;
                        Opacity = (opacityNotEnded) ? Opacity + _timerOpacityStep : _timerOpacityFinal;
                        listBox1.Invalidate();
                    } else {
                        KillAnimTimer();
                        FinalizeDisplay();
                    }
                };
                _timer.Start();
            } else
                FinalizeDisplay();
        }

        private void FinalizeDisplay() {
            Width = _timerWidthFinal;
            listBox1.Height = _timerHeightFinal;
            Height = listBox1.Height + _extraHeight + 8;
            Opacity = _timerOpacityFinal;
            //listBox1.Invalidate();
            // draw selector conditionnally
            if (pictureBox1.Enabled) {
                var extraHeight = _imgSize + _verticalSpacing;
                pictureBox1.Top = pictureBox1.Top - (extraHeight - pictureBox1.Height);
                pictureBox1.Height = extraHeight;
                pictureBox1.Width = _selectorData.Last().Value.Position.X + _selectorData.Last().Value.Position.Width + 3;
                pictureBox1.Visible = true;
                pictureBox1.Invalidate();
            }
            Npp.GrabFocus();
        }

        private void KillAnimTimer() {
            try {
                _timer.Stop();
                _timer.Dispose();
            } catch (Exception) {
                // ignored
            }
        }

        private void listBox1_MeasureItem(object sender, MeasureItemEventArgs e) {
            var item = (CompletionData) listBox1.Items[e.Index];
            var size = e.Graphics.MeasureString(item.DisplayText, listBox1.Font);
            e.ItemHeight = (int) size.Height + _verticalSpacing;
            e.ItemWidth = (int) size.Width + _imgSize + _scrollBarSize; //ensure enough space for the icon and the scrollbar
            listBox1.HorizontalExtent = e.ItemWidth;
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e) {
            if (e.Index == -1)
                return;

            var item = (CompletionData) listBox1.Items[e.Index];

            e.DrawBackground();

            var brush = Brushes.Black;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                brush = Brushes.White;

            var rect = e.Bounds;
            rect.Offset(_imgSize + 1, _verticalSpacing);
            e.Graphics.DrawString(item.DisplayText, e.Font, brush, rect, StringFormat.GenericDefault);

            var image = GetImageFor(item);
            if (image != null) {
                var r = e.Bounds;
                r.Width = _imgSize + 1;
                e.Graphics.FillRectangle(Brushes.White, r);
                r.Y = r.Y + 1;
                r.Width = _imgSize;
                r.Height = _imgSize;
                e.Graphics.DrawImage(image, r);
            }
        }

        private Image GetImageFor(CompletionData item) {
            Image img = ImageResources.autocompletion_snippets;
            switch (item.Type) {
                case CompletionType.Keyword:
                    img = ImageResources.autocompletion_keyword;
                    break;
                case CompletionType.Table:
                    img = ImageResources.autocompletion_table;
                    break;
                case CompletionType.Field:
                    img = DataBaseInfo.IsFieldInPk(_currentTable, item.DisplayText) ? ImageResources.autocompletion_field_pk : ImageResources.autocompletion_field;
                    break;
                case CompletionType.Snippet:
                    img = ImageResources.autocompletion_snippets;
                    break;
                case CompletionType.Buffer:
                    img = ImageResources.autocompletion_snippets;
                    break;
                case CompletionType.Temptable:
                    img = ImageResources.autocompletion_snippets;
                    break;
                case CompletionType.Function:
                    img = ImageResources.autocompletion_snippets;
                    break;
                case CompletionType.Procedure:
                    img = ImageResources.autocompletion_snippets;
                    break;
                case CompletionType.Special:
                    img = ImageResources.autocompletion_snippets;
                    break;
                case CompletionType.Empty:
                    img = ImageResources.autocompletion_empty;
                    break;
            }
            return img;
        }

        #endregion


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
    }

    public class SelectorItem {
        public bool Active { get; set; }
        public Image Img { get; set; }
        public Rectangle Position { get; set; }
    }

    public class AutoCompleteListBox : ListBox {
        // ReSharper disable once InconsistentNaming
        private bool mShowScroll;

        protected override CreateParams CreateParams {
            get {
                var cp = base.CreateParams;
                if (!mShowScroll)
                    cp.Style = cp.Style & ~0x200000;
                return cp;
            }
        }

        public bool ShowScrollbar {
            get { return mShowScroll; }
            set {
                if (value != mShowScroll) {
                    mShowScroll = value;
                    if (IsHandleCreated)
                        RecreateHandle();
                }
            }
        }
    }
}