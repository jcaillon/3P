
using System.Collections.Generic;

namespace _3PA.MainFeatures.DockableExplorer {

    /// <summary>
    /// Defines the different icons used by the tree view (see GetImageList)
    /// </summary>
    public enum IconType {
        EverythingInCodeOrder = 0,
        DefinitionsBlock,
        PreprocessorBlock,
        MainBlock,
        Procedures,
        Functions,
        OnEvents,
        Includes = 7,
        Run
    }

    /// <summary>
    /// base class
    /// </summary>
    public abstract class ExplorerObject {
        public string DisplayText { get; set; }

        public IconType IconType { get; set; }

        public int GoToLine { get; set; }
    }

    /// <summary>
    /// represent a root item
    /// </summary>
    public class ExplorerCategories : ExplorerObject {
        /// <summary>
        /// List of children items
        /// </summary>
        public List<ExplorerItems> Items {
            get {
                if (_items == null)
                    _items = CodeExplorer.GetItemsFor(IconType);
                return _items;
            }
        }
        private List<ExplorerItems> _items; 

        /// <summary>
        /// Does it have children?
        /// </summary>
        public bool HasChildren { get; set; }
    }

    /// <summary>
    /// represent a child item
    /// </summary>
    public class ExplorerItems : ExplorerObject { }
}
