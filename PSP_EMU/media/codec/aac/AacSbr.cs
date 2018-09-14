using System;

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
//	import static Math.pow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacDecoder.AAC_ERROR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacDecoder.TYPE_CCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacDecoder.TYPE_CPE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacDecoder.TYPE_SCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacSbrData.f_huffman_env_1_5dB_bits;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacSbrData.f_huffman_env_1_5dB_codes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacSbrData.f_huffman_env_3_0dB_bits;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacSbrData.f_huffman_env_3_0dB_codes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacSbrData.f_huffman_env_bal_1_5dB_bits;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacSbrData.f_huffman_env_bal_1_5dB_codes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacSbrData.f_huffman_env_bal_3_0dB_bits;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacSbrData.f_huffman_env_bal_3_0dB_codes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacSbrData.sbr_offset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacSbrData.sbr_qmf_window_ds;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacSbrData.sbr_qmf_window_us;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacSbrData.t_huffman_env_1_5dB_bits;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacSbrData.t_huffman_env_1_5dB_codes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacSbrData.t_huffman_env_3_0dB_bits;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacSbrData.t_huffman_env_3_0dB_codes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacSbrData.t_huffman_env_bal_1_5dB_bits;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacSbrData.t_huffman_env_bal_1_5dB_codes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacSbrData.t_huffman_env_bal_3_0dB_bits;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacSbrData.t_huffman_env_bal_3_0dB_codes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacSbrData.t_huffman_noise_3_0dB_bits;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacSbrData.t_huffman_noise_3_0dB_codes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacSbrData.t_huffman_noise_bal_3_0dB_bits;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacSbrData.t_huffman_noise_bal_3_0dB_codes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.SBRData.SBR_SYNTHESIS_BUF_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.util.CodecUtils.FLT_EPSILON;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.util.CodecUtils.exp2f;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.util.CodecUtils.log2f;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.util.CodecUtils.lrintf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.util.CodecUtils.sqrtf;

	using BitReader = pspsharp.media.codec.util.BitReader;
	using FFT = pspsharp.media.codec.util.FFT;
	using FloatDSP = pspsharp.media.codec.util.FloatDSP;
	using IBitReader = pspsharp.media.codec.util.IBitReader;
	using VLC = pspsharp.media.codec.util.VLC;
	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;

	public class AacSbr
	{
		private static Logger log = AacDecoder.log;
		private const int ENVELOPE_ADJUSTMENT_OFFSET = 2;
		private const float NOISE_FLOOR_OFFSET = 6.0f;
		private const int EXTENSION_ID_PS = 2;
		private const int T_HUFFMAN_ENV_1_5DB = 0;
		private const int F_HUFFMAN_ENV_1_5DB = 1;
		private const int T_HUFFMAN_ENV_BAL_1_5DB = 2;
		private const int F_HUFFMAN_ENV_BAL_1_5DB = 3;
		private const int T_HUFFMAN_ENV_3_0DB = 4;
		private const int F_HUFFMAN_ENV_3_0DB = 5;
		private const int T_HUFFMAN_ENV_BAL_3_0DB = 6;
		private const int F_HUFFMAN_ENV_BAL_3_0DB = 7;
		private const int T_HUFFMAN_NOISE_3_0DB = 8;
		private const int T_HUFFMAN_NOISE_BAL_3_0DB = 9;
		private static readonly VLC[] vlc_sbr = new VLC[10];
		private static readonly int[] vlc_sbr_lav = new int[] {60, 60, 24, 24, 31, 31, 12, 12, 31, 12};
		/// <summary>
		/// ceil(log2(index+1)) </summary>
		private static readonly int[] ceil_log2 = new int[] {0, 1, 2, 2, 3, 3};
		/// <summary>
		/// bs_frame_class - frame class of current SBR frame (14496-3 sp04 p98)
		/// </summary>
		private const int FIXFIX = 0;
		private const int FIXVAR = 1;
		private const int VARFIX = 2;
		private const int VARVAR = 3;

		public static void sbrInit()
		{
			for (int i = 0; i < vlc_sbr.Length; i++)
			{
				vlc_sbr[i] = new VLC();
			}
			vlc_sbr[T_HUFFMAN_ENV_1_5DB].initVLCSparse(9, t_huffman_env_1_5dB_codes.length, t_huffman_env_1_5dB_bits, t_huffman_env_1_5dB_codes, null);
			vlc_sbr[F_HUFFMAN_ENV_1_5DB].initVLCSparse(9, f_huffman_env_1_5dB_codes.length, f_huffman_env_1_5dB_bits, f_huffman_env_1_5dB_codes, null);
			vlc_sbr[T_HUFFMAN_ENV_BAL_1_5DB].initVLCSparse(9, t_huffman_env_bal_1_5dB_codes.length, t_huffman_env_bal_1_5dB_bits, t_huffman_env_bal_1_5dB_codes, null);
			vlc_sbr[F_HUFFMAN_ENV_BAL_1_5DB].initVLCSparse(9, f_huffman_env_bal_1_5dB_codes.length, f_huffman_env_bal_1_5dB_bits, f_huffman_env_bal_1_5dB_codes, null);
			vlc_sbr[T_HUFFMAN_ENV_3_0DB].initVLCSparse(9, t_huffman_env_3_0dB_codes.length, t_huffman_env_3_0dB_bits, t_huffman_env_3_0dB_codes, null);
			vlc_sbr[F_HUFFMAN_ENV_3_0DB].initVLCSparse(9, f_huffman_env_3_0dB_codes.length, f_huffman_env_3_0dB_bits, f_huffman_env_3_0dB_codes, null);
			vlc_sbr[T_HUFFMAN_ENV_BAL_3_0DB].initVLCSparse(9, t_huffman_env_bal_3_0dB_codes.length, t_huffman_env_bal_3_0dB_bits, t_huffman_env_bal_3_0dB_codes, null);
			vlc_sbr[F_HUFFMAN_ENV_BAL_3_0DB].initVLCSparse(9, f_huffman_env_bal_3_0dB_codes.length, f_huffman_env_bal_3_0dB_bits, f_huffman_env_bal_3_0dB_codes, null);
			vlc_sbr[T_HUFFMAN_NOISE_3_0DB].initVLCSparse(9, t_huffman_noise_3_0dB_codes.length, t_huffman_noise_3_0dB_bits, t_huffman_noise_3_0dB_codes, null);
			vlc_sbr[T_HUFFMAN_NOISE_BAL_3_0DB].initVLCSparse(9, t_huffman_noise_bal_3_0dB_codes.length, t_huffman_noise_bal_3_0dB_bits, t_huffman_noise_bal_3_0dB_codes, null);
		}

		private static void getBits1Vector(IBitReader br, int[] vec, int vecOffset, int elements)
		{
			for (int i = 0; i < elements; i++)
			{
				vec[vecOffset + i] = br.read1();
			}
		}

		private static int readSbrGrid(Context ac, SpectralBandReplication sbr, SBRData chData)
		{
			IBitReader br = ac.br;
			int bsPointer = 0;
			// frameLengthFlag ? 15 : 16; 960 sample length frames unsupported; this value is numTimeSlots
			int absBordTrail = 16;
			int numRelLead, numRelTrail;
			int bsNumEnvOld = chData.bsNumEnv;

			chData.bsFreqRes[0] = chData.bsFreqRes[chData.bsNumEnv];
			chData.bsAmpRes = sbr.bsAmpResHeader;
			chData.tEnvNumEnvOld = chData.tEnv[bsNumEnvOld];

			switch (chData.bsFrameClass = br.read(2))
			{
				case FIXFIX:
					chData.bsNumEnv = 1 << br.read(2);
					numRelLead = chData.bsNumEnv - 1;
					if (chData.bsNumEnv == 1)
					{
						chData.bsAmpRes = false;
					}

					if (chData.bsNumEnv > 4)
					{
						log.error(string.Format("Invalid bitstream, too many SBR envelopes in FIXFIX type SBR frame: {0:D}", chData.bsNumEnv));
						return -1;
					}

					chData.tEnv[0] = 0;
					chData.tEnv[chData.bsNumEnv] = absBordTrail;

					absBordTrail = (absBordTrail + (chData.bsNumEnv >> 1)) / chData.bsNumEnv;
					for (int i = 0; i < numRelLead; i++)
					{
						chData.tEnv[i + 1] = chData.tEnv[i] + absBordTrail;
					}

					chData.bsFreqRes[1] = br.read1();
					for (int i = 1; i < chData.bsNumEnv; i++)
					{
						chData.bsFreqRes[i + 1] = chData.bsFreqRes[1];
					}
					break;
				case FIXVAR:
					absBordTrail += br.read(2);
					numRelTrail = br.read(2);
					chData.bsNumEnv = numRelTrail + 1;
					chData.tEnv[0] = 0;
					chData.tEnv[chData.bsNumEnv] = absBordTrail;

					for (int i = 0; i < numRelTrail; i++)
					{
						chData.tEnv[chData.bsNumEnv - 1 - i] = chData.tEnv[chData.bsNumEnv - i] - 2 * br.read(2) - 2;
					}

					bsPointer = br.read(ceil_log2[chData.bsNumEnv]);

					for (int i = 0; i < chData.bsNumEnv; i++)
					{
						chData.bsFreqRes[chData.bsNumEnv - i] = br.read1();
					}
					break;
				case VARFIX:
					chData.tEnv[0] = br.read(2);
					numRelLead = br.read(2);
					chData.bsNumEnv = numRelLead + 1;
					chData.tEnv[chData.bsNumEnv] = absBordTrail;

					for (int i = 0; i < numRelLead; i++)
					{
						chData.tEnv[i + 1] = chData.tEnv[i] + 2 * br.read(2) + 2;
					}

					bsPointer = br.read(ceil_log2[chData.bsNumEnv]);

					getBits1Vector(br, chData.bsFreqRes, 1, chData.bsNumEnv);
					break;
				case VARVAR:
					chData.tEnv[0] = br.read(2);
					absBordTrail += br.read(2);
					numRelLead = br.read(2);
					numRelTrail = br.read(2);
					chData.bsNumEnv = numRelLead + numRelTrail + 1;

					if (chData.bsNumEnv > 5)
					{
						log.error(string.Format("Invalid bitstream, too many SBR envelopes in VARVAR type SBR frame: {0:D}", chData.bsNumEnv));
						return -1;
					}

					chData.tEnv[chData.bsNumEnv] = absBordTrail;

					for (int i = 0; i < numRelLead; i++)
					{
						chData.tEnv[i + 1] = chData.tEnv[i] + 2 * br.read(2) + 2;
					}
					for (int i = 0; i < numRelTrail; i++)
					{
						chData.tEnv[chData.bsNumEnv - 1 - i] = chData.tEnv[chData.bsNumEnv - i] - 2 * br.read(2) - 2;
					}

					bsPointer = br.read(ceil_log2[chData.bsNumEnv]);

					getBits1Vector(br, chData.bsFreqRes, 1, chData.bsNumEnv);
					break;
			}

			if (bsPointer > chData.bsNumEnv + 1)
			{
				log.error(string.Format("Invalid bitstream, bs_pointer points to a middle noise border outside the time borders table: {0:D}", bsPointer));
				return -1;
			}

			for (int i = 1; i <= chData.bsNumEnv; i++)
			{
				if (chData.tEnv[i - 1] > chData.tEnv[i])
				{
					log.error(string.Format("Non monotone time borders"));
					return -1;
				}
			}

			chData.bsNumNoise = (chData.bsNumEnv > 1 ? 1 : 0) + 1;

			chData.tQ[0] = chData.tEnv[0];
			chData.tQ[chData.bsNumNoise] = chData.tEnv[chData.bsNumEnv];
			if (chData.bsNumNoise > 1)
			{
				int idx;
				if (chData.bsFrameClass == FIXFIX)
				{
					idx = chData.bsNumEnv >> 1;
				}
				else if ((chData.bsFrameClass & 1) != 0)
				{ // FIXVAR or VARVAR
					idx = chData.bsNumEnv - System.Math.Max(bsPointer - 1, 1);
				}
				else
				{ // VARFIX
					if (bsPointer == 0)
					{
						idx = 1;
					}
					else if (bsPointer == 1)
					{
						idx = chData.bsNumEnv - 1;
					}
					else
					{ // bs_pointer > 1
						idx = bsPointer - 1;
					}
				}
				chData.tQ[1] = chData.tEnv[idx];
			}

			chData.eA[0] = -(chData.eA[1] != bsNumEnvOld ? 1 : 0); // l_APrev
			chData.eA[1] = -1;
			if ((chData.bsFrameClass & 1) != 0 && bsPointer != 0)
			{ // FIXVAR or VARVAR and bs_pointer != 0
				chData.eA[1] = chData.bsNumEnv + 1 - bsPointer;
			}
			else if ((chData.bsFrameClass == 2) && (bsPointer > 1))
			{ // VARFIX and bs_pointer > 1
				chData.eA[1] = bsPointer - 1;
			}

			return 0;
		}

		/// Read how the envelope and noise floor data is delta coded
		private static void readSbrDtdf(SpectralBandReplication sbr, IBitReader br, SBRData chData)
		{
			getBits1Vector(br, chData.bsDfEnv, 0, chData.bsNumEnv);
			getBits1Vector(br, chData.bsDfNoise, 0, chData.bsNumNoise);
		}

		/// Read inverse filtering data
		private static void readSbrInvf(SpectralBandReplication sbr, IBitReader br, SBRData chData)
		{
			Array.Copy(chData.bsInvfMode[0], 0, chData.bsInvfMode[1], 0, 5);
			for (int i = 0; i < sbr.nQ; i++)
			{
				chData.bsInvfMode[0][i] = br.read(2);
			}
		}

		private static void readSbrEnvelope(SpectralBandReplication sbr, IBitReader br, SBRData chData, bool ch)
		{
			int bits;
			VLC tHuff, fHuff;
			int tLav, fLav;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int delta = (ch && sbr.bsCoupling ? 1 : 0) + 1;
			int delta = (ch && sbr.bsCoupling ? 1 : 0) + 1;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int odd = sbr.n[1] & 1;
			int odd = sbr.n[1] & 1;

			if (sbr.bsCoupling && ch)
			{
				if (chData.bsAmpRes)
				{
					bits = 5;
					tHuff = vlc_sbr[T_HUFFMAN_ENV_BAL_3_0DB];
					tLav = vlc_sbr_lav[T_HUFFMAN_ENV_BAL_3_0DB];
					fHuff = vlc_sbr[F_HUFFMAN_ENV_BAL_3_0DB];
					fLav = vlc_sbr_lav[F_HUFFMAN_ENV_BAL_3_0DB];
				}
				else
				{
					bits = 6;
					tHuff = vlc_sbr[T_HUFFMAN_ENV_BAL_1_5DB];
					tLav = vlc_sbr_lav[T_HUFFMAN_ENV_BAL_1_5DB];
					fHuff = vlc_sbr[F_HUFFMAN_ENV_BAL_1_5DB];
					fLav = vlc_sbr_lav[F_HUFFMAN_ENV_BAL_1_5DB];
				}
			}
			else
			{
				if (chData.bsAmpRes)
				{
					bits = 6;
					tHuff = vlc_sbr[T_HUFFMAN_ENV_3_0DB];
					tLav = vlc_sbr_lav[T_HUFFMAN_ENV_3_0DB];
					fHuff = vlc_sbr[F_HUFFMAN_ENV_3_0DB];
					fLav = vlc_sbr_lav[F_HUFFMAN_ENV_3_0DB];
				}
				else
				{
					bits = 7;
					tHuff = vlc_sbr[T_HUFFMAN_ENV_1_5DB];
					tLav = vlc_sbr_lav[T_HUFFMAN_ENV_1_5DB];
					fHuff = vlc_sbr[F_HUFFMAN_ENV_1_5DB];
					fLav = vlc_sbr_lav[F_HUFFMAN_ENV_1_5DB];
				}
			}

			for (int i = 0; i < chData.bsNumEnv; i++)
			{
				if (chData.bsDfEnv[i] != 0)
				{
					// bsFreqRes[0] == bsFreqRes[bsNumEnv] from prev frame
					if (chData.bsFreqRes[i + 1] == chData.bsFreqRes[i])
					{
						for (int j = 0; j < sbr.n[chData.bsFreqRes[i + 1]]; j++)
						{
							chData.envFacs[i + 1][j] = chData.envFacs[i][j] + delta * (tHuff.getVLC2(br, 3) - tLav);
						}
					}
					else if (chData.bsFreqRes[i + 1] != 0)
					{
						for (int j = 0; j < sbr.n[chData.bsFreqRes[i + 1]]; j++)
						{
							int k = (j + odd) >> 1; // find k such that f_tablelow[k] <= fTablehigh[j] < f_tablelow[k + 1]
							chData.envFacs[i + 1][j] = chData.envFacs[i][k] + delta * (tHuff.getVLC2(br, 3) - tLav);
						}
					}
					else
					{
						for (int j = 0; j < sbr.n[chData.bsFreqRes[i + 1]]; j++)
						{
							int k = j != 0 ? 2 * j - odd : 0; // find k such that fTablehigh[k] == f_tablelow[j]
							chData.envFacs[i + 1][j] = chData.envFacs[i][k] + delta * (tHuff.getVLC2(br, 3) - tLav);
						}
					}
				}
				else
				{
					chData.envFacs[i + 1][0] = delta * br.read(bits); // bs_env_start_value_balance
					for (int j = 1; j < sbr.n[chData.bsFreqRes[i + 1]]; j++)
					{
						chData.envFacs[i + 1][j] = chData.envFacs[i + 1][j - 1] + delta * (fHuff.getVLC2(br, 3) - fLav);
					}
				}
			}

			//assign 0th elements of envFacs from last elements
			Array.Copy(chData.envFacs[chData.bsNumEnv], 0, chData.envFacs[0], 0, chData.envFacs[0].Length);
		}

		private static void readSbrNoise(SpectralBandReplication sbr, IBitReader br, SBRData chData, bool ch)
		{
			int i, j;
			VLC t_huff, f_huff;
			int t_lav, f_lav;
			int delta = (ch && sbr.bsCoupling ? 1 : 0) + 1;

			if (sbr.bsCoupling)
			{
				t_huff = vlc_sbr[T_HUFFMAN_NOISE_BAL_3_0DB];
				t_lav = vlc_sbr_lav[T_HUFFMAN_NOISE_BAL_3_0DB];
				f_huff = vlc_sbr[F_HUFFMAN_ENV_BAL_3_0DB];
				f_lav = vlc_sbr_lav[F_HUFFMAN_ENV_BAL_3_0DB];
			}
			else
			{
				t_huff = vlc_sbr[T_HUFFMAN_NOISE_3_0DB];
				t_lav = vlc_sbr_lav[T_HUFFMAN_NOISE_3_0DB];
				f_huff = vlc_sbr[F_HUFFMAN_ENV_3_0DB];
				f_lav = vlc_sbr_lav[F_HUFFMAN_ENV_3_0DB];
			}

			for (i = 0; i < chData.bsNumNoise; i++)
			{
				if (chData.bsDfNoise[i] != 0)
				{
					for (j = 0; j < sbr.nQ; j++)
					{
						chData.noiseFacs[i + 1][j] = chData.noiseFacs[i][j] + delta * (t_huff.getVLC2(br, 2) - t_lav);
					}
				}
				else
				{
					chData.noiseFacs[i + 1][0] = delta * br.read(5); // bs_noise_start_value_balance or bs_noise_start_value_level
					for (j = 1; j < sbr.nQ; j++)
					{
						chData.noiseFacs[i + 1][j] = chData.noiseFacs[i + 1][j - 1] + delta * (f_huff.getVLC2(br, 3) - f_lav);
					}
				}
			}

			//assign 0th elements of noiseFacs from last elements
			Array.Copy(chData.noiseFacs[chData.bsNumNoise], 0, chData.noiseFacs[0], 0, chData.noiseFacs[0].Length);
		}

		private static int readSbrSingleChannelElement(Context ac, SpectralBandReplication sbr)
		{
			if (ac.br.readBool())
			{ // bs_data_extra
				ac.br.skip(4); // bs_reserved
			}

			if (readSbrGrid(ac, sbr, sbr.data[0]) != 0)
			{
				return -1;
			}

			readSbrDtdf(sbr, ac.br, sbr.data[0]);
			readSbrInvf(sbr, ac.br, sbr.data[0]);
			readSbrEnvelope(sbr, ac.br, sbr.data[0], false);
			readSbrNoise(sbr, ac.br, sbr.data[0], false);

			sbr.data[0].bsAddHarmonicFlag = ac.br.readBool();
			if (sbr.data[0].bsAddHarmonicFlag)
			{
				getBits1Vector(ac.br, sbr.data[0].bsAddHarmonic, 0, sbr.n[1]);
			}

			return 0;
		}

		private static int readSbrExtension(Context ac, SpectralBandReplication sbr, int bsExtensionId, int numBitsLeft)
		{
			switch (bsExtensionId)
			{
				case EXTENSION_ID_PS:
					if (ac.oc[1].m4ac.ps == 0)
					{
						log.error(string.Format("Parametric Stereo signaled to be not-present but was found in the bitstream"));
						ac.br.skip(numBitsLeft);
						numBitsLeft = 0;
					}
					else
					{
						numBitsLeft -= AacPs.readData(ac, sbr.ps, numBitsLeft);
					}
					break;
				default:
					// some files contain 0-padding
					if (bsExtensionId != 0 || numBitsLeft > 16 || ac.br.peek(numBitsLeft) != 0)
					{
						log.error(string.Format("Reserved SBR extensions"));
					}
					ac.br.skip(numBitsLeft);
					numBitsLeft = 0;
					break;
			}

			return numBitsLeft;
		}

		private static int readSbrData(Context ac, SpectralBandReplication sbr, int idAac)
		{
			int cnt = ac.br.BitsRead;

			if (idAac == TYPE_SCE || idAac == TYPE_CCE)
			{
				if (readSbrSingleChannelElement(ac, sbr) != 0)
				{
					sbrTurnoff(sbr);
					return ac.br.BitsRead - cnt;
				}
			}
			else
			{
				log.error(string.Format("Invalid bitstream - cannot apply SBR to element type {0:D}", idAac));
				sbrTurnoff(sbr);
				return ac.br.BitsRead - cnt;
			}

			if (ac.br.readBool())
			{ // bs_extended_data
				int numBitsLeft = ac.br.read(4); // bs_extension_size
				if (numBitsLeft == 15)
				{
					numBitsLeft += ac.br.read(8); // bs_esc_count
				}

				numBitsLeft <<= 3;
				while (numBitsLeft > 7)
				{
					numBitsLeft -= 2;
					numBitsLeft = readSbrExtension(ac, sbr, ac.br.read(2), numBitsLeft);
				}

				if (numBitsLeft < 0)
				{
					log.error(string.Format("SBD Extension over read"));
				}
				else if (numBitsLeft > 0)
				{
					ac.br.skip(numBitsLeft);
				}
			}

			return ac.br.BitsRead - cnt;
		}

		private static void makeBands(int[] bands, int bandsOffset, int start, int stop, int numBands)
		{
			float @base = (float) pow(stop / (double) start, 1.0 / numBands);
			float prod = start;
			int previous = start;

			for (int k = 0; k < numBands - 1; k++)
			{
				prod *= @base;
				int present = (int) Math.rint(prod);
				bands[bandsOffset + k] = present - previous;
				previous = present;
			}
			bands[bandsOffset + numBands - 1] = stop - previous;
		}

		private static int checkNMaster(int nMaster, int bsXoverBand)
		{
			// Requirements (14496-3 sp04 p205)
			if (nMaster <= 0)
			{
				log.error(string.Format("Invalid n_master: {0:D}", nMaster));
				return -1;
			}
			if (bsXoverBand >= nMaster)
			{
				log.error(string.Format("Invalid bitstream, crossover band index beyond array bounds: {0:D}", bsXoverBand));
				return -1;
			}

			return 0;
		}

		private static int arrayMinInt(int[] array, int arrayOffset, int nel)
		{
			int min = array[arrayOffset];

			for (int i = 1; i < nel; i++)
			{
				min = System.Math.Min(array[arrayOffset + i], min);
			}

			return min;
		}

		private static bool inTableInt(int[] table, int lastEl, int needle)
		{
			for (int i = 0; i <= lastEl; i++)
			{
				if (table[i] == needle)
				{
					return true;
				}
			}

			return false;
		}

		/// Master Frequency Band Table (14496-3 sp04 p194)
		private static int sbrMakeFMaster(Context ac, SpectralBandReplication sbr, SpectrumParameters spectrum)
		{
			int temp, maxQmfSubbands;
			int startMin, stopMin;
			int[] sbrOffsetPtr;
			int[] stopDk = new int[13];

			if (sbr.sampleRate < 32000)
			{
				temp = 3000;
			}
			else if (sbr.sampleRate < 64000)
			{
				temp = 4000;
			}
			else
			{
				temp = 5000;
			}

			switch (sbr.sampleRate)
			{
				case 16000:
					sbrOffsetPtr = sbr_offset[0];
					break;
				case 22050:
					sbrOffsetPtr = sbr_offset[1];
					break;
				case 24000:
					sbrOffsetPtr = sbr_offset[2];
					break;
				case 32000:
					sbrOffsetPtr = sbr_offset[3];
					break;
				case 44100:
			case 48000:
		case 64000:
					sbrOffsetPtr = sbr_offset[4];
					break;
				case 88200:
			case 96000:
		case 128000:
	case 176400:
	case 192000:
					sbrOffsetPtr = sbr_offset[5];
					break;
				default:
					log.error(string.Format("Unsupported sample rate for SBR: {0:D}", sbr.sampleRate));
					return -1;
			}

			startMin = ((temp << 7) + (sbr.sampleRate >> 1)) / sbr.sampleRate;
			stopMin = ((temp << 8) + (sbr.sampleRate >> 1)) / sbr.sampleRate;

			sbr.k[0] = startMin + sbrOffsetPtr[spectrum.bsStartFreq];

			if (spectrum.bsStopFreq < 14)
			{
				sbr.k[2] = stopMin;
				makeBands(stopDk, 0, stopMin, 64, 13);
				Arrays.sort(stopDk);
				for (int k = 0; k < spectrum.bsStopFreq; k++)
				{
					sbr.k[2] += stopDk[k];
				}
			}
			else if (spectrum.bsStopFreq == 14)
			{
				sbr.k[2] = 2 * sbr.k[0];
			}
			else if (spectrum.bsStopFreq == 15)
			{
				sbr.k[2] = 3 * sbr.k[0];
			}
			else
			{
				log.error(string.Format("Invalid bsStopFreq: {0:D}", spectrum.bsStopFreq));
				return -1;
			}
			sbr.k[2] = System.Math.Min(64, sbr.k[2]);

			// Requirements (14496-3 sp04 p205)
			if (sbr.sampleRate <= 32000)
			{
				maxQmfSubbands = 48;
			}
			else if (sbr.sampleRate == 44100)
			{
				maxQmfSubbands = 35;
			}
			else if (sbr.sampleRate >= 48000)
			{
				maxQmfSubbands = 32;
			}
			else
			{
				log.error(string.Format("Unsupported sample rate {0:D}", sbr.sampleRate));
				return -1;
			}

			if (sbr.k[2] - sbr.k[0] > maxQmfSubbands)
			{
				log.error(string.Format("Invalid bitstream, too many QMF subbands: {0:D}", sbr.k[2] - sbr.k[0]));
				return -1;
			}

			if (spectrum.bsFreqScale == 0)
			{
				int dk, k2diff;

				dk = spectrum.bsAlterScale + 1;
				sbr.nMaster = ((sbr.k[2] - sbr.k[0] + (dk & 2)) >> dk) << 1;
				if (checkNMaster(sbr.nMaster, sbr.spectrumParams.bsXoverBand) != 0)
				{
					return -1;
				}

				for (int k = 1; k <= sbr.nMaster; k++)
				{
					sbr.fMaster[k] = dk;
				}

				k2diff = sbr.k[2] - sbr.k[0] - sbr.nMaster * dk;
				if (k2diff < 0)
				{
					sbr.fMaster[1]--;
					sbr.fMaster[2] -= (k2diff < -1 ? 1 : 0);
				}
				else if (k2diff != 0)
				{
					sbr.fMaster[sbr.nMaster]++;
				}

				sbr.fMaster[0] = sbr.k[0];
				for (int k = 1; k <= sbr.nMaster; k++)
				{
					sbr.fMaster[k] += sbr.fMaster[k - 1];
				}
			}
			else
			{
				int half_bands = 7 - spectrum.bsFreqScale; // bsFreqScale  = {1,2,3}
				int num_bands_0;
				bool two_regions;
				int vdk0_max, vdk1_min;
				int[] vk0 = new int[49];

				if (49 * sbr.k[2] > 110 * sbr.k[0])
				{
					two_regions = true;
					sbr.k[1] = 2 * sbr.k[0];
				}
				else
				{
					two_regions = false;
					sbr.k[1] = sbr.k[2];
				}

				num_bands_0 = lrintf(half_bands * log2f(sbr.k[1] / (float)sbr.k[0])) * 2;

				if (num_bands_0 <= 0)
				{ // Requirements (14496-3 sp04 p205)
					log.error(string.Format("Invalid num_bands_0: {0:D}", num_bands_0));
					return -1;
				}

				vk0[0] = 0;

				makeBands(vk0, 1, sbr.k[0], sbr.k[1], num_bands_0);

				Arrays.sort(vk0, 1, 1 + num_bands_0);
				vdk0_max = vk0[num_bands_0];

				vk0[0] = sbr.k[0];
				for (int k = 1; k <= num_bands_0; k++)
				{
					if (vk0[k] <= 0)
					{ // Requirements (14496-3 sp04 p205)
						log.error(string.Format("Invalid vDk0[{0:D}]: {1:D}", k, vk0[k]));
						return -1;
					}
					vk0[k] += vk0[k - 1];
				}

				if (two_regions)
				{
					int[] vk1 = new int[49];
					float invwarp = spectrum.bsAlterScale != 0 ? 0.76923076923076923077f : 1.0f; // bsAlterScale = {0,1}
					int num_bands_1 = lrintf(half_bands * invwarp * log2f(sbr.k[2] / (float)sbr.k[1])) * 2;

					makeBands(vk1, 1, sbr.k[1], sbr.k[2], num_bands_1);

					vdk1_min = arrayMinInt(vk1, 1, num_bands_1);

					if (vdk1_min < vdk0_max)
					{
						int change;
						Arrays.sort(vk1, 1, 1 + num_bands_1);
						change = System.Math.Min(vdk0_max - vk1[1], (vk1[num_bands_1] - vk1[1]) >> 1);
						vk1[1] += change;
						vk1[num_bands_1] -= change;
					}

					Arrays.sort(vk1, 1, 1 + num_bands_1);

					vk1[0] = sbr.k[1];
					for (int k = 1; k <= num_bands_1; k++)
					{
						if (vk1[k] <= 0)
						{ // Requirements (14496-3 sp04 p205)
							log.error(string.Format("Invalid vDk1[{0:D}]: {1:D}", k, vk1[k]));
							return -1;
						}
						vk1[k] += vk1[k - 1];
					}

					sbr.nMaster = num_bands_0 + num_bands_1;
					if (checkNMaster(sbr.nMaster, sbr.spectrumParams.bsXoverBand) != 0)
					{
						return -1;
					}

					Array.Copy(vk0, 0, sbr.fMaster, 0, num_bands_0 + 1);
					Array.Copy(vk1, 1, sbr.fMaster, num_bands_0 + 1, num_bands_1);
				}
				else
				{
					sbr.nMaster = num_bands_0;
					if (checkNMaster(sbr.nMaster, sbr.spectrumParams.bsXoverBand) != 0)
					{
						return -1;
					}
					Array.Copy(vk0, 0, sbr.fMaster, 0, num_bands_0 + 1);
				}
			}

			return 0;
		}

		internal static readonly float[] bands_warped = new float[] {1.32715174233856803909f, 1.18509277094158210129f, 1.11987160404675912501f};

		/// Limiter Frequency Band Table (14496-3 sp04 p198)
		private static void sbrMakeFTablelim(SpectralBandReplication sbr)
		{
			if (sbr.bsLimiterBands > 0)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float limBandsPerOctaveWarped = bands_warped[sbr.bsLimiterBands - 1];
				float limBandsPerOctaveWarped = bands_warped[sbr.bsLimiterBands - 1];
				int[] patchBorders = new int[7];
				int @in = 1;
				int @out = 0;

				patchBorders[0] = sbr.kx[1];
				for (int k = 1; k <= sbr.numPatches; k++)
				{
					patchBorders[k] = patchBorders[k - 1] + sbr.patchNumSubbands[k - 1];
				}

				Array.Copy(sbr.fTablelow, 0, sbr.fTablelim, 0, sbr.n[0] + 1);
				if (sbr.numPatches > 1)
				{
					Array.Copy(sbr.fTablelim, sbr.n[0] + 1, patchBorders, 1, sbr.numPatches - 1);
				}

				Arrays.sort(sbr.fTablelim, 0, sbr.numPatches + sbr.n[0]);

				sbr.nLim = sbr.n[0] + sbr.numPatches - 1;
				while (@out < sbr.nLim)
				{
					if (sbr.fTablelim[@in] >= sbr.fTablelim[@out] * limBandsPerOctaveWarped)
					{
						sbr.fTablelim[++@out] = sbr.fTablelim[@in++];
					}
					else if (sbr.fTablelim[@in] == sbr.fTablelim[@out] || !inTableInt(patchBorders, sbr.numPatches, sbr.fTablelim[@in]))
					{
						@in++;
						sbr.nLim--;
					}
					else if (!inTableInt(patchBorders, sbr.numPatches, sbr.fTablelim[@out]))
					{
						sbr.fTablelim[@out] = sbr.fTablelim[@in++];
						sbr.nLim--;
					}
					else
					{
						sbr.fTablelim[++@out] = sbr.fTablelim[@in++];
					}
				}
			}
			else
			{
				sbr.fTablelim[0] = sbr.fTablelow[0];
				sbr.fTablelim[1] = sbr.fTablelow[sbr.n[0]];
				sbr.nLim = 1;
			}
		}

		/// High Frequency Generation - Patch finalruction (14496-3 sp04 p216 fig. 4.46)
		private static int sbrHfCalcNpatches(Context ac, SpectralBandReplication sbr)
		{
			int msb = sbr.k[0];
			int usb = sbr.kx[1];
			int goal_sb = ((1000 << 11) + (sbr.sampleRate >> 1)) / sbr.sampleRate;

			sbr.numPatches = 0;

			int k;
			if (goal_sb < sbr.kx[1] + sbr.m[1])
			{
				for (k = 0; sbr.fMaster[k] < goal_sb; k++)
				{
				}
			}
			else
			{
				k = sbr.nMaster;
			}

			int sb = 0;
			do
			{
				int odd = 0;
				for (int i = k; i == k || sb > (sbr.k[0] - 1 + msb - odd); i--)
				{
					sb = sbr.fMaster[i];
					odd = (sb + sbr.k[0]) & 1;
				}

				// Requirements (14496-3 sp04 p205) sets the maximum number of patches to 5.
				// After this check the final number of patches can still be six which is
				// illegal however the Coding Technologies decoder check stream has a final
				// count of 6 patches
				if (sbr.numPatches > 5)
				{
					log.error(string.Format("Too many patches: {0:D}", sbr.numPatches));
					return -1;
				}

				sbr.patchNumSubbands[sbr.numPatches] = System.Math.Max(sb - usb, 0);
				sbr.patchStartSubband[sbr.numPatches] = sbr.k[0] - odd - sbr.patchNumSubbands[sbr.numPatches];

				if (sbr.patchNumSubbands[sbr.numPatches] > 0)
				{
					usb = sb;
					msb = sb;
					sbr.numPatches++;
				}
				else
				{
					msb = sbr.kx[1];
				}

				if (sbr.fMaster[k] - sb < 3)
				{
					k = sbr.nMaster;
				}
			} while (sb != sbr.kx[1] + sbr.m[1]);

			if (sbr.numPatches > 1 && sbr.patchNumSubbands[sbr.numPatches - 1] < 3)
			{
				sbr.numPatches--;
			}

			return 0;
		}

		/// Derived Frequency Band Tables (14496-3 sp04 p197)
		private static int sbrMakeFDerived(Context ac, SpectralBandReplication sbr)
		{
			sbr.n[1] = sbr.nMaster - sbr.spectrumParams.bsXoverBand;
			sbr.n[0] = (sbr.n[1] + 1) >> 1;

			Array.Copy(sbr.fMaster, sbr.spectrumParams.bsXoverBand, sbr.fTablehigh, 0, sbr.n[1] + 1);
			sbr.m[1] = sbr.fTablehigh[sbr.n[1]] - sbr.fTablehigh[0];
			sbr.kx[1] = sbr.fTablehigh[0];

			// Requirements (14496-3 sp04 p205)
			if (sbr.kx[1] + sbr.m[1] > 64)
			{
				log.error(string.Format("Stop frequency border too high: {0:D}", sbr.kx[1] + sbr.m[1]));
				return -1;
			}
			if (sbr.kx[1] > 32)
			{
				log.error(string.Format("Start frequency border too high: {0:D}", sbr.kx[1]));
				return -1;
			}

			sbr.fTablelow[0] = sbr.fTablehigh[0];
			int temp = sbr.n[1] & 1;
			for (int k = 1; k <= sbr.n[0]; k++)
			{
				sbr.fTablelow[k] = sbr.fTablehigh[2 * k - temp];
			}

			sbr.nQ = System.Math.Max(1, lrintf(sbr.spectrumParams.bsNoiseBands * log2f(sbr.k[2] / (float)sbr.kx[1]))); // 0 <= bs_noise_bands <= 3
			if (sbr.nQ > 5)
			{
				log.error(string.Format("Too many noise floor scale factors: {0:D}", sbr.nQ));
				return -1;
			}

			sbr.fTablenoise[0] = sbr.fTablelow[0];
			temp = 0;
			for (int k = 1; k <= sbr.nQ; k++)
			{
				temp += (sbr.n[0] - temp) / (sbr.nQ + 1 - k);
				sbr.fTablenoise[k] = sbr.fTablelow[temp];
			}

			if (sbrHfCalcNpatches(ac, sbr) < 0)
			{
				return -1;
			}

			sbrMakeFTablelim(sbr);

			sbr.data[0].fIndexnoise = 0;
			sbr.data[1].fIndexnoise = 0;

			return 0;
		}

		private static int readSbrHeader(SpectralBandReplication sbr, BitReader br)
		{
			int cnt = br.BitsRead;
			int oldBsLimiterBands = sbr.bsLimiterBands;
			SpectrumParameters oldSpectrumParams = new SpectrumParameters();

			sbr.start = true;

			// Save last spectrum parameters variables to compare to new ones
			oldSpectrumParams.copy(sbr.spectrumParams);

			sbr.bsAmpResHeader = br.readBool();
			sbr.spectrumParams.bsStartFreq = br.read(4);
			sbr.spectrumParams.bsStopFreq = br.read(4);
			sbr.spectrumParams.bsXoverBand = br.read(3);
											  br.skip(2); // bs_reserved

			bool bs_header_extra_1 = br.readBool();
			bool bs_header_extra_2 = br.readBool();

			if (bs_header_extra_1)
			{
				sbr.spectrumParams.bsFreqScale = br.read(2);
				sbr.spectrumParams.bsAlterScale = br.read1();
				sbr.spectrumParams.bsNoiseBands = br.read(2);
			}
			else
			{
				sbr.spectrumParams.bsFreqScale = 2;
				sbr.spectrumParams.bsAlterScale = 1;
				sbr.spectrumParams.bsNoiseBands = 2;
			}

			// Check if spectrum parameters changed
			if (!oldSpectrumParams.Equals(sbr.spectrumParams))
			{
				sbr.reset = true;
			}

			if (bs_header_extra_2)
			{
				sbr.bsLimiterBands = br.read(2);
				sbr.bsLimiterGains = br.read(2);
				sbr.bsInterpolFreq = br.readBool();
				sbr.bsSmoothingMode = br.readBool();
			}
			else
			{
				sbr.bsLimiterBands = 2;
				sbr.bsLimiterGains = 2;
				sbr.bsInterpolFreq = true;
				sbr.bsSmoothingMode = true;
			}

			if (sbr.bsLimiterBands != oldBsLimiterBands && !sbr.reset)
			{
				sbrMakeFTablelim(sbr);
			}

			return br.BitsRead - cnt;
		}

		private static void sbrReset(Context ac, SpectralBandReplication sbr)
		{
			int err = sbrMakeFMaster(ac, sbr, sbr.spectrumParams);
			if (err >= 0)
			{
				err = sbrMakeFDerived(ac, sbr);
			}
			if (err < 0)
			{
				log.error(string.Format("SBR reset failed. Switching SBR to pure upsampling mode"));
				sbrTurnoff(sbr);
			}
		}

		/// <summary>
		/// Decode Spectral Band Replication extension data; reference: table 4.55.
		/// </summary>
		/// <param name="crc"> flag indicating the presence of CRC checksum </param>
		/// <param name="cnt"> length of TYPE_FIL syntactic element in bytes
		/// </param>
		/// <returns>  Returns number of bytes consumed from the TYPE_FIL element. </returns>
		public static int decodeSbrExtension(Context ac, SpectralBandReplication sbr, bool crc, int cnt, int idAac)
		{
			int numSbrBits = 0;

			sbr.reset = false;

			if (sbr.sampleRate == 0)
			{
				sbr.sampleRate = 2 * ac.oc[1].m4ac.sampleRate;
			}
			if (ac.oc[1].m4ac.extSampleRate == 0)
			{
				ac.oc[1].m4ac.extSampleRate = 2 * ac.oc[1].m4ac.sampleRate;
			}

			if (crc)
			{
				ac.br.skip(10); // bs_sbr_crc_bits
				numSbrBits += 10;
			}

			// Save some state from the previous frame
			sbr.kx[0] = sbr.kx[1];
			sbr.m[0] = sbr.m[1];
			sbr.kxAndMPushed = true;

			numSbrBits++;
			if (ac.br.readBool())
			{ // bs_header_flag
				numSbrBits += readSbrHeader(sbr, ac.br);
			}

			if (sbr.reset)
			{
				sbrReset(ac, sbr);
			}

			if (sbr.start)
			{
				numSbrBits += readSbrData(ac, sbr, idAac);
			}

			int numSkipBits = (cnt * 8 - 4 - numSbrBits);
			ac.br.skip(numSkipBits);

			int numAlignBits = numSkipBits & 7;
			int bytesRead = (numSbrBits + numAlignBits + 4) >> 3;

			if (bytesRead > cnt)
			{
				log.error(string.Format("Expected to read {0:D} SBR bytes actually read {1:D}", cnt, bytesRead));
			}

			return cnt;
		}

		// Places SBR in pure upsampling mode
		private static void sbrTurnoff(SpectralBandReplication sbr)
		{
			sbr.start = false;
			// Init defaults used in pure upsampling mode
			sbr.kx[1] = 32; // Typo in spec, kx' inits to 32
			sbr.m[1] = 0;
			// Reset values for first SBR header
			sbr.data[0].eA[1] = -1;
			sbr.data[1].eA[1] = -1;
			sbr.spectrumParams.reset();
		}

		public static void ctxInit(SpectralBandReplication sbr)
		{
			if (sbr.mdct != null)
			{
				return;
			}

			sbr.kx[0] = sbr.kx[1];
			sbrTurnoff(sbr);
			sbr.data[0].synthesisFilterbankSamplesOffset = SBRData.SBR_SYNTHESIS_BUF_SIZE - (1280 - 128);
			sbr.data[1].synthesisFilterbankSamplesOffset = SBRData.SBR_SYNTHESIS_BUF_SIZE - (1280 - 128);
			/* SBR requires samples to be scaled to +/-32768.0 to work correctly.
			 * mdct scale factors are adjusted to scale up from +/-1.0 at analysis
			 * and scale back down at synthesis. */
			sbr.mdct.mdctInit(7, true, 1.9 / (64 * 32768.0));
			sbr.mdctAna.mdctInit(7, true, -2.0 * 32768.0);
		}

		public static void ctxClose(SpectralBandReplication sbr)
		{
		}

		/// Dequantization and stereo decoding (14496-3 sp04 p203)
		private static void sbrDequant(SpectralBandReplication sbr, int idAac)
		{
			int k, e;
			int ch;

			if (idAac == TYPE_CPE && sbr.bsCoupling)
			{
				float alpha = sbr.data[0].bsAmpRes ? 1.0f : 0.5f;
				float pan_offset = sbr.data[0].bsAmpRes ? 12.0f : 24.0f;
				for (e = 1; e <= sbr.data[0].bsNumEnv; e++)
				{
					for (k = 0; k < sbr.n[sbr.data[0].bsFreqRes[e]]; k++)
					{
						float temp1 = exp2f(sbr.data[0].envFacs[e][k] * alpha + 7.0f);
						float temp2 = exp2f((pan_offset - sbr.data[1].envFacs[e][k]) * alpha);
						float fac;
						if (temp1 > 1E20)
						{
							log.error(string.Format("envelope scalefactor overflow in dequant"));
							temp1 = 1;
						}
						fac = temp1 / (1.0f + temp2);
						sbr.data[0].envFacs[e][k] = fac;
						sbr.data[1].envFacs[e][k] = fac * temp2;
					}
				}
				for (e = 1; e <= sbr.data[0].bsNumNoise; e++)
				{
					for (k = 0; k < sbr.nQ; k++)
					{
						float temp1 = exp2f(NOISE_FLOOR_OFFSET - sbr.data[0].noiseFacs[e][k] + 1);
						float temp2 = exp2f(12 - sbr.data[1].noiseFacs[e][k]);
						float fac;
						if (temp1 > 1E20)
						{
							log.error(string.Format("envelope scalefactor overflow in dequant"));
							temp1 = 1;
						}
						fac = temp1 / (1.0f + temp2);
						sbr.data[0].noiseFacs[e][k] = fac;
						sbr.data[1].noiseFacs[e][k] = fac * temp2;
					}
				}
			}
			else
			{ // SCE or one non-coupled CPE
				for (ch = 0; ch < (idAac == TYPE_CPE ? 1 : 0) + 1; ch++)
				{
					float alpha = sbr.data[ch].bsAmpRes ? 1.0f : 0.5f;
					for (e = 1; e <= sbr.data[ch].bsNumEnv; e++)
					{
						for (k = 0; k < sbr.n[sbr.data[ch].bsFreqRes[e]]; k++)
						{
							sbr.data[ch].envFacs[e][k] = exp2f(alpha * sbr.data[ch].envFacs[e][k] + 6.0f);
							if (sbr.data[ch].envFacs[e][k] > 1E20)
							{
								log.error(string.Format("envelope scalefactor overflow in dequant"));
								sbr.data[ch].envFacs[e][k] = 1;
							}
						}
					}

					for (e = 1; e <= sbr.data[ch].bsNumNoise; e++)
					{
						for (k = 0; k < sbr.nQ; k++)
						{
							sbr.data[ch].noiseFacs[e][k] = exp2f(NOISE_FLOOR_OFFSET - sbr.data[ch].noiseFacs[e][k]);
						}
					}
				}
			}
		}

		/// <summary>
		/// Analysis QMF Bank (14496-3 sp04 p206)
		/// </summary>
		/// <param name="x">       pointer to the beginning of the first sample window </param>
		/// <param name="W">       array of complex-valued samples split into subbands </param>
		private static void sbrQmfAnalysis(FFT mdct, float[] @in, float[] x, float[] z, float[][][][] W, int bufIdx)
		{
			Array.Copy(x, 1024, x, 0, 320 - 32);
			Array.Copy(@in, 0, x, 288, 1024);

			int xOffset = 0;
			for (int i = 0; i < 32; i++)
			{ // numTimeSlots*RATE = 16*2 as 960 sample frames
										   // are not supported
				FloatDSP.vectorFmulReverse(z, 0, AacSbrData.sbr_qmf_window_ds, 0, x, xOffset, 320);
				SBRDSP.sum64x5(z, 0);
				SBRDSP.qmfPreShuffle(z, 0);
				mdct.imdctHalf(z, 0, z, 64);
				SBRDSP.qmfPostShuffle(W[bufIdx][i], z, 0);
				xOffset += 32;
			}
		}

		/// <summary>
		/// Synthesis QMF Bank (14496-3 sp04 p206) and Downsampled Synthesis QMF Bank
		/// (14496-3 sp04 p206)
		/// </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private static void sbrQmfSynthesis(pspsharp.media.codec.util.FFT mdct, float out[], float X[][][], float mdctBuf[], float v0[], int vOff[], final int div)
		private static void sbrQmfSynthesis(FFT mdct, float[] @out, float[][][] X, float[] mdctBuf, float[] v0, int[] vOff, int div)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float sbrQmfWindow[] = div != 0 ? sbr_qmf_window_ds : sbr_qmf_window_us;
			float[] sbrQmfWindow = div != 0 ? sbr_qmf_window_ds : sbr_qmf_window_us;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int step = 128 >> div;
			int step = 128 >> div;
			int v;
			int outOffset = 0;
			for (int i = 0; i < 32; i++)
			{
				if (vOff[0] < step)
				{
					int saved_samples = (1280 - 128) >> div;
					Array.Copy(v0, 0, v0, SBR_SYNTHESIS_BUF_SIZE - saved_samples, saved_samples);
					vOff[0] = SBR_SYNTHESIS_BUF_SIZE - saved_samples - step;
				}
				else
				{
					vOff[0] -= step;
				}
				v = vOff[0];
				if (div != 0)
				{
					for (int n = 0; n < 32; n++)
					{
						X[0][i][n] = -X[0][i][n];
						X[0][i][32 + n] = X[1][i][31 - n];
					}
					mdct.imdctHalf(mdctBuf, 0, X[0][i], 0);
					SBRDSP.qmfDeintNeg(v0, v, mdctBuf, 0);
				}
				else
				{
					SBRDSP.negOdd64(X[1][i], 0);
					mdct.imdctHalf(mdctBuf, 0, X[0][i], 0);
					mdct.imdctHalf(mdctBuf, 64, X[1][i], 0);
					SBRDSP.qmfDeintBfly(v0, v, mdctBuf, 64, mdctBuf, 0);
				}
				FloatDSP.vectorFmul(@out, outOffset, v0, v, sbrQmfWindow, (0), 64 >> div);
				FloatDSP.vectorFmulAdd(@out, outOffset, v0, v + (192 >> div), sbrQmfWindow, (64 >> div), @out, outOffset, 64 >> div);
				FloatDSP.vectorFmulAdd(@out, outOffset, v0, v + (256 >> div), sbrQmfWindow, (128 >> div), @out, outOffset, 64 >> div);
				FloatDSP.vectorFmulAdd(@out, outOffset, v0, v + (448 >> div), sbrQmfWindow, (192 >> div), @out, outOffset, 64 >> div);
				FloatDSP.vectorFmulAdd(@out, outOffset, v0, v + (512 >> div), sbrQmfWindow, (256 >> div), @out, outOffset, 64 >> div);
				FloatDSP.vectorFmulAdd(@out, outOffset, v0, v + (704 >> div), sbrQmfWindow, (320 >> div), @out, outOffset, 64 >> div);
				FloatDSP.vectorFmulAdd(@out, outOffset, v0, v + (768 >> div), sbrQmfWindow, (384 >> div), @out, outOffset, 64 >> div);
				FloatDSP.vectorFmulAdd(@out, outOffset, v0, v + (960 >> div), sbrQmfWindow, (448 >> div), @out, outOffset, 64 >> div);
				FloatDSP.vectorFmulAdd(@out, outOffset, v0, v + (1024 >> div), sbrQmfWindow, (512 >> div), @out, outOffset, 64 >> div);
				FloatDSP.vectorFmulAdd(@out, outOffset, v0, v + (1216 >> div), sbrQmfWindow, (576 >> div), @out, outOffset, 64 >> div);
				outOffset += 64 >> div;
			}
		}

		private static readonly float[] bw_tab = new float[] {0.0f, 0.75f, 0.9f, 0.98f};

		/// Chirp Factors (14496-3 sp04 p214)
		private static void sbrChirp(SpectralBandReplication sbr, SBRData chData)
		{
			int i;
			float newBw;

			for (i = 0; i < sbr.nQ; i++)
			{
				if (chData.bsInvfMode[0][i] + chData.bsInvfMode[1][i] == 1)
				{
					newBw = 0.6f;
				}
				else
				{
					newBw = bw_tab[chData.bsInvfMode[0][i]];
				}

				if (newBw < chData.bwArray[i])
				{
					newBw = 0.75f * newBw + 0.25f * chData.bwArray[i];
				}
				else
				{
					newBw = 0.90625f * newBw + 0.09375f * chData.bwArray[i];
				}
				chData.bwArray[i] = newBw < 0.015625f ? 0.0f : newBw;
			}
		}

		/// High Frequency Generator (14496-3 sp04 p215)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private static int sbrHfGen(Context ac, SpectralBandReplication sbr, float Xhigh[][][], final float Xlow[][][], final float alpha0[][], final float alpha1[][], final float bwArray[], final int tEnv[], int bsNumEnv)
		private static int sbrHfGen(Context ac, SpectralBandReplication sbr, float[][][] Xhigh, float[][][] Xlow, float[][] alpha0, float[][] alpha1, float[] bwArray, int[] tEnv, int bsNumEnv)
		{
			int g = 0;
			int k = sbr.kx[1];
			for (int j = 0; j < sbr.numPatches; j++)
			{
				for (int x = 0; x < sbr.patchNumSubbands[j]; x++, k++)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int p = sbr.patchStartSubband[j] + x;
					int p = sbr.patchStartSubband[j] + x;
					while (g <= sbr.nQ && k >= sbr.fTablenoise[g])
					{
						g++;
					}
					g--;

					if (g < 0)
					{
						log.error(string.Format("ERROR : no subband found for frequency {0:D}", k));
						return -1;
					}

					SBRDSP.hf_gen(Xhigh[k], ENVELOPE_ADJUSTMENT_OFFSET, Xlow[p], ENVELOPE_ADJUSTMENT_OFFSET, alpha0[p], alpha1[p], bwArray[g], 2 * tEnv[0], 2 * tEnv[bsNumEnv]);
				}
			}
			if (k < sbr.m[1] + sbr.kx[1])
			{
				Arrays.fill(Xhigh, k, sbr.m[1] + sbr.kx[1], 0f);
			}

			return 0;
		}

		/// <summary>
		/// High Frequency Adjustment (14496-3 sp04 p217) and Mapping
		/// (14496-3 sp04 p217)
		/// </summary>
		private static int sbrMapping(Context ac, SpectralBandReplication sbr, SBRData chData, int[] eA)
		{
			for (int i = 1; i < 8; i++)
			{
				Arrays.fill(chData.sIndexmapped[i], 0);
			}

			for (int e = 0; e < chData.bsNumEnv; e++)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int ilim = sbr.n[chData.bsFreqRes[e + 1]];
				int ilim = sbr.n[chData.bsFreqRes[e + 1]];
				int[] table = chData.bsFreqRes[e + 1] != 0 ? sbr.fTablehigh : sbr.fTablelow;

				if (sbr.kx[1] != table[0])
				{
					log.error(string.Format("kx != f_table{high,low}[0]. Derived frequency tables were not regenerated."));
					sbrTurnoff(sbr);
					return AAC_ERROR;
				}
				for (int i = 0; i < ilim; i++)
				{
					for (int m = table[i]; m < table[i + 1]; m++)
					{
						sbr.eOrigmapped[e][m - sbr.kx[1]] = chData.envFacs[e+1][i];
					}
				}

				// ch_data.bsNumNoise > 1 => 2 noise floors
				int k = (chData.bsNumNoise > 1) && (chData.tEnv[e] >= chData.tQ[1]) ? 1 : 0;
				for (int i = 0; i < sbr.nQ; i++)
				{
					for (int m = sbr.fTablenoise[i]; m < sbr.fTablenoise[i + 1]; m++)
					{
						sbr.qMapped[e][m - sbr.kx[1]] = chData.noiseFacs[k + 1][i];
					}
				}

				for (int i = 0; i < sbr.n[1]; i++)
				{
					if (chData.bsAddHarmonicFlag)
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int m_midpoint = (sbr.fTablehigh[i] + sbr.fTablehigh[i + 1]) >> 1;
						int m_midpoint = (sbr.fTablehigh[i] + sbr.fTablehigh[i + 1]) >> 1;

						chData.sIndexmapped[e + 1][m_midpoint - sbr.kx[1]] = chData.bsAddHarmonic[i] * (e >= eA[1] || (chData.sIndexmapped[0][m_midpoint - sbr.kx[1]] == 1) ? 1 : 0);
					}
				}

				for (int i = 0; i < ilim; i++)
				{
					int additional_sinusoid_present = 0;
					for (int m = table[i]; m < table[i + 1]; m++)
					{
						if (chData.sIndexmapped[e + 1][m - sbr.kx[1]] != 0)
						{
							additional_sinusoid_present = 1;
							break;
						}
					}
					Arrays.fill(sbr.sMapped[e], table[i] - sbr.kx[1], table[i + 1] - sbr.kx[1], additional_sinusoid_present);
				}
			}

			Array.Copy(chData.sIndexmapped[chData.bsNumEnv], 0, chData.sIndexmapped[0], 0, chData.sIndexmapped[0].Length);

			return 0;
		}

		/// Estimation of current envelope (14496-3 sp04 p218)
		private static void sbrEnvEstimate(float[][] eCurr, float[][][] Xhigh, SpectralBandReplication sbr, SBRData chData)
		{
			int kx1 = sbr.kx[1];

			if (sbr.bsInterpolFreq)
			{
				for (int e = 0; e < chData.bsNumEnv; e++)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float recipEnvSize = 0.5f / (chData.tEnv[e + 1] - chData.tEnv[e]);
					float recipEnvSize = 0.5f / (chData.tEnv[e + 1] - chData.tEnv[e]);
					int ilb = chData.tEnv[e] * 2 + ENVELOPE_ADJUSTMENT_OFFSET;
					int iub = chData.tEnv[e + 1] * 2 + ENVELOPE_ADJUSTMENT_OFFSET;

					for (int m = 0; m < sbr.m[1]; m++)
					{
						float sum = SBRDSP.sum_square(Xhigh[m + kx1], ilb, iub - ilb);
						eCurr[e][m] = sum * recipEnvSize;
					}
				}
			}
			else
			{
				for (int e = 0; e < chData.bsNumEnv; e++)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int envSize = 2 * (chData.tEnv[e + 1] - chData.tEnv[e]);
					int envSize = 2 * (chData.tEnv[e + 1] - chData.tEnv[e]);
					int ilb = chData.tEnv[e] * 2 + ENVELOPE_ADJUSTMENT_OFFSET;
					int iub = chData.tEnv[e + 1] * 2 + ENVELOPE_ADJUSTMENT_OFFSET;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int table[] = chData.bsFreqRes[e + 1] != 0 ? sbr.fTablehigh : sbr.fTablelow;
					int[] table = chData.bsFreqRes[e + 1] != 0 ? sbr.fTablehigh : sbr.fTablelow;

					for (int p = 0; p < sbr.n[chData.bsFreqRes[e + 1]]; p++)
					{
						float sum = 0.0f;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int den = envSize * (table[p + 1] - table[p]);
						int den = envSize * (table[p + 1] - table[p]);

						for (int k = table[p]; k < table[p + 1]; k++)
						{
							sum += SBRDSP.sum_square(Xhigh[k], ilb, iub - ilb);
						}
						sum /= den;
						for (int k = table[p]; k < table[p + 1]; k++)
						{
							eCurr[e][k - kx1] = sum;
						}
					}
				}
			}
		}

		// max gain limits : -3dB, 0dB, 3dB, inf dB (limiter off)
		private static readonly float[] limgain = new float[] {0.70795f, 1.0f, 1.41254f, 10000000000f};

		/// <summary>
		/// Calculation of levels of additional HF signal components (14496-3 sp04 p219)
		/// and Calculation of gain (14496-3 sp04 p219)
		/// </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private static void sbrGainCalc(SpectralBandReplication sbr, SBRData chData, final int eA[])
		private static void sbrGainCalc(SpectralBandReplication sbr, SBRData chData, int[] eA)
		{
			int e, k, m;

			for (e = 0; e < chData.bsNumEnv; e++)
			{
				int delta = !(e == eA[1]) || (e == eA[0]) ? 1 : 0;
				for (k = 0; k < sbr.nLim; k++)
				{
					float gain_boost, gain_max;
					float[] sum = new float[] {0.0f, 0.0f};
					for (m = sbr.fTablelim[k] - sbr.kx[1]; m < sbr.fTablelim[k + 1] - sbr.kx[1]; m++)
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float temp = sbr.eOrigmapped[e][m] / (1.0f + sbr.qMapped[e][m]);
						float temp = sbr.eOrigmapped[e][m] / (1.0f + sbr.qMapped[e][m]);
						sbr.qM[e][m] = sqrtf(temp * sbr.qMapped[e][m]);
						sbr.sM[e][m] = sqrtf(temp * chData.sIndexmapped[e + 1][m]);
						if (sbr.sMapped[e][m] == 0)
						{
							sbr.gain[e][m] = sqrtf(sbr.eOrigmapped[e][m] / ((1.0f + sbr.eCurr[e][m]) * (1.0f + sbr.qMapped[e][m] * delta)));
						}
						else
						{
							sbr.gain[e][m] = sqrtf(sbr.eOrigmapped[e][m] * sbr.qMapped[e][m] / ((1.0f + sbr.eCurr[e][m]) * (1.0f + sbr.qMapped[e][m])));
						}
					}
					for (m = sbr.fTablelim[k] - sbr.kx[1]; m < sbr.fTablelim[k + 1] - sbr.kx[1]; m++)
					{
						sum[0] += sbr.eOrigmapped[e][m];
						sum[1] += sbr.eCurr[e][m];
					}
					gain_max = limgain[sbr.bsLimiterGains] * sqrtf((FLT_EPSILON + sum[0]) / (FLT_EPSILON + sum[1]));
					gain_max = System.Math.Min(100000.0f, gain_max);
					for (m = sbr.fTablelim[k] - sbr.kx[1]; m < sbr.fTablelim[k + 1] - sbr.kx[1]; m++)
					{
						float qM_max = sbr.qM[e][m] * gain_max / sbr.gain[e][m];
						sbr.qM[e][m] = System.Math.Min(sbr.qM[e][m], qM_max);
						sbr.gain[e][m] = System.Math.Min(sbr.gain[e][m], gain_max);
					}
					sum[0] = sum[1] = 0.0f;
					for (m = sbr.fTablelim[k] - sbr.kx[1]; m < sbr.fTablelim[k + 1] - sbr.kx[1]; m++)
					{
						sum[0] += sbr.eOrigmapped[e][m];
						sum[1] += sbr.eCurr[e][m] * sbr.gain[e][m] * sbr.gain[e][m] + sbr.sM[e][m] * sbr.sM[e][m] + (delta != 0 && sbr.sM[e][m] == 0 ? 1 : 0) * sbr.qM[e][m] * sbr.qM[e][m];
					}
					gain_boost = sqrtf((FLT_EPSILON + sum[0]) / (FLT_EPSILON + sum[1]));
					gain_boost = System.Math.Min(1.584893192f, gain_boost);
					for (m = sbr.fTablelim[k] - sbr.kx[1]; m < sbr.fTablelim[k + 1] - sbr.kx[1]; m++)
					{
						sbr.gain[e][m] *= gain_boost;
						sbr.qM[e][m] *= gain_boost;
						sbr.sM[e][m] *= gain_boost;
					}
				}
			}
		}

		/// Generate the subband filtered lowband
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private static int sbrLfGen(SpectralBandReplication sbr, float Xlow[][][], final float W[][][][], int bufIdx)
		private static int sbrLfGen(SpectralBandReplication sbr, float[][][] Xlow, float[][][][] W, int bufIdx)
		{
			const int tHFGen = 8;
			const int iF = 32;
			Utilities.fill(Xlow, 0f);

			for (int k = 0; k < sbr.kx[1]; k++)
			{
				for (int i = tHFGen; i < iF + tHFGen; i++)
				{
					Xlow[k][i][0] = W[bufIdx][i - tHFGen][k][0];
					Xlow[k][i][1] = W[bufIdx][i - tHFGen][k][1];
				}
			}
			bufIdx = 1 - bufIdx;
			for (int k = 0; k < sbr.kx[0]; k++)
			{
				for (int i = 0; i < tHFGen; i++)
				{
					Xlow[k][i][0] = W[bufIdx][i + iF - tHFGen][k][0];
					Xlow[k][i][1] = W[bufIdx][i + iF - tHFGen][k][1];
				}
			}

			return 0;
		}

		/// <summary>
		/// High Frequency Generation (14496-3 sp04 p214+) and Inverse Filtering
		/// (14496-3 sp04 p214)
		/// Warning: This routine does not seem numerically stable.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private static void sbrHfInverseFilter(float alpha0[][], float alpha1[][], final float Xlow[][][], int k0)
		private static void sbrHfInverseFilter(float[][] alpha0, float[][] alpha1, float[][][] Xlow, int k0)
		{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: float[][][] phi = new float[3][2][2];
			float[][][] phi = RectangularArrays.ReturnRectangularFloatArray(3, 2, 2);
			for (int k = 0; k < k0; k++)
			{
				SBRDSP.autocorrelate(Xlow[k], phi);

				float dk = phi[2][1][0] * phi[1][0][0] - (phi[1][1][0] * phi[1][1][0] + phi[1][1][1] * phi[1][1][1]) / 1.000001f;

				if (dk == 0f)
				{
					alpha1[k][0] = 0f;
					alpha1[k][1] = 0f;
				}
				else
				{
					float tempReal, tempIm;
					tempReal = phi[0][0][0] * phi[1][1][0] - phi[0][0][1] * phi[1][1][1] - phi[0][1][0] * phi[1][0][0];
					tempIm = phi[0][0][0] * phi[1][1][1] + phi[0][0][1] * phi[1][1][0] - phi[0][1][1] * phi[1][0][0];

					alpha1[k][0] = tempReal / dk;
					alpha1[k][1] = tempIm / dk;
				}

				if (phi[1][0][0] == 0f)
				{
					alpha0[k][0] = 0f;
					alpha0[k][1] = 0f;
				}
				else
				{
					float tempReal, tempIm;
					tempReal = phi[0][0][0] + alpha1[k][0] * phi[1][1][0] + alpha1[k][1] * phi[1][1][1];
					tempIm = phi[0][0][1] + alpha1[k][1] * phi[1][1][0] - alpha1[k][0] * phi[1][1][1];

					alpha0[k][0] = -tempReal / phi[1][0][0];
					alpha0[k][1] = -tempIm / phi[1][0][0];
				}

				if (alpha1[k][0] * alpha1[k][0] + alpha1[k][1] * alpha1[k][1] >= 16.0f || alpha0[k][0] * alpha0[k][0] + alpha0[k][1] * alpha0[k][1] >= 16.0f)
				{
					alpha1[k][0] = 0f;
					alpha1[k][1] = 0f;
					alpha0[k][0] = 0f;
					alpha0[k][1] = 0f;
				}
			}
		}

		private static readonly float[] h_smooth = new float[] {0.33333333333333f, 0.30150283239582f, 0.21816949906249f, 0.11516383427084f, 0.03183050093751f};

		/// Assembling HF Signals (14496-3 sp04 p220)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private static void sbrHfAssemble(float Y1[][][], final float Xhigh[][][], SpectralBandReplication sbr, SBRData chData, final int eA[])
		private static void sbrHfAssemble(float[][][] Y1, float[][][] Xhigh, SpectralBandReplication sbr, SBRData chData, int[] eA)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int h_SL = 4 * (!sbr.bsSmoothingMode ? 1 : 0);
			int h_SL = 4 * (!sbr.bsSmoothingMode ? 1 : 0);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int kx = sbr.kx[1];
			int kx = sbr.kx[1];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int m_max = sbr.m[1];
			int m_max = sbr.m[1];
			float[][] gTemp = chData.gTemp; float[][] qTemp = chData.qTemp;
			int indexnoise = chData.fIndexnoise;
			int indexsine = chData.fIndexsine;

			if (sbr.reset)
			{
				for (int i = 0; i < h_SL; i++)
				{
					Array.Copy(sbr.gain[0], 0, gTemp[i + 2 * chData.tEnv[0]], 0, m_max);
					Array.Copy(sbr.qM[0], 0, qTemp[i + 2 * chData.tEnv[0]], 0, m_max);
				}
			}
			else if (h_SL != 0)
			{
				Array.Copy(gTemp[2 * chData.tEnvNumEnvOld], 0, gTemp[2 * chData.tEnv[0]], 0, 4);
				Array.Copy(qTemp[2 * chData.tEnvNumEnvOld], 0, qTemp[2 * chData.tEnv[0]], 0, 4);
			}

			for (int e = 0; e < chData.bsNumEnv; e++)
			{
				for (int i = 2 * chData.tEnv[e]; i < 2 * chData.tEnv[e + 1]; i++)
				{
					Array.Copy(sbr.gain[e], 0, gTemp[h_SL + i], 0, m_max);
					Array.Copy(sbr.qM[e], 0, qTemp[h_SL + i], 0, m_max);
				}
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float g_filt_tab[] = new float[48];
			float[] g_filt_tab = new float[48];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float q_filt_tab[] = new float[48];
			float[] q_filt_tab = new float[48];
			for (int e = 0; e < chData.bsNumEnv; e++)
			{
				for (int i = 2 * chData.tEnv[e]; i < 2 * chData.tEnv[e + 1]; i++)
				{
					float gFilt[], qFilt[];

					if (h_SL != 0 && e != eA[0] && e != eA[1])
					{
						gFilt = g_filt_tab;
						qFilt = q_filt_tab;
						for (int m = 0; m < m_max; m++)
						{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int idx1 = i + h_SL;
							int idx1 = i + h_SL;
							gFilt[m] = 0.0f;
							qFilt[m] = 0.0f;
							for (int j = 0; j <= h_SL; j++)
							{
								gFilt[m] += gTemp[idx1 - j][m] * h_smooth[j];
								qFilt[m] += qTemp[idx1 - j][m] * h_smooth[j];
							}
						}
					}
					else
					{
						gFilt = gTemp[i + h_SL];
						qFilt = qTemp[i];
					}

					SBRDSP.hfGFilt(Y1[i], kx, Xhigh, kx, gFilt, m_max, i + ENVELOPE_ADJUSTMENT_OFFSET);

					if (e != eA[0] && e != eA[1])
					{
						SBRDSP.hf_apply_noise(Y1[i], kx, sbr.sM[e], qFilt, indexnoise, kx, m_max, indexsine);
					}
					else
					{
						int idx = indexsine & 1;
						int A = (1 - ((indexsine + (kx & 1)) & 2));
						int B = (A ^ (-idx)) + idx;
						float[] @out = Y1[i][kx];
						float[] @in = sbr.sM[e];
						int m;
						for (m = 0; m + 1 < m_max; m += 2)
						{
							@out[idx + 2 * m] += @in[m] * A;
							@out[idx + 2 * m + 2] += @in[m + 1] * B;
						}
						if ((m_max & 1) != 0)
						{
							@out[idx + 2 * m] += @in[m] * A;
						}
					}
					indexnoise = (indexnoise + m_max) & 0x1ff;
					indexsine = (indexsine + 1) & 3;
				}
			}
			chData.fIndexnoise = indexnoise;
			chData.fIndexsine = indexsine;
		}

		/// Generate the subband filtered lowband
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private static int sbrXGen(SpectralBandReplication sbr, float X[][][], final float Y0[][][], final float Y1[][][], final float Xlow[][][], int ch)
		private static int sbrXGen(SpectralBandReplication sbr, float[][][] X, float[][][] Y0, float[][][] Y1, float[][][] Xlow, int ch)
		{
			int k, i;
			const int i_f = 32;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int i_Temp = Math.max(2*sbr.data[ch].tEnvNumEnvOld - i_f, 0);
			int i_Temp = System.Math.Max(2 * sbr.data[ch].tEnvNumEnvOld - i_f, 0);

			Utilities.fill(X, 0f);
			for (k = 0; k < sbr.kx[0]; k++)
			{
				for (i = 0; i < i_Temp; i++)
				{
					X[0][i][k] = Xlow[k][i + ENVELOPE_ADJUSTMENT_OFFSET][0];
					X[1][i][k] = Xlow[k][i + ENVELOPE_ADJUSTMENT_OFFSET][1];
				}
			}
			for (; k < sbr.kx[0] + sbr.m[0]; k++)
			{
				for (i = 0; i < i_Temp; i++)
				{
					X[0][i][k] = Y0[i + i_f][k][0];
					X[1][i][k] = Y0[i + i_f][k][1];
				}
			}

			for (k = 0; k < sbr.kx[1]; k++)
			{
				for (i = i_Temp; i < 38; i++)
				{
					X[0][i][k] = Xlow[k][i + ENVELOPE_ADJUSTMENT_OFFSET][0];
					X[1][i][k] = Xlow[k][i + ENVELOPE_ADJUSTMENT_OFFSET][1];
				}
			}
			for (; k < sbr.kx[1] + sbr.m[1]; k++)
			{
				for (i = i_Temp; i < i_f; i++)
				{
					X[0][i][k] = Y1[i][k][0];
					X[1][i][k] = Y1[i][k][1];
				}
			}

			return 0;
		}

		public static void sbrApply(Context ac, SpectralBandReplication sbr, int idAac, float[] L, float[] R)
		{
			int downsampled = ac.oc[1].m4ac.extSampleRate < sbr.sampleRate ? 1 : 0;
			int nch = (idAac == TYPE_CPE) ? 2 : 1;

			if (!sbr.kxAndMPushed)
			{
				sbr.kx[0] = sbr.kx[1];
				sbr.m[0] = sbr.m[1];
			}
			else
			{
				sbr.kxAndMPushed = false;
			}

			if (sbr.start)
			{
				sbrDequant(sbr, idAac);
			}

			for (int ch = 0; ch < nch; ch++)
			{
				/* decode channel */
				sbrQmfAnalysis(sbr.mdctAna, ch != 0 ? R : L, sbr.data[ch].analysisFilterbankSamples, sbr.qmfFilterScratch, sbr.data[ch].W, sbr.data[ch].Ypos);
				sbrLfGen(sbr, sbr.Xlow, sbr.data[ch].W, sbr.data[ch].Ypos);
				sbr.data[ch].Ypos ^= 1;
				if (sbr.start)
				{
					sbrHfInverseFilter(sbr.alpha0, sbr.alpha1, sbr.Xlow, sbr.k[0]);
					sbrChirp(sbr, sbr.data[ch]);
					sbrHfGen(ac, sbr, sbr.Xhigh, sbr.Xlow, sbr.alpha0, sbr.alpha1, sbr.data[ch].bwArray, sbr.data[ch].tEnv, sbr.data[ch].bsNumEnv);

					// hf_adj
					int err = sbrMapping(ac, sbr, sbr.data[ch], sbr.data[ch].eA);
					if (err == 0)
					{
						sbrEnvEstimate(sbr.eCurr, sbr.Xhigh, sbr, sbr.data[ch]);
						sbrGainCalc(sbr, sbr.data[ch], sbr.data[ch].eA);
						sbrHfAssemble(sbr.data[ch].Y[sbr.data[ch].Ypos], sbr.Xhigh, sbr, sbr.data[ch], sbr.data[ch].eA);
					}
				}

				/* synthesis */
				sbrXGen(sbr, sbr.X[ch], sbr.data[ch].Y[1 - sbr.data[ch].Ypos], sbr.data[ch].Y[sbr.data[ch].Ypos], sbr.Xlow, ch);
			}

			if (ac.oc[1].m4ac.ps == 1)
			{
				if (sbr.ps.start)
				{
					AacPs.psApply(sbr.ps, sbr.X[0], sbr.X[1], sbr.kx[1] + sbr.m[1]);
				}
				else
				{
					Utilities.copy(sbr.X[1], sbr.X[0]);
				}
				nch = 2;
			}

			int[] tmp = new int[1];
			tmp[0] = sbr.data[0].synthesisFilterbankSamplesOffset;
			sbrQmfSynthesis(sbr.mdct, L, sbr.X[0], sbr.qmfFilterScratch, sbr.data[0].synthesisFilterbankSamples, tmp, downsampled);
			sbr.data[0].synthesisFilterbankSamplesOffset = tmp[0];

			if (nch == 2)
			{
				tmp[0] = sbr.data[1].synthesisFilterbankSamplesOffset;
				sbrQmfSynthesis(sbr.mdct, R, sbr.X[1], sbr.qmfFilterScratch, sbr.data[1].synthesisFilterbankSamples, tmp, downsampled);
				sbr.data[1].synthesisFilterbankSamplesOffset = tmp[0];
			}
		}
	}

}