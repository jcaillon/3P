using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using YamuiFramework.Helper;
using YamuiFramework.Themes;
using _3PA.Interop;

namespace _3PA.MainFeatures.NppInterfaceForm {

    /// <summary>
    /// This is the base class for the tooltips and the autocomplete form
    /// </summary>
    public partial class NppInterfaceForm : Form {

        #region fields
        /// <summary>
        /// Should be set when you create the new form
        /// CurrentForegroundWindow = WinApi.GetForegroundWindow();
        /// </summary>
        public IntPtr CurrentForegroundWindow;

        /// <summary>
        /// Set to true if scintilla should get the focus back, false if you want
        /// to use CurrentForegroundWindow
        /// </summary>
        public bool GiveFocusBackToScintilla = true;

        /// <summary>
        /// Sets the Opacity to give to the window when it's not focused
        /// </summary>
        public double UnfocusedOpacity;

        /// <summary>
        /// Sets the Opacity to give to the window when it's focused
        /// </summary>
        public double FocusedOpacity;

        private bool _allowInitialdisplay;
        private bool _focusAllowed;
        // check the npp window rect, if it has changed from a previous state, close this form (poll every 500ms)
        private Rectangle? _nppRect;
        private Timer _timerCheckNppRect;
        /// <summary>
        /// Use this to know if the form is currently activated
        /// </summary>
        public bool IsActivated;
        #endregion

        #region constructor
        /// <summary>
        /// Create a new npp interface form, please set CurrentForegroundWindow
        /// </summary>
        public NppInterfaceForm() {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);

            InitializeComponent();

            // register to Npp
            FormIntegration.RegisterToNpp(Handle);

            // timer to check if the npp window changed
            _timerCheckNppRect = new Timer {
                Enabled = true,
                Interval = 500
            };
            _timerCheckNppRect.Tick += TimerCheckNppRectTick;

            Opacity = 0;
            Visible = false;
            Tag = false;
            Closing += OnClosing;
        }
        #endregion

        /// <summary>
        /// hides the form
        /// </summary>
        public void Cloack() {
            GiveFocusBack();

            Visible = false;

            // move this to an invisible part of the screen, otherwise we can see this window
            // if another window with Opacity <1 is in front Oo
            var pt = Screen.PrimaryScreen.WorkingArea.Location;
            pt.Offset(new Point(-Width, -Height));
            Location = pt;
        }

        /// <summary>
        /// show the form
        /// </summary>
        public void UnCloack() {
            _allowInitialdisplay = true;
            Opacity = UnfocusedOpacity;
            Visible = true;

            GiveFocusBack();
        }

        /// <summary>
        /// Call this method instead of Close() to really close this form
        /// </summary>
        public void ForceClose() {
            FormIntegration.UnRegisterToNpp(Handle);
            Tag = true;
            Close();
        }

        /// <summary>
        /// instead of closing, cloak this form (invisible)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="cancelEventArgs"></param>
        private void OnClosing(object sender, CancelEventArgs cancelEventArgs) {
            if ((bool)Tag) return;
            cancelEventArgs.Cancel = true;
            Cloack();
        }

        /// <summary>
        /// Gives focus back to the owner window
        /// </summary>
        public void GiveFocusBack() {
            if (GiveFocusBackToScintilla)
                Npp.GrabFocus();
            else
                WinApi.SetForegroundWindow(CurrentForegroundWindow);
            IsActivated = !IsActivated;
            Opacity = UnfocusedOpacity;
        }

        protected override void OnActivated(EventArgs e) {
            // Activate the window that previously had focus
            if (!_focusAllowed)
                if (GiveFocusBackToScintilla)
                    Npp.GrabFocus();
                else
                    WinApi.SetForegroundWindow(CurrentForegroundWindow);
            else {
                IsActivated = true;
                Opacity = FocusedOpacity;
            }
            base.OnActivated(e);
        }

        protected override void OnShown(EventArgs e) {
            _focusAllowed = true;
            base.OnShown(e);
        }

        private void TimerCheckNppRectTick(object sender, EventArgs e) {
            try {
                var rect = Npp.GetWindowRect();
                if (_nppRect.HasValue && _nppRect.Value != rect)
                    Close();
                _nppRect = rect;
            } catch (Exception) {
                // ignored
            }
        }

        /// <summary>
        /// This ensures the form is never visible at start
        /// </summary>
        /// <param name="value"></param>
        protected override void SetVisibleCore(bool value) {
            base.SetVisibleCore(_allowInitialdisplay ? value : _allowInitialdisplay);
        }
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

        #region Shadows

        private bool _mAeroEnabled; // variables for box shadow
        private const int CsDropshadow = 0x00020000;
        private const int WmNcpaint = 0x0085;

        private const int WmNchittest = 0x84; // variables for dragging the form
        private const int Htclient = 0x1;
        private const int Htcaption = 0x2;

        protected override CreateParams CreateParams {
            get {
                _mAeroEnabled = CheckAeroEnabled();

                var cp = base.CreateParams;
                if (!_mAeroEnabled)
                    cp.ClassStyle |= CsDropshadow;

                return cp;
            }
        }

        private bool CheckAeroEnabled() {
            if (Environment.OSVersion.Version.Major >= 6) {
                var enabled = 0;
                DwmApi.DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1);
            }
            return false;
        }

        protected override void WndProc(ref Message m) {

            if (m.Msg == WmNcpaint && _mAeroEnabled) {
                var v = 2;
                DwmApi.DwmSetWindowAttribute(Handle, 2, ref v, 4);
                var margins = new DwmApi.MARGINS(1, 1, 1, 1);
                DwmApi.DwmExtendFrameIntoClientArea(Handle, ref margins);
            }


            base.WndProc(ref m);

            if (m.Msg == WmNchittest && (int)m.Result == Htclient) // drag the form
                m.Result = (IntPtr)Htcaption;
        }
        #endregion
    }
}
