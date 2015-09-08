using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Forms;
using YamuiFramework.Helper;
using YamuiFramework.Themes;
using _3PA.Interop;
using _3PA.Lib;

namespace _3PA.MainFeatures.Appli {
    public partial class AppliForm : YamuiForm {

        #region fields
        /// <summary>
        /// Should be set when you create the new form
        /// CurrentForegroundWindow = WinApi.GetForegroundWindow();
        /// </summary>
        public IntPtr CurrentForegroundWindow;
        private bool _allowshowdisplay;
        #endregion

        #region constructor

        public AppliForm() {
            InitializeComponent();

            // register to Npp
            FormIntegration.RegisterToNpp(Handle);

            Opacity = 0;
            Visible = false;
            Tag = false;
            Closing += OnClosing;
        }

        #endregion

        #region Cloack mechanism
        /// <summary>
        /// hides the form
        /// </summary>
        public void Cloack() {
            Visible = false;
            GiveFocusBack();
        }

        /// <summary>
        /// show the form
        /// </summary>
        public void UnCloack() {
            Opacity = 1;
            Visible = true;
        }

        /// <summary>
        /// Call this method instead of Close() to really close this form
        /// </summary>
        public void ForceClose() {
            Tag = true;
            Close();
        }

        /// <summary>
        /// instead of closing, cloak this form (invisible)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="cancelEventArgs"></param>
        private void OnClosing(object sender, CancelEventArgs cancelEventArgs) {
            if ((bool) Tag) return;
            cancelEventArgs.Cancel = true;
            Cloack();
        }

        /// <summary>
        /// Gives focus back to the owner window
        /// </summary>
        public void GiveFocusBack() {
            WinApi.SetForegroundWindow(CurrentForegroundWindow);
            Opacity = Config.Instance.AppliOpacityUnfocused;
        }

        /// <summary>
        /// When the form gets activated..
        /// </summary>
        /// <param name="e"></param>
        protected override void OnActivated(EventArgs e) {
            Opacity = 1;
            base.OnActivated(e);
        }

        protected override void OnDeactivate(EventArgs e) {
            Opacity = Config.Instance.AppliOpacityUnfocused;
            base.OnDeactivate(e);
        }

        /// <summary>
        /// This ensures the form is never visible at start
        /// </summary>
        /// <param name="value"></param>
        protected override void SetVisibleCore(bool value) {
            base.SetVisibleCore(_allowshowdisplay ? value : _allowshowdisplay);
        }

        /// <summary>
        /// should be called after Show() or ShowDialog() for a sweet animation
        /// </summary>
        public void DoShow() {
            _allowshowdisplay = true;
            Visible = true;
            Opacity = 0;
            Transition.run(this, "Opacity", 1d, new TransitionType_Acceleration(200));
        }
        #endregion


        private void yamuiTabControl1_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void yamuiTabPage9_Click(object sender, EventArgs e) {

        }

        private void Form1_Load(object sender, EventArgs e) {
            //ApplyHideSettingGlobally(this);
        }

        private void yamuiLink6_Click(object sender, EventArgs e) {
            GoToPage("yamuiTabSecAppearance");
        }

        private void yamuiLink7_Click(object sender, EventArgs e) {
            var toastNotification = new YamuiNotifications("<img src='high_importance' />This is a notification test", 5);
            toastNotification.Show();
            var toastNotification2 = new YamuiNotifications("<img src='poison' />Can i display a link? <br><a href='plswork?'>yop</a>", 0);
            toastNotification2.LinkClicked += (o, args) => {
                MessageBox.Show(args.Link);
            };
            toastNotification2.Show();
        }

        private void classic1_Load(object sender, EventArgs e) {

        }

        private void text1_Load(object sender, EventArgs e) {

        }

        private static bool _lab = true;
        private void yamuiLink8_Click(object sender, EventArgs e) {
            statusLabel.UseCustomForeColor = true;
            statusLabel.ForeColor = ThemeManager.Current.LabelsColorsNormalForeColor;
            var t = new Transition(new TransitionType_Linear(500));
            if (_lab) 
                t.add(statusLabel, "Text", "Hello world!");
            else
                t.add(statusLabel, "Text", "<b>WARNING :</b> this user is awesome");
            t.add(statusLabel, "ForeColor", ThemeManager.AccentColor);
            t.TransitionCompletedEvent += (o, args) => {
                Transition.run(statusLabel, "ForeColor", ThemeManager.Current.LabelsColorsNormalForeColor, new TransitionType_CriticalDamping(400));
            };
            t.run();
            _lab = !_lab;
        }
    }
}
