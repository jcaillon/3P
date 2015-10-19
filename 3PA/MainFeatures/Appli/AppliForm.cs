using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Fonts;
using YamuiFramework.Forms;
using YamuiFramework.Helper;
using YamuiFramework.Themes;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures.Appli.Pages;
using _3PA.MainFeatures.Appli.Pages.Set;

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

            // create the tabs / content
            var content = new List<YamuiMainMenuTab> {
                new YamuiMainMenuTab("Set", "set", false, new List<YamuiSecMenuTab>() {
                    new YamuiSecMenuTab("ENVIRONMENT", "environment", new SetEnvironment())
                }),
                new YamuiMainMenuTab("Options", "options", true, new List<YamuiSecMenuTab>() {
                    new YamuiSecMenuTab("APPEARANCES", "appearances", new SettingAppearance())
                }),
            };
            CreateContent(content);

            // title
            labelTitle.Text = @"<span style='font-size: 18px'><b>3</b>P : <b>P</b>rogress <b>P</b>rogrammers <b>P</b>al</span>";

            // register to Npp
            FormIntegration.RegisterToNpp(Handle);

            // reorder tab indexes
            (new TabOrderManager(this)).SetTabOrder(TabOrderManager.TabScheme.AcrossFirst);

            Opacity = 0;
            Visible = false;
            Tag = false;
            KeyPreview = true;
        }

        #endregion

        #region Cloack mechanism
        /// <summary>
        /// hides the form
        /// </summary>
        public void Cloack() {
            GiveFocusBack();
            Visible = false;
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
            FormIntegration.UnRegisterToNpp(Handle);
            Tag = true;
            Close();
        }

        /// <summary>
        /// Gives focus back to the owner window
        /// </summary>
        public void GiveFocusBack() {
            //WinApi.SetForegroundWindow(CurrentForegroundWindow);
            Npp.GrabFocus();
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
            if (!HasModalOpened)
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

        protected override void OnClosing(CancelEventArgs e) {
            if (((bool) Tag)) return;
            e.Cancel = true;
            base.OnClosing(e);
            Cloack();
        }
        #endregion

        #region events

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e) {
            if (e.Alt && e.KeyCode == Keys.Space) {
                Cloack();
                e.IsInputKey = false;
            }
            base.OnPreviewKeyDown(e);
        }

        #endregion


        private void yamuiLink6_Click(object sender, EventArgs e) {
            GoToPage("appearances");
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
