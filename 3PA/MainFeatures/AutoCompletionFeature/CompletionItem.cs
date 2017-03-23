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
        public virtual CompletionType Type { get { return 0; } }

        /// <summary>
        /// Allows to display small "tag" picture on the left of a completionData in the autocomp list,
        /// see the ParseFlag enumeration for all the possibilities
        /// It works as a Flag, call HasFlag() method to if a certain flag is set and use
        /// Flag = Flag | ParseFlag.Reserved to set a flag!
        /// </summary>
        public ParseFlag Flags { get; set; }

        /// <summary>
        /// Used for sorting the auto completion list, the higher the ranking, the higher in the list
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
            get { return Utils.GetImageFromStr(Type.ToString()); }
        }

        /// <summary>
        /// The text that describes this item type
        /// </summary>
        public override string ItemTypeText {
            get { return "Category : <span class='SubTextColor'><b>" + Type + "</b></span><br><br>"; }
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

        #region Factory

        public static class Factory {
            public static CompletionItem New(CompletionType type) {
                switch (type) {
                    case CompletionType.Snippet:
                        return new SnippetCompletionItem();
                    case CompletionType.VariablePrimitive:
                        return new VariablePrimitiveCompletionItem();
                    case CompletionType.VariableComplex:
                        return new VariableComplexCompletionItem();
                    case CompletionType.Widget:
                        return new WidgetCompletionItem();
                    case CompletionType.Function:
                        return new FunctionCompletionItem();
                    case CompletionType.Procedure:
                        return new ProcedureCompletionItem();
                    case CompletionType.Database:
                        return new DatabaseCompletionItem();
                    case CompletionType.TempTable:
                        return new TempTableCompletionItem();
                    case CompletionType.Table:
                        return new TableCompletionItem();
                    case CompletionType.Sequence:
                        return new SequenceCompletionItem();
                    case CompletionType.Preprocessed:
                        return new PreprocessedCompletionItem();
                    case CompletionType.Label:
                        return new LabelCompletionItem();
                    case CompletionType.Keyword:
                        return new KeywordCompletionItem();
                    case CompletionType.KeywordObject:
                        return new KeywordObjectCompletionItem();
                    case CompletionType.FieldPk:
                        return new FieldPkCompletionItem();
                    case CompletionType.Field:
                        return new FieldCompletionItem();
                    default:
                        throw new Exception("You forgot to add the type" + type + " to the factory! Noob!");
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// This enum order defines the default order for the auto completion
    /// </summary>
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


    internal class SnippetCompletionItem : CompletionItem {

        public override CompletionType Type { get { return CompletionType.Snippet; } }

        public override Image ItemTypeImage { get { return ImageResources.Snippet; } }
    }


    internal abstract class VariableCompletionItem : CompletionItem {
    }


    internal class VariablePrimitiveCompletionItem : VariableCompletionItem {

        public override CompletionType Type { get { return CompletionType.VariablePrimitive; } }

        public override Image ItemTypeImage { get { return ImageResources.VariablePrimitive; } }
    }


    internal class VariableComplexCompletionItem : VariableCompletionItem {

        public override CompletionType Type { get { return CompletionType.VariableComplex; } }

        public override Image ItemTypeImage { get { return ImageResources.VariableComplex; } }
    }


    internal class WidgetCompletionItem : VariableCompletionItem {

        public override CompletionType Type { get { return CompletionType.Widget; } }

        public override Image ItemTypeImage { get { return ImageResources.Widget; } }
    }


    internal class FunctionCompletionItem : CompletionItem {

        public override CompletionType Type { get { return CompletionType.Function; } }

        public override Image ItemTypeImage { get { return ImageResources.Function; } }
    }


    internal class ProcedureCompletionItem : CompletionItem {

        public override CompletionType Type { get { return CompletionType.Procedure; } }

        public override Image ItemTypeImage { get { return ImageResources.Procedure; } }
    }


    internal class DatabaseCompletionItem : CompletionItem {

        public override CompletionType Type { get { return CompletionType.Database; } }

        public override Image ItemTypeImage { get { return ImageResources.Database; } }
    }


    internal class TempTableCompletionItem : CompletionItem {

        public override CompletionType Type { get { return CompletionType.TempTable; } }

        public override Image ItemTypeImage { get { return ImageResources.TempTable; } }
    }


    internal class TableCompletionItem : CompletionItem {

        public override CompletionType Type { get { return CompletionType.Table; } }

        public override Image ItemTypeImage { get { return ImageResources.Table; } }
    }


    internal class SequenceCompletionItem : CompletionItem {

        public override CompletionType Type { get { return CompletionType.Sequence; } }

        public override Image ItemTypeImage { get { return ImageResources.Sequence; } }
    }


    internal class PreprocessedCompletionItem : CompletionItem {

        public override CompletionType Type { get { return CompletionType.Preprocessed; } }

        public override Image ItemTypeImage { get { return ImageResources.Preprocessed; } }
    }


    internal class LabelCompletionItem : CompletionItem {

        public override CompletionType Type { get { return CompletionType.Label; } }

        public override Image ItemTypeImage { get { return ImageResources.Label; } }
    }

    
    internal class KeywordCompletionItem : CompletionItem {

        public KeywordType KeywordType { get; set; }

        public override CompletionType Type { get { return CompletionType.Keyword; } }

        public override Image ItemTypeImage { get { return ImageResources.Keyword; } }
    }


    /// <summary>
    /// Keyword types enumeration
    /// </summary>
    public enum KeywordType {
        // below are the types that go to the Keyword category
        Statement,
        Function,
        Operator,
        Option,
        Type,
        Widget,
        Preprocessor,
        Handle,
        Event,
        Keyboard,
        Abbreviation,
        Appbuilder,
        Unknow,

        // below are the types that go into the KeywordObject category
        Attribute = 30,
        Property,
        Method
    }


    internal class KeywordObjectCompletionItem : KeywordCompletionItem {

        public override CompletionType Type { get { return CompletionType.KeywordObject; } }

        public override Image ItemTypeImage { get { return ImageResources.KeywordObject; } }
    }


    internal class FieldCompletionItem : CompletionItem {

        public override CompletionType Type { get { return CompletionType.Field; } }

        public override Image ItemTypeImage { get { return ImageResources.Field; } }
    }


    internal class FieldPkCompletionItem : FieldCompletionItem {

        public override CompletionType Type { get { return CompletionType.FieldPk; } }

        public override Image ItemTypeImage { get { return ImageResources.FieldPk; } }
    }

}