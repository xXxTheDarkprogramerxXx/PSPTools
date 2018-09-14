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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.mp3.Mp3Decoder.SBLIMIT;

	// layer 3 granule
	public class Granule
	{
		public int scfsi;
		public int part23Length;
		public int bigValues;
		public int globalGain;
		public int scalefacCompress;
		public int blockType;
		public int switchPoint;
		public int[] tableSelect = new int[3];
		public int[] subblockGain = new int[3];
		public int scalefacScale;
		public int count1tableSelect;
		public int[] regionSize = new int[3]; // number of huffman codes in each region
		internal int preflag;
		public int shortStart, longEnd; // long/short band indexes
		public int[] scaleFactors = new int[40];
		public float[] sbHybrid = new float[SBLIMIT * 18]; // 576 samples
		public int granuleStartPosition;
	}

}