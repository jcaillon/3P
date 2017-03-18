using System.Drawing;
using System.Windows.Forms;

namespace _3PA.NppCore.NppInterfaceForm {
    /// <summary>
    /// An empty form that does absolutely nothing
    /// </summary>
    internal class NppEmptyForm : Form {

        protected override bool ShowWithoutActivation {
            get { return true; }
        }

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
