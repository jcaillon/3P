#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (FileTransfert.cs) is part of 3P.
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
using System.Collections.Generic;

namespace _3PA.Lib.Ftp {
    internal class FileTransfert {
        public int NbSimultaneousFiles { get; set; }

        public List<FileToTransfert> ListOfFileToTransfert { get; set; }

        public long TransfertProgression {
            get { return 0; }
        }

        public void StartTransfert() {}

        public void StopTransfert() {}
    }

    internal class FileToTransfert {
        public string Local { get; set; }
        public string Remote { get; set; }
        public TransfertStatus Done { get; set; }
    }

    internal enum TransfertStatus {
        None,
        Transfered,
        Failed
    }
}