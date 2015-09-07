using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware.Utilities;
using _3PA.Images;

namespace _3PA.MainFeatures.DockableExplorer {

    public static class ExplorerContent {

        private static List<ExplorerCategories> _categories;
        private static List<ExplorerCategories> _unsortedCategory;
        private static List<ExplorerItems> _items;
        private static List<ExplorerItems> _sortedItems; 

        public static void Init() {
            _categories = new List<ExplorerCategories>() {
                new ExplorerCategories() { DisplayText = "MainBlock", MyIcon = IconType.MainBlock, HasChildren = false },
                new ExplorerCategories() { DisplayText = "Definition", MyIcon = IconType.Definition, HasChildren = false },
                new ExplorerCategories() { DisplayText = "Procedures", MyIcon = IconType.Procedures, HasChildren = true},
                new ExplorerCategories() { DisplayText = "Functions", MyIcon = IconType.Functions, HasChildren = true}
            };
            _unsortedCategory = new List<ExplorerCategories>() {
                new ExplorerCategories() { DisplayText = "Everything in code order", MyIcon = IconType.EverythingCodeOrder, HasChildren = true }
            };
        }

        public static void SetItems(List<ExplorerItems> items) {
            _sortedItems = items;
            _items = items.OrderBy(explorerItems => explorerItems.DisplayText).ToList();
        }

        public static List<ExplorerItems> GetItemsFor(IconType type) {
            if (type != IconType.EverythingCodeOrder)
                return _items.Where(item => item.MyIcon == type).ToList();
            else
                return _sortedItems;
        }

        public static List<ExplorerCategories> Categories {
            get { return _categories; }
        }

        public static List<ExplorerCategories> UnsortedCategory {
            get { return _unsortedCategory; }
        }

        /// <summary>
        /// Returns the Image list used by the tree view, should correspond to the IconType!
        /// same order!
        /// </summary>
        /// <returns></returns>
        public static ImageList GetImageList() {
            var imageListOfTypes = new ImageList {
                TransparentColor = Color.Transparent,
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(20, 20)
            };
            ImagelistAdd.AddFromImage(ImageResources.code, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.exterior, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.TempTable, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Field, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.FieldPk, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Snippet, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Function, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Procedure, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.UserVariablePrimitive, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.UserVariableOther, imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Preprocessed, imageListOfTypes);
            return imageListOfTypes;
        }
    }

    /// <summary>
    /// Defines the different icons used by the tree view (see GetImageList)
    /// </summary>
    public enum IconType {
        EverythingCodeOrder,
        MainBlock,
        Definition,
        Procedures,
        Functions
    }

    public class ExplorerCategories {
        public string DisplayText { get; set; }

        public IconType MyIcon { get; set; }

        public List<ExplorerItems> Items {
            get {
                if (_items == null)
                    _items = ExplorerContent.GetItemsFor(MyIcon);
                return _items;
            }
        }
        private List<ExplorerItems> _items; 

        public bool HasChildren { get; set; }
    }

    public class ExplorerItems {
        public string DisplayText { get; set; }

        public IconType MyIcon { get; set; }

        public int GoToLine { get; set; }
    }

}
