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
//	import static pspsharp.media.codec.atrac3plus.Atrac3plusDecoder.ATRAC3P_PQF_FIR_LEN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.atrac3plus.Atrac3plusDecoder.ATRAC3P_SUBBANDS;

	/// <summary>
	/// Channel unit parameters </summary>
	public class ChannelUnitContext
	{
		// Channel unit variables
		public int unitType; ///< unit type (mono/stereo)
		public int numQuantUnits;
		public int numSubbands;
		public int usedQuantUnits; ///< number of quant units with coded spectrum
		public int numCodedSubbands; ///< number of subbands with coded spectrum
		public bool muteFlag; ///< mute flag
		public bool useFullTable; ///< 1 - full table list, 0 - restricted one
		public bool noisePresent; ///< 1 - global noise info present
		public int noiseLevelIndex; ///< global noise level index
		public int noiseTableIndex; ///< global noise RNG table index
		public bool[] swapChannels = new bool[ATRAC3P_SUBBANDS]; ///< 1 - perform subband-wise channel swapping
		public bool[] negateCoeffs = new bool[ATRAC3P_SUBBANDS]; ///< 1 - subband-wise IMDCT coefficients negation
		public Channel[] channels = new Channel[2];

		// Variables related to GHA tones
		public WaveSynthParams[] waveSynthHist = new WaveSynthParams[2]; ///< waves synth history for two frames
		public WaveSynthParams wavesInfo;
		public WaveSynthParams wavesInfoPrev;

		public IPQFChannelContext[] ipqfCtx = new IPQFChannelContext[2];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public float[][] prevBuf = new float[2][Atrac3plusDecoder.ATRAC3P_FRAME_SAMPLES]; ///< overlapping buffer
		public float[][] prevBuf = RectangularArrays.ReturnRectangularFloatArray(2, Atrac3plusDecoder.ATRAC3P_FRAME_SAMPLES); ///< overlapping buffer

		public class IPQFChannelContext
		{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public float[][] buf1 = new float[ATRAC3P_PQF_FIR_LEN * 2][8];
			public float[][] buf1 = RectangularArrays.ReturnRectangularFloatArray(ATRAC3P_PQF_FIR_LEN * 2, 8);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public float[][] buf2 = new float[ATRAC3P_PQF_FIR_LEN * 2][8];
			public float[][] buf2 = RectangularArrays.ReturnRectangularFloatArray(ATRAC3P_PQF_FIR_LEN * 2, 8);
			public int pos;
		}

		public ChannelUnitContext()
		{
			for (int ch = 0; ch < channels.Length; ch++)
			{
				channels[ch] = new Channel(ch);
			}

			for (int i = 0; i < waveSynthHist.Length; i++)
			{
				waveSynthHist[i] = new WaveSynthParams();
			}
			wavesInfo = waveSynthHist[0];
			wavesInfoPrev = waveSynthHist[1];

			for (int i = 0; i < ipqfCtx.Length; i++)
			{
				ipqfCtx[i] = new IPQFChannelContext();
			}
		}
	}

}