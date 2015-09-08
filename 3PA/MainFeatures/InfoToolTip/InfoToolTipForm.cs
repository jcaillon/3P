using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace _3PA.MainFeatures.ToolTip {
    public partial class InfoToolTipForm : NppInterfaceForm.NppInterfaceForm {



        #region constructor

        public InfoToolTipForm() {
            InitializeComponent();
        }

        #endregion

        #region public
        public void SetText(string content) {
            labelContent.Text = content;
        }

        public void SetPosition() {
            Location = new Point(500, 500);
        }
        #endregion


    }
}
