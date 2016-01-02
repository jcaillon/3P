using System;
using System.Collections.Generic;
using System.Windows.Forms;
using YamuiDemoApp.Pages.control;
using YamuiDemoApp.Pages.Navigation;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Forms;
using YamuiFramework.Themes;
using _3PA.MainFeatures.Appli.Pages.Options;

namespace YamuiDemoApp {
    public partial class Form1 : YamuiForm {
        public Form1() {
            InitializeComponent();

            CreateContent(new List<YamuiMainMenu> {
                new YamuiMainMenu("Control", "control", false, new List<YamuiSecMenu> {
                    new YamuiSecMenu("Classic", "classic", new Classic()),
                    new YamuiSecMenu("Item controls", "controls", new ItemControl())
                }),
                new YamuiMainMenu("Settings", null, true, new List<YamuiSecMenu> {
                    new YamuiSecMenu("yamuiTabSecAppearance", "yamuiTabSecAppearance", new SettingAppearance()),
                }),
                new YamuiMainMenu("Navigation", "navigation", false, new List<YamuiSecMenu> {
                    new YamuiSecMenu("Other", "other", new Other()),
                }),
            });

            CreateTopLinks(new List<string> {"APPEARANCES", "NOTIFICATIONS", "TEST"}, (sender, tabArgs) => {
                switch (tabArgs.SelectedIndex) {
                    case 0:
                        ShowPage("yamuiTabSecAppearance");
                        break;
                    case 1:
                        var toastNotification = new YamuiNotifications("<img src='high_importance' />This is a notification test", 5);
                        toastNotification.Show();
                        var toastNotification2 = new YamuiNotifications("<img src='poison' />Can i display a link? <br><a href='plswork?'>yop</a>", 0);
                        toastNotification2.LinkClicked += (o, args) => {
                        MessageBox.Show(args.Link);
                        };
                        toastNotification2.Show();
                        break;
                    case 2:
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
                        break;
                }
            });
        }

        private static bool _lab = true;

    }
}
