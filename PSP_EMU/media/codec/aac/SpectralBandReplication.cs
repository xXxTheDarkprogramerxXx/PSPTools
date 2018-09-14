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
	using FFT = pspsharp.media.codec.util.FFT;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// Spectral Band Replication
	/// </summary>
	public class SpectralBandReplication
	{
		internal int sampleRate;
		internal bool start;
		internal bool reset;
		internal SpectrumParameters spectrumParams = new SpectrumParameters();
		internal bool bsAmpResHeader;
		internal int bsLimiterBands;
		internal int bsLimiterGains;
		internal bool bsInterpolFreq;
		internal bool bsSmoothingMode;
		internal bool bsCoupling;
		internal int[] k = new int[5]; ///< k0, k1, k2
		///kx', and kx respectively, kx is the first QMF subband where SBR is used.
		///kx' is its value from the previous frame
		internal int[] kx = new int[2];
		///M' and M respectively, M is the number of QMF subbands that use SBR.
		internal int[] m = new int[2];
		internal bool kxAndMPushed;
		///The number of frequency bands in f_master
		internal int nMaster;
		internal SBRData[] data = new SBRData[2];
		internal PSContext ps = new PSContext();
		///N_Low and N_High respectively, the number of frequency bands for low and high resolution
		internal int[] n = new int[2];
		///Number of noise floor bands
		internal int nQ;
		///Number of limiter bands
		internal int nLim;
		///The master QMF frequency grouping
		internal int[] fMaster = new int[49];
		///Frequency borders for low resolution SBR
		internal int[] fTablelow = new int[25];
		///Frequency borders for high resolution SBR
		internal int[] fTablehigh = new int[49];
		///Frequency borders for noise floors
		internal int[] fTablenoise = new int[6];
		///Frequency borders for the limiter
		internal int[] fTablelim = new int[30];
		internal int numPatches;
		internal int[] patchNumSubbands = new int[6];
		internal int[] patchStartSubband = new int[6];
		///QMF low frequency input to the HF generator
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][][] Xlow = new float[32][40][2];
		internal float[][][] Xlow = RectangularArrays.ReturnRectangularFloatArray(32, 40, 2);
		///QMF output of the HF generator
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][][] Xhigh = new float[64][40][2];
		internal float[][][] Xhigh = RectangularArrays.ReturnRectangularFloatArray(64, 40, 2);
		///QMF values of the reconstructed signal
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][][][] X = new float[2][2][38][64];
		internal float[][][][] X = RectangularArrays.ReturnRectangularFloatArray(2, 2, 38, 64);
		///Zeroth coefficient used to filter the subband signals
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][] alpha0 = new float[64][2];
		internal float[][] alpha0 = RectangularArrays.ReturnRectangularFloatArray(64, 2);
		///First coefficient used to filter the subband signals
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][] alpha1 = new float[64][2];
		internal float[][] alpha1 = RectangularArrays.ReturnRectangularFloatArray(64, 2);
		///Dequantized envelope scalefactors, remapped
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][] eOrigmapped = new float[7][48];
		internal float[][] eOrigmapped = RectangularArrays.ReturnRectangularFloatArray(7, 48);
		///Dequantized noise scalefactors, remapped
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][] qMapped = new float[7][48];
		internal float[][] qMapped = RectangularArrays.ReturnRectangularFloatArray(7, 48);
		///Sinusoidal presence, remapped
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal int[][] sMapped = new int[7][48];
		internal int[][] sMapped = RectangularArrays.ReturnRectangularIntArray(7, 48);
		///Estimated envelope
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][] eCurr = new float[7][48];
		internal float[][] eCurr = RectangularArrays.ReturnRectangularFloatArray(7, 48);
		///Amplitude adjusted noise scalefactors
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][] qM = new float[7][48];
		internal float[][] qM = RectangularArrays.ReturnRectangularFloatArray(7, 48);
		///Sinusoidal levels
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][] sM = new float[7][48];
		internal float[][] sM = RectangularArrays.ReturnRectangularFloatArray(7, 48);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal float[][] gain = new float [7][48];
		internal float[][] gain = RectangularArrays.ReturnRectangularFloatArray(7, 48);
		internal float[] qmfFilterScratch = new float[5 * 64]; // originally: float[5][64]
		internal FFT mdctAna = new FFT();
		internal FFT mdct = new FFT();

		public SpectralBandReplication()
		{
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = new SBRData();
			}
		}

		public virtual void copy(SpectralBandReplication that)
		{
			sampleRate = that.sampleRate;
			start = that.start;
			reset = that.reset;
			spectrumParams.copy(that.spectrumParams);
			bsAmpResHeader = that.bsAmpResHeader;
			bsLimiterBands = that.bsLimiterBands;
			bsLimiterGains = that.bsLimiterGains;
			bsInterpolFreq = that.bsInterpolFreq;
			bsSmoothingMode = that.bsSmoothingMode;
			bsCoupling = that.bsCoupling;
			Utilities.copy(k, that.k);
			Utilities.copy(kx, that.kx);
			Utilities.copy(m, that.m);
			kxAndMPushed = that.kxAndMPushed;
			nMaster = that.nMaster;
			for (int i = 0; i < data.Length; i++)
			{
				data[i].copy(that.data[i]);
			}
			ps.copy(that.ps);
			Utilities.copy(n, that.n);
			nQ = that.nQ;
			nLim = that.nLim;
			Utilities.copy(fMaster, that.fMaster);
			Utilities.copy(fTablelow, that.fTablelow);
			Utilities.copy(fTablehigh, that.fTablehigh);
			Utilities.copy(fTablenoise, that.fTablenoise);
			Utilities.copy(fTablelim, that.fTablelim);
			numPatches = that.numPatches;
			Utilities.copy(patchNumSubbands, that.patchNumSubbands);
			Utilities.copy(patchStartSubband, that.patchStartSubband);
			Utilities.copy(Xlow, that.Xlow);
			Utilities.copy(Xhigh, that.Xhigh);
			Utilities.copy(X, that.X);
			Utilities.copy(alpha0, that.alpha0);
			Utilities.copy(alpha1, that.alpha1);
			Utilities.copy(eOrigmapped, that.eOrigmapped);
			Utilities.copy(qMapped, that.qMapped);
			Utilities.copy(sMapped, that.sMapped);
			Utilities.copy(eCurr, that.eCurr);
			Utilities.copy(qM, that.qM);
			Utilities.copy(sM, that.sM);
			Utilities.copy(gain, that.gain);
			Utilities.copy(qmfFilterScratch, that.qmfFilterScratch);
			mdctAna.copy(that.mdctAna);
			mdct.copy(that.mdct);
		}
	}

}