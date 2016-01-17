#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (DllExportAttribute.cs) is part of 3P.
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
using System.Runtime.InteropServices;

// ReSharper disable All

// BE CAREFUL, DONT CHANGE THE NAMESPACE HERE!!! It should be : NppPlugin.DllExport
namespace NppPlugin.DllExport {

    [AttributeUsage(AttributeTargets.Method)]
    class DllExportAttribute : Attribute {

        public DllExportAttribute() {
        }

        public DllExportAttribute(string exportName)
            : this(exportName, CallingConvention.StdCall) {
        }

        public DllExportAttribute(string exportName, CallingConvention callingConvention) {
            ExportName = exportName;
            CallingConvention = callingConvention;
        }

        CallingConvention _callingConvention;

        public CallingConvention CallingConvention {
            get { return _callingConvention; }
            set { _callingConvention = value; }
        }

        string _exportName;

        public string ExportName {
            get { return _exportName; }
            set { _exportName = value; }
        }

    }

}