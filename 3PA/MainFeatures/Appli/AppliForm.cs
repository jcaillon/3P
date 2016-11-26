#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
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
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Forms;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures.Appli.Pages.Actions;
using _3PA.MainFeatures.Appli.Pages.Home;
using _3PA.MainFeatures.Appli.Pages.Options;
using _3PA.MainFeatures.Appli.Pages.Set;

namespace _3PA.MainFeatures.Appli {

    internal partial class AppliForm : YamuiForm {

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
            CreateContent(new List<YamuiMainMenu> {
                new YamuiMainMenu("TESTS", null, false, new List<YamuiSecMenu> {
                    new YamuiSecMenu("Autocomp", PageNames.Welcome.ToString(), new ProfilesPage())
                }),
                new YamuiMainMenu("Home", null, false, new List<YamuiSecMenu> {
                    new YamuiSecMenu("WELCOME", PageNames.Welcome.ToString(), new HomePage())
                }),
                new YamuiMainMenu("Set", null, false, new List<YamuiSecMenu> {
                    new YamuiSecMenu("ENVIRONMENT", PageNames.SetEnvironment.ToString(), new SetEnvironment()),
                    new YamuiSecMenu("DEPLOYMENT RULES", PageNames.DeploymentRules.ToString(), new SetDeploymentRules()),
                    new YamuiSecMenu("FILE INFORMATION", PageNames.FileInfo.ToString(), new SetFileInfo()),
                    //new YamuiSecMenu("PERSISTENT PROCEDURES", null, new template())
                }),
                new YamuiMainMenu("Actions", null, false, new List<YamuiSecMenu> {
                //    new YamuiSecMenu("CUSTOM SCRIPTS", null, new template()),
                    new YamuiSecMenu("DEPLOY YOUR APPLICATION", PageNames.MassCompiler.ToString(), new DoDeployPage()),
                }),
                new YamuiMainMenu("Options", null, false, new List<YamuiSecMenu> {
                    //new YamuiSecMenu("PROFILES", PageNames.OptionsGeneral.ToString(), new ProfilesPage()),
                    new YamuiSecMenu("GENERAL", PageNames.OptionsGeneral.ToString(), new OptionPage(new List<string> { "General", "Compilation" })),
                    new YamuiSecMenu("COLOR SCHEMES", "colors", new SettingAppearance()),
                    new YamuiSecMenu("UPDATES", "updates", new OptionPage(new List<string> { "Updates" })),
                    new YamuiSecMenu("AUTO-COMPLETION", "autocompletion", new OptionPage(new List<string> { "Auto-completion" })),
                    new YamuiSecMenu("CODE EDITION", "codeedition", new OptionPage(new List<string> { "Code edition" })),
                    new YamuiSecMenu("MISC", PageNames.OptionsMisc.ToString(), new OptionPage(new List<string> { "File explorer", "Code explorer", "Tooltip" })),
                    new YamuiSecMenu("OTHERS", "others", new OthersPage()),
                    new YamuiSecMenu("SHORTCUTS", null, new ShortCutsPage()),
                    new YamuiSecMenu("SHARE/EXPORT CONFIG", PageNames.ExportShareConf.ToString(), new ExportPage())
                })
            });

            CreateTopLinks(new List<string> { "FEEDBACK", "REPORT A BUG", "HELP" }, (sender, tabArgs) => {
                switch (tabArgs.SelectedIndex) {
                    case 0:
                        Process.Start(@"https://github.com/jcaillon/3P/issues/3");
                        break;
                    case 1:
                        Process.Start(@"" + Config.IssueUrl + "");
                        break;
                    case 2:
                        Process.Start(@"http://jcaillon.github.io/3P/");
                        break;
                }
            }, 110, 8);

            // title
            string strongBold = "<span class='AccentColor'>";
            labelTitle.Text = @"<img src='" + ThemeManager.GetLogo + @"' style='padding-right: 10px'><span class='AppliTitle'>" + strongBold + @"P</span>rogress " + strongBold + @"P</span>rogrammers " + strongBold + @"P</span>al</span> <span style='padding-left: 6px; font-size: 12px;' class='SubTextColor'><b>" + AssemblyInfo.Version + (Environment.Is64BitProcess ? " x64" : " x86") + (AssemblyInfo.IsPreRelease ? " (beta)" : "") + (Config.IsDevelopper ? " (debug)" : "") + @"</b></span>";

            // register to Npp
            FormIntegration.RegisterToNpp(Handle);

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
            if (!((bool) Tag)) {
                e.Cancel = true;
                Cloack();
            } else {
                base.OnClosing(e);
            }
        }
        #endregion

    }

    internal enum PageNames {
        Welcome,
        FileInfo,
        OptionsGeneral,
        OptionsMisc,
        ExportShareConf,
        SetEnvironment,
        MassCompiler,
        DeploymentRules
    }
}
