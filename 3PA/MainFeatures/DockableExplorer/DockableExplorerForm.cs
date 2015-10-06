using System;
using System.Windows.Forms;
using YamuiFramework.Themes;

namespace _3PA.MainFeatures.DockableExplorer {
    public partial class DockableExplorerForm : Form {

        #region fields

        /// <summary>
        /// Access the codeExplorer component
        /// </summary>
        public CodeExplorerPage CodeExplorerPage {
            get { return _codeExplorerPage; }
        }

        #endregion

        #region constructor

        public DockableExplorerForm() {
            InitializeComponent();

            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);
        }

        #endregion

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

    }
}
