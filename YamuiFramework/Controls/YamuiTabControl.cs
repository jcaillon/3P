using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Fonts;
using YamuiFramework.Forms;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {

    #region YamuiTabPageCollection
    [ToolboxItem(false)]
    [Editor("YamuiFramework.Controls.YamuiTabPageCollectionEditor", typeof(UITypeEditor))]
    public class YamuiTabPageCollection : TabControl.TabPageCollection {
        public YamuiTabPageCollection(YamuiTabControl owner)
            : base(owner) { }
    }
    #endregion

    [Designer("YamuiFramework.Controls.YamuiTabControlDesigner")]
    [ToolboxBitmap(typeof(TabControl))]
    public class YamuiTabControl : TabControl {

        #region Fields
        private ContentAlignment _textAlign = ContentAlignment.TopLeft;
        [DefaultValue(ContentAlignment.TopLeft)]
        [Category("Yamui")]
        public ContentAlignment TextAlign {
            get {
                return _textAlign;
            }
            set {
                _textAlign = value;
            }
        }

        [Editor("YamuiFramework.Controls.YamuiTabPageCollectionEditor", typeof(UITypeEditor))]
        public new TabPageCollection TabPages {
            get {
                return base.TabPages;
            }
        }

        private TabFunction _function = TabFunction.Main;
        [DefaultValue(TabFunction.Main)]
        [Category("Yamui")]
        public TabFunction Function {
            get { return _function; }
            set {
                _function = value;
                SetStuff();
                Font = FontManager.GetTabControlFont(Function);
            }
        }

        [Category("Yamui")]
        [DefaultValue(false)]
        public bool UseCustomBackColor { get; set; }

        private bool _showNormallyHiddenTabs;
        public bool ShowNormallyHiddenTabs {
            get { return _showNormallyHiddenTabs; }
            set { _showNormallyHiddenTabs = value; }
        }

        // used to remember the position of each tab
        private Dictionary<int, Rectangle> _getRekt = new Dictionary<int, Rectangle>();

        /// <summary>
        /// this is the actual tab index that should be display! dont use SelectedIndex but use this instead
        /// </summary>
        private int _selectedIndex;
        [Browsable(false)]
        public int SelectIndex {
            get { return _selectedIndex; }
            set {
                if (value < 0) return;
                if (value == _selectedIndex) return;
                _selectedIndex = value;
                _fromSelectIndex = true;
                SelectedIndex = _selectedIndex;
            }
        }

        private bool _fromSelectIndex;
        private int _lastSelectedTab;

        // the index of the current tab hovered by the cursor
        private int _hotTrackTab = -1;

        private bool _isHovered;
        private bool _isFocused;

        // the following reference is used to always know the size and position of a secondary tabpage (for animation purposes)
        private static YamuiTabPage _referencePage;
        private static YamuiTabAnimation _referenceSmokeScreen;
        #endregion

        #region Constructor

        public YamuiTabControl() {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.Selectable |
                ControlStyles.AllPaintingInWmPaint, true);

            MouseMove += (sender, args) => UpdateHotTrack(args.Location);

            SetStuff();
        }

        private void SetStuff() {
            var itemWidth = TabPages.Count > 0 ? Width / TabPages.Count : 5;
            itemWidth = Math.Max(itemWidth - 5, 0);
            // we set the item size so the scroll bars never need to appear
            ItemSize = new Size(itemWidth, (Function == TabFunction.Main) ? 32 : 18);
            Dock = DockStyle.Fill;
        }
        #endregion

        #region "tab animator"
        protected override void OnSelecting(TabControlCancelEventArgs e) {
            // cancel the tab selecting if it was not set through SelectIndex
            if (!_fromSelectIndex) {
                e.Cancel = true;
                return;
            }
            _fromSelectIndex = false;

            // if we switch from normallyHiddenPage, we want to show the normal menu again
            if (ShowNormallyHiddenTabs) {
                _getRekt.Clear();
                YamuiTabPage lastPage = (YamuiTabPage)TabPages[_lastSelectedTab];
                lastPage.HiddenState = true;
                ShowNormallyHiddenTabs = false;
            } else {
                YamuiTabPage tabPage = (YamuiTabPage)TabPages[e.TabPageIndex];
                if (tabPage.HiddenState != tabPage.HiddenPage) {
                    // we are selecting a hidden page
                    _getRekt.Clear();
                    ShowNormallyHiddenTabs = true;
                }
            }
            Invalidate(new Rectangle(0, 0, Width, ItemSize.Height));
            Update();

            _lastSelectedTab = e.TabPageIndex;

            base.OnSelecting(e);

            // animation of the tab!
            TabAnimator();
        }

        public void TabAnimator() {
            // the principle is easy, we create a foreground form on top of our form with the same back ground,
            // and we animate its opacity value from 1 to 0 to effectivly create a fade in animation
            var x = (YamuiTabPage)SelectedTab;
            if (_referencePage == null && x.Function == TabFunction.Secondary) _referencePage = (YamuiTabPage)SelectedTab;
            if (!ThemeManager.TabAnimationAllowed) return;
            try {
                if (_referenceSmokeScreen == null) {
                    // this means we just loaded the main form, no need to do another anim
                    _referenceSmokeScreen = new YamuiTabAnimation(FindForm(), _referencePage) {Opacity = 0d};
                    return;
                }
                var t = new Transition(new TransitionType_Acceleration(500));
                _referenceSmokeScreen.Opacity = 1d;
                t.add(_referenceSmokeScreen, "Opacity", 0d);
                // we need to force the new tab to draw before we animate it
                Application.DoEvents();
                t.run();
            } catch (Exception) {
                // ignored
            }
        }
        #endregion

        #region tabhover
        // returns the index of the tab under the cursor, or -1 if no tab is under
        private int GetTabUnderCursor(Point loc) {
            for (int i = 0; i < TabPages.Count; i++) {
                if (GetRektOf(i).Contains(loc))
                    return i;
            }
            return -1;
        }

        // updates hot tracking based on the current cursor position
        private void UpdateHotTrack(Point loc) {
            int hot = GetTabUnderCursor(loc);
            if (hot != _hotTrackTab) {
                // invalidate the old hot-track item to remove hot-track effects
                if (_hotTrackTab != -1)
                    Invalidate(GetRektOf(_hotTrackTab));

                _hotTrackTab = hot;

                // invalidate the new hot-track item to add hot-track effects
                if (_hotTrackTab != -1)
                    Invalidate(GetRektOf(_hotTrackTab));

                // force the tab to redraw invalidated regions
                Update();
            }
        }
        #endregion

        #region Paint Methods
        protected override void OnPaint(PaintEventArgs e) {
            Color backColor = UseCustomBackColor ? BackColor : ThemeManager.Current.TabsColorsNormalBackColor;
            e.Graphics.Clear(backColor);

            if (_showNormallyHiddenTabs) _getRekt.Clear();
            for (var index = 0; index < TabPages.Count; index++) {
                YamuiTabPage tabPage = (YamuiTabPage)TabPages[index];
                if (_showNormallyHiddenTabs && tabPage.HiddenPage || !_showNormallyHiddenTabs && !tabPage.HiddenPage || DesignMode)
                    DrawTab(index, e.Graphics, tabPage);
            }
        }

        private void DrawTab(int index, Graphics graphics, YamuiTabPage tabPage) {
            
            Font usedFont = FontManager.GetTabControlFont((Function == TabFunction.Secondary && index != SelectIndex) ? TabFunction.SecondaryNotSelected : Function);
            Rectangle thisTabRekt;

            if (!_getRekt.ContainsKey(index)) {
                var textWidth = TextRenderer.MeasureText(graphics, tabPage.Text, usedFont, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.Top | TextFormatFlags.Left).Width + ((Function == TabFunction.Secondary) ? 10 : 0);
                if (DesignMode) textWidth = ItemSize.Width; // in the designer mode, be sure to display everthing
                if (_getRekt.Count == 0)
                    thisTabRekt = new Rectangle(0, 0, textWidth, ItemSize.Height);
                else
                    thisTabRekt = new Rectangle(_getRekt.Last().Value.X + _getRekt.Last().Value.Width, _getRekt.Last().Value.Y, textWidth, _getRekt.Last().Value.Height);
                _getRekt.Add(index, thisTabRekt);
            } else {
                thisTabRekt = GetRektOf(index);
            }

            Color foreColor = ThemeManager.TabsColors.ForeGround(_isFocused, (index == _hotTrackTab && _isHovered), index == SelectIndex);
            TextRenderer.DrawText(graphics, tabPage.Text, usedFont, thisTabRekt, foreColor, TextFormatFlags.Top | TextFormatFlags.Left);
        }
        #endregion

        #region Overridden Methods
        protected override void OnSelectedIndexChanged(EventArgs e) {
            base.OnSelectedIndexChanged(e);
            Focus();
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            Invalidate();
        }

        private new Rectangle GetTabRect(int index) {
            if (index < 0)
                return new Rectangle();
            Rectangle baseRect = base.GetTabRect(index);
            return baseRect;
        }

        protected override void OnCreateControl() {
            base.OnCreateControl();
            SetStuff();
        }

        protected override void OnControlAdded(ControlEventArgs e) {
            base.OnControlAdded(e);
            SetStuff();
        }

        protected override void OnControlRemoved(ControlEventArgs e) {
            base.OnControlRemoved(e);
            SetStuff();
        }
        #endregion

        #region Helper Methods
        private void SaveFormCurrentPath() {
            // try to save in the history of the form
            try {
                YamuiForm ownerForm = (YamuiForm)FindForm();
                if (ownerForm != null) ownerForm.SaveCurrentPathInHistory();
            } catch (Exception) {
                // ignored
            }
        }

        private Rectangle GetRektOf(int index) {
            return _getRekt.ContainsKey(index) ? _getRekt[index] : new Rectangle();
        }

        public int GetIndexOf(YamuiTabPage page) {
            for (int i = 0; i < TabPages.Count; i++) {
                var tPage = (YamuiTabPage)TabPages[i];
                if (tPage == page)
                    return i;
            }
            return -1;
        }
        #endregion

        #region Managing isHovered, isPressed, isFocused

        #region Focus Methods

        protected override void OnGotFocus(EventArgs e) {
            _isFocused = true;
            try {
                if (Function == TabFunction.Secondary) {
                    YamuiTabControl par = (YamuiTabControl)Parent.Parent;
                    par.ForceUnfocus();
                    par.Invalidate();
                }
            } catch (Exception) {
                // ignored
            }
            Invalidate();
            base.OnGotFocus(e);
        }

        public void ForceUnfocus() {
            _isFocused = false;
        }

        protected override void OnLostFocus(EventArgs e) {
            _isFocused = false;
            Invalidate();

            base.OnLostFocus(e);
        }

        protected override void OnEnter(EventArgs e) {
            _isFocused = true;
            Invalidate();

            base.OnEnter(e);
        }

        protected override void OnLeave(EventArgs e) {
            _isFocused = false;
            Invalidate();

            base.OnLeave(e);
        }

        #endregion

        #region Keyboard Methods

        protected override void OnKeyDown(KeyEventArgs e) {
            try {
                if (!_isFocused) return;
                if (e.KeyCode == Keys.Space) {
                    if (SelectIndex == _hotTrackTab) return;
                    SaveFormCurrentPath();
                    SelectIndex = _hotTrackTab;
                    Invalidate();
                }
                if (e.KeyCode == Keys.Left) {
                    if (!ShowNormallyHiddenTabs && SelectIndex > 0) SelectIndex--;
                    Invalidate();
                }
                if (e.KeyCode == Keys.Right) {
                    if (!ShowNormallyHiddenTabs && SelectIndex < TabPages.Count - 1) {
                        YamuiTabPage tabPage = (YamuiTabPage) TabPages[SelectedIndex + 1];
                        if (tabPage.HiddenState != true) SelectIndex++;
                    }
                    Invalidate();
                }
                if (e.KeyCode == Keys.Up) {
                    try {
                        if (Function == TabFunction.Secondary) {
                            Parent.Parent.Focus();
                        }
                    } catch (Exception) {
                        // ignored
                    }
                }
                if (e.KeyCode == Keys.Down) {
                    try {
                        if (Function == TabFunction.Main) {
                            var listCtrl = ControlHelper.GetAll(Controls[SelectIndex], typeof(YamuiTabControl));
                            if (listCtrl != null) listCtrl.First().Focus();
                        }
                    } catch (Exception) {
                        // ignored
                    }
                }
            } catch (Exception) {
                Invalidate();
            }
            base.OnKeyDown(e);
        }
        #endregion

        #region Mouse Methods

        protected override void OnMouseEnter(EventArgs e) {
            _isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                if (SelectIndex == _hotTrackTab) return;
                SaveFormCurrentPath();
                SelectIndex = _hotTrackTab;
                Invalidate();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseLeave(EventArgs e) {
            _isHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        #endregion

        #endregion
    }

    #region YamuiTabControlDesigner
    internal class YamuiTabControlDesigner : ParentControlDesigner {
        #region Fields

        private readonly DesignerVerbCollection _designerVerbs = new DesignerVerbCollection();

        private IDesignerHost _designerHost;

        private ISelectionService _selectionService;

        public override SelectionRules SelectionRules {
            get { return Control.Dock == DockStyle.Fill ? SelectionRules.Visible : base.SelectionRules; }
        }

        public override DesignerVerbCollection Verbs {
            get {
                if (_designerVerbs.Count == 2) {
                    var myControl = (YamuiTabControl)Control;
                    _designerVerbs[1].Enabled = myControl.TabCount != 0;
                }
                return _designerVerbs;
            }
        }

        public IDesignerHost DesignerHost {
            get { return _designerHost ?? (_designerHost = (IDesignerHost)(GetService(typeof(IDesignerHost)))); }
        }

        public ISelectionService SelectionService {
            get { return _selectionService ?? (_selectionService = (ISelectionService)(GetService(typeof(ISelectionService)))); }
        }

        #endregion

        #region Constructor

        public YamuiTabControlDesigner() {
            var verb1 = new DesignerVerb("Add Tab", OnAddPage);
            var verb2 = new DesignerVerb("Remove Tab", OnRemovePage);
            _designerVerbs.AddRange(new[] { verb1, verb2 });
        }

        #endregion

        #region Private Methods

        private void OnAddPage(Object sender, EventArgs e) {
            var parentControl = (YamuiTabControl)Control;
            var oldTabs = parentControl.Controls;

            RaiseComponentChanging(TypeDescriptor.GetProperties(parentControl)["TabPages"]);

            var p = (YamuiTabPage)(DesignerHost.CreateComponent(typeof(YamuiTabPage)));
            p.Text = p.Name;
            parentControl.TabPages.Add(p);

            RaiseComponentChanged(TypeDescriptor.GetProperties(parentControl)["TabPages"],
                oldTabs, parentControl.TabPages);
            parentControl.SelectedTab = p;

            SetVerbs();
        }

        private void OnRemovePage(Object sender, EventArgs e) {
            var parentControl = (YamuiTabControl)Control;
            var oldTabs = parentControl.Controls;

            if (parentControl.SelectedIndex < 0) {
                return;
            }

            RaiseComponentChanging(TypeDescriptor.GetProperties(parentControl)["TabPages"]);

            DesignerHost.DestroyComponent(parentControl.TabPages[parentControl.SelectedIndex]);

            RaiseComponentChanged(TypeDescriptor.GetProperties(parentControl)["TabPages"],
                oldTabs, parentControl.TabPages);

            SelectionService.SetSelectedComponents(new IComponent[] {
                parentControl
            }, SelectionTypes.Auto);

            SetVerbs();
        }

        private void SetVerbs() {
            var parentControl = (YamuiTabControl)Control;

            switch (parentControl.TabPages.Count) {
                case 0:
                    Verbs[1].Enabled = false;
                    break;
                default:
                    Verbs[1].Enabled = true;
                    break;
            }
        }

        #endregion

        #region Overrides

        protected override void WndProc(ref Message m) {
            base.WndProc(ref m);
            switch (m.Msg) {
                case (int)WinApi.Messages.WM_NCHITTEST:
                    if (m.Result.ToInt32() == (int)WinApi.HitTest.HTTRANSPARENT) {
                        m.Result = (IntPtr)WinApi.HitTest.HTCLIENT;
                    }
                    break;
            }
        }

        protected override bool GetHitTest(Point point) {
            if (SelectionService.PrimarySelection == Control) {
                var hti = new WinApi.TCHITTESTINFO {
                    pt = Control.PointToClient(point),
                    flags = 0
                };

                var m = new Message {
                    HWnd = Control.Handle,
                    Msg = WinApi.TCM_HITTEST
                };

                var lparam =
                    Marshal.AllocHGlobal(Marshal.SizeOf(hti));
                Marshal.StructureToPtr(hti,
                    lparam, false);
                m.LParam = lparam;

                base.WndProc(ref m);
                Marshal.FreeHGlobal(lparam);

                if (m.Result.ToInt32() != -1) {
                    return hti.flags != (int)WinApi.TabControlHitTest.TCHT_NOWHERE;
                }
            }

            return false;
        }
        
        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("ImeMode");
            properties.Remove("FlatAppearance");
            properties.Remove("FlatStyle");
            properties.Remove("AutoEllipsis");
            properties.Remove("UseCompatibleTextRendering");

            properties.Remove("Image");
            properties.Remove("ImageAlign");
            properties.Remove("ImageIndex");
            properties.Remove("ImageKey");
            properties.Remove("ImageList");
            properties.Remove("TextImageRelation");

            properties.Remove("BackgroundImage");
            properties.Remove("BackgroundImageLayout");
            properties.Remove("UseVisualStyleBackColor");

            //properties.Remove("ItemSize");
            properties.Remove("Font");
            properties.Remove("RightToLeft");
            
            base.PreFilterProperties(properties);
            /*
            string[] propertiesToHide = {
                "AccessibleDescription", 
                "AccessibleName", 
                "AccessibleRole", 
                "Alignment", 
                "AllowDrop", 
                "Appearance", 
                "AutoScrollOffset", 
                "CausesValidation", 
                "HotTrack", 
                "MaximumSize", 
                "MinimumSize", 
                "Font", 
                "Anchor", 
                "DrawMode", 
                "Enabled",
                "ImeMode", 
                "ItemSize", 
                "Location", 
                "Locked", 
                "Margin", 
                "MaximumSize", 
                "MinimumSize", 
                "Modifiers", 
                "Multiline", 
                "RightToLeft", 
                "RightToLeftLayout", 
                "ShowToolTips", 
                "Size",
                "Multiline", 
                "RightToLeft", 
                "RightToLeftLayout", 
                "ShowToolTips", 
                "Size",
                "SizeMode", 
                "Tag", 
                "UseWaitCursor", 
                "ShowNormallyHiddenTabs"
            };

            foreach (string propname in propertiesToHide) {
                var prop = (PropertyDescriptor)properties[propname];
                if (prop != null) {
                    AttributeCollection runtimeAttributes = prop.Attributes;
                    // make a copy of the original attributes but make room for one extra attribute
                    Attribute[] attrs = new Attribute[runtimeAttributes.Count + 1];
                    runtimeAttributes.CopyTo(attrs, 0);
                    attrs[runtimeAttributes.Count] = new BrowsableAttribute(false);
                    prop = TypeDescriptor.CreateProperty(this.GetType(), propname, prop.PropertyType, attrs);
                    properties[propname] = prop;
                }
            }
            */
        }

        #endregion
    }

    #endregion

    #region YamuiTabPageCollectionEditor

    internal class YamuiTabPageCollectionEditor : CollectionEditor {
        protected override CollectionForm CreateCollectionForm() {
            var baseForm = base.CreateCollectionForm();
            baseForm.Text = "YamuiTabPage Collection Editor";
            return baseForm;
        }

        public YamuiTabPageCollectionEditor(Type type)
            : base(type) { }

        protected override Type CreateCollectionItemType() {
            return typeof(YamuiTabPage);
        }

        protected override Type[] CreateNewItemTypes() {
            return new[] { typeof(YamuiTabPage) };
        }
    }

    #endregion

}
