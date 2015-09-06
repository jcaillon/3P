using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrightIdeasSoftware;
using BrightIdeasSoftware.Utilities;
using YamuiFramework.Fonts;
using YamuiFramework.Themes;
using _3PA.Images;
using _3PA.MainFeatures.AutoCompletion;

namespace _3PA.MainFeatures.DockableExplorer {
    public partial class DockableExplorerForm : Form {

        private ImageList _imageListOfTypes;

        public DockableExplorerForm() {
            InitializeComponent();

            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);


            // Can the given object be expanded?
            ovlTree.CanExpandGetter = delegate(Object x) {
                return (x is ExplorerCategories) && ((ExplorerCategories)x).HasChildren;
            };

            // What objects should belong underneath the given model object?
            ovlTree.ChildrenGetter = delegate(Object x) {
                if (x is ExplorerCategories)
                    return ((ExplorerCategories)x).Items;
                throw new ArgumentException("??");
            };

            // set the image list to use for the keywords
            _imageListOfTypes = new ImageList {
                TransparentColor = Color.Transparent,
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(20, 20)
            };
            ImagelistAdd.AddFromImage(ImageResources.Keyword, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Table, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.TempTable, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Field, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.FieldPk, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Snippet, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Function, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Procedure, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.UserVariablePrimitive, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.UserVariableOther, _imageListOfTypes);
            ImagelistAdd.AddFromImage(ImageResources.Preprocessed, _imageListOfTypes);
            ovlTree.SmallImageList = _imageListOfTypes;
            DisplayText.ImageGetter += rowObject => {
                if (rowObject is ExplorerCategories) {
                    var x = (ExplorerCategories)rowObject;
                    return (int) x.MyIcon;
                }
                var y = (ExplorerItems)rowObject;
                return (int)y.MyIcon;
            };


            // Style the control
            ovlTree.OwnerDraw = true;
            ovlTree.Font = FontManager.GetLabelFont(LabelFunction.AutoCompletion);
            ovlTree.BackColor = ThemeManager.Current.AutoCompletionNormalBackColor;
            ovlTree.AlternateRowBackColor = ThemeManager.Current.AutoCompletionNormalAlternateBackColor;
            ovlTree.ForeColor = ThemeManager.Current.AutoCompletionNormalForeColor;
            ovlTree.HighlightBackgroundColor = ThemeManager.Current.AutoCompletionFocusBackColor;
            ovlTree.HighlightForegroundColor = ThemeManager.Current.AutoCompletionFocusForeColor;
            ovlTree.UnfocusedHighlightBackgroundColor = ovlTree.HighlightBackgroundColor;
            ovlTree.UnfocusedHighlightForegroundColor = ovlTree.HighlightForegroundColor;

            // Decorate and configure hot item
            ovlTree.UseHotItem = true;
            ovlTree.HotItemStyle = new HotItemStyle();
            ovlTree.HotItemStyle.BackColor = ThemeManager.Current.AutoCompletionHoverBackColor;
            ovlTree.HotItemStyle.ForeColor = ThemeManager.Current.AutoCompletionHoverForeColor;

            // overlay of empty list :
            ovlTree.EmptyListMsg = "Nothing to see here!";
            TextOverlay textOverlay = ovlTree.EmptyListMsgOverlay as TextOverlay;
            if (textOverlay != null) {
                textOverlay.TextColor = ThemeManager.Current.AutoCompletionNormalForeColor;
                textOverlay.BackColor = ThemeManager.Current.AutoCompletionNormalAlternateBackColor;
                textOverlay.BorderColor = ThemeManager.Current.AutoCompletionNormalForeColor;
                textOverlay.BorderWidth = 4.0f;
                textOverlay.Font = FontManager.GetFont(FontStyle.Bold, 30f);
                textOverlay.Rotation = -5;
            }

        }

        #region Paint Methods
        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e) {
            var backColor = ThemeManager.Current.FormColorBackColor;
            e.Graphics.Clear(backColor);
        }
        #endregion

        /// <summary>
        /// Check/uncheck the menu depending on this form visibility
        /// </summary>
        /// <param name="e"></param>
        protected override void OnVisibleChanged(EventArgs e) {
            DockableExplorer.UpdateMenuItemChecked();
            base.OnVisibleChanged(e);
        }

        protected override void OnLoad(EventArgs e) {
            RefreshExplorer();
            base.OnLoad(e);
        }

        public void RefreshExplorer() {
            ExplorerContent.Init();
            ovlTree.SetObjects(ExplorerContent.Categories);
        }
    }
}
