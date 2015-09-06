using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace _3PA.Lib {
    class ErrorHandler {

        public static void ShowErrors(Exception e, string message, string fileName) {
            MessageBox.Show("Error in " + AssemblyInfo.ProductTitle + ", couldn't load the following file : \n" +
                            fileName +
                            "\nThe file has been renamed with the '_errors' suffix to avoid further problems.");
            if (File.Exists(fileName + "_errors"))
                File.Delete(fileName + "_errors");
            File.Move(fileName, fileName + "_errors");
            ShowErrors(e, message);
        }

        public static void ShowErrors(Exception e, string message) {
#if DEBUG
            MessageBox.Show("Custom error : " + message + "\n" + e.ToString());
#else
#endif
        }
    }
}
