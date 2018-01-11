#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (CodeItem.cs) is part of 3P.
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

namespace _3PA.MainFeatures.CodeExplorer {
    /// <summary>
    /// base class
    /// </summary>
    internal class CodeItem : FilteredTypeTreeListItem {
        /// <summary>
        /// corresponds to the icon displayed next to DisplayText, if set to 0 (=BranchIcon) then the icon
        /// chosen for this item is the icon corresponding to the branch
        /// </summary>
        public virtual CodeExplorerIconType Type { get; set; }

        /// <summary>
        /// if the item has no children, clicking on it will make the caret move to this line
        /// </summary>
        public int GoToLine { get; set; }

        /// <summary>
        /// if the item has no children, clicking on it will make the caret move to this GoToColumn
        /// </summary>
        public int GoToColumn { get; set; }

        /// <summary>
        /// path of the document in which the item was found, for example if we found a procedure block in a .i
        /// we want to switch to the owning document before going to the line
        /// </summary>
        public string DocumentOwner { get; set; }

        /// <summary>
        /// Flags for the item, is directly related to the images displayed on the right of the item
        /// </summary>
        public ParseFlag Flags { get; set; }

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
        /// Should this item be hidden when in searching mode?
        /// </summary>
        public override bool HideWhileSearching {
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
                    if (l == 0 || !Flags.HasFlag(l))
                        return;
                    Image tryImg = (Image) ImageResources.ResourceManager.GetObject(s);
                    if (tryImg != null)
                        outList.Add(tryImg);
                });
                return outList;
            }
        }

        /// <summary>
        /// to override, that should return the list of the children for this item (if any) or null
        /// </summary>
        public override List<FilteredTypeTreeListItem> Children { get; set; }

        #region Factory

        public static class Factory {
            public static CodeItem New(CodeExplorerIconType type) {
                switch (type) {
                    case CodeExplorerIconType.RunInternal:
                        return new RunInternalCodeItem();
                    case CodeExplorerIconType.RunExternal:
                        return new RunExternalCodeItem();
                    case CodeExplorerIconType.DynamicFunctionCallExternal:
                        return new DynamicFunctionCallExternalCodeItem();
                    case CodeExplorerIconType.DynamicFunctionCall:
                        return new DynamicFunctionCallCodeItem();
                    case CodeExplorerIconType.StaticFunctionCall:
                        return new StaticFunctionCallCodeItem();
                    case CodeExplorerIconType.TempTable:
                        return new TempTableCodeItem();
                    case CodeExplorerIconType.Table:
                        return new TableCodeItem();

                    case CodeExplorerIconType.Subscribe:
                        return new SubscribeCodeItem();
                    case CodeExplorerIconType.Publish:
                        return new PublishCodeItem();
                    case CodeExplorerIconType.Unsubscribe:
                        return new UnsubscribeCodeItem();

                    case CodeExplorerIconType.MainBlock:
                        return new MainBlockCodeItem();
                    case CodeExplorerIconType.Prototype:
                        return new PrototypeCodeItem();

                    case CodeExplorerIconType.DefinitionBlock:
                    case CodeExplorerIconType.XtfrBlock:
                    case CodeExplorerIconType.PreprocessorBlock:
                    case CodeExplorerIconType.SettingsBlock:
                    case CodeExplorerIconType.CreateWindowBlock:
                    case CodeExplorerIconType.RuntimeBlock:
                        return new AnonymousCodeItem();

                    case CodeExplorerIconType.Procedure:
                        return new ProcedureCodeItem();
                    case CodeExplorerIconType.ExternalProcedure:
                        return new ExternalProcedureCodeItem();

                    default:
                        throw new Exception("Missing type " + type + " for the factory!");
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Anonymous code item for branches or other item that don't need a type button on the bottom
    /// </summary>
    internal class AnonymousCodeItem : CodeItem {
        /// <summary>
        /// don't display a type button for those
        /// </summary>
        public override int ItemType {
            get { return -1; }
        }

        public override Image ItemImage {
            get { return Utils.GetImageFromStr(Type.ToString()); }
        }
    }

    /// <summary>
    /// Anonymous code item for branches or other item that don't need a type button on the bottom
    /// </summary>
    internal class BranchCodeItem : AnonymousCodeItem {
        public BranchCodeItem() {
            // expanded by default
            IsExpanded = true;
        }

        /// <summary>
        /// Should this item be hidden when in searching mode?
        /// </summary>
        public override bool HideWhileSearching {
            get { return true; }
        }
    }

    /// <summary>
    /// Corresponds to an image, displayed on the left of an item
    /// </summary>
    internal enum CodeExplorerIconType {
        Root,

        Block,
        DefinitionBlock,
        XtfrBlock,
        PreprocessorBlock,
        Prototype,
        SettingsBlock,
        CreateWindowBlock,
        RuntimeBlock,

        ProgramParameter,
        Parameter,

        OnEvent,

        MainBlock,

        Include,

        ExternalProcedure,
        Procedure,
        Function,

        Subscribe,
        Publish,
        Unsubscribe,

        DefinedTempTable,
        Table,
        TempTable,

        RunInternal,
        RunExternal,

        StaticFunctionCall,
        DynamicFunctionCallExternal,
        DynamicFunctionCall,

        Browse,

        TempTableUsed,
        TableUsed
    }

    internal class RootCodeItem : CodeItem {
        public override CodeExplorerIconType Type {
            get { return CodeExplorerIconType.Root; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Root; }
        }
    }

    internal class BrowseCodeItem : CodeItem {
        public override CodeExplorerIconType Type {
            get { return CodeExplorerIconType.Browse; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Browse; }
        }
    }

    internal class ProcedureCodeItem : CodeItem {
        public override CodeExplorerIconType Type {
            get { return CodeExplorerIconType.Procedure; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Procedure; }
        }
    }

    internal class ExternalProcedureCodeItem : CodeItem {
        public override CodeExplorerIconType Type {
            get { return CodeExplorerIconType.ExternalProcedure; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.ExternalProcedure; }
        }
    }

    internal class FunctionCodeItem : CodeItem {
        public override CodeExplorerIconType Type {
            get { return CodeExplorerIconType.Function; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Function; }
        }
    }

    internal class OnEventCodeItem : CodeItem {
        public override CodeExplorerIconType Type {
            get { return CodeExplorerIconType.OnEvent; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.OnEvent; }
        }
    }

    internal class IncludeCodeItem : CodeItem {
        public override CodeExplorerIconType Type {
            get { return CodeExplorerIconType.Include; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Include; }
        }
    }

    internal class MainBlockCodeItem : CodeItem {
        public override CodeExplorerIconType Type {
            get { return CodeExplorerIconType.MainBlock; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.MainBlock; }
        }
    }

    internal class UnsubscribeCodeItem : CodeItem {
        public override CodeExplorerIconType Type {
            get { return CodeExplorerIconType.Unsubscribe; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Unsubscribe; }
        }
    }

    internal class SubscribeCodeItem : CodeItem {
        public override CodeExplorerIconType Type {
            get { return CodeExplorerIconType.Subscribe; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Subscribe; }
        }
    }

    internal class PublishCodeItem : CodeItem {
        public override CodeExplorerIconType Type {
            get { return CodeExplorerIconType.Publish; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Publish; }
        }
    }

    internal class PrototypeCodeItem : CodeItem {
        public override CodeExplorerIconType Type {
            get { return CodeExplorerIconType.Prototype; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Prototype; }
        }
    }

    internal class StaticFunctionCallCodeItem : CodeItem {
        public override CodeExplorerIconType Type {
            get { return CodeExplorerIconType.StaticFunctionCall; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.StaticFunctionCall; }
        }
    }

    internal class DynamicFunctionCallCodeItem : CodeItem {
        public override CodeExplorerIconType Type {
            get { return CodeExplorerIconType.DynamicFunctionCall; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.DynamicFunctionCall; }
        }
    }

    internal class DynamicFunctionCallExternalCodeItem : CodeItem {
        public override CodeExplorerIconType Type {
            get { return CodeExplorerIconType.DynamicFunctionCallExternal; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.DynamicFunctionCallExternal; }
        }
    }

    internal class RunInternalCodeItem : CodeItem {
        public override CodeExplorerIconType Type {
            get { return CodeExplorerIconType.RunInternal; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.RunInternal; }
        }
    }

    internal class RunExternalCodeItem : CodeItem {
        public override CodeExplorerIconType Type {
            get { return CodeExplorerIconType.RunExternal; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.RunExternal; }
        }
    }

    internal class TableCodeItem : CodeItem {
        public override CodeExplorerIconType Type {
            get { return CodeExplorerIconType.Table; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Table; }
        }
    }

    internal class TempTableCodeItem : CodeItem {
        public override CodeExplorerIconType Type {
            get { return CodeExplorerIconType.TempTable; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.TempTable; }
        }
    }

    internal class ParameterCodeItem : CodeItem {
        public override CodeExplorerIconType Type {
            get { return CodeExplorerIconType.Parameter; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Parameter; }
        }
    }
}