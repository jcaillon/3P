using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using _3PA.Interop;
using _3PA.Properties;

namespace _3PA {
    /// <summary>
    ///     This class contains very generic wrappers for basic Notepad++ functionality.
    /// </summary>
    public partial class Npp {

        /// <summary>
        ///     Gets the Notepad++ main window handle.
        /// </summary>
        /// <value>
        ///     The Notepad++ main window handle.
        /// </value>
        public static IntPtr HandleNpp {
            get { return Plug.NppData._nppHandle; }
        }

        /// <summary>
        ///     Get the IWin32Window of the Npp window
        ///     Must be used as an input for forms.Show() in order to link the create form to the Npp window
        ///     if the user switches applications, the dialog hides with Notepad++
        /// </summary>
        public static IWin32Window Win32WindowNpp {
            get { return new WindowWrapper(HandleNpp); }
        }


        public static void SetToolbarImage(Bitmap image, int pluginId) {
            var tbIcons = new toolbarIcons { hToolbarBmp = image.GetHbitmap() };
            var pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
            Marshal.StructureToPtr(tbIcons, pTbIcons, false);
            Win32.SendMessage(HandleNpp, NppMsg.NPPM_ADDTOOLBARICON, Plug.FuncItems.Items[pluginId]._cmdID, pTbIcons);
            Marshal.FreeHGlobal(pTbIcons);
        }

        /// <summary>
        /// Gets the file path of each file currently opened, return
        /// the files separated by a new line
        /// </summary>
        /// <returns></returns>
        public static string GetOpenedFiles() {
            var output = new StringBuilder();
            int nbFile = (int)Win32.SendMessage(HandleNpp, NppMsg.NPPM_GETNBOPENFILES, 0, 0);
            using (ClikeStringArray cStrArray = new ClikeStringArray(nbFile, Win32.MAX_PATH)) {
                if (Win32.SendMessage(HandleNpp, NppMsg.NPPM_GETOPENFILENAMES, cStrArray.NativePointer, nbFile) != IntPtr.Zero)
                    foreach (string file in cStrArray.ManagedStringsUnicode) output.AppendLine(file);
            }
            return output.ToString();
        }

        /// <summary>
        /// Gets the file path of each file in the session file, return
        /// the files separated by a new line
        /// </summary>
        /// <param name="sessionFilePath"></param>
        /// <returns></returns>
        public static string GetSessionFiles(string sessionFilePath) {
            var output = new StringBuilder();
            int nbFile = (int)Win32.SendMessage(HandleNpp, NppMsg.NPPM_GETNBSESSIONFILES, 0, sessionFilePath);
            if (nbFile > 0) {
                using (ClikeStringArray cStrArray = new ClikeStringArray(nbFile, Win32.MAX_PATH)) {
                    if (Win32.SendMessage(HandleNpp, NppMsg.NPPM_GETSESSIONFILES, cStrArray.NativePointer, sessionFilePath) != IntPtr.Zero)
                        foreach (string file in cStrArray.ManagedStringsUnicode) output.AppendLine(file);
                }
            }
            return output.ToString();
        }

        /// <summary>
        /// Saves the session into a file
        /// </summary>
        /// <param name="sessionFilePath"></param>
        /// <returns></returns>
        public static bool SaveSession(string sessionFilePath) {
            string sessionPath = Marshal.PtrToStringUni(Win32.SendMessage(HandleNpp, NppMsg.NPPM_SAVECURRENTSESSION, 0, sessionFilePath));
            return !string.IsNullOrEmpty(sessionPath);
        }

        /// <summary>
        ///     Gets the name of the current document.
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentDocument() {
            string path;
            Win32.SendMessage(HandleNpp, NppMsg.NPPM_GETFULLCURRENTPATH, 0, out path);
            return path;
        }

        /// <summary>
        ///     Saves the current document.
        /// </summary>
        public static void SaveCurrentDocument() {
            Win32.SendMessage(HandleNpp, NppMsg.NPPM_SAVECURRENTFILE, 0, 0);
        }

        public static string GetCurrentFileName() {
            return Path.GetFileName(GetCurrentFile());
        }

        public static string GetCurrentFile() {
            var path = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(HandleNpp, NppMsg.NPPM_GETFULLCURRENTPATH, 0, path);
            return path.ToString();
        }

        public static void DisplayInNewDocument(string text) {
            Win32.SendMessage(HandleNpp, NppMsg.NPPM_MENUCOMMAND, 0, NppMenuCmd.FileNew);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_GRABFOCUS, 0, 0);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_ADDTEXT, text);
        }



        /// <summary>
        ///     Determines whether the current file has the specified extension (e.g. ".cs").
        ///     <para>Note it is case insensitive.</para>
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <returns></returns>
        public static bool IsCurrentFileHasExtension(string extension) {
            var path = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(HandleNpp, NppMsg.NPPM_GETFULLCURRENTPATH, 0, path);
            var file = path.ToString();
            return !string.IsNullOrWhiteSpace(file) && file.EndsWith(extension, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// returns the current file's extension
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentFileExtension() {
            string currentFileExtension;
            Win32.SendMessage(HandleNpp, NppMsg.NPPM_GETEXTPART, 0, out currentFileExtension);
            return currentFileExtension;
        }


        public static string GetConfigDir() {
            var buffer = new StringBuilder(260);
            Win32.SendMessage(HandleNpp, NppMsg.NPPM_GETPLUGINSCONFIGDIR, 260, buffer);
            var configDir = Path.Combine(buffer.ToString(), Resources.PluginFolderName);
            if (!Directory.Exists(configDir))
                Directory.CreateDirectory(configDir);
            return configDir;
        }


        public static void Exit() {
            const int wmCommand = 0x111;
            Win32.SendMessage(HandleNpp, (NppMsg)wmCommand, (int)NppMenuCmd.FileExit, 0);
        }

        public static void ShowInactiveTopmost(Form frm) {
            Win32.ShowWindow(frm.Handle, SwShownoactivate);
            //SetWindowPos(frm.Handle.ToInt32(), HWND_TOPMOST, frm.Left, frm.Top, frm.Width, frm.Height, SWP_NOACTIVATE);
            Win32.SetWindowPos(frm.Handle.ToInt32(), HandleNpp.ToInt32(), frm.Left, frm.Top, frm.Width, frm.Height,
                SwpNoactivate);
        }


        /// <summary>
        /// Sets the label in the status of npp (bottom right)
        /// </summary>
        /// <remarks>WARNING : THIS METHOD IS HIGHLY UNSTABLE, USE ONLY FOR DEBUG!</remarks>
        /// <param name="labelText"></param>
        /// <returns></returns>
        public static string SetStatusbarLabel(string labelText) {
            string retval = null;
            var mainWindowHandle = HandleNpp;
            // find status bar control on the main window of the application
            var statusBarHandle = Win32.FindWindowEx(mainWindowHandle, IntPtr.Zero, "msctls_statusbar32", IntPtr.Zero);
            if (statusBarHandle != IntPtr.Zero) {
                //cut current text
                var size = (int)Win32.SendMessage(statusBarHandle, SbGettextlength, 0, IntPtr.Zero);
                var buffer = new StringBuilder(size);
                Win32.SendMessage(statusBarHandle, (NppMsg)SbGettext, 0, buffer);
                retval = buffer.ToString();

                // set text for the existing part with index 0
                var text = Marshal.StringToHGlobalAuto(labelText);
                Win32.SendMessage(statusBarHandle, SbSettext, 0, text);
                Marshal.FreeHGlobal(text);

                //the foolowing may be needed for puture features
                // create new parts width array
                var nParts = Win32.SendMessage(statusBarHandle, SbGetparts, 0, IntPtr.Zero).ToInt32();
                nParts++;
                var memPtr = Marshal.AllocHGlobal(sizeof(int) * nParts);
                var partWidth = 100; // set parts width according to the form size
                for (var i = 0; i < nParts; i++) {
                    Marshal.WriteInt32(memPtr, i * sizeof(int), partWidth);
                    partWidth += partWidth;
                }
                Win32.SendMessage(statusBarHandle, SbGetparts, nParts, memPtr);
                Marshal.FreeHGlobal(memPtr);

                //// set text for the new part
                var text0 = Marshal.StringToHGlobalAuto(labelText);
                Win32.SendMessage(statusBarHandle, SbGetparts, nParts - 1, text0);
                Marshal.FreeHGlobal(text0);
            }
            return retval;
        }

    }
}