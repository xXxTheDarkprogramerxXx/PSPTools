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
namespace pspsharp.media.codec.atrac3
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.atrac3.Atrac3Decoder.SAMPLES_PER_FRAME;
	using Atrac = pspsharp.media.codec.atrac3plus.Atrac;
	using BitReader = pspsharp.media.codec.util.BitReader;
	using FFT = pspsharp.media.codec.util.FFT;

	public class Context
	{
		public BitReader br;
		public int codingMode;
		public ChannelUnit[] units = new ChannelUnit[2];
		public int channels;
		public int outputChannels;
		public int blockAlign;
		// joint-stereo related variables
		internal int[] matrixCoeffIndexPrev = new int[4];
		internal int[] matrixCoeffIndexNow = new int[4];
		internal int[] matrixCoeffIndexNext = new int[4];
		internal int[] weightingDelay = new int[6];
		// data buffers
		public float[] tempBuf = new float[1070];

		public Atrac gaincCtx;
		public FFT mdctCtx;

//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public float[][] samples = new float[2][SAMPLES_PER_FRAME];
		public float[][] samples = RectangularArrays.ReturnRectangularFloatArray(2, SAMPLES_PER_FRAME);
	}

}