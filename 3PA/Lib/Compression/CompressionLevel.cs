#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (CompressionLevel.cs) is part of 3P.
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
namespace _3PA.Lib.Compression {
    /// <summary>
    /// Specifies the compression level ranging from minimum compresion to
    /// maximum compression, or no compression at all.
    /// </summary>
    /// <remarks>
    /// Although only four values are enumerated, any integral value between
    /// <see cref="CompressionLevel.Min"/> and <see cref="CompressionLevel.Max"/> can also be used.
    /// </remarks>
    public enum CompressionLevel {
        /// <summary>Do not compress files, only store.</summary>
        None = 0,

        /// <summary>Minimum compression; fastest.</summary>
        Min = 1,

        /// <summary>A compromize between speed and compression efficiency.</summary>
        Normal = 6,

        /// <summary>Maximum compression; slowest.</summary>
        Max = 10
    }
}