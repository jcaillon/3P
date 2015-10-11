
using System;
using System.Collections.Generic;
using _3PA.Lib;

namespace _3PA.MainFeatures.DockableExplorer {

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
    }

    [Flags]
    public enum CodeExplorerFlag {
        IsTooLong = 1,
        HasChildren = 2,
        Uncertain = 4,
    }

    public class CodeExplorerTypeAttr : Extensions.EnumAttr {
        public string DisplayText { get; set; }
        public int Order { get; set; }
    }
}
