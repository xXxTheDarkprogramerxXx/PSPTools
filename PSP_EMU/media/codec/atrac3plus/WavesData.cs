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
	/// Parameters of a group of sine waves </summary>
	public class WavesData
	{
		internal WaveEnvelope pendEnv; ///< pending envelope from the previous frame
		internal WaveEnvelope currEnv; ///< group envelope from the current frame
		internal int numWavs; ///< number of sine waves in the group
		internal int startIndex; ///< start index into global tones table for that subband

		public WavesData()
		{
			pendEnv = new WaveEnvelope();
			currEnv = new WaveEnvelope();
		}

		public virtual void clear()
		{
			pendEnv.clear();
			currEnv.clear();
			numWavs = 0;
			startIndex = 0;
		}

		public virtual void copy(WavesData from)
		{
			this.pendEnv.copy(from.pendEnv);
			this.currEnv.copy(from.currEnv);
			this.numWavs = from.numWavs;
			this.startIndex = from.startIndex;
		}
	}

}