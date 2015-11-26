#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
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
using _3PA.Lib;

namespace _3PA.MainFeatures.CodeExplorer {

    /// <summary>
    /// base class
    /// </summary>
    public class CodeExplorerItem {
        public string DisplayText { get; set; }
       
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
        /// if the item has no children, clicking on it will make the carret move to this line
        /// </summary>
        public int GoToLine { get; set; }

        /// <summary>
        /// if the item has no children, clicking on it will make the carret move to this GoToColumn
        /// </summary>
        public int GoToColumn { get; set; }

        /// <summary>
        /// the document in which the item was found, for example if we found a procedure block in a .i
        /// we want to switch to the owning document before going to the line
        /// </summary>
        public string DocumentOwner { get; set; }

        /// <summary>
        /// The level of the item defines its place in the tree, level 0 is the root, 1 is deeper and so on...
        /// </summary>
        public int Level { 
            get { return _level; }
            set { _level = value; }
        }
        private int _level = 1;

        /// <summary>
        /// Flags for the item, is directly related to the images displayed on the right of the item
        /// </summary>
        public CodeExplorerFlag Flag { get; set; }

        /// <summary>
        /// The string to display right next to the Flags
        /// </summary>
        public string SubString { get; set; }

        /// <summary>
        /// Does it have children?
        /// </summary>
        public bool HasChildren { get; set; }

        /// <summary>
        /// Set this to true if the item doesn't represent a block and therefor should not have a "mouse" selection
        /// image on the right
        /// </summary>
        public bool IsNotBlock { get; set; }

        /// <summary>
        /// unique identifier for this item
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// List of children items
        /// </summary>
        public List<CodeExplorerItem> Items {
            get {
                if (_items == null)
                    _items = CodeExplorerPage.GetItemsFor(this);
                return _items;
            }
        }
        private List<CodeExplorerItem> _items;
    }

    /// <summary>
    /// Defines the different types of explorerItems (is related to the image display in the code explorer)
    /// The attribute is used for the text displayed for the branch
    /// ((ExplorerTypeAttr)ExplorerType.GetAttributes()).DisplayText
    /// </summary>
    public enum CodeExplorerBranch {
        [CodeExplorerTypeAttr(DisplayText = "Everything in code order")]
        EverythingInCodeOrder,
        [CodeExplorerTypeAttr(DisplayText = "Root")]
        Root,
        [CodeExplorerTypeAttr(DisplayText = "Appbuilder blocks")]
        Block,
        [CodeExplorerTypeAttr(DisplayText = "Main block")]
        MainBlock,
        [CodeExplorerTypeAttr(DisplayText = "Procedures")]
        Procedure,
        [CodeExplorerTypeAttr(DisplayText = "Functions")]
        Function,
        [CodeExplorerTypeAttr(DisplayText = "ON events")]
        OnEvent,
        [CodeExplorerTypeAttr(DisplayText = "Includes")]
        Include,
        [CodeExplorerTypeAttr(DisplayText = "Run statements")]
        Run,
        [CodeExplorerTypeAttr(DisplayText = "Dynamic function calls")]
        DynamicFunctionCall,
        [CodeExplorerTypeAttr(DisplayText = "Browse definitions")]
        Browse,
        [CodeExplorerTypeAttr(DisplayText = "Tables used")]
        TableUsed,
        [CodeExplorerTypeAttr(DisplayText = "Program parameters")]
        ProgramParameter,
        
    }

    /// <summary>
    /// Corresponds to an image, displayed on the left of an item
    /// </summary>
    public enum CodeExplorerIconType {
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
    public enum CodeExplorerFlag {
        // the block has too much characters and the program will not be openable in the appbuilder
        IsTooLong = 1,
        // applies for Run statement, the program/proc to run is VALUE(something) so we only guess which one it is
        Uncertain = 2,
        // if a table found w/o the database name before it
        MissingDbName = 4,
        // if the .i file is not found in the propath
        NotFound = 8,
        // if the extracted info is from an external file (.i)
        External = 16,
    }

    public class CodeExplorerTypeAttr : Extensions.EnumAttr {
        public string DisplayText { get; set; }
        public int Order { get; set; }
    }
}
