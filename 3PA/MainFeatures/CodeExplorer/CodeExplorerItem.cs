#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
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
using _3PA.Lib;
using _3PA.MainFeatures.FilteredLists;

namespace _3PA.MainFeatures.CodeExplorer {

    /// <summary>
    /// base class
    /// </summary>
    internal class CodeExplorerItem : FilteredItemTree {
       
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
    }

    /// <summary>
    /// Defines the different types of explorerItems (is related to the image display in the code explorer)
    /// The attribute is used for the text displayed for the branch
    /// ((ExplorerTypeAttr)ExplorerType.GetAttributes()).DisplayText
    /// </summary>
    internal enum CodeExplorerBranch {
        [DisplayAttr(Name = "Everything in code order")]
        EverythingInCodeOrder,
        [DisplayAttr(Name = "Root")]
        Root,
        [DisplayAttr(Name = "Appbuilder blocks")]
        Block,
        [DisplayAttr(Name = "Main block")]
        MainBlock,
        [DisplayAttr(Name = "Procedures")]
        Procedure,
        [DisplayAttr(Name = "Functions")]
        Function,
        [DisplayAttr(Name = "ON events")]
        OnEvent,
        [DisplayAttr(Name = "Includes")]
        Include,
        [DisplayAttr(Name = "Run statements")]
        Run,
        [DisplayAttr(Name = "Dynamic function calls")]
        DynamicFunctionCall,
        [DisplayAttr(Name = "Browse definitions")]
        Browse,
        [DisplayAttr(Name = "Tables used")]
        TableUsed,
        [DisplayAttr(Name = "Program parameters")]
        ProgramParameter,
        [DisplayAttr(Name = "Temp-tables used")]
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
