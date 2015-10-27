using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamuiFramework.Forms;
using _3PA.Html;
using _3PA.MainFeatures;

namespace _3PA.Lib {
    class ErrorHandler {

        private static Dictionary<string, bool> _catchedErrors = new Dictionary<string, bool>();

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

            // don't show/store the same error twice in a session
            if (_catchedErrors.ContainsKey(errorToStr))
                return;
            _catchedErrors.Add(errorToStr, true);

            // log the error into a file

            // show it to the user, conditionally
            if (!Config.Instance.GlobalShowAllErros)
                return;

            UserCommunication.Notify(errorToStr.Replace("à", "<br>à"), MessageImage.Poison, "Oops an error has occured!", message, 0, 500);
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
