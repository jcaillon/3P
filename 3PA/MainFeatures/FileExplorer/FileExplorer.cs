#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (FileExplorer.cs) is part of 3P.
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
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using _3PA.Images;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;

namespace _3PA.MainFeatures.FileExplorer {
    public class FileExplorer {

        #region fields

        /// <summary>
        /// Index command that activates this dockable window
        /// </summary>
        public static int DockableCommandIndex;

        /// <summary>
        /// Form accessor
        /// </summary>
        public static FileExplorerForm ExplorerForm { get; private set; }

        /// <summary>
        /// Does the form exists and is visible?
        /// </summary>
        public static bool IsVisible {
            get { return ExplorerForm != null && ExplorerForm.Visible; }
        }

        #endregion

        #region handling form

        /// <summary>
        /// Toggle the docked form on and off, can be called first and will initialize the form
        /// </summary>
        public static void Toggle() {
            try {
                // initialize if not done
                if (ExplorerForm == null) {
                    Init();
                } else {
                    Win32.SendMessage(Npp.HandleNpp, !ExplorerForm.Visible ? NppMsg.NPPM_DMMSHOW : NppMsg.NPPM_DMMHIDE, 0, ExplorerForm.Handle);
                }
                if (ExplorerForm == null) return;
                //ExplorerForm.FileExplorerPage.UseAlternativeBackColor = Config.Instance.CodeExplorerUseAlternateColors;
                UpdateMenuItemChecked();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in Dockable explorer");
            }
        }

        /// <summary>
        /// Use this to redraw the docked form
        /// </summary>
        public static void Redraw() {
            if (IsVisible) {
                //ExplorerForm.FileExplorerPage.StyleOvlTree();
                ExplorerForm.Invalidate();
                ExplorerForm.Refresh();
            }
        }

        /// <summary>
        /// Either check or uncheck the menu, depending on the visibility of the form
        /// (does it both on the menu and toolbar)
        /// </summary>
        public static void UpdateMenuItemChecked() {
            if (ExplorerForm == null) return;
            Win32.SendMessage(Npp.HandleNpp, NppMsg.NPPM_SETMENUITEMCHECK, Plug.FuncItems.Items[DockableCommandIndex]._cmdID, ExplorerForm.Visible ? 1 : 0);
            Config.Instance.CodeExplorerVisible = ExplorerForm.Visible;
        }

        /// <summary>
        /// Initialize the form
        /// </summary>
        public static void Init() {

            ExplorerForm = new FileExplorerForm();

            NppTbData nppTbData = new NppTbData {
                hClient = ExplorerForm.Handle,
                pszName = AssemblyInfo.ProductTitle + " - File explorer",
                dlgID = DockableCommandIndex,
                uMask = NppTbMsg.DWS_DF_CONT_LEFT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR,
                hIconTab = (uint) Utils.GetIconFromImage(ImageResources.code_explorer_logo).Handle,
                pszModuleName = AssemblyInfo.ProductTitle
            };

            IntPtr ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(nppTbData));
            Marshal.StructureToPtr(nppTbData, ptrNppTbData, false);

            Win32.SendMessage(Npp.HandleNpp, NppMsg.NPPM_DMMREGASDCKDLG, 0, ptrNppTbData);
        }

        #endregion

        public static void Refresh() { }


        public static List<FileObject> ListFileOjectsInDirectory(string dirPath, bool recursive = true) {
            var output = new List<FileObject>();
            if (!Directory.Exists(dirPath))
                return output;

            // get dir info
            var dirInfo = new DirectoryInfo(dirPath);

            // for each file in the dir
            foreach (var fileInfo in dirInfo.GetFiles()) {
                FileType fileType;
                if(!Enum.TryParse(fileInfo.Extension.Replace(".",""), true, out fileType))
                    fileType = FileType.Unknow;
                output.Add(new FileObject {
                    FileName = fileInfo.Name,
                    BasePath = fileInfo.DirectoryName,
                    Flags = FileFlag.ReadOnly,
                    Size = fileInfo.Length,
                    CreateDateTime = fileInfo.CreationTime,
                    ModifieDateTime = fileInfo.LastWriteTime,
                    Type = fileType
                });
            }

            // for each folder in dir
            foreach (var directoryInfo in dirInfo.GetDirectories()) {
                // recursive
                output.AddRange(ListFileOjectsInDirectory(directoryInfo.FullName));
                output.Add(new FileObject {
                    FileName = directoryInfo.Name,
                    BasePath = Path.GetPathRoot(directoryInfo.FullName),
                    CreateDateTime = directoryInfo.CreationTime,
                    ModifieDateTime = directoryInfo.LastWriteTime,
                    Type = FileType.Folder
                });
            }

            return output;
        } 
    }

    #region FileObject

    /// <summary>
    /// Object describing a file
    /// </summary>
    public class FileObject {
        public string FileName { get; set; }
        public string BasePath { get; set; }
        public DateTime ModifieDateTime { get; set; }
        public DateTime CreateDateTime { get; set; }
        public long Size { get; set; }
        public FileType Type { get; set; }
        public FileFlag Flags { get; set; }
        public string SubString { get; set; }
    }

    /// <summary>
    /// Type of an objetc file (depends on the file's extension)
    /// </summary>
    public enum FileType {
        Unknow,
        Df,
        E,
        Folder,
        I,
        Lst,
        P,
        R,
        T,
        W
    }

    /// <summary>
    /// File's flags
    /// </summary>
    [Flags]
    public enum FileFlag {
        /// <summary>
        /// Is the file starred by the user
        /// </summary>
        Favourite = 1,
        ReadOnly = 2,
    }

    #endregion

}
