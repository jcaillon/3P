#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (DropToWindows.cs) is part of 3P.
// 
// 3P is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// 3P is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with 3P. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;

namespace _3PA.Lib {

    public static class DropToWindows {
        public static void DropToAppBuilder(string prowin32Path, string pfPath, string filename) {
            /*
                List<string> listOfFiles = new List<string>(new string[] {Npp.GetCurrentDocument(), Npp.GetCurrentDocument()});
                DropFileToWindow.Do("WinMergeU.exe", @"", Npp.GetCurrentDocument());
            */
            //string prowin32Path = "prowin32.exe";
            //string pfPath = @"sac1\sacdev.pf";
            //List<string> listOfFiles = new List<string>(new string[] { Npp.GetCurrentDocument(), Npp.GetCurrentDocument() });
            //DropFileToWindow.Do("WinMergeU.exe", @"", listOfFiles);

            DropFileToWindow.Do(prowin32Path, pfPath, filename);
        }
    }

    public static class DropFileToWindow {

        #region "give window focus"
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hwnd);
        #endregion

        public static void Do(string strToFindInCommandLine, string strToFindInCommandLine2, string filename, bool giveFocus = true) {
            Do(strToFindInCommandLine, strToFindInCommandLine2, new List<string>(new[] { filename }), giveFocus);
        }

        public static void Do(string strToFindInCommandLine, string strToFindInCommandLine2, IList<string> filename, bool giveFocus = true) {

            int procID = 0;

            string wmiQuery = "select ProcessId, CommandLine from Win32_Process";
            ManagementObjectSearcher search = new ManagementObjectSearcher(wmiQuery);
            ManagementObjectCollection processList = search.Get();

            foreach (ManagementObject process in processList) {
                if (process["CommandLine"] != null) {
                    var cmdLine = process["CommandLine"].ToString();
                    if (cmdLine.Contains(strToFindInCommandLine) && cmdLine.Contains(strToFindInCommandLine2)) {
                        procID = Convert.ToInt32(process["ProcessId"]);
                    }
                }
            }

            if (procID != 0) {
                IntPtr winHandle = FindWindow.GetProcessWindow(procID);
                MmdDrop.DropFile(winHandle, filename.Select(x => new MmdDropFile(x)).ToList());

                if (giveFocus)
                    SetForegroundWindow(winHandle);
            }
        }
    }

    public static class FindWindow {
        [DllImport("user32.dll", SetLastError = false)]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern IntPtr GetParent(IntPtr hWnd);

        public static IntPtr GetProcessWindow(int processId) {
            IntPtr prevWindow = IntPtr.Zero;
            while (true) {
                IntPtr desktopWindow = GetDesktopWindow();
                if (desktopWindow == IntPtr.Zero)
                    break;
                IntPtr nextWindow = FindWindowEx(desktopWindow, prevWindow, null, null);
                if (nextWindow == IntPtr.Zero)
                    break;
                uint procId = 0;
                GetWindowThreadProcessId(nextWindow, out procId);
                if (procId == processId) {
                    bool isWindowVisible = IsWindowVisible(nextWindow);
                    bool hasParent = GetParent(nextWindow) == IntPtr.Zero;
                    if (isWindowVisible && hasParent) {
                        return nextWindow;
                    }
                }
                prevWindow = nextWindow;
            }
            return IntPtr.Zero;
        }
    }

    public static class MmdDrop {
        [SuppressUnmanagedCodeSecurity, DllImport("user32")]
        static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        const uint WM_DROPFILES = 0x233;

        struct DropFiles {
            public uint pFiles;
            public int x;
            public int y;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fNC;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fWide;
        }

        public static void DropFile(IntPtr hWnd, MmdDropFile file) {
            DropFile(hWnd, new[] { file });
        }

        public static void DropFile(IntPtr hWnd, IList<MmdDropFile> files) {
            var names = Encoding.Unicode.GetBytes(string.Join("\0", files.Select(_ => _.FullName).ToArray()) + "\0\0");
            var pipes = files.Where(_ => _.IsPipe).Select(_ => new {
                Pipe = new NamedPipeServerStream(_.FileName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous),
                File = _
            }).ToArray();
            var dropFilesSize = Marshal.SizeOf(typeof(DropFiles));
            var hGlobal = Marshal.AllocHGlobal(dropFilesSize + names.Length);

            var dropFiles = new DropFiles {
                pFiles = (uint)dropFilesSize,
                x = 0,
                y = 0,
                fNC = false,
                fWide = true
            };

            Marshal.StructureToPtr(dropFiles, hGlobal, true);
            Marshal.Copy(names, 0, new IntPtr(hGlobal.ToInt64() + dropFiles.pFiles), names.Length);

            PostMessage(hWnd, WM_DROPFILES, hGlobal, IntPtr.Zero);

            // Marshal.FreeHGlobal(hGlobal);

            foreach (var i in pipes)
                using (var handle = new ManualResetEvent(false)) {
                    var success = false;

                    i.Pipe.BeginWaitForConnection(ar => {
                        try {
                            i.Pipe.EndWaitForConnection(ar);
                            success = true;

                            try {
                                i.File.Stream.CopyTo(i.Pipe, (int)i.File.Stream.Length);
                                i.Pipe.WaitForPipeDrain();
                            } catch (IOException) {
                            }

                            i.Pipe.Dispose();
                            i.File.Stream.Dispose();
                            handle.Set();
                        } catch (ObjectDisposedException) {
                        }
                    }, null);

                    if (i.File.Timeout != -1)
                        ThreadPool.QueueUserWorkItem(_ => {
                            Thread.Sleep(i.File.Timeout);

                            if (!success && !i.Pipe.IsConnected) {
                                i.Pipe.Dispose();
                                i.File.Stream.Dispose();
                                handle.Set();
                            }
                        });

                    handle.WaitOne();
                }
        }
    }

    public class MmdDropFile {
        public string FileName { get; set; }

        public string FullName {
            get {
                return IsPipe ? @"\\.\pipe\" + FileName : FileName;
            }
        }

        public Stream Stream { get; set; }

        public bool IsPipe {
            get {
                return Stream != null;
            }
        }

        public int Timeout {
            get;
            set;
        }

        public MmdDropFile(string fileName)
            : this(fileName, null) {
        }

        public MmdDropFile(string fileName, Stream stream) {
            Timeout = -1;
            FileName = stream == null ? fileName : Path.GetFileName(fileName);
            Stream = stream;
        }
    }
}
