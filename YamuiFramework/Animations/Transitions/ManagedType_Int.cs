#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (ManagedType_Int.cs) is part of YamuiFramework.
// 
// YamuiFramework is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// YamuiFramework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with YamuiFramework. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System;

namespace YamuiFramework.Animations.Transitions {
    /// <summary>
    /// Manages transitions for int properties.
    /// </summary>
    internal class ManagedType_Int : IManagedType {
        #region IManagedType Members

        /// <summary>
        /// Returns the type we are managing.
        /// </summary>
        public Type getManagedType() {
            return typeof(int);
        }

        /// <summary>
        /// Returns a copy of the int passed in.
        /// </summary>
        public object copy(object o) {
            int value = (int) o;
            return value;
        }

        /// <summary>
        /// Returns the value between the start and end for the percentage passed in.
        /// </summary>
        public object getIntermediateValue(object start, object end, double dPercentage) {
            int iStart = (int) start;
            int iEnd = (int) end;
            return Utility.interpolate(iStart, iEnd, dPercentage);
        }

        #endregion
    }
}