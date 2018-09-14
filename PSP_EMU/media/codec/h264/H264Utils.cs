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
namespace pspsharp.media.codec.h264
{
	public class H264Utils
	{
		private const int CLAMP_BASE = 512;
		// Array to clamp values in range [0..255]
		private static readonly int[] clamp = new int[CLAMP_BASE * 2 + 256];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: private static readonly int[][] redMap = new int[256][256];
		private static readonly int[][] redMap = RectangularArrays.ReturnRectangularIntArray(256, 256);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: private static readonly int[][] blueMap = new int[256][256];
		private static readonly int[][] blueMap = RectangularArrays.ReturnRectangularIntArray(256, 256);
		private static readonly int[] lumaYuvjToYuvTable = new int[256];

		static H264Utils()
		{
			initClamp();
			initRedMap(0xFF);
			initBlueMap();
			initYuvj();
		}

		/// <summary>
		/// Initialize array to clamp values in range [0..255]
		/// </summary>
		private static void initClamp()
		{
			for (int i = 0; i < 256; i++)
			{
				clamp[CLAMP_BASE + i] = i;
			}
			for (int i = 0; i < CLAMP_BASE; i++)
			{
				clamp[i] = 0;
				clamp[i + CLAMP_BASE + 256] = 255;
			}
		}

		/// <summary>
		/// The red color component is only depending on the
		/// luma and Cr components, not Cb.
		/// Pre-build a map for all possible combinations of
		/// luma and Cr values.
		/// </summary>
		/// <param name="alpha">  the value of the alpha component [0..255] </param>
		private static void initRedMap(int alpha)
		{
			alpha <<= 24;
			for (int luma = 0; luma <= 0xFF; luma++)
			{
				for (int cr = 0; cr <= 0xFF; cr++)
				{
					int c = luma - 16;
					int e = cr - 128;

					int red = (298 * c + 409 * e + 128) >> 8;
					red = clamp[red + CLAMP_BASE]; // clamp to [0..255]

					redMap[luma][cr] = alpha | red;
				}
			}
		}

		/// <summary>
		/// The blue color component is only depending on the
		/// luma and Cb components, not Cr.
		/// Pre-build a map for all possible combinations of
		/// luma and Cb values.
		/// </summary>
		private static void initBlueMap()
		{
			for (int luma = 0; luma <= 0xFF; luma++)
			{
				for (int cb = 0; cb <= 0xFF; cb++)
				{
					int c = luma - 16;
					int d = cb - 128;

					int blue = (298 * c + 516 * d + 128) >> 8;
					blue = clamp[blue + CLAMP_BASE]; // clamp to [0..255]

					blueMap[luma][cb] = blue << 16;
				}
			}
		}

		private static void initYuvj()
		{
			for (int i = 0; i < 256; i++)
			{
				lumaYuvjToYuvTable[i] = System.Math.Round(i / 255f * 224f + 16f);
			}
		}

		public static void YUV2ARGB(int width, int height, int[] luma, int[] cb, int[] cr, int[] argb)
		{
			// Convert YUV to ABGR
			YUV2ABGR(width, height, luma, cb, cr, argb);

			// Convert ABGR to ARGB (i.e. switch blue and red color components)
			for (int i = 0; i < argb.Length; i++)
			{
				int color = argb[i];
				color = (color & unchecked((int)0xFF00FF00)) | ((color & 0x00FF0000) >> 16) | ((color & 0x000000FF) << 16);
				argb[i] = color;
			}
		}

		public static void YUV2ABGR(int width, int height, int[] luma, int[] cb, int[] cr, int[] abgr)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int width2 = width >> 1;
			int width2 = width >> 1;

			int offset = 0;
			for (int y = 0; y < height; y++)
			{
				int offset2 = (y >> 1) * width2;
				for (int x = 0; x < width; x++, offset++)
				{
					int c = luma[offset] & 0xFF;
					int d = cb[offset2 + (x >> 1)] & 0xFF;
					int e = cr[offset2 + (x >> 1)] & 0xFF;

					// The red and blue color components have been already
					// pre-computed.
					int red = redMap[c][e];
					int blue = blueMap[c][d];

					// The green color components is depending on the
					// luma, Cr and Cb components. Pre-computing all the
					// possible combinations would result in a too high memory
					// usage: 256*256*256*4 bytes = 64Mb.
					// So compute the green color component here.
					c -= 16;
					d -= 128;
					e -= 128;

					int green = (298 * c - 100 * d - 208 * e + 128) >> 8;
					green = clamp[green + CLAMP_BASE]; // clamp to [0..255]

					abgr[offset] = blue | (green << 8) | red;
				}
			}
		}

		public static int Alpha
		{
			set
			{
				initRedMap(value & 0xFF);
			}
		}

		public static void YUVJ2YUV(int[] lumaYuvj, int[] lumaYuv, int size)
		{
			for (int i = 0; i < size; i++)
			{
				lumaYuv[i] = lumaYuvjToYuvTable[lumaYuvj[i]];
			}
		}

		public static int findExtradata(int[] input, int inputOffset, int inputLength)
		{
			int state = -1;
			bool hasSps = false;
			for (int i = 0; i <= inputLength; i++)
			{
				if ((state & 0xFFFFFF1F) == 0x107)
				{
					hasSps = true;
				}
				/*  if ((state&0xFFFFFF1F) == 0x101 ||
				 *     (state&0xFFFFFF1F) == 0x102 ||
				 *     (state&0xFFFFFF1F) == 0x105) {
				 *  }
				 */
				if ((state & 0xFFFFFF00) == 0x100 && (state & 0xFFFFFF1F) != 0x107 && (state & 0xFFFFFF1F) != 0x108 && (state & 0xFFFFFF1F) != 0x109)
				{
					if (hasSps)
					{
						while (i > 4 && input[inputOffset + i - 5] == 0)
						{
							i--;
						}
						return i - 4;
					}
				}
				if (i < inputLength)
				{
					state = (state << 8) | input[inputOffset + i];
				}
			}

			return 0;
		}
	}

}