#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (IManagedType.cs) is part of YamuiFramework.
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
    /// Interface for all types we can perform transitions on. 
    /// Each type (e.g. int, double, Color) that we can perform a transition on 
    /// needs to have its own class that implements this interface. These classes 
    /// tell the transition system how to act on objects of that type.
    /// </summary>
    internal interface IManagedType {
        /// <summary>
        /// Returns the Type that the instance is managing.
        /// </summary>
        Type getManagedType();

        /// <summary>
        /// Returns a deep copy of the object passed in. (In particular this is 
        /// needed for types that are objects.)
        /// </summary>
        object copy(object o);

        /// <summary>
        /// Returns an object holding the value between the start and end corresponding
        /// to the percentage passed in. (Note: the percentage can be less than 0% or
        /// greater than 100%.)
        /// </summary>
        object getIntermediateValue(object start, object end, double dPercentage);
    }
}