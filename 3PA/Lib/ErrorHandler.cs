using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using _3PA.MainFeatures;

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
            var errorToStr = e.ToString();

            // log the error into a file

            // show it to the user, conditionally
            if (!Config.Instance.GlobalShowAllErros)
                return;
            UserCommunication.Notify(@"<img src='poison' /><b class='NotificationTitle'>Oops an error has occured!</b><br><b>" + message + @"</b><br><br>" + errorToStr.Replace("à", "<br>à"), 0, Screen.PrimaryScreen.WorkingArea.Height / 3);
        }

        public static void UnhandledErrorHandler(object sender, UnhandledExceptionEventArgs args) {
            ShowErrors((Exception) args.ExceptionObject, "Unhandled error!");
        }

        public static void ThreadErrorHandler(object sender, ThreadExceptionEventArgs e) {
            ShowErrors(e.Exception, "Thread error!");
        }

        public static void UnobservedErrorHandler(object sender, UnobservedTaskExceptionEventArgs e) {
            ShowErrors(e.Exception, "Unobserved task error!");
        }
    }
}
