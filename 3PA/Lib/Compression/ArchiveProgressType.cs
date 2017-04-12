#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (ArchiveProgressType.cs) is part of 3P.
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
namespace WixToolset.Dtf.Compression {
    /// <summary>
    /// The type of progress event.
    /// </summary>
    /// <remarks>
    /// <p>PACKING EXAMPLE: The following sequence of events might be received when
    /// extracting a simple archive file with 2 files.</p>
    /// <list type="table">
    /// <listheader><term>Message Type</term><description>Description</description></listheader>
    /// <item><term>StartArchive</term> <description>Begin extracting archive</description></item>
    /// <item><term>StartFile</term>    <description>Begin extracting first file</description></item>
    /// <item><term>PartialFile</term>  <description>Extracting first file</description></item>
    /// <item><term>PartialFile</term>  <description>Extracting first file</description></item>
    /// <item><term>FinishFile</term>   <description>Finished extracting first file</description></item>
    /// <item><term>StartFile</term>    <description>Begin extracting second file</description></item>
    /// <item><term>PartialFile</term>  <description>Extracting second file</description></item>
    /// <item><term>FinishFile</term>   <description>Finished extracting second file</description></item>
    /// <item><term>FinishArchive</term><description>Finished extracting archive</description></item>
    /// </list>
    /// <p></p>
    /// <p>UNPACKING EXAMPLE:  Packing 3 files into 2 archive chunks, where the second file is
    ///	continued to the second archive chunk.</p>
    /// <list type="table">
    /// <listheader><term>Message Type</term><description>Description</description></listheader>
    /// <item><term>StartFile</term>     <description>Begin compressing first file</description></item>
    /// <item><term>FinishFile</term>    <description>Finished compressing first file</description></item>
    /// <item><term>StartFile</term>     <description>Begin compressing second file</description></item>
    /// <item><term>PartialFile</term>   <description>Compressing second file</description></item>
    /// <item><term>PartialFile</term>   <description>Compressing second file</description></item>
    /// <item><term>FinishFile</term>    <description>Finished compressing second file</description></item>
    /// <item><term>StartArchive</term>  <description>Begin writing first archive</description></item>
    /// <item><term>PartialArchive</term><description>Writing first archive</description></item>
    /// <item><term>FinishArchive</term> <description>Finished writing first archive</description></item>
    /// <item><term>StartFile</term>     <description>Begin compressing third file</description></item>
    /// <item><term>PartialFile</term>   <description>Compressing third file</description></item>
    /// <item><term>FinishFile</term>    <description>Finished compressing third file</description></item>
    /// <item><term>StartArchive</term>  <description>Begin writing second archive</description></item>
    /// <item><term>PartialArchive</term><description>Writing second archive</description></item>
    /// <item><term>FinishArchive</term> <description>Finished writing second archive</description></item>
    /// </list>
    /// </remarks>
    public enum ArchiveProgressType {
        /// <summary>Status message before beginning the packing or unpacking an individual file.</summary>
        StartFile,

        /// <summary>Status message (possibly reported multiple times) during the process of packing or unpacking a file.</summary>
        PartialFile,

        /// <summary>Status message after completion of the packing or unpacking an individual file.</summary>
        FinishFile,

        /// <summary>Status message before beginning the packing or unpacking an archive.</summary>
        StartArchive,

        /// <summary>Status message (possibly reported multiple times) during the process of packing or unpacking an archiv.</summary>
        PartialArchive,

        /// <summary>Status message after completion of the packing or unpacking of an archive.</summary>
        FinishArchive
    }
}