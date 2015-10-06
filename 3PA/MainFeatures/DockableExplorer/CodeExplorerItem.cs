
using System.Collections.Generic;
using _3PA.Lib;

namespace _3PA.MainFeatures.DockableExplorer {

    /// <summary>
    /// base class
    /// </summary>
    public class ExplorerItem {
        public string DisplayText { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ExplorerType Type { get; set; }

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
                    _items = CodeExplorerPage.GetItemsFor(Type);
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
        Procedures,
        [ExplorerTypeAttr(DisplayText = "Functions")]
        Functions,
        [ExplorerTypeAttr(DisplayText = "ON events")]
        OnEvents,
        [ExplorerTypeAttr(DisplayText = "Includes")]
        Includes,
        [ExplorerTypeAttr(DisplayText = "Run statements")]
        Run
    }

    public class ExplorerTypeAttr : Extensions.EnumAttr {
        public string DisplayText { get; set; }
    }
}
