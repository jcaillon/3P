
using System.Collections.Generic;
using _3PA.Lib;

namespace _3PA.MainFeatures.DockableExplorer {

    /// <summary>
    /// base class
    /// </summary>
    public class ExplorerItem {
        public string DisplayText { get; set; }

        /// <summary>
        /// the type of the item, also corresponds to the icon displayed next to DisplayText
        /// </summary>
        public ExplorerType Type { get; set; }
        
        /// <summary>
        /// the branch to which this item belongs (if the item is not part of the "root")
        /// </summary>
        public ExplorerType BranchType { get; set; }

        /// <summary>
        /// if the item has no children, clicking on it will make the carret move to this line
        /// </summary>
        public int GoToLine { get; set; }

        /// <summary>
        /// Set to true if the item should appear in the tree's root and not under a branch
        /// </summary>
        public bool IsRoot { get; set; }

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
        public List<ExplorerItem> Items {
            get {
                if (_items == null)
                    _items = CodeExplorerPage.GetItemsFor(BranchType);
                return _items;
            }
        }
        private List<ExplorerItem> _items;
    }

    /// <summary>
    /// Defines the different types of explorerItems (is related to the image display in the code explorer)
    /// ((ExplorerTypeAttr)ExplorerType.GetAttributes()).DisplayText
    /// </summary>
    public enum ExplorerType {
        [ExplorerTypeAttr(DisplayText = "Everything in code order")]
        EverythingInCodeOrder,
        [ExplorerTypeAttr(DisplayText = "Root")]
        Root,
        [ExplorerTypeAttr(DisplayText = "Appbuilder blocks")]
        Block,
        [ExplorerTypeAttr(DisplayText = "Main block")]
        MainBlock,
        [ExplorerTypeAttr(DisplayText = "Procedures")]
        Procedure,
        [ExplorerTypeAttr(DisplayText = "Functions")]
        Function,
        [ExplorerTypeAttr(DisplayText = "ON events")]
        OnEvent,
        [ExplorerTypeAttr(DisplayText = "Includes")]
        Include,
        [ExplorerTypeAttr(DisplayText = "Run statements")]
        Run,
        [ExplorerTypeAttr(DisplayText = "Browse definitions")]
        Browse,
        [ExplorerTypeAttr(DisplayText = "")]
        DefinitionBlock,
        [ExplorerTypeAttr(DisplayText = "")]
        XtfrBlock,
        [ExplorerTypeAttr(DisplayText = "")]
        PreprocessorBlock,
        [ExplorerTypeAttr(DisplayText = "")]
        Prototype,
        [ExplorerTypeAttr(DisplayText = "")]
        SettingsBlock,
        [ExplorerTypeAttr(DisplayText = "")]
        CreateWindowBlock,
        [ExplorerTypeAttr(DisplayText = "")]
        RuntimeBlock,
    }

    public class ExplorerTypeAttr : Extensions.EnumAttr {
        public string DisplayText { get; set; }
        public int Order { get; set; }
    }
}
