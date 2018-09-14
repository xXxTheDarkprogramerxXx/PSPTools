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
	using Utilities = pspsharp.util.Utilities;

	public class SBRData
	{
		public const int SBR_SYNTHESIS_BUF_SIZE = (1280 - 128) * 2;

		// Main bitstream data variables
		internal int bsFrameClass;
		internal bool bsAddHarmonicFlag;
		internal int bsNumEnv;
		internal int[] bsFreqRes = new int[7];
		internal int bsNumNoise;
		internal int[] bsDfEnv = new int[5];
		internal int[] bsDfNoise = new int[2];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal int[][] bsInvfMode = new int[2][5];
		internal int[][] bsInvfMode = RectangularArrays.ReturnRectangularIntArray(2, 5);
		internal int[] bsAddHarmonic = new int[48];
		internal bool bsAmpRes;

		// State variables
		internal float[] synthesisFilterbankSamples = new float[SBR_SYNTHESIS_BUF_SIZE];
		internal float[] analysisFilterbankSamples = new float[1312];
		internal int synthesisFilterbankSamplesOffset;
		///l_APrev and l_A
		internal int[] eA = new int[2];
		///Chirp factors
		internal float[] bwArray = new float[5];
		///QMF values of the original signal
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][][][] W = new float[2][32][32][2];
		internal float[][][][] W = RectangularArrays.ReturnRectangularFloatArray(2, 32, 32, 2);
		///QMF output of the HF adjustor
		internal int Ypos;
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][][][] Y = new float[2][38][64][2];
		internal float[][][][] Y = RectangularArrays.ReturnRectangularFloatArray(2, 38, 64, 2);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][] gTemp = new float[42][48];
		internal float[][] gTemp = RectangularArrays.ReturnRectangularFloatArray(42, 48);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][] qTemp = new float[42][48];
		internal float[][] qTemp = RectangularArrays.ReturnRectangularFloatArray(42, 48);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal int[][] sIndexmapped = new int[8][48];
		internal int[][] sIndexmapped = RectangularArrays.ReturnRectangularIntArray(8, 48);
		///Envelope scalefactors
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][] envFacs = new float[6][48];
		internal float[][] envFacs = RectangularArrays.ReturnRectangularFloatArray(6, 48);
		///Noise scalefactors
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][] noiseFacs = new float[3][5];
		internal float[][] noiseFacs = RectangularArrays.ReturnRectangularFloatArray(3, 5);
		///Envelope time borders
		internal int[] tEnv = new int[8];
		///Envelope time border of the last envelope of the previous frame
		internal int tEnvNumEnvOld;
		///Noise time borders
		internal int[] tQ = new int[3];
		internal int fIndexnoise;
		internal int fIndexsine;

		public virtual void copy(SBRData that)
		{
			bsFrameClass = that.bsFrameClass;
			bsAddHarmonicFlag = that.bsAddHarmonicFlag;
			bsNumEnv = that.bsNumEnv;
			Utilities.copy(bsFreqRes, that.bsFreqRes);
			bsNumNoise = that.bsNumNoise;
			Utilities.copy(bsDfEnv, that.bsDfEnv);
			Utilities.copy(bsDfNoise, that.bsDfNoise);
			Utilities.copy(bsInvfMode, that.bsInvfMode);
			Utilities.copy(bsAddHarmonic, that.bsAddHarmonic);
			bsAmpRes = that.bsAmpRes;

			// State variables
			Utilities.copy(synthesisFilterbankSamples, that.synthesisFilterbankSamples);
			Utilities.copy(analysisFilterbankSamples, that.analysisFilterbankSamples);
			synthesisFilterbankSamplesOffset = that.synthesisFilterbankSamplesOffset;
			Utilities.copy(eA, that.eA);
			Utilities.copy(bwArray, that.bwArray);
			Utilities.copy(W, that.W);
			Utilities.copy(Y, that.Y);
			Utilities.copy(gTemp, that.gTemp);
			Utilities.copy(qTemp, that.qTemp);
			Utilities.copy(sIndexmapped, that.sIndexmapped);
			Utilities.copy(envFacs, that.envFacs);
			Utilities.copy(noiseFacs, that.noiseFacs);
			Utilities.copy(tEnv, that.tEnv);
			tEnvNumEnvOld = that.tEnvNumEnvOld;
			Utilities.copy(tQ, that.tQ);
			fIndexnoise = that.fIndexnoise;
			fIndexsine = that.fIndexsine;
		}
	}

}