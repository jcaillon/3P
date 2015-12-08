#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (AppliForm.cs) is part of 3P.
// 
// 3P is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// 3P is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with 3P. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Forms;
using YamuiFramework.Helper;
using _3PA.Html;
using _3PA.Interop;
using _3PA.MainFeatures.Appli.Pages;
using _3PA.MainFeatures.Appli.Pages.Options;
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
                new YamuiMainMenuTab("Set", "set", false, new List<YamuiSecMenuTab> {
                    new YamuiSecMenuTab("ENVIRONMENT", "environment", new SetEnvironment())
                }),
                new YamuiMainMenuTab("Options", "options", false, new List<YamuiSecMenuTab> {
                    new YamuiSecMenuTab("APPEARANCES", "appearances", new SettingAppearance())
                }),
                new YamuiMainMenuTab("About 3P", "about", true, new List<YamuiSecMenuTab> {
                    new YamuiSecMenuTab("SOFTWARE INFORMATION", "soft_info", new PageAbout())
                })
            };
            CreateContent(content);

            // title
            UpdateTitle();

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

        #region Public methods

        /// <summary>
        /// Updates the title of the main form
        /// </summary>
        public void UpdateTitle() {
            string strongBold = "<span class='AccentColor'>";
            labelTitle.Text = @"<img src='" + LocalHtmlHandler.GetLogo() + @"' style='padding-right: 10px'><span class='AppliTitle'>" + strongBold + @"P</span>rogress " + strongBold + @"P</span>rogrammers " + strongBold + @"P</span>al</span>";
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
            if ((e.Alt && e.KeyCode == Keys.Space) || e.KeyCode == Keys.Escape) {
                Cloack();
                e.IsInputKey = false;
            }
            base.OnPreviewKeyDown(e);
        }      

        private void yamuiLink6_Click(object sender, EventArgs e) {
            GoToPage("appearances");
        }

        private void yamuiLink7_Click(object sender, EventArgs e) {
            GoToPage("soft_info");
        }

        private void yamuiLink8_Click(object sender, EventArgs e) {
            Process.Start(@"http://jcaillon.github.io/3P/");
            /*
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
             * */
        }

        #endregion

    }
}
