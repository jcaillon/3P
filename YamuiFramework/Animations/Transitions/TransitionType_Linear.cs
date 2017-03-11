#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (TransitionType_Linear.cs) is part of YamuiFramework.
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
    /// This class manages a linear transition. The percentage complete for the transition
    /// increases linearly with time.
    /// </summary>
    public class TransitionType_Linear : ITransitionType {
        #region Public methods

        /// <summary>
        /// Constructor. You pass in the time (in milliseconds) that the
        /// transition will take.
        /// </summary>
        public TransitionType_Linear(int iTransitionTime) {
            if (iTransitionTime <= 0) {
                throw new Exception("Transition time must be greater than zero.");
            }
            m_dTransitionTime = iTransitionTime;
        }

        #endregion

        #region ITransitionMethod Members

        /// <summary>
        /// We return the percentage completed.
        /// </summary>
        public void onTimer(int iTime, out double dPercentage, out bool bCompleted) {
            dPercentage = (iTime/m_dTransitionTime);
            if (dPercentage >= 1.0) {
                dPercentage = 1.0;
                bCompleted = true;
            } else {
                bCompleted = false;
            }
        }

        #endregion

        #region Private data

        private double m_dTransitionTime;

        #endregion
    }
}