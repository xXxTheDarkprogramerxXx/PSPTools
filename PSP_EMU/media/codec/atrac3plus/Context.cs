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
//	import static pspsharp.media.codec.atrac3plus.Atrac3plusDecoder.ATRAC3P_FRAME_SAMPLES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.atrac3plus.Atrac3plusDecoder.ATRAC3P_SUBBAND_SAMPLES;
	using BitReader = pspsharp.media.codec.util.BitReader;
	using FFT = pspsharp.media.codec.util.FFT;

	public class Context
	{
		public BitReader br;
		public Atrac3plusDsp dsp;

		public ChannelUnit[] channelUnits = new ChannelUnit[16]; ///< global channel units
		public int numChannelBlocks = 2; ///< number of channel blocks
		public int outputChannels;

		public Atrac gaincCtx; ///< gain compensation context
		public FFT mdctCtx;
		public FFT ipqfDctCtx; ///< IDCT context used by IPQF

//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public float[][] samples = new float[2][ATRAC3P_FRAME_SAMPLES]; ///< quantized MDCT sprectrum
		public float[][] samples = RectangularArrays.ReturnRectangularFloatArray(2, ATRAC3P_FRAME_SAMPLES); ///< quantized MDCT sprectrum
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public float[][] mdctBuf = new float[2][ATRAC3P_FRAME_SAMPLES + ATRAC3P_SUBBAND_SAMPLES]; ///< output of the IMDCT
		public float[][] mdctBuf = RectangularArrays.ReturnRectangularFloatArray(2, ATRAC3P_FRAME_SAMPLES + ATRAC3P_SUBBAND_SAMPLES); ///< output of the IMDCT
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public float[][] timeBuf = new float[2][ATRAC3P_FRAME_SAMPLES]; ///< output of the gain compensation
		public float[][] timeBuf = RectangularArrays.ReturnRectangularFloatArray(2, ATRAC3P_FRAME_SAMPLES); ///< output of the gain compensation
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public float[][] outpBuf = new float[2][ATRAC3P_FRAME_SAMPLES];
		public float[][] outpBuf = RectangularArrays.ReturnRectangularFloatArray(2, ATRAC3P_FRAME_SAMPLES);
	}

}