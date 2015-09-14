using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using _3PA.Interop;

namespace _3PA.MainFeatures.Lexer {
    class Lexer {
        public static void Colorize(int endPos) {
            Npp.SetStatusbarLabel("styling...");
            int startPos = Npp.GetSylingNeededStartPos();
            SetCustomStyles();
            //            SCNotification* notify = (SCNotification*)nmhdr;
            //const int line_number = SendEditor(SCI_LINEFROMPOSITION, SendEditor(SCI_GETENDSTYLED));
            //const int start_pos = SendEditor(SCI_POSITIONFROMLINE, (WPARAM)line_number);
            // int end_pos = ;
            Npp.StyleText(32, startPos, endPos);
        }

        public static void SetCustomStyles() {
            Npp.SetDefaultStyle(Color.BlueViolet, Color.Crimson);
        }

    }
}
