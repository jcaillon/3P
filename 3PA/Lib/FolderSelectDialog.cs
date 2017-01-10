#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (FolderSelectDialog.cs) is part of 3P.
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
using System.Reflection;
using System.Windows.Forms;
using _3PA.Interop;

namespace _3PA.Lib {
    /// <summary>
    /// Wraps System.Windows.Forms.OpenFileDialog to make it present
    /// a vista-style dialog.
    /// </summary>
    public class FolderSelectDialog {
        // Wrapped dialog
        OpenFileDialog _ofd;

        /// <summary>
        /// Default constructor
        /// </summary>
        public FolderSelectDialog() {
            _ofd = new OpenFileDialog {
                Filter = "Folders|\n",
                AddExtension = false,
                CheckFileExists = false,
                DereferenceLinks = true,
                Multiselect = false
            };
        }

        #region Properties

        /// <summary>
        /// Gets/Sets the initial folder to be selected. A null value selects the current directory.
        /// </summary>
        public string InitialDirectory {
            get { return _ofd.InitialDirectory; }
            set { _ofd.InitialDirectory = string.IsNullOrEmpty(value) ? Environment.CurrentDirectory : value; }
        }

        /// <summary>
        /// Gets/Sets the title to show in the dialog
        /// </summary>
        public string Title {
            get { return _ofd.Title; }
            set { _ofd.Title = value ?? "Select a folder"; }
        }

        /// <summary>
        /// Gets the selected folder
        /// </summary>
        public string FileName {
            get { return _ofd.FileName; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the dialog
        /// </summary>
        /// <returns>True if the user presses OK else false</returns>
        public bool ShowDialog() {
            return ShowDialog(IntPtr.Zero);
        }

        /// <summary>
        /// Shows the dialog
        /// </summary>
        /// <param name="hWndOwner">Handle of the control to be parent</param>
        /// <returns>True if the user presses OK else false</returns>
        public bool ShowDialog(IntPtr hWndOwner) {
            bool flag;

            if (Environment.OSVersion.Version.Major >= 6) {
                var r = new Reflector("System.Windows.Forms");

                uint num = 0;
                Type typeIFileDialog = r.GetType("FileDialogNative.IFileDialog");
                object dialog = r.Call(_ofd, "CreateVistaDialog");
                r.Call(_ofd, "OnBeforeVistaDialog", dialog);

                uint options = (uint)r.CallAs(typeof(FileDialog), _ofd, "GetOptions");
                options |= (uint)r.GetEnum("FileDialogNative.FOS", "FOS_PICKFOLDERS");
                r.CallAs(typeIFileDialog, dialog, "SetOptions", options);

                object pfde = r.New("FileDialog.VistaDialogEvents", _ofd);
                object[] parameters = { pfde, num };
                r.CallAs2(typeIFileDialog, dialog, "Advise", parameters);
                num = (uint)parameters[1];
                try {
                    int num2 = (int)r.CallAs(typeIFileDialog, dialog, "Show", hWndOwner);
                    flag = 0 == num2;
                } finally {
                    r.CallAs(typeIFileDialog, dialog, "Unadvise", num);
                    GC.KeepAlive(pfde);
                }

            } else {
                using (var fbd = new FolderBrowserDialog {
                        Description = Title,
                        SelectedPath = InitialDirectory,
                        ShowNewFolderButton = false
                    }) {
                    if (fbd.ShowDialog(new WindowWrapper(hWndOwner)) != DialogResult.OK) return false;
                    _ofd.FileName = fbd.SelectedPath;
                }
                flag = true;
            }

            return flag;
        }

        #endregion
    }

    /// <summary>
    /// This class is from the Front-End for Dosbox and is used to present a 'vista' dialog box to select folders.
    /// Being able to use a vista style dialog box to select folders is much better then using the shell folder browser.
    /// http://code.google.com/p/fed/
    ///
    /// Example:
    /// var r = new Reflector("System.Windows.Forms");
    /// </summary>
    public class Reflector {
        #region variables

        string _mNs;
        Assembly _mAsmb;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ns">The namespace containing types to be used</param>
        public Reflector(string ns)
            : this(ns, ns) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="an">A specific assembly name (used if the assembly name does not tie exactly with the namespace)</param>
        /// <param name="ns">The namespace containing types to be used</param>
        public Reflector(string an, string ns) {
            _mNs = ns;
            _mAsmb = null;
            foreach (AssemblyName aN in Assembly.GetExecutingAssembly().GetReferencedAssemblies()) {
                if (aN.FullName.StartsWith(an)) {
                    _mAsmb = Assembly.Load(aN);
                    break;
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Return a Type instance for a type 'typeName'
        /// </summary>
        /// <param name="typeName">The name of the type</param>
        /// <returns>A type instance</returns>
        public Type GetType(string typeName) {
            Type type = null;
            string[] names = typeName.Split('.');

            if (names.Length > 0)
                type = _mAsmb.GetType(_mNs + "." + names[0]);

            for (int i = 1; i < names.Length; ++i) {
                if (type != null)
                    type = type.GetNestedType(names[i], BindingFlags.NonPublic);
            }
            return type;
        }

        /// <summary>
        /// Create a new object of a named type passing along any params
        /// </summary>
        /// <param name="name">The name of the type to create</param>
        /// <param name="parameters"></param>
        /// <returns>An instantiated type</returns>
        public object New(string name, params object[] parameters) {
            Type type = GetType(name);

            ConstructorInfo[] ctorInfos = type.GetConstructors();
            foreach (ConstructorInfo ci in ctorInfos) {
                try {
                    return ci.Invoke(parameters);
                } catch {
                    // ignored
                }
            }

            return null;
        }

        /// <summary>
        /// Calls method 'func' on object 'obj' passing parameters 'parameters'
        /// </summary>
        /// <param name="obj">The object on which to excute function 'func'</param>
        /// <param name="func">The function to execute</param>
        /// <param name="parameters">The parameters to pass to function 'func'</param>
        /// <returns>The result of the function invocation</returns>
        public object Call(object obj, string func, params object[] parameters) {
            return Call2(obj, func, parameters);
        }

        /// <summary>
        /// Calls method 'func' on object 'obj' passing parameters 'parameters'
        /// </summary>
        /// <param name="obj">The object on which to excute function 'func'</param>
        /// <param name="func">The function to execute</param>
        /// <param name="parameters">The parameters to pass to function 'func'</param>
        /// <returns>The result of the function invocation</returns>
        public object Call2(object obj, string func, object[] parameters) {
            return CallAs2(obj.GetType(), obj, func, parameters);
        }

        /// <summary>
        /// Calls method 'func' on object 'obj' which is of type 'type' passing parameters 'parameters'
        /// </summary>
        /// <param name="type">The type of 'obj'</param>
        /// <param name="obj">The object on which to excute function 'func'</param>
        /// <param name="func">The function to execute</param>
        /// <param name="parameters">The parameters to pass to function 'func'</param>
        /// <returns>The result of the function invocation</returns>
        public object CallAs(Type type, object obj, string func, params object[] parameters) {
            return CallAs2(type, obj, func, parameters);
        }

        /// <summary>
        /// Calls method 'func' on object 'obj' which is of type 'type' passing parameters 'parameters'
        /// </summary>
        /// <param name="type">The type of 'obj'</param>
        /// <param name="obj">The object on which to excute function 'func'</param>
        /// <param name="func">The function to execute</param>
        /// <param name="parameters">The parameters to pass to function 'func'</param>
        /// <returns>The result of the function invocation</returns>
        public object CallAs2(Type type, object obj, string func, object[] parameters) {
            MethodInfo methInfo = type.GetMethod(func, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return methInfo.Invoke(obj, parameters);
        }

        /// <summary>
        /// Returns the value of property 'prop' of object 'obj'
        /// </summary>
        /// <param name="obj">The object containing 'prop'</param>
        /// <param name="prop">The property name</param>
        /// <returns>The property value</returns>
        public object Get(object obj, string prop) {
            return GetAs(obj.GetType(), obj, prop);
        }

        /// <summary>
        /// Returns the value of property 'prop' of object 'obj' which has type 'type'
        /// </summary>
        /// <param name="type">The type of 'obj'</param>
        /// <param name="obj">The object containing 'prop'</param>
        /// <param name="prop">The property name</param>
        /// <returns>The property value</returns>
        public object GetAs(Type type, object obj, string prop) {
            PropertyInfo propInfo = type.GetProperty(prop, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return propInfo.GetValue(obj, null);
        }

        /// <summary>
        /// Returns an enum value
        /// </summary>
        /// <param name="typeName">The name of enum type</param>
        /// <param name="name">The name of the value</param>
        /// <returns>The enum value</returns>
        public object GetEnum(string typeName, string name) {
            Type type = GetType(typeName);
            FieldInfo fieldInfo = type.GetField(name);
            return fieldInfo.GetValue(null);
        }

        #endregion

    }

}
