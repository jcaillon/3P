#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (ITransitionType.cs) is part of YamuiFramework.
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
namespace YamuiFramework.Animations.Transitions
{
    public interface ITransitionType
    {
		/// <summary>
		/// Called by the Transition framework when its timer ticks to pass in the
		/// time (in ms) since the transition started. 
		/// 
		/// You should return (in an out parameter) the percentage movement towards 
		/// the destination value for the time passed in. Note: this does not need to
		/// be a smooth transition from 0% to 100%. You can overshoot with values
		/// greater than 100% or undershoot if you need to (for example, to have some
		/// form of "elasticity").
		/// 
		/// The percentage should be returned as (for example) 0.1 for 10%.
		/// 
		/// You should return (in an out parameter) whether the transition has completed.
		/// (This may not be at the same time as the percentage has moved to 100%.)
		/// </summary>
		void onTimer(int iTime, out double dPercentage, out bool bCompleted);
    }
}
