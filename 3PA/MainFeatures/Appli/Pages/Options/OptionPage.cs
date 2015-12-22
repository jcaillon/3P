#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (OptionPage.cs) is part of 3P.
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
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Fonts;
using YamuiFramework.HtmlRenderer.WinForms;
using YamuiFramework.Themes;
using _3PA.Images;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;

namespace _3PA.MainFeatures.Appli.Pages.Options {

    /// <summary>
    /// This page is built programatically
    /// </summary>
    public partial class OptionPage : YamuiPage {

        #region constructor
        public OptionPage() {
            InitializeComponent();

            // avoid glitch when using background image
            NoTransparentBackground = true;

            GeneratePage();
        }

        #endregion

        #region private

        private void GeneratePage() {
            var lastCategory = "";
            var yPos = 0;
            var configInstance = Config.Instance;

            ForEachConfigPropertyWithDisplayAttribute((property, attribute) => {
                var valObj = property.GetValue(configInstance);

                // new group
                if (!lastCategory.EqualsCi(attribute.GroupName)) {
                    if (!string.IsNullOrEmpty(lastCategory))
                        yPos += 15;
                    lastCategory = attribute.GroupName;
                    dockedPanel.Controls.Add(new YamuiLabel {
                        AutoSize = true,
                        Function = LabelFunction.Heading,
                        Location = new Point(0, yPos),
                        Text = lastCategory.ToUpper()
                    });
                    yPos += 30;
                }

                // name of the field
                var label = new HtmlLabel {
                    AutoSizeHeightOnly = true,
                    BackColor = Color.Transparent,
                    Location = new Point(30, yPos),
                    Size = new Size(190, 10),
                    IsSelectionEnabled = false,
                    Text = attribute.Name
                };
                dockedPanel.Controls.Add(label);

                var listRangeAttr = property.GetCustomAttributes(typeof(RangeAttribute), false);
                var rangeAttr = (listRangeAttr.Any()) ? (RangeAttribute)listRangeAttr.FirstOrDefault() : null;
                if (rangeAttr != null && ((valObj is int || valObj is double)))
                    tooltip.SetToolTip(label, attribute.Description + "<br><br><b><i>" + "Min value = " + rangeAttr.Minimum + "<br>Max value = " + rangeAttr.Maximum + "</i></b>");
                else
                    tooltip.SetToolTip(label, attribute.Description);


                if (valObj is string) {
                    // string
                    var strControl = new YamuiTextBox {
                        Lines = new string[0],
                        Location = new Point(240, yPos),
                        ScrollBars = ScrollBars.None,
                        SelectedText = (string)property.GetValue(configInstance),
                        Size = new Size(300, 20),
                        Tag = attribute.Name
                    };
                    dockedPanel.Controls.Add(strControl);
                    var strButton = new YamuiImageButton {
                        BackGrndImage = ImageResources.Save,
                        Size = new Size(20, 20),
                        Location = new Point(545, yPos),
                        Tag = strControl
                    };
                    strButton.ButtonPressed += NumButtonOnButtonPressed;
                    tooltip.SetToolTip(strButton, "Click to <b>set the value</b> of this field<br>Otherwise, your modifications will not be saved");
                    dockedPanel.Controls.Add(strButton);

                } if (valObj is int || valObj is double) {
                    // number
                    var numControl = new YamuiTextBox {
                        Lines = new string[0],
                        Location = new Point(240, yPos),
                        ScrollBars = ScrollBars.None,
                        SelectedText = ((valObj is int) ? ((int)property.GetValue(configInstance)).ToString() : ((double)property.GetValue(configInstance)).ToString(CultureInfo.CurrentCulture)),
                        Size = new Size(300, 20),
                        Tag = attribute.Name
                    };
                    dockedPanel.Controls.Add(numControl);
                    var numButton = new YamuiImageButton {
                        BackGrndImage = ImageResources.Save,
                        Size = new Size(20, 20),
                        Location = new Point(545, yPos),
                        Tag = numControl
                    };
                    numButton.ButtonPressed += NumButtonOnButtonPressed;
                    tooltip.SetToolTip(numButton, "Click to <b>set the value</b> of this field<br>Otherwise, your modifications will not be saved");
                    dockedPanel.Controls.Add(numButton);

                } else if (valObj is bool) {
                    // bool
                    var toggleControl = new YamuiToggle {
                        Location = new Point(240, yPos),
                        Size = new Size(80, 15),
                        Text = @" ",
                        Checked = (bool)valObj,
                        Tag = attribute.Name
                    };
                    toggleControl.CheckedChanged += ToggleControlOnCheckedChanged;
                    tooltip.SetToolTip(toggleControl, "Click to <b>toggle on/off</b> this feature<br>Your choice is automatically applied");
                    dockedPanel.Controls.Add(toggleControl);
                }


                yPos += label.Height + 20;
            });

            yPos += 40;
            var defaultButton = new YamuiButton {
                Location = new Point(30, yPos),
                Size = new Size(94, 23),
                Text = @"To default"
            };
            defaultButton.ButtonPressed += DefaultButtonOnButtonPressed;
            tooltip.SetToolTip(defaultButton, "Click to <b>reset</b> all the options to default");
            dockedPanel.Controls.Add(defaultButton);
            yPos += 50;

            dockedPanel.Controls.Add(new YamuiLabel {
                AutoSize = true,
                Function = LabelFunction.Heading,
                Location = new Point(0, yPos),
                Text = @" "
            });

            Height = yPos + 20;

        }

        private void DefaultButtonOnButtonPressed(object sender, ButtonPressedEventArgs buttonPressedEventArgs) {

            // apply default settings
            var defaultConfig = new ConfigObject();
            var configInstance = Config.Instance;
            ForEachConfigPropertyWithDisplayAttribute((property, attribute) => {
                var valObj = property.GetValue(defaultConfig);
                property.SetValue(configInstance, valObj);
                foreach (var control in dockedPanel.Controls) {
                    var ctrl = (Control) control;
                    if (ctrl.Tag != null && ctrl.Tag is string) {
                        if (((string) ctrl.Tag).Equals(attribute.Name)) {
                            if (ctrl is YamuiToggle)
                                ((YamuiToggle) ctrl).Checked = (bool) valObj;
                            else
                                ((YamuiTextBox)ctrl).Text = valObj.ToString();
                        }
                    }
                }
            });

            ApplySettings();
        }

        private void NumButtonOnButtonPressed(object sender, ButtonPressedEventArgs buttonPressedEventArgs) {
            var textBox = (YamuiTextBox) ((YamuiImageButton) sender).Tag;
            var propertyName = (string) textBox.Tag;
            SetPropertyValue(propertyName, textBox);
        }

        private void ToggleControlOnCheckedChanged(object sender, EventArgs eventArgs) {
            var toggle = (YamuiToggle) sender;
            var propertyName = (string) toggle.Tag;
            SetPropertyValue(propertyName, toggle);
        }

        /// <summary>
        /// Sets the value or a property that has a display name "propertyName"
        /// to the value "value"
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="sender"></param>
        private void SetPropertyValue(string propertyName, object sender) {
            var configInstance = Config.Instance;

            ForEachConfigPropertyWithDisplayAttribute((property, attribute) => {
                if (propertyName.Equals(attribute.Name)) {
                    var valObj = property.GetValue(configInstance);
                    if (valObj is bool) {
                        property.SetValue(configInstance, ((YamuiToggle)sender).Checked);
                    } else {
                        if (valObj is string) {
                            property.SetValue(configInstance, ((YamuiTextBox)sender).Text);
                        } else {
                            double ouptut;
                            if (!double.TryParse(((YamuiTextBox)sender).Text, out ouptut)) {
                                BlinkTextBox((YamuiTextBox)sender, ThemeManager.Current.GenericErrorColor);
                                return;
                            }
                            var listRangeAttr = property.GetCustomAttributes(typeof(RangeAttribute), false);
                            var rangeAttr = (listRangeAttr.Any()) ? (RangeAttribute)listRangeAttr.FirstOrDefault() : null;
                            if (rangeAttr != null) {
                                if (ouptut > (int)rangeAttr.Maximum || ouptut < (int)rangeAttr.Minimum) {
                                    BlinkTextBox((YamuiTextBox)sender, ThemeManager.Current.GenericErrorColor);
                                    return;
                                }
                            }
                            if (valObj is int)
                                property.SetValue(configInstance, (int)ouptut);
                            else
                                property.SetValue(configInstance, ouptut);
                        }
                    }

                    if (attribute.AutoGenerateField) {
                        // need to refresh stuff to really apply this option
                        ApplySettings();
                    }
                }
            });
        }

        /// <summary>
        /// For certain config properties, we need to refresh stuff to see a difference
        /// </summary>
        private void ApplySettings() {
            ThemeManager.TabAnimationAllowed = Config.Instance.AppliAllowTabAnimation;
            CodeExplorer.CodeExplorer.ExplorerForm.UseAlternateBackColor = Config.Instance.GlobalUseAlternateBackColorOnGrid;
            FileExplorer.FileExplorer.ExplorerForm.UseAlternateBackColor = Config.Instance.GlobalUseAlternateBackColorOnGrid;
            CodeExplorer.CodeExplorer.RedrawCodeExplorerList();
            FileExplorer.FileExplorer.RedrawFileExplorerList();
            AutoComplete.ForceClose();
            InfoToolTip.InfoToolTip.ForceClose();
            Plug.ApplyPluginSpecificOptions(false);

            Npp.MouseDwellTime = Config.Instance.ToolTipmsBeforeShowing;
        }

        /// <summary>
        /// Makes the given textbox blink
        /// </summary>
        /// <param name="textBox"></param>
        /// <param name="blinkColor"></param>
        private void BlinkTextBox(YamuiTextBox textBox, Color blinkColor) {
            textBox.UseCustomBackColor = true;
            Transition.run(textBox, "BackColor", ThemeManager.Current.ButtonColorsNormalBackColor, blinkColor, new TransitionType_Flash(3, 300), (o, args) => { textBox.UseCustomBackColor = false; });
        }

        /// <summary>
        /// Execute an action for each property of the config object that has a display attribute
        /// </summary>
        /// <param name="action"></param>
        private void ForEachConfigPropertyWithDisplayAttribute(Action<FieldInfo, DisplayAttribute> action) {
            var properties = typeof (ConfigObject).GetFields();

            /* loop through fields */
            foreach (var property in properties) {
                if (property.IsPrivate) continue;
                var listCustomAttr = property.GetCustomAttributes(typeof (DisplayAttribute), false);
                if (!listCustomAttr.Any()) 
                    continue;
                var displayAttr = (DisplayAttribute) listCustomAttr.FirstOrDefault();
                if (displayAttr == null)
                    continue;
                if (string.IsNullOrEmpty(displayAttr.Name))
                    continue;
                // execute the action with the loop property and display attribute
                action(property, displayAttr);
            }
        }

        #endregion




    }
}
