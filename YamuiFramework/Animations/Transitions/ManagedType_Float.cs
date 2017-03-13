#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (ManagedType_Float.cs) is part of YamuiFramework.
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
    internal class ManagedType_Float : IManagedType {
        #region IManagedType Members

        /// <summary>
        /// Returns the type we're managing.
        /// </summary>
        public Type getManagedType() {
            return typeof(float);
        }

        /// <summary>
        /// Returns a copy of the float passed in.
        /// </summary>
        public object copy(object o) {
            float f = (float) o;
            return f;
        }

        /// <summary>
        /// Returns the interpolated value for the percentage passed in.
        /// </summary>
        public object getIntermediateValue(object start, object end, double dPercentage) {
            float fStart = (float) start;
            float fEnd = (float) end;
            return Utility.interpolate(fStart, fEnd, dPercentage);
        }

        #endregion
    }
}