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
namespace pspsharp.media.codec.mp3
{
	public class Mp3Header
	{
		public int frameSize;
		public int errorProtection;
		public int layer;
		public int sampleRate;
		public int sampleRateIndex; // between 0 and 8
		public int rawSampleRateIndex; // between 0 and 2
		public int bitRate;
		public int bitrateIndex; // between 0 and 14
		public int nbChannels;
		public int mode;
		public int modeExt;
		public int lsf;
		public int mpeg25;
		public int version;
		public int maxSamples;

		public override string ToString()
		{
			return string.Format("Mp3Header[version {0:D}, layer{1:D}, {2:D} Hz, {3:D} kbits/s, {4}]", version, layer, sampleRate, bitRate, nbChannels == 2 ? "stereo" : "mono");
		}
	}

}