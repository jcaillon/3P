#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (TransitionType_ThrowAndCatch.cs) is part of YamuiFramework.
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
using System.Collections.Generic;

namespace YamuiFramework.Animations.Transitions
{
    /// <summary>
    /// This transition bounces the property to a destination value and back to the
    /// original value. It is decelerated to the destination and then acclerated back
    /// as if being thrown against gravity and then descending back with gravity.
    /// </summary>
    public class TransitionType_ThrowAndCatch : TransitionType_UserDefined
    {
        #region Public methods

        /// <summary>
        /// Constructor. You pass in the total time taken for the bounce.
        /// </summary>
        public TransitionType_ThrowAndCatch(int iTransitionTime)
        {
            // We create a custom "user-defined" transition to do the work...
            IList<TransitionElement> elements = new List<TransitionElement>();
            elements.Add(new TransitionElement(50, 100, InterpolationMethod.Deceleration));
            elements.Add(new TransitionElement(100, 0, InterpolationMethod.Accleration));
            setup(elements, iTransitionTime);
        }

        #endregion
    }
}
