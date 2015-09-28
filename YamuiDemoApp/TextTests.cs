using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using _3PA;

namespace YamuiDemoApp {
    class TextTests {

        public static void Run() {

            var str = "(database.table";
            int nb;
            var tt = Abl.ReadAblWord(str, false, out nb);
            if (nb == 1) {
                tt = tt.Split('.')[0];
                MessageBox.Show(tt);
            }

            var str2 = "zefrferz(zeffzef.mykeyword:";
            var tt2 = Abl.ReadAblWord(str2, true);
            int startPos = str2.Length - 1 - tt2.Length;
            if (startPos >= 0)
                MessageBox.Show(str2.Substring(startPos, 1));

            MessageBox.Show((" ".ToCharArray()[0]).ToString());
        }
    }
}
