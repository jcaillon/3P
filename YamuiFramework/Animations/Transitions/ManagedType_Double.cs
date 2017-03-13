#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (ManagedType_Double.cs) is part of YamuiFramework.
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
    /// Manages transitions for double properties.
    /// </summary>
    internal class ManagedType_Double : IManagedType {
        #region IManagedType Members

        /// <summary>
        ///  Returns the type managed by this class.
        /// </summary>
        public Type getManagedType() {
            return typeof(double);
        }

        /// <summary>
        /// Returns a copy of the double passed in.
        /// </summary>
        public object copy(object o) {
            double d = (double) o;
            return d;
        }

        /// <summary>
        /// Returns the value between start and end for the percentage passed in.
        /// </summary>
        public object getIntermediateValue(object start, object end, double dPercentage) {
            double dStart = (double) start;
            double dEnd = (double) end;
            return Utility.interpolate(dStart, dEnd, dPercentage);
        }

        #endregion
    }
}