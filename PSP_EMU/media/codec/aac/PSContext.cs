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

	public class PSContext
	{
		public const int PS_MAX_NUM_ENV = 5;
		public const int PS_MAX_NR_IIDICC = 34;
		public const int PS_MAX_NR_IPDOPD = 17;
		public const int PS_MAX_SSB = 91;
		public const int PS_MAX_AP_BANDS = 50;
		public const int PS_QMF_TIME_SLOTS = 32;
		public const int PS_MAX_DELAY = 14;
		public const int PS_AP_LINKS = 3;
		public const int PS_MAX_AP_DELAY = 5;

		public bool start;
		internal bool enableIid;
		internal int iidQuant;
		internal int nrIidPar;
		internal int nrIpdopdPar;
		internal bool enableIcc;
		internal int iccMode;
		internal int nrIccPar;
		internal bool enableExt;
		internal int frameClass;
		internal int numEnvOld;
		internal int numEnv;
		internal bool enableIpdopd;
		internal int[] borderPosition = new int[PS_MAX_NUM_ENV + 1];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal int[][] iidPar = new int[PS_MAX_NUM_ENV][PS_MAX_NR_IIDICC]; ///< Inter-channel Intensity Difference Parameters
		internal int[][] iidPar = RectangularArrays.ReturnRectangularIntArray(PS_MAX_NUM_ENV, PS_MAX_NR_IIDICC); ///< Inter-channel Intensity Difference Parameters
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal int[][] iccPar = new int[PS_MAX_NUM_ENV][PS_MAX_NR_IIDICC]; ///< Inter-Channel Coherence Parameters
		internal int[][] iccPar = RectangularArrays.ReturnRectangularIntArray(PS_MAX_NUM_ENV, PS_MAX_NR_IIDICC); ///< Inter-Channel Coherence Parameters
		// ipd/opd is iid/icc sized so that the same functions can handle both
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal int[][] ipdPar = new int[PS_MAX_NUM_ENV][PS_MAX_NR_IIDICC]; ///< Inter-channel Phase Difference Parameters
		internal int[][] ipdPar = RectangularArrays.ReturnRectangularIntArray(PS_MAX_NUM_ENV, PS_MAX_NR_IIDICC); ///< Inter-channel Phase Difference Parameters
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal int[][] opdPar = new int[PS_MAX_NUM_ENV][PS_MAX_NR_IIDICC]; ///< Overall Phase Difference Parameters
		internal int[][] opdPar = RectangularArrays.ReturnRectangularIntArray(PS_MAX_NUM_ENV, PS_MAX_NR_IIDICC); ///< Overall Phase Difference Parameters
		internal bool is34bands;
		internal bool is34bandsOld;

//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][][] inBuf = new float[5][44][2];
		internal float[][][] inBuf = RectangularArrays.ReturnRectangularFloatArray(5, 44, 2);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][][] delay = new float[PS_MAX_SSB][PS_QMF_TIME_SLOTS + PS_MAX_DELAY][2];
		internal float[][][] delay = RectangularArrays.ReturnRectangularFloatArray(PS_MAX_SSB, PS_QMF_TIME_SLOTS + PS_MAX_DELAY, 2);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][][][] apDelay = new float[PS_MAX_AP_BANDS][PS_AP_LINKS][PS_QMF_TIME_SLOTS + PS_MAX_AP_DELAY][2];
		internal float[][][][] apDelay = RectangularArrays.ReturnRectangularFloatArray(PS_MAX_AP_BANDS, PS_AP_LINKS, PS_QMF_TIME_SLOTS + PS_MAX_AP_DELAY, 2);
		internal float[] peakDecayNrg = new float[34];
		internal float[] powerSmooth = new float[34];
		internal float[] peakDecayDiffSmooth = new float[34];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][][] H11 = new float [2][PS_MAX_NUM_ENV+1][PS_MAX_NR_IIDICC];
		internal float[][][] H11 = RectangularArrays.ReturnRectangularFloatArray(2, PS_MAX_NUM_ENV + 1, PS_MAX_NR_IIDICC);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][][] H12 = new float [2][PS_MAX_NUM_ENV+1][PS_MAX_NR_IIDICC];
		internal float[][][] H12 = RectangularArrays.ReturnRectangularFloatArray(2, PS_MAX_NUM_ENV + 1, PS_MAX_NR_IIDICC);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][][] H21 = new float [2][PS_MAX_NUM_ENV+1][PS_MAX_NR_IIDICC];
		internal float[][][] H21 = RectangularArrays.ReturnRectangularFloatArray(2, PS_MAX_NUM_ENV + 1, PS_MAX_NR_IIDICC);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][][] H22 = new float [2][PS_MAX_NUM_ENV+1][PS_MAX_NR_IIDICC];
		internal float[][][] H22 = RectangularArrays.ReturnRectangularFloatArray(2, PS_MAX_NUM_ENV + 1, PS_MAX_NR_IIDICC);
		internal int[] opdHist = new int[PS_MAX_NR_IIDICC];
		internal int[] ipdHist = new int[PS_MAX_NR_IIDICC];

		public virtual void copy(PSContext that)
		{
			start = that.start;
			enableIid = that.enableIid;
			iidQuant = that.iidQuant;
			nrIidPar = that.nrIidPar;
			nrIpdopdPar = that.nrIpdopdPar;
			enableIcc = that.enableIcc;
			iccMode = that.iccMode;
			nrIccPar = that.nrIccPar;
			enableExt = that.enableExt;
			frameClass = that.frameClass;
			numEnvOld = that.numEnvOld;
			numEnv = that.numEnv;
			enableIpdopd = that.enableIpdopd;
			Utilities.copy(borderPosition, that.borderPosition);
			Utilities.copy(iidPar, that.iidPar);
			Utilities.copy(iccPar, that.iccPar);
			Utilities.copy(ipdPar, that.ipdPar);
			Utilities.copy(opdPar, that.opdPar);
			is34bands = that.is34bands;
			is34bandsOld = that.is34bandsOld;

			Utilities.copy(inBuf, that.inBuf);
			Utilities.copy(delay, that.delay);
			Utilities.copy(apDelay, that.apDelay);
			Utilities.copy(peakDecayNrg, that.peakDecayNrg);
			Utilities.copy(powerSmooth, that.powerSmooth);
			Utilities.copy(peakDecayDiffSmooth, that.peakDecayDiffSmooth);
			Utilities.copy(H11, that.H11);
			Utilities.copy(H12, that.H12);
			Utilities.copy(H21, that.H21);
			Utilities.copy(H22, that.H22);
			Utilities.copy(opdHist, that.opdHist);
			Utilities.copy(ipdHist, that.ipdHist);
		}
	}

}