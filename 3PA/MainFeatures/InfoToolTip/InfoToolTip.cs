#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (InfoToolTip.cs) is part of 3P.
// 
// // 3P is a free software: you can redistribute it and/or modify
// // it under the terms of the GNU General Public License as published by
// // the Free Software Foundation, either version 3 of the License, or
// // (at your option) any later version.
// 
// // 3P is distributed in the hope that it will be useful,
// // but WITHOUT ANY WARRANTY; without even the implied warranty of
// // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// // GNU General Public License for more details.
// 
// // You should have received a copy of the GNU General Public License
// // along with 3P. If not, see <http://www.gnu.org/licenses/>.
// // ========================================================================
#endregion
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using BrightIdeasSoftware;
using _3PA.Images;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.InfoToolTip {
    class InfoToolTip {

        #region fields

        private static InfoToolTipForm _form;

        /// <summary>
        /// Was the form opened because the user left his mouse too long on a word?
        /// </summary>
        private static bool _openedFromDwell;

        /// <summary>
        /// If a tooltip is opened and it's a parsed item, this point leads to its definition
        /// </summary>
        public static Point GoToDefinitionPoint = new Point(-1, -1);

        #endregion

        #region Tooltip

        public static void ShowToolTip(bool openedFromDwell = false) {
            if (Config.Instance.ToolTipDeactivate) return;

            // remember if the popup was opened because of the dwell time
            _openedFromDwell = openedFromDwell;

            // instanciate the form
            if (_form == null) {
                _form = new InfoToolTipForm {
                    UnfocusedOpacity = Config.Instance.ToolTipUnfocusedOpacity,
                    FocusedOpacity = Config.Instance.ToolTipFocusedOpacity
                };
                _form.Show(Npp.Win32WindowNpp);
            }

            // opened from dwell
            if (openedFromDwell) {
                var position = Npp.GetPositionFromMouseLocation();

                // sets the tooltip content
                if (position < 0) 
                    return;
                var data = AutoComplete.FindInCompletionData(Npp.GetWordAtPosition(position), position);
                if (data == null) return;
                SetToolTip(data);

                // update position
                var point = Npp.GetPointXyFromPosition(position);
                point.Offset(Npp.GetWindowRect().Location);
                var lineHeight = Npp.GetTextHeight(Npp.GetCaretLineNumber());
                point.Y += lineHeight + 5;
                _form.SetPosition(point, lineHeight + 5);
            }

            if (!_form.Visible)
                _form.UnCloack();
        }

        #endregion

        #region SetToolTip text

        /// <summary>
        /// Sets the content of the tooltip (when we want to descibe something present
        /// in the completionData list)
        /// </summary>
        private static void SetToolTip(CompletionData data) {
            var toDisplay = new StringBuilder();

            // general stuff
            toDisplay.Append("<div class='InfoToolTip'>");
            toDisplay.Append("<div class='ToolTipName'><img style='padding-right: 7px;' src ='" + data.Type + "'>" + data.Type + "</div>");

            // the rest depends on the data type
            try {
                switch (data.Type) {
                    case CompletionType.TempTable:
                    case CompletionType.Table:
                        // buffer
                        if (data.ParsedItem is ParsedDefine)
                            toDisplay.Append(FormatRowWithImg(ParseFlag.Buffer.ToString(), "BUFFER FOR <span class='ToolTipSubString'>" + data.SubString + "</span>"));

                        var tbItem = ParserHandler.FindAnyTableOrBufferByName(data.DisplayText);
                        if (tbItem != null) {
                            if (!string.IsNullOrEmpty(tbItem.Description))
                                toDisplay.Append(FormatRow("Description", tbItem.Description));
                            toDisplay.Append(FormatRow("Number of fields", tbItem.Fields.Count.ToString()));

                            if (tbItem.Triggers.Count > 0) {
                                toDisplay.Append(FormatSubtitle("TRIGGERS"));
                                foreach (var parsedTrigger in tbItem.Triggers)
                                    toDisplay.Append(FormatRow(parsedTrigger.Event, parsedTrigger.ProcName));
                            }

                            if (tbItem.Indexes.Count > 0) {
                                toDisplay.Append(FormatSubtitle("INDEXES"));
                                foreach (var parsedIndex in tbItem.Indexes)
                                    toDisplay.Append(FormatRow(parsedIndex.Name, parsedIndex.Flag + " - " + parsedIndex.FieldsList.Aggregate((i, j) => i + ", " + j)));
                            }
                        }
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
                            toDisplay.Append(FormatRow("Type", "<span class='ToolTipSubString'>" + data.SubString + "</span>"));
                            toDisplay.Append(FormatRow("Owner table", ((ParsedTable)data.ParsedItem).Name));
                            if (!string.IsNullOrEmpty(fieldFound.Description))
                                toDisplay.Append(FormatRow("Description", fieldFound.Description));
                            if (!string.IsNullOrEmpty(fieldFound.Format))
                                toDisplay.Append(FormatRow("Format", fieldFound.Format));
                            if (!string.IsNullOrEmpty(fieldFound.InitialValue))
                                toDisplay.Append(FormatRow("Initial value", fieldFound.InitialValue));
                            toDisplay.Append(FormatRow("Order", fieldFound.Order.ToString()));
                        }
  
                        break;
                    case CompletionType.Function:
                        var funcItem = (ParsedFunction) data.ParsedItem;
                        toDisplay.Append(FormatRow("Return type", "<span class='ToolTipSubString'>" + funcItem.ParsedReturnType + "</span>"));
                        if (funcItem.PrototypeLine > 0)
                            toDisplay.Append("<a href=''>Go to prototype</a>");

                        toDisplay.Append(FormatSubtitle("PARAMETERS"));
                        if (!string.IsNullOrEmpty(funcItem.Parameters)) {
                            foreach (var param in funcItem.Parameters.Split(',')) {
                                toDisplay.Append(FormatRowWithImg(ParseFlag.Parameter.ToString(), param.Trim()));
                            }
                        } else
                            toDisplay.Append("No parameters!<br>");
                        break;
                    case CompletionType.Keyword:
                    case CompletionType.KeywordObject:
                        toDisplay.Append(FormatRow("Type of keyword", "<span class='ToolTipSubString'>" + data.SubString + "</span>"));
                        toDisplay.Append(FormatSubtitle("DESCRIPTION"));
                        // TODO
                        toDisplay.Append(FormatSubtitle("SYNTHAX"));
                        // TODO
                        break;
                    case CompletionType.Label:
                        break;
                    case CompletionType.Preprocessed:
                        var preprocItem = (ParsedPreProc) data.ParsedItem;
                        if (preprocItem.UndefinedLine > 0)
                            toDisplay.Append(FormatRow("Undefined line", preprocItem.UndefinedLine.ToString()));
                        break;
                    case CompletionType.Snippet:
                        // TODO
                        break;
                    case CompletionType.VariableComplex:
                    case CompletionType.VariablePrimitive:
                    case CompletionType.Widget:
                        var varItem = (ParsedDefine) data.ParsedItem;
                        toDisplay.Append(FormatRow("Define type", "<span class='ToolTipSubString'>" + varItem.Type + "</span>"));
                        if (!string.IsNullOrEmpty(varItem.TempPrimitiveType))
                            toDisplay.Append(FormatRow("Variable type", "<span class='ToolTipSubString'>" + varItem.PrimitiveType + "</span>"));
                        if (varItem.AsLike == ParsedAsLike.Like)
                            toDisplay.Append(FormatRow("Is LIKE", varItem.TempPrimitiveType));
                        if (!string.IsNullOrEmpty(varItem.ViewAs))
                            toDisplay.Append(FormatRow("Screen representation", varItem.ViewAs));
                        toDisplay.Append(FormatRow("Define flags", varItem.LcFlagString));
                        toDisplay.Append(FormatRow("Rest of decla", varItem.Left));
                        break;

                }
            } catch (Exception e) {
                toDisplay.Append("Error when appending info :<br>" + e + "<br>");
            }

            // parsed item?
            if (data.FromParser) {
                toDisplay.Append(FormatSubtitle("ORIGINS"));
                toDisplay.Append(FormatRow("Scope name", data.ParsedItem.OwnerName));
                if (!Npp.GetCurrentFilePath().Equals(data.ParsedItem.FilePath))
                    toDisplay.Append(FormatRow("Owner file", data.ParsedItem.FilePath));
            }

            // Flags
            var flagStrBuilder = new StringBuilder();
            foreach (var name in Enum.GetNames(typeof(ParseFlag))) {
                ParseFlag flag = (ParseFlag)Enum.Parse(typeof(ParseFlag), name);
                if (flag == 0) continue;
                if (!data.Flag.HasFlag(flag)) continue;
                flagStrBuilder.Append(FormatRowWithImg(name, "<b>" + name + "</b>"));
            }
            if (flagStrBuilder.Length > 0) {
                toDisplay.Append(FormatSubtitle("FLAGS"));
                toDisplay.Append(flagStrBuilder);
            }

            // parsed item?
            if (data.FromParser) {
                toDisplay.Append("<div class='ToolTipBottomGoTo'>[CTRL + B] GO TO DEFINITION</div>");
                GoToDefinitionPoint = new Point(data.ParsedItem.Line, data.ParsedItem.Column);
            }

            toDisplay.Append("</div>");
            _form.SetText(toDisplay.ToString());

        }

        #region formatting functions

        private static string FormatRow(string describe, string result) {
            return "- " + describe + " : <b>" + result + "</b><br>";
        }

        private static string FormatRowWithImg(string image, string text) {
            return "<div class='ToolTipRowWithImg'><img style='padding-right: 2px; padding-left: 5px;' src ='" + image + "' height='15px'>" + text + "</div>";
        }

        private static string FormatSubtitle(string text) {
            return "<div class='ToolTipSubTitle'>" + text + "</div>";
        }

        #endregion


        #endregion


        #region handle form

        /// <summary>
        /// Closes the form
        /// </summary>
        public static void Close(bool calledFromDwellEnd = false) {
            try {
                if (calledFromDwellEnd && !_openedFromDwell) return;
                _form.Cloack();
                _openedFromDwell = false;
                GoToDefinitionPoint = new Point(-1, -1);
            } catch (Exception) {
                // ignored
            }
        }

        /// <summary>
        /// Forces the form to close, only when leaving npp
        /// </summary>
        public static void ForceClose() {
            try {
                _form.ForceClose();
                _form = null;
            } catch (Exception) {
                // ignored
            }
        }

        #endregion

    }
}
