#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (CompletionItem.cs) is part of 3P.
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
using YamuiFramework.Controls.YamuiList;
using _3PA.Lib;
using _3PA.MainFeatures.Parser;
using _3PA._Resource;

namespace _3PA.MainFeatures.AutoCompletionFeature {
    /// <summary>
    /// class used in the auto completion feature
    /// </summary>
    internal class CompletionItem : FilteredTypeListItem {
        /// <summary>
        /// Type of completion
        /// </summary>
        public CompletionType Type { get; set; }

        /// <summary>
        /// Allows to display small "tag" picture on the left of a completionData in the autocomp list,
        /// see the ParseFlag enumeration for all the possibilities
        /// It works as a Flag, call HasFlag() method to if a certain flag is set and use
        /// Flag = Flag | ParseFlag.Reserved to set a flag!
        /// </summary>
        public ParseFlag Flags { get; set; }

        /// <summary>
        /// Used for sorting the autocompletion list, the higher the ranking, the higher in the list
        /// the item is
        /// </summary>
        public int Ranking { get; set; }

        /// <summary>
        /// Indicates whether or not this completionData is created by the parser Visitor
        /// </summary>
        public bool FromParser { get; set; }

        /// <summary>
        /// When the FromParser is true, contains the ParsedItem extracted by the parser
        /// </summary>
        public ParsedBaseItem ParsedBaseItem { get; set; }

        /// <summary>
        /// This field is only used when Type == CompletionType.Keyword, it contains the keyword type...
        /// </summary>
        public KeywordType KeywordType { get; set; }

        /// <summary>
        /// Use this method to do an action for each flag of the item...
        /// </summary>
        /// <param name="toApplyOnFlag"></param>
        public void DoForEachFlag(Action<string, ParseFlag> toApplyOnFlag) {
            typeof(ParseFlag).ForEach<ParseFlag>((s, l) => {
                if (l == 0 || !Flags.HasFlag((ParseFlag)l))
                    return;
                toApplyOnFlag(s, (ParseFlag) l);
            });
        }

        /// <summary>
        /// The piece of text displayed in the list
        /// </summary>
        public override string DisplayText { get; set; }

        /// <summary>
        /// return the image to display for this item
        /// If null, the image corresponding to ItemTypeImage will be used instead
        /// </summary>
        public override Image ItemImage { get; set; }

        /// <summary>
        /// return this item type (a unique int for each item type)
        /// if the value is strictly inferior to 0, the button for this type will not appear
        /// on the bottom of list
        /// </summary>
        public override int ItemType {
            get { return (int) Type; }
        }

        /// <summary>
        /// return the image that will be used to identify this item
        /// type, it will be used for the bottom buttons of the list
        /// All items of a given type should return the same image! The image used for the 
        /// bottom buttons will be that of the first item found for the given type
        /// </summary>
        public override Image ItemTypeImage {
            get { return Utils.GetImageFromStr(((CompletionType) ItemType).ToString()); }
        }

        /// <summary>
        /// The text that describes this item type
        /// </summary>
        public override string ItemTypeText {
            get { return "Category : <span class='SubTextColor'><b>" + ((CompletionType) ItemType) + "</b></span><br><br>"; }
        }

        /// <summary>
        /// return true if the item is to be highlighted
        /// </summary>
        public override bool IsRowHighlighted {
            get { return false; }
        }

        /// <summary>
        /// return a string containing the subtext to display
        /// </summary>
        public override string SubText { get; set; }

        /// <summary>
        /// return a list of images to be displayed (in reverse order) for the item
        /// </summary>
        public override List<Image> TagImages {
            get {
                var outList = new List<Image>();
                typeof(ParseFlag).ForEach<ParseFlag>((s, l) => {
                    if (l == 0 || !Flags.HasFlag((ParseFlag)l))
                        return;
                    Image tryImg = (Image)ImageResources.ResourceManager.GetObject(s);
                    if (tryImg != null)
                        outList.Add(tryImg);
                });
                return outList;
            }
        }

        #region Children and parent

        /// <summary>
        /// return the list of the children for this item (if any) or null
        /// </summary>
        public List<CompletionItem> Children { get; set; }

        /// <summary>
        /// The char that separate this item's word with each child item's word
        /// The chars that, once entered after THIS word, should trigger the list of children
        /// If you add a new child separator, you should register it in the AutoCompletion class
        /// </summary>
        public char? ChildSeparator { get; set; }

        /// <summary>
        /// Parent completionItem of this item (the parent has this item in its children)
        /// </summary>
        public CompletionItem ParentItem { get; set; }

        #endregion
    }

    internal class SnippetCompletionItem : CompletionItem {

    }

    internal class VariablePrimitiveCompletionItem : CompletionItem {

    }

    internal class VariableComplexCompletionItem : CompletionItem {

    }

    internal class WidgetCompletionItem : CompletionItem {

    }

    internal class FunctionCompletionItem : CompletionItem {

    }

    internal class ProcedureCompletionItem : CompletionItem {

    }

    internal class DatabaseCompletionItem : CompletionItem {

    }

    internal class TempTableCompletionItem : CompletionItem {

    }

    internal class TableCompletionItem : CompletionItem {

    }

    internal enum CompletionType {
        Snippet,
        VariablePrimitive,
        VariableComplex,
        Widget,
        Function,
        Procedure,
        Database,
        TempTable,
        Table,
        Sequence,
        Preprocessed,
        Label,
        Keyword,
        KeywordObject,
        FieldPk,
        Field
    }
}