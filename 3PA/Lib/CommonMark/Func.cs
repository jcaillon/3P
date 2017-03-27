#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (Func.cs) is part of 3P.
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

namespace _3PA.Lib.CommonMark {
#if v2_0
/// <summary>An alternative to <c>System.Func</c> which is not present in .NET 2.0.</summary>
    public delegate TResult Func<out TResult>();

    /// <summary>An alternative to <c>System.Func</c> which is not present in .NET 2.0.</summary>
    public delegate TResult Func<in T, out TResult>(T arg);

    /// <summary>An alternative to <c>System.Action</c> which is not present in .NET 2.0.</summary>
    public delegate void Action<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);
#endif
}