#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (CodeExplorerItem.cs) is part of 3P.
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
using System.Drawing;
using YamuiFramework.Controls.YamuiList;
using _3PA.Images;
using _3PA.Lib;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.CodeExplorer {

    /// <summary>
    /// base class
    /// </summary>
    internal class CodeExplorerItem : FilteredTypeTreeListItem {
       
        /// <summary>
        /// the branch to which this item belongs (if the item is not part of the "root")
        /// </summary>
        public CodeExplorerBranch Branch { get; set; }

        /// <summary>
        /// corresponds to the icon displayed next to DisplayText, if set to 0 (=BranchIcon) then the icon
        /// chosen for this item is the icon corresponding to the branch
        /// </summary>
        public CodeExplorerIconType IconType { get; set; }

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
        public CodeExplorerFlag Flag { get; set; }

        /// <summary>
        /// The string to display right next to the Flags
        /// </summary>
        public string SubString { get; set; }

        /// <summary>
        /// Set this to true if the item doesn't represent a block and therefor should not have a "mouse" selection
        /// image on the right
        /// </summary>
        public bool IsNotBlock { get; set; }

        /// <summary>
        /// This item should be on the root of the tree?
        /// </summary>
        public bool IsRoot { get { return Branch == CodeExplorerBranch.Root || Branch == CodeExplorerBranch.MainBlock; } }

        /// <summary>
        /// Apply an action for each flag of the item
        /// </summary>
        /// <param name="toApplyOnFlag"></param>
        public void DoForEachFlag(Action<string, CodeExplorerFlag> toApplyOnFlag) {
            foreach (var name in Enum.GetNames(typeof(CodeExplorerFlag))) {
                CodeExplorerFlag flag = (CodeExplorerFlag)Enum.Parse(typeof(CodeExplorerFlag), name);
                if (flag == 0 || !Flag.HasFlag(flag)) continue;
                toApplyOnFlag(name, flag);
            }
        }


        /// <summary>
        /// The piece of text displayed in the list
        /// </summary>
        public override string DisplayText { get; set; }

        /// <summary>
        /// return the image to display for this item
        /// If null, the image corresponding to ItemTypeImage will be used instead
        /// </summary>
        public override Image ItemImage { get { return null; } }

        /// <summary>
        /// return this item type (a unique int for each item type)
        /// if the value is strictly inferior to 0, the button for this type will not appear
        /// on the bottom of list
        /// </summary>
        public override int ItemType { get { return (int) IconType; } }

        /// <summary>
        /// return the image that will be used to identify this item
        /// type, it will be used for the bottom buttons of the list
        /// All items of a given type should return the same image! The image used for the 
        /// bottom buttons will be that of the first item found for the given type
        /// </summary>
        public override Image ItemTypeImage {
            get {
                return Utils.GetImageFromStr(IconType > 0 ? IconType.ToString() : Branch.ToString());
            }
        }

        /// <summary>
        /// The text that describes this item type
        /// </summary>
        public override string ItemTypeText {
            get {
                return "Category : <span class='SubTextColor'><b>" + IconType + "</b></span><br><br>";
            }
        }

        /// <summary>
        /// return true if the item is to be highlighted
        /// </summary>
        public override bool IsRowHighlighted {
            get {
                var curScope = ParserHandler.GetScopeOfLine(Npp.Line.CurrentLine);
                return curScope != null && !IsNotBlock && DisplayText.Equals(curScope.Name);
            } 
        }

        /// <summary>
        /// return a string containing the subtext to display
        /// </summary>
        public override string SubText { get { return SubString; } }

        /// <summary>
        /// return a list of images to be displayed (in reverse order) for the item
        /// </summary>
        public override List<Image> TagImages {
            get {
                var outList = new List<Image>();
                foreach (var name in Enum.GetNames(typeof(CodeExplorerFlag))) {
                    CodeExplorerFlag flag = (CodeExplorerFlag)Enum.Parse(typeof(CodeExplorerFlag), name);
                    if (flag == 0 || !Flag.HasFlag(flag)) continue;

                    Image tryImg = (Image)ImageResources.ResourceManager.GetObject(name);
                    if (tryImg != null)
                        outList.Add(tryImg);
                }
                return outList;
            }
        }

        /// <summary>
        /// to override, that should return the list of the children for this item (if any) or null
        /// </summary>
        public override List<FilteredTypeTreeListItem> Children { get; set; }

    }

    /// <summary>
    /// Defines the different types of explorerItems (is related to the image display in the code explorer)
    /// The attribute is used for the text displayed for the branch
    /// ((ExplorerTypeAttr)ExplorerType.GetAttributes()).DisplayText
    /// </summary>
    internal enum CodeExplorerBranch {
        [Description("Everything in code order")]
        EverythingInCodeOrder,
        [Description("Root")]
        Root,
        [Description("Appbuilder blocks")]
        Block,
        [Description("Main block")]
        MainBlock,
        [Description("Procedures")]
        Procedure,
        [Description("Functions")]
        Function,
        [Description("ON events")]
        OnEvent,
        [Description("Includes")]
        Include,
        [Description("Run statements")]
        Run,
        [Description("Dynamic function calls")]
        DynamicFunctionCall,
        [Description("Browse definitions")]
        Browse,
        [Description("Tables used")]
        TableUsed,
        [Description("Program parameters")]
        ProgramParameter,
        [Description("Temp-tables used")]
        TempTableUsed
    }

    /// <summary>
    /// Corresponds to an image, displayed on the left of an item
    /// </summary>
    internal enum CodeExplorerIconType {
        BranchIcon,
        DefinitionBlock,
        XtfrBlock,
        PreprocessorBlock,
        Prototype,
        SettingsBlock,
        CreateWindowBlock,
        RuntimeBlock,
        FunctionCallInternal,
        FunctionCallExternal,
        RunInternal,
        RunExternal,
        Table,
        TempTable,
        Parameter
    }

    [Flags]
    internal enum CodeExplorerFlag {
        // the block has too much characters and the program will not be open-able in the appbuilder
        IsTooLong = 1,
        // applies for Run statement, the program/proc to run is VALUE(something) so we only guess which one it is
        Uncertain = 2,
        // if a table found w/o the database name before it
        MissingDbName = 4,
        // if the .i file is not found in the propath
        NotFound = 8,
        // if the extracted info is from an external file (.i)
        External = 16,
        // a run file has the keyword PERSISTENT
        LoadPersistent = 32,
        // a proc or func was loaded in persistent
        Persistent = 64,
        // private proc
        Private = 128,
        // is a buffer
        Buffer = 256
    }
}
