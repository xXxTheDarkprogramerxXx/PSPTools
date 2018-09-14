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
namespace pspsharp.media.codec.aac
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacDecoder.MAX_CHANNELS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacDecoder.MAX_ELEM_ID;
	using BitReader = pspsharp.media.codec.util.BitReader;
	using FFT = pspsharp.media.codec.util.FFT;

	public class Context
	{
		public BitReader br;
		public int frameSize;
		public int channels;
		public int skipSamples;
		public int nbSamples;
		public int randomState;
		public int sampleRate;
		public int outputChannels;

		internal bool isSaved; ///< Set if elements have stored overlap from previous frame
		internal DynamicRangeControl cheDrc = new DynamicRangeControl();

		// Channel element related data
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal ChannelElement[][] che = new ChannelElement[4][MAX_ELEM_ID];
		internal ChannelElement[][] che = RectangularArrays.ReturnRectangularChannelElementArray(4, MAX_ELEM_ID);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal ChannelElement[][] tagCheMap = new ChannelElement[4][MAX_ELEM_ID];
		internal ChannelElement[][] tagCheMap = RectangularArrays.ReturnRectangularChannelElementArray(4, MAX_ELEM_ID);
		public int tagsMapped;

		public float[] bufMdct = new float[1024];

		internal FFT mdct;
		internal FFT mdctSmall;
		internal FFT mdctLd;
		internal FFT mdctLtp;

		// Members user for output
		internal SingleChannelElement[] outputElement = new SingleChannelElement[MAX_CHANNELS]; ///< Points to each SingleChannelElement

		public int dmonoMode;

		public float[] temp = new float[128];

		public OutputConfiguration[] oc = new OutputConfiguration[2];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public float[][] samples = new float[2][2048];
		public float[][] samples = RectangularArrays.ReturnRectangularFloatArray(2, 2048);

		public Context()
		{
			for (int i = 0; i < oc.Length; i++)
			{
				oc[i] = new OutputConfiguration();
			}
		}
	}

}