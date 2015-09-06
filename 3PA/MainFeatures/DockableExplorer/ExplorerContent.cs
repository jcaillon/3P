using System;
using System.Collections.Generic;
using System.Linq;

namespace _3PA.MainFeatures.DockableExplorer {

    public static class ExplorerContent {

        private static List<ExplorerCategories> _categories;
        private static List<ExplorerItems> _items; 

        public static void Init() {
            _categories = new List<ExplorerCategories>() {
                new ExplorerCategories() { DisplayText = "MainBlock", MyIcon = IconType.MainBlock, HasChildren = false },
                new ExplorerCategories() { DisplayText = "Definition", MyIcon = IconType.Definition, HasChildren = false },
                new ExplorerCategories() { DisplayText = "Procedures", MyIcon = IconType.Procedures, HasChildren = true},
                new ExplorerCategories() { DisplayText = "Functions", MyIcon = IconType.Functions, HasChildren = true}
            };

            _items = new List<ExplorerItems>() {
                new ExplorerItems() {DisplayText = "function 1", MyIcon = IconType.Functions},
                new ExplorerItems() {DisplayText = "function 2", MyIcon = IconType.Functions},
                new ExplorerItems() {DisplayText = "function 3", MyIcon = IconType.Functions},
                new ExplorerItems() {DisplayText = "function 4", MyIcon = IconType.Functions},
                new ExplorerItems() {DisplayText = "proc 1", MyIcon = IconType.Procedures}
            };
        }

        public static List<ExplorerItems> GetItemsFor(IconType type) {
            return _items.Where(item => item.MyIcon == type).ToList();
        }

        public static List<ExplorerCategories> Categories {
            get { return _categories; }
        }
    }
    public enum IconType {
        MainBlock,
        Definition,
        Procedures,
        Functions
    }

    public class ExplorerCategories {
        public string DisplayText { get; set; }

        public IconType MyIcon { get; set; }

        private List<ExplorerItems> _items; 

        public List<ExplorerItems> Items {
            get {
                if (_items == null)
                    _items = ExplorerContent.GetItemsFor(MyIcon);
                return _items;
            }
        }

        public bool HasChildren { get; set; }
    }

    public class ExplorerItems {
        public string DisplayText { get; set; }

        public IconType MyIcon { get; set; }

        public int GoToLine { get; set; }
    }

}
