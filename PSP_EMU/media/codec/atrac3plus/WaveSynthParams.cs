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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.atrac3plus.Atrac3plusDecoder.ATRAC3P_SUBBANDS;

	public class WaveSynthParams
	{
		internal bool tonesPresent; ///< 1 - tones info present
		internal int amplitudeMode; ///< 1 - low range, 0 - high range
		internal int numToneBands; ///< number of PQF bands with tones
		internal bool[] toneSharing = new bool[ATRAC3P_SUBBANDS]; ///< 1 - subband-wise tone sharing flags
		internal bool[] toneMaster = new bool[ATRAC3P_SUBBANDS]; ///< 1 - subband-wise tone channel swapping
		internal bool[] phaseShift = new bool[ATRAC3P_SUBBANDS]; ///< 1 - subband-wise 180 degrees phase shifting
		internal int tonesIndex; ///< total sum of tones in this unit
		internal WaveParam[] waves = new WaveParam[48];

		public WaveSynthParams()
		{
			for (int i = 0; i < waves.Length; i++)
			{
				waves[i] = new WaveParam();
			}
		}
	}

}