// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace WixToolset.Dtf.Compression {

    public interface IArchiveInfo {

        /// <summary>
        /// Compresses files into the archive, specifying the names used to
        /// store the files in the archive.
        /// </summary>
        /// <param name="files">A mapping from internal file paths to
        /// external file paths.</param>
        /// <param name="compLevel">The compression level used when creating
        /// the archive.</param>
        /// <param name="progressHandler">Handler for receiving progress information;
        /// this may be null if progress is not desired.</param>
        void PackFileSet(IDictionary<string, string> files, CompressionLevel compLevel, EventHandler<ArchiveProgressEventArgs> progressHandler);
        
    }
}