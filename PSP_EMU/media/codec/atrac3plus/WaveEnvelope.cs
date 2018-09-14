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
	public class WaveEnvelope
	{
		internal bool hasStartPoint; ///< indicates start point within the GHA window
		internal bool hasStopPoint; ///< indicates stop point within the GHA window
		internal int startPos; ///< start position expressed in n*4 samples
		internal int stopPos; ///< stop  position expressed in n*4 samples

		public virtual void clear()
		{
			hasStartPoint = false;
			hasStopPoint = false;
			startPos = 0;
			stopPos = 0;
		}

		public virtual void copy(WaveEnvelope from)
		{
			this.hasStartPoint = from.hasStartPoint;
			this.hasStopPoint = from.hasStopPoint;
			this.startPos = from.startPos;
			this.stopPos = from.stopPos;
		}
	}

}