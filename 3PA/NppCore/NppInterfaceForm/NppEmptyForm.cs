using System.Drawing;
using System.Windows.Forms;
using YamuiFramework.Helper;

namespace _3PA.NppCore.NppInterfaceForm {
    /// <summary>
    /// An empty form that does absolutely nothing
    /// </summary>
    internal class NppEmptyForm : Form {

        #region ShowWithoutActivation & Don't show in ATL+TAB

        /// <summary>
        /// This indicates that the form should not take focus when shown
        /// specify it through the CreateParams
        /// </summary>
        protected override bool ShowWithoutActivation {
            get { return true; }
        }

        /// <summary>
        /// Don't show in ATL+TAB
        /// </summary>
        protected override CreateParams CreateParams {
            get {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= (int)WinApi.WindowStylesEx.WS_EX_TOOLWINDOW;
                return createParams;
            }
        }

        #endregion

        public NppEmptyForm() {
            Visible = false;
            ControlBox = false;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            ClientSize = new Size(1, 1);
        }
    }
}
