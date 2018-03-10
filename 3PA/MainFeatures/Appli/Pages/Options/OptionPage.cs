#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using YamuiFramework.Controls;
using YamuiFramework.Fonts;
using YamuiFramework.Helper;
using YamuiFramework.HtmlRenderer.WinForms;
using YamuiFramework.Themes;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletionFeature;
using _3PA.NppCore;
using _3PA._Resource;

namespace _3PA.MainFeatures.Appli.Pages.Options {
    /// <summary>
    /// This page is built programatically
    /// </summary>
    internal partial class OptionPage : YamuiPage {

        #region fields

        private List<string> _allowedGroups;

        private AsapButDelayableAction _saveAction;

        #endregion

        #region constructor

        public OptionPage(List<string> allowedGroups) {
            InitializeComponent();
            _allowedGroups = allowedGroups;
            _saveAction = new AsapButDelayableAction(2000, () => {
                this.SafeInvoke(page => {
                    Save();
                });
            });
            GeneratePage();
        }

        #endregion

        #region private methods

        /// <summary>
        /// Dynamically generates the page
        /// </summary>
        private void GeneratePage() {

            var lastCategory = "";
            var yPos = 0;

            ForEachConfigPropertyWithDisplayAttribute((property, attribute) => {

                // new group
                if (!lastCategory.EqualsCi(attribute.GroupName)) {
                    if (!string.IsNullOrEmpty(lastCategory))
                        // ReSharper disable once AccessToModifiedClosure
                        yPos += 10;
                    lastCategory = attribute.GroupName;
                    Controls.Add(new YamuiLabel {
                        AutoSize = true,
                        Function = FontFunction.Heading,
                        Location = new Point(0, yPos),
                        Text = lastCategory.ToUpper()
                    });
                    yPos += 30;
                }

                Controls.Add(InsertInputForItem(property, attribute, ref yPos));
                Controls.Add(InsertLabelForItem(attribute, ref yPos));
            });

            yPos += 15;
            
            // add a button for the updates
            if (_allowedGroups.Contains("Updates")) {

                var updateButton = new YamuiButton {
                    Location = new Point(30, yPos),
                    Size = new Size(150, 24),
                    Text = @"Check for 3P updates",
                    BackGrndImage = ImageResources.Update
                };
                updateButton.ButtonPressed += (sender, args) => Updater<MainUpdaterWrapper>.Instance.CheckForUpdate();
                tooltip.SetToolTip(updateButton, "Click to <b>check for updates</b>");
                Controls.Add(updateButton);

                updateButton = new YamuiButton {
                    Location = new Point(185, yPos),
                    Size = new Size(170, 24),
                    Text = @"Check for Prolint updates",
                    BackGrndImage = ImageResources.ProlintCode
                };
                updateButton.ButtonPressed += (sender, args) => {
                    Updater<ProlintUpdaterWrapper>.Instance.CheckForUpdate();
                    Updater<ProparseUpdaterWrapper>.Instance.CheckForUpdate();
                };
                tooltip.SetToolTip(updateButton, "Click to <b>check for updates</b>");
                Controls.Add(updateButton);

                yPos += updateButton.Height + 5;
            }

            // add a button for the updates
            if (_allowedGroups.Contains("General")) {

                var button = new YamuiButton {
                    Location = new Point(30, yPos),
                    Size = new Size(170, 24),
                    Text = @"Modify notepad++ options"
                };
                button.ButtonPressed += (sender, args) => Npp.ConfXml.ModifyingNppConfig();
                tooltip.SetToolTip(button, "Click to <b>modify notepad++ options</b>");
                Controls.Add(button);
                
                yPos += button.Height + 5;
            }

            Height = yPos + 50;
        }

        /// <summary>
        /// Insert the option label
        /// </summary>
        private HtmlLabel InsertLabelForItem(Config.ConfigAttribute attr, ref int yPos) {
            var lbl = new HtmlLabel {
                AutoSizeHeightOnly = true,
                BackColor = Color.Transparent,
                Location = new Point(30, yPos),
                Size = new Size(270, 10),
                IsSelectionEnabled = false,
                Text = attr.Label
            };

            yPos += lbl.Height + 10;

            return lbl;
        }

        /// <summary>
        /// Insert the correct input for the option
        /// </summary>
        private Control InsertInputForItem(FieldInfo property, Config.ConfigAttribute attr, ref int yPos) {
            // Build control type
            Control retVal;
            
            if (property.FieldType == typeof(bool)) {
                // for bool
                var tg = new YamuiButtonToggle {
                    Location = new Point(320, yPos),
                    Size = new Size(40, 16),
                    Text = null,
                    Checked = (bool)property.GetValue(Config.Instance)
                };
                tg.ButtonPressed += (sender, args) => OnFieldModified();
                retVal = tg;

            } else if (property.FieldType.IsEnum) {
                // for enum
                var dataSource = new List<string>();
                foreach (var name in Enum.GetNames(property.FieldType)) {
                    var attribute = Attribute.GetCustomAttribute(property.FieldType.GetField(name), typeof(DescriptionAttribute), true) as DescriptionAttribute;
                    dataSource.Add(attribute != null ? attribute.Description : name);
                }
                dataSource = dataSource.Select(s => s.Replace("_", " ").Trim()).ToNonNullList();
                var cb = new YamuiComboBox {
                    Location = new Point(320, yPos),
                    Size = new Size(Math.Min(300, dataSource.Select(s => TextRenderer.MeasureText(s, FontManager.GetStandardFont()).Width).Max() + 25), 20),
                    DataSource = dataSource,
                };
                cb.SelectedIndex = Enum.GetNames(property.FieldType).IndexOf(property.GetValue(Config.Instance).ConvertToStr());
                cb.SelectedIndexChangedByUser += box => OnFieldModified();
                retVal = cb;

            } else {
                // for everything else
                var tb = new YamuiTextBox {
                    Location = new Point(320, yPos),
                    Size = new Size(300, 20),
                    Text = property.GetValue(Config.Instance).ConvertToStr(),
                    Multiline = false,
                    CausesValidation = true
                };
                tb.Enter += (s, e) => tb.SelectAll();
                if (property.FieldType == typeof(char))
                    tb.KeyPress += (s, e) => {
                        e.Handled = !char.IsControl(e.KeyChar) && tb.TextLength > 0;
                    };
                else
                    tb.KeyPress += (s, e) => {
                        e.Handled = Utilities.IsInvalidKey(e.KeyChar, property.FieldType);
                    };
                tb.Validating += ValidateTextBox;
                tb.Validated += (s, e) => errorProvider.SetError(tb, "");
                tb.TextChanged += (sender, args) => OnFieldModified();
                errorProvider.SetIconPadding(tb, -18);
                errorProvider.Icon = ImageResources.IcoError;
                retVal = tb;
            }

            var undoButton = new YamuiButtonImage {
                BackGrndImage = ImageResources.UndoUserAction,
                Size = new Size(20, 20),
                Location = new Point(retVal.Left + retVal.Width + 5, yPos),
                Tag = retVal,
                TabStop = false
            };
            undoButton.ButtonPressed += OnUndoButton;
            Controls.Add(undoButton);
            tooltip.SetToolTip(undoButton, "Click to <b>reset this field</b> to its default value");

            // add tooltip on the control
            if (!string.IsNullOrEmpty(attr.Tooltip)) {
                var rangeAttr = (RangeAttribute)property.GetCustomAttributes(typeof(RangeAttribute), false).FirstOrDefault();
                tooltip.SetToolTip(retVal, "<b>" + attr.Label + ":</b><br><br>" + attr.Tooltip + (rangeAttr != null ? "<br><b><i>" + "Min value = " + rangeAttr.Minimum + "<br>Max value = " + rangeAttr.Maximum + "</i></b>" : ""));
            }

            // Set standard props
            retVal.Name = "option_" + property.Name;
            retVal.Tag = property;
            
            return retVal;
        }

        /// <summary>
        /// Validates the content of a text box (set event.Cancel to true if invalid)
        /// </summary>
        private void ValidateTextBox(object o, CancelEventArgs cancelEventArgs) {
            var tb = (YamuiTextBox) o;
            var property = (FieldInfo) tb.Tag;
            var rangeAttr = (RangeAttribute)property.GetCustomAttributes(typeof(RangeAttribute), false).FirstOrDefault();

            bool invalid = false;
            if (!string.IsNullOrEmpty(tb.Text)) {
                invalid = !tb.Text.CanConvertToType(property.FieldType);
            }
            
            if (invalid) {
                errorProvider.SetError(tb, "The value has an invalid format for <" + property.FieldType.Name + ">.");
            } else {
                // for numbers, check limit values
                if (property.FieldType == typeof(int) || property.FieldType == typeof(double)) {
                    if (rangeAttr != null) {
                        double ouptut = (double) tb.Text.ConvertFromStr(typeof(double));
                        double maxRange = 0;
                        double minRange = 0;
                        try {
                            maxRange = (double)rangeAttr.Maximum;
                            minRange = (double)rangeAttr.Minimum;
                        } catch (Exception ex) {
                            if (ex is InvalidCastException) {
                                maxRange = (int)rangeAttr.Maximum;
                                minRange = (int)rangeAttr.Minimum;
                            }
                        }
                        if (ouptut > maxRange || ouptut < minRange) {
                            invalid = true;
                            errorProvider.SetError(tb, "The value should be between " + minRange + " to " + maxRange + ".");
                        }
                    }
                }
            }
            if (!invalid)
                errorProvider.SetError(tb, "");
            cancelEventArgs.Cancel = invalid;
        }

        /// <summary>
        /// For certain config properties, we need to refresh stuff to see a difference
        /// </summary>
        private void ApplySettings() {
            YamuiThemeManager.TabAnimationAllowed = Config.Instance.AppliAllowTabAnimation;
            CodeExplorer.CodeExplorer.Instance.ApplyColorSettings();
            FileExplorer.FileExplorer.Instance.ApplyColorSettings();
            AutoCompletion.ForceClose();
            InfoToolTip.InfoToolTip.ForceClose();
            Plug.ApplyOptionsForScintilla();
            Sci.MouseDwellTime = Config.Instance.ToolTipmsBeforeShowing;
            Keywords.Instance.ResetCompletionItems(); // when changing case
            DataBase.Instance.ResetCompletionItems();
        }

        /// <summary>
        /// Execute an action for each property of the config object that has a display attribute
        /// </summary>
        /// <param name="action"></param>
        private void ForEachConfigPropertyWithDisplayAttribute(Action<FieldInfo, Config.ConfigAttribute> action) {
            var properties = typeof(Config.ConfigObject).GetFields();

            /* loop through fields */
            foreach (var property in properties) {
                if (property.IsPrivate) {
                    continue;
                }
                var listCustomAttr = property.GetCustomAttributes(typeof(Config.ConfigAttribute), false);
                if (!listCustomAttr.Any()) {
                    continue;
                }
                var attr = (Config.ConfigAttribute)listCustomAttr.FirstOrDefault();
                if (attr == null) {
                    continue;
                }
                if (string.IsNullOrEmpty(attr.Label) || !_allowedGroups.Contains(attr.GroupName, StringComparer.CurrentCultureIgnoreCase)) {
                    continue;
                }

                // execute the action with the loop property and display attribute
                action(property, attr);
            }
        }

        #endregion

        #region on events

        /// <summary>
        /// Called when an option is modified
        /// </summary>
        private void OnFieldModified() {
            _saveAction.DoDelayable();
        }

        /// <summary>
        /// Called to save all the options of the page
        /// </summary>
        private void Save() {
            bool needApplySetting = false;

            // refresh stuff on screen
            foreach (Control inputControl in Controls) {
                if (inputControl.Name.StartsWith("option_")) {
                    
                    var property = (FieldInfo)inputControl.Tag;
                    var attr = property.GetCustomAttributes(typeof(Config.ConfigAttribute), false).FirstOrDefault() as Config.ConfigAttribute;
                    object inputValue = null;

                    if (inputControl is YamuiComboBox) {
                        inputValue = Enum.GetNames(property.FieldType).ToList()[((YamuiComboBox)inputControl).SelectedIndex].ConvertFromStr(property.FieldType);
                    } else if (inputControl is YamuiButtonToggle) {
                        inputValue = ((YamuiButtonToggle)inputControl).Checked;
                    } else if (inputControl is YamuiTextBox) {
                        var cancelArg = new CancelEventArgs();
                        ValidateTextBox(inputControl, cancelArg);
                        if (cancelArg.Cancel) {
                            continue;
                        }
                        inputValue = ((YamuiTextBox) inputControl).Text.ConvertFromStr(property.FieldType);    
                    }

                    Config.Instance.SetValueOf(property.Name, inputValue);

                    needApplySetting = needApplySetting || attr != null && attr.NeedApplySetting;
                }
            }

            // need to refresh stuff to really apply this option?
            if (needApplySetting)
                ApplySettings();

            Config.Save();
            Appli.Notify("Options saved successfully", 2);
        }
        
        /// <summary>
        /// Reset an option, setting it to its default value
        /// </summary>
        private void OnUndoButton(object sender, EventArgs eventArgs) {
            var inputControl = (Control) ((YamuiButtonImage)sender).Tag;
            var property = (FieldInfo) inputControl.Tag;
            var defaultValue = new Config.ConfigObject().GetValueOf(property.Name);

            if (inputControl is YamuiComboBox) {
                ((YamuiComboBox)inputControl).SelectedIndex = Enum.GetNames(property.FieldType).IndexOf(defaultValue.ConvertToStr());
            } else if (inputControl is YamuiButtonToggle) {
                ((YamuiButtonToggle)inputControl).Checked = (bool)defaultValue;
            } else if (inputControl is YamuiTextBox) {
                ((YamuiTextBox)inputControl).Text = defaultValue.ConvertToStr();
            }

            OnFieldModified();
        }
        
        #endregion

    }
}