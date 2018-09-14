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
//	import static pspsharp.media.codec.mp3.Mp3Decoder.LAST_BUF_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.mp3.Mp3Decoder.MP3_MAX_CHANNELS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.mp3.Mp3Decoder.SBLIMIT;
	using BitReader = pspsharp.media.codec.util.BitReader;

	public class Context
	{
		public BitReader br;
		public Mp3Header header = new Mp3Header();
		public float[][] samples;
		public int[] lastBuf = new int[LAST_BUF_SIZE];
		public int lastBufSize;
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public float[][] synthBuf = new float[MP3_MAX_CHANNELS][512 * 2];
		public float[][] synthBuf = RectangularArrays.ReturnRectangularFloatArray(MP3_MAX_CHANNELS, 512 * 2);
		public int[] synthBufOffset = new int[MP3_MAX_CHANNELS];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public float[][] sbSamples = new float[MP3_MAX_CHANNELS][36 * SBLIMIT];
		public float[][] sbSamples = RectangularArrays.ReturnRectangularFloatArray(MP3_MAX_CHANNELS, 36 * SBLIMIT);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public float[][] mdctBuf = new float[MP3_MAX_CHANNELS][SBLIMIT * 18]; // previous samples, for layer 3 MDCT
		public float[][] mdctBuf = RectangularArrays.ReturnRectangularFloatArray(MP3_MAX_CHANNELS, SBLIMIT * 18); // previous samples, for layer 3 MDCT
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public Granule[][] granules = new Granule[2][2]; // Used in Layer 3
		public Granule[][] granules = RectangularArrays.ReturnRectangularGranuleArray(2, 2); // Used in Layer 3
		public int aduMode; ///<0 for standard mp3, 1 for adu formatted mp3
		public int[] ditherState = new int[1];
		public int errRecognition;
		public int outputChannels;

		public Context()
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					granules[i][j] = new Granule();
				}
			}
		}
	}

}