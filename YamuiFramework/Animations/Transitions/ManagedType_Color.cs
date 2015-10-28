#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (ManagedType_Color.cs) is part of YamuiFramework.
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
using System.Drawing;

namespace YamuiFramework.Animations.Transitions
{
	/// <summary>
	/// Class that manages transitions for Color properties. For these we
	/// need to transition the R, G, B and A sub-properties independently.
	/// </summary>
	internal class ManagedType_Color : IManagedType
	{
		#region IManagedType Members

		/// <summary>
		/// Returns the type we are managing.
		/// </summary>
		public Type getManagedType()
		{
			return typeof(Color);
		}

		/// <summary>
		/// Returns a copy of the color object passed in.
		/// </summary>
		public object copy(object o)
		{
			Color c = (Color)o;
			Color result = Color.FromArgb(c.ToArgb());
			return result;
		}

		/// <summary>
		/// Creates an intermediate value for the colors depending on the percentage passed in.
		/// </summary>
		public object getIntermediateValue(object start, object end, double dPercentage)
		{
			Color startColor = (Color)start;
			Color endColor = (Color)end;

			// We interpolate the R, G, B and A components separately...
			int iStart_R = startColor.R;
			int iStart_G = startColor.G;
			int iStart_B = startColor.B;
			int iStart_A = startColor.A;

			int iEnd_R = endColor.R;
			int iEnd_G = endColor.G;
			int iEnd_B = endColor.B;
			int iEnd_A = endColor.A;

			int new_R = Utility.interpolate(iStart_R, iEnd_R, dPercentage);
			int new_G = Utility.interpolate(iStart_G, iEnd_G, dPercentage);
			int new_B = Utility.interpolate(iStart_B, iEnd_B, dPercentage);
			int new_A = Utility.interpolate(iStart_A, iEnd_A, dPercentage);

			return Color.FromArgb(new_A, new_R, new_G, new_B);
		}

		#endregion
	}
}
