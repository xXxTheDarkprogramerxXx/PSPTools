using System;

/*
This file is part of pspsharp.

pspsharp is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

pspsharp is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with pspsharp.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace pspsharp.media.codec.atrac3plus
{
	/// <summary>
	///  Gain control parameters for one subband.
	/// </summary>
	public class AtracGainInfo
	{
		public int numPoints; ///< number of gain control points
		public int[] levCode = new int[7]; ///< level at corresponding control point
		public int[] locCode = new int[7]; ///< location of gain control points

		public virtual void clear()
		{
			numPoints = 0;
			for (int i = 0; i < 7; i++)
			{
				levCode[i] = 0;
				locCode[i] = 0;
			}
		}

		public virtual void copy(AtracGainInfo from)
		{
			this.numPoints = from.numPoints;
			Array.Copy(from.levCode, 0, this.levCode, 0, levCode.Length);
			Array.Copy(from.locCode, 0, this.locCode, 0, locCode.Length);
		}
	}

}