#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (TransitionType_CriticalDamping.cs) is part of YamuiFramework.
// 
// // YamuiFramework is a free software: you can redistribute it and/or modify
// // it under the terms of the GNU General Public License as published by
// // the Free Software Foundation, either version 3 of the License, or
// // (at your option) any later version.
// 
// // YamuiFramework is distributed in the hope that it will be useful,
// // but WITHOUT ANY WARRANTY; without even the implied warranty of
// // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// // GNU General Public License for more details.
// 
// // You should have received a copy of the GNU General Public License
// // along with YamuiFramework. If not, see <http://www.gnu.org/licenses/>.
// // ========================================================================
#endregion
using System;

namespace YamuiFramework.Animations.Transitions
{
    /// <summary>
    /// This transition animates with an exponential decay. This has a damping effect
    /// similar to the motion of a needle on an electomagnetically controlled dial.
    /// </summary>
	public class TransitionType_CriticalDamping : ITransitionType
	{
		#region Public methods

		/// <summary>
		/// Constructor. You pass in the time that the transition 
		/// will take (in milliseconds).
		/// </summary>
		public TransitionType_CriticalDamping(int iTransitionTime)
		{
			if (iTransitionTime <= 0)
			{
				throw new Exception("Transition time must be greater than zero.");
			}
			m_dTransitionTime = iTransitionTime;
		}

		#endregion

		#region ITransitionMethod Members

		/// <summary>
		/// </summary>
		public void onTimer(int iTime, out double dPercentage, out bool bCompleted)
		{
			// We find the percentage time elapsed...
			double dElapsed = iTime / m_dTransitionTime;
			dPercentage = (1.0 - Math.Exp(-1.0 * dElapsed * 5)) / 0.993262053;

			if (dElapsed >= 1.0)
			{
				dPercentage = 1.0;
				bCompleted = true;
			}
			else
			{
				bCompleted = false;
			}
		}

		#endregion

		#region Private data

		private double m_dTransitionTime;

		#endregion
	}
}
