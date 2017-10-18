#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (InfoToolTip.cs) is part of 3P.
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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using YamuiFramework.Helper;
using YamuiFramework.HtmlRenderer.Core.Core.Entities;
using _3PA.MainFeatures.AutoCompletionFeature;
using _3PA.MainFeatures.Parser;
using _3PA.MainFeatures.Pro;
using _3PA.MainFeatures.SyntaxHighlighting;
using _3PA.NppCore;
using _3PA.WindowsCore;

namespace _3PA.MainFeatures.InfoToolTip {
    internal static class InfoToolTip {

        #region fields

        // The tooltip form
        private static InfoToolTipForm _form;

        // we save the conditions with which we showed the tooltip to be able to update it as is
        private static List<CompletionItem> _currentCompletionList;

        /// <summary>
        /// Was the form opened because the user left his mouse too long on a word?
        /// </summary>
        private static bool _openedFromDwell;

        /// <summary>
        /// Was the form displayed for an autocompletion item?
        /// </summary>
        private static bool _openedForCompletion;

        /// <summary>
        /// If a tooltip is opened and it's a parsed item, this point leads to its definition
        /// </summary>
        public static Point GoToDefinitionPoint = new Point(-1, -1);

        public static string GoToDefinitionFile;

        /// <summary>
        /// the current word that the tooltip displays
        /// </summary>
        public static string CurrentWord;

        /// <summary>
        /// Index of the tooltip to show in case where a word corresponds to several items in the
        /// CompletionItem list
        /// </summary>
        public static int IndexToShow;

        /// <summary>
        /// is used to make sure that we finish to display a tooltip before trying to display another one
        /// </summary>
        private static object _thisLock = new object();

        #endregion

        #region public misc

        /// <summary>
        /// Returns the current CompletionItem used in the tooltip
        /// </summary>
        /// <returns></returns>
        public static CompletionItem GetCurrentlyDisplayedCompletionData() {
            if (_currentCompletionList == null)
                return null;
            if (IndexToShow < 0)
                IndexToShow = _currentCompletionList.Count - 1;
            if (IndexToShow >= _currentCompletionList.Count)
                IndexToShow = 0;
            return _currentCompletionList.ElementAt(IndexToShow);
        }

        #endregion

        #region Tooltip

        /// <summary>
        /// Method called when the tooltip is opened from the mouse being inactive on scintilla
        /// </summary>
        public static void ShowToolTipFromDwell(bool openTemporary = true) {
            if (Config.Instance.ToolTipDeactivate) return;
            InitIfneeded();

            var position = Sci.GetPositionFromMouseLocation();
            if (position < 0)
                return;

            // check caret context, dont display a tooltip for comments
            var curContext = (SciStyleId) Sci.GetStyleAt(position);
            if (curContext == SciStyleId.Comment)
                return;

            // sets the tooltip content
            var data = AutoCompletion.FindInCompletionData(Sci.GetWordAtPosition(position, AutoCompletion.CurrentLangAllChars, AutoCompletion.CurrentLangAdditionalChars), Sci.LineFromPosition(position));
            if (data != null && data.Count > 0)
                _currentCompletionList = data;
            else
                return;

            // in strings, only functions trigger the tooltip
            if ((curContext == SciStyleId.DoubleQuote || curContext == SciStyleId.SimpleQuote) && _currentCompletionList.First().Type != CompletionType.Function)
                return;

            SetToolTip();

            // update position
            var point = Sci.GetPointXyFromPosition(position);
            point.Offset(Sci.GetScintillaRectangle().Location);
            var lineHeight = Sci.TextHeight(Sci.Line.CurrentLine);
            point.Y += lineHeight + 5;
            _form.Location = _form.GetBestAutocompPosition(point, lineHeight + 5);

            _openedFromDwell = openTemporary;
            if (!_form.Visible) {
                _form.UnCloak();
            }
        }

        /// <summary>
        /// Called when a tooltip is shown and the user presses CTRL + down/up to show 
        /// the other definitions available
        /// </summary>
        public static void TryToShowIndex() {
            if (_currentCompletionList == null) return;

            // refresh tooltip with the correct index
            _form.Cloak();
            SetToolTip();
            if (!_form.Visible)
                _form.UnCloak();
        }

        /// <summary>
        /// Method called when the tooltip is opened to help the user during autocompletion
        /// </summary>
        public static void ShowToolTipFromAutocomplete(CompletionItem item, AutoCompletionForm parentForm) {
            if (Config.Instance.ToolTipDeactivate)
                return;

            bool lockTaken = false;
            try {
                Monitor.TryEnter(_thisLock, 0, ref lockTaken);
                if (!lockTaken) return;

                InitIfneeded();

                // sets the tooltip content
                _currentCompletionList = new List<CompletionItem> {item};
                SetToolTip();

                // update position
                _form.Location = parentForm.GetToolTipBestPosition(_form.Size);

                _openedFromDwell = false;
                _openedForCompletion = true;
                if (!_form.Visible)
                    _form.UnCloak();
            } finally {
                if (lockTaken) Monitor.Exit(_thisLock);
            }
        }

        /// <summary>
        /// Handles the clicks on the link displayed in the tooltip
        /// </summary>
        /// <param name="htmlLinkClickedEventArgs"></param>
        private static void ClickHandler(HtmlLinkClickedEventArgs htmlLinkClickedEventArgs) {
            var split = htmlLinkClickedEventArgs.Link.Split('#');
            var actionType = split[0];
            bool handled = true;
            switch (actionType) {
                case "gotoownerfile":
                    if (split.Length > 1) {
                        Npp.Goto(split[1]);
                        Cloak();
                    }
                    break;
                case "trigger":
                    if (split.Length > 1) {
                        var fullPath = ProEnvironment.Current.FindFirstFileInEnv(split[1]);
                        Npp.Goto(string.IsNullOrEmpty(fullPath) ? split[1] : fullPath);
                        Cloak();
                    }
                    break;
                case "proto":
                    if (split.Length > 3) {
                        Npp.Goto(split[1], int.Parse(split[2]), int.Parse(split[3]));
                        Cloak();
                    }
                    break;
                case "gotodefinition":
                    ProMisc.GoToDefinition(false);
                    break;
                case "nexttooltip":
                    IndexToShow++;
                    TryToShowIndex();
                    break;
                default:
                    handled = false;
                    break;
            }
            htmlLinkClickedEventArgs.Handled = handled;
        }

        #endregion

        #region SetToolTip text

        /// <summary>
        /// Sets the content of the tooltip (when we want to descibe something present in the completionData list)
        /// </summary>
        private static void SetToolTip() {
            var popupMinWidth = 250;
            var toDisplay = new StringBuilder();

            GoToDefinitionFile = null;

            // only select one item from the list
            var item = GetCurrentlyDisplayedCompletionData();
            if (item == null)
                return;

            CurrentWord = item.DisplayText;

            // general stuff
            toDisplay.Append("<div class='InfoToolTip' id='ToolTip'>");
            toDisplay.Append(
                "<div class='ToolTipName' style=\"background-repeat: no-repeat; background-position: left center; background-image: url('" + item.Type + "'); padding-left: 25px; padding-top: 6px; padding-bottom: 6px;\">" + @"
                    <div>" + item.DisplayText + @"</div>
                    <div class='ToolTipSubString'>" + item.Type + @"</div>
                </div>");

            if (item is TableCompletionItem) {
                popupMinWidth = Math.Min(500, Npp.NppScreen.WorkingArea.Width / 2);
            }

            // the rest depends on the item type
            try {
                toDisplay.Append(item);
            } catch (Exception e) {
                toDisplay.Append("Error when appending info :<br>" + e + "<br>");
            }

            var parsedItem = item.ParsedBaseItem as ParsedItem;

            // parsed item?
            if (parsedItem != null && item.FromParser) {
                toDisplay.Append(HtmlHelper.FormatSubtitle("ORIGINS"));
                if (parsedItem.Scope != null)
                    toDisplay.Append(HtmlHelper.FormatRow("Scope name", parsedItem.Scope.Name));
                if (!Npp.CurrentFileInfo.Path.Equals(parsedItem.FilePath))
                    toDisplay.Append(HtmlHelper.FormatRow("Owner file", "<a class='ToolGotoDefinition' href='gotoownerfile#" + parsedItem.FilePath + "'>" + parsedItem.FilePath + "</a>"));
            }

            // Flags
            var flagStrBuilder = new StringBuilder();
            item.DoForEachFlag((name, flag) => { flagStrBuilder.Append(HtmlHelper.FormatRowWithImg(name, "<b>" + name + "</b>")); });
            if (flagStrBuilder.Length > 0) {
                toDisplay.Append(HtmlHelper.FormatSubtitle("FLAGS"));
                toDisplay.Append(flagStrBuilder);
            }

            toDisplay.Append(@"<div class='ToolTipBottomGoTo'>
                [HIT CTRL ONCE] Prevent auto-close");

            // parsed item?
            if (parsedItem != null && item.FromParser) {
                toDisplay.Append(@"<br>[" + Config.Instance.GetShortcutSpecFromName("Go_To_Definition").ToUpper() + "] <a class='ToolGotoDefinition' href='gotodefinition'>Go to definition</a>");
                GoToDefinitionPoint = new Point(parsedItem.Line, parsedItem.Column);
                GoToDefinitionFile = parsedItem.FilePath;
            }
            if (_currentCompletionList.Count > 1)
                toDisplay.Append("<br>[CTRL + <span class='ToolTipDownArrow'>" + (char) 242 + "</span>] <a class='ToolGotoDefinition' href='nexttooltip'>Read next tooltip</a>");
            toDisplay.Append("</div>");
            toDisplay.Append(_currentCompletionList.Count > 1 ? @"<div class='ToolTipCount'>" + (IndexToShow + 1) + "/" + _currentCompletionList.Count + @"</div>" : "");

            toDisplay.Append("</div>");

            _form.SetText(toDisplay.ToString(), popupMinWidth);
        }

        #endregion

        #region handle form

        /// <summary>
        /// Method to init the tooltip form if needed
        /// </summary>
        public static void InitIfneeded() {
            // instanciate the form
            if (_form == null) {
                _form = new InfoToolTipForm {
                    UnfocusedOpacity = Config.Instance.ToolTipOpacity,
                    FocusedOpacity = Config.Instance.ToolTipOpacity
                };
                _form.Show(Npp.Win32Handle);
                _form.SetLinkClickedEvent(ClickHandler);
            }
        }

        /// <summary>
        /// Is a tooltip visible?
        /// </summary>
        /// <returns></returns>
        public static bool IsVisible {
            get { return !(_form == null || !_form.IsVisible); }
        }

        /// <summary>
        /// Closes the form
        /// </summary>
        public static void Cloak(bool calledFromDwellEnd = false) {
            try {
                if (calledFromDwellEnd && !_openedFromDwell) return;
                if (_form != null)
                    _form.SafeSyncInvoke(form => form.Cloak());
                _openedFromDwell = false;
                _openedForCompletion = false;
                _currentCompletionList = null;
                GoToDefinitionFile = null;
            } catch (Exception e) {
                ErrorHandler.LogError(e);
            }
        }

        /// <summary>
        /// Closes the tooltip, but only if it was opened to help for an autocompletion item
        /// </summary>
        public static void CloseIfOpenedForCompletion() {
            if (_openedForCompletion)
                Cloak();
        }

        /// <summary>
        /// Forces the form to close, only when leaving npp
        /// </summary>
        public static void ForceClose() {
            try {
                if (_form != null)
                    _form.ForceClose();
                _form = null;
            } catch (Exception e) {
                ErrorHandler.LogError(e);
            }
        }

        /// <summary>
        /// Returns true if the cursor is within the form window
        /// </summary>
        public static bool IsMouseIn() {
            return Win32Api.IsCursorIn(_form.Handle);
        }

        #endregion
    }
}