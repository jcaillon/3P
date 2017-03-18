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
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletionFeature;
using _3PA.MainFeatures.Parser;
using _3PA.MainFeatures.Pro;
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
            if (_currentCompletionList == null) return null;
            if (IndexToShow < 0) IndexToShow = _currentCompletionList.Count - 1;
            if (IndexToShow >= _currentCompletionList.Count) IndexToShow = 0;
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
            var curContext = (UdlStyles) Sci.GetStyleAt(position);
            if (curContext == UdlStyles.Comment || curContext == UdlStyles.Delimiter8)
                return;

            // sets the tooltip content
            var data = AutoCompletion.FindInCompletionData(Sci.GetAblWordAtPosition(position), position);
            if (data != null && data.Count > 0)
                _currentCompletionList = data;
            else
                return;

            // in strings, only functions trigger the tooltip
            if ((curContext == UdlStyles.Delimiter1 || curContext == UdlStyles.Delimiter2 || curContext == UdlStyles.Delimiter4 || curContext == UdlStyles.Delimiter5) && _currentCompletionList.First().Type != CompletionType.Function)
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
                _form.UnCloack();
            }
        }

        /// <summary>
        /// Called when a tooltip is shown and the user presses CTRL + down/up to show 
        /// the other definitions available
        /// </summary>
        public static void TryToShowIndex() {
            if (_currentCompletionList == null) return;

            // refresh tooltip with the correct index
            _form.Cloack();
            SetToolTip();
            if (!_form.Visible)
                _form.UnCloack();
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
                    _form.UnCloack();
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
                    ProMisc.GoToDefinition();
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
            var data = GetCurrentlyDisplayedCompletionData();
            if (data == null) return;

            CurrentWord = data.DisplayText;

            // general stuff
            toDisplay.Append("<div class='InfoToolTip' id='ToolTip'>");
            toDisplay.Append(@"
                <table width='100%' class='ToolTipName'>
                    <tr style='vertical-align: top;'>
                    <td>
                        <table width='100%' style='margin: 0; padding: 0;'>
                            <tr>
                                <td rowspan='2' style='width: 25px;'>
                                    <img src='" + data.Type + @"'>
                                </td>
                                <td>
                                    " + data.DisplayText + @"
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <span class='ToolTipSubString'>" + data.Type + @"</span>
                                </td>
                            </tr>
                        </table>
                    </td>");
            if (_currentCompletionList.Count > 1)
                toDisplay.Append(@"
                        <td class='ToolTipCount'>" +
                                 (IndexToShow + 1) + "/" + _currentCompletionList.Count + @"
                        </td>");
            toDisplay.Append(@"
                    </tr>
                </table>");

            // the rest depends on the item type
            try {
                switch (data.Type) {
                    case CompletionType.TempTable:
                    case CompletionType.Table:
                        popupMinWidth = Math.Min(500, Npp.NppScreen.WorkingArea.Width/2);
                        // buffer
                        if (data.FromParser) {
                            if (data.ParsedItem is ParsedDefine) {
                                toDisplay.Append(FormatRowWithImg(ParseFlag.Buffer.ToString(), "BUFFER FOR " + FormatSubString(data.SubString)));
                            }
                            if (data.ParsedItem is ParsedTable && !string.IsNullOrEmpty(data.SubString)) {
                                toDisplay.Append(FormatRow("Is like", (data.SubString.Contains("?")) ? "Unknow table [" + ((ParsedTable) data.ParsedItem).LcLikeTable + "]" : data.SubString.Replace("Like ", "")));
                            }
                        }

                        var tbItem = ParserHandler.FindAnyTableOrBufferByName(data.DisplayText);
                        if (tbItem != null) {
                            if (!string.IsNullOrEmpty(tbItem.Description)) {
                                toDisplay.Append(FormatRow("Description", tbItem.Description));
                            }

                            if (tbItem.Fields.Count > 0) {
                                toDisplay.Append(FormatSubtitle("FIELDS [x" + tbItem.Fields.Count + "]"));
                                toDisplay.Append("<table width='100%;'>");
                                foreach (var parsedField in tbItem.Fields) {
                                    toDisplay.Append("<tr><td><img src='" + (parsedField.Flags.HasFlag(ParseFlag.Primary) ? CompletionType.FieldPk.ToString() : CompletionType.Field.ToString()) + "'></td><td style='padding-right: 4px'>" + (parsedField.Flags.HasFlag(ParseFlag.Mandatory) ? "<img src='Mandatory'>" : "") + "</td><td style='padding-right: 8px'>" + parsedField.Name + "</td><td style='padding-right: 8px'>" + parsedField.Type + "</td><td style='padding-right: 8px'> = " + (string.IsNullOrEmpty(parsedField.InitialValue) ? "DEFAULT" : parsedField.Type == ParsedPrimitiveType.Character ? parsedField.InitialValue.ProQuoter() : parsedField.InitialValue) + "</td><td style='padding-right: 8px'>" + parsedField.Description + "</td></tr>");
                                }
                                toDisplay.Append("</table>");
                            }

                            if (tbItem.Triggers.Count > 0) {
                                toDisplay.Append(FormatSubtitle("TRIGGERS [x" + tbItem.Triggers.Count + "]"));
                                foreach (var parsedTrigger in tbItem.Triggers) {
                                    toDisplay.Append(FormatRow(parsedTrigger.Event, "<a class='ToolGotoDefinition' href='trigger#" + parsedTrigger.ProcName + "'>" + parsedTrigger.ProcName + "</a>"));
                                }
                            }

                            if (tbItem.Indexes.Count > 0) {
                                toDisplay.Append(FormatSubtitle("INDEXES [x" + tbItem.Indexes.Count + "]"));
                                foreach (var parsedIndex in tbItem.Indexes) {
                                    toDisplay.Append(FormatRow(parsedIndex.Name, (parsedIndex.Flag != ParsedIndexFlag.None ? parsedIndex.Flag + " - " : "") + parsedIndex.FieldsList.Aggregate((i, j) => i + ", " + j)));
                                }
                            }
                        }
                        break;
                    case CompletionType.Sequence:
                        toDisplay.Append(FormatRow("Database logical name", data.SubString));
                        break;
                    case CompletionType.Database:
                        var dbItem = DataBase.GetDb(data.DisplayText);

                        toDisplay.Append(FormatRow("Logical name", dbItem.LogicalName));
                        toDisplay.Append(FormatRow("Physical name", dbItem.PhysicalName));
                        toDisplay.Append(FormatRow("Progress version", dbItem.ProgressVersion));
                        toDisplay.Append(FormatRow("Number of Tables", dbItem.Tables.Count.ToString()));
                        break;
                    case CompletionType.Field:
                    case CompletionType.FieldPk:
                        // find field
                        var fieldFound = DataBase.FindFieldByName(data.DisplayText, (ParsedTable) data.ParsedItem);
                        if (fieldFound != null) {
                            if (fieldFound.AsLike == ParsedAsLike.Like) {
                                toDisplay.Append(FormatRow("Is LIKE", fieldFound.TempType));
                            }
                            toDisplay.Append(FormatRow("Type", FormatSubString(data.SubString)));
                            toDisplay.Append(FormatRow("Owner table", ((ParsedTable) data.ParsedItem).Name));
                            if (!string.IsNullOrEmpty(fieldFound.Description))
                                toDisplay.Append(FormatRow("Description", fieldFound.Description));
                            if (!string.IsNullOrEmpty(fieldFound.Format))
                                toDisplay.Append(FormatRow("Format", fieldFound.Format));
                            if (!string.IsNullOrEmpty(fieldFound.InitialValue))
                                toDisplay.Append(FormatRow("Initial value", fieldFound.InitialValue));
                            toDisplay.Append(FormatRow("Order", fieldFound.Order.ToString()));
                        }

                        break;
                    case CompletionType.Procedure:
                        // find its parameters
                        toDisplay.Append(FormatSubtitle("PARAMETERS"));
                        var paramList = ParserHandler.FindProcedureParameters(data);
                        if (paramList.Count > 0)
                            foreach (var parameter in paramList) {
                                var defItem = (ParsedDefine) parameter.ParsedItem;
                                toDisplay.Append(FormatRowParam(defItem.Flags, parameter.DisplayText + " as <span class='ToolTipSubString'>" + defItem.PrimitiveType + "</span>"));
                            }
                        else
                            toDisplay.Append("None");
                        break;
                    case CompletionType.Function:
                        var funcItem = (ParsedFunction) data.ParsedItem;
                        toDisplay.Append(FormatSubtitle("RETURN TYPE"));
                        toDisplay.Append(FormatRowParam(ParseFlag.Output, "Returns " + FormatSubString(funcItem.ReturnType.ToString())));

                        toDisplay.Append(FormatSubtitle("PARAMETERS"));
                        var param2List = ParserHandler.FindProcedureParameters(data);
                        if (param2List.Count > 0)
                            foreach (var parameter in param2List) {
                                var defItem = (ParsedDefine) parameter.ParsedItem;
                                toDisplay.Append(FormatRowParam(defItem.Flags, parameter.DisplayText + " as " + FormatSubString(defItem.PrimitiveType.ToString())));
                            }
                        else
                            toDisplay.Append("None");

                        var funcImplem = data.ParsedItem as ParsedImplementation;
                        if (funcImplem != null) {
                            toDisplay.Append(FormatSubtitle("PROTOTYPE"));
                            if (funcImplem.HasPrototype)
                                toDisplay.Append(FormatRowWithImg("Prototype", "<a class='ToolGotoDefinition' href='proto#" + funcItem.FilePath + "#" + funcImplem.PrototypeLine + "#" + funcImplem.PrototypeColumn + "'>Go to prototype</a>"));
                            else
                                toDisplay.Append("Has none");
                        } else {
                            toDisplay.Append(FormatSubtitle("DEFINED IN"));
                            toDisplay.Append("Function defined in an external procedure or is a web service operation");
                        }
                        break;
                    case CompletionType.Keyword:
                    case CompletionType.KeywordObject:
                        toDisplay.Append(FormatRow("Type of keyword", FormatSubString(data.SubString)));
                        // for abbreviations, find the complete keyword first
                        string keyword = data.DisplayText;
                        if (data.KeywordType == KeywordType.Abbreviation) {
                            keyword = Keywords.GetFullKeyword(keyword);
                            var associatedKeyword = AutoCompletion.FindInCompletionData(keyword, 0);
                            if (associatedKeyword != null && associatedKeyword.Count > 0)
                                data = associatedKeyword.First();
                        }
                        string keyToFind = null;
                        // for the keywords define and create, we try to match the second keyword that goes with it
                        if (data.KeywordType == KeywordType.Statement &&
                            (keyword.EqualsCi("define") || keyword.EqualsCi("create"))) {
                            var lineStr = Sci.GetLine(Sci.LineFromPosition(Sci.GetPositionFromMouseLocation())).Text;
                            var listOfSecWords = new List<string> {"ALIAS", "BROWSE", "BUFFER", "BUTTON", "CALL", "CLIENT-PRINCIPAL", "DATA-SOURCE", "DATABASE", "DATASET", "EVENT", "FRAME", "IMAGE", "MENU", "PARAMETER", "PROPERTY", "QUERY", "RECTANGLE", "SAX-ATTRIBUTES", "SAX-READER", "SAX-WRITER", "SERVER", "SERVER-SOCKET", "SOAP-HEADER", "SOAP-HEADER-ENTRYREF", "SOCKET", "STREAM", "SUB-MENU", "TEMP-TABLE", "VARIABLE", "WIDGET-POOL", "WORK-TABLE", "WORKFILE", "X-DOCUMENT", "X-NODEREF"};
                            foreach (var word in listOfSecWords) {
                                if (lineStr.ContainsFast(word)) {
                                    keyToFind = string.Join(" ", keyword, word, data.SubString);
                                    break;
                                }
                            }
                        }
                        if (keyToFind == null)
                            keyToFind = string.Join(" ", keyword, data.SubString);

                        var dataHelp = Keywords.GetKeywordHelp(keyToFind);
                        if (dataHelp != null) {
                            toDisplay.Append(FormatSubtitle("DESCRIPTION"));
                            toDisplay.Append(dataHelp.Description);

                            // synthax
                            if (dataHelp.Synthax.Count >= 1 && !string.IsNullOrEmpty(dataHelp.Synthax[0])) {
                                toDisplay.Append(FormatSubtitle("SYNTAX"));
                                toDisplay.Append(@"<div class='ToolTipcodeSnippet'>");
                                var i = 0;
                                foreach (var synthax in dataHelp.Synthax) {
                                    if (i > 0) toDisplay.Append(@"<br>");
                                    toDisplay.Append(synthax);
                                    i++;
                                }
                                toDisplay.Append(@"</div>");
                            }
                        } else {
                            toDisplay.Append(FormatSubtitle("404 NOT FOUND"));
                            if (data.KeywordType == KeywordType.Option)
                                toDisplay.Append("<i><b>Sorry, this keyword doesn't have any help associated</b><br>Since this keyword is an option, try to hover the first keyword of the statement or refer to the 4GL help</i>");
                            else
                                toDisplay.Append("<i><b>Sorry, this keyword doesn't have any help associated</b><br>Please refer to the 4GL help</i>");
                        }

                        CurrentWord = keyToFind;
                        break;
                    case CompletionType.Label:
                        break;
                    case CompletionType.Preprocessed:
                        var preprocItem = (ParsedPreProcVariable) data.ParsedItem;
                        if (preprocItem.UndefinedLine > 0)
                            toDisplay.Append(FormatRow("Undefined line", preprocItem.UndefinedLine.ToString()));
                        toDisplay.Append(FormatSubtitle("VALUE"));
                        toDisplay.Append(@"<div class='ToolTipcodeSnippet'>");
                        toDisplay.Append(preprocItem.Value);
                        toDisplay.Append(@"</div>");
                        break;
                    case CompletionType.Snippet:
                        // TODO
                        break;
                    case CompletionType.VariableComplex:
                    case CompletionType.VariablePrimitive:
                    case CompletionType.Widget:
                        var varItem = (ParsedDefine) data.ParsedItem;
                        toDisplay.Append(FormatRow("Define type", FormatSubString(varItem.Type.ToString())));
                        if (!string.IsNullOrEmpty(varItem.TempPrimitiveType))
                            toDisplay.Append(FormatRow("Variable type", FormatSubString(varItem.PrimitiveType.ToString())));
                        if (varItem.AsLike == ParsedAsLike.Like)
                            toDisplay.Append(FormatRow("Is LIKE", varItem.TempPrimitiveType));
                        if (!string.IsNullOrEmpty(varItem.ViewAs))
                            toDisplay.Append(FormatRow("Screen representation", varItem.ViewAs));
                        if (!string.IsNullOrEmpty(varItem.Left)) {
                            toDisplay.Append(FormatSubtitle("END OF DECLARATION"));
                            toDisplay.Append(@"<div class='ToolTipcodeSnippet'>");
                            toDisplay.Append(varItem.Left);
                            toDisplay.Append(@"</div>");
                        }
                        break;
                }
            } catch (Exception e) {
                toDisplay.Append("Error when appending info :<br>" + e + "<br>");
            }

            // parsed item?
            if (data.FromParser) {
                toDisplay.Append(FormatSubtitle("ORIGINS"));
                toDisplay.Append(FormatRow("Scope name", data.ParsedItem.Scope.Name));
                if (!Npp.CurrentFile.Path.Equals(data.ParsedItem.FilePath))
                    toDisplay.Append(FormatRow("Owner file", "<a class='ToolGotoDefinition' href='gotoownerfile#" + data.ParsedItem.FilePath + "'>" + data.ParsedItem.FilePath + "</a>"));
            }

            // Flags
            var flagStrBuilder = new StringBuilder();
            data.DoForEachFlag((name, flag) => { flagStrBuilder.Append(FormatRowWithImg(name, "<b>" + name + "</b>")); });
            if (flagStrBuilder.Length > 0) {
                toDisplay.Append(FormatSubtitle("FLAGS"));
                toDisplay.Append(flagStrBuilder);
            }

            toDisplay.Append(@"<div class='ToolTipBottomGoTo'>
                [HIT CTRL ONCE] Prevent auto-close");
            // parsed item?
            if (data.FromParser) {
                toDisplay.Append(@"<br>[" + Config.Instance.GetShortcutSpecFromName("Go_To_Definition").ToUpper() + "] <a class='ToolGotoDefinition' href='gotodefinition'>Go to definition</a>");
                GoToDefinitionPoint = new Point(data.ParsedItem.Line, data.ParsedItem.Column);
                GoToDefinitionFile = data.ParsedItem.FilePath;
            }
            if (_currentCompletionList.Count > 1)
                toDisplay.Append("<br>[CTRL + <span class='ToolTipDownArrow'>" + (char) 242 + "</span>] <a class='ToolGotoDefinition' href='nexttooltip'>Read next tooltip</a>");
            toDisplay.Append("</div>");

            toDisplay.Append("</div>");

            _form.SetText(toDisplay.ToString(), popupMinWidth);
        }

        #region formatting functions

        private static string FormatRow(string describe, string result) {
            return "- " + describe + " : <b>" + result + "</b><br>";
        }

        private static string FormatRowWithImg(string image, string text) {
            return "<div class='ToolTipRowWithImg'><img style='padding-right: 2px; padding-left: 5px;' src='" + image + "' height='15px'>" + text + "</div>";
        }

        private static string FormatRowParam(ParseFlag flags, string text) {
            var image = ParseFlag.Input.ToString();
            if (flags.HasFlag(ParseFlag.InputOutput))
                image = ParseFlag.InputOutput.ToString();
            else if (flags.HasFlag(ParseFlag.Output))
                image = ParseFlag.Output.ToString();
            else if (flags.HasFlag(ParseFlag.Return))
                image = ParseFlag.Return.ToString();
            return "<div class='ToolTipRowWithImg'><img style='padding-right: 2px; padding-left: 5px;' src='" + image + "' height='15px'>" + text + "</div>";
        }

        private static string FormatSubtitle(string text) {
            return "<div class='ToolTipSubTitle'>" + text + "</div>";
        }

        private static string FormatSubString(string text) {
            return "<span class='ToolTipSubString'>" + text + "</span>";
        }

        #endregion

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
                    _form.SafeSyncInvoke(form => form.Cloack());
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